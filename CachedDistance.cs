using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200015D RID: 349
	public class CachedDistance
	{
		// Token: 0x06002AC0 RID: 10944 RVA: 0x001D8579 File Offset: 0x001D6779
		public CachedDistance(Vector2 startWorldPos, Vector2 endWorldPos, float dist, double recalculationTime)
		{
			this.StartWorldPos = startWorldPos;
			this.EndWorldPos = endWorldPos;
			this.Distance = dist;
			this.RecalculationTime = recalculationTime;
		}

		// Token: 0x06002AC1 RID: 10945 RVA: 0x001D85A0 File Offset: 0x001D67A0
		public bool ShouldUpdateDistance(Vector2 currentStartWorldPos, Vector2 currentEndWorldPos, float minDistanceToUpdate = 500f)
		{
			if (Timing.TotalTime < this.RecalculationTime)
			{
				return false;
			}
			float minDistSquared = minDistanceToUpdate * minDistanceToUpdate;
			return Vector2.DistanceSquared(this.StartWorldPos, currentStartWorldPos) > minDistSquared || Vector2.DistanceSquared(this.EndWorldPos, currentEndWorldPos) > minDistSquared;
		}

		// Token: 0x0400162A RID: 5674
		public readonly Vector2 StartWorldPos;

		// Token: 0x0400162B RID: 5675
		public readonly Vector2 EndWorldPos;

		// Token: 0x0400162C RID: 5676
		public readonly float Distance;

		// Token: 0x0400162D RID: 5677
		public double RecalculationTime;
	}
}
