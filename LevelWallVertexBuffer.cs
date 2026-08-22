using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000DF RID: 223
	internal class LevelWallVertexBuffer : IDisposable
	{
		// Token: 0x1700085B RID: 2139
		// (get) Token: 0x06001F0B RID: 7947 RVA: 0x0013246E File Offset: 0x0013066E
		// (set) Token: 0x06001F0C RID: 7948 RVA: 0x00132476 File Offset: 0x00130676
		public bool IsDisposed { get; private set; }

		// Token: 0x06001F0D RID: 7949 RVA: 0x00132480 File Offset: 0x00130680
		public LevelWallVertexBuffer(VertexPositionColorTexture[] wallVertices, VertexPositionColorTexture[] wallEdgeVertices, VertexPositionColor[] wallInnerVertices, Texture2D wallTexture, Texture2D edgeTexture)
		{
			if (wallVertices.Length == 0)
			{
				throw new ArgumentException("Failed to instantiate a LevelWallVertexBuffer (no wall vertices).");
			}
			if (wallVertices.Length == 0)
			{
				throw new ArgumentException("Failed to instantiate a LevelWallVertexBuffer (no wall edge vertices).");
			}
			this.wallVertices = wallVertices;
			this.WallBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, wallVertices.Length, BufferUsage.WriteOnly);
			this.WallBuffer.SetData<VertexPositionColorTexture>(this.wallVertices);
			this.WallTexture = wallTexture;
			this.wallEdgeVertices = wallEdgeVertices;
			this.WallEdgeBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, wallEdgeVertices.Length, BufferUsage.WriteOnly);
			this.WallEdgeBuffer.SetData<VertexPositionColorTexture>(this.wallEdgeVertices);
			this.EdgeTexture = edgeTexture;
			if (wallInnerVertices != null)
			{
				this.wallInnerVertices = wallInnerVertices;
				this.WallInnerBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColor.VertexDeclaration, wallInnerVertices.Length, BufferUsage.WriteOnly);
				this.WallInnerBuffer.SetData<VertexPositionColor>(this.wallInnerVertices);
			}
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x00132568 File Offset: 0x00130768
		public void Append(VertexPositionColorTexture[] newWallVertices, VertexPositionColorTexture[] newWallEdgeVertices, VertexPositionColor[] newWallInnerVertices)
		{
			this.WallBuffer = LevelWallVertexBuffer.<Append>g__Append|13_0<VertexPositionColorTexture>(this.WallBuffer, ref this.wallVertices, newWallVertices, VertexPositionColorTexture.VertexDeclaration);
			this.WallEdgeBuffer = LevelWallVertexBuffer.<Append>g__Append|13_0<VertexPositionColorTexture>(this.WallEdgeBuffer, ref this.wallEdgeVertices, newWallEdgeVertices, VertexPositionColorTexture.VertexDeclaration);
			this.WallInnerBuffer = LevelWallVertexBuffer.<Append>g__Append|13_0<VertexPositionColor>(this.WallInnerBuffer, ref this.wallInnerVertices, newWallInnerVertices, VertexPositionColor.VertexDeclaration);
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x001325CC File Offset: 0x001307CC
		public void Dispose()
		{
			this.IsDisposed = true;
			VertexBuffer wallEdgeBuffer = this.WallEdgeBuffer;
			if (wallEdgeBuffer != null)
			{
				wallEdgeBuffer.Dispose();
			}
			VertexBuffer wallBuffer = this.WallBuffer;
			if (wallBuffer == null)
			{
				return;
			}
			wallBuffer.Dispose();
		}

		// Token: 0x06001F10 RID: 7952 RVA: 0x001325F8 File Offset: 0x001307F8
		[CompilerGenerated]
		internal static VertexBuffer <Append>g__Append|13_0<T>(VertexBuffer buffer, ref T[] currentVertices, T[] newVertices, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
		{
			if (buffer != null)
			{
				buffer.Dispose();
			}
			int originalVertexCount = currentVertices.Length;
			int newBufferSize = originalVertexCount + newVertices.Length;
			buffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, vertexDeclaration, newBufferSize, BufferUsage.WriteOnly);
			Array.Resize<T>(ref currentVertices, newBufferSize);
			Array.Copy(newVertices, 0, currentVertices, originalVertexCount, newVertices.Length);
			buffer.SetData<T>(currentVertices);
			return buffer;
		}

		// Token: 0x04000FDA RID: 4058
		public VertexBuffer WallBuffer;

		// Token: 0x04000FDB RID: 4059
		public VertexBuffer WallEdgeBuffer;

		// Token: 0x04000FDC RID: 4060
		public VertexBuffer WallInnerBuffer;

		// Token: 0x04000FDD RID: 4061
		public readonly Texture2D WallTexture;

		// Token: 0x04000FDE RID: 4062
		public readonly Texture2D EdgeTexture;

		// Token: 0x04000FDF RID: 4063
		private VertexPositionColorTexture[] wallVertices;

		// Token: 0x04000FE0 RID: 4064
		private VertexPositionColorTexture[] wallEdgeVertices;

		// Token: 0x04000FE1 RID: 4065
		private VertexPositionColor[] wallInnerVertices;
	}
}
