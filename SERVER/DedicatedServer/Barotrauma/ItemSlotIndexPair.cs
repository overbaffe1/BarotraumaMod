using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000124 RID: 292
	internal readonly struct ItemSlotIndexPair : IEquatable<ItemSlotIndexPair>
	{
		// Token: 0x06001B98 RID: 7064 RVA: 0x000CD249 File Offset: 0x000CB449
		public ItemSlotIndexPair(int Slot, int StackIndex)
		{
			this.Slot = Slot;
			this.StackIndex = StackIndex;
		}

		// Token: 0x1700081D RID: 2077
		// (get) Token: 0x06001B99 RID: 7065 RVA: 0x000CD259 File Offset: 0x000CB459
		// (set) Token: 0x06001B9A RID: 7066 RVA: 0x000CD261 File Offset: 0x000CB461
		public int Slot { get; set; }

		// Token: 0x1700081E RID: 2078
		// (get) Token: 0x06001B9B RID: 7067 RVA: 0x000CD26A File Offset: 0x000CB46A
		// (set) Token: 0x06001B9C RID: 7068 RVA: 0x000CD272 File Offset: 0x000CB472
		public int StackIndex { get; set; }

		// Token: 0x06001B9D RID: 7069 RVA: 0x000CD27C File Offset: 0x000CB47C
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

		// Token: 0x06001B9E RID: 7070 RVA: 0x000CD2CC File Offset: 0x000CB4CC
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

		// Token: 0x06001B9F RID: 7071 RVA: 0x000CD344 File Offset: 0x000CB544
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

		// Token: 0x06001BA0 RID: 7072 RVA: 0x000CD458 File Offset: 0x000CB658
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

		// Token: 0x06001BA1 RID: 7073 RVA: 0x000CD4A4 File Offset: 0x000CB6A4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Slot = ");
			builder.Append(this.Slot.ToString());
			builder.Append(", StackIndex = ");
			builder.Append(this.StackIndex.ToString());
			return true;
		}

		// Token: 0x06001BA2 RID: 7074 RVA: 0x000CD500 File Offset: 0x000CB700
		[CompilerGenerated]
		public static bool operator !=(ItemSlotIndexPair left, ItemSlotIndexPair right)
		{
			return !(left == right);
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x000CD50C File Offset: 0x000CB70C
		[CompilerGenerated]
		public static bool operator ==(ItemSlotIndexPair left, ItemSlotIndexPair right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x000CD516 File Offset: 0x000CB716
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<int>.Default.GetHashCode(this.<Slot>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<StackIndex>k__BackingField);
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x000CD53F File Offset: 0x000CB73F
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is ItemSlotIndexPair && this.Equals((ItemSlotIndexPair)obj);
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x000CD557 File Offset: 0x000CB757
		[CompilerGenerated]
		public bool Equals(ItemSlotIndexPair other)
		{
			return EqualityComparer<int>.Default.Equals(this.<Slot>k__BackingField, other.<Slot>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<StackIndex>k__BackingField, other.<StackIndex>k__BackingField);
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x000CD589 File Offset: 0x000CB789
		[CompilerGenerated]
		public void Deconstruct(out int Slot, out int StackIndex)
		{
			Slot = this.Slot;
			StackIndex = this.StackIndex;
		}
	}
}
