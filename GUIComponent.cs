using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RestSharp;
using Steamworks;

namespace Barotrauma
{
	// Token: 0x02000082 RID: 130
	public abstract class GUIComponent
	{
		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x060011FF RID: 4607 RVA: 0x000B2827 File Offset: 0x000B0A27
		public GUIComponent Parent
		{
			get
			{
				RectTransform parent = this.RectTransform.Parent;
				if (parent == null)
				{
					return null;
				}
				return parent.GUIComponent;
			}
		}

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06001200 RID: 4608 RVA: 0x000B283F File Offset: 0x000B0A3F
		public IEnumerable<GUIComponent> Children
		{
			get
			{
				return from c in this.RectTransform.Children
				select c.GUIComponent;
			}
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x000B2870 File Offset: 0x000B0A70
		public T GetChild<T>() where T : GUIComponent
		{
			return this.Children.FirstOrDefault((GUIComponent c) => c is T) as T;
		}

		// Token: 0x06001202 RID: 4610 RVA: 0x000B28A6 File Offset: 0x000B0AA6
		public T GetAnyChild<T>() where T : GUIComponent
		{
			return this.GetAllChildren().FirstOrDefault((GUIComponent c) => c is T) as T;
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x000B28DC File Offset: 0x000B0ADC
		public IEnumerable<T> GetAllChildren<T>() where T : GUIComponent
		{
			return this.GetAllChildren().OfType<T>();
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x000B28E9 File Offset: 0x000B0AE9
		public IEnumerable<GUIComponent> GetAllChildren()
		{
			return from c in this.RectTransform.GetAllChildren()
			select c.GUIComponent;
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x000B291A File Offset: 0x000B0B1A
		public GUIComponent GetChild(int index)
		{
			if (index < 0 || index >= this.CountChildren)
			{
				return null;
			}
			return this.RectTransform.GetChild(index).GUIComponent;
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x000B293C File Offset: 0x000B0B3C
		public int GetChildIndex(GUIComponent child)
		{
			if (child == null)
			{
				return -1;
			}
			return this.RectTransform.GetChildIndex(child.RectTransform);
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x000B2954 File Offset: 0x000B0B54
		public GUIComponent GetChildByUserData(object obj)
		{
			foreach (GUIComponent child in this.Children)
			{
				if (object.Equals(child.UserData, obj))
				{
					return child;
				}
			}
			return null;
		}

		// Token: 0x06001208 RID: 4616 RVA: 0x000B29B0 File Offset: 0x000B0BB0
		public bool IsParentOf(GUIComponent component, bool recursive = true)
		{
			return component != null && this.RectTransform.IsParentOf(component.RectTransform, recursive);
		}

		// Token: 0x06001209 RID: 4617 RVA: 0x000B29C9 File Offset: 0x000B0BC9
		public bool IsChildOf(GUIComponent component, bool recursive = true)
		{
			return component != null && this.RectTransform.IsChildOf(component.RectTransform, recursive);
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x000B29E2 File Offset: 0x000B0BE2
		public virtual void RemoveChild(GUIComponent child)
		{
			if (child == null)
			{
				return;
			}
			child.RectTransform.Parent = null;
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x000B29F4 File Offset: 0x000B0BF4
		public GUIComponent FindChild(Func<GUIComponent, bool> predicate, bool recursive = false)
		{
			GUIComponent matchingChild = this.Children.FirstOrDefault(predicate);
			if (recursive && matchingChild == null)
			{
				foreach (GUIComponent child in this.Children)
				{
					matchingChild = child.FindChild(predicate, recursive);
					if (matchingChild != null)
					{
						return matchingChild;
					}
				}
				return matchingChild;
			}
			return matchingChild;
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x000B2A60 File Offset: 0x000B0C60
		public GUIComponent FindChild(object userData, bool recursive = false)
		{
			GUIComponent matchingChild = this.Children.FirstOrDefault((GUIComponent c) => object.Equals(c.UserData, userData));
			if (recursive && matchingChild == null)
			{
				foreach (GUIComponent child in this.Children)
				{
					matchingChild = child.FindChild(userData, recursive);
					if (matchingChild != null)
					{
						return matchingChild;
					}
				}
				return matchingChild;
			}
			return matchingChild;
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x000B2AEC File Offset: 0x000B0CEC
		public IEnumerable<GUIComponent> FindChildren(object userData)
		{
			return from c in this.Children
			where object.Equals(c.UserData, userData)
			select c;
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x000B2B20 File Offset: 0x000B0D20
		public IEnumerable<GUIComponent> FindChildren(Func<GUIComponent, bool> predicate)
		{
			return from c in this.Children
			where predicate(c)
			select c;
		}

		// Token: 0x0600120F RID: 4623 RVA: 0x000B2B51 File Offset: 0x000B0D51
		public virtual void ClearChildren()
		{
			this.RectTransform.ClearChildren();
		}

		// Token: 0x06001210 RID: 4624 RVA: 0x000B2B5E File Offset: 0x000B0D5E
		public void SetAsFirstChild()
		{
			this.RectTransform.SetAsFirstChild();
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x000B2B6B File Offset: 0x000B0D6B
		public void SetAsLastChild()
		{
			this.RectTransform.SetAsLastChild();
		}

		// Token: 0x17000470 RID: 1136
		// (get) Token: 0x06001212 RID: 4626 RVA: 0x000B2B78 File Offset: 0x000B0D78
		// (set) Token: 0x06001213 RID: 4627 RVA: 0x000B2B80 File Offset: 0x000B0D80
		public bool AutoUpdate { get; set; } = true;

		// Token: 0x17000471 RID: 1137
		// (get) Token: 0x06001214 RID: 4628 RVA: 0x000B2B89 File Offset: 0x000B0D89
		// (set) Token: 0x06001215 RID: 4629 RVA: 0x000B2B91 File Offset: 0x000B0D91
		public bool AutoDraw { get; set; } = true;

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06001216 RID: 4630 RVA: 0x000B2B9A File Offset: 0x000B0D9A
		// (set) Token: 0x06001217 RID: 4631 RVA: 0x000B2BA2 File Offset: 0x000B0DA2
		public int UpdateOrder { get; set; }

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06001218 RID: 4632 RVA: 0x000B2BAB File Offset: 0x000B0DAB
		// (set) Token: 0x06001219 RID: 4633 RVA: 0x000B2BB3 File Offset: 0x000B0DB3
		public bool Bounce { get; set; }

		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x0600121A RID: 4634 RVA: 0x000B2BBC File Offset: 0x000B0DBC
		// (set) Token: 0x0600121B RID: 4635 RVA: 0x000B2BC4 File Offset: 0x000B0DC4
		public bool GlowOnSelect { get; set; }

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x0600121C RID: 4636 RVA: 0x000B2BCD File Offset: 0x000B0DCD
		// (set) Token: 0x0600121D RID: 4637 RVA: 0x000B2BD5 File Offset: 0x000B0DD5
		public Vector2 UVOffset { get; set; }

		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x0600121E RID: 4638 RVA: 0x000B2BDE File Offset: 0x000B0DDE
		public virtual float FlashTimer
		{
			get
			{
				return this.flashTimer;
			}
		}

		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x0600121F RID: 4639 RVA: 0x000B2BE6 File Offset: 0x000B0DE6
		// (set) Token: 0x06001220 RID: 4640 RVA: 0x000B2BF0 File Offset: 0x000B0DF0
		public bool IgnoreLayoutGroups
		{
			get
			{
				return this.ignoreLayoutGroups;
			}
			set
			{
				if (this.ignoreLayoutGroups == value)
				{
					return;
				}
				this.ignoreLayoutGroups = value;
				GUILayoutGroup layoutGroup = this.Parent as GUILayoutGroup;
				if (layoutGroup != null)
				{
					layoutGroup.NeedsToRecalculate = true;
				}
			}
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06001221 RID: 4641 RVA: 0x000B2C24 File Offset: 0x000B0E24
		// (set) Token: 0x06001222 RID: 4642 RVA: 0x000B2C2C File Offset: 0x000B0E2C
		public virtual GUIFont Font { get; set; }

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06001223 RID: 4643 RVA: 0x000B2C35 File Offset: 0x000B0E35
		// (set) Token: 0x06001224 RID: 4644 RVA: 0x000B2C3D File Offset: 0x000B0E3D
		public virtual RichString ToolTip
		{
			get
			{
				return this.toolTip;
			}
			set
			{
				this.toolTip = value;
			}
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06001225 RID: 4645 RVA: 0x000B2C46 File Offset: 0x000B0E46
		public GUIComponentStyle Style
		{
			get
			{
				return GUIComponentStyle.FromHierarchy(this.styleHierarchy);
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06001226 RID: 4646 RVA: 0x000B2C53 File Offset: 0x000B0E53
		// (set) Token: 0x06001227 RID: 4647 RVA: 0x000B2C5B File Offset: 0x000B0E5B
		public bool Visible { get; set; }

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x06001228 RID: 4648 RVA: 0x000B2C64 File Offset: 0x000B0E64
		// (set) Token: 0x06001229 RID: 4649 RVA: 0x000B2C6C File Offset: 0x000B0E6C
		public virtual bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				this.enabled = value;
			}
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x0600122A RID: 4650 RVA: 0x000B2C78 File Offset: 0x000B0E78
		public Vector2 Center
		{
			get
			{
				return new Vector2((float)this.Rect.Center.X, (float)this.Rect.Center.Y);
			}
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x000B2CB4 File Offset: 0x000B0EB4
		public void ClampToArea(Rectangle clampArea)
		{
			Rectangle componentRect = this.Rect;
			int x = componentRect.X;
			int y = componentRect.Y;
			if (componentRect.Width <= clampArea.Width)
			{
				if (componentRect.Left < clampArea.Left)
				{
					x = clampArea.Left;
				}
				else if (componentRect.Right > clampArea.Right)
				{
					x = clampArea.Right - componentRect.Width;
				}
			}
			else
			{
				x = clampArea.Left - (componentRect.Width - clampArea.Width) / 2;
			}
			if (componentRect.Height <= clampArea.Height)
			{
				if (componentRect.Top < clampArea.Top)
				{
					y = clampArea.Top;
				}
				else if (componentRect.Bottom > clampArea.Bottom)
				{
					y = clampArea.Bottom - componentRect.Height;
				}
			}
			else
			{
				y = clampArea.Top - (componentRect.Height - clampArea.Height) / 2;
			}
			Point moveAmount = new Point(x - componentRect.X, y - componentRect.Y);
			this.RectTransform.ScreenSpaceOffset += moveAmount;
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x000B2DC8 File Offset: 0x000B0FC8
		protected Rectangle ClampRect(Rectangle r)
		{
			if (this.Parent == null)
			{
				return r;
			}
			Rectangle parentRect = (!this.Parent.ClampMouseRectToParent) ? this.Parent.Rect : this.Parent.ClampRect(this.Parent.Rect);
			if (parentRect.Width <= 0 || parentRect.Height <= 0)
			{
				return Rectangle.Empty;
			}
			if (parentRect.X > r.X)
			{
				int diff = parentRect.X - r.X;
				r.X = parentRect.X;
				r.Width -= diff;
			}
			if (parentRect.Y > r.Y)
			{
				int diff2 = parentRect.Y - r.Y;
				r.Y = parentRect.Y;
				r.Height -= diff2;
			}
			if (parentRect.X + parentRect.Width < r.X + r.Width)
			{
				int diff3 = r.X + r.Width - (parentRect.X + parentRect.Width);
				r.Width -= diff3;
			}
			if (parentRect.Y + parentRect.Height < r.Y + r.Height)
			{
				int diff4 = r.Y + r.Height - (parentRect.Y + parentRect.Height);
				r.Height -= diff4;
			}
			if (r.Width <= 0 || r.Height <= 0)
			{
				return Rectangle.Empty;
			}
			return r;
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x0600122D RID: 4653 RVA: 0x000B2F35 File Offset: 0x000B1135
		public virtual Rectangle Rect
		{
			get
			{
				return this.RectTransform.Rect;
			}
		}

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x0600122E RID: 4654 RVA: 0x000B2F42 File Offset: 0x000B1142
		// (set) Token: 0x0600122F RID: 4655 RVA: 0x000B2F4A File Offset: 0x000B114A
		public bool ClampMouseRectToParent { get; set; }

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06001230 RID: 4656 RVA: 0x000B2F53 File Offset: 0x000B1153
		public virtual Rectangle MouseRect
		{
			get
			{
				if (!this.CanBeFocused)
				{
					return Rectangle.Empty;
				}
				if (!this.ClampMouseRectToParent)
				{
					return this.Rect;
				}
				return this.ClampRect(this.Rect);
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06001231 RID: 4657 RVA: 0x000B2F7E File Offset: 0x000B117E
		// (set) Token: 0x06001232 RID: 4658 RVA: 0x000B2F86 File Offset: 0x000B1186
		public virtual Color OutlineColor { get; set; }

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06001233 RID: 4659 RVA: 0x000B2F8F File Offset: 0x000B118F
		// (set) Token: 0x06001234 RID: 4660 RVA: 0x000B2F98 File Offset: 0x000B1198
		public virtual bool Selected
		{
			get
			{
				return this.isSelected;
			}
			set
			{
				this.isSelected = value;
				foreach (GUIComponent child in this.Children)
				{
					child.Selected = value;
				}
			}
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06001235 RID: 4661 RVA: 0x000B2FEC File Offset: 0x000B11EC
		// (set) Token: 0x06001236 RID: 4662 RVA: 0x000B2FF4 File Offset: 0x000B11F4
		public virtual GUIComponent.ComponentState State
		{
			get
			{
				return this._state;
			}
			set
			{
				if (this._state != value)
				{
					this.spriteFadeTimer = this.SpriteCrossFadeTime;
					this.colorFadeTimer = this.ColorCrossFadeTime;
					this._previousState = this._state;
				}
				this._state = value;
			}
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06001237 RID: 4663 RVA: 0x000B302A File Offset: 0x000B122A
		public int CountChildren
		{
			get
			{
				return this.RectTransform.CountChildren;
			}
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06001238 RID: 4664 RVA: 0x000B3037 File Offset: 0x000B1237
		// (set) Token: 0x06001239 RID: 4665 RVA: 0x000B303F File Offset: 0x000B123F
		public Color DefaultColor { get; set; }

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x0600123A RID: 4666 RVA: 0x000B3048 File Offset: 0x000B1248
		// (set) Token: 0x0600123B RID: 4667 RVA: 0x000B3050 File Offset: 0x000B1250
		public virtual Color Color
		{
			get
			{
				return this.color;
			}
			set
			{
				this.color = value;
			}
		}

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x0600123C RID: 4668 RVA: 0x000B3059 File Offset: 0x000B1259
		// (set) Token: 0x0600123D RID: 4669 RVA: 0x000B3061 File Offset: 0x000B1261
		public virtual Color HoverColor
		{
			get
			{
				return this.hoverColor;
			}
			set
			{
				this.hoverColor = value;
			}
		}

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x0600123E RID: 4670 RVA: 0x000B306A File Offset: 0x000B126A
		// (set) Token: 0x0600123F RID: 4671 RVA: 0x000B3072 File Offset: 0x000B1272
		public virtual Color SelectedColor
		{
			get
			{
				return this.selectedColor;
			}
			set
			{
				this.selectedColor = value;
			}
		}

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06001240 RID: 4672 RVA: 0x000B307B File Offset: 0x000B127B
		// (set) Token: 0x06001241 RID: 4673 RVA: 0x000B3083 File Offset: 0x000B1283
		public virtual Color DisabledColor
		{
			get
			{
				return this.disabledColor;
			}
			set
			{
				this.disabledColor = value;
			}
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06001242 RID: 4674 RVA: 0x000B308C File Offset: 0x000B128C
		// (set) Token: 0x06001243 RID: 4675 RVA: 0x000B3094 File Offset: 0x000B1294
		public virtual Color PressedColor
		{
			get
			{
				return this.pressedColor;
			}
			set
			{
				this.pressedColor = value;
			}
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06001244 RID: 4676 RVA: 0x000B309D File Offset: 0x000B129D
		// (set) Token: 0x06001245 RID: 4677 RVA: 0x000B30A5 File Offset: 0x000B12A5
		public TransitionMode ColorTransition { get; private set; }

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06001246 RID: 4678 RVA: 0x000B30AE File Offset: 0x000B12AE
		// (set) Token: 0x06001247 RID: 4679 RVA: 0x000B30B6 File Offset: 0x000B12B6
		public SpriteFallBackState FallBackState { get; private set; }

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06001248 RID: 4680 RVA: 0x000B30BF File Offset: 0x000B12BF
		// (set) Token: 0x06001249 RID: 4681 RVA: 0x000B30C7 File Offset: 0x000B12C7
		public float SpriteCrossFadeTime { get; private set; }

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x0600124A RID: 4682 RVA: 0x000B30D0 File Offset: 0x000B12D0
		// (set) Token: 0x0600124B RID: 4683 RVA: 0x000B30D8 File Offset: 0x000B12D8
		public float ColorCrossFadeTime { get; private set; }

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x0600124C RID: 4684 RVA: 0x000B30E1 File Offset: 0x000B12E1
		// (set) Token: 0x0600124D RID: 4685 RVA: 0x000B30E9 File Offset: 0x000B12E9
		public virtual bool PlaySoundOnSelect { get; set; }

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x0600124E RID: 4686 RVA: 0x000B30F2 File Offset: 0x000B12F2
		// (set) Token: 0x0600124F RID: 4687 RVA: 0x000B30FA File Offset: 0x000B12FA
		public RectTransform RectTransform
		{
			get
			{
				return this.rectTransform;
			}
			private set
			{
				this.rectTransform = value;
				if (this.rectTransform != null)
				{
					this.rectTransform.GUIComponent = this;
				}
			}
		}

		// Token: 0x06001250 RID: 4688 RVA: 0x000B3118 File Offset: 0x000B1318
		protected GUIComponent(string style, RectTransform rectT)
		{
			this.RectTransform = rectT;
			this.Visible = true;
			this.OutlineColor = Color.Transparent;
			this.Font = GUIStyle.Font;
			this.CanBeFocused = true;
			if (style != null)
			{
				GUIStyle.Apply(this, style, null);
			}
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x000B317C File Offset: 0x000B137C
		protected GUIComponent(string style)
		{
			this.Visible = true;
			this.OutlineColor = Color.Transparent;
			this.Font = GUIStyle.Font;
			this.CanBeFocused = true;
			if (style != null)
			{
				GUIStyle.Apply(this, style, null);
			}
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x000B31D7 File Offset: 0x000B13D7
		public virtual void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			if (!this.Visible)
			{
				return;
			}
			this.UpdateOrder = order;
			GUI.AddToUpdateList(this);
			if (!ignoreChildren)
			{
				this.RectTransform.AddChildrenToGUIUpdateList(ignoreChildren, order);
			}
			Action<GUIComponent> onAddedToGUIUpdateList = this.OnAddedToGUIUpdateList;
			if (onAddedToGUIUpdateList == null)
			{
				return;
			}
			onAddedToGUIUpdateList(this);
		}

		// Token: 0x06001253 RID: 4691 RVA: 0x000B3210 File Offset: 0x000B1410
		public void RemoveFromGUIUpdateList(bool alsoChildren = true)
		{
			GUI.RemoveFromUpdateList(this, alsoChildren);
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x000B3219 File Offset: 0x000B1419
		public void UpdateAuto(float deltaTime)
		{
			if (this.AutoUpdate)
			{
				this.Update(deltaTime);
			}
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x000B322A File Offset: 0x000B142A
		public void UpdateManually(float deltaTime, bool alsoChildren = false, bool recursive = true)
		{
			if (!this.Visible)
			{
				return;
			}
			this.AutoUpdate = false;
			this.Update(deltaTime);
			if (alsoChildren)
			{
				this.UpdateChildren(deltaTime, recursive);
			}
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x000B3250 File Offset: 0x000B1450
		protected virtual void Update(float deltaTime)
		{
			if (!this.Visible)
			{
				return;
			}
			if (this.CanBeFocused && this.OnSecondaryClicked != null && GUI.IsMouseOn(this) && PlayerInput.SecondaryMouseButtonClicked())
			{
				GUIComponent.SecondaryButtonDownHandler onSecondaryClicked = this.OnSecondaryClicked;
				if (onSecondaryClicked != null)
				{
					onSecondaryClicked(this, this.UserData);
				}
			}
			if (this.Bounce)
			{
				if (this.bounceTimer > 3f || this.bounceDown)
				{
					this.RectTransform.ScreenSpaceOffset = new Point(this.RectTransform.ScreenSpaceOffset.X, (int)(-(int)(this.bounceJump * 15f * GUI.Scale)));
					if (!this.bounceDown)
					{
						this.bounceJump += deltaTime * 4f;
						if (this.bounceJump > 0.5f)
						{
							this.bounceDown = true;
						}
					}
					else
					{
						this.bounceJump -= deltaTime * 4f;
						if (this.bounceJump <= 0f)
						{
							this.bounceJump = 0f;
							this.bounceTimer = 0f;
							this.bounceDown = false;
							this.Bounce = false;
						}
					}
				}
				else
				{
					this.bounceTimer += deltaTime;
				}
			}
			if (this.flashTimer > 0f)
			{
				this.flashTimer -= deltaTime;
			}
			if (this.spriteFadeTimer > 0f)
			{
				this.spriteFadeTimer -= deltaTime;
			}
			if (this.colorFadeTimer > 0f)
			{
				this.colorFadeTimer -= deltaTime;
			}
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x000B33D0 File Offset: 0x000B15D0
		public virtual void ForceLayoutRecalculation()
		{
			this.ForceUpdate();
			this.ForceUpdate();
			foreach (GUIComponent child in this.Children)
			{
				child.ForceLayoutRecalculation();
			}
		}

		// Token: 0x06001258 RID: 4696 RVA: 0x000B3428 File Offset: 0x000B1628
		public void ForceUpdate()
		{
			this.Update(0.016666668f);
		}

		// Token: 0x06001259 RID: 4697 RVA: 0x000B3438 File Offset: 0x000B1638
		public void UpdateChildren(float deltaTime, bool recursive)
		{
			foreach (RectTransform child in this.RectTransform.Children)
			{
				child.GUIComponent.UpdateManually(deltaTime, recursive, recursive);
			}
		}

		// Token: 0x0600125A RID: 4698 RVA: 0x000B3494 File Offset: 0x000B1694
		public void DrawAuto(SpriteBatch spriteBatch)
		{
			if (this.AutoDraw)
			{
				this.Draw(spriteBatch);
			}
		}

		// Token: 0x0600125B RID: 4699 RVA: 0x000B34A5 File Offset: 0x000B16A5
		public virtual void DrawManually(SpriteBatch spriteBatch, bool alsoChildren = false, bool recursive = true)
		{
			if (!this.Visible)
			{
				return;
			}
			this.AutoDraw = false;
			this.Draw(spriteBatch);
			if (alsoChildren)
			{
				this.DrawChildren(spriteBatch, recursive);
			}
		}

		// Token: 0x0600125C RID: 4700 RVA: 0x000B34CC File Offset: 0x000B16CC
		public virtual void DrawChildren(SpriteBatch spriteBatch, bool recursive)
		{
			foreach (RectTransform child in this.RectTransform.Children)
			{
				child.GUIComponent.DrawManually(spriteBatch, recursive, recursive);
			}
		}

		// Token: 0x0600125D RID: 4701 RVA: 0x000B3528 File Offset: 0x000B1728
		protected virtual Color GetColor(GUIComponent.ComponentState state)
		{
			if (!this.Enabled)
			{
				return this.DisabledColor;
			}
			if (this.ExternalHighlight)
			{
				return this.HoverColor;
			}
			switch (state)
			{
			case GUIComponent.ComponentState.Hover:
				return this.HoverColor;
			case GUIComponent.ComponentState.Pressed:
				return this.PressedColor;
			case GUIComponent.ComponentState.Selected:
				if (!this.GlowOnSelect)
				{
					return this.SelectedColor;
				}
				break;
			case GUIComponent.ComponentState.HoverSelected:
				return this.HoverColor;
			}
			return this.Color;
		}

		// Token: 0x0600125E RID: 4702 RVA: 0x000B35A4 File Offset: 0x000B17A4
		protected Color GetBlendedColor(Color targetColor, ref Color blendedColor)
		{
			blendedColor = ((this.ColorCrossFadeTime > 0f) ? Color.Lerp(blendedColor, targetColor, MathUtils.InverseLerp(this.ColorCrossFadeTime, 0f, ToolBox.GetEasing(this.ColorTransition, this.colorFadeTimer))) : targetColor);
			return blendedColor;
		}

		// Token: 0x0600125F RID: 4703 RVA: 0x000B35FC File Offset: 0x000B17FC
		protected virtual void Draw(SpriteBatch spriteBatch)
		{
			if (!this.Visible)
			{
				return;
			}
			Rectangle rect = this.Rect;
			this.GetBlendedColor(this.GetColor(this.State), ref this._currentColor);
			if ((float)this._currentColor.A > 0f && (this.sprites == null || !this.sprites.Any<KeyValuePair<GUIComponent.ComponentState, List<UISprite>>>()))
			{
				GUI.DrawRectangle(spriteBatch, rect, this._currentColor * ((float)this._currentColor.A / 255f), true, 0f, 1f);
			}
			if (this.sprites != null && this._currentColor.A > 0)
			{
				List<UISprite> previousSprites;
				if (!this.sprites.TryGetValue(this._previousState, out previousSprites) || previousSprites.None(null))
				{
					SpriteFallBackState fallBackState3 = this.FallBackState;
					GUIComponent.ComponentState fallBackState;
					if (fallBackState3 == SpriteFallBackState.Toggle)
					{
						this.sprites.TryGetValue(this.Selected ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None, out previousSprites);
					}
					else if (Enum.TryParse<GUIComponent.ComponentState>(this.FallBackState.ToString(), true, out fallBackState))
					{
						this.sprites.TryGetValue(fallBackState, out previousSprites);
					}
				}
				List<UISprite> currentSprites;
				if (!this.sprites.TryGetValue(this.State, out currentSprites) || currentSprites.None(null))
				{
					SpriteFallBackState fallBackState4 = this.FallBackState;
					GUIComponent.ComponentState fallBackState2;
					if (fallBackState4 == SpriteFallBackState.Toggle)
					{
						this.sprites.TryGetValue(this.Selected ? GUIComponent.ComponentState.Selected : GUIComponent.ComponentState.None, out currentSprites);
					}
					else if (Enum.TryParse<GUIComponent.ComponentState>(this.FallBackState.ToString(), true, out fallBackState2))
					{
						this.sprites.TryGetValue(fallBackState2, out currentSprites);
					}
				}
				if (this._previousState != this.State && currentSprites != previousSprites && previousSprites != null && previousSprites.Any<UISprite>())
				{
					Color previousColor = this.GetColor(this._previousState);
					foreach (UISprite uiSprite in previousSprites)
					{
						if (this.SpriteCrossFadeTime <= 0f)
						{
							goto IL_214;
						}
						if (!uiSprite.CrossFadeOut)
						{
							if (currentSprites == null)
							{
								goto IL_214;
							}
							if (!currentSprites.Any((UISprite s) => s.CrossFadeIn))
							{
								goto IL_214;
							}
						}
						float num = MathUtils.InverseLerp(0f, this.SpriteCrossFadeTime, ToolBox.GetEasing(uiSprite.TransitionMode, this.spriteFadeTimer));
						IL_23D:
						float alphaMultiplier = num;
						if (alphaMultiplier > 0f)
						{
							uiSprite.Draw(spriteBatch, rect, previousColor * alphaMultiplier, this.SpriteEffects, new Vector2?(this.UVOffset));
							continue;
						}
						continue;
						IL_214:
						num = 0f;
						goto IL_23D;
					}
				}
				if (currentSprites != null && currentSprites.Any<UISprite>())
				{
					foreach (UISprite uiSprite2 in currentSprites)
					{
						if (this.SpriteCrossFadeTime <= 0f)
						{
							goto IL_2EE;
						}
						if (!uiSprite2.CrossFadeIn)
						{
							if (previousSprites == null)
							{
								goto IL_2EE;
							}
							if (!previousSprites.Any((UISprite s) => s.CrossFadeOut))
							{
								goto IL_2EE;
							}
						}
						float num2 = MathUtils.InverseLerp(this.SpriteCrossFadeTime, 0f, ToolBox.GetEasing(uiSprite2.TransitionMode, this.spriteFadeTimer));
						IL_324:
						float alphaMultiplier2 = num2;
						if (alphaMultiplier2 > 0f)
						{
							Vector2 offset = new Vector2((float)MathUtils.PositiveModulo((int)(-(int)this.UVOffset.X), uiSprite2.Sprite.SourceRect.Width), (float)MathUtils.PositiveModulo((int)(-(int)this.UVOffset.Y), uiSprite2.Sprite.SourceRect.Height));
							uiSprite2.Draw(spriteBatch, rect, this._currentColor * alphaMultiplier2, this.SpriteEffects, new Vector2?(offset));
							continue;
						}
						continue;
						IL_2EE:
						num2 = (float)this._currentColor.A / 255f;
						goto IL_324;
					}
				}
			}
			if (this.GlowOnSelect && this.State == GUIComponent.ComponentState.Selected)
			{
				GUIStyle.UIGlow.Draw(spriteBatch, this.Rect, this.SelectedColor, SpriteEffects.None);
			}
			if (this.flashTimer > 0f)
			{
				int flashCycleCount = (int)Math.Max(this.flashDuration, 1f);
				float flashCycleDuration = this.flashDuration / (float)flashCycleCount;
				Rectangle flashRect = this.Rect;
				flashRect.Inflate(this.flashRectInflate.X, this.flashRectInflate.Y);
				if (this.useRectangleFlash)
				{
					GUI.DrawRectangle(spriteBatch, flashRect, this.flashColor * (float)Math.Sin((double)(this.flashTimer % flashCycleDuration / flashCycleDuration * 3.1415927f * 0.8f)), true, 0f, 1f);
					return;
				}
				GUISprite glow = this.useCircularFlash ? GUIStyle.UIGlowCircular : GUIStyle.UIGlow;
				glow.Draw(spriteBatch, flashRect, this.flashColor * (float)Math.Sin((double)(this.flashTimer % flashCycleDuration / flashCycleDuration * 3.1415927f * 0.8f)), SpriteEffects.None);
			}
		}

		// Token: 0x06001260 RID: 4704 RVA: 0x000B3B08 File Offset: 0x000B1D08
		public void DrawToolTip(SpriteBatch spriteBatch)
		{
			if (!this.Visible)
			{
				return;
			}
			GUIComponent.DrawToolTip(spriteBatch, this.ToolTip, this.Rect, Anchor.BottomCenter, Pivot.TopLeft);
		}

		// Token: 0x06001261 RID: 4705 RVA: 0x000B3B28 File Offset: 0x000B1D28
		public static void DrawToolTip(SpriteBatch spriteBatch, RichString toolTip, Vector2 pos, Color? textColor = null, Color? backgroundColor = null)
		{
			if (ObjectiveManager.ContentRunning)
			{
				return;
			}
			int width = (int)(400f * GUI.Scale);
			int height = (int)(18f * GUI.Scale);
			Point padding = new Point((int)(10f * GUI.Scale));
			if (GUIComponent.toolTipBlock == null || (RichString)GUIComponent.toolTipBlock.UserData != toolTip)
			{
				RectTransform rectT = new RectTransform(new Point(width, height), null, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUIComponent.toolTipBlock = new GUITextBlock(rectT, toolTip, null, smallFont, Alignment.Left, true, "GUIToolTip", null);
				if (textColor != null)
				{
					GUIComponent.toolTipBlock.TextColor = textColor.Value;
				}
				if (backgroundColor != null)
				{
					GUIComponent.toolTipBlock.Color = backgroundColor.Value;
				}
				GUIComponent.toolTipBlock.RectTransform.NonScaledSize = new Point((int)(GUIStyle.SmallFont.MeasureString(GUIComponent.toolTipBlock.WrappedText, false).X + (float)padding.X + GUIComponent.toolTipBlock.Padding.X + GUIComponent.toolTipBlock.Padding.Z), (int)(GUIStyle.SmallFont.MeasureString(GUIComponent.toolTipBlock.WrappedText, false).Y + (float)padding.Y + GUIComponent.toolTipBlock.Padding.Y + GUIComponent.toolTipBlock.Padding.W));
				GUIComponent.toolTipBlock.UserData = toolTip;
			}
			GUIComponent.toolTipBlock.RectTransform.AbsoluteOffset = pos.ToPoint();
			GUIComponent.toolTipBlock.SetTextPos();
			if (GUIComponent.toolTipBlock.Rect.Right > GameMain.GraphicsWidth - 10)
			{
				GUIComponent.toolTipBlock.RectTransform.AbsoluteOffset -= new Point(GUIComponent.toolTipBlock.Rect.Width, 0);
			}
			if (GUIComponent.toolTipBlock.Rect.Bottom > GameMain.GraphicsHeight - 10)
			{
				GUIComponent.toolTipBlock.RectTransform.AbsoluteOffset -= new Point(0, GUIComponent.toolTipBlock.Rect.Height);
			}
			GUIComponent.toolTipBlock.DrawManually(spriteBatch, false, true);
		}

		// Token: 0x06001262 RID: 4706 RVA: 0x000B3D70 File Offset: 0x000B1F70
		public static void DrawToolTip(SpriteBatch spriteBatch, RichString toolTip, Rectangle targetElement, Anchor anchor = Anchor.BottomCenter, Pivot pivot = Pivot.TopLeft)
		{
			GUIComponent.<>c__DisplayClass190_0 CS$<>8__locals1;
			CS$<>8__locals1.anchor = anchor;
			CS$<>8__locals1.targetElement = targetElement;
			CS$<>8__locals1.pivot = pivot;
			if (ObjectiveManager.ContentRunning)
			{
				return;
			}
			int width = (int)(400f * GUI.Scale);
			int height = (int)(18f * GUI.Scale);
			Point padding = new Point((int)(10f * GUI.Scale));
			if (GUIComponent.toolTipBlock == null || (RichString)GUIComponent.toolTipBlock.UserData != toolTip)
			{
				RectTransform rectT = new RectTransform(new Point(width, height), null, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUIComponent.toolTipBlock = new GUITextBlock(rectT, toolTip, null, smallFont, Alignment.Left, true, "GUIToolTip", null);
				GUIComponent.toolTipBlock.RectTransform.NonScaledSize = new Point((int)(GUIComponent.toolTipBlock.Font.MeasureString(GUIComponent.toolTipBlock.WrappedText, false).X + (float)padding.X + GUIComponent.toolTipBlock.Padding.X + GUIComponent.toolTipBlock.Padding.Z), (int)(GUIComponent.toolTipBlock.Font.MeasureString(GUIComponent.toolTipBlock.WrappedText, false).Y + (float)padding.Y + GUIComponent.toolTipBlock.Padding.Y + GUIComponent.toolTipBlock.Padding.W));
				GUIComponent.toolTipBlock.UserData = toolTip;
			}
			GUIComponent.<DrawToolTip>g__CalculateOffset|190_0(ref CS$<>8__locals1);
			if (GUIComponent.toolTipBlock.Rect.Right > GameMain.GraphicsWidth - 10)
			{
				CS$<>8__locals1.anchor = RectTransform.MoveAnchorLeft(CS$<>8__locals1.anchor);
				CS$<>8__locals1.pivot = (Pivot)RectTransform.MoveAnchorRight((Anchor)CS$<>8__locals1.pivot);
				GUIComponent.<DrawToolTip>g__CalculateOffset|190_0(ref CS$<>8__locals1);
			}
			if (GUIComponent.toolTipBlock.Rect.Bottom > GameMain.GraphicsHeight - 10)
			{
				CS$<>8__locals1.anchor = RectTransform.MoveAnchorTop(CS$<>8__locals1.anchor);
				CS$<>8__locals1.pivot = (Pivot)RectTransform.MoveAnchorBottom((Anchor)CS$<>8__locals1.pivot);
				GUIComponent.<DrawToolTip>g__CalculateOffset|190_0(ref CS$<>8__locals1);
			}
			GUIComponent.toolTipBlock.SetTextPos();
			GUIComponent.toolTipBlock.DrawManually(spriteBatch, false, true);
		}

		// Token: 0x06001263 RID: 4707 RVA: 0x000B3F94 File Offset: 0x000B2194
		protected virtual void SetAlpha(float a)
		{
			this.color = new Color((float)this.color.R / 255f, (float)this.color.G / 255f, (float)this.color.B / 255f, a);
			this.hoverColor = new Color((float)this.hoverColor.R / 255f, (float)this.hoverColor.G / 255f, (float)this.hoverColor.B / 255f, a);
			this.disabledColor = new Color((float)this.disabledColor.R / 255f, (float)this.disabledColor.G / 255f, (float)this.disabledColor.B / 255f, a);
		}

		// Token: 0x06001264 RID: 4708 RVA: 0x000B4068 File Offset: 0x000B2268
		public virtual void Flash(Color? color = null, float flashDuration = 1.5f, bool useRectangleFlash = false, bool useCircularFlash = false, Vector2? flashRectInflate = null)
		{
			this.flashTimer = flashDuration;
			this.flashRectInflate = (flashRectInflate ?? Vector2.Zero);
			this.useRectangleFlash = useRectangleFlash;
			this.useCircularFlash = useCircularFlash;
			this.flashDuration = flashDuration;
			this.flashColor = ((color == null) ? GUIStyle.Red : color.Value);
		}

		// Token: 0x06001265 RID: 4709 RVA: 0x000B40D4 File Offset: 0x000B22D4
		public void ImmediateFlash(Color? color = null)
		{
			this.flashTimer = 0.07853982f;
			this.flashDuration = 0.1f;
			this.flashColor = ((color == null) ? GUIStyle.Red : color.Value);
		}

		// Token: 0x06001266 RID: 4710 RVA: 0x000B4110 File Offset: 0x000B2310
		public void FadeOut(float duration, bool removeAfter, float wait = 0f, Action onRemove = null, bool alsoChildren = false)
		{
			CoroutineManager.StartCoroutine(this.LerpAlpha(0f, duration, removeAfter, wait, onRemove), "");
			if (alsoChildren)
			{
				foreach (GUIComponent child in this.Children)
				{
					child.FadeOut(duration, removeAfter, wait, onRemove, alsoChildren);
				}
			}
		}

		// Token: 0x06001267 RID: 4711 RVA: 0x000B4184 File Offset: 0x000B2384
		public void FadeIn(float wait, float duration, bool alsoChildren = false)
		{
			this.SetAlpha(0f);
			CoroutineManager.StartCoroutine(this.LerpAlpha(1f, duration, false, wait, null), "");
			if (alsoChildren)
			{
				foreach (GUIComponent child in this.Children)
				{
					child.FadeIn(wait, duration, alsoChildren);
				}
			}
		}

		// Token: 0x06001268 RID: 4712 RVA: 0x000B41FC File Offset: 0x000B23FC
		public void SlideIn(float wait, float duration, int amount, SlideDirection direction)
		{
			RectTransform rectTransform = this.RectTransform;
			Point screenSpaceOffset;
			switch (direction)
			{
			case SlideDirection.Up:
				screenSpaceOffset = new Point(0, amount);
				break;
			case SlideDirection.Down:
				screenSpaceOffset = new Point(0, -amount);
				break;
			case SlideDirection.Left:
				screenSpaceOffset = new Point(amount, 0);
				break;
			case SlideDirection.Right:
				screenSpaceOffset = new Point(-amount, 0);
				break;
			default:
				screenSpaceOffset = this.RectTransform.ScreenSpaceOffset;
				break;
			}
			rectTransform.ScreenSpaceOffset = screenSpaceOffset;
			CoroutineManager.StartCoroutine(this.SlideToPosition(duration, wait, Vector2.Zero), "");
		}

		// Token: 0x06001269 RID: 4713 RVA: 0x000B4280 File Offset: 0x000B2480
		public void SlideOut(float duration, int amount, SlideDirection direction)
		{
			this.RectTransform.ScreenSpaceOffset = Point.Zero;
			Vector2 vector;
			switch (direction)
			{
			case SlideDirection.Up:
				vector = new Vector2(0f, (float)amount);
				break;
			case SlideDirection.Down:
				vector = new Vector2(0f, (float)(-(float)amount));
				break;
			case SlideDirection.Left:
				vector = new Vector2((float)amount, 0f);
				break;
			case SlideDirection.Right:
				vector = new Vector2((float)(-(float)amount), 0f);
				break;
			default:
				vector = Vector2.Zero;
				break;
			}
			Vector2 targetPos = vector;
			CoroutineManager.StartCoroutine(this.SlideToPosition(duration, 0f, targetPos), "");
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x000B4313 File Offset: 0x000B2513
		private IEnumerable<CoroutineStatus> SlideToPosition(float duration, float wait, Vector2 target)
		{
			GUIComponent.<SlideToPosition>d__198 <SlideToPosition>d__ = new GUIComponent.<SlideToPosition>d__198(-2);
			<SlideToPosition>d__.<>4__this = this;
			<SlideToPosition>d__.<>3__duration = duration;
			<SlideToPosition>d__.<>3__wait = wait;
			<SlideToPosition>d__.<>3__target = target;
			return <SlideToPosition>d__;
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x000B4338 File Offset: 0x000B2538
		private IEnumerable<CoroutineStatus> LerpAlpha(float to, float duration, bool removeAfter, float wait = 0f, Action onRemove = null)
		{
			GUIComponent.<LerpAlpha>d__199 <LerpAlpha>d__ = new GUIComponent.<LerpAlpha>d__199(-2);
			<LerpAlpha>d__.<>4__this = this;
			<LerpAlpha>d__.<>3__to = to;
			<LerpAlpha>d__.<>3__duration = duration;
			<LerpAlpha>d__.<>3__removeAfter = removeAfter;
			<LerpAlpha>d__.<>3__wait = wait;
			<LerpAlpha>d__.<>3__onRemove = onRemove;
			return <LerpAlpha>d__;
		}

		// Token: 0x0600126C RID: 4716 RVA: 0x000B436D File Offset: 0x000B256D
		public void Pulsate(Vector2 startScale, Vector2 endScale, float duration)
		{
			if (CoroutineManager.IsCoroutineRunning(this.pulsateCoroutine))
			{
				return;
			}
			this.pulsateCoroutine = CoroutineManager.StartCoroutine(this.DoPulsate(startScale, endScale, duration), "Pulsate" + this.ToString());
		}

		// Token: 0x0600126D RID: 4717 RVA: 0x000B43A1 File Offset: 0x000B25A1
		private IEnumerable<CoroutineStatus> DoPulsate(Vector2 startScale, Vector2 endScale, float duration)
		{
			GUIComponent.<DoPulsate>d__201 <DoPulsate>d__ = new GUIComponent.<DoPulsate>d__201(-2);
			<DoPulsate>d__.<>4__this = this;
			<DoPulsate>d__.<>3__startScale = startScale;
			<DoPulsate>d__.<>3__endScale = endScale;
			<DoPulsate>d__.<>3__duration = duration;
			return <DoPulsate>d__;
		}

		// Token: 0x0600126E RID: 4718 RVA: 0x000B43C8 File Offset: 0x000B25C8
		public virtual void ApplyStyle(GUIComponentStyle style)
		{
			if (style == null)
			{
				return;
			}
			this.color = style.Color;
			this._currentColor = this.color;
			this.hoverColor = style.HoverColor;
			this.selectedColor = style.SelectedColor;
			this.pressedColor = style.PressedColor;
			this.disabledColor = style.DisabledColor;
			this.sprites = style.Sprites;
			this.OutlineColor = style.OutlineColor;
			this.SpriteCrossFadeTime = style.SpriteCrossFadeTime;
			this.ColorCrossFadeTime = style.ColorCrossFadeTime;
			this.ColorTransition = style.TransitionMode;
			this.FallBackState = style.FallBackState;
			if (this.rectTransform != null)
			{
				this.ApplySizeRestrictions(style);
			}
			this.styleHierarchy = GUIComponentStyle.ToHierarchy(style);
		}

		// Token: 0x0600126F RID: 4719 RVA: 0x000B4484 File Offset: 0x000B2684
		public void ApplySizeRestrictions(GUIComponentStyle style)
		{
			if (style.Width != null)
			{
				this.RectTransform.MinSize = new Point(style.Width.Value, this.RectTransform.MinSize.Y);
				this.RectTransform.MaxSize = new Point(style.Width.Value, this.RectTransform.MaxSize.Y);
				if (this.rectTransform.IsFixedSize)
				{
					this.RectTransform.Resize(new Point(style.Width.Value, this.rectTransform.NonScaledSize.Y), true);
				}
			}
			if (style.Height != null)
			{
				this.RectTransform.MinSize = new Point(this.RectTransform.MinSize.X, style.Height.Value);
				this.RectTransform.MaxSize = new Point(this.RectTransform.MaxSize.X, style.Height.Value);
				if (this.rectTransform.IsFixedSize)
				{
					this.RectTransform.Resize(new Point(this.rectTransform.NonScaledSize.X, style.Height.Value), true);
				}
			}
		}

		// Token: 0x06001270 RID: 4720 RVA: 0x000B45E7 File Offset: 0x000B27E7
		public void InheritTotalChildrenMinHeight()
		{
			this.RectTransform.InheritTotalChildrenMinHeight();
		}

		// Token: 0x06001271 RID: 4721 RVA: 0x000B45F4 File Offset: 0x000B27F4
		public void InheritTotalChildrenHeight()
		{
			this.RectTransform.InheritTotalChildrenHeight();
		}

		// Token: 0x06001272 RID: 4722 RVA: 0x000B4604 File Offset: 0x000B2804
		public static GUIComponent FromXML(ContentXElement element, RectTransform parent)
		{
			GUIComponent component = null;
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("conditional", StringComparison.OrdinalIgnoreCase) && !GUIComponent.CheckConditional(subElement))
				{
					return null;
				}
			}
			string text = element.Name.ToString().ToLowerInvariant();
			if (text != null)
			{
				switch (text.Length)
				{
				case 4:
				{
					char c2 = text[0];
					if (c2 != 'l')
					{
						if (c2 != 't')
						{
							goto IL_38F;
						}
						if (!(text == "text"))
						{
							goto IL_38F;
						}
					}
					else
					{
						if (!(text == "link"))
						{
							goto IL_38F;
						}
						component = GUIComponent.LoadLink(element, parent);
						goto IL_3B6;
					}
					break;
				}
				case 5:
				{
					char c2 = text[0];
					if (c2 != 'f')
					{
						if (c2 != 'i')
						{
							goto IL_38F;
						}
						if (!(text == "image"))
						{
							goto IL_38F;
						}
						goto IL_363;
					}
					else
					{
						if (!(text == "frame"))
						{
							goto IL_38F;
						}
						goto IL_324;
					}
					break;
				}
				case 6:
					if (!(text == "button"))
					{
						goto IL_38F;
					}
					goto IL_336;
				case 7:
				{
					char c2 = text[0];
					if (c2 != 'l')
					{
						if (c2 != 's')
						{
							goto IL_38F;
						}
						if (!(text == "spacing"))
						{
							goto IL_38F;
						}
						goto IL_324;
					}
					else
					{
						if (!(text == "listbox"))
						{
							goto IL_38F;
						}
						goto IL_345;
					}
					break;
				}
				case 8:
				{
					char c2 = text[3];
					if (c2 != 'd')
					{
						if (c2 != 'f')
						{
							if (c2 != 'i')
							{
								goto IL_38F;
							}
							if (!(text == "guiimage"))
							{
								goto IL_38F;
							}
							goto IL_363;
						}
						else
						{
							if (!(text == "guiframe"))
							{
								goto IL_38F;
							}
							goto IL_324;
						}
					}
					else
					{
						if (!(text == "gridtext"))
						{
							goto IL_38F;
						}
						GUIComponent.LoadGridText(element, parent);
						return null;
					}
					break;
				}
				case 9:
				{
					char c2 = text[0];
					if (c2 != 'a')
					{
						if (c2 != 'g')
						{
							goto IL_38F;
						}
						if (!(text == "guibutton"))
						{
							goto IL_38F;
						}
						goto IL_336;
					}
					else
					{
						if (!(text == "accordion"))
						{
							goto IL_38F;
						}
						return GUIComponent.LoadAccordion(element, parent, element.GetAttributeBool("openontop", false));
					}
					break;
				}
				case 10:
					if (!(text == "guilistbox"))
					{
						goto IL_38F;
					}
					goto IL_345;
				case 11:
				{
					char c2 = text[0];
					if (c2 != 'c')
					{
						if (c2 != 'l')
						{
							goto IL_38F;
						}
						if (!(text == "layoutgroup"))
						{
							goto IL_38F;
						}
						goto IL_354;
					}
					else
					{
						if (!(text == "conditional"))
						{
							goto IL_38F;
						}
						goto IL_3B6;
					}
					break;
				}
				case 12:
					if (!(text == "guitextblock"))
					{
						goto IL_38F;
					}
					break;
				case 13:
					goto IL_38F;
				case 14:
					if (!(text == "guilayoutgroup"))
					{
						goto IL_38F;
					}
					goto IL_354;
				default:
					goto IL_38F;
				}
				component = GUIComponent.LoadGUITextBlock(element, parent, null, null);
				goto IL_3B6;
				IL_324:
				component = GUIComponent.LoadGUIFrame(element, parent);
				goto IL_3B6;
				IL_336:
				component = GUIComponent.LoadGUIButton(element, parent);
				goto IL_3B6;
				IL_345:
				component = GUIComponent.LoadGUIListBox(element, parent);
				goto IL_3B6;
				IL_354:
				component = GUIComponent.LoadGUILayoutGroup(element, parent);
				goto IL_3B6;
				IL_363:
				component = GUIComponent.LoadGUIImage(element, parent);
				IL_3B6:
				if (component != null)
				{
					foreach (ContentXElement subElement2 in element.Elements())
					{
						if (!subElement2.Name.ToString().Equals("conditional", StringComparison.OrdinalIgnoreCase))
						{
							ContentXElement element2 = subElement2;
							GUIListBox listBox2 = component as GUIListBox;
							GUIComponent.FromXML(element2, (listBox2 != null) ? listBox2.Content.RectTransform : component.RectTransform);
						}
					}
					component.toolTip = element.GetAttributeString("tooltip", string.Empty);
					GUITextBlock guitextBlock;
					if ((guitextBlock = (component as GUITextBlock)) == null)
					{
						GUIButton guibutton = component as GUIButton;
						guitextBlock = ((guibutton != null) ? guibutton.TextBlock : null);
					}
					GUITextBlock textBlock = guitextBlock;
					if (textBlock != null)
					{
						if (element.GetAttributeBool("autoscalevertical", false))
						{
							textBlock.AutoScaleVertical = true;
						}
						if (element.GetAttributeBool("autoscalehorizontal", false))
						{
							textBlock.AutoScaleHorizontal = true;
						}
					}
					if (element.GetAttributeBool("resizetofitchildren", false))
					{
						string key = "relativeresizescale";
						Vector2 one = Vector2.One;
						Vector2 relativeResizeScale = element.GetAttributeVector2(key, one);
						GUILayoutGroup layoutGroup = component as GUILayoutGroup;
						if (layoutGroup != null)
						{
							RectTransform rectTransform = layoutGroup.RectTransform;
							Point nonScaledSize;
							if (!layoutGroup.IsHorizontal)
							{
								RectTransform rectTransform2 = component.RectTransform;
								Point point = new Point(layoutGroup.Rect.Width, layoutGroup.Children.Sum((GUIComponent c) => c.Rect.Height));
								rectTransform2.MinSize = point;
								nonScaledSize = point;
							}
							else
							{
								nonScaledSize = new Point(layoutGroup.Children.Sum((GUIComponent c) => c.Rect.Width), layoutGroup.Rect.Height);
							}
							rectTransform.NonScaledSize = nonScaledSize;
							if (layoutGroup.CountChildren > 0)
							{
								layoutGroup.RectTransform.NonScaledSize += (layoutGroup.IsHorizontal ? new Point((int)((float)(layoutGroup.CountChildren - 1) * ((float)layoutGroup.AbsoluteSpacing + (float)layoutGroup.Rect.Width * layoutGroup.RelativeSpacing)), 0) : new Point(0, (int)((float)(layoutGroup.CountChildren - 1) * ((float)layoutGroup.AbsoluteSpacing + (float)layoutGroup.Rect.Height * layoutGroup.RelativeSpacing))));
							}
						}
						else
						{
							GUIListBox listBox = component as GUIListBox;
							if (listBox != null)
							{
								RectTransform rectTransform3 = listBox.RectTransform;
								Point nonScaledSize2;
								if (!listBox.ScrollBar.IsHorizontal)
								{
									RectTransform rectTransform4 = component.RectTransform;
									Point point = new Point(listBox.Rect.Width, listBox.Children.Sum((GUIComponent c) => c.Rect.Height + listBox.Spacing));
									rectTransform4.MinSize = point;
									nonScaledSize2 = point;
								}
								else
								{
									nonScaledSize2 = new Point(listBox.Children.Sum((GUIComponent c) => c.Rect.Width + listBox.Spacing), listBox.Rect.Height);
								}
								rectTransform3.NonScaledSize = nonScaledSize2;
							}
							else
							{
								component.RectTransform.NonScaledSize = new Point(component.Children.Max((GUIComponent c) => c.Rect.Right) - component.Children.Min((GUIComponent c) => c.Rect.X), component.Children.Max((GUIComponent c) => c.Rect.Bottom) - component.Children.Min((GUIComponent c) => c.Rect.Y));
							}
						}
						component.RectTransform.NonScaledSize = component.RectTransform.NonScaledSize.Multiply(relativeResizeScale);
					}
				}
				return component;
			}
			IL_38F:
			string str = "Loading GUI component \"";
			XName name = element.Name;
			throw new NotImplementedException(str + ((name != null) ? name.ToString() : null) + "\" from XML is not implemented.");
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x000B4DCC File Offset: 0x000B2FCC
		private static bool CheckConditional(XElement element)
		{
			foreach (XAttribute attribute in element.Attributes())
			{
				string conditionName = attribute.Name.ToString().ToLowerInvariant();
				if (conditionName != null)
				{
					switch (conditionName.Length)
					{
					case 8:
					{
						if (!(conditionName == "language"))
						{
							continue;
						}
						IEnumerable<LanguageIdentifier> languages = from s in element.GetAttributeIdentifierArray(attribute.Name.ToString(), Array.Empty<Identifier>(), true)
						select new LanguageIdentifier(s);
						if (!languages.Any((LanguageIdentifier l) => GameSettings.CurrentConfig.Language == l))
						{
							return false;
						}
						continue;
					}
					case 9:
					case 10:
					case 12:
					case 17:
						continue;
					case 11:
					{
						if (!(conditionName == "gameversion"))
						{
							continue;
						}
						Version version = new Version(attribute.Value);
						if (GameMain.Version != version)
						{
							return false;
						}
						continue;
					}
					case 13:
						if (!(conditionName == "appsubscribed"))
						{
							continue;
						}
						break;
					case 14:
					{
						char c = conditionName[1];
						if (c != 'a')
						{
							if (c != 'i')
							{
								continue;
							}
							if (!(conditionName == "mingameversion"))
							{
								continue;
							}
							Version minVersion = new Version(attribute.Value);
							if (GameMain.Version < minVersion)
							{
								return false;
							}
							continue;
						}
						else
						{
							if (!(conditionName == "maxgameversion"))
							{
								continue;
							}
							Version maxVersion = new Version(attribute.Value);
							if (GameMain.Version > maxVersion)
							{
								return false;
							}
							continue;
						}
						break;
					}
					case 15:
					{
						if (!(conditionName == "mingamelaunches"))
						{
							continue;
						}
						int minLaunches;
						if (int.TryParse(attribute.Value, out minLaunches))
						{
							return SteamManager.GetStatInt(AchievementStat.GameLaunchCount) > minLaunches;
						}
						return false;
					}
					case 16:
						if (!(conditionName == "appnotsubscribed"))
						{
							continue;
						}
						break;
					case 18:
					{
						if (!(conditionName == "buildconfiguration"))
						{
							continue;
						}
						string a = attribute.Value.ToString().ToLowerInvariant();
						if (!(a == "debug") && !(a == "unstable") && a == "release")
						{
							return true;
						}
						return false;
					}
					case 19:
					{
						if (!(conditionName == "identifierdismissed"))
						{
							continue;
						}
						Identifier identifier = element.GetAttributeIdentifier(attribute.Name.ToString(), Identifier.Empty);
						if (MainMenuScreen.DismissedNotifications.Contains(identifier))
						{
							return false;
						}
						continue;
					}
					default:
						continue;
					}
					int appId;
					if (SteamManager.IsInitialized && int.TryParse(attribute.Value, out appId))
					{
						return SteamApps.IsSubscribedToApp(appId) == (conditionName == "appsubscribed");
					}
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001274 RID: 4724 RVA: 0x000B5110 File Offset: 0x000B3310
		private static GUITextBlock LoadGUITextBlock(XElement element, RectTransform parent, string overrideText = null, Anchor? anchor = null)
		{
			string text = overrideText ?? ((element.Attribute("text") == null) ? element.ElementInnerText() : element.GetAttributeString("text", ""));
			text = text.Replace("\\n", "\n");
			string style = element.GetAttributeString("style", "");
			if (style == "null")
			{
				style = null;
			}
			Color? color = null;
			if (element.Attribute("color") != null)
			{
				color = new Color?(element.GetAttributeColor("color", Color.White));
			}
			float scale = element.GetAttributeFloat("scale", 1f);
			bool wrap = element.GetAttributeBool("wrap", true);
			Alignment alignment = element.GetAttributeEnum("alignment", text.Contains('\n') ? Alignment.Left : Alignment.Center);
			GUIFont font;
			if (!GUIStyle.Fonts.TryGetValue(element.GetAttributeIdentifier("font", "Font"), out font))
			{
				font = GUIStyle.Font;
			}
			GUITextBlock textBlock = new GUITextBlock(RectTransform.Load(element, parent, Anchor.TopLeft), RichString.Rich(text, null), color, font, alignment, wrap, style, null)
			{
				TextScale = scale
			};
			if (anchor != null)
			{
				textBlock.RectTransform.SetPosition(anchor.Value, null);
			}
			textBlock.RectTransform.IsFixedSize = true;
			textBlock.RectTransform.NonScaledSize = new Point(textBlock.Rect.Width, textBlock.Rect.Height);
			return textBlock;
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x000B52A4 File Offset: 0x000B34A4
		private static GUIButton LoadLink(XElement element, RectTransform parent)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			GUIButton button = GUIComponent.LoadGUIButton(element, parent);
			string url = element.GetAttributeString("url", "");
			button.OnClicked = delegate(GUIButton btn, object userdata)
			{
				try
				{
					if (!identifier.IsEmpty)
					{
						MainMenuScreen.AddDismissedNotification(identifier);
					}
					if (SteamManager.IsInitialized)
					{
						SteamManager.OverlayCustomUrl(url);
					}
					else
					{
						ToolBox.OpenFileWithShell(url);
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to open url \"" + url + "\".", e, null, false, false);
				}
				return true;
			};
			return button;
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x000B5300 File Offset: 0x000B3500
		private static void LoadGridText(XElement element, RectTransform parent)
		{
			string text = (element.Attribute("text") == null) ? element.ElementInnerText() : element.GetAttributeString("text", "");
			text = text.Replace("\\n", "\n");
			string[] elements = text.Split(',', StringSplitOptions.None);
			RectTransform lineContainer = null;
			for (int i = 0; i < elements.Length; i++)
			{
				switch (i % 3)
				{
				case 0:
					lineContainer = GUIComponent.LoadGUITextBlock(element, parent, elements[i], new Anchor?(Anchor.CenterLeft)).RectTransform;
					lineContainer.Anchor = Anchor.TopCenter;
					lineContainer.Pivot = Pivot.TopCenter;
					lineContainer.NonScaledSize = new Point((int)((float)parent.NonScaledSize.X * 0.7f), lineContainer.NonScaledSize.Y);
					break;
				case 1:
					GUIComponent.LoadGUITextBlock(element, lineContainer, elements[i], new Anchor?(Anchor.Center)).TextAlignment = Alignment.Center;
					break;
				case 2:
					GUIComponent.LoadGUITextBlock(element, lineContainer, elements[i], new Anchor?(Anchor.CenterRight)).TextAlignment = Alignment.CenterRight;
					break;
				}
			}
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x000B5404 File Offset: 0x000B3604
		private static GUIFrame LoadGUIFrame(XElement element, RectTransform parent)
		{
			string style = element.GetAttributeString("style", element.Name.ToString().Equals("spacing", StringComparison.OrdinalIgnoreCase) ? null : "");
			if (style == "null")
			{
				style = null;
			}
			return new GUIFrame(RectTransform.Load(element, parent, Anchor.TopLeft), style, null);
		}

		// Token: 0x06001278 RID: 4728 RVA: 0x000B5464 File Offset: 0x000B3664
		private static GUIButton LoadGUIButton(XElement element, RectTransform parent)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			string style = element.GetAttributeString("style", "");
			if (style == "null")
			{
				style = null;
			}
			Alignment textAlignment = Alignment.Center;
			Enum.TryParse<Alignment>(element.GetAttributeString("textalignment", "Center"), out textAlignment);
			string text = (element.Attribute("text") == null) ? element.ElementInnerText() : element.GetAttributeString("text", "");
			text = text.Replace("\\n", "\n");
			return new GUIButton(RectTransform.Load(element, parent, Anchor.TopLeft), text, textAlignment, style, null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					if (!identifier.IsEmpty)
					{
						MainMenuScreen.AddDismissedNotification(identifier);
					}
					return true;
				}
			};
		}

		// Token: 0x06001279 RID: 4729 RVA: 0x000B553C File Offset: 0x000B373C
		private static GUIListBox LoadGUIListBox(XElement element, RectTransform parent)
		{
			string style = element.GetAttributeString("style", "");
			if (style == "null")
			{
				style = null;
			}
			bool isHorizontal = element.GetAttributeBool("ishorizontal", !element.GetAttributeBool("isvertical", true));
			RectTransform rectT = RectTransform.Load(element, parent, Anchor.TopLeft);
			bool isHorizontal2 = isHorizontal;
			string style2 = style;
			return new GUIListBox(rectT, isHorizontal2, null, style2, true, false);
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x000B55A0 File Offset: 0x000B37A0
		private static GUILayoutGroup LoadGUILayoutGroup(XElement element, RectTransform parent)
		{
			bool isHorizontal = element.GetAttributeBool("ishorizontal", !element.GetAttributeBool("isvertical", true));
			Anchor childAnchor;
			Enum.TryParse<Anchor>(element.GetAttributeString("childanchor", "TopLeft"), out childAnchor);
			return new GUILayoutGroup(RectTransform.Load(element, parent, Anchor.TopLeft), isHorizontal, childAnchor)
			{
				Stretch = element.GetAttributeBool("stretch", false),
				RelativeSpacing = element.GetAttributeFloat("relativespacing", 0f),
				AbsoluteSpacing = element.GetAttributeInt("absolutespacing", 0)
			};
		}

		// Token: 0x0600127B RID: 4731 RVA: 0x000B562C File Offset: 0x000B382C
		private static GUIImage LoadGUIImage(ContentXElement element, RectTransform parent)
		{
			string url = element.GetAttributeString("url", "");
			Sprite sprite;
			if (!string.IsNullOrEmpty(url))
			{
				string localFileName = Path.GetFileNameWithoutExtension(url.Replace("/", "").Replace(":", "").Replace("https", "").Replace("http", "")).Replace(".", "");
				localFileName += Path.GetExtension(url);
				string localFilePath = Path.Combine(new string[]
				{
					"Downloads",
					localFileName
				});
				if (!File.Exists(localFilePath))
				{
					Uri baseAddress = new Uri(url);
					Uri remoteDirectory = new Uri(baseAddress, ".");
					string remoteFileName = Path.GetFileName(baseAddress.LocalPath);
					RestClient client = RestFactory.CreateClient(remoteDirectory.ToString());
					RestRequest request = RestFactory.CreateRequest(remoteFileName, Method.GET);
					IRestResponse response = client.Execute(request);
					if (response.ErrorException != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Connection error: Failed to load remote sprite from ");
						defaultInterpolatedStringHandler.AppendFormatted(url);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendLiteral("(");
						defaultInterpolatedStringHandler.AppendFormatted(response.ErrorException.Message);
						defaultInterpolatedStringHandler.AppendLiteral(").");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						return null;
					}
					if (response.ResponseStatus != ResponseStatus.Completed)
					{
						return null;
					}
					if (response.StatusCode != HttpStatusCode.OK)
					{
						return null;
					}
					if (!Directory.Exists("Downloads"))
					{
						Directory.CreateDirectory("Downloads", false);
					}
					File.WriteAllBytes(localFilePath, response.RawBytes, true);
				}
				sprite = new Sprite(element, "Downloads", localFileName, false, 1f);
			}
			else
			{
				sprite = new Sprite(element, "", "", false, 1f);
			}
			string key = "scaletofit";
			GUIImage.ScalingMode scaleToFit2 = GUIImage.ScalingMode.ScaleToFitSmallestExtent;
			GUIImage.ScalingMode scaleToFit = element.GetAttributeEnum<GUIImage.ScalingMode>(key, scaleToFit2);
			RectTransform rectT = RectTransform.Load(element, parent, Anchor.TopLeft);
			Sprite sprite2 = sprite;
			scaleToFit2 = scaleToFit;
			return new GUIImage(rectT, sprite2, null, scaleToFit2);
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x000B5830 File Offset: 0x000B3A30
		private static GUIButton LoadAccordion(ContentXElement element, RectTransform parent, bool openOnTop)
		{
			GUIButton button = GUIComponent.LoadGUIButton(element, parent);
			List<GUIComponent> content = new List<GUIComponent>();
			foreach (ContentXElement subElement in element.Elements())
			{
				GUIComponent contentElement = GUIComponent.FromXML(subElement, parent);
				if (contentElement != null)
				{
					contentElement.Visible = false;
					contentElement.IgnoreLayoutGroups = true;
					content.Add(contentElement);
					contentElement.UserData = new ValueTuple<Anchor, Pivot>(contentElement.RectTransform.Anchor, contentElement.RectTransform.Pivot);
				}
			}
			button.OnClicked = delegate(GUIButton btn, object userdata)
			{
				GUIComponent guicomponent = content.FirstOrDefault<GUIComponent>();
				bool visible = guicomponent == null || guicomponent.Visible;
				foreach (GUIComponent contentElement2 in content)
				{
					if (openOnTop)
					{
						contentElement2.rectTransform.Parent = null;
						contentElement2.rectTransform.SetPosition(Anchor.TopLeft, null);
						ValueTuple<Anchor, Pivot> valueTuple = (ValueTuple<Anchor, Pivot>)contentElement2.UserData;
						Anchor anchor = valueTuple.Item1;
						Pivot pivot = valueTuple.Item2;
						contentElement2.rectTransform.ScreenSpaceOffset = RectTransform.CalculateAnchorPoint(anchor, button.Rect) + RectTransform.CalculatePivotOffset(pivot, contentElement2.Rect.Size);
						contentElement2.Visible = true;
						if (GUIComponent.OpenAccordionPopups.Contains(contentElement2))
						{
							GUIComponent.OpenAccordionPopups.Remove(contentElement2);
						}
						else
						{
							GUIComponent.OpenAccordionPopups.Clear();
							GUIComponent.OpenAccordionPopups.Add(contentElement2);
						}
					}
					else
					{
						contentElement2.Visible = !visible;
						contentElement2.IgnoreLayoutGroups = !contentElement2.Visible;
					}
				}
				GUILayoutGroup layoutGroup = button.Parent as GUILayoutGroup;
				if (layoutGroup != null)
				{
					layoutGroup.Recalculate();
				}
				return true;
			};
			return button;
		}

		// Token: 0x0600127E RID: 4734 RVA: 0x000B5914 File Offset: 0x000B3B14
		[CompilerGenerated]
		internal static void <DrawToolTip>g__CalculateOffset|190_0(ref GUIComponent.<>c__DisplayClass190_0 A_0)
		{
			GUIComponent.toolTipBlock.RectTransform.AbsoluteOffset = RectTransform.CalculateAnchorPoint(A_0.anchor, A_0.targetElement) + RectTransform.CalculatePivotOffset(A_0.pivot, GUIComponent.toolTipBlock.RectTransform.NonScaledSize);
		}

		// Token: 0x0400090A RID: 2314
		public CursorState HoverCursor;

		// Token: 0x0400090B RID: 2315
		public bool AlwaysOverrideCursor;

		// Token: 0x0400090C RID: 2316
		public GUIComponent.SecondaryButtonDownHandler OnSecondaryClicked;

		// Token: 0x04000911 RID: 2321
		private float bounceTimer;

		// Token: 0x04000912 RID: 2322
		private float bounceJump;

		// Token: 0x04000913 RID: 2323
		private bool bounceDown;

		// Token: 0x04000914 RID: 2324
		public Action<GUIComponent> OnAddedToGUIUpdateList;

		// Token: 0x04000915 RID: 2325
		public Action<GUIComponent> OnDrawToolTip;

		// Token: 0x04000916 RID: 2326
		protected Alignment alignment;

		// Token: 0x04000917 RID: 2327
		protected Identifier[] styleHierarchy;

		// Token: 0x04000918 RID: 2328
		public bool CanBeFocused;

		// Token: 0x04000919 RID: 2329
		protected Color color;

		// Token: 0x0400091A RID: 2330
		protected Color hoverColor;

		// Token: 0x0400091B RID: 2331
		protected Color selectedColor;

		// Token: 0x0400091C RID: 2332
		protected Color disabledColor;

		// Token: 0x0400091D RID: 2333
		protected Color pressedColor;

		// Token: 0x04000920 RID: 2336
		private CoroutineHandle pulsateCoroutine;

		// Token: 0x04000921 RID: 2337
		protected Color flashColor;

		// Token: 0x04000922 RID: 2338
		protected float flashDuration = 1.5f;

		// Token: 0x04000923 RID: 2339
		private bool useRectangleFlash;

		// Token: 0x04000924 RID: 2340
		private bool useCircularFlash;

		// Token: 0x04000925 RID: 2341
		protected float flashTimer;

		// Token: 0x04000926 RID: 2342
		private Vector2 flashRectInflate;

		// Token: 0x04000927 RID: 2343
		private bool ignoreLayoutGroups;

		// Token: 0x04000929 RID: 2345
		private RichString toolTip;

		// Token: 0x0400092B RID: 2347
		protected bool enabled;

		// Token: 0x0400092C RID: 2348
		private static GUITextBlock toolTipBlock;

		// Token: 0x0400092E RID: 2350
		public Dictionary<GUIComponent.ComponentState, List<UISprite>> sprites;

		// Token: 0x0400092F RID: 2351
		public SpriteEffects SpriteEffects;

		// Token: 0x04000931 RID: 2353
		protected GUIComponent.ComponentState _state;

		// Token: 0x04000932 RID: 2354
		protected GUIComponent.ComponentState _previousState;

		// Token: 0x04000933 RID: 2355
		protected bool isSelected;

		// Token: 0x04000934 RID: 2356
		public object UserData;

		// Token: 0x0400093A RID: 2362
		private float spriteFadeTimer;

		// Token: 0x0400093B RID: 2363
		private float colorFadeTimer;

		// Token: 0x0400093C RID: 2364
		public bool ExternalHighlight;

		// Token: 0x0400093E RID: 2366
		private RectTransform rectTransform;

		// Token: 0x0400093F RID: 2367
		protected Color _currentColor;

		// Token: 0x04000940 RID: 2368
		public static readonly List<GUIComponent> OpenAccordionPopups = new List<GUIComponent>();

		// Token: 0x02000939 RID: 2361
		// (Invoke) Token: 0x06007134 RID: 28980
		public delegate bool SecondaryButtonDownHandler(GUIComponent component, object userData);

		// Token: 0x0200093A RID: 2362
		public enum ComponentState
		{
			// Token: 0x040040B0 RID: 16560
			None,
			// Token: 0x040040B1 RID: 16561
			Hover,
			// Token: 0x040040B2 RID: 16562
			Pressed,
			// Token: 0x040040B3 RID: 16563
			Selected,
			// Token: 0x040040B4 RID: 16564
			HoverSelected
		}
	}
}
