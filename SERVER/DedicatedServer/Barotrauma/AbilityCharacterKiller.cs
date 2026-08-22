using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B6 RID: 182
	internal class AbilityCharacterKiller : AbilityObject, IAbilityCharacter
	{
		// Token: 0x06001565 RID: 5477 RVA: 0x000B8CC8 File Offset: 0x000B6EC8
		public AbilityCharacterKiller(Character character)
		{
			this.Character = character;
		}

		// Token: 0x17000639 RID: 1593
		// (get) Token: 0x06001566 RID: 5478 RVA: 0x000B8CD7 File Offset: 0x000B6ED7
		// (set) Token: 0x06001567 RID: 5479 RVA: 0x000B8CDF File Offset: 0x000B6EDF
		public Character Character { get; set; }
	}
}
