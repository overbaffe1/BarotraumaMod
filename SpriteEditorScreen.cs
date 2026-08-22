using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200011E RID: 286
	internal class SpriteEditorScreen : EditorScreen
	{
		// Token: 0x17000A3C RID: 2620
		// (get) Token: 0x060026E2 RID: 9954 RVA: 0x0019EA41 File Offset: 0x0019CC41
		private Texture2D SelectedTexture
		{
			get
			{
				Sprite sprite = this.lastSprite;
				if (sprite == null)
				{
					return null;
				}
				return sprite.Texture;
			}
		}

		// Token: 0x17000A3D RID: 2621
		// (get) Token: 0x060026E3 RID: 9955 RVA: 0x0019EA54 File Offset: 0x0019CC54
		private bool ControlDown
		{
			get
			{
				return PlayerInput.KeyDown(Keys.LeftControl) || PlayerInput.KeyDown(Keys.RightControl);
			}
		}

		// Token: 0x17000A3E RID: 2622
		// (get) Token: 0x060026E4 RID: 9956 RVA: 0x0019EA6E File Offset: 0x0019CC6E
		public override Camera Cam
		{
			get
			{
				return this.cam;
			}
		}

		// Token: 0x17000A3F RID: 2623
		// (get) Token: 0x060026E5 RID: 9957 RVA: 0x0019EA76 File Offset: 0x0019CC76
		public GUIComponent TopPanel
		{
			get
			{
				return this.topPanel;
			}
		}

		// Token: 0x060026E6 RID: 9958 RVA: 0x0019EA80 File Offset: 0x0019CC80
		public SpriteEditorScreen()
		{
			this.cam = new Camera();
			GameMain.Instance.ResolutionChanged += this.CreateUI;
			this.CreateUI();
		}

		// Token: 0x060026E7 RID: 9959 RVA: 0x0019EB1C File Offset: 0x0019CD1C
		private void CreateUI()
		{
			this.originLabel = TextManager.Get("charactereditor.origin");
			this.positionLabel = TextManager.GetWithVariable("charactereditor.position", "[coordinates]", string.Empty, FormatCapitals.No);
			this.sizeLabel = TextManager.Get("charactereditor.size");
			this.topPanel = new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 60)
			}, "GUIFrameTop", null);
			this.topPanelContents = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.8f), this.topPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Vector2(0.14f, 0.4f), this.topPanelContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, TextManager.Get("spriteeditor.reloadtexture"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object userData)
			{
				List<Sprite> selected = this.selectedSprites.ToList<Sprite>();
				Sprite firstSelected = selected.First<Sprite>();
				selected.ForEach(delegate(Sprite s)
				{
					s.ReloadTexture();
				});
				this.RefreshLists();
				this.textureList.Select(firstSelected.FullPath, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
				selected.ForEachMod(delegate(Sprite s)
				{
					this.spriteList.Select(s, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
				});
				this.texturePathText.Text = TextManager.GetWithVariable("spriteeditor.texturesreloaded", "[filepath]", firstSelected.FilePath.Value, FormatCapitals.No);
				this.texturePathText.TextColor = GUIStyle.Green;
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.14f, 0.4f), this.topPanelContents.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, TextManager.Get("spriteeditor.resetchanges"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object userData)
			{
				if (this.SelectedTexture == null)
				{
					return false;
				}
				foreach (Sprite sprite in this.loadedSprites)
				{
					if (!(sprite.FullPath != this.selectedTexturePath))
					{
						ContentXElement element2 = sprite.SourceElement;
						ContentXElement contentXElement = null;
						if (!(element2 == contentXElement))
						{
							Sprite sprite2 = sprite;
							ContentXElement contentXElement2 = element2;
							string key = "sourcerect";
							Rectangle sourceRect = sprite.SourceRect;
							sprite2.SourceRect = contentXElement2.GetAttributeRect(key, sourceRect);
							Sprite sprite3 = sprite;
							ContentXElement contentXElement3 = element2;
							string key2 = "origin";
							Vector2 vector = new Vector2(0.5f, 0.5f);
							sprite3.RelativeOrigin = contentXElement3.GetAttributeVector2(key2, vector);
						}
					}
				}
				this.ResetWidgets();
				this.xmlPathText.Text = TextManager.Get("spriteeditor.resetsuccessful");
				this.xmlPathText.TextColor = GUIStyle.Green;
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.14f, 0.4f), this.topPanelContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.15f, 0.1f)
			}, TextManager.Get("spriteeditor.saveselectedsprites"), Alignment.Center, "", null).OnClicked = ((GUIButton button, object userData) => this.SaveSprites(this.selectedSprites));
			new GUIButton(new RectTransform(new Vector2(0.14f, 0.4f), this.topPanelContents.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.15f, 0.1f)
			}, TextManager.Get("spriteeditor.saveallsprites"), Alignment.Center, "", null).OnClicked = ((GUIButton button, object userData) => this.SaveSprites(this.loadedSprites));
			GUITextBlock.AutoScaleAndNormalize(from c in this.topPanelContents.Children
			where c is GUIButton
			select ((GUIButton)c).TextBlock, true, false, null);
			new GUITextBlock(new RectTransform(new Vector2(0.2f, 0.2f), this.topPanelContents.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.3f)
			}, TextManager.Get("spriteeditor.zoom"), null, null, Alignment.Left, false, "", null);
			this.zoomBar = new GUIScrollBar(new RectTransform(new Vector2(0.2f, 0.35f), this.topPanelContents.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.05f, 0.3f)
			}, 0.1f, null, "GUISlider", null)
			{
				BarScroll = this.GetBarScrollValue(),
				Step = 0.01f,
				OnMoved = delegate(GUIScrollBar scrollBar, float value)
				{
					this.zoom = MathHelper.Lerp(0.25f, 10f, value);
					this.viewAreaOffset = Point.Zero;
					return true;
				}
			};
			GUIButton resetBtn = new GUIButton(new RectTransform(new Vector2(0.05f, 0.35f), this.topPanelContents.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.055f, 0.3f)
			}, TextManager.Get("spriteeditor.resetzoom"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton box, object data)
				{
					this.ResetZoom();
					return true;
				}
			};
			resetBtn.TextBlock.AutoScaleHorizontal = true;
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.2f, 0.2f), this.topPanelContents.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.3f)
			}, TextManager.Get("spriteeditor.showgrid"), null, "");
			guitickBox.Selected = this.drawGrid;
			guitickBox.OnSelected = delegate(GUITickBox tickBox)
			{
				this.drawGrid = tickBox.Selected;
				return true;
			};
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(new Vector2(0.2f, 0.2f), this.topPanelContents.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.17f, 0.3f)
			}, TextManager.Get("spriteeditor.snaptogrid"), null, "");
			guitickBox2.Selected = this.snapToGrid;
			guitickBox2.OnSelected = delegate(GUITickBox tickBox)
			{
				this.snapToGrid = tickBox.Selected;
				return true;
			};
			this.texturePathText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.4f), this.topPanelContents.RectTransform, Anchor.Center, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.4f, 0f)
			}, "", new Color?(Color.LightGray), null, Alignment.Left, false, "", null);
			this.xmlPathText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.4f), this.topPanelContents.RectTransform, Anchor.Center, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.4f, 0f)
			}, "", new Color?(Color.LightGray), null, Alignment.Left, false, "", null);
			this.leftPanel = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f - this.topPanel.RectTransform.RelativeSize.Y), this.Frame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, "GUIFrameLeft", null);
			GUILayoutGroup paddedLeftPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), this.leftPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			GUILayoutGroup filterArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.03f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, true, Anchor.TopLeft)
			{
				Stretch = true,
				UserData = "filterarea"
			};
			RectTransform rectT = new RectTransform(Vector2.One, filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("serverlog.filter");
			GUIFont font = GUIStyle.Font;
			this.filterTexturesLabel = new GUITextBlock(rectT, text6, null, font, Alignment.CenterLeft, false, "", null)
			{
				IgnoreLayoutGroups = true
			};
			RectTransform rectT2 = new RectTransform(new Vector2(0.8f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.Font;
			this.filterTexturesBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, true, true);
			filterArea.RectTransform.MinSize = this.filterTexturesBox.RectTransform.MinSize;
			this.filterTexturesBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.FilterTextures(text);
				return true;
			};
			this.textureList = new GUIListBox(new RectTransform(new Vector2(1f, 1f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent listBox, object userData)
				{
					string newTexturePath = userData as string;
					if (this.selectedTexturePath == null || this.selectedTexturePath != newTexturePath)
					{
						this.selectedTexturePath = newTexturePath;
						this.ResetZoom();
						this.spriteList.Select(this.loadedSprites.First((Sprite s) => s.FilePath == this.selectedTexturePath), GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
						this.UpdateScrollBar(this.spriteList);
					}
					foreach (GUIComponent child in this.spriteList.Content.Children)
					{
						GUITextBlock textBlock = (GUITextBlock)child;
						Sprite sprite = (Sprite)textBlock.UserData;
						textBlock.TextColor = new Color(textBlock.TextColor, (sprite.FilePath == this.selectedTexturePath) ? 1f : 0.4f);
						if (sprite.FilePath == this.selectedTexturePath)
						{
							textBlock.Visible = true;
						}
					}
					this.texturePathText.TextColor = Color.LightGray;
					this.topPanelContents.Visible = true;
					return true;
				}
			};
			this.rightPanel = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f - this.topPanel.RectTransform.RelativeSize.Y), this.Frame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, "GUIFrameRight", null);
			GUILayoutGroup paddedRightPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), this.rightPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			filterArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.03f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, true, Anchor.TopLeft)
			{
				Stretch = true,
				UserData = "filterarea"
			};
			RectTransform rectT3 = new RectTransform(Vector2.One, filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("serverlog.filter");
			font = GUIStyle.Font;
			this.filterSpritesLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.CenterLeft, false, "", null)
			{
				IgnoreLayoutGroups = true
			};
			RectTransform rectT4 = new RectTransform(new Vector2(0.8f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text4 = "";
			font = GUIStyle.Font;
			this.filterSpritesBox = new GUITextBox(rectT4, text4, null, font, Alignment.Left, false, "", null, true, true);
			filterArea.RectTransform.MinSize = this.filterSpritesBox.RectTransform.MinSize;
			this.filterSpritesBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.FilterSprites(text);
				return true;
			};
			this.spriteList = new GUIListBox(new RectTransform(new Vector2(1f, 1f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent listBox, object userData)
				{
					Sprite sprite = userData as Sprite;
					if (sprite != null)
					{
						this.SelectSprite(sprite);
						return true;
					}
					return false;
				}
			};
			this.bottomPanel = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.05f), this.Frame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, new Color?(Color.Black * 0.5f));
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(new Vector2(0.2f, 0.5f), this.bottomPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("charactereditor.editbackgroundcolor"), null, "");
			guitickBox3.Selected = this.editBackgroundColor;
			guitickBox3.OnSelected = delegate(GUITickBox box)
			{
				this.editBackgroundColor = box.Selected;
				return true;
			};
			this.backgroundColorPanel = new GUIFrame(new RectTransform(new Point(400, 80), this.Frame.RectTransform, Anchor.BottomCenter, null, ScaleBasis.Normal, false)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, null, new Color?(Color.Black * 0.4f));
			new GUITextBlock(new RectTransform(new Vector2(0.2f, 1f), this.backgroundColorPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(80, 26)
			}, TextManager.Get("spriteeditor.backgroundcolor"), new Color?(Color.WhiteSmoke), null, Alignment.Left, false, "", null);
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 1f), this.backgroundColorPanel.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(20, 0)
			}, true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUIComponent[] fields = new GUIComponent[4];
			LocalizedString[] colorComponentLabels = new LocalizedString[]
			{
				TextManager.Get("spriteeditor.colorcomponentr"),
				TextManager.Get("spriteeditor.colorcomponentg"),
				TextManager.Get("spriteeditor.colorcomponentb")
			};
			for (int i = 2; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.2f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(40, 0),
					MaxSize = new Point(100, 50)
				}, null, new Color?(Color.Black * 0.6f));
				RectTransform rectT5 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = colorComponentLabels[i];
				font = GUIStyle.SmallFont;
				GUITextBlock colorLabel = new GUITextBlock(rectT5, text5, null, font, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueInt = new int?(0);
				numberInput.MaxValueInt = new int?(255);
				numberInput.Font = GUIStyle.SmallFont;
				switch (i)
				{
				case 0:
				{
					colorLabel.TextColor = GUIStyle.Red;
					numberInput.IntValue = (int)this.backgroundColor.R;
					GUINumberInput guinumberInput = numberInput;
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
					{
						this.backgroundColor.R = (byte)numInput.IntValue;
					}));
					break;
				}
				case 1:
				{
					colorLabel.TextColor = GUIStyle.Green;
					numberInput.IntValue = (int)this.backgroundColor.G;
					GUINumberInput guinumberInput2 = numberInput;
					guinumberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput2.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
					{
						this.backgroundColor.G = (byte)numInput.IntValue;
					}));
					break;
				}
				case 2:
				{
					colorLabel.TextColor = Color.DeepSkyBlue;
					numberInput.IntValue = (int)this.backgroundColor.B;
					GUINumberInput guinumberInput3 = numberInput;
					guinumberInput3.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput3.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
					{
						this.backgroundColor.B = (byte)numInput.IntValue;
					}));
					break;
				}
				}
			}
		}

		// Token: 0x060026E8 RID: 9960 RVA: 0x0019FCA4 File Offset: 0x0019DEA4
		private void LoadSprites()
		{
			this.loadedSprites.ForEach(delegate(Sprite s)
			{
				s.Remove();
			});
			this.loadedSprites.Clear();
			List<ContentPackage> contentPackages = ContentPackageManager.EnabledPackages.All.ToList<ContentPackage>();
			ContentPackage vanilla = GameMain.VanillaContent;
			if (vanilla != null)
			{
				contentPackages.Remove(vanilla);
			}
			foreach (ContentPackage contentPackage in contentPackages)
			{
				foreach (ContentFile file in contentPackage.Files)
				{
					if (file.Path.EndsWith(".xml"))
					{
						XDocument doc = XMLExtensions.TryLoadXml(file.Path);
						if (doc != null)
						{
							this.<LoadSprites>g__LoadSprites|42_1(doc.Root.FromPackage(file.Path.ContentPackage));
						}
					}
				}
			}
		}

		// Token: 0x060026E9 RID: 9961 RVA: 0x0019FDA4 File Offset: 0x0019DFA4
		private bool SaveSprites(IEnumerable<Sprite> sprites)
		{
			if (this.SelectedTexture == null)
			{
				return false;
			}
			if (sprites.None(null))
			{
				return false;
			}
			HashSet<XDocument> docsToSave = new HashSet<XDocument>();
			foreach (Sprite sprite in sprites)
			{
				if (!(sprite.FullPath != this.selectedTexturePath))
				{
					ContentXElement element = sprite.SourceElement;
					ContentXElement contentXElement = null;
					if (!(element == contentXElement))
					{
						element.SetAttributeValue("sourcerect", XMLExtensions.RectToString(sprite.SourceRect));
						element.SetAttributeValue("origin", XMLExtensions.Vector2ToString(sprite.RelativeOrigin));
						docsToSave.Add(element.Document);
					}
				}
			}
			this.xmlPathText.Text = TextManager.Get("spriteeditor.allchangessavedto");
			foreach (XDocument doc in docsToSave)
			{
				string xmlPath = doc.ParseContentPathFromUri();
				GUITextBlock guitextBlock = this.xmlPathText;
				RichString text = guitextBlock.Text;
				guitextBlock.Text = ((text != null) ? text.ToString() : null) + "\n" + xmlPath;
				doc.SaveSafe(xmlPath, SaveOptions.None, false, 0);
			}
			this.xmlPathText.TextColor = GUIStyle.Green;
			return true;
		}

		// Token: 0x060026EA RID: 9962 RVA: 0x0019FF0C File Offset: 0x0019E10C
		public override void AddToGUIUpdateList()
		{
			this.leftPanel.AddToGUIUpdateList(false, 0);
			this.rightPanel.AddToGUIUpdateList(false, 0);
			this.topPanel.AddToGUIUpdateList(false, 0);
			this.bottomPanel.AddToGUIUpdateList(false, 0);
			if (this.editBackgroundColor)
			{
				this.backgroundColorPanel.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x060026EB RID: 9963 RVA: 0x0019FF64 File Offset: 0x0019E164
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			Widget.EnableMultiSelect = this.ControlDown;
			this.spriteList.SelectMultiple = Widget.EnableMultiSelect;
			if ((Widget.SelectedWidgets.None(null) || Widget.EnableMultiSelect) && this.SelectedTexture != null && GUI.MouseOn == null)
			{
				foreach (Sprite sprite in this.loadedSprites)
				{
					if (!(sprite.FullPath != this.selectedTexturePath) && PlayerInput.PrimaryMouseButtonClicked())
					{
						Rectangle scaledRect = new Rectangle(this.textureRect.Location + sprite.SourceRect.Location.Multiply(this.zoom), sprite.SourceRect.Size.Multiply(this.zoom));
						if (scaledRect.Contains(PlayerInput.MousePosition))
						{
							this.spriteList.Select(sprite, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
							this.UpdateScrollBar(this.spriteList);
							this.UpdateScrollBar(this.textureList);
							GUI.KeyboardDispatcher.Subscriber = null;
						}
					}
				}
			}
			if (GUI.MouseOn == null)
			{
				if (PlayerInput.ScrollWheelSpeed != 0)
				{
					float newZoom = MathHelper.Clamp(this.zoom + (float)PlayerInput.ScrollWheelSpeed * (float)deltaTime * 0.05f * this.zoom, 0.25f, 10f);
					float zoomDeltaPrc = (newZoom - this.zoom) / this.zoom;
					this.zoom = newZoom;
					this.zoomBar.BarScroll = this.GetBarScrollValue();
					Vector2 mouseDelta = (this.GetViewArea.Center - this.viewAreaOffset - PlayerInput.MousePosition.ToPoint()).ToVector2();
					Vector2 newViewAreaOffset = this.viewAreaOffset.ToVector2();
					this.viewAreaOffset = (newViewAreaOffset + (mouseDelta + newViewAreaOffset) * zoomDeltaPrc).ToPoint();
				}
				this.widgets.Values.ForEach(delegate(Widget w)
				{
					w.Update((float)deltaTime);
				});
				if (PlayerInput.MidButtonHeld())
				{
					Vector2 moveSpeed = PlayerInput.MouseSpeed * (float)deltaTime * 100f;
					this.viewAreaOffset += moveSpeed.ToPoint();
				}
			}
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (PlayerInput.KeyDown(Keys.LeftControl) && PlayerInput.KeyHit(Keys.C))
				{
					string text = "";
					if (this.selectedSprites.Count == 1)
					{
						Sprite selectedSprite = this.selectedSprites.First<Sprite>();
						ContentXElement sourceElement = selectedSprite.SourceElement;
						ContentXElement contentXElement = null;
						if (sourceElement != contentXElement)
						{
							string sourceRectText = "sourcerect=\"" + XMLExtensions.RectToString(selectedSprite.SourceRect) + "\"";
							text += sourceRectText;
						}
					}
					else
					{
						foreach (Sprite selectedSprite2 in this.selectedSprites)
						{
							ContentXElement sourceElement = selectedSprite2.SourceElement;
							ContentXElement contentXElement = null;
							if (!(sourceElement == contentXElement))
							{
								XElement xElement = new XElement(selectedSprite2.SourceElement.Element);
								xElement.SetAttributeValue("sourcerect", XMLExtensions.RectToString(selectedSprite2.SourceRect));
								xElement.SetAttributeValue("origin", XMLExtensions.Vector2ToString(selectedSprite2.RelativeOrigin));
								string str = text;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
								defaultInterpolatedStringHandler.AppendFormatted<XElement>(xElement);
								text = str + defaultInterpolatedStringHandler.ToStringAndClear();
								if (this.selectedSprites.Last<Sprite>() != selectedSprite2)
								{
									text += Environment.NewLine;
								}
							}
						}
					}
					Clipboard.SetText(text);
				}
				if (PlayerInput.KeyHit(Keys.Left))
				{
					this.Nudge(Keys.Left);
				}
				if (PlayerInput.KeyHit(Keys.Right))
				{
					this.Nudge(Keys.Right);
				}
				if (PlayerInput.KeyHit(Keys.Down))
				{
					this.Nudge(Keys.Down);
				}
				if (PlayerInput.KeyHit(Keys.Up))
				{
					this.Nudge(Keys.Up);
				}
				if (PlayerInput.KeyDown(Keys.Left))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > (double)this.holdTime)
					{
						this.Nudge(Keys.Left);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Right))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > (double)this.holdTime)
					{
						this.Nudge(Keys.Right);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Down))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > (double)this.holdTime)
					{
						this.Nudge(Keys.Down);
					}
				}
				else if (PlayerInput.KeyDown(Keys.Up))
				{
					this.holdTimer += deltaTime;
					if (this.holdTimer > (double)this.holdTime)
					{
						this.Nudge(Keys.Up);
					}
				}
				else
				{
					this.holdTimer = 0.0;
				}
				float moveSpeed2 = 600f * this.zoom;
				float moveSpeedDeltaTime = (float)((double)moveSpeed2 * deltaTime);
				Vector2 viewOffsetMove = Vector2.Zero;
				if (PlayerInput.KeyDown(Keys.W))
				{
					viewOffsetMove.Y += moveSpeedDeltaTime;
				}
				if (PlayerInput.KeyDown(Keys.S))
				{
					viewOffsetMove.Y -= moveSpeedDeltaTime;
				}
				if (PlayerInput.KeyDown(Keys.A))
				{
					viewOffsetMove.X += moveSpeedDeltaTime;
				}
				if (PlayerInput.KeyDown(Keys.D))
				{
					viewOffsetMove.X -= moveSpeedDeltaTime;
				}
				this.viewAreaOffset += viewOffsetMove.ToPoint();
			}
		}

		// Token: 0x060026EC RID: 9964 RVA: 0x001A053C File Offset: 0x0019E73C
		private void Nudge(Keys key)
		{
			switch (key)
			{
			case Keys.Left:
				using (List<Sprite>.Enumerator enumerator = this.selectedSprites.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Sprite sprite = enumerator.Current;
						Rectangle newRect = sprite.SourceRect;
						if (this.ControlDown)
						{
							newRect.Width--;
						}
						else
						{
							newRect.X--;
						}
						this.UpdateSourceRect(sprite, newRect);
					}
					return;
				}
				break;
			case Keys.Up:
				goto IL_14B;
			case Keys.Right:
				break;
			case Keys.Down:
				goto IL_E5;
			default:
				return;
			}
			using (List<Sprite>.Enumerator enumerator2 = this.selectedSprites.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Sprite sprite2 = enumerator2.Current;
					Rectangle newRect2 = sprite2.SourceRect;
					if (this.ControlDown)
					{
						newRect2.Width++;
					}
					else
					{
						newRect2.X++;
					}
					this.UpdateSourceRect(sprite2, newRect2);
				}
				return;
			}
			IL_E5:
			using (List<Sprite>.Enumerator enumerator3 = this.selectedSprites.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					Sprite sprite3 = enumerator3.Current;
					Rectangle newRect3 = sprite3.SourceRect;
					if (this.ControlDown)
					{
						newRect3.Height++;
					}
					else
					{
						newRect3.Y++;
					}
					this.UpdateSourceRect(sprite3, newRect3);
				}
				return;
			}
			IL_14B:
			foreach (Sprite sprite4 in this.selectedSprites)
			{
				Rectangle newRect4 = sprite4.SourceRect;
				if (this.ControlDown)
				{
					newRect4.Height--;
				}
				else
				{
					newRect4.Y--;
				}
				this.UpdateSourceRect(sprite4, newRect4);
			}
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x001A0730 File Offset: 0x0019E930
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			graphics.Clear(this.backgroundColor);
			SpriteSortMode sortMode = SpriteSortMode.Deferred;
			BlendState blendState = null;
			RasterizerState scissorTestEnable = GameMain.ScissorTestEnable;
			spriteBatch.Begin(sortMode, blendState, SamplerState.PointClamp, null, scissorTestEnable, null, null);
			Rectangle viewArea = this.GetViewArea;
			if (this.SelectedTexture != null)
			{
				this.textureRect = new Rectangle((int)((float)viewArea.Center.X - (float)this.SelectedTexture.Bounds.Width / 2f * this.zoom), (int)((float)viewArea.Center.Y - (float)this.SelectedTexture.Bounds.Height / 2f * this.zoom), (int)((float)this.SelectedTexture.Bounds.Width * this.zoom), (int)((float)this.SelectedTexture.Bounds.Height * this.zoom));
				spriteBatch.Draw(this.SelectedTexture, viewArea.Center.ToVector2(), null, Color.White, 0f, new Vector2((float)this.SelectedTexture.Bounds.Width / 2f, (float)this.SelectedTexture.Bounds.Height / 2f), this.zoom, SpriteEffects.None, 0f);
				GUI.DrawRectangle(spriteBatch, this.textureRect, Color.Gray, false, 0f, 1f);
				if (this.drawGrid)
				{
					this.DrawGrid(spriteBatch, this.textureRect, this.zoom, Submarine.GridSize);
				}
				foreach (GUIComponent element in this.spriteList.Content.Children)
				{
					SpriteEditorScreen.<>c__DisplayClass49_0 CS$<>8__locals1 = new SpriteEditorScreen.<>c__DisplayClass49_0();
					CS$<>8__locals1.<>4__this = this;
					object userData = element.UserData;
					CS$<>8__locals1.sprite = (userData as Sprite);
					if (CS$<>8__locals1.sprite != null && !(CS$<>8__locals1.sprite.FullPath != this.selectedTexturePath))
					{
						Rectangle sourceRect = new Rectangle(this.textureRect.X + (int)((float)CS$<>8__locals1.sprite.SourceRect.X * this.zoom), this.textureRect.Y + (int)((float)CS$<>8__locals1.sprite.SourceRect.Y * this.zoom), (int)((float)CS$<>8__locals1.sprite.SourceRect.Width * this.zoom), (int)((float)CS$<>8__locals1.sprite.SourceRect.Height * this.zoom));
						bool isSelected = this.selectedSprites.Contains(CS$<>8__locals1.sprite);
						GUI.DrawRectangle(spriteBatch, sourceRect, isSelected ? GUIStyle.Orange : (GUIStyle.Red * 0.5f), false, 0f, (float)(isSelected ? 2 : 1));
						Identifier id = CS$<>8__locals1.sprite.Identifier;
						if (!id.IsEmpty)
						{
							SpriteEditorScreen.<>c__DisplayClass49_1 CS$<>8__locals2 = new SpriteEditorScreen.<>c__DisplayClass49_1();
							CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
							int widgetSize = 10;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(id);
							defaultInterpolatedStringHandler.AppendLiteral("_origin");
							Widget originWidget = this.GetWidget(defaultInterpolatedStringHandler.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.sprite, widgetSize, WidgetShape.Cross, delegate(Widget w)
							{
								w.Tooltip = TextManager.AddPunctuation(':', new LocalizedString[]
								{
									CS$<>8__locals2.CS$<>8__locals1.<>4__this.originLabel,
									CS$<>8__locals2.CS$<>8__locals1.sprite.RelativeOrigin.FormatDoubleDecimal()
								});
								w.MouseHeld += delegate(float dTime)
								{
									w.DrawPos = PlayerInput.MousePosition.Clamp(CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + CS$<>8__locals2.CS$<>8__locals1.<Draw>g__GetTopLeft|0() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom, CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + CS$<>8__locals2.CS$<>8__locals1.<Draw>g__GetBottomRight|2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom);
									CS$<>8__locals2.CS$<>8__locals1.sprite.Origin = (w.DrawPos - CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() - CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location.ToVector2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom;
									w.Tooltip = TextManager.AddPunctuation(':', new LocalizedString[]
									{
										CS$<>8__locals2.CS$<>8__locals1.<>4__this.originLabel,
										CS$<>8__locals2.CS$<>8__locals1.sprite.RelativeOrigin.FormatDoubleDecimal()
									});
								};
								w.Refresh = delegate()
								{
									w.DrawPos = (CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + (CS$<>8__locals2.CS$<>8__locals1.sprite.Origin + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location.ToVector2()) * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom).Clamp(CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + CS$<>8__locals2.CS$<>8__locals1.<Draw>g__GetTopLeft|0() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom, CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + CS$<>8__locals2.CS$<>8__locals1.<Draw>g__GetBottomRight|2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom);
								};
							});
							SpriteEditorScreen.<>c__DisplayClass49_1 CS$<>8__locals3 = CS$<>8__locals2;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 1);
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(id);
							defaultInterpolatedStringHandler2.AppendLiteral("_position");
							CS$<>8__locals3.positionWidget = this.GetWidget(defaultInterpolatedStringHandler2.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.sprite, widgetSize, WidgetShape.Rectangle, delegate(Widget w)
							{
								w.Tooltip = CS$<>8__locals2.CS$<>8__locals1.<>4__this.positionLabel + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location;
								w.MouseHeld += delegate(float dTime)
								{
									w.DrawPos = ((CS$<>8__locals2.CS$<>8__locals1.<>4__this.drawGrid && CS$<>8__locals2.CS$<>8__locals1.<>4__this.snapToGrid) ? CS$<>8__locals2.CS$<>8__locals1.<>4__this.SnapToGrid(PlayerInput.MousePosition, CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect, CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom, Submarine.GridSize, Submarine.GridSize.X / 4f * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom) : PlayerInput.MousePosition);
									w.DrawPos = new Vector2((float)Math.Ceiling((double)w.DrawPos.X), (float)Math.Ceiling((double)w.DrawPos.Y));
									CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect = new Rectangle(((w.DrawPos - CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2()) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom).ToPoint(), CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Size);
									GUITextBlock textBox = CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteList.SelectedComponent as GUITextBlock;
									if (textBox != null)
									{
										textBox.Text = CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetSpriteName(CS$<>8__locals2.CS$<>8__locals1.sprite) + " " + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.ToString();
									}
									w.Tooltip = CS$<>8__locals2.CS$<>8__locals1.<>4__this.positionLabel + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location;
								};
								w.Refresh = delegate()
								{
									w.DrawPos = CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location.ToVector2() * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom;
								};
							});
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(5, 1);
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(id);
							defaultInterpolatedStringHandler3.AppendLiteral("_size");
							Widget sizeWidget = this.GetWidget(defaultInterpolatedStringHandler3.ToStringAndClear(), CS$<>8__locals2.CS$<>8__locals1.sprite, widgetSize, WidgetShape.Rectangle, delegate(Widget w)
							{
								w.Tooltip = TextManager.AddPunctuation(':', new LocalizedString[]
								{
									CS$<>8__locals2.CS$<>8__locals1.<>4__this.sizeLabel,
									CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Size.ToString()
								});
								w.MouseHeld += delegate(float dTime)
								{
									w.DrawPos = ((CS$<>8__locals2.CS$<>8__locals1.<>4__this.drawGrid && CS$<>8__locals2.CS$<>8__locals1.<>4__this.snapToGrid) ? CS$<>8__locals2.CS$<>8__locals1.<>4__this.SnapToGrid(PlayerInput.MousePosition, CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect, CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom, Submarine.GridSize, Submarine.GridSize.X / 4f * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom) : PlayerInput.MousePosition);
									w.DrawPos = new Vector2((float)Math.Ceiling((double)w.DrawPos.X), (float)Math.Ceiling((double)w.DrawPos.Y));
									CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect = new Rectangle(CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Location, ((w.DrawPos - CS$<>8__locals2.positionWidget.DrawPos) / CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom).ToPoint());
									CS$<>8__locals2.CS$<>8__locals1.sprite.RelativeOrigin = CS$<>8__locals2.CS$<>8__locals1.sprite.RelativeOrigin;
									GUITextBlock textBox = CS$<>8__locals2.CS$<>8__locals1.<>4__this.spriteList.SelectedComponent as GUITextBlock;
									if (textBox != null)
									{
										textBox.Text = CS$<>8__locals2.CS$<>8__locals1.<>4__this.GetSpriteName(CS$<>8__locals2.CS$<>8__locals1.sprite) + " " + CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.ToString();
									}
									w.Tooltip = TextManager.AddPunctuation(':', new LocalizedString[]
									{
										CS$<>8__locals2.CS$<>8__locals1.<>4__this.sizeLabel,
										CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Size.ToString()
									});
								};
								w.Refresh = delegate()
								{
									w.DrawPos = CS$<>8__locals2.CS$<>8__locals1.<>4__this.textureRect.Location.ToVector2() + new Vector2((float)CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Right, (float)CS$<>8__locals2.CS$<>8__locals1.sprite.SourceRect.Bottom) * CS$<>8__locals2.CS$<>8__locals1.<>4__this.zoom;
								};
							});
							originWidget.MouseDown += delegate()
							{
								GUI.KeyboardDispatcher.Subscriber = null;
							};
							CS$<>8__locals2.positionWidget.MouseDown += delegate()
							{
								GUI.KeyboardDispatcher.Subscriber = null;
							};
							sizeWidget.MouseDown += delegate()
							{
								GUI.KeyboardDispatcher.Subscriber = null;
							};
							if (isSelected)
							{
								CS$<>8__locals2.positionWidget.Draw(spriteBatch, (float)deltaTime);
								sizeWidget.Draw(spriteBatch, (float)deltaTime);
								originWidget.Draw(spriteBatch, (float)deltaTime);
							}
						}
					}
				}
			}
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x001A0C00 File Offset: 0x0019EE00
		private void DrawGrid(SpriteBatch spriteBatch, Rectangle gridArea, float zoom, Vector2 gridSize)
		{
			gridSize *= zoom;
			if (gridSize.X < 1f)
			{
				return;
			}
			if (gridSize.Y < 1f)
			{
				return;
			}
			int xLines = (int)((float)gridArea.Width / gridSize.X);
			int yLines = (int)((float)gridArea.Height / gridSize.Y);
			for (int x = 0; x <= xLines; x++)
			{
				GUI.DrawLine(spriteBatch, new Vector2((float)gridArea.X + (float)x * gridSize.X, (float)gridArea.Y), new Vector2((float)gridArea.X + (float)x * gridSize.X, (float)gridArea.Bottom), Color.White * 0.25f, 0f, 1f);
			}
			for (int y = 0; y <= yLines; y++)
			{
				GUI.DrawLine(spriteBatch, new Vector2((float)gridArea.X, (float)gridArea.Y + (float)y * gridSize.Y), new Vector2((float)gridArea.Right, (float)gridArea.Y + (float)y * gridSize.Y), Color.White * 0.25f, 0f, 1f);
			}
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x001A0D28 File Offset: 0x0019EF28
		private Vector2 SnapToGrid(Vector2 position, Rectangle gridArea, float zoom, Vector2 gridSize, float tolerance)
		{
			gridSize *= zoom;
			if (gridSize.X < 1f)
			{
				return position;
			}
			if (gridSize.Y < 1f)
			{
				return position;
			}
			Vector2 snappedPos = position;
			snappedPos.X -= (float)gridArea.X;
			snappedPos.Y -= (float)gridArea.Y;
			Vector2 gridPos = new Vector2(MathUtils.RoundTowardsClosest(snappedPos.X, gridSize.X), MathUtils.RoundTowardsClosest(snappedPos.Y, gridSize.Y));
			if (Math.Abs(gridPos.X - snappedPos.X) < tolerance)
			{
				snappedPos.X = gridPos.X;
			}
			if (Math.Abs(gridPos.Y - snappedPos.Y) < tolerance)
			{
				snappedPos.Y = gridPos.Y;
			}
			snappedPos.X += (float)gridArea.X;
			snappedPos.Y += (float)gridArea.Y;
			return snappedPos;
		}

		// Token: 0x060026F0 RID: 9968 RVA: 0x001A0E1C File Offset: 0x0019F01C
		private void FilterTextures(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				this.filterTexturesLabel.Visible = true;
				this.textureList.Content.Children.ForEach(delegate(GUIComponent c)
				{
					c.Visible = true;
				});
				return;
			}
			text = text.ToLower();
			this.filterTexturesLabel.Visible = false;
			foreach (GUIComponent child in this.textureList.Content.Children)
			{
				GUITextBlock textBlock = child as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.Visible = textBlock.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
				}
			}
		}

		// Token: 0x060026F1 RID: 9969 RVA: 0x001A0EE8 File Offset: 0x0019F0E8
		private void FilterSprites(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				this.filterSpritesLabel.Visible = true;
				this.spriteList.Content.Children.ForEach(delegate(GUIComponent c)
				{
					c.Visible = true;
				});
				return;
			}
			text = text.ToLower();
			this.filterSpritesLabel.Visible = false;
			foreach (GUIComponent child in this.spriteList.Content.Children)
			{
				GUITextBlock textBlock = child as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.Visible = textBlock.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
				}
			}
		}

		// Token: 0x060026F2 RID: 9970 RVA: 0x001A0FB4 File Offset: 0x0019F1B4
		public override void Select()
		{
			base.Select();
			this.LoadSprites();
			this.RefreshLists();
			this.spriteList.Select(0, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
		}

		// Token: 0x060026F3 RID: 9971 RVA: 0x001A0FD8 File Offset: 0x0019F1D8
		protected override void DeselectEditorSpecific()
		{
			this.loadedSprites.ForEach(delegate(Sprite s)
			{
				s.Remove();
			});
			this.loadedSprites.Clear();
			this.ResetWidgets();
			List<Sprite> reloadedSprites = new List<Sprite>();
			foreach (Sprite sprite in this.dirtySprites)
			{
				foreach (Sprite s2 in Sprite.LoadedSprites)
				{
					if (s2.FullPath == sprite.FullPath && !reloadedSprites.Contains(s2))
					{
						s2.ReloadXML();
						reloadedSprites.Add(s2);
					}
				}
			}
			this.dirtySprites.Clear();
			this.filterSpritesBox.Text = "";
			this.filterTexturesBox.Text = "";
		}

		// Token: 0x060026F4 RID: 9972 RVA: 0x001A10F4 File Offset: 0x0019F2F4
		public void SelectSprite(Sprite sprite)
		{
			this.lastSprite = sprite;
			if (!this.loadedSprites.Contains(sprite))
			{
				this.loadedSprites.Add(sprite);
				this.RefreshLists();
			}
			if (this.selectedSprites.Any((Sprite s) => s.FullPath != this.selectedTexturePath))
			{
				this.ResetWidgets();
			}
			if (Widget.EnableMultiSelect)
			{
				if (this.selectedSprites.Contains(sprite))
				{
					this.selectedSprites.Remove(sprite);
				}
				else
				{
					this.selectedSprites.Add(sprite);
					this.dirtySprites.Add(sprite);
				}
			}
			else
			{
				this.selectedSprites.Clear();
				this.selectedSprites.Add(sprite);
				this.dirtySprites.Add(sprite);
			}
			if (sprite.FullPath != this.selectedTexturePath)
			{
				this.textureList.Select(sprite.FullPath, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled);
				this.UpdateScrollBar(this.textureList);
			}
			this.xmlPathText.Text = string.Empty;
			foreach (Sprite s2 in this.selectedSprites)
			{
				this.texturePathText.Text = s2.FilePath.Value;
				ContentXElement element = s2.SourceElement;
				ContentXElement contentXElement = null;
				if (element != contentXElement)
				{
					string xmlPath = element.ParseContentPathFromUri();
					if (!this.xmlPathText.Text.Contains(xmlPath, StringComparison.Ordinal))
					{
						GUITextBlock guitextBlock = this.xmlPathText;
						RichString text = guitextBlock.Text;
						guitextBlock.Text = ((text != null) ? text.ToString() : null) + "\n" + xmlPath;
					}
				}
			}
			this.xmlPathText.TextColor = Color.LightGray;
		}

		// Token: 0x060026F5 RID: 9973 RVA: 0x001A12C0 File Offset: 0x0019F4C0
		public void RefreshLists()
		{
			this.selectedSprites.Clear();
			this.textureList.ClearChildren();
			this.spriteList.ClearChildren();
			this.ResetWidgets();
			HashSet<string> textures = new HashSet<string>();
			foreach (Sprite sprite in from s in this.loadedSprites
			orderby Path.GetFileNameWithoutExtension(s.FilePath.Value)
			select s)
			{
				if (!sprite.FilePath.IsNullOrEmpty())
				{
					string normalizedFilePath = sprite.FilePath.FullPath;
					if (!textures.Contains(normalizedFilePath))
					{
						GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.textureList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
						{
							MinSize = new Point(0, 20)
						}, Path.GetFileName(sprite.FilePath.Value), null, null, Alignment.Left, false, "", null);
						guitextBlock.ToolTip = sprite.FilePath.Value;
						guitextBlock.UserData = sprite.FullPath;
						textures.Add(normalizedFilePath);
					}
				}
			}
			foreach (Sprite sprite2 in this.loadedSprites.OrderBy(delegate(Sprite s)
			{
				ContentPath attributeContentPath = s.SourceElement.GetAttributeContentPath("texture");
				return ((attributeContentPath != null) ? attributeContentPath.Value : null) ?? string.Empty;
			}))
			{
				string elementLocalName = sprite2.SourceElement.Element.Name.LocalName;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 6);
				defaultInterpolatedStringHandler.AppendFormatted(this.GetSpriteName(sprite2));
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(sprite2.SourceRect.X);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(sprite2.SourceRect.Y);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(sprite2.SourceRect.Width);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(sprite2.SourceRect.Height);
				defaultInterpolatedStringHandler.AppendLiteral(") [");
				defaultInterpolatedStringHandler.AppendFormatted(elementLocalName);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				string text = defaultInterpolatedStringHandler.ToStringAndClear();
				if (string.Equals(elementLocalName, "sprite", StringComparison.InvariantCultureIgnoreCase))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 5);
					defaultInterpolatedStringHandler2.AppendFormatted(this.GetSpriteName(sprite2));
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(sprite2.SourceRect.X);
					defaultInterpolatedStringHandler2.AppendLiteral(", ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(sprite2.SourceRect.Y);
					defaultInterpolatedStringHandler2.AppendLiteral(", ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(sprite2.SourceRect.Width);
					defaultInterpolatedStringHandler2.AppendLiteral(", ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(sprite2.SourceRect.Height);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					text = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.spriteList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, text, null, null, Alignment.Left, false, "", null).UserData = sprite2;
			}
			this.topPanelContents.Visible = false;
		}

		// Token: 0x060026F6 RID: 9974 RVA: 0x001A16D4 File Offset: 0x0019F8D4
		public void ResetZoom()
		{
			if (this.SelectedTexture == null)
			{
				return;
			}
			Rectangle viewArea = this.GetViewArea;
			float width = (float)viewArea.Width / (float)this.SelectedTexture.Width;
			float height = (float)viewArea.Height / (float)this.SelectedTexture.Height;
			this.zoom = Math.Min(1f, Math.Min(width, height));
			this.zoomBar.BarScroll = this.GetBarScrollValue();
			this.viewAreaOffset = Point.Zero;
		}

		// Token: 0x17000A40 RID: 2624
		// (get) Token: 0x060026F7 RID: 9975 RVA: 0x001A1750 File Offset: 0x0019F950
		private Rectangle GetViewArea
		{
			get
			{
				int margin = 20;
				Rectangle viewArea = new Rectangle(this.leftPanel.Rect.Right + margin + this.viewAreaOffset.X, this.topPanel.Rect.Bottom + margin + this.viewAreaOffset.Y, this.rightPanel.Rect.Left - this.leftPanel.Rect.Right - margin * 2, this.Frame.Rect.Height - this.topPanel.Rect.Height - margin * 2);
				return viewArea;
			}
		}

		// Token: 0x060026F8 RID: 9976 RVA: 0x001A17FA File Offset: 0x0019F9FA
		private float GetBarScrollValue()
		{
			return MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0.25f, 10f, this.zoom));
		}

		// Token: 0x060026F9 RID: 9977 RVA: 0x001A1820 File Offset: 0x0019FA20
		private string GetSpriteName(Sprite sprite)
		{
			ContentXElement sourceElement = sprite.SourceElement;
			ContentXElement contentXElement = null;
			if (sourceElement == contentXElement)
			{
				return string.Empty;
			}
			string name = sprite.Name;
			if (string.IsNullOrWhiteSpace(name))
			{
				ContentXElement parent = sourceElement.Parent;
				name = ((parent != null) ? parent.GetAttributeString("identifier", string.Empty) : null);
			}
			if (string.IsNullOrEmpty(name))
			{
				ContentXElement parent2 = sourceElement.Parent;
				name = ((parent2 != null) ? parent2.GetAttributeString("name", string.Empty) : null);
			}
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
			return Path.GetFileNameWithoutExtension(sprite.FilePath.Value);
		}

		// Token: 0x060026FA RID: 9978 RVA: 0x001A18B4 File Offset: 0x0019FAB4
		private void UpdateScrollBar(GUIListBox listBox)
		{
			GUIScrollBar sb = listBox.ScrollBar;
			sb.BarScroll = MathHelper.Clamp(MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, (float)(listBox.Content.CountChildren - 1), (float)listBox.SelectedIndex)), sb.MinValue, sb.MaxValue);
		}

		// Token: 0x060026FB RID: 9979 RVA: 0x001A190D File Offset: 0x0019FB0D
		private void UpdateSourceRect(Sprite sprite, Rectangle newRect)
		{
			sprite.SourceRect = newRect;
			sprite.RelativeOrigin = sprite.RelativeOrigin;
		}

		// Token: 0x060026FC RID: 9980 RVA: 0x001A1924 File Offset: 0x0019FB24
		private Widget GetWidget(string id, Sprite sprite, int size = 5, WidgetShape shape = WidgetShape.Rectangle, Action<Widget> initMethod = null)
		{
			Widget widget;
			if (!this.widgets.TryGetValue(id, out widget))
			{
				int selectedSize = (int)Math.Round((double)((float)size * 1.5f));
				widget = new Widget(id, size, shape)
				{
					Data = sprite,
					Color = Color.Yellow,
					SecondaryColor = new Color?(Color.Gray),
					TooltipOffset = new Vector2?(new Vector2((float)(selectedSize / 2 + 5), -10f))
				};
				widget.PreDraw += delegate(SpriteBatch sp, float dTime)
				{
					if (!widget.IsControlled)
					{
						widget.Refresh();
					}
				};
				widget.PreUpdate += delegate(float dTime)
				{
					widget.Enabled = this.selectedSprites.Contains(sprite);
				};
				widget.PostUpdate += delegate(float dTime)
				{
					widget.InputAreaMargin = (widget.IsControlled ? 1000 : 0);
					widget.Size = (widget.IsSelected ? selectedSize : size);
					widget.IsFilled = widget.IsControlled;
				};
				this.widgets.Add(id, widget);
				if (initMethod != null)
				{
					initMethod(widget);
				}
			}
			return widget;
		}

		// Token: 0x060026FD RID: 9981 RVA: 0x001A1A8A File Offset: 0x0019FC8A
		private void ResetWidgets()
		{
			this.widgets.Clear();
			Widget.SelectedWidgets.Clear();
		}

		// Token: 0x06002710 RID: 10000 RVA: 0x001A1E88 File Offset: 0x001A0088
		[CompilerGenerated]
		private void <LoadSprites>g__LoadSprites|42_1(ContentXElement element)
		{
			string[] spriteElementNames = new string[]
			{
				"Sprite",
				"DeformableSprite",
				"BackgroundSprite",
				"BrokenSprite",
				"ContainedSprite",
				"InventoryIcon",
				"Icon",
				"VineSprite",
				"LeafSprite",
				"FlowerSprite",
				"DecorativeSprite",
				"BarrelSprite",
				"RailSprite",
				"ChargeSprite",
				"SchematicSprite",
				"WeldedSprite"
			};
			foreach (string spriteElementName in spriteElementNames)
			{
				element.GetChildElements(spriteElementName).ForEach(delegate(ContentXElement s)
				{
					this.<LoadSprites>g__CreateSprite|42_2(s);
				});
			}
			element.Elements().ForEach(delegate(ContentXElement e)
			{
				this.<LoadSprites>g__LoadSprites|42_1(e);
			});
		}

		// Token: 0x06002713 RID: 10003 RVA: 0x001A1F7C File Offset: 0x001A017C
		[CompilerGenerated]
		private void <LoadSprites>g__CreateSprite|42_2(ContentXElement element)
		{
			if (element.Attributes().None(null))
			{
				return;
			}
			string spriteFolder = "";
			ContentPath texturePath = null;
			if (element.GetAttribute("texture") != null)
			{
				texturePath = element.GetAttributeContentPath("texture");
			}
			else if (element.Name.ToString().ToLower() == "vinesprite")
			{
				texturePath = element.Parent.GetAttributeContentPath("vineatlas");
			}
			if (texturePath.IsNullOrEmpty())
			{
				return;
			}
			if (texturePath.Value.Contains("[GENDER]") || texturePath.Value.Contains("[HEADID]") || texturePath.Value.Contains("[RACE]") || texturePath.Value.Contains("[VARIANT]"))
			{
				return;
			}
			if (!texturePath.Value.Contains("/"))
			{
				string parsedPath = element.ParseContentPathFromUri();
				spriteFolder = Path.GetDirectoryName(parsedPath);
			}
			this.loadedSprites.Add(new Sprite(element, spriteFolder, texturePath.Value, true, 1f));
		}

		// Token: 0x040013AB RID: 5035
		private GUIListBox textureList;

		// Token: 0x040013AC RID: 5036
		private GUIListBox spriteList;

		// Token: 0x040013AD RID: 5037
		private GUIFrame topPanel;

		// Token: 0x040013AE RID: 5038
		private GUIFrame leftPanel;

		// Token: 0x040013AF RID: 5039
		private GUIFrame rightPanel;

		// Token: 0x040013B0 RID: 5040
		private GUIFrame bottomPanel;

		// Token: 0x040013B1 RID: 5041
		private GUIFrame backgroundColorPanel;

		// Token: 0x040013B2 RID: 5042
		private bool drawGrid;

		// Token: 0x040013B3 RID: 5043
		private bool snapToGrid;

		// Token: 0x040013B4 RID: 5044
		private GUIFrame topPanelContents;

		// Token: 0x040013B5 RID: 5045
		private GUITextBlock texturePathText;

		// Token: 0x040013B6 RID: 5046
		private GUITextBlock xmlPathText;

		// Token: 0x040013B7 RID: 5047
		private GUIScrollBar zoomBar;

		// Token: 0x040013B8 RID: 5048
		private readonly List<Sprite> selectedSprites = new List<Sprite>();

		// Token: 0x040013B9 RID: 5049
		private readonly List<Sprite> dirtySprites = new List<Sprite>();

		// Token: 0x040013BA RID: 5050
		private Sprite lastSprite;

		// Token: 0x040013BB RID: 5051
		private string selectedTexturePath;

		// Token: 0x040013BC RID: 5052
		private Rectangle textureRect;

		// Token: 0x040013BD RID: 5053
		private float zoom = 1f;

		// Token: 0x040013BE RID: 5054
		private const float MinZoom = 0.25f;

		// Token: 0x040013BF RID: 5055
		private const float MaxZoom = 10f;

		// Token: 0x040013C0 RID: 5056
		private GUITextBox filterSpritesBox;

		// Token: 0x040013C1 RID: 5057
		private GUITextBlock filterSpritesLabel;

		// Token: 0x040013C2 RID: 5058
		private GUITextBox filterTexturesBox;

		// Token: 0x040013C3 RID: 5059
		private GUITextBlock filterTexturesLabel;

		// Token: 0x040013C4 RID: 5060
		private LocalizedString originLabel;

		// Token: 0x040013C5 RID: 5061
		private LocalizedString positionLabel;

		// Token: 0x040013C6 RID: 5062
		private LocalizedString sizeLabel;

		// Token: 0x040013C7 RID: 5063
		private bool editBackgroundColor;

		// Token: 0x040013C8 RID: 5064
		private Color backgroundColor = new Color(0.051f, 0.149f, 0.271f, 1f);

		// Token: 0x040013C9 RID: 5065
		private readonly Camera cam;

		// Token: 0x040013CA RID: 5066
		private readonly HashSet<Sprite> loadedSprites = new HashSet<Sprite>();

		// Token: 0x040013CB RID: 5067
		private double holdTimer;

		// Token: 0x040013CC RID: 5068
		private readonly float holdTime = 0.2f;

		// Token: 0x040013CD RID: 5069
		private Point viewAreaOffset;

		// Token: 0x040013CE RID: 5070
		private Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();
	}
}
