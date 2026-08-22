using System;

namespace Barotrauma
{
	// Token: 0x020002C9 RID: 713
	internal struct NetWalletSetSalaryUpdate : INetSerializableStruct
	{
		// Token: 0x04001F4B RID: 8011
		[NetworkSerialize(55)]
		public Option<ushort> Target;

		// Token: 0x04001F4C RID: 8012
		[NetworkSerialize(58, MinValueInt = 0, MaxValueInt = 100)]
		public int NewRewardDistribution;
	}
}
