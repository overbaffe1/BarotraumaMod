using System;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x0200015E RID: 350
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class AlternativeContentTypeNames : Attribute
	{
		// Token: 0x06001CCF RID: 7375 RVA: 0x000D03B6 File Offset: 0x000CE5B6
		public AlternativeContentTypeNames(params string[] names)
		{
			this.Names = names.ToIdentifiers().ToImmutableHashSet<Identifier>();
		}

		// Token: 0x04000D19 RID: 3353
		public readonly ImmutableHashSet<Identifier> Names;
	}
}
