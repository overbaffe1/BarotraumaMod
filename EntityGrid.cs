using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000314 RID: 788
	internal class EntityGrid
	{
		// Token: 0x17001085 RID: 4229
		// (get) Token: 0x06003EF3 RID: 16115 RVA: 0x00234BC4 File Offset: 0x00232DC4
		public Rectangle WorldRect
		{
			get
			{
				if (this.Submarine == null)
				{
					return this.limits;
				}
				return new Rectangle((int)((float)this.limits.X + this.Submarine.WorldPosition.X), (int)((float)this.limits.Y + this.Submarine.WorldPosition.Y), this.limits.Width, this.limits.Height);
			}
		}

		// Token: 0x06003EF4 RID: 16116 RVA: 0x00234C38 File Offset: 0x00232E38
		public EntityGrid(Submarine submarine, float cellSize)
		{
			int padding = 128;
			this.limits = new Rectangle(submarine.Borders.X - padding, submarine.Borders.Y + padding, submarine.Borders.Width + padding * 2, submarine.Borders.Height + padding * 2);
			this.Submarine = submarine;
			this.cellSize = cellSize;
			this.InitializeGrid();
		}

		// Token: 0x06003EF5 RID: 16117 RVA: 0x00234CA8 File Offset: 0x00232EA8
		public EntityGrid(Rectangle worldRect, float cellSize)
		{
			this.limits = worldRect;
			this.cellSize = cellSize;
			this.InitializeGrid();
		}

		// Token: 0x06003EF6 RID: 16118 RVA: 0x00234CC4 File Offset: 0x00232EC4
		private void InitializeGrid()
		{
			this.allEntities = new List<MapEntity>();
			this.entities = new List<MapEntity>[(int)Math.Ceiling((double)((float)this.limits.Width / this.cellSize)), (int)Math.Ceiling((double)((float)this.limits.Height / this.cellSize))];
			for (int x = 0; x < this.entities.GetLength(0); x++)
			{
				for (int y = 0; y < this.entities.GetLength(1); y++)
				{
					this.entities[x, y] = new List<MapEntity>();
				}
			}
		}

		// Token: 0x06003EF7 RID: 16119 RVA: 0x00234D5C File Offset: 0x00232F5C
		public void InsertEntity(MapEntity entity)
		{
			Rectangle rect = entity.Rect;
			Rectangle indices = this.GetIndices(rect);
			if (indices.Width < 0 || indices.X >= this.entities.GetLength(0) || indices.Height < 0 || indices.Y >= this.entities.GetLength(1))
			{
				DebugConsole.ThrowError("Error in EntityGrid.InsertEntity: " + ((entity != null) ? entity.ToString() : null) + " is outside the grid", null, null, false, false);
				return;
			}
			for (int x = Math.Max(indices.X, 0); x <= Math.Min(indices.Width, this.entities.GetLength(0) - 1); x++)
			{
				for (int y = Math.Max(indices.Y, 0); y <= Math.Min(indices.Height, this.entities.GetLength(1) - 1); y++)
				{
					this.entities[x, y].Add(entity);
				}
			}
			this.allEntities.Add(entity);
		}

		// Token: 0x06003EF8 RID: 16120 RVA: 0x00234E58 File Offset: 0x00233058
		public void RemoveEntity(MapEntity entity)
		{
			for (int x = 0; x < this.entities.GetLength(0); x++)
			{
				for (int y = 0; y < this.entities.GetLength(1); y++)
				{
					if (this.entities[x, y].Contains(entity))
					{
						this.entities[x, y].Remove(entity);
					}
				}
			}
			this.allEntities.Remove(entity);
		}

		// Token: 0x06003EF9 RID: 16121 RVA: 0x00234ECC File Offset: 0x002330CC
		public void Clear()
		{
			for (int x = 0; x < this.entities.GetLength(0); x++)
			{
				for (int y = 0; y < this.entities.GetLength(1); y++)
				{
					this.entities[x, y].Clear();
				}
			}
			this.allEntities.Clear();
		}

		// Token: 0x06003EFA RID: 16122 RVA: 0x00234F24 File Offset: 0x00233124
		public IEnumerable<MapEntity> GetAllEntities()
		{
			return this.allEntities;
		}

		// Token: 0x06003EFB RID: 16123 RVA: 0x00234F2C File Offset: 0x0023312C
		public List<MapEntity> GetEntities(Vector2 position)
		{
			if (!MathUtils.IsValid(position))
			{
				return null;
			}
			if (this.Submarine != null)
			{
				position -= this.Submarine.HiddenSubPosition;
			}
			Point indices = this.GetIndices(position);
			if (indices.X < 0 || indices.Y < 0 || indices.X >= this.entities.GetLength(0) || indices.Y >= this.entities.GetLength(1))
			{
				return null;
			}
			return this.entities[indices.X, indices.Y];
		}

		// Token: 0x06003EFC RID: 16124 RVA: 0x00234FBC File Offset: 0x002331BC
		public Rectangle GetIndices(Rectangle rect)
		{
			Rectangle indices = Rectangle.Empty;
			indices.X = (int)Math.Floor((double)((float)(rect.X - this.limits.X) / this.cellSize));
			indices.Y = (int)Math.Floor((double)((float)(this.limits.Y - rect.Y) / this.cellSize));
			indices.Width = (int)Math.Floor((double)((float)(rect.Right - this.limits.X) / this.cellSize));
			indices.Height = (int)Math.Floor((double)((float)(this.limits.Y - (rect.Y - rect.Height)) / this.cellSize));
			return indices;
		}

		// Token: 0x06003EFD RID: 16125 RVA: 0x00235078 File Offset: 0x00233278
		public Point GetIndices(Vector2 position)
		{
			return new Point((int)Math.Floor((double)((position.X - (float)this.limits.X) / this.cellSize)), (int)Math.Floor((double)(((float)this.limits.Y - position.Y) / this.cellSize)));
		}

		// Token: 0x040020B1 RID: 8369
		private List<MapEntity> allEntities;

		// Token: 0x040020B2 RID: 8370
		private List<MapEntity>[,] entities;

		// Token: 0x040020B3 RID: 8371
		private readonly Rectangle limits;

		// Token: 0x040020B4 RID: 8372
		private readonly float cellSize;

		// Token: 0x040020B5 RID: 8373
		public readonly Submarine Submarine;
	}
}
