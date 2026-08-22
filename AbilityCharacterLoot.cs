using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B4 RID: 436
	internal sealed class AbilityCharacterLoot : AbilityObject, IAbilityCharacter
	{
		// Token: 0x17000CD4 RID: 3284
		// (get) Token: 0x0600310F RID: 12559 RVA: 0x00203968 File Offset: 0x00201B68
		// (set) Token: 0x06003110 RID: 12560 RVA: 0x00203970 File Offset: 0x00201B70
		public Character Character { get; set; }

		// Token: 0x06003111 RID: 12561 RVA: 0x00203979 File Offset: 0x00201B79
		public AbilityCharacterLoot(Character character)
		{
			this.Character = character;
		}
	}
}
