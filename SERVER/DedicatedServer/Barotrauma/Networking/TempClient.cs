using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200037B RID: 891
	[NetworkSerialize(7)]
	internal struct TempClient : INetSerializableStruct
	{
		// Token: 0x04001ADD RID: 6877
		public string Name;

		// Token: 0x04001ADE RID: 6878
		public Identifier PreferredJob;

		// Token: 0x04001ADF RID: 6879
		public CharacterTeamType TeamID;

		// Token: 0x04001AE0 RID: 6880
		public CharacterTeamType PreferredTeam;

		// Token: 0x04001AE1 RID: 6881
		public ushort NameId;

		// Token: 0x04001AE2 RID: 6882
		public AccountInfo AccountInfo;

		// Token: 0x04001AE3 RID: 6883
		public byte SessionId;

		// Token: 0x04001AE4 RID: 6884
		public ushort CharacterId;

		// Token: 0x04001AE5 RID: 6885
		public float Karma;

		// Token: 0x04001AE6 RID: 6886
		public bool Muted;

		// Token: 0x04001AE7 RID: 6887
		public bool InGame;

		// Token: 0x04001AE8 RID: 6888
		public bool HasPermissions;

		// Token: 0x04001AE9 RID: 6889
		public bool IsOwner;

		// Token: 0x04001AEA RID: 6890
		public bool IsDownloading;
	}
}
