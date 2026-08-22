using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000344 RID: 836
	internal class CharacterAbilityModifyAttackData : CharacterAbility
	{
		// Token: 0x060032B1 RID: 12977 RVA: 0x00156CCC File Offset: 0x00154ECC
		public CharacterAbilityModifyAttackData(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			ContentXElement afflictionElements = abilityElement.GetChildElement("afflictions");
			if (afflictionElements != null)
			{
				this.afflictions = CharacterAbilityGroup.ParseAfflictions(base.CharacterTalent, afflictionElements);
			}
			this.addedDamageMultiplier = abilityElement.GetAttributeFloat("addeddamagemultiplier", 0f);
			this.addedPenetration = abilityElement.GetAttributeFloat("addedpenetration", 0f);
			this.implode = abilityElement.GetAttributeBool("implode", false);
		}

		// Token: 0x060032B2 RID: 12978 RVA: 0x00156D4C File Offset: 0x00154F4C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilityAttackData attackData = abilityObject as AbilityAttackData;
			if (attackData != null)
			{
				if (attackData.Afflictions == null)
				{
					attackData.Afflictions = this.afflictions;
				}
				else
				{
					attackData.Afflictions.AddRange(this.afflictions);
				}
				attackData.DamageMultiplier += this.addedDamageMultiplier;
				attackData.AddedPenetration += this.addedPenetration;
				attackData.ShouldImplode = this.implode;
				return;
			}
			base.LogAbilityObjectMismatch();
		}

		// Token: 0x0400190C RID: 6412
		private readonly List<Affliction> afflictions = new List<Affliction>();

		// Token: 0x0400190D RID: 6413
		private readonly float addedDamageMultiplier;

		// Token: 0x0400190E RID: 6414
		private readonly float addedPenetration;

		// Token: 0x0400190F RID: 6415
		private readonly bool implode;
	}
}
