using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000611 RID: 1553
	internal class DivideComponent : ArithmeticComponent
	{
		// Token: 0x0600641F RID: 25631 RVA: 0x0033FA2C File Offset: 0x0033DC2C
		public DivideComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06006420 RID: 25632 RVA: 0x0033FA36 File Offset: 0x0033DC36
		protected override float Calculate(float signal1, float signal2)
		{
			if (MathUtils.NearlyEqual(signal2, 0f, 0.0001f))
			{
				return float.NaN;
			}
			return signal1 / signal2;
		}
	}
}
