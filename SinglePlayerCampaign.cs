using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200006A RID: 106
	internal class SinglePlayerCampaign : CampaignMode
	{
		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000F60 RID: 3936 RVA: 0x00092B84 File Offset: 0x00090D84
		public override bool Paused
		{
			get
			{
				return base.ForceMapUI || CoroutineManager.IsCoroutineRunning("LevelTransition") || (base.ShowCampaignUI && this.CampaignUI.SelectedTab == CampaignMode.InteractionType.Map) || (base.SlideshowPlayer != null && !base.SlideshowPlayer.LastTextShown);
			}
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x00092BD8 File Offset: 0x00090DD8
		public override void UpdateWhilePaused(float deltaTime)
		{
			if (CoroutineManager.IsCoroutineRunning("LevelTransition") || CoroutineManager.IsCoroutineRunning("SubmarineTransition") || this.gameOver)
			{
				return;
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
			SlideshowPlayer slideshowPlayer = base.SlideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.UpdateManually(deltaTime, false, true);
			}
			ChatBox chatBox = base.CrewManager.ChatBox;
			if (chatBox != null)
			{
				chatBox.Update(deltaTime);
			}
			base.CrewManager.UpdateReports();
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000F62 RID: 3938 RVA: 0x00092C92 File Offset: 0x00090E92
		// (set) Token: 0x06000F63 RID: 3939 RVA: 0x00092C9A File Offset: 0x00090E9A
		public override bool PurchasedHullRepairs
		{
			get
			{
				return this.PurchasedHullRepairsInLatestSave;
			}
			set
			{
				this.PurchasedHullRepairsInLatestSave = value;
			}
		}

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000F64 RID: 3940 RVA: 0x00092CA3 File Offset: 0x00090EA3
		// (set) Token: 0x06000F65 RID: 3941 RVA: 0x00092CAB File Offset: 0x00090EAB
		public override bool PurchasedLostShuttles
		{
			get
			{
				return this.PurchasedLostShuttlesInLatestSave;
			}
			set
			{
				this.PurchasedLostShuttlesInLatestSave = value;
			}
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000F66 RID: 3942 RVA: 0x00092CB4 File Offset: 0x00090EB4
		// (set) Token: 0x06000F67 RID: 3943 RVA: 0x00092CBC File Offset: 0x00090EBC
		public override bool PurchasedItemRepairs
		{
			get
			{
				return this.PurchasedItemRepairsInLatestSave;
			}
			set
			{
				this.PurchasedItemRepairsInLatestSave = value;
			}
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x00092CC8 File Offset: 0x00090EC8
		private SinglePlayerCampaign(string mapSeed, CampaignSettings settings) : base(GameModePreset.SinglePlayerCampaign, settings)
		{
			this.UpgradeManager = new UpgradeManager(this);
			this.Settings = settings;
			base.InitFactions();
			this.map = new Map(this, mapSeed);
			foreach (JobPrefab jobPrefab in JobPrefab.Prefabs)
			{
				for (int i = 0; i < jobPrefab.InitialCount; i++)
				{
					int variant = Rand.Range(0, jobPrefab.Variants, Rand.RandSync.Unsynced);
					base.CrewManager.AddCharacterInfo(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab, variant, Rand.RandSync.Unsynced, default(Identifier)));
				}
			}
			this.InitUI();
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x00092D98 File Offset: 0x00090F98
		private SinglePlayerCampaign(XElement element) : base(GameModePreset.SinglePlayerCampaign, CampaignSettings.Empty)
		{
			base.IsFirstRound = false;
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "metadata")
				{
					this.CampaignMetadata.Load(subElement);
				}
			}
			base.InitFactions();
			foreach (XElement subElement2 in element.Elements())
			{
				string a2 = subElement2.Name.ToString().ToLowerInvariant();
				if (!(a2 == "campaignsettings"))
				{
					if (!(a2 == "crew"))
					{
						if (a2 == "map")
						{
							this.map = Map.Load(this, subElement2);
						}
					}
					else
					{
						GameMain.GameSession.CrewManager = new CrewManager(subElement2, true);
						base.ActiveOrdersElement = subElement2.GetChildElement("activeorders", StringComparison.OrdinalIgnoreCase);
					}
				}
				else
				{
					this.Settings = new CampaignSettings(subElement2);
				}
			}
			base.LoadSaveSharedSingleAndMultiplayer(element);
			if (this.UpgradeManager == null)
			{
				this.UpgradeManager = new UpgradeManager(this);
			}
			this.InitUI();
			if (this.map == null)
			{
				throw new Exception("Failed to load the campaign save file (saved with an older, incompatible version of Barotrauma).");
			}
			this.savedOnStart = true;
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x00092F20 File Offset: 0x00091120
		public static SinglePlayerCampaign StartNew(string mapSeed, CampaignSettings startingSettings)
		{
			return new SinglePlayerCampaign(mapSeed, startingSettings);
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00092F29 File Offset: 0x00091129
		public static SinglePlayerCampaign Load(XElement element)
		{
			return new SinglePlayerCampaign(element);
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x00092F34 File Offset: 0x00091134
		private void InitUI()
		{
			base.CreateEndRoundButton();
			this.campaignUIContainer = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), "InnerGlow", new Color?(Color.Black));
			this.CampaignUI = new CampaignUI(this, this.campaignUIContainer)
			{
				StartRound = delegate()
				{
					this.TryEndRound();
				}
			};
			this.endRoundButton = base.CreateEndRoundButton();
			this.endRoundButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				base.TryEndRoundWithFuelCheck(delegate
				{
					this.TryEndRound();
				}, delegate
				{
					base.ShowCampaignUI = true;
					this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
				});
				return true;
			};
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x00092FD5 File Offset: 0x000911D5
		public override void HUDScaleChanged()
		{
			base.CreateEndRoundButton();
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x00092FE0 File Offset: 0x000911E0
		public override void Start()
		{
			base.Start();
			this.CargoManager.CreatePurchasedItems();
			this.UpgradeManager.ApplyUpgrades();
			this.UpgradeManager.SanityCheckUpgrades();
			if (!this.savedOnStart)
			{
				GUI.SetSavingIndicatorState(true);
				SaveUtil.SaveGame(GameMain.GameSession.DataPath, true);
				this.savedOnStart = true;
			}
			base.CrewDead = false;
			this.endTimer = 5f;
			base.CrewManager.InitSinglePlayerRound();
			base.LoadPets();
			base.LoadActiveOrders();
			this.CargoManager.InitPurchasedIDCards();
			GUI.DisableSavingIndicatorDelayed(3f);
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x00093078 File Offset: 0x00091278
		protected override void LoadInitialLevel()
		{
			GameMain instance = GameMain.Instance;
			LocationConnection selectedConnection = this.map.SelectedConnection;
			LevelData level = ((selectedConnection != null) ? selectedConnection.LevelData : null) ?? this.map.CurrentLocation.LevelData;
			Location currentLocation = this.map.CurrentLocation;
			LocationConnection selectedConnection2 = this.map.SelectedConnection;
			instance.ShowLoading(this.DoLoadInitialLevel(level, currentLocation != ((selectedConnection2 != null) ? selectedConnection2.Locations[0] : null)), true);
		}

		// Token: 0x06000F70 RID: 3952 RVA: 0x000930EB File Offset: 0x000912EB
		private IEnumerable<CoroutineStatus> DoLoadInitialLevel(LevelData level, bool mirror)
		{
			SinglePlayerCampaign.<DoLoadInitialLevel>d__26 <DoLoadInitialLevel>d__ = new SinglePlayerCampaign.<DoLoadInitialLevel>d__26(-2);
			<DoLoadInitialLevel>d__.<>4__this = this;
			<DoLoadInitialLevel>d__.<>3__level = level;
			<DoLoadInitialLevel>d__.<>3__mirror = mirror;
			return <DoLoadInitialLevel>d__;
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x00093109 File Offset: 0x00091309
		private IEnumerable<CoroutineStatus> DoInitialCameraTransition()
		{
			SinglePlayerCampaign.<DoInitialCameraTransition>d__27 <DoInitialCameraTransition>d__ = new SinglePlayerCampaign.<DoInitialCameraTransition>d__27(-2);
			<DoInitialCameraTransition>d__.<>4__this = this;
			return <DoInitialCameraTransition>d__;
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x00093119 File Offset: 0x00091319
		protected override IEnumerable<CoroutineStatus> DoLevelTransition(CampaignMode.TransitionType transitionType, LevelData newLevel, Submarine leavingSub, bool mirror)
		{
			SinglePlayerCampaign.<DoLevelTransition>d__28 <DoLevelTransition>d__ = new SinglePlayerCampaign.<DoLevelTransition>d__28(-2);
			<DoLevelTransition>d__.<>4__this = this;
			<DoLevelTransition>d__.<>3__transitionType = transitionType;
			<DoLevelTransition>d__.<>3__newLevel = newLevel;
			<DoLevelTransition>d__.<>3__mirror = mirror;
			return <DoLevelTransition>d__;
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x00093140 File Offset: 0x00091340
		protected override void EndCampaignProjSpecific()
		{
			GameMain.GameSession.SubmarineInfo = new SubmarineInfo(GameMain.GameSession.Submarine);
			SaveUtil.SaveGame(GameMain.GameSession.DataPath, false);
			GameMain.CampaignEndScreen.Select();
			GUI.DisableHUD = false;
			GameMain.CampaignEndScreen.OnFinished = delegate()
			{
				this.showCampaignResetText = true;
				this.LoadInitialLevel();
				base.IsFirstRound = true;
			};
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x0009319C File Offset: 0x0009139C
		public override void Update(float deltaTime)
		{
			if (CoroutineManager.IsCoroutineRunning("LevelTransition") || CoroutineManager.IsCoroutineRunning("SubmarineTransition") || this.gameOver)
			{
				return;
			}
			base.Update(deltaTime);
			SlideshowPlayer slideshowPlayer = base.SlideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.UpdateManually(deltaTime, false, true);
			}
			Map map = base.Map;
			if (map != null)
			{
				Radiation radiation = map.Radiation;
				if (radiation != null)
				{
					radiation.UpdateRadiation(deltaTime);
				}
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
			if (base.ShowCampaignUI || base.ForceMapUI)
			{
				Character.DisableControls = true;
			}
			if (!GUI.DisableHUD && !GUI.DisableUpperHUD)
			{
				this.endRoundButton.UpdateManually(deltaTime, false, true);
				if (CoroutineManager.IsCoroutineRunning("LevelTransition") || base.ForceMapUI)
				{
					return;
				}
			}
			if (Level.Loaded.Type == LevelData.LevelType.Outpost)
			{
				base.KeepCharactersCloseToOutpost(deltaTime);
				if (this.wasDocked)
				{
					IEnumerable<Submarine> connectedSubs = Submarine.MainSub.GetConnectedSubs();
					if (Level.Loaded.StartOutpost == null || !connectedSubs.Contains(Level.Loaded.StartOutpost))
					{
						base.ForceMapUI = true;
						this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
					}
				}
				else if (Level.Loaded.IsEndBiome)
				{
					LevelData levelData;
					Submarine leavingSub;
					CampaignMode.TransitionType transitionType = base.GetAvailableTransition(out levelData, out leavingSub);
					if (transitionType == CampaignMode.TransitionType.ProgressToNextLocation)
					{
						base.LoadNewLevel();
					}
				}
				else if (!Submarine.MainSub.AtStartExit)
				{
					Submarine startOutpost = Level.Loaded.StartOutpost;
					if (startOutpost != null)
					{
						IReadOnlyList<WayPoint> exitPoints = startOutpost.ExitPoints;
						if (exitPoints != null && exitPoints.Count > 0)
						{
							goto IL_281;
						}
					}
					base.ForceMapUI = true;
					this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
				}
			}
			else
			{
				LevelData levelData;
				Submarine leavingSub2;
				CampaignMode.TransitionType transitionType2 = base.GetAvailableTransition(out levelData, out leavingSub2);
				if (Level.Loaded.IsEndBiome && transitionType2 == CampaignMode.TransitionType.ProgressToNextLocation)
				{
					base.LoadNewLevel();
				}
				else if (transitionType2 == CampaignMode.TransitionType.ProgressToNextLocation && Level.Loaded.EndOutpost != null && Level.Loaded.EndOutpost.DockedTo.Contains(leavingSub2))
				{
					base.LoadNewLevel();
				}
				else if (transitionType2 == CampaignMode.TransitionType.ReturnToPreviousLocation && Level.Loaded.StartOutpost != null && Level.Loaded.StartOutpost.DockedTo.Contains(leavingSub2))
				{
					base.LoadNewLevel();
				}
				else if (transitionType2 == CampaignMode.TransitionType.None && this.CampaignUI.SelectedTab == CampaignMode.InteractionType.Map)
				{
					base.ShowCampaignUI = false;
				}
				HintManager.OnAvailableTransition(transitionType2);
			}
			IL_281:
			if (!base.CrewDead)
			{
				if (base.CrewManager.GetCharacters().None((Character c) => !c.IsDead && !base.CrewManager.IsFired(c)))
				{
					base.CrewDead = true;
					return;
				}
			}
			else
			{
				this.endTimer -= deltaTime;
				if (this.endTimer <= 0f)
				{
					this.GameOver();
				}
			}
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x0009347C File Offset: 0x0009167C
		private bool TryEndRound()
		{
			LevelData nextLevel;
			Submarine leavingSub;
			CampaignMode.TransitionType transitionType = base.GetAvailableTransition(out nextLevel, out leavingSub);
			if (leavingSub == null || transitionType == CampaignMode.TransitionType.None)
			{
				return false;
			}
			if (nextLevel == null)
			{
				base.ForceMapUI = true;
				this.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
				this.map.SelectLocation(-1);
				return false;
			}
			if (transitionType == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
			{
				base.Map.SetLocation(base.Map.Locations.IndexOf(Level.Loaded.EndLocation ?? base.Map.CurrentLocation));
			}
			List<Submarine> subsToLeaveBehind = CampaignMode.GetSubsToLeaveBehind(leavingSub);
			if (subsToLeaveBehind.Any<Submarine>())
			{
				LocalizedString msg = TextManager.Get((subsToLeaveBehind.Count == 1) ? "LeaveSubBehind" : "LeaveSubsBehind");
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("Warning"), msg, new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUIButton guibutton = msgBox.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					this.LoadNewLevel();
					return true;
				}));
				GUIButton guibutton2 = msgBox.Buttons[0];
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
				msgBox.Buttons[0].UserData = Submarine.Loaded.FindAll((Submarine s) => !subsToLeaveBehind.Contains(s));
				GUIButton guibutton3 = msgBox.Buttons[1];
				guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
			}
			else
			{
				base.LoadNewLevel();
			}
			return true;
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x00093664 File Offset: 0x00091864
		private void GameOver()
		{
			this.gameOver = true;
			GameMain.GameSession.EndRound("", CampaignMode.TransitionType.None, null, true);
			this.EnableRoundSummaryGameOverState();
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x00093698 File Offset: 0x00091898
		private void EnableRoundSummaryGameOverState()
		{
			RoundSummary roundSummary = GameMain.GameSession.RoundSummary;
			if (roundSummary != null)
			{
				roundSummary.ContinueButton.Visible = false;
				roundSummary.ContinueButton.IgnoreLayoutGroups = true;
				new GUIButton(new RectTransform(new Vector2(0.25f, 1f), roundSummary.ButtonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("QuitButton"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object obj)
				{
					GameMain.MainMenuScreen.Select();
					GUIMessageBox.MessageBoxes.Remove(roundSummary.Frame);
					return true;
				};
				new GUIButton(new RectTransform(new Vector2(0.25f, 1f), roundSummary.ButtonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadGameButton"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object obj)
				{
					GameMain.GameSession.LoadPreviousSave();
					GUIMessageBox.MessageBoxes.Remove(roundSummary.Frame);
					return true;
				};
			}
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x000937CC File Offset: 0x000919CC
		public override void Save(XElement element, bool isSavingOnLoading)
		{
			XElement modeElement = new XElement("SinglePlayerCampaign", new object[]
			{
				new XAttribute("purchasedlostshuttles", this.PurchasedLostShuttles),
				new XAttribute("purchasedhullrepairs", this.PurchasedHullRepairs),
				new XAttribute("purchaseditemrepairs", this.PurchasedItemRepairs),
				new XAttribute("cheatsenabled", this.CheatsEnabled)
			});
			modeElement.Add(this.Settings.Save());
			modeElement.Add(base.SaveStats());
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.EventManager : null) != null)
			{
				XContainer xcontainer = modeElement;
				GameSession gameSession2 = GameMain.GameSession;
				xcontainer.Add((gameSession2 != null) ? gameSession2.EventManager.Save() : null);
			}
			foreach (ValueTuple<CharacterTeamType, Identifier> valueTuple in GameMain.GameSession.UnlockedRecipes)
			{
				CharacterTeamType team = valueTuple.Item1;
				Identifier unlockedRecipe = valueTuple.Item2;
				modeElement.Add(new XElement("unlockedrecipe", new object[]
				{
					new XAttribute("identifier", unlockedRecipe),
					new XAttribute("team", team)
				}));
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.Info != null)
				{
					if (c.IsDead)
					{
						base.CrewManager.RemoveCharacterInfo(c.Info);
					}
					c.Info.LastControlled = (c == this.lastControlledCharacter);
					c.Info.HealthData = new XElement("health");
					c.CharacterHealth.Save(c.Info.HealthData);
					if (c.Inventory != null)
					{
						c.Info.InventoryData = new XElement("inventory");
						c.SaveInventory();
						CharacterInventory inventory = c.Inventory;
						if (inventory != null)
						{
							inventory.DeleteAllItems();
						}
					}
					c.Info.SaveOrderData();
				}
			}
			base.SavePets(modeElement);
			XElement crewManagerElement = base.CrewManager.Save(modeElement);
			base.SaveActiveOrders(crewManagerElement);
			this.CampaignMetadata.Save(modeElement);
			base.Map.Save(modeElement);
			CargoManager cargoManager = this.CargoManager;
			if (cargoManager != null)
			{
				cargoManager.SavePurchasedItems(modeElement);
			}
			UpgradeManager upgradeManager = this.UpgradeManager;
			if (upgradeManager != null)
			{
				upgradeManager.Save(modeElement);
			}
			modeElement.Add(this.Bank.Save());
			element.Add(modeElement);
		}

		// Token: 0x040007BB RID: 1979
		public const int MinimumInitialMoney = 0;

		// Token: 0x040007BC RID: 1980
		private float endTimer;

		// Token: 0x040007BD RID: 1981
		private bool savedOnStart;

		// Token: 0x040007BE RID: 1982
		private bool gameOver;

		// Token: 0x040007BF RID: 1983
		private Character lastControlledCharacter;

		// Token: 0x040007C0 RID: 1984
		private bool showCampaignResetText;
	}
}
