using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E9 RID: 1257
	public interface IClientLoggerService : IReusableService, IService, IDisposable
	{
		// Token: 0x06005211 RID: 21009
		void AddToGUIUpdateList();

		// Token: 0x06005212 RID: 21010
		void ShowErrorOverlay(string message, float time = 5f, float duration = 1.5f);
	}
}
