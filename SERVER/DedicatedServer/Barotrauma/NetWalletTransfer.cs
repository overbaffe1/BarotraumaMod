using System;

namespace Barotrauma
{
	// Token: 0x020001DB RID: 475
	[NetworkSerialize(42)]
	internal struct NetWalletTransfer : INetSerializableStruct
	{
		// Token: 0x040010AC RID: 4268
		public Option<ushort> Sender;

		// Token: 0x040010AD RID: 4269
		public Option<ushort> Receiver;

		// Token: 0x040010AE RID: 4270
		public int Amount;
	}
}
