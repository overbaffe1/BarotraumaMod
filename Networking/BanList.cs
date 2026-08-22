using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000457 RID: 1111
	[NullableContext(1)]
	[Nullable(0)]
	internal class BanList
	{
		// Token: 0x1700130B RID: 4875
		// (get) Token: 0x06004A8A RID: 19082 RVA: 0x0028F8CC File Offset: 0x0028DACC
		// (set) Token: 0x06004A8B RID: 19083 RVA: 0x0028F8D4 File Offset: 0x0028DAD4
		[Nullable(2)]
		public GUIComponent BanFrame { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x06004A8C RID: 19084 RVA: 0x0028F8E0 File Offset: 0x0028DAE0
		private void RecreateBanFrame()
		{
			if (this.BanFrame != null)
			{
				GUIComponent parent = this.BanFrame.Parent;
				parent.RemoveChild(this.BanFrame);
				this.CreateBanFrame(parent);
			}
		}

		// Token: 0x06004A8D RID: 19085 RVA: 0x0028F918 File Offset: 0x0028DB18
		public GUIComponent CreateBanFrame(GUIComponent parent)
		{
			this.BanFrame = new GUIListBox(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			foreach (BannedPlayer bannedPlayer in this.bannedPlayers)
			{
				if (!this.localRemovedBans.Contains(bannedPlayer.UniqueIdentifier))
				{
					GUIFrame playerFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), ((GUIListBox)this.BanFrame).Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 70)
					}, "InnerFrame", null)
					{
						UserData = this.BanFrame
					};
					GUILayoutGroup paddedPlayerFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.85f), playerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
					{
						Stretch = true,
						RelativeSpacing = 0.05f,
						CanBeFocused = true
					};
					GUILayoutGroup topArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						Stretch = true,
						RelativeSpacing = 0.02f
					};
					Either<Address, AccountId> addressOrAccountId = bannedPlayer.AddressOrAccountId;
					string nameText = bannedPlayer.Name;
					Address address;
					AccountId accountId;
					if (addressOrAccountId.TryCast<Address>(out address))
					{
						nameText = nameText + " (" + address.StringRepresentation + ")";
					}
					else if (addressOrAccountId.TryCast<AccountId>(out accountId))
					{
						nameText = nameText + " (" + accountId.StringRepresentation + ")";
					}
					GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), topArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), nameText, null, null, Alignment.Left, false, "", null)
					{
						CanBeFocused = true
					};
					textBlock.RectTransform.MinSize = new Point(0, (int)textBlock.Font.MeasureString(textBlock.Text.SanitizedValue, false).Y);
					GUIButton removeButton = new GUIButton(new RectTransform(new Vector2(0.2f, 0.4f), topArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("BanListRemove"), Alignment.Center, "GUIButtonSmall", null)
					{
						IgnoreLayoutGroups = true,
						UserData = bannedPlayer,
						OnClicked = new GUIButton.OnClickedHandler(this.RemoveBan),
						Enabled = false
					};
					GUIButton guibutton = removeButton;
					guibutton.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guibutton.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
					{
						GameClient client = GameMain.Client;
						component.Enabled = (client != null && client.HasPermission(ClientPermissions.Unban));
					}));
					topArea.RectTransform.MinSize = new Point(0, Math.Max(textBlock.RectTransform.MinSize.Y, removeButton.RectTransform.MinSize.Y));
					topArea.RectTransform.IsFixedSize = true;
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					SerializableDateTime expirationTime;
					RichString text = bannedPlayer.ExpirationTime.TryUnwrap(out expirationTime) ? TextManager.GetWithVariable("BanExpires", "[time]", expirationTime.ToLocalUserString(), FormatCapitals.No) : TextManager.Get("BanPermanent");
					GUIFont smallFont = GUIStyle.SmallFont;
					new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
					LocalizedString reason = TextManager.GetServerMessage(bannedPlayer.Reason).Fallback(bannedPlayer.Reason, true);
					RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), paddedPlayerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = TextManager.Get("BanReason") + " " + (string.IsNullOrEmpty(bannedPlayer.Reason) ? TextManager.Get("None") : reason);
					smallFont = GUIStyle.SmallFont;
					new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Left, true, "", null).ToolTip = reason;
					paddedPlayerFrame.Recalculate();
					new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), ((GUIListBox)this.BanFrame).Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
				}
			}
			return this.BanFrame;
		}

		// Token: 0x06004A8E RID: 19086 RVA: 0x0028FEC4 File Offset: 0x0028E0C4
		private bool RemoveBan(GUIButton button, object obj)
		{
			BannedPlayer banned = obj as BannedPlayer;
			if (banned == null)
			{
				return false;
			}
			this.localRemovedBans.Add(banned.UniqueIdentifier);
			this.RecreateBanFrame();
			GameClient client = GameMain.Client;
			if (client != null)
			{
				ServerSettings serverSettings = client.ServerSettings;
				if (serverSettings != null)
				{
					serverSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
				}
			}
			return true;
		}

		// Token: 0x06004A8F RID: 19087 RVA: 0x0028FF24 File Offset: 0x0028E124
		public void ClientAdminRead(IReadMessage incMsg)
		{
			if (!incMsg.ReadBoolean())
			{
				incMsg.ReadPadBits();
				return;
			}
			bool isOwner = incMsg.ReadBoolean();
			incMsg.ReadPadBits();
			this.bannedPlayers.Clear();
			uint bannedPlayerCount = incMsg.ReadVariableUInt32();
			int i = 0;
			while (i < (int)bannedPlayerCount)
			{
				string name = incMsg.ReadString();
				uint uniqueIdentifier = incMsg.ReadUInt32();
				bool includesExpiration = incMsg.ReadBoolean();
				incMsg.ReadPadBits();
				Option<SerializableDateTime> expiration = Option<SerializableDateTime>.None();
				if (includesExpiration)
				{
					double hoursFromNow = incMsg.ReadDouble();
					SerializableDateTime localNow = SerializableDateTime.LocalNow;
					TimeSpan timeSpan = TimeSpan.FromHours(hoursFromNow);
					expiration = Option<SerializableDateTime>.Some(localNow + timeSpan);
				}
				string reason = incMsg.ReadString();
				Either<Address, AccountId> addressOrAccountId;
				if (!isOwner)
				{
					addressOrAccountId = new UnknownAddress();
					goto IL_F7;
				}
				bool isAddress = incMsg.ReadBoolean();
				incMsg.ReadPadBits();
				string str = incMsg.ReadString();
				Address address;
				if (isAddress && Address.Parse(str).TryUnwrap(out address))
				{
					addressOrAccountId = address;
					goto IL_F7;
				}
				AccountId accountId;
				if (AccountId.Parse(str).TryUnwrap(out accountId))
				{
					addressOrAccountId = accountId;
					goto IL_F7;
				}
				IL_111:
				i++;
				continue;
				IL_F7:
				this.bannedPlayers.Add(new BannedPlayer(uniqueIdentifier, name, addressOrAccountId, reason, expiration));
				goto IL_111;
			}
			if (this.BanFrame != null)
			{
				GUIComponent parent = this.BanFrame.Parent;
				parent.RemoveChild(this.BanFrame);
				this.CreateBanFrame(parent);
			}
		}

		// Token: 0x06004A90 RID: 19088 RVA: 0x00290078 File Offset: 0x0028E278
		public void ClientAdminWrite(IWriteMessage outMsg)
		{
			outMsg.WriteVariableUInt32((uint)this.localRemovedBans.Count);
			foreach (uint uniqueId in this.localRemovedBans)
			{
				outMsg.WriteUInt32(uniqueId);
			}
			this.localRemovedBans.Clear();
		}

		// Token: 0x1700130C RID: 4876
		// (get) Token: 0x06004A91 RID: 19089 RVA: 0x002900E8 File Offset: 0x0028E2E8
		public IReadOnlyList<BannedPlayer> BannedPlayers
		{
			get
			{
				return this.bannedPlayers;
			}
		}

		// Token: 0x1700130D RID: 4877
		// (get) Token: 0x06004A92 RID: 19090 RVA: 0x002900F0 File Offset: 0x0028E2F0
		public IEnumerable<string> BannedNames
		{
			get
			{
				return from bp in this.bannedPlayers
				select bp.Name;
			}
		}

		// Token: 0x1700130E RID: 4878
		// (get) Token: 0x06004A93 RID: 19091 RVA: 0x0029011C File Offset: 0x0028E31C
		public IEnumerable<Either<Address, AccountId>> BannedAddresses
		{
			get
			{
				return from bp in this.bannedPlayers
				select bp.AddressOrAccountId;
			}
		}

		// Token: 0x06004A94 RID: 19092 RVA: 0x00290148 File Offset: 0x0028E348
		public BanList()
		{
			this.bannedPlayers = new List<BannedPlayer>();
		}

		// Token: 0x040026FD RID: 9981
		public List<uint> localRemovedBans = new List<uint>();

		// Token: 0x040026FE RID: 9982
		private readonly List<BannedPlayer> bannedPlayers;
	}
}
