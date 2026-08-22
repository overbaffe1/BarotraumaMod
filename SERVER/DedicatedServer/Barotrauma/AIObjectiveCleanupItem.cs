using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000067 RID: 103
	internal class AIObjectiveCleanupItem : AIObjective
	{
		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x06000E47 RID: 3655 RVA: 0x0008AACE File Offset: 0x00088CCE
		// (set) Token: 0x06000E48 RID: 3656 RVA: 0x0008AAD6 File Offset: 0x00088CD6
		public override Identifier Identifier { get; set; } = "cleanup item".ToIdentifier();

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x06000E49 RID: 3657 RVA: 0x0008AADF File Offset: 0x00088CDF
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x06000E4A RID: 3658 RVA: 0x0008AAE2 File Offset: 0x00088CE2
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x06000E4B RID: 3659 RVA: 0x0008AAE5 File Offset: 0x00088CE5
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x0008AAE8 File Offset: 0x00088CE8
		// (set) Token: 0x06000E4D RID: 3661 RVA: 0x0008AAF0 File Offset: 0x00088CF0
		public bool IsPriority { get; set; }

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x06000E4E RID: 3662 RVA: 0x0008AAF9 File Offset: 0x00088CF9
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0008AAFC File Offset: 0x00088CFC
		public AIObjectiveCleanupItem(Item item, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.item = item;
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x0008AB40 File Offset: 0x00088D40
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			float distanceFactor = 0.9f;
			if (!this.IsPriority && this.item.CurrentHull != this.character.CurrentHull)
			{
				distanceFactor = base.GetDistanceFactor(this.item.WorldPosition, 0f, 5f, 5000f, 0.9f);
			}
			bool isSelected = this.character.HasItem(this.item, false, null);
			float selectedBonus = isSelected ? (100f - this.MaxDevotion) : 0f;
			float devotion = (base.CumulatedDevotion + selectedBonus) / 100f;
			float reduction = (float)(this.IsPriority ? 1 : (isSelected ? 2 : 3));
			float max = 60f - reduction;
			base.Priority = MathHelper.Lerp(0f, max, MathHelper.Clamp(devotion + distanceFactor * base.PriorityModifier, 0f, 1f));
			if (this.moveItemObjective == null)
			{
				base.Priority /= 2f;
			}
			return base.Priority;
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x0008AC5C File Offset: 0x00088E5C
		protected override void Act(float deltaTime)
		{
			AIObjectiveCleanupItem.<>c__DisplayClass22_0 CS$<>8__locals1 = new AIObjectiveCleanupItem.<>c__DisplayClass22_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.subObjectives.Any<AIObjective>())
			{
				return;
			}
			if (HumanAIController.FindSuitableContainer(this.character, this.item, this.ignoredContainers, ref this.itemIndex, out CS$<>8__locals1.suitableContainer))
			{
				if (CS$<>8__locals1.suitableContainer != null)
				{
					AIObjectiveCleanupItem.<>c__DisplayClass22_0 CS$<>8__locals2 = CS$<>8__locals1;
					bool equip;
					if (this.item.GetComponent<Holdable>() == null)
					{
						if (this.item.AllowedSlots.Any((InvSlotType s) => s != InvSlotType.Any))
						{
							equip = this.item.AllowedSlots.None((InvSlotType s) => s == InvSlotType.Card || s == InvSlotType.Head || s == InvSlotType.Headset || s == InvSlotType.InnerClothes || s == InvSlotType.OuterClothes || s == InvSlotType.HealthInterface);
						}
						else
						{
							equip = false;
						}
					}
					else
					{
						equip = true;
					}
					CS$<>8__locals2.equip = equip;
					base.TryAddSubObjective<AIObjectiveMoveItem>(ref this.moveItemObjective, () => new AIObjectiveMoveItem(CS$<>8__locals1.<>4__this.character, CS$<>8__locals1.<>4__this.item, CS$<>8__locals1.<>4__this.objectiveManager, null, CS$<>8__locals1.suitableContainer.GetComponent<ItemContainer>(), 1f)
					{
						Equip = CS$<>8__locals1.equip,
						TakeWholeStack = true,
						DropIfFails = true
					}, delegate
					{
						if (CS$<>8__locals1.equip)
						{
							CS$<>8__locals1.<>4__this.HumanAIController.ReequipUnequipped();
						}
						CS$<>8__locals1.<>4__this.IsCompleted = true;
					}, delegate
					{
						if (CS$<>8__locals1.equip)
						{
							CS$<>8__locals1.<>4__this.HumanAIController.ReequipUnequipped();
						}
						AIObjectiveMoveItem aiobjectiveMoveItem = CS$<>8__locals1.<>4__this.moveItemObjective;
						if (aiobjectiveMoveItem != null)
						{
							AIObjectiveContainItem containObjective = aiobjectiveMoveItem.ContainObjective;
							if (containObjective != null && containObjective.CanBeCompleted)
							{
								CS$<>8__locals1.<>4__this.ignoredContainers.Add(CS$<>8__locals1.suitableContainer);
								return;
							}
						}
						CS$<>8__locals1.<>4__this.Abandon = true;
					});
				}
				else
				{
					base.Abandon = true;
				}
			}
			this.objectiveManager.GetObjective<AIObjectiveIdle>().Wander(deltaTime);
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x0008AD7C File Offset: 0x00088F7C
		protected override bool CheckObjectiveState()
		{
			if (this.item.IgnoreByAI(this.character) || Item.DeconstructItems.Contains(this.item))
			{
				base.Abandon = true;
			}
			else if (this.item.ParentInventory != null && this.item.GetRootInventoryOwner() != this.character)
			{
				if (!this.objectiveManager.HasOrder<AIObjectiveCleanupItems>(null))
				{
					base.Abandon = true;
				}
				else if (this.item.Container != null && !AIObjectiveCleanupItems.IsValidContainer(this.item.Container, this.character))
				{
					base.Abandon = true;
				}
			}
			return !base.Abandon && base.IsCompleted;
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x0008AE2A File Offset: 0x0008902A
		public override void Reset()
		{
			base.Reset();
			this.ignoredContainers.Clear();
			this.itemIndex = 0;
			this.moveItemObjective = null;
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x0008AE4C File Offset: 0x0008904C
		public void DropTarget()
		{
			if (this.item != null && this.character.HasItem(this.item, false, null))
			{
				this.item.Drop(this.character, true, true);
			}
		}

		// Token: 0x040006AE RID: 1710
		public readonly Item item;

		// Token: 0x040006B0 RID: 1712
		private readonly List<Item> ignoredContainers = new List<Item>();

		// Token: 0x040006B1 RID: 1713
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x040006B2 RID: 1714
		private int itemIndex;
	}
}
