using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B7 RID: 439
	internal class AbilityAttackResult : AbilityObject, IAbilityAttackResult
	{
		// Token: 0x17000CDE RID: 3294
		// (get) Token: 0x06003125 RID: 12581 RVA: 0x00203ABC File Offset: 0x00201CBC
		// (set) Token: 0x06003126 RID: 12582 RVA: 0x00203AC4 File Offset: 0x00201CC4
		public AttackResult AttackResult { get; set; }

		// Token: 0x06003127 RID: 12583 RVA: 0x00203ACD File Offset: 0x00201CCD
		public AbilityAttackResult(AttackResult attackResult)
		{
			this.AttackResult = attackResult;
		}
	}
}
