using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000077 RID: 119
	public class GUIComponentStyle : GUIPrefab
	{
		// Token: 0x06001104 RID: 4356 RVA: 0x000A6708 File Offset: 0x000A4908
		public static GUIComponentStyle FromHierarchy(IReadOnlyList<Identifier> hierarchy)
		{
			if (hierarchy == null || hierarchy.None(null))
			{
				return null;
			}
			GUIComponentStyle style;
			GUIStyle.ComponentStyles.TryGet(hierarchy[0], out style);
			for (int i = 1; i < hierarchy.Count; i++)
			{
				if (style == null)
				{
					return null;
				}
				style.ChildStyles.TryGetValue(hierarchy[i], out style);
			}
			return style;
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x000A6764 File Offset: 0x000A4964
		public static Identifier[] ToHierarchy(GUIComponentStyle style)
		{
			List<Identifier> ids = new List<Identifier>();
			while (style != null)
			{
				ids.Insert(0, style.Identifier);
				style = style.ParentStyle;
			}
			return ids.ToArray();
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06001106 RID: 4358 RVA: 0x000A6797 File Offset: 0x000A4997
		// (set) Token: 0x06001107 RID: 4359 RVA: 0x000A679F File Offset: 0x000A499F
		public int? Width { get; private set; }

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06001108 RID: 4360 RVA: 0x000A67A8 File Offset: 0x000A49A8
		// (set) Token: 0x06001109 RID: 4361 RVA: 0x000A67B0 File Offset: 0x000A49B0
		public int? Height { get; private set; }

		// Token: 0x0600110A RID: 4362 RVA: 0x000A67BC File Offset: 0x000A49BC
		public GUIComponentStyle(ContentXElement element, UIStyleFile file, GUIComponentStyle parent = null) : base(element, file)
		{
			this.Name = element.Name.LocalName;
			this.Element = element;
			this.Sprites = new Dictionary<GUIComponent.ComponentState, List<UISprite>>();
			foreach (object obj in Enum.GetValues(typeof(GUIComponent.ComponentState)))
			{
				GUIComponent.ComponentState state = (GUIComponent.ComponentState)obj;
				this.Sprites[state] = new List<UISprite>();
			}
			this.ParentStyle = parent;
			this.ChildStyles = new Dictionary<Identifier, GUIComponentStyle>();
			string key = "padding";
			Vector4 zero = Vector4.Zero;
			this.Padding = element.GetAttributeVector4(key, zero);
			string key2 = "color";
			Color color = Color.Transparent;
			this.Color = element.GetAttributeColor(key2, color);
			this.HoverColor = element.GetAttributeColor("hovercolor", this.Color);
			this.SelectedColor = element.GetAttributeColor("selectedcolor", this.Color);
			this.DisabledColor = element.GetAttributeColor("disabledcolor", this.Color);
			this.PressedColor = element.GetAttributeColor("pressedcolor", this.Color);
			string key3 = "outlinecolor";
			color = Color.Transparent;
			this.OutlineColor = element.GetAttributeColor(key3, color);
			string key4 = "textcolor";
			color = Color.Black;
			this.TextColor = element.GetAttributeColor(key4, color);
			this.HoverTextColor = element.GetAttributeColor("hovertextcolor", this.TextColor);
			this.DisabledTextColor = element.GetAttributeColor("disabledtextcolor", this.TextColor);
			this.SelectedTextColor = element.GetAttributeColor("selectedtextcolor", this.TextColor);
			this.SpriteCrossFadeTime = element.GetAttributeFloat("spritefadetime", this.SpriteCrossFadeTime);
			this.ColorCrossFadeTime = element.GetAttributeFloat("colorfadetime", this.ColorCrossFadeTime);
			TransitionMode transition;
			if (Enum.TryParse<TransitionMode>(element.GetAttributeString("colortransition", string.Empty), true, out transition))
			{
				this.TransitionMode = transition;
			}
			SpriteFallBackState s;
			if (Enum.TryParse<SpriteFallBackState>(element.GetAttributeString("fallbackstate", GUIComponent.ComponentState.None.ToString()), true, out s))
			{
				this.FallBackState = s;
			}
			this.Font = element.GetAttributeIdentifier("font", "");
			this.ForceUpperCase = element.GetAttributeBool("forceuppercase", false);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (a == "size")
					{
						continue;
					}
				}
				else
				{
					UISprite newSprite = new UISprite(subElement);
					Rectangle sourceRect = newSprite.Sprite.SourceRect;
					if ((sourceRect.Width <= 1 || sourceRect.Height <= 1) && newSprite.Tile)
					{
						DebugConsole.AddWarning("Sprite \"" + subElement.GetAttributeString("name", this.Name) + "\" has a size of 1 or less which may cause performance problems.", element.ContentPackage);
					}
					GUIComponent.ComponentState spriteState = GUIComponent.ComponentState.None;
					if (subElement.GetAttribute("state") != null)
					{
						string stateStr = subElement.GetAttributeString("state", "None");
						Enum.TryParse<GUIComponent.ComponentState>(stateStr, out spriteState);
						this.Sprites[spriteState].Add(newSprite);
						if (spriteState == GUIComponent.ComponentState.HoverSelected && !this.Sprites.ContainsKey(GUIComponent.ComponentState.HoverSelected))
						{
							this.Sprites[GUIComponent.ComponentState.HoverSelected].Add(newSprite);
							continue;
						}
						continue;
					}
					else
					{
						using (IEnumerator enumerator3 = Enum.GetValues(typeof(GUIComponent.ComponentState)).GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								object obj2 = enumerator3.Current;
								GUIComponent.ComponentState state2 = (GUIComponent.ComponentState)obj2;
								this.Sprites[state2].Add(newSprite);
							}
							continue;
						}
					}
				}
				Identifier styleName = subElement.NameAsIdentifier();
				if (this.ChildStyles.ContainsKey(styleName))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"UI style \"",
						element.Name.ToString(),
						"\" contains multiple child styles with the same name (\"",
						styleName.ToString(),
						"\")!"
					}), null, null, false, false);
					this.ChildStyles[styleName] = new GUIComponentStyle(subElement, file, this);
				}
				else
				{
					this.ChildStyles.Add(styleName, new GUIComponentStyle(subElement, file, this));
				}
			}
			this.GetSize(element);
		}

		// Token: 0x0600110B RID: 4363 RVA: 0x000A6C88 File Offset: 0x000A4E88
		public Sprite GetDefaultSprite()
		{
			return this.GetSprite(GUIComponent.ComponentState.None);
		}

		// Token: 0x0600110C RID: 4364 RVA: 0x000A6C91 File Offset: 0x000A4E91
		public Sprite GetSprite(GUIComponent.ComponentState state)
		{
			if (!this.Sprites.ContainsKey(state))
			{
				return null;
			}
			List<UISprite> list = this.Sprites[state];
			if (list == null)
			{
				return null;
			}
			UISprite uisprite = list.First<UISprite>();
			if (uisprite == null)
			{
				return null;
			}
			return uisprite.Sprite;
		}

		// Token: 0x0600110D RID: 4365 RVA: 0x000A6CC8 File Offset: 0x000A4EC8
		public void RefreshSize()
		{
			this.Width = null;
			this.Height = null;
			this.GetSize(this.Element);
			foreach (GUIComponentStyle childStyle in this.ChildStyles.Values)
			{
				childStyle.RefreshSize();
			}
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x000A6D50 File Offset: 0x000A4F50
		private void GetSize(XElement element)
		{
			Point size = new Point(0, 0);
			foreach (XElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("size", StringComparison.OrdinalIgnoreCase))
				{
					Point maxResolution = subElement.GetAttributePoint("maxresolution", new Point(int.MaxValue, int.MaxValue));
					if (GameMain.GraphicsWidth <= maxResolution.X && GameMain.GraphicsHeight <= maxResolution.Y)
					{
						size = new Point(base.ParseSize(subElement, "width"), base.ParseSize(subElement, "height"));
						break;
					}
				}
			}
			if (size.X > 0)
			{
				this.Width = new int?(size.X);
			}
			if (size.Y > 0)
			{
				this.Height = new int?(size.Y);
			}
		}

		// Token: 0x0600110F RID: 4367 RVA: 0x000A6E40 File Offset: 0x000A5040
		public override void Dispose()
		{
		}

		// Token: 0x04000870 RID: 2160
		public readonly Vector4 Padding;

		// Token: 0x04000871 RID: 2161
		public readonly Color Color;

		// Token: 0x04000872 RID: 2162
		public readonly Color HoverColor;

		// Token: 0x04000873 RID: 2163
		public readonly Color SelectedColor;

		// Token: 0x04000874 RID: 2164
		public readonly Color PressedColor;

		// Token: 0x04000875 RID: 2165
		public readonly Color DisabledColor;

		// Token: 0x04000876 RID: 2166
		public readonly Color TextColor;

		// Token: 0x04000877 RID: 2167
		public readonly Color HoverTextColor;

		// Token: 0x04000878 RID: 2168
		public readonly Color SelectedTextColor;

		// Token: 0x04000879 RID: 2169
		public readonly Color DisabledTextColor;

		// Token: 0x0400087A RID: 2170
		public readonly float SpriteCrossFadeTime;

		// Token: 0x0400087B RID: 2171
		public readonly float ColorCrossFadeTime;

		// Token: 0x0400087C RID: 2172
		public readonly TransitionMode TransitionMode;

		// Token: 0x0400087D RID: 2173
		public readonly Identifier Font;

		// Token: 0x0400087E RID: 2174
		public readonly bool ForceUpperCase;

		// Token: 0x0400087F RID: 2175
		public readonly Color OutlineColor;

		// Token: 0x04000880 RID: 2176
		public readonly ContentXElement Element;

		// Token: 0x04000881 RID: 2177
		public readonly Dictionary<GUIComponent.ComponentState, List<UISprite>> Sprites;

		// Token: 0x04000882 RID: 2178
		public SpriteFallBackState FallBackState;

		// Token: 0x04000883 RID: 2179
		public readonly GUIComponentStyle ParentStyle;

		// Token: 0x04000884 RID: 2180
		public readonly Dictionary<Identifier, GUIComponentStyle> ChildStyles;

		// Token: 0x04000885 RID: 2181
		public readonly string Name;
	}
}
