using System;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B5 RID: 181
	internal class AbilityAttackResult : AbilityObject, IAbilityAttackResult
	{
		// Token: 0x17000638 RID: 1592
		// (get) Token: 0x06001562 RID: 5474 RVA: 0x000B8CA8 File Offset: 0x000B6EA8
		// (set) Token: 0x06001563 RID: 5475 RVA: 0x000B8CB0 File Offset: 0x000B6EB0
		public AttackResult AttackResult { get; set; }

		// Token: 0x06001564 RID: 5476 RVA: 0x000B8CB9 File Offset: 0x000B6EB9
		public AbilityAttackResult(AttackResult attackResult)
		{
			this.AttackResult = attackResult;
		}
	}
}
