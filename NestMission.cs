using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200005C RID: 92
	internal class NestMission : Mission
	{
		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x000731C6 File Offset: 0x000713C6
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State > 0 && !this.requireDelivery;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x06000CAF RID: 3247 RVA: 0x000731DC File Offset: 0x000713DC
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x000731DF File Offset: 0x000713DF
		// (set) Token: 0x06000CB1 RID: 3249 RVA: 0x000731E7 File Offset: 0x000713E7
		public override int State
		{
			get
			{
				return base.State;
			}
			set
			{
				base.State = value;
				if (base.State > 0 && this.selectedCave != null)
				{
					this.selectedCave.MissionsToDisplayOnSonar.Remove(this);
				}
			}
		}

		// Token: 0x06000CB2 RID: 3250 RVA: 0x00073214 File Offset: 0x00071414
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			byte selectedCaveIndex = msg.ReadByte();
			this.nestPosition = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			if (selectedCaveIndex < 255 && Level.Loaded != null)
			{
				if ((int)selectedCaveIndex < Level.Loaded.Caves.Count)
				{
					this.selectedCave = Level.Loaded.Caves[(int)selectedCaveIndex];
					this.selectedCave.MissionsToDisplayOnSonar.Add(this);
					this.SpawnNestObjects(Level.Loaded, this.selectedCave);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Cave index out of bounds when reading nest mission data. Index: ");
					defaultInterpolatedStringHandler.AppendFormatted<byte>(selectedCaveIndex);
					defaultInterpolatedStringHandler.AppendLiteral(", number of caves: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(Level.Loaded.Caves.Count);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			ushort itemCount = msg.ReadUInt16();
			for (int i = 0; i < (int)itemCount; i++)
			{
				Item item = Item.ReadSpawnData(msg, true);
				this.items.Add(item);
				if (item.body != null)
				{
					item.body.FarseerBody.BodyType = BodyType.Kinematic;
				}
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x0007333C File Offset: 0x0007153C
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				NestMission.<get_SonarLabels>d__22 <get_SonarLabels>d__ = new NestMission.<get_SonarLabels>d__22(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x0007335C File Offset: 0x0007155C
		public NestMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.itemSpawnRadius = prefab.ConfigElement.GetAttributeFloat("itemspawnradius", 800f);
			this.approachItemsRadius = prefab.ConfigElement.GetAttributeFloat("approachitemsradius", this.itemSpawnRadius * 2f);
			this.monsterSpawnRadius = prefab.ConfigElement.GetAttributeFloat("monsterspawnradius", this.approachItemsRadius * 2f);
			this.nestObjectRadius = prefab.ConfigElement.GetAttributeFloat("nestobjectradius", this.itemSpawnRadius * 2f);
			this.nestObjectAmount = prefab.ConfigElement.GetAttributeInt("nestobjectamount", 10);
			this.requireDelivery = prefab.ConfigElement.GetAttributeBool("requiredelivery", false);
			string spawnPositionTypeStr = prefab.ConfigElement.GetAttributeString("spawntype", "");
			if (string.IsNullOrWhiteSpace(spawnPositionTypeStr) || !Enum.TryParse<Level.PositionType>(spawnPositionTypeStr, true, out this.spawnPositionType))
			{
				this.spawnPositionType = (Level.PositionType.Cave | Level.PositionType.Ruin);
			}
			foreach (ContentXElement monsterElement in prefab.ConfigElement.GetChildElements("monster"))
			{
				if (GameMain.NetworkMember != null || !monsterElement.GetAttributeBool("multiplayeronly", false))
				{
					Identifier speciesName = monsterElement.GetAttributeIdentifier("character", Identifier.Empty);
					int defaultCount = monsterElement.GetAttributeInt("count", -1);
					if (defaultCount < 0)
					{
						defaultCount = monsterElement.GetAttributeInt("amount", 1);
					}
					int min = Math.Min(monsterElement.GetAttributeInt("min", defaultCount), 255);
					int max = Math.Min(Math.Max(min, monsterElement.GetAttributeInt("max", defaultCount)), 255);
					CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
					if (characterPrefab != null)
					{
						this.monsterPrefabs.Add(new Tuple<CharacterPrefab, Point>(characterPrefab, new Point(min, max)));
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in monster mission \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Could not find a character prefab with the name \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
				}
			}
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x00073620 File Offset: 0x00071820
		protected override void StartMissionSpecific(Level level)
		{
			if (this.items.Any<Item>())
			{
				DebugConsole.AddWarning("Item list was not empty at the start of a nest mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.items.Clear();
			}
			if (!Mission.IsClient)
			{
				float minDistance = (this.spawnPositionType == Level.PositionType.Ruin || this.spawnPositionType == Level.PositionType.Cave || this.spawnPositionType == Level.PositionType.Wreck) ? 0f : ((float)Level.Loaded.Size.X * 0.3f);
				this.nestPosition = Level.Loaded.GetRandomItemPos(this.spawnPositionType, 100f, minDistance, 30f, null);
				List<GraphEdge> spawnEdges = new List<GraphEdge>();
				if (this.spawnPositionType == Level.PositionType.Cave)
				{
					Level.Cave closestCave = null;
					float closestCaveDist = float.PositiveInfinity;
					foreach (Level.Cave cave in Level.Loaded.Caves)
					{
						float dist = Vector2.DistanceSquared(this.nestPosition, cave.Area.Center.ToVector2());
						if (dist < closestCaveDist)
						{
							closestCave = cave;
							closestCaveDist = dist;
						}
					}
					if (closestCave != null)
					{
						this.selectedCave = closestCave;
						this.selectedCave.MissionsToDisplayOnSonar.Add(this);
						this.SpawnNestObjects(level, closestCave);
					}
					List<VoronoiCell> nearbyCells = Level.Loaded.GetCells(this.nestPosition, 3);
					if (nearbyCells.Any<VoronoiCell>())
					{
						List<GraphEdge> validEdges = new List<GraphEdge>();
						using (IEnumerator<GraphEdge> enumerator2 = nearbyCells.SelectMany((VoronoiCell c) => c.Edges).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								GraphEdge edge = enumerator2.Current;
								if (edge.NextToCave && edge.IsSolid && !Level.Loaded.ExtraWalls.Any((LevelWall w) => w.IsPointInside(edge.Center + edge.GetNormal(edge.Cell1 ?? edge.Cell2) * 100f)))
								{
									validEdges.Add(edge);
								}
							}
						}
						if (validEdges.Any<GraphEdge>())
						{
							spawnEdges.AddRange((from e in validEdges
							where MathUtils.LineSegmentToPointDistanceSquared(e.Point1.ToPoint(), e.Point2.ToPoint(), this.nestPosition.ToPoint()) < (double)(this.itemSpawnRadius * this.itemSpawnRadius)
							select e).Distinct<GraphEdge>());
						}
						if (!spawnEdges.Any<GraphEdge>())
						{
							GraphEdge closestEdge = null;
							float closestDistSqr = float.PositiveInfinity;
							foreach (GraphEdge edge3 in nearbyCells.SelectMany((VoronoiCell c) => c.Edges))
							{
								if (edge3.NextToCave && edge3.IsSolid)
								{
									float dist2 = Vector2.DistanceSquared(edge3.Center, this.nestPosition);
									if (dist2 < closestDistSqr)
									{
										closestEdge = edge3;
										closestDistSqr = dist2;
									}
								}
							}
							if (closestEdge != null)
							{
								spawnEdges.Add(closestEdge);
								this.itemSpawnRadius = Math.Max(this.itemSpawnRadius, (float)Math.Sqrt((double)closestDistSqr) * 1.5f);
							}
						}
					}
				}
				foreach (ContentXElement subElement in this.itemConfig.Elements())
				{
					Identifier itemIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
					ItemPrefab itemPrefab = MapEntityPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab;
					if (itemPrefab == null)
					{
						DebugConsole.ThrowError("Couldn't spawn item for nest mission: item prefab \"" + itemIdentifier.ToString() + "\" not found", null, this.Prefab.ContentPackage, false, false);
					}
					else
					{
						Vector2 spawnPos = this.nestPosition;
						float rotation = 0f;
						if (spawnEdges.Any<GraphEdge>())
						{
							Func<Item, bool> <>9__4;
							for (int i = 0; i < 10; i++)
							{
								GraphEdge edge2 = spawnEdges.GetRandom(Rand.RandSync.ServerAndClient);
								spawnPos = Vector2.Lerp(edge2.Point1, edge2.Point2, Rand.Range(0.1f, 0.9f, Rand.RandSync.ServerAndClient));
								Vector2 normal = Vector2.UnitY;
								if (edge2.Cell1 != null && edge2.Cell1.CellType == CellType.Solid)
								{
									normal = edge2.GetNormal(edge2.Cell1);
								}
								else if (edge2.Cell2 != null && edge2.Cell2.CellType == CellType.Solid)
								{
									normal = edge2.GetNormal(edge2.Cell2);
								}
								spawnPos += normal * 10f;
								rotation = MathUtils.VectorToAngle(normal) - 1.5707964f;
								IEnumerable<Item> source = this.items;
								Func<Item, bool> predicate;
								if ((predicate = <>9__4) == null)
								{
									predicate = (<>9__4 = ((Item it) => Vector2.DistanceSquared(it.WorldPosition, spawnPos) > 30f));
								}
								if (source.All(predicate))
								{
									break;
								}
							}
						}
						Item item = new Item(itemPrefab, spawnPos, null, 0, true);
						item.body.FarseerBody.BodyType = BodyType.Kinematic;
						item.body.SetTransformIgnoreContacts(item.body.SimPosition, rotation, true);
						item.FindHull();
						item.AddTag("nestmission");
						item.AddTag(this.Prefab.Identifier);
						this.items.Add(item);
						ContentXElement statusEffectElement = subElement.GetChildElement("StatusEffectOnApproach") ?? subElement.GetChildElement("statuseffectonapproach");
						ContentXElement contentXElement = null;
						if (statusEffectElement != contentXElement)
						{
							this.statusEffectOnApproach.Add(item, StatusEffect.Load(statusEffectElement, this.Prefab.Identifier.Value));
						}
					}
				}
			}
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x00073BEC File Offset: 0x00071DEC
		private void SpawnNestObjects(Level level, Level.Cave cave)
		{
			level.LevelObjectManager.PlaceNestObjects(level, cave, this.nestPosition, this.nestObjectRadius, this.nestObjectAmount);
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x00073C10 File Offset: 0x00071E10
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				foreach (Item item in this.items)
				{
					if (item.ParentInventory != null && item.body != null)
					{
						item.body.FarseerBody.BodyType = BodyType.Dynamic;
					}
				}
				return;
			}
			int state = this.State;
			if (state != 0)
			{
				if (state != 1)
				{
					return;
				}
				if (!Submarine.MainSub.AtEitherExit)
				{
					return;
				}
				this.State = 2;
			}
			else
			{
				foreach (Item item2 in this.items)
				{
					if (item2.ParentInventory != null && item2.body != null)
					{
						item2.body.FarseerBody.BodyType = BodyType.Dynamic;
					}
					if (this.statusEffectOnApproach.ContainsKey(item2))
					{
						foreach (Character character in Character.CharacterList)
						{
							if (character.IsPlayer && Vector2.DistanceSquared(this.nestPosition, character.WorldPosition) < this.approachItemsRadius * this.approachItemsRadius)
							{
								this.statusEffectOnApproach[item2].Apply(this.statusEffectOnApproach[item2].type, 1f, item2, item2, null);
								this.statusEffectOnApproach.Remove(item2);
								break;
							}
						}
					}
				}
				if (this.monsterPrefabs.Any<Tuple<CharacterPrefab, Point>>())
				{
					foreach (Character character2 in Character.CharacterList)
					{
						if (character2.IsPlayer && Vector2.DistanceSquared(this.nestPosition, character2.WorldPosition) < this.monsterSpawnRadius * this.monsterSpawnRadius)
						{
							foreach (Tuple<CharacterPrefab, Point> monster in this.monsterPrefabs)
							{
								int amount = Rand.Range(monster.Item2.X, monster.Item2.Y + 1, Rand.RandSync.Unsynced);
								int i = 0;
								while (i < amount)
								{
									int tries = 0;
									Vector2 offsetPosition;
									do
									{
										offsetPosition = this.nestPosition + Rand.Vector(100f, Rand.RandSync.Unsynced);
										tries++;
										if (tries > 10)
										{
											goto Block_34;
										}
									}
									while (Level.Loaded.IsPositionInsideWall(offsetPosition));
									IL_255:
									Character.Create(monster.Item1.Identifier, offsetPosition, ToolBox.RandomSeed(8), null, 0, false, true, true, null, true, true);
									i++;
									continue;
									Block_34:
									offsetPosition = this.nestPosition;
									goto IL_255;
								}
							}
							if (Level.Loaded.IsPositionInsideWall(this.nestPosition))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Error in nest mission \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral("\": nest position was inside a wall (");
								defaultInterpolatedStringHandler.AppendFormatted<Vector2>(this.nestPosition);
								defaultInterpolatedStringHandler.AppendLiteral(").");
								DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
							}
							this.monsterPrefabs.Clear();
							break;
						}
					}
				}
				if (this.AllItemsDestroyedOrRetrieved())
				{
					this.State = 1;
					return;
				}
			}
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x00073FFC File Offset: 0x000721FC
		private bool AllItemsDestroyedOrRetrieved()
		{
			if (this.requireDelivery)
			{
				using (List<Item>.Enumerator enumerator = this.items.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item item = enumerator.Current;
						Hull currentHull = item.CurrentHull;
						Submarine submarine;
						if ((submarine = ((currentHull != null) ? currentHull.Submarine : null)) == null)
						{
							Entity rootInventoryOwner = item.GetRootInventoryOwner();
							submarine = ((rootInventoryOwner != null) ? rootInventoryOwner.Submarine : null);
						}
						Submarine parentSub = submarine;
						if (parentSub != null)
						{
							SubmarineInfo info = parentSub.Info;
							SubmarineType? submarineType = (info != null) ? new SubmarineType?(info.Type) : null;
							SubmarineType submarineType2 = SubmarineType.Player;
							if (submarineType.GetValueOrDefault() == submarineType2 & submarineType != null)
							{
								continue;
							}
						}
						return false;
					}
					return true;
				}
			}
			foreach (Item item2 in this.items)
			{
				if (!item2.Removed && item2.Condition > 0f && Vector2.Distance(item2.WorldPosition, this.nestPosition) <= Math.Max(this.itemSpawnRadius * 2f, 3000f))
				{
					Hull currentHull2 = item2.CurrentHull;
					Submarine submarine2;
					if ((submarine2 = ((currentHull2 != null) ? currentHull2.Submarine : null)) == null)
					{
						Entity rootInventoryOwner2 = item2.GetRootInventoryOwner();
						submarine2 = ((rootInventoryOwner2 != null) ? rootInventoryOwner2.Submarine : null);
					}
					Submarine parentSub2 = submarine2;
					if (parentSub2 != null)
					{
						SubmarineInfo info2 = parentSub2.Info;
						SubmarineType? submarineType = (info2 != null) ? new SubmarineType?(info2.Type) : null;
						SubmarineType submarineType2 = SubmarineType.Player;
						if (submarineType.GetValueOrDefault() == submarineType2 & submarineType != null)
						{
							continue;
						}
					}
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x000741C0 File Offset: 0x000723C0
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.AllItemsDestroyedOrRetrieved();
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x000741C8 File Offset: 0x000723C8
		protected override void EndMissionSpecific(bool completed)
		{
			foreach (Item item in this.items)
			{
				if (item != null && !item.Removed)
				{
					item.Remove();
				}
			}
			this.items.Clear();
			this.failed = (!completed && this.state > 0);
		}

		// Token: 0x04000688 RID: 1672
		private readonly ContentXElement itemConfig;

		// Token: 0x04000689 RID: 1673
		private readonly List<Item> items = new List<Item>();

		// Token: 0x0400068A RID: 1674
		private readonly Dictionary<Item, StatusEffect> statusEffectOnApproach = new Dictionary<Item, StatusEffect>();

		// Token: 0x0400068B RID: 1675
		private readonly List<Tuple<CharacterPrefab, Point>> monsterPrefabs = new List<Tuple<CharacterPrefab, Point>>();

		// Token: 0x0400068C RID: 1676
		private float itemSpawnRadius = 800f;

		// Token: 0x0400068D RID: 1677
		private readonly float approachItemsRadius = 1000f;

		// Token: 0x0400068E RID: 1678
		private readonly float nestObjectRadius = 1000f;

		// Token: 0x0400068F RID: 1679
		private readonly float monsterSpawnRadius = 3000f;

		// Token: 0x04000690 RID: 1680
		private readonly int nestObjectAmount = 10;

		// Token: 0x04000691 RID: 1681
		private readonly bool requireDelivery;

		// Token: 0x04000692 RID: 1682
		private readonly Level.PositionType spawnPositionType;

		// Token: 0x04000693 RID: 1683
		private Vector2 nestPosition;

		// Token: 0x04000694 RID: 1684
		private Level.Cave selectedCave;
	}
}
