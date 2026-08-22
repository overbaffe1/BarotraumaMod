using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000622 RID: 1570
	internal class SubtractComponent : ArithmeticComponent
	{
		// Token: 0x0600649C RID: 25756 RVA: 0x00341A68 File Offset: 0x0033FC68
		public SubtractComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600649D RID: 25757 RVA: 0x00341A72 File Offset: 0x0033FC72
		protected override float Calculate(float signal1, float signal2)
		{
			return signal1 - signal2;
		}
	}
}
