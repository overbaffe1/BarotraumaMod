using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200018A RID: 394
	internal class CheckItemAction : BinaryOptionAction
	{
		// Token: 0x1700089B RID: 2203
		// (get) Token: 0x06001E21 RID: 7713 RVA: 0x000D4716 File Offset: 0x000D2916
		// (set) Token: 0x06001E22 RID: 7714 RVA: 0x000D471E File Offset: 0x000D291E
		[Serialize("", IsPropertySaveable.Yes, "Either the tag of the item(s) we want to check, or a character/container the items are inside.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x1700089C RID: 2204
		// (get) Token: 0x06001E23 RID: 7715 RVA: 0x000D4727 File Offset: 0x000D2927
		// (set) Token: 0x06001E24 RID: 7716 RVA: 0x000D472F File Offset: 0x000D292F
		[Serialize("", IsPropertySaveable.Yes, "The target item must have one of these identifiers.", "", false)]
		public string ItemIdentifiers { get; set; }

		// Token: 0x1700089D RID: 2205
		// (get) Token: 0x06001E25 RID: 7717 RVA: 0x000D4738 File Offset: 0x000D2938
		// (set) Token: 0x06001E26 RID: 7718 RVA: 0x000D4740 File Offset: 0x000D2940
		[Serialize("", IsPropertySaveable.Yes, "The target item must have at least one of these tags.", "", false)]
		public string ItemTags { get; set; }

		// Token: 0x1700089E RID: 2206
		// (get) Token: 0x06001E27 RID: 7719 RVA: 0x000D4749 File Offset: 0x000D2949
		// (set) Token: 0x06001E28 RID: 7720 RVA: 0x000D4751 File Offset: 0x000D2951
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of matching items for the check to succeed.", "", false)]
		public int Amount { get; set; }

		// Token: 0x1700089F RID: 2207
		// (get) Token: 0x06001E29 RID: 7721 RVA: 0x000D475A File Offset: 0x000D295A
		// (set) Token: 0x06001E2A RID: 7722 RVA: 0x000D4762 File Offset: 0x000D2962
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of a hull the target must be inside.", "", false)]
		public Identifier HullTag { get; set; }

		// Token: 0x170008A0 RID: 2208
		// (get) Token: 0x06001E2B RID: 7723 RVA: 0x000D476B File Offset: 0x000D296B
		// (set) Token: 0x06001E2C RID: 7724 RVA: 0x000D4773 File Offset: 0x000D2973
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the first target when the check succeeds.", "", false)]
		public Identifier ApplyTagToTarget { get; set; }

		// Token: 0x170008A1 RID: 2209
		// (get) Token: 0x06001E2D RID: 7725 RVA: 0x000D477C File Offset: 0x000D297C
		// (set) Token: 0x06001E2E RID: 7726 RVA: 0x000D4784 File Offset: 0x000D2984
		[Serialize("", IsPropertySaveable.Yes, "Tag to apply to the found item(s) when the check succeeds.", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x170008A2 RID: 2210
		// (get) Token: 0x06001E2F RID: 7727 RVA: 0x000D478D File Offset: 0x000D298D
		// (set) Token: 0x06001E30 RID: 7728 RVA: 0x000D4795 File Offset: 0x000D2995
		[Serialize(false, IsPropertySaveable.Yes, "Does the item need to be equipped for the check to succeed?", "", false)]
		public bool RequireEquipped { get; set; }

		// Token: 0x170008A3 RID: 2211
		// (get) Token: 0x06001E31 RID: 7729 RVA: 0x000D479E File Offset: 0x000D299E
		// (set) Token: 0x06001E32 RID: 7730 RVA: 0x000D47A6 File Offset: 0x000D29A6
		[Serialize(false, IsPropertySaveable.Yes, "Does the item need to be worn for the check to succeed?", "", false)]
		public bool RequireWorn { get; set; }

		// Token: 0x170008A4 RID: 2212
		// (get) Token: 0x06001E33 RID: 7731 RVA: 0x000D47AF File Offset: 0x000D29AF
		// (set) Token: 0x06001E34 RID: 7732 RVA: 0x000D47B7 File Offset: 0x000D29B7
		[Serialize(true, IsPropertySaveable.Yes, "If enabled, the doesn't need to be directly inside the container/character we're checking, but can be nested inside multiple containers (e.g. in a toolbelt in a character's inventory).", "", false)]
		public bool Recursive { get; set; }

		// Token: 0x170008A5 RID: 2213
		// (get) Token: 0x06001E35 RID: 7733 RVA: 0x000D47C0 File Offset: 0x000D29C0
		// (set) Token: 0x06001E36 RID: 7734 RVA: 0x000D47C8 File Offset: 0x000D29C8
		[Serialize(-1, IsPropertySaveable.Yes, "Can be used to require the item to be in a specific ItemContainer of the target container. For example, the input slots of a fabricator (the first ItemContainer of the fabricator, with an index of 0).", "", false)]
		public int ItemContainerIndex { get; set; }

		// Token: 0x170008A6 RID: 2214
		// (get) Token: 0x06001E37 RID: 7735 RVA: 0x000D47D1 File Offset: 0x000D29D1
		// (set) Token: 0x06001E38 RID: 7736 RVA: 0x000D47D9 File Offset: 0x000D29D9
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

		// Token: 0x170008A7 RID: 2215
		// (get) Token: 0x06001E39 RID: 7737 RVA: 0x000D47F1 File Offset: 0x000D29F1
		// (set) Token: 0x06001E3A RID: 7738 RVA: 0x000D47F9 File Offset: 0x000D29F9
		[Serialize(false, IsPropertySaveable.Yes, "When enabled, the number of matching items is compared to the number of matching items there were at the start of the round. Only valid if RequiredConditionalMatchPercentage is set.", "", false)]
		public bool CompareToInitialAmount { get; set; }

		// Token: 0x06001E3B RID: 7739 RVA: 0x000D4804 File Offset: 0x000D2A04
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

		// Token: 0x06001E3C RID: 7740 RVA: 0x000D4AA0 File Offset: 0x000D2CA0
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

		// Token: 0x06001E3D RID: 7741 RVA: 0x000D4AF4 File Offset: 0x000D2CF4
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

		// Token: 0x06001E3E RID: 7742 RVA: 0x000D4DF0 File Offset: 0x000D2FF0
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

		// Token: 0x06001E3F RID: 7743 RVA: 0x000D4F4C File Offset: 0x000D314C
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

		// Token: 0x06001E40 RID: 7744 RVA: 0x000D4FB0 File Offset: 0x000D31B0
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

		// Token: 0x06001E41 RID: 7745 RVA: 0x000D50D4 File Offset: 0x000D32D4
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

		// Token: 0x04000E89 RID: 3721
		private readonly bool checkPercentage;

		// Token: 0x04000E8A RID: 3722
		private float requiredConditionalMatchPercentage;

		// Token: 0x04000E8C RID: 3724
		private readonly IReadOnlyList<PropertyConditional> conditionals;

		// Token: 0x04000E8D RID: 3725
		private readonly Identifier[] itemIdentifierSplit;

		// Token: 0x04000E8E RID: 3726
		private readonly Identifier[] itemTags;

		// Token: 0x04000E8F RID: 3727
		private readonly List<Item> tempTargetItems = new List<Item>();
	}
}
