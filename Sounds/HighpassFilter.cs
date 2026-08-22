using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000442 RID: 1090
	public sealed class HighpassFilter : BiQuad
	{
		// Token: 0x060048BB RID: 18619 RVA: 0x0027C57A File Offset: 0x0027A77A
		public HighpassFilter(int sampleRate, double frequency) : base(sampleRate, frequency, BiQuad.DefaultQ, 6.0)
		{
		}

		// Token: 0x060048BC RID: 18620 RVA: 0x0027C594 File Offset: 0x0027A794
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double norm = 1.0 / (1.0 + i / this._q + i * i);
			this.A0 = 1.0 * norm;
			this.A1 = -2.0 * this.A0;
			this.A2 = this.A0;
			this.B1 = 2.0 * (i * i - 1.0) * norm;
			this.B2 = (1.0 - i / this._q + i * i) * norm;
		}
	}
}
