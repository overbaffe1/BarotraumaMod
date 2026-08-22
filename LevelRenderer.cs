using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Particles;
using Barotrauma.RuinGeneration;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000E0 RID: 224
	internal class LevelRenderer : IDisposable
	{
		// Token: 0x1700085C RID: 2140
		// (get) Token: 0x06001F11 RID: 7953 RVA: 0x0013264A File Offset: 0x0013084A
		// (set) Token: 0x06001F12 RID: 7954 RVA: 0x00132652 File Offset: 0x00130852
		public Color FlashColor { get; private set; }

		// Token: 0x1700085D RID: 2141
		// (get) Token: 0x06001F13 RID: 7955 RVA: 0x0013265B File Offset: 0x0013085B
		// (set) Token: 0x06001F14 RID: 7956 RVA: 0x00132663 File Offset: 0x00130863
		public float ChromaticAberrationStrength
		{
			get
			{
				return this.chromaticAberrationStrength;
			}
			set
			{
				this.chromaticAberrationStrength = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x1700085E RID: 2142
		// (get) Token: 0x06001F15 RID: 7957 RVA: 0x0013267B File Offset: 0x0013087B
		// (set) Token: 0x06001F16 RID: 7958 RVA: 0x00132683 File Offset: 0x00130883
		public float CollapseEffectStrength { get; set; }

		// Token: 0x1700085F RID: 2143
		// (get) Token: 0x06001F17 RID: 7959 RVA: 0x0013268C File Offset: 0x0013088C
		// (set) Token: 0x06001F18 RID: 7960 RVA: 0x00132694 File Offset: 0x00130894
		public Vector2 CollapseEffectOrigin { get; set; }

		// Token: 0x06001F19 RID: 7961 RVA: 0x001326A0 File Offset: 0x001308A0
		public LevelRenderer(Level level)
		{
			this.cullNone = new RasterizerState
			{
				CullMode = CullMode.None
			};
			if (LevelRenderer.wallEdgeEffect == null)
			{
				LevelRenderer.wallEdgeEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f),
					VertexColorEnabled = true,
					TextureEnabled = true,
					Texture = level.GenerationParams.WallEdgeSprite.Texture
				};
				LevelRenderer.wallEdgeEffect.CurrentTechnique = LevelRenderer.wallEdgeEffect.Techniques["BasicEffect_Texture"];
			}
			if (LevelRenderer.wallCenterEffect == null)
			{
				LevelRenderer.wallCenterEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					VertexColorEnabled = true,
					TextureEnabled = true,
					Texture = level.GenerationParams.WallSprite.Texture
				};
				LevelRenderer.wallCenterEffect.CurrentTechnique = LevelRenderer.wallCenterEffect.Techniques["BasicEffect_Texture"];
			}
			if (LevelRenderer.wallInnerEffect == null)
			{
				LevelRenderer.wallInnerEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					VertexColorEnabled = true,
					TextureEnabled = false
				};
				LevelRenderer.wallInnerEffect.CurrentTechnique = LevelRenderer.wallInnerEffect.Techniques["BasicEffect_Texture"];
			}
			this.level = level;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x001327F4 File Offset: 0x001309F4
		public void ReloadTextures()
		{
			this.level.GenerationParams.WallEdgeSprite.ReloadTexture();
			LevelRenderer.wallEdgeEffect.Texture = this.level.GenerationParams.WallEdgeSprite.Texture;
			this.level.GenerationParams.WallSprite.ReloadTexture();
			LevelRenderer.wallCenterEffect.Texture = this.level.GenerationParams.WallSprite.Texture;
		}

		// Token: 0x06001F1B RID: 7963 RVA: 0x00132869 File Offset: 0x00130A69
		public void Flash()
		{
			this.flashTimer = 1f;
		}

		// Token: 0x06001F1C RID: 7964 RVA: 0x00132878 File Offset: 0x00130A78
		public void Update(float deltaTime, Camera cam)
		{
			if (this.CollapseEffectStrength > 0f)
			{
				this.CollapseEffectStrength = Math.Max(0f, this.CollapseEffectStrength - deltaTime);
			}
			if (this.ChromaticAberrationStrength > 0f)
			{
				this.ChromaticAberrationStrength = Math.Max(0f, this.ChromaticAberrationStrength - deltaTime * 10f);
			}
			if (this.level.GenerationParams.FlashInterval.Y > 0f)
			{
				this.flashCooldown -= deltaTime;
				if (this.flashCooldown <= 0f)
				{
					this.flashTimer = 1f;
					Sound flashSound = this.level.GenerationParams.FlashSound;
					if (flashSound != null)
					{
						flashSound.Play(new float?(1f), SoundManager.SoundCategoryDefault);
					}
					this.flashCooldown = Rand.Range(this.level.GenerationParams.FlashInterval.X, this.level.GenerationParams.FlashInterval.Y, Rand.RandSync.Unsynced);
				}
				if (this.flashTimer > 0f)
				{
					float brightness = this.flashTimer * 1.1f - PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.66f) * 0.1f;
					this.FlashColor = this.level.GenerationParams.FlashColor.Multiply(MathHelper.Clamp(brightness, 0f, 1f), false);
					this.flashTimer -= deltaTime * 0.5f;
				}
				else
				{
					this.FlashColor = Color.TransparentBlack;
				}
			}
			Vector2 currentWaterParticleVel = this.level.GenerationParams.WaterParticleVelocity;
			foreach (ILevelRenderableObject obj in this.level.LevelObjectManager.GetAllVisibleObjects())
			{
				LevelObject levelObject = obj as LevelObject;
				if (levelObject != null && levelObject.Triggers != null)
				{
					Vector2 objectMaxFlow = Vector2.Zero;
					foreach (LevelTrigger trigger in levelObject.Triggers)
					{
						Vector2 vel = trigger.GetWaterFlowVelocity(cam.WorldViewCenter);
						if (vel.LengthSquared() > objectMaxFlow.LengthSquared())
						{
							objectMaxFlow = vel;
						}
					}
					currentWaterParticleVel += objectMaxFlow;
				}
			}
			this.waterParticleVelocity = Vector2.Lerp(this.waterParticleVelocity, currentWaterParticleVel, deltaTime);
			WaterRenderer instance = WaterRenderer.Instance;
			if (instance != null)
			{
				instance.ScrollWater(this.waterParticleVelocity, deltaTime);
			}
			this.level.GenerationParams.UpdateWaterParticleOffset(ref this.waterParticleOffset, this.waterParticleVelocity, deltaTime);
		}

		// Token: 0x06001F1D RID: 7965 RVA: 0x00132B28 File Offset: 0x00130D28
		public static VertexPositionColorTexture[] GetColoredVertices(VertexPositionTexture[] vertices, Color color)
		{
			VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				verts[i] = new VertexPositionColorTexture(vertices[i].Position, color, vertices[i].TextureCoordinate);
			}
			return verts;
		}

		// Token: 0x06001F1E RID: 7966 RVA: 0x00132B74 File Offset: 0x00130D74
		public void SetVertices(VertexPositionColorTexture[] wallVertices, VertexPositionColorTexture[] wallEdgeVertices, VertexPositionColor[] wallInnerVertices, Texture2D wallTexture, Texture2D edgeTexture)
		{
			LevelWallVertexBuffer existingBuffer = this.vertexBuffers.Find((LevelWallVertexBuffer vb) => vb.WallTexture == wallTexture && vb.EdgeTexture == edgeTexture);
			if (existingBuffer != null)
			{
				existingBuffer.Append(wallVertices, wallEdgeVertices, wallInnerVertices);
				return;
			}
			this.vertexBuffers.Add(new LevelWallVertexBuffer(wallVertices, wallEdgeVertices, wallInnerVertices, wallTexture, edgeTexture));
		}

		// Token: 0x06001F1F RID: 7967 RVA: 0x00132BDC File Offset: 0x00130DDC
		public void DrawBackground(SpriteBatch spriteBatch, Camera cam, LevelObjectManager backgroundSpriteManager = null, BackgroundCreatureManager backgroundCreatureManager = null, ParticleManager particleManager = null)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null, null);
			this.level.GenerationParams.DrawBackgrounds(spriteBatch, cam);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null, new Matrix?(cam.Transform));
			if (backgroundSpriteManager != null)
			{
				backgroundSpriteManager.DrawObjectsBack(spriteBatch, backgroundCreatureManager, cam);
			}
			this.level.GenerationParams.DrawWaterParticles(spriteBatch, cam, this.waterParticleOffset);
			ParticleManager particleManager2 = GameMain.ParticleManager;
			if (particleManager2 != null)
			{
				particleManager2.Draw(spriteBatch, true, new bool?(false), ParticleBlendState.AlphaBlend, new bool?(true));
			}
			spriteBatch.End();
			this.RenderWalls(GameMain.Instance.GraphicsDevice, cam);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null, new Matrix?(cam.Transform));
			if (backgroundSpriteManager != null)
			{
				backgroundSpriteManager.DrawObjectsMid(spriteBatch, backgroundCreatureManager, cam);
			}
			spriteBatch.End();
		}

		// Token: 0x06001F20 RID: 7968 RVA: 0x00132CD2 File Offset: 0x00130ED2
		public void DrawForeground(SpriteBatch spriteBatch, Camera cam, BackgroundCreatureManager backgroundCreatureManager, LevelObjectManager backgroundSpriteManager = null)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null, new Matrix?(cam.Transform));
			if (backgroundSpriteManager != null)
			{
				backgroundSpriteManager.DrawObjectsFront(spriteBatch, backgroundCreatureManager, cam);
			}
			spriteBatch.End();
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x00132D0C File Offset: 0x00130F0C
		public void DrawDebugOverlay(SpriteBatch spriteBatch, Camera cam)
		{
			if (GameMain.DebugDraw && cam.Zoom > 0.1f)
			{
				List<VoronoiCell> cells = this.level.GetCells(cam.WorldViewCenter, 2);
				foreach (VoronoiCell cell in cells)
				{
					GUI.DrawRectangle(spriteBatch, new Vector2(cell.Center.X - 10f, -cell.Center.Y - 10f), new Vector2(20f, 20f), Color.Cyan, true, 0f, 1f);
					GUI.DrawLine(spriteBatch, new Vector2(cell.Edges[0].Point1.X + cell.Translation.X, -(cell.Edges[0].Point1.Y + cell.Translation.Y)), new Vector2(cell.Center.X, -cell.Center.Y), Color.Blue * 0.5f, 0f, 1f);
					foreach (GraphEdge edge in cell.Edges)
					{
						GUI.DrawLine(spriteBatch, new Vector2(edge.Point1.X + cell.Translation.X, -(edge.Point1.Y + cell.Translation.Y)), new Vector2(edge.Point2.X + cell.Translation.X, -(edge.Point2.Y + cell.Translation.Y)), edge.NextToCave ? Color.Red : ((cell.Body == null) ? (Color.Cyan * 0.5f) : (edge.IsSolid ? Color.White : Color.Gray)), 0f, (float)(edge.NextToCave ? 8 : 1));
						Vector2 normal = edge.GetNormal(cell);
						GUI.DrawLine(spriteBatch, (edge.Center + cell.Translation).FlipY(), (edge.Center + cell.Translation + normal * 32f).FlipY(), Color.Red * 0.5f, 0f, 3f);
					}
					foreach (Vector2 point in cell.BodyVertices)
					{
						GUI.DrawRectangle(spriteBatch, new Vector2(point.X + cell.Translation.X, -(point.Y + cell.Translation.Y)), new Vector2(10f, 10f), Color.White, true, 0f, 1f);
					}
				}
				foreach (Level.AbyssIsland abyssIsland in this.level.AbyssIslands)
				{
					GUI.DrawRectangle(spriteBatch, new Vector2((float)abyssIsland.Area.X, (float)(-(float)abyssIsland.Area.Y - abyssIsland.Area.Height)), abyssIsland.Area.Size.ToVector2(), Color.Cyan, false, 0f, 5f);
				}
				foreach (Ruin ruin in this.level.Ruins)
				{
					ruin.DebugDraw(spriteBatch);
				}
			}
			Vector2 pos = new Vector2(0f, (float)(-(float)this.level.Size.Y));
			if ((float)cam.WorldView.Y >= -pos.Y - 1024f)
			{
				int topBarrierWidth = this.level.GenerationParams.WallEdgeSprite.Texture.Width;
				int topBarrierHeight = this.level.GenerationParams.WallEdgeSprite.Texture.Height;
				pos.X = (float)(cam.WorldView.X - topBarrierWidth);
				int width = (int)(Math.Ceiling((double)((float)(cam.WorldView.Width / 1024) + 4f)) * (double)topBarrierWidth);
				GUI.DrawRectangle(spriteBatch, new Rectangle((int)MathUtils.Round(pos.X, (float)topBarrierWidth), -cam.WorldView.Y, width, (int)((float)cam.WorldView.Y + pos.Y) - 60), Color.Black, true, 0f, 1f);
				Texture2D texture = this.level.GenerationParams.WallEdgeSprite.Texture;
				Rectangle destinationRectangle = new Rectangle((int)MathUtils.Round(pos.X, (float)topBarrierWidth), (int)(pos.Y - (float)topBarrierHeight + this.level.GenerationParams.WallEdgeExpandOutwardsAmount), width, topBarrierHeight);
				Rectangle? sourceRectangle = new Rectangle?(new Rectangle(0, 0, width, -topBarrierHeight));
				LightManager lightManager = GameMain.LightManager;
				spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, (lightManager != null && lightManager.LightingEnabled) ? GameMain.LightManager.AmbientLight : this.level.WallColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
			if (cam.WorldView.Y - cam.WorldView.Height < this.level.SeaFloorTopPos + 1024)
			{
				int bottomBarrierWidth = this.level.GenerationParams.WallEdgeSprite.Texture.Width;
				int bottomBarrierHeight = this.level.GenerationParams.WallEdgeSprite.Texture.Height;
				pos = new Vector2((float)(cam.WorldView.X - bottomBarrierWidth), (float)(-(float)this.level.BottomPos));
				int width2 = (int)(Math.Ceiling((double)((float)(cam.WorldView.Width / bottomBarrierWidth) + 4f)) * (double)bottomBarrierWidth);
				GUI.DrawRectangle(spriteBatch, new Rectangle((int)MathUtils.Round(pos.X, (float)bottomBarrierWidth), -(this.level.BottomPos - 60), width2, this.level.BottomPos - (cam.WorldView.Y - cam.WorldView.Height)), Color.Black, true, 0f, 1f);
				Texture2D texture2 = this.level.GenerationParams.WallEdgeSprite.Texture;
				Rectangle destinationRectangle2 = new Rectangle((int)MathUtils.Round(pos.X, (float)bottomBarrierWidth), -this.level.BottomPos - (int)this.level.GenerationParams.WallEdgeExpandOutwardsAmount, width2, bottomBarrierHeight);
				Rectangle? sourceRectangle2 = new Rectangle?(new Rectangle(0, 0, width2, -bottomBarrierHeight));
				LightManager lightManager2 = GameMain.LightManager;
				spriteBatch.Draw(texture2, destinationRectangle2, sourceRectangle2, (lightManager2 != null && lightManager2.LightingEnabled) ? GameMain.LightManager.AmbientLight : this.level.WallColor, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
			}
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x001334B0 File Offset: 0x001316B0
		public void RenderWalls(GraphicsDevice graphicsDevice, Camera cam)
		{
			if (!this.vertexBuffers.Any<LevelWallVertexBuffer>())
			{
				return;
			}
			RasterizerState defaultRasterizerState = graphicsDevice.RasterizerState;
			Matrix transformMatrix = cam.ShaderTransform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 100f) * 0.5f;
			graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			graphicsDevice.RasterizerState = this.cullNone;
			for (int i = 0; i < 2; i++)
			{
				List<LevelWall> wallList = (i == 0) ? this.level.ExtraWalls : this.level.UnsyncedExtraWalls;
				foreach (LevelWall wall in wallList)
				{
					DestructibleLevelWall destructibleWall = wall as DestructibleLevelWall;
					if (destructibleWall != null && !destructibleWall.Destroyed && wall.IsVisible(cam.WorldView))
					{
						BasicEffect basicEffect = LevelRenderer.wallCenterEffect;
						Sprite destructibleWallSprite = this.level.GenerationParams.DestructibleWallSprite;
						basicEffect.Texture = (((destructibleWallSprite != null) ? destructibleWallSprite.Texture : null) ?? this.level.GenerationParams.WallSprite.Texture);
						LevelRenderer.wallCenterEffect.World = wall.GetTransform() * transformMatrix;
						LevelRenderer.wallCenterEffect.Alpha = wall.Alpha;
						LevelRenderer.wallCenterEffect.CurrentTechnique.Passes[0].Apply();
						graphicsDevice.SetVertexBuffer(wall.WallBuffer);
						graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)wall.WallBuffer.VertexCount / 3f)));
						if (destructibleWall.Damage > 0f)
						{
							LevelRenderer.wallCenterEffect.Texture = this.level.GenerationParams.WallSpriteDestroyed.Texture;
							LevelRenderer.wallCenterEffect.Alpha = MathHelper.Lerp(0.2f, 1f, destructibleWall.Damage / destructibleWall.MaxHealth) * wall.Alpha;
							LevelRenderer.wallCenterEffect.CurrentTechnique.Passes[0].Apply();
							graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)wall.WallEdgeBuffer.VertexCount / 3f)));
						}
						BasicEffect basicEffect2 = LevelRenderer.wallEdgeEffect;
						Sprite destructibleWallEdgeSprite = this.level.GenerationParams.DestructibleWallEdgeSprite;
						basicEffect2.Texture = (((destructibleWallEdgeSprite != null) ? destructibleWallEdgeSprite.Texture : null) ?? this.level.GenerationParams.WallEdgeSprite.Texture);
						LevelRenderer.wallEdgeEffect.World = wall.GetTransform() * transformMatrix;
						LevelRenderer.wallEdgeEffect.Alpha = wall.Alpha;
						LevelRenderer.wallEdgeEffect.CurrentTechnique.Passes[0].Apply();
						graphicsDevice.SetVertexBuffer(wall.WallEdgeBuffer);
						graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)wall.WallEdgeBuffer.VertexCount / 3f)));
					}
				}
			}
			LevelRenderer.wallEdgeEffect.Alpha = (LevelRenderer.wallInnerEffect.Alpha = (LevelRenderer.wallCenterEffect.Alpha = 1f));
			LevelRenderer.wallCenterEffect.World = (LevelRenderer.wallInnerEffect.World = (LevelRenderer.wallEdgeEffect.World = transformMatrix));
			foreach (LevelWallVertexBuffer vertexBuffer in this.vertexBuffers)
			{
				LevelRenderer.wallInnerEffect.CurrentTechnique.Passes[0].Apply();
				graphicsDevice.SetVertexBuffer(vertexBuffer.WallInnerBuffer);
				graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)vertexBuffer.WallInnerBuffer.VertexCount / 3f)));
				LevelRenderer.wallCenterEffect.Texture = vertexBuffer.WallTexture;
				LevelRenderer.wallCenterEffect.CurrentTechnique.Passes[0].Apply();
				graphicsDevice.SetVertexBuffer(vertexBuffer.WallBuffer);
				graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)vertexBuffer.WallBuffer.VertexCount / 3f)));
				LevelRenderer.wallEdgeEffect.Texture = vertexBuffer.EdgeTexture;
				LevelRenderer.wallEdgeEffect.CurrentTechnique.Passes[0].Apply();
				graphicsDevice.SetVertexBuffer(vertexBuffer.WallEdgeBuffer);
				graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)vertexBuffer.WallEdgeBuffer.VertexCount / 3f)));
			}
			LevelRenderer.wallCenterEffect.Texture = this.level.GenerationParams.WallSprite.Texture;
			LevelRenderer.wallEdgeEffect.Texture = this.level.GenerationParams.WallEdgeSprite.Texture;
			for (int j = 0; j < 2; j++)
			{
				List<LevelWall> wallList2 = (j == 0) ? this.level.ExtraWalls : this.level.UnsyncedExtraWalls;
				foreach (LevelWall wall2 in wallList2)
				{
					if (!(wall2 is DestructibleLevelWall) && wall2.IsVisible(cam.WorldView))
					{
						LevelRenderer.wallCenterEffect.World = wall2.GetTransform() * transformMatrix;
						LevelRenderer.wallCenterEffect.Alpha = wall2.Alpha;
						LevelRenderer.wallCenterEffect.CurrentTechnique.Passes[0].Apply();
						graphicsDevice.SetVertexBuffer(wall2.WallBuffer);
						graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)wall2.WallBuffer.VertexCount / 3f)));
						LevelRenderer.wallEdgeEffect.World = wall2.GetTransform() * transformMatrix;
						LevelRenderer.wallEdgeEffect.Alpha = wall2.Alpha;
						LevelRenderer.wallEdgeEffect.CurrentTechnique.Passes[0].Apply();
						graphicsDevice.SetVertexBuffer(wall2.WallEdgeBuffer);
						graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (int)Math.Floor((double)((float)wall2.WallEdgeBuffer.VertexCount / 3f)));
					}
				}
			}
			graphicsDevice.RasterizerState = defaultRasterizerState;
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x00133B2C File Offset: 0x00131D2C
		public void Dispose()
		{
			foreach (LevelWallVertexBuffer vertexBuffer in this.vertexBuffers)
			{
				vertexBuffer.Dispose();
			}
			this.vertexBuffers.Clear();
		}

		// Token: 0x04000FE3 RID: 4067
		private static BasicEffect wallEdgeEffect;

		// Token: 0x04000FE4 RID: 4068
		private static BasicEffect wallCenterEffect;

		// Token: 0x04000FE5 RID: 4069
		private static BasicEffect wallInnerEffect;

		// Token: 0x04000FE6 RID: 4070
		private Vector2 waterParticleOffset;

		// Token: 0x04000FE7 RID: 4071
		private Vector2 waterParticleVelocity;

		// Token: 0x04000FE8 RID: 4072
		private float flashCooldown;

		// Token: 0x04000FE9 RID: 4073
		private float flashTimer;

		// Token: 0x04000FEB RID: 4075
		private readonly RasterizerState cullNone;

		// Token: 0x04000FEC RID: 4076
		private readonly Level level;

		// Token: 0x04000FED RID: 4077
		private readonly List<LevelWallVertexBuffer> vertexBuffers = new List<LevelWallVertexBuffer>();

		// Token: 0x04000FEE RID: 4078
		private float chromaticAberrationStrength;
	}
}
