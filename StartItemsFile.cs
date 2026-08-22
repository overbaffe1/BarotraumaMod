using System;

namespace Barotrauma
{
	// Token: 0x02000244 RID: 580
	internal sealed class StartItemsFile : GenericPrefabFile<StartItemSet>
	{
		// Token: 0x06003745 RID: 14149 RVA: 0x00214AAE File Offset: 0x00212CAE
		public StartItemsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003746 RID: 14150 RVA: 0x00214AB8 File Offset: 0x00212CB8
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "itemset";
		}

		// Token: 0x06003747 RID: 14151 RVA: 0x00214AC6 File Offset: 0x00212CC6
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "startitems";
		}

		// Token: 0x17000E95 RID: 3733
		// (get) Token: 0x06003748 RID: 14152 RVA: 0x00214AD4 File Offset: 0x00212CD4
		protected override PrefabCollection<StartItemSet> Prefabs
		{
			get
			{
				return StartItemSet.Sets;
			}
		}

		// Token: 0x06003749 RID: 14153 RVA: 0x00214ADB File Offset: 0x00212CDB
		protected override StartItemSet CreatePrefab(ContentXElement element)
		{
			return new StartItemSet(element, this);
		}
	}
}
