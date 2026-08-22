using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000049 RID: 73
	internal class InventoryHighlightAction : EventAction
	{
		// Token: 0x06000A94 RID: 2708 RVA: 0x0006249C File Offset: 0x0006069C
		private void SetHighlight(Entity entity)
		{
			Item item = entity as Item;
			if (item != null)
			{
				int i = 0;
				using (IEnumerator<ItemContainer> enumerator = item.GetComponents<ItemContainer>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ItemContainer itemContainer = enumerator.Current;
						if (this.ItemContainerIndex == -1 || i == this.ItemContainerIndex)
						{
							this.SetHighlight(itemContainer.Inventory);
						}
						i++;
					}
					return;
				}
			}
			Character c = entity as Character;
			if (c != null)
			{
				this.SetHighlight(c.Inventory);
			}
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x0006252C File Offset: 0x0006072C
		private void SetHighlight(Inventory inventory)
		{
			if (((inventory != null) ? inventory.visualSlots : null) == null)
			{
				return;
			}
			for (int i = 0; i < inventory.visualSlots.Length; i++)
			{
				if (inventory.visualSlots[i].HighlightTimer <= 0f)
				{
					Item item = inventory.GetItemAt(i);
					if (this.IsSuitableItem(item) || (this.Recursive && ((item != null) ? item.OwnInventory : null) != null && item.OwnInventory.FindAllItems((Item it) => this.IsSuitableItem(it), true, null).Any<Item>()))
					{
						inventory.visualSlots[i].ShowBorderHighlight(InventoryHighlightAction.highlightColor, 0.5f, 0.5f, 0.1f);
					}
				}
			}
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x000625E0 File Offset: 0x000607E0
		private bool IsSuitableItem(Item item)
		{
			Identifier itemIdentifier = this.ItemIdentifier;
			if (itemIdentifier.IsEmpty && item == null)
			{
				return true;
			}
			if (item != null)
			{
				Prefab prefab = item.Prefab;
				itemIdentifier = this.ItemIdentifier;
				return prefab.Identifier == itemIdentifier || item.HasTag(this.ItemIdentifier);
			}
			return false;
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000A97 RID: 2711 RVA: 0x00062630 File Offset: 0x00060830
		// (set) Token: 0x06000A98 RID: 2712 RVA: 0x00062638 File Offset: 0x00060838
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity or entities whose inventory the item should be highlighted in. Must be a character or an item with an inventory.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x00062641 File Offset: 0x00060841
		// (set) Token: 0x06000A9A RID: 2714 RVA: 0x00062649 File Offset: 0x00060849
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item(s) to highlight.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x00062652 File Offset: 0x00060852
		// (set) Token: 0x06000A9C RID: 2716 RVA: 0x0006265A File Offset: 0x0006085A
		[Serialize(-1, IsPropertySaveable.Yes, "If the target is an item with multiple ItemContainer components (i.e. multiple inventories), such as a fabricator, this determines which inventory to highlight the item in (0 = first, 1 = second). If negative, it doesn't matter which inventory the item is in.", "", false)]
		public int ItemContainerIndex { get; set; }

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000A9D RID: 2717 RVA: 0x00062663 File Offset: 0x00060863
		// (set) Token: 0x06000A9E RID: 2718 RVA: 0x0006266B File Offset: 0x0006086B
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the action will go look through all the containers in the target inventory (e.g. highlighting a tank in a welding tool in the target inventory).", "", false)]
		public bool Recursive { get; set; }

		// Token: 0x06000A9F RID: 2719 RVA: 0x00062674 File Offset: 0x00060874
		public InventoryHighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x0006267E File Offset: 0x0006087E
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.UpdateProjSpecific();
			this.isFinished = true;
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x00062698 File Offset: 0x00060898
		private void UpdateProjSpecific()
		{
			foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
			{
				this.SetHighlight(target);
			}
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x000626F0 File Offset: 0x000608F0
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x000626F8 File Offset: 0x000608F8
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000569 RID: 1385
		private static readonly Color highlightColor = Color.Orange;

		// Token: 0x0400056E RID: 1390
		private bool isFinished;
	}
}
