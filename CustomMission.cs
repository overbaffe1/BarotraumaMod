using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000052 RID: 82
	internal sealed class CustomMission : Mission
	{
		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06000B6E RID: 2926 RVA: 0x0006B06A File Offset: 0x0006926A
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State == this.SuccessState;
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06000B6F RID: 2927 RVA: 0x0006B07A File Offset: 0x0006927A
		public override bool DisplayAsFailed
		{
			get
			{
				return this.State == this.FailureState;
			}
		}

		// Token: 0x06000B70 RID: 2928 RVA: 0x0006B08C File Offset: 0x0006928C
		[NullableContext(1)]
		public CustomMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x0006B0E8 File Offset: 0x000692E8
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			bool flag = this.State == this.SuccessState;
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = !this.RequireDestinationReached;
				bool flag4 = flag3;
				if (!flag4)
				{
					bool flag5 = transitionType == CampaignMode.TransitionType.ProgressToNextLocation || transitionType == CampaignMode.TransitionType.ProgressToNextEmptyLocation;
					flag4 = flag5;
				}
				flag2 = flag4;
			}
			return flag2;
		}

		// Token: 0x040005EF RID: 1519
		public readonly int SuccessState = prefab.ConfigElement.GetAttributeInt("SuccessState", 1);

		// Token: 0x040005F0 RID: 1520
		public readonly int FailureState = prefab.ConfigElement.GetAttributeInt("FailureState", -1);

		// Token: 0x040005F1 RID: 1521
		public bool RequireDestinationReached = prefab.ConfigElement.GetAttributeBool("RequireDestinationReached", false);
	}
}
