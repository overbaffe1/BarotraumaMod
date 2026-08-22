using System;

namespace Barotrauma
{
	// Token: 0x02000220 RID: 544
	public struct LuaInt64
	{
		// Token: 0x060025AD RID: 9645 RVA: 0x000F5F3C File Offset: 0x000F413C
		public LuaInt64(double v)
		{
			this.Value = (long)v;
		}

		// Token: 0x060025AE RID: 9646 RVA: 0x000F5F46 File Offset: 0x000F4146
		public LuaInt64(double lo, double hi)
		{
			this.Value = (long)((ulong)Convert.ToUInt32(lo) | (ulong)((ulong)((long)Convert.ToInt32(hi)) << 32));
		}

		// Token: 0x060025AF RID: 9647 RVA: 0x000F5F60 File Offset: 0x000F4160
		public LuaInt64(string v, int radix = 10)
		{
			this.Value = Convert.ToInt64(v, radix);
		}

		// Token: 0x060025B0 RID: 9648 RVA: 0x000F5F6F File Offset: 0x000F416F
		public static implicit operator long(LuaInt64 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025B1 RID: 9649 RVA: 0x000F5F77 File Offset: 0x000F4177
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125E RID: 4702
		public readonly long Value;
	}
}
