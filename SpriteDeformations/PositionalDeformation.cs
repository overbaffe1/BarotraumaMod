using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000438 RID: 1080
	internal class PositionalDeformation : SpriteDeformation
	{
		// Token: 0x17001259 RID: 4697
		// (get) Token: 0x06004826 RID: 18470 RVA: 0x002794D8 File Offset: 0x002776D8
		private PositionalDeformationParams positionalDeformationParams
		{
			get
			{
				return base.Params as PositionalDeformationParams;
			}
		}

		// Token: 0x06004827 RID: 18471 RVA: 0x002794E5 File Offset: 0x002776E5
		public PositionalDeformation(XElement element) : base(element, new PositionalDeformationParams(element))
		{
		}

		// Token: 0x06004828 RID: 18472 RVA: 0x002794F4 File Offset: 0x002776F4
		public override void Update(float deltaTime)
		{
			if (this.positionalDeformationParams.RecoverSpeed <= 0f)
			{
				return;
			}
			for (int x = 0; x < base.Resolution.X; x++)
			{
				for (int y = 0; y < base.Resolution.Y; y++)
				{
					if (base.Deformation[x, y].LengthSquared() < 1E-06f)
					{
						base.Deformation[x, y] = Vector2.Zero;
					}
					else
					{
						Vector2 reduction = base.Deformation[x, y];
						base.Deformation[x, y] -= reduction.ClampLength(this.positionalDeformationParams.RecoverSpeed) * deltaTime;
					}
				}
			}
		}

		// Token: 0x06004829 RID: 18473 RVA: 0x002795B8 File Offset: 0x002777B8
		public void Deform(Vector2 worldPosition, Vector2 amount, float deltaTime, Matrix transformMatrix)
		{
			Vector2 pos = Vector2.Transform(worldPosition, transformMatrix);
			Point deformIndex = new Point((int)(pos.X * (float)(base.Resolution.X - 1)), (int)(pos.Y * (float)(base.Resolution.Y - 1)));
			if (deformIndex.X < 0 || deformIndex.Y < 0)
			{
				return;
			}
			if (deformIndex.X >= base.Resolution.X || deformIndex.Y >= base.Resolution.Y)
			{
				return;
			}
			amount = amount.ClampLength(this.positionalDeformationParams.MaxDeformation);
			float invFalloff = 1f - this.positionalDeformationParams.Falloff;
			for (int x = 0; x < base.Resolution.X; x++)
			{
				float normalizedDiffX = (float)Math.Abs(x - deformIndex.X) / ((float)base.Resolution.X * 0.5f);
				for (int y = 0; y < base.Resolution.Y; y++)
				{
					float normalizedDiffY = (float)Math.Abs(y - deformIndex.Y) / ((float)base.Resolution.Y * 0.5f);
					Vector2 targetDeformation = amount * MathHelper.Clamp(1f - new Vector2(normalizedDiffX, normalizedDiffY).Length() * this.positionalDeformationParams.Falloff, 0f, 1f);
					Vector2 diff = targetDeformation - base.Deformation[x, y];
					base.Deformation[x, y] += diff.ClampLength(this.positionalDeformationParams.ReactionSpeed) * deltaTime;
				}
			}
		}

		// Token: 0x0600482A RID: 18474 RVA: 0x0027976B File Offset: 0x0027796B
		protected override void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY)
		{
			deformation = base.Deformation;
			multiplier = 1f;
		}

		// Token: 0x04002566 RID: 9574
		public PositionalDeformation.ReactionType Type;

		// Token: 0x02001142 RID: 4418
		public enum ReactionType
		{
			// Token: 0x04005B5B RID: 23387
			ReactToTriggerers
		}
	}
}
