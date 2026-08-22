using System;
using System.Reflection;
using Barotrauma.LuaCs.Compatibility;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000531 RID: 1329
	public interface ILuaPatcher : IReusableService, IService, IDisposable
	{
		// Token: 0x060054A5 RID: 21669
		string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054A6 RID: 21670
		string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054A7 RID: 21671
		string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054A8 RID: 21672
		string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054A9 RID: 21673
		bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType);

		// Token: 0x060054AA RID: 21674
		bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType);

		// Token: 0x060054AB RID: 21675
		void HookMethod(string identifier, MethodBase method, LuaCsPatch patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before, IAssemblyPlugin owner = null);

		// Token: 0x060054AC RID: 21676
		void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054AD RID: 21677
		void HookMethod(string identifier, string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054AE RID: 21678
		void HookMethod(string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);

		// Token: 0x060054AF RID: 21679
		void HookMethod(string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before);
	}
}
