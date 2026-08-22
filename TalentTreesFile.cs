using System;

namespace Barotrauma
{
	// Token: 0x02000249 RID: 585
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TalentTreesFile : GenericPrefabFile<TalentTree>
	{
		// Token: 0x0600375A RID: 14170 RVA: 0x00214BF1 File Offset: 0x00212DF1
		public TalentTreesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600375B RID: 14171 RVA: 0x00214BFB File Offset: 0x00212DFB
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "talenttree";
		}

		// Token: 0x0600375C RID: 14172 RVA: 0x00214C09 File Offset: 0x00212E09
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "talenttrees";
		}

		// Token: 0x17000E98 RID: 3736
		// (get) Token: 0x0600375D RID: 14173 RVA: 0x00214C17 File Offset: 0x00212E17
		protected override PrefabCollection<TalentTree> Prefabs
		{
			get
			{
				return TalentTree.JobTalentTrees;
			}
		}

		// Token: 0x0600375E RID: 14174 RVA: 0x00214C1E File Offset: 0x00212E1E
		protected override TalentTree CreatePrefab(ContentXElement element)
		{
			return new TalentTree(element, this);
		}
	}
}
