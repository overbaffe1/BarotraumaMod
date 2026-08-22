using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000436 RID: 1078
	internal class NoiseDeformation : SpriteDeformation
	{
		// Token: 0x17001254 RID: 4692
		// (get) Token: 0x06004818 RID: 18456 RVA: 0x00279312 File Offset: 0x00277512
		private NoiseDeformationParams NoiseDeformationParams
		{
			get
			{
				return base.Params as NoiseDeformationParams;
			}
		}

		// Token: 0x06004819 RID: 18457 RVA: 0x0027931F File Offset: 0x0027751F
		public NoiseDeformation(XElement element) : base(element, new NoiseDeformationParams(element))
		{
			this.phase = Rand.Range(0f, 255f, Rand.RandSync.Unsynced);
			this.UpdateNoise();
		}

		// Token: 0x0600481A RID: 18458 RVA: 0x0027934C File Offset: 0x0027754C
		private void UpdateNoise()
		{
			for (int x = 0; x < base.Resolution.X; x++)
			{
				float normalizedX = (float)x / (float)(base.Resolution.X - 1) * this.NoiseDeformationParams.Frequency;
				for (int y = 0; y < base.Resolution.Y; y++)
				{
					float normalizedY = (float)y / (float)(base.Resolution.X - 1) * this.NoiseDeformationParams.Frequency;
					base.Deformation[x, y] = new Vector2(PerlinNoise.GetPerlin(normalizedX + this.phase, normalizedY + this.phase) - 0.5f, PerlinNoise.GetPerlin(normalizedY - this.phase, normalizedX - this.phase) - 0.5f);
				}
			}
		}

		// Token: 0x0600481B RID: 18459 RVA: 0x0027940F File Offset: 0x0027760F
		protected override void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY = false)
		{
			deformation = base.Deformation;
			multiplier = this.NoiseDeformationParams.Amplitude * base.Params.Strength;
		}

		// Token: 0x0600481C RID: 18460 RVA: 0x00279434 File Offset: 0x00277634
		public override void Update(float deltaTime)
		{
			if (this.NoiseDeformationParams.ChangeSpeed > 0f)
			{
				this.phase += deltaTime * this.NoiseDeformationParams.ChangeSpeed / 100f;
				this.phase %= 1f;
				this.UpdateNoise();
			}
		}

		// Token: 0x04002561 RID: 9569
		private float phase;
	}
}
