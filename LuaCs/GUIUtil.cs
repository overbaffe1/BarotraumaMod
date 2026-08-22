using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E2 RID: 1250
	[NullableContext(1)]
	[Nullable(0)]
	public static class GUIUtil
	{
		// Token: 0x06005175 RID: 20853 RVA: 0x002BD364 File Offset: 0x002BB564
		[return: TupleElementNames(new string[]
		{
			"Left",
			"Right"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static ValueTuple<GUILayoutGroup, GUILayoutGroup> CreateSidebars(GUIFrame parent, bool split = false)
		{
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup left = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame centerFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.025f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (split)
			{
				new GUICustomComponent(new RectTransform(Vector2.One, centerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
				{
					sb.DrawLine(new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Top), new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Bottom), GUIStyle.TextColorDim, 2f);
				}, null);
			}
			GUILayoutGroup right = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			return new ValueTuple<GUILayoutGroup, GUILayoutGroup>(left, right);
		}

		// Token: 0x06005176 RID: 20854 RVA: 0x002BD4E8 File Offset: 0x002BB6E8
		[return: TupleElementNames(new string[]
		{
			"Left",
			"Right"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static ValueTuple<GUILayoutGroup, GUILayoutGroup> CreateSidebars(GUILayoutGroup parent, bool split = false)
		{
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup left = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUIFrame centerFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.025f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			if (split)
			{
				new GUICustomComponent(new RectTransform(Vector2.One, centerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
				{
					sb.DrawLine(new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Top), new ValueTuple<float, float>((float)c.Rect.Center.X, (float)c.Rect.Bottom), GUIStyle.TextColorDim, 2f);
				}, null);
			}
			GUILayoutGroup right = new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.4875f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			return new ValueTuple<GUILayoutGroup, GUILayoutGroup>(left, right);
		}

		// Token: 0x06005177 RID: 20855 RVA: 0x002BD66C File Offset: 0x002BB86C
		public static GUILayoutGroup CreateCenterLayout(GUIFrame parent)
		{
			return new GUILayoutGroup(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), parent.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				ChildAnchor = Anchor.TopCenter
			};
		}

		// Token: 0x06005178 RID: 20856 RVA: 0x002BD6C0 File Offset: 0x002BB8C0
		public static RectTransform NewItemRectT(GUILayoutGroup parent, Vector2 adjustRatio)
		{
			return new RectTransform(new ValueTuple<float, float>(1f * adjustRatio.X, 0.06f * adjustRatio.Y), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
		}

		// Token: 0x06005179 RID: 20857 RVA: 0x002BD718 File Offset: 0x002BB918
		public static void Spacer(GUILayoutGroup parent, Vector2 adjustRatio)
		{
			new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f * adjustRatio.X, 0.03f * adjustRatio.Y), parent.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null);
		}

		// Token: 0x0600517A RID: 20858 RVA: 0x002BD780 File Offset: 0x002BB980
		public static void ClearChildElements(GUIComponent component, bool clearSelfFromParent = false)
		{
			component.GetAllChildren().ForEachMod(delegate(GUIComponent c)
			{
				c.Visible = false;
				component.RemoveChild(c);
			});
			if (clearSelfFromParent && component.Parent != null)
			{
				component.Parent.RemoveChild(component);
			}
		}

		// Token: 0x0600517B RID: 20859 RVA: 0x002BD7DC File Offset: 0x002BB9DC
		public static GUITextBlock Label(GUILayoutGroup parent, LocalizedString str, GUIFont font, Vector2 adjustRatio)
		{
			return new GUITextBlock(GUIUtil.NewItemRectT(parent, adjustRatio), str, null, font, Alignment.Left, false, "", null);
		}

		// Token: 0x0600517C RID: 20860 RVA: 0x002BD817 File Offset: 0x002BBA17
		public static GUIDropDown DropdownEnum<[Nullable(0)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<T, LocalizedString> tooltipFunc, T currentValue, Action<T> setter, Vector2 adjustRatio) where T : Enum
		{
			return GUIUtil.Dropdown<T>(parent, textFunc, tooltipFunc, (T[])Enum.GetValues(typeof(T)), currentValue, setter, adjustRatio, 1f);
		}

		// Token: 0x0600517D RID: 20861 RVA: 0x002BD840 File Offset: 0x002BBA40
		public static GUIDropDown Dropdown<[Nullable(2)] T>(GUILayoutGroup parent, Func<T, LocalizedString> textFunc, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<T, LocalizedString> tooltipFunc, IReadOnlyList<T> values, T currentValue, Action<T> setter, Vector2 adjustRatio, float listBoxScale = 1f)
		{
			GUIDropDown dropdown = new GUIDropDown(GUIUtil.NewItemRectT(parent, adjustRatio), null, 4, "", false, false, Alignment.CenterLeft, listBoxScale);
			values.ForEach(delegate(T v)
			{
				GUIDropDown dropdown = dropdown;
				LocalizedString text = textFunc(v);
				object userData = v;
				Func<T, LocalizedString> tooltipFunc2 = tooltipFunc;
				dropdown.AddItem(text, userData, ((tooltipFunc2 != null) ? tooltipFunc2(v) : null) ?? null, null, null);
			});
			int childIndex = values.IndexOf(currentValue);
			dropdown.Select(childIndex);
			dropdown.ListBox.ForceLayoutRecalculation();
			dropdown.ListBox.ScrollToElement(dropdown.ListBox.Content.GetChild(childIndex), GUIListBox.PlaySelectSound.No);
			dropdown.OnSelected = delegate(GUIComponent dd, object obj)
			{
				setter((T)((object)obj));
				return true;
			};
			return dropdown;
		}

		// Token: 0x0600517E RID: 20862 RVA: 0x002BD904 File Offset: 0x002BBB04
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public static ValueTuple<GUIScrollBar, GUITextBlock> Slider(GUILayoutGroup parent, Vector2 range, int steps, Func<float, string> labelFunc, float currentValue, Action<float> setter, [Nullable(2)] LocalizedString tooltip, Vector2 adjustRatio)
		{
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(adjustRatio, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIScrollBar slider = new GUIScrollBar(new RectTransform(new ValueTuple<float, float>(0.72f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 1f, null, "GUISlider", null)
			{
				Range = range,
				BarScrollValue = currentValue,
				Step = 1f / (float)(steps - 1),
				BarSize = 1f / (float)steps
			};
			if (tooltip != null)
			{
				slider.ToolTip = tooltip;
			}
			GUITextBlock label = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(0.28f, 1f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), labelFunc(currentValue), null, null, Alignment.Center, false, "", null);
			slider.OnMoved = delegate(GUIScrollBar sb, float val)
			{
				label.Text = labelFunc(sb.BarScrollValue);
				setter(sb.BarScrollValue);
				return true;
			};
			return new ValueTuple<GUIScrollBar, GUITextBlock>(slider, label);
		}

		// Token: 0x0600517F RID: 20863 RVA: 0x002BDA98 File Offset: 0x002BBC98
		public static GUITickBox Tickbox(GUILayoutGroup parent, LocalizedString label, LocalizedString tooltip, bool currentValue, Action<bool> setter, Vector2 adjustRatio)
		{
			return new GUITickBox(GUIUtil.NewItemRectT(parent, adjustRatio), label, null, "")
			{
				Selected = currentValue,
				ToolTip = tooltip,
				OnSelected = delegate(GUITickBox tb)
				{
					setter(tb.Selected);
					return true;
				}
			};
		}

		// Token: 0x06005180 RID: 20864 RVA: 0x002BDAEE File Offset: 0x002BBCEE
		public static string Percentage(float v)
		{
			return ToolBox.GetFormattedPercentage(v);
		}

		// Token: 0x06005181 RID: 20865 RVA: 0x002BDAF6 File Offset: 0x002BBCF6
		public static int Round(float v)
		{
			return (int)MathF.Round(v);
		}
	}
}
