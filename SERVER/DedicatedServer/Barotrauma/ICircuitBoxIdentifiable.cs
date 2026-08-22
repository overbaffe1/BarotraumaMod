using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000123 RID: 291
	public interface ICircuitBoxIdentifiable
	{
		// Token: 0x1700081C RID: 2076
		// (get) Token: 0x06001B96 RID: 7062
		ushort ID { get; }

		// Token: 0x06001B97 RID: 7063 RVA: 0x000CD1F0 File Offset: 0x000CB3F0
		public static ushort FindFreeID<T>(IReadOnlyCollection<T> ids) where T : ICircuitBoxIdentifiable
		{
			ImmutableHashSet<ushort> sortedIds = (from i in ids
			select i.ID).ToImmutableHashSet<ushort>();
			for (ushort j = 0; j < 65534; j += 1)
			{
				if (!sortedIds.Contains(j))
				{
					return j;
				}
			}
			return ushort.MaxValue;
		}

		// Token: 0x04000CF9 RID: 3321
		public const ushort NullComponentID = 65535;
	}
}
