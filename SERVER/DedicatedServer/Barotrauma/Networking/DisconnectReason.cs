using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000392 RID: 914
	internal enum DisconnectReason
	{
		// Token: 0x04001B96 RID: 7062
		Unknown,
		// Token: 0x04001B97 RID: 7063
		Disconnected,
		// Token: 0x04001B98 RID: 7064
		Banned,
		// Token: 0x04001B99 RID: 7065
		Kicked,
		// Token: 0x04001B9A RID: 7066
		ServerShutdown,
		// Token: 0x04001B9B RID: 7067
		ServerCrashed,
		// Token: 0x04001B9C RID: 7068
		ServerFull,
		// Token: 0x04001B9D RID: 7069
		AuthenticationRequired,
		// Token: 0x04001B9E RID: 7070
		AuthenticationFailed,
		// Token: 0x04001B9F RID: 7071
		SessionTaken,
		// Token: 0x04001BA0 RID: 7072
		TooManyFailedLogins,
		// Token: 0x04001BA1 RID: 7073
		InvalidName,
		// Token: 0x04001BA2 RID: 7074
		NameTaken,
		// Token: 0x04001BA3 RID: 7075
		InvalidVersion,
		// Token: 0x04001BA4 RID: 7076
		SteamP2PError,
		// Token: 0x04001BA5 RID: 7077
		MalformedData,
		// Token: 0x04001BA6 RID: 7078
		Timeout,
		// Token: 0x04001BA7 RID: 7079
		ExcessiveDesyncOldEvent,
		// Token: 0x04001BA8 RID: 7080
		ExcessiveDesyncRemovedEvent,
		// Token: 0x04001BA9 RID: 7081
		SyncTimeout,
		// Token: 0x04001BAA RID: 7082
		SteamP2PTimeOut
	}
}
