using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x0200025A RID: 602
	internal class AbilityAttackerSubmarine : AbilityObject, IAbilityCharacter, IAbilitySubmarine
	{
		// Token: 0x06002B46 RID: 11078 RVA: 0x0011C417 File Offset: 0x0011A617
		public AbilityAttackerSubmarine(Character character, Submarine submarine)
		{
			this.Character = character;
			this.Submarine = submarine;
		}

		// Token: 0x17000CE1 RID: 3297
		// (get) Token: 0x06002B47 RID: 11079 RVA: 0x0011C42D File Offset: 0x0011A62D
		// (set) Token: 0x06002B48 RID: 11080 RVA: 0x0011C435 File Offset: 0x0011A635
		public Character Character { get; set; }

		// Token: 0x17000CE2 RID: 3298
		// (get) Token: 0x06002B49 RID: 11081 RVA: 0x0011C43E File Offset: 0x0011A63E
		// (set) Token: 0x06002B4A RID: 11082 RVA: 0x0011C446 File Offset: 0x0011A646
		public Submarine Submarine { get; set; }
	}
}
