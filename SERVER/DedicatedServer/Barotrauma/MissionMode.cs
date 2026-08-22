using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000033 RID: 51
	internal abstract class MissionMode : GameMode
	{
		// Token: 0x06000643 RID: 1603 RVA: 0x0003ABA8 File Offset: 0x00038DA8
		public override void ShowStartMessage()
		{
			foreach (Mission mission in this.missions)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("Mission"));
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ServerMessage);
				GameServer.Log(mission.Description.Value, ServerLog.MessageType.ServerMessage);
			}
		}

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x0003AC44 File Offset: 0x00038E44
		public override IEnumerable<Mission> Missions
		{
			get
			{
				return this.missions;
			}
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0003AC4C File Offset: 0x00038E4C
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

		// Token: 0x06000646 RID: 1606 RVA: 0x0003ACD8 File Offset: 0x00038ED8
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

		// Token: 0x06000647 RID: 1607 RVA: 0x0003AE30 File Offset: 0x00039030
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

		// Token: 0x06000648 RID: 1608 RVA: 0x0003AEC4 File Offset: 0x000390C4
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

		// Token: 0x0400031D RID: 797
		private readonly List<Mission> missions = new List<Mission>();
	}
}
