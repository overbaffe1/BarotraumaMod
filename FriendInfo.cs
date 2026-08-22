using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000128 RID: 296
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class FriendInfo : IDisposable
	{
		// Token: 0x17000A56 RID: 2646
		// (get) Token: 0x0600281C RID: 10268 RVA: 0x001BEA8E File Offset: 0x001BCC8E
		// (set) Token: 0x0600281D RID: 10269 RVA: 0x001BEA96 File Offset: 0x001BCC96
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<Sprite> Avatar { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] set; }

		// Token: 0x17000A57 RID: 2647
		// (get) Token: 0x0600281E RID: 10270 RVA: 0x001BEA9F File Offset: 0x001BCC9F
		public bool IsInServer
		{
			get
			{
				return this.CurrentStatus == FriendStatus.PlayingBarotrauma && this.ConnectCommand.IsSome();
			}
		}

		// Token: 0x17000A58 RID: 2648
		// (get) Token: 0x0600281F RID: 10271 RVA: 0x001BEAB7 File Offset: 0x001BCCB7
		public bool IsOnline
		{
			get
			{
				return this.CurrentStatus > FriendStatus.Offline;
			}
		}

		// Token: 0x17000A59 RID: 2649
		// (get) Token: 0x06002820 RID: 10272 RVA: 0x001BEAC4 File Offset: 0x001BCCC4
		public LocalizedString StatusText
		{
			get
			{
				LocalizedString result;
				if (this.CurrentStatus == FriendStatus.Offline)
				{
					result = "";
				}
				else if (this.ConnectCommand.IsSome())
				{
					result = TextManager.GetWithVariable("FriendPlayingOnServer", "[servername]", this.ServerName, FormatCapitals.No);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Friend");
					defaultInterpolatedStringHandler.AppendFormatted<FriendStatus>(this.CurrentStatus);
					result = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return result;
			}
		}

		// Token: 0x06002821 RID: 10273 RVA: 0x001BEB44 File Offset: 0x001BCD44
		public FriendInfo(string name, AccountId id, FriendStatus status, string serverName, [Nullable(0)] Option<ConnectCommand> connectCommand, FriendProvider provider)
		{
			this.Name = name;
			this.Id = id;
			this.CurrentStatus = status;
			this.ServerName = serverName;
			this.ConnectCommand = connectCommand;
			this.Provider = provider;
			Option.UnspecifiedNone none = Option.None;
			this.Avatar = none;
		}

		// Token: 0x06002822 RID: 10274 RVA: 0x001BEB98 File Offset: 0x001BCD98
		public void RetrieveOrInheritAvatar([Nullable(new byte[]
		{
			0,
			1
		})] Option<Sprite> inheritableAvatar, int size)
		{
			if (this.Avatar.IsSome())
			{
				return;
			}
			if (inheritableAvatar.IsSome())
			{
				this.Avatar = inheritableAvatar;
				return;
			}
			TaskPool.Add("RetrieveAvatar", this.Provider.RetrieveAvatar(this, size), delegate(Task t)
			{
				Option<Sprite> spr;
				if (!t.TryGetResult(out spr))
				{
					return;
				}
				this.Avatar = this.Avatar.Fallback(spr);
			});
		}

		// Token: 0x06002823 RID: 10275 RVA: 0x001BEBEC File Offset: 0x001BCDEC
		public void Dispose()
		{
			Sprite avatar;
			if (this.Avatar.TryUnwrap(out avatar))
			{
				avatar.Remove();
			}
			Option.UnspecifiedNone none = Option.None;
			this.Avatar = none;
		}

		// Token: 0x0400145C RID: 5212
		public readonly string Name;

		// Token: 0x0400145D RID: 5213
		public readonly AccountId Id;

		// Token: 0x0400145E RID: 5214
		public readonly FriendStatus CurrentStatus;

		// Token: 0x0400145F RID: 5215
		public readonly string ServerName;

		// Token: 0x04001460 RID: 5216
		[Nullable(0)]
		public readonly Option<ConnectCommand> ConnectCommand;

		// Token: 0x04001461 RID: 5217
		public readonly FriendProvider Provider;
	}
}
