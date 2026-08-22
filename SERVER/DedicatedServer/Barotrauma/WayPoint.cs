using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000263 RID: 611
	internal class WayPoint : MapEntity
	{
		// Token: 0x17000D33 RID: 3379
		// (get) Token: 0x06002C02 RID: 11266 RVA: 0x001211F8 File Offset: 0x0011F3F8
		public bool IsInWater
		{
			get
			{
				return this.CurrentHull == null || this.CurrentHull.Surface > this.Position.Y;
			}
		}

		// Token: 0x17000D34 RID: 3380
		// (get) Token: 0x06002C03 RID: 11267 RVA: 0x0012121C File Offset: 0x0011F41C
		public bool IsTraversable
		{
			get
			{
				return !this.IsObstructed && (this.openGaps == null || this.openGaps.Count == 0 || this.IsInWater);
			}
		}

		// Token: 0x06002C04 RID: 11268 RVA: 0x00121245 File Offset: 0x0011F445
		public void OnGapStateChanged(bool open, Gap gap)
		{
			if (this.openGaps == null)
			{
				this.openGaps = new HashSet<Gap>();
			}
			if (open)
			{
				this.openGaps.Add(gap);
				return;
			}
			this.openGaps.Remove(gap);
		}

		// Token: 0x17000D35 RID: 3381
		// (get) Token: 0x06002C05 RID: 11269 RVA: 0x00121278 File Offset: 0x0011F478
		// (set) Token: 0x06002C06 RID: 11270 RVA: 0x00121280 File Offset: 0x0011F480
		public Gap ConnectedGap { get; set; }

		// Token: 0x17000D36 RID: 3382
		// (get) Token: 0x06002C07 RID: 11271 RVA: 0x00121289 File Offset: 0x0011F489
		public Door ConnectedDoor
		{
			get
			{
				Gap connectedGap = this.ConnectedGap;
				if (connectedGap == null)
				{
					return null;
				}
				return connectedGap.ConnectedDoor;
			}
		}

		// Token: 0x17000D37 RID: 3383
		// (get) Token: 0x06002C08 RID: 11272 RVA: 0x0012129C File Offset: 0x0011F49C
		// (set) Token: 0x06002C09 RID: 11273 RVA: 0x001212A4 File Offset: 0x0011F4A4
		public Hull CurrentHull { get; private set; }

		// Token: 0x17000D38 RID: 3384
		// (get) Token: 0x06002C0A RID: 11274 RVA: 0x001212AD File Offset: 0x0011F4AD
		// (set) Token: 0x06002C0B RID: 11275 RVA: 0x001212B5 File Offset: 0x0011F4B5
		public SpawnType SpawnType
		{
			get
			{
				return this.spawnType;
			}
			set
			{
				this.spawnType = value;
			}
		}

		// Token: 0x17000D39 RID: 3385
		// (get) Token: 0x06002C0C RID: 11276 RVA: 0x001212BE File Offset: 0x0011F4BE
		// (set) Token: 0x06002C0D RID: 11277 RVA: 0x001212C6 File Offset: 0x0011F4C6
		public Point ExitPointSize { get; private set; }

		// Token: 0x17000D3A RID: 3386
		// (get) Token: 0x06002C0E RID: 11278 RVA: 0x001212D0 File Offset: 0x0011F4D0
		public Rectangle ExitPointWorldRect
		{
			get
			{
				return new Rectangle((int)this.WorldPosition.X - this.ExitPointSize.X / 2, (int)this.WorldPosition.Y + this.ExitPointSize.Y / 2, this.ExitPointSize.X, this.ExitPointSize.Y);
			}
		}

		// Token: 0x17000D3B RID: 3387
		// (get) Token: 0x06002C0F RID: 11279 RVA: 0x0012132C File Offset: 0x0011F52C
		// (set) Token: 0x06002C10 RID: 11280 RVA: 0x00121334 File Offset: 0x0011F534
		public Action<WayPoint> OnLinksChanged { get; set; }

		// Token: 0x17000D3C RID: 3388
		// (get) Token: 0x06002C11 RID: 11281 RVA: 0x0012133D File Offset: 0x0011F53D
		public override string Name
		{
			get
			{
				if (this.spawnType != SpawnType.Path)
				{
					return "SpawnPoint";
				}
				return "WayPoint";
			}
		}

		// Token: 0x17000D3D RID: 3389
		// (get) Token: 0x06002C12 RID: 11282 RVA: 0x00121352 File Offset: 0x0011F552
		// (set) Token: 0x06002C13 RID: 11283 RVA: 0x0012135A File Offset: 0x0011F55A
		public string IdCardDesc { get; private set; }

		// Token: 0x17000D3E RID: 3390
		// (get) Token: 0x06002C14 RID: 11284 RVA: 0x00121363 File Offset: 0x0011F563
		// (set) Token: 0x06002C15 RID: 11285 RVA: 0x0012136C File Offset: 0x0011F56C
		public string[] IdCardTags
		{
			get
			{
				return this.idCardTags;
			}
			private set
			{
				this.idCardTags = value;
				for (int i = 0; i < this.idCardTags.Length; i++)
				{
					this.idCardTags[i] = this.idCardTags[i].Trim().ToLowerInvariant();
				}
			}
		}

		// Token: 0x17000D3F RID: 3391
		// (get) Token: 0x06002C16 RID: 11286 RVA: 0x001213AD File Offset: 0x0011F5AD
		public IEnumerable<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x17000D40 RID: 3392
		// (get) Token: 0x06002C17 RID: 11287 RVA: 0x001213B5 File Offset: 0x0011F5B5
		// (set) Token: 0x06002C18 RID: 11288 RVA: 0x001213BD File Offset: 0x0011F5BD
		public JobPrefab AssignedJob { get; private set; }

		// Token: 0x06002C19 RID: 11289 RVA: 0x001213C6 File Offset: 0x0011F5C6
		public WayPoint(Vector2 position, SpawnType spawnType, Submarine submarine, Gap gap = null) : this(new Rectangle((int)position.X - 3, (int)position.Y + 3, 6, 6), submarine)
		{
			this.spawnType = spawnType;
			this.ConnectedGap = gap;
		}

		// Token: 0x06002C1A RID: 11290 RVA: 0x001213F7 File Offset: 0x0011F5F7
		public WayPoint(MapEntityPrefab prefab, Rectangle rectangle) : this(rectangle, Submarine.MainSub)
		{
			if (prefab.Identifier.Contains("spawn"))
			{
				this.spawnType = SpawnType.Human;
				return;
			}
			this.SpawnType = SpawnType.Path;
		}

		// Token: 0x06002C1B RID: 11291 RVA: 0x00121426 File Offset: 0x0011F626
		public WayPoint(Rectangle newRect, Submarine submarine) : this(WayPoint.Type.WayPoint, newRect, submarine, 0)
		{
		}

		// Token: 0x06002C1C RID: 11292 RVA: 0x00121434 File Offset: 0x0011F634
		public WayPoint(WayPoint.Type type, Rectangle newRect, Submarine submarine, ushort id = 0) : base((type == WayPoint.Type.WayPoint) ? CoreEntityPrefab.WayPointPrefab : CoreEntityPrefab.SpawnPointPrefab, submarine, id)
		{
			this.rect = newRect;
			this.idCardTags = Array.Empty<string>();
			this.tags = new HashSet<Identifier>();
			base.InsertToList();
			WayPoint.WayPointList.Add(this);
			DebugConsole.Log("Created waypoint (" + this.ID.ToString() + ")");
			this.FindHull();
		}

		// Token: 0x06002C1D RID: 11293 RVA: 0x001214AC File Offset: 0x0011F6AC
		public override MapEntity Clone()
		{
			return new WayPoint(this.rect, base.Submarine)
			{
				IdCardDesc = this.IdCardDesc,
				idCardTags = this.idCardTags,
				tags = this.tags,
				spawnType = this.spawnType,
				AssignedJob = this.AssignedJob
			};
		}

		// Token: 0x06002C1E RID: 11294 RVA: 0x00121508 File Offset: 0x0011F708
		public static bool GenerateSubWaypoints(Submarine submarine)
		{
			if (!Hull.HullList.Any<Hull>())
			{
				DebugConsole.ThrowError("Couldn't generate waypoints: no hulls found.", null, null, false, false);
				return false;
			}
			List<WayPoint> existingWaypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Path);
			foreach (WayPoint wayPoint in existingWaypoints)
			{
				wayPoint.Remove();
			}
			List<Door> openDoors = new List<Door>();
			foreach (Item item in Item.ItemList)
			{
				Door door = item.GetComponent<Door>();
				if (door != null && !door.Body.Enabled)
				{
					openDoors.Add(door);
					door.Body.Enabled = true;
				}
			}
			bool isRuin = submarine.Info.ShouldBeRuin;
			float diffFromHullEdge = 50f;
			float minDist = 100f;
			float heightFromFloor = 110f;
			float hullMinHeight = 100f;
			HashSet<WayPoint> removals = new HashSet<WayPoint>();
			foreach (Hull hull in Hull.HullList)
			{
				if (isRuin)
				{
					diffFromHullEdge = 75f;
					List<WayPoint> hullWaypoints = new List<WayPoint>();
					float top = (float)hull.Rect.Y;
					float bottom = (float)(hull.Rect.Y - hull.Rect.Height);
					if (hull.Rect.Width < 300 || hull.Rect.Height < 300)
					{
						if (hull.Rect.Width > hull.Rect.Height)
						{
							float y = (float)(hull.Rect.Y - hull.Rect.Height / 2);
							for (float x = (float)hull.Rect.X + diffFromHullEdge; x <= (float)hull.Rect.Right - diffFromHullEdge; x += minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x, y), SpawnType.Path, submarine, null));
							}
						}
						else
						{
							float x2 = (float)(hull.Rect.X + hull.Rect.Width / 2);
							for (float y2 = top - diffFromHullEdge; y2 >= bottom + diffFromHullEdge; y2 -= minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x2, y2), SpawnType.Path, submarine, null));
							}
						}
					}
					if (hullWaypoints.None(null))
					{
						for (float x3 = (float)hull.Rect.X + diffFromHullEdge; x3 <= (float)hull.Rect.Right - diffFromHullEdge; x3 += minDist)
						{
							for (float y3 = top - diffFromHullEdge; y3 >= bottom + diffFromHullEdge; y3 -= minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x3, y3), SpawnType.Path, submarine, null));
							}
						}
						if (hullWaypoints.None(null))
						{
							hullWaypoints.Add(new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height / 2)), SpawnType.Path, submarine, null));
						}
						foreach (WayPoint wp7 in hullWaypoints)
						{
							foreach (Structure wall in Structure.WallList)
							{
								if (wall.HasBody)
								{
									Rectangle rect = wall.Rect;
									rect.Inflate(10, 10);
									if (rect.ContainsWorld(wp7.Position))
									{
										removals.Add(wp7);
									}
								}
							}
						}
					}
					using (List<WayPoint>.Enumerator enumerator6 = hullWaypoints.GetEnumerator())
					{
						while (enumerator6.MoveNext())
						{
							WayPoint wayPoint2 = enumerator6.Current;
							for (int dir = -1; dir <= 1; dir += 2)
							{
								WayPoint closest = wayPoint2.FindClosest(dir, true, new Vector2(minDist * 1.9f, minDist), null, null, null);
								if (closest != null && closest.CurrentHull == wayPoint2.CurrentHull)
								{
									wayPoint2.ConnectTo(closest);
								}
								closest = wayPoint2.FindClosest(dir, false, new Vector2(minDist, minDist * 1.9f), null, null, null);
								if (closest != null && closest.CurrentHull == wayPoint2.CurrentHull)
								{
									wayPoint2.ConnectTo(closest);
								}
							}
						}
						continue;
					}
				}
				if ((float)hull.Rect.Height >= hullMinHeight)
				{
					Body floor = null;
					for (int i = 0; i < 5; i++)
					{
						float horizontalOffset = 0f;
						switch (i)
						{
						case 1:
							horizontalOffset = (float)hull.RectWidth * 0.2f;
							break;
						case 2:
							horizontalOffset = (float)hull.RectWidth * 0.4f;
							break;
						case 3:
							horizontalOffset = (float)(-(float)hull.RectWidth) * 0.2f;
							break;
						case 4:
							horizontalOffset = (float)(-(float)hull.RectWidth) * 0.4f;
							break;
						}
						horizontalOffset = ConvertUnits.ToSimUnits(horizontalOffset);
						Vector2 floorPos = new Vector2(hull.SimPosition.X + horizontalOffset, ConvertUnits.ToSimUnits(hull.Rect.Y - hull.RectHeight - 50));
						floor = Submarine.PickBody(new Vector2(hull.SimPosition.X + horizontalOffset, hull.SimPosition.Y), floorPos, null, new Category?(Category.Cat1 | Category.Cat3), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
						if (floor != null)
						{
							break;
						}
					}
					if (floor != null)
					{
						float waypointHeight = ((float)hull.Rect.Height > heightFromFloor * 2f) ? heightFromFloor : ((float)(hull.Rect.Height / 2));
						if ((float)hull.Rect.Width < diffFromHullEdge * 3f)
						{
							new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
						}
						else
						{
							WayPoint previousWaypoint = null;
							for (float x4 = (float)hull.Rect.X + diffFromHullEdge; x4 <= (float)hull.Rect.Right - diffFromHullEdge; x4 += minDist)
							{
								WayPoint wayPoint3 = new WayPoint(new Vector2(x4, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
								if (wayPoint3.FindStairs() != null)
								{
									removals.Add(wayPoint3);
								}
								else
								{
									if (previousWaypoint != null)
									{
										wayPoint3.ConnectTo(previousWaypoint);
									}
									previousWaypoint = wayPoint3;
								}
							}
							if (previousWaypoint == null)
							{
								new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
							}
						}
					}
				}
			}
			foreach (Structure platform in Structure.WallList)
			{
				if (platform.IsPlatform)
				{
					float waypointHeight2 = heightFromFloor;
					WayPoint prevWaypoint = null;
					for (float x5 = (float)platform.Rect.X + diffFromHullEdge; x5 <= (float)platform.Rect.Right - diffFromHullEdge; x5 += minDist)
					{
						WayPoint wayPoint4 = new WayPoint(new Vector2(x5, (float)platform.Rect.Y + waypointHeight2), SpawnType.Path, submarine, null);
						if (prevWaypoint != null)
						{
							wayPoint4.ConnectTo(prevWaypoint);
						}
						if (wayPoint4 != null)
						{
							for (int dir2 = -1; dir2 <= 1; dir2 += 2)
							{
								if (wayPoint4.FindClosest(dir2, true, new Vector2(minDist, heightFromFloor), null, prevWaypoint.ToEnumerable<WayPoint>(), null) != null)
								{
									wayPoint4.Remove();
									wayPoint4 = null;
									break;
								}
							}
						}
						prevWaypoint = wayPoint4;
					}
				}
			}
			float outSideWaypointInterval = 100f;
			if (!isRuin && submarine.Info.Type != SubmarineType.OutpostModule)
			{
				List<ValueTuple<WayPoint, int>> outsideWaypoints = new List<ValueTuple<WayPoint, int>>();
				Rectangle borders = Hull.GetBorders();
				int originalWidth = borders.Width;
				int originalHeight = borders.Height;
				borders.X -= Math.Min(500, originalWidth / 4);
				borders.Y += Math.Min(500, originalHeight / 4);
				borders.Width += Math.Min(1500, originalWidth / 2);
				borders.Height += Math.Min(1000, originalHeight / 2);
				borders.Location -= MathUtils.ToPoint(submarine.HiddenSubPosition);
				if ((float)borders.Width <= outSideWaypointInterval * 2f)
				{
					borders.Inflate(outSideWaypointInterval * 2f - (float)borders.Width, 0f);
				}
				if ((float)borders.Height <= outSideWaypointInterval * 2f)
				{
					int inflateAmount = (int)(outSideWaypointInterval * 2f) - borders.Height;
					borders.Y += inflateAmount / 2;
					borders.Height += inflateAmount;
				}
				WayPoint[,] cornerWaypoint = new WayPoint[2, 2];
				for (int j = 0; j < 2; j++)
				{
					for (float x6 = (float)borders.X + outSideWaypointInterval; x6 < (float)borders.Right - outSideWaypointInterval; x6 += outSideWaypointInterval)
					{
						WayPoint wayPoint5 = new WayPoint(new Vector2(x6, (float)(borders.Y - borders.Height * j)) + submarine.HiddenSubPosition, SpawnType.Path, submarine, null);
						outsideWaypoints.Add(new ValueTuple<WayPoint, int>(wayPoint5, j));
						if (x6 == (float)borders.X + outSideWaypointInterval)
						{
							cornerWaypoint[j, 0] = wayPoint5;
						}
						else
						{
							wayPoint5.ConnectTo(WayPoint.WayPointList[WayPoint.WayPointList.Count - 2]);
						}
					}
					cornerWaypoint[j, 1] = WayPoint.WayPointList[WayPoint.WayPointList.Count - 1];
				}
				for (int k = 0; k < 2; k++)
				{
					WayPoint wayPoint6 = null;
					for (float y4 = (float)(borders.Y - borders.Height); y4 < (float)borders.Y; y4 += outSideWaypointInterval)
					{
						wayPoint6 = new WayPoint(new Vector2((float)(borders.X + borders.Width * k), y4) + submarine.HiddenSubPosition, SpawnType.Path, submarine, null);
						outsideWaypoints.Add(new ValueTuple<WayPoint, int>(wayPoint6, k));
						if (y4 == (float)(borders.Y - borders.Height))
						{
							wayPoint6.ConnectTo(cornerWaypoint[1, k]);
						}
						else
						{
							wayPoint6.ConnectTo(WayPoint.WayPointList[WayPoint.WayPointList.Count - 2]);
						}
					}
					wayPoint6.ConnectTo(cornerWaypoint[0, k]);
				}
				Vector2 center = ConvertUnits.ToSimUnits(submarine.HiddenSubPosition);
				float halfHeight = ConvertUnits.ToSimUnits(borders.Height / 2);
				foreach (ValueTuple<WayPoint, int> wayPoint7 in outsideWaypoints)
				{
					WayPoint wp2 = wayPoint7.Item1;
					float xDiff = center.X - wp2.SimPosition.X;
					Vector2 targetPos = new Vector2(center.X - xDiff * 0.5f, center.Y);
					Body wall2 = Submarine.PickBody(wp2.SimPosition, targetPos, null, new Category?(Category.Cat1), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					if (wall2 == null)
					{
						targetPos = new Vector2(center.X - xDiff, center.Y);
						wall2 = Submarine.PickBody(wp2.SimPosition, targetPos, null, new Category?(Category.Cat1), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					}
					if (wall2 != null)
					{
						float distanceFromWall = 1f;
						if (xDiff > 0f && !submarine.Info.HasTag(SubmarineTag.Shuttle))
						{
							float yDist = Math.Abs(center.Y - wp2.SimPosition.Y);
							distanceFromWall = MathHelper.Lerp(1f, 3f, MathUtils.InverseLerp(halfHeight, 0f, yDist));
						}
						Vector2 newPos = Submarine.LastPickedPosition + Submarine.LastPickedNormal * distanceFromWall;
						wp2.rect = new Rectangle(ConvertUnits.ToDisplayUnits(newPos).ToPoint(), wp2.rect.Size);
						wp2.FindHull();
					}
				}
				WayPoint previous = null;
				float tooClose = outSideWaypointInterval / 2f;
				foreach (ValueTuple<WayPoint, int> wayPoint8 in outsideWaypoints)
				{
					WayPoint wp3 = wayPoint8.Item1;
					if (wp3.CurrentHull == null)
					{
						if (Submarine.PickBody(wp3.SimPosition, wp3.SimPosition + Vector2.Normalize(center - wp3.SimPosition) * 0.1f, null, new Category?(Category.Cat1 | Category.Cat5), true, (Fixture f) => !(f.Body.UserData is Submarine), true) == null)
						{
							foreach (ValueTuple<WayPoint, int> otherWayPoint in outsideWaypoints)
							{
								WayPoint otherWp = otherWayPoint.Item1;
								if (otherWp != wp3 && !removals.Contains(otherWp))
								{
									float sqrDist = Vector2.DistanceSquared(wp3.Position, otherWp.Position);
									if (!removals.Contains(previous) && sqrDist < tooClose * tooClose)
									{
										removals.Add(wp3);
									}
								}
							}
							previous = wp3;
							continue;
						}
					}
					removals.Add(wp3);
					previous = wp3;
				}
				using (HashSet<WayPoint>.Enumerator enumerator11 = removals.GetEnumerator())
				{
					while (enumerator11.MoveNext())
					{
						WayPoint wp = enumerator11.Current;
						outsideWaypoints.RemoveAll((ValueTuple<WayPoint, int> w) => w.Item1 == wp);
					}
				}
				removals.ForEach(delegate(WayPoint wp)
				{
					wp.Remove();
				});
				WayPoint.<>c__DisplayClass65_2 CS$<>8__locals3 = new WayPoint.<>c__DisplayClass65_2();
				CS$<>8__locals3.i = 0;
				Func<MapEntity, bool> <>9__9;
				Func<MapEntity, bool> <>9__10;
				while (CS$<>8__locals3.i < outsideWaypoints.Count)
				{
					WayPoint.<>c__DisplayClass65_3 CS$<>8__locals4 = new WayPoint.<>c__DisplayClass65_3();
					CS$<>8__locals4.CS$<>8__locals1 = CS$<>8__locals3;
					CS$<>8__locals4.current = outsideWaypoints[CS$<>8__locals4.CS$<>8__locals1.i].Item1;
					IEnumerable<MapEntity> linkedTo = CS$<>8__locals4.current.linkedTo;
					Func<MapEntity, bool> predicate;
					if ((predicate = <>9__9) == null)
					{
						predicate = (<>9__9 = ((MapEntity l) => !removals.Contains(l)));
					}
					if (linkedTo.Count(predicate) <= 1)
					{
						CS$<>8__locals4.next = null;
						int maxConnections = 2;
						float tooFar = outSideWaypointInterval * 5f;
						int n = 0;
						while (n < maxConnections && CS$<>8__locals4.current.linkedTo.Count < maxConnections)
						{
							float num = tooFar;
							IEnumerable<MapEntity> linkedTo2 = CS$<>8__locals4.current.linkedTo;
							Func<MapEntity, bool> predicate2;
							if ((predicate2 = <>9__10) == null)
							{
								predicate2 = (<>9__10 = ((MapEntity l) => !removals.Contains(l)));
							}
							tooFar = num / (float)linkedTo2.Count(predicate2);
							WayPoint.<>c__DisplayClass65_3 CS$<>8__locals5 = CS$<>8__locals4;
							WayPoint current = CS$<>8__locals4.current;
							IEnumerable<ValueTuple<WayPoint, int>> waypointList = outsideWaypoints;
							float tolerance2 = tooFar;
							Body ignoredBody = null;
							IEnumerable<WayPoint> ignored = null;
							Func<ValueTuple<WayPoint, int>, bool> filter;
							if ((filter = CS$<>8__locals4.<>9__11) == null)
							{
								filter = (CS$<>8__locals4.<>9__11 = delegate(ValueTuple<WayPoint, int> wp)
								{
									if (wp.Item1 != CS$<>8__locals4.next)
									{
										IEnumerable<MapEntity> linkedTo3 = wp.Item1.linkedTo;
										Func<MapEntity, bool> predicate3;
										if ((predicate3 = CS$<>8__locals4.<>9__12) == null)
										{
											predicate3 = (CS$<>8__locals4.<>9__12 = ((MapEntity e) => CS$<>8__locals4.current.linkedTo.Contains(e)));
										}
										if (linkedTo3.None(predicate3) && wp.Item1.linkedTo.Count < 2)
										{
											return wp.Item2 < CS$<>8__locals4.CS$<>8__locals1.i;
										}
									}
									return false;
								});
							}
							CS$<>8__locals5.next = current.FindClosestOutside(waypointList, tolerance2, ignoredBody, ignored, filter);
							if (CS$<>8__locals4.next != null)
							{
								CS$<>8__locals4.current.ConnectTo(CS$<>8__locals4.next);
							}
							n++;
						}
					}
					int i2 = CS$<>8__locals3.i;
					CS$<>8__locals3.i = i2 + 1;
				}
			}
			removals.ForEach(delegate(WayPoint wp)
			{
				wp.Remove();
			});
			removals.Clear();
			foreach (MapEntity mapEntity in MapEntity.MapEntityList.ToList<MapEntity>())
			{
				Structure structure = mapEntity as Structure;
				if (structure != null && structure.StairDirection != Direction.None)
				{
					WayPoint[] stairPoints = new WayPoint[3];
					float margin = -32f;
					stairPoints[0] = new WayPoint(new Vector2((float)(structure.Rect.X + 5), (float)structure.Rect.Y - ((structure.StairDirection == Direction.Left) ? margin : ((float)(structure.Rect.Height - 100)))), SpawnType.Path, submarine, null);
					stairPoints[1] = new WayPoint(new Vector2((float)(structure.Rect.Right - 5), (float)structure.Rect.Y - ((structure.StairDirection == Direction.Left) ? ((float)(structure.Rect.Height - 100)) : margin)), SpawnType.Path, submarine, null);
					for (int m = 0; m < 2; m++)
					{
						for (int dir3 = -1; dir3 <= 1; dir3 += 2)
						{
							WayPoint closest2 = stairPoints[m].FindClosest(dir3, true, new Vector2(minDist * 1.5f, minDist / 2f), null, null, (WayPoint wp) => wp.Stairs == null) ?? stairPoints[m].FindClosest(dir3, true, new Vector2(minDist * 1.5f, minDist / 2f), null, null, null);
							if (closest2 != null)
							{
								stairPoints[m].ConnectTo(closest2);
							}
						}
					}
					stairPoints[2] = new WayPoint((stairPoints[0].Position + stairPoints[1].Position) / 2f, SpawnType.Path, submarine, null);
					stairPoints[0].ConnectTo(stairPoints[2]);
					stairPoints[2].ConnectTo(stairPoints[1]);
					stairPoints.ForEach(delegate(WayPoint wp)
					{
						wp.FindStairs();
					});
				}
			}
			foreach (Item item2 in Item.ItemList)
			{
				Ladder ladders2 = item2.GetComponent<Ladder>();
				if (ladders2 != null)
				{
					Vector2 bottomPoint = new Vector2((float)item2.Rect.Center.X, (float)(item2.Rect.Top - item2.Rect.Height + 10));
					List<ValueTuple<WayPoint, bool>> ladderPoints = new List<ValueTuple<WayPoint, bool>>
					{
						new ValueTuple<WayPoint, bool>(new WayPoint(bottomPoint, SpawnType.Path, submarine, null), true)
					};
					List<Body> ignoredBodies = new List<Body>();
					WayPoint lowestPoint = ladderPoints[0].Item1;
					WayPoint prevPoint = lowestPoint;
					Vector2 prevPos = prevPoint.SimPosition;
					Body ground = Submarine.PickBody(lowestPoint.SimPosition, lowestPoint.SimPosition - Vector2.UnitY, ignoredBodies, new Category?(Category.Cat1 | Category.Cat3 | Category.Cat4), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					float startHeight = (ground != null) ? ConvertUnits.ToDisplayUnits(ground.Position.Y) : bottomPoint.Y;
					startHeight += heightFromFloor;
					WayPoint startPoint = lowestPoint;
					Vector2 nextPos = new Vector2((float)item2.Rect.Center.X, startHeight);
					if (lowestPoint == null || (Math.Abs(startPoint.Position.Y - startHeight) > 40f && Hull.FindHull(nextPos, null, true, true) != null))
					{
						startPoint = new WayPoint(nextPos, SpawnType.Path, submarine, null);
						ladderPoints.Add(new ValueTuple<WayPoint, bool>(startPoint, true));
						if (lowestPoint != null)
						{
							startPoint.ConnectTo(lowestPoint);
						}
						prevPoint = startPoint;
						prevPos = prevPoint.SimPosition;
					}
					for (float y5 = startPoint.Position.Y + 75f; y5 < (float)item2.Rect.Y - 1f; y5 += 75f)
					{
						Body pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(new Vector2(startPoint.Position.X, y5)), prevPos, ignoredBodies, new Category?(Category.Cat1), false, delegate(Fixture f)
						{
							Item pickedItem = f.Body.UserData as Item;
							return pickedItem != null && pickedItem.GetComponent<Door>() != null;
						}, false);
						Door pickedDoor = null;
						if (pickedBody != null)
						{
							pickedDoor = (((pickedBody != null) ? pickedBody.UserData : null) as Item).GetComponent<Door>();
						}
						else
						{
							pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(new Vector2(startPoint.Position.X, y5)), prevPos, ignoredBodies, null, false, (Fixture f) => f.Body.UserData is Structure, false);
						}
						if (pickedBody != null)
						{
							ignoredBodies.Add(pickedBody);
						}
						if (pickedDoor != null)
						{
							WayPoint newPoint = new WayPoint(pickedDoor.Item.Position, SpawnType.Path, submarine, null);
							ladderPoints.Add(new ValueTuple<WayPoint, bool>(newPoint, true));
							newPoint.ConnectedGap = pickedDoor.LinkedGap;
							newPoint.ConnectTo(prevPoint);
							prevPoint = newPoint;
							prevPos = new Vector2(prevPos.X, ConvertUnits.ToSimUnits(pickedDoor.Item.Position.Y - (float)pickedDoor.Item.Rect.Height));
							y5 = Math.Max(pickedDoor.Item.Position.Y, y5);
						}
						else
						{
							Vector2 pos = (pickedBody == null) ? new Vector2(startPoint.Position.X, y5) : (ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition) + Vector2.UnitY * heightFromFloor);
							WayPoint newPoint2 = new WayPoint(pos, SpawnType.Path, submarine, null);
							ladderPoints.Add(new ValueTuple<WayPoint, bool>(newPoint2, pickedBody != null));
							newPoint2.ConnectTo(prevPoint);
							prevPoint = newPoint2;
							prevPos = ConvertUnits.ToSimUnits(newPoint2.Position);
							if (pickedBody != null)
							{
								y5 = Math.Max(newPoint2.Position.Y, y5);
							}
						}
					}
					if (prevPoint.rect.Y < item2.Rect.Y - 40)
					{
						WayPoint wayPoint9 = new WayPoint(new Vector2((float)item2.Rect.Center.X, (float)item2.Rect.Y - 1f), SpawnType.Path, submarine, null);
						ladderPoints.Add(new ValueTuple<WayPoint, bool>(wayPoint9, true));
						wayPoint9.ConnectTo(prevPoint);
					}
					IEnumerable<WayPoint> ladderWaypoints = from lp in ladderPoints
					select lp.Item1;
					foreach (ValueTuple<WayPoint, bool> ladderPoint in ladderPoints)
					{
						WayPoint wp4 = ladderPoint.Item1;
						wp4.Ladders = ladders2;
						if (ladderPoint.Item2)
						{
							bool isHatch = wp4.ConnectedGap != null && !wp4.ConnectedGap.IsRoomToRoom;
							for (int dir4 = -1; dir4 <= 1; dir4 += 2)
							{
								WayPoint wayPoint13;
								if (!isHatch)
								{
									WayPoint wayPoint12 = wp4;
									int dir7 = dir4;
									bool horizontalSearch = true;
									Vector2 tolerance3 = new Vector2(150f, 100f);
									Gap connectedGap = wp4.ConnectedGap;
									Body ignoredBody2;
									if (connectedGap == null)
									{
										ignoredBody2 = null;
									}
									else
									{
										Door connectedDoor = connectedGap.ConnectedDoor;
										ignoredBody2 = ((connectedDoor != null) ? connectedDoor.Body.FarseerBody : null);
									}
									wayPoint13 = wayPoint12.FindClosest(dir7, horizontalSearch, tolerance3, ignoredBody2, ladderWaypoints, null);
								}
								else
								{
									WayPoint wayPoint14 = wp4;
									int dir8 = dir4;
									bool horizontalSearch2 = true;
									Vector2 tolerance4 = new Vector2(500f, 1000f);
									Gap connectedGap2 = wp4.ConnectedGap;
									Body ignoredBody3;
									if (connectedGap2 == null)
									{
										ignoredBody3 = null;
									}
									else
									{
										Door connectedDoor2 = connectedGap2.ConnectedDoor;
										ignoredBody3 = ((connectedDoor2 != null) ? connectedDoor2.Body.FarseerBody : null);
									}
									wayPoint13 = wayPoint14.FindClosest(dir8, horizontalSearch2, tolerance4, ignoredBody3, ladderWaypoints, (WayPoint wp) => wp.CurrentHull == null);
								}
								WayPoint closest3 = wayPoint13;
								if (closest3 != null)
								{
									wp4.ConnectTo(closest3);
								}
							}
						}
					}
				}
			}
			foreach (Item item3 in Item.ItemList)
			{
				Ladder ladders = item3.GetComponent<Ladder>();
				if (ladders != null)
				{
					IOrderedEnumerable<WayPoint> wps = from wp in WayPoint.WayPointList
					where wp.Ladders == ladders
					orderby wp.Rect.Y descending
					select wp;
					WayPoint cap = wps.First<WayPoint>();
					WayPoint above = cap.FindClosest(1, false, new Vector2(25f, 50f), null, null, (WayPoint wp) => wp.Ladders != null && wp.Ladders != ladders);
					if (above != null)
					{
						above.ConnectTo(cap);
					}
					WayPoint bottom2 = wps.Last<WayPoint>();
					WayPoint below = bottom2.FindClosest(-1, false, new Vector2(25f, 50f), null, null, (WayPoint wp) => wp.Ladders != null && wp.Ladders != ladders);
					if (below != null)
					{
						below.ConnectTo(bottom2);
					}
				}
			}
			using (List<Gap>.Enumerator enumerator16 = Gap.GapList.GetEnumerator())
			{
				while (enumerator16.MoveNext())
				{
					Gap gap = enumerator16.Current;
					if (gap.IsHorizontal)
					{
						if (isRuin)
						{
							if (gap.Rect.Height < 50)
							{
								continue;
							}
						}
						else if ((float)gap.Rect.Height < hullMinHeight)
						{
							continue;
						}
						Vector2 pos2 = new Vector2((float)gap.Rect.Center.X, (float)(gap.Rect.Y - gap.Rect.Height) + heightFromFloor);
						if (isRuin)
						{
							pos2.Y = (float)(gap.Rect.Y - gap.Rect.Height / 2);
						}
						WayPoint wayPoint10 = new WayPoint(pos2, SpawnType.Path, submarine, gap);
						Vector2 tolerance = (gap.IsRoomToRoom && !isRuin) ? new Vector2(150f, 70f) : new Vector2(1000f, 1000f);
						for (int dir5 = -1; dir5 <= 1; dir5 += 2)
						{
							WayPoint wayPoint15 = wayPoint10;
							int dir9 = dir5;
							bool horizontalSearch3 = true;
							Vector2 tolerance5 = tolerance;
							Door connectedDoor3 = gap.ConnectedDoor;
							WayPoint closest4 = wayPoint15.FindClosest(dir9, horizontalSearch3, tolerance5, (connectedDoor3 != null) ? connectedDoor3.Body.FarseerBody : null, null, null);
							if (closest4 != null)
							{
								wayPoint10.ConnectTo(closest4);
							}
						}
					}
					else
					{
						if (!isRuin)
						{
							if (gap.IsRoomToRoom)
							{
								continue;
							}
							if (gap.linkedTo.None((MapEntity l) => l is Hull))
							{
								continue;
							}
						}
						if ((float)gap.Rect.Width >= 50f)
						{
							Vector2 pos3 = new Vector2((float)gap.Rect.Center.X, (float)(gap.Rect.Y - gap.Rect.Height / 2));
							if (!WayPoint.WayPointList.Any((WayPoint wp) => wp.ConnectedGap == gap))
							{
								WayPoint wayPoint11 = new WayPoint(pos3, SpawnType.Path, submarine, gap);
								Hull connectedHull = (Hull)gap.linkedTo.First((MapEntity l) => l is Hull);
								int dir6 = Math.Sign(connectedHull.Position.Y - gap.Position.Y);
								WayPoint closest5 = wayPoint11.FindClosest(dir6, false, isRuin ? new Vector2(500f, 500f) : new Vector2(50f, 100f), null, null, null);
								if (closest5 != null)
								{
									wayPoint11.ConnectTo(closest5);
								}
								if (isRuin)
								{
									closest5 = wayPoint11.FindClosest(-dir6, false, isRuin ? new Vector2(500f, 500f) : new Vector2(50f, 100f), null, null, null);
									if (closest5 != null)
									{
										wayPoint11.ConnectTo(closest5);
									}
								}
								for (dir6 = -1; dir6 <= 1; dir6 += 2)
								{
									WayPoint wayPoint16 = wayPoint11;
									int dir10 = dir6;
									bool horizontalSearch4 = true;
									Vector2 tolerance6 = new Vector2(500f, 1000f);
									Door connectedDoor4 = gap.ConnectedDoor;
									closest5 = wayPoint16.FindClosest(dir10, horizontalSearch4, tolerance6, (connectedDoor4 != null) ? connectedDoor4.Body.FarseerBody : null, null, (WayPoint wp) => wp.CurrentHull == null);
									if (closest5 != null)
									{
										wayPoint11.ConnectTo(closest5);
									}
								}
							}
						}
					}
				}
			}
			List<WayPoint> orphans = WayPoint.WayPointList.FindAll((WayPoint w) => w.spawnType == SpawnType.Path && w.linkedTo.None(null));
			foreach (WayPoint wp5 in orphans)
			{
				wp5.Remove();
			}
			foreach (WayPoint wp6 in WayPoint.WayPointList)
			{
				if (wp6.SpawnType == SpawnType.Path && wp6.CurrentHull == null && wp6.Ladders == null && wp6.linkedTo.Count < 2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(133, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't automatically link the waypoint ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(wp6.ID);
					defaultInterpolatedStringHandler.AppendLiteral(" outside of the submarine. You should do it manually. The waypoint ID is shown in red color.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			foreach (Door door2 in openDoors)
			{
				door2.Body.Enabled = false;
			}
			return true;
		}

		// Token: 0x06002C1F RID: 11295 RVA: 0x00123518 File Offset: 0x00121718
		private WayPoint FindClosestOutside(IEnumerable<ValueTuple<WayPoint, int>> waypointList, float tolerance, Body ignoredBody = null, IEnumerable<WayPoint> ignored = null, Func<ValueTuple<WayPoint, int>, bool> filter = null)
		{
			float closestDist = 0f;
			WayPoint closest = null;
			foreach (ValueTuple<WayPoint, int> wayPoint in waypointList)
			{
				WayPoint wp = wayPoint.Item1;
				if (wp.SpawnType == SpawnType.Path && wp != this && !this.linkedTo.Contains(wp) && (ignored == null || !ignored.Contains(wp)) && (filter == null || filter(wayPoint)))
				{
					float sqrDist = Vector2.DistanceSquared(this.Position, wp.Position);
					if (sqrDist <= tolerance * tolerance && (closest == null || sqrDist < closestDist))
					{
						Body body = Submarine.CheckVisibility(this.SimPosition, wp.SimPosition, true, true, false, true, true, null);
						if (body == null || body == ignoredBody || body.UserData is Submarine || (!(body.UserData is Structure) && !body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1)))
						{
							closestDist = sqrDist;
							closest = wp;
						}
					}
				}
			}
			return closest;
		}

		// Token: 0x06002C20 RID: 11296 RVA: 0x00123648 File Offset: 0x00121848
		private WayPoint FindClosest(int dir, bool horizontalSearch, Vector2 tolerance, Body ignoredBody = null, IEnumerable<WayPoint> ignored = null, Func<WayPoint, bool> filter = null)
		{
			if (dir != -1 && dir != 1)
			{
				return null;
			}
			float closestDist = 0f;
			WayPoint closest = null;
			foreach (WayPoint wp in WayPoint.WayPointList)
			{
				if (wp.SpawnType == SpawnType.Path && wp != this)
				{
					float xDiff = wp.Position.X - this.Position.X;
					float yDiff = wp.Position.Y - this.Position.Y;
					float xDist = Math.Abs(xDiff);
					float yDist = Math.Abs(yDiff);
					if (tolerance.X >= xDist && tolerance.Y >= yDist)
					{
						float diff;
						float dist;
						if (horizontalSearch)
						{
							diff = xDiff;
							dist = xDist + yDist / 5f;
						}
						else
						{
							diff = yDiff;
							dist = yDist + xDist / 5f;
							if (wp.Ladders != null)
							{
								dist *= 0.5f;
							}
						}
						if (Math.Sign(diff) == dir && !this.linkedTo.Contains(wp) && (ignored == null || !ignored.Contains(wp)) && (filter == null || filter(wp)) && (closest == null || dist < closestDist))
						{
							Body body = Submarine.CheckVisibility(this.SimPosition, wp.SimPosition, true, true, false, true, true, null);
							if (body != null && body != ignoredBody && !(body.UserData is Submarine))
							{
								if (body.UserData is Structure)
								{
									continue;
								}
								if (body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1))
								{
									Item i = body.UserData as Item;
									if (i != null && i.GetComponent<Door>() != null)
									{
										continue;
									}
								}
							}
							closestDist = dist;
							closest = wp;
						}
					}
				}
			}
			return closest;
		}

		// Token: 0x06002C21 RID: 11297 RVA: 0x00123840 File Offset: 0x00121A40
		public void ConnectTo(WayPoint wayPoint2)
		{
			if (!this.linkedTo.Contains(wayPoint2))
			{
				this.linkedTo.Add(wayPoint2);
				Action<WayPoint> onLinksChanged = this.OnLinksChanged;
				if (onLinksChanged != null)
				{
					onLinksChanged(this);
				}
			}
			if (!wayPoint2.linkedTo.Contains(this))
			{
				wayPoint2.linkedTo.Add(this);
				Action<WayPoint> onLinksChanged2 = wayPoint2.OnLinksChanged;
				if (onLinksChanged2 == null)
				{
					return;
				}
				onLinksChanged2(wayPoint2);
			}
		}

		// Token: 0x06002C22 RID: 11298 RVA: 0x001238A4 File Offset: 0x00121AA4
		public static WayPoint GetRandom(SpawnType spawnType = SpawnType.Human, JobPrefab assignedJob = null, Submarine sub = null, bool useSyncedRand = false, string spawnPointTag = null, bool ignoreSubmarine = false)
		{
			Func<Identifier, bool> <>9__1;
			return WayPoint.WayPointList.GetRandom(delegate(WayPoint wp)
			{
				if ((ignoreSubmarine || wp.Submarine == sub) && !wp.spawnType.HasFlag(SpawnType.Disabled) && wp.spawnType == spawnType)
				{
					if (!spawnPointTag.IsNullOrEmpty())
					{
						IEnumerable<Identifier> source = wp.Tags;
						Func<Identifier, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Identifier t) => t == spawnPointTag));
						}
						if (!source.Any(predicate))
						{
							return false;
						}
					}
					return assignedJob == null || (assignedJob != null && wp.AssignedJob == assignedJob);
				}
				return false;
			}, useSyncedRand ? Rand.RandSync.ServerAndClient : Rand.RandSync.Unsynced);
		}

		// Token: 0x06002C23 RID: 11299 RVA: 0x001238FC File Offset: 0x00121AFC
		public static WayPoint[] SelectCrewSpawnPoints(List<CharacterInfo> crew, Submarine submarine)
		{
			List<WayPoint> subWayPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Submarine == submarine);
			if (submarine.ForcedOutpostModuleWayPoints != null && submarine.ForcedOutpostModuleWayPoints.Any<WayPoint>())
			{
				subWayPoints = new List<WayPoint>(submarine.ForcedOutpostModuleWayPoints);
				submarine.ForcedOutpostModuleWayPoints.Clear();
			}
			subWayPoints.Shuffle(Rand.RandSync.Unsynced);
			List<WayPoint> unassignedWayPoints = subWayPoints.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Human);
			WayPoint[] assignedWayPoints = new WayPoint[crew.Count];
			for (int i = 0; i < crew.Count; i++)
			{
				for (int j = 0; j < unassignedWayPoints.Count; j++)
				{
					if (crew[i].Job.Prefab == unassignedWayPoints[j].AssignedJob)
					{
						assignedWayPoints[i] = unassignedWayPoints[j];
						unassignedWayPoints.RemoveAt(j);
						break;
					}
				}
			}
			for (int k = 0; k < crew.Count; k++)
			{
				if (assignedWayPoints[k] == null)
				{
					foreach (WayPoint wp2 in subWayPoints)
					{
						if (wp2.spawnType == SpawnType.Human && wp2.AssignedJob == crew[k].Job.Prefab)
						{
							assignedWayPoints[k] = wp2;
							break;
						}
					}
					if (assignedWayPoints[k] == null)
					{
						List<WayPoint> nonJobSpecificPoints = subWayPoints.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Human && wp.AssignedJob == null);
						if (nonJobSpecificPoints.Any<WayPoint>())
						{
							assignedWayPoints[k] = nonJobSpecificPoints[Rand.Int(nonJobSpecificPoints.Count, Rand.RandSync.ServerAndClient)];
						}
						if (assignedWayPoints[k] == null)
						{
							assignedWayPoints[k] = WayPoint.GetRandom(SpawnType.Human, null, submarine, true, null, false);
						}
					}
				}
			}
			for (int l = 0; l < assignedWayPoints.Length; l++)
			{
				if (assignedWayPoints[l] == null)
				{
					DebugConsole.AddWarning("Couldn't find a waypoint for " + crew[l].Name + "!", null);
					assignedWayPoints[l] = WayPoint.WayPointList[0];
				}
			}
			return assignedWayPoints;
		}

		// Token: 0x06002C24 RID: 11300 RVA: 0x00123B48 File Offset: 0x00121D48
		public static WayPoint[] SelectOutpostSpawnPoints(List<CharacterInfo> crew, CharacterTeamType teamID)
		{
			List<WayPoint> potentialSpawnPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Human && wp.Submarine == Level.Loaded.StartOutpost);
			if (GameMain.GameSession.GameMode is PvPMode)
			{
				Identifier teamSpawnTag = ("deathmatch" + teamID.ToString()).ToIdentifier();
				if (potentialSpawnPoints.Any((WayPoint wp) => wp.Tags.Contains(teamSpawnTag)))
				{
					potentialSpawnPoints = potentialSpawnPoints.FindAll((WayPoint wp) => wp.Tags.Contains(teamSpawnTag));
				}
			}
			else
			{
				potentialSpawnPoints = potentialSpawnPoints.FindAll(delegate(WayPoint wp)
				{
					Hull currentHull = wp.CurrentHull;
					return ((currentHull != null) ? currentHull.OutpostModuleTags : null) != null && wp.CurrentHull.OutpostModuleTags.Contains(Barotrauma.Tags.Airlock);
				});
			}
			if (potentialSpawnPoints.None(null))
			{
				return potentialSpawnPoints.ToArray();
			}
			List<WayPoint> spawnPoints = new List<WayPoint>();
			int i;
			Func<WayPoint, bool> <>9__4;
			int j;
			for (i = 0; i < crew.Count; i = j + 1)
			{
				IEnumerable<WayPoint> source = potentialSpawnPoints;
				Func<WayPoint, bool> predicate;
				if ((predicate = <>9__4) == null)
				{
					predicate = (<>9__4 = ((WayPoint wp) => wp.AssignedJob == crew[i].Job.Prefab));
				}
				IEnumerable<WayPoint> spawnPointsForJob = source.Where(predicate);
				IEnumerable<WayPoint> spawnPointsForAnyJob = from wp in potentialSpawnPoints
				where wp.AssignedJob == null
				select wp;
				if (spawnPointsForJob.Any<WayPoint>())
				{
					spawnPoints.Add(spawnPointsForJob.GetRandomUnsynced<WayPoint>());
				}
				else if (spawnPointsForAnyJob.Any<WayPoint>())
				{
					spawnPoints.Add(spawnPointsForAnyJob.GetRandomUnsynced<WayPoint>());
				}
				else
				{
					spawnPoints.Add(potentialSpawnPoints.GetRandomUnsynced<WayPoint>());
				}
				j = i;
			}
			return spawnPoints.ToArray();
		}

		// Token: 0x06002C25 RID: 11301 RVA: 0x00123CF0 File Offset: 0x00121EF0
		public void FindHull()
		{
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
		}

		// Token: 0x06002C26 RID: 11302 RVA: 0x00123D0B File Offset: 0x00121F0B
		public override void OnMapLoaded()
		{
			if (base.Submarine == null)
			{
				return;
			}
			this.InitializeLinks();
			this.FindHull();
			this.FindStairs();
		}

		// Token: 0x06002C27 RID: 11303 RVA: 0x00123D2C File Offset: 0x00121F2C
		private Structure FindStairs()
		{
			this.Stairs = null;
			Body pickedBody = Submarine.PickBody(this.SimPosition, this.SimPosition - new Vector2(0f, 1.2f), null, new Category?(Category.Cat4), true, null, false);
			if (pickedBody != null)
			{
				Structure structure = pickedBody.UserData as Structure;
				if (structure != null && structure.StairDirection != Direction.None)
				{
					this.Stairs = structure;
				}
			}
			return this.Stairs;
		}

		// Token: 0x06002C28 RID: 11304 RVA: 0x00123D98 File Offset: 0x00121F98
		public void InitializeLinks()
		{
			if (this.gapId > 0)
			{
				this.ConnectedGap = (Entity.FindEntityByID(this.gapId) as Gap);
				this.gapId = 0;
			}
			if (this.ladderId > 0)
			{
				Item ladderItem = Entity.FindEntityByID(this.ladderId) as Item;
				if (ladderItem != null)
				{
					this.Ladders = ladderItem.GetComponent<Ladder>();
				}
				this.ladderId = 0;
			}
		}

		// Token: 0x06002C29 RID: 11305 RVA: 0x00123DFC File Offset: 0x00121FFC
		public static WayPoint Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			Rectangle rect = new Rectangle(int.Parse(element.GetAttribute("x").Value), int.Parse(element.GetAttribute("y").Value), (int)Submarine.GridSize.X, (int)Submarine.GridSize.Y);
			SpawnType spawnType;
			Enum.TryParse<SpawnType>(element.GetAttributeString("spawn", "Path"), out spawnType);
			WayPoint w = new WayPoint((spawnType == SpawnType.Path) ? WayPoint.Type.WayPoint : WayPoint.Type.SpawnPoint, rect, submarine, idRemap.GetOffsetId(element))
			{
				spawnType = spawnType,
				Layer = element.GetAttributeString("Layer", null)
			};
			string idCardDescString = element.GetAttributeString("idcarddesc", "");
			if (!string.IsNullOrWhiteSpace(idCardDescString))
			{
				w.IdCardDesc = idCardDescString;
			}
			string idCardTagString = element.GetAttributeString("idcardtags", "");
			if (!string.IsNullOrWhiteSpace(idCardTagString))
			{
				w.IdCardTags = idCardTagString.Split(',', StringSplitOptions.None);
			}
			WayPoint wayPoint = w;
			string key = "exitpointsize";
			Point zero = Point.Zero;
			wayPoint.ExitPointSize = element.GetAttributePoint(key, zero);
			w.tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			Identifier jobIdentifier = element.GetAttributeIdentifier("job", Identifier.Empty);
			if (!jobIdentifier.IsEmpty)
			{
				w.AssignedJob = JobPrefab.Get(jobIdentifier);
			}
			w.linkedToID = new List<ushort>();
			w.ladderId = idRemap.GetOffsetId(element.GetAttributeInt("ladders", 0));
			w.gapId = idRemap.GetOffsetId(element.GetAttributeInt("gap", 0));
			int i = 0;
			while (element.GetAttribute("linkedto" + i.ToString()) != null)
			{
				int srcId = int.Parse(element.GetAttribute("linkedto" + i.ToString()).Value);
				int destId = (int)idRemap.GetOffsetId(srcId);
				if (destId > 0)
				{
					w.linkedToID.Add((ushort)destId);
				}
				else
				{
					WayPoint wayPoint2 = w;
					if (wayPoint2.unresolvedLinkedToID == null)
					{
						wayPoint2.unresolvedLinkedToID = new List<ushort>();
					}
					w.unresolvedLinkedToID.Add((ushort)srcId);
				}
				i++;
			}
			return w;
		}

		// Token: 0x06002C2A RID: 11306 RVA: 0x00124010 File Offset: 0x00122210
		public override XElement Save(XElement parentElement)
		{
			if (!this.ShouldBeSaved)
			{
				return null;
			}
			XElement element = new XElement("WayPoint");
			element.Add(new object[]
			{
				new XAttribute("ID", this.ID),
				new XAttribute("x", (int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)),
				new XAttribute("y", (int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)),
				new XAttribute("spawn", this.spawnType),
				new XAttribute("Layer", base.Layer ?? string.Empty)
			});
			if (this.SpawnType == SpawnType.ExitPoint)
			{
				element.Add(new XAttribute("exitpointsize", XMLExtensions.PointToString(this.ExitPointSize)));
			}
			if (!string.IsNullOrWhiteSpace(this.IdCardDesc))
			{
				element.Add(new XAttribute("idcarddesc", this.IdCardDesc));
			}
			if (this.idCardTags.Length != 0)
			{
				element.Add(new XAttribute("idcardtags", string.Join(",", this.idCardTags)));
			}
			if (this.tags.Count > 0)
			{
				element.Add(new XAttribute("tags", string.Join<Identifier>(",", this.tags)));
			}
			if (this.AssignedJob != null)
			{
				element.Add(new XAttribute("job", this.AssignedJob.Identifier));
			}
			if (this.ConnectedGap != null)
			{
				element.Add(new XAttribute("gap", this.ConnectedGap.ID));
			}
			if (this.Ladders != null)
			{
				element.Add(new XAttribute("ladders", this.Ladders.Item.ID));
			}
			parentElement.Add(element);
			if (this.linkedTo != null)
			{
				int i = 0;
				foreach (MapEntity e in this.linkedTo)
				{
					if (e.ShouldBeSaved && e.Removed == base.Removed)
					{
						Submarine submarine = e.Submarine;
						SubmarineType? submarineType = (submarine != null) ? new SubmarineType?(submarine.Info.Type) : null;
						Submarine submarine2 = base.Submarine;
						SubmarineType? submarineType2 = (submarine2 != null) ? new SubmarineType?(submarine2.Info.Type) : null;
						if (submarineType.GetValueOrDefault() == submarineType2.GetValueOrDefault() & submarineType != null == (submarineType2 != null))
						{
							element.Add(new XAttribute("linkedto" + i.ToString(), e.ID));
							i++;
						}
					}
				}
			}
			return element;
		}

		// Token: 0x06002C2B RID: 11307 RVA: 0x00124360 File Offset: 0x00122560
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			WayPoint.WayPointList.Remove(this);
		}

		// Token: 0x06002C2C RID: 11308 RVA: 0x00124374 File Offset: 0x00122574
		public override void Remove()
		{
			base.Remove();
			this.CurrentHull = null;
			this.ConnectedGap = null;
			this.Tunnel = null;
			this.Ruin = null;
			this.Stairs = null;
			this.Ladders = null;
			this.OnLinksChanged = null;
			WayPoint.WayPointList.Remove(this);
		}

		// Token: 0x040015B8 RID: 5560
		public static List<WayPoint> WayPointList = new List<WayPoint>();

		// Token: 0x040015B9 RID: 5561
		public static bool ShowWayPoints = true;

		// Token: 0x040015BA RID: 5562
		public static bool ShowSpawnPoints = true;

		// Token: 0x040015BB RID: 5563
		public const float LadderWaypointInterval = 75f;

		// Token: 0x040015BC RID: 5564
		protected SpawnType spawnType;

		// Token: 0x040015BD RID: 5565
		private string[] idCardTags;

		// Token: 0x040015BE RID: 5566
		private ushort ladderId;

		// Token: 0x040015BF RID: 5567
		public Ladder Ladders;

		// Token: 0x040015C0 RID: 5568
		public Structure Stairs;

		// Token: 0x040015C1 RID: 5569
		private HashSet<Identifier> tags;

		// Token: 0x040015C2 RID: 5570
		public bool IsObstructed;

		// Token: 0x040015C3 RID: 5571
		private HashSet<Gap> openGaps;

		// Token: 0x040015C4 RID: 5572
		private ushort gapId;

		// Token: 0x040015C7 RID: 5575
		public Level.Tunnel Tunnel;

		// Token: 0x040015C8 RID: 5576
		public Ruin Ruin;

		// Token: 0x040015C9 RID: 5577
		public Level.Cave Cave;

		// Token: 0x02000ACB RID: 2763
		public enum Type
		{
			// Token: 0x04003733 RID: 14131
			WayPoint,
			// Token: 0x04003734 RID: 14132
			SpawnPoint
		}
	}
}
