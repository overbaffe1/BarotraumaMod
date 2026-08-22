using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000051 RID: 81
	internal class CombatMission : Mission
	{
		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000B56 RID: 2902 RVA: 0x0006A718 File Offset: 0x00068918
		public override LocalizedString Description
		{
			get
			{
				if (this.descriptions == null)
				{
					return "";
				}
				GameClient client = GameMain.Client;
				if (((client != null) ? client.Character : null) == null)
				{
					return this.descriptions[0];
				}
				return this.descriptions[(GameMain.Client.Character.TeamID == CharacterTeamType.Team1) ? 1 : 2];
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000B57 RID: 2903 RVA: 0x0006A771 File Offset: 0x00068971
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000B58 RID: 2904 RVA: 0x0006A774 File Offset: 0x00068974
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000B59 RID: 2905 RVA: 0x0006A778 File Offset: 0x00068978
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				CombatMission.<get_SonarLabels>d__11 <get_SonarLabels>d__ = new CombatMission.<get_SonarLabels>d__11(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x0006A798 File Offset: 0x00068998
		public static Color GetTeamColor(CharacterTeamType teamID)
		{
			if (teamID == CharacterTeamType.Team1)
			{
				GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("CoalitionIcon");
				if (componentStyle == null)
				{
					return GUIStyle.Blue;
				}
				return componentStyle.Color;
			}
			else
			{
				if (teamID != CharacterTeamType.Team2)
				{
					return Color.White;
				}
				GUIComponentStyle componentStyle2 = GUIStyle.GetComponentStyle("SeparatistIcon");
				if (componentStyle2 == null)
				{
					return GUIStyle.Orange;
				}
				return componentStyle2.Color;
			}
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x0006A7F0 File Offset: 0x000689F0
		public int GetClientKillCount(Client client)
		{
			int kills;
			if (this.clientKills.TryGetValue(client.SessionId, out kills))
			{
				return kills;
			}
			return 0;
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0006A818 File Offset: 0x00068A18
		public int GetClientDeathCount(Client client)
		{
			int deaths;
			if (this.clientDeaths.TryGetValue(client.SessionId, out deaths))
			{
				return deaths;
			}
			return 0;
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x0006A840 File Offset: 0x00068A40
		public int GetBotKillCount(CharacterInfo botInfo)
		{
			int kills;
			if (this.botKills.TryGetValue(botInfo.ID, out kills))
			{
				return kills;
			}
			return 0;
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x0006A868 File Offset: 0x00068A68
		public int GetBotDeathCount(CharacterInfo botInfo)
		{
			int deaths;
			if (this.botDeaths.TryGetValue(botInfo.ID, out deaths))
			{
				return deaths;
			}
			return 0;
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x0006A890 File Offset: 0x00068A90
		public override void ClientRead(IReadMessage msg)
		{
			base.ClientRead(msg);
			this.Scores[0] = (int)msg.ReadUInt16();
			this.Scores[1] = (int)msg.ReadUInt16();
			uint clientCount = msg.ReadVariableUInt32();
			int i = 0;
			while ((long)i < (long)((ulong)clientCount))
			{
				byte clientId = msg.ReadByte();
				this.clientDeaths[clientId] = (int)msg.ReadVariableUInt32();
				this.clientKills[clientId] = (int)msg.ReadVariableUInt32();
				i++;
			}
			uint botCount = msg.ReadVariableUInt32();
			int j = 0;
			while ((long)j < (long)((ulong)botCount))
			{
				ushort botId = msg.ReadUInt16();
				this.botDeaths[botId] = (int)msg.ReadVariableUInt32();
				this.botKills[botId] = (int)msg.ReadVariableUInt32();
				j++;
			}
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06000B60 RID: 2912 RVA: 0x0006A947 File Offset: 0x00068B47
		public override bool AllowRespawning
		{
			get
			{
				return this.allowRespawning;
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000B61 RID: 2913 RVA: 0x0006A94F File Offset: 0x00068B4F
		// (set) Token: 0x06000B62 RID: 2914 RVA: 0x0006A957 File Offset: 0x00068B57
		public TagAction.SubType TargetSubmarineType { get; set; }

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06000B63 RID: 2915 RVA: 0x0006A960 File Offset: 0x00068B60
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

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06000B64 RID: 2916 RVA: 0x0006A978 File Offset: 0x00068B78
		public bool HasWinScore
		{
			get
			{
				return this.winCondition != CombatMission.WinCondition.LastManStanding || this.PointsPerKill != 0;
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06000B65 RID: 2917 RVA: 0x0006A98D File Offset: 0x00068B8D
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

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000B66 RID: 2918 RVA: 0x0006A9BC File Offset: 0x00068BBC
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

		// Token: 0x06000B67 RID: 2919 RVA: 0x0006AA58 File Offset: 0x00068C58
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

		// Token: 0x06000B68 RID: 2920 RVA: 0x0006AE18 File Offset: 0x00069018
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

		// Token: 0x06000B69 RID: 2921 RVA: 0x0006AE6F File Offset: 0x0006906F
		public static bool IsInWinningTeam(Character character)
		{
			return character != null && CombatMission.Winner != CharacterTeamType.None && CombatMission.Winner == character.TeamID;
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x0006AE8C File Offset: 0x0006908C
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

		// Token: 0x06000B6B RID: 2923 RVA: 0x0006B02B File Offset: 0x0006922B
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return CombatMission.Winner > CharacterTeamType.None;
		}

		// Token: 0x040005E1 RID: 1505
		private readonly Dictionary<byte, int> clientKills = new Dictionary<byte, int>();

		// Token: 0x040005E2 RID: 1506
		private readonly Dictionary<byte, int> clientDeaths = new Dictionary<byte, int>();

		// Token: 0x040005E3 RID: 1507
		private readonly Dictionary<ushort, int> botKills = new Dictionary<ushort, int>();

		// Token: 0x040005E4 RID: 1508
		private readonly Dictionary<ushort, int> botDeaths = new Dictionary<ushort, int>();

		// Token: 0x040005E5 RID: 1509
		private Submarine[] subs;

		// Token: 0x040005E6 RID: 1510
		private readonly LocalizedString[] descriptions;

		// Token: 0x040005E7 RID: 1511
		private static LocalizedString[] teamNames = new LocalizedString[]
		{
			"Team A",
			"Team B"
		};

		// Token: 0x040005E8 RID: 1512
		private readonly bool allowRespawning;

		// Token: 0x040005E9 RID: 1513
		private readonly CombatMission.WinCondition winCondition;

		// Token: 0x040005EA RID: 1514
		private Submarine targetSubmarine;

		// Token: 0x040005EB RID: 1515
		private LocalizedString targetSubmarineSonarLabel;

		// Token: 0x040005ED RID: 1517
		public readonly int PointsPerKill;

		// Token: 0x040005EE RID: 1518
		public readonly int[] Scores = new int[2];

		// Token: 0x020007D0 RID: 2000
		private enum WinCondition
		{
			// Token: 0x04003BD7 RID: 15319
			LastManStanding,
			// Token: 0x04003BD8 RID: 15320
			KillCount,
			// Token: 0x04003BD9 RID: 15321
			ControlSubmarine
		}
	}
}
