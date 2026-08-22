using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000385 RID: 901
	internal readonly struct ApplicableResourceCollection
	{
		// Token: 0x06004428 RID: 17448 RVA: 0x00256CE4 File Offset: 0x00254EE4
		[NullableContext(1)]
		public ApplicableResourceCollection(IEnumerable<ItemPrefab> matchingItems, int count, UpgradeResourceCost cost)
		{
			this.MatchingItems = matchingItems.ToImmutableArray<ItemPrefab>();
			this.Count = count;
			this.Cost = cost;
		}

		// Token: 0x06004429 RID: 17449 RVA: 0x00256D00 File Offset: 0x00254F00
		public static ApplicableResourceCollection CreateFor(UpgradeResourceCost cost)
		{
			return new ApplicableResourceCollection(ItemPrefab.Prefabs.Where(new Func<ItemPrefab, bool>(cost.MatchesItem)), cost.Amount, cost);
		}

		// Token: 0x040023C4 RID: 9156
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<ItemPrefab> MatchingItems;

		// Token: 0x040023C5 RID: 9157
		public readonly UpgradeResourceCost Cost;

		// Token: 0x040023C6 RID: 9158
		public readonly int Count;
	}
}
