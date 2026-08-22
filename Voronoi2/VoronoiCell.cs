using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Voronoi2
{
	// Token: 0x0200000D RID: 13
	public class VoronoiCell
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00003B7B File Offset: 0x00001D7B
		public Vector2 Center
		{
			get
			{
				return new Vector2((float)this.Site.Coord.X, (float)this.Site.Coord.Y) + this.Translation;
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003BB0 File Offset: 0x00001DB0
		public VoronoiCell(Vector2[] vertices)
		{
			this.Edges = new List<GraphEdge>();
			this.BodyVertices = new List<Vector2>();
			Vector2 midPoint = Vector2.Zero;
			foreach (Vector2 vertex in vertices)
			{
				midPoint += vertex;
			}
			midPoint /= (float)vertices.Length;
			for (int i = 0; i < vertices.Length; i++)
			{
				GraphEdge ge = new GraphEdge(vertices[i], vertices[MathUtils.PositiveModulo(i + 1, vertices.Length)]);
				this.Edges.Add(ge);
			}
			this.Site = new Site();
			this.Site.SetPoint(midPoint);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003C61 File Offset: 0x00001E61
		public VoronoiCell(Site site)
		{
			this.Edges = new List<GraphEdge>();
			this.BodyVertices = new List<Vector2>();
			this.Site = site;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003C88 File Offset: 0x00001E88
		public bool IsPointInside(Vector2 point)
		{
			if (!this.IsPointInsideAABB(point, 0f))
			{
				return false;
			}
			Vector2 transformedPoint = point - this.Translation;
			foreach (GraphEdge edge in this.Edges)
			{
				if (MathUtils.LineSegmentsIntersect(transformedPoint, this.Center - this.Translation, edge.Point1, edge.Point2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003D20 File Offset: 0x00001F20
		public bool IsPointInsideAABB(Vector2 point2, float margin)
		{
			Vector2 transformedPoint = point2 - this.Translation;
			Vector2 max = transformedPoint + Vector2.One * margin;
			Vector2 min = transformedPoint - Vector2.One * margin;
			if (this.<IsPointInsideAABB>g__AllOutsideBounds|15_4((GraphEdge e, float t) => e.Point1.X < t && e.Point2.X < t, min.X))
			{
				return false;
			}
			if (this.<IsPointInsideAABB>g__AllOutsideBounds|15_4((GraphEdge e, float t) => e.Point1.Y < t && e.Point2.Y < t, min.Y))
			{
				return false;
			}
			if (this.<IsPointInsideAABB>g__AllOutsideBounds|15_4((GraphEdge e, float t) => e.Point1.X > t && e.Point2.X > t, max.X))
			{
				return false;
			}
			return !this.<IsPointInsideAABB>g__AllOutsideBounds|15_4((GraphEdge e, float t) => e.Point1.Y > t && e.Point2.Y > t, max.Y);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003E1C File Offset: 0x0000201C
		public void GetBounds(out Vector2 min, out Vector2 max)
		{
			min = this.Center;
			max = this.Center;
			foreach (GraphEdge edge in this.Edges)
			{
				min = new Vector2(Math.Min(edge.Point1.X, min.X), Math.Min(edge.Point1.Y, min.Y));
				min = new Vector2(Math.Min(edge.Point2.X, min.X), Math.Min(edge.Point2.Y, min.Y));
				max = new Vector2(Math.Max(edge.Point1.X, max.X), Math.Max(edge.Point1.Y, max.Y));
				max = new Vector2(Math.Max(edge.Point2.X, max.X), Math.Max(edge.Point2.Y, max.Y));
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003F64 File Offset: 0x00002164
		[CompilerGenerated]
		private bool <IsPointInsideAABB>g__AllOutsideBounds|15_4(Func<GraphEdge, float, bool> predicate, float bounds)
		{
			foreach (GraphEdge edge in this.Edges)
			{
				if (!predicate(edge, bounds))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04000042 RID: 66
		public List<GraphEdge> Edges;

		// Token: 0x04000043 RID: 67
		public Site Site;

		// Token: 0x04000044 RID: 68
		public List<Vector2> BodyVertices;

		// Token: 0x04000045 RID: 69
		public Body Body;

		// Token: 0x04000046 RID: 70
		public CellType CellType;

		// Token: 0x04000047 RID: 71
		public Vector2 Translation;

		// Token: 0x04000048 RID: 72
		public bool Island;

		// Token: 0x04000049 RID: 73
		public bool IsDestructible;

		// Token: 0x0400004A RID: 74
		public bool DoesDamage;

		// Token: 0x0400004B RID: 75
		public Action OnDestroyed;
	}
}
