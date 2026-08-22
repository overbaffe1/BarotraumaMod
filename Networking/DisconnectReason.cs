using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200048E RID: 1166
	internal enum DisconnectReason
	{
		// Token: 0x04002981 RID: 10625
		Unknown,
		// Token: 0x04002982 RID: 10626
		Disconnected,
		// Token: 0x04002983 RID: 10627
		Banned,
		// Token: 0x04002984 RID: 10628
		Kicked,
		// Token: 0x04002985 RID: 10629
		ServerShutdown,
		// Token: 0x04002986 RID: 10630
		ServerCrashed,
		// Token: 0x04002987 RID: 10631
		ServerFull,
		// Token: 0x04002988 RID: 10632
		AuthenticationRequired,
		// Token: 0x04002989 RID: 10633
		AuthenticationFailed,
		// Token: 0x0400298A RID: 10634
		SessionTaken,
		// Token: 0x0400298B RID: 10635
		TooManyFailedLogins,
		// Token: 0x0400298C RID: 10636
		InvalidName,
		// Token: 0x0400298D RID: 10637
		NameTaken,
		// Token: 0x0400298E RID: 10638
		InvalidVersion,
		// Token: 0x0400298F RID: 10639
		SteamP2PError,
		// Token: 0x04002990 RID: 10640
		MalformedData,
		// Token: 0x04002991 RID: 10641
		Timeout,
		// Token: 0x04002992 RID: 10642
		ExcessiveDesyncOldEvent,
		// Token: 0x04002993 RID: 10643
		ExcessiveDesyncRemovedEvent,
		// Token: 0x04002994 RID: 10644
		SyncTimeout,
		// Token: 0x04002995 RID: 10645
		SteamP2PTimeOut
	}
}
