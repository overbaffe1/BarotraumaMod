using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000333 RID: 819
	internal class CharacterAbilityGainSimultaneousSkill : CharacterAbility
	{
		// Token: 0x0600327A RID: 12922 RVA: 0x001559A8 File Offset: 0x00153BA8
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

		// Token: 0x0600327B RID: 12923 RVA: 0x00155A2C File Offset: 0x00153C2C
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

		// Token: 0x040018DB RID: 6363
		private readonly Identifier skillIdentifier;

		// Token: 0x040018DC RID: 6364
		private readonly bool ignoreAbilitySkillGain;

		// Token: 0x040018DD RID: 6365
		private readonly bool targetAllies;
	}
}
