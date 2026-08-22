using System;
using System.Collections.Immutable;
using LightInject;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000412 RID: 1042
	public interface IServicesProvider
	{
		// Token: 0x06003B42 RID: 15170
		void RegisterServiceType<TSvcInterface, TService>(ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface;

		// Token: 0x06003B43 RID: 15171
		void RegisterServiceType<TSvcInterface, TService>(string name, ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface;

		// Token: 0x06003B44 RID: 15172
		void RegisterServiceResolver<TSvcInterface>(Func<ServiceContainer, TSvcInterface> factory) where TSvcInterface : class, IService;

		// Token: 0x06003B45 RID: 15173
		void CompileAndRun();

		// Token: 0x06003B46 RID: 15174
		void InjectServices<T>(T inst) where T : class;

		// Token: 0x06003B47 RID: 15175
		bool TryGetService<TSvcInterface>(out TSvcInterface service) where TSvcInterface : class, IService;

		// Token: 0x06003B48 RID: 15176
		TSvcInterface GetService<TSvcInterface>() where TSvcInterface : class, IService;

		// Token: 0x06003B49 RID: 15177
		bool TryGetService<TSvcInterface>(string name, out TSvcInterface service) where TSvcInterface : class, IService;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06003B4A RID: 15178
		// (remove) Token: 0x06003B4B RID: 15179
		event Action<Type, IService> OnServiceInstanced;

		// Token: 0x06003B4C RID: 15180
		ImmutableArray<TSvc> GetAllServices<TSvc>() where TSvc : class, IService;

		// Token: 0x06003B4D RID: 15181
		void DisposeAndReset();
	}
}
