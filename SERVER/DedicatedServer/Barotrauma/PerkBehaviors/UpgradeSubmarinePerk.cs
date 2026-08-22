using System;
using System.Collections.Generic;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020002EB RID: 747
	internal class UpgradeSubmarinePerk : PerkBase
	{
		// Token: 0x17000E14 RID: 3604
		// (get) Token: 0x060031AA RID: 12714 RVA: 0x00152390 File Offset: 0x00150590
		// (set) Token: 0x060031AB RID: 12715 RVA: 0x00152398 File Offset: 0x00150598
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier UpgradeIdentifier { get; set; }

		// Token: 0x17000E15 RID: 3605
		// (get) Token: 0x060031AC RID: 12716 RVA: 0x001523A1 File Offset: 0x001505A1
		// (set) Token: 0x060031AD RID: 12717 RVA: 0x001523A9 File Offset: 0x001505A9
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier CategoryIdentifier { get; set; }

		// Token: 0x17000E16 RID: 3606
		// (get) Token: 0x060031AE RID: 12718 RVA: 0x001523B2 File Offset: 0x001505B2
		// (set) Token: 0x060031AF RID: 12719 RVA: 0x001523BA File Offset: 0x001505BA
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int Level { get; set; }

		// Token: 0x17000E17 RID: 3607
		// (get) Token: 0x060031B0 RID: 12720 RVA: 0x001523C3 File Offset: 0x001505C3
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerAndClients;
			}
		}

		// Token: 0x060031B1 RID: 12721 RVA: 0x001523C6 File Offset: 0x001505C6
		public UpgradeSubmarinePerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x060031B2 RID: 12722 RVA: 0x001523D0 File Offset: 0x001505D0
		public override void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, Submarine teamSubmarine)
		{
			if (teamSubmarine == null)
			{
				return;
			}
			UpgradePrefab upgradePrefab;
			bool prefabFound = UpgradePrefab.Prefabs.TryGet(this.UpgradeIdentifier, out upgradePrefab);
			UpgradeCategory upgradeCategory;
			bool categoryFound = UpgradeCategory.Categories.TryGet(this.CategoryIdentifier, out upgradeCategory);
			if (!prefabFound)
			{
				DebugConsole.ThrowError("UpgradeSubmarinePerk: Upgrade prefab not found", null, null, false, false);
				return;
			}
			if (upgradePrefab.IsWallUpgrade)
			{
				using (List<Structure>.Enumerator enumerator = teamSubmarine.GetWalls(true).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Structure structure = enumerator.Current;
						structure.AddUpgrade(new Upgrade(structure, upgradePrefab, this.Level, null), true);
					}
					return;
				}
			}
			if (categoryFound)
			{
				using (List<Item>.Enumerator enumerator2 = teamSubmarine.GetItems(true).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item item = enumerator2.Current;
						if (upgradeCategory.CanBeApplied(item, upgradePrefab))
						{
							item.AddUpgrade(new Upgrade(item, upgradePrefab, this.Level, null), true);
						}
					}
					return;
				}
			}
			DebugConsole.ThrowError("UpgradeSubmarinePerk: Upgrade category not found", null, null, false, false);
		}
	}
}
