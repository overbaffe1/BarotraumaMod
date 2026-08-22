using System;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F6 RID: 758
	[TypePreviouslyKnownAs("AbilityConditionItemInSubmarine")]
	internal class AbilityConditionInSubmarine : AbilityConditionData
	{
		// Token: 0x060031D3 RID: 12755 RVA: 0x00153044 File Offset: 0x00151244
		public AbilityConditionInSubmarine(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			if (conditionElement.GetAttribute("submarinetype") != null)
			{
				string key = "submarinetype";
				SubmarineType submarineType = SubmarineType.Player;
				this.submarineType = new SubmarineType?(conditionElement.GetAttributeEnum<SubmarineType>(key, submarineType));
			}
		}

		// Token: 0x060031D4 RID: 12756 RVA: 0x00153080 File Offset: 0x00151280
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

		// Token: 0x060031D5 RID: 12757 RVA: 0x00153108 File Offset: 0x00151308
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

		// Token: 0x0400188E RID: 6286
		private readonly SubmarineType? submarineType;
	}
}
