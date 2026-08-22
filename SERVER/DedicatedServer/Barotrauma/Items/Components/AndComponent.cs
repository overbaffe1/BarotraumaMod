using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E4 RID: 1252
	internal sealed class AndComponent : BooleanOperatorComponent
	{
		// Token: 0x060046E1 RID: 18145 RVA: 0x001C48FC File Offset: 0x001C2AFC
		public AndComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046E2 RID: 18146 RVA: 0x001C4906 File Offset: 0x001C2B06
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs >= 2;
		}
	}
}
