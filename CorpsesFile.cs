using System;

namespace Barotrauma
{
	// Token: 0x02000225 RID: 549
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CorpsesFile : GenericPrefabFile<CorpsePrefab>
	{
		// Token: 0x060036B8 RID: 14008 RVA: 0x0021391A File Offset: 0x00211B1A
		public CorpsesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036B9 RID: 14009 RVA: 0x00213924 File Offset: 0x00211B24
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "corpse";
		}

		// Token: 0x060036BA RID: 14010 RVA: 0x00213932 File Offset: 0x00211B32
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "corpses";
		}

		// Token: 0x17000E84 RID: 3716
		// (get) Token: 0x060036BB RID: 14011 RVA: 0x00213940 File Offset: 0x00211B40
		protected override PrefabCollection<CorpsePrefab> Prefabs
		{
			get
			{
				return CorpsePrefab.Prefabs;
			}
		}

		// Token: 0x060036BC RID: 14012 RVA: 0x00213947 File Offset: 0x00211B47
		protected override CorpsePrefab CreatePrefab(ContentXElement element)
		{
			return new CorpsePrefab(element, this);
		}
	}
}
