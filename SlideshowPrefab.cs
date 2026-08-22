using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x020002D7 RID: 727
	internal class SlideshowPrefab : Prefab
	{
		// Token: 0x06003D14 RID: 15636 RVA: 0x0022D38C File Offset: 0x0022B58C
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

		// Token: 0x06003D15 RID: 15637 RVA: 0x0022D428 File Offset: 0x0022B628
		public override void Dispose()
		{
		}

		// Token: 0x04001FA7 RID: 8103
		public static readonly PrefabCollection<SlideshowPrefab> Prefabs = new PrefabCollection<SlideshowPrefab>();

		// Token: 0x04001FA8 RID: 8104
		public readonly ImmutableArray<SlideshowPrefab.Slide> Slides;

		// Token: 0x02000F83 RID: 3971
		public class Slide
		{
			// Token: 0x06008956 RID: 35158 RVA: 0x003A7C94 File Offset: 0x003A5E94
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

			// Token: 0x040055D7 RID: 21975
			public readonly LocalizedString Text;

			// Token: 0x040055D8 RID: 21976
			public readonly Sprite Portrait;

			// Token: 0x040055D9 RID: 21977
			public readonly float FadeInDelay;

			// Token: 0x040055DA RID: 21978
			public readonly float FadeInDuration;

			// Token: 0x040055DB RID: 21979
			public readonly float FadeOutDuration;

			// Token: 0x040055DC RID: 21980
			public readonly float TextFadeInDelay;

			// Token: 0x040055DD RID: 21981
			public readonly float TextFadeInDuration;
		}
	}
}
