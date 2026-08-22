using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000043 RID: 67
	internal class Decal
	{
		// Token: 0x060009F8 RID: 2552 RVA: 0x0005F770 File Offset: 0x0005D970
		public void Draw(SpriteBatch spriteBatch, Hull hull, float depth)
		{
			if (this.Sprite.Texture == null)
			{
				return;
			}
			Vector2 drawPos = this.position + hull.Rect.Location.ToVector2();
			if (hull.Submarine != null)
			{
				drawPos += hull.Submarine.DrawPosition;
			}
			drawPos.Y = -drawPos.Y;
			spriteBatch.Draw(this.Sprite.Texture, drawPos, new Rectangle?(this.clippedSourceRect), this.Color * this.GetAlpha(), 0f, Vector2.Zero, this.Scale, SpriteEffects.None, depth);
			if (GameMain.DebugDraw && this.affectedSections != null && this.affectedSections.Count > 0)
			{
				Vector2 drawOffset = (hull.Submarine == null) ? Vector2.Zero : hull.Submarine.DrawPosition;
				Point sectionSize = this.affectedSections.First<BackgroundSection>().Rect.Size;
				Rectangle drawPositionRect = new Rectangle((int)(drawOffset.X + (float)hull.Rect.X), (int)(drawOffset.Y + (float)hull.Rect.Y), sectionSize.X, sectionSize.Y);
				foreach (BackgroundSection section in this.affectedSections)
				{
					GUI.DrawRectangle(spriteBatch, new Vector2((float)(drawPositionRect.X + section.Rect.X), (float)(-(float)(drawPositionRect.Y + section.Rect.Y))), new Vector2((float)sectionSize.X, (float)sectionSize.Y), Color.Red, false, 0f, (float)((int)Math.Max(1.5f / Screen.Selected.Cam.Zoom, 1f)));
				}
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x060009F9 RID: 2553 RVA: 0x0005F968 File Offset: 0x0005DB68
		// (set) Token: 0x060009FA RID: 2554 RVA: 0x0005F970 File Offset: 0x0005DB70
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

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x060009FB RID: 2555 RVA: 0x0005F989 File Offset: 0x0005DB89
		public float FadeInTime
		{
			get
			{
				return this.Prefab.FadeInTime;
			}
		}

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x060009FC RID: 2556 RVA: 0x0005F996 File Offset: 0x0005DB96
		public float FadeOutTime
		{
			get
			{
				return this.Prefab.FadeOutTime;
			}
		}

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x060009FD RID: 2557 RVA: 0x0005F9A3 File Offset: 0x0005DBA3
		public float LifeTime
		{
			get
			{
				return this.Prefab.LifeTime;
			}
		}

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x060009FE RID: 2558 RVA: 0x0005F9B0 File Offset: 0x0005DBB0
		// (set) Token: 0x060009FF RID: 2559 RVA: 0x0005F9B8 File Offset: 0x0005DBB8
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

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000A00 RID: 2560 RVA: 0x0005F9D0 File Offset: 0x0005DBD0
		// (set) Token: 0x06000A01 RID: 2561 RVA: 0x0005F9D8 File Offset: 0x0005DBD8
		public Color Color { get; set; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000A02 RID: 2562 RVA: 0x0005F9E4 File Offset: 0x0005DBE4
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

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000A03 RID: 2563 RVA: 0x0005FA6A File Offset: 0x0005DC6A
		// (set) Token: 0x06000A04 RID: 2564 RVA: 0x0005FA72 File Offset: 0x0005DC72
		public Vector2 CenterPosition { get; private set; }

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x0005FA7B File Offset: 0x0005DC7B
		// (set) Token: 0x06000A06 RID: 2566 RVA: 0x0005FA83 File Offset: 0x0005DC83
		public Vector2 NonClampedPosition { get; private set; }

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000A07 RID: 2567 RVA: 0x0005FA8C File Offset: 0x0005DC8C
		// (set) Token: 0x06000A08 RID: 2568 RVA: 0x0005FA94 File Offset: 0x0005DC94
		public int SpriteIndex { get; private set; }

		// Token: 0x06000A09 RID: 2569 RVA: 0x0005FAA0 File Offset: 0x0005DCA0
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

		// Token: 0x06000A0A RID: 2570 RVA: 0x0005FDFC File Offset: 0x0005DFFC
		public void Update(float deltaTime)
		{
			this.fadeTimer += deltaTime;
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x0005FE0C File Offset: 0x0005E00C
		public void ForceRefreshFadeTimer(float val)
		{
			this.cleaned = false;
			this.fadeTimer = val;
		}

		// Token: 0x06000A0C RID: 2572 RVA: 0x0005FE1C File Offset: 0x0005E01C
		public void StopFadeIn()
		{
			this.Color *= this.GetAlpha();
			this.fadeTimer = this.Prefab.FadeInTime;
		}

		// Token: 0x06000A0D RID: 2573 RVA: 0x0005FE46 File Offset: 0x0005E046
		public bool AffectsSection(BackgroundSection section)
		{
			return this.affectedSections != null && this.affectedSections.Contains(section);
		}

		// Token: 0x06000A0E RID: 2574 RVA: 0x0005FE60 File Offset: 0x0005E060
		public void Clean(float val)
		{
			this.cleaned = true;
			float sizeModifier = MathHelper.Clamp(this.Sprite.size.X * this.Sprite.size.Y * this.Scale / 10000f, 1f, 25f);
			this.BaseAlpha -= val * -1f / sizeModifier;
		}

		// Token: 0x06000A0F RID: 2575 RVA: 0x0005FEC8 File Offset: 0x0005E0C8
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

		// Token: 0x0400051E RID: 1310
		public readonly DecalPrefab Prefab;

		// Token: 0x0400051F RID: 1311
		private Vector2 position;

		// Token: 0x04000520 RID: 1312
		private float fadeTimer;

		// Token: 0x04000521 RID: 1313
		public readonly Sprite Sprite;

		// Token: 0x04000522 RID: 1314
		private float baseAlpha = 1f;

		// Token: 0x04000527 RID: 1319
		private readonly HashSet<BackgroundSection> affectedSections;

		// Token: 0x04000528 RID: 1320
		private readonly Hull hull;

		// Token: 0x04000529 RID: 1321
		public readonly float Scale;

		// Token: 0x0400052A RID: 1322
		private Rectangle clippedSourceRect;

		// Token: 0x0400052B RID: 1323
		private bool cleaned;
	}
}
