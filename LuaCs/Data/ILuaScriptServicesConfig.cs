using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059B RID: 1435
	public interface ILuaScriptServicesConfig : IService, IDisposable
	{
		// Token: 0x170015B0 RID: 5552
		// (get) Token: 0x06005735 RID: 22325
		bool SafeLuaIOEnabled { get; }

		// Token: 0x170015B1 RID: 5553
		// (get) Token: 0x06005736 RID: 22326
		bool UseCaching { get; }
	}
}
