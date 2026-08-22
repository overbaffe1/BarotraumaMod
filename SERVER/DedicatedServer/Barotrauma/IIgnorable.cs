using System;

namespace Barotrauma
{
	// Token: 0x02000233 RID: 563
	internal interface IIgnorable : ISpatialEntity
	{
		// Token: 0x06002695 RID: 9877
		bool IgnoreByAI(Character character);

		// Token: 0x17000B15 RID: 2837
		// (get) Token: 0x06002696 RID: 9878
		// (set) Token: 0x06002697 RID: 9879
		bool OrderedToBeIgnored { get; set; }
	}
}
