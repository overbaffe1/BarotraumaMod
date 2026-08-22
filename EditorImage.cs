using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000106 RID: 262
	[NullableContext(1)]
	[Nullable(0)]
	internal class EditorImage
	{
		// Token: 0x06002484 RID: 9348 RVA: 0x00171BC4 File Offset: 0x0016FDC4
		public EditorImage(string path, Vector2 pos)
		{
			this.Image = Sprite.LoadTexture(path, false, null);
			this.ImagePath = path;
			this.Position = pos;
			this.UpdateRectangle();
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x00171C1A File Offset: 0x0016FE1A
		public bool IsMouseOn()
		{
			return this.Bounds.Contains(this.GetMousePos());
		}

		// Token: 0x06002486 RID: 9350 RVA: 0x00171C30 File Offset: 0x0016FE30
		public Vector2 GetMousePos()
		{
			EditorImage.DrawTargetType drawTarget = this.DrawTarget;
			if (drawTarget == EditorImage.DrawTargetType.Camera)
			{
				return PlayerInput.MousePosition;
			}
			if (drawTarget != EditorImage.DrawTargetType.World)
			{
				return PlayerInput.MousePosition;
			}
			Vector2 pos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
			pos.Y = -pos.Y;
			return pos;
		}

		// Token: 0x06002487 RID: 9351 RVA: 0x00171C80 File Offset: 0x0016FE80
		public void Update(float deltaTime)
		{
			if (!this.Selected)
			{
				return;
			}
			if (this.widgets.Values.Any((Widget w) => w.IsSelected))
			{
				return;
			}
			if (PlayerInput.PrimaryMouseButtonDown() && !this.disableMove && this.IsMouseOn())
			{
				this.isDragging = true;
			}
			if (this.isDragging)
			{
				Camera cam = Screen.Selected.Cam;
				if (PlayerInput.MouseSpeed != Vector2.Zero)
				{
					Vector2 mouseSpeed = PlayerInput.MouseSpeed;
					if (this.DrawTarget == EditorImage.DrawTargetType.World)
					{
						mouseSpeed /= cam.Zoom;
					}
					this.Position += mouseSpeed;
					this.UpdateRectangle();
				}
			}
			if (PlayerInput.KeyDown(Keys.OemPlus) || PlayerInput.KeyDown(Keys.Up))
			{
				this.Opacity += 0.01f;
			}
			if (PlayerInput.KeyDown(Keys.OemMinus) || PlayerInput.KeyDown(Keys.Down))
			{
				this.Opacity -= 0.01f;
			}
			if (PlayerInput.KeyHit(Keys.D0))
			{
				this.Opacity = 1f;
			}
			this.Opacity = Math.Clamp(this.Opacity, 0f, 1f);
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				this.isDragging = false;
			}
		}

		// Token: 0x06002488 RID: 9352 RVA: 0x00171DCC File Offset: 0x0016FFCC
		private void DrawWidgets(SpriteBatch spriteBatch)
		{
			float widgetSize = (this.Image == null) ? 100f : ((float)Math.Max(this.Image.Width, this.Image.Height) / 2f);
			int width = 3;
			int size = 32;
			if (this.DrawTarget == EditorImage.DrawTargetType.World)
			{
				width = Math.Max(width, (int)((float)width / Screen.Selected.Cam.Zoom));
			}
			Action<float> <>9__3;
			Widget currentWidget = this.GetWidget("transform", size, (float)width, delegate(Widget widget)
			{
				widget.MouseDown += delegate()
				{
					widget.Color = GUIStyle.Green;
					this.prevAngle = this.Rotation;
					this.disableMove = true;
				};
				widget.Deselected += delegate()
				{
					widget.Color = Color.Yellow;
					this.disableMove = false;
				};
				Widget widget2 = widget;
				Action<float> value;
				if ((value = <>9__3) == null)
				{
					value = (<>9__3 = delegate(float deltaTime)
					{
						this.Rotation = this.GetRotationAngle(this.Position) + 1.5707964f;
						float distance = Vector2.Distance(this.Position, this.GetMousePos());
						this.Scale = Math.Abs(distance) / widgetSize;
						if (PlayerInput.IsShiftDown())
						{
							this.Rotation = (float)Math.Round((double)(this.Rotation / 0.7853982f)) * 0.7853982f;
						}
						if (PlayerInput.IsCtrlDown())
						{
							this.Scale = (float)Math.Round((double)(this.Scale / 0.1f)) * 0.1f;
						}
						this.UpdateRectangle();
					});
				}
				widget2.MouseHeld += value;
				widget.PreUpdate += delegate(float deltaTime)
				{
					if (this.DrawTarget != EditorImage.DrawTargetType.World)
					{
						return;
					}
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
					widget.DrawPos = Screen.Selected.Cam.WorldToScreen(widget.DrawPos);
				};
				widget.PostUpdate += delegate(float deltaTime)
				{
					if (this.DrawTarget != EditorImage.DrawTargetType.World)
					{
						return;
					}
					widget.DrawPos = Screen.Selected.Cam.ScreenToWorld(widget.DrawPos);
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
				};
				widget.PreDraw += delegate(SpriteBatch sprtBtch, float deltaTime)
				{
					Widget widget3 = widget;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Scale: ");
					defaultInterpolatedStringHandler.AppendFormatted<double>(Math.Round((double)this.Scale, 2));
					defaultInterpolatedStringHandler.AppendLiteral("\n");
					defaultInterpolatedStringHandler.AppendLiteral("Rotation: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathHelper.ToDegrees(this.Rotation));
					widget3.Tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
					float rotation = this.Rotation - 1.5707964f;
					widget.DrawPos = this.Position + new Vector2((float)Math.Cos((double)rotation), (float)Math.Sin((double)rotation)) * (this.Scale * widgetSize);
					widget.Update(deltaTime);
				};
			});
			currentWidget.Draw(spriteBatch, 0.016666668f);
			GUI.DrawLine(spriteBatch, this.Position, currentWidget.DrawPos, GUIStyle.Green, 0f, (float)width);
		}

		// Token: 0x06002489 RID: 9353 RVA: 0x00171E90 File Offset: 0x00170090
		private float GetRotationAngle(Vector2 drawPosition)
		{
			Vector2 rotationVector = this.GetMousePos() - drawPosition;
			rotationVector.Normalize();
			double angle = Math.Atan2((double)MathHelper.ToRadians(rotationVector.Y), (double)MathHelper.ToRadians(rotationVector.X));
			if (angle < 0.0)
			{
				angle = ((Math.Abs(angle - (double)this.prevAngle) < Math.Abs(angle + 6.283185307179586 - (double)this.prevAngle)) ? angle : (angle + 6.283185307179586));
			}
			else if (angle > 0.0)
			{
				angle = ((Math.Abs(angle - (double)this.prevAngle) < Math.Abs(angle - 6.283185307179586 - (double)this.prevAngle)) ? angle : (angle - 6.283185307179586));
			}
			angle = (double)MathHelper.Clamp((float)angle, -6.2831855f, 6.2831855f);
			this.prevAngle = (float)angle;
			return (float)angle;
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x00171F74 File Offset: 0x00170174
		private Widget GetWidget(string id, int size, float thickness = 1f, [Nullable(new byte[]
		{
			2,
			1
		})] Action<Widget> initMethod = null)
		{
			Widget widget;
			if (!this.widgets.TryGetValue(id, out widget))
			{
				widget = new Widget(id, size, WidgetShape.Rectangle)
				{
					Color = Color.Yellow,
					RequireMouseOn = false
				};
				this.widgets.Add(id, widget);
				if (initMethod != null)
				{
					initMethod(widget);
				}
			}
			widget.Size = size;
			widget.Thickness = thickness;
			return widget;
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x00171FD4 File Offset: 0x001701D4
		public void UpdateRectangle()
		{
			if (this.Image == null)
			{
				this.Bounds = new Rectangle((int)this.Position.X, (int)this.Position.Y, 512, 512);
				return;
			}
			Vector2 size = new Vector2((float)this.Image.Width * this.Scale, (float)this.Image.Height * this.Scale);
			this.Bounds = new Rectangle((this.Position - size / 2f).ToPoint(), size.ToPoint());
		}

		// Token: 0x0600248C RID: 9356 RVA: 0x00172074 File Offset: 0x00170274
		public void Draw(SpriteBatch spriteBatch)
		{
			if (this.Image == null)
			{
				return;
			}
			spriteBatch.Draw(this.Image, this.Position, null, Color.White * this.Opacity, this.Rotation, new Vector2((float)this.Image.Width / 2f, (float)this.Image.Height / 2f), this.Scale, SpriteEffects.None, 0f);
		}

		// Token: 0x0600248D RID: 9357 RVA: 0x001720F0 File Offset: 0x001702F0
		public void DrawEditing(SpriteBatch spriteBatch, Camera cam)
		{
			Rectangle bounds = this.Bounds;
			int width = 4;
			if (this.DrawTarget == EditorImage.DrawTargetType.World)
			{
				width = (int)((float)width / cam.Zoom);
			}
			GUI.DrawRectangle(spriteBatch, bounds, this.Selected ? GUIStyle.Red : GUIStyle.Green, false, 0f, (float)width);
			if (this.Selected)
			{
				this.DrawWidgets(spriteBatch);
			}
		}

		// Token: 0x04001231 RID: 4657
		[Nullable(2)]
		public Texture2D Image;

		// Token: 0x04001232 RID: 4658
		public string ImagePath;

		// Token: 0x04001233 RID: 4659
		public Vector2 Position;

		// Token: 0x04001234 RID: 4660
		public float Rotation;

		// Token: 0x04001235 RID: 4661
		public float Opacity = 1f;

		// Token: 0x04001236 RID: 4662
		public float Scale = 1f;

		// Token: 0x04001237 RID: 4663
		public EditorImage.DrawTargetType DrawTarget;

		// Token: 0x04001238 RID: 4664
		public bool Selected;

		// Token: 0x04001239 RID: 4665
		public Rectangle Bounds;

		// Token: 0x0400123A RID: 4666
		private float prevAngle;

		// Token: 0x0400123B RID: 4667
		private bool disableMove;

		// Token: 0x0400123C RID: 4668
		private bool isDragging;

		// Token: 0x0400123D RID: 4669
		private readonly Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();

		// Token: 0x02000C20 RID: 3104
		[NullableContext(0)]
		public enum DrawTargetType
		{
			// Token: 0x04004A0E RID: 18958
			Camera,
			// Token: 0x04004A0F RID: 18959
			World
		}
	}
}
