using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x020003C0 RID: 960
	internal class AbilityConditionLocation : AbilityConditionData
	{
		// Token: 0x06004617 RID: 17943 RVA: 0x0026B34C File Offset: 0x0026954C
		public AbilityConditionLocation(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			if (conditionElement.GetAttribute("hasoutpost") != null)
			{
				this.hasOutpost = new bool?(conditionElement.GetAttributeBool("hasoutpost", false));
			}
			this.locationIdentifiers = conditionElement.GetAttributeIdentifierArray("locationtype", Array.Empty<Identifier>(), true);
			this.isPositiveReputation = conditionElement.GetAttributeBool("ispositivereputation", false);
		}

		// Token: 0x06004618 RID: 17944 RVA: 0x0026B3B0 File Offset: 0x002695B0
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

		// Token: 0x04002455 RID: 9301
		private readonly bool? hasOutpost;

		// Token: 0x04002456 RID: 9302
		private readonly Identifier[] locationIdentifiers;

		// Token: 0x04002457 RID: 9303
		private readonly bool isPositiveReputation;
	}
}
