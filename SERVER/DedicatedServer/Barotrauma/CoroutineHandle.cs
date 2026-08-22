using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000169 RID: 361
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CoroutineHandle
	{
		// Token: 0x06001D4E RID: 7502 RVA: 0x000D142C File Offset: 0x000CF62C
		public CoroutineHandle(IEnumerator<CoroutineStatus> coroutine, string name = "")
		{
			this.Coroutine = coroutine;
			this.Name = (string.IsNullOrWhiteSpace(name) ? (coroutine.ToString() ?? "") : name);
			this.Exception = null;
		}

		// Token: 0x04000D35 RID: 3381
		public readonly IEnumerator<CoroutineStatus> Coroutine;

		// Token: 0x04000D36 RID: 3382
		public readonly string Name;

		// Token: 0x04000D37 RID: 3383
		[Nullable(2)]
		public Exception Exception;

		// Token: 0x04000D38 RID: 3384
		public bool AbortRequested;
	}
}
