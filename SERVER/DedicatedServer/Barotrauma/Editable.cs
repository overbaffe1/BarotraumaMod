using System;

namespace Barotrauma
{
	// Token: 0x0200027C RID: 636
	[AttributeUsage(AttributeTargets.Property)]
	internal class Editable : Attribute
	{
		// Token: 0x06002D2E RID: 11566 RVA: 0x00129970 File Offset: 0x00127B70
		public Editable()
		{
		}

		// Token: 0x06002D2F RID: 11567 RVA: 0x001299C0 File Offset: 0x00127BC0
		public Editable(int minValue, int maxValue)
		{
			this.MinValueInt = minValue;
			this.MaxValueInt = maxValue;
		}

		// Token: 0x06002D30 RID: 11568 RVA: 0x00129A1C File Offset: 0x00127C1C
		public Editable(float minValue, float maxValue, int decimals = 1)
		{
			this.MinValueFloat = minValue;
			this.MaxValueFloat = maxValue;
			this.DecimalCount = decimals;
		}

		// Token: 0x0400162E RID: 5678
		public int MaxLength = -1;

		// Token: 0x0400162F RID: 5679
		public int DecimalCount = 1;

		// Token: 0x04001630 RID: 5680
		public int MinValueInt = int.MinValue;

		// Token: 0x04001631 RID: 5681
		public int MaxValueInt = int.MaxValue;

		// Token: 0x04001632 RID: 5682
		public float MinValueFloat = float.MinValue;

		// Token: 0x04001633 RID: 5683
		public float MaxValueFloat = float.MaxValue;

		// Token: 0x04001634 RID: 5684
		public bool ForceShowPlusMinusButtons;

		// Token: 0x04001635 RID: 5685
		public float ValueStep;

		// Token: 0x04001636 RID: 5686
		public bool TransferToSwappedItem;

		// Token: 0x04001637 RID: 5687
		public string[] VectorComponentLabels;

		// Token: 0x04001638 RID: 5688
		public string FallBackTextTag;

		// Token: 0x04001639 RID: 5689
		public bool ReadOnly;
	}
}
