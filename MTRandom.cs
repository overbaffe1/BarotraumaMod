using System;

namespace Barotrauma
{
	// Token: 0x0200038D RID: 909
	public sealed class MTRandom : Random
	{
		// Token: 0x06004469 RID: 17513 RVA: 0x00264376 File Offset: 0x00262576
		public MTRandom()
		{
			this.Initialize((uint)Environment.TickCount);
		}

		// Token: 0x0600446A RID: 17514 RVA: 0x00264389 File Offset: 0x00262589
		public MTRandom(int seed)
		{
			this.Initialize((uint)Math.Abs(seed));
		}

		// Token: 0x0600446B RID: 17515 RVA: 0x002643A0 File Offset: 0x002625A0
		private void Initialize(uint seed)
		{
			this.mt = new uint[624];
			this.mti = 625;
			this.mag01 = new uint[]
			{
				0U,
				2567483615U
			};
			this.mt[0] = seed;
			for (int i = 1; i < 624; i++)
			{
				this.mt[i] = (uint)((ulong)(1812433253U * (this.mt[i - 1] ^ this.mt[i - 1] >> 30)) + (ulong)((long)i));
			}
		}

		// Token: 0x0600446C RID: 17516 RVA: 0x00264420 File Offset: 0x00262620
		private uint NextUInt32()
		{
			if (this.mti >= 624)
			{
				this.GenRandAll();
				this.mti = 0;
			}
			uint[] array = this.mt;
			int num = this.mti;
			this.mti = num + 1;
			uint y = array[num];
			y ^= y >> 11;
			y ^= (y << 7 & 2636928640U);
			y ^= (y << 15 & 4022730752U);
			return y ^ y >> 18;
		}

		// Token: 0x0600446D RID: 17517 RVA: 0x00264488 File Offset: 0x00262688
		public override int Next()
		{
			int retval = (int)(2147483647U & this.NextUInt32());
			if (retval == 2147483647)
			{
				return this.NextInt32();
			}
			return retval;
		}

		// Token: 0x0600446E RID: 17518 RVA: 0x002644B4 File Offset: 0x002626B4
		public override int Next(int minValue, int maxValue)
		{
			int range = maxValue - minValue;
			return minValue + this.Next(range);
		}

		// Token: 0x0600446F RID: 17519 RVA: 0x002644CE File Offset: 0x002626CE
		public override void NextBytes(byte[] buffer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06004470 RID: 17520 RVA: 0x002644D5 File Offset: 0x002626D5
		public override void NextBytes(Span<byte> buffer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06004471 RID: 17521 RVA: 0x002644DC File Offset: 0x002626DC
		public override int Next(int maxValue)
		{
			return (int)(this.NextDouble() * (double)maxValue);
		}

		// Token: 0x06004472 RID: 17522 RVA: 0x002644E8 File Offset: 0x002626E8
		public int NextInt32()
		{
			return (int)(2147483647U & this.NextUInt32());
		}

		// Token: 0x06004473 RID: 17523 RVA: 0x002644F6 File Offset: 0x002626F6
		public override double NextDouble()
		{
			return 4.656612873077393E-10 * (double)this.NextInt32();
		}

		// Token: 0x06004474 RID: 17524 RVA: 0x0026450C File Offset: 0x0026270C
		private void GenRandAll()
		{
			int kk = 1;
			uint y = this.mt[0] & 2147483648U;
			uint p;
			do
			{
				p = this.mt[kk];
				this.mt[kk - 1] = (this.mt[kk + 396] ^ (y | (p & 2147483647U)) >> 1 ^ this.mag01[(int)(p & 1U)]);
				y = (p & 2147483648U);
			}
			while (++kk < 228);
			do
			{
				p = this.mt[kk];
				this.mt[kk - 1] = (this.mt[kk + -228] ^ (y | (p & 2147483647U)) >> 1 ^ this.mag01[(int)(p & 1U)]);
				y = (p & 2147483648U);
			}
			while (++kk < 624);
			p = this.mt[0];
			this.mt[623] = (this.mt[396] ^ (y | (p & 2147483647U)) >> 1 ^ this.mag01[(int)(p & 1U)]);
		}

		// Token: 0x040023D7 RID: 9175
		private const int N = 624;

		// Token: 0x040023D8 RID: 9176
		private const int M = 397;

		// Token: 0x040023D9 RID: 9177
		private const uint MATRIX_A = 2567483615U;

		// Token: 0x040023DA RID: 9178
		private const uint UPPER_MASK = 2147483648U;

		// Token: 0x040023DB RID: 9179
		private const uint LOWER_MASK = 2147483647U;

		// Token: 0x040023DC RID: 9180
		private const uint TEMPER1 = 2636928640U;

		// Token: 0x040023DD RID: 9181
		private const uint TEMPER2 = 4022730752U;

		// Token: 0x040023DE RID: 9182
		private const int TEMPER3 = 11;

		// Token: 0x040023DF RID: 9183
		private const int TEMPER4 = 7;

		// Token: 0x040023E0 RID: 9184
		private const int TEMPER5 = 15;

		// Token: 0x040023E1 RID: 9185
		private const int TEMPER6 = 18;

		// Token: 0x040023E2 RID: 9186
		private uint[] mt;

		// Token: 0x040023E3 RID: 9187
		private int mti;

		// Token: 0x040023E4 RID: 9188
		private uint[] mag01;

		// Token: 0x040023E5 RID: 9189
		private const double c_realUnitInt = 4.656612873077393E-10;
	}
}
