using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000065 RID: 101
	internal class AIObjectiveChargeBatteries : AIObjectiveLoop<PowerContainer>
	{
		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x06000E27 RID: 3623 RVA: 0x00089F2F File Offset: 0x0008812F
		// (set) Token: 0x06000E28 RID: 3624 RVA: 0x00089F37 File Offset: 0x00088137
		public override Identifier Identifier { get; set; } = "charge batteries".ToIdentifier();

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x06000E29 RID: 3625 RVA: 0x00089F40 File Offset: 0x00088140
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x00089F43 File Offset: 0x00088143
		public AIObjectiveChargeBatteries(Character character, AIObjectiveManager objectiveManager, Identifier option, float priorityModifier) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00089F60 File Offset: 0x00088160
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

		// Token: 0x06000E2C RID: 3628 RVA: 0x0008A074 File Offset: 0x00088274
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

		// Token: 0x06000E2D RID: 3629 RVA: 0x0008A0FC File Offset: 0x000882FC
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

		// Token: 0x06000E2E RID: 3630 RVA: 0x0008A194 File Offset: 0x00088394
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

		// Token: 0x06000E2F RID: 3631 RVA: 0x0008A1EC File Offset: 0x000883EC
		protected override AIObjective ObjectiveConstructor(PowerContainer battery)
		{
			return new AIObjectiveOperateItem(battery, this.character, this.objectiveManager, this.Option, false, null, false, null, base.PriorityModifier)
			{
				Override = !this.character.IsDismissed,
				completionCondition = (() => this.IsReady(battery))
			};
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x0008A25A File Offset: 0x0008845A
		protected override void OnObjectiveCompleted(AIObjective objective, PowerContainer target)
		{
			HumanAIController.RemoveTargets<AIObjectiveChargeBatteries, PowerContainer>(this.character, target);
		}

		// Token: 0x040006A0 RID: 1696
		private IEnumerable<PowerContainer> batteryList;
	}
}
