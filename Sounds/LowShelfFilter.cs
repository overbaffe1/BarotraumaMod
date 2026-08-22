using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000445 RID: 1093
	public sealed class LowShelfFilter : BiQuad
	{
		// Token: 0x060048C1 RID: 18625 RVA: 0x0027C7EE File Offset: 0x0027A9EE
		public LowShelfFilter(int sampleRate, double frequency, double gainDB) : base(sampleRate, frequency, BiQuad.DefaultQ, gainDB)
		{
		}

		// Token: 0x060048C2 RID: 18626 RVA: 0x0027C800 File Offset: 0x0027AA00
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double v = Math.Pow(10.0, Math.Abs(this._gainDB) / 20.0);
			double norm;
			if (this._gainDB >= 0.0)
			{
				norm = 1.0 / (1.0 + 1.4142135623730951 * i + i * i);
				this.A0 = (1.0 + Math.Sqrt(2.0 * v) * i + v * i * i) * norm;
				this.A1 = 2.0 * (v * i * i - 1.0) * norm;
				this.A2 = (1.0 - Math.Sqrt(2.0 * v) * i + v * i * i) * norm;
				this.B1 = 2.0 * (i * i - 1.0) * norm;
				this.B2 = (1.0 - 1.4142135623730951 * i + i * i) * norm;
				return;
			}
			norm = 1.0 / (1.0 + Math.Sqrt(2.0 * v) * i + v * i * i);
			this.A0 = (1.0 + 1.4142135623730951 * i + i * i) * norm;
			this.A1 = 2.0 * (i * i - 1.0) * norm;
			this.A2 = (1.0 - 1.4142135623730951 * i + i * i) * norm;
			this.B1 = 2.0 * (v * i * i - 1.0) * norm;
			this.B2 = (1.0 - Math.Sqrt(2.0 * v) * i + v * i * i) * norm;
		}
	}
}
