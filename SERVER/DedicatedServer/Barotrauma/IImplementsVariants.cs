using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000271 RID: 625
	[NullableContext(2)]
	public interface IImplementsVariants<[Nullable(0)] T> where T : Prefab
	{
		// Token: 0x17000D51 RID: 3409
		// (get) Token: 0x06002CBA RID: 11450
		Identifier VariantOf { get; }

		// Token: 0x17000D52 RID: 3410
		// (get) Token: 0x06002CBB RID: 11451
		// (set) Token: 0x06002CBC RID: 11452
		T ParentPrefab { get; set; }

		// Token: 0x06002CBD RID: 11453
		[NullableContext(1)]
		void InheritFrom(T parent);
	}
}
