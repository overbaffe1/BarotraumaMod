using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B5 RID: 437
	internal class AbilityCharacterKill : AbilityObject, IAbilityCharacter
	{
		// Token: 0x06003112 RID: 12562 RVA: 0x00203988 File Offset: 0x00201B88
		public AbilityCharacterKill(Character character, Character killer)
		{
			this.Character = character;
			this.Killer = killer;
		}

		// Token: 0x17000CD5 RID: 3285
		// (get) Token: 0x06003113 RID: 12563 RVA: 0x0020399E File Offset: 0x00201B9E
		// (set) Token: 0x06003114 RID: 12564 RVA: 0x002039A6 File Offset: 0x00201BA6
		public Character Character { get; set; }

		// Token: 0x17000CD6 RID: 3286
		// (get) Token: 0x06003115 RID: 12565 RVA: 0x002039AF File Offset: 0x00201BAF
		// (set) Token: 0x06003116 RID: 12566 RVA: 0x002039B7 File Offset: 0x00201BB7
		public Character Killer { get; set; }
	}
}
