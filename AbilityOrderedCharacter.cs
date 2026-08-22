using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B9 RID: 441
	internal class AbilityOrderedCharacter : AbilityObject, IAbilityCharacter
	{
		// Token: 0x0600312B RID: 12587 RVA: 0x00203AFC File Offset: 0x00201CFC
		public AbilityOrderedCharacter(Character character)
		{
			this.Character = character;
		}

		// Token: 0x17000CE0 RID: 3296
		// (get) Token: 0x0600312C RID: 12588 RVA: 0x00203B0B File Offset: 0x00201D0B
		// (set) Token: 0x0600312D RID: 12589 RVA: 0x00203B13 File Offset: 0x00201D13
		public Character Character { get; set; }
	}
}
