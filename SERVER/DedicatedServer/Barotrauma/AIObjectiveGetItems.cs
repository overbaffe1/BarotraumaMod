using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000077 RID: 119
	[NullableContext(1)]
	[Nullable(0)]
	internal class AIObjectiveGetItems : AIObjective
	{
		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06000FF6 RID: 4086 RVA: 0x00094A59 File Offset: 0x00092C59
		// (set) Token: 0x06000FF7 RID: 4087 RVA: 0x00094A61 File Offset: 0x00092C61
		public override Identifier Identifier { get; set; } = "get items".ToIdentifier();

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06000FF8 RID: 4088 RVA: 0x00094A6C File Offset: 0x00092C6C
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06000FF9 RID: 4089 RVA: 0x00094A96 File Offset: 0x00092C96
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06000FFA RID: 4090 RVA: 0x00094A99 File Offset: 0x00092C99
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06000FFB RID: 4091 RVA: 0x00094A9C File Offset: 0x00092C9C
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06000FFC RID: 4092 RVA: 0x00094A9F File Offset: 0x00092C9F
		// (set) Token: 0x06000FFD RID: 4093 RVA: 0x00094AA7 File Offset: 0x00092CA7
		public bool AllowStealing { get; set; }

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06000FFE RID: 4094 RVA: 0x00094AB0 File Offset: 0x00092CB0
		// (set) Token: 0x06000FFF RID: 4095 RVA: 0x00094AB8 File Offset: 0x00092CB8
		public bool TakeWholeStack { get; set; }

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06001000 RID: 4096 RVA: 0x00094AC1 File Offset: 0x00092CC1
		// (set) Token: 0x06001001 RID: 4097 RVA: 0x00094AC9 File Offset: 0x00092CC9
		public bool AllowVariants { get; set; }

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06001002 RID: 4098 RVA: 0x00094AD2 File Offset: 0x00092CD2
		// (set) Token: 0x06001003 RID: 4099 RVA: 0x00094ADA File Offset: 0x00092CDA
		public bool Equip { get; set; }

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06001004 RID: 4100 RVA: 0x00094AE3 File Offset: 0x00092CE3
		// (set) Token: 0x06001005 RID: 4101 RVA: 0x00094AEB File Offset: 0x00092CEB
		public bool Wear { get; set; }

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06001006 RID: 4102 RVA: 0x00094AF4 File Offset: 0x00092CF4
		// (set) Token: 0x06001007 RID: 4103 RVA: 0x00094AFC File Offset: 0x00092CFC
		public bool CheckInventory { get; set; }

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06001008 RID: 4104 RVA: 0x00094B05 File Offset: 0x00092D05
		// (set) Token: 0x06001009 RID: 4105 RVA: 0x00094B0D File Offset: 0x00092D0D
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x0600100A RID: 4106 RVA: 0x00094B16 File Offset: 0x00092D16
		// (set) Token: 0x0600100B RID: 4107 RVA: 0x00094B1E File Offset: 0x00092D1E
		public bool CheckPathForEachItem { get; set; }

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x0600100C RID: 4108 RVA: 0x00094B27 File Offset: 0x00092D27
		// (set) Token: 0x0600100D RID: 4109 RVA: 0x00094B2F File Offset: 0x00092D2F
		public bool RequireNonEmpty { get; set; }

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x0600100E RID: 4110 RVA: 0x00094B38 File Offset: 0x00092D38
		// (set) Token: 0x0600100F RID: 4111 RVA: 0x00094B40 File Offset: 0x00092D40
		public bool RequireAllItems { get; set; }

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06001010 RID: 4112 RVA: 0x00094B49 File Offset: 0x00092D49
		// (set) Token: 0x06001011 RID: 4113 RVA: 0x00094B51 File Offset: 0x00092D51
		public bool RequireDivingSuitAdequate { get; set; }

		// Token: 0x06001012 RID: 4114 RVA: 0x00094B5C File Offset: 0x00092D5C
		public AIObjectiveGetItems(Character character, AIObjectiveManager objectiveManager, IEnumerable<Identifier> identifiersOrTags, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.gearTags = AIObjectiveGetItem.ParseGearTags(identifiersOrTags).ToImmutableArray<Identifier>();
			this.ignoredTags = AIObjectiveGetItem.ParseIgnoredTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x00094BB9 File Offset: 0x00092DB9
		protected override bool CheckObjectiveState()
		{
			return this.subObjectivesCreated && this.subObjectives.None(null);
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x00094BD4 File Offset: 0x00092DD4
		protected override void Act(float deltaTime)
		{
			if (this.subObjectivesCreated)
			{
				return;
			}
			ImmutableArray<Identifier>.Enumerator enumerator = this.gearTags.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AIObjectiveGetItems.<>c__DisplayClass63_0 CS$<>8__locals1 = new AIObjectiveGetItems.<>c__DisplayClass63_0();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.tag = enumerator.Current;
				AIObjectiveGetItem getItem;
				if (!this.subObjectives.Any(delegate(AIObjective so)
				{
					AIObjectiveGetItem getItem = so as AIObjectiveGetItem;
					return getItem != null && getItem.IdentifiersOrTags.Contains(CS$<>8__locals1.tag);
				}))
				{
					int count = this.gearTags.Count((Identifier t) => t == CS$<>8__locals1.tag);
					getItem = null;
					base.TryAddSubObjective<AIObjectiveGetItem>(ref getItem, delegate
					{
						AIObjectiveGetItem getItem = new AIObjectiveGetItem(CS$<>8__locals1.<>4__this.character, CS$<>8__locals1.tag, CS$<>8__locals1.<>4__this.objectiveManager, CS$<>8__locals1.<>4__this.Equip, CS$<>8__locals1.<>4__this.CheckInventory && count <= 1, 1f, false)
						{
							AllowVariants = CS$<>8__locals1.<>4__this.AllowVariants,
							Wear = CS$<>8__locals1.<>4__this.Wear,
							TakeWholeStack = CS$<>8__locals1.<>4__this.TakeWholeStack,
							AllowStealing = CS$<>8__locals1.<>4__this.AllowStealing,
							ignoredIdentifiersOrTags = CS$<>8__locals1.<>4__this.ignoredTags,
							CheckPathForEachItem = CS$<>8__locals1.<>4__this.CheckPathForEachItem,
							RequireNonEmpty = CS$<>8__locals1.<>4__this.RequireNonEmpty,
							ItemCount = count,
							SpeakIfFails = CS$<>8__locals1.<>4__this.RequireAllItems
						};
						if (CS$<>8__locals1.<>4__this.ItemFilter != null)
						{
							getItem = getItem;
							Func<Item, bool> itemFilter;
							if ((itemFilter = CS$<>8__locals1.<>9__5) == null)
							{
								itemFilter = (CS$<>8__locals1.<>9__5 = ((Item it) => CS$<>8__locals1.<>4__this.ItemFilter(it, CS$<>8__locals1.tag)));
							}
							getItem.ItemFilter = itemFilter;
						}
						return getItem;
					}, delegate
					{
						AIObjectiveGetItem getItem = getItem;
						Item item = (getItem != null) ? getItem.TargetItem : null;
						if (item != null)
						{
							item.IsOwnedBy(CS$<>8__locals1.<>4__this.character);
							CS$<>8__locals1.<>4__this.achievedItems.Add(item);
						}
					}, delegate
					{
						AIObjectiveGetItem getItem = getItem;
						Item item = (getItem != null) ? getItem.TargetItem : null;
						if (item != null)
						{
							CS$<>8__locals1.<>4__this.achievedItems.Remove(item);
						}
						CS$<>8__locals1.<>4__this.RemoveSubObjective<AIObjectiveGetItem>(ref getItem);
						if (CS$<>8__locals1.<>4__this.RequireAllItems)
						{
							CS$<>8__locals1.<>4__this.Abandon = true;
						}
					});
				}
			}
			this.subObjectivesCreated = true;
		}

		// Token: 0x06001015 RID: 4117 RVA: 0x00094CB2 File Offset: 0x00092EB2
		public override void Reset()
		{
			base.Reset();
			this.subObjectivesCreated = false;
			this.achievedItems.Clear();
		}

		// Token: 0x0400078A RID: 1930
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Func<Item, Identifier, bool> ItemFilter;

		// Token: 0x0400078B RID: 1931
		[Nullable(0)]
		private readonly ImmutableArray<Identifier> gearTags;

		// Token: 0x0400078C RID: 1932
		private readonly ImmutableHashSet<Identifier> ignoredTags;

		// Token: 0x0400078D RID: 1933
		private bool subObjectivesCreated;

		// Token: 0x0400078E RID: 1934
		public readonly HashSet<Item> achievedItems = new HashSet<Item>();
	}
}
