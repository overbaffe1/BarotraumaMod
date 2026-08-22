using System;

namespace Barotrauma
{
	// Token: 0x0200034D RID: 845
	[AttributeUsage(AttributeTargets.Property)]
	internal class Editable : Attribute
	{
		// Token: 0x06004240 RID: 16960 RVA: 0x00249A60 File Offset: 0x00247C60
		public Editable()
		{
		}

		// Token: 0x06004241 RID: 16961 RVA: 0x00249AB0 File Offset: 0x00247CB0
		public Editable(int minValue, int maxValue)
		{
			this.MinValueInt = minValue;
			this.MaxValueInt = maxValue;
		}

		// Token: 0x06004242 RID: 16962 RVA: 0x00249B0C File Offset: 0x00247D0C
		public Editable(float minValue, float maxValue, int decimals = 1)
		{
			this.MinValueFloat = minValue;
			this.MaxValueFloat = maxValue;
			this.DecimalCount = decimals;
		}

		// Token: 0x04002272 RID: 8818
		public int MaxLength = -1;

		// Token: 0x04002273 RID: 8819
		public int DecimalCount = 1;

		// Token: 0x04002274 RID: 8820
		public int MinValueInt = int.MinValue;

		// Token: 0x04002275 RID: 8821
		public int MaxValueInt = int.MaxValue;

		// Token: 0x04002276 RID: 8822
		public float MinValueFloat = float.MinValue;

		// Token: 0x04002277 RID: 8823
		public float MaxValueFloat = float.MaxValue;

		// Token: 0x04002278 RID: 8824
		public bool ForceShowPlusMinusButtons;

		// Token: 0x04002279 RID: 8825
		public float ValueStep;

		// Token: 0x0400227A RID: 8826
		public bool TransferToSwappedItem;

		// Token: 0x0400227B RID: 8827
		public string[] VectorComponentLabels;

		// Token: 0x0400227C RID: 8828
		public string FallBackTextTag;

		// Token: 0x0400227D RID: 8829
		public bool ReadOnly;
	}
}
