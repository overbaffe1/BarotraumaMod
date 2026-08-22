using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200004F RID: 79
	public static class XnaToConsoleColor
	{
		// Token: 0x06000C01 RID: 3073 RVA: 0x00071EF8 File Offset: 0x000700F8
		public static ConsoleColor Convert(Color xnaCol)
		{
			if (XnaToConsoleColor.dictionary == null)
			{
				XnaToConsoleColor.dictionary = new Dictionary<Color, ConsoleColor>
				{
					{
						Color.White,
						ConsoleColor.White
					},
					{
						Color.Gray,
						ConsoleColor.Gray
					},
					{
						Color.LightGray,
						ConsoleColor.Gray
					},
					{
						Color.DarkGray,
						ConsoleColor.Gray
					},
					{
						Color.Red,
						ConsoleColor.Red
					},
					{
						Color.DarkRed,
						ConsoleColor.DarkRed
					},
					{
						Color.Yellow,
						ConsoleColor.Yellow
					},
					{
						Color.Orange,
						ConsoleColor.Yellow
					},
					{
						Color.Green,
						ConsoleColor.Green
					},
					{
						Color.Lime,
						ConsoleColor.Green
					},
					{
						Color.Blue,
						ConsoleColor.Blue
					},
					{
						Color.Cyan,
						ConsoleColor.Cyan
					},
					{
						Color.DarkBlue,
						ConsoleColor.DarkBlue
					},
					{
						Color.Pink,
						ConsoleColor.Magenta
					},
					{
						Color.Magenta,
						ConsoleColor.Magenta
					}
				};
			}
			ConsoleColor val = ConsoleColor.White;
			if (XnaToConsoleColor.dictionary.TryGetValue(xnaCol, out val))
			{
				return val;
			}
			return XnaToConsoleColor.GetClosestConsoleColor(xnaCol);
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x00071FF4 File Offset: 0x000701F4
		public static ConsoleColor GetClosestConsoleColor(Color color)
		{
			Vector3 hls = ToolBox.RgbToHLS(color.ToVector3());
			if ((double)hls.Z < 0.5)
			{
				switch ((int)((double)hls.Y * 3.5))
				{
				case 0:
					return ConsoleColor.Black;
				case 1:
					return ConsoleColor.DarkGray;
				case 2:
					return ConsoleColor.Gray;
				default:
					return ConsoleColor.White;
				}
			}
			else
			{
				int hue = (int)Math.Round((double)(hls.X / 60f), MidpointRounding.AwayFromZero);
				if ((double)hls.Y < 0.4)
				{
					switch (hue)
					{
					case 1:
						return ConsoleColor.DarkYellow;
					case 2:
						return ConsoleColor.DarkGreen;
					case 3:
						return ConsoleColor.DarkCyan;
					case 4:
						return ConsoleColor.DarkBlue;
					case 5:
						return ConsoleColor.DarkMagenta;
					default:
						return ConsoleColor.DarkRed;
					}
				}
				else
				{
					switch (hue)
					{
					case 1:
						return ConsoleColor.Yellow;
					case 2:
						return ConsoleColor.Green;
					case 3:
						return ConsoleColor.Cyan;
					case 4:
						return ConsoleColor.Blue;
					case 5:
						return ConsoleColor.Magenta;
					default:
						return ConsoleColor.Red;
					}
				}
			}
		}

		// Token: 0x04000534 RID: 1332
		private static Dictionary<Color, ConsoleColor> dictionary;
	}
}
