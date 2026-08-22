using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000C1 RID: 193
	internal class Widget
	{
		// Token: 0x170005DD RID: 1501
		// (get) Token: 0x060017B4 RID: 6068 RVA: 0x000EA700 File Offset: 0x000E8900
		public Rectangle DrawRect
		{
			get
			{
				return new Rectangle((int)(this.DrawPos.X - (float)this.Size / 2f), (int)(this.DrawPos.Y - (float)this.Size / 2f), this.Size, this.Size);
			}
		}

		// Token: 0x170005DE RID: 1502
		// (get) Token: 0x060017B5 RID: 6069 RVA: 0x000EA754 File Offset: 0x000E8954
		public Rectangle InputRect
		{
			get
			{
				Rectangle inputRect = this.DrawRect;
				inputRect.Inflate(this.InputAreaMargin, this.InputAreaMargin);
				return inputRect;
			}
		}

		// Token: 0x170005DF RID: 1503
		// (get) Token: 0x060017B6 RID: 6070 RVA: 0x000EA77C File Offset: 0x000E897C
		// (set) Token: 0x060017B7 RID: 6071 RVA: 0x000EA784 File Offset: 0x000E8984
		public Vector2 DrawPos { get; set; }

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060017B8 RID: 6072 RVA: 0x000EA790 File Offset: 0x000E8990
		// (remove) Token: 0x060017B9 RID: 6073 RVA: 0x000EA7C8 File Offset: 0x000E89C8
		public event Action Selected;

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060017BA RID: 6074 RVA: 0x000EA800 File Offset: 0x000E8A00
		// (remove) Token: 0x060017BB RID: 6075 RVA: 0x000EA838 File Offset: 0x000E8A38
		public event Action Deselected;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060017BC RID: 6076 RVA: 0x000EA870 File Offset: 0x000E8A70
		// (remove) Token: 0x060017BD RID: 6077 RVA: 0x000EA8A8 File Offset: 0x000E8AA8
		public event Action Hovered;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x060017BE RID: 6078 RVA: 0x000EA8E0 File Offset: 0x000E8AE0
		// (remove) Token: 0x060017BF RID: 6079 RVA: 0x000EA918 File Offset: 0x000E8B18
		public event Action MouseUp;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x060017C0 RID: 6080 RVA: 0x000EA950 File Offset: 0x000E8B50
		// (remove) Token: 0x060017C1 RID: 6081 RVA: 0x000EA988 File Offset: 0x000E8B88
		public event Action MouseDown;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x060017C2 RID: 6082 RVA: 0x000EA9C0 File Offset: 0x000E8BC0
		// (remove) Token: 0x060017C3 RID: 6083 RVA: 0x000EA9F8 File Offset: 0x000E8BF8
		public event Action<float> MouseHeld;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x060017C4 RID: 6084 RVA: 0x000EAA30 File Offset: 0x000E8C30
		// (remove) Token: 0x060017C5 RID: 6085 RVA: 0x000EAA68 File Offset: 0x000E8C68
		public event Action<float> PreUpdate;

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x060017C6 RID: 6086 RVA: 0x000EAAA0 File Offset: 0x000E8CA0
		// (remove) Token: 0x060017C7 RID: 6087 RVA: 0x000EAAD8 File Offset: 0x000E8CD8
		public event Action<float> PostUpdate;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x060017C8 RID: 6088 RVA: 0x000EAB10 File Offset: 0x000E8D10
		// (remove) Token: 0x060017C9 RID: 6089 RVA: 0x000EAB48 File Offset: 0x000E8D48
		public event Action<SpriteBatch, float> PreDraw;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x060017CA RID: 6090 RVA: 0x000EAB80 File Offset: 0x000E8D80
		// (remove) Token: 0x060017CB RID: 6091 RVA: 0x000EABB8 File Offset: 0x000E8DB8
		public event Action<SpriteBatch, float> PostDraw;

		// Token: 0x170005E0 RID: 1504
		// (get) Token: 0x060017CC RID: 6092 RVA: 0x000EABED File Offset: 0x000E8DED
		public bool IsSelected
		{
			get
			{
				return this.enabled && Widget.SelectedWidgets.Contains(this);
			}
		}

		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x060017CD RID: 6093 RVA: 0x000EAC04 File Offset: 0x000E8E04
		public bool IsControlled
		{
			get
			{
				return this.IsSelected && PlayerInput.PrimaryMouseButtonHeld();
			}
		}

		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x060017CE RID: 6094 RVA: 0x000EAC18 File Offset: 0x000E8E18
		public bool IsMouseOver
		{
			get
			{
				return GUI.MouseOn == null && this.InputRect.Contains(PlayerInput.MousePosition);
			}
		}

		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x060017CF RID: 6095 RVA: 0x000EAC41 File Offset: 0x000E8E41
		// (set) Token: 0x060017D0 RID: 6096 RVA: 0x000EAC49 File Offset: 0x000E8E49
		public bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				this.enabled = value;
				if (!this.enabled && Widget.SelectedWidgets.Contains(this))
				{
					Widget.SelectedWidgets.Remove(this);
				}
			}
		}

		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x060017D1 RID: 6097 RVA: 0x000EAC73 File Offset: 0x000E8E73
		// (set) Token: 0x060017D2 RID: 6098 RVA: 0x000EAC7A File Offset: 0x000E8E7A
		public static bool EnableMultiSelect
		{
			get
			{
				return Widget.multiselect;
			}
			set
			{
				Widget.multiselect = value;
				if (!Widget.multiselect && Widget.SelectedWidgets.Multiple(null))
				{
					Widget.SelectedWidgets = Widget.SelectedWidgets.Take(1).ToList<Widget>();
				}
			}
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x000EACAC File Offset: 0x000E8EAC
		public Widget(string id, int size, WidgetShape shape)
		{
			this.Id = id;
			this.Size = size;
			this.Shape = shape;
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x000EAD34 File Offset: 0x000E8F34
		public virtual void Update(float deltaTime)
		{
			Action<float> preUpdate = this.PreUpdate;
			if (preUpdate != null)
			{
				preUpdate(deltaTime);
			}
			if (!this.enabled)
			{
				return;
			}
			if (this.IsMouseOver || (!this.RequireMouseOn && Widget.SelectedWidgets.Contains(this) && PlayerInput.PrimaryMouseButtonHeld()))
			{
				Action hovered = this.Hovered;
				if (hovered != null)
				{
					hovered();
				}
				if ((this.RequireMouseOn || PlayerInput.PrimaryMouseButtonDown()) && ((Widget.multiselect && !Widget.SelectedWidgets.Contains(this)) || Widget.SelectedWidgets.None(null)))
				{
					Widget.SelectedWidgets.Add(this);
					Action selected = this.Selected;
					if (selected != null)
					{
						selected();
					}
				}
			}
			else if (Widget.SelectedWidgets.Contains(this))
			{
				Widget.SelectedWidgets.Remove(this);
				Action deselected = this.Deselected;
				if (deselected != null)
				{
					deselected();
				}
			}
			if (this.IsSelected)
			{
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					Action mouseDown = this.MouseDown;
					if (mouseDown != null)
					{
						mouseDown();
					}
				}
				if (PlayerInput.PrimaryMouseButtonHeld())
				{
					Action<float> mouseHeld = this.MouseHeld;
					if (mouseHeld != null)
					{
						mouseHeld(deltaTime);
					}
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					Action mouseUp = this.MouseUp;
					if (mouseUp != null)
					{
						mouseUp();
					}
				}
			}
			Action<float> postUpdate = this.PostUpdate;
			if (postUpdate == null)
			{
				return;
			}
			postUpdate(deltaTime);
		}

		// Token: 0x060017D5 RID: 6101 RVA: 0x000EAE6C File Offset: 0x000E906C
		public virtual void Draw(SpriteBatch spriteBatch, float deltaTime)
		{
			Action<SpriteBatch, float> preDraw = this.PreDraw;
			if (preDraw != null)
			{
				preDraw(spriteBatch, deltaTime);
			}
			Rectangle drawRect = this.DrawRect;
			switch (this.Shape)
			{
			case WidgetShape.Rectangle:
				if (this.SecondaryColor != null)
				{
					GUI.DrawRectangle(spriteBatch, drawRect, this.SecondaryColor.Value, this.IsFilled, 0f, 2f);
				}
				GUI.DrawRectangle(spriteBatch, drawRect, this.Color, this.IsFilled, 0f, (float)(this.IsSelected ? ((int)(this.Thickness * 3f)) : ((int)this.Thickness)));
				break;
			case WidgetShape.Circle:
				if (this.SecondaryColor != null)
				{
					spriteBatch.DrawCircle(this.DrawPos, (float)(this.Size / 2), this.Sides, this.SecondaryColor.Value, 2f);
				}
				spriteBatch.DrawCircle(this.DrawPos, (float)(this.Size / 2), this.Sides, this.Color, (float)(this.IsSelected ? 3 : 1));
				break;
			case WidgetShape.Cross:
			{
				float halfSize = (float)(this.Size / 2);
				if (this.SecondaryColor != null)
				{
					GUI.DrawLine(spriteBatch, this.DrawPos + Vector2.UnitY * halfSize, this.DrawPos - Vector2.UnitY * halfSize, this.SecondaryColor.Value, 0f, 2f);
					GUI.DrawLine(spriteBatch, this.DrawPos + Vector2.UnitX * halfSize, this.DrawPos - Vector2.UnitX * halfSize, this.SecondaryColor.Value, 0f, 2f);
				}
				GUI.DrawLine(spriteBatch, this.DrawPos + Vector2.UnitY * halfSize, this.DrawPos - Vector2.UnitY * halfSize, this.Color, 0f, (float)(this.IsSelected ? 3 : 1));
				GUI.DrawLine(spriteBatch, this.DrawPos + Vector2.UnitX * halfSize, this.DrawPos - Vector2.UnitX * halfSize, this.Color, 0f, (float)(this.IsSelected ? 3 : 1));
				break;
			}
			default:
				throw new NotImplementedException(this.Shape.ToString());
			}
			if (this.IsSelected && this.ShowTooltip && !this.Tooltip.IsNullOrEmpty())
			{
				Vector2 offset = this.TooltipOffset ?? new Vector2((float)this.Size, (float)(-(float)this.Size) / 2f);
				GUIComponent.DrawToolTip(spriteBatch, this.Tooltip, this.DrawPos + offset, new Color?(this.TextColor), new Color?(this.TextBackgroundColor));
			}
			Action<SpriteBatch, float> postDraw = this.PostDraw;
			if (postDraw == null)
			{
				return;
			}
			postDraw(spriteBatch, deltaTime);
		}

		// Token: 0x04000C38 RID: 3128
		public WidgetShape Shape;

		// Token: 0x04000C39 RID: 3129
		public LocalizedString Tooltip;

		// Token: 0x04000C3A RID: 3130
		public bool ShowTooltip = true;

		// Token: 0x04000C3C RID: 3132
		public int Size = 10;

		// Token: 0x04000C3D RID: 3133
		public float Thickness = 1f;

		// Token: 0x04000C3E RID: 3134
		public int Sides = 40;

		// Token: 0x04000C3F RID: 3135
		public bool IsFilled;

		// Token: 0x04000C40 RID: 3136
		public int InputAreaMargin;

		// Token: 0x04000C41 RID: 3137
		public Color Color = GUIStyle.Red;

		// Token: 0x04000C42 RID: 3138
		public Color? SecondaryColor;

		// Token: 0x04000C43 RID: 3139
		public Color TextColor = Color.White;

		// Token: 0x04000C44 RID: 3140
		public Color TextBackgroundColor = Color.Black * 0.5f;

		// Token: 0x04000C45 RID: 3141
		public readonly string Id;

		// Token: 0x04000C50 RID: 3152
		public bool RequireMouseOn = true;

		// Token: 0x04000C51 RID: 3153
		public Action Refresh;

		// Token: 0x04000C52 RID: 3154
		public object Data;

		// Token: 0x04000C53 RID: 3155
		private bool enabled = true;

		// Token: 0x04000C54 RID: 3156
		private static bool multiselect;

		// Token: 0x04000C55 RID: 3157
		public Vector2? TooltipOffset;

		// Token: 0x04000C56 RID: 3158
		public Widget LinkedWidget;

		// Token: 0x04000C57 RID: 3159
		public static List<Widget> SelectedWidgets = new List<Widget>();
	}
}
