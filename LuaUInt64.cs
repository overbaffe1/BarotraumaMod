using System;

namespace Barotrauma
{
	// Token: 0x02000309 RID: 777
	public struct LuaUInt64
	{
		// Token: 0x06003E99 RID: 16025 RVA: 0x0023393C File Offset: 0x00231B3C
		public LuaUInt64(double v)
		{
			this.Value = (ulong)v;
		}

		// Token: 0x06003E9A RID: 16026 RVA: 0x00233946 File Offset: 0x00231B46
		public LuaUInt64(double lo, double hi)
		{
			this.Value = ((ulong)Convert.ToUInt32(lo) | (ulong)Convert.ToUInt32(hi) << 32);
		}

		// Token: 0x06003E9B RID: 16027 RVA: 0x00233960 File Offset: 0x00231B60
		public LuaUInt64(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt64(v, radix);
		}

		// Token: 0x06003E9C RID: 16028 RVA: 0x0023396F File Offset: 0x00231B6F
		public static implicit operator ulong(LuaUInt64 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E9D RID: 16029 RVA: 0x00233977 File Offset: 0x00231B77
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400208B RID: 8331
		public readonly ulong Value;
	}
}
