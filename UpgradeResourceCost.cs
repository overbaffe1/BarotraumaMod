using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000384 RID: 900
	internal readonly struct UpgradeResourceCost
	{
		// Token: 0x06004423 RID: 17443 RVA: 0x00256BDC File Offset: 0x00254DDC
		[NullableContext(1)]
		public UpgradeResourceCost(ContentXElement element)
		{
			this.Amount = element.GetAttributeInt("amount", 0);
			this.targetTags = element.GetAttributeIdentifierArray("item", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			string key = "levels";
			Range<int> range = new Range<int>(0, 99);
			this.TargetLevels = element.GetAttributeRange(key, range);
		}

		// Token: 0x06004424 RID: 17444 RVA: 0x00256C38 File Offset: 0x00254E38
		public bool AppliesForLevel(int currentLevel)
		{
			return this.TargetLevels.Contains(currentLevel);
		}

		// Token: 0x06004425 RID: 17445 RVA: 0x00256C48 File Offset: 0x00254E48
		public bool AppliesForLevel(Range<int> newLevels)
		{
			return newLevels.Start <= this.TargetLevels.End && newLevels.End >= this.TargetLevels.Start;
		}

		// Token: 0x06004426 RID: 17446 RVA: 0x00256C88 File Offset: 0x00254E88
		[NullableContext(1)]
		public bool MatchesItem(Item item)
		{
			return this.MatchesItem(item.Prefab);
		}

		// Token: 0x06004427 RID: 17447 RVA: 0x00256C98 File Offset: 0x00254E98
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

		// Token: 0x040023C1 RID: 9153
		public readonly int Amount;

		// Token: 0x040023C2 RID: 9154
		private readonly ImmutableArray<Identifier> targetTags;

		// Token: 0x040023C3 RID: 9155
		public readonly Range<int> TargetLevels;
	}
}
