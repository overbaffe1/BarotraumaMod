using System;

namespace Barotrauma
{
	// Token: 0x020001DA RID: 474
	internal struct NetWalletUpdate : INetSerializableStruct
	{
		// Token: 0x040010AB RID: 4267
		[NetworkSerialize(35, ArrayMaxSize = 256)]
		public NetWalletTransaction[] Transactions;
	}
}
