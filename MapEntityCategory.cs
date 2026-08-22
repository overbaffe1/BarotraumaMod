using System;

namespace Barotrauma
{
	// Token: 0x02000325 RID: 805
	[Flags]
	internal enum MapEntityCategory
	{
		// Token: 0x04002199 RID: 8601
		None = 0,
		// Token: 0x0400219A RID: 8602
		Structure = 1,
		// Token: 0x0400219B RID: 8603
		Decorative = 2,
		// Token: 0x0400219C RID: 8604
		Machine = 4,
		// Token: 0x0400219D RID: 8605
		Medical = 8,
		// Token: 0x0400219E RID: 8606
		Weapon = 16,
		// Token: 0x0400219F RID: 8607
		Diving = 32,
		// Token: 0x040021A0 RID: 8608
		Equipment = 64,
		// Token: 0x040021A1 RID: 8609
		Fuel = 128,
		// Token: 0x040021A2 RID: 8610
		Electrical = 256,
		// Token: 0x040021A3 RID: 8611
		Material = 1024,
		// Token: 0x040021A4 RID: 8612
		Alien = 2048,
		// Token: 0x040021A5 RID: 8613
		Wrecked = 4096,
		// Token: 0x040021A6 RID: 8614
		ItemAssembly = 8192,
		// Token: 0x040021A7 RID: 8615
		Legacy = 16384,
		// Token: 0x040021A8 RID: 8616
		Misc = 32768
	}
}
