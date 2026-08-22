using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000187 RID: 391
	internal class AIObjectivePrepare : AIObjective
	{
		// Token: 0x17000BD1 RID: 3025
		// (get) Token: 0x06002E36 RID: 11830 RVA: 0x001EEE73 File Offset: 0x001ED073
		// (set) Token: 0x06002E37 RID: 11831 RVA: 0x001EEE7B File Offset: 0x001ED07B
		public override Identifier Identifier { get; set; } = "prepare".ToIdentifier();

		// Token: 0x17000BD2 RID: 3026
		// (get) Token: 0x06002E38 RID: 11832 RVA: 0x001EEE84 File Offset: 0x001ED084
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000BD3 RID: 3027
		// (get) Token: 0x06002E39 RID: 11833 RVA: 0x001EEEAE File Offset: 0x001ED0AE
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BD4 RID: 3028
		// (get) Token: 0x06002E3A RID: 11834 RVA: 0x001EEEB1 File Offset: 0x001ED0B1
		public override bool KeepDivingGearOnAlsoWhenInactive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BD5 RID: 3029
		// (get) Token: 0x06002E3B RID: 11835 RVA: 0x001EEEB4 File Offset: 0x001ED0B4
		public override bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BD6 RID: 3030
		// (get) Token: 0x06002E3C RID: 11836 RVA: 0x001EEEB7 File Offset: 0x001ED0B7
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BD7 RID: 3031
		// (get) Token: 0x06002E3D RID: 11837 RVA: 0x001EEEBA File Offset: 0x001ED0BA
		// (set) Token: 0x06002E3E RID: 11838 RVA: 0x001EEEC2 File Offset: 0x001ED0C2
		public bool KeepActiveWhenReady { get; set; }

		// Token: 0x17000BD8 RID: 3032
		// (get) Token: 0x06002E3F RID: 11839 RVA: 0x001EEECB File Offset: 0x001ED0CB
		// (set) Token: 0x06002E40 RID: 11840 RVA: 0x001EEED3 File Offset: 0x001ED0D3
		public bool CheckInventory { get; set; }

		// Token: 0x17000BD9 RID: 3033
		// (get) Token: 0x06002E41 RID: 11841 RVA: 0x001EEEDC File Offset: 0x001ED0DC
		// (set) Token: 0x06002E42 RID: 11842 RVA: 0x001EEEE4 File Offset: 0x001ED0E4
		public bool FindAllItems { get; set; }

		// Token: 0x17000BDA RID: 3034
		// (get) Token: 0x06002E43 RID: 11843 RVA: 0x001EEEED File Offset: 0x001ED0ED
		// (set) Token: 0x06002E44 RID: 11844 RVA: 0x001EEEF5 File Offset: 0x001ED0F5
		public bool Equip { get; set; }

		// Token: 0x17000BDB RID: 3035
		// (get) Token: 0x06002E45 RID: 11845 RVA: 0x001EEEFE File Offset: 0x001ED0FE
		// (set) Token: 0x06002E46 RID: 11846 RVA: 0x001EEF06 File Offset: 0x001ED106
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x17000BDC RID: 3036
		// (get) Token: 0x06002E47 RID: 11847 RVA: 0x001EEF0F File Offset: 0x001ED10F
		// (set) Token: 0x06002E48 RID: 11848 RVA: 0x001EEF17 File Offset: 0x001ED117
		public bool RequireNonEmpty { get; set; }

		// Token: 0x06002E49 RID: 11849 RVA: 0x001EEF20 File Offset: 0x001ED120
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

		// Token: 0x06002E4A RID: 11850 RVA: 0x001EEF54 File Offset: 0x001ED154
		public AIObjectivePrepare(Character character, AIObjectiveManager objectiveManager, Item targetItem, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetItem = targetItem;
		}

		// Token: 0x06002E4B RID: 11851 RVA: 0x001EEF8C File Offset: 0x001ED18C
		public AIObjectivePrepare(Character character, AIObjectiveManager objectiveManager, IEnumerable<Identifier> optionalItems, IEnumerable<Identifier> requiredItems = null, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.optionalItems = optionalItems.ToImmutableArray<Identifier>();
			if (requiredItems != null)
			{
				this.requiredItems = requiredItems.ToImmutableArray<Identifier>();
			}
		}

		// Token: 0x06002E4C RID: 11852 RVA: 0x001EEFD9 File Offset: 0x001ED1D9
		protected override bool CheckObjectiveState()
		{
			return base.IsCompleted;
		}

		// Token: 0x06002E4D RID: 11853 RVA: 0x001EEFE4 File Offset: 0x001ED1E4
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

		// Token: 0x06002E4E RID: 11854 RVA: 0x001EF03C File Offset: 0x001ED23C
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

		// Token: 0x06002E4F RID: 11855 RVA: 0x001EF177 File Offset: 0x001ED377
		public override void Reset()
		{
			base.Reset();
			this.subObjectivesCreated = false;
			this.getMultipleItemsObjective = null;
			this.getSingleItemObjective = null;
			this.getAllItemsObjective = null;
		}

		// Token: 0x06002E50 RID: 11856 RVA: 0x001EF19C File Offset: 0x001ED39C
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

		// Token: 0x04001824 RID: 6180
		private AIObjectiveGetItem getSingleItemObjective;

		// Token: 0x04001825 RID: 6181
		private AIObjectiveGetItems getAllItemsObjective;

		// Token: 0x04001826 RID: 6182
		private AIObjectiveGetItems getMultipleItemsObjective;

		// Token: 0x04001827 RID: 6183
		private bool subObjectivesCreated;

		// Token: 0x04001828 RID: 6184
		private readonly Item targetItem;

		// Token: 0x04001829 RID: 6185
		private readonly ImmutableArray<Identifier> requiredItems;

		// Token: 0x0400182A RID: 6186
		private readonly ImmutableArray<Identifier> optionalItems;
	}
}
