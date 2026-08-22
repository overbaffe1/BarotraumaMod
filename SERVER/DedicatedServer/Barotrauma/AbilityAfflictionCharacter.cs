using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000D8 RID: 216
	internal class AbilityAfflictionCharacter : AbilityObject, IAbilityAffliction, IAbilityCharacter
	{
		// Token: 0x06001784 RID: 6020 RVA: 0x000C3495 File Offset: 0x000C1695
		public AbilityAfflictionCharacter(Affliction affliction, Character character)
		{
			this.Affliction = affliction;
			this.Character = character;
		}

		// Token: 0x170006E6 RID: 1766
		// (get) Token: 0x06001785 RID: 6021 RVA: 0x000C34AB File Offset: 0x000C16AB
		// (set) Token: 0x06001786 RID: 6022 RVA: 0x000C34B3 File Offset: 0x000C16B3
		public Character Character { get; set; }

		// Token: 0x170006E7 RID: 1767
		// (get) Token: 0x06001787 RID: 6023 RVA: 0x000C34BC File Offset: 0x000C16BC
		// (set) Token: 0x06001788 RID: 6024 RVA: 0x000C34C4 File Offset: 0x000C16C4
		public Affliction Affliction { get; set; }
	}
}
