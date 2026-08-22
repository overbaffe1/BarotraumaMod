using System;

namespace Barotrauma
{
	// Token: 0x02000144 RID: 324
	[RequiredByCorePackage(new Type[]
	{
		typeof(OutpostFile)
	})]
	internal sealed class OutpostConfigFile : GenericPrefabFile<OutpostGenerationParams>
	{
		// Token: 0x06001C3D RID: 7229 RVA: 0x000CE9FD File Offset: 0x000CCBFD
		public OutpostConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x000CEA07 File Offset: 0x000CCC07
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "OutpostConfig";
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x000CEA15 File Offset: 0x000CCC15
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "OutpostGenerationParameters";
		}

		// Token: 0x1700082F RID: 2095
		// (get) Token: 0x06001C40 RID: 7232 RVA: 0x000CEA23 File Offset: 0x000CCC23
		protected override PrefabCollection<OutpostGenerationParams> Prefabs
		{
			get
			{
				return OutpostGenerationParams.OutpostParams;
			}
		}

		// Token: 0x06001C41 RID: 7233 RVA: 0x000CEA2A File Offset: 0x000CCC2A
		protected override OutpostGenerationParams CreatePrefab(ContentXElement element)
		{
			return new OutpostGenerationParams(element, this);
		}
	}
}
