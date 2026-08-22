using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200003C RID: 60
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxUI
	{
		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000949 RID: 2377 RVA: 0x0005344C File Offset: 0x0005164C
		public bool Locked
		{
			get
			{
				return this.CircuitBox.IsLocked();
			}
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0005345C File Offset: 0x0005165C
		public CircuitBoxUI(CircuitBox box)
		{
			Option.UnspecifiedNone none = Option.None;
			this.selection = none;
			this.searchTerm = string.Empty;
			this.VirtualWires = new List<CircuitBoxWireRenderer>();
			base..ctor();
			this.camera = new Camera
			{
				MinZoom = 0.25f,
				MaxZoom = 2f
			};
			this.CircuitBox = box;
			this.MouseSnapshotHandler = new CircuitBoxMouseDragSnapshotHandler(this);
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x000534E0 File Offset: 0x000516E0
		public void CreateGUI(GUIFrame parent)
		{
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.97f, 0.95f), parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			this.circuitComponent = new GUICustomComponent(new RectTransform(Vector2.One, paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = component.Rect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, new Matrix?(this.camera.Transform));
				this.DrawCircuits(spriteBatch);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
				this.DrawHUD(spriteBatch, component.Rect);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}, null);
			GUIScissorComponent menuContainer = new GUIScissorComponent(new RectTransform(Vector2.One, paddedFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal))
			{
				CanBeFocused = false
			};
			this.componentMenuOpen = true;
			this.componentMenu = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), menuContainer.Content.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), "", null);
			this.toggleMenuButton = new GUIButton(new RectTransform(new Point(300, 30), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				MinSize = new Point(0, 15)
			}, Alignment.Center, "UIToggleButtonVertical", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.componentMenuOpen = !this.componentMenuOpen;
					if (this.Locked)
					{
						this.componentMenuOpen = false;
					}
					foreach (GUIComponent child in btn.Children)
					{
						child.SpriteEffects = (this.componentMenuOpen ? SpriteEffects.None : SpriteEffects.FlipVertically);
					}
					return true;
				}
			};
			GUILayoutGroup menuLayout = new GUILayoutGroup(new RectTransform(Vector2.One, this.componentMenu.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup headerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), menuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup labelLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.33f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup searchBarLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.33f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUITextBlock searchBarLabel = new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), searchBarLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Filter", null, null, Alignment.Left, false, "", null);
			GUITextBox searchbar = new GUITextBox(new RectTransform(new Vector2(0.85f, 1f), searchBarLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null, true, true);
			new GUIFrame(new RectTransform(new Vector2(0.5f, 0.01f), menuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			this.componentList = new GUIListBox(new RectTransform(new Vector2(0.95f, 0.65f), menuLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				UseGridLayout = true,
				OnSelected = delegate(GUIComponent _, object o)
				{
					ItemPrefab prefab2 = o as ItemPrefab;
					if (prefab2 == null)
					{
						return false;
					}
					this.CircuitBox.HeldComponent = Option.Some<ItemPrefab>(prefab2);
					return true;
				}
			};
			GUILayoutGroup inventoryLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.33f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.Center);
			GUILayoutGroup indicatorLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.2f, 1f), inventoryLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUIImage indicatorIcon = new GUIImage(new RectTransform(new Vector2(0.5f, 0.8f), indicatorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "CircuitIndicatorIcon", GUIImage.ScalingMode.None);
			RectTransform rectT = new RectTransform(new Vector2(0.5f, 1f), indicatorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.GetInventoryText();
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			this.inventoryIndicatorText = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			int gapSize = GUI.IntScale(8f);
			Point zero = Point.Zero;
			GUIListBox.OnSelectedHandler onWireSelected;
			if ((onWireSelected = CircuitBoxUI.<>O.<0>__SelectWire) == null)
			{
				onWireSelected = (CircuitBoxUI.<>O.<0>__SelectWire = new GUIListBox.OnSelectedHandler(CircuitBoxUI.SelectWire));
			}
			this.selectedWireFrame = SubEditorScreen.CreateWiringPanel(zero, onWireSelected);
			this.selectedWireFrame.RectTransform.AbsoluteOffset = new Point(parent.Rect.X - (this.selectedWireFrame.Rect.Width + gapSize), parent.Rect.Y);
			foreach (ItemPrefab prefab in from p in ItemPrefab.Prefabs
			orderby p.Name
			select p)
			{
				if (prefab.Tags.Contains("circuitboxcomponent"))
				{
					CircuitBoxUI.CreateComponentElement(prefab, this.componentList.Content.RectTransform);
				}
			}
			searchbar.OnTextChanged += delegate(GUITextBox tb, string s)
			{
				this.searchTerm = s;
				this.UpdateComponentList();
				return true;
			};
			int buttonHeight = (int)((float)GUIStyle.ItemFrameMargin.Y * 0.4f);
			new GUIButton(new RectTransform(new Point(buttonHeight), parent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(buttonHeight / 4),
				MinSize = new Point(buttonHeight)
			}, Alignment.Center, "GUIButtonSettings", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				ContextMenuOption[] array = new ContextMenuOption[2];
				int num = 0;
				ContextMenuOption contextMenuOption = new ContextMenuOption("circuitboxsetting.resetview", true, new Action(this.<CreateGUI>g__ResetCamera|20_6))
				{
					Tooltip = TextManager.Get("circuitboxsettingdescription.resetview")
				};
				array[num] = contextMenuOption;
				int num2 = 1;
				string label = "circuitboxsetting.find";
				bool isEnabled = true;
				ContextMenuOption[] array2 = new ContextMenuOption[3];
				int num3 = 0;
				contextMenuOption = new ContextMenuOption("circuitboxsetting.focusinput", true, delegate()
				{
					this.<CreateGUI>g__FindInputOutput|20_7(CircuitBoxInputOutputNode.Type.Input);
				})
				{
					Tooltip = TextManager.Get("circuitboxsettingdescription.focusinput")
				};
				array2[num3] = contextMenuOption;
				int num4 = 1;
				contextMenuOption = new ContextMenuOption("circuitboxsetting.focusoutput", true, delegate()
				{
					this.<CreateGUI>g__FindInputOutput|20_7(CircuitBoxInputOutputNode.Type.Output);
				})
				{
					Tooltip = TextManager.Get("circuitboxsettingdescription.focusoutput")
				};
				array2[num4] = contextMenuOption;
				int num5 = 2;
				contextMenuOption = new ContextMenuOption("circuitboxsetting.focuscircuits", this.CircuitBox.Components.Any<CircuitBoxComponent>(), new Action(this.<CreateGUI>g__FindCircuit|20_8))
				{
					Tooltip = TextManager.Get("circuitboxsettingdescription.focuscircuits")
				};
				array2[num5] = contextMenuOption;
				array[num2] = new ContextMenuOption(label, isEnabled, array2);
				GUIContextMenu.CreateContextMenu(array);
				return true;
			};
			this.MouseSnapshotHandler.UpdateConnections();
			foreach (CircuitBoxComponent node in this.CircuitBox.Components)
			{
				node.OnUICreated();
			}
			foreach (CircuitBoxInputOutputNode node2 in this.CircuitBox.InputOutputNodes)
			{
				node2.OnUICreated();
			}
			foreach (CircuitBoxWire wire in this.CircuitBox.Wires)
			{
				wire.Update();
			}
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x00053CDC File Offset: 0x00051EDC
		private string GetInventoryText()
		{
			ItemContainer container = this.CircuitBox.ComponentContainer;
			if (container == null)
			{
				return "0/0";
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted<int>(container.Inventory.AllItems.Count<Item>());
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(container.Capacity);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x00053D40 File Offset: 0x00051F40
		public void UpdateComponentList()
		{
			GUITextBlock text = this.inventoryIndicatorText;
			if (text != null)
			{
				text.Text = this.GetInventoryText();
			}
			if (this.componentList == null)
			{
				return;
			}
			ImmutableArray<Item> playerInventory = CircuitBox.GetSortedCircuitBoxItemsFromPlayer(Character.Controlled);
			foreach (GUIComponent child in this.componentList.Content.Children)
			{
				ItemPrefab prefab = child.UserData as ItemPrefab;
				if (prefab != null)
				{
					child.Enabled = (!this.CircuitBox.IsFull && (!CircuitBox.IsInGame() || CircuitBox.GetApplicableResourcePlayerHas(prefab, playerInventory).IsSome()));
					GUILayoutGroup child2 = child.GetChild<GUILayoutGroup>();
					GUIImage image = (child2 != null) ? child2.GetChild<GUIImage>() : null;
					if (image != null)
					{
						image.Enabled = child.Enabled;
					}
					child.ToolTip = (child.Enabled ? prefab.Description : RichString.Rich(TextManager.GetWithVariable(new Identifier("CircuitBoxUIComponentNotAvailable"), new Identifier("[item]"), prefab.Name, FormatCapitals.No), null));
					if (string.IsNullOrWhiteSpace(this.searchTerm))
					{
						child.Visible = true;
					}
					else
					{
						child.Visible = prefab.Name.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase);
					}
				}
			}
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x00053EA4 File Offset: 0x000520A4
		private static bool SelectWire(GUIComponent component, object obj)
		{
			ItemPrefab prefab = obj as ItemPrefab;
			if (prefab == null)
			{
				return false;
			}
			CircuitBoxWire.SelectedWirePrefab = prefab;
			return true;
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x00053EC4 File Offset: 0x000520C4
		private static void CreateComponentElement(ItemPrefab prefab, RectTransform parent)
		{
			GUIFrame itemFrame = new GUIFrame(new RectTransform(new Vector2(0.1f, 0.9f), parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 50)
			}, "GUITextBox", null)
			{
				UserData = prefab
			};
			itemFrame.RectTransform.MinSize = new Point(0, itemFrame.Rect.Width);
			itemFrame.RectTransform.MaxSize = new Point(int.MaxValue, itemFrame.Rect.Width);
			itemFrame.ToolTip = prefab.Name;
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), itemFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.03f,
				CanBeFocused = false
			};
			Sprite icon;
			Color iconColor;
			if (prefab.InventoryIcon != null)
			{
				icon = prefab.InventoryIcon;
				iconColor = prefab.InventoryIconColor;
			}
			else
			{
				icon = prefab.Sprite;
				iconColor = prefab.SpriteColor;
			}
			GUIImage img = null;
			if (icon != null)
			{
				img = new GUIImage(new RectTransform(new Vector2(1f, 0.8f), paddedFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), icon, null, GUIImage.ScalingMode.None)
				{
					CanBeFocused = false,
					LoadAsynchronously = true,
					DisabledColor = Color.DarkGray * 0.8f,
					Color = iconColor
				};
			}
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
			RichString text = prefab.Name;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Center, false, "", null)
			{
				CanBeFocused = false
			};
			textBlock.Text = ToolBox.LimitString(textBlock.Text, textBlock.Font, textBlock.Rect.Width);
			paddedFrame.Recalculate();
			if (img != null)
			{
				img.Scale = Math.Min(Math.Min((float)img.Rect.Width / img.Sprite.size.X, (float)img.Rect.Height / img.Sprite.size.Y), 1.5f);
				img.RectTransform.NonScaledSize = new Point((int)(img.Sprite.size.X * img.Scale), img.Rect.Height);
			}
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x000541BC File Offset: 0x000523BC
		private void DrawHUD(SpriteBatch spriteBatch, Rectangle screenRect)
		{
			float scale = GUI.Scale / 1.5f;
			Vector2 offset = new Vector2(20f, 40f) * scale;
			foreach (KeyValuePair<Character, CircuitBoxCursor> keyValuePair in this.CircuitBox.ActiveCursors)
			{
				Character character2;
				CircuitBoxCursor circuitBoxCursor;
				keyValuePair.Deconstruct(out character2, out circuitBoxCursor);
				Character character = character2;
				CircuitBoxCursor cursor = circuitBoxCursor;
				if (cursor.IsActive)
				{
					Vector2 cursorWorldPos = this.camera.WorldToScreen(cursor.DrawPosition);
					Vector2 dragStart;
					if (cursor.Info.DragStart.TryUnwrap(out dragStart))
					{
						this.DrawSelection(spriteBatch, dragStart, cursor.DrawPosition, cursor.Color);
					}
					ItemPrefab otherHeldPrefab;
					if (cursor.HeldPrefab.TryUnwrap(out otherHeldPrefab))
					{
						otherHeldPrefab.Sprite.Draw(spriteBatch, cursorWorldPos, 0f, 1f, SpriteEffects.None);
					}
					Sprite sprite = this.cursorSprite;
					if (sprite != null)
					{
						sprite.Draw(spriteBatch, cursorWorldPos, cursor.Color, 0f, scale, SpriteEffects.None, null);
					}
					GUI.DrawString(spriteBatch, cursorWorldPos + offset, character.Name, cursor.Color, new Color?(Color.Black), GUI.IntScale(4f), GUIStyle.SmallFont, ForceUpperCase.Inherit);
				}
			}
			RectangleF rect;
			if (this.selection.TryUnwrap(out rect))
			{
				Vector2 pos = rect.Location;
				Vector2 pos2 = new Vector2(rect.Location.X + rect.Size.X, rect.Location.Y + rect.Size.Y);
				this.DrawSelection(spriteBatch, pos, pos2, GUIStyle.Blue);
			}
			ItemPrefab component;
			if (this.CircuitBox.HeldComponent.TryUnwrap(out component))
			{
				component.Sprite.Draw(spriteBatch, PlayerInput.MousePosition, 0f, 1f, SpriteEffects.None);
			}
			if (PlayerInput.PrimaryMouseButtonHeld() && this.MouseSnapshotHandler.LastConnectorUnderCursor.IsSome())
			{
				CircuitBoxWire.SelectedWirePrefab.Sprite.Draw(spriteBatch, PlayerInput.MousePosition, CircuitBoxWire.SelectedWirePrefab.SpriteColor, 0f, this.camera.Zoom, SpriteEffects.None, null);
			}
			foreach (CircuitBoxComponent c in this.CircuitBox.Components)
			{
				c.DrawHUD(spriteBatch, this.camera);
			}
			foreach (CircuitBoxInputOutputNode i in this.CircuitBox.InputOutputNodes)
			{
				i.DrawHUD(spriteBatch, this.camera);
			}
			if (this.Locked)
			{
				LocalizedString lockedText = TextManager.Get("CircuitBoxLocked").Fallback(TextManager.Get("ConnectionLocked"), false);
				Vector2 size = GUIStyle.LargeFont.MeasureString(lockedText, false);
				Vector2 pos3 = new Vector2((float)screenRect.Center.X - size.X / 2f, (float)screenRect.Top + (float)screenRect.Height * 0.05f);
				GUI.DrawString(spriteBatch, pos3, lockedText, Color.Red, new Color?(Color.Black), 8, GUIStyle.LargeFont, ForceUpperCase.Inherit);
			}
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x00054568 File Offset: 0x00052768
		private void DrawSelection(SpriteBatch spriteBatch, Vector2 pos1, Vector2 pos2, Color color)
		{
			Vector2 location = this.camera.WorldToScreen(pos1);
			location.Y = -location.Y;
			Vector2 location2 = this.camera.WorldToScreen(pos2);
			location2.Y = -location2.Y;
			MapEntity.DrawSelectionRect(spriteBatch, location, new Vector2(-(location.X - location2.X), location.Y - location2.Y), color);
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x000545D4 File Offset: 0x000527D4
		public static void DrawRectangleWithBorder(SpriteBatch spriteBatch, RectangleF rect, Color fillColor, Color borderColor)
		{
			GUI.DrawFilledRectangle(spriteBatch, rect, fillColor, 0f);
			CircuitBoxUI.DrawRectangleOnlyBorder(spriteBatch, rect, borderColor);
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x000545EC File Offset: 0x000527EC
		private static void DrawRectangleOnlyBorder(SpriteBatch spriteBatch, RectangleF rect, Color borderColor)
		{
			Vector2 topRight = new Vector2(rect.Right, rect.Top);
			Vector2 topLeft = new Vector2(rect.Left, rect.Top);
			Vector2 bottomRight = new Vector2(rect.Right, rect.Bottom);
			Vector2 bottomLeft = new Vector2(rect.Left, rect.Bottom);
			Vector2 offset = new Vector2(0f, CircuitBoxUI.lineWidth / 2f);
			spriteBatch.DrawLine(topRight, topLeft, borderColor, CircuitBoxUI.lineWidth);
			spriteBatch.DrawLine(topLeft - offset, bottomLeft + offset, borderColor, CircuitBoxUI.lineWidth);
			spriteBatch.DrawLine(bottomLeft, bottomRight, borderColor, CircuitBoxUI.lineWidth);
			spriteBatch.DrawLine(bottomRight + offset, topRight - offset, borderColor, CircuitBoxUI.lineWidth);
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x000546B8 File Offset: 0x000528B8
		private void DrawCircuits(SpriteBatch spriteBatch)
		{
			this.camera.UpdateTransform(true, false);
			SubEditorScreen.DrawOutOfBoundsArea(spriteBatch, this.camera, 8192f, GUIStyle.Red * 0.33f);
			SubEditorScreen.DrawGrid(spriteBatch, this.camera, CircuitBoxUI.gridSize.X, CircuitBoxUI.gridSize.Y, false);
			CircuitBoxUI.lineWidth = 1f / this.camera.Zoom;
			Vector2 mousePos = this.GetCursorPosition();
			mousePos.Y = -mousePos.Y;
			foreach (CircuitBoxLabelNode label in this.CircuitBox.Labels)
			{
				if (label.IsSelected)
				{
					label.DrawSelection(spriteBatch, this.GetSelectionColor(label));
				}
				label.Draw(spriteBatch, label.Position, label.Color);
			}
			foreach (CircuitBoxWire wire in this.CircuitBox.Wires)
			{
				wire.Renderer.Draw(spriteBatch, this.GetSelectionColor(wire));
			}
			foreach (CircuitBoxComponent node in this.CircuitBox.Components)
			{
				if (node.IsSelected)
				{
					node.DrawSelection(spriteBatch, this.GetSelectionColor(node));
				}
				node.Draw(spriteBatch, node.Position, node.Item.Prefab.SignalComponentColor * CircuitBoxNode.Opacity);
			}
			foreach (CircuitBoxInputOutputNode ioNode in this.CircuitBox.InputOutputNodes)
			{
				if (ioNode.IsSelected)
				{
					ioNode.DrawSelection(spriteBatch, this.GetSelectionColor(ioNode));
				}
				Color color = (ioNode.NodeType == CircuitBoxInputOutputNode.Type.Input) ? GUIStyle.Green : GUIStyle.Red;
				ioNode.Draw(spriteBatch, ioNode.Position, color * CircuitBoxNode.Opacity);
			}
			if (this.MouseSnapshotHandler.IsDragging)
			{
				ImmutableHashSet<CircuitBoxNode> draggedNodes = this.MouseSnapshotHandler.GetMoveAffectedComponents();
				Vector2 dragOffset = this.MouseSnapshotHandler.GetDragAmount(this.GetCursorPosition());
				foreach (CircuitBoxNode moveable in draggedNodes)
				{
					CircuitBoxComponent node2 = moveable as CircuitBoxComponent;
					Color color3;
					if (node2 == null)
					{
						CircuitBoxLabelNode label2 = moveable as CircuitBoxLabelNode;
						if (label2 == null)
						{
							CircuitBoxInputOutputNode ioNode2 = moveable as CircuitBoxInputOutputNode;
							if (ioNode2 == null)
							{
								color3 = Color.White;
							}
							else
							{
								color3 = ((ioNode2.NodeType == CircuitBoxInputOutputNode.Type.Input) ? GUIStyle.Green : GUIStyle.Red);
							}
						}
						else
						{
							color3 = label2.Color;
						}
					}
					else
					{
						color3 = node2.Item.Prefab.SignalComponentColor;
					}
					Color color2 = color3;
					moveable.Draw(spriteBatch, moveable.Position + dragOffset, color2 * 0.5f);
				}
			}
			ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode> resize;
			if (this.MouseSnapshotHandler.IsResizing && this.MouseSnapshotHandler.LastResizeAffectedNode.TryUnwrap(out resize))
			{
				ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode> valueTuple = resize;
				CircuitBoxResizeDirection dir = valueTuple.Item1;
				CircuitBoxNode node3 = valueTuple.Item2;
				Vector2 dragOffset2 = this.MouseSnapshotHandler.GetDragAmount(this.GetCursorPosition());
				RectangleF rect = node3.Rect;
				rect.Y = -rect.Y;
				rect.Y -= rect.Height;
				if (dir.HasFlag(CircuitBoxResizeDirection.Down))
				{
					rect.Height -= dragOffset2.Y;
					rect.Height = Math.Max(rect.Height, CircuitBoxLabelNode.MinSize.Y + 48f);
				}
				if (dir.HasFlag(CircuitBoxResizeDirection.Right))
				{
					rect.Width += dragOffset2.X;
					rect.Width = Math.Max(rect.Width, CircuitBoxLabelNode.MinSize.X);
				}
				if (dir.HasFlag(CircuitBoxResizeDirection.Left))
				{
					float oldWidth = rect.Width;
					rect.Width -= dragOffset2.X;
					rect.Width = Math.Max(rect.Width, CircuitBoxLabelNode.MinSize.X);
					float actualResize = rect.Width - oldWidth;
					rect.X -= actualResize;
				}
				CircuitBoxUI.DrawRectangleOnlyBorder(spriteBatch, rect, GUIStyle.Yellow);
			}
			CircuitBoxWireRenderer draggedWire;
			if (CircuitBoxUI.DraggedWire.TryUnwrap(out draggedWire))
			{
				draggedWire.Draw(spriteBatch, GUIStyle.Yellow);
			}
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x00054BBC File Offset: 0x00052DBC
		private Color GetSelectionColor(CircuitBoxNode node)
		{
			return this.GetSelectionColor(node.SelectedBy, node.IsSelectedByMe);
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x00054BD0 File Offset: 0x00052DD0
		private Color GetSelectionColor(CircuitBoxWire wire)
		{
			return this.GetSelectionColor(wire.SelectedBy, wire.IsSelectedByMe);
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x00054BE4 File Offset: 0x00052DE4
		private Color GetSelectionColor(ushort selectedBy, bool isSelectedByMe)
		{
			if (isSelectedByMe)
			{
				return GUIStyle.Yellow;
			}
			foreach (KeyValuePair<Character, CircuitBoxCursor> keyValuePair in this.CircuitBox.ActiveCursors)
			{
				Character character;
				CircuitBoxCursor circuitBoxCursor;
				keyValuePair.Deconstruct(out character, out circuitBoxCursor);
				CircuitBoxCursor cursor = circuitBoxCursor;
				if (cursor.Info.CharacterID == selectedBy)
				{
					return cursor.Color;
				}
			}
			return GUIStyle.Yellow;
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00054C78 File Offset: 0x00052E78
		public Vector2 GetCursorPosition()
		{
			return this.cursorPos;
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x00054C80 File Offset: 0x00052E80
		[NullableContext(0)]
		public Option<Vector2> GetDragStart()
		{
			return from f in this.selection
			select f.Location;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x00054CAC File Offset: 0x00052EAC
		public void Update(float deltaTime)
		{
			this.cursorPos = this.camera.ScreenToWorld(PlayerInput.MousePosition);
			foreach (CircuitBoxWire wire4 in this.CircuitBox.Wires)
			{
				wire4.Update();
			}
			bool foundSelected = false;
			foreach (CircuitBoxComponent node3 in this.CircuitBox.Components)
			{
				if (node3.IsSelectedByMe)
				{
					foundSelected = true;
					if (this.circuitComponent != null)
					{
						node3.UpdateEditing(this.circuitComponent.RectTransform);
						break;
					}
					break;
				}
			}
			if (!foundSelected)
			{
				CircuitBoxComponent.RemoveEditingHUD();
			}
			bool isMouseOn = GUI.MouseOn == this.circuitComponent;
			if (isMouseOn)
			{
				Character.DisableControls = true;
			}
			this.camera.MoveCamera(deltaTime, true, isMouseOn, isMouseOn, new bool?(false));
			if (this.camera.TargetPos != Vector2.Zero && MathUtils.NearlyEqual(this.camera.Position, this.camera.TargetPos, 0.01f))
			{
				this.camera.TargetPos = Vector2.Zero;
			}
			if (isMouseOn)
			{
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					if (this.CircuitBox.HeldComponent.IsNone())
					{
						this.MouseSnapshotHandler.StartDragging();
					}
					else
					{
						this.MouseSnapshotHandler.ClearSnapshot();
					}
				}
				if (PlayerInput.DoubleClicked() && this.MouseSnapshotHandler.FindWireUnderCursor(this.cursorPos).IsNone())
				{
					CircuitBoxNode topmostNode = this.GetTopmostNode(this.MouseSnapshotHandler.FindNodesUnderCursor(this.cursorPos));
					CircuitBoxLabelNode label2 = topmostNode as CircuitBoxLabelNode;
					if (label2 != null && this.circuitComponent != null)
					{
						label2.PromptEditText(this.circuitComponent);
					}
				}
				if (PlayerInput.MidButtonHeld() || (PlayerInput.IsAltDown() && PlayerInput.PrimaryMouseButtonHeld()))
				{
					Vector2 moveSpeed = PlayerInput.MouseSpeed / this.camera.Zoom;
					moveSpeed.X = -moveSpeed.X;
					this.camera.Position += moveSpeed;
				}
				if (PlayerInput.PrimaryMouseButtonHeld())
				{
					this.MouseSnapshotHandler.UpdateDrag(this.GetCursorPosition());
				}
				CircuitBoxConnection c;
				if (this.MouseSnapshotHandler.IsWiring && this.MouseSnapshotHandler.LastConnectorUnderCursor.TryUnwrap(out c))
				{
					Vector2 start = c.Rect.Center;
					Vector2 end = this.GetCursorPosition();
					end.Y = -end.Y;
					if (!c.IsOutput)
					{
						Vector2 vector = end;
						end = start;
						start = vector;
					}
					CircuitBoxWireRenderer wire2;
					if (CircuitBoxUI.DraggedWire.TryUnwrap(out wire2))
					{
						wire2.Recompute(start, end, CircuitBoxWire.SelectedWirePrefab.SpriteColor);
					}
					else
					{
						Option.UnspecifiedNone none = Option.None;
						CircuitBoxUI.DraggedWire = Option.Some<CircuitBoxWireRenderer>(new CircuitBoxWireRenderer(none, start, end, GUIStyle.Red, this.CircuitBox.WireSprite));
					}
				}
				else
				{
					Option.UnspecifiedNone none = Option.None;
					CircuitBoxUI.DraggedWire = none;
				}
				if (PlayerInput.SecondaryMouseButtonClicked())
				{
					this.OpenContextMenu();
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					bool selectedNode = false;
					ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode> r;
					if (this.MouseSnapshotHandler.IsResizing && this.MouseSnapshotHandler.LastResizeAffectedNode.TryUnwrap(out r))
					{
						ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode> valueTuple = r;
						CircuitBoxResizeDirection dir = valueTuple.Item1;
						CircuitBoxNode node2 = valueTuple.Item2;
						this.CircuitBox.ResizeNode(node2, dir, this.MouseSnapshotHandler.GetDragAmount(this.cursorPos));
					}
					ItemPrefab prefab;
					if (this.CircuitBox.HeldComponent.TryUnwrap(out prefab))
					{
						this.CircuitBox.AddComponent(prefab, this.cursorPos);
					}
					else if (this.MouseSnapshotHandler.IsDragging && PlayerInput.PrimaryMouseButtonReleased())
					{
						this.CircuitBox.MoveComponent(this.MouseSnapshotHandler.GetDragAmount(this.cursorPos), this.MouseSnapshotHandler.GetMoveAffectedComponents());
					}
					else if (!this.MouseSnapshotHandler.IsWiring)
					{
						selectedNode = this.TrySelectComponentsUnderCursor();
					}
					CircuitBoxConnection one;
					CircuitBoxConnection two;
					if (this.MouseSnapshotHandler.IsWiring && this.MouseSnapshotHandler.LastConnectorUnderCursor.TryUnwrap(out one) && this.MouseSnapshotHandler.FindConnectorUnderCursor(this.cursorPos).TryUnwrap(out two))
					{
						this.CircuitBox.AddWire(one, two);
					}
					CircuitBoxWire wire3;
					if (this.MouseSnapshotHandler.LastWireUnderCursor.TryUnwrap(out wire3) && !this.MouseSnapshotHandler.IsDragging && !selectedNode)
					{
						this.CircuitBox.SelectWires(ImmutableArray.Create<CircuitBoxWire>(wire3), !PlayerInput.IsShiftDown());
					}
					else if (this.CircuitBox.Wires.Any((CircuitBoxWire wire) => wire.IsSelectedByMe))
					{
						this.CircuitBox.SelectWires(ImmutableArray<CircuitBoxWire>.Empty, !PlayerInput.IsShiftDown());
					}
					CircuitBox circuitBox = this.CircuitBox;
					Option.UnspecifiedNone none = Option.None;
					circuitBox.HeldComponent = none;
					this.MouseSnapshotHandler.EndDragging();
				}
				if (this.MouseSnapshotHandler.GetLastComponentsUnderCursor().IsEmpty && this.MouseSnapshotHandler.LastConnectorUnderCursor.IsNone())
				{
					this.UpdateSelection();
				}
				bool hitDeleteCombo = PlayerInput.KeyHit(Keys.Delete) || (PlayerInput.IsCtrlDown() && PlayerInput.KeyHit(Keys.D));
				if (GUI.KeyboardDispatcher.Subscriber == null && hitDeleteCombo)
				{
					this.CircuitBox.RemoveComponents((from node in this.CircuitBox.Components
					where node.IsSelectedByMe
					select node).ToArray<CircuitBoxComponent>());
					this.CircuitBox.RemoveWires((from wire in this.CircuitBox.Wires
					where wire.IsSelectedByMe
					select wire).ToImmutableArray<CircuitBoxWire>());
					this.CircuitBox.RemoveLabel((from label in this.CircuitBox.Labels
					where label.IsSelectedByMe
					select label).ToImmutableArray<CircuitBoxLabelNode>());
				}
			}
			GUIFrame menu = this.componentMenu;
			if (menu != null)
			{
				GUIButton button = this.toggleMenuButton;
				if (button != null)
				{
					button.Enabled = !this.Locked;
					this.componentMenuOpenState = ((this.componentMenuOpen && !this.Locked) ? Math.Min(this.componentMenuOpenState + deltaTime * 5f, 1f) : Math.Max(this.componentMenuOpenState - deltaTime * 5f, 0f));
					menu.RectTransform.ScreenSpaceOffset = Vector2.Lerp(new Vector2(0f, (float)(menu.Rect.Height - 10)), Vector2.Zero, this.componentMenuOpenState).ToPoint();
					button.RectTransform.AbsoluteOffset = new Point(menu.Rect.X + (menu.Rect.Width / 2 - button.Rect.Width / 2), menu.Rect.Y - button.Rect.Height);
				}
			}
			GUIFrame wireFrame = this.selectedWireFrame;
			if (wireFrame != null)
			{
				wireFrame.Visible = !this.Locked;
			}
			this.camera.Position = Vector2.Clamp(this.camera.Position, new Vector2(-4096f), new Vector2(4096f));
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0005542C File Offset: 0x0005362C
		public void SetMenuVisibility(bool state)
		{
			this.componentMenuOpen = state;
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00055438 File Offset: 0x00053638
		private void UpdateSelection()
		{
			if (!PlayerInput.IsAltDown() && PlayerInput.PrimaryMouseButtonDown())
			{
				this.selection = Option.Some<RectangleF>(new RectangleF(this.GetCursorPosition(), Vector2.Zero));
			}
			RectangleF rect;
			if (!this.selection.TryUnwrap(out rect))
			{
				return;
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				Option.UnspecifiedNone none = Option.None;
				this.selection = none;
				RectangleF selectionRect = Submarine.AbsRectF(rect.Location, rect.Size);
				float treshold = 12f / this.camera.Zoom;
				if (selectionRect.Size.X < treshold || selectionRect.Size.Y < treshold)
				{
					return;
				}
				this.CircuitBox.SelectComponents((from n in this.MouseSnapshotHandler.Nodes
				where selectionRect.Intersects(n.Rect)
				select n).ToImmutableHashSet<CircuitBoxNode>(), !PlayerInput.IsShiftDown());
				return;
			}
			else
			{
				RectangleF oldRect = rect;
				rect.Size = this.camera.ScreenToWorld(PlayerInput.MousePosition) - rect.Location;
				if (rect.Equals(oldRect))
				{
					return;
				}
				this.selection = Option.Some<RectangleF>(rect);
				return;
			}
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x00055568 File Offset: 0x00053768
		private bool TrySelectComponentsUnderCursor()
		{
			CircuitBoxNode foundNode = this.GetTopmostNode(this.MouseSnapshotHandler.GetLastComponentsUnderCursor());
			if (foundNode is CircuitBoxLabelNode && this.MouseSnapshotHandler.LastWireUnderCursor.IsSome())
			{
				foundNode = null;
			}
			this.CircuitBox.SelectComponents((foundNode == null) ? ImmutableArray<CircuitBoxNode>.Empty : ImmutableArray.Create<CircuitBoxNode>(foundNode), !PlayerInput.IsShiftDown());
			return foundNode != null;
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x000555D0 File Offset: 0x000537D0
		private void OpenContextMenu()
		{
			Option<CircuitBoxWire> wireOption = this.MouseSnapshotHandler.FindWireUnderCursor(this.cursorPos);
			ImmutableArray<CircuitBoxWire> wireSelection = (from w in this.CircuitBox.Wires
			where w.IsSelectedByMe
			select w).ToImmutableArray<CircuitBoxWire>();
			CircuitBoxNode nodeOption = this.GetTopmostNode(this.MouseSnapshotHandler.FindNodesUnderCursor(this.cursorPos));
			ImmutableArray<CircuitBoxComponent> nodeSelection = (from n in this.CircuitBox.Components
			where n.IsSelectedByMe
			select n).ToImmutableArray<CircuitBoxComponent>();
			ImmutableArray<CircuitBoxLabelNode> labels = (from l in this.CircuitBox.Labels
			where l.IsSelectedByMe
			select l).ToImmutableArray<CircuitBoxLabelNode>();
			LocalizedString label = TextManager.Get("delete");
			bool flag = wireOption.IsSome();
			bool flag2 = flag;
			if (!flag2)
			{
				bool flag3 = nodeOption is CircuitBoxComponent || nodeOption is CircuitBoxLabelNode;
				flag2 = flag3;
			}
			ContextMenuOption option = new ContextMenuOption(label, flag2 && !this.Locked, delegate()
			{
				CircuitBoxWire wire;
				if (wireOption.TryUnwrap(out wire))
				{
					this.CircuitBox.RemoveWires(wire.IsSelected ? wireSelection : ImmutableArray.Create<CircuitBoxWire>(wire));
					return;
				}
				CircuitBoxComponent node = nodeOption as CircuitBoxComponent;
				if (node != null)
				{
					this.CircuitBox.RemoveComponents(node.IsSelected ? nodeSelection : ImmutableArray.Create<CircuitBoxComponent>(node));
					return;
				}
				CircuitBoxLabelNode label2 = nodeOption as CircuitBoxLabelNode;
				if (label2 == null)
				{
					return;
				}
				this.CircuitBox.RemoveLabel(label2.IsSelected ? labels : ImmutableArray.Create<CircuitBoxLabelNode>(label2));
			});
			ContextMenuOption editLabel = new ContextMenuOption(TextManager.Get("circuitboxeditlabel"), nodeOption is CircuitBoxLabelNode && !this.Locked, delegate()
			{
				if (this.circuitComponent == null)
				{
					return;
				}
				CircuitBoxLabelNode label2 = nodeOption as CircuitBoxLabelNode;
				if (label2 == null)
				{
					return;
				}
				label2.PromptEditText(this.circuitComponent);
			});
			ContextMenuOption editConnections = new ContextMenuOption(TextManager.Get("circuitboxrenameconnections"), nodeOption is CircuitBoxInputOutputNode && !this.Locked, delegate()
			{
				if (this.circuitComponent == null)
				{
					return;
				}
				CircuitBoxInputOutputNode io = nodeOption as CircuitBoxInputOutputNode;
				if (io == null)
				{
					return;
				}
				io.PromptEdit(this.circuitComponent);
			});
			ContextMenuOption addLabelOption = new ContextMenuOption(TextManager.Get("circuitboxaddlabel"), !this.Locked, delegate()
			{
				this.CircuitBox.AddLabel(this.cursorPos);
			});
			ContextMenuOption[] allOptions = new ContextMenuOption[]
			{
				addLabelOption,
				editLabel,
				editConnections,
				option
			};
			CircuitBoxComponent comp = nodeOption as CircuitBoxComponent;
			if (comp != null)
			{
				GUIContextMenu.CreateContextMenu(new Vector2?(PlayerInput.MousePosition), comp.Item.Name, new Color?(comp.Item.Prefab.SignalComponentColor), allOptions);
				return;
			}
			CircuitBoxWire foundWire;
			if (wireOption.TryUnwrap(out foundWire))
			{
				GUIContextMenu.CreateContextMenu(new Vector2?(PlayerInput.MousePosition), foundWire.UsedItemPrefab.Name, new Color?(foundWire.Color), allOptions);
				return;
			}
			GUIContextMenu.CreateContextMenu(allOptions);
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x00055880 File Offset: 0x00053A80
		[return: Nullable(2)]
		public CircuitBoxNode GetTopmostNode(ImmutableHashSet<CircuitBoxNode> nodes)
		{
			CircuitBoxNode foundNode = null;
			ImmutableArray<CircuitBoxNode> allNodes = this.MouseSnapshotHandler.Nodes.ToImmutableArray<CircuitBoxNode>();
			for (int i = allNodes.Length - 1; i >= 0; i--)
			{
				CircuitBoxNode node = allNodes[i];
				if (nodes.Contains(node))
				{
					foundNode = node;
					break;
				}
			}
			return foundNode;
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x000558CB File Offset: 0x00053ACB
		public void AddToGUIUpdateList()
		{
			GUIButton guibutton = this.toggleMenuButton;
			if (guibutton != null)
			{
				guibutton.AddToGUIUpdateList(false, 0);
			}
			GUIFrame guiframe = this.selectedWireFrame;
			if (guiframe == null)
			{
				return;
			}
			guiframe.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x000558F4 File Offset: 0x00053AF4
		// Note: this type is marked as 'beforefieldinit'.
		static CircuitBoxUI()
		{
			Option.UnspecifiedNone none = Option.None;
			CircuitBoxUI.DraggedWire = none;
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x00055BAF File Offset: 0x00053DAF
		[CompilerGenerated]
		private void <CreateGUI>g__ResetCamera|20_6()
		{
			this.camera.TargetPos = Vector2.One;
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x00055BC4 File Offset: 0x00053DC4
		[CompilerGenerated]
		private void <CreateGUI>g__FindInputOutput|20_7(CircuitBoxInputOutputNode.Type type)
		{
			CircuitBoxInputOutputNode input = this.CircuitBox.InputOutputNodes.FirstOrDefault((CircuitBoxInputOutputNode n) => n.NodeType == type);
			if (input == null)
			{
				return;
			}
			this.camera.TargetPos = input.Position;
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x00055C10 File Offset: 0x00053E10
		[CompilerGenerated]
		private void <CreateGUI>g__FindCircuit|20_8()
		{
			CircuitBoxComponent closestComponent = this.CircuitBox.Components.MinBy((CircuitBoxComponent c) => Vector2.DistanceSquared(c.Position, this.camera.Position));
			if (closestComponent == null)
			{
				return;
			}
			this.camera.TargetPos = closestComponent.Position;
		}

		// Token: 0x040004CE RID: 1230
		private readonly Camera camera;

		// Token: 0x040004CF RID: 1231
		private static readonly Vector2 gridSize = new Vector2(128f);

		// Token: 0x040004D0 RID: 1232
		public readonly CircuitBox CircuitBox;

		// Token: 0x040004D1 RID: 1233
		private bool componentMenuOpen;

		// Token: 0x040004D2 RID: 1234
		private float componentMenuOpenState;

		// Token: 0x040004D3 RID: 1235
		[Nullable(2)]
		private GUICustomComponent circuitComponent;

		// Token: 0x040004D4 RID: 1236
		[Nullable(2)]
		private GUIFrame componentMenu;

		// Token: 0x040004D5 RID: 1237
		[Nullable(2)]
		private GUIButton toggleMenuButton;

		// Token: 0x040004D6 RID: 1238
		[Nullable(2)]
		private GUIFrame selectedWireFrame;

		// Token: 0x040004D7 RID: 1239
		[Nullable(2)]
		private GUIListBox componentList;

		// Token: 0x040004D8 RID: 1240
		[Nullable(2)]
		private GUITextBlock inventoryIndicatorText;

		// Token: 0x040004D9 RID: 1241
		[Nullable(2)]
		private readonly Sprite cursorSprite = GUIStyle.CursorSprite[CursorState.Default];

		// Token: 0x040004DA RID: 1242
		[Nullable(0)]
		private Option<RectangleF> selection;

		// Token: 0x040004DB RID: 1243
		private string searchTerm;

		// Token: 0x040004DC RID: 1244
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<CircuitBoxWireRenderer> DraggedWire;

		// Token: 0x040004DD RID: 1245
		public readonly CircuitBoxMouseDragSnapshotHandler MouseSnapshotHandler;

		// Token: 0x040004DE RID: 1246
		public List<CircuitBoxWireRenderer> VirtualWires;

		// Token: 0x040004DF RID: 1247
		private const float lineBaseWidth = 1f;

		// Token: 0x040004E0 RID: 1248
		private static float lineWidth;

		// Token: 0x040004E1 RID: 1249
		private Vector2 cursorPos;

		// Token: 0x02000726 RID: 1830
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040038EC RID: 14572
			[Nullable(0)]
			public static GUIListBox.OnSelectedHandler <0>__SelectWire;
		}
	}
}
