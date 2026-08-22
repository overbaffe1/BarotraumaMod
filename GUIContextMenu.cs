using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000084 RID: 132
	[NullableContext(1)]
	[Nullable(0)]
	internal class GUIContextMenu : GUIComponent
	{
		// Token: 0x06001283 RID: 4739 RVA: 0x000B59EC File Offset: 0x000B3BEC
		public GUIContextMenu(Vector2? position, LocalizedString header, string style, params ContextMenuOption[] options) : base(style, new RectTransform(Point.Zero, GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false))
		{
			Vector2 pos = position ?? PlayerInput.MousePosition;
			GUIFont headerFont = GUIStyle.SubHeadingFont;
			GUIFont font = GUIStyle.SmallFont;
			Vector4 padding = new Vector4(4f);
			Vector4 headerPadding = new Vector4(8f);
			int horizontalPadding = (int)(padding.X + padding.Z);
			int verticalPadding = (int)(padding.Y + padding.W);
			bool hasHeader = !header.IsNullOrWhiteSpace();
			Dictionary<ContextMenuOption, Vector2> optionsAndSizes = new Dictionary<ContextMenuOption, Vector2>();
			Point estimatedSize = new Point(horizontalPadding, verticalPadding);
			if (hasHeader)
			{
				this.InflateSize(ref estimatedSize, header, headerFont);
			}
			foreach (ContextMenuOption option in options)
			{
				Vector2 optionSize = this.InflateSize(ref estimatedSize, option.Label, font);
				optionsAndSizes.Add(option, optionSize);
			}
			estimatedSize = estimatedSize.Multiply(1.2f);
			base.RectTransform.NonScaledSize = estimatedSize;
			base.RectTransform.AbsoluteOffset = pos.ToPoint();
			GUILayoutGroup background = new GUILayoutGroup(new RectTransform(Vector2.One, base.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			Point listSize = estimatedSize;
			if (hasHeader)
			{
				Point sz = Point.Zero;
				this.InflateSize(ref sz, header, headerFont);
				listSize.Y -= sz.Y;
				RectTransform rectT = new RectTransform(sz, background.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text = header;
				GUIFont font2 = headerFont;
				this.HeaderLabel = new GUITextBlock(rectT, text, null, font2, Alignment.Left, false, "", null)
				{
					Padding = headerPadding
				};
			}
			GUIListBox optionList = new GUIListBox(new RectTransform(listSize, background.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), false, null, null, true, false)
			{
				AutoHideScrollBar = false,
				ScrollBarVisible = false,
				Padding = (hasHeader ? new Vector4(4f, 0f, 4f, 4f) : padding),
				PlaySoundOnSelect = true
			};
			foreach (KeyValuePair<ContextMenuOption, Vector2> keyValuePair in optionsAndSizes)
			{
				ContextMenuOption contextMenuOption;
				Vector2 vector;
				keyValuePair.Deconstruct(out contextMenuOption, out vector);
				ContextMenuOption option2 = contextMenuOption;
				Vector2 size = vector;
				RectTransform rectT2 = new RectTransform(size.ToPoint(), optionList.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text2 = option2.Label;
				GUIFont font2 = font;
				GUITextBlock optionElement = new GUITextBlock(rectT2, text2, null, font2, Alignment.Left, false, "", null)
				{
					UserData = option2,
					Enabled = option2.IsEnabled
				};
				this.Options.Add(option2, optionElement);
				if (!option2.Tooltip.IsNullOrWhiteSpace() && optionElement.Enabled)
				{
					optionElement.ToolTip = option2.Tooltip;
				}
				if (option2.OnSelected == null)
				{
					optionElement.TextAlignment = Alignment.BottomLeft;
					optionElement.TextColor = (optionElement.DisabledTextColor = GUIStyle.Green);
				}
				else if (!option2.IsEnabled)
				{
					optionElement.TextColor *= 0.5f;
				}
			}
			List<GUIComponent> children = optionList.Content.Children.ToList<GUIComponent>();
			foreach (GUITextBlock block in (from c in children
			where c is GUITextBlock
			select c).Cast<GUITextBlock>())
			{
				object userData = block.UserData;
				if (userData is ContextMenuOption)
				{
					ContextMenuOption option3 = (ContextMenuOption)userData;
					bool flag = option3.OnSelected == null;
				}
				block.RectTransform.NonScaledSize = new Point((int)(block.TextSize.X + (block.Padding.X + block.Padding.Z)), (int)Math.Max(block.TextSize.Y * 1.2f, 18f * GUI.Scale));
			}
			int largestWidth = children.Max((GUIComponent c) => c.Rect.Width + horizontalPadding);
			if (this.HeaderLabel != null)
			{
				RectTransform headerTransform = this.HeaderLabel.RectTransform;
				headerTransform.MinSize = new Point((int)(this.HeaderLabel.TextSize.X + (headerPadding.X + headerPadding.Z)), headerTransform.NonScaledSize.Y);
				if (largestWidth < headerTransform.MinSize.X)
				{
					largestWidth = headerTransform.MinSize.X;
				}
			}
			foreach (GUIComponent c2 in children)
			{
				c2.RectTransform.MinSize = new Point(largestWidth, c2.Rect.Height);
			}
			Point newSize = new Point(largestWidth, children.Sum((GUIComponent c) => c.Rect.Height) + verticalPadding);
			base.RectTransform.NonScaledSize = new Point(newSize.X, (int)((float)newSize.Y / optionList.RectTransform.RelativeSize.Y));
			optionList.RectTransform.NonScaledSize = newSize;
			if (base.RectTransform.Rect.Bottom > GameMain.GraphicsHeight)
			{
				Rectangle rect = base.RectTransform.Rect;
				base.RectTransform.AbsoluteOffset = new Point(rect.X, rect.Y - rect.Height);
			}
			if (base.RectTransform.Rect.Right > GameMain.GraphicsWidth)
			{
				Rectangle rect2 = base.RectTransform.Rect;
				base.RectTransform.AbsoluteOffset = new Point(rect2.X - rect2.Width, rect2.Y);
			}
			background.Recalculate();
			optionList.OnSelected = new GUIListBox.OnSelectedHandler(this.OnSelected);
		}

		// Token: 0x06001284 RID: 4740 RVA: 0x000B60E0 File Offset: 0x000B42E0
		public static GUIContextMenu CreateContextMenu(params ContextMenuOption[] options)
		{
			return GUIContextMenu.CreateContextMenu(new Vector2?(PlayerInput.MousePosition), string.Empty, null, options);
		}

		// Token: 0x06001285 RID: 4741 RVA: 0x000B6110 File Offset: 0x000B4310
		public static GUIContextMenu CreateContextMenu(Vector2? pos, LocalizedString header, Color? headerColor, params ContextMenuOption[] options)
		{
			GUIContextMenu menu = new GUIContextMenu(pos, header, "GUIToolTip", options);
			if (headerColor != null)
			{
				GUITextBlock headerLabel = menu.HeaderLabel;
				if (headerLabel != null)
				{
					headerLabel.OverrideTextColor(headerColor.Value);
				}
			}
			GUIContextMenu.CurrentContextMenu = menu;
			return menu;
		}

		// Token: 0x06001286 RID: 4742 RVA: 0x000B6154 File Offset: 0x000B4354
		private bool OnSelected(GUIComponent _, object data)
		{
			if (data is ContextMenuOption)
			{
				ContextMenuOption option = (ContextMenuOption)data;
				if (option.IsEnabled)
				{
					GUIContextMenu.CurrentContextMenu = null;
					option.OnSelected();
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001287 RID: 4743 RVA: 0x000B618C File Offset: 0x000B438C
		private Vector2 InflateSize(ref Point size, LocalizedString label, ScalableFont font)
		{
			Vector2 textSize = font.MeasureString(label, false);
			size.X = Math.Max((int)Math.Ceiling((double)textSize.X), size.X);
			size.Y += (int)Math.Ceiling((double)textSize.Y);
			return textSize;
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x000B61D8 File Offset: 0x000B43D8
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.ParentOption != null)
			{
				this.ParentOption.State = GUIComponent.ComponentState.Hover;
			}
			if (this.SubMenu != null && !this.SubMenu.IsMouseOver())
			{
				this.SubMenu = null;
				return;
			}
			foreach (KeyValuePair<ContextMenuOption, GUITextBlock> keyValuePair in this.Options)
			{
				ContextMenuOption contextMenuOption;
				GUITextBlock guitextBlock;
				keyValuePair.Deconstruct(out contextMenuOption, out guitextBlock);
				ContextMenuOption option = contextMenuOption;
				GUITextBlock textBlock = guitextBlock;
				if (GUI.MouseOn == textBlock && option.IsEnabled)
				{
					ContextMenuOption[] subOptions = option.SubOptions;
					if (subOptions != null && subOptions.Any<ContextMenuOption>())
					{
						Vector2 subMenuPos = new Vector2((float)(textBlock.MouseRect.Right + 4), (float)textBlock.MouseRect.Y);
						this.SubMenu = new GUIContextMenu(new Vector2?(subMenuPos), "", "GUIToolTip", subOptions)
						{
							ParentOption = textBlock
						};
					}
				}
			}
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x000B62E8 File Offset: 0x000B44E8
		private bool IsMouseOver()
		{
			Rectangle expandedRect = this.Rect;
			expandedRect.Inflate(20, 20);
			bool isMouseOn = expandedRect.Contains(PlayerInput.MousePosition);
			if (this.ParentOption != null)
			{
				isMouseOn |= (GUI.MouseOn == this.ParentOption);
			}
			if (!isMouseOn && this.SubMenu != null)
			{
				isMouseOn = this.SubMenu.IsMouseOver();
			}
			return isMouseOn;
		}

		// Token: 0x0600128A RID: 4746 RVA: 0x000B6344 File Offset: 0x000B4544
		public override void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			base.AddToGUIUpdateList(ignoreChildren, order);
			GUIContextMenu subMenu = this.SubMenu;
			if (subMenu == null)
			{
				return;
			}
			subMenu.AddToGUIUpdateList(false, 2);
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x000B6360 File Offset: 0x000B4560
		public static void AddActiveToGUIUpdateList()
		{
			if (GUIContextMenu.CurrentContextMenu != null && !GUIContextMenu.CurrentContextMenu.IsMouseOver())
			{
				GUIContextMenu.CurrentContextMenu = null;
			}
			GUIContextMenu currentContextMenu = GUIContextMenu.CurrentContextMenu;
			if (currentContextMenu == null)
			{
				return;
			}
			currentContextMenu.AddToGUIUpdateList(false, 2);
		}

		// Token: 0x04000946 RID: 2374
		[Nullable(2)]
		public static GUIContextMenu CurrentContextMenu;

		// Token: 0x04000947 RID: 2375
		private readonly Dictionary<ContextMenuOption, GUITextBlock> Options = new Dictionary<ContextMenuOption, GUITextBlock>();

		// Token: 0x04000948 RID: 2376
		[Nullable(2)]
		private GUIContextMenu SubMenu;

		// Token: 0x04000949 RID: 2377
		[Nullable(2)]
		public readonly GUITextBlock HeaderLabel;

		// Token: 0x0400094A RID: 2378
		[Nullable(2)]
		public GUITextBlock ParentOption;
	}
}
