using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020002B8 RID: 696
	internal class AbilityMissionExperienceGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission, IAbilityCharacter
	{
		// Token: 0x06003C1C RID: 15388 RVA: 0x00226F64 File Offset: 0x00225164
		public AbilityMissionExperienceGainMultiplier(Mission mission, float missionExperienceGainMultiplier, Character character)
		{
			this.Value = missionExperienceGainMultiplier;
			this.Mission = mission;
			this.Character = character;
		}

		// Token: 0x17000FCB RID: 4043
		// (get) Token: 0x06003C1D RID: 15389 RVA: 0x00226F81 File Offset: 0x00225181
		// (set) Token: 0x06003C1E RID: 15390 RVA: 0x00226F89 File Offset: 0x00225189
		public float Value { get; set; }

		// Token: 0x17000FCC RID: 4044
		// (get) Token: 0x06003C1F RID: 15391 RVA: 0x00226F92 File Offset: 0x00225192
		// (set) Token: 0x06003C20 RID: 15392 RVA: 0x00226F9A File Offset: 0x0022519A
		public Mission Mission { get; set; }

		// Token: 0x17000FCD RID: 4045
		// (get) Token: 0x06003C21 RID: 15393 RVA: 0x00226FA3 File Offset: 0x002251A3
		// (set) Token: 0x06003C22 RID: 15394 RVA: 0x00226FAB File Offset: 0x002251AB
		public Character Character { get; set; }
	}
}
