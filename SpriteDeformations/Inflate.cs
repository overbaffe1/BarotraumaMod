using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000432 RID: 1074
	internal class Inflate : SpriteDeformation
	{
		// Token: 0x1700124A RID: 4682
		// (get) Token: 0x060047FC RID: 18428 RVA: 0x00278C4B File Offset: 0x00276E4B
		// (set) Token: 0x060047FD RID: 18429 RVA: 0x00278C53 File Offset: 0x00276E53
		public override float Phase
		{
			get
			{
				return this.phase;
			}
			set
			{
				this.phase = value;
			}
		}

		// Token: 0x1700124B RID: 4683
		// (get) Token: 0x060047FE RID: 18430 RVA: 0x00278C5C File Offset: 0x00276E5C
		private InflateParams InflateParams
		{
			get
			{
				return base.Params as InflateParams;
			}
		}

		// Token: 0x060047FF RID: 18431 RVA: 0x00278C6C File Offset: 0x00276E6C
		public Inflate(XElement element) : base(element, new InflateParams(element))
		{
			this.deformation = new Vector2[base.Resolution.X, base.Resolution.Y];
			for (int x = 0; x < base.Resolution.X; x++)
			{
				float normalizedX = (float)x / (float)(base.Resolution.X - 1);
				for (int y = 0; y < base.Resolution.Y; y++)
				{
					float normalizedY = (float)y / (float)(base.Resolution.X - 1);
					Vector2 centerDiff = new Vector2(normalizedX - 0.5f, normalizedY - 0.5f);
					float centerDist = centerDiff.Length() * 2f;
					if (centerDist != 0f)
					{
						this.deformation[x, y] = centerDiff / centerDist * Math.Min(1f, centerDist);
					}
				}
			}
			this.phase = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06004800 RID: 18432 RVA: 0x00278D68 File Offset: 0x00276F68
		protected override void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY = false)
		{
			deformation = this.deformation;
			multiplier = ((this.InflateParams.Frequency <= 0f) ? this.InflateParams.Scale : ((float)(Math.Sin((double)this.phase) + 1.0) / 2f * this.InflateParams.Scale));
			multiplier *= base.Params.Strength;
		}

		// Token: 0x06004801 RID: 18433 RVA: 0x00278DD7 File Offset: 0x00276FD7
		public override void Update(float deltaTime)
		{
			if (!base.Params.UseMovementSine)
			{
				this.phase += deltaTime * this.InflateParams.Frequency;
				this.phase %= 6.2831855f;
			}
		}

		// Token: 0x04002553 RID: 9555
		private float phase;

		// Token: 0x04002554 RID: 9556
		private Vector2[,] deformation;
	}
}
