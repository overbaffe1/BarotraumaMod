using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000343 RID: 835
	internal class CharacterAbilityModifyAffliction : CharacterAbility
	{
		// Token: 0x060032AF RID: 12975 RVA: 0x00156BD0 File Offset: 0x00154DD0
		public CharacterAbilityModifyAffliction(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.afflictionIdentifiers = abilityElement.GetAttributeIdentifierArray("afflictionidentifiers", Array.Empty<Identifier>(), true);
			this.replaceWith = abilityElement.GetAttributeIdentifier("replacewith", Identifier.Empty);
			this.addedMultiplier = abilityElement.GetAttributeFloat("addedmultiplier", 0f);
		}

		// Token: 0x060032B0 RID: 12976 RVA: 0x00156C28 File Offset: 0x00154E28
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

		// Token: 0x04001909 RID: 6409
		private readonly Identifier[] afflictionIdentifiers;

		// Token: 0x0400190A RID: 6410
		private readonly Identifier replaceWith;

		// Token: 0x0400190B RID: 6411
		private readonly float addedMultiplier;
	}
}
