using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x0200048B RID: 1163
	public interface ILuaCsHook : ILuaPatcher, IReusableService, IService, IDisposable, ILuaCsShim
	{
		// Token: 0x06003E24 RID: 15908
		[Obsolete("ACsMod is deprecated. Use ILuaEventService.Add() instead.")]
		void Add(string eventName, string identifier, LuaCsFunc callback, object owner = null);

		// Token: 0x06003E25 RID: 15909
		[Obsolete("ACsMod is deprecated. Use ILuaEventService.Add() instead.")]
		void Add(string eventName, LuaCsFunc callback, object owner = null);

		// Token: 0x06003E26 RID: 15910
		void Remove(string eventName, string identifier);

		// Token: 0x06003E27 RID: 15911
		object Call(string eventName, params object[] args);

		// Token: 0x06003E28 RID: 15912
		T Call<T>(string eventName, params object[] args);

		// Token: 0x02000D72 RID: 3442
		public enum HookMethodType
		{
			// Token: 0x04003FC0 RID: 16320
			Before,
			// Token: 0x04003FC1 RID: 16321
			After
		}
	}
}
