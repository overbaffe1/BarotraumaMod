using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000609 RID: 1545
	internal sealed class AndComponent : BooleanOperatorComponent
	{
		// Token: 0x060063F0 RID: 25584 RVA: 0x0033EF94 File Offset: 0x0033D194
		public AndComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060063F1 RID: 25585 RVA: 0x0033EF9E File Offset: 0x0033D19E
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs >= 2;
		}
	}
}
