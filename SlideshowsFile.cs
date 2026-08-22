using System;

namespace Barotrauma
{
	// Token: 0x02000242 RID: 578
	internal sealed class SlideshowsFile : GenericPrefabFile<SlideshowPrefab>
	{
		// Token: 0x17000E93 RID: 3731
		// (get) Token: 0x0600373A RID: 14138 RVA: 0x002149F7 File Offset: 0x00212BF7
		protected override PrefabCollection<SlideshowPrefab> Prefabs
		{
			get
			{
				return SlideshowPrefab.Prefabs;
			}
		}

		// Token: 0x0600373B RID: 14139 RVA: 0x002149FE File Offset: 0x00212BFE
		public SlideshowsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600373C RID: 14140 RVA: 0x00214A08 File Offset: 0x00212C08
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "Slideshow";
		}

		// Token: 0x0600373D RID: 14141 RVA: 0x00214A16 File Offset: 0x00212C16
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "Slideshows";
		}

		// Token: 0x0600373E RID: 14142 RVA: 0x00214A24 File Offset: 0x00212C24
		protected override SlideshowPrefab CreatePrefab(ContentXElement element)
		{
			return new SlideshowPrefab(this, element);
		}
	}
}
