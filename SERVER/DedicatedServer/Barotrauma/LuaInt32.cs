using System;

namespace Barotrauma
{
	// Token: 0x0200021E RID: 542
	public struct LuaInt32
	{
		// Token: 0x060025A5 RID: 9637 RVA: 0x000F5EE0 File Offset: 0x000F40E0
		public LuaInt32(double v)
		{
			this.Value = (int)v;
		}

		// Token: 0x060025A6 RID: 9638 RVA: 0x000F5EEA File Offset: 0x000F40EA
		public LuaInt32(string v, int radix = 10)
		{
			this.Value = Convert.ToInt32(v, radix);
		}

		// Token: 0x060025A7 RID: 9639 RVA: 0x000F5EF9 File Offset: 0x000F40F9
		public static implicit operator int(LuaInt32 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025A8 RID: 9640 RVA: 0x000F5F01 File Offset: 0x000F4101
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125C RID: 4700
		public readonly int Value;
	}
}
