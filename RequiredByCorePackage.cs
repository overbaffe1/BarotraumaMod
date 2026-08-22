using System;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x02000253 RID: 595
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class RequiredByCorePackage : Attribute
	{
		// Token: 0x060037BF RID: 14271 RVA: 0x0021630E File Offset: 0x0021450E
		public RequiredByCorePackage(params Type[] alternativeTypes)
		{
			this.AlternativeTypes = alternativeTypes.ToImmutableHashSet<Type>();
		}

		// Token: 0x04001C25 RID: 7205
		public readonly ImmutableHashSet<Type> AlternativeTypes;
	}
}
