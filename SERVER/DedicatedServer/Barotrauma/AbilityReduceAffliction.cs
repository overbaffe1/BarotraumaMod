using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000D9 RID: 217
	internal class AbilityReduceAffliction : AbilityObject, IAbilityCharacter, IAbilityValue
	{
		// Token: 0x06001789 RID: 6025 RVA: 0x000C34CD File Offset: 0x000C16CD
		public AbilityReduceAffliction(Character character, float value)
		{
			this.Character = character;
			this.Value = value;
		}

		// Token: 0x170006E8 RID: 1768
		// (get) Token: 0x0600178A RID: 6026 RVA: 0x000C34E3 File Offset: 0x000C16E3
		// (set) Token: 0x0600178B RID: 6027 RVA: 0x000C34EB File Offset: 0x000C16EB
		public Character Character { get; set; }

		// Token: 0x170006E9 RID: 1769
		// (get) Token: 0x0600178C RID: 6028 RVA: 0x000C34F4 File Offset: 0x000C16F4
		// (set) Token: 0x0600178D RID: 6029 RVA: 0x000C34FC File Offset: 0x000C16FC
		public float Value { get; set; }
	}
}
