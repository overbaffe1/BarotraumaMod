using System;
using System.Collections.Generic;
using System.Linq;
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
	// Token: 0x0200046D RID: 1133
	internal class RespawnManager : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x17001357 RID: 4951
		// (get) Token: 0x06004C22 RID: 19490 RVA: 0x0029F560 File Offset: 0x0029D760
		// (set) Token: 0x06004C23 RID: 19491 RVA: 0x0029F568 File Offset: 0x0029D768
		public int PendingRespawnCount { get; private set; }

		// Token: 0x17001358 RID: 4952
		// (get) Token: 0x06004C24 RID: 19492 RVA: 0x0029F571 File Offset: 0x0029D771
		// (set) Token: 0x06004C25 RID: 19493 RVA: 0x0029F579 File Offset: 0x0029D779
		public int RequiredRespawnCount { get; private set; }

		// Token: 0x17001359 RID: 4953
		// (get) Token: 0x06004C26 RID: 19494 RVA: 0x0029F582 File Offset: 0x0029D782
		// (set) Token: 0x06004C27 RID: 19495 RVA: 0x0029F58A File Offset: 0x0029D78A
		public bool ForceSpawnInMainSub { get; private set; }

		// Token: 0x1700135A RID: 4954
		// (get) Token: 0x06004C28 RID: 19496 RVA: 0x0029F593 File Offset: 0x0029D793
		// (set) Token: 0x06004C29 RID: 19497 RVA: 0x0029F59B File Offset: 0x0029D79B
		public DateTime ReturnTime { get; private set; }

		// Token: 0x1700135B RID: 4955
		// (get) Token: 0x06004C2A RID: 19498 RVA: 0x0029F5A4 File Offset: 0x0029D7A4
		// (set) Token: 0x06004C2B RID: 19499 RVA: 0x0029F5AC File Offset: 0x0029D7AC
		public DateTime RespawnTime { get; private set; }

		// Token: 0x1700135C RID: 4956
		// (get) Token: 0x06004C2C RID: 19500 RVA: 0x0029F5B5 File Offset: 0x0029D7B5
		// (set) Token: 0x06004C2D RID: 19501 RVA: 0x0029F5BD File Offset: 0x0029D7BD
		public RespawnManager.State CurrentState { get; private set; }

		// Token: 0x1700135D RID: 4957
		// (get) Token: 0x06004C2E RID: 19502 RVA: 0x0029F5C6 File Offset: 0x0029D7C6
		// (set) Token: 0x06004C2F RID: 19503 RVA: 0x0029F5CE File Offset: 0x0029D7CE
		public bool ReturnCountdownStarted { get; private set; }

		// Token: 0x1700135E RID: 4958
		// (get) Token: 0x06004C30 RID: 19504 RVA: 0x0029F5D7 File Offset: 0x0029D7D7
		// (set) Token: 0x06004C31 RID: 19505 RVA: 0x0029F5DF File Offset: 0x0029D7DF
		public bool RespawnCountdownStarted { get; private set; }

		// Token: 0x06004C32 RID: 19506 RVA: 0x0029F5E8 File Offset: 0x0029D7E8
		public static void ShowDeathPromptIfNeeded(float delay = 1f)
		{
			if (RespawnManager.UseDeathPrompt)
			{
				DeathPrompt.Create(delay);
			}
		}

		// Token: 0x06004C33 RID: 19507 RVA: 0x0029F5F8 File Offset: 0x0029D7F8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			CharacterTeamType myTeamId = (CharacterTeamType)msg.ReadByte();
			foreach (RespawnManager.TeamSpecificState teamSpecificState in this.teamSpecificStates.Values)
			{
				CharacterTeamType teamId = (CharacterTeamType)msg.ReadByte();
				bool respawnPromptPending = false;
				bool clientHasChosenNewBotViaShuttle = false;
				RespawnManager.State newState = (RespawnManager.State)msg.ReadRangedInteger(0, Enum.GetNames(typeof(RespawnManager.State)).Length);
				switch (newState)
				{
				case RespawnManager.State.Waiting:
				{
					teamSpecificState.PendingRespawnCount = (int)msg.ReadUInt16();
					teamSpecificState.RequiredRespawnCount = (int)msg.ReadUInt16();
					respawnPromptPending = msg.ReadBoolean();
					clientHasChosenNewBotViaShuttle = msg.ReadBoolean();
					teamSpecificState.RespawnCountdownStarted = msg.ReadBoolean();
					this.ResetShuttle(teamSpecificState);
					float newRespawnTime = msg.ReadSingle();
					teamSpecificState.RespawnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(newRespawnTime * 1000f));
					this.SetShuttleBodyType(teamSpecificState.TeamID, BodyType.Static);
					break;
				}
				case RespawnManager.State.Transporting:
				{
					teamSpecificState.ReturnCountdownStarted = msg.ReadBoolean();
					this.maxTransportTime = msg.ReadSingle();
					float transportTimeLeft = msg.ReadSingle();
					teamSpecificState.ReturnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(transportTimeLeft * 1000f));
					teamSpecificState.RespawnCountdownStarted = false;
					this.SetShuttleBodyType(teamSpecificState.TeamID, BodyType.Dynamic);
					break;
				}
				case RespawnManager.State.Returning:
					teamSpecificState.RespawnCountdownStarted = false;
					break;
				}
				teamSpecificState.CurrentState = newState;
				if (respawnPromptPending && !clientHasChosenNewBotViaShuttle)
				{
					GameMain.Client.HasSpawned = true;
					DeathPrompt.Create(1f);
				}
				if (teamId == myTeamId)
				{
					this.PendingRespawnCount = teamSpecificState.PendingRespawnCount;
					this.RequiredRespawnCount = teamSpecificState.RequiredRespawnCount;
					this.ReturnTime = teamSpecificState.ReturnTime;
					this.RespawnTime = teamSpecificState.RespawnTime;
					this.CurrentState = teamSpecificState.CurrentState;
					this.ReturnCountdownStarted = teamSpecificState.ReturnCountdownStarted;
					this.RespawnCountdownStarted = teamSpecificState.RespawnCountdownStarted;
				}
			}
			msg.ReadPadBits();
		}

		// Token: 0x1700135F RID: 4959
		// (get) Token: 0x06004C34 RID: 19508 RVA: 0x0029F7F8 File Offset: 0x0029D9F8
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

		// Token: 0x17001360 RID: 4960
		// (get) Token: 0x06004C35 RID: 19509 RVA: 0x0029F844 File Offset: 0x0029DA44
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

		// Token: 0x17001361 RID: 4961
		// (get) Token: 0x06004C36 RID: 19510 RVA: 0x0029F890 File Offset: 0x0029DA90
		public static bool UseDeathPrompt
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				return ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && Level.Loaded != null;
			}
		}

		// Token: 0x17001362 RID: 4962
		// (get) Token: 0x06004C37 RID: 19511 RVA: 0x0029F8B4 File Offset: 0x0029DAB4
		public bool UsingShuttle
		{
			get
			{
				return this.respawnShuttles.Any<KeyValuePair<CharacterTeamType, Submarine>>();
			}
		}

		// Token: 0x06004C38 RID: 19512 RVA: 0x0029F8C4 File Offset: 0x0029DAC4
		public bool CanRespawnAgain(CharacterTeamType team)
		{
			RespawnManager.TeamSpecificState state;
			return this.teamSpecificStates.TryGetValue(team, out state) && state.CurrentState == RespawnManager.State.Transporting && this.maxTransportTime <= 0f;
		}

		// Token: 0x17001363 RID: 4963
		// (get) Token: 0x06004C39 RID: 19513 RVA: 0x0029F8FE File Offset: 0x0029DAFE
		public IEnumerable<Submarine> RespawnShuttles
		{
			get
			{
				return this.respawnShuttles.Values;
			}
		}

		// Token: 0x06004C3A RID: 19514 RVA: 0x0029F90C File Offset: 0x0029DB0C
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
		}

		// Token: 0x06004C3B RID: 19515 RVA: 0x0029FD68 File Offset: 0x0029DF68
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

		// Token: 0x06004C3C RID: 19516 RVA: 0x0029FDDC File Offset: 0x0029DFDC
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
				case RespawnManager.State.Transporting:
					this.UpdateTransporting(teamSpecificState, deltaTime);
					break;
				case RespawnManager.State.Returning:
					this.UpdateReturning(teamSpecificState, deltaTime);
					break;
				}
			}
		}

		// Token: 0x06004C3D RID: 19517 RVA: 0x0029FE78 File Offset: 0x0029E078
		private void UpdateTransporting(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			if (this.maxTransportTime <= 0f)
			{
				return;
			}
			this.UpdateTransportingProjSpecific(teamSpecificState, deltaTime);
		}

		// Token: 0x06004C3E RID: 19518 RVA: 0x0029FE90 File Offset: 0x0029E090
		private void UpdateTransportingProjSpecific(RespawnManager.TeamSpecificState teamSpecificState, float deltaTime)
		{
			GameClient client = GameMain.Client;
			if (((client != null) ? client.Character : null) != null)
			{
				Submarine submarine = GameMain.Client.Character.Submarine;
				if (submarine != null && submarine.IsRespawnShuttle && GameMain.Client.Character.TeamID == teamSpecificState.TeamID)
				{
					if (!teamSpecificState.ReturnCountdownStarted)
					{
						return;
					}
					if ((teamSpecificState.ReturnTime - DateTime.Now).TotalSeconds < 20.0 && (DateTime.Now - this.lastShuttleLeavingWarningTime).TotalSeconds > 30.0)
					{
						this.lastShuttleLeavingWarningTime = DateTime.Now;
						GameMain.Client.AddChatMessage("ServerMessage.ShuttleLeaving", ChatMessageType.Server, "", null, null, PlayerConnectionChangeType.None, null);
					}
					return;
				}
			}
		}

		// Token: 0x06004C3F RID: 19519 RVA: 0x0029FF60 File Offset: 0x0029E160
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

		// Token: 0x06004C40 RID: 19520 RVA: 0x0029FFDC File Offset: 0x0029E1DC
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
			}
		}

		// Token: 0x06004C41 RID: 19521 RVA: 0x002A0044 File Offset: 0x0029E244
		public Submarine GetShuttle(CharacterTeamType team)
		{
			Submarine sub;
			if (this.respawnShuttles.TryGetValue(team, out sub))
			{
				return sub;
			}
			return null;
		}

		// Token: 0x06004C42 RID: 19522 RVA: 0x002A0064 File Offset: 0x0029E264
		public RespawnManager.TeamSpecificState GetTeamSpecificState(CharacterTeamType team)
		{
			RespawnManager.TeamSpecificState state;
			if (this.teamSpecificStates.TryGetValue(team, out state))
			{
				return state;
			}
			return null;
		}

		// Token: 0x06004C43 RID: 19523 RVA: 0x002A0084 File Offset: 0x0029E284
		private void SetShuttleBodyType(CharacterTeamType team, BodyType bodyType)
		{
			Submarine shuttle = this.GetShuttle(team);
			if (shuttle != null)
			{
				shuttle.PhysicsBody.BodyType = bodyType;
			}
		}

		// Token: 0x06004C44 RID: 19524 RVA: 0x002A00A8 File Offset: 0x0029E2A8
		private void ResetShuttle(RespawnManager.TeamSpecificState teamSpecificState)
		{
			teamSpecificState.ReturnTime = DateTime.Now + new TimeSpan(0, 0, 0, 0, (int)(this.maxTransportTime * 1000f));
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
							foreach (ItemComponent itemComponent in item.Components)
							{
								itemComponent.StopLoopingSound();
							}
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
						if (Character.Controlled == c)
						{
							Character.Controlled = null;
						}
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

		// Token: 0x06004C45 RID: 19525 RVA: 0x002A0554 File Offset: 0x0029E754
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

		// Token: 0x06004C46 RID: 19526 RVA: 0x002A05F6 File Offset: 0x0029E7F6
		public void RespawnCharacters(Vector2? shuttlePos)
		{
		}

		// Token: 0x06004C47 RID: 19527 RVA: 0x002A05F8 File Offset: 0x0029E7F8
		public static AfflictionPrefab GetRespawnPenaltyAfflictionPrefab()
		{
			return AfflictionPrefab.Prefabs.First((AfflictionPrefab a) => a.AfflictionType == "respawnpenalty");
		}

		// Token: 0x06004C48 RID: 19528 RVA: 0x002A0623 File Offset: 0x0029E823
		public static Affliction GetRespawnPenaltyAffliction()
		{
			AfflictionPrefab respawnPenaltyAfflictionPrefab = RespawnManager.GetRespawnPenaltyAfflictionPrefab();
			if (respawnPenaltyAfflictionPrefab == null)
			{
				return null;
			}
			return respawnPenaltyAfflictionPrefab.Instantiate(10f, null);
		}

		// Token: 0x06004C49 RID: 19529 RVA: 0x002A063C File Offset: 0x0029E83C
		public static void GiveRespawnPenaltyAffliction(Character character)
		{
			Affliction respawnPenaltyAffliction = RespawnManager.GetRespawnPenaltyAffliction();
			if (respawnPenaltyAffliction != null)
			{
				character.CharacterHealth.ApplyAffliction(null, respawnPenaltyAffliction, true, false, true);
			}
		}

		// Token: 0x06004C4A RID: 19530 RVA: 0x002A0664 File Offset: 0x0029E864
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

		// Token: 0x06004C4B RID: 19531 RVA: 0x002A0844 File Offset: 0x0029EA44
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

		// Token: 0x040027C9 RID: 10185
		private DateTime lastShuttleLeavingWarningTime;

		// Token: 0x040027D2 RID: 10194
		private readonly NetworkMember networkMember;

		// Token: 0x040027D3 RID: 10195
		private readonly Dictionary<CharacterTeamType, List<Steering>> shuttleSteering = new Dictionary<CharacterTeamType, List<Steering>>();

		// Token: 0x040027D4 RID: 10196
		private readonly Dictionary<CharacterTeamType, List<Door>> shuttleDoors = new Dictionary<CharacterTeamType, List<Door>>();

		// Token: 0x040027D5 RID: 10197
		private readonly Dictionary<CharacterTeamType, List<ItemContainer>> respawnContainers = new Dictionary<CharacterTeamType, List<ItemContainer>>();

		// Token: 0x040027D6 RID: 10198
		private readonly Dictionary<CharacterTeamType, RespawnManager.TeamSpecificState> teamSpecificStates = new Dictionary<CharacterTeamType, RespawnManager.TeamSpecificState>();

		// Token: 0x040027D7 RID: 10199
		private float maxTransportTime;

		// Token: 0x040027D8 RID: 10200
		private float updateReturnTimer;

		// Token: 0x040027D9 RID: 10201
		private Dictionary<CharacterTeamType, Submarine> respawnShuttles = new Dictionary<CharacterTeamType, Submarine>();

		// Token: 0x02001206 RID: 4614
		public enum State
		{
			// Token: 0x04005DE6 RID: 24038
			Waiting,
			// Token: 0x04005DE7 RID: 24039
			Transporting,
			// Token: 0x04005DE8 RID: 24040
			Returning
		}

		// Token: 0x02001207 RID: 4615
		public class TeamSpecificState
		{
			// Token: 0x060092EF RID: 37615 RVA: 0x003CA7E2 File Offset: 0x003C89E2
			public TeamSpecificState(CharacterTeamType teamID)
			{
				this.TeamID = teamID;
			}

			// Token: 0x04005DE9 RID: 24041
			public readonly CharacterTeamType TeamID;

			// Token: 0x04005DEA RID: 24042
			public RespawnManager.State State;

			// Token: 0x04005DEB RID: 24043
			public readonly List<Character> RespawnedCharacters = new List<Character>();

			// Token: 0x04005DEC RID: 24044
			public DateTime RespawnTime;

			// Token: 0x04005DED RID: 24045
			public DateTime ReturnTime;

			// Token: 0x04005DEE RID: 24046
			public DateTime DespawnTime;

			// Token: 0x04005DEF RID: 24047
			public bool RespawnCountdownStarted;

			// Token: 0x04005DF0 RID: 24048
			public bool ReturnCountdownStarted;

			// Token: 0x04005DF1 RID: 24049
			public int PendingRespawnCount;

			// Token: 0x04005DF2 RID: 24050
			public int RequiredRespawnCount;

			// Token: 0x04005DF3 RID: 24051
			public int PrevPendingRespawnCount;

			// Token: 0x04005DF4 RID: 24052
			public int PrevRequiredRespawnCount;

			// Token: 0x04005DF5 RID: 24053
			public RespawnManager.State CurrentState;

			// Token: 0x04005DF6 RID: 24054
			public readonly List<Item> RespawnItems = new List<Item>();
		}
	}
}
