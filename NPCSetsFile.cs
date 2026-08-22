using System;

namespace Barotrauma
{
	// Token: 0x02000237 RID: 567
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class NPCSetsFile : GenericPrefabFile<NPCSet>
	{
		// Token: 0x0600370E RID: 14094 RVA: 0x0021423B File Offset: 0x0021243B
		public NPCSetsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600370F RID: 14095 RVA: 0x00214245 File Offset: 0x00212445
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "npcset";
		}

		// Token: 0x06003710 RID: 14096 RVA: 0x00214253 File Offset: 0x00212453
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "npcsets";
		}

		// Token: 0x17000E8F RID: 3727
		// (get) Token: 0x06003711 RID: 14097 RVA: 0x00214261 File Offset: 0x00212461
		protected override PrefabCollection<NPCSet> Prefabs
		{
			get
			{
				return NPCSet.Sets;
			}
		}

		// Token: 0x06003712 RID: 14098 RVA: 0x00214268 File Offset: 0x00212468
		protected override NPCSet CreatePrefab(ContentXElement element)
		{
			return new NPCSet(element, this);
		}
	}
}
