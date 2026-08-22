using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000192 RID: 402
	internal class PathNode
	{
		// Token: 0x06002F49 RID: 12105 RVA: 0x001F4AC8 File Offset: 0x001F2CC8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PathNode ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.WayPointID);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06002F4A RID: 12106 RVA: 0x001F4AFF File Offset: 0x001F2CFF
		public PathNode(WayPoint wayPoint)
		{
			this.Waypoint = wayPoint;
			this.Position = wayPoint.SimPosition;
			this.WayPointID = (int)this.Waypoint.ID;
		}

		// Token: 0x06002F4B RID: 12107 RVA: 0x001F4B38 File Offset: 0x001F2D38
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

		// Token: 0x06002F4C RID: 12108 RVA: 0x001F4D54 File Offset: 0x001F2F54
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

		// Token: 0x06002F4D RID: 12109 RVA: 0x001F4E58 File Offset: 0x001F3058
		public void ResetBlocked()
		{
			this.blocked = null;
		}

		// Token: 0x0400189A RID: 6298
		public int state;

		// Token: 0x0400189B RID: 6299
		public PathNode Parent;

		// Token: 0x0400189C RID: 6300
		public float F;

		// Token: 0x0400189D RID: 6301
		public float G;

		// Token: 0x0400189E RID: 6302
		public float H;

		// Token: 0x0400189F RID: 6303
		public readonly List<PathNode> connections = new List<PathNode>();

		// Token: 0x040018A0 RID: 6304
		public List<float> distances;

		// Token: 0x040018A1 RID: 6305
		public Vector2 TempPosition;

		// Token: 0x040018A2 RID: 6306
		public float TempDistance;

		// Token: 0x040018A3 RID: 6307
		public readonly WayPoint Waypoint;

		// Token: 0x040018A4 RID: 6308
		public readonly Vector2 Position;

		// Token: 0x040018A5 RID: 6309
		public readonly int WayPointID;

		// Token: 0x040018A6 RID: 6310
		private bool? blocked;
	}
}
