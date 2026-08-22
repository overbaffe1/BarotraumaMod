using System;

namespace Barotrauma
{
	// Token: 0x0200012A RID: 298
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CaveGenerationParametersFile : GenericPrefabFile<CaveGenerationParams>
	{
		// Token: 0x06001BBA RID: 7098 RVA: 0x000CD94A File Offset: 0x000CBB4A
		public CaveGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x000CD954 File Offset: 0x000CBB54
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "cave";
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x000CD962 File Offset: 0x000CBB62
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "cavegenerationparameters";
		}

		// Token: 0x17000820 RID: 2080
		// (get) Token: 0x06001BBD RID: 7101 RVA: 0x000CD970 File Offset: 0x000CBB70
		protected override PrefabCollection<CaveGenerationParams> Prefabs
		{
			get
			{
				return CaveGenerationParams.CaveParams;
			}
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x000CD977 File Offset: 0x000CBB77
		protected override CaveGenerationParams CreatePrefab(ContentXElement element)
		{
			return new CaveGenerationParams(element, this);
		}
	}
}
