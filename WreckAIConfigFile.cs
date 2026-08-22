using System;

namespace Barotrauma
{
	// Token: 0x0200024E RID: 590
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class WreckAIConfigFile : GenericPrefabFile<WreckAIConfig>
	{
		// Token: 0x06003777 RID: 14199 RVA: 0x00215309 File Offset: 0x00213509
		public WreckAIConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003778 RID: 14200 RVA: 0x00215313 File Offset: 0x00213513
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "wreckaiconfig";
		}

		// Token: 0x06003779 RID: 14201 RVA: 0x00215321 File Offset: 0x00213521
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "wreckaiconfigs";
		}

		// Token: 0x17000E9B RID: 3739
		// (get) Token: 0x0600377A RID: 14202 RVA: 0x0021532F File Offset: 0x0021352F
		protected override PrefabCollection<WreckAIConfig> Prefabs
		{
			get
			{
				return WreckAIConfig.Prefabs;
			}
		}

		// Token: 0x0600377B RID: 14203 RVA: 0x00215336 File Offset: 0x00213536
		protected override WreckAIConfig CreatePrefab(ContentXElement element)
		{
			return new WreckAIConfig(element, this);
		}
	}
}
