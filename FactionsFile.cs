using System;

namespace Barotrauma
{
	// Token: 0x0200022A RID: 554
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class FactionsFile : GenericPrefabFile<FactionPrefab>
	{
		// Token: 0x060036CC RID: 14028 RVA: 0x002139EA File Offset: 0x00211BEA
		public FactionsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036CD RID: 14029 RVA: 0x002139F4 File Offset: 0x00211BF4
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "faction";
		}

		// Token: 0x060036CE RID: 14030 RVA: 0x00213A02 File Offset: 0x00211C02
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "factions";
		}

		// Token: 0x17000E87 RID: 3719
		// (get) Token: 0x060036CF RID: 14031 RVA: 0x00213A10 File Offset: 0x00211C10
		protected override PrefabCollection<FactionPrefab> Prefabs
		{
			get
			{
				return FactionPrefab.Prefabs;
			}
		}

		// Token: 0x060036D0 RID: 14032 RVA: 0x00213A17 File Offset: 0x00211C17
		protected override FactionPrefab CreatePrefab(ContentXElement element)
		{
			return new FactionPrefab(element, this);
		}
	}
}
