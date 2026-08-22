using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000022 RID: 34
	internal class CombatMission : Mission
	{
		// Token: 0x17000153 RID: 339
		// (get) Token: 0x0600045C RID: 1116 RVA: 0x0002632E File Offset: 0x0002452E
		[Nullable(1)]
		public override LocalizedString Description
		{
			[NullableContext(1)]
			get
			{
				if (this.descriptions == null)
				{
					return "";
				}
				return this.descriptions[0];
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0002634C File Offset: 0x0002454C
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			this.CheckTeamCharacters();
			if (this.state == 0)
			{
				this.CheckWinCondition(deltaTime);
				for (int i = 0; i < this.teamDead.Length; i++)
				{
					if (!this.teamDead[i] && this.teamDead[1 - i])
					{
						this.SetWinningTeam(i);
						return;
					}
				}
				return;
			}
			this.roundEndTimer -= deltaTime;
			if (this.roundEndTimer > 0f)
			{
				return;
			}
			if (!this.teamDead[0] || !this.teamDead[1])
			{
				CharacterTeamType? winningTeam = GameMain.GameSession.WinningTeam;
				CharacterTeamType characterTeamType = CharacterTeamType.None;
				if (!(winningTeam.GetValueOrDefault() == characterTeamType & winningTeam != null))
				{
					GameMain.Server.EndGame(CampaignMode.TransitionType.None, false, null);
				}
				return;
			}
			GameMain.GameSession.WinningTeam = new CharacterTeamType?(CharacterTeamType.None);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.EndGame(CampaignMode.TransitionType.None, false, null);
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00026424 File Offset: 0x00024624
		private void CheckTeamCharacters()
		{
			for (int i = 0; i < this.crews.Length; i++)
			{
				foreach (Character character in this.crews[i])
				{
					if (character.IsDead)
					{
						this.AddKill(character);
					}
				}
			}
			this.crews[0].Clear();
			this.crews[1].Clear();
			foreach (Character character2 in Character.CharacterList)
			{
				if (!character2.IsDead)
				{
					if (character2.TeamID == CharacterTeamType.Team1)
					{
						this.crews[0].Add(character2);
					}
					else if (character2.TeamID == CharacterTeamType.Team2)
					{
						this.crews[1].Add(character2);
					}
					if (character2.IsBot)
					{
						HumanAIController humanAi = character2.AIController as HumanAIController;
						if (humanAi != null)
						{
							OrderPrefab assaultOrder;
							if (!humanAi.ObjectiveManager.HasOrder<AIObjectiveFightIntruders>((AIObjectiveFightIntruders o) => o.TargetCharactersInOtherSubs) && OrderPrefab.Prefabs.TryGet(Tags.AssaultEnemyOrder, out assaultOrder))
							{
								character2.SetOrder(assaultOrder.CreateInstance(OrderPrefab.OrderTargetType.Entity, null, false).WithManualPriority(CharacterInfo.HighestManualOrderPriority), true, false, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x000265A8 File Offset: 0x000247A8
		private void CheckWinCondition(float deltaTime)
		{
			switch (this.winCondition)
			{
			case CombatMission.WinCondition.LastManStanding:
				if (this.crews[0].Count == 0 && this.crews[1].Count == 0)
				{
					this.teamDead[0] = (this.teamDead[1] = true);
					this.state = 1;
				}
				else
				{
					this.teamDead[0] = this.crews[0].All((Character c) => c.IsDead || c.IsIncapacitated);
					this.teamDead[1] = this.crews[1].All((Character c) => c.IsDead || c.IsIncapacitated);
					if (this.teamDead[0] && this.teamDead[1])
					{
						this.state = 1;
					}
				}
				break;
			case CombatMission.WinCondition.ControlSubmarine:
				this.CheckTargetSubmarineControl(deltaTime);
				break;
			}
			this.CheckScore();
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x000266A4 File Offset: 0x000248A4
		private void CheckScore()
		{
			for (int i = 0; i < this.crews.Length; i++)
			{
				if (this.Scores[i] >= this.WinScore)
				{
					this.SetWinningTeam(i);
					return;
				}
			}
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x000266DC File Offset: 0x000248DC
		private void CheckTargetSubmarineControl(float deltaTime)
		{
			if (this.targetSubmarine == null)
			{
				return;
			}
			this.timeInTargetSubmarineTimer += deltaTime;
			if (this.timeInTargetSubmarineTimer < 1f)
			{
				return;
			}
			this.timeInTargetSubmarineTimer = 0f;
			bool crew1InSubmarine = this.crews[0].Any((Character c) => c.Submarine == this.targetSubmarine);
			bool crew2InSubmarine = this.crews[1].Any((Character c) => c.Submarine == this.targetSubmarine);
			for (int i = 0; i < this.crews.Length; i++)
			{
				if (this.crews[i].Any((Character c) => c.Submarine == this.targetSubmarine) && this.crews[1 - i].None((Character c) => c.Submarine == this.targetSubmarine))
				{
					this.Scores[i]++;
					GameServer server = GameMain.Server;
					if (server != null)
					{
						server.UpdateMissionState(this);
					}
				}
			}
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x000267BC File Offset: 0x000249BC
		public void AddToScore(CharacterTeamType team, int amount)
		{
			if (!this.HasWinScore)
			{
				return;
			}
			int index;
			if (team != CharacterTeamType.Team1)
			{
				if (team != CharacterTeamType.Team2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to increase the score of an invalid team (");
					defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(team);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					DebugConsole.AddSafeError(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				index = 1;
			}
			else
			{
				index = 0;
			}
			this.Scores[index] = MathHelper.Clamp(this.Scores[index] + amount, 0, this.WinScore);
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.UpdateMissionState(this);
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x0002684C File Offset: 0x00024A4C
		[NullableContext(1)]
		private void AddKill(Character character)
		{
			List<CombatMission.KillCount> list = this.kills;
			CauseOfDeath causeOfDeath = character.CauseOfDeath;
			list.Add(new CombatMission.KillCount(character, (causeOfDeath != null) ? causeOfDeath.Killer : null));
			if (this.winCondition == CombatMission.WinCondition.KillCount)
			{
				this.Scores[(character.TeamID == CharacterTeamType.Team1) ? 1 : 0] += this.PointsPerKill;
			}
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.UpdateMissionState(this);
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x000268B3 File Offset: 0x00024AB3
		private void SetWinningTeam(int teamIndex)
		{
			this.State = teamIndex + 1;
			GameMain.GameSession.WinningTeam = new CharacterTeamType?((teamIndex == 0) ? CharacterTeamType.Team1 : CharacterTeamType.Team2);
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x000268D4 File Offset: 0x00024AD4
		[NullableContext(1)]
		public override void ServerWrite(IWriteMessage msg)
		{
			base.ServerWrite(msg);
			msg.WriteUInt16((ushort)this.Scores[0]);
			msg.WriteUInt16((ushort)this.Scores[1]);
			IEnumerable<Client> uniqueClients = (from k in this.kills
			select k.VictimClient).Union(from k in this.kills
			select k.KillerClient).NotNull<Client>();
			msg.WriteVariableUInt32((uint)uniqueClients.Count<Client>());
			using (IEnumerator<Client> enumerator = uniqueClients.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Client client = enumerator.Current;
					msg.WriteByte(client.SessionId);
					msg.WriteVariableUInt32((uint)this.kills.Count((CombatMission.KillCount k) => k.VictimClient == client));
					msg.WriteVariableUInt32((uint)this.kills.Count((CombatMission.KillCount k) => k.KillerClient == client));
				}
			}
			IEnumerable<CharacterInfo> uniqueBots = from c in (from k in this.kills
			select k.Killer).Union(from k in this.kills
			select k.Victim).NotNull<Character>()
			where c.Info != null && c.IsBot
			select c.Info;
			msg.WriteVariableUInt32((uint)uniqueBots.Count<CharacterInfo>());
			using (IEnumerator<CharacterInfo> enumerator2 = uniqueBots.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					CharacterInfo botInfo = enumerator2.Current;
					msg.WriteUInt16(botInfo.ID);
					msg.WriteVariableUInt32((uint)this.kills.Count(delegate(CombatMission.KillCount k)
					{
						Character victim = k.Victim;
						return ((victim != null) ? victim.Info : null) == botInfo;
					}));
					msg.WriteVariableUInt32((uint)this.kills.Count(delegate(CombatMission.KillCount k)
					{
						Character killer = k.Killer;
						return ((killer != null) ? killer.Info : null) == botInfo;
					}));
				}
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x00026B40 File Offset: 0x00024D40
		public override bool AllowRespawning
		{
			get
			{
				return this.allowRespawning;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000467 RID: 1127 RVA: 0x00026B48 File Offset: 0x00024D48
		// (set) Token: 0x06000468 RID: 1128 RVA: 0x00026B50 File Offset: 0x00024D50
		public TagAction.SubType TargetSubmarineType { get; set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000469 RID: 1129 RVA: 0x00026B59 File Offset: 0x00024D59
		public int WinScore
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null)
				{
					return 10;
				}
				return networkMember.ServerSettings.WinScorePvP;
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x00026B71 File Offset: 0x00024D71
		public bool HasWinScore
		{
			get
			{
				return this.winCondition != CombatMission.WinCondition.LastManStanding || this.PointsPerKill != 0;
			}
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600046B RID: 1131 RVA: 0x00026B86 File Offset: 0x00024D86
		public static CharacterTeamType Winner
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null || gameSession.WinningTeam == null)
				{
					return CharacterTeamType.None;
				}
				return GameMain.GameSession.WinningTeam.Value;
			}
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600046C RID: 1132 RVA: 0x00026BB4 File Offset: 0x00024DB4
		public override LocalizedString SuccessMessage
		{
			get
			{
				if (CombatMission.Winner == CharacterTeamType.None || base.SuccessMessage.IsNullOrEmpty())
				{
					return "";
				}
				if (!TextManager.ContainsTag("MissionSuccess." + this.Prefab.TextIdentifier.ToString()))
				{
					return "";
				}
				CharacterTeamType loser = (CombatMission.Winner == CharacterTeamType.Team1) ? CharacterTeamType.Team2 : CharacterTeamType.Team1;
				return base.SuccessMessage.Replace("[loser]", CombatMission.GetTeamName(loser), StringComparison.Ordinal).Replace("[winner]", CombatMission.GetTeamName(CombatMission.Winner), StringComparison.Ordinal);
			}
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x00026C50 File Offset: 0x00024E50
		public CombatMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.allowRespawning = prefab.ConfigElement.GetAttributeBool("AllowRespawning", false);
			ContentXElement configElement = prefab.ConfigElement;
			string key = "WinCondition";
			CombatMission.WinCondition winCondition = this.allowRespawning ? CombatMission.WinCondition.KillCount : CombatMission.WinCondition.LastManStanding;
			this.winCondition = configElement.GetAttributeEnum<CombatMission.WinCondition>(key, winCondition);
			this.PointsPerKill = prefab.ConfigElement.GetAttributeInt("PointsPerKill", 0);
			ContentXElement configElement2 = prefab.ConfigElement;
			string key2 = "TargetSubmarineType";
			TagAction.SubType subType = TagAction.SubType.Any;
			this.TargetSubmarineType = configElement2.GetAttributeEnum<TagAction.SubType>(key2, subType);
			string sonarTag = prefab.ConfigElement.GetAttributeString("targetSubmarineSonarLabel", string.Empty);
			if (!sonarTag.IsNullOrEmpty())
			{
				this.targetSubmarineSonarLabel = TextManager.Get(sonarTag);
			}
			if (this.allowRespawning && this.winCondition == CombatMission.WinCondition.LastManStanding)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in mission ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(": win condition cannot be \"last man standing\" when respawning is enabled.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, prefab.ContentPackage, false, false);
			}
			this.descriptions = new LocalizedString[]
			{
				TextManager.Get("MissionDescriptionNeutral." + prefab.TextIdentifier.ToString()).Fallback(prefab.ConfigElement.GetAttributeString("descriptionneutral", ""), true),
				TextManager.Get("MissionDescription1." + prefab.TextIdentifier.ToString()).Fallback(prefab.ConfigElement.GetAttributeString("description1", ""), true),
				TextManager.Get("MissionDescription2." + prefab.TextIdentifier.ToString()).Fallback(prefab.ConfigElement.GetAttributeString("description2", ""), true)
			};
			for (int i = 0; i < this.descriptions.Length; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					LocalizedString[] array = this.descriptions;
					int num = i;
					LocalizedString localizedString = this.descriptions[i];
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("[location");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(j + 1);
					defaultInterpolatedStringHandler2.AppendLiteral("]");
					array[num] = localizedString.Replace(defaultInterpolatedStringHandler2.ToStringAndClear(), locations[j].DisplayName, StringComparison.Ordinal).Replace("[winscore]", this.WinScore.ToString(), StringComparison.Ordinal);
				}
			}
			CombatMission.teamNames = new LocalizedString[]
			{
				TextManager.Get("MissionTeam1." + prefab.TextIdentifier.ToString()).Fallback(TextManager.Get(prefab.ConfigElement.GetAttributeString("teamname1", "missionteam1.pvpmission")), true),
				TextManager.Get("MissionTeam2." + prefab.TextIdentifier.ToString()).Fallback(TextManager.Get(prefab.ConfigElement.GetAttributeString("teamname2", "missionteam2.pvpmission")), true)
			};
			if (this.winCondition == CombatMission.WinCondition.KillCount && this.PointsPerKill == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(75, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Potential error in mission ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral(": win condition is kill count, but ");
				defaultInterpolatedStringHandler3.AppendFormatted("PointsPerKill");
				defaultInterpolatedStringHandler3.AppendLiteral(" is set to 0.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
			}
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00026FFC File Offset: 0x000251FC
		public static LocalizedString GetTeamName(CharacterTeamType teamID)
		{
			if (teamID == CharacterTeamType.Team1)
			{
				if (CombatMission.teamNames.Length == 0)
				{
					return "Team 1";
				}
				return CombatMission.teamNames[0];
			}
			else
			{
				if (teamID != CharacterTeamType.Team2)
				{
					return "Invalid Team";
				}
				if (CombatMission.teamNames.Length <= 1)
				{
					return "Team 2";
				}
				return CombatMission.teamNames[1];
			}
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00027053 File Offset: 0x00025253
		public static bool IsInWinningTeam(Character character)
		{
			return character != null && CombatMission.Winner != CharacterTeamType.None && CombatMission.Winner == character.TeamID;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00027070 File Offset: 0x00025270
		protected override void StartMissionSpecific(Level level)
		{
			if (GameMain.NetworkMember == null)
			{
				DebugConsole.ThrowError("Combat missions cannot be played in the single player mode.", null, null, false, false);
				return;
			}
			this.subs = new Submarine[]
			{
				Submarine.MainSubs[0],
				Submarine.MainSubs[1]
			};
			if (this.Prefab.LoadSubmarines)
			{
				this.subs[0].NeutralizeBallast();
				this.subs[0].TeamID = CharacterTeamType.Team1;
				this.subs[0].GetConnectedSubs().ForEach(delegate(Submarine s)
				{
					s.TeamID = CharacterTeamType.Team1;
				});
				this.subs[1].NeutralizeBallast();
				this.subs[1].TeamID = CharacterTeamType.Team2;
				this.subs[1].GetConnectedSubs().ForEach(delegate(Submarine s)
				{
					s.TeamID = CharacterTeamType.Team2;
				});
				GameSession.PlaceSubAtInitialPosition(this.subs[1], level, false, false);
				this.subs[1].FlipX(null);
			}
			this.crews = new List<Character>[]
			{
				new List<Character>(),
				new List<Character>()
			};
			this.roundEndTimer = 5f;
			if (this.TargetSubmarineType != TagAction.SubType.Any)
			{
				this.targetSubmarine = Submarine.Loaded.FirstOrDefault((Submarine s) => TagAction.SubmarineTypeMatches(s, this.TargetSubmarineType));
				if (this.targetSubmarine == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in mission ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(": could not find a submarine of the type ");
					defaultInterpolatedStringHandler.AppendFormatted<TagAction.SubType>(this.TargetSubmarineType);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
			}
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00027236 File Offset: 0x00025436
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return CombatMission.Winner > CharacterTeamType.None;
		}

		// Token: 0x04000224 RID: 548
		private const float RoundEndDuration = 5f;

		// Token: 0x04000225 RID: 549
		[Nullable(1)]
		private readonly bool[] teamDead = new bool[2];

		// Token: 0x04000226 RID: 550
		[Nullable(1)]
		private List<Character>[] crews;

		// Token: 0x04000227 RID: 551
		[Nullable(1)]
		private readonly List<CombatMission.KillCount> kills = new List<CombatMission.KillCount>();

		// Token: 0x04000228 RID: 552
		private float roundEndTimer;

		// Token: 0x04000229 RID: 553
		private float timeInTargetSubmarineTimer;

		// Token: 0x0400022A RID: 554
		private Submarine[] subs;

		// Token: 0x0400022B RID: 555
		private readonly LocalizedString[] descriptions;

		// Token: 0x0400022C RID: 556
		private static LocalizedString[] teamNames = new LocalizedString[]
		{
			"Team A",
			"Team B"
		};

		// Token: 0x0400022D RID: 557
		private readonly bool allowRespawning;

		// Token: 0x0400022E RID: 558
		private readonly CombatMission.WinCondition winCondition;

		// Token: 0x0400022F RID: 559
		private Submarine targetSubmarine;

		// Token: 0x04000230 RID: 560
		private LocalizedString targetSubmarineSonarLabel;

		// Token: 0x04000232 RID: 562
		public readonly int PointsPerKill;

		// Token: 0x04000233 RID: 563
		public readonly int[] Scores = new int[2];

		// Token: 0x020005D2 RID: 1490
		[NullableContext(2)]
		[Nullable(0)]
		private class KillCount
		{
			// Token: 0x06004C51 RID: 19537 RVA: 0x001DD098 File Offset: 0x001DB298
			[NullableContext(1)]
			public KillCount(Character victim, [Nullable(2)] Character killer)
			{
				this.Victim = victim;
				this.VictimClient = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => victim.IsClientOwner(c));
				this.Killer = killer;
				if (killer != null)
				{
					this.KillerClient = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => killer.IsClientOwner(c));
				}
			}

			// Token: 0x040027AA RID: 10154
			[Nullable(1)]
			public readonly Character Victim;

			// Token: 0x040027AB RID: 10155
			public readonly Client VictimClient;

			// Token: 0x040027AC RID: 10156
			public readonly Character Killer;

			// Token: 0x040027AD RID: 10157
			public readonly Client KillerClient;
		}

		// Token: 0x020005D3 RID: 1491
		private enum WinCondition
		{
			// Token: 0x040027AF RID: 10159
			LastManStanding,
			// Token: 0x040027B0 RID: 10160
			KillCount,
			// Token: 0x040027B1 RID: 10161
			ControlSubmarine
		}
	}
}
