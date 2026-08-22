using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001C5 RID: 453
	internal sealed class CustomMission : Mission
	{
		// Token: 0x0600217D RID: 8573 RVA: 0x000E178C File Offset: 0x000DF98C
		[NullableContext(1)]
		public CustomMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
		}

		// Token: 0x0600217E RID: 8574 RVA: 0x000E17E8 File Offset: 0x000DF9E8
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

		// Token: 0x04000FFC RID: 4092
		public readonly int SuccessState = prefab.ConfigElement.GetAttributeInt("SuccessState", 1);

		// Token: 0x04000FFD RID: 4093
		public readonly int FailureState = prefab.ConfigElement.GetAttributeInt("FailureState", -1);

		// Token: 0x04000FFE RID: 4094
		public bool RequireDestinationReached = prefab.ConfigElement.GetAttributeBool("RequireDestinationReached", false);
	}
}
