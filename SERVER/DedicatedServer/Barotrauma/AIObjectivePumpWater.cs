using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000082 RID: 130
	internal class AIObjectivePumpWater : AIObjectiveLoop<Pump>
	{
		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x0600114F RID: 4431 RVA: 0x0009C3E2 File Offset: 0x0009A5E2
		// (set) Token: 0x06001150 RID: 4432 RVA: 0x0009C3EA File Offset: 0x0009A5EA
		public override Identifier Identifier { get; set; } = "pump water".ToIdentifier();

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x06001151 RID: 4433 RVA: 0x0009C3F3 File Offset: 0x0009A5F3
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06001152 RID: 4434 RVA: 0x0009C3F6 File Offset: 0x0009A5F6
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001153 RID: 4435 RVA: 0x0009C3F9 File Offset: 0x0009A5F9
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x0009C3FC File Offset: 0x0009A5FC
		public AIObjectivePumpWater(Character character, AIObjectiveManager objectiveManager, Identifier option, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x06001155 RID: 4437 RVA: 0x0009C419 File Offset: 0x0009A619
		protected override void FindTargets()
		{
			if (this.Option == null)
			{
				return;
			}
			base.FindTargets();
		}

		// Token: 0x06001156 RID: 4438 RVA: 0x0009C430 File Offset: 0x0009A630
		protected override bool IsValidTarget(Pump pump)
		{
			Pump pump2 = pump;
			return ((pump2 != null) ? pump2.Item : null) != null && !pump.Item.Removed && !pump.Item.IgnoreByAI(this.character) && pump.Item.IsInteractable(this.character) && pump.CanBeSelected && !pump.IsAutoControlled && pump.Item.ConditionPercentage > 0f && pump.Item.CurrentHull != null && pump.Item.CurrentHull.FireSources.Count <= 0 && (this.character.Submarine == null || pump.Item.Submarine == null || this.character.Submarine.IsConnectedTo(pump.Item.Submarine)) && !Character.CharacterList.Any((Character c) => c.CurrentHull == pump.Item.CurrentHull && !this.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c)) && !pump.Item.IsClaimedByBallastFlora && !this.IsReady(pump);
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x0009C598 File Offset: 0x0009A798
		protected override IEnumerable<Pump> GetList()
		{
			if (this.pumpList == null)
			{
				if (this.character == null || this.character.Submarine == null)
				{
					return Array.Empty<Pump>();
				}
				this.pumpList = new List<Pump>();
				foreach (Item item in this.character.Submarine.GetItems(true))
				{
					Pump pump = item.GetComponent<Pump>();
					if (pump != null && pump.Item.Submarine != null && pump.Item.CurrentHull != null && pump.Item.Submarine.TeamID == this.character.TeamID && !pump.Item.HasTag(Tags.Ballast))
					{
						this.pumpList.Add(pump);
					}
				}
			}
			return this.pumpList;
		}

		// Token: 0x06001158 RID: 4440 RVA: 0x0009C688 File Offset: 0x0009A888
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (this.Option == "stoppumping")
			{
				return base.Targets.Max((Pump t) => MathHelper.Lerp(0f, 100f, Math.Abs(t.FlowPercentage / 100f)));
			}
			return base.Targets.Max((Pump t) => MathHelper.Lerp(100f, 0f, Math.Abs(-t.FlowPercentage / 100f)));
		}

		// Token: 0x06001159 RID: 4441 RVA: 0x0009C710 File Offset: 0x0009A910
		private bool IsReady(Pump pump)
		{
			if (this.Option == "stoppumping")
			{
				return !pump.IsActive || MathUtils.NearlyEqual(pump.FlowPercentage, 0f, 0.0001f);
			}
			return !pump.Item.InWater || (pump.IsActive && pump.FlowPercentage <= -99.9f);
		}

		// Token: 0x0600115A RID: 4442 RVA: 0x0009C778 File Offset: 0x0009A978
		protected override AIObjective ObjectiveConstructor(Pump pump)
		{
			return new AIObjectiveOperateItem(pump, this.character, this.objectiveManager, this.Option, false, null, false, null, 1f)
			{
				completionCondition = (() => this.IsReady(pump))
			};
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x0009C7D1 File Offset: 0x0009A9D1
		protected override void OnObjectiveCompleted(AIObjective objective, Pump target)
		{
			HumanAIController.RemoveTargets<AIObjectivePumpWater, Pump>(this.character, target);
		}

		// Token: 0x04000838 RID: 2104
		private List<Pump> pumpList;
	}
}
