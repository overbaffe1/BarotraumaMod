using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002E8 RID: 744
	internal class StartItem
	{
		// Token: 0x06003DB3 RID: 15795 RVA: 0x00230690 File Offset: 0x0022E890
		[NullableContext(1)]
		public StartItem(XElement element)
		{
			this.Item = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.Amount = element.GetAttributeInt("Amount", 1);
			this.MultiPlayerOnly = element.GetAttributeBool("MultiPlayerOnly", false);
		}

		// Token: 0x04002061 RID: 8289
		public Identifier Item;

		// Token: 0x04002062 RID: 8290
		public int Amount;

		// Token: 0x04002063 RID: 8291
		public bool MultiPlayerOnly;
	}
}
