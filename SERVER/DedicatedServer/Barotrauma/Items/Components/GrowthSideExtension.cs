using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B8 RID: 1208
	internal static class GrowthSideExtension
	{
		// Token: 0x0600449E RID: 17566 RVA: 0x001B7B40 File Offset: 0x001B5D40
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

		// Token: 0x0600449F RID: 17567 RVA: 0x001B7B64 File Offset: 0x001B5D64
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
