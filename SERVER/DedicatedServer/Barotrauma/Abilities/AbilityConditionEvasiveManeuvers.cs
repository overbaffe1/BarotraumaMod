using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F4 RID: 756
	internal class AbilityConditionEvasiveManeuvers : AbilityConditionData
	{
		// Token: 0x060031CF RID: 12751 RVA: 0x00152F4B File Offset: 0x0015114B
		public AbilityConditionEvasiveManeuvers(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031D0 RID: 12752 RVA: 0x00152F58 File Offset: 0x00151158
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
