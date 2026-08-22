using System;

namespace Barotrauma
{
	// Token: 0x0200013E RID: 318
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class MissionsFile : GenericPrefabFile<MissionPrefab>
	{
		// Token: 0x06001C21 RID: 7201 RVA: 0x000CE5ED File Offset: 0x000CC7ED
		public MissionsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x000CE5F7 File Offset: 0x000CC7F7
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x000CE603 File Offset: 0x000CC803
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "missions";
		}

		// Token: 0x1700082C RID: 2092
		// (get) Token: 0x06001C24 RID: 7204 RVA: 0x000CE611 File Offset: 0x000CC811
		protected override PrefabCollection<MissionPrefab> Prefabs
		{
			get
			{
				return MissionPrefab.Prefabs;
			}
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x000CE618 File Offset: 0x000CC818
		protected override MissionPrefab CreatePrefab(ContentXElement element)
		{
			return new MissionPrefab(element, this);
		}
	}
}
