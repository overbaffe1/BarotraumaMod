using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020002D1 RID: 721
	internal class CoOpMode : MissionMode
	{
		// Token: 0x06003CF1 RID: 15601 RVA: 0x0022C7D4 File Offset: 0x0022A9D4
		public CoOpMode(GameModePreset preset, IEnumerable<MissionPrefab> missionPrefabs) : base(preset, MissionMode.ValidateMissionPrefabs(missionPrefabs, MissionPrefab.CoOpMissionClasses))
		{
		}

		// Token: 0x06003CF2 RID: 15602 RVA: 0x0022C7E8 File Offset: 0x0022A9E8
		public CoOpMode(GameModePreset preset, IEnumerable<Identifier> missionTypes, string seed) : base(preset, MissionMode.ValidateMissionTypes(missionTypes, MissionPrefab.CoOpMissionClasses), seed)
		{
		}
	}
}
