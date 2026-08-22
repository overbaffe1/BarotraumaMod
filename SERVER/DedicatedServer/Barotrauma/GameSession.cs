using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.PerkBehaviors;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001EA RID: 490
	[NullableContext(1)]
	[Nullable(0)]
	internal class GameSession
	{
		// Token: 0x17000A15 RID: 2581
		// (get) Token: 0x06002317 RID: 8983 RVA: 0x000EA035 File Offset: 0x000E8235
		// (set) Token: 0x06002318 RID: 8984 RVA: 0x000EA03D File Offset: 0x000E823D
		public Version LastSaveVersion { get; set; } = GameMain.Version;

		// Token: 0x17000A16 RID: 2582
		// (get) Token: 0x06002319 RID: 8985 RVA: 0x000EA046 File Offset: 0x000E8246
		// (set) Token: 0x0600231A RID: 8986 RVA: 0x000EA04E File Offset: 0x000E824E
		public float RoundDuration { get; private set; }

		// Token: 0x17000A17 RID: 2583
		// (get) Token: 0x0600231B RID: 8987 RVA: 0x000EA057 File Offset: 0x000E8257
		public IEnumerable<Mission> Missions
		{
			get
			{
				return this.missions;
			}
		}

		// Token: 0x17000A18 RID: 2584
		// (get) Token: 0x0600231C RID: 8988 RVA: 0x000EA05F File Offset: 0x000E825F
		public IEnumerable<Character> Casualties
		{
			get
			{
				return this.casualties;
			}
		}

		// Token: 0x0600231D RID: 8989 RVA: 0x000EA067 File Offset: 0x000E8267
		public void IncrementPermadeath([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId)
		{
			this.permadeathsPerAccount[accountId] = this.permadeathsPerAccount.GetValueOrDefault(accountId, 0) + 1;
		}

		// Token: 0x0600231E RID: 8990 RVA: 0x000EA084 File Offset: 0x000E8284
		public int PermadeathCountForAccount([Nullable(new byte[]
		{
			0,
			1
		})] Option<AccountId> accountId)
		{
			return this.permadeathsPerAccount.GetValueOrDefault(accountId, 0);
		}

		// Token: 0x17000A19 RID: 2585
		// (get) Token: 0x0600231F RID: 8991 RVA: 0x000EA093 File Offset: 0x000E8293
		// (set) Token: 0x06002320 RID: 8992 RVA: 0x000EA09B File Offset: 0x000E829B
		public bool IsRunning { get; private set; }

		// Token: 0x17000A1A RID: 2586
		// (get) Token: 0x06002321 RID: 8993 RVA: 0x000EA0A4 File Offset: 0x000E82A4
		// (set) Token: 0x06002322 RID: 8994 RVA: 0x000EA0AC File Offset: 0x000E82AC
		public bool RoundEnding { get; private set; }

		// Token: 0x17000A1B RID: 2587
		// (get) Token: 0x06002323 RID: 8995 RVA: 0x000EA0B5 File Offset: 0x000E82B5
		// (set) Token: 0x06002324 RID: 8996 RVA: 0x000EA0BD File Offset: 0x000E82BD
		[Nullable(2)]
		public Level Level { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000A1C RID: 2588
		// (get) Token: 0x06002325 RID: 8997 RVA: 0x000EA0C6 File Offset: 0x000E82C6
		// (set) Token: 0x06002326 RID: 8998 RVA: 0x000EA0CE File Offset: 0x000E82CE
		[Nullable(2)]
		public LevelData LevelData { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000A1D RID: 2589
		// (get) Token: 0x06002327 RID: 8999 RVA: 0x000EA0D7 File Offset: 0x000E82D7
		// (set) Token: 0x06002328 RID: 9000 RVA: 0x000EA0DF File Offset: 0x000E82DF
		public bool MirrorLevel { get; private set; }

		// Token: 0x17000A1E RID: 2590
		// (get) Token: 0x06002329 RID: 9001 RVA: 0x000EA0E8 File Offset: 0x000E82E8
		[Nullable(2)]
		public Map Map
		{
			[NullableContext(2)]
			get
			{
				CampaignMode campaignMode = this.GameMode as CampaignMode;
				if (campaignMode == null)
				{
					return null;
				}
				return campaignMode.Map;
			}
		}

		// Token: 0x17000A1F RID: 2591
		// (get) Token: 0x0600232A RID: 9002 RVA: 0x000EA100 File Offset: 0x000E8300
		[Nullable(2)]
		public CampaignMode Campaign
		{
			[NullableContext(2)]
			get
			{
				return this.GameMode as CampaignMode;
			}
		}

		// Token: 0x17000A20 RID: 2592
		// (get) Token: 0x0600232B RID: 9003 RVA: 0x000EA110 File Offset: 0x000E8310
		public Location StartLocation
		{
			get
			{
				if (this.Map != null)
				{
					return this.Map.CurrentLocation;
				}
				if (this.dummyLocations == null)
				{
					this.dummyLocations = ((this.LevelData == null) ? GameSession.CreateDummyLocations(string.Empty, null) : GameSession.CreateDummyLocations(this.LevelData, null));
				}
				if (this.dummyLocations == null)
				{
					throw new NullReferenceException("dummyLocations is null somehow!");
				}
				return this.dummyLocations[0];
			}
		}

		// Token: 0x17000A21 RID: 2593
		// (get) Token: 0x0600232C RID: 9004 RVA: 0x000EA17C File Offset: 0x000E837C
		public Location EndLocation
		{
			get
			{
				if (this.Map != null)
				{
					return this.Map.SelectedLocation;
				}
				if (this.dummyLocations == null)
				{
					this.dummyLocations = ((this.LevelData == null) ? GameSession.CreateDummyLocations(string.Empty, null) : GameSession.CreateDummyLocations(this.LevelData, null));
				}
				if (this.dummyLocations == null)
				{
					throw new NullReferenceException("dummyLocations is null somehow!");
				}
				return this.dummyLocations[1];
			}
		}

		// Token: 0x17000A22 RID: 2594
		// (get) Token: 0x0600232D RID: 9005 RVA: 0x000EA1E7 File Offset: 0x000E83E7
		// (set) Token: 0x0600232E RID: 9006 RVA: 0x000EA1EF File Offset: 0x000E83EF
		public SubmarineInfo SubmarineInfo { get; set; }

		// Token: 0x17000A23 RID: 2595
		// (get) Token: 0x0600232F RID: 9007 RVA: 0x000EA1F8 File Offset: 0x000E83F8
		// (set) Token: 0x06002330 RID: 9008 RVA: 0x000EA200 File Offset: 0x000E8400
		public SubmarineInfo EnemySubmarineInfo { get; set; }

		// Token: 0x17000A24 RID: 2596
		// (get) Token: 0x06002331 RID: 9009 RVA: 0x000EA209 File Offset: 0x000E8409
		// (set) Token: 0x06002332 RID: 9010 RVA: 0x000EA211 File Offset: 0x000E8411
		[Nullable(2)]
		public Submarine Submarine { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000A25 RID: 2597
		// (get) Token: 0x06002333 RID: 9011 RVA: 0x000EA21A File Offset: 0x000E841A
		[Nullable(new byte[]
		{
			1,
			0
		})]
		public IEnumerable<ValueTuple<CharacterTeamType, Identifier>> UnlockedRecipes
		{
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			get
			{
				return this.unlockedRecipes;
			}
		}

		// Token: 0x17000A26 RID: 2598
		// (get) Token: 0x06002334 RID: 9012 RVA: 0x000EA222 File Offset: 0x000E8422
		// (set) Token: 0x06002335 RID: 9013 RVA: 0x000EA22A File Offset: 0x000E842A
		public CampaignDataPath DataPath { get; set; }

		// Token: 0x17000A27 RID: 2599
		// (get) Token: 0x06002336 RID: 9014 RVA: 0x000EA233 File Offset: 0x000E8433
		public bool TraitorsEnabled
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				return ((networkMember != null) ? networkMember.ServerSettings : null) != null && GameMain.NetworkMember.ServerSettings.TraitorProbability > 0f;
			}
		}

		// Token: 0x06002337 RID: 9015 RVA: 0x000EA260 File Offset: 0x000E8460
		private GameSession(SubmarineInfo submarineInfo)
		{
			this.SubmarineInfo = submarineInfo;
			this.EnemySubmarineInfo = this.SubmarineInfo;
			GameMain.GameSession = this;
			this.EventManager = new EventManager();
		}

		// Token: 0x06002338 RID: 9016 RVA: 0x000EA2D9 File Offset: 0x000E84D9
		private GameSession(SubmarineInfo submarineInfo, SubmarineInfo enemySubmarineInfo) : this(submarineInfo)
		{
			this.EnemySubmarineInfo = enemySubmarineInfo;
		}

		// Token: 0x06002339 RID: 9017 RVA: 0x000EA2EC File Offset: 0x000E84EC
		public GameSession(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			0,
			1
		})] Option<SubmarineInfo> enemySub, CampaignDataPath dataPath, GameModePreset gameModePreset, CampaignSettings settings, [Nullable(2)] string seed = null, [Nullable(2)] IEnumerable<Identifier> missionTypes = null) : this(submarineInfo)
		{
			this.DataPath = dataPath;
			this.CrewManager = new CrewManager(gameModePreset.IsSinglePlayer);
			this.GameMode = this.InstantiateGameMode(gameModePreset, seed, submarineInfo, settings, null, missionTypes);
			SubmarineInfo enemySubmarine;
			this.EnemySubmarineInfo = (enemySub.TryUnwrap(out enemySubmarine) ? enemySubmarine : submarineInfo);
			this.InitOwnedSubs(submarineInfo, null);
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x000EA34C File Offset: 0x000E854C
		public GameSession(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			0,
			1
		})] Option<SubmarineInfo> enemySub, GameModePreset gameModePreset, [Nullable(2)] string seed = null, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<MissionPrefab> missionPrefabs = null) : this(submarineInfo)
		{
			this.CrewManager = new CrewManager(gameModePreset.IsSinglePlayer);
			this.GameMode = this.InstantiateGameMode(gameModePreset, seed, submarineInfo, CampaignSettings.Empty, missionPrefabs, null);
			SubmarineInfo enemySubmarine;
			this.EnemySubmarineInfo = (enemySub.TryUnwrap(out enemySubmarine) ? enemySubmarine : submarineInfo);
			this.InitOwnedSubs(submarineInfo, null);
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x000EA3A8 File Offset: 0x000E85A8
		public GameSession(SubmarineInfo submarineInfo, List<SubmarineInfo> ownedSubmarines, XDocument doc, CampaignDataPath campaignData) : this(submarineInfo)
		{
			this.DataPath = campaignData;
			GameMain.GameSession = this;
			XElement root = doc.Root;
			if (root == null)
			{
				throw new NullReferenceException("Game session XML element is invalid: document is null.");
			}
			XElement rootElement = root;
			this.LastSaveVersion = doc.Root.GetAttributeVersion("version", GameMain.Version);
			foreach (XElement subElement in rootElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "gamemode" || a == "singleplayercampaign")
				{
					throw new Exception("The server cannot load a single player campaign.");
				}
				if (!(a == "multiplayercampaign"))
				{
					if (a == "permadeaths")
					{
						this.permadeathsPerAccount = new Dictionary<Option<AccountId>, int>();
						foreach (XElement accountElement in subElement.Elements("account"))
						{
							XAttribute accountIdAttr = accountElement.Attribute("id");
							if (accountIdAttr != null)
							{
								XAttribute permadeathCountAttr = accountElement.Attribute("permadeathcount");
								if (permadeathCountAttr != null)
								{
									try
									{
										this.permadeathsPerAccount[AccountId.Parse(accountIdAttr.Value)] = int.Parse(permadeathCountAttr.Value);
									}
									catch (Exception e)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 3);
										defaultInterpolatedStringHandler.AppendLiteral("Exception while trying to load permadeath counts!\n");
										defaultInterpolatedStringHandler.AppendFormatted<Exception>(e);
										defaultInterpolatedStringHandler.AppendLiteral("\n id: ");
										defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(accountIdAttr);
										defaultInterpolatedStringHandler.AppendLiteral("\n permadeathcount: ");
										defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(permadeathCountAttr);
										DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
									}
								}
							}
						}
					}
				}
				else
				{
					this.CrewManager = new CrewManager(false);
					MultiPlayerCampaign mpCampaign = MultiPlayerCampaign.LoadNew(subElement);
					this.GameMode = mpCampaign;
					if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
					{
						mpCampaign.LoadNewLevel();
						this.InitOwnedSubs(submarineInfo, ownedSubmarines);
						SaveUtil.SaveGame(campaignData, true);
					}
				}
			}
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x000EA618 File Offset: 0x000E8818
		private void InitOwnedSubs(SubmarineInfo submarineInfo, [Nullable(new byte[]
		{
			2,
			1
		})] List<SubmarineInfo> ownedSubmarines = null)
		{
			this.OwnedSubmarines = (ownedSubmarines ?? new List<SubmarineInfo>());
			if (submarineInfo != null && !this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == submarineInfo.Name))
			{
				this.OwnedSubmarines.Add(submarineInfo);
			}
		}

		// Token: 0x0600233D RID: 9021 RVA: 0x000EA674 File Offset: 0x000E8874
		private GameMode InstantiateGameMode(GameModePreset gameModePreset, [Nullable(2)] string seed, SubmarineInfo selectedSub, CampaignSettings settings, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<MissionPrefab> missionPrefabs = null, [Nullable(2)] IEnumerable<Identifier> missionTypes = null)
		{
			if (gameModePreset.GameModeType == typeof(CoOpMode))
			{
				if (missionPrefabs == null)
				{
					return new CoOpMode(gameModePreset, missionTypes, seed ?? ToolBox.RandomSeed(8));
				}
				return new CoOpMode(gameModePreset, missionPrefabs);
			}
			else if (gameModePreset.GameModeType == typeof(PvPMode))
			{
				if (missionPrefabs == null)
				{
					return new PvPMode(gameModePreset, missionTypes, seed ?? ToolBox.RandomSeed(8));
				}
				return new PvPMode(gameModePreset, missionPrefabs);
			}
			else
			{
				if (gameModePreset.GameModeType == typeof(MultiPlayerCampaign))
				{
					MultiPlayerCampaign campaign = MultiPlayerCampaign.StartNew(seed ?? ToolBox.RandomSeed(8), settings);
					if (selectedSub != null)
					{
						campaign.Bank.Deduct(selectedSub.Price);
						campaign.Bank.Balance = Math.Max(campaign.Bank.Balance, 0);
						GameServer server = GameMain.Server;
						float? num;
						if (server == null)
						{
							num = null;
						}
						else
						{
							ServerSettings serverSettings = server.ServerSettings;
							num = ((serverSettings != null) ? new float?(serverSettings.NewCampaignDefaultSalary) : null);
						}
						float? num2 = num;
						if (num2 != null)
						{
							float salary = num2.GetValueOrDefault();
							campaign.Bank.SetRewardDistribution((int)Math.Round((double)salary, 0));
						}
					}
					return campaign;
				}
				if (gameModePreset.GameModeType == typeof(GameMode))
				{
					return new GameMode(gameModePreset);
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a game mode of the type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(gameModePreset.GameModeType);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0600233E RID: 9022 RVA: 0x000EA804 File Offset: 0x000E8A04
		public static Location[] CreateDummyLocations(LevelData levelData, [Nullable(2)] LocationType forceLocationType = null)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
			OutpostGenerationParams forceParams = (levelData != null) ? levelData.ForceOutpostGenerationParams : null;
			if (forceLocationType == null && forceParams != null && forceParams.AllowedLocationTypes.Any<Identifier>() && !forceParams.AllowedLocationTypes.Contains("Any".ToIdentifier()))
			{
				forceLocationType = (from lt in LocationType.Prefabs
				where forceParams.AllowedLocationTypes.Contains(lt.Identifier)
				select lt).GetRandom(rand);
			}
			Location[] dummyLocations = GameSession.CreateDummyLocations(rand, forceLocationType);
			List<Faction> factions = new List<Faction>();
			foreach (FactionPrefab factionPrefab in FactionPrefab.Prefabs)
			{
				factions.Add(new Faction(new CampaignMetadata(), factionPrefab));
			}
			foreach (Location location in dummyLocations)
			{
				if (location.Type.HasOutpost)
				{
					location.Faction = CampaignMode.GetRandomFaction(factions, rand, false, true);
					location.SecondaryFaction = CampaignMode.GetRandomFaction(factions, rand, true, true);
				}
			}
			return dummyLocations;
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x000EA93C File Offset: 0x000E8B3C
		public static Location[] CreateDummyLocations(string seed, [Nullable(2)] LocationType forceLocationType = null)
		{
			return GameSession.CreateDummyLocations(new MTRandom(ToolBox.StringToInt(seed)), forceLocationType);
		}

		// Token: 0x06002340 RID: 9024 RVA: 0x000EA950 File Offset: 0x000E8B50
		private static Location[] CreateDummyLocations(Random rand, [Nullable(2)] LocationType forceLocationType = null)
		{
			Location[] dummyLocations = new Location[2];
			for (int i = 0; i < 2; i++)
			{
				dummyLocations[i] = Location.CreateRandom(new Vector2((float)rand.NextDouble() * 10000f, (float)rand.NextDouble() * 10000f), null, null, rand, true, forceLocationType, null);
			}
			return dummyLocations;
		}

		// Token: 0x06002341 RID: 9025 RVA: 0x000EA9AE File Offset: 0x000E8BAE
		[NullableContext(2)]
		public static bool ShouldApplyDisembarkPoints(GameModePreset preset)
		{
			return preset == null || preset == GameModePreset.Sandbox || preset == GameModePreset.Mission || preset == GameModePreset.PvP;
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x000EA9CF File Offset: 0x000E8BCF
		public void LoadPreviousSave()
		{
			AchievementManager.OnRoundEnded(this, true);
			Submarine.Unload();
			SaveUtil.LoadGame(this.DataPath);
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x000EA9E8 File Offset: 0x000E8BE8
		public void SwitchSubmarine(SubmarineInfo newSubmarine, bool transferItems, [Nullable(2)] Client client = null)
		{
			if (!this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == newSubmarine.Name))
			{
				this.OwnedSubmarines.Add(newSubmarine);
			}
			else
			{
				for (int i = 0; i < this.OwnedSubmarines.Count; i++)
				{
					if (this.OwnedSubmarines[i].Name == newSubmarine.Name)
					{
						newSubmarine = this.OwnedSubmarines[i];
						break;
					}
				}
			}
			this.Campaign.PendingSubmarineSwitch = newSubmarine;
			this.Campaign.TransferItemsOnSubSwitch = transferItems;
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x000EAA98 File Offset: 0x000E8C98
		public bool TryPurchaseSubmarine(SubmarineInfo newSubmarine, [Nullable(2)] Client client = null)
		{
			if (this.Campaign == null)
			{
				return false;
			}
			int price = newSubmarine.GetPrice(null, null);
			if (GameMain.NetworkMember != null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null || !networkMember.IsServer)
				{
					goto IL_4E;
				}
			}
			if (!this.Campaign.TryPurchase(client, price))
			{
				return false;
			}
			IL_4E:
			if (!this.OwnedSubmarines.Any((SubmarineInfo s) => s.Name == newSubmarine.Name))
			{
				GameAnalyticsManager.AddMoneySpentEvent(price, GameAnalyticsManager.MoneySink.SubmarinePurchase, newSubmarine.Name);
				this.OwnedSubmarines.Add(newSubmarine);
				MultiPlayerCampaign multiPlayerCampaign = this.Campaign as MultiPlayerCampaign;
				if (multiPlayerCampaign != null)
				{
					multiPlayerCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.SubList);
				}
			}
			return true;
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x000EAB48 File Offset: 0x000E8D48
		public bool IsSubmarineOwned(SubmarineInfo query)
		{
			Submarine mainSub = Submarine.MainSub;
			return ((mainSub != null) ? mainSub.Info.Name : null) == query.Name || (this.OwnedSubmarines != null && this.OwnedSubmarines.Any((SubmarineInfo os) => os.Name == query.Name));
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x000EABB0 File Offset: 0x000E8DB0
		public bool IsCurrentLocationRadiated()
		{
			Map map = this.Map;
			if (((map != null) ? map.CurrentLocation : null) == null || this.Campaign == null)
			{
				return false;
			}
			bool isRadiated = this.Map.CurrentLocation.IsRadiated();
			Level loaded = Level.Loaded;
			Location endLocation = (loaded != null) ? loaded.EndLocation : null;
			if (endLocation != null)
			{
				isRadiated |= endLocation.IsRadiated();
			}
			return isRadiated;
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x000EAC0C File Offset: 0x000E8E0C
		public void StartRound(string levelSeed, float? difficulty = null, [Nullable(2)] LevelGenerationParams levelGenerationParams = null, Identifier forceBiome = default(Identifier))
		{
			if (this.GameMode == null)
			{
				return;
			}
			LevelData randomLevel = null;
			bool pvpOnly = this.GameMode is PvPMode;
			foreach (Mission mission in this.Missions.Union(this.GameMode.Missions))
			{
				MissionPrefab missionPrefab = mission.Prefab;
				if (missionPrefab != null && missionPrefab.AllowedLocationTypes.Any<Identifier>() && !missionPrefab.AllowedConnectionTypes.Any<ValueTuple<Identifier, Identifier>>())
				{
					Random rand = new MTRandom(ToolBox.StringToInt(levelSeed));
					LocationType locationType = (from lt in LocationType.Prefabs
					orderby lt.UintIdentifier
					where missionPrefab.AllowedLocationTypes.Any((Identifier m) => m == lt.Identifier)
					select lt).GetRandom(rand);
					this.dummyLocations = GameSession.CreateDummyLocations(levelSeed, locationType);
					if (!GameSession.<StartRound>g__tryCreateFaction|93_5(mission.Prefab.RequiredLocationFaction, this.dummyLocations, delegate(Location loc, Faction fac)
					{
						loc.Faction = fac;
					}))
					{
						GameSession.<StartRound>g__tryCreateFaction|93_5(locationType.Faction, this.dummyLocations, delegate(Location loc, Faction fac)
						{
							loc.Faction = fac;
						});
						GameSession.<StartRound>g__tryCreateFaction|93_5(locationType.SecondaryFaction, this.dummyLocations, delegate(Location loc, Faction fac)
						{
							loc.SecondaryFaction = fac;
						});
					}
					randomLevel = LevelData.CreateRandom(levelSeed, difficulty, levelGenerationParams, forceBiome, true, pvpOnly);
					break;
				}
			}
			if (randomLevel == null)
			{
				randomLevel = LevelData.CreateRandom(levelSeed, difficulty, levelGenerationParams, forceBiome, false, pvpOnly);
			}
			this.StartRound(randomLevel, false, null, null);
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x000EAE00 File Offset: 0x000E9000
		[NullableContext(2)]
		private bool TryGenerateStationAroundModule(SubmarineInfo moduleInfo, out Submarine outpostSub)
		{
			outpostSub = null;
			if (moduleInfo == null)
			{
				return false;
			}
			IEnumerable<OutpostGenerationParams> allSuitableOutpostParams = from outpostParam in OutpostGenerationParams.OutpostParams
			where base.<TryGenerateStationAroundModule>g__IsOutpostParamsSuitable|2(outpostParam)
			select outpostParam;
			OutpostGenerationParams suitableOutpostParams = (from p in allSuitableOutpostParams
			where p.AllowedLocationTypes.Any<Identifier>()
			select p).GetRandomUnsynced<OutpostGenerationParams>() ?? allSuitableOutpostParams.GetRandomUnsynced<OutpostGenerationParams>();
			if (suitableOutpostParams == null)
			{
				DebugConsole.AddWarning("No suitable generation parameters found for ForceOutpostModule, skipping outpost generation!", null);
				return false;
			}
			LocationType suitableLocationType = (from locationType in LocationType.Prefabs
			where suitableOutpostParams.AllowedLocationTypes.Contains(locationType.Identifier)
			select locationType).GetRandomUnsynced<LocationType>();
			if (suitableLocationType == null)
			{
				DebugConsole.AddWarning("No suitable location type found for ForceOutpostModule, skipping outpost generation!", null);
				return false;
			}
			OutpostGenerationParams.ModuleCount requiredFactionModuleCount = suitableOutpostParams.ModuleCounts.FirstOrDefault((OutpostGenerationParams.ModuleCount mc) => !mc.RequiredFaction.IsEmpty && moduleInfo.OutpostModuleInfo.ModuleFlags.Contains(mc.Identifier));
			Identifier requiredFactionId = (requiredFactionModuleCount != null) ? requiredFactionModuleCount.RequiredFaction : Identifier.Empty;
			if (requiredFactionId.IsEmpty)
			{
				outpostSub = OutpostGenerator.Generate(suitableOutpostParams, suitableLocationType, false, false);
				return outpostSub != null;
			}
			Location[] dummyLocations = GameSession.CreateDummyLocations("1337", suitableLocationType);
			Location dummyLocation = dummyLocations[0];
			FactionPrefab factionPrefab;
			if (FactionPrefab.Prefabs.TryGet(requiredFactionId, out factionPrefab))
			{
				if (factionPrefab.ControlledOutpostPercentage > factionPrefab.SecondaryControlledOutpostPercentage)
				{
					dummyLocation.Faction = new Faction(null, factionPrefab);
				}
				else
				{
					dummyLocation.SecondaryFaction = new Faction(null, factionPrefab);
				}
				outpostSub = OutpostGenerator.Generate(suitableOutpostParams, dummyLocation, false, false);
				return outpostSub != null;
			}
			return false;
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x000EAF74 File Offset: 0x000E9174
		[NullableContext(2)]
		public void StartRound(LevelData levelData, bool mirrorLevel = false, SubmarineInfo startOutpost = null, SubmarineInfo endOutpost = null)
		{
			this.RoundDuration = 0f;
			AfflictionPrefab.LoadAllEffectsAndTreatmentSuitabilities();
			this.MirrorLevel = mirrorLevel;
			if (this.SubmarineInfo == null)
			{
				DebugConsole.ThrowError("Couldn't start game session, submarine not selected.", null, null, false, false);
				return;
			}
			if (this.SubmarineInfo.IsFileCorrupted)
			{
				DebugConsole.ThrowError("Couldn't start game session, submarine file corrupted.", null, null, false, false);
				return;
			}
			if (this.SubmarineInfo.SubmarineElement.Elements().Count<XElement>() == 0)
			{
				DebugConsole.ThrowError("Couldn't start game session, saved submarine is empty. The submarine file may be corrupted.", null, null, false, false);
				return;
			}
			Submarine.LockX = (Submarine.LockY = false);
			this.LevelData = levelData;
			Submarine.Unload();
			bool loadSubmarine = this.GameMode.Missions.None((Mission m) => !m.Prefab.LoadSubmarines);
			if (loadSubmarine)
			{
				Submarine outpostSub;
				if (this.TryGenerateStationAroundModule(this.ForceOutpostModule, out outpostSub))
				{
					this.Submarine = (Submarine.MainSub = (outpostSub ?? new Submarine(this.SubmarineInfo, true, null, null)));
				}
				else
				{
					this.Submarine = (Submarine.MainSub = new Submarine(this.SubmarineInfo, true, null, null));
				}
				using (IEnumerator<Submarine> enumerator = this.Submarine.GetConnectedSubs().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Submarine sub = enumerator.Current;
						sub.TeamID = CharacterTeamType.Team1;
						foreach (Item item in Item.ItemList)
						{
							if (item.Submarine == sub)
							{
								foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
								{
									wifiComponent.TeamID = sub.TeamID;
								}
							}
						}
					}
					goto IL_1D1;
				}
			}
			this.Submarine = (Submarine.MainSub = null);
			IL_1D1:
			this.GameMode.AddExtraMissions(this.LevelData);
			foreach (Mission mission in this.GameMode.Missions)
			{
				mission.SetLevel(levelData);
			}
			if (Submarine.MainSubs[1] == null && loadSubmarine)
			{
				SubmarineInfo submarineInfo;
				if (!(this.GameMode is PvPMode))
				{
					Mission mission2 = this.GameMode.Missions.FirstOrDefault((Mission m) => m.EnemySubmarineInfo != null);
					submarineInfo = ((mission2 != null) ? mission2.EnemySubmarineInfo : null);
				}
				else
				{
					submarineInfo = this.EnemySubmarineInfo;
				}
				SubmarineInfo enemySubmarineInfo = submarineInfo;
				if (enemySubmarineInfo != null)
				{
					Submarine.MainSubs[1] = new Submarine(enemySubmarineInfo, true, null, null);
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (serverSettings != null && serverSettings.LockAllDefaultWires && Submarine.MainSubs[0] != null)
			{
				List<Item> items = new List<Item>();
				items.AddRange(Submarine.MainSubs[0].GetItems(true));
				if (Submarine.MainSubs[1] != null)
				{
					items.AddRange(Submarine.MainSubs[1].GetItems(true));
				}
				foreach (Item item2 in items)
				{
					CircuitBox cb = item2.GetComponent<CircuitBox>();
					if (cb != null)
					{
						cb.TemporarilyLocked = true;
					}
					Wire wire = item2.GetComponent<Wire>();
					if (wire != null && !wire.NoAutoLock)
					{
						if (wire.Connections.Any((Connection c) => c != null))
						{
							wire.Locked = true;
						}
					}
				}
			}
			Level level = null;
			if (levelData != null)
			{
				level = Level.Generate(levelData, mirrorLevel, this.StartLocation, this.EndLocation, startOutpost, endOutpost);
			}
			this.InitializeLevel(level);
			Powered.Grids.Clear();
			this.casualties.Clear();
			CampaignMode campaignMode = this.GameMode as CampaignMode;
			if (campaignMode != null && campaignMode.ItemsRelocatedToMainSub)
			{
				GameMain.Server.SendChatMessage(TextManager.Get("itemrelocated").Value, new ChatMessageType?(ChatMessageType.ServerMessageBoxInGame), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				campaignMode.ItemsRelocatedToMainSub = false;
			}
			EventManager eventManager = this.EventManager;
			if (eventManager != null)
			{
				EventLog eventLog = eventManager.EventLog;
				if (eventLog != null)
				{
					eventLog.Clear();
				}
			}
			if (campaignMode != null && !campaignMode.DivingSuitWarningShown && Level.Loaded != null && Level.Loaded.GetRealWorldDepth(0f) > 4000f)
			{
				campaignMode.DivingSuitWarningShown = true;
			}
		}

		// Token: 0x0600234A RID: 9034 RVA: 0x000EB410 File Offset: 0x000E9610
		[NullableContext(2)]
		private void InitializeLevel(Level level)
		{
			StatusEffect.StopAll();
			bool forceDocking = false;
			this.LevelData = ((level != null) ? level.LevelData : null);
			this.Level = level;
			GameSession.PlaceSubAtInitialPosition(this.Submarine, this.Level, true, forceDocking);
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.Info.IsOutpost || sub.Info.IsBeacon || sub.Info.IsWreck)
				{
					sub.DisableObstructedWayPoints();
				}
			}
			Entity.Spawner = new EntitySpawner();
			if (this.GameMode != null)
			{
				this.missions.Clear();
				this.missions.AddRange(this.GameMode.Missions);
				this.GameMode.Start();
				foreach (Mission mission in this.missions)
				{
					int prevEntityCount = Entity.GetEntities().Count;
					mission.Start(Level.Loaded);
					if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Entity.GetEntities().Count != prevEntityCount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Entity count has changed after starting a mission (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(mission.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(") as a client. ");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + "The clients should not instantiate entities themselves when starting the mission, but instead the server should inform the client of the spawned entities using Mission.ServerWriteInitial.", null, null, false, false);
					}
				}
				EventManager eventManager = this.EventManager;
				if (eventManager != null)
				{
					eventManager.StartRound(Level.Loaded);
				}
				Level level2 = this.Level;
				AchievementManager.OnStartRound((level2 != null) ? level2.LevelData.Biome : null);
				this.GameMode.ShowStartMessage();
				if (GameMain.NetworkMember == null)
				{
					if (this.Level != null)
					{
						if (GameMain.GameSession.Missions.None((Mission m) => !m.Prefab.AllowOutpostNPCs))
						{
							this.Level.SpawnNPCs();
						}
						this.Level.SpawnCorpses();
						this.Level.PrepareBeaconStation();
					}
					else
					{
						foreach (Submarine sub2 in Submarine.Loaded)
						{
							bool flag;
							if (sub2 == null)
							{
								flag = (null != null);
							}
							else
							{
								SubmarineInfo info = sub2.Info;
								flag = (((info != null) ? info.OutpostGenerationParams : null) != null);
							}
							if (flag)
							{
								OutpostGenerator.SpawnNPCs(this.StartLocation, sub2);
							}
						}
					}
					CampaignMode campaign = this.Campaign;
					AutoItemPlacer.SpawnItems((campaign != null) ? new Identifier?(campaign.Settings.StartItemSet) : null);
				}
				MultiPlayerCampaign mpCampaign = this.GameMode as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					mpCampaign.UpgradeManager.ApplyUpgrades();
					mpCampaign.UpgradeManager.SanityCheckUpgrades();
				}
			}
			CreatureMetrics.RecentlyEncountered.Clear();
			Camera cam = GameMain.GameScreen.Cam;
			Character controlled = Character.Controlled;
			Vector2 position;
			if (controlled == null)
			{
				Submarine mainSub = Submarine.MainSub;
				position = ((mainSub != null) ? mainSub.WorldPosition : Submarine.Loaded.First<Submarine>().WorldPosition);
			}
			else
			{
				position = controlled.WorldPosition;
			}
			cam.Position = position;
			this.RoundDuration = 0f;
			GameMain.ResetFrameTime();
			this.IsRunning = true;
		}

		// Token: 0x0600234B RID: 9035 RVA: 0x000EB77C File Offset: 0x000E997C
		[NullableContext(2)]
		public static void PlaceSubAtInitialPosition(Submarine sub, Level level, bool placeAtStart = true, bool forceDocking = false)
		{
			if (level == null || sub == null)
			{
				if (sub != null)
				{
					sub.SetPosition(Vector2.Zero, null, true);
				}
				return;
			}
			Submarine outpost = placeAtStart ? level.StartOutpost : level.EndOutpost;
			Vector2 originalSubPos = sub.WorldPosition;
			WayPoint spawnPoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.SpawnType.HasFlag(SpawnType.Submarine) && wp.Submarine == outpost);
			if (spawnPoint != null)
			{
				sub.SetPosition(spawnPoint.WorldPosition, null, true);
				sub.NeutralizeBallast();
				sub.EnableMaintainPosition();
			}
			else if (outpost != null)
			{
				Rectangle outpostBorders = outpost.GetDockedBorders(true);
				Rectangle subBorders = sub.GetDockedBorders(true);
				sub.SetPosition(outpost.WorldPosition - new Vector2(0f, (float)(outpostBorders.Height / 2 + subBorders.Height / 2)), null, true);
				float closestDistance = 0f;
				DockingPort myPort = null;
				DockingPort outPostPort = null;
				foreach (DockingPort port in DockingPort.List)
				{
					if (!port.IsHorizontal && !port.Docked)
					{
						if (port.Item.Submarine == outpost)
						{
							if (port.DockingTarget == null || (outPostPort != null && !outPostPort.MainDockingPort && port.MainDockingPort))
							{
								outPostPort = port;
							}
						}
						else if (port.Item.Submarine == sub && port.Item.WorldPosition.Y >= sub.WorldPosition.Y)
						{
							float dist = Vector2.DistanceSquared(port.Item.WorldPosition, outpost.WorldPosition);
							if ((myPort == null || dist < closestDistance || port.MainDockingPort) && (myPort == null || !myPort.MainDockingPort))
							{
								myPort = port;
								closestDistance = dist;
							}
						}
					}
				}
				if (myPort != null && outPostPort != null)
				{
					Vector2 portDiff = myPort.Item.WorldPosition - sub.WorldPosition;
					Vector2 spawnPos = outPostPort.Item.WorldPosition - portDiff - Vector2.UnitY * outPostPort.DockedDistance;
					bool startDocked = level.Type == LevelData.LevelType.Outpost || forceDocking;
					if (startDocked)
					{
						sub.SetPosition(spawnPos, null, true);
						myPort.Dock(outPostPort);
						myPort.Lock(true, false, true);
					}
					else
					{
						sub.SetPosition(spawnPos - Vector2.UnitY * 100f, null, true);
						sub.NeutralizeBallast();
						sub.EnableMaintainPosition();
					}
				}
				else
				{
					sub.NeutralizeBallast();
					sub.EnableMaintainPosition();
				}
			}
			else
			{
				sub.SetPosition(sub.FindSpawnPos(placeAtStart ? level.StartPosition : level.EndPosition, null, 0f, 0), null, true);
				sub.NeutralizeBallast();
				sub.EnableMaintainPosition();
			}
			foreach (Item item in sub.GetItems(true))
			{
				Steering steering = item.GetComponent<Steering>();
				if (steering != null && steering.MaintainPos)
				{
					steering.RefreshPosToMaintain();
				}
			}
			List<MapEntity> linkedSubs = MapEntity.MapEntityList.FindAll((MapEntity me) => me is LinkedSubmarine);
			foreach (MapEntity mapEntity in linkedSubs)
			{
				LinkedSubmarine ls = (LinkedSubmarine)mapEntity;
				if (ls.Sub != null && ls.Submarine == sub && ls.LoadSub && !ls.Sub.DockedTo.Contains(sub) && !sub.Info.LeftBehindDockingPortIDs.Contains(ls.OriginalLinkedToID) && ls.Sub.Info.SubmarineElement.Attribute("location") == null)
				{
					ls.SetPositionRelativeToMainSub();
				}
			}
		}

		// Token: 0x0600234C RID: 9036 RVA: 0x000EBBA4 File Offset: 0x000E9DA4
		public void Update(float deltaTime)
		{
			this.RoundDuration += deltaTime;
			EventManager eventManager = this.EventManager;
			if (eventManager != null)
			{
				eventManager.Update(deltaTime);
			}
			GameMode gameMode = this.GameMode;
			if (gameMode != null)
			{
				gameMode.Update(deltaTime);
			}
			for (int i = this.missions.Count - 1; i >= 0; i--)
			{
				this.missions[i].Update(deltaTime);
			}
		}

		// Token: 0x0600234D RID: 9037 RVA: 0x000EBC0D File Offset: 0x000E9E0D
		[NullableContext(2)]
		public Mission GetMission(int index)
		{
			if (index < 0 || index >= this.missions.Count)
			{
				return null;
			}
			return this.missions[index];
		}

		// Token: 0x0600234E RID: 9038 RVA: 0x000EBC2F File Offset: 0x000E9E2F
		public int GetMissionIndex(Mission mission)
		{
			return this.missions.IndexOf(mission);
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x000EBC40 File Offset: 0x000E9E40
		public void EnforceMissionOrder(List<Identifier> missionIdentifiers)
		{
			List<Mission> sortedMissions = new List<Mission>();
			using (List<Identifier>.Enumerator enumerator = missionIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier missionId = enumerator.Current;
					Mission matchingMission = this.missions.Find((Mission m) => m.Prefab.Identifier == missionId);
					if (matchingMission != null)
					{
						sortedMissions.Add(matchingMission);
						this.missions.Remove(matchingMission);
					}
				}
			}
			this.missions.AddRange(sortedMissions);
		}

		// Token: 0x06002350 RID: 9040 RVA: 0x000EBCD4 File Offset: 0x000E9ED4
		public static ImmutableHashSet<Character> GetSessionCrewCharacters(CharacterType type)
		{
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			if (crewManager == null)
			{
				return ImmutableHashSet<Character>.Empty;
			}
			HashSet<Character> characters = new HashSet<Character>();
			IEnumerable<Character> players = from c in GameMain.Server.ConnectedClients
			select c.Character into c
			where ((c != null) ? c.Info : null) != null && !c.IsDead
			select c;
			IEnumerable<Character> bots = (from characterInfo in crewManager.GetCharacterInfos(false)
			where GameMain.Server.ConnectedClients.None((Client c) => c.CharacterInfo == characterInfo)
			select characterInfo.Character).NotNull<Character>();
			if (type.HasFlag(CharacterType.Bot))
			{
				foreach (Character bot in bots)
				{
					characters.Add(bot);
				}
			}
			if (type.HasFlag(CharacterType.Player))
			{
				foreach (Character player in players)
				{
					characters.Add(player);
				}
			}
			return characters.ToImmutableHashSet<Character>();
		}

		// Token: 0x06002351 RID: 9041 RVA: 0x000EBE58 File Offset: 0x000EA058
		public void EndRound(string endMessage, CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None, TraitorManager.TraitorResults? traitorResults = null, bool createRoundSummary = true)
		{
			this.RoundEnding = true;
			Powered.Grids.Clear();
			Powered.ChangedConnections.Clear();
			try
			{
				EventManager eventManager = this.EventManager;
				if (eventManager != null)
				{
					eventManager.TriggerOnEndRoundActions();
				}
				ImmutableHashSet<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
				int prevMoney = this.<EndRound>g__GetAmountOfMoney|105_0(crewCharacters);
				this.EndMissions(transitionType);
				foreach (Character character in crewCharacters)
				{
					character.CheckTalents(AbilityEffectType.OnRoundEnd);
				}
				AchievementManager.OnRoundEnded(this, false);
				GameServer server = GameMain.Server;
				if (server != null)
				{
					TraitorManager traitorManager = server.TraitorManager;
					if (traitorManager != null)
					{
						traitorManager.EndRound();
					}
				}
				GameMode gameMode = this.GameMode;
				if (gameMode != null)
				{
					gameMode.End(transitionType);
				}
				EventManager eventManager2 = this.EventManager;
				if (eventManager2 != null)
				{
					eventManager2.EndRound();
				}
				StatusEffect.StopAll();
				AfflictionPrefab.ClearAllEffects();
				this.IsRunning = false;
				bool flag;
				if (GameMain.Server != null)
				{
					flag = GameMain.Server.ConnectedClients.Any((Client c) => c.InGame && c.Character != null && !c.Character.IsDead);
				}
				else
				{
					flag = false;
				}
				GameAnalyticsManager.ProgressionStatus progressionStatus = flag ? GameAnalyticsManager.ProgressionStatus.Complete : GameAnalyticsManager.ProgressionStatus.Fail;
				GameMode gameMode2 = this.GameMode;
				GameAnalyticsManager.AddProgressionEvent(progressionStatus, ((gameMode2 != null) ? gameMode2.Preset.Identifier.Value : null) ?? "none", (double)this.RoundDuration);
				string str = "EndRound:";
				GameMode gameMode3 = this.GameMode;
				string text;
				if (gameMode3 == null)
				{
					text = null;
				}
				else
				{
					GameModePreset preset = gameMode3.Preset;
					text = ((preset != null) ? preset.Identifier.Value : null);
				}
				string eventId = str + (text ?? "none") + ":";
				this.LogEndRoundStats(eventId, traitorResults);
				CampaignMode campaignMode = this.GameMode as CampaignMode;
				if (campaignMode != null)
				{
					GameAnalyticsManager.AddDesignEvent(eventId + "MoneyEarned", (double)(this.<EndRound>g__GetAmountOfMoney|105_0(crewCharacters) - prevMoney));
					campaignMode.TotalPlayTime += (double)this.RoundDuration;
				}
				this.missions.Clear();
			}
			catch (Exception e)
			{
				string errorMsg = "Unknown error while ending the round.";
				DebugConsole.ThrowError(errorMsg, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameSession.EndRound:UnknownError", GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + e.StackTrace);
				if (Timing.TotalTime > this.LastEndRoundErrorMessageTime + 1.0)
				{
					GameServer server2 = GameMain.Server;
					if (server2 != null)
					{
						server2.SendChatMessage(errorMsg + "\n" + e.StackTrace, new ChatMessageType?(ChatMessageType.Error), null, null, PlayerConnectionChangeType.None, ChatMode.None);
					}
					this.LastEndRoundErrorMessageTime = Timing.TotalTime;
				}
			}
			finally
			{
				this.RoundEnding = false;
			}
		}

		// Token: 0x06002352 RID: 9042 RVA: 0x000EC110 File Offset: 0x000EA310
		public void EndMissions(CampaignMode.TransitionType transitionType)
		{
			ImmutableHashSet<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			foreach (Mission mission in this.missions)
			{
				mission.End(transitionType);
			}
			if (this.missions.Any<Mission>())
			{
				if (this.missions.Any((Mission m) => m.Completed))
				{
					foreach (Character character in crewCharacters)
					{
						character.CheckTalents(AbilityEffectType.OnAnyMissionCompleted);
					}
				}
				if (this.missions.All((Mission m) => m.Completed))
				{
					foreach (Character character2 in crewCharacters)
					{
						character2.CheckTalents(AbilityEffectType.OnAllMissionsCompleted);
					}
				}
			}
		}

		// Token: 0x06002353 RID: 9043 RVA: 0x000EC254 File Offset: 0x000EA454
		public static PerkCollection GetPerks()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (serverSettings == null)
			{
				return PerkCollection.Empty;
			}
			ImmutableArray<DisembarkPerkPrefab>.Builder team1Builder = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			ImmutableArray<DisembarkPerkPrefab>.Builder team2Builder = ImmutableArray.CreateBuilder<DisembarkPerkPrefab>();
			foreach (Identifier coalitionPerk in serverSettings.SelectedCoalitionPerks)
			{
				DisembarkPerkPrefab disembarkPerk;
				if (DisembarkPerkPrefab.Prefabs.TryGet(coalitionPerk, out disembarkPerk))
				{
					team1Builder.Add(disembarkPerk);
				}
			}
			foreach (Identifier separatistsPerk in serverSettings.SelectedSeparatistsPerks)
			{
				DisembarkPerkPrefab disembarkPerk2;
				if (DisembarkPerkPrefab.Prefabs.TryGet(separatistsPerk, out disembarkPerk2))
				{
					team2Builder.Add(disembarkPerk2);
				}
			}
			return new PerkCollection(team1Builder.ToImmutable(), team2Builder.ToImmutable());
		}

		// Token: 0x06002354 RID: 9044 RVA: 0x000EC314 File Offset: 0x000EA514
		public static bool ValidatedDisembarkPoints(GameModePreset preset, IEnumerable<Identifier> missionTypes)
		{
			GameSession.<>c__DisplayClass108_0 CS$<>8__locals1;
			CS$<>8__locals1.preset = preset;
			CS$<>8__locals1.missionTypes = missionTypes;
			NetworkMember networkMember = GameMain.NetworkMember;
			ServerSettings settings = (networkMember != null) ? networkMember.ServerSettings : null;
			if (settings == null)
			{
				return false;
			}
			bool checkBothTeams = CS$<>8__locals1.preset == GameModePreset.PvP;
			PerkCollection perks = GameSession.GetPerks();
			int team1TotalCost = GameSession.<ValidatedDisembarkPoints>g__GetTotalCost|108_0(perks.Team1Perks, ref CS$<>8__locals1);
			if (team1TotalCost > settings.DisembarkPointAllowance)
			{
				return false;
			}
			if (checkBothTeams)
			{
				int team2TotalCost = GameSession.<ValidatedDisembarkPoints>g__GetTotalCost|108_0(perks.Team2Perks, ref CS$<>8__locals1);
				if (team2TotalCost > settings.DisembarkPointAllowance)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002355 RID: 9045 RVA: 0x000EC398 File Offset: 0x000EA598
		public static bool ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(GameModePreset preset, IEnumerable<Identifier> missionTypes)
		{
			if (preset == GameModePreset.Mission || preset == GameModePreset.PvP)
			{
				IEnumerable<Identifier> missionTypesToCheck = MissionMode.ValidateMissionTypes(missionTypes, (preset == GameModePreset.PvP) ? MissionPrefab.PvPMissionClasses : MissionPrefab.CoOpMissionClasses);
				using (IEnumerator<Identifier> enumerator = missionTypesToCheck.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Identifier missionType = enumerator.Current;
						IEnumerable<MissionPrefab> prefabs = MissionPrefab.Prefabs;
						Func<MissionPrefab, bool> predicate;
						Func<MissionPrefab, bool> <>9__0;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = delegate(MissionPrefab mp)
							{
								Identifier type = mp.Type;
								return type == missionType;
							});
						}
						foreach (MissionPrefab missionPrefab in prefabs.Where(predicate))
						{
							if (missionPrefab.LoadSubmarines)
							{
								return false;
							}
						}
					}
				}
				return true;
			}
			return true;
		}

		// Token: 0x06002356 RID: 9046 RVA: 0x000EC480 File Offset: 0x000EA680
		private void LogStartRoundStats()
		{
			if (!GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
			{
				return;
			}
			GameAnalyticsManager.ProgressionStatus progressionStatus = GameAnalyticsManager.ProgressionStatus.Start;
			GameMode gameMode = this.GameMode;
			string text;
			if (gameMode == null)
			{
				text = null;
			}
			else
			{
				GameModePreset preset = gameMode.Preset;
				text = ((preset != null) ? preset.Identifier.Value : null);
			}
			GameAnalyticsManager.AddProgressionEvent(progressionStatus, text ?? "none");
			string str = "StartRound:";
			GameMode gameMode2 = this.GameMode;
			string text2;
			if (gameMode2 == null)
			{
				text2 = null;
			}
			else
			{
				GameModePreset preset2 = gameMode2.Preset;
				text2 = ((preset2 != null) ? preset2.Identifier.Value : null);
			}
			string eventId = str + (text2 ?? "none") + ":";
			string str2 = eventId;
			string str3 = "Submarine:";
			Submarine mainSub = Submarine.MainSub;
			string text3;
			if (mainSub == null)
			{
				text3 = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				text3 = ((info != null) ? info.Name : null);
			}
			GameAnalyticsManager.AddDesignEvent(str2 + str3 + (text3 ?? "none"));
			string str4 = eventId;
			string str5 = "GameMode:";
			GameMode gameMode3 = this.GameMode;
			string text4;
			if (gameMode3 == null)
			{
				text4 = null;
			}
			else
			{
				GameModePreset preset3 = gameMode3.Preset;
				text4 = ((preset3 != null) ? preset3.Identifier.Value : null);
			}
			GameAnalyticsManager.AddDesignEvent(str4 + str5 + (text4 ?? "none"));
			string str6 = eventId;
			string str7 = "CrewSize:";
			CrewManager crewManager = this.CrewManager;
			int? num;
			if (crewManager == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
				num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
			}
			int? num2 = num;
			GameAnalyticsManager.AddDesignEvent(str6 + str7 + num2.GetValueOrDefault().ToString());
			foreach (Mission mission in this.missions)
			{
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"MissionType:",
					mission.Prefab.Type.ToString() ?? "none",
					":",
					mission.Prefab.Identifier.ToString()
				}));
			}
			if (Level.Loaded != null)
			{
				Identifier? identifier;
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					LevelGenerationParams generationParams = Level.Loaded.GenerationParams;
					identifier = ((generationParams != null) ? new Identifier?(generationParams.Identifier) : null);
				}
				else
				{
					Submarine startOutpost = Level.Loaded.StartOutpost;
					if (startOutpost == null)
					{
						identifier = null;
					}
					else
					{
						SubmarineInfo info2 = startOutpost.Info;
						if (info2 == null)
						{
							identifier = null;
						}
						else
						{
							OutpostGenerationParams outpostGenerationParams = info2.OutpostGenerationParams;
							identifier = ((outpostGenerationParams != null) ? new Identifier?(outpostGenerationParams.Identifier) : null);
						}
					}
				}
				Identifier levelId = identifier ?? "null".ToIdentifier();
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"LevelType:",
					Level.Loaded.Type.ToString(),
					":",
					levelId.ToString()
				}));
			}
			string str8 = eventId;
			string str9 = "Biome:";
			Level loaded = Level.Loaded;
			string text5;
			if (loaded == null)
			{
				text5 = null;
			}
			else
			{
				LevelData levelData = loaded.LevelData;
				if (levelData == null)
				{
					text5 = null;
				}
				else
				{
					Biome biome = levelData.Biome;
					text5 = ((biome != null) ? biome.Identifier.Value : null);
				}
			}
			GameAnalyticsManager.AddDesignEvent(str8 + str9 + (text5 ?? "none"));
			CampaignMode campaignMode = this.GameMode as CampaignMode;
			if (campaignMode != null)
			{
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RadiationEnabled:" + campaignMode.Settings.RadiationEnabled.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:WorldHostility:" + campaignMode.Settings.WorldHostility.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShowHuskWarning:" + campaignMode.Settings.ShowHuskWarning.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:StartItemSet:" + campaignMode.Settings.StartItemSet.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:MaxMissionCount:" + campaignMode.Settings.MaxMissionCount.ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RepairFailMultiplier:" + ((int)(campaignMode.Settings.RepairFailMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:FuelMultiplier:" + ((int)(campaignMode.Settings.FuelMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:MissionRewardMultiplier:" + ((int)(campaignMode.Settings.MissionRewardMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:CrewVitalityMultiplier:" + ((int)(campaignMode.Settings.CrewVitalityMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:NonCrewVitalityMultiplier:" + ((int)(campaignMode.Settings.NonCrewVitalityMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:OxygenMultiplier:" + ((int)(campaignMode.Settings.OxygenMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:RepairFailMultiplier:" + ((int)(campaignMode.Settings.RepairFailMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShipyardPriceMultiplier:" + ((int)(campaignMode.Settings.ShipyardPriceMultiplier * 100f)).ToString());
				GameAnalyticsManager.AddDesignEvent("CampaignSettings:ShopPriceMultiplier:" + ((int)(campaignMode.Settings.ShopPriceMultiplier * 100f)).ToString());
				bool firstTimeInBiome = this.Map != null && !this.Map.Connections.Any((LocationConnection c) => c.Passed && c.Biome == this.LevelData.Biome);
				if (firstTimeInBiome)
				{
					string str10 = eventId;
					Level loaded2 = Level.Loaded;
					string text6;
					if (loaded2 == null)
					{
						text6 = null;
					}
					else
					{
						LevelData levelData2 = loaded2.LevelData;
						if (levelData2 == null)
						{
							text6 = null;
						}
						else
						{
							Biome biome2 = levelData2.Biome;
							text6 = ((biome2 != null) ? biome2.Identifier.Value : null);
						}
					}
					GameAnalyticsManager.AddDesignEvent(str10 + (text6 ?? "none") + "Discovered:Playtime", campaignMode.TotalPlayTime);
					string str11 = eventId;
					Level loaded3 = Level.Loaded;
					string text7;
					if (loaded3 == null)
					{
						text7 = null;
					}
					else
					{
						LevelData levelData3 = loaded3.LevelData;
						if (levelData3 == null)
						{
							text7 = null;
						}
						else
						{
							Biome biome3 = levelData3.Biome;
							text7 = ((biome3 != null) ? biome3.Identifier.Value : null);
						}
					}
					GameAnalyticsManager.AddDesignEvent(str11 + (text7 ?? "none") + "Discovered:PassedLevels", (double)campaignMode.TotalPassedLevels);
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				ServerSettings serverSettings = (networkMember != null) ? networkMember.ServerSettings : null;
				if (serverSettings != null)
				{
					GameAnalyticsManager.AddDesignEvent("ServerSettings:RespawnMode:" + serverSettings.RespawnMode.ToString());
					GameAnalyticsManager.AddDesignEvent("ServerSettings:IronmanMode:" + serverSettings.IronmanModeActive.ToString());
					GameAnalyticsManager.AddDesignEvent("ServerSettings:AllowBotTakeoverOnPermadeath:" + serverSettings.AllowBotTakeoverOnPermadeath.ToString());
				}
			}
		}

		// Token: 0x06002357 RID: 9047 RVA: 0x000ECB38 File Offset: 0x000EAD38
		public void LogEndRoundStats(string eventId, TraitorManager.TraitorResults? traitorResults = null)
		{
			if (!GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
			{
				return;
			}
			Submarine mainSub = Submarine.MainSub;
			bool? flag;
			if (mainSub == null)
			{
				flag = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				flag = ((info != null) ? new bool?(info.IsVanillaSubmarine()) : null);
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				string str = "Submarine:";
				Submarine mainSub2 = Submarine.MainSub;
				string text;
				if (mainSub2 == null)
				{
					text = null;
				}
				else
				{
					SubmarineInfo info2 = mainSub2.Info;
					text = ((info2 != null) ? info2.Name : null);
				}
				GameAnalyticsManager.AddDesignEvent(eventId + str + (text ?? "none"), (double)this.RoundDuration);
			}
			string str2 = "GameMode:";
			GameMode gameMode = this.GameMode;
			GameAnalyticsManager.AddDesignEvent(eventId + str2 + (((gameMode != null) ? gameMode.Name.Value : null) ?? "none"), (double)this.RoundDuration);
			string str3 = "CrewSize:";
			CrewManager crewManager = this.CrewManager;
			int? num;
			if (crewManager == null)
			{
				num = null;
			}
			else
			{
				IEnumerable<CharacterInfo> characterInfos = crewManager.GetCharacterInfos(false);
				num = ((characterInfos != null) ? new int?(characterInfos.Count<CharacterInfo>()) : null);
			}
			int? num2 = num;
			GameAnalyticsManager.AddDesignEvent(eventId + str3 + num2.GetValueOrDefault().ToString(), (double)this.RoundDuration);
			foreach (Mission mission in this.missions)
			{
				GameAnalyticsManager.AddDesignEvent(string.Concat(new string[]
				{
					eventId,
					"MissionType:",
					mission.Prefab.Type.ToString() ?? "none",
					":",
					mission.Prefab.Identifier.ToString(),
					":",
					mission.Completed ? "Completed" : "Failed"
				}), (double)this.RoundDuration);
			}
			if (!ContentPackageManager.ModsEnabled && Level.Loaded != null)
			{
				Identifier? identifier;
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					LevelGenerationParams generationParams = Level.Loaded.GenerationParams;
					identifier = ((generationParams != null) ? new Identifier?(generationParams.Identifier) : null);
				}
				else
				{
					Submarine startOutpost = Level.Loaded.StartOutpost;
					if (startOutpost == null)
					{
						identifier = null;
					}
					else
					{
						SubmarineInfo info3 = startOutpost.Info;
						if (info3 == null)
						{
							identifier = null;
						}
						else
						{
							OutpostGenerationParams outpostGenerationParams = info3.OutpostGenerationParams;
							identifier = ((outpostGenerationParams != null) ? new Identifier?(outpostGenerationParams.Identifier) : null);
						}
					}
				}
				Identifier levelId = identifier ?? "null".ToIdentifier();
				string str4 = "LevelType:";
				Level loaded = Level.Loaded;
				GameAnalyticsManager.AddDesignEvent(eventId + str4 + (((loaded != null) ? loaded.Type.ToString() : null) ?? ("none:" + levelId.ToString())), (double)this.RoundDuration);
				string str5 = "Biome:";
				Level loaded2 = Level.Loaded;
				string text2;
				if (loaded2 == null)
				{
					text2 = null;
				}
				else
				{
					LevelData levelData = loaded2.LevelData;
					if (levelData == null)
					{
						text2 = null;
					}
					else
					{
						Biome biome = levelData.Biome;
						text2 = ((biome != null) ? biome.Identifier.Value : null);
					}
				}
				GameAnalyticsManager.AddDesignEvent(eventId + str5 + (text2 ?? "none"), (double)this.RoundDuration);
			}
			if (traitorResults != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler.AppendLiteral("TraitorEvent:");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(traitorResults.Value.TraitorEventIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(":");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(traitorResults.Value.ObjectiveSuccessful);
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("TraitorEvent:");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(traitorResults.Value.TraitorEventIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(":");
				defaultInterpolatedStringHandler2.AppendFormatted(traitorResults.Value.VotedCorrectTraitor ? "TraitorIdentifier" : "TraitorUnidentified");
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
		}

		// Token: 0x06002358 RID: 9048 RVA: 0x000ECF44 File Offset: 0x000EB144
		public void KillCharacter(Character character)
		{
			if (this.CrewManager != null && this.CrewManager.GetCharacterInfos(false).Contains(character.Info))
			{
				this.casualties.Add(character);
			}
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x000ECF74 File Offset: 0x000EB174
		public void ReviveCharacter(Character character)
		{
			this.casualties.Remove(character);
		}

		// Token: 0x0600235A RID: 9050 RVA: 0x000ECF83 File Offset: 0x000EB183
		public void UnlockRecipe(CharacterTeamType team, Identifier identifier, bool showNotifications)
		{
			if (this.unlockedRecipes.Add(new ValueTuple<CharacterTeamType, Identifier>(team, identifier)))
			{
				GameMain.Server.UnlockRecipe(team, identifier);
			}
		}

		// Token: 0x0600235B RID: 9051 RVA: 0x000ECFA5 File Offset: 0x000EB1A5
		public bool HasUnlockedRecipe(Character character, Identifier itemIdentifier)
		{
			return character != null && this.unlockedRecipes.Contains(new ValueTuple<CharacterTeamType, Identifier>(character.TeamID, itemIdentifier));
		}

		// Token: 0x0600235C RID: 9052 RVA: 0x000ECFC4 File Offset: 0x000EB1C4
		public static bool IsCompatibleWithEnabledContentPackages(IList<string> contentPackageNames, out LocalizedString errorMsg)
		{
			errorMsg = "";
			if (!contentPackageNames.Any<string>())
			{
				return true;
			}
			List<string> missingPackages = new List<string>();
			using (IEnumerator<string> enumerator = contentPackageNames.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string packageName = enumerator.Current;
					if (!ContentPackageManager.EnabledPackages.All.Any((ContentPackage cp) => cp.NameMatches(packageName)))
					{
						missingPackages.Add(packageName);
					}
				}
			}
			List<string> excessPackages = new List<string>();
			using (IEnumerator<ContentPackage> enumerator2 = ContentPackageManager.EnabledPackages.All.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ContentPackage cp = enumerator2.Current;
					if (cp.HasMultiplayerSyncedContent && !contentPackageNames.Any((string p) => cp.NameMatches(p)))
					{
						excessPackages.Add(cp.Name);
					}
				}
			}
			bool orderMismatch = false;
			if (missingPackages.Count == 0 && missingPackages.Count == 0)
			{
				ImmutableArray<ContentPackage> enabledPackages = (from cp in ContentPackageManager.EnabledPackages.All
				where cp.HasMultiplayerSyncedContent
				select cp).ToImmutableArray<ContentPackage>();
				int i = 0;
				while (i < contentPackageNames.Count && i < enabledPackages.Length)
				{
					if (!enabledPackages[i].NameMatches(contentPackageNames[i]))
					{
						orderMismatch = true;
						break;
					}
					i++;
				}
			}
			if (!orderMismatch && missingPackages.Count == 0 && excessPackages.Count == 0)
			{
				return true;
			}
			if (missingPackages.Count == 1)
			{
				errorMsg = TextManager.GetWithVariable("campaignmode.missingcontentpackage", "[missingcontentpackage]", missingPackages[0], FormatCapitals.No);
			}
			else if (missingPackages.Count > 1)
			{
				errorMsg = TextManager.GetWithVariable("campaignmode.missingcontentpackages", "[missingcontentpackages]", string.Join(", ", missingPackages), FormatCapitals.No);
			}
			if (excessPackages.Count == 1)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.incompatiblecontentpackage", "[incompatiblecontentpackage]", excessPackages[0], FormatCapitals.No);
			}
			else if (excessPackages.Count > 1)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.incompatiblecontentpackages", "[incompatiblecontentpackages]", string.Join(", ", excessPackages), FormatCapitals.No);
			}
			if (orderMismatch)
			{
				if (!errorMsg.IsNullOrEmpty())
				{
					errorMsg += "\n";
				}
				errorMsg += TextManager.GetWithVariable("campaignmode.contentpackageordermismatch", "[loadorder]", string.Join(", ", contentPackageNames), FormatCapitals.No);
			}
			return false;
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x000ED2A0 File Offset: 0x000EB4A0
		public void Save(string filePath, bool isSavingOnLoading)
		{
			CampaignMode campaign = this.GameMode as CampaignMode;
			if (campaign == null)
			{
				throw new NotSupportedException("GameSessions can only be saved when playing in a campaign mode.");
			}
			XDocument doc = new XDocument(new object[]
			{
				new XElement("Gamesession")
			});
			XElement root = doc.Root;
			if (root == null)
			{
				throw new NullReferenceException("Game session XML element is invalid: document is null.");
			}
			XElement rootElement = root;
			rootElement.Add(new XAttribute("savetime", SerializableDateTime.UtcNow.ToUnixTime()));
			XContainer xcontainer = rootElement;
			XName name = "currentlocation";
			Map map = this.Map;
			object obj;
			if (map == null)
			{
				obj = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				obj = ((currentLocation != null) ? currentLocation.NameIdentifier.Value : null);
			}
			xcontainer.Add(new XAttribute(name, obj ?? string.Empty));
			XContainer xcontainer2 = rootElement;
			XName name2 = "currentlocationnameformatindex";
			Map map2 = this.Map;
			int? num;
			if (map2 == null)
			{
				num = null;
			}
			else
			{
				Location currentLocation2 = map2.CurrentLocation;
				num = ((currentLocation2 != null) ? new int?(currentLocation2.NameFormatIndex) : null);
			}
			int? num2 = num;
			xcontainer2.Add(new XAttribute(name2, num2.GetValueOrDefault(-1)));
			XContainer xcontainer3 = rootElement;
			XName name3 = "locationtype";
			Map map3 = this.Map;
			Identifier? identifier;
			if (map3 == null)
			{
				identifier = null;
			}
			else
			{
				Location currentLocation3 = map3.CurrentLocation;
				if (currentLocation3 == null)
				{
					identifier = null;
				}
				else
				{
					LocationType type = currentLocation3.Type;
					identifier = ((type != null) ? new Identifier?(type.Identifier) : null);
				}
			}
			xcontainer3.Add(new XAttribute(name3, identifier ?? Identifier.Empty));
			XContainer xcontainer4 = rootElement;
			XName name4 = "nextleveltype";
			LevelData nextLevel = campaign.NextLevel;
			LevelData.LevelType levelType;
			if (nextLevel == null)
			{
				LevelData levelData = this.LevelData;
				levelType = ((levelData != null) ? levelData.Type : LevelData.LevelType.Outpost);
			}
			else
			{
				levelType = nextLevel.Type;
			}
			xcontainer4.Add(new XAttribute(name4, levelType));
			rootElement.Add(new XAttribute("ismultiplayer", campaign is MultiPlayerCampaign));
			this.LastSaveVersion = GameMain.Version;
			rootElement.Add(new XAttribute("version", GameMain.Version));
			Submarine submarine = this.Submarine;
			if (((submarine != null) ? submarine.Info : null) != null && !this.Submarine.Removed && this.Campaign != null)
			{
				bool hasNewPendingSub = this.Campaign.PendingSubmarineSwitch != null && this.Campaign.PendingSubmarineSwitch.MD5Hash.StringRepresentation != this.Submarine.Info.MD5Hash.StringRepresentation;
				if (hasNewPendingSub)
				{
					this.Campaign.SwitchSubs();
				}
			}
			rootElement.Add(new XAttribute("submarine", (this.SubmarineInfo == null) ? "" : this.SubmarineInfo.Name));
			if (this.OwnedSubmarines != null)
			{
				List<string> ownedSubmarineNames = new List<string>();
				XElement ownedSubsElement = new XElement("ownedsubmarines");
				rootElement.Add(ownedSubsElement);
				foreach (SubmarineInfo ownedSub in this.OwnedSubmarines)
				{
					ownedSubsElement.Add(new XElement("sub", new XAttribute("name", ownedSub.Name)));
				}
			}
			if (this.Map != null)
			{
				rootElement.Add(new XAttribute("mapseed", this.Map.Seed));
			}
			rootElement.Add(new XAttribute("selectedcontentpackagenames", string.Join("|", from cp in ContentPackageManager.EnabledPackages.All
			where cp.HasMultiplayerSyncedContent
			select cp.Name.Replace("|", "\\|"))));
			XElement permadeathsElement = new XElement("permadeaths");
			foreach (KeyValuePair<Option<AccountId>, int> kvp in this.permadeathsPerAccount)
			{
				AccountId accountId;
				if (kvp.Key.TryUnwrap(out accountId))
				{
					permadeathsElement.Add(new XElement("account", new object[]
					{
						new XAttribute("id", accountId.StringRepresentation),
						new XAttribute("permadeathcount", kvp.Value)
					}));
				}
			}
			rootElement.Add(permadeathsElement);
			XContainer xcontainer5 = rootElement;
			XName name5 = "respawnmode";
			NetworkMember networkMember = GameMain.NetworkMember;
			RespawnMode? respawnMode;
			if (networkMember == null)
			{
				respawnMode = null;
			}
			else
			{
				ServerSettings serverSettings = networkMember.ServerSettings;
				respawnMode = ((serverSettings != null) ? new RespawnMode?(serverSettings.RespawnMode) : null);
			}
			RespawnMode? respawnMode2 = respawnMode;
			xcontainer5.Add(new XAttribute(name5, respawnMode2.GetValueOrDefault()));
			((CampaignMode)this.GameMode).Save(doc.Root, isSavingOnLoading);
			doc.SaveSafe(filePath, SaveOptions.None, true, 0);
		}

		// Token: 0x0600235E RID: 9054 RVA: 0x000ED7C8 File Offset: 0x000EB9C8
		[CompilerGenerated]
		internal static bool <StartRound>g__tryCreateFaction|93_5(Identifier factionIdentifier, Location[] locations, Action<Location, Faction> setter)
		{
			if (factionIdentifier.IsEmpty)
			{
				return false;
			}
			FactionPrefab prefab;
			if (!FactionPrefab.Prefabs.TryGet(factionIdentifier, out prefab))
			{
				return false;
			}
			if (locations.Length == 0)
			{
				return false;
			}
			Faction newFaction = new Faction(null, prefab);
			for (int i = 0; i < locations.Length; i++)
			{
				setter(locations[i], newFaction);
			}
			return true;
		}

		// Token: 0x0600235F RID: 9055 RVA: 0x000ED818 File Offset: 0x000EBA18
		[CompilerGenerated]
		internal static bool <TryGenerateStationAroundModule>g__IsSuitableLocationType|94_7(IEnumerable<Identifier> allowedLocationTypes, Identifier locationType)
		{
			return allowedLocationTypes.None(null) || allowedLocationTypes.Contains("Any".ToIdentifier()) || allowedLocationTypes.Contains(locationType);
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x000ED840 File Offset: 0x000EBA40
		[CompilerGenerated]
		private int <EndRound>g__GetAmountOfMoney|105_0(IEnumerable<Character> crew)
		{
			CampaignMode campaign = this.GameMode as CampaignMode;
			if (campaign == null)
			{
				return 0;
			}
			int result;
			if (GameMain.NetworkMember == null)
			{
				result = campaign.Bank.Balance;
			}
			else
			{
				result = crew.Sum((Character c) => c.Wallet.Balance) + campaign.Bank.Balance;
			}
			return result;
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x000ED8A8 File Offset: 0x000EBAA8
		[CompilerGenerated]
		internal static int <ValidatedDisembarkPoints>g__GetTotalCost|108_0([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<DisembarkPerkPrefab> perksToCheck, ref GameSession.<>c__DisplayClass108_0 A_1)
		{
			if ((A_1.preset == GameModePreset.Mission || A_1.preset == GameModePreset.PvP) && GameSession.ShouldIgnorePerksThatCanNotApplyWithoutSubmarine(A_1.preset, A_1.missionTypes))
			{
				perksToCheck = (from p in perksToCheck
				where p.PerkBehaviors.All((PerkBase b) => b.CanApplyWithoutSubmarine())
				select p).ToImmutableArray<DisembarkPerkPrefab>();
			}
			return perksToCheck.Sum((DisembarkPerkPrefab p) => p.Cost);
		}

		// Token: 0x040010FF RID: 4351
		public readonly EventManager EventManager;

		// Token: 0x04001100 RID: 4352
		[Nullable(2)]
		public GameMode GameMode;

		// Token: 0x04001101 RID: 4353
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private Location[] dummyLocations;

		// Token: 0x04001102 RID: 4354
		[Nullable(2)]
		public CrewManager CrewManager;

		// Token: 0x04001104 RID: 4356
		public double TimeSpentCleaning;

		// Token: 0x04001105 RID: 4357
		public double TimeSpentPainting;

		// Token: 0x04001106 RID: 4358
		private readonly List<Mission> missions = new List<Mission>();

		// Token: 0x04001107 RID: 4359
		private readonly HashSet<Character> casualties = new HashSet<Character>();

		// Token: 0x04001108 RID: 4360
		[Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		private Dictionary<Option<AccountId>, int> permadeathsPerAccount = new Dictionary<Option<AccountId>, int>();

		// Token: 0x04001109 RID: 4361
		public CharacterTeamType? WinningTeam;

		// Token: 0x04001111 RID: 4369
		[Nullable(2)]
		public SubmarineInfo ForceOutpostModule;

		// Token: 0x04001112 RID: 4370
		public List<SubmarineInfo> OwnedSubmarines = new List<SubmarineInfo>();

		// Token: 0x04001114 RID: 4372
		[TupleElementNames(new string[]
		{
			"team",
			"identifier"
		})]
		[Nullable(new byte[]
		{
			1,
			0
		})]
		private readonly HashSet<ValueTuple<CharacterTeamType, Identifier>> unlockedRecipes = new HashSet<ValueTuple<CharacterTeamType, Identifier>>();

		// Token: 0x04001116 RID: 4374
		private double LastEndRoundErrorMessageTime;

		// Token: 0x02000992 RID: 2450
		[NullableContext(0)]
		public enum InfoFrameTab
		{
			// Token: 0x040033BF RID: 13247
			Crew,
			// Token: 0x040033C0 RID: 13248
			Mission,
			// Token: 0x040033C1 RID: 13249
			MyCharacter,
			// Token: 0x040033C2 RID: 13250
			Traitor
		}
	}
}
