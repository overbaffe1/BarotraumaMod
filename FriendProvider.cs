using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200012B RID: 299
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class FriendProvider
	{
		// Token: 0x06002831 RID: 10289 RVA: 0x001BF2DC File Offset: 0x001BD4DC
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public Task<Option<FriendInfo>> RetrieveFriendWithAvatar(AccountId id, int size)
		{
			FriendProvider.<RetrieveFriendWithAvatar>d__0 <RetrieveFriendWithAvatar>d__;
			<RetrieveFriendWithAvatar>d__.<>t__builder = AsyncTaskMethodBuilder<Option<FriendInfo>>.Create();
			<RetrieveFriendWithAvatar>d__.<>4__this = this;
			<RetrieveFriendWithAvatar>d__.id = id;
			<RetrieveFriendWithAvatar>d__.size = size;
			<RetrieveFriendWithAvatar>d__.<>1__state = -1;
			<RetrieveFriendWithAvatar>d__.<>t__builder.Start<FriendProvider.<RetrieveFriendWithAvatar>d__0>(ref <RetrieveFriendWithAvatar>d__);
			return <RetrieveFriendWithAvatar>d__.<>t__builder.Task;
		}

		// Token: 0x06002832 RID: 10290
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public abstract Task<Option<FriendInfo>> RetrieveFriend(AccountId id);

		// Token: 0x06002833 RID: 10291
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public abstract Task<ImmutableArray<FriendInfo>> RetrieveFriends();

		// Token: 0x06002834 RID: 10292
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public abstract Task<Option<Sprite>> RetrieveAvatar(FriendInfo friend, int avatarSize);

		// Token: 0x06002835 RID: 10293
		public abstract Task<string> GetSelfUserName();
	}
}
