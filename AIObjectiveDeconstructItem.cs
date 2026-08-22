using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000171 RID: 369
	internal class AIObjectiveDeconstructItem : AIObjective
	{
		// Token: 0x17000B10 RID: 2832
		// (get) Token: 0x06002BF7 RID: 11255 RVA: 0x001E20B0 File Offset: 0x001E02B0
		// (set) Token: 0x06002BF8 RID: 11256 RVA: 0x001E20B8 File Offset: 0x001E02B8
		public override Identifier Identifier { get; set; } = "deconstruct item".ToIdentifier();

		// Token: 0x17000B11 RID: 2833
		// (get) Token: 0x06002BF9 RID: 11257 RVA: 0x001E20C1 File Offset: 0x001E02C1
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B12 RID: 2834
		// (get) Token: 0x06002BFA RID: 11258 RVA: 0x001E20C4 File Offset: 0x001E02C4
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002BFB RID: 11259 RVA: 0x001E20C8 File Offset: 0x001E02C8
		public AIObjectiveDeconstructItem(Item item, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Item = item;
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x001E2100 File Offset: 0x001E0300
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

		// Token: 0x06002BFD RID: 11261 RVA: 0x001E2170 File Offset: 0x001E0370
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

		// Token: 0x06002BFE RID: 11262 RVA: 0x001E21D0 File Offset: 0x001E03D0
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

		// Token: 0x06002BFF RID: 11263 RVA: 0x001E2304 File Offset: 0x001E0504
		private void StartDeconstructor()
		{
			this.deconstructor.SetActive(true, this.character, true);
		}

		// Token: 0x06002C00 RID: 11264 RVA: 0x001E231C File Offset: 0x001E051C
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

		// Token: 0x06002C01 RID: 11265 RVA: 0x001E237C File Offset: 0x001E057C
		public override void Reset()
		{
			base.Reset();
			this.moveItemObjective = null;
		}

		// Token: 0x06002C02 RID: 11266 RVA: 0x001E238C File Offset: 0x001E058C
		public void DropTarget()
		{
			if (this.Item != null && this.character.HasItem(this.Item, false, null))
			{
				this.Item.Drop(this.character, true, true);
			}
		}

		// Token: 0x04001704 RID: 5892
		public readonly Item Item;

		// Token: 0x04001705 RID: 5893
		private Deconstructor deconstructor;

		// Token: 0x04001706 RID: 5894
		private AIObjectiveMoveItem moveItemObjective;

		// Token: 0x04001707 RID: 5895
		private AIObjectiveGoTo gotoObjective;
	}
}
