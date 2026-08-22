using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C3 RID: 195
	internal class VisualSlot
	{
		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x06001811 RID: 6161 RVA: 0x000EF9D5 File Offset: 0x000EDBD5
		public bool IsHighlighted
		{
			get
			{
				return this.State == GUIComponent.ComponentState.Hover;
			}
		}

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x06001812 RID: 6162 RVA: 0x000EF9E0 File Offset: 0x000EDBE0
		public Rectangle EquipButtonRect
		{
			get
			{
				if (this.IsMoving)
				{
					return VisualSlot.offScreenRect;
				}
				int buttonDir = Math.Sign(this.SubInventoryDir);
				float sizeY = Inventory.UnequippedIndicator.size.Y * Inventory.UIScale;
				Vector2 equipIndicatorPos = new Vector2((float)this.Rect.Left, (float)this.Rect.Center.Y + ((float)(this.Rect.Height / 2) + 15f * Inventory.UIScale) * (float)buttonDir - sizeY / 2f);
				equipIndicatorPos += this.DrawOffset;
				return new Rectangle((int)equipIndicatorPos.X, (int)equipIndicatorPos.Y, this.Rect.Width, (int)sizeY);
			}
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x000EFA94 File Offset: 0x000EDC94
		public VisualSlot(Rectangle rect)
		{
			this.Rect = rect;
			this.InteractRect = rect;
			this.InteractRect.Inflate(5, 5);
			this.State = GUIComponent.ComponentState.None;
			this.Color = Color.White * 0.4f;
		}

		// Token: 0x06001814 RID: 6164 RVA: 0x000EFAEC File Offset: 0x000EDCEC
		public bool MouseOn()
		{
			Rectangle rect = this.InteractRect;
			rect.Location += this.DrawOffset.ToPoint();
			return rect.Contains(PlayerInput.MousePosition);
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x000EFB2C File Offset: 0x000EDD2C
		public void ShowBorderHighlight(Color color, float fadeInDuration, float fadeOutDuration, float scaleUpAmount = 0.5f)
		{
			if (this.highlightCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.highlightCoroutine);
				this.highlightCoroutine = null;
			}
			this.HighlightScaleUpAmount = scaleUpAmount;
			this.currentHighlightState = 0f;
			this.fadeInDuration = fadeInDuration;
			this.fadeOutDuration = fadeOutDuration;
			this.currentHighlightColor = color;
			this.HighlightTimer = 1f;
			this.highlightCoroutine = CoroutineManager.StartCoroutine(this.UpdateBorderHighlight(), "");
		}

		// Token: 0x06001816 RID: 6166 RVA: 0x000EFB9C File Offset: 0x000EDD9C
		private IEnumerable<CoroutineStatus> UpdateBorderHighlight()
		{
			VisualSlot.<UpdateBorderHighlight>d__29 <UpdateBorderHighlight>d__ = new VisualSlot.<UpdateBorderHighlight>d__29(-2);
			<UpdateBorderHighlight>d__.<>4__this = this;
			return <UpdateBorderHighlight>d__;
		}

		// Token: 0x06001817 RID: 6167 RVA: 0x000EFBAC File Offset: 0x000EDDAC
		public void MoveBorderHighlight(VisualSlot newSlot)
		{
			if (this.highlightCoroutine == null)
			{
				return;
			}
			CoroutineManager.StopCoroutines(this.highlightCoroutine);
			this.highlightCoroutine = null;
			newSlot.HighlightScaleUpAmount = this.HighlightScaleUpAmount;
			newSlot.currentHighlightState = this.currentHighlightState;
			newSlot.fadeInDuration = this.fadeInDuration;
			newSlot.fadeOutDuration = this.fadeOutDuration;
			newSlot.currentHighlightColor = this.currentHighlightColor;
			newSlot.highlightCoroutine = CoroutineManager.StartCoroutine(newSlot.UpdateBorderHighlight(), "");
		}

		// Token: 0x04000C69 RID: 3177
		public Rectangle Rect;

		// Token: 0x04000C6A RID: 3178
		public Rectangle InteractRect;

		// Token: 0x04000C6B RID: 3179
		public bool Disabled;

		// Token: 0x04000C6C RID: 3180
		public GUIComponent.ComponentState State;

		// Token: 0x04000C6D RID: 3181
		public Vector2 DrawOffset;

		// Token: 0x04000C6E RID: 3182
		public Color Color;

		// Token: 0x04000C6F RID: 3183
		public Color HighlightColor;

		// Token: 0x04000C70 RID: 3184
		public float HighlightScaleUpAmount;

		// Token: 0x04000C71 RID: 3185
		private CoroutineHandle highlightCoroutine;

		// Token: 0x04000C72 RID: 3186
		public float HighlightTimer;

		// Token: 0x04000C73 RID: 3187
		public Sprite SlotSprite;

		// Token: 0x04000C74 RID: 3188
		public int InventoryKeyIndex = -1;

		// Token: 0x04000C75 RID: 3189
		public int SubInventoryDir = -1;

		// Token: 0x04000C76 RID: 3190
		public float QuickUseTimer;

		// Token: 0x04000C77 RID: 3191
		public LocalizedString QuickUseButtonToolTip;

		// Token: 0x04000C78 RID: 3192
		public bool IsMoving;

		// Token: 0x04000C79 RID: 3193
		private static Rectangle offScreenRect = new Rectangle(new Point(-1000, 0), Point.Zero);

		// Token: 0x04000C7A RID: 3194
		public GUIComponent.ComponentState EquipButtonState;

		// Token: 0x04000C7B RID: 3195
		private float currentHighlightState;

		// Token: 0x04000C7C RID: 3196
		private float fadeInDuration;

		// Token: 0x04000C7D RID: 3197
		private float fadeOutDuration;

		// Token: 0x04000C7E RID: 3198
		private Color currentHighlightColor;
	}
}
