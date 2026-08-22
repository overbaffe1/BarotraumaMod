using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x020001AD RID: 429
	internal class RemoveItemAction : EventAction
	{
		// Token: 0x17000928 RID: 2344
		// (get) Token: 0x06001FE9 RID: 8169 RVA: 0x000DA181 File Offset: 0x000D8381
		// (set) Token: 0x06001FEA RID: 8170 RVA: 0x000DA189 File Offset: 0x000D8389
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item(s) to remove.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000929 RID: 2345
		// (get) Token: 0x06001FEB RID: 8171 RVA: 0x000DA192 File Offset: 0x000D8392
		// (set) Token: 0x06001FEC RID: 8172 RVA: 0x000DA19A File Offset: 0x000D839A
		[Serialize("", IsPropertySaveable.Yes, "Optional list of identifiers the item(s) must have. You might for example want to go through all tagged items inside a cabinet, but only remove specific types of items.", "", false)]
		public string ItemIdentifiers { get; set; }

		// Token: 0x1700092A RID: 2346
		// (get) Token: 0x06001FED RID: 8173 RVA: 0x000DA1A3 File Offset: 0x000D83A3
		// (set) Token: 0x06001FEE RID: 8174 RVA: 0x000DA1AB File Offset: 0x000D83AB
		[Serialize(1, IsPropertySaveable.Yes, "Maximum number of items to remove.", "", false)]
		public int Amount { get; set; }

		// Token: 0x06001FEF RID: 8175 RVA: 0x000DA1B4 File Offset: 0x000D83B4
		public RemoveItemAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (string.IsNullOrEmpty(this.ItemIdentifiers))
			{
				this.ItemIdentifiers = element.GetAttributeString("itemidentifier", element.GetAttributeString("identifier", string.Empty));
			}
			this.itemIdentifierSplit = this.ItemIdentifiers.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x000DA212 File Offset: 0x000D8412
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FF1 RID: 8177 RVA: 0x000DA21A File Offset: 0x000D841A
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x000DA224 File Offset: 0x000D8424
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			bool hasValidTargets = false;
			foreach (Entity target in targets)
			{
				Character character = target as Character;
				if ((character != null && character.Inventory != null) || target is Item)
				{
					hasValidTargets = true;
					break;
				}
			}
			if (!hasValidTargets)
			{
				return;
			}
			HashSet<Item> removedItems = new HashSet<Item>();
			Func<Item, bool> <>9__0;
			foreach (Entity target2 in targets)
			{
				Character character2 = target2 as Character;
				Inventory inventory = (character2 != null) ? character2.Inventory : null;
				if (inventory != null)
				{
					while (removedItems.Count < this.Amount)
					{
						Inventory inventory2 = inventory;
						Func<Item, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((Item it) => it != null && !removedItems.Contains(it) && (this.itemIdentifierSplit.Count == 0 || this.itemIdentifierSplit.Contains(it.Prefab.Identifier))));
						}
						Item item = inventory2.FindItem(predicate, true);
						if (item == null)
						{
							break;
						}
						Entity.Spawner.AddItemToRemoveQueue(item);
						removedItems.Add(item);
					}
				}
				else
				{
					Item item2 = target2 as Item;
					if (item2 != null && (this.itemIdentifierSplit.Count == 0 || this.itemIdentifierSplit.Contains(item2.Prefab.Identifier)))
					{
						Entity.Spawner.AddItemToRemoveQueue(item2);
						removedItems.Add(item2);
						if (removedItems.Count >= this.Amount)
						{
							break;
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x04000F31 RID: 3889
		private readonly ImmutableHashSet<Identifier> itemIdentifierSplit;

		// Token: 0x04000F32 RID: 3890
		private bool isFinished;
	}
}
