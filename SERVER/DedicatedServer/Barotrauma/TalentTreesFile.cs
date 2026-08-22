using System;

namespace Barotrauma
{
	// Token: 0x02000153 RID: 339
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TalentTreesFile : GenericPrefabFile<TalentTree>
	{
		// Token: 0x06001C71 RID: 7281 RVA: 0x000CF02D File Offset: 0x000CD22D
		public TalentTreesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C72 RID: 7282 RVA: 0x000CF037 File Offset: 0x000CD237
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "talenttree";
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x000CF045 File Offset: 0x000CD245
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "talenttrees";
		}

		// Token: 0x17000835 RID: 2101
		// (get) Token: 0x06001C74 RID: 7284 RVA: 0x000CF053 File Offset: 0x000CD253
		protected override PrefabCollection<TalentTree> Prefabs
		{
			get
			{
				return TalentTree.JobTalentTrees;
			}
		}

		// Token: 0x06001C75 RID: 7285 RVA: 0x000CF05A File Offset: 0x000CD25A
		protected override TalentTree CreatePrefab(ContentXElement element)
		{
			return new TalentTree(element, this);
		}
	}
}
