using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001B7 RID: 439
	internal class TriggerAction : EventAction
	{
		// Token: 0x1700095E RID: 2398
		// (get) Token: 0x060020C2 RID: 8386 RVA: 0x000DD5B9 File Offset: 0x000DB7B9
		// (set) Token: 0x060020C3 RID: 8387 RVA: 0x000DD5C1 File Offset: 0x000DB7C1
		[Serialize("", IsPropertySaveable.Yes, "Tag of the first entity that will be used for trigger checks.", "", false)]
		public Identifier Target1Tag { get; set; }

		// Token: 0x1700095F RID: 2399
		// (get) Token: 0x060020C4 RID: 8388 RVA: 0x000DD5CA File Offset: 0x000DB7CA
		// (set) Token: 0x060020C5 RID: 8389 RVA: 0x000DD5D2 File Offset: 0x000DB7D2
		[Serialize("", IsPropertySaveable.Yes, "Tag of the second entity that will be used for trigger checks.", "", false)]
		public Identifier Target2Tag { get; set; }

		// Token: 0x17000960 RID: 2400
		// (get) Token: 0x060020C6 RID: 8390 RVA: 0x000DD5DB File Offset: 0x000DB7DB
		// (set) Token: 0x060020C7 RID: 8391 RVA: 0x000DD5E3 File Offset: 0x000DB7E3
		[Serialize("", IsPropertySaveable.Yes, "If set, the first target has to be within an outpost module of this type.", "", false)]
		public Identifier TargetModuleType { get; set; }

		// Token: 0x17000961 RID: 2401
		// (get) Token: 0x060020C8 RID: 8392 RVA: 0x000DD5EC File Offset: 0x000DB7EC
		// (set) Token: 0x060020C9 RID: 8393 RVA: 0x000DD5F4 File Offset: 0x000DB7F4
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the first entity when the trigger check succeeds.", "", false)]
		public Identifier ApplyToTarget1 { get; set; }

		// Token: 0x17000962 RID: 2402
		// (get) Token: 0x060020CA RID: 8394 RVA: 0x000DD5FD File Offset: 0x000DB7FD
		// (set) Token: 0x060020CB RID: 8395 RVA: 0x000DD605 File Offset: 0x000DB805
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the second entity when the trigger check succeeds.", "", false)]
		public Identifier ApplyToTarget2 { get; set; }

		// Token: 0x17000963 RID: 2403
		// (get) Token: 0x060020CC RID: 8396 RVA: 0x000DD60E File Offset: 0x000DB80E
		// (set) Token: 0x060020CD RID: 8397 RVA: 0x000DD616 File Offset: 0x000DB816
		[Serialize(TriggerAction.TriggerType.Inside, IsPropertySaveable.Yes, "Determines if the targets must be inside or outside of the radius.", "", false)]
		public TriggerAction.TriggerType Type { get; set; }

		// Token: 0x17000964 RID: 2404
		// (get) Token: 0x060020CE RID: 8398 RVA: 0x000DD61F File Offset: 0x000DB81F
		// (set) Token: 0x060020CF RID: 8399 RVA: 0x000DD627 File Offset: 0x000DB827
		[Serialize(0f, IsPropertySaveable.Yes, "Range to activate the trigger.", "", false)]
		public float Radius { get; set; }

		// Token: 0x17000965 RID: 2405
		// (get) Token: 0x060020D0 RID: 8400 RVA: 0x000DD630 File Offset: 0x000DB830
		// (set) Token: 0x060020D1 RID: 8401 RVA: 0x000DD638 File Offset: 0x000DB838
		[Serialize(true, IsPropertySaveable.Yes, "If true, characters who are being targeted by some enemy cannot trigger the action.", "", false)]
		public bool DisableInCombat { get; set; }

		// Token: 0x17000966 RID: 2406
		// (get) Token: 0x060020D2 RID: 8402 RVA: 0x000DD641 File Offset: 0x000DB841
		// (set) Token: 0x060020D3 RID: 8403 RVA: 0x000DD649 File Offset: 0x000DB849
		[Serialize(true, IsPropertySaveable.Yes, "If true, dead/unconscious characters cannot trigger the action.", "", false)]
		public bool DisableIfTargetIncapacitated { get; set; }

		// Token: 0x17000967 RID: 2407
		// (get) Token: 0x060020D4 RID: 8404 RVA: 0x000DD652 File Offset: 0x000DB852
		// (set) Token: 0x060020D5 RID: 8405 RVA: 0x000DD65A File Offset: 0x000DB85A
		[Serialize(false, IsPropertySaveable.Yes, "If true, one target must interact with the other to trigger the action.", "", false)]
		public bool WaitForInteraction { get; set; }

		// Token: 0x17000968 RID: 2408
		// (get) Token: 0x060020D6 RID: 8406 RVA: 0x000DD663 File Offset: 0x000DB863
		// (set) Token: 0x060020D7 RID: 8407 RVA: 0x000DD66B File Offset: 0x000DB86B
		[Serialize(false, IsPropertySaveable.Yes, "If true, the action can be triggered by interacting with any matching target (not just the 1st one).", "", false)]
		public bool AllowMultipleTargets { get; set; }

		// Token: 0x17000969 RID: 2409
		// (get) Token: 0x060020D8 RID: 8408 RVA: 0x000DD674 File Offset: 0x000DB874
		// (set) Token: 0x060020D9 RID: 8409 RVA: 0x000DD67C File Offset: 0x000DB87C
		[Serialize(false, IsPropertySaveable.Yes, "If true and using multiple targets, all targets must be inside/outside the radius.", "", false)]
		public bool CheckAllTargets { get; set; }

		// Token: 0x1700096A RID: 2410
		// (get) Token: 0x060020DA RID: 8410 RVA: 0x000DD685 File Offset: 0x000DB885
		// (set) Token: 0x060020DB RID: 8411 RVA: 0x000DD68D File Offset: 0x000DB88D
		[Serialize(false, IsPropertySaveable.Yes, "If true, interacting with the target will make the character select it.", "", false)]
		public bool SelectOnTrigger { get; set; }

		// Token: 0x060020DC RID: 8412 RVA: 0x000DD698 File Offset: 0x000DB898
		public TriggerAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (element.GetAttribute("IgnoreIncapacitatedCharacters") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in ");
				defaultInterpolatedStringHandler.AppendFormatted("TriggerAction");
				defaultInterpolatedStringHandler.AppendLiteral(", event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": ");
				defaultInterpolatedStringHandler.AppendFormatted("IgnoreIncapacitatedCharacters");
				defaultInterpolatedStringHandler.AppendLiteral(" is a property of ");
				defaultInterpolatedStringHandler.AppendFormatted("TagAction");
				defaultInterpolatedStringHandler.AppendLiteral(", did you mean ");
				defaultInterpolatedStringHandler.AppendFormatted("DisableIfTargetIncapacitated");
				defaultInterpolatedStringHandler.AppendLiteral("?");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
			}
		}

		// Token: 0x060020DD RID: 8413 RVA: 0x000DD779 File Offset: 0x000DB979
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060020DE RID: 8414 RVA: 0x000DD781 File Offset: 0x000DB981
		public override void Reset()
		{
			this.ResetTargetIcons();
			this.isRunning = false;
			this.isFinished = false;
		}

		// Token: 0x060020DF RID: 8415 RVA: 0x000DD798 File Offset: 0x000DB998
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isRunning = true;
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.Target1Tag);
			if (!targets.Any<Entity>())
			{
				return;
			}
			this.triggerers.Clear();
			using (IEnumerator<Entity> enumerator = targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TriggerAction.<>c__DisplayClass61_0 CS$<>8__locals1 = new TriggerAction.<>c__DisplayClass61_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.e1 = enumerator.Current;
					if (this.DisableInCombat && TriggerAction.IsInCombat(CS$<>8__locals1.e1))
					{
						if (this.CheckAllTargets)
						{
							return;
						}
					}
					else
					{
						if (this.DisableIfTargetIncapacitated)
						{
							Character character = CS$<>8__locals1.e1 as Character;
							if (character != null && (character.IsDead || character.IsIncapacitated))
							{
								if (this.CheckAllTargets)
								{
									return;
								}
								continue;
							}
						}
						if (!this.TargetModuleType.IsEmpty)
						{
							Hull hull;
							if (!this.CheckAllTargets && this.CheckDistanceToHull(CS$<>8__locals1.e1, out hull))
							{
								this.Trigger(CS$<>8__locals1.e1, hull);
								return;
							}
							if (this.CheckAllTargets)
							{
								if (!this.CheckDistanceToHull(CS$<>8__locals1.e1, out hull))
								{
									return;
								}
								this.triggerers.Add(new ValueTuple<Entity, Entity>(CS$<>8__locals1.e1, hull));
							}
						}
						else
						{
							CS$<>8__locals1.targets2 = this.ParentEvent.GetTargets(this.Target2Tag);
							using (IEnumerator<Entity> enumerator2 = CS$<>8__locals1.targets2.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									TriggerAction.<>c__DisplayClass61_1 CS$<>8__locals2 = new TriggerAction.<>c__DisplayClass61_1();
									CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
									CS$<>8__locals2.e2 = enumerator2.Current;
									if (CS$<>8__locals2.CS$<>8__locals1.e1 != CS$<>8__locals2.e2)
									{
										if (this.DisableInCombat && TriggerAction.IsInCombat(CS$<>8__locals2.e2))
										{
											if (this.CheckAllTargets)
											{
												return;
											}
										}
										else
										{
											if (this.DisableIfTargetIncapacitated)
											{
												Character character2 = CS$<>8__locals2.e2 as Character;
												if (character2 != null && (character2.IsDead || character2.IsIncapacitated))
												{
													if (this.CheckAllTargets)
													{
														return;
													}
													continue;
												}
											}
											if (this.WaitForInteraction)
											{
												Character player = null;
												Character npc = null;
												Item item = null;
												Character @char = CS$<>8__locals2.CS$<>8__locals1.e1 as Character;
												if (@char != null)
												{
													if (@char.IsPlayer)
													{
														player = @char;
													}
													else if (npc == null)
													{
														npc = @char;
													}
												}
												else if (item == null)
												{
													item = (CS$<>8__locals2.CS$<>8__locals1.e1 as Item);
												}
												Character char2 = CS$<>8__locals2.e2 as Character;
												if (char2 != null)
												{
													if (char2.IsPlayer)
													{
														player = char2;
													}
													else if (npc == null)
													{
														npc = char2;
													}
												}
												else if (item == null)
												{
													item = (CS$<>8__locals2.e2 as Item);
												}
												if (player != null)
												{
													if (npc != null)
													{
														if (!this.npcsOrItems.Any(delegate(Either<Character, Item> n)
														{
															Character npc2;
															return n.TryGet(out npc2) && npc2 == npc;
														}))
														{
															this.npcsOrItems.Add(npc);
														}
														if (npc.CampaignInteractionType != CampaignMode.InteractionType.Talk)
														{
															if (npc.CampaignInteractionType != CampaignMode.InteractionType.Examine)
															{
																npc.CampaignInteractionType = CampaignMode.InteractionType.Examine;
																npc.RequireConsciousnessForCustomInteract = this.DisableIfTargetIncapacitated;
																npc.SetCustomInteract(delegate(Character npc, Character interactor)
																{
																	if (CS$<>8__locals2.CS$<>8__locals1.e1 == npc && CS$<>8__locals2.CS$<>8__locals1.<>4__this.ParentEvent.GetTargets(CS$<>8__locals2.CS$<>8__locals1.<>4__this.Target2Tag).Contains(interactor))
																	{
																		CS$<>8__locals2.CS$<>8__locals1.<>4__this.Trigger(npc, interactor);
																		return;
																	}
																	if (CS$<>8__locals2.CS$<>8__locals1.<>4__this.ParentEvent.GetTargets(CS$<>8__locals2.CS$<>8__locals1.<>4__this.Target1Tag).Contains(interactor) && CS$<>8__locals2.e2 == npc)
																	{
																		CS$<>8__locals2.CS$<>8__locals1.<>4__this.Trigger(interactor, npc);
																	}
																}, TextManager.Get("CampaignInteraction.Talk"));
																GameMain.NetworkMember.CreateEntityEvent(npc, default(Character.AssignCampaignInteractionEventData));
															}
															if (!this.AllowMultipleTargets)
															{
																return;
															}
														}
													}
													else if (item != null)
													{
														if (!this.npcsOrItems.Any(delegate(Either<Character, Item> n)
														{
															Item item2;
															return n.TryGet(out item2) && item2 == item;
														}))
														{
															this.npcsOrItems.Add(item);
														}
														Item item3 = item;
														CampaignMode.InteractionType interactionType = CampaignMode.InteractionType.Examine;
														NetworkMember networkMember = GameMain.NetworkMember;
														IEnumerable<Client> targetClients;
														if (networkMember == null)
														{
															targetClients = null;
														}
														else
														{
															IEnumerable<Client> connectedClients = networkMember.ConnectedClients;
															Func<Client, bool> predicate;
															if ((predicate = CS$<>8__locals2.CS$<>8__locals1.<>9__3) == null)
															{
																predicate = (CS$<>8__locals2.CS$<>8__locals1.<>9__3 = ((Client c) => c.Character != null && CS$<>8__locals2.CS$<>8__locals1.targets2.Contains(c.Character)));
															}
															targetClients = connectedClients.Where(predicate);
														}
														item3.AssignCampaignInteractionType(interactionType, targetClients);
														if (player.SelectedItem == item || player.SelectedSecondaryItem == item || (player.Inventory != null && player.Inventory.Contains(item)) || (player.FocusedItem == item && player.IsKeyHit(InputType.Use)))
														{
															this.Trigger(CS$<>8__locals2.CS$<>8__locals1.e1, CS$<>8__locals2.e2);
															return;
														}
													}
												}
											}
											else
											{
												TriggerAction.<>c__DisplayClass61_3 CS$<>8__locals4;
												CS$<>8__locals4.pos1 = CS$<>8__locals2.CS$<>8__locals1.e1.WorldPosition;
												CS$<>8__locals4.pos2 = CS$<>8__locals2.e2.WorldPosition;
												this.distance = Vector2.Distance(CS$<>8__locals4.pos1, CS$<>8__locals4.pos2);
												if (this.Type == TriggerAction.TriggerType.Inside == CS$<>8__locals2.<Update>g__IsWithinRadius|4(ref CS$<>8__locals4))
												{
													if (!this.CheckAllTargets)
													{
														this.Trigger(CS$<>8__locals2.CS$<>8__locals1.e1, CS$<>8__locals2.e2);
														return;
													}
													this.triggerers.Add(new ValueTuple<Entity, Entity>(CS$<>8__locals2.CS$<>8__locals1.e1, CS$<>8__locals2.e2));
												}
												else if (this.CheckAllTargets)
												{
													return;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			foreach (ValueTuple<Entity, Entity> valueTuple in this.triggerers)
			{
				Entity e = valueTuple.Item1;
				Entity e2 = valueTuple.Item2;
				this.Trigger(e, e2);
			}
		}

		// Token: 0x060020E0 RID: 8416 RVA: 0x000DDDC0 File Offset: 0x000DBFC0
		private void ResetTargetIcons()
		{
			foreach (Either<Character, Item> npcOrItem in this.npcsOrItems)
			{
				Character npc;
				Item item;
				if (npcOrItem.TryGet(out npc))
				{
					npc.CampaignInteractionType = CampaignMode.InteractionType.None;
					npc.SetCustomInteract(null, null);
					npc.RequireConsciousnessForCustomInteract = true;
					GameMain.NetworkMember.CreateEntityEvent(npc, default(Character.AssignCampaignInteractionEventData));
				}
				else if (npcOrItem.TryGet(out item))
				{
					item.AssignCampaignInteractionType(CampaignMode.InteractionType.None, null);
				}
			}
		}

		// Token: 0x060020E1 RID: 8417 RVA: 0x000DDE5C File Offset: 0x000DC05C
		private bool CheckDistanceToHull(Entity e, out Hull hull)
		{
			hull = null;
			if (this.Radius > 0f)
			{
				foreach (Hull potentialHull in Hull.HullList)
				{
					if (potentialHull.OutpostModuleTags.Contains(this.TargetModuleType))
					{
						Rectangle hullRect = potentialHull.WorldRect;
						hullRect.Inflate(this.Radius, this.Radius);
						if (Submarine.RectContains(hullRect, e.WorldPosition, false))
						{
							hull = potentialHull;
							return this.Type == TriggerAction.TriggerType.Inside;
						}
					}
				}
				return this.Type == TriggerAction.TriggerType.Outside;
			}
			Character character = e as Character;
			if (character != null && character.CurrentHull != null && character.CurrentHull.OutpostModuleTags.Contains(this.TargetModuleType))
			{
				hull = character.CurrentHull;
				return this.Type == TriggerAction.TriggerType.Inside;
			}
			Item item = e as Item;
			if (item != null && item.CurrentHull != null && item.CurrentHull.OutpostModuleTags.Contains(this.TargetModuleType))
			{
				hull = item.CurrentHull;
				return this.Type == TriggerAction.TriggerType.Inside;
			}
			return this.Type == TriggerAction.TriggerType.Outside;
		}

		// Token: 0x060020E2 RID: 8418 RVA: 0x000DDF98 File Offset: 0x000DC198
		private static bool IsInCombat(Entity entity)
		{
			Character character = entity as Character;
			if (character == null)
			{
				return false;
			}
			foreach (Character c in Character.CharacterList)
			{
				if (!c.IsDead && !c.Removed && !c.IsIncapacitated && c.Enabled)
				{
					if (c.IsBot)
					{
						HumanAIController humanAi = c.AIController as HumanAIController;
						if (humanAi != null)
						{
							AIObjectiveCombat combatObjective = humanAi.ObjectiveManager.CurrentObjective as AIObjectiveCombat;
							if (combatObjective != null && combatObjective.Enemy == character)
							{
								return true;
							}
							continue;
						}
					}
					AIController aicontroller = c.AIController;
					EnemyAIController enemyAI = aicontroller as EnemyAIController;
					if (enemyAI == null)
					{
						goto IL_B9;
					}
					AIState state = enemyAI.State;
					if (state != AIState.Attack && state != AIState.Aggressive)
					{
						goto IL_B9;
					}
					bool flag = true;
					IL_BC:
					if (!flag)
					{
						continue;
					}
					AITarget selectedAiTarget = enemyAI.SelectedAiTarget;
					if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == character || c.CurrentHull == character.CurrentHull)
					{
						return true;
					}
					continue;
					IL_B9:
					flag = false;
					goto IL_BC;
				}
			}
			return false;
		}

		// Token: 0x060020E3 RID: 8419 RVA: 0x000DE0C0 File Offset: 0x000DC2C0
		private void Trigger(Entity entity1, Entity entity2)
		{
			this.ResetTargetIcons();
			if (!this.ApplyToTarget1.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyToTarget1, entity1);
			}
			if (!this.ApplyToTarget2.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyToTarget2, entity2);
			}
			Character player = null;
			Entity target = null;
			Character character = entity1 as Character;
			if (character != null && character.IsPlayer)
			{
				player = (entity1 as Character);
				target = entity2;
			}
			else
			{
				character = (entity2 as Character);
				if (character != null && character.IsPlayer)
				{
					player = (entity2 as Character);
					target = entity1;
				}
			}
			if (player != null && this.SelectOnTrigger)
			{
				Character targetCharacter = target as Character;
				if (targetCharacter != null)
				{
					player.SelectCharacter(targetCharacter);
				}
				else
				{
					Item targetItem = target as Item;
					if (targetItem != null)
					{
						if (targetItem.IsSecondaryItem)
						{
							player.SelectedSecondaryItem = targetItem;
						}
						else
						{
							player.SelectedItem = targetItem;
						}
					}
				}
			}
			this.isRunning = false;
			this.isFinished = true;
		}

		// Token: 0x060020E4 RID: 8420 RVA: 0x000DE1AC File Offset: 0x000DC3AC
		public override string ToDebugString()
		{
			if (this.TargetModuleType.IsEmpty)
			{
				string targetStr = "none";
				if (this.npcsOrItems.Any<Either<Character, Item>>())
				{
					targetStr = string.Join(", ", this.npcsOrItems.Select(delegate(Either<Character, Item> npcOrItem)
					{
						Character character;
						if (npcOrItem.TryGet(out character))
						{
							return character.Name;
						}
						Item item;
						if (!npcOrItem.TryGet(out item))
						{
							return "none";
						}
						return item.Name;
					}));
				}
				return string.Concat(new string[]
				{
					ToolBox.GetDebugSymbol(this.isFinished, this.isRunning),
					" TriggerAction -> (",
					this.WaitForInteraction ? ("Selected non-player target: " + targetStr.ColorizeObject() + ", ") : ("Distance: " + ((int)this.distance).ColorizeObject() + ", "),
					"Radius: ",
					this.Radius.ColorizeObject(),
					", TargetTags: ",
					this.Target1Tag.ColorizeObject(),
					", ",
					this.Target2Tag.ColorizeObject(),
					")"
				});
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, this.isRunning));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("TriggerAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTags: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Target1Tag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetModuleType.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F83 RID: 3971
		private float distance;

		// Token: 0x04000F84 RID: 3972
		private bool isFinished;

		// Token: 0x04000F85 RID: 3973
		public bool isRunning;

		// Token: 0x04000F86 RID: 3974
		private readonly List<Either<Character, Item>> npcsOrItems = new List<Either<Character, Item>>();

		// Token: 0x04000F87 RID: 3975
		[TupleElementNames(new string[]
		{
			"e1",
			"e2"
		})]
		private readonly List<ValueTuple<Entity, Entity>> triggerers = new List<ValueTuple<Entity, Entity>>();

		// Token: 0x02000939 RID: 2361
		public enum TriggerType
		{
			// Token: 0x0400326F RID: 12911
			Inside,
			// Token: 0x04003270 RID: 12912
			Outside
		}
	}
}
