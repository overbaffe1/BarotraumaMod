using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200010B RID: 267
	internal class SpecialNode : EditorNode
	{
		// Token: 0x060024CA RID: 9418 RVA: 0x00173C72 File Offset: 0x00171E72
		[NullableContext(1)]
		public SpecialNode(string name) : base(name)
		{
			base.Size = new Vector2(256f, 256f);
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x00173C90 File Offset: 0x00171E90
		public override Rectangle GetDrawRectangle()
		{
			return EventNode.ScaleRectFromConnections(this.Connections, base.Rectangle);
		}
	}
}
