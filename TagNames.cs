using System;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000130 RID: 304
	public class TagNames : Attribute
	{
		// Token: 0x06002882 RID: 10370 RVA: 0x001C35D4 File Offset: 0x001C17D4
		public TagNames(params string[] names)
		{
			this.Names = (from n in names
			select n.ToIdentifier()).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x0400149C RID: 5276
		public readonly ImmutableHashSet<Identifier> Names;
	}
}
