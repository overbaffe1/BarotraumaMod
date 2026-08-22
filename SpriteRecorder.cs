using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000157 RID: 343
	internal sealed class SpriteRecorder : ISpriteBatch, IDisposable
	{
		// Token: 0x17000AB9 RID: 2745
		// (get) Token: 0x06002A30 RID: 10800 RVA: 0x001D285C File Offset: 0x001D0A5C
		// (set) Token: 0x06002A31 RID: 10801 RVA: 0x001D2864 File Offset: 0x001D0A64
		public Vector2 Min { get; private set; }

		// Token: 0x17000ABA RID: 2746
		// (get) Token: 0x06002A32 RID: 10802 RVA: 0x001D286D File Offset: 0x001D0A6D
		// (set) Token: 0x06002A33 RID: 10803 RVA: 0x001D2875 File Offset: 0x001D0A75
		public Vector2 Max { get; private set; }

		// Token: 0x06002A34 RID: 10804 RVA: 0x001D287E File Offset: 0x001D0A7E
		public void Begin(SpriteSortMode sortMode)
		{
			this.ReadyToRender = false;
			this.currentSortMode = sortMode;
		}

		// Token: 0x06002A35 RID: 10805 RVA: 0x001D2890 File Offset: 0x001D0A90
		private void AppendCommand(SpriteRecorder.Command command)
		{
			if (this.isDisposed)
			{
				return;
			}
			if (this.commandList.Count == 0)
			{
				this.Min = command.Min;
				this.Max = command.Max;
			}
			this.Min = new Vector2(Math.Min(command.Min.X, this.Min.X), Math.Min(command.Min.Y, this.Min.Y));
			this.Max = new Vector2(Math.Max(command.Max.X, this.Max.X), Math.Max(command.Max.Y, this.Max.Y));
			this.commandList.Add(command);
		}

		// Token: 0x06002A36 RID: 10806 RVA: 0x001D2964 File Offset: 0x001D0B64
		public void Draw(Texture2D texture, Vector2 pos, Rectangle? srcRect, Color color, float rotationRad, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth)
		{
			if (this.isDisposed)
			{
				return;
			}
			SpriteRecorder.Command command = SpriteRecorder.Command.FromTransform(texture, pos, srcRect ?? texture.Bounds, color, rotationRad, origin, scale, effects, depth, this.commandList.Count);
			this.AppendCommand(command);
		}

		// Token: 0x06002A37 RID: 10807 RVA: 0x001D29BC File Offset: 0x001D0BBC
		public void Draw(Texture2D texture, VertexPositionColorTexture[] vertices, float layerDepth, int? count = null)
		{
			if (this.isDisposed)
			{
				return;
			}
			int iters = count ?? (vertices.Length / 4);
			for (int i = 0; i < iters; i++)
			{
				VertexPositionColorTexture[] subset = RuntimeHelpers.GetSubArray<VertexPositionColorTexture>(vertices, new Range(i * 4, i * 4 + 4));
				SpriteRecorder.Command command = new SpriteRecorder.Command(texture, subset[2], subset[3], subset[0], subset[1], layerDepth, SpriteRecorder.Command.GetMinPosition(subset), SpriteRecorder.Command.GetMaxPosition(subset), this.commandList.Count);
				this.AppendCommand(command);
			}
		}

		// Token: 0x06002A38 RID: 10808 RVA: 0x001D2A60 File Offset: 0x001D0C60
		public void End()
		{
			if (this.isDisposed)
			{
				return;
			}
			SpriteSortMode spriteSortMode = this.currentSortMode;
			if (spriteSortMode != SpriteSortMode.BackToFront)
			{
				if (spriteSortMode == SpriteSortMode.FrontToBack)
				{
					this.commandList.Sort(delegate(SpriteRecorder.Command c1, SpriteRecorder.Command c2)
					{
						if (c1.Depth < c2.Depth)
						{
							return -1;
						}
						if (c1.Depth > c2.Depth)
						{
							return 1;
						}
						if (c1.Index < c2.Index)
						{
							return 1;
						}
						if (c1.Index <= c2.Index)
						{
							return 0;
						}
						return -1;
					});
				}
			}
			else
			{
				this.commandList.Sort(delegate(SpriteRecorder.Command c1, SpriteRecorder.Command c2)
				{
					if (c1.Depth < c2.Depth)
					{
						return 1;
					}
					if (c1.Depth > c2.Depth)
					{
						return -1;
					}
					if (c1.Index < c2.Index)
					{
						return 1;
					}
					if (c1.Index <= c2.Index)
					{
						return 0;
					}
					return -1;
				});
			}
			for (int i = 1; i < this.commandList.Count; i++)
			{
				if (this.commandList[i].Texture != this.commandList[i - 1].Texture)
				{
					for (int j = i - 1; j >= 0; j--)
					{
						if (this.commandList[j].Texture == this.commandList[i].Texture)
						{
							this.commandList.SiftElement(i, j + 1);
							break;
						}
						if (this.commandList[j].Overlaps(this.commandList[i]))
						{
							break;
						}
					}
				}
			}
			if (this.isDisposed)
			{
				return;
			}
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				if (this.isDisposed)
				{
					return;
				}
				if (this.commandList.Count == 0)
				{
					return;
				}
				int startIndex = 0;
				for (int k = 1; k < this.commandList.Count; k++)
				{
					if (this.commandList[k].Texture != this.commandList[startIndex].Texture)
					{
						this.maxSpriteCount = Math.Max(this.maxSpriteCount, k - startIndex);
						this.recordedBuffers.Add(new SpriteRecorder.RecordedBuffer(this.commandList, startIndex, k - startIndex));
						startIndex = k;
					}
				}
				this.recordedBuffers.Add(new SpriteRecorder.RecordedBuffer(this.commandList, startIndex, this.commandList.Count - startIndex));
				this.maxSpriteCount = Math.Max(this.maxSpriteCount, this.commandList.Count - startIndex);
			});
			this.commandList.Clear();
			this.ReadyToRender = true;
		}

		// Token: 0x06002A39 RID: 10809 RVA: 0x001D2BC0 File Offset: 0x001D0DC0
		public void Render(Camera cam)
		{
			if (!this.ReadyToRender)
			{
				return;
			}
			GraphicsDevice gfxDevice = GameMain.Instance.GraphicsDevice;
			if (SpriteRecorder.BasicEffect == null)
			{
				SpriteRecorder.BasicEffect = new BasicEffect(gfxDevice);
			}
			SpriteRecorder.BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(new Rectangle(0, 0, cam.Resolution.X, cam.Resolution.Y), -1f, 1f);
			SpriteRecorder.BasicEffect.View = cam.Transform;
			SpriteRecorder.BasicEffect.World = Matrix.Identity;
			SpriteRecorder.BasicEffect.TextureEnabled = true;
			SpriteRecorder.BasicEffect.VertexColorEnabled = true;
			SpriteRecorder.BasicEffect.Alpha = 1f;
			int requiredIndexCount = this.maxSpriteCount * 6;
			if (requiredIndexCount > 0 && (this.indexBuffer == null || this.indexBuffer.IndexCount < requiredIndexCount))
			{
				IndexBuffer indexBuffer = this.indexBuffer;
				if (indexBuffer != null)
				{
					indexBuffer.Dispose();
				}
				this.indexBuffer = new IndexBuffer(gfxDevice, IndexElementSize.SixteenBits, requiredIndexCount * 2, BufferUsage.WriteOnly);
				ushort[] indices = new ushort[requiredIndexCount * 2];
				for (int i = 0; i < indices.Length; i += 6)
				{
					indices[i] = (ushort)(i / 6 * 4 + 1);
					indices[i + 1] = (ushort)(i / 6 * 4);
					indices[i + 2] = (ushort)(i / 6 * 4 + 2);
					indices[i + 3] = (ushort)(i / 6 * 4 + 1);
					indices[i + 4] = (ushort)(i / 6 * 4 + 2);
					indices[i + 5] = (ushort)(i / 6 * 4 + 3);
				}
				this.indexBuffer.SetData<ushort>(indices);
			}
			gfxDevice.Indices = this.indexBuffer;
			for (int j = 0; j < this.recordedBuffers.Count; j++)
			{
				gfxDevice.SetVertexBuffer(this.recordedBuffers[j].VertexBuffer);
				SpriteRecorder.BasicEffect.Texture = this.recordedBuffers[j].Texture;
				SpriteRecorder.BasicEffect.CurrentTechnique.Passes[0].Apply();
				gfxDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.recordedBuffers[j].PolyCount);
			}
		}

		// Token: 0x06002A3A RID: 10810 RVA: 0x001D2DB8 File Offset: 0x001D0FB8
		public void Dispose()
		{
			this.isDisposed = true;
			foreach (SpriteRecorder.RecordedBuffer buffer in this.recordedBuffers)
			{
				buffer.VertexBuffer.Dispose();
			}
			this.recordedBuffers.Clear();
			this.commandList.Clear();
			IndexBuffer indexBuffer = this.indexBuffer;
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
			}
			this.indexBuffer = null;
			this.ReadyToRender = false;
		}

		// Token: 0x0400160F RID: 5647
		public static BasicEffect BasicEffect;

		// Token: 0x04001610 RID: 5648
		private readonly List<SpriteRecorder.RecordedBuffer> recordedBuffers = new List<SpriteRecorder.RecordedBuffer>();

		// Token: 0x04001611 RID: 5649
		private readonly List<SpriteRecorder.Command> commandList = new List<SpriteRecorder.Command>();

		// Token: 0x04001612 RID: 5650
		private SpriteSortMode currentSortMode;

		// Token: 0x04001613 RID: 5651
		private IndexBuffer indexBuffer;

		// Token: 0x04001614 RID: 5652
		private int maxSpriteCount;

		// Token: 0x04001615 RID: 5653
		public volatile bool ReadyToRender;

		// Token: 0x04001616 RID: 5654
		private volatile bool isDisposed;

		// Token: 0x02000DCC RID: 3532
		public readonly struct Command : IEquatable<SpriteRecorder.Command>
		{
			// Token: 0x0600826C RID: 33388 RVA: 0x0039A94C File Offset: 0x00398B4C
			public Command(Texture2D Texture, VertexPositionColorTexture VertexBL, VertexPositionColorTexture VertexBR, VertexPositionColorTexture VertexTL, VertexPositionColorTexture VertexTR, float Depth, Vector2 Min, Vector2 Max, int Index)
			{
				this.Texture = Texture;
				this.VertexBL = VertexBL;
				this.VertexBR = VertexBR;
				this.VertexTL = VertexTL;
				this.VertexTR = VertexTR;
				this.Depth = Depth;
				this.Min = Min;
				this.Max = Max;
				this.Index = Index;
			}

			// Token: 0x17001B07 RID: 6919
			// (get) Token: 0x0600826D RID: 33389 RVA: 0x0039A99E File Offset: 0x00398B9E
			// (set) Token: 0x0600826E RID: 33390 RVA: 0x0039A9A6 File Offset: 0x00398BA6
			public Texture2D Texture { get; set; }

			// Token: 0x17001B08 RID: 6920
			// (get) Token: 0x0600826F RID: 33391 RVA: 0x0039A9AF File Offset: 0x00398BAF
			// (set) Token: 0x06008270 RID: 33392 RVA: 0x0039A9B7 File Offset: 0x00398BB7
			public VertexPositionColorTexture VertexBL { get; set; }

			// Token: 0x17001B09 RID: 6921
			// (get) Token: 0x06008271 RID: 33393 RVA: 0x0039A9C0 File Offset: 0x00398BC0
			// (set) Token: 0x06008272 RID: 33394 RVA: 0x0039A9C8 File Offset: 0x00398BC8
			public VertexPositionColorTexture VertexBR { get; set; }

			// Token: 0x17001B0A RID: 6922
			// (get) Token: 0x06008273 RID: 33395 RVA: 0x0039A9D1 File Offset: 0x00398BD1
			// (set) Token: 0x06008274 RID: 33396 RVA: 0x0039A9D9 File Offset: 0x00398BD9
			public VertexPositionColorTexture VertexTL { get; set; }

			// Token: 0x17001B0B RID: 6923
			// (get) Token: 0x06008275 RID: 33397 RVA: 0x0039A9E2 File Offset: 0x00398BE2
			// (set) Token: 0x06008276 RID: 33398 RVA: 0x0039A9EA File Offset: 0x00398BEA
			public VertexPositionColorTexture VertexTR { get; set; }

			// Token: 0x17001B0C RID: 6924
			// (get) Token: 0x06008277 RID: 33399 RVA: 0x0039A9F3 File Offset: 0x00398BF3
			// (set) Token: 0x06008278 RID: 33400 RVA: 0x0039A9FB File Offset: 0x00398BFB
			public float Depth { get; set; }

			// Token: 0x17001B0D RID: 6925
			// (get) Token: 0x06008279 RID: 33401 RVA: 0x0039AA04 File Offset: 0x00398C04
			// (set) Token: 0x0600827A RID: 33402 RVA: 0x0039AA0C File Offset: 0x00398C0C
			public Vector2 Min { get; set; }

			// Token: 0x17001B0E RID: 6926
			// (get) Token: 0x0600827B RID: 33403 RVA: 0x0039AA15 File Offset: 0x00398C15
			// (set) Token: 0x0600827C RID: 33404 RVA: 0x0039AA1D File Offset: 0x00398C1D
			public Vector2 Max { get; set; }

			// Token: 0x17001B0F RID: 6927
			// (get) Token: 0x0600827D RID: 33405 RVA: 0x0039AA26 File Offset: 0x00398C26
			// (set) Token: 0x0600827E RID: 33406 RVA: 0x0039AA2E File Offset: 0x00398C2E
			public int Index { get; set; }

			// Token: 0x0600827F RID: 33407 RVA: 0x0039AA38 File Offset: 0x00398C38
			public static Vector2 GetMinPosition(params VertexPositionColorTexture[] vertices)
			{
				return new Vector2(MathUtils.Min((from v in vertices
				select v.Position.X).ToArray<float>()), MathUtils.Min((from v in vertices
				select v.Position.Y).ToArray<float>()));
			}

			// Token: 0x06008280 RID: 33408 RVA: 0x0039AAA8 File Offset: 0x00398CA8
			public static Vector2 GetMaxPosition(params VertexPositionColorTexture[] vertices)
			{
				return new Vector2(MathUtils.Max((from v in vertices
				select v.Position.X).ToArray<float>()), MathUtils.Max((from v in vertices
				select v.Position.Y).ToArray<float>()));
			}

			// Token: 0x06008281 RID: 33409 RVA: 0x0039AB18 File Offset: 0x00398D18
			public static SpriteRecorder.Command FromTransform(Texture2D texture, Vector2 pos, Rectangle srcRect, Color color, float rotationRad, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth, int index)
			{
				int srcRectLeft = srcRect.Left;
				int srcRectRight = srcRect.Right;
				int srcRectTop = srcRect.Top;
				int srcRectBottom = srcRect.Bottom;
				if (effects.HasFlag(SpriteEffects.FlipHorizontally))
				{
					int num = srcRectLeft;
					srcRectLeft = srcRectRight;
					srcRectRight = num;
				}
				if (effects.HasFlag(SpriteEffects.FlipVertically))
				{
					int num2 = srcRectTop;
					srcRectTop = srcRectBottom;
					srcRectBottom = num2;
				}
				float sin = (float)Math.Sin((double)rotationRad);
				float cos = (float)Math.Cos((double)rotationRad);
				Vector2 size = srcRect.Size.ToVector2() * scale;
				Vector2 wAdd = new Vector2(size.X * cos, size.X * sin);
				Vector2 hAdd = new Vector2(-size.Y * sin, size.Y * cos);
				pos.X -= origin.X * scale.X * cos - origin.Y * scale.Y * sin;
				pos.Y -= origin.Y * scale.Y * cos + origin.X * scale.X * sin;
				VertexPositionColorTexture vertexTl = new VertexPositionColorTexture
				{
					Color = color,
					Position = new Vector3(pos.X, pos.Y, 0f),
					TextureCoordinate = new Vector2((float)srcRectLeft / (float)texture.Width, (float)srcRectTop / (float)texture.Height)
				};
				VertexPositionColorTexture vertexTr = new VertexPositionColorTexture
				{
					Color = color,
					Position = new Vector3(pos.X + wAdd.X, pos.Y + wAdd.Y, 0f),
					TextureCoordinate = new Vector2((float)srcRectRight / (float)texture.Width, (float)srcRectTop / (float)texture.Height)
				};
				VertexPositionColorTexture vertexBl = new VertexPositionColorTexture
				{
					Color = color,
					Position = new Vector3(pos.X + hAdd.X, pos.Y + hAdd.Y, 0f),
					TextureCoordinate = new Vector2((float)srcRectLeft / (float)texture.Width, (float)srcRectBottom / (float)texture.Height)
				};
				VertexPositionColorTexture vertexBr = new VertexPositionColorTexture
				{
					Color = color,
					Position = new Vector3(pos.X + wAdd.X + hAdd.X, pos.Y + wAdd.Y + hAdd.Y, 0f),
					TextureCoordinate = new Vector2((float)srcRectRight / (float)texture.Width, (float)srcRectBottom / (float)texture.Height)
				};
				Vector2 min = SpriteRecorder.Command.GetMinPosition(new VertexPositionColorTexture[]
				{
					vertexTl,
					vertexTr,
					vertexBl,
					vertexBr
				});
				Vector2 max = SpriteRecorder.Command.GetMaxPosition(new VertexPositionColorTexture[]
				{
					vertexTl,
					vertexTr,
					vertexBl,
					vertexBr
				});
				return new SpriteRecorder.Command(texture, vertexBl, vertexBr, vertexTl, vertexTr, depth, min, max, index);
			}

			// Token: 0x06008282 RID: 33410 RVA: 0x0039AE38 File Offset: 0x00399038
			public bool Overlaps(SpriteRecorder.Command other)
			{
				return this.Min.X <= other.Max.X && this.Max.X >= other.Min.X && this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y;
			}

			// Token: 0x06008283 RID: 33411 RVA: 0x0039AEB0 File Offset: 0x003990B0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Command");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06008284 RID: 33412 RVA: 0x0039AEFC File Offset: 0x003990FC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Texture = ");
				builder.Append(this.Texture);
				builder.Append(", VertexBL = ");
				builder.Append(this.VertexBL.ToString());
				builder.Append(", VertexBR = ");
				builder.Append(this.VertexBR.ToString());
				builder.Append(", VertexTL = ");
				builder.Append(this.VertexTL.ToString());
				builder.Append(", VertexTR = ");
				builder.Append(this.VertexTR.ToString());
				builder.Append(", Depth = ");
				builder.Append(this.Depth.ToString());
				builder.Append(", Min = ");
				builder.Append(this.Min.ToString());
				builder.Append(", Max = ");
				builder.Append(this.Max.ToString());
				builder.Append(", Index = ");
				builder.Append(this.Index.ToString());
				return true;
			}

			// Token: 0x06008285 RID: 33413 RVA: 0x0039B05B File Offset: 0x0039925B
			[CompilerGenerated]
			public static bool operator !=(SpriteRecorder.Command left, SpriteRecorder.Command right)
			{
				return !(left == right);
			}

			// Token: 0x06008286 RID: 33414 RVA: 0x0039B067 File Offset: 0x00399267
			[CompilerGenerated]
			public static bool operator ==(SpriteRecorder.Command left, SpriteRecorder.Command right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008287 RID: 33415 RVA: 0x0039B074 File Offset: 0x00399274
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((((((EqualityComparer<Texture2D>.Default.GetHashCode(this.<Texture>k__BackingField) * -1521134295 + EqualityComparer<VertexPositionColorTexture>.Default.GetHashCode(this.<VertexBL>k__BackingField)) * -1521134295 + EqualityComparer<VertexPositionColorTexture>.Default.GetHashCode(this.<VertexBR>k__BackingField)) * -1521134295 + EqualityComparer<VertexPositionColorTexture>.Default.GetHashCode(this.<VertexTL>k__BackingField)) * -1521134295 + EqualityComparer<VertexPositionColorTexture>.Default.GetHashCode(this.<VertexTR>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Depth>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Min>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Max>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Index>k__BackingField);
			}

			// Token: 0x06008288 RID: 33416 RVA: 0x0039B149 File Offset: 0x00399349
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SpriteRecorder.Command && this.Equals((SpriteRecorder.Command)obj);
			}

			// Token: 0x06008289 RID: 33417 RVA: 0x0039B164 File Offset: 0x00399364
			[CompilerGenerated]
			public bool Equals(SpriteRecorder.Command other)
			{
				return EqualityComparer<Texture2D>.Default.Equals(this.<Texture>k__BackingField, other.<Texture>k__BackingField) && EqualityComparer<VertexPositionColorTexture>.Default.Equals(this.<VertexBL>k__BackingField, other.<VertexBL>k__BackingField) && EqualityComparer<VertexPositionColorTexture>.Default.Equals(this.<VertexBR>k__BackingField, other.<VertexBR>k__BackingField) && EqualityComparer<VertexPositionColorTexture>.Default.Equals(this.<VertexTL>k__BackingField, other.<VertexTL>k__BackingField) && EqualityComparer<VertexPositionColorTexture>.Default.Equals(this.<VertexTR>k__BackingField, other.<VertexTR>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Depth>k__BackingField, other.<Depth>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Min>k__BackingField, other.<Min>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Max>k__BackingField, other.<Max>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Index>k__BackingField, other.<Index>k__BackingField);
			}

			// Token: 0x0600828A RID: 33418 RVA: 0x0039B254 File Offset: 0x00399454
			[CompilerGenerated]
			public void Deconstruct(out Texture2D Texture, out VertexPositionColorTexture VertexBL, out VertexPositionColorTexture VertexBR, out VertexPositionColorTexture VertexTL, out VertexPositionColorTexture VertexTR, out float Depth, out Vector2 Min, out Vector2 Max, out int Index)
			{
				Texture = this.Texture;
				VertexBL = this.VertexBL;
				VertexBR = this.VertexBR;
				VertexTL = this.VertexTL;
				VertexTR = this.VertexTR;
				Depth = this.Depth;
				Min = this.Min;
				Max = this.Max;
				Index = this.Index;
			}
		}

		// Token: 0x02000DCD RID: 3533
		private struct RecordedBuffer
		{
			// Token: 0x0600828B RID: 33419 RVA: 0x0039B2C8 File Offset: 0x003994C8
			public RecordedBuffer(List<SpriteRecorder.Command> commandList, int startIndex, int count)
			{
				this.Texture = commandList[startIndex].Texture;
				this.VertexBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, count * 4, BufferUsage.WriteOnly);
				VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[count * 4];
				for (int i = 0; i < count; i++)
				{
					vertices[i * 4] = commandList[startIndex + i].VertexBL;
					vertices[i * 4 + 1] = commandList[startIndex + i].VertexBR;
					vertices[i * 4 + 2] = commandList[startIndex + i].VertexTL;
					vertices[i * 4 + 3] = commandList[startIndex + i].VertexTR;
				}
				this.VertexBuffer.SetData<VertexPositionColorTexture>(vertices);
				this.PolyCount = count * 2;
			}

			// Token: 0x0400509C RID: 20636
			public readonly Texture2D Texture;

			// Token: 0x0400509D RID: 20637
			public readonly VertexBuffer VertexBuffer;

			// Token: 0x0400509E RID: 20638
			public readonly int PolyCount;
		}
	}
}
