using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001BF RID: 447
	internal sealed class AbilitySkillGain : AbilityObject, IAbilityValue, IAbilitySkillIdentifier, IAbilityCharacter
	{
		// Token: 0x06003168 RID: 12648 RVA: 0x002044FE File Offset: 0x002026FE
		public AbilitySkillGain(float skillAmount, Identifier skillIdentifier, Character character, bool gainedFromAbility)
		{
			this.Value = skillAmount;
			this.SkillIdentifier = skillIdentifier;
			this.Character = character;
			this.GainedFromAbility = gainedFromAbility;
		}

		// Token: 0x17000CF1 RID: 3313
		// (get) Token: 0x06003169 RID: 12649 RVA: 0x00204523 File Offset: 0x00202723
		// (set) Token: 0x0600316A RID: 12650 RVA: 0x0020452B File Offset: 0x0020272B
		public Character Character { get; set; }

		// Token: 0x17000CF2 RID: 3314
		// (get) Token: 0x0600316B RID: 12651 RVA: 0x00204534 File Offset: 0x00202734
		// (set) Token: 0x0600316C RID: 12652 RVA: 0x0020453C File Offset: 0x0020273C
		public float Value { get; set; }

		// Token: 0x17000CF3 RID: 3315
		// (get) Token: 0x0600316D RID: 12653 RVA: 0x00204545 File Offset: 0x00202745
		// (set) Token: 0x0600316E RID: 12654 RVA: 0x0020454D File Offset: 0x0020274D
		public Identifier SkillIdentifier { get; set; }

		// Token: 0x17000CF4 RID: 3316
		// (get) Token: 0x0600316F RID: 12655 RVA: 0x00204556 File Offset: 0x00202756
		public bool GainedFromAbility { get; }
	}
}
