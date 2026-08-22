using System;

namespace Barotrauma
{
	// Token: 0x020001DF RID: 479
	internal sealed class InvalidWallet : Wallet
	{
		// Token: 0x060022B0 RID: 8880 RVA: 0x000E8D63 File Offset: 0x000E6F63
		public InvalidWallet() : base(Option<Character>.None())
		{
		}

		// Token: 0x170009F0 RID: 2544
		// (get) Token: 0x060022B1 RID: 8881 RVA: 0x000E8D70 File Offset: 0x000E6F70
		// (set) Token: 0x060022B2 RID: 8882 RVA: 0x000E8D73 File Offset: 0x000E6F73
		public override int Balance
		{
			get
			{
				return 0;
			}
			set
			{
				throw new InvalidOperationException("Tried to set the balance on an invalid wallet");
			}
		}

		// Token: 0x170009F1 RID: 2545
		// (get) Token: 0x060022B3 RID: 8883 RVA: 0x000E8D7F File Offset: 0x000E6F7F
		// (set) Token: 0x060022B4 RID: 8884 RVA: 0x000E8D82 File Offset: 0x000E6F82
		public override int RewardDistribution
		{
			get
			{
				return 0;
			}
			set
			{
				throw new InvalidOperationException("Tried to set the reward distribution on an invalid wallet");
			}
		}
	}
}
