using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000129 RID: 297
	internal sealed class CompositeFriendProvider : FriendProvider
	{
		// Token: 0x06002825 RID: 10277 RVA: 0x001BEC51 File Offset: 0x001BCE51
		public CompositeFriendProvider(params FriendProvider[] providers)
		{
			this.providers = providers.ToImmutableArray<FriendProvider>();
		}

		// Token: 0x06002826 RID: 10278 RVA: 0x001BEC6C File Offset: 0x001BCE6C
		public override Task<Option<FriendInfo>> RetrieveFriend(AccountId id)
		{
			CompositeFriendProvider.<RetrieveFriend>d__2 <RetrieveFriend>d__;
			<RetrieveFriend>d__.<>t__builder = AsyncTaskMethodBuilder<Option<FriendInfo>>.Create();
			<RetrieveFriend>d__.<>4__this = this;
			<RetrieveFriend>d__.id = id;
			<RetrieveFriend>d__.<>1__state = -1;
			<RetrieveFriend>d__.<>t__builder.Start<CompositeFriendProvider.<RetrieveFriend>d__2>(ref <RetrieveFriend>d__);
			return <RetrieveFriend>d__.<>t__builder.Task;
		}

		// Token: 0x06002827 RID: 10279 RVA: 0x001BECB8 File Offset: 0x001BCEB8
		public override Task<ImmutableArray<FriendInfo>> RetrieveFriends()
		{
			CompositeFriendProvider.<RetrieveFriends>d__3 <RetrieveFriends>d__;
			<RetrieveFriends>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<FriendInfo>>.Create();
			<RetrieveFriends>d__.<>4__this = this;
			<RetrieveFriends>d__.<>1__state = -1;
			<RetrieveFriends>d__.<>t__builder.Start<CompositeFriendProvider.<RetrieveFriends>d__3>(ref <RetrieveFriends>d__);
			return <RetrieveFriends>d__.<>t__builder.Task;
		}

		// Token: 0x06002828 RID: 10280 RVA: 0x001BECFC File Offset: 0x001BCEFC
		public override Task<Option<Sprite>> RetrieveAvatar(FriendInfo friend, int avatarSize)
		{
			CompositeFriendProvider.<RetrieveAvatar>d__4 <RetrieveAvatar>d__;
			<RetrieveAvatar>d__.<>t__builder = AsyncTaskMethodBuilder<Option<Sprite>>.Create();
			<RetrieveAvatar>d__.<>4__this = this;
			<RetrieveAvatar>d__.friend = friend;
			<RetrieveAvatar>d__.avatarSize = avatarSize;
			<RetrieveAvatar>d__.<>1__state = -1;
			<RetrieveAvatar>d__.<>t__builder.Start<CompositeFriendProvider.<RetrieveAvatar>d__4>(ref <RetrieveAvatar>d__);
			return <RetrieveAvatar>d__.<>t__builder.Task;
		}

		// Token: 0x06002829 RID: 10281 RVA: 0x001BED50 File Offset: 0x001BCF50
		public override Task<string> GetSelfUserName()
		{
			CompositeFriendProvider.<GetSelfUserName>d__5 <GetSelfUserName>d__;
			<GetSelfUserName>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<GetSelfUserName>d__.<>4__this = this;
			<GetSelfUserName>d__.<>1__state = -1;
			<GetSelfUserName>d__.<>t__builder.Start<CompositeFriendProvider.<GetSelfUserName>d__5>(ref <GetSelfUserName>d__);
			return <GetSelfUserName>d__.<>t__builder.Task;
		}

		// Token: 0x04001463 RID: 5219
		private readonly ImmutableArray<FriendProvider> providers;
	}
}
