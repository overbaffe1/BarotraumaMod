using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000FE RID: 254
	internal class BlurEffect
	{
		// Token: 0x06002403 RID: 9219 RVA: 0x001687F5 File Offset: 0x001669F5
		public BlurEffect(Effect effect, float dx, float dy)
		{
			this.Effect = effect;
			this.SetParameters(dx, dy);
		}

		// Token: 0x06002404 RID: 9220 RVA: 0x0016880C File Offset: 0x00166A0C
		public void SetParameters(float dx, float dy)
		{
			EffectParameter weightsParameter = this.Effect.Parameters["SampleWeights"];
			EffectParameter offsetsParameter = this.Effect.Parameters["SampleOffsets"];
			int sampleCount = weightsParameter.Elements.Count;
			float[] sampleWeights = new float[sampleCount];
			Vector2[] sampleOffsets = new Vector2[sampleCount];
			sampleWeights[0] = this.ComputeGaussian(0f);
			sampleOffsets[0] = new Vector2(0f);
			float totalWeights = sampleWeights[0];
			for (int i = 0; i < sampleCount / 2; i++)
			{
				float weight = this.ComputeGaussian((float)(i + 1));
				sampleWeights[i * 2 + 1] = weight;
				sampleWeights[i * 2 + 2] = weight;
				totalWeights += weight * 2f;
				float sampleOffset = (float)(i * 2) + 1.5f;
				Vector2 delta = new Vector2(dx, dy) * sampleOffset;
				sampleOffsets[i * 2 + 1] = delta;
				sampleOffsets[i * 2 + 2] = -delta;
			}
			for (int j = 0; j < sampleWeights.Length; j++)
			{
				sampleWeights[j] /= totalWeights;
			}
			weightsParameter.SetValue(sampleWeights);
			offsetsParameter.SetValue(sampleOffsets);
		}

		// Token: 0x06002405 RID: 9221 RVA: 0x00168934 File Offset: 0x00166B34
		private float ComputeGaussian(float n)
		{
			float theta = 2f;
			return (float)(1.0 / Math.Sqrt(6.283185307179586 * (double)theta) * Math.Exp((double)(-(double)(n * n) / (2f * theta * theta))));
		}

		// Token: 0x040011F3 RID: 4595
		public readonly Effect Effect;
	}
}
