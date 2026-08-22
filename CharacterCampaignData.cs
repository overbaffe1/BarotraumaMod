using System;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020002D0 RID: 720
	internal class CharacterCampaignData
	{
		// Token: 0x17001019 RID: 4121
		// (get) Token: 0x06003CEE RID: 15598 RVA: 0x0022C7BB File Offset: 0x0022A9BB
		// (set) Token: 0x06003CEF RID: 15599 RVA: 0x0022C7C3 File Offset: 0x0022A9C3
		public XElement OrderData { get; private set; }

		// Token: 0x04001F79 RID: 8057
		public readonly CharacterInfo CharacterInfo;

		// Token: 0x04001F7A RID: 8058
		public readonly string Name;

		// Token: 0x04001F7B RID: 8059
		public readonly Address ClientAddress;

		// Token: 0x04001F7C RID: 8060
		public readonly Option<AccountId> AccountId;

		// Token: 0x04001F7D RID: 8061
		private XElement itemData;

		// Token: 0x04001F7E RID: 8062
		private XElement healthData;

		// Token: 0x04001F80 RID: 8064
		public XElement WalletData;
	}
}
