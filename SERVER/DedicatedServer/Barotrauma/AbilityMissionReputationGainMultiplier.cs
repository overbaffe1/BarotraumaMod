using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001C9 RID: 457
	internal class AbilityMissionReputationGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission, IAbilityCharacter
	{
		// Token: 0x0600218E RID: 8590 RVA: 0x000E1909 File Offset: 0x000DFB09
		public AbilityMissionReputationGainMultiplier(Mission mission, float reputationMultiplier, Character character)
		{
			this.Value = reputationMultiplier;
			this.Mission = mission;
			this.Character = character;
		}

		// Token: 0x17000990 RID: 2448
		// (get) Token: 0x0600218F RID: 8591 RVA: 0x000E1926 File Offset: 0x000DFB26
		// (set) Token: 0x06002190 RID: 8592 RVA: 0x000E192E File Offset: 0x000DFB2E
		public float Value { get; set; }

		// Token: 0x17000991 RID: 2449
		// (get) Token: 0x06002191 RID: 8593 RVA: 0x000E1937 File Offset: 0x000DFB37
		// (set) Token: 0x06002192 RID: 8594 RVA: 0x000E193F File Offset: 0x000DFB3F
		public Mission Mission { get; set; }

		// Token: 0x17000992 RID: 2450
		// (get) Token: 0x06002193 RID: 8595 RVA: 0x000E1948 File Offset: 0x000DFB48
		// (set) Token: 0x06002194 RID: 8596 RVA: 0x000E1950 File Offset: 0x000DFB50
		public Character Character { get; set; }
	}
}
