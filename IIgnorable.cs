using System;

namespace Barotrauma
{
	// Token: 0x02000318 RID: 792
	internal interface IIgnorable : ISpatialEntity
	{
		// Token: 0x06003F13 RID: 16147
		bool IgnoreByAI(Character character);

		// Token: 0x1700108F RID: 4239
		// (get) Token: 0x06003F14 RID: 16148
		// (set) Token: 0x06003F15 RID: 16149
		bool OrderedToBeIgnored { get; set; }
	}
}
