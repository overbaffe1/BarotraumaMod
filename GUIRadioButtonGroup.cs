using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x0200009C RID: 156
	public class GUIRadioButtonGroup : GUIComponent
	{
		// Token: 0x0600141B RID: 5147 RVA: 0x000BF7ED File Offset: 0x000BD9ED
		public GUIRadioButtonGroup() : base(null)
		{
			this.radioButtons = new Dictionary<int, GUITickBox>();
			this.selected = null;
		}

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x0600141C RID: 5148 RVA: 0x000BF80D File Offset: 0x000BDA0D
		// (set) Token: 0x0600141D RID: 5149 RVA: 0x000BF818 File Offset: 0x000BDA18
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
				foreach (KeyValuePair<int, GUITickBox> rbPair in this.radioButtons)
				{
					rbPair.Value.Enabled = value;
				}
			}
		}

		// Token: 0x0600141E RID: 5150 RVA: 0x000BF878 File Offset: 0x000BDA78
		public void AddRadioButton(int key, GUITickBox radioButton)
		{
			int? num = this.selected;
			if (num.GetValueOrDefault() == key & num != null)
			{
				radioButton.Selected = true;
			}
			else if (radioButton.Selected)
			{
				this.selected = new int?(key);
			}
			radioButton.SetRadioButtonGroup(this);
			this.radioButtons.Add(key, radioButton);
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x000BF8D4 File Offset: 0x000BDAD4
		public void SelectRadioButton(GUITickBox radioButton)
		{
			foreach (KeyValuePair<int, GUITickBox> rbPair in this.radioButtons)
			{
				if (radioButton == rbPair.Value)
				{
					this.Selected = new int?(rbPair.Key);
					break;
				}
			}
		}

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x06001420 RID: 5152 RVA: 0x000BF940 File Offset: 0x000BDB40
		// (set) Token: 0x06001421 RID: 5153 RVA: 0x000BF948 File Offset: 0x000BDB48
		public new int? Selected
		{
			get
			{
				return this.selected;
			}
			set
			{
				GUIRadioButtonGroup.RadioButtonGroupDelegate onSelect = this.OnSelect;
				if (onSelect != null)
				{
					onSelect(this, value);
				}
				if (this.selected != null && this.selected.Equals(value))
				{
					return;
				}
				this.selected = value;
				foreach (KeyValuePair<int, GUITickBox> radioButton in this.radioButtons)
				{
					if (radioButton.Key.Equals(value))
					{
						radioButton.Value.Selected = true;
					}
					else if (radioButton.Value.Selected)
					{
						radioButton.Value.Selected = false;
					}
				}
			}
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x06001422 RID: 5154 RVA: 0x000BFA18 File Offset: 0x000BDC18
		public GUITickBox SelectedRadioButton
		{
			get
			{
				if (this.selected == null)
				{
					return null;
				}
				return this.radioButtons[this.selected.Value];
			}
		}

		// Token: 0x040009EE RID: 2542
		private Dictionary<int, GUITickBox> radioButtons;

		// Token: 0x040009EF RID: 2543
		public GUIRadioButtonGroup.RadioButtonGroupDelegate OnSelect;

		// Token: 0x040009F0 RID: 2544
		private int? selected;

		// Token: 0x02000973 RID: 2419
		// (Invoke) Token: 0x060071D7 RID: 29143
		public delegate void RadioButtonGroupDelegate(GUIRadioButtonGroup rbg, int? val);
	}
}
