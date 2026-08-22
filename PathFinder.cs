using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000193 RID: 403
	internal class PathFinder
	{
		// Token: 0x17000C2B RID: 3115
		// (get) Token: 0x06002F4E RID: 12110 RVA: 0x001F4E66 File Offset: 0x001F3066
		// (set) Token: 0x06002F4F RID: 12111 RVA: 0x001F4E6E File Offset: 0x001F306E
		public bool InsideSubmarine { get; set; }

		// Token: 0x17000C2C RID: 3116
		// (get) Token: 0x06002F50 RID: 12112 RVA: 0x001F4E77 File Offset: 0x001F3077
		// (set) Token: 0x06002F51 RID: 12113 RVA: 0x001F4E7F File Offset: 0x001F307F
		public bool ApplyPenaltyToOutsideNodes { get; set; }

		// Token: 0x06002F52 RID: 12114 RVA: 0x001F4E88 File Offset: 0x001F3088
		public PathFinder(List<WayPoint> wayPoints, bool isCharacter)
		{
			List<WayPoint> list;
			if (!isCharacter)
			{
				list = wayPoints.FindAll((WayPoint w) => w.Submarine == null);
			}
			else
			{
				list = wayPoints;
			}
			List<WayPoint> filtered = list;
			this.nodes = PathNode.GenerateNodes(filtered, true);
			foreach (WayPoint wp in wayPoints)
			{
				WayPoint wayPoint = wp;
				wayPoint.OnLinksChanged = (Action<WayPoint>)Delegate.Combine(wayPoint.OnLinksChanged, new Action<WayPoint>(this.WaypointLinksChanged));
			}
			this.sortedNodes = new List<PathNode>(this.nodes.Count);
			this.isCharacter = isCharacter;
		}

		// Token: 0x06002F53 RID: 12115 RVA: 0x001F4F50 File Offset: 0x001F3150
		private void WaypointLinksChanged(WayPoint wp)
		{
			PathFinder.<>c__DisplayClass15_0 CS$<>8__locals1 = new PathFinder.<>c__DisplayClass15_0();
			CS$<>8__locals1.wp = wp;
			if (Submarine.Unloading)
			{
				return;
			}
			CS$<>8__locals1.node = this.nodes.Find((PathNode n) => n.Waypoint == CS$<>8__locals1.wp);
			if (CS$<>8__locals1.node == null)
			{
				return;
			}
			int i;
			int i2;
			for (i = CS$<>8__locals1.node.connections.Count - 1; i >= 0; i = i2 - 1)
			{
				if (CS$<>8__locals1.wp.linkedTo.FirstOrDefault((MapEntity l) => l == CS$<>8__locals1.node.connections[i].Waypoint) == null)
				{
					CS$<>8__locals1.node.connections.RemoveAt(i);
					CS$<>8__locals1.node.distances.RemoveAt(i);
				}
				i2 = i;
			}
			for (int j = 0; j < CS$<>8__locals1.wp.linkedTo.Count; j++)
			{
				MapEntity mapEntity = CS$<>8__locals1.wp.linkedTo[j];
				WayPoint connected = mapEntity as WayPoint;
				if (connected != null && !CS$<>8__locals1.node.connections.Any((PathNode n) => n.Waypoint == connected))
				{
					PathNode matchingNode = this.nodes.Find((PathNode n) => n.Waypoint == connected);
					if (matchingNode == null)
					{
						return;
					}
					CS$<>8__locals1.node.connections.Add(matchingNode);
					CS$<>8__locals1.node.distances.Add(Vector2.Distance(CS$<>8__locals1.node.Position, matchingNode.Position));
				}
			}
		}

		// Token: 0x06002F54 RID: 12116 RVA: 0x001F5100 File Offset: 0x001F3300
		public SteeringPath FindPath(Vector2 start, Vector2 end, Submarine hostSub = null, string errorMsgStr = null, float minGapSize = 0f, Func<PathNode, bool> startNodeFilter = null, Func<PathNode, bool> endNodeFilter = null, Func<PathNode, bool> nodeFilter = null, bool checkVisibility = true, float outsideNodePenalty = 0f)
		{
			PathFinder.<>c__DisplayClass17_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.start = start;
			CS$<>8__locals1.startNodeFilter = startNodeFilter;
			CS$<>8__locals1.checkVisibility = checkVisibility;
			CS$<>8__locals1.end = end;
			CS$<>8__locals1.endNodeFilter = endNodeFilter;
			CS$<>8__locals1.nodeFilter = nodeFilter;
			CS$<>8__locals1.minGapSize = minGapSize;
			foreach (PathNode node in this.nodes)
			{
				node.ResetBlocked();
			}
			foreach (PathNode node2 in this.nodes)
			{
				node2.TempPosition = node2.Position;
				Submarine wpSub = node2.Waypoint.Submarine;
				if (hostSub != null && wpSub == null)
				{
					node2.TempPosition -= hostSub.SimPosition;
				}
				else if (wpSub != null && hostSub != null && wpSub != hostSub)
				{
					node2.TempPosition -= hostSub.SimPosition - wpSub.SimPosition;
				}
				else if (hostSub == null && wpSub != null)
				{
					node2.TempPosition += wpSub.SimPosition;
				}
			}
			this.sortedNodes.Clear();
			PathNode startNode = null;
			foreach (PathNode node3 in this.nodes)
			{
				float xDiff = Math.Abs(CS$<>8__locals1.start.X - node3.TempPosition.X);
				float yDiff = Math.Abs(CS$<>8__locals1.start.Y - node3.TempPosition.Y);
				if (this.InsideSubmarine)
				{
					Submarine submarine = node3.Waypoint.Submarine;
					bool? flag;
					if (submarine == null)
					{
						flag = null;
					}
					else
					{
						SubmarineInfo info = submarine.Info;
						flag = ((info != null) ? new bool?(info.IsRuin) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault())
					{
						if (yDiff > 1f && node3.Waypoint.Ladders == null && node3.Waypoint.Stairs == null)
						{
							yDiff += 10f;
						}
						if (Math.Abs(yDiff) > 1f)
						{
							yDiff *= 10f;
						}
					}
				}
				node3.TempDistance = xDiff + yDiff;
				if (node3.Waypoint.CurrentHull == null && this.ApplyPenaltyToOutsideNodes)
				{
					node3.TempDistance *= 10f;
				}
				if (node3.TempDistance <= (this.InsideSubmarine ? 100f : 800f))
				{
					if (node3.TempDistance < ConvertUnits.ToSimUnits(100f) && this.<FindPath>g__IsValidStartNode|17_0(node3, ref CS$<>8__locals1))
					{
						startNode = node3;
						break;
					}
					node3.TempDistance += (Math.Abs(CS$<>8__locals1.end.X - node3.TempPosition.X) + Math.Abs(CS$<>8__locals1.end.Y - node3.TempPosition.Y)) / 100f;
					int i = 0;
					while (i < this.sortedNodes.Count && this.sortedNodes[i].TempDistance < node3.TempDistance)
					{
						i++;
					}
					this.sortedNodes.Insert(i, node3);
				}
			}
			if (startNode == null)
			{
				foreach (PathNode node4 in this.sortedNodes)
				{
					if (this.<FindPath>g__IsValidStartNode|17_0(node4, ref CS$<>8__locals1))
					{
						startNode = node4;
						break;
					}
				}
			}
			if (startNode == null)
			{
				return new SteeringPath(true);
			}
			this.sortedNodes.Clear();
			PathNode endNode = null;
			foreach (PathNode node5 in this.nodes)
			{
				node5.TempDistance = Vector2.DistanceSquared(CS$<>8__locals1.end, node5.TempPosition);
				if (this.InsideSubmarine)
				{
					if (this.ApplyPenaltyToOutsideNodes && node5.Waypoint.CurrentHull == null)
					{
						node5.TempDistance *= 10f;
					}
					if (node5.Waypoint.ConnectedDoor != null)
					{
						node5.TempDistance *= 10f;
					}
				}
				if (node5.TempDistance <= (this.InsideSubmarine ? 10000f : 640000f))
				{
					if (node5.TempDistance < 1f && this.<FindPath>g__IsValidEndNode|17_1(node5, ref CS$<>8__locals1))
					{
						endNode = node5;
						break;
					}
					int j = 0;
					while (j < this.sortedNodes.Count && this.sortedNodes[j].TempDistance < node5.TempDistance)
					{
						j++;
					}
					this.sortedNodes.Insert(j, node5);
				}
			}
			if (endNode == null)
			{
				foreach (PathNode node6 in this.sortedNodes)
				{
					if (this.<FindPath>g__IsValidEndNode|17_1(node6, ref CS$<>8__locals1))
					{
						endNode = node6;
						break;
					}
				}
			}
			if (endNode == null)
			{
				return new SteeringPath(true);
			}
			float outsideNodeCostPenalty = outsideNodePenalty;
			if (this.ApplyPenaltyToOutsideNodes)
			{
				outsideNodeCostPenalty += 100f;
			}
			return this.FindPath(startNode, endNode, CS$<>8__locals1.nodeFilter, errorMsgStr, CS$<>8__locals1.minGapSize, outsideNodeCostPenalty);
		}

		// Token: 0x06002F55 RID: 12117 RVA: 0x001F5704 File Offset: 0x001F3904
		private SteeringPath FindPath(PathNode start, PathNode end, Func<PathNode, bool> filter = null, string errorMsgStr = "", float minGapSize = 0f, float outsideNodePenalty = 0f)
		{
			PathFinder.<>c__DisplayClass18_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.outsideNodePenalty = outsideNodePenalty;
			if (start == end)
			{
				SteeringPath path = new SteeringPath(false);
				path.AddNode(start.Waypoint);
				return path;
			}
			foreach (PathNode node in this.nodes)
			{
				node.Parent = null;
				node.state = 0;
				node.F = 0f;
				node.G = 0f;
				node.H = 0f;
			}
			start.state = 1;
			for (;;)
			{
				PathFinder.<>c__DisplayClass18_1 CS$<>8__locals2;
				CS$<>8__locals2.currNode = null;
				float dist = float.MaxValue;
				foreach (PathNode node2 in this.nodes)
				{
					if (node2.state == 1 && node2.F <= dist && (filter == null || filter(node2)) && (node2.Waypoint.ConnectedGap == null || this.CanFitThroughGap(node2.Waypoint.ConnectedGap, minGapSize)))
					{
						dist = node2.F;
						CS$<>8__locals2.currNode = node2;
					}
				}
				if (CS$<>8__locals2.currNode == null || CS$<>8__locals2.currNode == end)
				{
					break;
				}
				CS$<>8__locals2.currNode.state = 2;
				PathFinder.<>c__DisplayClass18_2 CS$<>8__locals3;
				CS$<>8__locals3.i = 0;
				while (CS$<>8__locals3.i < CS$<>8__locals2.currNode.connections.Count)
				{
					PathFinder.<>c__DisplayClass18_3 CS$<>8__locals4;
					CS$<>8__locals4.nextNode = CS$<>8__locals2.currNode.connections[CS$<>8__locals3.i];
					switch (CS$<>8__locals4.nextNode.state)
					{
					case -1:
					case 1:
					{
						float tempG = this.<FindPath>g__CalculateNodeCost|18_0(ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3, ref CS$<>8__locals4);
						if (tempG < CS$<>8__locals4.nextNode.G)
						{
							CS$<>8__locals4.nextNode.G = tempG;
							CS$<>8__locals4.nextNode.F = CS$<>8__locals4.nextNode.G + CS$<>8__locals4.nextNode.H;
							CS$<>8__locals4.nextNode.Parent = CS$<>8__locals2.currNode;
							CS$<>8__locals4.nextNode.state = 1;
						}
						break;
					}
					case 0:
					{
						CS$<>8__locals4.nextNode.H = Vector2.Distance(CS$<>8__locals4.nextNode.Position, end.Position);
						float cost = this.<FindPath>g__CalculateNodeCost|18_0(ref CS$<>8__locals1, ref CS$<>8__locals2, ref CS$<>8__locals3, ref CS$<>8__locals4);
						if (cost < float.PositiveInfinity)
						{
							CS$<>8__locals4.nextNode.G = cost;
							CS$<>8__locals4.nextNode.F = CS$<>8__locals4.nextNode.G + CS$<>8__locals4.nextNode.H;
							CS$<>8__locals4.nextNode.Parent = CS$<>8__locals2.currNode;
							CS$<>8__locals4.nextNode.state = 1;
						}
						else
						{
							CS$<>8__locals4.nextNode.state = -1;
						}
						break;
					}
					}
					int i2 = CS$<>8__locals3.i;
					CS$<>8__locals3.i = i2 + 1;
				}
			}
			if (end.state == 0 || end.Parent == null)
			{
				return new SteeringPath(true);
			}
			SteeringPath path2 = new SteeringPath(false);
			List<WayPoint> finalPath = new List<WayPoint>();
			PathNode pathNode = end;
			while (pathNode != start && pathNode != null)
			{
				finalPath.Add(pathNode.Waypoint);
				if (finalPath.Count > this.nodes.Count)
				{
					return new SteeringPath(true);
				}
				path2.Cost += pathNode.F;
				pathNode = pathNode.Parent;
			}
			finalPath.Add(start.Waypoint);
			for (int i = finalPath.Count - 1; i >= 0; i--)
			{
				path2.AddNode(finalPath[i]);
			}
			return path2;
		}

		// Token: 0x06002F56 RID: 12118 RVA: 0x001F5AC8 File Offset: 0x001F3CC8
		private bool CanFitThroughGap(Gap gap, float minWidth)
		{
			if (!gap.IsHorizontal)
			{
				return (float)gap.RectWidth > minWidth;
			}
			return (float)gap.RectHeight > minWidth;
		}

		// Token: 0x06002F57 RID: 12119 RVA: 0x001F5AE7 File Offset: 0x001F3CE7
		[CompilerGenerated]
		private bool <FindPath>g__IsValidStartNode|17_0(PathNode node, ref PathFinder.<>c__DisplayClass17_0 A_2)
		{
			return this.<FindPath>g__IsValidNode|17_2(node, new ValueTuple<bool, Vector2>(this.isCharacter, A_2.start), A_2.startNodeFilter, ref A_2);
		}

		// Token: 0x06002F58 RID: 12120 RVA: 0x001F5B08 File Offset: 0x001F3D08
		[CompilerGenerated]
		private bool <FindPath>g__IsValidEndNode|17_1(PathNode node, ref PathFinder.<>c__DisplayClass17_0 A_2)
		{
			return this.<FindPath>g__IsValidNode|17_2(node, new ValueTuple<bool, Vector2>(this.isCharacter & A_2.checkVisibility, A_2.end), A_2.endNodeFilter, ref A_2);
		}

		// Token: 0x06002F59 RID: 12121 RVA: 0x001F5B30 File Offset: 0x001F3D30
		[CompilerGenerated]
		private bool <FindPath>g__IsValidNode|17_2(PathNode node, [TupleElementNames(new string[]
		{
			"check",
			"start"
		})] ValueTuple<bool, Vector2> visibilityCheck, Func<PathNode, bool> extraFilter, ref PathFinder.<>c__DisplayClass17_0 A_4)
		{
			if (A_4.nodeFilter != null && !A_4.nodeFilter(node))
			{
				return false;
			}
			if (extraFilter != null && !extraFilter(node))
			{
				return false;
			}
			if (this.GetSingleNodePenalty != null && this.GetSingleNodePenalty(node) == null)
			{
				return false;
			}
			if (node.Waypoint.ConnectedGap != null && !this.CanFitThroughGap(node.Waypoint.ConnectedGap, A_4.minGapSize))
			{
				return false;
			}
			if (visibilityCheck.Item1)
			{
				Body body = Submarine.PickBody(visibilityCheck.Item2, node.TempPosition, null, new Category?(Category.Cat1 | Category.Cat4 | Category.Cat8), true, null, false);
				if (body != null)
				{
					if (body.UserData is Submarine)
					{
						return false;
					}
					Structure s = body.UserData as Structure;
					if (s != null && !s.IsPlatform)
					{
						return false;
					}
					if (body.UserData is VoronoiCell)
					{
						return false;
					}
					if (body.UserData is Item && body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002F5A RID: 12122 RVA: 0x001F5C48 File Offset: 0x001F3E48
		[CompilerGenerated]
		private float <FindPath>g__CalculateNodeCost|18_0(ref PathFinder.<>c__DisplayClass18_0 A_1, ref PathFinder.<>c__DisplayClass18_1 A_2, ref PathFinder.<>c__DisplayClass18_2 A_3, ref PathFinder.<>c__DisplayClass18_3 A_4)
		{
			float penalty = 0f;
			if (this.GetNodePenalty != null)
			{
				float? nodePenalty = this.GetNodePenalty(A_2.currNode, A_4.nextNode);
				if (nodePenalty == null)
				{
					return float.PositiveInfinity;
				}
				penalty += nodePenalty.Value;
			}
			if (A_2.currNode.Waypoint.CurrentHull == null)
			{
				penalty += A_1.outsideNodePenalty;
			}
			return A_2.currNode.G + A_2.currNode.distances[A_3.i] + penalty;
		}

		// Token: 0x040018A7 RID: 6311
		public PathFinder.GetNodePenaltyHandler GetNodePenalty;

		// Token: 0x040018A8 RID: 6312
		public PathFinder.GetSingleNodePenaltyHandler GetSingleNodePenalty;

		// Token: 0x040018A9 RID: 6313
		private readonly List<PathNode> nodes;

		// Token: 0x040018AA RID: 6314
		private readonly bool isCharacter;

		// Token: 0x040018AD RID: 6317
		private readonly List<PathNode> sortedNodes;

		// Token: 0x02000E7A RID: 3706
		// (Invoke) Token: 0x0600847C RID: 33916
		public delegate float? GetNodePenaltyHandler(PathNode node, PathNode prevNode);

		// Token: 0x02000E7B RID: 3707
		// (Invoke) Token: 0x06008480 RID: 33920
		public delegate float? GetSingleNodePenaltyHandler(PathNode node);
	}
}
