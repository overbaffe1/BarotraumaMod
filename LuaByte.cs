using System;

namespace Barotrauma
{
	// Token: 0x02000303 RID: 771
	public struct LuaByte
	{
		// Token: 0x06003E80 RID: 16000 RVA: 0x0023380E File Offset: 0x00231A0E
		public LuaByte(double v)
		{
			this.Value = (byte)v;
		}

		// Token: 0x06003E81 RID: 16001 RVA: 0x00233818 File Offset: 0x00231A18
		public LuaByte(string v, int radix = 10)
		{
			this.Value = Convert.ToByte(v, radix);
		}

		// Token: 0x06003E82 RID: 16002 RVA: 0x00233827 File Offset: 0x00231A27
		public static implicit operator byte(LuaByte luaValue)
		{
			return luaValue.Value;
		}

		// Token: 0x06003E83 RID: 16003 RVA: 0x0023382F File Offset: 0x00231A2F
		public override string ToString()
		{
			return this.Value.ToString();
		}

		// Token: 0x04002085 RID: 8325
		public readonly byte Value;
	}
}
