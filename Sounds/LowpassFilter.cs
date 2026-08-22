using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000441 RID: 1089
	public sealed class LowpassFilter : BiQuad
	{
		// Token: 0x060048B9 RID: 18617 RVA: 0x0027C4AA File Offset: 0x0027A6AA
		public LowpassFilter(int sampleRate, double frequency) : base(sampleRate, frequency, BiQuad.DefaultQ, 6.0)
		{
		}

		// Token: 0x060048BA RID: 18618 RVA: 0x0027C4C4 File Offset: 0x0027A6C4
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double norm = 1.0 / (1.0 + i / this._q + i * i);
			this.A0 = i * i * norm;
			this.A1 = 2.0 * this.A0;
			this.A2 = this.A0;
			this.B1 = 2.0 * (i * i - 1.0) * norm;
			this.B2 = (1.0 - i / this._q + i * i) * norm;
		}
	}
}
