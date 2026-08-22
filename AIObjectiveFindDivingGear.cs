using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000177 RID: 375
	internal class AIObjectiveFindDivingGear : AIObjective
	{
		// Token: 0x17000B2C RID: 2860
		// (get) Token: 0x06002C58 RID: 11352 RVA: 0x001E3978 File Offset: 0x001E1B78
		// (set) Token: 0x06002C59 RID: 11353 RVA: 0x001E3980 File Offset: 0x001E1B80
		public override Identifier Identifier { get; set; } = "find diving gear".ToIdentifier();

		// Token: 0x17000B2D RID: 2861
		// (get) Token: 0x06002C5A RID: 11354 RVA: 0x001E398C File Offset: 0x001E1B8C
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.gearTag);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000B2E RID: 2862
		// (get) Token: 0x06002C5B RID: 11355 RVA: 0x001E39DB File Offset: 0x001E1BDB
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B2F RID: 2863
		// (get) Token: 0x06002C5C RID: 11356 RVA: 0x001E39DE File Offset: 0x001E1BDE
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B30 RID: 2864
		// (get) Token: 0x06002C5D RID: 11357 RVA: 0x001E39E1 File Offset: 0x001E1BE1
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B31 RID: 2865
		// (get) Token: 0x06002C5E RID: 11358 RVA: 0x001E39E4 File Offset: 0x001E1BE4
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002C5F RID: 11359 RVA: 0x001E39E7 File Offset: 0x001E1BE7
		protected override bool CheckObjectiveState()
		{
			return this.targetItem != null && this.character.HasEquippedItem(this.targetItem, new InvSlotType?(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes), null);
		}

		// Token: 0x06002C60 RID: 11360 RVA: 0x001E3A0C File Offset: 0x001E1C0C
		public AIObjectiveFindDivingGear(Character character, bool needsDivingSuit, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.gearTag = (needsDivingSuit ? Tags.HeavyDivingGear : Tags.LightDivingGear);
		}

		// Token: 0x06002C61 RID: 11361 RVA: 0x001E3A54 File Offset: 0x001E1C54
		protected override void Act(float deltaTime)
		{
			this.TrySetTargetItem(this.character.Inventory.FindItem((Item it) => it.HasTag(this.gearTag) && AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, this.gearTag, this.character), true));
			if (this.targetItem == null && this.gearTag == Tags.LightDivingGear)
			{
				this.TrySetTargetItem(this.character.Inventory.FindItem((Item it) => it.HasTag(Tags.HeavyDivingGear) && AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, Tags.HeavyDivingGear, this.character), true));
			}
			bool findDivingGear = this.targetItem == null || (!this.character.HasEquippedItem(this.targetItem, new InvSlotType?(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes), null) && this.targetItem.ContainedItems.Any(new Func<Item, bool>(this.IsSuitableContainedOxygenSource)));
			if (findDivingGear)
			{
				bool mustFindMorePressureProtection = !this.objectiveManager.FailedToFindDivingGearForDepth && this.character.Inventory.FindItem((Item it) => it.HasTag(Tags.HeavyDivingGear) && !AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, Tags.HeavyDivingGear, this.character), true) != null;
				if (this.gearTag == Tags.LightDivingGear)
				{
					Item divingSuit = this.character.GetEquippedItem(Tags.HeavyDivingGear, new InvSlotType?(InvSlotType.InnerClothes | InvSlotType.OuterClothes));
					if (divingSuit != null && divingSuit.ContainedItems.None(new Func<Item, bool>(this.IsSuitableContainedOxygenSource)))
					{
						this.targetItem = divingSuit;
						findDivingGear = false;
					}
				}
				if (findDivingGear)
				{
					Func<Item, float> <>9__8;
					base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getDivingGear, delegate
					{
						if (this.targetItem == null && this.character.IsOnPlayerTeam)
						{
							this.character.Speak(TextManager.Get("DialogGetDivingGear").Value, null, 0f, "getdivinggear".ToIdentifier(), 30f);
						}
						bool flag;
						AIObjectiveGetItem getItemObjective = new AIObjectiveGetItem(this.character, this.gearTag, this.objectiveManager, true, true, 1f, false)
						{
							IsFindDivingGearSubObjective = true,
							AllowStealing = this.HumanAIController.NeedsDivingGear(this.character.CurrentHull, out flag, null),
							AllowToFindDivingGear = false,
							AllowDangerousPressure = true,
							EquipSlotType = new InvSlotType?(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes),
							Wear = true
						};
						if (this.gearTag == Tags.HeavyDivingGear)
						{
							if (mustFindMorePressureProtection)
							{
								getItemObjective.ItemFilter = ((Item it) => AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, this.gearTag, this.character));
							}
							else
							{
								getItemObjective.GetItemPriority = delegate(Item it)
								{
									if (!AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, this.gearTag, this.character))
									{
										return 1f;
									}
									return 1000f;
								};
							}
							AIObjectiveGetItem aiobjectiveGetItem = getItemObjective;
							Func<Item, float> getItemPriority;
							if ((getItemPriority = <>9__8) == null)
							{
								getItemPriority = (<>9__8 = delegate(Item it)
								{
									if (AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, this.gearTag, this.character))
									{
										return 1000f;
									}
									if (!mustFindMorePressureProtection)
									{
										return 1f;
									}
									return 0f;
								});
							}
							aiobjectiveGetItem.GetItemPriority = getItemPriority;
						}
						return getItemObjective;
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveGetItem>(ref this.getDivingGear);
						IEnumerable<Item> masks;
						if (this.gearTag == Tags.HeavyDivingGear && HumanAIController.HasItem(this.character, Tags.LightDivingGear, out masks, default(Identifier), 0f, true, true, null))
						{
							foreach (Item mask in masks)
							{
								if (mask != this.targetItem)
								{
									this.character.Inventory.TryPutItem(mask, this.character, CharacterInventory.AnySlot, true, false, true);
								}
							}
						}
					}, delegate
					{
						if (mustFindMorePressureProtection)
						{
							this.objectiveManager.FailedToFindDivingGearForDepth = true;
						}
						this.Abandon = true;
					});
				}
			}
			if (!findDivingGear)
			{
				float min = AIObjectiveFindDivingGear.GetMinOxygen(this.character);
				if (this.targetItem.OwnInventory != null && this.targetItem.OwnInventory.AllItems.None(new Func<Item, bool>(this.IsSuitableContainedOxygenSource)))
				{
					Func<Item, bool> <>9__13;
					base.TryAddSubObjective<AIObjectiveContainItem>(ref this.getOxygen, delegate
					{
						if (this.character.IsOnPlayerTeam)
						{
							Character character = this.character;
							Identifier oxygenSource = Tags.OxygenSource;
							float min = min;
							IEnumerable<Item> enumerable;
							if (HumanAIController.HasItem(character, oxygenSource, out enumerable, default(Identifier), min, false, true, null))
							{
								this.character.Speak(TextManager.Get("dialogswappingoxygentank").Value, null, 0f, "swappingoxygentank".ToIdentifier(), 30f);
								Inventory inventory = this.character.Inventory;
								Func<Item, bool> predicate;
								if ((predicate = <>9__13) == null)
								{
									predicate = (<>9__13 = ((Item i) => i.HasTag(Tags.OxygenSource) && i.Condition > min));
								}
								if (inventory.FindAllItems(predicate, true, null).Count == 1)
								{
									this.character.Speak(TextManager.Get("dialoglastoxygentank").Value, null, 0f, "dialoglastoxygentank".ToIdentifier(), 30f);
								}
							}
							else
							{
								this.character.Speak(TextManager.Get("DialogGetOxygenTank").Value, null, 0f, "getoxygentank".ToIdentifier(), 30f);
							}
						}
						ItemContainer container = this.targetItem.GetComponent<ItemContainer>();
						bool flag;
						AIObjectiveContainItem objective = new AIObjectiveContainItem(this.character, Tags.OxygenSource, container, this.objectiveManager, 1f, this.character.TeamID == CharacterTeamType.FriendlyNPC)
						{
							AllowToFindDivingGear = false,
							AllowDangerousPressure = true,
							ConditionLevel = 10f,
							RemoveExistingWhenNecessary = true,
							TargetSlot = this.oxygenSourceSlotIndex,
							AllowStealing = this.HumanAIController.NeedsDivingGear(this.character.CurrentHull, out flag, null)
						};
						if (container.HasSubContainers)
						{
							objective.TargetSlot = container.FindSuitableSubContainerIndex(Tags.OxygenSource);
						}
						objective.RemoveExistingPredicate = ((Item i) => objective.IsInTargetSlot(i));
						return objective;
					}, delegate
					{
						base.RemoveSubObjective<AIObjectiveContainItem>(ref this.getOxygen);
						this.<Act>g__ReportOxygenTankCount|22_12();
					}, delegate
					{
						this.getOxygen = null;
						int remainingTanks = this.<Act>g__ReportOxygenTankCount|22_12();
						base.TryAddSubObjective<AIObjectiveContainItem>(ref this.getOxygen, () => new AIObjectiveContainItem(this.character, Tags.OxygenSource, this.targetItem.GetComponent<ItemContainer>(), this.objectiveManager, 1f, this.character.TeamID == CharacterTeamType.FriendlyNPC)
						{
							AllowToFindDivingGear = false,
							AllowDangerousPressure = true,
							RemoveExisting = true
						}, delegate
						{
							base.RemoveSubObjective<AIObjectiveContainItem>(ref this.getOxygen);
						}, delegate
						{
							this.Abandon = true;
							IEnumerable<Item> enumerable;
							if (remainingTanks > 0 && !HumanAIController.HasItem(this.character, Tags.OxygenSource, out enumerable, default(Identifier), 0.01f, false, true, null))
							{
								this.character.Speak(TextManager.Get("dialogcantfindtoxygen").Value, null, 0f, "cantfindoxygen".ToIdentifier(), 30f);
							}
						});
					});
				}
			}
		}

		// Token: 0x06002C62 RID: 11362 RVA: 0x001E3C58 File Offset: 0x001E1E58
		public static bool IsSuitablePressureProtection(Item item, Identifier tag, Character character)
		{
			if (tag == Tags.HeavyDivingGear)
			{
				Level loaded = Level.Loaded;
				float realWorldDepth = (loaded != null) ? loaded.GetRealWorldDepth(character.WorldPosition.Y) : 0f;
				Wearable wearable = item.GetComponent<Wearable>();
				if (wearable == null || wearable.PressureProtection < realWorldDepth + 500f)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002C63 RID: 11363 RVA: 0x001E3CB0 File Offset: 0x001E1EB0
		private bool IsSuitableContainedOxygenSource(Item item)
		{
			return item != null && item.HasTag(Tags.OxygenSource) && item.Condition > 0f && (this.oxygenSourceSlotIndex == null || item.ParentInventory.IsInSlot(item, this.oxygenSourceSlotIndex.Value));
		}

		// Token: 0x06002C64 RID: 11364 RVA: 0x001E3D04 File Offset: 0x001E1F04
		private void TrySetTargetItem(Item item)
		{
			if (this.targetItem == item)
			{
				return;
			}
			this.targetItem = item;
			Item item2 = this.targetItem;
			int? num;
			if (item2 == null)
			{
				num = null;
			}
			else
			{
				ItemContainer component = item2.GetComponent<ItemContainer>();
				num = ((component != null) ? component.FindSuitableSubContainerIndex(Tags.OxygenSource) : null);
			}
			this.oxygenSourceSlotIndex = num;
		}

		// Token: 0x06002C65 RID: 11365 RVA: 0x001E3D5B File Offset: 0x001E1F5B
		public override void Reset()
		{
			base.Reset();
			this.getDivingGear = null;
			this.getOxygen = null;
			this.targetItem = null;
			this.oxygenSourceSlotIndex = null;
		}

		// Token: 0x06002C66 RID: 11366 RVA: 0x001E3D84 File Offset: 0x001E1F84
		public static float GetMinOxygen(Character character)
		{
			float min = 0.01f;
			float minOxygen = character.IsInFriendlySub ? 10f : min;
			if (minOxygen > min && character.Inventory.AllItems.Any((Item i) => i.HasTag(Tags.OxygenSource) && i.ConditionPercentage >= minOxygen))
			{
				minOxygen = min;
			}
			return minOxygen;
		}

		// Token: 0x06002C71 RID: 11377 RVA: 0x001E4014 File Offset: 0x001E2214
		[CompilerGenerated]
		private int <Act>g__ReportOxygenTankCount|22_12()
		{
			if (this.character.Submarine != Submarine.MainSub)
			{
				return 1;
			}
			Submarine mainSub = Submarine.MainSub;
			int num;
			if (mainSub == null)
			{
				num = 0;
			}
			else
			{
				num = mainSub.GetItems(false).Count((Item i) => i.HasTag(Tags.OxygenSource) && i.Condition > 1f);
			}
			int remainingOxygenTanks = num;
			if (remainingOxygenTanks == 0)
			{
				this.character.Speak(TextManager.Get("DialogOutOfOxygenTanks").Value, null, 0f, "outofoxygentanks".ToIdentifier(), 30f);
			}
			else if (remainingOxygenTanks < 10)
			{
				this.character.Speak(TextManager.Get("DialogLowOnOxygenTanks").Value, null, 0f, "lowonoxygentanks".ToIdentifier(), 30f);
			}
			return remainingOxygenTanks;
		}

		// Token: 0x0400171C RID: 5916
		private readonly Identifier gearTag;

		// Token: 0x0400171D RID: 5917
		private AIObjectiveGetItem getDivingGear;

		// Token: 0x0400171E RID: 5918
		private AIObjectiveContainItem getOxygen;

		// Token: 0x0400171F RID: 5919
		private Item targetItem;

		// Token: 0x04001720 RID: 5920
		private int? oxygenSourceSlotIndex;

		// Token: 0x04001721 RID: 5921
		private const float MinOxygen = 10f;
	}
}
