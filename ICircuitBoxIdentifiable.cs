using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200021A RID: 538
	public interface ICircuitBoxIdentifiable
	{
		// Token: 0x17000E7C RID: 3708
		// (get) Token: 0x06003673 RID: 13939
		ushort ID { get; }

		// Token: 0x06003674 RID: 13940 RVA: 0x0021291C File Offset: 0x00210B1C
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

		// Token: 0x04001C06 RID: 7174
		public const ushort NullComponentID = 65535;
	}
}
