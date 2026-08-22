using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000150 RID: 336
	internal static class GraphicsQuad
	{
		// Token: 0x06002A0F RID: 10767 RVA: 0x001D14A4 File Offset: 0x001CF6A4
		public static void Init(GraphicsDevice graphics)
		{
			if (GraphicsQuad.graphicsDevice != null)
			{
				return;
			}
			GraphicsQuad.graphicsDevice = graphics;
			GraphicsQuad.vertexBuffer = new VertexBuffer(graphics, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
			GraphicsQuad.indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 4, BufferUsage.WriteOnly);
			GraphicsQuad.InitVertexData();
			GraphicsQuad.indexBuffer.SetData<ushort>(new ushort[]
			{
				0,
				1,
				2,
				3
			});
			GraphicsQuad.basicEffect = new BasicEffect(graphics)
			{
				TextureEnabled = true
			};
			GameMain.Instance.ResolutionChanged += delegate()
			{
				GraphicsQuad.InitVertexData();
			};
		}

		// Token: 0x06002A10 RID: 10768 RVA: 0x001D153C File Offset: 0x001CF73C
		private static void InitVertexData()
		{
			Vector2 halfPixelOffset = Vector2.Zero;
			VertexPositionTexture[] vertices = new VertexPositionTexture[]
			{
				new VertexPositionTexture(new Vector3(-1f, -1f, 1f), new Vector2(0f, 1f) + halfPixelOffset),
				new VertexPositionTexture(new Vector3(-1f, 1f, 1f), new Vector2(0f, 0f) + halfPixelOffset),
				new VertexPositionTexture(new Vector3(1f, -1f, 1f), new Vector2(1f, 1f) + halfPixelOffset),
				new VertexPositionTexture(new Vector3(1f, 1f, 1f), new Vector2(1f, 0f) + halfPixelOffset)
			};
			GraphicsQuad.vertexBuffer.SetData<VertexPositionTexture>(vertices);
		}

		// Token: 0x06002A11 RID: 10769 RVA: 0x001D1635 File Offset: 0x001CF835
		public static void UseBasicEffect(Texture2D texture)
		{
			GraphicsQuad.basicEffect.Texture = texture;
			GraphicsQuad.basicEffect.CurrentTechnique.Passes[0].Apply();
		}

		// Token: 0x06002A12 RID: 10770 RVA: 0x001D165C File Offset: 0x001CF85C
		public static void Render()
		{
			GraphicsQuad.graphicsDevice.SetVertexBuffer(GraphicsQuad.vertexBuffer);
			GraphicsQuad.graphicsDevice.Indices = GraphicsQuad.indexBuffer;
			GraphicsQuad.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2);
		}

		// Token: 0x040015FE RID: 5630
		private static VertexBuffer vertexBuffer;

		// Token: 0x040015FF RID: 5631
		private static IndexBuffer indexBuffer;

		// Token: 0x04001600 RID: 5632
		private static BasicEffect basicEffect;

		// Token: 0x04001601 RID: 5633
		private static GraphicsDevice graphicsDevice;
	}
}
