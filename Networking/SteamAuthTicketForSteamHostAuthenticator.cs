using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Steam;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x02000496 RID: 1174
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamAuthTicketForSteamHostAuthenticator : Authenticator
	{
		// Token: 0x06004E32 RID: 20018 RVA: 0x002AC83C File Offset: 0x002AAA3C
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
			BeginAuthResult startResult = SteamUser.BeginAuthSession(authTicket.Data, clientSteamId.Value);
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

		// Token: 0x06004E33 RID: 20019 RVA: 0x002AC8D0 File Offset: 0x002AAAD0
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
			SteamUser.EndAuthSession(clientSteamId.Value);
		}

		// Token: 0x06004E34 RID: 20020 RVA: 0x002AC920 File Offset: 0x002AAB20
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			SteamAuthTicketForSteamHostAuthenticator.<VerifyTicket>d__2 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<SteamAuthTicketForSteamHostAuthenticator.<VerifyTicket>d__2>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06004E35 RID: 20021 RVA: 0x002AC964 File Offset: 0x002AAB64
		public override void EndAuthSession(AccountId accountId)
		{
			SteamId steamId = accountId as SteamId;
			if (steamId == null)
			{
				return;
			}
			SteamUser.EndAuthSession(steamId.Value);
		}
	}
}
