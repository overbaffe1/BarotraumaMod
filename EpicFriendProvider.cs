using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200012A RID: 298
	internal sealed class EpicFriendProvider : FriendProvider
	{
		// Token: 0x0600282A RID: 10282 RVA: 0x001BED93 File Offset: 0x001BCF93
		private FriendInfo EgsFriendToFriendInfo(EosInterface.EgsFriend egsFriend)
		{
			return new FriendInfo(egsFriend.DisplayName, egsFriend.EpicAccountId, egsFriend.Status, egsFriend.ServerName, ConnectCommand.Parse(egsFriend.ConnectCommand), this);
		}

		// Token: 0x0600282B RID: 10283 RVA: 0x001BEDC4 File Offset: 0x001BCFC4
		public override Task<Option<FriendInfo>> RetrieveFriend(AccountId id)
		{
			EpicFriendProvider.<RetrieveFriend>d__1 <RetrieveFriend>d__;
			<RetrieveFriend>d__.<>t__builder = AsyncTaskMethodBuilder<Option<FriendInfo>>.Create();
			<RetrieveFriend>d__.<>4__this = this;
			<RetrieveFriend>d__.id = id;
			<RetrieveFriend>d__.<>1__state = -1;
			<RetrieveFriend>d__.<>t__builder.Start<EpicFriendProvider.<RetrieveFriend>d__1>(ref <RetrieveFriend>d__);
			return <RetrieveFriend>d__.<>t__builder.Task;
		}

		// Token: 0x0600282C RID: 10284 RVA: 0x001BEE10 File Offset: 0x001BD010
		public override Task<ImmutableArray<FriendInfo>> RetrieveFriends()
		{
			EpicFriendProvider.<RetrieveFriends>d__2 <RetrieveFriends>d__;
			<RetrieveFriends>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<FriendInfo>>.Create();
			<RetrieveFriends>d__.<>4__this = this;
			<RetrieveFriends>d__.<>1__state = -1;
			<RetrieveFriends>d__.<>t__builder.Start<EpicFriendProvider.<RetrieveFriends>d__2>(ref <RetrieveFriends>d__);
			return <RetrieveFriends>d__.<>t__builder.Task;
		}

		// Token: 0x0600282D RID: 10285 RVA: 0x001BEE54 File Offset: 0x001BD054
		public override Task<Option<Sprite>> RetrieveAvatar(FriendInfo friend, int avatarSize)
		{
			EpicAccountId epicAccount = friend.Id as EpicAccountId;
			if (epicAccount == null)
			{
				Option.UnspecifiedNone none = Option.None;
				return Task.FromResult<Option<Sprite>>(none);
			}
			Color color = Color.Black;
			ulong mostSignificant64Bits;
			ulong leastSignificant64Bits;
			if (ulong.TryParse(epicAccount.EosStringRepresentation.Substring(0, 16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out mostSignificant64Bits) && ulong.TryParse(epicAccount.EosStringRepresentation.Substring(16), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out leastSignificant64Bits))
			{
				BigInteger fullId = mostSignificant64Bits;
				fullId <<= 64;
				fullId |= leastSignificant64Bits;
				BigInteger idMaxValue = ulong.MaxValue;
				idMaxValue <<= 64;
				idMaxValue |= ulong.MaxValue;
				BigInteger middleRangeSize = idMaxValue / 7;
				BigInteger firstRangeSize = middleRangeSize / 2;
				if (fullId <= firstRangeSize)
				{
					color = EpicFriendProvider.egsProfileColors[0];
				}
				else
				{
					color = EpicFriendProvider.egsProfileColors[(int)((fullId - firstRangeSize) / middleRangeSize) + 1];
				}
			}
			char glyphChar = friend.Name.FallbackNullOrEmpty("?")[0];
			ScalableFont font = GUIStyle.UnscaledSmallFont.GetFontForStr(glyphChar.ToString());
			Texture2D tex = null;
			if (font != null)
			{
				ValueTuple<ScalableFont.GlyphData, Texture2D> glyphDataAndTextureForChar = font.GetGlyphDataAndTextureForChar(glyphChar);
				ScalableFont.GlyphData glyphData = glyphDataAndTextureForChar.Item1;
				Texture2D glyphTexture = glyphDataAndTextureForChar.Item2;
				Vector2 glyphSize = new Vector2((float)glyphData.TexCoords.Width, (float)glyphData.TexCoords.Height);
				int texSize = (int)Math.Max(MathUtils.RoundUpToPowerOfTwo((uint)(font.LineHeight * 1.5f)), MathUtils.RoundUpToPowerOfTwo((uint)(font.LineHeight * 1.5f)));
				if (glyphTexture != null)
				{
					Color[] glyphTextureData = new Color[(int)glyphSize.X * (int)glyphSize.Y];
					glyphTexture.GetData<Color>(0, new Rectangle?(glyphData.TexCoords), glyphTextureData, 0, glyphTextureData.Length);
					Color[] texData = (from _ in Enumerable.Range(0, texSize * texSize)
					select color).ToArray<Color>();
					Point start = (new Vector2((float)texSize, (float)texSize) / 2f - glyphSize / 2f).ToPoint();
					Point end = start + glyphSize.ToPoint();
					for (int x = start.X; x < end.X; x++)
					{
						for (int y = start.Y; y < end.Y; y++)
						{
							texData[x + y * texSize] = Color.Lerp(color, Color.White, (float)glyphTextureData[x - start.X + (y - start.Y) * (int)glyphSize.X].A / 255f);
						}
					}
					tex = new Texture2D(GameMain.GraphicsDeviceManager.GraphicsDevice, texSize, texSize);
					tex.SetData<Color>(texData);
				}
			}
			if (tex == null)
			{
				tex = new Texture2D(GameMain.GraphicsDeviceManager.GraphicsDevice, 2, 2);
				tex.SetData<Color>(new Color[]
				{
					color,
					color,
					color,
					color
				});
			}
			Sprite sprite = new Sprite(tex, null, null, 0f, null);
			return Task.FromResult<Option<Sprite>>(Option.Some<Sprite>(sprite));
		}

		// Token: 0x0600282E RID: 10286 RVA: 0x001BF1EC File Offset: 0x001BD3EC
		public override Task<string> GetSelfUserName()
		{
			EpicFriendProvider.<GetSelfUserName>d__5 <GetSelfUserName>d__;
			<GetSelfUserName>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<GetSelfUserName>d__.<>1__state = -1;
			<GetSelfUserName>d__.<>t__builder.Start<EpicFriendProvider.<GetSelfUserName>d__5>(ref <GetSelfUserName>d__);
			return <GetSelfUserName>d__.<>t__builder.Task;
		}

		// Token: 0x04001464 RID: 5220
		private static readonly ImmutableArray<Color> egsProfileColors = new Color[]
		{
			new Color(4293962064U),
			new Color(4281047120U),
			new Color(4281049230U),
			new Color(4287959625U),
			new Color(4288285809U),
			new Color(4282264006U),
			new Color(4281890285U),
			new Color(4280198637U)
		}.ToImmutableArray<Color>();
	}
}
