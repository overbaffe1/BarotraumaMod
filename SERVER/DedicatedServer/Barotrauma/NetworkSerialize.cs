using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200026A RID: 618
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class NetworkSerialize : Attribute
	{
		// Token: 0x06002C4B RID: 11339 RVA: 0x00125124 File Offset: 0x00123324
		public NetworkSerialize([CallerLineNumber] int lineNumber = 0)
		{
			this.OrderKey = lineNumber;
		}

		// Token: 0x040015D9 RID: 5593
		public int MaxValueInt = int.MaxValue;

		// Token: 0x040015DA RID: 5594
		public int MinValueInt = int.MinValue;

		// Token: 0x040015DB RID: 5595
		public float MaxValueFloat = float.MaxValue;

		// Token: 0x040015DC RID: 5596
		public float MinValueFloat = float.MinValue;

		// Token: 0x040015DD RID: 5597
		public int NumberOfBits = 8;

		// Token: 0x040015DE RID: 5598
		public bool IncludeColorAlpha;

		// Token: 0x040015DF RID: 5599
		public int ArrayMaxSize = 65535;

		// Token: 0x040015E0 RID: 5600
		public readonly int OrderKey;
	}
}
