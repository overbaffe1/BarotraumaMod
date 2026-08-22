using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009F RID: 159
	internal class WreckAI : SubmarineTurretAI, IServerSerializable, INetSerializable
	{
		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x06001304 RID: 4868 RVA: 0x000A64D0 File Offset: 0x000A46D0
		// (set) Token: 0x06001305 RID: 4869 RVA: 0x000A64D8 File Offset: 0x000A46D8
		public bool IsAlive { get; private set; }

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x06001306 RID: 4870 RVA: 0x000A64E1 File Offset: 0x000A46E1
		// (set) Token: 0x06001307 RID: 4871 RVA: 0x000A64E9 File Offset: 0x000A46E9
		public WreckAIConfig Config { get; private set; }

		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x06001308 RID: 4872 RVA: 0x000A64F2 File Offset: 0x000A46F2
		private bool IsClient
		{
			get
			{
				return GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient;
			}
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x000A6507 File Offset: 0x000A4707
		private bool IsThalamus(MapEntityPrefab entityPrefab)
		{
			return WreckAI.IsThalamus(entityPrefab, this.Config.Entity);
		}

		// Token: 0x0600130A RID: 4874 RVA: 0x000A651A File Offset: 0x000A471A
		private static IEnumerable<T> GetThalamusEntities<T>(Submarine wreck, Identifier tag) where T : MapEntity
		{
			return WreckAI.GetThalamusEntities(wreck, tag).OfType<T>();
		}

		// Token: 0x0600130B RID: 4875 RVA: 0x000A6528 File Offset: 0x000A4728
		private static IEnumerable<MapEntity> GetThalamusEntities(Submarine wreck, Identifier tag)
		{
			return from e in MapEntity.MapEntityList
			where e.Submarine == wreck && e.Prefab != null && WreckAI.IsThalamus(e.Prefab, tag)
			select e;
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x000A655F File Offset: 0x000A475F
		public static bool IsThalamus(MapEntityPrefab entityPrefab, Identifier tag)
		{
			return entityPrefab.HasSubCategory("thalamus") || entityPrefab.Tags.Contains(tag);
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x000A657C File Offset: 0x000A477C
		public static WreckAI Create(Submarine wreck)
		{
			WreckAI wreckAI = new WreckAI(wreck);
			if (wreckAI.Config == null)
			{
				return null;
			}
			return wreckAI;
		}

		// Token: 0x0600130E RID: 4878 RVA: 0x000A659C File Offset: 0x000A479C
		private WreckAI(Submarine wreck) : base(wreck, default(Identifier))
		{
			this.GetConfig();
			if (this.Config == null)
			{
				return;
			}
			IEnumerable<ItemPrefab> thalamusPrefabs = ItemPrefab.Prefabs.Where(new Func<ItemPrefab, bool>(this.IsThalamus));
			ItemPrefab brainPrefab = thalamusPrefabs.GetRandom((ItemPrefab i) => i.Tags.Contains(this.Config.Brain), Rand.RandSync.ServerAndClient);
			if (brainPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(100, 2);
				defaultInterpolatedStringHandler.AppendLiteral("WreckAI ");
				defaultInterpolatedStringHandler.AppendFormatted(wreck.Info.Name);
				defaultInterpolatedStringHandler.AppendLiteral(": Could not find any brain prefab with the tag ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Config.Brain);
				defaultInterpolatedStringHandler.AppendLiteral("! Cannot continue. Failed to create wreck AI.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Config.ContentPackage, false, false);
				return;
			}
			this.thalamusItems = WreckAI.GetThalamusEntities<Item>(wreck, this.Config.Entity).ToList<Item>();
			this.hulls.AddRange(wreck.GetHulls(false));
			this.brain = new Item(brainPrefab, Vector2.Zero, wreck, 0, true);
			this.thalamusItems.Add(this.brain);
			Point minSize = this.brain.Rect.Size.Multiply(this.brain.Scale);
			List<ValueTuple<Hull, float>> potentialBrainHulls = WreckAI.GetPotentialBrainRooms(wreck, this.Config, minSize, this.thalamusItems);
			Hull brainHull = ToolBox.SelectWeightedRandom<Hull>((from pbh in potentialBrainHulls
			select pbh.Item1).ToList<Hull>(), (from pbh in potentialBrainHulls
			select pbh.Item2).ToList<float>(), Rand.RandSync.ServerAndClient);
			IEnumerable<StructurePrefab> thalamusStructurePrefabs = StructurePrefab.Prefabs.Where(new Func<StructurePrefab, bool>(this.IsThalamus));
			if (brainHull == null)
			{
				DebugConsole.ThrowError("Wreck AI " + wreck.Info.Name + ": Cannot find a suitable room for the Thalamus brain. Using a random room. The wreck should be fixed so that there's at least one room where the following conditions are met: No linked hulls, no open gaps in the floor or to outside the sub, and no other Thalamus items present in the hull.", null, this.Config.ContentPackage, false, false);
				brainHull = this.hulls.GetRandom(Rand.RandSync.ServerAndClient);
			}
			if (brainHull == null)
			{
				DebugConsole.ThrowError("Wreck AI " + wreck.Info.Name + ": Cannot find any room for the brain! Failed to create the Thalamus.", null, this.Config.ContentPackage, false, false);
				return;
			}
			brainHull.WaterVolume = brainHull.Volume;
			this.brain.SetTransform(brainHull.SimPosition, 0f, false, true, null);
			this.brain.CurrentHull = brainHull;
			foreach (Door door in from g in brainHull.ConnectedGaps
			select g.ConnectedDoor)
			{
				if (door != null)
				{
					door.IsJammed = true;
					this.jammedDoors.Add(door);
				}
			}
			StructurePrefab backgroundPrefab = thalamusStructurePrefabs.GetRandom((StructurePrefab i) => i.Tags.Contains(this.Config.BrainRoomBackground), Rand.RandSync.ServerAndClient);
			if (backgroundPrefab != null)
			{
				Structure background = new Structure(brainHull.Rect, backgroundPrefab, wreck, 0, null);
				background.SpriteDepth -= 0.01f;
			}
			foreach (Item item in this.thalamusItems)
			{
				item.IsLayerHidden = false;
				if (item.HasTag(this.Config.Spawner) && !this.spawnOrgans.Contains(item))
				{
					this.spawnOrgans.Add(item);
					if (item.CurrentHull != null)
					{
						item.CurrentHull.WaterVolume = item.CurrentHull.Volume;
					}
				}
			}
			this.wayPoints.AddRange(wreck.GetWaypoints(false));
			this.IsAlive = true;
			this.thalamusStructures = WreckAI.GetThalamusEntities<Structure>(wreck, this.Config.Entity).ToList<Structure>();
		}

		// Token: 0x0600130F RID: 4879 RVA: 0x000A69E4 File Offset: 0x000A4BE4
		private void GetConfig()
		{
			if (this.Config == null)
			{
				this.Config = WreckAIConfig.GetRandom();
			}
			if (this.Config == null)
			{
				DebugConsole.ThrowError("WreckAI: No wreck AI config found!", null, null, false, false);
			}
		}

		// Token: 0x06001310 RID: 4880 RVA: 0x000A6A1C File Offset: 0x000A4C1C
		protected override void LoadAllTurrets()
		{
			this.GetConfig();
			foreach (Turret turret in this.turrets)
			{
				base.LoadTurret(turret, (ItemPrefab ip) => this.Config.ForbiddenAmmunition.None((Identifier id) => id == ip.Identifier));
			}
		}

		// Token: 0x06001311 RID: 4881 RVA: 0x000A6A84 File Offset: 0x000A4C84
		public override void Update(float deltaTime)
		{
			if (!this.IsAlive)
			{
				return;
			}
			if (base.Submarine == null || base.Submarine.Removed)
			{
				this.Remove();
				return;
			}
			if (this.brain == null || this.brain.Removed || this.brain.Condition <= 0f)
			{
				this.Kill();
				return;
			}
			this.destroyedOrgans.Clear();
			foreach (Item organ in this.spawnOrgans)
			{
				if (organ.Condition <= 0f)
				{
					this.destroyedOrgans.Add(organ);
				}
			}
			this.destroyedOrgans.ForEach(delegate(Item o)
			{
				this.spawnOrgans.Remove(o);
			});
			if (!this.IsClient && !this.initialCellsSpawned)
			{
				this.SpawnInitialCells();
			}
			bool isSomeoneNearby = false;
			float minDist = 20000f;
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				Vector2? spectatePos = client.SpectatePos;
				if (spectatePos != null && this.IsCloseEnough(spectatePos.Value, minDist))
				{
					isSomeoneNearby = true;
					break;
				}
			}
			if (!isSomeoneNearby)
			{
				foreach (Submarine submarine in Submarine.Loaded)
				{
					if (submarine.Info.Type == SubmarineType.Player && this.IsCloseEnough(submarine.WorldPosition, minDist))
					{
						isSomeoneNearby = true;
						break;
					}
				}
			}
			if (!isSomeoneNearby)
			{
				foreach (Character c in Character.CharacterList)
				{
					if ((c.IsPlayer || c.IsOnPlayerTeam) && this.IsCloseEnough(c.WorldPosition, minDist))
					{
						isSomeoneNearby = true;
						break;
					}
				}
			}
			if (!isSomeoneNearby)
			{
				return;
			}
			base.OperateTurrets(deltaTime, this.Config.Entity);
			if (!this.IsClient)
			{
				this.UpdateReinforcements(deltaTime);
			}
		}

		// Token: 0x06001312 RID: 4882 RVA: 0x000A6CCC File Offset: 0x000A4ECC
		private bool IsCloseEnough(Vector2 targetPos, float minDist)
		{
			return Vector2.DistanceSquared(targetPos, base.Submarine.WorldPosition) < minDist * minDist;
		}

		// Token: 0x06001313 RID: 4883 RVA: 0x000A6CE4 File Offset: 0x000A4EE4
		private void SpawnInitialCells()
		{
			int brainRoomCells = Rand.Range(this.MinCellsPerBrainRoom, this.MaxCellsPerRoom + 1, Rand.RandSync.Unsynced);
			Hull currentHull = this.brain.CurrentHull;
			float? num = (currentHull != null) ? new float?(currentHull.WaterPercentage) : null;
			float minWaterLevel = this.MinWaterLevel;
			Character character;
			if (num.GetValueOrDefault() >= minWaterLevel & num != null)
			{
				int i = 0;
				while (i < brainRoomCells && this.TrySpawnCell(out character, this.brain.CurrentHull))
				{
					i++;
				}
			}
			int cellsInside = Rand.Range(this.MinCellsInside, this.MaxCellsInside + 1, Rand.RandSync.Unsynced);
			int j = 0;
			while (j < cellsInside && this.TrySpawnCell(out character, null))
			{
				j++;
			}
			int cellsOutside = Rand.Range(this.MinCellsOutside, this.MaxCellsOutside + 1, Rand.RandSync.Unsynced);
			cellsOutside = Math.Clamp(cellsOutside + brainRoomCells + cellsInside - this.protectiveCells.Count, cellsOutside, this.MaxCellsOutside);
			for (int k = 0; k < cellsOutside; k++)
			{
				ISpatialEntity targetEntity = this.wayPoints.GetRandomUnsynced((WayPoint wp) => wp.CurrentHull == null);
				if (targetEntity == null || !this.TrySpawnCell(out character, targetEntity))
				{
					break;
				}
			}
			this.initialCellsSpawned = true;
		}

		// Token: 0x06001314 RID: 4884 RVA: 0x000A6E28 File Offset: 0x000A5028
		public void Kill()
		{
			this.jammedDoors.ForEach(delegate(Door d)
			{
				d.IsJammed = false;
			});
			this.thalamusItems.ForEach(delegate(Item i)
			{
				i.Condition = 0f;
			});
			foreach (Turret turret in this.turrets)
			{
				foreach (Item item in turret.ActiveProjectiles)
				{
					Projectile component = item.GetComponent<Projectile>();
					if (component != null && component.IsStuckToTarget)
					{
						item.Condition = 0f;
					}
				}
			}
			this.protectiveCells.ForEach(delegate(Character c)
			{
				c.OnDeath = (Character.OnDeathHandler)Delegate.Remove(c.OnDeath, new Character.OnDeathHandler(this.OnCellDeath));
			});
			if (!this.IsClient)
			{
				WreckAIConfig config = this.Config;
				if (config != null && config.KillAgentsWhenEntityDies)
				{
					this.protectiveCells.ForEach(delegate(Character c)
					{
						c.Kill(CauseOfDeathType.Unknown, null, false, true);
					});
					if (!string.IsNullOrWhiteSpace(this.Config.OffensiveAgent))
					{
						foreach (Character character in Character.CharacterList)
						{
							Identifier speciesName = character.SpeciesName;
							if (speciesName == this.Config.OffensiveAgent)
							{
								float maxDistance = 10000f;
								if (Vector2.DistanceSquared(character.WorldPosition, base.Submarine.WorldPosition) < maxDistance * maxDistance)
								{
									character.Kill(CauseOfDeathType.Unknown, null, false, true);
								}
							}
						}
					}
				}
			}
			this.protectiveCells.Clear();
			this.IsAlive = false;
		}

		// Token: 0x06001315 RID: 4885 RVA: 0x000A7034 File Offset: 0x000A5234
		public void Remove()
		{
			this.Kill();
			WreckAI.RemoveThalamusItems(base.Submarine);
			List<Item> list = this.thalamusItems;
			if (list != null)
			{
				list.Clear();
			}
			List<Structure> list2 = this.thalamusStructures;
			if (list2 == null)
			{
				return;
			}
			list2.Clear();
		}

		// Token: 0x06001316 RID: 4886 RVA: 0x000A7068 File Offset: 0x000A5268
		public static void RemoveThalamusItems(Submarine wreck)
		{
			List<MapEntity> thalamusItems = new List<MapEntity>();
			foreach (WreckAIConfig wreckAiConfig in WreckAIConfig.Prefabs)
			{
				thalamusItems.AddRange(WreckAI.GetThalamusEntities(wreck, wreckAiConfig.Entity));
			}
			thalamusItems = thalamusItems.Distinct<MapEntity>().ToList<MapEntity>();
			using (List<MapEntity>.Enumerator enumerator2 = thalamusItems.GetEnumerator())
			{
				Action<Fixture> <>9__1;
				while (enumerator2.MoveNext())
				{
					MapEntity thalamusItem = enumerator2.Current;
					thalamusItem.Remove();
					IEnumerable<Fixture> source = from f in wreck.PhysicsBody.FarseerBody.FixtureList
					where f.UserData == thalamusItem
					select f;
					Action<Fixture> action;
					if ((action = <>9__1) == null)
					{
						action = (<>9__1 = delegate(Fixture f)
						{
							wreck.PhysicsBody.FarseerBody.Remove(f);
						});
					}
					source.ForEachMod(action);
				}
			}
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x06001317 RID: 4887 RVA: 0x000A7180 File Offset: 0x000A5380
		private int MinCellsPerBrainRoom
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MinAgentsPerBrainRoom);
			}
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x06001318 RID: 4888 RVA: 0x000A7194 File Offset: 0x000A5394
		private int MaxCellsPerRoom
		{
			get
			{
				return this.CalculateCellCount(1, this.Config.MaxAgentsPerRoom);
			}
		}

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06001319 RID: 4889 RVA: 0x000A71A8 File Offset: 0x000A53A8
		private int MinCellsOutside
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MinAgentsOutside);
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x0600131A RID: 4890 RVA: 0x000A71BC File Offset: 0x000A53BC
		private int MaxCellsOutside
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MaxAgentsOutside);
			}
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x0600131B RID: 4891 RVA: 0x000A71D0 File Offset: 0x000A53D0
		private int MinCellsInside
		{
			get
			{
				return this.CalculateCellCount(3, this.Config.MinAgentsInside);
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x0600131C RID: 4892 RVA: 0x000A71E4 File Offset: 0x000A53E4
		private int MaxCellsInside
		{
			get
			{
				return this.CalculateCellCount(5, this.Config.MaxAgentsInside);
			}
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x0600131D RID: 4893 RVA: 0x000A71F8 File Offset: 0x000A53F8
		private int MaxCellCount
		{
			get
			{
				return this.CalculateCellCount(5, this.Config.MaxAgentCount);
			}
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x0600131E RID: 4894 RVA: 0x000A720C File Offset: 0x000A540C
		private float MinWaterLevel
		{
			get
			{
				return this.Config.MinWaterLevel;
			}
		}

		// Token: 0x0600131F RID: 4895 RVA: 0x000A721C File Offset: 0x000A541C
		private int CalculateCellCount(int minValue, int maxValue)
		{
			if (maxValue == 0)
			{
				return 0;
			}
			Level loaded = Level.Loaded;
			float difficulty = (loaded != null) ? loaded.Difficulty : 0f;
			float t = MathUtils.InverseLerp(0f, 100f, difficulty * this.Config.AgentSpawnCountDifficultyMultiplier);
			return (int)Math.Round((double)MathHelper.Lerp((float)minValue, (float)maxValue, t));
		}

		// Token: 0x06001320 RID: 4896 RVA: 0x000A7274 File Offset: 0x000A5474
		private float GetSpawnTime()
		{
			float randomFactor = this.Config.AgentSpawnDelayRandomFactor;
			float delay = this.Config.AgentSpawnDelay;
			float min = delay;
			float max = delay * 6f;
			Level loaded = Level.Loaded;
			float difficulty = (loaded != null) ? loaded.Difficulty : 0f;
			float t = difficulty * this.Config.AgentSpawnDelayDifficultyMultiplier * Rand.Range(1f - randomFactor, 1f + randomFactor, Rand.RandSync.Unsynced);
			return MathHelper.Lerp(max, min, MathUtils.InverseLerp(0f, 100f, t));
		}

		// Token: 0x06001321 RID: 4897 RVA: 0x000A72F8 File Offset: 0x000A54F8
		private void UpdateReinforcements(float deltaTime)
		{
			if (this.spawnOrgans.Count == 0)
			{
				return;
			}
			this.cellSpawnTimer -= deltaTime;
			if (this.cellSpawnTimer < 0f)
			{
				Character character;
				this.TrySpawnCell(out character, this.spawnOrgans.GetRandomUnsynced<Item>());
				this.cellSpawnTimer = this.GetSpawnTime();
			}
		}

		// Token: 0x06001322 RID: 4898 RVA: 0x000A7350 File Offset: 0x000A5550
		private bool TrySpawnCell(out Character cell, ISpatialEntity targetEntity = null)
		{
			cell = null;
			if (this.protectiveCells.Count >= this.MaxCellCount)
			{
				return false;
			}
			Hull h;
			if (targetEntity == null)
			{
				ISpatialEntity randomUnsynced = this.wayPoints.GetRandomUnsynced((WayPoint wp) => wp.CurrentHull != null && this.populatedHulls.Count((Hull h) => h == wp.CurrentHull) < this.MaxCellsPerRoom && wp.CurrentHull.WaterPercentage >= this.MinWaterLevel);
				targetEntity = (randomUnsynced ?? this.hulls.GetRandomUnsynced((Hull h) => this.populatedHulls.Count((Hull h2) => h2 == h) < this.MaxCellsPerRoom && h.WaterPercentage >= this.MinWaterLevel));
			}
			if (targetEntity == null)
			{
				return false;
			}
			h = (targetEntity as Hull);
			if (h != null)
			{
				this.populatedHulls.Add(h);
			}
			else
			{
				WayPoint wp2 = targetEntity as WayPoint;
				if (wp2 != null && wp2.CurrentHull != null)
				{
					this.populatedHulls.Add(wp2.CurrentHull);
				}
			}
			cell = Character.Create(this.Config.DefensiveAgent, targetEntity.WorldPosition, ToolBox.RandomSeed(8), null, 0, false, true, true, null, true, true);
			this.protectiveCells.Add(cell);
			Character character = cell;
			character.OnDeath = (Character.OnDeathHandler)Delegate.Combine(character.OnDeath, new Character.OnDeathHandler(this.OnCellDeath));
			this.cellSpawnTimer = this.GetSpawnTime();
			return true;
		}

		// Token: 0x06001323 RID: 4899 RVA: 0x000A7451 File Offset: 0x000A5651
		private void OnCellDeath(Character character, CauseOfDeath causeOfDeath)
		{
			this.protectiveCells.Remove(character);
		}

		// Token: 0x06001324 RID: 4900 RVA: 0x000A7460 File Offset: 0x000A5660
		public void ServerEventWrite(IWriteMessage msg, Client client, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.IsAlive);
		}

		// Token: 0x06001325 RID: 4901 RVA: 0x000A7470 File Offset: 0x000A5670
		[return: TupleElementNames(new string[]
		{
			"hull",
			"weight"
		})]
		public static List<ValueTuple<Hull, float>> GetPotentialBrainRooms(Submarine wreck, WreckAIConfig wreckAI, Point minSize, IEnumerable<Item> thalamusItems = null)
		{
			List<ValueTuple<Hull, float>> potentialBrainHulls = new List<ValueTuple<Hull, float>>();
			Vector2 sufficientSize = new Vector2((float)(minSize.X * 2), (float)minSize.Y * 1.1f);
			Rectangle worldBounds = ToolBox.GetWorldBounds(wreck.WorldPosition.ToPoint(), new Point(wreck.Borders.Width, wreck.Borders.Height));
			if (thalamusItems == null)
			{
				thalamusItems = WreckAI.GetThalamusEntities<Item>(wreck, wreckAI.Entity);
			}
			using (List<Hull>.Enumerator enumerator = wreck.GetHulls(false).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Hull hull = enumerator.Current;
					if (!hull.GetLinkedEntities<Hull>(null, null, null).Any<Hull>() && !hull.ConnectedGaps.Any(delegate(Gap g)
					{
						if (g.Open <= 0f)
						{
							Door connectedDoor = g.ConnectedDoor;
							if (connectedDoor == null || connectedDoor.Item.Condition > 0f)
							{
								return false;
							}
						}
						return !g.IsRoomToRoom || g.Position.Y < hull.Position.Y;
					}) && !thalamusItems.Any((Item i) => i.CurrentHull == hull && !i.HasTag(Tags.WireItem)) && hull.Rect.Width >= minSize.X && hull.Rect.Height >= minSize.Y)
					{
						float weight;
						if (hull.IsAirlock)
						{
							weight = 0f;
						}
						else
						{
							float distanceFromCenter = Vector2.Distance(wreck.WorldPosition, hull.WorldPosition);
							float distanceFactor = MathHelper.Lerp(1f, 0.5f, MathUtils.InverseLerp(0f, (float)Math.Max(worldBounds.Width, worldBounds.Height) / 2f, distanceFromCenter));
							float horizontalSizeFactor = MathHelper.Lerp(0.5f, 1f, MathUtils.InverseLerp((float)minSize.X, sufficientSize.X, (float)hull.Rect.Width));
							float verticalSizeFactor = MathHelper.Lerp(0.5f, 1f, MathUtils.InverseLerp((float)minSize.Y, sufficientSize.Y, (float)hull.Rect.Height));
							weight = verticalSizeFactor * horizontalSizeFactor * distanceFactor;
						}
						if (weight > 0f || potentialBrainHulls.None(null))
						{
							potentialBrainHulls.Add(new ValueTuple<Hull, float>(hull, weight));
						}
					}
				}
			}
			foreach (ValueTuple<Hull, float> valueTuple in potentialBrainHulls)
			{
				Hull hull2 = valueTuple.Item1;
				float weight2 = valueTuple.Item2;
			}
			return potentialBrainHulls;
		}

		// Token: 0x0400090D RID: 2317
		private readonly List<Item> thalamusItems;

		// Token: 0x0400090E RID: 2318
		private readonly List<Structure> thalamusStructures;

		// Token: 0x0400090F RID: 2319
		private readonly List<WayPoint> wayPoints = new List<WayPoint>();

		// Token: 0x04000910 RID: 2320
		private readonly List<Hull> hulls = new List<Hull>();

		// Token: 0x04000911 RID: 2321
		private readonly List<Item> spawnOrgans = new List<Item>();

		// Token: 0x04000912 RID: 2322
		private readonly List<Door> jammedDoors = new List<Door>();

		// Token: 0x04000913 RID: 2323
		private readonly Item brain;

		// Token: 0x04000914 RID: 2324
		private bool initialCellsSpawned;

		// Token: 0x04000916 RID: 2326
		private readonly List<Item> destroyedOrgans = new List<Item>();

		// Token: 0x04000917 RID: 2327
		private readonly List<Character> protectiveCells = new List<Character>();

		// Token: 0x04000918 RID: 2328
		private readonly List<Hull> populatedHulls = new List<Hull>();

		// Token: 0x04000919 RID: 2329
		private float cellSpawnTimer;
	}
}
