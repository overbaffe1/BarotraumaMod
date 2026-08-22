using System;

namespace Barotrauma
{
	// Token: 0x02000158 RID: 344
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class WreckAIConfigFile : GenericPrefabFile<WreckAIConfig>
	{
		// Token: 0x06001C86 RID: 7302 RVA: 0x000CF39D File Offset: 0x000CD59D
		public WreckAIConfigFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C87 RID: 7303 RVA: 0x000CF3A7 File Offset: 0x000CD5A7
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "wreckaiconfig";
		}

		// Token: 0x06001C88 RID: 7304 RVA: 0x000CF3B5 File Offset: 0x000CD5B5
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "wreckaiconfigs";
		}

		// Token: 0x17000838 RID: 2104
		// (get) Token: 0x06001C89 RID: 7305 RVA: 0x000CF3C3 File Offset: 0x000CD5C3
		protected override PrefabCollection<WreckAIConfig> Prefabs
		{
			get
			{
				return WreckAIConfig.Prefabs;
			}
		}

		// Token: 0x06001C8A RID: 7306 RVA: 0x000CF3CA File Offset: 0x000CD5CA
		protected override WreckAIConfig CreatePrefab(ContentXElement element)
		{
			return new WreckAIConfig(element, this);
		}
	}
}
