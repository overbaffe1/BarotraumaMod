using System;
using Barotrauma.LuaCs.Compatibility;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200052D RID: 1325
	public interface ILuaEventService : ILuaSafeEventService, ILuaService, IService, IDisposable, ILuaCsHook, ILuaPatcher, IReusableService, ILuaCsShim
	{
	}
}
