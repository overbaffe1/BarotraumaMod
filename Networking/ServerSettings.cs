using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma.Networking
{
	// Token: 0x02000471 RID: 1137
	internal class ServerSettings : ISerializableEntity
	{
		// Token: 0x06004CA0 RID: 19616 RVA: 0x002A4650 File Offset: 0x002A2850
		public void ClientAdminRead(IReadMessage incMsg)
		{
			for (;;)
			{
				uint key = incMsg.ReadUInt32();
				if (key == 0U)
				{
					break;
				}
				if (this.netProperties.ContainsKey(key))
				{
					bool changedLocally = this.netProperties[key].ChangedLocally;
					this.netProperties[key].Read(incMsg);
					this.netProperties[key].TempValue = this.netProperties[key].Value;
					if (this.netProperties[key].GUIComponent != null && !changedLocally)
					{
						this.netProperties[key].GUIComponentValue = this.netProperties[key].Value;
					}
				}
				else
				{
					uint size = incMsg.ReadVariableUInt32();
					incMsg.BitPosition += (int)(8U * size);
				}
			}
			if (this.ReadMonsterEnabled(incMsg))
			{
				GUIComponent guicomponent = this.monstersEnabledPanel;
				if (guicomponent != null && guicomponent.Visible)
				{
					GUIComponent parent = this.monstersEnabledPanel.Parent;
					if (parent != null)
					{
						parent.RemoveChild(this.monstersEnabledPanel);
					}
					this.monstersEnabledPanel = this.CreateMonstersEnabledPanel();
					this.monstersEnabledPanel.Visible = true;
				}
			}
			this.BanList.ClientAdminRead(incMsg);
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (netLobbyScreen == null)
			{
				return;
			}
			netLobbyScreen.RefreshPlaystyleIcons();
		}

		// Token: 0x06004CA1 RID: 19617 RVA: 0x002A4784 File Offset: 0x002A2984
		public void ClientRead(IReadMessage incMsg)
		{
			ServerSettings.NetFlags requiredFlags = (ServerSettings.NetFlags)incMsg.ReadByte();
			this.PlayStyle = (PlayStyle)incMsg.ReadByte();
			this.MaxPlayers = (int)incMsg.ReadByte();
			this.HasPassword = incMsg.ReadBoolean();
			this.IsPublic = incMsg.ReadBoolean();
			GameClient.SetLobbyPublic(this.IsPublic);
			this.AllowFileTransfers = incMsg.ReadBoolean();
			incMsg.ReadPadBits();
			this.TickRate = incMsg.ReadRangedInteger(1, 60);
			if (requiredFlags.HasFlag(ServerSettings.NetFlags.Properties))
			{
				if (this.ReadExtraCargo(incMsg))
				{
					GUIComponent guicomponent = this.extraCargoPanel;
					if (guicomponent != null && guicomponent.Visible)
					{
						GUIComponent parent = this.extraCargoPanel.Parent;
						if (parent != null)
						{
							parent.RemoveChild(this.extraCargoPanel);
						}
						this.extraCargoPanel = this.CreateExtraCargoPanel();
						this.extraCargoPanel.Visible = true;
					}
				}
				if (this.ReadPerks(incMsg))
				{
					NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
					if (netLobbyScreen != null)
					{
						netLobbyScreen.UpdateDisembarkPointListFromServerSettings();
					}
				}
			}
			if (requiredFlags.HasFlag(ServerSettings.NetFlags.HiddenSubs))
			{
				this.ReadHiddenSubs(incMsg);
			}
			GameMain.NetLobbyScreen.UpdateSubVisibility();
			bool isAdmin = incMsg.ReadBoolean();
			incMsg.ReadPadBits();
			if (isAdmin)
			{
				this.ClientAdminRead(incMsg);
			}
		}

		// Token: 0x06004CA2 RID: 19618 RVA: 0x002A48AC File Offset: 0x002A2AAC
		public static bool HasPermissionToChangePerks()
		{
			if (GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				return true;
			}
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			bool isPvP = ((netLobbyScreen != null) ? netLobbyScreen.SelectedMode : null) == GameModePreset.PvP;
			CharacterTeamType teamPreference = MultiplayerPreferences.Instance.TeamPreference;
			bool flag = teamPreference - CharacterTeamType.Team1 <= 1;
			bool hasSelectedTeam = flag;
			GameClient client = GameMain.Client;
			ImmutableArray<Client> immutableArray;
			if (client == null)
			{
				immutableArray = ImmutableArray<Client>.Empty;
			}
			else
			{
				immutableArray = (from c in client.ConnectedClients
				where c.SessionId != GameMain.Client.SessionId
				select c).ToImmutableArray<Client>();
			}
			ImmutableArray<Client> otherClients = immutableArray;
			if (!isPvP)
			{
				return !otherClients.Any((Client c) => c.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings));
			}
			if (!hasSelectedTeam)
			{
				return false;
			}
			return !(from c in otherClients
			where c.PreferredTeam == MultiplayerPreferences.Instance.TeamPreference
			select c).Any((Client c) => c.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings));
		}

		// Token: 0x06004CA3 RID: 19619 RVA: 0x002A49C0 File Offset: 0x002A2BC0
		public void ClientAdminWritePerks()
		{
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteByte(3);
			this.WritePerks(outMsg);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(outMsg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004CA4 RID: 19620 RVA: 0x002A4A00 File Offset: 0x002A2C00
		public void ClientAdminWrite(ServerSettings.NetFlags dataToSend, Identifier addedMissionType = default(Identifier), Identifier removedMissionType = default(Identifier), int traitorDangerLevel = 0)
		{
			if (!GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				return;
			}
			if (ServerSettings.SuppressNetworkMessages)
			{
				return;
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteByte(2);
			outMsg.WriteByte((byte)dataToSend);
			if (dataToSend.HasFlag(ServerSettings.NetFlags.Properties))
			{
				this.WriteExtraCargo(outMsg);
				IEnumerable<KeyValuePair<uint, ServerSettings.NetPropertyData>> changedProperties = from kvp in this.netProperties
				where kvp.Value.ChangedLocally
				select kvp;
				uint count = (uint)changedProperties.Count<KeyValuePair<uint, ServerSettings.NetPropertyData>>();
				bool changedMonsterSettings = this.tempMonsterEnabled != null && this.tempMonsterEnabled.Any((KeyValuePair<Identifier, bool> p) => p.Value != this.MonsterEnabled[p.Key]);
				outMsg.WriteUInt32(count);
				foreach (KeyValuePair<uint, ServerSettings.NetPropertyData> prop in changedProperties)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Changed ");
					defaultInterpolatedStringHandler.AppendFormatted(prop.Value.Name.Value);
					defaultInterpolatedStringHandler.AppendLiteral(" to ");
					defaultInterpolatedStringHandler.AppendFormatted<object>(prop.Value.GUIComponentValue);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
					outMsg.WriteUInt32(prop.Key);
					prop.Value.Write(outMsg, prop.Value.GUIComponentValue);
				}
				outMsg.WriteBoolean(changedMonsterSettings);
				outMsg.WritePadBits();
				if (changedMonsterSettings)
				{
					this.WriteMonsterEnabled(outMsg, this.tempMonsterEnabled);
				}
				this.BanList.ClientAdminWrite(outMsg);
			}
			if (dataToSend.HasFlag(ServerSettings.NetFlags.HiddenSubs))
			{
				this.WriteHiddenSubs(outMsg);
			}
			if (dataToSend.HasFlag(ServerSettings.NetFlags.Misc))
			{
				outMsg.WriteIdentifier(addedMissionType);
				outMsg.WriteIdentifier(removedMissionType);
				outMsg.WriteByte((byte)(traitorDangerLevel + 1));
				outMsg.WritePadBits();
			}
			if (dataToSend.HasFlag(ServerSettings.NetFlags.LevelSeed))
			{
				outMsg.WriteString(GameMain.NetLobbyScreen.LevelSeedBox.Text);
			}
			GameMain.Client.ClientPeer.Send(outMsg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06004CA5 RID: 19621 RVA: 0x002A4C34 File Offset: 0x002A2E34
		private ServerSettings.NetPropertyData GetPropertyData(string name)
		{
			KeyValuePair<uint, ServerSettings.NetPropertyData> matchingProperty = this.netProperties.FirstOrDefault(delegate(KeyValuePair<uint, ServerSettings.NetPropertyData> p)
			{
				Identifier name2 = p.Value.Name;
				return name2 == name;
			});
			if (matchingProperty.Equals(default(KeyValuePair<uint, ServerSettings.NetPropertyData>)))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a ");
				defaultInterpolatedStringHandler.AppendFormatted("ServerSettings");
				defaultInterpolatedStringHandler.AppendLiteral(" property with the name \"");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return matchingProperty.Value;
		}

		// Token: 0x06004CA6 RID: 19622 RVA: 0x002A4CDE File Offset: 0x002A2EDE
		public void AssignGUIComponent(string propertyName, GUIComponent component)
		{
			this.GetPropertyData(propertyName).AssignGUIComponent(component);
		}

		// Token: 0x06004CA7 RID: 19623 RVA: 0x002A4CED File Offset: 0x002A2EED
		public void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			GUIFrame guiframe = this.settingsFrame;
			if (guiframe == null)
			{
				return;
			}
			guiframe.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06004CA8 RID: 19624 RVA: 0x002A4D0C File Offset: 0x002A2F0C
		private void CreateSettingsFrame()
		{
			foreach (ServerSettings.NetPropertyData prop in this.netProperties.Values)
			{
				prop.TempValue = prop.Value;
			}
			this.settingsFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.settingsFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, this.settingsFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Alignment.Center, null, null);
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock)
				{
					this.ToggleSettingsFrame(btn, userData);
				}
				return true;
			}));
			new GUIButton(new RectTransform(Vector2.One, this.settingsFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Alignment.Center, null, null).OnClicked = new GUIButton.OnClickedHandler(this.ToggleSettingsFrame);
			GUIFrame innerFrame = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.85f), this.settingsFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 430)
			}, "", null);
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(innerFrame.Rect.Size - new Point(GUI.IntScale(20f)), innerFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("serversettingsbutton");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			GUILayoutGroup buttonArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUIFrame tabContent = new GUIFrame(new RectTransform(new Vector2(1f, 0.85f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			IEnumerable<ServerSettings.SettingsTab> settingsTabTypes = Enum.GetValues(typeof(ServerSettings.SettingsTab)).Cast<ServerSettings.SettingsTab>();
			foreach (ServerSettings.SettingsTab settingsTab in settingsTabTypes)
			{
				this.settingsTabs[settingsTab] = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), tabContent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				Dictionary<ServerSettings.SettingsTab, GUIButton> dictionary = this.tabButtons;
				ServerSettings.SettingsTab key = settingsTab;
				RectTransform rectT2 = new RectTransform(new Vector2(0.2f, 1.2f), buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ServerSettings");
				defaultInterpolatedStringHandler.AppendFormatted<ServerSettings.SettingsTab>(settingsTab);
				defaultInterpolatedStringHandler.AppendLiteral("Tab");
				dictionary[key] = new GUIButton(rectT2, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), Alignment.Center, "GUITabButton", null)
				{
					UserData = settingsTab,
					OnClicked = new GUIButton.OnClickedHandler(this.SelectSettingsTab)
				};
			}
			GUITextBlock.AutoScaleAndNormalize(from b in this.tabButtons.Values
			select b.TextBlock, true, false, null);
			this.SelectSettingsTab(this.tabButtons[ServerSettings.SettingsTab.ServerIdentity], 0);
			this.tabButtons[ServerSettings.SettingsTab.Banlist].Enabled = (GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.Ban) || GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.Unban));
			GUIFrame buttonContainer = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.05f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Vector2(0.25f, 1f), buttonContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null).OnClicked = new GUIButton.OnClickedHandler(this.ToggleSettingsFrame);
			this.CreateServerIdentityTab(this.settingsTabs[ServerSettings.SettingsTab.ServerIdentity]);
			this.CreateGeneralTab(this.settingsTabs[ServerSettings.SettingsTab.General]);
			this.CreateAntigriefingTab(this.settingsTabs[ServerSettings.SettingsTab.Antigriefing]);
			this.CreateBanlistTab(this.settingsTabs[ServerSettings.SettingsTab.Banlist]);
			if (GameMain.Client == null || !GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				foreach (KeyValuePair<ServerSettings.SettingsTab, GUIComponent> settingsTab2 in this.settingsTabs)
				{
					this.SetElementInteractability(settingsTab2.Value, false);
				}
			}
			this.extraCargoButton.Enabled = (this.monstersEnabledButton.Enabled = true);
		}

		// Token: 0x06004CA9 RID: 19625 RVA: 0x002A5428 File Offset: 0x002A3628
		private void SetElementInteractability(GUIComponent parent, bool interactable)
		{
			foreach (GUIComponent child in parent.GetAllChildren<GUIComponent>())
			{
				child.Enabled = interactable;
				child.DisabledColor = new Microsoft.Xna.Framework.Color(child.Color, (float)child.Color.A / 255f * 0.8f);
				GUITextBlock textBlock = child as GUITextBlock;
				if (textBlock != null)
				{
					textBlock.DisabledTextColor = new Microsoft.Xna.Framework.Color(textBlock.TextColor, (float)textBlock.TextColor.A / 255f * 0.8f);
				}
			}
		}

		// Token: 0x06004CAA RID: 19626 RVA: 0x002A54D8 File Offset: 0x002A36D8
		private void CreateServerIdentityTab(GUIComponent parent)
		{
			ServerSettings.<>c__DisplayClass27_0 CS$<>8__locals1 = new ServerSettings.<>c__DisplayClass27_0();
			GameClient client = GameMain.Client;
			if (!(((client != null) ? client.ClientPeer : null) is LidgrenClientPeer))
			{
				GUITickBox isPublic = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("publicserver"), null, "")
				{
					ToolTip = TextManager.Get("publicservertooltip")
				};
				this.AssignGUIComponent("IsPublic", isPublic);
			}
			GUITextBlock serverNameLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerName"), null, null, Alignment.Left, false, "", null);
			GUITextBox serverNameBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), serverNameLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), GameMain.Client.ServerSettings.ServerName, null, null, Alignment.Left, false, "", null, false, true)
			{
				OverflowClip = true,
				MaxTextLength = new int?(NetConfig.ServerNameMaxLength)
			};
			serverNameBox.OnDeselected += delegate(GUITextBox textBox, Keys key)
			{
				if (textBox.Text.IsNullOrWhiteSpace())
				{
					textBox.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Red), 1.5f, false, false, null);
					if (GameMain.Client != null)
					{
						textBox.Text = GameMain.Client.ServerSettings.ServerName;
					}
				}
				GameClient client2 = GameMain.Client;
				if (client2 == null)
				{
					return;
				}
				client2.ServerSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			};
			this.AssignGUIComponent("ServerName", serverNameBox);
			GUITextBlock motdHeader = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerMOTD"), null, null, Alignment.Left, false, "", null);
			GUITextBlock motdCharacterCount = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), motdHeader.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.CenterRight, false, "", null);
			CS$<>8__locals1.serverMessageContainer = new GUIListBox(new RectTransform(new Vector2(1f, 0.2f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = true
			};
			ServerSettings.<>c__DisplayClass27_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT = new RectTransform(Vector2.One, CS$<>8__locals1.serverMessageContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			Microsoft.Xna.Framework.Color? textColor = null;
			GUIFont font = null;
			Alignment textAlignment = Alignment.TopLeft;
			bool wrap = true;
			string style = "GUITextBoxNoBorder";
			Microsoft.Xna.Framework.Color? color = null;
			CS$<>8__locals2.serverMessageBox = new GUITextBox(rectT, text2, textColor, font, textAlignment, wrap, style, color, false, true)
			{
				MaxTextLength = new int?(NetConfig.ServerMessageMaxLength)
			};
			ServerSettings.<>c__DisplayClass27_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(Vector2.One, CS$<>8__locals1.serverMessageBox.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			color = new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.DarkGray * 0.6f);
			GUIFont font2 = GUIStyle.Font;
			CS$<>8__locals3.serverMessageHint = new GUITextBlock(rectT2, TextManager.Get("ClickToWriteServerMessage"), color, font2, Alignment.TopLeft, false, "", null);
			this.AssignGUIComponent("ServerMessageText", CS$<>8__locals1.serverMessageBox);
			CS$<>8__locals1.serverMessageBox.OnSelected += delegate(GUITextBox textBox, Keys key)
			{
				CS$<>8__locals1.serverMessageHint.Visible = false;
				base.<CreateServerIdentityTab>g__updateServerMessageScrollBasedOnCaret|1();
			};
			CS$<>8__locals1.serverMessageBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				CS$<>8__locals1.serverMessageHint.Visible = (!textBox.Selected && !textBox.Readonly && string.IsNullOrWhiteSpace(textBox.Text));
				base.<CreateServerIdentityTab>g__RefreshServerInfoSize|4();
				return true;
			};
			CS$<>8__locals1.serverMessageBox.RectTransform.SizeChanged += CS$<>8__locals1.<CreateServerIdentityTab>g__RefreshServerInfoSize|4;
			GUITextBlock guitextBlock = motdCharacterCount;
			guitextBlock.TextGetter = (GUITextBlock.TextGetterHandler)Delegate.Combine(guitextBlock.TextGetter, new GUITextBlock.TextGetterHandler(() => CS$<>8__locals1.serverMessageBox.Text.Length.ToString() + " / " + NetConfig.ServerMessageMaxLength.ToString()));
			GUITextBox serverMessageBox = CS$<>8__locals1.serverMessageBox;
			serverMessageBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(serverMessageBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
			{
				string str = textBox.Text;
				int caretIndex = textBox.CaretIndex;
				textBox.Text = str.Substring(0, caretIndex) + "\n" + str.Substring(caretIndex);
				textBox.CaretIndex = caretIndex + 1;
				return true;
			}));
			CS$<>8__locals1.serverMessageBox.OnDeselected += delegate(GUITextBox textBox, Keys key)
			{
				if (!textBox.Readonly)
				{
					GameClient client2 = GameMain.Client;
					if (client2 != null)
					{
						ServerSettings serverSettings = client2.ServerSettings;
						if (serverSettings != null)
						{
							serverSettings.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
						}
					}
				}
				CS$<>8__locals1.serverMessageHint.Visible = (!textBox.Readonly && string.IsNullOrWhiteSpace(textBox.Text));
			};
			CS$<>8__locals1.serverMessageBox.OnKeyHit += delegate(GUITextBox sender, Keys key)
			{
				base.<CreateServerIdentityTab>g__updateServerMessageScrollBasedOnCaret|1();
			};
			GUITextBlock playStyleLayoutLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsPlayStyle"), null, null, Alignment.Left, false, "", null);
			GUISelectionCarousel<PlayStyle> playStyleSelection = new GUISelectionCarousel<PlayStyle>(new RectTransform(new Vector2(0.5f, 1f), playStyleLayoutLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<PlayStyle, LocalizedString>>());
			foreach (object obj in Enum.GetValues(typeof(PlayStyle)))
			{
				PlayStyle playStyle = (PlayStyle)obj;
				playStyleSelection.AddElement(playStyle, TextManager.Get("servertag." + playStyle.ToString()), TextManager.Get("servertagdescription." + playStyle.ToString()));
			}
			this.AssignGUIComponent("PlayStyle", playStyleSelection);
			GUITextBlock passwordLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Password"), null, null, Alignment.Left, false, "", null);
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), passwordLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsSetPassword"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				ServerSettings.CreateChangePasswordPrompt();
				return true;
			};
			GUITickBox wrongPasswordBanBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsBanAfterWrongPassword"), null, "");
			this.AssignGUIComponent("BanAfterWrongPassword", wrongPasswordBanBox);
			GUINumberInput allowedPasswordRetries = NetLobbyScreen.CreateLabeledNumberInput(parent, "ServerSettingsPasswordRetriesBeforeBan", 0, 10, null, null);
			this.AssignGUIComponent("MaxPasswordRetriesBeforeBan", allowedPasswordRetries);
			GUINumberInput maxPlayers = NetLobbyScreen.CreateLabeledNumberInput(parent, "MaxPlayers", 0, NetConfig.MaxPlayers, null, null);
			this.AssignGUIComponent("MaxPlayers", maxPlayers);
			GUITextBlock languageLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Language"), null, null, Alignment.Left, false, "", null);
			GUIDropDown languageDD = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), languageLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			foreach (ServerLanguageOptions.LanguageOption language in ServerLanguageOptions.Options)
			{
				languageDD.AddItem(language.Label, language.Identifier, null, null, null);
			}
			languageLabel.InheritTotalChildrenMinHeight();
			this.AssignGUIComponent("Language", languageDD);
		}

		// Token: 0x06004CAB RID: 19627 RVA: 0x002A5DB8 File Offset: 0x002A3FB8
		private static void CreateChangePasswordPrompt()
		{
			GUIMessageBox passwordMsgBox = new GUIMessageBox(TextManager.Get("ServerSettingsSetPassword"), "", new LocalizedString[]
			{
				TextManager.Get("OK"),
				TextManager.Get("Cancel")
			}, new Vector2?(new Vector2(0.25f, 0.1f)), new Point?(new Point(400, GUI.IntScale(170f))), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup passwordHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), passwordMsgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			GUITextBox passwordBox = new GUITextBox(new RectTransform(new Vector2(0.8f, 1f), passwordHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				Censor = true
			};
			passwordMsgBox.Content.Recalculate();
			passwordMsgBox.Content.InheritTotalChildrenHeight();
			passwordMsgBox.Content.Parent.RectTransform.MinSize = new Point(0, (int)((float)passwordMsgBox.Content.RectTransform.MinSize.Y / passwordMsgBox.Content.RectTransform.RelativeSize.Y));
			GUIButton okButton = passwordMsgBox.Buttons[0];
			GUIButton okButton3 = okButton;
			okButton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(okButton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object __)
			{
				DebugConsole.ExecuteCommand("setpassword \"" + passwordBox.Text + "\"");
				return true;
			}));
			GUIButton okButton2 = okButton;
			okButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(okButton2.OnClicked, new GUIButton.OnClickedHandler(passwordMsgBox.Close));
			GUIButton cancelButton = passwordMsgBox.Buttons[1];
			cancelButton.OnClicked = delegate(GUIButton _, object __)
			{
				GUIMessageBox passwordMsgBox = passwordMsgBox;
				if (passwordMsgBox != null)
				{
					passwordMsgBox.Close();
				}
				passwordMsgBox = null;
				return true;
			};
			GUITextBox passwordBox2 = passwordBox;
			passwordBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(passwordBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox _, string __)
			{
				okButton.OnClicked(okButton, okButton.UserData);
				return true;
			}));
			passwordBox.Select(-1, false);
		}

		// Token: 0x06004CAC RID: 19628 RVA: 0x002A6040 File Offset: 0x002A4240
		private void CreateGeneralTab(GUIComponent parent)
		{
			ServerSettings.<>c__DisplayClass29_0 CS$<>8__locals1 = new ServerSettings.<>c__DisplayClass29_0();
			CS$<>8__locals1.<>4__this = this;
			GUIListBox listBox = new GUIListBox(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "GUIListBoxNoBorder", true, false)
			{
				AutoHideScrollBar = true,
				CurrentSelectMode = GUIListBox.SelectMode.None
			};
			NetLobbyScreen.CreateSubHeader("serversettingscategory.roundmanagement", listBox.Content, null);
			GUITickBox endVoteBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsEndRoundVoting"), null, "");
			this.AssignGUIComponent("AllowEndVoting", endVoteBox);
			GUIComponent content = listBox.Content;
			string empty = string.Empty;
			string valueLabelTag = "ServerSettingsEndRoundVotesRequired";
			string empty2 = string.Empty;
			float? step = null;
			Vector2? range = null;
			GUIScrollBar slider;
			GUITextBlock sliderLabel;
			NetLobbyScreen.CreateLabeledSlider(content, empty, valueLabelTag, empty2, out slider, out sliderLabel, step, range);
			CS$<>8__locals1.endRoundLabel = sliderLabel.Text;
			slider.Step = 0.2f;
			slider.Range = new Vector2(0.5f, 1f);
			this.AssignGUIComponent("EndVoteRequiredRatio", slider);
			slider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				((GUITextBlock)scrollBar.UserData).Text = CS$<>8__locals1.endRoundLabel + " " + (int)MathUtils.Round(scrollBar.BarScrollValue * 100f, 10f) + " %";
				return true;
			};
			slider.OnMoved(slider, slider.BarScroll);
			GUITextBlock subSelectionLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsSubSelection"), null, null, Alignment.Left, false, "", null);
			GUISelectionCarousel<SelectionMode> subSelection = new GUISelectionCarousel<SelectionMode>(new RectTransform(new Vector2(0.5f, 1f), subSelectionLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<SelectionMode, LocalizedString>>());
			foreach (object obj in Enum.GetValues(typeof(SelectionMode)))
			{
				SelectionMode selectionMode = (SelectionMode)obj;
				subSelection.AddElement(selectionMode, TextManager.Get(selectionMode.ToString()), null);
			}
			this.AssignGUIComponent("SubSelectionMode", subSelection);
			GUITextBlock gameModeSelectionLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsModeSelection"), null, null, Alignment.Left, false, "", null);
			GUISelectionCarousel<SelectionMode> gameModeSelection = new GUISelectionCarousel<SelectionMode>(new RectTransform(new Vector2(0.5f, 1f), gameModeSelectionLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<SelectionMode, LocalizedString>>());
			foreach (object obj2 in Enum.GetValues(typeof(SelectionMode)))
			{
				SelectionMode selectionMode2 = (SelectionMode)obj2;
				gameModeSelection.AddElement(selectionMode2, TextManager.Get(selectionMode2.ToString()), null);
			}
			this.AssignGUIComponent("ModeSelectionMode", gameModeSelection);
			CS$<>8__locals1.autoRestartDelayLabel = TextManager.Get("ServerSettingsAutoRestartDelay") + " ";
			GUITickBox autorestartBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("AutoRestart"), null, "");
			this.AssignGUIComponent("AutoRestart", autorestartBox);
			GUIComponent content2 = listBox.Content;
			string empty3 = string.Empty;
			string empty4 = string.Empty;
			string empty5 = string.Empty;
			range = new Vector2?(new Vector2(10f, 300f));
			GUIScrollBar startIntervalSlider;
			GUITextBlock startIntervalSliderLabel;
			NetLobbyScreen.CreateLabeledSlider(content2, empty3, empty4, empty5, out startIntervalSlider, out startIntervalSliderLabel, null, range);
			startIntervalSlider.StepValue = 10f;
			startIntervalSlider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				text2.Text = CS$<>8__locals1.autoRestartDelayLabel + ToolBox.SecondsToReadableTime(scrollBar.BarScrollValue);
				return true;
			};
			this.AssignGUIComponent("AutoRestartInterval", startIntervalSlider);
			startIntervalSlider.OnMoved(startIntervalSlider, startIntervalSlider.BarScroll);
			GUITickBox startWhenClientsReady = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsStartWhenClientsReady"), null, "");
			this.AssignGUIComponent("StartWhenClientsReady", startWhenClientsReady);
			GUIComponent content3 = listBox.Content;
			string empty6 = string.Empty;
			string valueLabelTag2 = "ServerSettingsStartWhenClientsReadyRatio";
			string empty7 = string.Empty;
			float? step2 = null;
			range = null;
			NetLobbyScreen.CreateLabeledSlider(content3, empty6, valueLabelTag2, empty7, out slider, out sliderLabel, step2, range);
			CS$<>8__locals1.clientsReadyRequiredLabel = sliderLabel.Text;
			slider.Step = 0.2f;
			slider.Range = new Vector2(0.5f, 1f);
			slider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				((GUITextBlock)scrollBar.UserData).Text = CS$<>8__locals1.clientsReadyRequiredLabel.Replace("[percentage]", ((int)MathUtils.Round(scrollBar.BarScrollValue * 100f, 10f)).ToString(), StringComparison.Ordinal);
				return true;
			};
			this.AssignGUIComponent("StartWhenClientsReadyRatio", slider);
			slider.OnMoved(slider, slider.BarScroll);
			GUITickBox randomizeLevelBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsRandomizeSeed"), null, "");
			this.AssignGUIComponent("RandomizeSeed", randomizeLevelBox);
			NetLobbyScreen.CreateSubHeader("gamemode.pvp", listBox.Content, null);
			GUITextBlock teamSelectModeLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("TeamSelectionMode"), null, null, Alignment.Left, false, "", null);
			teamSelectModeLabel.ToolTip = TextManager.Get("TeamSelectionMode.tooltip");
			GUISelectionCarousel<PvpTeamSelectionMode> teamSelectionMode = new GUISelectionCarousel<PvpTeamSelectionMode>(new RectTransform(new Vector2(0.5f, 0.6f), teamSelectModeLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<PvpTeamSelectionMode, LocalizedString>>());
			foreach (object obj3 in Enum.GetValues(typeof(PvpTeamSelectionMode)))
			{
				PvpTeamSelectionMode teamSelectionModeOption = (PvpTeamSelectionMode)obj3;
				string optionName = teamSelectionModeOption.ToString();
				teamSelectionMode.AddElement(teamSelectionModeOption, TextManager.Get("TeamSelectionMode." + optionName), TextManager.Get("TeamSelectionMode." + optionName + ".tooltip"));
			}
			this.AssignGUIComponent("PvpTeamSelectionMode", teamSelectionMode);
			GUITextBlock autoBalanceThresholdLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("AutoBalanceThreshold"), null, null, Alignment.Left, false, "", null);
			LocalizedString autoBalanceThresholdTooltip = TextManager.Get("AutoBalanceThreshold.tooltip");
			autoBalanceThresholdLabel.ToolTip = autoBalanceThresholdTooltip;
			GUISelectionCarousel<int> autoBalanceThreshold = new GUISelectionCarousel<int>(new RectTransform(new Vector2(0.5f, 0.6f), autoBalanceThresholdLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<int, LocalizedString>>());
			autoBalanceThreshold.AddElement(0, TextManager.Get("AutoBalanceThreshold.Off"), autoBalanceThresholdTooltip);
			autoBalanceThreshold.AddElement(1, "1", autoBalanceThresholdTooltip);
			autoBalanceThreshold.AddElement(2, "2", autoBalanceThresholdTooltip);
			autoBalanceThreshold.AddElement(3, "3", autoBalanceThresholdTooltip);
			this.AssignGUIComponent("PvpAutoBalanceThreshold", autoBalanceThreshold);
			NetLobbyScreen.CreateSubHeader("serversettingsroundstab", listBox.Content, null);
			GUITickBox voiceChatEnabled = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsVoiceChatEnabled"), null, "");
			this.AssignGUIComponent("VoiceChatEnabled", voiceChatEnabled);
			GUITickBox allowSpecBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowSpectating"), null, "");
			this.AssignGUIComponent("AllowSpectating", allowSpecBox);
			GUITickBox allowAfkBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowAFK"), null, "")
			{
				ToolTip = TextManager.Get("ServerSettingsAllowAFK.tooltip")
			};
			this.AssignGUIComponent("AllowAFK", allowAfkBox);
			GUITextBlock losModeLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LosEffect"), null, null, Alignment.Left, false, "", null);
			GUISelectionCarousel<LosMode> losModeSelection = new GUISelectionCarousel<LosMode>(new RectTransform(new Vector2(0.5f, 0.6f), losModeLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<LosMode, LocalizedString>>());
			foreach (LosMode losMode in Enum.GetValues(typeof(LosMode)).Cast<LosMode>())
			{
				GUISelectionCarousel<LosMode> guiselectionCarousel = losModeSelection;
				LosMode value = losMode;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
				defaultInterpolatedStringHandler.AppendLiteral("LosMode");
				defaultInterpolatedStringHandler.AppendFormatted<LosMode>(losMode);
				LocalizedString text = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("LosMode");
				defaultInterpolatedStringHandler2.AppendFormatted<LosMode>(losMode);
				defaultInterpolatedStringHandler2.AppendLiteral(".tooltip");
				guiselectionCarousel.AddElement(value, text, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
			}
			this.AssignGUIComponent("LosMode", losModeSelection);
			GUITextBlock healthBarModeLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowEnemyHealthBars"), null, null, Alignment.Left, false, "", null);
			GUISelectionCarousel<EnemyHealthBarMode> healthBarModeSelection = new GUISelectionCarousel<EnemyHealthBarMode>(new RectTransform(new Vector2(0.5f, 0.6f), healthBarModeLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", Array.Empty<ValueTuple<EnemyHealthBarMode, LocalizedString>>());
			foreach (EnemyHealthBarMode healthBarMode in Enum.GetValues(typeof(EnemyHealthBarMode)).Cast<EnemyHealthBarMode>())
			{
				GUISelectionCarousel<EnemyHealthBarMode> guiselectionCarousel2 = healthBarModeSelection;
				EnemyHealthBarMode value2 = healthBarMode;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("ShowEnemyHealthBars.");
				defaultInterpolatedStringHandler3.AppendFormatted<EnemyHealthBarMode>(healthBarMode);
				guiselectionCarousel2.AddElement(value2, TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()), null);
			}
			this.AssignGUIComponent("ShowEnemyHealthBars", healthBarModeSelection);
			GUITickBox disableBotConversationsBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsDisableBotConversations"), null, "");
			this.AssignGUIComponent("DisableBotConversations", disableBotConversationsBox);
			NetLobbyScreen.CreateSubHeader("serversettingscategory.misc", listBox.Content, null);
			GUITickBox shareSubsBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsShareSubFiles"), null, "");
			this.AssignGUIComponent("AllowFileTransfers", shareSubsBox);
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsSaveLogs"), null, "");
			guitickBox.OnSelected = ((GUITickBox GUITickBox) => true);
			GUITickBox saveLogsBox = guitickBox;
			this.AssignGUIComponent("SaveServerLogs", saveLogsBox);
			CS$<>8__locals1.newCampaignDefaultSalaryLabel = TextManager.Get("ServerSettingsNewCampaignDefaultSalary");
			GUIComponent content4 = listBox.Content;
			string headerTag = "ServerSettingsNewCampaignDefaultSalary";
			string valueLabelTag3 = "ServerSettingsKickVotesRequired";
			string tooltipTag = "ServerSettingsNewCampaignDefaultSalaryToolTip";
			float? step3 = null;
			range = null;
			GUIScrollBar defaultSalarySlider;
			GUITextBlock defaultSalarySliderLabel;
			NetLobbyScreen.CreateLabeledSlider(content4, headerTag, valueLabelTag3, tooltipTag, out defaultSalarySlider, out defaultSalarySliderLabel, step3, range);
			defaultSalarySlider.Range = new Vector2(0f, 100f);
			defaultSalarySlider.StepValue = 1f;
			defaultSalarySlider.OnMoved = delegate(GUIScrollBar scrollBar, float _)
			{
				GUITextBlock text2 = scrollBar.UserData as GUITextBlock;
				if (text2 == null)
				{
					return false;
				}
				text2.Text = TextManager.AddPunctuation(':', new LocalizedString[]
				{
					CS$<>8__locals1.newCampaignDefaultSalaryLabel,
					TextManager.GetWithVariable("percentageformat", "[value]", ((int)Math.Round((double)scrollBar.BarScrollValue, 0)).ToString(), FormatCapitals.No)
				});
				return true;
			};
			this.AssignGUIComponent("NewCampaignDefaultSalary", defaultSalarySlider);
			defaultSalarySlider.OnMoved(defaultSalarySlider, defaultSalarySlider.BarScroll);
			GUINumberInput pvpDisembarkPoints = NetLobbyScreen.CreateLabeledNumberInput(listBox.Content, "serversettingsdisembarkpoints", 0, 100, "serversettingsdisembarkpointstooltip", null);
			this.AssignGUIComponent("DisembarkPointAllowance", pvpDisembarkPoints);
			GUILayoutGroup buttonHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			this.monstersEnabledButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsMonsterSpawns"), Alignment.Center, "GUIButtonSmall", null)
			{
				Enabled = !GameMain.NetworkMember.GameStarted
			};
			this.monstersEnabledPanel = this.CreateMonstersEnabledPanel();
			this.monstersEnabledButton.UserData = "monstersenabled";
			this.monstersEnabledButton.OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<CreateGeneralTab>g__ExtraSettingsButtonClicked|5);
			this.extraCargoButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAdditionalCargo"), Alignment.Center, "GUIButtonSmall", null)
			{
				Enabled = !GameMain.NetworkMember.GameStarted
			};
			this.extraCargoPanel = this.CreateExtraCargoPanel();
			this.extraCargoButton.UserData = "extracargo";
			this.extraCargoButton.OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<CreateGeneralTab>g__ExtraSettingsButtonClicked|5);
			GUITextBlock.AutoScaleAndNormalize(from c in buttonHolder.Children
			select ((GUIButton)c).TextBlock, true, false, null);
		}

		// Token: 0x06004CAD RID: 19629 RVA: 0x002A7164 File Offset: 0x002A5364
		private GUIComponent CreateMonstersEnabledPanel()
		{
			GUIListBox monsterFrame = new GUIListBox(new RectTransform(new Vector2(0.5f, 0.7f), this.settingsTabs[ServerSettings.SettingsTab.General].RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.BottomRight), null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Visible = false,
				IgnoreLayoutGroups = true
			};
			this.InitMonstersEnabled();
			List<Identifier> monsterNames = this.MonsterEnabled.Keys.ToList<Identifier>();
			this.tempMonsterEnabled = new Dictionary<Identifier, bool>(this.MonsterEnabled);
			using (List<Identifier>.Enumerator enumerator = monsterNames.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier s = enumerator.Current;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Character.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(s);
					LocalizedString translatedLabel = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(s.Value, true);
					GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), monsterFrame.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 25)
					}, translatedLabel, null, "");
					guitickBox.Selected = this.tempMonsterEnabled[s];
					guitickBox.OnSelected = delegate(GUITickBox tb)
					{
						this.tempMonsterEnabled[s] = tb.Selected;
						return true;
					};
				}
			}
			monsterFrame.Content.RectTransform.SortChildren(delegate(RectTransform c1, RectTransform c2)
			{
				GUITickBox guitickBox2 = c1.GUIComponent as GUITickBox;
				LocalizedString name = ((guitickBox2 != null) ? guitickBox2.Text : null) ?? string.Empty;
				GUITickBox guitickBox3 = c2.GUIComponent as GUITickBox;
				LocalizedString name2 = ((guitickBox3 != null) ? guitickBox3.Text : null) ?? string.Empty;
				return name.CompareTo(name2);
			});
			if (GameMain.Client == null || !GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				this.SetElementInteractability(monsterFrame.Content, false);
			}
			return monsterFrame;
		}

		// Token: 0x06004CAE RID: 19630 RVA: 0x002A7378 File Offset: 0x002A5578
		private GUIComponent CreateExtraCargoPanel()
		{
			GUIFrame cargoFrame = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.7f), this.settingsTabs[ServerSettings.SettingsTab.General].RectTransform, Anchor.BottomRight, new Pivot?(Pivot.BottomLeft), null, null, ScaleBasis.Normal), "", null)
			{
				Visible = false,
				IgnoreLayoutGroups = true
			};
			GUILayoutGroup cargoContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), cargoFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), cargoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("serverlog.filter");
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock filterText = new GUITextBlock(rectT, text4, null, font, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(0.5f, 1f), filterText.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.Font;
			GUITextBox entityFilterBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, true, true);
			filterText.RectTransform.MinSize = new Point(0, entityFilterBox.RectTransform.MinSize.Y);
			GUIListBox cargoList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), cargoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			entityFilterBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				foreach (GUIComponent child in cargoList.Content.Children)
				{
					ItemPrefab itemPrefab = child.UserData as ItemPrefab;
					if (itemPrefab != null)
					{
						child.Visible = (string.IsNullOrEmpty(text) || itemPrefab.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
					}
				}
				return true;
			};
			using (IEnumerator<ItemPrefab> enumerator = (from ip in ItemPrefab.Prefabs
			orderby ip.Name
			select ip).GetEnumerator())
			{
				Action <>9__3;
				while (enumerator.MoveNext())
				{
					ItemPrefab ip = enumerator.Current;
					if (ip.AllowAsExtraCargo != null)
					{
						if (!ip.AllowAsExtraCargo.Value)
						{
							continue;
						}
					}
					else if (!ip.CanBeBought)
					{
						continue;
					}
					GUILayoutGroup itemFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), cargoList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 30)
					}, true, Anchor.TopLeft)
					{
						Stretch = true,
						UserData = ip,
						RelativeSpacing = 0.05f
					};
					if (ip.InventoryIcon != null || ip.Sprite != null)
					{
						GUIImage img = new GUIImage(new RectTransform(new Point(itemFrame.Rect.Height), itemFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), ip.InventoryIcon ?? ip.Sprite, true, null)
						{
							CanBeFocused = false
						};
						img.Color = ((img.Sprite == ip.InventoryIcon) ? ip.InventoryIconColor : ip.SpriteColor);
					}
					RectTransform rectT3 = new RectTransform(new Vector2(0.75f, 1f), itemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = ip.Name;
					font = GUIStyle.SmallFont;
					GUITextBlock guitextBlock = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
					guitextBlock.Wrap = true;
					guitextBlock.CanBeFocused = false;
					int cargoVal;
					this.ExtraCargo.TryGetValue(ip, out cargoVal);
					GUINumberInput amountInput = new GUINumberInput(new RectTransform(new Vector2(0.35f, 1f), itemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.CenterLeft, null, GUINumberInput.ButtonVisibility.Automatic, null)
					{
						MinValueInt = new int?(0),
						MaxValueInt = new int?(10),
						IntValue = cargoVal
					};
					GUINumberInput guinumberInput = amountInput;
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numberInput)
					{
						if (this.ExtraCargo.ContainsKey(ip))
						{
							this.ExtraCargo[ip] = numberInput.IntValue;
							if (numberInput.IntValue <= 0)
							{
								this.ExtraCargo.Remove(ip);
							}
						}
						else if (this.ExtraCargo.Keys.Count < 20)
						{
							this.ExtraCargo.Add(ip, numberInput.IntValue);
						}
						numberInput.IntValue = (this.ExtraCargo.ContainsKey(ip) ? this.ExtraCargo[ip] : 0);
						Action action;
						if ((action = <>9__3) == null)
						{
							action = (<>9__3 = delegate()
							{
								foreach (GUIComponent child in cargoList.Content.GetAllChildren())
								{
									GUINumberInput otherNumberInput = child.GetChild<GUINumberInput>();
									if (otherNumberInput != null)
									{
										GUIComponent plusButton = otherNumberInput.PlusButton;
										bool enabled;
										if (this.ExtraCargo.Keys.Count < 20)
										{
											int intValue = otherNumberInput.IntValue;
											int? maxValueInt = otherNumberInput.MaxValueInt;
											enabled = (intValue < maxValueInt.GetValueOrDefault() & maxValueInt != null);
										}
										else
										{
											enabled = false;
										}
										plusButton.Enabled = enabled;
									}
								}
							});
						}
						CoroutineManager.Invoke(action, 0f);
					}));
				}
			}
			if (GameMain.Client == null || !GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.ManageSettings))
			{
				this.SetElementInteractability(cargoList.Content, false);
			}
			return cargoFrame;
		}

		// Token: 0x06004CAF RID: 19631 RVA: 0x002A7910 File Offset: 0x002A5B10
		private void CreateAntigriefingTab(GUIComponent parent)
		{
			ServerSettings.<>c__DisplayClass32_0 CS$<>8__locals1 = new ServerSettings.<>c__DisplayClass32_0();
			CS$<>8__locals1.<>4__this = this;
			GUIListBox listBox = new GUIListBox(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "GUIListBoxNoBorder", true, false)
			{
				AutoHideScrollBar = true,
				CurrentSelectMode = GUIListBox.SelectMode.None
			};
			GUIListBox tickBoxContainer = new GUIListBox(new RectTransform(new Vector2(1f, 0.268f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = true,
				UseGridLayout = true
			};
			tickBoxContainer.Padding *= 2f;
			GUITickBox allowFriendlyFire = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowFriendlyFire"), null, "");
			this.AssignGUIComponent("AllowFriendlyFire", allowFriendlyFire);
			GUITickBox allowDragAndDropGive = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowDragAndDropGive"), null, "");
			this.AssignGUIComponent("AllowDragAndDropGive", allowDragAndDropGive);
			GUITickBox killableNPCs = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsKillableNPCs"), null, "");
			this.AssignGUIComponent("KillableNPCs", killableNPCs);
			GUITickBox destructibleOutposts = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsDestructibleOutposts"), null, "");
			this.AssignGUIComponent("DestructibleOutposts", destructibleOutposts);
			GUITickBox lockAllDefaultWires = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsLockAllDefaultWires"), null, "");
			this.AssignGUIComponent("LockAllDefaultWires", lockAllDefaultWires);
			GUITickBox allowRewiring = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowRewiring"), null, "");
			this.AssignGUIComponent("AllowRewiring", allowRewiring);
			GUITickBox allowWifiChatter = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowWifiChat"), null, "");
			this.AssignGUIComponent("AllowLinkingWifiToChat", allowWifiChatter);
			GUITickBox allowDisguises = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowDisguises"), null, "");
			this.AssignGUIComponent("AllowDisguises", allowDisguises);
			GUITickBox allowImmediateItemDeliveryBox = new GUITickBox(new RectTransform(new Vector2(0.48f, 0.05f), tickBoxContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsImmediateItemDelivery"), null, "");
			this.AssignGUIComponent("AllowImmediateItemDelivery", allowImmediateItemDeliveryBox);
			GUITextBlock.AutoScaleAndNormalize(from c in tickBoxContainer.Content.Children
			select ((GUITickBox)c).TextBlock, true, false, null);
			tickBoxContainer.RectTransform.MinSize = new Point(0, (int)((float)tickBoxContainer.Content.Children.First<GUIComponent>().Rect.Height * 2f + tickBoxContainer.Padding.Y + tickBoxContainer.Padding.W));
			tickBoxContainer.RectTransform.MinSize = new Point(0, (int)((float)tickBoxContainer.Content.Children.First<GUIComponent>().Rect.Height * 2f + tickBoxContainer.Padding.Y + tickBoxContainer.Padding.W));
			GUITickBox voteKickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsAllowVoteKick"), null, "");
			this.AssignGUIComponent("AllowVoteKick", voteKickBox);
			GUIScrollBar slider;
			GUITextBlock sliderLabel;
			NetLobbyScreen.CreateLabeledSlider(listBox.Content, string.Empty, "ServerSettingsKickVotesRequired", string.Empty, out slider, out sliderLabel, null, null);
			ServerSettings.<>c__DisplayClass32_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString text = sliderLabel.Text;
			CS$<>8__locals2.votesRequiredLabel = ((text != null) ? text.ToString() : null) + " ";
			slider.Step = 0.2f;
			slider.Range = new Vector2(0.5f, 1f);
			slider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				((GUITextBlock)scrollBar.UserData).Text = CS$<>8__locals1.votesRequiredLabel + (int)MathUtils.Round(scrollBar.BarScrollValue * 100f, 10f) + " %";
				return true;
			};
			this.AssignGUIComponent("KickVoteRequiredRatio", slider);
			slider.OnMoved(slider, slider.BarScroll);
			NetLobbyScreen.CreateLabeledSlider(listBox.Content, string.Empty, "ServerSettingsAutobanTime", "ServerSettingsAutobanTime.Tooltip", out slider, out sliderLabel, null, null);
			ServerSettings.<>c__DisplayClass32_0 CS$<>8__locals3 = CS$<>8__locals1;
			RichString text2 = sliderLabel.Text;
			CS$<>8__locals3.autobanLabel = ((text2 != null) ? text2.ToString() : null) + " ";
			slider.Range = new Vector2(0f, this.MaxAutoBanTime);
			slider.StepValue = 900f;
			slider.OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
			{
				((GUITextBlock)scrollBar.UserData).Text = CS$<>8__locals1.autobanLabel + ToolBox.SecondsToReadableTime(scrollBar.BarScrollValue);
				return true;
			};
			this.AssignGUIComponent("AutoBanTime", slider);
			slider.OnMoved(slider, slider.BarScroll);
			GUINumberInput maximumTransferAmount = NetLobbyScreen.CreateLabeledNumberInput(listBox.Content, "serversettingsmaximumtransferrequest", 0, 1073741823, "serversettingsmaximumtransferrequesttooltip", null);
			this.AssignGUIComponent("MaximumMoneyTransferRequest", maximumTransferAmount);
			GUIDropDown lootedMoneyDestination = NetLobbyScreen.CreateLabeledDropdown(listBox.Content, "serversettingslootedmoneydestination", 2, "serversettingslootedmoneydestinationtooltip");
			lootedMoneyDestination.AddItem(TextManager.Get("lootedmoneydestination.bank"), LootedMoneyDestination.Bank, null, null, null);
			lootedMoneyDestination.AddItem(TextManager.Get("lootedmoneydestination.wallet"), LootedMoneyDestination.Wallet, null, null, null);
			this.AssignGUIComponent("LootedMoneyDestination", lootedMoneyDestination);
			GUITickBox enableDosProtection = new GUITickBox(new RectTransform(new Vector2(0.5f, 0f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsEnableDoSProtection"), null, "")
			{
				ToolTip = TextManager.Get("ServerSettingsEnableDoSProtectionTooltip")
			};
			this.AssignGUIComponent("EnableDoSProtection", enableDosProtection);
			GUIScrollBar maxPacketSlider;
			GUITextBlock maxPacketSliderLabel;
			NetLobbyScreen.CreateLabeledSlider(listBox.Content, string.Empty, "ServerSettingsMaxPacketAmount", string.Empty, out maxPacketSlider, out maxPacketSliderLabel, null, null);
			CS$<>8__locals1.maxPacketCountLabel = maxPacketSliderLabel.Text;
			maxPacketSlider.Step = 0.001f;
			maxPacketSlider.Range = new Vector2(1200f, 10000f);
			maxPacketSlider.ToolTip = ServerSettings.packetAmountTooltip;
			maxPacketSlider.OnMoved = delegate(GUIScrollBar scrollBar, float _)
			{
				GUITextBlock textBlock = (GUITextBlock)scrollBar.UserData;
				int value = (int)MathF.Floor(scrollBar.BarScrollValue);
				LocalizedString valueText = (value > 1200) ? value.ToString() : TextManager.Get("ServerSettingsNoLimit");
				if (value > 1200)
				{
					if (value >= 3500)
					{
						textBlock.TextColor = GUIStyle.TextColorNormal;
						scrollBar.ToolTip = ServerSettings.packetAmountTooltip;
					}
					else
					{
						textBlock.TextColor = GUIStyle.Red;
						scrollBar.ToolTip = ServerSettings.packetAmountTooltipWarning;
					}
				}
				else
				{
					textBlock.TextColor = GUIStyle.Green;
					scrollBar.ToolTip = ServerSettings.packetAmountTooltip;
				}
				GUITextBlock guitextBlock = textBlock;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(CS$<>8__locals1.maxPacketCountLabel);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(valueText);
				guitextBlock.Text = defaultInterpolatedStringHandler.ToStringAndClear();
				return true;
			};
			this.AssignGUIComponent("MaxPacketAmount", maxPacketSlider);
			maxPacketSlider.OnMoved(maxPacketSlider, maxPacketSlider.BarScroll);
			NetLobbyScreen.CreateSubHeader("Karma", listBox.Content, "KarmaExplanation");
			CS$<>8__locals1.karmaBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 1f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ServerSettingsUseKarma"), null, "")
			{
				ToolTip = TextManager.Get("KarmaExplanation")
			};
			this.AssignGUIComponent("KarmaEnabled", CS$<>8__locals1.karmaBox);
			this.karmaPresetDD = new GUIDropDown(new RectTransform(new Vector2(1f, 0.05f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			foreach (string karmaPreset in GameMain.NetworkMember.KarmaManager.Presets.Keys)
			{
				this.karmaPresetDD.AddItem(TextManager.Get("KarmaPreset." + karmaPreset), karmaPreset, null, null, null);
			}
			this.karmaElements.Add(this.karmaPresetDD);
			GUIFrame karmaSettingsContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.5f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.karmaElements.Add(karmaSettingsContainer);
			this.karmaSettingsList = new GUIListBox(new RectTransform(Vector2.One, karmaSettingsContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				Spacing = (int)(8f * GUI.Scale)
			};
			this.karmaSettingsList.Padding *= 2f;
			this.karmaPresetDD.SelectItem(this.KarmaPreset);
			this.SetElementInteractability(this.karmaSettingsList.Content, !CS$<>8__locals1.karmaBox.Selected || this.KarmaPreset != "custom");
			GameMain.NetworkMember.KarmaManager.CreateSettingsFrame(this.karmaSettingsList.Content);
			this.karmaPresetDD.OnSelected = delegate(GUIComponent selected, object obj)
			{
				string newKarmaPreset = obj as string;
				if (newKarmaPreset == CS$<>8__locals1.<>4__this.KarmaPreset)
				{
					return true;
				}
				List<ServerSettings.NetPropertyData> properties = CS$<>8__locals1.<>4__this.netProperties.Values.ToList<ServerSettings.NetPropertyData>();
				List<object> prevValues = new List<object>();
				foreach (ServerSettings.NetPropertyData prop in CS$<>8__locals1.<>4__this.netProperties.Values)
				{
					prevValues.Add(prop.TempValue);
					if (prop.GUIComponent != null)
					{
						prop.Value = prop.GUIComponentValue;
					}
				}
				if (CS$<>8__locals1.<>4__this.KarmaPreset == "custom")
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null)
					{
						KarmaManager karmaManager = networkMember.KarmaManager;
						if (karmaManager != null)
						{
							karmaManager.SaveCustomPreset();
						}
					}
					NetworkMember networkMember2 = GameMain.NetworkMember;
					if (networkMember2 != null)
					{
						KarmaManager karmaManager2 = networkMember2.KarmaManager;
						if (karmaManager2 != null)
						{
							karmaManager2.Save();
						}
					}
				}
				CS$<>8__locals1.<>4__this.KarmaPreset = newKarmaPreset;
				GameMain.NetworkMember.KarmaManager.SelectPreset(CS$<>8__locals1.<>4__this.KarmaPreset);
				CS$<>8__locals1.<>4__this.karmaSettingsList.Content.ClearChildren();
				GameMain.NetworkMember.KarmaManager.CreateSettingsFrame(CS$<>8__locals1.<>4__this.karmaSettingsList.Content);
				CS$<>8__locals1.<>4__this.SetElementInteractability(CS$<>8__locals1.<>4__this.karmaSettingsList.Content, !CS$<>8__locals1.karmaBox.Selected || CS$<>8__locals1.<>4__this.KarmaPreset != "custom");
				for (int i = 0; i < CS$<>8__locals1.<>4__this.netProperties.Count; i++)
				{
					properties[i].TempValue = prevValues[i];
				}
				return true;
			};
			this.AssignGUIComponent("KarmaPreset", this.karmaPresetDD);
			CS$<>8__locals1.karmaBox.OnSelected = delegate(GUITickBox tb)
			{
				CS$<>8__locals1.<>4__this.SetElementInteractability(CS$<>8__locals1.<>4__this.karmaSettingsList.Content, !CS$<>8__locals1.karmaBox.Selected || CS$<>8__locals1.<>4__this.KarmaPreset != "custom");
				CS$<>8__locals1.<>4__this.karmaElements.ForEach(delegate(GUIComponent e)
				{
					e.Visible = tb.Selected;
				});
				return true;
			};
			this.karmaElements.ForEach(delegate(GUIComponent e)
			{
				e.Visible = CS$<>8__locals1.<>4__this.KarmaEnabled;
			});
			listBox.Content.InheritTotalChildrenMinHeight();
		}

		// Token: 0x06004CB0 RID: 19632 RVA: 0x002A8500 File Offset: 0x002A6700
		private void CreateBanlistTab(GUIComponent parent)
		{
			this.BanList.CreateBanFrame(parent);
		}

		// Token: 0x06004CB1 RID: 19633 RVA: 0x002A8510 File Offset: 0x002A6710
		private bool SelectSettingsTab(GUIButton button, object obj)
		{
			this.selectedTab = (ServerSettings.SettingsTab)obj;
			foreach (ServerSettings.SettingsTab key in this.settingsTabs.Keys)
			{
				this.settingsTabs[key].Visible = (key == this.selectedTab);
				this.tabButtons[key].Selected = (key == this.selectedTab);
			}
			return true;
		}

		// Token: 0x06004CB2 RID: 19634 RVA: 0x002A85A4 File Offset: 0x002A67A4
		public void Close()
		{
			if (this.KarmaPreset == "custom")
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					KarmaManager karmaManager = networkMember.KarmaManager;
					if (karmaManager != null)
					{
						karmaManager.SaveCustomPreset();
					}
				}
				NetworkMember networkMember2 = GameMain.NetworkMember;
				if (networkMember2 != null)
				{
					KarmaManager karmaManager2 = networkMember2.KarmaManager;
					if (karmaManager2 != null)
					{
						karmaManager2.Save();
					}
				}
			}
			this.ClientAdminWrite(ServerSettings.NetFlags.Properties, default(Identifier), default(Identifier), 0);
			foreach (ServerSettings.NetPropertyData prop in this.netProperties.Values)
			{
				prop.GUIComponent = null;
			}
			this.settingsFrame = null;
			GameMain.NetLobbyScreen.AssignComponentsToServerSettings();
		}

		// Token: 0x06004CB3 RID: 19635 RVA: 0x002A8670 File Offset: 0x002A6870
		public bool ToggleSettingsFrame(GUIButton button, object obj)
		{
			if (GameMain.NetworkMember == null)
			{
				return false;
			}
			if (this.settingsFrame == null)
			{
				this.CreateSettingsFrame();
			}
			else
			{
				this.Close();
			}
			return false;
		}

		// Token: 0x17001380 RID: 4992
		// (get) Token: 0x06004CB4 RID: 19636 RVA: 0x002A8692 File Offset: 0x002A6892
		public string Name
		{
			get
			{
				return "ServerSettings";
			}
		}

		// Token: 0x17001381 RID: 4993
		// (get) Token: 0x06004CB5 RID: 19637 RVA: 0x002A8699 File Offset: 0x002A6899
		// (set) Token: 0x06004CB6 RID: 19638 RVA: 0x002A86A1 File Offset: 0x002A68A1
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06004CB7 RID: 19639 RVA: 0x002A86AC File Offset: 0x002A68AC
		private void InitProjSpecific()
		{
			IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(base.GetType()).Cast<PropertyDescriptor>();
			this.SerializableProperties = new Dictionary<Identifier, SerializableProperty>();
			foreach (PropertyDescriptor property in properties)
			{
				SerializableProperty objProperty = new SerializableProperty(property);
				this.SerializableProperties.Add(property.Name.ToIdentifier(), objProperty);
			}
		}

		// Token: 0x06004CB8 RID: 19640 RVA: 0x002A8728 File Offset: 0x002A6928
		public ServerSettings(NetworkMember networkMember, string serverName, int port, int queryPort, int maxPlayers, bool isPublic, bool enableUPnP, IPAddress listenIp)
		{
			this.ServerLog = new ServerLog(serverName);
			this.BanList = new BanList();
			this.ExtraCargo = new Dictionary<ItemPrefab, int>();
			this.HiddenSubs = new HashSet<string>();
			PermissionPreset.List.Clear();
			PermissionPreset.LoadAll(ServerSettings.PermissionPresetFile);
			PermissionPreset.LoadAll(ServerSettings.PermissionPresetFileCustom);
			this.InitProjSpecific();
			this.ServerName = serverName;
			this.ListenIPAddress = listenIp;
			this.Port = port;
			this.QueryPort = queryPort;
			this.EnableUPnP = enableUPnP;
			this.MaxPlayers = maxPlayers;
			this.IsPublic = isPublic;
			this.netProperties = new Dictionary<uint, ServerSettings.NetPropertyData>();
			using (MD5 md5 = MD5.Create())
			{
				List<SerializableProperty> saveProperties = SerializableProperty.GetProperties<Serialize>(this);
				foreach (SerializableProperty property in saveProperties)
				{
					string typeName = SerializableProperty.GetSupportedTypeName(property.PropertyType);
					if ((typeName != null || property.PropertyType.IsEnum) && property.GetAttribute<DoNotSyncOverNetwork>() == null)
					{
						ServerSettings.NetPropertyData netPropertyData = new ServerSettings.NetPropertyData(this, property, typeName);
						uint key = ToolBoxCore.IdentifierToUint32Hash(netPropertyData.Name, md5);
						if (key == 0U)
						{
							key += 1U;
						}
						if (this.netProperties.ContainsKey(key))
						{
							string[] array = new string[7];
							array[0] = "Hashing collision in ServerSettings.netProperties: ";
							int num = 1;
							ServerSettings.NetPropertyData netPropertyData3 = this.netProperties[key];
							array[num] = ((netPropertyData3 != null) ? netPropertyData3.ToString() : null);
							array[2] = " has same key as ";
							array[3] = property.Name;
							array[4] = " (";
							array[5] = key.ToString();
							array[6] = ")";
							throw new Exception(string.Concat(array));
						}
						this.netProperties.Add(key, netPropertyData);
					}
				}
				List<SerializableProperty> karmaProperties = SerializableProperty.GetProperties<Serialize>(networkMember.KarmaManager);
				foreach (SerializableProperty property2 in karmaProperties)
				{
					object value = property2.GetValue(networkMember.KarmaManager);
					if (value != null)
					{
						string typeName2 = SerializableProperty.GetSupportedTypeName(value.GetType());
						if (typeName2 != null || property2.PropertyType.IsEnum)
						{
							ServerSettings.NetPropertyData netPropertyData2 = new ServerSettings.NetPropertyData(networkMember.KarmaManager, property2, typeName2);
							uint key2 = ToolBoxCore.IdentifierToUint32Hash(netPropertyData2.Name, md5);
							if (this.netProperties.ContainsKey(key2))
							{
								string[] array2 = new string[7];
								array2[0] = "Hashing collision in ServerSettings.netProperties: ";
								int num2 = 1;
								ServerSettings.NetPropertyData netPropertyData4 = this.netProperties[key2];
								array2[num2] = ((netPropertyData4 != null) ? netPropertyData4.ToString() : null);
								array2[2] = " has same key as ";
								array2[3] = property2.Name;
								array2[4] = " (";
								array2[5] = key2.ToString();
								array2[6] = ")";
								throw new Exception(string.Concat(array2));
							}
							this.netProperties.Add(key2, netPropertyData2);
						}
					}
				}
			}
		}

		// Token: 0x17001382 RID: 4994
		// (get) Token: 0x06004CB9 RID: 19641 RVA: 0x002A8B20 File Offset: 0x002A6D20
		// (set) Token: 0x06004CBA RID: 19642 RVA: 0x002A8B28 File Offset: 0x002A6D28
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerName
		{
			get
			{
				return this.serverName;
			}
			set
			{
				string newName = value;
				if (newName.Length > NetConfig.ServerNameMaxLength)
				{
					newName = newName.Substring(0, NetConfig.ServerNameMaxLength);
				}
				if (this.serverName == newName)
				{
					return;
				}
				if (newName.IsNullOrWhiteSpace())
				{
					return;
				}
				this.serverName = newName;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17001383 RID: 4995
		// (get) Token: 0x06004CBB RID: 19643 RVA: 0x002A8B77 File Offset: 0x002A6D77
		// (set) Token: 0x06004CBC RID: 19644 RVA: 0x002A8B80 File Offset: 0x002A6D80
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string ServerMessageText
		{
			get
			{
				return this.serverMessageText;
			}
			set
			{
				string val = value;
				if (val.Length > NetConfig.ServerMessageMaxLength)
				{
					val = val.Substring(0, NetConfig.ServerMessageMaxLength);
				}
				if (this.serverMessageText == val)
				{
					return;
				}
				GUIButton serverMessageButton = GameMain.NetLobbyScreen.ServerMessageButton;
				if (serverMessageButton != null)
				{
					serverMessageButton.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Green), 1.5f, false, false, null);
					serverMessageButton.Pulsate(Vector2.One, Vector2.One * 1.2f, 1f);
				}
				this.serverMessageText = val;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x17001384 RID: 4996
		// (get) Token: 0x06004CBD RID: 19645 RVA: 0x002A8C18 File Offset: 0x002A6E18
		// (set) Token: 0x06004CBE RID: 19646 RVA: 0x002A8C20 File Offset: 0x002A6E20
		public Dictionary<Identifier, bool> MonsterEnabled { get; private set; }

		// Token: 0x17001385 RID: 4997
		// (get) Token: 0x06004CBF RID: 19647 RVA: 0x002A8C29 File Offset: 0x002A6E29
		// (set) Token: 0x06004CC0 RID: 19648 RVA: 0x002A8C31 File Offset: 0x002A6E31
		public Dictionary<ItemPrefab, int> ExtraCargo { get; private set; }

		// Token: 0x17001386 RID: 4998
		// (get) Token: 0x06004CC1 RID: 19649 RVA: 0x002A8C3A File Offset: 0x002A6E3A
		// (set) Token: 0x06004CC2 RID: 19650 RVA: 0x002A8C42 File Offset: 0x002A6E42
		public HashSet<string> HiddenSubs { get; set; }

		// Token: 0x17001387 RID: 4999
		// (get) Token: 0x06004CC3 RID: 19651 RVA: 0x002A8C4B File Offset: 0x002A6E4B
		// (set) Token: 0x06004CC4 RID: 19652 RVA: 0x002A8C53 File Offset: 0x002A6E53
		public List<ServerSettings.SavedClientPermission> ClientPermissions { get; private set; } = new List<ServerSettings.SavedClientPermission>();

		// Token: 0x17001388 RID: 5000
		// (get) Token: 0x06004CC5 RID: 19653 RVA: 0x002A8C5C File Offset: 0x002A6E5C
		// (set) Token: 0x06004CC6 RID: 19654 RVA: 0x002A8C64 File Offset: 0x002A6E64
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool IsPublic { get; set; }

		// Token: 0x17001389 RID: 5001
		// (get) Token: 0x06004CC7 RID: 19655 RVA: 0x002A8C6D File Offset: 0x002A6E6D
		// (set) Token: 0x06004CC8 RID: 19656 RVA: 0x002A8C75 File Offset: 0x002A6E75
		[Serialize(20, IsPropertySaveable.Yes, "", "", false)]
		public int TickRate
		{
			get
			{
				return this.tickRate;
			}
			set
			{
				this.tickRate = MathHelper.Clamp(value, 1, 60);
			}
		}

		// Token: 0x1700138A RID: 5002
		// (get) Token: 0x06004CC9 RID: 19657 RVA: 0x002A8C86 File Offset: 0x002A6E86
		// (set) Token: 0x06004CCA RID: 19658 RVA: 0x002A8C8E File Offset: 0x002A6E8E
		[Serialize(150, IsPropertySaveable.Yes, "Maximum amount of lag compensation for firing weapons, in milliseconds. E.g. when a client fires a gun, the server will be notified about it with some latency, and checks if it hit anything in the past (at the time the shot was taken), up to this limit. The largest allowed lag compensation is 500 milliseconds.", "", false)]
		public int MaxLagCompensation
		{
			get
			{
				return this.maxLagCompensation;
			}
			set
			{
				this.maxLagCompensation = MathHelper.Clamp(value, 0, 500);
			}
		}

		// Token: 0x1700138B RID: 5003
		// (get) Token: 0x06004CCB RID: 19659 RVA: 0x002A8CA2 File Offset: 0x002A6EA2
		public float MaxLagCompensationSeconds
		{
			get
			{
				return (float)this.maxLagCompensation / 1000f;
			}
		}

		// Token: 0x1700138C RID: 5004
		// (get) Token: 0x06004CCC RID: 19660 RVA: 0x002A8CB1 File Offset: 0x002A6EB1
		// (set) Token: 0x06004CCD RID: 19661 RVA: 0x002A8CB9 File Offset: 0x002A6EB9
		[Serialize(true, IsPropertySaveable.Yes, "Do clients need to be authenticated (e.g. based on Steam ID or an EGS ownership token). Can be disabled if you for example want to play the game in a local network without a connection to external services.", "", false)]
		public bool RequireAuthentication { get; set; }

		// Token: 0x1700138D RID: 5005
		// (get) Token: 0x06004CCE RID: 19662 RVA: 0x002A8CC2 File Offset: 0x002A6EC2
		// (set) Token: 0x06004CCF RID: 19663 RVA: 0x002A8CCA File Offset: 0x002A6ECA
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool RandomizeSeed { get; set; }

		// Token: 0x1700138E RID: 5006
		// (get) Token: 0x06004CD0 RID: 19664 RVA: 0x002A8CD3 File Offset: 0x002A6ED3
		// (set) Token: 0x06004CD1 RID: 19665 RVA: 0x002A8CDB File Offset: 0x002A6EDB
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool UseRespawnShuttle { get; private set; }

		// Token: 0x1700138F RID: 5007
		// (get) Token: 0x06004CD2 RID: 19666 RVA: 0x002A8CE4 File Offset: 0x002A6EE4
		// (set) Token: 0x06004CD3 RID: 19667 RVA: 0x002A8CEC File Offset: 0x002A6EEC
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float RespawnInterval { get; private set; }

		// Token: 0x17001390 RID: 5008
		// (get) Token: 0x06004CD4 RID: 19668 RVA: 0x002A8CF5 File Offset: 0x002A6EF5
		// (set) Token: 0x06004CD5 RID: 19669 RVA: 0x002A8CFD File Offset: 0x002A6EFD
		[Serialize(180f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxTransportTime { get; private set; }

		// Token: 0x17001391 RID: 5009
		// (get) Token: 0x06004CD6 RID: 19670 RVA: 0x002A8D06 File Offset: 0x002A6F06
		// (set) Token: 0x06004CD7 RID: 19671 RVA: 0x002A8D0E File Offset: 0x002A6F0E
		[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
		public float MinRespawnRatio { get; private set; }

		// Token: 0x17001392 RID: 5010
		// (get) Token: 0x06004CD8 RID: 19672 RVA: 0x002A8D17 File Offset: 0x002A6F17
		// (set) Token: 0x06004CD9 RID: 19673 RVA: 0x002A8D1F File Offset: 0x002A6F1F
		[Serialize(20f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillLossPercentageOnDeath { get; private set; }

		// Token: 0x17001393 RID: 5011
		// (get) Token: 0x06004CDA RID: 19674 RVA: 0x002A8D28 File Offset: 0x002A6F28
		// (set) Token: 0x06004CDB RID: 19675 RVA: 0x002A8D30 File Offset: 0x002A6F30
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float SkillLossPercentageOnImmediateRespawn { get; private set; }

		// Token: 0x17001394 RID: 5012
		// (get) Token: 0x06004CDC RID: 19676 RVA: 0x002A8D39 File Offset: 0x002A6F39
		// (set) Token: 0x06004CDD RID: 19677 RVA: 0x002A8D41 File Offset: 0x002A6F41
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		public float ReplaceCostPercentage { get; private set; }

		// Token: 0x17001395 RID: 5013
		// (get) Token: 0x06004CDE RID: 19678 RVA: 0x002A8D4A File Offset: 0x002A6F4A
		// (set) Token: 0x06004CDF RID: 19679 RVA: 0x002A8D52 File Offset: 0x002A6F52
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowBotTakeoverOnPermadeath { get; private set; }

		// Token: 0x17001396 RID: 5014
		// (get) Token: 0x06004CE0 RID: 19680 RVA: 0x002A8D5B File Offset: 0x002A6F5B
		// (set) Token: 0x06004CE1 RID: 19681 RVA: 0x002A8D63 File Offset: 0x002A6F63
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool IronmanMode { get; private set; }

		// Token: 0x17001397 RID: 5015
		// (get) Token: 0x06004CE2 RID: 19682 RVA: 0x002A8D6C File Offset: 0x002A6F6C
		public bool IronmanModeActive
		{
			get
			{
				return this.IronmanMode && this.respawnMode == RespawnMode.Permadeath;
			}
		}

		// Token: 0x17001398 RID: 5016
		// (get) Token: 0x06004CE3 RID: 19683 RVA: 0x002A8D81 File Offset: 0x002A6F81
		// (set) Token: 0x06004CE4 RID: 19684 RVA: 0x002A8D89 File Offset: 0x002A6F89
		[Serialize(60f, IsPropertySaveable.Yes, "", "", false)]
		public float AutoRestartInterval { get; set; }

		// Token: 0x17001399 RID: 5017
		// (get) Token: 0x06004CE5 RID: 19685 RVA: 0x002A8D92 File Offset: 0x002A6F92
		// (set) Token: 0x06004CE6 RID: 19686 RVA: 0x002A8D9A File Offset: 0x002A6F9A
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool StartWhenClientsReady { get; set; }

		// Token: 0x1700139A RID: 5018
		// (get) Token: 0x06004CE7 RID: 19687 RVA: 0x002A8DA3 File Offset: 0x002A6FA3
		// (set) Token: 0x06004CE8 RID: 19688 RVA: 0x002A8DAB File Offset: 0x002A6FAB
		[Serialize(PvpTeamSelectionMode.PlayerPreference, IsPropertySaveable.Yes, "", "", false)]
		public PvpTeamSelectionMode PvpTeamSelectionMode { get; private set; }

		// Token: 0x1700139B RID: 5019
		// (get) Token: 0x06004CE9 RID: 19689 RVA: 0x002A8DB4 File Offset: 0x002A6FB4
		// (set) Token: 0x06004CEA RID: 19690 RVA: 0x002A8DBC File Offset: 0x002A6FBC
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int PvpAutoBalanceThreshold { get; private set; }

		// Token: 0x1700139C RID: 5020
		// (get) Token: 0x06004CEB RID: 19691 RVA: 0x002A8DC5 File Offset: 0x002A6FC5
		// (set) Token: 0x06004CEC RID: 19692 RVA: 0x002A8DCD File Offset: 0x002A6FCD
		[Serialize(0.8f, IsPropertySaveable.Yes, "", "", false)]
		public float StartWhenClientsReadyRatio { get; private set; }

		// Token: 0x1700139D RID: 5021
		// (get) Token: 0x06004CED RID: 19693 RVA: 0x002A8DD6 File Offset: 0x002A6FD6
		// (set) Token: 0x06004CEE RID: 19694 RVA: 0x002A8DDE File Offset: 0x002A6FDE
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float PvPStunResist { get; private set; }

		// Token: 0x1700139E RID: 5022
		// (get) Token: 0x06004CEF RID: 19695 RVA: 0x002A8DE7 File Offset: 0x002A6FE7
		// (set) Token: 0x06004CF0 RID: 19696 RVA: 0x002A8DEF File Offset: 0x002A6FEF
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PvPSpawnMonsters { get; private set; }

		// Token: 0x1700139F RID: 5023
		// (get) Token: 0x06004CF1 RID: 19697 RVA: 0x002A8DF8 File Offset: 0x002A6FF8
		// (set) Token: 0x06004CF2 RID: 19698 RVA: 0x002A8E00 File Offset: 0x002A7000
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool PvPSpawnWrecks { get; private set; }

		// Token: 0x170013A0 RID: 5024
		// (get) Token: 0x06004CF3 RID: 19699 RVA: 0x002A8E09 File Offset: 0x002A7009
		// (set) Token: 0x06004CF4 RID: 19700 RVA: 0x002A8E11 File Offset: 0x002A7011
		[Serialize("Random", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Biome { get; private set; }

		// Token: 0x170013A1 RID: 5025
		// (get) Token: 0x06004CF5 RID: 19701 RVA: 0x002A8E1A File Offset: 0x002A701A
		// (set) Token: 0x06004CF6 RID: 19702 RVA: 0x002A8E22 File Offset: 0x002A7022
		[Serialize("Random", IsPropertySaveable.Yes, "", "", false)]
		public Identifier SelectedOutpostName { get; private set; }

		// Token: 0x170013A2 RID: 5026
		// (get) Token: 0x06004CF7 RID: 19703 RVA: 0x002A8E2B File Offset: 0x002A702B
		// (set) Token: 0x06004CF8 RID: 19704 RVA: 0x002A8E33 File Offset: 0x002A7033
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowSpectating
		{
			get
			{
				return this.allowSpectating;
			}
			private set
			{
				if (this.allowSpectating == value)
				{
					return;
				}
				this.allowSpectating = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013A3 RID: 5027
		// (get) Token: 0x06004CF9 RID: 19705 RVA: 0x002A8E4D File Offset: 0x002A704D
		// (set) Token: 0x06004CFA RID: 19706 RVA: 0x002A8E55 File Offset: 0x002A7055
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowAFK
		{
			get
			{
				return this.allowAFK;
			}
			private set
			{
				if (this.allowAFK == value)
				{
					return;
				}
				this.allowAFK = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013A4 RID: 5028
		// (get) Token: 0x06004CFB RID: 19707 RVA: 0x002A8E6F File Offset: 0x002A706F
		// (set) Token: 0x06004CFC RID: 19708 RVA: 0x002A8E77 File Offset: 0x002A7077
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool SaveServerLogs { get; private set; }

		// Token: 0x170013A5 RID: 5029
		// (get) Token: 0x06004CFD RID: 19709 RVA: 0x002A8E80 File Offset: 0x002A7080
		// (set) Token: 0x06004CFE RID: 19710 RVA: 0x002A8E88 File Offset: 0x002A7088
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowModDownloads { get; private set; } = true;

		// Token: 0x170013A6 RID: 5030
		// (get) Token: 0x06004CFF RID: 19711 RVA: 0x002A8E91 File Offset: 0x002A7091
		// (set) Token: 0x06004D00 RID: 19712 RVA: 0x002A8E99 File Offset: 0x002A7099
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowFileTransfers { get; private set; }

		// Token: 0x170013A7 RID: 5031
		// (get) Token: 0x06004D01 RID: 19713 RVA: 0x002A8EA2 File Offset: 0x002A70A2
		// (set) Token: 0x06004D02 RID: 19714 RVA: 0x002A8EAA File Offset: 0x002A70AA
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowRemoteCampaignInteractions { get; private set; }

		// Token: 0x170013A8 RID: 5032
		// (get) Token: 0x06004D03 RID: 19715 RVA: 0x002A8EB3 File Offset: 0x002A70B3
		// (set) Token: 0x06004D04 RID: 19716 RVA: 0x002A8EBB File Offset: 0x002A70BB
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool VoiceChatEnabled
		{
			get
			{
				return this.voiceChatEnabled;
			}
			set
			{
				if (this.voiceChatEnabled == value)
				{
					return;
				}
				this.voiceChatEnabled = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013A9 RID: 5033
		// (get) Token: 0x06004D05 RID: 19717 RVA: 0x002A8ED5 File Offset: 0x002A70D5
		// (set) Token: 0x06004D06 RID: 19718 RVA: 0x002A8EDD File Offset: 0x002A70DD
		[Serialize(PlayStyle.Casual, IsPropertySaveable.Yes, "", "", false)]
		public PlayStyle PlayStyle
		{
			get
			{
				return this.playstyleSelection;
			}
			set
			{
				this.playstyleSelection = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013AA RID: 5034
		// (get) Token: 0x06004D07 RID: 19719 RVA: 0x002A8EED File Offset: 0x002A70ED
		// (set) Token: 0x06004D08 RID: 19720 RVA: 0x002A8EF5 File Offset: 0x002A70F5
		[Serialize(LosMode.Transparent, IsPropertySaveable.Yes, "", "", false)]
		public LosMode LosMode { get; set; }

		// Token: 0x170013AB RID: 5035
		// (get) Token: 0x06004D09 RID: 19721 RVA: 0x002A8EFE File Offset: 0x002A70FE
		// (set) Token: 0x06004D0A RID: 19722 RVA: 0x002A8F06 File Offset: 0x002A7106
		[Serialize(EnemyHealthBarMode.ShowAll, IsPropertySaveable.Yes, "", "", false)]
		public EnemyHealthBarMode ShowEnemyHealthBars { get; set; }

		// Token: 0x170013AC RID: 5036
		// (get) Token: 0x06004D0B RID: 19723 RVA: 0x002A8F0F File Offset: 0x002A710F
		// (set) Token: 0x06004D0C RID: 19724 RVA: 0x002A8F1C File Offset: 0x002A711C
		[Serialize(800, IsPropertySaveable.Yes, "", "", false)]
		public int LinesPerLogFile
		{
			get
			{
				return this.ServerLog.LinesPerFile;
			}
			set
			{
				this.ServerLog.LinesPerFile = value;
			}
		}

		// Token: 0x170013AD RID: 5037
		// (get) Token: 0x06004D0D RID: 19725 RVA: 0x002A8F2A File Offset: 0x002A712A
		// (set) Token: 0x06004D0E RID: 19726 RVA: 0x002A8F32 File Offset: 0x002A7132
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AutoRestart
		{
			get
			{
				return this.autoRestart;
			}
			set
			{
				this.autoRestart = value;
				this.AutoRestartTimer = (this.autoRestart ? this.AutoRestartInterval : 0f);
			}
		}

		// Token: 0x170013AE RID: 5038
		// (get) Token: 0x06004D0F RID: 19727 RVA: 0x002A8F56 File Offset: 0x002A7156
		// (set) Token: 0x06004D10 RID: 19728 RVA: 0x002A8F66 File Offset: 0x002A7166
		public bool HasPassword
		{
			get
			{
				return !string.IsNullOrEmpty(this.password);
			}
			set
			{
				this.password = (value ? (this.password ?? "_") : null);
			}
		}

		// Token: 0x170013AF RID: 5039
		// (get) Token: 0x06004D11 RID: 19729 RVA: 0x002A8F83 File Offset: 0x002A7183
		// (set) Token: 0x06004D12 RID: 19730 RVA: 0x002A8F8B File Offset: 0x002A718B
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowVoteKick { get; set; }

		// Token: 0x170013B0 RID: 5040
		// (get) Token: 0x06004D13 RID: 19731 RVA: 0x002A8F94 File Offset: 0x002A7194
		// (set) Token: 0x06004D14 RID: 19732 RVA: 0x002A8F9C File Offset: 0x002A719C
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowEndVoting { get; set; }

		// Token: 0x170013B1 RID: 5041
		// (get) Token: 0x06004D15 RID: 19733 RVA: 0x002A8FA5 File Offset: 0x002A71A5
		// (set) Token: 0x06004D16 RID: 19734 RVA: 0x002A8FB0 File Offset: 0x002A71B0
		[Serialize(RespawnMode.MidRound, IsPropertySaveable.Yes, "", "", false)]
		public RespawnMode RespawnMode
		{
			get
			{
				return this.respawnMode;
			}
			set
			{
				if (this.respawnMode == value)
				{
					return;
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.GameStarted && networkMember.IsServer)
				{
					return;
				}
				this.respawnMode = value;
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013B2 RID: 5042
		// (get) Token: 0x06004D17 RID: 19735 RVA: 0x002A8FEF File Offset: 0x002A71EF
		// (set) Token: 0x06004D18 RID: 19736 RVA: 0x002A8FF7 File Offset: 0x002A71F7
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int BotCount { get; set; }

		// Token: 0x170013B3 RID: 5043
		// (get) Token: 0x06004D19 RID: 19737 RVA: 0x002A9000 File Offset: 0x002A7200
		// (set) Token: 0x06004D1A RID: 19738 RVA: 0x002A9008 File Offset: 0x002A7208
		[Serialize(16, IsPropertySaveable.Yes, "", "", false)]
		public int MaxBotCount { get; set; }

		// Token: 0x170013B4 RID: 5044
		// (get) Token: 0x06004D1B RID: 19739 RVA: 0x002A9011 File Offset: 0x002A7211
		// (set) Token: 0x06004D1C RID: 19740 RVA: 0x002A9019 File Offset: 0x002A7219
		[Serialize(BotSpawnMode.Normal, IsPropertySaveable.Yes, "", "", false)]
		public BotSpawnMode BotSpawnMode { get; set; }

		// Token: 0x170013B5 RID: 5045
		// (get) Token: 0x06004D1D RID: 19741 RVA: 0x002A9022 File Offset: 0x002A7222
		// (set) Token: 0x06004D1E RID: 19742 RVA: 0x002A902A File Offset: 0x002A722A
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DisableBotConversations { get; set; }

		// Token: 0x170013B6 RID: 5046
		// (get) Token: 0x06004D1F RID: 19743 RVA: 0x002A9033 File Offset: 0x002A7233
		// (set) Token: 0x06004D20 RID: 19744 RVA: 0x002A903B File Offset: 0x002A723B
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float SelectedLevelDifficulty
		{
			get
			{
				return this.selectedLevelDifficulty;
			}
			set
			{
				this.selectedLevelDifficulty = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x170013B7 RID: 5047
		// (get) Token: 0x06004D21 RID: 19745 RVA: 0x002A9053 File Offset: 0x002A7253
		// (set) Token: 0x06004D22 RID: 19746 RVA: 0x002A905B File Offset: 0x002A725B
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowDisguises { get; set; }

		// Token: 0x170013B8 RID: 5048
		// (get) Token: 0x06004D23 RID: 19747 RVA: 0x002A9064 File Offset: 0x002A7264
		// (set) Token: 0x06004D24 RID: 19748 RVA: 0x002A906C File Offset: 0x002A726C
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowRewiring { get; set; }

		// Token: 0x170013B9 RID: 5049
		// (get) Token: 0x06004D25 RID: 19749 RVA: 0x002A9075 File Offset: 0x002A7275
		// (set) Token: 0x06004D26 RID: 19750 RVA: 0x002A907D File Offset: 0x002A727D
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowImmediateItemDelivery { get; set; }

		// Token: 0x170013BA RID: 5050
		// (get) Token: 0x06004D27 RID: 19751 RVA: 0x002A9086 File Offset: 0x002A7286
		// (set) Token: 0x06004D28 RID: 19752 RVA: 0x002A908E File Offset: 0x002A728E
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool LockAllDefaultWires { get; set; }

		// Token: 0x170013BB RID: 5051
		// (get) Token: 0x06004D29 RID: 19753 RVA: 0x002A9097 File Offset: 0x002A7297
		// (set) Token: 0x06004D2A RID: 19754 RVA: 0x002A909F File Offset: 0x002A729F
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowLinkingWifiToChat { get; set; }

		// Token: 0x170013BC RID: 5052
		// (get) Token: 0x06004D2B RID: 19755 RVA: 0x002A90A8 File Offset: 0x002A72A8
		// (set) Token: 0x06004D2C RID: 19756 RVA: 0x002A90B0 File Offset: 0x002A72B0
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowFriendlyFire { get; set; }

		// Token: 0x170013BD RID: 5053
		// (get) Token: 0x06004D2D RID: 19757 RVA: 0x002A90B9 File Offset: 0x002A72B9
		// (set) Token: 0x06004D2E RID: 19758 RVA: 0x002A90C1 File Offset: 0x002A72C1
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool AllowDragAndDropGive { get; set; }

		// Token: 0x170013BE RID: 5054
		// (get) Token: 0x06004D2F RID: 19759 RVA: 0x002A90CA File Offset: 0x002A72CA
		// (set) Token: 0x06004D30 RID: 19760 RVA: 0x002A90D2 File Offset: 0x002A72D2
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DestructibleOutposts { get; set; }

		// Token: 0x170013BF RID: 5055
		// (get) Token: 0x06004D31 RID: 19761 RVA: 0x002A90DB File Offset: 0x002A72DB
		// (set) Token: 0x06004D32 RID: 19762 RVA: 0x002A90E3 File Offset: 0x002A72E3
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool KillableNPCs { get; set; }

		// Token: 0x170013C0 RID: 5056
		// (get) Token: 0x06004D33 RID: 19763 RVA: 0x002A90EC File Offset: 0x002A72EC
		// (set) Token: 0x06004D34 RID: 19764 RVA: 0x002A90F4 File Offset: 0x002A72F4
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool BanAfterWrongPassword { get; set; }

		// Token: 0x170013C1 RID: 5057
		// (get) Token: 0x06004D35 RID: 19765 RVA: 0x002A90FD File Offset: 0x002A72FD
		// (set) Token: 0x06004D36 RID: 19766 RVA: 0x002A9105 File Offset: 0x002A7305
		[Serialize(3, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPasswordRetriesBeforeBan { get; private set; }

		// Token: 0x170013C2 RID: 5058
		// (get) Token: 0x06004D37 RID: 19767 RVA: 0x002A910E File Offset: 0x002A730E
		// (set) Token: 0x06004D38 RID: 19768 RVA: 0x002A9116 File Offset: 0x002A7316
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool EnableDoSProtection { get; private set; }

		// Token: 0x170013C3 RID: 5059
		// (get) Token: 0x06004D39 RID: 19769 RVA: 0x002A911F File Offset: 0x002A731F
		// (set) Token: 0x06004D3A RID: 19770 RVA: 0x002A9127 File Offset: 0x002A7327
		[Serialize(4000, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPacketAmount { get; private set; }

		// Token: 0x170013C4 RID: 5060
		// (get) Token: 0x06004D3B RID: 19771 RVA: 0x002A9130 File Offset: 0x002A7330
		// (set) Token: 0x06004D3C RID: 19772 RVA: 0x002A9138 File Offset: 0x002A7338
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SelectedSubmarine { get; set; }

		// Token: 0x170013C5 RID: 5061
		// (get) Token: 0x06004D3D RID: 19773 RVA: 0x002A9141 File Offset: 0x002A7341
		// (set) Token: 0x06004D3E RID: 19774 RVA: 0x002A9149 File Offset: 0x002A7349
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SelectedShuttle { get; set; }

		// Token: 0x170013C6 RID: 5062
		// (get) Token: 0x06004D3F RID: 19775 RVA: 0x002A9152 File Offset: 0x002A7352
		// (set) Token: 0x06004D40 RID: 19776 RVA: 0x002A915A File Offset: 0x002A735A
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TraitorProbability
		{
			get
			{
				return this.traitorProbability;
			}
			set
			{
				if (MathUtils.NearlyEqual(this.traitorProbability, value, 0.0001f))
				{
					return;
				}
				this.traitorProbability = MathHelper.Clamp(value, 0f, 1f);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013C7 RID: 5063
		// (get) Token: 0x06004D41 RID: 19777 RVA: 0x002A918D File Offset: 0x002A738D
		// (set) Token: 0x06004D42 RID: 19778 RVA: 0x002A9198 File Offset: 0x002A7398
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int TraitorDangerLevel
		{
			get
			{
				return this.traitorDangerLevel;
			}
			set
			{
				int clampedValue = MathHelper.Clamp(value, 1, 3);
				if (this.traitorDangerLevel == clampedValue)
				{
					return;
				}
				this.traitorDangerLevel = clampedValue;
				this.ServerDetailsChanged = true;
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen == null)
				{
					return;
				}
				netLobbyScreen.SetTraitorDangerLevel(this.traitorDangerLevel);
			}
		}

		// Token: 0x170013C8 RID: 5064
		// (get) Token: 0x06004D43 RID: 19779 RVA: 0x002A91DB File Offset: 0x002A73DB
		// (set) Token: 0x06004D44 RID: 19780 RVA: 0x002A91E3 File Offset: 0x002A73E3
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		public int TraitorsMinPlayerCount
		{
			get
			{
				return this.traitorsMinPlayerCount;
			}
			set
			{
				this.traitorsMinPlayerCount = MathHelper.Clamp(value, 1, NetConfig.MaxPlayers);
			}
		}

		// Token: 0x170013C9 RID: 5065
		// (get) Token: 0x06004D45 RID: 19781 RVA: 0x002A91F7 File Offset: 0x002A73F7
		// (set) Token: 0x06004D46 RID: 19782 RVA: 0x002A91FF File Offset: 0x002A73FF
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		public float MinPercentageOfPlayersForTraitorAccusation { get; set; }

		// Token: 0x170013CA RID: 5066
		// (get) Token: 0x06004D47 RID: 19783 RVA: 0x002A9208 File Offset: 0x002A7408
		// (set) Token: 0x06004D48 RID: 19784 RVA: 0x002A9210 File Offset: 0x002A7410
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public LanguageIdentifier Language { get; set; }

		// Token: 0x170013CB RID: 5067
		// (get) Token: 0x06004D49 RID: 19785 RVA: 0x002A9219 File Offset: 0x002A7419
		// (set) Token: 0x06004D4A RID: 19786 RVA: 0x002A9221 File Offset: 0x002A7421
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode SubSelectionMode
		{
			get
			{
				return this.subSelectionMode;
			}
			set
			{
				this.subSelectionMode = value;
				this.AllowSubVoting = (this.subSelectionMode == SelectionMode.Vote);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013CC RID: 5068
		// (get) Token: 0x06004D4B RID: 19787 RVA: 0x002A9240 File Offset: 0x002A7440
		// (set) Token: 0x06004D4C RID: 19788 RVA: 0x002A9248 File Offset: 0x002A7448
		[Serialize(SelectionMode.Manual, IsPropertySaveable.Yes, "", "", false)]
		public SelectionMode ModeSelectionMode
		{
			get
			{
				return this.modeSelectionMode;
			}
			set
			{
				this.modeSelectionMode = value;
				this.AllowModeVoting = (this.modeSelectionMode == SelectionMode.Vote);
				this.ServerDetailsChanged = true;
			}
		}

		// Token: 0x170013CD RID: 5069
		// (get) Token: 0x06004D4D RID: 19789 RVA: 0x002A9267 File Offset: 0x002A7467
		// (set) Token: 0x06004D4E RID: 19790 RVA: 0x002A926F File Offset: 0x002A746F
		public BanList BanList { get; private set; }

		// Token: 0x170013CE RID: 5070
		// (get) Token: 0x06004D4F RID: 19791 RVA: 0x002A9278 File Offset: 0x002A7478
		// (set) Token: 0x06004D50 RID: 19792 RVA: 0x002A9280 File Offset: 0x002A7480
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float EndVoteRequiredRatio { get; private set; }

		// Token: 0x170013CF RID: 5071
		// (get) Token: 0x06004D51 RID: 19793 RVA: 0x002A9289 File Offset: 0x002A7489
		// (set) Token: 0x06004D52 RID: 19794 RVA: 0x002A9291 File Offset: 0x002A7491
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float VoteRequiredRatio { get; private set; }

		// Token: 0x170013D0 RID: 5072
		// (get) Token: 0x06004D53 RID: 19795 RVA: 0x002A929A File Offset: 0x002A749A
		// (set) Token: 0x06004D54 RID: 19796 RVA: 0x002A92A2 File Offset: 0x002A74A2
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float VoteTimeout { get; private set; }

		// Token: 0x170013D1 RID: 5073
		// (get) Token: 0x06004D55 RID: 19797 RVA: 0x002A92AB File Offset: 0x002A74AB
		// (set) Token: 0x06004D56 RID: 19798 RVA: 0x002A92B3 File Offset: 0x002A74B3
		[Serialize(0.6f, IsPropertySaveable.Yes, "", "", false)]
		public float KickVoteRequiredRatio { get; private set; }

		// Token: 0x170013D2 RID: 5074
		// (get) Token: 0x06004D57 RID: 19799 RVA: 0x002A92BC File Offset: 0x002A74BC
		// (set) Token: 0x06004D58 RID: 19800 RVA: 0x002A92C4 File Offset: 0x002A74C4
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float DisallowKickVoteTime { get; private set; }

		// Token: 0x170013D3 RID: 5075
		// (get) Token: 0x06004D59 RID: 19801 RVA: 0x002A92CD File Offset: 0x002A74CD
		// (set) Token: 0x06004D5A RID: 19802 RVA: 0x002A92D5 File Offset: 0x002A74D5
		[Serialize(300f, IsPropertySaveable.Yes, "", "", false)]
		public float KillDisconnectedTime { get; set; }

		// Token: 0x170013D4 RID: 5076
		// (get) Token: 0x06004D5B RID: 19803 RVA: 0x002A92DE File Offset: 0x002A74DE
		// (set) Token: 0x06004D5C RID: 19804 RVA: 0x002A92E6 File Offset: 0x002A74E6
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float DespawnDisconnectedPermadeathTime { get; private set; }

		// Token: 0x170013D5 RID: 5077
		// (get) Token: 0x06004D5D RID: 19805 RVA: 0x002A92EF File Offset: 0x002A74EF
		// (set) Token: 0x06004D5E RID: 19806 RVA: 0x002A92F7 File Offset: 0x002A74F7
		[Serialize(600f, IsPropertySaveable.Yes, "", "", false)]
		public float KickAFKTime { get; private set; }

		// Token: 0x170013D6 RID: 5078
		// (get) Token: 0x06004D5F RID: 19807 RVA: 0x002A9300 File Offset: 0x002A7500
		// (set) Token: 0x06004D60 RID: 19808 RVA: 0x002A9308 File Offset: 0x002A7508
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		public float MinimumMidRoundSyncTimeout { get; private set; } = 30f;

		// Token: 0x170013D7 RID: 5079
		// (get) Token: 0x06004D61 RID: 19809 RVA: 0x002A9311 File Offset: 0x002A7511
		// (set) Token: 0x06004D62 RID: 19810 RVA: 0x002A9319 File Offset: 0x002A7519
		[Serialize(120f, IsPropertySaveable.Yes, "", "", false)]
		public float RoundStartSyncDuration { get; private set; } = 120f;

		// Token: 0x170013D8 RID: 5080
		// (get) Token: 0x06004D63 RID: 19811 RVA: 0x002A9322 File Offset: 0x002A7522
		// (set) Token: 0x06004D64 RID: 19812 RVA: 0x002A932A File Offset: 0x002A752A
		[Serialize(15f, IsPropertySaveable.Yes, "", "", false)]
		public float EventRemovalTime { get; private set; } = 15f;

		// Token: 0x170013D9 RID: 5081
		// (get) Token: 0x06004D65 RID: 19813 RVA: 0x002A9333 File Offset: 0x002A7533
		// (set) Token: 0x06004D66 RID: 19814 RVA: 0x002A933B File Offset: 0x002A753B
		[Serialize(20f, IsPropertySaveable.Yes, "", "", false)]
		public float OldReceivedEventKickTime { get; private set; } = 20f;

		// Token: 0x170013DA RID: 5082
		// (get) Token: 0x06004D67 RID: 19815 RVA: 0x002A9344 File Offset: 0x002A7544
		// (set) Token: 0x06004D68 RID: 19816 RVA: 0x002A934C File Offset: 0x002A754C
		[Serialize(40f, IsPropertySaveable.Yes, "", "", false)]
		public float OldEventKickTime { get; private set; } = 40f;

		// Token: 0x170013DB RID: 5083
		// (get) Token: 0x06004D69 RID: 19817 RVA: 0x002A9355 File Offset: 0x002A7555
		// (set) Token: 0x06004D6A RID: 19818 RVA: 0x002A935D File Offset: 0x002A755D
		[Serialize(60f, IsPropertySaveable.Yes, "", "", false)]
		public float TimeoutThresholdNotInGame { get; private set; } = 60f;

		// Token: 0x170013DC RID: 5084
		// (get) Token: 0x06004D6B RID: 19819 RVA: 0x002A9366 File Offset: 0x002A7566
		// (set) Token: 0x06004D6C RID: 19820 RVA: 0x002A936E File Offset: 0x002A756E
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		public float TimeoutThresholdInGame { get; private set; } = 10f;

		// Token: 0x170013DD RID: 5085
		// (get) Token: 0x06004D6D RID: 19821 RVA: 0x002A9377 File Offset: 0x002A7577
		// (set) Token: 0x06004D6E RID: 19822 RVA: 0x002A9380 File Offset: 0x002A7580
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool KarmaEnabled
		{
			get
			{
				return this.karmaEnabled;
			}
			set
			{
				this.karmaEnabled = value;
				if (this.karmaSettingsList != null)
				{
					this.SetElementInteractability(this.karmaSettingsList.Content, !this.karmaEnabled || this.KarmaPreset != "custom");
				}
				this.karmaElements.ForEach(delegate(GUIComponent e)
				{
					e.Visible = this.karmaEnabled;
				});
			}
		}

		// Token: 0x170013DE RID: 5086
		// (get) Token: 0x06004D6F RID: 19823 RVA: 0x002A93DF File Offset: 0x002A75DF
		// (set) Token: 0x06004D70 RID: 19824 RVA: 0x002A93E8 File Offset: 0x002A75E8
		[Serialize("default", IsPropertySaveable.Yes, "", "", false)]
		public string KarmaPreset
		{
			get
			{
				return this.karmaPreset;
			}
			set
			{
				if (this.karmaPreset == value)
				{
					return;
				}
				if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null)
					{
						KarmaManager karmaManager = networkMember.KarmaManager;
						if (karmaManager != null)
						{
							karmaManager.SelectPreset(value);
						}
					}
				}
				this.karmaPreset = value;
			}
		}

		// Token: 0x170013DF RID: 5087
		// (get) Token: 0x06004D71 RID: 19825 RVA: 0x002A943A File Offset: 0x002A763A
		// (set) Token: 0x06004D72 RID: 19826 RVA: 0x002A9442 File Offset: 0x002A7642
		[Serialize("sandbox", IsPropertySaveable.Yes, "", "", false)]
		public Identifier GameModeIdentifier { get; set; }

		// Token: 0x170013E0 RID: 5088
		// (get) Token: 0x06004D73 RID: 19827 RVA: 0x002A944B File Offset: 0x002A764B
		// (set) Token: 0x06004D74 RID: 19828 RVA: 0x002A9484 File Offset: 0x002A7684
		[Serialize("All", IsPropertySaveable.Yes, "", "", false)]
		public string MissionTypes
		{
			get
			{
				return string.Join<Identifier>(",", from t in this.AllowedRandomMissionTypes
				select t.ToIdentifier<Identifier>());
			}
			set
			{
				this.AllowedRandomMissionTypes = (from t in value.Split(",", StringSplitOptions.None)
				select t.ToIdentifier()).Distinct<Identifier>().ToList<Identifier>();
				this.ValidateMissionTypes();
			}
		}

		// Token: 0x170013E1 RID: 5089
		// (get) Token: 0x06004D75 RID: 19829 RVA: 0x002A94D7 File Offset: 0x002A76D7
		// (set) Token: 0x06004D76 RID: 19830 RVA: 0x002A94DF File Offset: 0x002A76DF
		[Serialize(8, IsPropertySaveable.Yes, "", "", false)]
		public int MaxPlayers
		{
			get
			{
				return this.maxPlayers;
			}
			set
			{
				this.maxPlayers = MathHelper.Clamp(value, 0, NetConfig.MaxPlayers);
			}
		}

		// Token: 0x170013E2 RID: 5090
		// (get) Token: 0x06004D77 RID: 19831 RVA: 0x002A94F3 File Offset: 0x002A76F3
		// (set) Token: 0x06004D78 RID: 19832 RVA: 0x002A94FB File Offset: 0x002A76FB
		public List<Identifier> AllowedRandomMissionTypes { get; private set; }

		// Token: 0x170013E3 RID: 5091
		// (get) Token: 0x06004D79 RID: 19833 RVA: 0x002A9504 File Offset: 0x002A7704
		// (set) Token: 0x06004D7A RID: 19834 RVA: 0x002A950C File Offset: 0x002A770C
		[Serialize(3600f, IsPropertySaveable.Yes, "", "", false)]
		public float AutoBanTime { get; private set; }

		// Token: 0x170013E4 RID: 5092
		// (get) Token: 0x06004D7B RID: 19835 RVA: 0x002A9515 File Offset: 0x002A7715
		// (set) Token: 0x06004D7C RID: 19836 RVA: 0x002A951D File Offset: 0x002A771D
		[Serialize(86400f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxAutoBanTime { get; private set; }

		// Token: 0x170013E5 RID: 5093
		// (get) Token: 0x06004D7D RID: 19837 RVA: 0x002A9526 File Offset: 0x002A7726
		// (set) Token: 0x06004D7E RID: 19838 RVA: 0x002A952E File Offset: 0x002A772E
		[Serialize(LootedMoneyDestination.Bank, IsPropertySaveable.Yes, "", "", false)]
		public LootedMoneyDestination LootedMoneyDestination { get; set; }

		// Token: 0x170013E6 RID: 5094
		// (get) Token: 0x06004D7F RID: 19839 RVA: 0x002A9537 File Offset: 0x002A7737
		// (set) Token: 0x06004D80 RID: 19840 RVA: 0x002A953F File Offset: 0x002A773F
		[Serialize(999999, IsPropertySaveable.Yes, "", "", false)]
		public int MaximumMoneyTransferRequest { get; set; }

		// Token: 0x170013E7 RID: 5095
		// (get) Token: 0x06004D81 RID: 19841 RVA: 0x002A9548 File Offset: 0x002A7748
		// (set) Token: 0x06004D82 RID: 19842 RVA: 0x002A9550 File Offset: 0x002A7750
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float NewCampaignDefaultSalary { get; set; }

		// Token: 0x170013E8 RID: 5096
		// (get) Token: 0x06004D83 RID: 19843 RVA: 0x002A9559 File Offset: 0x002A7759
		// (set) Token: 0x06004D84 RID: 19844 RVA: 0x002A9561 File Offset: 0x002A7761
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool TrackOpponentInPvP { get; set; }

		// Token: 0x170013E9 RID: 5097
		// (get) Token: 0x06004D85 RID: 19845 RVA: 0x002A956A File Offset: 0x002A776A
		// (set) Token: 0x06004D86 RID: 19846 RVA: 0x002A9572 File Offset: 0x002A7772
		[Serialize(7, IsPropertySaveable.Yes, "", "", false)]
		public int DisembarkPointAllowance { get; set; }

		// Token: 0x170013EA RID: 5098
		// (get) Token: 0x06004D87 RID: 19847 RVA: 0x002A957B File Offset: 0x002A777B
		// (set) Token: 0x06004D88 RID: 19848 RVA: 0x002A9583 File Offset: 0x002A7783
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[DoNotSyncOverNetwork]
		public Identifier[] SelectedCoalitionPerks { get; set; } = Array.Empty<Identifier>();

		// Token: 0x170013EB RID: 5099
		// (get) Token: 0x06004D89 RID: 19849 RVA: 0x002A958C File Offset: 0x002A778C
		// (set) Token: 0x06004D8A RID: 19850 RVA: 0x002A9594 File Offset: 0x002A7794
		[Serialize(200, IsPropertySaveable.Yes, "", "", false)]
		public int WinScorePvP { get; set; }

		// Token: 0x170013EC RID: 5100
		// (get) Token: 0x06004D8B RID: 19851 RVA: 0x002A959D File Offset: 0x002A779D
		// (set) Token: 0x06004D8C RID: 19852 RVA: 0x002A95A5 File Offset: 0x002A77A5
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[DoNotSyncOverNetwork]
		public Identifier[] SelectedSeparatistsPerks { get; set; } = Array.Empty<Identifier>();

		// Token: 0x170013ED RID: 5101
		// (get) Token: 0x06004D8D RID: 19853 RVA: 0x002A95AE File Offset: 0x002A77AE
		// (set) Token: 0x06004D8E RID: 19854 RVA: 0x002A95B6 File Offset: 0x002A77B6
		public CampaignSettings CampaignSettings { get; set; } = CampaignSettings.Empty;

		// Token: 0x170013EE RID: 5102
		// (get) Token: 0x06004D8F RID: 19855 RVA: 0x002A95BF File Offset: 0x002A77BF
		// (set) Token: 0x06004D90 RID: 19856 RVA: 0x002A95C8 File Offset: 0x002A77C8
		public bool AllowSubVoting
		{
			get
			{
				return this.allowSubVoting;
			}
			set
			{
				if (value == this.allowSubVoting)
				{
					return;
				}
				this.allowSubVoting = value;
				GameMain.NetLobbyScreen.SubList.Enabled = (value || (GameMain.Client != null && GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.SelectSub)));
				GUITextBlock subVotesLabel = GameMain.NetLobbyScreen.Frame.FindChild("subvotes", true) as GUITextBlock;
				subVotesLabel.Visible = value;
				GUIButton subVisButton = GameMain.NetLobbyScreen.SubVisibilityButton;
				subVisButton.RectTransform.AbsoluteOffset = new Point(value ? ((int)(subVotesLabel.TextSize.X + (float)subVisButton.Rect.Width)) : 0, 0);
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.Voting.UpdateVoteTexts(null, VoteType.Sub);
				}
				GameMain.NetLobbyScreen.SubList.Deselect();
			}
		}

		// Token: 0x170013EF RID: 5103
		// (get) Token: 0x06004D91 RID: 19857 RVA: 0x002A9694 File Offset: 0x002A7894
		// (set) Token: 0x06004D92 RID: 19858 RVA: 0x002A969C File Offset: 0x002A789C
		public bool AllowModeVoting
		{
			get
			{
				return this.allowModeVoting;
			}
			set
			{
				if (value == this.allowModeVoting)
				{
					return;
				}
				this.allowModeVoting = value;
				GameMain.NetLobbyScreen.ModeList.Enabled = (value || (GameMain.Client != null && GameMain.Client.HasPermission(Barotrauma.Networking.ClientPermissions.SelectMode)));
				GameMain.NetLobbyScreen.Frame.FindChild("modevotes", true).Visible = value;
				foreach (GUIComponent guiComponent in GameMain.NetLobbyScreen.ModeList.Content.Children)
				{
					GUIFrame frame = guiComponent as GUIFrame;
					if (frame != null)
					{
						frame.CanBeFocused = (!this.allowModeVoting || ((GameModePreset)frame.UserData).Votable);
					}
				}
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.Voting.UpdateVoteTexts(null, VoteType.Mode);
				}
				GameMain.NetLobbyScreen.ModeList.Deselect();
			}
		}

		// Token: 0x06004D93 RID: 19859 RVA: 0x002A9798 File Offset: 0x002A7998
		public void SetPassword(string password)
		{
			this.password = (string.IsNullOrEmpty(password) ? null : password);
		}

		// Token: 0x06004D94 RID: 19860 RVA: 0x002A97AC File Offset: 0x002A79AC
		public static byte[] SaltPassword(byte[] password, int salt)
		{
			byte[] saltedPw = new byte[password.Length * 2];
			for (int i = 0; i < password.Length; i++)
			{
				saltedPw[i * 2] = password[i];
				saltedPw[i * 2 + 1] = (byte)(salt >> 8 * (i % 4) & 255);
			}
			return NetUtility.ComputeSHAHash(saltedPw);
		}

		// Token: 0x06004D95 RID: 19861 RVA: 0x002A97FC File Offset: 0x002A79FC
		public bool IsPasswordCorrect(byte[] input, int salt)
		{
			if (!this.HasPassword)
			{
				return true;
			}
			byte[] saltedPw = ServerSettings.SaltPassword(Encoding.UTF8.GetBytes(this.password), salt);
			return saltedPw.SequenceEqual(input);
		}

		// Token: 0x170013F0 RID: 5104
		// (get) Token: 0x06004D96 RID: 19862 RVA: 0x002A983B File Offset: 0x002A7A3B
		// (set) Token: 0x06004D97 RID: 19863 RVA: 0x002A9843 File Offset: 0x002A7A43
		public List<Range<int>> AllowedClientNameChars { get; private set; } = new List<Range<int>>();

		// Token: 0x06004D98 RID: 19864 RVA: 0x002A984C File Offset: 0x002A7A4C
		private void InitMonstersEnabled()
		{
			if (this.MonsterEnabled == null || this.MonsterEnabled.Count != CharacterPrefab.Prefabs.Count<CharacterPrefab>())
			{
				this.MonsterEnabled = (from p in CharacterPrefab.Prefabs
				select new ValueTuple<Identifier, bool>(p.Identifier, true)).ToDictionary<Identifier, bool>();
			}
		}

		// Token: 0x06004D99 RID: 19865 RVA: 0x002A98AC File Offset: 0x002A7AAC
		private static IReadOnlyList<Identifier> ExtractAndSortKeys(IReadOnlyDictionary<Identifier, bool> monsterEnabled)
		{
			return (from k in monsterEnabled.Keys
			orderby CharacterPrefab.Prefabs[k].UintIdentifier
			select k).ToImmutableArray<Identifier>();
		}

		// Token: 0x06004D9A RID: 19866 RVA: 0x002A98E4 File Offset: 0x002A7AE4
		public bool ReadMonsterEnabled(IReadMessage inc)
		{
			bool changed = false;
			this.InitMonstersEnabled();
			IReadOnlyList<Identifier> monsterNames = ServerSettings.ExtractAndSortKeys(this.MonsterEnabled);
			uint receivedMonsterCount = inc.ReadVariableUInt32();
			if ((long)monsterNames.Count != (long)((ulong)receivedMonsterCount))
			{
				inc.BitPosition += (int)receivedMonsterCount;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Expected monster count ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(monsterNames.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", got ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(receivedMonsterCount);
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			else
			{
				foreach (Identifier s in monsterNames)
				{
					bool prevEnabled;
					this.MonsterEnabled.TryGetValue(s, out prevEnabled);
					this.MonsterEnabled[s] = inc.ReadBoolean();
					changed |= (prevEnabled != this.MonsterEnabled[s]);
				}
			}
			inc.ReadPadBits();
			return changed;
		}

		// Token: 0x06004D9B RID: 19867 RVA: 0x002A99E8 File Offset: 0x002A7BE8
		public void WriteMonsterEnabled(IWriteMessage msg, Dictionary<Identifier, bool> monsterEnabled = null)
		{
			this.InitMonstersEnabled();
			if (monsterEnabled == null)
			{
				monsterEnabled = this.MonsterEnabled;
			}
			IReadOnlyList<Identifier> monsterNames = ServerSettings.ExtractAndSortKeys(monsterEnabled);
			msg.WriteVariableUInt32((uint)monsterNames.Count);
			foreach (Identifier s in monsterNames)
			{
				msg.WriteBoolean(monsterEnabled[s]);
			}
			msg.WritePadBits();
		}

		// Token: 0x06004D9C RID: 19868 RVA: 0x002A9A60 File Offset: 0x002A7C60
		public bool ReadExtraCargo(IReadMessage msg)
		{
			bool changed = false;
			uint count = msg.ReadUInt32();
			if (this.ExtraCargo == null || (ulong)count != (ulong)((long)this.ExtraCargo.Count))
			{
				changed = true;
			}
			Dictionary<ItemPrefab, int> extraCargo = new Dictionary<ItemPrefab, int>();
			int i = 0;
			while ((long)i < (long)((ulong)count))
			{
				Identifier prefabIdentifier = msg.ReadIdentifier();
				byte amount = msg.ReadByte();
				ItemPrefab itemPrefab = MapEntityPrefab.Find(null, prefabIdentifier, false) as ItemPrefab;
				if (itemPrefab != null && amount > 0 && this.ExtraCargo.Keys.Count<ItemPrefab>() < 20 && (!this.ExtraCargo.ContainsKey(itemPrefab) || this.ExtraCargo[itemPrefab] < 10))
				{
					if (changed || !this.ExtraCargo.ContainsKey(itemPrefab) || this.ExtraCargo[itemPrefab] != (int)amount)
					{
						changed = true;
					}
					extraCargo.Add(itemPrefab, (int)amount);
				}
				i++;
			}
			if (changed)
			{
				this.ExtraCargo = extraCargo;
			}
			return changed;
		}

		// Token: 0x06004D9D RID: 19869 RVA: 0x002A9B44 File Offset: 0x002A7D44
		public void WriteExtraCargo(IWriteMessage msg)
		{
			if (this.ExtraCargo == null)
			{
				msg.WriteUInt32(0U);
				return;
			}
			msg.WriteUInt32((uint)this.ExtraCargo.Count);
			foreach (KeyValuePair<ItemPrefab, int> kvp in this.ExtraCargo)
			{
				msg.WriteIdentifier(kvp.Key.Identifier);
				msg.WriteByte((byte)kvp.Value);
			}
		}

		// Token: 0x06004D9E RID: 19870 RVA: 0x002A9BD4 File Offset: 0x002A7DD4
		public void WritePerks(IWriteMessage msg)
		{
			List<DisembarkPerkPrefab> coalitionPerks = ServerSettings.<WritePerks>g__GetPerks|512_0(this.SelectedCoalitionPerks);
			msg.WriteVariableUInt32((uint)coalitionPerks.Count);
			foreach (DisembarkPerkPrefab perk in coalitionPerks)
			{
				msg.WriteUInt32(perk.UintIdentifier);
			}
			List<DisembarkPerkPrefab> separatistsPerks = ServerSettings.<WritePerks>g__GetPerks|512_0(this.SelectedSeparatistsPerks);
			msg.WriteVariableUInt32((uint)separatistsPerks.Count);
			foreach (DisembarkPerkPrefab perk2 in separatistsPerks)
			{
				msg.WriteUInt32(perk2.UintIdentifier);
			}
		}

		// Token: 0x06004D9F RID: 19871 RVA: 0x002A9C9C File Offset: 0x002A7E9C
		public bool ReadPerks(IReadMessage msg)
		{
			uint coalitionCount = msg.ReadVariableUInt32();
			Identifier[] newCoalitionPerks = new Identifier[coalitionCount];
			int i = 0;
			while ((long)i < (long)((ulong)coalitionCount))
			{
				uint id = msg.ReadUInt32();
				DisembarkPerkPrefab prefab = DisembarkPerkPrefab.Prefabs.Find((DisembarkPerkPrefab p) => p.UintIdentifier == id);
				if (prefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Perk not found: ");
					defaultInterpolatedStringHandler.AppendFormatted<uint>(id);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					newCoalitionPerks[i] = prefab.Identifier;
				}
				i++;
			}
			uint separatistsCount = msg.ReadVariableUInt32();
			Identifier[] newSeparatistsPerks = new Identifier[separatistsCount];
			int j = 0;
			while ((long)j < (long)((ulong)separatistsCount))
			{
				uint id = msg.ReadUInt32();
				DisembarkPerkPrefab prefab2 = DisembarkPerkPrefab.Prefabs.Find((DisembarkPerkPrefab p) => p.UintIdentifier == id);
				if (prefab2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Perk not found: ");
					defaultInterpolatedStringHandler2.AppendFormatted<uint>(id);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					newSeparatistsPerks[j] = prefab2.Identifier;
				}
				j++;
			}
			bool changed = !this.SelectedCoalitionPerks.SequenceEqual(newCoalitionPerks) || !this.SelectedSeparatistsPerks.SequenceEqual(newSeparatistsPerks);
			this.SelectedCoalitionPerks = newCoalitionPerks;
			this.SelectedSeparatistsPerks = newSeparatistsPerks;
			return changed;
		}

		// Token: 0x06004DA0 RID: 19872 RVA: 0x002A9E24 File Offset: 0x002A8024
		public void ReadHiddenSubs(IReadMessage msg)
		{
			IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
			this.HiddenSubs.Clear();
			uint count = msg.ReadVariableUInt32();
			int i = 0;
			while ((long)i < (long)((ulong)count))
			{
				int index = (int)msg.ReadUInt16();
				if (index < subList.Count)
				{
					string submarineName = subList[index].Name;
					this.HiddenSubs.Add(submarineName);
				}
				i++;
			}
		}

		// Token: 0x06004DA1 RID: 19873 RVA: 0x002A9E88 File Offset: 0x002A8088
		public void WriteHiddenSubs(IWriteMessage msg)
		{
			IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
			msg.WriteVariableUInt32((uint)this.HiddenSubs.Count);
			using (HashSet<string>.Enumerator enumerator = this.HiddenSubs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string submarineName = enumerator.Current;
					msg.WriteUInt16((ushort)subList.FindIndex((SubmarineInfo s) => s.Name.Equals(submarineName, StringComparison.OrdinalIgnoreCase)));
				}
			}
		}

		// Token: 0x06004DA2 RID: 19874 RVA: 0x002A9F14 File Offset: 0x002A8114
		public void UpdateServerListInfo(Action<Identifier, object> setter)
		{
			ServerSettings.<>c__DisplayClass516_0 CS$<>8__locals1;
			CS$<>8__locals1.setter = setter;
			ServerSettings.<UpdateServerListInfo>g__set|516_0("ServerName", this.ServerName, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("MaxPlayers", this.MaxPlayers, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("HasPassword", this.HasPassword, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("message", this.ServerMessageText, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("version", GameMain.Version, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("playercount", GameMain.NetworkMember.ConnectedClients.Count, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("contentpackages", from p in ContentPackageManager.EnabledPackages.All
			where p.HasMultiplayerSyncedContent
			select p, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("modeselectionmode", this.ModeSelectionMode, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("subselectionmode", this.SubSelectionMode, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("voicechatenabled", this.VoiceChatEnabled, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("allowspectating", this.AllowSpectating, ref CS$<>8__locals1);
			RespawnMode respawnMode = this.RespawnMode;
			bool flag = respawnMode - RespawnMode.MidRound <= 1;
			ServerSettings.<UpdateServerListInfo>g__set|516_0("allowrespawn", flag, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("traitors", this.TraitorProbability.ToString(CultureInfo.InvariantCulture), ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("friendlyfireenabled", this.AllowFriendlyFire, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("karmaenabled", this.KarmaEnabled, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("gamestarted", GameMain.NetworkMember.GameStarted, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("gamemode", this.GameModeIdentifier, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("playstyle", this.PlayStyle, ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("language", this.Language.ToString(), ref CS$<>8__locals1);
			ServerSettings.<UpdateServerListInfo>g__set|516_0("eoscrossplay", EosInterface.IdQueries.IsLoggedIntoEosConnect || EosSessionManager.CurrentOwnedSession.IsSome(), ref CS$<>8__locals1);
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			if (((netLobbyScreen != null) ? netLobbyScreen.SelectedSub : null) != null)
			{
				ServerSettings.<UpdateServerListInfo>g__set|516_0("submarine", GameMain.NetLobbyScreen.SelectedSub.Name, ref CS$<>8__locals1);
			}
			if (SteamClient.IsLoggedOn)
			{
				NetPingLocation? netPingLocation;
				string pingLocation = (SteamNetworkingUtils.LocalPingLocation != null) ? netPingLocation.GetValueOrDefault().ToString() : null;
				if (!pingLocation.IsNullOrEmpty())
				{
					ServerSettings.<UpdateServerListInfo>g__set|516_0("steampinglocation", pingLocation, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06004DA3 RID: 19875 RVA: 0x002AA1B5 File Offset: 0x002A83B5
		private void ValidateMissionTypes()
		{
			this.ValidateMissionTypes(MissionPrefab.CoOpMissionClasses.Values);
			this.ValidateMissionTypes(MissionPrefab.PvPMissionClasses.Values);
		}

		// Token: 0x06004DA4 RID: 19876 RVA: 0x002AA1D8 File Offset: 0x002A83D8
		private void ValidateMissionTypes(IEnumerable<Type> availableMissionClasses)
		{
			if (this.AllowedRandomMissionTypes.Contains(Tags.MissionTypeAll))
			{
				return;
			}
			if (MissionPrefab.GetAllMultiplayerSelectableMissionTypes().None((Identifier missionType) => MissionPrefab.Prefabs.Any(delegate(MissionPrefab p)
			{
				Identifier type = p.Type;
				return type == missionType && this.AllowedRandomMissionTypes.Contains(p.Type) && availableMissionClasses.Contains(p.MissionClass);
			})))
			{
				MissionPrefab matchingMission = MissionPrefab.Prefabs.First((MissionPrefab p) => availableMissionClasses.Contains(p.MissionClass));
				if (matchingMission == null)
				{
					DebugConsole.ThrowError("No missions found for any of the available mission classes (" + string.Join<Type>(",", availableMissionClasses) + ")", null, null, false, false);
					return;
				}
				this.AllowedRandomMissionTypes.Add(matchingMission.Type);
			}
		}

		// Token: 0x06004DA5 RID: 19877 RVA: 0x002AA278 File Offset: 0x002A8478
		// Note: this type is marked as 'beforefieldinit'.
		static ServerSettings()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(ServerSettings.packetAmountTooltip);
			defaultInterpolatedStringHandler.AppendLiteral("\n\n‖color:gui.red‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("PacketLimitWarning"));
			defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
			ServerSettings.packetAmountTooltipWarning = RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			ReadOnlySpan<char> str = "Data";
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			ServerSettings.PermissionPresetFile = str + new ReadOnlySpan<char>(ref directorySeparatorChar) + "permissionpresets.xml";
			ReadOnlySpan<char> str2 = "Data";
			char directorySeparatorChar2 = Path.DirectorySeparatorChar;
			ServerSettings.PermissionPresetFileCustom = str2 + new ReadOnlySpan<char>(ref directorySeparatorChar2) + "permissionpresets_player.xml";
		}

		// Token: 0x06004DA9 RID: 19881 RVA: 0x002AA390 File Offset: 0x002A8590
		[CompilerGenerated]
		internal static List<DisembarkPerkPrefab> <WritePerks>g__GetPerks|512_0(Identifier[] perkIdentifiers)
		{
			List<DisembarkPerkPrefab> perks = new List<DisembarkPerkPrefab>();
			foreach (Identifier perk in perkIdentifiers)
			{
				DisembarkPerkPrefab prefab;
				if (DisembarkPerkPrefab.Prefabs.TryGet(perk, out prefab))
				{
					perks.Add(prefab);
				}
			}
			return perks;
		}

		// Token: 0x06004DAA RID: 19882 RVA: 0x002AA3D3 File Offset: 0x002A85D3
		[CompilerGenerated]
		internal static void <UpdateServerListInfo>g__set|516_0(string key, object obj, ref ServerSettings.<>c__DisplayClass516_0 A_2)
		{
			A_2.setter(key.ToIdentifier(), obj);
		}

		// Token: 0x04002808 RID: 10248
		private static readonly LocalizedString packetAmountTooltip = TextManager.Get("ServerSettingsMaxPacketAmountTooltip");

		// Token: 0x04002809 RID: 10249
		private static readonly RichString packetAmountTooltipWarning;

		// Token: 0x0400280A RID: 10250
		public static bool SuppressNetworkMessages;

		// Token: 0x0400280B RID: 10251
		private Dictionary<Identifier, bool> tempMonsterEnabled;

		// Token: 0x0400280C RID: 10252
		private GUIFrame settingsFrame;

		// Token: 0x0400280D RID: 10253
		private readonly Dictionary<ServerSettings.SettingsTab, GUIComponent> settingsTabs = new Dictionary<ServerSettings.SettingsTab, GUIComponent>();

		// Token: 0x0400280E RID: 10254
		private readonly Dictionary<ServerSettings.SettingsTab, GUIButton> tabButtons = new Dictionary<ServerSettings.SettingsTab, GUIButton>();

		// Token: 0x0400280F RID: 10255
		private ServerSettings.SettingsTab selectedTab;

		// Token: 0x04002810 RID: 10256
		private readonly List<GUIComponent> karmaElements = new List<GUIComponent>();

		// Token: 0x04002811 RID: 10257
		private GUIDropDown karmaPresetDD;

		// Token: 0x04002812 RID: 10258
		private GUIListBox karmaSettingsList;

		// Token: 0x04002813 RID: 10259
		private GUIComponent extraCargoPanel;

		// Token: 0x04002814 RID: 10260
		private GUIComponent monstersEnabledPanel;

		// Token: 0x04002815 RID: 10261
		private GUIButton extraCargoButton;

		// Token: 0x04002816 RID: 10262
		private GUIButton monstersEnabledButton;

		// Token: 0x04002817 RID: 10263
		public const int PacketLimitMin = 1200;

		// Token: 0x04002818 RID: 10264
		public const int PacketLimitWarning = 3500;

		// Token: 0x04002819 RID: 10265
		public const int PacketLimitDefault = 4000;

		// Token: 0x0400281A RID: 10266
		public const int PacketLimitMax = 10000;

		// Token: 0x0400281B RID: 10267
		public const string SettingsFile = "serversettings.xml";

		// Token: 0x0400281C RID: 10268
		public static readonly string PermissionPresetFile;

		// Token: 0x0400281D RID: 10269
		public static readonly string PermissionPresetFileCustom;

		// Token: 0x0400281E RID: 10270
		public bool ServerDetailsChanged;

		// Token: 0x04002820 RID: 10272
		private readonly Dictionary<uint, ServerSettings.NetPropertyData> netProperties;

		// Token: 0x04002821 RID: 10273
		private string serverName = string.Empty;

		// Token: 0x04002822 RID: 10274
		private string serverMessageText;

		// Token: 0x04002823 RID: 10275
		public int Port;

		// Token: 0x04002824 RID: 10276
		public int QueryPort;

		// Token: 0x04002825 RID: 10277
		public IPAddress ListenIPAddress;

		// Token: 0x04002826 RID: 10278
		public bool EnableUPnP;

		// Token: 0x04002827 RID: 10279
		public ServerLog ServerLog;

		// Token: 0x04002829 RID: 10281
		public const int MaxExtraCargoItemsOfType = 10;

		// Token: 0x0400282A RID: 10282
		public const int MaxExtraCargoItemTypes = 20;

		// Token: 0x0400282D RID: 10285
		private float selectedLevelDifficulty;

		// Token: 0x0400282E RID: 10286
		private string password;

		// Token: 0x0400282F RID: 10287
		public float AutoRestartTimer;

		// Token: 0x04002830 RID: 10288
		private bool autoRestart;

		// Token: 0x04002831 RID: 10289
		private int maxPlayers;

		// Token: 0x04002834 RID: 10292
		public const int DefaultTickRate = 20;

		// Token: 0x04002835 RID: 10293
		private int tickRate = 20;

		// Token: 0x04002836 RID: 10294
		private int maxLagCompensation = 150;

		// Token: 0x0400284C RID: 10316
		private bool allowSpectating;

		// Token: 0x0400284D RID: 10317
		private bool allowAFK;

		// Token: 0x04002852 RID: 10322
		private bool voiceChatEnabled;

		// Token: 0x04002853 RID: 10323
		private PlayStyle playstyleSelection;

		// Token: 0x04002858 RID: 10328
		private RespawnMode respawnMode;

		// Token: 0x0400286C RID: 10348
		private float traitorProbability;

		// Token: 0x0400286D RID: 10349
		private int traitorDangerLevel;

		// Token: 0x0400286E RID: 10350
		private int traitorsMinPlayerCount;

		// Token: 0x04002871 RID: 10353
		private SelectionMode subSelectionMode;

		// Token: 0x04002872 RID: 10354
		private SelectionMode modeSelectionMode;

		// Token: 0x04002883 RID: 10371
		private bool karmaEnabled;

		// Token: 0x04002884 RID: 10372
		private string karmaPreset = "default";

		// Token: 0x04002892 RID: 10386
		private bool allowSubVoting;

		// Token: 0x04002893 RID: 10387
		private bool allowModeVoting;

		// Token: 0x02001224 RID: 4644
		private class NetPropertyData
		{
			// Token: 0x0600933F RID: 37695 RVA: 0x003CB812 File Offset: 0x003C9A12
			public void AssignGUIComponent(GUIComponent component)
			{
				this.GUIComponent = component;
				this.GUIComponentValue = this.property.GetValue(this.parentObject);
				this.TempValue = this.GUIComponentValue;
			}

			// Token: 0x17001CE0 RID: 7392
			// (get) Token: 0x06009340 RID: 37696 RVA: 0x003CB840 File Offset: 0x003C9A40
			// (set) Token: 0x06009341 RID: 37697 RVA: 0x003CB954 File Offset: 0x003C9B54
			public object GUIComponentValue
			{
				get
				{
					if (this.GUIComponent == null)
					{
						return null;
					}
					GUITickBox tickBox = this.GUIComponent as GUITickBox;
					if (tickBox != null)
					{
						return tickBox.Selected;
					}
					GUITextBox textBox = this.GUIComponent as GUITextBox;
					if (textBox != null)
					{
						return textBox.Text;
					}
					GUIScrollBar scrollBar = this.GUIComponent as GUIScrollBar;
					if (scrollBar != null)
					{
						if (this.property.PropertyType == typeof(int))
						{
							return (int)MathF.Floor(scrollBar.BarScrollValue);
						}
						return scrollBar.BarScrollValue;
					}
					else
					{
						GUIRadioButtonGroup radioButtonGroup = this.GUIComponent as GUIRadioButtonGroup;
						if (radioButtonGroup != null)
						{
							return radioButtonGroup.Selected;
						}
						GUIDropDown dropdown = this.GUIComponent as GUIDropDown;
						if (dropdown != null)
						{
							return dropdown.SelectedData;
						}
						GUINumberInput numInput = this.GUIComponent as GUINumberInput;
						if (numInput != null)
						{
							if (numInput.InputType == NumberType.Int)
							{
								return numInput.IntValue;
							}
							return numInput.FloatValue;
						}
						else
						{
							IGUISelectionCarouselAccessor selectionCarousel = this.GUIComponent as IGUISelectionCarouselAccessor;
							if (selectionCarousel != null)
							{
								return selectionCarousel.GetSelectedElement();
							}
							return null;
						}
					}
				}
				set
				{
					if (this.GUIComponent == null)
					{
						return;
					}
					GUITickBox tickBox = this.GUIComponent as GUITickBox;
					if (tickBox != null)
					{
						tickBox.Selected = (bool)value;
						return;
					}
					GUITextBox textBox = this.GUIComponent as GUITextBox;
					if (textBox != null)
					{
						textBox.Text = (string)value;
						return;
					}
					GUIScrollBar scrollBar = this.GUIComponent as GUIScrollBar;
					if (scrollBar != null)
					{
						if (value is int)
						{
							int i = (int)value;
							scrollBar.BarScrollValue = (float)i;
						}
						else
						{
							scrollBar.BarScrollValue = (float)value;
						}
						GUIScrollBar.OnMovedHandler onMoved = scrollBar.OnMoved;
						if (onMoved == null)
						{
							return;
						}
						onMoved(scrollBar, scrollBar.BarScroll);
						return;
					}
					else
					{
						GUIRadioButtonGroup radioButtonGroup = this.GUIComponent as GUIRadioButtonGroup;
						if (radioButtonGroup != null)
						{
							radioButtonGroup.Selected = new int?((int)value);
							return;
						}
						GUIDropDown dropdown = this.GUIComponent as GUIDropDown;
						if (dropdown != null)
						{
							dropdown.SelectItem(value);
							return;
						}
						GUINumberInput numInput = this.GUIComponent as GUINumberInput;
						if (numInput == null)
						{
							IGUISelectionCarouselAccessor selectionCarousel = this.GUIComponent as IGUISelectionCarouselAccessor;
							if (selectionCarousel != null)
							{
								selectionCarousel.SelectElement(value);
							}
							return;
						}
						if (numInput.InputType == NumberType.Int)
						{
							numInput.IntValue = (int)value;
							return;
						}
						numInput.FloatValue = (float)value;
						return;
					}
				}
			}

			// Token: 0x17001CE1 RID: 7393
			// (get) Token: 0x06009342 RID: 37698 RVA: 0x003CBA7C File Offset: 0x003C9C7C
			public bool ChangedLocally
			{
				get
				{
					if (this.GUIComponent == null)
					{
						return false;
					}
					GUIDropDown dropDown = this.GUIComponent as GUIDropDown;
					return (dropDown == null || dropDown.SelectedIndex != -1) && !this.PropEquals(this.TempValue, this.GUIComponentValue);
				}
			}

			// Token: 0x17001CE2 RID: 7394
			// (get) Token: 0x06009343 RID: 37699 RVA: 0x003CBAC2 File Offset: 0x003C9CC2
			public Identifier Name
			{
				get
				{
					return this.property.Name.ToIdentifier();
				}
			}

			// Token: 0x17001CE3 RID: 7395
			// (get) Token: 0x06009344 RID: 37700 RVA: 0x003CBAD4 File Offset: 0x003C9CD4
			// (set) Token: 0x06009345 RID: 37701 RVA: 0x003CBAE7 File Offset: 0x003C9CE7
			public object Value
			{
				get
				{
					return this.property.GetValue(this.parentObject);
				}
				set
				{
					this.property.SetValue(this.parentObject, value);
				}
			}

			// Token: 0x06009346 RID: 37702 RVA: 0x003CBAFB File Offset: 0x003C9CFB
			public NetPropertyData(object parentObject, SerializableProperty property, string typeString)
			{
				this.property = property;
				this.typeString = typeString;
				this.parentObject = parentObject;
			}

			// Token: 0x06009347 RID: 37703 RVA: 0x003CBB18 File Offset: 0x003C9D18
			public bool PropEquals(object a, object b)
			{
				string a2 = this.typeString;
				if (!(a2 == "float"))
				{
					if (!(a2 == "int"))
					{
						if (!(a2 == "bool"))
						{
							if (!(a2 == "Enum"))
							{
								return a == b || string.Equals((a != null) ? a.ToString() : null, (b != null) ? b.ToString() : null, StringComparison.OrdinalIgnoreCase);
							}
							Enum ea = a as Enum;
							if (ea == null)
							{
								return false;
							}
							Enum eb = b as Enum;
							return eb != null && ea.Equals(eb);
						}
						else
						{
							if (!(a is bool))
							{
								return false;
							}
							bool ba = (bool)a;
							if (b is bool)
							{
								bool bb = (bool)b;
								return ba == bb;
							}
							return false;
						}
					}
					else
					{
						if (!(a is int))
						{
							return false;
						}
						int ia = (int)a;
						if (b is int)
						{
							int ib = (int)b;
							return ia == ib;
						}
						return false;
					}
				}
				else
				{
					if (!(a is float))
					{
						return false;
					}
					float fa = (float)a;
					if (b is float)
					{
						float fb = (float)b;
						return MathUtils.NearlyEqual(fa, fb, 0.0001f);
					}
					return false;
				}
			}

			// Token: 0x06009348 RID: 37704 RVA: 0x003CBC44 File Offset: 0x003C9E44
			public void Read(IReadMessage msg)
			{
				int oldPos = msg.BitPosition;
				uint size = msg.ReadVariableUInt32();
				string text = this.typeString;
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						if (!(text == "int"))
						{
							goto IL_2BF;
						}
						if (size == 4U)
						{
							this.property.SetValue(this.parentObject, msg.ReadInt32());
							return;
						}
						break;
					case 4:
					case 6:
					case 8:
						goto IL_2BF;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c != 'f')
							{
								goto IL_2BF;
							}
							if (!(text == "float"))
							{
								goto IL_2BF;
							}
							if (size == 4U)
							{
								this.property.SetValue(this.parentObject, msg.ReadSingle());
								return;
							}
						}
						else
						{
							if (!(text == "color"))
							{
								goto IL_2BF;
							}
							if (size == 4U)
							{
								byte r = msg.ReadByte();
								byte g = msg.ReadByte();
								byte b = msg.ReadByte();
								byte a = msg.ReadByte();
								this.property.SetValue(this.parentObject, new Microsoft.Xna.Framework.Color(r, g, b, a));
								return;
							}
						}
						break;
					}
					case 7:
						switch (text[6])
						{
						case '2':
							if (!(text == "vector2"))
							{
								goto IL_2BF;
							}
							if (size == 8U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector2(x, y));
								return;
							}
							break;
						case '3':
							if (!(text == "vector3"))
							{
								goto IL_2BF;
							}
							if (size == 12U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								float z = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector3(x, y, z));
								return;
							}
							break;
						case '4':
							if (!(text == "vector4"))
							{
								goto IL_2BF;
							}
							if (size == 16U)
							{
								float x = msg.ReadSingle();
								float y = msg.ReadSingle();
								float z = msg.ReadSingle();
								float w = msg.ReadSingle();
								this.property.SetValue(this.parentObject, new Vector4(x, y, z, w));
								return;
							}
							break;
						default:
							goto IL_2BF;
						}
						break;
					case 9:
						if (!(text == "rectangle"))
						{
							goto IL_2BF;
						}
						if (size == 16U)
						{
							int ix = msg.ReadInt32();
							int iy = msg.ReadInt32();
							int width = msg.ReadInt32();
							int height = msg.ReadInt32();
							this.property.SetValue(this.parentObject, new Rectangle(ix, iy, width, height));
							return;
						}
						break;
					default:
						goto IL_2BF;
					}
					msg.BitPosition += (int)(8U * size);
					return;
				}
				IL_2BF:
				msg.BitPosition = oldPos;
				string incVal = msg.ReadString();
				this.property.TrySetValue(this.parentObject, incVal);
			}

			// Token: 0x06009349 RID: 37705 RVA: 0x003CBF44 File Offset: 0x003CA144
			public void Write(IWriteMessage msg, object overrideValue = null)
			{
				if (overrideValue == null)
				{
					overrideValue = this.Value;
				}
				string text = this.typeString;
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						if (text == "int")
						{
							msg.WriteVariableUInt32(4U);
							msg.WriteInt32((int)overrideValue);
							return;
						}
						break;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 'f')
							{
								if (text == "float")
								{
									msg.WriteVariableUInt32(4U);
									msg.WriteSingle((float)overrideValue);
									return;
								}
							}
						}
						else if (text == "color")
						{
							msg.WriteVariableUInt32(4U);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).R);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).G);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).B);
							msg.WriteByte(((Microsoft.Xna.Framework.Color)overrideValue).A);
							return;
						}
						break;
					}
					case 7:
						switch (text[6])
						{
						case '2':
							if (text == "vector2")
							{
								msg.WriteVariableUInt32(8U);
								msg.WriteSingle(((Vector2)overrideValue).X);
								msg.WriteSingle(((Vector2)overrideValue).Y);
								return;
							}
							break;
						case '3':
							if (text == "vector3")
							{
								msg.WriteVariableUInt32(12U);
								msg.WriteSingle(((Vector3)overrideValue).X);
								msg.WriteSingle(((Vector3)overrideValue).Y);
								msg.WriteSingle(((Vector3)overrideValue).Z);
								return;
							}
							break;
						case '4':
							if (text == "vector4")
							{
								msg.WriteVariableUInt32(16U);
								msg.WriteSingle(((Vector4)overrideValue).X);
								msg.WriteSingle(((Vector4)overrideValue).Y);
								msg.WriteSingle(((Vector4)overrideValue).Z);
								msg.WriteSingle(((Vector4)overrideValue).W);
								return;
							}
							break;
						}
						break;
					case 9:
						if (text == "rectangle")
						{
							msg.WriteVariableUInt32(16U);
							msg.WriteInt32(((Rectangle)overrideValue).X);
							msg.WriteInt32(((Rectangle)overrideValue).Y);
							msg.WriteInt32(((Rectangle)overrideValue).Width);
							msg.WriteInt32(((Rectangle)overrideValue).Height);
							return;
						}
						break;
					}
				}
				string strVal = overrideValue.ToString();
				msg.WriteString(strVal);
			}

			// Token: 0x04005E52 RID: 24146
			public GUIComponent GUIComponent;

			// Token: 0x04005E53 RID: 24147
			public object TempValue;

			// Token: 0x04005E54 RID: 24148
			private readonly SerializableProperty property;

			// Token: 0x04005E55 RID: 24149
			private readonly string typeString;

			// Token: 0x04005E56 RID: 24150
			private readonly object parentObject;
		}

		// Token: 0x02001225 RID: 4645
		private enum SettingsTab
		{
			// Token: 0x04005E58 RID: 24152
			ServerIdentity,
			// Token: 0x04005E59 RID: 24153
			General,
			// Token: 0x04005E5A RID: 24154
			Antigriefing,
			// Token: 0x04005E5B RID: 24155
			Banlist
		}

		// Token: 0x02001226 RID: 4646
		[Flags]
		public enum NetFlags : byte
		{
			// Token: 0x04005E5D RID: 24157
			None = 0,
			// Token: 0x04005E5E RID: 24158
			Properties = 4,
			// Token: 0x04005E5F RID: 24159
			Misc = 8,
			// Token: 0x04005E60 RID: 24160
			LevelSeed = 16,
			// Token: 0x04005E61 RID: 24161
			HiddenSubs = 32
		}

		// Token: 0x02001227 RID: 4647
		public class SavedClientPermission
		{
			// Token: 0x0600934A RID: 37706 RVA: 0x003CC1EE File Offset: 0x003CA3EE
			public SavedClientPermission(string name, Either<Address, AccountId> addressOrAccountId, ClientPermissions permissions, IEnumerable<DebugConsole.Command> permittedCommands)
			{
				this.Name = name;
				this.AddressOrAccountId = addressOrAccountId;
				this.Permissions = permissions;
				this.PermittedCommands = permittedCommands.ToImmutableHashSet<DebugConsole.Command>();
			}

			// Token: 0x04005E62 RID: 24162
			public readonly Either<Address, AccountId> AddressOrAccountId;

			// Token: 0x04005E63 RID: 24163
			public readonly string Name;

			// Token: 0x04005E64 RID: 24164
			public readonly ImmutableHashSet<DebugConsole.Command> PermittedCommands;

			// Token: 0x04005E65 RID: 24165
			public readonly ClientPermissions Permissions;
		}
	}
}
