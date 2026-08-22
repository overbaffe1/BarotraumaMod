using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000085 RID: 133
	internal class AIObjectiveRescue : AIObjective
	{
		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x0600118C RID: 4492 RVA: 0x0009DE1B File Offset: 0x0009C01B
		// (set) Token: 0x0600118D RID: 4493 RVA: 0x0009DE23 File Offset: 0x0009C023
		public override Identifier Identifier { get; set; } = "rescue".ToIdentifier();

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x0600118E RID: 4494 RVA: 0x0009DE2C File Offset: 0x0009C02C
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x0600118F RID: 4495 RVA: 0x0009DE2F File Offset: 0x0009C02F
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x06001190 RID: 4496 RVA: 0x0009DE32 File Offset: 0x0009C032
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06001191 RID: 4497 RVA: 0x0009DE35 File Offset: 0x0009C035
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06001192 RID: 4498 RVA: 0x0009DE38 File Offset: 0x0009C038
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x0009DE3C File Offset: 0x0009C03C
		public AIObjectiveRescue(Character character, Character targetCharacter, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			if (targetCharacter == null)
			{
				string errorMsg = "Attempted to create a Rescue objective with no target!\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(character.Name + ": " + errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("AIObjectiveRescue:ctor:targetnull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				base.Abandon = true;
				return;
			}
			this.Target = targetCharacter;
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x0009DEE5 File Offset: 0x0009C0E5
		protected override void OnAbandon()
		{
			this.character.SelectedCharacter = null;
			base.OnAbandon();
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x0009DEF9 File Offset: 0x0009C0F9
		protected override void OnCompleted()
		{
			this.character.SelectedCharacter = null;
			base.OnCompleted();
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x0009DF10 File Offset: 0x0009C110
		protected override void Act(float deltaTime)
		{
			if (this.Target == null || this.Target.Removed || this.Target.IsDead)
			{
				base.Abandon = true;
				return;
			}
			Character otherRescuer = this.Target.SelectedBy;
			if (otherRescuer != null && otherRescuer != this.character)
			{
				base.Abandon = (otherRescuer.IsPlayer || this.character.GetSkillLevel(Tags.MedicalSkill) < otherRescuer.GetSkillLevel(Tags.MedicalSkill));
				return;
			}
			if (this.Target != this.character && this.Target.IsIncapacitated)
			{
				if ((!this.ignoreOxygen && this.character.SelectedCharacter == this.Target) || this.character.CanInteractWith(this.Target, 200f, true, false))
				{
					IEnumerable<Item> masks;
					IEnumerable<Item> suits;
					if (HumanAIController.HasItem(this.Target, Tags.HeavyDivingGear, out suits, default(Identifier), 0f, true, true, null))
					{
						Item suit2 = suits.FirstOrDefault<Item>();
						if (suit2 != null)
						{
							AIController.UnequipEmptyItems(this.character, suit2, true, false);
							AIController.UnequipContainedItems(this.character, suit2, (Item it) => it.HasTag(Tags.WeldingFuel), true, false, null);
						}
					}
					else if (HumanAIController.HasItem(this.Target, Tags.LightDivingGear, out masks, default(Identifier), 0f, true, true, null))
					{
						Item mask = masks.FirstOrDefault<Item>();
						if (mask != null)
						{
							AIController.UnequipEmptyItems(this.character, mask, true, false);
							AIController.UnequipContainedItems(this.character, mask, (Item it) => it.HasTag(Tags.WeldingFuel), true, false, null);
						}
					}
					if (this.<Act>g__ShouldRemoveDivingSuit|29_9())
					{
						suits.ForEach(delegate(Item suit)
						{
							suit.Drop(this.character, true, true);
						});
					}
					else if (suits.Any<Item>())
					{
						if (suits.None(delegate(Item s)
						{
							ItemInventory ownInventory = s.OwnInventory;
							if (((ownInventory != null) ? ownInventory.AllItems : null) != null)
							{
								return s.OwnInventory.AllItems.Any((Item it) => it.HasTag(Tags.OxygenSource) && it.ConditionPercentage > 0f);
							}
							return false;
						}))
						{
							Item spareOxygenTank = AIObjectiveRescue.<Act>g__FindOxygenTank|29_15(this.Target) ?? AIObjectiveRescue.<Act>g__FindOxygenTank|29_15(this.character);
							if (spareOxygenTank != null)
							{
								Item suit = suits.FirstOrDefault<Item>();
								if (suit != null)
								{
									base.TryAddSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective, () => new AIObjectiveContainItem(this.character, spareOxygenTank, suit.GetComponent<ItemContainer>(), this.objectiveManager, 1f), delegate
									{
										base.RemoveSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective);
									}, delegate
									{
										this.RemoveSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective);
										this.ignoreOxygen = true;
										if (this.<Act>g__ShouldRemoveDivingSuit|29_9())
										{
											suits.ForEach(delegate(Item suit)
											{
												suit.Drop(this.character, true, true);
											});
										}
									});
									return;
								}
							}
						}
					}
				}
				if (this.character.Submarine != null && this.Target.CurrentHull != null && base.HumanAIController.GetHullSafety(this.Target.CurrentHull, this.Target, null) < 40f)
				{
					if (this.character.SelectedCharacter != this.Target)
					{
						if (base.HumanAIController.VisibleHulls.Contains(this.Target.CurrentHull) && this.Target.CurrentHull.DisplayName != null)
						{
							this.character.Speak(TextManager.GetWithVariables("DialogFoundUnconsciousTarget", new ValueTuple<string, LocalizedString, FormatCapitals>[]
							{
								new ValueTuple<string, LocalizedString, FormatCapitals>("[targetname]", this.Target.DisplayName, FormatCapitals.No),
								new ValueTuple<string, LocalizedString, FormatCapitals>("[roomname]", this.Target.CurrentHull.DisplayName, FormatCapitals.Yes)
							}).Value, null, 1f, ("foundunconscioustarget" + this.Target.Name).ToIdentifier(), 60f);
						}
						if (!this.character.CanInteractWith(this.Target, 200f, true, false))
						{
							base.RemoveSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective);
							base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
							base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.Target, this.character, this.objectiveManager, false, true, 1f, 0f)
							{
								CloseEnough = 100f,
								DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachPatient,
								TargetName = this.Target.DisplayName
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
								base.Abandon = true;
							});
						}
						else
						{
							this.character.SelectCharacter(this.Target);
						}
					}
					else
					{
						if (this.safeHull == null)
						{
							if (this.findHullTimer > 0f)
							{
								this.findHullTimer -= deltaTime;
							}
							else
							{
								Hull potentialSafeHull;
								AIObjectiveFindSafety.HullSearchStatus hullSearchStatus = this.objectiveManager.GetObjective<AIObjectiveFindSafety>().FindBestHull(out potentialSafeHull, base.HumanAIController.VisibleHulls, true);
								if (hullSearchStatus != AIObjectiveFindSafety.HullSearchStatus.Finished)
								{
									return;
								}
								this.safeHull = potentialSafeHull;
								this.findHullTimer = this.findHullInterval * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
							}
						}
						if (this.safeHull != null && this.character.CurrentHull != this.safeHull)
						{
							base.RemoveSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective);
							base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
							base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.safeHull, this.character, this.objectiveManager, false, true, 1f, 0f)
							{
								DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachPlace
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
								this.safeHull = this.character.CurrentHull;
							});
						}
					}
				}
			}
			if (this.subObjectives.Any<AIObjective>())
			{
				return;
			}
			if (this.Target != this.character && !this.character.CanInteractWith(this.Target, 200f, true, false))
			{
				base.RemoveSubObjective<AIObjectiveContainItem>(ref this.replaceOxygenObjective);
				base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
				base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.Target, this.character, this.objectiveManager, false, true, 1f, 0f)
				{
					CloseEnough = 100f,
					DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachPatient,
					TargetName = this.Target.DisplayName
				}, delegate
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
				}, delegate
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					base.Abandon = true;
				});
				return;
			}
			if (this.character != this.Target && this.character.SelectedCharacter != this.Target)
			{
				Hull currentHull = this.Target.CurrentHull;
				if (((currentHull != null) ? currentHull.DisplayName : null) != null)
				{
					this.character.Speak(TextManager.GetWithVariables("DialogFoundWoundedTarget", new ValueTuple<string, LocalizedString, FormatCapitals>[]
					{
						new ValueTuple<string, LocalizedString, FormatCapitals>("[targetname]", this.Target.DisplayName, FormatCapitals.No),
						new ValueTuple<string, LocalizedString, FormatCapitals>("[roomname]", this.Target.CurrentHull.DisplayName, FormatCapitals.Yes)
					}).Value, null, 1f, ("foundwoundedtarget" + this.Target.Name).ToIdentifier(), 60f);
				}
			}
			this.GiveTreatment(deltaTime);
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x0009E5C4 File Offset: 0x0009C7C4
		private void GiveTreatment(float deltaTime)
		{
			if (this.Target == null)
			{
				string errorMsg = this.character.Name + ": Attempted to update a Rescue objective with no target!";
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				base.Abandon = true;
				return;
			}
			base.SteeringManager.Reset();
			if (!this.Target.IsPlayer)
			{
				AIController aicontroller = this.Target.AIController;
				if (aicontroller != null)
				{
					SteeringManager steeringManager = aicontroller.SteeringManager;
					if (steeringManager != null)
					{
						steeringManager.Reset();
					}
				}
			}
			if (this.treatmentTimer > 0f)
			{
				this.treatmentTimer -= deltaTime;
				return;
			}
			this.treatmentTimer = 0.5f;
			float cprSuitability = (this.Target.Oxygen < 0f) ? (-this.Target.Oxygen * 100f) : 0f;
			float bestSuitability = 0f;
			Item bestItem = null;
			Affliction afflictionToTreat = null;
			foreach (Affliction affliction in AIObjectiveRescue.GetSortedAfflictions(this.Target, true))
			{
				CharacterHealth characterHealth = this.Target.CharacterHealth;
				Dictionary<Identifier, float> treatmentSuitability2 = this.currentTreatmentSuitabilities;
				Limb afflictionLimb = this.Target.CharacterHealth.GetAfflictionLimb(affliction);
				characterHealth.GetSuitableTreatments(treatmentSuitability2, this.character, afflictionLimb, false, true, false, 10f);
				foreach (KeyValuePair<Identifier, float> treatmentSuitability3 in this.currentTreatmentSuitabilities)
				{
					float thisSuitability = this.currentTreatmentSuitabilities[treatmentSuitability3.Key];
					if (thisSuitability > 0f)
					{
						Item matchingItem = AIObjectiveRescue.FindMedicalItem(this.character.Inventory, treatmentSuitability3.Key);
						if (matchingItem == null && this.Target.IsIncapacitated)
						{
							matchingItem = AIObjectiveRescue.FindMedicalItem(this.Target.Inventory, treatmentSuitability3.Key);
						}
						if (matchingItem != null)
						{
							float suitabilityForThisAffliction = affliction.Prefab.GetTreatmentSuitability(matchingItem);
							float totalSuitability = thisSuitability * suitabilityForThisAffliction;
							if (matchingItem != null && totalSuitability > bestSuitability)
							{
								bestItem = matchingItem;
								afflictionToTreat = affliction;
								bestSuitability = totalSuitability;
							}
						}
					}
				}
			}
			if (bestItem != null && bestSuitability > cprSuitability)
			{
				if (this.Target != this.character)
				{
					this.character.SelectCharacter(this.Target);
				}
				this.ApplyTreatment(afflictionToTreat, bestItem);
				this.treatmentTimer = 2f;
				return;
			}
			if (this.character.Submarine != null && this.character.Submarine.TeamID == this.character.TeamID)
			{
				this.Target.CharacterHealth.GetSuitableTreatments(this.currentTreatmentSuitabilities, this.character, null, false, true, false, 10f);
				if (this.currentTreatmentSuitabilities.Any((KeyValuePair<Identifier, float> s) => s.Value > cprSuitability))
				{
					this.itemNameList.Clear();
					this.suitableItemIdentifiers.Clear();
					using (IEnumerator<KeyValuePair<Identifier, float>> enumerator3 = (from s in this.currentTreatmentSuitabilities
					orderby s.Value descending
					select s).GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							KeyValuePair<Identifier, float> treatmentSuitability = enumerator3.Current;
							ItemPrefab itemPrefab;
							if (treatmentSuitability.Value > cprSuitability && ItemPrefab.Prefabs.TryGet(treatmentSuitability.Key, out itemPrefab) && !Item.ItemList.None(delegate(Item it)
							{
								Prefab prefab = it.Prefab;
								Identifier key = treatmentSuitability.Key;
								return prefab.Identifier == key;
							}))
							{
								this.suitableItemIdentifiers.Add(itemPrefab.Identifier);
								if (this.itemNameList.Count < 4)
								{
									this.itemNameList.Add(itemPrefab.Name);
								}
							}
						}
					}
					if (this.itemNameList.Any<LocalizedString>())
					{
						LocalizedString itemListStr = "";
						if (this.itemNameList.Count == 1)
						{
							itemListStr = this.itemNameList[0];
						}
						else if (this.itemNameList.Count == 2)
						{
							itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsLast", new ValueTuple<string, LocalizedString>[]
							{
								new ValueTuple<string, LocalizedString>("[treatment1]", this.itemNameList[0]),
								new ValueTuple<string, LocalizedString>("[treatment2]", this.itemNameList[1])
							});
						}
						else
						{
							itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsFirst", new ValueTuple<string, LocalizedString>[]
							{
								new ValueTuple<string, LocalizedString>("[treatment1]", this.itemNameList[0]),
								new ValueTuple<string, LocalizedString>("[treatment2]", this.itemNameList[1])
							});
							for (int i = 2; i < this.itemNameList.Count - 1; i++)
							{
								itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsFirst", new ValueTuple<string, LocalizedString>[]
								{
									new ValueTuple<string, LocalizedString>("[treatment1]", itemListStr),
									new ValueTuple<string, LocalizedString>("[treatment2]", this.itemNameList[i])
								});
							}
							itemListStr = TextManager.GetWithVariables("DialogRequiredTreatmentOptionsLast", new ValueTuple<string, LocalizedString>[]
							{
								new ValueTuple<string, LocalizedString>("[treatment1]", itemListStr),
								new ValueTuple<string, LocalizedString>("[treatment2]", this.itemNameList.Last<LocalizedString>())
							});
						}
						if (this.Target != this.character && this.character.IsOnPlayerTeam)
						{
							this.character.Speak(TextManager.GetWithVariables("DialogListRequiredTreatments", new ValueTuple<string, LocalizedString, FormatCapitals>[]
							{
								new ValueTuple<string, LocalizedString, FormatCapitals>("[targetname]", this.Target.DisplayName, FormatCapitals.No),
								new ValueTuple<string, LocalizedString, FormatCapitals>("[treatmentlist]", itemListStr, FormatCapitals.Yes)
							}).Value, null, 2f, ("listrequiredtreatments" + this.Target.Name).ToIdentifier(), 60f);
						}
						IEnumerable<Identifier> itemsToFind = from kvp in this.currentTreatmentSuitabilities
						where kvp.Value > 0f && this.character.Inventory.AllItems.None(delegate(Item it)
						{
							Prefab prefab = it.Prefab;
							Identifier key = kvp.Key;
							return prefab.Identifier == key;
						})
						select kvp.Key;
						base.RemoveSubObjective<AIObjectiveGetItem>(ref this.getItemObjective);
						Func<Item, float> <>9__9;
						base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getItemObjective, delegate
						{
							AIObjectiveGetItem aiobjectiveGetItem = new AIObjectiveGetItem(this.character, itemsToFind, this.objectiveManager, true, true, 1f, this.character.TeamID == CharacterTeamType.FriendlyNPC);
							Func<Item, float> getItemPriority;
							if ((getItemPriority = <>9__9) == null)
							{
								getItemPriority = (<>9__9 = ((Item it) => this.currentTreatmentSuitabilities.GetValueOrDefault(it.Prefab.Identifier)));
							}
							aiobjectiveGetItem.GetItemPriority = getItemPriority;
							return aiobjectiveGetItem;
						}, delegate
						{
							this.RemoveSubObjective<AIObjectiveGetItem>(ref this.getItemObjective);
						}, delegate
						{
							this.Abandon = true;
							if (this.character.IsOnPlayerTeam)
							{
								this.SpeakCannotTreat();
							}
						});
					}
					else if (cprSuitability <= 0f)
					{
						base.Abandon = true;
						this.SpeakCannotTreat();
					}
				}
			}
			else if (!this.Target.IsUnconscious)
			{
				base.Abandon = true;
				this.SpeakCannotTreat();
				return;
			}
			if (this.character != this.Target)
			{
				if (cprSuitability > 0f)
				{
					this.character.SelectCharacter(this.Target);
					this.character.AnimController.Anim = AnimController.Animation.CPR;
					this.performedCpr = true;
					return;
				}
				this.character.DeselectCharacter();
			}
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x0009ED2C File Offset: 0x0009CF2C
		public static Item FindMedicalItem(Inventory inventory, Identifier itemIdentifier)
		{
			return AIObjectiveRescue.FindMedicalItem(inventory, (Item it) => it.Prefab.Identifier == itemIdentifier);
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x0009ED58 File Offset: 0x0009CF58
		public static Item FindMedicalItem(Inventory inventory, Func<Item, bool> predicate)
		{
			if (inventory == null)
			{
				return null;
			}
			Item match = inventory.FindItem(predicate, false);
			if (match != null)
			{
				return match;
			}
			foreach (Item potentialContainer in inventory.AllItems.OrderByDescending(delegate(Item it)
			{
				ItemInventory ownInventory2 = it.OwnInventory;
				if (ownInventory2 == null)
				{
					return -1;
				}
				return ownInventory2.Capacity;
			}))
			{
				ItemInventory ownInventory = potentialContainer.OwnInventory;
				match = ((ownInventory != null) ? ownInventory.FindItem(predicate, true) : null);
				if (match != null)
				{
					return match;
				}
			}
			return null;
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x0009EDF4 File Offset: 0x0009CFF4
		private void SpeakCannotTreat()
		{
			LocalizedString msg = (this.character == this.Target) ? TextManager.Get("dialogcannottreatself") : TextManager.GetWithVariable("dialogcannottreatpatient", "[name]", this.Target.DisplayName, FormatCapitals.No);
			Character character = this.character;
			string value = msg.Value;
			Identifier identifier = "cannottreatpatient".ToIdentifier();
			character.Speak(value, null, 0f, identifier, 20f);
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x0009EE6C File Offset: 0x0009D06C
		private void ApplyTreatment(Affliction affliction, Item item)
		{
			item.ApplyTreatment(this.character, this.Target, this.Target.CharacterHealth.GetAfflictionLimb(affliction));
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x0009EE94 File Offset: 0x0009D094
		protected override bool CheckObjectiveState()
		{
			base.IsCompleted = (AIObjectiveRescueAll.GetVitalityFactor(this.Target) >= AIObjectiveRescueAll.GetVitalityThreshold(this.objectiveManager, this.character, this.Target));
			if (base.IsCompleted && this.Target != this.character && this.character.IsOnPlayerTeam)
			{
				string textTag = this.performedCpr ? "DialogTargetResuscitated" : "DialogTargetHealed";
				LocalizedString withVariable = TextManager.GetWithVariable(textTag, "[targetname]", this.Target.DisplayName, FormatCapitals.No);
				string message = (withVariable != null) ? withVariable.Value : null;
				Character character = this.character;
				string message2 = message;
				Identifier identifier = ("targethealed" + this.Target.Name).ToIdentifier();
				character.Speak(message2, null, 1f, identifier, 60f);
			}
			return base.IsCompleted;
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x0009EF78 File Offset: 0x0009D178
		protected override float GetPriority()
		{
			if (this.Target == null)
			{
				base.Abandon = true;
			}
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
			}
			if (base.Abandon)
			{
				return base.Priority;
			}
			if (this.character.CurrentHull != null && Character.CharacterList.Any((Character c) => c.CurrentHull == this.Target.CurrentHull && !HumanAIController.IsFriendly(this.character, c, false, false) && HumanAIController.IsActive(c)))
			{
				base.Priority = 0f;
				base.Abandon = true;
				return base.Priority;
			}
			float horizontalDistance = Math.Abs(this.character.WorldPosition.X - this.Target.WorldPosition.X);
			float verticalDistance = Math.Abs(this.character.WorldPosition.Y - this.Target.WorldPosition.Y);
			Submarine submarine = this.character.Submarine;
			SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
			if (submarineInfo != null && !submarineInfo.IsRuin)
			{
				verticalDistance *= 2f;
			}
			float distanceFactor = MathHelper.Lerp(1f, 0.1f, MathUtils.InverseLerp(0f, 5000f, horizontalDistance + verticalDistance));
			if (this.character.CurrentHull != null && this.Target.CurrentHull == this.character.CurrentHull)
			{
				distanceFactor = 1f;
			}
			float vitalityFactor = 1f - AIObjectiveRescueAll.GetVitalityFactor(this.Target) / 100f;
			float devotion = base.CumulatedDevotion / 100f;
			base.Priority = MathHelper.Lerp(0f, 90f, MathHelper.Clamp(devotion + vitalityFactor * distanceFactor * base.PriorityModifier, 0f, 1f));
			return base.Priority;
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x0009F115 File Offset: 0x0009D315
		public static IEnumerable<Affliction> GetSortedAfflictions(Character character, bool excludeBuffs = true)
		{
			return CharacterHealth.SortAfflictionsBySeverity(character.CharacterHealth.GetAllAfflictions(), excludeBuffs);
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x0009F128 File Offset: 0x0009D328
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.getItemObjective = null;
			this.replaceOxygenObjective = null;
			this.safeHull = null;
			this.ignoreOxygen = false;
			this.character.SelectedCharacter = null;
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x0009F15F File Offset: 0x0009D35F
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x0009F172 File Offset: 0x0009D372
		[CompilerGenerated]
		private bool <Act>g__ShouldRemoveDivingSuit|29_9()
		{
			if (this.Target.OxygenAvailable < 30f)
			{
				Hull currentHull = this.Target.CurrentHull;
				return currentHull != null && currentHull.LethalPressure <= 0f;
			}
			return false;
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x0009F1D6 File Offset: 0x0009D3D6
		[CompilerGenerated]
		internal static Item <Act>g__FindOxygenTank|29_15(Character c)
		{
			return c.Inventory.FindItem(delegate(Item i)
			{
				if (i.HasTag(Tags.OxygenSource) && i.ConditionPercentage > 1f)
				{
					return i.FindParentInventory(delegate(Inventory inv)
					{
						Item otherItem = inv.Owner as Item;
						return otherItem != null && otherItem.HasTag(Tags.DivingGear);
					}) == null;
				}
				return false;
			}, true);
		}

		// Token: 0x04000849 RID: 2121
		private const float TreatmentDelay = 0.5f;

		// Token: 0x0400084A RID: 2122
		private const float CloseEnoughToTreat = 100f;

		// Token: 0x0400084B RID: 2123
		public readonly Character Target;

		// Token: 0x0400084C RID: 2124
		private AIObjectiveGoTo goToObjective;

		// Token: 0x0400084D RID: 2125
		private AIObjectiveContainItem replaceOxygenObjective;

		// Token: 0x0400084E RID: 2126
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x0400084F RID: 2127
		private float treatmentTimer;

		// Token: 0x04000850 RID: 2128
		private Hull safeHull;

		// Token: 0x04000851 RID: 2129
		private float findHullTimer;

		// Token: 0x04000852 RID: 2130
		private bool ignoreOxygen;

		// Token: 0x04000853 RID: 2131
		private readonly float findHullInterval = 1f;

		// Token: 0x04000854 RID: 2132
		private bool performedCpr;

		// Token: 0x04000855 RID: 2133
		private readonly List<Identifier> suitableItemIdentifiers = new List<Identifier>();

		// Token: 0x04000856 RID: 2134
		private readonly List<LocalizedString> itemNameList = new List<LocalizedString>();

		// Token: 0x04000857 RID: 2135
		private readonly Dictionary<Identifier, float> currentTreatmentSuitabilities = new Dictionary<Identifier, float>();
	}
}
