using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000039 RID: 57
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxLabelNode : CircuitBoxNode, ICircuitBoxIdentifiable
	{
		// Token: 0x0600090C RID: 2316 RVA: 0x00051844 File Offset: 0x0004FA44
		public override void DrawHeader(SpriteBatch spriteBatch, RectangleF rect, Color color)
		{
			Vector2 pos = new Vector2(rect.X + 8f, rect.Center.Y - this.headerLabel.Size.Y / 2f);
			LocalizedString value = this.headerLabel.Value;
			Color color2 = GUIStyle.TextColorNormal;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUI.DrawString(spriteBatch, pos, value, color2, null, 0, largeFont, ForceUpperCase.Inherit);
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x000518B4 File Offset: 0x0004FAB4
		public override void DrawBody(SpriteBatch spriteBatch, RectangleF rect, Color color)
		{
			this.bodyLabel.TextOffset = rect.Location - this.bodyLabel.Rect.Location.ToVector2() + new Vector2(8f);
			this.bodyLabel.DrawManually(spriteBatch, false, true);
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x00051910 File Offset: 0x0004FB10
		public override void OnResized(RectangleF rect)
		{
			this.UpdateTextSizes(rect);
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0005191C File Offset: 0x0004FB1C
		private void UpdateTextSizes(RectangleF rect)
		{
			Point size = new Point((int)rect.Width - 16, (int)rect.Height - 16);
			this.bodyLabel.RectTransform.NonScaledSize = size;
			this.bodyLabel.Text = CircuitBoxLabelNode.<UpdateTextSizes>g__GetLocalizedText|6_0(this.BodyText);
			if (this.bodyLabel.Font != null)
			{
				this.bodyLabel.Text = ToolBox.LimitStringHeight(this.bodyLabel.WrappedText.Value, this.bodyLabel.Font, size.Y);
			}
			this.headerLabel = new CircuitBoxLabel(ToolBox.LimitString(CircuitBoxLabelNode.<UpdateTextSizes>g__GetLocalizedText|6_0(this.HeaderText), GUIStyle.LargeFont, size.X), GUIStyle.LargeFont);
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x000519E4 File Offset: 0x0004FBE4
		public void PromptEditText(GUIComponent parent)
		{
			CircuitBoxLabelNode.<>c__DisplayClass7_0 CS$<>8__locals1 = new CircuitBoxLabelNode.<>c__DisplayClass7_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.parent = parent;
			CS$<>8__locals1.newColor = this.Color;
			CircuitBoxUI ui = this.CircuitBox.UI;
			if (ui != null)
			{
				ui.SetMenuVisibility(false);
			}
			GUIFrame backgroundBlocker = new GUIFrame(new RectTransform(Vector2.One, CS$<>8__locals1.parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null)
			{
				UserData = "LabelEditPrompt"
			};
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.3f, 0.8f), backgroundBlocker.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			GUILayoutGroup colorLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), colorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null).IgnoreLayoutGroups = true;
			GUILayoutGroup colorArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), colorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIFrame labelArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.65f), mainLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			CS$<>8__locals1.header = new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), labelArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "CircuitBoxTop", null);
			CS$<>8__locals1.frame = new GUIFrame(new RectTransform(new Vector2(1f, 0.86f), labelArea.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), "CircuitBoxFrame", null);
			CS$<>8__locals1.header.Color = (CS$<>8__locals1.frame.Color = this.Color);
			CircuitBoxLabelNode.<>c__DisplayClass7_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(Vector2.One, CS$<>8__locals1.header.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			string value = this.HeaderText.Value;
			GUIFont font = this.headerLabel.Font;
			CS$<>8__locals2.headerTextBox = new GUITextBox(rectT, value, null, font, Alignment.Left, false, "GUITextBoxNoStyle", null, false, true)
			{
				MaxTextLength = new int?(255),
				Text = this.HeaderText.Value
			};
			CircuitBoxLabelNode.<>c__DisplayClass7_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(ToolBox.PaddingSizeParentRelative(CS$<>8__locals1.frame.RectTransform, 0.95f), CS$<>8__locals1.frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			string value2 = this.BodyText.Value;
			font = GUIStyle.Font;
			CS$<>8__locals3.bodyTextBox = new GUITextBox(rectT2, value2, null, font, Alignment.TopLeft, true, "GUITextBoxNoStyle", null, false, true)
			{
				MaxTextLength = new int?(255)
			};
			GUITextBox bodyTextBox = CS$<>8__locals1.bodyTextBox;
			bodyTextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(bodyTextBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
			{
				int caretIndex = textBox.CaretIndex;
				textBox.Text = text.Substring(0, caretIndex) + "\n" + text.Substring(caretIndex);
				textBox.CaretIndex = caretIndex + 1;
				return true;
			}));
			CircuitBoxLabelNode.<>c__DisplayClass7_0 CS$<>8__locals4 = CS$<>8__locals1;
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals1.frame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0.03f, 0.02f);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.bodyTextBox.Text.Length);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(255);
			RichString text3 = defaultInterpolatedStringHandler.ToStringAndClear();
			font = GUIStyle.SmallFont;
			CS$<>8__locals4.characterLimit = new GUITextBlock(rectTransform, text3, null, font, Alignment.Right, false, "", null);
			CS$<>8__locals1.bodyTextBox.OnTextChanged += delegate(GUITextBox textBox, string _)
			{
				textBox.TextColor = (textBox.TextBlock.SelectedTextColor = ((textBox.Text.Length > 255) ? GUIStyle.Red : GUIStyle.TextColorNormal));
				GUITextBlock characterLimit = CS$<>8__locals1.characterLimit;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<int>(textBox.Text.Length);
				defaultInterpolatedStringHandler2.AppendLiteral("/");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(255);
				characterLimit.Text = defaultInterpolatedStringHandler2.ToStringAndClear();
				return true;
			};
			CS$<>8__locals1.bodyTextBox.OnDeselected += delegate(GUITextBox textBox, Keys _)
			{
				CircuitBoxLabelNode.<PromptEditText>g__UpdateLabelColor|7_1(textBox);
			};
			CS$<>8__locals1.headerTextBox.OnDeselected += delegate(GUITextBox textBox, Keys _)
			{
				CircuitBoxLabelNode.<PromptEditText>g__UpdateLabelColor|7_1(textBox);
			};
			CircuitBoxLabelNode.<PromptEditText>g__UpdateLabelColor|7_1(CS$<>8__locals1.bodyTextBox);
			CircuitBoxLabelNode.<PromptEditText>g__UpdateLabelColor|7_1(CS$<>8__locals1.headerTextBox);
			mainLayout.Recalculate();
			CS$<>8__locals1.headerTextBox.ForceUpdate();
			new GUIButton(new RectTransform(new Vector2(0.5f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("confirm"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				CS$<>8__locals1.<>4__this.CircuitBox.RenameLabel(CS$<>8__locals1.<>4__this, CS$<>8__locals1.newColor, new NetLimitedString(CS$<>8__locals1.headerTextBox.Text), new NetLimitedString(CS$<>8__locals1.bodyTextBox.Text));
				CS$<>8__locals1.<>4__this.RemoveEditPrompt(CS$<>8__locals1.parent);
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				CS$<>8__locals1.<>4__this.RemoveEditPrompt(CS$<>8__locals1.parent);
				return true;
			};
			LocalizedString[] colorComponentLabels = new LocalizedString[]
			{
				TextManager.Get("spriteeditor.colorcomponentr"),
				TextManager.Get("spriteeditor.colorcomponentg"),
				TextManager.Get("spriteeditor.colorcomponentb")
			};
			for (int i = 0; i <= 2; i++)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.33f, 1f), colorArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				RectTransform rectT3 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = colorComponentLabels[i];
				font = GUIStyle.SubHeadingFont;
				GUITextBlock colorLabel = new GUITextBlock(rectT3, text2, null, font, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SubHeadingFont,
					MinValueInt = new int?(0),
					MaxValueInt = new int?(255)
				};
				switch (i)
				{
				case 0:
				{
					colorLabel.TextColor = GUIStyle.Red;
					numberInput.IntValue = (int)this.Color.R;
					GUINumberInput guinumberInput = numberInput;
					Delegate onValueChanged = guinumberInput.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b;
					if ((b = CS$<>8__locals1.<>9__7) == null)
					{
						b = (CS$<>8__locals1.<>9__7 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.newColor.R = (byte)numInput.IntValue;
							CS$<>8__locals1.header.Color = (CS$<>8__locals1.frame.Color = CS$<>8__locals1.newColor);
						});
					}
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged, b);
					break;
				}
				case 1:
				{
					colorLabel.TextColor = GUIStyle.Green;
					numberInput.IntValue = (int)this.Color.G;
					GUINumberInput guinumberInput2 = numberInput;
					Delegate onValueChanged2 = guinumberInput2.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b2;
					if ((b2 = CS$<>8__locals1.<>9__8) == null)
					{
						b2 = (CS$<>8__locals1.<>9__8 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.newColor.G = (byte)numInput.IntValue;
							CS$<>8__locals1.header.Color = (CS$<>8__locals1.frame.Color = CS$<>8__locals1.newColor);
						});
					}
					guinumberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged2, b2);
					break;
				}
				case 2:
				{
					colorLabel.TextColor = GUIStyle.Blue;
					numberInput.IntValue = (int)this.Color.B;
					GUINumberInput guinumberInput3 = numberInput;
					Delegate onValueChanged3 = guinumberInput3.OnValueChanged;
					GUINumberInput.OnValueChangedHandler b3;
					if ((b3 = CS$<>8__locals1.<>9__9) == null)
					{
						b3 = (CS$<>8__locals1.<>9__9 = delegate(GUINumberInput numInput)
						{
							CS$<>8__locals1.newColor.B = (byte)numInput.IntValue;
							CS$<>8__locals1.header.Color = (CS$<>8__locals1.frame.Color = CS$<>8__locals1.newColor);
						});
					}
					guinumberInput3.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged3, b3);
					break;
				}
				}
			}
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x00052300 File Offset: 0x00050500
		public void RemoveEditPrompt(GUIComponent parent)
		{
			GUIComponent promptParent = parent.FindChild("LabelEditPrompt", false);
			if (promptParent == null)
			{
				return;
			}
			parent.RemoveChild(promptParent);
		}

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000912 RID: 2322 RVA: 0x00052325 File Offset: 0x00050525
		public ushort ID { get; }

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000913 RID: 2323 RVA: 0x0005232D File Offset: 0x0005052D
		public override bool IsResizable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000914 RID: 2324 RVA: 0x00052330 File Offset: 0x00050530
		public static NetLimitedString DefaultHeaderText
		{
			get
			{
				return new NetLimitedString("label");
			}
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0005233C File Offset: 0x0005053C
		public CircuitBoxLabelNode(ushort id, Color color, Vector2 pos, CircuitBox circuitBox) : base(circuitBox)
		{
			this.Size = new Vector2(256f);
			base.Position = pos;
			this.ID = id;
			this.Color = color;
			base.UpdatePositions();
			RectTransform rectT = new RectTransform(Point.Zero, null, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text = string.Empty;
			GUIFont font = GUIStyle.Font;
			this.bodyLabel = new GUITextBlock(rectT, text, null, font, Alignment.TopLeft, true, "", null);
			this.headerLabel = new CircuitBoxLabel(this.HeaderText.Value, GUIStyle.LargeFont);
			base.UpdateDrawRects();
			this.UpdateTextSizes(this.DrawRect);
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x00052411 File Offset: 0x00050611
		public void EditText(NetLimitedString header, NetLimitedString body)
		{
			this.HeaderText = header;
			this.BodyText = body;
			this.UpdateTextSizes(this.DrawRect);
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x00052430 File Offset: 0x00050630
		public XElement Save()
		{
			return new XElement("Label", new object[]
			{
				new XAttribute("id", this.ID),
				new XAttribute("color", this.Color.ToStringHex()),
				new XAttribute("position", XMLExtensions.Vector2ToString(base.Position)),
				new XAttribute("size", XMLExtensions.Vector2ToString(this.Size)),
				new XAttribute("header", this.HeaderText),
				new XAttribute("body", this.BodyText)
			});
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x00052504 File Offset: 0x00050704
		public static CircuitBoxLabelNode LoadFromXML(ContentXElement element, CircuitBox circuitBox)
		{
			ushort id = element.GetAttributeUInt16("id", ushort.MaxValue);
			string key = "position";
			Vector2 zero = Vector2.Zero;
			Vector2 position = element.GetAttributeVector2(key, zero);
			string key2 = "size";
			zero = Vector2.Zero;
			Vector2 size = element.GetAttributeVector2(key2, zero);
			string key3 = "color";
			Color white = Color.White;
			Color color = element.GetAttributeColor(key3, white);
			string header = element.GetAttributeString("header", string.Empty);
			string body = element.GetAttributeString("body", string.Empty);
			CircuitBoxLabelNode labelNode = new CircuitBoxLabelNode(id, color, position, circuitBox)
			{
				Size = size,
				HeaderText = new NetLimitedString(header),
				BodyText = new NetLimitedString(body)
			};
			labelNode.EditText(new NetLimitedString(header), new NetLimitedString(body));
			labelNode.UpdatePositions();
			labelNode.UpdateTextSizes(labelNode.Rect);
			return labelNode;
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x000525F3 File Offset: 0x000507F3
		[CompilerGenerated]
		internal static LocalizedString <UpdateTextSizes>g__GetLocalizedText|6_0(NetLimitedString text)
		{
			return TextManager.Get(text.Value).Fallback(text.Value, true);
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x00052614 File Offset: 0x00050814
		[CompilerGenerated]
		internal static void <PromptEditText>g__UpdateLabelColor|7_1(GUITextBox box)
		{
			bool found = TextManager.ContainsTag(box.Text);
			box.TextColor = (found ? GUIStyle.Orange : GUIStyle.TextColorNormal);
			if (found)
			{
				box.ToolTip = TextManager.GetWithVariable("StringPropertyTranslate", "[translation]", TextManager.Get(box.Text), FormatCapitals.No);
				return;
			}
			box.ToolTip = string.Empty;
		}

		// Token: 0x040004B1 RID: 1201
		private CircuitBoxLabel headerLabel;

		// Token: 0x040004B2 RID: 1202
		private readonly GUITextBlock bodyLabel;

		// Token: 0x040004B3 RID: 1203
		private const string PromptUserData = "LabelEditPrompt";

		// Token: 0x040004B4 RID: 1204
		public Color Color;

		// Token: 0x040004B6 RID: 1206
		public NetLimitedString BodyText = NetLimitedString.Empty;

		// Token: 0x040004B7 RID: 1207
		public NetLimitedString HeaderText = CircuitBoxLabelNode.DefaultHeaderText;

		// Token: 0x040004B8 RID: 1208
		public static Vector2 MinSize = new Vector2(128f, 8f);
	}
}
