using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200011F RID: 287
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxNode : CircuitBoxSelectable
	{
		// Token: 0x17000817 RID: 2071
		// (get) Token: 0x06001B81 RID: 7041 RVA: 0x000CC6C4 File Offset: 0x000CA8C4
		public virtual bool IsResizable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000818 RID: 2072
		// (get) Token: 0x06001B82 RID: 7042 RVA: 0x000CC6C7 File Offset: 0x000CA8C7
		// (set) Token: 0x06001B83 RID: 7043 RVA: 0x000CC6CF File Offset: 0x000CA8CF
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = new Vector2(Math.Clamp(value.X, -4096f, 4096f), Math.Clamp(value.Y, -4096f, 4096f));
				this.UpdatePositions();
			}
		}

		// Token: 0x06001B84 RID: 7044 RVA: 0x000CC70C File Offset: 0x000CA90C
		public CircuitBoxNode(CircuitBox circuitBox)
		{
			this.CircuitBox = circuitBox;
		}

		// Token: 0x06001B85 RID: 7045 RVA: 0x000CC728 File Offset: 0x000CA928
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"Size",
			"Pos"
		})]
		public ValueTuple<Vector2, Vector2> ResizeBy(CircuitBoxResizeDirection directions, Vector2 amount)
		{
			Vector2 newSize = this.Size;
			Vector2 newPos = this.Position;
			amount.Y = -amount.Y;
			if (directions.HasFlag(CircuitBoxResizeDirection.Down))
			{
				newSize.Y += amount.Y;
				newSize.Y = Math.Max(newSize.Y, CircuitBoxLabelNode.MinSize.Y);
				newPos = new Vector2(newPos.X, newPos.Y - (newSize.Y - this.Size.Y) / 2f);
			}
			if (directions.HasFlag(CircuitBoxResizeDirection.Right))
			{
				newSize.X += amount.X;
				newSize.X = Math.Max(newSize.X, CircuitBoxLabelNode.MinSize.X);
				newPos = new Vector2(newPos.X + (newSize.X - this.Size.X) / 2f, newPos.Y);
			}
			if (directions.HasFlag(CircuitBoxResizeDirection.Left))
			{
				newSize.X -= amount.X;
				newSize.X = Math.Max(newSize.X, CircuitBoxLabelNode.MinSize.X);
				newPos = new Vector2(newPos.X + (this.Size.X - newSize.X) / 2f, newPos.Y);
			}
			return new ValueTuple<Vector2, Vector2>(newSize, newPos);
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x000CC89C File Offset: 0x000CAA9C
		public void ApplyResize(Vector2 newSize, Vector2 newPos)
		{
			if (!MathUtils.IsValid(newSize))
			{
				return;
			}
			this.Size = newSize;
			this.Position = newPos;
			this.UpdatePositions();
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x000CC8BC File Offset: 0x000CAABC
		public static Vector2 CalculateSize(IReadOnlyList<CircuitBoxConnection> conns)
		{
			Vector2 leftSize = Vector2.Zero;
			Vector2 rightSize = Vector2.Zero;
			foreach (CircuitBoxConnection c in conns)
			{
				if (c.IsOutput)
				{
					rightSize.X = MathF.Max(rightSize.X, c.Length);
				}
				else
				{
					leftSize.X = MathF.Max(leftSize.X, c.Length);
				}
				if (c.IsOutput)
				{
					rightSize.Y += CircuitBoxConnection.Size;
				}
				else
				{
					leftSize.Y += CircuitBoxConnection.Size;
				}
			}
			return new Vector2(leftSize.X + 64f + rightSize.X, 64f + MathF.Max(leftSize.Y, rightSize.Y));
		}

		// Token: 0x06001B88 RID: 7048 RVA: 0x000CC99C File Offset: 0x000CAB9C
		protected void UpdatePositions()
		{
			Vector2 rectStart = this.Position - this.Size / 2f;
			Vector2 rectSize = this.Size;
			rectSize.Y += 48f;
			this.Rect = new RectangleF(rectStart, rectSize);
			int leftIndex = 0;
			int rightIndex = 0;
			int inputCount = 0;
			int outputCount = 0;
			foreach (CircuitBoxConnection c3 in this.Connectors)
			{
				if (c3.IsOutput)
				{
					outputCount++;
				}
				else
				{
					inputCount++;
				}
			}
			Vector2 drawPos = this.Position;
			drawPos.Y = -drawPos.Y;
			foreach (CircuitBoxConnection c2 in from c in this.Connectors
			orderby c.Connection.DisplayOrder
			select c)
			{
				bool isOutput = c2.IsOutput;
				int yIndex = isOutput ? rightIndex : leftIndex;
				int count = isOutput ? outputCount : inputCount;
				float totalHeight = (float)count * CircuitBoxConnection.Size / 2f;
				float y = (float)yIndex * CircuitBoxConnection.Size - totalHeight;
				float halfWidth = this.Rect.Width / 2f - CircuitBoxConnection.Size / 2f;
				halfWidth -= 16f;
				float xOffset = c2.IsOutput ? halfWidth : (-halfWidth);
				Vector2 inputPos = drawPos + new Vector2(xOffset, y + c2.Rect.Height / 2f);
				c2.Position = inputPos;
				if (isOutput)
				{
					rightIndex++;
				}
				else
				{
					leftIndex++;
				}
			}
		}

		// Token: 0x04000CE0 RID: 3296
		public Vector2 Size;

		// Token: 0x04000CE1 RID: 3297
		public RectangleF Rect;

		// Token: 0x04000CE2 RID: 3298
		private Vector2 position;

		// Token: 0x04000CE3 RID: 3299
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxConnection> Connectors = ImmutableArray<CircuitBoxConnection>.Empty;

		// Token: 0x04000CE4 RID: 3300
		public static float Opacity = 0.8f;

		// Token: 0x04000CE5 RID: 3301
		public readonly CircuitBox CircuitBox;
	}
}
