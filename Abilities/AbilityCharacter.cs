using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003EE RID: 1006
	internal class AbilityCharacter : AbilityObject, IAbilityCharacter
	{
		// Token: 0x0600467D RID: 18045 RVA: 0x0026C79C File Offset: 0x0026A99C
		public AbilityCharacter(Character character)
		{
			this.Character = character;
		}

		// Token: 0x17001214 RID: 4628
		// (get) Token: 0x0600467E RID: 18046 RVA: 0x0026C7AB File Offset: 0x0026A9AB
		// (set) Token: 0x0600467F RID: 18047 RVA: 0x0026C7B3 File Offset: 0x0026A9B3
		public Character Character { get; set; }
	}
}
