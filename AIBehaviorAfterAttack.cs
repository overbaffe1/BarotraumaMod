using System;

namespace Barotrauma
{
	// Token: 0x020001AD RID: 429
	public enum AIBehaviorAfterAttack
	{
		// Token: 0x04001971 RID: 6513
		FallBack,
		// Token: 0x04001972 RID: 6514
		FallBackUntilCanAttack,
		// Token: 0x04001973 RID: 6515
		PursueIfCanAttack,
		// Token: 0x04001974 RID: 6516
		Pursue,
		// Token: 0x04001975 RID: 6517
		Eat,
		// Token: 0x04001976 RID: 6518
		FollowThrough,
		// Token: 0x04001977 RID: 6519
		FollowThroughWithoutObstacleAvoidance,
		// Token: 0x04001978 RID: 6520
		FollowThroughUntilCanAttack,
		// Token: 0x04001979 RID: 6521
		IdleUntilCanAttack,
		// Token: 0x0400197A RID: 6522
		Reverse,
		// Token: 0x0400197B RID: 6523
		ReverseUntilCanAttack
	}
}
