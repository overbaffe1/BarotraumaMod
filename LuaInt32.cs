using System;

namespace Barotrauma
{
	// Token: 0x02000306 RID: 774
	public struct LuaInt32
	{
		// Token: 0x06003E8C RID: 16012 RVA: 0x00233898 File Offset: 0x00231A98
		public LuaInt32(double v)
		{
			this.Value = (int)v;
		}

		// Token: 0x06003E8D RID: 16013 RVA: 0x002338A2 File Offset: 0x00231AA2
		public LuaInt32(string v, int radix = 10)
		{
			this.Value = Convert.ToInt32(v, radix);
		}

		// Token: 0x06003E8E RID: 16014 RVA: 0x002338B1 File Offset: 0x00231AB1
		public static implicit operator int(LuaInt32 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E8F RID: 16015 RVA: 0x002338B9 File Offset: 0x00231AB9
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002088 RID: 8328
		public readonly int Value;
	}
}
