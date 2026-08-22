using System;

namespace Barotrauma
{
	// Token: 0x0200012F RID: 303
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CorpsesFile : GenericPrefabFile<CorpsePrefab>
	{
		// Token: 0x06001BD9 RID: 7129 RVA: 0x000CDE82 File Offset: 0x000CC082
		public CorpsesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BDA RID: 7130 RVA: 0x000CDE8C File Offset: 0x000CC08C
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "corpse";
		}

		// Token: 0x06001BDB RID: 7131 RVA: 0x000CDE9A File Offset: 0x000CC09A
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "corpses";
		}

		// Token: 0x17000823 RID: 2083
		// (get) Token: 0x06001BDC RID: 7132 RVA: 0x000CDEA8 File Offset: 0x000CC0A8
		protected override PrefabCollection<CorpsePrefab> Prefabs
		{
			get
			{
				return CorpsePrefab.Prefabs;
			}
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x000CDEAF File Offset: 0x000CC0AF
		protected override CorpsePrefab CreatePrefab(ContentXElement element)
		{
			return new CorpsePrefab(element, this);
		}
	}
}
