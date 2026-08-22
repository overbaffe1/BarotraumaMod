using System;

namespace Barotrauma
{
	// Token: 0x0200014E RID: 334
	internal sealed class StartItemsFile : GenericPrefabFile<StartItemSet>
	{
		// Token: 0x06001C5C RID: 7260 RVA: 0x000CEEE7 File Offset: 0x000CD0E7
		public StartItemsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C5D RID: 7261 RVA: 0x000CEEF1 File Offset: 0x000CD0F1
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "itemset";
		}

		// Token: 0x06001C5E RID: 7262 RVA: 0x000CEEFF File Offset: 0x000CD0FF
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "startitems";
		}

		// Token: 0x17000832 RID: 2098
		// (get) Token: 0x06001C5F RID: 7263 RVA: 0x000CEF0D File Offset: 0x000CD10D
		protected override PrefabCollection<StartItemSet> Prefabs
		{
			get
			{
				return StartItemSet.Sets;
			}
		}

		// Token: 0x06001C60 RID: 7264 RVA: 0x000CEF14 File Offset: 0x000CD114
		protected override StartItemSet CreatePrefab(ContentXElement element)
		{
			return new StartItemSet(element, this);
		}
	}
}
