using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000315 RID: 789
	internal class BackgroundSection
	{
		// Token: 0x17001086 RID: 4230
		// (get) Token: 0x06003EFE RID: 16126 RVA: 0x002350CC File Offset: 0x002332CC
		// (set) Token: 0x06003EFF RID: 16127 RVA: 0x002350D4 File Offset: 0x002332D4
		public float ColorStrength { get; protected set; }

		// Token: 0x17001087 RID: 4231
		// (get) Token: 0x06003F00 RID: 16128 RVA: 0x002350DD File Offset: 0x002332DD
		// (set) Token: 0x06003F01 RID: 16129 RVA: 0x002350E8 File Offset: 0x002332E8
		public Color Color
		{
			get
			{
				return this.color;
			}
			protected set
			{
				this.color = value;
				this.colorVector4 = new Vector4((float)value.R / 255f, (float)value.G / 255f, (float)value.B / 255f, (float)value.A / 255f);
			}
		}

		// Token: 0x06003F02 RID: 16130 RVA: 0x00235140 File Offset: 0x00233340
		public BackgroundSection(Rectangle rect, ushort index, ushort rowIndex)
		{
			this.Rect = rect;
			this.Index = index;
			this.ColorStrength = 0f;
			this.RowIndex = rowIndex;
			this.Noise = new Vector2(PerlinNoise.GetPerlin((float)this.Rect.X / 1000f, (float)this.Rect.Y / 1000f), PerlinNoise.GetPerlin((float)this.Rect.Y / 1000f + 0.5f, (float)this.Rect.X / 1000f + 0.5f));
			this.Color = (this.DirtColor = Color.Lerp(new Color(10, 10, 10, 100), new Color(54, 57, 28, 200), this.Noise.X));
			PrefabCollection<GrimeSprite> grimeSprites = DecalManager.GrimeSprites;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted("GrimeSprite");
			defaultInterpolatedStringHandler.AppendFormatted<int>((int)index % DecalManager.GrimeSpriteCount);
			this.GrimeSprite = grimeSprites[defaultInterpolatedStringHandler.ToStringAndClear()].Sprite;
		}

		// Token: 0x06003F03 RID: 16131 RVA: 0x00235258 File Offset: 0x00233458
		public BackgroundSection(Rectangle rect, ushort index, float colorStrength, Color color, ushort rowIndex)
		{
			this.Rect = rect;
			this.Index = index;
			this.ColorStrength = colorStrength;
			this.Color = color;
			this.RowIndex = rowIndex;
			this.Noise = new Vector2(PerlinNoise.GetPerlin((float)this.Rect.X / 1000f, (float)this.Rect.Y / 1000f), PerlinNoise.GetPerlin((float)this.Rect.Y / 1000f + 0.5f, (float)this.Rect.X / 1000f + 0.5f));
			this.DirtColor = Color.Lerp(new Color(10, 10, 10, 100), new Color(54, 57, 28, 200), this.Noise.X);
			PrefabCollection<GrimeSprite> grimeSprites = DecalManager.GrimeSprites;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted("GrimeSprite");
			defaultInterpolatedStringHandler.AppendFormatted<int>((int)index % DecalManager.GrimeSpriteCount);
			this.GrimeSprite = grimeSprites[defaultInterpolatedStringHandler.ToStringAndClear()].Sprite;
		}

		// Token: 0x06003F04 RID: 16132 RVA: 0x0023536B File Offset: 0x0023356B
		public bool SetColor(Color color)
		{
			if (this.Color == color)
			{
				return false;
			}
			this.Color = color;
			return true;
		}

		// Token: 0x06003F05 RID: 16133 RVA: 0x00235388 File Offset: 0x00233588
		public float SetColorStrength(float colorStrength)
		{
			if (this.ColorStrength == colorStrength)
			{
				return -1f;
			}
			float previous = this.ColorStrength;
			this.ColorStrength = colorStrength;
			return previous;
		}

		// Token: 0x06003F06 RID: 16134 RVA: 0x002353B3 File Offset: 0x002335B3
		public bool LerpColor(Color to, float amount)
		{
			if (this.Color == to)
			{
				return false;
			}
			this.colorVector4 = Vector4.Lerp(this.colorVector4, to.ToVector4(), amount);
			this.color = new Color(this.colorVector4);
			return true;
		}

		// Token: 0x06003F07 RID: 16135 RVA: 0x002353F0 File Offset: 0x002335F0
		public Color GetStrengthAdjustedColor()
		{
			return this.Color * this.ColorStrength;
		}

		// Token: 0x040020B6 RID: 8374
		public Rectangle Rect;

		// Token: 0x040020B7 RID: 8375
		public ushort Index;

		// Token: 0x040020B8 RID: 8376
		public ushort RowIndex;

		// Token: 0x040020B9 RID: 8377
		private Vector4 colorVector4;

		// Token: 0x040020BA RID: 8378
		private Color color;

		// Token: 0x040020BB RID: 8379
		public readonly Vector2 Noise;

		// Token: 0x040020BC RID: 8380
		public readonly Color DirtColor;

		// Token: 0x040020BD RID: 8381
		public Sprite GrimeSprite;
	}
}
