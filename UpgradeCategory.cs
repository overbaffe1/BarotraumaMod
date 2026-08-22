using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000382 RID: 898
	[NullableContext(1)]
	[Nullable(0)]
	internal class UpgradeCategory : UpgradeContentPrefab
	{
		// Token: 0x06004419 RID: 17433 RVA: 0x00256760 File Offset: 0x00254960
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

		// Token: 0x0600441A RID: 17434 RVA: 0x0025687C File Offset: 0x00254A7C
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

		// Token: 0x0600441B RID: 17435 RVA: 0x00256908 File Offset: 0x00254B08
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

		// Token: 0x0600441C RID: 17436 RVA: 0x002569E8 File Offset: 0x00254BE8
		[NullableContext(2)]
		public static UpgradeCategory Find(Identifier identifier)
		{
			if (identifier.IsEmpty)
			{
				return null;
			}
			return UpgradeCategory.Categories.Find((UpgradeCategory category) => category.Identifier == identifier);
		}

		// Token: 0x0600441D RID: 17437 RVA: 0x00256A27 File Offset: 0x00254C27
		public override void Dispose()
		{
		}

		// Token: 0x040023B7 RID: 9143
		public static readonly PrefabCollection<UpgradeCategory> Categories = new PrefabCollection<UpgradeCategory>();

		// Token: 0x040023B8 RID: 9144
		private readonly ImmutableHashSet<Identifier> selfItemTags;

		// Token: 0x040023B9 RID: 9145
		private readonly HashSet<Identifier> prefabsThatAllowUpgrades = new HashSet<Identifier>();

		// Token: 0x040023BA RID: 9146
		public readonly bool IsWallUpgrade;

		// Token: 0x040023BB RID: 9147
		public readonly LocalizedString Name;

		// Token: 0x040023BC RID: 9148
		private readonly object mutex = new object();

		// Token: 0x040023BD RID: 9149
		public readonly IEnumerable<Identifier> ItemTags;
	}
}
