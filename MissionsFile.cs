using System;

namespace Barotrauma
{
	// Token: 0x02000234 RID: 564
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class MissionsFile : GenericPrefabFile<MissionPrefab>
	{
		// Token: 0x06003700 RID: 14080 RVA: 0x00214085 File Offset: 0x00212285
		public MissionsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003701 RID: 14081 RVA: 0x0021408F File Offset: 0x0021228F
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06003702 RID: 14082 RVA: 0x0021409B File Offset: 0x0021229B
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "missions";
		}

		// Token: 0x17000E8D RID: 3725
		// (get) Token: 0x06003703 RID: 14083 RVA: 0x002140A9 File Offset: 0x002122A9
		protected override PrefabCollection<MissionPrefab> Prefabs
		{
			get
			{
				return MissionPrefab.Prefabs;
			}
		}

		// Token: 0x06003704 RID: 14084 RVA: 0x002140B0 File Offset: 0x002122B0
		protected override MissionPrefab CreatePrefab(ContentXElement element)
		{
			return new MissionPrefab(element, this);
		}
	}
}
