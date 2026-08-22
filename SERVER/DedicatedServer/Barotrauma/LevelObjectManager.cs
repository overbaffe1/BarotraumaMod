using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200023D RID: 573
	internal class LevelObjectManager : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x17000BA3 RID: 2979
		// (get) Token: 0x060027D9 RID: 10201 RVA: 0x00101FD5 File Offset: 0x001001D5
		// (set) Token: 0x060027DA RID: 10202 RVA: 0x00101FDD File Offset: 0x001001DD
		public float GlobalForceDecreaseTimer { get; private set; }

		// Token: 0x060027DB RID: 10203 RVA: 0x00101FE6 File Offset: 0x001001E6
		public LevelObjectManager() : base(null, 0)
		{
		}

		// Token: 0x060027DC RID: 10204 RVA: 0x00101FF0 File Offset: 0x001001F0
		public void PlaceObjects(Level level, int amount)
		{
			this.objectGrid = new List<LevelObject>[level.Size.X / 2000, (level.Size.Y - level.BottomPos) / 2000];
			List<LevelObjectManager.SpawnPosition> availableSpawnPositions = new List<LevelObjectManager.SpawnPosition>();
			List<VoronoiCell> levelCells = level.GetAllCells();
			availableSpawnPositions.AddRange(LevelObjectManager.GetAvailableSpawnPositions(levelCells, LevelObjectPrefab.SpawnPosType.Wall));
			availableSpawnPositions.AddRange(LevelObjectManager.GetAvailableSpawnPositions(level.SeaFloor.Cells, LevelObjectPrefab.SpawnPosType.SeaFloor));
			using (List<Structure>.Enumerator enumerator = Structure.WallList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Structure structure = enumerator.Current;
					if (structure.HasBody && !structure.IsHidden)
					{
						LevelObjectPrefab.SpawnPosType spawnPosType;
						if (level.Ruins.Any((Ruin r) => r.Submarine == structure.Submarine))
						{
							spawnPosType = LevelObjectPrefab.SpawnPosType.RuinWall;
						}
						else
						{
							Submarine submarine = structure.Submarine;
							bool flag;
							if (submarine == null)
							{
								flag = false;
							}
							else
							{
								SubmarineInfo info = submarine.Info;
								flag = (((info != null) ? new SubmarineType?(info.Type) : null).GetValueOrDefault() == SubmarineType.Outpost);
							}
							if (!flag)
							{
								continue;
							}
							spawnPosType = LevelObjectPrefab.SpawnPosType.OutpostWall;
						}
						if (structure.IsHorizontal)
						{
							bool topHull = Hull.FindHull(structure.WorldPosition + Vector2.UnitY * 64f, null, true, true) != null;
							bool bottomHull = Hull.FindHull(structure.WorldPosition - Vector2.UnitY * 64f, null, true, true) != null;
							if (!topHull || !bottomHull)
							{
								availableSpawnPositions.Add(new LevelObjectManager.SpawnPosition(new GraphEdge(new Vector2((float)structure.WorldRect.X, structure.WorldPosition.Y), new Vector2((float)structure.WorldRect.Right, structure.WorldPosition.Y)), bottomHull ? Vector2.UnitY : (-Vector2.UnitY), spawnPosType, bottomHull ? Alignment.Bottom : Alignment.Top));
							}
						}
						else
						{
							bool rightHull = Hull.FindHull(structure.WorldPosition + Vector2.UnitX * 64f, null, true, true) != null;
							bool leftHull = Hull.FindHull(structure.WorldPosition - Vector2.UnitX * 64f, null, true, true) != null;
							if (!rightHull || !leftHull)
							{
								availableSpawnPositions.Add(new LevelObjectManager.SpawnPosition(new GraphEdge(new Vector2(structure.WorldPosition.X, (float)structure.WorldRect.Y), new Vector2(structure.WorldPosition.X, (float)(structure.WorldRect.Y - structure.WorldRect.Height))), leftHull ? Vector2.UnitX : (-Vector2.UnitX), spawnPosType, leftHull ? Alignment.Left : Alignment.Right));
							}
						}
					}
				}
			}
			foreach (Level.InterestingPosition posOfInterest in level.PositionsOfInterest)
			{
				if (posOfInterest.PositionType == Level.PositionType.MainPath || posOfInterest.PositionType == Level.PositionType.SidePath)
				{
					List<LevelObjectManager.SpawnPosition> list = availableSpawnPositions;
					Point position = posOfInterest.Position;
					Vector2 point = position.ToVector2();
					position = posOfInterest.Position;
					list.Add(new LevelObjectManager.SpawnPosition(new GraphEdge(point, position.ToVector2() + Vector2.UnitX), Vector2.UnitY, LevelObjectPrefab.SpawnPosType.MainPath, Alignment.Top));
				}
			}
			availableSpawnPositions.Add(new LevelObjectManager.SpawnPosition(new GraphEdge(level.StartPosition - Vector2.UnitX, level.StartPosition + Vector2.UnitX), -Vector2.UnitY, LevelObjectPrefab.SpawnPosType.LevelStart, Alignment.Top));
			availableSpawnPositions.Add(new LevelObjectManager.SpawnPosition(new GraphEdge(level.EndPosition - Vector2.UnitX, level.EndPosition + Vector2.UnitX), -Vector2.UnitY, LevelObjectPrefab.SpawnPosType.LevelEnd, Alignment.Top));
			List<LevelObjectPrefab> availablePrefabs = (from p in LevelObjectPrefab.Prefabs
			orderby p.UintIdentifier
			select p).ToList<LevelObjectPrefab>();
			this.objects = new List<LevelObject>();
			this.updateableObjects = new List<LevelObject>();
			Dictionary<LevelObjectPrefab, List<LevelObjectManager.SpawnPosition>> suitableSpawnPositions = new Dictionary<LevelObjectPrefab, List<LevelObjectManager.SpawnPosition>>();
			Dictionary<LevelObjectPrefab, List<float>> spawnPositionWeights = new Dictionary<LevelObjectPrefab, List<float>>();
			for (int i = 0; i < amount; i++)
			{
				LevelObjectPrefab prefab = LevelObjectManager.GetRandomPrefab(level, availablePrefabs);
				if (prefab != null)
				{
					if (!suitableSpawnPositions.ContainsKey(prefab))
					{
						float minDistance = (float)level.Size.X * 0.2f;
						bool allowAtStart = prefab.AllowAtStart;
						bool allowAtEnd = prefab.AllowAtEnd;
						GameSession gameSession = GameMain.GameSession;
						if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
						{
							allowAtEnd = (allowAtStart = (allowAtEnd & allowAtStart));
						}
						Func<LevelObjectPrefab.SpawnPosType, bool> <>9__5;
						suitableSpawnPositions.Add(prefab, availableSpawnPositions.Where(delegate(LevelObjectManager.SpawnPosition sp)
						{
							IEnumerable<LevelObjectPrefab.SpawnPosType> spawnPosTypes = sp.SpawnPosTypes;
							Func<LevelObjectPrefab.SpawnPosType, bool> predicate;
							if ((predicate = <>9__5) == null)
							{
								predicate = (<>9__5 = ((LevelObjectPrefab.SpawnPosType type) => prefab.SpawnPos.HasFlag(type)));
							}
							return spawnPosTypes.Any(predicate) && sp.Length >= prefab.MinSurfaceWidth && (allowAtStart || !level.IsCloseToStart(sp.GraphEdge.Center, minDistance)) && (allowAtEnd || !level.IsCloseToEnd(sp.GraphEdge.Center, minDistance)) && (sp.Alignment == Alignment.Any || prefab.Alignment.HasFlag(sp.Alignment));
						}).ToList<LevelObjectManager.SpawnPosition>());
						spawnPositionWeights.Add(prefab, (from sp in suitableSpawnPositions[prefab]
						select sp.GetSpawnProbability(prefab)).ToList<float>());
					}
					LevelObjectManager.SpawnPosition spawnPosition = ToolBox.SelectWeightedRandom<LevelObjectManager.SpawnPosition>(suitableSpawnPositions[prefab], spawnPositionWeights[prefab], Rand.RandSync.ServerAndClient);
					if (spawnPosition != null || prefab.SpawnPos == LevelObjectPrefab.SpawnPosType.None)
					{
						this.PlaceObject(prefab, spawnPosition, level, null);
						if (prefab.MaxCount < amount && this.objects.Count((LevelObject o) => o.Prefab == prefab) >= prefab.MaxCount)
						{
							availablePrefabs.Remove(prefab);
						}
					}
				}
			}
			foreach (Level.Cave cave in level.Caves)
			{
				availablePrefabs = (from p in LevelObjectPrefab.Prefabs
				where p.SpawnPos.HasFlag(LevelObjectPrefab.SpawnPosType.CaveWall)
				orderby p.UintIdentifier
				select p).ToList<LevelObjectPrefab>();
				availableSpawnPositions.Clear();
				suitableSpawnPositions.Clear();
				spawnPositionWeights.Clear();
				IEnumerable<VoronoiCell> caveCells = cave.Tunnels.SelectMany((Level.Tunnel t) => t.Cells);
				List<VoronoiCell> caveWallCells = new List<VoronoiCell>();
				foreach (GraphEdge edge in caveCells.SelectMany((VoronoiCell c) => c.Edges))
				{
					if (edge.NextToCave)
					{
						VoronoiCell cell = edge.Cell1;
						if (cell != null && cell.CellType == CellType.Solid)
						{
							caveWallCells.Add(edge.Cell1);
						}
						VoronoiCell cell2 = edge.Cell2;
						if (cell2 != null && cell2.CellType == CellType.Solid)
						{
							caveWallCells.Add(edge.Cell2);
						}
					}
				}
				availableSpawnPositions.AddRange(LevelObjectManager.GetAvailableSpawnPositions(caveWallCells.Distinct<VoronoiCell>(), LevelObjectPrefab.SpawnPosType.CaveWall));
				for (int j = 0; j < cave.CaveGenerationParams.LevelObjectAmount; j++)
				{
					LevelObjectPrefab prefab = LevelObjectManager.GetRandomPrefab(cave.CaveGenerationParams, availablePrefabs, true);
					if (prefab != null)
					{
						if (!suitableSpawnPositions.ContainsKey(prefab))
						{
							suitableSpawnPositions.Add(prefab, (from sp in availableSpawnPositions
							where sp.Length >= prefab.MinSurfaceWidth && (sp.Alignment == Alignment.Any || prefab.Alignment.HasFlag(sp.Alignment))
							select sp).ToList<LevelObjectManager.SpawnPosition>());
							spawnPositionWeights.Add(prefab, (from sp in suitableSpawnPositions[prefab]
							select sp.GetSpawnProbability(prefab)).ToList<float>());
						}
						LevelObjectManager.SpawnPosition spawnPosition2 = ToolBox.SelectWeightedRandom<LevelObjectManager.SpawnPosition>(suitableSpawnPositions[prefab], spawnPositionWeights[prefab], Rand.RandSync.ServerAndClient);
						if (spawnPosition2 != null || prefab.SpawnPos == LevelObjectPrefab.SpawnPosType.None)
						{
							this.PlaceObject(prefab, spawnPosition2, level, cave);
							if (amount > prefab.MaxCount && this.objects.Count > prefab.MaxCount)
							{
								int objectCount = 0;
								for (int k = 0; k < this.objects.Count; k++)
								{
									if (this.objects[k].Prefab == prefab && this.objects[k].ParentCave == cave)
									{
										objectCount++;
										if (objectCount >= prefab.MaxCount)
										{
											break;
										}
									}
								}
								if (objectCount >= prefab.MaxCount)
								{
									availablePrefabs.Remove(prefab);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060027DD RID: 10205 RVA: 0x00102A70 File Offset: 0x00100C70
		public void PlaceNestObjects(Level level, Level.Cave cave, Vector2 nestPosition, float nestRadius, int objectAmount)
		{
			Rand.SetSyncedSeed(ToolBox.StringToInt(level.Seed));
			List<LevelObjectPrefab> availablePrefabs = (from p in LevelObjectPrefab.Prefabs
			where p.SpawnPos.HasFlag(LevelObjectPrefab.SpawnPosType.NestWall)
			orderby p.UintIdentifier
			select p).ToList<LevelObjectPrefab>();
			Dictionary<LevelObjectPrefab, List<LevelObjectManager.SpawnPosition>> suitableSpawnPositions = new Dictionary<LevelObjectPrefab, List<LevelObjectManager.SpawnPosition>>();
			Dictionary<LevelObjectPrefab, List<float>> spawnPositionWeights = new Dictionary<LevelObjectPrefab, List<float>>();
			List<LevelObjectManager.SpawnPosition> availableSpawnPositions = new List<LevelObjectManager.SpawnPosition>();
			IEnumerable<VoronoiCell> caveCells = cave.Tunnels.SelectMany((Level.Tunnel t) => t.Cells);
			List<VoronoiCell> caveWallCells = new List<VoronoiCell>();
			foreach (GraphEdge edge in caveCells.SelectMany((VoronoiCell c) => c.Edges))
			{
				if (edge.NextToCave && MathUtils.LineSegmentToPointDistanceSquared(edge.Point1.ToPoint(), edge.Point2.ToPoint(), nestPosition.ToPoint()) <= (double)(nestRadius * nestRadius))
				{
					VoronoiCell cell = edge.Cell1;
					if (cell != null && cell.CellType == CellType.Solid)
					{
						caveWallCells.Add(edge.Cell1);
					}
					VoronoiCell cell2 = edge.Cell2;
					if (cell2 != null && cell2.CellType == CellType.Solid)
					{
						caveWallCells.Add(edge.Cell2);
					}
				}
			}
			availableSpawnPositions.AddRange(LevelObjectManager.GetAvailableSpawnPositions(caveWallCells.Distinct<VoronoiCell>(), LevelObjectPrefab.SpawnPosType.CaveWall));
			for (int i = 0; i < objectAmount; i++)
			{
				LevelObjectPrefab prefab = LevelObjectManager.GetRandomPrefab(cave.CaveGenerationParams, availablePrefabs, false);
				if (prefab != null)
				{
					if (!suitableSpawnPositions.ContainsKey(prefab))
					{
						suitableSpawnPositions.Add(prefab, (from sp in availableSpawnPositions
						where sp.Length >= prefab.MinSurfaceWidth && (sp.Alignment == Alignment.Any || prefab.Alignment.HasFlag(sp.Alignment))
						select sp).ToList<LevelObjectManager.SpawnPosition>());
						spawnPositionWeights.Add(prefab, (from sp in suitableSpawnPositions[prefab]
						select sp.GetSpawnProbability(prefab)).ToList<float>());
					}
					LevelObjectManager.SpawnPosition spawnPosition = ToolBox.SelectWeightedRandom<LevelObjectManager.SpawnPosition>(suitableSpawnPositions[prefab], spawnPositionWeights[prefab], Rand.RandSync.ServerAndClient);
					if (spawnPosition != null || prefab.SpawnPos == LevelObjectPrefab.SpawnPosType.None)
					{
						this.PlaceObject(prefab, spawnPosition, level, null);
						if (this.objects.Count((LevelObject o) => o.Prefab == prefab) >= prefab.MaxCount)
						{
							availablePrefabs.Remove(prefab);
						}
					}
				}
			}
		}

		// Token: 0x060027DE RID: 10206 RVA: 0x00102D3C File Offset: 0x00100F3C
		private void PlaceObject(LevelObjectPrefab prefab, LevelObjectManager.SpawnPosition spawnPosition, Level level, Level.Cave parentCave = null)
		{
			float rotation = 0f;
			if (prefab.AlignWithSurface && spawnPosition != null && spawnPosition.Normal.LengthSquared() > 0.001f)
			{
				rotation = MathUtils.VectorToAngle(new Vector2(spawnPosition.Normal.Y, spawnPosition.Normal.X));
			}
			rotation += Rand.Range(prefab.RandomRotationRad.X, prefab.RandomRotationRad.Y, Rand.RandSync.ServerAndClient);
			Vector2 position = Vector2.Zero;
			Vector2 edgeDir = Vector2.UnitX;
			if (spawnPosition == null)
			{
				position = new Vector2(Rand.Range(0f, (float)level.Size.X, Rand.RandSync.ServerAndClient), Rand.Range(0f, (float)level.Size.Y, Rand.RandSync.ServerAndClient));
			}
			else
			{
				edgeDir = (spawnPosition.GraphEdge.Point1 - spawnPosition.GraphEdge.Point2) / spawnPosition.Length;
				position = spawnPosition.GraphEdge.Point2 + edgeDir * Rand.Range(prefab.MinSurfaceWidth / 2f, spawnPosition.Length - prefab.MinSurfaceWidth / 2f, Rand.RandSync.ServerAndClient);
			}
			if (!MathUtils.NearlyEqual(prefab.RandomOffset.X, 0f, 0.0001f) || !MathUtils.NearlyEqual(prefab.RandomOffset.Y, 0f, 0.0001f))
			{
				Vector2 offsetDir = (spawnPosition.Normal.LengthSquared() > 0.001f) ? spawnPosition.Normal : Rand.Vector(1f, Rand.RandSync.ServerAndClient);
				position += offsetDir * Rand.Range(prefab.RandomOffset.X, prefab.RandomOffset.Y, Rand.RandSync.ServerAndClient);
			}
			LevelObject newObject = new LevelObject(prefab, new Vector3(position, Rand.Range(prefab.DepthRange.X, prefab.DepthRange.Y, Rand.RandSync.ServerAndClient)), Rand.Range(prefab.MinSize, prefab.MaxSize, Rand.RandSync.ServerAndClient), rotation);
			this.AddObject(newObject, level);
			newObject.ParentCave = parentCave;
			using (List<LevelObjectPrefab.ChildObject>.Enumerator enumerator = prefab.ChildObjects.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					LevelObjectPrefab.ChildObject child = enumerator.Current;
					int childCount = Rand.Range(child.MinCount, child.MaxCount + 1, Rand.RandSync.ServerAndClient);
					Func<LevelObjectPrefab, bool> <>9__0;
					for (int i = 0; i < childCount; i++)
					{
						IEnumerable<LevelObjectPrefab> prefabs = LevelObjectPrefab.Prefabs;
						Func<LevelObjectPrefab, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((LevelObjectPrefab p) => child.AllowedNames.Contains(p.Name)));
						}
						IEnumerable<LevelObjectPrefab> matchingPrefabs = prefabs.Where(predicate);
						int prefabCount = matchingPrefabs.Count<LevelObjectPrefab>();
						LevelObjectPrefab childPrefab = (prefabCount == 0) ? null : matchingPrefabs.ElementAt(Rand.Range(0, prefabCount, Rand.RandSync.ServerAndClient));
						if (childPrefab != null)
						{
							Vector2 childPos = position + edgeDir * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient) * prefab.MinSurfaceWidth;
							LevelObject childObject = new LevelObject(childPrefab, new Vector3(childPos, Rand.Range(childPrefab.DepthRange.X, childPrefab.DepthRange.Y, Rand.RandSync.ServerAndClient)), Rand.Range(childPrefab.MinSize, childPrefab.MaxSize, Rand.RandSync.ServerAndClient), rotation + Rand.Range(childPrefab.RandomRotationRad.X, childPrefab.RandomRotationRad.Y, Rand.RandSync.ServerAndClient));
							this.AddObject(childObject, level);
							childObject.ParentCave = parentCave;
						}
					}
				}
			}
		}

		// Token: 0x060027DF RID: 10207 RVA: 0x001030C0 File Offset: 0x001012C0
		private void AddObject(LevelObject newObject, Level level)
		{
			if (newObject.Triggers != null)
			{
				Action<LevelTrigger, Entity> <>9__4;
				foreach (LevelTrigger trigger in newObject.Triggers)
				{
					LevelTrigger levelTrigger2 = trigger;
					Delegate onTriggered = levelTrigger2.OnTriggered;
					Action<LevelTrigger, Entity> b;
					if ((b = <>9__4) == null)
					{
						b = (<>9__4 = delegate(LevelTrigger levelTrigger, Entity obj)
						{
							this.OnObjectTriggered(newObject, levelTrigger, obj);
						});
					}
					levelTrigger2.OnTriggered = (Action<LevelTrigger, Entity>)Delegate.Combine(onTriggered, b);
				}
			}
			List<Vector2> spriteCorners = new List<Vector2>
			{
				Vector2.Zero,
				Vector2.Zero,
				Vector2.Zero,
				Vector2.Zero
			};
			Sprite sprite2;
			if ((sprite2 = newObject.Sprite) == null)
			{
				DeformableSprite deformableSprite = newObject.Prefab.DeformableSprite;
				sprite2 = ((deformableSprite != null) ? deformableSprite.Sprite : null);
			}
			Sprite sprite = sprite2;
			if (sprite != null)
			{
				Vector2 halfSize = sprite.size * newObject.Scale / 2f;
				spriteCorners[0] = -halfSize;
				spriteCorners[1] = new Vector2(-halfSize.X, halfSize.Y);
				spriteCorners[2] = halfSize;
				spriteCorners[3] = new Vector2(halfSize.X, -halfSize.Y);
				Vector2 pivotOffset = sprite.Origin * newObject.Scale - halfSize;
				pivotOffset.X = -pivotOffset.X;
				pivotOffset = new Vector2((float)((double)pivotOffset.X * Math.Cos((double)(-(double)newObject.Rotation)) - (double)pivotOffset.Y * Math.Sin((double)(-(double)newObject.Rotation))), (float)((double)pivotOffset.X * Math.Sin((double)(-(double)newObject.Rotation)) + (double)pivotOffset.Y * Math.Cos((double)(-(double)newObject.Rotation))));
				for (int i = 0; i < 4; i++)
				{
					spriteCorners[i] = new Vector2((float)((double)spriteCorners[i].X * Math.Cos((double)(-(double)newObject.Rotation)) - (double)spriteCorners[i].Y * Math.Sin((double)(-(double)newObject.Rotation))), (float)((double)spriteCorners[i].X * Math.Sin((double)(-(double)newObject.Rotation)) + (double)spriteCorners[i].Y * Math.Cos((double)(-(double)newObject.Rotation))));
					List<Vector2> list2 = spriteCorners;
					int index = i;
					list2[index] += new Vector2(newObject.Position.X, newObject.Position.Y) + pivotOffset;
				}
			}
			float minX = spriteCorners.Min((Vector2 c) => c.X) - newObject.Position.Z;
			float maxX = spriteCorners.Max((Vector2 c) => c.X) + newObject.Position.Z;
			float minY = spriteCorners.Min((Vector2 c) => c.Y) - newObject.Position.Z - (float)level.BottomPos;
			float maxY = spriteCorners.Max((Vector2 c) => c.Y) + newObject.Position.Z - (float)level.BottomPos;
			if (newObject.Triggers != null)
			{
				foreach (LevelTrigger trigger2 in newObject.Triggers)
				{
					if (trigger2.PhysicsBody != null)
					{
						for (int j = 0; j < trigger2.PhysicsBody.FarseerBody.FixtureList.Count; j++)
						{
							Transform transform;
							trigger2.PhysicsBody.FarseerBody.GetTransform(out transform);
							AABB aabb;
							trigger2.PhysicsBody.FarseerBody.FixtureList[j].Shape.ComputeAABB(out aabb, ref transform, j);
							minX = Math.Min(minX, ConvertUnits.ToDisplayUnits(aabb.LowerBound.X));
							maxX = Math.Max(maxX, ConvertUnits.ToDisplayUnits(aabb.UpperBound.X));
							minY = Math.Min(minY, ConvertUnits.ToDisplayUnits(aabb.LowerBound.Y) - (float)level.BottomPos);
							maxY = Math.Max(maxY, ConvertUnits.ToDisplayUnits(aabb.UpperBound.Y) - (float)level.BottomPos);
						}
					}
				}
			}
			this.objects.Add(newObject);
			if (newObject.NeedsUpdate)
			{
				this.updateableObjects.Add(newObject);
			}
			newObject.Position += new Vector3(0f, 0f, (minX + minY) % 100f * 1E-05f);
			int xStart = (int)Math.Floor((double)(minX / 2000f));
			int xEnd = (int)Math.Floor((double)(maxX / 2000f));
			if (xEnd < 0 || xStart >= this.objectGrid.GetLength(0))
			{
				return;
			}
			int yStart = (int)Math.Floor((double)(minY / 2000f));
			int yEnd = (int)Math.Floor((double)(maxY / 2000f));
			if (yEnd < 0 || yStart >= this.objectGrid.GetLength(1))
			{
				return;
			}
			xStart = Math.Max(xStart, 0);
			xEnd = Math.Min(xEnd, this.objectGrid.GetLength(0) - 1);
			yStart = Math.Max(yStart, 0);
			yEnd = Math.Min(yEnd, this.objectGrid.GetLength(1) - 1);
			for (int x = xStart; x <= xEnd; x++)
			{
				for (int y = yStart; y <= yEnd; y++)
				{
					List<LevelObject> list = this.objectGrid[x, y];
					if (list == null)
					{
						list = (this.objectGrid[x, y] = new List<LevelObject>());
					}
					int drawOrderIndex = 0;
					while (drawOrderIndex < list.Count && list[drawOrderIndex].Position.Z < newObject.Position.Z)
					{
						drawOrderIndex++;
					}
					list.Insert(drawOrderIndex, newObject);
				}
			}
		}

		// Token: 0x060027E0 RID: 10208 RVA: 0x001037D8 File Offset: 0x001019D8
		public static Point GetGridIndices(Vector2 worldPosition)
		{
			return new Point((int)Math.Floor((double)(worldPosition.X / 2000f)), (int)Math.Floor((double)((worldPosition.Y - (float)Level.Loaded.BottomPos) / 2000f)));
		}

		// Token: 0x060027E1 RID: 10209 RVA: 0x00103811 File Offset: 0x00101A11
		public IEnumerable<LevelObject> GetAllObjects()
		{
			return this.objects;
		}

		// Token: 0x060027E2 RID: 10210 RVA: 0x0010381C File Offset: 0x00101A1C
		public IEnumerable<LevelObject> GetAllObjects(Vector2 worldPosition, float radius)
		{
			Point minIndices = LevelObjectManager.GetGridIndices(worldPosition - Vector2.One * radius);
			if (minIndices.X >= this.objectGrid.GetLength(0) || minIndices.Y >= this.objectGrid.GetLength(1))
			{
				return Enumerable.Empty<LevelObject>();
			}
			Point maxIndices = LevelObjectManager.GetGridIndices(worldPosition + Vector2.One * radius);
			if (maxIndices.X < 0 || maxIndices.Y < 0)
			{
				return Enumerable.Empty<LevelObject>();
			}
			minIndices.X = Math.Max(0, minIndices.X);
			minIndices.Y = Math.Max(0, minIndices.Y);
			maxIndices.X = Math.Min(this.objectGrid.GetLength(0) - 1, maxIndices.X);
			maxIndices.Y = Math.Min(this.objectGrid.GetLength(1) - 1, maxIndices.Y);
			LevelObjectManager.objectsInRange.Clear();
			for (int x = minIndices.X; x <= maxIndices.X; x++)
			{
				for (int y = minIndices.Y; y <= maxIndices.Y; y++)
				{
					if (this.objectGrid[x, y] != null)
					{
						foreach (LevelObject obj in this.objectGrid[x, y])
						{
							if (!obj.Prefab.HideWhenBroken || obj.Health > 0f)
							{
								LevelObjectManager.objectsInRange.Add(obj);
							}
						}
					}
				}
			}
			return LevelObjectManager.objectsInRange;
		}

		// Token: 0x060027E3 RID: 10211 RVA: 0x001039C4 File Offset: 0x00101BC4
		private static List<LevelObjectManager.SpawnPosition> GetAvailableSpawnPositions(IEnumerable<VoronoiCell> cells, LevelObjectPrefab.SpawnPosType spawnPosType)
		{
			List<LevelObjectPrefab.SpawnPosType> spawnPosTypes = new List<LevelObjectPrefab.SpawnPosType>(4);
			List<LevelObjectManager.SpawnPosition> availableSpawnPositions = new List<LevelObjectManager.SpawnPosition>();
			bool requireCaveSpawnPos = spawnPosType == LevelObjectPrefab.SpawnPosType.CaveWall;
			foreach (VoronoiCell cell in cells)
			{
				foreach (GraphEdge edge in cell.Edges)
				{
					if (edge.IsSolid && !edge.OutsideLevel && requireCaveSpawnPos == edge.NextToCave)
					{
						Vector2 normal = edge.GetNormal(cell);
						Alignment edgeAlignment = (Alignment)0;
						if (normal.Y < -0.5f)
						{
							edgeAlignment |= Alignment.Bottom;
						}
						else if (normal.Y > 0.5f)
						{
							edgeAlignment |= Alignment.Top;
						}
						else if (normal.X < -0.5f)
						{
							edgeAlignment |= Alignment.Left;
						}
						else if (normal.X > 0.5f)
						{
							edgeAlignment |= Alignment.Right;
						}
						spawnPosTypes.Clear();
						spawnPosTypes.Add(spawnPosType);
						if (spawnPosType.HasFlag(LevelObjectPrefab.SpawnPosType.MainPathWall) && edge.NextToMainPath)
						{
							spawnPosTypes.Add(LevelObjectPrefab.SpawnPosType.MainPathWall);
						}
						if (spawnPosType.HasFlag(LevelObjectPrefab.SpawnPosType.SidePathWall) && edge.NextToSidePath)
						{
							spawnPosTypes.Add(LevelObjectPrefab.SpawnPosType.SidePathWall);
						}
						if (spawnPosType.HasFlag(LevelObjectPrefab.SpawnPosType.CaveWall) && edge.NextToCave)
						{
							spawnPosTypes.Add(LevelObjectPrefab.SpawnPosType.CaveWall);
						}
						availableSpawnPositions.Add(new LevelObjectManager.SpawnPosition(edge, normal, spawnPosTypes, edgeAlignment));
					}
				}
			}
			return availableSpawnPositions;
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x00103B90 File Offset: 0x00101D90
		public void Update(float deltaTime, Camera cam)
		{
			this.GlobalForceDecreaseTimer += deltaTime;
			if (this.GlobalForceDecreaseTimer > 1000000f)
			{
				this.GlobalForceDecreaseTimer = 0f;
			}
			if (this.updateableObjects != null)
			{
				foreach (LevelObject obj in this.updateableObjects)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsServer)
					{
						obj.NetworkUpdateTimer -= deltaTime;
						if (obj.NeedsNetworkSyncing && obj.NetworkUpdateTimer <= 0f)
						{
							GameMain.NetworkMember.CreateEntityEvent(this, new LevelObjectManager.EventData(obj));
							obj.NeedsNetworkSyncing = false;
							obj.NetworkUpdateTimer = NetConfig.LevelObjectUpdateInterval;
						}
					}
					if (!obj.Prefab.HideWhenBroken || obj.Health > 0f)
					{
						if (obj.Triggers != null)
						{
							obj.ActivePrefab = obj.Prefab;
							for (int i = 0; i < obj.Triggers.Count; i++)
							{
								obj.Triggers[i].Update(deltaTime);
								if (obj.Triggers[i].IsTriggered && obj.Prefab.OverrideProperties[i] != null)
								{
									obj.ActivePrefab = obj.Prefab.OverrideProperties[i];
								}
							}
						}
						if (obj.PhysicsBody != null && obj.Prefab.PhysicsBodyTriggerIndex > -1)
						{
							obj.PhysicsBody.Enabled = obj.Triggers[obj.Prefab.PhysicsBodyTriggerIndex].IsTriggered;
						}
					}
				}
			}
		}

		// Token: 0x060027E5 RID: 10213 RVA: 0x00103D50 File Offset: 0x00101F50
		private void OnObjectTriggered(LevelObject triggeredObject, LevelTrigger trigger, Entity triggerer)
		{
			if (trigger.TriggerOthersDistance <= 0f)
			{
				return;
			}
			foreach (LevelObject obj in this.objects)
			{
				if (obj != triggeredObject && obj.Triggers != null)
				{
					foreach (LevelTrigger otherTrigger in obj.Triggers)
					{
						otherTrigger.OtherTriggered(trigger, triggerer);
					}
				}
			}
		}

		// Token: 0x060027E6 RID: 10214 RVA: 0x00103DFC File Offset: 0x00101FFC
		private static LevelObjectPrefab GetRandomPrefab(Level level, IList<LevelObjectPrefab> availablePrefabs)
		{
			if (availablePrefabs.Sum((LevelObjectPrefab p) => p.GetCommonness(level.LevelData)) <= 0f)
			{
				return null;
			}
			return ToolBox.SelectWeightedRandom<LevelObjectPrefab>(availablePrefabs, (from p in availablePrefabs
			select p.GetCommonness(level.LevelData)).ToList<float>(), Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060027E7 RID: 10215 RVA: 0x00103E50 File Offset: 0x00102050
		private static LevelObjectPrefab GetRandomPrefab(CaveGenerationParams caveParams, IList<LevelObjectPrefab> availablePrefabs, bool requireCaveSpecificOverride)
		{
			if (availablePrefabs.Sum((LevelObjectPrefab p) => p.GetCommonness(caveParams, requireCaveSpecificOverride)) <= 0f)
			{
				return null;
			}
			return ToolBox.SelectWeightedRandom<LevelObjectPrefab>(availablePrefabs, (from p in availablePrefabs
			select p.GetCommonness(caveParams, requireCaveSpecificOverride)).ToList<float>(), Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060027E8 RID: 10216 RVA: 0x00103EAC File Offset: 0x001020AC
		public override void Remove()
		{
			LevelObjectManager.objectsInRange.Clear();
			if (this.objects != null)
			{
				foreach (LevelObject obj in this.objects)
				{
					obj.Remove();
				}
				this.objects.Clear();
				this.updateableObjects.Clear();
			}
			base.Remove();
		}

		// Token: 0x060027E9 RID: 10217 RVA: 0x00103F2C File Offset: 0x0010212C
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			if (extraData is LevelObjectManager.EventData)
			{
				LevelObjectManager.EventData eventData = (LevelObjectManager.EventData)extraData;
				LevelObject obj = eventData.LevelObject;
				msg.WriteRangedInteger(this.objects.IndexOf(obj), 0, this.objects.Count);
				obj.ServerWrite(msg, c);
				return;
			}
			throw new Exception("Malformed LevelObjectManager event: expected LevelObjectManager.EventData");
		}

		// Token: 0x04001390 RID: 5008
		private const int GridSize = 2000;

		// Token: 0x04001391 RID: 5009
		private List<LevelObject> objects;

		// Token: 0x04001392 RID: 5010
		private List<LevelObject> updateableObjects;

		// Token: 0x04001393 RID: 5011
		private List<LevelObject>[,] objectGrid;

		// Token: 0x04001394 RID: 5012
		public const float ParallaxStrength = 0.0001f;

		// Token: 0x04001396 RID: 5014
		private static readonly HashSet<LevelObject> objectsInRange = new HashSet<LevelObject>();

		// Token: 0x02000A16 RID: 2582
		private readonly struct EventData : NetEntityEvent.IData
		{
			// Token: 0x06005BE6 RID: 23526 RVA: 0x001FF6C3 File Offset: 0x001FD8C3
			public EventData(LevelObject levelObject)
			{
				this.LevelObject = levelObject;
			}

			// Token: 0x0400352E RID: 13614
			public readonly LevelObject LevelObject;
		}

		// Token: 0x02000A17 RID: 2583
		private class SpawnPosition
		{
			// Token: 0x06005BE7 RID: 23527 RVA: 0x001FF6CC File Offset: 0x001FD8CC
			public SpawnPosition(GraphEdge graphEdge, Vector2 normal, LevelObjectPrefab.SpawnPosType spawnPosType, Alignment alignment) : this(graphEdge, normal, spawnPosType.ToEnumerable<LevelObjectPrefab.SpawnPosType>(), alignment)
			{
			}

			// Token: 0x06005BE8 RID: 23528 RVA: 0x001FF6E0 File Offset: 0x001FD8E0
			public SpawnPosition(GraphEdge graphEdge, Vector2 normal, IEnumerable<LevelObjectPrefab.SpawnPosType> spawnPosTypes, Alignment alignment)
			{
				this.GraphEdge = graphEdge;
				this.Normal = (normal.NearlyEquals(Vector2.Zero) ? Vector2.UnitY : Vector2.Normalize(normal));
				this.SpawnPosTypes.AddRange(spawnPosTypes);
				if (spawnPosTypes.Contains(LevelObjectPrefab.SpawnPosType.MainPath) || spawnPosTypes.Contains(LevelObjectPrefab.SpawnPosType.LevelStart) || spawnPosTypes.Contains(LevelObjectPrefab.SpawnPosType.LevelEnd))
				{
					this.Length = 1000f;
					this.Normal = Vector2.Zero;
					this.Alignment = Alignment.Any;
				}
				else
				{
					this.Alignment = alignment;
					this.Length = Vector2.Distance(graphEdge.Point1, graphEdge.Point2);
				}
				this.noiseVal = (float)(PerlinNoise.CalculatePerlin((double)(this.GraphEdge.Point1.X / 10000f), (double)(this.GraphEdge.Point1.Y / 10000f), 0.5) + PerlinNoise.CalculatePerlin((double)(this.GraphEdge.Point1.X / 20000f), (double)(this.GraphEdge.Point1.Y / 20000f), 0.5));
			}

			// Token: 0x06005BE9 RID: 23529 RVA: 0x001FF814 File Offset: 0x001FDA14
			public float GetSpawnProbability(LevelObjectPrefab prefab)
			{
				if (prefab.ClusteringAmount <= 0f)
				{
					return this.Length;
				}
				float noise = (this.noiseVal + PerlinNoise.GetPerlin(prefab.ClusteringGroup, prefab.ClusteringGroup * 0.3f)) % 1f;
				return this.Length * (float)Math.Pow((double)noise, (double)prefab.ClusteringAmount);
			}

			// Token: 0x0400352F RID: 13615
			public readonly GraphEdge GraphEdge;

			// Token: 0x04003530 RID: 13616
			public readonly Vector2 Normal;

			// Token: 0x04003531 RID: 13617
			public readonly List<LevelObjectPrefab.SpawnPosType> SpawnPosTypes = new List<LevelObjectPrefab.SpawnPosType>();

			// Token: 0x04003532 RID: 13618
			public readonly Alignment Alignment;

			// Token: 0x04003533 RID: 13619
			public readonly float Length;

			// Token: 0x04003534 RID: 13620
			private readonly float noiseVal;
		}
	}
}
