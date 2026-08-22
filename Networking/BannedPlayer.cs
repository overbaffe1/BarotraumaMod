using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000456 RID: 1110
	[NullableContext(1)]
	[Nullable(0)]
	internal class BannedPlayer
	{
		// Token: 0x06004A88 RID: 19080 RVA: 0x0028F862 File Offset: 0x0028DA62
		public BannedPlayer(uint uniqueIdentifier, string name, Either<Address, AccountId> addressOrAccountId, string reason, [Nullable(0)] Option<SerializableDateTime> expiration)
		{
			this.Name = name;
			this.AddressOrAccountId = addressOrAccountId;
			this.UniqueIdentifier = uniqueIdentifier;
			this.Reason = reason;
			this.ExpirationTime = expiration;
		}

		// Token: 0x06004A89 RID: 19081 RVA: 0x0028F890 File Offset: 0x0028DA90
		public bool MatchesClient(Client client)
		{
			AccountId bannedAccountId;
			AccountId accountId;
			return client != null && (this.AddressOrAccountId.TryGet(out bannedAccountId) && client.AccountId.TryUnwrap(out accountId)) && bannedAccountId.Equals(accountId);
		}

		// Token: 0x040026F7 RID: 9975
		public readonly string Name;

		// Token: 0x040026F8 RID: 9976
		public readonly Either<Address, AccountId> AddressOrAccountId;

		// Token: 0x040026F9 RID: 9977
		public readonly string Reason;

		// Token: 0x040026FA RID: 9978
		[Nullable(0)]
		public Option<SerializableDateTime> ExpirationTime;

		// Token: 0x040026FB RID: 9979
		public readonly uint UniqueIdentifier;
	}
}
