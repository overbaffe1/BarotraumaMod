using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000237 RID: 567
	internal static class CaveGenerator
	{
		// Token: 0x060026CB RID: 9931 RVA: 0x000FDA78 File Offset: 0x000FBC78
		public static List<VoronoiCell> GraphEdgesToCells(List<GraphEdge> graphEdges, Rectangle borders, float gridCellSize, out List<VoronoiCell>[,] cellGrid)
		{
			List<VoronoiCell> cells = new List<VoronoiCell>();
			cellGrid = new List<VoronoiCell>[(int)Math.Ceiling((double)((float)borders.Width / gridCellSize)), (int)Math.Ceiling((double)((float)borders.Height / gridCellSize))];
			int x = 0;
			while ((float)x < (float)borders.Width / gridCellSize)
			{
				int y = 0;
				while ((float)y < (float)borders.Height / gridCellSize)
				{
					cellGrid[x, y] = new List<VoronoiCell>();
					y++;
				}
				x++;
			}
			foreach (GraphEdge ge in graphEdges)
			{
				if (Vector2.DistanceSquared(ge.Point1, ge.Point2) >= 0.001f)
				{
					for (int i = 0; i < 2; i++)
					{
						Site site = (i == 0) ? ge.Site1 : ge.Site2;
						int x2 = (int)Math.Floor((site.Coord.X - (double)borders.X) / (double)gridCellSize);
						int y2 = (int)Math.Floor((site.Coord.Y - (double)borders.Y) / (double)gridCellSize);
						x2 = MathHelper.Clamp(x2, 0, cellGrid.GetLength(0) - 1);
						y2 = MathHelper.Clamp(y2, 0, cellGrid.GetLength(1) - 1);
						VoronoiCell cell = cellGrid[x2, y2].Find((VoronoiCell c) => c.Site == site);
						if (cell == null)
						{
							cell = new VoronoiCell(site);
							cellGrid[x2, y2].Add(cell);
							cells.Add(cell);
						}
						if (ge.Cell1 == null)
						{
							ge.Cell1 = cell;
						}
						else
						{
							ge.Cell2 = cell;
						}
						cell.Edges.Add(ge);
					}
				}
			}
			foreach (VoronoiCell cell2 in cells)
			{
				Vector2? point = null;
				Vector2? point2 = null;
				foreach (GraphEdge ge2 in cell2.Edges)
				{
					if (MathUtils.NearlyEqual(ge2.Point1.X, (float)borders.X, 0.0001f) || MathUtils.NearlyEqual(ge2.Point1.X, (float)borders.Right, 0.0001f) || MathUtils.NearlyEqual(ge2.Point1.Y, (float)borders.Y, 0.0001f) || MathUtils.NearlyEqual(ge2.Point1.Y, (float)borders.Bottom, 0.0001f))
					{
						if (point == null)
						{
							point = new Vector2?(ge2.Point1);
						}
						else if (point2 == null)
						{
							if (MathUtils.NearlyEqual(point.Value, ge2.Point1, 0.0001f))
							{
								continue;
							}
							point2 = new Vector2?(ge2.Point1);
						}
					}
					if (MathUtils.NearlyEqual(ge2.Point2.X, (float)borders.X, 0.0001f) || MathUtils.NearlyEqual(ge2.Point2.X, (float)borders.Right, 0.0001f) || MathUtils.NearlyEqual(ge2.Point2.Y, (float)borders.Y, 0.0001f) || MathUtils.NearlyEqual(ge2.Point2.Y, (float)borders.Bottom, 0.0001f))
					{
						if (point == null)
						{
							point = new Vector2?(ge2.Point2);
						}
						else
						{
							if (MathUtils.NearlyEqual(point.Value, ge2.Point2, 0.0001f))
							{
								continue;
							}
							point2 = new Vector2?(ge2.Point2);
						}
					}
					if (point != null && point2 != null)
					{
						bool point1OnSide = MathUtils.NearlyEqual(point.Value.X, (float)borders.X, 0.0001f) || MathUtils.NearlyEqual(point.Value.X, (float)borders.Right, 0.0001f);
						bool point2OnSide = MathUtils.NearlyEqual(point2.Value.X, (float)borders.X, 0.0001f) || MathUtils.NearlyEqual(point2.Value.X, (float)borders.Right, 0.0001f);
						if (point1OnSide != point2OnSide)
						{
							Vector2 cornerPos = new Vector2((float)((point.Value.X < (float)borders.Center.X) ? borders.X : borders.Right), (float)((point.Value.Y < (float)borders.Center.Y) ? borders.Y : borders.Bottom));
							cell2.Edges.Add(new GraphEdge(point.Value, cornerPos)
							{
								Cell1 = cell2,
								IsSolid = true,
								Site1 = cell2.Site,
								OutsideLevel = true
							});
							cell2.Edges.Add(new GraphEdge(point2.Value, cornerPos)
							{
								Cell1 = cell2,
								IsSolid = true,
								Site1 = cell2.Site,
								OutsideLevel = true
							});
							break;
						}
						cell2.Edges.Add(new GraphEdge(point.Value, point2.Value)
						{
							Cell1 = cell2,
							IsSolid = true,
							Site1 = cell2.Site,
							OutsideLevel = true
						});
						break;
					}
				}
			}
			return cells;
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x000FE06C File Offset: 0x000FC26C
		public static void GeneratePath(Level.Tunnel tunnel, Level level)
		{
			List<VoronoiCell> targetCells = new List<VoronoiCell>();
			for (int i = 0; i < tunnel.Nodes.Count; i++)
			{
				VoronoiCell closestCell = level.GetClosestCell(tunnel.Nodes[i].ToVector2());
				if (closestCell != null && !targetCells.Contains(closestCell))
				{
					targetCells.Add(closestCell);
				}
			}
			tunnel.Cells.AddRange(CaveGenerator.GeneratePath(targetCells, level.GetAllCells()));
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x000FE0DC File Offset: 0x000FC2DC
		public static List<VoronoiCell> GeneratePath(List<VoronoiCell> targetCells, List<VoronoiCell> cells)
		{
			List<VoronoiCell> pathCells = new List<VoronoiCell>();
			if (targetCells.Count == 0)
			{
				return pathCells;
			}
			VoronoiCell currentCell = targetCells[0];
			currentCell.CellType = CellType.Path;
			pathCells.Add(currentCell);
			int currentTargetIndex = 0;
			int iterationsLeft = cells.Count / 2;
			Func<VoronoiCell, bool> <>9__0;
			do
			{
				int edgeIndex = 0;
				double smallestDist = double.PositiveInfinity;
				for (int i = 0; i < currentCell.Edges.Count; i++)
				{
					VoronoiCell adjacentCell = currentCell.Edges[i].AdjacentCell(currentCell);
					if (adjacentCell != null)
					{
						double dist = MathUtils.Distance(adjacentCell.Site.Coord.X, adjacentCell.Site.Coord.Y, targetCells[currentTargetIndex].Site.Coord.X, targetCells[currentTargetIndex].Site.Coord.Y);
						dist += MathUtils.Distance(adjacentCell.Site.Coord.X, adjacentCell.Site.Coord.Y, currentCell.Site.Coord.X, currentCell.Site.Coord.Y) * 0.5;
						if (Vector2.DistanceSquared(currentCell.Edges[i].Point1, currentCell.Edges[i].Point2) < 22500f)
						{
							double num = dist;
							double num2 = (double)10f;
							IEnumerable<VoronoiCell> source = pathCells;
							Func<VoronoiCell, bool> predicate;
							if ((predicate = <>9__0) == null)
							{
								predicate = (<>9__0 = ((VoronoiCell c) => c == currentCell));
							}
							dist = num * (num2 / (double)Math.Max((float)source.Count(predicate), 1f));
						}
						if (dist < smallestDist)
						{
							edgeIndex = i;
							smallestDist = dist;
						}
					}
				}
				currentCell = currentCell.Edges[edgeIndex].AdjacentCell(currentCell);
				currentCell.CellType = CellType.Path;
				pathCells.Add(currentCell);
				iterationsLeft--;
				if (currentCell == targetCells[currentTargetIndex])
				{
					currentTargetIndex++;
					if (currentTargetIndex >= targetCells.Count)
					{
						break;
					}
				}
			}
			while (currentCell != targetCells[targetCells.Count - 1] && iterationsLeft > 0);
			return pathCells;
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x000FE33C File Offset: 0x000FC53C
		public static void RoundCell(VoronoiCell cell, float minEdgeLength = 500f, float roundingAmount = 0.5f, float irregularity = 0.1f, float minThickness = 0f)
		{
			CompareCCW compareCCW = new CompareCCW(cell.Center);
			List<GraphEdge> tempEdges = new List<GraphEdge>();
			foreach (GraphEdge edge in cell.Edges)
			{
				if (!edge.IsSolid || edge.OutsideLevel)
				{
					tempEdges.Add(edge);
				}
				else
				{
					Vector2 edgeDiff = edge.Point2 - edge.Point1;
					Vector2 edgeDir = Vector2.Normalize(edgeDiff);
					float maxExtrusion = float.PositiveInfinity;
					VoronoiCell adjacentEmptyCell = edge.AdjacentCell(cell);
					if (adjacentEmptyCell != null && adjacentEmptyCell.CellType == CellType.Solid)
					{
						adjacentEmptyCell = null;
					}
					if (adjacentEmptyCell != null)
					{
						GraphEdge adjacentEdge = null;
						foreach (GraphEdge otherEdge in adjacentEmptyCell.Edges)
						{
							if (otherEdge != edge)
							{
								VoronoiCell voronoiCell = otherEdge.AdjacentCell(adjacentEmptyCell);
								if (voronoiCell != null && voronoiCell.CellType <= CellType.Solid)
								{
									Vector2 otherEdgeDir = Vector2.Normalize(otherEdge.Point2 - otherEdge.Point1);
									if (Math.Abs(Vector2.Dot(otherEdgeDir, edgeDir)) > 0.7f)
									{
										adjacentEdge = otherEdge;
										break;
									}
								}
							}
						}
						if (adjacentEdge != null)
						{
							maxExtrusion = new float[]
							{
								Vector2.Distance(edge.Point1, adjacentEdge.Point1),
								Vector2.Distance(edge.Point1, adjacentEdge.Point2),
								Vector2.Distance(edge.Point1, adjacentEdge.Point2),
								Vector2.Distance(edge.Point2, adjacentEdge.Point1)
							}.Min();
							maxExtrusion = Math.Max(0f, maxExtrusion - 200f);
						}
					}
					List<Vector2> edgePoints = new List<Vector2>();
					Vector2 edgeNormal = edge.GetNormal(cell);
					float edgeLength = Vector2.Distance(edge.Point1, edge.Point2);
					int pointCount = (int)Math.Max(Math.Ceiling((double)(edgeLength / minEdgeLength)), 1.0);
					for (int i = 0; i <= pointCount; i++)
					{
						if (i == 0)
						{
							edgePoints.Add(edge.Point1);
						}
						else if (i == pointCount)
						{
							edgePoints.Add(edge.Point2);
						}
						else
						{
							float centerF = 0.5f - Math.Abs(0.5f - (float)i / (float)pointCount);
							centerF = MathF.Sin(centerF * 3.1415927f);
							float randomVariance = irregularity * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient);
							float extrusionAmount = edgeLength * (roundingAmount * 0.25f * centerF + randomVariance * 0.25f);
							extrusionAmount = Math.Min(extrusionAmount, maxExtrusion);
							Vector2 nonExtrudedPoint = edge.Point1 + edgeDiff * ((float)i / (float)pointCount);
							Vector2 nextPoint = edge.Point1 + edgeDiff * ((float)(i + 1) / (float)pointCount);
							if (extrusionAmount < 0f && minThickness > 0f)
							{
								foreach (GraphEdge otherEdge2 in cell.Edges)
								{
									if (otherEdge2 != edge)
									{
										float margin = minThickness * (float)Math.Sign(extrusionAmount);
										Vector2 intersection;
										if (MathUtils.GetLineIntersection(nonExtrudedPoint, nonExtrudedPoint + edgeNormal * (extrusionAmount + margin), otherEdge2.Point1, otherEdge2.Point2, false, out intersection))
										{
											extrusionAmount = Math.Min(extrusionAmount, Vector2.Distance(edge.Point1, intersection)) - margin;
											extrusionAmount = Math.Min(extrusionAmount, edge.Length / 2f);
										}
									}
								}
							}
							Vector2 extrudedPoint = nonExtrudedPoint + edgeNormal * extrusionAmount;
							List<VoronoiCell> nearbyCells = Level.Loaded.GetCells(extrudedPoint, 2);
							bool isInside = false;
							foreach (VoronoiCell nearbyCell in nearbyCells)
							{
								if (nearbyCell != cell && nearbyCell.CellType == CellType.Solid)
								{
									if (nearbyCell.IsPointInside(extrudedPoint))
									{
										isInside = true;
										break;
									}
									Vector2 triangleCenter = (edge.Point1 + edge.Point2 + extrudedPoint) / 3f;
									foreach (GraphEdge nearbyEdge in nearbyCell.Edges)
									{
										if (!MathUtils.LineSegmentsIntersect(nearbyEdge.Point1, triangleCenter, edge.Point1, extrudedPoint) && !MathUtils.LineSegmentsIntersect(nearbyEdge.Point1, triangleCenter, edge.Point2, extrudedPoint) && !MathUtils.LineSegmentsIntersect(nearbyEdge.Point1, triangleCenter, edge.Point1, edge.Point2))
										{
											isInside = true;
											break;
										}
									}
									if (isInside)
									{
										break;
									}
								}
							}
							if (!isInside && Vector2.Dot(edgeNormal, GraphEdge.GetNormal(cell, edgePoints.Last<Vector2>(), extrudedPoint)) >= 0f && Vector2.Dot(edgeNormal, GraphEdge.GetNormal(cell, extrudedPoint, edge.Point2)) >= 0f && compareCCW.Compare(edgePoints.Last<Vector2>(), nonExtrudedPoint) == compareCCW.Compare(edgePoints.Last<Vector2>(), extrudedPoint) && compareCCW.Compare(nonExtrudedPoint, nextPoint) == compareCCW.Compare(extrudedPoint, nextPoint))
							{
								edgePoints.Add(extrudedPoint);
							}
						}
					}
					for (int j = 0; j < edgePoints.Count - 1; j++)
					{
						tempEdges.Add(new GraphEdge(edgePoints[j], edgePoints[j + 1])
						{
							Cell1 = edge.Cell1,
							Cell2 = edge.Cell2,
							IsSolid = edge.IsSolid,
							Site1 = edge.Site1,
							Site2 = edge.Site2,
							OutsideLevel = edge.OutsideLevel,
							NextToCave = edge.NextToCave,
							NextToMainPath = edge.NextToMainPath,
							NextToSidePath = edge.NextToSidePath
						});
					}
				}
			}
			cell.Edges = tempEdges;
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x000FE9B0 File Offset: 0x000FCBB0
		public static Body GeneratePolygons(List<VoronoiCell> cells, Level level, out List<Vector2[]> renderTriangles)
		{
			renderTriangles = new List<Vector2[]>();
			List<Vector2> tempVertices = new List<Vector2>();
			List<Vector2> bodyPoints = new List<Vector2>();
			Body cellBody = new Body
			{
				SleepingAllowed = false,
				BodyType = BodyType.Static,
				CollisionCategories = Category.Cat8
			};
			GameMain.World.Add(cellBody, false);
			for (int i = cells.Count - 1; i >= 0; i--)
			{
				VoronoiCell cell = cells[i];
				bodyPoints.Clear();
				tempVertices.Clear();
				using (List<GraphEdge>.Enumerator enumerator = cell.Edges.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GraphEdge ge = enumerator.Current;
						if (Vector2.DistanceSquared(ge.Point1, ge.Point2) >= 0.01f)
						{
							if (!tempVertices.Any((Vector2 v) => Vector2.DistanceSquared(ge.Point1, v) < 1f))
							{
								tempVertices.Add(ge.Point1);
								bodyPoints.Add(ge.Point1);
							}
							if (!tempVertices.Any((Vector2 v) => Vector2.DistanceSquared(ge.Point2, v) < 1f))
							{
								tempVertices.Add(ge.Point2);
								bodyPoints.Add(ge.Point2);
							}
						}
					}
				}
				if (tempVertices.Count < 3 || bodyPoints.Count < 2)
				{
					cells.RemoveAt(i);
				}
				else
				{
					renderTriangles.AddRange(MathUtils.TriangulateConvexHull(tempVertices, cell.Center));
					if (bodyPoints.Count >= 2)
					{
						if (bodyPoints.Count < 3)
						{
							foreach (Vector2 vertex in tempVertices)
							{
								if (!bodyPoints.Contains(vertex))
								{
									bodyPoints.Add(vertex);
									break;
								}
							}
						}
						for (int j = 0; j < bodyPoints.Count; j++)
						{
							cell.BodyVertices.Add(bodyPoints[j]);
							bodyPoints[j] = ConvertUnits.ToSimUnits(bodyPoints[j]);
						}
						if (cell.CellType != CellType.Empty)
						{
							cellBody.UserData = cell;
							List<Vector2[]> triangles = MathUtils.TriangulateConvexHull(bodyPoints, ConvertUnits.ToSimUnits(cell.Center));
							for (int k = 0; k < triangles.Count; k++)
							{
								Vector2 a = triangles[k][0];
								Vector2 b = triangles[k][1];
								Vector2 c = triangles[k][2];
								float area = Math.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2f;
								if (area >= 1f)
								{
									Vertices bodyVertices = new Vertices(triangles[k]);
									PolygonShape polygon = new PolygonShape(bodyVertices, 5f);
									Fixture fixture = new Fixture(polygon, Category.Cat8, Category.All)
									{
										UserData = cell
									};
									cellBody.Add(fixture, false);
									if (fixture.Shape.MassData.Area < 1.1920929E-07f)
									{
										string[] array = new string[7];
										array[0] = "Invalid triangle created by CaveGenerator (";
										int num = 1;
										Vector2 vector = triangles[k][0];
										array[num] = vector.ToString();
										array[2] = ", ";
										int num2 = 3;
										vector = triangles[k][1];
										array[num2] = vector.ToString();
										array[4] = ", ";
										int num3 = 5;
										vector = triangles[k][2];
										array[num3] = vector.ToString();
										array[6] = ")";
										DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
										string identifier = "CaveGenerator.GeneratePolygons:InvalidTriangle";
										GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Warning;
										string[] array2 = new string[8];
										array2[0] = "Invalid triangle created by CaveGenerator (";
										int num4 = 1;
										vector = triangles[k][0];
										array2[num4] = vector.ToString();
										array2[2] = ", ";
										int num5 = 3;
										vector = triangles[k][1];
										array2[num5] = vector.ToString();
										array2[4] = ", ";
										int num6 = 5;
										vector = triangles[k][2];
										array2[num6] = vector.ToString();
										array2[6] = "). Seed: ";
										array2[7] = level.Seed;
										GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, string.Concat(array2));
									}
								}
							}
							cell.Body = cellBody;
						}
					}
				}
			}
			cellBody.ResetMassData();
			return cellBody;
		}

		// Token: 0x060026D0 RID: 9936 RVA: 0x000FEE64 File Offset: 0x000FD064
		public static List<Vector2> CreateRandomChunk(float radius, int vertexCount, float radiusVariance)
		{
			return CaveGenerator.CreateRandomChunk(radius * 2f, radius * 2f, vertexCount, radiusVariance);
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x000FEE7C File Offset: 0x000FD07C
		public static List<Vector2> CreateRandomChunk(float width, float height, int vertexCount, float radiusVariance)
		{
			List<Vector2> verts = new List<Vector2>();
			float angleStep = 6.2831855f / (float)vertexCount;
			float angle = 0f;
			for (int i = 0; i < vertexCount; i++)
			{
				Vector2 dir = new Vector2((float)Math.Cos((double)angle), (float)Math.Sin((double)angle));
				verts.Add(new Vector2(dir.X * width / 2f, dir.Y * height / 2f) + dir * Rand.Range(-radiusVariance, radiusVariance, Rand.RandSync.ServerAndClient));
				angle += angleStep;
			}
			return verts;
		}
	}
}
