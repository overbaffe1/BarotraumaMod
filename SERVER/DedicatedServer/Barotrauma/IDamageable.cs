using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000231 RID: 561
	internal interface IDamageable
	{
		// Token: 0x17000B0E RID: 2830
		// (get) Token: 0x0600268A RID: 9866
		Vector2 SimPosition { get; }

		// Token: 0x17000B0F RID: 2831
		// (get) Token: 0x0600268B RID: 9867
		Vector2 WorldPosition { get; }

		// Token: 0x17000B10 RID: 2832
		// (get) Token: 0x0600268C RID: 9868
		float Health { get; }

		// Token: 0x0600268D RID: 9869
		AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true);

		// Token: 0x020009FF RID: 2559
		public readonly struct AttackEventData
		{
			// Token: 0x06005B80 RID: 23424 RVA: 0x001FEC12 File Offset: 0x001FCE12
			public AttackEventData(ISpatialEntity attacker, IDamageable targetEntity, Limb targetLimb, Vector2 attackSimPosition)
			{
				this.Attacker = attacker;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.AttackSimPosition = attackSimPosition;
			}

			// Token: 0x040034EF RID: 13551
			public readonly ISpatialEntity Attacker;

			// Token: 0x040034F0 RID: 13552
			public readonly IDamageable TargetEntity;

			// Token: 0x040034F1 RID: 13553
			public readonly Limb TargetLimb;

			// Token: 0x040034F2 RID: 13554
			public readonly Vector2 AttackSimPosition;
		}
	}
}
