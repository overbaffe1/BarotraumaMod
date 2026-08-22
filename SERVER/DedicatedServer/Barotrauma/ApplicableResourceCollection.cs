using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002B9 RID: 697
	internal readonly struct ApplicableResourceCollection
	{
		// Token: 0x06002F90 RID: 12176 RVA: 0x0013C7A0 File Offset: 0x0013A9A0
		[NullableContext(1)]
		public ApplicableResourceCollection(IEnumerable<ItemPrefab> matchingItems, int count, UpgradeResourceCost cost)
		{
			this.MatchingItems = matchingItems.ToImmutableArray<ItemPrefab>();
			this.Count = count;
			this.Cost = cost;
		}

		// Token: 0x06002F91 RID: 12177 RVA: 0x0013C7BC File Offset: 0x0013A9BC
		public static ApplicableResourceCollection CreateFor(UpgradeResourceCost cost)
		{
			return new ApplicableResourceCollection(ItemPrefab.Prefabs.Where(new Func<ItemPrefab, bool>(cost.MatchesItem)), cost.Amount, cost);
		}

		// Token: 0x040017E1 RID: 6113
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<ItemPrefab> MatchingItems;

		// Token: 0x040017E2 RID: 6114
		public readonly UpgradeResourceCost Cost;

		// Token: 0x040017E3 RID: 6115
		public readonly int Count;
	}
}
