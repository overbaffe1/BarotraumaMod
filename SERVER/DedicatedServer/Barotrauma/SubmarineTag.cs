using System;
using System.ComponentModel;

namespace Barotrauma
{
	// Token: 0x0200025E RID: 606
	[Flags]
	public enum SubmarineTag
	{
		// Token: 0x04001571 RID: 5489
		[Description("Shuttle")]
		Shuttle = 1,
		// Token: 0x04001572 RID: 5490
		[Description("Hide in menus")]
		HideInMenus = 2
	}
}
