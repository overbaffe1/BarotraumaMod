using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016C RID: 364
	internal class Decal
	{
		// Token: 0x1700085B RID: 2139
		// (get) Token: 0x06001D61 RID: 7521 RVA: 0x000D19CE File Offset: 0x000CFBCE
		// (set) Token: 0x06001D62 RID: 7522 RVA: 0x000D19D6 File Offset: 0x000CFBD6
		public float FadeTimer
		{
			get
			{
				return this.fadeTimer;
			}
			set
			{
				this.fadeTimer = MathHelper.Clamp(value, 0f, this.LifeTime);
			}
		}

		// Token: 0x1700085C RID: 2140
		// (get) Token: 0x06001D63 RID: 7523 RVA: 0x000D19EF File Offset: 0x000CFBEF
		public float FadeInTime
		{
			get
			{
				return this.Prefab.FadeInTime;
			}
		}

		// Token: 0x1700085D RID: 2141
		// (get) Token: 0x06001D64 RID: 7524 RVA: 0x000D19FC File Offset: 0x000CFBFC
		public float FadeOutTime
		{
			get
			{
				return this.Prefab.FadeOutTime;
			}
		}

		// Token: 0x1700085E RID: 2142
		// (get) Token: 0x06001D65 RID: 7525 RVA: 0x000D1A09 File Offset: 0x000CFC09
		public float LifeTime
		{
			get
			{
				return this.Prefab.LifeTime;
			}
		}

		// Token: 0x1700085F RID: 2143
		// (get) Token: 0x06001D66 RID: 7526 RVA: 0x000D1A16 File Offset: 0x000CFC16
		// (set) Token: 0x06001D67 RID: 7527 RVA: 0x000D1A1E File Offset: 0x000CFC1E
		public float BaseAlpha
		{
			get
			{
				return this.baseAlpha;
			}
			set
			{
				this.baseAlpha = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000860 RID: 2144
		// (get) Token: 0x06001D68 RID: 7528 RVA: 0x000D1A36 File Offset: 0x000CFC36
		// (set) Token: 0x06001D69 RID: 7529 RVA: 0x000D1A3E File Offset: 0x000CFC3E
		public Color Color { get; set; }

		// Token: 0x17000861 RID: 2145
		// (get) Token: 0x06001D6A RID: 7530 RVA: 0x000D1A48 File Offset: 0x000CFC48
		public Vector2 WorldPosition
		{
			get
			{
				Vector2 worldPos = this.position + this.clippedSourceRect.Size.ToVector2() / 2f * this.Scale + this.hull.Rect.Location.ToVector2();
				if (this.hull.Submarine != null)
				{
					worldPos += this.hull.Submarine.DrawPosition;
				}
				return worldPos;
			}
		}

		// Token: 0x17000862 RID: 2146
		// (get) Token: 0x06001D6B RID: 7531 RVA: 0x000D1ACE File Offset: 0x000CFCCE
		// (set) Token: 0x06001D6C RID: 7532 RVA: 0x000D1AD6 File Offset: 0x000CFCD6
		public Vector2 CenterPosition { get; private set; }

		// Token: 0x17000863 RID: 2147
		// (get) Token: 0x06001D6D RID: 7533 RVA: 0x000D1ADF File Offset: 0x000CFCDF
		// (set) Token: 0x06001D6E RID: 7534 RVA: 0x000D1AE7 File Offset: 0x000CFCE7
		public Vector2 NonClampedPosition { get; private set; }

		// Token: 0x17000864 RID: 2148
		// (get) Token: 0x06001D6F RID: 7535 RVA: 0x000D1AF0 File Offset: 0x000CFCF0
		// (set) Token: 0x06001D70 RID: 7536 RVA: 0x000D1AF8 File Offset: 0x000CFCF8
		public int SpriteIndex { get; private set; }

		// Token: 0x06001D71 RID: 7537 RVA: 0x000D1B04 File Offset: 0x000CFD04
		public Decal(DecalPrefab prefab, float scale, Vector2 worldPosition, Hull hull, int? spriteIndex = null)
		{
			this.Prefab = prefab;
			this.hull = hull;
			this.NonClampedPosition = (this.position = worldPosition - hull.WorldRect.Location.ToVector2());
			Vector2 drawPos = this.position + hull.Rect.Location.ToVector2();
			this.SpriteIndex = (spriteIndex ?? Rand.Range(0, prefab.Sprites.Count, Rand.RandSync.Unsynced));
			this.Sprite = prefab.Sprites[this.SpriteIndex];
			this.Color = prefab.Color;
			Rectangle drawRect = new Rectangle((int)(drawPos.X - this.Sprite.size.X / 2f * scale), (int)(drawPos.Y + this.Sprite.size.Y / 2f * scale), (int)(this.Sprite.size.X * scale), (int)(this.Sprite.size.Y * scale));
			Rectangle overFlowAmount = new Rectangle((int)Math.Max((float)(hull.Rect.X - drawRect.X), 0f), (int)Math.Max((float)(drawRect.Y - hull.Rect.Y), 0f), (int)Math.Max((float)(drawRect.Right - hull.Rect.Right), 0f), (int)Math.Max((float)(hull.Rect.Y - hull.Rect.Height - (drawRect.Y - drawRect.Height)), 0f));
			this.clippedSourceRect = new Rectangle(this.Sprite.SourceRect.X + (int)((float)overFlowAmount.X / scale), this.Sprite.SourceRect.Y + (int)((float)overFlowAmount.Y / scale), this.Sprite.SourceRect.Width - (int)((float)(overFlowAmount.X + overFlowAmount.Width) / scale), this.Sprite.SourceRect.Height - (int)((float)(overFlowAmount.Y + overFlowAmount.Height) / scale));
			this.CenterPosition = this.position;
			this.position -= new Vector2(this.Sprite.size.X / 2f * scale - (float)overFlowAmount.X, -this.Sprite.size.Y / 2f * scale + (float)overFlowAmount.Y);
			this.Scale = scale;
			foreach (BackgroundSection section in hull.GetBackgroundSectionsViaContaining(new Rectangle((int)this.position.X, (int)this.position.Y - drawRect.Height, drawRect.Width, drawRect.Height)))
			{
				if (this.affectedSections == null)
				{
					this.affectedSections = new HashSet<BackgroundSection>();
				}
				this.affectedSections.Add(section);
			}
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x000D1E60 File Offset: 0x000D0060
		public void Update(float deltaTime)
		{
			this.fadeTimer += deltaTime;
		}

		// Token: 0x06001D73 RID: 7539 RVA: 0x000D1E70 File Offset: 0x000D0070
		public void ForceRefreshFadeTimer(float val)
		{
			this.cleaned = false;
			this.fadeTimer = val;
		}

		// Token: 0x06001D74 RID: 7540 RVA: 0x000D1E80 File Offset: 0x000D0080
		public void StopFadeIn()
		{
			this.Color *= this.GetAlpha();
			this.fadeTimer = this.Prefab.FadeInTime;
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x000D1EAA File Offset: 0x000D00AA
		public bool AffectsSection(BackgroundSection section)
		{
			return this.affectedSections != null && this.affectedSections.Contains(section);
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x000D1EC4 File Offset: 0x000D00C4
		public void Clean(float val)
		{
			this.cleaned = true;
			float sizeModifier = MathHelper.Clamp(this.Sprite.size.X * this.Sprite.size.Y * this.Scale / 10000f, 1f, 25f);
			this.BaseAlpha -= val * -1f / sizeModifier;
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x000D1F2C File Offset: 0x000D012C
		private float GetAlpha()
		{
			if (this.fadeTimer < this.Prefab.FadeInTime && !this.cleaned)
			{
				return this.BaseAlpha * this.fadeTimer / this.Prefab.FadeInTime;
			}
			if (this.cleaned || this.fadeTimer > this.Prefab.LifeTime - this.Prefab.FadeOutTime)
			{
				return this.BaseAlpha * Math.Min((this.Prefab.LifeTime - this.fadeTimer) / this.Prefab.FadeOutTime, 1f);
			}
			return this.BaseAlpha;
		}

		// Token: 0x04000D42 RID: 3394
		public readonly DecalPrefab Prefab;

		// Token: 0x04000D43 RID: 3395
		private Vector2 position;

		// Token: 0x04000D44 RID: 3396
		private float fadeTimer;

		// Token: 0x04000D45 RID: 3397
		public readonly Sprite Sprite;

		// Token: 0x04000D46 RID: 3398
		private float baseAlpha = 1f;

		// Token: 0x04000D4B RID: 3403
		private readonly HashSet<BackgroundSection> affectedSections;

		// Token: 0x04000D4C RID: 3404
		private readonly Hull hull;

		// Token: 0x04000D4D RID: 3405
		public readonly float Scale;

		// Token: 0x04000D4E RID: 3406
		private Rectangle clippedSourceRect;

		// Token: 0x04000D4F RID: 3407
		private bool cleaned;
	}
}
