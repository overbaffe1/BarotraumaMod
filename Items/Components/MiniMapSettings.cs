using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C2 RID: 1474
	internal readonly struct MiniMapSettings
	{
		// Token: 0x06005C68 RID: 23656 RVA: 0x002F79D4 File Offset: 0x002F5BD4
		public MiniMapSettings(bool createHullElements = false, Color? elementColor = null)
		{
			this.CreateHullElements = createHullElements;
			this.ElementColor = (elementColor ?? MiniMap.MiniMapBaseColor);
		}

		// Token: 0x04002F29 RID: 12073
		public static MiniMapSettings Default = new MiniMapSettings(true, new Color?(MiniMap.MiniMapBaseColor));

		// Token: 0x04002F2A RID: 12074
		public readonly bool CreateHullElements;

		// Token: 0x04002F2B RID: 12075
		public readonly Color ElementColor;
	}
}
