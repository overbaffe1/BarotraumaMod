using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200008D RID: 141
	public class GUIMessageBox : GUIFrame
	{
		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x06001374 RID: 4980 RVA: 0x000BAFBB File Offset: 0x000B91BB
		private static int DefaultWidth
		{
			get
			{
				return Math.Max(400, (int)(400f * ((float)GameMain.GraphicsWidth / GUI.ReferenceResolution.X)));
			}
		}

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x06001375 RID: 4981 RVA: 0x000BAFDF File Offset: 0x000B91DF
		private bool IsAnimated
		{
			get
			{
				return this.type == GUIMessageBox.Type.InGame || this.type == GUIMessageBox.Type.Hint || this.type == GUIMessageBox.Type.Tutorial;
			}
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x06001376 RID: 4982 RVA: 0x000BAFFE File Offset: 0x000B91FE
		// (set) Token: 0x06001377 RID: 4983 RVA: 0x000BB006 File Offset: 0x000B9206
		public List<GUIButton> Buttons { get; private set; } = new List<GUIButton>();

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x06001378 RID: 4984 RVA: 0x000BB00F File Offset: 0x000B920F
		// (set) Token: 0x06001379 RID: 4985 RVA: 0x000BB017 File Offset: 0x000B9217
		public GUILayoutGroup Content { get; private set; }

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x0600137A RID: 4986 RVA: 0x000BB020 File Offset: 0x000B9220
		// (set) Token: 0x0600137B RID: 4987 RVA: 0x000BB028 File Offset: 0x000B9228
		public GUIFrame InnerFrame { get; private set; }

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x0600137C RID: 4988 RVA: 0x000BB031 File Offset: 0x000B9231
		// (set) Token: 0x0600137D RID: 4989 RVA: 0x000BB039 File Offset: 0x000B9239
		public GUITextBlock Header { get; private set; }

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x0600137E RID: 4990 RVA: 0x000BB042 File Offset: 0x000B9242
		// (set) Token: 0x0600137F RID: 4991 RVA: 0x000BB04A File Offset: 0x000B924A
		public GUITextBlock Text { get; private set; }

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x06001380 RID: 4992 RVA: 0x000BB053 File Offset: 0x000B9253
		// (set) Token: 0x06001381 RID: 4993 RVA: 0x000BB05B File Offset: 0x000B925B
		public Identifier Tag { get; private set; }

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x06001382 RID: 4994 RVA: 0x000BB064 File Offset: 0x000B9264
		// (set) Token: 0x06001383 RID: 4995 RVA: 0x000BB06C File Offset: 0x000B926C
		public bool Closed { get; private set; }

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x06001384 RID: 4996 RVA: 0x000BB075 File Offset: 0x000B9275
		// (set) Token: 0x06001385 RID: 4997 RVA: 0x000BB07D File Offset: 0x000B927D
		public GUIImage Icon { get; private set; }

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x06001386 RID: 4998 RVA: 0x000BB086 File Offset: 0x000B9286
		// (set) Token: 0x06001387 RID: 4999 RVA: 0x000BB0A1 File Offset: 0x000B92A1
		public Color IconColor
		{
			get
			{
				if (this.Icon != null)
				{
					return this.Icon.Color;
				}
				return Color.White;
			}
			set
			{
				if (this.Icon == null)
				{
					return;
				}
				this.Icon.Color = value;
			}
		}

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x06001388 RID: 5000 RVA: 0x000BB0B8 File Offset: 0x000B92B8
		// (set) Token: 0x06001389 RID: 5001 RVA: 0x000BB0C0 File Offset: 0x000B92C0
		public bool Draggable { get; set; }

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x0600138A RID: 5002 RVA: 0x000BB0C9 File Offset: 0x000B92C9
		// (set) Token: 0x0600138B RID: 5003 RVA: 0x000BB0D1 File Offset: 0x000B92D1
		public GUIImage BackgroundIcon { get; private set; }

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x0600138C RID: 5004 RVA: 0x000BB0DA File Offset: 0x000B92DA
		// (set) Token: 0x0600138D RID: 5005 RVA: 0x000BB0E2 File Offset: 0x000B92E2
		public bool FlashOnAutoCloseCondition { get; set; }

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x0600138E RID: 5006 RVA: 0x000BB0EB File Offset: 0x000B92EB
		// (set) Token: 0x0600138F RID: 5007 RVA: 0x000BB0F3 File Offset: 0x000B92F3
		public Action OnEnterPressed { get; set; }

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x06001390 RID: 5008 RVA: 0x000BB0FC File Offset: 0x000B92FC
		public GUIMessageBox.Type MessageBoxType
		{
			get
			{
				return this.type;
			}
		}

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x06001391 RID: 5009 RVA: 0x000BB104 File Offset: 0x000B9304
		public static GUIComponent VisibleBox
		{
			get
			{
				return GUIMessageBox.MessageBoxes.LastOrDefault<GUIComponent>();
			}
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x000BB110 File Offset: 0x000B9310
		public GUIMessageBox(LocalizedString headerText, LocalizedString text, Vector2? relativeSize = null, Point? minSize = null, GUIMessageBox.Type type = GUIMessageBox.Type.Default) : this(headerText, text, new LocalizedString[]
		{
			"OK"
		}, relativeSize, minSize, Alignment.TopLeft, type, "", null, "", null, null, false)
		{
			this.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(this.Close);
			this.OnEnterPressed = delegate()
			{
				this.Buttons[0].OnClicked(this.Buttons[0], this.Buttons[0].UserData);
			};
		}

		// Token: 0x06001393 RID: 5011 RVA: 0x000BB188 File Offset: 0x000B9388
		public GUIMessageBox(RichString headerText, RichString text, LocalizedString[] buttons, Vector2? relativeSize = null, Point? minSize = null, Alignment textAlignment = Alignment.TopLeft, GUIMessageBox.Type type = GUIMessageBox.Type.Default, string tag = "", Sprite icon = null, string iconStyle = "", Sprite backgroundIcon = null, Func<bool> autoCloseCondition = null, bool hideCloseButton = false) : base(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), (GUIStyle.GetComponentStyle("GUIMessageBox." + type.ToString()) != null) ? ("GUIMessageBox." + type.ToString()) : "GUIMessageBox", null)
		{
			float num = (float)GUIMessageBox.DefaultWidth;
			float num2;
			if (type != GUIMessageBox.Type.Default)
			{
				if (type != GUIMessageBox.Type.Hint)
				{
					num2 = 1.5f;
				}
				else
				{
					num2 = 1.25f;
				}
			}
			else
			{
				num2 = 1f;
			}
			int width = (int)(num * num2);
			int height = 0;
			if (relativeSize != null)
			{
				width = (int)((float)GameMain.GraphicsWidth * relativeSize.Value.X);
				height = (int)((float)GameMain.GraphicsHeight * relativeSize.Value.Y);
			}
			if (minSize != null)
			{
				width = Math.Max(width, minSize.Value.X);
				if (height > 0)
				{
					height = Math.Max(height, minSize.Value.Y);
				}
			}
			if (backgroundIcon != null)
			{
				this.BackgroundIcon = new GUIImage(new RectTransform(backgroundIcon.size.ToPoint(), base.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), backgroundIcon, null, GUIImage.ScalingMode.None)
				{
					IgnoreLayoutGroups = true,
					Color = Color.Transparent
				};
			}
			Anchor anchor2;
			switch (type)
			{
			case GUIMessageBox.Type.InGame:
				anchor2 = Anchor.TopCenter;
				break;
			case GUIMessageBox.Type.Vote:
				anchor2 = Anchor.TopRight;
				break;
			case GUIMessageBox.Type.Hint:
				anchor2 = Anchor.TopRight;
				break;
			case GUIMessageBox.Type.Tutorial:
				anchor2 = Anchor.TopCenter;
				break;
			default:
				anchor2 = Anchor.Center;
				break;
			}
			Anchor anchor = anchor2;
			this.InnerFrame = new GUIFrame(new RectTransform(new Point(width, height), base.RectTransform, anchor, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, null, null);
			if (type == GUIMessageBox.Type.Vote)
			{
				int offset = GUI.IntScale(64f);
				this.InnerFrame.RectTransform.ScreenSpaceOffset = new Point(-offset, offset);
				this.CanBeFocused = false;
			}
			GUIStyle.Apply(this.InnerFrame, "", this);
			this.type = type;
			this.Tag = tag.ToIdentifier();
			if (type == GUIMessageBox.Type.Default || type == GUIMessageBox.Type.Vote || type == GUIMessageBox.Type.Warning)
			{
				this.Content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.85f), this.InnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					AbsoluteSpacing = 5
				};
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				this.Header = new GUITextBlock(rectT, headerText, null, subHeadingFont, Alignment.Center, true, "", null);
				GUIStyle.Apply(this.Header, "", this);
				this.Header.RectTransform.MinSize = new Point(0, this.Header.Rect.Height);
				Point point;
				if (!text.IsNullOrWhiteSpace())
				{
					this.Text = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text, null, null, textAlignment, true, "", null);
					GUIStyle.Apply(this.Text, "", this);
					RectTransform rectTransform = this.Text.RectTransform;
					RectTransform rectTransform2 = this.Text.RectTransform;
					RectTransform rectTransform3 = this.Text.RectTransform;
					point = new Point(this.Text.Rect.Width, this.Text.Rect.Height);
					rectTransform3.MaxSize = point;
					rectTransform.NonScaledSize = (rectTransform2.MinSize = point);
					this.Text.RectTransform.IsFixedSize = true;
				}
				GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), this.Content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
				{
					AbsoluteSpacing = 5,
					IgnoreLayoutGroups = true
				};
				int buttonSize = 35;
				GUIComponentStyle buttonStyle = GUIStyle.GetComponentStyle("GUIButton");
				if (buttonStyle != null && buttonStyle.Height != null)
				{
					buttonSize = buttonStyle.Height.Value;
				}
				RectTransform rectTransform4 = buttonContainer.RectTransform;
				RectTransform rectTransform5 = buttonContainer.RectTransform;
				RectTransform rectTransform6 = buttonContainer.RectTransform;
				point = new Point(buttonContainer.Rect.Width, (buttonSize + 5) * buttons.Length);
				rectTransform6.MaxSize = point;
				rectTransform4.NonScaledSize = (rectTransform5.MinSize = point);
				buttonContainer.RectTransform.IsFixedSize = true;
				if (height == 0)
				{
					height += this.Header.Rect.Height + this.Content.AbsoluteSpacing;
					height += ((this.Text == null) ? 0 : this.Text.Rect.Height) + this.Content.AbsoluteSpacing;
					height += buttonContainer.Rect.Height + 20;
					if (minSize != null)
					{
						height = Math.Max(height, minSize.Value.Y);
					}
					this.InnerFrame.RectTransform.NonScaledSize = new Point(this.InnerFrame.Rect.Width, (int)Math.Max((float)height / this.Content.RectTransform.RelativeSize.Y, (float)(height + (int)(50f * GUI.yScale))));
					this.Content.RectTransform.NonScaledSize = new Point(this.Content.Rect.Width, height);
				}
				this.Buttons = new List<GUIButton>(buttons.Length);
				for (int i = 0; i < buttons.Length; i++)
				{
					GUIButton button = new GUIButton(new RectTransform(new Vector2(0.6f, 1f / (float)buttons.Length), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), buttons[i], Alignment.Center, "", null);
					this.Buttons.Add(button);
				}
				GUITextBlock.AutoScaleAndNormalize(from btn in this.Buttons
				select btn.TextBlock, true, false, null);
			}
			else if (type == GUIMessageBox.Type.InGame || type == GUIMessageBox.Type.Tutorial)
			{
				this.InnerFrame.RectTransform.AbsoluteOffset = new Point(0, GameMain.GraphicsHeight);
				this.CanBeFocused = false;
				this.AutoClose = (type == GUIMessageBox.Type.InGame);
				GUIStyle.Apply(this.InnerFrame, "", this);
				GUILayoutGroup horizontalLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.95f), this.InnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				if (icon != null)
				{
					this.Icon = new GUIImage(new RectTransform(new Vector2(0.2f, 0.95f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), icon, true, null);
				}
				else if (iconStyle != string.Empty)
				{
					this.Icon = new GUIImage(new RectTransform(new Vector2(0.2f, 0.95f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), iconStyle, true);
				}
				this.Content = new GUILayoutGroup(new RectTransform(new Vector2((this.Icon != null) ? 0.65f : 0.85f, 1f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				if (!hideCloseButton)
				{
					GUIFrame buttonContainer2 = new GUIFrame(new RectTransform(new Vector2(0.15f, 1f), horizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
					this.Buttons = new List<GUIButton>(1)
					{
						new GUIButton(new RectTransform(new Vector2(0.3f, 0.5f), buttonContainer2.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, "UIToggleButton", null)
						{
							OnClicked = new GUIButton.OnClickedHandler(this.Close),
							UserData = UIHighlightAction.ElementId.MessageBoxCloseButton
						}
					};
					InputType? closeInput = null;
					if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Use].MouseButton == MouseButton.None)
					{
						closeInput = new InputType?(InputType.Use);
					}
					else if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Select].MouseButton == MouseButton.None)
					{
						closeInput = new InputType?(InputType.Select);
					}
					if (closeInput != null)
					{
						GUIComponent guicomponent = this.Buttons[0];
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 2);
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("Close"));
						defaultInterpolatedStringHandler.AppendLiteral(" ([InputType.");
						defaultInterpolatedStringHandler.AppendFormatted<InputType>(closeInput.Value);
						defaultInterpolatedStringHandler.AppendLiteral("])");
						guicomponent.ToolTip = TextManager.ParseInputTypes(defaultInterpolatedStringHandler.ToStringAndClear(), false);
						GUIButton guibutton = this.Buttons[0];
						guibutton.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
						{
							if (!this.closing && this.openState >= 1f && PlayerInput.KeyHit(closeInput.Value))
							{
								GUIButton btn = component as GUIButton;
								if (btn != null)
								{
									btn.OnClicked(btn, btn.UserData);
								}
								if (btn != null)
								{
									btn.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
							}
						}));
					}
				}
				else
				{
					this.Buttons.Clear();
				}
				this.Header = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), headerText, new Color?(GUIStyle.TextColorBright), null, Alignment.Left, true, "", null);
				GUIStyle.Apply(this.Header, "", this);
				this.Header.RectTransform.MinSize = new Point(0, this.Header.Rect.Height);
				if (!text.IsNullOrWhiteSpace())
				{
					this.Text = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text, null, null, textAlignment, true, "", null);
					GUIStyle.Apply(this.Text, "", this);
					this.Content.Recalculate();
					RectTransform rectTransform7 = this.Text.RectTransform;
					RectTransform rectTransform8 = this.Text.RectTransform;
					RectTransform rectTransform9 = this.Text.RectTransform;
					Point point = new Point(this.Text.Rect.Width, Math.Min(this.Text.Rect.Height, GameMain.GraphicsHeight));
					rectTransform9.MaxSize = point;
					rectTransform7.NonScaledSize = (rectTransform8.MinSize = point);
					this.Text.RectTransform.IsFixedSize = true;
					if (headerText.IsNullOrWhiteSpace())
					{
						this.Content.ChildAnchor = Anchor.Center;
					}
				}
				if (height == 0)
				{
					height += this.Header.Rect.Height + this.Content.AbsoluteSpacing;
					height += ((this.Text == null) ? 0 : this.Text.Rect.Height) + this.Content.AbsoluteSpacing;
					if (minSize != null)
					{
						height = Math.Max(height, minSize.Value.Y);
					}
					this.InnerFrame.RectTransform.NonScaledSize = new Point(this.InnerFrame.Rect.Width, (int)Math.Max((float)height / this.Content.RectTransform.RelativeSize.Y, (float)(height + (int)(50f * GUI.yScale))));
					this.Content.RectTransform.NonScaledSize = new Point(this.Content.Rect.Width, height);
				}
				if (!hideCloseButton)
				{
					this.Buttons[0].RectTransform.MaxSize = new Point((int)(0.4f * (float)this.Buttons[0].Rect.Y), this.Buttons[0].Rect.Y);
				}
			}
			else if (type == GUIMessageBox.Type.Hint)
			{
				this.CanBeFocused = false;
				GUIStyle.Apply(this.InnerFrame, "", this);
				GUIMessageBox.<>c__DisplayClass75_1 CS$<>8__locals2;
				CS$<>8__locals2.absoluteSpacing = GUIStyle.ItemFrameMargin.Multiply(0.2f);
				GUILayoutGroup verticalLayoutGroup = new GUILayoutGroup(new RectTransform(this.<.ctor>g__GetVerticalLayoutGroupSize|75_2(ref CS$<>8__locals2), this.InnerFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopCenter)
				{
					AbsoluteSpacing = CS$<>8__locals2.absoluteSpacing.Y,
					Stretch = true
				};
				GUILayoutGroup topHorizontalLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.7f), verticalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				int iconMaxHeight;
				if (icon != null)
				{
					this.Icon = new GUIImage(new RectTransform(new Vector2(0.15f, 0.95f), topHorizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), icon, true, null);
					iconMaxHeight = (int)this.Icon.Sprite.size.Y;
				}
				else
				{
					bool iconStyleDefined = !string.IsNullOrEmpty(iconStyle);
					this.Icon = new GUIImage(new RectTransform(new Vector2(0.15f, 0.95f), topHorizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), iconStyleDefined ? iconStyle : "GUIButtonInfo", true);
					if (!iconStyleDefined)
					{
						this.Icon.Color = Color.Orange;
					}
					Sprite defaultSprite = this.Icon.Style.GetDefaultSprite();
					iconMaxHeight = (int)((defaultSprite != null) ? defaultSprite.size.Y : (GUI.yScale * 40f));
				}
				iconMaxHeight = Math.Min((int)(GUI.yScale * 40f), iconMaxHeight);
				int iconMinHeight = Math.Min((int)(GUI.yScale * 40f), iconMaxHeight);
				this.Icon.RectTransform.MinSize = new Point(this.Icon.Rect.Width, iconMinHeight);
				this.Icon.RectTransform.MaxSize = new Point(this.Icon.Rect.Width, iconMaxHeight);
				this.Content = new GUILayoutGroup(new RectTransform(new Vector2((this.Icon != null) ? 0.85f : 1f, 1f), topHorizontalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					AbsoluteSpacing = CS$<>8__locals2.absoluteSpacing.Y
				};
				GUIFrame bottomContainer = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.3f), verticalLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					CanBeFocused = true
				};
				GUILayoutGroup tickBoxLayoutGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.67f, 1f), bottomContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					CanBeFocused = true,
					Stretch = true
				};
				Vector2 tickBoxRelativeSize = new Vector2(1f, 0.5f);
				GUITickBox dontShowAgainTickBox = new GUITickBox(new RectTransform(tickBoxRelativeSize, tickBoxLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("hintmessagebox.dontshowagain"), null, "")
				{
					ToolTip = TextManager.Get("hintmessagebox.dontshowagaintooltip"),
					UserData = "dontshowagain"
				};
				GUITickBox disableHintsTickBox = new GUITickBox(new RectTransform(tickBoxRelativeSize, tickBoxLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("hintmessagebox.disablehints"), null, "")
				{
					ToolTip = TextManager.Get("hintmessagebox.disablehintstooltip"),
					UserData = "disablehints"
				};
				this.Buttons = new List<GUIButton>(1)
				{
					new GUIButton(new RectTransform(new Vector2(0.33f, 1f), bottomContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("hintmessagebox.dismiss"), Alignment.Center, "GUIButtonSmall", null)
					{
						OnClicked = new GUIButton.OnClickedHandler(this.Close)
					}
				};
				this.Header = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), headerText, null, null, Alignment.Left, true, "", null);
				GUIStyle.Apply(this.Header, "", this);
				this.Header.RectTransform.MinSize = new Point(0, this.Header.Rect.Height);
				if (!text.IsNullOrWhiteSpace())
				{
					this.Text = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text, null, null, textAlignment, true, "", null);
					GUIStyle.Apply(this.Text, "", this);
					this.Content.Recalculate();
					RectTransform rectTransform10 = this.Text.RectTransform;
					RectTransform rectTransform11 = this.Text.RectTransform;
					RectTransform rectTransform12 = this.Text.RectTransform;
					Point point = new Point(this.Text.Rect.Width, this.Text.Rect.Height);
					rectTransform12.MaxSize = point;
					rectTransform10.NonScaledSize = (rectTransform11.MinSize = point);
					this.Text.RectTransform.IsFixedSize = true;
					if (headerText.IsNullOrWhiteSpace())
					{
						this.Header.RectTransform.Parent = null;
						this.Content.ChildAnchor = Anchor.Center;
					}
				}
				if (height == 0)
				{
					height = CS$<>8__locals2.absoluteSpacing.Y;
					int upperContainerHeight = CS$<>8__locals2.absoluteSpacing.Y;
					if (this.Header.Rect.Height > 0)
					{
						upperContainerHeight += this.Header.Rect.Height + this.Content.AbsoluteSpacing;
					}
					if (this.Text != null)
					{
						upperContainerHeight += this.Text.Rect.Height + this.Content.AbsoluteSpacing;
					}
					upperContainerHeight = Math.Max(upperContainerHeight, this.Icon.Rect.Height);
					height += upperContainerHeight;
					height += CS$<>8__locals2.absoluteSpacing.Y;
					int bottomContainerHeight = dontShowAgainTickBox.Rect.Height + disableHintsTickBox.Rect.Height;
					height += bottomContainerHeight;
					height += CS$<>8__locals2.absoluteSpacing.Y;
					if (minSize != null)
					{
						height = Math.Max(height, minSize.Value.Y);
					}
					this.InnerFrame.RectTransform.NonScaledSize = new Point(this.InnerFrame.Rect.Width, height);
					verticalLayoutGroup.RectTransform.NonScaledSize = this.<.ctor>g__GetVerticalLayoutGroupSize|75_2(ref CS$<>8__locals2);
					float upperContainerRelativeHeight = (float)upperContainerHeight / (float)(upperContainerHeight + bottomContainerHeight);
					topHorizontalLayoutGroup.RectTransform.RelativeSize = new Vector2(topHorizontalLayoutGroup.RectTransform.RelativeSize.X, upperContainerRelativeHeight);
					bottomContainer.RectTransform.RelativeSize = new Vector2(bottomContainer.RectTransform.RelativeSize.X, 1f - upperContainerRelativeHeight);
					verticalLayoutGroup.Recalculate();
					topHorizontalLayoutGroup.Recalculate();
					this.Content.Recalculate();
					tickBoxLayoutGroup.Recalculate();
				}
				this.InnerFrame.RectTransform.AbsoluteOffset = new Point(GUI.IntScale(64f), -this.InnerFrame.Rect.Height);
			}
			this.autoCloseCondition = autoCloseCondition;
			GUIMessageBox.MessageBoxes.Add(this);
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x000BC784 File Offset: 0x000BA984
		public GUIMessageBox(Identifier hintIdentifier, LocalizedString text, Sprite icon) : this("", text, Array.Empty<LocalizedString>(), null, null, Alignment.CenterLeft, GUIMessageBox.Type.Hint, "", icon, "", null, null, false)
		{
			GUITickBox dontShowAgainTickBox = this.InnerFrame.FindChild("dontshowagain", true) as GUITickBox;
			if (dontShowAgainTickBox != null)
			{
				GUITickBox guitickBox = dontShowAgainTickBox;
				GUITickBox.OnSelectedHandler onSelected;
				if ((onSelected = GUIMessageBox.<>O.<0>__OnDontShowAgain) == null)
				{
					onSelected = (GUIMessageBox.<>O.<0>__OnDontShowAgain = new GUITickBox.OnSelectedHandler(HintManager.OnDontShowAgain));
				}
				guitickBox.OnSelected = onSelected;
				dontShowAgainTickBox.UserData = hintIdentifier;
			}
			GUITickBox disableHintsTickBox = this.InnerFrame.FindChild("disablehints", true) as GUITickBox;
			if (disableHintsTickBox != null)
			{
				GUITickBox guitickBox2 = disableHintsTickBox;
				GUITickBox.OnSelectedHandler onSelected2;
				if ((onSelected2 = GUIMessageBox.<>O.<1>__OnDisableHints) == null)
				{
					onSelected2 = (GUIMessageBox.<>O.<1>__OnDisableHints = new GUITickBox.OnSelectedHandler(HintManager.OnDisableHints));
				}
				guitickBox2.OnSelected = onSelected2;
				disableHintsTickBox.UserData = hintIdentifier;
			}
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x000BC864 File Offset: 0x000BAA64
		public static void AddActiveToGUIUpdateList()
		{
			if (GUIMessageBox.messageBoxTypes == null)
			{
				GUIMessageBox.messageBoxTypes = (GUIMessageBox.Type[])Enum.GetValues(typeof(GUIMessageBox.Type));
			}
			foreach (GUIMessageBox.Type type in GUIMessageBox.messageBoxTypes)
			{
				if (type != GUIMessageBox.Type.Hint || !GUI.DisableHUD)
				{
					for (int i = 0; i < GUIMessageBox.MessageBoxes.Count; i++)
					{
						if (GUIMessageBox.MessageBoxes[i] != null)
						{
							GUIMessageBox messageBox = GUIMessageBox.MessageBoxes[i] as GUIMessageBox;
							if (messageBox == null)
							{
								if (type == GUIMessageBox.Type.Default)
								{
									GUIMessageBox.MessageBoxes[i].AddToGUIUpdateList(false, 0);
									if (!(GUIMessageBox.MessageBoxes[i].UserData is RoundSummary))
									{
										break;
									}
								}
							}
							else if (messageBox.type == type && (messageBox.DisplayInLoadingScreens || !GameMain.Instance.LoadingScreenOpen) && !messageBox.DrawOnTop)
							{
								messageBox.AddToGUIUpdateList(false, 0);
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06001396 RID: 5014 RVA: 0x000BC95C File Offset: 0x000BAB5C
		public void SetBackgroundIcon(Sprite icon)
		{
			if (icon == null)
			{
				return;
			}
			GUIImage backgroundIcon = this.BackgroundIcon;
			if (icon == ((backgroundIcon != null) ? backgroundIcon.Sprite : null))
			{
				return;
			}
			GUIImage newIcon = new GUIImage(new RectTransform(icon.size.ToPoint(), base.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), icon, null, GUIImage.ScalingMode.None)
			{
				IgnoreLayoutGroups = true,
				Color = Color.Transparent
			};
			if (this.newBackgroundIcon != null)
			{
				this.RemoveChild(this.newBackgroundIcon);
				this.newBackgroundIcon = null;
			}
			this.newBackgroundIcon = newIcon;
		}

		// Token: 0x06001397 RID: 5015 RVA: 0x000BC9EC File Offset: 0x000BABEC
		protected override void Update(float deltaTime)
		{
			if (PlayerInput.KeyHit(Keys.Enter))
			{
				Action onEnterPressed = this.OnEnterPressed;
				if (onEnterPressed != null)
				{
					onEnterPressed();
				}
			}
			if (this.Draggable)
			{
				GUIComponent mouseOn = GUI.MouseOn;
				GUIComponent guicomponent;
				if (mouseOn == null)
				{
					guicomponent = null;
				}
				else
				{
					GUIComponent parent2 = mouseOn.Parent;
					guicomponent = ((parent2 != null) ? parent2.Parent : null);
				}
				GUIComponent parent = guicomponent;
				if ((GUI.MouseOn == this.InnerFrame || this.InnerFrame.IsParentOf(GUI.MouseOn, true)) && !(GUI.MouseOn is GUIButton) && !(GUI.MouseOn is GUIColorPicker) && !(GUI.MouseOn is GUITextBox) && !(parent is GUITextBox))
				{
					GUI.MouseCursor = CursorState.Move;
					if (PlayerInput.PrimaryMouseButtonDown())
					{
						this.DraggingPosition = base.RectTransform.ScreenSpaceOffset.ToVector2() - PlayerInput.MousePosition;
					}
				}
				if (PlayerInput.PrimaryMouseButtonHeld() && this.DraggingPosition != Vector2.Zero)
				{
					GUI.MouseCursor = CursorState.Dragging;
					base.RectTransform.ScreenSpaceOffset = (PlayerInput.MousePosition + this.DraggingPosition).ToPoint();
				}
				else
				{
					this.DraggingPosition = Vector2.Zero;
				}
			}
			if (this.IsAnimated)
			{
				Vector2 initialPos;
				Vector2 defaultPos;
				Vector2 endPos;
				if (this.type == GUIMessageBox.Type.InGame || this.type == GUIMessageBox.Type.Tutorial)
				{
					initialPos = new Vector2(0f, (float)GameMain.GraphicsHeight);
					defaultPos = new Vector2(0f, (float)(HUDLayoutSettings.InventoryAreaLower.Y - this.InnerFrame.Rect.Height) - 20f * GUI.Scale);
					endPos = new Vector2((float)GameMain.GraphicsWidth, defaultPos.Y);
				}
				else
				{
					initialPos = new Vector2((float)GUI.IntScale(64f), (float)(-(float)this.InnerFrame.Rect.Height));
					defaultPos = new Vector2(initialPos.X, (float)(HUDLayoutSettings.ButtonAreaTop.Height + GUI.IntScale(64f)));
					endPos = new Vector2((float)(-(float)this.InnerFrame.Rect.Width), defaultPos.Y);
				}
				if (!this.closing)
				{
					Point step = Vector2.SmoothStep(initialPos, defaultPos, this.openState).ToPoint();
					this.InnerFrame.RectTransform.AbsoluteOffset = step;
					if (this.BackgroundIcon != null)
					{
						this.BackgroundIcon.RectTransform.AbsoluteOffset = new Point(this.InnerFrame.Rect.Location.X - (int)((float)this.BackgroundIcon.Rect.Size.X / 1.25f), (int)defaultPos.Y - this.BackgroundIcon.Rect.Size.Y / 2);
						if (!MathUtils.NearlyEqual(this.openState, 1f, 0.0001f))
						{
							this.BackgroundIcon.Color = ToolBox.GradientLerp(this.openState, new Color[]
							{
								Color.Transparent,
								Color.White
							});
						}
					}
					if (!(Screen.Selected is RoundSummaryScreen))
					{
						if (!GUIMessageBox.MessageBoxes.Any((GUIComponent mb) => mb.UserData is RoundSummary))
						{
							this.openState = Math.Min(this.openState + deltaTime * 2f, 1f);
						}
					}
					if (GUI.MouseOn != this.InnerFrame && !this.InnerFrame.IsParentOf(GUI.MouseOn, true) && this.AutoClose)
					{
						this.inGameCloseTimer += deltaTime;
					}
					if (this.inGameCloseTimer >= 15f)
					{
						this.Close();
					}
					else if (this.autoCloseCondition != null && this.autoCloseCondition())
					{
						this.Close();
						if (this.FlashOnAutoCloseCondition)
						{
							this.InnerFrame.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
					}
				}
				else
				{
					this.openState += deltaTime * 2f;
					Point step2 = Vector2.SmoothStep(defaultPos, endPos, this.openState - 1f).ToPoint();
					this.InnerFrame.RectTransform.AbsoluteOffset = step2;
					if (this.BackgroundIcon != null)
					{
						this.BackgroundIcon.Color *= 0.9f;
					}
					if (this.openState >= 2f)
					{
						GUIComponent parent3 = base.Parent;
						if (parent3 != null)
						{
							parent3.RemoveChild(this);
						}
						if (GUIMessageBox.MessageBoxes.Contains(this))
						{
							GUIMessageBox.MessageBoxes.Remove(this);
						}
					}
				}
				if (this.newBackgroundIcon != null)
				{
					if (!this.iconSwitching)
					{
						if (this.BackgroundIcon != null)
						{
							this.BackgroundIcon.Color *= 0.9f;
							if (this.BackgroundIcon.Color.A == 0)
							{
								this.BackgroundIcon = null;
								this.iconSwitching = true;
								this.RemoveChild(this.BackgroundIcon);
							}
						}
						else
						{
							this.iconSwitching = true;
						}
						this.iconState = 0f;
						return;
					}
					this.newBackgroundIcon.SetAsFirstChild();
					this.newBackgroundIcon.RectTransform.AbsoluteOffset = new Point(this.InnerFrame.Rect.Location.X - (int)((float)this.newBackgroundIcon.Rect.Size.X / 1.25f), (int)defaultPos.Y - this.newBackgroundIcon.Rect.Size.Y / 2);
					this.newBackgroundIcon.Color = Color.Lerp(Color.Transparent, Color.White, this.iconState);
					if (this.newBackgroundIcon.Color.A == 255)
					{
						this.BackgroundIcon = this.newBackgroundIcon;
						this.BackgroundIcon.SetAsFirstChild();
						this.newBackgroundIcon = null;
						this.iconSwitching = false;
					}
					this.iconState = Math.Min(this.iconState + deltaTime * 2f, 1f);
				}
			}
		}

		// Token: 0x06001398 RID: 5016 RVA: 0x000BD000 File Offset: 0x000BB200
		public void Close()
		{
			if (this.IsAnimated)
			{
				this.closing = true;
			}
			else
			{
				GUIComponent parent = base.Parent;
				if (parent != null)
				{
					parent.RemoveChild(this);
				}
				if (GUIMessageBox.MessageBoxes.Contains(this))
				{
					GUIMessageBox.MessageBoxes.Remove(this);
				}
			}
			this.Closed = true;
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x000BD050 File Offset: 0x000BB250
		public bool Close(GUIButton button, object obj)
		{
			base.RectTransform.Parent = null;
			this.Close();
			return true;
		}

		// Token: 0x0600139A RID: 5018 RVA: 0x000BD065 File Offset: 0x000BB265
		public static void CloseAll()
		{
			GUIMessageBox.MessageBoxes.Clear();
		}

		// Token: 0x0600139B RID: 5019 RVA: 0x000BD074 File Offset: 0x000BB274
		public static void Close(Identifier tag)
		{
			foreach (GUIComponent messageBox in GUIMessageBox.MessageBoxes)
			{
				GUIMessageBox mb = messageBox as GUIMessageBox;
				if (mb != null)
				{
					Identifier tag2 = mb.Tag;
					if (tag2 == tag)
					{
						mb.Close();
					}
				}
			}
		}

		// Token: 0x0600139C RID: 5020 RVA: 0x000BD0E4 File Offset: 0x000BB2E4
		public static void Close(string tag)
		{
			GUIMessageBox.Close(tag.ToIdentifier());
		}

		// Token: 0x0600139D RID: 5021 RVA: 0x000BD0F4 File Offset: 0x000BB2F4
		public void AddButton(RectTransform rectT, string text, GUIButton.OnClickedHandler onClick)
		{
			rectT.Parent = base.RectTransform;
			this.Buttons.Add(new GUIButton(rectT, text, Alignment.Center, "", null)
			{
				OnClicked = onClick
			});
		}

		// Token: 0x0600139E RID: 5022 RVA: 0x000BD13C File Offset: 0x000BB33C
		public static GUIMessageBox CreateLoadingBox(LocalizedString text, [TupleElementNames(new string[]
		{
			"Label",
			"Action"
		})] ValueTuple<LocalizedString, Action<GUIMessageBox>>[] buttons = null, Vector2? relativeSize = null)
		{
			GUIMessageBox.<>c__DisplayClass87_0 CS$<>8__locals1 = new GUIMessageBox.<>c__DisplayClass87_0();
			CS$<>8__locals1.buttons = buttons;
			if (CS$<>8__locals1.buttons == null)
			{
				CS$<>8__locals1.buttons = Array.Empty<ValueTuple<LocalizedString, Action<GUIMessageBox>>>();
			}
			Vector2? vector = relativeSize;
			Vector2 relativeSizeFallback = vector ?? new ValueTuple<float, float>(0.7f, 0.5f);
			GUIMessageBox.<>c__DisplayClass87_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = "";
			RichString text2 = "";
			vector = new Vector2?(relativeSizeFallback);
			CS$<>8__locals2.newMessageBox = new GUIMessageBox(headerText, text2, (from b in CS$<>8__locals1.buttons
			select b.Item1).ToArray<LocalizedString>(), vector, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			CS$<>8__locals1.newMessageBox.InnerFrame.RectTransform.ScaleBasis = ScaleBasis.BothHeight;
			for (int i = 0; i < CS$<>8__locals1.buttons.Length; i++)
			{
				int capturedIndex = i;
				CS$<>8__locals1.newMessageBox.Buttons[i].OnClicked = delegate(GUIButton _, object _)
				{
					CS$<>8__locals1.buttons[capturedIndex].Item2(CS$<>8__locals1.newMessageBox);
					return false;
				};
			}
			new GUITextBlock(new RectTransform(new ValueTuple<float, float>(0.9f, 0f), CS$<>8__locals1.newMessageBox.InnerFrame.RectTransform, Anchor.Center, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new ValueTuple<float, float>(0f, -0.125f)
			}, text, null, null, Alignment.Center, true, "", null);
			new GUICustomComponent(new RectTransform(Vector2.One * 0.25f, CS$<>8__locals1.newMessageBox.InnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothHeight), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				GUIStyle.GenericThrobber.Draw(sb, (int)(Timing.TotalTime * 20.0) % GUIStyle.GenericThrobber.FrameCount, component.Rect.Center.ToVector2(), Color.White, GUIStyle.GenericThrobber.FrameSize.ToVector2() * 0.5f, 0f, component.Rect.Size.ToVector2() / GUIStyle.GenericThrobber.FrameSize.ToVector2(), SpriteEffects.None, null);
			}, null);
			GUIMessageBox.MessageBoxes.Remove(CS$<>8__locals1.newMessageBox);
			GUIMessageBox.MessageBoxes.Insert(0, CS$<>8__locals1.newMessageBox);
			return CS$<>8__locals1.newMessageBox;
		}

		// Token: 0x060013A1 RID: 5025 RVA: 0x000BD3CC File Offset: 0x000BB5CC
		[CompilerGenerated]
		private Point <.ctor>g__GetVerticalLayoutGroupSize|75_2(ref GUIMessageBox.<>c__DisplayClass75_1 A_1)
		{
			return this.InnerFrame.Rect.Size - A_1.absoluteSpacing.Multiply(2);
		}

		// Token: 0x040009AF RID: 2479
		public static readonly List<GUIComponent> MessageBoxes = new List<GUIComponent>();

		// Token: 0x040009B0 RID: 2480
		private float inGameCloseTimer;

		// Token: 0x040009B1 RID: 2481
		private const float inGameCloseTime = 15f;

		// Token: 0x040009B9 RID: 2489
		public bool DisplayInLoadingScreens;

		// Token: 0x040009BC RID: 2492
		public Vector2 DraggingPosition = Vector2.Zero;

		// Token: 0x040009BE RID: 2494
		private GUIImage newBackgroundIcon;

		// Token: 0x040009BF RID: 2495
		public bool AutoClose;

		// Token: 0x040009C0 RID: 2496
		private float openState;

		// Token: 0x040009C1 RID: 2497
		private float iconState;

		// Token: 0x040009C2 RID: 2498
		private bool iconSwitching;

		// Token: 0x040009C3 RID: 2499
		private bool closing;

		// Token: 0x040009C4 RID: 2500
		private readonly GUIMessageBox.Type type;

		// Token: 0x040009C5 RID: 2501
		private readonly Func<bool> autoCloseCondition;

		// Token: 0x040009C8 RID: 2504
		public bool DrawOnTop;

		// Token: 0x040009C9 RID: 2505
		private static GUIMessageBox.Type[] messageBoxTypes;

		// Token: 0x02000963 RID: 2403
		public enum Type
		{
			// Token: 0x04004140 RID: 16704
			Default,
			// Token: 0x04004141 RID: 16705
			InGame,
			// Token: 0x04004142 RID: 16706
			Vote,
			// Token: 0x04004143 RID: 16707
			Hint,
			// Token: 0x04004144 RID: 16708
			Tutorial,
			// Token: 0x04004145 RID: 16709
			Warning
		}

		// Token: 0x02000964 RID: 2404
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004146 RID: 16710
			public static GUITickBox.OnSelectedHandler <0>__OnDontShowAgain;

			// Token: 0x04004147 RID: 16711
			public static GUITickBox.OnSelectedHandler <1>__OnDisableHints;
		}
	}
}
