using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B2 RID: 1202
	[NullableContext(2)]
	[Nullable(0)]
	internal class EntitySpawnerComponent : ItemComponent, IDrawableComponent
	{
		// Token: 0x17001231 RID: 4657
		// (get) Token: 0x06004455 RID: 17493 RVA: 0x001B69EB File Offset: 0x001B4BEB
		// (set) Token: 0x06004456 RID: 17494 RVA: 0x001B69F3 File Offset: 0x001B4BF3
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item to spawn, does nothing if SpeciesName is set. Separate by comma to have multiple items spawn at random.", "", false)]
		public string ItemIdentifier { get; set; }

		// Token: 0x17001232 RID: 4658
		// (get) Token: 0x06004457 RID: 17495 RVA: 0x001B69FC File Offset: 0x001B4BFC
		// (set) Token: 0x06004458 RID: 17496 RVA: 0x001B6A04 File Offset: 0x001B4C04
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Species name of the creature to spawn, takes priority if ItemIdentifier is set. Separate by comma to have multiple creatures spawn at random.", "", false)]
		public string SpeciesName { get; set; }

		// Token: 0x17001233 RID: 4659
		// (get) Token: 0x06004459 RID: 17497 RVA: 0x001B6A0D File Offset: 0x001B4C0D
		// (set) Token: 0x0600445A RID: 17498 RVA: 0x001B6A15 File Offset: 0x001B4C15
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Only spawn if crew members are within certain area", "", false)]
		public bool OnlySpawnWhenCrewInRange { get; set; }

		// Token: 0x17001234 RID: 4660
		// (get) Token: 0x0600445B RID: 17499 RVA: 0x001B6A1E File Offset: 0x001B4C1E
		// (set) Token: 0x0600445C RID: 17500 RVA: 0x001B6A26 File Offset: 0x001B4C26
		[Editable]
		[Serialize(EntitySpawnerComponent.AreaShape.Rectangle, IsPropertySaveable.Yes, "Shape of the area where crew members need to stay", "", false)]
		public EntitySpawnerComponent.AreaShape CrewAreaShape { get; set; }

		// Token: 0x17001235 RID: 4661
		// (get) Token: 0x0600445D RID: 17501 RVA: 0x001B6A2F File Offset: 0x001B4C2F
		// (set) Token: 0x0600445E RID: 17502 RVA: 0x001B6A37 File Offset: 0x001B4C37
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize("500,500", IsPropertySaveable.Yes, "Size of the rectangle where crew members need to stay. Does nothing if CrewAreaShape is set to Circle", "", false)]
		public Vector2 CrewAreaBounds { get; set; }

		// Token: 0x17001236 RID: 4662
		// (get) Token: 0x0600445F RID: 17503 RVA: 0x001B6A40 File Offset: 0x001B4C40
		// (set) Token: 0x06004460 RID: 17504 RVA: 0x001B6A48 File Offset: 0x001B4C48
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Radius of the circle to spawn stuff in. Does nothing if CrewAreaShape is set to Rectangle", "", false)]
		public float CrewAreaRadius { get; set; }

		// Token: 0x17001237 RID: 4663
		// (get) Token: 0x06004461 RID: 17505 RVA: 0x001B6A51 File Offset: 0x001B4C51
		// (set) Token: 0x06004462 RID: 17506 RVA: 0x001B6A59 File Offset: 0x001B4C59
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 10f)]
		[Serialize("0,0", IsPropertySaveable.Yes, "Offset of the crew area from the center of the item", "", false)]
		public Vector2 CrewAreaOffset { get; set; }

		// Token: 0x17001238 RID: 4664
		// (get) Token: 0x06004463 RID: 17507 RVA: 0x001B6A62 File Offset: 0x001B4C62
		// (set) Token: 0x06004464 RID: 17508 RVA: 0x001B6A6A File Offset: 0x001B4C6A
		[Editable]
		[Serialize(EntitySpawnerComponent.AreaShape.Rectangle, IsPropertySaveable.Yes, "Shape of the area where enemies or items are spawned", "", false)]
		public EntitySpawnerComponent.AreaShape SpawnAreaShape { get; set; }

		// Token: 0x17001239 RID: 4665
		// (get) Token: 0x06004465 RID: 17509 RVA: 0x001B6A73 File Offset: 0x001B4C73
		// (set) Token: 0x06004466 RID: 17510 RVA: 0x001B6A7B File Offset: 0x001B4C7B
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize("500,500", IsPropertySaveable.Yes, "Size of the rectangle where items or creatures will be spawned. Does nothing if SpawnAreaShape is set to Circle", "", false)]
		public Vector2 SpawnAreaBounds { get; set; }

		// Token: 0x1700123A RID: 4666
		// (get) Token: 0x06004467 RID: 17511 RVA: 0x001B6A84 File Offset: 0x001B4C84
		// (set) Token: 0x06004468 RID: 17512 RVA: 0x001B6A8C File Offset: 0x001B4C8C
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Radius of the circle where items or creatures will be spawned. Does nothing if SpawnAreaShape is set to Rectangle", "", false)]
		public float SpawnAreaRadius { get; set; }

		// Token: 0x1700123B RID: 4667
		// (get) Token: 0x06004469 RID: 17513 RVA: 0x001B6A95 File Offset: 0x001B4C95
		// (set) Token: 0x0600446A RID: 17514 RVA: 0x001B6A9D File Offset: 0x001B4C9D
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 10f)]
		[Serialize("0,0", IsPropertySaveable.Yes, "Offset of the spawn area from the center of the item", "", false)]
		public Vector2 SpawnAreaOffset { get; set; }

		// Token: 0x1700123C RID: 4668
		// (get) Token: 0x0600446B RID: 17515 RVA: 0x001B6AA6 File Offset: 0x001B4CA6
		// (set) Token: 0x0600446C RID: 17516 RVA: 0x001B6AAE File Offset: 0x001B4CAE
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 1f)]
		[Serialize("10,40", IsPropertySaveable.Yes, "Time range between spawn attempts in seconds. Set both to a negative value to disable automatic spawning.", "", false)]
		public Vector2 SpawnTimerRange { get; set; }

		// Token: 0x1700123D RID: 4669
		// (get) Token: 0x0600446D RID: 17517 RVA: 0x001B6AB7 File Offset: 0x001B4CB7
		// (set) Token: 0x0600446E RID: 17518 RVA: 0x001B6ABF File Offset: 0x001B4CBF
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 1f, ValueStep = 1f, DecimalCount = 0)]
		[Serialize("1,3", IsPropertySaveable.Yes, "Minumum and maximum amount of items or creatures to spawn in one attempt", "", false)]
		public Vector2 SpawnAmountRange { get; set; }

		// Token: 0x1700123E RID: 4670
		// (get) Token: 0x0600446F RID: 17519 RVA: 0x001B6AC8 File Offset: 0x001B4CC8
		// (set) Token: 0x06004470 RID: 17520 RVA: 0x001B6AD0 File Offset: 0x001B4CD0
		[Editable(MinValueInt = 0, MaxValueInt = 2147483647)]
		[Serialize(8, IsPropertySaveable.Yes, "Total maximum amount of items or creatures that can be spawned. 0 = unrestricted.", "", false)]
		public int MaximumAmount { get; set; }

		// Token: 0x1700123F RID: 4671
		// (get) Token: 0x06004471 RID: 17521 RVA: 0x001B6AD9 File Offset: 0x001B4CD9
		// (set) Token: 0x06004472 RID: 17522 RVA: 0x001B6AE1 File Offset: 0x001B4CE1
		[Editable(MinValueInt = 0, MaxValueInt = 2147483647)]
		[Serialize(8, IsPropertySaveable.Yes, "Amount of items or creatures in the spawn area that will prevent further items or creatures from being spawned. 0 = unrestricted.", "", false)]
		public int MaximumAmountInArea { get; set; }

		// Token: 0x17001240 RID: 4672
		// (get) Token: 0x06004473 RID: 17523 RVA: 0x001B6AEA File Offset: 0x001B4CEA
		// (set) Token: 0x06004474 RID: 17524 RVA: 0x001B6AF2 File Offset: 0x001B4CF2
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Inflate the circle of rectangle by this value to extend the area that counts towards the maximum amount of items or enemies to be spawned", "", false)]
		public float MaximumAmountRangePadding { get; set; }

		// Token: 0x17001241 RID: 4673
		// (get) Token: 0x06004475 RID: 17525 RVA: 0x001B6AFB File Offset: 0x001B4CFB
		// (set) Token: 0x06004476 RID: 17526 RVA: 0x001B6B03 File Offset: 0x001B4D03
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool CanSpawn { get; set; } = true;

		// Token: 0x17001242 RID: 4674
		// (get) Token: 0x06004477 RID: 17527 RVA: 0x001B6B0C File Offset: 0x001B4D0C
		// (set) Token: 0x06004478 RID: 17528 RVA: 0x001B6B14 File Offset: 0x001B4D14
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PreloadCharacter { get; set; }

		// Token: 0x17001243 RID: 4675
		// (get) Token: 0x06004479 RID: 17529 RVA: 0x001B6B1D File Offset: 0x001B4D1D
		// (set) Token: 0x0600447A RID: 17530 RVA: 0x001B6B25 File Offset: 0x001B4D25
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the \"spawn monsters\" setting affect this item in the PvP mode?", "", false)]
		public bool AffectedByPvPSpawnMonstersSetting { get; set; }

		// Token: 0x17001244 RID: 4676
		// (get) Token: 0x0600447B RID: 17531 RVA: 0x001B6B30 File Offset: 0x001B4D30
		private bool DisabledByByPvPSpawnMonstersSetting
		{
			get
			{
				if (!this.SpeciesName.IsNullOrEmpty() && this.AffectedByPvPSpawnMonstersSetting)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
					{
						NetworkMember networkMember = GameMain.NetworkMember;
						if (networkMember != null)
						{
							ServerSettings serverSettings = networkMember.ServerSettings;
							if (serverSettings != null)
							{
								return !serverSettings.PvPSpawnMonsters;
							}
						}
						return false;
					}
				}
				return false;
			}
		}

		// Token: 0x0600447C RID: 17532 RVA: 0x001B6B8A File Offset: 0x001B4D8A
		[NullableContext(1)]
		public EntitySpawnerComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600447D RID: 17533 RVA: 0x001B6BA4 File Offset: 0x001B4DA4
		public override void OnItemLoaded()
		{
			if (!string.IsNullOrWhiteSpace(this.ItemIdentifier))
			{
				string[] allItems = this.ItemIdentifier.Split(',', StringSplitOptions.None);
				foreach (string itemIdentifier in allItems)
				{
					string trimmedString = itemIdentifier.Trim();
					bool found = false;
					foreach (ItemPrefab prefab in ItemPrefab.Prefabs)
					{
						if (trimmedString == prefab.Identifier)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error loading EntitySpawnerComponent - item prefab \"",
							this.name,
							"\" (identifier \"",
							trimmedString,
							"\") not found."
						}), null, null, false, false);
					}
				}
			}
		}

		// Token: 0x0600447E RID: 17534 RVA: 0x001B6C88 File Offset: 0x001B4E88
		[NullableContext(1)]
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.DisabledByByPvPSpawnMonstersSetting)
			{
				this.CanSpawn = false;
			}
			else if (this.PreloadCharacter && !Screen.Selected.IsEditor && !this.preloadInitiated)
			{
				this.SpawnCharacter(Vector2.Zero, delegate(Character c)
				{
					this.preloadedCharacter = c;
					c.DisabledByEvent = true;
				});
				this.preloadInitiated = true;
				return;
			}
			base.Update(deltaTime, cam);
			this.item.SendSignal(this.CanSpawn ? "1" : "0", "state_out");
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			float minTime = Math.Min(this.SpawnTimerRange.X, this.SpawnTimerRange.Y);
			float maxTime = Math.Max(this.SpawnTimerRange.X, this.SpawnTimerRange.Y);
			if (minTime < 0f && maxTime < 0f)
			{
				return;
			}
			float value = this.spawnTimerGoal.GetValueOrDefault();
			if (this.spawnTimerGoal == null)
			{
				value = Rand.Range(minTime, maxTime, Rand.RandSync.Unsynced);
				this.spawnTimerGoal = new float?(value);
			}
			this.spawnTimer += deltaTime;
			float num = this.spawnTimer;
			float? num2 = this.spawnTimerGoal;
			if (num > num2.GetValueOrDefault() & num2 != null)
			{
				this.Spawn();
				this.spawnTimerGoal = null;
				this.spawnTimer = 0f;
			}
		}

		// Token: 0x0600447F RID: 17535 RVA: 0x001B6DE4 File Offset: 0x001B4FE4
		[NullableContext(1)]
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			bool isNonZero = signal.value != "0";
			NetworkMember networkMember = GameMain.NetworkMember;
			bool isClient = networkMember != null && networkMember.IsClient;
			string name = connection.Name;
			if (!(name == "set_state"))
			{
				if (!(name == "toggle"))
				{
					if (!(name == "trigger_in"))
					{
						return;
					}
					if (isNonZero && !isClient)
					{
						this.Spawn();
					}
				}
				else if (isNonZero)
				{
					this.CanSpawn = !this.CanSpawn;
					return;
				}
				return;
			}
			this.CanSpawn = isNonZero;
		}

		// Token: 0x06004480 RID: 17536 RVA: 0x001B6E6C File Offset: 0x001B506C
		private RectangleF GetAreaRectangle(Vector2 size, Vector2 offset, bool draw)
		{
			Vector2 pos = this.item.WorldPosition;
			pos += offset;
			if (draw)
			{
				pos.Y = -pos.Y;
			}
			RectangleF rect = new RectangleF(pos.X - size.X / 2f, pos.Y - size.Y / 2f, size.X, size.Y);
			return rect;
		}

		// Token: 0x06004481 RID: 17537 RVA: 0x001B6ED8 File Offset: 0x001B50D8
		private bool CanSpawnMore()
		{
			if (!this.CanSpawn || this.DisabledByByPvPSpawnMonstersSetting)
			{
				return false;
			}
			if (this.MaximumAmount > 0 && this.spawnedAmount >= this.MaximumAmount)
			{
				return false;
			}
			if (this.OnlySpawnWhenCrewInRange && !Character.CharacterList.Any((Character c) => !c.IsDead && c.IsOnPlayerTeam && this.IsInRange(c.WorldPosition, true, false)))
			{
				return false;
			}
			if (this.MaximumAmountInArea <= 0)
			{
				return true;
			}
			int amount;
			if (!string.IsNullOrWhiteSpace(this.SpeciesName))
			{
				amount = Character.CharacterList.Count(delegate(Character c)
				{
					if (!c.IsDead)
					{
						Identifier speciesName = c.SpeciesName;
						if (speciesName == this.SpeciesName)
						{
							return this.IsInRange(c.WorldPosition, false, true);
						}
					}
					return false;
				});
			}
			else
			{
				if (string.IsNullOrWhiteSpace(this.ItemIdentifier))
				{
					return false;
				}
				amount = Item.ItemList.Count((Item it) => it.Submarine == this.item.Submarine && it.Prefab.Identifier == this.ItemIdentifier && this.IsInRange(it.WorldPosition, false, true));
			}
			return amount < this.MaximumAmountInArea;
		}

		// Token: 0x06004482 RID: 17538 RVA: 0x001B6F94 File Offset: 0x001B5194
		private bool IsInRange(Vector2 worldPos, bool crewArea = false, bool rangePad = false)
		{
			Vector2 offset = crewArea ? this.CrewAreaOffset : this.SpawnAreaOffset;
			EntitySpawnerComponent.AreaShape areaShape = crewArea ? this.CrewAreaShape : this.SpawnAreaShape;
			if (areaShape == EntitySpawnerComponent.AreaShape.Rectangle)
			{
				RectangleF rect = this.GetAreaRectangle(crewArea ? this.CrewAreaBounds : this.SpawnAreaBounds, offset, false);
				if (rangePad)
				{
					rect.Inflate(this.MaximumAmountRangePadding, this.MaximumAmountRangePadding);
				}
				return rect.Contains(worldPos);
			}
			if (areaShape == EntitySpawnerComponent.AreaShape.Circle)
			{
				Vector2 center = this.item.WorldPosition + offset;
				float distance = (crewArea ? this.CrewAreaRadius : this.SpawnAreaRadius) + (rangePad ? this.MaximumAmountRangePadding : 0f);
				return Vector2.DistanceSquared(worldPos, center) < distance * distance;
			}
			return false;
		}

		// Token: 0x06004483 RID: 17539 RVA: 0x001B704C File Offset: 0x001B524C
		public void Spawn()
		{
			if (!this.CanSpawnMore())
			{
				return;
			}
			int minAmount = Math.Min((int)this.SpawnAmountRange.X, (int)this.SpawnAmountRange.Y);
			int maxAmount = Math.Max((int)this.SpawnAmountRange.X, (int)this.SpawnAmountRange.Y);
			int amount = Rand.Range(minAmount, maxAmount, Rand.RandSync.Unsynced);
			Vector2 offset = this.SpawnAreaOffset;
			EntitySpawnerComponent.AreaShape spawnAreaShape = this.SpawnAreaShape;
			if (spawnAreaShape != EntitySpawnerComponent.AreaShape.Rectangle)
			{
				if (spawnAreaShape == EntitySpawnerComponent.AreaShape.Circle)
				{
					float num;
					float num2;
					(this.item.WorldPosition + offset).Deconstruct(out num, out num2);
					float x = num;
					float y = num2;
					for (int i = 0; i < Math.Max(1, amount); i++)
					{
						float angle = Rand.Range(-6.2831855f, 6.2831855f, Rand.RandSync.Unsynced);
						float distance = Rand.Range(0f, this.SpawnAreaRadius, Rand.RandSync.Unsynced);
						Vector2 spawnPos = new Vector2(x + distance * (float)Math.Cos((double)angle), y + distance * (float)Math.Sin((double)angle));
						this.<Spawn>g__SpawnEntity|91_0(spawnPos);
					}
					return;
				}
			}
			else
			{
				RectangleF rect = this.GetAreaRectangle(this.SpawnAreaBounds, offset, false);
				for (int j = 0; j < Math.Max(1, amount); j++)
				{
					float minX = Math.Min(rect.Left, rect.Right);
					float maxX = Math.Max(rect.Left, rect.Right);
					float minY = Math.Min(rect.Top, rect.Bottom);
					float maxY = Math.Max(rect.Top, rect.Bottom);
					Vector2 spawnPos2 = new Vector2(Rand.Range(minX, maxX, Rand.RandSync.Unsynced), Rand.Range(minY, maxY, Rand.RandSync.Unsynced));
					this.<Spawn>g__SpawnEntity|91_0(spawnPos2);
				}
			}
		}

		// Token: 0x06004484 RID: 17540 RVA: 0x001B71F4 File Offset: 0x001B53F4
		private void SpawnCharacter(Vector2 pos, [Nullable(new byte[]
		{
			2,
			1
		})] Action<Character> onSpawn = null)
		{
			if (!string.IsNullOrWhiteSpace(this.SpeciesName))
			{
				Identifier[] allSpecies = this.SpeciesName.ToIdentifiers(",").ToArray<Identifier>();
				Identifier species = allSpecies.GetRandomUnsynced<Identifier>();
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddCharacterToSpawnQueue(species, pos, onSpawn);
			}
		}

		// Token: 0x06004489 RID: 17545 RVA: 0x001B72E8 File Offset: 0x001B54E8
		[CompilerGenerated]
		private void <Spawn>g__SpawnEntity|91_0(Vector2 pos)
		{
			if (string.IsNullOrWhiteSpace(this.SpeciesName))
			{
				if (!string.IsNullOrWhiteSpace(this.ItemIdentifier))
				{
					Identifier[] allItems = this.ItemIdentifier.ToIdentifiers(",").ToArray<Identifier>();
					Identifier itemIdentifier = allItems.GetRandomUnsynced<Identifier>();
					ItemPrefab prefab = ItemPrefab.Find(null, itemIdentifier);
					if (prefab == null)
					{
						return;
					}
					Submarine sub = this.item.Submarine;
					if (sub != null)
					{
						pos -= sub.Position;
					}
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.AddItemToSpawnQueue(prefab, pos, this.item.Submarine, null, null, null);
					}
					this.spawnedAmount++;
				}
				return;
			}
			if (this.preloadedCharacter != null)
			{
				this.preloadedCharacter.DisabledByEvent = false;
				this.preloadedCharacter.TeleportTo(pos);
				this.preloadedCharacter = null;
				this.spawnedAmount++;
				return;
			}
			this.SpawnCharacter(pos, null);
			this.spawnedAmount++;
		}

		// Token: 0x040020BC RID: 8380
		private float spawnTimer;

		// Token: 0x040020BD RID: 8381
		private float? spawnTimerGoal;

		// Token: 0x040020BE RID: 8382
		private int spawnedAmount;

		// Token: 0x040020BF RID: 8383
		private Character preloadedCharacter;

		// Token: 0x040020C0 RID: 8384
		private bool preloadInitiated;

		// Token: 0x02000DFF RID: 3583
		[NullableContext(0)]
		public enum AreaShape
		{
			// Token: 0x0400416A RID: 16746
			Rectangle,
			// Token: 0x0400416B RID: 16747
			Circle
		}
	}
}
