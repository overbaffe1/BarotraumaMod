using System;

namespace Barotrauma
{
	// Token: 0x020002CC RID: 716
	internal sealed class InvalidWallet : Wallet
	{
		// Token: 0x06003CB2 RID: 15538 RVA: 0x0022BDC3 File Offset: 0x00229FC3
		public InvalidWallet() : base(Option<Character>.None())
		{
		}

		// Token: 0x17000FFC RID: 4092
		// (get) Token: 0x06003CB3 RID: 15539 RVA: 0x0022BDD0 File Offset: 0x00229FD0
		// (set) Token: 0x06003CB4 RID: 15540 RVA: 0x0022BDD3 File Offset: 0x00229FD3
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

		// Token: 0x17000FFD RID: 4093
		// (get) Token: 0x06003CB5 RID: 15541 RVA: 0x0022BDDF File Offset: 0x00229FDF
		// (set) Token: 0x06003CB6 RID: 15542 RVA: 0x0022BDE2 File Offset: 0x00229FE2
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
