using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000307 RID: 775
	internal class AbilityConditionHasAffliction : AbilityConditionDataless
	{
		// Token: 0x060031FD RID: 12797 RVA: 0x00153C9F File Offset: 0x00151E9F
		public AbilityConditionHasAffliction(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.afflictionIdentifier = conditionElement.GetAttributeIdentifier("afflictionidentifier", Identifier.Empty);
			this.minimumPercentage = conditionElement.GetAttributeFloat("minimumpercentage", 0f);
		}

		// Token: 0x060031FE RID: 12798 RVA: 0x00153CD8 File Offset: 0x00151ED8
		protected override bool MatchesConditionSpecific()
		{
			if (!this.afflictionIdentifier.IsEmpty)
			{
				Affliction affliction = this.character.CharacterHealth.GetAffliction(this.afflictionIdentifier, true);
				return affliction != null && affliction.Strength >= affliction.Prefab.ActivationThreshold && this.minimumPercentage <= affliction.Strength / affliction.Prefab.MaxStrength;
			}
			return false;
		}

		// Token: 0x040018A3 RID: 6307
		private readonly Identifier afflictionIdentifier;

		// Token: 0x040018A4 RID: 6308
		private readonly float minimumPercentage;
	}
}
