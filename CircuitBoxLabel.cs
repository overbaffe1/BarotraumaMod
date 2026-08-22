using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000038 RID: 56
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct CircuitBoxLabel
	{
		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000908 RID: 2312 RVA: 0x000517EC File Offset: 0x0004F9EC
		public LocalizedString Value { get; }

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000909 RID: 2313 RVA: 0x000517F4 File Offset: 0x0004F9F4
		public Vector2 Size { get; }

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x0600090A RID: 2314 RVA: 0x000517FC File Offset: 0x0004F9FC
		public GUIFont Font { get; }

		// Token: 0x0600090B RID: 2315 RVA: 0x00051804 File Offset: 0x0004FA04
		public CircuitBoxLabel(LocalizedString value, GUIFont font)
		{
			this.Value = value;
			this.Font = font;
			this.Size = font.MeasureString(font.ForceUpperCase ? value.Value.ToUpperInvariant() : value.Value, false);
		}
	}
}
