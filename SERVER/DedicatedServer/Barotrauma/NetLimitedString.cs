using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002C6 RID: 710
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct NetLimitedString
	{
		// Token: 0x06003015 RID: 12309 RVA: 0x0014AEB9 File Offset: 0x001490B9
		public NetLimitedString(string value)
		{
			this.Value = ((value.Length > 255) ? value.Substring(0, 255) : value);
		}

		// Token: 0x06003016 RID: 12310 RVA: 0x0014AEDD File Offset: 0x001490DD
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x04001817 RID: 6167
		public readonly string Value;

		// Token: 0x04001818 RID: 6168
		public const int MaxLength = 255;

		// Token: 0x04001819 RID: 6169
		public static readonly NetLimitedString Empty = new NetLimitedString(string.Empty);
	}
}
