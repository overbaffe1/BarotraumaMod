using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000600 RID: 1536
	internal struct PlantSlot
	{
		// Token: 0x060063D4 RID: 25556 RVA: 0x0033EB2C File Offset: 0x0033CD2C
		[NullableContext(1)]
		public PlantSlot(ContentXElement element)
		{
			string key = "offset";
			Vector2 zero = Vector2.Zero;
			this.Offset = element.GetAttributeVector2(key, zero);
			this.Size = element.GetAttributeFloat("size", 0.5f);
		}

		// Token: 0x060063D5 RID: 25557 RVA: 0x0033EB68 File Offset: 0x0033CD68
		public PlantSlot(Vector2 offset, float size)
		{
			this.Offset = offset;
			this.Size = size;
		}

		// Token: 0x040033C4 RID: 13252
		public Vector2 Offset;

		// Token: 0x040033C5 RID: 13253
		public float Size;
	}
}
