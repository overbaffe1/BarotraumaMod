using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000381 RID: 897
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class UpgradeContentPrefab : Prefab
	{
		// Token: 0x06004417 RID: 17431 RVA: 0x002566EB File Offset: 0x002548EB
		public UpgradeContentPrefab(ContentXElement element, UpgradeModulesFile file) : base(file, element)
		{
		}

		// Token: 0x040023B6 RID: 9142
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
