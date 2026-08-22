using System;

namespace Barotrauma
{
	// Token: 0x020001DC RID: 476
	internal struct NetWalletSetSalaryUpdate : INetSerializableStruct
	{
		// Token: 0x040010AF RID: 4271
		[NetworkSerialize(55)]
		public Option<ushort> Target;

		// Token: 0x040010B0 RID: 4272
		[NetworkSerialize(58, MinValueInt = 0, MaxValueInt = 100)]
		public int NewRewardDistribution;
	}
}
