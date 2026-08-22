using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200009F RID: 159
	[NullableContext(2)]
	public interface IGUISelectionCarouselAccessor
	{
		// Token: 0x0600144C RID: 5196
		object GetSelectedElement();

		// Token: 0x0600144D RID: 5197
		void SelectElement(object value);
	}
}
