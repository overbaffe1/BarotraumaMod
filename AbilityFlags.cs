using System;

namespace Barotrauma
{
	// Token: 0x0200026B RID: 619
	[Flags]
	public enum AbilityFlags
	{
		// Token: 0x04001D12 RID: 7442
		None = 0,
		// Token: 0x04001D13 RID: 7443
		MustWalk = 1,
		// Token: 0x04001D14 RID: 7444
		ImmuneToPressure = 2,
		// Token: 0x04001D15 RID: 7445
		IgnoredByEnemyAI = 4,
		// Token: 0x04001D16 RID: 7446
		MoveNormallyWhileDragging = 8,
		// Token: 0x04001D17 RID: 7447
		CanTinker = 16,
		// Token: 0x04001D18 RID: 7448
		CanTinkerFabricatorsAndDeconstructors = 32,
		// Token: 0x04001D19 RID: 7449
		TinkeringPowersDevices = 64,
		// Token: 0x04001D1A RID: 7450
		GainSkillPastMaximum = 128,
		// Token: 0x04001D1B RID: 7451
		RetainExperienceForNewCharacter = 256,
		// Token: 0x04001D1C RID: 7452
		AllowSecondOrderedTarget = 512,
		// Token: 0x04001D1D RID: 7453
		AlwaysStayConscious = 1024,
		// Token: 0x04001D1E RID: 7454
		CanNotDieToAfflictions = 2048
	}
}
