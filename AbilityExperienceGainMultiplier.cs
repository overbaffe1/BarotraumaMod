using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001C0 RID: 448
	internal class AbilityExperienceGainMultiplier : AbilityObject, IAbilityValue
	{
		// Token: 0x06003170 RID: 12656 RVA: 0x0020455E File Offset: 0x0020275E
		public AbilityExperienceGainMultiplier(float experienceGainMultiplier)
		{
			this.Value = experienceGainMultiplier;
		}

		// Token: 0x17000CF5 RID: 3317
		// (get) Token: 0x06003171 RID: 12657 RVA: 0x0020456D File Offset: 0x0020276D
		// (set) Token: 0x06003172 RID: 12658 RVA: 0x00204575 File Offset: 0x00202775
		public float Value { get; set; }
	}
}
