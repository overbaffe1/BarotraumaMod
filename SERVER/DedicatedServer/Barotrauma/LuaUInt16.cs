using System;

namespace Barotrauma
{
	// Token: 0x0200021D RID: 541
	public struct LuaUInt16
	{
		// Token: 0x060025A1 RID: 9633 RVA: 0x000F5EB2 File Offset: 0x000F40B2
		public LuaUInt16(double v)
		{
			this.Value = (ushort)v;
		}

		// Token: 0x060025A2 RID: 9634 RVA: 0x000F5EBC File Offset: 0x000F40BC
		public LuaUInt16(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt16(v, radix);
		}

		// Token: 0x060025A3 RID: 9635 RVA: 0x000F5ECB File Offset: 0x000F40CB
		public static implicit operator ushort(LuaUInt16 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025A4 RID: 9636 RVA: 0x000F5ED3 File Offset: 0x000F40D3
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125B RID: 4699
		public readonly ushort Value;
	}
}
