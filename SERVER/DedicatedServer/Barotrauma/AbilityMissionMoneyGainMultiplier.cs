using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001C7 RID: 455
	internal class AbilityMissionMoneyGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission
	{
		// Token: 0x06002182 RID: 8578 RVA: 0x000E1881 File Offset: 0x000DFA81
		public AbilityMissionMoneyGainMultiplier(Mission mission, float moneyGainMultiplier)
		{
			this.Value = moneyGainMultiplier;
			this.Mission = mission;
		}

		// Token: 0x1700098B RID: 2443
		// (get) Token: 0x06002183 RID: 8579 RVA: 0x000E1897 File Offset: 0x000DFA97
		// (set) Token: 0x06002184 RID: 8580 RVA: 0x000E189F File Offset: 0x000DFA9F
		public float Value { get; set; }

		// Token: 0x1700098C RID: 2444
		// (get) Token: 0x06002185 RID: 8581 RVA: 0x000E18A8 File Offset: 0x000DFAA8
		// (set) Token: 0x06002186 RID: 8582 RVA: 0x000E18B0 File Offset: 0x000DFAB0
		public Mission Mission { get; set; }
	}
}
