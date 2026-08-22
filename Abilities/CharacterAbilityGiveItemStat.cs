using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FD RID: 1021
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveItemStat : CharacterAbility
	{
		// Token: 0x060046C3 RID: 18115 RVA: 0x0026DD60 File Offset: 0x0026BF60
		public CharacterAbilityGiveItemStat(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			string key = "stattype";
			ItemTalentStats itemTalentStats = ItemTalentStats.None;
			this.stat = abilityElement.GetAttributeEnum<ItemTalentStats>(key, itemTalentStats);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
			this.stackable = abilityElement.GetAttributeBool("stackable", true);
			this.save = abilityElement.GetAttributeBool("save", false);
		}

		// Token: 0x060046C4 RID: 18116 RVA: 0x0026DDC4 File Offset: 0x0026BFC4
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x060046C5 RID: 18117 RVA: 0x0026DDD0 File Offset: 0x0026BFD0
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			IAbilityItem ability = abilityObject as IAbilityItem;
			if (ability == null)
			{
				return;
			}
			ability.Item.StatManager.ApplyStat(this.stat, this.stackable, this.save, this.value, base.CharacterTalent);
		}

		// Token: 0x040024A6 RID: 9382
		private readonly ItemTalentStats stat;

		// Token: 0x040024A7 RID: 9383
		private readonly float value;

		// Token: 0x040024A8 RID: 9384
		private readonly bool stackable;

		// Token: 0x040024A9 RID: 9385
		private readonly bool save;
	}
}
