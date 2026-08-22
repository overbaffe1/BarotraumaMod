using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000C4 RID: 196
	internal class Inventory
	{
		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x06001819 RID: 6169 RVA: 0x000EFC42 File Offset: 0x000EDE42
		public static float UIScale
		{
			get
			{
				return ((float)GameMain.GraphicsWidth / 1920f + (float)GameMain.GraphicsHeight / 1080f) / 2.5f * GameSettings.CurrentConfig.Graphics.InventoryScale;
			}
		}

		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x0600181A RID: 6170 RVA: 0x000EFC73 File Offset: 0x000EDE73
		public static int ContainedIndicatorHeight
		{
			get
			{
				return (int)(15f * Inventory.UIScale);
			}
		}

		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x0600181B RID: 6171 RVA: 0x000EFC84 File Offset: 0x000EDE84
		public static Sprite SlotSpriteSmall
		{
			get
			{
				if (Inventory.slotSpriteSmall == null)
				{
					Inventory.slotSpriteSmall = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(10, 6, 119, 120)), null, 0f);
					Inventory.SlotSpriteSmall.size = new Vector2((float)Inventory.SlotSpriteSmall.SourceRect.Width * 0.575f, (float)Inventory.SlotSpriteSmall.SourceRect.Height * 0.575f);
				}
				return Inventory.slotSpriteSmall;
			}
		}

		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x0600181C RID: 6172 RVA: 0x000EFD06 File Offset: 0x000EDF06
		// (set) Token: 0x0600181D RID: 6173 RVA: 0x000EFD0E File Offset: 0x000EDF0E
		public Rectangle BackgroundFrame { get; protected set; }

		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x0600181E RID: 6174 RVA: 0x000EFD17 File Offset: 0x000EDF17
		public static bool DraggingItemToWorld
		{
			get
			{
				return Character.Controlled != null && !Character.Controlled.HasSelectedAnyItem && CharacterHealth.OpenHealthWindow == null && Inventory.DraggingItems.Any<Item>();
			}
		}

		// Token: 0x170005F1 RID: 1521
		// (set) Token: 0x0600181F RID: 6175 RVA: 0x000EFD3F File Offset: 0x000EDF3F
		public int SlotsPerRow
		{
			set
			{
				this.slotsPerRow = Math.Max(1, value);
			}
		}

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x06001820 RID: 6176 RVA: 0x000EFD4E File Offset: 0x000EDF4E
		public static Inventory.SlotReference SelectedSlot
		{
			get
			{
				Inventory.SlotReference slotReference = Inventory.selectedSlot;
				bool flag;
				if (slotReference == null)
				{
					flag = (null != null);
				}
				else
				{
					Inventory parentInventory = slotReference.ParentInventory;
					flag = (((parentInventory != null) ? parentInventory.Owner : null) != null);
				}
				if (!flag || Inventory.selectedSlot.ParentInventory.Owner.Removed)
				{
					return null;
				}
				return Inventory.selectedSlot;
			}
		}

		// Token: 0x06001821 RID: 6177 RVA: 0x000EFD8C File Offset: 0x000EDF8C
		public Inventory GetReplacementOrThis()
		{
			Inventory replacedBy = this.ReplacedBy;
			return ((replacedBy != null) ? replacedBy.GetReplacementOrThis() : null) ?? this;
		}

		// Token: 0x06001822 RID: 6178 RVA: 0x000EFDA8 File Offset: 0x000EDFA8
		public virtual void CreateSlots()
		{
			this.visualSlots = new VisualSlot[this.capacity];
			int rows = (int)Math.Ceiling((double)this.capacity / (double)this.slotsPerRow);
			int columns = Math.Min(this.slotsPerRow, this.capacity);
			Vector2 spacing = new Vector2(5f * Inventory.UIScale);
			spacing.Y += ((this is CharacterInventory) ? (Inventory.UnequippedIndicator.size.Y * Inventory.UIScale) : ((float)Inventory.ContainedIndicatorHeight));
			Vector2 rectSize = new Vector2(60f * Inventory.UIScale);
			this.padding = new Vector4(spacing.X, spacing.Y, spacing.X, spacing.X);
			Vector2 slotAreaSize = new Vector2((float)columns * rectSize.X + (float)(columns - 1) * spacing.X, (float)rows * rectSize.Y + (float)(rows - 1) * spacing.Y);
			slotAreaSize.X += this.padding.X + this.padding.Z;
			slotAreaSize.Y += this.padding.Y + this.padding.W;
			Vector2 topLeft = new Vector2((float)(GameMain.GraphicsWidth / 2) - slotAreaSize.X / 2f, (float)(GameMain.GraphicsHeight / 2) - slotAreaSize.Y / 2f);
			Vector2 center = topLeft + slotAreaSize / 2f;
			if (this.RectTransform != null)
			{
				Vector2 scale = new Vector2((float)this.RectTransform.Rect.Width / slotAreaSize.X, (float)this.RectTransform.Rect.Height / slotAreaSize.Y);
				spacing *= scale;
				rectSize *= scale;
				this.padding.X = this.padding.X * scale.X;
				this.padding.Z = this.padding.Z * scale.X;
				this.padding.Y = this.padding.Y * scale.Y;
				this.padding.W = this.padding.W * scale.Y;
				center = this.RectTransform.Rect.Center.ToVector2();
				topLeft = this.RectTransform.TopLeft.ToVector2() + new Vector2(this.padding.X, this.padding.Y);
				this.prevRect = this.RectTransform.Rect;
			}
			Rectangle slotRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)rectSize.X, (int)rectSize.Y);
			for (int i = 0; i < this.capacity; i++)
			{
				int row = (int)Math.Floor((double)i / (double)this.slotsPerRow);
				int slotsPerThisRow = Math.Min(this.slotsPerRow, this.capacity - row * this.slotsPerRow);
				int slotNumberOnThisRow = i - row * this.slotsPerRow;
				int rowWidth = (int)(rectSize.X * (float)slotsPerThisRow + spacing.X * (float)(slotsPerThisRow - 1));
				slotRect.X = (int)center.X - rowWidth / 2;
				slotRect.X += (int)((rectSize.X + spacing.X) * (float)(slotNumberOnThisRow % slotsPerThisRow));
				slotRect.Y = (int)(topLeft.Y + (rectSize.Y + spacing.Y) * (float)row);
				this.visualSlots[i] = new VisualSlot(slotRect);
				this.visualSlots[i].InteractRect = new Rectangle((int)((float)this.visualSlots[i].Rect.X - spacing.X / 2f - 1f), (int)((float)this.visualSlots[i].Rect.Y - spacing.Y / 2f - 1f), (int)((float)this.visualSlots[i].Rect.Width + spacing.X + 2f), (int)((float)this.visualSlots[i].Rect.Height + spacing.Y + 2f));
				if (this.visualSlots[i].Rect.Width > this.visualSlots[i].Rect.Height)
				{
					this.visualSlots[i].Rect.Inflate((this.visualSlots[i].Rect.Height - this.visualSlots[i].Rect.Width) / 2, 0);
				}
				else
				{
					this.visualSlots[i].Rect.Inflate(0, (this.visualSlots[i].Rect.Width - this.visualSlots[i].Rect.Height) / 2);
				}
			}
			if (Inventory.selectedSlot != null && Inventory.selectedSlot.ParentInventory == this)
			{
				Inventory.selectedSlot = new Inventory.SlotReference(this, this.visualSlots[Inventory.selectedSlot.SlotIndex], Inventory.selectedSlot.SlotIndex, Inventory.selectedSlot.IsSubSlot, Inventory.selectedSlot.Inventory);
			}
			this.CalculateBackgroundFrame();
		}

		// Token: 0x06001823 RID: 6179 RVA: 0x000F02D2 File Offset: 0x000EE4D2
		protected virtual void CalculateBackgroundFrame()
		{
		}

		// Token: 0x06001824 RID: 6180 RVA: 0x000F02D4 File Offset: 0x000EE4D4
		public bool Movable()
		{
			return this.movableFrameRect.Size != Point.Zero;
		}

		// Token: 0x06001825 RID: 6181 RVA: 0x000F02EC File Offset: 0x000EE4EC
		public bool IsInventoryHoverAvailable(Character owner, ItemContainer container)
		{
			if (container == null && this is ItemInventory)
			{
				container = (this as ItemInventory).Container;
			}
			return container != null && (owner.SelectedCharacter != null || owner == null || !container.KeepOpenWhenEquippedBy(owner) || !owner.HasEquippedItem(container.Item, null, null));
		}

		// Token: 0x06001826 RID: 6182 RVA: 0x000F0347 File Offset: 0x000EE547
		public virtual bool HideSlot(int i)
		{
			return this.visualSlots[i].Disabled || (this.slots[i].HideIfEmpty && this.slots[i].Empty());
		}

		// Token: 0x06001827 RID: 6183 RVA: 0x000F0378 File Offset: 0x000EE578
		public virtual void Update(float deltaTime, Camera cam, bool subInventory = false)
		{
			if (this.visualSlots == null || this.isSubInventory != subInventory || (this.RectTransform != null && this.RectTransform.Rect != this.prevRect))
			{
				this.CreateSlots();
				this.isSubInventory = subInventory;
			}
			if (!subInventory || this.OpenState >= 0.99f || this.OpenState < 0.01f)
			{
				for (int i = 0; i < this.capacity; i++)
				{
					if (!this.HideSlot(i))
					{
						this.UpdateSlot(this.visualSlots[i], i, this.slots[i].Items.FirstOrDefault<Item>(), subInventory);
					}
				}
				if (!this.isSubInventory)
				{
					this.ControlInput(cam);
				}
			}
		}

		// Token: 0x06001828 RID: 6184 RVA: 0x000F042B File Offset: 0x000EE62B
		protected virtual void ControlInput(Camera cam)
		{
			if (Inventory.selectedSlot != null && !Inventory.DraggingItemToWorld && cam.GetZoomAmountFromPrevious() <= 0.25f)
			{
				cam.Freeze = true;
			}
		}

		// Token: 0x06001829 RID: 6185 RVA: 0x000F0450 File Offset: 0x000EE650
		protected void UpdateSlot(VisualSlot slot, int slotIndex, Item item, bool isSubSlot)
		{
			Rectangle interactRect = slot.InteractRect;
			interactRect.Location += slot.DrawOffset.ToPoint();
			bool mouseOnGUI = false;
			bool mouseOn = interactRect.Contains(PlayerInput.MousePosition) && !this.Locked && !mouseOnGUI && !slot.Disabled && Inventory.IsMouseOnInventory;
			if (SubEditorScreen.IsSubEditor() && PlayerInput.IsCtrlDown())
			{
				Inventory.DraggingItems.Clear();
				bool mouseDrag = SubEditorScreen.MouseDragStart != Vector2.Zero && Vector2.Distance(PlayerInput.MousePosition, SubEditorScreen.MouseDragStart) >= GUI.Scale * 20f;
				if (mouseOn && (PlayerInput.PrimaryMouseButtonClicked() || mouseDrag) && item != null)
				{
					slot.ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.4f, 0.5f);
					if (!mouseDrag)
					{
						SoundPlayer.PlayUISound(GUISoundType.PickItem);
					}
					if (!item.Removed)
					{
						SubEditorScreen.BulkItemBufferInUse = SubEditorScreen.ItemRemoveMutex;
						SubEditorScreen.BulkItemBuffer.Add(new AddOrDeleteCommand(new List<MapEntity>
						{
							item
						}, true, true));
					}
					ItemInventory ownInventory = item.OwnInventory;
					if (ownInventory != null)
					{
						ownInventory.DeleteAllItems();
					}
					item.Remove();
				}
			}
			if (PlayerInput.PrimaryMouseButtonHeld() && PlayerInput.SecondaryMouseButtonHeld())
			{
				mouseOn = false;
			}
			if (Inventory.selectedSlot != null && Inventory.selectedSlot.Slot != slot)
			{
				if (Inventory.selectedSlot.IsSubSlot && !isSubSlot)
				{
					mouseOn = false;
				}
				else if (!Inventory.selectedSlot.IsSubSlot && isSubSlot && mouseOn)
				{
					Inventory.selectedSlot = null;
				}
			}
			slot.State = GUIComponent.ComponentState.None;
			if (mouseOn && (Inventory.DraggingItems.Any<Item>() || Inventory.selectedSlot == null || Inventory.selectedSlot.Slot == slot) && Inventory.DraggingInventory == null)
			{
				slot.State = GUIComponent.ComponentState.Hover;
				if (Inventory.selectedSlot == null || (!Inventory.selectedSlot.IsSubSlot && isSubSlot))
				{
					Item item2 = this.slots[slotIndex].FirstOrDefault();
					Inventory subInventory;
					if (item2 == null)
					{
						subInventory = null;
					}
					else
					{
						ItemContainer component = item2.GetComponent<ItemContainer>();
						subInventory = ((component != null) ? component.Inventory : null);
					}
					Inventory.SlotReference slotRef = new Inventory.SlotReference(this, slot, slotIndex, isSubSlot, subInventory);
					SubEditorScreen editor = Screen.Selected as SubEditorScreen;
					if (editor != null && !editor.WiringMode && slotRef.ParentInventory is CharacterInventory)
					{
						return;
					}
					if (Inventory.CanSelectSlot(slotRef))
					{
						Inventory.selectedSlot = slotRef;
					}
				}
				if (!Inventory.DraggingItems.Any<Item>())
				{
					IEnumerable<Item> enumerable;
					if (Screen.Selected != GameMain.GameScreen)
					{
						IEnumerable<Item> items = this.slots[slotIndex].Items;
						enumerable = items;
					}
					else
					{
						enumerable = from it in this.slots[slotIndex].Items
						where it.IsInteractable(Character.Controlled)
						select it;
					}
					IEnumerable<Item> interactableItems = enumerable;
					if (interactableItems.Any<Item>())
					{
						if (Inventory.availableContextualOrder.Item1 != null)
						{
							if (PlayerInput.PrimaryMouseButtonClicked())
							{
								GameMain.GameSession.CrewManager.SetCharacterOrder(null, new Order(OrderPrefab.Prefabs[Inventory.availableContextualOrder.Item2], Inventory.availableContextualOrder.Item1, null, Character.Controlled, false), true);
							}
							Inventory.availableContextualOrder = default(ValueTuple<Item, Identifier>);
							return;
						}
						if (PlayerInput.KeyDown(InputType.Command) && PlayerInput.KeyDown(InputType.ContextualCommand))
						{
							GameSession gameSession = GameMain.GameSession;
							if (((gameSession != null) ? gameSession.CrewManager : null) != null)
							{
								GameMain.GameSession.CrewManager.OpenCommandUI(interactableItems.FirstOrDefault<Item>(), true);
								return;
							}
						}
						if (PlayerInput.PrimaryMouseButtonDown())
						{
							if (PlayerInput.KeyDown(InputType.TakeHalfFromInventorySlot))
							{
								Inventory.DraggingItems.AddRange(interactableItems.Skip(interactableItems.Count<Item>() / 2));
							}
							else if (PlayerInput.KeyDown(InputType.TakeOneFromInventorySlot))
							{
								Inventory.DraggingItems.Add(interactableItems.First<Item>());
							}
							else
							{
								Inventory.DraggingItems.AddRange(interactableItems);
							}
							Inventory.DraggingSlot = slot;
							return;
						}
					}
				}
				else if (PlayerInput.PrimaryMouseButtonReleased())
				{
					IEnumerable<Item> enumerable2;
					if (Screen.Selected != GameMain.GameScreen)
					{
						IEnumerable<Item> items = this.slots[slotIndex].Items;
						enumerable2 = items;
					}
					else
					{
						enumerable2 = from it in this.slots[slotIndex].Items
						where it.IsInteractable(Character.Controlled)
						select it;
					}
					IEnumerable<Item> interactableItems2 = enumerable2;
					if (PlayerInput.DoubleClicked() && interactableItems2.Any<Item>())
					{
						Inventory.doubleClickedItems.Clear();
						if (PlayerInput.KeyDown(InputType.TakeHalfFromInventorySlot))
						{
							Inventory.doubleClickedItems.AddRange(interactableItems2.Skip(interactableItems2.Count<Item>() / 2));
							return;
						}
						if (PlayerInput.KeyDown(InputType.TakeOneFromInventorySlot))
						{
							Inventory.doubleClickedItems.Add(interactableItems2.First<Item>());
							return;
						}
						Inventory.doubleClickedItems.AddRange(interactableItems2);
					}
				}
			}
		}

		// Token: 0x0600182A RID: 6186 RVA: 0x000F08B4 File Offset: 0x000EEAB4
		protected Inventory GetSubInventory(int slotIndex)
		{
			Item item = this.slots[slotIndex].FirstOrDefault();
			ItemContainer container = (item != null) ? item.GetComponent<ItemContainer>() : null;
			if (container == null)
			{
				return null;
			}
			return container.Inventory;
		}

		// Token: 0x0600182B RID: 6187 RVA: 0x000F08E6 File Offset: 0x000EEAE6
		protected virtual ItemInventory GetActiveEquippedSubInventory(int slotIndex)
		{
			return null;
		}

		// Token: 0x0600182C RID: 6188 RVA: 0x000F08EC File Offset: 0x000EEAEC
		public void UpdateSubInventory(float deltaTime, int slotIndex, Camera cam)
		{
			Item item = this.slots[slotIndex].FirstOrDefault();
			if (item == null)
			{
				return;
			}
			ItemContainer container = item.GetComponent<ItemContainer>();
			if (container == null || !container.DrawInventory)
			{
				return;
			}
			if (container.Inventory.DrawWhenEquipped)
			{
				return;
			}
			ItemInventory subInventory = container.Inventory;
			if (subInventory.visualSlots == null)
			{
				subInventory.CreateSlots();
			}
			this.canMove = (container.MovableFrame && !subInventory.IsInventoryHoverAvailable(this.Owner as Character, container) && subInventory.originalPos != Point.Zero);
			CharacterInventory characterInventory = this as CharacterInventory;
			if (characterInventory != null && characterInventory.CurrentLayout != CharacterInventory.Layout.Default)
			{
				this.canMove = false;
			}
			if (this.canMove)
			{
				subInventory.HideTimer = 1f;
				subInventory.OpenState = 1f;
				if (subInventory.movableFrameRect.Contains(PlayerInput.MousePosition) && PlayerInput.SecondaryMouseButtonClicked())
				{
					container.Inventory.savedPosition = container.Inventory.originalPos;
				}
				if (subInventory.movableFrameRect.Contains(PlayerInput.MousePosition) || (Inventory.DraggingInventory != null && Inventory.DraggingInventory == subInventory))
				{
					if (Inventory.DraggingInventory == null)
					{
						if (PlayerInput.PrimaryMouseButtonDown())
						{
							Inventory.DraggingItems.Clear();
							Inventory.DraggingSlot = null;
							Inventory.DraggingInventory = subInventory;
						}
					}
					else if (PlayerInput.PrimaryMouseButtonReleased())
					{
						Inventory.DraggingInventory = null;
						subInventory.savedPosition = PlayerInput.MousePosition.ToPoint();
					}
					else if (Inventory.DraggingInventory == subInventory)
					{
						subInventory.savedPosition = PlayerInput.MousePosition.ToPoint();
					}
				}
			}
			int itemCapacity = subInventory.slots.Length;
			VisualSlot slot = this.visualSlots[slotIndex];
			int dir = slot.SubInventoryDir;
			Rectangle subRect = slot.Rect;
			Vector2 spacing = new Vector2(10f * Inventory.UIScale, (10f + Inventory.UnequippedIndicator.size.Y) * Inventory.UIScale * GUI.AspectRatioAdjustment);
			int columns = MathHelper.Clamp((int)Math.Floor(Math.Sqrt((double)itemCapacity)), 1, container.SlotsPerRow);
			while ((float)(itemCapacity / columns) * ((float)subRect.Height + spacing.Y) > (float)GameMain.GraphicsHeight * 0.5f)
			{
				columns++;
			}
			int width = (int)((float)(subRect.Width * columns) + spacing.X * (float)(columns - 1));
			int startX = slot.Rect.Center.X - (int)((float)width / 2f);
			int startY = (dir < 0) ? (slot.EquipButtonRect.Y - subRect.Height - (int)(35f * Inventory.UIScale)) : (slot.EquipButtonRect.Bottom + (int)(10f * Inventory.UIScale));
			if (this.canMove)
			{
				startX += subInventory.savedPosition.X - subInventory.originalPos.X;
				startY += subInventory.savedPosition.Y - subInventory.originalPos.Y;
			}
			float totalHeight = (float)(itemCapacity / columns) * ((float)subRect.Height + spacing.Y);
			int padding = (int)(20f * Inventory.UIScale);
			startX = Math.Max(startX, padding);
			startX -= Math.Max(startX + width - GameMain.GraphicsWidth + padding, 0);
			startY = Math.Max(startY, (int)totalHeight - padding / 2);
			startY -= Math.Max(startY - GameMain.GraphicsHeight + padding * 2 + (this.canMove ? ((int)(40f * Inventory.UIScale)) : 0), 0);
			subRect.X = startX;
			subRect.Y = startY;
			subInventory.OpenState = ((subInventory.HideTimer >= 0.5f) ? Math.Min(subInventory.OpenState + deltaTime * 8f, 1f) : Math.Max(subInventory.OpenState - deltaTime * 5f, 0f));
			for (int i = 0; i < itemCapacity; i++)
			{
				subInventory.visualSlots[i].Rect = subRect;
				VisualSlot visualSlot = subInventory.visualSlots[i];
				visualSlot.Rect.Location = visualSlot.Rect.Location + new Point(0, (int)totalHeight * -dir);
				subInventory.visualSlots[i].DrawOffset = Vector2.SmoothStep(new Vector2(0f, (float)(-50 * dir)), new Vector2(0f, totalHeight * (float)dir), subInventory.OpenState);
				subInventory.visualSlots[i].InteractRect = new Rectangle((int)((float)subInventory.visualSlots[i].Rect.X - spacing.X / 2f - 1f), (int)((float)subInventory.visualSlots[i].Rect.Y - spacing.Y / 2f - 1f), (int)((float)subInventory.visualSlots[i].Rect.Width + spacing.X + 2f), (int)((float)subInventory.visualSlots[i].Rect.Height + spacing.Y + 2f));
				if ((i + 1) % columns == 0)
				{
					subRect.X = startX;
					subRect.Y += subRect.Height * dir;
					subRect.Y += (int)(spacing.Y * (float)dir);
				}
				else
				{
					subRect.X = (int)((float)subInventory.visualSlots[i].Rect.Right + spacing.X);
				}
			}
			if (this.canMove)
			{
				subInventory.movableFrameRect.X = subRect.X - (int)spacing.X;
				subInventory.movableFrameRect.Y = subRect.Y + (int)spacing.Y;
			}
			this.visualSlots[slotIndex].State = GUIComponent.ComponentState.Hover;
			subInventory.isSubInventory = true;
			subInventory.Update(deltaTime, cam, true);
		}

		// Token: 0x0600182D RID: 6189 RVA: 0x000F0EA4 File Offset: 0x000EF0A4
		public void ClearSubInventories()
		{
			if (Inventory.highlightedSubInventorySlots.Count == 0)
			{
				return;
			}
			foreach (Inventory.SlotReference highlightedSubInventorySlot in Inventory.highlightedSubInventorySlots)
			{
				highlightedSubInventorySlot.Inventory.HideTimer = 0f;
			}
			Inventory.highlightedSubInventorySlots.Clear();
		}

		// Token: 0x0600182E RID: 6190 RVA: 0x000F0F18 File Offset: 0x000EF118
		public virtual void Draw(SpriteBatch spriteBatch, bool subInventory = false)
		{
			if (this.visualSlots == null || this.isSubInventory != subInventory)
			{
				return;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (!this.HideSlot(i))
				{
					if (!Inventory.DraggingItems.Any<Item>())
					{
						goto IL_6D;
					}
					if (!this.slots[i].Items.All((Item it) => Inventory.DraggingItems.Contains(it)))
					{
						goto IL_6D;
					}
					bool flag = this.visualSlots[i].MouseOn();
					IL_6E:
					bool drawItem = flag;
					Inventory.DrawSlot(spriteBatch, this, this.visualSlots[i], this.slots[i].FirstOrDefault(), i, drawItem, InvSlotType.Any);
					goto IL_8E;
					IL_6D:
					flag = true;
					goto IL_6E;
				}
				IL_8E:;
			}
		}

		// Token: 0x0600182F RID: 6191 RVA: 0x000F0FC4 File Offset: 0x000EF1C4
		public static bool IsMouseOnSlot(VisualSlot slot)
		{
			Rectangle rect = new Rectangle(slot.InteractRect.X, slot.InteractRect.Y, slot.InteractRect.Width, slot.InteractRect.Height);
			rect.Offset(slot.DrawOffset);
			return rect.Contains(PlayerInput.MousePosition);
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x06001830 RID: 6192 RVA: 0x000F101D File Offset: 0x000EF21D
		// (set) Token: 0x06001831 RID: 6193 RVA: 0x000F1024 File Offset: 0x000EF224
		public static bool IsMouseOnInventory { get; private set; }

		// Token: 0x06001832 RID: 6194 RVA: 0x000F102C File Offset: 0x000EF22C
		public static void RefreshMouseOnInventory()
		{
			Inventory.IsMouseOnInventory = Inventory.DetermineMouseOnInventory(false);
		}

		// Token: 0x06001833 RID: 6195 RVA: 0x000F103C File Offset: 0x000EF23C
		private static bool DetermineMouseOnInventory(bool ignoreDraggedItem = false)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) != null && (GameMain.GameSession.Campaign.ShowCampaignUI || GameMain.GameSession.Campaign.ForceMapUI))
			{
				return false;
			}
			if (GameSession.IsTabMenuOpen)
			{
				return false;
			}
			if (CrewManager.IsCommandInterfaceOpen)
			{
				return false;
			}
			if (Character.Controlled == null)
			{
				return false;
			}
			if (!ignoreDraggedItem && (Inventory.DraggingItems.Any<Item>() || Inventory.DraggingInventory != null))
			{
				return true;
			}
			SubEditorScreen editor = Screen.Selected as SubEditorScreen;
			bool isSubEditor = editor != null && !editor.WiringMode;
			if (Character.Controlled.Inventory != null && !isSubEditor && Inventory.<DetermineMouseOnInventory>g__IsOnInventorySlot|79_0(Character.Controlled.Inventory))
			{
				return true;
			}
			Character selectedCharacter = Character.Controlled.SelectedCharacter;
			if (((selectedCharacter != null) ? selectedCharacter.Inventory : null) != null && !isSubEditor && Inventory.<DetermineMouseOnInventory>g__IsOnInventorySlot|79_0(Character.Controlled.SelectedCharacter.Inventory))
			{
				return true;
			}
			if (Character.Controlled.SelectedItem != null)
			{
				foreach (ItemComponent ic in Character.Controlled.SelectedItem.ActiveHUDs)
				{
					ItemContainer itemContainer = ic as ItemContainer;
					bool flag;
					if (itemContainer == null)
					{
						flag = (null != null);
					}
					else
					{
						ItemInventory inventory = itemContainer.Inventory;
						flag = (((inventory != null) ? inventory.visualSlots : null) != null);
					}
					if (flag)
					{
						foreach (VisualSlot slot in itemContainer.Inventory.visualSlots)
						{
							if (slot.InteractRect.Contains(PlayerInput.MousePosition) || slot.EquipButtonRect.Contains(PlayerInput.MousePosition))
							{
								return true;
							}
						}
					}
				}
			}
			foreach (Inventory.SlotReference highlightedSubInventorySlot in Inventory.highlightedSubInventorySlots)
			{
				if (Inventory.GetSubInventoryHoverArea(highlightedSubInventorySlot).Contains(PlayerInput.MousePosition))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x000F1254 File Offset: 0x000EF454
		public static CursorState GetInventoryMouseCursor()
		{
			Character character = Character.Controlled;
			if (character == null)
			{
				return CursorState.Default;
			}
			if (Inventory.DraggingItems.Any<Item>() || Inventory.DraggingInventory != null)
			{
				return CursorState.Dragging;
			}
			CharacterInventory inv = character.Inventory;
			Character selectedCharacter = character.SelectedCharacter;
			CharacterInventory selInv = (selectedCharacter != null) ? selectedCharacter.Inventory : null;
			if (inv == null)
			{
				return CursorState.Default;
			}
			foreach (Item item in inv.AllItems)
			{
				ItemContainer container = (item != null) ? item.GetComponent<ItemContainer>() : null;
				if (container != null)
				{
					if (container.Inventory.visualSlots != null)
					{
						if (container.Inventory.visualSlots.Any((VisualSlot slot) => slot.IsHighlighted))
						{
							return CursorState.Hand;
						}
					}
					if (container.Inventory.movableFrameRect.Contains(PlayerInput.MousePosition))
					{
						return CursorState.Move;
					}
				}
			}
			if (selInv != null)
			{
				for (int i = 0; i < selInv.visualSlots.Length; i++)
				{
					VisualSlot slot4 = selInv.visualSlots[i];
					Item item2 = selInv.slots[i].FirstOrDefault();
					if (slot4.InteractRect.Contains(PlayerInput.MousePosition) || (slot4.EquipButtonRect.Contains(PlayerInput.MousePosition) && item2 != null && item2.AllowedSlots.Contains(InvSlotType.Any)))
					{
						return CursorState.Hand;
					}
					ItemContainer container2 = (item2 != null) ? item2.GetComponent<ItemContainer>() : null;
					if (container2 != null && container2.Inventory.visualSlots != null)
					{
						if (container2.Inventory.visualSlots.Any((VisualSlot slot) => slot.IsHighlighted))
						{
							return CursorState.Hand;
						}
					}
				}
			}
			if (character.SelectedItem != null)
			{
				foreach (ItemComponent ic in character.SelectedItem.ActiveHUDs)
				{
					ItemContainer itemContainer = ic as ItemContainer;
					bool flag;
					if (itemContainer == null)
					{
						flag = (null != null);
					}
					else
					{
						ItemInventory inventory = itemContainer.Inventory;
						flag = (((inventory != null) ? inventory.visualSlots : null) != null);
					}
					if (flag && ic.Item.IsInteractable(character))
					{
						foreach (VisualSlot slot2 in itemContainer.Inventory.visualSlots)
						{
							if (slot2.InteractRect.Contains(PlayerInput.MousePosition) || slot2.EquipButtonRect.Contains(PlayerInput.MousePosition))
							{
								return CursorState.Hand;
							}
						}
					}
				}
			}
			for (int j = 0; j < inv.visualSlots.Length; j++)
			{
				VisualSlot slot3 = inv.visualSlots[j];
				Item item3 = inv.slots[j].FirstOrDefault();
				if (slot3.EquipButtonRect.Contains(PlayerInput.MousePosition) && item3 != null && item3.AllowedSlots.Contains(InvSlotType.Any))
				{
					return CursorState.Hand;
				}
				if (slot3.InteractRect.Contains(PlayerInput.MousePosition) && slot3.IsHighlighted)
				{
					return CursorState.Hand;
				}
			}
			return CursorState.Default;
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x000F1590 File Offset: 0x000EF790
		protected static void DrawToolTip(SpriteBatch spriteBatch, RichString toolTip, Rectangle highlightedSlot)
		{
			GUIComponent.DrawToolTip(spriteBatch, toolTip, highlightedSlot, Anchor.BottomRight, Pivot.TopLeft);
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x000F159C File Offset: 0x000EF79C
		public void DrawSubInventory(SpriteBatch spriteBatch, int slotIndex)
		{
			Item item = this.slots[slotIndex].FirstOrDefault();
			if (item == null)
			{
				return;
			}
			ItemContainer container = item.GetComponent<ItemContainer>();
			if (container == null || !container.DrawInventory)
			{
				return;
			}
			if (container.Inventory.visualSlots == null || !container.Inventory.isSubInventory)
			{
				return;
			}
			if (container.Inventory.DrawWhenEquipped)
			{
				return;
			}
			int itemCapacity = container.Capacity;
			if (slotIndex < 0 || slotIndex >= this.capacity)
			{
				return;
			}
			if (!this.canMove)
			{
				Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
				if (this.visualSlots[slotIndex].SubInventoryDir > 0)
				{
					spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(new Point(0, this.visualSlots[slotIndex].Rect.Bottom), new Point(GameMain.GraphicsWidth, Math.Max(GameMain.GraphicsHeight - this.visualSlots[slotIndex].Rect.Bottom, 0)));
				}
				else
				{
					spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(new Point(0, 0), new Point(GameMain.GraphicsWidth, this.visualSlots[slotIndex].Rect.Y));
				}
				container.Inventory.Draw(spriteBatch, true);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
			}
			else
			{
				container.Inventory.Draw(spriteBatch, true);
			}
			Sprite inventoryBottomSprite = container.InventoryBottomSprite;
			if (inventoryBottomSprite != null)
			{
				inventoryBottomSprite.Draw(spriteBatch, new Vector2((float)this.visualSlots[slotIndex].Rect.Center.X, (float)this.visualSlots[slotIndex].Rect.Y) + this.visualSlots[slotIndex].DrawOffset, 0f, Inventory.UIScale, SpriteEffects.None);
			}
			Sprite inventoryTopSprite = container.InventoryTopSprite;
			if (inventoryTopSprite != null)
			{
				inventoryTopSprite.Draw(spriteBatch, new Vector2((float)this.visualSlots[slotIndex].Rect.Center.X, (float)container.Inventory.visualSlots[container.Inventory.visualSlots.Length - 1].Rect.Y) + container.Inventory.visualSlots[container.Inventory.visualSlots.Length - 1].DrawOffset, 0f, Inventory.UIScale, SpriteEffects.None);
			}
			if (container.MovableFrame && !this.IsInventoryHoverAvailable(this.Owner as Character, container))
			{
				if (container.Inventory.positionUpdateQueued)
				{
					int height = (int)(40f * Inventory.UIScale);
					this.CreateSlots();
					container.Inventory.movableFrameRect = new Rectangle(container.Inventory.BackgroundFrame.X, container.Inventory.BackgroundFrame.Y - height, container.Inventory.BackgroundFrame.Width, height);
					this.draggableIndicatorScale = 1.25f * Inventory.UIScale;
					this.draggableIndicatorOffset = Inventory.DraggableIndicator.size * this.draggableIndicatorScale / 2f;
					this.draggableIndicatorOffset += new Vector2((float)height / 2f - this.draggableIndicatorOffset.Y);
					container.Inventory.originalPos = (container.Inventory.savedPosition = container.Inventory.movableFrameRect.Center);
					container.Inventory.positionUpdateQueued = false;
				}
				if (container.Inventory.movableFrameRect.Size == Point.Zero || GUI.HasSizeChanged(this.prevScreenResolution, this.prevUIScale, this.prevHUDScale))
				{
					container.Inventory.savedPosition = container.Inventory.originalPos;
					this.prevScreenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
					this.prevUIScale = Inventory.UIScale;
					this.prevHUDScale = GUI.Scale;
					container.Inventory.positionUpdateQueued = true;
					return;
				}
				Color color = this.movableFrameRectColor;
				if (Inventory.DraggingInventory != null && Inventory.DraggingInventory != container.Inventory)
				{
					color *= 0.7f;
				}
				else if (container.Inventory.movableFrameRect.Contains(PlayerInput.MousePosition))
				{
					color = Color.Lerp(color, PlayerInput.PrimaryMouseButtonHeld() ? Color.Black : Color.White, 0.25f);
				}
				GUI.DrawRectangle(spriteBatch, container.Inventory.movableFrameRect, color, true, 0f, 1f);
				Inventory.DraggableIndicator.Draw(spriteBatch, container.Inventory.movableFrameRect.Location.ToVector2() + this.draggableIndicatorOffset, 0f, this.draggableIndicatorScale, SpriteEffects.None);
			}
		}

		// Token: 0x06001837 RID: 6199 RVA: 0x000F1A64 File Offset: 0x000EFC64
		public static void UpdateDragging()
		{
			if (Screen.Selected == GameMain.GameScreen)
			{
				Inventory.DraggingItems.RemoveAll((Item it) => !Character.Controlled.CanInteractWith(it, true));
			}
			if (Inventory.DraggingItems.Any<Item>() && PlayerInput.PrimaryMouseButtonReleased())
			{
				Character.Controlled.ClearInputs();
				bool mouseOnPortrait = CharacterHUD.MouseOnCharacterPortrait();
				if (!Inventory.DetermineMouseOnInventory(true) && (CharacterHealth.OpenHealthWindow != null || mouseOnPortrait) && Inventory.<UpdateDragging>g__TryPortraitAndHealthDrop|83_1(mouseOnPortrait))
				{
					return;
				}
				if (Inventory.selectedSlot == null)
				{
					Inventory.<UpdateDragging>g__HandleOutsideInventoryDrop|83_2();
				}
				else if (!Inventory.DraggingItems.Any((Item it) => Inventory.selectedSlot.ParentInventory.slots[Inventory.selectedSlot.SlotIndex].Contains(it)))
				{
					Inventory.<UpdateDragging>g__HandleInventorySlotDrop|83_3();
				}
				Inventory.DraggingItems.Clear();
			}
			if (Inventory.selectedSlot != null && !Inventory.CanSelectSlot(Inventory.selectedSlot))
			{
				Inventory.selectedSlot = null;
			}
		}

		// Token: 0x06001838 RID: 6200 RVA: 0x000F1B48 File Offset: 0x000EFD48
		private static bool IsValidTargetForDragDropGive(Character giver, Character receiver, IEnumerable<Item> draggedItems)
		{
			if (giver == null || receiver == null || draggedItems.None(null))
			{
				return false;
			}
			if (receiver == giver)
			{
				return false;
			}
			CharacterInventory.AccessLevel accessLevel;
			if (draggedItems.Any((Item it) => it.HasTag(Tags.HandLockerItem)))
			{
				accessLevel = CharacterInventory.AccessLevel.AllowBotsAndPets;
			}
			else
			{
				accessLevel = (Inventory.IsDragAndDropGiveAllowed ? CharacterInventory.AccessLevel.AllowFriendly : CharacterInventory.AccessLevel.AllowBotsAndPets);
			}
			return receiver.IsInventoryAccessibleTo(giver, accessLevel) && receiver.Inventory.CanBePut(draggedItems.FirstOrDefault<Item>(), InvSlotType.Any);
		}

		// Token: 0x06001839 RID: 6201 RVA: 0x000F1BC4 File Offset: 0x000EFDC4
		private static bool CanSelectSlot(Inventory.SlotReference selectedSlot)
		{
			if (!Inventory.IsMouseOnInventory)
			{
				return false;
			}
			if (!selectedSlot.Slot.MouseOn())
			{
				return false;
			}
			Inventory parentInventory = selectedSlot.ParentInventory;
			Entity owner = (parentInventory != null) ? parentInventory.Owner : null;
			Item item = owner as Item;
			Entity rootOwner = (item != null) ? item.GetRootInventoryOwner() : null;
			if (Inventory.<CanSelectSlot>g__OwnerInaccessible|85_0(owner) && (rootOwner == owner || Inventory.<CanSelectSlot>g__OwnerInaccessible|85_0(rootOwner)))
			{
				return false;
			}
			Item parentItem = (owner as Item) ?? ((selectedSlot != null) ? selectedSlot.Item : null);
			Item parentItem2 = parentItem;
			Character ownerCharacter = ((parentItem2 != null) ? parentItem2.GetRootInventoryOwner() : null) as Character;
			if (ownerCharacter != null && ownerCharacter == Character.Controlled)
			{
				CharacterHealth openHealthWindow = CharacterHealth.OpenHealthWindow;
				if (((openHealthWindow != null) ? openHealthWindow.Character : null) != ownerCharacter && ownerCharacter.Inventory.IsInLimbSlot(parentItem, InvSlotType.HealthInterface) && Screen.Selected != GameMain.SubEditorScreen)
				{
					Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.Item == parentItem);
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600183A RID: 6202 RVA: 0x000F1CBC File Offset: 0x000EFEBC
		protected static Rectangle GetSubInventoryHoverArea(Inventory.SlotReference subSlot)
		{
			if (Character.Controlled == null)
			{
				return Rectangle.Empty;
			}
			bool flag;
			if (subSlot.Inventory.Movable())
			{
				Inventory parentInventory = subSlot.ParentInventory;
				Character controlled = Character.Controlled;
				Item item = subSlot.Item;
				flag = !parentInventory.IsInventoryHoverAvailable(controlled, (item != null) ? item.GetComponent<ItemContainer>() : null);
			}
			else
			{
				flag = false;
			}
			bool isMovable = flag;
			bool unEquipped = Character.Controlled.Inventory == subSlot.ParentInventory && !Character.Controlled.HasEquippedItem(subSlot.Item, null, null);
			CharacterInventory characterInventory = subSlot.ParentInventory as CharacterInventory;
			bool isDefaultLayout = characterInventory == null || characterInventory.CurrentLayout == CharacterInventory.Layout.Default;
			Rectangle hoverArea;
			if ((Screen.Selected == GameMain.SubEditorScreen && !GameMain.SubEditorScreen.DrawCharacterInventory) || (isMovable && !unEquipped && isDefaultLayout))
			{
				hoverArea = subSlot.Inventory.BackgroundFrame;
				hoverArea.Location += subSlot.Slot.DrawOffset.ToPoint();
				if (subSlot.Inventory.movableFrameRect != Rectangle.Empty)
				{
					hoverArea = Rectangle.Union(hoverArea, subSlot.Inventory.movableFrameRect);
				}
			}
			else
			{
				hoverArea = subSlot.Slot.Rect;
				hoverArea.Location += subSlot.Slot.DrawOffset.ToPoint();
				hoverArea = Rectangle.Union(hoverArea, subSlot.Slot.EquipButtonRect);
			}
			Inventory inventory = subSlot.Inventory;
			if (((inventory != null) ? inventory.visualSlots : null) != null)
			{
				foreach (VisualSlot slot in subSlot.Inventory.visualSlots)
				{
					Rectangle subSlotRect = slot.InteractRect;
					subSlotRect.Location += slot.DrawOffset.ToPoint();
					hoverArea = Rectangle.Union(hoverArea, subSlotRect);
				}
				if (subSlot.Slot.SubInventoryDir >= 0)
				{
					int over = subSlot.Slot.Rect.Y - hoverArea.Y;
					hoverArea.Y += over;
					hoverArea.Height -= over;
				}
			}
			float inflateAmount = 10f * Inventory.UIScale;
			hoverArea.Inflate(inflateAmount, inflateAmount);
			return hoverArea;
		}

		// Token: 0x0600183B RID: 6203 RVA: 0x000F1EF0 File Offset: 0x000F00F0
		public static void DrawFront(SpriteBatch spriteBatch)
		{
			Inventory.<>c__DisplayClass87_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (GUI.PauseMenuOpen || GUI.SettingsMenuOpen)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) != null && (GameMain.GameSession.Campaign.ShowCampaignUI || GameMain.GameSession.Campaign.ForceMapUI))
			{
				return;
			}
			Inventory.subInventorySlotsToDraw.Clear();
			Inventory.subInventorySlotsToDraw.AddRange(Inventory.highlightedSubInventorySlots);
			foreach (Inventory.SlotReference slot in Inventory.subInventorySlotsToDraw)
			{
				int slotIndex = Array.IndexOf<VisualSlot>(slot.ParentInventory.visualSlots, slot.Slot);
				if (slotIndex > -1 && slotIndex < slot.ParentInventory.visualSlots.Length)
				{
					Item item = slot.Item;
					bool? flag;
					if (item == null)
					{
						flag = null;
					}
					else
					{
						ItemContainer component = item.GetComponent<ItemContainer>();
						flag = ((component != null) ? new bool?(component.HasRequiredItems(Character.Controlled, false, null)) : null);
					}
					bool? flag2 = flag;
					if (flag2.GetValueOrDefault(true))
					{
						slot.ParentInventory.DrawSubInventory(CS$<>8__locals1.spriteBatch, slotIndex);
					}
				}
			}
			if (Inventory.DraggingItems.Any<Item>())
			{
				Inventory.<DrawFront>g__DrawDragRelated|87_0(ref CS$<>8__locals1);
			}
			if (Inventory.selectedSlot != null && Inventory.selectedSlot.Item != null)
			{
				Rectangle slotRect = Inventory.selectedSlot.Slot.Rect;
				slotRect.Location += Inventory.selectedSlot.Slot.DrawOffset.ToPoint();
				if (Inventory.selectedSlot.TooltipNeedsRefresh())
				{
					Inventory.selectedSlot.RefreshTooltip();
				}
				if (!Inventory.slotIconTooltip.IsNullOrEmpty())
				{
					Inventory.DrawToolTip(CS$<>8__locals1.spriteBatch, Inventory.slotIconTooltip, slotRect);
				}
				else
				{
					Inventory.DrawToolTip(CS$<>8__locals1.spriteBatch, Inventory.selectedSlot.Tooltip, slotRect);
				}
				Inventory.slotIconTooltip = string.Empty;
			}
		}

		// Token: 0x0600183C RID: 6204 RVA: 0x000F20F0 File Offset: 0x000F02F0
		public static void DrawSlot(SpriteBatch spriteBatch, Inventory inventory, VisualSlot slot, Item item, int slotIndex, bool drawItem = true, InvSlotType type = InvSlotType.Any)
		{
			Inventory.<>c__DisplayClass90_0 CS$<>8__locals1 = new Inventory.<>c__DisplayClass90_0();
			CS$<>8__locals1.slot = slot;
			CS$<>8__locals1.slotIndex = slotIndex;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.rect = CS$<>8__locals1.slot.Rect;
			Inventory.<>c__DisplayClass90_0 CS$<>8__locals2 = CS$<>8__locals1;
			CS$<>8__locals2.rect.Location = CS$<>8__locals2.rect.Location + CS$<>8__locals1.slot.DrawOffset.ToPoint();
			if (CS$<>8__locals1.slot.HighlightColor.A > 0)
			{
				float inflateAmount = (float)CS$<>8__locals1.slot.HighlightColor.A / 255f * CS$<>8__locals1.slot.HighlightScaleUpAmount * 0.5f;
				CS$<>8__locals1.rect.Inflate((float)CS$<>8__locals1.rect.Width * inflateAmount, (float)CS$<>8__locals1.rect.Height * inflateAmount);
			}
			Color slotColor = Color.White;
			Item parentItem = ((inventory != null) ? inventory.Owner : null) as Item;
			if (parentItem != null && !parentItem.IsPlayerTeamInteractable)
			{
				slotColor = Color.Gray;
			}
			ItemContainer itemContainer = (item != null) ? item.GetComponent<ItemContainer>() : null;
			if (itemContainer != null && (itemContainer.InventoryTopSprite != null || itemContainer.InventoryBottomSprite != null))
			{
				if (!Inventory.highlightedSubInventorySlots.Any((Inventory.SlotReference s) => s.Slot == CS$<>8__locals1.slot))
				{
					Sprite inventoryBottomSprite = itemContainer.InventoryBottomSprite;
					if (inventoryBottomSprite != null)
					{
						inventoryBottomSprite.Draw(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.rect.Center.X, (float)CS$<>8__locals1.rect.Y), 0f, Inventory.UIScale, SpriteEffects.None);
					}
					Sprite inventoryTopSprite = itemContainer.InventoryTopSprite;
					if (inventoryTopSprite != null)
					{
						inventoryTopSprite.Draw(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.rect.Center.X, (float)CS$<>8__locals1.rect.Y), 0f, Inventory.UIScale, SpriteEffects.None);
					}
				}
				drawItem = false;
			}
			else
			{
				Sprite slotSprite = CS$<>8__locals1.slot.SlotSprite ?? Inventory.SlotSpriteSmall;
				if (inventory != null && inventory.Locked)
				{
					slotColor = Color.Gray * 0.5f;
				}
				CS$<>8__locals1.spriteBatch.Draw(slotSprite.Texture, CS$<>8__locals1.rect, new Rectangle?(slotSprite.SourceRect), slotColor);
				if (SubEditorScreen.IsSubEditor() && PlayerInput.IsCtrlDown())
				{
					Inventory.SlotReference slotReference = Inventory.selectedSlot;
					if (((slotReference != null) ? slotReference.Slot : null) == CS$<>8__locals1.slot)
					{
						GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, GUIStyle.Red * 0.3f, true, 0f, 1f);
					}
				}
				bool canBePut = false;
				if (Inventory.DraggingItems.Any<Item>() && inventory != null && CS$<>8__locals1.slotIndex > -1 && CS$<>8__locals1.slotIndex < inventory.visualSlots.Length)
				{
					Item itemInSlot = inventory.slots[CS$<>8__locals1.slotIndex].FirstOrDefault();
					if (inventory.CanBePutInSlot(Inventory.DraggingItems.First<Item>(), CS$<>8__locals1.slotIndex, false))
					{
						canBePut = true;
					}
					else if (((itemInSlot != null) ? itemInSlot.OwnInventory : null) != null && itemInSlot.OwnInventory.CanBePut(Inventory.DraggingItems.First<Item>()) && itemInSlot.OwnInventory.Container.AllowDragAndDrop && itemInSlot.OwnInventory.Container.DrawInventory)
					{
						canBePut = true;
					}
					else if (inventory.slots[CS$<>8__locals1.slotIndex] == null && inventory == Character.Controlled.Inventory && !Inventory.DraggingItems.First<Item>().AllowedSlots.Any((InvSlotType a) => a.HasFlag(Character.Controlled.Inventory.SlotTypes[CS$<>8__locals1.slotIndex])) && Character.Controlled.Inventory.CanBeAutoMovedToCorrectSlots(Inventory.DraggingItems.First<Item>()))
					{
						canBePut = true;
					}
				}
				if (CS$<>8__locals1.slot.MouseOn() && canBePut)
				{
					Inventory.SlotReference slotReference2 = Inventory.selectedSlot;
					if (((slotReference2 != null) ? slotReference2.Slot : null) == CS$<>8__locals1.slot)
					{
						GUIStyle.UIGlow.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, GUIStyle.Green, SpriteEffects.None);
					}
				}
				if (item != null && drawItem)
				{
					if (!item.IsFullCondition && !item.Prefab.HideConditionBar && (itemContainer == null || !itemContainer.ShowConditionInContainedStateIndicator))
					{
						int dir = CS$<>8__locals1.slot.SubInventoryDir;
						Rectangle conditionIndicatorArea;
						if (itemContainer != null && itemContainer.ShowContainedStateIndicator)
						{
							conditionIndicatorArea = new Rectangle(CS$<>8__locals1.rect.X, CS$<>8__locals1.rect.Bottom - (int)(10f * GUI.Scale), CS$<>8__locals1.rect.Width, (int)(10f * GUI.Scale));
						}
						else
						{
							conditionIndicatorArea = new Rectangle(CS$<>8__locals1.rect.X, (dir < 0) ? (CS$<>8__locals1.rect.Bottom + HUDLayoutSettings.Padding / 2) : (CS$<>8__locals1.rect.Y - HUDLayoutSettings.Padding / 2 - Inventory.ContainedIndicatorHeight), CS$<>8__locals1.rect.Width, Inventory.ContainedIndicatorHeight);
							conditionIndicatorArea.Inflate(-4, 0);
						}
						GUIComponentStyle indicatorStyle = GUIStyle.GetComponentStyle("ContainedStateIndicator.Default");
						Sprite indicatorSprite = (indicatorStyle != null) ? indicatorStyle.GetDefaultSprite() : null;
						Sprite emptyIndicatorSprite = (indicatorStyle != null) ? indicatorStyle.GetSprite(GUIComponent.ComponentState.Hover) : null;
						Inventory.DrawItemStateIndicator(CS$<>8__locals1.spriteBatch, inventory, indicatorSprite, emptyIndicatorSprite, conditionIndicatorArea, item.Condition / item.MaxCondition, false);
					}
					if (itemContainer != null && itemContainer.ShowContainedStateIndicator && itemContainer.Capacity > 0)
					{
						float containedState = itemContainer.GetContainedIndicatorState();
						int dir2 = CS$<>8__locals1.slot.SubInventoryDir;
						Rectangle containedIndicatorArea = new Rectangle(CS$<>8__locals1.rect.X, (dir2 < 0) ? (CS$<>8__locals1.rect.Bottom + HUDLayoutSettings.Padding / 2) : (CS$<>8__locals1.rect.Y - HUDLayoutSettings.Padding / 2 - Inventory.ContainedIndicatorHeight), CS$<>8__locals1.rect.Width, Inventory.ContainedIndicatorHeight);
						containedIndicatorArea.Inflate(-4, 0);
						Sprite sprite2;
						if ((sprite2 = itemContainer.ContainedStateIndicator) == null)
						{
							GUIComponentStyle indicatorStyle2 = itemContainer.IndicatorStyle;
							sprite2 = ((indicatorStyle2 != null) ? indicatorStyle2.GetDefaultSprite() : null);
						}
						Sprite indicatorSprite2 = sprite2;
						Sprite sprite3;
						if ((sprite3 = itemContainer.ContainedStateIndicatorEmpty) == null)
						{
							GUIComponentStyle indicatorStyle3 = itemContainer.IndicatorStyle;
							sprite3 = ((indicatorStyle3 != null) ? indicatorStyle3.GetSprite(GUIComponent.ComponentState.Hover) : null);
						}
						Sprite emptyIndicatorSprite2 = sprite3;
						GUIComponentStyle indicatorStyle4 = itemContainer.IndicatorStyle;
						bool usingDefaultSprite = ((indicatorStyle4 != null) ? indicatorStyle4.Name : null) == "ContainedStateIndicator.Default";
						SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
						Sprite indicatorSprite3 = indicatorSprite2;
						Sprite emptyIndicatorSprite3 = emptyIndicatorSprite2;
						Rectangle containedIndicatorArea2 = containedIndicatorArea;
						float containedState2 = containedState;
						bool pulsate;
						if (!usingDefaultSprite && containedState >= 0f && containedState < 0.25f)
						{
							Character controlled = Character.Controlled;
							if (inventory == ((controlled != null) ? controlled.Inventory : null))
							{
								pulsate = Character.Controlled.HasEquippedItem(item, null, null);
								goto IL_63B;
							}
						}
						pulsate = false;
						IL_63B:
						Inventory.DrawItemStateIndicator(spriteBatch2, inventory, indicatorSprite3, emptyIndicatorSprite3, containedIndicatorArea2, containedState2, pulsate);
					}
					if (item.Quality != 0)
					{
						GUIComponentStyle style = GUIStyle.GetComponentStyle("InnerGlowSmall");
						if (style == null)
						{
							GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, GUIStyle.GetQualityColor(item.Quality) * 0.7f, false, 0f, 1f);
						}
						else
						{
							UISprite uisprite = style.Sprites[GUIComponent.ComponentState.None].FirstOrDefault<UISprite>();
							if (uisprite != null)
							{
								uisprite.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, GUIStyle.GetQualityColor(item.Quality) * 0.5f, SpriteEffects.None, null);
							}
						}
					}
				}
				else
				{
					Sprite sprite4;
					if (parentItem == null)
					{
						sprite4 = null;
					}
					else
					{
						ItemContainer component = parentItem.GetComponent<ItemContainer>();
						sprite4 = ((component != null) ? component.GetSlotIcon(CS$<>8__locals1.slotIndex) : null);
					}
					Sprite slotIcon = sprite4;
					if (slotIcon != null)
					{
						slotIcon.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect.Center.ToVector2(), GUIStyle.EquipmentSlotIconColor, 0f, Math.Min((float)CS$<>8__locals1.rect.Width / slotIcon.size.X, (float)CS$<>8__locals1.rect.Height / slotIcon.size.Y) * 0.8f, SpriteEffects.None, null);
					}
				}
			}
			if (GameMain.DebugDraw)
			{
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, Color.White, false, 0f, 1f);
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.slot.EquipButtonRect, Color.White, false, 0f, 1f);
			}
			if (CS$<>8__locals1.slot.HighlightColor != Color.Transparent)
			{
				GUIStyle.UIGlow.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.rect, CS$<>8__locals1.slot.HighlightColor, SpriteEffects.None);
			}
			if (item != null && drawItem)
			{
				Sprite sprite5;
				if ((sprite5 = item.OverrideInventorySprite) == null)
				{
					sprite5 = (item.Prefab.InventoryIcon ?? item.Sprite);
				}
				Sprite sprite = sprite5;
				float scale = Math.Min(Math.Min((float)(CS$<>8__locals1.rect.Width - 10) / sprite.size.X, (float)(CS$<>8__locals1.rect.Height - 10) / sprite.size.Y), 2f);
				Vector2 itemPos = CS$<>8__locals1.rect.Center.ToVector2();
				if (itemPos.Y > (float)GameMain.GraphicsHeight)
				{
					itemPos.Y -= Math.Min(itemPos.Y + sprite.size.Y / 2f * scale - (float)GameMain.GraphicsHeight, itemPos.Y - sprite.size.Y / 2f * scale - (float)CS$<>8__locals1.rect.Y);
				}
				float rotation = 0f;
				if (CS$<>8__locals1.slot.HighlightColor.A > 0)
				{
					rotation = (float)Math.Sin((double)(CS$<>8__locals1.slot.HighlightTimer * 6.2831855f)) * CS$<>8__locals1.slot.HighlightTimer * 0.3f;
				}
				Color spriteColor = (sprite == item.Sprite) ? item.GetSpriteColor(null, false) : item.GetInventoryIconColor();
				if (inventory != null)
				{
					if (!inventory.Locked)
					{
						if (!inventory.slots[CS$<>8__locals1.slotIndex].Items.All((Item it) => !it.IsInteractable(Character.Controlled)))
						{
							goto IL_9BA;
						}
					}
					spriteColor *= 0.5f;
				}
				IL_9BA:
				if (CharacterHealth.OpenHealthWindow != null && !item.UseInHealthInterface && !item.AllowedSlots.Contains(InvSlotType.HealthInterface) && item.GetComponent<GeneticMaterial>() == null)
				{
					spriteColor = Color.Lerp(spriteColor, Color.TransparentBlack, 0.5f);
				}
				else
				{
					sprite.Draw(CS$<>8__locals1.spriteBatch, itemPos + Vector2.One * 2f, Color.Black * 0.6f, rotation, scale, SpriteEffects.None, null);
				}
				sprite.Draw(CS$<>8__locals1.spriteBatch, itemPos, spriteColor, rotation, scale, SpriteEffects.None, null);
				OrderPrefab deconstructOrder;
				if (item.OrderedToBeIgnored)
				{
					OrderPrefab ignoreOrder;
					if (OrderPrefab.Prefabs.TryGet(Tags.IgnoreThis, out ignoreOrder))
					{
						bool mouseOn;
						CS$<>8__locals1.<DrawSlot>g__DrawSideIcon|1(ignoreOrder.SymbolSprite, Direction.Right, TextManager.Get("tooltip.ignored"), ignoreOrder.Color, out mouseOn);
						if (mouseOn)
						{
							Inventory.availableContextualOrder = new ValueTuple<Item, Identifier>(item, Tags.UnignoreThis);
						}
					}
				}
				else if (Item.DeconstructItems.Contains(item) && OrderPrefab.Prefabs.TryGet(Tags.DeconstructThis, out deconstructOrder))
				{
					bool mouseOn2;
					CS$<>8__locals1.<DrawSlot>g__DrawSideIcon|1(deconstructOrder.SymbolSprite, Direction.Right, TextManager.Get("tooltip.markedfordeconstruction"), GUIStyle.Red, out mouseOn2);
					if (mouseOn2)
					{
						Inventory.availableContextualOrder = new ValueTuple<Item, Identifier>(item, Tags.DontDeconstructThis);
					}
				}
				else
				{
					if (!item.Illegitimate)
					{
						if (inventory == null)
						{
							goto IL_B89;
						}
						if (!inventory.slots[CS$<>8__locals1.slotIndex].Items.Any((Item it) => it.Illegitimate))
						{
							goto IL_B89;
						}
					}
					if (CharacterInventory.LimbSlotIcons.ContainsKey(InvSlotType.LeftHand))
					{
						bool flag;
						CS$<>8__locals1.<DrawSlot>g__DrawSideIcon|1(CharacterInventory.LimbSlotIcons[InvSlotType.LeftHand], Direction.Left, TextManager.Get("tooltip.stolenitem"), GUIStyle.Red, out flag);
					}
				}
				IL_B89:
				int maxStackSize = item.Prefab.GetMaxStackSize(inventory);
				ItemInventory itemInventory = inventory as ItemInventory;
				if (itemInventory != null)
				{
					maxStackSize = Math.Min(maxStackSize, itemInventory.Container.GetMaxStackSize(CS$<>8__locals1.slotIndex));
				}
				if (maxStackSize > 1 && inventory != null)
				{
					int num;
					if (!CS$<>8__locals1.slot.MouseOn())
					{
						num = (from it in inventory.slots[CS$<>8__locals1.slotIndex].Items
						where !Inventory.DraggingItems.Contains(it)
						select it).Count<Item>();
					}
					else
					{
						num = inventory.slots[CS$<>8__locals1.slotIndex].Items.Count;
					}
					int itemCount = num;
					if (item.IsFullCondition || MathUtils.NearlyEqual(item.Condition, 0f, 0.0001f) || itemCount > 1)
					{
						Vector2 stackCountPos = new Vector2((float)CS$<>8__locals1.rect.Right, (float)CS$<>8__locals1.rect.Bottom);
						string stackCountText = "x" + itemCount.ToString();
						stackCountPos -= GUIStyle.SmallFont.MeasureString(stackCountText, false) + new Vector2(4f, 2f);
						GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, stackCountText, stackCountPos + Vector2.One, Color.Black, ForceUpperCase.Inherit, false);
						GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, stackCountText, stackCountPos, Color.White, ForceUpperCase.Inherit, false);
					}
				}
				if (HealingCooldown.IsOnCooldown && item.HasTag(Tags.MedicalItem))
				{
					RectangleF cdRect = CS$<>8__locals1.rect;
					cdRect.Height *= HealingCooldown.NormalizedCooldown;
					cdRect.Y += (float)CS$<>8__locals1.rect.Height;
					GUI.DrawFilledRectangle(CS$<>8__locals1.spriteBatch, cdRect, Color.White * 0.5f, 0f);
				}
			}
			if (inventory != null && !inventory.Locked)
			{
				Character controlled2 = Character.Controlled;
				if (((controlled2 != null) ? controlled2.Inventory : null) == inventory && CS$<>8__locals1.slot.InventoryKeyIndex != -1 && CS$<>8__locals1.slot.InventoryKeyIndex < GameSettings.CurrentConfig.InventoryKeyMap.Bindings.Length)
				{
					CS$<>8__locals1.spriteBatch.Draw(Inventory.slotHotkeySprite.Texture, CS$<>8__locals1.rect.ScaleSize(1.15f), new Rectangle?(Inventory.slotHotkeySprite.SourceRect), slotColor);
					GUIStyle.HotkeyFont.DrawString(CS$<>8__locals1.spriteBatch, GameSettings.CurrentConfig.InventoryKeyMap.Bindings[CS$<>8__locals1.slot.InventoryKeyIndex].Name, CS$<>8__locals1.rect.Location.ToVector2() + new Vector2((float)((int)(4.25f * Inventory.UIScale)), (float)((int)Math.Ceiling((double)(-1.5f * Inventory.UIScale)))), Color.Black, 0f, Vector2.Zero, Vector2.One * GUI.AspectRatioAdjustment, SpriteEffects.None, 0f);
				}
			}
		}

		// Token: 0x0600183D RID: 6205 RVA: 0x000F2F88 File Offset: 0x000F1188
		private static void DrawItemStateIndicator(SpriteBatch spriteBatch, Inventory inventory, Sprite indicatorSprite, Sprite emptyIndicatorSprite, Rectangle containedIndicatorArea, float containedState, bool pulsate = false)
		{
			Color backgroundColor = GUIStyle.ColorInventoryBackground;
			if (indicatorSprite == null)
			{
				containedIndicatorArea.Inflate(0, -2);
				GUI.DrawRectangle(spriteBatch, containedIndicatorArea, backgroundColor, true, 0f, 1f);
				GUI.DrawRectangle(spriteBatch, new Rectangle(containedIndicatorArea.X, containedIndicatorArea.Y, (int)((float)containedIndicatorArea.Width * containedState), containedIndicatorArea.Height), ToolBox.GradientLerp(containedState, new Color[]
				{
					GUIStyle.ColorInventoryEmpty,
					GUIStyle.ColorInventoryHalf,
					GUIStyle.ColorInventoryFull
				}) * 0.8f, true, 0f, 1f);
				GUI.DrawLine(spriteBatch, new Vector2((float)(containedIndicatorArea.X + (int)((float)containedIndicatorArea.Width * containedState)), (float)containedIndicatorArea.Y), new Vector2((float)(containedIndicatorArea.X + (int)((float)containedIndicatorArea.Width * containedState)), (float)containedIndicatorArea.Bottom), Color.Black * 0.8f, 0f, 1f);
				return;
			}
			float indicatorScale = Math.Min((float)containedIndicatorArea.Width / (float)indicatorSprite.SourceRect.Width, (float)containedIndicatorArea.Height / (float)indicatorSprite.SourceRect.Height);
			if (pulsate)
			{
				indicatorScale += ((float)Math.Sin(Timing.TotalTime * 5.0) + 1f) * 0.2f;
			}
			indicatorSprite.Draw(spriteBatch, containedIndicatorArea.Center.ToVector2(), (inventory != null && inventory.Locked) ? (backgroundColor * 0.5f) : backgroundColor, indicatorSprite.size / 2f, 0f, indicatorScale, SpriteEffects.None, null);
			if (containedState > 0f)
			{
				Color indicatorColor = ToolBox.GradientLerp(containedState, new Color[]
				{
					GUIStyle.ColorInventoryEmpty,
					GUIStyle.ColorInventoryHalf,
					GUIStyle.ColorInventoryFull
				});
				if (inventory != null && inventory.Locked)
				{
					indicatorColor *= 0.5f;
				}
				spriteBatch.Draw(indicatorSprite.Texture, containedIndicatorArea.Center.ToVector2(), new Rectangle?(new Rectangle(indicatorSprite.SourceRect.Location, new Point((int)((float)indicatorSprite.SourceRect.Width * containedState), indicatorSprite.SourceRect.Height))), indicatorColor, 0f, indicatorSprite.size / 2f, indicatorScale, SpriteEffects.None, 0f);
				spriteBatch.Draw(indicatorSprite.Texture, containedIndicatorArea.Center.ToVector2(), new Rectangle?(new Rectangle(indicatorSprite.SourceRect.X - 1 + (int)((float)indicatorSprite.SourceRect.Width * containedState), indicatorSprite.SourceRect.Y, Math.Max((int)Math.Ceiling((double)(1f / indicatorScale)), 2), indicatorSprite.SourceRect.Height)), Color.Black, 0f, new Vector2(indicatorSprite.size.X * (0.5f - containedState), indicatorSprite.size.Y * 0.5f), indicatorScale, SpriteEffects.None, 0f);
				return;
			}
			if (emptyIndicatorSprite != null)
			{
				Color indicatorColor2 = GUIStyle.ColorInventoryEmptyOverlay;
				if (inventory != null && inventory.Locked)
				{
					indicatorColor2 *= 0.5f;
				}
				emptyIndicatorSprite.Draw(spriteBatch, containedIndicatorArea.Center.ToVector2(), indicatorColor2, emptyIndicatorSprite.size / 2f, 0f, indicatorScale, SpriteEffects.None, null);
			}
		}

		// Token: 0x0600183E RID: 6206 RVA: 0x000F3334 File Offset: 0x000F1534
		public void ClientEventRead(IReadMessage msg)
		{
			ushort lastEventID = msg.ReadUInt16();
			if (this.partialReceivedItemIDs == null)
			{
				this.partialReceivedItemIDs = new List<ushort>[this.capacity];
			}
			bool readyToApply;
			this.SharedRead(msg, this.partialReceivedItemIDs, out readyToApply);
			if (!readyToApply)
			{
				return;
			}
			this.receivedItemIDs = this.partialReceivedItemIDs.ToArray<List<ushort>>();
			this.partialReceivedItemIDs = null;
			if (this.syncItemsDelay > 0f || GameMain.Client.MidRoundSyncing || NetIdUtils.IdMoreRecent(lastEventID, GameMain.Client.EntityEventManager.LastReceivedID))
			{
				if (this.syncItemsCoroutine != null)
				{
					CoroutineManager.StopCoroutines(this.syncItemsCoroutine);
				}
				this.syncItemsCoroutine = CoroutineManager.StartCoroutine(this.SyncItemsAfterDelay(lastEventID), "");
				return;
			}
			if (this.syncItemsCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.syncItemsCoroutine);
				this.syncItemsCoroutine = null;
			}
			this.ApplyReceivedState();
		}

		// Token: 0x0600183F RID: 6207 RVA: 0x000F3407 File Offset: 0x000F1607
		private IEnumerable<CoroutineStatus> SyncItemsAfterDelay(ushort lastEventID)
		{
			Inventory.<SyncItemsAfterDelay>d__93 <SyncItemsAfterDelay>d__ = new Inventory.<SyncItemsAfterDelay>d__93(-2);
			<SyncItemsAfterDelay>d__.<>4__this = this;
			<SyncItemsAfterDelay>d__.<>3__lastEventID = lastEventID;
			return <SyncItemsAfterDelay>d__;
		}

		// Token: 0x06001840 RID: 6208 RVA: 0x000F3420 File Offset: 0x000F1620
		public void ApplyReceivedState()
		{
			if (this.receivedItemIDs == null || (this.Owner != null && this.Owner.Removed))
			{
				return;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				foreach (Item item in this.slots[i].Items.ToList<Item>())
				{
					if (!this.receivedItemIDs[i].Contains(item.ID))
					{
						item.Drop(null, true, true);
					}
				}
			}
			for (int j = this.capacity - 1; j >= 0; j--)
			{
				if (this.receivedItemIDs[j].Any<ushort>())
				{
					foreach (ushort id in this.receivedItemIDs[j])
					{
						Item item2 = Entity.FindEntityByID(id) as Item;
						if (item2 != null && !this.slots[j].Contains(item2))
						{
							Item thisItem = this.Owner as Item;
							if (thisItem != null && thisItem.Container == item2)
							{
								thisItem.Drop(null, true, true);
							}
							if (!this.TryPutItem(item2, j, false, false, null, false, false, true))
							{
								try
								{
									this.ForceToSlot(item2, j);
								}
								catch (InvalidOperationException e)
								{
									DebugConsole.AddSafeError(e.Message + "\n" + e.StackTrace.CleanupStackTrace());
								}
							}
							for (int k = 0; k < this.capacity; k++)
							{
								if (this.slots[k].Contains(item2) && !this.receivedItemIDs[k].Contains(item2.ID))
								{
									this.slots[k].RemoveItem(item2);
								}
							}
						}
					}
				}
			}
			this.receivedItemIDs = null;
		}

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x06001841 RID: 6209 RVA: 0x000F3628 File Offset: 0x000F1828
		// (set) Token: 0x06001842 RID: 6210 RVA: 0x000F3630 File Offset: 0x000F1830
		public int ExtraStackSize
		{
			get
			{
				return this.extraStackSize;
			}
			set
			{
				this.extraStackSize = MathHelper.Max(value, 0);
			}
		}

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x06001843 RID: 6211 RVA: 0x000F363F File Offset: 0x000F183F
		public virtual IEnumerable<Item> AllItems
		{
			get
			{
				return this.GetAllItems(this is CharacterInventory);
			}
		}

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001844 RID: 6212 RVA: 0x000F3650 File Offset: 0x000F1850
		public IEnumerable<Item> AllItemsMod
		{
			get
			{
				this.allItemsList.Clear();
				this.allItemsList.AddRange(this.AllItems);
				return this.allItemsList;
			}
		}

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x06001845 RID: 6213 RVA: 0x000F3674 File Offset: 0x000F1874
		public int Capacity
		{
			get
			{
				return this.capacity;
			}
		}

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x06001846 RID: 6214 RVA: 0x000F367C File Offset: 0x000F187C
		public static bool IsDragAndDropGiveAllowed
		{
			get
			{
				return GameMain.NetworkMember == null || GameMain.NetworkMember.ServerSettings.AllowDragAndDropGive;
			}
		}

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x06001847 RID: 6215 RVA: 0x000F3696 File Offset: 0x000F1896
		public int EmptySlotCount
		{
			get
			{
				return this.slots.Count((Inventory.ItemSlot i) => !i.Empty());
			}
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x000F36C4 File Offset: 0x000F18C4
		public Inventory(Entity owner, int capacity, int slotsPerRow = 5)
		{
			this.capacity = capacity;
			this.Owner = owner;
			this.slots = new Inventory.ItemSlot[capacity];
			for (int i = 0; i < capacity; i++)
			{
				this.slots[i] = new Inventory.ItemSlot(this);
			}
			this.slotsPerRow = slotsPerRow;
			if (Inventory.DraggableIndicator == null)
			{
				Inventory.DraggableIndicator = GUIStyle.GetComponentStyle("GUIDragIndicator").GetDefaultSprite();
				Inventory.slotHotkeySprite = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(258, 7, 120, 120)), null, 0f);
				Inventory.EquippedIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 137, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
				Inventory.EquippedHoverIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 157, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
				Inventory.EquippedClickedIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 177, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
				Inventory.UnequippedIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 197, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
				Inventory.UnequippedHoverIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 217, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
				Inventory.UnequippedClickedIndicator = new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(550, 237, 87, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f);
			}
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x000F391A File Offset: 0x000F1B1A
		public IEnumerable<Item> GetAllItems(bool checkForDuplicates)
		{
			Inventory.<GetAllItems>d__120 <GetAllItems>d__ = new Inventory.<GetAllItems>d__120(-2);
			<GetAllItems>d__.<>4__this = this;
			<GetAllItems>d__.<>3__checkForDuplicates = checkForDuplicates;
			return <GetAllItems>d__;
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x000F3934 File Offset: 0x000F1B34
		private void NotifyItemComponentsOfChange()
		{
			Character character = this.Owner as Character;
			if (character != null && character == Character.Controlled)
			{
				Item selectedItem = character.SelectedItem;
				if (selectedItem != null)
				{
					CircuitBox component = selectedItem.GetComponent<CircuitBox>();
					if (component != null)
					{
						component.OnViewUpdateProjSpecific();
					}
				}
			}
			Item it = this.Owner as Item;
			if (it == null)
			{
				return;
			}
			foreach (ItemComponent c in it.Components)
			{
				c.OnInventoryChanged();
			}
			Inventory parentInventory = it.ParentInventory;
			if (parentInventory == null)
			{
				return;
			}
			parentInventory.NotifyItemComponentsOfChange();
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x000F39DC File Offset: 0x000F1BDC
		public bool Contains(Item item)
		{
			return this.slots.Any((Inventory.ItemSlot i) => i.Contains(item));
		}

		// Token: 0x0600184C RID: 6220 RVA: 0x000F3A10 File Offset: 0x000F1C10
		public Item FirstOrDefault()
		{
			foreach (Inventory.ItemSlot itemSlot in this.slots)
			{
				Item item = itemSlot.FirstOrDefault();
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x000F3A44 File Offset: 0x000F1C44
		public Item LastOrDefault()
		{
			for (int i = this.slots.Length - 1; i >= 0; i--)
			{
				Item item = this.slots[i].LastOrDefault();
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x0600184E RID: 6222 RVA: 0x000F3A7A File Offset: 0x000F1C7A
		private bool IsIndexInRange(int index)
		{
			return index >= 0 && index < this.slots.Length;
		}

		// Token: 0x0600184F RID: 6223 RVA: 0x000F3A8D File Offset: 0x000F1C8D
		public Item GetItemAt(int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return null;
			}
			return this.slots[index].FirstOrDefault();
		}

		// Token: 0x06001850 RID: 6224 RVA: 0x000F3AA7 File Offset: 0x000F1CA7
		public IEnumerable<Item> GetItemsAt(int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return Enumerable.Empty<Item>();
			}
			return this.slots[index].Items;
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x000F3AC5 File Offset: 0x000F1CC5
		public int GetItemStackSlotIndex(Item item, int index)
		{
			if (!this.IsIndexInRange(index))
			{
				return -1;
			}
			return this.slots[index].Items.IndexOf(item);
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x000F3AE8 File Offset: 0x000F1CE8
		public int FindIndex(Item item)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06001853 RID: 6227 RVA: 0x000F3B1C File Offset: 0x000F1D1C
		public List<int> FindIndices(Item item)
		{
			List<int> indices = new List<int>();
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					indices.Add(i);
				}
			}
			return indices;
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x000F3B58 File Offset: 0x000F1D58
		public virtual bool ItemOwnsSelf(Item item)
		{
			if (this.Owner == null)
			{
				return false;
			}
			if (!(this.Owner is Item))
			{
				return false;
			}
			Item ownerItem = this.Owner as Item;
			return ownerItem == item || (ownerItem.ParentInventory != null && ownerItem.ParentInventory.ItemOwnsSelf(item));
		}

		// Token: 0x06001855 RID: 6229 RVA: 0x000F3BA8 File Offset: 0x000F1DA8
		public virtual int FindAllowedSlot(Item item, bool ignoreCondition = false)
		{
			if (this.ItemOwnsSelf(item))
			{
				return -1;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					return -1;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (this.slots[j].CanBePut(item, ignoreCondition))
				{
					return j;
				}
			}
			return -1;
		}

		// Token: 0x06001856 RID: 6230 RVA: 0x000F3C08 File Offset: 0x000F1E08
		public bool CanBePut(Item item)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.CanBePutInSlot(item, i, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001857 RID: 6231 RVA: 0x000F3C34 File Offset: 0x000F1E34
		public virtual bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return !this.ItemOwnsSelf(item) && this.IsIndexInRange(i) && this.slots[i].CanBePut(item, ignoreCondition);
		}

		// Token: 0x06001858 RID: 6232 RVA: 0x000F3C5C File Offset: 0x000F1E5C
		public bool CanProbablyBePut(ItemPrefab itemPrefab, float? condition = null, int? quality = null)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.CanBePutInSlot(itemPrefab, i, condition, quality))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001859 RID: 6233 RVA: 0x000F3C89 File Offset: 0x000F1E89
		public virtual bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition = null, int? quality = null)
		{
			return this.IsIndexInRange(i) && this.slots[i].CanProbablyBePut(itemPrefab, condition, quality);
		}

		// Token: 0x0600185A RID: 6234 RVA: 0x000F3CA8 File Offset: 0x000F1EA8
		public int HowManyCanBePut(ItemPrefab itemPrefab, float? condition = null)
		{
			int count = 0;
			for (int i = 0; i < this.capacity; i++)
			{
				count += this.HowManyCanBePut(itemPrefab, i, condition, false);
			}
			return count;
		}

		// Token: 0x0600185B RID: 6235 RVA: 0x000F3CD8 File Offset: 0x000F1ED8
		public virtual int HowManyCanBePut(ItemPrefab itemPrefab, int i, float? condition, bool ignoreItemsInSlot = false)
		{
			if (!this.IsIndexInRange(i))
			{
				return 0;
			}
			return this.slots[i].HowManyCanBePut(itemPrefab, null, condition, ignoreItemsInSlot);
		}

		// Token: 0x0600185C RID: 6236 RVA: 0x000F3D10 File Offset: 0x000F1F10
		public virtual bool TryPutItem(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			int slot = this.FindAllowedSlot(item, ignoreCondition);
			if (slot < 0)
			{
				return false;
			}
			this.PutItem(item, slot, user, true, createNetworkEvent, triggerOnInsertedEffects);
			return true;
		}

		// Token: 0x0600185D RID: 6237 RVA: 0x000F3D3C File Offset: 0x000F1F3C
		public virtual bool TryPutItem(Item item, int i, bool allowSwapping, bool allowCombine, Character user, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			if (!this.IsIndexInRange(i))
			{
				string thisItemStr = ((item != null) ? item.Prefab.Identifier.Value : null) ?? "null";
				string ownerStr = "null";
				Item ownerItem = this.Owner as Item;
				if (ownerItem != null)
				{
					ownerStr = ownerItem.Prefab.Identifier.Value;
				}
				else
				{
					Character ownerCharacter = this.Owner as Character;
					if (ownerCharacter != null)
					{
						ownerStr = ownerCharacter.SpeciesName.Value;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Inventory.TryPutItem failed: index was out of range (item: ");
				defaultInterpolatedStringHandler.AppendFormatted(thisItemStr);
				defaultInterpolatedStringHandler.AppendLiteral(", inventory: ");
				defaultInterpolatedStringHandler.AppendFormatted(ownerStr);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				GameAnalyticsManager.AddErrorEventOnce("Inventory.TryPutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			if (this.Owner == null)
			{
				return false;
			}
			if (this.slots[i].Any() && allowCombine && this.slots[i].First().Combine(item, user))
			{
				return this.slots[i].Any() || this.TryPutItem(item, i, allowSwapping, allowCombine, user, createNetworkEvent, ignoreCondition, triggerOnInsertedEffects);
			}
			if (this.CanBePutInSlot(item, i, ignoreCondition))
			{
				this.PutItem(item, i, user, true, createNetworkEvent, triggerOnInsertedEffects);
				return true;
			}
			if (this.slots[i].Any() && item.ParentInventory != null && allowSwapping)
			{
				Item itemInSlot = this.slots[i].First();
				if (itemInSlot.OwnInventory != null && !itemInSlot.OwnInventory.Contains(item))
				{
					ItemContainer component = itemInSlot.GetComponent<ItemContainer>();
					if (component != null && component.GetMaxStackSize(0) == 1 && itemInSlot.OwnInventory.TrySwapping(0, item, user, createNetworkEvent, false))
					{
						return true;
					}
				}
				return this.TrySwapping(i, item, user, createNetworkEvent, true) || this.TrySwapping(i, item, user, createNetworkEvent, false);
			}
			if (this.visualSlots != null && createNetworkEvent)
			{
				this.visualSlots[i].ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
			}
			return false;
		}

		// Token: 0x0600185E RID: 6238 RVA: 0x000F3F54 File Offset: 0x000F2154
		protected virtual void PutItem(Item item, int i, Character user, bool removeItem = true, bool createNetworkEvent = true, bool triggerOnInsertedEffects = true)
		{
			if (!this.IsIndexInRange(i))
			{
				string errorMsg = "Inventory.PutItem failed: index was out of range(" + i.ToString() + ").\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("Inventory.PutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (this.Owner == null)
			{
				return;
			}
			Inventory prevInventory = item.ParentInventory;
			Inventory prevOwnerInventory = item.FindParentInventory((Inventory inv) => inv is CharacterInventory);
			if (createNetworkEvent)
			{
				this.CreateNetworkEvent();
				if (prevInventory != null && prevInventory != this)
				{
					prevInventory.syncItemsDelay = 1f;
				}
			}
			if (removeItem)
			{
				bool createNetworkEvent2;
				if (createNetworkEvent)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					createNetworkEvent2 = (networkMember != null && networkMember.IsServer);
				}
				else
				{
					createNetworkEvent2 = false;
				}
				item.Drop(user, createNetworkEvent2, false);
				Inventory parentInventory = item.ParentInventory;
				if (parentInventory != null)
				{
					parentInventory.RemoveItem(item);
				}
			}
			this.slots[i].Add(item);
			item.ParentInventory = this;
			if (this.visualSlots != null)
			{
				this.visualSlots[i].ShowBorderHighlight(Color.White, 0.1f, 0.4f, 0.5f);
				Inventory.SlotReference slotReference = Inventory.selectedSlot;
				if (((slotReference != null) ? slotReference.Inventory : null) == this)
				{
					Inventory.selectedSlot.ForceTooltipRefresh = true;
				}
			}
			CharacterHUD.RecreateHudTextsIfControlling(user);
			if (item.body != null)
			{
				item.body.Enabled = false;
				item.body.BodyType = BodyType.Dynamic;
				item.SetTransform(item.SimPosition, 0f, false, true, null);
				item.body.UpdateDrawPosition(false);
			}
			if (this is CharacterInventory)
			{
				if (prevInventory != this && prevOwnerInventory != this)
				{
					HumanAIController.ItemTaken(item, user);
				}
			}
			else
			{
				CharacterInventory currentInventory = item.FindParentInventory((Inventory inv) => inv is CharacterInventory) as CharacterInventory;
				if (currentInventory != null && currentInventory != prevInventory)
				{
					HumanAIController.ItemTaken(item, user);
				}
			}
			this.NotifyItemComponentsOfChange();
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x000F4120 File Offset: 0x000F2320
		public bool IsEmpty()
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Any())
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001860 RID: 6240 RVA: 0x000F4150 File Offset: 0x000F2350
		public virtual bool IsFull(bool takeStacksIntoAccount = false)
		{
			if (takeStacksIntoAccount)
			{
				for (int i = 0; i < this.capacity; i++)
				{
					if (!this.slots[i].Any())
					{
						return false;
					}
					Item item = this.slots[i].FirstOrDefault();
					if (this.slots[i].Items.Count < item.Prefab.GetMaxStackSize(this))
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < this.capacity; j++)
				{
					if (!this.slots[j].Any())
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001861 RID: 6241 RVA: 0x000F41D8 File Offset: 0x000F23D8
		protected bool TrySwapping(int index, Item item, Character user, bool createNetworkEvent, bool swapWholeStack)
		{
			Inventory.<>c__DisplayClass144_0 CS$<>8__locals1 = new Inventory.<>c__DisplayClass144_0();
			CS$<>8__locals1.user = user;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.index = index;
			CS$<>8__locals1.createNetworkEvent = createNetworkEvent;
			if (((item != null) ? item.ParentInventory : null) == null || !this.slots[CS$<>8__locals1.index].Any())
			{
				return false;
			}
			if (this.slots[CS$<>8__locals1.index].Items.Any((Item it) => !it.IsInteractable(CS$<>8__locals1.user)))
			{
				return false;
			}
			if (!this.AllowSwappingContainedItems)
			{
				return false;
			}
			CS$<>8__locals1.otherInventory = item.ParentInventory;
			bool otherIsEquipped = false;
			CS$<>8__locals1.otherIndex = -1;
			for (int i = 0; i < CS$<>8__locals1.otherInventory.slots.Length; i++)
			{
				if (CS$<>8__locals1.otherInventory.slots[i].Contains(item))
				{
					CharacterInventory characterInventory = CS$<>8__locals1.otherInventory as CharacterInventory;
					if (characterInventory != null)
					{
						if (characterInventory.SlotTypes[i] == InvSlotType.Any)
						{
							CS$<>8__locals1.otherIndex = i;
							break;
						}
						otherIsEquipped = true;
					}
				}
			}
			if (CS$<>8__locals1.otherIndex == -1)
			{
				CS$<>8__locals1.otherIndex = CS$<>8__locals1.otherInventory.FindIndex(item);
				if (CS$<>8__locals1.otherIndex == -1)
				{
					DebugConsole.ThrowError("Something went wrong when trying to swap items between inventory slots: couldn't find the source item from it's inventory.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return false;
				}
			}
			CS$<>8__locals1.existingItems = new List<Item>();
			if (swapWholeStack)
			{
				CS$<>8__locals1.existingItems.AddRange(this.slots[CS$<>8__locals1.index].Items);
				int j5;
				int j;
				for (j = 0; j < this.capacity; j = j5 + 1)
				{
					if (CS$<>8__locals1.existingItems.Any((Item existingItem) => CS$<>8__locals1.<>4__this.slots[j].Contains(existingItem)))
					{
						this.slots[j].RemoveAllItems();
					}
					j5 = j;
				}
			}
			else
			{
				CS$<>8__locals1.existingItems.Add(this.slots[CS$<>8__locals1.index].FirstOrDefault());
				int j5;
				int j;
				for (j = 0; j < this.capacity; j = j5 + 1)
				{
					if (CS$<>8__locals1.existingItems.Any((Item existingItem) => CS$<>8__locals1.<>4__this.slots[j].Contains(existingItem)))
					{
						this.slots[j].RemoveItem(CS$<>8__locals1.existingItems.First<Item>());
					}
					j5 = j;
				}
			}
			CS$<>8__locals1.stackedItems = new List<Item>();
			if (swapWholeStack)
			{
				for (int j6 = 0; j6 < CS$<>8__locals1.otherInventory.capacity; j6++)
				{
					if (CS$<>8__locals1.otherInventory.slots[j6].Contains(item) && !CS$<>8__locals1.stackedItems.Contains(item))
					{
						CS$<>8__locals1.stackedItems.AddRange(CS$<>8__locals1.otherInventory.slots[j6].Items);
						CS$<>8__locals1.otherInventory.slots[j6].RemoveAllItems();
					}
				}
			}
			else if (!CS$<>8__locals1.stackedItems.Contains(item))
			{
				CS$<>8__locals1.stackedItems.Add(item);
				CS$<>8__locals1.otherInventory.slots[CS$<>8__locals1.otherIndex].RemoveItem(item);
			}
			bool swapSuccessful;
			if (otherIsEquipped)
			{
				swapSuccessful = (CS$<>8__locals1.stackedItems.Distinct<Item>().All((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) && (CS$<>8__locals1.existingItems.All((Item existingItem) => CS$<>8__locals1.otherInventory.TryPutItem(existingItem, CS$<>8__locals1.otherIndex, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) || (CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.otherInventory.TryPutItem(CS$<>8__locals1.existingItems.First<Item>(), CS$<>8__locals1.user, CharacterInventory.AnySlot, CS$<>8__locals1.createNetworkEvent, false, true))));
			}
			else
			{
				swapSuccessful = ((CS$<>8__locals1.existingItems.All((Item existingItem) => CS$<>8__locals1.otherInventory.TryPutItem(existingItem, CS$<>8__locals1.otherIndex, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)) || (CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.otherInventory.TryPutItem(CS$<>8__locals1.existingItems.First<Item>(), CS$<>8__locals1.user, CharacterInventory.AnySlot, CS$<>8__locals1.createNetworkEvent, false, true))) && CS$<>8__locals1.stackedItems.Distinct<Item>().All((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true)));
				if (!swapSuccessful && CS$<>8__locals1.existingItems.Count == 1 && CS$<>8__locals1.existingItems[0].AllowDroppingOnSwapWith(item))
				{
					Item container = CS$<>8__locals1.existingItems[0].Container;
					CharacterInventory characterInv = ((container != null) ? container.ParentInventory : null) as CharacterInventory;
					if (characterInv == null || !characterInv.TryPutItem(CS$<>8__locals1.existingItems[0], CS$<>8__locals1.user, new List<InvSlotType>
					{
						InvSlotType.Any
					}, true, false, true))
					{
						CS$<>8__locals1.existingItems[0].Drop(CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, true);
					}
					swapSuccessful = CS$<>8__locals1.stackedItems.Distinct<Item>().Any((Item stackedItem) => CS$<>8__locals1.<>4__this.TryPutItem(stackedItem, CS$<>8__locals1.index, false, false, CS$<>8__locals1.user, CS$<>8__locals1.createNetworkEvent, false, true));
					if (swapSuccessful)
					{
						SoundPlayer.PlayUISound(GUISoundType.DropItem);
						if (CS$<>8__locals1.otherInventory.visualSlots != null && CS$<>8__locals1.otherIndex > -1)
						{
							CS$<>8__locals1.otherInventory.visualSlots[CS$<>8__locals1.otherIndex].ShowBorderHighlight(Color.Transparent, 0.1f, 0.1f, 0.5f);
						}
					}
				}
			}
			if (swapSuccessful)
			{
				if (this.visualSlots != null)
				{
					for (int k = 0; k < this.capacity; k++)
					{
						if (this.slots[k].Contains(item))
						{
							this.visualSlots[k].ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.9f, 0.5f);
						}
					}
				}
				if (CS$<>8__locals1.otherInventory.visualSlots != null)
				{
					for (int l = 0; l < CS$<>8__locals1.otherInventory.capacity; l++)
					{
						if (CS$<>8__locals1.otherInventory.slots[l].Contains(CS$<>8__locals1.existingItems.FirstOrDefault<Item>()))
						{
							CS$<>8__locals1.otherInventory.visualSlots[l].ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.9f, 0.5f);
						}
					}
				}
				return true;
			}
			if (swapWholeStack)
			{
				foreach (Item stackedItem2 in CS$<>8__locals1.stackedItems)
				{
					for (int m = 0; m < this.capacity; m++)
					{
						if (this.slots[m].Contains(stackedItem2))
						{
							this.slots[m].RemoveItem(stackedItem2);
						}
					}
				}
				using (List<Item>.Enumerator enumerator2 = CS$<>8__locals1.existingItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item existingItem2 = enumerator2.Current;
						for (int n = 0; n < CS$<>8__locals1.otherInventory.capacity; n++)
						{
							if (CS$<>8__locals1.otherInventory.slots[n].Contains(existingItem2))
							{
								CS$<>8__locals1.otherInventory.slots[n].RemoveItem(existingItem2);
							}
						}
					}
					goto IL_78C;
				}
			}
			for (int j2 = 0; j2 < this.capacity; j2++)
			{
				if (this.slots[j2].Contains(item))
				{
					Inventory.ItemSlot itemSlot = this.slots[j2];
					Func<Item, bool> predicate;
					if ((predicate = CS$<>8__locals1.<>9__9) == null)
					{
						predicate = (CS$<>8__locals1.<>9__9 = ((Item it) => CS$<>8__locals1.existingItems.Contains(it) || CS$<>8__locals1.stackedItems.Contains(it)));
					}
					itemSlot.RemoveWhere(predicate);
				}
			}
			for (int j3 = 0; j3 < CS$<>8__locals1.otherInventory.capacity; j3++)
			{
				if (CS$<>8__locals1.otherInventory.slots[j3].Contains(CS$<>8__locals1.existingItems.FirstOrDefault<Item>()))
				{
					Inventory.ItemSlot itemSlot2 = CS$<>8__locals1.otherInventory.slots[j3];
					Func<Item, bool> predicate2;
					if ((predicate2 = CS$<>8__locals1.<>9__10) == null)
					{
						predicate2 = (CS$<>8__locals1.<>9__10 = ((Item it) => CS$<>8__locals1.existingItems.Contains(it) || CS$<>8__locals1.stackedItems.Contains(it)));
					}
					itemSlot2.RemoveWhere(predicate2);
				}
			}
			IL_78C:
			if (otherIsEquipped)
			{
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.existingItems, this, CS$<>8__locals1.index);
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.stackedItems, CS$<>8__locals1.otherInventory, CS$<>8__locals1.otherIndex);
			}
			else
			{
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.stackedItems, CS$<>8__locals1.otherInventory, CS$<>8__locals1.otherIndex);
				CS$<>8__locals1.<TrySwapping>g__TryPutAndForce|5(CS$<>8__locals1.existingItems, this, CS$<>8__locals1.index);
			}
			if (CS$<>8__locals1.createNetworkEvent)
			{
				this.CreateNetworkEvent();
				CS$<>8__locals1.otherInventory.CreateNetworkEvent();
			}
			if (this.visualSlots != null)
			{
				for (int j4 = 0; j4 < this.capacity; j4++)
				{
					if (this.slots[j4].Contains(CS$<>8__locals1.existingItems.FirstOrDefault<Item>()))
					{
						this.visualSlots[j4].ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
					}
				}
			}
			return false;
		}

		// Token: 0x06001862 RID: 6242 RVA: 0x000F4A64 File Offset: 0x000F2C64
		public void CreateNetworkEvent()
		{
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			if (GameMain.NetworkMember.IsClient)
			{
				this.syncItemsDelay = 1f;
			}
			List<Range> slotRanges = new List<Range>();
			int startIndex = 0;
			int itemCount = 0;
			for (int i = 0; i < this.capacity; i++)
			{
				int count = this.slots[i].Items.Count;
				if (itemCount + count > 128 || i == this.capacity - 1)
				{
					slotRanges.Add(new Range(startIndex, i + 1));
					startIndex = i + 1;
					itemCount = 0;
				}
				itemCount += count;
			}
			foreach (Range slotRange in slotRanges)
			{
				this.CreateNetworkEvent(slotRange);
			}
		}

		// Token: 0x06001863 RID: 6243 RVA: 0x000F4B40 File Offset: 0x000F2D40
		protected virtual void CreateNetworkEvent(Range slotRange)
		{
		}

		// Token: 0x06001864 RID: 6244 RVA: 0x000F4B44 File Offset: 0x000F2D44
		public Item FindItem(Func<Item, bool> predicate, bool recursive)
		{
			Item match = this.AllItems.FirstOrDefault(predicate);
			if (match == null && recursive)
			{
				foreach (Item item in this.AllItems)
				{
					if (((item != null) ? item.OwnInventory : null) != null)
					{
						match = item.OwnInventory.FindItem(predicate, true);
						if (match != null)
						{
							return match;
						}
					}
				}
				return match;
			}
			return match;
		}

		// Token: 0x06001865 RID: 6245 RVA: 0x000F4BC8 File Offset: 0x000F2DC8
		public List<Item> FindAllItems(Func<Item, bool> predicate = null, bool recursive = false, List<Item> list = null)
		{
			if (list == null)
			{
				list = new List<Item>();
			}
			foreach (Item item in this.AllItems)
			{
				if (predicate == null || predicate(item))
				{
					list.Add(item);
				}
				if (recursive)
				{
					ItemInventory ownInventory = item.OwnInventory;
					if (ownInventory != null)
					{
						ownInventory.FindAllItems(predicate, true, list);
					}
				}
			}
			return list;
		}

		// Token: 0x06001866 RID: 6246 RVA: 0x000F4C44 File Offset: 0x000F2E44
		public Item FindItemByTag(Identifier tag, bool recursive = false)
		{
			if (tag.IsEmpty)
			{
				return null;
			}
			return this.FindItem((Item i) => i.HasTag(tag), recursive);
		}

		// Token: 0x06001867 RID: 6247 RVA: 0x000F4C80 File Offset: 0x000F2E80
		public Item FindItemByIdentifier(Identifier identifier, bool recursive = false)
		{
			if (identifier.IsEmpty)
			{
				return null;
			}
			return this.FindItem((Item i) => i.Prefab.Identifier == identifier, recursive);
		}

		// Token: 0x06001868 RID: 6248 RVA: 0x000F4CBC File Offset: 0x000F2EBC
		public virtual void RemoveItem(Item item)
		{
			if (item == null)
			{
				return;
			}
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Contains(item))
				{
					this.slots[i].RemoveItem(item);
					item.ParentInventory = null;
					if (this.visualSlots != null)
					{
						this.visualSlots[i].ShowBorderHighlight(Color.White, 0.1f, 0.4f, 0.5f);
						Inventory.SlotReference slotReference = Inventory.selectedSlot;
						if (((slotReference != null) ? slotReference.Inventory : null) == this)
						{
							Inventory.selectedSlot.ForceTooltipRefresh = true;
						}
					}
					CharacterHUD.RecreateHudTextsIfFocused(new Item[]
					{
						item
					});
				}
			}
			this.NotifyItemComponentsOfChange();
		}

		// Token: 0x06001869 RID: 6249 RVA: 0x000F4D68 File Offset: 0x000F2F68
		public void ForceToSlot(Item item, int index)
		{
			this.slots[index].Add(item);
			item.ParentInventory = this;
			CharacterInventory characterInventory = this as CharacterInventory;
			Character character = ((characterInventory != null) ? characterInventory.Owner : null) as Character;
			bool equipped = character != null && character.HasEquippedItem(item, null, null);
			if (item.body != null && !equipped)
			{
				item.body.Enabled = false;
				item.body.BodyType = BodyType.Dynamic;
			}
		}

		// Token: 0x0600186A RID: 6250 RVA: 0x000F4DDD File Offset: 0x000F2FDD
		public void ForceRemoveFromSlot(Item item, int index)
		{
			this.slots[index].RemoveItem(item);
		}

		// Token: 0x0600186B RID: 6251 RVA: 0x000F4DED File Offset: 0x000F2FED
		public bool IsInSlot(Item item, int index)
		{
			return this.IsIndexInRange(index) && this.slots[index].Contains(item);
		}

		// Token: 0x0600186C RID: 6252 RVA: 0x000F4E08 File Offset: 0x000F3008
		public bool IsSlotEmpty(int index)
		{
			return this.IsIndexInRange(index) && this.slots[index].Empty();
		}

		// Token: 0x0600186D RID: 6253 RVA: 0x000F4E24 File Offset: 0x000F3024
		public void SharedRead(IReadMessage msg, List<ushort>[] receivedItemIds, out bool readyToApply)
		{
			byte start = msg.ReadByte();
			byte end = msg.ReadByte();
			if (start == 0)
			{
				for (int i = 0; i < this.capacity; i++)
				{
					receivedItemIds[i] = null;
				}
			}
			for (int j = (int)start; j < (int)end; j++)
			{
				List<ushort> newItemIds = new List<ushort>();
				int itemCount = msg.ReadRangedInteger(0, 63);
				for (int k = 0; k < itemCount; k++)
				{
					newItemIds.Add(msg.ReadUInt16());
				}
				receivedItemIds[j] = newItemIds;
			}
			readyToApply = !receivedItemIds.Contains(null);
		}

		// Token: 0x0600186E RID: 6254 RVA: 0x000F4EA4 File Offset: 0x000F30A4
		public void SharedWrite(IWriteMessage msg, Range slotRange)
		{
			int start = slotRange.Start.Value;
			int end = slotRange.End.Value;
			msg.WriteByte((byte)start);
			msg.WriteByte((byte)end);
			for (int i = start; i < end; i++)
			{
				msg.WriteRangedInteger(this.slots[i].Items.Count, 0, 63);
				for (int j = 0; j < Math.Min(this.slots[i].Items.Count, 63); j++)
				{
					Item item = this.slots[i].Items[j];
					msg.WriteUInt16((item != null) ? item.ID : 0);
				}
			}
		}

		// Token: 0x0600186F RID: 6255 RVA: 0x000F4F5C File Offset: 0x000F315C
		public void DeleteAllItems()
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.slots[i].Any())
				{
					foreach (Item item in this.slots[i].Items)
					{
						foreach (ItemContainer itemContainer in item.GetComponents<ItemContainer>())
						{
							itemContainer.Inventory.DeleteAllItems();
						}
					}
					this.slots[i].Items.ForEachMod(delegate(Item it)
					{
						it.Remove();
					});
					this.slots[i].RemoveAllItems();
				}
			}
		}

		// Token: 0x06001871 RID: 6257 RVA: 0x000F5080 File Offset: 0x000F3280
		[CompilerGenerated]
		internal static bool <DetermineMouseOnInventory>g__IsOnInventorySlot|79_0(Inventory inventory)
		{
			for (int i = 0; i < inventory.visualSlots.Length; i++)
			{
				if (!inventory.HideSlot(i))
				{
					VisualSlot slot = inventory.visualSlots[i];
					if (slot.InteractRect.Contains(PlayerInput.MousePosition))
					{
						return true;
					}
					if (slot.EquipButtonRect.Contains(PlayerInput.MousePosition) && i >= 0 && inventory.slots.Length > i && !inventory.slots[i].Empty())
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001872 RID: 6258 RVA: 0x000F50FC File Offset: 0x000F32FC
		[CompilerGenerated]
		internal static bool <UpdateDragging>g__TryPortraitAndHealthDrop|83_1(bool mouseOnPortrait)
		{
			bool dropSuccessful = false;
			foreach (Item item in Inventory.DraggingItems)
			{
				Inventory inventory = item.ParentInventory;
				List<int> indices = (inventory != null) ? inventory.FindIndices(item) : null;
				dropSuccessful |= (CharacterHealth.OpenHealthWindow ?? Character.Controlled.CharacterHealth).OnItemDropped(item, mouseOnPortrait);
				if (dropSuccessful)
				{
					if (indices == null || inventory.visualSlots == null)
					{
						break;
					}
					using (List<int>.Enumerator enumerator2 = indices.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int i = enumerator2.Current;
							VisualSlot visualSlot = inventory.visualSlots[i];
							if (visualSlot != null)
							{
								visualSlot.ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.4f, 0.5f);
							}
						}
						break;
					}
				}
			}
			if (dropSuccessful)
			{
				Inventory.DraggingItems.Clear();
				return true;
			}
			return false;
		}

		// Token: 0x06001873 RID: 6259 RVA: 0x000F5208 File Offset: 0x000F3408
		[CompilerGenerated]
		internal static void <UpdateDragging>g__HandleOutsideInventoryDrop|83_2()
		{
			Item item = Character.Controlled.FocusedItem;
			bool flag;
			if (item != null)
			{
				ItemInventory inventory = item.OwnInventory;
				if (inventory != null)
				{
					ItemContainer container = item.GetComponent<ItemContainer>();
					if (container != null && container.HasRequiredItems(Character.Controlled, false, null) && container.AllowDragAndDrop)
					{
						flag = inventory.CanBePut(Inventory.DraggingItems.FirstOrDefault<Item>());
						goto IL_4C;
					}
				}
			}
			flag = false;
			IL_4C:
			bool isTargetingValidContainer = flag;
			bool isTargetingValidCharacter = Inventory.IsValidTargetForDragDropGive(Character.Controlled, Character.Controlled.FocusedCharacter, Inventory.DraggingItems);
			if (Inventory.DraggingItemToWorld && (isTargetingValidContainer || isTargetingValidCharacter))
			{
				bool anySuccess = false;
				foreach (Item it2 in Inventory.DraggingItems)
				{
					bool success = false;
					if (isTargetingValidContainer)
					{
						success = Character.Controlled.FocusedItem.OwnInventory.TryPutItem(it2, Character.Controlled, null, true, false, true);
					}
					if (!success && isTargetingValidCharacter)
					{
						success = Character.Controlled.FocusedCharacter.Inventory.TryPutItem(it2, Character.Controlled, CharacterInventory.AnySlot, true, false, true);
					}
					if (!success)
					{
						break;
					}
					anySuccess = true;
				}
				if (anySuccess)
				{
					SoundPlayer.PlayUISound(GUISoundType.PickItem);
					return;
				}
			}
			else
			{
				if (Screen.Selected is SubEditorScreen)
				{
					Item item2 = Inventory.DraggingItems.First<Item>();
					if (((item2 != null) ? item2.ParentInventory : null) != null)
					{
						SubEditorScreen.StoreCommand(new InventoryPlaceCommand(Inventory.DraggingItems.First<Item>().ParentInventory, new List<Item>(Inventory.DraggingItems), true));
					}
				}
				bool removed = false;
				SubEditorScreen editor = Screen.Selected as SubEditorScreen;
				if (editor != null)
				{
					if (editor.EntityMenu.Rect.Contains(PlayerInput.MousePosition))
					{
						Inventory.DraggingItems.ForEachMod(delegate(Item it)
						{
							it.Remove();
						});
						removed = true;
					}
					else if (editor.WiringMode)
					{
						Inventory.DraggingItems.ForEachMod(delegate(Item it)
						{
							it.Remove();
						});
						removed = true;
					}
					else
					{
						Inventory.DraggingItems.ForEachMod(delegate(Item it)
						{
							it.Drop(Character.Controlled, true, true);
						});
					}
				}
				else
				{
					Inventory.DraggingItems.ForEachMod(delegate(Item it)
					{
						it.Drop(Character.Controlled, true, true);
					});
					Inventory.DraggingItems.First<Item>().CreateDroppedStack(Inventory.DraggingItems, false);
				}
				SoundPlayer.PlayUISound(removed ? GUISoundType.PickItem : GUISoundType.DropItem);
			}
		}

		// Token: 0x06001874 RID: 6260 RVA: 0x000F5498 File Offset: 0x000F3698
		[CompilerGenerated]
		internal static void <UpdateDragging>g__HandleInventorySlotDrop|83_3()
		{
			Inventory oldInventory = Inventory.DraggingItems.First<Item>().ParentInventory;
			Inventory selectedInventory = Inventory.selectedSlot.ParentInventory;
			int slotIndex = Inventory.selectedSlot.SlotIndex;
			int oldSlot = (oldInventory == null) ? 0 : Array.IndexOf(oldInventory.slots, Inventory.DraggingItems);
			if (selectedInventory.slots[slotIndex].Empty() && selectedInventory == Character.Controlled.Inventory && !Inventory.DraggingItems.First<Item>().AllowedSlots.Any((InvSlotType a) => a.HasFlag(Character.Controlled.Inventory.SlotTypes[slotIndex])) && Inventory.DraggingItems.Any((Item it) => selectedInventory.TryPutItem(it, Character.Controlled, it.AllowedSlots, true, false, true)))
			{
				if (selectedInventory.visualSlots != null)
				{
					int i2;
					int i;
					for (i = 0; i < selectedInventory.visualSlots.Length; i = i2 + 1)
					{
						if (Inventory.DraggingItems.Any((Item it) => selectedInventory.slots[i].Contains(it)))
						{
							selectedInventory.visualSlots[slotIndex].ShowBorderHighlight(Color.White, 0.1f, 0.4f, 0.5f);
						}
						i2 = i;
					}
					selectedInventory.visualSlots[slotIndex].ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
				}
				SoundPlayer.PlayUISound(GUISoundType.PickItem);
			}
			else
			{
				bool anySuccess = false;
				bool flag;
				if (Inventory.DraggingItems.Count((Item it) => !it.IsFullCondition && it.Condition > 0f) <= 1)
				{
					flag = (selectedInventory.GetItemsAt(slotIndex).Count((Item it) => !it.IsFullCondition && it.Condition > 0f) <= 1);
				}
				else
				{
					flag = false;
				}
				bool allowCombine = flag;
				int itemCount = 0;
				foreach (Item item in Inventory.DraggingItems)
				{
					Item itemAt = selectedInventory.GetItemAt(slotIndex);
					ItemContainer itemContainer;
					if (itemAt == null)
					{
						itemContainer = null;
					}
					else
					{
						ItemInventory ownInventory = itemAt.OwnInventory;
						itemContainer = ((ownInventory != null) ? ownInventory.Container : null);
					}
					ItemContainer container = itemContainer;
					if (container != null && container.Inventory.CanBePut(item) && (!container.AllowDragAndDrop || !container.IsAccessible()))
					{
						allowCombine = false;
					}
					bool success = selectedInventory.TryPutItem(item, slotIndex, !anySuccess, allowCombine, Character.Controlled, true, false, true);
					if (success)
					{
						anySuccess = true;
						itemCount++;
					}
					if (!success)
					{
						break;
					}
					if (itemCount >= item.Prefab.GetMaxStackSize(selectedInventory))
					{
						break;
					}
				}
				if (anySuccess)
				{
					Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.ParentInventory == oldInventory || s.ParentInventory == selectedInventory);
					if (SubEditorScreen.IsSubEditor())
					{
						foreach (Item draggingItem in Inventory.DraggingItems)
						{
							if (selectedInventory.slots[slotIndex].Contains(draggingItem))
							{
								SubEditorScreen.StoreCommand(new InventoryMoveCommand(oldInventory, selectedInventory, draggingItem, oldSlot, slotIndex));
							}
						}
					}
					if (selectedInventory.visualSlots != null)
					{
						selectedInventory.visualSlots[slotIndex].ShowBorderHighlight(Color.White, 0.1f, 0.4f, 0.5f);
					}
					SoundPlayer.PlayUISound(GUISoundType.PickItem);
				}
				else
				{
					if (selectedInventory.visualSlots != null)
					{
						selectedInventory.visualSlots[slotIndex].ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
					}
					SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
				}
			}
			selectedInventory.HideTimer = 2f;
			Inventory parentInventory = Inventory.selectedSlot.ParentInventory;
			Item parentItem = ((parentInventory != null) ? parentInventory.Owner : null) as Item;
			if (parentItem != null && parentItem.ParentInventory != null)
			{
				for (int j = 0; j < parentItem.ParentInventory.capacity; j++)
				{
					if (!parentItem.ParentInventory.HideSlot(j) && parentItem.ParentInventory.slots[j].FirstOrDefault() == parentItem)
					{
						Inventory.highlightedSubInventorySlots.Add(new Inventory.SlotReference(parentItem.ParentInventory, parentItem.ParentInventory.visualSlots[j], j, false, Inventory.selectedSlot.ParentInventory));
						break;
					}
				}
			}
			Inventory.DraggingItems.Clear();
			Inventory.DraggingSlot = null;
		}

		// Token: 0x06001875 RID: 6261 RVA: 0x000F597C File Offset: 0x000F3B7C
		[CompilerGenerated]
		internal static bool <CanSelectSlot>g__OwnerInaccessible|85_0(Entity owner)
		{
			return owner != Character.Controlled && owner != Character.Controlled.SelectedCharacter && owner != Character.Controlled.SelectedItem && (Character.Controlled.SelectedItem == null || !Character.Controlled.SelectedItem.linkedTo.Contains(owner));
		}

		// Token: 0x06001876 RID: 6262 RVA: 0x000F59D4 File Offset: 0x000F3BD4
		[CompilerGenerated]
		internal static void <DrawFront>g__DrawDragRelated|87_0(ref Inventory.<>c__DisplayClass87_0 A_0)
		{
			if (Inventory.DraggingSlot == null || !Inventory.DraggingSlot.MouseOn())
			{
				Item firstDraggingItem = Inventory.DraggingItems.First<Item>();
				Sprite sprite2;
				if ((sprite2 = firstDraggingItem.OverrideInventorySprite) == null)
				{
					sprite2 = (firstDraggingItem.Prefab.InventoryIcon ?? firstDraggingItem.Sprite);
				}
				Sprite sprite = sprite2;
				int iconSize = (int)(64f * GUI.Scale);
				float scale = Math.Min(Math.Min((float)iconSize / sprite.size.X, (float)iconSize / sprite.size.Y), 1.5f);
				Vector2 itemPos = PlayerInput.MousePosition;
				bool mouseOnHealthInterface = (CharacterHealth.OpenHealthWindow != null && CharacterHealth.OpenHealthWindow.MouseOnElement) || CharacterHUD.MouseOnCharacterPortrait();
				bool flag;
				if (mouseOnHealthInterface)
				{
					flag = Inventory.DraggingItems.Any((Item it) => it.UseInHealthInterface);
				}
				else
				{
					flag = false;
				}
				mouseOnHealthInterface = flag;
				if ((GUI.MouseOn == null || mouseOnHealthInterface) && Inventory.selectedSlot == null)
				{
					UISprite shadowSprite = GUIStyle.GetComponentStyle("OuterGlow").Sprites[GUIComponent.ComponentState.None][0];
					ValueTuple<LocalizedString, Color> valueTuple = Inventory.<DrawFront>g__GetDragLabelTextAndColor|87_1(mouseOnHealthInterface);
					LocalizedString toolTip = valueTuple.Item1;
					Color toolTipColor = valueTuple.Item2;
					Vector2 nameSize = GUIStyle.Font.MeasureString(Inventory.DraggingItems.First<Item>().Name, false);
					Vector2 toolTipSize = GUIStyle.SmallFont.MeasureString(toolTip, false);
					int textWidth = (int)Math.Max(nameSize.X, toolTipSize.X);
					int textSpacing = (int)(15f * GUI.Scale);
					Vector2 textPos = itemPos;
					int textDir = (textPos.X + (float)textWidth * 1.5f > (float)GameMain.GraphicsWidth) ? -1 : 1;
					int textOffset = (textDir == 1) ? 0 : -1;
					textPos += new Vector2((float)((iconSize / 2 + textSpacing) * textDir), 0f);
					Point shadowPadding = new Point(40, 20).Multiply(GUI.Scale);
					Point shadowSize = new Point(iconSize + textWidth + textSpacing, iconSize) + shadowPadding.Multiply(2);
					shadowSprite.Draw(A_0.spriteBatch, new Rectangle(itemPos.ToPoint() - new Point((iconSize / 2 - shadowPadding.X) * textDir - shadowSize.X * textOffset, iconSize / 2 + shadowPadding.Y), shadowSize), Color.Black * 0.8f, SpriteEffects.None, null);
					RichString richString = RichString.Rich(Inventory.DraggingItems.First<Item>().Name, null);
					SpriteBatch spriteBatch = A_0.spriteBatch;
					Vector2 pos = textPos + new Vector2(nameSize.X * (float)textOffset, (float)(-(float)iconSize / 2));
					string sanitizedValue = richString.SanitizedValue;
					Color white = Color.White;
					ImmutableArray<RichTextData>? richTextData = richString.RichTextData;
					GUI.DrawStringWithColors(spriteBatch, pos, sanitizedValue, white, richTextData, null, 0, null, 0f);
					SpriteBatch spriteBatch2 = A_0.spriteBatch;
					Vector2 pos2 = textPos + new Vector2(toolTipSize.X * (float)textOffset, 0f);
					LocalizedString text = toolTip;
					Color color = toolTipColor;
					GUIFont smallFont = GUIStyle.SmallFont;
					GUI.DrawString(spriteBatch2, pos2, text, color, null, 0, smallFont, ForceUpperCase.Inherit);
				}
				Item draggedItem = Inventory.DraggingItems.First<Item>();
				sprite.Draw(A_0.spriteBatch, itemPos + Vector2.One * 2f, Color.Black, 0f, scale, SpriteEffects.None, null);
				sprite.Draw(A_0.spriteBatch, itemPos, (sprite == draggedItem.Sprite) ? draggedItem.GetSpriteColor(null, false) : draggedItem.GetInventoryIconColor(), 0f, scale, SpriteEffects.None, null);
				if (draggedItem.Prefab.GetMaxStackSize(null) > 1)
				{
					int stackAmount = Inventory.DraggingItems.Count;
					Inventory.SlotReference slotReference = Inventory.selectedSlot;
					if (((slotReference != null) ? slotReference.ParentInventory : null) != null)
					{
						Item item = Inventory.selectedSlot.Item;
						if (((item != null) ? item.OwnInventory : null) != null)
						{
							int maxAmountPerSlot = 0;
							for (int i = 0; i < Inventory.SelectedSlot.Item.OwnInventory.Capacity; i++)
							{
								maxAmountPerSlot = Math.Max(maxAmountPerSlot, Inventory.selectedSlot.Item.OwnInventory.HowManyCanBePut(draggedItem.Prefab, i, new float?(draggedItem.Condition), true));
							}
							stackAmount = Math.Min(stackAmount, maxAmountPerSlot);
						}
						else
						{
							stackAmount = Math.Min(stackAmount, Inventory.selectedSlot.ParentInventory.HowManyCanBePut(draggedItem.Prefab, Inventory.selectedSlot.SlotIndex, new float?(draggedItem.Condition), true));
						}
					}
					Vector2 stackCountPos = itemPos + Vector2.One * (float)iconSize * 0.25f;
					string stackCountText = "x" + stackAmount.ToString();
					GUIStyle.SmallFont.DrawString(A_0.spriteBatch, stackCountText, stackCountPos + Vector2.One, Color.Black, ForceUpperCase.Inherit, false);
					GUIStyle.SmallFont.DrawString(A_0.spriteBatch, stackCountText, stackCountPos, GUIStyle.TextColorBright, ForceUpperCase.Inherit, false);
				}
			}
		}

		// Token: 0x06001877 RID: 6263 RVA: 0x000F5EC4 File Offset: 0x000F40C4
		[CompilerGenerated]
		internal static ValueTuple<LocalizedString, Color> <DrawFront>g__GetDragLabelTextAndColor|87_1(bool mouseOnHealthInterface)
		{
			bool useDragDropGive = Inventory.IsValidTargetForDragDropGive(Character.Controlled, Character.Controlled.FocusedCharacter, Inventory.DraggingItems);
			Color toolTipColor = Color.LightGreen;
			LocalizedString toolTip;
			if (mouseOnHealthInterface)
			{
				toolTip = TextManager.Get("QuickUseAction.UseTreatment");
			}
			else if (Character.Controlled.FocusedItem != null)
			{
				toolTip = TextManager.GetWithVariable("PutItemIn", "[itemname]", Character.Controlled.FocusedItem.Name, FormatCapitals.Yes);
			}
			else if (useDragDropGive)
			{
				toolTip = TextManager.GetWithVariable("GiveItemTo", "[character]", Character.Controlled.FocusedCharacter.Name, FormatCapitals.Yes);
			}
			else
			{
				toolTipColor = GUIStyle.Red;
				SubEditorScreen editor = Screen.Selected as SubEditorScreen;
				toolTip = TextManager.Get((editor != null && editor.EntityMenu.Rect.Contains(PlayerInput.MousePosition)) ? "Delete" : "DropItem");
			}
			return new ValueTuple<LocalizedString, Color>(toolTip, toolTipColor);
		}

		// Token: 0x04000C7F RID: 3199
		protected float prevUIScale = Inventory.UIScale;

		// Token: 0x04000C80 RID: 3200
		protected float prevHUDScale = GUI.Scale;

		// Token: 0x04000C81 RID: 3201
		protected Point prevScreenResolution;

		// Token: 0x04000C82 RID: 3202
		protected static Sprite slotHotkeySprite;

		// Token: 0x04000C83 RID: 3203
		private static Sprite slotSpriteSmall;

		// Token: 0x04000C84 RID: 3204
		public const float SlotSpriteSmallScale = 0.575f;

		// Token: 0x04000C85 RID: 3205
		public static Sprite DraggableIndicator;

		// Token: 0x04000C86 RID: 3206
		public static Sprite UnequippedIndicator;

		// Token: 0x04000C87 RID: 3207
		public static Sprite UnequippedHoverIndicator;

		// Token: 0x04000C88 RID: 3208
		public static Sprite UnequippedClickedIndicator;

		// Token: 0x04000C89 RID: 3209
		public static Sprite EquippedIndicator;

		// Token: 0x04000C8A RID: 3210
		public static Sprite EquippedHoverIndicator;

		// Token: 0x04000C8B RID: 3211
		public static Sprite EquippedClickedIndicator;

		// Token: 0x04000C8C RID: 3212
		public static Inventory DraggingInventory;

		// Token: 0x04000C8D RID: 3213
		public Inventory ReplacedBy;

		// Token: 0x04000C8F RID: 3215
		private List<ushort>[] partialReceivedItemIDs;

		// Token: 0x04000C90 RID: 3216
		private List<ushort>[] receivedItemIDs;

		// Token: 0x04000C91 RID: 3217
		private CoroutineHandle syncItemsCoroutine;

		// Token: 0x04000C92 RID: 3218
		public float HideTimer;

		// Token: 0x04000C93 RID: 3219
		private bool isSubInventory;

		// Token: 0x04000C94 RID: 3220
		private const float movableFrameRectHeight = 40f;

		// Token: 0x04000C95 RID: 3221
		private Color movableFrameRectColor = new Color(60, 60, 60);

		// Token: 0x04000C96 RID: 3222
		private Rectangle movableFrameRect;

		// Token: 0x04000C97 RID: 3223
		private Point savedPosition;

		// Token: 0x04000C98 RID: 3224
		private Point originalPos;

		// Token: 0x04000C99 RID: 3225
		private bool canMove;

		// Token: 0x04000C9A RID: 3226
		private bool positionUpdateQueued;

		// Token: 0x04000C9B RID: 3227
		private Vector2 draggableIndicatorOffset;

		// Token: 0x04000C9C RID: 3228
		private float draggableIndicatorScale;

		// Token: 0x04000C9D RID: 3229
		public static VisualSlot DraggingSlot;

		// Token: 0x04000C9E RID: 3230
		public static readonly List<Item> DraggingItems = new List<Item>();

		// Token: 0x04000C9F RID: 3231
		public static readonly List<Item> doubleClickedItems = new List<Item>();

		// Token: 0x04000CA0 RID: 3232
		protected Vector4 padding;

		// Token: 0x04000CA1 RID: 3233
		private int slotsPerRow;

		// Token: 0x04000CA2 RID: 3234
		protected static HashSet<Inventory.SlotReference> highlightedSubInventorySlots = new HashSet<Inventory.SlotReference>();

		// Token: 0x04000CA3 RID: 3235
		private static readonly List<Inventory.SlotReference> subInventorySlotsToDraw = new List<Inventory.SlotReference>();

		// Token: 0x04000CA4 RID: 3236
		protected static Inventory.SlotReference selectedSlot;

		// Token: 0x04000CA5 RID: 3237
		public VisualSlot[] visualSlots;

		// Token: 0x04000CA6 RID: 3238
		private Rectangle prevRect;

		// Token: 0x04000CA7 RID: 3239
		public RectTransform RectTransform;

		// Token: 0x04000CA8 RID: 3240
		public bool DrawWhenEquipped;

		// Token: 0x04000CA9 RID: 3241
		public float OpenState;

		// Token: 0x04000CAB RID: 3243
		[TupleElementNames(new string[]
		{
			"target",
			"orderIdentifier"
		})]
		private static ValueTuple<Item, Identifier> availableContextualOrder;

		// Token: 0x04000CAC RID: 3244
		private static LocalizedString slotIconTooltip;

		// Token: 0x04000CAD RID: 3245
		public const int MaxPossibleStackSize = 63;

		// Token: 0x04000CAE RID: 3246
		public const int MaxItemsPerNetworkEvent = 128;

		// Token: 0x04000CAF RID: 3247
		public readonly Entity Owner;

		// Token: 0x04000CB0 RID: 3248
		protected readonly int capacity;

		// Token: 0x04000CB1 RID: 3249
		protected readonly Inventory.ItemSlot[] slots;

		// Token: 0x04000CB2 RID: 3250
		public bool Locked;

		// Token: 0x04000CB3 RID: 3251
		protected float syncItemsDelay;

		// Token: 0x04000CB4 RID: 3252
		private int extraStackSize;

		// Token: 0x04000CB5 RID: 3253
		private readonly List<Item> allItemsList = new List<Item>();

		// Token: 0x04000CB6 RID: 3254
		public bool AllowSwappingContainedItems = true;

		// Token: 0x02000A61 RID: 2657
		public class SlotReference
		{
			// Token: 0x17001A7D RID: 6781
			// (get) Token: 0x060074F3 RID: 29939 RVA: 0x0037408B File Offset: 0x0037228B
			// (set) Token: 0x060074F4 RID: 29940 RVA: 0x00374093 File Offset: 0x00372293
			public RichString Tooltip { get; private set; }

			// Token: 0x060074F5 RID: 29941 RVA: 0x0037409C File Offset: 0x0037229C
			public SlotReference(Inventory parentInventory, VisualSlot slot, int slotIndex, bool isSubSlot, Inventory subInventory = null)
			{
				this.ParentInventory = parentInventory;
				this.Slot = slot;
				this.SlotIndex = slotIndex;
				this.Inventory = subInventory;
				this.IsSubSlot = isSubSlot;
				this.Item = this.ParentInventory.GetItemAt(slotIndex);
				this.RefreshTooltip();
			}

			// Token: 0x060074F6 RID: 29942 RVA: 0x003740EC File Offset: 0x003722EC
			public bool TooltipNeedsRefresh()
			{
				return this.ForceTooltipRefresh || (this.Item != null && (PlayerInput.KeyDown(InputType.ContextualCommand) != this.tooltipShowedContextualOptions || (int)this.Item.ConditionPercentage != this.tooltipDisplayedCondition));
			}

			// Token: 0x060074F7 RID: 29943 RVA: 0x0037412C File Offset: 0x0037232C
			public void RefreshTooltip()
			{
				this.ForceTooltipRefresh = false;
				if (this.Item == null)
				{
					return;
				}
				IEnumerable<Item> itemsInSlot = null;
				if (this.ParentInventory != null && this.Item != null)
				{
					itemsInSlot = this.ParentInventory.GetItemsAt(this.SlotIndex);
				}
				this.Tooltip = Inventory.SlotReference.GetTooltip(this.Item, itemsInSlot, Character.Controlled);
				this.tooltipDisplayedCondition = (int)this.Item.ConditionPercentage;
				this.tooltipShowedContextualOptions = PlayerInput.KeyDown(InputType.ContextualCommand);
			}

			// Token: 0x060074F8 RID: 29944 RVA: 0x003741A4 File Offset: 0x003723A4
			private static RichString GetTooltip(Item item, IEnumerable<Item> itemsInSlot, Character character)
			{
				if (item == null)
				{
					return null;
				}
				LocalizedString toolTip = "";
				if (GameMain.DebugDraw)
				{
					toolTip = item.ToString();
				}
				else
				{
					LocalizedString description = item.Description;
					if (item.HasTag(Tags.IdCardTag) || item.HasTag(Tags.DespawnContainer))
					{
						string[] readTags = item.Tags.Split(',', StringSplitOptions.None);
						string idName = null;
						string idJob = null;
						foreach (string tag in readTags)
						{
							string[] s = tag.Split(':', StringSplitOptions.None);
							string a = s[0];
							if (!(a == "name"))
							{
								if (a == "job" || a == "jobid")
								{
									idJob = s[1];
								}
							}
							else
							{
								idName = s[1];
							}
						}
						if (idName != null)
						{
							if (idJob == null)
							{
								description = TextManager.GetWithVariable("IDCardName", "[name]", idName, FormatCapitals.No);
							}
							else
							{
								description = TextManager.GetWithVariables("IDCardNameJob", new ValueTuple<string, LocalizedString, FormatCapitals>[]
								{
									new ValueTuple<string, LocalizedString, FormatCapitals>("[name]", idName, FormatCapitals.No),
									new ValueTuple<string, LocalizedString, FormatCapitals>("[job]", TextManager.Get("jobname." + idJob).Fallback(idJob, true), FormatCapitals.Yes)
								});
							}
							if (!string.IsNullOrEmpty(item.Description))
							{
								description = description + " " + item.Description;
							}
						}
					}
					LocalizedString name = item.Name;
					foreach (ItemComponent component in item.Components)
					{
						component.AddTooltipInfo(ref name, ref description);
					}
					if (item.Prefab.ShowContentsInTooltip && item.OwnInventory != null)
					{
						using (IEnumerator<string> enumerator2 = (from it in item.OwnInventory.AllItems
						select it.Name).Distinct<string>().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								string itemName = enumerator2.Current;
								int itemCount = item.OwnInventory.AllItems.Count((Item it) => it != null && it.Name == itemName);
								description += ((itemCount == 1) ? ("\n    " + itemName) : ("\n    " + itemName + " x" + itemCount.ToString()));
							}
						}
					}
					string colorStr = (item.Illegitimate ? GUIStyle.Red : Color.White).ToStringHex();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
					defaultInterpolatedStringHandler.AppendLiteral("‖color:");
					defaultInterpolatedStringHandler.AppendFormatted(colorStr);
					defaultInterpolatedStringHandler.AppendLiteral("‖");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(name);
					defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
					toolTip = defaultInterpolatedStringHandler.ToStringAndClear();
					if (item.GetComponent<Quality>() != null)
					{
						toolTip += "\n" + TextManager.GetWithVariable("itemname.quality" + item.Quality.ToString(), "[itemname]", "", FormatCapitals.No).Fallback(TextManager.GetWithVariable("itemname.quality3", "[itemname]", "", FormatCapitals.No), true).TrimStart();
					}
					if (itemsInSlot.All((Item it) => !it.IsInteractable(Character.Controlled)))
					{
						toolTip += " " + TextManager.Get("connectionlocked");
					}
					if (!item.IsFullCondition && !item.Prefab.HideConditionInTooltip)
					{
						string conditionColorStr = ToolBox.GradientLerp(item.Condition / item.MaxCondition, new Color[]
						{
							GUIStyle.ColorInventoryEmpty,
							GUIStyle.ColorInventoryHalf,
							GUIStyle.ColorInventoryFull
						}).ToStringHex();
						LocalizedString left = toolTip;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(24, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
						defaultInterpolatedStringHandler2.AppendFormatted(conditionColorStr);
						defaultInterpolatedStringHandler2.AppendLiteral("‖ (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>((int)item.ConditionPercentage);
						defaultInterpolatedStringHandler2.AppendLiteral(" %)‖color:end‖");
						toolTip = left + defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					if (!description.IsNullOrEmpty())
					{
						toolTip += '\n' + description;
					}
					if (item.Prefab.UnlockedRecipeInToolTip.Length != 0)
					{
						GameSession GameSession = GameMain.GameSession;
						if (GameSession != null)
						{
							if (item.Prefab.UnlockedRecipeInToolTip.All((Identifier id) => GameSession.HasUnlockedRecipe(Character.Controlled, id)))
							{
								LocalizedString left2 = toolTip;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 2);
								defaultInterpolatedStringHandler3.AppendLiteral("\n‖color:");
								defaultInterpolatedStringHandler3.AppendFormatted(GUIStyle.Green.ToStringHex());
								defaultInterpolatedStringHandler3.AppendLiteral("‖");
								defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.Get("unlockedrecipe.true"));
								defaultInterpolatedStringHandler3.AppendLiteral("‖color:end‖");
								toolTip = left2 + defaultInterpolatedStringHandler3.ToStringAndClear();
							}
							else
							{
								LocalizedString left3 = toolTip;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(20, 2);
								defaultInterpolatedStringHandler4.AppendLiteral("\n‖color:");
								defaultInterpolatedStringHandler4.AppendFormatted(GUIStyle.Yellow.ToStringHex());
								defaultInterpolatedStringHandler4.AppendLiteral("‖");
								defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(TextManager.Get("unlockedrecipe.false"));
								defaultInterpolatedStringHandler4.AppendLiteral("‖color:end‖");
								toolTip = left3 + defaultInterpolatedStringHandler4.ToStringAndClear();
							}
						}
					}
					if (item.Prefab.ContentPackage != GameMain.VanillaContent && item.Prefab.ContentPackage != null)
					{
						colorStr = Color.MediumPurple.ToStringHex();
						LocalizedString left4 = toolTip;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(20, 2);
						defaultInterpolatedStringHandler5.AppendLiteral("\n‖color:");
						defaultInterpolatedStringHandler5.AppendFormatted(colorStr);
						defaultInterpolatedStringHandler5.AppendLiteral("‖");
						defaultInterpolatedStringHandler5.AppendFormatted(item.Prefab.ContentPackage.Name);
						defaultInterpolatedStringHandler5.AppendLiteral("‖color:end‖");
						toolTip = left4 + defaultInterpolatedStringHandler5.ToStringAndClear();
					}
				}
				if (itemsInSlot.Count<Item>() > 1)
				{
					LocalizedString left5 = toolTip;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(31, 2);
					defaultInterpolatedStringHandler6.AppendLiteral("\n‖color:gui.blue‖[");
					GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
					defaultInterpolatedStringHandler6.AppendFormatted<LocalizedString>(keyMap.KeyBindText(InputType.TakeOneFromInventorySlot));
					defaultInterpolatedStringHandler6.AppendLiteral("] ");
					defaultInterpolatedStringHandler6.AppendFormatted<LocalizedString>(TextManager.Get("inputtype.takeonefrominventoryslot"));
					defaultInterpolatedStringHandler6.AppendLiteral("‖color:end‖");
					toolTip = left5 + defaultInterpolatedStringHandler6.ToStringAndClear();
					LocalizedString left6 = toolTip;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(31, 2);
					defaultInterpolatedStringHandler7.AppendLiteral("\n‖color:gui.blue‖[");
					keyMap = GameSettings.CurrentConfig.KeyMap;
					defaultInterpolatedStringHandler7.AppendFormatted<LocalizedString>(keyMap.KeyBindText(InputType.TakeHalfFromInventorySlot));
					defaultInterpolatedStringHandler7.AppendLiteral("] ");
					defaultInterpolatedStringHandler7.AppendFormatted<LocalizedString>(TextManager.Get("inputtype.takehalffrominventoryslot"));
					defaultInterpolatedStringHandler7.AppendLiteral("‖color:end‖");
					toolTip = left6 + defaultInterpolatedStringHandler7.ToStringAndClear();
				}
				if (new ImmutableArray<SkillRequirementHint>?(item.Prefab.SkillRequirementHints) != null && item.Prefab.SkillRequirementHints.Any<SkillRequirementHint>())
				{
					toolTip += item.Prefab.GetSkillRequirementHints(character);
				}
				if (PlayerInput.KeyDown(InputType.ContextualCommand))
				{
					LocalizedString left7 = toolTip;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler8.AppendLiteral("\n‖color:gui.blue‖");
					defaultInterpolatedStringHandler8.AppendFormatted<LocalizedString>(TextManager.ParseInputTypes(TextManager.Get("itemmsgcontextualorders"), false));
					defaultInterpolatedStringHandler8.AppendLiteral("‖color:end‖");
					toolTip = left7 + defaultInterpolatedStringHandler8.ToStringAndClear();
				}
				else
				{
					string colorStr2 = (Color.LightGray * 0.7f).ToStringHex();
					LocalizedString left8 = toolTip;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(20, 2);
					defaultInterpolatedStringHandler9.AppendLiteral("\n‖color:");
					defaultInterpolatedStringHandler9.AppendFormatted(colorStr2);
					defaultInterpolatedStringHandler9.AppendLiteral("‖");
					defaultInterpolatedStringHandler9.AppendFormatted<LocalizedString>(TextManager.Get("itemmsg.morreoptionsavailable"));
					defaultInterpolatedStringHandler9.AppendLiteral("‖color:end‖");
					toolTip = left8 + defaultInterpolatedStringHandler9.ToStringAndClear();
				}
				return RichString.Rich(toolTip, null);
			}

			// Token: 0x04004419 RID: 17433
			public readonly Inventory ParentInventory;

			// Token: 0x0400441A RID: 17434
			public readonly int SlotIndex;

			// Token: 0x0400441B RID: 17435
			public VisualSlot Slot;

			// Token: 0x0400441C RID: 17436
			public Inventory Inventory;

			// Token: 0x0400441D RID: 17437
			public readonly Item Item;

			// Token: 0x0400441E RID: 17438
			public readonly bool IsSubSlot;

			// Token: 0x04004420 RID: 17440
			public int tooltipDisplayedCondition;

			// Token: 0x04004421 RID: 17441
			public bool tooltipShowedContextualOptions;

			// Token: 0x04004422 RID: 17442
			public bool ForceTooltipRefresh;
		}

		// Token: 0x02000A62 RID: 2658
		public class ItemSlot
		{
			// Token: 0x17001A7E RID: 6782
			// (get) Token: 0x060074F9 RID: 29945 RVA: 0x00374A24 File Offset: 0x00372C24
			public IReadOnlyList<Item> Items
			{
				get
				{
					return this.items;
				}
			}

			// Token: 0x060074FA RID: 29946 RVA: 0x00374A2C File Offset: 0x00372C2C
			public ItemSlot(Inventory inventory)
			{
				this.inventory = inventory;
			}

			// Token: 0x060074FB RID: 29947 RVA: 0x00374A48 File Offset: 0x00372C48
			public bool CanBePut(Item item, bool ignoreCondition = false)
			{
				if (item == null)
				{
					return false;
				}
				if (this.items.Count > 0)
				{
					if (!ignoreCondition)
					{
						if (item.IsFullCondition)
						{
							if (this.items.Any((Item it) => !it.IsFullCondition))
							{
								return false;
							}
						}
						else
						{
							if (!MathUtils.NearlyEqual(item.Condition, 0f, 0.0001f))
							{
								return false;
							}
							if (this.items.Any((Item it) => !MathUtils.NearlyEqual(it.Condition, 0f, 0.0001f)))
							{
								return false;
							}
						}
					}
					if (this.items[0].Quality != item.Quality)
					{
						return false;
					}
					if (this.items[0].Prefab.Identifier != item.Prefab.Identifier || this.items.Count + 1 > item.Prefab.GetMaxStackSize(this.inventory))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x060074FC RID: 29948 RVA: 0x00374B54 File Offset: 0x00372D54
			public bool CanProbablyBePut(ItemPrefab itemPrefab, float? condition = null, int? quality = null)
			{
				if (itemPrefab == null)
				{
					return false;
				}
				if (this.items.Count > 0)
				{
					if (condition != null)
					{
						if (MathUtils.NearlyEqual(condition.Value, 0f, 0.0001f))
						{
							if (this.items.Any((Item it) => it.Condition > 0f))
							{
								return false;
							}
						}
						else
						{
							if (!MathUtils.NearlyEqual(condition.Value, itemPrefab.Health, 0.0001f))
							{
								return false;
							}
							if (this.items.Any((Item it) => !it.IsFullCondition))
							{
								return false;
							}
						}
					}
					else if (this.items.Any((Item it) => !it.IsFullCondition))
					{
						return false;
					}
					if (quality != null && this.items[0].Quality != quality.Value)
					{
						return false;
					}
					if (this.items[0].Prefab.Identifier != itemPrefab.Identifier || this.items.Count + 1 > itemPrefab.GetMaxStackSize(this.inventory))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x060074FD RID: 29949 RVA: 0x00374CA8 File Offset: 0x00372EA8
			public int HowManyCanBePut(ItemPrefab itemPrefab, int? maxStackSize = null, float? condition = null, bool ignoreItemsInSlot = false)
			{
				if (itemPrefab == null)
				{
					return 0;
				}
				int value = maxStackSize.GetValueOrDefault();
				if (maxStackSize == null)
				{
					value = itemPrefab.GetMaxStackSize(this.inventory);
					maxStackSize = new int?(value);
				}
				if (this.items.Count <= 0 || ignoreItemsInSlot)
				{
					return maxStackSize.Value;
				}
				if (condition != null)
				{
					if (MathUtils.NearlyEqual(condition.Value, 0f, 0.0001f))
					{
						if (this.items.Any((Item it) => it.Condition > 0f))
						{
							return 0;
						}
					}
					else
					{
						if (!MathUtils.NearlyEqual(condition.Value, itemPrefab.Health, 0.0001f))
						{
							return 0;
						}
						if (this.items.Any((Item it) => !it.IsFullCondition))
						{
							return 0;
						}
					}
				}
				else if (this.items.Any((Item it) => !it.IsFullCondition))
				{
					return 0;
				}
				if (this.items[0].Prefab.Identifier != itemPrefab.Identifier)
				{
					return 0;
				}
				return maxStackSize.Value - this.items.Count;
			}

			// Token: 0x060074FE RID: 29950 RVA: 0x00374E04 File Offset: 0x00373004
			public void Add(Item item)
			{
				if (item == null)
				{
					throw new InvalidOperationException("Tried to add a null item to an inventory slot.");
				}
				if (this.items.Count > 0)
				{
					if (this.items[0].Prefab.Identifier != item.Prefab.Identifier)
					{
						throw new InvalidOperationException("Tried to stack different types of items.");
					}
					if (this.items.Count + 1 > item.Prefab.GetMaxStackSize(this.inventory))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Tried to add an item to a full inventory slot (stack already full, x");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.items.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.items.First<Item>().Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(").");
						throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				if (this.items.Contains(item))
				{
					return;
				}
				int index = 0;
				int i = 0;
				while (i < this.items.Count && this.items[i].Condition <= item.Condition)
				{
					index++;
					i++;
				}
				this.items.Insert(index, item);
			}

			// Token: 0x060074FF RID: 29951 RVA: 0x00374F40 File Offset: 0x00373140
			public Item RemoveItem()
			{
				if (this.items.Count == 0)
				{
					return null;
				}
				Item item = this.items[0];
				this.items.RemoveAt(0);
				return item;
			}

			// Token: 0x06007500 RID: 29952 RVA: 0x00374F76 File Offset: 0x00373176
			public void RemoveItem(Item item)
			{
				this.items.Remove(item);
			}

			// Token: 0x06007501 RID: 29953 RVA: 0x00374F85 File Offset: 0x00373185
			public void RemoveAllItems()
			{
				this.items.Clear();
			}

			// Token: 0x06007502 RID: 29954 RVA: 0x00374F94 File Offset: 0x00373194
			public void RemoveWhere(Func<Item, bool> predicate)
			{
				this.items.RemoveAll((Item it) => predicate(it));
			}

			// Token: 0x06007503 RID: 29955 RVA: 0x00374FC6 File Offset: 0x003731C6
			public bool Any()
			{
				return this.items.Count > 0;
			}

			// Token: 0x06007504 RID: 29956 RVA: 0x00374FD6 File Offset: 0x003731D6
			public bool Empty()
			{
				return this.items.Count == 0;
			}

			// Token: 0x06007505 RID: 29957 RVA: 0x00374FE6 File Offset: 0x003731E6
			public Item First()
			{
				return this.items[0];
			}

			// Token: 0x06007506 RID: 29958 RVA: 0x00374FF4 File Offset: 0x003731F4
			public Item FirstOrDefault()
			{
				return this.items.FirstOrDefault<Item>();
			}

			// Token: 0x06007507 RID: 29959 RVA: 0x00375001 File Offset: 0x00373201
			public Item LastOrDefault()
			{
				return this.items.LastOrDefault<Item>();
			}

			// Token: 0x06007508 RID: 29960 RVA: 0x0037500E File Offset: 0x0037320E
			public bool Contains(Item item)
			{
				return this.items.Contains(item);
			}

			// Token: 0x04004423 RID: 17443
			private readonly List<Item> items = new List<Item>(63);

			// Token: 0x04004424 RID: 17444
			public bool HideIfEmpty;

			// Token: 0x04004425 RID: 17445
			private readonly Inventory inventory;
		}
	}
}
