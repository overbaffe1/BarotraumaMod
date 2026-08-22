using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000490 RID: 1168
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(7)]
	public readonly struct AccountInfo : INetSerializableStruct
	{
		// Token: 0x17001406 RID: 5126
		// (get) Token: 0x06004E11 RID: 19985 RVA: 0x002AC428 File Offset: 0x002AA628
		public bool IsNone
		{
			get
			{
				return this.AccountId.IsNone() && this.OtherMatchingIds.Length == 0;
			}
		}

		// Token: 0x06004E12 RID: 19986 RVA: 0x002AC447 File Offset: 0x002AA647
		public AccountInfo(AccountId accountId, params AccountId[] otherIds)
		{
			this = new AccountInfo(Option<Barotrauma.Networking.AccountId>.Some(accountId), otherIds);
		}

		// Token: 0x06004E13 RID: 19987 RVA: 0x002AC458 File Offset: 0x002AA658
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

		// Token: 0x06004E14 RID: 19988 RVA: 0x002AC49B File Offset: 0x002AA69B
		public bool Matches(AccountId accountId)
		{
			return this.AccountId.ValueEquals(accountId) || this.OtherMatchingIds.Contains(accountId);
		}

		// Token: 0x06004E15 RID: 19989 RVA: 0x002AC4BC File Offset: 0x002AA6BC
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

		// Token: 0x06004E16 RID: 19990 RVA: 0x002AC516 File Offset: 0x002AA716
		public override int GetHashCode()
		{
			return this.AccountId.GetHashCode();
		}

		// Token: 0x06004E17 RID: 19991 RVA: 0x002AC529 File Offset: 0x002AA729
		public static bool operator ==(AccountInfo a, AccountInfo b)
		{
			return a.Equals(b);
		}

		// Token: 0x06004E18 RID: 19992 RVA: 0x002AC53E File Offset: 0x002AA73E
		public static bool operator !=(AccountInfo a, AccountInfo b)
		{
			return !(a == b);
		}

		// Token: 0x040029A2 RID: 10658
		public static readonly AccountInfo None = new AccountInfo(Option<Barotrauma.Networking.AccountId>.None(), Array.Empty<AccountId>());

		// Token: 0x040029A3 RID: 10659
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<AccountId> AccountId;

		// Token: 0x040029A4 RID: 10660
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<AccountId> OtherMatchingIds;
	}
}
