using System;

namespace Barotrauma
{
	// Token: 0x0200014F RID: 335
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class StructureFile : GenericPrefabFile<StructurePrefab>
	{
		// Token: 0x06001C61 RID: 7265 RVA: 0x000CEF1D File Offset: 0x000CD11D
		public StructureFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C62 RID: 7266 RVA: 0x000CEF27 File Offset: 0x000CD127
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x000CEF33 File Offset: 0x000CD133
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "prefabs" || identifier == "structures";
		}

		// Token: 0x17000833 RID: 2099
		// (get) Token: 0x06001C64 RID: 7268 RVA: 0x000CEF51 File Offset: 0x000CD151
		protected override PrefabCollection<StructurePrefab> Prefabs
		{
			get
			{
				return StructurePrefab.Prefabs;
			}
		}

		// Token: 0x06001C65 RID: 7269 RVA: 0x000CEF58 File Offset: 0x000CD158
		protected override StructurePrefab CreatePrefab(ContentXElement element)
		{
			return new StructurePrefab(element, this);
		}
	}
}
