using System;

namespace Barotrauma
{
	// Token: 0x02000131 RID: 305
	internal sealed class DisembarkPerkFile : GenericPrefabFile<DisembarkPerkPrefab>
	{
		// Token: 0x06001BE2 RID: 7138 RVA: 0x000CDED9 File Offset: 0x000CC0D9
		public DisembarkPerkFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x000CDEE3 File Offset: 0x000CC0E3
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "disembarkperk";
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x000CDEF1 File Offset: 0x000CC0F1
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "disembarkperks";
		}

		// Token: 0x17000824 RID: 2084
		// (get) Token: 0x06001BE5 RID: 7141 RVA: 0x000CDEFF File Offset: 0x000CC0FF
		protected override PrefabCollection<DisembarkPerkPrefab> Prefabs
		{
			get
			{
				return DisembarkPerkPrefab.Prefabs;
			}
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x000CDF06 File Offset: 0x000CC106
		protected override DisembarkPerkPrefab CreatePrefab(ContentXElement element)
		{
			return new DisembarkPerkPrefab(element, this);
		}
	}
}
