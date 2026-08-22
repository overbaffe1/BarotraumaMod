using System;
using System.Collections.Generic;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x02000407 RID: 1031
	internal class CharacterAbilityIncreaseSkill : CharacterAbility
	{
		// Token: 0x17001225 RID: 4645
		// (get) Token: 0x060046E2 RID: 18146 RVA: 0x0026E7F8 File Offset: 0x0026C9F8
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046E3 RID: 18147 RVA: 0x0026E7FC File Offset: 0x0026C9FC
		public CharacterAbilityIncreaseSkill(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.skillIdentifier = abilityElement.GetAttributeIdentifier("skillidentifier", "");
			this.skillIncrease = abilityElement.GetAttributeFloat("skillincrease", 0f);
			if (this.skillIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent \"" + characterAbilityGroup.CharacterTalent.DebugIdentifier + "\" - skill identifier not defined in CharacterAbilityIncreaseSkill.", null, abilityElement.ContentPackage, false, false);
			}
			if (MathUtils.NearlyEqual(this.skillIncrease, 0f, 0.0001f))
			{
				DebugConsole.AddWarning("Possible error in talent \"" + characterAbilityGroup.CharacterTalent.DebugIdentifier + "\" - skill increase set to 0.", abilityElement.ContentPackage);
			}
		}

		// Token: 0x060046E4 RID: 18148 RVA: 0x0026E8AE File Offset: 0x0026CAAE
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x060046E5 RID: 18149 RVA: 0x0026E8BC File Offset: 0x0026CABC
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
			Character character = (abilityCharacter != null) ? abilityCharacter.Character : null;
			if (character != null)
			{
				this.ApplyEffectSpecific(character);
				return;
			}
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x060046E6 RID: 18150 RVA: 0x0026E8F4 File Offset: 0x0026CAF4
		private void ApplyEffectSpecific(Character character)
		{
			if (this.skillIdentifier == "random")
			{
				CharacterInfo info = character.Info;
				Skill skill2;
				if (info == null)
				{
					skill2 = null;
				}
				else
				{
					Job job = info.Job;
					if (job == null)
					{
						skill2 = null;
					}
					else
					{
						IEnumerable<Skill> skills = job.GetSkills();
						skill2 = ((skills != null) ? skills.GetRandomUnsynced<Skill>() : null);
					}
				}
				Skill skill = skill2;
				if (skill == null)
				{
					return;
				}
				CharacterInfo info2 = character.Info;
				if (info2 == null)
				{
					return;
				}
				info2.IncreaseSkillLevel(skill.Identifier, this.skillIncrease, true, false);
				return;
			}
			else
			{
				CharacterInfo info3 = character.Info;
				if (info3 == null)
				{
					return;
				}
				info3.IncreaseSkillLevel(this.skillIdentifier, this.skillIncrease, true, false);
				return;
			}
		}

		// Token: 0x040024C7 RID: 9415
		private readonly Identifier skillIdentifier;

		// Token: 0x040024C8 RID: 9416
		private readonly float skillIncrease;
	}
}
