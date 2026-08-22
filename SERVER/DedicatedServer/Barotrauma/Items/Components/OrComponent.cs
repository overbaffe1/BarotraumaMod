using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E6 RID: 1254
	internal sealed class OrComponent : BooleanOperatorComponent
	{
		// Token: 0x060046EF RID: 18159 RVA: 0x001C4C3C File Offset: 0x001C2E3C
		public OrComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046F0 RID: 18160 RVA: 0x001C4C46 File Offset: 0x001C2E46
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs > 0;
		}
	}
}
