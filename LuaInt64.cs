using System;

namespace Barotrauma
{
	// Token: 0x02000308 RID: 776
	public struct LuaInt64
	{
		// Token: 0x06003E94 RID: 16020 RVA: 0x002338F4 File Offset: 0x00231AF4
		public LuaInt64(double v)
		{
			this.Value = (long)v;
		}

		// Token: 0x06003E95 RID: 16021 RVA: 0x002338FE File Offset: 0x00231AFE
		public LuaInt64(double lo, double hi)
		{
			this.Value = (long)((ulong)Convert.ToUInt32(lo) | (ulong)((ulong)((long)Convert.ToInt32(hi)) << 32));
		}

		// Token: 0x06003E96 RID: 16022 RVA: 0x00233918 File Offset: 0x00231B18
		public LuaInt64(string v, int radix = 10)
		{
			this.Value = Convert.ToInt64(v, radix);
		}

		// Token: 0x06003E97 RID: 16023 RVA: 0x00233927 File Offset: 0x00231B27
		public static implicit operator long(LuaInt64 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E98 RID: 16024 RVA: 0x0023392F File Offset: 0x00231B2F
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400208A RID: 8330
		public readonly long Value;
	}
}
