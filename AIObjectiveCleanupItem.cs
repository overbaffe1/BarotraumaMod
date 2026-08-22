using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016D RID: 365
	internal class AIObjectiveCleanupItem : AIObjective
	{
		// Token: 0x17000AE1 RID: 2785
		// (get) Token: 0x06002B4F RID: 11087 RVA: 0x001DD962 File Offset: 0x001DBB62
		// (set) Token: 0x06002B50 RID: 11088 RVA: 0x001DD96A File Offset: 0x001DBB6A
		public override Identifier Identifier { get; set; } = "cleanup item".ToIdentifier();

		// Token: 0x17000AE2 RID: 2786
		// (get) Token: 0x06002B51 RID: 11089 RVA: 0x001DD973 File Offset: 0x001DBB73
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000AE3 RID: 2787
		// (get) Token: 0x06002B52 RID: 11090 RVA: 0x001DD976 File Offset: 0x001DBB76
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AE4 RID: 2788
		// (get) Token: 0x06002B53 RID: 11091 RVA: 0x001DD979 File Offset: 0x001DBB79
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AE5 RID: 2789
		// (get) Token: 0x06002B54 RID: 11092 RVA: 0x001DD97C File Offset: 0x001DBB7C
		// (set) Token: 0x06002B55 RID: 11093 RVA: 0x001DD984 File Offset: 0x001DBB84
		public bool IsPriority { get; set; }

		// Token: 0x17000AE6 RID: 2790
		// (get) Token: 0x06002B56 RID: 11094 RVA: 0x001DD98D File Offset: 0x001DBB8D
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002B57 RID: 11095 RVA: 0x001DD990 File Offset: 0x001DBB90
		public AIObjectiveCleanupItem(Item item, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.item = item;
		}

		// Token: 0x06002B58 RID: 11096 RVA: 0x001DD9D4 File Offset: 0x001DBBD4
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

		// Token: 0x06002B59 RID: 11097 RVA: 0x001DDAF0 File Offset: 0x001DBCF0
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

		// Token: 0x06002B5A RID: 11098 RVA: 0x001DDC10 File Offset: 0x001DBE10
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

		// Token: 0x06002B5B RID: 11099 RVA: 0x001DDCBE File Offset: 0x001DBEBE
		public override void Reset()
		{
			base.Reset();
			this.ignoredContainers.Clear();
			this.itemIndex = 0;
			this.moveItemObjective = null;
		}

		// Token: 0x06002B5C RID: 11100 RVA: 0x001DDCE0 File Offset: 0x001DBEE0
		public void DropTarget()
		{
			if (this.item != null && this.character.HasItem(this.item, false, null))
			{
				this.item.Drop(this.character, true, true);
			}
		}

		// Token: 0x040016A8 RID: 5800
		public readonly Item item;

		// Token: 0x040016AA RID: 5802
		private readonly List<Item> ignoredContainers = new List<Item>();

		// Token: 0x040016AB RID: 5803
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x040016AC RID: 5804
		private int itemIndex;
	}
}
