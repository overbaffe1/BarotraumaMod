using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001D5 RID: 469
	internal class AbilityReduceAffliction : AbilityObject, IAbilityCharacter, IAbilityValue
	{
		// Token: 0x06003285 RID: 12933 RVA: 0x00209CC3 File Offset: 0x00207EC3
		public AbilityReduceAffliction(Character character, float value)
		{
			this.Character = character;
			this.Value = value;
		}

		// Token: 0x17000D4F RID: 3407
		// (get) Token: 0x06003286 RID: 12934 RVA: 0x00209CD9 File Offset: 0x00207ED9
		// (set) Token: 0x06003287 RID: 12935 RVA: 0x00209CE1 File Offset: 0x00207EE1
		public Character Character { get; set; }

		// Token: 0x17000D50 RID: 3408
		// (get) Token: 0x06003288 RID: 12936 RVA: 0x00209CEA File Offset: 0x00207EEA
		// (set) Token: 0x06003289 RID: 12937 RVA: 0x00209CF2 File Offset: 0x00207EF2
		public float Value { get; set; }
	}
}
