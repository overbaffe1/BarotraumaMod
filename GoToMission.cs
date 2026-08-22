using System;

namespace Barotrauma
{
	// Token: 0x02000056 RID: 86
	internal class GoToMission : Mission
	{
		// Token: 0x17000337 RID: 823
		// (get) Token: 0x06000BA9 RID: 2985 RVA: 0x0006D6EE File Offset: 0x0006B8EE
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State >= this.Prefab.MaxProgressState && (base.Completed || this.completeCheckDataAction == null);
			}
		}

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06000BAA RID: 2986 RVA: 0x0006D718 File Offset: 0x0006B918
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0006D71B File Offset: 0x0006B91B
		public GoToMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x0006D726 File Offset: 0x0006B926
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.Outpost)
			{
				this.State = Math.Max(1, this.State);
			}
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0006D750 File Offset: 0x0006B950
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			Level loaded = Level.Loaded;
			return (loaded != null && loaded.Type == LevelData.LevelType.Outpost) || transitionType == CampaignMode.TransitionType.ProgressToNextLocation;
		}
	}
}
