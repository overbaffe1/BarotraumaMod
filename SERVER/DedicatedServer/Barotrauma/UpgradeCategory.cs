using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002B6 RID: 694
	[NullableContext(1)]
	[Nullable(0)]
	internal class UpgradeCategory : UpgradeContentPrefab
	{
		// Token: 0x06002F81 RID: 12161 RVA: 0x0013C21C File Offset: 0x0013A41C
		public UpgradeCategory(ContentXElement element, UpgradeModulesFile file) : base(element, file)
		{
			Identifier[] attributeIdentifierArray = element.GetAttributeIdentifierArray("items", Array.Empty<Identifier>(), true);
			this.selfItemTags = (((attributeIdentifierArray != null) ? attributeIdentifierArray.ToImmutableHashSet<Identifier>() : null) ?? ImmutableHashSet<Identifier>.Empty);
			this.Name = element.GetAttributeString("name", string.Empty);
			this.IsWallUpgrade = element.GetAttributeBool("wallupgrade", false);
			this.ItemTags = this.selfItemTags.CollectionConcat(this.prefabsThatAllowUpgrades);
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", Identifier.Empty);
			if (!nameIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameIdentifier);
				this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			if (this.Name.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("UpgradeCategory.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				this.Name = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
		}

		// Token: 0x06002F82 RID: 12162 RVA: 0x0013C338 File Offset: 0x0013A538
		public void DeterminePrefabsThatAllowUpgrades()
		{
			object obj = this.mutex;
			lock (obj)
			{
				this.prefabsThatAllowUpgrades.Clear();
				this.prefabsThatAllowUpgrades.UnionWith(from it in ItemPrefab.Prefabs
				where it.GetAllowedUpgrades().Contains(this.Identifier)
				select it.Identifier);
			}
		}

		// Token: 0x06002F83 RID: 12163 RVA: 0x0013C3C4 File Offset: 0x0013A5C4
		public bool CanBeApplied(MapEntity item, [Nullable(2)] UpgradePrefab upgradePrefab)
		{
			if (upgradePrefab != null)
			{
				Submarine submarine = item.Submarine;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (!upgradePrefab.IsApplicable(info))
					{
						return false;
					}
				}
			}
			bool isStructure = item is Structure;
			bool isWallUpgrade = this.IsWallUpgrade;
			if (isWallUpgrade)
			{
				return isStructure;
			}
			if (isStructure)
			{
				return false;
			}
			if (upgradePrefab != null && upgradePrefab.IsDisallowed(item))
			{
				return false;
			}
			object obj = this.mutex;
			bool result;
			lock (obj)
			{
				result = (item.Prefab.GetAllowedUpgrades().Contains(this.Identifier) || this.ItemTags.Any((Identifier tag) => item.Prefab.Tags.Contains(tag) || item.Prefab.Identifier == tag));
			}
			return result;
		}

		// Token: 0x06002F84 RID: 12164 RVA: 0x0013C4A4 File Offset: 0x0013A6A4
		[NullableContext(2)]
		public static UpgradeCategory Find(Identifier identifier)
		{
			if (identifier.IsEmpty)
			{
				return null;
			}
			return UpgradeCategory.Categories.Find((UpgradeCategory category) => category.Identifier == identifier);
		}

		// Token: 0x06002F85 RID: 12165 RVA: 0x0013C4E3 File Offset: 0x0013A6E3
		public override void Dispose()
		{
		}

		// Token: 0x040017D4 RID: 6100
		public static readonly PrefabCollection<UpgradeCategory> Categories = new PrefabCollection<UpgradeCategory>();

		// Token: 0x040017D5 RID: 6101
		private readonly ImmutableHashSet<Identifier> selfItemTags;

		// Token: 0x040017D6 RID: 6102
		private readonly HashSet<Identifier> prefabsThatAllowUpgrades = new HashSet<Identifier>();

		// Token: 0x040017D7 RID: 6103
		public readonly bool IsWallUpgrade;

		// Token: 0x040017D8 RID: 6104
		public readonly LocalizedString Name;

		// Token: 0x040017D9 RID: 6105
		private readonly object mutex = new object();

		// Token: 0x040017DA RID: 6106
		public readonly IEnumerable<Identifier> ItemTags;
	}
}
