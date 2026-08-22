using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005EB RID: 1515
	internal static class GrowthSideExtension
	{
		// Token: 0x06006342 RID: 25410 RVA: 0x0033B740 File Offset: 0x00339940
		public static int Count(this TileSide side)
		{
			int i = (int)side;
			int count = 0;
			while (i != 0)
			{
				count += (i & 1);
				i >>= 1;
			}
			return count;
		}

		// Token: 0x06006343 RID: 25411 RVA: 0x0033B764 File Offset: 0x00339964
		public static TileSide GetOppositeSide(this TileSide side)
		{
			switch (side)
			{
			case TileSide.Top:
				return TileSide.Bottom;
			case TileSide.Left:
				return TileSide.Right;
			case TileSide.Top | TileSide.Left:
				break;
			case TileSide.Bottom:
				return TileSide.Top;
			default:
				if (side == TileSide.Right)
				{
					return TileSide.Left;
				}
				break;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Expected Left, Right, Bottom or Top, got ");
			defaultInterpolatedStringHandler.AppendFormatted<TileSide>(side);
			throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
		}
	}
}
