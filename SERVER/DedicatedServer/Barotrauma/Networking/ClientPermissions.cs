using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200037C RID: 892
	[Flags]
	public enum ClientPermissions
	{
		// Token: 0x04001AEC RID: 6892
		None = 0,
		// Token: 0x04001AED RID: 6893
		ManageRound = 1,
		// Token: 0x04001AEE RID: 6894
		Kick = 2,
		// Token: 0x04001AEF RID: 6895
		Ban = 4,
		// Token: 0x04001AF0 RID: 6896
		Unban = 8,
		// Token: 0x04001AF1 RID: 6897
		SelectSub = 16,
		// Token: 0x04001AF2 RID: 6898
		SelectMode = 32,
		// Token: 0x04001AF3 RID: 6899
		ManageCampaign = 64,
		// Token: 0x04001AF4 RID: 6900
		ConsoleCommands = 128,
		// Token: 0x04001AF5 RID: 6901
		ServerLog = 256,
		// Token: 0x04001AF6 RID: 6902
		ManageSettings = 512,
		// Token: 0x04001AF7 RID: 6903
		ManagePermissions = 1024,
		// Token: 0x04001AF8 RID: 6904
		KarmaImmunity = 2048,
		// Token: 0x04001AF9 RID: 6905
		ManageMoney = 4096,
		// Token: 0x04001AFA RID: 6906
		SellInventoryItems = 8192,
		// Token: 0x04001AFB RID: 6907
		SellSubItems = 16384,
		// Token: 0x04001AFC RID: 6908
		ManageMap = 32768,
		// Token: 0x04001AFD RID: 6909
		ManageHires = 65536,
		// Token: 0x04001AFE RID: 6910
		ManageBotTalents = 131072,
		// Token: 0x04001AFF RID: 6911
		SpamImmunity = 262144,
		// Token: 0x04001B00 RID: 6912
		All = 524287
	}
}
