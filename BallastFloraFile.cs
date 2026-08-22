using System;

namespace Barotrauma
{
	// Token: 0x0200021E RID: 542
	[RequiredByCorePackage(new Type[]
	{

	})]
	[AlternativeContentTypeNames(new string[]
	{
		"MapCreature"
	})]
	internal sealed class BallastFloraFile : GenericPrefabFile<BallastFloraPrefab>
	{
		// Token: 0x06003692 RID: 13970 RVA: 0x0021308D File Offset: 0x0021128D
		public BallastFloraFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003693 RID: 13971 RVA: 0x00213097 File Offset: 0x00211297
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "ballastflorabehavior";
		}

		// Token: 0x06003694 RID: 13972 RVA: 0x002130A5 File Offset: 0x002112A5
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "ballastflorabehaviors";
		}

		// Token: 0x17000E80 RID: 3712
		// (get) Token: 0x06003695 RID: 13973 RVA: 0x002130B3 File Offset: 0x002112B3
		protected override PrefabCollection<BallastFloraPrefab> Prefabs
		{
			get
			{
				return BallastFloraPrefab.Prefabs;
			}
		}

		// Token: 0x06003696 RID: 13974 RVA: 0x002130BA File Offset: 0x002112BA
		protected override BallastFloraPrefab CreatePrefab(ContentXElement element)
		{
			return new BallastFloraPrefab(element, this);
		}
	}
}
