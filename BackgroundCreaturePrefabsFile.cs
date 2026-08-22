using System;

namespace Barotrauma
{
	// Token: 0x0200021D RID: 541
	[NotSyncedInMultiplayer]
	internal sealed class BackgroundCreaturePrefabsFile : GenericPrefabFile<BackgroundCreaturePrefab>
	{
		// Token: 0x0600368C RID: 13964 RVA: 0x00213052 File Offset: 0x00211252
		public BackgroundCreaturePrefabsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600368D RID: 13965 RVA: 0x0021305C File Offset: 0x0021125C
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x0600368E RID: 13966 RVA: 0x00213068 File Offset: 0x00211268
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "backgroundcreatures";
		}

		// Token: 0x17000E7F RID: 3711
		// (get) Token: 0x0600368F RID: 13967 RVA: 0x00213076 File Offset: 0x00211276
		protected override PrefabCollection<BackgroundCreaturePrefab> Prefabs
		{
			get
			{
				return BackgroundCreaturePrefab.Prefabs;
			}
		}

		// Token: 0x06003690 RID: 13968 RVA: 0x0021307D File Offset: 0x0021127D
		protected override BackgroundCreaturePrefab CreatePrefab(ContentXElement element)
		{
			return new BackgroundCreaturePrefab(element, this);
		}

		// Token: 0x06003691 RID: 13969 RVA: 0x00213086 File Offset: 0x00211286
		public sealed override Md5Hash CalculateHash()
		{
			return Md5Hash.Blank;
		}
	}
}
