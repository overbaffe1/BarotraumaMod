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
	// Token: 0x02000023 RID: 35
	internal class WreckAI : SubmarineTurretAI, IServerSerializable, INetSerializable
	{
		// Token: 0x060002DA RID: 730 RVA: 0x0001A3DB File Offset: 0x000185DB
		private IEnumerable<CoroutineStatus> FadeOutColors(float time)
		{
			WreckAI.<FadeOutColors>d__1 <FadeOutColors>d__ = new WreckAI.<FadeOutColors>d__1(-2);
			<FadeOutColors>d__.<>4__this = this;
			<FadeOutColors>d__.<>3__time = time;
			return <FadeOutColors>d__;
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0001A3F2 File Offset: 0x000185F2
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.IsAlive = msg.ReadBoolean();
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060002DC RID: 732 RVA: 0x0001A400 File Offset: 0x00018600
		// (set) Token: 0x060002DD RID: 733 RVA: 0x0001A408 File Offset: 0x00018608
		public bool IsAlive { get; private set; }

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0001A411 File Offset: 0x00018611
		// (set) Token: 0x060002DF RID: 735 RVA: 0x0001A419 File Offset: 0x00018619
		public WreckAIConfig Config { get; private set; }

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x0001A422 File Offset: 0x00018622
		private bool IsClient
		{
			get
			{
				return GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient;
			}
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0001A437 File Offset: 0x00018637
		private bool IsThalamus(MapEntityPrefab entityPrefab)
		{
			return WreckAI.IsThalamus(entityPrefab, this.Config.Entity);
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0001A44A File Offset: 0x0001864A
		private static IEnumerable<T> GetThalamusEntities<T>(Submarine wreck, Identifier tag) where T : MapEntity
		{
			return WreckAI.GetThalamusEntities(wreck, tag).OfType<T>();
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0001A458 File Offset: 0x00018658
		private static IEnumerable<MapEntity> GetThalamusEntities(Submarine wreck, Identifier tag)
		{
			return from e in MapEntity.MapEntityList
			where e.Submarine == wreck && e.Prefab != null && WreckAI.IsThalamus(e.Prefab, tag)
			select e;
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0001A48F File Offset: 0x0001868F
		public static bool IsThalamus(MapEntityPrefab entityPrefab, Identifier tag)
		{
			return entityPrefab.HasSubCategory("thalamus") || entityPrefab.Tags.Contains(tag);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x0001A4AC File Offset: 0x000186AC
		public static WreckAI Create(Submarine wreck)
		{
			WreckAI wreckAI = new WreckAI(wreck);
			if (wreckAI.Config == null)
			{
				return null;
			}
			return wreckAI;
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0001A4CC File Offset: 0x000186CC
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

		// Token: 0x060002E7 RID: 743 RVA: 0x0001A914 File Offset: 0x00018B14
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

		// Token: 0x060002E8 RID: 744 RVA: 0x0001A94C File Offset: 0x00018B4C
		protected override void LoadAllTurrets()
		{
			this.GetConfig();
			foreach (Turret turret in this.turrets)
			{
				base.LoadTurret(turret, (ItemPrefab ip) => this.Config.ForbiddenAmmunition.None((Identifier id) => id == ip.Identifier));
			}
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0001A9B4 File Offset: 0x00018BB4
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
			if (this.IsCloseEnough(GameMain.GameScreen.Cam.Position, minDist))
			{
				isSomeoneNearby = true;
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

		// Token: 0x060002EA RID: 746 RVA: 0x0001ABB4 File Offset: 0x00018DB4
		private bool IsCloseEnough(Vector2 targetPos, float minDist)
		{
			return Vector2.DistanceSquared(targetPos, base.Submarine.WorldPosition) < minDist * minDist;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0001ABCC File Offset: 0x00018DCC
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

		// Token: 0x060002EC RID: 748 RVA: 0x0001AD10 File Offset: 0x00018F10
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
			this.FadeOutColors();
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

		// Token: 0x060002ED RID: 749 RVA: 0x0001AF20 File Offset: 0x00019120
		private void FadeOutColors()
		{
			if (this.fadeOutRoutine != null)
			{
				CoroutineManager.StopCoroutines(this.fadeOutRoutine);
			}
			this.fadeOutRoutine = CoroutineManager.StartCoroutine(this.FadeOutColors(this.Config.DeadEntityColorFadeOutTime), "");
		}

		// Token: 0x060002EE RID: 750 RVA: 0x0001AF56 File Offset: 0x00019156
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

		// Token: 0x060002EF RID: 751 RVA: 0x0001AF8C File Offset: 0x0001918C
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

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x0001B0A4 File Offset: 0x000192A4
		private int MinCellsPerBrainRoom
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MinAgentsPerBrainRoom);
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x0001B0B8 File Offset: 0x000192B8
		private int MaxCellsPerRoom
		{
			get
			{
				return this.CalculateCellCount(1, this.Config.MaxAgentsPerRoom);
			}
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0001B0CC File Offset: 0x000192CC
		private int MinCellsOutside
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MinAgentsOutside);
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x0001B0E0 File Offset: 0x000192E0
		private int MaxCellsOutside
		{
			get
			{
				return this.CalculateCellCount(0, this.Config.MaxAgentsOutside);
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0001B0F4 File Offset: 0x000192F4
		private int MinCellsInside
		{
			get
			{
				return this.CalculateCellCount(3, this.Config.MinAgentsInside);
			}
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x0001B108 File Offset: 0x00019308
		private int MaxCellsInside
		{
			get
			{
				return this.CalculateCellCount(5, this.Config.MaxAgentsInside);
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x0001B11C File Offset: 0x0001931C
		private int MaxCellCount
		{
			get
			{
				return this.CalculateCellCount(5, this.Config.MaxAgentCount);
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x0001B130 File Offset: 0x00019330
		private float MinWaterLevel
		{
			get
			{
				return this.Config.MinWaterLevel;
			}
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0001B140 File Offset: 0x00019340
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

		// Token: 0x060002F9 RID: 761 RVA: 0x0001B198 File Offset: 0x00019398
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

		// Token: 0x060002FA RID: 762 RVA: 0x0001B21C File Offset: 0x0001941C
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

		// Token: 0x060002FB RID: 763 RVA: 0x0001B274 File Offset: 0x00019474
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

		// Token: 0x060002FC RID: 764 RVA: 0x0001B375 File Offset: 0x00019575
		private void OnCellDeath(Character character, CauseOfDeath causeOfDeath)
		{
			this.protectiveCells.Remove(character);
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0001B384 File Offset: 0x00019584
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

		// Token: 0x040001E9 RID: 489
		private CoroutineHandle fadeOutRoutine;

		// Token: 0x040001EB RID: 491
		private readonly List<Item> thalamusItems;

		// Token: 0x040001EC RID: 492
		private readonly List<Structure> thalamusStructures;

		// Token: 0x040001ED RID: 493
		private readonly List<WayPoint> wayPoints = new List<WayPoint>();

		// Token: 0x040001EE RID: 494
		private readonly List<Hull> hulls = new List<Hull>();

		// Token: 0x040001EF RID: 495
		private readonly List<Item> spawnOrgans = new List<Item>();

		// Token: 0x040001F0 RID: 496
		private readonly List<Door> jammedDoors = new List<Door>();

		// Token: 0x040001F1 RID: 497
		private readonly Item brain;

		// Token: 0x040001F2 RID: 498
		private bool initialCellsSpawned;

		// Token: 0x040001F4 RID: 500
		private readonly List<Item> destroyedOrgans = new List<Item>();

		// Token: 0x040001F5 RID: 501
		private readonly List<Character> protectiveCells = new List<Character>();

		// Token: 0x040001F6 RID: 502
		private readonly List<Hull> populatedHulls = new List<Hull>();

		// Token: 0x040001F7 RID: 503
		private float cellSpawnTimer;
	}
}
