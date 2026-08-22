using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200015B RID: 347
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentPackageId
	{
		// Token: 0x17000846 RID: 2118
		// (get) Token: 0x06001CC3 RID: 7363
		public abstract string StringRepresentation { get; }

		// Token: 0x06001CC4 RID: 7364 RVA: 0x000D02EA File Offset: 0x000CE4EA
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x06001CC5 RID: 7365
		[NullableContext(2)]
		public abstract override bool Equals(object obj);

		// Token: 0x06001CC6 RID: 7366
		public abstract override int GetHashCode();

		// Token: 0x06001CC7 RID: 7367 RVA: 0x000D02F2 File Offset: 0x000CE4F2
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<ContentPackageId> Parse(string s)
		{
			return ReflectionUtils.ParseDerived<ContentPackageId, string>(s);
		}
	}
}
