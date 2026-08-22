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
	// Token: 0x02000029 RID: 41
	internal class NestMission : Mission
	{
		// Token: 0x06000506 RID: 1286 RVA: 0x0002CD64 File Offset: 0x0002AF64
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteByte((byte)((this.selectedCave == null || Level.Loaded == null || !Level.Loaded.Caves.Contains(this.selectedCave)) ? 255 : Level.Loaded.Caves.IndexOf(this.selectedCave)));
			msg.WriteSingle(this.nestPosition.X);
			msg.WriteSingle(this.nestPosition.Y);
			msg.WriteUInt16((ushort)this.items.Count);
			foreach (Item item in this.items)
			{
				item.WriteSpawnData(msg, item.ID, 0, 0, -1);
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000507 RID: 1287 RVA: 0x0002CE48 File Offset: 0x0002B048
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
				NestMission.<get_SonarLabels>d__15 <get_SonarLabels>d__ = new NestMission.<get_SonarLabels>d__15(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0002CE68 File Offset: 0x0002B068
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

		// Token: 0x06000509 RID: 1289 RVA: 0x0002D12C File Offset: 0x0002B32C
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

		// Token: 0x0600050A RID: 1290 RVA: 0x0002D6F8 File Offset: 0x0002B8F8
		private void SpawnNestObjects(Level level, Level.Cave cave)
		{
			level.LevelObjectManager.PlaceNestObjects(level, cave, this.nestPosition, this.nestObjectRadius, this.nestObjectAmount);
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0002D71C File Offset: 0x0002B91C
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

		// Token: 0x0600050C RID: 1292 RVA: 0x0002DB08 File Offset: 0x0002BD08
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

		// Token: 0x0600050D RID: 1293 RVA: 0x0002DCCC File Offset: 0x0002BECC
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.AllItemsDestroyedOrRetrieved();
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0002DCD4 File Offset: 0x0002BED4
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

		// Token: 0x04000281 RID: 641
		private readonly ContentXElement itemConfig;

		// Token: 0x04000282 RID: 642
		private readonly List<Item> items = new List<Item>();

		// Token: 0x04000283 RID: 643
		private readonly Dictionary<Item, StatusEffect> statusEffectOnApproach = new Dictionary<Item, StatusEffect>();

		// Token: 0x04000284 RID: 644
		private readonly List<Tuple<CharacterPrefab, Point>> monsterPrefabs = new List<Tuple<CharacterPrefab, Point>>();

		// Token: 0x04000285 RID: 645
		private float itemSpawnRadius = 800f;

		// Token: 0x04000286 RID: 646
		private readonly float approachItemsRadius = 1000f;

		// Token: 0x04000287 RID: 647
		private readonly float nestObjectRadius = 1000f;

		// Token: 0x04000288 RID: 648
		private readonly float monsterSpawnRadius = 3000f;

		// Token: 0x04000289 RID: 649
		private readonly int nestObjectAmount = 10;

		// Token: 0x0400028A RID: 650
		private readonly bool requireDelivery;

		// Token: 0x0400028B RID: 651
		private readonly Level.PositionType spawnPositionType;

		// Token: 0x0400028C RID: 652
		private Vector2 nestPosition;

		// Token: 0x0400028D RID: 653
		private Level.Cave selectedCave;
	}
}
