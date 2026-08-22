using System;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000202 RID: 514
	internal static class ClientExtensions
	{
		// Token: 0x060024DA RID: 9434 RVA: 0x000F317F File Offset: 0x000F137F
		public static void SetClientCharacter(this Client client, Character character)
		{
			GameMain.Server.SetClientCharacter(client, character);
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x000F318D File Offset: 0x000F138D
		public static void Kick(this Client client, string reason = "")
		{
			GameMain.Server.KickClient(client.Connection, reason);
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x000F31A0 File Offset: 0x000F13A0
		public static void Ban(this Client client, string reason = "", float seconds = -1f)
		{
			if (seconds == -1f)
			{
				GameMain.Server.BanClient(client, reason, null);
				return;
			}
			GameMain.Server.BanClient(client, reason, new TimeSpan?(TimeSpan.FromSeconds((double)seconds)));
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x000F31E3 File Offset: 0x000F13E3
		public static bool CheckPermission(this Client client, ClientPermissions permissions)
		{
			return client.Permissions.HasFlag(permissions);
		}
	}
}
