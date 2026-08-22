using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004BB RID: 1211
	internal class Pickable : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001262 RID: 4706
		// (get) Token: 0x060044E6 RID: 17638 RVA: 0x001B9216 File Offset: 0x001B7416
		public virtual bool IsAttached
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17001263 RID: 4707
		// (get) Token: 0x060044E7 RID: 17639 RVA: 0x001B9219 File Offset: 0x001B7419
		public List<InvSlotType> AllowedSlots
		{
			get
			{
				return this.allowedSlots;
			}
		}

		// Token: 0x17001264 RID: 4708
		// (get) Token: 0x060044E8 RID: 17640 RVA: 0x001B9221 File Offset: 0x001B7421
		public bool PickingDone
		{
			get
			{
				return this.pickTimer >= base.PickingTime;
			}
		}

		// Token: 0x17001265 RID: 4709
		// (get) Token: 0x060044E9 RID: 17641 RVA: 0x001B9234 File Offset: 0x001B7434
		public Character Picker
		{
			get
			{
				if (this.picker != null && this.picker.Removed)
				{
					this.picker = null;
				}
				return this.picker;
			}
		}

		// Token: 0x060044EA RID: 17642 RVA: 0x001B9258 File Offset: 0x001B7458
		public Pickable(Item item, ContentXElement element) : base(item, element)
		{
			this.allowedSlots = new List<InvSlotType>();
			string slotString = element.GetAttributeString("slots", "Any");
			string[] slotCombinations = slotString.Split(',', StringSplitOptions.None);
			foreach (string slotCombination in slotCombinations)
			{
				string[] slots = slotCombination.Split('+', StringSplitOptions.None);
				InvSlotType allowedSlot = InvSlotType.None;
				foreach (string slot in slots)
				{
					string a = slot.ToLowerInvariant();
					if (a == "bothhands")
					{
						allowedSlot = (InvSlotType.RightHand | InvSlotType.LeftHand);
					}
					else
					{
						allowedSlot |= (InvSlotType)Enum.Parse(typeof(InvSlotType), slot.Trim());
					}
				}
				this.allowedSlots.Add(allowedSlot);
			}
			this.canBePicked = true;
		}

		// Token: 0x060044EB RID: 17643 RVA: 0x001B9328 File Offset: 0x001B7528
		public override bool Pick(Character picker)
		{
			if (this.pickTimer > 0f)
			{
				return false;
			}
			if (base.PickingTime >= 3.4028235E+38f)
			{
				return false;
			}
			if (picker == null || picker.Inventory == null)
			{
				return false;
			}
			if (!picker.Inventory.AccessibleWhenAlive && !picker.Inventory.AccessibleByOwner)
			{
				return false;
			}
			if (base.PickingTime > 0f)
			{
				AbilityItemPickingTime abilityPickingTime = new AbilityItemPickingTime(base.PickingTime, this.item.Prefab);
				picker.CheckTalents(AbilityEffectType.OnItemPicked, abilityPickingTime);
				if (this.RequiredItems.ContainsKey(RelatedItem.RelationType.Equipped))
				{
					foreach (RelatedItem ri in this.RequiredItems[RelatedItem.RelationType.Equipped])
					{
						foreach (Item heldItem in picker.HeldItems)
						{
							if (ri.MatchesItem(heldItem))
							{
								abilityPickingTime.Value /= 1f + heldItem.Prefab.AddedPickingSpeedMultiplier;
							}
						}
					}
				}
				if ((picker.PickingItem == null || picker.PickingItem == this.item) && base.PickingTime <= 3.4028235E+38f)
				{
					this.activePicker = picker;
					this.item.CreateServerEvent<Pickable>(this);
					this.pickingCoroutine = CoroutineManager.StartCoroutine(this.WaitForPick(picker, abilityPickingTime.Value), "");
				}
				return false;
			}
			return this.OnPicked(picker);
		}

		// Token: 0x060044EC RID: 17644 RVA: 0x001B94C0 File Offset: 0x001B76C0
		public virtual bool OnPicked(Character picker)
		{
			return this.OnPicked(picker, true, true);
		}

		// Token: 0x060044ED RID: 17645 RVA: 0x001B94CC File Offset: 0x001B76CC
		public bool OnPicked(Character picker, bool pickDroppedStack, bool playSound = true)
		{
			if (this.item.GetComponents<Pickable>().Any<Pickable>())
			{
				bool alreadyEquipped = false;
				int i;
				Func<InvSlotType, bool> <>9__0;
				int i2;
				for (i = 0; i < picker.Inventory.Capacity; i = i2 + 1)
				{
					if (picker.Inventory.GetItemsAt(i).Contains(this.item) && picker.Inventory.SlotTypes[i] != InvSlotType.Any)
					{
						IEnumerable<InvSlotType> source = this.allowedSlots;
						Func<InvSlotType, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((InvSlotType a) => a.HasFlag(picker.Inventory.SlotTypes[i])));
						}
						if (!source.Any(predicate))
						{
							alreadyEquipped = true;
							break;
						}
					}
					i2 = i;
				}
				if (alreadyEquipped)
				{
					return false;
				}
			}
			List<Item> droppedStack = pickDroppedStack ? this.item.DroppedStack.ToList<Item>() : null;
			if (picker.Inventory.TryPutItemWithAutoEquipCheck(this.item, picker, this.allowedSlots, true))
			{
				if (!picker.HeldItems.Contains(this.item) && this.item.body != null)
				{
					this.item.body.Enabled = false;
				}
				this.picker = picker;
				for (int j = this.item.linkedTo.Count - 1; j >= 0; j--)
				{
					this.item.linkedTo[j].RemoveLinked(this.item);
				}
				this.item.linkedTo.Clear();
				this.DropConnectedWires(picker);
				base.ApplyStatusEffects(ActionType.OnPicked, 1f, picker, null, null, null, null, 1f);
				if (pickDroppedStack)
				{
					foreach (Item droppedItem in droppedStack)
					{
						if (droppedItem != this.item)
						{
							droppedItem.GetComponent<Pickable>().OnPicked(picker, false, false);
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x060044EE RID: 17646 RVA: 0x001B9708 File Offset: 0x001B7908
		private IEnumerable<CoroutineStatus> WaitForPick(Character picker, float requiredTime)
		{
			Pickable.<WaitForPick>d__17 <WaitForPick>d__ = new Pickable.<WaitForPick>d__17(-2);
			<WaitForPick>d__.<>4__this = this;
			<WaitForPick>d__.<>3__picker = picker;
			<WaitForPick>d__.<>3__requiredTime = requiredTime;
			return <WaitForPick>d__;
		}

		// Token: 0x060044EF RID: 17647 RVA: 0x001B9728 File Offset: 0x001B7928
		protected void StopPicking(Character picker)
		{
			if (picker != null)
			{
				picker.AnimController.StopUsingItem();
				picker.PickingItem = null;
			}
			if (this.pickingCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.pickingCoroutine);
				this.pickingCoroutine = null;
			}
			this.activePicker = null;
			this.pickTimer = 0f;
		}

		// Token: 0x060044F0 RID: 17648 RVA: 0x001B9778 File Offset: 0x001B7978
		protected void DropConnectedWires(Character character)
		{
			Vector2 pos = (character == null) ? this.item.SimPosition : character.SimPosition;
			foreach (ConnectionPanel connectionPanel in this.item.GetComponents<ConnectionPanel>())
			{
				connectionPanel.DisconnectedWires.Clear();
				foreach (Connection c in connectionPanel.Connections)
				{
					foreach (Wire w in c.Wires.ToArray<Wire>())
					{
						if (w != null)
						{
							w.Item.Drop(character, true, true);
							w.Item.SetTransform(pos, 0f, true, true, null);
						}
					}
				}
			}
		}

		// Token: 0x060044F1 RID: 17649 RVA: 0x001B9878 File Offset: 0x001B7A78
		public override void Drop(Character dropper, bool setTransform = true)
		{
			if (this.picker == null)
			{
				this.picker = dropper;
			}
			Vector2 bodyDropPos = Vector2.Zero;
			if (this.picker == null || this.picker.Inventory == null)
			{
				if (this.item.ParentInventory != null && this.item.ParentInventory.Owner != null && !this.item.ParentInventory.Owner.Removed)
				{
					bodyDropPos = this.item.ParentInventory.Owner.SimPosition;
					PhysicsBody body = this.item.body;
					if (body != null)
					{
						body.ResetDynamics();
					}
				}
			}
			else if (!this.picker.Removed)
			{
				this.DropConnectedWires(this.picker);
				this.item.Submarine = this.picker.Submarine;
				bodyDropPos = this.picker.SimPosition;
				this.picker.Inventory.RemoveItem(this.item);
				this.picker = null;
			}
			if (this.item.body != null && !this.item.body.Enabled && setTransform)
			{
				if (this.item.body.Removed)
				{
					DebugConsole.ThrowError("Failed to drop the Pickable component of the item \"" + this.item.Name + "\" (body has been removed" + (this.item.Removed ? ", item has been removed)" : ")"), null, null, false, false);
					return;
				}
				this.item.body.ResetDynamics();
				this.item.SetTransform(bodyDropPos, 0f, true, true, null);
				this.item.body.Enabled = true;
			}
		}

		// Token: 0x060044F2 RID: 17650 RVA: 0x001B9A22 File Offset: 0x001B7C22
		public virtual void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character character = this.activePicker;
			msg.WriteUInt16((character != null) ? character.ID : 0);
		}

		// Token: 0x060044F3 RID: 17651 RVA: 0x001B9A3C File Offset: 0x001B7C3C
		public virtual void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			ushort pickerID = msg.ReadUInt16();
			if (pickerID == 0)
			{
				this.StopPicking(this.activePicker);
				return;
			}
			this.Pick(Entity.FindEntityByID(pickerID) as Character);
		}

		// Token: 0x04002114 RID: 8468
		protected Character picker;

		// Token: 0x04002115 RID: 8469
		protected List<InvSlotType> allowedSlots;

		// Token: 0x04002116 RID: 8470
		private float pickTimer;

		// Token: 0x04002117 RID: 8471
		private Character activePicker;

		// Token: 0x04002118 RID: 8472
		private CoroutineHandle pickingCoroutine;
	}
}
