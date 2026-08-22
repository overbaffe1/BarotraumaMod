using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x0200056E RID: 1390
	public interface ILuaCsHook : ILuaPatcher, IReusableService, IService, IDisposable, ILuaCsShim
	{
		// Token: 0x060055E1 RID: 21985
		[Obsolete("ACsMod is deprecated. Use ILuaEventService.Add() instead.")]
		void Add(string eventName, string identifier, LuaCsFunc callback, object owner = null);

		// Token: 0x060055E2 RID: 21986
		[Obsolete("ACsMod is deprecated. Use ILuaEventService.Add() instead.")]
		void Add(string eventName, LuaCsFunc callback, object owner = null);

		// Token: 0x060055E3 RID: 21987
		void Remove(string eventName, string identifier);

		// Token: 0x060055E4 RID: 21988
		object Call(string eventName, params object[] args);

		// Token: 0x060055E5 RID: 21989
		T Call<T>(string eventName, params object[] args);

		// Token: 0x02001380 RID: 4992
		public enum HookMethodType
		{
			// Token: 0x040062D2 RID: 25298
			Before,
			// Token: 0x040062D3 RID: 25299
			After
		}
	}
}
