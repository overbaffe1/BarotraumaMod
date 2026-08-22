using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000EF RID: 239
	internal sealed class SubmarinePreview : IDisposable
	{
		// Token: 0x06002286 RID: 8838 RVA: 0x0015B9C5 File Offset: 0x00159BC5
		public static void Create(SubmarineInfo submarineInfo)
		{
			SubmarinePreview.Close();
			SubmarinePreview.instance = new SubmarinePreview(submarineInfo);
		}

		// Token: 0x06002287 RID: 8839 RVA: 0x0015B9D7 File Offset: 0x00159BD7
		public static void Close()
		{
			SubmarinePreview submarinePreview = SubmarinePreview.instance;
			if (submarinePreview != null)
			{
				submarinePreview.Dispose();
			}
			SubmarinePreview.instance = null;
		}

		// Token: 0x06002288 RID: 8840 RVA: 0x0015B9F0 File Offset: 0x00159BF0
		private SubmarinePreview(SubmarineInfo subInfo)
		{
			SubmarinePreview.<>c__DisplayClass15_0 CS$<>8__locals1 = new SubmarinePreview.<>c__DisplayClass15_0();
			CS$<>8__locals1.<>4__this = this;
			this.camera = new Camera();
			this.submarineInfo = subInfo;
			this.spriteRecorder = new SpriteRecorder();
			this.isDisposed = false;
			this.loadTask = null;
			this.hullCollections = new List<SubmarinePreview.HullCollection>();
			this.doors = new List<SubmarinePreview.Door>();
			this.previewFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.previewFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			new GUIButton(new RectTransform(Vector2.One, this.previewFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Alignment.Center, null, null).OnClicked = delegate(GUIButton btn, object obj)
			{
				this.Dispose();
				return false;
			};
			GUIFrame innerFrame = new GUIFrame(new RectTransform(Vector2.One * 0.9f, this.previewFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			int innerPadding = GUI.IntScale(100f);
			GUIFrame innerPadded = new GUIFrame(new RectTransform(new Point(innerFrame.Rect.Width - innerPadding, innerFrame.Rect.Height - innerPadding), this.previewFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null)
			{
				OutlineColor = Color.Black,
				OutlineThickness = 2f
			};
			CS$<>8__locals1.titleText = null;
			CS$<>8__locals1.specsContainer = null;
			new GUICustomComponent(new RectTransform(Vector2.One, innerPadded.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				if (this.isDisposed)
				{
					return;
				}
				this.camera.UpdateTransform(true, false);
				Rectangle drawRect = new Rectangle(component.Rect.X + 1, component.Rect.Y + 1, component.Rect.Width - 2, component.Rect.Height - 2);
				this.RenderSubmarine(spriteBatch, drawRect, component);
			}, delegate(float deltaTime, GUICustomComponent component)
			{
				if (CS$<>8__locals1.<>4__this.isDisposed)
				{
					return;
				}
				bool isMouseOnComponent = GUI.MouseOn == component;
				CS$<>8__locals1.<>4__this.camera.MoveCamera(deltaTime, true, isMouseOnComponent, true, new bool?(false));
				if (isMouseOnComponent && (PlayerInput.MidButtonHeld() || PlayerInput.PrimaryMouseButtonHeld()))
				{
					Vector2 moveSpeed = PlayerInput.MouseSpeed * deltaTime * 60f / CS$<>8__locals1.<>4__this.camera.Zoom;
					moveSpeed.X = -moveSpeed.X;
					CS$<>8__locals1.<>4__this.camera.Position += moveSpeed;
				}
				if (CS$<>8__locals1.titleText != null && CS$<>8__locals1.specsContainer != null)
				{
					CS$<>8__locals1.specsContainer.Visible = GUI.IsMouseOn(CS$<>8__locals1.titleText);
				}
				if (PlayerInput.KeyHit(Keys.Escape))
				{
					CS$<>8__locals1.<>4__this.Dispose();
				}
			});
			GUIFrame topContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.07f), innerPadded.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				Color = Color.Black * 0.65f
			};
			GUILayoutGroup topLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.97f, 0.71428573f), topContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			SubmarinePreview.<>c__DisplayClass15_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(new Vector2(0.95f, 1f), topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = subInfo.DisplayName;
			GUIFont largeFont = GUIStyle.LargeFont;
			CS$<>8__locals2.titleText = new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			new GUIButton(new RectTransform(new Vector2(0.05f, 1f), topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				this.Dispose();
				return false;
			};
			CS$<>8__locals1.specsContainer = new GUIListBox(new RectTransform(new Vector2(0.4f, 1f), innerPadded.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.015f, 0.07f)
			}, false, null, "", true, false)
			{
				CurrentSelectMode = GUIListBox.SelectMode.None,
				Color = Color.Black * 0.65f,
				ScrollBarEnabled = false,
				ScrollBarVisible = false,
				Spacing = GUI.IntScale(5f)
			};
			subInfo.CreateSpecsWindow(CS$<>8__locals1.specsContainer, GUIStyle.Font, false, true, true, false);
			CS$<>8__locals1.width = CS$<>8__locals1.specsContainer.Rect.Width;
			CS$<>8__locals1.<.ctor>g__recalculateSpecsContainerHeight|2();
			CS$<>8__locals1.specsContainer.Content.GetAllChildren<GUITextBlock>().ForEach(delegate(GUITextBlock c)
			{
				GUITextBlock firstChild = c.Children.FirstOrDefault<GUIComponent>() as GUITextBlock;
				if (firstChild != null)
				{
					firstChild.CalculateHeightFromText(0, false);
					firstChild.SetTextPos();
					c.RectTransform.MinSize = new Point(0, firstChild.Rect.Height);
				}
				c.CalculateHeightFromText(0, false);
				c.SetTextPos();
			});
			CS$<>8__locals1.<.ctor>g__recalculateSpecsContainerHeight|2();
			TaskPool.Add("GeneratePreviewMeshes", this.GeneratePreviewMeshes(), delegate(Task _)
			{
				if (this.isDisposed)
				{
					return;
				}
				this.camera.Position = (this.bounds.Item1 + this.bounds.Item2) * new ValueTuple<float, float>(0.5f, -0.5f);
				Vector2 span2d = this.bounds.Item2 - this.bounds.Item1;
				Vector2 scaledSpan2d = span2d / this.camera.Resolution.ToVector2();
				float scaledSpan = Math.Max(scaledSpan2d.X, scaledSpan2d.Y);
				this.camera.MinZoom = Math.Min(0.1f, 0.4f / scaledSpan);
				this.camera.Zoom = 0.7f / scaledSpan;
				this.camera.StopMovement();
				this.camera.UpdateTransform(false, false);
			});
		}

		// Token: 0x06002289 RID: 8841 RVA: 0x0015BF26 File Offset: 0x0015A126
		public static void AddToGUIUpdateList()
		{
			SubmarinePreview submarinePreview = SubmarinePreview.instance;
			if (submarinePreview == null)
			{
				return;
			}
			GUIFrame guiframe = submarinePreview.previewFrame;
			if (guiframe == null)
			{
				return;
			}
			guiframe.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0600228A RID: 8842 RVA: 0x0015BF43 File Offset: 0x0015A143
		public Task GeneratePreviewMeshes()
		{
			if (this.loadTask != null)
			{
				throw new InvalidOperationException("Tried to start SubmarinePreview loadTask more than once!");
			}
			this.loadTask = Task.Run(new Func<Task>(this.GeneratePreviewMeshesInternal));
			return this.loadTask;
		}

		// Token: 0x0600228B RID: 8843 RVA: 0x0015BF78 File Offset: 0x0015A178
		private Task GeneratePreviewMeshesInternal()
		{
			SubmarinePreview.<GeneratePreviewMeshesInternal>d__18 <GeneratePreviewMeshesInternal>d__;
			<GeneratePreviewMeshesInternal>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<GeneratePreviewMeshesInternal>d__.<>4__this = this;
			<GeneratePreviewMeshesInternal>d__.<>1__state = -1;
			<GeneratePreviewMeshesInternal>d__.<>t__builder.Start<SubmarinePreview.<GeneratePreviewMeshesInternal>d__18>(ref <GeneratePreviewMeshesInternal>d__);
			return <GeneratePreviewMeshesInternal>d__.<>t__builder.Task;
		}

		// Token: 0x0600228C RID: 8844 RVA: 0x0015BFBC File Offset: 0x0015A1BC
		private static void ExtractItemContainerIds(XElement component, HashSet<int> ids)
		{
			string containedString = component.GetAttributeString("contained", "");
			string[] itemIdStrings = containedString.Split(',', StringSplitOptions.None);
			for (int i = 0; i < itemIdStrings.Length; i++)
			{
				foreach (string idStr in itemIdStrings[i].Split(';', StringSplitOptions.None))
				{
					int id;
					if (int.TryParse(idStr, NumberStyles.Any, CultureInfo.InvariantCulture, out id) && id != 0 && !ids.Contains(id))
					{
						ids.Add(id);
					}
				}
			}
		}

		// Token: 0x0600228D RID: 8845 RVA: 0x0015C044 File Offset: 0x0015A244
		private static void ExtractConnectionPanelLinks(XElement component, HashSet<int> ids)
		{
			IEnumerable<XElement> pins = component.Elements("input").Concat(component.Elements("output"));
			foreach (XElement pin in pins)
			{
				IEnumerable<XElement> links = pin.Elements("link");
				foreach (XElement link in links)
				{
					int id = link.GetAttributeInt("w", 0);
					if (id != 0 && !ids.Contains(id))
					{
						ids.Add(id);
					}
				}
			}
		}

		// Token: 0x0600228E RID: 8846 RVA: 0x0015C118 File Offset: 0x0015A318
		private void BakeWireNodes(XElement element)
		{
			Identifier prefabIdentifier = element.GetAttributeIdentifier("identifier", "");
			if (prefabIdentifier.IsEmpty)
			{
				return;
			}
			ItemPrefab prefab;
			if (!ItemPrefab.Prefabs.TryGet(prefabIdentifier, out prefab))
			{
				return;
			}
			ContentXElement prefabWireComponentElement = prefab.ConfigElement.GetChildElement("wire");
			if (prefabWireComponentElement == null)
			{
				return;
			}
			XElement wireComponent = element.GetChildElement("wire", StringComparison.OrdinalIgnoreCase);
			if (wireComponent == null)
			{
				return;
			}
			Color color = element.GetAttributeColor("spritecolor") ?? Color.White;
			ImmutableArray<Vector2> nodes = Wire.ExtractNodes(wireComponent).ToImmutableArray<Vector2>();
			Sprite wireSprite = Wire.ExtractWireSprite(prefab.ConfigElement);
			float depth = element.GetAttributeBool("usespritedepth", false) ? element.GetAttributeFloat("spritedepth", 1f) : wireSprite.Depth;
			float width = prefabWireComponentElement.GetAttributeFloat("width", 0.3f);
			for (int i = 0; i < nodes.Length - 1; i++)
			{
				ValueTuple<Vector2, Vector2> line = new ValueTuple<Vector2, Vector2>(nodes[i], nodes[i + 1]);
				Wire.WireSection wireSegment = new Wire.WireSection(line.Item1, line.Item2);
				wireSegment.Draw(this.spriteRecorder, wireSprite, color, Vector2.Zero, depth, width);
			}
		}

		// Token: 0x0600228F RID: 8847 RVA: 0x0015C258 File Offset: 0x0015A458
		private void BakeMapEntity(XElement element)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (identifier.IsEmpty)
			{
				return;
			}
			Rectangle rect = element.GetAttributeRect("rect", Rectangle.Empty);
			if (rect.Equals(Rectangle.Empty))
			{
				return;
			}
			float depth = element.GetAttributeFloat("spritedepth", 1f);
			bool flippedX = element.GetAttributeBool("flippedx", false);
			bool flippedY = element.GetAttributeBool("flippedy", false);
			float scale = element.GetAttributeFloat("scale", 1f);
			Color color = element.GetAttributeColor("spritecolor", Color.White);
			float rotationRad = MathHelper.ToRadians(element.GetAttributeFloat("rotation", 0f));
			Identifier identifier2 = element.NameAsIdentifier();
			ItemPrefab ip;
			MapEntityPrefab prefab;
			if (identifier2 == "item" && ItemPrefab.Prefabs.TryGet(identifier, out ip))
			{
				prefab = ip;
			}
			else
			{
				prefab = MapEntityPrefab.FindByIdentifier(identifier);
			}
			if (prefab == null)
			{
				return;
			}
			flippedX &= prefab.CanSpriteFlipX;
			flippedY &= prefab.CanSpriteFlipY;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (flippedX)
			{
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			if (flippedY)
			{
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			SpriteEffects prevEffects = prefab.Sprite.effects;
			prefab.Sprite.effects ^= spriteEffects;
			bool overrideSprite = false;
			ItemPrefab itemPrefab = prefab as ItemPrefab;
			if (itemPrefab != null)
			{
				this.BakeItemComponents(itemPrefab, rect, color, scale, rotationRad, depth, out overrideSprite);
			}
			if (!overrideSprite)
			{
				StructurePrefab structurePrefab = prefab as StructurePrefab;
				if (structurePrefab != null)
				{
					this.ParseUpgrades(structurePrefab.ConfigElement, ref scale);
					if (!prefab.ResizeVertical)
					{
						rect.Height = (int)((float)rect.Height * scale / prefab.Scale);
					}
					if (!prefab.ResizeHorizontal)
					{
						rect.Width = (int)((float)rect.Width * scale / prefab.Scale);
					}
					Vector2 textureScale = element.GetAttributeVector2("texturescale", Vector2.One);
					Vector2 backGroundOffset = Vector2.Zero;
					Vector2 textureOffset = element.GetAttributeVector2("textureoffset", Vector2.Zero);
					textureOffset = Structure.UpgradeTextureOffset(rect.Size.ToVector2(), textureOffset, this.submarineInfo, prefab.Sprite.SourceRect, textureScale * scale, flippedX, flippedY);
					backGroundOffset = new Vector2(MathUtils.PositiveModulo(-textureOffset.X, (float)prefab.Sprite.SourceRect.Width * textureScale.X * scale), MathUtils.PositiveModulo(-textureOffset.Y, (float)prefab.Sprite.SourceRect.Height * textureScale.Y * scale));
					Sprite sprite = prefab.Sprite;
					ISpriteBatch spriteBatch = this.spriteRecorder;
					Vector2 position = new Vector2((float)(rect.X + rect.Width / 2), (float)(-(float)(rect.Y - rect.Height / 2)));
					Vector2 targetSize = rect.Size.ToVector2();
					Vector2? origin = new Vector2?(rect.Size.ToVector2() * new Vector2(0.5f, 0.5f));
					Color? color2 = new Color?(color);
					Vector2? vector = new Vector2?(backGroundOffset);
					Vector2? textureScale2 = new Vector2?(textureScale * scale);
					float? depth2 = new float?(depth);
					sprite.DrawTiled(spriteBatch, position, targetSize, rotationRad, origin, color2, vector, textureScale2, depth2);
				}
				else if (itemPrefab != null)
				{
					bool usePrefabValues = element.GetAttributeBool("isoverride", false) != itemPrefab.IsOverride;
					if (usePrefabValues)
					{
						scale = itemPrefab.ConfigElement.GetAttributeFloat(scale, new string[]
						{
							"scale",
							"Scale"
						});
					}
					this.ParseUpgrades(itemPrefab.ConfigElement, ref scale);
					if (prefab.ResizeVertical || prefab.ResizeHorizontal)
					{
						if (!prefab.ResizeHorizontal)
						{
							rect.Width = (int)(prefab.Sprite.size.X * scale);
						}
						if (!prefab.ResizeVertical)
						{
							rect.Height = (int)(prefab.Sprite.size.Y * scale);
						}
						Vector2 spritePos = rect.Center.ToVector2();
						Sprite sprite2 = prefab.Sprite;
						ISpriteBatch spriteBatch2 = this.spriteRecorder;
						Vector2 position2 = rect.Location.ToVector2() * new Vector2(1f, -1f);
						Vector2 targetSize2 = rect.Size.ToVector2();
						float rotation = 0f;
						Color? color2 = new Color?(color);
						Vector2? textureScale2 = new Vector2?(Vector2.One * scale);
						float? depth2 = new float?(depth);
						Vector2? vector = null;
						Vector2? origin2 = vector;
						Color? color3 = color2;
						vector = null;
						sprite2.DrawTiled(spriteBatch2, position2, targetSize2, rotation, origin2, color3, vector, textureScale2, depth2);
						foreach (DecorativeSprite decorativeSprite in itemPrefab.DecorativeSprites)
						{
							float offsetState = 0f;
							Vector2 offset = decorativeSprite.GetOffset(ref offsetState, Vector2.Zero, 0f) * scale;
							if (flippedX)
							{
								offset.X = -offset.X;
							}
							if (flippedY)
							{
								offset.Y = -offset.Y;
							}
							Sprite sprite3 = decorativeSprite.Sprite;
							ISpriteBatch spriteBatch3 = this.spriteRecorder;
							Vector2 position3 = new Vector2(spritePos.X + offset.X - (float)(rect.Width / 2), -(spritePos.Y + offset.Y + (float)(rect.Height / 2)));
							Vector2 targetSize3 = rect.Size.ToVector2();
							float rotation2 = 0f;
							color2 = new Color?(color);
							textureScale2 = new Vector2?(Vector2.One * scale);
							depth2 = new float?(Math.Min(depth + (decorativeSprite.Sprite.Depth - prefab.Sprite.Depth), 0.999f));
							vector = null;
							Vector2? origin3 = vector;
							Color? color4 = color2;
							vector = null;
							sprite3.DrawTiled(spriteBatch3, position3, targetSize3, rotation2, origin3, color4, vector, textureScale2, depth2);
						}
					}
					else
					{
						rect.Width = (int)((float)rect.Width * scale / prefab.Scale);
						rect.Height = (int)((float)rect.Height * scale / prefab.Scale);
						Vector2 spritePos2 = rect.Center.ToVector2();
						spritePos2.Y -= (float)rect.Height;
						prefab.Sprite.Draw(this.spriteRecorder, spritePos2 * new Vector2(1f, -1f), color, prefab.Sprite.Origin, rotationRad, scale, prefab.Sprite.effects, new float?(depth));
						foreach (DecorativeSprite decorativeSprite2 in itemPrefab.DecorativeSprites)
						{
							float rotationState = 0f;
							float offsetState2 = 0f;
							float rot = decorativeSprite2.GetRotation(ref rotationState, 0f);
							Vector2 offset2 = decorativeSprite2.GetOffset(ref offsetState2, Vector2.Zero, 0f) * scale;
							if (flippedX)
							{
								offset2.X = -offset2.X;
							}
							if (flippedY)
							{
								offset2.Y = -offset2.Y;
							}
							float throwAway = 0f;
							decorativeSprite2.Sprite.Draw(this.spriteRecorder, new Vector2(spritePos2.X + offset2.X, -(spritePos2.Y + offset2.Y)), color, decorativeSprite2.Sprite.Origin, rotationRad + rot, decorativeSprite2.GetScale(ref throwAway, 0f) * scale, prefab.Sprite.effects, new float?(Math.Min(depth + (decorativeSprite2.Sprite.Depth - prefab.Sprite.Depth), 0.999f)));
						}
					}
				}
			}
			prefab.Sprite.effects = prevEffects;
		}

		// Token: 0x06002290 RID: 8848 RVA: 0x0015C9F8 File Offset: 0x0015ABF8
		private void BakeItemComponents(ItemPrefab prefab, Rectangle rect, Color color, float scale, float rotationRad, float depth, out bool overrideSprite)
		{
			overrideSprite = false;
			float relativeScale = scale / prefab.Scale;
			foreach (ContentXElement subElement in prefab.ConfigElement.Elements())
			{
				string a = subElement.Name.LocalName.ToLowerInvariant();
				if (!(a == "turret"))
				{
					if (!(a == "door"))
					{
						if (a == "ladder")
						{
							ContentXElement backgroundSprElem = subElement.Elements().FirstOrDefault((ContentXElement e) => e.Name.LocalName.Equals("backgroundsprite", StringComparison.OrdinalIgnoreCase));
							ContentXElement contentXElement = null;
							if (backgroundSprElem != contentXElement)
							{
								Sprite backgroundSprite = new Sprite(backgroundSprElem, "", "", false, 1f);
								Sprite sprite = backgroundSprite;
								ISpriteBatch spriteBatch = this.spriteRecorder;
								Vector2 position = new Vector2((float)rect.Left, (float)(-(float)rect.Top)) - backgroundSprite.Origin * scale;
								Vector2 targetSize = new Vector2(backgroundSprite.size.X * scale, (float)rect.Height);
								float rotation = 0f;
								Color? color2 = new Color?(color);
								Vector2? textureScale = new Vector2?(Vector2.One * scale);
								float? depth2 = new float?(depth + 0.1f);
								sprite.DrawTiled(spriteBatch, position, targetSize, rotation, null, color2, null, textureScale, depth2);
							}
						}
					}
					else
					{
						Rectangle rectangle = rect;
						Vector2 vector = rect.Size.ToVector2() * relativeScale;
						rectangle.Size = vector.ToPoint();
						Rectangle scaledRect = rectangle;
						this.doors.Add(new SubmarinePreview.Door(scaledRect));
						ContentXElement doorSpriteElem = subElement.Elements().FirstOrDefault((ContentXElement e) => e.Name.LocalName.Equals("sprite", StringComparison.OrdinalIgnoreCase));
						ContentXElement contentXElement = null;
						if (doorSpriteElem != contentXElement)
						{
							string texturePath = doorSpriteElem.GetAttributeStringUnrestricted("texture", "");
							Vector2 pos = scaledRect.Location.ToVector2() * new Vector2(1f, -1f);
							if (subElement.GetAttributeBool("horizontal", false))
							{
								pos.Y += (float)scaledRect.Height * 0.5f;
							}
							else
							{
								pos.X += (float)scaledRect.Width * 0.5f;
							}
							Sprite doorSprite = new Sprite(doorSpriteElem, texturePath.Contains("/") ? "" : Path.GetDirectoryName(prefab.FilePath), "", false, 1f);
							this.spriteRecorder.Draw(doorSprite.Texture, pos, new Rectangle?(new Rectangle(doorSprite.SourceRect.X, doorSprite.SourceRect.Y, (int)doorSprite.size.X, (int)doorSprite.size.Y)), color, 0f, doorSprite.Origin, new Vector2(scale), SpriteEffects.None, doorSprite.Depth);
						}
					}
				}
				else
				{
					Sprite barrelSprite = null;
					Sprite railSprite = null;
					foreach (ContentXElement turretSubElem in subElement.Elements())
					{
						string a2 = turretSubElem.Name.ToString().ToLowerInvariant();
						if (!(a2 == "barrelsprite"))
						{
							if (a2 == "railsprite")
							{
								railSprite = new Sprite(turretSubElem, "", "", false, 1f);
							}
						}
						else
						{
							barrelSprite = new Sprite(turretSubElem, "", "", false, 1f);
						}
					}
					ContentXElement contentXElement2 = subElement;
					string key = "barrelpos";
					Vector2 vector = Vector2.Zero;
					Vector2 barrelPos = contentXElement2.GetAttributeVector2(key, vector);
					Vector2 relativeBarrelPos = barrelPos * prefab.Scale - new Vector2((float)(rect.Width / 2), (float)(rect.Height / 2));
					Vector2 transformedBarrelPos = MathUtils.RotatePoint(relativeBarrelPos, rotationRad);
					Vector2 drawPos = new Vector2((float)rect.X + (float)rect.Width * relativeScale / 2f + transformedBarrelPos.X * relativeScale, (float)rect.Y - (float)rect.Height * relativeScale / 2f - transformedBarrelPos.Y * relativeScale);
					drawPos.Y = -drawPos.Y;
					if (railSprite != null)
					{
						railSprite.Draw(this.spriteRecorder, drawPos, color, rotationRad, scale, SpriteEffects.None, new float?(depth + (railSprite.Depth - prefab.Sprite.Depth)));
					}
					if (barrelSprite != null)
					{
						barrelSprite.Draw(this.spriteRecorder, drawPos, color, rotationRad, scale, SpriteEffects.None, new float?(depth + (barrelSprite.Depth - prefab.Sprite.Depth)));
					}
				}
			}
		}

		// Token: 0x06002291 RID: 8849 RVA: 0x0015CEF8 File Offset: 0x0015B0F8
		private void ParseUpgrades(XElement prefabConfigElement, ref float scale)
		{
			foreach (XElement upgrade in prefabConfigElement.Elements("Upgrade"))
			{
				Version upgradeVersion = new Version(upgrade.GetAttributeString("gameversion", "0.0.0.0"));
				if (upgradeVersion >= this.submarineInfo.GameVersion)
				{
					string scaleModifier = upgrade.GetAttributeString("scale", "*1");
					float parsedScale2;
					if (scaleModifier.StartsWith("*"))
					{
						float parsedScale;
						if (float.TryParse(scaleModifier.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedScale))
						{
							scale *= parsedScale;
						}
					}
					else if (float.TryParse(scaleModifier, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedScale2))
					{
						scale = parsedScale2;
					}
				}
			}
		}

		// Token: 0x06002292 RID: 8850 RVA: 0x0015CFD0 File Offset: 0x0015B1D0
		private void RenderSubmarine(SpriteBatch spriteBatch, Rectangle scissorRectangle, GUIComponent component)
		{
			if (this.spriteRecorder == null)
			{
				return;
			}
			GUI.DrawRectangle(spriteBatch, scissorRectangle, new Color(0.051f, 0.149f, 0.271f, 1f), true, 0f, 1f);
			if (!this.spriteRecorder.ReadyToRender)
			{
				LocalizedString localizedString;
				if (this.loadTask.IsCompleted)
				{
					AggregateException exception = this.loadTask.Exception;
					localizedString = (((exception != null) ? exception.ToString() : null) ?? "Task completed without marking as ready to render");
				}
				else
				{
					localizedString = TextManager.Get(new string[]
					{
						"generatingsubmarinepreview",
						"loading"
					});
				}
				LocalizedString waitText = localizedString;
				Vector2 origin = GUIStyle.Font.MeasureString(waitText, false) * 0.5f;
				origin.X = MathF.Round(origin.X);
				origin.Y = MathF.Round(origin.Y);
				GUIStyle.Font.DrawString(spriteBatch, waitText, scissorRectangle.Center.ToVector2(), Color.White, 0f, origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft);
				return;
			}
			spriteBatch.End();
			Rectangle prevScissorRect = GameMain.Instance.GraphicsDevice.ScissorRectangle;
			GameMain.Instance.GraphicsDevice.ScissorRectangle = scissorRectangle;
			RasterizerState prevRasterizerState = GameMain.Instance.GraphicsDevice.RasterizerState;
			GameMain.Instance.GraphicsDevice.RasterizerState = GameMain.ScissorTestEnable;
			this.spriteRecorder.Render(this.camera);
			Vector2 mousePos = this.camera.ScreenToWorld(PlayerInput.MousePosition);
			mousePos.Y = -mousePos.Y;
			spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, GameMain.ScissorTestEnable, null, new Matrix?(this.camera.Transform));
			GameMain.Instance.GraphicsDevice.ScissorRectangle = scissorRectangle;
			foreach (SubmarinePreview.HullCollection hullCollection in this.hullCollections)
			{
				bool mouseOver = false;
				if (GUI.MouseOn == null || GUI.MouseOn == component)
				{
					foreach (Rectangle rect in hullCollection.Rects)
					{
						mouseOver = rect.Contains(mousePos);
						if (mouseOver)
						{
							break;
						}
					}
				}
				foreach (Rectangle rect2 in hullCollection.Rects)
				{
					GUI.DrawRectangle(spriteBatch, rect2, mouseOver ? Color.Red : Color.Blue, false, mouseOver ? 0.45f : 0.5f, (mouseOver ? 4f : 2f) / this.camera.Zoom);
				}
				if (mouseOver)
				{
					LocalizedString str = hullCollection.Name;
					Vector2 strSize = GUIStyle.Font.MeasureString(str, false) / this.camera.Zoom;
					Vector2 padding = new Vector2(30f, 30f) / this.camera.Zoom;
					Vector2 shift = new Vector2(10f, 0f) / this.camera.Zoom;
					GUI.DrawRectangle(spriteBatch, mousePos + shift, strSize + padding, Color.Black, true, 0.25f, 1f);
					GUIStyle.Font.DrawString(spriteBatch, str, mousePos + shift + (strSize + padding) * 0.5f, Color.White, 0f, strSize * this.camera.Zoom * 0.5f, 1f / this.camera.Zoom, SpriteEffects.None, 0f, Alignment.TopLeft);
				}
			}
			foreach (SubmarinePreview.Door door in this.doors)
			{
				GUI.DrawRectangle(spriteBatch, door.Rect, GUIStyle.Green * 0.5f, true, 0.4f, 1f);
			}
			spriteBatch.End();
			GameMain.Instance.GraphicsDevice.ScissorRectangle = prevScissorRect;
			GameMain.Instance.GraphicsDevice.RasterizerState = prevRasterizerState;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
		}

		// Token: 0x06002293 RID: 8851 RVA: 0x0015D490 File Offset: 0x0015B690
		public void Dispose()
		{
			if (this.previewFrame != null)
			{
				this.previewFrame.RectTransform.Parent = null;
				this.previewFrame = null;
			}
			SpriteRecorder spriteRecorder = this.spriteRecorder;
			if (spriteRecorder != null)
			{
				spriteRecorder.Dispose();
			}
			this.spriteRecorder = null;
			this.camera = null;
			this.isDisposed = true;
		}

		// Token: 0x04001164 RID: 4452
		private readonly SubmarineInfo submarineInfo;

		// Token: 0x04001165 RID: 4453
		private SpriteRecorder spriteRecorder;

		// Token: 0x04001166 RID: 4454
		private Camera camera;

		// Token: 0x04001167 RID: 4455
		private Task loadTask;

		// Token: 0x04001168 RID: 4456
		[TupleElementNames(new string[]
		{
			"Min",
			"Max"
		})]
		private ValueTuple<Vector2, Vector2> bounds;

		// Token: 0x04001169 RID: 4457
		private volatile bool isDisposed;

		// Token: 0x0400116A RID: 4458
		private GUIFrame previewFrame;

		// Token: 0x0400116B RID: 4459
		private readonly List<SubmarinePreview.HullCollection> hullCollections = new List<SubmarinePreview.HullCollection>();

		// Token: 0x0400116C RID: 4460
		private readonly List<SubmarinePreview.Door> doors;

		// Token: 0x0400116D RID: 4461
		private static SubmarinePreview instance;

		// Token: 0x02000BB8 RID: 3000
		private sealed class LoadedHull
		{
			// Token: 0x060079A9 RID: 31145 RVA: 0x0037F154 File Offset: 0x0037D354
			public LoadedHull(XElement element)
			{
				this.ID = (ushort)element.GetAttributeInt("id", 0);
				this.NameIdentifier = element.GetAttributeIdentifier("roomname", "");
				this.Rect = element.GetAttributeRect("rect", Rectangle.Empty);
				this.Rect.Y = -this.Rect.Y;
				this.LinkedHulls = element.GetAttributeUshortArray("linked", Array.Empty<ushort>()).ToImmutableList<ushort>();
			}

			// Token: 0x040048A6 RID: 18598
			public ushort ID;

			// Token: 0x040048A7 RID: 18599
			public readonly ImmutableList<ushort> LinkedHulls;

			// Token: 0x040048A8 RID: 18600
			public readonly Rectangle Rect;

			// Token: 0x040048A9 RID: 18601
			public readonly Identifier NameIdentifier;
		}

		// Token: 0x02000BB9 RID: 3001
		private sealed class HullCollection
		{
			// Token: 0x060079AA RID: 31146 RVA: 0x0037F1D8 File Offset: 0x0037D3D8
			public HullCollection(SubmarinePreview.LoadedHull hull)
			{
				this.Name = TextManager.Get(hull.NameIdentifier).Fallback(hull.NameIdentifier.ToString(), true);
				this.AddHull(hull);
			}

			// Token: 0x060079AB RID: 31147 RVA: 0x0037F235 File Offset: 0x0037D435
			public void AddHull(SubmarinePreview.LoadedHull hull)
			{
				this.Hulls.Add(hull);
				this.Rects.Add(hull.Rect);
			}

			// Token: 0x060079AC RID: 31148 RVA: 0x0037F254 File Offset: 0x0037D454
			private bool Contains(ushort hullId)
			{
				return this.Hulls.Any((SubmarinePreview.LoadedHull h) => h.ID == hullId);
			}

			// Token: 0x060079AD RID: 31149 RVA: 0x0037F288 File Offset: 0x0037D488
			public bool IsLinkedTo(SubmarinePreview.HullCollection other)
			{
				Func<ushort, bool> <>9__2;
				Func<ushort, bool> <>9__3;
				return this.Hulls.Any(delegate(SubmarinePreview.LoadedHull h)
				{
					IEnumerable<ushort> linkedHulls = h.LinkedHulls;
					Func<ushort, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((ushort id) => other.Contains(id)));
					}
					return linkedHulls.Any(predicate);
				}) || other.Hulls.Any(delegate(SubmarinePreview.LoadedHull h)
				{
					IEnumerable<ushort> linkedHulls = h.LinkedHulls;
					Func<ushort, bool> predicate;
					if ((predicate = <>9__3) == null)
					{
						predicate = (<>9__3 = ((ushort id) => this.Contains(id)));
					}
					return linkedHulls.Any(predicate);
				});
			}

			// Token: 0x040048AA RID: 18602
			public readonly List<SubmarinePreview.LoadedHull> Hulls = new List<SubmarinePreview.LoadedHull>();

			// Token: 0x040048AB RID: 18603
			public readonly List<Rectangle> Rects = new List<Rectangle>();

			// Token: 0x040048AC RID: 18604
			public readonly LocalizedString Name;
		}

		// Token: 0x02000BBA RID: 3002
		private readonly struct Door
		{
			// Token: 0x060079AE RID: 31150 RVA: 0x0037F2E0 File Offset: 0x0037D4E0
			public Door(Rectangle rect)
			{
				rect.Y = -rect.Y;
				this.Rect = rect;
			}

			// Token: 0x040048AD RID: 18605
			public readonly Rectangle Rect;
		}
	}
}
