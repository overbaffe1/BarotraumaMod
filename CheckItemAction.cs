using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200027E RID: 638
	internal class CheckItemAction : BinaryOptionAction
	{
		// Token: 0x17000EEF RID: 3823
		// (get) Token: 0x060038ED RID: 14573 RVA: 0x00219CCA File Offset: 0x00217ECA
		// (set) Token: 0x060038EE RID: 14574 RVA: 0x00219CD2 File Offset: 0x00217ED2
		[Serialize("", IsPropertySaveable.Yes, "Either the tag of the item(s) we want to check, or a character/container the items are inside.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000EF0 RID: 3824
		// (get) Token: 0x060038EF RID: 14575 RVA: 0x00219CDB File Offset: 0x00217EDB
		// (set) Token: 0x060038F0 RID: 14576 RVA: 0x00219CE3 File Offset: 0x00217EE3
		[Serialize("", IsPropertySaveable.Yes, "The target item must have one of these identifiers.", "", false)]
		public string ItemIdentifiers { get; set; }

		// Token: 0x17000EF1 RID: 3825
		// (get) Token: 0x060038F1 RID: 14577 RVA: 0x00219CEC File Offset: 0x00217EEC
		// (set) Token: 0x060038F2 RID: 14578 RVA: 0x00219CF4 File Offset: 0x00217EF4
		[Serialize("", IsPropertySaveable.Yes, "The target item must have at least one of these tags.", "", false)]
		public string ItemTags { get; set; }

		// Token: 0x17000EF2 RID: 3826
		// (get) Token: 0x060038F3 RID: 14579 RVA: 0x00219CFD File Offset: 0x00217EFD
		// (set) Token: 0x060038F4 RID: 14580 RVA: 0x00219D05 File Offset: 0x00217F05
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of matching items for the check to succeed.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000EF3 RID: 3827
		// (get) Token: 0x060038F5 RID: 14581 RVA: 0x00219D0E File Offset: 0x00217F0E
		// (set) Token: 0x060038F6 RID: 14582 RVA: 0x00219D16 File Offset: 0x00217F16
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of a hull the target must be inside.", "", false)]
		public Identifier HullTag { get; set; }

		// Token: 0x17000EF4 RID: 3828
		// (get) Token: 0x060038F7 RID: 14583 RVA: 0x00219D1F File Offset: 0x00217F1F
		// (set) Token: 0x060038F8 RID: 14584 RVA: 0x00219D27 File Offset: 0x00217F27
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the first target when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x17000EF5 RID: 3829
		// (get) Token: 0x060038F9 RID: 14585 RVA: 0x00219D30 File Offset: 0x00217F30
		// (set) Token: 0x060038FA RID: 14586 RVA: 0x00219D38 File Offset: 0x00217F38
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the found item(s) when the check succeeds.", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x17000EF6 RID: 3830
		// (get) Token: 0x060038FB RID: 14587 RVA: 0x00219D41 File Offset: 0x00217F41
		// (set) Token: 0x060038FC RID: 14588 RVA: 0x00219D49 File Offset: 0x00217F49
		[Serialize(false, IsPropertySaveable.Yes, "Does the item need to be equipped for the check to succeed?", "", false)]
		public bool RequireEquipped { get; set; }

		// Token: 0x17000EF7 RID: 3831
		// (get) Token: 0x060038FD RID: 14589 RVA: 0x00219D52 File Offset: 0x00217F52
		// (set) Token: 0x060038FE RID: 14590 RVA: 0x00219D5A File Offset: 0x00217F5A
		[Serialize(false, IsPropertySaveable.Yes, "Does the item need to be worn for the check to succeed?", "", false)]
		public bool RequireWorn { get; set; }

		// Token: 0x17000EF8 RID: 3832
		// (get) Token: 0x060038FF RID: 14591 RVA: 0x00219D63 File Offset: 0x00217F63
		// (set) Token: 0x06003900 RID: 14592 RVA: 0x00219D6B File Offset: 0x00217F6B
		[Serialize(true, IsPropertySaveable.Yes, "If enabled, the doesn't need to be directly inside the container/character we're checking, but can be nested inside multiple containers (e.g. in a toolbelt in a character's inventory).", "", false)]
		public bool Recursive { get; set; }

		// Token: 0x17000EF9 RID: 3833
		// (get) Token: 0x06003901 RID: 14593 RVA: 0x00219D74 File Offset: 0x00217F74
		// (set) Token: 0x06003902 RID: 14594 RVA: 0x00219D7C File Offset: 0x00217F7C
		[Serialize(-1, IsPropertySaveable.Yes, "Can be used to require the item to be in a specific ItemContainer of the target container. For example, the input slots of a fabricator (the first ItemContainer of the fabricator, with an index of 0).", "", false)]
		public int ItemContainerIndex { get; set; }

		// Token: 0x17000EFA RID: 3834
		// (get) Token: 0x06003903 RID: 14595 RVA: 0x00219D85 File Offset: 0x00217F85
		// (set) Token: 0x06003904 RID: 14596 RVA: 0x00219D8D File Offset: 0x00217F8D
		[Serialize(100f, IsPropertySaveable.Yes, "What percentage of targets do the conditionals need to match for the check to succeed?", "", false)]
		public float RequiredConditionalMatchPercentage
		{
			get
			{
				return this.requiredConditionalMatchPercentage;
			}
			set
			{
				this.requiredConditionalMatchPercentage = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000EFB RID: 3835
		// (get) Token: 0x06003905 RID: 14597 RVA: 0x00219DA5 File Offset: 0x00217FA5
		// (set) Token: 0x06003906 RID: 14598 RVA: 0x00219DAD File Offset: 0x00217FAD
		[Serialize(false, IsPropertySaveable.Yes, "When enabled, the number of matching items is compared to the number of matching items there were at the start of the round. Only valid if RequiredConditionalMatchPercentage is set.", "", false)]
		public bool CompareToInitialAmount { get; set; }

		// Token: 0x06003907 RID: 14599 RVA: 0x00219DB8 File Offset: 0x00217FB8
		public CheckItemAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.itemIdentifierSplit = this.ItemIdentifiers.ToIdentifiers(",").ToArray<Identifier>();
			this.itemTags = this.ItemTags.ToIdentifiers(",").ToArray<Identifier>();
			List<PropertyConditional> conditionalList = new List<PropertyConditional>();
			foreach (ContentXElement subElement in element.GetChildElements("conditional"))
			{
				conditionalList.AddRange(PropertyConditional.FromXElement(subElement, null));
			}
			this.conditionals = conditionalList;
			if (this.itemTags.None(null) && this.ItemIdentifiers.None(null) && this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". ");
				defaultInterpolatedStringHandler.AppendFormatted("CheckItemAction");
				defaultInterpolatedStringHandler.AppendLiteral(" does't define either tags or identifiers of the item to check.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			this.checkPercentage = (element.GetAttribute("RequiredConditionalMatchPercentage") != null);
			if (this.checkPercentage && this.conditionals.None(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(111, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\". ");
				defaultInterpolatedStringHandler2.AppendFormatted("CheckItemAction");
				defaultInterpolatedStringHandler2.AppendLiteral(" requires conditionals to be met on ");
				defaultInterpolatedStringHandler2.AppendFormatted<float>(this.requiredConditionalMatchPercentage);
				defaultInterpolatedStringHandler2.AppendLiteral("% of the targets, but there are no conditionals defined.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
			}
			if (this.Amount != 1 && this.checkPercentage)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(52, 4);
				defaultInterpolatedStringHandler3.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral("\". Cannot define both '");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.Amount);
				defaultInterpolatedStringHandler3.AppendLiteral("' and '");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(this.RequiredConditionalMatchPercentage);
				defaultInterpolatedStringHandler3.AppendLiteral("' in ");
				defaultInterpolatedStringHandler3.AppendFormatted("CheckItemAction");
				defaultInterpolatedStringHandler3.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06003908 RID: 14600 RVA: 0x0021A054 File Offset: 0x00218254
		private bool EnoughTargets(int totalTargets, int targetsWithConditionalsMatched)
		{
			if (this.checkPercentage)
			{
				if (this.CompareToInitialAmount)
				{
					totalTargets = this.ParentEvent.GetInitialTargetCount(this.TargetTag);
				}
				return MathUtils.Percentage((float)targetsWithConditionalsMatched, (float)totalTargets) >= this.RequiredConditionalMatchPercentage;
			}
			return targetsWithConditionalsMatched >= this.Amount;
		}

		// Token: 0x06003909 RID: 14601 RVA: 0x0021A0A8 File Offset: 0x002182A8
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			if (!this.HullTag.IsEmpty)
			{
				IEnumerable<Hull> hulls = this.ParentEvent.GetTargets(this.HullTag).OfType<Hull>();
				targets = targets.Where(delegate(Entity t)
				{
					Item it = t as Item;
					if (it == null || !hulls.Contains(it.CurrentHull))
					{
						Character c = t as Character;
						return c != null && hulls.Contains(c.CurrentHull);
					}
					return true;
				});
			}
			if (targets.Any<Entity>())
			{
				int targetCount = targets.Count<Entity>();
				if (targetCount >= this.Amount)
				{
					this.tempTargetItems.Clear();
					foreach (Entity target in targets)
					{
						Item item = target as Item;
						if (item != null && (this.itemTags.Any(new Func<Identifier, bool>(item.HasTag)) || this.itemIdentifierSplit.Contains(item.Prefab.Identifier) || (this.itemTags.None(null) && this.itemIdentifierSplit.None(null) && this.conditionals.Any<PropertyConditional>())) && this.ConditionalsMatch(item, null))
						{
							this.tempTargetItems.Add(item);
						}
					}
					if (this.EnoughTargets(targetCount, this.tempTargetItems.Count))
					{
						this.TryApplyTagToItems(this.tempTargetItems);
						return new bool?(true);
					}
				}
				foreach (Entity target2 in targets)
				{
					Character character = target2 as Character;
					if (character != null)
					{
						Inventory inventory = character.Inventory;
						if (this.CheckInventory(character.Inventory, character))
						{
							if (!this.ApplyTagToTarget.IsEmpty)
							{
								this.ParentEvent.AddTarget(this.ApplyTagToTarget, target2);
							}
							return new bool?(true);
						}
					}
					else
					{
						Item item2 = target2 as Item;
						if (item2 != null)
						{
							int i = 0;
							foreach (ItemContainer itemContainer in item2.GetComponents<ItemContainer>())
							{
								if ((this.ItemContainerIndex == -1 || i == this.ItemContainerIndex) && this.CheckInventory(itemContainer.Inventory, null))
								{
									if (!this.ApplyTagToTarget.IsEmpty)
									{
										this.ParentEvent.AddTarget(this.ApplyTagToTarget, target2);
									}
									return new bool?(true);
								}
								i++;
							}
						}
					}
				}
				return new bool?(false);
			}
			if (this.conditionals.Any<PropertyConditional>())
			{
				return new bool?(false);
			}
			return null;
		}

		// Token: 0x0600390A RID: 14602 RVA: 0x0021A3A4 File Offset: 0x002185A4
		private bool CheckInventory(Inventory inventory, Character character)
		{
			if (inventory == null)
			{
				return false;
			}
			int targetCount = 0;
			HashSet<Item> eventTargets = new HashSet<Item>();
			this.tempTargetItems.Clear();
			foreach (Identifier tag in this.itemTags)
			{
				foreach (Entity target in this.ParentEvent.GetTargets(tag))
				{
					Item item = target as Item;
					if (item != null)
					{
						eventTargets.Add(item);
					}
				}
			}
			Func<Item, bool> <>9__0;
			Func<Item, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((Item it) => this.itemTags.Any(new Func<Identifier, bool>(it.HasTag)) || this.itemIdentifierSplit.Contains(it.Prefab.Identifier) || eventTargets.Contains(it)));
			}
			foreach (Item item2 in inventory.FindAllItems(predicate, this.Recursive, null))
			{
				targetCount++;
				if (this.ConditionalsMatch(item2, character))
				{
					this.tempTargetItems.Add(item2);
				}
			}
			if (this.EnoughTargets(targetCount, this.tempTargetItems.Count))
			{
				this.TryApplyTagToItems(this.tempTargetItems);
				return true;
			}
			return false;
		}

		// Token: 0x0600390B RID: 14603 RVA: 0x0021A500 File Offset: 0x00218700
		private void TryApplyTagToItems(IEnumerable<Item> items)
		{
			if (!this.ApplyTagToItem.IsEmpty)
			{
				foreach (Item targetItem in items)
				{
					this.ParentEvent.AddTarget(this.ApplyTagToItem, targetItem);
				}
			}
		}

		// Token: 0x0600390C RID: 14604 RVA: 0x0021A564 File Offset: 0x00218764
		private bool ConditionalsMatch(Item item, Character character = null)
		{
			if (item == null)
			{
				return false;
			}
			foreach (PropertyConditional conditional in this.conditionals)
			{
				if (!item.ConditionalMatches(conditional))
				{
					return false;
				}
			}
			if (this.RequireEquipped)
			{
				return character != null && character.HasEquippedItem(item, null, null);
			}
			if (!this.RequireWorn)
			{
				return true;
			}
			if (character == null)
			{
				return false;
			}
			foreach (Wearable wearable in item.GetComponents<Wearable>())
			{
				foreach (InvSlotType allowedSlot in wearable.AllowedSlots)
				{
					if (allowedSlot != InvSlotType.Any && character.HasEquippedItem(item, new InvSlotType?(allowedSlot), null))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600390D RID: 14605 RVA: 0x0021A688 File Offset: 0x00218888
		public override string ToDebugString()
		{
			string[] array = new string[5];
			int num = 0;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckItemAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
			array[1] = (this.ItemTags.Any<char>() ? ("ItemTags: " + this.ItemTags.ColorizeObject() + ", ") : ("ItemIdentifiers: " + this.ItemIdentifiers.ColorizeObject() + ", "));
			array[2] = "Succeeded: ";
			array[3] = this.succeeded.ColorizeObject();
			array[4] = ")";
			return string.Concat(array);
		}

		// Token: 0x04001D80 RID: 7552
		private readonly bool checkPercentage;

		// Token: 0x04001D81 RID: 7553
		private float requiredConditionalMatchPercentage;

		// Token: 0x04001D83 RID: 7555
		private readonly IReadOnlyList<PropertyConditional> conditionals;

		// Token: 0x04001D84 RID: 7556
		private readonly Identifier[] itemIdentifierSplit;

		// Token: 0x04001D85 RID: 7557
		private readonly Identifier[] itemTags;

		// Token: 0x04001D86 RID: 7558
		private readonly List<Item> tempTargetItems = new List<Item>();
	}
}
