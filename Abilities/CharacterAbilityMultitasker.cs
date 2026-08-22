using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000420 RID: 1056
	internal class CharacterAbilityMultitasker : CharacterAbility
	{
		// Token: 0x06004732 RID: 18226 RVA: 0x0027027F File Offset: 0x0026E47F
		public CharacterAbilityMultitasker(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x06004733 RID: 18227 RVA: 0x0027028C File Offset: 0x0026E48C
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

		// Token: 0x04002506 RID: 9478
		private Identifier lastSkillIdentifier;
	}
}
