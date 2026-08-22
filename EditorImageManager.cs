using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000105 RID: 261
	[NullableContext(1)]
	[Nullable(0)]
	internal class EditorImageManager
	{
		// Token: 0x06002477 RID: 9335 RVA: 0x00171314 File Offset: 0x0016F514
		public void Save(XElement element)
		{
			XElement saveElement = new XElement("editorimages");
			foreach (EditorImage image in this.Images)
			{
				EditorImageManager.EditorImageContainer container = EditorImageManager.EditorImageContainer.ImageToContainer(image);
				saveElement.Add(EditorImageManager.EditorImageContainer.SerializeImage(container));
			}
			foreach (EditorImageManager.EditorImageContainer container2 in this.PendingImages)
			{
				saveElement.Add(EditorImageManager.EditorImageContainer.SerializeImage(container2));
			}
			element.Add(saveElement);
		}

		// Token: 0x06002478 RID: 9336 RVA: 0x001713D4 File Offset: 0x0016F5D4
		public void Load(XElement element)
		{
			this.Clear(true);
			foreach (XElement subElement in element.Elements())
			{
				EditorImageManager.EditorImageContainer? tempImage = EditorImageManager.EditorImageContainer.Load(subElement);
				if (tempImage != null)
				{
					this.PendingImages.Add(tempImage.Value);
				}
			}
		}

		// Token: 0x06002479 RID: 9337 RVA: 0x00171444 File Offset: 0x0016F644
		public void OnEditorSelected()
		{
			this.editModeText = TextManager.Get("SubEditor.ImageEditingMode");
			this.textSize = GUIStyle.LargeFont.MeasureString(this.editModeText, false);
			this.TryLoadPendingImages();
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x00171474 File Offset: 0x0016F674
		private void TryLoadPendingImages()
		{
			if (this.PendingImages.Count == 0)
			{
				return;
			}
			this.Clear(false);
			foreach (EditorImageManager.EditorImageContainer pendingImage in this.PendingImages)
			{
				EditorImage img = pendingImage.CreateImage();
				if (img.Image != null)
				{
					this.Images.Add(img);
					img.UpdateRectangle();
				}
			}
			this.UpdateImageCategories();
			this.PendingImages.Clear();
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x00171508 File Offset: 0x0016F708
		public void Clear(bool alsoPending = false)
		{
			foreach (EditorImage img in this.Images)
			{
				Texture2D image = img.Image;
				if (image != null)
				{
					image.Dispose();
				}
			}
			this.Images.Clear();
			this.screenImages.Clear();
			this.worldImages.Clear();
			if (alsoPending)
			{
				this.PendingImages.Clear();
			}
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x00171594 File Offset: 0x0016F794
		public void Update(float deltaTime)
		{
			if (!this.EditorMode)
			{
				return;
			}
			foreach (EditorImage image in this.Images)
			{
				image.Update(deltaTime);
			}
			if (PlayerInput.PrimaryMouseButtonDown())
			{
				EditorImage hover = this.Images.FirstOrDefault((EditorImage img) => img.IsMouseOn());
				if (hover != null)
				{
					foreach (EditorImage image2 in this.Images)
					{
						image2.Selected = false;
					}
					hover.Selected = true;
				}
			}
			if (PlayerInput.KeyHit(Keys.Delete) || (PlayerInput.IsCtrlDown() && PlayerInput.KeyHit(Keys.D)))
			{
				this.Images.RemoveAll((EditorImage img) => img.Selected);
				this.UpdateImageCategories();
			}
			if (PlayerInput.KeyHit(Keys.Space))
			{
				foreach (EditorImage image3 in this.Images)
				{
					if (image3.Selected)
					{
						if (image3.DrawTarget == EditorImage.DrawTargetType.World)
						{
							Vector2 pos = image3.Position;
							pos.Y = -pos.Y;
							pos = Screen.Selected.Cam.WorldToScreen(pos);
							if (PlayerInput.IsShiftDown())
							{
								pos = new Vector2((float)GameMain.GraphicsWidth / 2f, (float)GameMain.GraphicsHeight / 2f);
							}
							image3.Position = pos;
							image3.DrawTarget = EditorImage.DrawTargetType.Camera;
							image3.Scale *= Screen.Selected.Cam.Zoom;
							image3.UpdateRectangle();
						}
						else
						{
							Vector2 pos2 = Screen.Selected.Cam.ScreenToWorld(image3.Position);
							pos2.Y = -pos2.Y;
							image3.Position = pos2;
							image3.DrawTarget = EditorImage.DrawTargetType.World;
							image3.Scale /= Screen.Selected.Cam.Zoom;
							image3.UpdateRectangle();
						}
					}
				}
				this.UpdateImageCategories();
			}
			MapEntity.DisableSelect = true;
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x00171838 File Offset: 0x0016FA38
		private void UpdateImageCategories()
		{
			this.screenImages.Clear();
			this.worldImages.Clear();
			foreach (EditorImage image in this.Images)
			{
				EditorImage.DrawTargetType drawTarget = image.DrawTarget;
				if (drawTarget == EditorImage.DrawTargetType.World)
				{
					this.worldImages.Add(image);
				}
				else
				{
					this.screenImages.Add(image);
				}
			}
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x001718C0 File Offset: 0x0016FAC0
		public void CreateImageWizard()
		{
			string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			if (!Directory.Exists(home))
			{
				return;
			}
			FileSelection.OnFileSelected = delegate(string file)
			{
				Vector2 pos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
				pos.Y = -pos.Y;
				this.Images.Add(new EditorImage(file, pos)
				{
					DrawTarget = EditorImage.DrawTargetType.World
				});
				this.UpdateImageCategories();
				GameSettings.SaveCurrentConfig();
			};
			FileSelection.ClearFileTypeFilters();
			FileSelection.AddFileTypeFilter("PNG", "*.png");
			FileSelection.AddFileTypeFilter("JPEG", "*.jpg, *.jpeg");
			FileSelection.AddFileTypeFilter("All files", "*.*");
			FileSelection.SelectFileTypeFilter("*.png");
			FileSelection.CurrentDirectory = home;
			FileSelection.Open = true;
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x00171938 File Offset: 0x0016FB38
		public void DrawEditing(SpriteBatch spriteBatch, Camera cam)
		{
			if (!this.EditorMode)
			{
				return;
			}
			this.DrawImages(spriteBatch, cam);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, null);
			Vector2 textPos = new Vector2((float)GameMain.GraphicsWidth / 2f - this.textSize.X / 2f, (float)GameMain.GraphicsHeight / 10f - this.textSize.Y / 2f);
			GUI.DrawString(spriteBatch, textPos, this.editModeText, GUIStyle.Yellow, new Color?(Color.Black * 0.4f), 8, GUIStyle.LargeFont, ForceUpperCase.Inherit);
			spriteBatch.End();
		}

		// Token: 0x06002480 RID: 9344 RVA: 0x001719EA File Offset: 0x0016FBEA
		public void Draw(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.EditorMode)
			{
				return;
			}
			this.DrawImages(spriteBatch, cam);
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x00171A00 File Offset: 0x0016FC00
		private void DrawImages(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.screenImages.Count > 0)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, null);
				foreach (EditorImage image in this.screenImages)
				{
					image.Draw(spriteBatch);
					if (this.EditorMode)
					{
						image.DrawEditing(spriteBatch, cam);
					}
				}
				spriteBatch.End();
			}
			if (this.worldImages.Count > 0)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, new Matrix?(cam.Transform));
				foreach (EditorImage image2 in this.worldImages)
				{
					image2.Draw(spriteBatch);
					if (this.EditorMode)
					{
						image2.DrawEditing(spriteBatch, cam);
					}
				}
				spriteBatch.End();
			}
		}

		// Token: 0x0400122A RID: 4650
		private readonly List<EditorImageManager.EditorImageContainer> PendingImages = new List<EditorImageManager.EditorImageContainer>();

		// Token: 0x0400122B RID: 4651
		public readonly List<EditorImage> Images = new List<EditorImage>();

		// Token: 0x0400122C RID: 4652
		private readonly List<EditorImage> screenImages = new List<EditorImage>();

		// Token: 0x0400122D RID: 4653
		private readonly List<EditorImage> worldImages = new List<EditorImage>();

		// Token: 0x0400122E RID: 4654
		public bool EditorMode;

		// Token: 0x0400122F RID: 4655
		private LocalizedString editModeText = "";

		// Token: 0x04001230 RID: 4656
		private Vector2 textSize = Vector2.Zero;

		// Token: 0x02000C1E RID: 3102
		[Nullable(0)]
		private struct EditorImageContainer
		{
			// Token: 0x06007AF6 RID: 31478 RVA: 0x00382E34 File Offset: 0x00381034
			public EditorImage CreateImage()
			{
				return new EditorImage(this.Path, this.Position)
				{
					Position = this.Position,
					Scale = this.Scale,
					Opacity = this.Opacity,
					Rotation = this.Rotation,
					DrawTarget = this.DrawTarget
				};
			}

			// Token: 0x06007AF7 RID: 31479 RVA: 0x00382E90 File Offset: 0x00381090
			public static EditorImageManager.EditorImageContainer? Load(XElement element)
			{
				string path = element.GetAttributeString("path", "");
				if (string.IsNullOrWhiteSpace(path))
				{
					return null;
				}
				Vector2 pos = element.GetAttributeVector2("pos", Vector2.Zero);
				float scale = element.GetAttributeFloat("scale", 1f);
				float rotation = element.GetAttributeFloat("rotation", 0f);
				float opacity = element.GetAttributeFloat("opacity", 1f);
				string drawTargetString = element.GetAttributeString("drawtarget", "");
				EditorImage.DrawTargetType drawTarget;
				if (!Enum.TryParse<EditorImage.DrawTargetType>(drawTargetString, out drawTarget))
				{
					drawTarget = EditorImage.DrawTargetType.World;
				}
				return new EditorImageManager.EditorImageContainer?(new EditorImageManager.EditorImageContainer
				{
					Path = path,
					Rotation = rotation,
					Opacity = opacity,
					Position = pos,
					Scale = scale,
					DrawTarget = drawTarget
				});
			}

			// Token: 0x06007AF8 RID: 31480 RVA: 0x00382F68 File Offset: 0x00381168
			public static EditorImageManager.EditorImageContainer ImageToContainer(EditorImage img)
			{
				return new EditorImageManager.EditorImageContainer
				{
					Path = img.ImagePath,
					Rotation = img.Rotation,
					Position = img.Position,
					Opacity = img.Opacity,
					Scale = img.Scale,
					DrawTarget = img.DrawTarget
				};
			}

			// Token: 0x06007AF9 RID: 31481 RVA: 0x00382FCC File Offset: 0x003811CC
			public static XElement SerializeImage(EditorImageManager.EditorImageContainer image)
			{
				return new XElement("image", new object[]
				{
					new XAttribute("pos", XMLExtensions.Vector2ToString(image.Position)),
					new XAttribute("rotation", image.Rotation),
					new XAttribute("opacity", image.Opacity),
					new XAttribute("path", image.Path),
					new XAttribute("scale", image.Scale),
					new XAttribute("drawtarget", image.DrawTarget.ToString())
				});
			}

			// Token: 0x04004A04 RID: 18948
			public float Rotation;

			// Token: 0x04004A05 RID: 18949
			public float Scale;

			// Token: 0x04004A06 RID: 18950
			public Vector2 Position;

			// Token: 0x04004A07 RID: 18951
			public string Path;

			// Token: 0x04004A08 RID: 18952
			public float Opacity;

			// Token: 0x04004A09 RID: 18953
			public EditorImage.DrawTargetType DrawTarget;
		}
	}
}
