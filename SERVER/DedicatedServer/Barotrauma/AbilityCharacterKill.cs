using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B3 RID: 179
	internal class AbilityCharacterKill : AbilityObject, IAbilityCharacter
	{
		// Token: 0x0600154F RID: 5455 RVA: 0x000B8B74 File Offset: 0x000B6D74
		public AbilityCharacterKill(Character character, Character killer)
		{
			this.Character = character;
			this.Killer = killer;
		}

		// Token: 0x1700062F RID: 1583
		// (get) Token: 0x06001550 RID: 5456 RVA: 0x000B8B8A File Offset: 0x000B6D8A
		// (set) Token: 0x06001551 RID: 5457 RVA: 0x000B8B92 File Offset: 0x000B6D92
		public Character Character { get; set; }

		// Token: 0x17000630 RID: 1584
		// (get) Token: 0x06001552 RID: 5458 RVA: 0x000B8B9B File Offset: 0x000B6D9B
		// (set) Token: 0x06001553 RID: 5459 RVA: 0x000B8BA3 File Offset: 0x000B6DA3
		public Character Killer { get; set; }
	}
}
