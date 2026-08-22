using System;

namespace Barotrauma
{
	// Token: 0x020002C6 RID: 710
	[NetworkSerialize(23)]
	internal struct WalletInfo : INetSerializableStruct
	{
		// Token: 0x04001F45 RID: 8005
		public int RewardDistribution;

		// Token: 0x04001F46 RID: 8006
		public int Balance;
	}
}
