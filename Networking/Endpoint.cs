using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000497 RID: 1175
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class Endpoint
	{
		// Token: 0x17001409 RID: 5129
		// (get) Token: 0x06004E37 RID: 20023
		public abstract string StringRepresentation { get; }

		// Token: 0x1700140A RID: 5130
		// (get) Token: 0x06004E38 RID: 20024
		public abstract LocalizedString ServerTypeString { get; }

		// Token: 0x06004E39 RID: 20025 RVA: 0x002AC994 File Offset: 0x002AAB94
		public Endpoint(Address address)
		{
			this.Address = address;
		}

		// Token: 0x06004E3A RID: 20026
		[NullableContext(2)]
		public abstract override bool Equals(object obj);

		// Token: 0x06004E3B RID: 20027
		public abstract override int GetHashCode();

		// Token: 0x06004E3C RID: 20028 RVA: 0x002AC9A3 File Offset: 0x002AABA3
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x06004E3D RID: 20029 RVA: 0x002AC9AB File Offset: 0x002AABAB
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<Endpoint> Parse(string str)
		{
			return ReflectionUtils.ParseDerived<Endpoint, string>(str);
		}

		// Token: 0x06004E3E RID: 20030 RVA: 0x002AC9B3 File Offset: 0x002AABB3
		[NullableContext(2)]
		public static bool operator ==(Endpoint a, Endpoint b)
		{
			if (a == null)
			{
				return b == null;
			}
			return a.Equals(b);
		}

		// Token: 0x06004E3F RID: 20031 RVA: 0x002AC9C4 File Offset: 0x002AABC4
		[NullableContext(2)]
		public static bool operator !=(Endpoint a, Endpoint b)
		{
			return !(a == b);
		}

		// Token: 0x040029AE RID: 10670
		public readonly Address Address;
	}
}
