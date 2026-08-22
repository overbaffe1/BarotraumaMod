using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000338 RID: 824
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveItemStatToTags : CharacterAbility
	{
		// Token: 0x0600328C RID: 12940 RVA: 0x00155F98 File Offset: 0x00154198
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

		// Token: 0x0600328D RID: 12941 RVA: 0x00156013 File Offset: 0x00154213
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime)
			{
				this.VerifyState(true, 0f);
			}
		}

		// Token: 0x0600328E RID: 12942 RVA: 0x00156024 File Offset: 0x00154224
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched)
			{
				this.ApplyEffect();
			}
		}

		// Token: 0x0600328F RID: 12943 RVA: 0x00156030 File Offset: 0x00154230
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

		// Token: 0x040018E9 RID: 6377
		private readonly ItemTalentStats stat;

		// Token: 0x040018EA RID: 6378
		private readonly float value;

		// Token: 0x040018EB RID: 6379
		private readonly ImmutableHashSet<Identifier> tags;

		// Token: 0x040018EC RID: 6380
		private readonly bool stackable;

		// Token: 0x040018ED RID: 6381
		private readonly bool save;
	}
}
