using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200018B RID: 395
	internal class AIObjectiveRescue : AIObjective
	{
		// Token: 0x17000BEB RID: 3051
		// (get) Token: 0x06002E94 RID: 11924 RVA: 0x001F0CB3 File Offset: 0x001EEEB3
		// (set) Token: 0x06002E95 RID: 11925 RVA: 0x001F0CBB File Offset: 0x001EEEBB
		public override Identifier Identifier { get; set; } = "rescue".ToIdentifier();

		// Token: 0x17000BEC RID: 3052
		// (get) Token: 0x06002E96 RID: 11926 RVA: 0x001F0CC4 File Offset: 0x001EEEC4
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BED RID: 3053
		// (get) Token: 0x06002E97 RID: 11927 RVA: 0x001F0CC7 File Offset: 0x001EEEC7
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BEE RID: 3054
		// (get) Token: 0x06002E98 RID: 11928 RVA: 0x001F0CCA File Offset: 0x001EEECA
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BEF RID: 3055
		// (get) Token: 0x06002E99 RID: 11929 RVA: 0x001F0CCD File Offset: 0x001EEECD
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BF0 RID: 3056
		// (get) Token: 0x06002E9A RID: 11930 RVA: 0x001F0CD0 File Offset: 0x001EEED0
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002E9B RID: 11931 RVA: 0x001F0CD4 File Offset: 0x001EEED4
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

		// Token: 0x06002E9C RID: 11932 RVA: 0x001F0D7D File Offset: 0x001EEF7D
		protected override void OnAbandon()
		{
			this.character.SelectedCharacter = null;
			base.OnAbandon();
		}

		// Token: 0x06002E9D RID: 11933 RVA: 0x001F0D91 File Offset: 0x001EEF91
		protected override void OnCompleted()
		{
			this.character.SelectedCharacter = null;
			base.OnCompleted();
		}

		// Token: 0x06002E9E RID: 11934 RVA: 0x001F0DA8 File Offset: 0x001EEFA8
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

		// Token: 0x06002E9F RID: 11935 RVA: 0x001F145C File Offset: 0x001EF65C
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

		// Token: 0x06002EA0 RID: 11936 RVA: 0x001F1BC4 File Offset: 0x001EFDC4
		public static Item FindMedicalItem(Inventory inventory, Identifier itemIdentifier)
		{
			return AIObjectiveRescue.FindMedicalItem(inventory, (Item it) => it.Prefab.Identifier == itemIdentifier);
		}

		// Token: 0x06002EA1 RID: 11937 RVA: 0x001F1BF0 File Offset: 0x001EFDF0
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

		// Token: 0x06002EA2 RID: 11938 RVA: 0x001F1C8C File Offset: 0x001EFE8C
		private void SpeakCannotTreat()
		{
			LocalizedString msg = (this.character == this.Target) ? TextManager.Get("dialogcannottreatself") : TextManager.GetWithVariable("dialogcannottreatpatient", "[name]", this.Target.DisplayName, FormatCapitals.No);
			Character character = this.character;
			string value = msg.Value;
			Identifier identifier = "cannottreatpatient".ToIdentifier();
			character.Speak(value, null, 0f, identifier, 20f);
		}

		// Token: 0x06002EA3 RID: 11939 RVA: 0x001F1D04 File Offset: 0x001EFF04
		private void ApplyTreatment(Affliction affliction, Item item)
		{
			item.ApplyTreatment(this.character, this.Target, this.Target.CharacterHealth.GetAfflictionLimb(affliction));
		}

		// Token: 0x06002EA4 RID: 11940 RVA: 0x001F1D2C File Offset: 0x001EFF2C
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

		// Token: 0x06002EA5 RID: 11941 RVA: 0x001F1E10 File Offset: 0x001F0010
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

		// Token: 0x06002EA6 RID: 11942 RVA: 0x001F1FAD File Offset: 0x001F01AD
		public static IEnumerable<Affliction> GetSortedAfflictions(Character character, bool excludeBuffs = true)
		{
			return CharacterHealth.SortAfflictionsBySeverity(character.CharacterHealth.GetAllAfflictions(), excludeBuffs);
		}

		// Token: 0x06002EA7 RID: 11943 RVA: 0x001F1FC0 File Offset: 0x001F01C0
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

		// Token: 0x06002EA8 RID: 11944 RVA: 0x001F1FF7 File Offset: 0x001F01F7
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x06002EA9 RID: 11945 RVA: 0x001F200A File Offset: 0x001F020A
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

		// Token: 0x06002EAD RID: 11949 RVA: 0x001F206E File Offset: 0x001F026E
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

		// Token: 0x04001843 RID: 6211
		private const float TreatmentDelay = 0.5f;

		// Token: 0x04001844 RID: 6212
		private const float CloseEnoughToTreat = 100f;

		// Token: 0x04001845 RID: 6213
		public readonly Character Target;

		// Token: 0x04001846 RID: 6214
		private AIObjectiveGoTo goToObjective;

		// Token: 0x04001847 RID: 6215
		private AIObjectiveContainItem replaceOxygenObjective;

		// Token: 0x04001848 RID: 6216
		private AIObjectiveGetItem getItemObjective;

		// Token: 0x04001849 RID: 6217
		private float treatmentTimer;

		// Token: 0x0400184A RID: 6218
		private Hull safeHull;

		// Token: 0x0400184B RID: 6219
		private float findHullTimer;

		// Token: 0x0400184C RID: 6220
		private bool ignoreOxygen;

		// Token: 0x0400184D RID: 6221
		private readonly float findHullInterval = 1f;

		// Token: 0x0400184E RID: 6222
		private bool performedCpr;

		// Token: 0x0400184F RID: 6223
		private readonly List<Identifier> suitableItemIdentifiers = new List<Identifier>();

		// Token: 0x04001850 RID: 6224
		private readonly List<LocalizedString> itemNameList = new List<LocalizedString>();

		// Token: 0x04001851 RID: 6225
		private readonly Dictionary<Identifier, float> currentTreatmentSuitabilities = new Dictionary<Identifier, float>();
	}
}
