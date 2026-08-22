using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000BE RID: 190
	internal sealed class AbilitySkillGain : AbilityObject, IAbilityValue, IAbilitySkillIdentifier, IAbilityCharacter
	{
		// Token: 0x060015A8 RID: 5544 RVA: 0x000B96F6 File Offset: 0x000B78F6
		public AbilitySkillGain(float skillAmount, Identifier skillIdentifier, Character character, bool gainedFromAbility)
		{
			this.Value = skillAmount;
			this.SkillIdentifier = skillIdentifier;
			this.Character = character;
			this.GainedFromAbility = gainedFromAbility;
		}

		// Token: 0x1700064B RID: 1611
		// (get) Token: 0x060015A9 RID: 5545 RVA: 0x000B971B File Offset: 0x000B791B
		// (set) Token: 0x060015AA RID: 5546 RVA: 0x000B9723 File Offset: 0x000B7923
		public Character Character { get; set; }

		// Token: 0x1700064C RID: 1612
		// (get) Token: 0x060015AB RID: 5547 RVA: 0x000B972C File Offset: 0x000B792C
		// (set) Token: 0x060015AC RID: 5548 RVA: 0x000B9734 File Offset: 0x000B7934
		public float Value { get; set; }

		// Token: 0x1700064D RID: 1613
		// (get) Token: 0x060015AD RID: 5549 RVA: 0x000B973D File Offset: 0x000B793D
		// (set) Token: 0x060015AE RID: 5550 RVA: 0x000B9745 File Offset: 0x000B7945
		public Identifier SkillIdentifier { get; set; }

		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x060015AF RID: 5551 RVA: 0x000B974E File Offset: 0x000B794E
		public bool GainedFromAbility { get; }
	}
}
