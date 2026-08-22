using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000494 RID: 1172
	internal sealed class EgsOwnershipTokenAuthenticator : Authenticator
	{
		// Token: 0x06004E2C RID: 20012 RVA: 0x002AC79C File Offset: 0x002AA99C
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			EgsOwnershipTokenAuthenticator.<VerifyTicket>d__0 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<EgsOwnershipTokenAuthenticator.<VerifyTicket>d__0>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06004E2D RID: 20013 RVA: 0x002AC7DF File Offset: 0x002AA9DF
		public override void EndAuthSession(AccountId accountId)
		{
		}
	}
}
