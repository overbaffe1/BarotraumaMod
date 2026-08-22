using System;

namespace Barotrauma
{
	// Token: 0x02000338 RID: 824
	internal interface IMemorizable<T>
	{
		// Token: 0x17001166 RID: 4454
		// (get) Token: 0x0600414F RID: 16719
		Memento<T> Memento { get; }

		// Token: 0x06004150 RID: 16720
		void StoreSnapshot();

		// Token: 0x06004151 RID: 16721
		void Undo();

		// Token: 0x06004152 RID: 16722
		void Redo();

		// Token: 0x06004153 RID: 16723
		void ClearHistory();
	}
}
