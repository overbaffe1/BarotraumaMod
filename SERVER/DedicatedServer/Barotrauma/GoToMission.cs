using System;

namespace Barotrauma
{
	// Token: 0x020001C6 RID: 454
	internal class GoToMission : Mission
	{
		// Token: 0x0600217F RID: 8575 RVA: 0x000E182E File Offset: 0x000DFA2E
		public GoToMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
		}

		// Token: 0x06002180 RID: 8576 RVA: 0x000E1839 File Offset: 0x000DFA39
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				this.State = Math.Max(1, this.State);
			}
		}

		// Token: 0x06002181 RID: 8577 RVA: 0x000E1863 File Offset: 0x000DFA63
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			Level loaded = Level.Loaded;
			return (loaded != null && loaded.Type == LevelData.LevelType.Outpost) || transitionType == CampaignMode.TransitionType.ProgressToNextLocation;
		}
	}
}
