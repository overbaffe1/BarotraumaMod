using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020002B6 RID: 694
	internal class MalfunctionEvent : Event
	{
		// Token: 0x06003C12 RID: 15378 RVA: 0x00226CE7 File Offset: 0x00224EE7
		public override string ToString()
		{
			return "MalfunctionEvent (" + string.Join<Identifier>(", ", this.targetItemIdentifiers) + ")";
		}

		// Token: 0x06003C13 RID: 15379 RVA: 0x00226D08 File Offset: 0x00224F08
		public MalfunctionEvent(EventPrefab prefab, int seed) : base(prefab, seed)
		{
			this.targetItems = new List<Item>();
			this.minItemAmount = prefab.ConfigElement.GetAttributeInt("minitemamount", 1);
			this.maxItemAmount = prefab.ConfigElement.GetAttributeInt("maxitemamount", this.minItemAmount);
			this.decreaseConditionAmount = prefab.ConfigElement.GetAttributeFloat("decreaseconditionamount", 0f);
			this.duration = prefab.ConfigElement.GetAttributeFloat("duration", 0f);
			this.targetItemIdentifiers = prefab.ConfigElement.GetAttributeIdentifierArray("itemidentifiers", Array.Empty<Identifier>(), true);
		}

		// Token: 0x06003C14 RID: 15380 RVA: 0x00226DB0 File Offset: 0x00224FB0
		protected override void InitEventSpecific(EventSet parentSet)
		{
			List<Item> matchingItems = Item.ItemList.FindAll((Item i) => i.Condition > 0f && this.targetItemIdentifiers.Contains(i.Prefab.Identifier));
			int itemAmount = Rand.Range(this.minItemAmount, this.maxItemAmount, Rand.RandSync.ServerAndClient);
			int j = 0;
			while (j < itemAmount && matchingItems.Count != 0)
			{
				this.targetItems.Add(matchingItems[Rand.Int(matchingItems.Count, Rand.RandSync.ServerAndClient)]);
				j++;
			}
		}

		// Token: 0x06003C15 RID: 15381 RVA: 0x00226E18 File Offset: 0x00225018
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			if (this.targetItems.Count == 0 || this.timer >= this.duration)
			{
				this.Finish();
				return;
			}
			this.targetItems.RemoveAll((Item i) => i.Removed || i.Condition <= 0f);
			foreach (Item item in this.targetItems)
			{
				if (this.duration <= 0f)
				{
					item.Condition = 0f;
				}
				else
				{
					item.Condition -= this.decreaseConditionAmount / this.duration * deltaTime;
				}
			}
			this.timer += deltaTime;
		}

		// Token: 0x04001ED7 RID: 7895
		private Identifier[] targetItemIdentifiers;

		// Token: 0x04001ED8 RID: 7896
		private List<Item> targetItems;

		// Token: 0x04001ED9 RID: 7897
		private int minItemAmount;

		// Token: 0x04001EDA RID: 7898
		private int maxItemAmount;

		// Token: 0x04001EDB RID: 7899
		private float decreaseConditionAmount;

		// Token: 0x04001EDC RID: 7900
		private float duration;

		// Token: 0x04001EDD RID: 7901
		private float timer;
	}
}
