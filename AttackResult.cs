using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020001AE RID: 430
	internal struct AttackResult
	{
		// Token: 0x17000CCE RID: 3278
		// (get) Token: 0x060030F9 RID: 12537 RVA: 0x0020366A File Offset: 0x0020186A
		// (set) Token: 0x060030FA RID: 12538 RVA: 0x00203672 File Offset: 0x00201872
		public float Damage { readonly get; private set; }

		// Token: 0x060030FB RID: 12539 RVA: 0x0020367C File Offset: 0x0020187C
		public AttackResult(List<Affliction> afflictions, Limb hitLimb, List<DamageModifier> appliedDamageModifiers = null)
		{
			this.HitLimb = hitLimb;
			this.Afflictions = new List<Affliction>();
			foreach (Affliction affliction in afflictions)
			{
				this.Afflictions.Add(affliction.Prefab.Instantiate(affliction.Strength, affliction.Source));
			}
			this.AppliedDamageModifiers = appliedDamageModifiers;
			this.Damage = this.Afflictions.Sum((Affliction a) => a.GetVitalityDecrease(null));
		}

		// Token: 0x060030FC RID: 12540 RVA: 0x00203730 File Offset: 0x00201930
		public AttackResult(float damage, List<DamageModifier> appliedDamageModifiers = null)
		{
			this.Damage = damage;
			this.HitLimb = null;
			this.Afflictions = null;
			this.AppliedDamageModifiers = appliedDamageModifiers;
		}

		// Token: 0x0400197D RID: 6525
		public readonly List<Affliction> Afflictions;

		// Token: 0x0400197E RID: 6526
		public readonly Limb HitLimb;

		// Token: 0x0400197F RID: 6527
		public readonly List<DamageModifier> AppliedDamageModifiers;
	}
}
