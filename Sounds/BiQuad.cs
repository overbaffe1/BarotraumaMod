using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000440 RID: 1088
	public abstract class BiQuad
	{
		// Token: 0x060048B4 RID: 18612 RVA: 0x0027C378 File Offset: 0x0027A578
		protected BiQuad(int sampleRate, double frequency, double q, double gainDb)
		{
			if (sampleRate <= 0)
			{
				throw new ArgumentOutOfRangeException("sampleRate");
			}
			if (frequency <= 0.0)
			{
				throw new ArgumentOutOfRangeException("frequency");
			}
			if (q <= 0.0)
			{
				throw new ArgumentOutOfRangeException("q");
			}
			if ((double)sampleRate < frequency * 2.0)
			{
				throw new ArgumentOutOfRangeException("sampleRate", "The sample rate has to be greater than or equal to 2 * frequency.");
			}
			this._sampleRate = sampleRate;
			this._frequency = frequency;
			this._q = q;
			this._gainDB = gainDb;
			this.CalculateBiQuadCoefficients();
		}

		// Token: 0x060048B5 RID: 18613 RVA: 0x0027C40C File Offset: 0x0027A60C
		public float Process(float input)
		{
			double o = (double)input * this.A0 + this.Z1;
			this.Z1 = (double)input * this.A1 + this.Z2 - this.B1 * o;
			this.Z2 = (double)input * this.A2 - this.B2 * o;
			return (float)o;
		}

		// Token: 0x060048B6 RID: 18614 RVA: 0x0027C464 File Offset: 0x0027A664
		public void Process(float[] input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				input[i] = this.Process(input[i]);
			}
		}

		// Token: 0x060048B7 RID: 18615
		protected abstract void CalculateBiQuadCoefficients();

		// Token: 0x040025B4 RID: 9652
		protected double A0;

		// Token: 0x040025B5 RID: 9653
		protected double A1;

		// Token: 0x040025B6 RID: 9654
		protected double A2;

		// Token: 0x040025B7 RID: 9655
		protected double B1;

		// Token: 0x040025B8 RID: 9656
		protected double B2;

		// Token: 0x040025B9 RID: 9657
		protected readonly double _q;

		// Token: 0x040025BA RID: 9658
		protected readonly double _gainDB;

		// Token: 0x040025BB RID: 9659
		protected double Z1;

		// Token: 0x040025BC RID: 9660
		protected double Z2;

		// Token: 0x040025BD RID: 9661
		protected readonly double _frequency;

		// Token: 0x040025BE RID: 9662
		protected readonly int _sampleRate;

		// Token: 0x040025BF RID: 9663
		protected static readonly double DefaultQ = 1.0 / Math.Sqrt(2.0);

		// Token: 0x040025C0 RID: 9664
		protected const double DefaultGainDb = 6.0;
	}
}
