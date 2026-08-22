using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B7 RID: 1207
	[NullableContext(2)]
	[Nullable(0)]
	internal class VineTile
	{
		// Token: 0x17001246 RID: 4678
		// (get) Token: 0x06004493 RID: 17555 RVA: 0x001B768A File Offset: 0x001B588A
		// (set) Token: 0x06004494 RID: 17556 RVA: 0x001B7694 File Offset: 0x001B5894
		public float GrowthStep
		{
			get
			{
				return this.growthStep;
			}
			set
			{
				this.growthStep = value;
				this.VineStep = Math.Min((float)Math.Pow((double)value, 2.0), 1f);
				if (value > 1f)
				{
					this.FlowerStep = Math.Min((float)Math.Pow((double)(value - 1f), 2.0), 1f);
				}
			}
		}

		// Token: 0x06004495 RID: 17557 RVA: 0x001B76F8 File Offset: 0x001B58F8
		public VineTile(Growable parent, Vector2 position, VineTileType type, FoliageConfig? flowerConfig = null, FoliageConfig? leafConfig = null, Rectangle? rect = null)
		{
			this.FlowerConfig = (flowerConfig ?? FoliageConfig.EmptyConfig);
			this.LeafConfig = (leafConfig ?? FoliageConfig.EmptyConfig);
			this.Position = position;
			this.Rect = (rect ?? VineTile.CreatePlantRect(position));
			this.Parent = parent;
			this.Type = type;
			this.diameter = (float)this.Rect.Width / 2f;
			this.AdjacentPositions = new Dictionary<TileSide, Vector2>
			{
				{
					TileSide.Top,
					new Vector2(this.Position.X, this.Position.Y + (float)this.Rect.Height)
				},
				{
					TileSide.Bottom,
					new Vector2(this.Position.X, this.Position.Y - (float)this.Rect.Height)
				},
				{
					TileSide.Left,
					new Vector2(this.Position.X - (float)this.Rect.Width, this.Position.Y)
				},
				{
					TileSide.Right,
					new Vector2(this.Position.X + (float)this.Rect.Width, this.Position.Y)
				}
			};
		}

		// Token: 0x06004496 RID: 17558 RVA: 0x001B786C File Offset: 0x001B5A6C
		public void UpdateScale(float deltaTime)
		{
			Growable parent = this.Parent;
			bool decayed = parent != null && parent.Decayed;
			if (decayed && this.GrowthStep > 1f)
			{
				if (this.DecayDelay > 0f)
				{
					this.DecayDelay -= deltaTime;
				}
				else
				{
					this.GrowthStep -= 0.25f * deltaTime;
				}
			}
			if (this.GrowthStep >= 2f || decayed)
			{
				return;
			}
			this.GrowthStep += deltaTime;
			if (this.GrowthStep < 1f)
			{
				float offsetAmount = this.diameter * this.VineStep - this.diameter;
				switch (this.Type)
				{
				case VineTileType.Stem:
				case VineTileType.StumpBottom:
					this.offset.Y = -offsetAmount;
					return;
				case VineTileType.StumpTop:
					this.offset.Y = offsetAmount;
					return;
				case VineTileType.StumpLeft:
					this.offset.X = offsetAmount;
					return;
				case VineTileType.StumpRight:
					this.offset.X = -offsetAmount;
					return;
				}
				this.offset = Vector2.Zero;
				return;
			}
			this.offset = Vector2.Zero;
		}

		// Token: 0x06004497 RID: 17559 RVA: 0x001B7995 File Offset: 0x001B5B95
		[NullableContext(1)]
		public Vector2 GetWorldPosition(Planter planter, Vector2 slotOffset)
		{
			return planter.Item.WorldPosition + slotOffset + this.Position;
		}

		// Token: 0x06004498 RID: 17560 RVA: 0x001B79B3 File Offset: 0x001B5BB3
		public void UpdateType()
		{
			if (this.Type == VineTileType.Stem)
			{
				return;
			}
			this.Type = (VineTileType)this.Sides;
		}

		// Token: 0x06004499 RID: 17561 RVA: 0x001B79CC File Offset: 0x001B5BCC
		public TileSide GetRandomFreeSide(Random random = null)
		{
			TileSide occupiedSides = this.Sides | this.BlockedSides;
			int setBits = occupiedSides.Count();
			if (setBits >= 4)
			{
				return TileSide.None;
			}
			int possible = 4 - setBits;
			int[] pool = new int[possible];
			int k = 0;
			int j = 0;
			while (k < 4)
			{
				if (!occupiedSides.HasFlag((TileSide)(1 << k)))
				{
					pool[j] = k;
					j++;
				}
				k++;
			}
			int value;
			if (this.Parent == null)
			{
				value = pool[Growable.RandomInt(0, possible, random)];
			}
			else
			{
				float num;
				float num2;
				float num3;
				float num4;
				this.Parent.GrowthWeights.Deconstruct(out num, out num2, out num3, out num4);
				float x = num;
				float y = num2;
				float z = num3;
				float w = num4;
				float[] weights = new float[]
				{
					x,
					y,
					z,
					w
				};
				value = pool.GetRandomByWeight((int i) => weights[i], Rand.RandSync.Unsynced);
			}
			return (TileSide)(1 << value);
		}

		// Token: 0x0600449A RID: 17562 RVA: 0x001B7ABE File Offset: 0x001B5CBE
		public bool CanGrowMore()
		{
			return (this.Sides | this.BlockedSides).Count() < 4;
		}

		// Token: 0x0600449B RID: 17563 RVA: 0x001B7AD5 File Offset: 0x001B5CD5
		public bool IsSideBlocked(TileSide side)
		{
			return this.BlockedSides.HasFlag(side) || this.Sides.HasFlag(side);
		}

		// Token: 0x0600449C RID: 17564 RVA: 0x001B7B07 File Offset: 0x001B5D07
		public static Rectangle CreatePlantRect(Vector2 pos)
		{
			return new Rectangle((int)pos.X - VineTile.Size / 2, (int)pos.Y + VineTile.Size / 2, VineTile.Size, VineTile.Size);
		}

		// Token: 0x040020E3 RID: 8419
		public TileSide Sides;

		// Token: 0x040020E4 RID: 8420
		public TileSide BlockedSides;

		// Token: 0x040020E5 RID: 8421
		public FoliageConfig FlowerConfig;

		// Token: 0x040020E6 RID: 8422
		public FoliageConfig LeafConfig;

		// Token: 0x040020E7 RID: 8423
		public int FailedGrowthAttempts;

		// Token: 0x040020E8 RID: 8424
		public Rectangle Rect;

		// Token: 0x040020E9 RID: 8425
		public Vector2 Position;

		// Token: 0x040020EA RID: 8426
		private readonly float diameter;

		// Token: 0x040020EB RID: 8427
		public Vector2 offset;

		// Token: 0x040020EC RID: 8428
		public VineTileType Type;

		// Token: 0x040020ED RID: 8429
		[Nullable(1)]
		public readonly Dictionary<TileSide, Vector2> AdjacentPositions;

		// Token: 0x040020EE RID: 8430
		public static int Size = 32;

		// Token: 0x040020EF RID: 8431
		public float VineStep;

		// Token: 0x040020F0 RID: 8432
		public float FlowerStep;

		// Token: 0x040020F1 RID: 8433
		private float growthStep;

		// Token: 0x040020F2 RID: 8434
		public Color HealthColor = Color.Transparent;

		// Token: 0x040020F3 RID: 8435
		public float DecayDelay;

		// Token: 0x040020F4 RID: 8436
		private readonly Growable Parent;
	}
}
