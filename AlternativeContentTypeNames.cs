using System;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x02000254 RID: 596
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class AlternativeContentTypeNames : Attribute
	{
		// Token: 0x060037C0 RID: 14272 RVA: 0x00216322 File Offset: 0x00214522
		public AlternativeContentTypeNames(params string[] names)
		{
			this.Names = names.ToIdentifiers().ToImmutableHashSet<Identifier>();
		}

		// Token: 0x04001C26 RID: 7206
		public readonly ImmutableHashSet<Identifier> Names;
	}
}
