using System;

namespace Barotrauma
{
	// Token: 0x0200021B RID: 539
	public struct LuaByte
	{
		// Token: 0x06002599 RID: 9625 RVA: 0x000F5E56 File Offset: 0x000F4056
		public LuaByte(double v)
		{
			this.Value = (byte)v;
		}

		// Token: 0x0600259A RID: 9626 RVA: 0x000F5E60 File Offset: 0x000F4060
		public LuaByte(string v, int radix = 10)
		{
			this.Value = Convert.ToByte(v, radix);
		}

		// Token: 0x0600259B RID: 9627 RVA: 0x000F5E6F File Offset: 0x000F406F
		public static implicit operator byte(LuaByte luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x0600259C RID: 9628 RVA: 0x000F5E77 File Offset: 0x000F4077
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04001259 RID: 4697
		public readonly byte Value;
	}
}
