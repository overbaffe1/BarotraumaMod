using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BC RID: 956
	[TypePreviouslyKnownAs("AbilityConditionItemInSubmarine")]
	internal class AbilityConditionInSubmarine : AbilityConditionData
	{
		// Token: 0x0600460D RID: 17933 RVA: 0x0026AEC4 File Offset: 0x002690C4
		public AbilityConditionInSubmarine(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			if (conditionElement.GetAttribute("submarinetype") != null)
			{
				string key = "submarinetype";
				SubmarineType submarineType = SubmarineType.Player;
				this.submarineType = new SubmarineType?(conditionElement.GetAttributeEnum<SubmarineType>(key, submarineType));
			}
		}

		// Token: 0x0600460E RID: 17934 RVA: 0x0026AF00 File Offset: 0x00269100
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			IAbilityItem abilityItem = abilityObject as IAbilityItem;
			Item item = (abilityItem != null) ? abilityItem.Item : null;
			if (item == null)
			{
				return this.MatchesCondition();
			}
			if (item.Submarine == null)
			{
				return false;
			}
			if (this.submarineType != null)
			{
				SubmarineInfo info = item.Submarine.Info;
				SubmarineType? submarineType = (info != null) ? new SubmarineType?(info.Type) : null;
				SubmarineType value = this.submarineType.Value;
				return submarineType.GetValueOrDefault() == value & submarineType != null;
			}
			return true;
		}

		// Token: 0x0600460F RID: 17935 RVA: 0x0026AF88 File Offset: 0x00269188
		public override bool MatchesCondition()
		{
			if (this.character.Submarine == null)
			{
				return false;
			}
			Submarine submarine = this.character.Submarine;
			SubmarineType? submarineType;
			if (submarine == null)
			{
				submarineType = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				submarineType = ((info != null) ? new SubmarineType?(info.Type) : null);
			}
			SubmarineType? submarineType2 = submarineType;
			SubmarineType? submarineType3 = this.submarineType;
			return submarineType2.GetValueOrDefault() == submarineType3.GetValueOrDefault() & submarineType2 != null == (submarineType3 != null);
		}

		// Token: 0x0400244F RID: 9295
		private readonly SubmarineType? submarineType;
	}
}
