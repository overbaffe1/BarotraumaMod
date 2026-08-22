using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200006B RID: 107
	internal class AIObjectiveDeconstructItem : AIObjective
	{
		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000EEF RID: 3823 RVA: 0x0008F21C File Offset: 0x0008D41C
		// (set) Token: 0x06000EF0 RID: 3824 RVA: 0x0008F224 File Offset: 0x0008D424
		public override Identifier Identifier { get; set; } = "deconstruct item".ToIdentifier();

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000EF1 RID: 3825 RVA: 0x0008F22D File Offset: 0x0008D42D
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06000EF2 RID: 3826 RVA: 0x0008F230 File Offset: 0x0008D430
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x0008F234 File Offset: 0x0008D434
		public AIObjectiveDeconstructItem(Item item, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Item = item;
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x0008F26C File Offset: 0x0008D46C
		protected override void Act(float deltaTime)
		{
			if (this.subObjectives.Any<AIObjective>())
			{
				return;
			}
			if (this.deconstructor == null)
			{
				this.deconstructor = this.FindDeconstructor();
				if (this.deconstructor == null)
				{
					base.Abandon = true;
					return;
				}
			}
			base.TryAddSubObjective<AIObjectiveMoveItem>(ref this.moveItemObjective, delegate
			{
				Character character = this.character;
				Item item = this.Item;
				AIObjectiveManager objectiveManager = this.objectiveManager;
				Item container = this.Item.Container;
				return new AIObjectiveMoveItem(character, item, objectiveManager, (container != null) ? container.GetComponent<ItemContainer>() : null, this.deconstructor.InputContainer, base.PriorityModifier)
				{
					Equip = true,
					RemoveExistingWhenNecessary = true
				};
			}, delegate
			{
				if (this.character.CanInteractWith(this.deconstructor.Item, true))
				{
					this.StartDeconstruction();
				}
				else
				{
					base.TryAddSubObjective<AIObjectiveGoTo>(ref this.gotoObjective, () => new AIObjectiveGoTo(this.Item, this.character, this.objectiveManager, false, true, base.PriorityModifier, 0f), delegate
					{
						this.StartDeconstruction();
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.gotoObjective);
					}, delegate
					{
						base.Abandon = true;
					});
				}
				base.RemoveSubObjective<AIObjectiveMoveItem>(ref this.moveItemObjective);
			}, delegate
			{
				base.Abandon = true;
			});
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x0008F2DC File Offset: 0x0008D4DC
		private void StartDeconstruction()
		{
			this.StartDeconstructor();
			Submarine submarine = this.deconstructor.Item.Submarine;
			if (submarine != null)
			{
				SubmarineInfo info = submarine.Info;
				if (info != null && info.IsOutpost)
				{
					base.HumanAIController.HandleRelocation(this.Item);
					this.deconstructor.RelocateOutputToMainSub = true;
				}
			}
			base.IsCompleted = true;
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x0008F33C File Offset: 0x0008D53C
		private Deconstructor FindDeconstructor()
		{
			Deconstructor closestDeconstructor = null;
			float bestDistFactor = 0f;
			using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item otherItem = enumerator.Current;
					Deconstructor potentialDeconstructor = otherItem.GetComponent<Deconstructor>();
					if (((potentialDeconstructor != null) ? potentialDeconstructor.InputContainer : null) != null && potentialDeconstructor.InputContainer.Inventory.CanBePut(this.Item) && potentialDeconstructor.Item.HasAccess(this.character) && (!this.Item.Prefab.DeconstructItems.Any<DeconstructItem>() || !this.Item.Prefab.DeconstructItems.None((DeconstructItem it) => it.IsValidDeconstructor(otherItem))))
					{
						float distFactor = AIObjective.GetDistanceFactor(this.Item.WorldPosition, potentialDeconstructor.Item.WorldPosition, 0.2f, 3f, 10000f, 1f);
						if (distFactor > bestDistFactor)
						{
							closestDeconstructor = potentialDeconstructor;
							bestDistFactor = distFactor;
						}
					}
				}
			}
			return closestDeconstructor;
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x0008F470 File Offset: 0x0008D670
		private void StartDeconstructor()
		{
			this.deconstructor.SetActive(true, this.character, true);
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x0008F488 File Offset: 0x0008D688
		protected override bool CheckObjectiveState()
		{
			if (this.Item.IgnoreByAI(this.character))
			{
				base.Abandon = true;
			}
			else if (this.deconstructor != null && this.deconstructor.Item.IgnoreByAI(this.character))
			{
				base.Abandon = true;
			}
			return !base.Abandon && base.IsCompleted;
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x0008F4E8 File Offset: 0x0008D6E8
		public override void Reset()
		{
			base.Reset();
			this.moveItemObjective = null;
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x0008F4F8 File Offset: 0x0008D6F8
		public void DropTarget()
		{
			if (this.Item != null && this.character.HasItem(this.Item, false, null))
			{
				this.Item.Drop(this.character, true, true);
			}
		}

		// Token: 0x0400070A RID: 1802
		public readonly Item Item;

		// Token: 0x0400070B RID: 1803
		private Deconstructor deconstructor;

		// Token: 0x0400070C RID: 1804
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x0400070D RID: 1805
		private AIObjectiveGoTo gotoObjective;
	}
}
