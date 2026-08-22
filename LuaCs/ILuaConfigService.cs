using System;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200052A RID: 1322
	public interface ILuaConfigService : ILuaService, IService, IDisposable
	{
		// Token: 0x0600549E RID: 21662
		Result LoadSavedValueForConfig(ISettingBase setting);

		// Token: 0x0600549F RID: 21663
		bool TryGetConfig<T>(ContentPackage package, string internalName, out T instance) where T : ISettingBase;

		// Token: 0x060054A0 RID: 21664
		Result SaveConfigValue(ISettingBase setting);
	}
}
