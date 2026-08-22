using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x02000414 RID: 1044
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityRemoveRandomIngredient : CharacterAbility
	{
		// Token: 0x0600470B RID: 18187 RVA: 0x0026F64C File Offset: 0x0026D84C
		public CharacterAbilityRemoveRandomIngredient(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			ContentXElement conditionElement = abilityElement.GetChildElement("AbilityConditionItem");
			ContentXElement contentXElement = null;
			if (conditionElement != contentXElement)
			{
				this.condition = new AbilityConditionItem(base.CharacterTalent, conditionElement);
			}
		}

		// Token: 0x0600470C RID: 18188 RVA: 0x0026F68C File Offset: 0x0026D88C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			Fabricator.AbilityFabricationItemIngredients ingredients = abilityObject as Fabricator.AbilityFabricationItemIngredients;
			if (ingredients != null)
			{
				List<Item> items = ingredients.Items;
				if (items != null && items.Count > 0)
				{
					List<Item> applicableIngredients = (this.condition == null) ? ingredients.Items.ToList<Item>() : (from it in ingredients.Items
					where this.condition.MatchesItem(it.Prefab)
					select it).ToList<Item>();
					if (applicableIngredients.None(null))
					{
						return;
					}
					ingredients.Items.Remove(applicableIngredients.GetRandom(Rand.RandSync.Unsynced));
					return;
				}
			}
		}

		// Token: 0x040024EB RID: 9451
		[Nullable(2)]
		private readonly AbilityConditionItem condition;
	}
}
