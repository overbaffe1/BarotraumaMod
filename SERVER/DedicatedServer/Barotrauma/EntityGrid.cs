using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200022C RID: 556
	internal class EntityGrid
	{
		// Token: 0x17000AED RID: 2797
		// (get) Token: 0x0600260C RID: 9740 RVA: 0x000F720C File Offset: 0x000F540C
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

		// Token: 0x0600260D RID: 9741 RVA: 0x000F7280 File Offset: 0x000F5480
		public EntityGrid(Submarine submarine, float cellSize)
		{
			int padding = 128;
			this.limits = new Rectangle(submarine.Borders.X - padding, submarine.Borders.Y + padding, submarine.Borders.Width + padding * 2, submarine.Borders.Height + padding * 2);
			this.Submarine = submarine;
			this.cellSize = cellSize;
			this.InitializeGrid();
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x000F72F0 File Offset: 0x000F54F0
		public EntityGrid(Rectangle worldRect, float cellSize)
		{
			this.limits = worldRect;
			this.cellSize = cellSize;
			this.InitializeGrid();
		}

		// Token: 0x0600260F RID: 9743 RVA: 0x000F730C File Offset: 0x000F550C
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

		// Token: 0x06002610 RID: 9744 RVA: 0x000F73A4 File Offset: 0x000F55A4
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

		// Token: 0x06002611 RID: 9745 RVA: 0x000F74A0 File Offset: 0x000F56A0
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

		// Token: 0x06002612 RID: 9746 RVA: 0x000F7514 File Offset: 0x000F5714
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

		// Token: 0x06002613 RID: 9747 RVA: 0x000F756C File Offset: 0x000F576C
		public IEnumerable<MapEntity> GetAllEntities()
		{
			return this.allEntities;
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x000F7574 File Offset: 0x000F5774
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

		// Token: 0x06002615 RID: 9749 RVA: 0x000F7604 File Offset: 0x000F5804
		public Rectangle GetIndices(Rectangle rect)
		{
			Rectangle indices = Rectangle.Empty;
			indices.X = (int)Math.Floor((double)((float)(rect.X - this.limits.X) / this.cellSize));
			indices.Y = (int)Math.Floor((double)((float)(this.limits.Y - rect.Y) / this.cellSize));
			indices.Width = (int)Math.Floor((double)((float)(rect.Right - this.limits.X) / this.cellSize));
			indices.Height = (int)Math.Floor((double)((float)(this.limits.Y - (rect.Y - rect.Height)) / this.cellSize));
			return indices;
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x000F76C0 File Offset: 0x000F58C0
		public Point GetIndices(Vector2 position)
		{
			return new Point((int)Math.Floor((double)((position.X - (float)this.limits.X) / this.cellSize)), (int)Math.Floor((double)(((float)this.limits.Y - position.Y) / this.cellSize)));
		}

		// Token: 0x04001285 RID: 4741
		private List<MapEntity> allEntities;

		// Token: 0x04001286 RID: 4742
		private List<MapEntity>[,] entities;

		// Token: 0x04001287 RID: 4743
		private readonly Rectangle limits;

		// Token: 0x04001288 RID: 4744
		private readonly float cellSize;

		// Token: 0x04001289 RID: 4745
		public readonly Submarine Submarine;
	}
}
