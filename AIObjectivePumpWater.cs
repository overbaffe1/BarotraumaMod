using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000188 RID: 392
	internal class AIObjectivePumpWater : AIObjectiveLoop<Pump>
	{
		// Token: 0x17000BDD RID: 3037
		// (get) Token: 0x06002E57 RID: 11863 RVA: 0x001EF27A File Offset: 0x001ED47A
		// (set) Token: 0x06002E58 RID: 11864 RVA: 0x001EF282 File Offset: 0x001ED482
		public override Identifier Identifier { get; set; } = "pump water".ToIdentifier();

		// Token: 0x17000BDE RID: 3038
		// (get) Token: 0x06002E59 RID: 11865 RVA: 0x001EF28B File Offset: 0x001ED48B
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BDF RID: 3039
		// (get) Token: 0x06002E5A RID: 11866 RVA: 0x001EF28E File Offset: 0x001ED48E
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BE0 RID: 3040
		// (get) Token: 0x06002E5B RID: 11867 RVA: 0x001EF291 File Offset: 0x001ED491
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002E5C RID: 11868 RVA: 0x001EF294 File Offset: 0x001ED494
		public AIObjectivePumpWater(Character character, AIObjectiveManager objectiveManager, Identifier option, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x06002E5D RID: 11869 RVA: 0x001EF2B1 File Offset: 0x001ED4B1
		protected override void FindTargets()
		{
			if (this.Option == null)
			{
				return;
			}
			base.FindTargets();
		}

		// Token: 0x06002E5E RID: 11870 RVA: 0x001EF2C8 File Offset: 0x001ED4C8
		protected override bool IsValidTarget(Pump pump)
		{
			Pump pump2 = pump;
			return ((pump2 != null) ? pump2.Item : null) != null && !pump.Item.Removed && !pump.Item.IgnoreByAI(this.character) && pump.Item.IsInteractable(this.character) && pump.CanBeSelected && !pump.IsAutoControlled && pump.Item.ConditionPercentage > 0f && pump.Item.CurrentHull != null && pump.Item.CurrentHull.FireSources.Count <= 0 && (this.character.Submarine == null || pump.Item.Submarine == null || this.character.Submarine.IsConnectedTo(pump.Item.Submarine)) && !Character.CharacterList.Any((Character c) => c.CurrentHull == pump.Item.CurrentHull && !this.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c)) && !pump.Item.IsClaimedByBallastFlora && !this.IsReady(pump);
		}

		// Token: 0x06002E5F RID: 11871 RVA: 0x001EF430 File Offset: 0x001ED630
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

		// Token: 0x06002E60 RID: 11872 RVA: 0x001EF520 File Offset: 0x001ED720
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

		// Token: 0x06002E61 RID: 11873 RVA: 0x001EF5A8 File Offset: 0x001ED7A8
		private bool IsReady(Pump pump)
		{
			if (this.Option == "stoppumping")
			{
				return !pump.IsActive || MathUtils.NearlyEqual(pump.FlowPercentage, 0f, 0.0001f);
			}
			return !pump.Item.InWater || (pump.IsActive && pump.FlowPercentage <= -99.9f);
		}

		// Token: 0x06002E62 RID: 11874 RVA: 0x001EF610 File Offset: 0x001ED810
		protected override AIObjective ObjectiveConstructor(Pump pump)
		{
			return new AIObjectiveOperateItem(pump, this.character, this.objectiveManager, this.Option, false, null, false, null, 1f)
			{
				completionCondition = (() => this.IsReady(pump))
			};
		}

		// Token: 0x06002E63 RID: 11875 RVA: 0x001EF669 File Offset: 0x001ED869
		protected override void OnObjectiveCompleted(AIObjective objective, Pump target)
		{
			HumanAIController.RemoveTargets<AIObjectivePumpWater, Pump>(this.character, target);
		}

		// Token: 0x04001832 RID: 6194
		private List<Pump> pumpList;
	}
}
