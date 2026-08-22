using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060C RID: 1548
	internal sealed class XorComponent : BooleanOperatorComponent
	{
		// Token: 0x06006400 RID: 25600 RVA: 0x0033F2E4 File Offset: 0x0033D4E4
		public XorComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06006401 RID: 25601 RVA: 0x0033F2EE File Offset: 0x0033D4EE
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs == 1;
		}
	}
}
