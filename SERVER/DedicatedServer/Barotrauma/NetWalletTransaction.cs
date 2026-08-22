using System;

namespace Barotrauma
{
	// Token: 0x020001DE RID: 478
	[NetworkSerialize(104)]
	internal struct NetWalletTransaction : INetSerializableStruct
	{
		// Token: 0x040010B3 RID: 4275
		public Option<ushort> CharacterID;

		// Token: 0x040010B4 RID: 4276
		public WalletChangedData ChangedData;

		// Token: 0x040010B5 RID: 4277
		public WalletInfo Info;
	}
}
