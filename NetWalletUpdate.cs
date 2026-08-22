using System;

namespace Barotrauma
{
	// Token: 0x020002C7 RID: 711
	internal struct NetWalletUpdate : INetSerializableStruct
	{
		// Token: 0x04001F47 RID: 8007
		[NetworkSerialize(35, ArrayMaxSize = 256)]
		public NetWalletTransaction[] Transactions;
	}
}
