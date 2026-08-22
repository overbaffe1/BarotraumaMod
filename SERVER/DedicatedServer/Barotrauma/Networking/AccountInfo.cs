using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000393 RID: 915
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(7)]
	public readonly struct AccountInfo : INetSerializableStruct
	{
		// Token: 0x17000F0B RID: 3851
		// (get) Token: 0x0600363C RID: 13884 RVA: 0x001738EB File Offset: 0x00171AEB
		public bool IsNone
		{
			get
			{
				return this.AccountId.IsNone() && this.OtherMatchingIds.Length == 0;
			}
		}

		// Token: 0x0600363D RID: 13885 RVA: 0x0017390A File Offset: 0x00171B0A
		public AccountInfo(AccountId accountId, params AccountId[] otherIds)
		{
			this = new AccountInfo(Option<Barotrauma.Networking.AccountId>.Some(accountId), otherIds);
		}

		// Token: 0x0600363E RID: 13886 RVA: 0x0017391C File Offset: 0x00171B1C
		public AccountInfo([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId, params AccountId[] otherIds)
		{
			this.AccountId = accountId;
			this.OtherMatchingIds = (from id in otherIds
			where !accountId.ValueEquals(id)
			select id).ToImmutableArray<AccountId>();
		}

		// Token: 0x0600363F RID: 13887 RVA: 0x0017395F File Offset: 0x00171B5F
		public bool Matches(AccountId accountId)
		{
			return this.AccountId.ValueEquals(accountId) || this.OtherMatchingIds.Contains(accountId);
		}

		// Token: 0x06003640 RID: 13888 RVA: 0x00173980 File Offset: 0x00171B80
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			bool result;
			if (obj is AccountInfo)
			{
				AccountInfo otherInfo = (AccountInfo)obj;
				result = (this.AccountId == otherInfo.AccountId && this.OtherMatchingIds.All(new Func<AccountId, bool>(otherInfo.OtherMatchingIds.Contains)));
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06003641 RID: 13889 RVA: 0x001739DA File Offset: 0x00171BDA
		public override int GetHashCode()
		{
			return this.AccountId.GetHashCode();
		}

		// Token: 0x06003642 RID: 13890 RVA: 0x001739ED File Offset: 0x00171BED
		public static bool operator ==(AccountInfo a, AccountInfo b)
		{
			return a.Equals(b);
		}

		// Token: 0x06003643 RID: 13891 RVA: 0x00173A02 File Offset: 0x00171C02
		public static bool operator !=(AccountInfo a, AccountInfo b)
		{
			return !(a == b);
		}

		// Token: 0x04001BAB RID: 7083
		public static readonly AccountInfo None = new AccountInfo(Option<Barotrauma.Networking.AccountId>.None(), Array.Empty<AccountId>());

		// Token: 0x04001BAC RID: 7084
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<AccountId> AccountId;

		// Token: 0x04001BAD RID: 7085
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<AccountId> OtherMatchingIds;
	}
}
