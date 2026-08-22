using System;

namespace Barotrauma
{
	// Token: 0x02000128 RID: 296
	[RequiredByCorePackage(new Type[]
	{

	})]
	[AlternativeContentTypeNames(new string[]
	{
		"MapCreature"
	})]
	internal sealed class BallastFloraFile : GenericPrefabFile<BallastFloraPrefab>
	{
		// Token: 0x06001BB4 RID: 7092 RVA: 0x000CD90A File Offset: 0x000CBB0A
		public BallastFloraFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BB5 RID: 7093 RVA: 0x000CD914 File Offset: 0x000CBB14
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "ballastflorabehavior";
		}

		// Token: 0x06001BB6 RID: 7094 RVA: 0x000CD922 File Offset: 0x000CBB22
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "ballastflorabehaviors";
		}

		// Token: 0x1700081F RID: 2079
		// (get) Token: 0x06001BB7 RID: 7095 RVA: 0x000CD930 File Offset: 0x000CBB30
		protected override PrefabCollection<BallastFloraPrefab> Prefabs
		{
			get
			{
				return BallastFloraPrefab.Prefabs;
			}
		}

		// Token: 0x06001BB8 RID: 7096 RVA: 0x000CD937 File Offset: 0x000CBB37
		protected override BallastFloraPrefab CreatePrefab(ContentXElement element)
		{
			return new BallastFloraPrefab(element, this);
		}
	}
}
