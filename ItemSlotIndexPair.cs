using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200021B RID: 539
	internal readonly struct ItemSlotIndexPair : IEquatable<ItemSlotIndexPair>
	{
		// Token: 0x06003675 RID: 13941 RVA: 0x00212975 File Offset: 0x00210B75
		public ItemSlotIndexPair(int Slot, int StackIndex)
		{
			this.Slot = Slot;
			this.StackIndex = StackIndex;
		}

		// Token: 0x17000E7D RID: 3709
		// (get) Token: 0x06003676 RID: 13942 RVA: 0x00212985 File Offset: 0x00210B85
		// (set) Token: 0x06003677 RID: 13943 RVA: 0x0021298D File Offset: 0x00210B8D
		public int Slot { get; set; }

		// Token: 0x17000E7E RID: 3710
		// (get) Token: 0x06003678 RID: 13944 RVA: 0x00212996 File Offset: 0x00210B96
		// (set) Token: 0x06003679 RID: 13945 RVA: 0x0021299E File Offset: 0x00210B9E
		public int StackIndex { get; set; }

		// Token: 0x0600367A RID: 13946 RVA: 0x002129A8 File Offset: 0x00210BA8
		[NullableContext(1)]
		[return: Nullable(0)]
		public static Option<ItemSlotIndexPair> TryDeserializeFromXML(ContentXElement element, string elementName)
		{
			string elementStr = element.GetAttributeString(elementName, string.Empty);
			if (string.IsNullOrEmpty(elementStr))
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			Point point = XMLExtensions.ParsePoint(elementStr, true);
			return Option.Some<ItemSlotIndexPair>(new ItemSlotIndexPair(point.X, point.Y));
		}

		// Token: 0x0600367B RID: 13947 RVA: 0x002129F8 File Offset: 0x00210BF8
		[NullableContext(1)]
		public static string Serialize(Item item)
		{
			Inventory parent = item.ParentInventory;
			if (item.ParentInventory == null)
			{
				throw new Exception("Item \"" + item.Name + "\" is not in an inventory.");
			}
			int slotIndex = parent.FindIndex(item);
			int stackIndex = parent.GetItemStackSlotIndex(item, slotIndex);
			if (slotIndex < 0 || stackIndex < 0)
			{
				throw new Exception("Unable to find item \"" + item.Name + "\" in its parent inventory.");
			}
			return XMLExtensions.PointToString(new Point(slotIndex, stackIndex));
		}

		// Token: 0x0600367C RID: 13948 RVA: 0x00212A70 File Offset: 0x00210C70
		[NullableContext(2)]
		public Item FindItemInContainer(ItemContainer container)
		{
			IEnumerable<Item> items = (container != null) ? container.Inventory.GetItemsAt(this.Slot) : null;
			if (items != null && this.StackIndex >= 0 && this.StackIndex < items.Count<Item>())
			{
				return items.ElementAt(this.StackIndex);
			}
			string errorMsg = "Circuit box error: failed to find an item in the container " + (((container != null) ? container.Item.Name : null) ?? "null") + ".";
			string[] array = new string[5];
			array[0] = errorMsg;
			array[1] = " Items: ";
			array[2] = (((items != null) ? items.Count<Item>().ToString() : null) ?? "null");
			array[3] = ", ";
			int num = 4;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
			defaultInterpolatedStringHandler.AppendLiteral(" Slot: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Slot);
			defaultInterpolatedStringHandler.AppendLiteral(", StackIndex: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.StackIndex);
			array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
			DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
			GameAnalyticsManager.AddErrorEventOnce("ItemSlotIndexPair.FindItemInContainer", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			return null;
		}

		// Token: 0x0600367D RID: 13949 RVA: 0x00212B84 File Offset: 0x00210D84
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ItemSlotIndexPair");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600367E RID: 13950 RVA: 0x00212BD0 File Offset: 0x00210DD0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Slot = ");
			builder.Append(this.Slot.ToString());
			builder.Append(", StackIndex = ");
			builder.Append(this.StackIndex.ToString());
			return true;
		}

		// Token: 0x0600367F RID: 13951 RVA: 0x00212C2C File Offset: 0x00210E2C
		[CompilerGenerated]
		public static bool operator !=(ItemSlotIndexPair left, ItemSlotIndexPair right)
		{
			return !(left == right);
		}

		// Token: 0x06003680 RID: 13952 RVA: 0x00212C38 File Offset: 0x00210E38
		[CompilerGenerated]
		public static bool operator ==(ItemSlotIndexPair left, ItemSlotIndexPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003681 RID: 13953 RVA: 0x00212C42 File Offset: 0x00210E42
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<int>.Default.GetHashCode(this.<Slot>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<StackIndex>k__BackingField);
		}

		// Token: 0x06003682 RID: 13954 RVA: 0x00212C6B File Offset: 0x00210E6B
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is ItemSlotIndexPair && this.Equals((ItemSlotIndexPair)obj);
		}

		// Token: 0x06003683 RID: 13955 RVA: 0x00212C83 File Offset: 0x00210E83
		[CompilerGenerated]
		public bool Equals(ItemSlotIndexPair other)
		{
			return EqualityComparer<int>.Default.Equals(this.<Slot>k__BackingField, other.<Slot>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<StackIndex>k__BackingField, other.<StackIndex>k__BackingField);
		}

		// Token: 0x06003684 RID: 13956 RVA: 0x00212CB5 File Offset: 0x00210EB5
		[CompilerGenerated]
		public void Deconstruct(out int Slot, out int StackIndex)
		{
			Slot = this.Slot;
			StackIndex = this.StackIndex;
		}
	}
}
