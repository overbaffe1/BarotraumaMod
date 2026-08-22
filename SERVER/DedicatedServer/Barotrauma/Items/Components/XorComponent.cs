using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E7 RID: 1255
	internal sealed class XorComponent : BooleanOperatorComponent
	{
		// Token: 0x060046F1 RID: 18161 RVA: 0x001C4C4C File Offset: 0x001C2E4C
		public XorComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046F2 RID: 18162 RVA: 0x001C4C56 File Offset: 0x001C2E56
		protected override bool GetOutput(int numTrueInputs)
		{
			return numTrueInputs == 1;
		}
	}
}
