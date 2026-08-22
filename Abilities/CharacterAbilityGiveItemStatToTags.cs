using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x020003FE RID: 1022
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveItemStatToTags : CharacterAbility
	{
		// Token: 0x060046C6 RID: 18118 RVA: 0x0026DE18 File Offset: 0x0026C018
		public CharacterAbilityGiveItemStatToTags(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			string key = "stattype";
			ItemTalentStats itemTalentStats = ItemTalentStats.None;
			this.stat = abilityElement.GetAttributeEnum<ItemTalentStats>(key, itemTalentStats);
			this.value = abilityElement.GetAttributeFloat("value", 0f);
			this.tags = abilityElement.GetAttributeIdentifierImmutableHashSet("tags", ImmutableHashSet<Identifier>.Empty, true);
			this.stackable = abilityElement.GetAttributeBool("stackable", true);
			this.save = abilityElement.GetAttributeBool("save", false);
		}

		// Token: 0x060046C7 RID: 18119 RVA: 0x0026DE93 File Offset: 0x0026C093
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime)
			{
				this.VerifyState(true, 0f);
			}
		}

		// Token: 0x060046C8 RID: 18120 RVA: 0x0026DEA4 File Offset: 0x0026C0A4
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x060046C9 RID: 18121 RVA: 0x0026DEB0 File Offset: 0x0026C0B0
		protected override void ApplyEffect()
		{
			if (base.Character == null)
			{
				return;
			}
			foreach (Item item in Item.ItemList)
			{
				Submarine submarine = item.Submarine;
				CharacterTeamType? characterTeamType = (submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null;
				CharacterTeamType teamID = base.Character.TeamID;
				if ((characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null) && (item.HasTag(this.tags) || this.tags.Contains(item.Prefab.Identifier)))
				{
					item.StatManager.ApplyStat(this.stat, this.stackable, this.save, this.value, base.CharacterTalent);
				}
			}
		}

		// Token: 0x040024AA RID: 9386
		private readonly ItemTalentStats stat;

		// Token: 0x040024AB RID: 9387
		private readonly float value;

		// Token: 0x040024AC RID: 9388
		private readonly ImmutableHashSet<Identifier> tags;

		// Token: 0x040024AD RID: 9389
		private readonly bool stackable;

		// Token: 0x040024AE RID: 9390
		private readonly bool save;
	}
}
