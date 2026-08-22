using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000434 RID: 1076
	internal class JointBendDeformation : SpriteDeformation
	{
		// Token: 0x1700124C RID: 4684
		// (get) Token: 0x06004803 RID: 18435 RVA: 0x00278E1B File Offset: 0x0027701B
		// (set) Token: 0x06004804 RID: 18436 RVA: 0x00278E23 File Offset: 0x00277023
		public float BendRight
		{
			get
			{
				return this.bendRight;
			}
			set
			{
				this.bendRight = MathHelper.Clamp(value, -this.MaxRotationInRadians, this.MaxRotationInRadians);
			}
		}

		// Token: 0x1700124D RID: 4685
		// (get) Token: 0x06004805 RID: 18437 RVA: 0x00278E3E File Offset: 0x0027703E
		// (set) Token: 0x06004806 RID: 18438 RVA: 0x00278E46 File Offset: 0x00277046
		public float BendLeft
		{
			get
			{
				return this.bendLeft;
			}
			set
			{
				this.bendLeft = MathHelper.Clamp(value, -this.MaxRotationInRadians, this.MaxRotationInRadians);
			}
		}

		// Token: 0x1700124E RID: 4686
		// (get) Token: 0x06004807 RID: 18439 RVA: 0x00278E61 File Offset: 0x00277061
		// (set) Token: 0x06004808 RID: 18440 RVA: 0x00278E69 File Offset: 0x00277069
		public float BendUp
		{
			get
			{
				return this.bendUp;
			}
			set
			{
				this.bendUp = MathHelper.Clamp(value, -this.MaxRotationInRadians, this.MaxRotationInRadians);
			}
		}

		// Token: 0x1700124F RID: 4687
		// (get) Token: 0x06004809 RID: 18441 RVA: 0x00278E84 File Offset: 0x00277084
		// (set) Token: 0x0600480A RID: 18442 RVA: 0x00278E8C File Offset: 0x0027708C
		public float BendDown
		{
			get
			{
				return this.bendDown;
			}
			set
			{
				this.bendDown = MathHelper.Clamp(value, -this.MaxRotationInRadians, this.MaxRotationInRadians);
			}
		}

		// Token: 0x17001250 RID: 4688
		// (get) Token: 0x0600480B RID: 18443 RVA: 0x00278EA7 File Offset: 0x002770A7
		private float MaxRotationInRadians
		{
			get
			{
				return MathHelper.ToRadians(base.Params.MaxRotation);
			}
		}

		// Token: 0x0600480C RID: 18444 RVA: 0x00278EBC File Offset: 0x002770BC
		public JointBendDeformation(XElement element) : base(element, new JointBendDeformationParams(element))
		{
		}

		// Token: 0x0600480D RID: 18445 RVA: 0x00278F35 File Offset: 0x00277135
		protected override void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY = false)
		{
			deformation = base.Deformation;
			multiplier = 1f;
		}

		// Token: 0x0600480E RID: 18446 RVA: 0x00278F48 File Offset: 0x00277148
		public override void Update(float deltaTime)
		{
			Vector2 normalizedPos = Vector2.Zero;
			for (int x = 0; x < base.Resolution.X; x++)
			{
				normalizedPos.X = (float)x / (float)(base.Resolution.X - 1);
				for (int y = 0; y < base.Resolution.Y; y++)
				{
					normalizedPos.Y = (float)y / (float)(base.Resolution.Y - 1);
					base.Deformation[x, y] = Vector2.Zero;
					if (Math.Abs(this.BendLeft) > 0.001f)
					{
						float strength = 1f - normalizedPos.X;
						strength = (strength - 0.5f) * 2f;
						if (strength > 0f)
						{
							Vector2 rotatedP = JointBendDeformation.RotatePointAroundTarget(normalizedPos, this.BendLeftRefPos, this.BendLeft * strength * base.Params.Strength);
							Vector2 offset = rotatedP - normalizedPos;
							offset.X *= this.Scale.Y / this.Scale.X;
							base.Deformation[x, y] += offset;
						}
					}
					if (Math.Abs(this.BendRight) > 0.001f)
					{
						float strength2 = normalizedPos.X;
						strength2 = (strength2 - 0.5f) * 2f;
						if (strength2 > 0f)
						{
							Vector2 rotatedP2 = JointBendDeformation.RotatePointAroundTarget(normalizedPos, this.BendRightRefPos, this.BendRight * strength2 * base.Params.Strength);
							Vector2 offset2 = rotatedP2 - normalizedPos;
							offset2.X *= this.Scale.Y / this.Scale.X;
							base.Deformation[x, y] += offset2;
						}
					}
					if (Math.Abs(this.BendUp) > 0.001f)
					{
						float strength3 = 1f - normalizedPos.Y;
						strength3 = (strength3 - 0.5f) * 2f;
						if (strength3 > 0f)
						{
							Vector2 rotatedP3 = JointBendDeformation.RotatePointAroundTarget(normalizedPos, this.BendUpRefPos, this.BendUp * strength3 * base.Params.Strength);
							Vector2 offset3 = rotatedP3 - normalizedPos;
							offset3.Y *= this.Scale.X / this.Scale.Y;
							base.Deformation[x, y] += offset3;
						}
					}
					if (Math.Abs(this.BendDown) > 0.001f)
					{
						float strength4 = normalizedPos.Y;
						strength4 = (strength4 - 0.5f) * 2f;
						if (strength4 > 0f)
						{
							Vector2 rotatedP4 = JointBendDeformation.RotatePointAroundTarget(normalizedPos, this.BendDownRefPos, this.BendDown * strength4 * base.Params.Strength);
							Vector2 offset4 = rotatedP4 - normalizedPos;
							offset4.Y *= this.Scale.X / this.Scale.Y;
							base.Deformation[x, y] += offset4;
						}
					}
				}
			}
		}

		// Token: 0x0600480F RID: 18447 RVA: 0x0027926C File Offset: 0x0027746C
		public static Vector2 RotatePointAroundTarget(Vector2 point, Vector2 target, float angle)
		{
			return JointBendDeformation.RotatePointAroundTarget(point, target, (float)Math.Sin((double)angle), (float)Math.Cos((double)angle));
		}

		// Token: 0x06004810 RID: 18448 RVA: 0x00279288 File Offset: 0x00277488
		public static Vector2 RotatePointAroundTarget(Vector2 point, Vector2 target, float sin, float cos)
		{
			Vector2 dir = point - target;
			float x = cos * dir.X - sin * dir.Y + target.X;
			float y = sin * dir.X + cos * dir.Y + target.Y;
			return new Vector2(x, y);
		}

		// Token: 0x04002555 RID: 9557
		private float bendRight;

		// Token: 0x04002556 RID: 9558
		public Vector2 BendRightRefPos = new Vector2(1f, 0.5f);

		// Token: 0x04002557 RID: 9559
		private float bendLeft;

		// Token: 0x04002558 RID: 9560
		public Vector2 BendLeftRefPos = new Vector2(0f, 0.5f);

		// Token: 0x04002559 RID: 9561
		private float bendUp;

		// Token: 0x0400255A RID: 9562
		public Vector2 BendUpRefPos = new Vector2(0.5f, 0f);

		// Token: 0x0400255B RID: 9563
		private float bendDown;

		// Token: 0x0400255C RID: 9564
		public Vector2 BendDownRefPos = new Vector2(0.5f, 1f);

		// Token: 0x0400255D RID: 9565
		public Vector2 Scale = Vector2.Zero;
	}
}
