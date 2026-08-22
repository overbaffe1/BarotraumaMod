using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040A RID: 1034
	internal class CharacterAbilityModifyAttackData : CharacterAbility
	{
		// Token: 0x060046EB RID: 18155 RVA: 0x0026EB4C File Offset: 0x0026CD4C
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

		// Token: 0x060046EC RID: 18156 RVA: 0x0026EBCC File Offset: 0x0026CDCC
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

		// Token: 0x040024CD RID: 9421
		private readonly List<Affliction> afflictions = new List<Affliction>();

		// Token: 0x040024CE RID: 9422
		private readonly float addedDamageMultiplier;

		// Token: 0x040024CF RID: 9423
		private readonly float addedPenetration;

		// Token: 0x040024D0 RID: 9424
		private readonly bool implode;
	}
}
