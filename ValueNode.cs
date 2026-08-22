using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200010A RID: 266
	[NullableContext(2)]
	[Nullable(0)]
	internal class ValueNode : EditorNode
	{
		// Token: 0x170009E3 RID: 2531
		// (get) Token: 0x060024BE RID: 9406 RVA: 0x001737DC File Offset: 0x001719DC
		// (set) Token: 0x060024BF RID: 9407 RVA: 0x001737E4 File Offset: 0x001719E4
		public object Value
		{
			get
			{
				return this.nodeValue;
			}
			set
			{
				this.nodeValue = value;
				string str = value as string;
				if (str != null)
				{
					LocalizedString translated = TextManager.Get(str);
					this.WrappedText = ((translated != null && translated.Loaded) ? translated.Value : str);
				}
				else
				{
					this.WrappedText = (((value != null) ? value.ToString() : null) ?? string.Empty);
				}
				this.valueTextSize = GUIStyle.SubHeadingFont.MeasureString(this.WrappedText, false);
			}
		}

		// Token: 0x170009E4 RID: 2532
		// (get) Token: 0x060024C0 RID: 9408 RVA: 0x0017385C File Offset: 0x00171A5C
		[Nullable(1)]
		public Type Type { [NullableContext(1)] get; }

		// Token: 0x060024C1 RID: 9409 RVA: 0x00173864 File Offset: 0x00171A64
		[NullableContext(1)]
		public ValueNode(Type type, string name) : base(name)
		{
			this.Type = type;
			this.Value = (type.IsValueType ? Activator.CreateInstance(type) : null);
			base.Size = new Vector2(256f, 32f);
			this.Connections.Add(new EventEditorNodeConnection(this, NodeConnectionType.Out, "Output", this.Type, null));
		}

		// Token: 0x060024C2 RID: 9410 RVA: 0x001738D8 File Offset: 0x00171AD8
		[NullableContext(1)]
		public override XElement Save()
		{
			XElement newElement = new XElement("ValueNode");
			newElement.Add(new XAttribute("i", this.ID));
			if (this.Value != null)
			{
				newElement.Add(new XAttribute("value", this.Value));
			}
			newElement.Add(new XAttribute("type", this.Type.ToString()));
			newElement.Add(new XAttribute("name", base.Name));
			newElement.Add(new XAttribute("xpos", base.Position.X));
			newElement.Add(new XAttribute("ypos", base.Position.Y));
			return newElement;
		}

		// Token: 0x060024C3 RID: 9411 RVA: 0x001739BE File Offset: 0x00171BBE
		public override XElement ToXML()
		{
			return null;
		}

		// Token: 0x060024C4 RID: 9412 RVA: 0x001739C4 File Offset: 0x00171BC4
		[NullableContext(1)]
		[return: Nullable(2)]
		public static EditorNode LoadValueNode(XElement element)
		{
			if (!string.Equals(element.Name.ToString(), "ValueNode", StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}
			string value = element.GetAttributeString("value", null);
			Type type = Type.GetType(element.GetAttributeString("type", string.Empty));
			if (type != null)
			{
				ValueNode newNode = new ValueNode(type, element.GetAttributeString("name", string.Empty))
				{
					ID = element.GetAttributeInt("i", -1)
				};
				float posX = element.GetAttributeFloat("xpos", 0f);
				float posY = element.GetAttributeFloat("ypos", 0f);
				newNode.Position = new Vector2(posX, posY);
				if (value != null)
				{
					if (type.IsEnum)
					{
						Array enums = Enum.GetValues(type);
						using (IEnumerator enumerator = enums.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								object @enum = enumerator.Current;
								if (string.Equals((@enum != null) ? @enum.ToString() : null, value, StringComparison.InvariantCultureIgnoreCase))
								{
									newNode.Value = @enum;
								}
							}
							return newNode;
						}
					}
					newNode.Value = EventEditorScreen.ChangeType(value, type);
				}
				return newNode;
			}
			return null;
		}

		// Token: 0x170009E5 RID: 2533
		// (get) Token: 0x060024C5 RID: 9413 RVA: 0x00173AFC File Offset: 0x00171CFC
		protected override Color BackgroundColor
		{
			get
			{
				return new Color(50, 50, 50);
			}
		}

		// Token: 0x170009E6 RID: 2534
		// (get) Token: 0x060024C6 RID: 9414 RVA: 0x00173B09 File Offset: 0x00171D09
		// (set) Token: 0x060024C7 RID: 9415 RVA: 0x00173B14 File Offset: 0x00171D14
		private string WrappedText
		{
			get
			{
				return this.wrappedText;
			}
			set
			{
				string valueText = value ?? "null";
				int width = base.Rectangle.Width;
				if (width == 0)
				{
					this.wrappedText = valueText;
					return;
				}
				if (width > 16)
				{
					width -= 16;
				}
				if (GUIStyle.SubHeadingFont.Value != null)
				{
					valueText = ToolBox.WrapText(valueText, (float)width, GUIStyle.SubHeadingFont.Value, 1f);
				}
				this.wrappedText = valueText;
			}
		}

		// Token: 0x060024C8 RID: 9416 RVA: 0x00173B78 File Offset: 0x00171D78
		public override Rectangle GetDrawRectangle()
		{
			Rectangle drawRectangle = base.Rectangle;
			Vector2 size = GUIStyle.SubHeadingFont.MeasureString(this.WrappedText ?? "", false);
			drawRectangle.Height = (int)Math.Max(size.Y + 16f, (float)drawRectangle.Height);
			return drawRectangle;
		}

		// Token: 0x060024C9 RID: 9417 RVA: 0x00173BD0 File Offset: 0x00171DD0
		[NullableContext(1)]
		protected override void DrawFront(SpriteBatch spriteBatch)
		{
			base.DrawFront(spriteBatch);
			Vector2 pos = this.GetDrawRectangle().Location.ToVector2() + this.GetDrawRectangle().Size.ToVector2() / 2f - this.valueTextSize / 2f;
			base.Rectangle.Inflate(-1, -1);
			Vector2 pos2 = pos;
			string text = this.WrappedText;
			Color propertyColor = EventEditorNodeConnection.GetPropertyColor(this.Type);
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUI.DrawString(spriteBatch, pos2, text, propertyColor, null, 0, subHeadingFont, ForceUpperCase.Inherit);
		}

		// Token: 0x0400124A RID: 4682
		private object nodeValue;

		// Token: 0x0400124B RID: 4683
		private Vector2 valueTextSize = Vector2.Zero;

		// Token: 0x0400124D RID: 4685
		private string wrappedText;
	}
}
