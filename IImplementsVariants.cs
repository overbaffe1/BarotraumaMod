using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000344 RID: 836
	[NullableContext(2)]
	public interface IImplementsVariants<[Nullable(0)] T> where T : Prefab
	{
		// Token: 0x17001178 RID: 4472
		// (get) Token: 0x060041DD RID: 16861
		Identifier VariantOf { get; }

		// Token: 0x17001179 RID: 4473
		// (get) Token: 0x060041DE RID: 16862
		// (set) Token: 0x060041DF RID: 16863
		T ParentPrefab { get; set; }

		// Token: 0x060041E0 RID: 16864
		[NullableContext(1)]
		void InheritFrom(T parent);
	}
}
