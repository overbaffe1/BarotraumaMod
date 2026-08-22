using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020001C3 RID: 451
	internal class MalfunctionEvent : Event
	{
		// Token: 0x06002170 RID: 8560 RVA: 0x000E0DB3 File Offset: 0x000DEFB3
		public override string ToString()
		{
			return "MalfunctionEvent (" + string.Join<Identifier>(", ", this.targetItemIdentifiers) + ")";
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x000E0DD4 File Offset: 0x000DEFD4
		public MalfunctionEvent(EventPrefab prefab, int seed) : base(prefab, seed)
		{
			this.targetItems = new List<Item>();
			this.minItemAmount = prefab.ConfigElement.GetAttributeInt("minitemamount", 1);
			this.maxItemAmount = prefab.ConfigElement.GetAttributeInt("maxitemamount", this.minItemAmount);
			this.decreaseConditionAmount = prefab.ConfigElement.GetAttributeFloat("decreaseconditionamount", 0f);
			this.duration = prefab.ConfigElement.GetAttributeFloat("duration", 0f);
			this.targetItemIdentifiers = prefab.ConfigElement.GetAttributeIdentifierArray("itemidentifiers", Array.Empty<Identifier>(), true);
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x000E0E7C File Offset: 0x000DF07C
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

		// Token: 0x06002173 RID: 8563 RVA: 0x000E0EE4 File Offset: 0x000DF0E4
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

		// Token: 0x04000FF1 RID: 4081
		private Identifier[] targetItemIdentifiers;

		// Token: 0x04000FF2 RID: 4082
		private List<Item> targetItems;

		// Token: 0x04000FF3 RID: 4083
		private int minItemAmount;

		// Token: 0x04000FF4 RID: 4084
		private int maxItemAmount;

		// Token: 0x04000FF5 RID: 4085
		private float decreaseConditionAmount;

		// Token: 0x04000FF6 RID: 4086
		private float duration;

		// Token: 0x04000FF7 RID: 4087
		private float timer;
	}
}
