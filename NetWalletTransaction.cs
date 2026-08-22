using System;

namespace Barotrauma
{
	// Token: 0x020002CB RID: 715
	[NetworkSerialize(104)]
	internal struct NetWalletTransaction : INetSerializableStruct
	{
		// Token: 0x04001F4F RID: 8015
		public Option<ushort> CharacterID;

		// Token: 0x04001F50 RID: 8016
		public WalletChangedData ChangedData;

		// Token: 0x04001F51 RID: 8017
		public WalletInfo Info;
	}
}
