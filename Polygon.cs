using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000B3 RID: 179
	public class Polygon : IEquatable<Polygon>
	{
		// Token: 0x0600163F RID: 5695 RVA: 0x000CFBF0 File Offset: 0x000CDDF0
		public Polygon(IEnumerable<Vector2> vertices)
		{
			this._localVertices = vertices.ToArray<Vector2>();
			this._transformedVertices = this._localVertices;
			this._offset = Vector2.Zero;
			this._rotation = 0f;
			this._scale = Vector2.One;
			this._isDirty = false;
		}

		// Token: 0x170005B2 RID: 1458
		// (get) Token: 0x06001640 RID: 5696 RVA: 0x000CFC43 File Offset: 0x000CDE43
		public Vector2[] Vertices
		{
			get
			{
				if (this._isDirty)
				{
					this._transformedVertices = this.GetTransformedVertices();
					this._isDirty = false;
				}
				return this._transformedVertices;
			}
		}

		// Token: 0x170005B3 RID: 1459
		// (get) Token: 0x06001641 RID: 5697 RVA: 0x000CFC66 File Offset: 0x000CDE66
		public float Left
		{
			get
			{
				return this.Vertices.Min((Vector2 v) => v.X);
			}
		}

		// Token: 0x170005B4 RID: 1460
		// (get) Token: 0x06001642 RID: 5698 RVA: 0x000CFC92 File Offset: 0x000CDE92
		public float Right
		{
			get
			{
				return this.Vertices.Max((Vector2 v) => v.X);
			}
		}

		// Token: 0x170005B5 RID: 1461
		// (get) Token: 0x06001643 RID: 5699 RVA: 0x000CFCBE File Offset: 0x000CDEBE
		public float Top
		{
			get
			{
				return this.Vertices.Min((Vector2 v) => v.Y);
			}
		}

		// Token: 0x170005B6 RID: 1462
		// (get) Token: 0x06001644 RID: 5700 RVA: 0x000CFCEA File Offset: 0x000CDEEA
		public float Bottom
		{
			get
			{
				return this.Vertices.Max((Vector2 v) => v.Y);
			}
		}

		// Token: 0x06001645 RID: 5701 RVA: 0x000CFD16 File Offset: 0x000CDF16
		public void Offset(Vector2 amount)
		{
			this._offset += amount;
			this._isDirty = true;
		}

		// Token: 0x06001646 RID: 5702 RVA: 0x000CFD31 File Offset: 0x000CDF31
		public void Rotate(float amount)
		{
			this._rotation += amount;
			this._isDirty = true;
		}

		// Token: 0x06001647 RID: 5703 RVA: 0x000CFD48 File Offset: 0x000CDF48
		public void Scale(Vector2 amount)
		{
			this._scale += amount;
			this._isDirty = true;
		}

		// Token: 0x06001648 RID: 5704 RVA: 0x000CFD64 File Offset: 0x000CDF64
		private Vector2[] GetTransformedVertices()
		{
			Vector2[] newVertices = new Vector2[this._localVertices.Length];
			bool isScaled = this._scale != Vector2.One;
			for (int i = 0; i < this._localVertices.Length; i++)
			{
				Vector2 p = this._localVertices[i];
				if (isScaled)
				{
					p *= this._scale;
				}
				if (this._rotation != 0f)
				{
					float cos = (float)Math.Cos((double)this._rotation);
					float sin = (float)Math.Sin((double)this._rotation);
					p = new Vector2(cos * p.X - sin * p.Y, sin * p.X + cos * p.Y);
				}
				newVertices[i] = p + this._offset;
			}
			return newVertices;
		}

		// Token: 0x06001649 RID: 5705 RVA: 0x000CFE34 File Offset: 0x000CE034
		public Polygon TransformedCopy(Vector2 offset, float rotation, Vector2 scale)
		{
			Polygon polygon = new Polygon(this._localVertices);
			polygon.Offset(offset);
			polygon.Rotate(rotation);
			polygon.Scale(scale - Vector2.One);
			return new Polygon(polygon.Vertices);
		}

		// Token: 0x0600164A RID: 5706 RVA: 0x000CFE77 File Offset: 0x000CE077
		public bool Contains(Vector2 point)
		{
			return this.Contains(point.X, point.Y);
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x000CFE8C File Offset: 0x000CE08C
		public bool Contains(float x, float y)
		{
			int intersects = 0;
			Vector2[] vertices = this.Vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				float x2 = vertices[i].X;
				float y2 = vertices[i].Y;
				float x3 = vertices[(i + 1) % vertices.Length].X;
				float y3 = vertices[(i + 1) % vertices.Length].Y;
				if (((y2 <= y && y < y3) || (y3 <= y && y < y2)) && x < (x3 - x2) / (y3 - y2) * (y - y2) + x2)
				{
					intersects++;
				}
			}
			return (intersects & 1) == 1;
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x000CFF25 File Offset: 0x000CE125
		public static bool operator ==(Polygon a, Polygon b)
		{
			return a.Equals(b);
		}

		// Token: 0x0600164D RID: 5709 RVA: 0x000CFF2E File Offset: 0x000CE12E
		public static bool operator !=(Polygon a, Polygon b)
		{
			return !(a == b);
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x000CFF3A File Offset: 0x000CE13A
		public override bool Equals(object obj)
		{
			return obj != null && obj is Polygon && this.Equals((Polygon)obj);
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x000CFF57 File Offset: 0x000CE157
		public bool Equals(Polygon other)
		{
			return this.Vertices.SequenceEqual(other.Vertices);
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x000CFF74 File Offset: 0x000CE174
		public override int GetHashCode()
		{
			return this.Vertices.Aggregate(27, (int current, Vector2 v) => current + 13 * current + v.GetHashCode());
		}

		// Token: 0x04000B2A RID: 2858
		private readonly Vector2[] _localVertices;

		// Token: 0x04000B2B RID: 2859
		private Vector2[] _transformedVertices;

		// Token: 0x04000B2C RID: 2860
		private Vector2 _offset;

		// Token: 0x04000B2D RID: 2861
		private float _rotation;

		// Token: 0x04000B2E RID: 2862
		private Vector2 _scale;

		// Token: 0x04000B2F RID: 2863
		private bool _isDirty;
	}
}
