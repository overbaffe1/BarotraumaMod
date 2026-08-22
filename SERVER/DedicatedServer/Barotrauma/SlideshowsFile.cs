using System;

namespace Barotrauma
{
	// Token: 0x0200014C RID: 332
	internal sealed class SlideshowsFile : GenericPrefabFile<SlideshowPrefab>
	{
		// Token: 0x17000831 RID: 2097
		// (get) Token: 0x06001C56 RID: 7254 RVA: 0x000CEEA7 File Offset: 0x000CD0A7
		protected override PrefabCollection<SlideshowPrefab> Prefabs
		{
			get
			{
				return SlideshowPrefab.Prefabs;
			}
		}

		// Token: 0x06001C57 RID: 7255 RVA: 0x000CEEAE File Offset: 0x000CD0AE
		public SlideshowsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C58 RID: 7256 RVA: 0x000CEEB8 File Offset: 0x000CD0B8
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "Slideshow";
		}

		// Token: 0x06001C59 RID: 7257 RVA: 0x000CEEC6 File Offset: 0x000CD0C6
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "Slideshows";
		}

		// Token: 0x06001C5A RID: 7258 RVA: 0x000CEED4 File Offset: 0x000CD0D4
		protected override SlideshowPrefab CreatePrefab(ContentXElement element)
		{
			return new SlideshowPrefab(this, element);
		}
	}
}
