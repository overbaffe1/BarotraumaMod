using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000618 RID: 1560
	internal class MultiplyComponent : ArithmeticComponent
	{
		// Token: 0x06006443 RID: 25667 RVA: 0x003402E4 File Offset: 0x0033E4E4
		public MultiplyComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06006444 RID: 25668 RVA: 0x003402EE File Offset: 0x0033E4EE
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 * signal2;
		}
	}
}
