using System;

namespace Barotrauma
{
	// Token: 0x02000265 RID: 613
	internal interface IMemorizable<T>
	{
		// Token: 0x17000D41 RID: 3393
		// (get) Token: 0x06002C30 RID: 11312
		Memento<T> Memento { get; }

		// Token: 0x06002C31 RID: 11313
		void StoreSnapshot();

		// Token: 0x06002C32 RID: 11314
		void Undo();

		// Token: 0x06002C33 RID: 11315
		void Redo();

		// Token: 0x06002C34 RID: 11316
		void ClearHistory();
	}
}
