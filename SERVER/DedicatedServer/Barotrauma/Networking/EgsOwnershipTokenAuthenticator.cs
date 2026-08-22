using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000397 RID: 919
	internal sealed class EgsOwnershipTokenAuthenticator : Authenticator
	{
		// Token: 0x06003657 RID: 13911 RVA: 0x00173C60 File Offset: 0x00171E60
		public override Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket)
		{
			EgsOwnershipTokenAuthenticator.<VerifyTicket>d__0 <VerifyTicket>d__;
			<VerifyTicket>d__.<>t__builder = AsyncTaskMethodBuilder<AccountInfo>.Create();
			<VerifyTicket>d__.ticket = ticket;
			<VerifyTicket>d__.<>1__state = -1;
			<VerifyTicket>d__.<>t__builder.Start<EgsOwnershipTokenAuthenticator.<VerifyTicket>d__0>(ref <VerifyTicket>d__);
			return <VerifyTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06003658 RID: 13912 RVA: 0x00173CA3 File Offset: 0x00171EA3
		public override void EndAuthSession(AccountId accountId)
		{
		}
	}
}
