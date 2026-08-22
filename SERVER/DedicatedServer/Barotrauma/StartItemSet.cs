using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000200 RID: 512
	[NullableContext(1)]
	[Nullable(0)]
	internal class StartItemSet : PrefabWithUintIdentifier
	{
		// Token: 0x060024D6 RID: 9430 RVA: 0x000F30F8 File Offset: 0x000F12F8
		public StartItemSet(ContentXElement element, StartItemsFile file) : base(file, element.GetAttributeIdentifier("identifier", Identifier.Empty))
		{
			this.Items = (from e in element.Elements()
			select new StartItem(e)).ToImmutableArray<StartItem>();
			this.Order = element.GetAttributeInt("order", 0);
		}

		// Token: 0x060024D7 RID: 9431 RVA: 0x000F3163 File Offset: 0x000F1363
		public override void Dispose()
		{
		}

		// Token: 0x04001238 RID: 4664
		public static readonly PrefabCollection<StartItemSet> Sets = new PrefabCollection<StartItemSet>();

		// Token: 0x04001239 RID: 4665
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<StartItem> Items;

		// Token: 0x0400123A RID: 4666
		public readonly int Order;
	}
}
