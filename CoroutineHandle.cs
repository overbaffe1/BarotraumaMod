using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200025E RID: 606
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CoroutineHandle
	{
		// Token: 0x06003831 RID: 14385 RVA: 0x00217053 File Offset: 0x00215253
		public CoroutineHandle(IEnumerator<CoroutineStatus> coroutine, string name = "")
		{
			this.Coroutine = coroutine;
			this.Name = (string.IsNullOrWhiteSpace(name) ? (coroutine.ToString() ?? "") : name);
			this.Exception = null;
		}

		// Token: 0x04001C3A RID: 7226
		public readonly IEnumerator<CoroutineStatus> Coroutine;

		// Token: 0x04001C3B RID: 7227
		public readonly string Name;

		// Token: 0x04001C3C RID: 7228
		[Nullable(2)]
		public Exception Exception;

		// Token: 0x04001C3D RID: 7229
		public bool AbortRequested;
	}
}
