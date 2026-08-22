using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200039A RID: 922
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class Endpoint
	{
		// Token: 0x17000F0E RID: 3854
		// (get) Token: 0x06003662 RID: 13922
		public abstract string StringRepresentation { get; }

		// Token: 0x17000F0F RID: 3855
		// (get) Token: 0x06003663 RID: 13923
		public abstract LocalizedString ServerTypeString { get; }

		// Token: 0x06003664 RID: 13924 RVA: 0x00173E58 File Offset: 0x00172058
		public Endpoint(Address address)
		{
			this.Address = address;
		}

		// Token: 0x06003665 RID: 13925
		[NullableContext(2)]
		public abstract override bool Equals(object obj);

		// Token: 0x06003666 RID: 13926
		public abstract override int GetHashCode();

		// Token: 0x06003667 RID: 13927 RVA: 0x00173E67 File Offset: 0x00172067
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x06003668 RID: 13928 RVA: 0x00173E6F File Offset: 0x0017206F
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<Endpoint> Parse(string str)
		{
			return ReflectionUtils.ParseDerived<Endpoint, string>(str);
		}

		// Token: 0x06003669 RID: 13929 RVA: 0x00173E77 File Offset: 0x00172077
		[NullableContext(2)]
		public static bool operator ==(Endpoint a, Endpoint b)
		{
			if (a == null)
			{
				return b == null;
			}
			return a.Equals(b);
		}

		// Token: 0x0600366A RID: 13930 RVA: 0x00173E88 File Offset: 0x00172088
		[NullableContext(2)]
		public static bool operator !=(Endpoint a, Endpoint b)
		{
			return !(a == b);
		}

		// Token: 0x04001BB7 RID: 7095
		public readonly Address Address;
	}
}
