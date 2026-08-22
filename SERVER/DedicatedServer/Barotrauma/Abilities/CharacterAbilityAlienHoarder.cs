using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000354 RID: 852
	internal class CharacterAbilityAlienHoarder : CharacterAbility
	{
		// Token: 0x060032E9 RID: 13033 RVA: 0x00157EDC File Offset: 0x001560DC
		public CharacterAbilityAlienHoarder(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.addedDamageMultiplierPerItem = abilityElement.GetAttributeFloat("addeddamagemultiplierperitem", 0f);
			this.maxAddedDamageMultiplier = abilityElement.GetAttributeFloat("maxaddedddamagemultiplier", float.MaxValue);
			this.tags = abilityElement.GetAttributeStringArray("tags", Array.Empty<string>(), true);
		}

		// Token: 0x060032EA RID: 13034 RVA: 0x00157F34 File Offset: 0x00156134
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			AbilityAttackData attackData = abilityObject as AbilityAttackData;
			if (attackData != null)
			{
				float totalAddedDamageMultiplier = 0f;
				using (IEnumerator<Item> enumerator = base.Character.Inventory.AllItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item item = enumerator.Current;
						if (this.tags.Any((string t) => item.Prefab.Tags.Any((Identifier p) => t == p)))
						{
							totalAddedDamageMultiplier += this.addedDamageMultiplierPerItem;
						}
					}
				}
				attackData.DamageMultiplier += Math.Min(totalAddedDamageMultiplier, this.maxAddedDamageMultiplier);
				return;
			}
			base.LogAbilityObjectMismatch();
		}

		// Token: 0x04001939 RID: 6457
		private readonly float addedDamageMultiplierPerItem;

		// Token: 0x0400193A RID: 6458
		private readonly float maxAddedDamageMultiplier;

		// Token: 0x0400193B RID: 6459
		private readonly string[] tags;
	}
}
