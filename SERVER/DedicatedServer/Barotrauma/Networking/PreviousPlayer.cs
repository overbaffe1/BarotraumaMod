using System;
using System.Collections.Generic;

namespace Barotrauma.Networking
{
	// Token: 0x0200036D RID: 877
	internal class PreviousPlayer
	{
		// Token: 0x06003464 RID: 13412 RVA: 0x00168AD0 File Offset: 0x00166CD0
		public PreviousPlayer(Client c)
		{
			this.Name = c.Name;
			this.Address = c.Connection.Endpoint.Address;
			this.AccountInfo = c.AccountInfo;
		}

		// Token: 0x06003465 RID: 13413 RVA: 0x00168B1C File Offset: 0x00166D1C
		public bool MatchesClient(Client c)
		{
			if (c.AccountInfo.AccountId.IsSome() && this.AccountInfo.AccountId.IsSome())
			{
				return c.AccountInfo.AccountId == this.AccountInfo.AccountId;
			}
			return c.AddressMatches(this.Address);
		}

		// Token: 0x04001A0D RID: 6669
		public string Name;

		// Token: 0x04001A0E RID: 6670
		public Address Address;

		// Token: 0x04001A0F RID: 6671
		public AccountInfo AccountInfo;

		// Token: 0x04001A10 RID: 6672
		public float Karma;

		// Token: 0x04001A11 RID: 6673
		public int KarmaKickCount;

		// Token: 0x04001A12 RID: 6674
		public readonly List<Client> KickVoters = new List<Client>();
	}
}
