using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.SpriteDeformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200013B RID: 315
	internal class DeformableSprite
	{
		// Token: 0x17000A82 RID: 2690
		// (get) Token: 0x060028EE RID: 10478 RVA: 0x001C5146 File Offset: 0x001C3346
		public static Effect Effect
		{
			get
			{
				return DeformableSprite.effect;
			}
		}

		// Token: 0x17000A83 RID: 2691
		// (get) Token: 0x060028EF RID: 10479 RVA: 0x001C514D File Offset: 0x001C334D
		public Point Subdivisions
		{
			get
			{
				return new Point(this.subDivX, this.subDivY);
			}
		}

		// Token: 0x17000A84 RID: 2692
		// (get) Token: 0x060028F0 RID: 10480 RVA: 0x001C5160 File Offset: 0x001C3360
		// (set) Token: 0x060028F1 RID: 10481 RVA: 0x001C5168 File Offset: 0x001C3368
		public bool Invert { get; set; }

		// Token: 0x060028F2 RID: 10482 RVA: 0x001C5171 File Offset: 0x001C3371
		public void EnsureLazyLoaded()
		{
			if (!this.initialized)
			{
				this.Init();
			}
		}

		// Token: 0x060028F3 RID: 10483 RVA: 0x001C5184 File Offset: 0x001C3384
		private void Init()
		{
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			foreach (DeformableSprite existing in DeformableSprite.list)
			{
				if (existing.initialized && existing != this && existing.Sprite.Texture == this.Sprite.Texture && existing.subDivX == this.subDivX && existing.subDivY == this.subDivY && existing.Sprite.SourceRect == this.Sprite.SourceRect)
				{
					this.vertexBuffer = existing.vertexBuffer;
					this.flippedVertexBuffer = existing.flippedVertexBuffer;
					this.indexBuffer = existing.indexBuffer;
					this.triangleCount = existing.triangleCount;
					this.uvTopLeft = existing.uvTopLeft;
					this.uvBottomRight = existing.uvBottomRight;
					this.uvTopLeftFlipped = existing.uvTopLeftFlipped;
					this.uvBottomRightFlipped = existing.uvBottomRightFlipped;
					Vector2[,] array = new Vector2[2, 2];
					array[0, 0] = Vector2.Zero;
					array[0, 1] = Vector2.Zero;
					array[1, 0] = Vector2.Zero;
					array[1, 1] = Vector2.Zero;
					this.Deform(array);
					return;
				}
			}
			if (this.Sprite.Texture != null)
			{
				this.SetupVertexBuffers();
				this.SetupIndexBuffer();
			}
		}

		// Token: 0x060028F4 RID: 10484 RVA: 0x001C531C File Offset: 0x001C351C
		private void SetupVertexBuffers()
		{
			Vector2 textureSize = new Vector2((float)this.Sprite.Texture.Width, (float)this.Sprite.Texture.Height);
			Point pos = this.Sprite.SourceRect.Location;
			Point size = this.Sprite.SourceRect.Size;
			this.uvTopLeft = Vector2.Divide(pos.ToVector2(), textureSize);
			this.uvBottomRight = Vector2.Divide((pos + size).ToVector2(), textureSize);
			this.uvTopLeftFlipped = Vector2.Divide(new Vector2((float)(pos.X + size.X), (float)pos.Y), textureSize);
			this.uvBottomRightFlipped = Vector2.Divide(new Vector2((float)pos.X, (float)(pos.Y + size.Y)), textureSize);
			if (this.Invert)
			{
				Vector2 temp = this.uvBottomRightFlipped;
				this.uvBottomRightFlipped = this.uvTopLeftFlipped;
				this.uvTopLeftFlipped = temp;
			}
			for (int i = 0; i < 2; i++)
			{
				bool flip = i == 1;
				VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(this.subDivX + 1) * (this.subDivY + 1)];
				for (int x = 0; x <= this.subDivX; x++)
				{
					for (int y = 0; y <= this.subDivY; y++)
					{
						Vector2 relativePos = new Vector2((float)x / (float)this.subDivX, (float)y / (float)this.subDivY);
						Vector2 uvCoord = flip ? (this.uvTopLeftFlipped + (this.uvBottomRightFlipped - this.uvTopLeftFlipped) * relativePos) : (this.uvTopLeft + (this.uvBottomRight - this.uvTopLeft) * relativePos);
						vertices[x + y * (this.subDivX + 1)] = new VertexPositionColorTexture(new Vector3(relativePos.X * (float)this.Sprite.SourceRect.Width, relativePos.Y * (float)this.Sprite.SourceRect.Height, 0f), Color.White, uvCoord);
					}
				}
				if (flip)
				{
					if (this.flippedVertexBuffer != null && this.flippedVertexBuffer.VertexCount != vertices.Length)
					{
						this.flippedVertexBuffer.Dispose();
						this.flippedVertexBuffer = null;
					}
					if (this.flippedVertexBuffer == null)
					{
						this.flippedVertexBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
					}
					this.flippedVertexBuffer.SetData<VertexPositionColorTexture>(vertices);
				}
				else
				{
					if (this.vertexBuffer != null && this.vertexBuffer.VertexCount != vertices.Length)
					{
						this.vertexBuffer.Dispose();
						this.vertexBuffer = null;
					}
					if (this.vertexBuffer == null)
					{
						this.vertexBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
					}
					this.vertexBuffer.SetData<VertexPositionColorTexture>(vertices);
				}
			}
			this.spritePos = this.Sprite.SourceRect.Location;
			this.spriteSize = this.Sprite.SourceRect.Size;
		}

		// Token: 0x060028F5 RID: 10485 RVA: 0x001C5644 File Offset: 0x001C3844
		private void SetupIndexBuffer()
		{
			this.triangleCount = this.subDivX * this.subDivY * 2;
			ushort[] indices = new ushort[this.triangleCount * 3];
			int offset = 0;
			for (int i = 0; i < this.triangleCount / 2; i++)
			{
				indices[i * 6] = (ushort)(i + offset + 1);
				indices[i * 6 + 1] = (ushort)(i + offset + (this.subDivX + 1) + 1);
				indices[i * 6 + 2] = (ushort)(i + offset + (this.subDivX + 1));
				indices[i * 6 + 3] = (ushort)(i + offset);
				indices[i * 6 + 4] = (ushort)(i + offset + 1);
				indices[i * 6 + 5] = (ushort)(i + offset + (this.subDivX + 1));
				if ((i + 1) % this.subDivX == 0)
				{
					offset++;
				}
			}
			IndexBuffer indexBuffer = this.indexBuffer;
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
			}
			this.indexBuffer = null;
			this.indexBuffer = new IndexBuffer(GameMain.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			this.indexBuffer.SetData<ushort>(indices);
			Vector2[,] array = new Vector2[2, 2];
			array[0, 0] = Vector2.Zero;
			array[0, 1] = Vector2.Zero;
			array[1, 0] = Vector2.Zero;
			array[1, 1] = Vector2.Zero;
			this.Deform(array);
		}

		// Token: 0x060028F6 RID: 10486 RVA: 0x001C577C File Offset: 0x001C397C
		public void Deform(Func<Vector2, Vector2> deformFunction)
		{
			if (!this.initialized)
			{
				this.Init();
			}
			Vector2[,] deformAmount = new Vector2[this.subDivX + 1, this.subDivY + 1];
			for (int x = 0; x <= this.subDivX; x++)
			{
				for (int y = 0; y <= this.subDivY; y++)
				{
					deformAmount[x, y] = deformFunction(new Vector2((float)x / (float)this.subDivX, (float)y / (float)this.subDivY));
				}
			}
			this.Deform(deformAmount);
		}

		// Token: 0x060028F7 RID: 10487 RVA: 0x001C5800 File Offset: 0x001C3A00
		public void Deform(Vector2[,] deform)
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.deformArrayWidth = deform.GetLength(0);
			this.deformArrayHeight = deform.GetLength(1);
			if (this.deformAmount == null || this.deformAmount.Length != this.deformArrayWidth * this.deformArrayHeight)
			{
				this.deformAmount = new Vector2[this.deformArrayWidth * this.deformArrayHeight];
			}
			for (int x = 0; x < this.deformArrayWidth; x++)
			{
				for (int y = 0; y < this.deformArrayHeight; y++)
				{
					this.deformAmount[x + y * this.deformArrayWidth] = deform[x, y];
				}
			}
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x001C58AC File Offset: 0x001C3AAC
		public void Reset()
		{
			Vector2[,] array = new Vector2[2, 2];
			array[0, 0] = Vector2.Zero;
			array[0, 1] = Vector2.Zero;
			array[1, 0] = Vector2.Zero;
			array[1, 1] = Vector2.Zero;
			this.Deform(array);
		}

		// Token: 0x060028F9 RID: 10489 RVA: 0x001C58FC File Offset: 0x001C3AFC
		public Matrix GetTransform(Vector3 pos, Vector2 origin, float rotate, Vector2 scale)
		{
			if (!this.initialized)
			{
				this.Init();
			}
			return Matrix.CreateTranslation(-origin.X, -origin.Y, 0f) * Matrix.CreateScale(scale.X, -scale.Y, 1f) * Matrix.CreateRotationZ(-rotate) * Matrix.CreateTranslation(pos);
		}

		// Token: 0x060028FA RID: 10490 RVA: 0x001C5964 File Offset: 0x001C3B64
		public void Draw(Camera cam, Vector3 pos, Vector2 origin, float rotate, Vector2 scale, Color color, bool mirror = false, bool invert = false)
		{
			if (this.Sprite.Texture == null)
			{
				return;
			}
			if (!this.initialized)
			{
				this.Init();
			}
			if (this.Sprite.SourceRect.Location != this.spritePos || this.Sprite.SourceRect.Size != this.spriteSize)
			{
				this.SetupVertexBuffers();
			}
			DeformableSprite.effect.Parameters["xTexture"].SetValue(this.Sprite.Texture);
			Matrix matrix = this.GetTransform(pos, origin, rotate, scale);
			DeformableSprite.effect.Parameters["xTransform"].SetValue(matrix * cam.ShaderTransform * Matrix.CreateOrthographic((float)cam.Resolution.X, (float)cam.Resolution.Y, -1f, 1f) * 0.5f);
			DeformableSprite.effect.Parameters["tintColor"].SetValue(color.ToVector4());
			DeformableSprite.effect.Parameters["deformArray"].SetValue(this.deformAmount);
			DeformableSprite.effect.Parameters["deformArrayWidth"].SetValue(this.deformArrayWidth);
			DeformableSprite.effect.Parameters["deformArrayHeight"].SetValue(this.deformArrayHeight);
			if (invert)
			{
				mirror = !mirror;
			}
			DeformableSprite.effect.Parameters["uvTopLeft"].SetValue(mirror ? this.uvTopLeftFlipped : this.uvTopLeft);
			DeformableSprite.effect.Parameters["uvBottomRight"].SetValue(mirror ? this.uvBottomRightFlipped : this.uvBottomRight);
			DeformableSprite.effect.GraphicsDevice.SetVertexBuffer(mirror ? this.flippedVertexBuffer : this.vertexBuffer);
			DeformableSprite.effect.GraphicsDevice.Indices = this.indexBuffer;
			DeformableSprite.effect.CurrentTechnique.Passes[0].Apply();
			DeformableSprite.effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.triangleCount);
		}

		// Token: 0x060028FB RID: 10491 RVA: 0x001C5BA8 File Offset: 0x001C3DA8
		public void Remove()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
			DeformableSprite.list.Remove(this);
			foreach (DeformableSprite otherSprite in DeformableSprite.list)
			{
				if (otherSprite.vertexBuffer == this.vertexBuffer)
				{
					return;
				}
			}
			VertexBuffer vertexBuffer = this.vertexBuffer;
			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
			}
			this.vertexBuffer = null;
			VertexBuffer vertexBuffer2 = this.flippedVertexBuffer;
			if (vertexBuffer2 != null)
			{
				vertexBuffer2.Dispose();
			}
			this.flippedVertexBuffer = null;
			IndexBuffer indexBuffer = this.indexBuffer;
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
			}
			this.indexBuffer = null;
		}

		// Token: 0x060028FC RID: 10492 RVA: 0x001C5C70 File Offset: 0x001C3E70
		public GUIComponent CreateEditor(GUIComponent parent, List<SpriteDeformation> deformations, string parentDebugName)
		{
			GUILayoutGroup container = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = 5,
				CanBeFocused = true
			};
			RectTransform rectTransform = new RectTransform(new Point(container.Rect.Width, (int)(60f * GUI.Scale)), container.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			rectTransform.IsFixedSize = true;
			RichString text = "Sprite Deformations";
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectTransform, text, null, largeFont, Alignment.BottomCenter, false, "", null);
			GUIComponent resolutionField = GUI.CreatePointField(new Point(this.subDivX + 1, this.subDivY + 1), (int)(30f * GUI.Scale), "Resolution", container.RectTransform, "How many vertices the deformable sprite has on the x and y axes. Larger values make the deformations look smoother, but are more performance intensive.");
			resolutionField.RectTransform.IsFixedSize = true;
			GUINumberInput xField = null;
			GUINumberInput yField = null;
			foreach (GUIComponent child in resolutionField.GetAllChildren())
			{
				if (yField == null)
				{
					yField = (child as GUINumberInput);
				}
				else
				{
					xField = (child as GUINumberInput);
					if (xField != null)
					{
						break;
					}
				}
			}
			xField.MinValueInt = new int?(2);
			xField.MaxValueInt = new int?(SpriteDeformationParams.ShaderMaxResolution.X - 1);
			xField.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				base.<CreateEditor>g__ChangeResolution|2();
			};
			yField.MinValueInt = new int?(2);
			yField.MaxValueInt = new int?(SpriteDeformationParams.ShaderMaxResolution.Y - 1);
			yField.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				base.<CreateEditor>g__ChangeResolution|2();
			};
			foreach (SpriteDeformation deformation in deformations)
			{
				SerializableEntityEditor deformEditor = new SerializableEntityEditor(container.RectTransform, deformation.Params, false, true, "", 24, GUIStyle.SubHeadingFont, true);
				deformEditor.RectTransform.MinSize = new Point(deformEditor.Rect.Width, deformEditor.Rect.Height);
			}
			GUIDropDown deformationDD = new GUIDropDown(new RectTransform(new Point(container.Rect.Width, 30), container.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "Add new sprite deformation", 4, "", false, false, Alignment.CenterLeft, 1f);
			deformationDD.OnSelected = delegate(GUIComponent selected, object userdata)
			{
				deformations.Add(SpriteDeformation.Load((string)userdata, parentDebugName));
				deformationDD.Text = "Add new sprite deformation";
				return false;
			};
			foreach (string deformationType in SpriteDeformation.DeformationTypes)
			{
				deformationDD.AddItem(deformationType, deformationType, null, null, null);
			}
			container.RectTransform.Resize(new Point(container.Rect.Width, container.Children.Sum((GUIComponent c) => c.Rect.Height + container.AbsoluteSpacing)), false);
			container.RectTransform.MinSize = new Point(0, container.Rect.Height);
			container.RectTransform.MaxSize = new Point(int.MaxValue, container.Rect.Height);
			container.RectTransform.IsFixedSize = true;
			container.Recalculate();
			return container;
		}

		// Token: 0x17000A85 RID: 2693
		// (get) Token: 0x060028FD RID: 10493 RVA: 0x001C60CC File Offset: 0x001C42CC
		public Vector2 Size
		{
			get
			{
				return this.Sprite.size;
			}
		}

		// Token: 0x17000A86 RID: 2694
		// (get) Token: 0x060028FE RID: 10494 RVA: 0x001C60D9 File Offset: 0x001C42D9
		// (set) Token: 0x060028FF RID: 10495 RVA: 0x001C60E6 File Offset: 0x001C42E6
		public Vector2 Origin
		{
			get
			{
				return this.Sprite.Origin;
			}
			set
			{
				this.Sprite.Origin = value;
			}
		}

		// Token: 0x17000A87 RID: 2695
		// (get) Token: 0x06002900 RID: 10496 RVA: 0x001C60F4 File Offset: 0x001C42F4
		// (set) Token: 0x06002901 RID: 10497 RVA: 0x001C60FC File Offset: 0x001C42FC
		public Sprite Sprite { get; private set; }

		// Token: 0x06002902 RID: 10498 RVA: 0x001C6105 File Offset: 0x001C4305
		public DeformableSprite(ContentXElement element, int? subdivisionsX = null, int? subdivisionsY = null, string filePath = "", bool lazyLoad = false, bool invert = false, float sourceRectScale = 1f)
		{
			this.Sprite = new Sprite(element, "", filePath, lazyLoad, sourceRectScale);
			this.InitProjSpecific(element, subdivisionsX, subdivisionsY, lazyLoad, invert);
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x001C6138 File Offset: 0x001C4338
		private void InitProjSpecific(XElement element, int? subdivisionsX, int? subdivisionsY, bool lazyLoad, bool invert)
		{
			if (DeformableSprite.effect == null)
			{
				DeformableSprite.effect = EffectLoader.Load("Effects/deformshader");
			}
			this.Invert = invert;
			Vector2 subdivisionsInXml = element.GetAttributeVector2("subdivisions", element.GetAttributeVector2("resolution", Vector2.One));
			this.subDivX = (subdivisionsX ?? ((int)subdivisionsInXml.X));
			this.subDivY = (subdivisionsY ?? ((int)subdivisionsInXml.Y));
			if (this.subDivX <= 0 || this.subDivY <= 0)
			{
				throw new ArgumentException("Deformable sprites must have one or more subdivisions on each axis.");
			}
			if (!lazyLoad)
			{
				this.Init();
			}
			DeformableSprite.list.Add(this);
		}

		// Token: 0x040014F3 RID: 5363
		private static List<DeformableSprite> list = new List<DeformableSprite>();

		// Token: 0x040014F4 RID: 5364
		private bool initialized;

		// Token: 0x040014F5 RID: 5365
		private int triangleCount;

		// Token: 0x040014F6 RID: 5366
		private VertexBuffer vertexBuffer;

		// Token: 0x040014F7 RID: 5367
		private VertexBuffer flippedVertexBuffer;

		// Token: 0x040014F8 RID: 5368
		private IndexBuffer indexBuffer;

		// Token: 0x040014F9 RID: 5369
		private Vector2 uvTopLeft;

		// Token: 0x040014FA RID: 5370
		private Vector2 uvBottomRight;

		// Token: 0x040014FB RID: 5371
		private Vector2 uvTopLeftFlipped;

		// Token: 0x040014FC RID: 5372
		private Vector2 uvBottomRightFlipped;

		// Token: 0x040014FD RID: 5373
		private Vector2[] deformAmount;

		// Token: 0x040014FE RID: 5374
		private int deformArrayWidth;

		// Token: 0x040014FF RID: 5375
		private int deformArrayHeight;

		// Token: 0x04001500 RID: 5376
		private int subDivX;

		// Token: 0x04001501 RID: 5377
		private int subDivY;

		// Token: 0x04001502 RID: 5378
		private static Effect effect;

		// Token: 0x04001504 RID: 5380
		private Point spritePos;

		// Token: 0x04001505 RID: 5381
		private Point spriteSize;
	}
}
