using System;

namespace Barotrauma
{
	// Token: 0x0200013C RID: 316
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class LocationTypesFile : GenericPrefabFile<LocationType>
	{
		// Token: 0x06001C18 RID: 7192 RVA: 0x000CE50D File Offset: 0x000CC70D
		public LocationTypesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x000CE517 File Offset: 0x000CC717
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x000CE523 File Offset: 0x000CC723
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "locationtypes";
		}

		// Token: 0x1700082B RID: 2091
		// (get) Token: 0x06001C1B RID: 7195 RVA: 0x000CE531 File Offset: 0x000CC731
		protected override PrefabCollection<LocationType> Prefabs
		{
			get
			{
				return LocationType.Prefabs;
			}
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x000CE538 File Offset: 0x000CC738
		protected override LocationType CreatePrefab(ContentXElement element)
		{
			return new LocationType(element, this);
		}
	}
}
