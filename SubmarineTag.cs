using System;
using System.ComponentModel;

namespace Barotrauma
{
	// Token: 0x02000333 RID: 819
	[Flags]
	public enum SubmarineTag
	{
		// Token: 0x040021FC RID: 8700
		[Description("Shuttle")]
		Shuttle = 1,
		// Token: 0x040021FD RID: 8701
		[Description("Hide in menus")]
		HideInMenus = 2
	}
}
