using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpFont;

namespace Barotrauma
{
	// Token: 0x02000060 RID: 96
	public class ScalableFont : IDisposable
	{
		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06000D01 RID: 3329 RVA: 0x00077B3E File Offset: 0x00075D3E
		// (set) Token: 0x06000D02 RID: 3330 RVA: 0x00077B46 File Offset: 0x00075D46
		public bool DynamicLoading { get; private set; }

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06000D03 RID: 3331 RVA: 0x00077B4F File Offset: 0x00075D4F
		// (set) Token: 0x06000D04 RID: 3332 RVA: 0x00077B57 File Offset: 0x00075D57
		public TextManager.SpeciallyHandledCharCategory SpeciallyHandledCharCategory { get; private set; }

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06000D05 RID: 3333 RVA: 0x00077B60 File Offset: 0x00075D60
		// (set) Token: 0x06000D06 RID: 3334 RVA: 0x00077B68 File Offset: 0x00075D68
		public uint Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
				if (this.graphicsDevice != null)
				{
					this.RenderAtlas(this.graphicsDevice, this.charRanges, this.texDims, this.baseChar);
				}
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06000D07 RID: 3335 RVA: 0x00077B97 File Offset: 0x00075D97
		public float LineHeight
		{
			get
			{
				return (float)this.baseHeight * 1.8f;
			}
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x00077BA8 File Offset: 0x00075DA8
		public static TextManager.SpeciallyHandledCharCategory ExtractShccFromXElement(XElement element)
		{
			return TextManager.SpeciallyHandledCharCategories.Where(delegate(TextManager.SpeciallyHandledCharCategory category)
			{
				XElement element2 = element;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral("is");
				defaultInterpolatedStringHandler.AppendFormatted<TextManager.SpeciallyHandledCharCategory>(category);
				string name = defaultInterpolatedStringHandler.ToStringAndClear();
				bool defaultValue;
				switch (category)
				{
				case TextManager.SpeciallyHandledCharCategory.CJK:
					defaultValue = false;
					goto IL_89;
				case TextManager.SpeciallyHandledCharCategory.Cyrillic:
					defaultValue = true;
					goto IL_89;
				case TextManager.SpeciallyHandledCharCategory.Japanese:
					defaultValue = false;
					goto IL_89;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("nameof");
				defaultInterpolatedStringHandler2.AppendFormatted<TextManager.SpeciallyHandledCharCategory>(category);
				defaultInterpolatedStringHandler2.AppendLiteral(" not implemented.");
				throw new NotImplementedException(defaultInterpolatedStringHandler2.ToStringAndClear());
				IL_89:
				return element2.GetAttributeBool(name, defaultValue);
			}).Aggregate(TextManager.SpeciallyHandledCharCategory.None, (TextManager.SpeciallyHandledCharCategory current, TextManager.SpeciallyHandledCharCategory category) => current | category);
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x00077C00 File Offset: 0x00075E00
		public ScalableFont(ContentXElement element, uint defaultSize = 14U, GraphicsDevice gd = null)
		{
			ContentPath attributeContentPath = element.GetAttributeContentPath("file");
			this..ctor((attributeContentPath != null) ? attributeContentPath.Value : null, (uint)element.GetAttributeInt("size", (int)defaultSize), gd, element.GetAttributeBool("dynamicloading", false), ScalableFont.ExtractShccFromXElement(element));
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x00077C50 File Offset: 0x00075E50
		public ScalableFont(string filename, uint size, GraphicsDevice gd = null, bool dynamicLoading = false, TextManager.SpeciallyHandledCharCategory speciallyHandledCharCategory = TextManager.SpeciallyHandledCharCategory.None)
		{
			this.rwl = new ReaderWriterLockSlim();
			base..ctor();
			object obj = ScalableFont.globalMutex;
			lock (obj)
			{
				if (ScalableFont.Lib == null)
				{
					ScalableFont.Lib = new Library();
				}
			}
			this.filename = filename;
			this.face = null;
			using (new ReadLock(this.rwl))
			{
				foreach (ScalableFont font in ScalableFont.FontList)
				{
					if (font.filename == filename)
					{
						this.face = font.face;
						break;
					}
				}
			}
			if (this.face == null)
			{
				this.face = new Face(ScalableFont.Lib, filename);
			}
			this.size = size;
			this.textures = new List<Texture2D>();
			this.texCoords = new Dictionary<uint, ScalableFont.GlyphData>();
			this.DynamicLoading = dynamicLoading;
			this.SpeciallyHandledCharCategory = speciallyHandledCharCategory;
			this.graphicsDevice = gd;
			if (gd != null && !dynamicLoading)
			{
				this.RenderAtlas(gd, null, 1024, 84U);
			}
			object obj2 = ScalableFont.globalMutex;
			lock (obj2)
			{
				ScalableFont.FontList.Add(this);
			}
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x00077DD0 File Offset: 0x00075FD0
		private void RenderAtlas(GraphicsDevice gd, uint[] charRanges = null, int texDims = 1024, uint baseChar = 84U)
		{
			if (this.DynamicLoading)
			{
				return;
			}
			if (charRanges == null)
			{
				charRanges = new uint[]
				{
					32U,
					65535U
				};
			}
			this.charRanges = charRanges;
			this.texDims = texDims;
			this.baseChar = baseChar;
			this.textures.ForEach(delegate(Texture2D t)
			{
				t.Dispose();
			});
			this.textures.Clear();
			this.texCoords.Clear();
			uint[] pixelBuffer = new uint[texDims * texDims];
			for (int i = 0; i < texDims * texDims; i++)
			{
				pixelBuffer[i] = 0U;
			}
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				this.textures.Add(new Texture2D(gd, texDims, texDims, false, SurfaceFormat.Color));
			});
			int texIndex = 0;
			Vector2 currentCoords = Vector2.Zero;
			int nextY = 0;
			using (new WriteLock(this.rwl))
			{
				this.face.SetPixelSizes(0U, this.size);
				this.face.LoadGlyph(this.face.GetCharIndex(baseChar), LoadFlags.Default, LoadTarget.Normal);
				this.baseHeight = this.face.Glyph.Metrics.Height.ToInt32();
				CrossThread.TaskDelegate <>9__3;
				CrossThread.TaskDelegate <>9__2;
				for (int j = 0; j < charRanges.Length; j += 2)
				{
					uint start = charRanges[j];
					uint end = charRanges[j + 1];
					for (uint k = start; k <= end; k += 1U)
					{
						uint glyphIndex = this.face.GetCharIndex(k);
						if (glyphIndex == 0U)
						{
							this.texCoords.Add(k, new ScalableFont.GlyphData(-1, default(Vector2), 0f, default(Rectangle)));
						}
						else
						{
							this.face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
							if (this.face.Glyph.Metrics.Width == 0 || this.face.Glyph.Metrics.Height == 0)
							{
								int texIndex4 = -1;
								float advance = Math.Max((float)this.face.Glyph.Metrics.HorizontalAdvance, 0f);
								ScalableFont.GlyphData blankData = new ScalableFont.GlyphData(texIndex4, default(Vector2), advance, default(Rectangle));
								this.texCoords.Add(k, blankData);
							}
							else
							{
								this.face.Glyph.RenderGlyph(RenderMode.Normal);
								byte[] bitmap = this.face.Glyph.Bitmap.BufferData;
								int glyphWidth = this.face.Glyph.Bitmap.Width;
								int glyphHeight = bitmap.Length / glyphWidth;
								if (glyphWidth > texDims - 1 || glyphHeight > texDims - 1)
								{
									throw new Exception(string.Concat(new string[]
									{
										this.filename,
										", ",
										this.size.ToString(),
										", ",
										((char)k).ToString(),
										"; Glyph dimensions exceed texture atlas dimensions"
									}));
								}
								nextY = Math.Max(nextY, glyphHeight + 2);
								if (currentCoords.X + (float)glyphWidth + 2f > (float)(texDims - 1))
								{
									currentCoords.X = 0f;
									currentCoords.Y += (float)nextY;
									nextY = 0;
								}
								if (currentCoords.Y + (float)glyphHeight + 2f > (float)(texDims - 1))
								{
									currentCoords.X = 0f;
									currentCoords.Y = 0f;
									CrossThread.TaskDelegate deleg;
									if ((deleg = <>9__3) == null)
									{
										deleg = (<>9__3 = delegate()
										{
											this.textures[texIndex].SetData<uint>(pixelBuffer);
											this.textures.Add(new Texture2D(gd, texDims, texDims, false, SurfaceFormat.Color));
										});
									}
									CrossThread.RequestExecutionOnMainThread(deleg);
									int texIndex2 = texIndex;
									texIndex = texIndex2 + 1;
									for (int l = 0; l < texDims * texDims; l++)
									{
										pixelBuffer[l] = 0U;
									}
								}
								float advance = (float)this.face.Glyph.Metrics.HorizontalAdvance;
								int texIndex3 = texIndex;
								Rectangle rectangle = new Rectangle((int)currentCoords.X, (int)currentCoords.Y, glyphWidth, glyphHeight);
								ScalableFont.GlyphData newData = new ScalableFont.GlyphData(texIndex3, new Vector2((float)this.face.Glyph.BitmapLeft, (float)(this.baseHeight * 14 / 10 - this.face.Glyph.BitmapTop)), advance, rectangle);
								this.texCoords.Add(k, newData);
								for (int y = 0; y < glyphHeight; y++)
								{
									for (int x = 0; x < glyphWidth; x++)
									{
										byte byteColor = bitmap[x + y * glyphWidth];
										pixelBuffer[(int)currentCoords.X + x + ((int)currentCoords.Y + y) * texDims] = (uint)((int)byteColor << 24 | 16777215);
									}
								}
								currentCoords.X += (float)(glyphWidth + 2);
							}
						}
					}
					CrossThread.TaskDelegate deleg2;
					if ((deleg2 = <>9__2) == null)
					{
						deleg2 = (<>9__2 = delegate()
						{
							this.textures[texIndex].SetData<uint>(pixelBuffer);
						});
					}
					CrossThread.RequestExecutionOnMainThread(deleg2);
				}
			}
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x0007831C File Offset: 0x0007651C
		private void DynamicRenderAtlas(GraphicsDevice gd, uint character, int texDims = 1024, uint baseChar = 84U)
		{
			bool missingCharacterFound = false;
			using (new ReadLock(this.rwl))
			{
				missingCharacterFound = !this.texCoords.ContainsKey(character);
			}
			if (!missingCharacterFound)
			{
				return;
			}
			this.DynamicRenderAtlas(gd, character.ToEnumerable<uint>(), texDims, baseChar);
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x00078378 File Offset: 0x00076578
		private void DynamicRenderAtlas(GraphicsDevice gd, string str, int texDims = 1024, uint baseChar = 84U)
		{
			bool missingCharacterFound = false;
			using (new ReadLock(this.rwl))
			{
				foreach (char character in str)
				{
					if (!this.texCoords.ContainsKey((uint)character))
					{
						missingCharacterFound = true;
						break;
					}
				}
			}
			if (!missingCharacterFound)
			{
				return;
			}
			this.DynamicRenderAtlas(gd, from c in str
			select (uint)c, texDims, baseChar);
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x00078410 File Offset: 0x00076610
		private void DynamicRenderAtlas(GraphicsDevice gd, IEnumerable<uint> characters, int texDims = 1024, uint baseChar = 84U)
		{
			if (Thread.CurrentThread != GameMain.MainThread)
			{
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					this.DynamicRenderAtlas(gd, characters, texDims, baseChar);
				});
				return;
			}
			using (new WriteLock(this.rwl))
			{
				if (this.textures.Count == 0)
				{
					this.texDims = texDims;
					this.baseChar = baseChar;
					this.face.SetPixelSizes(0U, this.size);
					this.face.LoadGlyph(this.face.GetCharIndex(baseChar), LoadFlags.Default, LoadTarget.Normal);
					this.baseHeight = this.face.Glyph.Metrics.Height.ToInt32();
					this.textures.Add(new Texture2D(gd, texDims, texDims, false, SurfaceFormat.Color));
				}
				bool anyChanges = false;
				bool firstChar = true;
				foreach (uint character in characters)
				{
					if (!this.texCoords.ContainsKey(character))
					{
						uint glyphIndex = this.face.GetCharIndex(character);
						if (glyphIndex == 0U)
						{
							this.texCoords.Add(character, new ScalableFont.GlyphData(-1, default(Vector2), 0f, default(Rectangle)));
						}
						else
						{
							this.face.SetPixelSizes(0U, this.size);
							this.face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
							if (this.face.Glyph.Metrics.Width == 0 || this.face.Glyph.Metrics.Height == 0)
							{
								int texIndex = -1;
								float advance = Math.Max((float)this.face.Glyph.Metrics.HorizontalAdvance, 0f);
								ScalableFont.GlyphData blankData = new ScalableFont.GlyphData(texIndex, default(Vector2), advance, default(Rectangle));
								this.texCoords.Add(character, blankData);
							}
							else
							{
								this.face.Glyph.RenderGlyph(RenderMode.Normal);
								byte[] bitmap = (byte[])this.face.Glyph.Bitmap.BufferData.Clone();
								int glyphWidth = this.face.Glyph.Bitmap.Width;
								int glyphHeight = bitmap.Length / glyphWidth;
								Fixed26Dot6 horizontalAdvance = this.face.Glyph.Metrics.HorizontalAdvance;
								Vector2 drawOffset = new Vector2((float)this.face.Glyph.BitmapLeft, (float)(this.baseHeight * 14 / 10 - this.face.Glyph.BitmapTop));
								if (glyphWidth > texDims - 1 || glyphHeight > texDims - 1)
								{
									throw new Exception(string.Concat(new string[]
									{
										this.filename,
										", ",
										this.size.ToString(),
										", ",
										((char)character).ToString(),
										"; Glyph dimensions exceed texture atlas dimensions"
									}));
								}
								this.currentDynamicAtlasNextY = Math.Max(this.currentDynamicAtlasNextY, glyphHeight + 2);
								if (this.currentDynamicAtlasCoords.X + (float)glyphWidth + 2f > (float)(texDims - 1))
								{
									this.currentDynamicAtlasCoords.X = 0f;
									this.currentDynamicAtlasCoords.Y = this.currentDynamicAtlasCoords.Y + (float)this.currentDynamicAtlasNextY;
									this.currentDynamicAtlasNextY = 0;
								}
								if (this.currentDynamicAtlasCoords.Y + (float)glyphHeight + 2f > (float)(texDims - 1))
								{
									if (!firstChar)
									{
										List<Texture2D> list = this.textures;
										list[list.Count - 1].SetData<uint>(this.currentDynamicPixelBuffer);
									}
									this.currentDynamicAtlasCoords.X = 0f;
									this.currentDynamicAtlasCoords.Y = 0f;
									this.currentDynamicAtlasNextY = 0;
									this.textures.Add(new Texture2D(gd, texDims, texDims, false, SurfaceFormat.Color));
									this.currentDynamicPixelBuffer = null;
								}
								float advance = (float)horizontalAdvance;
								int texIndex2 = this.textures.Count - 1;
								Rectangle rectangle = new Rectangle((int)this.currentDynamicAtlasCoords.X, (int)this.currentDynamicAtlasCoords.Y, glyphWidth, glyphHeight);
								ScalableFont.GlyphData newData = new ScalableFont.GlyphData(texIndex2, drawOffset, advance, rectangle);
								this.texCoords.Add(character, newData);
								if (this.currentDynamicPixelBuffer == null)
								{
									this.currentDynamicPixelBuffer = new uint[texDims * texDims];
									this.textures[newData.TexIndex].GetData<uint>(this.currentDynamicPixelBuffer, 0, texDims * texDims);
								}
								for (int y = 0; y < glyphHeight; y++)
								{
									for (int x = 0; x < glyphWidth; x++)
									{
										byte byteColor = bitmap[x + y * glyphWidth];
										this.currentDynamicPixelBuffer[(int)this.currentDynamicAtlasCoords.X + x + ((int)this.currentDynamicAtlasCoords.Y + y) * texDims] = (uint)((int)byteColor << 24 | 16777215);
									}
								}
								this.currentDynamicAtlasCoords.X = this.currentDynamicAtlasCoords.X + (float)(glyphWidth + 2);
								firstChar = false;
								anyChanges = true;
							}
						}
					}
				}
				if (anyChanges)
				{
					List<Texture2D> list2 = this.textures;
					list2[list2.Count - 1].SetData<uint>(this.currentDynamicPixelBuffer);
				}
			}
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x000789D0 File Offset: 0x00076BD0
		private void HandleNewLineAndAlignment(string text, in Vector2 advanceUnit, in Vector2 position, in Vector2 scale, Alignment alignment, int i, ref float lineWidth, ref Vector2 currentLineOffset, ref int lineNum, ref Vector2 currentPos, out uint charIndex, out bool shouldContinue)
		{
			if (lineWidth < 0f || text[i] == '\n')
			{
				bool isHorizontallyCentered = (alignment & Alignment.CenterX) == Alignment.CenterX;
				bool isAlignedToRight = (alignment & Alignment.Right) == Alignment.Right;
				if (isHorizontallyCentered || isAlignedToRight)
				{
					int startIndex = (lineWidth < 0f) ? i : (i + 1);
					lineWidth = 0f;
					int j = startIndex;
					while (j < text.Length && text[j] != '\n')
					{
						uint chrIndex = (uint)text[j];
						ScalableFont.GlyphData gd2 = this.GetGlyphData(chrIndex);
						lineWidth += gd2.Advance;
						j++;
					}
					currentLineOffset = -lineWidth * advanceUnit * scale.X;
					if (isHorizontallyCentered)
					{
						currentLineOffset *= 0.5f;
					}
					currentLineOffset.X = MathF.Round(currentLineOffset.X);
					currentLineOffset.Y = MathF.Round(currentLineOffset.Y);
				}
			}
			if (text[i] == '\n')
			{
				lineNum++;
				currentPos = position;
				currentPos.X -= this.LineHeight * (float)lineNum * advanceUnit.Y * scale.Y;
				currentPos.Y += this.LineHeight * (float)lineNum * advanceUnit.X * scale.Y;
				shouldContinue = true;
				charIndex = 0U;
				return;
			}
			shouldContinue = false;
			charIndex = (uint)text[i];
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x00078B4C File Offset: 0x00076D4C
		private ScalableFont.GlyphData GetGlyphData(uint charIndex)
		{
			ScalableFont.GlyphData gd;
			if (this.texCoords.TryGetValue(charIndex, out gd) || this.texCoords.TryGetValue(9633U, out gd))
			{
				return gd;
			}
			return new ScalableFont.GlyphData(-1, default(Vector2), 0f, default(Rectangle));
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x00078B9C File Offset: 0x00076D9C
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects se, float layerDepth, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			if (this.textures.Count == 0 && !this.DynamicLoading)
			{
				return;
			}
			text = this.ApplyUpperCase(text, forceUpperCase);
			if (this.DynamicLoading)
			{
				this.DynamicRenderAtlas(this.graphicsDevice, text, 1024, 84U);
			}
			float lineWidth = -1f;
			Vector2 currentLineOffset = Vector2.Zero;
			int lineNum = 0;
			Vector2 currentPos = position;
			Vector2 advanceUnit = (rotation == 0f) ? Vector2.UnitX : new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
			for (int i = 0; i < text.Length; i++)
			{
				uint charIndex;
				bool shouldContinue;
				this.HandleNewLineAndAlignment(text, advanceUnit, position, scale, alignment, i, ref lineWidth, ref currentLineOffset, ref lineNum, ref currentPos, out charIndex, out shouldContinue);
				if (!shouldContinue)
				{
					ScalableFont.GlyphData gd = this.GetGlyphData(charIndex);
					if (gd.TexIndex >= 0)
					{
						if (gd.TexIndex >= this.textures.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Error while rendering text. Texture index was out of range. Text: ");
							defaultInterpolatedStringHandler.AppendFormatted(text);
							defaultInterpolatedStringHandler.AppendLiteral(", char: ");
							defaultInterpolatedStringHandler.AppendFormatted<uint>(charIndex);
							defaultInterpolatedStringHandler.AppendLiteral(" index: ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(gd.TexIndex);
							defaultInterpolatedStringHandler.AppendLiteral(", texture count: ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(this.textures.Count);
							throw new ArgumentOutOfRangeException(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						Texture2D tex = this.textures[gd.TexIndex];
						Vector2 drawOffset;
						drawOffset.X = gd.DrawOffset.X * advanceUnit.X * scale.X - gd.DrawOffset.Y * advanceUnit.Y * scale.Y;
						drawOffset.Y = gd.DrawOffset.X * advanceUnit.Y * scale.Y + gd.DrawOffset.Y * advanceUnit.X * scale.X;
						sb.Draw(tex, currentPos + currentLineOffset + drawOffset, new Rectangle?(gd.TexCoords), color, rotation, origin, scale, se, layerDepth);
					}
					currentPos += gd.Advance * advanceUnit * scale.X;
				}
			}
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x00078DE0 File Offset: 0x00076FE0
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects se, float layerDepth, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			this.DrawString(sb, text, position, color, rotation, origin, new Vector2(scale), se, layerDepth, alignment, forceUpperCase);
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x00078E0C File Offset: 0x0007700C
		private string ApplyUpperCase(string text, ForceUpperCase forceUpperCase)
		{
			string result;
			switch (forceUpperCase)
			{
			case Barotrauma.ForceUpperCase.Inherit:
				result = (this.ForceUpperCase ? text.ToUpperInvariant() : text);
				break;
			case Barotrauma.ForceUpperCase.No:
				result = text;
				break;
			case Barotrauma.ForceUpperCase.Yes:
				result = text.ToUpperInvariant();
				break;
			default:
				<PrivateImplementationDetails>.ThrowSwitchExpressionException(forceUpperCase);
				break;
			}
			return result;
		}

		// Token: 0x06000D14 RID: 3348 RVA: 0x00078E5C File Offset: 0x0007705C
		public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit, bool italics = false)
		{
			if (this.textures.Count == 0 && !this.DynamicLoading)
			{
				return;
			}
			text = this.ApplyUpperCase(text, forceUpperCase);
			if (this.DynamicLoading)
			{
				this.DynamicRenderAtlas(this.graphicsDevice, text, 1024, 84U);
			}
			ScalableFont.quadVertices[0].Color = color;
			ScalableFont.quadVertices[1].Color = color;
			ScalableFont.quadVertices[2].Color = color;
			ScalableFont.quadVertices[3].Color = color;
			Vector2 currentPos = position;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					currentPos.X = position.X;
					currentPos.Y += this.LineHeight;
				}
				else
				{
					uint charIndex = (uint)text[i];
					ScalableFont.GlyphData gd = this.GetGlyphData(charIndex);
					if (gd.TexIndex >= 0)
					{
						float halfCharHeight = (float)gd.TexCoords.Height * 0.5f;
						float topItalicOffset = 0f;
						float bottomItalicOffset = 0f;
						if (italics)
						{
							topItalicOffset = (halfCharHeight - gd.DrawOffset.Y) * 0.35f + (float)this.baseHeight * 0.18f;
							bottomItalicOffset = (-halfCharHeight - gd.DrawOffset.Y) * 0.35f + (float)this.baseHeight * 0.18f;
						}
						Texture2D tex = this.textures[gd.TexIndex];
						float left = (float)gd.TexCoords.Left / (float)tex.Width;
						float bottom = (float)gd.TexCoords.Bottom / (float)tex.Height;
						float top = (float)gd.TexCoords.Top / (float)tex.Height;
						float right = (float)gd.TexCoords.Right / (float)tex.Width;
						ScalableFont.quadVertices[0].Position = new Vector3(currentPos + gd.DrawOffset + new ValueTuple<float, float>(bottomItalicOffset, (float)gd.TexCoords.Height), 0f);
						ScalableFont.quadVertices[0].TextureCoordinate = new Vector2(left, bottom);
						ScalableFont.quadVertices[1].Position = new Vector3(currentPos + gd.DrawOffset + new ValueTuple<float, float>(topItalicOffset, 0f), 0f);
						ScalableFont.quadVertices[1].TextureCoordinate = new Vector2(left, top);
						ScalableFont.quadVertices[2].Position = new Vector3(currentPos + gd.DrawOffset + new ValueTuple<float, float>((float)gd.TexCoords.Width + bottomItalicOffset, (float)gd.TexCoords.Height), 0f);
						ScalableFont.quadVertices[2].TextureCoordinate = new Vector2(right, bottom);
						ScalableFont.quadVertices[3].Position = new Vector3(currentPos + gd.DrawOffset + new ValueTuple<float, float>((float)gd.TexCoords.Width + topItalicOffset, 0f), 0f);
						ScalableFont.quadVertices[3].TextureCoordinate = new Vector2(right, top);
						sb.Draw(tex, ScalableFont.quadVertices, 0f, null);
					}
					currentPos.X += gd.Advance;
				}
			}
		}

		// Token: 0x06000D15 RID: 3349 RVA: 0x000791FC File Offset: 0x000773FC
		public void DrawStringWithColors(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects se, float layerDepth, in ImmutableArray<RichTextData>? richTextData, int rtdOffset = 0, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			this.DrawStringWithColors(sb, text, position, color, rotation, origin, new Vector2(scale), se, layerDepth, richTextData, rtdOffset, alignment, forceUpperCase);
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x0007922C File Offset: 0x0007742C
		public void DrawStringWithColors(SpriteBatch sb, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects se, float layerDepth, in ImmutableArray<RichTextData>? richTextData, int rtdOffset = 0, Alignment alignment = Alignment.TopLeft, ForceUpperCase forceUpperCase = Barotrauma.ForceUpperCase.Inherit)
		{
			if (this.textures.Count == 0 && !this.DynamicLoading)
			{
				return;
			}
			if (richTextData == null || richTextData.Value.Length <= 0)
			{
				this.DrawString(sb, text, position, color, rotation, origin, scale, se, layerDepth, Alignment.TopLeft, forceUpperCase);
				return;
			}
			text = this.ApplyUpperCase(text, forceUpperCase);
			float lineWidth = -1f;
			Vector2 currentLineOffset = Vector2.Zero;
			if (this.DynamicLoading)
			{
				this.DynamicRenderAtlas(this.graphicsDevice, text, 1024, 84U);
			}
			int lineNum = 0;
			Vector2 currentPos = position;
			Vector2 advanceUnit = (rotation == 0f) ? Vector2.UnitX : new Vector2((float)Math.Cos((double)rotation), (float)Math.Sin((double)rotation));
			int richTextDataIndex = 0;
			RichTextData currentRichTextData = richTextData.Value[richTextDataIndex];
			for (int i = 0; i < text.Length; i++)
			{
				uint charIndex;
				bool shouldContinue;
				this.HandleNewLineAndAlignment(text, advanceUnit, position, scale, alignment, i, ref lineWidth, ref currentLineOffset, ref lineNum, ref currentPos, out charIndex, out shouldContinue);
				if (!shouldContinue)
				{
					while (currentRichTextData != null && i + rtdOffset > currentRichTextData.EndIndex + lineNum)
					{
						richTextDataIndex++;
						currentRichTextData = ((richTextDataIndex < richTextData.Value.Length) ? richTextData.Value[richTextDataIndex] : null);
					}
					Color currentTextColor;
					if (currentRichTextData != null && currentRichTextData.StartIndex + lineNum <= i + rtdOffset && i + rtdOffset <= currentRichTextData.EndIndex + lineNum)
					{
						currentTextColor = (currentRichTextData.Color * currentRichTextData.Alpha).GetValueOrDefault(color);
						if (!string.IsNullOrEmpty(currentRichTextData.Metadata))
						{
							currentTextColor = Color.Lerp(currentTextColor, Color.White, 0.5f);
						}
					}
					else
					{
						currentTextColor = color;
					}
					ScalableFont.GlyphData gd = this.GetGlyphData(charIndex);
					if (gd.TexIndex >= 0)
					{
						Texture2D tex = this.textures[gd.TexIndex];
						Vector2 drawOffset;
						drawOffset.X = gd.DrawOffset.X * advanceUnit.X * scale.X - gd.DrawOffset.Y * advanceUnit.Y * scale.Y;
						drawOffset.Y = gd.DrawOffset.X * advanceUnit.Y * scale.Y + gd.DrawOffset.Y * advanceUnit.X * scale.X;
						sb.Draw(tex, currentPos + currentLineOffset + drawOffset, new Rectangle?(gd.TexCoords), currentTextColor, rotation, origin, scale, se, layerDepth);
					}
					currentPos += gd.Advance * advanceUnit * scale.X;
				}
			}
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x00079514 File Offset: 0x00077714
		public string WrapText(string text, float width)
		{
			Vector2 vector;
			Vector2[] array;
			return this.WrapText(text, width, 0, out vector, false, out array);
		}

		// Token: 0x06000D18 RID: 3352 RVA: 0x00079530 File Offset: 0x00077730
		public string WrapText(string text, float width, int requestCharPos, out Vector2 requestedCharPos)
		{
			Vector2[] array;
			return this.WrapText(text, width, requestCharPos, out requestedCharPos, false, out array);
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x0007954C File Offset: 0x0007774C
		public string WrapText(string text, float width, out Vector2[] allCharPositions)
		{
			Vector2 vector;
			return this.WrapText(text, width, 0, out vector, true, out allCharPositions);
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x00079568 File Offset: 0x00077768
		private string WrapText(string text, float width, int requestCharPos, out Vector2 requestedCharPos, bool returnAllCharPositions, out Vector2[] allCharPositions)
		{
			ScalableFont.<>c__DisplayClass51_0 CS$<>8__locals1;
			CS$<>8__locals1.requestCharPos = requestCharPos;
			CS$<>8__locals1.text = text;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.currLineStart = 0;
			CS$<>8__locals1.currentPos = Vector2.Zero;
			CS$<>8__locals1.foundCharPos = Vector2.Zero;
			CS$<>8__locals1.lastBreakerIndex = null;
			CS$<>8__locals1.result = "";
			CS$<>8__locals1.allCharPos = (returnAllCharPositions ? new Vector2[CS$<>8__locals1.text.Length + 1] : null);
			ScalableFont.<>c__DisplayClass51_1 CS$<>8__locals2;
			CS$<>8__locals2.i = 0;
			while (CS$<>8__locals2.i < CS$<>8__locals1.text.Length)
			{
				this.<WrapText>g__recordCurrentPos|51_0(ref CS$<>8__locals1, ref CS$<>8__locals2);
				if (CS$<>8__locals1.text[CS$<>8__locals2.i] == '\n')
				{
					this.<WrapText>g__nextLine|51_1(ref CS$<>8__locals1, ref CS$<>8__locals2);
				}
				else
				{
					float advance = this.GetGlyphData((uint)CS$<>8__locals1.text[CS$<>8__locals2.i]).Advance;
					if (CS$<>8__locals1.currentPos.X + advance >= width)
					{
						if (CS$<>8__locals2.i > 0 && char.IsWhiteSpace(CS$<>8__locals1.text[CS$<>8__locals2.i]) && !char.IsWhiteSpace(CS$<>8__locals1.text[CS$<>8__locals2.i - 1]))
						{
							advance = width - CS$<>8__locals1.currentPos.X;
						}
						else
						{
							if (CS$<>8__locals1.lastBreakerIndex != null)
							{
								CS$<>8__locals2.i = CS$<>8__locals1.lastBreakerIndex.Value + 1;
								advance = this.GetGlyphData((uint)CS$<>8__locals1.text[CS$<>8__locals2.i]).Advance;
							}
							this.<WrapText>g__nextLine|51_1(ref CS$<>8__locals1, ref CS$<>8__locals2);
							this.<WrapText>g__recordCurrentPos|51_0(ref CS$<>8__locals1, ref CS$<>8__locals2);
						}
					}
					CS$<>8__locals1.currentPos.X = CS$<>8__locals1.currentPos.X + advance;
					if (!char.IsWhiteSpace(CS$<>8__locals1.text[CS$<>8__locals2.i]))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
						defaultInterpolatedStringHandler.AppendFormatted<char>(CS$<>8__locals1.text[CS$<>8__locals2.i]);
						if (!TextManager.IsCJK(defaultInterpolatedStringHandler.ToStringAndClear()))
						{
							goto IL_1EB;
						}
					}
					CS$<>8__locals1.lastBreakerIndex = new int?(CS$<>8__locals2.i);
				}
				IL_1EB:
				int i = CS$<>8__locals2.i;
				CS$<>8__locals2.i = i + 1;
			}
			if (CS$<>8__locals1.requestCharPos >= CS$<>8__locals1.text.Length)
			{
				CS$<>8__locals1.foundCharPos = CS$<>8__locals1.currentPos;
			}
			if (CS$<>8__locals1.allCharPos != null)
			{
				CS$<>8__locals1.allCharPos[CS$<>8__locals1.text.Length] = CS$<>8__locals1.currentPos;
			}
			allCharPositions = CS$<>8__locals1.allCharPos;
			CS$<>8__locals1.result += CS$<>8__locals1.text.Substring(CS$<>8__locals1.currLineStart).Remove("\n", StringComparison.Ordinal);
			requestedCharPos = CS$<>8__locals1.foundCharPos;
			return CS$<>8__locals1.result;
		}

		// Token: 0x06000D1B RID: 3355 RVA: 0x00079817 File Offset: 0x00077A17
		public Vector2 MeasureString(LocalizedString str, bool removeExtraSpacing = false)
		{
			return this.MeasureString(str.Value, removeExtraSpacing);
		}

		// Token: 0x06000D1C RID: 3356 RVA: 0x00079828 File Offset: 0x00077A28
		public Vector2 MeasureString(string text, bool removeExtraSpacing = false)
		{
			if (text == null)
			{
				return Vector2.Zero;
			}
			float currentLineX = 0f;
			Vector2 retVal = Vector2.Zero;
			if (!removeExtraSpacing)
			{
				retVal.Y = this.LineHeight;
			}
			else
			{
				retVal.Y = (float)this.baseHeight;
			}
			if (this.DynamicLoading)
			{
				this.DynamicRenderAtlas(this.graphicsDevice, text, 1024, 84U);
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					currentLineX = 0f;
					retVal.Y += this.LineHeight;
				}
				else
				{
					uint charIndex = (uint)text[i];
					currentLineX += this.GetGlyphData(charIndex).Advance;
					retVal.X = Math.Max(retVal.X, currentLineX);
				}
			}
			return retVal;
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x000798EC File Offset: 0x00077AEC
		public Vector2 MeasureChar(char c)
		{
			Vector2 retVal = Vector2.Zero;
			retVal.Y = this.LineHeight;
			ScalableFont.GlyphData gd = this.GetGlyphDataAndTextureForChar(c).Item1;
			retVal.X = gd.Advance;
			return retVal;
		}

		// Token: 0x06000D1E RID: 3358 RVA: 0x00079928 File Offset: 0x00077B28
		[return: TupleElementNames(new string[]
		{
			"GlyphData",
			"Texture"
		})]
		public ValueTuple<ScalableFont.GlyphData, Texture2D> GetGlyphDataAndTextureForChar(char c)
		{
			if (this.DynamicLoading && !this.texCoords.ContainsKey((uint)c))
			{
				this.DynamicRenderAtlas(this.graphicsDevice, (uint)c, 1024, 84U);
			}
			ScalableFont.GlyphData gd = this.GetGlyphData((uint)c);
			Texture2D tex = (gd.TexIndex >= 0) ? this.textures[gd.TexIndex] : null;
			return new ValueTuple<ScalableFont.GlyphData, Texture2D>(gd, tex);
		}

		// Token: 0x06000D1F RID: 3359 RVA: 0x00079990 File Offset: 0x00077B90
		public void Dispose()
		{
			ScalableFont.FontList.Remove(this);
			foreach (Texture2D texture in this.textures)
			{
				texture.Dispose();
			}
			this.textures.Clear();
		}

		// Token: 0x06000D21 RID: 3361 RVA: 0x00079A23 File Offset: 0x00077C23
		[CompilerGenerated]
		private void <WrapText>g__recordCurrentPos|51_0(ref ScalableFont.<>c__DisplayClass51_0 A_1, ref ScalableFont.<>c__DisplayClass51_1 A_2)
		{
			if (A_2.i == A_1.requestCharPos)
			{
				A_1.foundCharPos = A_1.currentPos;
			}
			if (A_1.allCharPos != null)
			{
				A_1.allCharPos[A_2.i] = A_1.currentPos;
			}
		}

		// Token: 0x06000D22 RID: 3362 RVA: 0x00079A60 File Offset: 0x00077C60
		[CompilerGenerated]
		private void <WrapText>g__nextLine|51_1(ref ScalableFont.<>c__DisplayClass51_0 A_1, ref ScalableFont.<>c__DisplayClass51_1 A_2)
		{
			string result = A_1.result;
			string text = A_1.text;
			int currLineStart = A_1.currLineStart;
			A_1.result = result + text.Substring(currLineStart, A_2.i - currLineStart).Remove("\n", StringComparison.Ordinal) + "\n";
			A_1.lastBreakerIndex = null;
			A_1.currentPos.X = 0f;
			A_1.currentPos.Y = A_1.currentPos.Y + this.LineHeight;
			A_1.currLineStart = A_2.i;
		}

		// Token: 0x040006B9 RID: 1721
		private static readonly List<ScalableFont> FontList = new List<ScalableFont>();

		// Token: 0x040006BA RID: 1722
		private static Library Lib = null;

		// Token: 0x040006BB RID: 1723
		private static readonly object globalMutex = new object();

		// Token: 0x040006BC RID: 1724
		private readonly ReaderWriterLockSlim rwl;

		// Token: 0x040006BD RID: 1725
		private readonly string filename;

		// Token: 0x040006BE RID: 1726
		private readonly Face face;

		// Token: 0x040006BF RID: 1727
		private uint size;

		// Token: 0x040006C0 RID: 1728
		private int baseHeight;

		// Token: 0x040006C1 RID: 1729
		private readonly Dictionary<uint, ScalableFont.GlyphData> texCoords;

		// Token: 0x040006C2 RID: 1730
		private readonly List<Texture2D> textures;

		// Token: 0x040006C3 RID: 1731
		private readonly GraphicsDevice graphicsDevice;

		// Token: 0x040006C4 RID: 1732
		private Vector2 currentDynamicAtlasCoords;

		// Token: 0x040006C5 RID: 1733
		private int currentDynamicAtlasNextY;

		// Token: 0x040006C6 RID: 1734
		private uint[] currentDynamicPixelBuffer;

		// Token: 0x040006C9 RID: 1737
		public bool ForceUpperCase;

		// Token: 0x040006CA RID: 1738
		private uint[] charRanges;

		// Token: 0x040006CB RID: 1739
		private int texDims;

		// Token: 0x040006CC RID: 1740
		private uint baseChar;

		// Token: 0x040006CD RID: 1741
		private static readonly VertexPositionColorTexture[] quadVertices = new VertexPositionColorTexture[4];

		// Token: 0x02000816 RID: 2070
		public readonly struct GlyphData : IEquatable<ScalableFont.GlyphData>
		{
			// Token: 0x06006CBE RID: 27838 RVA: 0x0035FF62 File Offset: 0x0035E162
			public GlyphData(int TexIndex = 0, Vector2 DrawOffset = default(Vector2), float Advance = 0f, Rectangle TexCoords = default(Rectangle))
			{
				this.TexIndex = TexIndex;
				this.DrawOffset = DrawOffset;
				this.Advance = Advance;
				this.TexCoords = TexCoords;
			}

			// Token: 0x17001A14 RID: 6676
			// (get) Token: 0x06006CBF RID: 27839 RVA: 0x0035FF81 File Offset: 0x0035E181
			// (set) Token: 0x06006CC0 RID: 27840 RVA: 0x0035FF89 File Offset: 0x0035E189
			public int TexIndex { get; set; }

			// Token: 0x17001A15 RID: 6677
			// (get) Token: 0x06006CC1 RID: 27841 RVA: 0x0035FF92 File Offset: 0x0035E192
			// (set) Token: 0x06006CC2 RID: 27842 RVA: 0x0035FF9A File Offset: 0x0035E19A
			public Vector2 DrawOffset { get; set; }

			// Token: 0x17001A16 RID: 6678
			// (get) Token: 0x06006CC3 RID: 27843 RVA: 0x0035FFA3 File Offset: 0x0035E1A3
			// (set) Token: 0x06006CC4 RID: 27844 RVA: 0x0035FFAB File Offset: 0x0035E1AB
			public float Advance { get; set; }

			// Token: 0x17001A17 RID: 6679
			// (get) Token: 0x06006CC5 RID: 27845 RVA: 0x0035FFB4 File Offset: 0x0035E1B4
			// (set) Token: 0x06006CC6 RID: 27846 RVA: 0x0035FFBC File Offset: 0x0035E1BC
			public Rectangle TexCoords { get; set; }

			// Token: 0x06006CC7 RID: 27847 RVA: 0x0035FFC8 File Offset: 0x0035E1C8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("GlyphData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006CC8 RID: 27848 RVA: 0x00360014 File Offset: 0x0035E214
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("TexIndex = ");
				builder.Append(this.TexIndex.ToString());
				builder.Append(", DrawOffset = ");
				builder.Append(this.DrawOffset.ToString());
				builder.Append(", Advance = ");
				builder.Append(this.Advance.ToString());
				builder.Append(", TexCoords = ");
				builder.Append(this.TexCoords.ToString());
				return true;
			}

			// Token: 0x06006CC9 RID: 27849 RVA: 0x003600BE File Offset: 0x0035E2BE
			[CompilerGenerated]
			public static bool operator !=(ScalableFont.GlyphData left, ScalableFont.GlyphData right)
			{
				return !(left == right);
			}

			// Token: 0x06006CCA RID: 27850 RVA: 0x003600CA File Offset: 0x0035E2CA
			[CompilerGenerated]
			public static bool operator ==(ScalableFont.GlyphData left, ScalableFont.GlyphData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006CCB RID: 27851 RVA: 0x003600D4 File Offset: 0x0035E2D4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<int>.Default.GetHashCode(this.<TexIndex>k__BackingField) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<DrawOffset>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Advance>k__BackingField)) * -1521134295 + EqualityComparer<Rectangle>.Default.GetHashCode(this.<TexCoords>k__BackingField);
			}

			// Token: 0x06006CCC RID: 27852 RVA: 0x00360136 File Offset: 0x0035E336
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ScalableFont.GlyphData && this.Equals((ScalableFont.GlyphData)obj);
			}

			// Token: 0x06006CCD RID: 27853 RVA: 0x00360150 File Offset: 0x0035E350
			[CompilerGenerated]
			public bool Equals(ScalableFont.GlyphData other)
			{
				return EqualityComparer<int>.Default.Equals(this.<TexIndex>k__BackingField, other.<TexIndex>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<DrawOffset>k__BackingField, other.<DrawOffset>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Advance>k__BackingField, other.<Advance>k__BackingField) && EqualityComparer<Rectangle>.Default.Equals(this.<TexCoords>k__BackingField, other.<TexCoords>k__BackingField);
			}

			// Token: 0x06006CCE RID: 27854 RVA: 0x003601BD File Offset: 0x0035E3BD
			[CompilerGenerated]
			public void Deconstruct(out int TexIndex, out Vector2 DrawOffset, out float Advance, out Rectangle TexCoords)
			{
				TexIndex = this.TexIndex;
				DrawOffset = this.DrawOffset;
				Advance = this.Advance;
				TexCoords = this.TexCoords;
			}
		}
	}
}
