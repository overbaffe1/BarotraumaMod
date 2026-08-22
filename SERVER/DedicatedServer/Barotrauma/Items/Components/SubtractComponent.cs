using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FF RID: 1279
	internal class SubtractComponent : ArithmeticComponent
	{
		// Token: 0x060047CC RID: 18380 RVA: 0x001C8DAC File Offset: 0x001C6FAC
		public SubtractComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060047CD RID: 18381 RVA: 0x001C8DB6 File Offset: 0x001C6FB6
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 - signal2;
		}
	}
}
