using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000398 RID: 920
	internal sealed class SteamAuthTicketForEosHostAuthenticator : Authenticator
	{
		// Token: 0x0600365A RID: 13914 RVA: 0x00173CB0 File Offset: 0x00171EB0
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			SteamAuthTicketForEosHostAuthenticator.<VerifyTicket>d__3 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<SteamAuthTicketForEosHostAuthenticator.<VerifyTicket>d__3>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x0600365B RID: 13915 RVA: 0x00173CF3 File Offset: 0x00171EF3
		public override void EndAuthSession(AccountId accountId)
		{
		}

		// Token: 0x04001BB4 RID: 7092
		private const string ServerUrl = "https://barotraumagame.com/baromaster/";

		// Token: 0x04001BB5 RID: 7093
		private const string ServerFile = "getOwnerSteamId.php";

		// Token: 0x04001BB6 RID: 7094
		private const int RemoteRequestVersion = 1;
	}
}
