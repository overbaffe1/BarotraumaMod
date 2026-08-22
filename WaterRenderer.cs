using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000E3 RID: 227
	internal class WaterRenderer : IDisposable
	{
		// Token: 0x17000869 RID: 2153
		// (get) Token: 0x06001F3E RID: 7998 RVA: 0x0013435D File Offset: 0x0013255D
		// (set) Token: 0x06001F3F RID: 7999 RVA: 0x00134365 File Offset: 0x00132565
		public Vector2 WavePos { get; private set; }

		// Token: 0x1700086A RID: 2154
		// (get) Token: 0x06001F40 RID: 8000 RVA: 0x0013436E File Offset: 0x0013256E
		// (set) Token: 0x06001F41 RID: 8001 RVA: 0x00134376 File Offset: 0x00132576
		public Effect WaterEffect { get; private set; }

		// Token: 0x1700086B RID: 2155
		// (get) Token: 0x06001F42 RID: 8002 RVA: 0x0013437F File Offset: 0x0013257F
		public Texture2D WaterTexture { get; }

		// Token: 0x06001F43 RID: 8003 RVA: 0x00134388 File Offset: 0x00132588
		public WaterRenderer(GraphicsDevice graphicsDevice)
		{
			this.WaterEffect = EffectLoader.Load("Effects/watershader");
			this.WaterTexture = TextureLoader.FromFile("Content/Effects/waterbump.png", true, false, null);
			this.WaterEffect.Parameters["xWaterBumpMap"].SetValue(this.WaterTexture);
			this.WaterEffect.Parameters["waterColor"].SetValue(this.waterColor.ToVector4());
			if (this.basicEffect == null)
			{
				this.basicEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					VertexColorEnabled = false,
					TextureEnabled = true
				};
			}
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x001344F4 File Offset: 0x001326F4
		public void RenderWater(SpriteBatch spriteBatch, RenderTarget2D texture, Camera cam)
		{
			spriteBatch.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
			this.WaterEffect.Parameters["xTexture"].SetValue(texture);
			Vector2 distortionStrength = (cam == null) ? WaterRenderer.DistortionStrength : (WaterRenderer.DistortionStrength * cam.Zoom);
			this.WaterEffect.Parameters["xWaveWidth"].SetValue(distortionStrength.X);
			this.WaterEffect.Parameters["xWaveHeight"].SetValue(distortionStrength.Y);
			if (WaterRenderer.BlurAmount > 0f)
			{
				this.WaterEffect.CurrentTechnique = this.WaterEffect.Techniques["WaterShaderBlurred"];
				this.WaterEffect.Parameters["xBlurDistance"].SetValue(WaterRenderer.BlurAmount / 100f);
			}
			else
			{
				this.WaterEffect.CurrentTechnique = this.WaterEffect.Techniques["WaterShader"];
			}
			Vector2 offset = this.WavePos;
			if (cam != null)
			{
				offset += cam.Position - new Vector2((float)cam.WorldView.Width / 2f, (float)(-(float)cam.WorldView.Height) / 2f);
				offset.Y += (float)cam.WorldView.Height;
				offset.X += (float)cam.WorldView.Width;
				offset *= WaterRenderer.DistortionScale;
			}
			offset.Y = -offset.Y;
			this.WaterEffect.Parameters["xUvOffset"].SetValue(new Vector2(offset.X / (float)GameMain.GraphicsWidth % 1f, offset.Y / (float)GameMain.GraphicsHeight % 1f));
			this.WaterEffect.Parameters["xBumpPos"].SetValue(Vector2.Zero);
			if (cam != null)
			{
				this.WaterEffect.Parameters["xBumpScale"].SetValue(new Vector2((float)cam.WorldView.Width / (float)GameMain.GraphicsWidth * WaterRenderer.DistortionScale.X, (float)cam.WorldView.Height / (float)GameMain.GraphicsHeight * WaterRenderer.DistortionScale.Y));
				this.WaterEffect.Parameters["xTransform"].SetValue(cam.ShaderTransform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 1f) * 0.5f);
				this.WaterEffect.Parameters["xUvTransform"].SetValue(cam.ShaderTransform * Matrix.CreateOrthographicOffCenter(0f, (float)(spriteBatch.GraphicsDevice.Viewport.Width * 2), (float)(spriteBatch.GraphicsDevice.Viewport.Height * 2), 0f, 0f, 1f) * Matrix.CreateTranslation(0.5f, 0.5f, 0f));
			}
			else
			{
				this.WaterEffect.Parameters["xBumpScale"].SetValue(new Vector2(1f, 1f));
				this.WaterEffect.Parameters["xTransform"].SetValue(Matrix.Identity * Matrix.CreateTranslation(-1f, 1f, 0f));
				this.WaterEffect.Parameters["xUvTransform"].SetValue(Matrix.CreateScale(0.5f, -0.5f, 0f));
			}
			this.WaterEffect.CurrentTechnique.Passes[0].Apply();
			Rectangle view = (cam != null) ? cam.WorldView : spriteBatch.GraphicsDevice.Viewport.Bounds;
			this.tempCorners[0] = new Vector3((float)view.X, (float)view.Y, 0.1f);
			this.tempCorners[1] = new Vector3((float)view.Right, (float)view.Y, 0.1f);
			this.tempCorners[2] = new Vector3((float)view.Right, (float)(view.Y - view.Height), 0.1f);
			this.tempCorners[3] = new Vector3((float)view.X, (float)(view.Y - view.Height), 0.1f);
			WaterVertexData backGroundColor = new WaterVertexData(0.1f, 0.1f, 0.5f, 1f);
			this.tempVertices[0] = new VertexPositionColorTexture(this.tempCorners[0], backGroundColor, Vector2.Zero);
			this.tempVertices[1] = new VertexPositionColorTexture(this.tempCorners[1], backGroundColor, Vector2.Zero);
			this.tempVertices[2] = new VertexPositionColorTexture(this.tempCorners[2], backGroundColor, Vector2.Zero);
			this.tempVertices[3] = new VertexPositionColorTexture(this.tempCorners[0], backGroundColor, Vector2.Zero);
			this.tempVertices[4] = new VertexPositionColorTexture(this.tempCorners[2], backGroundColor, Vector2.Zero);
			this.tempVertices[5] = new VertexPositionColorTexture(this.tempCorners[3], backGroundColor, Vector2.Zero);
			spriteBatch.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, this.tempVertices, 0, 2);
			foreach (KeyValuePair<EntityGrid, VertexPositionColorTexture[]> subVerts in this.IndoorsVertices)
			{
				if (this.PositionInIndoorsBuffer.ContainsKey(subVerts.Key) && this.PositionInIndoorsBuffer[subVerts.Key] != 0)
				{
					offset = this.WavePos;
					if (subVerts.Key.Submarine != null)
					{
						offset -= subVerts.Key.Submarine.WorldPosition;
					}
					if (cam != null)
					{
						offset += cam.Position - new Vector2((float)cam.WorldView.Width / 2f, (float)(-(float)cam.WorldView.Height) / 2f);
						offset.Y += (float)cam.WorldView.Height;
						offset.X += (float)cam.WorldView.Width;
						offset *= WaterRenderer.DistortionScale;
					}
					offset.Y = -offset.Y;
					this.WaterEffect.Parameters["xUvOffset"].SetValue(new Vector2(offset.X / (float)GameMain.GraphicsWidth % 1f, offset.Y / (float)GameMain.GraphicsHeight % 1f));
					this.WaterEffect.CurrentTechnique.Passes[0].Apply();
					spriteBatch.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, subVerts.Value, 0, this.PositionInIndoorsBuffer[subVerts.Key] / 3);
				}
			}
			this.WaterEffect.Parameters["xTexture"].SetValue(null);
			this.WaterEffect.CurrentTechnique.Passes[0].Apply();
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x00134CA0 File Offset: 0x00132EA0
		public void ScrollWater(Vector2 vel, float deltaTime)
		{
			this.WavePos -= vel * deltaTime;
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x00134CBC File Offset: 0x00132EBC
		public void RenderAir(GraphicsDevice graphicsDevice, Camera cam, RenderTarget2D texture, Matrix transform)
		{
			if (this.vertices == null || this.vertices.Length < 0 || this.PositionInBuffer <= 0)
			{
				return;
			}
			this.basicEffect.Texture = texture;
			this.basicEffect.View = Matrix.Identity;
			this.basicEffect.World = transform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 1f) * 0.5f * Matrix.CreateTranslation(0f, 0f, 0f);
			this.basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, this.vertices, 0, this.PositionInBuffer / 3);
			this.basicEffect.Texture = null;
			this.basicEffect.CurrentTechnique.Passes[0].Apply();
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x00134DBC File Offset: 0x00132FBC
		public void ResetBuffers()
		{
			this.PositionInBuffer = 0;
			this.PositionInIndoorsBuffer.Clear();
			this.buffersToRemove.Clear();
			foreach (EntityGrid buffer in this.IndoorsVertices.Keys)
			{
				Submarine submarine = buffer.Submarine;
				if (submarine != null && submarine.Removed)
				{
					this.buffersToRemove.Add(buffer);
				}
			}
			foreach (EntityGrid bufferToRemove in this.buffersToRemove)
			{
				this.IndoorsVertices.Remove(bufferToRemove);
			}
		}

		// Token: 0x06001F48 RID: 8008 RVA: 0x00134E94 File Offset: 0x00133094
		public void Dispose()
		{
			if (this.WaterEffect != null)
			{
				this.WaterEffect.Dispose();
				this.WaterEffect = null;
			}
			if (this.basicEffect != null)
			{
				this.basicEffect.Dispose();
				this.basicEffect = null;
			}
		}

		// Token: 0x04001001 RID: 4097
		public static WaterRenderer Instance;

		// Token: 0x04001002 RID: 4098
		public const int DefaultBufferSize = 2000;

		// Token: 0x04001003 RID: 4099
		public const int DefaultIndoorsBufferSize = 3000;

		// Token: 0x04001004 RID: 4100
		public static Vector2 DistortionScale = new Vector2(2f, 1.5f);

		// Token: 0x04001005 RID: 4101
		public static Vector2 DistortionStrength = new Vector2(0.01f, 0.33f);

		// Token: 0x04001006 RID: 4102
		public static float BlurAmount = 0f;

		// Token: 0x04001008 RID: 4104
		public readonly Color waterColor = new Color(0.375f, 0.4f, 0.45f, 1f);

		// Token: 0x04001009 RID: 4105
		public readonly WaterVertexData IndoorsWaterColor = new WaterVertexData(0.1f, 0.1f, 0.5f, 1f);

		// Token: 0x0400100A RID: 4106
		public readonly WaterVertexData IndoorsSurfaceTopColor = new WaterVertexData(0.5f, 0.5f, 0f, 1f);

		// Token: 0x0400100B RID: 4107
		public readonly WaterVertexData IndoorsSurfaceBottomColor = new WaterVertexData(0.2f, 0.1f, 0.9f, 1f);

		// Token: 0x0400100C RID: 4108
		public VertexPositionTexture[] vertices = new VertexPositionTexture[2000];

		// Token: 0x0400100D RID: 4109
		public Dictionary<EntityGrid, VertexPositionColorTexture[]> IndoorsVertices = new Dictionary<EntityGrid, VertexPositionColorTexture[]>();

		// Token: 0x0400100F RID: 4111
		private BasicEffect basicEffect;

		// Token: 0x04001010 RID: 4112
		public int PositionInBuffer;

		// Token: 0x04001011 RID: 4113
		public Dictionary<EntityGrid, int> PositionInIndoorsBuffer = new Dictionary<EntityGrid, int>();

		// Token: 0x04001013 RID: 4115
		private readonly VertexPositionColorTexture[] tempVertices = new VertexPositionColorTexture[6];

		// Token: 0x04001014 RID: 4116
		private readonly Vector3[] tempCorners = new Vector3[4];

		// Token: 0x04001015 RID: 4117
		private readonly List<EntityGrid> buffersToRemove = new List<EntityGrid>();
	}
}
