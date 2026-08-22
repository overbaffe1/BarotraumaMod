using System;

namespace Barotrauma
{
	// Token: 0x02000307 RID: 775
	public struct LuaUInt32
	{
		// Token: 0x06003E90 RID: 16016 RVA: 0x002338C6 File Offset: 0x00231AC6
		public LuaUInt32(double v)
		{
			this.Value = (uint)v;
		}

		// Token: 0x06003E91 RID: 16017 RVA: 0x002338D0 File Offset: 0x00231AD0
		public LuaUInt32(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt32(v, radix);
		}

		// Token: 0x06003E92 RID: 16018 RVA: 0x002338DF File Offset: 0x00231ADF
		public static implicit operator uint(LuaUInt32 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E93 RID: 16019 RVA: 0x002338E7 File Offset: 0x00231AE7
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002089 RID: 8329
		public readonly uint Value;
	}
}
