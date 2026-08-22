using System;

namespace Barotrauma
{
	// Token: 0x02000134 RID: 308
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class FactionsFile : GenericPrefabFile<FactionPrefab>
	{
		// Token: 0x06001BED RID: 7149 RVA: 0x000CDF52 File Offset: 0x000CC152
		public FactionsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x000CDF5C File Offset: 0x000CC15C
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "faction";
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x000CDF6A File Offset: 0x000CC16A
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "factions";
		}

		// Token: 0x17000826 RID: 2086
		// (get) Token: 0x06001BF0 RID: 7152 RVA: 0x000CDF78 File Offset: 0x000CC178
		protected override PrefabCollection<FactionPrefab> Prefabs
		{
			get
			{
				return FactionPrefab.Prefabs;
			}
		}

		// Token: 0x06001BF1 RID: 7153 RVA: 0x000CDF7F File Offset: 0x000CC17F
		protected override FactionPrefab CreatePrefab(ContentXElement element)
		{
			return new FactionPrefab(element, this);
		}
	}
}
