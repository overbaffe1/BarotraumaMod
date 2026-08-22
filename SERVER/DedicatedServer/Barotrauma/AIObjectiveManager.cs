using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200007E RID: 126
	internal class AIObjectiveManager
	{
		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x060010C3 RID: 4291 RVA: 0x000993C4 File Offset: 0x000975C4
		// (set) Token: 0x060010C4 RID: 4292 RVA: 0x000993CC File Offset: 0x000975CC
		public List<AIObjective> Objectives { get; private set; } = new List<AIObjective>();

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x060010C5 RID: 4293 RVA: 0x000993D5 File Offset: 0x000975D5
		public HumanAIController HumanAIController
		{
			get
			{
				return this.character.AIController as HumanAIController;
			}
		}

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x060010C6 RID: 4294 RVA: 0x000993E7 File Offset: 0x000975E7
		// (set) Token: 0x060010C7 RID: 4295 RVA: 0x000993EF File Offset: 0x000975EF
		public float WaitTimer
		{
			get
			{
				return this._waitTimer;
			}
			set
			{
				this._waitTimer = (this.IsAllowedToWait() ? value : 0f);
			}
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x060010C8 RID: 4296 RVA: 0x00099407 File Offset: 0x00097607
		public List<Order> CurrentOrders { get; } = new List<Order>();

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x060010C9 RID: 4297 RVA: 0x0009940F File Offset: 0x0009760F
		// (set) Token: 0x060010CA RID: 4298 RVA: 0x00099421 File Offset: 0x00097621
		public AIObjective CurrentOrder
		{
			get
			{
				return this.ForcedOrder ?? this.currentOrder;
			}
			private set
			{
				this.currentOrder = value;
			}
		}

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x060010CB RID: 4299 RVA: 0x0009942A File Offset: 0x0009762A
		// (set) Token: 0x060010CC RID: 4300 RVA: 0x00099432 File Offset: 0x00097632
		public AIObjective ForcedOrder { get; private set; }

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x060010CD RID: 4301 RVA: 0x0009943B File Offset: 0x0009763B
		// (set) Token: 0x060010CE RID: 4302 RVA: 0x00099443 File Offset: 0x00097643
		public AIObjective CurrentObjective { get; private set; }

		// Token: 0x060010CF RID: 4303 RVA: 0x0009944C File Offset: 0x0009764C
		public AIObjectiveManager(Character character)
		{
			this.character = character;
			this.CreateAutonomousObjectives();
		}

		// Token: 0x060010D0 RID: 4304 RVA: 0x00099482 File Offset: 0x00097682
		public void AddObjective(AIObjective objective)
		{
			this.AddObjective<AIObjective>(objective);
		}

		// Token: 0x060010D1 RID: 4305 RVA: 0x0009948C File Offset: 0x0009768C
		public void AddObjective<T>(T objective) where T : AIObjective
		{
			if (objective == null)
			{
				return;
			}
			Type type = objective.GetType();
			if (objective.AllowMultipleInstances)
			{
				T existingObjective = this.Objectives.FirstOrDefault((AIObjective o) => o.GetType() == type) as T;
				if (existingObjective != null && existingObjective.IsDuplicate<T>(objective))
				{
					this.Objectives.Remove(existingObjective);
				}
			}
			else
			{
				this.Objectives.RemoveAll((AIObjective o) => o.GetType() == type);
			}
			this.Objectives.Add(objective);
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x060010D2 RID: 4306 RVA: 0x0009953B File Offset: 0x0009773B
		// (set) Token: 0x060010D3 RID: 4307 RVA: 0x00099543 File Offset: 0x00097743
		public Dictionary<AIObjective, CoroutineHandle> DelayedObjectives { get; private set; } = new Dictionary<AIObjective, CoroutineHandle>();

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x060010D4 RID: 4308 RVA: 0x0009954C File Offset: 0x0009774C
		// (set) Token: 0x060010D5 RID: 4309 RVA: 0x00099554 File Offset: 0x00097754
		public bool FailedAutonomousObjectives { get; private set; }

		// Token: 0x060010D6 RID: 4310 RVA: 0x00099560 File Offset: 0x00097760
		private void ClearIgnored()
		{
			HumanAIController humanAi = this.character.AIController as HumanAIController;
			if (humanAi != null)
			{
				humanAi.UnreachableHulls.Clear();
				humanAi.IgnoredItems.Clear();
			}
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x00099598 File Offset: 0x00097798
		public void CreateAutonomousObjectives()
		{
			if (this.character.IsDead)
			{
				return;
			}
			foreach (KeyValuePair<AIObjective, CoroutineHandle> delayedObjective in this.DelayedObjectives)
			{
				CoroutineManager.StopCoroutines(delayedObjective.Value);
			}
			AIObjectiveIdle prevIdleObjective = this.GetObjective<AIObjectiveIdle>();
			this.DelayedObjectives.Clear();
			this.Objectives.Clear();
			this.FailedAutonomousObjectives = false;
			this.AddObjective<AIObjectiveFindSafety>(new AIObjectiveFindSafety(this.character, this, 1f));
			AIObjectiveIdle newIdleObjective = new AIObjectiveIdle(this.character, this, 1f);
			if (prevIdleObjective != null)
			{
				newIdleObjective.TargetHull = prevIdleObjective.TargetHull;
				newIdleObjective.Behavior = prevIdleObjective.Behavior;
				prevIdleObjective.PreferredOutpostModuleTypes.ForEach(delegate(Identifier t)
				{
					newIdleObjective.PreferredOutpostModuleTypes.Add(t);
				});
			}
			this.AddObjective<AIObjectiveIdle>(newIdleObjective);
			int objectiveCount = this.Objectives.Count;
			CharacterInfo info = this.character.Info;
			if (((info != null) ? info.Job : null) != null)
			{
				using (List<AutonomousObjective>.Enumerator enumerator2 = this.character.Info.Job.Prefab.AutonomousObjectives.GetEnumerator())
				{
					Func<Submarine, bool> <>9__1;
					while (enumerator2.MoveNext())
					{
						AutonomousObjective autonomousObjective = enumerator2.Current;
						OrderPrefab orderPrefab2 = OrderPrefab.Prefabs[autonomousObjective.Identifier];
						if (orderPrefab2 == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Could not find a matching prefab by the identifier: '");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(autonomousObjective.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("'");
							throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						OrderPrefab orderPrefab = orderPrefab2;
						Item item = null;
						if (orderPrefab.MustSetTarget)
						{
							List<Item> matchingItems = orderPrefab.GetMatchingItems(this.character.Submarine, false, new CharacterTeamType?(this.character.Info.TeamID), this.character, default(Identifier));
							item = ((matchingItems != null) ? matchingItems.GetRandomUnsynced<Item>() : null);
						}
						Order order = new Order(orderPrefab, autonomousObjective.Option, item ?? this.character.CurrentHull, orderPrefab.GetTargetItemComponent(item), this.character, false);
						if (order != null)
						{
							if ((order.IgnoreAtOutpost || autonomousObjective.IgnoreAtOutpost) && Level.IsLoadedFriendlyOutpost && this.character.TeamID != CharacterTeamType.FriendlyNPC && !this.character.IsFriendlyNPCTurnedHostile && Submarine.MainSub != null)
							{
								IEnumerable<Submarine> dockedTo = Submarine.MainSub.DockedTo;
								Func<Submarine, bool> predicate;
								if ((predicate = <>9__1) == null)
								{
									predicate = (<>9__1 = ((Submarine s) => s.TeamID != CharacterTeamType.FriendlyNPC && s.TeamID != this.character.TeamID));
								}
								if (dockedTo.None(predicate))
								{
									continue;
								}
							}
							if (!autonomousObjective.IgnoreAtNonOutpost || Level.IsLoadedFriendlyOutpost)
							{
								AIObjective objective = this.CreateObjective(order, autonomousObjective.PriorityModifier);
								if (objective != null && objective.CanBeCompleted)
								{
									this.AddObjective<AIObjective>(objective, Rand.Value(Rand.RandSync.Unsynced) / 2f, null);
									objectiveCount++;
								}
							}
						}
					}
					goto IL_3A9;
				}
			}
			string text;
			if (this.character.Info != null)
			{
				text = "The character " + this.character.DisplayName + " has been set to use human ai, but has no job. This may cause issues with the AI. Consider configuring some jobs for the character type.";
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(141, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("The character ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.character.DisplayName);
				defaultInterpolatedStringHandler2.AppendLiteral(" has been set to use human ai, but has no ");
				defaultInterpolatedStringHandler2.AppendFormatted("CharacterInfo");
				defaultInterpolatedStringHandler2.AppendLiteral(". This may cause issues with the AI. Consider adding ");
				defaultInterpolatedStringHandler2.AppendFormatted("HasCharacterInfo");
				defaultInterpolatedStringHandler2.AppendLiteral("=\"True\" to the character config.");
				text = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			string warningMsg = text;
			DebugConsole.AddWarning(warningMsg, this.character.Prefab.ContentPackage);
			IL_3A9:
			this._waitTimer = Math.Max(this._waitTimer, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced) * (float)objectiveCount);
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x000999A8 File Offset: 0x00097BA8
		public void AddObjective<T>(T objective, float delay, Action callback = null) where T : AIObjective
		{
			if (objective == null)
			{
				return;
			}
			CoroutineHandle coroutine;
			if (this.DelayedObjectives.TryGetValue(objective, out coroutine))
			{
				CoroutineManager.StopCoroutines(coroutine);
				this.DelayedObjectives.Remove(objective);
			}
			coroutine = CoroutineManager.Invoke(delegate
			{
				if (GameMain.GameSession == null || (Level.Loaded == null && !(GameMain.GameSession.GameMode is TestGameMode)))
				{
					return;
				}
				this.DelayedObjectives.Remove(objective);
				this.AddObjective<T>(objective);
				Action callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2();
			}, delay);
			this.DelayedObjectives.Add(objective, coroutine);
		}

		// Token: 0x060010D9 RID: 4313 RVA: 0x00099A3F File Offset: 0x00097C3F
		public T GetObjective<T>() where T : AIObjective
		{
			return this.Objectives.FirstOrDefault((AIObjective o) => o is T) as T;
		}

		// Token: 0x060010DA RID: 4314 RVA: 0x00099A78 File Offset: 0x00097C78
		private AIObjective GetCurrentObjective()
		{
			AIObjective previousObjective = this.CurrentObjective;
			AIObjective firstObjective = this.Objectives.FirstOrDefault<AIObjective>();
			bool currentObjectiveIsOrder = this.CurrentOrder != null && firstObjective != null && this.CurrentOrder.Priority > firstObjective.Priority;
			this.CurrentObjective = (currentObjectiveIsOrder ? this.CurrentOrder : firstObjective);
			if (previousObjective == this.CurrentObjective)
			{
				return this.CurrentObjective;
			}
			if (previousObjective != null)
			{
				previousObjective.OnDeselected();
			}
			if (this.CurrentObjective != null)
			{
				this.CurrentObjective.OnSelected();
				this.GetObjective<AIObjectiveIdle>().CalculatePriority(Math.Max(this.CurrentObjective.Priority - 10f, 0f));
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				GameMain.NetworkMember.CreateEntityEvent(this.character, new Character.ObjectiveManagerStateEventData(currentObjectiveIsOrder ? AIObjectiveManager.ObjectiveType.Order : AIObjectiveManager.ObjectiveType.Objective));
			}
			return this.CurrentObjective;
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x00099B58 File Offset: 0x00097D58
		public float GetCurrentPriority()
		{
			if (this.CurrentObjective == null)
			{
				return 0f;
			}
			float num;
			if (!this.CurrentObjective.SubObjectives.Any<AIObjective>())
			{
				num = 0f;
			}
			else
			{
				num = this.CurrentObjective.SubObjectives.Max((AIObjective so) => so.Priority);
			}
			float subObjectivePriority = num;
			return Math.Max(this.CurrentObjective.Priority, subObjectivePriority);
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00099BD0 File Offset: 0x00097DD0
		public void UpdateObjectives(float deltaTime)
		{
			AIObjectiveManager.<>c__DisplayClass51_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			AIObjectiveManager.<UpdateObjectives>g__UpdateOrderObjective|51_0(this.ForcedOrder, ref CS$<>8__locals1);
			if (this.CurrentOrders.Any<Order>())
			{
				foreach (Order order in this.CurrentOrders)
				{
					AIObjective orderObjective = order.Objective;
					AIObjectiveManager.<UpdateObjectives>g__UpdateOrderObjective|51_0(orderObjective, ref CS$<>8__locals1);
				}
			}
			if (this.WaitTimer > 0f)
			{
				this.WaitTimer -= CS$<>8__locals1.deltaTime;
				return;
			}
			for (int i = 0; i < this.Objectives.Count; i++)
			{
				AIObjective objective = this.Objectives[i];
				if (objective.IsCompleted)
				{
					this.Objectives.Remove(objective);
				}
				else if (!objective.CanBeCompleted)
				{
					this.Objectives.Remove(objective);
					this.FailedAutonomousObjectives = true;
				}
				else
				{
					objective.Update(CS$<>8__locals1.deltaTime);
				}
			}
			this.GetCurrentObjective();
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00099CE4 File Offset: 0x00097EE4
		public void SortObjectives()
		{
			AIObjective forcedOrder = this.ForcedOrder;
			if (forcedOrder != null)
			{
				forcedOrder.CalculatePriority();
			}
			AIObjective orderWithHighestPriority = null;
			float highestPriority = 0f;
			int i = this.CurrentOrders.Count - 1;
			while (i >= 0 && this.CurrentOrders.Count > i)
			{
				AIObjective orderObjective = this.CurrentOrders[i].Objective;
				if (orderObjective != null)
				{
					orderObjective.CalculatePriority();
					if (orderWithHighestPriority == null || orderObjective.Priority > highestPriority)
					{
						orderWithHighestPriority = orderObjective;
						highestPriority = orderObjective.Priority;
					}
				}
				i--;
			}
			this.CurrentOrder = orderWithHighestPriority;
			for (int j = this.Objectives.Count - 1; j >= 0; j--)
			{
				this.Objectives[j].CalculatePriority();
			}
			if (this.Objectives.Any<AIObjective>())
			{
				this.Objectives.Sort((AIObjective x, AIObjective y) => y.Priority.CompareTo(x.Priority));
			}
			AIObjective currentObjective = this.GetCurrentObjective();
			if (currentObjective == null)
			{
				return;
			}
			currentObjective.SortSubObjectives();
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x00099DDF File Offset: 0x00097FDF
		public void DoCurrentObjective(float deltaTime)
		{
			if (this.WaitTimer > 0f)
			{
				this.character.AIController.SteeringManager.Reset();
				return;
			}
			AIObjective currentObjective = this.CurrentObjective;
			if (currentObjective == null)
			{
				return;
			}
			currentObjective.TryComplete(deltaTime);
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x00099E15 File Offset: 0x00098015
		public void SetForcedOrder(AIObjective objective)
		{
			this.ForcedOrder = objective;
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x00099E1E File Offset: 0x0009801E
		public void ClearForcedOrder()
		{
			this.ForcedOrder = null;
			this.SortObjectives();
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x00099E30 File Offset: 0x00098030
		public void SetOrder(Order order, bool speak)
		{
			if (this.character.IsDead)
			{
				return;
			}
			this.ClearIgnored();
			if (order == null || order.IsDismissal)
			{
				if (order.Option != Identifier.Empty)
				{
					if (this.CurrentOrders.Any((Order o) => o.MatchesDismissedOrder(order.Option)))
					{
						Order dismissedOrderInfo = this.CurrentOrders.First((Order o) => o.MatchesDismissedOrder(order.Option));
						this.CurrentOrders.Remove(dismissedOrderInfo);
					}
				}
				else
				{
					this.CurrentOrders.Clear();
				}
			}
			int i = this.CurrentOrders.Count - 1;
			while (i >= 0 && this.CurrentOrders.Count > i)
			{
				Order currentOrder = this.CurrentOrders[i];
				if (currentOrder.Objective == null || currentOrder.MatchesOrder(order))
				{
					this.CurrentOrders.RemoveAt(i);
				}
				else
				{
					Order currentOrderInfo = this.character.GetCurrentOrder(currentOrder);
					if (currentOrderInfo != null)
					{
						int currentPriority = currentOrderInfo.ManualPriority;
						if (currentOrder.ManualPriority != currentPriority)
						{
							this.CurrentOrders[i] = currentOrder.WithManualPriority(currentPriority);
						}
					}
					else
					{
						this.CurrentOrders.RemoveAt(i);
					}
				}
				i--;
			}
			this.FailedToFindDivingGearForDepth = false;
			AIObjective newCurrentObjective = this.CreateObjective(order, 1f);
			if (newCurrentObjective != null)
			{
				newCurrentObjective.Abandoned += delegate()
				{
					this.DismissSelf(order);
				};
				this.CurrentOrders.Add(order.WithObjective(newCurrentObjective));
			}
			else if (!order.IsDismissal)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to create an objective for the order: ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(order.Name);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (!this.HasOrders())
			{
				this.CreateAutonomousObjectives();
				return;
			}
			if (newCurrentObjective != null && speak && this.character.IsOnPlayerTeam)
			{
				LocalizedString msg = newCurrentObjective.IsAllowed ? TextManager.Get("DialogAffirmative") : TextManager.Get("DialogNegative");
				this.character.Speak(msg.Value, null, 1f, default(Identifier), 0f);
			}
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x0009A088 File Offset: 0x00098288
		public unsafe AIObjective CreateObjective(Order order, float priorityModifier = 1f)
		{
			if (order == null || order.IsDismissal)
			{
				return null;
			}
			string text = order.Identifier.Value.ToLowerInvariant();
			AIObjective newObjective;
			if (text != null)
			{
				switch (text.Length)
				{
				case 4:
					if (!(text == "wait"))
					{
						goto IL_A1E;
					}
					newObjective = new AIObjectiveGoTo(order.TargetSpatialEntity ?? this.character, this.character, this, true, true, priorityModifier, 0f)
					{
						AllowGoingOutside = true,
						IsWaitOrder = true,
						DebugLogWhenFails = false,
						SpeakIfFails = false,
						CloseEnough = 100f
					};
					goto IL_ABF;
				case 5:
				{
					if (!(text == "steer"))
					{
						goto IL_A1E;
					}
					Order order2 = order;
					Item item = ((order2 != null) ? order2.TargetEntity : null) as Item;
					Steering steering = (item != null) ? item.GetComponent<Steering>() : null;
					if (steering != null)
					{
						Steering steering2 = steering;
						Submarine submarine = steering.Item.Submarine;
						steering2.PosToMaintain = ((submarine != null) ? new Vector2?(submarine.WorldPosition) : null);
					}
					if (order.TargetItemComponent == null)
					{
						return null;
					}
					if (!order.TargetItemComponent.Item.IsInteractable(this.character))
					{
						return null;
					}
					newObjective = new AIObjectiveOperateItem(order.TargetItemComponent, this.character, this, order.Option, false, null, order.UseController, order.ConnectedController, priorityModifier)
					{
						Repeat = true,
						Override = (order.OrderGiver != null && order.OrderGiver.IsCommanding)
					};
					goto IL_ABF;
				}
				case 6:
				{
					char c2 = text[2];
					if (c2 != 'l')
					{
						if (c2 != 's')
						{
							if (c2 != 't')
							{
								goto IL_A1E;
							}
							if (!(text == "return"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveReturn(this.character, order.OrderGiver, this, priorityModifier);
							newObjective.Completed += delegate()
							{
								this.DismissSelf(order);
							};
							goto IL_ABF;
						}
						else
						{
							if (!(text == "rescue"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveRescueAll(this.character, this, priorityModifier);
							goto IL_ABF;
						}
					}
					else
					{
						if (!(text == "follow"))
						{
							goto IL_A1E;
						}
						if (order.OrderGiver == null)
						{
							return null;
						}
						Func<AIObjectiveGoTo, bool> <>9__4;
						newObjective = new AIObjectiveGoTo(order.OrderGiver, this.character, this, true, true, priorityModifier, 0f)
						{
							CloseEnough = Rand.Range(80f, 100f, Rand.RandSync.Unsynced),
							CloseEnoughMultiplier = Math.Min(1f + (float)this.HumanAIController.CountBotsInTheCrew(delegate(HumanAIController c)
							{
								AIObjectiveManager objectiveManager = c.ObjectiveManager;
								Func<AIObjectiveGoTo, bool> predicate;
								if ((predicate = <>9__4) == null)
								{
									predicate = (<>9__4 = ((AIObjectiveGoTo o) => o.Target == order.OrderGiver));
								}
								return objectiveManager.HasOrder<AIObjectiveGoTo>(predicate);
							}) * Rand.Range(0.8f, 1f, Rand.RandSync.Unsynced), 4f),
							ExtraDistanceOutsideSub = 100f,
							ExtraDistanceWhileSwimming = 100f,
							AllowGoingOutside = true,
							IgnoreIfTargetDead = true,
							IsFollowOrder = true,
							Mimic = this.character.IsOnPlayerTeam,
							DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachPlace
						};
						goto IL_ABF;
					}
					break;
				}
				case 7:
				{
					if (!(text == "getitem"))
					{
						goto IL_A1E;
					}
					Character character = this.character;
					Item targetItem2;
					if ((targetItem2 = (order.TargetEntity as Item)) == null)
					{
						ItemComponent targetItemComponent = order.TargetItemComponent;
						targetItem2 = ((targetItemComponent != null) ? targetItemComponent.Item : null);
					}
					newObjective = new AIObjectiveGetItem(character, targetItem2, this, false, priorityModifier)
					{
						MustBeSpecificItem = true
					};
					goto IL_ABF;
				}
				case 8:
					if (!(text == "fixleaks"))
					{
						goto IL_A1E;
					}
					newObjective = new AIObjectiveFixLeaks(this.character, this, priorityModifier, order.TargetEntity as Hull);
					goto IL_ABF;
				case 9:
				{
					char c2 = text[0];
					if (c2 != 'l')
					{
						if (c2 != 'p')
						{
							goto IL_A1E;
						}
						if (!(text == "pumpwater"))
						{
							goto IL_A1E;
						}
						Pump targetPump = order.TargetItemComponent as Pump;
						if (targetPump == null)
						{
							newObjective = new AIObjectivePumpWater(this.character, this, order.Option, priorityModifier);
							goto IL_ABF;
						}
						if (!order.TargetItemComponent.Item.IsInteractable(this.character))
						{
							return null;
						}
						AIObjectiveOperateItem aiobjectiveOperateItem = new AIObjectiveOperateItem(targetPump, this.character, this, order.Option, false, null, false, null, priorityModifier);
						Character orderGiver = order.OrderGiver;
						aiobjectiveOperateItem.Override = (orderGiver != null && orderGiver.IsCommanding);
						newObjective = aiobjectiveOperateItem;
						newObjective.Completed += delegate()
						{
							this.DismissSelf(order);
						};
						goto IL_ABF;
					}
					else
					{
						if (!(text == "loaditems"))
						{
							goto IL_A1E;
						}
						newObjective = new AIObjectiveLoadItems(this.character, this, order.Option, order.GetTargetItems(order.Option), order.TargetEntity as Item, priorityModifier);
						goto IL_ABF;
					}
					break;
				}
				case 10:
				{
					if (!(text == "findweapon"))
					{
						goto IL_A1E;
					}
					Item tItem = order.TargetEntity as Item;
					AIObjectivePrepare prepareObjective;
					if (tItem != null)
					{
						prepareObjective = new AIObjectivePrepare(this.character, this, tItem, 1f);
					}
					else
					{
						prepareObjective = new AIObjectivePrepare(this.character, this, order.GetTargetItems(order.Option), *order.RequireItems, 1f)
						{
							CheckInventory = false,
							EvaluateCombatPriority = true,
							FindAllItems = false,
							RequireNonEmpty = true
						};
					}
					prepareObjective.KeepActiveWhenReady = false;
					prepareObjective.Equip = true;
					newObjective = prepareObjective;
					newObjective.Completed += delegate()
					{
						this.DismissSelf(order);
					};
					goto IL_ABF;
				}
				case 11:
					if (!(text == "findthieves"))
					{
						goto IL_A1E;
					}
					newObjective = new AIObjectiveFindThieves(this.character, this, priorityModifier);
					goto IL_ABF;
				case 12:
				{
					char c2 = text[0];
					if (c2 != 'a')
					{
						if (c2 != 'c')
						{
							if (c2 != 's')
							{
								goto IL_A1E;
							}
							if (!(text == "setchargepct"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveOperateItem(order.TargetItemComponent, this.character, this, order.Option, false, null, false, null, priorityModifier)
							{
								Override = !this.character.IsDismissed,
								completionCondition = delegate()
								{
									float pct;
									if (float.TryParse(order.Option.Value, out pct))
									{
										float targetRatio = Math.Clamp(pct, 0f, 1f);
										float currentRatio = (order.TargetItemComponent as PowerContainer).RechargeRatio;
										return Math.Abs(targetRatio - currentRatio) < 0.05f;
									}
									return true;
								}
							};
							goto IL_ABF;
						}
						else
						{
							if (!(text == "cleanupitems"))
							{
								goto IL_A1E;
							}
							Item targetItem = order.TargetEntity as Item;
							if (targetItem == null)
							{
								newObjective = new AIObjectiveCleanupItems(this.character, this, null, priorityModifier);
								goto IL_ABF;
							}
							if (targetItem.HasTag(Tags.AllowCleanup) && targetItem.ParentInventory == null && targetItem.OwnInventory != null)
							{
								newObjective = new AIObjectiveCleanupItems(this.character, this, targetItem.OwnInventory.AllItems, priorityModifier);
								goto IL_ABF;
							}
							newObjective = new AIObjectiveCleanupItems(this.character, this, targetItem, priorityModifier);
							goto IL_ABF;
						}
					}
					else
					{
						if (!(text == "assaultenemy"))
						{
							goto IL_A1E;
						}
						newObjective = new AIObjectiveFightIntruders(this.character, this, priorityModifier)
						{
							TargetCharactersInOtherSubs = true
						};
						goto IL_ABF;
					}
					break;
				}
				case 13:
				{
					char c2 = text[0];
					if (c2 != 'i')
					{
						if (c2 != 'r')
						{
							goto IL_A1E;
						}
						if (!(text == "repairsystems"))
						{
							goto IL_A1E;
						}
					}
					else
					{
						if (!(text == "inspectnoises"))
						{
							goto IL_A1E;
						}
						newObjective = new AIObjectiveInspectNoises(this.character, this, priorityModifier);
						goto IL_ABF;
					}
					break;
				}
				case 14:
					if (!(text == "fightintruders"))
					{
						goto IL_A1E;
					}
					newObjective = new AIObjectiveFightIntruders(this.character, this, priorityModifier);
					goto IL_ABF;
				case 15:
				{
					char c2 = text[1];
					if (c2 != 'h')
					{
						if (c2 != 's')
						{
							if (c2 != 'x')
							{
								goto IL_A1E;
							}
							if (!(text == "extinguishfires"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveExtinguishFires(this.character, this, priorityModifier);
							goto IL_ABF;
						}
						else
						{
							if (!(text == "escapehandcuffs"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveEscapeHandcuffs(this.character, this, true, false, priorityModifier);
							goto IL_ABF;
						}
					}
					else
					{
						if (!(text == "chargebatteries"))
						{
							goto IL_A1E;
						}
						newObjective = new AIObjectiveChargeBatteries(this.character, this, order.Option, priorityModifier);
						goto IL_ABF;
					}
					break;
				}
				case 16:
				{
					char c2 = text[6];
					if (c2 != 'e')
					{
						if (c2 != 'm')
						{
							if (c2 != 't')
							{
								goto IL_A1E;
							}
							if (!(text == "deconstructitems"))
							{
								goto IL_A1E;
							}
							newObjective = new AIObjectiveDeconstructItems(this.character, this, priorityModifier);
							goto IL_ABF;
						}
						else if (!(text == "repairmechanical"))
						{
							goto IL_A1E;
						}
					}
					else if (!(text == "repairelectrical"))
					{
						goto IL_A1E;
					}
					break;
				}
				case 17:
				case 18:
				case 19:
					goto IL_A1E;
				case 20:
					if (!(text == "prepareforexpedition"))
					{
						goto IL_A1E;
					}
					newObjective = new AIObjectivePrepare(this.character, this, order.GetTargetItems(order.Option), *order.RequireItems, 1f)
					{
						KeepActiveWhenReady = true,
						CheckInventory = true,
						Equip = false,
						FindAllItems = true,
						RequireNonEmpty = false
					};
					goto IL_ABF;
				default:
					goto IL_A1E;
				}
				newObjective = new AIObjectiveRepairItems(this.character, this, priorityModifier, order.TargetEntity as Item)
				{
					RelevantSkill = order.AppropriateSkill
				};
				goto IL_ABF;
			}
			IL_A1E:
			if (order.TargetItemComponent == null)
			{
				return null;
			}
			if (!order.TargetItemComponent.Item.IsInteractable(this.character))
			{
				return null;
			}
			newObjective = new AIObjectiveOperateItem(order.TargetItemComponent, this.character, this, order.Option, false, null, order.UseController, order.ConnectedController, priorityModifier)
			{
				Repeat = true,
				Override = (order.OrderGiver != null && order.OrderGiver.IsCommanding)
			};
			if (newObjective.Abandon)
			{
				return null;
			}
			IL_ABF:
			if (newObjective != null)
			{
				newObjective.Identifier = order.Identifier;
			}
			newObjective.IgnoreAtOutpost = order.IgnoreAtOutpost;
			return newObjective;
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x0009AB7C File Offset: 0x00098D7C
		private void DismissSelf(Order order)
		{
			Order currentOrder = this.CurrentOrders.FirstOrDefault((Order oi) => oi.MatchesOrder(order.Identifier, order.Option));
			if (currentOrder == null)
			{
				return;
			}
			Order dismissOrder = currentOrder.GetDismissal();
			GameServer server = GameMain.Server;
			if (server != null)
			{
				server.SendOrderChatMessage(new OrderChatMessage(dismissOrder, this.character, this.character, true));
			}
			this.SetOrder(dismissOrder, false);
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x0009ABE4 File Offset: 0x00098DE4
		private bool IsAllowedToWait()
		{
			if (!this.character.IsOnPlayerTeam)
			{
				return false;
			}
			if (this.HasOrders())
			{
				return false;
			}
			if (this.CurrentObjective is AIObjectiveCombat || this.CurrentObjective is AIObjectiveFindSafety)
			{
				return false;
			}
			if (this.character.AnimController.InWater)
			{
				return false;
			}
			if (this.character.IsClimbing)
			{
				return false;
			}
			HumanAIController humanAI = this.character.AIController as HumanAIController;
			return (humanAI == null || !humanAI.UnsafeHulls.Contains(this.character.CurrentHull)) && !AIObjectiveIdle.IsForbidden(this.character.CurrentHull);
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x0009AC8C File Offset: 0x00098E8C
		public bool IsCurrentOrder<T>() where T : AIObjective
		{
			return this.CurrentOrder is T;
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x0009AC9C File Offset: 0x00098E9C
		public bool IsCurrentObjective<T>() where T : AIObjective
		{
			return this.CurrentObjective is T;
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x0009ACAC File Offset: 0x00098EAC
		public bool HasObjectiveOrOrder<T>() where T : AIObjective
		{
			return this.Objectives.Any((AIObjective o) => o is T) || this.HasOrder<T>(null);
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x0009ACE3 File Offset: 0x00098EE3
		public AIObjective GetActiveObjective()
		{
			AIObjective currentObjective = this.CurrentObjective;
			if (currentObjective == null)
			{
				return null;
			}
			return currentObjective.GetActiveObjective();
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x0009ACF8 File Offset: 0x00098EF8
		public T GetOrder<T>() where T : AIObjective
		{
			Order order = this.CurrentOrders.FirstOrDefault((Order o) => o.Objective is T);
			return ((order != null) ? order.Objective : null) as T;
		}

		// Token: 0x060010EA RID: 4330 RVA: 0x0009AD48 File Offset: 0x00098F48
		public Order GetOrder(AIObjective objective)
		{
			return this.CurrentOrders.FirstOrDefault((Order o) => o.Objective == objective);
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x0009AD7C File Offset: 0x00098F7C
		public T GetLastActiveObjective<T>() where T : AIObjective
		{
			AIObjective currentObjective = this.CurrentObjective;
			object obj;
			if (currentObjective == null)
			{
				obj = null;
			}
			else
			{
				obj = currentObjective.GetSubObjectivesRecursive(true).LastOrDefault((AIObjective so) => so is T);
			}
			return obj as T;
		}

		// Token: 0x060010EC RID: 4332 RVA: 0x0009ADCC File Offset: 0x00098FCC
		public T GetFirstActiveObjective<T>() where T : AIObjective
		{
			AIObjective currentObjective = this.CurrentObjective;
			object obj;
			if (currentObjective == null)
			{
				obj = null;
			}
			else
			{
				obj = currentObjective.GetSubObjectivesRecursive(true).FirstOrDefault((AIObjective so) => so is T);
			}
			return obj as T;
		}

		// Token: 0x060010ED RID: 4333 RVA: 0x0009AE1A File Offset: 0x0009901A
		public IEnumerable<T> GetActiveObjectives<T>() where T : AIObjective
		{
			if (this.CurrentObjective == null)
			{
				return Enumerable.Empty<T>();
			}
			return this.CurrentObjective.GetSubObjectivesRecursive(true).OfType<T>();
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x0009AE3C File Offset: 0x0009903C
		public bool HasActiveObjective<T>() where T : AIObjective
		{
			if (this.CurrentObjective is T)
			{
				return true;
			}
			if (this.CurrentObjective != null)
			{
				return this.CurrentObjective.GetSubObjectivesRecursive(false).Any((AIObjective so) => so is T);
			}
			return false;
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x0009AE94 File Offset: 0x00099094
		public bool IsOrder(AIObjective objective)
		{
			if (objective == this.ForcedOrder)
			{
				return true;
			}
			foreach (Order order in this.CurrentOrders)
			{
				if (order.Objective == objective)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x0009AEFC File Offset: 0x000990FC
		public bool HasOrders()
		{
			return this.ForcedOrder != null || this.CurrentOrders.Any<Order>();
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x0009AF14 File Offset: 0x00099114
		public bool HasOrder<T>(Func<T, bool> predicate = null) where T : AIObjective
		{
			T forcedOrder = this.ForcedOrder as T;
			return (forcedOrder != null && (predicate == null || predicate(forcedOrder))) || this.CurrentOrders.Any(delegate(Order o)
			{
				T order = o.Objective as T;
				return order != null && (predicate == null || predicate(order));
			});
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x0009AF78 File Offset: 0x00099178
		public float GetOrderPriority(AIObjective objective)
		{
			if (objective == this.ForcedOrder)
			{
				return 70f;
			}
			Order currentOrder = this.CurrentOrders.FirstOrDefault((Order o) => o.Objective == objective);
			if (currentOrder.Objective == null)
			{
				return 70f;
			}
			if (currentOrder.ManualPriority <= 0)
			{
				return 0f;
			}
			if (objective.ForceHighestPriority)
			{
				return 70f;
			}
			if (objective.PrioritizeIfSubObjectivesActive && objective.SubObjectives.Any<AIObjective>())
			{
				return 70f;
			}
			return MathHelper.Lerp(60f, 69f, MathUtils.InverseLerp(1f, (float)CharacterInfo.HighestManualOrderPriority, (float)currentOrder.ManualPriority));
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x0009B039 File Offset: 0x00099239
		public Order GetCurrentOrderInfo()
		{
			if (this.currentOrder == null)
			{
				return null;
			}
			return this.CurrentOrders.FirstOrDefault((Order o) => o.Objective == this.CurrentOrder);
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x0009B05C File Offset: 0x0009925C
		[CompilerGenerated]
		internal static void <UpdateObjectives>g__UpdateOrderObjective|51_0(AIObjective orderObjective, ref AIObjectiveManager.<>c__DisplayClass51_0 A_1)
		{
			if (orderObjective == null)
			{
				return;
			}
			orderObjective.Update(A_1.deltaTime);
		}

		// Token: 0x040007FA RID: 2042
		public const float MaxObjectivePriority = 100f;

		// Token: 0x040007FB RID: 2043
		public const float EmergencyObjectivePriority = 90f;

		// Token: 0x040007FC RID: 2044
		public const float HighestOrderPriority = 70f;

		// Token: 0x040007FD RID: 2045
		public const float LowestOrderPriority = 60f;

		// Token: 0x040007FE RID: 2046
		public const float RunPriority = 50f;

		// Token: 0x040007FF RID: 2047
		public const float baseDevotion = 5f;

		// Token: 0x04000801 RID: 2049
		private readonly Character character;

		// Token: 0x04000802 RID: 2050
		private float _waitTimer;

		// Token: 0x04000804 RID: 2052
		private AIObjective currentOrder;

		// Token: 0x04000809 RID: 2057
		public bool FailedToFindDivingGearForDepth;

		// Token: 0x020007F6 RID: 2038
		public enum ObjectiveType
		{
			// Token: 0x04002E67 RID: 11879
			None,
			// Token: 0x04002E68 RID: 11880
			Order,
			// Token: 0x04002E69 RID: 11881
			Objective,
			// Token: 0x04002E6A RID: 11882
			MinValue = 0,
			// Token: 0x04002E6B RID: 11883
			MaxValue = 2
		}
	}
}
