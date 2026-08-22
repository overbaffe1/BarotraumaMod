using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000495 RID: 1173
	internal sealed class SteamAuthTicketForEosHostAuthenticator : Authenticator
	{
		// Token: 0x06004E2F RID: 20015 RVA: 0x002AC7EC File Offset: 0x002AA9EC
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			SteamAuthTicketForEosHostAuthenticator.<VerifyTicket>d__3 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<SteamAuthTicketForEosHostAuthenticator.<VerifyTicket>d__3>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06004E30 RID: 20016 RVA: 0x002AC82F File Offset: 0x002AAA2F
		public override void EndAuthSession(AccountId accountId)
		{
		}

		// Token: 0x040029AB RID: 10667
		private const string ServerUrl = "https://barotraumagame.com/baromaster/";

		// Token: 0x040029AC RID: 10668
		private const string ServerFile = "getOwnerSteamId.php";

		// Token: 0x040029AD RID: 10669
		private const int RemoteRequestVersion = 1;
	}
}
