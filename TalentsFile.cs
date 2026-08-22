using System;

namespace Barotrauma
{
	// Token: 0x02000248 RID: 584
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TalentsFile : GenericPrefabFile<TalentPrefab>
	{
		// Token: 0x06003755 RID: 14165 RVA: 0x00214BBB File Offset: 0x00212DBB
		public TalentsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003756 RID: 14166 RVA: 0x00214BC5 File Offset: 0x00212DC5
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "talent";
		}

		// Token: 0x06003757 RID: 14167 RVA: 0x00214BD3 File Offset: 0x00212DD3
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "talents";
		}

		// Token: 0x17000E97 RID: 3735
		// (get) Token: 0x06003758 RID: 14168 RVA: 0x00214BE1 File Offset: 0x00212DE1
		protected override PrefabCollection<TalentPrefab> Prefabs
		{
			get
			{
				return TalentPrefab.TalentPrefabs;
			}
		}

		// Token: 0x06003759 RID: 14169 RVA: 0x00214BE8 File Offset: 0x00212DE8
		protected override TalentPrefab CreatePrefab(ContentXElement element)
		{
			return new TalentPrefab(element, this);
		}
	}
}
