using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B8 RID: 440
	internal class AbilityCharacterKiller : AbilityObject, IAbilityCharacter
	{
		// Token: 0x06003128 RID: 12584 RVA: 0x00203ADC File Offset: 0x00201CDC
		public AbilityCharacterKiller(Character character)
		{
			this.Character = character;
		}

		// Token: 0x17000CDF RID: 3295
		// (get) Token: 0x06003129 RID: 12585 RVA: 0x00203AEB File Offset: 0x00201CEB
		// (set) Token: 0x0600312A RID: 12586 RVA: 0x00203AF3 File Offset: 0x00201CF3
		public Character Character { get; set; }
	}
}
