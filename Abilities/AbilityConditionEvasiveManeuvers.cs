using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BA RID: 954
	internal class AbilityConditionEvasiveManeuvers : AbilityConditionData
	{
		// Token: 0x06004609 RID: 17929 RVA: 0x0026ADCB File Offset: 0x00268FCB
		public AbilityConditionEvasiveManeuvers(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x0600460A RID: 17930 RVA: 0x0026ADD8 File Offset: 0x00268FD8
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilitySubmarine abilitySubmarine = abilityObject as IAbilitySubmarine;
			Submarine submarine = (abilitySubmarine != null) ? abilitySubmarine.Submarine : null;
			if (submarine != null)
			{
				IAbilityCharacter abilityCharacter = abilityObject as IAbilityCharacter;
				Character attackingCharacter = (abilityCharacter != null) ? abilityCharacter.Character : null;
				if (attackingCharacter != null)
				{
					return submarine.TeamID == this.character.TeamID && this.character.Submarine == submarine && attackingCharacter.TeamID != this.character.TeamID;
				}
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilitySubmarine));
			return false;
		}
	}
}
