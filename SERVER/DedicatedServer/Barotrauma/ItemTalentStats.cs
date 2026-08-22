using System;

namespace Barotrauma
{
	// Token: 0x02000176 RID: 374
	internal enum ItemTalentStats
	{
		// Token: 0x04000E0E RID: 3598
		None,
		// Token: 0x04000E0F RID: 3599
		DetoriationSpeed,
		// Token: 0x04000E10 RID: 3600
		BatteryCapacity,
		// Token: 0x04000E11 RID: 3601
		EngineSpeed,
		// Token: 0x04000E12 RID: 3602
		EngineMaxSpeed,
		// Token: 0x04000E13 RID: 3603
		PumpSpeed,
		// Token: 0x04000E14 RID: 3604
		ReactorMaxOutput,
		// Token: 0x04000E15 RID: 3605
		ReactorFuelConsumption,
		// Token: 0x04000E16 RID: 3606
		DeconstructorSpeed,
		// Token: 0x04000E17 RID: 3607
		FabricationSpeed,
		// Token: 0x04000E18 RID: 3608
		ExtraStackSize,
		// Token: 0x04000E19 RID: 3609
		[Obsolete("Use PumpSpeed instead.")]
		PumpMaxFlow = 5
	}
}
