using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FE RID: 510
	internal class RelatedItem
	{
		// Token: 0x17000AAF RID: 2735
		// (get) Token: 0x060024B4 RID: 9396 RVA: 0x000F224C File Offset: 0x000F044C
		// (set) Token: 0x060024B5 RID: 9397 RVA: 0x000F2254 File Offset: 0x000F0454
		public bool MatchOnEmpty { get; set; }

		// Token: 0x17000AB0 RID: 2736
		// (get) Token: 0x060024B6 RID: 9398 RVA: 0x000F225D File Offset: 0x000F045D
		// (set) Token: 0x060024B7 RID: 9399 RVA: 0x000F2265 File Offset: 0x000F0465
		public bool RequireEmpty { get; set; }

		// Token: 0x17000AB1 RID: 2737
		// (get) Token: 0x060024B8 RID: 9400 RVA: 0x000F226E File Offset: 0x000F046E
		private bool RequireOrMatchOnEmpty
		{
			get
			{
				return this.MatchOnEmpty || this.RequireEmpty;
			}
		}

		// Token: 0x17000AB2 RID: 2738
		// (get) Token: 0x060024B9 RID: 9401 RVA: 0x000F2280 File Offset: 0x000F0480
		// (set) Token: 0x060024BA RID: 9402 RVA: 0x000F2288 File Offset: 0x000F0488
		public bool IgnoreInEditor { get; set; }

		// Token: 0x17000AB3 RID: 2739
		// (get) Token: 0x060024BB RID: 9403 RVA: 0x000F2291 File Offset: 0x000F0491
		// (set) Token: 0x060024BC RID: 9404 RVA: 0x000F2299 File Offset: 0x000F0499
		public ImmutableHashSet<Identifier> ExcludedIdentifiers { get; private set; }

		// Token: 0x17000AB4 RID: 2740
		// (get) Token: 0x060024BD RID: 9405 RVA: 0x000F22A2 File Offset: 0x000F04A2
		// (set) Token: 0x060024BE RID: 9406 RVA: 0x000F22AA File Offset: 0x000F04AA
		public bool ExcludeBroken { get; private set; }

		// Token: 0x17000AB5 RID: 2741
		// (get) Token: 0x060024BF RID: 9407 RVA: 0x000F22B3 File Offset: 0x000F04B3
		// (set) Token: 0x060024C0 RID: 9408 RVA: 0x000F22BB File Offset: 0x000F04BB
		public bool ExcludeFullCondition { get; private set; }

		// Token: 0x17000AB6 RID: 2742
		// (get) Token: 0x060024C1 RID: 9409 RVA: 0x000F22C4 File Offset: 0x000F04C4
		// (set) Token: 0x060024C2 RID: 9410 RVA: 0x000F22CC File Offset: 0x000F04CC
		public bool AllowVariants { get; private set; } = true;

		// Token: 0x17000AB7 RID: 2743
		// (get) Token: 0x060024C3 RID: 9411 RVA: 0x000F22D5 File Offset: 0x000F04D5
		public RelatedItem.RelationType Type
		{
			get
			{
				return this.type;
			}
		}

		// Token: 0x17000AB8 RID: 2744
		// (get) Token: 0x060024C4 RID: 9412 RVA: 0x000F22DD File Offset: 0x000F04DD
		// (set) Token: 0x060024C5 RID: 9413 RVA: 0x000F22E5 File Offset: 0x000F04E5
		public bool IsOptional { get; set; }

		// Token: 0x17000AB9 RID: 2745
		// (get) Token: 0x060024C6 RID: 9414 RVA: 0x000F22EE File Offset: 0x000F04EE
		// (set) Token: 0x060024C7 RID: 9415 RVA: 0x000F2300 File Offset: 0x000F0500
		public string JoinedIdentifiers
		{
			get
			{
				return this.Identifiers.ConvertToString(",");
			}
			set
			{
				this.Identifiers = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			}
		}

		// Token: 0x17000ABA RID: 2746
		// (get) Token: 0x060024C8 RID: 9416 RVA: 0x000F2318 File Offset: 0x000F0518
		// (set) Token: 0x060024C9 RID: 9417 RVA: 0x000F2320 File Offset: 0x000F0520
		public ImmutableHashSet<Identifier> Identifiers { get; private set; }

		// Token: 0x17000ABB RID: 2747
		// (get) Token: 0x060024CA RID: 9418 RVA: 0x000F2329 File Offset: 0x000F0529
		// (set) Token: 0x060024CB RID: 9419 RVA: 0x000F233B File Offset: 0x000F053B
		public string JoinedExcludedIdentifiers
		{
			get
			{
				return this.ExcludedIdentifiers.ConvertToString(",");
			}
			set
			{
				this.ExcludedIdentifiers = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			}
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x000F2354 File Offset: 0x000F0554
		public bool MatchesItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (this.ExcludedIdentifiers.Contains(item.Prefab.Identifier))
			{
				return false;
			}
			foreach (Identifier excludedIdentifier in this.ExcludedIdentifiers)
			{
				if (item.HasTag(excludedIdentifier))
				{
					return false;
				}
			}
			Inventory parentInventory = item.ParentInventory;
			Character character = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
			if (character != null && this.CharacterInventorySlotType != InvSlotType.None && !character.HasEquippedItem(item, new InvSlotType?(this.CharacterInventorySlotType), null))
			{
				return false;
			}
			if (this.Identifiers.Contains(item.Prefab.Identifier))
			{
				return true;
			}
			foreach (Identifier identifier in this.Identifiers)
			{
				if (item.HasTag(identifier))
				{
					return true;
				}
			}
			return this.AllowVariants && !item.Prefab.VariantOf.IsEmpty && this.Identifiers.Contains(item.Prefab.VariantOf);
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x000F24AC File Offset: 0x000F06AC
		public bool MatchesItem(ItemPrefab itemPrefab)
		{
			if (itemPrefab == null)
			{
				return false;
			}
			if (this.ExcludedIdentifiers.Contains(itemPrefab.Identifier))
			{
				return false;
			}
			foreach (Identifier excludedIdentifier in this.ExcludedIdentifiers)
			{
				if (itemPrefab.Tags.Contains(excludedIdentifier))
				{
					return false;
				}
			}
			if (this.Identifiers.Contains(itemPrefab.Identifier))
			{
				return true;
			}
			foreach (Identifier identifier in this.Identifiers)
			{
				if (itemPrefab.Tags.Contains(identifier))
				{
					return true;
				}
			}
			return this.AllowVariants && !itemPrefab.VariantOf.IsEmpty && this.Identifiers.Contains(itemPrefab.VariantOf);
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x000F25BC File Offset: 0x000F07BC
		public RelatedItem(Identifier[] identifiers, Identifier[] excludedIdentifiers)
		{
			this.Identifiers = (from id in identifiers
			select id.Value.Trim().ToIdentifier()).ToImmutableHashSet<Identifier>();
			this.ExcludedIdentifiers = (from id in excludedIdentifiers
			select id.Value.Trim().ToIdentifier()).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x000F2648 File Offset: 0x000F0848
		public RelatedItem(ContentXElement element, string parentDebugName)
		{
			Identifier[] identifiers;
			if (element.GetAttribute("name") != null)
			{
				DebugConsole.ThrowError("Error in RelatedItem config (" + (string.IsNullOrEmpty(parentDebugName) ? element.ToString() : parentDebugName) + ") - use item tags or identifiers instead of names.", null, element.ContentPackage, false, false);
				Identifier[] itemNames = element.GetAttributeIdentifierArray("name", Array.Empty<Identifier>(), true);
				List<Identifier> convertedIdentifiers = new List<Identifier>();
				Identifier[] array = itemNames;
				for (int i = 0; i < array.Length; i++)
				{
					Identifier itemName = array[i];
					ItemPrefab matchingItem = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Name == itemName.Value);
					if (matchingItem != null)
					{
						convertedIdentifiers.Add(matchingItem.Identifier);
					}
					else
					{
						convertedIdentifiers.Add(itemName);
					}
				}
				identifiers = convertedIdentifiers.ToArray();
			}
			else
			{
				identifiers = (element.GetAttributeIdentifierArray("items", null, true) ?? element.GetAttributeIdentifierArray("item", null, true));
				if (identifiers == null)
				{
					identifiers = (element.GetAttributeIdentifierArray("identifiers", null, true) ?? element.GetAttributeIdentifierArray("tags", null, true));
					if (identifiers == null)
					{
						identifiers = (element.GetAttributeIdentifierArray("identifier", null, true) ?? element.GetAttributeIdentifierArray("tag", Array.Empty<Identifier>(), true));
					}
				}
			}
			this.Identifiers = identifiers.ToImmutableHashSet<Identifier>();
			Identifier[] excludedIdentifiers = element.GetAttributeIdentifierArray("excludeditems", null, true) ?? element.GetAttributeIdentifierArray("excludeditem", null, true);
			if (excludedIdentifiers == null)
			{
				excludedIdentifiers = (element.GetAttributeIdentifierArray("excludedidentifiers", null, true) ?? element.GetAttributeIdentifierArray("excludedtags", null, true));
				if (excludedIdentifiers == null)
				{
					excludedIdentifiers = (element.GetAttributeIdentifierArray("excludedidentifier", null, true) ?? element.GetAttributeIdentifierArray("excludedtag", Array.Empty<Identifier>(), true));
				}
			}
			this.ExcludedIdentifiers = excludedIdentifiers.ToImmutableHashSet<Identifier>();
			this.ExcludeBroken = element.GetAttributeBool("excludebroken", true);
			this.RequireEmpty = element.GetAttributeBool("requireempty", false);
			this.ExcludeFullCondition = element.GetAttributeBool("excludefullcondition", false);
			this.AllowVariants = element.GetAttributeBool("allowvariants", true);
			this.Rotation = element.GetAttributeFloat("rotation", 0f);
			this.SetActive = element.GetAttributeBool("setactive", false);
			this.BlameEquipperForDeath = element.GetAttributeBool("BlameEquipperForDeath", false);
			string key = "CharacterInventorySlotType";
			InvSlotType invSlotType = InvSlotType.None;
			this.CharacterInventorySlotType = element.GetAttributeEnum<InvSlotType>(key, invSlotType);
			if (element.GetAttribute("Hide") != null)
			{
				this.Hide = element.GetAttributeBool("Hide", false);
			}
			if (element.GetAttribute("ItemPos") != null)
			{
				string key2 = "ItemPos";
				Vector2 zero = Vector2.Zero;
				this.ItemPos = new Vector2?(element.GetAttributeVector2(key2, zero));
			}
			string typeStr = element.GetAttributeString("type", "");
			if (string.IsNullOrEmpty(typeStr))
			{
				string a = element.Name.ToString().ToLowerInvariant();
				if (!(a == "containable"))
				{
					if (a == "suitablefertilizer" || a == "suitableseed")
					{
						typeStr = "None";
					}
				}
				else
				{
					typeStr = "Contained";
				}
			}
			if (!Enum.TryParse<RelatedItem.RelationType>(typeStr, true, out this.type))
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Error in RelatedItem config (",
					parentDebugName,
					") - \"",
					typeStr,
					"\" is not a valid relation type."
				}), null, element.ContentPackage, false, false);
				this.type = RelatedItem.RelationType.Invalid;
			}
			this.MsgTag = element.GetAttributeIdentifier("msg", Identifier.Empty);
			LocalizedString msg = TextManager.Get(this.MsgTag);
			if (!msg.Loaded)
			{
				this.Msg = this.MsgTag.Value;
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("statuseffect", StringComparison.OrdinalIgnoreCase))
				{
					this.StatusEffects.Add(StatusEffect.Load(subElement, parentDebugName));
				}
			}
			this.IsOptional = element.GetAttributeBool("optional", false);
			this.IgnoreInEditor = element.GetAttributeBool("ignoreineditor", false);
			this.MatchOnEmpty = element.GetAttributeBool("matchonempty", false);
			this.TargetSlot = element.GetAttributeInt("targetslot", -1);
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x000F2AB0 File Offset: 0x000F0CB0
		public bool CheckRequirements(Character character, Item parentItem)
		{
			switch (this.type)
			{
			case RelatedItem.RelationType.Contained:
				return parentItem != null && this.CheckContained(parentItem);
			case RelatedItem.RelationType.Equipped:
				if (character == null)
				{
					return false;
				}
				foreach (Item item in character.Inventory.AllItemsMod)
				{
					if (character.HasEquippedItem(item, null, null) && RelatedItem.<CheckRequirements>g__CheckItem|62_0(item, this))
					{
						if (this.RequireEmpty && item.Condition > 0f)
						{
							return false;
						}
						return true;
					}
				}
				return this.RequireOrMatchOnEmpty;
			case RelatedItem.RelationType.Picked:
			{
				if (character == null)
				{
					return false;
				}
				if (character.Inventory == null)
				{
					return this.MatchOnEmpty || this.RequireEmpty;
				}
				IEnumerable<Item> allItems = (this.TargetSlot == -1) ? character.Inventory.AllItems : character.Inventory.GetItemsAt(this.TargetSlot);
				if (this.RequireOrMatchOnEmpty && allItems.None(null))
				{
					return true;
				}
				using (IEnumerator<Item> enumerator2 = allItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item pickedItem = enumerator2.Current;
						if (pickedItem != null && RelatedItem.<CheckRequirements>g__CheckItem|62_0(pickedItem, this))
						{
							if (this.RequireEmpty && pickedItem.Condition > 0f)
							{
								return false;
							}
							return true;
						}
					}
					return false;
				}
				break;
			}
			case RelatedItem.RelationType.Container:
				if (parentItem == null || parentItem.Container == null)
				{
					return this.MatchOnEmpty || this.RequireEmpty;
				}
				return RelatedItem.<CheckRequirements>g__CheckItem|62_0(parentItem.Container, this);
			}
			return true;
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x000F2C68 File Offset: 0x000F0E68
		private bool CheckContained(Item parentItem)
		{
			if (parentItem.OwnInventory == null)
			{
				return false;
			}
			if (this.TargetSlot == -1 && this.RequireOrMatchOnEmpty)
			{
				bool isEmpty = parentItem.OwnInventory.IsEmpty();
				if (this.RequireEmpty)
				{
					return isEmpty;
				}
				if (this.MatchOnEmpty && isEmpty)
				{
					return true;
				}
			}
			foreach (ItemContainer container in parentItem.GetComponents<ItemContainer>())
			{
				if (this.TargetSlot > -1 && this.RequireOrMatchOnEmpty)
				{
					Item itemInSlot = container.Inventory.GetItemAt(this.TargetSlot);
					if (this.RequireEmpty)
					{
						return itemInSlot == null;
					}
					if (this.MatchOnEmpty && itemInSlot == null)
					{
						return true;
					}
				}
				foreach (Item contained in container.Inventory.AllItems)
				{
					if (this.TargetSlot <= -1 || container.Inventory.FindIndex(contained) == this.TargetSlot)
					{
						if ((!this.ExcludeBroken || contained.Condition > 0f) && (!this.ExcludeFullCondition || !contained.IsFullCondition) && this.MatchesItem(contained))
						{
							return true;
						}
						if (this.CheckContained(contained))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x000F2DE4 File Offset: 0x000F0FE4
		public void Save(XElement element)
		{
			element.Add(new object[]
			{
				new XAttribute("items", this.JoinedIdentifiers),
				new XAttribute("type", this.type.ToString()),
				new XAttribute("characterinventoryslottype", this.CharacterInventorySlotType.ToString()),
				new XAttribute("optional", this.IsOptional),
				new XAttribute("ignoreineditor", this.IgnoreInEditor),
				new XAttribute("excludebroken", this.ExcludeBroken),
				new XAttribute("requireempty", this.RequireEmpty),
				new XAttribute("excludefullcondition", this.ExcludeFullCondition),
				new XAttribute("targetslot", this.TargetSlot),
				new XAttribute("allowvariants", this.AllowVariants),
				new XAttribute("rotation", this.Rotation),
				new XAttribute("setactive", this.SetActive)
			});
			if (this.Hide)
			{
				element.Add(new XAttribute("Hide", true));
			}
			if (this.ItemPos != null)
			{
				element.Add(new XAttribute("ItemPos", this.ItemPos.Value));
			}
			if (this.ExcludedIdentifiers.Count > 0)
			{
				element.Add(new XAttribute("excludedidentifiers", this.JoinedExcludedIdentifiers));
			}
			if (!this.Msg.IsNullOrWhiteSpace())
			{
				element.Add(new XAttribute("msg", this.MsgTag.IsEmpty ? this.Msg : this.MsgTag.Value));
			}
		}

		// Token: 0x060024D3 RID: 9427 RVA: 0x000F302C File Offset: 0x000F122C
		public static RelatedItem Load(ContentXElement element, bool returnEmpty, string parentDebugName)
		{
			RelatedItem ri = new RelatedItem(element, parentDebugName);
			if (ri.Type == RelatedItem.RelationType.Invalid)
			{
				return null;
			}
			if (ri.Identifiers.None(null) && ri.ExcludedIdentifiers.None(null) && !returnEmpty)
			{
				return null;
			}
			return ri;
		}

		// Token: 0x060024D4 RID: 9428 RVA: 0x000F306E File Offset: 0x000F126E
		[CompilerGenerated]
		internal static bool <CheckRequirements>g__CheckItem|62_0(Item i, RelatedItem ri)
		{
			return (!ri.ExcludeBroken || ri.RequireEmpty || i.Condition > 0f) && (!ri.ExcludeFullCondition || !i.IsFullCondition) && ri.MatchesItem(i);
		}

		// Token: 0x04001225 RID: 4645
		private readonly RelatedItem.RelationType type;

		// Token: 0x04001226 RID: 4646
		public List<StatusEffect> StatusEffects = new List<StatusEffect>();

		// Token: 0x04001227 RID: 4647
		public LocalizedString Msg;

		// Token: 0x04001228 RID: 4648
		public Identifier MsgTag;

		// Token: 0x0400122C RID: 4652
		public int TargetSlot = -1;

		// Token: 0x0400122D RID: 4653
		public InvSlotType CharacterInventorySlotType;

		// Token: 0x0400122E RID: 4654
		public Vector2? ItemPos;

		// Token: 0x0400122F RID: 4655
		public bool Hide;

		// Token: 0x04001230 RID: 4656
		public float Rotation;

		// Token: 0x04001231 RID: 4657
		public bool SetActive;

		// Token: 0x04001232 RID: 4658
		public bool BlameEquipperForDeath;

		// Token: 0x020009C0 RID: 2496
		public enum RelationType
		{
			// Token: 0x04003444 RID: 13380
			None,
			// Token: 0x04003445 RID: 13381
			Contained,
			// Token: 0x04003446 RID: 13382
			Equipped,
			// Token: 0x04003447 RID: 13383
			Picked,
			// Token: 0x04003448 RID: 13384
			Container,
			// Token: 0x04003449 RID: 13385
			Invalid
		}
	}
}
