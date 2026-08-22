using System;

namespace Barotrauma
{
	// Token: 0x02000232 RID: 562
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class LocationTypesFile : GenericPrefabFile<LocationType>
	{
		// Token: 0x060036F7 RID: 14071 RVA: 0x00213FA5 File Offset: 0x002121A5
		public LocationTypesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036F8 RID: 14072 RVA: 0x00213FAF File Offset: 0x002121AF
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x060036F9 RID: 14073 RVA: 0x00213FBB File Offset: 0x002121BB
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "locationtypes";
		}

		// Token: 0x17000E8C RID: 3724
		// (get) Token: 0x060036FA RID: 14074 RVA: 0x00213FC9 File Offset: 0x002121C9
		protected override PrefabCollection<LocationType> Prefabs
		{
			get
			{
				return LocationType.Prefabs;
			}
		}

		// Token: 0x060036FB RID: 14075 RVA: 0x00213FD0 File Offset: 0x002121D0
		protected override LocationType CreatePrefab(ContentXElement element)
		{
			return new LocationType(element, this);
		}
	}
}
