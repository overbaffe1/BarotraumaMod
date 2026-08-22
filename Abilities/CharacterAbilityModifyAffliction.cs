using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000409 RID: 1033
	internal class CharacterAbilityModifyAffliction : CharacterAbility
	{
		// Token: 0x060046E9 RID: 18153 RVA: 0x0026EA50 File Offset: 0x0026CC50
		public CharacterAbilityModifyAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionIdentifiers = abilityElement.GetAttributeIdentifierArray("afflictionidentifiers", Array.Empty<Identifier>(), true);
			this.replaceWith = abilityElement.GetAttributeIdentifier("replacewith", Identifier.Empty);
			this.addedMultiplier = abilityElement.GetAttributeFloat("addedmultiplier", 0f);
		}

		// Token: 0x060046EA RID: 18154 RVA: 0x0026EAA8 File Offset: 0x0026CCA8
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityAffliction abilityAffliction = abilityObject as IAbilityAffliction;
			Affliction affliction = (abilityAffliction != null) ? abilityAffliction.Affliction : null;
			if (affliction != null)
			{
				foreach (Identifier afflictionIdentifier in this.afflictionIdentifiers)
				{
					Identifier identifier = affliction.Identifier;
					if (!(identifier != afflictionIdentifier))
					{
						AfflictionPrefab afflictionPrefab = affliction.Prefab;
						if (!this.replaceWith.IsEmpty)
						{
							AfflictionPrefab.Prefabs.TryGet(this.replaceWith, out afflictionPrefab);
						}
						abilityAffliction.Affliction = new Affliction(afflictionPrefab, affliction.Strength * (1f + this.addedMultiplier));
					}
				}
				return;
			}
			base.LogAbilityObjectMismatch();
		}

		// Token: 0x040024CA RID: 9418
		private readonly Identifier[] afflictionIdentifiers;

		// Token: 0x040024CB RID: 9419
		private readonly Identifier replaceWith;

		// Token: 0x040024CC RID: 9420
		private readonly float addedMultiplier;
	}
}
