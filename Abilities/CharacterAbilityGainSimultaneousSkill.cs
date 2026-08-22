using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F9 RID: 1017
	internal class CharacterAbilityGainSimultaneousSkill : CharacterAbility
	{
		// Token: 0x060046B4 RID: 18100 RVA: 0x0026D828 File Offset: 0x0026BA28
		public CharacterAbilityGainSimultaneousSkill(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.skillIdentifier = abilityElement.GetAttributeIdentifier("skillidentifier", "");
			this.ignoreAbilitySkillGain = abilityElement.GetAttributeBool("ignoreabilityskillgain", true);
			this.targetAllies = abilityElement.GetAttributeBool("targetallies", false);
			if (this.skillIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ": skill identifier not defined.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060046B5 RID: 18101 RVA: 0x0026D8AC File Offset: 0x0026BAAC
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilitySkillGain abilitySkillGain = abilityObject as AbilitySkillGain;
			if (abilitySkillGain != null)
			{
				if (this.ignoreAbilitySkillGain && abilitySkillGain.GainedFromAbility)
				{
					return;
				}
				Identifier identifier = (this.skillIdentifier == "inherit") ? abilitySkillGain.SkillIdentifier : this.skillIdentifier;
				if (this.targetAllies)
				{
					using (IEnumerator<Character> enumerator = Character.GetFriendlyCrew(base.Character).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Character otherCharacter = enumerator.Current;
							if (otherCharacter != base.Character)
							{
								otherCharacter.Info.IncreaseSkillLevel(identifier, abilitySkillGain.Value, true, false);
							}
						}
						return;
					}
				}
				CharacterInfo info = base.Character.Info;
				if (info == null)
				{
					return;
				}
				info.IncreaseSkillLevel(identifier, abilitySkillGain.Value, true, false);
				return;
			}
			else
			{
				base.LogAbilityObjectMismatch();
			}
		}

		// Token: 0x0400249C RID: 9372
		private readonly Identifier skillIdentifier;

		// Token: 0x0400249D RID: 9373
		private readonly bool ignoreAbilitySkillGain;

		// Token: 0x0400249E RID: 9374
		private readonly bool targetAllies;
	}
}
