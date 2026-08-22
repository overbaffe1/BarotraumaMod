using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200008C RID: 140
	internal class PathNode
	{
		// Token: 0x06001241 RID: 4673 RVA: 0x000A1C30 File Offset: 0x0009FE30
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PathNode ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.WayPointID);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001242 RID: 4674 RVA: 0x000A1C67 File Offset: 0x0009FE67
		public PathNode(WayPoint wayPoint)
		{
			this.Waypoint = wayPoint;
			this.Position = wayPoint.SimPosition;
			this.WayPointID = (int)this.Waypoint.ID;
		}

		// Token: 0x06001243 RID: 4675 RVA: 0x000A1CA0 File Offset: 0x0009FEA0
		public static List<PathNode> GenerateNodes(List<WayPoint> wayPoints, bool removeOrphans)
		{
			Dictionary<int, PathNode> nodes = new Dictionary<int, PathNode>();
			foreach (WayPoint wayPoint in wayPoints)
			{
				if (wayPoint != null && !nodes.ContainsKey((int)wayPoint.ID))
				{
					nodes.Add((int)wayPoint.ID, new PathNode(wayPoint));
				}
			}
			foreach (KeyValuePair<int, PathNode> node in nodes)
			{
				foreach (MapEntity linked in node.Value.Waypoint.linkedTo)
				{
					PathNode connectedNode;
					nodes.TryGetValue((int)linked.ID, out connectedNode);
					if (connectedNode != null)
					{
						if (!node.Value.connections.Contains(connectedNode))
						{
							node.Value.connections.Add(connectedNode);
						}
						if (!connectedNode.connections.Contains(node.Value))
						{
							connectedNode.connections.Add(node.Value);
						}
					}
				}
			}
			List<PathNode> nodeList = nodes.Values.ToList<PathNode>();
			if (removeOrphans)
			{
				nodeList.RemoveAll((PathNode n) => n.connections.Count == 0);
			}
			foreach (PathNode node2 in nodeList)
			{
				node2.distances = new List<float>();
				for (int i = 0; i < node2.connections.Count; i++)
				{
					node2.distances.Add(Vector2.Distance(node2.Position, node2.connections[i].Position));
				}
			}
			return nodeList;
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x000A1EBC File Offset: 0x000A00BC
		public bool IsBlocked()
		{
			if (this.blocked != null)
			{
				return this.blocked.Value;
			}
			this.blocked = new bool?(false);
			if (this.Waypoint.Submarine != null)
			{
				return this.blocked.Value;
			}
			Level.Tunnel tunnel = this.Waypoint.Tunnel;
			if (tunnel == null || tunnel.Type != Level.TunnelType.Cave)
			{
				return this.blocked.Value;
			}
			foreach (LevelWall w in Level.Loaded.ExtraWalls)
			{
				if (w.IsPointInside(this.Waypoint.Position))
				{
					DestructibleLevelWall d = w as DestructibleLevelWall;
					if (d != null)
					{
						this.blocked = new bool?(!d.Destroyed);
					}
					if (this.blocked.Value)
					{
						break;
					}
				}
			}
			return this.blocked.Value;
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x000A1FC0 File Offset: 0x000A01C0
		public void ResetBlocked()
		{
			this.blocked = null;
		}

		// Token: 0x040008A0 RID: 2208
		public int state;

		// Token: 0x040008A1 RID: 2209
		public PathNode Parent;

		// Token: 0x040008A2 RID: 2210
		public float F;

		// Token: 0x040008A3 RID: 2211
		public float G;

		// Token: 0x040008A4 RID: 2212
		public float H;

		// Token: 0x040008A5 RID: 2213
		public readonly List<PathNode> connections = new List<PathNode>();

		// Token: 0x040008A6 RID: 2214
		public List<float> distances;

		// Token: 0x040008A7 RID: 2215
		public Vector2 TempPosition;

		// Token: 0x040008A8 RID: 2216
		public float TempDistance;

		// Token: 0x040008A9 RID: 2217
		public readonly WayPoint Waypoint;

		// Token: 0x040008AA RID: 2218
		public readonly Vector2 Position;

		// Token: 0x040008AB RID: 2219
		public readonly int WayPointID;

		// Token: 0x040008AC RID: 2220
		private bool? blocked;
	}
}
