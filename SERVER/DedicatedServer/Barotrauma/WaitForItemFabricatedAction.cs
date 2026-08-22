using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001BE RID: 446
	[NullableContext(1)]
	[Nullable(0)]
	internal class WaitForItemFabricatedAction : EventAction
	{
		// Token: 0x1700097A RID: 2426
		// (get) Token: 0x06002123 RID: 8483 RVA: 0x000DE94D File Offset: 0x000DCB4D
		// (set) Token: 0x06002124 RID: 8484 RVA: 0x000DE955 File Offset: 0x000DCB55
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character who must fabricate the item. If empty, it doesn't matter who fabricates it.", "", false)]
		public Identifier CharacterTag { get; set; }

		// Token: 0x1700097B RID: 2427
		// (get) Token: 0x06002125 RID: 8485 RVA: 0x000DE95E File Offset: 0x000DCB5E
		// (set) Token: 0x06002126 RID: 8486 RVA: 0x000DE966 File Offset: 0x000DCB66
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item that must be fabricated. Optional if ItemTag is set.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x1700097C RID: 2428
		// (get) Token: 0x06002127 RID: 8487 RVA: 0x000DE96F File Offset: 0x000DCB6F
		// (set) Token: 0x06002128 RID: 8488 RVA: 0x000DE977 File Offset: 0x000DCB77
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must be fabricated. Optional if ItemIdentifier is set.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x1700097D RID: 2429
		// (get) Token: 0x06002129 RID: 8489 RVA: 0x000DE980 File Offset: 0x000DCB80
		// (set) Token: 0x0600212A RID: 8490 RVA: 0x000DE988 File Offset: 0x000DCB88
		[Serialize(1, IsPropertySaveable.Yes, "Number of items that need to be fabricated.", "", false)]
		public int Amount { get; set; }

		// Token: 0x1700097E RID: 2430
		// (get) Token: 0x0600212B RID: 8491 RVA: 0x000DE991 File Offset: 0x000DCB91
		// (set) Token: 0x0600212C RID: 8492 RVA: 0x000DE999 File Offset: 0x000DCB99
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the fabricated item(s).", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x0600212D RID: 8493 RVA: 0x000DE9A4 File Offset: 0x000DCBA4
		public WaitForItemFabricatedAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.ItemTag.IsEmpty && this.ItemIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". ");
				defaultInterpolatedStringHandler.AppendFormatted("WaitForItemFabricatedAction");
				defaultInterpolatedStringHandler.AppendLiteral(" does't define either a tag or an identifier of the item to check.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			foreach (Item item in Item.ItemList)
			{
				Fabricator fabricator = item.GetComponent<Fabricator>();
				if (fabricator != null)
				{
					Fabricator fabricator2 = fabricator;
					fabricator2.OnItemFabricated = (Action<Item, Character>)Delegate.Combine(fabricator2.OnItemFabricated, new Action<Item, Character>(this.OnItemFabricated));
				}
			}
		}

		// Token: 0x0600212E RID: 8494 RVA: 0x000DEAAC File Offset: 0x000DCCAC
		public void OnItemFabricated(Item item, Character character)
		{
			if (item == null)
			{
				return;
			}
			Identifier identifier = this.CharacterTag;
			if (!identifier.IsEmpty && !this.ParentEvent.GetTargets(this.CharacterTag).Contains(character))
			{
				return;
			}
			identifier = this.ItemIdentifier;
			if (!identifier.IsEmpty)
			{
				Prefab prefab = item.Prefab;
				identifier = this.ItemIdentifier;
				if (prefab.Identifier == identifier)
				{
					goto IL_77;
				}
			}
			if (this.ItemTag.IsEmpty || !item.HasTag(this.ItemTag))
			{
				return;
			}
			IL_77:
			identifier = this.ApplyTagToItem;
			if (!identifier.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyTagToItem, item);
			}
			this.counter++;
		}

		// Token: 0x0600212F RID: 8495 RVA: 0x000DEB60 File Offset: 0x000DCD60
		public override bool IsFinished(ref string goTo)
		{
			return this.counter >= this.Amount;
		}

		// Token: 0x06002130 RID: 8496 RVA: 0x000DEB73 File Offset: 0x000DCD73
		public override void Reset()
		{
			this.counter = 0;
		}

		// Token: 0x06002131 RID: 8497 RVA: 0x000DEB7C File Offset: 0x000DCD7C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.counter >= this.Amount, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("WaitForItemFabricatedAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemTag);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.counter);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Amount);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000FA3 RID: 4003
		private int counter;
	}
}
