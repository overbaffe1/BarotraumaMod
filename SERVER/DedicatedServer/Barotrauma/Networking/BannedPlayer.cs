using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000365 RID: 869
	[NullableContext(1)]
	[Nullable(0)]
	internal class BannedPlayer
	{
		// Token: 0x17000E4D RID: 3661
		// (get) Token: 0x0600334F RID: 13135 RVA: 0x0015A614 File Offset: 0x00158814
		public bool Expired
		{
			get
			{
				SerializableDateTime expirationTime;
				if (this.ExpirationTime.TryUnwrap(out expirationTime))
				{
					SerializableDateTime localNow = SerializableDateTime.LocalNow;
					return localNow > expirationTime;
				}
				return false;
			}
		}

		// Token: 0x06003350 RID: 13136 RVA: 0x0015A641 File Offset: 0x00158841
		public BannedPlayer(string name, Either<Address, AccountId> addressOrAccountId, string reason, [Nullable(0)] Option<SerializableDateTime> expirationTime)
		{
			this.Name = name;
			this.AddressOrAccountId = addressOrAccountId;
			this.Reason = reason;
			this.ExpirationTime = expirationTime;
			this.UniqueIdentifier = BannedPlayer.LastIdentifier;
			BannedPlayer.LastIdentifier += 1U;
		}

		// Token: 0x06003351 RID: 13137 RVA: 0x0015A680 File Offset: 0x00158880
		public bool MatchesClient(Client client)
		{
			AccountId bannedAccountId;
			AccountId accountId;
			return client != null && (this.AddressOrAccountId.TryGet(out bannedAccountId) && client.AccountId.TryUnwrap(out accountId)) && bannedAccountId.Equals(accountId);
		}

		// Token: 0x0400196D RID: 6509
		private static uint LastIdentifier;

		// Token: 0x0400196E RID: 6510
		public readonly string Name;

		// Token: 0x0400196F RID: 6511
		public readonly Either<Address, AccountId> AddressOrAccountId;

		// Token: 0x04001970 RID: 6512
		public readonly string Reason;

		// Token: 0x04001971 RID: 6513
		[Nullable(0)]
		public Option<SerializableDateTime> ExpirationTime;

		// Token: 0x04001972 RID: 6514
		public readonly uint UniqueIdentifier;
	}
}
