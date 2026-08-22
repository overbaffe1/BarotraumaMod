using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000F5 RID: 245
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class ServerProvider
	{
		// Token: 0x06002334 RID: 9012 RVA: 0x00163901 File Offset: 0x00161B01
		public void RetrieveServers(Action<ServerInfo, ServerProvider> onServerDataReceived, Action onQueryCompleted)
		{
			this.Cancel();
			this.RetrieveServersImpl(onServerDataReceived, onQueryCompleted);
		}

		// Token: 0x06002335 RID: 9013
		protected abstract void RetrieveServersImpl(Action<ServerInfo, ServerProvider> action, Action onQueryCompleted);

		// Token: 0x06002336 RID: 9014
		public abstract void Cancel();
	}
}
