using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000513 RID: 1299
	public interface ILoggerSubscriber
	{
		// Token: 0x06005407 RID: 21511
		void OnLog(PendingLog pendingLog);
	}
}
