using System;

namespace Barotrauma
{
	// Token: 0x02000305 RID: 773
	public struct LuaUInt16
	{
		// Token: 0x06003E88 RID: 16008 RVA: 0x0023386A File Offset: 0x00231A6A
		public LuaUInt16(double v)
		{
			this.Value = (ushort)v;
		}

		// Token: 0x06003E89 RID: 16009 RVA: 0x00233874 File Offset: 0x00231A74
		public LuaUInt16(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt16(v, radix);
		}

		// Token: 0x06003E8A RID: 16010 RVA: 0x00233883 File Offset: 0x00231A83
		public static implicit operator ushort(LuaUInt16 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E8B RID: 16011 RVA: 0x0023388B File Offset: 0x00231A8B
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002087 RID: 8327
		public readonly ushort Value;
	}
}
