using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Steam;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x02000399 RID: 921
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamAuthTicketForSteamHostAuthenticator : Authenticator
	{
		// Token: 0x0600365D RID: 13917 RVA: 0x00173D00 File Offset: 0x00171F00
		private static BeginAuthResult BeginAuthSession(AuthTicket authTicket, SteamId clientSteamId)
		{
			if (!SteamManager.IsInitialized)
			{
				return BeginAuthResult.ServerNotConnectedToSteam;
			}
			if (authTicket.Data == null)
			{
				return BeginAuthResult.InvalidTicket;
			}
			DebugConsole.Log("Authenticating Steam client " + ((clientSteamId != null) ? clientSteamId.ToString() : null));
			BeginAuthResult startResult = SteamServer.BeginAuthSession(authTicket.Data, clientSteamId.Value);
			if (startResult != BeginAuthResult.OK)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Steam authentication failed: failed to start auth session (");
				defaultInterpolatedStringHandler.AppendFormatted<BeginAuthResult>(startResult);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return startResult;
		}

		// Token: 0x0600365E RID: 13918 RVA: 0x00173D94 File Offset: 0x00171F94
		private static void EndAuthSession(SteamId clientSteamId)
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Ending auth session with Steam client ");
			defaultInterpolatedStringHandler.AppendFormatted<SteamId>(clientSteamId);
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			SteamServer.EndAuthSession(clientSteamId.Value);
		}

		// Token: 0x0600365F RID: 13919 RVA: 0x00173DE4 File Offset: 0x00171FE4
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			SteamAuthTicketForSteamHostAuthenticator.<VerifyTicket>d__2 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<SteamAuthTicketForSteamHostAuthenticator.<VerifyTicket>d__2>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06003660 RID: 13920 RVA: 0x00173E28 File Offset: 0x00172028
		public override void EndAuthSession(AccountId accountId)
		{
			SteamId steamId = accountId as SteamId;
			if (steamId == null)
			{
				return;
			}
			SteamServer.EndAuthSession(steamId.Value);
		}
	}
}
