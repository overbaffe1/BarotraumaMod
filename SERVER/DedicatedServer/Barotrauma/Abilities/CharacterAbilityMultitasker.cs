using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200035A RID: 858
	internal class CharacterAbilityMultitasker : CharacterAbility
	{
		// Token: 0x060032F8 RID: 13048 RVA: 0x001583FF File Offset: 0x001565FF
		public CharacterAbilityMultitasker(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x060032F9 RID: 13049 RVA: 0x0015840C File Offset: 0x0015660C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilitySkillIdentifier abilitySkillIdentifier = abilityObject as IAbilitySkillIdentifier;
			if (abilitySkillIdentifier != null)
			{
				Identifier skillIdentifier = abilitySkillIdentifier.SkillIdentifier;
				if (skillIdentifier != this.lastSkillIdentifier)
				{
					this.lastSkillIdentifier = skillIdentifier;
					CharacterInfo info = base.Character.Info;
					if (info == null)
					{
						return;
					}
					info.IncreaseSkillLevel(skillIdentifier, 1f, true, false);
				}
			}
		}

		// Token: 0x04001945 RID: 6469
		private Identifier lastSkillIdentifier;
	}
}
