using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D6 RID: 1238
	internal struct PlantSlot
	{
		// Token: 0x0600465D RID: 18013 RVA: 0x001C1698 File Offset: 0x001BF898
		[NullableContext(1)]
		public PlantSlot(ContentXElement element)
		{
			string key = "offset";
			Vector2 zero = Vector2.Zero;
			this.Offset = element.GetAttributeVector2(key, zero);
			this.Size = element.GetAttributeFloat("size", 0.5f);
		}

		// Token: 0x0600465E RID: 18014 RVA: 0x001C16D4 File Offset: 0x001BF8D4
		public PlantSlot(Vector2 offset, float size)
		{
			this.Offset = offset;
			this.Size = size;
		}

		// Token: 0x040021DC RID: 8668
		public Vector2 Offset;

		// Token: 0x040021DD RID: 8669
		public float Size;
	}
}
