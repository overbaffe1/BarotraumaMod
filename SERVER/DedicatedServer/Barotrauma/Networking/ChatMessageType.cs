using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000379 RID: 889
	public enum ChatMessageType
	{
		// Token: 0x04001AC9 RID: 6857
		Default,
		// Token: 0x04001ACA RID: 6858
		Error,
		// Token: 0x04001ACB RID: 6859
		Dead,
		// Token: 0x04001ACC RID: 6860
		Server,
		// Token: 0x04001ACD RID: 6861
		Radio,
		// Token: 0x04001ACE RID: 6862
		Private,
		// Token: 0x04001ACF RID: 6863
		Console,
		// Token: 0x04001AD0 RID: 6864
		MessageBox,
		// Token: 0x04001AD1 RID: 6865
		Order,
		// Token: 0x04001AD2 RID: 6866
		ServerLog,
		// Token: 0x04001AD3 RID: 6867
		ServerMessageBox,
		// Token: 0x04001AD4 RID: 6868
		ServerMessageBoxInGame,
		// Token: 0x04001AD5 RID: 6869
		Team,
		// Token: 0x04001AD6 RID: 6870
		BlockedBySpamFilter
	}
}
