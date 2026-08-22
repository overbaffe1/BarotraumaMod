using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020002D2 RID: 722
	internal class GameModePreset
	{
		// Token: 0x06003CF3 RID: 15603 RVA: 0x0022C800 File Offset: 0x0022AA00
		public GameModePreset(Identifier identifier, Type type, bool isSinglePlayer = false, bool votable = true)
		{
			this.Name = TextManager.Get("GameMode." + identifier.ToString());
			this.Description = TextManager.Get("GameModeDescription." + identifier.ToString()).Fallback("", true);
			this.Identifier = identifier;
			this.GameModeType = type;
			this.IsSinglePlayer = isSinglePlayer;
			this.Votable = votable;
			GameModePreset.List.Add(this);
		}

		// Token: 0x06003CF4 RID: 15604 RVA: 0x0022C890 File Offset: 0x0022AA90
		public static void Init()
		{
			GameModePreset.Tutorial = new GameModePreset("tutorial".ToIdentifier(), typeof(TutorialMode), true, true);
			GameModePreset.DevSandbox = new GameModePreset("devsandbox".ToIdentifier(), typeof(GameMode), true, true);
			GameModePreset.SinglePlayerCampaign = new GameModePreset("singleplayercampaign".ToIdentifier(), typeof(SinglePlayerCampaign), true, true);
			GameModePreset.TestMode = new GameModePreset("testmode".ToIdentifier(), typeof(TestGameMode), true, true);
			GameModePreset.Sandbox = new GameModePreset("sandbox".ToIdentifier(), typeof(GameMode), false, true);
			GameModePreset.Mission = new GameModePreset("mission".ToIdentifier(), typeof(CoOpMode), false, true);
			GameModePreset.PvP = new GameModePreset("pvp".ToIdentifier(), typeof(PvPMode), false, true);
			GameModePreset.MultiPlayerCampaign = new GameModePreset("multiplayercampaign".ToIdentifier(), typeof(MultiPlayerCampaign), false, true);
		}

		// Token: 0x04001F81 RID: 8065
		public static List<GameModePreset> List = new List<GameModePreset>();

		// Token: 0x04001F82 RID: 8066
		public static GameModePreset SinglePlayerCampaign;

		// Token: 0x04001F83 RID: 8067
		public static GameModePreset MultiPlayerCampaign;

		// Token: 0x04001F84 RID: 8068
		public static GameModePreset Tutorial;

		// Token: 0x04001F85 RID: 8069
		public static GameModePreset Mission;

		// Token: 0x04001F86 RID: 8070
		public static GameModePreset PvP;

		// Token: 0x04001F87 RID: 8071
		public static GameModePreset TestMode;

		// Token: 0x04001F88 RID: 8072
		public static GameModePreset Sandbox;

		// Token: 0x04001F89 RID: 8073
		public static GameModePreset DevSandbox;

		// Token: 0x04001F8A RID: 8074
		public readonly Type GameModeType;

		// Token: 0x04001F8B RID: 8075
		public readonly LocalizedString Name;

		// Token: 0x04001F8C RID: 8076
		public readonly LocalizedString Description;

		// Token: 0x04001F8D RID: 8077
		public readonly Identifier Identifier;

		// Token: 0x04001F8E RID: 8078
		public readonly bool IsSinglePlayer;

		// Token: 0x04001F8F RID: 8079
		public readonly bool Votable;
	}
}
