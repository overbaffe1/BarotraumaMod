using System;

namespace Barotrauma
{
	// Token: 0x02000177 RID: 375
	[Flags]
	public enum AbilityFlags
	{
		// Token: 0x04000E1B RID: 3611
		None = 0,
		// Token: 0x04000E1C RID: 3612
		MustWalk = 1,
		// Token: 0x04000E1D RID: 3613
		ImmuneToPressure = 2,
		// Token: 0x04000E1E RID: 3614
		IgnoredByEnemyAI = 4,
		// Token: 0x04000E1F RID: 3615
		MoveNormallyWhileDragging = 8,
		// Token: 0x04000E20 RID: 3616
		CanTinker = 16,
		// Token: 0x04000E21 RID: 3617
		CanTinkerFabricatorsAndDeconstructors = 32,
		// Token: 0x04000E22 RID: 3618
		TinkeringPowersDevices = 64,
		// Token: 0x04000E23 RID: 3619
		GainSkillPastMaximum = 128,
		// Token: 0x04000E24 RID: 3620
		RetainExperienceForNewCharacter = 256,
		// Token: 0x04000E25 RID: 3621
		AllowSecondOrderedTarget = 512,
		// Token: 0x04000E26 RID: 3622
		AlwaysStayConscious = 1024,
		// Token: 0x04000E27 RID: 3623
		CanNotDieToAfflictions = 2048
	}
}
