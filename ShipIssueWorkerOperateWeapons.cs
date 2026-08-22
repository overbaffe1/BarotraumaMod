using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200019B RID: 411
	internal class ShipIssueWorkerOperateWeapons : ShipIssueWorkerItem
	{
		// Token: 0x17000C4E RID: 3150
		// (get) Token: 0x06002FB1 RID: 12209 RVA: 0x001F733A File Offset: 0x001F553A
		public override float RedundantIssueModifier
		{
			get
			{
				return 0.8f;
			}
		}

		// Token: 0x17000C4F RID: 3151
		// (get) Token: 0x06002FB2 RID: 12210 RVA: 0x001F7341 File Offset: 0x001F5541
		public override bool AllowEasySwitching
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002FB3 RID: 12211 RVA: 0x001F7344 File Offset: 0x001F5544
		public ShipIssueWorkerOperateWeapons(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x06002FB4 RID: 12212 RVA: 0x001F735C File Offset: 0x001F555C
		private float GetTargetingImportance(Entity entity)
		{
			float currentDistanceToEnemy = Vector2.Distance(entity.WorldPosition, base.TargetItem.WorldPosition);
			if (currentDistanceToEnemy > 10000f)
			{
				return 0f;
			}
			float importance = MathHelper.Clamp(100f - currentDistanceToEnemy / 100f, 10f, 50f);
			if (base.TargetItem.Submarine != null && importance > 0f)
			{
				Turret turret = base.TargetItemComponent as Turret;
				if (turret != null)
				{
					if (!turret.IsWithinAimingRadius(entity.WorldPosition))
					{
						importance *= 0.1f;
					}
				}
				else
				{
					Vector2 dir = entity.WorldPosition - base.TargetItem.WorldPosition;
					Vector2 submarineDir = base.TargetItem.WorldPosition - base.TargetItem.Submarine.WorldPosition;
					if (Vector2.Dot(dir, submarineDir) < 0f)
					{
						importance *= 0.1f;
					}
				}
			}
			return importance;
		}

		// Token: 0x06002FB5 RID: 12213 RVA: 0x001F7438 File Offset: 0x001F5638
		public override void CalculateImportanceSpecific()
		{
			this.targetingImportances.Clear();
			foreach (Character character in this.shipCommandManager.EnemyCharacters)
			{
				this.targetingImportances.Add(this.GetTargetingImportance(character));
			}
			if (this.targetingImportances.Any((float i) => i > 0f))
			{
				this.targetingImportances.Sort();
				base.Importance = Math.Max(this.targetingImportances.TakeLast(3).Sum(), 10f);
			}
			Turret turret = base.TargetItemComponent as Turret;
			if (turret != null && !turret.HasPowerToShoot())
			{
				base.Importance = Math.Max(10f / this.RedundantIssueModifier, base.Importance);
				return;
			}
		}

		// Token: 0x040018CF RID: 6351
		private readonly List<float> targetingImportances = new List<float>();
	}
}
