using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000478 RID: 1144
	[Flags]
	public enum ClientPermissions
	{
		// Token: 0x040028D7 RID: 10455
		None = 0,
		// Token: 0x040028D8 RID: 10456
		ManageRound = 1,
		// Token: 0x040028D9 RID: 10457
		Kick = 2,
		// Token: 0x040028DA RID: 10458
		Ban = 4,
		// Token: 0x040028DB RID: 10459
		Unban = 8,
		// Token: 0x040028DC RID: 10460
		SelectSub = 16,
		// Token: 0x040028DD RID: 10461
		SelectMode = 32,
		// Token: 0x040028DE RID: 10462
		ManageCampaign = 64,
		// Token: 0x040028DF RID: 10463
		ConsoleCommands = 128,
		// Token: 0x040028E0 RID: 10464
		ServerLog = 256,
		// Token: 0x040028E1 RID: 10465
		ManageSettings = 512,
		// Token: 0x040028E2 RID: 10466
		ManagePermissions = 1024,
		// Token: 0x040028E3 RID: 10467
		KarmaImmunity = 2048,
		// Token: 0x040028E4 RID: 10468
		ManageMoney = 4096,
		// Token: 0x040028E5 RID: 10469
		SellInventoryItems = 8192,
		// Token: 0x040028E6 RID: 10470
		SellSubItems = 16384,
		// Token: 0x040028E7 RID: 10471
		ManageMap = 32768,
		// Token: 0x040028E8 RID: 10472
		ManageHires = 65536,
		// Token: 0x040028E9 RID: 10473
		ManageBotTalents = 131072,
		// Token: 0x040028EA RID: 10474
		SpamImmunity = 262144,
		// Token: 0x040028EB RID: 10475
		All = 524287
	}
}
