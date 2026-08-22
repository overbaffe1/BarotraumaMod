using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020002B0 RID: 688
	[NullableContext(1)]
	[Nullable(0)]
	internal class WaitForItemFabricatedAction : EventAction
	{
		// Token: 0x17000FB9 RID: 4025
		// (get) Token: 0x06003BC1 RID: 15297 RVA: 0x002247E1 File Offset: 0x002229E1
		// (set) Token: 0x06003BC2 RID: 15298 RVA: 0x002247E9 File Offset: 0x002229E9
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character who must fabricate the item. If empty, it doesn't matter who fabricates it.", "", false)]
		public Identifier CharacterTag { get; set; }

		// Token: 0x17000FBA RID: 4026
		// (get) Token: 0x06003BC3 RID: 15299 RVA: 0x002247F2 File Offset: 0x002229F2
		// (set) Token: 0x06003BC4 RID: 15300 RVA: 0x002247FA File Offset: 0x002229FA
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item that must be fabricated. Optional if ItemTag is set.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x17000FBB RID: 4027
		// (get) Token: 0x06003BC5 RID: 15301 RVA: 0x00224803 File Offset: 0x00222A03
		// (set) Token: 0x06003BC6 RID: 15302 RVA: 0x0022480B File Offset: 0x00222A0B
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must be fabricated. Optional if ItemIdentifier is set.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000FBC RID: 4028
		// (get) Token: 0x06003BC7 RID: 15303 RVA: 0x00224814 File Offset: 0x00222A14
		// (set) Token: 0x06003BC8 RID: 15304 RVA: 0x0022481C File Offset: 0x00222A1C
		[Serialize(1, IsPropertySaveable.Yes, "Number of items that need to be fabricated.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000FBD RID: 4029
		// (get) Token: 0x06003BC9 RID: 15305 RVA: 0x00224825 File Offset: 0x00222A25
		// (set) Token: 0x06003BCA RID: 15306 RVA: 0x0022482D File Offset: 0x00222A2D
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the fabricated item(s).", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x06003BCB RID: 15307 RVA: 0x00224838 File Offset: 0x00222A38
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

		// Token: 0x06003BCC RID: 15308 RVA: 0x00224940 File Offset: 0x00222B40
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

		// Token: 0x06003BCD RID: 15309 RVA: 0x002249F4 File Offset: 0x00222BF4
		public override bool IsFinished(ref string goTo)
		{
			return this.counter >= this.Amount;
		}

		// Token: 0x06003BCE RID: 15310 RVA: 0x00224A07 File Offset: 0x00222C07
		public override void Reset()
		{
			this.counter = 0;
		}

		// Token: 0x06003BCF RID: 15311 RVA: 0x00224A10 File Offset: 0x00222C10
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

		// Token: 0x04001E87 RID: 7815
		private int counter;
	}
}
