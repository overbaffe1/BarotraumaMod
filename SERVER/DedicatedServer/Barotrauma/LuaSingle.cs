using System;

namespace Barotrauma
{
	// Token: 0x02000222 RID: 546
	public struct LuaSingle
	{
		// Token: 0x060025B7 RID: 9655 RVA: 0x000F5FCC File Offset: 0x000F41CC
		public LuaSingle(double v)
		{
			this.Value = (float)v;
		}

		// Token: 0x060025B8 RID: 9656 RVA: 0x000F5FD6 File Offset: 0x000F41D6
		public LuaSingle(string v)
		{
			this.Value = float.Parse(v);
		}

		// Token: 0x060025B9 RID: 9657 RVA: 0x000F5FE4 File Offset: 0x000F41E4
		public static implicit operator float(LuaSingle luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x060025BA RID: 9658 RVA: 0x000F5FEC File Offset: 0x000F41EC
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04001260 RID: 4704
		public readonly float Value;
	}
}
