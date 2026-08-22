using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000027 RID: 39
	internal abstract class Mission
	{
		// Token: 0x060004BB RID: 1211 RVA: 0x0002A1F4 File Offset: 0x000283F4
		public static int DistributeRewardsToCrew(IEnumerable<Character> crew, int totalReward)
		{
			int remainingRewards = totalReward;
			float sum = (float)Mission.GetRewardDistibutionSum(crew, 0);
			if (MathUtils.NearlyEqual(sum, 0f, 0.0001f))
			{
				return remainingRewards;
			}
			foreach (Character character in crew)
			{
				int rewardDistribution = character.Wallet.RewardDistribution;
				float rewardWeight = (sum > 100f) ? ((float)rewardDistribution / sum) : ((float)rewardDistribution / 100f);
				int reward = Math.Min(remainingRewards, (int)((float)totalReward * rewardWeight));
				character.Wallet.Give(reward);
				remainingRewards -= reward;
				if (remainingRewards <= 0)
				{
					break;
				}
			}
			return remainingRewards;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x0002A2A4 File Offset: 0x000284A4
		public virtual void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			msg.WriteUInt16((ushort)this.State);
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x0002A2B3 File Offset: 0x000284B3
		public virtual void ServerWrite(IWriteMessage msg)
		{
			msg.WriteUInt16((ushort)this.State);
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x060004BE RID: 1214 RVA: 0x0002A2C2 File Offset: 0x000284C2
		// (set) Token: 0x060004BF RID: 1215 RVA: 0x0002A2CC File Offset: 0x000284CC
		public virtual int State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state != value)
				{
					int previousState = this.state;
					this.state = value;
					this.TryTriggerEvents(this.state);
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.UpdateMissionState(this);
					}
					this.ShowMessage(this.State);
					Action<Mission> onMissionStateChanged = this.OnMissionStateChanged;
					if (onMissionStateChanged != null)
					{
						onMissionStateChanged(this);
					}
					this.MissionStateChanged(previousState);
				}
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x060004C0 RID: 1216 RVA: 0x0002A332 File Offset: 0x00028532
		// (set) Token: 0x060004C1 RID: 1217 RVA: 0x0002A33A File Offset: 0x0002853A
		public int TimesAttempted { get; set; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x0002A343 File Offset: 0x00028543
		protected static bool IsClient
		{
			get
			{
				return GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient;
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x0002A358 File Offset: 0x00028558
		public virtual LocalizedString Name
		{
			get
			{
				return this.Prefab.Name;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0002A365 File Offset: 0x00028565
		public virtual LocalizedString SuccessMessage
		{
			get
			{
				return this.successMessage;
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060004C5 RID: 1221 RVA: 0x0002A36D File Offset: 0x0002856D
		public virtual LocalizedString FailureMessage
		{
			get
			{
				return this.failureMessage;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0002A375 File Offset: 0x00028575
		public virtual LocalizedString Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060004C7 RID: 1223 RVA: 0x0002A37D File Offset: 0x0002857D
		public virtual bool AllowUndocking
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060004C8 RID: 1224 RVA: 0x0002A380 File Offset: 0x00028580
		public virtual int Reward
		{
			get
			{
				return this.Prefab.Reward;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060004C9 RID: 1225 RVA: 0x0002A38D File Offset: 0x0002858D
		public ImmutableList<MissionPrefab.ReputationReward> ReputationRewards
		{
			get
			{
				return this.Prefab.ReputationRewards;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x0002A39A File Offset: 0x0002859A
		// (set) Token: 0x060004CB RID: 1227 RVA: 0x0002A3A2 File Offset: 0x000285A2
		public bool Completed
		{
			get
			{
				return this.completed;
			}
			set
			{
				this.completed = value;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060004CC RID: 1228 RVA: 0x0002A3AB File Offset: 0x000285AB
		public bool Failed
		{
			get
			{
				return this.failed || this.ForceFailure;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060004CD RID: 1229 RVA: 0x0002A3BD File Offset: 0x000285BD
		public virtual bool AllowRespawning
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x0002A3C0 File Offset: 0x000285C0
		public virtual int TeamCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060004CF RID: 1231 RVA: 0x0002A3C3 File Offset: 0x000285C3
		public virtual SubmarineInfo EnemySubmarineInfo
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060004D0 RID: 1232 RVA: 0x0002A3C6 File Offset: 0x000285C6
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public virtual IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				return Enumerable.Empty<ValueTuple<LocalizedString, Vector2>>();
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x0002A3CD File Offset: 0x000285CD
		public Identifier SonarIconIdentifier
		{
			get
			{
				return this.Prefab.SonarIconIdentifier;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0002A3DA File Offset: 0x000285DA
		public int? Difficulty
		{
			get
			{
				return this.Prefab.Difficulty;
			}
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0002A3E8 File Offset: 0x000285E8
		public Mission(MissionPrefab prefab, Location[] locations, Submarine sub)
		{
			this.Prefab = prefab;
			this.description = prefab.Description.Value;
			this.successMessage = prefab.SuccessMessage.Value;
			this.failureMessage = prefab.FailureMessage.Value;
			this.Headers = prefab.Headers;
			LocalizedString[] messages = prefab.Messages.ToArray<LocalizedString>();
			this.OriginLocation = locations[0];
			this.Locations = locations;
			ContentXElement endConditionElement = prefab.ConfigElement.GetChildElement("completeCheckDataAction");
			ContentXElement contentXElement = null;
			if (endConditionElement != contentXElement)
			{
				ContentXElement element = endConditionElement;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Mission (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				this.completeCheckDataAction = new CheckDataAction(element, defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.descriptionWithoutReward = this.ReplaceVariablesInMissionMessage(this.description, sub, false);
			this.description = this.ReplaceVariablesInMissionMessage(this.description, sub, true);
			this.successMessage = this.ReplaceVariablesInMissionMessage(this.successMessage, sub, true);
			this.failureMessage = this.ReplaceVariablesInMissionMessage(this.failureMessage, sub, true);
			for (int i = 0; i < messages.Length; i++)
			{
				messages[i] = this.ReplaceVariablesInMissionMessage(messages[i], sub, true);
			}
			this.Messages = messages.ToImmutableArray<LocalizedString>();
			this.characterConfig = prefab.ConfigElement.GetChildElement("Characters");
			if (prefab.ConfigElement.GetChildElements("Characters").Count<ContentXElement>() > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(89, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in mission ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(": multiple <Characters> elements found. Only the first one will be used.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), prefab.ContentPackage);
			}
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0002A5E4 File Offset: 0x000287E4
		public LocalizedString ReplaceVariablesInMissionMessage(LocalizedString message, Submarine sub, bool replaceReward = true)
		{
			for (int locationIndex = 0; locationIndex < 2; locationIndex++)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.orange‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Locations[locationIndex].DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
				string locationName = defaultInterpolatedStringHandler.ToStringAndClear();
				message = message.Replace("[location" + (locationIndex + 1).ToString() + "]", locationName, StringComparison.Ordinal);
			}
			if (replaceReward)
			{
				string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.GetReward(sub)) + "‖end‖";
				message = message.Replace("[reward]", rewardText, StringComparison.Ordinal);
			}
			return message;
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x0002A6A9 File Offset: 0x000288A9
		protected virtual void MissionStateChanged(int previousState)
		{
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0002A6AB File Offset: 0x000288AB
		public virtual void SetLevel(LevelData level)
		{
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x0002A6AD File Offset: 0x000288AD
		public static Mission LoadRandom(Location[] locations, string seed, bool requireCorrectLocationType, IEnumerable<Identifier> missionTypes, bool isSinglePlayer = false, float? difficultyLevel = null)
		{
			return Mission.LoadRandom(locations, new MTRandom(ToolBox.StringToInt(seed)), requireCorrectLocationType, missionTypes, isSinglePlayer, difficultyLevel);
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x0002A6C8 File Offset: 0x000288C8
		public static Mission LoadRandom(Location[] locations, MTRandom rand, bool requireCorrectLocationType, IEnumerable<Identifier> missionTypes, bool isSinglePlayer = false, float? difficultyLevel = null)
		{
			List<MissionPrefab> allowedMissions = new List<MissionPrefab>();
			if (missionTypes.None(null))
			{
				return null;
			}
			allowedMissions.AddRange(from m in MissionPrefab.Prefabs
			where missionTypes.Contains(m.Type)
			select m);
			allowedMissions.RemoveAll(delegate(MissionPrefab m)
			{
				if (!isSinglePlayer)
				{
					return m.SingleplayerOnly;
				}
				return m.MultiplayerOnly;
			});
			if (requireCorrectLocationType)
			{
				allowedMissions.RemoveAll((MissionPrefab m) => !m.IsAllowed(locations[0], locations[1]));
			}
			if (difficultyLevel != null)
			{
				allowedMissions.RemoveAll((MissionPrefab m) => !m.IsAllowedDifficulty(difficultyLevel.Value));
			}
			if (allowedMissions.Count == 0)
			{
				return null;
			}
			MissionPrefab missionPrefab = ToolBox.SelectWeightedRandom<MissionPrefab>(allowedMissions, (MissionPrefab m) => (float)m.Commonness, rand);
			return missionPrefab.Instantiate(locations, Submarine.MainSub);
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x0002A7B6 File Offset: 0x000289B6
		public virtual float GetBaseReward(Submarine sub)
		{
			return (float)this.Prefab.Reward;
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0002A7C4 File Offset: 0x000289C4
		public int GetReward(Submarine sub)
		{
			float reward = this.GetBaseReward(sub);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				reward *= campaign.Settings.MissionRewardMultiplier;
			}
			return (int)Math.Round((double)reward);
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x0002A804 File Offset: 0x00028A04
		protected void InitCharacters(Submarine submarine)
		{
			this.characters.Clear();
			this.characterItems.Clear();
			ContentXElement contentXElement = null;
			if (this.characterConfig != contentXElement)
			{
				foreach (ContentXElement cxe in this.characterConfig.Elements())
				{
					XElement element = cxe;
					if (GameMain.NetworkMember != null || !element.GetAttributeBool("multiplayeronly", false))
					{
						int defaultCount = element.GetAttributeInt("count", -1);
						if (defaultCount < 0)
						{
							defaultCount = element.GetAttributeInt("amount", 1);
						}
						int min = Math.Min(element.GetAttributeInt("min", defaultCount), 255);
						int max = Math.Min(Math.Max(min, element.GetAttributeInt("max", defaultCount)), 255);
						int count = Rand.Range(min, max + 1, Rand.RandSync.Unsynced);
						if (element.Attribute("identifier") != null && element.Attribute("from") != null)
						{
							HumanPrefab humanPrefab = this.GetHumanPrefabFromElement(element);
							if (humanPrefab == null)
							{
								DebugConsole.ThrowError("Couldn't spawn a human character for a mission: human prefab \"" + element.GetAttributeString("identifier", string.Empty) + "\" not found", null, this.Prefab.ContentPackage, false, false);
							}
							else
							{
								for (int i = 0; i < count; i++)
								{
									this.LoadHuman(humanPrefab, element, submarine);
								}
							}
						}
						else
						{
							Identifier speciesName = element.GetAttributeIdentifier("character", element.GetAttributeIdentifier("identifier", Identifier.Empty));
							CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
							if (characterPrefab == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
								defaultInterpolatedStringHandler.AppendLiteral("Couldn't spawn a character for a mission: character prefab \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
								defaultInterpolatedStringHandler.AppendLiteral("\" not found");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
							}
							else
							{
								for (int j = 0; j < count; j++)
								{
									this.LoadMonster(characterPrefab, element, submarine);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0002AA24 File Offset: 0x00028C24
		private SpawnAction.SpawnLocationType GetSpawnLocationTypeFromSubmarineType(Submarine sub)
		{
			switch (sub.Info.Type)
			{
			case SubmarineType.Player:
				return SpawnAction.SpawnLocationType.MainSub;
			case SubmarineType.Outpost:
			case SubmarineType.OutpostModule:
				return SpawnAction.SpawnLocationType.Outpost;
			case SubmarineType.Wreck:
				return SpawnAction.SpawnLocationType.Wreck;
			case SubmarineType.BeaconStation:
				return SpawnAction.SpawnLocationType.BeaconStation;
			case SubmarineType.Ruin:
				return SpawnAction.SpawnLocationType.Ruin;
			}
			return SpawnAction.SpawnLocationType.Any;
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x0002AA78 File Offset: 0x00028C78
		protected virtual Character LoadHuman(HumanPrefab humanPrefab, XElement element, Submarine submarine)
		{
			Identifier[] moduleFlags = element.GetAttributeIdentifierArray("moduleflags", null, true);
			Identifier[] spawnPointTags = element.GetAttributeIdentifierArray("spawnpointtags", null, true);
			SpawnType spawnPointType = element.GetAttributeEnum("spawnpointtype", SpawnType.Human);
			SpawnAction.SpawnLocationType spawnLocationTypeFromSubmarineType = this.GetSpawnLocationTypeFromSubmarineType(submarine);
			SpawnType? spawnPointType2 = new SpawnType?(spawnPointType);
			IEnumerable<Identifier> enumerable = moduleFlags;
			IEnumerable<Identifier> moduleFlags2 = enumerable ?? humanPrefab.GetModuleFlags();
			enumerable = spawnPointTags;
			ISpatialEntity spawnPos = SpawnAction.GetSpawnPos(spawnLocationTypeFromSubmarineType, spawnPointType2, moduleFlags2, enumerable ?? humanPrefab.GetSpawnPointTags(), element.GetAttributeBool("asfaraspossible", false), false, true);
			if (spawnPos == null)
			{
				spawnPos = submarine.GetHulls(false).GetRandomUnsynced<Hull>();
			}
			CharacterTeamType teamId = element.GetAttributeEnum("teamid", CharacterTeamType.None);
			Submarine startOutpost = Level.Loaded.StartOutpost;
			CharacterTeamType originalTeam = (startOutpost != null) ? startOutpost.TeamID : teamId;
			Character spawnedCharacter = Mission.CreateHuman(humanPrefab, this.characters, this.characterItems, submarine, originalTeam, spawnPos, Rand.RandSync.ServerAndClient);
			if (teamId != originalTeam)
			{
				spawnedCharacter.SetOriginalTeamAndChangeTeam(teamId, true);
			}
			if (element.GetAttribute("color", StringComparison.OrdinalIgnoreCase) != null)
			{
				spawnedCharacter.UniqueNameColor = new Color?(element.GetAttributeColor("color", Color.Red));
			}
			SubmarineInfo outPostInfo = submarine.Info;
			if (outPostInfo != null && outPostInfo.IsOutpost)
			{
				outPostInfo.AddOutpostNPCIdentifierOrTag(spawnedCharacter, humanPrefab.Identifier);
				foreach (Identifier tag in humanPrefab.GetTags())
				{
					outPostInfo.AddOutpostNPCIdentifierOrTag(spawnedCharacter, tag);
				}
			}
			WayPoint wp = spawnPos as WayPoint;
			if (wp != null)
			{
				spawnedCharacter.GiveIdCardTags(wp, false);
			}
			this.InitCharacter(spawnedCharacter, element);
			return spawnedCharacter;
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x0002AC08 File Offset: 0x00028E08
		protected virtual Character LoadMonster(CharacterPrefab monsterPrefab, XElement element, Submarine submarine)
		{
			Identifier[] moduleFlags = element.GetAttributeIdentifierArray("moduleflags", null, true);
			Identifier[] spawnPointTags = element.GetAttributeIdentifierArray("spawnpointtags", null, true);
			ISpatialEntity spawnPos = SpawnAction.GetSpawnPos(SpawnAction.SpawnLocationType.Outpost, new SpawnType?(SpawnType.Enemy), moduleFlags, spawnPointTags, element.GetAttributeBool("asfaraspossible", false), false, true);
			if (spawnPos == null)
			{
				spawnPos = submarine.GetHulls(false).GetRandomUnsynced<Hull>();
			}
			Character spawnedCharacter = Character.Create(monsterPrefab.Identifier, spawnPos.WorldPosition, ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true);
			this.characters.Add(spawnedCharacter);
			if (spawnedCharacter.Inventory != null)
			{
				this.characterItems.Add(spawnedCharacter, spawnedCharacter.Inventory.FindAllItems(null, true, null));
			}
			EnemyAIController enemyAi = spawnedCharacter.AIController as EnemyAIController;
			if (enemyAi != null && submarine != null)
			{
				enemyAi.SetUnattackableSubmarines(submarine, true, true, true);
			}
			this.InitCharacter(spawnedCharacter, element);
			return spawnedCharacter;
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x0002ACD4 File Offset: 0x00028ED4
		protected virtual void InitCharacter(Character character, XElement element)
		{
			if (element.GetAttributeBool(Tags.IgnoredByAI.Value, false))
			{
				character.AddAbilityFlag(AbilityFlags.IgnoredByEnemyAI);
			}
			float playDeadProbability = element.GetAttributeFloat("playdeadprobability", -1f);
			if (playDeadProbability >= 0f)
			{
				character.EvaluatePlayDeadProbability(new float?(playDeadProbability));
			}
			float huskProbability = element.GetAttributeFloat("huskprobability", 0f);
			if (huskProbability > 0f && Rand.Value(Rand.RandSync.Unsynced) <= huskProbability)
			{
				character.TurnIntoHusk(null, null);
				return;
			}
			if (element.GetAttributeBool("corpse", false))
			{
				character.Kill(CauseOfDeathType.Unknown, null, false, false);
			}
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x0002AD6C File Offset: 0x00028F6C
		public void Start(Level level)
		{
			this.state = 0;
			this.delayedTriggerEvents.Clear();
			using (List<string>.Enumerator enumerator = this.Prefab.UnhideEntitySubCategories.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string categoryToShow = enumerator.Current;
					IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
					Func<MapEntity, bool> predicate;
					Func<MapEntity, bool> <>9__0;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = delegate(MapEntity me)
						{
							MapEntityPrefab prefab = me.Prefab;
							return prefab != null && prefab.HasSubCategory(categoryToShow);
						});
					}
					foreach (MapEntity entityToShow in mapEntityList.Where(predicate))
					{
						entityToShow.IsLayerHidden = false;
					}
				}
			}
			this.level = level;
			this.TryTriggerEvents(0);
			this.StartMissionSpecific(level);
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x0002AE50 File Offset: 0x00029050
		protected virtual void StartMissionSpecific(Level level)
		{
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0002AE54 File Offset: 0x00029054
		public void Update(float deltaTime)
		{
			for (int i = this.delayedTriggerEvents.Count - 1; i >= 0; i--)
			{
				this.delayedTriggerEvents[i].Delay -= deltaTime;
				if (this.delayedTriggerEvents[i].Delay <= 0f)
				{
					this.TriggerEvent(this.delayedTriggerEvents[i].TriggerEvent);
					this.delayedTriggerEvents.RemoveAt(i);
				}
			}
			this.UpdateMissionSpecific(deltaTime);
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0002AED4 File Offset: 0x000290D4
		protected virtual void UpdateMissionSpecific(float deltaTime)
		{
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x0002AED6 File Offset: 0x000290D6
		protected void ShowMessage(int missionState)
		{
			this.ShowMessageProjSpecific(missionState);
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x0002AEE0 File Offset: 0x000290E0
		private void ShowMessageProjSpecific(int missionState)
		{
			int messageIndex = missionState - 1;
			if (messageIndex >= this.Headers.Length && messageIndex >= this.Messages.Length)
			{
				return;
			}
			if (messageIndex < 0)
			{
				return;
			}
			LocalizedString header = (messageIndex < this.Headers.Length) ? this.Headers[messageIndex] : "";
			LocalizedString message = (messageIndex < this.Messages.Length) ? this.Messages[messageIndex] : "";
			if (!message.IsNullOrEmpty())
			{
				message = this.ModifyMessage(message, false);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 3);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("MissionInfo"));
			defaultInterpolatedStringHandler.AppendLiteral(": ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(header);
			defaultInterpolatedStringHandler.AppendLiteral(" - ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(message);
			GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0002AFC1 File Offset: 0x000291C1
		protected virtual LocalizedString ModifyMessage(LocalizedString message, bool color = true)
		{
			return message;
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x0002AFC4 File Offset: 0x000291C4
		private void TryTriggerEvents(int state)
		{
			foreach (MissionPrefab.TriggerEvent triggerEvent in this.Prefab.TriggerEvents)
			{
				if (triggerEvent.State == state)
				{
					this.TryTriggerEvent(triggerEvent);
				}
			}
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x0002B028 File Offset: 0x00029228
		private void TryTriggerEvent(MissionPrefab.TriggerEvent trigger)
		{
			if (trigger.CampaignOnly)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					return;
				}
			}
			if (trigger.Delay > 0f || trigger.State == 0)
			{
				if (!this.delayedTriggerEvents.Any((Mission.DelayedTriggerEvent t) => t.TriggerEvent == trigger))
				{
					this.delayedTriggerEvents.Add(new Mission.DelayedTriggerEvent(trigger, trigger.Delay));
					return;
				}
			}
			else
			{
				this.TriggerEvent(trigger);
			}
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0002B0CC File Offset: 0x000292CC
		private void TriggerEvent(MissionPrefab.TriggerEvent trigger)
		{
			if (trigger.CampaignOnly)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					return;
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			EventPrefab eventPrefab = EventPrefab.FindEventPrefab(trigger.EventIdentifier, trigger.EventTag, this.Prefab.ContentPackage);
			if (eventPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Mission ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" failed to trigger an event (identifier: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(trigger.EventIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", tag: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(trigger.EventTag);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			GameSession gameSession2 = GameMain.GameSession;
			if (((gameSession2 != null) ? gameSession2.EventManager : null) != null)
			{
				Event newEvent = eventPrefab.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
				newEvent.TriggeringMission = this;
				GameMain.GameSession.EventManager.ActivateEvent(newEvent);
			}
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0002B1EC File Offset: 0x000293EC
		public void End(CampaignMode.TransitionType transitionType)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient)
			{
				this.completed = (!this.ForceFailure && this.DetermineCompleted(transitionType) && (this.completeCheckDataAction == null || this.completeCheckDataAction.GetSuccess()));
			}
			if (this.completed)
			{
				if (this.Prefab.LocationTypeChangeOnCompleted != null)
				{
					this.ChangeLocationType(this.Prefab.LocationTypeChangeOnCompleted);
				}
				try
				{
					this.GiveReward();
				}
				catch (Exception e)
				{
					string errorMsg = "Unknown error while giving mission rewards.";
					DebugConsole.ThrowError(errorMsg, e, this.Prefab.ContentPackage, false, false);
					GameAnalyticsManager.AddErrorEventOnce("Mission.End:GiveReward", GameAnalyticsManager.ErrorSeverity.Error, errorMsg + "\n" + e.StackTrace);
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.SendChatMessage(errorMsg + "\n" + e.StackTrace, new ChatMessageType?(ChatMessageType.Error), null, null, PlayerConnectionChangeType.None, ChatMode.None);
					}
				}
			}
			int timesAttempted = this.TimesAttempted;
			this.TimesAttempted = timesAttempted + 1;
			this.EndMissionSpecific(this.completed);
			if (this.ForceFailure)
			{
				this.failed = true;
			}
		}

		// Token: 0x060004EB RID: 1259
		protected abstract bool DetermineCompleted(CampaignMode.TransitionType transitionType);

		// Token: 0x060004EC RID: 1260 RVA: 0x0002B30C File Offset: 0x0002950C
		protected virtual void EndMissionSpecific(bool completed)
		{
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0002B310 File Offset: 0x00029510
		public int GetFinalReward(Submarine sub)
		{
			int? num = this.finalReward;
			if (num == null)
			{
				return this.GetReward(sub);
			}
			return num.GetValueOrDefault();
		}

		// Token: 0x060004EE RID: 1262 RVA: 0x0002B33C File Offset: 0x0002953C
		private void CalculateFinalReward(Submarine sub)
		{
			int reward = this.GetReward(sub);
			IEnumerable<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			AbilityMissionMoneyGainMultiplier missionMoneyGainMultiplier = new AbilityMissionMoneyGainMultiplier(this, 1f);
			CharacterTalent.CheckTalentsForCrew(crewCharacters, AbilityEffectType.OnGainMissionMoney, missionMoneyGainMultiplier);
			crewCharacters.ForEach(delegate(Character c)
			{
				missionMoneyGainMultiplier.Value += c.GetStatValue(StatTypes.MissionMoneyGainMultiplier, true);
			});
			this.finalReward = new int?((int)((float)reward * missionMoneyGainMultiplier.Value));
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0002B3AC File Offset: 0x000295AC
		private float CalculateDifficultyXPMultiplier()
		{
			float selectedMissionDifficulty = MathUtils.InverseLerp(1f, 4f, (float)this.Prefab.Difficulty.GetValueOrDefault());
			return MathHelper.Lerp(1f, 1.3f, selectedMissionDifficulty);
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x0002B3F0 File Offset: 0x000295F0
		private void GiveReward()
		{
			Mission.<>c__DisplayClass94_0 CS$<>8__locals1 = new Mission.<>c__DisplayClass94_0();
			GameMode gameMode = GameMain.GameSession.GameMode;
			CS$<>8__locals1.campaign = (gameMode as CampaignMode);
			if (CS$<>8__locals1.campaign == null)
			{
				return;
			}
			float xpReward = this.GetBaseReward(Submarine.MainSub) * this.Prefab.ExperienceMultiplier * CS$<>8__locals1.campaign.Settings.ExperienceRewardMultiplier;
			float xpGain = xpReward * this.level.LevelData.Biome.ExperienceFromMissionRewards * this.CalculateDifficultyXPMultiplier();
			IEnumerable<Character> crewCharacters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			CS$<>8__locals1.experienceGainMultiplier = new AbilityMissionExperienceGainMultiplier(this, 1f, null);
			crewCharacters.ForEach(delegate(Character c)
			{
				CS$<>8__locals1.experienceGainMultiplier.Value += c.GetStatValue(StatTypes.MissionExperienceGainMultiplier, true);
			});
			this.DistributeExperienceToCrew(crewCharacters, (int)(xpGain * CS$<>8__locals1.experienceGainMultiplier.Value));
			this.CalculateFinalReward(Submarine.MainSub);
			this.finalReward = new int?(Mission.DistributeRewardsToCrew(GameSession.GetSessionCrewCharacters(CharacterType.Player), this.finalReward.Value));
			bool flag;
			if (!GameMain.IsSingleplayer)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				flag = (networkMember != null && networkMember.IsServer);
			}
			else
			{
				flag = true;
			}
			bool isSingleplayerOrServer = flag;
			if (isSingleplayerOrServer)
			{
				int? num = this.finalReward;
				int num2 = 0;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					CS$<>8__locals1.campaign.Bank.Give(this.finalReward.Value);
				}
				foreach (Character character in crewCharacters)
				{
					character.Info.MissionsCompletedSinceDeath++;
				}
				using (ImmutableList<MissionPrefab.ReputationReward>.Enumerator enumerator2 = this.ReputationRewards.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						MissionPrefab.ReputationReward reputationReward = enumerator2.Current;
						AbilityMissionReputationGainMultiplier reputationGainMultiplier = new AbilityMissionReputationGainMultiplier(this, 1f, null);
						foreach (Character c2 in crewCharacters)
						{
							c2.CheckTalents(AbilityEffectType.OnCrewGainMissionReputation, reputationGainMultiplier);
						}
						float amount = reputationReward.Amount * reputationGainMultiplier.Value;
						if (reputationReward.FactionIdentifier == "location")
						{
							Reputation reputation = this.OriginLocation.Reputation;
							if (reputation != null)
							{
								reputation.AddReputation(amount, float.MaxValue);
							}
							CS$<>8__locals1.<GiveReward>g__TryGiveReputationForOpposingFaction|1(this.OriginLocation.Faction, reputationReward.AmountForOpposingFaction);
						}
						else
						{
							Faction faction = CS$<>8__locals1.campaign.Factions.Find((Faction faction1) => faction1.Prefab.Identifier == reputationReward.FactionIdentifier);
							if (faction != null)
							{
								faction.Reputation.AddReputation(amount, float.MaxValue);
								CS$<>8__locals1.<GiveReward>g__TryGiveReputationForOpposingFaction|1(faction, reputationReward.AmountForOpposingFaction);
							}
						}
					}
				}
			}
			if (this.Prefab.DataRewards != null)
			{
				foreach (ValueTuple<Identifier, object, SetDataAction.OperationType> valueTuple in this.Prefab.DataRewards)
				{
					Identifier identifier = valueTuple.Item1;
					object value = valueTuple.Item2;
					SetDataAction.OperationType operation = valueTuple.Item3;
					SetDataAction.PerformOperation(CS$<>8__locals1.campaign.CampaignMetadata, identifier, value, operation);
				}
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0002B788 File Offset: 0x00029988
		private void DistributeExperienceToCrew(IEnumerable<Character> crew, int experienceGain)
		{
			Mission.<>c__DisplayClass95_0 CS$<>8__locals1 = new Mission.<>c__DisplayClass95_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.crew = crew;
			CS$<>8__locals1.experienceGain = experienceGain;
			CS$<>8__locals1.traitorExpSteal = new Dictionary<Character, float>();
			CS$<>8__locals1.totalExpSteal = 0f;
			foreach (TraitorManager.ActiveTraitorEvent traitorEvent in GameMain.Server.TraitorManager.ActiveEvents)
			{
				if (traitorEvent.TraitorEvent.CurrentState == TraitorEvent.State.Completed)
				{
					Client traitor2 = traitorEvent.Traitor;
					if (((traitor2 != null) ? traitor2.Character : null) != null && GameMain.Server.ConnectedClients.Contains(traitorEvent.Traitor))
					{
						float expSteal = Math.Max(traitorEvent.TraitorEvent.Prefab.StealPercentageOfExperience, 0f);
						CS$<>8__locals1.<DistributeExperienceToCrew>g__AddTraitorExpSteal|2(traitorEvent.Traitor.Character, expSteal);
						foreach (Client secondaryTraitor in traitorEvent.TraitorEvent.SecondaryTraitors)
						{
							CS$<>8__locals1.<DistributeExperienceToCrew>g__AddTraitorExpSteal|2(secondaryTraitor.Character, expSteal);
						}
					}
				}
			}
			CS$<>8__locals1.totalExpSteal = CS$<>8__locals1.traitorExpSteal.Values.Sum();
			if (CS$<>8__locals1.totalExpSteal > 100f)
			{
				foreach (Character traitor in CS$<>8__locals1.traitorExpSteal.Keys)
				{
					Dictionary<Character, float> traitorExpSteal = CS$<>8__locals1.traitorExpSteal;
					Character key = traitor;
					traitorExpSteal[key] /= CS$<>8__locals1.totalExpSteal;
				}
				CS$<>8__locals1.totalExpSteal = 100f;
			}
			if (CS$<>8__locals1.totalExpSteal > 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Traitors stole ");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)CS$<>8__locals1.totalExpSteal);
				defaultInterpolatedStringHandler.AppendLiteral("% of the total experience.");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Traitors);
			}
			CS$<>8__locals1.nonTraitorCount = GameSession.GetSessionCrewCharacters(CharacterType.Both).Count((Character c) => !CS$<>8__locals1.traitorExpSteal.ContainsKey(c));
			foreach (Client c2 in GameMain.Server.ConnectedClients)
			{
				Mission.<>c__DisplayClass95_0 CS$<>8__locals2 = CS$<>8__locals1;
				Character character = c2.Character;
				CS$<>8__locals2.<DistributeExperienceToCrew>g__GiveMissionExperience|1(((character != null) ? character.Info : null) ?? c2.CharacterInfo);
			}
			foreach (Character bot in GameSession.GetSessionCrewCharacters(CharacterType.Bot))
			{
				CS$<>8__locals1.<DistributeExperienceToCrew>g__GiveMissionExperience|1(bot.Info);
			}
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x0002BA78 File Offset: 0x00029C78
		public static int GetRewardDistibutionSum(IEnumerable<Character> crew, int rewardDistribution = 0)
		{
			return crew.Sum((Character c) => c.Wallet.RewardDistribution) + rewardDistribution;
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0002BAA4 File Offset: 0x00029CA4
		[return: TupleElementNames(new string[]
		{
			"Amount",
			"Percentage",
			"Sum"
		})]
		public static ValueTuple<int, int, float> GetRewardShare(int rewardDistribution, IEnumerable<Character> crew, Option<int> reward)
		{
			float sum = (float)Mission.GetRewardDistibutionSum(crew, rewardDistribution);
			if (MathUtils.NearlyEqual(sum, 0f, 0.0001f))
			{
				return new ValueTuple<int, int, float>(0, 0, sum);
			}
			float rewardWeight = (sum > 100f) ? ((float)rewardDistribution / sum) : ((float)rewardDistribution / 100f);
			int rewardPercentage = (int)(rewardWeight * 100f);
			int a;
			int amount = reward.TryUnwrap(out a) ? a : 0;
			return new ValueTuple<int, int, float>((int)((float)amount * rewardWeight), rewardPercentage, sum);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0002BB14 File Offset: 0x00029D14
		protected void ChangeLocationType(LocationTypeChange change)
		{
			if (change == null)
			{
				throw new ArgumentException();
			}
			CampaignMode campaign = GameMain.GameSession.GameMode as CampaignMode;
			if (campaign != null && !Mission.IsClient)
			{
				int srcIndex = -1;
				for (int i = 0; i < this.Locations.Length; i++)
				{
					if (this.Locations[i].Type.Identifier == change.CurrentType)
					{
						srcIndex = i;
						break;
					}
				}
				if (srcIndex == -1)
				{
					return;
				}
				Location location = this.Locations[srcIndex];
				if (location.LocationTypeChangesBlocked)
				{
					return;
				}
				if (change.RequiredDurationRange.X > 0)
				{
					location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(change, Rand.Range(change.RequiredDurationRange.X, change.RequiredDurationRange.Y, Rand.RandSync.Unsynced), this.Prefab));
					return;
				}
				location.ChangeType(campaign, LocationType.Prefabs[change.ChangeToType], true, true);
				location.LocationTypeChangeCooldown = change.CooldownAfterChange;
			}
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0002BC00 File Offset: 0x00029E00
		public virtual void AdjustLevelData(LevelData levelData)
		{
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0002BC04 File Offset: 0x00029E04
		protected HumanPrefab GetHumanPrefabFromElement(XElement element)
		{
			if (element.Attribute("name") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - use character identifiers instead of names to configure the characters.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			Identifier characterIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			Identifier characterFrom = element.GetAttributeIdentifier("from", Identifier.Empty);
			HumanPrefab humanPrefab = NPCSet.Get(characterFrom, characterIdentifier, true, this.Prefab.ContentPackage);
			if (humanPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn character for mission: character prefab \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(characterIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\" not found in the NPC set \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(characterFrom);
				defaultInterpolatedStringHandler2.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			return humanPrefab;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x0002BD04 File Offset: 0x00029F04
		protected static Character CreateHuman(HumanPrefab humanPrefab, List<Character> characters, Dictionary<Character, List<Item>> characterItems, Submarine submarine, CharacterTeamType teamType, ISpatialEntity positionToStayIn = null, Rand.RandSync humanPrefabRandSync = Rand.RandSync.ServerAndClient)
		{
			CharacterInfo characterInfo = humanPrefab.CreateCharacterInfo(Rand.RandSync.ServerAndClient);
			characterInfo.TeamID = teamType;
			if (positionToStayIn == null)
			{
				SpawnType spawnType = SpawnType.Human;
				Job job = characterInfo.Job;
				positionToStayIn = (WayPoint.GetRandom(spawnType, (job != null) ? job.Prefab : null, submarine, false, null, false) ?? WayPoint.GetRandom(SpawnType.Human, null, submarine, false, null, false));
			}
			Character spawnedCharacter = Character.Create(characterInfo.SpeciesName, positionToStayIn.WorldPosition, ToolBox.RandomSeed(8), characterInfo, 0, false, true, false, null, true, true);
			spawnedCharacter.HumanPrefab = humanPrefab;
			humanPrefab.InitializeCharacter(spawnedCharacter, positionToStayIn);
			humanPrefab.GiveItems(spawnedCharacter, submarine, positionToStayIn as WayPoint, Rand.RandSync.ServerAndClient, false);
			characters.Add(spawnedCharacter);
			characterItems.Add(spawnedCharacter, spawnedCharacter.Inventory.FindAllItems(null, true, null));
			return spawnedCharacter;
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x0002BDB4 File Offset: 0x00029FB4
		protected ItemPrefab FindItemPrefab(XElement element)
		{
			ItemPrefab itemPrefab;
			if (element.Attribute("name") != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" - use item identifiers instead of names to configure the items");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				string itemName = element.GetAttributeString("name", "");
				itemPrefab = (MapEntityPrefab.Find(itemName, null, true) as ItemPrefab);
				if (itemPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn item for mission \"");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("\": item prefab \"");
					defaultInterpolatedStringHandler2.AppendFormatted(itemName);
					defaultInterpolatedStringHandler2.AppendLiteral("\" not found");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			else
			{
				string itemIdentifier = element.GetAttributeString("identifier", "");
				itemPrefab = (MapEntityPrefab.Find(null, itemIdentifier, true) as ItemPrefab);
				if (itemPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Couldn't spawn item for mission \"");
					defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(this.Name);
					defaultInterpolatedStringHandler3.AppendLiteral("\": item prefab \"");
					defaultInterpolatedStringHandler3.AppendFormatted(itemIdentifier);
					defaultInterpolatedStringHandler3.AppendLiteral("\" not found");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
			return itemPrefab;
		}

		// Token: 0x060004F9 RID: 1273 RVA: 0x0002BF2C File Offset: 0x0002A12C
		protected Vector2? GetCargoSpawnPosition(ItemPrefab itemPrefab, out Submarine cargoRoomSub)
		{
			cargoRoomSub = null;
			WayPoint cargoSpawnPos = WayPoint.GetRandom(SpawnType.Cargo, null, Submarine.MainSub, true, null, false);
			if (cargoSpawnPos == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Couldn't spawn items for mission \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\": no waypoints marked as Cargo were found");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			Hull cargoRoom = cargoSpawnPos.CurrentHull;
			if (cargoRoom == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(91, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Couldn't spawn items for mission \"");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(this.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\": waypoints marked as Cargo must be placed inside a room");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return null;
			}
			cargoRoomSub = cargoRoom.Submarine;
			return new Vector2?(new Vector2(cargoSpawnPos.Position.X + Rand.Range(-20f, 20f, Rand.RandSync.ServerAndClient), (float)(cargoRoom.Rect.Y - cargoRoom.Rect.Height) + itemPrefab.Size.Y / 2f));
		}

		// Token: 0x060004FA RID: 1274 RVA: 0x0002C058 File Offset: 0x0002A258
		protected static SubmarineInfo GetRandomSubmarineByTagsAndDifficulty(IEnumerable<Identifier> tags, LevelData levelData, Func<SubmarineInfo, bool> submarineSelector, string submarineTypeName)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(levelData.Seed));
			float levelDifficulty = levelData.Difficulty;
			List<SubmarineInfo> submarinesWithTags = SubmarineInfo.SavedSubmarines.Where(submarineSelector).Where(delegate(SubmarineInfo s)
			{
				ExtraSubmarineInfo extraInfo = s.GetExtraSubmarineInfo;
				return extraInfo != null && (tags.None(null) || tags.Any((Identifier t) => extraInfo.MissionTags.Contains(t)));
			}).ToList<SubmarineInfo>();
			List<SubmarineInfo> matchingSubmarines = submarinesWithTags.Where(delegate(SubmarineInfo s)
			{
				ExtraSubmarineInfo extraInfo = s.GetExtraSubmarineInfo;
				return extraInfo != null && levelDifficulty >= extraInfo.MinLevelDifficulty && levelDifficulty <= extraInfo.MaxLevelDifficulty;
			}).ToList<SubmarineInfo>();
			if (matchingSubmarines.Count == 0)
			{
				if (submarinesWithTags.Count > 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Found ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(submarinesWithTags.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted(submarineTypeName);
					defaultInterpolatedStringHandler.AppendLiteral("(s) with matching tags \"");
					defaultInterpolatedStringHandler.AppendFormatted(string.Join<Identifier>(", ", tags));
					defaultInterpolatedStringHandler.AppendLiteral("\", but none are suitable for level difficulty ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(levelDifficulty, "F1");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return null;
			}
			return matchingSubmarines[rand.Next(matchingSubmarines.Count)];
		}

		// Token: 0x04000264 RID: 612
		public readonly MissionPrefab Prefab;

		// Token: 0x04000265 RID: 613
		private bool completed;

		// Token: 0x04000266 RID: 614
		protected bool failed;

		// Token: 0x04000267 RID: 615
		protected Level level;

		// Token: 0x04000268 RID: 616
		protected int state;

		// Token: 0x0400026A RID: 618
		protected readonly CheckDataAction completeCheckDataAction;

		// Token: 0x0400026B RID: 619
		public readonly ImmutableArray<LocalizedString> Headers;

		// Token: 0x0400026C RID: 620
		public readonly ImmutableArray<LocalizedString> Messages;

		// Token: 0x0400026D RID: 621
		private int? finalReward;

		// Token: 0x0400026E RID: 622
		private readonly LocalizedString successMessage;

		// Token: 0x0400026F RID: 623
		private readonly LocalizedString failureMessage;

		// Token: 0x04000270 RID: 624
		protected LocalizedString description;

		// Token: 0x04000271 RID: 625
		protected LocalizedString descriptionWithoutReward;

		// Token: 0x04000272 RID: 626
		public bool ForceFailure;

		// Token: 0x04000273 RID: 627
		public Location OriginLocation;

		// Token: 0x04000274 RID: 628
		public readonly Location[] Locations;

		// Token: 0x04000275 RID: 629
		private readonly List<Mission.DelayedTriggerEvent> delayedTriggerEvents = new List<Mission.DelayedTriggerEvent>();

		// Token: 0x04000276 RID: 630
		public Action<Mission> OnMissionStateChanged;

		// Token: 0x04000277 RID: 631
		protected readonly ContentXElement characterConfig;

		// Token: 0x04000278 RID: 632
		protected readonly List<Character> characters = new List<Character>();

		// Token: 0x04000279 RID: 633
		protected readonly Dictionary<Character, List<Item>> characterItems = new Dictionary<Character, List<Item>>();

		// Token: 0x020005E2 RID: 1506
		private class DelayedTriggerEvent
		{
			// Token: 0x06004C7F RID: 19583 RVA: 0x001DD46F File Offset: 0x001DB66F
			public DelayedTriggerEvent(MissionPrefab.TriggerEvent triggerEvent, float delay)
			{
				this.TriggerEvent = triggerEvent;
				this.Delay = delay;
			}

			// Token: 0x040027DE RID: 10206
			public readonly MissionPrefab.TriggerEvent TriggerEvent;

			// Token: 0x040027DF RID: 10207
			public float Delay;
		}
	}
}
