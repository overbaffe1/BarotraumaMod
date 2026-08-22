using System;

namespace Barotrauma
{
	// Token: 0x02000302 RID: 770
	public struct LuaSByte
	{
		// Token: 0x06003E7C RID: 15996 RVA: 0x002337E0 File Offset: 0x002319E0
		public LuaSByte(double v)
		{
			this.Value = (sbyte)v;
		}

		// Token: 0x06003E7D RID: 15997 RVA: 0x002337EA File Offset: 0x002319EA
		public LuaSByte(string v, int radix = 10)
		{
			this.Value = Convert.ToSByte(v, radix);
		}

		// Token: 0x06003E7E RID: 15998 RVA: 0x002337F9 File Offset: 0x002319F9
		public static implicit operator sbyte(LuaSByte luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E7F RID: 15999 RVA: 0x00233801 File Offset: 0x00231A01
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002084 RID: 8324
		public readonly sbyte Value;
	}
}
