using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034E RID: 846
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityRemoveRandomIngredient : CharacterAbility
	{
		// Token: 0x060032D1 RID: 13009 RVA: 0x001577CC File Offset: 0x001559CC
		public CharacterAbilityRemoveRandomIngredient(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			ContentXElement conditionElement = abilityElement.GetChildElement("AbilityConditionItem");
			ContentXElement contentXElement = null;
			if (conditionElement != contentXElement)
			{
				this.condition = new AbilityConditionItem(base.CharacterTalent, conditionElement);
			}
		}

		// Token: 0x060032D2 RID: 13010 RVA: 0x0015780C File Offset: 0x00155A0C
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

		// Token: 0x0400192A RID: 6442
		[Nullable(2)]
		private readonly AbilityConditionItem condition;
	}
}
