using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000095 RID: 149
	internal class ShipIssueWorkerOperateWeapons : ShipIssueWorkerItem
	{
		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x060012A9 RID: 4777 RVA: 0x000A445A File Offset: 0x000A265A
		public override float RedundantIssueModifier
		{
			get
			{
				return 0.8f;
			}
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x060012AA RID: 4778 RVA: 0x000A4461 File Offset: 0x000A2661
		public override bool AllowEasySwitching
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060012AB RID: 4779 RVA: 0x000A4464 File Offset: 0x000A2664
		public ShipIssueWorkerOperateWeapons(ShipCommandManager shipCommandManager, Order order) : base(shipCommandManager, order)
		{
		}

		// Token: 0x060012AC RID: 4780 RVA: 0x000A447C File Offset: 0x000A267C
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

		// Token: 0x060012AD RID: 4781 RVA: 0x000A4558 File Offset: 0x000A2758
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

		// Token: 0x040008D5 RID: 2261
		private readonly List<float> targetingImportances = new List<float>();
	}
}
