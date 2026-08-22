using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002B8 RID: 696
	internal readonly struct UpgradeResourceCost
	{
		// Token: 0x06002F8B RID: 12171 RVA: 0x0013C698 File Offset: 0x0013A898
		[NullableContext(1)]
		public UpgradeResourceCost(ContentXElement element)
		{
			this.Amount = element.GetAttributeInt("amount", 0);
			this.targetTags = element.GetAttributeIdentifierArray("item", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			string key = "levels";
			Range<int> range = new Range<int>(0, 99);
			this.TargetLevels = element.GetAttributeRange(key, range);
		}

		// Token: 0x06002F8C RID: 12172 RVA: 0x0013C6F4 File Offset: 0x0013A8F4
		public bool AppliesForLevel(int currentLevel)
		{
			return this.TargetLevels.Contains(currentLevel);
		}

		// Token: 0x06002F8D RID: 12173 RVA: 0x0013C704 File Offset: 0x0013A904
		public bool AppliesForLevel(Range<int> newLevels)
		{
			return newLevels.Start <= this.TargetLevels.End && newLevels.End >= this.TargetLevels.Start;
		}

		// Token: 0x06002F8E RID: 12174 RVA: 0x0013C744 File Offset: 0x0013A944
		[NullableContext(1)]
		public bool MatchesItem(Item item)
		{
			return this.MatchesItem(item.Prefab);
		}

		// Token: 0x06002F8F RID: 12175 RVA: 0x0013C754 File Offset: 0x0013A954
		[NullableContext(1)]
		public bool MatchesItem(ItemPrefab item)
		{
			foreach (Identifier tag in this.targetTags)
			{
				if (tag.Equals(item.Identifier) || item.Tags.Contains(tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040017DE RID: 6110
		public readonly int Amount;

		// Token: 0x040017DF RID: 6111
		private readonly ImmutableArray<Identifier> targetTags;

		// Token: 0x040017E0 RID: 6112
		public readonly Range<int> TargetLevels;
	}
}
