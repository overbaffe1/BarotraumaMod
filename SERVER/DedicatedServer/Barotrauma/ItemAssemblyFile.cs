using System;

namespace Barotrauma
{
	// Token: 0x02000137 RID: 311
	[NotSyncedInMultiplayer]
	internal sealed class ItemAssemblyFile : GenericPrefabFile<ItemAssemblyPrefab>
	{
		// Token: 0x06001BFD RID: 7165 RVA: 0x000CE17B File Offset: 0x000CC37B
		public ItemAssemblyFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x000CE185 File Offset: 0x000CC385
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "itemassembly";
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x000CE193 File Offset: 0x000CC393
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "itemassemblies";
		}

		// Token: 0x17000828 RID: 2088
		// (get) Token: 0x06001C00 RID: 7168 RVA: 0x000CE1A1 File Offset: 0x000CC3A1
		protected override PrefabCollection<ItemAssemblyPrefab> Prefabs
		{
			get
			{
				return ItemAssemblyPrefab.Prefabs;
			}
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x000CE1A8 File Offset: 0x000CC3A8
		protected override ItemAssemblyPrefab CreatePrefab(ContentXElement element)
		{
			return new ItemAssemblyPrefab(element, this);
		}
	}
}
