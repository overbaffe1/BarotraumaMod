using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A0 RID: 160
	[NullableContext(1)]
	[Nullable(0)]
	public class GUISelectionCarousel<[Nullable(2)] T> : GUIComponent, IGUISelectionCarouselAccessor
	{
		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x0600144E RID: 5198 RVA: 0x000C08FB File Offset: 0x000BEAFB
		// (set) Token: 0x0600144F RID: 5199 RVA: 0x000C0903 File Offset: 0x000BEB03
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public Func<T, bool> ElementSelectionCondition { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06001450 RID: 5200 RVA: 0x000C090C File Offset: 0x000BEB0C
		// (set) Token: 0x06001451 RID: 5201 RVA: 0x000C0914 File Offset: 0x000BEB14
		public GUITextBlock TextBlock { get; private set; }

		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x06001452 RID: 5202 RVA: 0x000C091D File Offset: 0x000BEB1D
		// (set) Token: 0x06001453 RID: 5203 RVA: 0x000C0925 File Offset: 0x000BEB25
		public GUIButton RightButton { get; private set; }

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x06001454 RID: 5204 RVA: 0x000C092E File Offset: 0x000BEB2E
		// (set) Token: 0x06001455 RID: 5205 RVA: 0x000C0936 File Offset: 0x000BEB36
		public GUIButton LeftButton { get; private set; }

		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x06001456 RID: 5206 RVA: 0x000C093F File Offset: 0x000BEB3F
		// (set) Token: 0x06001457 RID: 5207 RVA: 0x000C0947 File Offset: 0x000BEB47
		[Nullable(new byte[]
		{
			2,
			0
		})]
		public GUISelectionCarousel<T>.Element SelectedElement { [return: Nullable(new byte[]
		{
			2,
			0
		})] get; [param: Nullable(new byte[]
		{
			2,
			0
		})] private set; }

		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x06001458 RID: 5208 RVA: 0x000C0950 File Offset: 0x000BEB50
		[Nullable(2)]
		public T SelectedValue
		{
			[NullableContext(2)]
			get
			{
				if (!(this.SelectedElement == null))
				{
					return this.SelectedElement.value;
				}
				return default(T);
			}
		}

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x06001459 RID: 5209 RVA: 0x000C0980 File Offset: 0x000BEB80
		public LocalizedString SelectedText
		{
			get
			{
				GUISelectionCarousel<T>.Element selectedElement = this.SelectedElement;
				return ((selectedElement != null) ? selectedElement.text : null) ?? string.Empty;
			}
		}

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x0600145A RID: 5210 RVA: 0x000C09A2 File Offset: 0x000BEBA2
		// (set) Token: 0x0600145B RID: 5211 RVA: 0x000C09AC File Offset: 0x000BEBAC
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				GUIComponent rightButton = this.RightButton;
				GUIComponent leftButton = this.LeftButton;
				this.TextBlock.Enabled = value;
				leftButton.Enabled = value;
				rightButton.Enabled = value;
				base.Enabled = value;
			}
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x0600145C RID: 5212 RVA: 0x000C09EA File Offset: 0x000BEBEA
		// (set) Token: 0x0600145D RID: 5213 RVA: 0x000C09F2 File Offset: 0x000BEBF2
		public override Color Color
		{
			get
			{
				return this.color;
			}
			set
			{
				this.color = value;
				this.TextBlock.Color = this.color;
			}
		}

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x0600145E RID: 5214 RVA: 0x000C0A0C File Offset: 0x000BEC0C
		// (set) Token: 0x0600145F RID: 5215 RVA: 0x000C0A19 File Offset: 0x000BEC19
		public Color TextColor
		{
			get
			{
				return this.TextBlock.TextColor;
			}
			set
			{
				this.TextBlock.TextColor = value;
			}
		}

		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x06001460 RID: 5216 RVA: 0x000C0A27 File Offset: 0x000BEC27
		// (set) Token: 0x06001461 RID: 5217 RVA: 0x000C0A2F File Offset: 0x000BEC2F
		public override Color HoverColor
		{
			get
			{
				return base.HoverColor;
			}
			set
			{
				base.HoverColor = value;
				this.TextBlock.HoverColor = value;
			}
		}

		// Token: 0x06001462 RID: 5218 RVA: 0x000C0A44 File Offset: 0x000BEC44
		public GUISelectionCarousel(RectTransform rectT, string style = "", [TupleElementNames(new string[]
		{
			"value",
			"text"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<T, LocalizedString>[] newElements) : base(style, rectT)
		{
			this.layoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.05f,
				Stretch = true
			};
			this.LeftButton = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleLeft", null);
			GUIStyle.Apply(this.LeftButton, "LeftButton", this);
			this.TextBlock = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Center, false, "GUITextBox", null);
			GUIStyle.Apply(this.TextBlock, "TextBlock", this);
			this.RightButton = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleRight", null);
			GUIStyle.Apply(this.RightButton, "RightButton", this);
			GUIButton rightButton = this.RightButton;
			rightButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(rightButton.OnClicked, new GUIButton.OnClickedHandler((GUIButton _, object _) => this.SelectNextValidElement(false)));
			GUIButton leftButton = this.LeftButton;
			leftButton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(leftButton.OnClicked, new GUIButton.OnClickedHandler((GUIButton _, object _) => this.SelectNextValidElement(true)));
			if (newElements != null && newElements.Any<ValueTuple<T, LocalizedString>>())
			{
				this.SetElements(newElements);
			}
		}

		// Token: 0x06001463 RID: 5219 RVA: 0x000C0C58 File Offset: 0x000BEE58
		[NullableContext(2)]
		public object GetSelectedElement()
		{
			return this.SelectedValue;
		}

		// Token: 0x06001464 RID: 5220 RVA: 0x000C0C68 File Offset: 0x000BEE68
		[NullableContext(2)]
		public void SelectElement(object value)
		{
			if (value == null)
			{
				this.SelectElement(null);
				return;
			}
			GUISelectionCarousel<T>.Element matchingElement = (from e in this.elements
			where value.Equals(e.value)
			select e).FirstOrDefault((GUISelectionCarousel<T>.Element e) => this.ElementSelectionCondition == null || this.ElementSelectionCondition(e.value));
			if (matchingElement != null)
			{
				this.SelectElement(matchingElement);
			}
		}

		// Token: 0x06001465 RID: 5221 RVA: 0x000C0CD4 File Offset: 0x000BEED4
		public void SelectElement([Nullable(new byte[]
		{
			2,
			0
		})] GUISelectionCarousel<T>.Element element)
		{
			this.SelectedElement = element;
			this.TextBlock.Text = (((element != null) ? element.text : null) ?? string.Empty);
			this.TextBlock.ToolTip = (((element != null) ? element.toolTip : null) ?? string.Empty);
			GUISelectionCarousel<T>.OnValueChangedHandler onValueChanged = this.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged(this);
		}

		// Token: 0x06001466 RID: 5222 RVA: 0x000C0D50 File Offset: 0x000BEF50
		public void SetElements([TupleElementNames(new string[]
		{
			"value",
			"text"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<T, LocalizedString>[] elements)
		{
			this.elements.Clear();
			foreach (ValueTuple<T, LocalizedString> valueTuple in elements)
			{
				T value = valueTuple.Item1;
				LocalizedString text = valueTuple.Item2;
				this.AddElement(value, text, null);
			}
		}

		// Token: 0x06001467 RID: 5223 RVA: 0x000C0D98 File Offset: 0x000BEF98
		public void SetElements([TupleElementNames(new string[]
		{
			"value",
			"text",
			"toolTip"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1,
			1
		})] params ValueTuple<T, LocalizedString, LocalizedString>[] elements)
		{
			this.elements.Clear();
			foreach (ValueTuple<T, LocalizedString, LocalizedString> valueTuple in elements)
			{
				T value = valueTuple.Item1;
				LocalizedString text = valueTuple.Item2;
				LocalizedString toolTip = valueTuple.Item3;
				this.AddElement(value, text, toolTip);
			}
		}

		// Token: 0x06001468 RID: 5224 RVA: 0x000C0DE8 File Offset: 0x000BEFE8
		public void AddElement(T value, LocalizedString text, [Nullable(2)] LocalizedString tooltip = null)
		{
			GUISelectionCarousel<T>.Element newElement = new GUISelectionCarousel<T>.Element(value, text, tooltip ?? string.Empty);
			this.elements.Add(newElement);
			if (this.SelectedElement == null)
			{
				this.SelectElement(newElement);
			}
		}

		// Token: 0x06001469 RID: 5225 RVA: 0x000C0E30 File Offset: 0x000BF030
		public void Refresh()
		{
			if (this.SelectedElement != null && (this.ElementSelectionCondition == null || this.ElementSelectionCondition(this.SelectedElement.value)))
			{
				return;
			}
			this.SelectElement(this.elements.FirstOrDefault((GUISelectionCarousel<T>.Element e) => this.ElementSelectionCondition == null || this.ElementSelectionCondition(e.value)));
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x000C0E8C File Offset: 0x000BF08C
		private bool SelectNextValidElement(bool directionLeft = false)
		{
			if (this.elements.Count < 2)
			{
				return false;
			}
			int currentIndex = (this.SelectedElement == null) ? -1 : this.elements.IndexOf(this.SelectedElement);
			int newIndex = currentIndex;
			for (int i = 0; i < this.elements.Count; i++)
			{
				newIndex = (directionLeft ? MathUtils.PositiveModulo(newIndex - 1, this.elements.Count) : ((newIndex + 1) % this.elements.Count));
				if (this.ElementSelectionCondition == null || this.ElementSelectionCondition(this.elements[newIndex].value))
				{
					this.SelectElement(this.elements[newIndex]);
					return true;
				}
			}
			this.SelectElement(null);
			return true;
		}

		// Token: 0x04000A03 RID: 2563
		[Nullable(new byte[]
		{
			2,
			0
		})]
		public GUISelectionCarousel<T>.OnValueChangedHandler OnValueChanged;

		// Token: 0x04000A08 RID: 2568
		[Nullable(new byte[]
		{
			1,
			1,
			0
		})]
		private readonly List<GUISelectionCarousel<T>.Element> elements = new List<GUISelectionCarousel<T>.Element>();

		// Token: 0x04000A09 RID: 2569
		private readonly GUILayoutGroup layoutGroup;

		// Token: 0x02000977 RID: 2423
		[Nullable(0)]
		public class Element : IEquatable<GUISelectionCarousel<T>.Element>
		{
			// Token: 0x060071E4 RID: 29156 RVA: 0x0036BDC0 File Offset: 0x00369FC0
			public Element(T value, LocalizedString text, LocalizedString toolTip)
			{
				this.value = value;
				this.text = text;
				this.toolTip = toolTip;
				base..ctor();
			}

			// Token: 0x17001A58 RID: 6744
			// (get) Token: 0x060071E5 RID: 29157 RVA: 0x0036BDDD File Offset: 0x00369FDD
			[CompilerGenerated]
			protected virtual Type EqualityContract
			{
				[CompilerGenerated]
				get
				{
					return typeof(GUISelectionCarousel<T>.Element);
				}
			}

			// Token: 0x17001A59 RID: 6745
			// (get) Token: 0x060071E6 RID: 29158 RVA: 0x0036BDE9 File Offset: 0x00369FE9
			// (set) Token: 0x060071E7 RID: 29159 RVA: 0x0036BDF1 File Offset: 0x00369FF1
			public T value { get; set; }

			// Token: 0x17001A5A RID: 6746
			// (get) Token: 0x060071E8 RID: 29160 RVA: 0x0036BDFA File Offset: 0x00369FFA
			// (set) Token: 0x060071E9 RID: 29161 RVA: 0x0036BE02 File Offset: 0x0036A002
			public LocalizedString text { get; set; }

			// Token: 0x17001A5B RID: 6747
			// (get) Token: 0x060071EA RID: 29162 RVA: 0x0036BE0B File Offset: 0x0036A00B
			// (set) Token: 0x060071EB RID: 29163 RVA: 0x0036BE13 File Offset: 0x0036A013
			public LocalizedString toolTip { get; set; }

			// Token: 0x060071EC RID: 29164 RVA: 0x0036BE1C File Offset: 0x0036A01C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Element");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060071ED RID: 29165 RVA: 0x0036BE68 File Offset: 0x0036A068
			[CompilerGenerated]
			protected virtual bool PrintMembers(StringBuilder builder)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
				builder.Append("value = ");
				builder.Append(this.value);
				builder.Append(", text = ");
				builder.Append(this.text);
				builder.Append(", toolTip = ");
				builder.Append(this.toolTip);
				return true;
			}

			// Token: 0x060071EE RID: 29166 RVA: 0x0036BECB File Offset: 0x0036A0CB
			[CompilerGenerated]
			public static bool operator !=([Nullable(new byte[]
			{
				2,
				0
			})] GUISelectionCarousel<T>.Element left, [Nullable(new byte[]
			{
				2,
				0
			})] GUISelectionCarousel<T>.Element right)
			{
				return !(left == right);
			}

			// Token: 0x060071EF RID: 29167 RVA: 0x0036BED7 File Offset: 0x0036A0D7
			[CompilerGenerated]
			public static bool operator ==([Nullable(new byte[]
			{
				2,
				0
			})] GUISelectionCarousel<T>.Element left, [Nullable(new byte[]
			{
				2,
				0
			})] GUISelectionCarousel<T>.Element right)
			{
				return left == right || (left != null && left.Equals(right));
			}

			// Token: 0x060071F0 RID: 29168 RVA: 0x0036BEEC File Offset: 0x0036A0EC
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<T>.Default.GetHashCode(this.<value>k__BackingField)) * -1521134295 + EqualityComparer<LocalizedString>.Default.GetHashCode(this.<text>k__BackingField)) * -1521134295 + EqualityComparer<LocalizedString>.Default.GetHashCode(this.<toolTip>k__BackingField);
			}

			// Token: 0x060071F1 RID: 29169 RVA: 0x0036BF4E File Offset: 0x0036A14E
			[NullableContext(2)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return this.Equals(obj as GUISelectionCarousel<T>.Element);
			}

			// Token: 0x060071F2 RID: 29170 RVA: 0x0036BF5C File Offset: 0x0036A15C
			[CompilerGenerated]
			public virtual bool Equals([Nullable(new byte[]
			{
				2,
				0
			})] GUISelectionCarousel<T>.Element other)
			{
				return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<T>.Default.Equals(this.<value>k__BackingField, other.<value>k__BackingField) && EqualityComparer<LocalizedString>.Default.Equals(this.<text>k__BackingField, other.<text>k__BackingField) && EqualityComparer<LocalizedString>.Default.Equals(this.<toolTip>k__BackingField, other.<toolTip>k__BackingField));
			}

			// Token: 0x060071F4 RID: 29172 RVA: 0x0036BFD5 File Offset: 0x0036A1D5
			[CompilerGenerated]
			protected Element([Nullable(new byte[]
			{
				1,
				0
			})] GUISelectionCarousel<T>.Element original)
			{
				this.value = original.<value>k__BackingField;
				this.text = original.<text>k__BackingField;
				this.toolTip = original.<toolTip>k__BackingField;
			}

			// Token: 0x060071F5 RID: 29173 RVA: 0x0036C001 File Offset: 0x0036A201
			[CompilerGenerated]
			public void Deconstruct(out T value, out LocalizedString text, out LocalizedString toolTip)
			{
				value = this.value;
				text = this.text;
				toolTip = this.toolTip;
			}
		}

		// Token: 0x02000978 RID: 2424
		// (Invoke) Token: 0x060071F7 RID: 29175
		[NullableContext(0)]
		public delegate void OnValueChangedHandler(GUISelectionCarousel<T> carousel);
	}
}
