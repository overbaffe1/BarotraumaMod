using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003CD RID: 973
	internal class AbilityConditionHasAffliction : AbilityConditionDataless
	{
		// Token: 0x06004637 RID: 17975 RVA: 0x0026BB1F File Offset: 0x00269D1F
		public AbilityConditionHasAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.afflictionIdentifier = conditionElement.GetAttributeIdentifier("afflictionidentifier", Identifier.Empty);
			this.minimumPercentage = conditionElement.GetAttributeFloat("minimumpercentage", 0f);
		}

		// Token: 0x06004638 RID: 17976 RVA: 0x0026BB58 File Offset: 0x00269D58
		protected override bool MatchesConditionSpecific()
		{
			if (!this.afflictionIdentifier.IsEmpty)
			{
				Affliction affliction = this.character.CharacterHealth.GetAffliction(this.afflictionIdentifier, true);
				return affliction != null && affliction.Strength >= affliction.Prefab.ActivationThreshold && this.minimumPercentage <= affliction.Strength / affliction.Prefab.MaxStrength;
			}
			return false;
		}

		// Token: 0x04002464 RID: 9316
		private readonly Identifier afflictionIdentifier;

		// Token: 0x04002465 RID: 9317
		private readonly float minimumPercentage;
	}
}
