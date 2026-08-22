using System;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F9 RID: 761
	internal class AbilityConditionItemIsStatic : AbilityConditionData
	{
		// Token: 0x060031DB RID: 12763 RVA: 0x0015348C File Offset: 0x0015168C
		public AbilityConditionItemIsStatic(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
		}

		// Token: 0x060031DC RID: 12764 RVA: 0x00153498 File Offset: 0x00151698
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
