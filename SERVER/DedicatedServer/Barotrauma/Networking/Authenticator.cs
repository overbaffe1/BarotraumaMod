using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Steam;

namespace Barotrauma.Networking
{
	// Token: 0x02000396 RID: 918
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class Authenticator
	{
		// Token: 0x06003653 RID: 13907
		public abstract Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket);

		// Token: 0x06003654 RID: 13908
		public abstract void EndAuthSession(AccountId accountId);

		// Token: 0x06003655 RID: 13909 RVA: 0x00173BE4 File Offset: 0x00171DE4
		public static ImmutableDictionary<AuthenticationTicketKind, Authenticator> GetAuthenticatorsForHost([Nullable(new byte[]
		{
			0,
			1
		})] Option<Endpoint> ownerEndpointOption)
		{
			Dictionary<AuthenticationTicketKind, Authenticator> authenticators = new Dictionary<AuthenticationTicketKind, Authenticator>();
			if (EosInterface.Core.IsInitialized)
			{
				authenticators.Add(AuthenticationTicketKind.EgsOwnershipToken, new EgsOwnershipTokenAuthenticator());
				Endpoint ownerEndpoint;
				if (ownerEndpointOption.TryUnwrap(out ownerEndpoint) && ownerEndpoint is EosP2PEndpoint)
				{
					authenticators.Add(AuthenticationTicketKind.SteamAuthTicketForEosHost, new SteamAuthTicketForEosHostAuthenticator());
				}
			}
			Endpoint ownerEndpoint2;
			if ((!ownerEndpointOption.TryUnwrap(out ownerEndpoint2) || !(ownerEndpoint2 is EosP2PEndpoint)) && SteamManager.IsInitialized)
			{
				authenticators.Add(AuthenticationTicketKind.SteamAuthTicketForSteamHost, new SteamAuthTicketForSteamHostAuthenticator());
			}
			return authenticators.ToImmutableDictionary<AuthenticationTicketKind, Authenticator>();
		}
	}
}
