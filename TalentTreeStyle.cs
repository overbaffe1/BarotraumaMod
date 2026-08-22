using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000BA RID: 186
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct TalentTreeStyle
	{
		// Token: 0x06001732 RID: 5938 RVA: 0x000DF9A1 File Offset: 0x000DDBA1
		public TalentTreeStyle(string componentStyle, Color color)
		{
			this.ComponentStyle = GUIStyle.GetComponentStyle(componentStyle);
			this.Color = color;
		}

		// Token: 0x04000BC3 RID: 3011
		public readonly GUIComponentStyle ComponentStyle;

		// Token: 0x04000BC4 RID: 3012
		public readonly Color Color;
	}
}
