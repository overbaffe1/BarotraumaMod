using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000477 RID: 1143
	[NetworkSerialize(7)]
	internal struct TempClient : INetSerializableStruct
	{
		// Token: 0x040028C8 RID: 10440
		public string Name;

		// Token: 0x040028C9 RID: 10441
		public Identifier PreferredJob;

		// Token: 0x040028CA RID: 10442
		public CharacterTeamType TeamID;

		// Token: 0x040028CB RID: 10443
		public CharacterTeamType PreferredTeam;

		// Token: 0x040028CC RID: 10444
		public ushort NameId;

		// Token: 0x040028CD RID: 10445
		public AccountInfo AccountInfo;

		// Token: 0x040028CE RID: 10446
		public byte SessionId;

		// Token: 0x040028CF RID: 10447
		public ushort CharacterId;

		// Token: 0x040028D0 RID: 10448
		public float Karma;

		// Token: 0x040028D1 RID: 10449
		public bool Muted;

		// Token: 0x040028D2 RID: 10450
		public bool InGame;

		// Token: 0x040028D3 RID: 10451
		public bool HasPermissions;

		// Token: 0x040028D4 RID: 10452
		public bool IsOwner;

		// Token: 0x040028D5 RID: 10453
		public bool IsDownloading;
	}
}
