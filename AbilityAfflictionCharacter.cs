using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001D4 RID: 468
	internal class AbilityAfflictionCharacter : AbilityObject, IAbilityAffliction, IAbilityCharacter
	{
		// Token: 0x06003280 RID: 12928 RVA: 0x00209C8B File Offset: 0x00207E8B
		public AbilityAfflictionCharacter(Affliction affliction, Character character)
		{
			this.Affliction = affliction;
			this.Character = character;
		}

		// Token: 0x17000D4D RID: 3405
		// (get) Token: 0x06003281 RID: 12929 RVA: 0x00209CA1 File Offset: 0x00207EA1
		// (set) Token: 0x06003282 RID: 12930 RVA: 0x00209CA9 File Offset: 0x00207EA9
		public Character Character { get; set; }

		// Token: 0x17000D4E RID: 3406
		// (get) Token: 0x06003283 RID: 12931 RVA: 0x00209CB2 File Offset: 0x00207EB2
		// (set) Token: 0x06003284 RID: 12932 RVA: 0x00209CBA File Offset: 0x00207EBA
		public Affliction Affliction { get; set; }
	}
}
