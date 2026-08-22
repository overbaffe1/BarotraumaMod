using System;

namespace Barotrauma
{
	// Token: 0x0200030B RID: 779
	public struct LuaDouble
	{
		// Token: 0x06003EA2 RID: 16034 RVA: 0x002339B1 File Offset: 0x00231BB1
		public LuaDouble(double v)
		{
			this.Value = v;
		}

		// Token: 0x06003EA3 RID: 16035 RVA: 0x002339BA File Offset: 0x00231BBA
		public LuaDouble(string v)
		{
			this.Value = double.Parse(v);
		}

		// Token: 0x06003EA4 RID: 16036 RVA: 0x002339C8 File Offset: 0x00231BC8
		public static implicit operator double(LuaDouble luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003EA5 RID: 16037 RVA: 0x002339D0 File Offset: 0x00231BD0
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400208D RID: 8333
		public readonly double Value;
	}
}
