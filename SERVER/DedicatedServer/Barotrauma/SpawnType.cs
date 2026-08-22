using System;

namespace Barotrauma
{
	// Token: 0x02000262 RID: 610
	[Flags]
	public enum SpawnType
	{
		// Token: 0x040015B0 RID: 5552
		Path = 0,
		// Token: 0x040015B1 RID: 5553
		Human = 1,
		// Token: 0x040015B2 RID: 5554
		Enemy = 2,
		// Token: 0x040015B3 RID: 5555
		Cargo = 4,
		// Token: 0x040015B4 RID: 5556
		Corpse = 8,
		// Token: 0x040015B5 RID: 5557
		Submarine = 16,
		// Token: 0x040015B6 RID: 5558
		ExitPoint = 32,
		// Token: 0x040015B7 RID: 5559
		Disabled = 64
	}
}
