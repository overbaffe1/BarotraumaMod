using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000059 RID: 89
	internal abstract class MissionMode : GameMode
	{
		// Token: 0x06000C19 RID: 3097 RVA: 0x000706B0 File Offset: 0x0006E8B0
		public override void ShowStartMessage()
		{
			foreach (Mission mission in this.missions)
			{
				if (mission.Prefab.ShowStartMessage)
				{
					RichString headerText = RichString.Rich(mission.Name, null);
					RichString text = RichString.Rich(mission.Description, null);
					LocalizedString[] buttons = Array.Empty<LocalizedString>();
					Sprite icon = mission.Prefab.Icon;
					GUIMessageBox guimessageBox = new GUIMessageBox(headerText, text, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.InGame, "", icon, "", null, null, false);
					guimessageBox.IconColor = mission.Prefab.IconColor;
					guimessageBox.UserData = "missionstartmessage";
				}
			}
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x06000C1A RID: 3098 RVA: 0x0007077C File Offset: 0x0006E97C
		public override IEnumerable<Mission> Missions
		{
			get
			{
				return this.missions;
			}
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x00070784 File Offset: 0x0006E984
		public MissionMode(GameModePreset preset, IEnumerable<MissionPrefab> missionPrefabs) : base(preset)
		{
			Location[] locations = new Location[]
			{
				GameMain.GameSession.StartLocation,
				GameMain.GameSession.EndLocation
			};
			foreach (MissionPrefab missionPrefab in missionPrefabs)
			{
				this.missions.Add(missionPrefab.Instantiate(locations, Submarine.MainSub));
			}
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x00070810 File Offset: 0x0006EA10
		public MissionMode(GameModePreset preset, IEnumerable<Identifier> missionTypes, string seed) : base(preset)
		{
			Location[] locations = new Location[]
			{
				GameMain.GameSession.StartLocation,
				GameMain.GameSession.EndLocation
			};
			float difficulty = GameMain.NetworkMember.ServerSettings.SelectedLevelDifficulty;
			Mission mission = Mission.LoadRandom(locations, seed, false, missionTypes, false, new float?(difficulty));
			if (mission == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(115, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find any missions matching the mission types ");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", from m in missionTypes
				select m.Value));
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("and the difficulty ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(difficulty);
				defaultInterpolatedStringHandler.AppendLiteral(". Ignoring the difficulty requirement...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				mission = Mission.LoadRandom(locations, seed, false, missionTypes, false, null);
			}
			if (mission != null)
			{
				this.missions.Add(mission);
				return;
			}
			DebugConsole.AddWarning("Could not find any missions matching the mission types " + string.Join(", ", from m in missionTypes
			select m.Value) + ".", null);
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x00070968 File Offset: 0x0006EB68
		protected static IEnumerable<MissionPrefab> ValidateMissionPrefabs(IEnumerable<MissionPrefab> missionPrefabs, Dictionary<Identifier, Type> missionClasses)
		{
			foreach (MissionPrefab missionPrefab in missionPrefabs)
			{
				if (!missionPrefab.CampaignOnly && !missionClasses.ContainsValue(missionPrefab.MissionClass))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Cannot start gamemode with a ");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(missionPrefab.MissionClass);
					defaultInterpolatedStringHandler.AppendLiteral(" mission.");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			return missionPrefabs;
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x000709FC File Offset: 0x0006EBFC
		public static IEnumerable<Identifier> ValidateMissionTypes(IEnumerable<Identifier> missionTypes, Dictionary<Identifier, Type> missionClasses)
		{
			return from type in missionTypes
			where (from missionPrefab in MissionPrefab.Prefabs
			orderby missionPrefab.UintIdentifier
			select missionPrefab).Any(delegate(MissionPrefab missionPrefab)
			{
				Identifier type = missionPrefab.Type;
				return type == type && !missionPrefab.CampaignOnly && missionClasses.ContainsValue(missionPrefab.MissionClass);
			})
			select type;
		}

		// Token: 0x04000639 RID: 1593
		private readonly List<Mission> missions = new List<Mission>();
	}
}
