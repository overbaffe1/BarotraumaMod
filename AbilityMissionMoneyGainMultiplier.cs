using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020002B7 RID: 695
	internal class AbilityMissionMoneyGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission
	{
		// Token: 0x06003C17 RID: 15383 RVA: 0x00226F2C File Offset: 0x0022512C
		public AbilityMissionMoneyGainMultiplier(Mission mission, float moneyGainMultiplier)
		{
			this.Value = moneyGainMultiplier;
			this.Mission = mission;
		}

		// Token: 0x17000FC9 RID: 4041
		// (get) Token: 0x06003C18 RID: 15384 RVA: 0x00226F42 File Offset: 0x00225142
		// (set) Token: 0x06003C19 RID: 15385 RVA: 0x00226F4A File Offset: 0x0022514A
		public float Value { get; set; }

		// Token: 0x17000FCA RID: 4042
		// (get) Token: 0x06003C1A RID: 15386 RVA: 0x00226F53 File Offset: 0x00225153
		// (set) Token: 0x06003C1B RID: 15387 RVA: 0x00226F5B File Offset: 0x0022515B
		public Mission Mission { get; set; }
	}
}
