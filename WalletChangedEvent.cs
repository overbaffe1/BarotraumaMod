using System;

namespace Barotrauma
{
	// Token: 0x020002C5 RID: 709
	internal readonly struct WalletChangedEvent
	{
		// Token: 0x06003CAE RID: 15534 RVA: 0x0022BCC4 File Offset: 0x00229EC4
		public WalletChangedEvent(Wallet wallet, WalletChangedData changedData, WalletInfo info)
		{
			this.Wallet = wallet;
			this.Info = info;
			this.ChangedData = changedData;
			this.Owner = wallet.Owner;
		}

		// Token: 0x04001F41 RID: 8001
		public readonly Option<Character> Owner;

		// Token: 0x04001F42 RID: 8002
		public readonly Wallet Wallet;

		// Token: 0x04001F43 RID: 8003
		public readonly WalletInfo Info;

		// Token: 0x04001F44 RID: 8004
		public readonly WalletChangedData ChangedData;
	}
}
