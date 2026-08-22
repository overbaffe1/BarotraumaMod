using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016B RID: 363
	internal class AIObjectiveChargeBatteries : AIObjectiveLoop<PowerContainer>
	{
		// Token: 0x17000ADC RID: 2780
		// (get) Token: 0x06002B2F RID: 11055 RVA: 0x001DCDC0 File Offset: 0x001DAFC0
		// (set) Token: 0x06002B30 RID: 11056 RVA: 0x001DCDC8 File Offset: 0x001DAFC8
		public override Identifier Identifier { get; set; } = "charge batteries".ToIdentifier();

		// Token: 0x17000ADD RID: 2781
		// (get) Token: 0x06002B31 RID: 11057 RVA: 0x001DCDD1 File Offset: 0x001DAFD1
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002B32 RID: 11058 RVA: 0x001DCDD4 File Offset: 0x001DAFD4
		public AIObjectiveChargeBatteries(Character character, AIObjectiveManager objectiveManager, Identifier option, float priorityModifier) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x06002B33 RID: 11059 RVA: 0x001DCDF4 File Offset: 0x001DAFF4
		protected override bool IsValidTarget(PowerContainer battery)
		{
			if (battery == null)
			{
				return false;
			}
			if (battery.OutputDisabled)
			{
				return false;
			}
			Item item = battery.Item;
			return !item.IgnoreByAI(this.character) && item.IsInteractable(this.character) && item.Submarine != null && item.CurrentHull != null && item.Submarine.TeamID == this.character.TeamID && (this.character.Submarine == null || this.character.Submarine.IsConnectedTo(item.Submarine)) && item.ConditionPercentage > 0f && !item.IsClaimedByBallastFlora && !Character.CharacterList.Any((Character c) => c.CurrentHull == item.CurrentHull && !this.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c)) && !this.IsReady(battery);
		}

		// Token: 0x06002B34 RID: 11060 RVA: 0x001DCF08 File Offset: 0x001DB108
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (this.Option == "charge")
			{
				return base.Targets.Max((PowerContainer t) => MathHelper.Lerp(100f, 0f, Math.Abs(0.5f - t.RechargeRatio)));
			}
			return base.Targets.Max((PowerContainer t) => MathHelper.Lerp(0f, 100f, t.RechargeRatio));
		}

		// Token: 0x06002B35 RID: 11061 RVA: 0x001DCF90 File Offset: 0x001DB190
		protected override IEnumerable<PowerContainer> GetList()
		{
			if (this.batteryList == null)
			{
				if (this.character == null || this.character.Submarine == null)
				{
					return Array.Empty<PowerContainer>();
				}
				this.batteryList = from i in this.character.Submarine.GetItems(true)
				select i.GetComponent<PowerContainer>() into b
				where b != null
				select b;
			}
			return this.batteryList;
		}

		// Token: 0x06002B36 RID: 11062 RVA: 0x001DD028 File Offset: 0x001DB228
		private bool IsReady(PowerContainer battery)
		{
			if (battery.HasBeenTuned && this.character.IsDismissed)
			{
				return true;
			}
			if (this.Option == "charge")
			{
				return battery.RechargeRatio >= 0.5f;
			}
			return battery.RechargeRatio <= 0f;
		}

		// Token: 0x06002B37 RID: 11063 RVA: 0x001DD080 File Offset: 0x001DB280
		protected override AIObjective ObjectiveConstructor(PowerContainer battery)
		{
			return new AIObjectiveOperateItem(battery, this.character, this.objectiveManager, this.Option, false, null, false, null, base.PriorityModifier)
			{
				Override = !this.character.IsDismissed,
				completionCondition = (() => this.IsReady(battery))
			};
		}

		// Token: 0x06002B38 RID: 11064 RVA: 0x001DD0EE File Offset: 0x001DB2EE
		protected override void OnObjectiveCompleted(AIObjective objective, PowerContainer target)
		{
			HumanAIController.RemoveTargets<AIObjectiveChargeBatteries, PowerContainer>(this.character, target);
		}

		// Token: 0x0400169A RID: 5786
		private IEnumerable<PowerContainer> batteryList;
	}
}
