using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200033D RID: 829
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class NetworkSerialize : Attribute
	{
		// Token: 0x0600416A RID: 16746 RVA: 0x00245468 File Offset: 0x00243668
		public NetworkSerialize([CallerLineNumber] int lineNumber = 0)
		{
			this.OrderKey = lineNumber;
		}

		// Token: 0x0400221F RID: 8735
		public int MaxValueInt = int.MaxValue;

		// Token: 0x04002220 RID: 8736
		public int MinValueInt = int.MinValue;

		// Token: 0x04002221 RID: 8737
		public float MaxValueFloat = float.MaxValue;

		// Token: 0x04002222 RID: 8738
		public float MinValueFloat = float.MinValue;

		// Token: 0x04002223 RID: 8739
		public int NumberOfBits = 8;

		// Token: 0x04002224 RID: 8740
		public bool IncludeColorAlpha;

		// Token: 0x04002225 RID: 8741
		public int ArrayMaxSize = 65535;

		// Token: 0x04002226 RID: 8742
		public readonly int OrderKey;
	}
}
