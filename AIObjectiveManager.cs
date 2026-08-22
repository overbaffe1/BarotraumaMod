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
	// Token: 0x02000184 RID: 388
	internal class AIObjectiveManager
	{
		// Token: 0x17000BAD RID: 2989
		// (get) Token: 0x06002DCB RID: 11723 RVA: 0x001EC258 File Offset: 0x001EA458
		// (set) Token: 0x06002DCC RID: 11724 RVA: 0x001EC260 File Offset: 0x001EA460
		public List<AIObjective> Objectives { get; private set; } = new List<AIObjective>();

		// Token: 0x17000BAE RID: 2990
		// (get) Token: 0x06002DCD RID: 11725 RVA: 0x001EC269 File Offset: 0x001EA469
		public HumanAIController HumanAIController
		{
			get
			{
				return this.character.AIController as HumanAIController;
			}
		}

		// Token: 0x17000BAF RID: 2991
		// (get) Token: 0x06002DCE RID: 11726 RVA: 0x001EC27B File Offset: 0x001EA47B
		// (set) Token: 0x06002DCF RID: 11727 RVA: 0x001EC283 File Offset: 0x001EA483
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

		// Token: 0x17000BB0 RID: 2992
		// (get) Token: 0x06002DD0 RID: 11728 RVA: 0x001EC29B File Offset: 0x001EA49B
		public List<Order> CurrentOrders { get; } = new List<Order>();

		// Token: 0x17000BB1 RID: 2993
		// (get) Token: 0x06002DD1 RID: 11729 RVA: 0x001EC2A3 File Offset: 0x001EA4A3
		// (set) Token: 0x06002DD2 RID: 11730 RVA: 0x001EC2B5 File Offset: 0x001EA4B5
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

		// Token: 0x17000BB2 RID: 2994
		// (get) Token: 0x06002DD3 RID: 11731 RVA: 0x001EC2BE File Offset: 0x001EA4BE
		// (set) Token: 0x06002DD4 RID: 11732 RVA: 0x001EC2C6 File Offset: 0x001EA4C6
		public AIObjective ForcedOrder { get; private set; }

		// Token: 0x17000BB3 RID: 2995
		// (get) Token: 0x06002DD5 RID: 11733 RVA: 0x001EC2CF File Offset: 0x001EA4CF
		// (set) Token: 0x06002DD6 RID: 11734 RVA: 0x001EC2D7 File Offset: 0x001EA4D7
		public AIObjective CurrentObjective { get; private set; }

		// Token: 0x06002DD7 RID: 11735 RVA: 0x001EC2E0 File Offset: 0x001EA4E0
		public AIObjectiveManager(Character character)
		{
			this.character = character;
			this.CreateAutonomousObjectives();
		}

		// Token: 0x06002DD8 RID: 11736 RVA: 0x001EC316 File Offset: 0x001EA516
		public void AddObjective(AIObjective objective)
		{
			this.AddObjective<AIObjective>(objective);
		}

		// Token: 0x06002DD9 RID: 11737 RVA: 0x001EC320 File Offset: 0x001EA520
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

		// Token: 0x17000BB4 RID: 2996
		// (get) Token: 0x06002DDA RID: 11738 RVA: 0x001EC3CF File Offset: 0x001EA5CF
		// (set) Token: 0x06002DDB RID: 11739 RVA: 0x001EC3D7 File Offset: 0x001EA5D7
		public Dictionary<AIObjective, CoroutineHandle> DelayedObjectives { get; private set; } = new Dictionary<AIObjective, CoroutineHandle>();

		// Token: 0x17000BB5 RID: 2997
		// (get) Token: 0x06002DDC RID: 11740 RVA: 0x001EC3E0 File Offset: 0x001EA5E0
		// (set) Token: 0x06002DDD RID: 11741 RVA: 0x001EC3E8 File Offset: 0x001EA5E8
		public bool FailedAutonomousObjectives { get; private set; }

		// Token: 0x06002DDE RID: 11742 RVA: 0x001EC3F4 File Offset: 0x001EA5F4
		private void ClearIgnored()
		{
			HumanAIController humanAi = this.character.AIController as HumanAIController;
			if (humanAi != null)
			{
				humanAi.UnreachableHulls.Clear();
				humanAi.IgnoredItems.Clear();
			}
		}

		// Token: 0x06002DDF RID: 11743 RVA: 0x001EC42C File Offset: 0x001EA62C
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

		// Token: 0x06002DE0 RID: 11744 RVA: 0x001EC83C File Offset: 0x001EAA3C
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

		// Token: 0x06002DE1 RID: 11745 RVA: 0x001EC8D3 File Offset: 0x001EAAD3
		public T GetObjective<T>() where T : AIObjective
		{
			return this.Objectives.FirstOrDefault((AIObjective o) => o is T) as T;
		}

		// Token: 0x06002DE2 RID: 11746 RVA: 0x001EC90C File Offset: 0x001EAB0C
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

		// Token: 0x06002DE3 RID: 11747 RVA: 0x001EC9EC File Offset: 0x001EABEC
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

		// Token: 0x06002DE4 RID: 11748 RVA: 0x001ECA64 File Offset: 0x001EAC64
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

		// Token: 0x06002DE5 RID: 11749 RVA: 0x001ECB78 File Offset: 0x001EAD78
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

		// Token: 0x06002DE6 RID: 11750 RVA: 0x001ECC73 File Offset: 0x001EAE73
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

		// Token: 0x06002DE7 RID: 11751 RVA: 0x001ECCA9 File Offset: 0x001EAEA9
		public void SetForcedOrder(AIObjective objective)
		{
			this.ForcedOrder = objective;
		}

		// Token: 0x06002DE8 RID: 11752 RVA: 0x001ECCB2 File Offset: 0x001EAEB2
		public void ClearForcedOrder()
		{
			this.ForcedOrder = null;
			this.SortObjectives();
		}

		// Token: 0x06002DE9 RID: 11753 RVA: 0x001ECCC4 File Offset: 0x001EAEC4
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

		// Token: 0x06002DEA RID: 11754 RVA: 0x001ECF1C File Offset: 0x001EB11C
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

		// Token: 0x06002DEB RID: 11755 RVA: 0x001EDA10 File Offset: 0x001EBC10
		private void DismissSelf(Order order)
		{
			Order currentOrder = this.CurrentOrders.FirstOrDefault((Order oi) => oi.MatchesOrder(order.Identifier, order.Option));
			if (currentOrder == null)
			{
				return;
			}
			Order dismissOrder = currentOrder.GetDismissal();
			GameSession gameSession = GameMain.GameSession;
			CrewManager cm = (gameSession != null) ? gameSession.CrewManager : null;
			if (cm != null && cm.IsSinglePlayer)
			{
				this.character.SetOrder(dismissOrder, true, false, false);
			}
		}

		// Token: 0x06002DEC RID: 11756 RVA: 0x001EDA7C File Offset: 0x001EBC7C
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

		// Token: 0x06002DED RID: 11757 RVA: 0x001EDB24 File Offset: 0x001EBD24
		public bool IsCurrentOrder<T>() where T : AIObjective
		{
			return this.CurrentOrder is T;
		}

		// Token: 0x06002DEE RID: 11758 RVA: 0x001EDB34 File Offset: 0x001EBD34
		public bool IsCurrentObjective<T>() where T : AIObjective
		{
			return this.CurrentObjective is T;
		}

		// Token: 0x06002DEF RID: 11759 RVA: 0x001EDB44 File Offset: 0x001EBD44
		public bool HasObjectiveOrOrder<T>() where T : AIObjective
		{
			return this.Objectives.Any((AIObjective o) => o is T) || this.HasOrder<T>(null);
		}

		// Token: 0x06002DF0 RID: 11760 RVA: 0x001EDB7B File Offset: 0x001EBD7B
		public AIObjective GetActiveObjective()
		{
			AIObjective currentObjective = this.CurrentObjective;
			if (currentObjective == null)
			{
				return null;
			}
			return currentObjective.GetActiveObjective();
		}

		// Token: 0x06002DF1 RID: 11761 RVA: 0x001EDB90 File Offset: 0x001EBD90
		public T GetOrder<T>() where T : AIObjective
		{
			Order order = this.CurrentOrders.FirstOrDefault((Order o) => o.Objective is T);
			return ((order != null) ? order.Objective : null) as T;
		}

		// Token: 0x06002DF2 RID: 11762 RVA: 0x001EDBE0 File Offset: 0x001EBDE0
		public Order GetOrder(AIObjective objective)
		{
			return this.CurrentOrders.FirstOrDefault((Order o) => o.Objective == objective);
		}

		// Token: 0x06002DF3 RID: 11763 RVA: 0x001EDC14 File Offset: 0x001EBE14
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

		// Token: 0x06002DF4 RID: 11764 RVA: 0x001EDC64 File Offset: 0x001EBE64
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

		// Token: 0x06002DF5 RID: 11765 RVA: 0x001EDCB2 File Offset: 0x001EBEB2
		public IEnumerable<T> GetActiveObjectives<T>() where T : AIObjective
		{
			if (this.CurrentObjective == null)
			{
				return Enumerable.Empty<T>();
			}
			return this.CurrentObjective.GetSubObjectivesRecursive(true).OfType<T>();
		}

		// Token: 0x06002DF6 RID: 11766 RVA: 0x001EDCD4 File Offset: 0x001EBED4
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

		// Token: 0x06002DF7 RID: 11767 RVA: 0x001EDD2C File Offset: 0x001EBF2C
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

		// Token: 0x06002DF8 RID: 11768 RVA: 0x001EDD94 File Offset: 0x001EBF94
		public bool HasOrders()
		{
			return this.ForcedOrder != null || this.CurrentOrders.Any<Order>();
		}

		// Token: 0x06002DF9 RID: 11769 RVA: 0x001EDDAC File Offset: 0x001EBFAC
		public bool HasOrder<T>(Func<T, bool> predicate = null) where T : AIObjective
		{
			T forcedOrder = this.ForcedOrder as T;
			return (forcedOrder != null && (predicate == null || predicate(forcedOrder))) || this.CurrentOrders.Any(delegate(Order o)
			{
				T order = o.Objective as T;
				return order != null && (predicate == null || predicate(order));
			});
		}

		// Token: 0x06002DFA RID: 11770 RVA: 0x001EDE10 File Offset: 0x001EC010
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

		// Token: 0x06002DFB RID: 11771 RVA: 0x001EDED1 File Offset: 0x001EC0D1
		public Order GetCurrentOrderInfo()
		{
			if (this.currentOrder == null)
			{
				return null;
			}
			return this.CurrentOrders.FirstOrDefault((Order o) => o.Objective == this.CurrentOrder);
		}

		// Token: 0x06002DFC RID: 11772 RVA: 0x001EDEF4 File Offset: 0x001EC0F4
		[CompilerGenerated]
		internal static void <UpdateObjectives>g__UpdateOrderObjective|51_0(AIObjective orderObjective, ref AIObjectiveManager.<>c__DisplayClass51_0 A_1)
		{
			if (orderObjective == null)
			{
				return;
			}
			orderObjective.Update(A_1.deltaTime);
		}

		// Token: 0x040017F4 RID: 6132
		public const float MaxObjectivePriority = 100f;

		// Token: 0x040017F5 RID: 6133
		public const float EmergencyObjectivePriority = 90f;

		// Token: 0x040017F6 RID: 6134
		public const float HighestOrderPriority = 70f;

		// Token: 0x040017F7 RID: 6135
		public const float LowestOrderPriority = 60f;

		// Token: 0x040017F8 RID: 6136
		public const float RunPriority = 50f;

		// Token: 0x040017F9 RID: 6137
		public const float baseDevotion = 5f;

		// Token: 0x040017FB RID: 6139
		private readonly Character character;

		// Token: 0x040017FC RID: 6140
		private float _waitTimer;

		// Token: 0x040017FE RID: 6142
		private AIObjective currentOrder;

		// Token: 0x04001803 RID: 6147
		public bool FailedToFindDivingGearForDepth;

		// Token: 0x02000E3E RID: 3646
		public enum ObjectiveType
		{
			// Token: 0x040051C7 RID: 20935
			None,
			// Token: 0x040051C8 RID: 20936
			Order,
			// Token: 0x040051C9 RID: 20937
			Objective,
			// Token: 0x040051CA RID: 20938
			MinValue = 0,
			// Token: 0x040051CB RID: 20939
			MaxValue = 2
		}
	}
}
