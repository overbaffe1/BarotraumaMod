using System;
using Barotrauma.Particles;

namespace Barotrauma
{
	// Token: 0x0200023D RID: 573
	[RequiredByCorePackage(new Type[]
	{

	})]
	[NotSyncedInMultiplayer]
	internal sealed class ParticlesFile : GenericPrefabFile<ParticlePrefab>
	{
		// Token: 0x06003723 RID: 14115 RVA: 0x002144DF File Offset: 0x002126DF
		public ParticlesFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003724 RID: 14116 RVA: 0x002144E9 File Offset: 0x002126E9
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06003725 RID: 14117 RVA: 0x002144F5 File Offset: 0x002126F5
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "prefabs" || identifier == "particles";
		}

		// Token: 0x17000E91 RID: 3729
		// (get) Token: 0x06003726 RID: 14118 RVA: 0x00214513 File Offset: 0x00212713
		protected override PrefabCollection<ParticlePrefab> Prefabs
		{
			get
			{
				return ParticlePrefab.Prefabs;
			}
		}

		// Token: 0x06003727 RID: 14119 RVA: 0x0021451A File Offset: 0x0021271A
		protected override ParticlePrefab CreatePrefab(ContentXElement element)
		{
			return new ParticlePrefab(element, this);
		}

		// Token: 0x06003728 RID: 14120 RVA: 0x00214523 File Offset: 0x00212723
		public override Md5Hash CalculateHash()
		{
			return Md5Hash.Blank;
		}
	}
}
