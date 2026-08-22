using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using EventInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200008B RID: 139
	public class GUIListBox : GUIComponent, IKeyboardSubscriber
	{
		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x060012EE RID: 4846 RVA: 0x000B84AC File Offset: 0x000B66AC
		// (set) Token: 0x060012EF RID: 4847 RVA: 0x000B84B4 File Offset: 0x000B66B4
		public GUIFrame ContentBackground { get; private set; }

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x060012F0 RID: 4848 RVA: 0x000B84BD File Offset: 0x000B66BD
		// (set) Token: 0x060012F1 RID: 4849 RVA: 0x000B84C5 File Offset: 0x000B66C5
		public GUIFrame Content { get; private set; }

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x060012F2 RID: 4850 RVA: 0x000B84CE File Offset: 0x000B66CE
		// (set) Token: 0x060012F3 RID: 4851 RVA: 0x000B84D6 File Offset: 0x000B66D6
		public GUIScrollBar ScrollBar { get; private set; }

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x060012F4 RID: 4852 RVA: 0x000B84E0 File Offset: 0x000B66E0
		private int ScrollBarSize
		{
			get
			{
				float desiredSize = 25f;
				float scaledSize = desiredSize * GUI.Scale;
				return (int)Math.Min((desiredSize + scaledSize) / 2f, (float)(this.Rect.Height / 3));
			}
		}

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x060012F5 RID: 4853 RVA: 0x000B8518 File Offset: 0x000B6718
		// (set) Token: 0x060012F6 RID: 4854 RVA: 0x000B8523 File Offset: 0x000B6723
		public bool SelectMultiple
		{
			get
			{
				return this.CurrentSelectMode > GUIListBox.SelectMode.SelectSingle;
			}
			set
			{
				this.CurrentSelectMode = (value ? GUIListBox.SelectMode.SelectMultiple : GUIListBox.SelectMode.SelectSingle);
			}
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x060012F7 RID: 4855 RVA: 0x000B8532 File Offset: 0x000B6732
		// (set) Token: 0x060012F8 RID: 4856 RVA: 0x000B853A File Offset: 0x000B673A
		public bool AllowMouseWheelScroll { get; set; } = true;

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x060012F9 RID: 4857 RVA: 0x000B8543 File Offset: 0x000B6743
		// (set) Token: 0x060012FA RID: 4858 RVA: 0x000B854B File Offset: 0x000B674B
		public bool AllowArrowKeyScroll { get; set; } = true;

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x060012FB RID: 4859 RVA: 0x000B8554 File Offset: 0x000B6754
		// (set) Token: 0x060012FC RID: 4860 RVA: 0x000B855C File Offset: 0x000B675C
		public bool SmoothScroll { get; set; }

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x060012FD RID: 4861 RVA: 0x000B8565 File Offset: 0x000B6765
		// (set) Token: 0x060012FE RID: 4862 RVA: 0x000B856D File Offset: 0x000B676D
		public bool ClampScrollToElements { get; set; }

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x060012FF RID: 4863 RVA: 0x000B8576 File Offset: 0x000B6776
		// (set) Token: 0x06001300 RID: 4864 RVA: 0x000B857E File Offset: 0x000B677E
		public bool FadeElements { get; set; }

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x06001301 RID: 4865 RVA: 0x000B8587 File Offset: 0x000B6787
		// (set) Token: 0x06001302 RID: 4866 RVA: 0x000B858F File Offset: 0x000B678F
		public bool PadBottom { get; set; }

		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x06001303 RID: 4867 RVA: 0x000B8598 File Offset: 0x000B6798
		// (set) Token: 0x06001304 RID: 4868 RVA: 0x000B85A0 File Offset: 0x000B67A0
		public bool SelectTop { get; set; }

		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06001305 RID: 4869 RVA: 0x000B85A9 File Offset: 0x000B67A9
		// (set) Token: 0x06001306 RID: 4870 RVA: 0x000B85B1 File Offset: 0x000B67B1
		public bool UseGridLayout
		{
			get
			{
				return this.useGridLayout;
			}
			set
			{
				if (this.useGridLayout == value)
				{
					return;
				}
				this.useGridLayout = value;
				this.childrenNeedsRecalculation = true;
				this.scrollBarNeedsRecalculation = true;
			}
		}

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06001307 RID: 4871 RVA: 0x000B85D2 File Offset: 0x000B67D2
		// (set) Token: 0x06001308 RID: 4872 RVA: 0x000B8606 File Offset: 0x000B6806
		public Vector4 Padding
		{
			get
			{
				if (this.overridePadding != null)
				{
					return this.overridePadding.Value;
				}
				if (base.Style == null)
				{
					return Vector4.Zero;
				}
				return base.Style.Padding;
			}
			set
			{
				this.dimensionsNeedsRecalculation = true;
				this.overridePadding = new Vector4?(value);
			}
		}

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06001309 RID: 4873 RVA: 0x000B861B File Offset: 0x000B681B
		public GUIComponent SelectedComponent
		{
			get
			{
				return this.selected.FirstOrDefault<GUIComponent>();
			}
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x0600130A RID: 4874 RVA: 0x000B8628 File Offset: 0x000B6828
		// (set) Token: 0x0600130B RID: 4875 RVA: 0x000B8630 File Offset: 0x000B6830
		public override bool Selected
		{
			get
			{
				return this.isSelected;
			}
			set
			{
				this.isSelected = value;
			}
		}

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x0600130C RID: 4876 RVA: 0x000B8639 File Offset: 0x000B6839
		public IReadOnlyList<GUIComponent> AllSelected
		{
			get
			{
				return this.selected;
			}
		}

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x0600130D RID: 4877 RVA: 0x000B8641 File Offset: 0x000B6841
		public object SelectedData
		{
			get
			{
				GUIComponent selectedComponent = this.SelectedComponent;
				if (selectedComponent == null)
				{
					return null;
				}
				return selectedComponent.UserData;
			}
		}

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x0600130E RID: 4878 RVA: 0x000B8654 File Offset: 0x000B6854
		public int SelectedIndex
		{
			get
			{
				if (this.SelectedComponent == null)
				{
					return -1;
				}
				return this.Content.RectTransform.GetChildIndex(this.SelectedComponent.RectTransform);
			}
		}

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x0600130F RID: 4879 RVA: 0x000B867B File Offset: 0x000B687B
		// (set) Token: 0x06001310 RID: 4880 RVA: 0x000B8688 File Offset: 0x000B6888
		public float BarScroll
		{
			get
			{
				return this.ScrollBar.BarScroll;
			}
			set
			{
				this.ScrollBar.BarScroll = value;
			}
		}

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06001311 RID: 4881 RVA: 0x000B8696 File Offset: 0x000B6896
		public float BarSize
		{
			get
			{
				return this.ScrollBar.BarSize;
			}
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001312 RID: 4882 RVA: 0x000B86A3 File Offset: 0x000B68A3
		public float TotalSize
		{
			get
			{
				return (float)this.totalSize;
			}
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06001313 RID: 4883 RVA: 0x000B86AC File Offset: 0x000B68AC
		// (set) Token: 0x06001314 RID: 4884 RVA: 0x000B86B4 File Offset: 0x000B68B4
		public int Spacing { get; set; }

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x06001315 RID: 4885 RVA: 0x000B86BD File Offset: 0x000B68BD
		// (set) Token: 0x06001316 RID: 4886 RVA: 0x000B86C5 File Offset: 0x000B68C5
		public override Color Color
		{
			get
			{
				return base.Color;
			}
			set
			{
				base.Color = value;
				this.Content.Color = value;
			}
		}

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x06001317 RID: 4887 RVA: 0x000B86DA File Offset: 0x000B68DA
		// (set) Token: 0x06001318 RID: 4888 RVA: 0x000B86E2 File Offset: 0x000B68E2
		public bool ScrollBarEnabled { get; set; } = true;

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x06001319 RID: 4889 RVA: 0x000B86EB File Offset: 0x000B68EB
		// (set) Token: 0x0600131A RID: 4890 RVA: 0x000B86F3 File Offset: 0x000B68F3
		public bool KeepSpaceForScrollBar { get; set; }

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x0600131B RID: 4891 RVA: 0x000B86FC File Offset: 0x000B68FC
		// (set) Token: 0x0600131C RID: 4892 RVA: 0x000B8704 File Offset: 0x000B6904
		public bool CanTakeKeyBoardFocus { get; set; } = true;

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x0600131D RID: 4893 RVA: 0x000B870D File Offset: 0x000B690D
		// (set) Token: 0x0600131E RID: 4894 RVA: 0x000B871A File Offset: 0x000B691A
		public bool ScrollBarVisible
		{
			get
			{
				return this.ScrollBar.Visible;
			}
			set
			{
				if (this.ScrollBar.Visible == value)
				{
					return;
				}
				this.ScrollBar.Visible = value;
				this.dimensionsNeedsRecalculation = true;
			}
		}

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x0600131F RID: 4895 RVA: 0x000B873E File Offset: 0x000B693E
		// (set) Token: 0x06001320 RID: 4896 RVA: 0x000B8746 File Offset: 0x000B6946
		public bool AutoHideScrollBar { get; set; } = true;

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06001321 RID: 4897 RVA: 0x000B874F File Offset: 0x000B694F
		// (set) Token: 0x06001322 RID: 4898 RVA: 0x000B8757 File Offset: 0x000B6957
		private bool IsScrollBarOnDefaultSide { get; set; }

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06001323 RID: 4899 RVA: 0x000B8760 File Offset: 0x000B6960
		// (set) Token: 0x06001324 RID: 4900 RVA: 0x000B8768 File Offset: 0x000B6968
		public GUIListBox.DragMode CurrentDragMode
		{
			get
			{
				return this.currentDragMode;
			}
			set
			{
				if (value == GUIListBox.DragMode.NoDragging && this.currentDragMode != GUIListBox.DragMode.NoDragging && this.isDraggingElement)
				{
					this.DraggedElement = null;
				}
				this.currentDragMode = value;
			}
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x06001325 RID: 4901 RVA: 0x000B878B File Offset: 0x000B698B
		private bool isDraggingElement
		{
			get
			{
				return this.draggedElement != null;
			}
		}

		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x06001326 RID: 4902 RVA: 0x000B8796 File Offset: 0x000B6996
		// (set) Token: 0x06001327 RID: 4903 RVA: 0x000B879E File Offset: 0x000B699E
		public bool HasDraggedElementIndexChanged { get; private set; }

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x06001328 RID: 4904 RVA: 0x000B87A7 File Offset: 0x000B69A7
		// (set) Token: 0x06001329 RID: 4905 RVA: 0x000B87B0 File Offset: 0x000B69B0
		public GUIComponent DraggedElement
		{
			get
			{
				return this.draggedElement;
			}
			set
			{
				if (value == this.draggedElement)
				{
					return;
				}
				this.draggedElement = value;
				this.HasDraggedElementIndexChanged = false;
				if (value == null)
				{
					return;
				}
				this.dragMousePosRelativeToTopLeftCorner = PlayerInput.MousePosition.ToPoint() - value.Rect.Location;
				if (this.SelectMultiple && !this.AllSelected.Contains(this.DraggedElement))
				{
					this.Select(this.DraggedElement.ToEnumerable<GUIComponent>());
				}
			}
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x0600132A RID: 4906 RVA: 0x000B882B File Offset: 0x000B6A2B
		// (set) Token: 0x0600132B RID: 4907 RVA: 0x000B8833 File Offset: 0x000B6A33
		public bool CanInteractWhenUnfocusable { get; set; }

		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x0600132C RID: 4908 RVA: 0x000B883C File Offset: 0x000B6A3C
		public override Rectangle MouseRect
		{
			get
			{
				if (!this.CanBeFocused && !this.CanInteractWhenUnfocusable)
				{
					return Rectangle.Empty;
				}
				if (!base.ClampMouseRectToParent)
				{
					return this.Rect;
				}
				return base.ClampRect(this.Rect);
			}
		}

		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x0600132D RID: 4909 RVA: 0x000B886F File Offset: 0x000B6A6F
		// (set) Token: 0x0600132E RID: 4910 RVA: 0x000B8877 File Offset: 0x000B6A77
		public override bool PlaySoundOnSelect { get; set; }

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x0600132F RID: 4911 RVA: 0x000B8880 File Offset: 0x000B6A80
		// (set) Token: 0x06001330 RID: 4912 RVA: 0x000B8888 File Offset: 0x000B6A88
		public bool PlaySoundOnDragStop { get; set; }

		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x06001331 RID: 4913 RVA: 0x000B8891 File Offset: 0x000B6A91
		// (set) Token: 0x06001332 RID: 4914 RVA: 0x000B8899 File Offset: 0x000B6A99
		public GUISoundType? SoundOnDragStart { get; set; }

		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x06001333 RID: 4915 RVA: 0x000B88A2 File Offset: 0x000B6AA2
		// (set) Token: 0x06001334 RID: 4916 RVA: 0x000B88AA File Offset: 0x000B6AAA
		public GUISoundType? SoundOnDragStop { get; set; }

		// Token: 0x06001335 RID: 4917 RVA: 0x000B88B3 File Offset: 0x000B6AB3
		private GUIListBox.AutoScroll GetAutoScroll(bool b)
		{
			if (!b)
			{
				return GUIListBox.AutoScroll.Disabled;
			}
			return GUIListBox.AutoScroll.Enabled;
		}

		// Token: 0x06001336 RID: 4918 RVA: 0x000B88BC File Offset: 0x000B6ABC
		public GUIListBox(RectTransform rectT, bool isHorizontal = false, Color? color = null, string style = "", bool isScrollBarOnDefaultSide = true, bool useMouseDownToSelect = false) : base(style, rectT)
		{
			this.isHorizontal = isHorizontal;
			this.HoverCursor = CursorState.Hand;
			this.CanBeFocused = true;
			this.selected = new List<GUIComponent>();
			this.useMouseDownToSelect = useMouseDownToSelect;
			this.ContentBackground = new GUIFrame(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), style, null)
			{
				CanBeFocused = false
			};
			this.Content = new GUIFrame(new RectTransform(Vector2.One, this.ContentBackground.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			this.Content.RectTransform.ChildrenChanged += delegate(RectTransform _)
			{
				this.scrollBarNeedsRecalculation = true;
				this.childrenNeedsRecalculation = true;
			};
			if (style != null)
			{
				GUIStyle.Apply(this.ContentBackground, "", this);
			}
			if (color != null)
			{
				this.color = color.Value;
			}
			this.IsScrollBarOnDefaultSide = isScrollBarOnDefaultSide;
			Point size;
			Anchor anchor;
			if (isHorizontal)
			{
				size = new Point((int)((float)this.Rect.Width - this.Padding.X - this.Padding.Z), (int)((float)this.ScrollBarSize * GUI.Scale));
				anchor = (isScrollBarOnDefaultSide ? Anchor.BottomCenter : Anchor.TopCenter);
			}
			else
			{
				size = new Point(this.ScrollBarSize, (int)((float)this.Rect.Height - this.Padding.Y - this.Padding.W));
				anchor = (isScrollBarOnDefaultSide ? Anchor.CenterRight : Anchor.CenterLeft);
			}
			RectTransform rectTransform = new RectTransform(size, rectT, anchor, null, ScaleBasis.Normal, false);
			rectTransform.AbsoluteOffset = (isHorizontal ? new Point(0, this.IsScrollBarOnDefaultSide ? ((int)this.Padding.W) : ((int)this.Padding.Y)) : new Point(this.IsScrollBarOnDefaultSide ? ((int)this.Padding.Z) : ((int)this.Padding.X), 0));
			float barSize = 1f;
			bool? flag = new bool?(isHorizontal);
			this.ScrollBar = new GUIScrollBar(rectTransform, barSize, null, "", flag);
			this.UpdateScrollBarSize();
			this.Enabled = true;
			this.ScrollBar.BarScroll = 0f;
			base.RectTransform.ScaleChanged += delegate()
			{
				this.dimensionsNeedsRecalculation = true;
			};
			base.RectTransform.SizeChanged += delegate()
			{
				this.dimensionsNeedsRecalculation = true;
			};
			this.UpdateDimensions();
			rectT.ChildrenChanged += this.CheckForChildren;
		}

		// Token: 0x06001337 RID: 4919 RVA: 0x000B8B97 File Offset: 0x000B6D97
		private void CheckForChildren(RectTransform rectT)
		{
			if (rectT == this.ScrollBar.RectTransform || rectT == this.Content.RectTransform || rectT == this.ContentBackground.RectTransform)
			{
				return;
			}
			throw new InvalidOperationException("Children were added to GUIListBox, Add them to GUIListBox.Content instead.");
		}

		// Token: 0x06001338 RID: 4920 RVA: 0x000B8BD0 File Offset: 0x000B6DD0
		public void UpdateDimensions()
		{
			this.dimensionsNeedsRecalculation = false;
			this.ContentBackground.RectTransform.Resize(this.Rect.Size, true);
			Point contentSize = (this.ResizeContentToMakeSpaceForScrollBar && (this.KeepSpaceForScrollBar ? this.ScrollBarEnabled : this.ScrollBarVisible)) ? this.CalculateFrameSize(this.ScrollBar.IsHorizontal, this.ScrollBarSize) : this.Rect.Size;
			this.Content.RectTransform.Resize(new Point((int)((float)contentSize.X - this.Padding.X - this.Padding.Z), (int)((float)contentSize.Y - this.Padding.Y - this.Padding.W)), true);
			if (!this.IsScrollBarOnDefaultSide)
			{
				this.Content.RectTransform.SetPosition(Anchor.BottomRight, null);
			}
			this.Content.RectTransform.AbsoluteOffset = new Point(this.IsScrollBarOnDefaultSide ? ((int)this.Padding.X) : ((int)this.Padding.Z), this.IsScrollBarOnDefaultSide ? ((int)this.Padding.Y) : ((int)this.Padding.W));
			this.ScrollBar.RectTransform.Resize(this.ScrollBar.IsHorizontal ? new Point((int)((float)this.Rect.Width - this.Padding.X - this.Padding.Z), this.ScrollBarSize) : new Point(this.ScrollBarSize, (int)((float)this.Rect.Height - this.Padding.Y - this.Padding.W)), true);
			this.ScrollBar.RectTransform.AbsoluteOffset = (this.ScrollBar.IsHorizontal ? new Point(0, (int)this.Padding.W) : new Point((int)this.Padding.Z, 0));
			this.UpdateScrollBarSize();
		}

		// Token: 0x06001339 RID: 4921 RVA: 0x000B8DEC File Offset: 0x000B6FEC
		public bool Select(object userData, GUIListBox.Force force = GUIListBox.Force.No, GUIListBox.AutoScroll autoScroll = GUIListBox.AutoScroll.Enabled)
		{
			IEnumerable<GUIComponent> children = this.Content.Children;
			int i = 0;
			bool wasSelected = false;
			foreach (GUIComponent child in children)
			{
				if (object.Equals(child.UserData, userData))
				{
					wasSelected = true;
					this.Select(i, force, autoScroll, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
					if (!this.SelectMultiple)
					{
						return true;
					}
				}
				i++;
			}
			return wasSelected;
		}

		// Token: 0x0600133A RID: 4922 RVA: 0x000B8E70 File Offset: 0x000B7070
		private Point CalculateFrameSize(bool isHorizontal, int scrollBarSize)
		{
			if (!isHorizontal)
			{
				return new Point(this.Rect.Width - scrollBarSize, this.Rect.Height);
			}
			return new Point(this.Rect.Width, this.Rect.Height - scrollBarSize);
		}

		// Token: 0x0600133B RID: 4923 RVA: 0x000B8EB0 File Offset: 0x000B70B0
		public Vector2 CalculateTopOffset()
		{
			int x = 0;
			int y = 0;
			if (this.ScrollBar.BarSize < 1f)
			{
				if (this.ScrollBar.IsHorizontal)
				{
					x -= (int)((float)(this.totalSize - this.Content.Rect.Width) * this.ScrollBar.BarScroll);
				}
				else
				{
					y -= (int)((float)(this.totalSize - this.Content.Rect.Height) * this.ScrollBar.BarScroll);
				}
			}
			return new Vector2((float)x, (float)y);
		}

		// Token: 0x0600133C RID: 4924 RVA: 0x000B8F3C File Offset: 0x000B713C
		private void CalculateChildrenOffsets(Action<int, Point> callback)
		{
			GUIListBox.<>c__DisplayClass171_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.callback = callback;
			Vector2 topOffset = this.CalculateTopOffset();
			CS$<>8__locals1.x = (int)topOffset.X;
			CS$<>8__locals1.y = (int)topOffset.Y;
			GUIListBox.<>c__DisplayClass171_1 CS$<>8__locals2;
			CS$<>8__locals2.i = 0;
			while (CS$<>8__locals2.i < this.Content.CountChildren)
			{
				GUIComponent child = this.Content.GetChild(CS$<>8__locals2.i);
				if (child != null && child.Visible)
				{
					if (base.RectTransform != null)
					{
						CS$<>8__locals1.callback(CS$<>8__locals2.i, new Point(CS$<>8__locals1.x, CS$<>8__locals1.y));
					}
					if (this.useGridLayout)
					{
						if (this.ScrollBar.IsHorizontal)
						{
							this.<CalculateChildrenOffsets>g__advanceGridLayout|171_0(ref CS$<>8__locals1.y, ref CS$<>8__locals1.x, child.Rect.Height, child.Rect.Width, this.Content.Rect.Height, ref CS$<>8__locals1, ref CS$<>8__locals2);
						}
						else
						{
							this.<CalculateChildrenOffsets>g__advanceGridLayout|171_0(ref CS$<>8__locals1.x, ref CS$<>8__locals1.y, child.Rect.Width, child.Rect.Height, this.Content.Rect.Width, ref CS$<>8__locals1, ref CS$<>8__locals2);
						}
					}
					else if (this.ScrollBar.IsHorizontal)
					{
						CS$<>8__locals1.x += child.Rect.Width + this.Spacing;
					}
					else
					{
						CS$<>8__locals1.y += child.Rect.Height + this.Spacing;
					}
				}
				int i = CS$<>8__locals2.i;
				CS$<>8__locals2.i = i + 1;
			}
		}

		// Token: 0x0600133D RID: 4925 RVA: 0x000B90E9 File Offset: 0x000B72E9
		private void RepositionChildren()
		{
			this.CalculateChildrenOffsets(delegate(int index, Point offset)
			{
				GUIComponent child = this.Content.GetChild(index);
				if (child != this.draggedElement && child.RectTransform.AbsoluteOffset != offset)
				{
					child.RectTransform.AbsoluteOffset = offset;
				}
			});
		}

		// Token: 0x0600133E RID: 4926 RVA: 0x000B9100 File Offset: 0x000B7300
		public void ScrollToElement(GUIComponent component, GUIListBox.PlaySelectSound playSelectSound = GUIListBox.PlaySelectSound.No)
		{
			if (playSelectSound == GUIListBox.PlaySelectSound.Yes)
			{
				SoundPlayer.PlayUISound(GUISoundType.Select);
			}
			List<GUIComponent> children = this.Content.Children.ToList<GUIComponent>();
			int index = children.IndexOf(component);
			if (index < 0)
			{
				return;
			}
			if (!this.Content.Children.Contains(component) || !component.Visible)
			{
				this.<ScrollToElement>g__performScroll|173_0(null);
				return;
			}
			this.<ScrollToElement>g__performScroll|173_0(component);
		}

		// Token: 0x0600133F RID: 4927 RVA: 0x000B9160 File Offset: 0x000B7360
		public void ScrollToEnd(float duration)
		{
			GUIListBox.<>c__DisplayClass174_0 CS$<>8__locals1 = new GUIListBox.<>c__DisplayClass174_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.duration = duration;
			CoroutineManager.StartCoroutine(CS$<>8__locals1.<ScrollToEnd>g__ScrollCoroutine|0(), "");
		}

		// Token: 0x06001340 RID: 4928 RVA: 0x000B9192 File Offset: 0x000B7392
		private void StartDraggingElement(GUIComponent child)
		{
			this.DraggedElement = child;
			if (Timing.TotalTime > this.lastDragStartTime + 0.20000000298023224)
			{
				this.lastDragStartTime = Timing.TotalTime;
				SoundPlayer.PlayUISound(this.SoundOnDragStart);
			}
		}

		// Token: 0x06001341 RID: 4929 RVA: 0x000B91C8 File Offset: 0x000B73C8
		private bool UpdateDragging()
		{
			if (this.CurrentDragMode == GUIListBox.DragMode.NoDragging || !this.isDraggingElement)
			{
				return false;
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				GUIComponent draggedElem = this.draggedElement;
				GUIListBox.OnRearrangedHandler onRearranged = this.OnRearranged;
				if (onRearranged != null)
				{
					onRearranged(this, draggedElem.UserData);
				}
				this.DraggedElement = null;
				if (this.PlaySoundOnDragStop)
				{
					SoundPlayer.PlayUISound(this.SoundOnDragStop);
				}
				this.RepositionChildren();
				return this.AllSelected.Contains(draggedElem);
			}
			GUIListBox.<>c__DisplayClass177_0 CS$<>8__locals1 = new GUIListBox.<>c__DisplayClass177_0();
			CS$<>8__locals1.<>4__this = this;
			Vector2 topOffset = this.CalculateTopOffset();
			Point mousePos = PlayerInput.MousePosition.ToPoint();
			this.draggedElement.RectTransform.AbsoluteOffset = mousePos - this.Content.Rect.Location - this.dragMousePosRelativeToTopLeftCorner;
			if (this.CurrentDragMode != GUIListBox.DragMode.DragOutsideBox)
			{
				Point offset2 = this.draggedElement.RectTransform.AbsoluteOffset;
				this.draggedElement.RectTransform.AbsoluteOffset = (this.isHorizontal ? new Point(offset2.X, 0) : new Point(0, offset2.Y));
			}
			CS$<>8__locals1.index = this.Content.RectTransform.GetChildIndex(this.draggedElement.RectTransform);
			CS$<>8__locals1.newIndex = CS$<>8__locals1.index;
			CS$<>8__locals1.draggedOffsetWhenReleased = Point.Zero;
			this.CalculateChildrenOffsets(delegate(int i, Point offset)
			{
				if (CS$<>8__locals1.index != i)
				{
					return;
				}
				CS$<>8__locals1.draggedOffsetWhenReleased = offset;
			});
			Rectangle draggedRectWhenReleased = new Rectangle(this.Content.Rect.Location + CS$<>8__locals1.draggedOffsetWhenReleased, this.draggedElement.Rect.Size);
			if (this.isHorizontal)
			{
				CS$<>8__locals1.<UpdateDragging>g__shiftIndices|1((float)mousePos.X, ref draggedRectWhenReleased.X, draggedRectWhenReleased.Width);
			}
			else
			{
				CS$<>8__locals1.<UpdateDragging>g__shiftIndices|1((float)mousePos.Y, ref draggedRectWhenReleased.Y, draggedRectWhenReleased.Height);
			}
			if (CS$<>8__locals1.newIndex != CS$<>8__locals1.index)
			{
				if (this.AllSelected.Count > 1)
				{
					this.selected.Sort((GUIComponent a, GUIComponent b) => this.Content.GetChildIndex(a) - this.Content.GetChildIndex(b));
					int indexOfDraggedElem = this.AllSelected.IndexOf(this.draggedElement);
					IEnumerable<GUIComponent> allSelected = this.AllSelected;
					if (CS$<>8__locals1.newIndex > CS$<>8__locals1.index)
					{
						allSelected = allSelected.Reverse<GUIComponent>();
					}
					using (IEnumerator<GUIComponent> enumerator = allSelected.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							GUIComponent elem = enumerator.Current;
							elem.RectTransform.RepositionChildInHierarchy(CS$<>8__locals1.newIndex + this.AllSelected.IndexOf(elem) - indexOfDraggedElem);
						}
						goto IL_2A7;
					}
				}
				this.draggedElement.RectTransform.RepositionChildInHierarchy(CS$<>8__locals1.newIndex);
				IL_2A7:
				this.HasDraggedElementIndexChanged = true;
			}
			return true;
		}

		// Token: 0x06001342 RID: 4930 RVA: 0x000B9498 File Offset: 0x000B7698
		private void UpdateChildrenRect()
		{
			if (this.UpdateDragging())
			{
				return;
			}
			if (this.SelectTop)
			{
				foreach (GUIComponent child in this.Content.Children)
				{
					child.CanBeFocused = !this.selected.Contains(child);
					if (!child.CanBeFocused)
					{
						child.State = GUIComponent.ComponentState.None;
					}
				}
			}
			if (this.SelectTop && this.Content.Children.Any<GUIComponent>() && this.scrollToElement == null)
			{
				GUIComponent component = this.Content.Children.FirstOrDefault((GUIComponent c) => (float)(c.Rect.Y - this.Content.Rect.Y) / (float)c.Rect.Height > -0.1f);
				if (component != null && !this.selected.Contains(component))
				{
					int index = this.Content.Children.ToList<GUIComponent>().IndexOf(component);
					if (index >= 0)
					{
						this.Select(index, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled, GUIListBox.TakeKeyBoardFocus.Yes, GUIListBox.PlaySelectSound.No);
					}
				}
			}
			for (int i = 0; i < this.Content.CountChildren; i++)
			{
				RectTransform child3 = this.Content.RectTransform.GetChild(i);
				GUIComponent child2 = (child3 != null) ? child3.GUIComponent : null;
				if (child2 != null && child2.Visible)
				{
					if (this.Enabled && (this.CanBeFocused || this.CanInteractWhenUnfocusable) && child2.CanBeFocused && child2.Rect.Contains(PlayerInput.MousePosition) && GUI.IsMouseOn(child2))
					{
						child2.State = GUIComponent.ComponentState.Hover;
						bool mouseDown = this.useMouseDownToSelect ? PlayerInput.PrimaryMouseButtonDown() : PlayerInput.PrimaryMouseButtonClicked();
						if (mouseDown)
						{
							if (this.SelectTop)
							{
								this.ScrollToElement(child2, GUIListBox.PlaySelectSound.No);
							}
							this.Select(i, GUIListBox.Force.No, GUIListBox.AutoScroll.Disabled, GUIListBox.TakeKeyBoardFocus.Yes, GUIListBox.PlaySelectSound.Yes);
						}
						if (this.CurrentDragMode != GUIListBox.DragMode.NoDragging && (this.CurrentSelectMode != GUIListBox.SelectMode.RequireShiftToSelectMultiple || (!PlayerInput.IsShiftDown() && !PlayerInput.IsCtrlDown())) && PlayerInput.PrimaryMouseButtonDown() && GUI.MouseOn == child2)
						{
							this.StartDraggingElement(child2);
						}
					}
					else if (this.selected.Contains(child2))
					{
						child2.State = GUIComponent.ComponentState.Selected;
						if (this.CheckSelected != null && this.CheckSelected() != child2.UserData)
						{
							this.selected.Remove(child2);
						}
					}
					else
					{
						child2.State = ((!child2.ExternalHighlight) ? GUIComponent.ComponentState.None : GUIComponent.ComponentState.Hover);
					}
				}
			}
		}

		// Token: 0x06001343 RID: 4931 RVA: 0x000B9700 File Offset: 0x000B7900
		public override void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			if (!base.Visible)
			{
				return;
			}
			if (!ignoreChildren)
			{
				foreach (GUIComponent child in base.Children)
				{
					if (child != this.Content && child != this.ScrollBar && child != this.ContentBackground)
					{
						child.AddToGUIUpdateList(ignoreChildren, order);
					}
				}
			}
			foreach (GUIComponent child2 in this.Content.Children)
			{
				if (!this.childVisible.ContainsKey(child2))
				{
					this.childVisible[child2] = child2.Visible;
				}
				if (this.childVisible[child2] != child2.Visible)
				{
					this.childVisible[child2] = child2.Visible;
					this.childrenNeedsRecalculation = true;
					this.scrollBarNeedsRecalculation = true;
					break;
				}
			}
			if (this.childrenNeedsRecalculation)
			{
				this.RecalculateChildren();
				this.childVisible.Clear();
			}
			base.UpdateOrder = order;
			GUI.AddToUpdateList(this);
			if (ignoreChildren)
			{
				Action<GUIComponent> onAddedToGUIUpdateList = this.OnAddedToGUIUpdateList;
				if (onAddedToGUIUpdateList == null)
				{
					return;
				}
				onAddedToGUIUpdateList(this);
				return;
			}
			else
			{
				int lastVisible = 0;
				for (int i = 0; i < this.Content.CountChildren; i++)
				{
					GUIComponent child3 = this.Content.GetChild(i);
					if (child3.Visible)
					{
						if (!this.IsChildInsideFrame(child3))
						{
							if (lastVisible > 0)
							{
								break;
							}
						}
						else
						{
							lastVisible = i;
							child3.AddToGUIUpdateList(false, order);
						}
					}
				}
				if (this.ScrollBar.Enabled)
				{
					this.ScrollBar.AddToGUIUpdateList(false, order);
				}
				Action<GUIComponent> onAddedToGUIUpdateList2 = this.OnAddedToGUIUpdateList;
				if (onAddedToGUIUpdateList2 == null)
				{
					return;
				}
				onAddedToGUIUpdateList2(this);
				return;
			}
		}

		// Token: 0x06001344 RID: 4932 RVA: 0x000B98C4 File Offset: 0x000B7AC4
		public override void ForceLayoutRecalculation()
		{
			base.ForceLayoutRecalculation();
			this.Content.ForceLayoutRecalculation();
			this.ScrollBar.ForceLayoutRecalculation();
		}

		// Token: 0x06001345 RID: 4933 RVA: 0x000B98E4 File Offset: 0x000B7AE4
		public void RecalculateChildren()
		{
			foreach (GUIComponent child in this.Content.Children)
			{
				this.ClampChildMouseRects(child);
			}
			this.RepositionChildren();
			this.childrenNeedsRecalculation = false;
		}

		// Token: 0x06001346 RID: 4934 RVA: 0x000B9944 File Offset: 0x000B7B44
		private void ClampChildMouseRects(GUIComponent child)
		{
			child.ClampMouseRectToParent = true;
			if (child is GUIListBox)
			{
				return;
			}
			foreach (GUIComponent grandChild in child.Children)
			{
				this.ClampChildMouseRects(grandChild);
			}
		}

		// Token: 0x06001347 RID: 4935 RVA: 0x000B99A4 File Offset: 0x000B7BA4
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			this.UpdateChildrenRect();
			this.RepositionChildren();
			if (this.scrollBarNeedsRecalculation)
			{
				this.UpdateScrollBarSize();
			}
			if (this.FadeElements)
			{
				foreach (KeyValuePair<GUIComponent, bool> keyValuePair in this.childVisible)
				{
					GUIComponent guicomponent;
					bool flag;
					keyValuePair.Deconstruct(out guicomponent, out flag);
					GUIComponent component = guicomponent;
					float lerp = 0f;
					float y = (float)component.Rect.Y;
					float contentY = (float)this.Content.Rect.Y;
					float height = (float)component.Rect.Height;
					if (y < (float)this.Content.Rect.Y)
					{
						float distance = (contentY - y) / height;
						lerp = distance;
					}
					float centerY = (float)this.Content.Rect.Y + (float)this.Content.Rect.Height / 2f;
					if (y > centerY)
					{
						float distance2 = (y - centerY) / (centerY - height);
						lerp = distance2;
					}
					component.Color = (component.HoverColor = ToolBox.GradientLerp(lerp, new Color[]
					{
						component.DefaultColor,
						Color.Transparent
					}));
					component.DisabledColor = ToolBox.GradientLerp(lerp, new Color[]
					{
						component.Style.DisabledColor,
						Color.Transparent
					});
					component.HoverColor = ToolBox.GradientLerp(lerp, new Color[]
					{
						component.Style.HoverColor,
						Color.Transparent
					});
					foreach (GUIComponent child in component.GetAllChildren())
					{
						Color gradient = ToolBox.GradientLerp(lerp, new Color[]
						{
							child.DefaultColor,
							Color.Transparent
						});
						child.Color = (child.HoverColor = gradient);
						GUITextBlock block = child as GUITextBlock;
						if (block != null)
						{
							block.TextColor = (block.HoverTextColor = gradient);
						}
					}
				}
			}
			if (this.scrollToElement != null)
			{
				if (!this.scrollToElement.Visible || !this.Content.Children.Contains(this.scrollToElement))
				{
					this.scrollToElement = null;
				}
				else
				{
					float diff = (float)(this.isHorizontal ? (this.scrollToElement.Rect.X - this.Content.Rect.X) : (this.scrollToElement.Rect.Y - this.Content.Rect.Y));
					float speed = MathHelper.Clamp(Math.Abs(diff) * 0.1f, 5f, 100f);
					if (Math.Abs(diff) < speed || GUIScrollBar.DraggingBar != null)
					{
						speed = Math.Abs(diff);
						this.scrollToElement = null;
					}
					this.BarScroll += speed * (float)Math.Sign(diff) / this.TotalSize;
				}
			}
			if (PlayerInput.ScrollWheelSpeed != 0 && this.AllowMouseWheelScroll && this.<Update>g__IsMouseOn|183_0())
			{
				if (this.SmoothScroll)
				{
					if (this.ClampScrollToElements)
					{
						bool scrollDown = Math.Clamp(PlayerInput.ScrollWheelSpeed, 0, 1) > 0;
						if (scrollDown)
						{
							this.SelectPrevious(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.Yes, GUIListBox.PlaySelectSound.Yes);
						}
						else
						{
							this.SelectNext(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.Yes, GUIListBox.PlaySelectSound.Yes);
						}
					}
				}
				else
				{
					this.ScrollBar.BarScroll -= (float)PlayerInput.ScrollWheelSpeed / 500f * this.ScrollBar.UnclampedBarSize;
				}
			}
			this.ScrollBar.Enabled = (this.ScrollBarEnabled && this.BarSize < 1f);
			if (this.AutoHideScrollBar)
			{
				this.ScrollBarVisible = this.ScrollBar.Enabled;
			}
			if (this.dimensionsNeedsRecalculation)
			{
				this.UpdateDimensions();
			}
		}

		// Token: 0x06001348 RID: 4936 RVA: 0x000B9DCC File Offset: 0x000B7FCC
		private static GUIListBox FindScrollableParentListBox(GUIComponent target)
		{
			GUIListBox listBox = target as GUIListBox;
			if (listBox != null && listBox.ScrollBarEnabled && listBox.BarSize < 1f)
			{
				return listBox;
			}
			if (((target != null) ? target.Parent : null) == null)
			{
				return null;
			}
			return GUIListBox.FindScrollableParentListBox(target.Parent);
		}

		// Token: 0x06001349 RID: 4937 RVA: 0x000B9E18 File Offset: 0x000B8018
		public void SelectNext(GUIListBox.Force force = GUIListBox.Force.No, GUIListBox.AutoScroll autoScroll = GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus takeKeyBoardFocus = GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound playSelectSound = GUIListBox.PlaySelectSound.No)
		{
			int index = this.SelectedIndex + 1;
			while (index < this.Content.CountChildren)
			{
				GUIComponent child = this.Content.GetChild(index);
				if (child.Visible && child.CanBeFocused)
				{
					this.Select(index, force, this.GetAutoScroll(!this.SmoothScroll && autoScroll == GUIListBox.AutoScroll.Enabled), takeKeyBoardFocus, playSelectSound);
					if (this.SmoothScroll)
					{
						this.ScrollToElement(child, playSelectSound);
						return;
					}
					break;
				}
				else
				{
					index++;
				}
			}
		}

		// Token: 0x0600134A RID: 4938 RVA: 0x000B9E94 File Offset: 0x000B8094
		public void SelectPrevious(GUIListBox.Force force = GUIListBox.Force.No, GUIListBox.AutoScroll autoScroll = GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus takeKeyBoardFocus = GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound playSelectSound = GUIListBox.PlaySelectSound.No)
		{
			int index = this.SelectedIndex - 1;
			while (index >= 0)
			{
				GUIComponent child = this.Content.GetChild(index);
				if (child.Visible && child.CanBeFocused)
				{
					this.Select(index, force, this.GetAutoScroll(!this.SmoothScroll && autoScroll == GUIListBox.AutoScroll.Enabled), takeKeyBoardFocus, playSelectSound);
					if (this.SmoothScroll)
					{
						this.ScrollToElement(child, playSelectSound);
						return;
					}
					break;
				}
				else
				{
					index--;
				}
			}
		}

		// Token: 0x0600134B RID: 4939 RVA: 0x000B9F04 File Offset: 0x000B8104
		public void Select(int childIndex, GUIListBox.Force force = GUIListBox.Force.No, GUIListBox.AutoScroll autoScroll = GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus takeKeyBoardFocus = GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound playSelectSound = GUIListBox.PlaySelectSound.No)
		{
			if (childIndex >= this.Content.CountChildren || childIndex < 0 || this.CurrentSelectMode == GUIListBox.SelectMode.None)
			{
				return;
			}
			GUIComponent child = this.Content.GetChild(childIndex);
			if (child == null)
			{
				return;
			}
			if (!child.Enabled)
			{
				return;
			}
			bool wasSelected = true;
			if (this.OnSelected != null)
			{
				wasSelected = (force == GUIListBox.Force.Yes || this.OnSelected(child, child.UserData));
			}
			if (!wasSelected)
			{
				return;
			}
			if (this.CurrentSelectMode == GUIListBox.SelectMode.SelectMultiple || (this.CurrentSelectMode == GUIListBox.SelectMode.RequireShiftToSelectMultiple && PlayerInput.IsCtrlDown()))
			{
				if (this.selected.Contains(child))
				{
					this.selected.Remove(child);
				}
				else
				{
					this.selected.Add(child);
				}
			}
			else if (this.CurrentSelectMode == GUIListBox.SelectMode.RequireShiftToSelectMultiple && PlayerInput.IsShiftDown())
			{
				GUIComponent first = this.SelectedComponent ?? child;
				GUIComponent last = child;
				int firstIndex = this.Content.GetChildIndex(first);
				int lastIndex = this.Content.GetChildIndex(last);
				int sgn = Math.Sign(lastIndex - firstIndex);
				this.selected.Clear();
				this.selected.Add(first);
				for (int i = firstIndex + sgn; i != lastIndex; i += sgn)
				{
					GUIComponent interChild = this.Content.GetChild(i);
					if (interChild != null && interChild.Visible)
					{
						this.selected.Add(interChild);
					}
				}
				if (first != last)
				{
					this.selected.Add(last);
				}
			}
			else
			{
				this.selected.Clear();
				this.selected.Add(child);
			}
			if (autoScroll == GUIListBox.AutoScroll.Enabled)
			{
				if (this.ScrollBar.IsHorizontal)
				{
					if (child.Rect.X < this.MouseRect.X)
					{
						this.ScrollBar.BarScroll -= (float)(this.MouseRect.X - child.Rect.X) / (float)(this.totalSize - this.Content.Rect.Width);
					}
					else if (child.Rect.Right > this.MouseRect.Right)
					{
						this.ScrollBar.BarScroll += (float)(child.Rect.Right - this.MouseRect.Right) / (float)(this.totalSize - this.Content.Rect.Width);
					}
				}
				else if (child.Rect.Y < this.MouseRect.Y)
				{
					this.ScrollBar.BarScroll -= (float)(this.MouseRect.Y - child.Rect.Y) / (float)(this.totalSize - this.Content.Rect.Height);
				}
				else if (child.Rect.Bottom > this.MouseRect.Bottom)
				{
					this.ScrollBar.BarScroll += (float)(child.Rect.Bottom - this.MouseRect.Bottom) / (float)(this.totalSize - this.Content.Rect.Height);
				}
			}
			if (takeKeyBoardFocus == GUIListBox.TakeKeyBoardFocus.Yes && this.CanTakeKeyBoardFocus)
			{
				if (base.RectTransform.GetAllChildren().None((RectTransform rt) => rt.GUIComponent == GUI.KeyboardDispatcher.Subscriber))
				{
					this.Selected = true;
					GUI.KeyboardDispatcher.Subscriber = this;
				}
			}
			if (playSelectSound == GUIListBox.PlaySelectSound.Yes && this.PlaySoundOnSelect && !child.PlaySoundOnSelect && (GUI.MouseOn == null || GUI.MouseOn.Parent == this.Content || !GUI.MouseOn.PlaySoundOnSelect))
			{
				SoundPlayer.PlayUISound(GUISoundType.Select);
			}
			GUIListBox.OnSelectedHandler afterSelected = this.AfterSelected;
			if (afterSelected == null)
			{
				return;
			}
			afterSelected(child, this.SelectedData);
		}

		// Token: 0x0600134C RID: 4940 RVA: 0x000BA2DC File Offset: 0x000B84DC
		public void Select(IEnumerable<GUIComponent> children)
		{
			if (this.CurrentSelectMode == GUIListBox.SelectMode.None)
			{
				return;
			}
			this.Selected = true;
			this.selected.Clear();
			this.selected.AddRange(from c in children
			where this.Content.Children.Contains(c)
			select c);
			foreach (GUIComponent child in this.selected)
			{
				GUIListBox.OnSelectedHandler onSelected = this.OnSelected;
				if (onSelected != null)
				{
					onSelected(child, child.UserData);
				}
			}
			GUIListBox.OnSelectedHandler afterSelected = this.AfterSelected;
			if (afterSelected == null)
			{
				return;
			}
			afterSelected(children.FirstOrDefault<GUIComponent>(), this.SelectedData);
		}

		// Token: 0x0600134D RID: 4941 RVA: 0x000BA398 File Offset: 0x000B8598
		public void Deselect()
		{
			this.Selected = false;
			if (GUI.KeyboardDispatcher.Subscriber == this)
			{
				GUI.KeyboardDispatcher.Subscriber = null;
			}
			this.selected.Clear();
		}

		// Token: 0x0600134E RID: 4942 RVA: 0x000BA3C4 File Offset: 0x000B85C4
		public void DeselectElement(GUIComponent child)
		{
			if (child == null)
			{
				return;
			}
			if (this.selected.Contains(child))
			{
				this.selected.Remove(child);
			}
		}

		// Token: 0x0600134F RID: 4943 RVA: 0x000BA3E8 File Offset: 0x000B85E8
		public void UpdateScrollBarSize()
		{
			this.scrollBarNeedsRecalculation = false;
			if (this.Content == null)
			{
				return;
			}
			this.totalSize = 0;
			IEnumerable<GUIComponent> children = from c in this.Content.Children
			where c.Visible
			select c;
			if (this.useGridLayout)
			{
				int pos = 0;
				using (IEnumerator<GUIComponent> enumerator = children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GUIComponent child = enumerator.Current;
						if (this.ScrollBar.IsHorizontal)
						{
							if (pos + child.Rect.Height + this.Spacing > this.Content.Rect.Height)
							{
								pos = 0;
								this.totalSize += child.Rect.Width + this.Spacing;
							}
							pos += child.Rect.Height + this.Spacing;
							if (child == children.Last<GUIComponent>())
							{
								this.totalSize += child.Rect.Width + this.Spacing;
							}
						}
						else
						{
							if (pos + child.Rect.Width + this.Spacing > this.Content.Rect.Width)
							{
								pos = 0;
								this.totalSize += child.Rect.Height + this.Spacing;
							}
							pos += child.Rect.Width + this.Spacing;
							if (child == children.Last<GUIComponent>())
							{
								this.totalSize += child.Rect.Height + this.Spacing;
							}
						}
					}
					goto IL_26C;
				}
			}
			foreach (GUIComponent child2 in children)
			{
				this.totalSize += (this.ScrollBar.IsHorizontal ? child2.Rect.Width : child2.Rect.Height);
			}
			this.totalSize += this.Content.CountChildren * this.Spacing;
			if (this.PadBottom)
			{
				GUIComponent last = this.Content.Children.LastOrDefault<GUIComponent>();
				if (last != null)
				{
					this.totalSize += this.Rect.Height - last.Rect.Height;
				}
			}
			IL_26C:
			float minScrollBarSize = 20f;
			this.ScrollBar.UnclampedBarSize = (this.ScrollBar.IsHorizontal ? Math.Min((float)this.Content.Rect.Width / (float)this.totalSize, 1f) : Math.Min((float)this.Content.Rect.Height / (float)this.totalSize, 1f));
			this.ScrollBar.BarSize = (this.ScrollBar.IsHorizontal ? Math.Max(this.ScrollBar.UnclampedBarSize, minScrollBarSize / (float)this.Content.Rect.Width) : Math.Max(this.ScrollBar.UnclampedBarSize, minScrollBarSize / (float)this.Content.Rect.Height));
		}

		// Token: 0x06001350 RID: 4944 RVA: 0x000BA75C File Offset: 0x000B895C
		public override void ClearChildren()
		{
			this.Content.ClearChildren();
			this.selected.Clear();
		}

		// Token: 0x06001351 RID: 4945 RVA: 0x000BA774 File Offset: 0x000B8974
		public override void RemoveChild(GUIComponent child)
		{
			if (child == null)
			{
				return;
			}
			child.RectTransform.Parent = null;
			if (this.selected.Contains(child))
			{
				this.selected.Remove(child);
			}
			if (this.draggedElement == child)
			{
				this.DraggedElement = null;
			}
			this.UpdateScrollBarSize();
		}

		// Token: 0x06001352 RID: 4946 RVA: 0x000BA7C2 File Offset: 0x000B89C2
		public override void DrawChildren(SpriteBatch spriteBatch, bool recursive)
		{
		}

		// Token: 0x06001353 RID: 4947 RVA: 0x000BA7C4 File Offset: 0x000B89C4
		protected override void Draw(SpriteBatch spriteBatch)
		{
			if (!base.Visible)
			{
				return;
			}
			this.ContentBackground.DrawManually(spriteBatch, false, true);
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (this.HideChildrenOutsideFrame && this.Content.CountChildren > 0)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, this.Content.Rect);
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			int lastVisible = 0;
			int i = 0;
			foreach (GUIComponent child in this.Content.Children)
			{
				if (child.Visible && (child != this.draggedElement || this.CurrentDragMode != GUIListBox.DragMode.DragOutsideBox))
				{
					if (!this.IsChildInsideFrame(child))
					{
						if (lastVisible > 0)
						{
							break;
						}
					}
					else
					{
						lastVisible = i;
						child.DrawManually(spriteBatch, true, true);
						i++;
					}
				}
			}
			if (this.isDraggingElement && this.CurrentDragMode == GUIListBox.DragMode.DragOutsideBox && this.HideDraggedElement)
			{
				Rectangle drawRect = this.DraggedElement.Rect;
				int draggedElementIndex = this.Content.GetChildIndex(this.DraggedElement);
				this.CalculateChildrenOffsets(delegate(int index, Point point)
				{
					if (draggedElementIndex == index)
					{
						drawRect.Location = this.Content.Rect.Location + point;
					}
				});
				GUI.DrawRectangle(spriteBatch, drawRect, Color.White * 0.5f, false, 0f, 2f);
			}
			if (this.HideChildrenOutsideFrame && this.Content.CountChildren > 0)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
			if (this.isDraggingElement && this.CurrentDragMode == GUIListBox.DragMode.DragOutsideBox && !this.HideDraggedElement)
			{
				this.draggedElement.DrawManually(spriteBatch, true, true);
			}
			if (this.ScrollBarVisible)
			{
				this.ScrollBar.DrawManually(spriteBatch, true, true);
			}
		}

		// Token: 0x06001354 RID: 4948 RVA: 0x000BA9D8 File Offset: 0x000B8BD8
		private bool IsChildInsideFrame(GUIComponent child)
		{
			if (child == null)
			{
				return false;
			}
			if (this.ScrollBar.IsHorizontal)
			{
				if (child.Rect.Right < this.Content.Rect.X)
				{
					return false;
				}
				if (child.Rect.X > this.Content.Rect.Right)
				{
					return false;
				}
			}
			else
			{
				if (child.Rect.Bottom < this.Content.Rect.Y)
				{
					return false;
				}
				if (child.Rect.Y > this.Content.Rect.Bottom)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001355 RID: 4949 RVA: 0x000BAA80 File Offset: 0x000B8C80
		public void ReceiveTextInput(char inputChar)
		{
			GUI.KeyboardDispatcher.Subscriber = null;
		}

		// Token: 0x06001356 RID: 4950 RVA: 0x000BAA8D File Offset: 0x000B8C8D
		public void ReceiveTextInput(string text)
		{
		}

		// Token: 0x06001357 RID: 4951 RVA: 0x000BAA8F File Offset: 0x000B8C8F
		public void ReceiveCommandInput(char command)
		{
		}

		// Token: 0x06001358 RID: 4952 RVA: 0x000BAA91 File Offset: 0x000B8C91
		public void ReceiveEditingInput(string text, int start, int length)
		{
		}

		// Token: 0x06001359 RID: 4953 RVA: 0x000BAA94 File Offset: 0x000B8C94
		public void ReceiveSpecialInput(Keys key)
		{
			switch (key)
			{
			case Keys.Left:
				if (this.isHorizontal && this.AllowArrowKeyScroll)
				{
					this.SelectPrevious(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
					return;
				}
				break;
			case Keys.Up:
				if (!this.isHorizontal && this.AllowArrowKeyScroll)
				{
					this.SelectPrevious(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
					return;
				}
				break;
			case Keys.Right:
				if (this.isHorizontal && this.AllowArrowKeyScroll)
				{
					this.SelectNext(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
					return;
				}
				break;
			case Keys.Down:
				if (!this.isHorizontal && this.AllowArrowKeyScroll)
				{
					this.SelectNext(GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.Yes);
					return;
				}
				break;
			default:
				GUI.KeyboardDispatcher.Subscriber = null;
				break;
			}
		}

		// Token: 0x0600135D RID: 4957 RVA: 0x000BAB58 File Offset: 0x000B8D58
		[CompilerGenerated]
		private void <CalculateChildrenOffsets>g__advanceGridLayout|171_0(ref int primaryCoord, ref int secondaryCoord, int primaryChildDimension, int secondaryChildDimension, int primaryParentDimension, ref GUIListBox.<>c__DisplayClass171_0 A_6, ref GUIListBox.<>c__DisplayClass171_1 A_7)
		{
			if (primaryCoord + primaryChildDimension + this.Spacing > primaryParentDimension)
			{
				primaryCoord = 0;
				secondaryCoord += secondaryChildDimension + this.Spacing;
				A_6.callback(A_7.i, new Point(A_6.x, A_6.y));
			}
			primaryCoord += primaryChildDimension + this.Spacing;
		}

		// Token: 0x0600135F RID: 4959 RVA: 0x000BABFC File Offset: 0x000B8DFC
		[CompilerGenerated]
		private void <ScrollToElement>g__performScroll|173_0(GUIComponent c)
		{
			if (this.SmoothScroll && this.PadBottom)
			{
				this.scrollToElement = c;
				return;
			}
			float diff = (float)(this.isHorizontal ? (c.Rect.X - this.Content.Rect.X) : (c.Rect.Y - this.Content.Rect.Y));
			this.ScrollBar.BarScroll += diff / this.TotalSize;
		}

		// Token: 0x06001363 RID: 4963 RVA: 0x000BACE8 File Offset: 0x000B8EE8
		[CompilerGenerated]
		private bool <Update>g__IsMouseOn|183_0()
		{
			return GUIListBox.FindScrollableParentListBox(GUI.MouseOn) == this || GUI.IsMouseOn(this.ScrollBar) || (this.CanInteractWhenUnfocusable && this.Content.Rect.Contains(PlayerInput.MousePosition));
		}

		// Token: 0x04000978 RID: 2424
		protected List<GUIComponent> selected;

		// Token: 0x04000979 RID: 2425
		public GUIListBox.OnSelectedHandler OnSelected;

		// Token: 0x0400097A RID: 2426
		public GUIListBox.OnSelectedHandler AfterSelected;

		// Token: 0x0400097B RID: 2427
		public GUIListBox.CheckSelectedHandler CheckSelected;

		// Token: 0x0400097C RID: 2428
		public GUIListBox.OnRearrangedHandler OnRearranged;

		// Token: 0x04000980 RID: 2432
		private readonly Dictionary<GUIComponent, bool> childVisible = new Dictionary<GUIComponent, bool>();

		// Token: 0x04000981 RID: 2433
		private int totalSize;

		// Token: 0x04000982 RID: 2434
		private bool childrenNeedsRecalculation;

		// Token: 0x04000983 RID: 2435
		private bool scrollBarNeedsRecalculation;

		// Token: 0x04000984 RID: 2436
		private bool dimensionsNeedsRecalculation;

		// Token: 0x04000985 RID: 2437
		public GUIListBox.SelectMode CurrentSelectMode;

		// Token: 0x04000986 RID: 2438
		public bool HideChildrenOutsideFrame = true;

		// Token: 0x04000987 RID: 2439
		public bool ResizeContentToMakeSpaceForScrollBar = true;

		// Token: 0x04000988 RID: 2440
		private bool useGridLayout;

		// Token: 0x04000989 RID: 2441
		private GUIComponent scrollToElement;

		// Token: 0x04000991 RID: 2449
		private readonly bool useMouseDownToSelect;

		// Token: 0x04000992 RID: 2450
		private Vector4? overridePadding;

		// Token: 0x04000999 RID: 2457
		private GUIListBox.DragMode currentDragMode;

		// Token: 0x0400099A RID: 2458
		private GUIComponent draggedElement;

		// Token: 0x0400099B RID: 2459
		private Point dragMousePosRelativeToTopLeftCorner;

		// Token: 0x0400099D RID: 2461
		public bool HideDraggedElement;

		// Token: 0x0400099E RID: 2462
		private readonly bool isHorizontal;

		// Token: 0x040009A4 RID: 2468
		private double lastDragStartTime;

		// Token: 0x02000954 RID: 2388
		// (Invoke) Token: 0x0600718E RID: 29070
		public delegate bool OnSelectedHandler(GUIComponent component, object obj);

		// Token: 0x02000955 RID: 2389
		// (Invoke) Token: 0x06007192 RID: 29074
		public delegate object CheckSelectedHandler();

		// Token: 0x02000956 RID: 2390
		// (Invoke) Token: 0x06007196 RID: 29078
		public delegate void OnRearrangedHandler(GUIListBox listBox, object obj);

		// Token: 0x02000957 RID: 2391
		public enum SelectMode
		{
			// Token: 0x0400411A RID: 16666
			SelectSingle,
			// Token: 0x0400411B RID: 16667
			SelectMultiple,
			// Token: 0x0400411C RID: 16668
			RequireShiftToSelectMultiple,
			// Token: 0x0400411D RID: 16669
			None
		}

		// Token: 0x02000958 RID: 2392
		public enum DragMode
		{
			// Token: 0x0400411F RID: 16671
			NoDragging,
			// Token: 0x04004120 RID: 16672
			DragWithinBox,
			// Token: 0x04004121 RID: 16673
			DragOutsideBox
		}

		// Token: 0x02000959 RID: 2393
		public enum Force
		{
			// Token: 0x04004123 RID: 16675
			Yes,
			// Token: 0x04004124 RID: 16676
			No
		}

		// Token: 0x0200095A RID: 2394
		public enum AutoScroll
		{
			// Token: 0x04004126 RID: 16678
			Enabled,
			// Token: 0x04004127 RID: 16679
			Disabled
		}

		// Token: 0x0200095B RID: 2395
		public enum TakeKeyBoardFocus
		{
			// Token: 0x04004129 RID: 16681
			Yes,
			// Token: 0x0400412A RID: 16682
			No
		}

		// Token: 0x0200095C RID: 2396
		public enum PlaySelectSound
		{
			// Token: 0x0400412C RID: 16684
			Yes,
			// Token: 0x0400412D RID: 16685
			No
		}
	}
}
