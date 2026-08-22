using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E2 RID: 1250
	internal class AdderComponent : ArithmeticComponent
	{
		// Token: 0x060046D5 RID: 18133 RVA: 0x001C466C File Offset: 0x001C286C
		public AdderComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046D6 RID: 18134 RVA: 0x001C4676 File Offset: 0x001C2876
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 + signal2;
		}
	}
}
