using System;
using System.Collections.Generic;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001B6 RID: 438
	internal class AbilityAttackData : AbilityObject, IAbilityCharacter
	{
		// Token: 0x17000CD7 RID: 3287
		// (get) Token: 0x06003117 RID: 12567 RVA: 0x002039C0 File Offset: 0x00201BC0
		// (set) Token: 0x06003118 RID: 12568 RVA: 0x002039C8 File Offset: 0x00201BC8
		public float DamageMultiplier { get; set; } = 1f;

		// Token: 0x17000CD8 RID: 3288
		// (get) Token: 0x06003119 RID: 12569 RVA: 0x002039D1 File Offset: 0x00201BD1
		// (set) Token: 0x0600311A RID: 12570 RVA: 0x002039D9 File Offset: 0x00201BD9
		public float AddedPenetration { get; set; }

		// Token: 0x17000CD9 RID: 3289
		// (get) Token: 0x0600311B RID: 12571 RVA: 0x002039E2 File Offset: 0x00201BE2
		// (set) Token: 0x0600311C RID: 12572 RVA: 0x002039EA File Offset: 0x00201BEA
		public List<Affliction> Afflictions { get; set; }

		// Token: 0x17000CDA RID: 3290
		// (get) Token: 0x0600311D RID: 12573 RVA: 0x002039F3 File Offset: 0x00201BF3
		// (set) Token: 0x0600311E RID: 12574 RVA: 0x002039FB File Offset: 0x00201BFB
		public bool ShouldImplode { get; set; }

		// Token: 0x17000CDB RID: 3291
		// (get) Token: 0x0600311F RID: 12575 RVA: 0x00203A04 File Offset: 0x00201C04
		public Attack SourceAttack { get; }

		// Token: 0x17000CDC RID: 3292
		// (get) Token: 0x06003120 RID: 12576 RVA: 0x00203A0C File Offset: 0x00201C0C
		// (set) Token: 0x06003121 RID: 12577 RVA: 0x00203A14 File Offset: 0x00201C14
		public Character Character { get; set; }

		// Token: 0x17000CDD RID: 3293
		// (get) Token: 0x06003122 RID: 12578 RVA: 0x00203A1D File Offset: 0x00201C1D
		// (set) Token: 0x06003123 RID: 12579 RVA: 0x00203A25 File Offset: 0x00201C25
		public Character Attacker { get; set; }

		// Token: 0x06003124 RID: 12580 RVA: 0x00203A30 File Offset: 0x00201C30
		public AbilityAttackData(Attack sourceAttack, Character target, Character attacker)
		{
			this.SourceAttack = sourceAttack;
			this.Character = target;
			if (attacker != null)
			{
				this.Attacker = attacker;
				attacker.CheckTalents(AbilityEffectType.OnAttack, this);
				target.CheckTalents(AbilityEffectType.OnAttacked, this);
				this.DamageMultiplier *= 1f + attacker.GetStatValue(StatTypes.AttackMultiplier, true);
				if (attacker.TeamID == target.TeamID)
				{
					this.DamageMultiplier *= 1f + attacker.GetStatValue(StatTypes.TeamAttackMultiplier, true);
				}
			}
		}
	}
}
