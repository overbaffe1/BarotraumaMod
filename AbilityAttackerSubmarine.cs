using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x02000331 RID: 817
	internal class AbilityAttackerSubmarine : AbilityObject, IAbilityCharacter, IAbilitySubmarine
	{
		// Token: 0x06004148 RID: 16712 RVA: 0x002446E7 File Offset: 0x002428E7
		public AbilityAttackerSubmarine(Character character, Submarine submarine)
		{
			this.Character = character;
			this.Submarine = submarine;
		}

		// Token: 0x17001164 RID: 4452
		// (get) Token: 0x06004149 RID: 16713 RVA: 0x002446FD File Offset: 0x002428FD
		// (set) Token: 0x0600414A RID: 16714 RVA: 0x00244705 File Offset: 0x00242905
		public Character Character { get; set; }

		// Token: 0x17001165 RID: 4453
		// (get) Token: 0x0600414B RID: 16715 RVA: 0x0024470E File Offset: 0x0024290E
		// (set) Token: 0x0600414C RID: 16716 RVA: 0x00244716 File Offset: 0x00242916
		public Submarine Submarine { get; set; }
	}
}
