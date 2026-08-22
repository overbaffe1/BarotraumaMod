using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Networking
{
	// Token: 0x02000376 RID: 886
	internal class RespawnManager : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x17000E8F RID: 3727
		// (get) Token: 0x060034D7 RID: 13527 RVA: 0x0016CB70 File Offset: 0x0016AD70
		public bool IsShuttleInsideLevel
		{
			get
			{
				return this.RespawnShuttles.Any((Submarine s) => s.WorldPosition.Y < (float)Level.Loaded.Size.Y);
			}
		}

		// Token: 0x060034D8 RID: 13528 RVA: 0x0016CB9C File Offset: 0x0016AD9C
		private IEnumerable<Client> GetClientsToRespawn(CharacterTeamType teamId)
		{
			RespawnManager.<GetClientsToRespawn>d__3 <GetClientsToRespawn>d__ = new RespawnManager.<GetClientsToRespawn>d__3(-2);
			<GetClientsToRespawn>d__.<>4__this = this;
			<GetClientsToRespawn>d__.<>3__teamId = teamId;
			return <GetClientsToRespawn>d__;
		}

		// Token: 0x060034D9 RID: 13529 RVA: 0x0016CBB4 File Offset: 0x0016ADB4
		private static bool IsRespawnDecisionPendingForClient(Client c)
		{
			if (Level.Loaded != null)
			{
				MultiPlayerCampaign campaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
				if (campaign != null)
				{
					if (!c2.InGame)
					{
						return false;
					}
					if (c2.SpectateOnly && (GameMain.Server.ServerSettings.AllowSpectating || GameMain.Server.OwnerConnection == c2.Connection))
					{
						return false;
					}
					if (c2.Character != null && !c2.Character.IsDead)
					{
						return false;
					}
					CharacterCampaignData matchingData = campaign.GetClientCharacterData(c2);
					if (matchingData != null && matchingData.HasSpawned)
					{
						if (Character.CharacterList.Any(delegate(Character c)
						{
							if (c.Info != matchingData.CharacterInfo)
							{
								return false;
							}
							if (c.IsDead)
							{
								CauseOfDeath causeOfDeath = c.CauseOfDeath;
								return causeOfDeath != null && causeOfDeath.Type == CauseOfDeathType.Disconnected;
							}
							return true;
						}))
						{
							return false;
						}
						if (c2.WaitForNextRoundRespawn == null)
						{
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x060034DA RID: 13530 RVA: 0x0016CC80 File Offset: 0x0016AE80
		private static bool ClientHasChosenNewBotViaShuttle(Client c)
		{
			MultiPlayerCampaign mpCampaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
			if (mpCampaign != null)
			{
				CharacterCampaignData matchingData = mpCampaign.GetClientCharacterData(c);
				if (matchingData != null)
				{
					return matchingData.ChosenNewBotViaShuttle;
				}
			}
			return false;
		}

		// Token: 0x060034DB RID: 13531 RVA: 0x0016CCB4 File Offset: 0x0016AEB4
		private static List<CharacterInfo> GetBotsToRespawn(CharacterTeamType teamId)
		{
			List<CharacterInfo> botInfos = (from botInfo in GameMain.GameSession.CrewManager.GetCharacterInfos(false)
			where botInfo.TeamID == teamId
			where GameMain.Server.ConnectedClients.None((Client c) => c.CharacterInfo == botInfo)
			select botInfo).ToList<CharacterInfo>();
			if (GameMain.Server.ServerSettings.BotSpawnMode == BotSpawnMode.Normal)
			{
				return (from ci in botInfos
				where ci.Character == null || ci.Character.IsDead
				select ci).ToList<CharacterInfo>();
			}
			int currPlayerCount = GameMain.Server.ConnectedClients.Count((Client c) => c.InGame && (!c.SpectateOnly || (!GameMain.Server.ServerSettings.AllowSpectating && GameMain.Server.OwnerConnection != c.Connection)));
			List<Character> existingBots = Character.CharacterList.FindAll((Character c) => c.IsBot && !c.IsDead && c.TeamID == teamId);
			int requiredBots = GameMain.Server.ServerSettings.BotCount - currPlayerCount;
			requiredBots -= existingBots.Count((Character b) => !b.IsDead);
			List<CharacterInfo> botsToRespawn = new List<CharacterInfo>();
			for (int i = 0; i < requiredBots; i++)
			{
				CharacterInfo botToRespawn = botInfos.FirstOrDefault((CharacterInfo b) => b.Character == null || b.Character.IsDead);
				if (botToRespawn == null)
				{
					botToRespawn = new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", null, 0, Rand.RandSync.Unsynced, default(Identifier))
					{
						TeamID = teamId
					};
				}
				else
				{
					botInfos.Remove(botToRespawn);
					existingBots.Remove(botToRespawn.Character);
				}
				botsToRespawn.Add(botToRespawn);
			}
			return botsToRespawn;
		}

		// Token: 0x060034DC RID: 13532 RVA: 0x0016CE74 File Offset: 0x0016B074
		private string GetRespawnShuttleText(CharacterTeamType team)
		{
			if (this.teamSpecificStates.Count == 1)
			{
				return "respawn shuttle";
			}
			if (team != CharacterTeamType.Team1)
			{
				return "respawn shuttle (team 2)";
			}
			return "respawn shuttle (team 1)";
		}

		// Token: 0x060034DD RID: 13533 RVA: 0x0016CE99 File Offset: 0x0016B099
		private string GetTeamNameText(CharacterTeamType team)
		{
			if (this.teamSpecificStates.Count == 1)
			{
				return "everyone";
			}
			if (team != CharacterTeamType.Team1)
			{
				return "team 2";
			}
			return "team 1";
		}

		// Token: 0x060034DE RID: 13534 RVA: 0x0016CEC0 File Offset: 0x0016B0C0
		private bool ShouldStartRespawnCountdown(RespawnManager.TeamSpecificState teamSpecificState)
		{
			int characterToRespawnCount = this.GetClientsToRespawn(teamSpecificState.TeamID).Count<Client>();
			return this.ShouldStartRespawnCountdown(characterToRespawnCount);
		}

		// Token: 0x060034DF RID: 13535 RVA: 0x0016CEE8 File Offset: 0x0016B0E8
		private static int GetMinCharactersToRespawn()
		{
			int respawnableClientCount = GameMain.Server.ConnectedClients.Count((Client c) => c.InGame && (!c.AFK || !GameMain.Server.ServerSettings.AllowAFK));
			return Math.Max((int)((float)respawnableClientCount * GameMain.Server.ServerSettings.MinRespawnRatio), 1);
		}

		// Token: 0x060034E0 RID: 13536 RVA: 0x0016CF3D File Offset: 0x0016B13D
		private bool ShouldStartRespawnCountdown(int characterToRespawnCount)
		{
			if (LuaCsSetup.Instance.Game.overrideRespawnSub)
			{
				characterToRespawnCount = 0;
			}
			return characterToRespawnCount >= RespawnManager.GetMinCharactersToRespawn();
		}

		// Token: 0x060034E1 RID: 13537 RVA: 0x0016CF60 File Offset: 0x0016B160
		public void DispatchShuttle(RespawnManager.TeamSpecificState teamSpecificState)
		{
			if (this.RespawnShuttles.Any<Submarine>())
			{
				this.ResetShuttle(teamSpecificState);
				if (LuaCsSetup.Instance.Game.overrideRespawnSub)
				{
					teamSpecificState.CurrentState = RespawnManager.State.Waiting;
				}
				else
				{
					teamSpecificState.CurrentState = RespawnManager.State.Transporting;
				}
				GameMain.Server.CreateEntityEvent(this, null);
				this.SetShuttleBodyType(teamSpecificState.TeamID, BodyType.Dynamic);
			}
			else
			{
				teamSpecificState.CurrentState = RespawnManager.State.Waiting;
				GameServer.Log("Respawning " + this.GetTeamNameText(teamSpecificState.TeamID) + " in the main sub.", ServerLog.MessageType.Spawning);
				GameMain.Server.CreateEntityEvent(this, null);
			}
			this.RespawnCharacters(teamSpecificState);
		}

		// Token: 0x060034E2 RID: 13538 RVA: 0x0016CFF8 File Offset: 0x0016B1F8
		private bool CheckShuttleEmpty(float deltaTime)
		{
			if (this.RespawnShuttles.All((Submarine respawnShuttle) => Character.CharacterList.None((Character c) => c.Submarine == respawnShuttle && !c.IsDead)))
			{
				this.shuttleEmptyTimer += deltaTime;
			}
			else
			{
				this.shuttleEmptyTimer = 0f;
			}
			return this.shuttleEmptyTimer > 1f;
		}

		// Token: 0x060034E3 RID: 13539 RVA: 0x0016D05C File Offset: 0x0016B25C
		private void RespawnCharacters(RespawnManager.TeamSpecificState teamSpecificState)
		{
			CharacterTeamType teamID = teamSpecificState.TeamID;
			int teamIndex = (teamID != CharacterTeamType.Team1) ? 1 : 0;
			bool anyCharacterSpawnedInShuttle = false;
			teamSpecificState.RespawnedCharacters.Clear();
			MultiPlayerCampaign campaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
			bool isPvPMode = GameMain.GameSession.GameMode is PvPMode;
			int teamCount = isPvPMode ? 2 : 1;
			List<Client> clients = this.GetClientsToRespawn(teamID).ToList<Client>();
			foreach (Client c3 in clients)
			{
				Character character = c3.Character;
				if (character != null)
				{
					character.DespawnNow(true);
				}
				c3.WaitForNextRoundRespawn = null;
				CharacterCampaignData matchingData = (campaign != null) ? campaign.GetClientCharacterData(c3) : null;
				if (matchingData != null)
				{
					c3.CharacterInfo = matchingData.CharacterInfo;
				}
				Client client = c3;
				if (client.CharacterInfo == null)
				{
					client.CharacterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, c3.Name, "", null, 0, Rand.RandSync.Unsynced, default(Identifier));
				}
				if (teamCount == 1)
				{
					c3.TeamID = teamID;
				}
				else if (isPvPMode && c3.TeamID == CharacterTeamType.None)
				{
					GameMain.Server.AssignClientToPvpTeamMidgame(c3);
				}
				c3.CharacterInfo.TeamID = c3.TeamID;
			}
			List<CharacterInfo> characterInfos = (from c in clients
			select c.CharacterInfo).ToList<CharacterInfo>();
			List<CharacterInfo> botsToSpawn = RespawnManager.GetBotsToRespawn(teamID);
			if (campaign == null)
			{
				characterInfos.AddRange(botsToSpawn);
				foreach (CharacterInfo bot in botsToSpawn)
				{
					Character character2 = bot.Character;
					if (character2 != null)
					{
						character2.DespawnNow(true);
					}
				}
			}
			GameMain.Server.AssignJobs(clients);
			foreach (Client c2 in clients)
			{
				if (((campaign != null) ? campaign.GetClientCharacterData(c2) : null) == null || c2.CharacterInfo.Job == null)
				{
					c2.CharacterInfo.Job = new Job(c2.AssignedJob.Prefab, isPvPMode, Rand.RandSync.Unsynced, c2.AssignedJob.Variant, Array.Empty<Skill>());
				}
			}
			Submarine mainSub = Submarine.MainSubs[teamIndex];
			Submarine respawnSub = null;
			Submarine respawnShuttle = this.GetShuttle(teamID);
			Vector2? shuttlePos = null;
			if (respawnShuttle != null)
			{
				respawnSub = respawnShuttle;
				shuttlePos = new Vector2?(this.FindSpawnPos(respawnShuttle, mainSub));
			}
			if (respawnSub == null)
			{
				respawnSub = (mainSub ?? Level.Loaded.StartOutpost);
			}
			ItemPrefab divingSuitPrefab = null;
			if ((shuttlePos != null && Level.Loaded.GetRealWorldDepth(shuttlePos.Value.Y) > 3500f) || (mainSub != null && Level.Loaded.GetRealWorldDepth(mainSub.WorldPosition.Y) > 3500f))
			{
				divingSuitPrefab = ItemPrefab.Prefabs.FirstOrDefault((ItemPrefab it) => it.Tags.Any((Identifier t) => t == "respawnsuitdeep"));
			}
			if (divingSuitPrefab == null)
			{
				divingSuitPrefab = (ItemPrefab.Prefabs.FirstOrDefault((ItemPrefab it) => it.Tags.Any((Identifier t) => t == "respawnsuit")) ?? ItemPrefab.Find(null, "divingsuit".ToIdentifier()));
			}
			ItemPrefab oxyPrefab = ItemPrefab.Find(null, "oxygentank".ToIdentifier());
			ItemPrefab scooterPrefab = ItemPrefab.Find(null, "underwaterscooter".ToIdentifier());
			ItemPrefab batteryPrefab = ItemPrefab.Find(null, "batterycell".ToIdentifier());
			WayPoint[] selectedSpawnPoints = (isPvPMode && Level.Loaded != null && Level.Loaded.ShouldSpawnCrewInsideOutpost()) ? WayPoint.SelectOutpostSpawnPoints(characterInfos, teamID) : WayPoint.SelectCrewSpawnPoints(characterInfos, respawnSub);
			WayPoint[] mainSubSpawnPoints = (mainSub != null) ? WayPoint.SelectCrewSpawnPoints(characterInfos, mainSub) : null;
			WayPoint cargoSp = WayPoint.WayPointList.Find((WayPoint wp) => wp.Submarine == respawnSub && wp.SpawnType == SpawnType.Cargo);
			int i = 0;
			while (i < characterInfos.Count)
			{
				CharacterInfo characterInfo = characterInfos[i];
				bool bot2 = botsToSpawn.Contains(characterInfo);
				characterInfo.ClearCurrentOrders();
				CharacterCampaignData characterCampaignData = null;
				bool forceSpawnInMainSub = false;
				if (!bot2)
				{
					if (clients[i].PendingName == characterInfo.Name)
					{
						GameServer server = GameMain.Server;
						if (server != null)
						{
							server.TryChangeClientName(clients[i], clients[i].PendingName, true);
						}
						clients[i].PendingName = null;
					}
					characterCampaignData = ((campaign != null) ? campaign.GetClientCharacterData(clients[i]) : null);
					if (characterCampaignData != null)
					{
						if (!characterCampaignData.HasSpawned)
						{
							forceSpawnInMainSub = true;
						}
						else
						{
							RespawnManager.ReduceCharacterSkillsOnDeath(characterInfo, false);
							characterInfo.RemoveSavedStatValuesOnDeath();
							characterInfo.CauseOfDeath = null;
						}
					}
				}
				if (!forceSpawnInMainSub && respawnShuttle != null)
				{
					anyCharacterSpawnedInShuttle = true;
				}
				Character character3 = Character.Create(characterInfo, (forceSpawnInMainSub ? mainSubSpawnPoints[i] : selectedSpawnPoints[i]).WorldPosition, characterInfo.Name, 0, !bot2, bot2, null, true);
				if (characterCampaignData != null)
				{
					characterCampaignData.ApplyWalletData(character3);
				}
				character3.LoadTalents();
				int salary;
				if (characterInfo.LastRewardDistribution.TryUnwrap(out salary))
				{
					character3.Wallet.SetRewardDistribution(salary);
				}
				teamSpecificState.RespawnedCharacters.Add(character3);
				if (bot2)
				{
					GameServer.Log(string.Format("Respawning bot {0} as {1}.", character3.Info.Name, characterInfo.Job.Name), ServerLog.MessageType.Spawning);
				}
				else
				{
					GameSession gameSession = GameMain.GameSession;
					MultiPlayerCampaign mpCampaign = ((gameSession != null) ? gameSession.GameMode : null) as MultiPlayerCampaign;
					if (mpCampaign != null && character3.Info != null)
					{
						character3.Info.SetExperience(Math.Max(character3.Info.ExperiencePoints, mpCampaign.GetSavedExperiencePoints(clients[i])));
						mpCampaign.ClearSavedExperiencePoints(clients[i]);
					}
					if (GameMain.Server.TraitorManager != null && clients[i].Character != null && GameMain.Server.TraitorManager.IsTraitor(clients[i].Character))
					{
						GameMain.Server.SendDirectChatMessage(TextManager.FormatServerMessage("TraitorRespawnMessage"), clients[i], ChatMessageType.ServerMessageBox);
					}
					clients[i].Character = character3;
					character3.SetOwnerClient(clients[i]);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Respawning ");
					defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(clients[i], null));
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<Endpoint>(clients[i].Connection.Endpoint);
					defaultInterpolatedStringHandler.AppendLiteral(") as ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(characterInfo.Job.Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Spawning);
				}
				if (respawnShuttle != null && anyCharacterSpawnedInShuttle)
				{
					GameServer.Log("Dispatching the " + this.GetRespawnShuttleText(teamID) + ".", ServerLog.MessageType.Spawning);
					respawnShuttle.SetPosition(shuttlePos.Value, null, true);
					respawnShuttle.Velocity = Vector2.Zero;
					respawnShuttle.NeutralizeBallast();
					respawnShuttle.EnableMaintainPosition();
					this.shuttleSteering[teamID].ForEach(delegate(Steering s)
					{
						s.TargetVelocity = Vector2.Zero;
					});
					List<Item> newRespawnItems = new List<Item>();
					Vector2 pos = (cargoSp != null) ? cargoSp.Position : character3.Position;
					if (divingSuitPrefab != null)
					{
						Item divingSuit = new Item(divingSuitPrefab, pos, respawnSub, 0, true);
						Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(divingSuit));
						newRespawnItems.Add(divingSuit);
						if (oxyPrefab != null && divingSuit.GetComponent<ItemContainer>() != null)
						{
							Item oxyTank = new Item(oxyPrefab, pos, respawnSub, 0, true);
							Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(oxyTank));
							divingSuit.Combine(oxyTank, null);
							newRespawnItems.Add(oxyTank);
						}
					}
					if (!(GameMain.GameSession.GameMode is CampaignMode) && scooterPrefab != null)
					{
						Item scooter = new Item(scooterPrefab, pos, respawnSub, 0, true);
						Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(scooter));
						newRespawnItems.Add(scooter);
						if (batteryPrefab != null)
						{
							Item battery = new Item(batteryPrefab, pos, respawnSub, 0, true);
							Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(battery));
							scooter.Combine(battery, null);
							newRespawnItems.Add(battery);
						}
					}
					foreach (Item respawnItem in newRespawnItems)
					{
						if (respawnItem.Container == null)
						{
							foreach (Item shuttleItem in respawnShuttle.GetItems(false))
							{
								if (!shuttleItem.NonInteractable && !shuttleItem.NonPlayerTeamInteractable)
								{
									ItemContainer container = shuttleItem.GetComponent<ItemContainer>();
									if (container != null && container.Inventory.TryPutItem(respawnItem, null, null, true, false, true))
									{
										break;
									}
								}
							}
						}
						teamSpecificState.RespawnItems.Add(respawnItem);
					}
					using (List<ItemContainer>.Enumerator enumerator6 = this.respawnContainers[teamID].GetEnumerator())
					{
						while (enumerator6.MoveNext())
						{
							ItemContainer respawnContainer = enumerator6.Current;
							teamSpecificState.RespawnItems.AddRange(AutoItemPlacer.RegenerateLoot(respawnShuttle, respawnContainer, 0f));
						}
						goto IL_A1F;
					}
					goto IL_96E;
				}
				goto IL_96E;
				IL_A1F:
				CharacterCampaignData characterData = (campaign != null) ? campaign.GetClientCharacterData(clients[i]) : null;
				if (characterData != null)
				{
					Level loaded = Level.Loaded;
					if ((loaded == null || loaded.Type != LevelData.LevelType.Outpost) && characterData.HasSpawned)
					{
						RespawnManager.ReduceCharacterSkillsOnDeath(characterInfos[i], true);
					}
				}
				WayPoint jobItemSpawnPoint = (mainSubSpawnPoints != null) ? mainSubSpawnPoints[i] : selectedSpawnPoints[i];
				if (characterData == null || characterData.HasSpawned)
				{
					character3.GiveJobItems(isPvPMode, jobItemSpawnPoint);
					if (campaign != null)
					{
						characterData = campaign.SetClientCharacterData(clients[i]);
						characterData.HasSpawned = true;
					}
				}
				else
				{
					if (characterData.HasItemData)
					{
						characterData.SpawnInventoryItems(character3, character3.Inventory);
					}
					else
					{
						character3.GiveJobItems(isPvPMode, jobItemSpawnPoint);
					}
					characterData.ApplyHealthData(character3, null);
					character3.GiveIdCardTags(jobItemSpawnPoint, false);
					characterData.HasSpawned = true;
				}
				character3.GiveIdCardTags(selectedSpawnPoints[i], true);
				i++;
				continue;
				IL_96E:
				if (!character3.InWater || divingSuitPrefab == null)
				{
					goto IL_A1F;
				}
				Item divingSuit2 = new Item(divingSuitPrefab, character3.Position, respawnSub, 0, true);
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(divingSuit2));
				character3.Inventory.TryPutItem(divingSuit2, null, divingSuit2.AllowedSlots, true, false, true);
				teamSpecificState.RespawnItems.Add(divingSuit2);
				if (oxyPrefab != null && divingSuit2.GetComponent<ItemContainer>() != null)
				{
					Item oxyTank2 = new Item(oxyPrefab, character3.Position, respawnSub, 0, true);
					Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(oxyTank2));
					divingSuit2.Combine(oxyTank2, null);
					teamSpecificState.RespawnItems.Add(oxyTank2);
					goto IL_A1F;
				}
				goto IL_A1F;
			}
		}

		// Token: 0x060034E4 RID: 13540 RVA: 0x0016DBD0 File Offset: 0x0016BDD0
		public static void ReduceCharacterSkillsOnDeath(CharacterInfo characterInfo, bool applyExtraSkillLoss = false)
		{
			if (((characterInfo != null) ? characterInfo.Job : null) == null)
			{
				return;
			}
			float resistanceMultiplier;
			float skillLossPercentage;
			if (applyExtraSkillLoss)
			{
				DebugConsole.Log("Calculating extra skill loss on respawn for " + characterInfo.Name + ":");
				resistanceMultiplier = characterInfo.LastResistanceMultiplierSkillLossRespawn;
				skillLossPercentage = RespawnManager.SkillLossPercentageOnImmediateRespawn;
			}
			else
			{
				DebugConsole.Log("Calculating base skill loss on death for " + characterInfo.Name + ":");
				resistanceMultiplier = characterInfo.LastResistanceMultiplierSkillLossDeath;
				skillLossPercentage = RespawnManager.SkillLossPercentageOnDeath;
			}
			skillLossPercentage *= resistanceMultiplier;
			foreach (Skill skill in characterInfo.Job.GetSkills())
			{
				skill.Level = RespawnManager.GetReducedSkill(characterInfo, skill, skillLossPercentage, null);
			}
		}

		// Token: 0x060034E5 RID: 13541 RVA: 0x0016DC9C File Offset: 0x0016BE9C
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)c.TeamID);
			foreach (RespawnManager.TeamSpecificState teamSpecificState in this.teamSpecificStates.Values)
			{
				msg.WriteByte((byte)teamSpecificState.TeamID);
				msg.WriteRangedInteger((int)teamSpecificState.CurrentState, 0, Enum.GetNames(typeof(RespawnManager.State)).Length);
				switch (teamSpecificState.CurrentState)
				{
				case RespawnManager.State.Waiting:
					msg.WriteUInt16((ushort)teamSpecificState.PendingRespawnCount);
					msg.WriteUInt16((ushort)teamSpecificState.RequiredRespawnCount);
					msg.WriteBoolean(RespawnManager.IsRespawnDecisionPendingForClient(c));
					msg.WriteBoolean(RespawnManager.ClientHasChosenNewBotViaShuttle(c));
					msg.WriteBoolean(teamSpecificState.RespawnCountdownStarted);
					msg.WriteSingle((float)(teamSpecificState.RespawnTime - DateTime.Now).TotalSeconds);
					break;
				case RespawnManager.State.Transporting:
					msg.WriteBoolean(teamSpecificState.ReturnCountdownStarted);
					msg.WriteSingle(GameMain.Server.ServerSettings.MaxTransportTime);
					msg.WriteSingle((float)(teamSpecificState.ReturnTime - DateTime.Now).TotalSeconds);
					break;
				}
			}
			msg.WritePadBits();
		}

		// Token: 0x17000E90 RID: 3728
		// (get) Token: 0x060034E6 RID: 13542 RVA: 0x0016DDFC File Offset: 0x0016BFFC
		public static float SkillLossPercentageOnDeath
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					num = ((serverSettings != null) ? new float?(serverSettings.SkillLossPercentageOnDeath) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault(20f);
			}
		}

		// Token: 0x17000E91 RID: 3729
		// (get) Token: 0x060034E7 RID: 13543 RVA: 0x0016DE48 File Offset: 0x0016C048
		public static float SkillLossPercentageOnImmediateRespawn
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					num = ((serverSettings != null) ? new float?(serverSettings.SkillLossPercentageOnImmediateRespawn) : null);
				}
				float? num2 = num;
				return num2.GetValueOrDefault(10f);
			}
		}

		// Token: 0x17000E92 RID: 3730
		// (get) Token: 0x060034E8 RID: 13544 RVA: 0x0016DE94 File Offset: 0x0016C094
		public static bool UseDeathPrompt
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				return ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && Level.Loaded != null;
			}
		}

		// Token: 0x17000E93 RID: 3731
		// (get) Token: 0x060034E9 RID: 13545 RVA: 0x0016DEB8 File Offset: 0x0016C0B8
		public bool UsingShuttle
		{
			get
			{
				return this.respawnShuttles.Any<KeyValuePair<CharacterTeamType, Submarine>>();
			}
		}

		// Token: 0x060034EA RID: 13546 RVA: 0x0016DEC8 File Offset: 0x0016C0C8
		public bool CanRespawnAgain(CharacterTeamType team)
		{
			RespawnManager.TeamSpecificState state;
			return this.teamSpecificStates.TryGetValue(team, out state) && state.CurrentState == RespawnManager.State.Transporting && this.maxTransportTime <= 0f;
		}

		// Token: 0x17000E94 RID: 3732
		// (get) Token: 0x060034EB RID: 13547 RVA: 0x0016DF02 File Offset: 0x0016C102
		public IEnumerable<Submarine> RespawnShuttles
		{
			get
			{
				return this.respawnShuttles.Values;
			}
		}

		// Token: 0x060034EC RID: 13548 RVA: 0x0016DF10 File Offset: 0x0016C110
		public RespawnManager(NetworkMember networkMember, SubmarineInfo shuttleInfo) : base(null, 65534)
		{
			this.networkMember = networkMember;
			this.teamSpecificStates = new Dictionary<CharacterTeamType, RespawnManager.TeamSpecificState>();
			GameSession gameSession = GameMain.GameSession;
			int teamCount = (((gameSession != null) ? gameSession.GameMode : null) is PvPMode) ? 2 : 1;
			if (Level.Loaded == null)
			{
				throw new InvalidOperationException("Attempted to instantiate a respawn manager before a level was loaded.");
			}
			bool shouldLoadShuttle = shuttleInfo != null && !Level.Loaded.ShouldSpawnCrewInsideOutpost();
			this.respawnShuttles.Clear();
			List<WifiComponent> wifiComponents = new List<WifiComponent>();
			for (int i = 0; i < teamCount; i++)
			{
				CharacterTeamType teamId = (i == 0) ? CharacterTeamType.Team1 : CharacterTeamType.Team2;
				this.teamSpecificStates.Add(teamId, new RespawnManager.TeamSpecificState(teamId));
				if (shouldLoadShuttle)
				{
					this.shuttleDoors.Add(teamId, new List<Door>());
					this.shuttleSteering.Add(teamId, new List<Steering>());
					this.respawnContainers.Add(teamId, new List<ItemContainer>());
					Submarine respawnShuttle = new Submarine(shuttleInfo, true, null, null);
					if (teamId == CharacterTeamType.Team2)
					{
						respawnShuttle.FlipX(null);
					}
					this.respawnShuttles.Add(teamId, respawnShuttle);
					respawnShuttle.PhysicsBody.FarseerBody.OnCollision += this.OnShuttleCollision;
					if (Submarine.MainSub != null)
					{
						respawnShuttle.SetCrushDepth(Math.Max(respawnShuttle.RealWorldCrushDepth, Submarine.MainSub.RealWorldCrushDepth * 1.2f));
					}
					foreach (Item item in Item.ItemList)
					{
						if (item.Submarine == respawnShuttle)
						{
							wifiComponents.AddRange(item.GetComponents<WifiComponent>());
						}
					}
					foreach (WifiComponent wifiComponent in wifiComponents)
					{
						wifiComponent.TeamID = CharacterTeamType.FriendlyNPC;
					}
					this.ResetShuttle(this.teamSpecificStates[teamId]);
					foreach (Item item2 in Item.ItemList)
					{
						if (item2.Submarine == respawnShuttle)
						{
							if (item2.HasTag(Tags.RespawnContainer))
							{
								GameSession gameSession2 = GameMain.GameSession;
								if (((gameSession2 != null) ? gameSession2.Missions : null) != null)
								{
									foreach (Mission mission in GameMain.GameSession.Missions)
									{
										item2.AddTag(Tags.RespawnContainer.AppendIfMissing("_" + mission.Prefab.Type.ToString()));
									}
								}
								this.respawnContainers[teamId].Add(item2.GetComponent<ItemContainer>());
							}
							Steering steering = item2.GetComponent<Steering>();
							if (steering != null)
							{
								this.shuttleSteering[teamId].Add(steering);
							}
							Door door = item2.GetComponent<Door>();
							if (door != null)
							{
								this.shuttleDoors[teamId].Add(door);
							}
							ConnectionPanel connectionPanel = item2.GetComponent<ConnectionPanel>();
							if (connectionPanel != null)
							{
								foreach (Connection connection in connectionPanel.Connections)
								{
									foreach (Wire wire in connection.Wires)
									{
										if (wire != null)
										{
											wire.Locked = true;
										}
									}
								}
							}
						}
					}
				}
			}
			GameServer server = networkMember as GameServer;
			if (server != null)
			{
				this.maxTransportTime = server.ServerSettings.MaxTransportTime;
			}
		}

		// Token: 0x060034ED RID: 13549 RVA: 0x0016E38C File Offset: 0x0016C58C
		private bool OnShuttleCollision(Fixture sender, Fixture other, Contact contact)
		{
			Submarine sub = sender.Body.UserData as Submarine;
			if (sub == null || !this.teamSpecificStates.ContainsKey(sub.TeamID))
			{
				return true;
			}
			if (this.teamSpecificStates[sub.TeamID].CurrentState == RespawnManager.State.Returning)
			{
				Body body = (other != null) ? other.Body : null;
				Level loaded = Level.Loaded;
				return body != ((loaded != null) ? loaded.TopBarrier : null);
			}
			return true;
		}

		// Token: 0x060034EE RID: 13550 RVA: 0x0016E400 File Offset: 0x0016C600
		public void Update(float deltaTime)
		{
			foreach (RespawnManager.TeamSpecificState teamSpecificState in this.teamSpecificStates.Values)
			{
				if (this.RespawnShuttles.None(null) && teamSpecificState.CurrentState != RespawnManager.State.Waiting)
				{
					teamSpecificState.CurrentState = RespawnManager.State.Waiting;
				}
				switch (teamSpecificState.CurrentState)
				{
				case RespawnManager.State.Waiting:
					this.UpdateWaiting(teamSpecificState);
					break;
				case RespawnManager.State.Transporting:
					this.UpdateTransporting(teamSpecificState, deltaTime);
					break;
				case RespawnManager.State.Returning:
					this.UpdateReturning(teamSpecificState, deltaTime);
					break;
				}
			}
		}

		// Token: 0x060034EF RID: 13551 RVA: 0x0016E4A4 File Offset: 0x0016C6A4
		private void UpdateWaiting(RespawnManager.TeamSpecificState teamSpecificState)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.RoundDuration < 60f)
			{
				return;
			}
			CharacterTeamType teamId = teamSpecificState.TeamID;
			Submarine respawnShuttle = this.GetShuttle(teamId);
			if (respawnShuttle != null && !LuaCsSetup.Instance.Game.overrideRespawnSub)
			{
				respawnShuttle.Velocity = Vector2.Zero;
			}
			teamSpecificState.PendingRespawnCount = this.GetClientsToRespawn(teamId).Count<Client>();
			GameSession gameSession2 = GameMain.GameSession;
			if (((gameSession2 != null) ? gameSession2.Campaign : null) == null)
			{
				teamSpecificState.PendingRespawnCount += RespawnManager.GetBotsToRespawn(teamId).Count;
			}
			teamSpecificState.RequiredRespawnCount = RespawnManager.GetMinCharactersToRespawn();
			if (teamSpecificState.PendingRespawnCount != teamSpecificState.PrevPendingRespawnCount || teamSpecificState.RequiredRespawnCount != teamSpecificState.PrevRequiredRespawnCount)
			{
				teamSpecificState.PrevPendingRespawnCount = teamSpecificState.PendingRespawnCount;
				teamSpecificState.PrevRequiredRespawnCount = teamSpecificState.RequiredRespawnCount;
				GameMain.Server.CreateEntityEvent(this, null);
			}
			if (teamSpecificState.RespawnCountdownStarted)
			{
				if (teamSpecificState.PendingRespawnCount == 0)
				{
					teamSpecificState.RespawnCountdownStarted = false;
					GameMain.Server.CreateEntityEvent(this, null);
				}
			}
			else
			{
				bool shouldStartCountdown = this.ShouldStartRespawnCountdown(teamSpecificState.PendingRespawnCount);
				if (shouldStartCountdown)
				{
					teamSpecificState.RespawnCountdownStarted = true;
					if (teamSpecificState.RespawnTime < DateTime.Now)
					{
						teamSpecificState.RespawnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(GameMain.Server.ServerSettings.RespawnInterval * 1000f));
					}
					GameMain.Server.CreateEntityEvent(this, null);
				}
			}
			if (teamSpecificState.RespawnCountdownStarted && DateTime.Now > teamSpecificState.RespawnTime)
			{
				this.DispatchShuttle(teamSpecificState);
				teamSpecificState.RespawnCountdownStarted = false;
			}
		}

		// Token: 0x060034F0 RID: 13552 RVA: 0x0016E62F File Offset: 0x0016C82F
		private void UpdateTransporting(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			if (this.maxTransportTime <= 0f)
			{
				return;
			}
			this.UpdateTransportingProjSpecific(teamSpecificState, deltaTime);
		}

		// Token: 0x060034F1 RID: 13553 RVA: 0x0016E648 File Offset: 0x0016C848
		private void UpdateTransportingProjSpecific(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			if (!teamSpecificState.ReturnCountdownStarted)
			{
				if (this.CheckShuttleEmpty(deltaTime))
				{
					teamSpecificState.ReturnTime = DateTime.Now;
					teamSpecificState.ReturnCountdownStarted = true;
				}
				else
				{
					if (!this.ShouldStartRespawnCountdown(teamSpecificState))
					{
						teamSpecificState.ReturnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(this.maxTransportTime * 1000f));
						teamSpecificState.DespawnTime = teamSpecificState.ReturnTime + new TimeSpan(0, 0, 30);
						return;
					}
					teamSpecificState.ReturnCountdownStarted = true;
					GameMain.Server.CreateEntityEvent(this, null);
				}
			}
			else if (this.CheckShuttleEmpty(deltaTime))
			{
				teamSpecificState.ReturnTime = DateTime.Now;
			}
			if (DateTime.Now > teamSpecificState.ReturnTime)
			{
				if (this.IsShuttleInsideLevel)
				{
					GameServer.Log("The " + this.GetRespawnShuttleText(teamSpecificState.TeamID) + " is leaving.", ServerLog.MessageType.ServerMessage);
				}
				teamSpecificState.CurrentState = RespawnManager.State.Returning;
				GameMain.Server.CreateEntityEvent(this, null);
				teamSpecificState.RespawnCountdownStarted = false;
				this.maxTransportTime = GameMain.Server.ServerSettings.MaxTransportTime;
			}
		}

		// Token: 0x060034F2 RID: 13554 RVA: 0x0016E758 File Offset: 0x0016C958
		public void ForceRespawn()
		{
			foreach (RespawnManager.TeamSpecificState teamSpecificState in this.teamSpecificStates.Values)
			{
				if (teamSpecificState.CurrentState != RespawnManager.State.Transporting)
				{
					this.ResetShuttle(teamSpecificState);
					teamSpecificState.RespawnCountdownStarted = true;
					teamSpecificState.RespawnTime = DateTime.Now;
					teamSpecificState.CurrentState = RespawnManager.State.Waiting;
				}
			}
		}

		// Token: 0x060034F3 RID: 13555 RVA: 0x0016E7D4 File Offset: 0x0016C9D4
		private void UpdateReturning(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			this.updateReturnTimer += deltaTime;
			if (this.updateReturnTimer > 1f)
			{
				this.updateReturnTimer = 0f;
				this.shuttleSteering[teamSpecificState.TeamID].ForEach(delegate(Steering steering)
				{
					steering.SetDestinationLevelStart();
				});
				this.UpdateReturningProjSpecific(teamSpecificState, deltaTime);
			}
		}

		// Token: 0x060034F4 RID: 13556 RVA: 0x0016E844 File Offset: 0x0016CA44
		private void UpdateReturningProjSpecific(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			if (teamSpecificState.DespawnTime > DateTime.Now + new TimeSpan(0, 0, 30) && this.CheckShuttleEmpty(deltaTime))
			{
				teamSpecificState.DespawnTime = DateTime.Now + new TimeSpan(0, 0, 30);
			}
			foreach (Door door in this.shuttleDoors[teamSpecificState.TeamID])
			{
				if (door.IsOpen)
				{
					door.TrySetState(false, false, true);
				}
			}
			List<Gap> shuttleGaps = Gap.GapList.FindAll((Gap g) => this.RespawnShuttles.Contains(g.Submarine) && g.ConnectedWall != null);
			shuttleGaps.ForEach(delegate(Gap g)
			{
				Entity.Spawner.AddEntityToRemoveQueue(g);
			});
			List<Item> dockingPorts = Item.ItemList.FindAll((Item i) => this.RespawnShuttles.Contains(i.Submarine) && i.GetComponent<DockingPort>() != null);
			dockingPorts.ForEach(delegate(Item d)
			{
				d.GetComponent<DockingPort>().Undock(true);
			});
			if (!this.IsShuttleInsideLevel || DateTime.Now > teamSpecificState.DespawnTime)
			{
				this.ResetShuttle(teamSpecificState);
				teamSpecificState.CurrentState = RespawnManager.State.Waiting;
				GameServer.Log("The " + this.GetRespawnShuttleText(teamSpecificState.TeamID) + " has left.", ServerLog.MessageType.Spawning);
				GameMain.Server.CreateEntityEvent(this, null);
				teamSpecificState.RespawnCountdownStarted = false;
				teamSpecificState.ReturnCountdownStarted = false;
			}
		}

		// Token: 0x060034F5 RID: 13557 RVA: 0x0016E9CC File Offset: 0x0016CBCC
		public Submarine GetShuttle(CharacterTeamType team)
		{
			Submarine sub;
			if (this.respawnShuttles.TryGetValue(team, out sub))
			{
				return sub;
			}
			return null;
		}

		// Token: 0x060034F6 RID: 13558 RVA: 0x0016E9EC File Offset: 0x0016CBEC
		public RespawnManager.TeamSpecificState GetTeamSpecificState(CharacterTeamType team)
		{
			RespawnManager.TeamSpecificState state;
			if (this.teamSpecificStates.TryGetValue(team, out state))
			{
				return state;
			}
			return null;
		}

		// Token: 0x060034F7 RID: 13559 RVA: 0x0016EA0C File Offset: 0x0016CC0C
		private void SetShuttleBodyType(CharacterTeamType team, BodyType bodyType)
		{
			Submarine shuttle = this.GetShuttle(team);
			if (shuttle != null)
			{
				shuttle.PhysicsBody.BodyType = bodyType;
			}
		}

		// Token: 0x060034F8 RID: 13560 RVA: 0x0016EA30 File Offset: 0x0016CC30
		private void ResetShuttle(RespawnManager.TeamSpecificState teamSpecificState)
		{
			teamSpecificState.ReturnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(this.maxTransportTime * 1000f));
			teamSpecificState.DespawnTime = teamSpecificState.ReturnTime + new TimeSpan(0, 0, 30);
			Submarine shuttle = this.GetShuttle(teamSpecificState.TeamID);
			if (shuttle == null)
			{
				return;
			}
			using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					if (item.Submarine == shuttle)
					{
						if (teamSpecificState.RespawnItems.Contains(item) || this.respawnContainers[teamSpecificState.TeamID].Any((ItemContainer container) => item.IsOwnedBy(container.Item)))
						{
							Entity.Spawner.AddItemToRemoveQueue(item);
						}
						else
						{
							item.Condition = item.MaxCondition;
							Repairable component = item.GetComponent<Repairable>();
							if (component != null)
							{
								component.ResetDeterioration();
							}
							PowerContainer powerContainer = item.GetComponent<PowerContainer>();
							if (powerContainer != null)
							{
								powerContainer.Charge = powerContainer.GetCapacity();
							}
							Door door = item.GetComponent<Door>();
							if (door != null)
							{
								door.Stuck = 0f;
							}
							Steering steering = item.GetComponent<Steering>();
							if (steering != null)
							{
								steering.MaintainPos = true;
								steering.AutoPilot = true;
								steering.UnsentChanges = true;
							}
						}
					}
				}
			}
			teamSpecificState.RespawnItems.Clear();
			foreach (Structure wall in Structure.WallList)
			{
				if (wall.Submarine == shuttle)
				{
					for (int i = 0; i < wall.SectionCount; i++)
					{
						wall.AddDamage(i, -100000f, null, true, false);
					}
				}
			}
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine == shuttle)
				{
					hull.OxygenPercentage = 100f;
					hull.WaterVolume = 0f;
					BallastFloraBehavior ballastFlora = hull.BallastFlora;
					if (ballastFlora != null)
					{
						ballastFlora.Remove();
					}
				}
			}
			Dictionary<Character, Vector2> characterPositions = new Dictionary<Character, Vector2>();
			foreach (Character c in Character.CharacterList)
			{
				if (c.Submarine == shuttle)
				{
					if (!teamSpecificState.RespawnedCharacters.Contains(c))
					{
						characterPositions.Add(c, c.WorldPosition);
					}
					else
					{
						c.Kill(CauseOfDeathType.Unknown, null, true, true);
						c.Enabled = false;
						Entity.Spawner.AddEntityToRemoveQueue(c);
						if (c.Inventory != null)
						{
							foreach (Item item2 in c.Inventory.AllItems)
							{
								Entity.Spawner.AddItemToRemoveQueue(item2);
							}
						}
					}
				}
			}
			shuttle.SetPosition(new Vector2((teamSpecificState.TeamID == CharacterTeamType.Team1) ? Level.Loaded.StartPosition.X : Level.Loaded.EndPosition.X, (float)(Level.Loaded.Size.Y + shuttle.Borders.Height)), null, true);
			shuttle.Velocity = Vector2.Zero;
			foreach (KeyValuePair<Character, Vector2> characterPosition in characterPositions)
			{
				characterPosition.Key.TeleportTo(characterPosition.Value);
			}
			this.SetShuttleBodyType(teamSpecificState.TeamID, BodyType.Static);
		}

		// Token: 0x060034F9 RID: 13561 RVA: 0x0016EE9C File Offset: 0x0016D09C
		public static float GetReducedSkill(CharacterInfo characterInfo, Skill skill, float skillLossPercentage, float? currentSkillLevel = null)
		{
			SkillPrefab skillPrefab = characterInfo.Job.Prefab.Skills.Find((SkillPrefab s) => skill.Identifier == s.Identifier);
			float currentLevel = currentSkillLevel ?? skill.Level;
			if (skillPrefab == null)
			{
				return currentLevel;
			}
			SkillPrefab skillPrefab2 = skillPrefab;
			GameSession gameSession = GameMain.GameSession;
			Range<float> levelRange = skillPrefab2.GetLevelRange(((gameSession != null) ? gameSession.GameMode : null) is PvPMode);
			if (currentLevel < levelRange.End)
			{
				return currentLevel;
			}
			return MathHelper.Lerp(currentLevel, levelRange.End, skillLossPercentage / 100f);
		}

		// Token: 0x060034FA RID: 13562 RVA: 0x0016EF3E File Offset: 0x0016D13E
		public void RespawnCharacters(Vector2? shuttlePos)
		{
		}

		// Token: 0x060034FB RID: 13563 RVA: 0x0016EF40 File Offset: 0x0016D140
		public static AfflictionPrefab GetRespawnPenaltyAfflictionPrefab()
		{
			return AfflictionPrefab.Prefabs.First((AfflictionPrefab a) => a.AfflictionType == "respawnpenalty");
		}

		// Token: 0x060034FC RID: 13564 RVA: 0x0016EF6B File Offset: 0x0016D16B
		public static Affliction GetRespawnPenaltyAffliction()
		{
			AfflictionPrefab respawnPenaltyAfflictionPrefab = RespawnManager.GetRespawnPenaltyAfflictionPrefab();
			if (respawnPenaltyAfflictionPrefab == null)
			{
				return null;
			}
			return respawnPenaltyAfflictionPrefab.Instantiate(10f, null);
		}

		// Token: 0x060034FD RID: 13565 RVA: 0x0016EF84 File Offset: 0x0016D184
		public static void GiveRespawnPenaltyAffliction(Character character)
		{
			Affliction respawnPenaltyAffliction = RespawnManager.GetRespawnPenaltyAffliction();
			if (respawnPenaltyAffliction != null)
			{
				character.CharacterHealth.ApplyAffliction(null, respawnPenaltyAffliction, true, false, true);
			}
		}

		// Token: 0x060034FE RID: 13566 RVA: 0x0016EFAC File Offset: 0x0016D1AC
		public Vector2 FindSpawnPos(Submarine respawnShuttle, Submarine mainSub)
		{
			if (Level.Loaded == null || Submarine.MainSub == null)
			{
				return Vector2.Zero;
			}
			Rectangle dockedBorders = respawnShuttle.GetDockedBorders(true);
			Vector2 diffFromDockedBorders = new Vector2((float)dockedBorders.Center.X, (float)(dockedBorders.Y - dockedBorders.Height / 2)) - new Vector2((float)respawnShuttle.Borders.Center.X, (float)(respawnShuttle.Borders.Y - respawnShuttle.Borders.Height / 2));
			int minWidth = Math.Max(dockedBorders.Width, 1000);
			int minHeight = Math.Max(dockedBorders.Height, 1000);
			List<Level.InterestingPosition> potentialSpawnPositions = this.FindValidSpawnPoints(respawnShuttle, (float)minWidth, (float)minHeight, 10000f, 5000f);
			if (potentialSpawnPositions.None(null))
			{
				DebugConsole.NewMessage("Failed to find a shuttle spawn position far away from submarines and characters, attempting to find one closer to to subs and characters...", null, false);
				potentialSpawnPositions = this.FindValidSpawnPoints(respawnShuttle, (float)minWidth, (float)minHeight, 1000f, 500f);
				if (potentialSpawnPositions.None(null))
				{
					DebugConsole.NewMessage("Failed to find a shuttle spawn position, using the level's start position instead.", null, false);
					return Level.Loaded.StartPosition;
				}
			}
			Vector2 bestSpawnPos = Level.Loaded.StartPosition;
			float bestSpawnPosValue = 0f;
			foreach (Level.InterestingPosition potentialSpawnPos in potentialSpawnPositions)
			{
				float num = 100000f;
				Point position = potentialSpawnPos.Position;
				float spawnPosValue = num / Math.Max(Vector2.Distance(position.ToVector2(), mainSub.WorldPosition), 1f);
				if ((float)potentialSpawnPos.Position.X > mainSub.WorldPosition.X)
				{
					spawnPosValue *= 0.1f;
				}
				if (spawnPosValue > bestSpawnPosValue)
				{
					position = potentialSpawnPos.Position;
					bestSpawnPos = position.ToVector2();
					bestSpawnPosValue = spawnPosValue;
				}
			}
			return bestSpawnPos;
		}

		// Token: 0x060034FF RID: 13567 RVA: 0x0016F18C File Offset: 0x0016D38C
		private List<Level.InterestingPosition> FindValidSpawnPoints(Submarine respawnShuttle, float minWidth, float minHeight, float minDistFromSubs, float minDistFromCharacters)
		{
			List<Level.InterestingPosition> potentialSpawnPositions = new List<Level.InterestingPosition>();
			foreach (Level.InterestingPosition potentialSpawnPos in from p in Level.Loaded.PositionsOfInterest
			where p.PositionType == Level.PositionType.MainPath
			select p)
			{
				bool invalid = false;
				foreach (Ruin ruin in Level.Loaded.Ruins)
				{
					if ((float)Math.Abs(ruin.Area.Center.X - potentialSpawnPos.Position.X) < (minWidth + (float)ruin.Area.Width) / 2f)
					{
						invalid = true;
						break;
					}
					if ((float)Math.Abs(ruin.Area.Center.Y - potentialSpawnPos.Position.Y) < (minHeight + (float)ruin.Area.Height) / 2f)
					{
						invalid = true;
						break;
					}
				}
				if (!invalid)
				{
					Level loaded = Level.Loaded;
					Point position = potentialSpawnPos.Position;
					List<VoronoiCell> tooCloseCells = loaded.GetTooCloseCells(position.ToVector2(), Math.Max(minWidth, minHeight));
					if (!tooCloseCells.Any<VoronoiCell>())
					{
						foreach (Submarine sub in Submarine.Loaded)
						{
							if (sub != respawnShuttle && !respawnShuttle.DockedTo.Contains(sub))
							{
								float minDist = Math.Max(Math.Max(minWidth, minHeight) + (float)Math.Max(sub.Borders.Width, sub.Borders.Height), minDistFromSubs);
								Vector2 worldPosition = sub.WorldPosition;
								position = potentialSpawnPos.Position;
								if (Vector2.DistanceSquared(worldPosition, position.ToVector2()) < minDist * minDist)
								{
									invalid = true;
									break;
								}
							}
						}
						if (!invalid)
						{
							foreach (Character character in Character.CharacterList)
							{
								if (character.IsDead)
								{
									if (Math.Abs(character.WorldPosition.X - (float)potentialSpawnPos.Position.X) < minWidth)
									{
										invalid = true;
										break;
									}
									if (Math.Abs(character.WorldPosition.Y - (float)potentialSpawnPos.Position.Y) < minHeight)
									{
										invalid = true;
										break;
									}
								}
								else
								{
									Vector2 worldPosition2 = character.WorldPosition;
									position = potentialSpawnPos.Position;
									if (Vector2.DistanceSquared(worldPosition2, position.ToVector2()) < minDistFromCharacters * minDistFromCharacters)
									{
										invalid = true;
										break;
									}
								}
							}
							if (!invalid)
							{
								potentialSpawnPositions.Add(potentialSpawnPos);
							}
						}
					}
				}
			}
			return potentialSpawnPositions;
		}

		// Token: 0x04001A3B RID: 6715
		private float shuttleEmptyTimer;

		// Token: 0x04001A3C RID: 6716
		private readonly NetworkMember networkMember;

		// Token: 0x04001A3D RID: 6717
		private readonly Dictionary<CharacterTeamType, List<Steering>> shuttleSteering = new Dictionary<CharacterTeamType, List<Steering>>();

		// Token: 0x04001A3E RID: 6718
		private readonly Dictionary<CharacterTeamType, List<Door>> shuttleDoors = new Dictionary<CharacterTeamType, List<Door>>();

		// Token: 0x04001A3F RID: 6719
		private readonly Dictionary<CharacterTeamType, List<ItemContainer>> respawnContainers = new Dictionary<CharacterTeamType, List<ItemContainer>>();

		// Token: 0x04001A40 RID: 6720
		private readonly Dictionary<CharacterTeamType, RespawnManager.TeamSpecificState> teamSpecificStates = new Dictionary<CharacterTeamType, RespawnManager.TeamSpecificState>();

		// Token: 0x04001A41 RID: 6721
		private float maxTransportTime;

		// Token: 0x04001A42 RID: 6722
		private float updateReturnTimer;

		// Token: 0x04001A43 RID: 6723
		private Dictionary<CharacterTeamType, Submarine> respawnShuttles = new Dictionary<CharacterTeamType, Submarine>();

		// Token: 0x02000C1D RID: 3101
		public enum State
		{
			// Token: 0x04003B6F RID: 15215
			Waiting,
			// Token: 0x04003B70 RID: 15216
			Transporting,
			// Token: 0x04003B71 RID: 15217
			Returning
		}

		// Token: 0x02000C1E RID: 3102
		public class TeamSpecificState
		{
			// Token: 0x06006315 RID: 25365 RVA: 0x00211091 File Offset: 0x0020F291
			public TeamSpecificState(CharacterTeamType teamID)
			{
				this.TeamID = teamID;
			}

			// Token: 0x04003B72 RID: 15218
			public readonly CharacterTeamType TeamID;

			// Token: 0x04003B73 RID: 15219
			public RespawnManager.State State;

			// Token: 0x04003B74 RID: 15220
			public readonly List<Character> RespawnedCharacters = new List<Character>();

			// Token: 0x04003B75 RID: 15221
			public DateTime RespawnTime;

			// Token: 0x04003B76 RID: 15222
			public DateTime ReturnTime;

			// Token: 0x04003B77 RID: 15223
			public DateTime DespawnTime;

			// Token: 0x04003B78 RID: 15224
			public bool RespawnCountdownStarted;

			// Token: 0x04003B79 RID: 15225
			public bool ReturnCountdownStarted;

			// Token: 0x04003B7A RID: 15226
			public int PendingRespawnCount;

			// Token: 0x04003B7B RID: 15227
			public int RequiredRespawnCount;

			// Token: 0x04003B7C RID: 15228
			public int PrevPendingRespawnCount;

			// Token: 0x04003B7D RID: 15229
			public int PrevRequiredRespawnCount;

			// Token: 0x04003B7E RID: 15230
			public RespawnManager.State CurrentState;

			// Token: 0x04003B7F RID: 15231
			public readonly List<Item> RespawnItems = new List<Item>();
		}
	}
}
