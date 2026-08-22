using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200018F RID: 399
	internal class OrderCategoryIcon : Prefab
	{
		// Token: 0x06002EDA RID: 11994 RVA: 0x001F2DF0 File Offset: 0x001F0FF0
		public OrderCategoryIcon(ContentXElement element, OrdersFile file) : base(file, element.GetAttributeIdentifier("category", ""))
		{
			this.Category = Enum.Parse<OrderCategory>(this.Identifier.Value, true);
			ContentXElement spriteElement = element.GetChildElement("sprite");
			this.Sprite = new Sprite(spriteElement, "", "", true, 1f);
			string key = "color";
			Color white = Color.White;
			this.Color = element.GetAttributeColor(key, white);
		}

		// Token: 0x06002EDB RID: 11995 RVA: 0x001F2E6C File Offset: 0x001F106C
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			sprite.Remove();
		}

		// Token: 0x04001862 RID: 6242
		public static readonly PrefabCollection<OrderCategoryIcon> OrderCategoryIcons = new PrefabCollection<OrderCategoryIcon>();

		// Token: 0x04001863 RID: 6243
		public readonly OrderCategory Category;

		// Token: 0x04001864 RID: 6244
		public readonly Sprite Sprite;

		// Token: 0x04001865 RID: 6245
		public readonly Color Color;
	}
}
