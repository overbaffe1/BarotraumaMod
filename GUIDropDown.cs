using System;
using System.Collections.Generic;
using System.Linq;
using EventInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000087 RID: 135
	public class GUIDropDown : GUIComponent, IKeyboardSubscriber
	{
		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x06001292 RID: 4754 RVA: 0x000B676C File Offset: 0x000B496C
		public GUIButton Button
		{
			get
			{
				return this.button;
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x06001293 RID: 4755 RVA: 0x000B6774 File Offset: 0x000B4974
		// (set) Token: 0x06001294 RID: 4756 RVA: 0x000B677C File Offset: 0x000B497C
		public bool Dropped { get; set; }

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x06001295 RID: 4757 RVA: 0x000B6785 File Offset: 0x000B4985
		// (set) Token: 0x06001296 RID: 4758 RVA: 0x000B678D File Offset: 0x000B498D
		public bool AllowNonText { get; set; }

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x06001297 RID: 4759 RVA: 0x000B6796 File Offset: 0x000B4996
		public object SelectedItemData
		{
			get
			{
				if (this.listBox.SelectedComponent == null)
				{
					return null;
				}
				return this.listBox.SelectedComponent.UserData;
			}
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x06001298 RID: 4760 RVA: 0x000B67B7 File Offset: 0x000B49B7
		// (set) Token: 0x06001299 RID: 4761 RVA: 0x000B67C4 File Offset: 0x000B49C4
		public override bool Enabled
		{
			get
			{
				return this.listBox.Enabled;
			}
			set
			{
				this.listBox.Enabled = value;
			}
		}

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x0600129A RID: 4762 RVA: 0x000B67D2 File Offset: 0x000B49D2
		// (set) Token: 0x0600129B RID: 4763 RVA: 0x000B67DF File Offset: 0x000B49DF
		public bool ButtonEnabled
		{
			get
			{
				return this.button.Enabled;
			}
			set
			{
				this.button.Enabled = value;
				if (this.icon != null)
				{
					this.icon.Enabled = value;
				}
			}
		}

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x0600129C RID: 4764 RVA: 0x000B6801 File Offset: 0x000B4A01
		public GUIComponent SelectedComponent
		{
			get
			{
				return this.listBox.SelectedComponent;
			}
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x0600129D RID: 4765 RVA: 0x000B680E File Offset: 0x000B4A0E
		// (set) Token: 0x0600129E RID: 4766 RVA: 0x000B6816 File Offset: 0x000B4A16
		public override bool Selected
		{
			get
			{
				return this.Dropped;
			}
			set
			{
				this.Dropped = value;
			}
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x0600129F RID: 4767 RVA: 0x000B681F File Offset: 0x000B4A1F
		public GUIListBox ListBox
		{
			get
			{
				return this.listBox;
			}
		}

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x060012A0 RID: 4768 RVA: 0x000B6827 File Offset: 0x000B4A27
		public object SelectedData
		{
			get
			{
				GUIComponent selectedComponent = this.listBox.SelectedComponent;
				if (selectedComponent == null)
				{
					return null;
				}
				return selectedComponent.UserData;
			}
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x060012A1 RID: 4769 RVA: 0x000B683F File Offset: 0x000B4A3F
		public int SelectedIndex
		{
			get
			{
				if (this.listBox.SelectedComponent == null)
				{
					return -1;
				}
				return this.listBox.Content.GetChildIndex(this.listBox.SelectedComponent);
			}
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x060012A2 RID: 4770 RVA: 0x000B686B File Offset: 0x000B4A6B
		// (set) Token: 0x060012A3 RID: 4771 RVA: 0x000B6878 File Offset: 0x000B4A78
		public Color ButtonTextColor
		{
			get
			{
				return this.button.TextColor;
			}
			set
			{
				this.button.TextColor = value;
			}
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x060012A4 RID: 4772 RVA: 0x000B6886 File Offset: 0x000B4A86
		// (set) Token: 0x060012A5 RID: 4773 RVA: 0x000B68A4 File Offset: 0x000B4AA4
		public override GUIFont Font
		{
			get
			{
				GUIButton guibutton = this.button;
				return ((guibutton != null) ? guibutton.Font : null) ?? base.Font;
			}
			set
			{
				if (this.button != null)
				{
					this.button.Font = value;
				}
			}
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x000B68BA File Offset: 0x000B4ABA
		public void ReceiveTextInput(char inputChar)
		{
			GUI.KeyboardDispatcher.Subscriber = null;
		}

		// Token: 0x060012A7 RID: 4775 RVA: 0x000B68C7 File Offset: 0x000B4AC7
		public void ReceiveTextInput(string text)
		{
		}

		// Token: 0x060012A8 RID: 4776 RVA: 0x000B68C9 File Offset: 0x000B4AC9
		public void ReceiveCommandInput(char command)
		{
		}

		// Token: 0x060012A9 RID: 4777 RVA: 0x000B68CB File Offset: 0x000B4ACB
		public void ReceiveEditingInput(string text, int start, int length)
		{
		}

		// Token: 0x060012AA RID: 4778 RVA: 0x000B68CD File Offset: 0x000B4ACD
		public void ReceiveSpecialInput(Keys key)
		{
			if (key == Keys.Up || key == Keys.Down)
			{
				this.listBox.ReceiveSpecialInput(key);
				GUI.KeyboardDispatcher.Subscriber = this;
				return;
			}
			GUI.KeyboardDispatcher.Subscriber = null;
		}

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x060012AB RID: 4779 RVA: 0x000B68FC File Offset: 0x000B4AFC
		public IEnumerable<object> SelectedDataMultiple
		{
			get
			{
				return this.selectedDataMultiple;
			}
		}

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x060012AC RID: 4780 RVA: 0x000B6904 File Offset: 0x000B4B04
		public IEnumerable<int> SelectedIndexMultiple
		{
			get
			{
				return this.selectedIndexMultiple;
			}
		}

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x060012AD RID: 4781 RVA: 0x000B690C File Offset: 0x000B4B0C
		// (set) Token: 0x060012AE RID: 4782 RVA: 0x000B6919 File Offset: 0x000B4B19
		public LocalizedString Text
		{
			get
			{
				return this.button.Text;
			}
			set
			{
				this.button.Text = value;
			}
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x060012AF RID: 4783 RVA: 0x000B6927 File Offset: 0x000B4B27
		// (set) Token: 0x060012B0 RID: 4784 RVA: 0x000B692F File Offset: 0x000B4B2F
		public override RichString ToolTip
		{
			get
			{
				return base.ToolTip;
			}
			set
			{
				base.ToolTip = value;
				this.button.ToolTip = value;
				this.listBox.ToolTip = value;
			}
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x060012B1 RID: 4785 RVA: 0x000B6950 File Offset: 0x000B4B50
		public GUIImage DropDownIcon
		{
			get
			{
				return this.icon;
			}
		}

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x060012B2 RID: 4786 RVA: 0x000B6958 File Offset: 0x000B4B58
		public Vector4 Padding
		{
			get
			{
				return this.button.TextBlock.Padding;
			}
		}

		// Token: 0x060012B3 RID: 4787 RVA: 0x000B696C File Offset: 0x000B4B6C
		public GUIDropDown(RectTransform rectT, LocalizedString text = null, int elementCount = 4, string style = "", bool selectMultiple = false, bool dropAbove = false, Alignment textAlignment = Alignment.CenterLeft, float listBoxScale = 1f) : base(style, rectT)
		{
			if (text == null)
			{
				text = LocalizedString.EmptyString;
			}
			this.HoverCursor = CursorState.Hand;
			this.CanBeFocused = true;
			this.selectMultiple = selectMultiple;
			this.button = new GUIButton(new RectTransform(Vector2.One, rectT, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text, textAlignment, "GUIDropDown", null)
			{
				OnClicked = new GUIButton.OnClickedHandler(this.OnClicked),
				TextBlock = 
				{
					OverflowClip = true
				}
			};
			GUIStyle.Apply(this.button, "", this);
			this.button.TextBlock.SetTextPos();
			Anchor listAnchor = dropAbove ? Anchor.TopCenter : Anchor.BottomCenter;
			Pivot listPivot = dropAbove ? Pivot.BottomCenter : Pivot.TopCenter;
			this.listBox = new GUIListBox(new RectTransform(new Point((int)((float)this.Rect.Width * listBoxScale), this.Rect.Height * MathHelper.Clamp(elementCount, 2, 10)), rectT, listAnchor, new Pivot?(listPivot), ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, false, null, null, true, false)
			{
				Enabled = !selectMultiple,
				PlaySoundOnSelect = true
			};
			if (!selectMultiple)
			{
				this.listBox.AfterSelected = delegate(GUIComponent component, object obj)
				{
					this.SelectItem(component, obj);
					GUIDropDown.OnSelectedHandler afterSelected = this.AfterSelected;
					if (afterSelected != null)
					{
						afterSelected(component, obj);
					}
					return true;
				};
			}
			GUIStyle.Apply(this.listBox, "GUIListBox", this);
			GUIStyle.Apply(this.listBox.ContentBackground, "GUIListBox", this);
			if (this.button.Style.ChildStyles.ContainsKey("dropdownicon".ToIdentifier()))
			{
				this.icon = new GUIImage(new RectTransform(new Vector2(0.6f, 0.6f), this.button.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point(5, 0)
				}, null, true);
				this.icon.ApplyStyle(this.button.Style.ChildStyles["dropdownicon".ToIdentifier()]);
				this.button.TextBlock.Padding += new Vector4(0f, 0f, (float)this.icon.Rect.Width, 0f);
			}
			this.currentHighestParent = this.FindHighestParent();
			GUIComponent guicomponent = this.currentHighestParent.GUIComponent;
			guicomponent.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guicomponent.OnAddedToGUIUpdateList, new Action<GUIComponent>(this.AddListBoxToGUIUpdateList));
			rectT.ParentChanged += delegate(RectTransform _)
			{
				this.RefreshListBoxParent();
			};
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x000B6C34 File Offset: 0x000B4E34
		private RectTransform FindHighestParent()
		{
			this.parentHierarchy.Clear();
			this.parentHierarchy = new List<RectTransform>
			{
				base.RectTransform.Parent
			};
			RectTransform parent = this.parentHierarchy.Last<RectTransform>();
			while (((parent != null) ? parent.Parent : null) != null)
			{
				this.parentHierarchy.Add(parent.Parent);
				parent = parent.Parent;
			}
			for (int i = this.parentHierarchy.Count - 1; i > 0; i--)
			{
				if (!(this.parentHierarchy[i] is GUICanvas) && this.parentHierarchy[i].GUIComponent != null && this.parentHierarchy[i].GUIComponent.Style != null)
				{
					GUIFrame guicomponent = this.parentHierarchy[i].GUIComponent;
					Screen selected = Screen.Selected;
					if (guicomponent != ((selected != null) ? selected.Frame : null))
					{
						break;
					}
				}
				this.parentHierarchy.RemoveAt(i);
			}
			return this.parentHierarchy.Last<RectTransform>();
		}

		// Token: 0x060012B5 RID: 4789 RVA: 0x000B6D30 File Offset: 0x000B4F30
		public GUIComponent AddItem(LocalizedString text, object userData = null, LocalizedString toolTip = null, Color? color = null, Color? textColor = null)
		{
			if (toolTip == null)
			{
				toolTip = "";
			}
			if (this.selectMultiple)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.listBox.Content.Rect.Width, this.button.Rect.Height), this.listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					IsFixedSize = false
				}, "ListBoxElement", color)
				{
					UserData = userData,
					ToolTip = toolTip
				};
				GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.8f), frame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
				{
					MaxSize = new Point(int.MaxValue, (int)((float)this.button.Rect.Height * 0.8f))
				}, text, null, "");
				guitickBox.UserData = userData;
				guitickBox.ToolTip = toolTip;
				guitickBox.OnSelected = delegate(GUITickBox tb)
				{
					if (this.MustSelectAtLeastOne && this.selectedIndexMultiple.Count <= 1 && !tb.Selected)
					{
						tb.Selected = true;
						return false;
					}
					if (this.OnSelected != null && !this.OnSelected(tb.Parent, tb.Parent.UserData))
					{
						return false;
					}
					List<LocalizedString> texts = new List<LocalizedString>();
					this.selectedDataMultiple.Clear();
					this.selectedIndexMultiple.Clear();
					int i = 0;
					foreach (GUIComponent child in this.ListBox.Content.Children)
					{
						GUITickBox tickBox = child.GetChild<GUITickBox>();
						if (tickBox != null && tickBox.Selected)
						{
							this.selectedDataMultiple.Add(child.UserData);
							this.selectedIndexMultiple.Add(i);
							texts.Add(tickBox.Text);
						}
						i++;
					}
					this.button.Text = LocalizedString.Join(", ", texts);
					GUIDropDown.OnSelectedHandler afterSelected = this.AfterSelected;
					if (afterSelected != null)
					{
						afterSelected(tb.Parent, this.SelectedData);
					}
					return true;
				};
				return frame;
			}
			return new GUITextBlock(new RectTransform(new Point(this.listBox.Content.Rect.Width, this.button.Rect.Height), this.listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, text, textColor, null, Alignment.Left, false, "ListBoxElement", color)
			{
				UserData = userData,
				ToolTip = toolTip
			};
		}

		// Token: 0x060012B6 RID: 4790 RVA: 0x000B6EDC File Offset: 0x000B50DC
		public override void ClearChildren()
		{
			this.listBox.ClearChildren();
		}

		// Token: 0x060012B7 RID: 4791 RVA: 0x000B6EE9 File Offset: 0x000B50E9
		public IEnumerable<GUIComponent> GetChildren()
		{
			return this.listBox.Content.Children;
		}

		// Token: 0x060012B8 RID: 4792 RVA: 0x000B6EFC File Offset: 0x000B50FC
		private bool SelectItem(GUIComponent component, object obj)
		{
			if (this.selectMultiple)
			{
				using (IEnumerator<GUIComponent> enumerator = this.ListBox.Content.Children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GUIComponent child = enumerator.Current;
						GUITickBox tickBox = child.GetChild<GUITickBox>();
						if (object.Equals(obj, child.UserData))
						{
							tickBox.Selected = true;
						}
					}
					goto IL_9F;
				}
			}
			GUITextBlock textBlock = component as GUITextBlock;
			if (textBlock == null)
			{
				textBlock = component.GetChild<GUITextBlock>();
				if (textBlock == null && !this.AllowNonText)
				{
					return false;
				}
			}
			this.button.Text = (((textBlock != null) ? textBlock.Text : null) ?? "");
			IL_9F:
			GUIDropDown.OnSelectedHandler onSelected = this.OnSelected;
			if (onSelected != null)
			{
				onSelected(component, obj);
			}
			this.Dropped = false;
			return true;
		}

		// Token: 0x060012B9 RID: 4793 RVA: 0x000B6FD4 File Offset: 0x000B51D4
		public void SelectItem(object userData)
		{
			if (this.selectMultiple)
			{
				this.SelectItem(this.listBox.Content.FindChild(userData, false), userData);
			}
			else
			{
				this.listBox.Select(userData, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
			}
			GUIDropDown.OnSelectedHandler afterSelected = this.AfterSelected;
			if (afterSelected == null)
			{
				return;
			}
			afterSelected(this.SelectedComponent, this.SelectedData);
		}

		// Token: 0x060012BA RID: 4794 RVA: 0x000B7034 File Offset: 0x000B5234
		public void Select(int index)
		{
			if (this.selectMultiple)
			{
				GUIComponent child = this.listBox.Content.GetChild(index);
				if (child != null)
				{
					this.SelectItem(null, child.UserData);
				}
			}
			else
			{
				this.listBox.Select(index, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
			}
			GUIDropDown.OnSelectedHandler afterSelected = this.AfterSelected;
			if (afterSelected == null)
			{
				return;
			}
			afterSelected(this, this.SelectedData);
		}

		// Token: 0x060012BB RID: 4795 RVA: 0x000B7098 File Offset: 0x000B5298
		private bool OnClicked(GUIComponent component, object obj)
		{
			if (this.wasOpened)
			{
				return false;
			}
			this.wasOpened = true;
			this.Dropped = !this.Dropped;
			if (this.Dropped && this.Enabled)
			{
				GUIDropDown.OnSelectedHandler onDropped = this.OnDropped;
				if (onDropped != null)
				{
					onDropped(this, this.UserData);
				}
				this.listBox.UpdateScrollBarSize();
				this.listBox.UpdateDimensions();
				GUI.KeyboardDispatcher.Subscriber = this;
			}
			else if (GUI.KeyboardDispatcher.Subscriber == this)
			{
				GUI.KeyboardDispatcher.Subscriber = null;
			}
			return true;
		}

		// Token: 0x060012BC RID: 4796 RVA: 0x000B712C File Offset: 0x000B532C
		public void RefreshListBoxParent()
		{
			GUIComponent guicomponent = this.currentHighestParent.GUIComponent;
			guicomponent.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Remove(guicomponent.OnAddedToGUIUpdateList, new Action<GUIComponent>(this.AddListBoxToGUIUpdateList));
			if (base.RectTransform.Parent == null)
			{
				return;
			}
			this.currentHighestParent = this.FindHighestParent();
			GUIComponent guicomponent2 = this.currentHighestParent.GUIComponent;
			guicomponent2.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(guicomponent2.OnAddedToGUIUpdateList, new Action<GUIComponent>(this.AddListBoxToGUIUpdateList));
		}

		// Token: 0x060012BD RID: 4797 RVA: 0x000B71AC File Offset: 0x000B53AC
		private void AddListBoxToGUIUpdateList(GUIComponent parent)
		{
			for (int i = 1; i < this.parentHierarchy.Count; i++)
			{
				if (!this.parentHierarchy[i].IsParentOf(this.parentHierarchy[i - 1], false))
				{
					parent.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Remove(parent.OnAddedToGUIUpdateList, new Action<GUIComponent>(this.AddListBoxToGUIUpdateList));
					return;
				}
			}
			if (this.Dropped)
			{
				this.listBox.AddToGUIUpdateList(false, 1);
			}
		}

		// Token: 0x060012BE RID: 4798 RVA: 0x000B7229 File Offset: 0x000B5429
		public override void DrawManually(SpriteBatch spriteBatch, bool alsoChildren = false, bool recursive = true)
		{
			if (!base.Visible)
			{
				return;
			}
			base.AutoDraw = false;
			this.Draw(spriteBatch);
			if (alsoChildren)
			{
				this.button.DrawManually(spriteBatch, alsoChildren, recursive);
			}
		}

		// Token: 0x060012BF RID: 4799 RVA: 0x000B7253 File Offset: 0x000B5453
		public override void AddToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			base.AddToGUIUpdateList(true, order);
			if (!ignoreChildren)
			{
				this.button.AddToGUIUpdateList(false, order);
			}
		}

		// Token: 0x060012C0 RID: 4800 RVA: 0x000B7270 File Offset: 0x000B5470
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			this.wasOpened = false;
			base.Update(deltaTime);
			if (this.Dropped && PlayerInput.PrimaryMouseButtonClicked() && !this.listBox.Rect.Contains(PlayerInput.MousePosition) && !this.button.Rect.Contains(PlayerInput.MousePosition))
			{
				this.Dropped = false;
				if (GUI.KeyboardDispatcher.Subscriber == this)
				{
					GUI.KeyboardDispatcher.Subscriber = null;
				}
			}
		}

		// Token: 0x04000954 RID: 2388
		public GUIDropDown.OnSelectedHandler OnSelected;

		// Token: 0x04000955 RID: 2389
		public GUIDropDown.OnSelectedHandler AfterSelected;

		// Token: 0x04000956 RID: 2390
		public GUIDropDown.OnSelectedHandler OnDropped;

		// Token: 0x04000957 RID: 2391
		private readonly GUIButton button;

		// Token: 0x04000958 RID: 2392
		private readonly GUIImage icon;

		// Token: 0x04000959 RID: 2393
		private readonly GUIListBox listBox;

		// Token: 0x0400095A RID: 2394
		private RectTransform currentHighestParent;

		// Token: 0x0400095B RID: 2395
		private List<RectTransform> parentHierarchy = new List<RectTransform>();

		// Token: 0x0400095C RID: 2396
		private readonly bool selectMultiple;

		// Token: 0x0400095F RID: 2399
		private readonly List<object> selectedDataMultiple = new List<object>();

		// Token: 0x04000960 RID: 2400
		private readonly List<int> selectedIndexMultiple = new List<int>();

		// Token: 0x04000961 RID: 2401
		public bool MustSelectAtLeastOne;

		// Token: 0x04000962 RID: 2402
		private bool wasOpened;

		// Token: 0x0200094C RID: 2380
		// (Invoke) Token: 0x0600717C RID: 29052
		public delegate bool OnSelectedHandler(GUIComponent selected, object obj = null);
	}
}
