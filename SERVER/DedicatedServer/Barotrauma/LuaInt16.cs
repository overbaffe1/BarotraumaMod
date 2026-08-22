using System;

namespace Barotrauma
{
	// Token: 0x0200021C RID: 540
	public struct LuaInt16
	{
		// Token: 0x0600259D RID: 9629 RVA: 0x000F5E84 File Offset: 0x000F4084
		public LuaInt16(double v)
		{
			this.Value = (short)v;
		}

		// Token: 0x0600259E RID: 9630 RVA: 0x000F5E8E File Offset: 0x000F408E
		public LuaInt16(string v, int radix = 10)
		{
			this.Value = Convert.ToInt16(v, radix);
		}

		// Token: 0x0600259F RID: 9631 RVA: 0x000F5E9D File Offset: 0x000F409D
		public static implicit operator short(LuaInt16 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025A0 RID: 9632 RVA: 0x000F5EA5 File Offset: 0x000F40A5
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125A RID: 4698
		public readonly short Value;
	}
}
