using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005A9 RID: 1449
	[NullableContext(2)]
	[Nullable(0)]
	internal class EntitySpawnerComponent : ItemComponent, IDrawableComponent
	{
		// Token: 0x170015F1 RID: 5617
		// (get) Token: 0x0600580E RID: 22542 RVA: 0x002D98A8 File Offset: 0x002D7AA8
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x0600580F RID: 22543 RVA: 0x002D98B0 File Offset: 0x002D7AB0
		[NullableContext(0)]
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (!editing)
			{
				return;
			}
			EntitySpawnerComponent.AreaShape spawnAreaShape = this.SpawnAreaShape;
			if (spawnAreaShape != EntitySpawnerComponent.AreaShape.Rectangle)
			{
				if (spawnAreaShape == EntitySpawnerComponent.AreaShape.Circle)
				{
					Vector2 center = this.item.WorldPosition;
					center += this.SpawnAreaOffset;
					center.Y = -center.Y;
					spriteBatch.DrawCircle(center, this.SpawnAreaRadius, 32, GUIStyle.Red, 4f);
					if (this.MaximumAmountRangePadding > 0f)
					{
						spriteBatch.DrawCircle(center, this.SpawnAreaRadius + this.MaximumAmountRangePadding, 32, GUIStyle.Red, 2f);
					}
				}
			}
			else
			{
				RectangleF rect = this.GetAreaRectangle(this.SpawnAreaBounds, this.SpawnAreaOffset, true);
				GUI.DrawRectangle(spriteBatch, rect.Location, rect.Size, GUIStyle.Red, false, 0f, 4f);
				if (this.MaximumAmountRangePadding > 0f)
				{
					rect.Inflate(this.MaximumAmountRangePadding, this.MaximumAmountRangePadding);
					GUI.DrawRectangle(spriteBatch, rect.Location, rect.Size, GUIStyle.Red, false, 0f, 2f);
				}
			}
			if (!this.OnlySpawnWhenCrewInRange)
			{
				return;
			}
			EntitySpawnerComponent.AreaShape crewAreaShape = this.CrewAreaShape;
			if (crewAreaShape == EntitySpawnerComponent.AreaShape.Rectangle)
			{
				RectangleF rect2 = this.GetAreaRectangle(this.CrewAreaBounds, this.CrewAreaOffset, true);
				GUI.DrawRectangle(spriteBatch, rect2.Location, rect2.Size, GUIStyle.Green, false, 0f, 4f);
				return;
			}
			if (crewAreaShape != EntitySpawnerComponent.AreaShape.Circle)
			{
				return;
			}
			Vector2 center2 = this.item.WorldPosition;
			center2 += this.CrewAreaOffset;
			center2.Y = -center2.Y;
			spriteBatch.DrawCircle(center2, this.CrewAreaRadius, 32, GUIStyle.Green, 1f);
		}

		// Token: 0x170015F2 RID: 5618
		// (get) Token: 0x06005810 RID: 22544 RVA: 0x002D9A78 File Offset: 0x002D7C78
		// (set) Token: 0x06005811 RID: 22545 RVA: 0x002D9A80 File Offset: 0x002D7C80
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item to spawn, does nothing if SpeciesName is set. Separate by comma to have multiple items spawn at random.", "", false)]
		public string ItemIdentifier { get; set; }

		// Token: 0x170015F3 RID: 5619
		// (get) Token: 0x06005812 RID: 22546 RVA: 0x002D9A89 File Offset: 0x002D7C89
		// (set) Token: 0x06005813 RID: 22547 RVA: 0x002D9A91 File Offset: 0x002D7C91
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Species name of the creature to spawn, takes priority if ItemIdentifier is set. Separate by comma to have multiple creatures spawn at random.", "", false)]
		public string SpeciesName { get; set; }

		// Token: 0x170015F4 RID: 5620
		// (get) Token: 0x06005814 RID: 22548 RVA: 0x002D9A9A File Offset: 0x002D7C9A
		// (set) Token: 0x06005815 RID: 22549 RVA: 0x002D9AA2 File Offset: 0x002D7CA2
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Only spawn if crew members are within certain area", "", false)]
		public bool OnlySpawnWhenCrewInRange { get; set; }

		// Token: 0x170015F5 RID: 5621
		// (get) Token: 0x06005816 RID: 22550 RVA: 0x002D9AAB File Offset: 0x002D7CAB
		// (set) Token: 0x06005817 RID: 22551 RVA: 0x002D9AB3 File Offset: 0x002D7CB3
		[Editable]
		[Serialize(EntitySpawnerComponent.AreaShape.Rectangle, IsPropertySaveable.Yes, "Shape of the area where crew members need to stay", "", false)]
		public EntitySpawnerComponent.AreaShape CrewAreaShape { get; set; }

		// Token: 0x170015F6 RID: 5622
		// (get) Token: 0x06005818 RID: 22552 RVA: 0x002D9ABC File Offset: 0x002D7CBC
		// (set) Token: 0x06005819 RID: 22553 RVA: 0x002D9AC4 File Offset: 0x002D7CC4
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize("500,500", IsPropertySaveable.Yes, "Size of the rectangle where crew members need to stay. Does nothing if CrewAreaShape is set to Circle", "", false)]
		public Vector2 CrewAreaBounds { get; set; }

		// Token: 0x170015F7 RID: 5623
		// (get) Token: 0x0600581A RID: 22554 RVA: 0x002D9ACD File Offset: 0x002D7CCD
		// (set) Token: 0x0600581B RID: 22555 RVA: 0x002D9AD5 File Offset: 0x002D7CD5
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Radius of the circle to spawn stuff in. Does nothing if CrewAreaShape is set to Rectangle", "", false)]
		public float CrewAreaRadius { get; set; }

		// Token: 0x170015F8 RID: 5624
		// (get) Token: 0x0600581C RID: 22556 RVA: 0x002D9ADE File Offset: 0x002D7CDE
		// (set) Token: 0x0600581D RID: 22557 RVA: 0x002D9AE6 File Offset: 0x002D7CE6
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 10f)]
		[Serialize("0,0", IsPropertySaveable.Yes, "Offset of the crew area from the center of the item", "", false)]
		public Vector2 CrewAreaOffset { get; set; }

		// Token: 0x170015F9 RID: 5625
		// (get) Token: 0x0600581E RID: 22558 RVA: 0x002D9AEF File Offset: 0x002D7CEF
		// (set) Token: 0x0600581F RID: 22559 RVA: 0x002D9AF7 File Offset: 0x002D7CF7
		[Editable]
		[Serialize(EntitySpawnerComponent.AreaShape.Rectangle, IsPropertySaveable.Yes, "Shape of the area where enemies or items are spawned", "", false)]
		public EntitySpawnerComponent.AreaShape SpawnAreaShape { get; set; }

		// Token: 0x170015FA RID: 5626
		// (get) Token: 0x06005820 RID: 22560 RVA: 0x002D9B00 File Offset: 0x002D7D00
		// (set) Token: 0x06005821 RID: 22561 RVA: 0x002D9B08 File Offset: 0x002D7D08
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize("500,500", IsPropertySaveable.Yes, "Size of the rectangle where items or creatures will be spawned. Does nothing if SpawnAreaShape is set to Circle", "", false)]
		public Vector2 SpawnAreaBounds { get; set; }

		// Token: 0x170015FB RID: 5627
		// (get) Token: 0x06005822 RID: 22562 RVA: 0x002D9B11 File Offset: 0x002D7D11
		// (set) Token: 0x06005823 RID: 22563 RVA: 0x002D9B19 File Offset: 0x002D7D19
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Radius of the circle where items or creatures will be spawned. Does nothing if SpawnAreaShape is set to Rectangle", "", false)]
		public float SpawnAreaRadius { get; set; }

		// Token: 0x170015FC RID: 5628
		// (get) Token: 0x06005824 RID: 22564 RVA: 0x002D9B22 File Offset: 0x002D7D22
		// (set) Token: 0x06005825 RID: 22565 RVA: 0x002D9B2A File Offset: 0x002D7D2A
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 10f)]
		[Serialize("0,0", IsPropertySaveable.Yes, "Offset of the spawn area from the center of the item", "", false)]
		public Vector2 SpawnAreaOffset { get; set; }

		// Token: 0x170015FD RID: 5629
		// (get) Token: 0x06005826 RID: 22566 RVA: 0x002D9B33 File Offset: 0x002D7D33
		// (set) Token: 0x06005827 RID: 22567 RVA: 0x002D9B3B File Offset: 0x002D7D3B
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f, ValueStep = 1f)]
		[Serialize("10,40", IsPropertySaveable.Yes, "Time range between spawn attempts in seconds. Set both to a negative value to disable automatic spawning.", "", false)]
		public Vector2 SpawnTimerRange { get; set; }

		// Token: 0x170015FE RID: 5630
		// (get) Token: 0x06005828 RID: 22568 RVA: 0x002D9B44 File Offset: 0x002D7D44
		// (set) Token: 0x06005829 RID: 22569 RVA: 0x002D9B4C File Offset: 0x002D7D4C
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 1f, ValueStep = 1f, DecimalCount = 0)]
		[Serialize("1,3", IsPropertySaveable.Yes, "Minumum and maximum amount of items or creatures to spawn in one attempt", "", false)]
		public Vector2 SpawnAmountRange { get; set; }

		// Token: 0x170015FF RID: 5631
		// (get) Token: 0x0600582A RID: 22570 RVA: 0x002D9B55 File Offset: 0x002D7D55
		// (set) Token: 0x0600582B RID: 22571 RVA: 0x002D9B5D File Offset: 0x002D7D5D
		[Editable(MinValueInt = 0, MaxValueInt = 2147483647)]
		[Serialize(8, IsPropertySaveable.Yes, "Total maximum amount of items or creatures that can be spawned. 0 = unrestricted.", "", false)]
		public int MaximumAmount { get; set; }

		// Token: 0x17001600 RID: 5632
		// (get) Token: 0x0600582C RID: 22572 RVA: 0x002D9B66 File Offset: 0x002D7D66
		// (set) Token: 0x0600582D RID: 22573 RVA: 0x002D9B6E File Offset: 0x002D7D6E
		[Editable(MinValueInt = 0, MaxValueInt = 2147483647)]
		[Serialize(8, IsPropertySaveable.Yes, "Amount of items or creatures in the spawn area that will prevent further items or creatures from being spawned. 0 = unrestricted.", "", false)]
		public int MaximumAmountInArea { get; set; }

		// Token: 0x17001601 RID: 5633
		// (get) Token: 0x0600582E RID: 22574 RVA: 0x002D9B77 File Offset: 0x002D7D77
		// (set) Token: 0x0600582F RID: 22575 RVA: 0x002D9B7F File Offset: 0x002D7D7F
		[Editable(MaxValueFloat = 2.1474836E+09f, MinValueFloat = 0f, ValueStep = 10f)]
		[Serialize(500f, IsPropertySaveable.Yes, "Inflate the circle of rectangle by this value to extend the area that counts towards the maximum amount of items or enemies to be spawned", "", false)]
		public float MaximumAmountRangePadding { get; set; }

		// Token: 0x17001602 RID: 5634
		// (get) Token: 0x06005830 RID: 22576 RVA: 0x002D9B88 File Offset: 0x002D7D88
		// (set) Token: 0x06005831 RID: 22577 RVA: 0x002D9B90 File Offset: 0x002D7D90
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool CanSpawn { get; set; } = true;

		// Token: 0x17001603 RID: 5635
		// (get) Token: 0x06005832 RID: 22578 RVA: 0x002D9B99 File Offset: 0x002D7D99
		// (set) Token: 0x06005833 RID: 22579 RVA: 0x002D9BA1 File Offset: 0x002D7DA1
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PreloadCharacter { get; set; }

		// Token: 0x17001604 RID: 5636
		// (get) Token: 0x06005834 RID: 22580 RVA: 0x002D9BAA File Offset: 0x002D7DAA
		// (set) Token: 0x06005835 RID: 22581 RVA: 0x002D9BB2 File Offset: 0x002D7DB2
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the \"spawn monsters\" setting affect this item in the PvP mode?", "", false)]
		public bool AffectedByPvPSpawnMonstersSetting { get; set; }

		// Token: 0x17001605 RID: 5637
		// (get) Token: 0x06005836 RID: 22582 RVA: 0x002D9BBC File Offset: 0x002D7DBC
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

		// Token: 0x06005837 RID: 22583 RVA: 0x002D9C16 File Offset: 0x002D7E16
		[NullableContext(1)]
		public EntitySpawnerComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06005838 RID: 22584 RVA: 0x002D9C30 File Offset: 0x002D7E30
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

		// Token: 0x06005839 RID: 22585 RVA: 0x002D9D14 File Offset: 0x002D7F14
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

		// Token: 0x0600583A RID: 22586 RVA: 0x002D9E70 File Offset: 0x002D8070
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

		// Token: 0x0600583B RID: 22587 RVA: 0x002D9EF8 File Offset: 0x002D80F8
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

		// Token: 0x0600583C RID: 22588 RVA: 0x002D9F64 File Offset: 0x002D8164
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

		// Token: 0x0600583D RID: 22589 RVA: 0x002DA020 File Offset: 0x002D8220
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

		// Token: 0x0600583E RID: 22590 RVA: 0x002DA0D8 File Offset: 0x002D82D8
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
						this.<Spawn>g__SpawnEntity|94_0(spawnPos);
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
					this.<Spawn>g__SpawnEntity|94_0(spawnPos2);
				}
			}
		}

		// Token: 0x0600583F RID: 22591 RVA: 0x002DA280 File Offset: 0x002D8480
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

		// Token: 0x06005844 RID: 22596 RVA: 0x002DA374 File Offset: 0x002D8574
		[CompilerGenerated]
		private void <Spawn>g__SpawnEntity|94_0(Vector2 pos)
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

		// Token: 0x04002D0A RID: 11530
		private float spawnTimer;

		// Token: 0x04002D0B RID: 11531
		private float? spawnTimerGoal;

		// Token: 0x04002D0C RID: 11532
		private int spawnedAmount;

		// Token: 0x04002D0D RID: 11533
		private Character preloadedCharacter;

		// Token: 0x04002D0E RID: 11534
		private bool preloadInitiated;

		// Token: 0x020013A3 RID: 5027
		[NullableContext(0)]
		public enum AreaShape
		{
			// Token: 0x0400630F RID: 25359
			Rectangle,
			// Token: 0x04006310 RID: 25360
			Circle
		}
	}
}
