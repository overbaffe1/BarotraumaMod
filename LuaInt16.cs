using System;

namespace Barotrauma
{
	// Token: 0x02000304 RID: 772
	public struct LuaInt16
	{
		// Token: 0x06003E84 RID: 16004 RVA: 0x0023383C File Offset: 0x00231A3C
		public LuaInt16(double v)
		{
			this.Value = (short)v;
		}

		// Token: 0x06003E85 RID: 16005 RVA: 0x00233846 File Offset: 0x00231A46
		public LuaInt16(string v, int radix = 10)
		{
			this.Value = Convert.ToInt16(v, radix);
		}

		// Token: 0x06003E86 RID: 16006 RVA: 0x00233855 File Offset: 0x00231A55
		public static implicit operator short(LuaInt16 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E87 RID: 16007 RVA: 0x0023385D File Offset: 0x00231A5D
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002086 RID: 8326
		public readonly short Value;
	}
}
