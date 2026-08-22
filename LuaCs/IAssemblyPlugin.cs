using System;
using Barotrauma.LuaCs.Events;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004FD RID: 1277
	public interface IAssemblyPlugin : IDisposable, IEventPluginPreInitialize, IEvent<IEventPluginPreInitialize>, IEvent, IEventPluginInitialize, IEvent<IEventPluginInitialize>, IEventPluginLoadCompleted, IEvent<IEventPluginLoadCompleted>
	{
	}
}
