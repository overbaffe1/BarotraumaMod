using System;
using Barotrauma.RuinGeneration;

namespace Barotrauma
{
	// Token: 0x0200023F RID: 575
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class RuinConfigFile : GenericPrefabFile<RuinGenerationParams>
	{
		// Token: 0x0600372E RID: 14126 RVA: 0x0021484D File Offset: 0x00212A4D
		public RuinConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600372F RID: 14127 RVA: 0x00214857 File Offset: 0x00212A57
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "RuinConfig";
		}

		// Token: 0x06003730 RID: 14128 RVA: 0x00214865 File Offset: 0x00212A65
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "RuinGenerationParameters";
		}

		// Token: 0x17000E92 RID: 3730
		// (get) Token: 0x06003731 RID: 14129 RVA: 0x00214873 File Offset: 0x00212A73
		protected override PrefabCollection<RuinGenerationParams> Prefabs
		{
			get
			{
				return RuinGenerationParams.RuinParams;
			}
		}

		// Token: 0x06003732 RID: 14130 RVA: 0x0021487A File Offset: 0x00212A7A
		protected override RuinGenerationParams CreatePrefab(ContentXElement element)
		{
			return new RuinGenerationParams(element, this);
		}
	}
}
