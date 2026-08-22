using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x0200027E RID: 638
	public interface ISerializableEntity
	{
		// Token: 0x17000D69 RID: 3433
		// (get) Token: 0x06002D32 RID: 11570
		string Name { get; }

		// Token: 0x17000D6A RID: 3434
		// (get) Token: 0x06002D33 RID: 11571
		Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }
	}
}
