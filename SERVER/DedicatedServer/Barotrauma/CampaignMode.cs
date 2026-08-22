using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000031 RID: 49
	internal abstract class CampaignMode : GameMode
	{
		// Token: 0x17000193 RID: 403
		// (get) Token: 0x060005D5 RID: 1493 RVA: 0x00035F6E File Offset: 0x0003416E
		// (set) Token: 0x060005D6 RID: 1494 RVA: 0x00035F76 File Offset: 0x00034176
		public bool MirrorLevel { get; protected set; }

		// Token: 0x060005D7 RID: 1495 RVA: 0x00035F7F File Offset: 0x0003417F
		private static bool IsOwner(Client client)
		{
			return client != null && client.Connection == GameMain.Server.OwnerConnection;
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x00035F98 File Offset: 0x00034198
		public static bool AllowedToManageCampaign(Client client, ClientPermissions permissions)
		{
			return client.HasPermission(permissions) || client.HasPermission(ClientPermissions.ManageCampaign) || CampaignMode.IsOwner(client) || CampaignMode.AnyOneAllowedToManageCampaign(permissions);
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x00035FBD File Offset: 0x000341BD
		public static bool AllowImmediateItemDelivery(Client client)
		{
			return client != null && GameMain.Server != null && (GameMain.Server.ServerSettings.AllowImmediateItemDelivery || client.HasPermission(ClientPermissions.ManageCampaign) || client.Connection == GameMain.Server.OwnerConnection);
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x00035FFA File Offset: 0x000341FA
		public static bool AllowedToManageWallets(Client client)
		{
			return CampaignMode.AllowedToManageCampaign(client, ClientPermissions.ManageMoney);
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x00036008 File Offset: 0x00034208
		public override void ShowStartMessage()
		{
			foreach (Mission mission in this.Missions)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("Mission"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
				GameServer.Log(mission.Description.Value, ServerLog.MessageType.ServerMessage);
			}
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x000360A0 File Offset: 0x000342A0
		public static bool HostileFactionDisablesInteraction(CampaignMode.InteractionType interactionType)
		{
			return interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Store && interactionType != CampaignMode.InteractionType.Examine;
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x000360B2 File Offset: 0x000342B2
		public static bool BlocksInteraction(CampaignMode.InteractionType interactionType)
		{
			return interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Cargo;
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x000360C1 File Offset: 0x000342C1
		public IReadOnlyList<Faction> Factions
		{
			get
			{
				return this.factions;
			}
		}

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x060005DF RID: 1503 RVA: 0x000360C9 File Offset: 0x000342C9
		// (set) Token: 0x060005E0 RID: 1504 RVA: 0x000360D1 File Offset: 0x000342D1
		protected XElement ActiveOrdersElement { get; set; }

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x060005E1 RID: 1505 RVA: 0x000360DA File Offset: 0x000342DA
		// (set) Token: 0x060005E2 RID: 1506 RVA: 0x000360E2 File Offset: 0x000342E2
		public bool IsFirstRound { get; protected set; } = true;

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x060005E3 RID: 1507 RVA: 0x000360EB File Offset: 0x000342EB
		public bool DisableEvents
		{
			get
			{
				return this.IsFirstRound && GameMain.GameSession.RoundDuration < 0f;
			}
		}

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x00036108 File Offset: 0x00034308
		// (set) Token: 0x060005E5 RID: 1509 RVA: 0x00036110 File Offset: 0x00034310
		public bool TransferItemsOnSubSwitch { get; set; }

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00036119 File Offset: 0x00034319
		// (set) Token: 0x060005E7 RID: 1511 RVA: 0x00036121 File Offset: 0x00034321
		public bool SwitchedSubsThisRound { get; private set; }

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x0003612A File Offset: 0x0003432A
		public Map Map
		{
			get
			{
				return this.map;
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x060005E9 RID: 1513 RVA: 0x00036134 File Offset: 0x00034334
		public override IEnumerable<Mission> Missions
		{
			get
			{
				CampaignMode.<get_Missions>d__62 <get_Missions>d__ = new CampaignMode.<get_Missions>d__62(-2);
				<get_Missions>d__.<>4__this = this;
				return <get_Missions>d__;
			}
		}

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x060005EA RID: 1514 RVA: 0x00036151 File Offset: 0x00034351
		public Location CurrentLocation
		{
			get
			{
				Map map = this.Map;
				if (map == null)
				{
					return null;
				}
				return map.CurrentLocation;
			}
		}

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x060005EB RID: 1515 RVA: 0x00036164 File Offset: 0x00034364
		// (set) Token: 0x060005EC RID: 1516 RVA: 0x0003616C File Offset: 0x0003436C
		public LevelData NextLevel { get; protected set; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x060005ED RID: 1517 RVA: 0x00036175 File Offset: 0x00034375
		// (set) Token: 0x060005EE RID: 1518 RVA: 0x0003617D File Offset: 0x0003437D
		public virtual bool PurchasedHullRepairs { get; set; }

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x060005EF RID: 1519 RVA: 0x00036186 File Offset: 0x00034386
		// (set) Token: 0x060005F0 RID: 1520 RVA: 0x0003618E File Offset: 0x0003438E
		public virtual bool PurchasedLostShuttles { get; set; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x060005F1 RID: 1521 RVA: 0x00036197 File Offset: 0x00034397
		// (set) Token: 0x060005F2 RID: 1522 RVA: 0x0003619F File Offset: 0x0003439F
		public virtual bool PurchasedItemRepairs { get; set; }

		// Token: 0x060005F3 RID: 1523 RVA: 0x000361A8 File Offset: 0x000343A8
		private static bool AnyOneAllowedToManageCampaign(ClientPermissions permissions)
		{
			if (GameMain.NetworkMember == null)
			{
				return true;
			}
			if (GameMain.NetworkMember.ConnectedClients.Count == 1)
			{
				return true;
			}
			if (GameMain.NetworkMember.GameStarted)
			{
				bool someOneHasPermissions = GameMain.NetworkMember.ConnectedClients.Any((Client c) => CampaignMode.IsOwner(c) || c.HasPermission(permissions));
				return !someOneHasPermissions || ((GameMain.GameSession == null || GameMain.GameSession.RoundDuration >= 60f) && GameMain.NetworkMember.ConnectedClients.None(delegate(Client c)
				{
					if (c.InGame)
					{
						Character character = c.Character;
						if (character != null && !character.IsIncapacitated && !character.IsDead)
						{
							return CampaignMode.IsOwner(c) || c.HasPermission(permissions);
						}
					}
					return false;
				}));
			}
			return GameMain.NetworkMember.ConnectedClients.None((Client c) => CampaignMode.IsOwner(c) || c.HasPermission(permissions));
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00036260 File Offset: 0x00034460
		protected CampaignMode(GameModePreset preset, CampaignSettings settings) : base(preset)
		{
			this.Settings = settings;
			this.Bank = new Wallet(Option<Character>.None())
			{
				Balance = settings.InitialMoney
			};
			this.CargoManager = new CargoManager(this);
			this.MedicalClinic = new MedicalClinic(this);
			this.CampaignMetadata = new CampaignMetadata();
			Identifier messageIdentifier = new Identifier("money");
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x000362F9 File Offset: 0x000344F9
		public virtual Wallet GetWallet(Client client = null)
		{
			return this.Bank;
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00036301 File Offset: 0x00034501
		public virtual bool TryPurchase(Client client, int price)
		{
			return price == 0 || this.GetWallet(client).TryDeduct(price);
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00036315 File Offset: 0x00034515
		public virtual int GetBalance(Client client = null)
		{
			return this.GetWallet(client).Balance;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00036323 File Offset: 0x00034523
		public bool CanAfford(int cost, Client client = null)
		{
			return this.GetBalance(client) >= cost;
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00036334 File Offset: 0x00034534
		public Location GetCurrentDisplayLocation()
		{
			Level loaded = Level.Loaded;
			LevelData levelData;
			Submarine submarine;
			if (((loaded != null) ? loaded.EndLocation : null) != null && !Level.Loaded.Generating && Level.Loaded.Type == LevelData.LevelType.LocationConnection && this.GetAvailableTransition(out levelData, out submarine) == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
			{
				return Level.Loaded.EndLocation;
			}
			Level loaded2 = Level.Loaded;
			return ((loaded2 != null) ? loaded2.StartLocation : null) ?? this.Map.CurrentLocation;
		}

		// Token: 0x060005FA RID: 1530 RVA: 0x000363A8 File Offset: 0x000345A8
		public static List<Submarine> GetSubsToLeaveBehind(Submarine leavingSub)
		{
			return Submarine.Loaded.FindAll((Submarine sub) => sub != leavingSub && !leavingSub.DockedTo.Contains(sub) && sub.Info.Type == SubmarineType.Player && sub.TeamID == CharacterTeamType.Team1 && !sub.IsRespawnShuttle && (sub.AtEndExit != leavingSub.AtEndExit || sub.AtStartExit != leavingSub.AtStartExit));
		}

		// Token: 0x060005FB RID: 1531 RVA: 0x000363D8 File Offset: 0x000345D8
		public SubmarineInfo GetPredefinedStartOutpost()
		{
			Map map = this.Map;
			OutpostGenerationParams outpostGenerationParams;
			if (map == null)
			{
				outpostGenerationParams = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					outpostGenerationParams = null;
				}
				else
				{
					LocationType type = currentLocation.Type;
					outpostGenerationParams = ((type != null) ? type.GetForcedOutpostGenerationParams() : null);
				}
			}
			OutpostGenerationParams parameters = outpostGenerationParams;
			if (parameters != null && !parameters.OutpostFilePath.IsNullOrEmpty())
			{
				return new SubmarineInfo(parameters.OutpostFilePath.Value, "", null, true, false)
				{
					OutpostGenerationParams = parameters
				};
			}
			return null;
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00036444 File Offset: 0x00034644
		public override void Start()
		{
			base.Start();
			this.dialogLastSpoken.Clear();
			this.characterOutOfBoundsTimer.Clear();
			foreach (Faction faction in this.factions)
			{
				faction.Reputation.ReputationAtRoundStart = faction.Reputation.Value;
			}
			if (this.PurchasedHullRepairsInLatestSave)
			{
				foreach (Structure wall in Structure.WallList)
				{
					if (wall.Submarine != null && wall.Submarine.Info.Type == SubmarineType.Player && (wall.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(wall.Submarine)))
					{
						for (int i = 0; i < wall.SectionCount; i++)
						{
							wall.SetDamage(i, 0f, null, false, true, false, false);
						}
					}
				}
				this.PurchasedHullRepairsInLatestSave = (this.PurchasedHullRepairs = false);
			}
			if (this.PurchasedItemRepairsInLatestSave)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.Submarine != null && item.Submarine.Info.Type == SubmarineType.Player && (item.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(item.Submarine)) && item.GetComponent<Repairable>() != null)
					{
						item.Condition = item.MaxCondition;
					}
				}
				this.PurchasedItemRepairsInLatestSave = (this.PurchasedItemRepairs = false);
			}
			this.PurchasedLostShuttlesInLatestSave = (this.PurchasedLostShuttles = false);
			IEnumerable<Submarine> connectedSubs = Submarine.MainSub.GetConnectedSubs();
			this.wasDocked = (Level.Loaded.StartOutpost != null && connectedSubs.Contains(Level.Loaded.StartOutpost));
			this.SwitchedSubsThisRound = false;
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x00036680 File Offset: 0x00034880
		public static int GetHullRepairCost()
		{
			float totalDamage = 0f;
			foreach (Structure wall in Structure.WallList)
			{
				if (wall.Submarine != null && wall.Submarine.Info.Type == SubmarineType.Player && (wall.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(wall.Submarine)))
				{
					for (int i = 0; i < wall.SectionCount; i++)
					{
						totalDamage += wall.SectionDamage(i);
					}
				}
			}
			return (int)Math.Min(totalDamage * 0.1f, 600f);
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x0003673C File Offset: 0x0003493C
		public static int GetItemRepairCost()
		{
			float totalRepairDuration = 0f;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine != null && item.Submarine.Info.Type == SubmarineType.Player && (item.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(item.Submarine)))
				{
					Repairable repairable = item.GetComponent<Repairable>();
					if (repairable != null)
					{
						totalRepairDuration += repairable.FixDurationHighSkill * (1f - item.Condition / item.MaxCondition);
					}
				}
			}
			return (int)Math.Min(totalRepairDuration * 1f, 2000f);
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x00036804 File Offset: 0x00034A04
		public void InitFactions()
		{
			this.factions = new List<Faction>();
			foreach (FactionPrefab factionPrefab in FactionPrefab.Prefabs)
			{
				this.factions.Add(new Faction(this.CampaignMetadata, factionPrefab));
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000600 RID: 1536 RVA: 0x0003686C File Offset: 0x00034A6C
		// (remove) Token: 0x06000601 RID: 1537 RVA: 0x000368A4 File Offset: 0x00034AA4
		public event Action BeforeLevelLoading;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000602 RID: 1538 RVA: 0x000368DC File Offset: 0x00034ADC
		// (remove) Token: 0x06000603 RID: 1539 RVA: 0x00036914 File Offset: 0x00034B14
		public event Action OnSaveAndQuit;

		// Token: 0x06000604 RID: 1540 RVA: 0x0003694C File Offset: 0x00034B4C
		public override void AddExtraMissions(LevelData levelData)
		{
			if (levelData == null)
			{
				throw new ArgumentException("Level data was null.");
			}
			this.extraMissions.Clear();
			Location currentLocation = this.Map.CurrentLocation;
			if (currentLocation == null)
			{
				throw new InvalidOperationException("Current location was null.");
			}
			if (levelData.Type == LevelData.LevelType.Outpost)
			{
				using (IEnumerator<Mission> enumerator = currentLocation.AvailableMissions.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Mission availableMission = enumerator.Current;
						if (availableMission.Locations[0] == currentLocation && availableMission.Locations[1] == currentLocation)
						{
							currentLocation.SelectMission(availableMission);
						}
					}
					goto IL_7CB;
				}
			}
			foreach (Mission mission in currentLocation.SelectedMissions.ToList<Mission>())
			{
				if (mission.Locations[0] == currentLocation && mission.Locations[1] == currentLocation)
				{
					currentLocation.DeselectMission(mission);
				}
			}
			foreach (Mission mission2 in currentLocation.AvailableMissions)
			{
				if (!mission2.Prefab.ShowInMenus || mission2.Prefab.IsSideObjective)
				{
					currentLocation.SelectMission(mission2);
				}
			}
			if (levelData.HasBeaconStation && !levelData.IsBeaconActive)
			{
				if (this.Missions.None(delegate(Mission m)
				{
					Identifier type2 = m.Prefab.Type;
					return type2 == Tags.MissionTypeBeacon;
				}))
				{
					IEnumerable<MissionPrefab> beaconMissionPrefabs = MissionPrefab.Prefabs.Where(delegate(MissionPrefab m)
					{
						if (m.IsSideObjective)
						{
							Identifier type2 = m.Type;
							return type2 == Tags.MissionTypeBeacon;
						}
						return false;
					});
					if (beaconMissionPrefabs.Any<MissionPrefab>())
					{
						IEnumerable<MissionPrefab> filteredMissions = from m in beaconMissionPrefabs
						where levelData.Difficulty >= (float)m.MinLevelDifficulty && levelData.Difficulty <= (float)m.MaxLevelDifficulty
						select m;
						if (filteredMissions.None(null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
							defaultInterpolatedStringHandler.AppendLiteral("No suitable beacon mission found matching the level difficulty ");
							defaultInterpolatedStringHandler.AppendFormatted<float>(levelData.Difficulty);
							defaultInterpolatedStringHandler.AppendLiteral(". Ignoring the restriction.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							beaconMissionPrefabs = filteredMissions;
						}
						Random rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
						MissionPrefab beaconMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(beaconMissionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand);
						this.extraMissions.Add(beaconMissionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
					}
				}
			}
			if (levelData.HasHuntingGrounds)
			{
				IOrderedEnumerable<MissionPrefab> huntingGroundsMissionPrefabs = from m in MissionPrefab.Prefabs
				where m.IsSideObjective && m.Tags.Contains("huntinggrounds")
				orderby m.UintIdentifier
				select m;
				if (!huntingGroundsMissionPrefabs.Any<MissionPrefab>())
				{
					DebugConsole.AddWarning("Could not find a hunting grounds mission for the level. No mission with the tag \"huntinggrounds\" found.", null);
				}
				else
				{
					Random rand2 = new MTRandom(ToolBox.StringToInt(levelData.Seed));
					List<MissionPrefab> prefabs = huntingGroundsMissionPrefabs.ToList<MissionPrefab>();
					List<float> weights = (from p in prefabs
					select (float)Math.Max(p.Commonness, 1)).ToList<float>();
					for (int i = 0; i < prefabs.Count; i++)
					{
						MissionPrefab prefab = prefabs[i];
						float weight = weights[i];
						if (prefab.Tags.Contains("easy"))
						{
							weight *= MathHelper.Lerp(0.2f, 2f, MathUtils.InverseLerp(80f, 25f, levelData.Difficulty));
						}
						else if (prefab.Tags.Contains("hard"))
						{
							weight *= MathHelper.Lerp(0.5f, 1.5f, MathUtils.InverseLerp(35f, 80f, levelData.Difficulty));
						}
						weights[i] = weight;
					}
					MissionPrefab huntingGroundsMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(prefabs, weights, rand2);
					if (!this.Missions.Any((Mission m) => m.Prefab.Tags.Contains("huntinggrounds")))
					{
						this.extraMissions.Add(huntingGroundsMissionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
					}
				}
			}
			using (IEnumerator<Faction> enumerator4 = (from f in this.factions
			orderby f.Prefab.MenuOrder
			select f).GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					Faction faction = enumerator4.Current;
					ImmutableArray<FactionPrefab.AutomaticMission>.Enumerator enumerator5 = faction.Prefab.AutomaticMissions.GetEnumerator();
					Func<Location, bool> <>9__9;
					Func<Location, bool> <>9__10;
					while (enumerator5.MoveNext())
					{
						FactionPrefab.AutomaticMission automaticMission = enumerator5.Current;
						if (faction.Reputation.Value >= automaticMission.MinReputation && faction.Reputation.Value <= automaticMission.MaxReputation)
						{
							if (automaticMission.DisallowBetweenOtherFactionOutposts && levelData.Type == LevelData.LevelType.LocationConnection)
							{
								IEnumerable<Location> locations = this.Map.SelectedConnection.Locations;
								Func<Location, bool> predicate;
								if ((predicate = <>9__9) == null)
								{
									predicate = (<>9__9 = ((Location l) => l.Faction != null && l.Faction != faction));
								}
								if (locations.All(predicate))
								{
									continue;
								}
							}
							if (automaticMission.MaxDistanceFromFactionOutpost < 2147483647)
							{
								Location startLocation = currentLocation;
								int maxDistanceFromFactionOutpost = automaticMission.MaxDistanceFromFactionOutpost;
								Func<Location, bool> criteria;
								if ((criteria = <>9__10) == null)
								{
									criteria = (<>9__10 = ((Location loc) => loc.Faction == faction));
								}
								if (!Map.LocationOrConnectionWithinDistance(startLocation, maxDistanceFromFactionOutpost, criteria, null))
								{
									continue;
								}
							}
							Random rand3 = new MTRandom(ToolBox.StringToInt(levelData.Seed + this.TotalPassedLevels.ToString()));
							if (levelData.Type == automaticMission.LevelType)
							{
								float probability = MathHelper.Lerp(automaticMission.MinProbability, automaticMission.MaxProbability, MathUtils.InverseLerp(automaticMission.MinReputation, automaticMission.MaxReputation, faction.Reputation.Value));
								if (rand3.NextDouble() < (double)probability)
								{
									Func<Identifier, bool> <>9__13;
									IOrderedEnumerable<MissionPrefab> missionPrefabs = from m in MissionPrefab.Prefabs.Where(delegate(MissionPrefab m)
									{
										IEnumerable<Identifier> tags = m.Tags;
										Func<Identifier, bool> predicate2;
										if ((predicate2 = <>9__13) == null)
										{
											predicate2 = (<>9__13 = ((Identifier t) => t == automaticMission.MissionTag));
										}
										return tags.Any(predicate2);
									})
									orderby m.UintIdentifier
									select m;
									if (missionPrefabs.Any<MissionPrefab>())
									{
										MissionPrefab missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(missionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand3);
										Identifier type = missionPrefab.Type;
										if (type == Tags.MissionTypePirate)
										{
											if (this.Missions.Any(delegate(Mission m)
											{
												Identifier type2 = m.Prefab.Type;
												return type2 == Tags.MissionTypePirate;
											}))
											{
												continue;
											}
										}
										if (automaticMission.LevelType == LevelData.LevelType.Outpost)
										{
											this.extraMissions.Add(missionPrefab.Instantiate(new Location[]
											{
												currentLocation,
												currentLocation
											}, Submarine.MainSub));
										}
										else
										{
											this.extraMissions.Add(missionPrefab.Instantiate(this.Map.SelectedConnection.Locations, Submarine.MainSub));
										}
									}
								}
							}
						}
					}
				}
			}
			IL_7CB:
			if (levelData.Biome.IsEndBiome)
			{
				Identifier endMissionTag = Identifier.Empty;
				if (levelData.Type == LevelData.LevelType.LocationConnection)
				{
					int locationIndex = this.map.EndLocations.IndexOf(this.map.SelectedLocation);
					if (locationIndex > -1)
					{
						endMissionTag = ("endlevel_locationconnection_" + locationIndex.ToString()).ToIdentifier();
					}
				}
				else
				{
					int locationIndex2 = this.map.EndLocations.IndexOf(this.map.CurrentLocation);
					if (locationIndex2 > -1)
					{
						endMissionTag = ("endlevel_location_" + locationIndex2.ToString()).ToIdentifier();
					}
				}
				if (!endMissionTag.IsEmpty)
				{
					IOrderedEnumerable<MissionPrefab> endLevelMissionPrefabs = from m in MissionPrefab.Prefabs
					where m.Tags.Contains(endMissionTag)
					orderby m.UintIdentifier
					select m;
					if (endLevelMissionPrefabs.Any<MissionPrefab>())
					{
						Random rand4 = new MTRandom(ToolBox.StringToInt(levelData.Seed));
						MissionPrefab endLevelMissionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(endLevelMissionPrefabs, (MissionPrefab p) => (float)p.Commonness, rand4);
						if (this.Missions.All(delegate(Mission m)
						{
							Identifier type2 = m.Prefab.Type;
							Identifier type3 = endLevelMissionPrefab.Type;
							return type2 != type3;
						}))
						{
							if (levelData.Type == LevelData.LevelType.LocationConnection)
							{
								this.extraMissions.Add(endLevelMissionPrefab.Instantiate(this.map.SelectedConnection.Locations, Submarine.MainSub));
								return;
							}
							this.extraMissions.Add(endLevelMissionPrefab.Instantiate(new Location[]
							{
								this.map.CurrentLocation,
								this.map.CurrentLocation
							}, Submarine.MainSub));
						}
					}
				}
			}
		}

		// Token: 0x06000605 RID: 1541 RVA: 0x00037378 File Offset: 0x00035578
		public void LoadNewLevel()
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (CoroutineManager.IsCoroutineRunning("LevelTransition"))
			{
				DebugConsole.ThrowError("Level transition already running.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			Action beforeLevelLoading = this.BeforeLevelLoading;
			if (beforeLevelLoading != null)
			{
				beforeLevelLoading();
			}
			this.BeforeLevelLoading = null;
			if (Level.Loaded == null || Submarine.MainSub == null)
			{
				this.LoadInitialLevel();
				return;
			}
			LevelData nextLevel;
			Submarine leavingSub;
			CampaignMode.TransitionType availableTransition = this.GetAvailableTransition(out nextLevel, out leavingSub);
			if (availableTransition == CampaignMode.TransitionType.None)
			{
				LocalizedString left = "Failed to load a new campaign level. No available level transitions (current location: ";
				Location currentLocation = this.map.CurrentLocation;
				LocalizedString left2 = left + (((currentLocation != null) ? currentLocation.DisplayName : null) ?? "null") + ", " + "selected location: ";
				Location selectedLocation = this.map.SelectedLocation;
				LocalizedString left3 = left2 + (((selectedLocation != null) ? selectedLocation.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
				string text;
				if (leavingSub == null)
				{
					text = null;
				}
				else
				{
					SubmarineInfo info = leavingSub.Info;
					text = ((info != null) ? info.Name : null);
				}
				DebugConsole.ThrowErrorLocalized(left3 + (text ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (nextLevel == null)
			{
				LocalizedString left4 = "Failed to load a new campaign level. No available level transitions (transition type: " + availableTransition.ToString() + ", current location: ";
				Location currentLocation2 = this.map.CurrentLocation;
				LocalizedString left5 = left4 + (((currentLocation2 != null) ? currentLocation2.DisplayName : null) ?? "null") + ", " + "selected location: ";
				Location selectedLocation2 = this.map.SelectedLocation;
				LocalizedString left6 = left5 + (((selectedLocation2 != null) ? selectedLocation2.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
				string text2;
				if (leavingSub == null)
				{
					text2 = null;
				}
				else
				{
					SubmarineInfo info2 = leavingSub.Info;
					text2 = ((info2 != null) ? info2.Name : null);
				}
				DebugConsole.ThrowErrorLocalized(left6 + (text2 ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			LocalizedString left7 = "Transitioning to " + (((nextLevel != null) ? nextLevel.Seed : null) ?? "null") + " (current location: ";
			Location currentLocation3 = this.map.CurrentLocation;
			LocalizedString left8 = left7 + (((currentLocation3 != null) ? currentLocation3.DisplayName : null) ?? "null") + ", " + "selected location: ";
			Location selectedLocation3 = this.map.SelectedLocation;
			LocalizedString left9 = left8 + (((selectedLocation3 != null) ? selectedLocation3.DisplayName : null) ?? "null") + ", " + "leaving sub: ";
			string text3;
			if (leavingSub == null)
			{
				text3 = null;
			}
			else
			{
				SubmarineInfo info3 = leavingSub.Info;
				text3 = ((info3 != null) ? info3.Name : null);
			}
			DebugConsole.NewMessage(left9 + (text3 ?? "null") + ", " + "at start: " + (((leavingSub != null) ? leavingSub.AtStartExit.ToString() : null) ?? "null") + ", " + "at end: " + (((leavingSub != null) ? leavingSub.AtEndExit.ToString() : null) ?? "null") + ", " + "transition type: " + availableTransition + ")", null, false);
			this.IsFirstRound = false;
			bool mirror = this.map.SelectedConnection != null && this.map.CurrentLocation != this.map.SelectedConnection.Locations[0];
			CoroutineManager.StartCoroutine(this.DoLevelTransition(availableTransition, nextLevel, leavingSub, mirror), "LevelTransition");
		}

		// Token: 0x06000606 RID: 1542
		protected abstract void LoadInitialLevel();

		// Token: 0x06000607 RID: 1543
		protected abstract IEnumerable<CoroutineStatus> DoLevelTransition(CampaignMode.TransitionType transitionType, LevelData newLevel, Submarine leavingSub, bool mirror);

		// Token: 0x06000608 RID: 1544 RVA: 0x00037924 File Offset: 0x00035B24
		public CampaignMode.TransitionType GetAvailableTransition(out LevelData nextLevel, out Submarine leavingSub)
		{
			if (Level.Loaded == null || Submarine.MainSub == null)
			{
				nextLevel = null;
				leavingSub = null;
				return CampaignMode.TransitionType.None;
			}
			leavingSub = CampaignMode.GetLeavingSub();
			if (leavingSub == null)
			{
				nextLevel = null;
				return CampaignMode.TransitionType.None;
			}
			if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
			{
				if (leavingSub.AtEndExit)
				{
					if (Level.Loaded.EndLocation != null && Level.Loaded.EndLocation.Type.HasOutpost && Level.Loaded.EndOutpost != null)
					{
						nextLevel = Level.Loaded.EndLocation.LevelData;
						return CampaignMode.TransitionType.ProgressToNextLocation;
					}
					if (this.map.SelectedConnection != null)
					{
						nextLevel = this.map.SelectedConnection.LevelData;
						return CampaignMode.TransitionType.ProgressToNextEmptyLocation;
					}
					nextLevel = null;
					return CampaignMode.TransitionType.ProgressToNextEmptyLocation;
				}
				else
				{
					if (!leavingSub.AtStartExit)
					{
						nextLevel = null;
						return CampaignMode.TransitionType.None;
					}
					if (this.map.CurrentLocation.Type.HasOutpost && Level.Loaded.StartOutpost != null)
					{
						nextLevel = this.map.CurrentLocation.LevelData;
						return CampaignMode.TransitionType.ReturnToPreviousLocation;
					}
					if (this.map.SelectedLocation != null && this.map.SelectedLocation != this.map.CurrentLocation && !this.map.CurrentLocation.Type.HasOutpost && this.map.SelectedConnection != null && Level.Loaded.LevelData != this.map.SelectedConnection.LevelData)
					{
						nextLevel = this.map.SelectedConnection.LevelData;
						return CampaignMode.TransitionType.LeaveLocation;
					}
					LocationConnection selectedConnection = this.map.SelectedConnection;
					nextLevel = ((selectedConnection != null) ? selectedConnection.LevelData : null);
					return CampaignMode.TransitionType.ReturnToPreviousEmptyLocation;
				}
			}
			else
			{
				if (Level.Loaded.Type != LevelData.LevelType.Outpost)
				{
					throw new NotImplementedException();
				}
				int currentEndLocationIndex = this.map.EndLocations.IndexOf(this.map.CurrentLocation);
				if (currentEndLocationIndex > -1)
				{
					if (currentEndLocationIndex == this.map.EndLocations.Count - 1)
					{
						Location startLocation = this.map.StartLocation;
						nextLevel = ((startLocation != null) ? startLocation.LevelData : null);
						return CampaignMode.TransitionType.End;
					}
					if (leavingSub.AtEndExit && currentEndLocationIndex < this.map.EndLocations.Count - 1)
					{
						Location location = this.map.EndLocations[currentEndLocationIndex + 1];
						nextLevel = ((location != null) ? location.LevelData : null);
						return CampaignMode.TransitionType.ProgressToNextLocation;
					}
					nextLevel = null;
					return CampaignMode.TransitionType.None;
				}
				else
				{
					LevelData levelData;
					if (this.map.SelectedLocation != null)
					{
						LocationConnection selectedConnection2 = this.map.SelectedConnection;
						levelData = ((selectedConnection2 != null) ? selectedConnection2.LevelData : null);
					}
					else
					{
						levelData = null;
					}
					nextLevel = levelData;
					if (nextLevel != null)
					{
						return CampaignMode.TransitionType.LeaveLocation;
					}
					return CampaignMode.TransitionType.None;
				}
			}
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x00037B90 File Offset: 0x00035D90
		public CampaignMode.TransitionType GetAvailableTransition()
		{
			LevelData levelData;
			Submarine submarine;
			return this.GetAvailableTransition(out levelData, out submarine);
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x00037BA8 File Offset: 0x00035DA8
		private static Submarine GetLeavingSub()
		{
			if (Level.IsLoadedOutpost)
			{
				return Submarine.MainSub;
			}
			IEnumerable<Character> leavingPlayers = from c in Character.CharacterList
			where !c.IsDead && (c == Character.Controlled || c.IsRemotePlayer)
			select c;
			Character character = leavingPlayers.FirstOrDefault<Character>();
			CharacterTeamType submarineTeam = (character != null) ? character.TeamID : CharacterTeamType.Team1;
			Submarine leavingSubAtStart = CampaignMode.<GetLeavingSub>g__GetLeavingSubAtStart|112_3(leavingPlayers, submarineTeam);
			Submarine leavingSubAtEnd = CampaignMode.<GetLeavingSub>g__GetLeavingSubAtEnd|112_4(leavingPlayers, submarineTeam);
			int playersInSubAtStart = (leavingSubAtStart == null || !leavingSubAtStart.AtStartExit) ? 0 : leavingPlayers.Count((Character c) => c.Submarine == leavingSubAtStart || leavingSubAtStart.DockedTo.Contains(c.Submarine) || (Level.Loaded.StartOutpost != null && c.Submarine == Level.Loaded.StartOutpost));
			int playersInSubAtEnd = (leavingSubAtEnd == null || !leavingSubAtEnd.AtEndExit) ? 0 : leavingPlayers.Count((Character c) => c.Submarine == leavingSubAtEnd || leavingSubAtEnd.DockedTo.Contains(c.Submarine) || (Level.Loaded.EndOutpost != null && c.Submarine == Level.Loaded.EndOutpost));
			if (playersInSubAtStart == 0 && playersInSubAtEnd == 0)
			{
				return null;
			}
			if (playersInSubAtStart <= playersInSubAtEnd)
			{
				return leavingSubAtEnd;
			}
			return leavingSubAtStart;
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00037C94 File Offset: 0x00035E94
		public override void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
			List<Item> takenItems = new List<Item>();
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.SpawnedInCurrentOutpost && item.OriginalModuleIndex >= 0)
					{
						Entity owner = item.GetRootInventoryOwner();
						bool? flag;
						if (owner == null)
						{
							flag = null;
						}
						else
						{
							Submarine submarine = owner.Submarine;
							if (submarine == null)
							{
								flag = null;
							}
							else
							{
								SubmarineInfo info = submarine.Info;
								flag = ((info != null) ? new bool?(info.IsOutpost) : null);
							}
						}
						bool? flag2 = flag;
						if (flag2.GetValueOrDefault())
						{
							Character character = owner as Character;
							if ((character == null || character.TeamID != CharacterTeamType.Team1) && item.Submarine != null && item.Submarine.Info.IsOutpost)
							{
								continue;
							}
						}
						takenItems.Add(item);
					}
				}
			}
			if (this.map != null && this.CargoManager != null)
			{
				this.map.CurrentLocation.RegisterTakenItems(takenItems);
				if (transitionType != CampaignMode.TransitionType.None)
				{
					this.UpdateStoreStock();
				}
			}
			if (GameMain.NetworkMember == null)
			{
				CargoManager cargoManager = this.CargoManager;
				if (cargoManager != null)
				{
					cargoManager.ClearItemsInBuyCrate();
				}
				CargoManager cargoManager2 = this.CargoManager;
				if (cargoManager2 != null)
				{
					cargoManager2.ClearItemsInSellCrate();
				}
				CargoManager cargoManager3 = this.CargoManager;
				if (cargoManager3 != null)
				{
					cargoManager3.ClearItemsInSellFromSubCrate();
				}
			}
			else if (GameMain.NetworkMember.IsServer)
			{
				CargoManager cargoManager4 = this.CargoManager;
				if (cargoManager4 != null)
				{
					cargoManager4.ClearItemsInBuyCrate();
				}
				CargoManager cargoManager5 = this.CargoManager;
				if (cargoManager5 != null)
				{
					cargoManager5.ClearItemsInSellFromSubCrate();
				}
			}
			else if (GameMain.NetworkMember.IsClient)
			{
				CargoManager cargoManager6 = this.CargoManager;
				if (cargoManager6 != null)
				{
					cargoManager6.ClearItemsInSellCrate();
				}
			}
			Level loaded2 = Level.Loaded;
			if (((loaded2 != null) ? loaded2.StartOutpost : null) != null)
			{
				List<Character> killedCharacters = new List<Character>();
				foreach (Character c3 in Level.Loaded.StartOutpost.Info.OutpostNPCs.SelectMany((KeyValuePair<Identifier, List<Character>> kpv) => kpv.Value))
				{
					if (c3.IsDead || c3.Removed)
					{
						killedCharacters.Add(c3);
					}
				}
				this.map.CurrentLocation.RegisterKilledCharacters(killedCharacters);
				Level.Loaded.StartOutpost.Info.OutpostNPCs.Clear();
			}
			List<Character> deadCharacters = Character.CharacterList.FindAll((Character c) => c.IsDead);
			foreach (Character c2 in deadCharacters)
			{
				if (c2.IsDead)
				{
					base.CrewManager.RemoveCharacterInfo(c2.Info);
					c2.DespawnNow(false);
				}
			}
			foreach (Item item2 in Item.ItemList.ToList<Item>())
			{
				if (item2.HasTag(Tags.IdCardTag))
				{
					Item container = item2.Container;
					if (container != null && container.HasTag(Tags.DespawnContainer))
					{
						item2.Remove();
					}
				}
			}
			foreach (CharacterInfo ci in base.CrewManager.GetCharacterInfos(false).ToList<CharacterInfo>())
			{
				if (ci.CauseOfDeath != null)
				{
					base.CrewManager.RemoveCharacterInfo(ci);
				}
			}
			foreach (DockingPort port in DockingPort.List)
			{
				if (port.Door != null & port.Item.Submarine.Info.Type == SubmarineType.Player)
				{
					DockingPort dockingTarget = port.DockingTarget;
					bool flag3;
					if (dockingTarget == null)
					{
						flag3 = (null != null);
					}
					else
					{
						Item item4 = dockingTarget.Item;
						flag3 = (((item4 != null) ? item4.Submarine : null) != null);
					}
					if (flag3 && port.DockingTarget.Item.Submarine.Info.IsOutpost)
					{
						port.Door.IsOpen = false;
					}
				}
			}
			foreach (Item item3 in Item.ItemList)
			{
				Submarine sub = item3.Submarine;
				if (sub != null && sub.Info.IsPlayer && (sub.TeamID == CharacterTeamType.Team1 || sub.TeamID == CharacterTeamType.Team2))
				{
					Reactor reactor = item3.GetComponent<Reactor>();
					if (reactor != null && reactor.LastAIUser != null && reactor.LastUser == reactor.LastAIUser)
					{
						reactor.AutoTemp = true;
					}
				}
			}
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x000381CC File Offset: 0x000363CC
		public void HandleSaveAndQuit()
		{
			Action onSaveAndQuit = this.OnSaveAndQuit;
			if (onSaveAndQuit != null)
			{
				onSaveAndQuit();
			}
			this.OnSaveAndQuit = null;
			if (Level.IsLoadedFriendlyOutpost)
			{
				this.UpdateStoreStock();
			}
			GameMain.GameSession.EndMissions(CampaignMode.TransitionType.None);
			EventManager eventManager = GameMain.GameSession.EventManager;
			if (eventManager == null)
			{
				return;
			}
			eventManager.StoreEventDataAtRoundEnd(true);
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x00038220 File Offset: 0x00036420
		public void UpdateStoreStock()
		{
			Map map = this.Map;
			if (map != null)
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation != null)
				{
					currentLocation.AddStock(this.CargoManager.SoldItems);
				}
			}
			CargoManager cargoManager = this.CargoManager;
			if (cargoManager != null)
			{
				cargoManager.ClearSoldItemsProjSpecific();
			}
			Map map2 = this.Map;
			if (map2 == null)
			{
				return;
			}
			Location currentLocation2 = map2.CurrentLocation;
			if (currentLocation2 == null)
			{
				return;
			}
			currentLocation2.RemoveStock(this.CargoManager.PurchasedItems);
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x0003828C File Offset: 0x0003648C
		public void EndCampaign()
		{
			foreach (Character c in Character.CharacterList)
			{
				if (c.IsOnPlayerTeam)
				{
					c.CharacterHealth.RemoveNegativeAfflictions();
				}
			}
			foreach (LocationConnection connection in this.Map.Connections)
			{
				connection.Difficulty = connection.Biome.AdjustedMaxDifficulty;
				connection.LevelData = new LevelData(connection)
				{
					IsBeaconActive = false,
					ForceOutpostGenerationParams = connection.LevelData.ForceOutpostGenerationParams
				};
				connection.LevelData.HasHuntingGrounds = connection.LevelData.OriginallyHadHuntingGrounds;
			}
			foreach (Location location in this.Map.Locations)
			{
				location.LevelData = new LevelData(location, this.Map, location.Biome.AdjustedMaxDifficulty)
				{
					ForceOutpostGenerationParams = location.LevelData.ForceOutpostGenerationParams
				};
				location.Reset(this);
			}
			this.Map.ClearLocationHistory();
			this.Map.SetLocation(this.Map.Locations.IndexOf(this.Map.StartLocation));
			this.Map.SelectLocation(-1);
			if (this.Map.Radiation != null)
			{
				this.Map.Radiation.Amount = this.Map.Radiation.Params.StartingRadiation;
			}
			foreach (Location location2 in this.Map.Locations)
			{
				location2.TurnsInRadiation = 0;
			}
			foreach (Faction faction in this.Factions)
			{
				faction.Reputation.SetReputation((float)faction.Prefab.InitialReputation);
			}
			this.EndCampaignProjSpecific();
			if (this.CampaignMetadata != null)
			{
				int loops = this.CampaignMetadata.GetInt("campaign.endings".ToIdentifier(), new int?(0));
				this.CampaignMetadata.SetValue("campaign.endings".ToIdentifier(), loops + 1);
			}
			this.Settings.TutorialEnabled = false;
			GameAnalyticsManager.ProgressionStatus progressionStatus = GameAnalyticsManager.ProgressionStatus.Complete;
			GameModePreset preset = base.Preset;
			GameAnalyticsManager.AddProgressionEvent(progressionStatus, ((preset != null) ? preset.Identifier.Value : null) ?? "none");
			string eventId = "FinishCampaign:";
			string str = eventId;
			string str2 = "Submarine:";
			Submarine mainSub = Submarine.MainSub;
			string text;
			if (mainSub == null)
			{
				text = null;
			}
			else
			{
				SubmarineInfo info = mainSub.Info;
				text = ((info != null) ? info.Name : null);
			}
			GameAnalyticsManager.AddDesignEvent(str + str2 + (text ?? "none"));
			string str3 = eventId;
			string str4 = "CrewSize:";
			CrewManager crewManager = base.CrewManager;
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
			GameAnalyticsManager.AddDesignEvent(str3 + str4 + num2.GetValueOrDefault().ToString());
			GameAnalyticsManager.AddDesignEvent(eventId + "Money", (double)this.Bank.Balance);
			GameAnalyticsManager.AddDesignEvent(eventId + "Playtime", this.TotalPlayTime);
			GameAnalyticsManager.AddDesignEvent(eventId + "PassedLevels", (double)this.TotalPassedLevels);
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00038660 File Offset: 0x00036860
		protected virtual void EndCampaignProjSpecific()
		{
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x00038662 File Offset: 0x00036862
		public Faction GetRandomFaction(Rand.RandSync randSync, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(this.Factions, randSync, false, allowEmpty);
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x00038672 File Offset: 0x00036872
		public Faction GetRandomSecondaryFaction(Rand.RandSync randSync, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(this.Factions, randSync, true, allowEmpty);
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x00038682 File Offset: 0x00036882
		public static Faction GetRandomFaction(IEnumerable<Faction> factions, Rand.RandSync randSync, bool secondary = false, bool allowEmpty = true)
		{
			return CampaignMode.GetRandomFaction(factions, Rand.GetRNG(randSync), secondary, allowEmpty);
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x00038694 File Offset: 0x00036894
		public static Faction GetRandomFaction(IEnumerable<Faction> factions, Random random, bool secondary = false, bool allowEmpty = true)
		{
			List<Faction> factionsList = (from f in factions
			orderby f.Prefab.Identifier
			select f).ToList<Faction>();
			List<float> weights = factionsList.Select(delegate(Faction f)
			{
				if (!secondary)
				{
					return f.Prefab.ControlledOutpostPercentage;
				}
				return f.Prefab.SecondaryControlledOutpostPercentage;
			}).ToList<float>();
			float percentageSum = weights.Sum();
			if (percentageSum < 100f && allowEmpty)
			{
				factionsList.Add(null);
				weights.Add(100f - percentageSum);
			}
			return ToolBox.SelectWeightedRandom<Faction>(factionsList, weights, random);
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x00038720 File Offset: 0x00036920
		public bool TryHireCharacter(Location location, CharacterInfo characterInfo, bool takeMoney = true, Client client = null, bool buyingNewCharacter = false)
		{
			if (characterInfo == null)
			{
				return false;
			}
			if (characterInfo.MinReputationToHire.Item1 != Identifier.Empty && MathF.Round(this.GetReputation(characterInfo.MinReputationToHire.Item1)) < characterInfo.MinReputationToHire.Item2)
			{
				return false;
			}
			int price = buyingNewCharacter ? this.NewCharacterCost(characterInfo) : HireManager.GetSalaryFor(characterInfo);
			if (takeMoney && !this.TryPurchase(client, price))
			{
				return false;
			}
			characterInfo.IsNewHire = true;
			characterInfo.Title = null;
			location.RemoveHireableCharacter(characterInfo);
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) is MultiPlayerCampaign)
			{
				base.CrewManager.ToggleReserveBenchStatus(characterInfo, client, true, true, false);
			}
			else
			{
				base.CrewManager.AddCharacterInfo(characterInfo);
			}
			int salary = characterInfo.Salary;
			GameAnalyticsManager.MoneySink moneySink = GameAnalyticsManager.MoneySink.Crew;
			Job job = characterInfo.Job;
			GameAnalyticsManager.AddMoneySpentEvent(salary, moneySink, ((job != null) ? job.Prefab.Identifier.Value : null) ?? "unknown");
			return true;
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x00038810 File Offset: 0x00036A10
		public int NewCharacterCost(CharacterInfo characterInfo)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			float characterCostPercentage = (networkMember != null) ? networkMember.ServerSettings.ReplaceCostPercentage : 100f;
			return (int)MathF.Round((float)HireManager.GetSalaryFor(characterInfo) * (characterCostPercentage / 100f));
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0003884D File Offset: 0x00036A4D
		public bool CanAffordNewCharacter(CharacterInfo characterInfo)
		{
			return this.CanAfford(this.NewCharacterCost(characterInfo), null);
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00038860 File Offset: 0x00036A60
		private void NPCInteract(Character npc, Character interactor)
		{
			if (!npc.AllowCustomInteract)
			{
				return;
			}
			HumanAIController humanAi = npc.AIController as HumanAIController;
			if (humanAi != null && !humanAi.AllowCampaignInteraction())
			{
				return;
			}
			string coroutineName = "DoCharacterWait." + ((npc != null) ? npc.ID : 0).ToString();
			if (!CoroutineManager.IsCoroutineRunning(coroutineName))
			{
				CoroutineManager.StartCoroutine(this.DoCharacterWait(npc, interactor), coroutineName);
			}
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x000388C4 File Offset: 0x00036AC4
		private IEnumerable<CoroutineStatus> DoCharacterWait(Character npc, Character interactor)
		{
			CampaignMode.<DoCharacterWait>d__126 <DoCharacterWait>d__ = new CampaignMode.<DoCharacterWait>d__126(-2);
			<DoCharacterWait>d__.<>3__npc = npc;
			<DoCharacterWait>d__.<>3__interactor = interactor;
			return <DoCharacterWait>d__;
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x000388DC File Offset: 0x00036ADC
		public void AssignNPCMenuInteraction(Character character, CampaignMode.InteractionType interactionType)
		{
			character.CampaignInteractionType = interactionType;
			if (character.CampaignInteractionType == CampaignMode.InteractionType.Store)
			{
				HumanPrefab humanPrefab = character.HumanPrefab;
				if (humanPrefab != null)
				{
					Identifier merchantId = humanPrefab.Identifier;
					character.MerchantIdentifier = merchantId;
					Location currentLocation = this.map.CurrentLocation;
					if (currentLocation != null)
					{
						Location.StoreInfo store = currentLocation.GetStore(merchantId);
						if (store != null)
						{
							store.SetMerchantFaction(character.Faction);
						}
					}
				}
			}
			character.DisableHealthWindow = (interactionType != CampaignMode.InteractionType.None && interactionType != CampaignMode.InteractionType.Examine && interactionType != CampaignMode.InteractionType.Talk);
			if (interactionType == CampaignMode.InteractionType.None)
			{
				character.SetCustomInteract(null, null);
				return;
			}
			character.SetCustomInteract(new Action<Character, Character>(this.NPCInteract), TextManager.Get("CampaignInteraction." + interactionType.ToString()));
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x0003898C File Offset: 0x00036B8C
		protected void KeepCharactersCloseToOutpost(float deltaTime)
		{
			if (!Level.IsLoadedFriendlyOutpost)
			{
				return;
			}
			Rectangle worldBorders = Submarine.MainSub.GetDockedBorders(true);
			worldBorders.Location += Submarine.MainSub.WorldPosition.ToPoint();
			foreach (Character c in Character.CharacterList)
			{
				if ((c != Character.Controlled && !c.IsRemotePlayer) || c.Removed || c.IsDead || c.IsIncapacitated || c.Submarine != null)
				{
					if (this.characterOutOfBoundsTimer.ContainsKey(c))
					{
						c.OverrideMovement = null;
						this.characterOutOfBoundsTimer.Remove(c);
					}
				}
				else if (c.WorldPosition.Y < (float)(worldBorders.Y - worldBorders.Height) - 3000f)
				{
					if (!this.characterOutOfBoundsTimer.ContainsKey(c))
					{
						this.characterOutOfBoundsTimer.Add(c, 0f);
					}
					else
					{
						Dictionary<Character, float> dictionary = this.characterOutOfBoundsTimer;
						Character key = c;
						dictionary[key] += deltaTime;
					}
				}
				else if (c.WorldPosition.Y > (float)(worldBorders.Y - worldBorders.Height) - 2500f && this.characterOutOfBoundsTimer.ContainsKey(c))
				{
					c.OverrideMovement = null;
					this.characterOutOfBoundsTimer.Remove(c);
				}
			}
			foreach (KeyValuePair<Character, float> character in this.characterOutOfBoundsTimer)
			{
				if (character.Value <= 0f && !base.IsSinglePlayer)
				{
					foreach (Client c2 in GameMain.Server.ConnectedClients)
					{
						GameMain.Server.SendDirectChatMessage(ChatMessage.Create(TextManager.Get("RadioAnnouncerName").Value, TextManager.Get("TooFarFromOutpostWarning").Value, ChatMessageType.Default, null, null, PlayerConnectionChangeType.None, null), c2);
					}
				}
				character.Key.OverrideMovement = new Vector2?(Vector2.UnitY * 10f);
				if (character.Value > 10f)
				{
					Vector2 teleportPos = character.Key.WorldPosition;
					teleportPos += Vector2.Normalize(Submarine.MainSub.WorldPosition - character.Key.WorldPosition) * 100f;
					character.Key.AnimController.SetPosition(ConvertUnits.ToSimUnits(teleportPos), false, true, false, true);
				}
			}
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00038CAC File Offset: 0x00036EAC
		public void OutpostNPCAttacked(Character npc, Character attacker, AttackResult attackResult)
		{
			if (npc == null || attacker == null || npc.IsDead || npc.IsInstigator)
			{
				return;
			}
			if (npc.TeamID != CharacterTeamType.FriendlyNPC)
			{
				return;
			}
			if (!attacker.IsRemotePlayer && attacker != Character.Controlled)
			{
				return;
			}
			Identifier faction2 = npc.Faction;
			if (faction2 != null)
			{
				Faction faction = this.Factions.FirstOrDefault(delegate(Faction f)
				{
					Prefab prefab = f.Prefab;
					Identifier faction3 = npc.Faction;
					return prefab.Identifier == faction3;
				});
				if (faction != null)
				{
					Reputation reputation = faction.Reputation;
					if (reputation == null)
					{
						return;
					}
					reputation.AddReputation(-attackResult.Damage * 0.025f, 20f);
					return;
				}
			}
			Map map = this.Map;
			Location location = (map != null) ? map.CurrentLocation : null;
			if (location != null)
			{
				Reputation reputation2 = location.Reputation;
				if (reputation2 == null)
				{
					return;
				}
				reputation2.AddReputation(-attackResult.Damage * 0.025f, 20f);
			}
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00038D9C File Offset: 0x00036F9C
		public Faction GetFaction(Identifier identifier)
		{
			return this.factions.Find((Faction f) => f.Prefab.Identifier == identifier);
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x00038DD0 File Offset: 0x00036FD0
		public float GetReputation(Identifier factionIdentifier)
		{
			CampaignMode.<>c__DisplayClass133_0 CS$<>8__locals1 = new CampaignMode.<>c__DisplayClass133_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.factionIdentifier = factionIdentifier;
			CampaignMode.<>c__DisplayClass133_0 CS$<>8__locals2 = CS$<>8__locals1;
			Identifier identifier = "location".ToIdentifier();
			Faction faction = (CS$<>8__locals2.factionIdentifier == identifier) ? this.factions.Find(delegate(Faction f)
			{
				Map map = CS$<>8__locals1.<>4__this.Map;
				Faction faction2;
				if (map == null)
				{
					faction2 = null;
				}
				else
				{
					Location currentLocation = map.CurrentLocation;
					faction2 = ((currentLocation != null) ? currentLocation.Faction : null);
				}
				return f == faction2;
			}) : this.factions.Find((Faction f) => f.Prefab.Identifier == CS$<>8__locals1.factionIdentifier);
			float? num;
			if (faction == null)
			{
				num = null;
			}
			else
			{
				Reputation reputation = faction.Reputation;
				num = ((reputation != null) ? new float?(reputation.Value) : null);
			}
			float? num2 = num;
			return num2.GetValueOrDefault();
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00038E74 File Offset: 0x00037074
		public FactionAffiliation GetFactionAffiliation(Identifier factionIdentifier)
		{
			Faction faction = this.GetFaction(factionIdentifier);
			return Faction.GetPlayerAffiliationStatus(faction);
		}

		// Token: 0x0600061F RID: 1567
		public abstract void Save(XElement element, bool isSavingOnLoading);

		// Token: 0x06000620 RID: 1568 RVA: 0x00038E90 File Offset: 0x00037090
		protected void LoadStats(XElement element)
		{
			this.TotalPlayTime = element.GetAttributeDouble("TotalPlayTime".ToLowerInvariant(), 0.0);
			this.TotalPassedLevels = element.GetAttributeInt("TotalPassedLevels".ToLowerInvariant(), 0);
			this.DivingSuitWarningShown = element.GetAttributeBool("DivingSuitWarningShown".ToLowerInvariant(), false);
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00038EEC File Offset: 0x000370EC
		protected XElement SaveStats()
		{
			return new XElement("stats", new object[]
			{
				new XAttribute("TotalPlayTime".ToLowerInvariant(), this.TotalPlayTime),
				new XAttribute("TotalPassedLevels".ToLowerInvariant(), this.TotalPassedLevels),
				new XAttribute("DivingSuitWarningShown".ToLowerInvariant(), this.DivingSuitWarningShown)
			});
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00038F74 File Offset: 0x00037174
		public void LogState()
		{
			DebugConsole.NewMessage("********* CAMPAIGN STATUS *********", new Color?(Color.White), false);
			DebugConsole.NewMessage("   Money: " + this.Bank.Balance.ToString(), new Color?(Color.White), false);
			DebugConsole.NewMessage("   Current location: " + this.map.CurrentLocation.DisplayName, new Color?(Color.White), false);
			DebugConsole.NewMessage("   Available destinations: ", new Color?(Color.White), false);
			for (int i = 0; i < this.map.CurrentLocation.Connections.Count; i++)
			{
				Location destination = this.map.CurrentLocation.Connections[i].OtherLocation(this.map.CurrentLocation);
				if (destination == this.map.SelectedLocation)
				{
					DebugConsole.NewMessage("     " + i.ToString() + ". " + destination.DisplayName + " [SELECTED]", new Color?(Color.White), false);
				}
				else
				{
					DebugConsole.NewMessage("     " + i.ToString() + ". " + destination.DisplayName, new Color?(Color.White), false);
				}
			}
			if (this.map.CurrentLocation != null)
			{
				foreach (Mission mission in this.map.CurrentLocation.SelectedMissions)
				{
					DebugConsole.NewMessage("   Selected mission: " + mission.Name, new Color?(Color.White), false);
					DebugConsole.NewMessage("\n" + mission.Description, new Color?(Color.White), false);
				}
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00039180 File Offset: 0x00037380
		public override void Remove()
		{
			base.Remove();
			Map map = this.map;
			if (map != null)
			{
				map.Remove();
			}
			this.map = null;
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x000391A0 File Offset: 0x000373A0
		public int NumberOfSelectableMissionsAtLocation(Location location)
		{
			Map map = this.Map;
			int? num;
			if (map == null)
			{
				num = null;
			}
			else
			{
				Location currentLocation = map.CurrentLocation;
				if (currentLocation == null)
				{
					num = null;
				}
				else
				{
					IEnumerable<Mission> selectedMissions = currentLocation.SelectedMissions;
					num = ((selectedMissions != null) ? new int?(selectedMissions.Count((Mission m) => m.Locations.Contains(location) && !m.Prefab.IsSideObjective)) : null);
				}
			}
			int? num2 = num;
			return num2.GetValueOrDefault();
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00039218 File Offset: 0x00037418
		public void CheckTooManyMissions(Location currentLocation, Client sender)
		{
			IEnumerable<LocationConnection> connections = currentLocation.Connections;
			Func<LocationConnection, Location> <>9__0;
			Func<LocationConnection, Location> selector;
			if ((selector = <>9__0) == null)
			{
				selector = (<>9__0 = ((LocationConnection c) => c.OtherLocation(currentLocation)));
			}
			using (IEnumerator<Location> enumerator = connections.Select(selector).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Location location = enumerator.Current;
					if (this.NumberOfSelectableMissionsAtLocation(location) > this.Settings.TotalMaxMissionCount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Client ");
						defaultInterpolatedStringHandler.AppendFormatted(sender.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" had too many missions selected for location ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.DisplayName);
						defaultInterpolatedStringHandler.AppendLiteral("! Count was ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.NumberOfSelectableMissionsAtLocation(location));
						defaultInterpolatedStringHandler.AppendLiteral(". Deselecting extra missions.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						IEnumerable<Mission> selectedMissions = currentLocation.SelectedMissions;
						Func<Mission, bool> predicate;
						Func<Mission, bool> <>9__1;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Mission m) => m.Locations[1] == location));
						}
						foreach (Mission mission in selectedMissions.Where(predicate).Skip(this.Settings.TotalMaxMissionCount).ToList<Mission>())
						{
							currentLocation.DeselectMission(mission);
						}
					}
				}
			}
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x000393DC File Offset: 0x000375DC
		protected static void LeaveUnconnectedSubs(Submarine leavingSub)
		{
			if (leavingSub != Submarine.MainSub && !leavingSub.DockedTo.Contains(Submarine.MainSub))
			{
				Submarine.MainSub = leavingSub;
				GameMain.GameSession.Submarine = leavingSub;
				GameMain.GameSession.SubmarineInfo = leavingSub.Info;
				leavingSub.Info.FilePath = Path.Combine(SaveUtil.TempPath, leavingSub.Info.Name + ".sub");
				List<Submarine> subsToLeaveBehind = CampaignMode.GetSubsToLeaveBehind(leavingSub);
				GameMain.GameSession.OwnedSubmarines.Add(leavingSub.Info);
				using (List<Submarine>.Enumerator enumerator = subsToLeaveBehind.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Submarine sub = enumerator.Current;
						GameMain.GameSession.OwnedSubmarines.RemoveAll((SubmarineInfo s) => s != leavingSub.Info && s.Name == sub.Info.Name);
						MapEntity.MapEntityList.RemoveAll((MapEntity e) => e.Submarine == sub && e is LinkedSubmarine);
						LinkedSubmarine.CreateDummy(leavingSub, sub);
					}
				}
			}
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x00039540 File Offset: 0x00037740
		public void SwitchSubs()
		{
			if (this.TransferItemsOnSubSwitch)
			{
				this.TransferItemsBetweenSubs();
			}
			this.RefreshOwnedSubmarines();
			this.SwitchedSubsThisRound = true;
			this.PendingSubmarineSwitch = null;
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00039564 File Offset: 0x00037764
		protected void TransferItemsBetweenSubs()
		{
			Submarine currentSub = GameMain.GameSession.Submarine;
			if (currentSub == null || currentSub.Removed)
			{
				DebugConsole.ThrowError("Cannot transfer items between subs, because the current sub is null or removed!", null, null, false, false);
				return;
			}
			List<ValueTuple<Item, Item>> itemsToTransfer = new List<ValueTuple<Item, Item>>();
			if (this.PendingSubmarineSwitch != null)
			{
				HashSet<Submarine> connectedSubs2 = (from s in currentSub.GetConnectedSubs()
				where s.Info.Type == SubmarineType.Player
				select s).ToHashSet<Submarine>();
				foreach (Item item in Item.ItemList)
				{
					if (!item.Removed && !item.NonInteractable && !item.NonPlayerTeamInteractable && !item.IsHidden && connectedSubs2.Contains(item.Submarine) && !item.Prefab.DontTransferBetweenSubs && !CampaignMode.<TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|144_4(item))
					{
						Entity rootOwner = item.GetRootInventoryOwner();
						if (!(rootOwner is Character))
						{
							Item ownerItem = rootOwner as Item;
							if ((ownerItem == null || (!ownerItem.NonInteractable && !item.NonPlayerTeamInteractable && !ownerItem.IsHidden)) && item.GetComponent<Door>() == null)
							{
								if (!item.Components.None((ItemComponent c) => c is Pickable))
								{
									if (!item.Components.Any(delegate(ItemComponent c)
									{
										Pickable p = c as Pickable;
										return p != null && p.IsAttached;
									}))
									{
										if (!item.Components.Any(delegate(ItemComponent c)
										{
											Wire w = c as Wire;
											if (w != null)
											{
												return w.Connections.Any((Connection c) => c != null);
											}
											return false;
										}))
										{
											itemsToTransfer.Add(new ValueTuple<Item, Item>(item, item.Container));
											item.Submarine = null;
										}
									}
								}
							}
						}
					}
				}
				foreach (ValueTuple<Item, Item> valueTuple in itemsToTransfer)
				{
					Item item2 = valueTuple.Item1;
					Item container = valueTuple.Item2;
					if (((container != null) ? container.Submarine : null) != null)
					{
						item2.Drop(null, false, false);
						item2.Submarine = null;
						foreach (ItemContainer itemContainer in item2.GetComponents<ItemContainer>())
						{
							itemContainer.Inventory.FindAllItems((Item _) => true, true, null).ForEach(delegate(Item it)
							{
								it.Submarine = null;
							});
						}
					}
				}
				currentSub.Info.NoItems = true;
			}
			GameMain.GameSession.SubmarineInfo = new SubmarineInfo(currentSub);
			if (this.PendingSubmarineSwitch != null && itemsToTransfer.Any<ValueTuple<Item, Item>>())
			{
				Submarine newSub = new Submarine(this.PendingSubmarineSwitch, true, null, null);
				IEnumerable<Submarine> connectedSubs = from s in newSub.GetConnectedSubs()
				where s.Info.Type == SubmarineType.Player
				select s;
				WayPoint wp2 = WayPoint.WayPointList.FirstOrDefault((WayPoint wp) => wp.SpawnType == SpawnType.Cargo && connectedSubs.Contains(wp.Submarine));
				Hull spawnHull = ((wp2 != null) ? wp2.CurrentHull : null) ?? Hull.HullList.FirstOrDefault((Hull h) => connectedSubs.Contains(h.Submarine) && !h.IsWetRoom);
				if (spawnHull == null)
				{
					DebugConsole.AddWarning("Failed to transfer items between subs. No cargo waypoint or dry hulls found in the new sub.", null);
					return;
				}
				HashSet<ValueTuple<Item, Item>> cargoContainers = (from it in itemsToTransfer
				where it.Item1.HasTag(Tags.Crate)
				select it).ToHashSet<ValueTuple<Item, Item>>();
				foreach (ValueTuple<Item, Item> valueTuple2 in cargoContainers)
				{
					Item item3 = valueTuple2.Item1;
					Vector2 simPos = ConvertUnits.ToSimUnits(CargoManager.GetCargoPos(spawnHull, item3.Prefab));
					item3.SetTransform(simPos, 0f, false, false, null);
					item3.CurrentHull = spawnHull;
					item3.Submarine = spawnHull.Submarine;
				}
				List<ItemContainer> availableContainers = CargoManager.FindReusableCargoContainers(connectedSubs, null).ToList<ItemContainer>();
				foreach (ValueTuple<Item, Item> valueTuple3 in itemsToTransfer)
				{
					Item item4 = valueTuple3.Item1;
					Item oldContainer = valueTuple3.Item2;
					if (!cargoContainers.Contains(new ValueTuple<Item, Item>(item4, oldContainer)))
					{
						Item newContainer = null;
						item4.Submarine = newSub;
						if (item4.Container == null)
						{
							newContainer = newSub.FindContainerFor(item4, true, true, true);
						}
						string text;
						if (newContainer != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(newContainer.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted(newContainer.Tags);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							text = defaultInterpolatedStringHandler.ToStringAndClear();
						}
						else
						{
							text = "(null)";
						}
						string newContainerName = text;
						if (item4.Container == null && (newContainer == null || !newContainer.OwnInventory.TryPutItem(item4, null, null, false, false, true)))
						{
							ItemContainer cargoContainer = CargoManager.GetOrCreateCargoContainerFor(item4.Prefab, spawnHull, ref availableContainers);
							if (cargoContainer == null || !cargoContainer.Inventory.TryPutItem(item4, null, null, false, false, true))
							{
								Vector2 simPos2 = ConvertUnits.ToSimUnits(CargoManager.GetCargoPos(spawnHull, item4.Prefab));
								item4.SetTransform(simPos2, 0f, false, false, null);
							}
							else
							{
								Submarine containerSub = cargoContainer.Item.Submarine;
								if (containerSub != null)
								{
									item4.Submarine = containerSub;
								}
								newContainerName = cargoContainer.Item.Prefab.Identifier.ToString();
							}
						}
						string msg;
						if (oldContainer != null)
						{
							if (newContainer == null && oldContainer == item4.Container)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(36, 4);
								defaultInterpolatedStringHandler2.AppendLiteral("Transferred ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(item4.Prefab.Identifier);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(item4.ID);
								defaultInterpolatedStringHandler2.AppendLiteral(") contained inside ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(oldContainer.Prefab.Identifier);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<ushort>(oldContainer.ID);
								defaultInterpolatedStringHandler2.AppendLiteral(")");
								msg = defaultInterpolatedStringHandler2.ToStringAndClear();
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 5);
								defaultInterpolatedStringHandler3.AppendLiteral("Transferred ");
								defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(item4.Prefab.Identifier);
								defaultInterpolatedStringHandler3.AppendLiteral(" (");
								defaultInterpolatedStringHandler3.AppendFormatted<ushort>(item4.ID);
								defaultInterpolatedStringHandler3.AppendLiteral(") from ");
								defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(oldContainer.Prefab.Identifier);
								defaultInterpolatedStringHandler3.AppendLiteral(" (");
								defaultInterpolatedStringHandler3.AppendFormatted(oldContainer.Tags);
								defaultInterpolatedStringHandler3.AppendLiteral(") to ");
								defaultInterpolatedStringHandler3.AppendFormatted(newContainerName);
								msg = defaultInterpolatedStringHandler3.ToStringAndClear();
							}
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(19, 3);
							defaultInterpolatedStringHandler4.AppendLiteral("Transferred ");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(item4.Prefab.Identifier);
							defaultInterpolatedStringHandler4.AppendLiteral(" (");
							defaultInterpolatedStringHandler4.AppendFormatted<ushort>(item4.ID);
							defaultInterpolatedStringHandler4.AppendLiteral(") to ");
							defaultInterpolatedStringHandler4.AppendFormatted(newContainerName);
							msg = defaultInterpolatedStringHandler4.ToStringAndClear();
						}
						DebugConsole.Log(msg);
					}
				}
				foreach (ValueTuple<Item, Item> valueTuple4 in itemsToTransfer)
				{
					Item item5 = valueTuple4.Item1;
					CampaignMode.<TransferItemsBetweenSubs>g__PropagateSubmarineProperty|144_14(item5);
				}
				newSub.Info.NoItems = false;
				this.PendingSubmarineSwitch = new SubmarineInfo(newSub);
			}
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x00039DE0 File Offset: 0x00037FE0
		protected void RefreshOwnedSubmarines()
		{
			if (this.PendingSubmarineSwitch != null)
			{
				SubmarineInfo previousSub = GameMain.GameSession.SubmarineInfo;
				GameMain.GameSession.SubmarineInfo = this.PendingSubmarineSwitch;
				for (int i = 0; i < GameMain.GameSession.OwnedSubmarines.Count; i++)
				{
					if (GameMain.GameSession.OwnedSubmarines[i].Name == previousSub.Name)
					{
						GameMain.GameSession.OwnedSubmarines[i] = previousSub;
						return;
					}
				}
			}
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x00039E5E File Offset: 0x0003805E
		public void SavePets(XElement parentElement = null)
		{
			this.petsElement = new XElement("pets");
			PetBehavior.SavePets(this.petsElement);
			if (parentElement != null)
			{
				parentElement.Add(this.petsElement);
			}
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x00039E90 File Offset: 0x00038090
		public void LoadSaveSharedSingleAndMultiplayer(XElement element)
		{
			this.PurchasedLostShuttlesInLatestSave = element.GetAttributeBool("purchasedlostshuttles", false);
			this.PurchasedHullRepairsInLatestSave = element.GetAttributeBool("purchasedhullrepairs", false);
			this.PurchasedItemRepairsInLatestSave = element.GetAttributeBool("purchaseditemrepairs", false);
			this.CheatsEnabled = element.GetAttributeBool("cheatsenabled", false);
			if (this.CheatsEnabled)
			{
				DebugConsole.CheatsEnabled = true;
				if (!AchievementManager.CheatsEnabled)
				{
					AchievementManager.CheatsEnabled = true;
					DebugConsole.NewMessage("Cheat commands have been enabled.", new Color?(Color.Red), false);
				}
			}
			int oldMoney = element.GetAttributeInt("money", 0);
			if (oldMoney > 0)
			{
				this.Bank = new Wallet(Option<Character>.None())
				{
					Balance = oldMoney
				};
			}
			foreach (XElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 4:
						if (text == "pets")
						{
							this.petsElement = subElement;
						}
						break;
					case 5:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 's')
							{
								if (text == "stats")
								{
									this.LoadStats(subElement);
								}
							}
						}
						else if (text == "cargo")
						{
							this.CargoManager.LoadPurchasedItems(subElement);
						}
						break;
					}
					case 6:
						if (text == "wallet")
						{
							this.Bank = new Wallet(Option<Character>.None(), subElement);
						}
						break;
					default:
						switch (length)
						{
						case 12:
							if (!(text == "eventmanager"))
							{
								continue;
							}
							GameMain.GameSession.EventManager.Load(subElement);
							continue;
						case 13:
							continue;
						case 14:
						{
							char c = text[1];
							if (c != 'n')
							{
								if (c != 'p')
								{
									continue;
								}
								if (!(text == "upgrademanager"))
								{
									continue;
								}
							}
							else
							{
								if (!(text == "unlockedrecipe"))
								{
									continue;
								}
								GameMain.GameSession.UnlockRecipe(subElement.GetAttributeEnum("team", CharacterTeamType.Team1), subElement.GetAttributeIdentifier("identifier", Identifier.Empty), false);
								continue;
							}
							break;
						}
						case 15:
							if (!(text == "pendingupgrades"))
							{
								continue;
							}
							break;
						default:
							continue;
						}
						this.UpgradeManager = new UpgradeManager(this, subElement, base.IsSinglePlayer);
						break;
					}
				}
			}
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0003A12C File Offset: 0x0003832C
		public void LoadPets()
		{
			if (this.petsElement != null)
			{
				PetBehavior.LoadPets(this.petsElement);
			}
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0003A141 File Offset: 0x00038341
		public void SaveActiveOrders(XElement parentElement = null)
		{
			this.ActiveOrdersElement = new XElement("activeorders");
			CrewManager crewManager = base.CrewManager;
			if (crewManager != null)
			{
				crewManager.SaveActiveOrders(this.ActiveOrdersElement);
			}
			if (parentElement != null)
			{
				parentElement.Add(this.ActiveOrdersElement);
			}
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0003A17E File Offset: 0x0003837E
		public void LoadActiveOrders()
		{
			CrewManager crewManager = base.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.LoadActiveOrders(this.ActiveOrdersElement);
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0003A198 File Offset: 0x00038398
		[CompilerGenerated]
		internal static Submarine <GetLeavingSub>g__GetLeavingSubAtStart|112_3(IEnumerable<Character> leavingPlayers, CharacterTeamType submarineTeam)
		{
			if (Level.Loaded.StartOutpost == null)
			{
				Submarine closestSub = Submarine.FindClosest(Level.Loaded.StartExitPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub == null)
				{
					return null;
				}
				if (!closestSub.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub;
				}
				return Submarine.MainSub;
			}
			else
			{
				if (Level.Loaded.StartOutpost.DockedTo.Any<Submarine>())
				{
					foreach (Submarine dockedSub in Level.Loaded.StartOutpost.DockedTo)
					{
						if (!dockedSub.IsRespawnShuttle && dockedSub.TeamID == submarineTeam)
						{
							return dockedSub.DockedTo.Contains(Submarine.MainSub) ? Submarine.MainSub : dockedSub;
						}
					}
				}
				if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
				{
					if (!leavingPlayers.Any((Character s) => s.Submarine == Level.Loaded.StartOutpost))
					{
						return null;
					}
				}
				Submarine closestSub2 = Submarine.FindClosest(Level.Loaded.StartOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub2 == null || !closestSub2.AtStartExit)
				{
					return null;
				}
				if (!closestSub2.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub2;
				}
				return Submarine.MainSub;
			}
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0003A2F0 File Offset: 0x000384F0
		[CompilerGenerated]
		internal static Submarine <GetLeavingSub>g__GetLeavingSubAtEnd|112_4(IEnumerable<Character> leavingPlayers, CharacterTeamType submarineTeam)
		{
			if (Level.Loaded.EndOutpost != null && Level.Loaded.EndOutpost.ExitPoints.Any<WayPoint>())
			{
				Submarine closestSub = Submarine.FindClosest(Level.Loaded.EndOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
				if (closestSub == null || !closestSub.AtEndExit)
				{
					return null;
				}
				if (!closestSub.DockedTo.Contains(Submarine.MainSub))
				{
					return closestSub;
				}
				return Submarine.MainSub;
			}
			else
			{
				if (Level.Loaded.Type == LevelData.LevelType.Outpost)
				{
					return null;
				}
				if (Level.Loaded.EndOutpost == null)
				{
					Submarine closestSub2 = Submarine.FindClosest(Level.Loaded.EndExitPosition, true, true, true, new CharacterTeamType?(submarineTeam));
					if (closestSub2 == null)
					{
						return null;
					}
					if (!closestSub2.DockedTo.Contains(Submarine.MainSub))
					{
						return closestSub2;
					}
					return Submarine.MainSub;
				}
				else
				{
					if (Level.Loaded.EndOutpost.DockedTo.Any<Submarine>())
					{
						foreach (Submarine dockedSub in Level.Loaded.EndOutpost.DockedTo)
						{
							if (!dockedSub.IsRespawnShuttle && dockedSub.TeamID == submarineTeam)
							{
								return dockedSub.DockedTo.Contains(Submarine.MainSub) ? Submarine.MainSub : dockedSub;
							}
						}
					}
					if (Level.Loaded.Type == LevelData.LevelType.LocationConnection)
					{
						if (!leavingPlayers.Any((Character s) => s.Submarine == Level.Loaded.EndOutpost))
						{
							return null;
						}
					}
					Submarine closestSub3 = Submarine.FindClosest(Level.Loaded.EndOutpost.WorldPosition, true, true, true, new CharacterTeamType?(submarineTeam));
					if (closestSub3 == null || !closestSub3.AtEndExit)
					{
						return null;
					}
					if (!closestSub3.DockedTo.Contains(Submarine.MainSub))
					{
						return closestSub3;
					}
					return Submarine.MainSub;
				}
			}
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0003A4C4 File Offset: 0x000386C4
		[CompilerGenerated]
		internal static bool <TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|144_4(Item item)
		{
			Inventory parentInventory = item.ParentInventory;
			Item parentOwner = ((parentInventory != null) ? parentInventory.Owner : null) as Item;
			return parentOwner != null && (CampaignMode.<TransferItemsBetweenSubs>g__HasProblematicComponent|144_6(parentOwner) || CampaignMode.<TransferItemsBetweenSubs>g__AnyParentInventoryDisableTransfer|144_4(parentOwner));
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0003A4FE File Offset: 0x000386FE
		[CompilerGenerated]
		internal static bool <TransferItemsBetweenSubs>g__HasProblematicComponent|144_6(Item it)
		{
			return it.Components.Any((ItemComponent c) => c.DontTransferInventoryBetweenSubs);
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x0003A52C File Offset: 0x0003872C
		[CompilerGenerated]
		internal static void <TransferItemsBetweenSubs>g__PropagateSubmarineProperty|144_14(Item item)
		{
			foreach (ItemContainer ownedContainer in item.GetComponents<ItemContainer>())
			{
				foreach (Item containedItem in ownedContainer.Inventory.AllItems)
				{
					containedItem.Submarine = item.Submarine;
					CampaignMode.<TransferItemsBetweenSubs>g__PropagateSubmarineProperty|144_14(containedItem);
				}
			}
		}

		// Token: 0x040002E9 RID: 745
		public const int MaxMoney = 1073741823;

		// Token: 0x040002EA RID: 746
		public const int InitialMoney = 8500;

		// Token: 0x040002EB RID: 747
		protected const float EndTransitionDuration = 5f;

		// Token: 0x040002EC RID: 748
		private const float FirstRoundEventDelay = 0f;

		// Token: 0x040002ED RID: 749
		public double TotalPlayTime;

		// Token: 0x040002EE RID: 750
		public int TotalPassedLevels;

		// Token: 0x040002EF RID: 751
		public readonly CargoManager CargoManager;

		// Token: 0x040002F0 RID: 752
		public UpgradeManager UpgradeManager;

		// Token: 0x040002F1 RID: 753
		public MedicalClinic MedicalClinic;

		// Token: 0x040002F2 RID: 754
		private List<Faction> factions;

		// Token: 0x040002F3 RID: 755
		public readonly CampaignMetadata CampaignMetadata;

		// Token: 0x040002F4 RID: 756
		protected XElement petsElement;

		// Token: 0x040002F6 RID: 758
		public CampaignSettings Settings;

		// Token: 0x040002F7 RID: 759
		private readonly List<Mission> extraMissions = new List<Mission>();

		// Token: 0x040002F8 RID: 760
		public readonly NamedEvent<WalletChangedEvent> OnMoneyChanged = new NamedEvent<WalletChangedEvent>();

		// Token: 0x040002FA RID: 762
		public bool CheatsEnabled;

		// Token: 0x040002FB RID: 763
		public const float HullRepairCostPerDamage = 0.1f;

		// Token: 0x040002FC RID: 764
		public const float ItemRepairCostPerRepairDuration = 1f;

		// Token: 0x040002FD RID: 765
		public const int ShuttleReplaceCost = 1000;

		// Token: 0x040002FE RID: 766
		public const int MaxHullRepairCost = 600;

		// Token: 0x040002FF RID: 767
		public const int MaxItemRepairCost = 2000;

		// Token: 0x04000300 RID: 768
		protected bool wasDocked;

		// Token: 0x04000301 RID: 769
		private readonly Dictionary<string, double> dialogLastSpoken = new Dictionary<string, double>();

		// Token: 0x04000302 RID: 770
		public SubmarineInfo PendingSubmarineSwitch;

		// Token: 0x04000305 RID: 773
		protected Map map;

		// Token: 0x04000306 RID: 774
		public Wallet Bank;

		// Token: 0x04000308 RID: 776
		public bool PurchasedLostShuttlesInLatestSave;

		// Token: 0x04000309 RID: 777
		public bool PurchasedHullRepairsInLatestSave;

		// Token: 0x0400030A RID: 778
		public bool PurchasedItemRepairsInLatestSave;

		// Token: 0x0400030E RID: 782
		public bool DivingSuitWarningShown;

		// Token: 0x0400030F RID: 783
		public bool ItemsRelocatedToMainSub;

		// Token: 0x04000312 RID: 786
		private readonly Dictionary<Character, float> characterOutOfBoundsTimer = new Dictionary<Character, float>();

		// Token: 0x0200062C RID: 1580
		[NetworkSerialize(16)]
		public readonly struct SaveInfo : INetSerializableStruct, IEquatable<CampaignMode.SaveInfo>
		{
			// Token: 0x06004D71 RID: 19825 RVA: 0x001DF741 File Offset: 0x001DD941
			public SaveInfo(string FilePath, Option<SerializableDateTime> SaveTime, string SubmarineName, RespawnMode RespawnMode, ImmutableArray<string> EnabledContentPackageNames)
			{
				this.FilePath = FilePath;
				this.SaveTime = SaveTime;
				this.SubmarineName = SubmarineName;
				this.RespawnMode = RespawnMode;
				this.EnabledContentPackageNames = EnabledContentPackageNames;
			}

			// Token: 0x170013DB RID: 5083
			// (get) Token: 0x06004D72 RID: 19826 RVA: 0x001DF768 File Offset: 0x001DD968
			// (set) Token: 0x06004D73 RID: 19827 RVA: 0x001DF770 File Offset: 0x001DD970
			public string FilePath { get; set; }

			// Token: 0x170013DC RID: 5084
			// (get) Token: 0x06004D74 RID: 19828 RVA: 0x001DF779 File Offset: 0x001DD979
			// (set) Token: 0x06004D75 RID: 19829 RVA: 0x001DF781 File Offset: 0x001DD981
			public Option<SerializableDateTime> SaveTime { get; set; }

			// Token: 0x170013DD RID: 5085
			// (get) Token: 0x06004D76 RID: 19830 RVA: 0x001DF78A File Offset: 0x001DD98A
			// (set) Token: 0x06004D77 RID: 19831 RVA: 0x001DF792 File Offset: 0x001DD992
			public string SubmarineName { get; set; }

			// Token: 0x170013DE RID: 5086
			// (get) Token: 0x06004D78 RID: 19832 RVA: 0x001DF79B File Offset: 0x001DD99B
			// (set) Token: 0x06004D79 RID: 19833 RVA: 0x001DF7A3 File Offset: 0x001DD9A3
			public RespawnMode RespawnMode { get; set; }

			// Token: 0x170013DF RID: 5087
			// (get) Token: 0x06004D7A RID: 19834 RVA: 0x001DF7AC File Offset: 0x001DD9AC
			// (set) Token: 0x06004D7B RID: 19835 RVA: 0x001DF7B4 File Offset: 0x001DD9B4
			public ImmutableArray<string> EnabledContentPackageNames { get; set; }

			// Token: 0x06004D7C RID: 19836 RVA: 0x001DF7C0 File Offset: 0x001DD9C0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SaveInfo");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004D7D RID: 19837 RVA: 0x001DF80C File Offset: 0x001DDA0C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("FilePath = ");
				builder.Append(this.FilePath);
				builder.Append(", SaveTime = ");
				builder.Append(this.SaveTime.ToString());
				builder.Append(", SubmarineName = ");
				builder.Append(this.SubmarineName);
				builder.Append(", RespawnMode = ");
				builder.Append(this.RespawnMode.ToString());
				builder.Append(", EnabledContentPackageNames = ");
				builder.Append(this.EnabledContentPackageNames.ToString());
				return true;
			}

			// Token: 0x06004D7E RID: 19838 RVA: 0x001DF8C1 File Offset: 0x001DDAC1
			[CompilerGenerated]
			public static bool operator !=(CampaignMode.SaveInfo left, CampaignMode.SaveInfo right)
			{
				return !(left == right);
			}

			// Token: 0x06004D7F RID: 19839 RVA: 0x001DF8CD File Offset: 0x001DDACD
			[CompilerGenerated]
			public static bool operator ==(CampaignMode.SaveInfo left, CampaignMode.SaveInfo right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004D80 RID: 19840 RVA: 0x001DF8D8 File Offset: 0x001DDAD8
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<string>.Default.GetHashCode(this.<FilePath>k__BackingField) * -1521134295 + EqualityComparer<Option<SerializableDateTime>>.Default.GetHashCode(this.<SaveTime>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<SubmarineName>k__BackingField)) * -1521134295 + EqualityComparer<RespawnMode>.Default.GetHashCode(this.<RespawnMode>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<string>>.Default.GetHashCode(this.<EnabledContentPackageNames>k__BackingField);
			}

			// Token: 0x06004D81 RID: 19841 RVA: 0x001DF951 File Offset: 0x001DDB51
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is CampaignMode.SaveInfo && this.Equals((CampaignMode.SaveInfo)obj);
			}

			// Token: 0x06004D82 RID: 19842 RVA: 0x001DF96C File Offset: 0x001DDB6C
			[CompilerGenerated]
			public bool Equals(CampaignMode.SaveInfo other)
			{
				return EqualityComparer<string>.Default.Equals(this.<FilePath>k__BackingField, other.<FilePath>k__BackingField) && EqualityComparer<Option<SerializableDateTime>>.Default.Equals(this.<SaveTime>k__BackingField, other.<SaveTime>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<SubmarineName>k__BackingField, other.<SubmarineName>k__BackingField) && EqualityComparer<RespawnMode>.Default.Equals(this.<RespawnMode>k__BackingField, other.<RespawnMode>k__BackingField) && EqualityComparer<ImmutableArray<string>>.Default.Equals(this.<EnabledContentPackageNames>k__BackingField, other.<EnabledContentPackageNames>k__BackingField);
			}

			// Token: 0x06004D83 RID: 19843 RVA: 0x001DF9F1 File Offset: 0x001DDBF1
			[CompilerGenerated]
			public void Deconstruct(out string FilePath, out Option<SerializableDateTime> SaveTime, out string SubmarineName, out RespawnMode RespawnMode, out ImmutableArray<string> EnabledContentPackageNames)
			{
				FilePath = this.FilePath;
				SaveTime = this.SaveTime;
				SubmarineName = this.SubmarineName;
				RespawnMode = this.RespawnMode;
				EnabledContentPackageNames = this.EnabledContentPackageNames;
			}
		}

		// Token: 0x0200062D RID: 1581
		public enum InteractionType
		{
			// Token: 0x040028B1 RID: 10417
			None,
			// Token: 0x040028B2 RID: 10418
			Talk,
			// Token: 0x040028B3 RID: 10419
			Examine,
			// Token: 0x040028B4 RID: 10420
			Map,
			// Token: 0x040028B5 RID: 10421
			Crew,
			// Token: 0x040028B6 RID: 10422
			Store,
			// Token: 0x040028B7 RID: 10423
			Upgrade,
			// Token: 0x040028B8 RID: 10424
			PurchaseSub,
			// Token: 0x040028B9 RID: 10425
			MedicalClinic,
			// Token: 0x040028BA RID: 10426
			Cargo
		}

		// Token: 0x0200062E RID: 1582
		public enum TransitionType
		{
			// Token: 0x040028BC RID: 10428
			None,
			// Token: 0x040028BD RID: 10429
			LeaveLocation,
			// Token: 0x040028BE RID: 10430
			ProgressToNextLocation,
			// Token: 0x040028BF RID: 10431
			ReturnToPreviousLocation,
			// Token: 0x040028C0 RID: 10432
			ReturnToPreviousEmptyLocation,
			// Token: 0x040028C1 RID: 10433
			ProgressToNextEmptyLocation,
			// Token: 0x040028C2 RID: 10434
			End
		}
	}
}
