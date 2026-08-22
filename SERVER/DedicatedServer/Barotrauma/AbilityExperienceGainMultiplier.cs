using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000BF RID: 191
	internal class AbilityExperienceGainMultiplier : AbilityObject, IAbilityValue
	{
		// Token: 0x060015B0 RID: 5552 RVA: 0x000B9756 File Offset: 0x000B7956
		public AbilityExperienceGainMultiplier(float experienceGainMultiplier)
		{
			this.Value = experienceGainMultiplier;
		}

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x060015B1 RID: 5553 RVA: 0x000B9765 File Offset: 0x000B7965
		// (set) Token: 0x060015B2 RID: 5554 RVA: 0x000B976D File Offset: 0x000B796D
		public float Value { get; set; }
	}
}
