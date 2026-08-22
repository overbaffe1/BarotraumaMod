using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000052 RID: 82
	public class CachedDistance
	{
		// Token: 0x06000C2B RID: 3115 RVA: 0x0007417D File Offset: 0x0007237D
		public CachedDistance(Vector2 startWorldPos, Vector2 endWorldPos, float dist, double recalculationTime)
		{
			this.StartWorldPos = startWorldPos;
			this.EndWorldPos = endWorldPos;
			this.Distance = dist;
			this.RecalculationTime = recalculationTime;
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x000741A4 File Offset: 0x000723A4
		public bool ShouldUpdateDistance(Vector2 currentStartWorldPos, Vector2 currentEndWorldPos, float minDistanceToUpdate = 500f)
		{
			if (Timing.TotalTime < this.RecalculationTime)
			{
				return false;
			}
			float minDistSquared = minDistanceToUpdate * minDistanceToUpdate;
			return Vector2.DistanceSquared(this.StartWorldPos, currentStartWorldPos) > minDistSquared || Vector2.DistanceSquared(this.EndWorldPos, currentEndWorldPos) > minDistSquared;
		}

		// Token: 0x0400053F RID: 1343
		public readonly Vector2 StartWorldPos;

		// Token: 0x04000540 RID: 1344
		public readonly Vector2 EndWorldPos;

		// Token: 0x04000541 RID: 1345
		public readonly float Distance;

		// Token: 0x04000542 RID: 1346
		public double RecalculationTime;
	}
}
