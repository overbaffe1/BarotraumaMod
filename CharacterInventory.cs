using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000C2 RID: 194
	internal class CharacterInventory : Inventory
	{
		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x060017D7 RID: 6103 RVA: 0x000EB180 File Offset: 0x000E9380
		public static Dictionary<InvSlotType, Sprite> LimbSlotIcons
		{
			get
			{
				if (CharacterInventory.limbSlotIcons == null)
				{
					CharacterInventory.limbSlotIcons = new Dictionary<InvSlotType, Sprite>();
					foreach (object obj in Enum.GetValues(typeof(InvSlotType)))
					{
						InvSlotType invSlotType = (InvSlotType)obj;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
						defaultInterpolatedStringHandler.AppendLiteral("InventorySlot.");
						defaultInterpolatedStringHandler.AppendFormatted<InvSlotType>(invSlotType);
						GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
						Sprite sprite = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
						if (sprite != null)
						{
							CharacterInventory.limbSlotIcons.Add(invSlotType, sprite);
						}
					}
					int margin = 2;
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.Headset, new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(384 + margin, 128 + margin, 128 - margin * 2, 128 - margin * 2)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.InnerClothes, new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(512 + margin, 128 + margin, 128 - margin * 2, 128 - margin * 2)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.Card, new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(640 + margin, 128 + margin, 128 - margin * 2, 128 - margin * 2)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.Head, new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(896 + margin, 128 + margin, 128 - margin * 2, 128 - margin * 2)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.LeftHand, new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(634, 0, 128, 128)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.RightHand, new Sprite("Content/UI/InventoryUIAtlas.png", new Rectangle?(new Rectangle(762, 0, 128, 128)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.OuterClothes, new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(256 + margin, 128 + margin, 128 - margin * 2, 128 - margin * 2)), null, 0f));
					CharacterInventory.<get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType.Bag, new Sprite("Content/UI/CommandUIAtlas.png", new Rectangle?(new Rectangle(639, 926, 128, 80)), null, 0f));
				}
				return CharacterInventory.limbSlotIcons;
			}
		}

		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x060017D8 RID: 6104 RVA: 0x000EB464 File Offset: 0x000E9664
		// (set) Token: 0x060017D9 RID: 6105 RVA: 0x000EB46C File Offset: 0x000E966C
		public CharacterInventory.Layout CurrentLayout
		{
			get
			{
				return this.layout;
			}
			set
			{
				if (this.layout == value)
				{
					return;
				}
				this.layout = value;
				this.SetSlotPositions(this.layout);
			}
		}

		// Token: 0x060017DA RID: 6106 RVA: 0x000EB48C File Offset: 0x000E968C
		protected override ItemInventory GetActiveEquippedSubInventory(int slotIndex)
		{
			Item item = this.slots[slotIndex].FirstOrDefault();
			if (item == null)
			{
				return null;
			}
			ItemContainer container = item.GetComponent<ItemContainer>();
			if (container == null || !container.KeepOpenWhenEquippedBy(this.character))
			{
				return null;
			}
			return container.Inventory;
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x000EB4CC File Offset: 0x000E96CC
		public override void CreateSlots()
		{
			if (this.visualSlots == null)
			{
				this.visualSlots = new VisualSlot[this.capacity];
			}
			float multiplier = Inventory.UIScale * GUI.AspectRatioAdjustment;
			for (int i = 0; i < this.capacity; i++)
			{
				VisualSlot prevSlot = this.visualSlots[i];
				Sprite slotSprite = Inventory.SlotSpriteSmall;
				Rectangle slotRect = new Rectangle((int)this.SlotPositions[i].X, (int)this.SlotPositions[i].Y, (int)(slotSprite.size.X * multiplier), (int)(slotSprite.size.Y * multiplier));
				Item item = this.slots[i].FirstOrDefault();
				ItemContainer itemContainer = (item != null) ? item.GetComponent<ItemContainer>() : null;
				if (itemContainer != null)
				{
					if (itemContainer.InventoryTopSprite != null)
					{
						slotRect.Width = Math.Max(slotRect.Width, (int)(itemContainer.InventoryTopSprite.size.X * Inventory.UIScale));
					}
					if (itemContainer.InventoryBottomSprite != null)
					{
						slotRect.Width = Math.Max(slotRect.Width, (int)(itemContainer.InventoryBottomSprite.size.X * Inventory.UIScale));
					}
				}
				this.visualSlots[i] = new VisualSlot(slotRect)
				{
					SubInventoryDir = Math.Sign(GameMain.GraphicsHeight / 2 - slotRect.Center.Y),
					Disabled = false,
					SlotSprite = slotSprite,
					Color = ((this.SlotTypes[i] == InvSlotType.Any) ? (Color.White * 0.2f) : (Color.White * 0.4f))
				};
				if (prevSlot != null)
				{
					this.visualSlots[i].DrawOffset = prevSlot.DrawOffset;
					this.visualSlots[i].Color = prevSlot.Color;
					prevSlot.MoveBorderHighlight(this.visualSlots[i]);
				}
				Inventory.SlotReference selectedSlot = Inventory.selectedSlot;
				if (((selectedSlot != null) ? selectedSlot.ParentInventory : null) == this && Inventory.selectedSlot.SlotIndex == i)
				{
					Inventory.selectedSlot = new Inventory.SlotReference(this, this.visualSlots[i], i, Inventory.selectedSlot.IsSubSlot, Inventory.selectedSlot.Inventory);
				}
			}
			this.AssignQuickUseNumKeys();
			Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.Inventory.OpenState <= 0f);
			foreach (Inventory.SlotReference subSlot in Inventory.highlightedSubInventorySlots)
			{
				if (subSlot.ParentInventory == this && subSlot.SlotIndex > 0 && subSlot.SlotIndex < this.visualSlots.Length)
				{
					subSlot.Slot = this.visualSlots[subSlot.SlotIndex];
				}
			}
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.CalculateBackgroundFrame();
		}

		// Token: 0x060017DC RID: 6108 RVA: 0x000EB7A4 File Offset: 0x000E99A4
		protected override void CalculateBackgroundFrame()
		{
			Rectangle frame = Rectangle.Empty;
			for (int i = 0; i < this.capacity; i++)
			{
				if (!this.HideSlot(i))
				{
					if (frame == Rectangle.Empty)
					{
						frame = this.visualSlots[i].Rect;
					}
					else
					{
						frame = Rectangle.Union(frame, this.visualSlots[i].Rect);
					}
				}
			}
			frame.Inflate(10, 30);
			frame.Location -= new Point(0, 25);
			base.BackgroundFrame = frame;
		}

		// Token: 0x060017DD RID: 6109 RVA: 0x000EB830 File Offset: 0x000E9A30
		public override bool HideSlot(int i)
		{
			if (this.visualSlots[i].Disabled || (this.slots[i].HideIfEmpty && this.slots[i].Empty()))
			{
				return true;
			}
			if (this.SlotTypes[i] == InvSlotType.HealthInterface)
			{
				if (CharacterHealth.OpenHealthWindow == null || Character.Controlled == null)
				{
					return true;
				}
				if (this.character == Character.Controlled)
				{
					if (CharacterHealth.OpenHealthWindow != Character.Controlled.CharacterHealth)
					{
						return true;
					}
				}
				else if (this.character == Character.Controlled.SelectedCharacter)
				{
					CharacterHealth openHealthWindow = CharacterHealth.OpenHealthWindow;
					Character controlled = Character.Controlled;
					object obj;
					if (controlled == null)
					{
						obj = null;
					}
					else
					{
						Character selectedCharacter = controlled.SelectedCharacter;
						obj = ((selectedCharacter != null) ? selectedCharacter.CharacterHealth : null);
					}
					if (openHealthWindow != obj)
					{
						return true;
					}
					if (this.character.IsPlayer && !this.character.IsIncapacitated)
					{
						return true;
					}
				}
			}
			if (this.layout == CharacterInventory.Layout.Default && (InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.SlotTypes[i]) && !this.personalSlotArea.Contains(this.visualSlots[i].Rect.Center + this.visualSlots[i].DrawOffset.ToPoint()))
			{
				return true;
			}
			Item item = this.slots[i].FirstOrDefault();
			return (item != null && this.SlotTypes[i] == InvSlotType.RightHand && this.IsInLimbSlot(item, InvSlotType.LeftHand)) || (item != null && this.SlotTypes[i] != InvSlotType.Any && this.IsInLimbSlot(item, InvSlotType.Any)) || (Screen.Selected == GameMain.SubEditorScreen && GameMain.SubEditorScreen.WiringMode && this.SlotTypes[i] != InvSlotType.Any && this.SlotTypes[i] != InvSlotType.LeftHand && this.SlotTypes[i] != InvSlotType.RightHand);
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x000EB9E6 File Offset: 0x000E9BE6
		public void RefreshSlotPositions()
		{
			this.SetSlotPositions(this.CurrentLayout);
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x000EB9F4 File Offset: 0x000E9BF4
		private void SetSlotPositions(CharacterInventory.Layout layout)
		{
			CharacterInventory.<>c__DisplayClass19_0 CS$<>8__locals1;
			CS$<>8__locals1.spacing = GUI.IntScale(5f);
			CharacterInventory.SlotSize = (Inventory.SlotSpriteSmall.size * Inventory.UIScale * GUI.AspectRatioAdjustment).ToPoint();
			int bottomOffset = CharacterInventory.<SetSlotPositions>g__GetBottomOffset|19_0(2, ref CS$<>8__locals1);
			int personalSlotY = CharacterInventory.<SetSlotPositions>g__GetVerticalOffsetFromBottom|19_1(2, ref CS$<>8__locals1);
			if (this.visualSlots == null)
			{
				this.CreateSlots();
			}
			if (this.visualSlots.None(null))
			{
				return;
			}
			switch (layout)
			{
			case CharacterInventory.Layout.Default:
			{
				int personalSlotCount = this.SlotTypes.Count((InvSlotType s) => (InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(s));
				int normalSlotCount = this.SlotTypes.Count((InvSlotType s) => !(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(s) && s != InvSlotType.HealthInterface);
				int x = GameMain.GraphicsWidth / 2 - normalSlotCount * (CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing) / 2;
				int upperX = HUDLayoutSettings.BottomRightInfoArea.X - CharacterInventory.SlotSize.X - CS$<>8__locals1.spacing;
				x -= Math.Max(x + normalSlotCount * (CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing) - (upperX - personalSlotCount * (CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing)), 0);
				int hideButtonSlotIndex = -1;
				for (int i = 0; i < this.SlotPositions.Length; i++)
				{
					if ((InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.SlotTypes[i]))
					{
						this.SlotPositions[i] = new Vector2((float)upperX, (float)(GameMain.GraphicsHeight - bottomOffset));
						upperX -= CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing;
						this.personalSlotArea = ((hideButtonSlotIndex == -1) ? new Rectangle(this.SlotPositions[i].ToPoint(), CharacterInventory.SlotSize) : Rectangle.Union(this.personalSlotArea, new Rectangle(this.SlotPositions[i].ToPoint(), CharacterInventory.SlotSize)));
						hideButtonSlotIndex = i;
					}
					else
					{
						this.SlotPositions[i] = new Vector2((float)x, (float)(GameMain.GraphicsHeight - bottomOffset));
						x += CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing;
					}
				}
				break;
			}
			case CharacterInventory.Layout.Left:
			{
				int x2 = HUDLayoutSettings.InventoryAreaLower.X;
				if (!GUI.IsUltrawide && GUI.IsHUDScaled)
				{
					x2 -= HUDLayoutSettings.ChatBoxArea.Width - 100;
				}
				int personalSlotX = x2;
				float y = (float)(GameMain.GraphicsHeight - bottomOffset);
				for (int j = 0; j < this.SlotPositions.Length; j++)
				{
					if (!this.HideSlot(j) && this.SlotTypes[j] != InvSlotType.HealthInterface && this.SlotTypes[j] != InvSlotType.RightHand && this.SlotTypes[j] != InvSlotType.LeftHand)
					{
						if ((InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.SlotTypes[j]))
						{
							this.SlotPositions[j] = new Vector2((float)personalSlotX, (float)personalSlotY);
							personalSlotX += this.visualSlots[j].Rect.Width + CS$<>8__locals1.spacing;
						}
						else
						{
							this.SlotPositions[j] = new Vector2((float)x2, y);
							x2 += this.visualSlots[j].Rect.Width + CS$<>8__locals1.spacing;
						}
					}
				}
				int handSlotX = x2 - this.visualSlots[0].Rect.Width - CS$<>8__locals1.spacing;
				for (int k = 0; k < this.SlotPositions.Length; k++)
				{
					if (this.SlotTypes[k] == InvSlotType.RightHand || this.SlotTypes[k] == InvSlotType.LeftHand)
					{
						bool rightSlot = this.SlotTypes[k] == InvSlotType.RightHand;
						this.SlotPositions[k] = new Vector2((float)(rightSlot ? handSlotX : (handSlotX - this.visualSlots[0].Rect.Width - CS$<>8__locals1.spacing)), (float)personalSlotY);
					}
					else if (this.HideSlot(k) && this.SlotTypes[k] != InvSlotType.HealthInterface)
					{
						this.SlotPositions[k] = new Vector2((float)x2, y);
						x2 += this.visualSlots[k].Rect.Width + CS$<>8__locals1.spacing;
					}
				}
				break;
			}
			case CharacterInventory.Layout.Right:
			{
				int x3 = HUDLayoutSettings.InventoryAreaLower.Right;
				int personalSlotX2 = HUDLayoutSettings.InventoryAreaLower.Right - CharacterInventory.SlotSize.X - CS$<>8__locals1.spacing;
				for (int l = 0; l < this.visualSlots.Length; l++)
				{
					if (!this.HideSlot(l) && this.SlotTypes[l] != InvSlotType.HealthInterface && this.SlotTypes[l] != InvSlotType.RightHand && this.SlotTypes[l] != InvSlotType.LeftHand && !(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.SlotTypes[l]))
					{
						x3 -= CharacterInventory.SlotSize.X + CS$<>8__locals1.spacing;
					}
				}
				int lowerX = x3;
				int handSlotX2 = x3;
				for (int m = 0; m < this.SlotPositions.Length; m++)
				{
					if (this.SlotTypes[m] == InvSlotType.RightHand || this.SlotTypes[m] == InvSlotType.LeftHand)
					{
						this.SlotPositions[m] = new Vector2((float)handSlotX2, (float)personalSlotY);
						handSlotX2 += this.visualSlots[m].Rect.Width + CS$<>8__locals1.spacing;
					}
					else if (!this.HideSlot(m) && this.SlotTypes[m] != InvSlotType.HealthInterface)
					{
						if ((InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.SlotTypes[m]))
						{
							this.SlotPositions[m] = new Vector2((float)personalSlotX2, (float)personalSlotY);
							personalSlotX2 -= this.visualSlots[m].Rect.Width + CS$<>8__locals1.spacing;
						}
						else
						{
							this.SlotPositions[m] = new Vector2((float)x3, (float)(GameMain.GraphicsHeight - bottomOffset));
							x3 += this.visualSlots[m].Rect.Width + CS$<>8__locals1.spacing;
						}
					}
				}
				x3 = lowerX;
				for (int n = 0; n < this.SlotPositions.Length; n++)
				{
					if (this.HideSlot(n) && this.SlotTypes[n] != InvSlotType.HealthInterface && this.SlotTypes[n] != InvSlotType.RightHand && this.SlotTypes[n] != InvSlotType.LeftHand)
					{
						x3 -= this.visualSlots[n].Rect.Width + CS$<>8__locals1.spacing;
						this.SlotPositions[n] = new Vector2((float)x3, (float)(GameMain.GraphicsHeight - bottomOffset));
					}
				}
				break;
			}
			case CharacterInventory.Layout.Center:
			{
				int columns = 5;
				int startX = GameMain.GraphicsWidth / 2 - (CharacterInventory.SlotSize.X * columns + CS$<>8__locals1.spacing * (columns - 1)) / 2;
				int startY = GameMain.GraphicsHeight / 2 - CharacterInventory.SlotSize.Y * 2;
				int x4 = startX;
				int y2 = startY;
				for (int i2 = 0; i2 < this.SlotPositions.Length; i2++)
				{
					if (!this.HideSlot(i2) && this.SlotTypes[i2] != InvSlotType.HealthInterface && (this.SlotTypes[i2] == InvSlotType.Card || this.SlotTypes[i2] == InvSlotType.Headset || this.SlotTypes[i2] == InvSlotType.InnerClothes))
					{
						this.SlotPositions[i2] = new Vector2((float)x4, (float)y2);
						x4 += this.visualSlots[i2].Rect.Width + CS$<>8__locals1.spacing;
					}
				}
				y2 += this.visualSlots[0].Rect.Height + CS$<>8__locals1.spacing + Inventory.ContainedIndicatorHeight + this.visualSlots[0].EquipButtonRect.Height;
				x4 = startX;
				int n2 = 0;
				for (int i3 = 0; i3 < this.SlotPositions.Length; i3++)
				{
					if (!this.HideSlot(i3) && this.SlotTypes[i3] != InvSlotType.HealthInterface && this.SlotTypes[i3] != InvSlotType.Card && this.SlotTypes[i3] != InvSlotType.Headset && this.SlotTypes[i3] != InvSlotType.InnerClothes)
					{
						this.SlotPositions[i3] = new Vector2((float)x4, (float)y2);
						x4 += this.visualSlots[i3].Rect.Width + CS$<>8__locals1.spacing;
						n2++;
						if (n2 >= columns)
						{
							x4 = startX;
							y2 += this.visualSlots[i3].Rect.Height + CS$<>8__locals1.spacing + Inventory.ContainedIndicatorHeight + this.visualSlots[i3].EquipButtonRect.Height;
							n2 = 0;
						}
					}
				}
				break;
			}
			}
			CharacterHealth characterHealth = this.character.CharacterHealth;
			if (characterHealth != null && characterHealth.UseHealthWindow)
			{
				Vector2 pos = this.character.CharacterHealth.InventorySlotContainer.Rect.Location.ToVector2();
				for (int i4 = 0; i4 < this.capacity; i4++)
				{
					if (this.SlotTypes[i4] == InvSlotType.HealthInterface)
					{
						this.SlotPositions[i4] = pos;
						pos.Y += (float)(this.visualSlots[i4].Rect.Height + CS$<>8__locals1.spacing);
					}
				}
			}
			this.CreateSlots();
			if (layout == CharacterInventory.Layout.Default)
			{
				HUDLayoutSettings.InventoryTopY = this.visualSlots[0].EquipButtonRect.Y - (int)(15f * GUI.Scale);
				return;
			}
			for (int i5 = 0; i5 < this.capacity; i5++)
			{
				this.visualSlots[i5].DrawOffset = Vector2.Zero;
			}
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x000EC3DA File Offset: 0x000EA5DA
		protected override void ControlInput(Camera cam)
		{
			base.ControlInput(cam);
			if (Inventory.highlightedSubInventorySlots.Any((Inventory.SlotReference i) => i.Inventory != null && i.Inventory.BackgroundFrame.Contains(PlayerInput.MousePosition)))
			{
				cam.Freeze = true;
			}
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x000EC418 File Offset: 0x000EA618
		public override void Update(float deltaTime, Camera cam, bool isSubInventory = false)
		{
			if (!this.AccessibleWhenAlive && !this.character.IsDead && !this.AccessibleByOwner)
			{
				this.syncItemsDelay = Math.Max(this.syncItemsDelay - deltaTime, 0f);
				Inventory.doubleClickedItems.Clear();
				return;
			}
			base.Update(deltaTime, cam, false);
			bool hoverOnInventory = GUI.MouseOn == null && ((Inventory.selectedSlot != null && Inventory.selectedSlot.IsSubSlot) || (Inventory.DraggingItems.Any<Item>() && (Inventory.DraggingSlot == null || !Inventory.DraggingSlot.MouseOn())));
			if (CharacterHealth.OpenHealthWindow != null)
			{
				hoverOnInventory = true;
			}
			if (hoverOnInventory)
			{
				this.HideTimer = 0.5f;
			}
			if (this.HideTimer > 0f)
			{
				this.HideTimer -= deltaTime;
			}
			this.UpdateSlotInput();
			CharacterInventory.hideSubInventories.Clear();
			Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.ParentInventory == this && (s.SlotIndex < 0 || s.SlotIndex >= this.slots.Length || this.slots[s.SlotIndex] == null || (Character.Controlled != null && !Character.Controlled.CanAccessInventory(s.Inventory, CharacterInventory.AccessLevel.AllowBotsAndPets))));
			Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.Item != null && s.ParentInventory == this && s.Item.ParentInventory != this);
			Inventory.highlightedSubInventorySlots.RemoveWhere((Inventory.SlotReference s) => s.Item != null && s.ParentInventory == this && Inventory.DraggingItems.Contains(s.Item));
			CharacterInventory.tempHighlightedSubInventorySlots.Clear();
			CharacterInventory.tempHighlightedSubInventorySlots.AddRange(Inventory.highlightedSubInventorySlots);
			foreach (Inventory.SlotReference highlightedSubInventorySlot in CharacterInventory.tempHighlightedSubInventorySlots)
			{
				if (highlightedSubInventorySlot.ParentInventory == this)
				{
					base.UpdateSubInventory(deltaTime, highlightedSubInventorySlot.SlotIndex, cam);
				}
				if (highlightedSubInventorySlot.Inventory.IsInventoryHoverAvailable(this.character, null))
				{
					Rectangle hoverArea = Inventory.GetSubInventoryHoverArea(highlightedSubInventorySlot);
					Inventory inventory = highlightedSubInventorySlot.Inventory;
					if (((inventory != null) ? inventory.visualSlots : null) == null || !hoverArea.Contains(PlayerInput.MousePosition))
					{
						CharacterInventory.hideSubInventories.Add(highlightedSubInventorySlot);
					}
					else
					{
						highlightedSubInventorySlot.Inventory.HideTimer = 1f;
					}
				}
			}
			Inventory.SlotReference selectedSlot = Inventory.selectedSlot;
			if (((selectedSlot != null) ? selectedSlot.ParentInventory : null) == this)
			{
				Inventory subInventory = base.GetSubInventory(Inventory.selectedSlot.SlotIndex);
				if (subInventory != null && subInventory.IsInventoryHoverAvailable(this.character, null))
				{
					Inventory.selectedSlot.Inventory = subInventory;
					if (!Inventory.highlightedSubInventorySlots.Any((Inventory.SlotReference s) => s.Inventory == subInventory))
					{
						this.ShowSubInventory(Inventory.selectedSlot, deltaTime, cam, CharacterInventory.hideSubInventories, false);
					}
				}
			}
			SubEditorScreen subEditor = Screen.Selected as SubEditorScreen;
			if (subEditor != null && !subEditor.WiringMode)
			{
				for (int k = 0; k < this.visualSlots.Length; k++)
				{
					Inventory subInventory2 = base.GetSubInventory(k);
					if (subInventory2 != null)
					{
						this.ShowSubInventory(new Inventory.SlotReference(this, this.visualSlots[k], k, false, subInventory2), deltaTime, cam, CharacterInventory.hideSubInventories, true);
					}
				}
			}
			foreach (Inventory.SlotReference subInventorySlot in CharacterInventory.hideSubInventories)
			{
				if (subInventorySlot.Inventory != null)
				{
					subInventorySlot.Inventory.HideTimer -= deltaTime;
					if (subInventorySlot.Inventory.HideTimer < 0.25f)
					{
						Inventory.highlightedSubInventorySlots.Remove(subInventorySlot);
					}
				}
			}
			if (this.character == Character.Controlled && this.character.SelectedCharacter == null)
			{
				Inventory.highlightedSubInventorySlots.RemoveWhere(delegate(Inventory.SlotReference s)
				{
					if (s.ParentInventory != this)
					{
						Inventory parentInventory = s.ParentInventory;
						return ((parentInventory != null) ? parentInventory.Owner : null) is Character;
					}
					return false;
				});
				int i2;
				int i;
				for (i = 0; i < this.capacity; i = i2 + 1)
				{
					Item item = this.slots[i].FirstOrDefault();
					if (item != null && !this.HideSlot(i) && this.character.HasEquippedItem(item, null, null))
					{
						ItemContainer itemContainer = item.GetComponent<ItemContainer>();
						if (itemContainer != null && itemContainer.KeepOpenWhenEquippedBy(this.character) && !Inventory.DraggingItems.Contains(item) && this.character.CanAccessInventory(itemContainer.Inventory, CharacterInventory.AccessLevel.AllowBotsAndPets) && !Inventory.highlightedSubInventorySlots.Any((Inventory.SlotReference s) => s.Inventory == itemContainer.Inventory && s.SlotIndex == i))
						{
							this.ShowSubInventory(new Inventory.SlotReference(this, this.visualSlots[i], i, false, itemContainer.Inventory), deltaTime, cam, CharacterInventory.hideSubInventories, true);
						}
					}
					i2 = i;
				}
			}
			if (Inventory.doubleClickedItems.Any<Item>())
			{
				CharacterInventory.QuickUseAction quickUseAction = this.GetQuickUseAction(Inventory.doubleClickedItems.First<Item>(), true, true, true);
				int itemCount = 0;
				foreach (Item doubleClickedItem in Inventory.doubleClickedItems)
				{
					this.QuickUseItem(doubleClickedItem, true, true, true, new CharacterInventory.QuickUseAction?(quickUseAction), doubleClickedItem == Inventory.doubleClickedItems.First<Item>());
					itemCount++;
					if (quickUseAction == CharacterInventory.QuickUseAction.Equip)
					{
						break;
					}
					if (quickUseAction == CharacterInventory.QuickUseAction.UseTreatment)
					{
						break;
					}
					if (doubleClickedItem.ParentInventory == this && !this.IsInLimbSlot(doubleClickedItem, InvSlotType.Any))
					{
						break;
					}
					if (quickUseAction == CharacterInventory.QuickUseAction.PutToContainer)
					{
						Item selectedItem = this.character.SelectedItem;
						int? num;
						if (selectedItem == null)
						{
							num = null;
						}
						else
						{
							ItemContainer component = selectedItem.GetComponent<ItemContainer>();
							num = ((component != null) ? new int?(component.MaxStackSize) : null);
						}
						int? num2 = num;
						if (num2.GetValueOrDefault() <= 1)
						{
							break;
						}
					}
					if ((quickUseAction == CharacterInventory.QuickUseAction.TakeFromContainer || quickUseAction == CharacterInventory.QuickUseAction.PutToEquippedItem) && doubleClickedItem.ParentInventory != null && itemCount >= doubleClickedItem.Prefab.GetMaxStackSize(doubleClickedItem.ParentInventory))
					{
						break;
					}
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (!this.HideSlot(j))
				{
					Item item2 = this.slots[j].FirstOrDefault();
					if (item2 != null)
					{
						VisualSlot slot = this.visualSlots[j];
						if (item2.AllowedSlots.Any((InvSlotType a) => a != InvSlotType.Any && a != InvSlotType.HealthInterface))
						{
							this.HandleButtonEquipStates(item2, slot, deltaTime);
						}
					}
				}
			}
			if (Inventory.DraggingItems.Any<Item>())
			{
				Item rootContainer = Inventory.DraggingItems.First<Item>().RootContainer;
				Inventory rootInventory = Inventory.DraggingItems.First<Item>().ParentInventory;
				if (rootContainer != null)
				{
					rootInventory = (rootContainer.ParentInventory ?? rootContainer.GetComponent<ItemContainer>().Inventory);
				}
				if (rootInventory != null && rootInventory.Owner != Character.Controlled && rootInventory.Owner != Character.Controlled.SelectedItem && rootInventory.Owner != Character.Controlled.SelectedCharacter && (rootContainer == null || !rootContainer.DisplaySideBySideWhenLinked || Character.Controlled.SelectedItem == null || !rootContainer.linkedTo.Contains(Character.Controlled.SelectedItem)))
				{
					Inventory.DraggingItems.Clear();
				}
			}
			Inventory.doubleClickedItems.Clear();
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x000ECB60 File Offset: 0x000EAD60
		public void UpdateSlotInput()
		{
			for (int i = 0; i < this.capacity; i++)
			{
				Item firstItem = this.slots[i].FirstOrDefault();
				if (firstItem != null && !Inventory.DraggingItems.Contains(firstItem))
				{
					Character controlled = Character.Controlled;
					if (((controlled != null) ? controlled.Inventory : null) == this && GUI.KeyboardDispatcher.Subscriber == null && !CrewManager.IsCommandInterfaceOpen && PlayerInput.InventoryKeyHit(this.visualSlots[i].InventoryKeyIndex) && (!SubEditorScreen.IsSubEditor() || !SubEditorScreen.SkipInventorySlotUpdate))
					{
						CharacterInventory.QuickUseAction quickUseAction = this.GetQuickUseAction(firstItem, true, false, true);
						foreach (Item itemToUse in this.slots[i].Items.ToList<Item>())
						{
							this.QuickUseItem(itemToUse, true, true, true, new CharacterInventory.QuickUseAction?(quickUseAction), itemToUse == firstItem);
							if (quickUseAction == CharacterInventory.QuickUseAction.Equip)
							{
								break;
							}
							if (quickUseAction == CharacterInventory.QuickUseAction.UseTreatment)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x000ECC74 File Offset: 0x000EAE74
		private void HandleButtonEquipStates(Item item, VisualSlot slot, float deltaTime)
		{
			slot.EquipButtonState = (slot.EquipButtonRect.Contains(PlayerInput.MousePosition) ? GUIComponent.ComponentState.Hover : GUIComponent.ComponentState.None);
			if (PlayerInput.PrimaryMouseButtonHeld() && PlayerInput.SecondaryMouseButtonHeld())
			{
				slot.EquipButtonState = GUIComponent.ComponentState.None;
			}
			if (slot.EquipButtonState != GUIComponent.ComponentState.Hover)
			{
				slot.QuickUseTimer = Math.Max(0f, slot.QuickUseTimer - deltaTime * 5f);
				return;
			}
			CharacterInventory.QuickUseAction quickUseAction = this.GetQuickUseAction(item, true, false, false);
			if (quickUseAction != CharacterInventory.QuickUseAction.Drop)
			{
				LocalizedString quickUseButtonToolTip;
				if (quickUseAction != CharacterInventory.QuickUseAction.None)
				{
					string tag = "QuickUseAction." + quickUseAction.ToString();
					string varName = "[equippeditem]";
					Item item2 = this.character.HeldItems.FirstOrDefault<Item>();
					quickUseButtonToolTip = TextManager.GetWithVariable(tag, varName, ((item2 != null) ? item2.Name : null) ?? ((item != null) ? item.Name : null), FormatCapitals.No);
				}
				else
				{
					quickUseButtonToolTip = "";
				}
				slot.QuickUseButtonToolTip = quickUseButtonToolTip;
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					slot.EquipButtonState = GUIComponent.ComponentState.Pressed;
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					this.QuickUseItem(item, true, false, false, null, true);
				}
			}
		}

		// Token: 0x060017E4 RID: 6116 RVA: 0x000ECD80 File Offset: 0x000EAF80
		private void ShowSubInventory(Inventory.SlotReference slotRef, float deltaTime, Camera cam, List<Inventory.SlotReference> hideSubInventories, bool isEquippedSubInventory)
		{
			Rectangle hoverArea = Inventory.GetSubInventoryHoverArea(slotRef);
			if (isEquippedSubInventory)
			{
				ItemInventory itemInventory = slotRef.Inventory as ItemInventory;
				if (itemInventory != null)
				{
					ItemContainer container = itemInventory.Container;
					if (container != null && container.MovableFrame && container.KeepOpenWhenEquipped)
					{
						goto IL_7D;
					}
				}
				foreach (Inventory.SlotReference highlightedSubInventorySlot in Inventory.highlightedSubInventorySlots)
				{
					if (highlightedSubInventorySlot != slotRef && hoverArea.Intersects(Inventory.GetSubInventoryHoverArea(highlightedSubInventorySlot)))
					{
						return;
					}
				}
			}
			IL_7D:
			if (isEquippedSubInventory)
			{
				slotRef.Inventory.OpenState = 1f;
			}
			Inventory.highlightedSubInventorySlots.Add(slotRef);
			slotRef.Inventory.HideTimer = 1f;
			base.UpdateSubInventory(deltaTime, slotRef.SlotIndex, cam);
			foreach (Inventory.SlotReference highlightedSubInventorySlot2 in Inventory.highlightedSubInventorySlots)
			{
				if (highlightedSubInventorySlot2 != slotRef && hoverArea.Intersects(Inventory.GetSubInventoryHoverArea(highlightedSubInventorySlot2)))
				{
					hideSubInventories.Add(highlightedSubInventorySlot2);
					highlightedSubInventorySlot2.Inventory.HideTimer = 0f;
				}
			}
			HintManager.OnShowSubInventory((slotRef != null) ? slotRef.Item : null);
		}

		// Token: 0x060017E5 RID: 6117 RVA: 0x000ECED4 File Offset: 0x000EB0D4
		public void AssignQuickUseNumKeys()
		{
			int keyBindIndex = 0;
			for (int i = 0; i < this.visualSlots.Length; i++)
			{
				if (!this.HideSlot(i) && this.SlotTypes[i] == InvSlotType.Any)
				{
					this.visualSlots[i].InventoryKeyIndex = keyBindIndex;
					keyBindIndex++;
				}
			}
		}

		// Token: 0x060017E6 RID: 6118 RVA: 0x000ECF1C File Offset: 0x000EB11C
		private CharacterInventory.QuickUseAction GetQuickUseAction(Item item, bool allowEquip, bool allowInventorySwap, bool allowApplyTreatment)
		{
			if (!item.IsInteractable(Character.Controlled))
			{
				return CharacterInventory.QuickUseAction.None;
			}
			if (allowApplyTreatment && CharacterHealth.OpenHealthWindow != null && !item.AllowedSlots.Contains(InvSlotType.HealthInterface))
			{
				return CharacterInventory.QuickUseAction.UseTreatment;
			}
			if (item.ParentInventory != this)
			{
				if (Screen.Selected == GameMain.GameScreen && !item.IsInteractable(Character.Controlled))
				{
					return CharacterInventory.QuickUseAction.None;
				}
				if (item.ParentInventory == null || item.ParentInventory.Locked)
				{
					return CharacterInventory.QuickUseAction.None;
				}
				if (allowInventorySwap)
				{
					if (item.Container == null || this.character.Inventory.FindIndex(item.Container) == -1)
					{
						if (this.character.HeldItems.Any((Item i) => i.OwnInventory != null && i.OwnInventory.CanBePut(item) && this.character.CanAccessInventory(i.OwnInventory, CharacterInventory.AccessLevel.AllowBotsAndPets)))
						{
							return CharacterInventory.QuickUseAction.PutToEquippedItem;
						}
						if (!(item.ParentInventory is CharacterInventory))
						{
							return CharacterInventory.QuickUseAction.TakeFromContainer;
						}
						return CharacterInventory.QuickUseAction.TakeFromCharacter;
					}
					else
					{
						Item selectedItem = this.character.SelectedItem;
						ItemContainer selectedContainer = (selectedItem != null) ? selectedItem.GetComponent<ItemContainer>() : null;
						if (selectedContainer != null && selectedContainer.Inventory != null && !selectedContainer.Inventory.Locked)
						{
							return CharacterInventory.QuickUseAction.PutToContainer;
						}
						if (this.character.Inventory.AccessibleWhenAlive || this.character.Inventory.AccessibleByOwner)
						{
							return CharacterInventory.QuickUseAction.TakeFromContainer;
						}
					}
				}
			}
			else
			{
				Item selectedItem2 = this.character.SelectedItem;
				ItemContainer selectedContainer2 = (selectedItem2 != null) ? selectedItem2.GetComponent<ItemContainer>() : null;
				if (selectedContainer2 != null && selectedContainer2.Inventory != null && !selectedContainer2.Inventory.Locked && selectedContainer2.DrawInventory && allowInventorySwap)
				{
					return CharacterInventory.QuickUseAction.PutToContainer;
				}
				Character selectedCharacter = this.character.SelectedCharacter;
				if (((selectedCharacter != null) ? selectedCharacter.Inventory : null) != null && !this.character.SelectedCharacter.Inventory.Locked && allowInventorySwap)
				{
					return CharacterInventory.QuickUseAction.PutToCharacter;
				}
				Character selectedBy = this.character.SelectedBy;
				if (((selectedBy != null) ? selectedBy.Inventory : null) != null && Character.Controlled == this.character.SelectedBy && !this.character.SelectedBy.Inventory.Locked && (this.character.SelectedBy.Inventory.AccessibleWhenAlive || this.character.SelectedBy.Inventory.AccessibleByOwner) && allowInventorySwap)
				{
					return CharacterInventory.QuickUseAction.TakeFromCharacter;
				}
				Item equippedContainer = this.character.HeldItems.FirstOrDefault((Item i) => i.OwnInventory != null && i.OwnInventory.Container.DrawInventory && this.character.CanAccessInventory(i.OwnInventory, CharacterInventory.AccessLevel.AllowBotsAndPets) && (i.OwnInventory.CanBePut(item) || ((i.OwnInventory.Capacity == 1 || i.OwnInventory.Container.HasSubContainers) && i.OwnInventory.AllowSwappingContainedItems && i.OwnInventory.Container.CanBeContained(item))));
				if (equippedContainer != null)
				{
					if (allowEquip)
					{
						if (!this.character.HasEquippedItem(item, null, null))
						{
							ItemContainer component = equippedContainer.GetComponent<ItemContainer>();
							if (component != null && !component.QuickUseMovesItemsInside && ((item.AllowedSlots.Contains(InvSlotType.RightHand) && this.character.Inventory.GetItemInLimbSlot(InvSlotType.RightHand) == null) || (item.AllowedSlots.Contains(InvSlotType.LeftHand) && this.character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand) == null)))
							{
								return CharacterInventory.QuickUseAction.Equip;
							}
						}
						else if (item.AllowedSlots.Contains(InvSlotType.Any))
						{
							return CharacterInventory.QuickUseAction.Unequip;
						}
					}
					return CharacterInventory.QuickUseAction.PutToEquippedItem;
				}
				if (allowEquip)
				{
					if (!this.character.HasEquippedItem(item, null, null) || item.GetComponents<Pickable>().Count<Pickable>() > 1)
					{
						return CharacterInventory.QuickUseAction.Equip;
					}
					if (item.AllowedSlots.Contains(InvSlotType.Any))
					{
						return CharacterInventory.QuickUseAction.Unequip;
					}
					return CharacterInventory.QuickUseAction.Drop;
				}
			}
			return CharacterInventory.QuickUseAction.None;
		}

		// Token: 0x060017E7 RID: 6119 RVA: 0x000ED28C File Offset: 0x000EB48C
		private void QuickUseItem(Item item, bool allowEquip, bool allowInventorySwap, bool allowApplyTreatment, CharacterInventory.QuickUseAction? action = null, bool playSound = true)
		{
			CharacterInventory.<>c__DisplayClass29_0 CS$<>8__locals1 = new CharacterInventory.<>c__DisplayClass29_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.item = item;
			SubEditorScreen editor = Screen.Selected as SubEditorScreen;
			if (editor != null && !editor.WiringMode && !Submarine.Unloading)
			{
				Inventory parentInventory = CS$<>8__locals1.item.ParentInventory;
				if (((parentInventory != null) ? parentInventory.visualSlots : null) != null)
				{
					VisualSlot[] invSlots = CS$<>8__locals1.item.ParentInventory.visualSlots;
					int i = 0;
					while (i < invSlots.Length && i >= 0 && invSlots.Length > i && i >= 0 && CS$<>8__locals1.item.ParentInventory.Capacity > i)
					{
						VisualSlot slot = invSlots[i];
						if (CS$<>8__locals1.item.ParentInventory.GetItemAt(i) == CS$<>8__locals1.item)
						{
							slot.ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.4f, 0.5f);
							SoundPlayer.PlayUISound(GUISoundType.PickItem);
							break;
						}
						i++;
					}
				}
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
				{
					CS$<>8__locals1.item
				}, true, true));
				CS$<>8__locals1.item.Remove();
				return;
			}
			CharacterInventory.QuickUseAction quickUseAction = action ?? this.GetQuickUseAction(CS$<>8__locals1.item, allowEquip, allowInventorySwap, allowApplyTreatment);
			CS$<>8__locals1.success = false;
			switch (quickUseAction)
			{
			case CharacterInventory.QuickUseAction.Equip:
				if (string.IsNullOrEmpty(CS$<>8__locals1.item.Prefab.EquipConfirmationText) || this.character != Character.Controlled)
				{
					CS$<>8__locals1.<QuickUseItem>g__Equip|0();
				}
				else
				{
					if (GUIMessageBox.MessageBoxes.Any((GUIComponent mb) => mb.UserData as string == "equipconfirmation"))
					{
						return;
					}
					GUIMessageBox equipConfirmation = new GUIMessageBox(string.Empty, TextManager.Get(CS$<>8__locals1.item.Prefab.EquipConfirmationText), new LocalizedString[]
					{
						TextManager.Get("yes"),
						TextManager.Get("no")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
					{
						UserData = "equipconfirmation"
					};
					equipConfirmation.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
					{
						CS$<>8__locals1.<QuickUseItem>g__Equip|0();
						equipConfirmation.Close();
						return true;
					};
					equipConfirmation.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(equipConfirmation.Close);
				}
				break;
			case CharacterInventory.QuickUseAction.Unequip:
				if (CS$<>8__locals1.item.AllowedSlots.Contains(InvSlotType.Any))
				{
					CS$<>8__locals1.success = this.TryPutItem(CS$<>8__locals1.item, Character.Controlled, new List<InvSlotType>
					{
						InvSlotType.Any
					}, true, false, true);
				}
				break;
			case CharacterInventory.QuickUseAction.Drop:
				return;
			case CharacterInventory.QuickUseAction.TakeFromContainer:
				for (int j = 0; j < this.capacity; j++)
				{
					ItemInventory activeSubInventory = this.GetActiveEquippedSubInventory(j);
					if (activeSubInventory != null)
					{
						CS$<>8__locals1.success = activeSubInventory.TryPutItem(CS$<>8__locals1.item, Character.Controlled, CS$<>8__locals1.item.AllowedSlots, true, false, true);
						break;
					}
				}
				if (!CS$<>8__locals1.success)
				{
					CS$<>8__locals1.success = this.TryPutItemWithAutoEquipCheck(CS$<>8__locals1.item, Character.Controlled, CS$<>8__locals1.item.AllowedSlots, true);
				}
				break;
			case CharacterInventory.QuickUseAction.TakeFromCharacter:
				if (this.character.SelectedBy != null && Character.Controlled == this.character.SelectedBy && this.character.SelectedBy.Inventory != null)
				{
					CS$<>8__locals1.success = this.character.SelectedBy.Inventory.TryPutItemWithAutoEquipCheck(CS$<>8__locals1.item, Character.Controlled, CS$<>8__locals1.item.AllowedSlots, true);
				}
				break;
			case CharacterInventory.QuickUseAction.PutToContainer:
			{
				Item selectedItem = this.character.SelectedItem;
				ItemContainer selectedContainer = (selectedItem != null) ? selectedItem.GetComponent<ItemContainer>() : null;
				if (selectedContainer != null && selectedContainer.Inventory != null)
				{
					CS$<>8__locals1.success = selectedContainer.Inventory.TryPutItem(CS$<>8__locals1.item, Character.Controlled, CS$<>8__locals1.item.AllowedSlots, true, false, true);
				}
				break;
			}
			case CharacterInventory.QuickUseAction.PutToCharacter:
				if (this.character.SelectedCharacter != null && this.character.SelectedCharacter.Inventory != null)
				{
					CS$<>8__locals1.success = this.character.SelectedCharacter.Inventory.TryPutItem(CS$<>8__locals1.item, Character.Controlled, CS$<>8__locals1.item.AllowedSlots, true, false, true);
				}
				break;
			case CharacterInventory.QuickUseAction.PutToEquippedItem:
			{
				IEnumerable<Item> heldItems = this.character.HeldItems;
				Func<Item, float> keySelector;
				if ((keySelector = CS$<>8__locals1.<>9__6) == null)
				{
					keySelector = (CS$<>8__locals1.<>9__6 = ((Item heldItem) => CharacterInventory.<QuickUseItem>g__GetContainPriority|29_1(CS$<>8__locals1.item, heldItem)));
				}
				foreach (Item heldItem2 in heldItems.OrderByDescending(keySelector))
				{
					if (heldItem2.OwnInventory != null && heldItem2.OwnInventory.Container.DrawInventory)
					{
						if (heldItem2.OwnInventory.Capacity != 1 && !heldItem2.OwnInventory.Container.HasSubContainers)
						{
							goto IL_57D;
						}
						Item itemAt = heldItem2.OwnInventory.GetItemAt(0);
						if (((itemAt != null) ? itemAt.Prefab : null) != CS$<>8__locals1.item.Prefab)
						{
							goto IL_57D;
						}
						bool flag = heldItem2.OwnInventory.GetItemsAt(0).Count<Item>() > 1;
						IL_57E:
						bool disallowSwapping = flag;
						if (heldItem2.OwnInventory.TryPutItem(CS$<>8__locals1.item, Character.Controlled, null, true, false, true) || ((heldItem2.OwnInventory.Capacity == 1 || heldItem2.OwnInventory.Container.HasSubContainers) && heldItem2.OwnInventory.TryPutItem(CS$<>8__locals1.item, 0, !disallowSwapping, false, Character.Controlled, true, false, true)))
						{
							CS$<>8__locals1.success = true;
							for (int k = 0; k < this.capacity; k++)
							{
								if (this.slots[k].Contains(heldItem2))
								{
									this.visualSlots[k].ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.4f, 0.5f);
								}
							}
							break;
						}
						continue;
						IL_57D:
						flag = false;
						goto IL_57E;
					}
				}
				break;
			}
			case CharacterInventory.QuickUseAction.UseTreatment:
			{
				CharacterHealth openHealthWindow = CharacterHealth.OpenHealthWindow;
				if (openHealthWindow == null)
				{
					return;
				}
				openHealthWindow.OnItemDropped(CS$<>8__locals1.item, true);
				return;
			}
			}
			if (CS$<>8__locals1.success)
			{
				for (int l = 0; l < this.capacity; l++)
				{
					if (this.slots[l].Contains(CS$<>8__locals1.item))
					{
						this.visualSlots[l].ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.4f, 0.5f);
					}
				}
			}
			Inventory.DraggingItems.Clear();
			if (playSound)
			{
				SoundPlayer.PlayUISound(CS$<>8__locals1.success ? GUISoundType.PickItem : GUISoundType.PickItemFail);
			}
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x000ED984 File Offset: 0x000EBB84
		public bool CanBeAutoMovedToCorrectSlots(Item item)
		{
			if (item == null)
			{
				return false;
			}
			foreach (InvSlotType allowedSlot in item.AllowedSlots)
			{
				InvSlotType slotsFree = InvSlotType.None;
				for (int i = 0; i < this.slots.Length; i++)
				{
					if (allowedSlot.HasFlag(this.SlotTypes[i]) && this.slots[i].Empty())
					{
						slotsFree |= this.SlotTypes[i];
					}
				}
				if (allowedSlot == slotsFree)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x000EDA28 File Offset: 0x000EBC28
		public void FlashAllowedSlots(Item item, Color color)
		{
			if (item == null || this.visualSlots == null)
			{
				return;
			}
			bool flashed = false;
			foreach (InvSlotType allowedSlot in item.AllowedSlots)
			{
				for (int i = 0; i < this.slots.Length; i++)
				{
					if (allowedSlot.HasFlag(this.SlotTypes[i]))
					{
						this.visualSlots[i].ShowBorderHighlight(color, 0.1f, 0.9f, 0.5f);
						flashed = true;
					}
				}
			}
			if (flashed)
			{
				SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
			}
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x000EDAD4 File Offset: 0x000EBCD4
		public void DrawOwn(SpriteBatch spriteBatch)
		{
			if (!this.AccessibleWhenAlive && !this.character.IsDead && !this.AccessibleByOwner)
			{
				return;
			}
			if (this.capacity == 0)
			{
				return;
			}
			if (this.visualSlots == null)
			{
				this.CreateSlots();
			}
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y || this.prevUIScale != Inventory.UIScale || this.prevHUDScale != GUI.Scale)
			{
				this.CreateSlots();
				this.SetSlotPositions(this.layout);
				this.prevUIScale = Inventory.UIScale;
				this.prevHUDScale = GUI.Scale;
			}
			if (this.layout == CharacterInventory.Layout.Center)
			{
				this.CalculateBackgroundFrame();
				GUI.DrawRectangle(spriteBatch, base.BackgroundFrame, Color.Black * 0.8f, true, 0f, 1f);
				GUI.DrawString(spriteBatch, new Vector2((float)((int)((float)base.BackgroundFrame.Center.X - GUIStyle.Font.MeasureString(this.character.Name, false).X / 2f)), (float)(base.BackgroundFrame.Y + 5)), this.character.Name, Color.White * 0.9f, null, 0, null, ForceUpperCase.Inherit);
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (!this.HideSlot(j) && this.SlotTypes[j] != InvSlotType.HealthInterface)
				{
					if (!Inventory.DraggingItems.Any<Item>())
					{
						goto IL_1D2;
					}
					if (!this.slots[j].Items.All((Item it) => Inventory.DraggingItems.Contains(it)))
					{
						goto IL_1D2;
					}
					bool flag = this.visualSlots[j].MouseOn();
					IL_1D3:
					bool drawItem = flag;
					Inventory.DrawSlot(spriteBatch, this, this.visualSlots[j], this.slots[j].FirstOrDefault(), j, drawItem, this.SlotTypes[j]);
					goto IL_200;
					IL_1D2:
					flag = true;
					goto IL_1D3;
				}
				IL_200:;
			}
			VisualSlot highlightedQuickUseSlot = null;
			Rectangle inventoryArea = Rectangle.Empty;
			int i;
			Func<Item, bool> <>9__1;
			Func<Item, bool> <>9__3;
			int i2;
			for (i = 0; i < this.capacity; i = i2 + 1)
			{
				if (!this.HideSlot(i))
				{
					inventoryArea = ((inventoryArea == Rectangle.Empty) ? this.visualSlots[i].InteractRect : Rectangle.Union(inventoryArea, this.visualSlots[i].InteractRect));
					if (!this.slots[i].Empty())
					{
						IEnumerable<Item> draggingItems = Inventory.DraggingItems;
						Func<Item, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Item it) => this.slots[i].Contains(it)));
						}
						if (!draggingItems.Any(predicate) || this.visualSlots[i].InteractRect.Contains(PlayerInput.MousePosition))
						{
							if (this.slots[i].First().AllowedSlots.Any((InvSlotType a) => a != InvSlotType.Any))
							{
								IEnumerable<Item> draggingItems2 = Inventory.DraggingItems;
								Func<Item, bool> predicate2;
								if ((predicate2 = <>9__3) == null)
								{
									predicate2 = (<>9__3 = ((Item it) => this.slots[i].Contains(it)));
								}
								if (draggingItems2.Any(predicate2) && !this.visualSlots[i].IsHighlighted)
								{
									goto IL_7BF;
								}
								if (this.IsInLimbSlot(this.slots[i].First(), InvSlotType.LeftHand))
								{
									Sprite icon = CharacterInventory.LimbSlotIcons[InvSlotType.LeftHand];
									icon.Draw(spriteBatch, new Vector2((float)this.visualSlots[i].Rect.X, (float)this.visualSlots[i].Rect.Bottom) + this.visualSlots[i].DrawOffset, Color.White * 0.6f, new Vector2(icon.size.X * 0.35f, icon.size.Y * 0.75f), 0f, (float)this.visualSlots[i].Rect.Width / icon.size.X * 0.7f, SpriteEffects.None, null);
								}
								if (this.IsInLimbSlot(this.slots[i].First(), InvSlotType.RightHand))
								{
									Sprite icon2 = CharacterInventory.LimbSlotIcons[InvSlotType.RightHand];
									icon2.Draw(spriteBatch, new Vector2((float)this.visualSlots[i].Rect.Right, (float)this.visualSlots[i].Rect.Bottom) + this.visualSlots[i].DrawOffset, Color.White * 0.6f, new Vector2(icon2.size.X * 0.65f, icon2.size.Y * 0.75f), 0f, (float)this.visualSlots[i].Rect.Width / icon2.size.X * 0.7f, SpriteEffects.None, null);
								}
								GUIComponent.ComponentState state = this.visualSlots[i].EquipButtonState;
								if (state == GUIComponent.ComponentState.Hover)
								{
									highlightedQuickUseSlot = this.visualSlots[i];
								}
								if (this.slots[i].First().AllowedSlots.Count<InvSlotType>() != 1 && this.SlotTypes[i] != InvSlotType.HealthInterface)
								{
									Color color = Color.White;
									if (this.Locked)
									{
										color *= 0.5f;
									}
									Vector2 indicatorScale = new Vector2((float)this.visualSlots[i].EquipButtonRect.Size.X / Inventory.EquippedIndicator.size.X, (float)this.visualSlots[i].EquipButtonRect.Size.Y / Inventory.EquippedIndicator.size.Y);
									bool isEquipped = this.character.HasEquippedItem(this.slots[i].First(), null, null);
									Sprite sprite2;
									switch (state)
									{
									case GUIComponent.ComponentState.None:
										sprite2 = (isEquipped ? Inventory.EquippedIndicator : Inventory.UnequippedIndicator);
										break;
									case GUIComponent.ComponentState.Hover:
										sprite2 = (isEquipped ? Inventory.EquippedHoverIndicator : Inventory.UnequippedHoverIndicator);
										break;
									case GUIComponent.ComponentState.Pressed:
									case GUIComponent.ComponentState.Selected:
									case GUIComponent.ComponentState.HoverSelected:
										sprite2 = (isEquipped ? Inventory.EquippedClickedIndicator : Inventory.UnequippedClickedIndicator);
										break;
									default:
										throw new NotImplementedException();
									}
									Sprite sprite = sprite2;
									sprite.Draw(spriteBatch, this.visualSlots[i].EquipButtonRect.Center.ToVector2(), color, Inventory.EquippedIndicator.Origin, 0f, indicatorScale, SpriteEffects.None, null);
									goto IL_7BF;
								}
								goto IL_7BF;
							}
						}
					}
					if (CharacterInventory.LimbSlotIcons.ContainsKey(this.SlotTypes[i]))
					{
						Sprite icon3 = CharacterInventory.LimbSlotIcons[this.SlotTypes[i]];
						icon3.Draw(spriteBatch, this.visualSlots[i].Rect.Center.ToVector2() + this.visualSlots[i].DrawOffset, GUIStyle.EquipmentSlotIconColor, icon3.size / 2f, 0f, (float)this.visualSlots[i].Rect.Width / icon3.size.X, SpriteEffects.None, null);
					}
				}
				IL_7BF:
				i2 = i;
			}
			if (this.Locked)
			{
				GUI.DrawRectangle(spriteBatch, inventoryArea, new Color(30, 30, 30, 100), true, 0f, 1f);
				GUIComponentStyle componentStyle = GUIStyle.GetComponentStyle("LockIcon");
				Sprite lockIcon = (componentStyle != null) ? componentStyle.GetDefaultSprite() : null;
				if (lockIcon != null)
				{
					lockIcon.Draw(spriteBatch, inventoryArea.Center.ToVector2(), 0f, Math.Min((float)inventoryArea.Height / lockIcon.size.Y * 0.7f, 1f), SpriteEffects.None);
				}
				if (inventoryArea.Contains(PlayerInput.MousePosition) && this.character.LockHands)
				{
					GUIComponent.DrawToolTip(spriteBatch, TextManager.Get("handcuffed"), new Rectangle(inventoryArea.Center - new Point(inventoryArea.Height / 2), new Point(inventoryArea.Height)), Anchor.BottomCenter, Pivot.TopLeft);
					return;
				}
			}
			else if (highlightedQuickUseSlot != null && !highlightedQuickUseSlot.QuickUseButtonToolTip.IsNullOrEmpty())
			{
				GUIComponent.DrawToolTip(spriteBatch, highlightedQuickUseSlot.QuickUseButtonToolTip, highlightedQuickUseSlot.EquipButtonRect, Anchor.BottomCenter, Pivot.TopLeft);
			}
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x000EE3D0 File Offset: 0x000EC5D0
		public void ClientEventWrite(IWriteMessage msg, Character.InventoryStateEventData extraData)
		{
			base.SharedWrite(msg, extraData.SlotRange);
			this.syncItemsDelay = 1f;
		}

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x060017EC RID: 6124 RVA: 0x000EE3EA File Offset: 0x000EC5EA
		public InvSlotType[] SlotTypes { get; }

		// Token: 0x060017ED RID: 6125 RVA: 0x000EE3F2 File Offset: 0x000EC5F2
		public static bool IsHandSlotType(InvSlotType s)
		{
			return s.HasFlag(InvSlotType.LeftHand) || s.HasFlag(InvSlotType.RightHand);
		}

		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x060017EE RID: 6126 RVA: 0x000EE41A File Offset: 0x000EC61A
		// (set) Token: 0x060017EF RID: 6127 RVA: 0x000EE422 File Offset: 0x000EC622
		public bool AccessibleWhenAlive { get; private set; }

		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x060017F0 RID: 6128 RVA: 0x000EE42B File Offset: 0x000EC62B
		// (set) Token: 0x060017F1 RID: 6129 RVA: 0x000EE433 File Offset: 0x000EC633
		public bool AccessibleByOwner { get; private set; }

		// Token: 0x060017F2 RID: 6130 RVA: 0x000EE43C File Offset: 0x000EC63C
		private static string[] ParseSlotTypes(ContentXElement element)
		{
			string slotString = element.GetAttributeString("slots", null);
			if (slotString != null)
			{
				return slotString.Split(',', StringSplitOptions.None);
			}
			return Array.Empty<string>();
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x000EE468 File Offset: 0x000EC668
		public CharacterInventory(ContentXElement element, Character character, bool spawnInitialItems) : base(character, CharacterInventory.ParseSlotTypes(element).Length, 5)
		{
			CharacterInventory <>4__this = this;
			this.character = character;
			this.IsEquipped = new bool[this.capacity];
			this.SlotTypes = new InvSlotType[this.capacity];
			this.AccessibleWhenAlive = element.GetAttributeBool("accessiblewhenalive", character.Info != null);
			this.AccessibleByOwner = element.GetAttributeBool("accessiblebyowner", this.AccessibleWhenAlive);
			string[] slotTypeNames = CharacterInventory.ParseSlotTypes(element);
			for (int i = 0; i < this.capacity; i++)
			{
				InvSlotType parsedSlotType = InvSlotType.Any;
				slotTypeNames[i] = slotTypeNames[i].Trim();
				if (!Enum.TryParse<InvSlotType>(slotTypeNames[i], out parsedSlotType))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in the inventory config of \"",
						character.SpeciesName.ToString(),
						"\" - ",
						slotTypeNames[i],
						" is not a valid inventory slot type."
					}), null, element.ContentPackage, false, false);
				}
				this.SlotTypes[i] = parsedSlotType;
				InvSlotType invSlotType = this.SlotTypes[i];
				if (invSlotType == InvSlotType.RightHand || invSlotType == InvSlotType.LeftHand)
				{
					this.slots[i].HideIfEmpty = true;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				InvSlotType slotType = this.SlotTypes[j];
				List<Inventory.ItemSlot> slotList;
				if (!this.slotsByType.TryGetValue(slotType, out slotList))
				{
					slotList = new List<Inventory.ItemSlot>();
					this.slotsByType[this.SlotTypes[j]] = slotList;
				}
				slotList.Add(this.slots[j]);
			}
			this.InitProjSpecific(element);
			IEnumerable<ContentXElement> itemElements = from e in element.Elements()
			where e.Name.ToString().Equals("item", StringComparison.OrdinalIgnoreCase)
			select e;
			int itemCount = itemElements.Count<ContentXElement>();
			if (itemCount > this.capacity)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(87, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral("\" is configured to spawn with more items than it has inventory capacity for.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (!spawnInitialItems)
			{
				return;
			}
			if (GameMain.Client != null)
			{
				return;
			}
			Action<Item> <>9__1;
			foreach (ContentXElement subElement in itemElements)
			{
				string itemIdentifier = subElement.GetAttributeString("identifier", "");
				ItemPrefab itemPrefab;
				if (!ItemPrefab.Prefabs.TryGet(itemIdentifier, out itemPrefab))
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in character inventory \"",
						character.SpeciesName.ToString(),
						"\" - item \"",
						itemIdentifier,
						"\" not found."
					}), null, element.ContentPackage, false, false);
				}
				else
				{
					string slotString = subElement.GetAttributeString("slot", "None");
					InvSlotType s;
					InvSlotType slot = Enum.TryParse<InvSlotType>(slotString, true, out s) ? s : InvSlotType.None;
					bool forceToSlot = subElement.GetAttributeBool("forcetoslot", false);
					int amount = subElement.GetAttributeInt("amount", 1);
					for (int k = 0; k < amount; k++)
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner != null)
						{
							ItemPrefab itemPrefab2 = itemPrefab;
							bool ignoreLimbSlots = forceToSlot;
							InvSlotType slot2 = slot;
							float? condition = null;
							int? quality = null;
							Action<Item> onSpawned;
							if ((onSpawned = <>9__1) == null)
							{
								onSpawned = (<>9__1 = delegate(Item item)
								{
									if (item != null && item.ParentInventory != <>4__this)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(59, 2);
										defaultInterpolatedStringHandler2.AppendLiteral("Failed to spawn the initial item \"");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(item.Prefab.Identifier);
										defaultInterpolatedStringHandler2.AppendLiteral("\" in the inventory of \"");
										defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(character.SpeciesName);
										defaultInterpolatedStringHandler2.AppendLiteral("\".");
										string errorMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
										DebugConsole.ThrowError(errorMsg, null, element.ContentPackage, false, false);
										GameAnalyticsManager.AddErrorEventOnce("CharacterInventory:FailedToSpawnInitialItem", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
										return;
									}
									if (!character.Enabled)
									{
										foreach (Item heldItem in character.HeldItems)
										{
											if (item.body != null)
											{
												item.body.Enabled = false;
											}
										}
									}
								});
							}
							spawner.AddItemToSpawnQueue(itemPrefab2, this, condition, quality, onSpawned, true, ignoreLimbSlots, slot2);
						}
					}
				}
			}
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x000EE868 File Offset: 0x000ECA68
		private void InitProjSpecific(XElement element)
		{
			this.SlotPositions = new Vector2[this.SlotTypes.Length];
			this.CurrentLayout = CharacterInventory.Layout.Default;
			this.SetSlotPositions(this.layout);
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x000EE890 File Offset: 0x000ECA90
		public Item FindEquippedItemByTag(Identifier tag)
		{
			if (tag.IsEmpty)
			{
				return null;
			}
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.SlotTypes[i] != InvSlotType.Any)
				{
					Item item = this.slots[i].FirstOrDefault();
					if (item != null && item.HasTag(tag))
					{
						return item;
					}
				}
			}
			return null;
		}

		// Token: 0x060017F6 RID: 6134 RVA: 0x000EE8E4 File Offset: 0x000ECAE4
		public int FindLimbSlot(InvSlotType limbSlot)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.SlotTypes[i] == limbSlot)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060017F7 RID: 6135 RVA: 0x000EE914 File Offset: 0x000ECB14
		public Item GetItemInLimbSlot(InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				return slotList.First<Inventory.ItemSlot>().FirstOrDefault();
			}
			return null;
		}

		// Token: 0x060017F8 RID: 6136 RVA: 0x000EE93E File Offset: 0x000ECB3E
		public IEnumerable<Item> GetItemsInLimbSlot(InvSlotType limbSlot)
		{
			CharacterInventory.<GetItemsInLimbSlot>d__58 <GetItemsInLimbSlot>d__ = new CharacterInventory.<GetItemsInLimbSlot>d__58(-2);
			<GetItemsInLimbSlot>d__.<>4__this = this;
			<GetItemsInLimbSlot>d__.<>3__limbSlot = limbSlot;
			return <GetItemsInLimbSlot>d__;
		}

		// Token: 0x060017F9 RID: 6137 RVA: 0x000EE958 File Offset: 0x000ECB58
		public bool IsInLimbSlot(Item item, InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (limbSlot == (InvSlotType.RightHand | InvSlotType.LeftHand))
			{
				if (this.GetItemsInLimbSlot(InvSlotType.RightHand).Contains(item) && this.GetItemsInLimbSlot(InvSlotType.LeftHand).Contains(item))
				{
					return true;
				}
			}
			else if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				foreach (Inventory.ItemSlot slot in slotList)
				{
					if (slot.Contains(item))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x000EE9E4 File Offset: 0x000ECBE4
		public bool IsSlotEmpty(InvSlotType limbSlot)
		{
			List<Inventory.ItemSlot> slotList;
			if (this.slotsByType.TryGetValue(limbSlot, out slotList))
			{
				foreach (Inventory.ItemSlot slot in slotList)
				{
					if (slot.Empty())
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x000EEA4C File Offset: 0x000ECC4C
		public bool CanBePut(Item item, InvSlotType slotType)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (slotType.HasFlag(this.SlotTypes[i]) && this.CanBePutInSlot(item, i, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060017FC RID: 6140 RVA: 0x000EEA94 File Offset: 0x000ECC94
		public override bool CanBePutInSlot(Item item, int i, bool ignoreCondition = false)
		{
			return base.CanBePutInSlot(item, i, ignoreCondition) && item.AllowedSlots.Any((InvSlotType s) => s.HasFlag(this.SlotTypes[i])) && (this.SlotTypes[i] == InvSlotType.Any || this.slots[i].Items.Count < 1);
		}

		// Token: 0x060017FD RID: 6141 RVA: 0x000EEB0C File Offset: 0x000ECD0C
		public override bool CanBePutInSlot(ItemPrefab itemPrefab, int i, float? condition, int? quality = null)
		{
			return base.CanBePutInSlot(itemPrefab, i, condition, quality) && (this.SlotTypes[i] == InvSlotType.Any || this.slots[i].Items.Count < 1);
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x000EEB3F File Offset: 0x000ECD3F
		public override void RemoveItem(Item item)
		{
			this.RemoveItem(item, false);
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x000EEB4C File Offset: 0x000ECD4C
		public void RemoveItem(Item item, bool tryEquipFromSameStack)
		{
			if (!base.Contains(item))
			{
				return;
			}
			bool wasEquipped = this.character.HasEquippedItem(item, null, null);
			List<int> indices = base.FindIndices(item);
			base.RemoveItem(item);
			this.CreateSlots();
			if (this.character == Character.Controlled)
			{
				Item selectedItem = this.character.SelectedItem;
				if (selectedItem != null)
				{
					CircuitBox component = selectedItem.GetComponent<CircuitBox>();
					if (component != null)
					{
						component.OnViewUpdateProjSpecific();
					}
				}
			}
			CharacterHUD.RecreateHudTextsIfControlling(this.character);
			if (tryEquipFromSameStack && wasEquipped)
			{
				int limbSlot = indices.Find((int j) => this.SlotTypes[j] != InvSlotType.Any);
				foreach (int i in indices)
				{
					Item itemInSameSlot = base.GetItemAt(i);
					if (itemInSameSlot != null)
					{
						if (this.TryPutItem(itemInSameSlot, limbSlot, false, false, this.character, true, false, true))
						{
							this.visualSlots[i].ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.412f, 0.5f);
							break;
						}
						break;
					}
				}
			}
		}

		// Token: 0x06001800 RID: 6144 RVA: 0x000EEC6C File Offset: 0x000ECE6C
		public bool TryPutItemWithAutoEquipCheck(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true)
		{
			if (item.AllowedSlots.Contains(InvSlotType.Any))
			{
				Wearable wearable = item.GetComponent<Wearable>();
				if (wearable != null && !wearable.AutoEquipWhenFull && !this.IsAnySlotAvailable(item))
				{
					return false;
				}
			}
			if (allowedSlots != null && allowedSlots.Any<InvSlotType>() && !allowedSlots.Contains(InvSlotType.Any))
			{
				bool allSlotsTaken = true;
				foreach (InvSlotType allowedSlot in allowedSlots)
				{
					if (allowedSlot == (InvSlotType.RightHand | InvSlotType.LeftHand))
					{
						int rightHandSlot = this.FindLimbSlot(InvSlotType.RightHand);
						int leftHandSlot = this.FindLimbSlot(InvSlotType.LeftHand);
						if (rightHandSlot > -1 && this.slots[rightHandSlot].CanBePut(item, false) && leftHandSlot > -1 && this.slots[leftHandSlot].CanBePut(item, false))
						{
							allSlotsTaken = false;
							break;
						}
					}
					else
					{
						int slot = this.FindLimbSlot(allowedSlot);
						if (slot > -1 && this.slots[slot].CanBePut(item, false))
						{
							allSlotsTaken = false;
							break;
						}
					}
				}
				if (allSlotsTaken)
				{
					int slot2 = this.FindLimbSlot(allowedSlots.First<InvSlotType>());
					if (slot2 > -1 && this.slots[slot2].Items.Any((Item it) => it != item) && this.slots[slot2].First().AllowDroppingOnSwapWith(item))
					{
						foreach (Item existingItem in this.slots[slot2].Items.ToList<Item>())
						{
							if (existingItem.IsInteractable(this.character))
							{
								existingItem.Drop(user, true, true);
								Inventory parentInventory = existingItem.ParentInventory;
								if (parentInventory != null)
								{
									parentInventory.RemoveItem(existingItem);
								}
							}
						}
					}
				}
			}
			return this.TryPutItem(item, user, allowedSlots, createNetworkEvent, false, true);
		}

		// Token: 0x06001801 RID: 6145 RVA: 0x000EEE7C File Offset: 0x000ED07C
		public override bool TryPutItem(Item item, Character user, IEnumerable<InvSlotType> allowedSlots = null, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			CharacterInventory.<>c__DisplayClass67_0 CS$<>8__locals1 = new CharacterInventory.<>c__DisplayClass67_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.item = item;
			if (allowedSlots == null || !allowedSlots.Any<InvSlotType>())
			{
				return false;
			}
			if (CS$<>8__locals1.item == null)
			{
				return false;
			}
			if (CS$<>8__locals1.item.Removed)
			{
				DebugConsole.ThrowError("Tried to put a removed item (" + CS$<>8__locals1.item.Name + ") in an inventory.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return false;
			}
			if (CS$<>8__locals1.item.GetComponent<Pickable>() == null || CS$<>8__locals1.item.AllowedSlots.None(null))
			{
				return false;
			}
			int currentSlot = -1;
			bool inWrongSlot = false;
			bool inSuitableSlot = false;
			int slotIndex;
			int num;
			for (slotIndex = 0; slotIndex < this.capacity; slotIndex = num + 1)
			{
				if (this.slots[slotIndex].Contains(CS$<>8__locals1.item))
				{
					currentSlot = slotIndex;
					InvSlotType firstMatchingSlotType = allowedSlots.FirstOrDefault((InvSlotType slot) => slot.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[slotIndex]));
					if (firstMatchingSlotType == InvSlotType.None)
					{
						inWrongSlot = true;
						break;
					}
					inSuitableSlot = true;
					IEnumerable<InvSlotType> individualFlags = EnumExtensions.GetIndividualFlags<InvSlotType>(firstMatchingSlotType);
					foreach (InvSlotType flag in individualFlags)
					{
						if (flag != InvSlotType.None && !this.IsInLimbSlot(CS$<>8__locals1.item, flag))
						{
							inSuitableSlot = false;
							break;
						}
					}
				}
				num = slotIndex;
			}
			if (inSuitableSlot && !inWrongSlot)
			{
				return true;
			}
			if (allowedSlots.Contains(InvSlotType.Any) && CS$<>8__locals1.item.AllowedSlots.Contains(InvSlotType.Any))
			{
				int freeIndex = this.GetFreeAnySlot(CS$<>8__locals1.item, inWrongSlot);
				if (freeIndex > -1)
				{
					this.PutItem(CS$<>8__locals1.item, freeIndex, user, true, createNetworkEvent, true);
					CS$<>8__locals1.item.Unequip(this.character);
					return true;
				}
			}
			int placedInSlot = -1;
			Func<InvSlotType, int> keySelector;
			if ((keySelector = CS$<>8__locals1.<>9__1) == null)
			{
				keySelector = (CS$<>8__locals1.<>9__1 = ((InvSlotType slotType) => (!CS$<>8__locals1.<>4__this.IsSlotEmpty(slotType)) ? 1 : 0));
			}
			foreach (InvSlotType allowedSlot in allowedSlots.OrderBy(keySelector))
			{
				if ((!allowedSlot.HasFlag(InvSlotType.RightHand) || this.character.AnimController.GetLimb(LimbType.RightHand, true, false, false) != null) && (!allowedSlot.HasFlag(InvSlotType.LeftHand) || this.character.AnimController.GetLimb(LimbType.LeftHand, true, false, false) != null))
				{
					bool free = true;
					int i;
					for (i = 0; i < this.capacity; i = num + 1)
					{
						if (allowedSlot.HasFlag(this.SlotTypes[i]) && CS$<>8__locals1.item.AllowedSlots.Any((InvSlotType s) => s.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[i])))
						{
							IEnumerable<Item> items = this.slots[i].Items;
							Func<Item, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__3) == null)
							{
								predicate = (CS$<>8__locals1.<>9__3 = ((Item it) => it != CS$<>8__locals1.item));
							}
							if (items.Any(predicate) && (!this.slots[i].First().AllowedSlots.Contains(InvSlotType.Any) || !this.TryPutItem(this.slots[i].FirstOrDefault(), this.character, new List<InvSlotType>
							{
								InvSlotType.Any
							}, true, ignoreCondition, true)))
							{
								free = false;
								for (int j = 0; j < this.capacity; j++)
								{
									if (this.visualSlots != null && this.slots[j] == this.slots[i])
									{
										this.visualSlots[j].ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.9f, 0.5f);
									}
								}
							}
						}
						num = i;
					}
					if (free)
					{
						int i;
						Func<InvSlotType, bool> <>9__5;
						for (i = 0; i < this.capacity; i = num + 1)
						{
							if (allowedSlot.HasFlag(this.SlotTypes[i]) && CS$<>8__locals1.item.GetComponents<Pickable>().Any(delegate(Pickable p)
							{
								IEnumerable<InvSlotType> allowedSlots2 = p.AllowedSlots;
								Func<InvSlotType, bool> predicate2;
								if ((predicate2 = <>9__5) == null)
								{
									predicate2 = (<>9__5 = ((InvSlotType s) => s.HasFlag(CS$<>8__locals1.<>4__this.SlotTypes[i])));
								}
								return allowedSlots2.Any(predicate2);
							}) && this.slots[i].Empty())
							{
								bool removeFromOtherSlots = CS$<>8__locals1.item.ParentInventory != this;
								if (placedInSlot == -1 && inWrongSlot && (!this.slots[i].HideIfEmpty || this.SlotTypes[currentSlot] != InvSlotType.Any))
								{
									removeFromOtherSlots = true;
								}
								this.PutItem(CS$<>8__locals1.item, i, user, removeFromOtherSlots, createNetworkEvent, true);
								CS$<>8__locals1.item.Equip(this.character);
								placedInSlot = i;
							}
							num = i;
						}
						if (placedInSlot > -1)
						{
							break;
						}
					}
				}
			}
			return placedInSlot > -1;
		}

		// Token: 0x06001802 RID: 6146 RVA: 0x000EF408 File Offset: 0x000ED608
		public bool IsAnySlotAvailable(Item item)
		{
			return this.GetFreeAnySlot(item, false) > -1;
		}

		// Token: 0x06001803 RID: 6147 RVA: 0x000EF418 File Offset: 0x000ED618
		private int GetFreeAnySlot(Item item, bool inWrongSlot)
		{
			for (int i = 0; i < this.capacity; i++)
			{
				if (this.SlotTypes[i] == InvSlotType.Any && !this.slots[i].Empty() && this.CanBePutInSlot(item, i, false))
				{
					return i;
				}
			}
			for (int j = 0; j < this.capacity; j++)
			{
				if (this.SlotTypes[j] == InvSlotType.Any && this.slots[j].Contains(item))
				{
					return j;
				}
			}
			for (int k = 0; k < this.capacity; k++)
			{
				if (this.SlotTypes[k] == InvSlotType.Any && this.CanBePutInSlot(item, k, false))
				{
					return k;
				}
			}
			Func<Item, bool> <>9__0;
			for (int l = 0; l < this.capacity; l++)
			{
				if (this.SlotTypes[l] == InvSlotType.Any)
				{
					if (inWrongSlot)
					{
						if (this.slots[l].Any())
						{
							IEnumerable<Item> items = this.slots[l].Items;
							Func<Item, bool> predicate;
							if ((predicate = <>9__0) == null)
							{
								predicate = (<>9__0 = ((Item it) => it != item));
							}
							if (items.Any(predicate))
							{
								goto IL_11B;
							}
						}
					}
					else if (!this.CanBePutInSlot(item, l, false))
					{
						goto IL_11B;
					}
					return l;
				}
				IL_11B:;
			}
			return -1;
		}

		// Token: 0x06001804 RID: 6148 RVA: 0x000EF554 File Offset: 0x000ED754
		public override bool TryPutItem(Item item, int index, bool allowSwapping, bool allowCombine, Character user, bool createNetworkEvent = true, bool ignoreCondition = false, bool triggerOnInsertedEffects = true)
		{
			if (index < 0 || index >= this.slots.Length)
			{
				string errorMsg = "CharacterInventory.TryPutItem failed: index was out of range(" + index.ToString() + ").\n" + Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce("CharacterInventory.TryPutItem:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return false;
			}
			if (this.slots[index].Any())
			{
				return !this.slots[index].Contains(item) && base.TryPutItem(item, index, allowSwapping, allowCombine, user, createNetworkEvent, ignoreCondition, true);
			}
			if (this.SlotTypes[index] != InvSlotType.Any)
			{
				InvSlotType placeToSlots = InvSlotType.None;
				bool slotsFree = true;
				foreach (Pickable pickable in item.GetComponents<Pickable>())
				{
					foreach (InvSlotType allowedSlot in pickable.AllowedSlots)
					{
						if (allowedSlot.HasFlag(this.SlotTypes[index]))
						{
							for (int i = 0; i < this.capacity; i++)
							{
								if (allowedSlot.HasFlag(this.SlotTypes[i]) && this.slots[i].Any() && !this.slots[i].Contains(item))
								{
									slotsFree = false;
									break;
								}
								placeToSlots = allowedSlot;
							}
						}
					}
				}
				return slotsFree && this.TryPutItem(item, user, new List<InvSlotType>
				{
					placeToSlots
				}, createNetworkEvent, ignoreCondition, true);
			}
			if (!item.GetComponents<Pickable>().Any((Pickable p) => p.AllowedSlots.Contains(InvSlotType.Any)))
			{
				return false;
			}
			if (this.slots[index].Any())
			{
				return this.slots[index].Contains(item);
			}
			this.PutItem(item, index, user, true, createNetworkEvent, true);
			return true;
		}

		// Token: 0x06001805 RID: 6149 RVA: 0x000EF754 File Offset: 0x000ED954
		protected override void PutItem(Item item, int i, Character user, bool removeItem = true, bool createNetworkEvent = true, bool triggerOnInsertedEffects = true)
		{
			base.PutItem(item, i, user, removeItem, createNetworkEvent, triggerOnInsertedEffects);
			this.CreateSlots();
			if (this.character == Character.Controlled)
			{
				HintManager.OnObtainedItem(this.character, item);
				Item selectedItem = this.character.SelectedItem;
				if (selectedItem != null)
				{
					CircuitBox component = selectedItem.GetComponent<CircuitBox>();
					if (component != null)
					{
						component.OnViewUpdateProjSpecific();
					}
				}
			}
			CharacterHUD.RecreateHudTextsIfControlling(this.character);
			if (item.CampaignInteractionType == CampaignMode.InteractionType.Cargo)
			{
				item.AssignCampaignInteractionType(CampaignMode.InteractionType.None, null);
			}
			item.Equipper = user;
		}

		// Token: 0x06001806 RID: 6150 RVA: 0x000EF7D4 File Offset: 0x000ED9D4
		protected override void CreateNetworkEvent(Range slotRange)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null)
			{
				return;
			}
			networkMember.CreateEntityEvent(this.character, new Character.InventoryStateEventData(slotRange));
		}

		// Token: 0x06001808 RID: 6152 RVA: 0x000EF832 File Offset: 0x000EDA32
		[CompilerGenerated]
		internal static void <get_LimbSlotIcons>g__AddIfMissing|4_0(InvSlotType slotType, Sprite sprite)
		{
			CharacterInventory.limbSlotIcons.TryAdd(slotType, sprite);
		}

		// Token: 0x06001809 RID: 6153 RVA: 0x000EF841 File Offset: 0x000EDA41
		[CompilerGenerated]
		internal static int <SetSlotPositions>g__GetBottomOffset|19_0(int multiplier, ref CharacterInventory.<>c__DisplayClass19_0 A_1)
		{
			return CharacterInventory.SlotSize.Y + A_1.spacing * multiplier + Inventory.ContainedIndicatorHeight;
		}

		// Token: 0x0600180A RID: 6154 RVA: 0x000EF85C File Offset: 0x000EDA5C
		[CompilerGenerated]
		internal static int <SetSlotPositions>g__GetVerticalOffsetFromBottom|19_1(int multiplier, ref CharacterInventory.<>c__DisplayClass19_0 A_1)
		{
			return GameMain.GraphicsHeight - (CharacterInventory.<SetSlotPositions>g__GetBottomOffset|19_0(multiplier, ref A_1) + A_1.spacing) * multiplier - (int)(Inventory.UnequippedIndicator.size.Y * Inventory.UIScale);
		}

		// Token: 0x0600180F RID: 6159 RVA: 0x000EF960 File Offset: 0x000EDB60
		[CompilerGenerated]
		internal static float <QuickUseItem>g__GetContainPriority|29_1(Item item, Item containerItem)
		{
			ItemContainer container = containerItem.GetComponent<ItemContainer>();
			if (container == null)
			{
				return 0f;
			}
			for (int i = 0; i < container.Inventory.Capacity; i++)
			{
				IEnumerable<Item> containedItems = container.Inventory.GetItemsAt(i);
				if (containedItems.Any<Item>() && container.Inventory.CanBePutInSlot(item, i, false))
				{
					return 10f;
				}
			}
			return -container.GetContainedIndicatorState();
		}

		// Token: 0x04000C58 RID: 3160
		private static Dictionary<InvSlotType, Sprite> limbSlotIcons;

		// Token: 0x04000C59 RID: 3161
		public const InvSlotType PersonalSlots = InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag;

		// Token: 0x04000C5A RID: 3162
		private Point screenResolution;

		// Token: 0x04000C5B RID: 3163
		public Vector2[] SlotPositions;

		// Token: 0x04000C5C RID: 3164
		public static Point SlotSize;

		// Token: 0x04000C5D RID: 3165
		private CharacterInventory.Layout layout;

		// Token: 0x04000C5E RID: 3166
		private Rectangle personalSlotArea;

		// Token: 0x04000C5F RID: 3167
		private static readonly List<Inventory.SlotReference> hideSubInventories = new List<Inventory.SlotReference>();

		// Token: 0x04000C60 RID: 3168
		private static readonly List<Inventory.SlotReference> tempHighlightedSubInventorySlots = new List<Inventory.SlotReference>();

		// Token: 0x04000C61 RID: 3169
		private readonly Character character;

		// Token: 0x04000C63 RID: 3171
		private readonly Dictionary<InvSlotType, List<Inventory.ItemSlot>> slotsByType = new Dictionary<InvSlotType, List<Inventory.ItemSlot>>();

		// Token: 0x04000C64 RID: 3172
		public static readonly List<InvSlotType> AnySlot = new List<InvSlotType>
		{
			InvSlotType.Any
		};

		// Token: 0x04000C65 RID: 3173
		public static readonly List<InvSlotType> BagSlot = new List<InvSlotType>
		{
			InvSlotType.Bag
		};

		// Token: 0x04000C66 RID: 3174
		protected bool[] IsEquipped;

		// Token: 0x02000A49 RID: 2633
		public enum Layout
		{
			// Token: 0x040043C6 RID: 17350
			Default,
			// Token: 0x040043C7 RID: 17351
			Left,
			// Token: 0x040043C8 RID: 17352
			Right,
			// Token: 0x040043C9 RID: 17353
			Center
		}

		// Token: 0x02000A4A RID: 2634
		private enum QuickUseAction
		{
			// Token: 0x040043CB RID: 17355
			None,
			// Token: 0x040043CC RID: 17356
			Equip,
			// Token: 0x040043CD RID: 17357
			Unequip,
			// Token: 0x040043CE RID: 17358
			Drop,
			// Token: 0x040043CF RID: 17359
			TakeFromContainer,
			// Token: 0x040043D0 RID: 17360
			TakeFromCharacter,
			// Token: 0x040043D1 RID: 17361
			PutToContainer,
			// Token: 0x040043D2 RID: 17362
			PutToCharacter,
			// Token: 0x040043D3 RID: 17363
			PutToEquippedItem,
			// Token: 0x040043D4 RID: 17364
			UseTreatment
		}

		// Token: 0x02000A4B RID: 2635
		public enum AccessLevel
		{
			// Token: 0x040043D6 RID: 17366
			OnlyIfIncapacitated,
			// Token: 0x040043D7 RID: 17367
			AllowBotsAndPets,
			// Token: 0x040043D8 RID: 17368
			AllowFriendly
		}
	}
}
