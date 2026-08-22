using System;
using Barotrauma.RuinGeneration;

namespace Barotrauma
{
	// Token: 0x02000149 RID: 329
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class RuinConfigFile : GenericPrefabFile<RuinGenerationParams>
	{
		// Token: 0x06001C4A RID: 7242 RVA: 0x000CED00 File Offset: 0x000CCF00
		public RuinConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x000CED0A File Offset: 0x000CCF0A
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "RuinConfig";
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x000CED18 File Offset: 0x000CCF18
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "RuinGenerationParameters";
		}

		// Token: 0x17000830 RID: 2096
		// (get) Token: 0x06001C4D RID: 7245 RVA: 0x000CED26 File Offset: 0x000CCF26
		protected override PrefabCollection<RuinGenerationParams> Prefabs
		{
			get
			{
				return RuinGenerationParams.RuinParams;
			}
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x000CED2D File Offset: 0x000CCF2D
		protected override RuinGenerationParams CreatePrefab(ContentXElement element)
		{
			return new RuinGenerationParams(element, this);
		}
	}
}
