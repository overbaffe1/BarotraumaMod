using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200033C RID: 828
	public class NetStructReadException : Exception
	{
		// Token: 0x06004169 RID: 16745 RVA: 0x0024545B File Offset: 0x0024365B
		[NullableContext(1)]
		public NetStructReadException(string message, [Nullable(2)] Exception innerException = null) : base(message, innerException)
		{
		}
	}
}
