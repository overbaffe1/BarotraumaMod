using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;

namespace Barotrauma
{
	// Token: 0x0200012E RID: 302
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SocialOverlay : IDisposable
	{
		// Token: 0x17000A5A RID: 2650
		// (get) Token: 0x0600283F RID: 10303 RVA: 0x001BF5AF File Offset: 0x001BD7AF
		// (set) Token: 0x06002840 RID: 10304 RVA: 0x001BF5B6 File Offset: 0x001BD7B6
		[Nullable(2)]
		public static SocialOverlay Instance { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x06002841 RID: 10305 RVA: 0x001BF5BE File Offset: 0x001BD7BE
		public static void Init()
		{
			if (SocialOverlay.Instance == null)
			{
				SocialOverlay.Instance = new SocialOverlay();
			}
		}

		// Token: 0x06002842 RID: 10306 RVA: 0x001BF5D4 File Offset: 0x001BD7D4
		private static RectTransform CreateRowRectT(GUIComponent parent, float heightScale = 1f)
		{
			return new RectTransform(new ValueTuple<float, float>(1f, heightScale / 7f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth);
		}

		// Token: 0x06002843 RID: 10307 RVA: 0x001BF620 File Offset: 0x001BD820
		private static GUILayoutGroup CreateRowLayout(GUIComponent parent, float heightScale = 1f)
		{
			GUILayoutGroup rowLayout = new GUILayoutGroup(SocialOverlay.CreateRowRectT(parent, heightScale), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, rowLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				rowLayout.RectTransform.NonScaledSize = base.<CreateRowLayout>g__calculateSize|1();
			});
			return rowLayout;
		}

		// Token: 0x06002844 RID: 10308 RVA: 0x001BF6B4 File Offset: 0x001BD8B4
		private void RecreateSelfPlayerRow()
		{
			Barotrauma.Networking.SteamId steamId;
			if (SteamManager.GetSteamId().TryUnwrap(out steamId))
			{
				this.selfPlayerRowLayout.ClearChildren();
				string username = SteamManager.GetUsername();
				AccountId id = steamId;
				FriendStatus status = FriendStatus.PlayingBarotrauma;
				string serverName = "";
				Option.UnspecifiedNone none = Option.None;
				new SocialOverlay.PlayerRow(new FriendInfo(username, id, status, serverName, none, this.friendProvider), this.selfPlayerRowLayout, false, null);
				return;
			}
			if (EosInterface.IdQueries.IsLoggedIntoEosConnect)
			{
				TaskPool.Add("GetEpicAccountIdForSelfPlayerRow", SocialOverlay.<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0(), delegate(Task t)
				{
					Option<EosInterface.EgsFriend> userInfoOption;
					EosInterface.EgsFriend userInfo;
					if (!t.TryGetResult(out userInfoOption) || !userInfoOption.TryUnwrap(out userInfo))
					{
						return;
					}
					this.selfPlayerRowLayout.ClearChildren();
					string displayName = userInfo.DisplayName;
					AccountId epicAccountId = userInfo.EpicAccountId;
					FriendStatus status2 = FriendStatus.PlayingBarotrauma;
					string serverName2 = "";
					Option.UnspecifiedNone none2 = Option.None;
					new SocialOverlay.PlayerRow(new FriendInfo(displayName, epicAccountId, status2, serverName2, none2, this.friendProvider), this.selfPlayerRowLayout, false, null);
				});
			}
		}

		// Token: 0x06002845 RID: 10309 RVA: 0x001BF734 File Offset: 0x001BD934
		private SocialOverlay()
		{
			this.background = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), "SocialOverlayBackground", null);
			GUILayoutGroup rightSideLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.9f, 1f), this.background.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight), true, Anchor.BottomLeft);
			this.linkHint = new GUIButton(new RectTransform(new ValueTuple<float, float>(0.5f, 0.12857142f), rightSideLayout.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothWidth), Alignment.Center, "FriendsButton", null)
			{
				OnClicked = delegate(GUIButton btn, object _)
				{
					GUIButton guibutton = this.eosConfigButton;
					if (guibutton != null)
					{
						guibutton.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
					}
					EosSteamPrimaryLogin.IsNewEosPlayer = false;
					btn.Visible = false;
					return false;
				},
				Visible = false
			};
			new GUITextBlock(new RectTransform(Vector2.One * 0.95f, this.linkHint.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("EosSettings.RecommendLinkingToEpicAccount"), null, null, Alignment.Left, true, "FriendsButton", null);
			GUIFrame content = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), rightSideLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "SocialOverlayFriendsList", null);
			new GUIButton(new RectTransform(Vector2.One * 0.08f, content.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopRight), null, null, ScaleBasis.BothWidth)
			{
				RelativeOffset = new ValueTuple<float, float>(-0.03f, 0.015f)
			}, Alignment.Center, "SocialOverlayCloseButton", null).OnClicked = delegate(GUIButton _, object _)
			{
				this.IsOpen = false;
				return false;
			};
			this.friendProvider = new CompositeFriendProvider(new FriendProvider[]
			{
				new SteamFriendProvider(),
				new EpicFriendProvider()
			});
			this.notificationHandler = new SocialOverlay.NotificationHandler();
			this.inviteHandler = new SocialOverlay.InviteHandler(this, this.friendProvider, this.notificationHandler);
			this.selectedFriendInfoFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.25f, 0.28f), this.background.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.BothHeight), "SocialOverlayPopup", null)
			{
				OutlineThickness = 1f,
				Visible = false
			};
			this.contentLayout = new GUILayoutGroup(new RectTransform(Vector2.One, content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.selfPlayerRowLayout = SocialOverlay.CreateRowLayout(this.contentLayout, 1f);
			this.RecreateSelfPlayerRow();
			this.friendPlayerListBox = new GUIListBox(new RectTransform(Vector2.One, this.contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				OnSelected = delegate(GUIComponent component, object userData)
				{
					FriendInfo friendInfo = userData as FriendInfo;
					if (friendInfo == null)
					{
						return false;
					}
					this.selectedFriendInfoFrame.Visible = true;
					this.selectedFriendInfoFrame.RectTransform.AbsoluteOffset = new ValueTuple<int, int>(this.background.Rect.Right - component.Rect.X, Math.Clamp(component.Rect.Center.Y - this.selectedFriendInfoFrame.Rect.Height / 2, 0, this.background.Rect.Bottom - this.selectedFriendInfoFrame.Rect.Height));
					this.PopulateSelectedFriendInfoFrame(friendInfo);
					return true;
				}
			};
			GUIScrollBar scrollBar = this.friendPlayerListBox.ScrollBar;
			scrollBar.OnMoved = (GUIScrollBar.OnMovedHandler)Delegate.Combine(scrollBar.OnMoved, new GUIScrollBar.OnMovedHandler(delegate(GUIScrollBar _, float _)
			{
				this.friendPlayerListBox.Deselect();
				return true;
			}));
			if (SteamManager.IsInitialized)
			{
				GUILayoutGroup eosConfigRowLayout = SocialOverlay.CreateRowLayout(this.contentLayout, 1.5f);
				eosConfigRowLayout.ChildAnchor = Anchor.CenterLeft;
				this.eosConfigButton = new GUIButton(new RectTransform(Vector2.One * 0.8f, eosConfigRowLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, null, null)
				{
					Enabled = (GameMain.NetworkMember == null),
					OnClicked = delegate(GUIButton _, object _)
					{
						this.ShowEosSettingsMenu();
						return true;
					}
				};
				new GUIFrame(new RectTransform(Vector2.One * 0.5f, this.eosConfigButton.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIButtonSettings", null).CanBeFocused = false;
				this.eosStatusTextContainer = new GUILayoutGroup(new RectTransform(Vector2.One, eosConfigRowLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				this.RefreshEosStatusText();
			}
			this.RefreshFriendList();
		}

		// Token: 0x06002846 RID: 10310 RVA: 0x001BFC68 File Offset: 0x001BDE68
		public void DisplayBindHintToPlayer()
		{
			if (this.IsOpen)
			{
				return;
			}
			GUIButton baseButton = new GUIButton(new RectTransform(Vector2.One, this.notificationHandler.NotificationContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new ValueTuple<float, float>(0f, -1f)
			}, Alignment.Center, "SocialOverlayPopup", null);
			baseButton.Frame.OutlineThickness = 1f;
			SocialOverlay.NotificationHandler.Notification notification = new SocialOverlay.NotificationHandler.Notification(DateTime.Now, baseButton);
			baseButton.OnClicked = delegate(GUIButton _, object _)
			{
				this.IsOpen = true;
				this.notificationHandler.RemoveNotification(notification);
				return false;
			};
			baseButton.OnSecondaryClicked = delegate(GUIComponent _, object _)
			{
				this.notificationHandler.RemoveNotification(notification);
				return false;
			};
			new GUITextBlock(new RectTransform(Vector2.One * 0.98f, baseButton.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("SocialOverlayShortcutHint", "[shortcut]", SocialOverlay.ShortcutBindText, FormatCapitals.No), null, null, Alignment.Center, true, "", null).CanBeFocused = false;
			this.notificationHandler.AddNotification(notification);
		}

		// Token: 0x06002847 RID: 10311 RVA: 0x001BFDC4 File Offset: 0x001BDFC4
		private unsafe void ShowEosSettingsMenu()
		{
			SocialOverlay.<>c__DisplayClass31_0 CS$<>8__locals1 = new SocialOverlay.<>c__DisplayClass31_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.hasEpicAccount = EosAccount.SelfAccountIds.OfType<EpicAccountId>().Any<EpicAccountId>();
			string manageAccountsText = CS$<>8__locals1.hasEpicAccount ? "EosSettings.ManageConnectedAccounts" : "EosSettings.LinkToEpicAccount";
			CS$<>8__locals1.eosEnabled = EosInterface.Core.IsInitialized;
			string enableButtonText = CS$<>8__locals1.eosEnabled ? "EosSettings.DisableEos" : "EosSettings.EnableEos";
			SocialOverlay.<>c__DisplayClass31_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("EosSettings");
			RichString text = string.Empty;
			LocalizedString[] buttons = new LocalizedString[]
			{
				TextManager.Get(manageAccountsText),
				TextManager.Get(enableButtonText),
				TextManager.Get("EosSettings.RequestDeletion")
			};
			Point? minSize = new Point?(new Point(GUI.IntScale(550f), 0));
			CS$<>8__locals2.msgBox = new GUIMessageBox(headerText, text, buttons, null, minSize, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				DrawOnTop = true
			};
			CS$<>8__locals1.msgBox.Buttons[0].Enabled = CS$<>8__locals1.eosEnabled;
			CS$<>8__locals1.msgBox.Buttons[0].ToolTip = TextManager.Get(manageAccountsText + ".Tooltip");
			CS$<>8__locals1.msgBox.Buttons[1].ToolTip = TextManager.Get(enableButtonText + ".Tooltip");
			CS$<>8__locals1.msgBox.Buttons[2].ToolTip = TextManager.Get("EosSettings.RequestDeletion.Tooltip");
			new GUIButton(new RectTransform(new Point(GUI.IntScale(35f)), CS$<>8__locals1.msgBox.InnerFrame.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(GUI.IntScale(8f))
			}, Alignment.Center, "SocialOverlayCloseButton", null).OnClicked = CS$<>8__locals1.<ShowEosSettingsMenu>g__closeMsgBox|0(CS$<>8__locals1.msgBox);
			GUIButton guibutton = CS$<>8__locals1.msgBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object _)
			{
				if (!CS$<>8__locals1.hasEpicAccount)
				{
					SocialOverlay.<>c__DisplayClass31_1 CS$<>8__locals3 = new SocialOverlay.<>c__DisplayClass31_1();
					CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals1;
					SocialOverlay.<>c__DisplayClass31_1 CS$<>8__locals4 = CS$<>8__locals3;
					LocalizedString text2 = TextManager.Get("EosLinkSteamToEpicLoadingText");
					ValueTuple<LocalizedString, Action<GUIMessageBox>>[] array = new ValueTuple<LocalizedString, Action<GUIMessageBox>>[1];
					array[0] = new ValueTuple<LocalizedString, Action<GUIMessageBox>>(TextManager.Get("Cancel"), delegate(GUIMessageBox msgBox)
					{
						msgBox.Close();
					});
					CS$<>8__locals4.loadingBox = GUIMessageBox.CreateLoadingBox(text2, array, new Vector2?(new ValueTuple<float, float>(0.35f, 0.25f)));
					CS$<>8__locals3.loadingBox.DrawOnTop = true;
					TaskPool.Add("LoginToEpicAccountAsSecondary", EosEpicSecondaryLogin.LoginToLinkedEpicAccount(), delegate(Task t)
					{
						Result<Unit, EosEpicSecondaryLogin.LoginError> result;
						if (t.TryGetResult(out result))
						{
							LocalizedString taskResultMsg;
							EosEpicSecondaryLogin.LoginError failure;
							if (result.IsSuccess)
							{
								taskResultMsg = TextManager.Get("EosLinkSuccess");
							}
							else if (result.TryUnwrapFailure(out failure))
							{
								taskResultMsg = TextManager.GetWithVariable("EosLinkError", "[error]", failure.ToString(), FormatCapitals.No);
							}
							else
							{
								taskResultMsg = TextManager.GetWithVariable("EosLinkError", "[error]", result.ToString(), FormatCapitals.No);
							}
							GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EosSettings.LinkToEpicAccount"), taskResultMsg, new LocalizedString[]
							{
								TextManager.Get("OK")
							}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
							{
								DrawOnTop = true
							};
							msgBox.Buttons[0].OnClicked = CS$<>8__locals3.CS$<>8__locals1.<ShowEosSettingsMenu>g__closeMsgBox|0(msgBox);
						}
						CS$<>8__locals3.loadingBox.Close();
					});
					CS$<>8__locals1.msgBox.Close();
				}
				else
				{
					GUIMessageBox prompt = GameMain.ShowOpenUriPrompt("https://www.epicgames.com/account/connections", "openlinkinbrowserprompt", null);
					prompt.DrawOnTop = true;
					CS$<>8__locals1.msgBox.Close();
				}
				return true;
			}));
			GUIButton guibutton2 = CS$<>8__locals1.msgBox.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object obj)
			{
				EosSteamPrimaryLogin.CrossplayChoice crossplayChoice = CS$<>8__locals1.eosEnabled ? EosSteamPrimaryLogin.CrossplayChoice.Disabled : EosSteamPrimaryLogin.CrossplayChoice.Enabled;
				EosSteamPrimaryLogin.HandleCrossplayChoiceChange(crossplayChoice);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.CrossplayChoice = crossplayChoice;
				GameSettings.SetCurrentConfig(config);
				GameSettings.SaveCurrentConfig();
				base.<ShowEosSettingsMenu>g__closeMsgBox|0(CS$<>8__locals1.msgBox)(btn, obj);
				return true;
			}));
			GUIButton guibutton3 = CS$<>8__locals1.msgBox.Buttons[2];
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object obj)
			{
				string bodyText = "I would like to delete all of my account information stored by Epic Games.";
				bool epicAccountIdAvailable = EosAccount.SelfAccountIds.OfType<EpicAccountId>().Any<EpicAccountId>();
				Barotrauma.Networking.SteamId steamId;
				bool steamIdAvailable = SteamManager.GetSteamId().TryUnwrap(out steamId);
				if (!steamIdAvailable && !epicAccountIdAvailable)
				{
					new GUIMessageBox(TextManager.Get("Error"), TextManager.GetWithVariable("EosSettings.RequestDeletion.NoAccountId", "[emailAddress]", "contact@barotraumagame.com", FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					return false;
				}
				if (epicAccountIdAvailable)
				{
					bodyText = bodyText + "\n\nMy Epic Account ID(s): " + string.Join(", ", from id in EosAccount.SelfAccountIds.OfType<EpicAccountId>()
					select id.StringRepresentation);
				}
				if (steamIdAvailable)
				{
					bodyText = bodyText + "\n\nMy Steam ID: " + steamId.StringRepresentation;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
				defaultInterpolatedStringHandler.AppendLiteral("mailto:");
				defaultInterpolatedStringHandler.AppendFormatted("contact@barotraumagame.com");
				defaultInterpolatedStringHandler.AppendLiteral("?");
				defaultInterpolatedStringHandler.AppendLiteral("subject=");
				defaultInterpolatedStringHandler.AppendFormatted(Uri.EscapeDataString("Requesting account information deletion"));
				defaultInterpolatedStringHandler.AppendLiteral("&body=");
				defaultInterpolatedStringHandler.AppendFormatted(Uri.EscapeDataString(bodyText));
				string uri = defaultInterpolatedStringHandler.ToStringAndClear();
				GUIMessageBox prompt = GameMain.ShowOpenUriPrompt(uri, TextManager.GetWithVariables("OpenLinkInEmailClient", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[recipient]", "contact@barotraumagame.com"),
					new ValueTuple<string, string>("[message]", bodyText)
				}));
				if (prompt != null)
				{
					prompt.DrawOnTop = true;
				}
				base.<ShowEosSettingsMenu>g__closeMsgBox|0(CS$<>8__locals1.msgBox)(btn, obj);
				return true;
			}));
		}

		// Token: 0x06002848 RID: 10312 RVA: 0x001C0048 File Offset: 0x001BE248
		private void PopulateSelectedFriendInfoFrame(FriendInfo friendInfo)
		{
			SocialOverlay.<>c__DisplayClass32_0 CS$<>8__locals1 = new SocialOverlay.<>c__DisplayClass32_0();
			CS$<>8__locals1.friendInfo = friendInfo;
			CS$<>8__locals1.<>4__this = this;
			this.selectedFriendInfoFrame.ClearChildren();
			CS$<>8__locals1.layout = new GUILayoutGroup(new RectTransform(Vector2.One * 0.9f, this.selectedFriendInfoFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addPadding|1();
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.08f), CS$<>8__locals1.layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CS$<>8__locals1.friendInfo.Name;
			GUIFont font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Center, false, "", null).ForceUpperCase = ForceUpperCase.No;
			RectTransform rectT2 = new RectTransform(new ValueTuple<float, float>(1f, 0.08f), CS$<>8__locals1.layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = CS$<>8__locals1.friendInfo.StatusText;
			font = GUIStyle.Font;
			new GUITextBlock(rectT2, text2, null, font, Alignment.TopCenter, false, "", null).ForceUpperCase = ForceUpperCase.No;
			CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addPadding|1();
			GUIButton viewProfileButton = CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addButton|2(CS$<>8__locals1.friendInfo.Id.ViewProfileLabel());
			viewProfileButton.OnClicked = delegate(GUIButton _, object _)
			{
				CS$<>8__locals1.friendInfo.Id.OpenProfile();
				return false;
			};
			if (CS$<>8__locals1.friendInfo.IsInServer)
			{
				GameClient client = GameMain.Client;
				ConnectCommand command;
				if ((client == null || !client.IsServerOwner) && CS$<>8__locals1.friendInfo.ConnectCommand.TryUnwrap(out command) && !command.IsClientConnectedToEndpoint())
				{
					GUIButton joinButton = CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addButton|2(TextManager.Get("ServerListJoin"));
					joinButton.OnClicked = delegate(GUIButton _, object _)
					{
						GameMain.Instance.ConnectCommand = CS$<>8__locals1.friendInfo.ConnectCommand;
						CS$<>8__locals1.<>4__this.selectedFriendInfoFrame.Visible = false;
						CS$<>8__locals1.<>4__this.IsOpen = false;
						return false;
					};
				}
			}
			if (this.inviteHandler.HasInviteFrom(CS$<>8__locals1.friendInfo.Id))
			{
				GUIButton declineButton = CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addButton|2(TextManager.Get("DeclineInvite"));
				declineButton.OnClicked = delegate(GUIButton _, object _)
				{
					CS$<>8__locals1.<>4__this.inviteHandler.ClearInvitesFrom(CS$<>8__locals1.friendInfo.Id);
					CS$<>8__locals1.<>4__this.selectedFriendInfoFrame.Visible = false;
					return false;
				};
			}
			if (GameMain.Client != null)
			{
				GUIButton inviteButton = CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addButton|2(TextManager.Get("InviteFriend"));
				inviteButton.OnClicked = delegate(GUIButton _, object _)
				{
					CS$<>8__locals1.<>4__this.selectedFriendInfoFrame.Visible = false;
					GameClient client2 = GameMain.Client;
					Endpoint endpoint = (client2 != null) ? client2.ClientPeer.ServerEndpoint : null;
					LidgrenEndpoint lidgrenEndpoint = endpoint as LidgrenEndpoint;
					Option<ConnectCommand> option;
					if (lidgrenEndpoint == null)
					{
						if (!(endpoint is P2PEndpoint) && !(endpoint is PipeEndpoint))
						{
							Option.UnspecifiedNone none = Option.None;
							option = none;
						}
						else
						{
							option = Option.Some<ConnectCommand>(new ConnectCommand(GameMain.Client.Name, GameMain.Client.ClientPeer.AllServerEndpoints.OfType<P2PEndpoint>().ToImmutableArray<P2PEndpoint>()));
						}
					}
					else
					{
						option = Option.Some<ConnectCommand>(new ConnectCommand(GameMain.Client.Name, lidgrenEndpoint));
					}
					Option<ConnectCommand> connectCommandOption = option;
					ConnectCommand connectCommand;
					if (!connectCommandOption.TryUnwrap(out connectCommand))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Could not create an invite for the endpoint ");
						GameClient client3 = GameMain.Client;
						defaultInterpolatedStringHandler.AppendFormatted<Endpoint>((client3 != null) ? client3.ClientPeer.ServerEndpoint : null);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						return false;
					}
					Barotrauma.Networking.SteamId friendSteamId = CS$<>8__locals1.friendInfo.Id as Barotrauma.Networking.SteamId;
					if (friendSteamId != null && SteamManager.IsInitialized)
					{
						Friend steamFriend = new Friend(friendSteamId.Value);
						steamFriend.InviteToGame(connectCommand.ToString());
					}
					else
					{
						SocialOverlay.<>c__DisplayClass32_1 CS$<>8__locals2 = new SocialOverlay.<>c__DisplayClass32_1();
						AccountId id = CS$<>8__locals1.friendInfo.Id;
						CS$<>8__locals2.friendEpicId = (id as EpicAccountId);
						if (CS$<>8__locals2.friendEpicId != null && EosInterface.Core.IsInitialized)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(6, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Invite");
							defaultInterpolatedStringHandler2.AppendFormatted<EpicAccountId>(CS$<>8__locals2.friendEpicId);
							TaskPool.Add(defaultInterpolatedStringHandler2.ToStringAndClear(), CS$<>8__locals2.<PopulateSelectedFriendInfoFrame>g__sendEpicInvite|6(), delegate(Task _)
							{
							});
						}
					}
					return false;
				};
			}
			CS$<>8__locals1.<PopulateSelectedFriendInfoFrame>g__addPadding|1();
		}

		// Token: 0x06002849 RID: 10313 RVA: 0x001C02E0 File Offset: 0x001BE4E0
		private void RefreshEosStatusText()
		{
			if (this.eosStatusTextContainer == null)
			{
				return;
			}
			this.eosStatusTextContainer.ClearChildren();
			bool linkedToEpicAccount = EosAccount.SelfAccountIds.OfType<EpicAccountId>().Any<EpicAccountId>();
			RectTransform rectT = new RectTransform(Vector2.One, this.eosStatusTextContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("EosStatus.");
			defaultInterpolatedStringHandler.AppendFormatted<EosInterface.Core.Status>(EosInterface.Core.CurrentStatus);
			new GUITextBlock(rectT, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()) + "\n" + TextManager.Get(linkedToEpicAccount ? "EosSettings.LinkedToAccount" : "EosSettings.NotLinkedToAccount"), null, null, Alignment.CenterLeft, true, "", null);
			this.linkHint.Visible = (!linkedToEpicAccount && EosSteamPrimaryLogin.IsNewEosPlayer);
		}

		// Token: 0x0600284A RID: 10314 RVA: 0x001C03D7 File Offset: 0x001BE5D7
		public void RefreshFriendList()
		{
			EosAccount.RefreshSelfAccountIds(delegate
			{
				this.RefreshEosStatusText();
				this.lastRefreshTime = DateTime.Now;
				if (EosInterface.Core.CurrentStatus != EosInterface.Core.Status.Online && !SteamManager.IsInitialized)
				{
					this.friendPlayerListBox.ClearChildren();
					GUITextBlock offlineLabel = this.<RefreshFriendList>g__insertLabel|34_1(TextManager.Get("SocialOverlayOffline"), 4f);
					offlineLabel.Wrap = true;
					return;
				}
				TaskPool.Add("RefreshFriendList", this.friendProvider.RetrieveFriends(), delegate(Task t)
				{
					ImmutableArray<FriendInfo> friends;
					if (!t.TryGetResult(out friends))
					{
						return;
					}
					this.friendPlayerListBox.ClearChildren();
					this.friendPlayerRows.ForEach(delegate(SocialOverlay.PlayerRow f)
					{
						f.FriendInfo.Dispose();
					});
					this.friendPlayerRows.Clear();
					ImmutableArray<FriendInfo> friendsOrdered = (from f in friends
					orderby f.CurrentStatus descending, this.inviteHandler.HasInviteFrom(f.Id) descending, f.Name
					select f).ToImmutableArray<FriendInfo>();
					bool prevWasOnline = true;
					if (friendsOrdered.Length > 0 && friendsOrdered[0].IsOnline)
					{
						this.<RefreshFriendList>g__insertLabel|34_1(TextManager.Get("Label.OnlineLabel"), 0.5f);
					}
					for (int friendIndex = 0; friendIndex < friendsOrdered.Length; friendIndex++)
					{
						FriendInfo friend = friendsOrdered[friendIndex];
						if (prevWasOnline && !friend.IsOnline)
						{
							if (friendIndex > 0)
							{
								this.<RefreshFriendList>g__insertLabel|34_1("", 0.5f);
							}
							this.<RefreshFriendList>g__insertLabel|34_1(TextManager.Get("Label.OfflineLabel"), 0.5f);
						}
						GUIFrame friendFrame = new GUIFrame(SocialOverlay.CreateRowRectT(this.friendPlayerListBox.Content, 1f), "ListBoxElement", null)
						{
							UserData = friend
						};
						GUILayoutGroup newRowLayout = SocialOverlay.CreateRowLayout(friendFrame, 1f);
						newRowLayout.RectTransform.RelativeSize = Vector2.One;
						newRowLayout.RectTransform.ScaleBasis = ScaleBasis.Normal;
						SocialOverlay.PlayerRow newRow = new SocialOverlay.PlayerRow(friend, newRowLayout, this.inviteHandler.HasInviteFrom(friend.Id), null);
						this.friendPlayerRows.Add(newRow);
						prevWasOnline = friend.IsOnline;
					}
					this.contentLayout.Recalculate();
					this.friendPlayerListBox.UpdateScrollBarSize();
				});
			});
		}

		// Token: 0x0600284B RID: 10315 RVA: 0x001C03EA File Offset: 0x001BE5EA
		public void AddToGuiUpdateList()
		{
			if (this.IsOpen)
			{
				this.background.AddToGUIUpdateList(false, 0);
			}
			this.notificationHandler.AddToGuiUpdateList();
		}

		// Token: 0x0600284C RID: 10316 RVA: 0x001C040C File Offset: 0x001BE60C
		public void Update()
		{
			this.inviteHandler.Update();
			this.notificationHandler.Update();
			if (!this.IsOpen)
			{
				return;
			}
			if (this.selectedFriendInfoFrame.Visible)
			{
				if (PlayerInput.PrimaryMouseButtonClicked() && this.selectedFriendInfoFrame.Visible && !GUI.IsMouseOn(this.friendPlayerListBox) && !GUI.IsMouseOn(this.selectedFriendInfoFrame))
				{
					this.friendPlayerListBox.Deselect();
				}
				if (GUI.IsMouseOn(this.friendPlayerListBox) && PlayerInput.ScrollWheelSpeed != 0)
				{
					this.friendPlayerListBox.Deselect();
				}
				if (!this.friendPlayerListBox.Selected)
				{
					this.selectedFriendInfoFrame.Visible = false;
				}
			}
			if (this.eosConfigButton != null)
			{
				bool eosConfigAccessible = GameMain.NetworkMember == null;
				if (eosConfigAccessible != this.eosConfigButton.Enabled)
				{
					this.eosConfigButton.Enabled = eosConfigAccessible;
					this.eosConfigButton.Children.ForEach(delegate(GUIComponent c)
					{
						c.Enabled = eosConfigAccessible;
					});
					this.eosConfigButton.ToolTip = (eosConfigAccessible ? string.Empty : TextManager.Get("CantAccessEOSSettingsInMP"));
				}
			}
			EosInterface.Core.Status currentEosStatus = EosInterface.Core.CurrentStatus;
			if (currentEosStatus != this.eosLastKnownStatus)
			{
				this.eosLastKnownStatus = currentEosStatus;
				this.RefreshEosStatusText();
			}
			if (DateTime.Now < this.lastRefreshTime + this.refreshInterval)
			{
				return;
			}
			this.RefreshFriendList();
		}

		// Token: 0x0600284D RID: 10317 RVA: 0x001C0583 File Offset: 0x001BE783
		public void Dispose()
		{
			this.inviteHandler.Dispose();
		}

		// Token: 0x0600284F RID: 10319 RVA: 0x001C05A4 File Offset: 0x001BE7A4
		[CompilerGenerated]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		internal static Task<Option<EosInterface.EgsFriend>> <RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0()
		{
			SocialOverlay.<<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d <<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d;
			<<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d.<>t__builder = AsyncTaskMethodBuilder<Option<EosInterface.EgsFriend>>.Create();
			<<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d.<>1__state = -1;
			<<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d.<>t__builder.Start<SocialOverlay.<<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d>(ref <<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d);
			return <<RecreateSelfPlayerRow>g__GetEpicAccountInfo|28_0>d.<>t__builder.Task;
		}

		// Token: 0x06002859 RID: 10329 RVA: 0x001C09F0 File Offset: 0x001BEBF0
		[CompilerGenerated]
		private GUITextBlock <RefreshFriendList>g__insertLabel|34_1(LocalizedString text, float heightScale = 0.5f)
		{
			GUIFrame labelContainer = new GUIFrame(SocialOverlay.CreateRowRectT(this.friendPlayerListBox.Content, 1f), null, null)
			{
				CanBeFocused = false
			};
			Vector2 oldRelativeSize = labelContainer.RectTransform.RelativeSize;
			labelContainer.RectTransform.RelativeSize = new ValueTuple<float, float>(oldRelativeSize.X, oldRelativeSize.Y * heightScale);
			RectTransform rectT = new RectTransform(Vector2.One, labelContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = text;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			return new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, false, "", null);
		}

		// Token: 0x04001465 RID: 5221
		public static readonly LocalizedString ShortcutBindText = TextManager.Get("SocialOverlayShortcutBind");

		// Token: 0x04001467 RID: 5223
		private readonly SocialOverlay.NotificationHandler notificationHandler;

		// Token: 0x04001468 RID: 5224
		private readonly SocialOverlay.InviteHandler inviteHandler;

		// Token: 0x04001469 RID: 5225
		private readonly GUIFrame background;

		// Token: 0x0400146A RID: 5226
		private readonly GUIButton linkHint;

		// Token: 0x0400146B RID: 5227
		private readonly GUILayoutGroup contentLayout;

		// Token: 0x0400146C RID: 5228
		private readonly GUIFrame selectedFriendInfoFrame;

		// Token: 0x0400146D RID: 5229
		private const float WidthToHeightRatio = 7f;

		// Token: 0x0400146E RID: 5230
		private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(30.0);

		// Token: 0x0400146F RID: 5231
		private DateTime lastRefreshTime;

		// Token: 0x04001470 RID: 5232
		public bool IsOpen;

		// Token: 0x04001471 RID: 5233
		private readonly FriendProvider friendProvider;

		// Token: 0x04001472 RID: 5234
		private readonly GUILayoutGroup selfPlayerRowLayout;

		// Token: 0x04001473 RID: 5235
		[Nullable(2)]
		private readonly GUIButton eosConfigButton;

		// Token: 0x04001474 RID: 5236
		[Nullable(2)]
		private readonly GUILayoutGroup eosStatusTextContainer;

		// Token: 0x04001475 RID: 5237
		private EosInterface.Core.Status eosLastKnownStatus;

		// Token: 0x04001476 RID: 5238
		private readonly GUIListBox friendPlayerListBox;

		// Token: 0x04001477 RID: 5239
		private readonly List<SocialOverlay.PlayerRow> friendPlayerRows = new List<SocialOverlay.PlayerRow>();

		// Token: 0x02000D57 RID: 3415
		[Nullable(0)]
		private sealed class NotificationHandler
		{
			// Token: 0x060080C8 RID: 32968 RVA: 0x00395F08 File Offset: 0x00394108
			public void Update()
			{
				DateTime now = DateTime.Now;
				float cumulativeNotificationOffset = 0f;
				for (int i = this.notifications.Count - 1; i >= 0; i--)
				{
					SocialOverlay.NotificationHandler.Notification notification = this.notifications[i];
					DateTime expiryTime = notification.ReceiveTime + SocialOverlay.NotificationHandler.notificationDuration;
					if (now > expiryTime || notification.GuiElement.Parent == null)
					{
						this.RemoveNotification(notification);
					}
					else
					{
						TimeSpan diffToStart = now - notification.ReceiveTime;
						TimeSpan diffToEnd = expiryTime - now;
						float offsetToAdd = 1f;
						offsetToAdd = Math.Min(offsetToAdd, (float)diffToStart.TotalSeconds / (float)SocialOverlay.NotificationHandler.notificationEasingTimeSpan.TotalSeconds);
						offsetToAdd = Math.Min(offsetToAdd, (float)diffToEnd.TotalSeconds / (float)SocialOverlay.NotificationHandler.notificationEasingTimeSpan.TotalSeconds);
						offsetToAdd = Math.Max(offsetToAdd, 0f);
						cumulativeNotificationOffset += offsetToAdd;
						notification.GuiElement.RectTransform.RelativeOffset = new ValueTuple<float, float>(0f, cumulativeNotificationOffset - 1f);
					}
				}
			}

			// Token: 0x060080C9 RID: 32969 RVA: 0x00396013 File Offset: 0x00394213
			public void AddToGuiUpdateList()
			{
				this.NotificationContainer.AddToGUIUpdateList(false, 0);
			}

			// Token: 0x060080CA RID: 32970 RVA: 0x00396022 File Offset: 0x00394222
			public void AddNotification(SocialOverlay.NotificationHandler.Notification notification)
			{
				this.notifications.Add(notification);
			}

			// Token: 0x060080CB RID: 32971 RVA: 0x00396030 File Offset: 0x00394230
			public void RemoveNotification(SocialOverlay.NotificationHandler.Notification notification)
			{
				this.notifications.Remove(notification);
				this.NotificationContainer.RemoveChild(notification.GuiElement);
			}

			// Token: 0x04004F26 RID: 20262
			private readonly List<SocialOverlay.NotificationHandler.Notification> notifications = new List<SocialOverlay.NotificationHandler.Notification>();

			// Token: 0x04004F27 RID: 20263
			private static readonly TimeSpan notificationDuration = TimeSpan.FromSeconds(8.0);

			// Token: 0x04004F28 RID: 20264
			private static readonly TimeSpan notificationEasingTimeSpan = TimeSpan.FromSeconds(0.5);

			// Token: 0x04004F29 RID: 20265
			public readonly GUIFrame NotificationContainer = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.4f, 0.15f), GUI.Canvas, Anchor.BottomRight, null, null, null, ScaleBasis.BothHeight), null, null)
			{
				CanBeFocused = false
			};

			// Token: 0x02001542 RID: 5442
			[Nullable(0)]
			public class Notification : IEquatable<SocialOverlay.NotificationHandler.Notification>
			{
				// Token: 0x06009D37 RID: 40247 RVA: 0x003EC6B0 File Offset: 0x003EA8B0
				public Notification(DateTime ReceiveTime, GUIComponent GuiElement)
				{
					this.ReceiveTime = ReceiveTime;
					this.GuiElement = GuiElement;
					base..ctor();
				}

				// Token: 0x17001D94 RID: 7572
				// (get) Token: 0x06009D38 RID: 40248 RVA: 0x003EC6C6 File Offset: 0x003EA8C6
				[CompilerGenerated]
				protected virtual Type EqualityContract
				{
					[CompilerGenerated]
					get
					{
						return typeof(SocialOverlay.NotificationHandler.Notification);
					}
				}

				// Token: 0x17001D95 RID: 7573
				// (get) Token: 0x06009D39 RID: 40249 RVA: 0x003EC6D2 File Offset: 0x003EA8D2
				// (set) Token: 0x06009D3A RID: 40250 RVA: 0x003EC6DA File Offset: 0x003EA8DA
				public DateTime ReceiveTime { get; set; }

				// Token: 0x17001D96 RID: 7574
				// (get) Token: 0x06009D3B RID: 40251 RVA: 0x003EC6E3 File Offset: 0x003EA8E3
				// (set) Token: 0x06009D3C RID: 40252 RVA: 0x003EC6EB File Offset: 0x003EA8EB
				public GUIComponent GuiElement { get; set; }

				// Token: 0x06009D3D RID: 40253 RVA: 0x003EC6F4 File Offset: 0x003EA8F4
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("Notification");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009D3E RID: 40254 RVA: 0x003EC740 File Offset: 0x003EA940
				[CompilerGenerated]
				protected virtual bool PrintMembers(StringBuilder builder)
				{
					RuntimeHelpers.EnsureSufficientExecutionStack();
					builder.Append("ReceiveTime = ");
					builder.Append(this.ReceiveTime.ToString());
					builder.Append(", GuiElement = ");
					builder.Append(this.GuiElement);
					return true;
				}

				// Token: 0x06009D3F RID: 40255 RVA: 0x003EC793 File Offset: 0x003EA993
				[NullableContext(2)]
				[CompilerGenerated]
				public static bool operator !=(SocialOverlay.NotificationHandler.Notification left, SocialOverlay.NotificationHandler.Notification right)
				{
					return !(left == right);
				}

				// Token: 0x06009D40 RID: 40256 RVA: 0x003EC79F File Offset: 0x003EA99F
				[NullableContext(2)]
				[CompilerGenerated]
				public static bool operator ==(SocialOverlay.NotificationHandler.Notification left, SocialOverlay.NotificationHandler.Notification right)
				{
					return left == right || (left != null && left.Equals(right));
				}

				// Token: 0x06009D41 RID: 40257 RVA: 0x003EC7B3 File Offset: 0x003EA9B3
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return (EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(this.<ReceiveTime>k__BackingField)) * -1521134295 + EqualityComparer<GUIComponent>.Default.GetHashCode(this.<GuiElement>k__BackingField);
				}

				// Token: 0x06009D42 RID: 40258 RVA: 0x003EC7F3 File Offset: 0x003EA9F3
				[NullableContext(2)]
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return this.Equals(obj as SocialOverlay.NotificationHandler.Notification);
				}

				// Token: 0x06009D43 RID: 40259 RVA: 0x003EC804 File Offset: 0x003EAA04
				[NullableContext(2)]
				[CompilerGenerated]
				public virtual bool Equals(SocialOverlay.NotificationHandler.Notification other)
				{
					return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<DateTime>.Default.Equals(this.<ReceiveTime>k__BackingField, other.<ReceiveTime>k__BackingField) && EqualityComparer<GUIComponent>.Default.Equals(this.<GuiElement>k__BackingField, other.<GuiElement>k__BackingField));
				}

				// Token: 0x06009D45 RID: 40261 RVA: 0x003EC865 File Offset: 0x003EAA65
				[CompilerGenerated]
				protected Notification(SocialOverlay.NotificationHandler.Notification original)
				{
					this.ReceiveTime = original.<ReceiveTime>k__BackingField;
					this.GuiElement = original.<GuiElement>k__BackingField;
				}

				// Token: 0x06009D46 RID: 40262 RVA: 0x003EC885 File Offset: 0x003EAA85
				[CompilerGenerated]
				public void Deconstruct(out DateTime ReceiveTime, out GUIComponent GuiElement)
				{
					ReceiveTime = this.ReceiveTime;
					GuiElement = this.GuiElement;
				}
			}
		}

		// Token: 0x02000D58 RID: 3416
		[Nullable(0)]
		private sealed class InviteHandler : IDisposable
		{
			// Token: 0x060080CE RID: 32974 RVA: 0x003960F0 File Offset: 0x003942F0
			public InviteHandler(SocialOverlay inSocialOverlay, FriendProvider inFriendProvider, SocialOverlay.NotificationHandler inNotificationHandler)
			{
				this.socialOverlay = inSocialOverlay;
				this.friendProvider = inFriendProvider;
				this.notificationHandler = inNotificationHandler;
				this.inviteReceivedEventIdentifier = this.GetHashCode().ToIdentifier<int>();
				EosInterface.Presence.OnInviteReceived.Register(this.inviteReceivedEventIdentifier, new Action<EosInterface.Presence.ReceiveInviteInfo>(this.OnEosInviteReceived));
				SteamFriends.OnChatMessage += this.OnSteamChatMsgReceived;
			}

			// Token: 0x060080CF RID: 32975 RVA: 0x00396164 File Offset: 0x00394364
			private void OnSteamChatMsgReceived(Friend steamFriend, string msgType, string msgContent)
			{
				if (!string.Equals(msgType, "InviteGame"))
				{
					return;
				}
				Barotrauma.Networking.SteamId friendId = new Barotrauma.Networking.SteamId(steamFriend.Id);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ReceivedInviteFrom");
				defaultInterpolatedStringHandler.AppendFormatted<Barotrauma.Networking.SteamId>(friendId);
				TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), this.friendProvider.RetrieveFriend(friendId), delegate(Task t)
				{
					Option<FriendInfo> friendInfoOption;
					if (!t.TryGetResult(out friendInfoOption))
					{
						return;
					}
					FriendInfo friendInfo;
					if (!friendInfoOption.TryUnwrap(out friendInfo))
					{
						return;
					}
					this.RegisterInvite(friendInfo, false);
				});
			}

			// Token: 0x060080D0 RID: 32976 RVA: 0x003961D4 File Offset: 0x003943D4
			private void OnEosInviteReceived(EosInterface.Presence.ReceiveInviteInfo info)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ReceivedInviteFrom");
				defaultInterpolatedStringHandler.AppendFormatted<EpicAccountId>(info.SenderId);
				TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), this.friendProvider.RetrieveFriendWithAvatar(info.SenderId, this.notificationHandler.NotificationContainer.Rect.Height), delegate(Task t)
				{
					Option<FriendInfo> friendInfoOption;
					if (!t.TryGetResult(out friendInfoOption))
					{
						return;
					}
					FriendInfo friendInfo;
					if (!friendInfoOption.TryUnwrap(out friendInfo))
					{
						return;
					}
					this.RegisterInvite(friendInfo, true);
				});
			}

			// Token: 0x060080D1 RID: 32977 RVA: 0x00396248 File Offset: 0x00394448
			public bool HasInviteFrom(AccountId sender)
			{
				return this.invites.Any((SocialOverlay.InviteHandler.Invite invite) => invite.Sender.Id == sender);
			}

			// Token: 0x060080D2 RID: 32978 RVA: 0x0039627C File Offset: 0x0039447C
			public void ClearInvitesFrom(AccountId sender)
			{
				foreach (SocialOverlay.InviteHandler.Invite invite2 in this.invites)
				{
					SocialOverlay.NotificationHandler.Notification notification;
					if (invite2.Sender.Id == sender && invite2.NotificationOption.TryUnwrap(out notification))
					{
						this.notificationHandler.RemoveNotification(notification);
					}
				}
				this.invites.RemoveAll((SocialOverlay.InviteHandler.Invite invite) => invite.Sender.Id == sender);
				EpicAccountId friendEpicId = sender as EpicAccountId;
				if (friendEpicId == null)
				{
					return;
				}
				ImmutableArray<EpicAccountId> selfEpicIds = EosInterface.IdQueries.GetLoggedInEpicIds();
				if (selfEpicIds.Length == 0)
				{
					return;
				}
				EpicAccountId selfEpicId = selfEpicIds[0];
				EosInterface.Presence.DeclineInvite(selfEpicId, friendEpicId);
			}

			// Token: 0x060080D3 RID: 32979 RVA: 0x0039635C File Offset: 0x0039455C
			public void Update()
			{
				DateTime now = DateTime.Now;
				for (int i = this.invites.Count - 1; i >= 0; i--)
				{
					SocialOverlay.InviteHandler.Invite invite = this.invites[i];
					DateTime expiryTime = invite.ReceiveTime + SocialOverlay.InviteHandler.inviteDuration;
					if (now > expiryTime)
					{
						SocialOverlay.NotificationHandler.Notification notification;
						if (invite.NotificationOption.TryUnwrap(out notification))
						{
							this.notificationHandler.RemoveNotification(notification);
						}
						this.invites.RemoveAt(i);
					}
				}
			}

			// Token: 0x060080D4 RID: 32980 RVA: 0x003963DC File Offset: 0x003945DC
			private void RegisterInvite(FriendInfo senderInfo, bool showNotification)
			{
				SocialOverlay.InviteHandler.<>c__DisplayClass13_0 CS$<>8__locals1 = new SocialOverlay.InviteHandler.<>c__DisplayClass13_0();
				CS$<>8__locals1.senderInfo = senderInfo;
				CS$<>8__locals1.<>4__this = this;
				DateTime now = DateTime.Now;
				FriendInfo senderInfo2 = CS$<>8__locals1.senderInfo;
				DateTime receiveTime = now;
				Option.UnspecifiedNone none = Option.None;
				SocialOverlay.InviteHandler.Invite invite = new SocialOverlay.InviteHandler.Invite(senderInfo2, receiveTime, none);
				if (showNotification)
				{
					SocialOverlay.InviteHandler.<>c__DisplayClass13_1 CS$<>8__locals2 = new SocialOverlay.InviteHandler.<>c__DisplayClass13_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					GUIButton baseButton = new GUIButton(new RectTransform(Vector2.One, this.notificationHandler.NotificationContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = new ValueTuple<float, float>(0f, -1f)
					}, Alignment.Center, "SocialOverlayPopup", null);
					baseButton.Frame.OutlineThickness = 1f;
					GUILayoutGroup topLayout = new GUILayoutGroup(new RectTransform(Vector2.One, baseButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
					{
						Stretch = true,
						RelativeSpacing = 0.05f
					};
					GUIFrame avatarContainer = new GUIFrame(new RectTransform(Vector2.One, topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null);
					GUICustomComponent avatarComponent = new GUICustomComponent(new RectTransform(Vector2.One * 0.8f, avatarContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothHeight), delegate(SpriteBatch sb, GUICustomComponent component)
					{
						Sprite avatar;
						if (!CS$<>8__locals2.CS$<>8__locals1.senderInfo.Avatar.TryUnwrap(out avatar))
						{
							return;
						}
						Rectangle rect = component.Rect;
						sb.Draw(avatar.Texture, rect, new Rectangle?(avatar.Texture.Bounds), Color.White);
					}, null);
					CS$<>8__locals2.textLayout = new GUILayoutGroup(new RectTransform(Vector2.One, topLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
					{
						Stretch = true
					};
					CS$<>8__locals2.<RegisterInvite>g__addPadding|1();
					CS$<>8__locals2.<RegisterInvite>g__addText|2(CS$<>8__locals2.CS$<>8__locals1.senderInfo.Name, GUIStyle.SubHeadingFont);
					CS$<>8__locals2.<RegisterInvite>g__addText|2(TextManager.Get("InvitedYou"), GUIStyle.Font);
					CS$<>8__locals2.<RegisterInvite>g__addPadding|1();
					CS$<>8__locals2.<RegisterInvite>g__addText|2(TextManager.GetWithVariable("ClickHereOrPressSocialOverlayShortcut", "[shortcut]", SocialOverlay.ShortcutBindText, FormatCapitals.No), GUIStyle.SmallFont);
					CS$<>8__locals2.<RegisterInvite>g__addPadding|1();
					CS$<>8__locals2.notification = new SocialOverlay.NotificationHandler.Notification(now, baseButton);
					baseButton.OnClicked = delegate(GUIButton _, object _)
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.socialOverlay.IsOpen = true;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.notificationHandler.RemoveNotification(CS$<>8__locals2.notification);
						return false;
					};
					baseButton.OnSecondaryClicked = delegate(GUIComponent _, object _)
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.notificationHandler.RemoveNotification(CS$<>8__locals2.notification);
						return false;
					};
					this.notificationHandler.AddNotification(CS$<>8__locals2.notification);
					SocialOverlay.InviteHandler.Invite invite2 = invite;
					invite2.NotificationOption = Option.Some<SocialOverlay.NotificationHandler.Notification>(CS$<>8__locals2.notification);
					invite = invite2;
				}
				this.invites.Add(invite);
			}

			// Token: 0x060080D5 RID: 32981 RVA: 0x003966B6 File Offset: 0x003948B6
			public void Dispose()
			{
				EosInterface.Presence.OnInviteReceived.Deregister(this.inviteReceivedEventIdentifier);
				SteamFriends.OnChatMessage -= this.OnSteamChatMsgReceived;
			}

			// Token: 0x04004F2A RID: 20266
			private readonly SocialOverlay socialOverlay;

			// Token: 0x04004F2B RID: 20267
			private readonly FriendProvider friendProvider;

			// Token: 0x04004F2C RID: 20268
			private readonly SocialOverlay.NotificationHandler notificationHandler;

			// Token: 0x04004F2D RID: 20269
			private readonly List<SocialOverlay.InviteHandler.Invite> invites = new List<SocialOverlay.InviteHandler.Invite>();

			// Token: 0x04004F2E RID: 20270
			private static readonly TimeSpan inviteDuration = TimeSpan.FromMinutes(5.0);

			// Token: 0x04004F2F RID: 20271
			private readonly Identifier inviteReceivedEventIdentifier;

			// Token: 0x02001543 RID: 5443
			[Nullable(0)]
			private readonly struct Invite : IEquatable<SocialOverlay.InviteHandler.Invite>
			{
				// Token: 0x06009D47 RID: 40263 RVA: 0x003EC89B File Offset: 0x003EAA9B
				public Invite(FriendInfo Sender, DateTime ReceiveTime, [Nullable(new byte[]
				{
					0,
					1
				})] Option<SocialOverlay.NotificationHandler.Notification> NotificationOption)
				{
					this.Sender = Sender;
					this.ReceiveTime = ReceiveTime;
					this.NotificationOption = NotificationOption;
				}

				// Token: 0x17001D97 RID: 7575
				// (get) Token: 0x06009D48 RID: 40264 RVA: 0x003EC8B2 File Offset: 0x003EAAB2
				// (set) Token: 0x06009D49 RID: 40265 RVA: 0x003EC8BA File Offset: 0x003EAABA
				public FriendInfo Sender { get; set; }

				// Token: 0x17001D98 RID: 7576
				// (get) Token: 0x06009D4A RID: 40266 RVA: 0x003EC8C3 File Offset: 0x003EAAC3
				// (set) Token: 0x06009D4B RID: 40267 RVA: 0x003EC8CB File Offset: 0x003EAACB
				public DateTime ReceiveTime { get; set; }

				// Token: 0x17001D99 RID: 7577
				// (get) Token: 0x06009D4C RID: 40268 RVA: 0x003EC8D4 File Offset: 0x003EAAD4
				// (set) Token: 0x06009D4D RID: 40269 RVA: 0x003EC8DC File Offset: 0x003EAADC
				[Nullable(new byte[]
				{
					0,
					1
				})]
				public Option<SocialOverlay.NotificationHandler.Notification> NotificationOption { [return: Nullable(new byte[]
				{
					0,
					1
				})] get; [param: Nullable(new byte[]
				{
					0,
					1
				})] set; }

				// Token: 0x06009D4E RID: 40270 RVA: 0x003EC8E8 File Offset: 0x003EAAE8
				[NullableContext(0)]
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("Invite");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009D4F RID: 40271 RVA: 0x003EC934 File Offset: 0x003EAB34
				[NullableContext(0)]
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("Sender = ");
					builder.Append(this.Sender);
					builder.Append(", ReceiveTime = ");
					builder.Append(this.ReceiveTime.ToString());
					builder.Append(", NotificationOption = ");
					builder.Append(this.NotificationOption.ToString());
					return true;
				}

				// Token: 0x06009D50 RID: 40272 RVA: 0x003EC9A9 File Offset: 0x003EABA9
				[CompilerGenerated]
				public static bool operator !=(SocialOverlay.InviteHandler.Invite left, SocialOverlay.InviteHandler.Invite right)
				{
					return !(left == right);
				}

				// Token: 0x06009D51 RID: 40273 RVA: 0x003EC9B5 File Offset: 0x003EABB5
				[CompilerGenerated]
				public static bool operator ==(SocialOverlay.InviteHandler.Invite left, SocialOverlay.InviteHandler.Invite right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009D52 RID: 40274 RVA: 0x003EC9BF File Offset: 0x003EABBF
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return (EqualityComparer<FriendInfo>.Default.GetHashCode(this.<Sender>k__BackingField) * -1521134295 + EqualityComparer<DateTime>.Default.GetHashCode(this.<ReceiveTime>k__BackingField)) * -1521134295 + EqualityComparer<Option<SocialOverlay.NotificationHandler.Notification>>.Default.GetHashCode(this.<NotificationOption>k__BackingField);
				}

				// Token: 0x06009D53 RID: 40275 RVA: 0x003EC9FF File Offset: 0x003EABFF
				[NullableContext(0)]
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is SocialOverlay.InviteHandler.Invite && this.Equals((SocialOverlay.InviteHandler.Invite)obj);
				}

				// Token: 0x06009D54 RID: 40276 RVA: 0x003ECA18 File Offset: 0x003EAC18
				[CompilerGenerated]
				public bool Equals(SocialOverlay.InviteHandler.Invite other)
				{
					return EqualityComparer<FriendInfo>.Default.Equals(this.<Sender>k__BackingField, other.<Sender>k__BackingField) && EqualityComparer<DateTime>.Default.Equals(this.<ReceiveTime>k__BackingField, other.<ReceiveTime>k__BackingField) && EqualityComparer<Option<SocialOverlay.NotificationHandler.Notification>>.Default.Equals(this.<NotificationOption>k__BackingField, other.<NotificationOption>k__BackingField);
				}

				// Token: 0x06009D55 RID: 40277 RVA: 0x003ECA6D File Offset: 0x003EAC6D
				[CompilerGenerated]
				public void Deconstruct(out FriendInfo Sender, out DateTime ReceiveTime, [Nullable(new byte[]
				{
					0,
					1
				})] out Option<SocialOverlay.NotificationHandler.Notification> NotificationOption)
				{
					Sender = this.Sender;
					ReceiveTime = this.ReceiveTime;
					NotificationOption = this.NotificationOption;
				}
			}
		}

		// Token: 0x02000D59 RID: 3417
		[Nullable(0)]
		private readonly struct PlayerRow
		{
			// Token: 0x060080D9 RID: 32985 RVA: 0x00396748 File Offset: 0x00394948
			internal PlayerRow(FriendInfo friendInfo, GUILayoutGroup containerLayout, bool invitedYou, [Nullable(new byte[]
			{
				2,
				1
			})] IEnumerable<LocalizedString> metadataText = null)
			{
				SocialOverlay.PlayerRow.<>c__DisplayClass3_0 CS$<>8__locals1 = new SocialOverlay.PlayerRow.<>c__DisplayClass3_0();
				CS$<>8__locals1.friendInfo = friendInfo;
				this.FriendInfo = CS$<>8__locals1.friendInfo;
				this.AvatarContainer = new GUIFrame(new RectTransform(Vector2.One, containerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null);
				this.InfoContainer = new GUIFrame(new RectTransform(Vector2.One, containerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				FriendInfo friendInfo2 = CS$<>8__locals1.friendInfo;
				Option.UnspecifiedNone none = Option.None;
				friendInfo2.RetrieveOrInheritAvatar(none, this.AvatarContainer.Rect.Height);
				SocialOverlay.PlayerRow.<>c__DisplayClass3_0 CS$<>8__locals2 = CS$<>8__locals1;
				RectTransform rectT = new RectTransform(Vector2.One * 0.9f, this.AvatarContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
				string style;
				if (!invitedYou)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Friend");
					defaultInterpolatedStringHandler.AppendFormatted<FriendStatus>(CS$<>8__locals1.friendInfo.CurrentStatus);
					style = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					style = "FriendInvitedYou";
				}
				CS$<>8__locals2.avatarBackground = new GUIFrame(rectT, style, null);
				CS$<>8__locals1.textLayout = new GUILayoutGroup(new RectTransform(Vector2.One, this.InfoContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true
				};
				CS$<>8__locals1.textBlocks = new List<GUITextBlock>();
				CS$<>8__locals1.<.ctor>g__addTextLayoutPadding|0();
				CS$<>8__locals1.<.ctor>g__addTextBlock|1(CS$<>8__locals1.friendInfo.Name, GUIStyle.SubHeadingFont);
				if (metadataText == null)
				{
					metadataText = new LocalizedString[]
					{
						CS$<>8__locals1.friendInfo.StatusText
					};
				}
				foreach (LocalizedString line in metadataText)
				{
					CS$<>8__locals1.<.ctor>g__addTextBlock|1(line, GUIStyle.Font);
				}
				CS$<>8__locals1.<.ctor>g__addTextLayoutPadding|0();
				RectTransform rectT2 = new RectTransform(Vector2.One, CS$<>8__locals1.avatarBackground.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				Action<float, GUICustomComponent> onUpdate = new Action<float, GUICustomComponent>(CS$<>8__locals1.<.ctor>g__updateTextAlignments|2);
				new GUICustomComponent(rectT2, new Action<SpriteBatch, GUICustomComponent>(CS$<>8__locals1.<.ctor>g__drawAvatar|3), onUpdate);
				if (invitedYou)
				{
					GUIImage inviteIcon = new GUIImage(new RectTransform(new Vector2(0.5f), CS$<>8__locals1.avatarBackground.RectTransform, Anchor.TopRight, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal)
					{
						RelativeOffset = Vector2.One * 0.15f
					}, "InviteNotification", GUIImage.ScalingMode.None)
					{
						ToolTip = TextManager.Get("InviteNotification")
					};
					GUIImage guiimage = inviteIcon;
					guiimage.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guiimage.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
					{
						if (component.FlashTimer <= 0f)
						{
							component.Flash(new Color?(GUIStyle.Green), 1.5f, false, true, null);
							component.Pulsate(Vector2.One, Vector2.One * 1.5f, 0.5f);
						}
					}));
				}
			}

			// Token: 0x04004F30 RID: 20272
			public readonly GUIFrame AvatarContainer;

			// Token: 0x04004F31 RID: 20273
			public readonly GUIFrame InfoContainer;

			// Token: 0x04004F32 RID: 20274
			public readonly FriendInfo FriendInfo;
		}
	}
}
