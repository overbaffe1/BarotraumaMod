using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001FF RID: 511
	internal class StartItem
	{
		// Token: 0x060024D5 RID: 9429 RVA: 0x000F30A8 File Offset: 0x000F12A8
		[NullableContext(1)]
		public StartItem(XElement element)
		{
			this.Item = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.Amount = element.GetAttributeInt("Amount", 1);
			this.MultiPlayerOnly = element.GetAttributeBool("MultiPlayerOnly", false);
		}

		// Token: 0x04001235 RID: 4661
		public Identifier Item;

		// Token: 0x04001236 RID: 4662
		public int Amount;

		// Token: 0x04001237 RID: 4663
		public bool MultiPlayerOnly;
	}
}
