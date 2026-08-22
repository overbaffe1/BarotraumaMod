using System;

namespace Barotrauma
{
	// Token: 0x020001D9 RID: 473
	[NetworkSerialize(23)]
	internal struct WalletInfo : INetSerializableStruct
	{
		// Token: 0x040010A9 RID: 4265
		public int RewardDistribution;

		// Token: 0x040010AA RID: 4266
		public int Balance;
	}
}
