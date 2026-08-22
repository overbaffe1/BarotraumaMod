using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000251 RID: 593
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentPackageId
	{
		// Token: 0x17000EA9 RID: 3753
		// (get) Token: 0x060037B4 RID: 14260
		public abstract string StringRepresentation { get; }

		// Token: 0x060037B5 RID: 14261 RVA: 0x00216256 File Offset: 0x00214456
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x060037B6 RID: 14262
		[NullableContext(2)]
		public abstract override bool Equals(object obj);

		// Token: 0x060037B7 RID: 14263
		public abstract override int GetHashCode();

		// Token: 0x060037B8 RID: 14264 RVA: 0x0021625E File Offset: 0x0021445E
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
