using System;

namespace Barotrauma
{
	// Token: 0x02000245 RID: 581
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class StructureFile : GenericPrefabFile<StructurePrefab>
	{
		// Token: 0x0600374A RID: 14154 RVA: 0x00214AE4 File Offset: 0x00212CE4
		public StructureFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600374B RID: 14155 RVA: 0x00214AEE File Offset: 0x00212CEE
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x0600374C RID: 14156 RVA: 0x00214AFA File Offset: 0x00212CFA
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "prefabs" || identifier == "structures";
		}

		// Token: 0x17000E96 RID: 3734
		// (get) Token: 0x0600374D RID: 14157 RVA: 0x00214B18 File Offset: 0x00212D18
		protected override PrefabCollection<StructurePrefab> Prefabs
		{
			get
			{
				return StructurePrefab.Prefabs;
			}
		}

		// Token: 0x0600374E RID: 14158 RVA: 0x00214B1F File Offset: 0x00212D1F
		protected override StructurePrefab CreatePrefab(ContentXElement element)
		{
			return new StructurePrefab(element, this);
		}
	}
}
