using System;

namespace Barotrauma.Sounds
{
	// Token: 0x02000446 RID: 1094
	public sealed class HighShelfFilter : BiQuad
	{
		// Token: 0x060048C3 RID: 18627 RVA: 0x0027CA16 File Offset: 0x0027AC16
		public HighShelfFilter(int sampleRate, double frequency, double gainDB) : base(sampleRate, frequency, BiQuad.DefaultQ, gainDB)
		{
		}

		// Token: 0x060048C4 RID: 18628 RVA: 0x0027CA28 File Offset: 0x0027AC28
		protected override void CalculateBiQuadCoefficients()
		{
			double i = Math.Tan(3.141592653589793 * this._frequency / (double)this._sampleRate);
			double v = Math.Pow(10.0, Math.Abs(this._gainDB) / 20.0);
			double norm;
			if (this._gainDB >= 0.0)
			{
				norm = 1.0 / (1.0 + 1.4142135623730951 * i + i * i);
				this.A0 = (v + Math.Sqrt(2.0 * v) * i + i * i) * norm;
				this.A1 = 2.0 * (i * i - v) * norm;
				this.A2 = (v - Math.Sqrt(2.0 * v) * i + i * i) * norm;
				this.B1 = 2.0 * (i * i - 1.0) * norm;
				this.B2 = (1.0 - 1.4142135623730951 * i + i * i) * norm;
				return;
			}
			norm = 1.0 / (v + Math.Sqrt(2.0 * v) * i + i * i);
			this.A0 = (1.0 + 1.4142135623730951 * i + i * i) * norm;
			this.A1 = 2.0 * (i * i - 1.0) * norm;
			this.A2 = (1.0 - 1.4142135623730951 * i + i * i) * norm;
			this.B1 = 2.0 * (i * i - v) * norm;
			this.B2 = (v - Math.Sqrt(2.0 * v) * i + i * i) * norm;
		}
	}
}
