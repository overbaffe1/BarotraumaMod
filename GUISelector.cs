using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000090 RID: 144
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class GUISelector<[Nullable(0)] T> where T : GUIPrefab
	{
		// Token: 0x060013D4 RID: 5076 RVA: 0x000BE44F File Offset: 0x000BC64F
		public GUISelector(string identifier)
		{
			this.Identifier = identifier.ToIdentifier();
		}

		// Token: 0x040009DD RID: 2525
		public readonly PrefabSelector<T> Prefabs = new PrefabSelector<T>();

		// Token: 0x040009DE RID: 2526
		public readonly Identifier Identifier;
	}
}
