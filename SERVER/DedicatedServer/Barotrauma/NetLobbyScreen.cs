using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000048 RID: 72
	internal class NetLobbyScreen : Screen
	{
		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000BBA RID: 3002 RVA: 0x0006FFC4 File Offset: 0x0006E1C4
		// (set) Token: 0x06000BBB RID: 3003 RVA: 0x0006FFCC File Offset: 0x0006E1CC
		public SubmarineInfo SelectedSub
		{
			get
			{
				return this.selectedSub;
			}
			set
			{
				this.selectedSub = value;
				this.lastUpdateID += 1;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null)
				{
					GameMain.NetworkMember.ServerSettings.ServerDetailsChanged = true;
				}
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06000BBC RID: 3004 RVA: 0x00070007 File Offset: 0x0006E207
		// (set) Token: 0x06000BBD RID: 3005 RVA: 0x0007000F File Offset: 0x0006E20F
		public SubmarineInfo SelectedEnemySub
		{
			[return: MaybeNull]
			get
			{
				return this.selectedEnemySub;
			}
			[param: AllowNull]
			set
			{
				this.selectedEnemySub = value;
				this.lastUpdateID += 1;
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06000BBE RID: 3006 RVA: 0x00070027 File Offset: 0x0006E227
		// (set) Token: 0x06000BBF RID: 3007 RVA: 0x0007002F File Offset: 0x0006E22F
		public SubmarineInfo SelectedShuttle
		{
			get
			{
				return this.selectedShuttle;
			}
			set
			{
				this.selectedShuttle = value;
				this.lastUpdateID += 1;
			}
		}

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06000BC0 RID: 3008 RVA: 0x00070047 File Offset: 0x0006E247
		public GameModePreset[] GameModes { get; }

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06000BC1 RID: 3009 RVA: 0x0007004F File Offset: 0x0006E24F
		// (set) Token: 0x06000BC2 RID: 3010 RVA: 0x00070058 File Offset: 0x0006E258
		public int SelectedModeIndex
		{
			get
			{
				return this.selectedModeIndex;
			}
			set
			{
				this.lastUpdateID += 1;
				this.selectedModeIndex = MathHelper.Clamp(value, 0, this.GameModes.Length - 1);
				if (this.SelectedMode != GameModePreset.MultiPlayerCampaign)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && Screen.Selected == this)
					{
						GameMain.GameSession = null;
					}
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null)
				{
					GameMain.NetworkMember.ServerSettings.GameModeIdentifier = this.SelectedModeIdentifier;
					GameMain.NetworkMember.ServerSettings.ServerDetailsChanged = true;
				}
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06000BC3 RID: 3011 RVA: 0x000700F6 File Offset: 0x0006E2F6
		// (set) Token: 0x06000BC4 RID: 3012 RVA: 0x0007010C File Offset: 0x0006E30C
		public Identifier SelectedModeIdentifier
		{
			get
			{
				return this.GameModes[this.SelectedModeIndex].Identifier;
			}
			set
			{
				Identifier selectedModeIdentifier = this.SelectedModeIdentifier;
				if (selectedModeIdentifier == value)
				{
					return;
				}
				for (int i = 0; i < this.GameModes.Length; i++)
				{
					if (this.GameModes[i].Identifier == value)
					{
						this.SelectedModeIndex = i;
						break;
					}
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null)
				{
					GameMain.NetworkMember.ServerSettings.GameModeIdentifier = this.SelectedModeIdentifier;
					GameMain.NetworkMember.ServerSettings.ServerDetailsChanged = true;
				}
			}
		}

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06000BC5 RID: 3013 RVA: 0x00070196 File Offset: 0x0006E396
		public GameModePreset SelectedMode
		{
			get
			{
				return this.GameModes[this.SelectedModeIndex];
			}
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06000BC6 RID: 3014 RVA: 0x000701A5 File Offset: 0x0006E3A5
		// (set) Token: 0x06000BC7 RID: 3015 RVA: 0x000701B8 File Offset: 0x0006E3B8
		public IEnumerable<Identifier> MissionTypes
		{
			get
			{
				return GameMain.NetworkMember.ServerSettings.AllowedRandomMissionTypes;
			}
			set
			{
				this.lastUpdateID += 1;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null)
				{
					GameMain.NetworkMember.ServerSettings.MissionTypes = string.Join<Identifier>(",", from t in value
					select t.ToIdentifier<Identifier>());
				}
			}
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x00070228 File Offset: 0x0006E428
		public NetLobbyScreen()
		{
			this.LevelSeed = ToolBox.RandomSeed(8);
			this.subs = (from s in SubmarineInfo.SavedSubmarines
			where s.Type == SubmarineType.Player && !s.HasTag(SubmarineTag.HideInMenus)
			select s).ToList<SubmarineInfo>();
			if (this.subs == null || this.subs.Count == 0)
			{
				throw new Exception("No submarines are available.");
			}
			this.selectedSub = this.subs.FirstOrDefault((SubmarineInfo s) => !s.HasTag(SubmarineTag.Shuttle));
			if (this.selectedSub == null)
			{
				DebugConsole.ThrowError("No full-size submarines available - choosing a shuttle as the main submarine.", null, null, false, false);
				this.selectedSub = this.subs[0];
			}
			this.selectedShuttle = this.subs.First((SubmarineInfo s) => s.HasTag(SubmarineTag.Shuttle));
			if (this.selectedShuttle == null)
			{
				DebugConsole.ThrowError("No shuttles available - choosing a full-size submarine as the shuttle.", null, null, false, false);
				this.selectedShuttle = this.subs[0];
			}
			DebugConsole.NewMessage("Selected sub: " + this.SelectedSub.Name, new Color?(Color.White), false);
			DebugConsole.NewMessage("Selected shuttle: " + this.SelectedShuttle.Name, new Color?(Color.White), false);
			this.GameModes = GameModePreset.List.ToArray();
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x000703B7 File Offset: 0x0006E5B7
		public IReadOnlyList<SubmarineInfo> GetSubList()
		{
			return this.subs;
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x000703BF File Offset: 0x0006E5BF
		public void AddSub(SubmarineInfo sub)
		{
			this.subs.Add(sub);
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000BCB RID: 3019 RVA: 0x000703CD File Offset: 0x0006E5CD
		// (set) Token: 0x06000BCC RID: 3020 RVA: 0x000703D8 File Offset: 0x0006E5D8
		public string LevelSeed
		{
			get
			{
				return this.levelSeed;
			}
			set
			{
				if (this.levelSeed == value)
				{
					return;
				}
				this.lastUpdateID += 1;
				this.levelSeed = value;
				LocationType.Random(new MTRandom(ToolBox.StringToInt(this.levelSeed)), null, null, false, null);
			}
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00070434 File Offset: 0x0006E634
		public void ToggleCampaignMode(bool enabled)
		{
			for (int i = 0; i < this.GameModes.Length; i++)
			{
				if (this.GameModes[i] == GameModePreset.MultiPlayerCampaign == enabled)
				{
					this.selectedModeIndex = i;
					break;
				}
			}
			this.lastUpdateID += 1;
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x00070480 File Offset: 0x0006E680
		public override void Select()
		{
			base.Select();
			Voting.ResetVotes(GameMain.Server.ConnectedClients, false);
			if (this.SelectedMode != GameModePreset.MultiPlayerCampaign)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode && Screen.Selected == this)
				{
					GameMain.GameSession = null;
				}
			}
			if (GameMain.Server.ServerSettings.SelectedSubmarine.IsNullOrEmpty())
			{
				ServerSettings serverSettings = GameMain.Server.ServerSettings;
				SubmarineInfo submarineInfo = this.SelectedSub;
				serverSettings.SelectedSubmarine = ((submarineInfo != null) ? submarineInfo.Name : null);
			}
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00070510 File Offset: 0x0006E710
		public void RandomizeSettings()
		{
			if (GameMain.Server.ServerSettings.RandomizeSeed)
			{
				this.LevelSeed = ToolBox.RandomSeed(8);
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) == null)
			{
				if (GameMain.Server.ServerSettings.SubSelectionMode == SelectionMode.Random)
				{
					List<SubmarineInfo> nonShuttles = (from c in SubmarineInfo.SavedSubmarines
					where !c.HasTag(SubmarineTag.Shuttle) && !c.HasTag(SubmarineTag.HideInMenus) && c.IsPlayer
					select c).ToList<SubmarineInfo>();
					this.SelectedSub = nonShuttles[Rand.Range(0, nonShuttles.Count, Rand.RandSync.Unsynced)];
				}
				if (GameMain.Server.ServerSettings.ModeSelectionMode == SelectionMode.Random)
				{
					GameModePreset[] allowedGameModes = Array.FindAll<GameModePreset>(this.GameModes, (GameModePreset m) => !m.IsSinglePlayer && m != GameModePreset.MultiPlayerCampaign);
					this.SelectedModeIdentifier = allowedGameModes[Rand.Range(0, allowedGameModes.Length, Rand.RandSync.Unsynced)].Identifier;
				}
				GameMain.Server.ServerSettings.SelectNonHiddenSubmarine(null);
			}
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000BD0 RID: 3024 RVA: 0x0007060F File Offset: 0x0006E80F
		// (set) Token: 0x06000BD1 RID: 3025 RVA: 0x00070636 File Offset: 0x0006E836
		public ushort LastUpdateID
		{
			get
			{
				if (GameMain.Server != null && this.lastUpdateID < 1)
				{
					this.lastUpdateID += 1;
				}
				return this.lastUpdateID;
			}
			set
			{
				this.lastUpdateID = value;
			}
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x0007063F File Offset: 0x0006E83F
		public void SetLevelDifficulty(float difficulty)
		{
			difficulty = MathHelper.Clamp(difficulty, 0f, 100f);
			if (GameMain.Server != null)
			{
				GameMain.Server.ServerSettings.SelectedLevelDifficulty = difficulty;
				this.lastUpdateID += 1;
			}
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x0007067C File Offset: 0x0006E87C
		public void SetBotCount(int botCount)
		{
			if (GameMain.Server != null)
			{
				if (botCount < 0)
				{
					botCount = GameMain.Server.ServerSettings.MaxBotCount;
				}
				if (botCount > GameMain.Server.ServerSettings.MaxBotCount)
				{
					botCount = 0;
				}
				GameMain.Server.ServerSettings.BotCount = botCount;
				this.lastUpdateID += 1;
			}
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x000706D9 File Offset: 0x0006E8D9
		public void SetBotSpawnMode(BotSpawnMode botSpawnMode)
		{
			if (GameMain.Server != null)
			{
				GameMain.Server.ServerSettings.BotSpawnMode = botSpawnMode;
				this.lastUpdateID += 1;
			}
		}

		// Token: 0x06000BD5 RID: 3029 RVA: 0x00070701 File Offset: 0x0006E901
		public void SetTraitorProbability(float probability)
		{
			if (GameMain.NetworkMember != null)
			{
				GameMain.NetworkMember.ServerSettings.TraitorProbability = probability;
			}
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x0007071A File Offset: 0x0006E91A
		public void SetTraitorDangerLevel(int dangerLevel)
		{
			if (GameMain.NetworkMember != null)
			{
				GameMain.NetworkMember.ServerSettings.TraitorDangerLevel = dangerLevel;
			}
			if (GameMain.Server != null)
			{
				GameMain.Server.ServerSettings.TraitorDangerLevel = dangerLevel;
			}
		}

		// Token: 0x04000508 RID: 1288
		private SubmarineInfo selectedSub;

		// Token: 0x04000509 RID: 1289
		private SubmarineInfo selectedEnemySub;

		// Token: 0x0400050A RID: 1290
		private SubmarineInfo selectedShuttle;

		// Token: 0x0400050B RID: 1291
		public bool RadiationEnabled = true;

		// Token: 0x0400050D RID: 1293
		private int selectedModeIndex;

		// Token: 0x0400050E RID: 1294
		private List<SubmarineInfo> subs;

		// Token: 0x0400050F RID: 1295
		private ushort lastUpdateID;

		// Token: 0x04000510 RID: 1296
		private string levelSeed = "";
	}
}
