using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B6 RID: 1462
	internal class Ladder : ItemComponent, IDrawableComponent
	{
		// Token: 0x170016E6 RID: 5862
		// (get) Token: 0x06005AF9 RID: 23289 RVA: 0x002EA30E File Offset: 0x002E850E
		public float BackgroundSpriteDepth
		{
			get
			{
				return this.item.GetDrawDepth() + 0.05f;
			}
		}

		// Token: 0x170016E7 RID: 5863
		// (get) Token: 0x06005AFA RID: 23290 RVA: 0x002EA321 File Offset: 0x002E8521
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005AFB RID: 23291 RVA: 0x002EA328 File Offset: 0x002E8528
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.backgroundSprite == null)
			{
				return;
			}
			Sprite sprite = this.backgroundSprite;
			Vector2 position = new Vector2(this.item.DrawPosition.X - (float)(this.item.Rect.Width / 2) * this.item.Scale, -(this.item.DrawPosition.Y + (float)(this.item.Rect.Height / 2))) - this.backgroundSprite.Origin * this.item.Scale;
			Vector2 targetSize = new Vector2(this.backgroundSprite.size.X * this.item.Scale, (float)this.item.Rect.Height);
			float rotation = 0f;
			Color? color = new Color?(overrideColor ?? this.item.Color);
			Vector2? textureScale = new Vector2?(Vector2.One * this.item.Scale);
			float? depth = new float?(this.BackgroundSpriteDepth);
			sprite.DrawTiled(spriteBatch, position, targetSize, rotation, null, color, null, textureScale, depth);
		}

		// Token: 0x170016E8 RID: 5864
		// (get) Token: 0x06005AFC RID: 23292 RVA: 0x002EA462 File Offset: 0x002E8662
		public static List<Ladder> List { get; } = new List<Ladder>();

		// Token: 0x06005AFD RID: 23293 RVA: 0x002EA469 File Offset: 0x002E8669
		public Ladder(Item item, ContentXElement element) : base(item, element)
		{
			this.InitProjSpecific(element);
			Ladder.List.Add(this);
		}

		// Token: 0x06005AFE RID: 23294 RVA: 0x002EA488 File Offset: 0x002E8688
		private void InitProjSpecific(ContentXElement element)
		{
			ContentXElement backgroundSpriteElement = element.GetChildElement("backgroundsprite");
			ContentXElement contentXElement = null;
			if (backgroundSpriteElement != contentXElement)
			{
				this.backgroundSprite = new Sprite(backgroundSpriteElement, "", "", false, 1f);
			}
		}

		// Token: 0x06005AFF RID: 23295 RVA: 0x002EA4CA File Offset: 0x002E86CA
		public override bool Select(Character character)
		{
			if (character == null || character.LockHands || character.Removed)
			{
				return false;
			}
			if (!character.CanClimb)
			{
				return false;
			}
			character.AnimController.StartClimbing();
			return true;
		}

		// Token: 0x06005B00 RID: 23296 RVA: 0x002EA4F7 File Offset: 0x002E86F7
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.RemoveProjSpecific();
			Ladder.List.Remove(this);
		}

		// Token: 0x06005B01 RID: 23297 RVA: 0x002EA511 File Offset: 0x002E8711
		private void RemoveProjSpecific()
		{
			Sprite sprite = this.backgroundSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.backgroundSprite = null;
		}

		// Token: 0x04002E63 RID: 11875
		private Sprite backgroundSprite;
	}
}
