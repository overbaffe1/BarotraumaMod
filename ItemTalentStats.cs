using System;

namespace Barotrauma
{
	// Token: 0x0200026A RID: 618
	internal enum ItemTalentStats
	{
		// Token: 0x04001D05 RID: 7429
		None,
		// Token: 0x04001D06 RID: 7430
		DetoriationSpeed,
		// Token: 0x04001D07 RID: 7431
		BatteryCapacity,
		// Token: 0x04001D08 RID: 7432
		EngineSpeed,
		// Token: 0x04001D09 RID: 7433
		EngineMaxSpeed,
		// Token: 0x04001D0A RID: 7434
		PumpSpeed,
		// Token: 0x04001D0B RID: 7435
		ReactorMaxOutput,
		// Token: 0x04001D0C RID: 7436
		ReactorFuelConsumption,
		// Token: 0x04001D0D RID: 7437
		DeconstructorSpeed,
		// Token: 0x04001D0E RID: 7438
		FabricationSpeed,
		// Token: 0x04001D0F RID: 7439
		ExtraStackSize,
		// Token: 0x04001D10 RID: 7440
		[Obsolete("Use PumpSpeed instead.")]
		PumpMaxFlow = 5
	}
}
