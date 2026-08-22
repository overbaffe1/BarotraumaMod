using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AC RID: 1452
	[NullableContext(1)]
	[Nullable(0)]
	internal class Growable : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x0600587F RID: 22655 RVA: 0x002DB5BC File Offset: 0x002D97BC
		protected override void RemoveComponentSpecific()
		{
			Sprite vineAtlas = this.VineAtlas;
			if (vineAtlas != null)
			{
				vineAtlas.Remove();
			}
			Sprite decayAtlas = this.DecayAtlas;
			if (decayAtlas != null)
			{
				decayAtlas.Remove();
			}
			foreach (Sprite sprite in this.FlowerSprites)
			{
				sprite.Remove();
			}
			foreach (Sprite sprite2 in this.LeafSprites)
			{
				sprite2.Remove();
			}
		}

		// Token: 0x06005880 RID: 22656 RVA: 0x002DB674 File Offset: 0x002D9874
		public void Draw(SpriteBatch spriteBatch, Planter planter, Vector2 offset, float depth)
		{
			float leafDepth = 0f;
			foreach (VineTile vine in this.Vines)
			{
				leafDepth += 0.0001f;
				this.DrawBranch(vine, spriteBatch, planter.Item.DrawPosition + offset, depth, leafDepth);
			}
			if (GameMain.DebugDraw)
			{
				foreach (Rectangle rect in this.FailedRectangles)
				{
					Rectangle wRect = rect;
					wRect.Y = -wRect.Y;
					wRect.Y -= wRect.Height;
					GUI.DrawRectangle(spriteBatch, wRect, Color.Red, false, 0f, 1f);
				}
			}
		}

		// Token: 0x06005881 RID: 22657 RVA: 0x002DB76C File Offset: 0x002D996C
		private void DrawBranch(VineTile vine, SpriteBatch spriteBatch, Vector2 position, float depth, float leafDepth)
		{
			Vector2 pos = position + vine.Position;
			pos.Y = -pos.Y;
			VineSprite vineSprite = this.VineSprites[vine.Type];
			Color color = this.Decayed ? this.DeadTint : this.VineTint;
			float layer = depth + 0.01f;
			float layer2 = depth + 0.02f;
			float layer3 = depth + 0.03f;
			float scale = this.VineScale * vine.VineStep;
			if (this.VineAtlas != null && this.VineAtlas.Loaded)
			{
				spriteBatch.Draw(this.VineAtlas.Texture, pos + vine.offset, new Rectangle?(vineSprite.SourceRect), color, 0f, vineSprite.AbsoluteOrigin, scale, SpriteEffects.None, layer3);
			}
			if (this.DecayAtlas != null && this.DecayAtlas.Loaded)
			{
				spriteBatch.Draw(this.DecayAtlas.Texture, pos, new Rectangle?(vineSprite.SourceRect), vine.HealthColor, 0f, vineSprite.AbsoluteOrigin, scale, SpriteEffects.None, layer2);
			}
			if (vine.FlowerConfig.Variant >= 0 && !this.Decayed)
			{
				Sprite flowerSprite = this.FlowerSprites[vine.FlowerConfig.Variant];
				Sprite sprite = flowerSprite;
				Vector2 pos2 = pos;
				Color flowerTint = this.FlowerTint;
				Vector2 origin = flowerSprite.Origin;
				float scale2 = this.BaseFlowerScale * vine.FlowerConfig.Scale * vine.FlowerStep;
				sprite.Draw(spriteBatch, pos2, flowerTint, origin, vine.FlowerConfig.Rotation, scale2, SpriteEffects.None, new float?(layer));
			}
			if (vine.LeafConfig.Variant >= 0)
			{
				Sprite leafSprite = this.LeafSprites[vine.LeafConfig.Variant];
				Sprite sprite2 = leafSprite;
				Vector2 pos3 = pos;
				Color color2 = this.Decayed ? this.DeadTint : this.LeafTint;
				Vector2 origin2 = leafSprite.Origin;
				float scale2 = this.BaseLeafScale * vine.LeafConfig.Scale * vine.FlowerStep;
				sprite2.Draw(spriteBatch, pos3, color2, origin2, vine.LeafConfig.Rotation, scale2, SpriteEffects.None, new float?(layer3 + leafDepth));
			}
		}

		// Token: 0x06005882 RID: 22658 RVA: 0x002DB974 File Offset: 0x002D9B74
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.Health = msg.ReadRangedSingle(0f, this.MaxWater, 8);
			int startOffset = msg.ReadRangedInteger(-1, this.MaximumVines);
			if (startOffset > -1)
			{
				int vineCount = msg.ReadRangedInteger(0, 32);
				List<VineTile> tiles = new List<VineTile>();
				for (int i = 0; i < vineCount; i++)
				{
					VineTileType vineType = (VineTileType)msg.ReadRangedInteger(0, 15);
					int flowerConfig = msg.ReadRangedInteger(0, 4095);
					int leafConfig = msg.ReadRangedInteger(0, 4095);
					sbyte posX = (sbyte)msg.ReadByte();
					sbyte posY = (sbyte)msg.ReadByte();
					Vector2 pos = new Vector2((float)((int)posX * VineTile.Size), (float)((int)posY * VineTile.Size));
					tiles.Add(new VineTile(this, pos, vineType, new FoliageConfig?(FoliageConfig.Deserialize(flowerConfig)), new FoliageConfig?(FoliageConfig.Deserialize(leafConfig)), null));
				}
				object obj = this.mutex;
				lock (obj)
				{
					for (int j = 0; j < vineCount; j++)
					{
						int index = j + startOffset;
						if (index >= this.Vines.Count)
						{
							this.Vines.Add(tiles[j]);
						}
						else
						{
							VineTile oldVine = this.Vines[index];
							VineTile newVine = tiles[j];
							newVine.GrowthStep = oldVine.GrowthStep;
							this.Vines[index] = newVine;
						}
					}
				}
			}
			this.UpdateBranchHealth();
			this.ResetPlanterSize();
		}

		// Token: 0x06005883 RID: 22659 RVA: 0x002DBB04 File Offset: 0x002D9D04
		private void ResetPlanterSize()
		{
			ItemInventory itemInventory = this.item.ParentInventory as ItemInventory;
			if (itemInventory != null)
			{
				Item parentItem = itemInventory.Owner as Item;
				if (parentItem != null)
				{
					Planter planter = parentItem.GetComponent<Planter>();
					if (planter != null)
					{
						planter.Item.ResetCachedVisibleSize();
					}
				}
			}
		}

		// Token: 0x17001617 RID: 5655
		// (get) Token: 0x06005884 RID: 22660 RVA: 0x002DBB49 File Offset: 0x002D9D49
		// (set) Token: 0x06005885 RID: 22661 RVA: 0x002DBB51 File Offset: 0x002D9D51
		[Serialize(1f, IsPropertySaveable.Yes, "How fast the plant grows. Value of 1 means a vine attempts to grow every 10 seconds while 2 and 0.5 mean every 5 and 20 seconds respectively.", "", false)]
		public float GrowthSpeed { get; set; }

		// Token: 0x17001618 RID: 5656
		// (get) Token: 0x06005886 RID: 22662 RVA: 0x002DBB5A File Offset: 0x002D9D5A
		// (set) Token: 0x06005887 RID: 22663 RVA: 0x002DBB62 File Offset: 0x002D9D62
		[Serialize(100f, IsPropertySaveable.Yes, "How much water the plant can hold. Affects how long the plant can survive without water.", "", false)]
		public float MaxWater { get; set; }

		// Token: 0x17001619 RID: 5657
		// (get) Token: 0x06005888 RID: 22664 RVA: 0x002DBB6B File Offset: 0x002D9D6B
		// (set) Token: 0x06005889 RID: 22665 RVA: 0x002DBB73 File Offset: 0x002D9D73
		[Serialize(1f, IsPropertySaveable.Yes, "How much extra water the plant uses per second while it is submerged in a flooded hull.", "", false)]
		public float ExtraWaterUsedPerSecondWhileFlooded { get; set; }

		// Token: 0x1700161A RID: 5658
		// (get) Token: 0x0600588A RID: 22666 RVA: 0x002DBB7C File Offset: 0x002D9D7C
		// (set) Token: 0x0600588B RID: 22667 RVA: 0x002DBB84 File Offset: 0x002D9D84
		[Serialize(1f, IsPropertySaveable.Yes, "How much water the plant consumes passively per second.", "", false)]
		public float WaterUsedPerSecond { get; set; }

		// Token: 0x1700161B RID: 5659
		// (get) Token: 0x0600588C RID: 22668 RVA: 0x002DBB8D File Offset: 0x002D9D8D
		// (set) Token: 0x0600588D RID: 22669 RVA: 0x002DBB95 File Offset: 0x002D9D95
		[Serialize(0.01f, IsPropertySaveable.Yes, "Percentage chance of a seed item being produced on growth ticks (every 10 seconds without a multiplier). 0.01 means 1% chance. Not used in vanilla plants.", "", false)]
		public float SeedSpawnChance { get; set; }

		// Token: 0x1700161C RID: 5660
		// (get) Token: 0x0600588E RID: 22670 RVA: 0x002DBB9E File Offset: 0x002D9D9E
		// (set) Token: 0x0600588F RID: 22671 RVA: 0x002DBBA6 File Offset: 0x002D9DA6
		[Serialize(0.01f, IsPropertySaveable.Yes, "How often a product item is produced on growth ticks (every 10 seconds without a multiplier). 0.01 means 1% chance.", "", false)]
		public float ProductSpawnChance { get; set; }

		// Token: 0x1700161D RID: 5661
		// (get) Token: 0x06005890 RID: 22672 RVA: 0x002DBBAF File Offset: 0x002D9DAF
		// (set) Token: 0x06005891 RID: 22673 RVA: 0x002DBBB7 File Offset: 0x002D9DB7
		[Serialize(0.5f, IsPropertySaveable.Yes, "Completely unused property that was added on the first design pass but due to the first pass being too complex was never used and now it is used by mods so it cannot be removed.", "", false)]
		public float MutationProbability { get; set; }

		// Token: 0x1700161E RID: 5662
		// (get) Token: 0x06005892 RID: 22674 RVA: 0x002DBBC0 File Offset: 0x002D9DC0
		// (set) Token: 0x06005893 RID: 22675 RVA: 0x002DBBC8 File Offset: 0x002D9DC8
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the flowers.", "", false)]
		public Color FlowerTint { get; set; }

		// Token: 0x1700161F RID: 5663
		// (get) Token: 0x06005894 RID: 22676 RVA: 0x002DBBD1 File Offset: 0x002D9DD1
		// (set) Token: 0x06005895 RID: 22677 RVA: 0x002DBBD9 File Offset: 0x002D9DD9
		[Serialize(3, IsPropertySaveable.Yes, "Number of flowers drawn.", "", false)]
		public int FlowerQuantity { get; set; }

		// Token: 0x17001620 RID: 5664
		// (get) Token: 0x06005896 RID: 22678 RVA: 0x002DBBE2 File Offset: 0x002D9DE2
		// (set) Token: 0x06005897 RID: 22679 RVA: 0x002DBBEA File Offset: 0x002D9DEA
		[Serialize(0.25f, IsPropertySaveable.Yes, "Size of the flower sprites.", "", false)]
		public float BaseFlowerScale { get; set; }

		// Token: 0x17001621 RID: 5665
		// (get) Token: 0x06005898 RID: 22680 RVA: 0x002DBBF3 File Offset: 0x002D9DF3
		// (set) Token: 0x06005899 RID: 22681 RVA: 0x002DBBFB File Offset: 0x002D9DFB
		[Serialize(0.5f, IsPropertySaveable.Yes, "Size of the leaf sprites.", "", false)]
		public float BaseLeafScale { get; set; }

		// Token: 0x17001622 RID: 5666
		// (get) Token: 0x0600589A RID: 22682 RVA: 0x002DBC04 File Offset: 0x002D9E04
		// (set) Token: 0x0600589B RID: 22683 RVA: 0x002DBC0C File Offset: 0x002D9E0C
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the leaves.", "", false)]
		public Color LeafTint { get; set; }

		// Token: 0x17001623 RID: 5667
		// (get) Token: 0x0600589C RID: 22684 RVA: 0x002DBC15 File Offset: 0x002D9E15
		// (set) Token: 0x0600589D RID: 22685 RVA: 0x002DBC1D File Offset: 0x002D9E1D
		[Serialize(0.33f, IsPropertySaveable.Yes, "Chance of a leaf appearing behind a branch.", "", false)]
		public float LeafProbability { get; set; }

		// Token: 0x17001624 RID: 5668
		// (get) Token: 0x0600589E RID: 22686 RVA: 0x002DBC26 File Offset: 0x002D9E26
		// (set) Token: 0x0600589F RID: 22687 RVA: 0x002DBC2E File Offset: 0x002D9E2E
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the vines.", "", false)]
		public Color VineTint { get; set; }

		// Token: 0x17001625 RID: 5669
		// (get) Token: 0x060058A0 RID: 22688 RVA: 0x002DBC37 File Offset: 0x002D9E37
		// (set) Token: 0x060058A1 RID: 22689 RVA: 0x002DBC3F File Offset: 0x002D9E3F
		[Serialize(32, IsPropertySaveable.Yes, "Maximum number of vine tiles the plant can grow.", "", false)]
		public int MaximumVines { get; set; }

		// Token: 0x17001626 RID: 5670
		// (get) Token: 0x060058A2 RID: 22690 RVA: 0x002DBC48 File Offset: 0x002D9E48
		// (set) Token: 0x060058A3 RID: 22691 RVA: 0x002DBC50 File Offset: 0x002D9E50
		[Serialize(0.25f, IsPropertySaveable.Yes, "Size of the vine sprites.", "", false)]
		public float VineScale { get; set; }

		// Token: 0x17001627 RID: 5671
		// (get) Token: 0x060058A4 RID: 22692 RVA: 0x002DBC59 File Offset: 0x002D9E59
		// (set) Token: 0x060058A5 RID: 22693 RVA: 0x002DBC61 File Offset: 0x002D9E61
		[Serialize("0.26,0.27,0.29,1.0", IsPropertySaveable.Yes, "Tint of a dead plant.", "", false)]
		public Color DeadTint { get; set; }

		// Token: 0x17001628 RID: 5672
		// (get) Token: 0x060058A6 RID: 22694 RVA: 0x002DBC6A File Offset: 0x002D9E6A
		// (set) Token: 0x060058A7 RID: 22695 RVA: 0x002DBC72 File Offset: 0x002D9E72
		[Serialize("1,1,1,1", IsPropertySaveable.Yes, "Probability for the plant to grow in a direction.", "", false)]
		public Vector4 GrowthWeights { get; set; }

		// Token: 0x17001629 RID: 5673
		// (get) Token: 0x060058A8 RID: 22696 RVA: 0x002DBC7B File Offset: 0x002D9E7B
		// (set) Token: 0x060058A9 RID: 22697 RVA: 0x002DBC83 File Offset: 0x002D9E83
		[Serialize(0f, IsPropertySaveable.Yes, "How much water is lost due to fires every 10 seconds.", "", false)]
		public float FireVulnerability { get; set; }

		// Token: 0x1700162A RID: 5674
		// (get) Token: 0x060058AA RID: 22698 RVA: 0x002DBC8C File Offset: 0x002D9E8C
		// (set) Token: 0x060058AB RID: 22699 RVA: 0x002DBC94 File Offset: 0x002D9E94
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Modifier to the percentage of product and seed items produced before the plant is fully grown based on how many vines have been grown. 0 would mean no products or seeds are produced while 0.5 would mean half of the normal amount.", "", false)]
		public Vector2 LinearProductAndSeedMultiplierBeforeFullyGrown { get; set; }

		// Token: 0x1700162B RID: 5675
		// (get) Token: 0x060058AC RID: 22700 RVA: 0x002DBC9D File Offset: 0x002D9E9D
		// (set) Token: 0x060058AD RID: 22701 RVA: 0x002DBCA5 File Offset: 0x002D9EA5
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		public float Health
		{
			get
			{
				return this.health;
			}
			set
			{
				this.health = Math.Clamp(value, 0f, this.MaxWater);
			}
		}

		// Token: 0x1700162C RID: 5676
		// (get) Token: 0x060058AE RID: 22702 RVA: 0x002DBCBE File Offset: 0x002D9EBE
		// (set) Token: 0x060058AF RID: 22703 RVA: 0x002DBCC6 File Offset: 0x002D9EC6
		public bool Decayed { get; set; }

		// Token: 0x1700162D RID: 5677
		// (get) Token: 0x060058B0 RID: 22704 RVA: 0x002DBCCF File Offset: 0x002D9ECF
		// (set) Token: 0x060058B1 RID: 22705 RVA: 0x002DBCD7 File Offset: 0x002D9ED7
		public bool FullyGrown { get; set; }

		// Token: 0x060058B2 RID: 22706 RVA: 0x002DBCE0 File Offset: 0x002D9EE0
		public Growable(Item item, ContentXElement element) : base(item, element)
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.MaxWater = element.GetAttributeFloat("maxhealth", this.MaxWater);
			this.WaterUsedPerSecond = element.GetAttributeFloat("hardiness", this.WaterUsedPerSecond);
			this.ExtraWaterUsedPerSecondWhileFlooded = element.GetAttributeFloat("floodtolerance", this.ExtraWaterUsedPerSecondWhileFlooded);
			this.ProductSpawnChance = element.GetAttributeFloat("productrate", this.ProductSpawnChance);
			this.SeedSpawnChance = element.GetAttributeFloat("seedrate", this.SeedSpawnChance);
			this.Health = this.MaxWater;
			if (element.HasElements)
			{
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "produceditem"))
					{
						if (a == "vinesprites")
						{
							this.LoadVines(subElement);
						}
					}
					else
					{
						this.ProducedItems.Add(new ProducedItem(this.item, subElement));
					}
				}
			}
			this.ProducedSeed = new ProducedItem(this.item, this.item.Prefab, 1f);
			this.flowerTiles = new int[this.FlowerQuantity];
		}

		// Token: 0x060058B3 RID: 22707 RVA: 0x002DBE90 File Offset: 0x002DA090
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.flowerTiles.All((int i) => i == 0))
			{
				this.GenerateFlowerTiles(null);
			}
		}

		// Token: 0x060058B4 RID: 22708 RVA: 0x002DBECC File Offset: 0x002DA0CC
		[NullableContext(2)]
		private void GenerateFlowerTiles(Random random = null)
		{
			this.flowerTiles = new int[this.FlowerQuantity];
			List<int> pool = new List<int>();
			for (int i = 0; i < this.MaximumVines - 1; i++)
			{
				pool.Add(i);
			}
			for (int j = 0; j < this.flowerTiles.Length; j++)
			{
				int index = Growable.RandomInt(0, pool.Count, random);
				this.flowerTiles[j] = pool[index];
				pool.RemoveAt(index);
			}
		}

		// Token: 0x060058B5 RID: 22709 RVA: 0x002DBF44 File Offset: 0x002DA144
		private void LoadVines(ContentXElement element)
		{
			ContentPath vineAtlasPath = element.GetAttributeContentPath("vineatlas") ?? ContentPath.Empty;
			ContentPath decayAtlasPath = element.GetAttributeContentPath("decayatlas") ?? ContentPath.Empty;
			if (!vineAtlasPath.IsNullOrEmpty())
			{
				this.VineAtlas = new Sprite(vineAtlasPath.Value, new Rectangle?(Rectangle.Empty), null, 0f);
			}
			if (!decayAtlasPath.IsNullOrEmpty())
			{
				this.DecayAtlas = new Sprite(decayAtlasPath.Value, new Rectangle?(Rectangle.Empty), null, 0f);
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "vinesprite"))
				{
					if (!(a == "flowersprite"))
					{
						if (a == "leafsprite")
						{
							this.LeafSprites.Add(new Sprite(subElement, "", "", false, 1f));
						}
					}
					else
					{
						this.FlowerSprites.Add(new Sprite(subElement, "", "", false, 1f));
					}
				}
				else
				{
					ContentXElement contentXElement = subElement;
					string key = "type";
					VineTileType vineTileType = VineTileType.Stem;
					VineTileType type = contentXElement.GetAttributeEnum<VineTileType>(key, vineTileType);
					this.VineSprites.Add(type, new VineSprite(subElement));
				}
				this.flowerVariants = this.FlowerSprites.Count;
				this.leafVariants = this.LeafSprites.Count;
			}
			foreach (VineTileType type2 in Enum.GetValues(typeof(VineTileType)).Cast<VineTileType>())
			{
				if (!this.VineSprites.ContainsKey(type2))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Vine sprite missing from ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendFormatted<VineTileType>(type2);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
		}

		// Token: 0x060058B6 RID: 22710 RVA: 0x002DC198 File Offset: 0x002DA398
		public void OnGrowthTick(Planter planter, PlantSlot slot)
		{
			if (this.Decayed)
			{
				return;
			}
			this.TryGenerateProduct(planter, slot);
			if (this.Health > 0f)
			{
				this.GrowVines(planter, slot);
				float multipler = (planter.Fertilizer > 0f) ? 0.5f : 1f;
				this.Health -= (this.accelerateDeath ? (this.WaterUsedPerSecond * 10f) : this.WaterUsedPerSecond) * multipler;
				if (planter.Item.InWater)
				{
					this.Health -= this.ExtraWaterUsedPerSecondWhileFlooded * multipler;
				}
			}
			this.CheckPlantState();
			this.UpdateBranchHealth();
		}

		// Token: 0x060058B7 RID: 22711 RVA: 0x002DC240 File Offset: 0x002DA440
		private void UpdateBranchHealth()
		{
			Color healthColor = Color.White * (1f - this.Health / this.MaxWater);
			foreach (VineTile vine in this.Vines)
			{
				vine.HealthColor = healthColor;
			}
		}

		// Token: 0x060058B8 RID: 22712 RVA: 0x002DC2B4 File Offset: 0x002DA4B4
		private void TryGenerateProduct(Planter planter, PlantSlot slot)
		{
			this.productDelay++;
			if (this.productDelay <= 10)
			{
				return;
			}
			this.productDelay = 0;
			float spawnChanceMultiplier = 1f;
			if (!this.FullyGrown)
			{
				if (this.LinearProductAndSeedMultiplierBeforeFullyGrown.NearlyEquals(Vector2.Zero))
				{
					return;
				}
				float growthProgress = (float)this.Vines.Count / (float)this.MaximumVines;
				spawnChanceMultiplier = MathHelper.Lerp(this.LinearProductAndSeedMultiplierBeforeFullyGrown.X, this.LinearProductAndSeedMultiplierBeforeFullyGrown.Y, growthProgress);
				if (MathUtils.NearlyEqual(spawnChanceMultiplier, 0f, 0.0001f))
				{
					return;
				}
			}
			bool spawnProduct = Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.ProductSpawnChance * spawnChanceMultiplier;
			bool spawnSeed = Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.SeedSpawnChance * spawnChanceMultiplier;
			if (!spawnProduct && !spawnSeed)
			{
				return;
			}
			VineTile vine = this.Vines.GetRandomUnsynced<VineTile>();
			Vector2 spawnPos = vine.GetWorldPosition(planter, slot.Offset);
			if (spawnProduct && this.ProducedItems.Any<ProducedItem>())
			{
				Growable.<TryGenerateProduct>g__SpawnItem|128_1(base.Item, this.ProducedItems.GetRandomByWeight((ProducedItem it) => it.Probability, Rand.RandSync.Unsynced), spawnPos);
				return;
			}
			if (spawnSeed)
			{
				Growable.<TryGenerateProduct>g__SpawnItem|128_1(base.Item, this.ProducedSeed, spawnPos);
			}
		}

		// Token: 0x060058B9 RID: 22713 RVA: 0x002DC400 File Offset: 0x002DA600
		private bool CheckPlantState()
		{
			if (this.Decayed)
			{
				return true;
			}
			if (this.Health <= 0f)
			{
				if (!this.Decayed)
				{
					string str = "MicroInteraction:";
					GameSession gameSession = GameMain.GameSession;
					string text;
					if (gameSession == null)
					{
						text = null;
					}
					else
					{
						GameMode gameMode = gameSession.GameMode;
						text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
					}
					GameAnalyticsManager.AddDesignEvent(str + (text ?? "null") + ":GardeningDied:" + this.item.Prefab.Identifier.ToString());
				}
				this.Decayed = true;
				foreach (VineTile vine in this.Vines)
				{
					vine.DecayDelay = (float)Growable.RandomDouble(0.0, 30.0, null);
				}
				return true;
			}
			if (this.Vines.Count >= this.MaximumVines && !this.FullyGrown)
			{
				this.FullyGrown = true;
				return true;
			}
			if (!this.FullyGrown && !this.accelerateDeath && this.Vines.Any<VineTile>())
			{
				if (this.Vines.All((VineTile tile) => !tile.CanGrowMore()))
				{
					this.accelerateDeath = true;
				}
			}
			if (this.item.ParentInventory is CharacterInventory)
			{
				this.Decayed = true;
				return true;
			}
			return false;
		}

		// Token: 0x060058BA RID: 22714 RVA: 0x002DC588 File Offset: 0x002DA788
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			this.UpdateFires(deltaTime);
			foreach (VineTile vine in this.Vines)
			{
				vine.UpdateScale(deltaTime);
			}
			this.CheckPlantState();
		}

		// Token: 0x060058BB RID: 22715 RVA: 0x002DC5F4 File Offset: 0x002DA7F4
		private void UpdateFires(float deltaTime)
		{
			if (!this.Decayed)
			{
				Hull currentHull = this.item.CurrentHull;
				List<FireSource> fireSources = (currentHull != null) ? currentHull.FireSources : null;
				if (fireSources != null && this.FireVulnerability > 0f)
				{
					if (this.fireCheckCooldown <= 0f)
					{
						foreach (FireSource source in fireSources)
						{
							if (source.IsInDamageRange(this.item.WorldPosition, source.DamageRange))
							{
								this.Health -= this.FireVulnerability;
							}
						}
						this.fireCheckCooldown = 10f;
						return;
					}
					this.fireCheckCooldown -= deltaTime;
				}
			}
		}

		// Token: 0x060058BC RID: 22716 RVA: 0x002DC6C4 File Offset: 0x002DA8C4
		private void GrowVines(Planter planter, PlantSlot slot)
		{
			if (this.FullyGrown)
			{
				return;
			}
			this.vineDelay++;
			if ((float)this.vineDelay <= 10f / this.GrowthSpeed)
			{
				return;
			}
			this.vineDelay = 0;
			if (!this.Vines.Any<VineTile>())
			{
				this.GenerateStem();
				return;
			}
			int count = this.Vines.Count;
			this.TryGenerateBranches(planter, slot, null, null);
			if (this.Vines.Count > count)
			{
				this.ResetPlanterSize();
			}
		}

		// Token: 0x060058BD RID: 22717 RVA: 0x002DC744 File Offset: 0x002DA944
		private void GenerateStem()
		{
			VineTile stem = new VineTile(this, Vector2.Zero, VineTileType.Stem, null, null, null)
			{
				BlockedSides = (TileSide.Left | TileSide.Bottom | TileSide.Right)
			};
			this.Vines.Add(stem);
		}

		// Token: 0x060058BE RID: 22718 RVA: 0x002DC790 File Offset: 0x002DA990
		[NullableContext(2)]
		private void TryGenerateBranches([Nullable(1)] Planter planter, PlantSlot slot, Random random = null, Random flowerRandom = null)
		{
			List<VineTile> newList = new List<VineTile>(this.Vines);
			foreach (VineTile oldVines in newList)
			{
				if (oldVines.FailedGrowthAttempts <= 8 && oldVines.CanGrowMore())
				{
					if (Growable.RandomInt(0, this.Vines.Count((VineTile tile) => tile.CanGrowMore()), random) == 0)
					{
						TileSide side = oldVines.GetRandomFreeSide(random);
						if (side == TileSide.None)
						{
							oldVines.FailedGrowthAttempts++;
						}
						else
						{
							if (this.GrowthWeights != Vector4.One)
							{
								float num;
								float num2;
								float num3;
								float num4;
								this.GrowthWeights.Deconstruct(out num, out num2, out num3, out num4);
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
								int index = (int)Math.Log2((double)side);
								if (MathUtils.NearlyEqual(weights[index], 0f, 0.0001f))
								{
									oldVines.FailedGrowthAttempts++;
									continue;
								}
							}
							Vector2 pos = oldVines.AdjacentPositions[side];
							Rectangle rect = VineTile.CreatePlantRect(pos);
							if (this.CollidesWithWorld(rect, planter, slot))
							{
								oldVines.BlockedSides |= side;
								oldVines.FailedGrowthAttempts++;
							}
							else
							{
								FoliageConfig flowerConfig = FoliageConfig.EmptyConfig;
								FoliageConfig leafConfig = FoliageConfig.EmptyConfig;
								if (this.flowerTiles.Any((int i) => this.Vines.Count == i))
								{
									flowerConfig = FoliageConfig.CreateRandomConfig(this.flowerVariants, Growable.MinFlowerScale, Growable.MaxFlowerScale, flowerRandom);
								}
								if ((double)this.LeafProbability >= Growable.RandomDouble(0.0, 1.0, flowerRandom) && this.leafVariants > 0)
								{
									leafConfig = FoliageConfig.CreateRandomConfig(this.leafVariants, Growable.MinLeafScale, Growable.MaxLeafScale, flowerRandom);
								}
								VineTile newVine = new VineTile(this, pos, VineTileType.CrossJunction, new FoliageConfig?(flowerConfig), new FoliageConfig?(leafConfig), new Rectangle?(rect));
								foreach (VineTile otherVine in this.Vines)
								{
									float num3;
									float num4;
									(pos - otherVine.Position).Deconstruct(out num4, out num3);
									float distX = num4;
									float distY = num3;
									int absDistX = (int)Math.Abs(distX);
									int absDistY = (int)Math.Abs(distY);
									if (absDistX <= newVine.Rect.Width && absDistY <= newVine.Rect.Height && (absDistX <= 0 || absDistY <= 0))
									{
										TileSide connectingSide = (absDistX > absDistY) ? ((distX > 0f) ? TileSide.Right : TileSide.Left) : ((distY > 0f) ? TileSide.Top : TileSide.Bottom);
										TileSide oppositeSide = connectingSide.GetOppositeSide();
										if (otherVine.BlockedSides.HasFlag(connectingSide))
										{
											newVine.BlockedSides |= oppositeSide;
										}
										else if (otherVine != oldVines)
										{
											otherVine.BlockedSides |= connectingSide;
											newVine.BlockedSides |= oppositeSide;
										}
										else
										{
											otherVine.Sides |= connectingSide;
											newVine.Sides |= oppositeSide;
										}
									}
								}
								this.Vines.Add(newVine);
								foreach (VineTile vine in this.Vines)
								{
									vine.UpdateType();
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060058BF RID: 22719 RVA: 0x002DCB88 File Offset: 0x002DAD88
		private bool CollidesWithWorld(Rectangle rect, Planter planter, PlantSlot slot)
		{
			if (this.Vines.Any((VineTile g) => g.Rect.Contains(rect)))
			{
				return true;
			}
			Rectangle worldRect = rect;
			worldRect.Location = planter.Item.WorldPosition.ToPoint() + slot.Offset.ToPoint() + worldRect.Location;
			worldRect.Y -= worldRect.Height;
			Rectangle planterRect = planter.Item.WorldRect;
			planterRect.Y -= planterRect.Height;
			if (planterRect.Intersects(worldRect))
			{
				return true;
			}
			Vector2 topLeft = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Left, (float)worldRect.Top));
			Vector2 topRight = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Right, (float)worldRect.Top));
			Vector2 bottomLeft = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Left, (float)worldRect.Bottom));
			Vector2 bottomRight = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Right, (float)worldRect.Bottom));
			return planterRect.Intersects(worldRect) || Growable.<CollidesWithWorld>g__LineCollides|135_1(topLeft, topRight) || Growable.<CollidesWithWorld>g__LineCollides|135_1(topRight, bottomRight) || Growable.<CollidesWithWorld>g__LineCollides|135_1(bottomRight, bottomLeft) || Growable.<CollidesWithWorld>g__LineCollides|135_1(bottomLeft, topLeft);
		}

		// Token: 0x060058C0 RID: 22720 RVA: 0x002DCCE0 File Offset: 0x002DAEE0
		public override XElement Save(XElement parentElement)
		{
			XElement element = base.Save(parentElement);
			element.Add(new XAttribute("flowertiles", string.Join<int>(",", this.flowerTiles)));
			element.Add(new XAttribute("decayed", this.Decayed));
			foreach (VineTile vine in this.Vines)
			{
				XElement vineElement = new XElement("Vine");
				vineElement.Add(new XAttribute("sides", (int)vine.Sides));
				vineElement.Add(new XAttribute("blockedsides", (int)vine.BlockedSides));
				vineElement.Add(new XAttribute("pos", XMLExtensions.Vector2ToString(vine.Position)));
				vineElement.Add(new XAttribute("tile", (int)vine.Type));
				vineElement.Add(new XAttribute("failedattempts", vine.FailedGrowthAttempts));
				vineElement.Add(new XAttribute("growthscale", vine.GrowthStep));
				vineElement.Add(new XAttribute("flowerconfig", vine.FlowerConfig.Serialize()));
				vineElement.Add(new XAttribute("leafconfig", vine.LeafConfig.Serialize()));
				element.Add(vineElement);
			}
			return element;
		}

		// Token: 0x060058C1 RID: 22721 RVA: 0x002DCEB0 File Offset: 0x002DB0B0
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.flowerTiles = componentElement.GetAttributeIntArray("flowertiles", Array.Empty<int>());
			this.Decayed = componentElement.GetAttributeBool("decayed", false);
			this.Vines.Clear();
			foreach (ContentXElement element in componentElement.Elements())
			{
				if (element.Name.ToString().Equals("vine", StringComparison.OrdinalIgnoreCase))
				{
					VineTileType type = (VineTileType)element.GetAttributeInt("tile", 0);
					ContentXElement contentXElement = element;
					string key = "pos";
					Vector2 zero = Vector2.Zero;
					Vector2 pos = contentXElement.GetAttributeVector2(key, zero);
					TileSide sides = (TileSide)element.GetAttributeInt("sides", 0);
					TileSide blockedSides = (TileSide)element.GetAttributeInt("blockedsides", 0);
					int failedAttempts = element.GetAttributeInt("failedattempts", 0);
					float growthscale = element.GetAttributeFloat("growthscale", 0f);
					int flowerConfig = element.GetAttributeInt("flowerconfig", FoliageConfig.EmptyConfigValue);
					int leafConfig = element.GetAttributeInt("leafconfig", FoliageConfig.EmptyConfigValue);
					VineTile tile = new VineTile(this, pos, type, new FoliageConfig?(FoliageConfig.Deserialize(flowerConfig)), new FoliageConfig?(FoliageConfig.Deserialize(leafConfig)), null)
					{
						Sides = sides,
						BlockedSides = blockedSides,
						FailedGrowthAttempts = failedAttempts,
						GrowthStep = growthscale
					};
					this.Vines.Add(tile);
				}
			}
		}

		// Token: 0x060058C2 RID: 22722 RVA: 0x002DD03C File Offset: 0x002DB23C
		private bool CanGrowMore()
		{
			return this.Vines.Any((VineTile tile) => tile.CanGrowMore());
		}

		// Token: 0x060058C3 RID: 22723 RVA: 0x002DD068 File Offset: 0x002DB268
		[NullableContext(2)]
		public static int RandomInt(int min, int max, Random random = null)
		{
			if (random == null)
			{
				return Rand.Range(min, max, Rand.RandSync.Unsynced);
			}
			return random.Next(min, max);
		}

		// Token: 0x060058C4 RID: 22724 RVA: 0x002DD080 File Offset: 0x002DB280
		[NullableContext(2)]
		public static double RandomDouble(double min, double max, Random random = null)
		{
			double? num = ((random != null) ? new double?(random.NextDouble()) : null) * (max - min) + min;
			if (num == null)
			{
				return Rand.Range(min, max, Rand.RandSync.Unsynced);
			}
			return num.GetValueOrDefault();
		}

		// Token: 0x060058C6 RID: 22726 RVA: 0x002DD13C File Offset: 0x002DB33C
		[CompilerGenerated]
		internal static void <TryGenerateProduct>g__SpawnItem|128_1(Item thisItem, ProducedItem producedItem, Vector2 pos)
		{
			if (producedItem.Prefab == null)
			{
				return;
			}
			string[] array = new string[6];
			array[0] = "MicroInteraction:";
			int num = 1;
			GameSession gameSession = GameMain.GameSession;
			string text;
			if (gameSession == null)
			{
				text = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
			}
			array[num] = (text ?? "null");
			array[2] = ":GardeningProduce:";
			array[3] = thisItem.Prefab.Identifier.ToString();
			array[4] = ":";
			array[5] = producedItem.Prefab.Identifier.ToString();
			GameAnalyticsManager.AddDesignEvent(string.Concat(array));
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			spawner.AddItemToSpawnQueue(producedItem.Prefab, pos, null, null, delegate(Item it)
			{
				foreach (StatusEffect effect in producedItem.StatusEffects)
				{
					it.ApplyStatusEffect(effect, ActionType.OnProduceSpawned, 1f, null, null, null, true, true, null);
				}
				it.ApplyStatusEffects(ActionType.OnProduceSpawned, 1f, null, null, null, true, null);
			});
		}

		// Token: 0x060058C8 RID: 22728 RVA: 0x002DD23F File Offset: 0x002DB43F
		[CompilerGenerated]
		internal static bool <CollidesWithWorld>g__LineCollides|135_1(Vector2 point1, Vector2 point2)
		{
			return Submarine.PickBody(point1, point2, null, new Category?(Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat8), true, (Fixture f) => !(f.UserData is Hull) && f.CollidesWith.HasFlag(Category.Cat5), false) != null;
		}

		// Token: 0x04002D21 RID: 11553
		public readonly Dictionary<VineTileType, VineSprite> VineSprites = new Dictionary<VineTileType, VineSprite>();

		// Token: 0x04002D22 RID: 11554
		public readonly List<Sprite> FlowerSprites = new List<Sprite>();

		// Token: 0x04002D23 RID: 11555
		public readonly List<Sprite> LeafSprites = new List<Sprite>();

		// Token: 0x04002D24 RID: 11556
		[Nullable(2)]
		public Sprite VineAtlas;

		// Token: 0x04002D25 RID: 11557
		[Nullable(2)]
		public Sprite DecayAtlas;

		// Token: 0x04002D26 RID: 11558
		private readonly object mutex = new object();

		// Token: 0x04002D27 RID: 11559
		public readonly HashSet<Rectangle> FailedRectangles = new HashSet<Rectangle>();

		// Token: 0x04002D3C RID: 11580
		private const float increasedDeathSpeed = 10f;

		// Token: 0x04002D3D RID: 11581
		private bool accelerateDeath;

		// Token: 0x04002D3E RID: 11582
		private float health;

		// Token: 0x04002D3F RID: 11583
		private int flowerVariants;

		// Token: 0x04002D40 RID: 11584
		private int leafVariants;

		// Token: 0x04002D41 RID: 11585
		private int[] flowerTiles;

		// Token: 0x04002D44 RID: 11588
		private const int maxProductDelay = 10;

		// Token: 0x04002D45 RID: 11589
		private const int maxVineGrowthDelay = 10;

		// Token: 0x04002D46 RID: 11590
		private int productDelay;

		// Token: 0x04002D47 RID: 11591
		private int vineDelay;

		// Token: 0x04002D48 RID: 11592
		private float fireCheckCooldown;

		// Token: 0x04002D49 RID: 11593
		public readonly List<ProducedItem> ProducedItems = new List<ProducedItem>();

		// Token: 0x04002D4A RID: 11594
		public readonly List<VineTile> Vines = new List<VineTile>();

		// Token: 0x04002D4B RID: 11595
		private readonly ProducedItem ProducedSeed;

		// Token: 0x04002D4C RID: 11596
		private static float MinFlowerScale = 0.5f;

		// Token: 0x04002D4D RID: 11597
		private static float MaxFlowerScale = 1f;

		// Token: 0x04002D4E RID: 11598
		private static float MinLeafScale = 0.5f;

		// Token: 0x04002D4F RID: 11599
		private static float MaxLeafScale = 1f;

		// Token: 0x04002D50 RID: 11600
		private const int VineChunkSize = 32;
	}
}
