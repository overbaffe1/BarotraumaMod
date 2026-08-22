using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000101 RID: 257
	internal class MultiPlayerCampaignSetupUI : CampaignSetupUI
	{
		// Token: 0x170009CC RID: 2508
		// (get) Token: 0x06002424 RID: 9252 RVA: 0x0016AD58 File Offset: 0x00168F58
		public bool LoadGameMenuVisible
		{
			get
			{
				GUIComponent loadGameContainer = this.loadGameContainer;
				return loadGameContainer != null && loadGameContainer.Visible;
			}
		}

		// Token: 0x06002425 RID: 9253 RVA: 0x0016AD78 File Offset: 0x00168F78
		public MultiPlayerCampaignSetupUI(GUIComponent newGameContainer, GUIComponent loadGameContainer, List<CampaignMode.SaveInfo> saveFiles = null) : base(newGameContainer, loadGameContainer)
		{
			GUILayoutGroup verticalLayout = new GUILayoutGroup(new RectTransform(Vector2.One, newGameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.025f
			};
			GUILayoutGroup nameSeedLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup campaignSettingLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.6f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0f
			};
			GUITextBlock saveLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.03f), nameSeedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, GUI.IntScale(24f))
			}, TextManager.Get("SaveName"), null, null, Alignment.CenterLeft, false, "", null);
			GUITextBox guitextBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), saveLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null, false, true);
			GUITextBox guitextBox2 = guitextBox;
			Func<string, string> textFilterFunction;
			if ((textFilterFunction = MultiPlayerCampaignSetupUI.<>O.<0>__RemoveInvalidFileNameChars) == null)
			{
				textFilterFunction = (MultiPlayerCampaignSetupUI.<>O.<0>__RemoveInvalidFileNameChars = new Func<string, string>(ToolBox.RemoveInvalidFileNameChars));
			}
			guitextBox2.textFilterFunction = textFilterFunction;
			this.saveNameBox = guitextBox;
			saveLabel.InheritTotalChildrenMinHeight();
			GUITextBlock seedLabel = new GUITextBlock(new RectTransform(new Vector2(1f, 0.03f), nameSeedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, GUI.IntScale(24f))
			}, TextManager.Get("MapSeed"), null, null, Alignment.CenterLeft, false, "", null);
			this.seedBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), seedLabel.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), ToolBox.RandomSeed(8), null, null, Alignment.Left, false, "", null, false, true);
			seedLabel.InheritTotalChildrenMinHeight();
			nameSeedLayout.InheritTotalChildrenMinHeight();
			this.campaignSettingElements = CampaignSetupUI.CreateCampaignSettingList(campaignSettingLayout, CampaignSettings.Empty, false);
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(int.MaxValue, GUI.IntScale(30f))
			}, true, Anchor.BottomRight);
			this.prevInitialMoney = 8000;
			RectTransform rectT = new RectTransform(new Vector2(0.6f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont smallFont = GUIStyle.SmallFont;
			base.InitialMoneyText = new GUITextBlock(rectT, text, new Color?(GUIStyle.Green), smallFont, Alignment.CenterRight, false, "", null)
			{
				TextGetter = delegate()
				{
					int defaultInitialMoney = 8000;
					int initialMoney = defaultInitialMoney;
					XAttribute attribute;
					if (CampaignModePresets.TryGetAttribute("StartingBalanceAmount".ToIdentifier(), this.campaignSettingElements.StartingFunds.GetValue().ToIdentifier<StartingBalanceAmountOption>(), out attribute))
					{
						initialMoney = attribute.GetAttributeInt(defaultInitialMoney);
					}
					if (this.prevInitialMoney != initialMoney)
					{
						GameMain.NetLobbyScreen.RefreshEnabledElements();
						this.prevInitialMoney = initialMoney;
					}
					if (GameMain.NetLobbyScreen.SelectedSub != null)
					{
						initialMoney -= GameMain.NetLobbyScreen.SelectedSub.Price;
					}
					initialMoney = Math.Max(initialMoney, 0);
					return TextManager.GetWithVariable("campaignstartingmoney", "[money]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", initialMoney), FormatCapitals.No);
				}
			};
			verticalLayout.Recalculate();
			this.CreateLoadMenu(saveFiles);
		}

		// Token: 0x06002426 RID: 9254 RVA: 0x0016B198 File Offset: 0x00169398
		public bool StartGameClicked(GUIButton button, object userdata)
		{
			if (string.IsNullOrWhiteSpace(this.saveNameBox.Text))
			{
				this.saveNameBox.Flash(new Color?(GUIStyle.Red), 5f, false, false, null);
				this.saveNameBox.Pulsate(Vector2.One, Vector2.One * 1.2f, 2f);
				GUIComponent newGameContainer = this.newGameContainer;
				if (newGameContainer != null)
				{
					newGameContainer.Flash(new Color?(GUIStyle.Red), 0.5f, false, false, null);
				}
				return false;
			}
			SubmarineInfo selectedSub = null;
			if (GameMain.NetLobbyScreen.SelectedSub == null)
			{
				return false;
			}
			selectedSub = GameMain.NetLobbyScreen.SelectedSub;
			if (selectedSub.SubmarineClass == SubmarineClass.Undefined)
			{
				new GUIMessageBox(TextManager.Get("error"), TextManager.Get("undefinedsubmarineselected"), null, null, GUIMessageBox.Type.Default);
				return false;
			}
			if (string.IsNullOrEmpty(selectedSub.MD5Hash.StringRepresentation))
			{
				new GUIMessageBox(TextManager.Get("error"), TextManager.Get("nohashsubmarineselected"), null, null, GUIMessageBox.Type.Default);
				return false;
			}
			string savePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Multiplayer, this.saveNameBox.Text);
			bool hasRequiredContentPackages = selectedSub.RequiredContentPackagesInstalled;
			CampaignSettings settings = this.campaignSettingElements.CreateSettings();
			if (selectedSub.HasTag(SubmarineTag.Shuttle) || !hasRequiredContentPackages)
			{
				if (!hasRequiredContentPackages)
				{
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ContentPackageMismatch"), TextManager.GetWithVariable("ContentPackageMismatchWarning", "[requiredcontentpackages]", string.Join(", ", selectedSub.RequiredContentPackages), FormatCapitals.No), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
					GUIButton guibutton = msgBox.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object obj)
					{
						if (GUIMessageBox.MessageBoxes.Count == 0)
						{
							Action<SubmarineInfo, string, string, CampaignSettings> startNewGame2 = this.StartNewGame;
							if (startNewGame2 != null)
							{
								startNewGame2(selectedSub, savePath, this.seedBox.Text, settings);
							}
							CoroutineManager.StartCoroutine(this.WaitForCampaignSetup(), "WaitForCampaignSetup");
						}
						return true;
					}));
					msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
				}
				if (selectedSub.HasTag(SubmarineTag.Shuttle))
				{
					GUIMessageBox msgBox2 = new GUIMessageBox(TextManager.Get("ShuttleSelected"), TextManager.Get("ShuttleWarning"), new LocalizedString[]
					{
						TextManager.Get("Yes"),
						TextManager.Get("No")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					msgBox2.Buttons[0].OnClicked = delegate(GUIButton button, object obj)
					{
						Action<SubmarineInfo, string, string, CampaignSettings> startNewGame2 = this.StartNewGame;
						if (startNewGame2 != null)
						{
							startNewGame2(selectedSub, savePath, this.seedBox.Text, settings);
						}
						CoroutineManager.StartCoroutine(this.WaitForCampaignSetup(), "WaitForCampaignSetup");
						return true;
					};
					GUIButton guibutton2 = msgBox2.Buttons[0];
					guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox2.Close));
					msgBox2.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox2.Close);
					return false;
				}
			}
			else
			{
				Action<SubmarineInfo, string, string, CampaignSettings> startNewGame = this.StartNewGame;
				if (startNewGame != null)
				{
					startNewGame(selectedSub, savePath, this.seedBox.Text, settings);
				}
				CoroutineManager.StartCoroutine(this.WaitForCampaignSetup(), "WaitForCampaignSetup");
			}
			return true;
		}

		// Token: 0x06002427 RID: 9255 RVA: 0x0016B557 File Offset: 0x00169757
		private IEnumerable<CoroutineStatus> WaitForCampaignSetup()
		{
			return new MultiPlayerCampaignSetupUI.<WaitForCampaignSetup>d__8(-2);
		}

		// Token: 0x06002428 RID: 9256 RVA: 0x0016B560 File Offset: 0x00169760
		public override void CreateLoadMenu(IEnumerable<CampaignMode.SaveInfo> saveFiles = null)
		{
			List<CampaignMode.SaveInfo> prevSaveFiles = this.prevSaveFiles;
			if (prevSaveFiles != null)
			{
				prevSaveFiles.Clear();
			}
			this.prevSaveFiles = null;
			this.loadGameContainer.ClearChildren();
			if (saveFiles == null)
			{
				saveFiles = SaveUtil.GetSaveFiles(SaveUtil.SaveType.Multiplayer, true, true);
			}
			GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.85f), this.loadGameContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.03f
			};
			base.CreateSaveFilteringHeader(leftColumn);
			this.saveList = new GUIListBox(new RectTransform(Vector2.One, leftColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = new GUIListBox.OnSelectedHandler(this.SelectSaveFile)
			};
			foreach (CampaignMode.SaveInfo saveInfo in saveFiles)
			{
				base.CreateSaveElement(saveInfo);
			}
			base.SortSaveList(CampaignSetupUI.SaveSortingType.LastPlayedDescending);
			this.loadGameButton = new GUIButton(new RectTransform(new Vector2(0.45f, 0.12f), this.loadGameContainer.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadButton"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object obj)
				{
					MultiPlayerCampaignSetupUI.<>c__DisplayClass9_0 CS$<>8__locals1 = new MultiPlayerCampaignSetupUI.<>c__DisplayClass9_0();
					CS$<>8__locals1.<>4__this = this;
					object selectedData = this.saveList.SelectedData;
					if (!(selectedData is CampaignMode.SaveInfo))
					{
						return false;
					}
					CS$<>8__locals1.saveInfo = (CampaignMode.SaveInfo)selectedData;
					if (string.IsNullOrWhiteSpace(CS$<>8__locals1.saveInfo.FilePath))
					{
						return false;
					}
					if (CS$<>8__locals1.saveInfo.RespawnMode != RespawnMode.None)
					{
						RespawnMode respawnMode = CS$<>8__locals1.saveInfo.RespawnMode;
						NetworkMember networkMember = GameMain.NetworkMember;
						RespawnMode? respawnMode2;
						if (networkMember == null)
						{
							respawnMode2 = null;
						}
						else
						{
							ServerSettings serverSettings = networkMember.ServerSettings;
							respawnMode2 = ((serverSettings != null) ? new RespawnMode?(serverSettings.RespawnMode) : null);
						}
						RespawnMode? respawnMode3 = respawnMode2;
						if (!(respawnMode == respawnMode3.GetValueOrDefault() & respawnMode3 != null))
						{
							MultiPlayerCampaignSetupUI.<>c__DisplayClass9_1 CS$<>8__locals2 = new MultiPlayerCampaignSetupUI.<>c__DisplayClass9_1();
							CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
							MultiPlayerCampaignSetupUI.<>c__DisplayClass9_1 CS$<>8__locals3 = CS$<>8__locals2;
							RichString headerText = TextManager.Get("Warning");
							string tag = "RespawnModeMismatch";
							ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
							int num = 0;
							string item = "[currentrespawnmode]";
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
							defaultInterpolatedStringHandler.AppendLiteral("respawnmode.");
							NetworkMember networkMember2 = GameMain.NetworkMember;
							RespawnMode? value;
							if (networkMember2 == null)
							{
								value = null;
							}
							else
							{
								ServerSettings serverSettings2 = networkMember2.ServerSettings;
								value = ((serverSettings2 != null) ? new RespawnMode?(serverSettings2.RespawnMode) : null);
							}
							defaultInterpolatedStringHandler.AppendFormatted<RespawnMode?>(value);
							array[num] = new ValueTuple<string, LocalizedString>(item, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
							int num2 = 1;
							string item2 = "[savedrespawnmode]";
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("respawnmode.");
							defaultInterpolatedStringHandler2.AppendFormatted<RespawnMode>(CS$<>8__locals2.CS$<>8__locals1.saveInfo.RespawnMode);
							array[num2] = new ValueTuple<string, LocalizedString>(item2, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
							CS$<>8__locals3.msgBox = new GUIMessageBox(headerText, TextManager.GetWithVariables(tag, array), new LocalizedString[]
							{
								TextManager.Get("RespawnModeMismatch.GoBack"),
								TextManager.Get("RespawnModeMismatch.LoadAnyway")
							}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
							CS$<>8__locals2.msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object obj)
							{
								CS$<>8__locals2.msgBox.Close();
								return true;
							};
							CS$<>8__locals2.msgBox.Buttons[1].OnClicked = delegate(GUIButton button, object obj)
							{
								CS$<>8__locals2.msgBox.Close();
								CS$<>8__locals2.CS$<>8__locals1.<CreateLoadMenu>g__LoadSaveGame|1();
								return true;
							};
							return false;
						}
					}
					CS$<>8__locals1.<CreateLoadMenu>g__LoadSaveGame|1();
					return true;
				},
				Enabled = false
			};
			GUILayoutGroup leftButtonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.45f, 0.15f), this.loadGameContainer.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			this.rollbackSaveButton = new GUIButton(new RectTransform(new Vector2(1f, 0.5f), leftButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("rollbackbutton"), Alignment.Center, "GUIButtonSmallFreeScale", null)
			{
				Visible = false,
				ToolTip = TextManager.Get("backuptooltip"),
				OnClicked = new GUIButton.OnClickedHandler(this.ViewBackupSaveMenu)
			};
			this.deleteMpSaveButton = new GUIButton(new RectTransform(new Vector2(1f, 0.5f), leftButtonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Delete"), Alignment.Center, "GUIButtonSmallFreeScale", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(base.DeleteSave),
				Visible = false
			};
		}

		// Token: 0x06002429 RID: 9257 RVA: 0x0016B87C File Offset: 0x00169A7C
		private bool ViewBackupSaveMenu(GUIButton button, object obj)
		{
			if (!(obj is CampaignMode.SaveInfo))
			{
				return false;
			}
			CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)obj;
			if (string.IsNullOrWhiteSpace(saveInfo.FilePath))
			{
				return false;
			}
			if (GameMain.Client.IsServerOwner)
			{
				base.CreateBackupMenu(SaveUtil.GetIndexData(saveInfo.FilePath), delegate(SaveUtil.BackupIndexData index)
				{
					this.LoadGame(saveInfo.FilePath, Option.Some<uint>(index.Index));
				});
			}
			else
			{
				this.RequestBackupIndexData(saveInfo.FilePath);
			}
			return true;
		}

		// Token: 0x0600242A RID: 9258 RVA: 0x0016B90C File Offset: 0x00169B0C
		private void RequestBackupIndexData(string savePath)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			GUI.SetCursorWaiting(10, null);
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("CampaignStartingPleaseWait"), TextManager.Get("CampaignStarting"), new LocalizedString[]
			{
				TextManager.Get("Cancel")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				UserData = "PleaseWaitPopup"
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton btn, object obj)
			{
				GUI.ClearCursorWait();
				return true;
			};
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.REQUEST_BACKUP_INDICES);
			msg.WriteString(savePath);
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
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x0600242B RID: 9259 RVA: 0x0016B9F0 File Offset: 0x00169BF0
		public void OnBackupIndicesReceived(IReadMessage message)
		{
			GUI.ClearCursorWait();
			foreach (GUIComponent component in GUIMessageBox.MessageBoxes.Where(delegate(GUIComponent mb)
			{
				string text = mb.UserData as string;
				return text != null && text == "PleaseWaitPopup";
			}).ToArray<GUIComponent>())
			{
				GUIMessageBox msgBox = component as GUIMessageBox;
				if (msgBox != null)
				{
					msgBox.Close();
				}
			}
			string path = message.ReadString();
			NetCollection<SaveUtil.BackupIndexData> indexData = INetSerializableStruct.Read<NetCollection<SaveUtil.BackupIndexData>>(message);
			base.CreateBackupMenu(indexData, delegate(SaveUtil.BackupIndexData selectedIndex)
			{
				CampaignSetupUI.LoadGameDelegate loadGame = this.LoadGame;
				if (loadGame == null)
				{
					return;
				}
				loadGame(path, Option.Some<uint>(selectedIndex.Index));
			});
		}

		// Token: 0x0600242C RID: 9260 RVA: 0x0016BA90 File Offset: 0x00169C90
		private bool SelectSaveFile(GUIComponent component, object obj)
		{
			if (obj is CampaignMode.SaveInfo)
			{
				CampaignMode.SaveInfo saveInfo = (CampaignMode.SaveInfo)obj;
				string fileName = saveInfo.FilePath;
				this.loadGameButton.Enabled = true;
				this.rollbackSaveButton.Visible = true;
				this.deleteMpSaveButton.Visible = (this.deleteMpSaveButton.Enabled = GameMain.Client.IsServerOwner);
				GUIComponent guicomponent = this.rollbackSaveButton;
				GUIComponent guicomponent2 = this.deleteMpSaveButton;
				GameSession gameSession = GameMain.GameSession;
				guicomponent.Enabled = (guicomponent2.Enabled = (((gameSession != null) ? gameSession.DataPath.LoadPath : null) != fileName));
				if (this.deleteMpSaveButton.Visible)
				{
					this.deleteMpSaveButton.UserData = saveInfo;
				}
				if (this.rollbackSaveButton.Visible)
				{
					this.rollbackSaveButton.UserData = saveInfo;
				}
				return true;
			}
			return true;
		}

		// Token: 0x04001207 RID: 4615
		private GUIButton rollbackSaveButton;

		// Token: 0x04001208 RID: 4616
		private GUIButton deleteMpSaveButton;

		// Token: 0x04001209 RID: 4617
		private int prevInitialMoney;

		// Token: 0x0400120A RID: 4618
		private CampaignSetupUI.CampaignSettingElements campaignSettingElements;

		// Token: 0x0400120B RID: 4619
		private const string PleaseWaitUserData = "PleaseWaitPopup";

		// Token: 0x02000C06 RID: 3078
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040049B1 RID: 18865
			public static Func<string, string> <0>__RemoveInvalidFileNameChars;
		}
	}
}
