using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x020001ED RID: 493
	internal class SlideshowPrefab : Prefab
	{
		// Token: 0x0600236E RID: 9070 RVA: 0x000EDD1C File Offset: 0x000EBF1C
		public SlideshowPrefab(ContentFile file, ContentXElement element) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			List<SlideshowPrefab.Slide> slides = new List<SlideshowPrefab.Slide>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "slide")
				{
					slides.Add(new SlideshowPrefab.Slide(subElement));
				}
			}
			this.Slides = slides.ToImmutableArray<SlideshowPrefab.Slide>();
		}

		// Token: 0x0600236F RID: 9071 RVA: 0x000EDDB8 File Offset: 0x000EBFB8
		public override void Dispose()
		{
		}

		// Token: 0x0400111E RID: 4382
		public static readonly PrefabCollection<SlideshowPrefab> Prefabs = new PrefabCollection<SlideshowPrefab>();

		// Token: 0x0400111F RID: 4383
		public readonly ImmutableArray<SlideshowPrefab.Slide> Slides;

		// Token: 0x020009A6 RID: 2470
		public class Slide
		{
			// Token: 0x06005A5A RID: 23130 RVA: 0x001FB9D4 File Offset: 0x001F9BD4
			public Slide(ContentXElement element)
			{
				string text = element.GetAttributeString("Text", string.Empty);
				this.Text = TextManager.Get(text).Fallback(text, true);
				this.FadeInDelay = element.GetAttributeFloat("FadeInDelay", 0f);
				this.FadeInDuration = element.GetAttributeFloat("FadeInDuration", 2f);
				this.FadeOutDuration = element.GetAttributeFloat("FadeOutDuration", 2f);
				this.TextFadeInDelay = element.GetAttributeFloat("TextFadeInDelay", 2f);
				this.TextFadeInDuration = element.GetAttributeFloat("TextFadeInDuration", 3f);
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (a == "portrait")
					{
						this.Portrait = new Sprite(subElement, "", "", true, 1f);
					}
				}
			}

			// Token: 0x040033F5 RID: 13301
			public readonly LocalizedString Text;

			// Token: 0x040033F6 RID: 13302
			public readonly Sprite Portrait;

			// Token: 0x040033F7 RID: 13303
			public readonly float FadeInDelay;

			// Token: 0x040033F8 RID: 13304
			public readonly float FadeInDuration;

			// Token: 0x040033F9 RID: 13305
			public readonly float FadeOutDuration;

			// Token: 0x040033FA RID: 13306
			public readonly float TextFadeInDelay;

			// Token: 0x040033FB RID: 13307
			public readonly float TextFadeInDuration;
		}
	}
}
