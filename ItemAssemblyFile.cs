using System;

namespace Barotrauma
{
	// Token: 0x0200022D RID: 557
	[NotSyncedInMultiplayer]
	internal sealed class ItemAssemblyFile : GenericPrefabFile<ItemAssemblyPrefab>
	{
		// Token: 0x060036DC RID: 14044 RVA: 0x00213C13 File Offset: 0x00211E13
		public ItemAssemblyFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036DD RID: 14045 RVA: 0x00213C1D File Offset: 0x00211E1D
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "itemassembly";
		}

		// Token: 0x060036DE RID: 14046 RVA: 0x00213C2B File Offset: 0x00211E2B
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "itemassemblies";
		}

		// Token: 0x17000E89 RID: 3721
		// (get) Token: 0x060036DF RID: 14047 RVA: 0x00213C39 File Offset: 0x00211E39
		protected override PrefabCollection<ItemAssemblyPrefab> Prefabs
		{
			get
			{
				return ItemAssemblyPrefab.Prefabs;
			}
		}

		// Token: 0x060036E0 RID: 14048 RVA: 0x00213C40 File Offset: 0x00211E40
		protected override ItemAssemblyPrefab CreatePrefab(ContentXElement element)
		{
			return new ItemAssemblyPrefab(element, this);
		}
	}
}
