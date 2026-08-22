using System;

namespace Barotrauma
{
	// Token: 0x0200024D RID: 589
	[Flags]
	internal enum MapEntityCategory
	{
		// Token: 0x040014C6 RID: 5318
		None = 0,
		// Token: 0x040014C7 RID: 5319
		Structure = 1,
		// Token: 0x040014C8 RID: 5320
		Decorative = 2,
		// Token: 0x040014C9 RID: 5321
		Machine = 4,
		// Token: 0x040014CA RID: 5322
		Medical = 8,
		// Token: 0x040014CB RID: 5323
		Weapon = 16,
		// Token: 0x040014CC RID: 5324
		Diving = 32,
		// Token: 0x040014CD RID: 5325
		Equipment = 64,
		// Token: 0x040014CE RID: 5326
		Fuel = 128,
		// Token: 0x040014CF RID: 5327
		Electrical = 256,
		// Token: 0x040014D0 RID: 5328
		Material = 1024,
		// Token: 0x040014D1 RID: 5329
		Alien = 2048,
		// Token: 0x040014D2 RID: 5330
		Wrecked = 4096,
		// Token: 0x040014D3 RID: 5331
		ItemAssembly = 8192,
		// Token: 0x040014D4 RID: 5332
		Legacy = 16384,
		// Token: 0x040014D5 RID: 5333
		Misc = 32768
	}
}
