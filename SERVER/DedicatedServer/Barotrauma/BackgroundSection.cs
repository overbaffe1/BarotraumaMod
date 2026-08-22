using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000230 RID: 560
	internal class BackgroundSection
	{
		// Token: 0x17000B0C RID: 2828
		// (get) Token: 0x06002680 RID: 9856 RVA: 0x000FC631 File Offset: 0x000FA831
		// (set) Token: 0x06002681 RID: 9857 RVA: 0x000FC639 File Offset: 0x000FA839
		public float ColorStrength { get; protected set; }

		// Token: 0x17000B0D RID: 2829
		// (get) Token: 0x06002682 RID: 9858 RVA: 0x000FC642 File Offset: 0x000FA842
		// (set) Token: 0x06002683 RID: 9859 RVA: 0x000FC64C File Offset: 0x000FA84C
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

		// Token: 0x06002684 RID: 9860 RVA: 0x000FC6A4 File Offset: 0x000FA8A4
		public BackgroundSection(Rectangle rect, ushort index, ushort rowIndex)
		{
			this.Rect = rect;
			this.Index = index;
			this.ColorStrength = 0f;
			this.RowIndex = rowIndex;
			this.Noise = new Vector2(PerlinNoise.GetPerlin((float)this.Rect.X / 1000f, (float)this.Rect.Y / 1000f), PerlinNoise.GetPerlin((float)this.Rect.Y / 1000f + 0.5f, (float)this.Rect.X / 1000f + 0.5f));
			this.Color = (this.DirtColor = Color.Lerp(new Color(10, 10, 10, 100), new Color(54, 57, 28, 200), this.Noise.X));
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x000FC77C File Offset: 0x000FA97C
		public BackgroundSection(Rectangle rect, ushort index, float colorStrength, Color color, ushort rowIndex)
		{
			this.Rect = rect;
			this.Index = index;
			this.ColorStrength = colorStrength;
			this.Color = color;
			this.RowIndex = rowIndex;
			this.Noise = new Vector2(PerlinNoise.GetPerlin((float)this.Rect.X / 1000f, (float)this.Rect.Y / 1000f), PerlinNoise.GetPerlin((float)this.Rect.Y / 1000f + 0.5f, (float)this.Rect.X / 1000f + 0.5f));
			this.DirtColor = Color.Lerp(new Color(10, 10, 10, 100), new Color(54, 57, 28, 200), this.Noise.X);
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x000FC850 File Offset: 0x000FAA50
		public bool SetColor(Color color)
		{
			if (this.Color == color)
			{
				return false;
			}
			this.Color = color;
			return true;
		}

		// Token: 0x06002687 RID: 9863 RVA: 0x000FC86C File Offset: 0x000FAA6C
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

		// Token: 0x06002688 RID: 9864 RVA: 0x000FC897 File Offset: 0x000FAA97
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

		// Token: 0x06002689 RID: 9865 RVA: 0x000FC8D4 File Offset: 0x000FAAD4
		public Color GetStrengthAdjustedColor()
		{
			return this.Color * this.ColorStrength;
		}

		// Token: 0x040012D7 RID: 4823
		public Rectangle Rect;

		// Token: 0x040012D8 RID: 4824
		public ushort Index;

		// Token: 0x040012D9 RID: 4825
		public ushort RowIndex;

		// Token: 0x040012DA RID: 4826
		private Vector4 colorVector4;

		// Token: 0x040012DB RID: 4827
		private Color color;

		// Token: 0x040012DC RID: 4828
		public readonly Vector2 Noise;

		// Token: 0x040012DD RID: 4829
		public readonly Color DirtColor;
	}
}
