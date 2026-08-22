using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B4 RID: 1460
	internal class ItemContainer : ItemComponent, IDrawableComponent
	{
		// Token: 0x170016A7 RID: 5799
		// (get) Token: 0x06005A41 RID: 23105 RVA: 0x002E5690 File Offset: 0x002E3890
		public Sprite InventoryTopSprite
		{
			get
			{
				return this.inventoryTopSprite;
			}
		}

		// Token: 0x170016A8 RID: 5800
		// (get) Token: 0x06005A42 RID: 23106 RVA: 0x002E5698 File Offset: 0x002E3898
		public Sprite InventoryBackSprite
		{
			get
			{
				return this.inventoryBackSprite;
			}
		}

		// Token: 0x170016A9 RID: 5801
		// (get) Token: 0x06005A43 RID: 23107 RVA: 0x002E56A0 File Offset: 0x002E38A0
		public Sprite InventoryBottomSprite
		{
			get
			{
				return this.inventoryBottomSprite;
			}
		}

		// Token: 0x170016AA RID: 5802
		// (get) Token: 0x06005A44 RID: 23108 RVA: 0x002E56A8 File Offset: 0x002E38A8
		// (set) Token: 0x06005A45 RID: 23109 RVA: 0x002E56B0 File Offset: 0x002E38B0
		public Sprite ContainedStateIndicator { get; private set; }

		// Token: 0x170016AB RID: 5803
		// (get) Token: 0x06005A46 RID: 23110 RVA: 0x002E56B9 File Offset: 0x002E38B9
		// (set) Token: 0x06005A47 RID: 23111 RVA: 0x002E56C1 File Offset: 0x002E38C1
		public Sprite ContainedStateIndicatorEmpty { get; private set; }

		// Token: 0x170016AC RID: 5804
		// (get) Token: 0x06005A48 RID: 23112 RVA: 0x002E56CA File Offset: 0x002E38CA
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170016AD RID: 5805
		// (get) Token: 0x06005A49 RID: 23113 RVA: 0x002E56CD File Offset: 0x002E38CD
		// (set) Token: 0x06005A4A RID: 23114 RVA: 0x002E56D5 File Offset: 0x002E38D5
		[Serialize(-1f, IsPropertySaveable.No, "Depth at which the contained sprites are drawn. If not set, the original depth of the item sprites is used.", "", false)]
		public float ContainedSpriteDepth { get; set; }

		// Token: 0x170016AE RID: 5806
		// (get) Token: 0x06005A4B RID: 23115 RVA: 0x002E56DE File Offset: 0x002E38DE
		// (set) Token: 0x06005A4C RID: 23116 RVA: 0x002E56E6 File Offset: 0x002E38E6
		[Serialize(null, IsPropertySaveable.No, "An optional text displayed above the item's inventory.", "", false)]
		public string UILabel { get; set; }

		// Token: 0x170016AF RID: 5807
		// (get) Token: 0x06005A4D RID: 23117 RVA: 0x002E56EF File Offset: 0x002E38EF
		// (set) Token: 0x06005A4E RID: 23118 RVA: 0x002E56F7 File Offset: 0x002E38F7
		public GUIComponentStyle IndicatorStyle { get; set; }

		// Token: 0x170016B0 RID: 5808
		// (get) Token: 0x06005A4F RID: 23119 RVA: 0x002E5700 File Offset: 0x002E3900
		// (set) Token: 0x06005A50 RID: 23120 RVA: 0x002E5708 File Offset: 0x002E3908
		[Serialize(null, IsPropertySaveable.No, "", "", false)]
		public string ContainedStateIndicatorStyle { get; set; }

		// Token: 0x170016B1 RID: 5809
		// (get) Token: 0x06005A51 RID: 23121 RVA: 0x002E5711 File Offset: 0x002E3911
		// (set) Token: 0x06005A52 RID: 23122 RVA: 0x002E5719 File Offset: 0x002E3919
		[Serialize(-1, IsPropertySaveable.No, "Can be used to make the contained state indicator display the condition of the item in a specific slot even when the container's capacity is more than 1.", "", false)]
		public int ContainedStateIndicatorSlot { get; set; }

		// Token: 0x170016B2 RID: 5810
		// (get) Token: 0x06005A53 RID: 23123 RVA: 0x002E5722 File Offset: 0x002E3922
		// (set) Token: 0x06005A54 RID: 23124 RVA: 0x002E572A File Offset: 0x002E392A
		[Serialize(true, IsPropertySaveable.No, "Should an indicator displaying the state of the contained items be displayed on this item's inventory slot. If this item can only contain one item, the indicator will display the condition of the contained item, otherwise it will indicate how full the item is.", "", false)]
		public bool ShowContainedStateIndicator { get; set; }

		// Token: 0x170016B3 RID: 5811
		// (get) Token: 0x06005A55 RID: 23125 RVA: 0x002E5733 File Offset: 0x002E3933
		// (set) Token: 0x06005A56 RID: 23126 RVA: 0x002E573B File Offset: 0x002E393B
		[Serialize(false, IsPropertySaveable.No, "If enabled, the condition of this item is displayed in the indicator that would normally show the state of the contained items. May be useful for items such as ammo boxes and magazines that spawn projectiles as needed, and use the condition to determine how many projectiles can be spawned in total.", "", false)]
		public bool ShowConditionInContainedStateIndicator { get; set; }

		// Token: 0x170016B4 RID: 5812
		// (get) Token: 0x06005A57 RID: 23127 RVA: 0x002E5744 File Offset: 0x002E3944
		// (set) Token: 0x06005A58 RID: 23128 RVA: 0x002E574C File Offset: 0x002E394C
		[Serialize(false, IsPropertySaveable.No, "If true, the contained state indicator calculates how full the item is based on the total amount of items that can be stacked inside it, as opposed to how many of the inventory slots are occupied. Note that only items in the main container or in the subcontainer are counted, depending on which container the first containable item match is found in. The item determining this can be defined with ContainedStateIndicatorSlot", "", false)]
		public bool ShowTotalStackCapacityInContainedStateIndicator { get; set; }

		// Token: 0x170016B5 RID: 5813
		// (get) Token: 0x06005A59 RID: 23129 RVA: 0x002E5755 File Offset: 0x002E3955
		// (set) Token: 0x06005A5A RID: 23130 RVA: 0x002E575D File Offset: 0x002E395D
		[Serialize(false, IsPropertySaveable.No, "Should the inventory of this item be kept open when the item is equipped by a character.", "", false)]
		public bool KeepOpenWhenEquipped { get; set; }

		// Token: 0x170016B6 RID: 5814
		// (get) Token: 0x06005A5B RID: 23131 RVA: 0x002E5766 File Offset: 0x002E3966
		// (set) Token: 0x06005A5C RID: 23132 RVA: 0x002E576E File Offset: 0x002E396E
		[Serialize(false, IsPropertySaveable.No, "Can the inventory of this item be moved around on the screen by the player.", "", false)]
		public bool MovableFrame { get; set; }

		// Token: 0x170016B7 RID: 5815
		// (get) Token: 0x06005A5D RID: 23133 RVA: 0x002E5777 File Offset: 0x002E3977
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005A5E RID: 23134 RVA: 0x002E5780 File Offset: 0x002E3980
		protected override void CreateGUI()
		{
			GUIFrame content = new GUIFrame(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null)
			{
				CanBeFocused = false
			};
			LocalizedString labelText = this.GetUILabel();
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = labelText;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock label = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.TopLeft, true, "", null)
			{
				IgnoreLayoutGroups = true
			};
			int buttonSize = GUIStyle.ItemFrameTopBarHeight;
			Point margin = new Point(buttonSize / 4, buttonSize / 6);
			int buttonCount = 0;
			GUILayoutGroup buttonArea = new GUILayoutGroup(new RectTransform(new Point(content.Rect.Width, buttonSize - margin.Y * 2), content.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(0, margin.Y)
			}, true, Anchor.TopRight)
			{
				AbsoluteSpacing = margin.X / 2
			};
			if (this.Inventory.Capacity > 1)
			{
				if (this.ShowSortButton)
				{
					buttonCount++;
					GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), Alignment.Center, "SortItemsButton", null);
					guibutton.ToolTip = TextManager.Get("SortItemsAlphabetically");
					guibutton.OnClicked = delegate(GUIButton btn, object userdata)
					{
						this.SortItems();
						return true;
					};
				}
				if (this.ShowMergeButton)
				{
					buttonCount++;
					GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, buttonArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Smallest), Alignment.Center, "MergeStacksButton", null);
					guibutton2.ToolTip = TextManager.Get("MergeItemStacks");
					guibutton2.OnClicked = delegate(GUIButton btn, object userdata)
					{
						this.MergeStacks();
						return true;
					};
				}
			}
			if (buttonCount > 0)
			{
				label.RectTransform.MaxSize = new Point(label.Parent.Rect.Width - buttonCount * buttonSize, int.MaxValue);
			}
			float minInventoryAreaSize = 0.5f;
			this.guiCustomComponent = new GUICustomComponent(new RectTransform(new Vector2(1f, (label == null) ? 1f : Math.Max(1f - label.RectTransform.RelativeSize.Y, minInventoryAreaSize)), content.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				this.Inventory.Draw(spriteBatch, false);
			}, null)
			{
				CanBeFocused = true
			};
			if (label != null && label.RectTransform.RelativeSize.Y > 0.5f)
			{
				int newHeight = (int)((float)base.GuiFrame.Rect.Height + 2f * (label.RectTransform.RelativeSize.Y - 0.5f) * (float)content.Rect.Height);
				if (newHeight > base.GuiFrame.RectTransform.MaxSize.Y)
				{
					Point newMaxSize = base.GuiFrame.RectTransform.MaxSize;
					newMaxSize.Y = newHeight;
					base.GuiFrame.RectTransform.MaxSize = newMaxSize;
				}
				base.GuiFrame.RectTransform.Resize(new Point(base.GuiFrame.Rect.Width, newHeight), true);
				content.RectTransform.Resize(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, true);
				label.CalculateHeightFromText(0, false);
				this.guiCustomComponent.RectTransform.Resize(new Vector2(1f, Math.Max(1f - label.RectTransform.RelativeSize.Y, minInventoryAreaSize)), true);
			}
			this.Inventory.RectTransform = this.guiCustomComponent.RectTransform;
		}

		// Token: 0x06005A5F RID: 23135 RVA: 0x002E5BDC File Offset: 0x002E3DDC
		private void SortItems()
		{
			List<List<Item>> itemsPerSlot = new List<List<Item>>();
			for (int k = 0; k < this.Inventory.Capacity; k++)
			{
				List<Item> items = this.Inventory.GetItemsAt(k).ToList<Item>();
				if (items.Any<Item>())
				{
					itemsPerSlot.Add(items);
					items.ForEach(delegate(Item it)
					{
						it.Drop(null, false, false);
					});
				}
			}
			IOrderedEnumerable<List<Item>> sortedItems = from i in itemsPerSlot
			orderby i.First<Item>().Name, i.Count descending, i.First<Item>().ContainedItems.Count<Item>() descending
			select i;
			foreach (List<Item> items2 in sortedItems)
			{
				int firstFreeSlot = -1;
				for (int j = 0; j < this.Inventory.Capacity; j++)
				{
					if (this.Inventory.GetItemAt(j) == null && this.Inventory.CanBePut(items2.First<Item>()))
					{
						firstFreeSlot = j;
						break;
					}
				}
				if (firstFreeSlot == -1)
				{
					items2.ForEach(delegate(Item it)
					{
						it.Drop(null, true, true);
					});
				}
				else
				{
					foreach (Item item in items2)
					{
						if (!this.Inventory.TryPutItem(item, firstFreeSlot, false, false, null, false, false, true) && !this.Inventory.TryPutItem(item, null, null, false, false, true))
						{
							item.Drop(null, true, true);
						}
					}
				}
			}
			this.Inventory.CreateNetworkEvent();
		}

		// Token: 0x06005A60 RID: 23136 RVA: 0x002E5DE8 File Offset: 0x002E3FE8
		private void MergeStacks()
		{
			for (int i = this.Inventory.Capacity - 1; i >= 0; i--)
			{
				List<Item> items = this.Inventory.GetItemsAt(i).ToList<Item>();
				if (!items.None(null))
				{
					int k;
					int j;
					for (j = 0; j < i; j = k + 1)
					{
						if (this.Inventory.GetItemsAt(j).Any<Item>() && this.Inventory.CanBePutInSlot(items.First<Item>(), j, false))
						{
							items.ForEach(delegate(Item it)
							{
								this.Inventory.TryPutItem(it, j, false, false, null, false, false, true);
							});
							break;
						}
						k = j;
					}
				}
			}
			this.Inventory.CreateNetworkEvent();
		}

		// Token: 0x06005A61 RID: 23137 RVA: 0x002E5EB0 File Offset: 0x002E40B0
		public LocalizedString GetUILabel()
		{
			if (this.UILabel == string.Empty)
			{
				return string.Empty;
			}
			if (this.UILabel != null)
			{
				return TextManager.Get("UILabel." + this.UILabel).Fallback(TextManager.Get(this.UILabel), true);
			}
			Item item = this.item;
			if (item == null)
			{
				return null;
			}
			return item.Prefab.Name;
		}

		// Token: 0x06005A62 RID: 23138 RVA: 0x002E5F1F File Offset: 0x002E411F
		public Sprite GetSlotIcon(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= this.slotIcons.Length)
			{
				return null;
			}
			return this.slotIcons[slotIndex];
		}

		// Token: 0x06005A63 RID: 23139 RVA: 0x002E5F3C File Offset: 0x002E413C
		public bool KeepOpenWhenEquippedBy(Character character)
		{
			if (!this.KeepOpenWhenEquipped || !character.HasEquippedItem(base.Item, null, null) || !character.CanAccessInventory(this.Inventory, CharacterInventory.AccessLevel.AllowBotsAndPets))
			{
				return false;
			}
			if (character.HeldItems.Count<Item>() > 1)
			{
				if (character.HeldItems.All(delegate(Item it)
				{
					ItemContainer component = it.GetComponent<ItemContainer>();
					return component != null && component.KeepOpenWhenEquipped;
				}))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06005A64 RID: 23140 RVA: 0x002E5FB8 File Offset: 0x002E41B8
		public float GetContainedIndicatorState()
		{
			if (this.ShowConditionInContainedStateIndicator)
			{
				return this.item.Condition / this.item.MaxCondition;
			}
			int targetSlot = Math.Max(this.ContainedStateIndicatorSlot, 0);
			if (targetSlot >= this.Inventory.Capacity)
			{
				return 0f;
			}
			IEnumerable<Item> containedItems = this.Inventory.GetItemsAt(targetSlot);
			if (containedItems == null)
			{
				return 0f;
			}
			Item containedItem = containedItems.FirstOrDefault<Item>();
			if (this.ShowTotalStackCapacityInContainedStateIndicator)
			{
				if (containedItem == null)
				{
					containedItem = (containedItems.FirstOrDefault<Item>() ?? this.Inventory.AllItems.FirstOrDefault((Item it) => this.CanBeContained(it, targetSlot)));
				}
				if (containedItem == null)
				{
					return 0f;
				}
				int ignoredItemCount = 0;
				List<RelatedItem> subContainableItems = this.AllSubContainableItems;
				float targetSlotCapacity = (float)Math.Min(containedItem.Prefab.MaxStackSize, this.GetMaxStackSize(targetSlot));
				float capacity = targetSlotCapacity * (float)this.MainContainerCapacity;
				if (subContainableItems != null)
				{
					bool useMainContainerCapacity = true;
					RelatedItem ri;
					foreach (Item it2 in this.Inventory.AllItems)
					{
						using (List<RelatedItem>.Enumerator enumerator2 = subContainableItems.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ri = enumerator2.Current;
								if (ri.MatchesItem(containedItem))
								{
									useMainContainerCapacity = false;
									break;
								}
								if (ri.MatchesItem(it2))
								{
									ignoredItemCount++;
								}
							}
						}
						if (!useMainContainerCapacity)
						{
							break;
						}
					}
					if (!useMainContainerCapacity)
					{
						ignoredItemCount = this.Inventory.AllItems.Count((Item it) => subContainableItems.Any((RelatedItem ri) => !ri.MatchesItem(it)));
						capacity = targetSlotCapacity * (float)(this.Capacity - this.MainContainerCapacity);
					}
				}
				int itemCount = this.Inventory.AllItems.Count<Item>() - ignoredItemCount;
				return Math.Min((float)itemCount / Math.Max(capacity, 1f), 1f);
			}
			else
			{
				if (this.Inventory.Capacity != 1 && this.ContainedStateIndicatorSlot <= -1)
				{
					return (float)this.Inventory.EmptySlotCount / (float)this.Inventory.Capacity;
				}
				if (containedItem == null)
				{
					return 0f;
				}
				ItemContainer containedItemContainer = containedItem.GetComponent<ItemContainer>();
				if (containedItemContainer != null && containedItemContainer.ShowContainedStateIndicator)
				{
					return containedItemContainer.GetContainedIndicatorState();
				}
				int maxStackSize = Math.Min(containedItem.Prefab.GetMaxStackSize(this.Inventory), this.GetMaxStackSize(targetSlot));
				if (maxStackSize == 1)
				{
					return containedItem.Condition / containedItem.MaxCondition;
				}
				return (float)containedItems.Count<Item>() / (float)maxStackSize;
			}
		}

		// Token: 0x06005A65 RID: 23141 RVA: 0x002E6270 File Offset: 0x002E4470
		public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.hideItems || (this.item.body != null && !this.item.body.Enabled))
			{
				return;
			}
			this.DrawContainedItems(spriteBatch, itemDepth, overrideColor);
		}

		// Token: 0x06005A66 RID: 23142 RVA: 0x002E62A4 File Offset: 0x002E44A4
		public void DrawContainedItems(SpriteBatch spriteBatch, float itemDepth, Color? overrideColor = null)
		{
			Item rootContainer = this.item.RootContainer;
			PhysicsBody rootBody = ((rootContainer != null) ? rootContainer.body : null) ?? this.item.body;
			Vector2 transformedItemIntervalHorizontal;
			Vector2 transformedItemIntervalVertical;
			bool flippedX;
			bool flippedY;
			Vector2 transformedItemPos = this.GetContainedPosition(true, out transformedItemIntervalHorizontal, out transformedItemIntervalVertical, out flippedX, out flippedY);
			bool isWiringMode = SubEditorScreen.TransparentWiringMode && SubEditorScreen.IsWiringMode();
			int i = 0;
			foreach (ItemContainer.ContainedItem contained in this.containedItems)
			{
				Item item = contained.Item;
				if (((item != null) ? item.Sprite : null) != null && !contained.Hide)
				{
					Vector2 itemPos = transformedItemPos;
					int targetSlotIndex = this.ItemsUseInventoryPlacement ? this.Inventory.FindIndex(contained.Item) : i;
					if (Math.Abs(this.ItemInterval.X) > 0.001f && Math.Abs(this.ItemInterval.Y) > 0.001f)
					{
						itemPos += transformedItemIntervalHorizontal * (float)(targetSlotIndex % this.ItemsPerRow);
						itemPos += transformedItemIntervalVertical * (float)(targetSlotIndex / this.ItemsPerRow);
					}
					else
					{
						itemPos += (transformedItemIntervalHorizontal + transformedItemIntervalVertical) * (float)targetSlotIndex;
					}
					if (contained.ItemPos != null)
					{
						Vector2 pos = contained.ItemPos.Value;
						if (this.item.body != null)
						{
							Matrix transform = Matrix.CreateRotationZ(this.item.body.DrawRotation);
							pos.X *= rootBody.Dir;
							itemPos = Vector2.Transform(pos, transform) + this.item.body.DrawPosition;
						}
						else
						{
							itemPos = pos;
							if (flippedX)
							{
								itemPos.X = -itemPos.X;
								itemPos.X += (float)this.item.Rect.Width;
							}
							if (flippedY)
							{
								itemPos.Y = -itemPos.Y;
								itemPos.Y -= (float)this.item.Rect.Height;
							}
							itemPos += new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y);
							if (this.item.Submarine != null)
							{
								itemPos += this.item.Submarine.DrawPosition;
							}
							if (Math.Abs(this.item.RotationRad) > 0.01f)
							{
								Matrix transform2 = Matrix.CreateRotationZ(-this.item.RotationRad);
								itemPos = Vector2.Transform(itemPos - this.item.DrawPosition, transform2) + this.item.DrawPosition;
							}
						}
					}
					if (this.CanAutoInteractWithContained(contained.Item))
					{
						Screen selected = Screen.Selected;
						if (selected == null || !selected.IsEditor)
						{
							contained.Item.IsHighlighted = this.item.IsHighlighted;
							this.item.IsHighlighted = false;
						}
					}
					Vector2 origin = contained.Item.Sprite.Origin;
					if (flippedX)
					{
						origin.X = (float)contained.Item.Sprite.SourceRect.Width - origin.X;
					}
					if (flippedY)
					{
						origin.Y = (float)contained.Item.Sprite.SourceRect.Height - origin.Y;
					}
					float containedSpriteDepth = (this.ContainedSpriteDepth < 0f) ? contained.Item.Sprite.Depth : this.ContainedSpriteDepth;
					if (targetSlotIndex < this.containedSpriteDepths.Length)
					{
						containedSpriteDepth = this.containedSpriteDepths[targetSlotIndex];
					}
					float num = containedSpriteDepth;
					Sprite sprite = this.item.Sprite;
					containedSpriteDepth = itemDepth + (num - ((sprite != null) ? sprite.Depth : this.item.SpriteDepth)) / 10000f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					float spriteRotation = this.ItemRotation;
					if (contained.Rotation != 0f)
					{
						spriteRotation = contained.Rotation;
					}
					bool flipX = (rootBody != null && rootBody.Dir == -1f) || flippedX;
					if (flipX)
					{
						spriteEffects |= SpriteEffects.FlipHorizontally;
					}
					bool flipY = flippedY;
					if (flipY)
					{
						spriteEffects |= SpriteEffects.FlipVertically;
					}
					contained.Item.Sprite.Draw(spriteBatch, new Vector2(itemPos.X, -itemPos.Y), overrideColor ?? (isWiringMode ? (contained.Item.GetSpriteColor(null, true) * 0.15f) : contained.Item.GetSpriteColor(null, true)), origin, -((contained.Item.body == null) ? 0f : contained.Item.body.DrawRotation), contained.Item.Scale, spriteEffects, new float?(containedSpriteDepth));
					contained.Item.DrawDecorativeSprites(spriteBatch, itemPos, flipX, flipY, (contained.Item.body == null) ? 0f : contained.Item.body.DrawRotation, containedSpriteDepth, overrideColor);
					foreach (ItemContainer ic in contained.Item.GetComponents<ItemContainer>())
					{
						if (!ic.hideItems)
						{
							ic.DrawContainedItems(spriteBatch, containedSpriteDepth, overrideColor);
						}
					}
					i++;
				}
			}
		}

		// Token: 0x06005A67 RID: 23143 RVA: 0x002E6868 File Offset: 0x002E4A68
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (!this.item.IsInteractable(character))
			{
				return;
			}
			if (this.Inventory.RectTransform != null)
			{
				this.guiCustomComponent.RectTransform.Parent = this.Inventory.RectTransform;
			}
			Inventory parentInventory = this.item.ParentInventory;
			if (((parentInventory != null) ? parentInventory.Owner : null) == character && character.SelectedItem == this.item)
			{
				character.SelectedItem = null;
			}
			GUIComponent guicomponent = this.guiCustomComponent;
			bool visible;
			if (this.DrawInventory)
			{
				Inventory parentInventory2 = this.item.ParentInventory;
				visible = (((parentInventory2 != null) ? parentInventory2.Owner : null) != character || this.Inventory.DrawWhenEquipped);
			}
			else
			{
				visible = false;
			}
			guicomponent.Visible = visible;
			if (!this.guiCustomComponent.Visible)
			{
				return;
			}
			this.Inventory.Update(deltaTime, cam, false);
		}

		// Token: 0x170016B8 RID: 5816
		// (get) Token: 0x06005A68 RID: 23144 RVA: 0x002E6935 File Offset: 0x002E4B35
		// (set) Token: 0x06005A69 RID: 23145 RVA: 0x002E693D File Offset: 0x002E4B3D
		[Serialize(5, IsPropertySaveable.No, "How many items can be contained inside this item.", "", false)]
		public int Capacity
		{
			get
			{
				return this.capacity;
			}
			private set
			{
				this.capacity = Math.Max(value, 0);
				this.MainContainerCapacity = value;
			}
		}

		// Token: 0x170016B9 RID: 5817
		// (get) Token: 0x06005A6A RID: 23146 RVA: 0x002E6953 File Offset: 0x002E4B53
		// (set) Token: 0x06005A6B RID: 23147 RVA: 0x002E695B File Offset: 0x002E4B5B
		public int MainContainerCapacity { get; private set; }

		// Token: 0x170016BA RID: 5818
		// (get) Token: 0x06005A6C RID: 23148 RVA: 0x002E6964 File Offset: 0x002E4B64
		// (set) Token: 0x06005A6D RID: 23149 RVA: 0x002E696C File Offset: 0x002E4B6C
		[Serialize(64, IsPropertySaveable.No, "How many items can be stacked in one slot. Does not increase the maximum stack size of the items themselves, e.g. a stack of bullets could have a maximum size of 8 but the number of bullets in a specific weapon could be restricted to 6.", "", false)]
		public int MaxStackSize
		{
			get
			{
				return this.maxStackSize;
			}
			set
			{
				this.maxStackSize = Math.Max(value, 1);
			}
		}

		// Token: 0x170016BB RID: 5819
		// (get) Token: 0x06005A6E RID: 23150 RVA: 0x002E697B File Offset: 0x002E4B7B
		// (set) Token: 0x06005A6F RID: 23151 RVA: 0x002E6983 File Offset: 0x002E4B83
		[Serialize(true, IsPropertySaveable.No, "Should the items contained inside this item be hidden. If set to false, you should use the ItemPos and ItemInterval properties to determine where the items get rendered.", "", false)]
		public bool HideItems
		{
			get
			{
				return this.hideItems;
			}
			set
			{
				this.hideItems = value;
				base.Drawable = !this.hideItems;
			}
		}

		// Token: 0x170016BC RID: 5820
		// (get) Token: 0x06005A70 RID: 23152 RVA: 0x002E699B File Offset: 0x002E4B9B
		// (set) Token: 0x06005A71 RID: 23153 RVA: 0x002E69A3 File Offset: 0x002E4BA3
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position where the contained items get drawn at (offset from the upper left corner of the sprite in pixels).", "", false)]
		public Vector2 ItemPos { get; set; }

		// Token: 0x170016BD RID: 5821
		// (get) Token: 0x06005A72 RID: 23154 RVA: 0x002E69AC File Offset: 0x002E4BAC
		// (set) Token: 0x06005A73 RID: 23155 RVA: 0x002E69B4 File Offset: 0x002E4BB4
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The interval at which the contained items are spaced apart from each other (in pixels).", "", false)]
		public Vector2 ItemInterval { get; set; }

		// Token: 0x170016BE RID: 5822
		// (get) Token: 0x06005A74 RID: 23156 RVA: 0x002E69BD File Offset: 0x002E4BBD
		// (set) Token: 0x06005A75 RID: 23157 RVA: 0x002E69C5 File Offset: 0x002E4BC5
		[Serialize(100, IsPropertySaveable.No, "How many items are placed in a row before starting a new row.", "", false)]
		public int ItemsPerRow { get; set; }

		// Token: 0x170016BF RID: 5823
		// (get) Token: 0x06005A76 RID: 23158 RVA: 0x002E69CE File Offset: 0x002E4BCE
		// (set) Token: 0x06005A77 RID: 23159 RVA: 0x002E69D6 File Offset: 0x002E4BD6
		[Serialize(false, IsPropertySaveable.No, "Should items be drawn based on their position within the inventory?", "", false)]
		public bool ItemsUseInventoryPlacement { get; set; }

		// Token: 0x170016C0 RID: 5824
		// (get) Token: 0x06005A78 RID: 23160 RVA: 0x002E69DF File Offset: 0x002E4BDF
		// (set) Token: 0x06005A79 RID: 23161 RVA: 0x002E69E7 File Offset: 0x002E4BE7
		[Serialize(true, IsPropertySaveable.No, "Should the inventory of this item be visible when the item is selected. Note that this does not prevent dragging and dropping items to the item.", "", false)]
		public bool DrawInventory { get; set; }

		// Token: 0x170016C1 RID: 5825
		// (get) Token: 0x06005A7A RID: 23162 RVA: 0x002E69F0 File Offset: 0x002E4BF0
		// (set) Token: 0x06005A7B RID: 23163 RVA: 0x002E69F8 File Offset: 0x002E4BF8
		[Serialize(true, IsPropertySaveable.No, "Allow dragging and dropping items to deposit items into this inventory.", "", false)]
		public bool AllowDragAndDrop { get; set; }

		// Token: 0x170016C2 RID: 5826
		// (get) Token: 0x06005A7C RID: 23164 RVA: 0x002E6A01 File Offset: 0x002E4C01
		// (set) Token: 0x06005A7D RID: 23165 RVA: 0x002E6A09 File Offset: 0x002E4C09
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool AllowSwappingContainedItems { get; set; }

		// Token: 0x170016C3 RID: 5827
		// (get) Token: 0x06005A7E RID: 23166 RVA: 0x002E6A12 File Offset: 0x002E4C12
		// (set) Token: 0x06005A7F RID: 23167 RVA: 0x002E6A1A File Offset: 0x002E4C1A
		[Serialize(true, IsPropertySaveable.No, "Should a button that allows sorting the items alphabetically be shown in the container's UI panel?", "", false)]
		public bool ShowSortButton { get; set; }

		// Token: 0x170016C4 RID: 5828
		// (get) Token: 0x06005A80 RID: 23168 RVA: 0x002E6A23 File Offset: 0x002E4C23
		// (set) Token: 0x06005A81 RID: 23169 RVA: 0x002E6A2B File Offset: 0x002E4C2B
		[Serialize(true, IsPropertySaveable.No, "Should a button that merges items into stacks be shown in the container's UI panel?", "", false)]
		public bool ShowMergeButton { get; set; }

		// Token: 0x170016C5 RID: 5829
		// (get) Token: 0x06005A82 RID: 23170 RVA: 0x002E6A34 File Offset: 0x002E4C34
		// (set) Token: 0x06005A83 RID: 23171 RVA: 0x002E6A3C File Offset: 0x002E4C3C
		[Serialize(true, IsPropertySaveable.Yes, "When this item is equipped, and you 'quick use' (double click / equip button) another equippable item, should the game attempt to move that item inside this one?", "", false)]
		public bool QuickUseMovesItemsInside { get; set; }

		// Token: 0x170016C6 RID: 5830
		// (get) Token: 0x06005A84 RID: 23172 RVA: 0x002E6A45 File Offset: 0x002E4C45
		// (set) Token: 0x06005A85 RID: 23173 RVA: 0x002E6A4D File Offset: 0x002E4C4D
		[Serialize(false, IsPropertySaveable.No, "If set to true, interacting with this item will make the character interact with the contained item(s), automatically picking them up if they can be picked up.", "", false)]
		public bool AutoInteractWithContained { get; set; }

		// Token: 0x170016C7 RID: 5831
		// (get) Token: 0x06005A86 RID: 23174 RVA: 0x002E6A56 File Offset: 0x002E4C56
		// (set) Token: 0x06005A87 RID: 23175 RVA: 0x002E6A68 File Offset: 0x002E4C68
		[Serialize("", IsPropertySaveable.Yes, "Interacting with this container will autointeract with contained items that have one of these tags. Only valid if AutoInteractWithContained is set to true.", "", false)]
		public string AutoInteractWithContainedTags
		{
			get
			{
				return this.autoInteractWithContainedTags.ConvertToString(",");
			}
			set
			{
				this.autoInteractWithContainedTags = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			}
		}

		// Token: 0x170016C8 RID: 5832
		// (get) Token: 0x06005A88 RID: 23176 RVA: 0x002E6A80 File Offset: 0x002E4C80
		// (set) Token: 0x06005A89 RID: 23177 RVA: 0x002E6A88 File Offset: 0x002E4C88
		[Serialize(true, IsPropertySaveable.No, "Is the container accessible in general.", "", false)]
		public bool AllowAccess { get; set; }

		// Token: 0x170016C9 RID: 5833
		// (get) Token: 0x06005A8A RID: 23178 RVA: 0x002E6A91 File Offset: 0x002E4C91
		// (set) Token: 0x06005A8B RID: 23179 RVA: 0x002E6A99 File Offset: 0x002E4C99
		[Serialize(false, IsPropertySaveable.No, "Is the container only accessible when it's broken. Doesn't apply to editors.", "", false)]
		public bool AccessOnlyWhenBroken { get; set; }

		// Token: 0x170016CA RID: 5834
		// (get) Token: 0x06005A8C RID: 23180 RVA: 0x002E6AA2 File Offset: 0x002E4CA2
		// (set) Token: 0x06005A8D RID: 23181 RVA: 0x002E6AAA File Offset: 0x002E4CAA
		[Serialize(true, IsPropertySaveable.No, "Is the container accessible when dropped.", "", false)]
		public bool AllowAccessWhenDropped { get; set; }

		// Token: 0x170016CB RID: 5835
		// (get) Token: 0x06005A8E RID: 23182 RVA: 0x002E6AB3 File Offset: 0x002E4CB3
		// (set) Token: 0x06005A8F RID: 23183 RVA: 0x002E6ABB File Offset: 0x002E4CBB
		[Serialize(5, IsPropertySaveable.No, "How many inventory slots the inventory has per row.", "", false)]
		public int SlotsPerRow { get; set; }

		// Token: 0x170016CC RID: 5836
		// (get) Token: 0x06005A90 RID: 23184 RVA: 0x002E6AC4 File Offset: 0x002E4CC4
		// (set) Token: 0x06005A91 RID: 23185 RVA: 0x002E6AD8 File Offset: 0x002E4CD8
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "Define items (by identifiers or tags) that bots should place inside this container. If empty, no restrictions are applied.", "", false)]
		public string ContainableRestrictions
		{
			get
			{
				return string.Join<Identifier>(",", this.containableRestrictions);
			}
			set
			{
				this.containableRestrictions.Clear();
				if (!value.IsNullOrEmpty())
				{
					foreach (string str in value.Split(',', StringSplitOptions.None))
					{
						if (!str.IsNullOrWhiteSpace())
						{
							this.containableRestrictions.Add(str.ToIdentifier());
						}
					}
				}
			}
		}

		// Token: 0x170016CD RID: 5837
		// (get) Token: 0x06005A92 RID: 23186 RVA: 0x002E6B2E File Offset: 0x002E4D2E
		// (set) Token: 0x06005A93 RID: 23187 RVA: 0x002E6B36 File Offset: 0x002E4D36
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should this container be automatically filled with items?", "", false)]
		public bool AutoFill { get; set; }

		// Token: 0x170016CE RID: 5838
		// (get) Token: 0x06005A94 RID: 23188 RVA: 0x002E6B3F File Offset: 0x002E4D3F
		// (set) Token: 0x06005A95 RID: 23189 RVA: 0x002E6B4C File Offset: 0x002E4D4C
		[Serialize(0f, IsPropertySaveable.No, "The rotation in which the contained sprites are drawn (in degrees).", "", false)]
		public float ItemRotation
		{
			get
			{
				return MathHelper.ToDegrees(this.itemRotation);
			}
			set
			{
				this.itemRotation = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170016CF RID: 5839
		// (get) Token: 0x06005A96 RID: 23190 RVA: 0x002E6B5A File Offset: 0x002E4D5A
		// (set) Token: 0x06005A97 RID: 23191 RVA: 0x002E6B62 File Offset: 0x002E4D62
		[Serialize("", IsPropertySaveable.No, "Specify an item for the container to spawn with.", "", false)]
		public string SpawnWithId { get; set; }

		// Token: 0x170016D0 RID: 5840
		// (get) Token: 0x06005A98 RID: 23192 RVA: 0x002E6B6B File Offset: 0x002E4D6B
		// (set) Token: 0x06005A99 RID: 23193 RVA: 0x002E6B73 File Offset: 0x002E4D73
		[Serialize(false, IsPropertySaveable.No, "Should the items configured using SpawnWithId spawn if this item is broken.", "", false)]
		public bool SpawnWithIdWhenBroken { get; set; }

		// Token: 0x170016D1 RID: 5841
		// (get) Token: 0x06005A9A RID: 23194 RVA: 0x002E6B7C File Offset: 0x002E4D7C
		// (set) Token: 0x06005A9B RID: 23195 RVA: 0x002E6B84 File Offset: 0x002E4D84
		[Serialize(false, IsPropertySaveable.No, "Should the items be injected into the user.", "", false)]
		public bool AutoInject { get; set; }

		// Token: 0x170016D2 RID: 5842
		// (get) Token: 0x06005A9C RID: 23196 RVA: 0x002E6B8D File Offset: 0x002E4D8D
		// (set) Token: 0x06005A9D RID: 23197 RVA: 0x002E6B95 File Offset: 0x002E4D95
		[Serialize(0.5f, IsPropertySaveable.No, "The health threshold that the user must reach in order to activate the autoinjection.", "", false)]
		public float AutoInjectThreshold { get; set; }

		// Token: 0x170016D3 RID: 5843
		// (get) Token: 0x06005A9E RID: 23198 RVA: 0x002E6B9E File Offset: 0x002E4D9E
		// (set) Token: 0x06005A9F RID: 23199 RVA: 0x002E6BA6 File Offset: 0x002E4DA6
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RemoveContainedItemsOnDeconstruct { get; set; }

		// Token: 0x170016D4 RID: 5844
		// (get) Token: 0x06005AA0 RID: 23200 RVA: 0x002E6BAF File Offset: 0x002E4DAF
		// (set) Token: 0x06005AA1 RID: 23201 RVA: 0x002E6BBC File Offset: 0x002E4DBC
		public bool Locked
		{
			get
			{
				return this.Inventory.Locked;
			}
			set
			{
				this.Inventory.Locked = value;
			}
		}

		// Token: 0x170016D5 RID: 5845
		// (get) Token: 0x06005AA2 RID: 23202 RVA: 0x002E6BCA File Offset: 0x002E4DCA
		public int ContainedItemCount
		{
			get
			{
				return this.Inventory.AllItems.Count<Item>();
			}
		}

		// Token: 0x170016D6 RID: 5846
		// (get) Token: 0x06005AA3 RID: 23203 RVA: 0x002E6BDC File Offset: 0x002E4DDC
		public int ContainedNonBrokenItemCount
		{
			get
			{
				return this.Inventory.AllItems.Count((Item it) => it.Condition > 0f);
			}
		}

		// Token: 0x170016D7 RID: 5847
		// (get) Token: 0x06005AA4 RID: 23204 RVA: 0x002E6C0D File Offset: 0x002E4E0D
		// (set) Token: 0x06005AA5 RID: 23205 RVA: 0x002E6C1A File Offset: 0x002E4E1A
		public int ExtraStackSize
		{
			get
			{
				return this.Inventory.ExtraStackSize;
			}
			set
			{
				this.Inventory.ExtraStackSize = value;
			}
		}

		// Token: 0x06005AA6 RID: 23206 RVA: 0x002E6C28 File Offset: 0x002E4E28
		public bool ShouldBeContained(string[] identifiersOrTags, out bool isRestrictionsDefined)
		{
			isRestrictionsDefined = this.containableRestrictions.Any<Identifier>();
			return !this.slotRestrictions.None((ItemContainer.SlotRestrictions s) => s.MatchesItem(this.item)) && (!isRestrictionsDefined || identifiersOrTags.Any((string id) => this.containableRestrictions.Any((Identifier r) => r == id)));
		}

		// Token: 0x06005AA7 RID: 23207 RVA: 0x002E6C7C File Offset: 0x002E4E7C
		public bool ShouldBeContained(Item item, out bool isRestrictionsDefined)
		{
			isRestrictionsDefined = this.containableRestrictions.Any<Identifier>();
			return !this.slotRestrictions.None((ItemContainer.SlotRestrictions s) => s.MatchesItem(item)) && (!isRestrictionsDefined || this.containableRestrictions.Any((Identifier id) => item.Prefab.Identifier == id || item.HasTag(id)));
		}

		// Token: 0x170016D8 RID: 5848
		// (get) Token: 0x06005AA8 RID: 23208 RVA: 0x002E6CE0 File Offset: 0x002E4EE0
		public ImmutableHashSet<Identifier> ContainableItemIdentifiers
		{
			get
			{
				return this.containableItemIdentifiers;
			}
		}

		// Token: 0x170016D9 RID: 5849
		// (get) Token: 0x06005AA9 RID: 23209 RVA: 0x002E6CE8 File Offset: 0x002E4EE8
		public List<RelatedItem> ContainableItems { get; }

		// Token: 0x170016DA RID: 5850
		// (get) Token: 0x06005AAA RID: 23210 RVA: 0x002E6CF0 File Offset: 0x002E4EF0
		public List<RelatedItem> AllSubContainableItems { get; }

		// Token: 0x06005AAB RID: 23211 RVA: 0x002E6CF8 File Offset: 0x002E4EF8
		public ItemContainer(Item item, ContentXElement element) : base(item, element)
		{
			int totalCapacity = this.capacity;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "containable"))
				{
					if (a == "subcontainer")
					{
						totalCapacity += subElement.GetAttributeInt("capacity", 1);
						this.HasSubContainers = true;
					}
				}
				else
				{
					RelatedItem containable = RelatedItem.Load(subElement, false, item.Name);
					if (containable == null)
					{
						string str = "Error in item config \"";
						ContentPath configFilePath = item.ConfigFilePath;
						DebugConsole.ThrowError(str + ((configFilePath != null) ? configFilePath.ToString() : null) + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
					}
					else
					{
						if (this.ContainableItems == null)
						{
							this.ContainableItems = new List<RelatedItem>();
						}
						this.ContainableItems.Add(containable);
					}
				}
			}
			this.Inventory = new ItemInventory(item, this, totalCapacity, this.SlotsPerRow);
			this.ExtraStackSize = element.GetAttributeInt("ExtraStackSize", 0);
			List<ItemContainer.SlotRestrictions> newSlotRestrictions = new List<ItemContainer.SlotRestrictions>(totalCapacity);
			for (int i = 0; i < this.capacity; i++)
			{
				newSlotRestrictions.Add(new ItemContainer.SlotRestrictions(this.maxStackSize, this.ContainableItems, false));
			}
			int subContainerIndex = this.capacity;
			foreach (ContentXElement subElement2 in element.Elements())
			{
				if (!(subElement2.Name.ToString().ToLowerInvariant() != "subcontainer"))
				{
					int subCapacity = subElement2.GetAttributeInt("capacity", 1);
					int subMaxStackSize = subElement2.GetAttributeInt("maxstacksize", this.maxStackSize);
					bool autoInject = subElement2.GetAttributeBool("autoinject", false);
					this.subContainersCanAutoInject = (this.subContainersCanAutoInject || autoInject);
					List<RelatedItem> subContainableItems = new List<RelatedItem>();
					foreach (ContentXElement subSubElement in subElement2.Elements())
					{
						if (!(subSubElement.Name.ToString().ToLowerInvariant() != "containable"))
						{
							RelatedItem containable2 = RelatedItem.Load(subSubElement, false, item.Name);
							if (containable2 == null)
							{
								string str2 = "Error in item config \"";
								ContentPath configFilePath2 = item.ConfigFilePath;
								DebugConsole.ThrowError(str2 + ((configFilePath2 != null) ? configFilePath2.ToString() : null) + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
							}
							else
							{
								subContainableItems.Add(containable2);
								if (this.AllSubContainableItems == null)
								{
									this.AllSubContainableItems = new List<RelatedItem>();
								}
								this.AllSubContainableItems.Add(containable2);
							}
						}
					}
					for (int j = subContainerIndex; j < subContainerIndex + subCapacity; j++)
					{
						newSlotRestrictions.Add(new ItemContainer.SlotRestrictions(subMaxStackSize, subContainableItems, autoInject));
					}
					subContainerIndex += subCapacity;
				}
			}
			this.capacity = totalCapacity;
			this.slotRestrictions = newSlotRestrictions.ToImmutableArray<ItemContainer.SlotRestrictions>();
			this.InitProjSpecific(element);
		}

		// Token: 0x06005AAC RID: 23212 RVA: 0x002E70B0 File Offset: 0x002E52B0
		public void ReloadContainableRestrictions(ContentXElement element)
		{
			int containableIndex = 0;
			foreach (ContentXElement subElement in element.GetChildElements("containable"))
			{
				RelatedItem containable = RelatedItem.Load(subElement, false, this.item.Name);
				if (containable == null)
				{
					DebugConsole.ThrowError("Error when loading containable restrictions for \"" + this.item.Name + "\" - containable with no identifiers.", null, element.ContentPackage, false, false);
				}
				else
				{
					this.ContainableItems[containableIndex] = containable;
					containableIndex++;
					if (containableIndex >= this.ContainableItems.Count)
					{
						break;
					}
				}
			}
			for (int i = 0; i < this.capacity; i++)
			{
				this.slotRestrictions[i].ContainableItems = this.ContainableItems;
			}
			ContentXElement childElement = element.GetChildElement("clearsubcontainerrestrictions");
			ContentXElement contentXElement = null;
			if (childElement != contentXElement)
			{
				for (int j = this.capacity - this.MainContainerCapacity; j < this.capacity; j++)
				{
					this.slotRestrictions[j].MaxStackSize = this.MaxStackSize;
					this.slotIcons[j] = null;
				}
			}
		}

		// Token: 0x06005AAD RID: 23213 RVA: 0x002E71EC File Offset: 0x002E53EC
		public int GetMaxStackSize(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= this.capacity)
			{
				return 0;
			}
			return this.slotRestrictions[slotIndex].MaxStackSize;
		}

		// Token: 0x06005AAE RID: 23214 RVA: 0x002E7210 File Offset: 0x002E5410
		private void InitProjSpecific(ContentXElement element)
		{
			this.slotIcons = new Sprite[this.capacity];
			int currCapacity = this.MainContainerCapacity;
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 8:
						if (text == "sloticon")
						{
							int index = subElement.GetAttributeInt("slotindex", -1);
							Sprite icon = new Sprite(subElement, "", "", false, 1f);
							for (int i = 0; i < this.capacity; i++)
							{
								if (i == index || index == -1)
								{
									this.slotIcons[i] = icon;
								}
							}
						}
						break;
					case 9:
						if (text == "topsprite")
						{
							this.inventoryTopSprite = new Sprite(subElement, "", "", false, 1f);
						}
						break;
					case 10:
						if (text == "backsprite")
						{
							this.inventoryBackSprite = new Sprite(subElement, "", "", false, 1f);
						}
						break;
					case 11:
						break;
					case 12:
					{
						char c = text[0];
						if (c != 'b')
						{
							if (c == 's')
							{
								if (text == "subcontainer")
								{
									int subContainerCapacity = subElement.GetAttributeInt("capacity", 1);
									ContentXElement slotIconElement = subElement.GetChildElement("sloticon");
									ContentXElement contentXElement = null;
									if (slotIconElement != contentXElement)
									{
										Sprite slotIcon = new Sprite(slotIconElement, "", "", false, 1f);
										for (int j = currCapacity; j < currCapacity + subContainerCapacity; j++)
										{
											this.slotIcons[j] = slotIcon;
										}
									}
									currCapacity += subContainerCapacity;
								}
							}
						}
						else if (text == "bottomsprite")
						{
							this.inventoryBottomSprite = new Sprite(subElement, "", "", false, 1f);
						}
						break;
					}
					default:
						if (length != 23)
						{
							if (length == 28)
							{
								if (text == "containedstateindicatorempty")
								{
									this.ContainedStateIndicatorEmpty = new Sprite(subElement, "", "", false, 1f);
								}
							}
						}
						else if (text == "containedstateindicator")
						{
							this.ContainedStateIndicator = new Sprite(subElement, "", "", false, 1f);
						}
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(this.ContainedStateIndicatorStyle))
			{
				if (this.ContainedStateIndicator == null)
				{
					this.IndicatorStyle = GUIStyle.GetComponentStyle("ContainedStateIndicator.Default");
				}
			}
			else
			{
				this.IndicatorStyle = GUIStyle.GetComponentStyle("ContainedStateIndicator." + this.ContainedStateIndicatorStyle);
				if (this.ContainedStateIndicator != null || this.ContainedStateIndicatorEmpty != null)
				{
					DebugConsole.AddWarning("Item \"" + this.item.Name + "\" defines both a contained state indicator style and a custom indicator sprite. Will use the custom sprite...", this.item.Prefab.ContentPackage);
				}
			}
			if (base.GuiFrame == null)
			{
				base.GuiFrame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					CanBeFocused = false
				};
				this.guiCustomComponent = new GUICustomComponent(new RectTransform(Vector2.One, base.GuiFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
				{
					this.Inventory.Draw(spriteBatch, false);
				}, null)
				{
					CanBeFocused = false
				};
				base.GuiFrame.RectTransform.ParentChanged += base.OnGUIParentChanged;
			}
			else
			{
				this.CreateGUI();
			}
			this.containedSpriteDepths = element.GetAttributeFloatArray("containedspritedepths", Array.Empty<float>());
		}

		// Token: 0x06005AAF RID: 23215 RVA: 0x002E764C File Offset: 0x002E584C
		public void OnItemContained(Item containedItem, bool triggerOnInsertedEffects = true)
		{
			int index = this.Inventory.FindIndex(containedItem);
			RelatedItem relatedItem = null;
			if (index >= 0 && index < this.slotRestrictions.Length && this.slotRestrictions[index].ContainableItems != null)
			{
				this.activeContainedItems.RemoveAll((ItemContainer.ActiveContainedItem i) => i.Item == containedItem);
				foreach (RelatedItem containableItem in this.slotRestrictions[index].ContainableItems)
				{
					if (containableItem.MatchesItem(containedItem))
					{
						if (relatedItem == null)
						{
							relatedItem = containableItem;
						}
						foreach (StatusEffect effect in containableItem.StatusEffects)
						{
							ItemContainer.ActiveContainedItem activeContainedItem = new ItemContainer.ActiveContainedItem(containedItem, effect, containableItem.ExcludeBroken, containableItem.ExcludeFullCondition, containableItem.BlameEquipperForDeath);
							this.activeContainedItems.Add(activeContainedItem);
							if (triggerOnInsertedEffects && this.ShouldApplyEffects(activeContainedItem))
							{
								Submarine submarine = this.item.Submarine;
								if ((submarine == null || !submarine.Loading) && !this.initializingLoadedItems && !containedItem.OnInsertedEffectsApplied)
								{
									activeContainedItem.StatusEffect.Apply(ActionType.OnInserted, 1f, this.item, this.targets, null);
								}
							}
						}
						if (triggerOnInsertedEffects)
						{
							containedItem.OnInsertedEffectsApplied = true;
						}
					}
				}
			}
			ItemContainer.ContainedItem containedItemInfo = new ItemContainer.ContainedItem(containedItem, relatedItem != null && relatedItem.Hide, (relatedItem != null) ? relatedItem.ItemPos : null, (relatedItem != null) ? relatedItem.Rotation : 0f);
			this.containedItems.RemoveAll((ItemContainer.ContainedItem d) => d.Item == containedItem);
			if (this.hideItems)
			{
				this.containedItems.Add(containedItemInfo);
			}
			else
			{
				int containedIndex = 0;
				while (containedIndex < this.containedItems.Count && index > this.Inventory.FindIndex(this.containedItems[containedIndex].Item))
				{
					containedIndex++;
				}
				this.containedItems.Insert(containedIndex, containedItemInfo);
			}
			if (this.item.GetComponent<Planter>() != null)
			{
				string str = "MicroInteraction:";
				GameSession gameSession = GameMain.GameSession;
				string text;
				if (gameSession == null)
				{
					text = null;
				}
				else
				{
					GameMode gameMode = gameSession.GameMode;
					text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
				}
				GameAnalyticsManager.AddDesignEvent(str + (text ?? "null") + ":GardeningPlanted:" + containedItem.Prefab.Identifier.ToString());
			}
			bool isActive;
			if (!this.hasSignalConnections && this.activeContainedItems.Count <= 0)
			{
				isActive = this.Inventory.AllItems.Any((Item it) => it.body != null);
			}
			else
			{
				isActive = true;
			}
			this.IsActive = isActive;
			if (this.IsActive)
			{
				Character owner = this.item.GetRootInventoryOwner() as Character;
				if (owner != null)
				{
					if (owner.HasEquippedItem(this.item, null, (InvSlotType slot) => slot.HasFlag(InvSlotType.LeftHand) || slot.HasFlag(InvSlotType.RightHand)))
					{
						this.SetContainedActive(true);
					}
				}
			}
			if (containedItem.FlippedX != this.item.FlippedX)
			{
				containedItem.FlipX(false, false);
			}
			if (containedItem.FlippedY != this.item.FlippedY)
			{
				containedItem.FlipY(false, false);
			}
			this.item.SetContainedItemPositions();
			CharacterHUD.RecreateHudTextsIfFocused(new Item[]
			{
				this.item,
				containedItem
			});
			this.OnContainedItemsChanged.Invoke(this);
		}

		// Token: 0x06005AB0 RID: 23216 RVA: 0x002E7A78 File Offset: 0x002E5C78
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			this.SetContainedItemPositions();
		}

		// Token: 0x06005AB1 RID: 23217 RVA: 0x002E7A80 File Offset: 0x002E5C80
		public void OnItemRemoved(Item containedItem)
		{
			foreach (ItemContainer.ActiveContainedItem activeContainedItem in this.activeContainedItems)
			{
				if (activeContainedItem.Item == containedItem && this.ShouldApplyEffects(activeContainedItem))
				{
					activeContainedItem.StatusEffect.Apply(ActionType.OnRemoved, 1f, this.item, this.targets, null);
				}
			}
			containedItem.OnInsertedEffectsApplied = false;
			this.activeContainedItems.RemoveAll((ItemContainer.ActiveContainedItem i) => i.Item == containedItem);
			this.containedItems.RemoveAll((ItemContainer.ContainedItem i) => i.Item == containedItem);
			this.item.SetContainedItemPositions();
			bool isActive;
			if (!this.hasSignalConnections && this.activeContainedItems.Count <= 0)
			{
				isActive = this.Inventory.AllItems.Any((Item it) => it.body != null);
			}
			else
			{
				isActive = true;
			}
			this.IsActive = isActive;
			CharacterHUD.RecreateHudTextsIfFocused(new Item[]
			{
				this.item,
				containedItem
			});
			this.OnContainedItemsChanged.Invoke(this);
		}

		// Token: 0x06005AB2 RID: 23218 RVA: 0x002E7BD8 File Offset: 0x002E5DD8
		public bool BlameEquipperForDeath()
		{
			return this.activeContainedItems.Any((ItemContainer.ActiveContainedItem c) => c.BlameEquipperForDeath);
		}

		// Token: 0x06005AB3 RID: 23219 RVA: 0x002E7C04 File Offset: 0x002E5E04
		public bool CanBeContained(Item item)
		{
			if (!this.AllowAccessWhenDropped)
			{
				PhysicsBody body = this.item.body;
				if (body != null && body.Enabled)
				{
					return false;
				}
			}
			return this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(item));
		}

		// Token: 0x06005AB4 RID: 23220 RVA: 0x002E7C58 File Offset: 0x002E5E58
		public bool CanBeContained(Item item, int index)
		{
			if (index < 0 || index >= this.capacity)
			{
				return false;
			}
			if (!this.AllowAccessWhenDropped)
			{
				PhysicsBody body = this.item.body;
				if (body != null && body.Enabled)
				{
					return false;
				}
			}
			return this.slotRestrictions[index].MatchesItem(item);
		}

		// Token: 0x06005AB5 RID: 23221 RVA: 0x002E7CA8 File Offset: 0x002E5EA8
		public bool CanBeContained(ItemPrefab itemPrefab)
		{
			return this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(itemPrefab));
		}

		// Token: 0x06005AB6 RID: 23222 RVA: 0x002E7CD9 File Offset: 0x002E5ED9
		public bool CanBeContained(ItemPrefab itemPrefab, int index)
		{
			return index >= 0 && index < this.capacity && this.slotRestrictions[index].MatchesItem(itemPrefab);
		}

		// Token: 0x06005AB7 RID: 23223 RVA: 0x002E7CFC File Offset: 0x002E5EFC
		public bool ContainsItemsWithSameIdentifier(Item item)
		{
			if (item == null)
			{
				return false;
			}
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				if (containedItem.Prefab.Identifier == item.Prefab.Identifier)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06005AB8 RID: 23224 RVA: 0x002E7D70 File Offset: 0x002E5F70
		public override void FlipX(bool relativeToSub)
		{
			base.FlipX(relativeToSub);
			if (this.HideItems)
			{
				return;
			}
			if (this.item.body == null)
			{
				return;
			}
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				if (containedItem.body != null && containedItem.body.Enabled && containedItem.body.Dir != this.item.body.Dir)
				{
					containedItem.FlipX(relativeToSub, false);
				}
			}
		}

		// Token: 0x06005AB9 RID: 23225 RVA: 0x002E7E14 File Offset: 0x002E6014
		public override void Update(float deltaTime, Camera cam)
		{
			if (!string.IsNullOrEmpty(this.SpawnWithId) && !this.alwaysContainedItemsSpawned)
			{
				this.SpawnAlwaysContainedItems();
				this.alwaysContainedItemsSpawned = true;
			}
			if (this.hasSignalConnections)
			{
				float totalConditionValue = 0f;
				float totalConditionPercentage = 0f;
				int totalItems = 0;
				foreach (Item item in this.Inventory.AllItems)
				{
					if (!MathUtils.NearlyEqual(item.Condition, 0f, 0.0001f))
					{
						totalConditionValue += item.Condition;
						totalConditionPercentage += item.ConditionPercentage;
						totalItems++;
					}
				}
				if (!MathUtils.NearlyEqual(totalConditionValue, this.prevTotalConditionValue, 0.0001f))
				{
					this.totalConditionValueString = ((int)totalConditionValue).ToString(CultureInfo.InvariantCulture);
					this.prevTotalConditionValue = totalConditionValue;
				}
				if (!MathUtils.NearlyEqual(totalConditionPercentage, this.prevTotalConditionPercentage, 0.0001f))
				{
					this.totalConditionPercentageString = ((int)totalConditionPercentage).ToString(CultureInfo.InvariantCulture);
					this.prevTotalConditionPercentage = totalConditionPercentage;
				}
				if (totalItems != this.prevTotalItems)
				{
					this.totalItemsString = totalItems.ToString(CultureInfo.InvariantCulture);
					this.prevTotalItems = totalItems;
				}
				this.item.SendSignal(this.totalConditionValueString, "contained_conditions");
				this.item.SendSignal(this.totalConditionPercentageString, "contained_conditions_percentage");
				this.item.SendSignal(this.totalItemsString, "contained_items");
			}
			CharacterInventory ownerInventory = this.item.ParentInventory as CharacterInventory;
			if (ownerInventory != null)
			{
				this.SetContainedItemPositionsIfNeeded();
				if (this.AutoInject || this.subContainersCanAutoInject)
				{
					this.autoInjectCooldown -= deltaTime;
					if (this.autoInjectCooldown <= 0f)
					{
						Entity entity = (ownerInventory != null) ? ownerInventory.Owner : null;
						Character ownerCharacter = entity as Character;
						if (ownerCharacter != null && !ownerCharacter.IsDead && ownerCharacter.HealthPercentage / 100f <= this.AutoInjectThreshold && ownerCharacter.HasEquippedItem(this.item, null, null))
						{
							if (this.AutoInject)
							{
								this.Inventory.AllItemsMod.ForEach(delegate(Item i)
								{
									base.<Update>g__Inject|1(i);
								});
							}
							else
							{
								Action<Item> <>9__2;
								for (int j = 0; j < this.slotRestrictions.Length; j++)
								{
									if (this.slotRestrictions[j].AutoInject)
									{
										IEnumerable<Item> itemsAt = this.Inventory.GetItemsAt(j);
										Action<Item> action;
										if ((action = <>9__2) == null)
										{
											action = (<>9__2 = delegate(Item i)
											{
												base.<Update>g__Inject|1(i);
											});
										}
										itemsAt.ForEachMod(action);
									}
								}
							}
							this.autoInjectCooldown = 1f;
						}
					}
				}
			}
			else if (this.item.body != null && this.item.body.Enabled)
			{
				if (this.item.body.FarseerBody.Awake)
				{
					this.SetContainedItemPositionsIfNeeded();
				}
			}
			else if (!this.hasSignalConnections && this.activeContainedItems.Count == 0)
			{
				this.IsActive = false;
				return;
			}
			foreach (ItemContainer.ActiveContainedItem activeContainedItem in this.activeContainedItems)
			{
				if (this.ShouldApplyEffects(activeContainedItem))
				{
					StatusEffect effect = activeContainedItem.StatusEffect;
					effect.Apply(ActionType.OnActive, deltaTime, this.item, this.targets, null);
					effect.Apply(ActionType.OnContaining, deltaTime, this.item, this.targets, null);
					Wearable component = this.item.GetComponent<Wearable>();
					if (component != null && component.IsActive)
					{
						effect.Apply(ActionType.OnWearing, deltaTime, this.item, this.targets, null);
					}
				}
			}
		}

		// Token: 0x06005ABA RID: 23226 RVA: 0x002E8224 File Offset: 0x002E6424
		private bool ShouldApplyEffects(ItemContainer.ActiveContainedItem activeContainedItem)
		{
			Item contained = activeContainedItem.Item;
			if (activeContainedItem.ExcludeBroken && contained.Condition <= 0f)
			{
				return false;
			}
			if (activeContainedItem.ExcludeFullCondition && contained.IsFullCondition)
			{
				return false;
			}
			StatusEffect effect = activeContainedItem.StatusEffect;
			this.targets.Clear();
			if (effect.HasTargetType(StatusEffect.TargetType.This))
			{
				this.targets.AddRange(this.item.AllPropertyObjects);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Contained))
			{
				this.targets.AddRange(contained.AllPropertyObjects);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Character))
			{
				Inventory parentInventory = this.item.ParentInventory;
				Character character = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
				if (character != null)
				{
					this.targets.Add(character);
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.NearbyItems) || effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
			{
				effect.AddNearbyTargets(this.item.WorldPosition, this.targets);
			}
			return true;
		}

		// Token: 0x06005ABB RID: 23227 RVA: 0x002E8314 File Offset: 0x002E6514
		private void SetContainedItemPositionsIfNeeded()
		{
			if (Vector2.DistanceSquared(this.prevContainedItemRefreshPosition, this.item.Position) <= 10f)
			{
				float num = this.prevContainedItemRefreshRotation;
				PhysicsBody body = this.item.body;
				if (Math.Abs((num - ((body != null) ? new float?(body.Rotation) : null)) ?? this.item.RotationRad) <= 0.01f)
				{
					return;
				}
			}
			this.SetContainedItemPositions();
			this.prevContainedItemRefreshPosition = this.item.Position;
			PhysicsBody body2 = this.item.body;
			this.prevContainedItemRefreshRotation = ((body2 != null) ? body2.Rotation : this.item.RotationRad);
		}

		// Token: 0x06005ABC RID: 23228 RVA: 0x002E83F4 File Offset: 0x002E65F4
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			if (this.IsActive)
			{
				this.Update(deltaTime, cam);
			}
		}

		// Token: 0x06005ABD RID: 23229 RVA: 0x002E8406 File Offset: 0x002E6606
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			return this.IsAccessible() && base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x06005ABE RID: 23230 RVA: 0x002E841C File Offset: 0x002E661C
		public bool IsAccessible()
		{
			if (!this.AllowAccess)
			{
				return false;
			}
			if (this.AccessOnlyWhenBroken)
			{
				Screen selected = Screen.Selected;
				return (selected != null && selected.IsEditor) || this.item.Condition <= 0f;
			}
			return true;
		}

		// Token: 0x06005ABF RID: 23231 RVA: 0x002E8468 File Offset: 0x002E6668
		public override bool Select(Character character)
		{
			if (this.item.Container != null)
			{
				return false;
			}
			if (!this.IsAccessible())
			{
				return false;
			}
			if (this.AutoInteractWithContained && character.SelectedItem == null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					foreach (Item contained in this.Inventory.AllItems)
					{
						if (this.CanAutoInteractWithContained(contained) && contained.TryInteract(character, false, false, false))
						{
							character.FocusedItem = contained;
							return false;
						}
					}
				}
			}
			AbilityItemContainer abilityItem = new AbilityItemContainer(this.item);
			character.CheckTalents(AbilityEffectType.OnOpenItemContainer, abilityItem);
			Inventory parentInventory = this.item.ParentInventory;
			return ((parentInventory != null) ? parentInventory.Owner : null) != character && base.Select(character);
		}

		// Token: 0x06005AC0 RID: 23232 RVA: 0x002E854C File Offset: 0x002E674C
		public override bool Pick(Character picker)
		{
			if (!this.IsAccessible())
			{
				return false;
			}
			if (this.AutoInteractWithContained)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					foreach (Item contained in this.Inventory.AllItems)
					{
						if (this.CanAutoInteractWithContained(contained) && contained.TryInteract(picker, false, false, false))
						{
							picker.FocusedItem = contained;
							return true;
						}
					}
				}
			}
			this.IsActive = true;
			return picker != null;
		}

		// Token: 0x06005AC1 RID: 23233 RVA: 0x002E85E8 File Offset: 0x002E67E8
		public override bool Combine(Item item, Character user)
		{
			if (!this.AllowDragAndDrop && user != null)
			{
				return false;
			}
			if (!this.slotRestrictions.Any((ItemContainer.SlotRestrictions s) => s.MatchesItem(item)))
			{
				return false;
			}
			if (user != null && !user.CanAccessInventory(this.Inventory, CharacterInventory.AccessLevel.AllowBotsAndPets))
			{
				return false;
			}
			if (base.Item.GetComponent<GeneticMaterial>() != null)
			{
				return false;
			}
			if (this.Inventory.TryPutItem(item, user, null, true, false, true))
			{
				this.IsActive = true;
				if (this.hideItems && item.body != null)
				{
					item.body.Enabled = false;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06005AC2 RID: 23234 RVA: 0x002E8695 File Offset: 0x002E6895
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.IsActive = true;
			this.SetContainedActive(false);
		}

		// Token: 0x06005AC3 RID: 23235 RVA: 0x002E86A8 File Offset: 0x002E68A8
		public override void Equip(Character character)
		{
			this.IsActive = true;
			if (character != null)
			{
				if (character.HasEquippedItem(this.item, null, (InvSlotType slot) => slot.HasFlag(InvSlotType.LeftHand) || slot.HasFlag(InvSlotType.RightHand)))
				{
					this.SetContainedActive(true);
					return;
				}
			}
			this.SetContainedActive(false);
		}

		// Token: 0x06005AC4 RID: 23236 RVA: 0x002E8704 File Offset: 0x002E6904
		private bool CanAutoInteractWithContained(Item containedItem)
		{
			return this.AutoInteractWithContained && (this.autoInteractWithContainedTags.None(null) || this.autoInteractWithContainedTags.Any((Identifier t) => containedItem.HasTag(t)));
		}

		// Token: 0x06005AC5 RID: 23237 RVA: 0x002E8750 File Offset: 0x002E6950
		private void SetContainedActive(bool active)
		{
			if (this.ContainableItems != null)
			{
				if (this.ContainableItems.Any((RelatedItem c) => c.SetActive))
				{
					goto IL_69;
				}
			}
			if (this.AllSubContainableItems != null)
			{
				if (this.AllSubContainableItems.Any((RelatedItem c) => c.SetActive))
				{
					goto IL_69;
				}
			}
			return;
			IL_69:
			foreach (Item containedItem in this.Inventory.AllItems)
			{
				RelatedItem containableItem = this.FindContainableItem(containedItem);
				if (containableItem != null && containableItem.SetActive)
				{
					foreach (ItemComponent ic in containedItem.Components)
					{
						ic.IsActive = active;
					}
					if (containedItem.body != null)
					{
						containedItem.body.Enabled = active;
						if (active)
						{
							containedItem.body.PhysEnabled = false;
						}
					}
				}
			}
			if (active)
			{
				this.FlipX(false);
			}
		}

		// Token: 0x06005AC6 RID: 23238 RVA: 0x002E8888 File Offset: 0x002E6A88
		private RelatedItem FindContainableItem(Item item)
		{
			int index = this.Inventory.FindIndex(item);
			if (index == -1)
			{
				return null;
			}
			ItemContainer.SlotRestrictions slotRestrictions = this.slotRestrictions[index];
			if (slotRestrictions == null)
			{
				return null;
			}
			List<RelatedItem> containableItems = slotRestrictions.ContainableItems;
			if (containableItems == null)
			{
				return null;
			}
			return containableItems.FirstOrDefault((RelatedItem ci) => ci.MatchesItem(item));
		}

		// Token: 0x06005AC7 RID: 23239 RVA: 0x002E88E8 File Offset: 0x002E6AE8
		public int? FindSuitableSubContainerIndex(Identifier itemTagOrIdentifier)
		{
			for (int i = 0; i < this.slotRestrictions.Length; i++)
			{
				if (this.slotRestrictions[i].MatchesItem(itemTagOrIdentifier))
				{
					return new int?(i);
				}
			}
			return null;
		}

		// Token: 0x06005AC8 RID: 23240 RVA: 0x002E8930 File Offset: 0x002E6B30
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if ((name == "activate" || name == "use" || name == "trigger_in") && signal.value != "0")
			{
				this.item.Use(1f, signal.sender, null, null, null);
			}
		}

		// Token: 0x06005AC9 RID: 23241 RVA: 0x002E8998 File Offset: 0x002E6B98
		public void SetContainedItemPositions()
		{
			if (this.containedItems.Count == 0)
			{
				return;
			}
			Item rootContainer = this.item.RootContainer;
			PhysicsBody rootBody = ((rootContainer != null) ? rootContainer.body : null) ?? this.item.body;
			Vector2 transformedItemIntervalHorizontal;
			Vector2 transformedItemIntervalVertical;
			bool flippedX;
			bool flippedY;
			Vector2 transformedItemPos = this.GetContainedPosition(false, out transformedItemIntervalHorizontal, out transformedItemIntervalVertical, out flippedX, out flippedY);
			int i = 0;
			Vector2 currentItemPos = transformedItemPos;
			foreach (ItemContainer.ContainedItem contained in this.containedItems)
			{
				Vector2 itemPos = currentItemPos;
				if (contained.ItemPos != null)
				{
					Vector2 pos = contained.ItemPos.Value;
					if (this.item.body != null)
					{
						Matrix transform = Matrix.CreateRotationZ(this.item.body.Rotation);
						pos.X *= rootBody.Dir;
						itemPos = Vector2.Transform(pos, transform) + this.item.body.Position;
					}
					else
					{
						itemPos = pos;
						if (flippedX)
						{
							itemPos.X = -itemPos.X;
							itemPos.X += (float)this.item.Rect.Width;
						}
						if (flippedY)
						{
							itemPos.Y = -itemPos.Y;
							itemPos.Y -= (float)this.item.Rect.Height;
						}
						itemPos += new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y);
						if (Math.Abs(this.item.RotationRad) > 0.01f)
						{
							Matrix transform2 = Matrix.CreateRotationZ(this.item.RotationRad);
							itemPos = Vector2.Transform(itemPos - this.item.Position, transform2) + this.item.Position;
						}
					}
				}
				if (contained.Item.body != null)
				{
					try
					{
						Vector2 simPos = ConvertUnits.ToSimUnits(itemPos);
						float rotation = this.itemRotation;
						if (contained.Rotation != 0f)
						{
							rotation = MathHelper.ToRadians(contained.Rotation);
						}
						if (this.item.body != null)
						{
							rotation *= rootBody.Dir;
							rotation += this.item.body.Rotation;
						}
						else
						{
							if (flippedX ^ flippedY)
							{
								rotation = -rotation;
							}
							rotation += -this.item.RotationRad;
						}
						contained.Item.body.FarseerBody.SetTransformIgnoreContacts(ref simPos, rotation);
						contained.Item.body.UpdateDrawPosition(false);
					}
					catch (Exception e)
					{
						DebugConsole.Log("SetTransformIgnoreContacts threw an exception in SetContainedItemPositions (" + e.Message + ")\n" + e.StackTrace.CleanupStackTrace());
						GameAnalyticsManager.AddErrorEventOnce("ItemContainer.SetContainedItemPositions.InvalidPosition:" + contained.Item.Name, GameAnalyticsManager.ErrorSeverity.Error, "SetTransformIgnoreContacts threw an exception in SetContainedItemPositions (" + e.Message + ")\n" + e.StackTrace.CleanupStackTrace());
					}
					contained.Item.body.Submarine = this.item.Submarine;
				}
				contained.Item.Rect = new Rectangle((int)(itemPos.X - (float)contained.Item.Rect.Width / 2f), (int)(itemPos.Y + (float)contained.Item.Rect.Height / 2f), contained.Item.Rect.Width, contained.Item.Rect.Height);
				contained.Item.Submarine = this.item.Submarine;
				contained.Item.CurrentHull = this.item.CurrentHull;
				contained.Item.SetContainedItemPositions();
				foreach (LightComponent lightComponent in contained.Item.GetComponents<LightComponent>())
				{
					lightComponent.SetLightSourceTransform();
				}
				i++;
				if (Math.Abs(this.ItemInterval.X) > 0.001f && Math.Abs(this.ItemInterval.Y) > 0.001f)
				{
					currentItemPos += transformedItemIntervalHorizontal;
					if (i % this.ItemsPerRow == 0)
					{
						currentItemPos = transformedItemPos;
						currentItemPos += transformedItemIntervalVertical * (float)(i / this.ItemsPerRow);
					}
				}
				else
				{
					currentItemPos += transformedItemIntervalHorizontal + transformedItemIntervalVertical;
				}
			}
		}

		// Token: 0x06005ACA RID: 23242 RVA: 0x002E8E80 File Offset: 0x002E7080
		private Vector2 GetContainedPosition(bool drawPosition, out Vector2 transformedItemIntervalHorizontal, out Vector2 transformedItemIntervalVertical, out bool flippedX, out bool flippedY)
		{
			Vector2 transformedItemPos = this.ItemPos * this.item.Scale;
			Vector2 transformedItemInterval = this.ItemInterval * this.item.Scale;
			transformedItemIntervalHorizontal = new Vector2(transformedItemInterval.X, 0f);
			transformedItemIntervalVertical = new Vector2(0f, transformedItemInterval.Y);
			if (this.item.RootContainer != null)
			{
				flippedX = (this.item.RootContainer.FlippedX && this.item.RootContainer.Prefab.CanSpriteFlipX);
				flippedY = (this.item.RootContainer.FlippedY && this.item.RootContainer.Prefab.CanSpriteFlipY);
			}
			else
			{
				flippedX = (this.item.FlippedX && this.item.Prefab.CanSpriteFlipX);
				flippedY = (this.item.FlippedY && this.item.Prefab.CanSpriteFlipY);
			}
			Item rootContainer = this.item.RootContainer;
			PhysicsBody rootBody = ((rootContainer != null) ? rootContainer.body : null) ?? this.item.body;
			bool bodyFlipped = rootBody != null && rootBody.Dir == -1f;
			if (this.ItemPos == Vector2.Zero && this.ItemInterval == Vector2.Zero && !drawPosition)
			{
				transformedItemPos = this.item.Position;
			}
			else if (this.item.body == null)
			{
				if (flippedX)
				{
					transformedItemPos.X = -transformedItemPos.X;
					transformedItemPos.X += (float)this.item.Rect.Width;
					transformedItemInterval.X = -transformedItemInterval.X;
					transformedItemIntervalHorizontal.X = -transformedItemIntervalHorizontal.X;
				}
				if (flippedY)
				{
					transformedItemPos.Y = -transformedItemPos.Y;
					transformedItemPos.Y -= (float)this.item.Rect.Height;
					transformedItemInterval.Y = -transformedItemInterval.Y;
					transformedItemIntervalVertical.Y = -transformedItemIntervalVertical.Y;
				}
				transformedItemPos += new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y);
				if (drawPosition && this.item.Submarine != null)
				{
					transformedItemPos += this.item.Submarine.DrawPosition;
				}
				if (Math.Abs(this.item.RotationRad) > 0.01f)
				{
					Matrix transform = Matrix.CreateRotationZ(-this.item.RotationRad);
					transformedItemPos = (drawPosition ? (Vector2.Transform(transformedItemPos - this.item.DrawPosition, transform) + this.item.DrawPosition) : (Vector2.Transform(transformedItemPos - this.item.Position, transform) + this.item.Position));
					transformedItemIntervalVertical = Vector2.Transform(transformedItemIntervalVertical, transform);
					transformedItemIntervalHorizontal = Vector2.Transform(transformedItemIntervalHorizontal, transform);
				}
			}
			else
			{
				Holdable component = this.item.GetComponent<Holdable>();
				if (component != null && component.Attachable)
				{
					transformedItemPos -= this.item.Rect.Size.FlipY().ToVector2() / 2f;
				}
				Matrix transform2 = Matrix.CreateRotationZ(drawPosition ? this.item.body.DrawRotation : this.item.body.Rotation);
				if (bodyFlipped)
				{
					transformedItemPos.X = -transformedItemPos.X;
					transformedItemInterval.X = -transformedItemInterval.X;
					transformedItemIntervalHorizontal.X = -transformedItemIntervalHorizontal.X;
				}
				transformedItemPos = Vector2.Transform(transformedItemPos, transform2);
				transformedItemIntervalVertical = Vector2.Transform(transformedItemIntervalVertical, transform2);
				transformedItemIntervalHorizontal = Vector2.Transform(transformedItemIntervalHorizontal, transform2);
				transformedItemPos += (drawPosition ? this.item.body.DrawPosition : this.item.body.Position);
			}
			return transformedItemPos;
		}

		// Token: 0x06005ACB RID: 23243 RVA: 0x002E92B4 File Offset: 0x002E74B4
		public override void OnItemLoaded()
		{
			this.Inventory.AllowSwappingContainedItems = this.AllowSwappingContainedItems;
			this.containableItemIdentifiers = this.slotRestrictions.SelectMany(delegate(ItemContainer.SlotRestrictions s)
			{
				List<RelatedItem> containableItems = s.ContainableItems;
				IEnumerable<Identifier> enumerable;
				if (containableItems == null)
				{
					enumerable = null;
				}
				else
				{
					enumerable = containableItems.SelectMany((RelatedItem ri) => ri.Identifiers);
				}
				return enumerable ?? Enumerable.Empty<Identifier>();
			}).ToImmutableHashSet<Identifier>();
			List<Connection> connections = this.item.Connections;
			bool flag;
			if (connections == null)
			{
				flag = false;
			}
			else
			{
				flag = connections.Any(delegate(Connection c)
				{
					string name = c.Name;
					return name == "contained_conditions" || name == "contained_conditions_percentage" || name == "contained_items";
				});
			}
			this.hasSignalConnections = flag;
			if (this.item.Submarine == null || !this.item.Submarine.Loading)
			{
				this.SpawnAlwaysContainedItems();
			}
		}

		// Token: 0x06005ACC RID: 23244 RVA: 0x002E9370 File Offset: 0x002E7570
		public override void OnMapLoaded()
		{
			if (this.itemIds != null)
			{
				this.initializingLoadedItems = true;
				ushort i = 0;
				while ((int)i < this.itemIds.Length)
				{
					if ((int)i >= this.Inventory.Capacity)
					{
						this.Inventory.TryPutItem(this.item, null, null, false, false, true);
					}
					else
					{
						foreach (ushort id in this.itemIds[(int)i])
						{
							Item item = Entity.FindEntityByID(id) as Item;
							if (item != null)
							{
								this.Inventory.TryPutItem(item, (int)i, false, false, null, false, true, true);
							}
						}
					}
					i += 1;
				}
				this.initializingLoadedItems = false;
				this.itemIds = null;
			}
			Submarine submarine = this.item.Submarine;
			if (((submarine != null) ? submarine.Info : null) != null && (this.item.Submarine.Info.IsOutpost || this.item.Submarine.Info.IsRuin))
			{
				if (this.SpawnWithId.Length > 0)
				{
					this.IsActive = true;
				}
			}
			else
			{
				this.SpawnAlwaysContainedItems();
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x06005ACD RID: 23245 RVA: 0x002E94AC File Offset: 0x002E76AC
		private void SpawnAlwaysContainedItems()
		{
			if (this.SpawnWithId.Length > 0 && (this.item.Condition > 0f || this.SpawnWithIdWhenBroken))
			{
				string[] splitIds = this.SpawnWithId.Split(',', StringSplitOptions.None);
				string[] array = splitIds;
				for (int i = 0; i < array.Length; i++)
				{
					string id = array[i];
					ItemPrefab prefab = ItemPrefab.Prefabs.Find((ItemPrefab m) => m.Identifier == id);
					if (prefab != null && this.Inventory != null && this.Inventory.CanProbablyBePut(prefab, null, null))
					{
						if (Screen.Selected != GameMain.SubEditorScreen && (Entity.Spawner == null || Entity.Spawner.Removed) && GameMain.NetworkMember == null)
						{
							Item spawnedItem = new Item(prefab, Vector2.Zero, null, 0, true);
							this.Inventory.TryPutItem(spawnedItem, null, spawnedItem.AllowedSlots, false, false, true);
							this.alwaysContainedItemsSpawned = true;
						}
						else
						{
							this.IsActive = true;
							EntitySpawner spawner = Entity.Spawner;
							if (spawner != null)
							{
								spawner.AddItemToSpawnQueue(prefab, this.Inventory, null, null, delegate(Item item)
								{
									this.alwaysContainedItemsSpawned = true;
								}, false, false, InvSlotType.None);
							}
						}
					}
				}
			}
		}

		// Token: 0x06005ACE RID: 23246 RVA: 0x002E960F File Offset: 0x002E780F
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x06005ACF RID: 23247 RVA: 0x002E9614 File Offset: 0x002E7814
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.inventoryTopSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.inventoryBackSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			Sprite sprite3 = this.inventoryBottomSprite;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			Sprite containedStateIndicator = this.ContainedStateIndicator;
			if (containedStateIndicator != null)
			{
				containedStateIndicator.Remove();
			}
			if (SubEditorScreen.IsSubEditor())
			{
				this.Inventory.DeleteAllItems();
				return;
			}
			if (!Submarine.Unloading)
			{
				this.Inventory.AllItemsMod.ForEach(delegate(Item it)
				{
					it.Drop(null, true, true);
				});
			}
		}

		// Token: 0x06005AD0 RID: 23248 RVA: 0x002E96B4 File Offset: 0x002E78B4
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			string containedString = componentElement.GetAttributeString("contained", "");
			string[] itemIdStrings = containedString.Split(',', StringSplitOptions.None);
			this.itemIds = new List<ushort>[itemIdStrings.Length];
			for (int i = 0; i < itemIdStrings.Length; i++)
			{
				List<ushort>[] array = this.itemIds;
				int num = i;
				if (array[num] == null)
				{
					array[num] = new List<ushort>();
				}
				foreach (string idStr in itemIdStrings[i].Split(';', StringSplitOptions.None))
				{
					int id;
					if (int.TryParse(idStr, out id))
					{
						this.itemIds[i].Add(idRemap.GetOffsetId(id));
					}
				}
			}
			this.ExtraStackSize = componentElement.GetAttributeInt("ExtraStackSize", 0);
		}

		// Token: 0x06005AD1 RID: 23249 RVA: 0x002E9778 File Offset: 0x002E7978
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			string[] itemIdStrings = new string[this.Inventory.Capacity];
			for (int i = 0; i < this.Inventory.Capacity; i++)
			{
				IEnumerable<Item> items = this.Inventory.GetItemsAt(i);
				itemIdStrings[i] = string.Join<string>(';', from it in items
				select it.ID.ToString());
			}
			componentElement.Add(new XAttribute("contained", string.Join(',', itemIdStrings)));
			componentElement.Add(new XAttribute("ExtraStackSize", this.ExtraStackSize));
			return componentElement;
		}

		// Token: 0x04002E07 RID: 11783
		private Sprite inventoryTopSprite;

		// Token: 0x04002E08 RID: 11784
		private Sprite inventoryBackSprite;

		// Token: 0x04002E09 RID: 11785
		private Sprite inventoryBottomSprite;

		// Token: 0x04002E0A RID: 11786
		private GUICustomComponent guiCustomComponent;

		// Token: 0x04002E0B RID: 11787
		private float[] containedSpriteDepths;

		// Token: 0x04002E0C RID: 11788
		private Sprite[] slotIcons;

		// Token: 0x04002E19 RID: 11801
		public readonly NamedEvent<ItemContainer> OnContainedItemsChanged = new NamedEvent<ItemContainer>();

		// Token: 0x04002E1A RID: 11802
		private bool alwaysContainedItemsSpawned;

		// Token: 0x04002E1B RID: 11803
		public readonly ItemInventory Inventory;

		// Token: 0x04002E1C RID: 11804
		private readonly List<ItemContainer.ActiveContainedItem> activeContainedItems = new List<ItemContainer.ActiveContainedItem>();

		// Token: 0x04002E1D RID: 11805
		private readonly List<ItemContainer.ContainedItem> containedItems = new List<ItemContainer.ContainedItem>();

		// Token: 0x04002E1E RID: 11806
		private List<ushort>[] itemIds;

		// Token: 0x04002E1F RID: 11807
		private int capacity;

		// Token: 0x04002E21 RID: 11809
		private int maxStackSize;

		// Token: 0x04002E22 RID: 11810
		private bool hideItems;

		// Token: 0x04002E2E RID: 11822
		private ImmutableHashSet<Identifier> autoInteractWithContainedTags = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x04002E33 RID: 11827
		private readonly HashSet<Identifier> containableRestrictions = new HashSet<Identifier>();

		// Token: 0x04002E35 RID: 11829
		private float itemRotation;

		// Token: 0x04002E3B RID: 11835
		private readonly ImmutableArray<ItemContainer.SlotRestrictions> slotRestrictions;

		// Token: 0x04002E3C RID: 11836
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04002E3D RID: 11837
		private float prevContainedItemRefreshRotation;

		// Token: 0x04002E3E RID: 11838
		private Vector2 prevContainedItemRefreshPosition;

		// Token: 0x04002E3F RID: 11839
		private float autoInjectCooldown = 1f;

		// Token: 0x04002E40 RID: 11840
		private const float AutoInjectInterval = 1f;

		// Token: 0x04002E41 RID: 11841
		private bool subContainersCanAutoInject;

		// Token: 0x04002E42 RID: 11842
		private ImmutableHashSet<Identifier> containableItemIdentifiers;

		// Token: 0x04002E45 RID: 11845
		public readonly bool HasSubContainers;

		// Token: 0x04002E46 RID: 11846
		public bool hasSignalConnections;

		// Token: 0x04002E47 RID: 11847
		private string totalConditionValueString = "";

		// Token: 0x04002E48 RID: 11848
		private string totalConditionPercentageString = "";

		// Token: 0x04002E49 RID: 11849
		private string totalItemsString = "";

		// Token: 0x04002E4A RID: 11850
		private float prevTotalConditionValue;

		// Token: 0x04002E4B RID: 11851
		private float prevTotalConditionPercentage;

		// Token: 0x04002E4C RID: 11852
		private int prevTotalItems;

		// Token: 0x04002E4D RID: 11853
		private bool initializingLoadedItems;

		// Token: 0x020013C2 RID: 5058
		private readonly struct ActiveContainedItem : IEquatable<ItemContainer.ActiveContainedItem>
		{
			// Token: 0x0600985C RID: 39004 RVA: 0x003DD947 File Offset: 0x003DBB47
			public ActiveContainedItem(Item Item, StatusEffect StatusEffect, bool ExcludeBroken, bool ExcludeFullCondition, bool BlameEquipperForDeath)
			{
				this.Item = Item;
				this.StatusEffect = StatusEffect;
				this.ExcludeBroken = ExcludeBroken;
				this.ExcludeFullCondition = ExcludeFullCondition;
				this.BlameEquipperForDeath = BlameEquipperForDeath;
			}

			// Token: 0x17001D27 RID: 7463
			// (get) Token: 0x0600985D RID: 39005 RVA: 0x003DD96E File Offset: 0x003DBB6E
			// (set) Token: 0x0600985E RID: 39006 RVA: 0x003DD976 File Offset: 0x003DBB76
			public Item Item { get; set; }

			// Token: 0x17001D28 RID: 7464
			// (get) Token: 0x0600985F RID: 39007 RVA: 0x003DD97F File Offset: 0x003DBB7F
			// (set) Token: 0x06009860 RID: 39008 RVA: 0x003DD987 File Offset: 0x003DBB87
			public StatusEffect StatusEffect { get; set; }

			// Token: 0x17001D29 RID: 7465
			// (get) Token: 0x06009861 RID: 39009 RVA: 0x003DD990 File Offset: 0x003DBB90
			// (set) Token: 0x06009862 RID: 39010 RVA: 0x003DD998 File Offset: 0x003DBB98
			public bool ExcludeBroken { get; set; }

			// Token: 0x17001D2A RID: 7466
			// (get) Token: 0x06009863 RID: 39011 RVA: 0x003DD9A1 File Offset: 0x003DBBA1
			// (set) Token: 0x06009864 RID: 39012 RVA: 0x003DD9A9 File Offset: 0x003DBBA9
			public bool ExcludeFullCondition { get; set; }

			// Token: 0x17001D2B RID: 7467
			// (get) Token: 0x06009865 RID: 39013 RVA: 0x003DD9B2 File Offset: 0x003DBBB2
			// (set) Token: 0x06009866 RID: 39014 RVA: 0x003DD9BA File Offset: 0x003DBBBA
			public bool BlameEquipperForDeath { get; set; }

			// Token: 0x06009867 RID: 39015 RVA: 0x003DD9C4 File Offset: 0x003DBBC4
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ActiveContainedItem");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009868 RID: 39016 RVA: 0x003DDA10 File Offset: 0x003DBC10
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", StatusEffect = ");
				builder.Append(this.StatusEffect);
				builder.Append(", ExcludeBroken = ");
				builder.Append(this.ExcludeBroken.ToString());
				builder.Append(", ExcludeFullCondition = ");
				builder.Append(this.ExcludeFullCondition.ToString());
				builder.Append(", BlameEquipperForDeath = ");
				builder.Append(this.BlameEquipperForDeath.ToString());
				return true;
			}

			// Token: 0x06009869 RID: 39017 RVA: 0x003DDAC5 File Offset: 0x003DBCC5
			[CompilerGenerated]
			public static bool operator !=(ItemContainer.ActiveContainedItem left, ItemContainer.ActiveContainedItem right)
			{
				return !(left == right);
			}

			// Token: 0x0600986A RID: 39018 RVA: 0x003DDAD1 File Offset: 0x003DBCD1
			[CompilerGenerated]
			public static bool operator ==(ItemContainer.ActiveContainedItem left, ItemContainer.ActiveContainedItem right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600986B RID: 39019 RVA: 0x003DDADC File Offset: 0x003DBCDC
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<StatusEffect>.Default.GetHashCode(this.<StatusEffect>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ExcludeBroken>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ExcludeFullCondition>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<BlameEquipperForDeath>k__BackingField);
			}

			// Token: 0x0600986C RID: 39020 RVA: 0x003DDB55 File Offset: 0x003DBD55
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemContainer.ActiveContainedItem && this.Equals((ItemContainer.ActiveContainedItem)obj);
			}

			// Token: 0x0600986D RID: 39021 RVA: 0x003DDB70 File Offset: 0x003DBD70
			[CompilerGenerated]
			public bool Equals(ItemContainer.ActiveContainedItem other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<StatusEffect>.Default.Equals(this.<StatusEffect>k__BackingField, other.<StatusEffect>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ExcludeBroken>k__BackingField, other.<ExcludeBroken>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ExcludeFullCondition>k__BackingField, other.<ExcludeFullCondition>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<BlameEquipperForDeath>k__BackingField, other.<BlameEquipperForDeath>k__BackingField);
			}

			// Token: 0x0600986E RID: 39022 RVA: 0x003DDBF5 File Offset: 0x003DBDF5
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out StatusEffect StatusEffect, out bool ExcludeBroken, out bool ExcludeFullCondition, out bool BlameEquipperForDeath)
			{
				Item = this.Item;
				StatusEffect = this.StatusEffect;
				ExcludeBroken = this.ExcludeBroken;
				ExcludeFullCondition = this.ExcludeFullCondition;
				BlameEquipperForDeath = this.BlameEquipperForDeath;
			}
		}

		// Token: 0x020013C3 RID: 5059
		private readonly struct ContainedItem : IEquatable<ItemContainer.ContainedItem>
		{
			// Token: 0x0600986F RID: 39023 RVA: 0x003DDC21 File Offset: 0x003DBE21
			public ContainedItem(Item Item, bool Hide, Vector2? ItemPos, float Rotation)
			{
				this.Item = Item;
				this.Hide = Hide;
				this.ItemPos = ItemPos;
				this.Rotation = Rotation;
			}

			// Token: 0x17001D2C RID: 7468
			// (get) Token: 0x06009870 RID: 39024 RVA: 0x003DDC40 File Offset: 0x003DBE40
			// (set) Token: 0x06009871 RID: 39025 RVA: 0x003DDC48 File Offset: 0x003DBE48
			public Item Item { get; set; }

			// Token: 0x17001D2D RID: 7469
			// (get) Token: 0x06009872 RID: 39026 RVA: 0x003DDC51 File Offset: 0x003DBE51
			// (set) Token: 0x06009873 RID: 39027 RVA: 0x003DDC59 File Offset: 0x003DBE59
			public bool Hide { get; set; }

			// Token: 0x17001D2E RID: 7470
			// (get) Token: 0x06009874 RID: 39028 RVA: 0x003DDC62 File Offset: 0x003DBE62
			// (set) Token: 0x06009875 RID: 39029 RVA: 0x003DDC6A File Offset: 0x003DBE6A
			public Vector2? ItemPos { get; set; }

			// Token: 0x17001D2F RID: 7471
			// (get) Token: 0x06009876 RID: 39030 RVA: 0x003DDC73 File Offset: 0x003DBE73
			// (set) Token: 0x06009877 RID: 39031 RVA: 0x003DDC7B File Offset: 0x003DBE7B
			public float Rotation { get; set; }

			// Token: 0x06009878 RID: 39032 RVA: 0x003DDC84 File Offset: 0x003DBE84
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ContainedItem");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009879 RID: 39033 RVA: 0x003DDCD0 File Offset: 0x003DBED0
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Item = ");
				builder.Append(this.Item);
				builder.Append(", Hide = ");
				builder.Append(this.Hide.ToString());
				builder.Append(", ItemPos = ");
				builder.Append(this.ItemPos.ToString());
				builder.Append(", Rotation = ");
				builder.Append(this.Rotation.ToString());
				return true;
			}

			// Token: 0x0600987A RID: 39034 RVA: 0x003DDD6C File Offset: 0x003DBF6C
			[CompilerGenerated]
			public static bool operator !=(ItemContainer.ContainedItem left, ItemContainer.ContainedItem right)
			{
				return !(left == right);
			}

			// Token: 0x0600987B RID: 39035 RVA: 0x003DDD78 File Offset: 0x003DBF78
			[CompilerGenerated]
			public static bool operator ==(ItemContainer.ContainedItem left, ItemContainer.ContainedItem right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600987C RID: 39036 RVA: 0x003DDD84 File Offset: 0x003DBF84
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<Item>.Default.GetHashCode(this.<Item>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Hide>k__BackingField)) * -1521134295 + EqualityComparer<Vector2?>.Default.GetHashCode(this.<ItemPos>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Rotation>k__BackingField);
			}

			// Token: 0x0600987D RID: 39037 RVA: 0x003DDDE6 File Offset: 0x003DBFE6
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ItemContainer.ContainedItem && this.Equals((ItemContainer.ContainedItem)obj);
			}

			// Token: 0x0600987E RID: 39038 RVA: 0x003DDE00 File Offset: 0x003DC000
			[CompilerGenerated]
			public bool Equals(ItemContainer.ContainedItem other)
			{
				return EqualityComparer<Item>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Hide>k__BackingField, other.<Hide>k__BackingField) && EqualityComparer<Vector2?>.Default.Equals(this.<ItemPos>k__BackingField, other.<ItemPos>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Rotation>k__BackingField, other.<Rotation>k__BackingField);
			}

			// Token: 0x0600987F RID: 39039 RVA: 0x003DDE6D File Offset: 0x003DC06D
			[CompilerGenerated]
			public void Deconstruct(out Item Item, out bool Hide, out Vector2? ItemPos, out float Rotation)
			{
				Item = this.Item;
				Hide = this.Hide;
				ItemPos = this.ItemPos;
				Rotation = this.Rotation;
			}
		}

		// Token: 0x020013C4 RID: 5060
		private class SlotRestrictions
		{
			// Token: 0x06009880 RID: 39040 RVA: 0x003DDE94 File Offset: 0x003DC094
			public SlotRestrictions(int maxStackSize, List<RelatedItem> containableItems, bool autoInject)
			{
				this.MaxStackSize = maxStackSize;
				this.ContainableItems = containableItems;
				this.AutoInject = autoInject;
			}

			// Token: 0x06009881 RID: 39041 RVA: 0x003DDEB4 File Offset: 0x003DC0B4
			public bool MatchesItem(Item item)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.MatchesItem(item));
			}

			// Token: 0x06009882 RID: 39042 RVA: 0x003DDEFC File Offset: 0x003DC0FC
			public bool MatchesItem(ItemPrefab itemPrefab)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.MatchesItem(itemPrefab));
			}

			// Token: 0x06009883 RID: 39043 RVA: 0x003DDF44 File Offset: 0x003DC144
			public bool MatchesItem(Identifier identifierOrTag)
			{
				return this.ContainableItems == null || this.ContainableItems.Count == 0 || this.ContainableItems.Any((RelatedItem c) => c.Identifiers.Contains(identifierOrTag) && !c.ExcludedIdentifiers.Contains(identifierOrTag));
			}

			// Token: 0x04006370 RID: 25456
			public int MaxStackSize;

			// Token: 0x04006371 RID: 25457
			public List<RelatedItem> ContainableItems;

			// Token: 0x04006372 RID: 25458
			public readonly bool AutoInject;
		}
	}
}
