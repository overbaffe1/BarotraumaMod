using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AB RID: 1451
	internal class VineSprite
	{
		// Token: 0x17001615 RID: 5653
		// (get) Token: 0x0600587A RID: 22650 RVA: 0x002DB53D File Offset: 0x002D973D
		// (set) Token: 0x0600587B RID: 22651 RVA: 0x002DB545 File Offset: 0x002D9745
		[Serialize("0,0,0,0", IsPropertySaveable.No, "", "", false)]
		public Rectangle SourceRect { get; private set; }

		// Token: 0x17001616 RID: 5654
		// (get) Token: 0x0600587C RID: 22652 RVA: 0x002DB54E File Offset: 0x002D974E
		// (set) Token: 0x0600587D RID: 22653 RVA: 0x002DB556 File Offset: 0x002D9756
		[Serialize("0.5,0.5", IsPropertySaveable.No, "", "", false)]
		public Vector2 Origin { get; private set; }

		// Token: 0x0600587E RID: 22654 RVA: 0x002DB560 File Offset: 0x002D9760
		[NullableContext(1)]
		public VineSprite(ContentXElement element)
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.AbsoluteOrigin = new Vector2((float)this.SourceRect.Width * this.Origin.X, (float)this.SourceRect.Height * this.Origin.Y);
		}

		// Token: 0x04002D20 RID: 11552
		public Vector2 AbsoluteOrigin;
	}
}
