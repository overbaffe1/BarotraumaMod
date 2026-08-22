using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000086 RID: 134
	public class GUIDragHandle : GUIComponent
	{
		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x0600128F RID: 4751 RVA: 0x000B646E File Offset: 0x000B466E
		public bool Dragging
		{
			get
			{
				return this.dragStarted;
			}
		}

		// Token: 0x06001290 RID: 4752 RVA: 0x000B6476 File Offset: 0x000B4676
		public GUIDragHandle(RectTransform rectT, RectTransform elementToMove, string style = "GUIDragIndicator") : base(style, rectT)
		{
			this.enabled = true;
			this.elementToMove = elementToMove;
			this.DragArea = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06001291 RID: 4753 RVA: 0x000B64A8 File Offset: 0x000B46A8
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			base.Update(deltaTime);
			if (this.enabled)
			{
				if (this.dragStarted)
				{
					Point moveAmount = (PlayerInput.MousePosition - this.dragStart).ToPoint() - this.elementToMove.ScreenSpaceOffset;
					Rectangle rect = this.elementToMove.Rect;
					rect.Location += moveAmount;
					moveAmount.X += Math.Max(this.DragArea.X - rect.X, 0);
					moveAmount.X -= Math.Max(rect.Right - this.DragArea.Right, 0);
					moveAmount.Y += Math.Max(this.DragArea.Y - rect.Y, 0);
					moveAmount.Y -= Math.Max(rect.Bottom - this.DragArea.Bottom, 0);
					if (moveAmount != Point.Zero)
					{
						this.elementToMove.ScreenSpaceOffset += moveAmount;
					}
					bool isPositionValid = this.ValidatePosition == null || this.ValidatePosition(this.elementToMove);
					if (!PlayerInput.PrimaryMouseButtonHeld())
					{
						if (!isPositionValid)
						{
							this.elementToMove.ScreenSpaceOffset = this.originalOffset;
							GUIComponent guicomponent = this.elementToMove.GUIComponent;
							if (guicomponent != null)
							{
								guicomponent.Flash(null, 1.5f, false, false, null);
							}
							SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
						}
						this.dragStarted = false;
					}
				}
				else if (this.Rect.Contains(PlayerInput.MousePosition) && this.CanBeFocused && this.Enabled && GUI.IsMouseOn(this) && !(GUI.MouseOn is GUIButton))
				{
					this.State = (this.Selected ? GUIComponent.ComponentState.HoverSelected : GUIComponent.ComponentState.Hover);
					if (PlayerInput.PrimaryMouseButtonDown())
					{
						this.originalOffset = this.elementToMove.ScreenSpaceOffset;
						this.dragStart = PlayerInput.MousePosition - this.elementToMove.ScreenSpaceOffset.ToVector2();
						this.dragStarted = true;
					}
				}
				else if (!this.ExternalHighlight)
				{
					this.State = (this.Selected ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None);
				}
				else
				{
					this.State = GUIComponent.ComponentState.Hover;
				}
			}
			foreach (GUIComponent child in base.Children)
			{
				if (!(child is GUIButton))
				{
					child.State = this.State;
					child.Enabled = this.enabled;
				}
			}
		}

		// Token: 0x0400094E RID: 2382
		private readonly RectTransform elementToMove;

		// Token: 0x0400094F RID: 2383
		private Point originalOffset;

		// Token: 0x04000950 RID: 2384
		private Vector2 dragStart;

		// Token: 0x04000951 RID: 2385
		private bool dragStarted;

		// Token: 0x04000952 RID: 2386
		public Rectangle DragArea;

		// Token: 0x04000953 RID: 2387
		public Func<RectTransform, bool> ValidatePosition;
	}
}
