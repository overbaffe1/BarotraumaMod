using System;
using Microsoft.Xna.Framework;

namespace Voronoi2
{
	// Token: 0x0200000E RID: 14
	public class GraphEdge
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00003ED8 File Offset: 0x000020D8
		public Vector2 Center
		{
			get
			{
				return (this.Point1 + this.Point2) / 2f;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00003EF5 File Offset: 0x000020F5
		public float Length
		{
			get
			{
				return Vector2.Distance(this.Point1, this.Point2);
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003F08 File Offset: 0x00002108
		public GraphEdge(Vector2 point1, Vector2 point2)
		{
			this.Point1 = point1;
			this.Point2 = point2;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003F1E File Offset: 0x0000211E
		public VoronoiCell AdjacentCell(VoronoiCell cell)
		{
			if (this.Cell1 == cell)
			{
				return this.Cell2;
			}
			if (this.Cell2 == cell)
			{
				return this.Cell1;
			}
			return null;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003F41 File Offset: 0x00002141
		public Vector2 GetNormal(VoronoiCell cell)
		{
			return GraphEdge.GetNormal(cell, this.Point1, this.Point2);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003F58 File Offset: 0x00002158
		public static Vector2 GetNormal(VoronoiCell cell, Vector2 point1, Vector2 point2)
		{
			Vector2 center = (point1 + point2) / 2f;
			Vector2 dir = Vector2.Normalize(point1 - point2);
			Vector2 normal = new Vector2(dir.Y, -dir.X);
			if (cell != null && Vector2.Dot(normal, Vector2.Normalize(center - (cell.Center - cell.Translation))) < 0f)
			{
				normal = -normal;
			}
			return normal;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003FCC File Offset: 0x000021CC
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"GraphEdge (",
				this.Point1.ToString(),
				", ",
				this.Point2.ToString(),
				")"
			});
		}

		// Token: 0x0400004C RID: 76
		public Vector2 Point1;

		// Token: 0x0400004D RID: 77
		public Vector2 Point2;

		// Token: 0x0400004E RID: 78
		public Site Site1;

		// Token: 0x0400004F RID: 79
		public Site Site2;

		// Token: 0x04000050 RID: 80
		public VoronoiCell Cell1;

		// Token: 0x04000051 RID: 81
		public VoronoiCell Cell2;

		// Token: 0x04000052 RID: 82
		public bool IsSolid;

		// Token: 0x04000053 RID: 83
		public bool OutsideLevel;

		// Token: 0x04000054 RID: 84
		public bool NextToCave;

		// Token: 0x04000055 RID: 85
		public bool NextToMainPath;

		// Token: 0x04000056 RID: 86
		public bool NextToSidePath;
	}
}
