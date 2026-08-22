using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C1 RID: 1473
	internal readonly struct RelativeEntityRect
	{
		// Token: 0x06005C63 RID: 23651 RVA: 0x002F78D8 File Offset: 0x002F5AD8
		public RelativeEntityRect(RectangleF worldBorders, RectangleF entityRect)
		{
			this.RelativePosition = new Vector2((entityRect.X - worldBorders.X) / worldBorders.Width, (worldBorders.Y - entityRect.Y) / worldBorders.Height);
			this.RelativeSize = new Vector2(entityRect.Width / worldBorders.Width, entityRect.Height / worldBorders.Height);
		}

		// Token: 0x06005C64 RID: 23652 RVA: 0x002F793D File Offset: 0x002F5B3D
		public Vector2 PositionRelativeTo(RectangleF frame, bool skipOffset = false)
		{
			if (skipOffset)
			{
				return this.RelativePosition * frame.Size;
			}
			return frame.Location + this.RelativePosition * frame.Size;
		}

		// Token: 0x06005C65 RID: 23653 RVA: 0x002F7973 File Offset: 0x002F5B73
		public Vector2 SizeRelativeTo(RectangleF frame)
		{
			return this.RelativeSize * frame.Size;
		}

		// Token: 0x06005C66 RID: 23654 RVA: 0x002F7987 File Offset: 0x002F5B87
		public RectangleF RectangleRelativeTo(RectangleF frame, bool skipOffset = false)
		{
			return new RectangleF(this.PositionRelativeTo(frame, skipOffset), this.SizeRelativeTo(frame));
		}

		// Token: 0x06005C67 RID: 23655 RVA: 0x002F799D File Offset: 0x002F5B9D
		public void Deconstruct(out float posX, out float posY, out float sizeX, out float sizeY)
		{
			posX = this.RelativePosition.X;
			posY = this.RelativePosition.Y;
			sizeX = this.RelativeSize.X;
			sizeY = this.RelativeSize.Y;
		}

		// Token: 0x04002F27 RID: 12071
		public readonly Vector2 RelativePosition;

		// Token: 0x04002F28 RID: 12072
		public readonly Vector2 RelativeSize;
	}
}
