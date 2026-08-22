using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000E1 RID: 225
	internal class LevelWall : IDisposable
	{
		// Token: 0x17000860 RID: 2144
		// (get) Token: 0x06001F24 RID: 7972 RVA: 0x00133B8C File Offset: 0x00131D8C
		// (set) Token: 0x06001F25 RID: 7973 RVA: 0x00133B94 File Offset: 0x00131D94
		public LevelWallVertexBuffer VertexBuffer { get; private set; }

		// Token: 0x17000861 RID: 2145
		// (get) Token: 0x06001F26 RID: 7974 RVA: 0x00133B9D File Offset: 0x00131D9D
		public VertexBuffer WallBuffer
		{
			get
			{
				return this.VertexBuffer.WallBuffer;
			}
		}

		// Token: 0x17000862 RID: 2146
		// (get) Token: 0x06001F27 RID: 7975 RVA: 0x00133BAA File Offset: 0x00131DAA
		public VertexBuffer WallEdgeBuffer
		{
			get
			{
				return this.VertexBuffer.WallEdgeBuffer;
			}
		}

		// Token: 0x17000863 RID: 2147
		// (get) Token: 0x06001F28 RID: 7976 RVA: 0x00133BB7 File Offset: 0x00131DB7
		public virtual float Alpha
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x00133BC0 File Offset: 0x00131DC0
		public Matrix GetTransform()
		{
			if (!this.Body.FixedRotation)
			{
				return Matrix.CreateRotationZ(this.Body.Rotation) * Matrix.CreateTranslation(new Vector3(ConvertUnits.ToDisplayUnits(this.Body.Position), 0f));
			}
			return Matrix.CreateTranslation(new Vector3(ConvertUnits.ToDisplayUnits(this.Body.Position), 0f));
		}

		// Token: 0x06001F2A RID: 7978 RVA: 0x00133C2E File Offset: 0x00131E2E
		public void SetWallVertices(VertexPositionColorTexture[] wallVertices, VertexPositionColorTexture[] wallEdgeVertices, Texture2D wallTexture, Texture2D edgeTexture)
		{
			if (this.VertexBuffer != null && !this.VertexBuffer.IsDisposed)
			{
				this.VertexBuffer.Dispose();
			}
			this.VertexBuffer = new LevelWallVertexBuffer(wallVertices, wallEdgeVertices, null, wallTexture, edgeTexture);
		}

		// Token: 0x06001F2B RID: 7979 RVA: 0x00133C64 File Offset: 0x00131E64
		public void GenerateVertices()
		{
			float zCoord = (this is DestructibleLevelWall) ? Rand.Range(0.9f, 1f, Rand.RandSync.Unsynced) : 0.9f;
			VertexPositionColor[] nonTexturedWallVerts = CaveGenerator.GenerateWallVertices(this.triangles, this.color, 0.9f).ToArray();
			VertexPositionColorTexture[] wallVerts = CaveGenerator.ConvertToTextured(nonTexturedWallVerts, this.level.GenerationParams.WallTextureSize);
			this.SetWallVertices(wallVerts, CaveGenerator.GenerateWallEdgeVertices(this.Cells, this.level.GenerationParams.WallEdgeExpandOutwardsAmount, this.level.GenerationParams.WallEdgeExpandInwardsAmount, this.color, this.color, this.level, zCoord, false).ToArray(), this.level.GenerationParams.WallSprite.Texture, this.level.GenerationParams.WallEdgeSprite.Texture);
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x00133D3C File Offset: 0x00131F3C
		public bool IsVisible(Rectangle worldView)
		{
			RectangleF worldViewInSimUnits = new RectangleF(ConvertUnits.ToSimUnits(worldView.Location.ToVector2()), ConvertUnits.ToSimUnits(worldView.Size.ToVector2()));
			foreach (Fixture fixture in this.Body.FixtureList)
			{
				AABB aabb;
				fixture.GetAABB(out aabb, 0);
				Vector2 lowerBound = aabb.LowerBound + this.Body.Position;
				if (lowerBound.X <= worldViewInSimUnits.Right && lowerBound.Y <= worldViewInSimUnits.Y)
				{
					Vector2 upperBound = aabb.UpperBound + this.Body.Position;
					if (upperBound.X >= worldViewInSimUnits.X && upperBound.Y >= worldViewInSimUnits.Y - worldViewInSimUnits.Height)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x17000864 RID: 2148
		// (get) Token: 0x06001F2D RID: 7981 RVA: 0x00133E4C File Offset: 0x0013204C
		// (set) Token: 0x06001F2E RID: 7982 RVA: 0x00133E54 File Offset: 0x00132054
		public List<VoronoiCell> Cells { get; private set; }

		// Token: 0x17000865 RID: 2149
		// (get) Token: 0x06001F2F RID: 7983 RVA: 0x00133E5D File Offset: 0x0013205D
		// (set) Token: 0x06001F30 RID: 7984 RVA: 0x00133E65 File Offset: 0x00132065
		public Body Body { get; private set; }

		// Token: 0x17000866 RID: 2150
		// (get) Token: 0x06001F31 RID: 7985 RVA: 0x00133E6E File Offset: 0x0013206E
		// (set) Token: 0x06001F32 RID: 7986 RVA: 0x00133E76 File Offset: 0x00132076
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

		// Token: 0x17000867 RID: 2151
		// (get) Token: 0x06001F33 RID: 7987 RVA: 0x00133E90 File Offset: 0x00132090
		// (set) Token: 0x06001F34 RID: 7988 RVA: 0x00133E98 File Offset: 0x00132098
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

		// Token: 0x17000868 RID: 2152
		// (get) Token: 0x06001F35 RID: 7989 RVA: 0x00133ED5 File Offset: 0x001320D5
		// (set) Token: 0x06001F36 RID: 7990 RVA: 0x00133EDD File Offset: 0x001320DD
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

		// Token: 0x06001F37 RID: 7991 RVA: 0x00133EF8 File Offset: 0x001320F8
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
				this.GenerateVertices();
			}
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x0013403C File Offset: 0x0013223C
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
			this.GenerateVertices();
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x001341B4 File Offset: 0x001323B4
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

		// Token: 0x06001F3A RID: 7994 RVA: 0x001342D4 File Offset: 0x001324D4
		public bool IsPointInside(Vector2 point)
		{
			return this.Cells.Any((VoronoiCell c) => c.IsPointInside(point));
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x00134305 File Offset: 0x00132505
		public void Dispose()
		{
			LevelWallVertexBuffer vertexBuffer = this.VertexBuffer;
			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
			}
			this.VertexBuffer = null;
		}

		// Token: 0x04000FF4 RID: 4084
		protected readonly Level level;

		// Token: 0x04000FF5 RID: 4085
		private readonly List<Vector2[]> triangles;

		// Token: 0x04000FF6 RID: 4086
		private readonly Color color;

		// Token: 0x04000FF7 RID: 4087
		private float moveState;

		// Token: 0x04000FF8 RID: 4088
		private float moveLength;

		// Token: 0x04000FF9 RID: 4089
		private Vector2 moveAmount;

		// Token: 0x04000FFA RID: 4090
		private float wallDamageOnTouch;

		// Token: 0x04000FFB RID: 4091
		public float MoveSpeed;

		// Token: 0x04000FFC RID: 4092
		private Vector2? originalPos;
	}
}
