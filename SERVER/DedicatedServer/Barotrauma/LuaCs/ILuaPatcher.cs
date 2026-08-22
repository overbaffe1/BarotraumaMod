using System;
using System.Reflection;
using Barotrauma.LuaCs.Compatibility;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200041E RID: 1054
	public interface ILuaPatcher : IReusableService, IService, IDisposable
	{
		// Token: 0x06003B89 RID: 15241
		string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B8A RID: 15242
		string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B8B RID: 15243
		string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B8C RID: 15244
		string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B8D RID: 15245
		bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType);

		// Token: 0x06003B8E RID: 15246
		bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType);

		// Token: 0x06003B8F RID: 15247
		void HookMethod(string identifier, MethodBase method, LuaCsPatch patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before, IAssemblyPlugin owner = null);

		// Token: 0x06003B90 RID: 15248
		void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B91 RID: 15249
		void HookMethod(string identifier, string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B92 RID: 15250
		void HookMethod(string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x06003B93 RID: 15251
		void HookMethod(string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);
	}
}
