using System;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Lights
{
	// Token: 0x020004D4 RID: 1236
	internal class CustomBlendStates
	{
		// Token: 0x17001477 RID: 5239
		// (get) Token: 0x06005063 RID: 20579 RVA: 0x002B4EF0 File Offset: 0x002B30F0
		// (set) Token: 0x06005064 RID: 20580 RVA: 0x002B4EF7 File Offset: 0x002B30F7
		public static BlendState Multiplicative { get; private set; } = new BlendState
		{
			ColorSourceBlend = Blend.DestinationColor,
			ColorDestinationBlend = Blend.SourceColor,
			ColorBlendFunction = BlendFunction.Add
		};
	}
}
