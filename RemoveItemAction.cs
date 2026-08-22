using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma
{
	// Token: 0x0200029F RID: 671
	internal class RemoveItemAction : EventAction
	{
		// Token: 0x17000F6E RID: 3950
		// (get) Token: 0x06003A95 RID: 14997 RVA: 0x0021FC9D File Offset: 0x0021DE9D
		// (set) Token: 0x06003A96 RID: 14998 RVA: 0x0021FCA5 File Offset: 0x0021DEA5
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item(s) to remove.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F6F RID: 3951
		// (get) Token: 0x06003A97 RID: 14999 RVA: 0x0021FCAE File Offset: 0x0021DEAE
		// (set) Token: 0x06003A98 RID: 15000 RVA: 0x0021FCB6 File Offset: 0x0021DEB6
		[Serialize("", IsPropertySaveable.Yes, "Optional list of identifiers the item(s) must have. You might for example want to go through all tagged items inside a cabinet, but only remove specific types of items.", "", false)]
		public string ItemIdentifiers { get; set; }

		// Token: 0x17000F70 RID: 3952
		// (get) Token: 0x06003A99 RID: 15001 RVA: 0x0021FCBF File Offset: 0x0021DEBF
		// (set) Token: 0x06003A9A RID: 15002 RVA: 0x0021FCC7 File Offset: 0x0021DEC7
		[Serialize(1, IsPropertySaveable.Yes, "Maximum number of items to remove.", "", false)]
		public int Amount { get; set; }

		// Token: 0x06003A9B RID: 15003 RVA: 0x0021FCD0 File Offset: 0x0021DED0
		public RemoveItemAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (string.IsNullOrEmpty(this.ItemIdentifiers))
			{
				this.ItemIdentifiers = element.GetAttributeString("itemidentifier", element.GetAttributeString("identifier", string.Empty));
			}
			this.itemIdentifierSplit = this.ItemIdentifiers.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06003A9C RID: 15004 RVA: 0x0021FD2E File Offset: 0x0021DF2E
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A9D RID: 15005 RVA: 0x0021FD36 File Offset: 0x0021DF36
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A9E RID: 15006 RVA: 0x0021FD40 File Offset: 0x0021DF40
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

		// Token: 0x04001E1A RID: 7706
		private readonly ImmutableHashSet<Identifier> itemIdentifierSplit;

		// Token: 0x04001E1B RID: 7707
		private bool isFinished;
	}
}
