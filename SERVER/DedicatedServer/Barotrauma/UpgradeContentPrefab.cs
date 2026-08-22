using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002B5 RID: 693
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class UpgradeContentPrefab : Prefab
	{
		// Token: 0x06002F7F RID: 12159 RVA: 0x0013C1A7 File Offset: 0x0013A3A7
		public UpgradeContentPrefab(ContentXElement element, UpgradeModulesFile file) : base(file, element)
		{
		}

		// Token: 0x040017D3 RID: 6099
		public static readonly PrefabCollection<UpgradeContentPrefab> PrefabsAndCategories = new PrefabCollection<UpgradeContentPrefab>(delegate(UpgradeContentPrefab prefab, bool isOverride)
		{
			UpgradePrefab upgradePrefab = prefab as UpgradePrefab;
			if (upgradePrefab != null)
			{
				UpgradePrefab.Prefabs.Add(upgradePrefab, isOverride);
				return;
			}
			UpgradeCategory upgradeCategory = prefab as UpgradeCategory;
			if (upgradeCategory != null)
			{
				UpgradeCategory.Categories.Add(upgradeCategory, isOverride);
			}
		}, delegate(UpgradeContentPrefab prefab)
		{
			UpgradePrefab upgradePrefab = prefab as UpgradePrefab;
			if (upgradePrefab != null)
			{
				UpgradePrefab.Prefabs.Remove(upgradePrefab);
				return;
			}
			UpgradeCategory upgradeCategory = prefab as UpgradeCategory;
			if (upgradeCategory != null)
			{
				UpgradeCategory.Categories.Remove(upgradeCategory);
			}
		}, delegate()
		{
			UpgradePrefab.Prefabs.SortAll();
			UpgradeCategory.Categories.SortAll();
		}, delegate(ContentFile file)
		{
			UpgradePrefab.Prefabs.AddOverrideFile(file);
			UpgradeCategory.Categories.AddOverrideFile(file);
		}, delegate(ContentFile file)
		{
			UpgradePrefab.Prefabs.RemoveOverrideFile(file);
			UpgradeCategory.Categories.RemoveOverrideFile(file);
		});
	}
}
