using System;
using Barotrauma.LuaCs.Events;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E6 RID: 998
	public interface IAssemblyPlugin : IDisposable, IEventPluginPreInitialize, IEvent<IEventPluginPreInitialize>, IEvent, IEventPluginInitialize, IEvent<IEventPluginInitialize>, IEventPluginLoadCompleted, IEvent<IEventPluginLoadCompleted>
	{
	}
}
