using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004ED RID: 1261
	internal class DivideComponent : ArithmeticComponent
	{
		// Token: 0x0600472E RID: 18222 RVA: 0x001C61F8 File Offset: 0x001C43F8
		public DivideComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600472F RID: 18223 RVA: 0x001C6202 File Offset: 0x001C4402
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
