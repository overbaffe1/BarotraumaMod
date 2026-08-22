using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000240 RID: 576
	internal class LevelWall : IDisposable
	{
		// Token: 0x17000BDD RID: 3037
		// (get) Token: 0x06002875 RID: 10357 RVA: 0x00106252 File Offset: 0x00104452
		// (set) Token: 0x06002876 RID: 10358 RVA: 0x0010625A File Offset: 0x0010445A
		public List<VoronoiCell> Cells { get; private set; }

		// Token: 0x17000BDE RID: 3038
		// (get) Token: 0x06002877 RID: 10359 RVA: 0x00106263 File Offset: 0x00104463
		// (set) Token: 0x06002878 RID: 10360 RVA: 0x0010626B File Offset: 0x0010446B
		public Body Body { get; private set; }

		// Token: 0x17000BDF RID: 3039
		// (get) Token: 0x06002879 RID: 10361 RVA: 0x00106274 File Offset: 0x00104474
		// (set) Token: 0x0600287A RID: 10362 RVA: 0x0010627C File Offset: 0x0010447C
		public Vector2 MoveAmount
		{
			get
			{
				return this.moveAmount;
			}
			set
			{
				this.moveAmount = value;
				this.moveLength = this.moveAmount.Length();
			}
		}

		// Token: 0x17000BE0 RID: 3040
		// (get) Token: 0x0600287B RID: 10363 RVA: 0x00106296 File Offset: 0x00104496
		// (set) Token: 0x0600287C RID: 10364 RVA: 0x001062A0 File Offset: 0x001044A0
		public float WallDamageOnTouch
		{
			get
			{
				return this.wallDamageOnTouch;
			}
			set
			{
				this.Cells.ForEach(delegate(VoronoiCell c)
				{
					c.DoesDamage = !MathUtils.NearlyEqual(value, 0f, 0.0001f);
				});
				this.wallDamageOnTouch = value;
			}
		}

		// Token: 0x17000BE1 RID: 3041
		// (get) Token: 0x0600287D RID: 10365 RVA: 0x001062DD File Offset: 0x001044DD
		// (set) Token: 0x0600287E RID: 10366 RVA: 0x001062E5 File Offset: 0x001044E5
		public float MoveState
		{
			get
			{
				return this.moveState;
			}
			set
			{
				this.moveState = MathHelper.Clamp(value, 0f, 6.2831855f);
			}
		}

		// Token: 0x0600287F RID: 10367 RVA: 0x00106300 File Offset: 0x00104500
		public LevelWall(List<Vector2> vertices, Color color, Level level, bool giftWrap = false, bool createBody = true)
		{
			this.level = level;
			this.color = color;
			List<Vector2> originalVertices = new List<Vector2>(vertices);
			if (giftWrap)
			{
				vertices = MathUtils.GiftWrap(vertices);
			}
			if (vertices.Count < 3)
			{
				throw new ArgumentException("Failed to generate a wall (not enough vertices). Original vertices: " + string.Join(", ", from v in originalVertices
				select v.ToString()));
			}
			VoronoiCell wallCell = new VoronoiCell(vertices.ToArray());
			for (int i = 0; i < wallCell.Edges.Count; i++)
			{
				wallCell.Edges[i].Cell1 = wallCell;
				wallCell.Edges[i].IsSolid = true;
			}
			this.Cells = new List<VoronoiCell>
			{
				wallCell
			};
			if (createBody)
			{
				this.Body = CaveGenerator.GeneratePolygons(this.Cells, level, out this.triangles);
				if (this.triangles.Count == 0)
				{
					throw new ArgumentException("Failed to generate a wall (not enough triangles). Original vertices: " + string.Join(", ", from v in originalVertices
					select v.ToString()));
				}
			}
		}

		// Token: 0x06002880 RID: 10368 RVA: 0x0010643C File Offset: 0x0010463C
		public LevelWall(List<Vector2> edgePositions, Vector2 extendAmount, Color color, Level level)
		{
			this.level = level;
			this.color = color;
			this.Cells = new List<VoronoiCell>();
			for (int i = 0; i < edgePositions.Count - 1; i++)
			{
				Vector2[] vertices = new Vector2[4];
				vertices[0] = edgePositions[i];
				vertices[1] = edgePositions[i + 1];
				vertices[2] = vertices[1] + extendAmount;
				vertices[3] = vertices[0] + extendAmount;
				VoronoiCell wallCell = new VoronoiCell(vertices)
				{
					CellType = CellType.Solid
				};
				wallCell.Edges[0].Cell1 = wallCell;
				wallCell.Edges[1].Cell1 = wallCell;
				wallCell.Edges[2].Cell1 = wallCell;
				wallCell.Edges[3].Cell1 = wallCell;
				wallCell.Edges[0].IsSolid = true;
				if (i > 1)
				{
					wallCell.Edges[3].Cell2 = this.Cells[i - 1];
					this.Cells[i - 1].Edges[1].Cell2 = wallCell;
				}
				this.Cells.Add(wallCell);
			}
			this.Body = CaveGenerator.GeneratePolygons(this.Cells, level, out this.triangles);
			this.Body.CollisionCategories = Category.Cat8;
		}

		// Token: 0x06002881 RID: 10369 RVA: 0x001065B0 File Offset: 0x001047B0
		public virtual void Update(float deltaTime)
		{
			if (this.Body.BodyType == BodyType.Static)
			{
				return;
			}
			Vector2 bodyPos = ConvertUnits.ToDisplayUnits(this.Body.Position);
			this.Cells.ForEach(delegate(VoronoiCell c)
			{
				c.Translation = bodyPos;
			});
			if (this.originalPos == null)
			{
				this.originalPos = new Vector2?(bodyPos);
			}
			if (this.moveLength > 0f && this.MoveSpeed > 0f)
			{
				this.moveState += this.MoveSpeed / this.moveLength * deltaTime;
				this.moveState %= 6.2831855f;
				Vector2 targetPos = ConvertUnits.ToSimUnits(this.originalPos.Value + this.moveAmount * (float)Math.Sin((double)this.moveState));
				this.Body.ApplyForce((targetPos - this.Body.Position).ClampLength(1f) * this.Body.Mass);
			}
		}

		// Token: 0x06002882 RID: 10370 RVA: 0x001066D0 File Offset: 0x001048D0
		public bool IsPointInside(Vector2 point)
		{
			return this.Cells.Any((VoronoiCell c) => c.IsPointInside(point));
		}

		// Token: 0x06002883 RID: 10371 RVA: 0x00106701 File Offset: 0x00104901
		public void Dispose()
		{
		}

		// Token: 0x040013E5 RID: 5093
		protected readonly Level level;

		// Token: 0x040013E6 RID: 5094
		private readonly List<Vector2[]> triangles;

		// Token: 0x040013E7 RID: 5095
		private readonly Color color;

		// Token: 0x040013E8 RID: 5096
		private float moveState;

		// Token: 0x040013E9 RID: 5097
		private float moveLength;

		// Token: 0x040013EA RID: 5098
		private Vector2 moveAmount;

		// Token: 0x040013EB RID: 5099
		private float wallDamageOnTouch;

		// Token: 0x040013EC RID: 5100
		public float MoveSpeed;

		// Token: 0x040013ED RID: 5101
		private Vector2? originalPos;
	}
}
