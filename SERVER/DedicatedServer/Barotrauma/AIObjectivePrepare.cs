using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000081 RID: 129
	internal class AIObjectivePrepare : AIObjective
	{
		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x0600112E RID: 4398 RVA: 0x0009BFDB File Offset: 0x0009A1DB
		// (set) Token: 0x0600112F RID: 4399 RVA: 0x0009BFE3 File Offset: 0x0009A1E3
		public override Identifier Identifier { get; set; } = "prepare".ToIdentifier();

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06001130 RID: 4400 RVA: 0x0009BFEC File Offset: 0x0009A1EC
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06001131 RID: 4401 RVA: 0x0009C016 File Offset: 0x0009A216
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x06001132 RID: 4402 RVA: 0x0009C019 File Offset: 0x0009A219
		public override bool KeepDivingGearOnAlsoWhenInactive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x06001133 RID: 4403 RVA: 0x0009C01C File Offset: 0x0009A21C
		public override bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06001134 RID: 4404 RVA: 0x0009C01F File Offset: 0x0009A21F
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06001135 RID: 4405 RVA: 0x0009C022 File Offset: 0x0009A222
		// (set) Token: 0x06001136 RID: 4406 RVA: 0x0009C02A File Offset: 0x0009A22A
		public bool KeepActiveWhenReady { get; set; }

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06001137 RID: 4407 RVA: 0x0009C033 File Offset: 0x0009A233
		// (set) Token: 0x06001138 RID: 4408 RVA: 0x0009C03B File Offset: 0x0009A23B
		public bool CheckInventory { get; set; }

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06001139 RID: 4409 RVA: 0x0009C044 File Offset: 0x0009A244
		// (set) Token: 0x0600113A RID: 4410 RVA: 0x0009C04C File Offset: 0x0009A24C
		public bool FindAllItems { get; set; }

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x0600113B RID: 4411 RVA: 0x0009C055 File Offset: 0x0009A255
		// (set) Token: 0x0600113C RID: 4412 RVA: 0x0009C05D File Offset: 0x0009A25D
		public bool Equip { get; set; }

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x0600113D RID: 4413 RVA: 0x0009C066 File Offset: 0x0009A266
		// (set) Token: 0x0600113E RID: 4414 RVA: 0x0009C06E File Offset: 0x0009A26E
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x0600113F RID: 4415 RVA: 0x0009C077 File Offset: 0x0009A277
		// (set) Token: 0x06001140 RID: 4416 RVA: 0x0009C07F File Offset: 0x0009A27F
		public bool RequireNonEmpty { get; set; }

		// Token: 0x06001141 RID: 4417 RVA: 0x0009C088 File Offset: 0x0009A288
		private AIObjective GetSubObjective()
		{
			if (this.getSingleItemObjective != null)
			{
				return this.getSingleItemObjective;
			}
			if (this.getAllItemsObjective == null || this.getAllItemsObjective.IsCompleted)
			{
				return this.getMultipleItemsObjective;
			}
			return this.getAllItemsObjective;
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x0009C0BC File Offset: 0x0009A2BC
		public AIObjectivePrepare(Character character, AIObjectiveManager objectiveManager, Item targetItem, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetItem = targetItem;
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x0009C0F4 File Offset: 0x0009A2F4
		public AIObjectivePrepare(Character character, AIObjectiveManager objectiveManager, IEnumerable<Identifier> optionalItems, IEnumerable<Identifier> requiredItems = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.optionalItems = optionalItems.ToImmutableArray<Identifier>();
			if (requiredItems != null)
			{
				this.requiredItems = requiredItems.ToImmutableArray<Identifier>();
			}
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x0009C141 File Offset: 0x0009A341
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x0009C14C File Offset: 0x0009A34C
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			base.Priority = this.objectiveManager.GetOrderPriority(this);
			AIObjective subObjective = this.GetSubObjective();
			if (subObjective != null && subObjective.IsCompleted)
			{
				base.Priority = 0f;
			}
			return base.Priority;
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x0009C1A4 File Offset: 0x0009A3A4
		protected override void Act(float deltaTime)
		{
			if (!this.subObjectivesCreated)
			{
				if (this.FindAllItems && this.targetItem == null)
				{
					this.getMultipleItemsObjective = this.<Act>g__CreateObjectives|50_0(this.optionalItems, false);
					if (new ImmutableArray<Identifier>?(this.requiredItems) != null && this.requiredItems.Any<Identifier>())
					{
						this.getAllItemsObjective = this.<Act>g__CreateObjectives|50_0(this.requiredItems, true);
					}
				}
				else
				{
					Func<AIObjectiveGetItem> getItemConstructor;
					if (this.targetItem != null)
					{
						getItemConstructor = (() => new AIObjectiveGetItem(this.character, this.targetItem, this.objectiveManager, this.Equip, 1f)
						{
							SpeakIfFails = true
						});
					}
					else
					{
						IEnumerable<Identifier> allItems = this.optionalItems;
						if (new ImmutableArray<Identifier>?(this.requiredItems) != null && this.requiredItems.Any<Identifier>())
						{
							allItems = this.requiredItems;
						}
						getItemConstructor = (() => new AIObjectiveGetItem(this.character, allItems, this.objectiveManager, this.Equip, this.CheckInventory, 1f, false)
						{
							EvaluateCombatPriority = this.EvaluateCombatPriority,
							SpeakIfFails = true,
							RequireNonEmpty = this.RequireNonEmpty
						});
					}
					if (!base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getSingleItemObjective, getItemConstructor, delegate
					{
						if (!this.KeepActiveWhenReady)
						{
							base.IsCompleted = true;
						}
					}, delegate
					{
						base.Abandon = true;
					}))
					{
						base.Abandon = true;
					}
				}
				this.subObjectivesCreated = true;
			}
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x0009C2DF File Offset: 0x0009A4DF
		public override void Reset()
		{
			base.Reset();
			this.subObjectivesCreated = false;
			this.getMultipleItemsObjective = null;
			this.getSingleItemObjective = null;
			this.getAllItemsObjective = null;
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x0009C304 File Offset: 0x0009A504
		[CompilerGenerated]
		private AIObjectiveGetItems <Act>g__CreateObjectives|50_0(IEnumerable<Identifier> itemTags, bool requireAll)
		{
			AIObjectiveGetItems objectiveReference = null;
			if (!base.TryAddSubObjective<AIObjectiveGetItems>(ref objectiveReference, delegate
			{
				AIObjectiveGetItems getItems = new AIObjectiveGetItems(this.character, this.objectiveManager, itemTags, 1f)
				{
					CheckInventory = this.CheckInventory,
					Equip = this.Equip,
					EvaluateCombatPriority = this.EvaluateCombatPriority,
					RequireNonEmpty = this.RequireNonEmpty,
					RequireAllItems = requireAll
				};
				if (itemTags.Contains(Tags.HeavyDivingGear))
				{
					getItems.ItemFilter = ((Item it, Identifier tag) => !(tag == Tags.HeavyDivingGear) || AIObjectiveFindDivingGear.IsSuitablePressureProtection(it, tag, this.character));
				}
				return getItems;
			}, delegate
			{
				if (!this.KeepActiveWhenReady)
				{
					base.IsCompleted = true;
				}
			}, delegate
			{
				base.Abandon = true;
			}))
			{
				base.Abandon = true;
			}
			return objectiveReference;
		}

		// Token: 0x0400082A RID: 2090
		private AIObjectiveGetItem getSingleItemObjective;

		// Token: 0x0400082B RID: 2091
		private AIObjectiveGetItems getAllItemsObjective;

		// Token: 0x0400082C RID: 2092
		private AIObjectiveGetItems getMultipleItemsObjective;

		// Token: 0x0400082D RID: 2093
		private bool subObjectivesCreated;

		// Token: 0x0400082E RID: 2094
		private readonly Item targetItem;

		// Token: 0x0400082F RID: 2095
		private readonly ImmutableArray<Identifier> requiredItems;

		// Token: 0x04000830 RID: 2096
		private readonly ImmutableArray<Identifier> optionalItems;
	}
}
