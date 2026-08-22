using System;
using System.Collections.Generic;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001E6 RID: 486
	internal class PvPMode : MissionMode
	{
		// Token: 0x06002301 RID: 8961 RVA: 0x000E9921 File Offset: 0x000E7B21
		public PvPMode(GameModePreset preset, IEnumerable<MissionPrefab> missionPrefabs) : base(preset, MissionMode.ValidateMissionPrefabs(missionPrefabs, MissionPrefab.PvPMissionClasses))
		{
			if (this.Missions.None(null))
			{
				throw new Exception("Attempted to start PvPMode without a mission.");
			}
		}

		// Token: 0x06002302 RID: 8962 RVA: 0x000E994E File Offset: 0x000E7B4E
		public PvPMode(GameModePreset preset, IEnumerable<Identifier> missionTypes, string seed) : base(preset, MissionMode.ValidateMissionTypes(missionTypes, MissionPrefab.PvPMissionClasses), seed)
		{
			if (this.Missions.None(null))
			{
				throw new Exception("Attempted to start PvPMode without a mission.");
			}
		}
	}
}
