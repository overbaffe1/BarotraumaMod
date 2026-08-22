using System;
using System.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000356 RID: 854
	internal class CharacterAbilityAtmosMachine : CharacterAbility
	{
		// Token: 0x060032ED RID: 13037 RVA: 0x00158058 File Offset: 0x00156258
		public CharacterAbilityAtmosMachine(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.addedValue = abilityElement.GetAttributeFloat("addedvalue", 0f);
			this.tags = abilityElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
			this.maxMultiplyCount = abilityElement.GetAttributeInt("maxmultiplycount", int.MaxValue);
		}

		// Token: 0x060032EE RID: 13038 RVA: 0x001580B0 File Offset: 0x001562B0
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

		// Token: 0x0400193D RID: 6461
		private readonly float addedValue;

		// Token: 0x0400193E RID: 6462
		private readonly Identifier[] tags;

		// Token: 0x0400193F RID: 6463
		private readonly int maxMultiplyCount;
	}
}
