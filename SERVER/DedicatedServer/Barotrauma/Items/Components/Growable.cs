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

namespace Barotrauma.Items.Components
{
	// Token: 0x02000494 RID: 1172
	[NullableContext(1)]
	[Nullable(0)]
	internal class Growable : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06003EF6 RID: 16118 RVA: 0x00195808 File Offset: 0x00193A08
		[NullableContext(0)]
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedSingle(this.Health, 0f, this.MaxWater, 8);
			Growable.EventData eventData;
			if (base.TryExtractEventData<Growable.EventData>(extraData, out eventData))
			{
				int offset = eventData.Offset;
				int amountToSend = Math.Min(this.Vines.Count - offset, 32);
				msg.WriteRangedInteger(offset, -1, this.MaximumVines);
				msg.WriteRangedInteger(amountToSend, 0, 32);
				for (int i = offset; i < offset + amountToSend; i++)
				{
					VineTile vine = this.Vines[i];
					Vector2 position = vine.Position;
					float num;
					float num2;
					position.Deconstruct(out num, out num2);
					float x = num;
					float y = num2;
					msg.WriteRangedInteger((int)((byte)vine.Type), 0, 15);
					msg.WriteRangedInteger(vine.FlowerConfig.Serialize(), 0, 4095);
					msg.WriteRangedInteger(vine.LeafConfig.Serialize(), 0, 4095);
					msg.WriteByte((byte)(x / (float)VineTile.Size));
					msg.WriteByte((byte)(y / (float)VineTile.Size));
				}
				return;
			}
			msg.WriteRangedInteger(-1, -1, this.MaximumVines);
		}

		// Token: 0x170010A0 RID: 4256
		// (get) Token: 0x06003EF7 RID: 16119 RVA: 0x0019591E File Offset: 0x00193B1E
		// (set) Token: 0x06003EF8 RID: 16120 RVA: 0x00195926 File Offset: 0x00193B26
		[Serialize(1f, IsPropertySaveable.Yes, "How fast the plant grows. Value of 1 means a vine attempts to grow every 10 seconds while 2 and 0.5 mean every 5 and 20 seconds respectively.", "", false)]
		public float GrowthSpeed { get; set; }

		// Token: 0x170010A1 RID: 4257
		// (get) Token: 0x06003EF9 RID: 16121 RVA: 0x0019592F File Offset: 0x00193B2F
		// (set) Token: 0x06003EFA RID: 16122 RVA: 0x00195937 File Offset: 0x00193B37
		[Serialize(100f, IsPropertySaveable.Yes, "How much water the plant can hold. Affects how long the plant can survive without water.", "", false)]
		public float MaxWater { get; set; }

		// Token: 0x170010A2 RID: 4258
		// (get) Token: 0x06003EFB RID: 16123 RVA: 0x00195940 File Offset: 0x00193B40
		// (set) Token: 0x06003EFC RID: 16124 RVA: 0x00195948 File Offset: 0x00193B48
		[Serialize(1f, IsPropertySaveable.Yes, "How much extra water the plant uses per second while it is submerged in a flooded hull.", "", false)]
		public float ExtraWaterUsedPerSecondWhileFlooded { get; set; }

		// Token: 0x170010A3 RID: 4259
		// (get) Token: 0x06003EFD RID: 16125 RVA: 0x00195951 File Offset: 0x00193B51
		// (set) Token: 0x06003EFE RID: 16126 RVA: 0x00195959 File Offset: 0x00193B59
		[Serialize(1f, IsPropertySaveable.Yes, "How much water the plant consumes passively per second.", "", false)]
		public float WaterUsedPerSecond { get; set; }

		// Token: 0x170010A4 RID: 4260
		// (get) Token: 0x06003EFF RID: 16127 RVA: 0x00195962 File Offset: 0x00193B62
		// (set) Token: 0x06003F00 RID: 16128 RVA: 0x0019596A File Offset: 0x00193B6A
		[Serialize(0.01f, IsPropertySaveable.Yes, "Percentage chance of a seed item being produced on growth ticks (every 10 seconds without a multiplier). 0.01 means 1% chance. Not used in vanilla plants.", "", false)]
		public float SeedSpawnChance { get; set; }

		// Token: 0x170010A5 RID: 4261
		// (get) Token: 0x06003F01 RID: 16129 RVA: 0x00195973 File Offset: 0x00193B73
		// (set) Token: 0x06003F02 RID: 16130 RVA: 0x0019597B File Offset: 0x00193B7B
		[Serialize(0.01f, IsPropertySaveable.Yes, "How often a product item is produced on growth ticks (every 10 seconds without a multiplier). 0.01 means 1% chance.", "", false)]
		public float ProductSpawnChance { get; set; }

		// Token: 0x170010A6 RID: 4262
		// (get) Token: 0x06003F03 RID: 16131 RVA: 0x00195984 File Offset: 0x00193B84
		// (set) Token: 0x06003F04 RID: 16132 RVA: 0x0019598C File Offset: 0x00193B8C
		[Serialize(0.5f, IsPropertySaveable.Yes, "Completely unused property that was added on the first design pass but due to the first pass being too complex was never used and now it is used by mods so it cannot be removed.", "", false)]
		public float MutationProbability { get; set; }

		// Token: 0x170010A7 RID: 4263
		// (get) Token: 0x06003F05 RID: 16133 RVA: 0x00195995 File Offset: 0x00193B95
		// (set) Token: 0x06003F06 RID: 16134 RVA: 0x0019599D File Offset: 0x00193B9D
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the flowers.", "", false)]
		public Color FlowerTint { get; set; }

		// Token: 0x170010A8 RID: 4264
		// (get) Token: 0x06003F07 RID: 16135 RVA: 0x001959A6 File Offset: 0x00193BA6
		// (set) Token: 0x06003F08 RID: 16136 RVA: 0x001959AE File Offset: 0x00193BAE
		[Serialize(3, IsPropertySaveable.Yes, "Number of flowers drawn.", "", false)]
		public int FlowerQuantity { get; set; }

		// Token: 0x170010A9 RID: 4265
		// (get) Token: 0x06003F09 RID: 16137 RVA: 0x001959B7 File Offset: 0x00193BB7
		// (set) Token: 0x06003F0A RID: 16138 RVA: 0x001959BF File Offset: 0x00193BBF
		[Serialize(0.25f, IsPropertySaveable.Yes, "Size of the flower sprites.", "", false)]
		public float BaseFlowerScale { get; set; }

		// Token: 0x170010AA RID: 4266
		// (get) Token: 0x06003F0B RID: 16139 RVA: 0x001959C8 File Offset: 0x00193BC8
		// (set) Token: 0x06003F0C RID: 16140 RVA: 0x001959D0 File Offset: 0x00193BD0
		[Serialize(0.5f, IsPropertySaveable.Yes, "Size of the leaf sprites.", "", false)]
		public float BaseLeafScale { get; set; }

		// Token: 0x170010AB RID: 4267
		// (get) Token: 0x06003F0D RID: 16141 RVA: 0x001959D9 File Offset: 0x00193BD9
		// (set) Token: 0x06003F0E RID: 16142 RVA: 0x001959E1 File Offset: 0x00193BE1
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the leaves.", "", false)]
		public Color LeafTint { get; set; }

		// Token: 0x170010AC RID: 4268
		// (get) Token: 0x06003F0F RID: 16143 RVA: 0x001959EA File Offset: 0x00193BEA
		// (set) Token: 0x06003F10 RID: 16144 RVA: 0x001959F2 File Offset: 0x00193BF2
		[Serialize(0.33f, IsPropertySaveable.Yes, "Chance of a leaf appearing behind a branch.", "", false)]
		public float LeafProbability { get; set; }

		// Token: 0x170010AD RID: 4269
		// (get) Token: 0x06003F11 RID: 16145 RVA: 0x001959FB File Offset: 0x00193BFB
		// (set) Token: 0x06003F12 RID: 16146 RVA: 0x00195A03 File Offset: 0x00193C03
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Color of the vines.", "", false)]
		public Color VineTint { get; set; }

		// Token: 0x170010AE RID: 4270
		// (get) Token: 0x06003F13 RID: 16147 RVA: 0x00195A0C File Offset: 0x00193C0C
		// (set) Token: 0x06003F14 RID: 16148 RVA: 0x00195A14 File Offset: 0x00193C14
		[Serialize(32, IsPropertySaveable.Yes, "Maximum number of vine tiles the plant can grow.", "", false)]
		public int MaximumVines { get; set; }

		// Token: 0x170010AF RID: 4271
		// (get) Token: 0x06003F15 RID: 16149 RVA: 0x00195A1D File Offset: 0x00193C1D
		// (set) Token: 0x06003F16 RID: 16150 RVA: 0x00195A25 File Offset: 0x00193C25
		[Serialize(0.25f, IsPropertySaveable.Yes, "Size of the vine sprites.", "", false)]
		public float VineScale { get; set; }

		// Token: 0x170010B0 RID: 4272
		// (get) Token: 0x06003F17 RID: 16151 RVA: 0x00195A2E File Offset: 0x00193C2E
		// (set) Token: 0x06003F18 RID: 16152 RVA: 0x00195A36 File Offset: 0x00193C36
		[Serialize("0.26,0.27,0.29,1.0", IsPropertySaveable.Yes, "Tint of a dead plant.", "", false)]
		public Color DeadTint { get; set; }

		// Token: 0x170010B1 RID: 4273
		// (get) Token: 0x06003F19 RID: 16153 RVA: 0x00195A3F File Offset: 0x00193C3F
		// (set) Token: 0x06003F1A RID: 16154 RVA: 0x00195A47 File Offset: 0x00193C47
		[Serialize("1,1,1,1", IsPropertySaveable.Yes, "Probability for the plant to grow in a direction.", "", false)]
		public Vector4 GrowthWeights { get; set; }

		// Token: 0x170010B2 RID: 4274
		// (get) Token: 0x06003F1B RID: 16155 RVA: 0x00195A50 File Offset: 0x00193C50
		// (set) Token: 0x06003F1C RID: 16156 RVA: 0x00195A58 File Offset: 0x00193C58
		[Serialize(0f, IsPropertySaveable.Yes, "How much water is lost due to fires every 10 seconds.", "", false)]
		public float FireVulnerability { get; set; }

		// Token: 0x170010B3 RID: 4275
		// (get) Token: 0x06003F1D RID: 16157 RVA: 0x00195A61 File Offset: 0x00193C61
		// (set) Token: 0x06003F1E RID: 16158 RVA: 0x00195A69 File Offset: 0x00193C69
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "Modifier to the percentage of product and seed items produced before the plant is fully grown based on how many vines have been grown. 0 would mean no products or seeds are produced while 0.5 would mean half of the normal amount.", "", false)]
		public Vector2 LinearProductAndSeedMultiplierBeforeFullyGrown { get; set; }

		// Token: 0x170010B4 RID: 4276
		// (get) Token: 0x06003F1F RID: 16159 RVA: 0x00195A72 File Offset: 0x00193C72
		// (set) Token: 0x06003F20 RID: 16160 RVA: 0x00195A7A File Offset: 0x00193C7A
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

		// Token: 0x170010B5 RID: 4277
		// (get) Token: 0x06003F21 RID: 16161 RVA: 0x00195A93 File Offset: 0x00193C93
		// (set) Token: 0x06003F22 RID: 16162 RVA: 0x00195A9B File Offset: 0x00193C9B
		public bool Decayed { get; set; }

		// Token: 0x170010B6 RID: 4278
		// (get) Token: 0x06003F23 RID: 16163 RVA: 0x00195AA4 File Offset: 0x00193CA4
		// (set) Token: 0x06003F24 RID: 16164 RVA: 0x00195AAC File Offset: 0x00193CAC
		public bool FullyGrown { get; set; }

		// Token: 0x06003F25 RID: 16165 RVA: 0x00195AB8 File Offset: 0x00193CB8
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

		// Token: 0x06003F26 RID: 16166 RVA: 0x00195C3C File Offset: 0x00193E3C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.flowerTiles.All((int i) => i == 0))
			{
				this.GenerateFlowerTiles(null);
			}
		}

		// Token: 0x06003F27 RID: 16167 RVA: 0x00195C78 File Offset: 0x00193E78
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

		// Token: 0x06003F28 RID: 16168 RVA: 0x00195CF0 File Offset: 0x00193EF0
		private void LoadVines(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "flowersprite"))
				{
					if (a == "leafsprite")
					{
						this.leafVariants++;
					}
				}
				else
				{
					this.flowerVariants++;
				}
			}
		}

		// Token: 0x06003F29 RID: 16169 RVA: 0x00195D84 File Offset: 0x00193F84
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
				if (this.FullyGrown)
				{
					if (this.serverHealthUpdateTimer > 10)
					{
						this.item.CreateServerEvent<Growable>(this);
						this.serverHealthUpdateTimer = 0;
					}
					else
					{
						this.serverHealthUpdateTimer++;
					}
				}
			}
			this.CheckPlantState();
		}

		// Token: 0x06003F2A RID: 16170 RVA: 0x00195E60 File Offset: 0x00194060
		private void UpdateBranchHealth()
		{
			Color healthColor = Color.White * (1f - this.Health / this.MaxWater);
			foreach (VineTile vine in this.Vines)
			{
				vine.HealthColor = healthColor;
			}
		}

		// Token: 0x06003F2B RID: 16171 RVA: 0x00195ED4 File Offset: 0x001940D4
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
				Growable.<TryGenerateProduct>g__SpawnItem|121_1(base.Item, this.ProducedItems.GetRandomByWeight((ProducedItem it) => it.Probability, Rand.RandSync.Unsynced), spawnPos);
				return;
			}
			if (spawnSeed)
			{
				Growable.<TryGenerateProduct>g__SpawnItem|121_1(base.Item, this.ProducedSeed, spawnPos);
			}
		}

		// Token: 0x06003F2C RID: 16172 RVA: 0x00196020 File Offset: 0x00194220
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
				this.item.CreateServerEvent<Growable>(this);
				return true;
			}
			if (this.Vines.Count >= this.MaximumVines && !this.FullyGrown)
			{
				this.FullyGrown = true;
				this.item.CreateServerEvent<Growable>(this);
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
				this.item.CreateServerEvent<Growable>(this);
				return true;
			}
			return false;
		}

		// Token: 0x06003F2D RID: 16173 RVA: 0x00196168 File Offset: 0x00194368
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			this.UpdateFires(deltaTime);
			this.CheckPlantState();
		}

		// Token: 0x06003F2E RID: 16174 RVA: 0x00196180 File Offset: 0x00194380
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

		// Token: 0x06003F2F RID: 16175 RVA: 0x00196250 File Offset: 0x00194450
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
				for (int i = 0; i < this.Vines.Count; i += 32)
				{
					this.item.CreateServerEvent<Growable>(this, new Growable.EventData(i));
				}
			}
		}

		// Token: 0x06003F30 RID: 16176 RVA: 0x001962F8 File Offset: 0x001944F8
		private void GenerateStem()
		{
			VineTile stem = new VineTile(this, Vector2.Zero, VineTileType.Stem, null, null, null)
			{
				BlockedSides = (TileSide.Left | TileSide.Bottom | TileSide.Right)
			};
			this.Vines.Add(stem);
		}

		// Token: 0x06003F31 RID: 16177 RVA: 0x00196344 File Offset: 0x00194544
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

		// Token: 0x06003F32 RID: 16178 RVA: 0x0019673C File Offset: 0x0019493C
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
			return planterRect.Intersects(worldRect) || Growable.<CollidesWithWorld>g__LineCollides|128_1(topLeft, topRight) || Growable.<CollidesWithWorld>g__LineCollides|128_1(topRight, bottomRight) || Growable.<CollidesWithWorld>g__LineCollides|128_1(bottomRight, bottomLeft) || Growable.<CollidesWithWorld>g__LineCollides|128_1(bottomLeft, topLeft);
		}

		// Token: 0x06003F33 RID: 16179 RVA: 0x00196894 File Offset: 0x00194A94
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
				vineElement.Add(new XAttribute("growthscale", this.Decayed ? 1f : 2f));
				vineElement.Add(new XAttribute("flowerconfig", vine.FlowerConfig.Serialize()));
				vineElement.Add(new XAttribute("leafconfig", vine.LeafConfig.Serialize()));
				element.Add(vineElement);
			}
			return element;
		}

		// Token: 0x06003F34 RID: 16180 RVA: 0x00196A70 File Offset: 0x00194C70
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

		// Token: 0x06003F35 RID: 16181 RVA: 0x00196BFC File Offset: 0x00194DFC
		private bool CanGrowMore()
		{
			return this.Vines.Any((VineTile tile) => tile.CanGrowMore());
		}

		// Token: 0x06003F36 RID: 16182 RVA: 0x00196C28 File Offset: 0x00194E28
		[NullableContext(2)]
		public static int RandomInt(int min, int max, Random random = null)
		{
			if (random == null)
			{
				return Rand.Range(min, max, Rand.RandSync.Unsynced);
			}
			return random.Next(min, max);
		}

		// Token: 0x06003F37 RID: 16183 RVA: 0x00196C40 File Offset: 0x00194E40
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

		// Token: 0x06003F39 RID: 16185 RVA: 0x00196CFC File Offset: 0x00194EFC
		[CompilerGenerated]
		internal static void <TryGenerateProduct>g__SpawnItem|121_1(Item thisItem, ProducedItem producedItem, Vector2 pos)
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

		// Token: 0x06003F3B RID: 16187 RVA: 0x00196DFF File Offset: 0x00194FFF
		[CompilerGenerated]
		internal static bool <CollidesWithWorld>g__LineCollides|128_1(Vector2 point1, Vector2 point2)
		{
			return Submarine.PickBody(point1, point2, null, new Category?(Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat8), true, (Fixture f) => !(f.UserData is Hull) && f.CollidesWith.HasFlag(Category.Cat5), false) != null;
		}

		// Token: 0x04001E17 RID: 7703
		private const int serverHealthUpdateDelay = 10;

		// Token: 0x04001E18 RID: 7704
		private int serverHealthUpdateTimer;

		// Token: 0x04001E19 RID: 7705
		public readonly HashSet<Rectangle> FailedRectangles = new HashSet<Rectangle>();

		// Token: 0x04001E2E RID: 7726
		private const float increasedDeathSpeed = 10f;

		// Token: 0x04001E2F RID: 7727
		private bool accelerateDeath;

		// Token: 0x04001E30 RID: 7728
		private float health;

		// Token: 0x04001E31 RID: 7729
		private int flowerVariants;

		// Token: 0x04001E32 RID: 7730
		private int leafVariants;

		// Token: 0x04001E33 RID: 7731
		private int[] flowerTiles;

		// Token: 0x04001E36 RID: 7734
		private const int maxProductDelay = 10;

		// Token: 0x04001E37 RID: 7735
		private const int maxVineGrowthDelay = 10;

		// Token: 0x04001E38 RID: 7736
		private int productDelay;

		// Token: 0x04001E39 RID: 7737
		private int vineDelay;

		// Token: 0x04001E3A RID: 7738
		private float fireCheckCooldown;

		// Token: 0x04001E3B RID: 7739
		public readonly List<ProducedItem> ProducedItems = new List<ProducedItem>();

		// Token: 0x04001E3C RID: 7740
		public readonly List<VineTile> Vines = new List<VineTile>();

		// Token: 0x04001E3D RID: 7741
		private readonly ProducedItem ProducedSeed;

		// Token: 0x04001E3E RID: 7742
		private static float MinFlowerScale = 0.5f;

		// Token: 0x04001E3F RID: 7743
		private static float MaxFlowerScale = 1f;

		// Token: 0x04001E40 RID: 7744
		private static float MinLeafScale = 0.5f;

		// Token: 0x04001E41 RID: 7745
		private static float MaxLeafScale = 1f;

		// Token: 0x04001E42 RID: 7746
		private const int VineChunkSize = 32;

		// Token: 0x02000D7E RID: 3454
		[NullableContext(0)]
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x0600674A RID: 26442 RVA: 0x0021FDD7 File Offset: 0x0021DFD7
			public EventData(int offset)
			{
				this.Offset = offset;
			}

			// Token: 0x04003FE3 RID: 16355
			public readonly int Offset;
		}
	}
}
