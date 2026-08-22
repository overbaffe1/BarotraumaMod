using System;

namespace Barotrauma
{
	// Token: 0x0200023A RID: 570
	[RequiredByCorePackage(new Type[]
	{
		typeof(OutpostFile)
	})]
	internal sealed class OutpostConfigFile : GenericPrefabFile<OutpostGenerationParams>
	{
		// Token: 0x0600371C RID: 14108 RVA: 0x00214495 File Offset: 0x00212695
		public OutpostConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600371D RID: 14109 RVA: 0x0021449F File Offset: 0x0021269F
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "OutpostConfig";
		}

		// Token: 0x0600371E RID: 14110 RVA: 0x002144AD File Offset: 0x002126AD
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "OutpostGenerationParameters";
		}

		// Token: 0x17000E90 RID: 3728
		// (get) Token: 0x0600371F RID: 14111 RVA: 0x002144BB File Offset: 0x002126BB
		protected override PrefabCollection<OutpostGenerationParams> Prefabs
		{
			get
			{
				return OutpostGenerationParams.OutpostParams;
			}
		}

		// Token: 0x06003720 RID: 14112 RVA: 0x002144C2 File Offset: 0x002126C2
		protected override OutpostGenerationParams CreatePrefab(ContentXElement element)
		{
			return new OutpostGenerationParams(element, this);
		}
	}
}
