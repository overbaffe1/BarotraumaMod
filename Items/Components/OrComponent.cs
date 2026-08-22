using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060B RID: 1547
	internal sealed class OrComponent : BooleanOperatorComponent
	{
		// Token: 0x060063FE RID: 25598 RVA: 0x0033F2D4 File Offset: 0x0033D4D4
		public OrComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060063FF RID: 25599 RVA: 0x0033F2DE File Offset: 0x0033D4DE
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs > 0;
		}
	}
}
