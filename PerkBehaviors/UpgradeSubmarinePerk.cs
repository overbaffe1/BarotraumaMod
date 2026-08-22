using System;
using System.Collections.Generic;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020003B1 RID: 945
	internal class UpgradeSubmarinePerk : PerkBase
	{
		// Token: 0x17001204 RID: 4612
		// (get) Token: 0x060045E4 RID: 17892 RVA: 0x0026A210 File Offset: 0x00268410
		// (set) Token: 0x060045E5 RID: 17893 RVA: 0x0026A218 File Offset: 0x00268418
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier UpgradeIdentifier { get; set; }

		// Token: 0x17001205 RID: 4613
		// (get) Token: 0x060045E6 RID: 17894 RVA: 0x0026A221 File Offset: 0x00268421
		// (set) Token: 0x060045E7 RID: 17895 RVA: 0x0026A229 File Offset: 0x00268429
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier CategoryIdentifier { get; set; }

		// Token: 0x17001206 RID: 4614
		// (get) Token: 0x060045E8 RID: 17896 RVA: 0x0026A232 File Offset: 0x00268432
		// (set) Token: 0x060045E9 RID: 17897 RVA: 0x0026A23A File Offset: 0x0026843A
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int Level { get; set; }

		// Token: 0x17001207 RID: 4615
		// (get) Token: 0x060045EA RID: 17898 RVA: 0x0026A243 File Offset: 0x00268443
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerAndClients;
			}
		}

		// Token: 0x060045EB RID: 17899 RVA: 0x0026A246 File Offset: 0x00268446
		public UpgradeSubmarinePerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x060045EC RID: 17900 RVA: 0x0026A250 File Offset: 0x00268450
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
