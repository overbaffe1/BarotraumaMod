using System;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200015A RID: 346
	internal static class WikiImage
	{
		// Token: 0x06002A93 RID: 10899 RVA: 0x001D5CD8 File Offset: 0x001D3ED8
		public static Rectangle CalculateBoundingBox(Character character)
		{
			WikiImage.<>c__DisplayClass0_0 CS$<>8__locals1;
			CS$<>8__locals1.boundingBox = new Rectangle(character.WorldPosition.ToPoint(), Point.Zero);
			foreach (Limb limb in character.AnimController.Limbs)
			{
				if (limb.ActiveSprite != null)
				{
					float extentX = limb.ActiveSprite.size.X * limb.Scale * limb.TextureScale * 0.5f;
					float extentY = limb.ActiveSprite.size.Y * limb.Scale * limb.TextureScale * 0.5f;
					Vector2 origin = (limb.ActiveSprite.Origin - limb.ActiveSprite.SourceRect.Size.ToVector2() * 0.5f) * limb.Scale * limb.TextureScale;
					WikiImage.<CalculateBoundingBox>g__addPointsToBBox|0_0(extentX, extentY, limb.WorldPosition, origin, limb.body.Rotation, ref CS$<>8__locals1);
				}
			}
			if (character.Inventory != null)
			{
				foreach (Item item in character.Inventory.AllItems)
				{
					if (((item != null) ? item.Sprite : null) != null && ((item != null) ? item.body : null) != null)
					{
						float extentX2 = item.Sprite.size.X * item.Scale * 0.5f;
						float extentY2 = item.Sprite.size.Y * item.Scale * 0.5f;
						Vector2 origin2 = (item.Sprite.Origin - item.Sprite.SourceRect.Size.ToVector2() * 0.5f) * item.Scale;
						WikiImage.<CalculateBoundingBox>g__addPointsToBBox|0_0(extentX2, extentY2, item.WorldPosition, origin2, item.body.Rotation, ref CS$<>8__locals1);
					}
				}
			}
			CS$<>8__locals1.boundingBox.X = CS$<>8__locals1.boundingBox.X - 25;
			CS$<>8__locals1.boundingBox.Y = CS$<>8__locals1.boundingBox.Y - 25;
			CS$<>8__locals1.boundingBox.Width = CS$<>8__locals1.boundingBox.Width + 50;
			CS$<>8__locals1.boundingBox.Height = CS$<>8__locals1.boundingBox.Height + 50;
			return CS$<>8__locals1.boundingBox;
		}

		// Token: 0x06002A94 RID: 10900 RVA: 0x001D5F70 File Offset: 0x001D4170
		public static void Create(Character character)
		{
			Rectangle boundingBox = WikiImage.CalculateBoundingBox(character);
			int texWidth = Math.Clamp((int)((float)boundingBox.Width * 2.5f), 512, 4096);
			float zoom = (float)texWidth / (float)boundingBox.Width;
			int texHeight = (int)(zoom * (float)boundingBox.Height);
			Camera cam = new Camera
			{
				AutoUpdateToScreenResolution = false,
				MaxZoom = zoom,
				MinZoom = zoom * 0.5f,
				Zoom = zoom
			};
			cam.SetResolution(new Point(texWidth, texHeight));
			cam.Position = boundingBox.Center.ToVector2();
			cam.UpdateTransform(false, true);
			using (RenderTarget2D rt = new RenderTarget2D(GameMain.Instance.GraphicsDevice, texWidth, texHeight, false, SurfaceFormat.Color, DepthFormat.None))
			{
				using (SpriteBatch spriteBatch = new SpriteBatch(GameMain.Instance.GraphicsDevice))
				{
					Viewport prevViewport = GameMain.Instance.GraphicsDevice.Viewport;
					GameMain.Instance.GraphicsDevice.Viewport = new Viewport(0, 0, texWidth, texHeight);
					GameMain.Instance.GraphicsDevice.SetRenderTarget(rt);
					GameMain.Instance.GraphicsDevice.Clear(Color.Transparent);
					spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, new Matrix?(cam.Transform));
					character.Draw(spriteBatch, cam);
					if (character.Inventory != null)
					{
						foreach (Item item in character.Inventory.AllItems)
						{
							if (item != null)
							{
								item.Draw(spriteBatch, false, false, null, null);
								item.Draw(spriteBatch, false, true, null, null);
							}
						}
					}
					spriteBatch.End();
					GameMain.Instance.GraphicsDevice.SetRenderTarget(null);
					GameMain.Instance.GraphicsDevice.Viewport = prevViewport;
					using (FileStream fs = File.Open("wikiimage.png", FileMode.Create, FileAccess.ReadWrite, null, true))
					{
						rt.SaveAsPng(fs, texWidth, texHeight);
					}
				}
			}
		}

		// Token: 0x06002A95 RID: 10901 RVA: 0x001D61FC File Offset: 0x001D43FC
		public static void Create(Submarine sub)
		{
			int width = 4096;
			int height = 4096;
			Rectangle subDimensions = sub.CalculateDimensions(false);
			Vector2 viewPos = subDimensions.Center.ToVector2();
			float scale = Math.Min((float)width / (float)subDimensions.Width, (float)height / (float)subDimensions.Height);
			Matrix viewMatrix = Matrix.CreateTranslation(new Vector3((float)width / 2f, (float)height / 2f, 0f));
			WikiImage.<>c__DisplayClass2_0 CS$<>8__locals1;
			CS$<>8__locals1.transform = Matrix.CreateTranslation(new Vector3(-viewPos.X, viewPos.Y, 0f)) * Matrix.CreateScale(new Vector3(scale, scale, 1f)) * viewMatrix;
			using (RenderTarget2D rt = new RenderTarget2D(GameMain.Instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None))
			{
				WikiImage.<>c__DisplayClass2_1 CS$<>8__locals2 = new WikiImage.<>c__DisplayClass2_1();
				CS$<>8__locals2.spriteBatch = new SpriteBatch(GameMain.Instance.GraphicsDevice);
				try
				{
					Viewport prevViewport = GameMain.Instance.GraphicsDevice.Viewport;
					GameMain.Instance.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
					GameMain.Instance.GraphicsDevice.SetRenderTarget(rt);
					GameMain.Instance.GraphicsDevice.Clear(Color.Transparent);
					CS$<>8__locals2.<Create>g__DrawBatch|4(delegate
					{
						Submarine.DrawBack(CS$<>8__locals2.spriteBatch, false, delegate(MapEntity e)
						{
							Structure s = e as Structure;
							return s != null && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null);
						});
					}, ref CS$<>8__locals1);
					CS$<>8__locals2.<Create>g__DrawBatch|4(delegate
					{
						Submarine.DrawBack(CS$<>8__locals2.spriteBatch, false, (MapEntity e) => !(e is Structure) || e.SpriteDepth < 0.9f);
					}, ref CS$<>8__locals1);
					CS$<>8__locals2.<Create>g__DrawBatch|4(delegate
					{
						Submarine.DrawDamageable(CS$<>8__locals2.spriteBatch, null, false, null);
					}, ref CS$<>8__locals1);
					CS$<>8__locals2.<Create>g__DrawBatch|4(delegate
					{
						Submarine.DrawFront(CS$<>8__locals2.spriteBatch, false, null);
					}, ref CS$<>8__locals1);
					GameMain.Instance.GraphicsDevice.SetRenderTarget(null);
					GameMain.Instance.GraphicsDevice.Viewport = prevViewport;
					using (FileStream fs = File.Open("wikiimage.png", FileMode.Create, FileAccess.ReadWrite, null, true))
					{
						rt.SaveAsPng(fs, width, height);
					}
				}
				finally
				{
					if (CS$<>8__locals2.spriteBatch != null)
					{
						((IDisposable)CS$<>8__locals2.spriteBatch).Dispose();
					}
				}
			}
		}

		// Token: 0x06002A96 RID: 10902 RVA: 0x001D644C File Offset: 0x001D464C
		[CompilerGenerated]
		internal static void <CalculateBoundingBox>g__addPointsToBBox|0_0(float extentX, float extentY, Vector2 worldPos, Vector2 origin, float rotation, ref WikiImage.<>c__DisplayClass0_0 A_5)
		{
			float sinRotation = (float)Math.Sin((double)rotation);
			float cosRotation = (float)Math.Cos((double)rotation);
			origin = new Vector2(origin.X * cosRotation + origin.Y * sinRotation, origin.X * sinRotation - origin.Y * cosRotation);
			Point limbPos = worldPos.ToPoint();
			A_5.boundingBox.AddPoint(limbPos);
			Vector2 xExtend = new Vector2(extentX * cosRotation, extentX * sinRotation);
			Vector2 yExtend = new Vector2(extentY * sinRotation, -extentY * cosRotation);
			A_5.boundingBox.AddPoint(limbPos + (xExtend + yExtend - origin).ToPoint());
			A_5.boundingBox.AddPoint(limbPos + (xExtend - yExtend - origin).ToPoint());
			A_5.boundingBox.AddPoint(limbPos + (-xExtend - yExtend - origin).ToPoint());
			A_5.boundingBox.AddPoint(limbPos + (-xExtend + yExtend - origin).ToPoint());
		}
	}
}
