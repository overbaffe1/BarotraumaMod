using System;

namespace Barotrauma
{
	// Token: 0x020000AA RID: 170
	public enum AIBehaviorAfterAttack
	{
		// Token: 0x040009C3 RID: 2499
		FallBack,
		// Token: 0x040009C4 RID: 2500
		FallBackUntilCanAttack,
		// Token: 0x040009C5 RID: 2501
		PursueIfCanAttack,
		// Token: 0x040009C6 RID: 2502
		Pursue,
		// Token: 0x040009C7 RID: 2503
		Eat,
		// Token: 0x040009C8 RID: 2504
		FollowThrough,
		// Token: 0x040009C9 RID: 2505
		FollowThroughWithoutObstacleAvoidance,
		// Token: 0x040009CA RID: 2506
		FollowThroughUntilCanAttack,
		// Token: 0x040009CB RID: 2507
		IdleUntilCanAttack,
		// Token: 0x040009CC RID: 2508
		Reverse,
		// Token: 0x040009CD RID: 2509
		ReverseUntilCanAttack
	}
}
