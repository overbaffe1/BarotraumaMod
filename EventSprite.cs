using System;

namespace Barotrauma
{
	// Token: 0x020002B4 RID: 692
	internal class EventSprite : Prefab
	{
		// Token: 0x06003BFC RID: 15356 RVA: 0x002256AE File Offset: 0x002238AE
		public EventSprite(ContentXElement element, RandomEventsFile file) : base(file, element.GetAttributeIdentifier("identifier", Identifier.Empty))
		{
			this.Sprite = new Sprite(element, "", "", false, 1f);
		}

		// Token: 0x06003BFD RID: 15357 RVA: 0x002256E3 File Offset: 0x002238E3
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			sprite.Remove();
		}

		// Token: 0x04001EAD RID: 7853
		public static readonly PrefabCollection<EventSprite> Prefabs = new PrefabCollection<EventSprite>();

		// Token: 0x04001EAE RID: 7854
		public readonly Sprite Sprite;
	}
}
