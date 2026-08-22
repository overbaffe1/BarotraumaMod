using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002E9 RID: 745
	[NullableContext(1)]
	[Nullable(0)]
	internal class StartItemSet : PrefabWithUintIdentifier
	{
		// Token: 0x06003DB4 RID: 15796 RVA: 0x002306E0 File Offset: 0x0022E8E0
		public StartItemSet(ContentXElement element, StartItemsFile file) : base(file, element.GetAttributeIdentifier("identifier", Identifier.Empty))
		{
			this.Items = (from e in element.Elements()
			select new StartItem(e)).ToImmutableArray<StartItem>();
			this.Order = element.GetAttributeInt("order", 0);
		}

		// Token: 0x06003DB5 RID: 15797 RVA: 0x0023074B File Offset: 0x0022E94B
		public override void Dispose()
		{
		}

		// Token: 0x04002064 RID: 8292
		public static readonly PrefabCollection<StartItemSet> Sets = new PrefabCollection<StartItemSet>();

		// Token: 0x04002065 RID: 8293
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<StartItem> Items;

		// Token: 0x04002066 RID: 8294
		public readonly int Order;
	}
}
