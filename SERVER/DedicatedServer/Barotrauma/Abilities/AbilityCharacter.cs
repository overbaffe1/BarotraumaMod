using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000328 RID: 808
	internal class AbilityCharacter : AbilityObject, IAbilityCharacter
	{
		// Token: 0x06003243 RID: 12867 RVA: 0x0015491C File Offset: 0x00152B1C
		public AbilityCharacter(Character character)
		{
			this.Character = character;
		}

		// Token: 0x17000E24 RID: 3620
		// (get) Token: 0x06003244 RID: 12868 RVA: 0x0015492B File Offset: 0x00152B2B
		// (set) Token: 0x06003245 RID: 12869 RVA: 0x00154933 File Offset: 0x00152B33
		public Character Character { get; set; }
	}
}
