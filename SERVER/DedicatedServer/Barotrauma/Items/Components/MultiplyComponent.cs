using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F5 RID: 1269
	internal class MultiplyComponent : ArithmeticComponent
	{
		// Token: 0x06004773 RID: 18291 RVA: 0x001C7618 File Offset: 0x001C5818
		public MultiplyComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004774 RID: 18292 RVA: 0x001C7622 File Offset: 0x001C5822
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 * signal2;
		}
	}
}
