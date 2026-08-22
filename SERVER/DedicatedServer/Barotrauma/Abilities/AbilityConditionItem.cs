using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F8 RID: 760
	internal class AbilityConditionItem : AbilityConditionData
	{
		// Token: 0x060031D8 RID: 12760 RVA: 0x001532B8 File Offset: 0x001514B8
		public AbilityConditionItem(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.identifiers = conditionElement.GetAttributeIdentifierArray("identifiers", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.tags = conditionElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			string key = "category";
			MapEntityCategory mapEntityCategory = MapEntityCategory.None;
			this.category = conditionElement.GetAttributeEnum<MapEntityCategory>(key, mapEntityCategory);
			if (this.identifiers.None(null) && this.tags.None(null) && this.category == MapEntityCategory.None)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent \"");
				defaultInterpolatedStringHandler.AppendFormatted<CharacterTalent>(characterTalent);
				defaultInterpolatedStringHandler.AppendLiteral("\". No identifiers, tags or category defined.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, conditionElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060031D9 RID: 12761 RVA: 0x00153394 File Offset: 0x00151594
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			ItemPrefab itemPrefab = null;
			IAbilityItemPrefab abilityItemPrefab2 = abilityObject as IAbilityItemPrefab;
			ItemPrefab abilityItemPrefab = (abilityItemPrefab2 != null) ? abilityItemPrefab2.ItemPrefab : null;
			if (abilityItemPrefab != null)
			{
				itemPrefab = abilityItemPrefab;
			}
			else
			{
				IAbilityItem abilityItem2 = abilityObject as IAbilityItem;
				Item abilityItem = (abilityItem2 != null) ? abilityItem2.Item : null;
				if (abilityItem != null)
				{
					itemPrefab = abilityItem.Prefab;
				}
			}
			if (itemPrefab != null)
			{
				return this.MatchesItem(itemPrefab);
			}
			base.LogAbilityConditionError(abilityObject, typeof(IAbilityItemPrefab));
			return false;
		}

		// Token: 0x060031DA RID: 12762 RVA: 0x001533F8 File Offset: 0x001515F8
		public bool MatchesItem(ItemPrefab itemPrefab)
		{
			return (this.category == MapEntityCategory.None || itemPrefab.Category.HasFlag(this.category)) && (!this.identifiers.Any<Identifier>() || this.identifiers.Any((Identifier t) => itemPrefab.Identifier == t)) && (!this.tags.Any<Identifier>() || this.tags.Any((Identifier t) => itemPrefab.Tags.Any((Identifier p) => t == p)));
		}

		// Token: 0x04001891 RID: 6289
		private readonly ImmutableArray<Identifier> identifiers;

		// Token: 0x04001892 RID: 6290
		private readonly ImmutableArray<Identifier> tags;

		// Token: 0x04001893 RID: 6291
		private readonly MapEntityCategory category;
	}
}
