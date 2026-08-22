using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BF RID: 1471
	internal readonly struct MiniMapHullData
	{
		// Token: 0x06005C62 RID: 23650 RVA: 0x002F7868 File Offset: 0x002F5A68
		public MiniMapHullData([Nullable(1)] List<List<Vector2>> polygon, RectangleF bounds, Point parentSize, ImmutableArray<RectangleF> rects, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Hull> hulls)
		{
			this.ParentSize = parentSize;
			this.Bounds = bounds;
			this.Polygon = polygon;
			int count = Math.Min(rects.Length, hulls.Length);
			this.RectDatas = new ValueTuple<RectangleF, Hull>[count];
			for (int i = 0; i < count; i++)
			{
				this.RectDatas[i] = new ValueTuple<RectangleF, Hull>(rects[i], hulls[i]);
			}
		}

		// Token: 0x04002F1E RID: 12062
		[Nullable(1)]
		public readonly List<List<Vector2>> Polygon;

		// Token: 0x04002F1F RID: 12063
		[TupleElementNames(new string[]
		{
			"Rect",
			"Hull"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public readonly ValueTuple<RectangleF, Hull>[] RectDatas;

		// Token: 0x04002F20 RID: 12064
		public readonly RectangleF Bounds;

		// Token: 0x04002F21 RID: 12065
		public readonly Point ParentSize;
	}
}
