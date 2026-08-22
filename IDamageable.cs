using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000316 RID: 790
	internal interface IDamageable
	{
		// Token: 0x17001088 RID: 4232
		// (get) Token: 0x06003F08 RID: 16136
		Vector2 SimPosition { get; }

		// Token: 0x17001089 RID: 4233
		// (get) Token: 0x06003F09 RID: 16137
		Vector2 WorldPosition { get; }

		// Token: 0x1700108A RID: 4234
		// (get) Token: 0x06003F0A RID: 16138
		float Health { get; }

		// Token: 0x06003F0B RID: 16139
		AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true);

		// Token: 0x02000FDD RID: 4061
		public readonly struct AttackEventData
		{
			// Token: 0x06008A6B RID: 35435 RVA: 0x003AA5ED File Offset: 0x003A87ED
			public AttackEventData(ISpatialEntity attacker, IDamageable targetEntity, Limb targetLimb, Vector2 attackSimPosition)
			{
				this.Attacker = attacker;
				this.TargetEntity = targetEntity;
				this.TargetLimb = targetLimb;
				this.AttackSimPosition = attackSimPosition;
			}

			// Token: 0x040056C8 RID: 22216
			public readonly ISpatialEntity Attacker;

			// Token: 0x040056C9 RID: 22217
			public readonly IDamageable TargetEntity;

			// Token: 0x040056CA RID: 22218
			public readonly Limb TargetLimb;

			// Token: 0x040056CB RID: 22219
			public readonly Vector2 AttackSimPosition;
		}
	}
}
