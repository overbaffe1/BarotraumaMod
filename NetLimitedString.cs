using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000391 RID: 913
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct NetLimitedString
	{
		// Token: 0x06004493 RID: 17555 RVA: 0x0026495D File Offset: 0x00262B5D
		public NetLimitedString(string value)
		{
			this.Value = ((value.Length > 255) ? value.Substring(0, 255) : value);
		}

		// Token: 0x06004494 RID: 17556 RVA: 0x00264981 File Offset: 0x00262B81
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x040023E9 RID: 9193
		public readonly string Value;

		// Token: 0x040023EA RID: 9194
		public const int MaxLength = 255;

		// Token: 0x040023EB RID: 9195
		public static readonly NetLimitedString Empty = new NetLimitedString(string.Empty);
	}
}
