using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020001E5 RID: 485
	internal class GameModePreset
	{
		// Token: 0x060022FE RID: 8958 RVA: 0x000E97F8 File Offset: 0x000E79F8
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

		// Token: 0x060022FF RID: 8959 RVA: 0x000E9888 File Offset: 0x000E7A88
		public static void Init()
		{
			GameModePreset.Sandbox = new GameModePreset("sandbox".ToIdentifier(), typeof(GameMode), false, true);
			GameModePreset.Mission = new GameModePreset("mission".ToIdentifier(), typeof(CoOpMode), false, true);
			GameModePreset.PvP = new GameModePreset("pvp".ToIdentifier(), typeof(PvPMode), false, true);
			GameModePreset.MultiPlayerCampaign = new GameModePreset("multiplayercampaign".ToIdentifier(), typeof(MultiPlayerCampaign), false, true);
		}

		// Token: 0x040010DF RID: 4319
		public static List<GameModePreset> List = new List<GameModePreset>();

		// Token: 0x040010E0 RID: 4320
		public static GameModePreset SinglePlayerCampaign;

		// Token: 0x040010E1 RID: 4321
		public static GameModePreset MultiPlayerCampaign;

		// Token: 0x040010E2 RID: 4322
		public static GameModePreset Tutorial;

		// Token: 0x040010E3 RID: 4323
		public static GameModePreset Mission;

		// Token: 0x040010E4 RID: 4324
		public static GameModePreset PvP;

		// Token: 0x040010E5 RID: 4325
		public static GameModePreset TestMode;

		// Token: 0x040010E6 RID: 4326
		public static GameModePreset Sandbox;

		// Token: 0x040010E7 RID: 4327
		public static GameModePreset DevSandbox;

		// Token: 0x040010E8 RID: 4328
		public readonly Type GameModeType;

		// Token: 0x040010E9 RID: 4329
		public readonly LocalizedString Name;

		// Token: 0x040010EA RID: 4330
		public readonly LocalizedString Description;

		// Token: 0x040010EB RID: 4331
		public readonly Identifier Identifier;

		// Token: 0x040010EC RID: 4332
		public readonly bool IsSinglePlayer;

		// Token: 0x040010ED RID: 4333
		public readonly bool Votable;
	}
}
