using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B2 RID: 178
	internal sealed class AbilityCharacterLoot : AbilityObject, IAbilityCharacter
	{
		// Token: 0x1700062E RID: 1582
		// (get) Token: 0x0600154C RID: 5452 RVA: 0x000B8B54 File Offset: 0x000B6D54
		// (set) Token: 0x0600154D RID: 5453 RVA: 0x000B8B5C File Offset: 0x000B6D5C
		public Character Character { get; set; }

		// Token: 0x0600154E RID: 5454 RVA: 0x000B8B65 File Offset: 0x000B6D65
		public AbilityCharacterLoot(Character character)
		{
			this.Character = character;
		}
	}
}
