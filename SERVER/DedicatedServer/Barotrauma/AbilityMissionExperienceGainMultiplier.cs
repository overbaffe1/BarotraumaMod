using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001C8 RID: 456
	internal class AbilityMissionExperienceGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission, IAbilityCharacter
	{
		// Token: 0x06002187 RID: 8583 RVA: 0x000E18B9 File Offset: 0x000DFAB9
		public AbilityMissionExperienceGainMultiplier(Mission mission, float missionExperienceGainMultiplier, Character character)
		{
			this.Value = missionExperienceGainMultiplier;
			this.Mission = mission;
			this.Character = character;
		}

		// Token: 0x1700098D RID: 2445
		// (get) Token: 0x06002188 RID: 8584 RVA: 0x000E18D6 File Offset: 0x000DFAD6
		// (set) Token: 0x06002189 RID: 8585 RVA: 0x000E18DE File Offset: 0x000DFADE
		public float Value { get; set; }

		// Token: 0x1700098E RID: 2446
		// (get) Token: 0x0600218A RID: 8586 RVA: 0x000E18E7 File Offset: 0x000DFAE7
		// (set) Token: 0x0600218B RID: 8587 RVA: 0x000E18EF File Offset: 0x000DFAEF
		public Mission Mission { get; set; }

		// Token: 0x1700098F RID: 2447
		// (get) Token: 0x0600218C RID: 8588 RVA: 0x000E18F8 File Offset: 0x000DFAF8
		// (set) Token: 0x0600218D RID: 8589 RVA: 0x000E1900 File Offset: 0x000DFB00
		public Character Character { get; set; }
	}
}
