using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002AA RID: 682
	internal class TriggerAction : EventAction
	{
		// Token: 0x17000FA5 RID: 4005
		// (get) Token: 0x06003B76 RID: 15222 RVA: 0x00223429 File Offset: 0x00221629
		// (set) Token: 0x06003B77 RID: 15223 RVA: 0x00223431 File Offset: 0x00221631
		[Serialize("", IsPropertySaveable.Yes, "Tag of the first entity that will be used for trigger checks.", "", false)]
		public Identifier Target1Tag { get; set; }

		// Token: 0x17000FA6 RID: 4006
		// (get) Token: 0x06003B78 RID: 15224 RVA: 0x0022343A File Offset: 0x0022163A
		// (set) Token: 0x06003B79 RID: 15225 RVA: 0x00223442 File Offset: 0x00221642
		[Serialize("", IsPropertySaveable.Yes, "Tag of the second entity that will be used for trigger checks.", "", false)]
		public Identifier Target2Tag { get; set; }

		// Token: 0x17000FA7 RID: 4007
		// (get) Token: 0x06003B7A RID: 15226 RVA: 0x0022344B File Offset: 0x0022164B
		// (set) Token: 0x06003B7B RID: 15227 RVA: 0x00223453 File Offset: 0x00221653
		[Serialize("", IsPropertySaveable.Yes, "If set, the first target has to be within an outpost module of this type.", "", false)]
		public Identifier TargetModuleType { get; set; }

		// Token: 0x17000FA8 RID: 4008
		// (get) Token: 0x06003B7C RID: 15228 RVA: 0x0022345C File Offset: 0x0022165C
		// (set) Token: 0x06003B7D RID: 15229 RVA: 0x00223464 File Offset: 0x00221664
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the first entity when the trigger check succeeds.", "", false)]
		public Identifier ApplyToTarget1 { get; set; }

		// Token: 0x17000FA9 RID: 4009
		// (get) Token: 0x06003B7E RID: 15230 RVA: 0x0022346D File Offset: 0x0022166D
		// (set) Token: 0x06003B7F RID: 15231 RVA: 0x00223475 File Offset: 0x00221675
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the second entity when the trigger check succeeds.", "", false)]
		public Identifier ApplyToTarget2 { get; set; }

		// Token: 0x17000FAA RID: 4010
		// (get) Token: 0x06003B80 RID: 15232 RVA: 0x0022347E File Offset: 0x0022167E
		// (set) Token: 0x06003B81 RID: 15233 RVA: 0x00223486 File Offset: 0x00221686
		[Serialize(TriggerAction.TriggerType.Inside, IsPropertySaveable.Yes, "Determines if the targets must be inside or outside of the radius.", "", false)]
		public TriggerAction.TriggerType Type { get; set; }

		// Token: 0x17000FAB RID: 4011
		// (get) Token: 0x06003B82 RID: 15234 RVA: 0x0022348F File Offset: 0x0022168F
		// (set) Token: 0x06003B83 RID: 15235 RVA: 0x00223497 File Offset: 0x00221697
		[Serialize(0f, IsPropertySaveable.Yes, "Range to activate the trigger.", "", false)]
		public float Radius { get; set; }

		// Token: 0x17000FAC RID: 4012
		// (get) Token: 0x06003B84 RID: 15236 RVA: 0x002234A0 File Offset: 0x002216A0
		// (set) Token: 0x06003B85 RID: 15237 RVA: 0x002234A8 File Offset: 0x002216A8
		[Serialize(true, IsPropertySaveable.Yes, "If true, characters who are being targeted by some enemy cannot trigger the action.", "", false)]
		public bool DisableInCombat { get; set; }

		// Token: 0x17000FAD RID: 4013
		// (get) Token: 0x06003B86 RID: 15238 RVA: 0x002234B1 File Offset: 0x002216B1
		// (set) Token: 0x06003B87 RID: 15239 RVA: 0x002234B9 File Offset: 0x002216B9
		[Serialize(true, IsPropertySaveable.Yes, "If true, dead/unconscious characters cannot trigger the action.", "", false)]
		public bool DisableIfTargetIncapacitated { get; set; }

		// Token: 0x17000FAE RID: 4014
		// (get) Token: 0x06003B88 RID: 15240 RVA: 0x002234C2 File Offset: 0x002216C2
		// (set) Token: 0x06003B89 RID: 15241 RVA: 0x002234CA File Offset: 0x002216CA
		[Serialize(false, IsPropertySaveable.Yes, "If true, one target must interact with the other to trigger the action.", "", false)]
		public bool WaitForInteraction { get; set; }

		// Token: 0x17000FAF RID: 4015
		// (get) Token: 0x06003B8A RID: 15242 RVA: 0x002234D3 File Offset: 0x002216D3
		// (set) Token: 0x06003B8B RID: 15243 RVA: 0x002234DB File Offset: 0x002216DB
		[Serialize(false, IsPropertySaveable.Yes, "If true, the action can be triggered by interacting with any matching target (not just the 1st one).", "", false)]
		public bool AllowMultipleTargets { get; set; }

		// Token: 0x17000FB0 RID: 4016
		// (get) Token: 0x06003B8C RID: 15244 RVA: 0x002234E4 File Offset: 0x002216E4
		// (set) Token: 0x06003B8D RID: 15245 RVA: 0x002234EC File Offset: 0x002216EC
		[Serialize(false, IsPropertySaveable.Yes, "If true and using multiple targets, all targets must be inside/outside the radius.", "", false)]
		public bool CheckAllTargets { get; set; }

		// Token: 0x17000FB1 RID: 4017
		// (get) Token: 0x06003B8E RID: 15246 RVA: 0x002234F5 File Offset: 0x002216F5
		// (set) Token: 0x06003B8F RID: 15247 RVA: 0x002234FD File Offset: 0x002216FD
		[Serialize(false, IsPropertySaveable.Yes, "If true, interacting with the target will make the character select it.", "", false)]
		public bool SelectOnTrigger { get; set; }

		// Token: 0x06003B90 RID: 15248 RVA: 0x00223508 File Offset: 0x00221708
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

		// Token: 0x06003B91 RID: 15249 RVA: 0x002235E9 File Offset: 0x002217E9
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003B92 RID: 15250 RVA: 0x002235F1 File Offset: 0x002217F1
		public override void Reset()
		{
			this.ResetTargetIcons();
			this.isRunning = false;
			this.isFinished = false;
		}

		// Token: 0x06003B93 RID: 15251 RVA: 0x00223608 File Offset: 0x00221808
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
														Character npc2;
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
																npc2 = npc;
																Action<Character, Character> onCustomInteract = delegate(Character npc, Character interactor)
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
																};
																string tag = "CampaignInteraction.Examine";
																string varName = "[key]";
																GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
																npc2.SetCustomInteract(onCustomInteract, TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.Use), FormatCapitals.No));
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

		// Token: 0x06003B94 RID: 15252 RVA: 0x00223C28 File Offset: 0x00221E28
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
				}
				else if (npcOrItem.TryGet(out item))
				{
					item.AssignCampaignInteractionType(CampaignMode.InteractionType.None, null);
				}
			}
		}

		// Token: 0x06003B95 RID: 15253 RVA: 0x00223CA8 File Offset: 0x00221EA8
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

		// Token: 0x06003B96 RID: 15254 RVA: 0x00223DE4 File Offset: 0x00221FE4
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

		// Token: 0x06003B97 RID: 15255 RVA: 0x00223F0C File Offset: 0x0022210C
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

		// Token: 0x06003B98 RID: 15256 RVA: 0x00223FF8 File Offset: 0x002221F8
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

		// Token: 0x04001E70 RID: 7792
		private float distance;

		// Token: 0x04001E71 RID: 7793
		private bool isFinished;

		// Token: 0x04001E72 RID: 7794
		public bool isRunning;

		// Token: 0x04001E73 RID: 7795
		private readonly List<Either<Character, Item>> npcsOrItems = new List<Either<Character, Item>>();

		// Token: 0x04001E74 RID: 7796
		[TupleElementNames(new string[]
		{
			"e1",
			"e2"
		})]
		private readonly List<ValueTuple<Entity, Entity>> triggerers = new List<ValueTuple<Entity, Entity>>();

		// Token: 0x02000F48 RID: 3912
		public enum TriggerType
		{
			// Token: 0x04005536 RID: 21814
			Inside,
			// Token: 0x04005537 RID: 21815
			Outside
		}
	}
}
