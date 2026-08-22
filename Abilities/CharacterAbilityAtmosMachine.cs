using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x0200041C RID: 1052
	internal class CharacterAbilityAtmosMachine : CharacterAbility
	{
		// Token: 0x06004727 RID: 18215 RVA: 0x0026FED8 File Offset: 0x0026E0D8
		public CharacterAbilityAtmosMachine(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.addedValue = abilityElement.GetAttributeFloat("addedvalue", 0f);
			this.tags = abilityElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
			this.maxMultiplyCount = abilityElement.GetAttributeInt("maxmultiplycount", int.MaxValue);
		}

		// Token: 0x06004728 RID: 18216 RVA: 0x0026FF30 File Offset: 0x0026E130
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityValue abilityValue = abilityObject as IAbilityValue;
			if (abilityValue != null)
			{
				int multiplyCount = 0;
				foreach (Item item in Item.ItemList)
				{
					if (item.Prefab.Tags.Any((Identifier t) => this.tags.Contains(t)))
					{
						multiplyCount++;
						if (multiplyCount == this.maxMultiplyCount)
						{
							break;
						}
					}
				}
				abilityValue.Value += this.addedValue * (float)multiplyCount;
			}
		}

		// Token: 0x040024FE RID: 9470
		private readonly float addedValue;

		// Token: 0x040024FF RID: 9471
		private readonly Identifier[] tags;

		// Token: 0x04002500 RID: 9472
		private readonly int maxMultiplyCount;
	}
}
