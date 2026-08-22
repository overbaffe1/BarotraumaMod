using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000097 RID: 151
	internal class ShipGlobalIssueRepairSystems : ShipGlobalIssue
	{
		// Token: 0x060012B0 RID: 4784 RVA: 0x000A46A8 File Offset: 0x000A28A8
		public ShipGlobalIssueRepairSystems(ShipCommandManager shipCommandManager) : base(shipCommandManager)
		{
		}

		// Token: 0x060012B1 RID: 4785 RVA: 0x000A46BC File Offset: 0x000A28BC
		public override void CalculateGlobalIssue()
		{
			this.itemsNeedingRepair.Clear();
			foreach (Item item in this.shipCommandManager.CommandedSubmarine.GetItems(true))
			{
				if (AIObjectiveRepairItems.ViableForRepair(item, this.shipCommandManager.character, this.shipCommandManager.character.AIController as HumanAIController) && !AIObjectiveRepairItems.NearlyFullCondition(item))
				{
					this.itemsNeedingRepair.Add(item);
				}
			}
			if (this.itemsNeedingRepair.Any<Item>())
			{
				this.itemsNeedingRepair.Sort((Item x, Item y) => y.ConditionPercentage.CompareTo(x.ConditionPercentage));
				float modifiedPercentage = this.itemsNeedingRepair.TakeLast(3).Average((Item x) => x.ConditionPercentage) * 0.6f + this.itemsNeedingRepair.TakeLast(10).Average((Item x) => x.ConditionPercentage) * 0.4f;
				base.GlobalImportance = 100f - modifiedPercentage;
			}
		}

		// Token: 0x040008D6 RID: 2262
		private readonly List<Item> itemsNeedingRepair = new List<Item>();
	}
}
