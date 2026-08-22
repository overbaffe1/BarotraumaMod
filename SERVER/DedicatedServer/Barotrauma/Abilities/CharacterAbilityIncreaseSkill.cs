using System;
using System.Collections.Generic;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x02000341 RID: 833
	internal class CharacterAbilityIncreaseSkill : CharacterAbility
	{
		// Token: 0x17000E35 RID: 3637
		// (get) Token: 0x060032A8 RID: 12968 RVA: 0x00156978 File Offset: 0x00154B78
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032A9 RID: 12969 RVA: 0x0015697C File Offset: 0x00154B7C
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

		// Token: 0x060032AA RID: 12970 RVA: 0x00156A2E File Offset: 0x00154C2E
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific(base.Character);
		}

		// Token: 0x060032AB RID: 12971 RVA: 0x00156A3C File Offset: 0x00154C3C
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

		// Token: 0x060032AC RID: 12972 RVA: 0x00156A74 File Offset: 0x00154C74
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

		// Token: 0x04001906 RID: 6406
		private readonly Identifier skillIdentifier;

		// Token: 0x04001907 RID: 6407
		private readonly float skillIncrease;
	}
}
