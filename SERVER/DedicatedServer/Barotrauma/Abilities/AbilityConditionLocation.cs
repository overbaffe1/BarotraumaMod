using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020002FA RID: 762
	internal class AbilityConditionLocation : AbilityConditionData
	{
		// Token: 0x060031DD RID: 12765 RVA: 0x001534CC File Offset: 0x001516CC
		public AbilityConditionLocation(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			if (conditionElement.GetAttribute("hasoutpost") != null)
			{
				this.hasOutpost = new bool?(conditionElement.GetAttributeBool("hasoutpost", false));
			}
			this.locationIdentifiers = conditionElement.GetAttributeIdentifierArray("locationtype", Array.Empty<Identifier>(), true);
			this.isPositiveReputation = conditionElement.GetAttributeBool("ispositivereputation", false);
		}

		// Token: 0x060031DE RID: 12766 RVA: 0x00153530 File Offset: 0x00151730
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilityLocation abilityLocation = abilityObject as IAbilityLocation;
			if (abilityLocation != null)
			{
				if (this.isPositiveReputation)
				{
					Location location = abilityLocation.Location;
					Reputation reputation = (location != null) ? location.Reputation : null;
					if (reputation == null)
					{
						return false;
					}
					if (reputation.Value <= 0f)
					{
						return false;
					}
				}
				return (!this.locationIdentifiers.Any<Identifier>() || this.locationIdentifiers.Contains(abilityLocation.Location.Type.Identifier)) && (this.hasOutpost == null || this.hasOutpost.Value == abilityLocation.Location.HasOutpost());
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityItemPrefab));
			return false;
		}

		// Token: 0x04001894 RID: 6292
		private readonly bool? hasOutpost;

		// Token: 0x04001895 RID: 6293
		private readonly Identifier[] locationIdentifiers;

		// Token: 0x04001896 RID: 6294
		private readonly bool isPositiveReputation;
	}
}
