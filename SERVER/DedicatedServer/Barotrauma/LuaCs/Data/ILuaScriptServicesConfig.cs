using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047F RID: 1151
	public interface ILuaScriptServicesConfig : IService, IDisposable
	{
		// Token: 0x1700104A RID: 4170
		// (get) Token: 0x06003DCC RID: 15820
		bool SafeLuaIOEnabled { get; }

		// Token: 0x1700104B RID: 4171
		// (get) Token: 0x06003DCD RID: 15821
		bool UseCaching { get; }
	}
}
