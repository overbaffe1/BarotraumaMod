using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000444 RID: 1092
	public sealed class NotchFilter : BiQuad
	{
		// Token: 0x060048BF RID: 18623 RVA: 0x0027C71D File Offset: 0x0027A91D
		public NotchFilter(int sampleRate, double frequency) : base(sampleRate, frequency, BiQuad.DefaultQ, 6.0)
		{
		}

		// Token: 0x060048C0 RID: 18624 RVA: 0x0027C738 File Offset: 0x0027A938
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double norm = 1.0 / (1.0 + i / this._q + i * i);
			this.A0 = (1.0 + i * i) * norm;
			this.A1 = 2.0 * (i * i - 1.0) * norm;
			this.A2 = this.A0;
			this.B1 = this.A1;
			this.B2 = (1.0 - i / this._q + i * i) * norm;
		}
	}
}
