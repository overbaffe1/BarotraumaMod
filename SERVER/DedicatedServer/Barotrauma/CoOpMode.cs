using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020001E3 RID: 483
	internal class CoOpMode : MissionMode
	{
		// Token: 0x060022EC RID: 8940 RVA: 0x000E9747 File Offset: 0x000E7947
		public CoOpMode(GameModePreset preset, IEnumerable<MissionPrefab> missionPrefabs) : base(preset, MissionMode.ValidateMissionPrefabs(missionPrefabs, MissionPrefab.CoOpMissionClasses))
		{
		}

		// Token: 0x060022ED RID: 8941 RVA: 0x000E975B File Offset: 0x000E795B
		public CoOpMode(GameModePreset preset, IEnumerable<Identifier> missionTypes, string seed) : base(preset, MissionMode.ValidateMissionTypes(missionTypes, MissionPrefab.CoOpMissionClasses), seed)
		{
		}
	}
}
