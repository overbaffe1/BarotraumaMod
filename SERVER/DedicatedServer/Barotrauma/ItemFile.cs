using System;

namespace Barotrauma
{
	// Token: 0x02000138 RID: 312
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class ItemFile : GenericPrefabFile<ItemPrefab>
	{
		// Token: 0x06001C02 RID: 7170 RVA: 0x000CE1B1 File Offset: 0x000CC3B1
		public ItemFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x000CE1BB File Offset: 0x000CC3BB
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x000CE1C7 File Offset: 0x000CC3C7
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "items";
		}

		// Token: 0x17000829 RID: 2089
		// (get) Token: 0x06001C05 RID: 7173 RVA: 0x000CE1D5 File Offset: 0x000CC3D5
		protected override PrefabCollection<ItemPrefab> Prefabs
		{
			get
			{
				return ItemPrefab.Prefabs;
			}
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x000CE1DC File Offset: 0x000CC3DC
		protected override ItemPrefab CreatePrefab(ContentXElement element)
		{
			return new ItemPrefab(element, this);
		}
	}
}
