using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Steam;

namespace Barotrauma.Networking
{
	// Token: 0x02000493 RID: 1171
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class Authenticator
	{
		// Token: 0x06004E28 RID: 20008
		public abstract Task<AccountInfo> VerifyTicket(AuthenticationTicket ticket);

		// Token: 0x06004E29 RID: 20009
		public abstract void EndAuthSession(AccountId accountId);

		// Token: 0x06004E2A RID: 20010 RVA: 0x002AC720 File Offset: 0x002AA920
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
