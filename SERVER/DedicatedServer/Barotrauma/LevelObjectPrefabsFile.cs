using System;

namespace Barotrauma
{
	// Token: 0x0200013B RID: 315
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class LevelObjectPrefabsFile : GenericPrefabFile<LevelObjectPrefab>
	{
		// Token: 0x06001C13 RID: 7187 RVA: 0x000CE4C3 File Offset: 0x000CC6C3
		public LevelObjectPrefabsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x000CE4CD File Offset: 0x000CC6CD
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x000CE4D9 File Offset: 0x000CC6D9
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "levelobjects";
		}

		// Token: 0x1700082A RID: 2090
		// (get) Token: 0x06001C16 RID: 7190 RVA: 0x000CE4E7 File Offset: 0x000CC6E7
		protected override PrefabCollection<LevelObjectPrefab> Prefabs
		{
			get
			{
				return LevelObjectPrefab.Prefabs;
			}
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x000CE4F0 File Offset: 0x000CC6F0
		protected override LevelObjectPrefab CreatePrefab(ContentXElement element)
		{
			return new LevelObjectPrefab(element, this, default(Identifier));
		}
	}
}
