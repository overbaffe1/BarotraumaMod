using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000337 RID: 823
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveItemStat : CharacterAbility
	{
		// Token: 0x06003289 RID: 12937 RVA: 0x00155EE0 File Offset: 0x001540E0
		public CharacterAbilityGiveItemStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			string key = "stattype";
			ItemTalentStats itemTalentStats = ItemTalentStats.None;
			this.stat = abilityElement.GetAttributeEnum<ItemTalentStats>(key, itemTalentStats);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
			this.stackable = abilityElement.GetAttributeBool("stackable", true);
			this.save = abilityElement.GetAttributeBool("save", false);
		}

		// Token: 0x0600328A RID: 12938 RVA: 0x00155F44 File Offset: 0x00154144
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x0600328B RID: 12939 RVA: 0x00155F50 File Offset: 0x00154150
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityItem ability = abilityObject as IAbilityItem;
			if (ability == null)
			{
				return;
			}
			ability.Item.StatManager.ApplyStat(this.stat, this.stackable, this.save, this.value, base.CharacterTalent);
		}

		// Token: 0x040018E5 RID: 6373
		private readonly ItemTalentStats stat;

		// Token: 0x040018E6 RID: 6374
		private readonly float value;

		// Token: 0x040018E7 RID: 6375
		private readonly bool stackable;

		// Token: 0x040018E8 RID: 6376
		private readonly bool save;
	}
}
