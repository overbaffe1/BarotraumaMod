using System;

namespace Barotrauma
{
	// Token: 0x0200024D RID: 589
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class UpgradeModulesFile : GenericPrefabFile<UpgradeContentPrefab>
	{
		// Token: 0x06003772 RID: 14194 RVA: 0x0021529A File Offset: 0x0021349A
		public UpgradeModulesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003773 RID: 14195 RVA: 0x002152A4 File Offset: 0x002134A4
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "upgrademodule" || identifier == "upgradecategory";
		}

		// Token: 0x06003774 RID: 14196 RVA: 0x002152C2 File Offset: 0x002134C2
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "upgrademodules";
		}

		// Token: 0x17000E9A RID: 3738
		// (get) Token: 0x06003775 RID: 14197 RVA: 0x002152D0 File Offset: 0x002134D0
		protected override PrefabCollection<UpgradeContentPrefab> Prefabs
		{
			get
			{
				return UpgradeContentPrefab.PrefabsAndCategories;
			}
		}

		// Token: 0x06003776 RID: 14198 RVA: 0x002152D8 File Offset: 0x002134D8
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
