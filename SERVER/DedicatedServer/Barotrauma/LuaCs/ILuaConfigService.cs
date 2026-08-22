using System;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000417 RID: 1047
	public interface ILuaConfigService : ILuaService, IService, IDisposable
	{
		// Token: 0x06003B82 RID: 15234
		Result LoadSavedValueForConfig(ISettingBase setting);

		// Token: 0x06003B83 RID: 15235
		bool TryGetConfig<T>(ContentPackage package, string internalName, out T instance) where T : ISettingBase;

		// Token: 0x06003B84 RID: 15236
		Result SaveConfigValue(ISettingBase setting);
	}
}
