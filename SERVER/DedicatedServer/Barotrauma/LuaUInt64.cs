using System;

namespace Barotrauma
{
	// Token: 0x02000221 RID: 545
	public struct LuaUInt64
	{
		// Token: 0x060025B2 RID: 9650 RVA: 0x000F5F84 File Offset: 0x000F4184
		public LuaUInt64(double v)
		{
			this.Value = (ulong)v;
		}

		// Token: 0x060025B3 RID: 9651 RVA: 0x000F5F8E File Offset: 0x000F418E
		public LuaUInt64(double lo, double hi)
		{
			this.Value = ((ulong)Convert.ToUInt32(lo) | (ulong)Convert.ToUInt32(hi) << 32);
		}

		// Token: 0x060025B4 RID: 9652 RVA: 0x000F5FA8 File Offset: 0x000F41A8
		public LuaUInt64(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt64(v, radix);
		}

		// Token: 0x060025B5 RID: 9653 RVA: 0x000F5FB7 File Offset: 0x000F41B7
		public static implicit operator ulong(LuaUInt64 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025B6 RID: 9654 RVA: 0x000F5FBF File Offset: 0x000F41BF
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125F RID: 4703
		public readonly ulong Value;
	}
}
