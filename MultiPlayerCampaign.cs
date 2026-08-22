using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000069 RID: 105
	internal class MultiPlayerCampaign : CampaignMode
	{
		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06000F26 RID: 3878 RVA: 0x00090060 File Offset: 0x0008E260
		public override bool Paused
		{
			get
			{
				return base.ForceMapUI || CoroutineManager.IsCoroutineRunning("LevelTransition");
			}
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06000F27 RID: 3879 RVA: 0x00090076 File Offset: 0x0008E276
		// (set) Token: 0x06000F28 RID: 3880 RVA: 0x0009007E File Offset: 0x0008E27E
		public ushort PendingSaveID
		{
			get
			{
				return this.pendingSaveID;
			}
			set
			{
				this.pendingSaveID = value;
				if (this.pendingSaveID == 0)
				{
					this.pendingSaveID += 1;
				}
			}
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06000F29 RID: 3881 RVA: 0x0009009E File Offset: 0x0008E29E
		public Wallet PersonalWallet
		{
			get
			{
				Character controlled = Character.Controlled;
				return ((controlled != null) ? controlled.Wallet : null) ?? Wallet.Invalid;
			}
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000F2A RID: 3882 RVA: 0x000900BA File Offset: 0x0008E2BA
		public override Wallet Wallet
		{
			get
			{
				return this.GetWallet(null);
			}
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x000900C3 File Offset: 0x0008E2C3
		public override int GetBalance(Client client = null)
		{
			if (!CampaignMode.AllowedToManageWallets())
			{
				return this.PersonalWallet.Balance;
			}
			return this.PersonalWallet.Balance + this.Bank.Balance;
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x000900EF File Offset: 0x0008E2EF
		public override Wallet GetWallet(Client client = null)
		{
			return this.PersonalWallet;
		}

		// Token: 0x06000F2D RID: 3885 RVA: 0x000900F8 File Offset: 0x0008E2F8
		public static void StartCampaignSetup(List<CampaignMode.SaveInfo> saveFiles)
		{
			GUIFrame parent = GameMain.NetLobbyScreen.CampaignSetupFrame;
			parent.ClearChildren();
			parent.Visible = true;
			GameMain.NetLobbyScreen.HighlightMode(GameMain.NetLobbyScreen.ModeList.Content.GetChildIndex(GameMain.NetLobbyScreen.ModeList.Content.GetChildByUserData(GameModePreset.MultiPlayerCampaign)));
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.07f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUIFrame campaignContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), layout.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null)
			{
				CanBeFocused = false
			};
			GUIFrame newCampaignContainer = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.95f), campaignContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame loadCampaignContainer = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.95f), campaignContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GameMain.NetLobbyScreen.CampaignSetupUI = new MultiPlayerCampaignSetupUI(newCampaignContainer, loadCampaignContainer, saveFiles);
			GUIButton newCampaignButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("NewCampaign"), Alignment.Center, "GUITabButton", null)
			{
				Selected = true
			};
			GUIButton loadCampaignButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadCampaign"), Alignment.Center, "GUITabButton", null);
			newCampaignButton.OnClicked = delegate(GUIButton btn, object obj)
			{
				newCampaignButton.Selected = true;
				loadCampaignButton.Selected = false;
				newCampaignContainer.Visible = true;
				loadCampaignContainer.Visible = false;
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					netLobbyScreen.RefreshStartButtonVisibility();
				}
				return true;
			};
			loadCampaignButton.OnClicked = delegate(GUIButton btn, object obj)
			{
				newCampaignButton.Selected = false;
				loadCampaignButton.Selected = true;
				newCampaignContainer.Visible = false;
				loadCampaignContainer.Visible = true;
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					netLobbyScreen.RefreshStartButtonVisibility();
				}
				return true;
			};
			loadCampaignContainer.Visible = false;
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				newCampaignButton.TextBlock,
				loadCampaignButton.TextBlock
			});
			GameMain.NetLobbyScreen.CampaignSetupUI.StartNewGame = delegate(SubmarineInfo sub, string saveName, string mapSeed, CampaignSettings settings)
			{
				GameMain.NetLobbyScreen.SetAFKSelected(false);
				GameMain.Client.SetupNewCampaign(sub, saveName, mapSeed, settings);
			};
			GameMain.NetLobbyScreen.CampaignSetupUI.LoadGame = delegate(string filePath, Option<uint> backupIndex)
			{
				GameMain.NetLobbyScreen.SetAFKSelected(false);
				GameMain.Client.SetupLoadCampaign(filePath, backupIndex);
			};
		}

		// Token: 0x06000F2E RID: 3886 RVA: 0x0009049B File Offset: 0x0008E69B
		public override void HUDScaleChanged()
		{
			this.CreateButtons();
		}

		// Token: 0x06000F2F RID: 3887 RVA: 0x000904A4 File Offset: 0x0008E6A4
		private void CreateButtons()
		{
			this.endRoundButton = base.CreateEndRoundButton();
			this.endRoundButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				base.TryEndRoundWithFuelCheck(delegate
				{
					GameMain.Client.RequestStartRound(false);
				}, delegate
				{
					base.ShowCampaignUI = true;
					if (this.CampaignUI == null)
					{
						this.InitCampaignUI();
					}
					this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
				});
				return true;
			};
			int readyButtonWidth = (int)(GUI.Scale * 50f * (GUI.IsUltrawide ? 3f : 1f));
			int readyButtonHeight = (int)(GUI.Scale * 40f);
			int readyButtonCenter = readyButtonHeight / 2;
			int screenMiddle = GameMain.GraphicsWidth / 2;
			this.ReadyCheckButton = new GUIButton(HUDLayoutSettings.ToRectTransform(new Rectangle(screenMiddle + this.endRoundButton.Rect.Width / 2 + GUI.IntScale(16f), HUDLayoutSettings.ButtonAreaTop.Center.Y - readyButtonCenter, readyButtonWidth, readyButtonHeight), GUI.Canvas), Alignment.Center, "RepairBuyButton", null)
			{
				ToolTip = TextManager.Get("ReadyCheck.Tooltip"),
				OnClicked = delegate(GUIButton <p0>, object <p1>)
				{
					if (base.CrewManager != null && base.CrewManager.ActiveReadyCheck == null)
					{
						ReadyCheck.CreateReadyCheck();
					}
					return true;
				},
				UserData = "ReadyCheckButton"
			};
		}

		// Token: 0x06000F30 RID: 3888 RVA: 0x000905A4 File Offset: 0x0008E7A4
		public void InitCampaignUI()
		{
			this.campaignUIContainer = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), "InnerGlow", new Color?(Color.Black));
			CampaignUI campaignUI = new CampaignUI(this, this.campaignUIContainer);
			campaignUI.StartRound = delegate()
			{
				GameMain.NetLobbyScreen.SetAFKSelected(false);
				GameMain.Client.RequestStartRound(false);
			};
			this.CampaignUI = campaignUI;
		}

		// Token: 0x06000F31 RID: 3889 RVA: 0x0009062E File Offset: 0x0008E82E
		public override void Start()
		{
			base.Start();
			CoroutineManager.StartCoroutine(this.DoInitialCameraTransition(), "MultiplayerCampaign.DoInitialCameraTransition");
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x00090647 File Offset: 0x0008E847
		protected override void LoadInitialLevel()
		{
			throw new InvalidOperationException("");
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x00090653 File Offset: 0x0008E853
		private IEnumerable<CoroutineStatus> DoInitialCameraTransition()
		{
			MultiPlayerCampaign.<DoInitialCameraTransition>d__19 <DoInitialCameraTransition>d__ = new MultiPlayerCampaign.<DoInitialCameraTransition>d__19(-2);
			<DoInitialCameraTransition>d__.<>4__this = this;
			return <DoInitialCameraTransition>d__;
		}

		// Token: 0x06000F34 RID: 3892 RVA: 0x00090663 File Offset: 0x0008E863
		protected override IEnumerable<CoroutineStatus> DoLevelTransition(CampaignMode.TransitionType transitionType, LevelData newLevel, Submarine leavingSub, bool mirror)
		{
			return new MultiPlayerCampaign.<DoLevelTransition>d__20(-2);
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x0009066C File Offset: 0x0008E86C
		private IEnumerable<CoroutineStatus> DoLevelTransition()
		{
			MultiPlayerCampaign.<DoLevelTransition>d__21 <DoLevelTransition>d__ = new MultiPlayerCampaign.<DoLevelTransition>d__21(-2);
			<DoLevelTransition>d__.<>4__this = this;
			return <DoLevelTransition>d__;
		}

		// Token: 0x06000F36 RID: 3894 RVA: 0x0009067C File Offset: 0x0008E87C
		public override void Update(float deltaTime)
		{
			if (CoroutineManager.IsCoroutineRunning("LevelTransition") || Level.Loaded == null)
			{
				return;
			}
			if (base.ShowCampaignUI || base.ForceMapUI)
			{
				if (this.CampaignUI == null)
				{
					this.InitCampaignUI();
				}
				Character.DisableControls = true;
			}
			base.Update(deltaTime);
			SlideshowPlayer slideshowPlayer = base.SlideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.UpdateManually(deltaTime, false, true);
			}
			if (PlayerInput.SecondaryMouseButtonClicked() || PlayerInput.KeyHit(Keys.Escape))
			{
				base.ShowCampaignUI = false;
				GUIComponent visibleBox = GUIMessageBox.VisibleBox;
				RoundSummary roundSummary = ((visibleBox != null) ? visibleBox.UserData : null) as RoundSummary;
				if (roundSummary != null && roundSummary.ContinueButton != null && roundSummary.ContinueButton.Visible)
				{
					GUIMessageBox.MessageBoxes.Remove(GUIMessageBox.VisibleBox);
				}
			}
			if (!GUI.DisableHUD && !GUI.DisableUpperHUD)
			{
				this.endRoundButton.UpdateManually(deltaTime, false, true);
				GUIButton readyCheckButton = this.ReadyCheckButton;
				if (readyCheckButton != null)
				{
					readyCheckButton.UpdateManually(deltaTime, false, true);
				}
				if (CoroutineManager.IsCoroutineRunning("LevelTransition") || base.ForceMapUI)
				{
					return;
				}
			}
			if (Level.Loaded.Type == LevelData.LevelType.Outpost)
			{
				if (this.wasDocked)
				{
					IEnumerable<Submarine> connectedSubs = Submarine.MainSub.GetConnectedSubs();
					if (Level.Loaded.StartOutpost == null || !connectedSubs.Contains(Level.Loaded.StartOutpost))
					{
						base.ForceMapUI = true;
						if (this.CampaignUI == null)
						{
							this.InitCampaignUI();
						}
						this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
					}
				}
				else if (!Level.Loaded.IsEndBiome && !Submarine.MainSub.AtStartExit)
				{
					base.ForceMapUI = true;
					if (this.CampaignUI == null)
					{
						this.InitCampaignUI();
					}
					this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
				}
				if (this.CampaignUI == null)
				{
					this.InitCampaignUI();
					return;
				}
			}
			else
			{
				LevelData levelData;
				Submarine submarine;
				CampaignMode.TransitionType transitionType = base.GetAvailableTransition(out levelData, out submarine);
				if (transitionType == CampaignMode.TransitionType.None)
				{
					CampaignUI campaignUI = this.CampaignUI;
					if (campaignUI != null && campaignUI.SelectedTab == CampaignMode.InteractionType.Map)
					{
						base.ShowCampaignUI = false;
					}
				}
				HintManager.OnAvailableTransition(transitionType);
			}
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x0009085C File Offset: 0x0008EA5C
		public override void UpdateWhilePaused(float deltaTime)
		{
			SlideshowPlayer slideshowPlayer = base.SlideshowPlayer;
			if (slideshowPlayer == null)
			{
				return;
			}
			slideshowPlayer.UpdateManually(deltaTime, false, true);
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x00090874 File Offset: 0x0008EA74
		public override void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
			base.End(transitionType);
			base.ForceMapUI = (base.ShowCampaignUI = false);
			SlideshowPlayer slideshowPlayer = base.SlideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.Finish();
			}
			GUIMessageBox.MessageBoxes.ForEachMod(delegate(GUIComponent mb)
			{
				GUIMessageBox msgBox = mb as GUIMessageBox;
				if (msgBox != null)
				{
					if (!ReadyCheck.IsReadyCheck(mb))
					{
						Pair<string, ushort> pair = mb.UserData as Pair<string, ushort>;
						if (pair == null || !pair.First.Equals("conversationaction", StringComparison.OrdinalIgnoreCase))
						{
							return;
						}
					}
					msgBox.Close();
				}
			});
			if (transitionType == CampaignMode.TransitionType.End)
			{
				base.EndCampaign();
				return;
			}
			base.IsFirstRound = false;
			CoroutineManager.StartCoroutine(this.DoLevelTransition(), "LevelTransition");
		}

		// Token: 0x06000F39 RID: 3897 RVA: 0x000908F8 File Offset: 0x0008EAF8
		protected override void EndCampaignProjSpecific()
		{
			GUIComponent visibleBox = GUIMessageBox.VisibleBox;
			RoundSummary roundSummary = ((visibleBox != null) ? visibleBox.UserData : null) as RoundSummary;
			if (roundSummary != null)
			{
				GUIMessageBox.MessageBoxes.Remove(GUIMessageBox.VisibleBox);
			}
			GameMain.CampaignEndScreen.Select();
			GUI.DisableHUD = false;
			GameMain.CampaignEndScreen.OnFinished = delegate()
			{
				GameMain.NetLobbyScreen.Select();
				if (GameMain.NetLobbyScreen.QuitCampaignButton != null)
				{
					GameMain.NetLobbyScreen.QuitCampaignButton.Enabled = false;
				}
			};
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x00090968 File Offset: 0x0008EB68
		public void ClientWrite(IWriteMessage msg)
		{
			msg.WriteUInt16((this.map.CurrentLocationIndex == -1) ? ushort.MaxValue : ((ushort)this.map.CurrentLocationIndex));
			msg.WriteUInt16((this.map.SelectedLocationIndex == -1) ? ushort.MaxValue : ((ushort)this.map.SelectedLocationIndex));
			IEnumerable<int> selectedMissionIndices = this.map.GetSelectedMissionIndices();
			msg.WriteByte((byte)selectedMissionIndices.Count<int>());
			foreach (int selectedMissionIndex in selectedMissionIndices)
			{
				msg.WriteByte((byte)selectedMissionIndex);
			}
			msg.WriteBoolean(this.PurchasedHullRepairs);
			msg.WriteBoolean(this.PurchasedItemRepairs);
			msg.WriteBoolean(this.PurchasedLostShuttles);
			MultiPlayerCampaign.WriteItems(msg, this.CargoManager.ItemsInBuyCrate);
			MultiPlayerCampaign.WriteItems(msg, this.CargoManager.ItemsInSellFromSubCrate);
			MultiPlayerCampaign.WriteItems(msg, this.CargoManager.PurchasedItems);
			MultiPlayerCampaign.WriteItems(msg, this.CargoManager.SoldItems);
			msg.WriteUInt16((ushort)this.UpgradeManager.PurchasedUpgrades.Count);
			foreach (PurchasedUpgrade purchasedUpgrade in this.UpgradeManager.PurchasedUpgrades)
			{
				UpgradePrefab upgradePrefab;
				UpgradeCategory upgradeCategory;
				int num;
				purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out num);
				UpgradePrefab prefab = upgradePrefab;
				UpgradeCategory category = upgradeCategory;
				int level = num;
				msg.WriteIdentifier(prefab.Identifier);
				msg.WriteIdentifier(category.Identifier);
				msg.WriteByte((byte)level);
			}
			msg.WriteUInt16((ushort)this.UpgradeManager.PurchasedItemSwaps.Count);
			foreach (PurchasedItemSwap itemSwap in this.UpgradeManager.PurchasedItemSwaps)
			{
				msg.WriteUInt16(itemSwap.ItemToRemove.ID);
				ItemPrefab itemToInstall = itemSwap.ItemToInstall;
				msg.WriteIdentifier((itemToInstall != null) ? itemToInstall.Identifier : Identifier.Empty);
			}
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x00090B98 File Offset: 0x0008ED98
		public static void ClientRead(IReadMessage msg)
		{
			MultiPlayerCampaign.NetFlags requiredFlags = (MultiPlayerCampaign.NetFlags)msg.ReadUInt16();
			bool isFirstRound = msg.ReadBoolean();
			byte campaignID = msg.ReadByte();
			MultiPlayerCampaign.<>c__DisplayClass27_0 CS$<>8__locals1;
			CS$<>8__locals1.roundId = msg.ReadByte();
			CS$<>8__locals1.saveID = msg.ReadUInt16();
			string mapSeed = msg.ReadString();
			bool refreshCampaignUI = false;
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			CS$<>8__locals1.campaign = (gameMode as MultiPlayerCampaign);
			if (CS$<>8__locals1.campaign == null || campaignID != CS$<>8__locals1.campaign.CampaignID)
			{
				string savePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Multiplayer, "Save_Default");
				SubmarineInfo submarineInfo = null;
				Option.UnspecifiedNone none = Option.None;
				GameMain.GameSession = new GameSession(submarineInfo, none, CampaignDataPath.CreateRegular(savePath), GameModePreset.MultiPlayerCampaign, CampaignSettings.Empty, mapSeed, null);
				CS$<>8__locals1.campaign = (MultiPlayerCampaign)GameMain.GameSession.GameMode;
				CS$<>8__locals1.campaign.CampaignID = campaignID;
				GameMain.NetLobbyScreen.ToggleCampaignMode(true);
			}
			if (NetIdUtils.IdMoreRecent(CS$<>8__locals1.saveID, CS$<>8__locals1.campaign.PendingSaveID))
			{
				CS$<>8__locals1.campaign.PendingSaveID = CS$<>8__locals1.saveID;
			}
			CS$<>8__locals1.campaign.IsFirstRound = isFirstRound;
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.Misc))
			{
				DebugConsole.Log("Received campaign update (Misc), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id = msg.ReadUInt16();
				bool purchasedHullRepairs = msg.ReadBoolean();
				bool purchasedItemRepairs = msg.ReadBoolean();
				bool purchasedLostShuttles = msg.ReadBoolean();
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.Misc, id, false, true, ref CS$<>8__locals1))
				{
					refreshCampaignUI = (CS$<>8__locals1.campaign.PurchasedHullRepairs != purchasedHullRepairs || CS$<>8__locals1.campaign.PurchasedItemRepairs != purchasedItemRepairs || CS$<>8__locals1.campaign.PurchasedLostShuttles != purchasedLostShuttles);
					CS$<>8__locals1.campaign.PurchasedHullRepairs = purchasedHullRepairs;
					CS$<>8__locals1.campaign.PurchasedItemRepairs = purchasedItemRepairs;
					CS$<>8__locals1.campaign.PurchasedLostShuttles = purchasedLostShuttles;
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.MapAndMissions))
			{
				DebugConsole.Log("Received campaign update (MapAndMissions), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id2 = msg.ReadUInt16();
				bool forceMapUI = msg.ReadBoolean();
				bool allowDebugTeleport = msg.ReadBoolean();
				ushort currentLocIndex = msg.ReadUInt16();
				ushort selectedLocIndex = msg.ReadUInt16();
				byte missionCount = msg.ReadByte();
				List<ValueTuple<Identifier, byte>> availableMissions = new List<ValueTuple<Identifier, byte>>();
				for (int i = 0; i < (int)missionCount; i++)
				{
					Identifier missionIdentifier = msg.ReadIdentifier();
					byte connectionIndex = msg.ReadByte();
					availableMissions.Add(new ValueTuple<Identifier, byte>(missionIdentifier, connectionIndex));
				}
				byte selectedMissionCount = msg.ReadByte();
				List<int> selectedMissionIndices = new List<int>();
				for (int j = 0; j < (int)selectedMissionCount; j++)
				{
					selectedMissionIndices.Add((int)msg.ReadByte());
				}
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.MapAndMissions, id2, true, true, ref CS$<>8__locals1))
				{
					CS$<>8__locals1.campaign.ForceMapUI = forceMapUI;
					CS$<>8__locals1.campaign.Map.AllowDebugTeleport = allowDebugTeleport;
					CS$<>8__locals1.campaign.Map.SetLocation((currentLocIndex == ushort.MaxValue) ? -1 : ((int)currentLocIndex));
					CS$<>8__locals1.campaign.Map.SelectLocation((selectedLocIndex == ushort.MaxValue) ? -1 : ((int)selectedLocIndex));
					using (List<ValueTuple<Identifier, byte>>.Enumerator enumerator = availableMissions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ValueTuple<Identifier, byte> availableMission = enumerator.Current;
							MissionPrefab missionPrefab = MissionPrefab.Prefabs.Find((MissionPrefab mp) => mp.Identifier == availableMission.Item1);
							if (missionPrefab == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 1);
								defaultInterpolatedStringHandler.AppendLiteral("Error when receiving campaign data from the server: mission prefab \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(availableMission.Item1);
								defaultInterpolatedStringHandler.AppendLiteral("\" not found.");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
							}
							else if (availableMission.Item2 == 255)
							{
								CS$<>8__locals1.campaign.Map.CurrentLocation.UnlockMission(missionPrefab);
							}
							else if (availableMission.Item2 < 0 || (int)availableMission.Item2 >= CS$<>8__locals1.campaign.Map.CurrentLocation.Connections.Count)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(142, 4);
								defaultInterpolatedStringHandler2.AppendLiteral("Error when receiving campaign data from the server: connection index for mission \"");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(availableMission.Item1);
								defaultInterpolatedStringHandler2.AppendLiteral("\" out of range (index: ");
								defaultInterpolatedStringHandler2.AppendFormatted<byte>(availableMission.Item2);
								defaultInterpolatedStringHandler2.AppendLiteral(", current location: ");
								defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CS$<>8__locals1.campaign.Map.CurrentLocation.DisplayName);
								defaultInterpolatedStringHandler2.AppendLiteral(", connections: ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(CS$<>8__locals1.campaign.Map.CurrentLocation.Connections.Count);
								defaultInterpolatedStringHandler2.AppendLiteral(").");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							}
							else
							{
								LocationConnection connection = CS$<>8__locals1.campaign.Map.CurrentLocation.Connections[(int)availableMission.Item2];
								CS$<>8__locals1.campaign.Map.CurrentLocation.UnlockMission(missionPrefab, connection);
							}
						}
					}
					CS$<>8__locals1.campaign.Map.SelectMission(selectedMissionIndices);
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, true, ref CS$<>8__locals1);
				}
				else
				{
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, false, ref CS$<>8__locals1);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.SubList))
			{
				DebugConsole.Log("Received campaign update (SubList), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id3 = msg.ReadUInt16();
				ushort ownedSubCount = msg.ReadUInt16();
				List<ushort> ownedSubIndices = new List<ushort>();
				for (int k = 0; k < (int)ownedSubCount; k++)
				{
					ownedSubIndices.Add(msg.ReadUInt16());
				}
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.SubList, id3, false, true, ref CS$<>8__locals1))
				{
					foreach (ushort ownedSubIndex in ownedSubIndices)
					{
						if ((int)ownedSubIndex >= GameMain.Client.ServerSubmarines.Count)
						{
							string errorMsg;
							if (GameMain.Client.ServerSubmarines.None(null))
							{
								errorMsg = "Error in ClientRead. Owned submarine index was out of bounds (list of server submarines is empty).";
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(73, 3);
								defaultInterpolatedStringHandler3.AppendLiteral("Error in ");
								defaultInterpolatedStringHandler3.AppendFormatted("ClientRead");
								defaultInterpolatedStringHandler3.AppendLiteral(". Owned submarine index was out of bounds. Index: ");
								defaultInterpolatedStringHandler3.AppendFormatted<ushort>(ownedSubIndex);
								defaultInterpolatedStringHandler3.AppendLiteral(", submarines: ");
								defaultInterpolatedStringHandler3.AppendFormatted(string.Join(", ", from s in GameMain.Client.ServerSubmarines
								select s.Name));
								errorMsg = defaultInterpolatedStringHandler3.ToStringAndClear();
							}
							DebugConsole.ThrowError(errorMsg, null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("MultiPlayerCampaign.ClientRead.OwnerSubIndexOutOfBounds" + ownedSubIndex.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						}
						else
						{
							SubmarineInfo sub = GameMain.Client.ServerSubmarines[(int)ownedSubIndex];
							if (GameMain.NetLobbyScreen.CheckIfCampaignSubMatches(sub, NetLobbyScreen.SubmarineDeliveryData.Owned) && GameMain.GameSession.OwnedSubmarines.None((SubmarineInfo s) => s.Name == sub.Name))
							{
								GameMain.GameSession.OwnedSubmarines.Add(sub);
							}
						}
					}
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.UpgradeManager))
			{
				DebugConsole.Log("Received campaign update (UpgradeManager), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id4 = msg.ReadUInt16();
				ushort pendingUpgradeCount = msg.ReadUInt16();
				List<PurchasedUpgrade> pendingUpgrades = new List<PurchasedUpgrade>();
				for (int l = 0; l < (int)pendingUpgradeCount; l++)
				{
					Identifier upgradeIdentifier = msg.ReadIdentifier();
					UpgradePrefab prefab = UpgradePrefab.Find(upgradeIdentifier);
					Identifier categoryIdentifier = msg.ReadIdentifier();
					UpgradeCategory category = UpgradeCategory.Find(categoryIdentifier);
					int upgradeLevel = (int)msg.ReadByte();
					if (prefab != null && category != null)
					{
						pendingUpgrades.Add(new PurchasedUpgrade(prefab, category, upgradeLevel));
					}
				}
				ushort purchasedItemSwapCount = msg.ReadUInt16();
				List<PurchasedItemSwap> purchasedItemSwaps = new List<PurchasedItemSwap>();
				for (int m = 0; m < (int)purchasedItemSwapCount; m++)
				{
					ushort itemToRemoveID = msg.ReadUInt16();
					Identifier itemToInstallIdentifier = msg.ReadIdentifier();
					ItemPrefab itemToInstall = itemToInstallIdentifier.IsEmpty ? null : ItemPrefab.Find(string.Empty, itemToInstallIdentifier);
					Item itemToRemove = Entity.FindEntityByID(itemToRemoveID) as Item;
					if (itemToRemove != null)
					{
						purchasedItemSwaps.Add(new PurchasedItemSwap(itemToRemove, itemToInstall));
					}
				}
				if (!Submarine.Unloading)
				{
					Submarine mainSub = Submarine.MainSub;
					if ((mainSub == null || !mainSub.Loading) && MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.UpgradeManager, id4, true, true, ref CS$<>8__locals1))
					{
						UpgradeStore.WaitForServerUpdate = false;
						CS$<>8__locals1.campaign.UpgradeManager.SetPendingUpgrades(pendingUpgrades);
						CS$<>8__locals1.campaign.UpgradeManager.PurchasedUpgrades.Clear();
						foreach (PurchasedItemSwap purchasedItemSwap in purchasedItemSwaps)
						{
							if (purchasedItemSwap.ItemToInstall == null)
							{
								CS$<>8__locals1.campaign.UpgradeManager.CancelItemSwap(purchasedItemSwap.ItemToRemove, true, null);
							}
							else
							{
								CS$<>8__locals1.campaign.UpgradeManager.PurchaseItemSwap(purchasedItemSwap.ItemToRemove, purchasedItemSwap.ItemToInstall, true, null);
							}
						}
						using (List<Item>.Enumerator enumerator4 = Item.ItemList.ToList<Item>().GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								Item item = enumerator4.Current;
								if (item.PendingItemSwap != null && !purchasedItemSwaps.Any((PurchasedItemSwap it) => it.ItemToRemove == item))
								{
									item.PendingItemSwap = null;
								}
							}
						}
						CampaignUI campaignUI = CS$<>8__locals1.campaign.CampaignUI;
						if (campaignUI != null)
						{
							UpgradeStore upgradeStore = campaignUI.UpgradeStore;
							if (upgradeStore != null)
							{
								upgradeStore.RequestRefresh(false);
							}
						}
					}
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate))
			{
				DebugConsole.Log("Received campaign update (ItemsInBuyCrate), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id5 = msg.ReadUInt16();
				Dictionary<Identifier, List<PurchasedItem>> buyCrateItems = MultiPlayerCampaign.ReadPurchasedItems(msg, null);
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate, id5, true, true, ref CS$<>8__locals1))
				{
					CS$<>8__locals1.campaign.CargoManager.SetItemsInBuyCrate(buyCrateItems);
					CS$<>8__locals1.campaign.SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate, id5);
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, true, ref CS$<>8__locals1);
				}
				else
				{
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, false, ref CS$<>8__locals1);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.ItemsInSellFromSubCrate))
			{
				DebugConsole.Log("Received campaign update (ItemsInSellFromSubCrate), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id6 = msg.ReadUInt16();
				Dictionary<Identifier, List<PurchasedItem>> subSellCrateItems = MultiPlayerCampaign.ReadPurchasedItems(msg, null);
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.ItemsInSellFromSubCrate, id6, true, true, ref CS$<>8__locals1))
				{
					CS$<>8__locals1.campaign.CargoManager.SetItemsInSubSellCrate(subSellCrateItems);
					CS$<>8__locals1.campaign.SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.ItemsInSellFromSubCrate, id6);
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, true, ref CS$<>8__locals1);
				}
				else
				{
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, false, ref CS$<>8__locals1);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.PurchasedItems))
			{
				DebugConsole.Log("Received campaign update (PuchasedItems), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id7 = msg.ReadUInt16();
				Dictionary<Identifier, List<PurchasedItem>> purchasedItems = MultiPlayerCampaign.ReadPurchasedItems(msg, null);
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.PurchasedItems, id7, true, true, ref CS$<>8__locals1))
				{
					CS$<>8__locals1.campaign.CargoManager.SetPurchasedItems(purchasedItems);
					CS$<>8__locals1.campaign.SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.PurchasedItems, id7);
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, true, ref CS$<>8__locals1);
				}
				else
				{
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, false, ref CS$<>8__locals1);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.SoldItems))
			{
				DebugConsole.Log("Received campaign update (SoldItems), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id8 = msg.ReadUInt16();
				Dictionary<Identifier, List<SoldItem>> soldItems = MultiPlayerCampaign.ReadSoldItems(msg);
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.SoldItems, id8, true, true, ref CS$<>8__locals1))
				{
					CS$<>8__locals1.campaign.CargoManager.SetSoldItems(soldItems);
					CS$<>8__locals1.campaign.SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.SoldItems, id8);
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, true, ref CS$<>8__locals1);
				}
				else
				{
					MultiPlayerCampaign.<ClientRead>g__ReadStores|27_1(msg, false, ref CS$<>8__locals1);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.Reputation))
			{
				DebugConsole.Log("Received campaign update (Reputation), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id9 = msg.ReadUInt16();
				Dictionary<Identifier, float> factionReps = new Dictionary<Identifier, float>();
				byte factionsCount = msg.ReadByte();
				for (int n = 0; n < (int)factionsCount; n++)
				{
					factionReps.Add(msg.ReadIdentifier(), msg.ReadSingle());
				}
				if (MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.Reputation, id9, true, true, ref CS$<>8__locals1))
				{
					foreach (KeyValuePair<Identifier, float> keyValuePair in factionReps)
					{
						Identifier identifier2;
						float num;
						keyValuePair.Deconstruct(out identifier2, out num);
						Identifier identifier = identifier2;
						float rep = num;
						Faction faction = CS$<>8__locals1.campaign.Factions.FirstOrDefault((Faction f) => f.Prefab.Identifier == identifier);
						if (((faction != null) ? faction.Reputation : null) != null)
						{
							faction.Reputation.SetReputation(rep);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(55, 1);
							defaultInterpolatedStringHandler4.AppendLiteral("Received an update for a faction that doesn't exist \"");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(identifier);
							defaultInterpolatedStringHandler4.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
						}
					}
					MultiPlayerCampaign campaign = CS$<>8__locals1.campaign;
					if (campaign != null)
					{
						CampaignUI campaignUI2 = campaign.CampaignUI;
						if (campaignUI2 != null)
						{
							UpgradeStore upgradeStore2 = campaignUI2.UpgradeStore;
							if (upgradeStore2 != null)
							{
								upgradeStore2.RequestRefresh(false);
							}
						}
					}
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.CharacterInfo))
			{
				DebugConsole.Log("Received campaign update (CharacterInfo), round id: " + CS$<>8__locals1.roundId.ToString());
				ushort id10 = msg.ReadUInt16();
				bool hasCharacterData = msg.ReadBoolean();
				CharacterInfo myCharacterInfo = null;
				bool waitForModsDownloaded = Screen.Selected is ModDownloadScreen;
				if (hasCharacterData)
				{
					myCharacterInfo = CharacterInfo.ClientRead(CharacterPrefab.HumanSpeciesName, msg, !waitForModsDownloaded);
				}
				if (!waitForModsDownloaded && MultiPlayerCampaign.<ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags.CharacterInfo, id10, true, Screen.Selected != GameMain.NetLobbyScreen, ref CS$<>8__locals1))
				{
					if (myCharacterInfo != null)
					{
						GameMain.Client.CharacterInfo = myCharacterInfo;
						GameMain.NetLobbyScreen.SetCampaignCharacterInfo(myCharacterInfo);
						GameMain.GameSession.RefreshAnyOpenPlayerInfo();
					}
					else
					{
						GameMain.NetLobbyScreen.SetCampaignCharacterInfo(null);
						if (!GameMain.NetLobbyScreen.CampaignCharacterDiscarded)
						{
							GameMain.GameSession.RefreshAnyOpenPlayerInfo();
						}
					}
				}
			}
			CS$<>8__locals1.campaign.SuppressStateSending = true;
			if (CS$<>8__locals1.campaign.LastSaveID == CS$<>8__locals1.saveID)
			{
				GameMain.NetLobbyScreen.ToggleCampaignMode(true);
			}
			if (refreshCampaignUI)
			{
				MultiPlayerCampaign campaign2 = CS$<>8__locals1.campaign;
				if (campaign2 != null)
				{
					CampaignUI campaignUI3 = campaign2.CampaignUI;
					if (campaignUI3 != null)
					{
						UpgradeStore upgradeStore3 = campaignUI3.UpgradeStore;
						if (upgradeStore3 != null)
						{
							upgradeStore3.RequestRefresh(false);
						}
					}
				}
			}
			CS$<>8__locals1.campaign.SuppressStateSending = false;
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x00091A90 File Offset: 0x0008FC90
		public void ClientReadCrew(IReadMessage msg)
		{
			bool createNotification = msg.ReadBoolean();
			ushort availableHireLength = msg.ReadUInt16();
			List<CharacterInfo> availableHires = new List<CharacterInfo>();
			for (int i = 0; i < (int)availableHireLength; i++)
			{
				CharacterInfo hire = CharacterInfo.ClientRead(CharacterPrefab.HumanSpeciesName, msg, true);
				hire.Salary = msg.ReadInt32();
				availableHires.Add(hire);
			}
			ushort pendingHireLength = msg.ReadUInt16();
			List<ushort> pendingHires = new List<ushort>();
			bool[] pendingHiresToReserveBench = new bool[(int)pendingHireLength];
			for (int j = 0; j < (int)pendingHireLength; j++)
			{
				pendingHires.Add(msg.ReadUInt16());
				pendingHiresToReserveBench[j] = msg.ReadBoolean();
			}
			List<CharacterInfo> hiredCharacters = new List<CharacterInfo>();
			List<CharacterInfo> updatedCrewManager = new List<CharacterInfo>();
			ushort crewLength = msg.ReadUInt16();
			for (int k = 0; k < (int)crewLength; k++)
			{
				CharacterInfo crewMember = CharacterInfo.ClientRead(CharacterPrefab.HumanSpeciesName, msg, true);
				if (crewMember.IsNewHire)
				{
					hiredCharacters.Add(crewMember);
				}
				updatedCrewManager.Add(crewMember);
			}
			CrewManager crewManager = GameMain.GameSession.CrewManager;
			bool crewManagerUpdated = crewManager != null && crewManager.UpdateCrewManagerIfNecessary(updatedCrewManager);
			ushort reserveBenchLength = msg.ReadUInt16();
			List<CharacterInfo> updatedReserveBench = new List<CharacterInfo>();
			for (int l = 0; l < (int)reserveBenchLength; l++)
			{
				CharacterInfo info2 = CharacterInfo.ClientRead(CharacterPrefab.HumanSpeciesName, msg, true);
				updatedReserveBench.Add(info2);
			}
			CrewManager crewManager2 = GameMain.GameSession.CrewManager;
			bool reserveBenchUpdated = crewManager2 != null && crewManager2.UpdateReserveBenchIfNeeded(updatedReserveBench);
			bool renameCrewMember = msg.ReadBoolean();
			if (renameCrewMember)
			{
				ushort renamedIdentifier = msg.ReadUInt16();
				string newName = msg.ReadString();
				CharacterInfo renamedCharacter = base.CrewManager.GetCharacterInfos(true).FirstOrDefault((CharacterInfo info) => info.ID == renamedIdentifier);
				if (renamedCharacter != null)
				{
					base.CrewManager.RenameCharacter(renamedCharacter, newName);
					renamedCharacter.RenamingEnabled = false;
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find a character to rename with the ID ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(renamedIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			bool fireCharacter = msg.ReadBoolean();
			if (fireCharacter)
			{
				ushort firedIdentifier = msg.ReadUInt16();
				CharacterInfo firedCharacter = base.CrewManager.GetCharacterInfos(true).FirstOrDefault((CharacterInfo info) => info.ID == firedIdentifier);
				if (firedCharacter != null)
				{
					base.CrewManager.FireCharacter(firedCharacter);
				}
			}
			Map map = this.map;
			bool flag;
			if (map == null)
			{
				flag = (null != null);
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				flag = (((currentLocation != null) ? currentLocation.HireManager : null) != null);
			}
			if (flag)
			{
				CampaignUI campaignUI = this.CampaignUI;
				if (((campaignUI != null) ? campaignUI.HRManagerUI : null) != null)
				{
					if (!NetIdUtils.IdMoreRecent(this.pendingSaveID, this.LastSaveID))
					{
						this.CampaignUI.HRManagerUI.SetHireables(this.map.CurrentLocation, availableHires);
						if (hiredCharacters.Any<CharacterInfo>())
						{
							this.CampaignUI.HRManagerUI.ValidateHires(hiredCharacters, false, false, createNotification);
						}
						this.CampaignUI.HRManagerUI.SetPendingHires(pendingHires, pendingHiresToReserveBench, this.map.CurrentLocation, false);
						goto IL_2F2;
					}
					goto IL_2F2;
				}
			}
			Location currentLocation2 = base.CurrentLocation;
			if (currentLocation2 != null)
			{
				currentLocation2.ForceHireableCharacters(availableHires);
			}
			IL_2F2:
			if (fireCharacter || renameCrewMember || crewManagerUpdated || reserveBenchUpdated)
			{
				CampaignUI campaignUI2 = this.CampaignUI;
				if (campaignUI2 != null)
				{
					HRManagerUI hrmanagerUI = campaignUI2.HRManagerUI;
					if (hrmanagerUI != null)
					{
						hrmanagerUI.RefreshHRView();
					}
				}
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return;
				}
				DeathPrompt deathPrompt = gameSession.DeathPrompt;
				if (deathPrompt == null)
				{
					return;
				}
				deathPrompt.UpdateBotList();
			}
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x00091DD4 File Offset: 0x0008FFD4
		public void ClientReadMoney(IReadMessage inc)
		{
			NetWalletUpdate update = INetSerializableStruct.Read<NetWalletUpdate>(inc);
			NetWalletTransaction[] transactions = update.Transactions;
			for (int i = 0; i < transactions.Length; i++)
			{
				NetWalletTransaction transaction = transactions[i];
				WalletInfo info = transaction.Info;
				ushort charID;
				if (transaction.CharacterID.TryUnwrap(out charID))
				{
					List<Character> characterList = Character.CharacterList;
					Character targetCharacter = (characterList != null) ? characterList.FirstOrDefault((Character c) => c.ID == charID) : null;
					if (targetCharacter == null)
					{
						break;
					}
					Wallet wallet = targetCharacter.Wallet;
					wallet.Balance = info.Balance;
					wallet.RewardDistribution = info.RewardDistribution;
					this.<ClientReadMoney>g__TryInvokeEvent|29_0(wallet, transaction.ChangedData, info);
				}
				else
				{
					this.Bank.Balance = info.Balance;
					this.Bank.RewardDistribution = info.RewardDistribution;
					this.<ClientReadMoney>g__TryInvokeEvent|29_0(this.Bank, transaction.ChangedData, info);
				}
			}
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x00091EC4 File Offset: 0x000900C4
		public override bool TryPurchase(Client client, int price)
		{
			if (price == 0)
			{
				return true;
			}
			if (!CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMoney))
			{
				return this.PersonalWallet.TryDeduct(price);
			}
			int balance = this.PersonalWallet.Balance;
			if (balance >= price)
			{
				return this.PersonalWallet.TryDeduct(price);
			}
			if (balance + this.Bank.Balance >= price)
			{
				int remainder = price - balance;
				if (balance > 0)
				{
					this.PersonalWallet.Deduct(balance);
				}
				this.Bank.Deduct(remainder);
				return true;
			}
			return false;
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00091F3F File Offset: 0x0009013F
		public override void Save(XElement element, bool isSavingOnLoading)
		{
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x00091F44 File Offset: 0x00090144
		public void LoadState(string filePath)
		{
			DebugConsole.Log("Loading save file for an existing game session (" + filePath + ")");
			SaveUtil.DecompressToDirectory(filePath, SaveUtil.TempPath);
			string gamesessionDocPath = Path.Combine(new string[]
			{
				SaveUtil.TempPath,
				"gamesession.xml"
			});
			XDocument doc = XMLExtensions.TryLoadXml(gamesessionDocPath);
			if (doc == null)
			{
				DebugConsole.ThrowError("Failed to load the state of a multiplayer campaign. Could not open the file \"" + gamesessionDocPath + "\".", null, null, false, false);
				return;
			}
			this.Load(doc.Root.Element("MultiPlayerCampaign"));
			SubmarineInfo selectedSub;
			GameMain.GameSession.OwnedSubmarines = SaveUtil.LoadOwnedSubmarines(doc, out selectedSub);
			GameMain.GameSession.SubmarineInfo = selectedSub;
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x00091FE9 File Offset: 0x000901E9
		public ushort GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags flag)
		{
			if (!MultiPlayerCampaign.ValidateFlag(flag))
			{
				return 0;
			}
			return this.lastUpdateID[flag];
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x00092001 File Offset: 0x00090201
		public void SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags flag, ushort id)
		{
			if (!MultiPlayerCampaign.ValidateFlag(flag))
			{
				return;
			}
			this.lastUpdateID[flag] = id;
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x0009201C File Offset: 0x0009021C
		public void IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags flag)
		{
			if (!MultiPlayerCampaign.ValidateFlag(flag))
			{
				return;
			}
			if (!this.lastUpdateID.ContainsKey(flag))
			{
				this.lastUpdateID[flag] = 0;
			}
			Dictionary<MultiPlayerCampaign.NetFlags, ushort> dictionary = this.lastUpdateID;
			ushort num = dictionary[flag];
			dictionary[flag] = num + 1;
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00092068 File Offset: 0x00090268
		public void IncrementAllLastUpdateIds()
		{
			foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
			{
				MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
				if (!this.lastUpdateID.ContainsKey(flag))
				{
					this.lastUpdateID[flag] = 0;
				}
				Dictionary<MultiPlayerCampaign.NetFlags, ushort> dictionary = this.lastUpdateID;
				MultiPlayerCampaign.NetFlags key = flag;
				ushort num = dictionary[key];
				dictionary[key] = num + 1;
			}
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x000920FC File Offset: 0x000902FC
		private static bool ValidateFlag(MultiPlayerCampaign.NetFlags flag)
		{
			return MathHelper.IsPowerOfTwo((int)flag);
		}

		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x06000F46 RID: 3910 RVA: 0x00092109 File Offset: 0x00090309
		// (set) Token: 0x06000F47 RID: 3911 RVA: 0x00092111 File Offset: 0x00090311
		public ushort LastSaveID
		{
			get
			{
				return this.lastSaveID;
			}
			set
			{
				this.lastSaveID = value;
			}
		}

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06000F48 RID: 3912 RVA: 0x0009211A File Offset: 0x0009031A
		// (set) Token: 0x06000F49 RID: 3913 RVA: 0x00092122 File Offset: 0x00090322
		public byte CampaignID { get; set; }

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000F4A RID: 3914 RVA: 0x0009212B File Offset: 0x0009032B
		// (set) Token: 0x06000F4B RID: 3915 RVA: 0x00092133 File Offset: 0x00090333
		public byte RoundID { get; set; }

		// Token: 0x06000F4C RID: 3916 RVA: 0x0009213C File Offset: 0x0009033C
		private MultiPlayerCampaign(CampaignSettings settings) : base(GameModePreset.MultiPlayerCampaign, settings)
		{
			MultiPlayerCampaign.currentCampaignID += 1;
			this.lastUpdateID = new Dictionary<MultiPlayerCampaign.NetFlags, ushort>();
			foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
			{
				MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
				this.lastUpdateID[flag] = 0;
			}
			this.CampaignID = MultiPlayerCampaign.currentCampaignID;
			this.UpgradeManager = new UpgradeManager(this);
			base.InitFactions();
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x000921EC File Offset: 0x000903EC
		public static MultiPlayerCampaign StartNew(string mapSeed, CampaignSettings settings)
		{
			MultiPlayerCampaign campaign = new MultiPlayerCampaign(settings);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				campaign.map = new Map(campaign, mapSeed);
			}
			campaign.InitProjSpecific();
			return campaign;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x00092228 File Offset: 0x00090428
		public static MultiPlayerCampaign LoadNew(XElement element)
		{
			MultiPlayerCampaign campaign = new MultiPlayerCampaign(CampaignSettings.Empty);
			campaign.Load(element);
			campaign.InitProjSpecific();
			campaign.IsFirstRound = false;
			return campaign;
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x00092255 File Offset: 0x00090455
		private void InitProjSpecific()
		{
			this.CreateButtons();
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x00092260 File Offset: 0x00090460
		public static string GetCharacterDataSavePath(string loadPath)
		{
			string directory = Path.GetDirectoryName(loadPath);
			string fileName = Path.GetFileNameWithoutExtension(loadPath);
			uint backupIndex;
			if (CampaignDataPath.IsBackupPath(loadPath, out backupIndex))
			{
				string trimmedFileName = Path.GetFileNameWithoutExtension(fileName);
				string[] array = new string[2];
				array[0] = directory;
				int num = 1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 3);
				defaultInterpolatedStringHandler.AppendFormatted(trimmedFileName);
				defaultInterpolatedStringHandler.AppendLiteral("_CharacterData");
				defaultInterpolatedStringHandler.AppendFormatted(".xml.bk");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(backupIndex);
				array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
				return Path.Combine(array);
			}
			return Path.Combine(new string[]
			{
				directory,
				fileName + "_CharacterData.xml"
			});
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x000922F5 File Offset: 0x000904F5
		public static string GetCharacterDataPathForLoading()
		{
			return MultiPlayerCampaign.GetCharacterDataSavePath(GameMain.GameSession.DataPath.LoadPath);
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x0009230B File Offset: 0x0009050B
		public static string GetCharacterDataPathForSaving()
		{
			return MultiPlayerCampaign.GetCharacterDataSavePath(GameMain.GameSession.DataPath.SavePath);
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00092324 File Offset: 0x00090524
		private void Load(XElement element)
		{
			base.LoadSaveSharedSingleAndMultiplayer(element);
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "campaignsettings"))
				{
					if (!(a == "map"))
					{
						if (!(a == "metadata"))
						{
							if (!(a == "bots"))
							{
								continue;
							}
						}
						else
						{
							Dictionary<Faction, float> prevReputations = base.Factions.ToDictionary((Faction k) => k, (Faction v) => v.Reputation.Value);
							this.CampaignMetadata.Load(subElement);
							using (IEnumerator<Faction> enumerator2 = base.Factions.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									Faction faction = enumerator2.Current;
									if (!MathUtils.NearlyEqual(prevReputations[faction], faction.Reputation.Value, 0.0001f))
									{
										NamedEvent<Reputation> onReputationValueChanged = faction.Reputation.OnReputationValueChanged;
										if (onReputationValueChanged != null)
										{
											onReputationValueChanged.Invoke(faction.Reputation);
										}
										Reputation.OnAnyReputationValueChanged.Invoke(faction.Reputation);
									}
								}
								continue;
							}
						}
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
						{
							base.CrewManager.HasBots = subElement.GetAttributeBool("hasbots", false);
							base.CrewManager.AddCharacterElements(subElement);
							base.ActiveOrdersElement = subElement.GetChildElement("activeorders", StringComparison.OrdinalIgnoreCase);
						}
					}
					else if (this.map == null)
					{
						this.map = Map.Load(this, subElement);
					}
					else
					{
						this.map.LoadState(this, subElement, this.LastSaveID > 0);
					}
				}
				else
				{
					this.Settings = new CampaignSettings(subElement);
					GameMain.NetworkMember.ServerSettings.CampaignSettings = this.Settings;
				}
			}
			if (this.UpgradeManager == null)
			{
				this.UpgradeManager = new UpgradeManager(this);
			}
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x00092574 File Offset: 0x00090774
		public static List<SubmarineInfo> GetCampaignSubs()
		{
			IEnumerable<SubmarineInfo> availableSubs = SubmarineInfo.SavedSubmarines;
			if (GameMain.Client != null)
			{
				availableSubs = GameMain.Client.ServerSubmarines;
			}
			List<SubmarineInfo> campaignSubs = (from s in availableSubs
			where s.IsCampaignCompatible && MultiPlayerCampaign.<GetCampaignSubs>g__isSubmarineVisible|61_0(s)
			select s).ToList<SubmarineInfo>();
			if (!campaignSubs.Any<SubmarineInfo>())
			{
				campaignSubs.AddRange(availableSubs.Where(new Func<SubmarineInfo, bool>(MultiPlayerCampaign.<GetCampaignSubs>g__isSubmarineVisible|61_0)));
			}
			if (!campaignSubs.Any<SubmarineInfo>())
			{
				campaignSubs.Add(GameMain.NetLobbyScreen.SelectedSub);
			}
			return campaignSubs;
		}

		// Token: 0x06000F55 RID: 3925 RVA: 0x00092600 File Offset: 0x00090800
		private static void WriteItems(IWriteMessage msg, Dictionary<Identifier, List<PurchasedItem>> purchasedItems)
		{
			msg.WriteByte((byte)purchasedItems.Count);
			foreach (KeyValuePair<Identifier, List<PurchasedItem>> storeItems in purchasedItems)
			{
				msg.WriteIdentifier(storeItems.Key);
				msg.WriteUInt16((ushort)storeItems.Value.Count);
				foreach (PurchasedItem item in storeItems.Value)
				{
					msg.WriteIdentifier(item.ItemPrefabIdentifier);
					msg.WriteBoolean(item.DeliverImmediately);
					msg.WriteRangedInteger(item.Quantity, 0, 100);
				}
			}
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x000926DC File Offset: 0x000908DC
		private static Dictionary<Identifier, List<PurchasedItem>> ReadPurchasedItems(IReadMessage msg, Client sender)
		{
			Dictionary<Identifier, List<PurchasedItem>> items = new Dictionary<Identifier, List<PurchasedItem>>();
			byte storeCount = msg.ReadByte();
			for (int i = 0; i < (int)storeCount; i++)
			{
				Identifier storeId = msg.ReadIdentifier();
				items.Add(storeId, new List<PurchasedItem>());
				ushort itemCount = msg.ReadUInt16();
				for (int j = 0; j < (int)itemCount; j++)
				{
					Identifier itemId = msg.ReadIdentifier();
					bool deliverImmediately = msg.ReadBoolean();
					int quantity = msg.ReadRangedInteger(0, 100);
					items[storeId].Add(new PurchasedItem(itemId, quantity, sender)
					{
						DeliverImmediately = deliverImmediately
					});
				}
			}
			return items;
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x00092768 File Offset: 0x00090968
		private static void WriteItems(IWriteMessage msg, Dictionary<Identifier, List<SoldItem>> soldItems)
		{
			msg.WriteByte((byte)soldItems.Count);
			foreach (KeyValuePair<Identifier, List<SoldItem>> storeItems in soldItems)
			{
				msg.WriteIdentifier(storeItems.Key);
				msg.WriteUInt16((ushort)storeItems.Value.Count);
				foreach (SoldItem item in storeItems.Value)
				{
					msg.WriteIdentifier(item.ItemPrefab.Identifier);
					msg.WriteUInt16(item.ID);
					msg.WriteBoolean(item.Removed);
					msg.WriteByte(item.SellerID);
					msg.WriteByte((byte)item.Origin);
				}
			}
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x00092860 File Offset: 0x00090A60
		private static Dictionary<Identifier, List<SoldItem>> ReadSoldItems(IReadMessage msg)
		{
			Dictionary<Identifier, List<SoldItem>> soldItems = new Dictionary<Identifier, List<SoldItem>>();
			byte storeCount = msg.ReadByte();
			for (int i = 0; i < (int)storeCount; i++)
			{
				Identifier storeId = msg.ReadIdentifier();
				soldItems.Add(storeId, new List<SoldItem>());
				ushort itemCount = msg.ReadUInt16();
				for (int j = 0; j < (int)itemCount; j++)
				{
					Identifier prefabId = msg.ReadIdentifier();
					ushort itemId = msg.ReadUInt16();
					bool removed = msg.ReadBoolean();
					byte sellerId = msg.ReadByte();
					byte origin = msg.ReadByte();
					soldItems[storeId].Add(new SoldItem(ItemPrefab.Prefabs[prefabId], itemId, removed, sellerId, (SoldItem.SellOrigin)origin));
				}
			}
			return soldItems;
		}

		// Token: 0x06000F5C RID: 3932 RVA: 0x00092978 File Offset: 0x00090B78
		[CompilerGenerated]
		internal static bool <ClientRead>g__ShouldApply|27_0(MultiPlayerCampaign.NetFlags flag, ushort id, bool requireUpToDateSave, bool requireCorrectRoundId = true, ref MultiPlayerCampaign.<>c__DisplayClass27_0 A_4)
		{
			if (requireCorrectRoundId && A_4.roundId != A_4.campaign.RoundID)
			{
				A_4.campaign.SetLastUpdateIdForFlag(flag, id);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Received campaing update for a different round (client: ");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(A_4.campaign.RoundID);
				defaultInterpolatedStringHandler.AppendLiteral(", server: ");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(A_4.roundId);
				defaultInterpolatedStringHandler.AppendLiteral("), ignoring...");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			if (NetIdUtils.IdMoreRecent(id, A_4.campaign.GetLastUpdateIdForFlag(flag)) && (!requireUpToDateSave || A_4.saveID == A_4.campaign.LastSaveID))
			{
				A_4.campaign.SetLastUpdateIdForFlag(flag, id);
				return true;
			}
			return false;
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x00092A4C File Offset: 0x00090C4C
		[CompilerGenerated]
		internal static void <ClientRead>g__ReadStores|27_1(IReadMessage msg, bool apply, ref MultiPlayerCampaign.<>c__DisplayClass27_0 A_2)
		{
			Dictionary<Identifier, ushort> storeBalances = new Dictionary<Identifier, ushort>();
			if (msg.ReadBoolean())
			{
				byte storeCount = msg.ReadByte();
				for (int i = 0; i < (int)storeCount; i++)
				{
					Identifier identifier = msg.ReadIdentifier();
					ushort storeBalance = msg.ReadUInt16();
					storeBalances.Add(identifier, storeBalance);
				}
			}
			if (apply)
			{
				foreach (KeyValuePair<Identifier, ushort> balance in storeBalances)
				{
					Map map = A_2.campaign.Map;
					Location.StoreInfo storeInfo;
					if (map == null)
					{
						storeInfo = null;
					}
					else
					{
						Location currentLocation = map.CurrentLocation;
						storeInfo = ((currentLocation != null) ? currentLocation.GetStore(balance.Key) : null);
					}
					Location.StoreInfo store = storeInfo;
					if (store != null)
					{
						store.Balance = (int)balance.Value;
					}
				}
			}
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x00092B10 File Offset: 0x00090D10
		[CompilerGenerated]
		private void <ClientReadMoney>g__TryInvokeEvent|29_0(Wallet wallet, WalletChangedData data, WalletInfo info)
		{
			if (data.BalanceChanged.IsSome() || data.RewardDistributionChanged.IsSome())
			{
				this.OnMoneyChanged.Invoke(new WalletChangedEvent(wallet, data, info));
			}
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x00092B44 File Offset: 0x00090D44
		[CompilerGenerated]
		internal static bool <GetCampaignSubs>g__isSubmarineVisible|61_0(SubmarineInfo s)
		{
			return !GameMain.NetworkMember.ServerSettings.HiddenSubs.Any((string h) => s.Name.Equals(h, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x040007B4 RID: 1972
		public bool SuppressStateSending;

		// Token: 0x040007B5 RID: 1973
		private ushort pendingSaveID = 1;

		// Token: 0x040007B6 RID: 1974
		private readonly Dictionary<MultiPlayerCampaign.NetFlags, ushort> lastUpdateID;

		// Token: 0x040007B7 RID: 1975
		private ushort lastSaveID;

		// Token: 0x040007B8 RID: 1976
		private static byte currentCampaignID;

		// Token: 0x0200089C RID: 2204
		[Flags]
		public enum NetFlags : ushort
		{
			// Token: 0x04003EB0 RID: 16048
			Misc = 1,
			// Token: 0x04003EB1 RID: 16049
			MapAndMissions = 2,
			// Token: 0x04003EB2 RID: 16050
			UpgradeManager = 4,
			// Token: 0x04003EB3 RID: 16051
			SubList = 8,
			// Token: 0x04003EB4 RID: 16052
			ItemsInBuyCrate = 16,
			// Token: 0x04003EB5 RID: 16053
			ItemsInSellFromSubCrate = 32,
			// Token: 0x04003EB6 RID: 16054
			PurchasedItems = 128,
			// Token: 0x04003EB7 RID: 16055
			SoldItems = 256,
			// Token: 0x04003EB8 RID: 16056
			Reputation = 512,
			// Token: 0x04003EB9 RID: 16057
			CharacterInfo = 2048
		}
	}
}
