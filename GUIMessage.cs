using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200008C RID: 140
	internal class GUIMessage
	{
		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x06001365 RID: 4965 RVA: 0x000BAD46 File Offset: 0x000B8F46
		public string Text
		{
			get
			{
				return this.coloredText.Text;
			}
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x06001366 RID: 4966 RVA: 0x000BAD53 File Offset: 0x000B8F53
		public Color Color
		{
			get
			{
				return this.coloredText.Color;
			}
		}

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x06001367 RID: 4967 RVA: 0x000BAD60 File Offset: 0x000B8F60
		// (set) Token: 0x06001368 RID: 4968 RVA: 0x000BAD68 File Offset: 0x000B8F68
		public Vector2 Pos
		{
			get
			{
				return this.pos;
			}
			set
			{
				this.pos = value;
			}
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x06001369 RID: 4969 RVA: 0x000BAD71 File Offset: 0x000B8F71
		// (set) Token: 0x0600136A RID: 4970 RVA: 0x000BAD79 File Offset: 0x000B8F79
		public Vector2 Velocity { get; private set; }

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x0600136B RID: 4971 RVA: 0x000BAD82 File Offset: 0x000B8F82
		public Vector2 Size
		{
			get
			{
				return this.size;
			}
		}

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x0600136C RID: 4972 RVA: 0x000BAD8A File Offset: 0x000B8F8A
		public float LifeTime
		{
			get
			{
				return this.lifeTime;
			}
		}

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x0600136D RID: 4973 RVA: 0x000BAD92 File Offset: 0x000B8F92
		// (set) Token: 0x0600136E RID: 4974 RVA: 0x000BAD9A File Offset: 0x000B8F9A
		public GUIFont Font { get; private set; }

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x0600136F RID: 4975 RVA: 0x000BADA3 File Offset: 0x000B8FA3
		// (set) Token: 0x06001370 RID: 4976 RVA: 0x000BADAB File Offset: 0x000B8FAB
		public Submarine Submarine { get; private set; }

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x06001371 RID: 4977 RVA: 0x000BADB4 File Offset: 0x000B8FB4
		public Vector2 DrawPos
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.Pos + this.Submarine.DrawPosition;
				}
				return this.Pos;
			}
		}

		// Token: 0x06001372 RID: 4978 RVA: 0x000BADDC File Offset: 0x000B8FDC
		public GUIMessage(string text, Color color, float lifeTime, GUIFont font = null)
		{
			this.coloredText = new ColoredText(text, color, false, false);
			this.lifeTime = lifeTime;
			this.Timer = lifeTime;
			this.size = font.MeasureString(text, false);
			this.Origin = new Vector2(0f, this.size.Y * 0.5f);
			this.Font = font;
		}

		// Token: 0x06001373 RID: 4979 RVA: 0x000BAE4C File Offset: 0x000B904C
		public GUIMessage(string text, Color color, Vector2 position, Vector2 velocity, float lifeTime, Alignment textAlignment = Alignment.Center, GUIFont font = null, Submarine sub = null)
		{
			this.coloredText = new ColoredText(text, color, false, false);
			this.WorldSpace = true;
			this.pos = position;
			this.Timer = lifeTime;
			this.Velocity = velocity;
			this.lifeTime = lifeTime;
			this.Font = font;
			this.size = font.MeasureString(text, false);
			this.Origin = new Vector2((float)((int)(0.5f * this.size.X)), (float)((int)(0.5f * this.size.Y)));
			if (textAlignment.HasFlag(Alignment.Left))
			{
				this.Origin.X = this.Origin.X - this.size.X * 0.5f;
			}
			if (textAlignment.HasFlag(Alignment.Right))
			{
				this.Origin.X = this.Origin.X + this.size.X * 0.5f;
			}
			if (textAlignment.HasFlag(Alignment.Top))
			{
				this.Origin.Y = this.Origin.Y - this.size.Y * 0.5f;
			}
			if (textAlignment.HasFlag(Alignment.Bottom))
			{
				this.Origin.Y = this.Origin.Y + this.size.Y * 0.5f;
			}
			this.Submarine = sub;
		}

		// Token: 0x040009A5 RID: 2469
		private ColoredText coloredText;

		// Token: 0x040009A6 RID: 2470
		private Vector2 pos;

		// Token: 0x040009A7 RID: 2471
		private float lifeTime;

		// Token: 0x040009A8 RID: 2472
		private Vector2 size;

		// Token: 0x040009A9 RID: 2473
		public readonly bool WorldSpace;

		// Token: 0x040009AB RID: 2475
		public Vector2 Origin;

		// Token: 0x040009AC RID: 2476
		public float Timer;
	}
}
