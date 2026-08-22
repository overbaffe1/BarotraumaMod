using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005EA RID: 1514
	[NullableContext(2)]
	[Nullable(0)]
	internal class VineTile
	{
		// Token: 0x17001918 RID: 6424
		// (get) Token: 0x06006337 RID: 25399 RVA: 0x0033B28A File Offset: 0x0033948A
		// (set) Token: 0x06006338 RID: 25400 RVA: 0x0033B294 File Offset: 0x00339494
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

		// Token: 0x06006339 RID: 25401 RVA: 0x0033B2F8 File Offset: 0x003394F8
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

		// Token: 0x0600633A RID: 25402 RVA: 0x0033B46C File Offset: 0x0033966C
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

		// Token: 0x0600633B RID: 25403 RVA: 0x0033B595 File Offset: 0x00339795
		[NullableContext(1)]
		public Vector2 GetWorldPosition(Planter planter, Vector2 slotOffset)
		{
			return planter.Item.WorldPosition + slotOffset + this.Position;
		}

		// Token: 0x0600633C RID: 25404 RVA: 0x0033B5B3 File Offset: 0x003397B3
		public void UpdateType()
		{
			if (this.Type == VineTileType.Stem)
			{
				return;
			}
			this.Type = (VineTileType)this.Sides;
		}

		// Token: 0x0600633D RID: 25405 RVA: 0x0033B5CC File Offset: 0x003397CC
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

		// Token: 0x0600633E RID: 25406 RVA: 0x0033B6BE File Offset: 0x003398BE
		public bool CanGrowMore()
		{
			return (this.Sides | this.BlockedSides).Count() < 4;
		}

		// Token: 0x0600633F RID: 25407 RVA: 0x0033B6D5 File Offset: 0x003398D5
		public bool IsSideBlocked(TileSide side)
		{
			return this.BlockedSides.HasFlag(side) || this.Sides.HasFlag(side);
		}

		// Token: 0x06006340 RID: 25408 RVA: 0x0033B707 File Offset: 0x00339907
		public static Rectangle CreatePlantRect(Vector2 pos)
		{
			return new Rectangle((int)pos.X - VineTile.Size / 2, (int)pos.Y + VineTile.Size / 2, VineTile.Size, VineTile.Size);
		}

		// Token: 0x0400336B RID: 13163
		public TileSide Sides;

		// Token: 0x0400336C RID: 13164
		public TileSide BlockedSides;

		// Token: 0x0400336D RID: 13165
		public FoliageConfig FlowerConfig;

		// Token: 0x0400336E RID: 13166
		public FoliageConfig LeafConfig;

		// Token: 0x0400336F RID: 13167
		public int FailedGrowthAttempts;

		// Token: 0x04003370 RID: 13168
		public Rectangle Rect;

		// Token: 0x04003371 RID: 13169
		public Vector2 Position;

		// Token: 0x04003372 RID: 13170
		private readonly float diameter;

		// Token: 0x04003373 RID: 13171
		public Vector2 offset;

		// Token: 0x04003374 RID: 13172
		public VineTileType Type;

		// Token: 0x04003375 RID: 13173
		[Nullable(1)]
		public readonly Dictionary<TileSide, Vector2> AdjacentPositions;

		// Token: 0x04003376 RID: 13174
		public static int Size = 32;

		// Token: 0x04003377 RID: 13175
		public float VineStep;

		// Token: 0x04003378 RID: 13176
		public float FlowerStep;

		// Token: 0x04003379 RID: 13177
		private float growthStep;

		// Token: 0x0400337A RID: 13178
		public Color HealthColor = Color.Transparent;

		// Token: 0x0400337B RID: 13179
		public float DecayDelay;

		// Token: 0x0400337C RID: 13180
		private readonly Growable Parent;
	}
}
