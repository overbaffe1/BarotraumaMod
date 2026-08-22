using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020002B9 RID: 697
	internal class AbilityMissionReputationGainMultiplier : AbilityObject, IAbilityValue, IAbilityMission, IAbilityCharacter
	{
		// Token: 0x06003C23 RID: 15395 RVA: 0x00226FB4 File Offset: 0x002251B4
		public AbilityMissionReputationGainMultiplier(Mission mission, float reputationMultiplier, Character character)
		{
			this.Value = reputationMultiplier;
			this.Mission = mission;
			this.Character = character;
		}

		// Token: 0x17000FCE RID: 4046
		// (get) Token: 0x06003C24 RID: 15396 RVA: 0x00226FD1 File Offset: 0x002251D1
		// (set) Token: 0x06003C25 RID: 15397 RVA: 0x00226FD9 File Offset: 0x002251D9
		public float Value { get; set; }

		// Token: 0x17000FCF RID: 4047
		// (get) Token: 0x06003C26 RID: 15398 RVA: 0x00226FE2 File Offset: 0x002251E2
		// (set) Token: 0x06003C27 RID: 15399 RVA: 0x00226FEA File Offset: 0x002251EA
		public Mission Mission { get; set; }

		// Token: 0x17000FD0 RID: 4048
		// (get) Token: 0x06003C28 RID: 15400 RVA: 0x00226FF3 File Offset: 0x002251F3
		// (set) Token: 0x06003C29 RID: 15401 RVA: 0x00226FFB File Offset: 0x002251FB
		public Character Character { get; set; }
	}
}
