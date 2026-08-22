using System;
using Barotrauma.LuaCs.Compatibility;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200041A RID: 1050
	public interface ILuaEventService : ILuaSafeEventService, ILuaService, IService, IDisposable, ILuaCsHook, ILuaPatcher, IReusableService, ILuaCsShim
	{
	}
}
