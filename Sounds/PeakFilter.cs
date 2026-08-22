using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000447 RID: 1095
	public sealed class PeakFilter : BiQuad
	{
		// Token: 0x060048C5 RID: 18629 RVA: 0x0027CC02 File Offset: 0x0027AE02
		public PeakFilter(int sampleRate, double frequency, double bandWidth, double peakGainDB) : base(sampleRate, frequency, bandWidth, peakGainDB)
		{
		}

		// Token: 0x060048C6 RID: 18630 RVA: 0x0027CC10 File Offset: 0x0027AE10
		protected override void CalculateBiQuadCoefficients()
		{
			double v = Math.Pow(10.0, Math.Abs(this._gainDB) / 20.0);
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double q = this._q;
			double norm;
			if (this._gainDB >= 0.0)
			{
				norm = 1.0 / (1.0 + 1.0 / q * i + i * i);
				this.A0 = (1.0 + v / q * i + i * i) * norm;
				this.A1 = 2.0 * (i * i - 1.0) * norm;
				this.A2 = (1.0 - v / q * i + i * i) * norm;
				this.B1 = this.A1;
				this.B2 = (1.0 - 1.0 / q * i + i * i) * norm;
				return;
			}
			norm = 1.0 / (1.0 + v / q * i + i * i);
			this.A0 = (1.0 + 1.0 / q * i + i * i) * norm;
			this.A1 = 2.0 * (i * i - 1.0) * norm;
			this.A2 = (1.0 - 1.0 / q * i + i * i) * norm;
			this.B1 = this.A1;
			this.B2 = (1.0 - v / q * i + i * i) * norm;
		}
	}
}
