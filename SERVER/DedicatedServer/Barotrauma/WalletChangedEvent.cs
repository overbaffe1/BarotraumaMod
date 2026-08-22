using System;

namespace Barotrauma
{
	// Token: 0x020001D8 RID: 472
	internal readonly struct WalletChangedEvent
	{
		// Token: 0x060022AC RID: 8876 RVA: 0x000E8C64 File Offset: 0x000E6E64
		public WalletChangedEvent(Wallet wallet, WalletChangedData changedData, WalletInfo info)
		{
			this.Wallet = wallet;
			this.Info = info;
			this.ChangedData = changedData;
			this.Owner = wallet.Owner;
		}

		// Token: 0x040010A5 RID: 4261
		public readonly Option<Character> Owner;

		// Token: 0x040010A6 RID: 4262
		public readonly Wallet Wallet;

		// Token: 0x040010A7 RID: 4263
		public readonly WalletInfo Info;

		// Token: 0x040010A8 RID: 4264
		public readonly WalletChangedData ChangedData;
	}
}
