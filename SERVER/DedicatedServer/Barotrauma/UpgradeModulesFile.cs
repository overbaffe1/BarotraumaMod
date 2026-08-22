using System;

namespace Barotrauma
{
	// Token: 0x02000157 RID: 343
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class UpgradeModulesFile : GenericPrefabFile<UpgradeContentPrefab>
	{
		// Token: 0x06001C81 RID: 7297 RVA: 0x000CF32E File Offset: 0x000CD52E
		public UpgradeModulesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x000CF338 File Offset: 0x000CD538
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "upgrademodule" || identifier == "upgradecategory";
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x000CF356 File Offset: 0x000CD556
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "upgrademodules";
		}

		// Token: 0x17000837 RID: 2103
		// (get) Token: 0x06001C84 RID: 7300 RVA: 0x000CF364 File Offset: 0x000CD564
		protected override PrefabCollection<UpgradeContentPrefab> Prefabs
		{
			get
			{
				return UpgradeContentPrefab.PrefabsAndCategories;
			}
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x000CF36C File Offset: 0x000CD56C
		protected override UpgradeContentPrefab CreatePrefab(ContentXElement element)
		{
			Identifier elemName = element.NameAsIdentifier();
			if (elemName == "upgradecategory")
			{
				return new UpgradeCategory(element, this);
			}
			return new UpgradePrefab(element, this);
		}
	}
}
