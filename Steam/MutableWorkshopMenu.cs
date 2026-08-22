using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma.Steam
{
	// Token: 0x0200042E RID: 1070
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class MutableWorkshopMenu : WorkshopMenu
	{
		// Token: 0x17001240 RID: 4672
		// (get) Token: 0x060047AD RID: 18349 RVA: 0x00273B3C File Offset: 0x00271D3C
		private CorePackage EnabledCorePackage
		{
			get
			{
				CorePackage corePackage = this.enabledCoreDropdown.SelectedData as CorePackage;
				if (corePackage == null)
				{
					throw new Exception("Valid core package not selected");
				}
				return corePackage;
			}
		}

		// Token: 0x17001241 RID: 4673
		// (get) Token: 0x060047AE RID: 18350 RVA: 0x00273B5D File Offset: 0x00271D5D
		// (set) Token: 0x060047AF RID: 18351 RVA: 0x00273B65 File Offset: 0x00271D65
		public bool ViewingItemDetails { get; private set; }

		// Token: 0x060047B0 RID: 18352 RVA: 0x00273B70 File Offset: 0x00271D70
		private void UpdateSubscribedModInstalls()
		{
			if (!MutableWorkshopMenu.EnableWorkshopSupport)
			{
				return;
			}
			uint numSubscribedMods = SteamManager.GetNumSubscribedItems();
			if (numSubscribedMods == this.memSubscribedModCount)
			{
				return;
			}
			this.memSubscribedModCount = numSubscribedMods;
			ImmutableHashSet<PublishedFileId> subscribedIds = SteamManager.Workshop.GetSubscribedItemIds();
			HashSet<ulong> installedIds = (from id in (from p in ContentPackageManager.WorkshopPackages
			select p.UgcId).NotNone<ContentPackageId>().OfType<SteamWorkshopId>()
			select id.Value).ToHashSet<ulong>();
			IEnumerable<PublishedFileId> source = subscribedIds;
			Func<PublishedFileId, bool> <>9__3;
			Func<PublishedFileId, bool> predicate;
			if ((predicate = <>9__3) == null)
			{
				predicate = (<>9__3 = ((PublishedFileId id2) => !installedIds.Contains(id2)));
			}
			foreach (PublishedFileId id3 in source.Where(predicate))
			{
				Item item = new Item(id3);
				if (!item.IsDownloading && !SteamManager.Workshop.IsInstalling(item))
				{
					SteamManager.Workshop.DownloadModThenEnqueueInstall(item);
				}
			}
			SteamManager.Workshop.DeleteUnsubscribedMods(delegate(ContentPackage[] removedPackages)
			{
				if (removedPackages.Any<ContentPackage>())
				{
					this.PopulateInstalledModLists(false, true);
				}
			});
		}

		// Token: 0x060047B1 RID: 18353 RVA: 0x00273CA0 File Offset: 0x00271EA0
		[return: TupleElementNames(new string[]
		{
			"Left",
			"center",
			"Right"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1
		})]
		private static ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup> CreateSidebars(GUIComponent parent, float leftWidth = 0.3875f, float centerWidth = 0.025f, float rightWidth = 0.5875f, bool split = false, float height = 1f)
		{
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, height), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup left = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(leftWidth, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame center = new GUIFrame(new RectTransform(new ValueTuple<float, float>(centerWidth, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (split)
			{
				new GUICustomComponent(new RectTransform(Vector2.One, center.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
				{
					sb.DrawLine(new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Top), new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Bottom), GUIStyle.TextColorDim, 2f);
				}, null);
			}
			GUILayoutGroup right = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(rightWidth, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			return new ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup>(left, center, right);
		}

		// Token: 0x060047B2 RID: 18354 RVA: 0x00273E28 File Offset: 0x00272028
		private static void HandleDraggingAcrossModLists(GUIListBox from, GUIListBox to)
		{
			if (to.Rect.Contains(PlayerInput.MousePosition) && from.DraggedElement != null)
			{
				GUIComponent draggedElement = from.DraggedElement;
				List<GUIComponent> selected = from.AllSelected.ToList<GUIComponent>();
				selected.Sort((GUIComponent a, GUIComponent b) => from.Content.GetChildIndex(a) - from.Content.GetChildIndex(b));
				float oldCount = (float)to.Content.CountChildren;
				float newCount = oldCount + (float)selected.Count;
				Point offset = draggedElement.RectTransform.AbsoluteOffset;
				offset += from.Content.Rect.Location;
				offset -= to.Content.Rect.Location;
				for (int i = 0; i < selected.Count; i++)
				{
					GUIComponent c = selected[i];
					c.Parent.RemoveChild(c);
					c.RectTransform.Parent = to.Content.RectTransform;
					c.RectTransform.RepositionChildInHierarchy((int)oldCount + i);
				}
				from.DraggedElement = null;
				from.Deselect();
				from.RecalculateChildren();
				from.RectTransform.RecalculateScale(true);
				to.RecalculateChildren();
				to.RectTransform.RecalculateScale(true);
				to.Select(selected);
				draggedElement.RectTransform.AbsoluteOffset = offset;
				to.DraggedElement = draggedElement;
				to.BarScroll *= oldCount / newCount;
			}
		}

		// Token: 0x060047B3 RID: 18355 RVA: 0x00273FC6 File Offset: 0x002721C6
		private void PlaySwapSound()
		{
			SoundPlayer.PlayUISound(this.swapSoundType);
		}

		// Token: 0x060047B4 RID: 18356 RVA: 0x00273FD4 File Offset: 0x002721D4
		private void SetSwapFunc(GUIListBox from, GUIListBox to)
		{
			this.currentSwapFunc = delegate()
			{
				to.Deselect();
				GUIComponent[] selected = from.AllSelected.ToArray<GUIComponent>();
				foreach (GUIComponent frame in selected)
				{
					frame.Parent.RemoveChild(frame);
					frame.RectTransform.Parent = to.Content.RectTransform;
				}
				from.RecalculateChildren();
				from.RectTransform.RecalculateScale(true);
				to.RecalculateChildren();
				to.RectTransform.RecalculateScale(true);
				to.Select(selected);
			};
			if (to == this.enabledRegularModsList)
			{
				this.swapSoundType = new GUISoundType?(GUISoundType.Increase);
				return;
			}
			if (to == this.disabledRegularModsList)
			{
				this.swapSoundType = new GUISoundType?(GUISoundType.Decrease);
				return;
			}
			this.swapSoundType = null;
		}

		// Token: 0x060047B5 RID: 18357 RVA: 0x0027404C File Offset: 0x0027224C
		private void CreateInstalledModsTab(out GUIDropDown enabledCoreDropdown, out GUIListBox enabledRegularModsList, out GUIListBox disabledRegularModsList, out Action<Either<Item, ContentPackage>> onInstalledInfoButtonHit, out GUITextBox modsListFilter, out Dictionary<MutableWorkshopMenu.Filter, GUITickBox> modsListFilterTickboxes, [Nullable(new byte[]
		{
			0,
			1
		})] out Option<GUIButton> bulkUpdateButton)
		{
			MutableWorkshopMenu.<>c__DisplayClass22_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass22_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.content = this.CreateNewContentFrame(MutableWorkshopMenu.Tab.InstalledMods);
			GUIListBox outerContainer;
			Action deselect;
			this.CreateWorkshopItemDetailContainer(CS$<>8__locals1.content, out outerContainer, delegate(Either<Item, ContentPackage> itemOrPackage, GUIFrame selectedFrame)
			{
				Item item;
				if (itemOrPackage.TryGet(out item))
				{
					CS$<>8__locals1.<>4__this.PopulateFrameWithItemInfo(item, selectedFrame);
				}
			}, delegate
			{
				CS$<>8__locals1.<>4__this.PopulateInstalledModLists(false, true);
			}, out onInstalledInfoButtonHit, out deselect);
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(Vector2.One, outerContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			mainLayout.RectTransform.SetAsFirstChild();
			ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup> valueTuple = MutableWorkshopMenu.CreateSidebars(mainLayout, 0.475f, 0.05f, 0.475f, false, 0.13f);
			GUILayoutGroup topLeft = valueTuple.Item1;
			GUILayoutGroup topRight = valueTuple.Item3;
			topLeft.Stretch = true;
			WorkshopMenu.Label(topLeft, TextManager.Get("enabledcore"), GUIStyle.SubHeadingFont, 1f);
			enabledCoreDropdown = WorkshopMenu.Dropdown<CorePackage>(topLeft, (CorePackage p) => p.Name, ContentPackageManager.CorePackages.ToArray<CorePackage>(), ContentPackageManager.EnabledPackages.Core, delegate(CorePackage p)
			{
			}, 0.07692308f);
			enabledCoreDropdown.AllowNonText = true;
			WorkshopMenu.Label(topLeft, "", GUIStyle.SubHeadingFont, 1f);
			topRight.ChildAnchor = Anchor.CenterLeft;
			CS$<>8__locals1.topRightButtons = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.5f), topRight.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			CS$<>8__locals1.<CreateInstalledModsTab>g__padTopRight|4(1f);
			CS$<>8__locals1.<CreateInstalledModsTab>g__padTopRight|4(3f);
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, CS$<>8__locals1.topRightButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "GUIReloadButton", null);
			guibutton.OnClicked = delegate(GUIButton b, object o)
			{
				CS$<>8__locals1.<>4__this.PopulateInstalledModLists(false, true);
				return false;
			};
			guibutton.ToolTip = TextManager.Get("RefreshModLists");
			Option<GUIButton> option;
			if (!MutableWorkshopMenu.EnableWorkshopSupport)
			{
				Option.UnspecifiedNone none = Option.None;
				option = none;
			}
			else
			{
				GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, CS$<>8__locals1.topRightButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "GUIUpdateButton", null);
				guibutton2.OnClicked = delegate(GUIButton b, object o)
				{
					BulkDownloader.PrepareUpdates();
					return false;
				};
				guibutton2.Enabled = false;
				option = Option.Some<GUIButton>(guibutton2);
			}
			bulkUpdateButton = option;
			CS$<>8__locals1.<CreateInstalledModsTab>g__padTopRight|4(0.1f);
			ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup> valueTuple2 = MutableWorkshopMenu.CreateSidebars(mainLayout, 0.475f, 0.05f, 0.475f, false, 0.8f);
			GUILayoutGroup left = valueTuple2.Item1;
			GUIFrame center = valueTuple2.Item2;
			GUILayoutGroup right = valueTuple2.Item3;
			right.ChildAnchor = Anchor.TopRight;
			GUITextBlock label = WorkshopMenu.Label(left, TextManager.Get("enabledregular"), GUIStyle.SubHeadingFont, 1f);
			new GUIImage(new RectTransform(new Point(label.Rect.Height), label.RectTransform, Anchor.CenterRight, null, ScaleBasis.Normal, false), "GUIButtonInfo", GUIImage.ScalingMode.None).ToolTip = TextManager.Get("ModLoadOrderExplanation");
			CS$<>8__locals1.enabledModsList = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.93f), left.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				CurrentDragMode = GUIListBox.DragMode.DragOutsideBox,
				CurrentSelectMode = GUIListBox.SelectMode.RequireShiftToSelectMultiple,
				HideDraggedElement = true,
				PlaySoundOnSelect = true,
				SoundOnDragStart = new GUISoundType?(GUISoundType.Select),
				SoundOnDragStop = new GUISoundType?(GUISoundType.Increase)
			};
			enabledRegularModsList = CS$<>8__locals1.enabledModsList;
			WorkshopMenu.Label(right, TextManager.Get("disabledregular"), GUIStyle.SubHeadingFont, 1f);
			CS$<>8__locals1.disabledModsList = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.93f), right.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				CurrentDragMode = GUIListBox.DragMode.DragOutsideBox,
				CurrentSelectMode = GUIListBox.SelectMode.RequireShiftToSelectMultiple,
				HideDraggedElement = true,
				PlaySoundOnSelect = true,
				SoundOnDragStart = new GUISoundType?(GUISoundType.Select),
				SoundOnDragStop = new GUISoundType?(GUISoundType.Decrease)
			};
			disabledRegularModsList = CS$<>8__locals1.disabledModsList;
			CS$<>8__locals1.centerButton = new GUIButton(new RectTransform(Vector2.One * 0.95f, center.RectTransform, Anchor.Center, null, null, null, ScaleBasis.BothWidth), Alignment.Center, "GUIButtonToggleLeft", null)
			{
				PlaySoundOnSelect = false,
				Visible = false,
				OnClicked = delegate(GUIButton button, object o)
				{
					if (CS$<>8__locals1.<>4__this.currentSwapFunc != null)
					{
						CS$<>8__locals1.<>4__this.PlaySwapSound();
						CS$<>8__locals1.<>4__this.currentSwapFunc();
					}
					return false;
				}
			};
			CS$<>8__locals1.enabledModsList.OnSelected = delegate(GUIComponent frame, object o)
			{
				CS$<>8__locals1.disabledModsList.Deselect();
				CS$<>8__locals1.centerButton.Visible = true;
				CS$<>8__locals1.centerButton.ApplyStyle(GUIStyle.GetComponentStyle("GUIButtonToggleRight"));
				CS$<>8__locals1.<>4__this.SetSwapFunc(CS$<>8__locals1.enabledModsList, CS$<>8__locals1.disabledModsList);
				return true;
			};
			CS$<>8__locals1.disabledModsList.OnSelected = delegate(GUIComponent frame, object o)
			{
				CS$<>8__locals1.enabledModsList.Deselect();
				CS$<>8__locals1.centerButton.Visible = true;
				CS$<>8__locals1.centerButton.ApplyStyle(GUIStyle.GetComponentStyle("GUIButtonToggleLeft"));
				CS$<>8__locals1.<>4__this.SetSwapFunc(CS$<>8__locals1.disabledModsList, CS$<>8__locals1.enabledModsList);
				return true;
			};
			CS$<>8__locals1.filterContainer = new GUILayoutGroup(WorkshopMenu.NewItemRectT(mainLayout, 1f), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			CS$<>8__locals1.<CreateInstalledModsTab>g__padFilterContainer|7(0.2f);
			GUIButton loadPresetBtn = CS$<>8__locals1.<CreateInstalledModsTab>g__filterLayoutButton|8("OpenButton");
			loadPresetBtn.ToolTip = TextManager.Get("LoadModListPresetHeader");
			loadPresetBtn.OnClicked = new GUIButton.OnClickedHandler(this.OpenLoadPreset);
			GUIButton savePresetBtn = CS$<>8__locals1.<CreateInstalledModsTab>g__filterLayoutButton|8("SaveButton");
			savePresetBtn.ToolTip = TextManager.Get("SaveModListPresetHeader");
			savePresetBtn.OnClicked = new GUIButton.OnClickedHandler(this.OpenSavePreset);
			CS$<>8__locals1.<CreateInstalledModsTab>g__padFilterContainer|7(0.05f);
			RectTransform searchRectT = new RectTransform(new ValueTuple<float, float>(0.5f, 1f), CS$<>8__locals1.filterContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			GUITextBox searchBox = base.CreateSearchBox(searchRectT);
			modsListFilter = searchBox;
			CS$<>8__locals1.filterTickboxes = new Dictionary<MutableWorkshopMenu.Filter, GUITickBox>();
			modsListFilterTickboxes = CS$<>8__locals1.filterTickboxes;
			CS$<>8__locals1.filterTickboxesDropdown = CS$<>8__locals1.<CreateInstalledModsTab>g__filterLayoutButton|8("SetupVisibilityButton");
			CS$<>8__locals1.filterTickboxesContainer = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.3f, 0.2f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothWidth), "InnerFrame", null);
			GUICustomComponent filterTickboxesUpdater = new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				CS$<>8__locals1.filterTickboxesContainer.Visible = CS$<>8__locals1.filterTickboxesDropdown.Selected;
				CS$<>8__locals1.filterTickboxesContainer.RectTransform.AbsoluteOffset = CS$<>8__locals1.filterTickboxesDropdown.Rect.Location - CS$<>8__locals1.content.Rect.Location + new ValueTuple<int, int>(CS$<>8__locals1.filterTickboxesDropdown.Rect.Width / 2, 0) - (CS$<>8__locals1.filterTickboxesContainer.Rect.Size.ToVector2() * new ValueTuple<float, float>(0.5f, 1f)).ToPoint();
				CS$<>8__locals1.filterTickboxesContainer.RectTransform.NonScaledSize = new Point((from tb in CS$<>8__locals1.filterTickboxes
				select (int)tb.Value.Font.MeasureString(tb.Value.GetChild<GUITextBlock>().Text, false).X).Max(), (from tb in CS$<>8__locals1.filterTickboxes
				select tb.Value.Rect.Height).Aggregate((int a, int b) => a + b)) + new ValueTuple<int, int>(CS$<>8__locals1.filterTickboxes.Values.First<GUITickBox>().Rect.Height * 4, CS$<>8__locals1.filterTickboxes.Values.First<GUITickBox>().Rect.Height / 2);
				if (PlayerInput.PrimaryMouseButtonClicked() && !GUI.IsMouseOn(CS$<>8__locals1.filterTickboxesDropdown) && !GUI.IsMouseOn(CS$<>8__locals1.filterTickboxesContainer))
				{
					CS$<>8__locals1.filterTickboxesDropdown.Selected = false;
				}
			});
			CS$<>8__locals1.filterTickboxesLayout = new GUILayoutGroup(new RectTransform(Vector2.One * 0.95f, CS$<>8__locals1.filterTickboxesContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			if (MutableWorkshopMenu.EnableWorkshopSupport)
			{
				CS$<>8__locals1.<CreateInstalledModsTab>g__addFilterTickbox|10(MutableWorkshopMenu.Filter.ShowLocal, "WorkshopMenu.EditButton", true);
				CS$<>8__locals1.<CreateInstalledModsTab>g__addFilterTickbox|10(MutableWorkshopMenu.Filter.ShowWorkshop, "WorkshopMenu.DownloadedIcon", true);
				CS$<>8__locals1.<CreateInstalledModsTab>g__addFilterTickbox|10(MutableWorkshopMenu.Filter.ShowPublished, "WorkshopMenu.PublishedIcon", true);
			}
			CS$<>8__locals1.<CreateInstalledModsTab>g__addFilterTickbox|10(MutableWorkshopMenu.Filter.ShowOnlySubs, null, false);
			CS$<>8__locals1.<CreateInstalledModsTab>g__addFilterTickbox|10(MutableWorkshopMenu.Filter.ShowOnlyItemAssemblies, null, false);
			CS$<>8__locals1.<CreateInstalledModsTab>g__padFilterContainer|7(0.25f);
			new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				GUIComponent guicomponent = CS$<>8__locals1.enabledModsList.DraggedElement;
				if (guicomponent != null)
				{
					guicomponent.DrawManually(spriteBatch, true, true);
				}
				GUIComponent guicomponent2 = CS$<>8__locals1.disabledModsList.DraggedElement;
				if (guicomponent2 == null)
				{
					return;
				}
				guicomponent2.DrawManually(spriteBatch, true, true);
			}, delegate(float f, GUICustomComponent component)
			{
				MutableWorkshopMenu.HandleDraggingAcrossModLists(CS$<>8__locals1.enabledModsList, CS$<>8__locals1.disabledModsList);
				MutableWorkshopMenu.HandleDraggingAcrossModLists(CS$<>8__locals1.disabledModsList, CS$<>8__locals1.enabledModsList);
				base.<CreateInstalledModsTab>g__UpdateDraggingSounds|13();
				if (PlayerInput.PrimaryMouseButtonClicked() && !GUI.IsMouseOn(CS$<>8__locals1.enabledModsList) && !GUI.IsMouseOn(CS$<>8__locals1.disabledModsList) && GUIContextMenu.CurrentContextMenu == null)
				{
					CS$<>8__locals1.enabledModsList.Deselect();
					CS$<>8__locals1.disabledModsList.Deselect();
					return;
				}
				if (!PlayerInput.IsCtrlDown() && !PlayerInput.IsShiftDown() && PlayerInput.DoubleClicked())
				{
					Action action = CS$<>8__locals1.<>4__this.currentSwapFunc;
					if (action == null)
					{
						return;
					}
					action();
				}
			});
		}

		// Token: 0x060047B6 RID: 18358 RVA: 0x002748C4 File Offset: 0x00272AC4
		protected override void UpdateModListItemVisibility()
		{
			string str = this.modsListFilter.Text;
			this.enabledRegularModsList.Content.Children.Concat(this.disabledRegularModsList.Content.Children).ForEach(delegate(GUIComponent c)
			{
				ContentPackage p = c.UserData as ContentPackage;
				c.Visible = (p == null || (this.ModNameMatches(p, str) && this.ModMatchesTickboxes(p, c)));
			});
		}

		// Token: 0x060047B7 RID: 18359 RVA: 0x00274928 File Offset: 0x00272B28
		private bool ModMatchesTickboxes(ContentPackage p, GUIComponent guiItem)
		{
			GUILayoutGroup child = guiItem.GetChild<GUILayoutGroup>();
			GUIButton iconBtn = (child != null) ? child.GetAllChildren<GUIButton>().Last<GUIButton>() : null;
			bool matches = false;
			if (MutableWorkshopMenu.EnableWorkshopSupport)
			{
				matches |= (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowLocal].Selected && ContentPackageManager.LocalPackages.Contains(p));
				bool flag = matches;
				bool flag2;
				if (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowPublished].Selected)
				{
					if (ContentPackageManager.WorkshopPackages.Contains(p))
					{
						Identifier? identifier;
						Identifier? identifier2;
						if (iconBtn == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							GUIComponentStyle style = iconBtn.Style;
							if (style == null)
							{
								identifier = null;
								identifier2 = identifier;
							}
							else
							{
								identifier2 = new Identifier?(style.Identifier);
							}
						}
						identifier = identifier2;
						flag2 = (identifier == "WorkshopMenu.PublishedIcon");
					}
					else
					{
						flag2 = false;
					}
				}
				else
				{
					flag2 = false;
				}
				matches = (flag || flag2);
				bool flag3 = matches;
				bool flag4;
				if (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowWorkshop].Selected)
				{
					if (ContentPackageManager.WorkshopPackages.Contains(p))
					{
						Identifier? identifier;
						Identifier? identifier3;
						if (iconBtn == null)
						{
							identifier = null;
							identifier3 = identifier;
						}
						else
						{
							GUIComponentStyle style2 = iconBtn.Style;
							if (style2 == null)
							{
								identifier = null;
								identifier3 = identifier;
							}
							else
							{
								identifier3 = new Identifier?(style2.Identifier);
							}
						}
						identifier = identifier3;
						flag4 = (identifier != "WorkshopMenu.PublishedIcon");
					}
					else
					{
						flag4 = false;
					}
				}
				else
				{
					flag4 = false;
				}
				matches = (flag3 || flag4);
			}
			else
			{
				matches = true;
			}
			if (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowOnlySubs].Selected && this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowOnlyItemAssemblies].Selected)
			{
				if (p.Files.All((ContentFile f) => f is BaseSubFile || f is ItemAssemblyFile))
				{
					return matches;
				}
			}
			if (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowOnlySubs].Selected)
			{
				if (p.Files.Any((ContentFile f) => !(f is BaseSubFile)))
				{
					return false;
				}
			}
			if (this.modsListFilterTickboxes[MutableWorkshopMenu.Filter.ShowOnlyItemAssemblies].Selected)
			{
				if (p.Files.Any((ContentFile f) => !(f is ItemAssemblyFile)))
				{
					matches = false;
				}
			}
			return matches;
		}

		// Token: 0x060047B8 RID: 18360 RVA: 0x00274B24 File Offset: 0x00272D24
		private void PrepareToShowModInfo(ContentPackage mod)
		{
			ContentPackageId ugcId;
			if (mod.UgcId.TryUnwrap(out ugcId))
			{
				SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
				if (workshopId != null)
				{
					Item cachedItem;
					if (mod.UgcItem.TryUnwrap(out cachedItem))
					{
						this.onInstalledInfoButtonHit(cachedItem);
						return;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendLiteral("PrepareToShow");
					defaultInterpolatedStringHandler.AppendFormatted<Option<ContentPackageId>>(mod.UgcId);
					defaultInterpolatedStringHandler.AppendLiteral("Info");
					TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), SteamManager.Workshop.GetItem(workshopId.Value), delegate(Task t)
					{
						Option<Item> itemOption;
						if (!t.TryGetResult(out itemOption))
						{
							return;
						}
						Item item;
						if (!itemOption.TryUnwrap(out item))
						{
							return;
						}
						this.onInstalledInfoButtonHit(item);
					});
					return;
				}
			}
		}

		// Token: 0x060047B9 RID: 18361 RVA: 0x00274BC4 File Offset: 0x00272DC4
		public void PopulateInstalledModLists(bool forceRefreshEnabled = false, bool refreshDisabled = true)
		{
			MutableWorkshopMenu.<>c__DisplayClass26_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass26_0();
			CS$<>8__locals1.<>4__this = this;
			this.ViewingItemDetails = false;
			GUIButton bulkUpdateButton;
			if (this.bulkUpdateButtonOption.TryUnwrap(out bulkUpdateButton))
			{
				bulkUpdateButton.Enabled = false;
				bulkUpdateButton.ToolTip = "";
			}
			ContentPackageManager.UpdateContentPackageList();
			CorePackage[] corePackages = ContentPackageManager.CorePackages.ToArray<CorePackage>();
			CorePackage currentCore = ContentPackageManager.EnabledPackages.Core;
			WorkshopMenu.SwapDropdownValues<CorePackage>(this.enabledCoreDropdown, (CorePackage p) => p.Name, corePackages, currentCore, delegate(CorePackage p)
			{
				CS$<>8__locals1.<>4__this.enabledCoreDropdown.Text = p.Name;
				CS$<>8__locals1.<>4__this.enabledCoreDropdown.ButtonTextColor = (p.HasAnyErrors ? GUIStyle.Red : GUIStyle.TextColorNormal);
			});
			foreach (GUIComponent element in this.enabledCoreDropdown.ListBox.Content.Children.ToArray<GUIComponent>())
			{
				this.enabledCoreDropdown.ListBox.RemoveChild(element);
				ContentPackage mod = element.UserData as ContentPackage;
				if (mod != null)
				{
					CS$<>8__locals1.<PopulateInstalledModLists>g__createBaseModListUi|3(mod, this.enabledCoreDropdown.ListBox, 0.24f);
				}
			}
			this.enabledCoreDropdown.Select(corePackages.IndexOf(currentCore));
			MutableWorkshopMenu.<>c__DisplayClass26_0 CS$<>8__locals2 = CS$<>8__locals1;
			IEnumerable<RegularPackage> source;
			if (!forceRefreshEnabled && this.enabledRegularModsList.Content.CountChildren + this.disabledRegularModsList.Content.CountChildren != 0)
			{
				source = from p in (from c in this.enabledRegularModsList.Content.Children
				select c.UserData).OfType<RegularPackage>()
				where ContentPackageManager.RegularPackages.Contains(p)
				select p;
			}
			else
			{
				IEnumerable<RegularPackage> regular = ContentPackageManager.EnabledPackages.Regular;
				source = regular;
			}
			CS$<>8__locals2.enabledMods = source.ToArray<RegularPackage>();
			IEnumerable<RegularPackage> disabledMods = from p in ContentPackageManager.RegularPackages
			where !CS$<>8__locals1.enabledMods.Contains(p)
			select p;
			CS$<>8__locals1.<PopulateInstalledModLists>g__addRegularModsToList|5(CS$<>8__locals1.enabledMods, this.enabledRegularModsList);
			if (refreshDisabled)
			{
				CS$<>8__locals1.<PopulateInstalledModLists>g__addRegularModsToList|5(disabledMods, this.disabledRegularModsList);
			}
			TaskPool.AddIfNotFound("DetermineWorkshopModIcons", SteamManager.Workshop.GetPublishedItems(), delegate(Task t)
			{
				ISet<Item> items;
				if (!t.TryGetResult(out items))
				{
					return;
				}
				HashSet<PublishedFileId> ids = (from it in items
				select it.Id).ToHashSet<PublishedFileId>();
				foreach (GUIComponent child in CS$<>8__locals1.<>4__this.enabledRegularModsList.Content.Children.Concat(CS$<>8__locals1.<>4__this.disabledRegularModsList.Content.Children))
				{
					RegularPackage mod2 = child.UserData as RegularPackage;
					ContentPackageId ugcId;
					if (mod2 != null && ContentPackageManager.WorkshopPackages.Contains(mod2) && mod2.UgcId.TryUnwrap(out ugcId))
					{
						SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
						if (workshopId != null)
						{
							GUILayoutGroup child2 = child.GetChild<GUILayoutGroup>();
							GUIButton btn = (child2 != null) ? child2.GetAllChildren<GUIButton>().Last<GUIButton>() : null;
							if (btn != null && btn.Style == null)
							{
								btn.ApplyStyle(GUIStyle.GetComponentStyle(ids.Contains(workshopId.Value) ? "WorkshopMenu.PublishedIcon" : "WorkshopMenu.DownloadedIcon"));
								btn.ToolTip = TextManager.Get(ids.Contains(workshopId.Value) ? "PublishedWorkshopMod" : "DownloadedWorkshopMod");
								btn.HoverCursor = CursorState.Default;
							}
						}
					}
				}
			});
			this.UpdateModListItemVisibility();
		}

		// Token: 0x060047BA RID: 18362 RVA: 0x00274DD8 File Offset: 0x00272FD8
		private void CreateDependencyErrorMessageBox(ContentPackage contentPackage, IEnumerable<PublishedFileId> missingDependencies)
		{
			MutableWorkshopMenu.<>c__DisplayClass27_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass27_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.msgBox = new GUIMessageBox(TextManager.Get("Error"), TextManager.GetWithVariable("workshop.dependencynotfoundtitle", "[name]", contentPackage.Name, FormatCapitals.No), new Vector2?(new Vector2(0.25f, 0f)), new Point?(new Point(GUI.IntScale(650f), GUI.IntScale(650f))), GUIMessageBox.Type.Default);
			CS$<>8__locals1.msgBox.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
			{
				SettingsMenu instance = SettingsMenu.Instance;
				if (instance != null)
				{
					instance.ApplyInstalledModChanges();
				}
				CS$<>8__locals1.msgBox.Close();
				return true;
			};
			GUIListBox textListBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.75f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			using (IEnumerator<PublishedFileId> enumerator = missingDependencies.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MutableWorkshopMenu.<>c__DisplayClass27_1 CS$<>8__locals2 = new MutableWorkshopMenu.<>c__DisplayClass27_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.dependency = enumerator.Current;
					MutableWorkshopMenu.<>c__DisplayClass27_2 CS$<>8__locals3 = new MutableWorkshopMenu.<>c__DisplayClass27_2();
					CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals2;
					MutableWorkshopMenu.<>c__DisplayClass27_2 CS$<>8__locals4 = CS$<>8__locals3;
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), textListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler.AppendLiteral("- ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("unknown"));
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<PublishedFileId>(CS$<>8__locals3.CS$<>8__locals2.dependency);
					CS$<>8__locals4.textBlock = new GUITextBlock(rectT, defaultInterpolatedStringHandler.ToStringAndClear(), null, null, Alignment.Left, false, "", null)
					{
						CanBeFocused = false
					};
					ContentPackage matchingPackage = ContentPackageManager.WorkshopPackages.FirstOrDefault(delegate(ContentPackage p)
					{
						ContentPackageId ugcId;
						if (p.UgcId.TryUnwrap(out ugcId))
						{
							SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
							if (workshopId != null)
							{
								return workshopId.Value == CS$<>8__locals3.CS$<>8__locals2.dependency.Value;
							}
						}
						return false;
					});
					if (matchingPackage != null)
					{
						CS$<>8__locals3.matchingListElement = this.disabledRegularModsList.Content.GetChildByUserData(matchingPackage);
						if (CS$<>8__locals3.matchingListElement != null)
						{
							CS$<>8__locals3.textBlock.Text = "- " + matchingPackage.Name;
							GUIButton enableButton = new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), CS$<>8__locals3.textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("workshopitemenabled"), Alignment.Center, "", null)
							{
								OnClicked = delegate(GUIButton btn, object userdata)
								{
									btn.Enabled = false;
									CS$<>8__locals3.textBlock.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Green), 1.5f, false, false, null);
									CS$<>8__locals3.matchingListElement.RectTransform.Parent = CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.enabledRegularModsList.Content.RectTransform;
									CS$<>8__locals3.matchingListElement.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Green), 1.5f, false, false, null);
									return true;
								}
							};
							CS$<>8__locals3.textBlock.RectTransform.MinSize = new Point(0, (int)((float)enableButton.Rect.Height * 1.2f));
							continue;
						}
					}
					GUIButton subscribeButton = new GUIButton(new RectTransform(new Vector2(0.25f, 0.9f), CS$<>8__locals3.textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("downloadbutton"), Alignment.Center, "", null)
					{
						Enabled = false
					};
					CS$<>8__locals3.textBlock.RectTransform.MinSize = new Point(0, (int)((float)subscribeButton.Rect.Height * 1.2f));
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(24, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("GetMissingDependencyInfo");
					defaultInterpolatedStringHandler2.AppendFormatted<PublishedFileId>(CS$<>8__locals3.CS$<>8__locals2.dependency);
					TaskPool.Add(defaultInterpolatedStringHandler2.ToStringAndClear(), SteamManager.Workshop.GetItem(CS$<>8__locals3.CS$<>8__locals2.dependency), delegate(Task t)
					{
						Option<Item> itemOption;
						if (!t.TryGetResult(out itemOption))
						{
							return;
						}
						Item item;
						if (!itemOption.TryUnwrap(out item))
						{
							return;
						}
						if (!item.Title.IsNullOrEmpty())
						{
							CS$<>8__locals3.textBlock.Text = "- " + item.Title;
						}
						if (!item.IsSubscribed)
						{
							subscribeButton.OnClicked = delegate(GUIButton btn, object userdata)
							{
								item.Subscribe();
								subscribeButton.Enabled = false;
								CS$<>8__locals3.textBlock.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Green), 1.5f, false, false, null);
								return true;
							};
							subscribeButton.Enabled = true;
						}
					});
				}
			}
		}

		// Token: 0x060047BB RID: 18363 RVA: 0x00275224 File Offset: 0x00273424
		private string ExtractTitle(Either<Item, ContentPackage> itemOrPackage)
		{
			ContentPackage package;
			string result;
			if (!itemOrPackage.TryGet(out package))
			{
				if ((result = ((T)itemOrPackage).Title) == null)
				{
					return "";
				}
			}
			else
			{
				result = package.Name;
			}
			return result;
		}

		// Token: 0x060047BC RID: 18364 RVA: 0x0027525C File Offset: 0x0027345C
		private void CreateWorkshopItemDetailContainer(GUIFrame parent, out GUIListBox outerContainer, Action<Either<Item, ContentPackage>, GUIFrame> onSelected, Action onDeselected, out Action<Either<Item, ContentPackage>> select, out Action deselect)
		{
			MutableWorkshopMenu.<>c__DisplayClass29_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass29_0();
			CS$<>8__locals1.onDeselected = onDeselected;
			CS$<>8__locals1.onSelected = onSelected;
			CS$<>8__locals1.selectedItemOrPackage = null;
			CS$<>8__locals1.outContainer = new GUIListBox(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, null, null, true, false)
			{
				ScrollBarEnabled = false,
				ScrollBarVisible = false,
				HoverCursor = CursorState.Default
			};
			outerContainer = CS$<>8__locals1.outContainer;
			GUILayoutGroup selectedLayout = new GUILayoutGroup(new RectTransform(Vector2.One, outerContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup selectedHeaderLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.05f), selectedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			deselect = new Action(CS$<>8__locals1.<CreateWorkshopItemDetailContainer>g__deselectMethod|0);
			new GUIButton(new RectTransform(new ValueTuple<float, float>(0.04f, 1f), selectedHeaderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleLeft", null).OnClicked = delegate(GUIButton button, object o)
			{
				base.<CreateWorkshopItemDetailContainer>g__deselectMethod|0();
				return false;
			};
			GUIFrame padding = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.005f), selectedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			CS$<>8__locals1.selectedFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.945f), selectedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUICustomComponent selectionScroller = new GUICustomComponent(new RectTransform(Vector2.Zero, outerContainer.Parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
			{
				float targetScroll = (CS$<>8__locals1.selectedItemOrPackage == null) ? 0f : 1f;
				CS$<>8__locals1.outContainer.ScrollBar.BarScroll = (MathUtils.NearlyEqual(targetScroll, CS$<>8__locals1.outContainer.ScrollBar.BarScroll, 0.0001f) ? targetScroll : MathHelper.Lerp(CS$<>8__locals1.outContainer.ScrollBar.BarScroll, targetScroll, 0.3f));
			});
			select = delegate(Either<Item, ContentPackage> itemOrPackage)
			{
				CS$<>8__locals1.selectedFrame.ClearChildren();
				GUIDropDown[] dropdowns = CS$<>8__locals1.outContainer.Content.GetAllChildren<GUIDropDown>().ToArray<GUIDropDown>();
				IEnumerable<GUIComponent> allChildren = CS$<>8__locals1.outContainer.Content.GetAllChildren().Concat(CS$<>8__locals1.selectedFrame.GetAllChildren());
				allChildren.ForEach(delegate(GUIComponent c)
				{
					GUIComponent parent2 = c.Parent;
					c.ClampMouseRectToParent = !(((parent2 != null) ? parent2.Parent : null) is GUIDropDown);
				});
				CS$<>8__locals1.selectedItemOrPackage = itemOrPackage;
				CS$<>8__locals1.onSelected(itemOrPackage, CS$<>8__locals1.selectedFrame);
			};
		}

		// Token: 0x060047BD RID: 18365 RVA: 0x002754E8 File Offset: 0x002736E8
		private void CreateWorkshopItemList(GUIFrame parent, out GUIListBox outerContainer, out GUIListBox workshopItemList, Action<Item, GUIFrame> onSelected)
		{
			this.CreateWorkshopItemOrPackageList(parent, out outerContainer, out workshopItemList, delegate(Either<Item, ContentPackage> itemOrPackage, GUIFrame frame)
			{
				onSelected((T)itemOrPackage, frame);
			});
		}

		// Token: 0x060047BE RID: 18366 RVA: 0x00275518 File Offset: 0x00273718
		private GUIButton CreateShowInSteamButton(Item workshopItem, RectTransform rectT)
		{
			return new GUIButton(rectT, TextManager.Get("WorkshopShowItemInSteam"), Alignment.Center, "GUIButtonSmall", null)
			{
				OnClicked = delegate(GUIButton button, object o)
				{
					SteamManager.OverlayCustomUrl(workshopItem.Url);
					return false;
				}
			};
		}

		// Token: 0x060047BF RID: 18367 RVA: 0x00275564 File Offset: 0x00273764
		[return: Nullable(2)]
		private GUIButton CreateShowInSteamButton(Either<Item, ContentPackage> itemOrPackage)
		{
			Item workshopItem;
			if (!itemOrPackage.TryGet(out workshopItem))
			{
				return null;
			}
			return this.CreateShowInSteamButton(workshopItem);
		}

		// Token: 0x060047C0 RID: 18368 RVA: 0x0027558C File Offset: 0x0027378C
		private void CreateWorkshopItemOrPackageList(GUIFrame parent, out GUIListBox outerContainer, out GUIListBox workshopItemList, Action<Either<Item, ContentPackage>, GUIFrame> onSelected)
		{
			GUIListBox itemList = null;
			Action<Either<Item, ContentPackage>> select;
			Action deselect;
			this.CreateWorkshopItemDetailContainer(parent, out outerContainer, onSelected, delegate
			{
				GUIListBox itemList = itemList;
				if (itemList == null)
				{
					return;
				}
				itemList.Deselect();
			}, out select, out deselect);
			itemList = new GUIListBox(new RectTransform(Vector2.One, outerContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			itemList.RectTransform.SetAsFirstChild();
			workshopItemList = itemList;
			GUIComponent deselectCarrier = base.CreateActionCarrier(outerContainer.Content, "deselect".ToIdentifier(), deselect);
			itemList.OnSelected = delegate(GUIComponent component, object userData)
			{
				if (GUI.MouseOn.Parent != itemList.Content)
				{
					return false;
				}
				Either<Item, ContentPackage> itemOrPackage = userData as Either<Item, ContentPackage>;
				if (itemOrPackage == null)
				{
					return false;
				}
				select(itemOrPackage);
				return true;
			};
		}

		// Token: 0x060047C1 RID: 18369 RVA: 0x00275668 File Offset: 0x00273868
		private void AddUnpublishedMods(ISet<Item> workshopItems)
		{
			MutableWorkshopMenu.<>c__DisplayClass34_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass34_0();
			if (!this.selfModsListOption.TryUnwrap(out CS$<>8__locals1.selfModsList))
			{
				return;
			}
			if (SteamManager.IsFreeWeekend())
			{
				CS$<>8__locals1.<AddUnpublishedMods>g__clearWithMessage|0(TextManager.Get("FreeWeekendCantPublish"));
				return;
			}
			if (SteamManager.IsFamilyShared())
			{
				CS$<>8__locals1.<AddUnpublishedMods>g__clearWithMessage|0(TextManager.Get("FamilySharedCantPublish"));
				return;
			}
			CS$<>8__locals1.publishedItems = (from item in workshopItems
			select new ValueTuple<Item, ContentPackage>(item, ContentPackageManager.LocalPackages.FirstOrDefault(delegate(ContentPackage p)
			{
				SteamWorkshopId workshopId;
				return p.TryExtractSteamWorkshopId(out workshopId) && workshopId.Value == item.Id;
			})) into t
			orderby t.Item2 == null
			select t).ThenByDescending(delegate([TupleElementNames(new string[]
			{
				"item",
				null
			})] ValueTuple<Item, ContentPackage> t)
			{
				ContentPackage p = t.Item2;
				if (p == null)
				{
					return t.Item1.LatestUpdateTime;
				}
				return MutableWorkshopMenu.<AddUnpublishedMods>g__getEditTime|34_1(p);
			}).ToArray<ValueTuple<Item, ContentPackage>>();
			GUIComponent[] publishedGuiComponents = (from c in CS$<>8__locals1.selfModsList.Content.Children
			orderby base.<AddUnpublishedMods>g__indexOfUserDataInPublishedItemsArray|5(c.UserData)
			select c).ToArray<GUIComponent>();
			ContentPackage[] unpublishedMods = ContentPackageManager.LocalPackages.Where(delegate(ContentPackage p)
			{
				SteamWorkshopId workshopId;
				return !p.TryExtractSteamWorkshopId(out workshopId) || !CS$<>8__locals1.publishedItems.Any(([TupleElementNames(new string[]
				{
					"WorkshopItem",
					"LocalPackage"
				})] ValueTuple<Item, ContentPackage> item) => item.Item1.Id == workshopId.Value);
			}).OrderByDescending(new Func<ContentPackage, DateTime>(MutableWorkshopMenu.<AddUnpublishedMods>g__getEditTime|34_1)).ToArray<ContentPackage>();
			if (unpublishedMods.Any<ContentPackage>())
			{
				RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.09090909f), CS$<>8__locals1.selfModsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.Get("UnpublishedModsHeader");
				GUIFont font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			}
			foreach (ContentPackage unpublishedMod in unpublishedMods)
			{
				GUIFrame unpublishedFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.18181819f), CS$<>8__locals1.selfModsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
				{
					UserData = unpublishedMod
				};
				GUILayoutGroup unpublishedLayout = new GUILayoutGroup(new RectTransform(Vector2.One, unpublishedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				new GUIFrame(new RectTransform(Vector2.One, unpublishedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null).CanBeFocused = false;
				RectTransform rectT2 = new RectTransform(Vector2.One, unpublishedLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = unpublishedMod.Name + "\n\n" + TextManager.GetWithVariable("LastLocalEditTime", "[datetime]", MutableWorkshopMenu.<AddUnpublishedMods>g__getEditTime|34_1(unpublishedMod).ToString(), FormatCapitals.No);
				GUIFont font = GUIStyle.Font;
				new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
				unpublishedLayout.Recalculate();
			}
			if (publishedGuiComponents.Any<GUIComponent>())
			{
				RectTransform rectT3 = new RectTransform(new ValueTuple<float, float>(1f, 0.09090909f), CS$<>8__locals1.selfModsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = TextManager.Get("PublishedModsHeader");
				GUIFont font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			}
			foreach (GUIComponent c2 in publishedGuiComponents)
			{
				c2.SetAsLastChild();
				GUITextBlock textBlock = c2.FindChild((GUIComponent b) => b is GUITextBlock, true) as GUITextBlock;
				GUITextBlock guitextBlock = textBlock;
				RichString text4 = guitextBlock.Text;
				guitextBlock.Text = ((text4 != null) ? text4.ToString() : null) + "\n";
				int index = CS$<>8__locals1.<AddUnpublishedMods>g__indexOfUserDataInPublishedItemsArray|5(c2.UserData);
				ValueTuple<Item, ContentPackage> valueTuple = CS$<>8__locals1.publishedItems[index];
				Item workshopItem = valueTuple.Item1;
				ContentPackage localMod = valueTuple.Item2;
				if (localMod != null)
				{
					GUITextBlock guitextBlock2 = textBlock;
					guitextBlock2.Text += "\n" + TextManager.GetWithVariable("LastLocalEditTime", "[datetime]", MutableWorkshopMenu.<AddUnpublishedMods>g__getEditTime|34_1(localMod).ToString(), FormatCapitals.No);
				}
				GUITextBlock guitextBlock3 = textBlock;
				guitextBlock3.Text += "\n" + TextManager.GetWithVariable("LatestPublishTime", "[datetime]", workshopItem.LatestUpdateTime.ToLocalTime().ToString(), FormatCapitals.No);
			}
		}

		// Token: 0x060047C2 RID: 18370 RVA: 0x00275BE0 File Offset: 0x00273DE0
		[return: TupleElementNames(new string[]
		{
			"Button",
			"Sprite"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		private static ValueTuple<GUIButton, GUIFrame> CreatePaddedButton(RectTransform rectT, string style, float spriteScale)
		{
			GUIButton button = new GUIButton(rectT, Alignment.Center, null, null);
			GUIFrame sprite = new GUIFrame(new RectTransform(Vector2.One * spriteScale, button.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), style, null)
			{
				CanBeFocused = false
			};
			return new ValueTuple<GUIButton, GUIFrame>(button, sprite);
		}

		// Token: 0x060047C3 RID: 18371 RVA: 0x00275C54 File Offset: 0x00273E54
		private static void CreateSubscribeButton(Item workshopItem, RectTransform rectT, float spriteScale)
		{
			LocalizedString subscribeTooltip = TextManager.Get("DownloadButton");
			LocalizedString unsubscribeTooptip = TextManager.Get("WorkshopItemUnsubscribe");
			ValueTuple<GUIButton, GUIFrame> valueTuple = MutableWorkshopMenu.CreatePaddedButton(rectT, "GUIPlusButton", spriteScale);
			GUIButton subscribeButton = valueTuple.Item1;
			GUIFrame subscribeButtonSprite = valueTuple.Item2;
			subscribeButton.ToolTip = subscribeTooltip;
			subscribeButton.OnClicked = delegate(GUIButton button, object o)
			{
				if (!SteamManager.IsInitialized)
				{
					return false;
				}
				if (!workshopItem.IsSubscribed)
				{
					workshopItem.Subscribe();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
					defaultInterpolatedStringHandler.AppendLiteral("DownloadSubscribedItem");
					defaultInterpolatedStringHandler.AppendFormatted<PublishedFileId>(workshopItem.Id);
					string name = defaultInterpolatedStringHandler.ToStringAndClear();
					Task task = SteamManager.Workshop.ForceRedownload(workshopItem, null);
					Action<Task> onCompletion;
					if ((onCompletion = MutableWorkshopMenu.<>O.<1>__IgnoredCallback) == null)
					{
						onCompletion = (MutableWorkshopMenu.<>O.<1>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
					}
					TaskPool.Add(name, task, onCompletion);
				}
				else
				{
					workshopItem.Unsubscribe();
					SteamManager.Workshop.Uninstall(workshopItem);
				}
				return false;
			};
			GUICustomComponent buttonStyleUpdater = new GUICustomComponent(new RectTransform(Vector2.Zero, subscribeButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float deltaTime, GUICustomComponent component)
			{
				if (!SteamManager.IsInitialized)
				{
					return;
				}
				GUIComponentStyle style = subscribeButtonSprite.Style;
				if (style != null)
				{
					Identifier styleId = style.Identifier;
					if (workshopItem.IsSubscribed && styleId != "GUIMinusButton")
					{
						subscribeButtonSprite.ApplyStyle(GUIStyle.GetComponentStyle("GUIMinusButton"));
						subscribeButton.ToolTip = unsubscribeTooptip;
					}
					if (!workshopItem.IsSubscribed && styleId != "GUIPlusButton")
					{
						subscribeButtonSprite.ApplyStyle(GUIStyle.GetComponentStyle("GUIPlusButton"));
						subscribeButton.ToolTip = subscribeTooltip;
					}
				}
			});
			float displayedDownloadAmount = workshopItem.DownloadAmount;
			new GUICustomComponent(new RectTransform(new ValueTuple<float, float>(1.22f, 1.22f), subscribeButtonSprite.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				MutableWorkshopMenu.<>c__DisplayClass36_1 CS$<>8__locals2;
				CS$<>8__locals2.spriteBatch = spriteBatch;
				CS$<>8__locals2.component = component;
				if (!SteamManager.IsInitialized)
				{
					return;
				}
				if (!workshopItem.IsSubscribed || (!workshopItem.IsDownloading && !workshopItem.IsDownloadPending && MathUtils.NearlyEqual(workshopItem.DownloadAmount, displayedDownloadAmount, 0.0001f)))
				{
					return;
				}
				MutableWorkshopMenu.<CreateSubscribeButton>g__drawSectionFuzzy|36_5(1f, Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.Black, GUIStyle.Blue, 0.2f), (float)CS$<>8__locals2.component.Rect.Width * 0.25f, ref CS$<>8__locals2);
				MutableWorkshopMenu.<CreateSubscribeButton>g__drawSectionFuzzy|36_5(1f, Microsoft.Xna.Framework.Color.Black, (float)CS$<>8__locals2.component.Rect.Width * 0.15f, ref CS$<>8__locals2);
				MutableWorkshopMenu.<CreateSubscribeButton>g__drawSectionFuzzy|36_5(displayedDownloadAmount, GUIStyle.Green, (float)CS$<>8__locals2.component.Rect.Width * 0.08f, ref CS$<>8__locals2);
			}, delegate(float deltaTime, GUICustomComponent component)
			{
				if (!SteamManager.IsInitialized)
				{
					return;
				}
				displayedDownloadAmount = Math.Min(workshopItem.DownloadAmount, MathHelper.Lerp(displayedDownloadAmount, workshopItem.DownloadAmount, 0.05f));
			}).CanBeFocused = false;
		}

		// Token: 0x060047C4 RID: 18372 RVA: 0x00275DA4 File Offset: 0x00273FA4
		private void PopulateItemList(GUIListBox itemListBox, Task<ISet<Item>> items, bool includeSubscribeButton, [Nullable(new byte[]
		{
			2,
			1
		})] Action<ISet<Item>> onFill = null)
		{
			itemListBox.ClearChildren();
			itemListBox.Deselect();
			itemListBox.ScrollBar.BarScroll = 0f;
			TaskPool.AddIfNotFound("PopulateTabWithItemList", items, delegate(Task t)
			{
				this.taskCancelSrc = (this.taskCancelSrc.IsCancellationRequested ? new CancellationTokenSource() : this.taskCancelSrc);
				itemListBox.ClearChildren();
				ISet<Item> workshopItems = ((Task<ISet<Item>>)t).Result;
				foreach (Item workshopItem in workshopItems)
				{
					GUIFrame itemFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.18181819f), itemListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
					{
						UserData = workshopItem
					};
					GUILayoutGroup itemLayout = new GUILayoutGroup(new RectTransform(Vector2.One, itemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						Stretch = true
					};
					GUIFrame thumbnailContainer = this.CreateThumbnailContainer(itemLayout, Vector2.One, ScaleBasis.BothHeight);
					this.CreateItemThumbnail(workshopItem, this.taskCancelSrc.Token, thumbnailContainer);
					thumbnailContainer.CanBeFocused = false;
					thumbnailContainer.GetAllChildren().ForEach(delegate(GUIComponent c)
					{
						c.CanBeFocused = false;
					});
					RectTransform rectT = new RectTransform(Vector2.One, itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text = workshopItem.Title ?? "";
					GUIFont font = GUIStyle.Font;
					new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
					if (includeSubscribeButton)
					{
						MutableWorkshopMenu.CreateSubscribeButton(workshopItem, new RectTransform(Vector2.One, itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), 0.4f);
					}
					itemLayout.Recalculate();
				}
				Action<ISet<Item>> onFill2 = onFill;
				if (onFill2 == null)
				{
					return;
				}
				onFill2(workshopItems);
			});
		}

		// Token: 0x060047C5 RID: 18373 RVA: 0x00275E18 File Offset: 0x00274018
		private GUIFrame CreateThumbnailContainer(GUIComponent parent, Vector2 relativeSize, ScaleBasis scaleBasis)
		{
			return new GUIFrame(new RectTransform(relativeSize, parent.RectTransform, Anchor.TopLeft, null, null, null, scaleBasis), "GUIFrameListBox", null);
		}

		// Token: 0x060047C6 RID: 18374 RVA: 0x00275E64 File Offset: 0x00274064
		private SteamManager.Workshop.ItemThumbnail CreateItemThumbnail(in Item workshopItem, CancellationToken cancellationToken, GUIFrame thumbnailContainer)
		{
			SteamManager.Workshop.ItemThumbnail thumbnail = new SteamManager.Workshop.ItemThumbnail(ref workshopItem, cancellationToken);
			this.itemThumbnails.Add(thumbnail);
			this.CreateAsyncThumbnailComponent(thumbnailContainer, () => thumbnail.Texture, () => thumbnail.Loading);
			return thumbnail;
		}

		// Token: 0x060047C7 RID: 18375 RVA: 0x00275EBC File Offset: 0x002740BC
		private GUICustomComponent CreateAsyncThumbnailComponent(GUIFrame thumbnailContainer, [Nullable(new byte[]
		{
			1,
			2
		})] Func<Texture2D> textureGetter, Func<bool> throbberEnabled)
		{
			int randomThrobberOffset = Rand.Range(0, 10, Rand.RandSync.Unsynced);
			return new GUICustomComponent(new RectTransform(Vector2.One, thumbnailContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
			{
				Rectangle rect = component.Rect;
				Texture2D texture = textureGetter();
				if (texture != null)
				{
					rect.Location += new ValueTuple<int, int>(4, 4);
					rect.Size -= new ValueTuple<int, int>(8, 8);
					Point destinationSizeMaxWidth = new ValueTuple<int, int>(rect.Width, rect.Width * texture.Height / texture.Width);
					Point destinationSizeMaxHeight = new ValueTuple<int, int>(rect.Height * texture.Width / texture.Height, rect.Height);
					Point destinationSize = (destinationSizeMaxHeight.X > rect.Width) ? destinationSizeMaxWidth : destinationSizeMaxHeight;
					Rectangle destinationRectangle = new Rectangle(rect.Center.X - destinationSize.X / 2, rect.Center.Y - destinationSize.Y / 2, destinationSize.X, destinationSize.Y);
					spriteBatch.Draw(texture, destinationRectangle, Microsoft.Xna.Framework.Color.White);
					return;
				}
				if (throbberEnabled())
				{
					GUISpriteSheet sheet = GUIStyle.GenericThrobber;
					Vector2 pos = rect.Center.ToVector2() - Vector2.One * (float)rect.Height * 0.4f;
					sheet.Draw(spriteBatch, ((int)Math.Floor(Timing.TotalTime * 24.0) + randomThrobberOffset) % sheet.FrameCount, pos, Microsoft.Xna.Framework.Color.White, Vector2.Zero, 0f, Vector2.One * (float)component.Rect.Height / sheet.FrameSize.ToVector2() * 0.8f, SpriteEffects.None, null);
				}
			}, null);
		}

		// Token: 0x060047C8 RID: 18376 RVA: 0x00275F2C File Offset: 0x0027412C
		private GUIListBox CreateTagsList(IEnumerable<Identifier> tags, RectTransform rectT, bool canBeFocused)
		{
			GUIListBox tagsList = new GUIListBox(rectT, false, null, null, true, false)
			{
				UseGridLayout = true,
				ScrollBarEnabled = true,
				ScrollBarVisible = false,
				HideChildrenOutsideFrame = true,
				Spacing = GUI.IntScale(4f)
			};
			tagsList.Content.ClampMouseRectToParent = false;
			foreach (Identifier tag in tags)
			{
				GUIButton tagBtn = new GUIButton(new RectTransform(new Vector2(0.25f, 0.125f), tagsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("workshop.contenttag." + tag.Value.RemoveWhitespace()).Fallback(tag.Value.CapitaliseFirstInvariant(), true), Alignment.Center, "GUIButtonRound", null)
				{
					CanBeFocused = canBeFocused,
					Selected = !canBeFocused,
					UserData = tag
				};
				tagBtn.RectTransform.NonScaledSize = tagBtn.Font.MeasureString(tagBtn.Text, false).ToPoint() + new Point(GUI.IntScale(15f), GUI.IntScale(5f));
				tagBtn.RectTransform.IsFixedSize = true;
				tagBtn.ClampMouseRectToParent = false;
			}
			return tagsList;
		}

		// Token: 0x060047C9 RID: 18377 RVA: 0x002760D0 File Offset: 0x002742D0
		private void PopulateFrameWithItemInfo(Item workshopItem, GUIFrame parentFrame)
		{
			MutableWorkshopMenu.<>c__DisplayClass42_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass42_0();
			CS$<>8__locals1.workshopItem = workshopItem;
			this.ViewingItemDetails = true;
			this.taskCancelSrc = (this.taskCancelSrc.IsCancellationRequested ? new CancellationTokenSource() : this.taskCancelSrc);
			CS$<>8__locals1.contentPackage = ContentPackageManager.WorkshopPackages.FirstOrDefault(delegate(ContentPackage p)
			{
				SteamWorkshopId workshopId;
				return p.TryExtractSteamWorkshopId(out workshopId) && workshopId.Value == CS$<>8__locals1.workshopItem.Id;
			});
			GUILayoutGroup verticalLayout = new GUILayoutGroup(new RectTransform(Vector2.One, parentFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			CS$<>8__locals1.headerLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.1f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUILayoutGroup titleAndAuthorLayout = new GUILayoutGroup(new RectTransform(Vector2.One, CS$<>8__locals1.headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.5f), titleAndAuthorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CS$<>8__locals1.workshopItem.Title ?? "";
			GUIFont font = GUIStyle.LargeFont;
			GUITextBlock selectedTitle = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			CS$<>8__locals1.author = CS$<>8__locals1.workshopItem.Owner;
			CS$<>8__locals1.authorButton = new GUIButton(new RectTransform(new ValueTuple<float, float>(1f, 0.5f), titleAndAuthorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.CenterLeft, null, null)
			{
				ForceUpperCase = ForceUpperCase.No,
				Font = GUIStyle.SubHeadingFont,
				TextColor = GUIStyle.TextColorNormal,
				HoverTextColor = Microsoft.Xna.Framework.Color.White,
				SelectedTextColor = GUIStyle.TextColorNormal,
				OnClicked = delegate(GUIButton button, object o)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("https://steamcommunity.com/profiles/");
					defaultInterpolatedStringHandler3.AppendFormatted<Steamworks.SteamId>(CS$<>8__locals1.author.Id);
					defaultInterpolatedStringHandler3.AppendLiteral("/myworkshopfiles/?appid=");
					defaultInterpolatedStringHandler3.AppendFormatted<uint>(602960U);
					SteamManager.OverlayCustomUrl(defaultInterpolatedStringHandler3.ToStringAndClear());
					return false;
				}
			};
			CS$<>8__locals1.authorPadding = CS$<>8__locals1.authorButton.GetChild<GUITextBlock>().Padding;
			ValueTuple<GUIButton, GUIFrame> valueTuple = MutableWorkshopMenu.CreatePaddedButton(CS$<>8__locals1.<PopulateFrameWithItemInfo>g__rightSideButtonRectT|1(), "GUIUpdateButton", 0.8f);
			CS$<>8__locals1.updateButton = valueTuple.Item1;
			CS$<>8__locals1.updateSprite = valueTuple.Item2;
			CS$<>8__locals1.updateButton.ToolTip = TextManager.Get("WorkshopItemUpdate");
			CS$<>8__locals1.updateButton.Visible = false;
			CS$<>8__locals1.updateButton.OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<PopulateFrameWithItemInfo>g__reinstallAction|2);
			if (CS$<>8__locals1.contentPackage != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler.AppendLiteral("DetermineUpdateRequired");
				defaultInterpolatedStringHandler.AppendFormatted<Option<ContentPackageId>>(CS$<>8__locals1.contentPackage.UgcId);
				TaskPool.AddIfNotFound(defaultInterpolatedStringHandler.ToStringAndClear(), CS$<>8__locals1.contentPackage.IsUpToDate(), delegate(Task t)
				{
					bool isUpToDate;
					if (!t.TryGetResult(out isUpToDate))
					{
						return;
					}
					CS$<>8__locals1.updateButton.Visible = !isUpToDate;
				});
			}
			valueTuple = MutableWorkshopMenu.CreatePaddedButton(CS$<>8__locals1.<PopulateFrameWithItemInfo>g__rightSideButtonRectT|1(), "GUIReloadButton", 0.8f);
			CS$<>8__locals1.reinstallButton = valueTuple.Item1;
			CS$<>8__locals1.reinstallSprite = valueTuple.Item2;
			CS$<>8__locals1.reinstallButton.ToolTip = TextManager.Get("WorkshopItemReinstall");
			CS$<>8__locals1.reinstallButton.OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.<PopulateFrameWithItemInfo>g__reinstallAction|2);
			GUICustomComponent reinstallButtonUpdater = new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.reinstallButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				GUIComponent reinstallButton = CS$<>8__locals1.reinstallButton;
				bool visible;
				if (!CS$<>8__locals1.workshopItem.IsSubscribed)
				{
					visible = (CS$<>8__locals1.workshopItem.Owner.Id == (from steamId in SteamManager.GetSteamId()
					select steamId.Value).Fallback(0UL));
				}
				else
				{
					visible = true;
				}
				reinstallButton.Visible = visible;
				CS$<>8__locals1.reinstallButton.Enabled = (!CS$<>8__locals1.workshopItem.IsDownloading && !CS$<>8__locals1.workshopItem.IsDownloadPending && !SteamManager.Workshop.IsInstalling(CS$<>8__locals1.workshopItem));
				CS$<>8__locals1.reinstallSprite.Color = (CS$<>8__locals1.reinstallButton.Enabled ? CS$<>8__locals1.reinstallSprite.Style.Color : Microsoft.Xna.Framework.Color.DimGray);
				CS$<>8__locals1.updateButton.Enabled = (CS$<>8__locals1.reinstallButton.Enabled && CS$<>8__locals1.contentPackage != null && ContentPackageManager.WorkshopPackages.Contains(CS$<>8__locals1.contentPackage));
				CS$<>8__locals1.updateSprite.Color = CS$<>8__locals1.reinstallSprite.Color;
				if (CS$<>8__locals1.contentPackage != null && !ContentPackageManager.WorkshopPackages.Contains(CS$<>8__locals1.contentPackage))
				{
					IEnumerable<ContentPackage> workshopPackages = ContentPackageManager.WorkshopPackages;
					Func<ContentPackage, bool> predicate;
					if ((predicate = CS$<>8__locals1.<>9__10) == null)
					{
						predicate = (CS$<>8__locals1.<>9__10 = delegate(ContentPackage p)
						{
							SteamWorkshopId workshopId;
							return p.TryExtractSteamWorkshopId(out workshopId) && workshopId.Value == CS$<>8__locals1.workshopItem.Id;
						});
					}
					if (workshopPackages.Any(predicate))
					{
						CS$<>8__locals1.updateButton.Visible = false;
					}
				}
			});
			MutableWorkshopMenu.CreateSubscribeButton(CS$<>8__locals1.workshopItem, CS$<>8__locals1.<PopulateFrameWithItemInfo>g__rightSideButtonRectT|1(), 0.8f);
			GUIFrame padding = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.15f, 1f), CS$<>8__locals1.headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null);
			padding = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.015f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup horizontalLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 0.45f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Request username for ");
			defaultInterpolatedStringHandler2.AppendFormatted<Steamworks.SteamId>(CS$<>8__locals1.author.Id);
			TaskPool.Add(defaultInterpolatedStringHandler2.ToStringAndClear(), CS$<>8__locals1.author.RequestInfoAsync(), delegate(Task t)
			{
				CS$<>8__locals1.authorButton.Text = (CS$<>8__locals1.author.Name ?? "");
				CS$<>8__locals1.authorButton.RectTransform.NonScaledSize = new ValueTuple<int, int>((int)(CS$<>8__locals1.authorButton.Font.MeasureString(CS$<>8__locals1.author.Name ?? "", false).X + CS$<>8__locals1.authorPadding.X + CS$<>8__locals1.authorPadding.Z), CS$<>8__locals1.authorButton.RectTransform.NonScaledSize.Y);
			});
			GUIFrame thumbnailSuperContainer = new GUIFrame(new RectTransform(Vector2.One, horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null);
			GUIFrame thumbnailContainer = this.CreateThumbnailContainer(thumbnailSuperContainer, Vector2.One, ScaleBasis.BothHeight);
			this.CreateItemThumbnail(CS$<>8__locals1.workshopItem, this.taskCancelSrc.Token, thumbnailContainer);
			thumbnailContainer.RectTransform.Anchor = Anchor.Center;
			thumbnailContainer.RectTransform.Pivot = Pivot.Center;
			GUIFrame statsBox = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.6f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIFrameListBox", null);
			GUILayoutGroup statsHorizontalLayout = new GUILayoutGroup(new RectTransform(Vector2.One, statsBox.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			CS$<>8__locals1.statsVertical0 = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(1f, 1f), statsHorizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			CS$<>8__locals1.<PopulateFrameWithItemInfo>g__statFrame|6("", "");
			GUIFrame scoreFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.12f), CS$<>8__locals1.statsVertical0.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			RectTransform rectT2 = new RectTransform(new ValueTuple<float, float>(0.4f, 1f), scoreFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("WorkshopItemScore");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock scoreLabel = new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			GUILayoutGroup scoreStarContainer = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.6f, 1f), scoreFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			Microsoft.Xna.Framework.Color starColor = Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.White, Microsoft.Xna.Framework.Color.Yellow, Math.Min(CS$<>8__locals1.workshopItem.Score * 2f, 1f)), Microsoft.Xna.Framework.Color.Lime, Math.Max(0f, (CS$<>8__locals1.workshopItem.Score - 0.5f) * 2f));
			for (int i = 0; i < 5; i++)
			{
				bool isStarLit = i <= WorkshopMenu.Round(CS$<>8__locals1.workshopItem.Score * 5f);
				GUIFrame star = new GUIFrame(new RectTransform(Vector2.One, scoreStarContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), isStarLit ? "GUIStarIconBright" : "GUIStarIconDark", null);
				if (isStarLit)
				{
					star.Color = starColor;
					star.HoverColor = starColor;
					star.SelectedColor = starColor;
				}
			}
			GUIFrame scoreTextPadding = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), scoreStarContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), null, null);
			GUIFrame scoreTextContainer = new GUIFrame(new RectTransform(Vector2.One, scoreStarContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 1.5f), scoreTextContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("WorkshopItemVotes", "[VoteCount]", (CS$<>8__locals1.workshopItem.VotesUp + CS$<>8__locals1.workshopItem.VotesDown).ToString(), FormatCapitals.No), null, null, Alignment.BottomLeft, false, "", null).Padding = Vector4.Zero;
			new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 1.5f), scoreTextContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("WorkshopItemSubscriptions", "[SubscriptionCount]", CS$<>8__locals1.workshopItem.NumUniqueSubscriptions.ToString(), FormatCapitals.No), null, null, Alignment.TopLeft, false, "", null).Padding = Vector4.Zero;
			CS$<>8__locals1.<PopulateFrameWithItemInfo>g__statFrame|6(TextManager.Get("WorkshopItemFileSize"), MathUtils.GetBytesReadable(CS$<>8__locals1.workshopItem.SizeOfFileInBytes));
			CS$<>8__locals1.<PopulateFrameWithItemInfo>g__statFrame|6(TextManager.Get("WorkshopItemCreationDate"), CS$<>8__locals1.workshopItem.Created.ToShortDateString());
			CS$<>8__locals1.<PopulateFrameWithItemInfo>g__statFrame|6(TextManager.Get("WorkshopItemModificationDate"), CS$<>8__locals1.workshopItem.Updated.ToShortDateString());
			RectTransform rectT3 = new RectTransform(new ValueTuple<float, float>(1f, 0.12f), CS$<>8__locals1.statsVertical0.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("WorkshopItemTags");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock tagsLabel = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
			this.CreateTagsList((CS$<>8__locals1.workshopItem.Tags ?? Array.Empty<string>()).ToIdentifiers(), new RectTransform(new ValueTuple<float, float>(0.97f, 0.3f), CS$<>8__locals1.statsVertical0.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false);
			GUIListBox descriptionListBox = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.38f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			WorkshopMenu.CreateBBCodeElement(CS$<>8__locals1.workshopItem, descriptionListBox);
			GUIFrame showInSteamContainer = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.05f), verticalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.CreateShowInSteamButton(CS$<>8__locals1.workshopItem, new RectTransform(new ValueTuple<float, float>(0.2f, 1f), showInSteamContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal));
		}

		// Token: 0x060047CA RID: 18378 RVA: 0x00276DDC File Offset: 0x00274FDC
		private bool OpenLoadPreset(GUIButton _, object __)
		{
			this.OpenLoadPreset();
			return false;
		}

		// Token: 0x060047CB RID: 18379 RVA: 0x00276DE8 File Offset: 0x00274FE8
		private void OpenLoadPreset()
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("LoadModListPresetHeader"), "", new LocalizedString[]
			{
				TextManager.Get("Load"),
				TextManager.Get("Cancel")
			}, new Vector2?(new ValueTuple<float, float>(0.4f, 0.6f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUIListBox presetListBox = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.7f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			ValueTuple<string, XDocument>[] array;
			if (!Directory.Exists("ModLists"))
			{
				array = Array.Empty<ValueTuple<string, XDocument>>();
			}
			else
			{
				array = (from d in Directory.GetFiles("ModLists").Select(new Func<string, ValueTuple<string, XDocument>>(MutableWorkshopMenu.<OpenLoadPreset>g__tryLoadXml|44_0))
				where d.Item2 != null
				select d).ToArray<ValueTuple<string, XDocument>>();
			}
			ValueTuple<string, XDocument>[] presets = array;
			ValueTuple<string, XDocument>[] array2 = presets;
			ModListPreset preset;
			for (int i = 0; i < array2.Length; i++)
			{
				ValueTuple<string, XDocument> doc = array2[i];
				preset = new ModListPreset(doc.Item2);
				GUIFrame presetFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.09f), presetListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
				{
					UserData = preset,
					ToolTip = preset.GetTooltip()
				};
				new GUITextBlock(new RectTransform(Vector2.One, presetFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), preset.Name, null, null, Alignment.Left, false, "", null).CanBeFocused = false;
				new GUIButton(new RectTransform(new ValueTuple<float, float>(0.2f, 1f), presetFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Delete"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object o)
				{
					File.Delete(doc.Item1, true);
					presetListBox.Content.RemoveChild(presetFrame);
					return false;
				};
			}
			Action<GUIComponent> <>9__7;
			Action<GUIComponent> <>9__8;
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				object selectedData = presetListBox.SelectedData;
				if (selectedData is ModListPreset)
				{
					ModListPreset preset = (ModListPreset)selectedData;
					GUIComponent[] allChildren = this.enabledRegularModsList.Content.Children.Concat(this.disabledRegularModsList.Content.Children).ToArray<GUIComponent>();
					this.enabledRegularModsList.ClearChildren();
					this.disabledRegularModsList.ClearChildren();
					GUIComponent[] toEnable = allChildren.Where(delegate(GUIComponent c)
					{
						RegularPackage p = c.UserData as RegularPackage;
						return p != null && preset.RegularPackages.Contains(p);
					}).OrderBy(delegate(GUIComponent c)
					{
						RegularPackage p = c.UserData as RegularPackage;
						if (p == null)
						{
							return int.MaxValue;
						}
						return preset.RegularPackages.IndexOf(p);
					}).ToArray<GUIComponent>();
					GUIComponent[] toDisable = (from c in allChildren
					where !toEnable.Contains(c)
					select c).ToArray<GUIComponent>();
					IEnumerable<GUIComponent> toEnable2 = toEnable;
					Action<GUIComponent> action;
					if ((action = <>9__7) == null)
					{
						action = (<>9__7 = delegate(GUIComponent c)
						{
							c.RectTransform.Parent = this.enabledRegularModsList.Content.RectTransform;
						});
					}
					toEnable2.ForEach(action);
					IEnumerable<GUIComponent> source = toDisable;
					Action<GUIComponent> action2;
					if ((action2 = <>9__8) == null)
					{
						action2 = (<>9__8 = delegate(GUIComponent c)
						{
							c.RectTransform.Parent = this.disabledRegularModsList.Content.RectTransform;
						});
					}
					source.ForEach(action2);
					this.enabledCoreDropdown.SelectItem(preset.CorePackage);
				}
				msgBox.Close();
				return false;
			};
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
		}

		// Token: 0x060047CC RID: 18380 RVA: 0x00277129 File Offset: 0x00275329
		private bool OpenSavePreset(GUIButton _, object __)
		{
			this.OpenSavePreset();
			return false;
		}

		// Token: 0x060047CD RID: 18381 RVA: 0x00277134 File Offset: 0x00275334
		private void OpenSavePreset()
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("SaveModListPresetHeader"), "", new LocalizedString[]
			{
				TextManager.Get("Save"),
				TextManager.Get("Cancel")
			}, new Vector2?(new ValueTuple<float, float>(0.4f, 0.2f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUITextBox nameBox = new GUITextBox(new RectTransform(new ValueTuple<float, float>(1f, 0.3f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				if (nameBox.Text.IsNullOrEmpty())
				{
					nameBox.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Red), 1.5f, false, false, null);
					return false;
				}
				CorePackage corePackage = this.enabledCoreDropdown.SelectedData as CorePackage;
				if (corePackage != null)
				{
					ModListPreset preset = new ModListPreset(nameBox.Text, corePackage, (from c in this.enabledRegularModsList.Content.Children
					select c.UserData).OfType<RegularPackage>().ToArray<RegularPackage>());
					preset.Save();
				}
				msgBox.Close();
				return false;
			};
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
		}

		// Token: 0x17001242 RID: 4674
		// (get) Token: 0x060047CE RID: 18382 RVA: 0x00277280 File Offset: 0x00275480
		// (set) Token: 0x060047CF RID: 18383 RVA: 0x00277288 File Offset: 0x00275488
		public MutableWorkshopMenu.Tab CurrentTab { get; private set; }

		// Token: 0x17001243 RID: 4675
		// (get) Token: 0x060047D0 RID: 18384 RVA: 0x00277291 File Offset: 0x00275491
		private static bool EnableWorkshopSupport
		{
			get
			{
				return SteamManager.IsInitialized;
			}
		}

		// Token: 0x060047D1 RID: 18385 RVA: 0x00277298 File Offset: 0x00275498
		public MutableWorkshopMenu(GUIFrame parent) : base(parent)
		{
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = GUI.IntScale(4f)
			};
			Vector2 tabberSize = MutableWorkshopMenu.EnableWorkshopSupport ? new ValueTuple<float, float>(1f, 0.05f) : Vector2.Zero;
			this.tabber = new GUILayoutGroup(new RectTransform(tabberSize, mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			this.tabContents = new Dictionary<MutableWorkshopMenu.Tab, ValueTuple<GUIButton, GUIFrame>>();
			if (MutableWorkshopMenu.EnableWorkshopSupport)
			{
				new GUIButton(new RectTransform(new ValueTuple<float, float>(1f, 0.05f), mainLayout.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("FindModsButton"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object o)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
					defaultInterpolatedStringHandler.AppendLiteral("https://steamcommunity.com/app/");
					defaultInterpolatedStringHandler.AppendFormatted<uint>(602960U);
					defaultInterpolatedStringHandler.AppendLiteral("/workshop/");
					SteamManager.OverlayCustomUrl(defaultInterpolatedStringHandler.ToStringAndClear());
					return false;
				};
			}
			else
			{
				this.tabber.Visible = false;
			}
			this.contentFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.95f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUICustomComponent(new RectTransform(Vector2.Zero, mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				this.UpdateSubscribedModInstalls();
			});
			this.CreateInstalledModsTab(out this.enabledCoreDropdown, out this.enabledRegularModsList, out this.disabledRegularModsList, out this.onInstalledInfoButtonHit, out this.modsListFilter, out this.modsListFilterTickboxes, out this.bulkUpdateButtonOption);
			if (MutableWorkshopMenu.EnableWorkshopSupport)
			{
				GUIListBox popularModList;
				this.CreatePopularModsTab(out popularModList);
				GUIListBox selfModsList;
				this.CreatePublishTab(out selfModsList);
				this.popularModsListOption = Option<GUIListBox>.Some(popularModList);
				this.selfModsListOption = Option<GUIListBox>.Some(selfModsList);
			}
			else
			{
				Option.UnspecifiedNone none = Option.None;
				this.popularModsListOption = none;
				none = Option.None;
				this.selfModsListOption = none;
			}
			this.SelectTab(MutableWorkshopMenu.Tab.InstalledMods);
		}

		// Token: 0x060047D2 RID: 18386 RVA: 0x00277535 File Offset: 0x00275735
		private void SwitchContent(GUIFrame newContent)
		{
			this.contentFrame.Children.ForEach(delegate(GUIComponent c)
			{
				c.Visible = false;
			});
			newContent.Visible = true;
		}

		// Token: 0x060047D3 RID: 18387 RVA: 0x00277570 File Offset: 0x00275770
		public void SelectTab(MutableWorkshopMenu.Tab tab)
		{
			this.CurrentTab = tab;
			this.SwitchContent(this.tabContents[tab].Item2);
			this.tabber.Children.ForEach(delegate(GUIComponent c)
			{
				GUIButton btn = c as GUIButton;
				if (btn != null)
				{
					btn.Selected = (btn == this.tabContents[tab].Item1);
				}
			});
			if (!this.taskCancelSrc.IsCancellationRequested)
			{
				this.taskCancelSrc.Cancel();
			}
			this.itemThumbnails.ForEach(delegate(SteamManager.Workshop.ItemThumbnail t)
			{
				t.Dispose();
			});
			this.itemThumbnails.Clear();
			switch (tab)
			{
			case MutableWorkshopMenu.Tab.InstalledMods:
				this.PopulateInstalledModLists(false, true);
				return;
			case MutableWorkshopMenu.Tab.PopularMods:
			{
				GUIListBox popularModsList;
				if (this.popularModsListOption.TryUnwrap(out popularModsList))
				{
					this.PopulateItemList(popularModsList, SteamManager.Workshop.GetPopularItems(), true, null);
					return;
				}
				break;
			}
			case MutableWorkshopMenu.Tab.Publish:
			{
				GUIListBox selfModsList;
				if (this.selfModsListOption.TryUnwrap(out selfModsList))
				{
					this.PopulateItemList(selfModsList, SteamManager.Workshop.GetPublishedItems(), false, new Action<ISet<Item>>(this.AddUnpublishedMods));
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x060047D4 RID: 18388 RVA: 0x0027768C File Offset: 0x0027588C
		private void AddButtonToTabber(MutableWorkshopMenu.Tab tab, GUIFrame content)
		{
			RectTransform rectT = new RectTransform(Vector2.One, this.tabber.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("workshopmenutab.");
			defaultInterpolatedStringHandler.AppendFormatted<MutableWorkshopMenu.Tab>(tab);
			GUIButton button = new GUIButton(rectT, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()), Alignment.Center, "GUITabButton", null)
			{
				OnClicked = delegate(GUIButton b, object _)
				{
					if (tab != this.CurrentTab)
					{
						this.SelectTab(tab);
					}
					return false;
				}
			};
			button.RectTransform.MaxSize = RectTransform.MaxPoint;
			button.Children.ForEach(delegate(GUIComponent c)
			{
				c.RectTransform.MaxSize = RectTransform.MaxPoint;
			});
			this.tabContents.Add(tab, new ValueTuple<GUIButton, GUIFrame>(button, content));
		}

		// Token: 0x060047D5 RID: 18389 RVA: 0x00277788 File Offset: 0x00275988
		private GUIFrame CreateNewContentFrame(MutableWorkshopMenu.Tab tab)
		{
			GUIFrame content = new GUIFrame(new RectTransform(Vector2.One * 0.98f, this.contentFrame.RectTransform, Anchor.Center, new Pivot?(Pivot.Center), null, null, ScaleBasis.Normal), null, null);
			this.AddButtonToTabber(tab, content);
			return content;
		}

		// Token: 0x060047D6 RID: 18390 RVA: 0x002777E8 File Offset: 0x002759E8
		private void CreatePopularModsTab(out GUIListBox popularModsList)
		{
			GUIFrame content = this.CreateNewContentFrame(MutableWorkshopMenu.Tab.PopularMods);
			if (!SteamManager.IsInitialized)
			{
				this.tabContents[MutableWorkshopMenu.Tab.PopularMods].Item1.Enabled = false;
			}
			GUIFrame listFrame = new GUIFrame(new RectTransform(Vector2.One, content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIListBox guilistBox;
			this.CreateWorkshopItemList(listFrame, out guilistBox, out popularModsList, new Action<Item, GUIFrame>(this.PopulateFrameWithItemInfo));
		}

		// Token: 0x060047D7 RID: 18391 RVA: 0x00277870 File Offset: 0x00275A70
		private void CreatePublishTab(out GUIListBox selfModsList)
		{
			GUIFrame content = this.CreateNewContentFrame(MutableWorkshopMenu.Tab.Publish);
			if (!SteamManager.IsInitialized)
			{
				this.tabContents[MutableWorkshopMenu.Tab.Publish].Item1.Enabled = false;
			}
			GUIListBox guilistBox;
			this.CreateWorkshopItemOrPackageList(content, out guilistBox, out selfModsList, new Action<Either<Item, ContentPackage>, GUIFrame>(this.PopulatePublishTab));
		}

		// Token: 0x060047D8 RID: 18392 RVA: 0x002778BC File Offset: 0x00275ABC
		public void Apply()
		{
			ContentPackageManager.EnabledPackages.SetCore(this.EnabledCorePackage);
			ContentPackageManager.EnabledPackages.SetRegular((from c in this.enabledRegularModsList.Content.Children
			select c.UserData as RegularPackage).OfType<RegularPackage>().ToArray<RegularPackage>());
			ContentPackageManager.CheckMissingDependencies();
			this.PopulateInstalledModLists(true, true);
			ContentPackageManager.LogEnabledRegularPackageErrors();
			this.enabledCoreDropdown.ButtonTextColor = (this.EnabledCorePackage.HasAnyErrors ? GUIStyle.Red : GUIStyle.TextColorNormal);
		}

		// Token: 0x060047D9 RID: 18393 RVA: 0x00277954 File Offset: 0x00275B54
		private void CreateLocalThumbnail(string path, GUIFrame thumbnailContainer)
		{
			thumbnailContainer.ClearChildren();
			MutableWorkshopMenu.LocalThumbnail localThumbnail = this.localThumbnail;
			if (localThumbnail != null)
			{
				localThumbnail.Dispose();
			}
			this.localThumbnail = new MutableWorkshopMenu.LocalThumbnail(path);
			this.CreateAsyncThumbnailComponent(thumbnailContainer, delegate
			{
				MutableWorkshopMenu.LocalThumbnail localThumbnail2 = this.localThumbnail;
				if (localThumbnail2 == null)
				{
					return null;
				}
				return localThumbnail2.Texture;
			}, delegate
			{
				MutableWorkshopMenu.LocalThumbnail localThumbnail2 = this.localThumbnail;
				return localThumbnail2 != null && localThumbnail2.Loading;
			});
		}

		// Token: 0x060047DA RID: 18394 RVA: 0x002779A4 File Offset: 0x00275BA4
		[return: TupleElementNames(new string[]
		{
			"FileCount",
			"ByteCount"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		private static Task<ValueTuple<int, int>> GetModDirInfo(string dir, GUITextBlock label)
		{
			MutableWorkshopMenu.<GetModDirInfo>d__74 <GetModDirInfo>d__;
			<GetModDirInfo>d__.<>t__builder = AsyncTaskMethodBuilder<ValueTuple<int, int>>.Create();
			<GetModDirInfo>d__.dir = dir;
			<GetModDirInfo>d__.label = label;
			<GetModDirInfo>d__.<>1__state = -1;
			<GetModDirInfo>d__.<>t__builder.Start<MutableWorkshopMenu.<GetModDirInfo>d__74>(ref <GetModDirInfo>d__);
			return <GetModDirInfo>d__.<>t__builder.Task;
		}

		// Token: 0x060047DB RID: 18395 RVA: 0x002779F0 File Offset: 0x00275BF0
		private void DeselectPublishedItem()
		{
			GUIListBox selfModsList;
			if (this.selfModsListOption.TryUnwrap(out selfModsList))
			{
				GUIComponent deselectCarrier = selfModsList.Parent.FindChild(delegate(GUIComponent c)
				{
					object userData2 = c.UserData;
					if (userData2 is WorkshopMenu.ActionCarrier)
					{
						Identifier id = ((WorkshopMenu.ActionCarrier)userData2).Id;
						return id == "deselect";
					}
					return false;
				}, false);
				object userData = deselectCarrier.UserData;
				Action action2;
				if (userData is WorkshopMenu.ActionCarrier)
				{
					Action action = ((WorkshopMenu.ActionCarrier)userData).Action;
					action2 = action;
				}
				else
				{
					action2 = null;
				}
				Action deselectAction = action2;
				if (deselectAction != null)
				{
					deselectAction();
				}
			}
			this.SelectTab(MutableWorkshopMenu.Tab.Publish);
		}

		// Token: 0x060047DC RID: 18396 RVA: 0x00277A70 File Offset: 0x00275C70
		private static bool PackageMatchesItem(ContentPackage p, Item workshopItem)
		{
			SteamWorkshopId workshopId;
			return p.TryExtractSteamWorkshopId(out workshopId) && workshopId.Value == workshopItem.Id;
		}

		// Token: 0x060047DD RID: 18397 RVA: 0x00277AA0 File Offset: 0x00275CA0
		private void PopulatePublishTab(Either<Item, ContentPackage> itemOrPackage, GUIFrame parentFrame)
		{
			MutableWorkshopMenu.<>c__DisplayClass77_0 CS$<>8__locals1 = new MutableWorkshopMenu.<>c__DisplayClass77_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.parentFrame = parentFrame;
			ContentPackageManager.LocalPackages.Refresh();
			ContentPackageManager.WorkshopPackages.Refresh();
			CS$<>8__locals1.parentFrame.ClearChildren();
			GUILayoutGroup mainLayout = new GUILayoutGroup(new RectTransform(Vector2.One, CS$<>8__locals1.parentFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
			Item item;
			CS$<>8__locals1.workshopItem = (itemOrPackage.TryGet(out item) ? item : default(Item));
			ContentPackage package;
			CS$<>8__locals1.localPackage = (itemOrPackage.TryGet(out package) ? package : ContentPackageManager.LocalPackages.FirstOrDefault((ContentPackage p) => MutableWorkshopMenu.PackageMatchesItem(p, CS$<>8__locals1.workshopItem)));
			ContentPackage workshopPackage = ContentPackageManager.WorkshopPackages.FirstOrDefault((ContentPackage p) => MutableWorkshopMenu.PackageMatchesItem(p, CS$<>8__locals1.workshopItem));
			if (CS$<>8__locals1.localPackage == null)
			{
				new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.15f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				bool workshopCopyExists = ContentPackageManager.WorkshopPackages.Any((ContentPackage p) => MutableWorkshopMenu.PackageMatchesItem(p, CS$<>8__locals1.workshopItem));
				new GUITextBlock(new RectTransform(new ValueTuple<float, float>(0.7f, 0.4f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(workshopCopyExists ? "LocalCopyRequired" : "ItemInstallRequired"), null, null, Alignment.Left, true, "", null);
				GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.6f, 0.1f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				new GUIButton(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Yes"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
				{
					MutableWorkshopMenu <>4__this = CS$<>8__locals1.<>4__this;
					Func<GUITextBlock, GUIMessageBox, IEnumerable<CoroutineStatus>> subcoroutine;
					if ((subcoroutine = CS$<>8__locals1.<>9__4) == null)
					{
						subcoroutine = (CS$<>8__locals1.<>9__4 = ((GUITextBlock currentStepText, GUIMessageBox messageBox) => CS$<>8__locals1.<>4__this.CreateLocalCopy(currentStepText, CS$<>8__locals1.workshopItem, CS$<>8__locals1.parentFrame)));
					}
					IEnumerable<CoroutineStatus> func = <>4__this.MessageBoxCoroutine(subcoroutine);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(16, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("CreateLocalCopy ");
					defaultInterpolatedStringHandler3.AppendFormatted<PublishedFileId>(CS$<>8__locals1.workshopItem.Id);
					CoroutineManager.StartCoroutine(func, defaultInterpolatedStringHandler3.ToStringAndClear());
					return false;
				};
				new GUIButton(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("No"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
				{
					CS$<>8__locals1.<>4__this.DeselectPublishedItem();
					return false;
				};
				return;
			}
			MutableWorkshopMenu.<>c__DisplayClass77_1 CS$<>8__locals2 = new MutableWorkshopMenu.<>c__DisplayClass77_1();
			CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
			if (!ContentPackageManager.LocalPackages.Contains(CS$<>8__locals2.CS$<>8__locals1.localPackage))
			{
				throw new Exception("Content package \"" + CS$<>8__locals2.CS$<>8__locals1.localPackage.Name + "\" is not a local package!");
			}
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.05f), mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = CS$<>8__locals2.CS$<>8__locals1.localPackage.Name;
			GUIFont largeFont = GUIStyle.LargeFont;
			GUITextBlock selectedTitle = new GUITextBlock(rectT, text2, null, largeFont, Alignment.Left, false, "", null);
			if (CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id != 0UL)
			{
				GUIButton showInSteamButton = this.CreateShowInSteamButton(CS$<>8__locals2.CS$<>8__locals1.workshopItem, new RectTransform(new ValueTuple<float, float>(0.2f, 1f), selectedTitle.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal));
			}
			WorkshopMenu.Spacer(mainLayout, 0.03f);
			ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup> valueTuple = MutableWorkshopMenu.CreateSidebars(mainLayout, 0.2f, 0.01f, 0.79f, false, 0.4f);
			GUILayoutGroup leftTop = valueTuple.Item1;
			GUILayoutGroup rightTop = valueTuple.Item3;
			leftTop.Stretch = true;
			rightTop.Stretch = true;
			WorkshopMenu.Label(leftTop, TextManager.Get("WorkshopItemPreviewImage"), GUIStyle.SubHeadingFont, 1f);
			CS$<>8__locals2.thumbnailPath = null;
			CS$<>8__locals2.thumbnailContainer = this.CreateThumbnailContainer(leftTop, Vector2.One, ScaleBasis.BothWidth);
			if (CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id != 0UL)
			{
				this.CreateItemThumbnail(CS$<>8__locals2.CS$<>8__locals1.workshopItem, this.taskCancelSrc.Token, CS$<>8__locals2.thumbnailContainer);
			}
			new GUIButton(WorkshopMenu.NewItemRectT(leftTop, 1f), TextManager.Get("WorkshopItemBrowse"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object o)
			{
				FileSelection.ClearFileTypeFilters();
				FileSelection.AddFileTypeFilter("PNG", "*.png");
				FileSelection.AddFileTypeFilter("JPEG", "*.jpg, *.jpeg");
				FileSelection.AddFileTypeFilter("All files", "*.*");
				FileSelection.SelectFileTypeFilter("*.png");
				FileSelection.CurrentDirectory = Path.GetFullPath(Path.GetDirectoryName(CS$<>8__locals2.CS$<>8__locals1.localPackage.Path));
				Action<string> onFileSelected;
				if ((onFileSelected = CS$<>8__locals2.<>9__14) == null)
				{
					onFileSelected = (CS$<>8__locals2.<>9__14 = delegate(string fn)
					{
						if (new FileInfo(fn).Length > 1048576L)
						{
							new GUIMessageBox(TextManager.Get("Error"), TextManager.Get("WorkshopItemPreviewImageTooLarge"), null, null, GUIMessageBox.Type.Default);
							return;
						}
						CS$<>8__locals2.thumbnailPath = fn;
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.CreateLocalThumbnail(CS$<>8__locals2.thumbnailPath, CS$<>8__locals2.thumbnailContainer);
					});
				}
				FileSelection.OnFileSelected = onFileSelected;
				FileSelection.Open = true;
				return false;
			};
			WorkshopMenu.Label(rightTop, TextManager.Get("WorkshopItemTitle"), GUIStyle.SubHeadingFont, 1f);
			CS$<>8__locals2.titleTextBox = new GUITextBox(WorkshopMenu.NewItemRectT(rightTop, 1f), CS$<>8__locals2.CS$<>8__locals1.localPackage.Name, null, null, Alignment.Left, false, "", null, false, true);
			WorkshopMenu.Label(rightTop, TextManager.Get("WorkshopItemDescription"), GUIStyle.SubHeadingFont, 1f);
			CS$<>8__locals2.descriptionTextBox = WorkshopMenu.ScrollableTextBox(rightTop, 6f, CS$<>8__locals2.CS$<>8__locals1.workshopItem.Description ?? string.Empty);
			if (CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id != 0UL)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("GetFullDescription");
				defaultInterpolatedStringHandler.AppendFormatted<PublishedFileId>(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id);
				TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), SteamManager.Workshop.GetItemAsap(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id.Value, true), delegate(Task t)
				{
					Option<Item> itemWithDescriptionOption;
					if (!t.TryGetResult(out itemWithDescriptionOption))
					{
						return;
					}
					Item itemWithDescription;
					CS$<>8__locals2.descriptionTextBox.Text = (itemWithDescriptionOption.TryUnwrap(out itemWithDescription) ? (itemWithDescription.Description ?? CS$<>8__locals2.descriptionTextBox.Text) : CS$<>8__locals2.descriptionTextBox.Text);
					CS$<>8__locals2.descriptionTextBox.Deselect();
				});
			}
			ValueTuple<GUILayoutGroup, GUIFrame, GUILayoutGroup> valueTuple2 = MutableWorkshopMenu.CreateSidebars(mainLayout, 0.49f, 0.01f, 0.5f, false, 0.5f);
			GUILayoutGroup leftBottom = valueTuple2.Item1;
			GUILayoutGroup rightBottom = valueTuple2.Item3;
			leftBottom.Stretch = true;
			rightBottom.Stretch = true;
			WorkshopMenu.Label(leftBottom, TextManager.Get("WorkshopItemVersion"), GUIStyle.SubHeadingFont, 1f);
			string modVersion = CS$<>8__locals2.CS$<>8__locals1.localPackage.ModVersion;
			if (workshopPackage != null)
			{
				string workshopVersion = workshopPackage.ModVersion;
				if (workshopVersion != null && modVersion.Equals(workshopVersion, StringComparison.OrdinalIgnoreCase))
				{
					modVersion = ModProject.IncrementModVersion(modVersion);
				}
			}
			CS$<>8__locals2.forbiddenVersionCharacters = new char[]
			{
				';',
				'='
			};
			CS$<>8__locals2.versionTextBox = new GUITextBox(WorkshopMenu.NewItemRectT(leftBottom, 1f), modVersion, null, null, Alignment.Left, false, "", null, false, true);
			CS$<>8__locals2.versionTextBox.OnTextChanged += delegate(GUITextBox box, string text)
			{
				IEnumerable<char> source = text;
				Func<char, bool> predicate;
				if ((predicate = CS$<>8__locals2.<>9__15) == null)
				{
					predicate = (CS$<>8__locals2.<>9__15 = ((char c) => CS$<>8__locals2.forbiddenVersionCharacters.Contains(c)));
				}
				if (source.Any(predicate))
				{
					foreach (char c2 in CS$<>8__locals2.forbiddenVersionCharacters)
					{
						string text3 = text;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(0, 1);
						defaultInterpolatedStringHandler3.AppendFormatted<char>(c2);
						text = text3.Replace(defaultInterpolatedStringHandler3.ToStringAndClear(), "");
					}
					box.Text = text;
					box.Flash(new Microsoft.Xna.Framework.Color?(GUIStyle.Red), 1.5f, false, false, null);
				}
				return true;
			};
			WorkshopMenu.Label(leftBottom, TextManager.Get("WorkshopItemChangeNote"), GUIStyle.SubHeadingFont, 1f);
			CS$<>8__locals2.changeNoteTextBox = WorkshopMenu.ScrollableTextBox(leftBottom, 5f, "");
			WorkshopMenu.Label(rightBottom, TextManager.Get("WorkshopItemTags"), GUIStyle.SubHeadingFont, 1f);
			GUIListBox tagsList = this.CreateTagsList(SteamManager.Workshop.Tags, WorkshopMenu.NewItemRectT(rightBottom, 4f), true);
			CS$<>8__locals2.tagButtons = (from GUIButton b in tagsList.Content.Children
			select new ValueTuple<Identifier, GUIButton>((Identifier)b.UserData, b)).ToDictionary<Identifier, GUIButton>();
			if (CS$<>8__locals2.CS$<>8__locals1.workshopItem.Tags != null)
			{
				foreach (Identifier tag in CS$<>8__locals2.CS$<>8__locals1.workshopItem.Tags.ToIdentifiers())
				{
					GUIButton button2;
					if (CS$<>8__locals2.tagButtons.TryGetValue(tag, out button2))
					{
						button2.Selected = true;
					}
				}
			}
			GUILayoutGroup visibilityLayout = new GUILayoutGroup(WorkshopMenu.NewItemRectT(rightBottom, 1f), true, Anchor.TopLeft);
			GUITextBlock visibilityLabel = WorkshopMenu.Label(visibilityLayout, TextManager.Get("WorkshopItemVisibility"), GUIStyle.SubHeadingFont, 1f);
			visibilityLabel.RectTransform.RelativeSize = new ValueTuple<float, float>(0.6f, 1f);
			visibilityLabel.TextAlignment = Alignment.CenterRight;
			CS$<>8__locals2.visibility = CS$<>8__locals2.CS$<>8__locals1.workshopItem.Visibility;
			GUIDropDown visibilityDropdown = WorkshopMenu.DropdownEnum<Visibility>(visibilityLayout, delegate(Visibility v)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(23, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("WorkshopItemVisibility.");
				defaultInterpolatedStringHandler3.AppendFormatted<Visibility>(v);
				return TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			}, CS$<>8__locals2.visibility, delegate(Visibility v)
			{
				CS$<>8__locals2.visibility = v;
			});
			visibilityDropdown.RectTransform.RelativeSize = new ValueTuple<float, float>(0.4f, 1f);
			GUITextBlock fileInfoLabel = WorkshopMenu.Label(rightBottom, "", GUIStyle.Font, 1f);
			fileInfoLabel.TextAlignment = Alignment.CenterRight;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("FileInfoLabel");
			defaultInterpolatedStringHandler2.AppendFormatted<PublishedFileId>(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id);
			TaskPool.AddWithResult<ValueTuple<int, int>>(defaultInterpolatedStringHandler2.ToStringAndClear(), MutableWorkshopMenu.GetModDirInfo(CS$<>8__locals2.CS$<>8__locals1.localPackage.Dir, fileInfoLabel), delegate([TupleElementNames(new string[]
			{
				"FileCount",
				"ByteCount"
			})] ValueTuple<int, int> t)
			{
			});
			CS$<>8__locals2.buttonLayout = new GUILayoutGroup(WorkshopMenu.NewItemRectT(rightBottom, 1f), true, Anchor.CenterRight);
			new GUIButton(CS$<>8__locals2.<PopulatePublishTab>g__newButtonRectT|12(), TextManager.Get((CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id != 0UL) ? "WorkshopItemUpdate" : "WorkshopItemPublish"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
			{
				MutableWorkshopMenu.<>c__DisplayClass77_2 CS$<>8__locals3 = new MutableWorkshopMenu.<>c__DisplayClass77_2();
				CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals2;
				string packageName = CS$<>8__locals2.CS$<>8__locals1.localPackage.Name;
				Result<ContentPackage, Exception> result = ContentPackageManager.ReloadContentPackage(CS$<>8__locals2.CS$<>8__locals1.localPackage);
				if (!result.TryUnwrapSuccess(out CS$<>8__locals2.CS$<>8__locals1.localPackage))
				{
					Exception exception;
					throw new Exception("\"" + packageName + "\" was removed upon reload", result.TryUnwrapFailure(out exception) ? exception : null);
				}
				CS$<>8__locals3.ugcEditor = ((CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id == 0UL) ? Editor.NewCommunityFile : new Editor(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id));
				MutableWorkshopMenu.<>c__DisplayClass77_2 CS$<>8__locals4 = CS$<>8__locals3;
				Editor editor = CS$<>8__locals3.ugcEditor.InLanguage(SteamUtils.SteamUILanguage ?? string.Empty);
				editor = editor.WithTitle(CS$<>8__locals2.titleTextBox.Text);
				editor = editor.WithDescription(CS$<>8__locals2.descriptionTextBox.Text);
				editor = editor.WithTags(from kvp in CS$<>8__locals2.tagButtons
				where kvp.Value.Selected
				select kvp.Key.Value);
				editor = editor.WithChangeLog(CS$<>8__locals2.changeNoteTextBox.Text);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(24, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("gameversion=");
				defaultInterpolatedStringHandler3.AppendFormatted<Version>(CS$<>8__locals2.CS$<>8__locals1.localPackage.GameVersion);
				defaultInterpolatedStringHandler3.AppendLiteral(";modversion=");
				defaultInterpolatedStringHandler3.AppendFormatted(CS$<>8__locals2.versionTextBox.Text);
				editor = editor.WithMetaData(defaultInterpolatedStringHandler3.ToStringAndClear());
				CS$<>8__locals4.ugcEditor = editor.WithPreviewFile(CS$<>8__locals2.thumbnailPath);
				CS$<>8__locals3.ugcEditor.Visibility = new Visibility?(CS$<>8__locals2.visibility);
				CoroutineManager.StartCoroutine(CS$<>8__locals2.CS$<>8__locals1.<>4__this.MessageBoxCoroutine((GUITextBlock currentStepText, GUIMessageBox messageBox) => CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.PublishItem(currentStepText, messageBox, CS$<>8__locals3.CS$<>8__locals2.versionTextBox.Text, CS$<>8__locals3.ugcEditor, CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.localPackage)), "");
				return false;
			};
			if (CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id != 0UL)
			{
				GUIButton deleteItemButton = new GUIButton(CS$<>8__locals2.<PopulatePublishTab>g__newButtonRectT|12(), TextManager.Get("WorkshopItemDelete"), Alignment.Center, "", new Microsoft.Xna.Framework.Color?(GUIStyle.Red))
				{
					OnClicked = delegate(GUIButton button, object o)
					{
						GUIMessageBox confirmDeletion = new GUIMessageBox(TextManager.Get("WorkshopItemDelete"), TextManager.GetWithVariable("WorkshopItemDeleteVerification", "[itemname]", CS$<>8__locals2.CS$<>8__locals1.workshopItem.Title, FormatCapitals.No), new LocalizedString[]
						{
							TextManager.Get("Yes"),
							TextManager.Get("No")
						}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
						Action<bool> <>9__23;
						confirmDeletion.Buttons[0].OnClicked = delegate(GUIButton yesBuffer, object o1)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(6, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("Delete");
							defaultInterpolatedStringHandler3.AppendFormatted<PublishedFileId>(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id);
							string name = defaultInterpolatedStringHandler3.ToStringAndClear();
							Task<bool> task = SteamUGC.DeleteFileAsync(CS$<>8__locals2.CS$<>8__locals1.workshopItem.Id);
							Action<bool> onCompletion;
							if ((onCompletion = <>9__23) == null)
							{
								onCompletion = (<>9__23 = delegate(bool t)
								{
									SteamManager.Workshop.Uninstall(CS$<>8__locals2.CS$<>8__locals1.workshopItem);
									confirmDeletion.Close();
									CS$<>8__locals2.CS$<>8__locals1.<>4__this.DeselectPublishedItem();
								});
							}
							TaskPool.AddWithResult<bool>(name, task, onCompletion);
							return false;
						};
						confirmDeletion.Buttons[1].OnClicked = delegate(GUIButton noButton, object o1)
						{
							confirmDeletion.Close();
							return false;
						};
						return false;
					},
					HoverColor = Microsoft.Xna.Framework.Color.Lerp(GUIStyle.Red, Microsoft.Xna.Framework.Color.White, 0.3f),
					PressedColor = Microsoft.Xna.Framework.Color.Lerp(GUIStyle.Red, Microsoft.Xna.Framework.Color.Black, 0.3f)
				};
				deleteItemButton.TextBlock.TextColor = Microsoft.Xna.Framework.Color.Black;
				deleteItemButton.TextBlock.HoverTextColor = Microsoft.Xna.Framework.Color.Black;
			}
		}

		// Token: 0x060047DE RID: 18398 RVA: 0x002785C1 File Offset: 0x002767C1
		private IEnumerable<CoroutineStatus> MessageBoxCoroutine(Func<GUITextBlock, GUIMessageBox, IEnumerable<CoroutineStatus>> subcoroutine)
		{
			MutableWorkshopMenu.<MessageBoxCoroutine>d__78 <MessageBoxCoroutine>d__ = new MutableWorkshopMenu.<MessageBoxCoroutine>d__78(-2);
			<MessageBoxCoroutine>d__.<>3__subcoroutine = subcoroutine;
			return <MessageBoxCoroutine>d__;
		}

		// Token: 0x060047DF RID: 18399 RVA: 0x002785D1 File Offset: 0x002767D1
		private IEnumerable<CoroutineStatus> CreateLocalCopy(GUITextBlock currentStepText, Item workshopItem, GUIFrame parentFrame)
		{
			MutableWorkshopMenu.<CreateLocalCopy>d__79 <CreateLocalCopy>d__ = new MutableWorkshopMenu.<CreateLocalCopy>d__79(-2);
			<CreateLocalCopy>d__.<>4__this = this;
			<CreateLocalCopy>d__.<>3__currentStepText = currentStepText;
			<CreateLocalCopy>d__.<>3__workshopItem = workshopItem;
			<CreateLocalCopy>d__.<>3__parentFrame = parentFrame;
			return <CreateLocalCopy>d__;
		}

		// Token: 0x060047E0 RID: 18400 RVA: 0x002785F6 File Offset: 0x002767F6
		private IEnumerable<CoroutineStatus> PublishItem(GUITextBlock currentStepText, GUIMessageBox messageBox, string modVersion, Editor editor, ContentPackage localPackage)
		{
			MutableWorkshopMenu.<PublishItem>d__80 <PublishItem>d__ = new MutableWorkshopMenu.<PublishItem>d__80(-2);
			<PublishItem>d__.<>4__this = this;
			<PublishItem>d__.<>3__currentStepText = currentStepText;
			<PublishItem>d__.<>3__messageBox = messageBox;
			<PublishItem>d__.<>3__modVersion = modVersion;
			<PublishItem>d__.<>3__editor = editor;
			<PublishItem>d__.<>3__localPackage = localPackage;
			return <PublishItem>d__;
		}

		// Token: 0x060047E2 RID: 18402 RVA: 0x00278664 File Offset: 0x00276864
		[CompilerGenerated]
		internal static Identifier? <PopulateInstalledModLists>g__GetButtonIconStyle|26_20(GUIComponent c)
		{
			GUILayoutGroup child = c.GetChild<GUILayoutGroup>();
			if (child == null)
			{
				return null;
			}
			GUIButton guibutton = child.GetAllChildren<GUIButton>().Last<GUIButton>();
			if (guibutton == null)
			{
				return null;
			}
			GUIComponentStyle style = guibutton.Style;
			if (style == null)
			{
				return null;
			}
			return new Identifier?(style.Identifier);
		}

		// Token: 0x060047E3 RID: 18403 RVA: 0x002786BA File Offset: 0x002768BA
		[CompilerGenerated]
		internal static void <PopulateInstalledModLists>g__NoOp|26_24()
		{
		}

		// Token: 0x060047E4 RID: 18404 RVA: 0x002786BC File Offset: 0x002768BC
		[CompilerGenerated]
		internal static DateTime <AddUnpublishedMods>g__getEditTime|34_1(ContentPackage p)
		{
			DateTime writeTime = File.GetLastWriteTime(p.Dir);
			string[] files = Directory.GetFiles(p.Dir, "*", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				DateTime newTime = File.GetLastWriteTime(file);
				if (newTime > writeTime)
				{
					writeTime = newTime;
				}
			}
			return writeTime;
		}

		// Token: 0x060047E5 RID: 18405 RVA: 0x00278714 File Offset: 0x00276914
		[CompilerGenerated]
		internal static void <CreateSubscribeButton>g__drawSection|36_4(float amount, Microsoft.Xna.Framework.Color color, float thickness, ref MutableWorkshopMenu.<>c__DisplayClass36_1 A_3)
		{
			GUI.DrawDonutSection(A_3.spriteBatch, A_3.component.Rect.Center.ToVector2() + new ValueTuple<float, float>(0f, 1f), new Range<float>((float)A_3.component.Rect.Width * 0.55f - thickness * 0.5f, (float)A_3.component.Rect.Width * 0.55f + thickness * 0.5f), amount * 3.1415927f * 2f, color, 0f, 0f);
		}

		// Token: 0x060047E6 RID: 18406 RVA: 0x002787BB File Offset: 0x002769BB
		[CompilerGenerated]
		internal static void <CreateSubscribeButton>g__drawSectionFuzzy|36_5(float amount, Microsoft.Xna.Framework.Color color, float thickness, ref MutableWorkshopMenu.<>c__DisplayClass36_1 A_3)
		{
			MutableWorkshopMenu.<CreateSubscribeButton>g__drawSection|36_4(amount, color, thickness, ref A_3);
			MutableWorkshopMenu.<CreateSubscribeButton>g__drawSection|36_4(amount, color * 0.6f, thickness + 0.5f, ref A_3);
			MutableWorkshopMenu.<CreateSubscribeButton>g__drawSection|36_4(amount, color * 0.3f, thickness + 1f, ref A_3);
		}

		// Token: 0x060047E7 RID: 18407 RVA: 0x002787F8 File Offset: 0x002769F8
		[CompilerGenerated]
		[return: TupleElementNames(new string[]
		{
			"Path",
			"Doc"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			2
		})]
		internal static ValueTuple<string, XDocument> <OpenLoadPreset>g__tryLoadXml|44_0(string path)
		{
			return new ValueTuple<string, XDocument>(path, XMLExtensions.TryLoadXml(path));
		}

		// Token: 0x04002537 RID: 9527
		private readonly GUIDropDown enabledCoreDropdown;

		// Token: 0x04002538 RID: 9528
		private readonly GUIListBox enabledRegularModsList;

		// Token: 0x04002539 RID: 9529
		private readonly GUIListBox disabledRegularModsList;

		// Token: 0x0400253A RID: 9530
		private readonly Action<Either<Item, ContentPackage>> onInstalledInfoButtonHit;

		// Token: 0x0400253B RID: 9531
		private readonly GUITextBox modsListFilter;

		// Token: 0x0400253C RID: 9532
		private readonly Dictionary<MutableWorkshopMenu.Filter, GUITickBox> modsListFilterTickboxes;

		// Token: 0x0400253D RID: 9533
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<GUIButton> bulkUpdateButtonOption;

		// Token: 0x0400253E RID: 9534
		[Nullable(2)]
		private GUIComponent draggedElement;

		// Token: 0x0400253F RID: 9535
		[Nullable(2)]
		private GUIListBox draggedElementOrigin;

		// Token: 0x04002540 RID: 9536
		[Nullable(2)]
		private Action currentSwapFunc;

		// Token: 0x04002541 RID: 9537
		private GUISoundType? swapSoundType;

		// Token: 0x04002543 RID: 9539
		private readonly GUILayoutGroup tabber;

		// Token: 0x04002544 RID: 9540
		[TupleElementNames(new string[]
		{
			"Button",
			"Content"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})]
		private readonly Dictionary<MutableWorkshopMenu.Tab, ValueTuple<GUIButton, GUIFrame>> tabContents;

		// Token: 0x04002545 RID: 9541
		private readonly GUIFrame contentFrame;

		// Token: 0x04002546 RID: 9542
		private CancellationTokenSource taskCancelSrc = new CancellationTokenSource();

		// Token: 0x04002547 RID: 9543
		private readonly HashSet<SteamManager.Workshop.ItemThumbnail> itemThumbnails = new HashSet<SteamManager.Workshop.ItemThumbnail>();

		// Token: 0x04002548 RID: 9544
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<GUIListBox> popularModsListOption;

		// Token: 0x04002549 RID: 9545
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<GUIListBox> selfModsListOption;

		// Token: 0x0400254A RID: 9546
		private uint memSubscribedModCount;

		// Token: 0x0400254B RID: 9547
		[Nullable(2)]
		private MutableWorkshopMenu.LocalThumbnail localThumbnail;

		// Token: 0x02001109 RID: 4361
		[NullableContext(0)]
		public enum Tab
		{
			// Token: 0x04005A5D RID: 23133
			InstalledMods,
			// Token: 0x04005A5E RID: 23134
			PopularMods,
			// Token: 0x04005A5F RID: 23135
			Publish
		}

		// Token: 0x0200110A RID: 4362
		[NullableContext(0)]
		private enum Filter
		{
			// Token: 0x04005A61 RID: 23137
			ShowLocal,
			// Token: 0x04005A62 RID: 23138
			ShowWorkshop,
			// Token: 0x04005A63 RID: 23139
			ShowPublished,
			// Token: 0x04005A64 RID: 23140
			ShowOnlySubs,
			// Token: 0x04005A65 RID: 23141
			ShowOnlyItemAssemblies
		}

		// Token: 0x0200110B RID: 4363
		[NullableContext(2)]
		[Nullable(0)]
		private class LocalThumbnail : IDisposable
		{
			// Token: 0x17001C93 RID: 7315
			// (get) Token: 0x06008EA6 RID: 36518 RVA: 0x003B58E7 File Offset: 0x003B3AE7
			// (set) Token: 0x06008EA7 RID: 36519 RVA: 0x003B58EF File Offset: 0x003B3AEF
			public Texture2D Texture { get; private set; }

			// Token: 0x06008EA8 RID: 36520 RVA: 0x003B58F8 File Offset: 0x003B3AF8
			[NullableContext(1)]
			public LocalThumbnail(string path)
			{
				MutableWorkshopMenu.LocalThumbnail.<>c__DisplayClass5_0 CS$<>8__locals1 = new MutableWorkshopMenu.LocalThumbnail.<>c__DisplayClass5_0();
				CS$<>8__locals1.path = path;
				base..ctor();
				CS$<>8__locals1.<>4__this = this;
				TaskPool.Add("LocalThumbnail " + CS$<>8__locals1.path, Task.Run<Texture2D>(delegate()
				{
					MutableWorkshopMenu.LocalThumbnail.<>c__DisplayClass5_0.<<-ctor>b__0>d <<-ctor>b__0>d;
					<<-ctor>b__0>d.<>t__builder = AsyncTaskMethodBuilder<Texture2D>.Create();
					<<-ctor>b__0>d.<>4__this = CS$<>8__locals1;
					<<-ctor>b__0>d.<>1__state = -1;
					<<-ctor>b__0>d.<>t__builder.Start<MutableWorkshopMenu.LocalThumbnail.<>c__DisplayClass5_0.<<-ctor>b__0>d>(ref <<-ctor>b__0>d);
					return <<-ctor>b__0>d.<>t__builder.Task;
				}), delegate(Task t)
				{
					CS$<>8__locals1.<>4__this.Loading = false;
					Task<Texture2D> texTask = t as Task<Texture2D>;
					if (!CS$<>8__locals1.<>4__this.disposed)
					{
						CS$<>8__locals1.<>4__this.Texture = texTask.Result;
						return;
					}
					Texture2D result = texTask.Result;
					if (result == null)
					{
						return;
					}
					result.Dispose();
				});
			}

			// Token: 0x06008EA9 RID: 36521 RVA: 0x003B5959 File Offset: 0x003B3B59
			public void Dispose()
			{
				if (this.disposed)
				{
					return;
				}
				this.disposed = true;
				Texture2D texture = this.Texture;
				if (texture == null)
				{
					return;
				}
				texture.Dispose();
			}

			// Token: 0x04005A67 RID: 23143
			public bool Loading = true;

			// Token: 0x04005A68 RID: 23144
			private bool disposed;
		}

		// Token: 0x0200110C RID: 4364
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005A69 RID: 23145
			[Nullable(0)]
			public static Action <0>__NoOp;

			// Token: 0x04005A6A RID: 23146
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <1>__IgnoredCallback;
		}
	}
}
