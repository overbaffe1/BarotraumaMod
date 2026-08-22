using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200019D RID: 413
	internal class ShipGlobalIssueRepairSystems : ShipGlobalIssue
	{
		// Token: 0x06002FB8 RID: 12216 RVA: 0x001F7588 File Offset: 0x001F5788
		public ShipGlobalIssueRepairSystems(ShipCommandManager shipCommandManager) : base(shipCommandManager)
		{
		}

		// Token: 0x06002FB9 RID: 12217 RVA: 0x001F759C File Offset: 0x001F579C
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

		// Token: 0x040018D0 RID: 6352
		private readonly List<Item> itemsNeedingRepair = new List<Item>();
	}
}
