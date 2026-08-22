using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D3 RID: 211
	internal class Swarm
	{
		// Token: 0x06001C41 RID: 7233 RVA: 0x0011A29C File Offset: 0x0011849C
		public Vector2 MidPoint()
		{
			if (this.Members.Count == 0)
			{
				return Vector2.Zero;
			}
			Vector2 midPoint = Vector2.Zero;
			foreach (BackgroundCreature member in this.Members)
			{
				midPoint += member.SimPosition;
			}
			midPoint /= (float)this.Members.Count;
			return midPoint;
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x0011A324 File Offset: 0x00118524
		public Vector2 AvgVelocity()
		{
			if (this.Members.Count == 0)
			{
				return Vector2.Zero;
			}
			Vector2 avgVel = Vector2.Zero;
			foreach (BackgroundCreature member in this.Members)
			{
				avgVel += member.Velocity;
			}
			avgVel /= (float)this.Members.Count;
			return avgVel;
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x0011A3AC File Offset: 0x001185AC
		public Swarm(List<BackgroundCreature> members, float maxDistance, float cohesion)
		{
			this.Members = members;
			this.MaxDistance = maxDistance;
			this.Cohesion = cohesion;
			foreach (BackgroundCreature bgSprite in members)
			{
				bgSprite.Swarm = this;
			}
		}

		// Token: 0x04000E88 RID: 3720
		public List<BackgroundCreature> Members;

		// Token: 0x04000E89 RID: 3721
		public readonly float MaxDistance;

		// Token: 0x04000E8A RID: 3722
		public readonly float Cohesion;
	}
}
