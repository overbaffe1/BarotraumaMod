using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000DC RID: 220
	internal class LevelObjectManager : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x06001E53 RID: 7763 RVA: 0x0012CC74 File Offset: 0x0012AE74
		public IEnumerable<ILevelRenderableObject> GetAllVisibleObjects()
		{
			this.allVisibleObjects.Clear();
			foreach (ILevelRenderableObject obj in this.visibleObjectsBack)
			{
				this.allVisibleObjects.Add(obj);
			}
			foreach (ILevelRenderableObject obj2 in this.visibleObjectsMid)
			{
				this.allVisibleObjects.Add(obj2);
			}
			foreach (ILevelRenderableObject obj3 in this.visibleObjectsFront)
			{
				this.allVisibleObjects.Add(obj3);
			}
			return this.allVisibleObjects;
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x0012CD74 File Offset: 0x0012AF74
		private void RefreshVisibleObjects(Rectangle currentIndices, BackgroundCreatureManager backgroundCreatureManager, float zoom)
		{
			LevelObjectManager.<>c__DisplayClass9_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.zoom = zoom;
			this.visibleObjectsBack.Clear();
			this.visibleObjectsMid.Clear();
			this.visibleObjectsFront.Clear();
			CS$<>8__locals1.minSizeToDraw = MathHelper.Lerp(10f, 5f, Math.Min(CS$<>8__locals1.zoom * 20f, 1f));
			int midIndexX = (currentIndices.X + currentIndices.Width) / 2;
			int midIndexY = (currentIndices.Y + currentIndices.Height) / 2;
			this.<RefreshVisibleObjects>g__CheckIndex|9_0(midIndexX, midIndexY, ref CS$<>8__locals1);
			for (int x = currentIndices.X; x <= currentIndices.Width; x++)
			{
				for (int y = currentIndices.Y; y <= currentIndices.Height; y++)
				{
					if (x != midIndexX || y != midIndexY)
					{
						this.<RefreshVisibleObjects>g__CheckIndex|9_0(x, y, ref CS$<>8__locals1);
					}
				}
			}
			foreach (BackgroundCreature backgroundCreature in backgroundCreatureManager.VisibleCreatures)
			{
				int drawOrderIndex = 0;
				int i = 0;
				while (i < this.visibleObjectsBack.Count && this.visibleObjectsBack[i].Position.Z <= backgroundCreature.Position.Z)
				{
					drawOrderIndex = i + 1;
					if (drawOrderIndex >= 600)
					{
						break;
					}
					i++;
				}
				if (drawOrderIndex >= 0 && drawOrderIndex < 600)
				{
					this.visibleObjectsBack.Insert(drawOrderIndex, backgroundCreature);
				}
			}
			this.visibleObjectsBack.Reverse();
			this.visibleObjectsMid.Reverse();
			this.visibleObjectsFront.Reverse();
			this.currentGridIndices = currentIndices;
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x0012CF24 File Offset: 0x0012B124
		public void DrawObjectsBack(SpriteBatch spriteBatch, BackgroundCreatureManager backgroundCreatureManager, Camera cam)
		{
			this.DrawObjects(spriteBatch, cam, backgroundCreatureManager, this.visibleObjectsBack);
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x0012CF35 File Offset: 0x0012B135
		public void DrawObjectsMid(SpriteBatch spriteBatch, BackgroundCreatureManager backgroundCreatureManager, Camera cam)
		{
			this.DrawObjects(spriteBatch, cam, backgroundCreatureManager, this.visibleObjectsMid);
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x0012CF46 File Offset: 0x0012B146
		public void DrawObjectsFront(SpriteBatch spriteBatch, BackgroundCreatureManager backgroundCreatureManager, Camera cam)
		{
			this.DrawObjects(spriteBatch, cam, backgroundCreatureManager, this.visibleObjectsFront);
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x0012CF58 File Offset: 0x0012B158
		private void DrawObjects(SpriteBatch spriteBatch, Camera cam, BackgroundCreatureManager backgroundCreatureManager, List<ILevelRenderableObject> objectList)
		{
			Rectangle indices = Rectangle.Empty;
			indices.X = (int)Math.Floor((double)((float)cam.WorldView.X / 2000f));
			if (indices.X >= this.objectGrid.GetLength(0))
			{
				return;
			}
			indices.Y = (int)Math.Floor((double)((float)(cam.WorldView.Y - cam.WorldView.Height - Level.Loaded.BottomPos) / 2000f));
			if (indices.Y >= this.objectGrid.GetLength(1))
			{
				return;
			}
			indices.Width = (int)Math.Floor((double)((float)cam.WorldView.Right / 2000f)) + 1;
			if (indices.Width < 0)
			{
				return;
			}
			indices.Height = (int)Math.Floor((double)((float)(cam.WorldView.Y - Level.Loaded.BottomPos) / 2000f)) + 1;
			if (indices.Height < 0)
			{
				return;
			}
			indices.X = Math.Max(indices.X, 0);
			indices.Y = Math.Max(indices.Y, 0);
			indices.Width = Math.Min(indices.Width, this.objectGrid.GetLength(0) - 1);
			indices.Height = Math.Min(indices.Height, this.objectGrid.GetLength(1) - 1);
			float z = 0f;
			if (this.ForceRefreshVisibleObjects || (this.currentGridIndices != indices && Timing.TotalTime > this.NextRefreshTime))
			{
				this.RefreshVisibleObjects(indices, backgroundCreatureManager, cam.Zoom);
				this.ForceRefreshVisibleObjects = false;
				if (cam.Zoom < 0.1f)
				{
					this.NextRefreshTime = Timing.TotalTime + (double)MathHelper.Lerp(1f, 0f, cam.Zoom * 10f);
				}
			}
			bool prevObjectHasDeformableSprite = false;
			foreach (ILevelRenderableObject obj2 in objectList)
			{
				Vector2 camDiff = new Vector2(obj2.Position.X, obj2.Position.Y) - cam.WorldViewCenter;
				camDiff.Y = -camDiff.Y;
				bool hasDeformableSprite = false;
				LevelObject levelObject = obj2 as LevelObject;
				if (levelObject == null)
				{
					goto IL_5D6;
				}
				hasDeformableSprite = (levelObject.ActivePrefab.DeformableSprite != null);
				if (hasDeformableSprite != prevObjectHasDeformableSprite)
				{
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null, new Matrix?(cam.Transform));
				}
				Sprite activeSprite = levelObject.Sprite;
				if (activeSprite != null)
				{
					activeSprite.Draw(spriteBatch, new Vector2(levelObject.Position.X, -levelObject.Position.Y) - camDiff * levelObject.Position.Z * 0.0001f, Color.Lerp(levelObject.Prefab.SpriteColor, levelObject.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), levelObject.Position.Z / levelObject.Prefab.FadeOutDepth), activeSprite.Origin, levelObject.CurrentRotation, levelObject.CurrentScale, SpriteEffects.None, new float?(z));
				}
				if (hasDeformableSprite)
				{
					if (levelObject.CurrentSpriteDeformation != null)
					{
						levelObject.ActivePrefab.DeformableSprite.Deform(levelObject.CurrentSpriteDeformation);
					}
					else
					{
						levelObject.ActivePrefab.DeformableSprite.Reset();
					}
					DeformableSprite deformableSprite = levelObject.ActivePrefab.DeformableSprite;
					if (deformableSprite != null)
					{
						deformableSprite.Draw(cam, new Vector3(new Vector2(levelObject.Position.X, levelObject.Position.Y) - camDiff * levelObject.Position.Z * 0.0001f, z * 10f), levelObject.ActivePrefab.DeformableSprite.Origin, levelObject.CurrentRotation, levelObject.CurrentScale, Color.Lerp(levelObject.Prefab.SpriteColor, levelObject.Prefab.SpriteColor.Multiply(Level.Loaded.BackgroundTextureColor), levelObject.Position.Z / 5000f), false, false);
					}
				}
				prevObjectHasDeformableSprite = hasDeformableSprite;
				if (GameMain.DebugDraw)
				{
					GUI.DrawRectangle(spriteBatch, new Vector2(levelObject.Position.X, -levelObject.Position.Y), new Vector2(10f, 10f), GUIStyle.Red, true, 0f, 1f);
					if (levelObject.Triggers != null)
					{
						using (List<LevelTrigger>.Enumerator enumerator2 = levelObject.Triggers.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								LevelTrigger trigger = enumerator2.Current;
								if (trigger.PhysicsBody != null)
								{
									GUI.DrawLine(spriteBatch, new Vector2(levelObject.Position.X, -levelObject.Position.Y), new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y), Color.Cyan, 0f, 3f);
									Vector2 flowForce = trigger.GetWaterFlowVelocity();
									if (flowForce.LengthSquared() > 1f)
									{
										flowForce.Y = -flowForce.Y;
										GUI.DrawLine(spriteBatch, new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y), new Vector2(trigger.WorldPosition.X, -trigger.WorldPosition.Y) + flowForce * 10f, GUIStyle.Orange, 0f, 5f);
									}
									trigger.PhysicsBody.UpdateDrawPosition(true);
									trigger.PhysicsBody.DebugDraw(spriteBatch, trigger.IsTriggered ? Color.Cyan : Color.DarkCyan, false);
								}
							}
							goto IL_638;
						}
						goto IL_5D6;
					}
					continue;
				}
				IL_638:
				prevObjectHasDeformableSprite = hasDeformableSprite;
				z += 0.0001f;
				continue;
				IL_5D6:
				BackgroundCreature backgroundCreature = obj2 as BackgroundCreature;
				if (backgroundCreature != null && cam.Zoom > 0.05f)
				{
					hasDeformableSprite = (backgroundCreature.Prefab.DeformableSprite != null);
					if (hasDeformableSprite != prevObjectHasDeformableSprite)
					{
						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null, new Matrix?(cam.Transform));
					}
					backgroundCreature.Draw(spriteBatch, cam);
					goto IL_638;
				}
				goto IL_638;
			}
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x0012D5F8 File Offset: 0x0012B7F8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int objIndex = msg.ReadRangedInteger(0, this.objects.Count);
			this.objects[objIndex].ClientRead(msg);
		}

		// Token: 0x1700081A RID: 2074
		// (get) Token: 0x06001E5A RID: 7770 RVA: 0x0012D62A File Offset: 0x0012B82A
		// (set) Token: 0x06001E5B RID: 7771 RVA: 0x0012D632 File Offset: 0x0012B832
		public float GlobalForceDecreaseTimer { get; private set; }

		// Token: 0x06001E5C RID: 7772 RVA: 0x0012D63C File Offset: 0x0012B83C
		public LevelObjectManager() : base(null, 0)
		{
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x0012D694 File Offset: 0x0012B894
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

		// Token: 0x06001E5E RID: 7774 RVA: 0x0012E114 File Offset: 0x0012C314
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

		// Token: 0x06001E5F RID: 7775 RVA: 0x0012E3E0 File Offset: 0x0012C5E0
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

		// Token: 0x06001E60 RID: 7776 RVA: 0x0012E764 File Offset: 0x0012C964
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
			if (newObject.ParticleEmitters != null)
			{
				foreach (ParticleEmitter emitter in newObject.ParticleEmitters)
				{
					Rectangle particleBounds = emitter.CalculateParticleBounds(new Vector2(newObject.Position.X, newObject.Position.Y));
					minX = Math.Min(minX, (float)particleBounds.X);
					maxX = Math.Max(maxX, (float)particleBounds.Right);
					minY = Math.Min(minY, (float)(particleBounds.Y - level.BottomPos));
					maxY = Math.Max(maxY, (float)(particleBounds.Bottom - level.BottomPos));
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

		// Token: 0x06001E61 RID: 7777 RVA: 0x0012EF38 File Offset: 0x0012D138
		public static Point GetGridIndices(Vector2 worldPosition)
		{
			return new Point((int)Math.Floor((double)(worldPosition.X / 2000f)), (int)Math.Floor((double)((worldPosition.Y - (float)Level.Loaded.BottomPos) / 2000f)));
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x0012EF71 File Offset: 0x0012D171
		public IEnumerable<LevelObject> GetAllObjects()
		{
			return this.objects;
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x0012EF7C File Offset: 0x0012D17C
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

		// Token: 0x06001E64 RID: 7780 RVA: 0x0012F124 File Offset: 0x0012D324
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

		// Token: 0x06001E65 RID: 7781 RVA: 0x0012F2F0 File Offset: 0x0012D4F0
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
			this.UpdateProjSpecific(deltaTime, cam);
		}

		// Token: 0x06001E66 RID: 7782 RVA: 0x0012F4B8 File Offset: 0x0012D6B8
		private void UpdateProjSpecific(float deltaTime, Camera cam)
		{
			foreach (ILevelRenderableObject obj in this.visibleObjectsBack)
			{
				LevelObject levelObj = obj as LevelObject;
				if (levelObj != null)
				{
					levelObj.Update(deltaTime, cam);
				}
			}
			foreach (ILevelRenderableObject obj2 in this.visibleObjectsMid)
			{
				LevelObject levelObj2 = obj2 as LevelObject;
				if (levelObj2 != null)
				{
					levelObj2.Update(deltaTime, cam);
				}
			}
			foreach (ILevelRenderableObject obj3 in this.visibleObjectsFront)
			{
				LevelObject levelObj3 = obj3 as LevelObject;
				if (levelObj3 != null)
				{
					levelObj3.Update(deltaTime, cam);
				}
			}
		}

		// Token: 0x06001E67 RID: 7783 RVA: 0x0012F5BC File Offset: 0x0012D7BC
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

		// Token: 0x06001E68 RID: 7784 RVA: 0x0012F668 File Offset: 0x0012D868
		private static LevelObjectPrefab GetRandomPrefab(Level level, IList<LevelObjectPrefab> availablePrefabs)
		{
			if (availablePrefabs.Sum((LevelObjectPrefab p) => p.GetCommonness(level.LevelData)) <= 0f)
			{
				return null;
			}
			return ToolBox.SelectWeightedRandom<LevelObjectPrefab>(availablePrefabs, (from p in availablePrefabs
			select p.GetCommonness(level.LevelData)).ToList<float>(), Rand.RandSync.ServerAndClient);
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x0012F6BC File Offset: 0x0012D8BC
		private static LevelObjectPrefab GetRandomPrefab(CaveGenerationParams caveParams, IList<LevelObjectPrefab> availablePrefabs, bool requireCaveSpecificOverride)
		{
			if (availablePrefabs.Sum((LevelObjectPrefab p) => p.GetCommonness(caveParams, requireCaveSpecificOverride)) <= 0f)
			{
				return null;
			}
			return ToolBox.SelectWeightedRandom<LevelObjectPrefab>(availablePrefabs, (from p in availablePrefabs
			select p.GetCommonness(caveParams, requireCaveSpecificOverride)).ToList<float>(), Rand.RandSync.ServerAndClient);
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x0012F718 File Offset: 0x0012D918
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
			this.RemoveProjSpecific();
			base.Remove();
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x0012F7A0 File Offset: 0x0012D9A0
		private void RemoveProjSpecific()
		{
			this.visibleObjectsBack.Clear();
			this.visibleObjectsMid.Clear();
			this.visibleObjectsFront.Clear();
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x0012F7C4 File Offset: 0x0012D9C4
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

		// Token: 0x06001E6E RID: 7790 RVA: 0x0012F828 File Offset: 0x0012DA28
		[CompilerGenerated]
		private void <RefreshVisibleObjects>g__CheckIndex|9_0(int x, int y, ref LevelObjectManager.<>c__DisplayClass9_0 A_3)
		{
			if (this.objectGrid[x, y] == null)
			{
				return;
			}
			foreach (LevelObject obj in this.objectGrid[x, y])
			{
				if (obj.CanBeVisible && (!obj.Prefab.HideWhenBroken || obj.Health > 0f))
				{
					if (A_3.zoom < 0.05f)
					{
						if (obj.Sprite != null && Math.Min(obj.Sprite.size.X * A_3.zoom, obj.Sprite.size.Y * A_3.zoom) < 5f)
						{
							continue;
						}
						LevelObjectPrefab activePrefab = obj.ActivePrefab;
						if (((activePrefab != null) ? activePrefab.DeformableSprite : null) != null && Math.Min(obj.ActivePrefab.DeformableSprite.Sprite.size.X * A_3.zoom, obj.ActivePrefab.DeformableSprite.Sprite.size.Y * A_3.zoom) < A_3.minSizeToDraw)
						{
							continue;
						}
						float zCutoff = MathHelper.Lerp(5000f, 500f, (0.05f - A_3.zoom) * 20f);
						if (obj.Position.Z > zCutoff)
						{
							continue;
						}
					}
					List<ILevelRenderableObject> objectList = (obj.Position.Z >= 0f) ? this.visibleObjectsBack : ((obj.Position.Z < -1f) ? this.visibleObjectsFront : this.visibleObjectsMid);
					if (objectList.Count < 600)
					{
						int drawOrderIndex = 0;
						for (int i = 0; i < objectList.Count; i++)
						{
							if (objectList[i] == obj)
							{
								drawOrderIndex = -1;
								break;
							}
							if (objectList[i].Position.Z > obj.Position.Z)
							{
								break;
							}
							drawOrderIndex = i + 1;
							if (drawOrderIndex >= 600)
							{
								break;
							}
						}
						if (drawOrderIndex >= 0 && drawOrderIndex < 600)
						{
							objectList.Insert(drawOrderIndex, obj);
						}
					}
				}
			}
		}

		// Token: 0x04000F78 RID: 3960
		private readonly List<ILevelRenderableObject> visibleObjectsBack = new List<ILevelRenderableObject>(600);

		// Token: 0x04000F79 RID: 3961
		private readonly List<ILevelRenderableObject> visibleObjectsMid = new List<ILevelRenderableObject>(600);

		// Token: 0x04000F7A RID: 3962
		private readonly List<ILevelRenderableObject> visibleObjectsFront = new List<ILevelRenderableObject>(600);

		// Token: 0x04000F7B RID: 3963
		private readonly HashSet<ILevelRenderableObject> allVisibleObjects = new HashSet<ILevelRenderableObject>(600);

		// Token: 0x04000F7C RID: 3964
		private double NextRefreshTime;

		// Token: 0x04000F7D RID: 3965
		private const int MaxVisibleObjects = 600;

		// Token: 0x04000F7E RID: 3966
		private Rectangle currentGridIndices;

		// Token: 0x04000F7F RID: 3967
		public bool ForceRefreshVisibleObjects;

		// Token: 0x04000F80 RID: 3968
		private const int GridSize = 2000;

		// Token: 0x04000F81 RID: 3969
		private List<LevelObject> objects;

		// Token: 0x04000F82 RID: 3970
		private List<LevelObject> updateableObjects;

		// Token: 0x04000F83 RID: 3971
		private List<LevelObject>[,] objectGrid;

		// Token: 0x04000F84 RID: 3972
		public const float ParallaxStrength = 0.0001f;

		// Token: 0x04000F86 RID: 3974
		private static readonly HashSet<LevelObject> objectsInRange = new HashSet<LevelObject>();

		// Token: 0x02000B38 RID: 2872
		private readonly struct EventData : NetEntityEvent.IData
		{
			// Token: 0x060077E9 RID: 30697 RVA: 0x0037C105 File Offset: 0x0037A305
			public EventData(LevelObject levelObject)
			{
				this.LevelObject = levelObject;
			}

			// Token: 0x040046F7 RID: 18167
			public readonly LevelObject LevelObject;
		}

		// Token: 0x02000B39 RID: 2873
		private class SpawnPosition
		{
			// Token: 0x060077EA RID: 30698 RVA: 0x0037C10E File Offset: 0x0037A30E
			public SpawnPosition(GraphEdge graphEdge, Vector2 normal, LevelObjectPrefab.SpawnPosType spawnPosType, Alignment alignment) : this(graphEdge, normal, spawnPosType.ToEnumerable<LevelObjectPrefab.SpawnPosType>(), alignment)
			{
			}

			// Token: 0x060077EB RID: 30699 RVA: 0x0037C120 File Offset: 0x0037A320
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

			// Token: 0x060077EC RID: 30700 RVA: 0x0037C254 File Offset: 0x0037A454
			public float GetSpawnProbability(LevelObjectPrefab prefab)
			{
				if (prefab.ClusteringAmount <= 0f)
				{
					return this.Length;
				}
				float noise = (this.noiseVal + PerlinNoise.GetPerlin(prefab.ClusteringGroup, prefab.ClusteringGroup * 0.3f)) % 1f;
				return this.Length * (float)Math.Pow((double)noise, (double)prefab.ClusteringAmount);
			}

			// Token: 0x040046F8 RID: 18168
			public readonly GraphEdge GraphEdge;

			// Token: 0x040046F9 RID: 18169
			public readonly Vector2 Normal;

			// Token: 0x040046FA RID: 18170
			public readonly List<LevelObjectPrefab.SpawnPosType> SpawnPosTypes = new List<LevelObjectPrefab.SpawnPosType>();

			// Token: 0x040046FB RID: 18171
			public readonly Alignment Alignment;

			// Token: 0x040046FC RID: 18172
			public readonly float Length;

			// Token: 0x040046FD RID: 18173
			private readonly float noiseVal;
		}
	}
}
