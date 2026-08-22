using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.IO;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;

namespace Barotrauma
{
	// Token: 0x02000158 RID: 344
	public static class TextureLoader
	{
		// Token: 0x17000ABB RID: 2747
		// (get) Token: 0x06002A3D RID: 10813 RVA: 0x001D2F4A File Offset: 0x001D114A
		// (set) Token: 0x06002A3E RID: 10814 RVA: 0x001D2F51 File Offset: 0x001D1151
		public static Texture2D PlaceHolderTexture { get; private set; }

		// Token: 0x06002A3F RID: 10815 RVA: 0x001D2F5C File Offset: 0x001D115C
		public static void Init(GraphicsDevice graphicsDevice, bool needsBmp = false)
		{
			TextureLoader._graphicsDevice = graphicsDevice;
			Color[] data = new Color[1024];
			for (int i = 0; i < 1024; i++)
			{
				data[i] = Color.Magenta;
			}
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				TextureLoader.PlaceHolderTexture = new Texture2D(graphicsDevice, 32, 32);
				TextureLoader.PlaceHolderTexture.SetData<Color>(data);
			});
		}

		// Token: 0x06002A40 RID: 10816 RVA: 0x001D2FC3 File Offset: 0x001D11C3
		public static void CancelAll()
		{
			TextureLoader.cancelAll = true;
		}

		// Token: 0x06002A41 RID: 10817 RVA: 0x001D2FD0 File Offset: 0x001D11D0
		private static byte[] CompressDxt5(byte[] data, int width, int height)
		{
			byte[] output = new byte[width * height];
			Parallel.For(0, width * height / 16, delegate(int i)
			{
				int i2 = i * 4;
				int inputOffset = (i2 % width + i2 / width * 4 * width) * 4;
				int outputOffset = i * 16;
				TextureLoader.CompressDxt5Block(data, inputOffset, width, output, outputOffset);
			});
			return output;
		}

		// Token: 0x06002A42 RID: 10818 RVA: 0x001D3028 File Offset: 0x001D1228
		private static void CompressDxt5Block(byte[] data, int inputOffset, int width, byte[] output, int outputOffset)
		{
			int r = 255;
			int g = 255;
			int b = 255;
			int a = 255;
			int r2 = 0;
			int g2 = 0;
			int b2 = 0;
			int a2 = 0;
			int y = 255000;
			int y2 = 0;
			for (int i = 0; i < 16; i++)
			{
				int pixelOffset = inputOffset + 4 * (i % 4 + width * (i >> 2));
				int r3 = (int)data[pixelOffset];
				int g3 = (int)data[pixelOffset + 1];
				int b3 = (int)data[pixelOffset + 2];
				int a3 = (int)data[pixelOffset + 3];
				int y3 = r3 * 299 + g3 * 587 + b3 * 114;
				if (y3 < y)
				{
					r = r3;
					g = g3;
					b = b3;
					y = y3;
				}
				if (y3 > y2)
				{
					r2 = r3;
					g2 = g3;
					b2 = b3;
					y2 = y3;
				}
				if (a3 < a)
				{
					a = a3;
				}
				if (a3 > a2)
				{
					a2 = a3;
				}
			}
			int r1_565 = r >> 3;
			int g1_565 = g >> 2;
			int b1_565 = b >> 3;
			int r2_565 = r2 >> 3;
			int g2_565 = g2 >> 2;
			int b2_565 = b2 >> 3;
			int y2y1Diff = y2 - y;
			if (y2y1Diff > 0 || a < a2)
			{
				for (int j = 0; j < 16; j++)
				{
					int pixelOffset2 = inputOffset + 4 * (j % 4 + width * (j >> 2));
					int r4 = (int)data[pixelOffset2];
					int g4 = (int)data[pixelOffset2 + 1];
					int b4 = (int)data[pixelOffset2 + 2];
					if (a < a2)
					{
						int a4 = (int)data[pixelOffset2 + 3];
						a4 -= a;
						a4 = a4 * 7 / (a2 - a);
						if (a4 < 7)
						{
							int num;
							if (a4 != 0)
							{
								if (a4 != 1)
								{
									num = 8 - a4;
								}
								else
								{
									num = 7;
								}
							}
							else
							{
								num = 1;
							}
							a4 = num;
							NetBitWriter.WriteByte((byte)a4, 3, output, outputOffset * 8 + 16 + j * 3);
						}
					}
					if (y2y1Diff > 0)
					{
						int y4 = r4 * 299 + g4 * 587 + b4 * 114;
						int diffY = y4 - y;
						int num;
						switch (diffY * 4 / y2y1Diff)
						{
						case 0:
							num = 0;
							break;
						case 1:
							num = 2;
							break;
						case 2:
							num = 3;
							break;
						default:
							num = 1;
							break;
						}
						int paletteIndex = num;
						int num2 = outputOffset + 12 + j / 4;
						output[num2] |= (byte)(paletteIndex << 2 * (j % 4));
					}
				}
			}
			output[outputOffset] = (byte)a2;
			output[outputOffset + 1] = (byte)a;
			output[outputOffset + 9] = (byte)(r1_565 << 3 | g1_565 >> 3);
			output[outputOffset + 8] = (byte)(g1_565 << 5 | b1_565);
			output[outputOffset + 11] = (byte)(r2_565 << 3 | g2_565 >> 3);
			output[outputOffset + 10] = (byte)(g2_565 << 5 | b2_565);
		}

		// Token: 0x06002A43 RID: 10819 RVA: 0x001D3294 File Offset: 0x001D1494
		public static Texture2D FromFile(string path, bool compress = true, bool mipmap = false, ContentPackage contentPackage = null)
		{
			Texture2D result;
			using (FileStream fileStream = File.OpenRead(path, true))
			{
				result = TextureLoader.FromStream(fileStream, path, compress, mipmap, contentPackage);
			}
			return result;
		}

		// Token: 0x06002A44 RID: 10820 RVA: 0x001D32D4 File Offset: 0x001D14D4
		public static Texture2D FromStream(Stream stream, string path = null, bool compress = true, bool mipmap = false, ContentPackage contentPackage = null)
		{
			Texture2D result;
			try
			{
				path = path.CleanUpPath();
				byte[] textureData = null;
				int width;
				int height;
				int channels;
				textureData = Texture2D.TextureDataFromStream(stream, out width, out height, out channels);
				SurfaceFormat format = SurfaceFormat.Color;
				if (GameSettings.CurrentConfig.Graphics.CompressTextures && compress)
				{
					if ((width & 3) == 0 && (height & 3) == 0)
					{
						textureData = TextureLoader.CompressDxt5(textureData, width, height);
						format = SurfaceFormat.Dxt5;
						mipmap = false;
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Could not compress a texture because the dimensions aren't a multiple of 4 (path: ");
						defaultInterpolatedStringHandler.AppendFormatted(path ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(", size: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(width);
						defaultInterpolatedStringHandler.AppendLiteral("x");
						defaultInterpolatedStringHandler.AppendFormatted<int>(height);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), contentPackage);
					}
				}
				Texture2D tex = null;
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					if (TextureLoader.cancelAll)
					{
						return;
					}
					tex = new Texture2D(TextureLoader._graphicsDevice, width, height, mipmap, format);
					tex.SetData<byte>(textureData);
				});
				result = tex;
			}
			catch (Exception e)
			{
				if (e is SharpDXException)
				{
					throw;
				}
				DebugConsole.ThrowError(string.IsNullOrEmpty(path) ? "Loading texture from stream failed!" : ("Loading texture \"" + path + "\" failed!"), e, null, false, false);
				result = null;
			}
			return result;
		}

		// Token: 0x0400161A RID: 5658
		private static volatile bool cancelAll;

		// Token: 0x0400161B RID: 5659
		private static GraphicsDevice _graphicsDevice;
	}
}
