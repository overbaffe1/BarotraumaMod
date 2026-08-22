using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000607 RID: 1543
	internal class AdderComponent : ArithmeticComponent
	{
		// Token: 0x060063E4 RID: 25572 RVA: 0x0033ED04 File Offset: 0x0033CF04
		public AdderComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060063E5 RID: 25573 RVA: 0x0033ED0E File Offset: 0x0033CF0E
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 + signal2;
		}
	}
}
