using System;

namespace Barotrauma
{
	// Token: 0x02000152 RID: 338
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TalentsFile : GenericPrefabFile<TalentPrefab>
	{
		// Token: 0x06001C6C RID: 7276 RVA: 0x000CEFF7 File Offset: 0x000CD1F7
		public TalentsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C6D RID: 7277 RVA: 0x000CF001 File Offset: 0x000CD201
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "talent";
		}

		// Token: 0x06001C6E RID: 7278 RVA: 0x000CF00F File Offset: 0x000CD20F
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "talents";
		}

		// Token: 0x17000834 RID: 2100
		// (get) Token: 0x06001C6F RID: 7279 RVA: 0x000CF01D File Offset: 0x000CD21D
		protected override PrefabCollection<TalentPrefab> Prefabs
		{
			get
			{
				return TalentPrefab.TalentPrefabs;
			}
		}

		// Token: 0x06001C70 RID: 7280 RVA: 0x000CF024 File Offset: 0x000CD224
		protected override TalentPrefab CreatePrefab(ContentXElement element)
		{
			return new TalentPrefab(element, this);
		}
	}
}
