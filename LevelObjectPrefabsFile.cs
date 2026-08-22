using System;

namespace Barotrauma
{
	// Token: 0x02000231 RID: 561
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class LevelObjectPrefabsFile : GenericPrefabFile<LevelObjectPrefab>
	{
		// Token: 0x060036F2 RID: 14066 RVA: 0x00213F5B File Offset: 0x0021215B
		public LevelObjectPrefabsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036F3 RID: 14067 RVA: 0x00213F65 File Offset: 0x00212165
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x060036F4 RID: 14068 RVA: 0x00213F71 File Offset: 0x00212171
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "levelobjects";
		}

		// Token: 0x17000E8B RID: 3723
		// (get) Token: 0x060036F5 RID: 14069 RVA: 0x00213F7F File Offset: 0x0021217F
		protected override PrefabCollection<LevelObjectPrefab> Prefabs
		{
			get
			{
				return LevelObjectPrefab.Prefabs;
			}
		}

		// Token: 0x060036F6 RID: 14070 RVA: 0x00213F88 File Offset: 0x00212188
		protected override LevelObjectPrefab CreatePrefab(ContentXElement element)
		{
			return new LevelObjectPrefab(element, this, default(Identifier));
		}
	}
}
