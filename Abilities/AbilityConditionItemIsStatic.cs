using System;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BF RID: 959
	internal class AbilityConditionItemIsStatic : AbilityConditionData
	{
		// Token: 0x06004615 RID: 17941 RVA: 0x0026B30C File Offset: 0x0026950C
		public AbilityConditionItemIsStatic(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x06004616 RID: 17942 RVA: 0x0026B318 File Offset: 0x00269518
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilityItem abilityItem = abilityObject as IAbilityItem;
			if (abilityItem != null)
			{
				Item item = abilityItem.Item;
				return item.GetComponent<Holdable>() == null && item.GetComponent<Wearable>() == null;
			}
			return false;
		}
	}
}
