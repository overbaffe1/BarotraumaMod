using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000443 RID: 1091
	public sealed class BandpassFilter : BiQuad
	{
		// Token: 0x060048BD RID: 18621 RVA: 0x0027C650 File Offset: 0x0027A850
		public BandpassFilter(int sampleRate, double frequency) : base(sampleRate, frequency, BiQuad.DefaultQ, 6.0)
		{
		}

		// Token: 0x060048BE RID: 18622 RVA: 0x0027C668 File Offset: 0x0027A868
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double norm = 1.0 / (1.0 + i / this._q + i * i);
			this.A0 = i / this._q * norm;
			this.A1 = 0.0;
			this.A2 = -this.A0;
			this.B1 = 2.0 * (i * i - 1.0) * norm;
			this.B2 = (1.0 - i / this._q + i * i) * norm;
		}
	}
}
