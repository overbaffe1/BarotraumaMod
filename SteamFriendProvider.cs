using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x0200012C RID: 300
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamFriendProvider : FriendProvider
	{
		// Token: 0x06002837 RID: 10295 RVA: 0x001BF338 File Offset: 0x001BD538
		private FriendInfo FromSteamFriend(Friend steamFriend)
		{
			string text = steamFriend.Name ?? "";
			AccountId accountId = new Barotrauma.Networking.SteamId(steamFriend.Id);
			FriendState state = steamFriend.State;
			FriendStatus friendStatus;
			if (state != FriendState.Offline)
			{
				if (state != FriendState.Invisible)
				{
					if (steamFriend.IsPlayingThisGame)
					{
						friendStatus = FriendStatus.PlayingBarotrauma;
					}
					else
					{
						Friend.FriendGameInfo? gameInfo = steamFriend.GameInfo;
						if (gameInfo != null && gameInfo.GetValueOrDefault().GameID.Value > 0UL)
						{
							friendStatus = FriendStatus.PlayingAnotherGame;
						}
						else
						{
							friendStatus = FriendStatus.NotPlaying;
						}
					}
				}
				else
				{
					friendStatus = FriendStatus.Offline;
				}
			}
			else
			{
				friendStatus = FriendStatus.Offline;
			}
			string name = text;
			AccountId id = accountId;
			FriendStatus status = friendStatus;
			string serverName = steamFriend.GetRichPresence("servername") ?? "";
			string connectCmd = steamFriend.GetRichPresence("connect");
			Option<ConnectCommand> connectCommand;
			if (connectCmd == null)
			{
				Option.UnspecifiedNone none = Option.None;
				connectCommand = none;
			}
			else
			{
				connectCommand = ConnectCommand.Parse(ToolBox.SplitCommand(connectCmd));
			}
			return new FriendInfo(name, id, status, serverName, connectCommand, this);
		}

		// Token: 0x06002838 RID: 10296 RVA: 0x001BF408 File Offset: 0x001BD608
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public override Task<Option<FriendInfo>> RetrieveFriend(AccountId id)
		{
			Barotrauma.Networking.SteamId steamId = id as Barotrauma.Networking.SteamId;
			Option<FriendInfo> result;
			if (steamId == null)
			{
				Option.UnspecifiedNone none = Option.None;
				result = none;
			}
			else
			{
				result = Option.Some<FriendInfo>(this.FromSteamFriend(new Friend(steamId.Value)));
			}
			return Task.FromResult<Option<FriendInfo>>(result);
		}

		// Token: 0x06002839 RID: 10297 RVA: 0x001BF44E File Offset: 0x001BD64E
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public override Task<ImmutableArray<FriendInfo>> RetrieveFriends()
		{
			return Task.FromResult<ImmutableArray<FriendInfo>>(SteamManager.IsInitialized ? SteamFriends.GetFriends().Select(new Func<Friend, FriendInfo>(this.FromSteamFriend)).ToImmutableArray<FriendInfo>() : ImmutableArray<FriendInfo>.Empty);
		}

		// Token: 0x0600283A RID: 10298 RVA: 0x001BF480 File Offset: 0x001BD680
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public override Task<Option<Sprite>> RetrieveAvatar(FriendInfo friend, int avatarSize)
		{
			SteamFriendProvider.<RetrieveAvatar>d__3 <RetrieveAvatar>d__;
			<RetrieveAvatar>d__.<>t__builder = AsyncTaskMethodBuilder<Option<Sprite>>.Create();
			<RetrieveAvatar>d__.friend = friend;
			<RetrieveAvatar>d__.avatarSize = avatarSize;
			<RetrieveAvatar>d__.<>1__state = -1;
			<RetrieveAvatar>d__.<>t__builder.Start<SteamFriendProvider.<RetrieveAvatar>d__3>(ref <RetrieveAvatar>d__);
			return <RetrieveAvatar>d__.<>t__builder.Task;
		}

		// Token: 0x0600283B RID: 10299 RVA: 0x001BF4CB File Offset: 0x001BD6CB
		public override Task<string> GetSelfUserName()
		{
			return Task.FromResult<string>(SteamManager.GetUsername());
		}

		// Token: 0x02000D55 RID: 3413
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004F1E RID: 20254
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Func<Steamworks.SteamId, Task<Image?>> <0>__GetSmallAvatarAsync;

			// Token: 0x04004F1F RID: 20255
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Func<Steamworks.SteamId, Task<Image?>> <1>__GetMediumAvatarAsync;

			// Token: 0x04004F20 RID: 20256
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Func<Steamworks.SteamId, Task<Image?>> <2>__GetLargeAvatarAsync;
		}
	}
}
