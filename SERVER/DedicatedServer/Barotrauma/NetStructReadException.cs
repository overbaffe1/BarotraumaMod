using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000269 RID: 617
	public class NetStructReadException : Exception
	{
		// Token: 0x06002C4A RID: 11338 RVA: 0x00125117 File Offset: 0x00123317
		[NullableContext(1)]
		public NetStructReadException(string message, [Nullable(2)] Exception innerException = null) : base(message, innerException)
		{
		}
	}
}
