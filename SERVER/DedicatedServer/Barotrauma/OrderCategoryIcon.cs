using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000089 RID: 137
	internal class OrderCategoryIcon : Prefab
	{
		// Token: 0x060011D2 RID: 4562 RVA: 0x0009FF58 File Offset: 0x0009E158
		public OrderCategoryIcon(ContentXElement element, OrdersFile file) : base(file, element.GetAttributeIdentifier("category", ""))
		{
			this.Category = Enum.Parse<OrderCategory>(this.Identifier.Value, true);
			ContentXElement spriteElement = element.GetChildElement("sprite");
			this.Sprite = new Sprite(spriteElement, "", "", true, 1f);
			string key = "color";
			Color white = Color.White;
			this.Color = element.GetAttributeColor(key, white);
		}

		// Token: 0x060011D3 RID: 4563 RVA: 0x0009FFD4 File Offset: 0x0009E1D4
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			sprite.Remove();
		}

		// Token: 0x04000868 RID: 2152
		public static readonly PrefabCollection<OrderCategoryIcon> OrderCategoryIcons = new PrefabCollection<OrderCategoryIcon>();

		// Token: 0x04000869 RID: 2153
		public readonly OrderCategory Category;

		// Token: 0x0400086A RID: 2154
		public readonly Sprite Sprite;

		// Token: 0x0400086B RID: 2155
		public readonly Color Color;
	}
}
