using System;

namespace Barotrauma
{
	// Token: 0x020002C2 RID: 706
	public sealed class MTRandom : Random
	{
		// Token: 0x06002FEB RID: 12267 RVA: 0x0014A8D2 File Offset: 0x00148AD2
		public MTRandom()
		{
			this.Initialize((uint)Environment.TickCount);
		}

		// Token: 0x06002FEC RID: 12268 RVA: 0x0014A8E5 File Offset: 0x00148AE5
		public MTRandom(int seed)
		{
			this.Initialize((uint)Math.Abs(seed));
		}

		// Token: 0x06002FED RID: 12269 RVA: 0x0014A8FC File Offset: 0x00148AFC
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

		// Token: 0x06002FEE RID: 12270 RVA: 0x0014A97C File Offset: 0x00148B7C
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

		// Token: 0x06002FEF RID: 12271 RVA: 0x0014A9E4 File Offset: 0x00148BE4
		public override int Next()
		{
			int retval = (int)(2147483647U & this.NextUInt32());
			if (retval == 2147483647)
			{
				return this.NextInt32();
			}
			return retval;
		}

		// Token: 0x06002FF0 RID: 12272 RVA: 0x0014AA10 File Offset: 0x00148C10
		public override int Next(int minValue, int maxValue)
		{
			int range = maxValue - minValue;
			return minValue + this.Next(range);
		}

		// Token: 0x06002FF1 RID: 12273 RVA: 0x0014AA2A File Offset: 0x00148C2A
		public override void NextBytes(byte[] buffer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002FF2 RID: 12274 RVA: 0x0014AA31 File Offset: 0x00148C31
		public override void NextBytes(Span<byte> buffer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002FF3 RID: 12275 RVA: 0x0014AA38 File Offset: 0x00148C38
		public override int Next(int maxValue)
		{
			return (int)(this.NextDouble() * (double)maxValue);
		}

		// Token: 0x06002FF4 RID: 12276 RVA: 0x0014AA44 File Offset: 0x00148C44
		public int NextInt32()
		{
			return (int)(2147483647U & this.NextUInt32());
		}

		// Token: 0x06002FF5 RID: 12277 RVA: 0x0014AA52 File Offset: 0x00148C52
		public override double NextDouble()
		{
			return 4.656612873077393E-10 * (double)this.NextInt32();
		}

		// Token: 0x06002FF6 RID: 12278 RVA: 0x0014AA68 File Offset: 0x00148C68
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

		// Token: 0x04001805 RID: 6149
		private const int N = 624;

		// Token: 0x04001806 RID: 6150
		private const int M = 397;

		// Token: 0x04001807 RID: 6151
		private const uint MATRIX_A = 2567483615U;

		// Token: 0x04001808 RID: 6152
		private const uint UPPER_MASK = 2147483648U;

		// Token: 0x04001809 RID: 6153
		private const uint LOWER_MASK = 2147483647U;

		// Token: 0x0400180A RID: 6154
		private const uint TEMPER1 = 2636928640U;

		// Token: 0x0400180B RID: 6155
		private const uint TEMPER2 = 4022730752U;

		// Token: 0x0400180C RID: 6156
		private const int TEMPER3 = 11;

		// Token: 0x0400180D RID: 6157
		private const int TEMPER4 = 7;

		// Token: 0x0400180E RID: 6158
		private const int TEMPER5 = 15;

		// Token: 0x0400180F RID: 6159
		private const int TEMPER6 = 18;

		// Token: 0x04001810 RID: 6160
		private uint[] mt;

		// Token: 0x04001811 RID: 6161
		private int mti;

		// Token: 0x04001812 RID: 6162
		private uint[] mag01;

		// Token: 0x04001813 RID: 6163
		private const double c_realUnitInt = 4.656612873077393E-10;
	}
}
