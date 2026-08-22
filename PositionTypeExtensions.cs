using System;

namespace Barotrauma
{
	// Token: 0x0200031B RID: 795
	internal static class PositionTypeExtensions
	{
		// Token: 0x06003F3A RID: 16186 RVA: 0x00235F7F File Offset: 0x0023417F
		public static bool IsEnclosedArea(this Level.PositionType positionType)
		{
			return positionType == Level.PositionType.Cave || positionType == Level.PositionType.AbyssCave || positionType.IsIndoorsArea();
		}

		// Token: 0x06003F3B RID: 16187 RVA: 0x00235F95 File Offset: 0x00234195
		public static bool IsIndoorsArea(this Level.PositionType positionType)
		{
			return positionType == Level.PositionType.Outpost || positionType == Level.PositionType.BeaconStation || positionType == Level.PositionType.Ruin || positionType == Level.PositionType.Wreck;
		}
	}
}
