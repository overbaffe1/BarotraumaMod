using System;

namespace Barotrauma
{
	// Token: 0x0200021A RID: 538
	public struct LuaSByte
	{
		// Token: 0x06002595 RID: 9621 RVA: 0x000F5E28 File Offset: 0x000F4028
		public LuaSByte(double v)
		{
			this.Value = (sbyte)v;
		}

		// Token: 0x06002596 RID: 9622 RVA: 0x000F5E32 File Offset: 0x000F4032
		public LuaSByte(string v, int radix = 10)
		{
			this.Value = Convert.ToSByte(v, radix);
		}

		// Token: 0x06002597 RID: 9623 RVA: 0x000F5E41 File Offset: 0x000F4041
		public static implicit operator sbyte(LuaSByte luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06002598 RID: 9624 RVA: 0x000F5E49 File Offset: 0x000F4049
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04001258 RID: 4696
		public readonly sbyte Value;
	}
}
