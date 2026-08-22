using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200017D RID: 381
	[NullableContext(1)]
	[Nullable(0)]
	internal class AIObjectiveGetItems : AIObjective
	{
		// Token: 0x17000B64 RID: 2916
		// (get) Token: 0x06002CFE RID: 11518 RVA: 0x001E78ED File Offset: 0x001E5AED
		// (set) Token: 0x06002CFF RID: 11519 RVA: 0x001E78F5 File Offset: 0x001E5AF5
		public override Identifier Identifier { get; set; } = "get items".ToIdentifier();

		// Token: 0x17000B65 RID: 2917
		// (get) Token: 0x06002D00 RID: 11520 RVA: 0x001E7900 File Offset: 0x001E5B00
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000B66 RID: 2918
		// (get) Token: 0x06002D01 RID: 11521 RVA: 0x001E792A File Offset: 0x001E5B2A
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B67 RID: 2919
		// (get) Token: 0x06002D02 RID: 11522 RVA: 0x001E792D File Offset: 0x001E5B2D
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B68 RID: 2920
		// (get) Token: 0x06002D03 RID: 11523 RVA: 0x001E7930 File Offset: 0x001E5B30
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B69 RID: 2921
		// (get) Token: 0x06002D04 RID: 11524 RVA: 0x001E7933 File Offset: 0x001E5B33
		// (set) Token: 0x06002D05 RID: 11525 RVA: 0x001E793B File Offset: 0x001E5B3B
		public bool AllowStealing { get; set; }

		// Token: 0x17000B6A RID: 2922
		// (get) Token: 0x06002D06 RID: 11526 RVA: 0x001E7944 File Offset: 0x001E5B44
		// (set) Token: 0x06002D07 RID: 11527 RVA: 0x001E794C File Offset: 0x001E5B4C
		public bool TakeWholeStack { get; set; }

		// Token: 0x17000B6B RID: 2923
		// (get) Token: 0x06002D08 RID: 11528 RVA: 0x001E7955 File Offset: 0x001E5B55
		// (set) Token: 0x06002D09 RID: 11529 RVA: 0x001E795D File Offset: 0x001E5B5D
		public bool AllowVariants { get; set; }

		// Token: 0x17000B6C RID: 2924
		// (get) Token: 0x06002D0A RID: 11530 RVA: 0x001E7966 File Offset: 0x001E5B66
		// (set) Token: 0x06002D0B RID: 11531 RVA: 0x001E796E File Offset: 0x001E5B6E
		public bool Equip { get; set; }

		// Token: 0x17000B6D RID: 2925
		// (get) Token: 0x06002D0C RID: 11532 RVA: 0x001E7977 File Offset: 0x001E5B77
		// (set) Token: 0x06002D0D RID: 11533 RVA: 0x001E797F File Offset: 0x001E5B7F
		public bool Wear { get; set; }

		// Token: 0x17000B6E RID: 2926
		// (get) Token: 0x06002D0E RID: 11534 RVA: 0x001E7988 File Offset: 0x001E5B88
		// (set) Token: 0x06002D0F RID: 11535 RVA: 0x001E7990 File Offset: 0x001E5B90
		public bool CheckInventory { get; set; }

		// Token: 0x17000B6F RID: 2927
		// (get) Token: 0x06002D10 RID: 11536 RVA: 0x001E7999 File Offset: 0x001E5B99
		// (set) Token: 0x06002D11 RID: 11537 RVA: 0x001E79A1 File Offset: 0x001E5BA1
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x17000B70 RID: 2928
		// (get) Token: 0x06002D12 RID: 11538 RVA: 0x001E79AA File Offset: 0x001E5BAA
		// (set) Token: 0x06002D13 RID: 11539 RVA: 0x001E79B2 File Offset: 0x001E5BB2
		public bool CheckPathForEachItem { get; set; }

		// Token: 0x17000B71 RID: 2929
		// (get) Token: 0x06002D14 RID: 11540 RVA: 0x001E79BB File Offset: 0x001E5BBB
		// (set) Token: 0x06002D15 RID: 11541 RVA: 0x001E79C3 File Offset: 0x001E5BC3
		public bool RequireNonEmpty { get; set; }

		// Token: 0x17000B72 RID: 2930
		// (get) Token: 0x06002D16 RID: 11542 RVA: 0x001E79CC File Offset: 0x001E5BCC
		// (set) Token: 0x06002D17 RID: 11543 RVA: 0x001E79D4 File Offset: 0x001E5BD4
		public bool RequireAllItems { get; set; }

		// Token: 0x17000B73 RID: 2931
		// (get) Token: 0x06002D18 RID: 11544 RVA: 0x001E79DD File Offset: 0x001E5BDD
		// (set) Token: 0x06002D19 RID: 11545 RVA: 0x001E79E5 File Offset: 0x001E5BE5
		public bool RequireDivingSuitAdequate { get; set; }

		// Token: 0x06002D1A RID: 11546 RVA: 0x001E79F0 File Offset: 0x001E5BF0
		public AIObjectiveGetItems(Character character, AIObjectiveManager objectiveManager, IEnumerable<Identifier> identifiersOrTags, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.gearTags = AIObjectiveGetItem.ParseGearTags(identifiersOrTags).ToImmutableArray<Identifier>();
			this.ignoredTags = AIObjectiveGetItem.ParseIgnoredTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06002D1B RID: 11547 RVA: 0x001E7A4D File Offset: 0x001E5C4D
		protected override bool CheckObjectiveState()
		{
			return this.subObjectivesCreated && this.subObjectives.None(null);
		}

		// Token: 0x06002D1C RID: 11548 RVA: 0x001E7A68 File Offset: 0x001E5C68
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

		// Token: 0x06002D1D RID: 11549 RVA: 0x001E7B46 File Offset: 0x001E5D46
		public override void Reset()
		{
			base.Reset();
			this.subObjectivesCreated = false;
			this.achievedItems.Clear();
		}

		// Token: 0x04001784 RID: 6020
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Func<Item, Identifier, bool> ItemFilter;

		// Token: 0x04001785 RID: 6021
		[Nullable(0)]
		private readonly ImmutableArray<Identifier> gearTags;

		// Token: 0x04001786 RID: 6022
		private readonly ImmutableHashSet<Identifier> ignoredTags;

		// Token: 0x04001787 RID: 6023
		private bool subObjectivesCreated;

		// Token: 0x04001788 RID: 6024
		public readonly HashSet<Item> achievedItems = new HashSet<Item>();
	}
}
