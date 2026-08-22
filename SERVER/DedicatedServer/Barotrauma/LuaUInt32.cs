using System;

namespace Barotrauma
{
	// Token: 0x0200021F RID: 543
	public struct LuaUInt32
	{
		// Token: 0x060025A9 RID: 9641 RVA: 0x000F5F0E File Offset: 0x000F410E
		public LuaUInt32(double v)
		{
			this.Value = (uint)v;
		}

		// Token: 0x060025AA RID: 9642 RVA: 0x000F5F18 File Offset: 0x000F4118
		public LuaUInt32(string v, int radix = 10)
		{
			this.Value = Convert.ToUInt32(v, radix);
		}

		// Token: 0x060025AB RID: 9643 RVA: 0x000F5F27 File Offset: 0x000F4127
		public static implicit operator uint(LuaUInt32 luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025AC RID: 9644 RVA: 0x000F5F2F File Offset: 0x000F412F
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x0400125D RID: 4701
		public readonly uint Value;
	}
}
