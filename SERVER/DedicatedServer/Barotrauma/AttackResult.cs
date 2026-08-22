using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020000AB RID: 171
	internal struct AttackResult
	{
		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x06001498 RID: 5272 RVA: 0x000B70F8 File Offset: 0x000B52F8
		// (set) Token: 0x06001499 RID: 5273 RVA: 0x000B7100 File Offset: 0x000B5300
		public float Damage { readonly get; private set; }

		// Token: 0x0600149A RID: 5274 RVA: 0x000B710C File Offset: 0x000B530C
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

		// Token: 0x0600149B RID: 5275 RVA: 0x000B71C0 File Offset: 0x000B53C0
		public AttackResult(float damage, List<DamageModifier> appliedDamageModifiers = null)
		{
			this.Damage = damage;
			this.HitLimb = null;
			this.Afflictions = null;
			this.AppliedDamageModifiers = appliedDamageModifiers;
		}

		// Token: 0x040009CF RID: 2511
		public readonly List<Affliction> Afflictions;

		// Token: 0x040009D0 RID: 2512
		public readonly Limb HitLimb;

		// Token: 0x040009D1 RID: 2513
		public readonly List<DamageModifier> AppliedDamageModifiers;
	}
}
