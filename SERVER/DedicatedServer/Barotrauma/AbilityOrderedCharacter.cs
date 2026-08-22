using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B7 RID: 183
	internal class AbilityOrderedCharacter : AbilityObject, IAbilityCharacter
	{
		// Token: 0x06001568 RID: 5480 RVA: 0x000B8CE8 File Offset: 0x000B6EE8
		public AbilityOrderedCharacter(Character character)
		{
			this.Character = character;
		}

		// Token: 0x1700063A RID: 1594
		// (get) Token: 0x06001569 RID: 5481 RVA: 0x000B8CF7 File Offset: 0x000B6EF7
		// (set) Token: 0x0600156A RID: 5482 RVA: 0x000B8CFF File Offset: 0x000B6EFF
		public Character Character { get; set; }
	}
}
