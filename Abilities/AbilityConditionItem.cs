using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BE RID: 958
	internal class AbilityConditionItem : AbilityConditionData
	{
		// Token: 0x06004612 RID: 17938 RVA: 0x0026B138 File Offset: 0x00269338
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

		// Token: 0x06004613 RID: 17939 RVA: 0x0026B214 File Offset: 0x00269414
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

		// Token: 0x06004614 RID: 17940 RVA: 0x0026B278 File Offset: 0x00269478
		public bool MatchesItem(ItemPrefab itemPrefab)
		{
			return (this.category == MapEntityCategory.None || itemPrefab.Category.HasFlag(this.category)) && (!this.identifiers.Any<Identifier>() || this.identifiers.Any((Identifier t) => itemPrefab.Identifier == t)) && (!this.tags.Any<Identifier>() || this.tags.Any((Identifier t) => itemPrefab.Tags.Any((Identifier p) => t == p)));
		}

		// Token: 0x04002452 RID: 9298
		private readonly ImmutableArray<Identifier> identifiers;

		// Token: 0x04002453 RID: 9299
		private readonly ImmutableArray<Identifier> tags;

		// Token: 0x04002454 RID: 9300
		private readonly MapEntityCategory category;
	}
}
