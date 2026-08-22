using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005ED RID: 1517
	internal class Pickable : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001923 RID: 6435
		// (get) Token: 0x06006364 RID: 25444 RVA: 0x0033CB72 File Offset: 0x0033AD72
		public virtual bool IsAttached
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17001924 RID: 6436
		// (get) Token: 0x06006365 RID: 25445 RVA: 0x0033CB75 File Offset: 0x0033AD75
		public List<InvSlotType> AllowedSlots
		{
			get
			{
				return this.allowedSlots;
			}
		}

		// Token: 0x17001925 RID: 6437
		// (get) Token: 0x06006366 RID: 25446 RVA: 0x0033CB7D File Offset: 0x0033AD7D
		public bool PickingDone
		{
			get
			{
				return this.pickTimer >= base.PickingTime;
			}
		}

		// Token: 0x17001926 RID: 6438
		// (get) Token: 0x06006367 RID: 25447 RVA: 0x0033CB90 File Offset: 0x0033AD90
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

		// Token: 0x06006368 RID: 25448 RVA: 0x0033CBB4 File Offset: 0x0033ADB4
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

		// Token: 0x06006369 RID: 25449 RVA: 0x0033CC84 File Offset: 0x0033AE84
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
					this.pickingCoroutine = CoroutineManager.StartCoroutine(this.WaitForPick(picker, abilityPickingTime.Value), "");
				}
				return false;
			}
			return this.OnPicked(picker);
		}

		// Token: 0x0600636A RID: 25450 RVA: 0x0033CE08 File Offset: 0x0033B008
		public virtual bool OnPicked(Character picker)
		{
			return this.OnPicked(picker, true, true);
		}

		// Token: 0x0600636B RID: 25451 RVA: 0x0033CE14 File Offset: 0x0033B014
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
				if (!GameMain.Instance.LoadingScreenOpen && playSound && picker == Character.Controlled)
				{
					SoundPlayer.PlayUISound(GUISoundType.PickItem);
				}
				base.PlaySound(ActionType.OnPicked, picker);
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
			if (!GameMain.Instance.LoadingScreenOpen && picker == Character.Controlled)
			{
				SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
			}
			return false;
		}

		// Token: 0x0600636C RID: 25452 RVA: 0x0033D0A0 File Offset: 0x0033B2A0
		private IEnumerable<CoroutineStatus> WaitForPick(Character picker, float requiredTime)
		{
			Pickable.<WaitForPick>d__17 <WaitForPick>d__ = new Pickable.<WaitForPick>d__17(-2);
			<WaitForPick>d__.<>4__this = this;
			<WaitForPick>d__.<>3__picker = picker;
			<WaitForPick>d__.<>3__requiredTime = requiredTime;
			return <WaitForPick>d__;
		}

		// Token: 0x0600636D RID: 25453 RVA: 0x0033D0C0 File Offset: 0x0033B2C0
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

		// Token: 0x0600636E RID: 25454 RVA: 0x0033D110 File Offset: 0x0033B310
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

		// Token: 0x0600636F RID: 25455 RVA: 0x0033D210 File Offset: 0x0033B410
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

		// Token: 0x06006370 RID: 25456 RVA: 0x0033D3BA File Offset: 0x0033B5BA
		public virtual void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Character character = this.activePicker;
			msg.WriteUInt16((character != null) ? character.ID : 0);
		}

		// Token: 0x06006371 RID: 25457 RVA: 0x0033D3D4 File Offset: 0x0033B5D4
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

		// Token: 0x0400338D RID: 13197
		protected Character picker;

		// Token: 0x0400338E RID: 13198
		protected List<InvSlotType> allowedSlots;

		// Token: 0x0400338F RID: 13199
		private float pickTimer;

		// Token: 0x04003390 RID: 13200
		private Character activePicker;

		// Token: 0x04003391 RID: 13201
		private CoroutineHandle pickingCoroutine;
	}
}
