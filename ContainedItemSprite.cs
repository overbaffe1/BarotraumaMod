using System;
using System.Collections.Immutable;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020000C8 RID: 200
	internal class ContainedItemSprite
	{
		// Token: 0x060019F0 RID: 6640 RVA: 0x001069EC File Offset: 0x00104BEC
		public ContainedItemSprite(ContentXElement element, string path = "", bool lazyLoad = false)
		{
			this.Sprite = new Sprite(element, path, "", lazyLoad, 1f);
			this.UseWhenAttached = element.GetAttributeBool("usewhenattached", false);
			Enum.TryParse<ContainedItemSprite.DecorativeSpriteBehaviorType>(element.GetAttributeString("decorativespritebehavior", "None"), true, out this.DecorativeSpriteBehavior);
			this.AllowedContainerIdentifiers = element.GetAttributeIdentifierArray("allowedcontaineridentifiers", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.AllowedContainerTags = element.GetAttributeIdentifierArray("allowedcontainertags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x060019F1 RID: 6641 RVA: 0x00106A80 File Offset: 0x00104C80
		public bool MatchesContainer(Item container)
		{
			return container != null && (this.AllowedContainerIdentifiers.Contains(container.Prefab.Identifier) || this.AllowedContainerTags.Any((Identifier t) => container.Prefab.Tags.Contains(t)));
		}

		// Token: 0x04000D4A RID: 3402
		public readonly Sprite Sprite;

		// Token: 0x04000D4B RID: 3403
		public readonly bool UseWhenAttached;

		// Token: 0x04000D4C RID: 3404
		public readonly ContainedItemSprite.DecorativeSpriteBehaviorType DecorativeSpriteBehavior;

		// Token: 0x04000D4D RID: 3405
		public readonly ImmutableHashSet<Identifier> AllowedContainerIdentifiers;

		// Token: 0x04000D4E RID: 3406
		public readonly ImmutableHashSet<Identifier> AllowedContainerTags;

		// Token: 0x02000AA0 RID: 2720
		public enum DecorativeSpriteBehaviorType
		{
			// Token: 0x04004510 RID: 17680
			None,
			// Token: 0x04004511 RID: 17681
			HideWhenVisible,
			// Token: 0x04004512 RID: 17682
			HideWhenNotVisible
		}
	}
}
