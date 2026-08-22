using System;

namespace Barotrauma
{
	// Token: 0x02000223 RID: 547
	public struct LuaDouble
	{
		// Token: 0x060025BB RID: 9659 RVA: 0x000F5FF9 File Offset: 0x000F41F9
		public LuaDouble(double v)
		{
			this.Value = v;
		}

		// Token: 0x060025BC RID: 9660 RVA: 0x000F6002 File Offset: 0x000F4202
		public LuaDouble(string v)
		{
			this.Value = double.Parse(v);
		}

		// Token: 0x060025BD RID: 9661 RVA: 0x000F6010 File Offset: 0x000F4210
		public static implicit operator double(LuaDouble luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025BE RID: 9662 RVA: 0x000F6018 File Offset: 0x000F4218
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04001261 RID: 4705
		public readonly double Value;
	}
}
