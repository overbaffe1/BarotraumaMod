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
	// Token: 0x020002E7 RID: 743
	internal class RelatedItem
	{
		// Token: 0x17001047 RID: 4167
		// (get) Token: 0x06003D92 RID: 15762 RVA: 0x0022F788 File Offset: 0x0022D988
		// (set) Token: 0x06003D93 RID: 15763 RVA: 0x0022F790 File Offset: 0x0022D990
		public bool MatchOnEmpty { get; set; }

		// Token: 0x17001048 RID: 4168
		// (get) Token: 0x06003D94 RID: 15764 RVA: 0x0022F799 File Offset: 0x0022D999
		// (set) Token: 0x06003D95 RID: 15765 RVA: 0x0022F7A1 File Offset: 0x0022D9A1
		public bool RequireEmpty { get; set; }

		// Token: 0x17001049 RID: 4169
		// (get) Token: 0x06003D96 RID: 15766 RVA: 0x0022F7AA File Offset: 0x0022D9AA
		private bool RequireOrMatchOnEmpty
		{
			get
			{
				return this.MatchOnEmpty || this.RequireEmpty;
			}
		}

		// Token: 0x1700104A RID: 4170
		// (get) Token: 0x06003D97 RID: 15767 RVA: 0x0022F7BC File Offset: 0x0022D9BC
		// (set) Token: 0x06003D98 RID: 15768 RVA: 0x0022F7C4 File Offset: 0x0022D9C4
		public bool IgnoreInEditor { get; set; }

		// Token: 0x1700104B RID: 4171
		// (get) Token: 0x06003D99 RID: 15769 RVA: 0x0022F7CD File Offset: 0x0022D9CD
		// (set) Token: 0x06003D9A RID: 15770 RVA: 0x0022F7D5 File Offset: 0x0022D9D5
		public ImmutableHashSet<Identifier> ExcludedIdentifiers { get; private set; }

		// Token: 0x1700104C RID: 4172
		// (get) Token: 0x06003D9B RID: 15771 RVA: 0x0022F7DE File Offset: 0x0022D9DE
		// (set) Token: 0x06003D9C RID: 15772 RVA: 0x0022F7E6 File Offset: 0x0022D9E6
		public bool ExcludeBroken { get; private set; }

		// Token: 0x1700104D RID: 4173
		// (get) Token: 0x06003D9D RID: 15773 RVA: 0x0022F7EF File Offset: 0x0022D9EF
		// (set) Token: 0x06003D9E RID: 15774 RVA: 0x0022F7F7 File Offset: 0x0022D9F7
		public bool ExcludeFullCondition { get; private set; }

		// Token: 0x1700104E RID: 4174
		// (get) Token: 0x06003D9F RID: 15775 RVA: 0x0022F800 File Offset: 0x0022DA00
		// (set) Token: 0x06003DA0 RID: 15776 RVA: 0x0022F808 File Offset: 0x0022DA08
		public bool AllowVariants { get; private set; } = true;

		// Token: 0x1700104F RID: 4175
		// (get) Token: 0x06003DA1 RID: 15777 RVA: 0x0022F811 File Offset: 0x0022DA11
		public RelatedItem.RelationType Type
		{
			get
			{
				return this.type;
			}
		}

		// Token: 0x17001050 RID: 4176
		// (get) Token: 0x06003DA2 RID: 15778 RVA: 0x0022F819 File Offset: 0x0022DA19
		// (set) Token: 0x06003DA3 RID: 15779 RVA: 0x0022F821 File Offset: 0x0022DA21
		public bool IsOptional { get; set; }

		// Token: 0x17001051 RID: 4177
		// (get) Token: 0x06003DA4 RID: 15780 RVA: 0x0022F82A File Offset: 0x0022DA2A
		// (set) Token: 0x06003DA5 RID: 15781 RVA: 0x0022F83C File Offset: 0x0022DA3C
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

		// Token: 0x17001052 RID: 4178
		// (get) Token: 0x06003DA6 RID: 15782 RVA: 0x0022F854 File Offset: 0x0022DA54
		// (set) Token: 0x06003DA7 RID: 15783 RVA: 0x0022F85C File Offset: 0x0022DA5C
		public ImmutableHashSet<Identifier> Identifiers { get; private set; }

		// Token: 0x17001053 RID: 4179
		// (get) Token: 0x06003DA8 RID: 15784 RVA: 0x0022F865 File Offset: 0x0022DA65
		// (set) Token: 0x06003DA9 RID: 15785 RVA: 0x0022F877 File Offset: 0x0022DA77
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

		// Token: 0x06003DAA RID: 15786 RVA: 0x0022F890 File Offset: 0x0022DA90
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

		// Token: 0x06003DAB RID: 15787 RVA: 0x0022F9E8 File Offset: 0x0022DBE8
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

		// Token: 0x06003DAC RID: 15788 RVA: 0x0022FAF8 File Offset: 0x0022DCF8
		public RelatedItem(Identifier[] identifiers, Identifier[] excludedIdentifiers)
		{
			this.Identifiers = (from id in identifiers
			select id.Value.Trim().ToIdentifier()).ToImmutableHashSet<Identifier>();
			this.ExcludedIdentifiers = (from id in excludedIdentifiers
			select id.Value.Trim().ToIdentifier()).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06003DAD RID: 15789 RVA: 0x0022FB84 File Offset: 0x0022DD84
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
			else
			{
				foreach (object obj in Enum.GetValues(typeof(InputType)))
				{
					InputType inputType = (InputType)obj;
					string inputTag = "[" + inputType.ToString().ToLowerInvariant() + "]";
					if (msg.Contains(inputTag, StringComparison.Ordinal))
					{
						LocalizedString localizedString = msg;
						string find = inputTag;
						GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
						msg = localizedString.Replace(find, keyMap.KeyBindText(inputType), StringComparison.Ordinal);
					}
				}
				this.Msg = msg;
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

		// Token: 0x06003DAE RID: 15790 RVA: 0x00230098 File Offset: 0x0022E298
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

		// Token: 0x06003DAF RID: 15791 RVA: 0x00230250 File Offset: 0x0022E450
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

		// Token: 0x06003DB0 RID: 15792 RVA: 0x002303CC File Offset: 0x0022E5CC
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

		// Token: 0x06003DB1 RID: 15793 RVA: 0x00230614 File Offset: 0x0022E814
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

		// Token: 0x06003DB2 RID: 15794 RVA: 0x00230656 File Offset: 0x0022E856
		[CompilerGenerated]
		internal static bool <CheckRequirements>g__CheckItem|62_0(Item i, RelatedItem ri)
		{
			return (!ri.ExcludeBroken || ri.RequireEmpty || i.Condition > 0f) && (!ri.ExcludeFullCondition || !i.IsFullCondition) && ri.MatchesItem(i);
		}

		// Token: 0x04002051 RID: 8273
		private readonly RelatedItem.RelationType type;

		// Token: 0x04002052 RID: 8274
		public List<StatusEffect> StatusEffects = new List<StatusEffect>();

		// Token: 0x04002053 RID: 8275
		public LocalizedString Msg;

		// Token: 0x04002054 RID: 8276
		public Identifier MsgTag;

		// Token: 0x04002058 RID: 8280
		public int TargetSlot = -1;

		// Token: 0x04002059 RID: 8281
		public InvSlotType CharacterInventorySlotType;

		// Token: 0x0400205A RID: 8282
		public Vector2? ItemPos;

		// Token: 0x0400205B RID: 8283
		public bool Hide;

		// Token: 0x0400205C RID: 8284
		public float Rotation;

		// Token: 0x0400205D RID: 8285
		public bool SetActive;

		// Token: 0x0400205E RID: 8286
		public bool BlameEquipperForDeath;

		// Token: 0x02000F8F RID: 3983
		public enum RelationType
		{
			// Token: 0x04005605 RID: 22021
			None,
			// Token: 0x04005606 RID: 22022
			Contained,
			// Token: 0x04005607 RID: 22023
			Equipped,
			// Token: 0x04005608 RID: 22024
			Picked,
			// Token: 0x04005609 RID: 22025
			Container,
			// Token: 0x0400560A RID: 22026
			Invalid
		}
	}
}
