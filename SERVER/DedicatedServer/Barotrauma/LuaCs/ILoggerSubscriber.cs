using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000400 RID: 1024
	public interface ILoggerSubscriber
	{
		// Token: 0x06003AEB RID: 15083
		void OnLog(PendingLog pendingLog);
	}
}
