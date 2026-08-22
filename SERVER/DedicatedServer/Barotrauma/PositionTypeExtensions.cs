using System;

namespace Barotrauma
{
	// Token: 0x02000239 RID: 569
	internal static class PositionTypeExtensions
	{
		// Token: 0x060026E5 RID: 9957 RVA: 0x000FF650 File Offset: 0x000FD850
		public static bool IsEnclosedArea(this Level.PositionType positionType)
		{
			return positionType == Level.PositionType.Cave || positionType == Level.PositionType.AbyssCave || positionType.IsIndoorsArea();
		}

		// Token: 0x060026E6 RID: 9958 RVA: 0x000FF666 File Offset: 0x000FD866
		public static bool IsIndoorsArea(this Level.PositionType positionType)
		{
			return positionType == Level.PositionType.Outpost || positionType == Level.PositionType.BeaconStation || positionType == Level.PositionType.Ruin || positionType == Level.PositionType.Wreck;
		}
	}
}
