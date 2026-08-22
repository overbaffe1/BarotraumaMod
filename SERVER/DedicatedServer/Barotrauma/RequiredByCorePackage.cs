using System;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x0200015D RID: 349
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RequiredByCorePackage : Attribute
	{
		// Token: 0x06001CCE RID: 7374 RVA: 0x000D03A2 File Offset: 0x000CE5A2
		public RequiredByCorePackage(params Type[] alternativeTypes)
		{
			this.AlternativeTypes = alternativeTypes.ToImmutableHashSet<Type>();
		}

		// Token: 0x04000D18 RID: 3352
		public readonly ImmutableHashSet<Type> AlternativeTypes;
	}
}
