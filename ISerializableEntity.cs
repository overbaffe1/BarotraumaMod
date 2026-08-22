using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x0200034F RID: 847
	public interface ISerializableEntity
	{
		// Token: 0x1700118B RID: 4491
		// (get) Token: 0x06004244 RID: 16964
		string Name { get; }

		// Token: 0x1700118C RID: 4492
		// (get) Token: 0x06004245 RID: 16965
		Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }
	}
}
