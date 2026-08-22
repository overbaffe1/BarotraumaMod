using System;

namespace Barotrauma
{
	// Token: 0x02000336 RID: 822
	[Flags]
	public enum SpawnType
	{
		// Token: 0x0400220C RID: 8716
		Path = 0,
		// Token: 0x0400220D RID: 8717
		Human = 1,
		// Token: 0x0400220E RID: 8718
		Enemy = 2,
		// Token: 0x0400220F RID: 8719
		Cargo = 4,
		// Token: 0x04002210 RID: 8720
		Corpse = 8,
		// Token: 0x04002211 RID: 8721
		Submarine = 16,
		// Token: 0x04002212 RID: 8722
		ExitPoint = 32,
		// Token: 0x04002213 RID: 8723
		Disabled = 64
	}
}
