using System;

namespace Barotrauma
{
	// Token: 0x02000135 RID: 309
	public enum SpamServerFilterType
	{
		// Token: 0x040014BF RID: 5311
		Invalid,
		// Token: 0x040014C0 RID: 5312
		NameEquals,
		// Token: 0x040014C1 RID: 5313
		NameContains,
		// Token: 0x040014C2 RID: 5314
		NameMatchesRegex,
		// Token: 0x040014C3 RID: 5315
		MessageEquals,
		// Token: 0x040014C4 RID: 5316
		MessageContains,
		// Token: 0x040014C5 RID: 5317
		MessageMatchesRegex,
		// Token: 0x040014C6 RID: 5318
		PlayerCountLarger,
		// Token: 0x040014C7 RID: 5319
		PlayerCountExact,
		// Token: 0x040014C8 RID: 5320
		MaxPlayersLarger,
		// Token: 0x040014C9 RID: 5321
		MaxPlayersExact,
		// Token: 0x040014CA RID: 5322
		GameModeEquals,
		// Token: 0x040014CB RID: 5323
		PlayStyleEquals,
		// Token: 0x040014CC RID: 5324
		Endpoint,
		// Token: 0x040014CD RID: 5325
		LanguageEquals,
		// Token: 0x040014CE RID: 5326
		LobbyId
	}
}
