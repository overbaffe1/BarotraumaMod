using System;
using System.Collections.Generic;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000B4 RID: 180
	internal class AbilityAttackData : AbilityObject, IAbilityCharacter
	{
		// Token: 0x17000631 RID: 1585
		// (get) Token: 0x06001554 RID: 5460 RVA: 0x000B8BAC File Offset: 0x000B6DAC
		// (set) Token: 0x06001555 RID: 5461 RVA: 0x000B8BB4 File Offset: 0x000B6DB4
		public float DamageMultiplier { get; set; } = 1f;

		// Token: 0x17000632 RID: 1586
		// (get) Token: 0x06001556 RID: 5462 RVA: 0x000B8BBD File Offset: 0x000B6DBD
		// (set) Token: 0x06001557 RID: 5463 RVA: 0x000B8BC5 File Offset: 0x000B6DC5
		public float AddedPenetration { get; set; }

		// Token: 0x17000633 RID: 1587
		// (get) Token: 0x06001558 RID: 5464 RVA: 0x000B8BCE File Offset: 0x000B6DCE
		// (set) Token: 0x06001559 RID: 5465 RVA: 0x000B8BD6 File Offset: 0x000B6DD6
		public List<Affliction> Afflictions { get; set; }

		// Token: 0x17000634 RID: 1588
		// (get) Token: 0x0600155A RID: 5466 RVA: 0x000B8BDF File Offset: 0x000B6DDF
		// (set) Token: 0x0600155B RID: 5467 RVA: 0x000B8BE7 File Offset: 0x000B6DE7
		public bool ShouldImplode { get; set; }

		// Token: 0x17000635 RID: 1589
		// (get) Token: 0x0600155C RID: 5468 RVA: 0x000B8BF0 File Offset: 0x000B6DF0
		public Attack SourceAttack { get; }

		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x0600155D RID: 5469 RVA: 0x000B8BF8 File Offset: 0x000B6DF8
		// (set) Token: 0x0600155E RID: 5470 RVA: 0x000B8C00 File Offset: 0x000B6E00
		public Character Character { get; set; }

		// Token: 0x17000637 RID: 1591
		// (get) Token: 0x0600155F RID: 5471 RVA: 0x000B8C09 File Offset: 0x000B6E09
		// (set) Token: 0x06001560 RID: 5472 RVA: 0x000B8C11 File Offset: 0x000B6E11
		public Character Attacker { get; set; }

		// Token: 0x06001561 RID: 5473 RVA: 0x000B8C1C File Offset: 0x000B6E1C
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
