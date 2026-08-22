using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000118 RID: 280
	internal class ParticleEditorScreen : EditorScreen
	{
		// Token: 0x17000A31 RID: 2609
		// (get) Token: 0x06002676 RID: 9846 RVA: 0x0019809F File Offset: 0x0019629F
		public override Camera Cam
		{
			get
			{
				return this.cam;
			}
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x001980A8 File Offset: 0x001962A8
		public ParticleEditorScreen()
		{
			this.cam = new Camera();
			GameMain.Instance.ResolutionChanged += this.CreateUI;
			this.CreateUI();
			if (File.Exists("Content/size_reference.png"))
			{
				this.sizeReference = TextureLoader.FromFile("Content/size_reference.png", false, false, null);
				this.sizeRefOrigin = new Vector2((float)this.sizeReference.Width / 2f, (float)this.sizeReference.Height / 2f);
			}
		}

		// Token: 0x06002678 RID: 9848 RVA: 0x00198180 File Offset: 0x00196380
		private void CreateUI()
		{
			this.Frame.ClearChildren();
			this.leftPanel = new GUIFrame(new RectTransform(new Vector2(0.125f, 1f), this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, "GUIFrameLeft", null);
			GUILayoutGroup paddedLeftPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), this.leftPanel.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f)
			}, false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			this.rightPanel = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f), this.Frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(350, 0)
			}, "GUIFrameRight", null);
			GUILayoutGroup paddedRightPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), this.rightPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f)
			}, false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.03f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.saveall"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				this.SerializeAll();
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.03f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ParticleEditor.CopyPrefabToClipboard"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				this.SerializeToClipboard(this.selectedPrefab);
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.03f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ParticleEditor.CopyEmitterToClipboard"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				this.SerializeEmitterToClipboard();
				return true;
			};
			GUIListBox emitterListBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.25f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			new SerializableEntityEditor(emitterListBox.Content.RectTransform, this.emitterProperties, false, true, "", 20, GUIStyle.SubHeadingFont, true);
			GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.6f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			GUILayoutGroup filterArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.03f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 20)
			}, true, Anchor.TopLeft)
			{
				Stretch = true,
				UserData = "filterarea"
			};
			RectTransform rectT = new RectTransform(Vector2.One, filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("serverlog.filter");
			GUIFont font = GUIStyle.Font;
			this.filterLabel = new GUITextBlock(rectT, text3, null, font, Alignment.Left, false, "", null)
			{
				IgnoreLayoutGroups = true
			};
			RectTransform rectT2 = new RectTransform(new Vector2(0.8f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.Font;
			this.filterBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, false, true);
			this.filterBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.FilterEmitters(text);
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.05f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUICancelButton", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				this.FilterEmitters("");
				this.filterBox.Text = "";
				this.filterBox.Flash(new Color?(Color.White), 1.5f, false, false, null);
				return true;
			};
			this.prefabList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			GUIListBox guilistBox = this.prefabList;
			guilistBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				this.cam.Position = Vector2.Zero;
				this.selectedPrefab = (obj as ParticlePrefab);
				this.emitterPrefab = new ParticleEmitterPrefab(this.selectedPrefab, this.emitterProperties);
				this.emitter = new ParticleEmitter(this.emitterPrefab);
				listBox.ClearChildren();
				new SerializableEntityEditor(listBox.Content.RectTransform, this.selectedPrefab, false, true, "", 20, GUIStyle.SubHeadingFont, true);
				return true;
			}));
			if (GameMain.ParticleManager != null)
			{
				this.RefreshPrefabList();
			}
		}

		// Token: 0x06002679 RID: 9849 RVA: 0x001987BE File Offset: 0x001969BE
		public override void Select()
		{
			base.Select();
			GameMain.ParticleManager.Camera = this.cam;
			this.RefreshPrefabList();
		}

		// Token: 0x0600267A RID: 9850 RVA: 0x001987DC File Offset: 0x001969DC
		protected override void DeselectEditorSpecific()
		{
			GameMain.ParticleManager.Camera = GameMain.GameScreen.Cam;
			this.filterBox.Text = "";
		}

		// Token: 0x0600267B RID: 9851 RVA: 0x00198804 File Offset: 0x00196A04
		private void RefreshPrefabList()
		{
			this.prefabList.ClearChildren();
			List<ParticlePrefab> particlePrefabs = ParticleManager.GetPrefabList();
			foreach (ParticlePrefab particlePrefab in particlePrefabs)
			{
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.prefabList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, particlePrefab.Name, null, null, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.UserData = particlePrefab;
			}
		}

		// Token: 0x0600267C RID: 9852 RVA: 0x001988F0 File Offset: 0x00196AF0
		private void FilterEmitters(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				this.filterLabel.Visible = true;
				this.prefabList.Content.Children.ForEach(delegate(GUIComponent c)
				{
					c.Visible = true;
				});
				return;
			}
			text = text.ToLower();
			this.filterLabel.Visible = false;
			foreach (GUIComponent child in this.prefabList.Content.Children)
			{
				GUITextBlock textBlock = child as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.Visible = textBlock.Text.ToLower().Contains(text, StringComparison.Ordinal);
				}
			}
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x001989C0 File Offset: 0x00196BC0
		private void SerializeAll()
		{
			Validation.SkipValidationInDebugBuilds = true;
			foreach (ParticlesFile configFile in ContentPackageManager.AllPackages.SelectMany((ContentPackage p) => p.GetFiles<ParticlesFile>()))
			{
				XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
				if (doc != null)
				{
					List<ParticlePrefab> prefabList = ParticleManager.GetPrefabList();
					foreach (ParticlePrefab prefab in prefabList)
					{
						foreach (XElement element in doc.Root.Elements())
						{
							if (element.Name.ToString().Equals(prefab.Name, StringComparison.OrdinalIgnoreCase))
							{
								SerializableProperty.SerializeProperties(prefab, element, true, false);
							}
						}
					}
					XmlWriterSettings settings = new XmlWriterSettings
					{
						Indent = true,
						OmitXmlDeclaration = true,
						NewLineOnAttributes = true
					};
					using (XmlWriter writer = XmlWriter.Create(configFile.Path.Value, settings))
					{
						doc.WriteTo(writer);
						writer.Flush();
					}
				}
			}
			Validation.SkipValidationInDebugBuilds = false;
		}

		// Token: 0x0600267E RID: 9854 RVA: 0x00198B7C File Offset: 0x00196D7C
		private void SerializeEmitterToClipboard()
		{
			XElement element = new XElement("ParticleEmitter");
			ParticlePrefab prefab = this.selectedPrefab;
			if (prefab != null)
			{
				element.Add(new XAttribute("particle", prefab.Identifier));
			}
			SerializableProperty.SerializeProperties(this.emitterProperties, element, false, true);
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true
			};
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				element.WriteTo(writer);
				writer.Flush();
			}
			Clipboard.SetText(sb.ToString());
		}

		// Token: 0x0600267F RID: 9855 RVA: 0x00198C28 File Offset: 0x00196E28
		private void SerializeToClipboard(ParticlePrefab prefab)
		{
			if (prefab == null)
			{
				return;
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineOnAttributes = true
			};
			XElement originalElement = null;
			foreach (ParticlesFile configFile in ContentPackageManager.AllPackages.SelectMany((ContentPackage p) => p.GetFiles<ParticlesFile>()))
			{
				XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
				if (doc != null)
				{
					List<ParticlePrefab> prefabList = ParticleManager.GetPrefabList();
					foreach (ParticlePrefab otherPrefab in prefabList)
					{
						foreach (XElement subElement in doc.Root.Elements())
						{
							if (subElement.Name.ToString().Equals(prefab.Name, StringComparison.OrdinalIgnoreCase))
							{
								SerializableProperty.SerializeProperties(prefab, subElement, true, false);
								originalElement = subElement;
								break;
							}
						}
					}
				}
			}
			if (originalElement == null)
			{
				originalElement = new XElement(prefab.Name);
				SerializableProperty.SerializeProperties(prefab, originalElement, true, false);
			}
			StringBuilder sb = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				originalElement.WriteTo(writer);
				writer.Flush();
			}
			Clipboard.SetText(sb.ToString());
		}

		// Token: 0x06002680 RID: 9856 RVA: 0x00198DD8 File Offset: 0x00196FD8
		public override void Update(double deltaTime)
		{
			this.cam.MoveCamera((float)deltaTime, true, GUI.MouseOn == null, true, null);
			if (GUI.MouseOn == null && PlayerInput.PrimaryMouseButtonHeld())
			{
				this.sizeRefPosition = this.cam.ScreenToWorld(PlayerInput.MousePosition);
			}
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				this.CreateContextMenu();
			}
			if (this.selectedPrefab != null && this.emitter != null)
			{
				this.emitter.Emit((float)deltaTime, Vector2.Zero, null, 0f, 0f, 1f, 1f, 1f, null, null, false, null);
			}
			GameMain.ParticleManager.Update((float)deltaTime);
		}

		// Token: 0x06002681 RID: 9857 RVA: 0x00198E8C File Offset: 0x0019708C
		private void CreateContextMenu()
		{
			GUIContextMenu.CreateContextMenu(new ContextMenuOption[]
			{
				new ContextMenuOption("subeditor.editbackgroundcolor", true, new Action(base.CreateBackgroundColorPicker)),
				new ContextMenuOption("editor.togglereferencecharacter", true, delegate()
				{
					this.sizeRefEnabled = !this.sizeRefEnabled;
				})
			});
		}

		// Token: 0x06002682 RID: 9858 RVA: 0x00198EE4 File Offset: 0x001970E4
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			this.cam.UpdateTransform(true, true);
			GameMain.ParticleManager.UpdateTransforms();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
			graphics.Clear(EditorScreen.BackgroundColor);
			GameMain.ParticleManager.Draw(spriteBatch, false, new bool?(false), ParticleBlendState.AlphaBlend, new bool?(false));
			GameMain.ParticleManager.Draw(spriteBatch, true, new bool?(false), ParticleBlendState.AlphaBlend, new bool?(false));
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(spriteBatch, false, new bool?(false), ParticleBlendState.Additive, new bool?(false));
			GameMain.ParticleManager.Draw(spriteBatch, true, new bool?(false), ParticleBlendState.Additive, new bool?(false));
			spriteBatch.End();
			if (this.sizeRefEnabled && this.sizeReference != null)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
				Vector2 pos = this.sizeRefPosition;
				pos.Y = -pos.Y;
				spriteBatch.Draw(this.sizeReference, pos, null, Color.White, 0f, this.sizeRefOrigin, new Vector2(0.4f), SpriteEffects.None, 0f);
				spriteBatch.End();
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x0400135A RID: 4954
		private GUIComponent rightPanel;

		// Token: 0x0400135B RID: 4955
		private GUIComponent leftPanel;

		// Token: 0x0400135C RID: 4956
		private GUIListBox prefabList;

		// Token: 0x0400135D RID: 4957
		private GUITextBox filterBox;

		// Token: 0x0400135E RID: 4958
		private GUITextBlock filterLabel;

		// Token: 0x0400135F RID: 4959
		private ParticlePrefab selectedPrefab;

		// Token: 0x04001360 RID: 4960
		private readonly ParticleEmitterProperties emitterProperties = new ParticleEmitterProperties(null)
		{
			ScaleMax = 1f,
			ScaleMin = 1f,
			AngleMax = 360f,
			AngleMin = 0f,
			ParticlesPerSecond = 1f
		};

		// Token: 0x04001361 RID: 4961
		private ParticleEmitterPrefab emitterPrefab;

		// Token: 0x04001362 RID: 4962
		private ParticleEmitter emitter;

		// Token: 0x04001363 RID: 4963
		private readonly Camera cam;

		// Token: 0x04001364 RID: 4964
		private const string sizeRefFilePath = "Content/size_reference.png";

		// Token: 0x04001365 RID: 4965
		private readonly Texture2D sizeReference;

		// Token: 0x04001366 RID: 4966
		private Vector2 sizeRefPosition = Vector2.Zero;

		// Token: 0x04001367 RID: 4967
		private readonly Vector2 sizeRefOrigin;

		// Token: 0x04001368 RID: 4968
		private bool sizeRefEnabled;
	}
}
