using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C3 RID: 1475
	[NullableContext(1)]
	[Nullable(0)]
	internal class MiniMap : Powered
	{
		// Token: 0x17001744 RID: 5956
		// (get) Token: 0x06005C6A RID: 23658 RVA: 0x002F7A1E File Offset: 0x002F5C1E
		// (set) Token: 0x06005C6B RID: 23659 RVA: 0x002F7A26 File Offset: 0x002F5C26
		private float Zoom
		{
			get
			{
				return this.zoom;
			}
			set
			{
				this.zoom = Math.Clamp(value, 0.5f, 10f);
			}
		}

		// Token: 0x17001745 RID: 5957
		// (get) Token: 0x06005C6C RID: 23660 RVA: 0x002F7A40 File Offset: 0x002F5C40
		private bool IsPortableItemAllowed
		{
			get
			{
				if (this.IsUsableOutsidePlayerSub)
				{
					return true;
				}
				if (this.item.Submarine == null)
				{
					return false;
				}
				Pickable handheldItem = this.item.GetComponent<Pickable>();
				if (handheldItem == null)
				{
					return true;
				}
				Character picker = handheldItem.Picker;
				CharacterTeamType? characterTeamType = (picker != null) ? new CharacterTeamType?(picker.TeamID) : null;
				CharacterTeamType teamID = this.item.Submarine.TeamID;
				return characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null;
			}
		}

		// Token: 0x17001746 RID: 5958
		// (get) Token: 0x06005C6D RID: 23661 RVA: 0x002F7ABA File Offset: 0x002F5CBA
		// (set) Token: 0x06005C6E RID: 23662 RVA: 0x002F7AC2 File Offset: 0x002F5CC2
		[Serialize(false, IsPropertySaveable.No, "If this item is portable, should it be usable outside the player submarine?", "", false)]
		public bool IsUsableOutsidePlayerSub { get; set; }

		// Token: 0x06005C6F RID: 23663 RVA: 0x002F7ACC File Offset: 0x002F5CCC
		private void SetDefaultMode()
		{
			MiniMapMode miniMapMode;
			if (this.EnableHullStatus)
			{
				miniMapMode = MiniMapMode.HullStatus;
			}
			else if (this.EnableElectricalView)
			{
				miniMapMode = MiniMapMode.ElectricalView;
			}
			else if (this.EnableItemFinder)
			{
				miniMapMode = MiniMapMode.ItemFinder;
			}
			else
			{
				miniMapMode = MiniMapMode.None;
			}
			this.currentMode = miniMapMode;
		}

		// Token: 0x06005C70 RID: 23664 RVA: 0x002F7B08 File Offset: 0x002F5D08
		protected override void CreateGUI()
		{
			base.GuiFrame.ClearChildren();
			base.TryCreateDragHandle();
			base.GuiFrame.RectTransform.RelativeOffset = new Vector2(0.05f, 0f);
			base.GuiFrame.CanBeFocused = true;
			GUICustomComponent submarineBack = new GUICustomComponent(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, new Action<SpriteBatch, GUICustomComponent>(this.DrawHUDBack), null);
			GUIFrame paddedContainer = new GUIFrame(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null);
			Vector2 one = Vector2.One;
			RectTransform rectTransform = paddedContainer.RectTransform;
			Anchor anchor = Anchor.Center;
			Pivot? pivot = null;
			Point? point = null;
			Point? minSize = point;
			point = null;
			this.submarineContainer = new GUIFrame(new RectTransform(one, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), null, null);
			GUICustomComponent submarineFront = new GUICustomComponent(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, new Action<SpriteBatch, GUICustomComponent>(this.DrawHUDFront), null)
			{
				CanBeFocused = false
			};
			Vector2 relativeSize = new Vector2(0.5f, 0.15f);
			RectTransform rectTransform2 = paddedContainer.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(relativeSize, rectTransform2, anchor2, pivot2, minSize2, point, ScaleBasis.Normal)
			{
				MaxSize = new Point(int.MaxValue, GUI.IntScale(40f))
			}, true, Anchor.TopLeft)
			{
				CanBeFocused = true
			};
			Vector2 relativeSize2 = new Vector2(0.25f, 1f);
			RectTransform rectTransform3 = buttonLayout.RectTransform;
			Anchor anchor3 = Anchor.TopLeft;
			Pivot? pivot3 = null;
			point = null;
			Point? minSize3 = point;
			point = null;
			GUIButton guibutton = new GUIButton(new RectTransform(relativeSize2, rectTransform3, anchor3, pivot3, minSize3, point, ScaleBasis.Normal), string.Empty, Alignment.Center, "StatusMonitorButton.HullStatus", null);
			guibutton.UserData = MiniMapMode.HullStatus;
			guibutton.Enabled = this.EnableHullStatus;
			guibutton.ToolTip = TextManager.Get("StatusMonitorButton.HullStatus.Tooltip");
			Vector2 relativeSize3 = new Vector2(0.25f, 1f);
			RectTransform rectTransform4 = buttonLayout.RectTransform;
			Anchor anchor4 = Anchor.TopLeft;
			Pivot? pivot4 = null;
			point = null;
			Point? minSize4 = point;
			point = null;
			GUIButton guibutton2 = new GUIButton(new RectTransform(relativeSize3, rectTransform4, anchor4, pivot4, minSize4, point, ScaleBasis.Normal), string.Empty, Alignment.Center, "StatusMonitorButton.ElectricalView", null);
			guibutton2.UserData = MiniMapMode.ElectricalView;
			guibutton2.Enabled = this.EnableElectricalView;
			guibutton2.ToolTip = TextManager.Get("StatusMonitorButton.ElectricalView.Tooltip");
			Vector2 relativeSize4 = new Vector2(0.25f, 1f);
			RectTransform rectTransform5 = buttonLayout.RectTransform;
			Anchor anchor5 = Anchor.TopLeft;
			Pivot? pivot5 = null;
			point = null;
			Point? minSize5 = point;
			point = null;
			this.modeSwitchButtons = ImmutableArray.Create<GUIButton>(guibutton, guibutton2, new GUIButton(new RectTransform(relativeSize4, rectTransform5, anchor5, pivot5, minSize5, point, ScaleBasis.Normal), string.Empty, Alignment.Center, "StatusMonitorButton.ItemFinder", null)
			{
				UserData = MiniMapMode.ItemFinder,
				Enabled = this.EnableItemFinder,
				ToolTip = TextManager.Get("StatusMonitorButton.ItemFinder.Tooltip")
			});
			foreach (GUIButton button in this.modeSwitchButtons)
			{
				button.OnClicked = delegate(GUIButton btn, object o)
				{
					if (o is MiniMapMode)
					{
						MiniMapMode i = (MiniMapMode)o;
						this.currentMode = i;
						this.Zoom = 1f;
						this.mapOffset = Vector2.Zero;
						this.recalculate = true;
						foreach (GUIButton otherButton in this.modeSwitchButtons)
						{
							otherButton.Selected = false;
						}
						btn.Selected = true;
						return true;
					}
					return false;
				};
				object userData = button.UserData;
				if (userData is MiniMapMode)
				{
					MiniMapMode buttonMode = (MiniMapMode)userData;
					button.Selected = (this.currentMode == buttonMode);
				}
			}
			OrderPrefab[] reports = (from o in OrderPrefab.Prefabs
			where o.IsVisibleAsReportButton
			orderby o.Identifier
			select o).ToArray<OrderPrefab>();
			Vector2 relativeSize5 = new Vector2(0.5f, 0.15f);
			RectTransform rectTransform6 = paddedContainer.RectTransform;
			Anchor anchor6 = Anchor.BottomCenter;
			Pivot? pivot6 = null;
			point = null;
			Point? minSize6 = point;
			point = null;
			GUIFrame bottomFrame = new GUIFrame(new RectTransform(relativeSize5, rectTransform6, anchor6, pivot6, minSize6, point, ScaleBasis.Normal)
			{
				MaxSize = new Point(int.MaxValue, GUI.IntScale(40f))
			}, null, null)
			{
				CanBeFocused = false
			};
			Vector2 relativeSize6 = new Vector2(1f);
			RectTransform rectTransform7 = bottomFrame.RectTransform;
			Anchor anchor7 = Anchor.TopLeft;
			Pivot? pivot7 = null;
			point = null;
			Point? minSize7 = point;
			point = null;
			this.reportFrame = new GUILayoutGroup(new RectTransform(relativeSize6, rectTransform7, anchor7, pivot7, minSize7, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			if (reports.Any<OrderPrefab>())
			{
				GameSession gameSession = GameMain.GameSession;
				CrewManager.CreateReportButtons((gameSession != null) ? gameSession.CrewManager : null, this.reportFrame, reports, true);
			}
			Vector2 relativeSize7 = new Vector2(1.5f, 1f);
			RectTransform rectTransform8 = bottomFrame.RectTransform;
			Anchor anchor8 = Anchor.Center;
			Pivot? pivot8 = null;
			point = null;
			Point? minSize8 = point;
			point = null;
			this.searchBarFrame = new GUILayoutGroup(new RectTransform(relativeSize7, rectTransform8, anchor8, pivot8, minSize8, point, ScaleBasis.Normal), true, Anchor.Center)
			{
				Visible = false
			};
			Vector2 relativeSize8 = new Vector2(1f);
			RectTransform rectTransform9 = this.searchBarFrame.RectTransform;
			Anchor anchor9 = Anchor.TopLeft;
			Pivot? pivot9 = null;
			point = null;
			Point? minSize9 = point;
			point = null;
			this.searchBar = new GUITextBox(new RectTransform(relativeSize8, rectTransform9, anchor9, pivot9, minSize9, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null, true, true)
			{
				OnEnterPressed = delegate(GUITextBox box, string text)
				{
					this.SearchItems(text);
					return true;
				}
			};
			Vector2 one2 = Vector2.One;
			RectTransform canvas = GUI.Canvas;
			Anchor anchor10 = Anchor.TopLeft;
			Pivot? pivot10 = null;
			point = null;
			Point? minSize10 = point;
			point = null;
			this.searchAutoComplete = new GUIFrame(new RectTransform(one2, canvas, anchor10, pivot10, minSize10, point, ScaleBasis.Normal), "GUIToolTip", null)
			{
				Visible = false,
				CanBeFocused = false
			};
			this.SetAutoCompletePosition(this.searchAutoComplete, this.searchBar);
			Vector2 one3 = Vector2.One;
			RectTransform rectTransform10 = this.searchAutoComplete.RectTransform;
			Anchor anchor11 = Anchor.TopLeft;
			Pivot? pivot11 = null;
			point = null;
			Point? minSize11 = point;
			point = null;
			GUIListBox listBox = new GUIListBox(new RectTransform(one3, rectTransform10, anchor11, pivot11, minSize11, point, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent component, object o)
				{
					ItemPrefab prefab = o as ItemPrefab;
					if (prefab != null)
					{
						this.searchedPrefab = prefab;
						this.searchBar.TextBlock.Text = prefab.Name;
						this.searchBar.Deselect();
						this.SearchItems(this.searchBar.Text);
					}
					return true;
				}
			};
			List<ItemPrefab> shownItemPrefabs = new List<ItemPrefab>();
			using (IEnumerator<ItemPrefab> enumerator2 = (from prefab in ItemPrefab.Prefabs
			orderby prefab.Name
			select prefab).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ItemPrefab prefab = enumerator2.Current;
					if (!prefab.HideInMenus && !shownItemPrefabs.Any((ItemPrefab ip) => MiniMap.DisplayAsSameItem(ip, prefab)))
					{
						MiniMap.CreateItemFrame(prefab, listBox.Content.RectTransform);
						shownItemPrefabs.Add(prefab);
					}
				}
			}
			this.searchBar.OnDeselected += delegate(GUITextBox sender, Keys key)
			{
				this.searchAutoComplete.Visible = false;
			};
			this.searchBar.OnSelected += delegate(GUITextBox sender, Keys key)
			{
				this.itemsFoundOnSub = (from it in Item.ItemList
				where this.VisibleOnItemFinder(it)
				select it.Prefab).ToImmutableHashSet<ItemPrefab>();
			};
			this.searchBar.OnKeyHit += this.ControlSearchTooltip;
			this.searchBar.OnTextChanged += this.UpdateSearchTooltip;
			Vector2 relativeSize9 = new Vector2(0.13f, 0.13f);
			RectTransform canvas2 = GUI.Canvas;
			Anchor anchor12 = Anchor.TopLeft;
			point = new Point?(new Point(250, 150));
			this.hullInfoFrame = new GUIFrame(new RectTransform(relativeSize9, canvas2, anchor12, null, point, null, ScaleBasis.Normal), "GUIToolTip", null)
			{
				CanBeFocused = false,
				Visible = false
			};
			Vector2 relativeSize10 = new Vector2(0.9f, 0.9f);
			RectTransform rectTransform11 = this.hullInfoFrame.RectTransform;
			Anchor anchor13 = Anchor.Center;
			Pivot? pivot12 = null;
			point = null;
			Point? minSize12 = point;
			point = null;
			GUILayoutGroup hullInfoContainer = new GUILayoutGroup(new RectTransform(relativeSize10, rectTransform11, anchor13, pivot12, minSize12, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			Vector2 relativeSize11 = new Vector2(1f, 0.4f);
			RectTransform rectTransform12 = hullInfoContainer.RectTransform;
			Anchor anchor14 = Anchor.TopLeft;
			Pivot? pivot13 = null;
			point = null;
			Point? minSize13 = point;
			point = null;
			this.tooltipHeader = new GUITextBlock(new RectTransform(relativeSize11, rectTransform12, anchor14, pivot13, minSize13, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null)
			{
				Wrap = true
			};
			Vector2 relativeSize12 = new Vector2(1f, 0.3f);
			RectTransform rectTransform13 = hullInfoContainer.RectTransform;
			Anchor anchor15 = Anchor.TopLeft;
			Pivot? pivot14 = null;
			point = null;
			Point? minSize14 = point;
			point = null;
			this.tooltipFirstLine = new GUITextBlock(new RectTransform(relativeSize12, rectTransform13, anchor15, pivot14, minSize14, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null)
			{
				Wrap = true
			};
			Vector2 relativeSize13 = new Vector2(1f, 0.3f);
			RectTransform rectTransform14 = hullInfoContainer.RectTransform;
			Anchor anchor16 = Anchor.TopLeft;
			Pivot? pivot15 = null;
			point = null;
			Point? minSize15 = point;
			point = null;
			this.tooltipSecondLine = new GUITextBlock(new RectTransform(relativeSize13, rectTransform14, anchor16, pivot15, minSize15, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null)
			{
				Wrap = true
			};
			Vector2 relativeSize14 = new Vector2(1f, 0.3f);
			RectTransform rectTransform15 = hullInfoContainer.RectTransform;
			Anchor anchor17 = Anchor.TopLeft;
			Pivot? pivot16 = null;
			point = null;
			Point? minSize16 = point;
			point = null;
			this.tooltipThirdLine = new GUITextBlock(new RectTransform(relativeSize14, rectTransform15, anchor17, pivot16, minSize16, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null)
			{
				Wrap = true
			};
			this.hullInfoFrame.Children.ForEach(delegate(GUIComponent c)
			{
				c.CanBeFocused = false;
				c.Children.ForEach(delegate(GUIComponent c2)
				{
					c2.CanBeFocused = false;
				});
			});
			RectTransform rectTransform16 = submarineBack.RectTransform;
			RectTransform rectTransform17 = submarineFront.RectTransform;
			RectTransform rectTransform18 = this.submarineContainer.RectTransform;
			Point maxSize = new Point(int.MaxValue, paddedContainer.Rect.Height - bottomFrame.Rect.Height - buttonLayout.Rect.Height);
			rectTransform18.MaxSize = maxSize;
			rectTransform16.MaxSize = (rectTransform17.MaxSize = maxSize);
		}

		// Token: 0x06005C71 RID: 23665 RVA: 0x002F861C File Offset: 0x002F681C
		private static Sprite GetPreviewSprite(ItemPrefab prefab)
		{
			return prefab.InventoryIcon ?? prefab.Sprite;
		}

		// Token: 0x06005C72 RID: 23666 RVA: 0x002F8630 File Offset: 0x002F6830
		private static bool DisplayAsSameItem(ItemPrefab prefab1, ItemPrefab prefab2)
		{
			if (prefab1 == prefab2)
			{
				return true;
			}
			if (!(prefab1.Name == prefab2.Name))
			{
				return false;
			}
			Sprite sprite = MiniMap.GetPreviewSprite(prefab1);
			Sprite sprite2 = MiniMap.GetPreviewSprite(prefab2);
			if (((sprite != null) ? sprite.FullPath : null) == ((sprite2 != null) ? sprite2.FullPath : null))
			{
				Rectangle? rectangle = (sprite != null) ? new Rectangle?(sprite.SourceRect) : null;
				return rectangle == ((sprite2 != null) ? new Rectangle?(sprite2.SourceRect) : null);
			}
			return false;
		}

		// Token: 0x06005C73 RID: 23667 RVA: 0x002F86F0 File Offset: 0x002F68F0
		private bool VisibleOnItemFinder(Item targetItem)
		{
			if (((targetItem != null) ? targetItem.Submarine : null) == null || this.item.Submarine == null)
			{
				return false;
			}
			if (!this.IsPortableItemAllowed)
			{
				return false;
			}
			if (!this.item.Submarine.IsEntityFoundOnThisSub(targetItem, true, false, false))
			{
				return false;
			}
			if (targetItem.NonInteractable || targetItem.IsHidden)
			{
				return false;
			}
			if (targetItem.GetComponent<Pickable>() == null)
			{
				return false;
			}
			Holdable holdable = targetItem.GetComponent<Holdable>();
			if (holdable != null && holdable.Attached)
			{
				return false;
			}
			Wire wire = targetItem.GetComponent<Wire>();
			if (wire != null)
			{
				if (wire.Connections.Any((Connection c) => c != null))
				{
					return false;
				}
			}
			Item container = targetItem.Container;
			if (container != null && container.NonInteractable)
			{
				return false;
			}
			Item container2 = targetItem.Container;
			ItemContainer itemContainer = (container2 != null) ? container2.GetComponent<ItemContainer>() : null;
			bool flag;
			if (itemContainer != null)
			{
				bool drawInventory = itemContainer.DrawInventory;
				if (!drawInventory || !itemContainer.AllowAccess)
				{
					flag = true;
					goto IL_F1;
				}
			}
			flag = false;
			IL_F1:
			return !flag && !targetItem.HasTag(Tags.TraitorMissionItem);
		}

		// Token: 0x06005C74 RID: 23668 RVA: 0x002F8804 File Offset: 0x002F6A04
		public override void AddToGUIUpdateList(int order = 0)
		{
			base.AddToGUIUpdateList(order);
			GUIFrame guiframe = this.hullInfoFrame;
			if (guiframe != null)
			{
				guiframe.AddToGUIUpdateList(false, order + 1);
			}
			if (this.currentMode == MiniMapMode.ItemFinder && this.searchBar.Selected)
			{
				GUIComponent guicomponent = this.searchAutoComplete;
				if (guicomponent == null)
				{
					return;
				}
				guicomponent.AddToGUIUpdateList(false, order + 1);
			}
		}

		// Token: 0x06005C75 RID: 23669 RVA: 0x002F8857 File Offset: 0x002F6A57
		private void ClearHUD()
		{
			this.subEntities.Clear();
			this.submarineContainer.ClearChildren();
			this.displayedSubs.Clear();
		}

		// Token: 0x06005C76 RID: 23670 RVA: 0x002F887C File Offset: 0x002F6A7C
		private void RefreshHUD()
		{
			this.ClearHUD();
			if (this.item.Submarine == null || !this.IsPortableItemAllowed)
			{
				return;
			}
			this.prevResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.scissorComponent = new GUIScissorComponent(new RectTransform(Vector2.One, this.submarineContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal));
			this.miniMapContainer = new GUIFrame(new RectTransform(Vector2.One, this.scissorComponent.Content.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			ImmutableHashSet<Item> hullPointsOfInterest = (from it in Item.ItemList
			where this.item.Submarine.IsEntityFoundOnThisSub(it, true, false, false) && !it.IsHidden && !it.NonInteractable && it.Prefab.ShowInStatusMonitor && (it.GetComponent<Door>() != null || it.GetComponent<Turret>() != null)
			select it).ToImmutableHashSet<Item>();
			this.miniMapFrame = MiniMap.CreateMiniMap(this.item.Submarine, this.submarineContainer, MiniMapSettings.Default, hullPointsOfInterest, out this.hullStatusComponents);
			IEnumerable<Item> electricalPointsOfInterest = from it in Item.ItemList
			where this.item.Submarine.IsEntityFoundOnThisSub(it, true, false, false) && !it.IsHidden && !it.NonInteractable && it.GetComponent<Repairable>() != null
			select it;
			this.electricalFrame = MiniMap.CreateMiniMap(this.item.Submarine, this.miniMapContainer, new MiniMapSettings(false, null), electricalPointsOfInterest, out this.electricalMapComponents);
			Dictionary<MiniMapGUIComponent, GUIComponent> electricChildren = new Dictionary<MiniMapGUIComponent, GUIComponent>();
			foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.electricalMapComponents)
			{
				MapEntity mapEntity;
				MiniMapGUIComponent miniMapGUIComponent;
				keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
				MapEntity entity = mapEntity;
				MiniMapGUIComponent component = miniMapGUIComponent;
				GUIComponent parent = component.RectComponent;
				Item it3 = entity as Item;
				if (it3 != null)
				{
					Sprite sprite = it3.Prefab.UpgradePreviewSprite;
					if (sprite != null)
					{
						GUIImage child = new GUIImage(new RectTransform(Vector2.One, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), sprite, null, GUIImage.ScalingMode.None)
						{
							OutlineColor = MiniMap.ElectricalBaseColor,
							Color = MiniMap.ElectricalBaseColor,
							HoverCursor = CursorState.Hand,
							SpriteEffects = ((this.item.Rotation > 90f && this.item.Rotation < 270f) ? SpriteEffects.FlipVertically : SpriteEffects.None)
						};
						electricChildren.Add(component, child);
					}
				}
			}
			this.electricalChildren = electricChildren.ToImmutableDictionary<MiniMapGUIComponent, GUIComponent>();
			Dictionary<MiniMapGUIComponent, GUIComponent> doorChilds = new Dictionary<MiniMapGUIComponent, GUIComponent>();
			Dictionary<MiniMapGUIComponent, GUIComponent> weaponChilds = new Dictionary<MiniMapGUIComponent, GUIComponent>();
			foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.hullStatusComponents)
			{
				MapEntity mapEntity;
				MiniMapGUIComponent miniMapGUIComponent;
				keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
				MapEntity entity2 = mapEntity;
				MiniMapGUIComponent component2 = miniMapGUIComponent;
				if (hullPointsOfInterest.Contains(entity2))
				{
					Item it2 = entity2 as Item;
					if (it2 != null)
					{
						if (it2.GetComponent<Door>() != null)
						{
							Point size = component2.BorderComponent.Rect.Size;
							size.X = Math.Max(size.X, 8);
							size.Y = Math.Max(size.Y, 8);
							float width = Math.Min(2f, (float)Math.Min(size.X, size.Y) / 8f);
							GUIFrame frame = new GUIFrame(new RectTransform(size, component2.RectComponent.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "ScanLines", new Color?(MiniMap.DoorIndicatorColor))
							{
								OutlineColor = MiniMap.DoorIndicatorColor,
								OutlineThickness = width
							};
							doorChilds.Add(component2, frame);
						}
						else
						{
							Turret turret = it2.GetComponent<Turret>();
							if (turret != null)
							{
								int parentWidth = (int)((float)this.submarineContainer.Rect.Width / 16f);
								GUICustomComponent frame2 = new GUICustomComponent(new RectTransform(new Point(parentWidth, parentWidth), component2.RectComponent.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), delegate(SpriteBatch batch, GUICustomComponent customComponent)
								{
									Vector2 center = customComponent.Center;
									float rotation = turret.Rotation;
									if (!this.hasPower)
									{
										float minRotation = MathHelper.ToRadians(Math.Min(turret.RotationLimits.X, turret.RotationLimits.Y));
										float maxRotation = MathHelper.ToRadians(Math.Max(turret.RotationLimits.X, turret.RotationLimits.Y));
										rotation = (minRotation + maxRotation) / 2f;
									}
									Sprite weaponSprite = turret.WeaponIndicatorSprite;
									if (weaponSprite != null)
									{
										Vector2 origin = weaponSprite.Origin;
										float scale = (float)parentWidth / Math.Max(weaponSprite.size.X, weaponSprite.size.Y);
										Color color = (!this.hasPower) ? MiniMap.NoPowerColor : ((turret.ActiveUser == null) ? Color.DimGray : GUIStyle.Green);
										weaponSprite.Draw(batch, center, color, origin, rotation, scale, SpriteEffects.None, null);
									}
								}, null)
								{
									CanBeFocused = false
								};
								weaponChilds.Add(component2, frame2);
							}
						}
					}
				}
			}
			this.doorChildren = doorChilds.ToImmutableDictionary<MiniMapGUIComponent, GUIComponent>();
			this.weaponChildren = weaponChilds.ToImmutableDictionary<MiniMapGUIComponent, GUIComponent>();
			Rectangle parentRect = this.miniMapFrame.Rect;
			this.displayedSubs.Clear();
			this.displayedSubs.Add(this.item.Submarine);
			this.displayedSubs.AddRange(from s in this.item.Submarine.DockedTo
			where s.TeamID == this.item.Submarine.TeamID
			select s);
			this.subEntities = (from w in MapEntity.MapEntityList.Where(delegate(MapEntity me)
			{
				Submarine sub = this.item.Submarine;
				return sub != null && sub.IsEntityFoundOnThisSub(me, true, false, false) && !me.IsHidden;
			})
			orderby w.SpriteDepth descending
			select w).ToList<MapEntity>();
			this.BakeSubmarine(this.item.Submarine, parentRect);
			this.elementSize = base.GuiFrame.Rect.Size;
		}

		// Token: 0x06005C77 RID: 23671 RVA: 0x002F8DF8 File Offset: 0x002F6FF8
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (this.item.Submarine != null || this.displayedSubs.Count <= 0)
			{
				Submarine itemSub = this.item.Submarine;
				if ((itemSub == null || (this.displayedSubs.Contains(itemSub) && !(from s in itemSub.DockedTo
				where s.TeamID == this.item.Submarine.TeamID
				select s).Any((Submarine s) => !this.displayedSubs.Contains(s) && itemSub.ConnectedDockingPorts[s].IsLocked) && !this.displayedSubs.Any((Submarine s) => s != itemSub && !itemSub.DockedTo.Contains(s)))) && this.IsPortableItemAllowed && this.prevResolution.X == GameMain.GraphicsWidth && this.prevResolution.Y == GameMain.GraphicsHeight && this.submarineContainer.Children.Any<GUIComponent>())
				{
					goto IL_E6;
				}
			}
			this.RefreshHUD();
			IL_E6:
			if (DateTime.Now > this.resetDataTime)
			{
				foreach (MiniMap.HullData hullData in this.hullDatas.Values)
				{
					if (!hullData.Distort)
					{
						if (Timing.TotalTime > hullData.LastOxygenDataTime + 1.0)
						{
							hullData.ReceivedOxygenAmount = null;
						}
						if (Timing.TotalTime > hullData.LastWaterDataTime + 1.0)
						{
							hullData.ReceivedWaterAmount = null;
						}
					}
				}
				this.resetDataTime = DateTime.Now + new TimeSpan(0, 0, 1);
			}
			if (this.cardRefreshTimer > 3f)
			{
				Submarine sub = this.item.Submarine;
				if (sub != null)
				{
					this.UpdateIDCards(sub);
				}
				this.cardRefreshTimer = 0f;
			}
			else
			{
				this.cardRefreshTimer += deltaTime;
			}
			if (this.scissorComponent != null)
			{
				if (PlayerInput.PrimaryMouseButtonDown() && this.currentMode != MiniMapMode.HullStatus && (GUI.MouseOn == this.scissorComponent || this.scissorComponent.IsParentOf(GUI.MouseOn, true)))
				{
					this.dragMapStart = new Vector2?(PlayerInput.MousePosition);
				}
				if (this.currentMode != MiniMapMode.HullStatus && Math.Abs(PlayerInput.ScrollWheelSpeed) > 0 && (GUI.MouseOn == this.scissorComponent || this.scissorComponent.IsParentOf(GUI.MouseOn, true)))
				{
					float newZoom = Math.Clamp(this.Zoom + (float)PlayerInput.ScrollWheelSpeed / 1000f * this.Zoom, 0.5f, 10f);
					float distanceScale = newZoom / this.Zoom;
					this.mapOffset *= distanceScale;
					this.recalculate |= !MathUtils.NearlyEqual(this.Zoom, newZoom, 0.0001f);
					this.Zoom = newZoom;
				}
			}
			Vector2? vector = this.dragMapStart;
			if (vector != null)
			{
				Vector2 dragStart = vector.GetValueOrDefault();
				if (this.dragMap || Vector2.DistanceSquared(dragStart, PlayerInput.MousePosition) > (float)GUI.IntScale(64f))
				{
					this.mapOffset.X = this.mapOffset.X + PlayerInput.MouseSpeed.X;
					this.mapOffset.Y = this.mapOffset.Y + PlayerInput.MouseSpeed.Y;
					this.recalculate = true;
					this.dragMap = true;
				}
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				this.dragMapStart = null;
				this.dragMap = false;
			}
			if (this.recalculate)
			{
				if (this.miniMapContainer != null)
				{
					this.miniMapContainer.RectTransform.LocalScale = new Vector2(this.Zoom);
					this.miniMapContainer.RectTransform.RecalculateChildren(true, true);
					this.miniMapContainer.RectTransform.AbsoluteOffset = this.mapOffset.ToPoint();
				}
				this.recalculate = false;
			}
			if (base.GuiFrame.Rect.Size != this.elementSize)
			{
				this.CreateGUI();
				this.elementSize = base.GuiFrame.Rect.Size;
			}
			float distort = this.item.Repairables.Any((Repairable r) => r.IsBelowRepairThreshold) ? (1f - this.item.Condition / this.item.MaxCondition) : 0f;
			foreach (MiniMap.HullData hullData2 in this.hullDatas.Values)
			{
				hullData2.DistortionTimer -= deltaTime;
				if (hullData2.DistortionTimer <= 0f)
				{
					hullData2.Distort = (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < distort * distort);
					if (hullData2.Distort)
					{
						hullData2.ReceivedOxygenAmount = new float?(Rand.Range(0f, 100f, Rand.RandSync.Unsynced));
						hullData2.ReceivedWaterAmount = new float?(Rand.Range(0f, 100f, Rand.RandSync.Unsynced));
					}
					hullData2.DistortionTimer = Rand.Range(1f, 10f, Rand.RandSync.Unsynced);
				}
			}
			this.UpdateHUDBack();
			if (this.blipState > 1f)
			{
				this.blipState = 0f;
			}
			this.blipState += deltaTime;
			if ((this.currentMode == MiniMapMode.HullStatus && !this.EnableHullStatus) || (this.currentMode == MiniMapMode.ElectricalView && !this.EnableElectricalView) || (this.currentMode == MiniMapMode.ItemFinder && !this.EnableItemFinder))
			{
				this.SetDefaultMode();
			}
			this.modeSwitchButtons[0].Enabled = this.EnableHullStatus;
			this.modeSwitchButtons[1].Enabled = this.EnableElectricalView;
			this.modeSwitchButtons[2].Enabled = this.EnableItemFinder;
		}

		// Token: 0x06005C78 RID: 23672 RVA: 0x002F93F8 File Offset: 0x002F75F8
		private void UpdateIDCards(Submarine sub)
		{
			if (this.hullDatas == null)
			{
				return;
			}
			foreach (MiniMap.HullData data in this.hullDatas.Values)
			{
				data.Cards.Clear();
			}
			foreach (Item it in sub.GetItems(true))
			{
				if (it != null)
				{
					Hull hull = it.CurrentHull;
					if (hull != null)
					{
						IdCard idCard = it.GetComponent<IdCard>();
						if (idCard != null && idCard.TeamID == sub.TeamID && this.hullDatas.ContainsKey(hull))
						{
							this.hullDatas[hull].Cards.Add(idCard);
						}
					}
				}
			}
		}

		// Token: 0x06005C79 RID: 23673 RVA: 0x002F94EC File Offset: 0x002F76EC
		private void DrawHUDFront(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.miniMapFrame == null)
			{
				return;
			}
			if (!this.HasPower)
			{
				Vector2 textSize = GUIStyle.Font.MeasureString(this.noPowerTip, false);
				Vector2 textPos = base.GuiFrame.Rect.Center.ToVector2();
				Color noPowerColor = GUIStyle.Orange * (float)Math.Abs(Math.Sin(Timing.TotalTime));
				GUI.DrawString(spriteBatch, textPos - textSize / 2f, this.noPowerTip, noPowerColor, new Color?(Color.Black * 0.8f), 0, GUIStyle.SubHeadingFont, ForceUpperCase.Inherit);
				return;
			}
			if (this.currentMode == MiniMapMode.HullStatus && this.item.Submarine != null && this.IsPortableItemAllowed)
			{
				Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
				spriteBatch.GraphicsDevice.ScissorRectangle = this.submarineContainer.Rect;
				UISprite value = GUIStyle.UIGlowSolidCircular.Value;
				Sprite sprite = (value != null) ? value.Sprite : null;
				float alpha = (MathF.Sin(this.blipState / 1f * 6.2831855f) + 1.5f) * 0.5f;
				if (sprite != null && this.ShowHullIntegrity)
				{
					Vector2 spriteSize = sprite.size;
					Rectangle worldBorders = this.item.Submarine.GetDockedBorders(false);
					worldBorders.Location += this.item.Submarine.WorldPosition.ToPoint();
					foreach (Gap gap in Gap.GapList)
					{
						if (!gap.IsRoomToRoom && gap.linkedTo.Count != 0 && gap.Submarine == this.item.Submarine && gap.ConnectedDoor == null && !gap.IsHidden)
						{
							RectangleF entityRect = MiniMap.ScaleRectToUI(gap, this.miniMapFrame.Rect, worldBorders);
							Vector2 scale = new Vector2(entityRect.Size.X / spriteSize.X, entityRect.Size.Y / spriteSize.Y) * 2f;
							Color color = ToolBox.GradientLerp(gap.Open, new Color[]
							{
								GUIStyle.HealthBarColorMedium,
								GUIStyle.HealthBarColorLow
							}) * alpha;
							sprite.Draw(spriteBatch, this.miniMapFrame.Rect.Location.ToVector2() + entityRect.Center, color, sprite.Origin, 0f, scale, SpriteEffects.None, null);
						}
					}
				}
				if (this.currentMode == MiniMapMode.HullStatus && this.hullStatusComponents != null)
				{
					foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.hullStatusComponents)
					{
						MapEntity mapEntity;
						MiniMapGUIComponent miniMapGUIComponent;
						keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
						MapEntity entity = mapEntity;
						MiniMapGUIComponent component = miniMapGUIComponent;
						Hull hull = entity as Hull;
						MiniMap.HullData hullData;
						if (hull != null && this.hullDatas.TryGetValue(hull, out hullData) && hullData != null)
						{
							this.DrawHullCards(spriteBatch, hull, hullData, component.RectComponent);
							Hull currentHull = this.item.CurrentHull;
							if (currentHull != null && currentHull == hull)
							{
								UISprite value2 = GUIStyle.YouAreHereCircle.Value;
								Sprite pingCircle = (value2 != null) ? value2.Sprite : null;
								if (pingCircle != null)
								{
									Vector2 charPos = this.item.WorldPosition;
									Vector2 hullPos = hull.WorldRect.Location.ToVector2();
									Vector2 hullSize = hull.WorldRect.Size.ToVector2();
									Vector2 relativePos = (charPos - hullPos) / hullSize * component.RectComponent.Rect.Size.ToVector2();
									relativePos.Y = -relativePos.Y;
									float parentWidth = (float)this.submarineContainer.Rect.Width / 64f;
									float spriteSize2 = pingCircle.size.X * (parentWidth / pingCircle.size.X);
									Vector2 drawPos = component.RectComponent.Rect.Location.ToVector2() + relativePos;
									drawPos -= new Vector2(spriteSize2, spriteSize2) / 2f;
									pingCircle.Draw(spriteBatch, drawPos, GUIStyle.Red * 0.8f, Vector2.Zero, 0f, parentWidth / pingCircle.size.X, SpriteEffects.None, null);
								}
							}
						}
					}
				}
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
		}

		// Token: 0x06005C7A RID: 23674 RVA: 0x002F9A74 File Offset: 0x002F7C74
		private void ControlSearchTooltip(GUITextBox sender, Keys key)
		{
			if (this.searchAutoComplete == null || !this.searchAutoComplete.Visible)
			{
				return;
			}
			GUIListBox listBox = this.searchAutoComplete.GetChild<GUIListBox>();
			if (listBox == null)
			{
				return;
			}
			if (key == Keys.Down)
			{
				listBox.SelectNext(GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
				return;
			}
			if (key == Keys.Up)
			{
				listBox.SelectPrevious(GUIListBox.Force.Yes, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
				return;
			}
			if (key == Keys.Enter)
			{
				GUIListBox.OnSelectedHandler onSelected = listBox.OnSelected;
				if (onSelected != null)
				{
					onSelected(listBox, listBox.SelectedData);
				}
				this.searchBar.Deselect();
			}
		}

		// Token: 0x06005C7B RID: 23675 RVA: 0x002F9AF0 File Offset: 0x002F7CF0
		private bool UpdateSearchTooltip(GUITextBox box, [Nullable(2)] string text)
		{
			if (text == null || this.itemsFoundOnSub == null || this.searchAutoComplete == null)
			{
				return false;
			}
			this.MiniMapBlips = null;
			this.searchedPrefab = null;
			this.searchAutoComplete.Visible = true;
			this.SetAutoCompletePosition(this.searchAutoComplete, box);
			GUIListBox listBox = this.searchAutoComplete.GetChild<GUIListBox>();
			if (((listBox != null) ? listBox.Content : null) == null)
			{
				return false;
			}
			bool first = true;
			int i = 0;
			foreach (GUIComponent component in listBox.Content.Children)
			{
				component.Visible = false;
				object userData = component.UserData;
				ItemPrefab prefab = userData as ItemPrefab;
				if (prefab != null)
				{
					LocalizedString prefabName = prefab.Name;
					if (prefabName != null && (this.itemsFoundOnSub.Contains(prefab) || this.itemsFoundOnSub.Any((ItemPrefab ip) => MiniMap.DisplayAsSameItem(ip, prefab))))
					{
						component.Visible = prefabName.ToLower().Contains(text.ToLower(), StringComparison.Ordinal);
						if (component.Visible && first)
						{
							listBox.Select(i, GUIListBox.Force.Yes, GUIListBox.AutoScroll.Disabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
							first = false;
						}
					}
				}
				i++;
			}
			listBox.BarScroll = 0f;
			listBox.RecalculateChildren();
			return true;
		}

		// Token: 0x06005C7C RID: 23676 RVA: 0x002F9C58 File Offset: 0x002F7E58
		private void SetAutoCompletePosition(GUIComponent tooltip, GUITextBox box)
		{
			int height = base.GuiFrame.Rect.Height / 2;
			tooltip.RectTransform.NonScaledSize = new Point(box.Rect.Width, height);
			tooltip.RectTransform.ScreenSpaceOffset = new Point(box.Rect.X, box.Rect.Y - height);
		}

		// Token: 0x06005C7D RID: 23677 RVA: 0x002F9CBC File Offset: 0x002F7EBC
		private static void CreateItemFrame(ItemPrefab prefab, RectTransform parent)
		{
			Sprite sprite = MiniMap.GetPreviewSprite(prefab);
			if (sprite == null)
			{
				return;
			}
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
			{
				UserData = prefab
			};
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIImage guiimage = new GUIImage(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), sprite, null, GUIImage.ScalingMode.None);
			guiimage.Color = prefab.InventoryIconColor;
			guiimage.UserData = prefab;
			GUITextBlock nameText = new GUITextBlock(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), prefab.Name, null, null, Alignment.Left, false, "", null);
			nameText.RectTransform.SizeChanged += delegate()
			{
				nameText.Text = ToolBox.LimitString(prefab.Name, nameText.Font, nameText.Rect.Width);
			};
		}

		// Token: 0x06005C7E RID: 23678 RVA: 0x002F9E5C File Offset: 0x002F805C
		private void SearchItems(string text)
		{
			if (this.searchedPrefab == null)
			{
				ItemPrefab first = ItemPrefab.Prefabs.FirstOrDefault((ItemPrefab p) => p.Name.ToLower().Equals(text.ToLower(), StringComparison.Ordinal));
				if (first == null)
				{
					this.searchBar.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
					return;
				}
				this.searchedPrefab = first;
			}
			if (this.item.Submarine == null)
			{
				return;
			}
			HashSet<Item> foundItems = new HashSet<Item>();
			foreach (Item it in Item.ItemList)
			{
				if (this.VisibleOnItemFinder(it) && MiniMap.DisplayAsSameItem(it.Prefab, this.searchedPrefab))
				{
					if (it.FindParentInventory(delegate(Inventory inv)
					{
						if (!(inv is CharacterInventory))
						{
							if (inv is ItemInventory)
							{
								Item item = inv.Owner as Item;
								if (item != null)
								{
									return item.IsHidden;
								}
							}
							return false;
						}
						return true;
					}) == null)
					{
						ItemInventory parent = it.FindParentInventory(delegate(Inventory inventory)
						{
							if (inventory is ItemInventory)
							{
								Item item = inventory.Owner as Item;
								if (item != null)
								{
									return item.ParentInventory == null;
								}
							}
							return false;
						}) as ItemInventory;
						if (parent != null)
						{
							foundItems.Add((Item)parent.Owner);
						}
						else
						{
							foundItems.Add(it);
						}
					}
				}
			}
			RectangleF dockedBorders = this.item.Submarine.GetDockedBorders(false);
			dockedBorders.Location += this.item.Submarine.WorldPosition;
			RectangleF parentRect = this.miniMapFrame.Rect;
			HashSet<Vector2> positions = new HashSet<Vector2>();
			foreach (Item foundItem in foundItems)
			{
				RelativeEntityRect scaledRect = new RelativeEntityRect(dockedBorders, foundItem.WorldRect);
				Vector2 pos = scaledRect.PositionRelativeTo(parentRect, true) + scaledRect.SizeRelativeTo(parentRect) / 2f;
				positions.Add(pos);
			}
			this.MiniMapBlips = positions.ToImmutableHashSet<Vector2>();
			MiniMap.HideGUIComponent(this.searchAutoComplete);
		}

		// Token: 0x06005C7F RID: 23679 RVA: 0x002FA0A4 File Offset: 0x002F82A4
		private void UpdateHUDBack()
		{
			this.HideModeSpecificFrames();
			if (this.item.Submarine == null || !this.IsPortableItemAllowed)
			{
				this.ClearHUD();
				return;
			}
			switch (this.currentMode)
			{
			case MiniMapMode.HullStatus:
				this.UpdateHullStatus();
				this.miniMapFrame.Visible = true;
				this.reportFrame.Visible = true;
				return;
			case MiniMapMode.ElectricalView:
				this.UpdateElectricalView();
				this.electricalFrame.Visible = true;
				return;
			case MiniMapMode.ItemFinder:
				this.searchBarFrame.Visible = true;
				return;
			default:
				return;
			}
		}

		// Token: 0x06005C80 RID: 23680 RVA: 0x002FA12D File Offset: 0x002F832D
		private void HideModeSpecificFrames()
		{
			MiniMap.HideGUIComponent(this.hullInfoFrame);
			MiniMap.HideGUIComponent(this.reportFrame);
			MiniMap.HideGUIComponent(this.searchBarFrame);
			MiniMap.HideGUIComponent(this.electricalFrame);
			MiniMap.HideGUIComponent(this.miniMapFrame);
		}

		// Token: 0x06005C81 RID: 23681 RVA: 0x002FA166 File Offset: 0x002F8366
		[NullableContext(2)]
		private static void HideGUIComponent(GUIComponent component)
		{
			if (component != null)
			{
				component.Visible = false;
			}
		}

		// Token: 0x06005C82 RID: 23682 RVA: 0x002FA174 File Offset: 0x002F8374
		private void UpdateHullStatus()
		{
			bool canHoverOverHull = true;
			foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.hullStatusComponents)
			{
				MapEntity mapEntity;
				MiniMapGUIComponent miniMapGUIComponent;
				keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
				MapEntity entity = mapEntity;
				MiniMapGUIComponent component = miniMapGUIComponent;
				if (!(entity is Hull))
				{
					GUIComponent rectComponent = component.RectComponent;
					GUIComponent child;
					if (this.doorChildren.TryGetValue(component, out child) && child != null)
					{
						if (this.item.Submarine == null || !this.hasPower)
						{
							child.Color = (child.OutlineColor = MiniMap.NoPowerDoorColor);
						}
						if (this.HasPower)
						{
							child.Color = (child.OutlineColor = MiniMap.DoorIndicatorColor);
							if (GUI.MouseOn == child)
							{
								this.SetTooltip(rectComponent.Rect.Center, entity.Name, string.Empty, string.Empty, string.Empty, null, null, null);
								canHoverOverHull = false;
								child.Color = (child.OutlineColor = MiniMap.HoverColor);
							}
						}
					}
				}
			}
			foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.hullStatusComponents)
			{
				MapEntity mapEntity;
				MiniMapGUIComponent miniMapGUIComponent;
				keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
				MiniMapGUIComponent miniMapGUIComponent2 = miniMapGUIComponent;
				GUIComponent guicomponent;
				GUIComponent guicomponent2;
				miniMapGUIComponent2.Deconstruct(out guicomponent, out guicomponent2);
				MapEntity entity2 = mapEntity;
				GUIComponent component2 = guicomponent;
				GUIComponent borderComponent = guicomponent2;
				if (this.item.Submarine == null || !this.hasPower)
				{
					component2.Color = (borderComponent.OutlineColor = MiniMap.NoPowerColor);
				}
				if (component2.Visible)
				{
					Hull hull = entity2 as Hull;
					if (hull != null)
					{
						if (!this.submarineContainer.Rect.Contains(component2.Rect) && hull.Submarine.Info.Type != SubmarineType.Player)
						{
							component2.Visible = (borderComponent.Visible = false);
						}
						else if (this.HasPower)
						{
							MiniMap.HullData hullData;
							this.hullDatas.TryGetValue(hull, out hullData);
							if (hullData == null)
							{
								hullData = new MiniMap.HullData();
								hull.GetLinkedHulls(hullData.LinkedHulls, false);
								this.hullDatas.Add(hull, hullData);
							}
							Color neutralColor = MiniMap.DefaultNeutralColor;
							Color borderColor = neutralColor;
							if (hull.IsWetRoom)
							{
								neutralColor = MiniMap.WetHullColor;
							}
							if (hullData.Distort)
							{
								borderComponent.OutlineColor = neutralColor * 0.5f;
								component2.Color = Color.Lerp(Color.Black, Color.DarkGray * 0.5f, Rand.Range(0f, 1f, Rand.RandSync.Unsynced));
							}
							else
							{
								if (this.RequireOxygenDetectors)
								{
									hullData.HullOxygenAmount = hullData.ReceivedOxygenAmount;
								}
								else if (hullData.LinkedHulls.Any<Hull>())
								{
									hullData.HullOxygenAmount = new float?(0f);
									foreach (Hull linkedHull in hullData.LinkedHulls)
									{
										hullData.HullOxygenAmount += linkedHull.OxygenPercentage;
									}
									hullData.HullOxygenAmount /= (float)hullData.LinkedHulls.Count;
								}
								else
								{
									hullData.HullOxygenAmount = new float?(hull.OxygenPercentage);
								}
								if (this.RequireWaterDetectors)
								{
									hullData.HullWaterAmount = hullData.ReceivedWaterAmount;
								}
								else if (hullData.LinkedHulls.Any<Hull>())
								{
									float waterVolume = 0f;
									float totalVolume = 0f;
									foreach (Hull linkedHull2 in hullData.LinkedHulls)
									{
										if ((float)WaterDetector.GetWaterPercentage(linkedHull2) > 0f)
										{
											waterVolume += linkedHull2.WaterVolume;
										}
										totalVolume += linkedHull2.Volume;
									}
									hullData.HullWaterAmount = new float?((waterVolume > 1f) ? ((float)MathHelper.Clamp((int)Math.Ceiling((double)(waterVolume / totalVolume * 100f)), 0, 100)) : 0f);
								}
								else
								{
									hullData.HullWaterAmount = new float?((float)WaterDetector.GetWaterPercentage(hull));
								}
								float gapOpenSum = 0f;
								if (this.ShowHullIntegrity)
								{
									float amount = 1f + (float)hullData.LinkedHulls.Count;
									gapOpenSum = (from g in hull.ConnectedGaps.Concat(hullData.LinkedHulls.SelectMany((Hull h) => h.ConnectedGaps))
									where g.linkedTo.Count == 1 && !g.IsHidden
									select g).Sum((Gap g) => g.Open) / amount;
									borderColor = Color.Lerp(neutralColor, GUIStyle.Red, Math.Min(gapOpenSum, 1f));
								}
								bool isHoveringOver = canHoverOverHull && GUI.MouseOn == component2;
								if (isHoveringOver)
								{
									LocalizedString header = hull.DisplayName;
									float? oxygenAmount = hullData.HullOxygenAmount;
									float? waterAmount = hullData.HullWaterAmount;
									LocalizedString line = (gapOpenSum > 0.1f) ? TextManager.Get("MiniMapHullBreach") : string.Empty;
									Color line1Color = GUIStyle.Red;
									LocalizedString line2 = (oxygenAmount == null) ? TextManager.Get("MiniMapAirQualityUnavailable") : TextManager.AddPunctuation(':', new LocalizedString[]
									{
										TextManager.Get("MiniMapAirQuality"),
										((int)Math.Round((double)oxygenAmount.Value)).ToString() + "%"
									});
									Color line2Color = (oxygenAmount == null) ? GUIStyle.Red : Color.Lerp(GUIStyle.Red, Color.LightGreen, oxygenAmount.Value / 100f);
									LocalizedString line3 = (waterAmount == null) ? TextManager.Get("MiniMapWaterLevelUnavailable") : TextManager.AddPunctuation(':', new LocalizedString[]
									{
										TextManager.Get("MiniMapWaterLevel"),
										((int)Math.Round((double)waterAmount.Value)).ToString() + "%"
									});
									Color line3Color = (waterAmount == null) ? GUIStyle.Red : Color.Lerp(Color.LightGreen, GUIStyle.Red, waterAmount.Value / 100f);
									this.SetTooltip(borderComponent.Rect.Center, header, line, line2, line3, new Color?(line1Color), new Color?(line2Color), new Color?(line3Color));
								}
								GameSession gameSession = GameMain.GameSession;
								object obj;
								if (gameSession == null)
								{
									obj = null;
								}
								else
								{
									CrewManager crewManager = gameSession.CrewManager;
									obj = ((crewManager != null) ? crewManager.DraggedOrderPrefab : null);
								}
								bool draggingReport = obj != null;
								foreach (Hull linkedHull3 in hullData.LinkedHulls)
								{
									if (this.hullStatusComponents.ContainsKey(linkedHull3))
									{
										isHoveringOver |= (canHoverOverHull && (this.hullStatusComponents[linkedHull3].RectComponent == GUI.MouseOn || (draggingReport && this.hullStatusComponents[linkedHull3].RectComponent.MouseRect.Contains(PlayerInput.MousePosition))));
										if (isHoveringOver)
										{
											break;
										}
									}
								}
								Color componentColor;
								if (isHoveringOver || (draggingReport && component2.MouseRect.Contains(PlayerInput.MousePosition)))
								{
									borderColor = Color.Lerp(borderColor, Color.White, 0.5f);
									componentColor = MiniMap.HoverColor;
								}
								else
								{
									componentColor = neutralColor * 0.8f;
								}
								borderComponent.OutlineColor = borderColor;
								component2.Color = componentColor;
							}
						}
					}
				}
			}
		}

		// Token: 0x06005C83 RID: 23683 RVA: 0x002FAA6C File Offset: 0x002F8C6C
		private void UpdateElectricalView()
		{
			foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.electricalMapComponents)
			{
				MapEntity mapEntity;
				MiniMapGUIComponent miniMapGUIComponent;
				keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
				MapEntity entity = mapEntity;
				MiniMapGUIComponent miniMapGuiComponent = miniMapGUIComponent;
				Item it = entity as Item;
				GUIComponent component;
				if (it != null && this.electricalChildren.TryGetValue(miniMapGuiComponent, out component))
				{
					if (entity.Removed)
					{
						component.Visible = false;
					}
					else
					{
						if (this.item.Submarine == null || !this.hasPower)
						{
							component.Color = (component.OutlineColor = MiniMap.NoPowerElectricalColor);
						}
						if (this.HasPower && miniMapGuiComponent.RectComponent.Visible)
						{
							int durability = (int)(it.Condition / (it.MaxCondition / it.MaxRepairConditionMultiplier) * 100f);
							Color color = ToolBox.GradientLerp((float)durability / 100f, new Color[]
							{
								GUIStyle.Red,
								GUIStyle.Orange,
								GUIStyle.Green,
								GUIStyle.Green
							});
							if (GUI.MouseOn == component)
							{
								LocalizedString line = string.Empty;
								LocalizedString line2 = string.Empty;
								PowerContainer battery = it.GetComponent<PowerContainer>();
								if (battery != null)
								{
									line2 = TextManager.GetWithVariable("statusmonitor.battery.tooltip", "[amount]", ((int)(battery.Charge / battery.GetCapacity() * 100f)).ToString(), FormatCapitals.No);
								}
								else
								{
									PowerTransfer powerTransfer = it.GetComponent<PowerTransfer>();
									if (powerTransfer != null)
									{
										int current = 0;
										int load = 0;
										if (powerTransfer.PowerConnections.Count > 0 && powerTransfer.PowerConnections[0].Grid != null)
										{
											current = (int)powerTransfer.PowerConnections[0].Grid.Power;
											load = (int)powerTransfer.PowerConnections[0].Grid.Load;
										}
										line = TextManager.GetWithVariable("statusmonitor.junctionpower.tooltip", "[amount]", current.ToString(), FormatCapitals.No).Fallback(TextManager.GetWithVariable("statusmonitor.junctioncurrent.tooltip", "[amount]", current.ToString(), FormatCapitals.No), true);
										line2 = TextManager.GetWithVariables("statusmonitor.junctionload.tooltip", new ValueTuple<string, string>[]
										{
											new ValueTuple<string, string>("[amount]", load.ToString()),
											new ValueTuple<string, string>("[load]", load.ToString())
										});
									}
								}
								LocalizedString line3 = TextManager.GetWithVariable("statusmonitor.durability.tooltip", "[amount]", durability.ToString(), FormatCapitals.No);
								Point center = component.Rect.Center;
								LocalizedString name = it.Prefab.Name;
								LocalizedString line4 = line;
								LocalizedString line5 = line2;
								LocalizedString line6 = line3;
								Color? line3Color = new Color?(color);
								this.SetTooltip(center, name, line4, line5, line6, null, null, line3Color);
								color = MiniMap.HoverColor;
							}
							component.Color = (component.OutlineColor = color);
						}
					}
				}
			}
		}

		// Token: 0x06005C84 RID: 23684 RVA: 0x002FADB4 File Offset: 0x002F8FB4
		private void DrawHUDBack(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.item.Submarine == null || !this.IsPortableItemAllowed)
			{
				return;
			}
			this.DrawSubmarine(spriteBatch);
			if (!this.HasPower)
			{
				return;
			}
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			spriteBatch.GraphicsDevice.ScissorRectangle = this.submarineContainer.Rect;
			if (this.currentMode == MiniMapMode.ItemFinder)
			{
				if (this.MiniMapBlips == null)
				{
					goto IL_3EF;
				}
				using (ImmutableHashSet<Vector2>.Enumerator enumerator = this.MiniMapBlips.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Vector2 blip = enumerator.Current;
						Vector2 parentSize = this.miniMapFrame.Rect.Size.ToVector2();
						UISprite value = GUIStyle.PingCircle.Value;
						Sprite pingCircle = (value != null) ? value.Sprite : null;
						if (pingCircle != null)
						{
							Vector2 targetSize = new Vector2(parentSize.X / 4f);
							Vector2 spriteScale = targetSize / pingCircle.size;
							float scale = Math.Min(this.blipState, 0.5f);
							float alpha = 1f - Math.Clamp((this.blipState - 0.25f) * 2f, 0f, 1f);
							pingCircle.Draw(spriteBatch, this.electricalFrame.Rect.Location.ToVector2() + blip * this.Zoom, GUIStyle.Red * alpha, pingCircle.Origin, 0f, spriteScale * scale, SpriteEffects.None, null);
						}
					}
					goto IL_3EF;
				}
			}
			bool hullsVisible = this.currentMode == MiniMapMode.HullStatus && this.item.Submarine != null;
			if (this.hullStatusComponents != null)
			{
				foreach (KeyValuePair<MapEntity, MiniMapGUIComponent> keyValuePair in this.hullStatusComponents)
				{
					MapEntity mapEntity;
					MiniMapGUIComponent miniMapGUIComponent;
					keyValuePair.Deconstruct(out mapEntity, out miniMapGUIComponent);
					MapEntity entity = mapEntity;
					MiniMapGUIComponent component = miniMapGUIComponent;
					Hull hull = entity as Hull;
					MiniMap.HullData hullData;
					if (hull != null && this.hullDatas.TryGetValue(hull, out hullData) && hullData != null && !hullData.Distort)
					{
						GUIComponent hullFrame = component.RectComponent;
						if (hullsVisible)
						{
							float? num = hullData.HullWaterAmount;
							if (num != null)
							{
								float waterAmount = num.GetValueOrDefault();
								if (!this.RequireWaterDetectors)
								{
									waterAmount = (float)WaterDetector.GetWaterPercentage(hull);
								}
								waterAmount /= 100f;
								if ((float)hullFrame.Rect.Height * waterAmount > 1f)
								{
									RectangleF waterRect = new RectangleF((float)hullFrame.Rect.X, (float)hullFrame.Rect.Y + (float)hullFrame.Rect.Height * (1f - waterAmount), (float)hullFrame.Rect.Width, (float)hullFrame.Rect.Height * waterAmount);
									GUI.DrawFilledRectangle(spriteBatch, waterRect, MiniMap.HullWaterColor, 0f);
									if (!MathUtils.NearlyEqual(waterAmount, 1f, 0.0001f))
									{
										Vector2 offset = new Vector2(0f, 1f);
										GUI.DrawLine(spriteBatch, waterRect.Location + offset, new Vector2(waterRect.Right, waterRect.Y) + offset, MiniMap.HullWaterLineColor, 0f, 1f);
									}
								}
							}
						}
						if (hullsVisible)
						{
							float? num = hullData.HullOxygenAmount;
							if (num != null)
							{
								float oxygenAmount = num.GetValueOrDefault();
								GUI.DrawRectangle(spriteBatch, hullFrame.Rect, Color.Lerp(GUIStyle.Red * 0.5f, GUIStyle.Green * 0.3f, oxygenAmount / 100f), true, 0f, 1f);
							}
						}
					}
				}
			}
			IL_3EF:
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
		}

		// Token: 0x06005C85 RID: 23685 RVA: 0x002FB214 File Offset: 0x002F9414
		private void SetTooltip(Point pos, LocalizedString header, LocalizedString line1, LocalizedString line2, LocalizedString line3, Color? line1Color = null, Color? line2Color = null, Color? line3Color = null)
		{
			if (this.hullInfoFrame == null)
			{
				return;
			}
			this.hullInfoFrame.RectTransform.ScreenSpaceOffset = pos;
			if (this.hullInfoFrame.Rect.Left > this.submarineContainer.Rect.Right)
			{
				this.hullInfoFrame.RectTransform.ScreenSpaceOffset = new Point(this.submarineContainer.Rect.Right, this.hullInfoFrame.RectTransform.ScreenSpaceOffset.Y);
			}
			if (this.hullInfoFrame.Rect.Top > this.submarineContainer.Rect.Bottom)
			{
				this.hullInfoFrame.RectTransform.ScreenSpaceOffset = new Point(this.hullInfoFrame.RectTransform.ScreenSpaceOffset.X, this.submarineContainer.Rect.Bottom);
			}
			if (this.hullInfoFrame.Rect.Right > GameMain.GraphicsWidth)
			{
				this.hullInfoFrame.RectTransform.ScreenSpaceOffset -= new Point(this.hullInfoFrame.Rect.Width, 0);
			}
			if (this.hullInfoFrame.Rect.Bottom > GameMain.GraphicsHeight)
			{
				this.hullInfoFrame.RectTransform.ScreenSpaceOffset -= new Point(0, this.hullInfoFrame.Rect.Height);
			}
			this.hullInfoFrame.Visible = true;
			this.tooltipHeader.Text = header;
			this.tooltipFirstLine.Text = line1;
			this.tooltipFirstLine.TextColor = (line1Color ?? GUIStyle.TextColorNormal);
			this.tooltipSecondLine.Text = line2;
			this.tooltipSecondLine.TextColor = (line2Color ?? GUIStyle.TextColorNormal);
			this.tooltipThirdLine.Text = line3;
			this.tooltipThirdLine.TextColor = (line3Color ?? GUIStyle.TextColorNormal);
		}

		// Token: 0x06005C86 RID: 23686 RVA: 0x002FB46C File Offset: 0x002F966C
		private void BakeSubmarine(Submarine sub, Rectangle container)
		{
			Texture2D texture2D = this.submarinePreview;
			if (texture2D != null)
			{
				texture2D.Dispose();
			}
			Rectangle parentRect = new Rectangle(container.X, container.Y, container.Width, container.Height);
			parentRect.Inflate(128, 128);
			RenderTarget2D rt = new RenderTarget2D(GameMain.Instance.GraphicsDevice, parentRect.Width, parentRect.Height, false, SurfaceFormat.Color, DepthFormat.None);
			using (SpriteBatch spriteBatch = new SpriteBatch(GameMain.Instance.GraphicsDevice))
			{
				GameMain.Instance.GraphicsDevice.SetRenderTarget(rt);
				GameMain.Instance.GraphicsDevice.Clear(Color.Transparent);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
				Rectangle worldBorders = sub.GetDockedBorders(false);
				worldBorders.Location += sub.WorldPosition.ToPoint();
				parentRect.Inflate(-128, -128);
				foreach (MapEntity entity in this.subEntities)
				{
					Structure wall = entity as Structure;
					if (wall != null)
					{
						if (wall.IsPlatform)
						{
							continue;
						}
						MiniMap.DrawStructure(spriteBatch, wall, parentRect, worldBorders, 128);
					}
					Item it = entity as Item;
					if (it != null && it.GetComponent<Pickable>() == null && it.ParentInventory == null)
					{
						MiniMap.DrawItem(spriteBatch, it, parentRect, worldBorders, 128);
					}
				}
				spriteBatch.End();
				GameMain.Instance.GraphicsDevice.SetRenderTarget(null);
				this.submarinePreview = rt;
			}
		}

		// Token: 0x06005C87 RID: 23687 RVA: 0x002FB640 File Offset: 0x002F9840
		private void DrawSubmarine(SpriteBatch spriteBatch)
		{
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.End();
			Texture2D texture = this.submarinePreview;
			if (texture != null)
			{
				GUIComponent mapContainer = this.miniMapContainer;
				if (mapContainer != null)
				{
					SpriteSortMode sortMode = SpriteSortMode.Deferred;
					SamplerState samplerState = GUI.SamplerState;
					BlendState nonPremultiplied = BlendState.NonPremultiplied;
					SamplerState samplerState2 = samplerState;
					DepthStencilState depthStencilState = null;
					Effect blueprintEffect = GameMain.GameScreen.BlueprintEffect;
					spriteBatch.Begin(sortMode, nonPremultiplied, samplerState2, depthStencilState, GameMain.ScissorTestEnable, blueprintEffect, null);
					spriteBatch.GraphicsDevice.ScissorRectangle = this.submarineContainer.Rect;
					GameMain.GameScreen.BlueprintEffect.Parameters["width"].SetValue((float)texture.Width);
					GameMain.GameScreen.BlueprintEffect.Parameters["height"].SetValue((float)texture.Height);
					MiniMapMode miniMapMode = this.currentMode;
					float scale2;
					if (miniMapMode != MiniMapMode.HullStatus)
					{
						if (miniMapMode != MiniMapMode.ElectricalView)
						{
							scale2 = 0.5f;
						}
						else
						{
							scale2 = 0.1f;
						}
					}
					else
					{
						scale2 = 0.1f;
					}
					Color blueprintBlue = MiniMap.BlueprintBlue * scale2;
					Vector2 origin = new Vector2((float)texture.Width / 2f, (float)texture.Height / 2f);
					float scale = (this.currentMode == MiniMapMode.HullStatus) ? 1f : this.Zoom;
					spriteBatch.Draw(texture, mapContainer.Center, null, blueprintBlue, 0f, origin, scale, SpriteEffects.None, 0f);
					spriteBatch.End();
				}
			}
			spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
		}

		// Token: 0x06005C88 RID: 23688 RVA: 0x002FB7D8 File Offset: 0x002F99D8
		private static void DrawItem(ISpriteBatch spriteBatch, Item item, Rectangle parent, Rectangle border, int inflate)
		{
			MiniMap.<>c__DisplayClass92_0 CS$<>8__locals1;
			CS$<>8__locals1.item = item;
			CS$<>8__locals1.parent = parent;
			CS$<>8__locals1.border = border;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.inflate = inflate;
			Sprite sprite = CS$<>8__locals1.item.Sprite;
			if (sprite == null)
			{
				return;
			}
			RectangleF entityRect = MiniMap.ScaleRectToUI(CS$<>8__locals1.item, CS$<>8__locals1.parent, CS$<>8__locals1.border);
			Vector2 spriteScale = new Vector2(entityRect.Size.X / sprite.size.X, entityRect.Size.Y / sprite.size.Y);
			Vector2 origin = new Vector2(sprite.Origin.X * spriteScale.X, sprite.Origin.Y * spriteScale.Y);
			if (!CS$<>8__locals1.item.Prefab.ShowInStatusMonitor)
			{
				Turret turret = CS$<>8__locals1.item.GetComponent<Turret>();
				if (turret != null)
				{
					Vector2 drawPos = turret.GetDrawPos();
					drawPos.Y = -drawPos.Y;
					Sprite barrelSprite = turret.BarrelSprite;
					if (barrelSprite != null)
					{
						MiniMap.<DrawItem>g__DrawAdditionalSprite|92_0(drawPos, barrelSprite, turret.Rotation + 1.5707964f, ref CS$<>8__locals1);
					}
				}
			}
			Vector2 pos = entityRect.Location + origin;
			pos.X += (float)CS$<>8__locals1.inflate;
			pos.Y += (float)CS$<>8__locals1.inflate;
			sprite.Draw(CS$<>8__locals1.spriteBatch, pos, CS$<>8__locals1.item.SpriteColor, sprite.Origin, CS$<>8__locals1.item.RotationRad, spriteScale, CS$<>8__locals1.item.SpriteEffects, null);
		}

		// Token: 0x06005C89 RID: 23689 RVA: 0x002FB978 File Offset: 0x002F9B78
		private static void DrawStructure(ISpriteBatch spriteBatch, Structure structure, Rectangle parent, Rectangle border, int inflate)
		{
			Sprite sprite = structure.Sprite;
			if (sprite == null)
			{
				return;
			}
			Vector2 textureOffset = structure.TextureOffset;
			textureOffset = new Vector2(MathUtils.PositiveModulo(-textureOffset.X, (float)sprite.SourceRect.Width * structure.TextureScale.X * structure.Scale), MathUtils.PositiveModulo(-textureOffset.Y, (float)sprite.SourceRect.Height * structure.TextureScale.Y * structure.Scale));
			RectangleF entityRect = MiniMap.ScaleRectToUI(structure, parent, border);
			Vector2 spriteScale = new Vector2(entityRect.Size.X / (float)structure.Rect.Width, entityRect.Size.Y / (float)structure.Rect.Height);
			float rotation = MathHelper.ToRadians(structure.Rotation);
			Sprite sprite2 = sprite;
			Vector2 position = entityRect.Location + entityRect.Size * 0.5f + new ValueTuple<float, float>((float)inflate, (float)inflate);
			Vector2 size = entityRect.Size;
			float rotation2 = rotation;
			Vector2? origin = new Vector2?(entityRect.Size * 0.5f);
			Color? color = new Color?(structure.SpriteColor);
			Vector2? startOffset = new Vector2?(textureOffset * spriteScale);
			Vector2? textureScale = new Vector2?(structure.TextureScale * structure.Scale * spriteScale);
			float? depth = new float?(structure.SpriteDepth);
			sprite2.DrawTiled(spriteBatch, position, size, sprite.effects ^ structure.SpriteEffects, rotation2, origin, color, startOffset, textureScale, depth);
		}

		// Token: 0x06005C8A RID: 23690 RVA: 0x002FBB08 File Offset: 0x002F9D08
		private static RectangleF ScaleRectToUI(MapEntity entity, RectangleF parentRect, RectangleF worldBorders)
		{
			return MiniMap.ScaleRectToUI(entity.WorldRect, parentRect, worldBorders);
		}

		// Token: 0x06005C8B RID: 23691 RVA: 0x002FBB1C File Offset: 0x002F9D1C
		private static RectangleF ScaleRectToUI(RectangleF rect, RectangleF parentRect, RectangleF worldBorders)
		{
			RelativeEntityRect relativeRect = new RelativeEntityRect(worldBorders, rect);
			return relativeRect.RectangleRelativeTo(parentRect, true);
		}

		// Token: 0x06005C8C RID: 23692 RVA: 0x002FBB3C File Offset: 0x002F9D3C
		private void DrawHullCards(SpriteBatch spriteBatch, Hull hull, MiniMap.HullData data, GUIComponent frame)
		{
			this.cardsToDraw.Clear();
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			if (crewManager != null)
			{
				List<CrewManager.ActiveOrder> orders = crewManager.ActiveOrders;
				if (orders != null)
				{
					foreach (CrewManager.ActiveOrder activeOrder in orders)
					{
						Order order = activeOrder.Order;
						if (order != null && order.SymbolSprite != null && order.TargetEntity is Hull && order.TargetEntity == hull)
						{
							this.cardsToDraw.Add(new MiniMapSprite(order));
						}
					}
				}
			}
			foreach (IdCard card in data.Cards)
			{
				JobPrefab job = card.OwnerJob;
				if (job != null && job.Icon != null)
				{
					this.cardsToDraw.Add(new MiniMapSprite(job));
				}
			}
			if (!this.cardsToDraw.Any<MiniMapSprite>())
			{
				return;
			}
			float num;
			float num2;
			frame.Center.Deconstruct(out num, out num2);
			float centerX = num;
			float centerY = num2;
			float totalWidth = 0f;
			float parentWidth = (float)this.submarineContainer.Rect.Width / 24f;
			int i = 0;
			foreach (MiniMapSprite info in this.cardsToDraw)
			{
				if (info.Sprite != null)
				{
					float spriteSize = info.Sprite.size.X * (parentWidth / info.Sprite.size.X) + 8f;
					if (totalWidth + spriteSize > (float)frame.Rect.Width)
					{
						break;
					}
					totalWidth += spriteSize;
					i++;
				}
			}
			if (i > 0)
			{
				totalWidth -= 8f;
			}
			float adjustedCenterX = centerX - totalWidth / 2f;
			float offset = 0f;
			int amount = 0;
			foreach (MiniMapSprite info2 in this.cardsToDraw)
			{
				Sprite sprite = info2.Sprite;
				if (sprite != null)
				{
					float scale = parentWidth / sprite.size.X;
					float spriteSize2 = sprite.size.X * scale;
					float posX = adjustedCenterX + offset;
					if (posX + spriteSize2 > (float)(frame.Rect.X + frame.Rect.Width) && amount > 0)
					{
						int amountLeft = this.cardsToDraw.Count - amount;
						if (amountLeft > 0)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
							defaultInterpolatedStringHandler.AppendLiteral("+");
							defaultInterpolatedStringHandler.AppendFormatted<int>(amountLeft);
							string text = defaultInterpolatedStringHandler.ToStringAndClear();
							GUIStyle.SubHeadingFont.MeasureString(text, false).Deconstruct(out num2, out num);
							float sizeX = num2;
							float sizeY = num;
							float maxWidth = Math.Max(sizeX, sizeY);
							Vector2 drawPos = new Vector2((float)frame.Rect.Right - sizeX, (float)frame.Rect.Y - sizeY / 2f);
							UISprite icon = GUIStyle.IconOverflowIndicator;
							if (icon != null)
							{
								icon.Draw(spriteBatch, new Rectangle((int)drawPos.X - 4, (int)drawPos.Y - 4, (int)maxWidth + 8, (int)maxWidth + 8), Color.White, SpriteEffects.None, null);
							}
							Vector2 pos2 = drawPos;
							string text2 = text;
							Color color = GUIStyle.TextColorNormal;
							GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
							GUI.DrawString(spriteBatch, pos2, text2, color, null, 0, subHeadingFont, ForceUpperCase.Inherit);
							break;
						}
						break;
					}
					else
					{
						float halfSize = spriteSize2 / 2f;
						if (i > 0)
						{
							offset += halfSize;
						}
						Vector2 pos = new Vector2(adjustedCenterX + offset, centerY);
						Sprite sprite2 = sprite;
						Vector2 pos3 = pos;
						Color color2 = info2.Color * 0.8f;
						num = scale;
						sprite2.Draw(spriteBatch, pos3, color2, sprite.size / 2f, 0f, num, SpriteEffects.None, null);
						offset += halfSize + 8f;
						amount++;
					}
				}
			}
		}

		// Token: 0x06005C8D RID: 23693 RVA: 0x002FBFC4 File Offset: 0x002FA1C4
		public static GUIFrame CreateMiniMap(Submarine sub, GUIComponent parent, MiniMapSettings settings)
		{
			ImmutableDictionary<MapEntity, MiniMapGUIComponent> immutableDictionary;
			return MiniMap.CreateMiniMap(sub, parent, settings, null, out immutableDictionary);
		}

		// Token: 0x06005C8E RID: 23694 RVA: 0x002FBFDC File Offset: 0x002FA1DC
		public static GUIFrame CreateMiniMap(Submarine sub, GUIComponent parent, MiniMapSettings settings, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<MapEntity> pointsOfInterest, out ImmutableDictionary<MapEntity, MiniMapGUIComponent> elements)
		{
			MiniMap.<>c__DisplayClass98_0 CS$<>8__locals1 = new MiniMap.<>c__DisplayClass98_0();
			CS$<>8__locals1.sub = sub;
			if (settings.Equals(default(MiniMapSettings)))
			{
				throw new ArgumentException("Provided MiniMapSettings is not valid, did you mean MiniMapSettings.Default?", "settings");
			}
			Dictionary<MapEntity, MiniMapGUIComponent> pointsOfInterestCollection = new Dictionary<MapEntity, MiniMapGUIComponent>();
			CS$<>8__locals1.worldBorders = CS$<>8__locals1.sub.GetDockedBorders(false);
			MiniMap.<>c__DisplayClass98_0 CS$<>8__locals2 = CS$<>8__locals1;
			CS$<>8__locals2.worldBorders.Location = CS$<>8__locals2.worldBorders.Location + CS$<>8__locals1.sub.WorldPosition;
			float aspectRatio = CS$<>8__locals1.worldBorders.Width / CS$<>8__locals1.worldBorders.Height;
			float parentAspectRatio = (float)parent.Rect.Width / (float)parent.Rect.Height;
			Vector2 containerScale = (parentAspectRatio > aspectRatio) ? new Vector2(aspectRatio / parentAspectRatio, 1f) : new Vector2(1f, parentAspectRatio / aspectRatio);
			CS$<>8__locals1.hullContainer = new GUIFrame(new RectTransform(containerScale * 0.9f, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			CS$<>8__locals1.connectedSubs = (from s in CS$<>8__locals1.sub.GetConnectedSubs()
			where s.TeamID == CS$<>8__locals1.sub.TeamID
			select s).ToImmutableHashSet<Submarine>();
			ImmutableArray<Hull> hullList = ImmutableArray<Hull>.Empty;
			CS$<>8__locals1.combinedHulls = ImmutableDictionary<Hull, ImmutableArray<Hull>>.Empty;
			if (settings.CreateHullElements)
			{
				hullList = Hull.HullList.Where(new Func<Hull, bool>(CS$<>8__locals1.<CreateMiniMap>g__IsPartofSub|1)).ToImmutableArray<Hull>();
				CS$<>8__locals1.combinedHulls = MiniMap.CombinedHulls(hullList);
			}
			foreach (Hull hull in hullList.Where(new Func<Hull, bool>(CS$<>8__locals1.<CreateMiniMap>g__IsStandaloneHull|2)))
			{
				RelativeEntityRect relativeRect = new RelativeEntityRect(CS$<>8__locals1.worldBorders, hull.WorldRect);
				GUIFrame hullFrame = new GUIFrame(new RectTransform(relativeRect.RelativeSize, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = relativeRect.RelativePosition
				}, "ScanLines", new Color?(settings.ElementColor))
				{
					OutlineColor = settings.ElementColor,
					OutlineThickness = 2f,
					UserData = hull
				};
				pointsOfInterestCollection.Add(hull, new MiniMapGUIComponent(hullFrame));
			}
			foreach (KeyValuePair<Hull, ImmutableArray<Hull>> keyValuePair in CS$<>8__locals1.combinedHulls)
			{
				Hull hull6;
				ImmutableArray<Hull> linkedHulls2;
				keyValuePair.Deconstruct(out hull6, out linkedHulls2);
				Hull mainHull = hull6;
				ImmutableArray<Hull> linkedHulls = linkedHulls2;
				MiniMapHullData data = MiniMap.ConstructHullPolygon(mainHull, linkedHulls, CS$<>8__locals1.hullContainer, CS$<>8__locals1.worldBorders);
				RelativeEntityRect relativeRect2 = new RelativeEntityRect(CS$<>8__locals1.worldBorders, data.Bounds);
				float highestY = 0f;
				float highestX = 0f;
				ValueTuple<RectangleF, Hull>[] rectDatas = data.RectDatas;
				for (int i = 0; i < rectDatas.Length; i++)
				{
					RectangleF r = rectDatas[i].Item1;
					float y = r.Y - -r.Height;
					float x = r.X;
					if (y > highestY)
					{
						highestY = y;
					}
					if (x > highestX)
					{
						highestX = x;
					}
				}
				Dictionary<Hull, GUIFrame> hullsAndFrames = new Dictionary<Hull, GUIFrame>();
				foreach (ValueTuple<RectangleF, Hull> valueTuple in data.RectDatas)
				{
					RectangleF snappredRect = valueTuple.Item1;
					Hull hull2 = valueTuple.Item2;
					RectangleF rect = snappredRect;
					rect.Height = -rect.Height;
					rect.Y -= rect.Height;
					float num;
					float num2;
					CS$<>8__locals1.hullContainer.Rect.Size.ToVector2().Deconstruct(out num, out num2);
					float parentW = num;
					float parentH = num2;
					Vector2 size = new Vector2(rect.Width / parentW, rect.Height / parentH);
					Vector2 pos = new Vector2(rect.X / parentW, rect.Y / parentH);
					GUIFrame hullFrame2 = new GUIFrame(new RectTransform(size, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = pos
					}, "ScanLinesSeamless", new Color?(settings.ElementColor))
					{
						UserData = hull2,
						UVOffset = new Vector2(highestX - rect.X, highestY - rect.Y)
					};
					hullsAndFrames.Add(hull2, hullFrame2);
				}
				foreach (KeyValuePair<Hull, GUIFrame> keyValuePair2 in hullsAndFrames)
				{
					GUIFrame guiframe;
					keyValuePair2.Deconstruct(out hull6, out guiframe);
					Hull hull3 = hull6;
					GUIFrame frame = guiframe;
					Rectangle rect2 = frame.Rect;
					using (Dictionary<Hull, GUIFrame>.Enumerator enumerator4 = hullsAndFrames.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							keyValuePair2 = enumerator4.Current;
							keyValuePair2.Deconstruct(out hull6, out guiframe);
							Hull hull4 = hull6;
							GUIFrame frame2 = guiframe;
							if (hull4 != hull3)
							{
								Rectangle rect3 = frame2.Rect;
								Point size2 = frame.RectTransform.NonScaledSize;
								int diffY = rect3.Top - rect2.Bottom;
								int diffX = rect3.Left - rect2.Right;
								if (diffY <= 2 && diffY > 0)
								{
									size2.Y += diffY;
								}
								if (diffX <= 2 && diffX > 0)
								{
									size2.X += diffX;
								}
								frame.RectTransform.NonScaledSize = size2;
							}
						}
					}
				}
				GUICustomComponent linkedHullFrame = new GUICustomComponent(new RectTransform(relativeRect2.RelativeSize, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = relativeRect2.RelativePosition
				}, delegate(SpriteBatch spriteBatch, GUICustomComponent component)
				{
					foreach (List<Vector2> list in data.Polygon)
					{
						spriteBatch.DrawPolygonInner(CS$<>8__locals1.hullContainer.Rect.Location.ToVector2(), list, component.OutlineColor, 2f);
					}
				}, delegate(float deltaTime, GUICustomComponent component)
				{
					if (component.Parent.Rect.Size != data.ParentSize)
					{
						data = MiniMap.ConstructHullPolygon(mainHull, linkedHulls, CS$<>8__locals1.hullContainer, CS$<>8__locals1.worldBorders);
					}
				})
				{
					UserData = hullsAndFrames.Values.ToHashSet<GUIFrame>(),
					OutlineColor = settings.ElementColor,
					CanBeFocused = false
				};
				foreach (KeyValuePair<Hull, GUIFrame> keyValuePair2 in hullsAndFrames)
				{
					GUIFrame guiframe;
					keyValuePair2.Deconstruct(out hull6, out guiframe);
					Hull hull5 = hull6;
					GUIFrame component2 = guiframe;
					pointsOfInterestCollection.Add(hull5, new MiniMapGUIComponent(component2, linkedHullFrame));
				}
			}
			if (pointsOfInterest != null)
			{
				foreach (MapEntity entity in pointsOfInterest)
				{
					RelativeEntityRect relativeRect3 = new RelativeEntityRect(CS$<>8__locals1.worldBorders, entity.WorldRect);
					GUIFrame poiComponent = new GUIFrame(new RectTransform(relativeRect3.RelativeSize, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = relativeRect3.RelativePosition
					}, null, null)
					{
						CanBeFocused = false,
						UserData = entity
					};
					pointsOfInterestCollection.Add(entity, new MiniMapGUIComponent(poiComponent));
				}
			}
			elements = pointsOfInterestCollection.ToImmutableDictionary<MapEntity, MiniMapGUIComponent>();
			return CS$<>8__locals1.hullContainer;
		}

		// Token: 0x06005C8F RID: 23695 RVA: 0x002FC830 File Offset: 0x002FAA30
		[return: Nullable(new byte[]
		{
			1,
			1,
			0,
			1
		})]
		private static ImmutableDictionary<Hull, ImmutableArray<Hull>> CombinedHulls([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Hull> hulls)
		{
			Dictionary<Hull, HashSet<Hull>> combinedHulls = new Dictionary<Hull, HashSet<Hull>>();
			ImmutableArray<Hull>.Enumerator enumerator = hulls.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Hull hull = enumerator.Current;
				if (!combinedHulls.ContainsKey(hull) && !combinedHulls.Values.Any((HashSet<Hull> hh) => hh.Contains(hull)))
				{
					List<Hull> linkedHulls = new List<Hull>();
					hull.GetLinkedHulls(linkedHulls, false);
					linkedHulls.Remove(hull);
					foreach (Hull linkedHull in linkedHulls)
					{
						if (!combinedHulls.ContainsKey(hull))
						{
							combinedHulls.Add(hull, new HashSet<Hull>());
						}
						combinedHulls[hull].Add(linkedHull);
					}
				}
			}
			return combinedHulls.ToImmutableDictionary((KeyValuePair<Hull, HashSet<Hull>> pair) => pair.Key, (KeyValuePair<Hull, HashSet<Hull>> pair) => pair.Value.ToImmutableArray<Hull>());
		}

		// Token: 0x06005C90 RID: 23696 RVA: 0x002FC96C File Offset: 0x002FAB6C
		private static MiniMapHullData ConstructHullPolygon(Hull mainHull, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Hull> linkedHulls, GUIComponent parent, RectangleF worldBorders)
		{
			Rectangle parentRect = parent.Rect;
			Dictionary<Hull, Rectangle> rects = new Dictionary<Hull, Rectangle>();
			Rectangle worldRect = mainHull.WorldRect;
			worldRect.Y = -worldRect.Y;
			rects.Add(mainHull, worldRect);
			foreach (Hull hull in linkedHulls)
			{
				Rectangle rect = hull.WorldRect;
				rect.Y = -rect.Y;
				worldRect = Rectangle.Union(worldRect, rect);
				rects.Add(hull, rect);
			}
			worldRect.Y = -worldRect.Y;
			List<RectangleF> normalizedRects = new List<RectangleF>();
			List<Hull> hullRefs = new List<Hull>();
			foreach (KeyValuePair<Hull, Rectangle> keyValuePair in rects)
			{
				Hull hull3;
				Rectangle rectangle;
				keyValuePair.Deconstruct(out hull3, out rectangle);
				Hull hull2 = hull3;
				Rectangle rect2 = rectangle;
				Rectangle wRect = rect2;
				wRect.Y = -wRect.Y;
				float num;
				float num2;
				float num3;
				float num4;
				new RelativeEntityRect(worldBorders, wRect).Deconstruct(out num, out num2, out num3, out num4);
				float posX = num;
				float posY = num2;
				float sizeX = num3;
				float sizeY = num4;
				RectangleF newRect = new RectangleF(posX * (float)parentRect.Width, posY * (float)parentRect.Height, sizeX * (float)parentRect.Width, sizeY * (float)parentRect.Height);
				normalizedRects.Add(newRect);
				hullRefs.Add(hull2);
			}
			hullRefs.Reverse();
			ImmutableArray<RectangleF> snappedRectangles = ToolBox.SnapRectangles(normalizedRects, 1);
			List<List<Vector2>> polygon = ToolBox.CombineRectanglesIntoShape(snappedRectangles);
			List<List<Vector2>> scaledPolygon = new List<List<Vector2>>();
			foreach (List<Vector2> list in polygon)
			{
				float num3;
				float num4;
				ToolBox.GetPolygonBoundingBoxSize(list).Deconstruct(out num4, out num3);
				float polySizeX = num4;
				float polySizeY = num3;
				float sizeX2 = polySizeX - 1f;
				float sizeY2 = polySizeY - 1f;
				scaledPolygon.Add(ToolBox.ScalePolygon(list, new Vector2(sizeX2 / polySizeX, sizeY2 / polySizeY)));
			}
			return new MiniMapHullData(scaledPolygon, worldRect, parentRect.Size, snappedRectangles, hullRefs.ToImmutableArray<Hull>());
		}

		// Token: 0x06005C91 RID: 23697 RVA: 0x002FCBA4 File Offset: 0x002FADA4
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			Item source = signal.source;
			if (source == null || source.CurrentHull == null)
			{
				return;
			}
			Hull sourceHull = source.CurrentHull;
			MiniMap.HullData hullData;
			if (!this.hullDatas.TryGetValue(sourceHull, out hullData))
			{
				hullData = new MiniMap.HullData();
				this.hullDatas.Add(sourceHull, hullData);
			}
			if (hullData.Distort)
			{
				return;
			}
			string name = connection.Name;
			if (!(name == "water_data_in"))
			{
				if (!(name == "oxygen_data_in"))
				{
					return;
				}
			}
			else
			{
				bool fromWaterDetector = source.GetComponent<WaterDetector>() != null;
				hullData.ReceivedWaterAmount = null;
				hullData.LastWaterDataTime = Timing.TotalTime;
				if (fromWaterDetector)
				{
					hullData.ReceivedWaterAmount = new float?((float)WaterDetector.GetWaterPercentage(sourceHull));
				}
				using (List<MapEntity>.Enumerator enumerator = sourceHull.linkedTo.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						MapEntity linked = enumerator.Current;
						Hull linkedHull = linked as Hull;
						if (linkedHull != null)
						{
							MiniMap.HullData linkedHullData;
							if (!this.hullDatas.TryGetValue(linkedHull, out linkedHullData))
							{
								linkedHullData = new MiniMap.HullData();
								this.hullDatas.Add(linkedHull, linkedHullData);
							}
							linkedHullData.ReceivedWaterAmount = null;
							if (fromWaterDetector)
							{
								linkedHullData.ReceivedWaterAmount = new float?((float)WaterDetector.GetWaterPercentage(linkedHull));
							}
						}
					}
					return;
				}
			}
			float oxy;
			if (!float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out oxy))
			{
				oxy = Rand.Range(0f, 100f, Rand.RandSync.Unsynced);
			}
			hullData.ReceivedOxygenAmount = new float?(oxy);
			hullData.LastOxygenDataTime = Timing.TotalTime;
			foreach (MapEntity linked2 in sourceHull.linkedTo)
			{
				Hull linkedHull2 = linked2 as Hull;
				if (linkedHull2 != null)
				{
					MiniMap.HullData linkedHullData2;
					if (!this.hullDatas.TryGetValue(linkedHull2, out linkedHullData2))
					{
						linkedHullData2 = new MiniMap.HullData();
						this.hullDatas.Add(linkedHull2, linkedHullData2);
					}
					linkedHullData2.ReceivedOxygenAmount = new float?(oxy);
				}
			}
		}

		// Token: 0x06005C92 RID: 23698 RVA: 0x002FCDB4 File Offset: 0x002FAFB4
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.searchAutoComplete != null)
			{
				this.searchAutoComplete.RectTransform.Parent = null;
				this.searchAutoComplete = null;
			}
			if (this.hullInfoFrame != null)
			{
				this.hullInfoFrame.RectTransform.Parent = null;
				this.hullInfoFrame = null;
			}
		}

		// Token: 0x17001747 RID: 5959
		// (get) Token: 0x06005C93 RID: 23699 RVA: 0x002FCE07 File Offset: 0x002FB007
		// (set) Token: 0x06005C94 RID: 23700 RVA: 0x002FCE0F File Offset: 0x002FB00F
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Does the machine require inputs from water detectors in order to show the water levels inside rooms.", "", false)]
		public bool RequireWaterDetectors { get; set; }

		// Token: 0x17001748 RID: 5960
		// (get) Token: 0x06005C95 RID: 23701 RVA: 0x002FCE18 File Offset: 0x002FB018
		// (set) Token: 0x06005C96 RID: 23702 RVA: 0x002FCE20 File Offset: 0x002FB020
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Does the machine require inputs from oxygen detectors in order to show the oxygen levels inside rooms.", "", false)]
		public bool RequireOxygenDetectors { get; set; }

		// Token: 0x17001749 RID: 5961
		// (get) Token: 0x06005C97 RID: 23703 RVA: 0x002FCE29 File Offset: 0x002FB029
		// (set) Token: 0x06005C98 RID: 23704 RVA: 0x002FCE31 File Offset: 0x002FB031
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should damaged walls be displayed by the machine.", "", false)]
		public bool ShowHullIntegrity { get; set; }

		// Token: 0x1700174A RID: 5962
		// (get) Token: 0x06005C99 RID: 23705 RVA: 0x002FCE3A File Offset: 0x002FB03A
		// (set) Token: 0x06005C9A RID: 23706 RVA: 0x002FCE42 File Offset: 0x002FB042
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable hull status mode.", "", false)]
		public bool EnableHullStatus { get; set; }

		// Token: 0x1700174B RID: 5963
		// (get) Token: 0x06005C9B RID: 23707 RVA: 0x002FCE4B File Offset: 0x002FB04B
		// (set) Token: 0x06005C9C RID: 23708 RVA: 0x002FCE53 File Offset: 0x002FB053
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable electrical view mode.", "", false)]
		public bool EnableElectricalView { get; set; }

		// Token: 0x1700174C RID: 5964
		// (get) Token: 0x06005C9D RID: 23709 RVA: 0x002FCE5C File Offset: 0x002FB05C
		// (set) Token: 0x06005C9E RID: 23710 RVA: 0x002FCE64 File Offset: 0x002FB064
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable item finder mode.", "", false)]
		public bool EnableItemFinder { get; set; }

		// Token: 0x06005C9F RID: 23711 RVA: 0x002FCE70 File Offset: 0x002FB070
		[NullableContext(0)]
		public MiniMap(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.InitProjSpecific();
		}

		// Token: 0x06005CA0 RID: 23712 RVA: 0x002FCED9 File Offset: 0x002FB0D9
		private void InitProjSpecific()
		{
			this.hullDatas = new Dictionary<Hull, MiniMap.HullData>();
			this.SetDefaultMode();
			this.noPowerTip = TextManager.Get("SteeringNoPowerTip");
			this.CreateGUI();
		}

		// Token: 0x06005CA1 RID: 23713 RVA: 0x002FCF04 File Offset: 0x002FB104
		[NullableContext(0)]
		public override void Update(float deltaTime, Camera cam)
		{
			this.hasPower = this.HasPower;
			if (this.hasPower)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
		}

		// Token: 0x06005CA2 RID: 23714 RVA: 0x002FCF40 File Offset: 0x002FB140
		[NullableContext(0)]
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			return base.PowerConsumption * MathHelper.Lerp(1.5f, 1f, this.item.Condition / this.item.MaxCondition);
		}

		// Token: 0x06005CA3 RID: 23715 RVA: 0x002FCF91 File Offset: 0x002FB191
		[NullableContext(0)]
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x06005CAF RID: 23727 RVA: 0x002FD29C File Offset: 0x002FB49C
		[CompilerGenerated]
		internal static void <DrawItem>g__DrawAdditionalSprite|92_0(Vector2 basePos, Sprite addSprite, float rotation, ref MiniMap.<>c__DisplayClass92_0 A_3)
		{
			RectangleF addRect = MiniMap.ScaleRectToUI(new RectangleF(basePos, addSprite.size * A_3.item.Scale), A_3.parent, A_3.border);
			Vector2 addScale = new Vector2(addRect.Size.X / addSprite.size.X, addRect.Size.Y / addSprite.size.Y);
			addSprite.Draw(A_3.spriteBatch, new Vector2(addRect.Location.X + (float)A_3.inflate, addRect.Location.Y + (float)A_3.inflate), A_3.item.SpriteColor, addSprite.Origin, rotation, addScale, A_3.item.SpriteEffects, null);
		}

		// Token: 0x04002F2C RID: 12076
		private Dictionary<Hull, MiniMap.HullData> hullDatas;

		// Token: 0x04002F2D RID: 12077
		private DateTime resetDataTime;

		// Token: 0x04002F2E RID: 12078
		private GUIFrame submarineContainer;

		// Token: 0x04002F2F RID: 12079
		[Nullable(2)]
		private GUIFrame hullInfoFrame;

		// Token: 0x04002F30 RID: 12080
		[Nullable(2)]
		private GUIScissorComponent scissorComponent;

		// Token: 0x04002F31 RID: 12081
		[Nullable(2)]
		private GUIComponent miniMapContainer;

		// Token: 0x04002F32 RID: 12082
		private GUIComponent miniMapFrame;

		// Token: 0x04002F33 RID: 12083
		private GUIComponent electricalFrame;

		// Token: 0x04002F34 RID: 12084
		private GUILayoutGroup reportFrame;

		// Token: 0x04002F35 RID: 12085
		private GUILayoutGroup searchBarFrame;

		// Token: 0x04002F36 RID: 12086
		private GUITextBox searchBar;

		// Token: 0x04002F37 RID: 12087
		[Nullable(2)]
		private GUIComponent searchAutoComplete;

		// Token: 0x04002F38 RID: 12088
		[Nullable(2)]
		private ItemPrefab searchedPrefab;

		// Token: 0x04002F39 RID: 12089
		private GUITextBlock tooltipHeader;

		// Token: 0x04002F3A RID: 12090
		private GUITextBlock tooltipFirstLine;

		// Token: 0x04002F3B RID: 12091
		private GUITextBlock tooltipSecondLine;

		// Token: 0x04002F3C RID: 12092
		private GUITextBlock tooltipThirdLine;

		// Token: 0x04002F3D RID: 12093
		private LocalizedString noPowerTip = string.Empty;

		// Token: 0x04002F3E RID: 12094
		private readonly List<Submarine> displayedSubs = new List<Submarine>();

		// Token: 0x04002F3F RID: 12095
		private Point prevResolution;

		// Token: 0x04002F40 RID: 12096
		private float cardRefreshTimer;

		// Token: 0x04002F41 RID: 12097
		private const float cardRefreshDelay = 3f;

		// Token: 0x04002F42 RID: 12098
		private readonly HashSet<MiniMapSprite> cardsToDraw = new HashSet<MiniMapSprite>();

		// Token: 0x04002F43 RID: 12099
		private List<MapEntity> subEntities = new List<MapEntity>();

		// Token: 0x04002F44 RID: 12100
		[Nullable(2)]
		private Texture2D submarinePreview;

		// Token: 0x04002F45 RID: 12101
		private MiniMapMode currentMode;

		// Token: 0x04002F46 RID: 12102
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private ImmutableArray<GUIButton> modeSwitchButtons;

		// Token: 0x04002F47 RID: 12103
		private Point elementSize;

		// Token: 0x04002F48 RID: 12104
		private ImmutableDictionary<MapEntity, MiniMapGUIComponent> hullStatusComponents;

		// Token: 0x04002F49 RID: 12105
		private ImmutableDictionary<MapEntity, MiniMapGUIComponent> electricalMapComponents;

		// Token: 0x04002F4A RID: 12106
		private ImmutableDictionary<MiniMapGUIComponent, GUIComponent> electricalChildren;

		// Token: 0x04002F4B RID: 12107
		private ImmutableDictionary<MiniMapGUIComponent, GUIComponent> doorChildren;

		// Token: 0x04002F4C RID: 12108
		private ImmutableDictionary<MiniMapGUIComponent, GUIComponent> weaponChildren;

		// Token: 0x04002F4D RID: 12109
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private ImmutableHashSet<ItemPrefab> itemsFoundOnSub;

		// Token: 0x04002F4E RID: 12110
		[Nullable(2)]
		private ImmutableHashSet<Vector2> MiniMapBlips;

		// Token: 0x04002F4F RID: 12111
		private float blipState;

		// Token: 0x04002F50 RID: 12112
		private const float maxBlipState = 1f;

		// Token: 0x04002F51 RID: 12113
		private const float maxZoom = 10f;

		// Token: 0x04002F52 RID: 12114
		private const float minZoom = 0.5f;

		// Token: 0x04002F53 RID: 12115
		private const float defaultZoom = 1f;

		// Token: 0x04002F54 RID: 12116
		private float zoom = 1f;

		// Token: 0x04002F55 RID: 12117
		private Vector2 mapOffset = Vector2.Zero;

		// Token: 0x04002F56 RID: 12118
		private bool dragMap;

		// Token: 0x04002F57 RID: 12119
		private Vector2? dragMapStart;

		// Token: 0x04002F58 RID: 12120
		private const int dragTreshold = 8;

		// Token: 0x04002F59 RID: 12121
		private bool recalculate;

		// Token: 0x04002F5A RID: 12122
		public static readonly Color MiniMapBaseColor = new Color(15, 178, 107);

		// Token: 0x04002F5B RID: 12123
		private static readonly Color WetHullColor = new Color(11, 122, 205);

		// Token: 0x04002F5C RID: 12124
		private static readonly Color DoorIndicatorColor = GUIStyle.Green;

		// Token: 0x04002F5D RID: 12125
		private static readonly Color NoPowerDoorColor = MiniMap.DoorIndicatorColor * 0.1f;

		// Token: 0x04002F5E RID: 12126
		private static readonly Color DefaultNeutralColor = MiniMap.MiniMapBaseColor * 0.8f;

		// Token: 0x04002F5F RID: 12127
		private static readonly Color HoverColor = Color.White;

		// Token: 0x04002F60 RID: 12128
		private static readonly Color BlueprintBlue = new Color(23, 38, 33);

		// Token: 0x04002F61 RID: 12129
		private static readonly Color HullWaterColor = new Color(17, 173, 179) * 0.5f;

		// Token: 0x04002F62 RID: 12130
		private static readonly Color HullWaterLineColor = Color.LightBlue * 0.5f;

		// Token: 0x04002F63 RID: 12131
		private static readonly Color NoPowerColor = MiniMap.MiniMapBaseColor * 0.1f;

		// Token: 0x04002F64 RID: 12132
		private static readonly Color ElectricalBaseColor = GUIStyle.Orange;

		// Token: 0x04002F65 RID: 12133
		private static readonly Color NoPowerElectricalColor = MiniMap.ElectricalBaseColor * 0.1f;

		// Token: 0x04002F67 RID: 12135
		private bool hasPower;

		// Token: 0x02001406 RID: 5126
		[NullableContext(0)]
		internal class HullData
		{
			// Token: 0x0400641F RID: 25631
			public float? HullOxygenAmount;

			// Token: 0x04006420 RID: 25632
			public float? HullWaterAmount;

			// Token: 0x04006421 RID: 25633
			public float? ReceivedOxygenAmount;

			// Token: 0x04006422 RID: 25634
			public float? ReceivedWaterAmount;

			// Token: 0x04006423 RID: 25635
			public double LastOxygenDataTime;

			// Token: 0x04006424 RID: 25636
			public double LastWaterDataTime;

			// Token: 0x04006425 RID: 25637
			public readonly HashSet<IdCard> Cards = new HashSet<IdCard>();

			// Token: 0x04006426 RID: 25638
			public bool Distort;

			// Token: 0x04006427 RID: 25639
			public float DistortionTimer;

			// Token: 0x04006428 RID: 25640
			public List<Hull> LinkedHulls = new List<Hull>();
		}
	}
}
