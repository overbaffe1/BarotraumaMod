using System;

namespace Barotrauma
{
	// Token: 0x02000141 RID: 321
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class NPCSetsFile : GenericPrefabFile<NPCSet>
	{
		// Token: 0x06001C2F RID: 7215 RVA: 0x000CE7A3 File Offset: 0x000CC9A3
		public NPCSetsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C30 RID: 7216 RVA: 0x000CE7AD File Offset: 0x000CC9AD
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "npcset";
		}

		// Token: 0x06001C31 RID: 7217 RVA: 0x000CE7BB File Offset: 0x000CC9BB
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "npcsets";
		}

		// Token: 0x1700082E RID: 2094
		// (get) Token: 0x06001C32 RID: 7218 RVA: 0x000CE7C9 File Offset: 0x000CC9C9
		protected override PrefabCollection<NPCSet> Prefabs
		{
			get
			{
				return NPCSet.Sets;
			}
		}

		// Token: 0x06001C33 RID: 7219 RVA: 0x000CE7D0 File Offset: 0x000CC9D0
		protected override NPCSet CreatePrefab(ContentXElement element)
		{
			return new NPCSet(element, this);
		}
	}
}
