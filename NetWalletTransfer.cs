using System;

namespace Barotrauma
{
	// Token: 0x020002C8 RID: 712
	[NetworkSerialize(42)]
	internal struct NetWalletTransfer : INetSerializableStruct
	{
		// Token: 0x04001F48 RID: 8008
		public Option<ushort> Sender;

		// Token: 0x04001F49 RID: 8009
		public Option<ushort> Receiver;

		// Token: 0x04001F4A RID: 8010
		public int Amount;
	}
}
