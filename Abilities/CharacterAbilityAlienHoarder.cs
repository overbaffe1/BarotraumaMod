using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041A RID: 1050
	internal class CharacterAbilityAlienHoarder : CharacterAbility
	{
		// Token: 0x06004723 RID: 18211 RVA: 0x0026FD5C File Offset: 0x0026DF5C
		public CharacterAbilityAlienHoarder(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.addedDamageMultiplierPerItem = abilityElement.GetAttributeFloat("addeddamagemultiplierperitem", 0f);
			this.maxAddedDamageMultiplier = abilityElement.GetAttributeFloat("maxaddedddamagemultiplier", float.MaxValue);
			this.tags = abilityElement.GetAttributeStringArray("tags", Array.Empty<string>(), true);
		}

		// Token: 0x06004724 RID: 18212 RVA: 0x0026FDB4 File Offset: 0x0026DFB4
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

		// Token: 0x040024FA RID: 9466
		private readonly float addedDamageMultiplierPerItem;

		// Token: 0x040024FB RID: 9467
		private readonly float maxAddedDamageMultiplier;

		// Token: 0x040024FC RID: 9468
		private readonly string[] tags;
	}
}
