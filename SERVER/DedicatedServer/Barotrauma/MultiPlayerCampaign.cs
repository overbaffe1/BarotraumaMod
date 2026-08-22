using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000034 RID: 52
	internal class MultiPlayerCampaign : CampaignMode
	{
		// Token: 0x06000649 RID: 1609 RVA: 0x0003AEF0 File Offset: 0x000390F0
		public override Wallet GetWallet(Client client = null)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client", "Client should not be null in multiplayer");
			}
			Character character = client.Character;
			if (character != null)
			{
				return character.Wallet;
			}
			return Wallet.Invalid;
		}

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x0003AF26 File Offset: 0x00039126
		// (set) Token: 0x0600064B RID: 1611 RVA: 0x0003AF2E File Offset: 0x0003912E
		public bool ForceMapUI
		{
			get
			{
				return this.forceMapUI;
			}
			set
			{
				if (this.forceMapUI == value)
				{
					return;
				}
				this.forceMapUI = value;
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
			}
		}

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x0600064C RID: 1612 RVA: 0x0003AF48 File Offset: 0x00039148
		// (set) Token: 0x0600064D RID: 1613 RVA: 0x0003AF50 File Offset: 0x00039150
		public bool GameOver { get; private set; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x0003AF59 File Offset: 0x00039159
		public override bool Paused
		{
			get
			{
				return this.ForceMapUI || CoroutineManager.IsCoroutineRunning("LevelTransition");
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x0600064F RID: 1615 RVA: 0x0003AF6F File Offset: 0x0003916F
		// (set) Token: 0x06000650 RID: 1616 RVA: 0x0003AF78 File Offset: 0x00039178
		public override bool PurchasedHullRepairs
		{
			get
			{
				return this.purchasedHullRepairs;
			}
			set
			{
				if (this.purchasedHullRepairs == value)
				{
					return;
				}
				this.purchasedHullRepairs = value;
				this.PurchasedHullRepairsInLatestSave = (this.PurchasedHullRepairsInLatestSave || value);
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc);
				DebugConsole.NewMessage("Set PurchasedHullRepairs to " + this.PurchasedHullRepairs.ToString(), new Color?(Color.Cyan), false);
			}
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000651 RID: 1617 RVA: 0x0003AFD3 File Offset: 0x000391D3
		// (set) Token: 0x06000652 RID: 1618 RVA: 0x0003AFDB File Offset: 0x000391DB
		public override bool PurchasedLostShuttles
		{
			get
			{
				return this.purchasedLostShuttles;
			}
			set
			{
				if (this.purchasedLostShuttles == value)
				{
					return;
				}
				this.purchasedLostShuttles = value;
				this.PurchasedLostShuttlesInLatestSave = (this.PurchasedLostShuttlesInLatestSave || value);
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc);
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000653 RID: 1619 RVA: 0x0003B003 File Offset: 0x00039203
		// (set) Token: 0x06000654 RID: 1620 RVA: 0x0003B00B File Offset: 0x0003920B
		public override bool PurchasedItemRepairs
		{
			get
			{
				return this.purchasedItemRepairs;
			}
			set
			{
				if (this.purchasedItemRepairs == value)
				{
					return;
				}
				this.purchasedItemRepairs = value;
				this.PurchasedItemRepairsInLatestSave = (this.PurchasedItemRepairsInLatestSave || value);
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc);
			}
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0003B034 File Offset: 0x00039234
		public static void StartNewCampaign(string savePath, string subPath, string seed, CampaignSettings startingSettings)
		{
			if (string.IsNullOrWhiteSpace(savePath))
			{
				return;
			}
			SubmarineInfo submarineInfo = new SubmarineInfo(subPath, "", null, true, false);
			Option.UnspecifiedNone none = Option.None;
			GameMain.GameSession = new GameSession(submarineInfo, none, CampaignDataPath.CreateRegular(savePath), GameModePreset.MultiPlayerCampaign, startingSettings, seed, null);
			GameMain.NetLobbyScreen.ToggleCampaignMode(true);
			SaveUtil.SaveGame(GameMain.GameSession.DataPath, false);
			DebugConsole.NewMessage("Campaign started!", new Color?(Color.Cyan), false);
			DebugConsole.NewMessage("Current location: " + GameMain.GameSession.Map.CurrentLocation.DisplayName, new Color?(Color.Cyan), false);
			((MultiPlayerCampaign)GameMain.GameSession.GameMode).LoadInitialLevel();
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0003B0F4 File Offset: 0x000392F4
		public static void LoadCampaign(CampaignDataPath path, Client client)
		{
			GameMain.NetLobbyScreen.ToggleCampaignMode(true);
			try
			{
				SaveUtil.LoadGame(path);
				MultiPlayerCampaign mpCampaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
				if (mpCampaign == null)
				{
					string str = "Failed to load a campaign. Unexpected game mode: ";
					GameMode gameMode = GameMain.GameSession.GameMode;
					DebugConsole.ThrowError(str + ((gameMode != null) ? gameMode.ToString() : null), null, null, false, false);
					return;
				}
				MultiPlayerCampaign multiPlayerCampaign = mpCampaign;
				ushort num = multiPlayerCampaign.LastSaveID;
				multiPlayerCampaign.LastSaveID = num + 1;
			}
			catch (Exception e)
			{
				string errorMsg = "Error while loading the save " + path.LoadPath;
				if (client != null)
				{
					GameServer server = GameMain.Server;
					if (server != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 3);
						defaultInterpolatedStringHandler.AppendFormatted(errorMsg);
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(e.Message);
						defaultInterpolatedStringHandler.AppendLiteral("\n");
						defaultInterpolatedStringHandler.AppendFormatted(e.StackTrace);
						server.SendDirectChatMessage(defaultInterpolatedStringHandler.ToStringAndClear(), client, ChatMessageType.Error);
					}
				}
				DebugConsole.ThrowError(errorMsg, e, null, false, false);
				return;
			}
			DebugConsole.NewMessage("Campaign loaded!", new Color?(Color.Cyan), false);
			DebugConsole.NewMessage((GameMain.GameSession.Map.SelectedLocation == null) ? GameMain.GameSession.Map.CurrentLocation.DisplayName : (GameMain.GameSession.Map.CurrentLocation.DisplayName + " -> " + GameMain.GameSession.Map.SelectedLocation.DisplayName), new Color?(Color.Cyan), false);
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0003B284 File Offset: 0x00039484
		protected override void LoadInitialLevel()
		{
			LocationConnection selectedConnection = this.map.SelectedConnection;
			base.NextLevel = (((selectedConnection != null) ? selectedConnection.LevelData : null) ?? this.map.CurrentLocation.LevelData);
			base.MirrorLevel = false;
			GameMain.Server.TryStartGame();
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0003B2D4 File Offset: 0x000394D4
		public static void StartCampaignSetup()
		{
			DebugConsole.NewMessage("********* CAMPAIGN SETUP *********", new Color?(Color.White), false);
			DebugConsole.ShowQuestionPrompt("Do you want to start a new campaign? Y/N", delegate(string arg)
			{
				if (arg.Equals("y", StringComparison.OrdinalIgnoreCase) || arg.Equals("yes", StringComparison.OrdinalIgnoreCase))
				{
					DebugConsole.ShowQuestionPrompt("Enter a save name for the campaign:", delegate(string saveName)
					{
						string savePath = SaveUtil.CreateSavePath(SaveUtil.SaveType.Multiplayer, saveName);
						MultiPlayerCampaign.StartNewCampaign(savePath, GameMain.NetLobbyScreen.SelectedSub.FilePath, GameMain.NetLobbyScreen.LevelSeed, CampaignSettings.Empty);
					}, null, -1);
					return;
				}
				CampaignMode.SaveInfo[] saveFiles = SaveUtil.GetSaveFiles(SaveUtil.SaveType.Multiplayer, false, true).ToArray<CampaignMode.SaveInfo>();
				if (saveFiles.Length == 0)
				{
					DebugConsole.ThrowError("No save files found.", null, null, false, false);
					return;
				}
				DebugConsole.NewMessage("Saved campaigns:", new Color?(Color.White), false);
				for (int i = 0; i < saveFiles.Length; i++)
				{
					DebugConsole.NewMessage("   " + i.ToString() + ". " + saveFiles[i].FilePath, new Color?(Color.White), false);
				}
				DebugConsole.ShowQuestionPrompt("Select a save file to load (0 - " + (saveFiles.Length - 1).ToString() + "):", delegate(string selectedSave)
				{
					int saveIndex = -1;
					if (!int.TryParse(selectedSave, out saveIndex))
					{
						return;
					}
					if (saveIndex < 0 || saveIndex >= saveFiles.Length)
					{
						DebugConsole.ThrowError("Invalid save file index.", null, null, false, false);
						return;
					}
					try
					{
						MultiPlayerCampaign.LoadCampaign(CampaignDataPath.CreateRegular(saveFiles[saveIndex].FilePath), null);
					}
					catch (Exception ex)
					{
						DebugConsole.ThrowError("Failed to load the campaign.", ex, null, false, false);
					}
				}, null, -1);
			}, null, -1);
		}

		// Token: 0x06000659 RID: 1625 RVA: 0x0003B321 File Offset: 0x00039521
		public override void Start()
		{
			base.Start();
			this.IncrementAllLastUpdateIds();
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0003B32F File Offset: 0x0003952F
		private static bool IsOwner(Client client)
		{
			return client != null && client.Connection == GameMain.Server.OwnerConnection;
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0003B348 File Offset: 0x00039548
		public void SaveExperiencePoints(Client client)
		{
			this.ClearSavedExperiencePoints(client);
			this.savedExperiencePoints.Add(new MultiPlayerCampaign.SavedExperiencePoints(client));
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0003B364 File Offset: 0x00039564
		public int GetSavedExperiencePoints(Client client)
		{
			MultiPlayerCampaign.SavedExperiencePoints savedExperiencePoints = this.savedExperiencePoints.Find((MultiPlayerCampaign.SavedExperiencePoints s) => client.AccountId == s.AccountId || client.Connection.Endpoint.Address == s.Address);
			if (savedExperiencePoints == null)
			{
				return 0;
			}
			return savedExperiencePoints.ExperiencePoints;
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0003B3A0 File Offset: 0x000395A0
		public void ClearSavedExperiencePoints(Client client)
		{
			this.savedExperiencePoints.RemoveAll((MultiPlayerCampaign.SavedExperiencePoints s) => client.AccountId == s.AccountId || client.Connection.Endpoint.Address == s.Address);
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0003B3D4 File Offset: 0x000395D4
		public void RefreshCharacterCampaignData(Character character, bool refreshHealthData)
		{
			CharacterCampaignData matchingData = this.characterData.FirstOrDefault((CharacterCampaignData c) => c.CharacterInfo == character.Info);
			if (matchingData != null)
			{
				matchingData.Refresh(character, refreshHealthData);
			}
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x0003B418 File Offset: 0x00039618
		public void SavePlayers()
		{
			using (IEnumerator<Client> enumerator = GameMain.Server.ConnectedClients.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Client c = enumerator.Current;
					if (c.Character != null && c.Character.Info == null)
					{
						c.Character = null;
					}
					Character character2 = c.Character;
					CharacterInfo characterInfo = (character2 != null) ? character2.Info : null;
					CharacterCampaignData matchingCharacterData = this.characterData.Find((CharacterCampaignData d) => d.MatchesClient(c));
					if (matchingCharacterData != null)
					{
						if (!matchingCharacterData.HasSpawned)
						{
							continue;
						}
						if (characterInfo == null)
						{
							characterInfo = matchingCharacterData.CharacterInfo;
						}
					}
					if (characterInfo != null && !characterInfo.Discarded)
					{
						CauseOfDeath causeOfDeath = characterInfo.CauseOfDeath;
						bool diedToDisconnect = causeOfDeath != null && causeOfDeath.Type == CauseOfDeathType.Disconnected;
						bool diedForReal = characterInfo.CauseOfDeath != null && !diedToDisconnect;
						if (diedForReal)
						{
							characterInfo.ApplyDeathEffects();
						}
						c.CharacterInfo = characterInfo;
						if (c.Character != null || diedForReal)
						{
							this.SetClientCharacterData(c);
						}
					}
				}
			}
			List<CharacterCampaignData> prevCharacterData = new List<CharacterCampaignData>(this.characterData);
			using (List<CharacterCampaignData>.Enumerator enumerator2 = prevCharacterData.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CharacterCampaignData data = enumerator2.Current;
					if (data.HasSpawned && !GameMain.Server.ConnectedClients.Any((Client c) => data.MatchesClient(c)))
					{
						Character character = Character.CharacterList.Find((Character c) => c.Info == data.CharacterInfo && !c.IsHusk);
						if (character != null)
						{
							if (character.IsDead)
							{
								CauseOfDeath causeOfDeath2 = character.CauseOfDeath;
								if (causeOfDeath2 == null || causeOfDeath2.Type != CauseOfDeathType.Disconnected)
								{
									goto IL_212;
								}
							}
							this.characterData.RemoveAll((CharacterCampaignData cd) => cd.IsDuplicate(data));
							CharacterCampaignData data2 = data;
							Character character3 = character;
							CauseOfDeath causeOfDeath3 = character.CauseOfDeath;
							data2.Refresh(character3, causeOfDeath3 == null || causeOfDeath3.Type != CauseOfDeathType.Disconnected);
							this.characterData.Add(data);
							continue;
						}
						IL_212:
						CauseOfDeath causeOfDeath = data.CharacterInfo.CauseOfDeath;
						if (causeOfDeath == null || causeOfDeath.Type != CauseOfDeathType.Disconnected)
						{
							data.CharacterInfo.ApplyDeathEffects();
							data.Reset();
						}
					}
				}
			}
			this.MoveDiscardedCharacterBalancesToBank();
			this.characterData.ForEach(delegate(CharacterCampaignData cd)
			{
				cd.HasSpawned = false;
			});
			foreach (CharacterCampaignData cd2 in this.characterData)
			{
				base.CrewManager.RemoveCharacterInfo(cd2.CharacterInfo);
			}
			base.SavePets(null);
			foreach (Character c2 in Character.CharacterList)
			{
				if (c2.Inventory != null)
				{
					if (Level.Loaded.Type == LevelData.LevelType.Outpost && c2.Submarine != Level.Loaded.StartOutpost)
					{
						base.Map.CurrentLocation.RegisterTakenItems(from it in c2.Inventory.AllItems
						where it.SpawnedInCurrentOutpost && it.OriginalModuleIndex > 0
						select it);
					}
					if (c2.Info != null && c2.IsBot)
					{
						if (c2.IsDead)
						{
							CauseOfDeath causeOfDeath4 = c2.CauseOfDeath;
							if (causeOfDeath4 == null || causeOfDeath4.Type != CauseOfDeathType.Disconnected)
							{
								base.CrewManager.RemoveCharacterInfo(c2.Info);
							}
						}
						c2.Info.HealthData = new XElement("health");
						c2.CharacterHealth.Save(c2.Info.HealthData);
						c2.Info.InventoryData = new XElement("inventory");
						c2.SaveInventory();
						c2.Info.SaveOrderData();
					}
					c2.Inventory.DeleteAllItems();
				}
			}
			base.SaveActiveOrders(null);
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0003B8D4 File Offset: 0x00039AD4
		public void MoveDiscardedCharacterBalancesToBank()
		{
			using (List<CharacterCampaignData>.Enumerator enumerator = this.discardedCharacters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CharacterCampaignData discardedCharacter = enumerator.Current;
					if (discardedCharacter.WalletData != null)
					{
						Character character = Character.CharacterList.Find((Character c) => c.Info == discardedCharacter.CharacterInfo);
						Wallet wallet = ((character != null) ? character.Wallet : null) ?? new Wallet(Option<Character>.None(), discardedCharacter.WalletData);
						this.Bank.Give(wallet.Balance);
					}
				}
			}
			this.discardedCharacters.Clear();
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0003B990 File Offset: 0x00039B90
		protected override IEnumerable<CoroutineStatus> DoLevelTransition(CampaignMode.TransitionType transitionType, LevelData newLevel, Submarine leavingSub, bool mirror)
		{
			MultiPlayerCampaign.<DoLevelTransition>d__43 <DoLevelTransition>d__ = new MultiPlayerCampaign.<DoLevelTransition>d__43(-2);
			<DoLevelTransition>d__.<>4__this = this;
			<DoLevelTransition>d__.<>3__transitionType = transitionType;
			<DoLevelTransition>d__.<>3__newLevel = newLevel;
			<DoLevelTransition>d__.<>3__leavingSub = leavingSub;
			<DoLevelTransition>d__.<>3__mirror = mirror;
			return <DoLevelTransition>d__;
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0003B9BD File Offset: 0x00039BBD
		public bool CanPurchaseSub(SubmarineInfo info, Client client)
		{
			return base.CanAfford(info.GetPrice(null, null), client) && MultiPlayerCampaign.GetCampaignSubs().Contains(info);
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0003B9E0 File Offset: 0x00039BE0
		public void DiscardClientCharacterData(Client client)
		{
			Func<CharacterCampaignData, bool> <>9__0;
			foreach (CharacterCampaignData data in this.characterData.ToList<CharacterCampaignData>())
			{
				if (data.MatchesClient(client))
				{
					IEnumerable<CharacterCampaignData> source = this.discardedCharacters;
					Func<CharacterCampaignData, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((CharacterCampaignData d) => d.MatchesClient(client)));
					}
					if (!source.Any(predicate))
					{
						this.discardedCharacters.Add(data);
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Client \"");
					defaultInterpolatedStringHandler.AppendFormatted<Client>(client);
					defaultInterpolatedStringHandler.AppendLiteral("\" discarded the character (");
					defaultInterpolatedStringHandler.AppendFormatted(data.Name);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					data.CharacterInfo.Discarded = true;
					this.characterData.Remove(data);
					this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.CharacterInfo);
				}
			}
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0003BB08 File Offset: 0x00039D08
		public CharacterCampaignData GetClientCharacterData(Client client)
		{
			return this.characterData.Find((CharacterCampaignData cd) => cd.MatchesClient(client));
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0003BB3C File Offset: 0x00039D3C
		public CharacterCampaignData GetCharacterData(CharacterInfo characterInfo)
		{
			return this.characterData.Find((CharacterCampaignData cd) => cd.CharacterInfo == characterInfo);
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0003BB70 File Offset: 0x00039D70
		public CharacterCampaignData SetClientCharacterData(Client client)
		{
			this.characterData.RemoveAll((CharacterCampaignData cd) => cd.MatchesClient(client));
			CharacterCampaignData data = new CharacterCampaignData(client);
			this.characterData.Add(data);
			this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.CharacterInfo);
			return data;
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0003BBC8 File Offset: 0x00039DC8
		public void AssignClientCharacterInfos(IEnumerable<Client> connectedClients)
		{
			foreach (Client client in connectedClients)
			{
				if (!client.SpectateOnly || !GameMain.Server.ServerSettings.AllowSpectating)
				{
					CharacterCampaignData matchingData = this.GetClientCharacterData(client);
					if (matchingData != null)
					{
						client.CharacterInfo = matchingData.CharacterInfo;
					}
				}
			}
			this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.CharacterInfo);
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0003BC44 File Offset: 0x00039E44
		public Dictionary<Client, Job> GetAssignedJobs(IEnumerable<Client> connectedClients)
		{
			Dictionary<Client, Job> assignedJobs = new Dictionary<Client, Job>();
			foreach (Client client in connectedClients)
			{
				CharacterCampaignData matchingData = this.GetClientCharacterData(client);
				if (matchingData != null)
				{
					assignedJobs.Add(client, matchingData.CharacterInfo.Job);
				}
			}
			return assignedJobs;
		}

		// Token: 0x06000669 RID: 1641 RVA: 0x0003BCAC File Offset: 0x00039EAC
		public override void Update(float deltaTime)
		{
			if (CoroutineManager.IsCoroutineRunning("LevelTransition"))
			{
				return;
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
			base.Update(deltaTime);
			MedicalClinic medicalClinic = this.MedicalClinic;
			if (medicalClinic != null)
			{
				medicalClinic.Update(deltaTime);
			}
			if (Level.Loaded != null)
			{
				if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
				{
					LevelData levelData;
					Submarine leavingSub;
					CampaignMode.TransitionType transitionType = base.GetAvailableTransition(out levelData, out leavingSub);
					if (transitionType == CampaignMode.TransitionType.End || (Level.Loaded.IsEndBiome && transitionType == CampaignMode.TransitionType.ProgressToNextLocation))
					{
						base.LoadNewLevel();
					}
					else
					{
						if (GameMain.Server.ConnectedClients.Count != 0)
						{
							if (!GameMain.Server.ConnectedClients.Any((Client c) => c.InGame && c.Character != null && !c.Character.IsDead))
							{
								goto IL_160;
							}
						}
						if (transitionType == CampaignMode.TransitionType.ProgressToNextLocation && Level.Loaded.EndOutpost != null && Level.Loaded.EndOutpost.DockedTo.Contains(leavingSub))
						{
							base.LoadNewLevel();
						}
						else if (transitionType == CampaignMode.TransitionType.ReturnToPreviousLocation && Level.Loaded.StartOutpost != null && Level.Loaded.StartOutpost.DockedTo.Contains(leavingSub))
						{
							base.LoadNewLevel();
						}
					}
				}
				else if (Level.Loaded.IsEndBiome)
				{
					LevelData levelData;
					Submarine leavingSub2;
					CampaignMode.TransitionType transitionType2 = base.GetAvailableTransition(out levelData, out leavingSub2);
					if (transitionType2 == CampaignMode.TransitionType.ProgressToNextLocation)
					{
						base.LoadNewLevel();
					}
				}
				else if (Level.Loaded.Type == LevelData.LevelType.Outpost)
				{
					base.KeepCharactersCloseToOutpost(deltaTime);
				}
			}
			IL_160:
			this.UpdateClientsToCheck(deltaTime);
			this.UpdateWallets();
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0003BE28 File Offset: 0x0003A028
		private void UpdateClientsToCheck(float deltaTime)
		{
			if (this.clientCheckTimer < 10f)
			{
				this.clientCheckTimer += deltaTime;
				return;
			}
			this.clientCheckTimer = 0f;
			this.walletsToCheck.Clear();
			this.walletsToCheck.Add(0, this.Bank);
			foreach (Character character in GameSession.GetSessionCrewCharacters(CharacterType.Player))
			{
				this.walletsToCheck.Add(character.ID, character.Wallet);
			}
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x0003BED0 File Offset: 0x0003A0D0
		private void UpdateWallets()
		{
			foreach (KeyValuePair<ushort, Wallet> keyValuePair in this.walletsToCheck)
			{
				ushort num;
				Wallet wallet2;
				keyValuePair.Deconstruct(out num, out wallet2);
				ushort id = num;
				Wallet wallet = wallet2;
				if (wallet.HasTransactions())
				{
					NetWalletTransaction transaction = wallet.DequeueAndMergeTransactions(id);
					if (wallet.ShouldForceUpdate || !transaction.ChangedData.BalanceChanged.IsNone() || !transaction.ChangedData.RewardDistributionChanged.IsNone())
					{
						this.transactions.Add(transaction);
						wallet.ShouldForceUpdate = false;
					}
				}
			}
			if (this.transactions.Count == 0)
			{
				return;
			}
			NetWalletUpdate walletUpdate = new NetWalletUpdate
			{
				Transactions = this.transactions.ToArray<NetWalletTransaction>()
			};
			this.transactions.Clear();
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				IWriteMessage msg = new WriteOnlyMessage().WithHeader(ServerPacketHeader.MONEY);
				((INetSerializableStruct)walletUpdate).Write(msg);
				GameServer server = GameMain.Server;
				if (server != null)
				{
					ServerPeer serverPeer = server.ServerPeer;
					if (serverPeer != null)
					{
						serverPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
					}
				}
			}
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x0003C038 File Offset: 0x0003A238
		public override void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
			this.GameOver = !GameMain.Server.ConnectedClients.Any((Client c) => c.InGame && c.Character != null && !c.Character.IsDead);
			base.End(transitionType);
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x0003C078 File Offset: 0x0003A278
		private bool IsFlagRequired(Client c, MultiPlayerCampaign.NetFlags flag)
		{
			ushort id;
			return !c.LastRecvCampaignUpdate.TryGetValue(flag, out id) || NetIdUtils.IdMoreRecent(this.GetLastUpdateIdForFlag(flag), id);
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0003C0A4 File Offset: 0x0003A2A4
		public void ServerWrite(IWriteMessage msg, Client c)
		{
			MultiPlayerCampaign.<>c__DisplayClass57_0 CS$<>8__locals1 = new MultiPlayerCampaign.<>c__DisplayClass57_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.c = c2;
			MultiPlayerCampaign.NetFlags requiredFlags = (from k in this.lastUpdateID.Keys
			where CS$<>8__locals1.<>4__this.IsFlagRequired(CS$<>8__locals1.c, k)
			select k).Aggregate((MultiPlayerCampaign.NetFlags)0, (MultiPlayerCampaign.NetFlags f1, MultiPlayerCampaign.NetFlags f2) => f1 | f2);
			msg.WriteUInt16((ushort)requiredFlags);
			msg.WriteBoolean(base.IsFirstRound);
			msg.WriteByte(this.CampaignID);
			msg.WriteByte(this.RoundID);
			msg.WriteUInt16(this.lastSaveID);
			msg.WriteString(this.map.Seed);
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.Misc))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc));
				msg.WriteBoolean(this.PurchasedHullRepairs);
				msg.WriteBoolean(this.PurchasedItemRepairs);
				msg.WriteBoolean(this.PurchasedLostShuttles);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.MapAndMissions))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions));
				msg.WriteBoolean(this.ForceMapUI);
				msg.WriteBoolean(this.map.AllowDebugTeleport);
				msg.WriteUInt16((this.map.CurrentLocationIndex == -1) ? ushort.MaxValue : ((ushort)this.map.CurrentLocationIndex));
				msg.WriteUInt16((this.map.SelectedLocationIndex == -1) ? ushort.MaxValue : ((ushort)this.map.SelectedLocationIndex));
				if (this.map.CurrentLocation != null)
				{
					msg.WriteByte((byte)this.map.CurrentLocation.AvailableMissions.Count<Mission>());
					using (IEnumerator<Mission> enumerator = this.map.CurrentLocation.AvailableMissions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Mission mission = enumerator.Current;
							msg.WriteIdentifier(mission.Prefab.Identifier);
							if (mission.Locations[0] == mission.Locations[1])
							{
								msg.WriteByte(byte.MaxValue);
							}
							else
							{
								Location missionDestination = (mission.Locations[0] == this.map.CurrentLocation) ? mission.Locations[1] : mission.Locations[0];
								LocationConnection connection = this.map.CurrentLocation.Connections.Find((LocationConnection c) => c.OtherLocation(CS$<>8__locals1.<>4__this.map.CurrentLocation) == missionDestination);
								msg.WriteByte((byte)this.map.CurrentLocation.Connections.IndexOf(connection));
							}
						}
						goto IL_295;
					}
				}
				msg.WriteByte(0);
				IL_295:
				IEnumerable<int> selectedMissionIndices = this.map.GetSelectedMissionIndices();
				msg.WriteByte((byte)selectedMissionIndices.Count<int>());
				foreach (int selectedMissionIndex in selectedMissionIndices)
				{
					msg.WriteByte((byte)selectedMissionIndex);
				}
				CS$<>8__locals1.<ServerWrite>g__WriteStores|2(msg);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.SubList))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.SubList));
				IReadOnlyList<SubmarineInfo> subList = GameMain.NetLobbyScreen.GetSubList();
				List<int> ownedSubmarineIndices = new List<int>();
				int j;
				int i;
				Func<SubmarineInfo, bool> <>9__4;
				for (i = 0; i < subList.Count; i = j + 1)
				{
					IEnumerable<SubmarineInfo> ownedSubmarines = GameMain.GameSession.OwnedSubmarines;
					Func<SubmarineInfo, bool> predicate;
					if ((predicate = <>9__4) == null)
					{
						predicate = (<>9__4 = ((SubmarineInfo s) => s.Name == subList[i].Name));
					}
					if (ownedSubmarines.Any(predicate))
					{
						ownedSubmarineIndices.Add(i);
					}
					j = i;
				}
				msg.WriteUInt16((ushort)ownedSubmarineIndices.Count);
				foreach (int index in ownedSubmarineIndices)
				{
					msg.WriteUInt16((ushort)index);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.UpgradeManager))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.UpgradeManager));
				msg.WriteUInt16((ushort)this.UpgradeManager.PendingUpgrades.Count);
				foreach (PurchasedUpgrade purchasedUpgrade in this.UpgradeManager.PendingUpgrades)
				{
					int j;
					UpgradePrefab upgradePrefab;
					UpgradeCategory upgradeCategory;
					purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out j);
					UpgradePrefab prefab = upgradePrefab;
					UpgradeCategory category = upgradeCategory;
					int level = j;
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
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate));
				MultiPlayerCampaign.WriteItems(msg, this.CargoManager.ItemsInBuyCrate);
				CS$<>8__locals1.<ServerWrite>g__WriteStores|2(msg);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.ItemsInSellFromSubCrate))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.ItemsInSellFromSubCrate));
				MultiPlayerCampaign.WriteItems(msg, this.CargoManager.ItemsInSellFromSubCrate);
				CS$<>8__locals1.<ServerWrite>g__WriteStores|2(msg);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.PurchasedItems))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.PurchasedItems));
				MultiPlayerCampaign.WriteItems(msg, this.CargoManager.PurchasedItems);
				CS$<>8__locals1.<ServerWrite>g__WriteStores|2(msg);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.SoldItems))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.SoldItems));
				MultiPlayerCampaign.WriteItems(msg, this.CargoManager.SoldItems);
				CS$<>8__locals1.<ServerWrite>g__WriteStores|2(msg);
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.Reputation))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Reputation));
				msg.WriteByte((byte)base.Factions.Count);
				foreach (Faction faction in base.Factions)
				{
					msg.WriteIdentifier(faction.Prefab.Identifier);
					msg.WriteSingle(faction.Reputation.Value);
				}
			}
			if (requiredFlags.HasFlag(MultiPlayerCampaign.NetFlags.CharacterInfo))
			{
				msg.WriteUInt16(this.GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.CharacterInfo));
				CharacterCampaignData characterData = this.GetClientCharacterData(CS$<>8__locals1.c);
				if (((characterData != null) ? characterData.CharacterInfo : null) == null)
				{
					msg.WriteBoolean(false);
					return;
				}
				msg.WriteBoolean(true);
				characterData.CharacterInfo.ServerWrite(msg);
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0003C7E8 File Offset: 0x0003A9E8
		public void ServerRead(IReadMessage msg, Client sender)
		{
			ushort currentLocIndex = msg.ReadUInt16();
			ushort selectedLocIndex = msg.ReadUInt16();
			byte selectedMissionCount = msg.ReadByte();
			List<int> selectedMissionIndices = new List<int>();
			for (int i = 0; i < (int)selectedMissionCount; i++)
			{
				selectedMissionIndices.Add((int)msg.ReadByte());
			}
			bool purchasedHullRepairs = msg.ReadBoolean();
			bool purchasedItemRepairs = msg.ReadBoolean();
			bool purchasedLostShuttles = msg.ReadBoolean();
			Dictionary<Identifier, List<PurchasedItem>> buyCrateItems = MultiPlayerCampaign.ReadPurchasedItems(msg, sender);
			Dictionary<Identifier, List<PurchasedItem>> subSellCrateItems = MultiPlayerCampaign.ReadPurchasedItems(msg, sender);
			Dictionary<Identifier, List<PurchasedItem>> purchasedItems = MultiPlayerCampaign.ReadPurchasedItems(msg, sender);
			Dictionary<Identifier, List<SoldItem>> soldItems = MultiPlayerCampaign.ReadSoldItems(msg);
			ushort purchasedUpgradeCount = msg.ReadUInt16();
			List<PurchasedUpgrade> purchasedUpgrades = new List<PurchasedUpgrade>();
			for (int j = 0; j < (int)purchasedUpgradeCount; j++)
			{
				Identifier upgradeIdentifier = msg.ReadIdentifier();
				UpgradePrefab prefab = UpgradePrefab.Find(upgradeIdentifier);
				Identifier categoryIdentifier = msg.ReadIdentifier();
				UpgradeCategory category = UpgradeCategory.Find(categoryIdentifier);
				int upgradeLevel = (int)msg.ReadByte();
				if (category != null && prefab != null)
				{
					purchasedUpgrades.Add(new PurchasedUpgrade(prefab, category, upgradeLevel));
				}
			}
			ushort purchasedItemSwapCount = msg.ReadUInt16();
			List<PurchasedItemSwap> purchasedItemSwaps = new List<PurchasedItemSwap>();
			for (int k = 0; k < (int)purchasedItemSwapCount; k++)
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
			if (purchasedUpgradeCount > 0 || purchasedItemSwapCount > 0)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.UpgradeManager);
			}
			int hullRepairCost = CampaignMode.GetHullRepairCost();
			int itemRepairCost = CampaignMode.GetItemRepairCost();
			int shuttleRetrieveCost = 1000;
			Location location = base.Map.CurrentLocation;
			if (location != null)
			{
				hullRepairCost = location.GetAdjustedMechanicalCost(hullRepairCost);
				itemRepairCost = location.GetAdjustedMechanicalCost(itemRepairCost);
				shuttleRetrieveCost = location.GetAdjustedMechanicalCost(shuttleRetrieveCost);
			}
			Wallet personalWallet = this.GetWallet(sender);
			if (personalWallet != null)
			{
				personalWallet.ForceUpdate();
			}
			if (CampaignMode.AllowedToManageWallets(sender))
			{
				this.Bank.ForceUpdate();
			}
			if (purchasedHullRepairs && !this.PurchasedHullRepairs && this.GetBalance(sender) >= hullRepairCost)
			{
				this.TryPurchase(sender, hullRepairCost);
				this.PurchasedHullRepairs = true;
				GameAnalyticsManager.AddMoneySpentEvent(hullRepairCost, GameAnalyticsManager.MoneySink.Service, "hullrepairs");
			}
			if (purchasedItemRepairs && !this.PurchasedItemRepairs && this.GetBalance(sender) >= itemRepairCost)
			{
				this.TryPurchase(sender, itemRepairCost);
				this.PurchasedItemRepairs = true;
				GameAnalyticsManager.AddMoneySpentEvent(itemRepairCost, GameAnalyticsManager.MoneySink.Service, "devicerepairs");
			}
			if (purchasedLostShuttles && !this.PurchasedLostShuttles)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.SubmarineInfo : null) != null && GameMain.GameSession.SubmarineInfo.LeftBehindSubDockingPortOccupied)
				{
					GameMain.Server.SendDirectChatMessage(TextManager.FormatServerMessage("ReplaceShuttleDockingPortOccupied"), sender, ChatMessageType.MessageBox);
				}
				else if (this.TryPurchase(sender, shuttleRetrieveCost))
				{
					this.PurchasedLostShuttles = true;
					GameAnalyticsManager.AddMoneySpentEvent(shuttleRetrieveCost, GameAnalyticsManager.MoneySink.Service, "retrieveshuttle");
				}
			}
			if ((int)currentLocIndex < base.Map.Locations.Count && base.Map.AllowDebugTeleport)
			{
				base.Map.SetLocation((int)currentLocIndex);
			}
			if (CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageMap))
			{
				base.Map.SelectLocation((selectedLocIndex == ushort.MaxValue) ? -1 : ((int)selectedLocIndex));
				if (base.Map.SelectedLocation == null)
				{
					base.Map.SelectRandomLocation(true);
				}
				if (base.Map.SelectedConnection != null)
				{
					base.Map.SelectMission(selectedMissionIndices);
				}
				base.CheckTooManyMissions(base.Map.CurrentLocation, sender);
			}
			if (this.HasCampaignInteractionAvailable(sender, CampaignMode.InteractionType.Store))
			{
				MultiPlayerCampaign.<>c__DisplayClass58_0 CS$<>8__locals1 = new MultiPlayerCampaign.<>c__DisplayClass58_0();
				Dictionary<Identifier, List<PurchasedItem>> prevBuyCrateItems = new Dictionary<Identifier, List<PurchasedItem>>();
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> kvp in this.CargoManager.ItemsInBuyCrate)
				{
					prevBuyCrateItems.Add(kvp.Key, new List<PurchasedItem>(kvp.Value));
				}
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> store in prevBuyCrateItems)
				{
					foreach (PurchasedItem item4 in store.Value.ToList<PurchasedItem>())
					{
						this.CargoManager.ModifyItemQuantityInBuyCrate(store.Key, item4.ItemPrefab, -item4.Quantity, sender);
					}
				}
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> store2 in buyCrateItems)
				{
					using (List<PurchasedItem>.Enumerator enumerator5 = store2.Value.ToList<PurchasedItem>().GetEnumerator())
					{
						while (enumerator5.MoveNext())
						{
							PurchasedItem item = enumerator5.Current;
							Map map = this.map;
							bool flag;
							if (map == null)
							{
								flag = (null != null);
							}
							else
							{
								Location currentLocation = map.CurrentLocation;
								flag = (((currentLocation != null) ? currentLocation.Stores : null) != null);
							}
							if (flag && this.map.CurrentLocation.Stores.ContainsKey(store2.Key))
							{
								PurchasedItem purchasedItem3 = this.map.CurrentLocation.Stores[store2.Key].Stock.Find((PurchasedItem s) => s.ItemPrefab == item.ItemPrefab);
								int availableQuantity = (purchasedItem3 != null) ? purchasedItem3.Quantity : 0;
								PurchasedItem buyCrateItem = this.CargoManager.GetBuyCrateItem(store2.Key, item.ItemPrefab);
								int alreadyPurchasedQuantity = (buyCrateItem != null) ? buyCrateItem.Quantity : this.CargoManager.GetPurchasedItemCount(store2.Key, item.ItemPrefab);
								item.Quantity = MathHelper.Clamp(item.Quantity, 0, availableQuantity - alreadyPurchasedQuantity);
								this.CargoManager.ModifyItemQuantityInBuyCrate(store2.Key, item.ItemPrefab, item.Quantity, sender);
							}
						}
					}
				}
				Dictionary<Identifier, List<PurchasedItem>> prevPurchasedItems = new Dictionary<Identifier, List<PurchasedItem>>();
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> kvp2 in this.CargoManager.PurchasedItems)
				{
					prevPurchasedItems.Add(kvp2.Key, new List<PurchasedItem>(kvp2.Value));
				}
				foreach (Identifier storeId in purchasedItems.Keys)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Purchased items (");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(storeId);
					defaultInterpolatedStringHandler.AppendLiteral("):\n");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					List<PurchasedItem> alreadyPurchased;
					if (prevPurchasedItems.TryGetValue(storeId, out alreadyPurchased))
					{
						IEnumerable<PurchasedItem> delivered = from it in alreadyPurchased
						where it.Delivered
						select it;
						IEnumerable<PurchasedItem> notDelivered = from it in alreadyPurchased
						where !it.Delivered
						select it;
						if (delivered.Any<PurchasedItem>())
						{
							DebugConsole.Log("  Already delivered:\n" + string.Concat(delivered.Select(delegate(PurchasedItem it)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(10, 2);
								defaultInterpolatedStringHandler4.AppendLiteral("    - ");
								defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(it.ItemPrefab.Name);
								defaultInterpolatedStringHandler4.AppendLiteral(" (x");
								defaultInterpolatedStringHandler4.AppendFormatted<int>(it.Quantity);
								defaultInterpolatedStringHandler4.AppendLiteral(")");
								return defaultInterpolatedStringHandler4.ToStringAndClear();
							})));
						}
						if (notDelivered.Any<PurchasedItem>())
						{
							DebugConsole.Log("  Already purchased:\n" + string.Concat((from it in notDelivered
							where !it.Delivered
							select it).Select(delegate(PurchasedItem it)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(10, 2);
								defaultInterpolatedStringHandler4.AppendLiteral("    - ");
								defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(it.ItemPrefab.Name);
								defaultInterpolatedStringHandler4.AppendLiteral(" (x");
								defaultInterpolatedStringHandler4.AppendFormatted<int>(it.Quantity);
								defaultInterpolatedStringHandler4.AppendLiteral(")");
								return defaultInterpolatedStringHandler4.ToStringAndClear();
							})));
						}
					}
					DebugConsole.Log("  New purchases:");
					using (List<PurchasedItem>.Enumerator enumerator8 = purchasedItems[storeId].GetEnumerator())
					{
						while (enumerator8.MoveNext())
						{
							PurchasedItem purchasedItem = enumerator8.Current;
							if (!purchasedItem.Delivered)
							{
								int quantity = purchasedItem.Quantity;
								if (alreadyPurchased != null)
								{
									quantity -= (from it in alreadyPurchased
									where it.DeliverImmediately == purchasedItem.DeliverImmediately && it.ItemPrefab == purchasedItem.ItemPrefab
									select it).Sum((PurchasedItem it) => it.Quantity);
								}
								if (quantity > 0)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 2);
									defaultInterpolatedStringHandler2.AppendLiteral("    - ");
									defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(purchasedItem.ItemPrefab.Name);
									defaultInterpolatedStringHandler2.AppendLiteral(" (x");
									defaultInterpolatedStringHandler2.AppendFormatted<int>(quantity);
									defaultInterpolatedStringHandler2.AppendLiteral(")");
									DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
								}
							}
						}
					}
				}
				foreach (Identifier storeId2 in soldItems.Keys)
				{
					DebugConsole.Log("Sold items:\n" + string.Concat(soldItems[storeId2].Select(delegate(SoldItem it)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler4.AppendLiteral(" - ");
						defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(it.ItemPrefab.Name);
						return defaultInterpolatedStringHandler4.ToStringAndClear();
					})));
				}
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> kvp3 in purchasedItems)
				{
					Identifier storeId3 = kvp3.Key;
					List<PurchasedItem> purchasedItemList = kvp3.Value;
					using (List<PurchasedItem>.Enumerator enumerator11 = purchasedItemList.GetEnumerator())
					{
						while (enumerator11.MoveNext())
						{
							PurchasedItem purchasedItem = enumerator11.Current;
							int desiredQuantity = purchasedItem.Quantity;
							List<PurchasedItem> alreadyPurchasedList;
							if (prevPurchasedItems.TryGetValue(storeId3, out alreadyPurchasedList))
							{
								PurchasedItem alreadyPurchased2 = alreadyPurchasedList.FirstOrDefault((PurchasedItem p) => p.ItemPrefab == purchasedItem.ItemPrefab && p.DeliverImmediately == purchasedItem.DeliverImmediately);
								if (alreadyPurchased2 != null)
								{
									desiredQuantity -= alreadyPurchased2.Quantity;
								}
							}
							PurchasedItem purchasedItem2 = this.map.CurrentLocation.Stores[storeId3].Stock.Find((PurchasedItem s) => s.ItemPrefab == purchasedItem.ItemPrefab);
							int availableQuantity2 = (purchasedItem2 != null) ? purchasedItem2.Quantity : 0;
							purchasedItem.Quantity = Math.Min(desiredQuantity, availableQuantity2);
						}
					}
					this.CargoManager.PurchaseItems(storeId3, purchasedItemList, false, sender);
				}
				foreach (KeyValuePair<Identifier, List<PurchasedItem>> keyValuePair in this.CargoManager.PurchasedItems)
				{
					Identifier identifier;
					List<PurchasedItem> list;
					keyValuePair.Deconstruct(out identifier, out list);
					Identifier storeIdentifier = identifier;
					List<PurchasedItem> items = list;
					if (!prevPurchasedItems.ContainsKey(storeIdentifier))
					{
						this.CargoManager.LogNewItemPurchases(storeIdentifier, items, sender);
					}
					else
					{
						List<PurchasedItem> newItems = new List<PurchasedItem>();
						List<PurchasedItem> prevItems = prevPurchasedItems[storeIdentifier];
						using (List<PurchasedItem>.Enumerator enumerator13 = items.GetEnumerator())
						{
							while (enumerator13.MoveNext())
							{
								PurchasedItem item = enumerator13.Current;
								PurchasedItem matching = prevItems.FirstOrDefault((PurchasedItem ppi) => ppi.ItemPrefab == item.ItemPrefab);
								if (matching == null)
								{
									newItems.Add(item);
								}
								else if (matching.Quantity < item.Quantity)
								{
									newItems.Add(new PurchasedItem(item.ItemPrefab, item.Quantity - matching.Quantity, sender));
								}
							}
						}
						if (newItems.Any<PurchasedItem>())
						{
							this.CargoManager.LogNewItemPurchases(storeIdentifier, newItems, sender);
						}
					}
				}
				bool allowedToSellSubItems = CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.SellSubItems);
				if (allowedToSellSubItems)
				{
					Dictionary<Identifier, List<PurchasedItem>> prevSubSellCrateItems = new Dictionary<Identifier, List<PurchasedItem>>(this.CargoManager.ItemsInSellFromSubCrate);
					foreach (KeyValuePair<Identifier, List<PurchasedItem>> store3 in prevSubSellCrateItems)
					{
						foreach (PurchasedItem item2 in store3.Value.ToList<PurchasedItem>())
						{
							this.CargoManager.ModifyItemQuantityInSubSellCrate(store3.Key, item2.ItemPrefab, -item2.Quantity, sender);
						}
					}
					foreach (KeyValuePair<Identifier, List<PurchasedItem>> store4 in subSellCrateItems)
					{
						foreach (PurchasedItem item3 in store4.Value.ToList<PurchasedItem>())
						{
							this.CargoManager.ModifyItemQuantityInSubSellCrate(store4.Key, item3.ItemPrefab, item3.Quantity, sender);
						}
					}
				}
				CS$<>8__locals1.allowedToSellInventoryItems = CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.SellInventoryItems);
				if (CS$<>8__locals1.allowedToSellInventoryItems && allowedToSellSubItems)
				{
					Dictionary<Identifier, List<SoldItem>> prevSoldItems = new Dictionary<Identifier, List<SoldItem>>(this.CargoManager.SoldItems);
					foreach (KeyValuePair<Identifier, List<SoldItem>> store5 in prevSoldItems)
					{
						this.CargoManager.BuyBackSoldItems(store5.Key, store5.Value.ToList<SoldItem>(), sender);
					}
					using (Dictionary<Identifier, List<SoldItem>>.Enumerator enumerator19 = soldItems.GetEnumerator())
					{
						while (enumerator19.MoveNext())
						{
							KeyValuePair<Identifier, List<SoldItem>> store6 = enumerator19.Current;
							this.CargoManager.SellItems(store6.Key, store6.Value.ToList<SoldItem>(), sender);
						}
						goto IL_E87;
					}
				}
				if (CS$<>8__locals1.allowedToSellInventoryItems || allowedToSellSubItems)
				{
					Dictionary<Identifier, List<SoldItem>> prevSoldItems2 = new Dictionary<Identifier, List<SoldItem>>(this.CargoManager.SoldItems);
					foreach (KeyValuePair<Identifier, List<SoldItem>> store7 in prevSoldItems2)
					{
						store7.Value.RemoveAll(new Predicate<SoldItem>(CS$<>8__locals1.<ServerRead>g__predicate|12));
						this.CargoManager.BuyBackSoldItems(store7.Key, store7.Value.ToList<SoldItem>(), sender);
					}
					foreach (KeyValuePair<Identifier, List<SoldItem>> store8 in soldItems)
					{
						store8.Value.RemoveAll(new Predicate<SoldItem>(CS$<>8__locals1.<ServerRead>g__predicate|12));
					}
					foreach (KeyValuePair<Identifier, List<SoldItem>> store9 in soldItems)
					{
						this.CargoManager.SellItems(store9.Key, store9.Value.ToList<SoldItem>(), sender);
					}
				}
			}
			IL_E87:
			if ((purchasedUpgrades.Any<PurchasedUpgrade>() || purchasedItemSwaps.Any<PurchasedItemSwap>()) && this.HasCampaignInteractionAvailable(sender, CampaignMode.InteractionType.Upgrade))
			{
				ImmutableHashSet<Character> characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
				foreach (PurchasedUpgrade purchasedUpgrade in purchasedUpgrades)
				{
					UpgradePrefab upgradePrefab;
					UpgradeCategory upgradeCategory;
					int num;
					purchasedUpgrade.Deconstruct(out upgradePrefab, out upgradeCategory, out num);
					UpgradePrefab prefab2 = upgradePrefab;
					UpgradeCategory category2 = upgradeCategory;
					this.UpgradeManager.TryPurchaseUpgrade(prefab2, category2, false, sender);
					UpgradePrice price2 = prefab2.Price;
					UpgradePrefab prefab3 = prefab2;
					int upgradeLevel2 = this.UpgradeManager.GetUpgradeLevel(prefab2, category2, null);
					Map map2 = base.Map;
					int price = price2.GetBuyPrice(prefab3, upgradeLevel2, (map2 != null) ? map2.CurrentLocation : null, characterList);
					int level = this.UpgradeManager.GetUpgradeLevel(prefab2, category2, null);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(31, 4);
					defaultInterpolatedStringHandler3.AppendLiteral("SERVER: Purchased level ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(level);
					defaultInterpolatedStringHandler3.AppendLiteral(" ");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(category2.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(".");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(prefab2.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(" for ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(price);
					GameServer.Log(defaultInterpolatedStringHandler3.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
				}
				foreach (PurchasedItemSwap purchasedItemSwap in purchasedItemSwaps)
				{
					if (purchasedItemSwap.ItemToInstall == null)
					{
						this.UpgradeManager.CancelItemSwap(purchasedItemSwap.ItemToRemove, false, sender);
					}
					else
					{
						this.UpgradeManager.PurchaseItemSwap(purchasedItemSwap.ItemToRemove, purchasedItemSwap.ItemToInstall, false, sender);
					}
				}
				using (List<Item>.Enumerator enumerator25 = Item.ItemList.GetEnumerator())
				{
					while (enumerator25.MoveNext())
					{
						Item item = enumerator25.Current;
						if (item.PendingItemSwap != null && !purchasedItemSwaps.Any((PurchasedItemSwap it) => it.ItemToRemove == item))
						{
							this.UpgradeManager.CancelItemSwap(item, false, null);
							item.PendingItemSwap = null;
						}
					}
				}
			}
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x0003DB00 File Offset: 0x0003BD00
		private bool HasCampaignInteractionAvailable(Client sender, CampaignMode.InteractionType interactionType)
		{
			if (sender.Character == null || sender.Character.IsIncapacitated)
			{
				return false;
			}
			GameServer server = GameMain.Server;
			ServerSettings serverSettings = (server != null) ? server.ServerSettings : null;
			if (serverSettings != null && serverSettings.AllowRemoteCampaignInteractions)
			{
				return true;
			}
			foreach (Character otherCharacter in Character.CharacterList)
			{
				if (otherCharacter.CampaignInteractionType == interactionType && sender.Character.CanInteractWith(otherCharacter, 250f, true, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x0003DBA8 File Offset: 0x0003BDA8
		public void ServerReadMoney(IReadMessage msg, Client sender)
		{
			MultiPlayerCampaign.<>c__DisplayClass60_0 CS$<>8__locals1;
			CS$<>8__locals1.sender = sender;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.transfer = INetSerializableStruct.Read<NetWalletTransfer>(msg);
			if (GameMain.Server == null)
			{
				return;
			}
			if (CS$<>8__locals1.transfer.Amount <= 0)
			{
				return;
			}
			ushort id;
			if (CS$<>8__locals1.transfer.Sender.TryUnwrap(out id))
			{
				if (id != CS$<>8__locals1.sender.CharacterID && !CampaignMode.AllowedToManageWallets(CS$<>8__locals1.sender))
				{
					return;
				}
				Wallet wallet = MultiPlayerCampaign.<ServerReadMoney>g__GetWalletByID|60_1(id);
				if (wallet is InvalidWallet)
				{
					return;
				}
				this.<ServerReadMoney>g__TransferMoney|60_0(wallet, ref CS$<>8__locals1);
				return;
			}
			else
			{
				if (!CampaignMode.AllowedToManageWallets(CS$<>8__locals1.sender))
				{
					ushort receiverId;
					if (CS$<>8__locals1.transfer.Receiver.TryUnwrap(out receiverId) && receiverId == CS$<>8__locals1.sender.CharacterID)
					{
						if (CS$<>8__locals1.transfer.Amount > GameMain.Server.ServerSettings.MaximumMoneyTransferRequest)
						{
							return;
						}
						GameMain.Server.Voting.StartTransferVote(CS$<>8__locals1.sender, null, CS$<>8__locals1.transfer.Amount, CS$<>8__locals1.sender);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
						defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.sender.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" started a vote to transfer ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.transfer.Amount);
						defaultInterpolatedStringHandler.AppendLiteral(" mk from the bank.");
						GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
					}
					return;
				}
				this.<ServerReadMoney>g__TransferMoney|60_0(this.Bank, ref CS$<>8__locals1);
				return;
			}
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x0003DD14 File Offset: 0x0003BF14
		public void ServerReadRewardDistribution(IReadMessage msg, Client sender)
		{
			NetWalletSetSalaryUpdate update = INetSerializableStruct.Read<NetWalletSetSalaryUpdate>(msg);
			if (!CampaignMode.AllowedToManageWallets(sender))
			{
				return;
			}
			ushort id;
			if (update.Target.TryUnwrap(out id))
			{
				Character targetCharacter = Character.CharacterList.FirstOrDefault((Character c) => c.ID == id);
				if (targetCharacter != null)
				{
					targetCharacter.Wallet.SetRewardDistribution(update.NewRewardDistribution);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 3);
				defaultInterpolatedStringHandler.AppendFormatted(sender.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" changed the salary of ");
				defaultInterpolatedStringHandler.AppendFormatted((targetCharacter != null) ? targetCharacter.Name : null);
				defaultInterpolatedStringHandler.AppendLiteral(" to ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(update.NewRewardDistribution);
				defaultInterpolatedStringHandler.AppendLiteral("%.");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
				return;
			}
			this.Bank.SetRewardDistribution(update.NewRewardDistribution);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(33, 2);
			defaultInterpolatedStringHandler2.AppendFormatted(sender.Name);
			defaultInterpolatedStringHandler2.AppendLiteral(" changed the default salary to ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(update.NewRewardDistribution);
			defaultInterpolatedStringHandler2.AppendLiteral("%.");
			GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.Money);
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x0003DE3C File Offset: 0x0003C03C
		public void ResetSalaries(Client sender)
		{
			if (!CampaignMode.AllowedToManageWallets(sender))
			{
				return;
			}
			foreach (Character character in GameSession.GetSessionCrewCharacters(CharacterType.Player))
			{
				character.Wallet.SetRewardDistribution(this.Bank.RewardDistribution);
			}
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x0003DEA8 File Offset: 0x0003C0A8
		public void ServerReadCrew(IReadMessage msg, Client sender)
		{
			MultiPlayerCampaign.<>c__DisplayClass63_0 CS$<>8__locals1 = new MultiPlayerCampaign.<>c__DisplayClass63_0();
			ushort[] pendingHires = null;
			bool[] pendingToReserveBench = null;
			bool updatePending = msg.ReadBoolean();
			if (updatePending)
			{
				ushort pendingHireLength = msg.ReadUInt16();
				pendingHires = new ushort[(int)pendingHireLength];
				pendingToReserveBench = new bool[(int)pendingHireLength];
				for (int i = 0; i < (int)pendingHireLength; i++)
				{
					pendingHires[i] = msg.ReadUInt16();
					pendingToReserveBench[i] = msg.ReadBoolean();
				}
			}
			bool validateHires = msg.ReadBoolean();
			bool renameCharacter = msg.ReadBoolean();
			CS$<>8__locals1.renamedIdentifier = 0;
			string newName = null;
			bool existingCrewMember = false;
			if (renameCharacter)
			{
				CS$<>8__locals1.renamedIdentifier = msg.ReadUInt16();
				newName = Client.SanitizeName(msg.ReadString(), 32);
				existingCrewMember = msg.ReadBoolean();
				GameServer server = GameMain.Server;
				string newName2 = newName;
				int renamedIdentifier = (int)CS$<>8__locals1.renamedIdentifier;
				CharacterInfo characterInfo2 = sender.CharacterInfo;
				ushort? num = (characterInfo2 != null) ? new ushort?(characterInfo2.ID) : null;
				int? num2 = (num != null) ? new int?((int)num.GetValueOrDefault()) : null;
				if (!server.IsNameValid(sender, newName2, renamedIdentifier == num2.GetValueOrDefault() & num2 != null))
				{
					renameCharacter = false;
				}
			}
			bool fireCharacter = msg.ReadBoolean();
			CS$<>8__locals1.firedIdentifier = -1;
			if (fireCharacter)
			{
				CS$<>8__locals1.firedIdentifier = (int)msg.ReadUInt16();
			}
			MultiPlayerCampaign.<>c__DisplayClass63_0 CS$<>8__locals2 = CS$<>8__locals1;
			Map map = this.map;
			CS$<>8__locals2.location = ((map != null) ? map.CurrentLocation : null);
			CharacterInfo firedCharacter = null;
			ValueTuple<ushort, string> appliedRename = new ValueTuple<ushort, string>(0, string.Empty);
			if (CS$<>8__locals1.location != null)
			{
				if (fireCharacter && CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageHires) && this.HasCampaignInteractionAvailable(sender, CampaignMode.InteractionType.Crew))
				{
					firedCharacter = base.CrewManager.GetCharacterInfos(true).FirstOrDefault((CharacterInfo info) => (int)info.ID == CS$<>8__locals1.firedIdentifier);
					if (firedCharacter != null)
					{
						Character character = firedCharacter.Character;
						if (character == null || character.IsBot)
						{
							base.CrewManager.FireCharacter(firedCharacter);
							goto IL_21F;
						}
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Tried to fire an invalid character (");
					defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.firedIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					GameServer.Log(sender.Name + " attempted to fire a character without having access to an appropriate NPC.", ServerLog.MessageType.Error);
				}
				IL_21F:
				if (renameCharacter)
				{
					CharacterInfo characterInfo = null;
					if (CampaignMode.AllowedToManageCampaign(sender, ClientPermissions.ManageHires))
					{
						if (existingCrewMember && base.CrewManager != null)
						{
							characterInfo = base.CrewManager.GetCharacterInfos(true).FirstOrDefault((CharacterInfo info) => info.ID == CS$<>8__locals1.renamedIdentifier);
						}
						else if (!existingCrewMember && CS$<>8__locals1.location.HireManager != null)
						{
							characterInfo = CS$<>8__locals1.location.HireManager.AvailableCharacters.FirstOrDefault((CharacterInfo info) => info.ID == CS$<>8__locals1.renamedIdentifier);
						}
						if (characterInfo != null && characterInfo != sender.CharacterInfo && !this.HasCampaignInteractionAvailable(sender, CampaignMode.InteractionType.Crew))
						{
							GameServer.Log(sender.Name + " attempted to rename a character without having access to an appropriate NPC.", ServerLog.MessageType.Error);
							characterInfo = null;
						}
					}
					else if (characterInfo == null)
					{
						int renamedIdentifier2 = (int)CS$<>8__locals1.renamedIdentifier;
						CharacterInfo characterInfo3 = sender.CharacterInfo;
						ushort? num = (characterInfo3 != null) ? new ushort?(characterInfo3.ID) : null;
						int? num2 = (num != null) ? new int?((int)num.GetValueOrDefault()) : null;
						if (renamedIdentifier2 == num2.GetValueOrDefault() & num2 != null)
						{
							characterInfo = sender.CharacterInfo;
						}
					}
					if (characterInfo != null)
					{
						if (characterInfo.Character != null)
						{
							Character character2 = characterInfo.Character;
							if ((character2 == null || !character2.IsBot) && (!characterInfo.RenamingEnabled || characterInfo != sender.CharacterInfo))
							{
								goto IL_42F;
							}
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(32, 3);
						defaultInterpolatedStringHandler2.AppendFormatted(sender.Name);
						defaultInterpolatedStringHandler2.AppendLiteral(" renamed the character \"");
						defaultInterpolatedStringHandler2.AppendFormatted(characterInfo.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\" as \"");
						defaultInterpolatedStringHandler2.AppendFormatted(newName);
						defaultInterpolatedStringHandler2.AppendLiteral("\".");
						GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
						if (existingCrewMember)
						{
							base.CrewManager.RenameCharacter(characterInfo, newName);
							if (characterInfo == sender.CharacterInfo)
							{
								characterInfo.RenamingEnabled = false;
							}
						}
						else
						{
							CS$<>8__locals1.location.HireManager.RenameCharacter(characterInfo, newName);
						}
						appliedRename = new ValueTuple<ushort, string>(characterInfo.ID, newName);
						goto IL_4B9;
					}
					IL_42F:
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(41, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Tried to rename an invalid character (");
					defaultInterpolatedStringHandler3.AppendFormatted<ushort>(CS$<>8__locals1.renamedIdentifier);
					defaultInterpolatedStringHandler3.AppendLiteral(", ");
					defaultInterpolatedStringHandler3.AppendFormatted(((characterInfo != null) ? characterInfo.Name : null) ?? "null");
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					string errorMsg = defaultInterpolatedStringHandler3.ToStringAndClear();
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameServer server2 = GameMain.Server;
					if (server2 != null)
					{
						server2.SendConsoleMessage(errorMsg, sender, new Color?(Color.Red));
					}
				}
				IL_4B9:
				if (CS$<>8__locals1.location.HireManager != null && this.HasCampaignInteractionAvailable(sender, CampaignMode.InteractionType.Crew))
				{
					if (validateHires)
					{
						foreach (CharacterInfo hireInfo in CS$<>8__locals1.location.HireManager.PendingHires)
						{
							base.TryHireCharacter(CS$<>8__locals1.location, hireInfo, true, sender, false);
						}
					}
					if (updatePending)
					{
						List<CharacterInfo> pendingHireInfos = new List<CharacterInfo>();
						int j = 0;
						ushort[] array = pendingHires;
						for (int k = 0; k < array.Length; k++)
						{
							ushort identifier = array[k];
							CharacterInfo match = CS$<>8__locals1.location.GetHireableCharacters().FirstOrDefault((CharacterInfo info) => info.ID == identifier);
							if (match == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(63, 1);
								defaultInterpolatedStringHandler4.AppendLiteral("Tried to add a character that doesn't exist (");
								defaultInterpolatedStringHandler4.AppendFormatted<ushort>(identifier);
								defaultInterpolatedStringHandler4.AppendLiteral(") to pending hires");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
							}
							else
							{
								match.BotStatus = (pendingToReserveBench[j++] ? BotStatus.PendingHireToReserveBench : BotStatus.PendingHireToActiveService);
								if (match.BotStatus == BotStatus.PendingHireToActiveService)
								{
									if (pendingHireInfos.Count((CharacterInfo ci) => ci.BotStatus == BotStatus.PendingHireToActiveService) + base.CrewManager.GetCharacterInfos(false).Count<CharacterInfo>() >= 16)
									{
										goto IL_61F;
									}
								}
								pendingHireInfos.Add(match);
							}
							IL_61F:;
						}
						CS$<>8__locals1.location.HireManager.PendingHires = pendingHireInfos;
					}
					CS$<>8__locals1.location.HireManager.AvailableCharacters.ForEachMod(delegate(CharacterInfo info)
					{
						if (!CS$<>8__locals1.location.HireManager.PendingHires.Contains(info))
						{
							CS$<>8__locals1.location.HireManager.RenameCharacter(info, info.OriginalName);
						}
					});
				}
				else
				{
					GameServer.Log(sender.Name + " attempted to hire characters without having access to an appropriate NPC.", ServerLog.MessageType.Error);
				}
			}
			if (renameCharacter && existingCrewMember)
			{
				this.SendCrewState(appliedRename, firedCharacter, true);
				return;
			}
			CharacterInfo firedCharacter2 = firedCharacter;
			this.SendCrewState(default(ValueTuple<ushort, string>), firedCharacter2, true);
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0003E56C File Offset: 0x0003C76C
		public void SendCrewState([TupleElementNames(new string[]
		{
			"id",
			"newName"
		})] ValueTuple<ushort, string> renamedCrewMember = default(ValueTuple<ushort, string>), CharacterInfo firedCharacter = null, bool createNotification = true)
		{
			List<CharacterInfo> availableHires = new List<CharacterInfo>();
			List<CharacterInfo> pendingHires = new List<CharacterInfo>();
			if (this.map.CurrentLocation != null && this.map.CurrentLocation.Type.HasHireableCharacters)
			{
				availableHires = this.map.CurrentLocation.GetHireableCharacters().ToList<CharacterInfo>();
				Location currentLocation = this.map.CurrentLocation;
				pendingHires = ((currentLocation != null) ? currentLocation.HireManager.PendingHires : null);
			}
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(23);
				msg.WriteBoolean(createNotification);
				msg.WriteUInt16((ushort)availableHires.Count);
				foreach (CharacterInfo hire in availableHires)
				{
					hire.ServerWrite(msg);
					msg.WriteInt32(hire.Salary);
				}
				msg.WriteUInt16((ushort)pendingHires.Count);
				foreach (CharacterInfo pendingHire in pendingHires)
				{
					msg.WriteUInt16(pendingHire.ID);
					msg.WriteBoolean(pendingHire.BotStatus == BotStatus.PendingHireToReserveBench);
				}
				IEnumerable<CharacterInfo> crewManager = base.CrewManager.GetCharacterInfos(false);
				msg.WriteUInt16((ushort)crewManager.Count<CharacterInfo>());
				foreach (CharacterInfo info in crewManager)
				{
					info.ServerWrite(msg);
				}
				IEnumerable<CharacterInfo> reserveBench = base.CrewManager.GetReserveBenchInfos();
				msg.WriteUInt16((ushort)reserveBench.Count<CharacterInfo>());
				foreach (CharacterInfo info2 in reserveBench)
				{
					info2.ServerWrite(msg);
				}
				bool validRenaming = renamedCrewMember.Item1 > 0 && !string.IsNullOrEmpty(renamedCrewMember.Item2);
				msg.WriteBoolean(validRenaming);
				if (validRenaming)
				{
					msg.WriteUInt16(renamedCrewMember.Item1);
					msg.WriteString(renamedCrewMember.Item2);
				}
				msg.WriteBoolean(firedCharacter != null);
				if (firedCharacter != null)
				{
					msg.WriteUInt16(firedCharacter.ID);
				}
				GameMain.Server.ServerPeer.Send(msg, client.Connection, DeliveryMethod.Reliable, true);
			}
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x0003E868 File Offset: 0x0003CA68
		public override bool TryPurchase(Client client, int price)
		{
			if (client != null && !GameMain.Server.ConnectedClients.Contains(client))
			{
				return false;
			}
			if (price == 0)
			{
				return true;
			}
			Wallet wallet = this.GetWallet(client);
			if (!CampaignMode.AllowedToManageWallets(client))
			{
				return wallet.TryDeduct(price);
			}
			int balance = wallet.Balance;
			if (balance >= price)
			{
				return wallet.TryDeduct(price);
			}
			if (balance + this.Bank.Balance >= price)
			{
				int remainder = price - balance;
				if (balance > 0)
				{
					wallet.Deduct(balance);
				}
				this.Bank.Deduct(remainder);
				return true;
			}
			return false;
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x0003E8EC File Offset: 0x0003CAEC
		public override int GetBalance(Client client = null)
		{
			if (client == null)
			{
				return 0;
			}
			Wallet wallet = this.GetWallet(client);
			if (!CampaignMode.AllowedToManageWallets(client))
			{
				return wallet.Balance;
			}
			return wallet.Balance + this.Bank.Balance;
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x0003E928 File Offset: 0x0003CB28
		public override void Save(XElement element, bool isSavingOnLoading)
		{
			element.Add(new XAttribute("campaignid", this.CampaignID));
			XElement modeElement = new XElement("MultiPlayerCampaign", new object[]
			{
				new XAttribute("purchasedlostshuttles", this.PurchasedLostShuttlesInLatestSave),
				new XAttribute("purchasedhullrepairs", this.PurchasedHullRepairsInLatestSave),
				new XAttribute("purchaseditemrepairs", this.PurchasedItemRepairsInLatestSave),
				new XAttribute("cheatsenabled", this.CheatsEnabled)
			});
			DebugConsole.NewMessage(string.Concat(new string[]
			{
				"Saved PurchasedHullRepairs: ",
				this.PurchasedHullRepairs.ToString(),
				" (in last save ",
				this.PurchasedHullRepairsInLatestSave.ToString(),
				")"
			}), new Color?(Color.Magenta), false);
			modeElement.Add(this.Settings.Save());
			modeElement.Add(base.SaveStats());
			GameServer server = GameMain.Server;
			TraitorManager traitorManager = (server != null) ? server.TraitorManager : null;
			if (traitorManager != null)
			{
				modeElement.Add(traitorManager.Save());
			}
			modeElement.Add(this.Bank.Save());
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
			CampaignMetadata campaignMetadata = this.CampaignMetadata;
			if (campaignMetadata != null)
			{
				campaignMetadata.Save(modeElement);
			}
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
			if (this.petsElement != null)
			{
				modeElement.Add(this.petsElement);
			}
			XElement crewManagerElement = base.CrewManager.SaveMultiplayer(modeElement);
			if (base.ActiveOrdersElement != null)
			{
				crewManagerElement.Add(base.ActiveOrdersElement);
			}
			XElement savedExperiencePointsElement = new XElement("SavedExperiencePoints");
			foreach (MultiPlayerCampaign.SavedExperiencePoints savedExperiencePoint in this.savedExperiencePoints)
			{
				AccountId accountId;
				savedExperiencePointsElement.Add(new XElement("Point", new object[]
				{
					new XAttribute("accountid", savedExperiencePoint.AccountId.TryUnwrap(out accountId) ? accountId.StringRepresentation : ""),
					new XAttribute("address", savedExperiencePoint.Address.StringRepresentation),
					new XAttribute("points", savedExperiencePoint.ExperiencePoints)
				}));
			}
			element.Add(modeElement);
			string characterDataPath = isSavingOnLoading ? MultiPlayerCampaign.GetCharacterDataPathForLoading() : MultiPlayerCampaign.GetCharacterDataPathForSaving();
			XDocument characterDataDoc = new XDocument(new object[]
			{
				new XElement("CharacterData")
			});
			foreach (CharacterCampaignData cd in this.characterData)
			{
				characterDataDoc.Root.Add(cd.Save());
			}
			try
			{
				SaveUtil.DeleteIfExists(characterDataPath);
				characterDataDoc.SaveSafe(characterDataPath, SaveOptions.None, false, 0);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving multiplayer campaign characters to \"" + characterDataPath + "\" failed!", e, null, false, false);
			}
			this.lastSaveID += 1;
			DebugConsole.Log("Campaign saved, save ID " + this.lastSaveID.ToString());
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0003ED90 File Offset: 0x0003CF90
		public void SaveSingleCharacter(CharacterCampaignData newData, bool skipBackup = false)
		{
			string characterDataPath = MultiPlayerCampaign.GetCharacterDataPathForSaving();
			if (!File.Exists(characterDataPath))
			{
				DebugConsole.ThrowError("Failed to load the character data for the campaign. Could not find the file \"" + characterDataPath + "\".", null, null, false, false);
				return;
			}
			XDocument loadedCharacterData = XMLExtensions.TryLoadXml(characterDataPath);
			if (((loadedCharacterData != null) ? loadedCharacterData.Root : null) == null)
			{
				return;
			}
			XElement oldData = loadedCharacterData.Root.Elements().FirstOrDefault((XElement subElement) => new CharacterCampaignData(subElement).IsDuplicate(newData));
			if (oldData != null)
			{
				if (!skipBackup)
				{
					this.replacedCharacterDataBackup.Add(new CharacterCampaignData(oldData));
				}
				oldData.Remove();
			}
			loadedCharacterData.Root.Add(newData.Save());
			try
			{
				loadedCharacterData.SaveSafe(characterDataPath, SaveOptions.None, false, 0);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving multiplayer campaign characters to \"" + characterDataPath + "\" failed!", e, null, false, false);
			}
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0003EE74 File Offset: 0x0003D074
		public CharacterCampaignData RestoreSingleCharacterFromBackup(Client client)
		{
			CharacterCampaignData characterToRestore = this.replacedCharacterDataBackup.Find((CharacterCampaignData cd) => cd.MatchesClient(client));
			if (characterToRestore != null)
			{
				this.replacedCharacterDataBackup.Remove(characterToRestore);
				return characterToRestore;
			}
			return null;
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0003EEB9 File Offset: 0x0003D0B9
		public ushort GetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags flag)
		{
			if (!MultiPlayerCampaign.ValidateFlag(flag))
			{
				return 0;
			}
			return this.lastUpdateID[flag];
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0003EED1 File Offset: 0x0003D0D1
		public void SetLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags flag, ushort id)
		{
			if (!MultiPlayerCampaign.ValidateFlag(flag))
			{
				return;
			}
			this.lastUpdateID[flag] = id;
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x0003EEEC File Offset: 0x0003D0EC
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

		// Token: 0x0600067E RID: 1662 RVA: 0x0003EF38 File Offset: 0x0003D138
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

		// Token: 0x0600067F RID: 1663 RVA: 0x0003EFCC File Offset: 0x0003D1CC
		private static bool ValidateFlag(MultiPlayerCampaign.NetFlags flag)
		{
			return MathHelper.IsPowerOfTwo((int)flag);
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x0003EFD9 File Offset: 0x0003D1D9
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x0003F000 File Offset: 0x0003D200
		public ushort LastSaveID
		{
			get
			{
				if (GameMain.Server != null && this.lastSaveID < 1)
				{
					this.lastSaveID += 1;
				}
				return this.lastSaveID;
			}
			set
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Misc);
				this.lastSaveID = value;
			}
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x0003F010 File Offset: 0x0003D210
		// (set) Token: 0x06000683 RID: 1667 RVA: 0x0003F018 File Offset: 0x0003D218
		public byte CampaignID { get; set; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000684 RID: 1668 RVA: 0x0003F021 File Offset: 0x0003D221
		// (set) Token: 0x06000685 RID: 1669 RVA: 0x0003F029 File Offset: 0x0003D229
		public byte RoundID { get; set; }

		// Token: 0x06000686 RID: 1670 RVA: 0x0003F034 File Offset: 0x0003D234
		private MultiPlayerCampaign(CampaignSettings settings) : base(GameModePreset.MultiPlayerCampaign, settings)
		{
			MultiPlayerCampaign.currentCampaignID += 1;
			this.lastUpdateID = new Dictionary<MultiPlayerCampaign.NetFlags, ushort>();
			foreach (object obj in Enum.GetValues(typeof(MultiPlayerCampaign.NetFlags)))
			{
				MultiPlayerCampaign.NetFlags flag = (MultiPlayerCampaign.NetFlags)obj;
				this.lastUpdateID[flag] = 1;
			}
			this.CampaignID = MultiPlayerCampaign.currentCampaignID;
			this.UpgradeManager = new UpgradeManager(this);
			base.InitFactions();
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0003F12C File Offset: 0x0003D32C
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

		// Token: 0x06000688 RID: 1672 RVA: 0x0003F168 File Offset: 0x0003D368
		public static MultiPlayerCampaign LoadNew(XElement element)
		{
			MultiPlayerCampaign campaign = new MultiPlayerCampaign(CampaignSettings.Empty);
			campaign.Load(element);
			campaign.InitProjSpecific();
			campaign.IsFirstRound = false;
			return campaign;
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0003F198 File Offset: 0x0003D398
		private void InitProjSpecific()
		{
			Identifier eventId = "MultiPlayerCampaign".ToIdentifier();
			this.CargoManager.OnItemsInBuyCrateChanged.RegisterOverwriteExisting(eventId, delegate(CargoManager _)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.ItemsInBuyCrate);
			});
			this.CargoManager.OnPurchasedItemsChanged.RegisterOverwriteExisting(eventId, delegate(CargoManager _)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.PurchasedItems);
			});
			this.CargoManager.OnSoldItemsChanged.RegisterOverwriteExisting(eventId, delegate(CargoManager _)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.SoldItems);
			});
			this.UpgradeManager.OnUpgradesChanged.RegisterOverwriteExisting(eventId, delegate(UpgradeManager _)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.UpgradeManager);
			});
			Reputation.OnAnyReputationValueChanged.RegisterOverwriteExisting(eventId, delegate(Reputation _)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.Reputation);
			});
			base.Map.OnLocationSelected = delegate(Location loc, LocationConnection connection)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
			};
			base.Map.OnMissionsSelected = delegate(LocationConnection loc, IEnumerable<Mission> mission)
			{
				this.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
			};
			ushort num = this.LastSaveID;
			this.LastSaveID = num + 1;
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0003F27C File Offset: 0x0003D47C
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

		// Token: 0x0600068B RID: 1675 RVA: 0x0003F311 File Offset: 0x0003D511
		public static string GetCharacterDataPathForLoading()
		{
			return MultiPlayerCampaign.GetCharacterDataSavePath(GameMain.GameSession.DataPath.LoadPath);
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x0003F327 File Offset: 0x0003D527
		public static string GetCharacterDataPathForSaving()
		{
			return MultiPlayerCampaign.GetCharacterDataSavePath(GameMain.GameSession.DataPath.SavePath);
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0003F340 File Offset: 0x0003D540
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
								if (!(a == "traitormanager"))
								{
									if (!(a == "savedexperiencepoints"))
									{
										continue;
									}
									foreach (XElement savedExp in subElement.Elements())
									{
										this.savedExperiencePoints.Add(new MultiPlayerCampaign.SavedExperiencePoints(savedExp));
									}
									continue;
								}
								else
								{
									GameServer server = GameMain.Server;
									if (server == null)
									{
										continue;
									}
									TraitorManager traitorManager = server.TraitorManager;
									if (traitorManager == null)
									{
										continue;
									}
									traitorManager.Load(subElement);
									continue;
								}
							}
						}
						else
						{
							Dictionary<Faction, float> prevReputations = base.Factions.ToDictionary((Faction k) => k, (Faction v) => v.Reputation.Value);
							this.CampaignMetadata.Load(subElement);
							using (IEnumerator<Faction> enumerator3 = base.Factions.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									Faction faction = enumerator3.Current;
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
				}
			}
			if (this.UpgradeManager == null)
			{
				this.UpgradeManager = new UpgradeManager(this);
			}
			this.characterData.Clear();
			string characterDataPath = MultiPlayerCampaign.GetCharacterDataPathForLoading();
			if (!File.Exists(characterDataPath))
			{
				DebugConsole.ThrowError("Failed to load the character data for the campaign. Could not find the file \"" + characterDataPath + "\".", null, null, false, false);
				return;
			}
			XDocument characterDataDoc = XMLExtensions.TryLoadXml(characterDataPath);
			if (((characterDataDoc != null) ? characterDataDoc.Root : null) == null)
			{
				return;
			}
			foreach (XElement subElement2 in characterDataDoc.Root.Elements())
			{
				this.characterData.Add(new CharacterCampaignData(subElement2));
			}
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0003F6D0 File Offset: 0x0003D8D0
		public static List<SubmarineInfo> GetCampaignSubs()
		{
			IEnumerable<SubmarineInfo> availableSubs = SubmarineInfo.SavedSubmarines;
			List<SubmarineInfo> campaignSubs = (from s in availableSubs
			where s.IsCampaignCompatible && MultiPlayerCampaign.<GetCampaignSubs>g__isSubmarineVisible|98_0(s)
			select s).ToList<SubmarineInfo>();
			if (!campaignSubs.Any<SubmarineInfo>())
			{
				campaignSubs.AddRange(availableSubs.Where(new Func<SubmarineInfo, bool>(MultiPlayerCampaign.<GetCampaignSubs>g__isSubmarineVisible|98_0)));
			}
			if (!campaignSubs.Any<SubmarineInfo>())
			{
				campaignSubs.Add(GameMain.NetLobbyScreen.SelectedSub);
			}
			return campaignSubs;
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0003F748 File Offset: 0x0003D948
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

		// Token: 0x06000690 RID: 1680 RVA: 0x0003F824 File Offset: 0x0003DA24
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
					if (!CampaignMode.AllowImmediateItemDelivery(sender))
					{
						deliverImmediately = false;
					}
					int quantity = msg.ReadRangedInteger(0, 100);
					items[storeId].Add(new PurchasedItem(itemId, quantity, sender)
					{
						DeliverImmediately = deliverImmediately
					});
				}
			}
			return items;
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x0003F8BC File Offset: 0x0003DABC
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

		// Token: 0x06000692 RID: 1682 RVA: 0x0003F9B4 File Offset: 0x0003DBB4
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

		// Token: 0x06000693 RID: 1683 RVA: 0x0003FA58 File Offset: 0x0003DC58
		[CompilerGenerated]
		private void <ServerReadMoney>g__TransferMoney|60_0(Wallet from, ref MultiPlayerCampaign.<>c__DisplayClass60_0 A_2)
		{
			if (!from.TryDeduct(A_2.transfer.Amount))
			{
				return;
			}
			ushort id;
			if (!A_2.transfer.Receiver.TryUnwrap(out id))
			{
				this.Bank.Give(A_2.transfer.Amount);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 4);
				defaultInterpolatedStringHandler.AppendFormatted(A_2.sender.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" transferred ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(A_2.transfer.Amount);
				defaultInterpolatedStringHandler.AppendLiteral(" mk to ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Bank.GetOwnerLogName());
				defaultInterpolatedStringHandler.AppendLiteral(" from ");
				defaultInterpolatedStringHandler.AppendFormatted(from.GetOwnerLogName());
				defaultInterpolatedStringHandler.AppendLiteral(".");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Money);
				return;
			}
			Wallet wallet = MultiPlayerCampaign.<ServerReadMoney>g__GetWalletByID|60_1(id);
			if (wallet is InvalidWallet)
			{
				return;
			}
			wallet.Give(A_2.transfer.Amount);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 4);
			defaultInterpolatedStringHandler2.AppendFormatted(A_2.sender.Name);
			defaultInterpolatedStringHandler2.AppendLiteral(" transferred ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(A_2.transfer.Amount);
			defaultInterpolatedStringHandler2.AppendLiteral(" mk to ");
			defaultInterpolatedStringHandler2.AppendFormatted(wallet.GetOwnerLogName());
			defaultInterpolatedStringHandler2.AppendLiteral(" from ");
			defaultInterpolatedStringHandler2.AppendFormatted(from.GetOwnerLogName());
			defaultInterpolatedStringHandler2.AppendLiteral(".");
			GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.Money);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x0003FBD8 File Offset: 0x0003DDD8
		[CompilerGenerated]
		internal static Wallet <ServerReadMoney>g__GetWalletByID|60_1(ushort id)
		{
			Character targetCharacter = Character.CharacterList.FirstOrDefault((Character c) => c.ID == id);
			if (targetCharacter != null)
			{
				return targetCharacter.Wallet;
			}
			return Wallet.Invalid;
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0003FC64 File Offset: 0x0003DE64
		[CompilerGenerated]
		internal static bool <GetCampaignSubs>g__isSubmarineVisible|98_0(SubmarineInfo s)
		{
			return !GameMain.NetworkMember.ServerSettings.HiddenSubs.Any((string h) => s.Name.Equals(h, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x0400031E RID: 798
		private readonly List<CharacterCampaignData> characterData = new List<CharacterCampaignData>();

		// Token: 0x0400031F RID: 799
		private readonly Dictionary<ushort, Wallet> walletsToCheck = new Dictionary<ushort, Wallet>();

		// Token: 0x04000320 RID: 800
		private readonly HashSet<NetWalletTransaction> transactions = new HashSet<NetWalletTransaction>();

		// Token: 0x04000321 RID: 801
		private const float clientCheckInterval = 10f;

		// Token: 0x04000322 RID: 802
		private float clientCheckTimer = 10f;

		// Token: 0x04000323 RID: 803
		public List<CharacterCampaignData> replacedCharacterDataBackup = new List<CharacterCampaignData>();

		// Token: 0x04000324 RID: 804
		private bool forceMapUI;

		// Token: 0x04000326 RID: 806
		private readonly List<MultiPlayerCampaign.SavedExperiencePoints> savedExperiencePoints = new List<MultiPlayerCampaign.SavedExperiencePoints>();

		// Token: 0x04000327 RID: 807
		private bool purchasedHullRepairs;

		// Token: 0x04000328 RID: 808
		private bool purchasedLostShuttles;

		// Token: 0x04000329 RID: 809
		private bool purchasedItemRepairs;

		// Token: 0x0400032A RID: 810
		private readonly List<CharacterCampaignData> discardedCharacters = new List<CharacterCampaignData>();

		// Token: 0x0400032B RID: 811
		private readonly Dictionary<MultiPlayerCampaign.NetFlags, ushort> lastUpdateID;

		// Token: 0x0400032C RID: 812
		private ushort lastSaveID;

		// Token: 0x0400032D RID: 813
		private static byte currentCampaignID;

		// Token: 0x02000647 RID: 1607
		private class SavedExperiencePoints
		{
			// Token: 0x06004DE9 RID: 19945 RVA: 0x001E071C File Offset: 0x001DE91C
			public SavedExperiencePoints(Client client)
			{
				this.AccountId = client.AccountId;
				this.Address = client.Connection.Endpoint.Address;
				Character character = client.Character;
				int? num;
				if (character == null)
				{
					num = null;
				}
				else
				{
					CharacterInfo info = character.Info;
					num = ((info != null) ? new int?(info.ExperiencePoints) : null);
				}
				int? num2 = num;
				this.ExperiencePoints = num2.GetValueOrDefault();
			}

			// Token: 0x06004DEA RID: 19946 RVA: 0x001E0794 File Offset: 0x001DE994
			public SavedExperiencePoints(XElement element)
			{
				this.AccountId = Barotrauma.Networking.AccountId.Parse(element.GetAttributeString("accountid", null) ?? element.GetAttributeString("steamid", ""));
				this.Address = Address.Parse(element.GetAttributeString("address", null) ?? element.GetAttributeString("endpoint", "")).Fallback(new UnknownAddress());
				this.ExperiencePoints = element.GetAttributeInt("points", 0);
			}

			// Token: 0x04002912 RID: 10514
			public readonly Option<AccountId> AccountId;

			// Token: 0x04002913 RID: 10515
			public readonly Address Address;

			// Token: 0x04002914 RID: 10516
			public readonly int ExperiencePoints;
		}

		// Token: 0x02000648 RID: 1608
		[Flags]
		public enum NetFlags : ushort
		{
			// Token: 0x04002916 RID: 10518
			Misc = 1,
			// Token: 0x04002917 RID: 10519
			MapAndMissions = 2,
			// Token: 0x04002918 RID: 10520
			UpgradeManager = 4,
			// Token: 0x04002919 RID: 10521
			SubList = 8,
			// Token: 0x0400291A RID: 10522
			ItemsInBuyCrate = 16,
			// Token: 0x0400291B RID: 10523
			ItemsInSellFromSubCrate = 32,
			// Token: 0x0400291C RID: 10524
			PurchasedItems = 128,
			// Token: 0x0400291D RID: 10525
			SoldItems = 256,
			// Token: 0x0400291E RID: 10526
			Reputation = 512,
			// Token: 0x0400291F RID: 10527
			CharacterInfo = 2048
		}
	}
}
