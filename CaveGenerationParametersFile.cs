using System;

namespace Barotrauma
{
	// Token: 0x02000220 RID: 544
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CaveGenerationParametersFile : GenericPrefabFile<CaveGenerationParams>
	{
		// Token: 0x06003698 RID: 13976 RVA: 0x002130CD File Offset: 0x002112CD
		public CaveGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003699 RID: 13977 RVA: 0x002130D7 File Offset: 0x002112D7
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "cave";
		}

		// Token: 0x0600369A RID: 13978 RVA: 0x002130E5 File Offset: 0x002112E5
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "cavegenerationparameters";
		}

		// Token: 0x17000E81 RID: 3713
		// (get) Token: 0x0600369B RID: 13979 RVA: 0x002130F3 File Offset: 0x002112F3
		protected override PrefabCollection<CaveGenerationParams> Prefabs
		{
			get
			{
				return CaveGenerationParams.CaveParams;
			}
		}

		// Token: 0x0600369C RID: 13980 RVA: 0x002130FA File Offset: 0x002112FA
		protected override CaveGenerationParams CreatePrefab(ContentXElement element)
		{
			return new CaveGenerationParams(element, this);
		}
	}
}
