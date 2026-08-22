using System;

namespace Barotrauma
{
	// Token: 0x0200030A RID: 778
	public struct LuaSingle
	{
		// Token: 0x06003E9E RID: 16030 RVA: 0x00233984 File Offset: 0x00231B84
		public LuaSingle(double v)
		{
			this.Value = (float)v;
		}

		// Token: 0x06003E9F RID: 16031 RVA: 0x0023398E File Offset: 0x00231B8E
		public LuaSingle(string v)
		{
			this.Value = float.Parse(v);
		}

		// Token: 0x06003EA0 RID: 16032 RVA: 0x0023399C File Offset: 0x00231B9C
		public static implicit operator float(LuaSingle luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003EA1 RID: 16033 RVA: 0x002339A4 File Offset: 0x00231BA4
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400208C RID: 8332
		public readonly float Value;
	}
}
