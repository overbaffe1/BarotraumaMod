using System;
using System.Collections.Immutable;
using LightInject;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000525 RID: 1317
	public interface IServicesProvider
	{
		// Token: 0x0600545E RID: 21598
		void RegisterServiceType<TSvcInterface, TService>(ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface;

		// Token: 0x0600545F RID: 21599
		void RegisterServiceType<TSvcInterface, TService>(string name, ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface;

		// Token: 0x06005460 RID: 21600
		void RegisterServiceResolver<TSvcInterface>(Func<ServiceContainer, TSvcInterface> factory) where TSvcInterface : class, IService;

		// Token: 0x06005461 RID: 21601
		void CompileAndRun();

		// Token: 0x06005462 RID: 21602
		void InjectServices<T>(T inst) where T : class;

		// Token: 0x06005463 RID: 21603
		bool TryGetService<TSvcInterface>(out TSvcInterface service) where TSvcInterface : class, IService;

		// Token: 0x06005464 RID: 21604
		TSvcInterface GetService<TSvcInterface>() where TSvcInterface : class, IService;

		// Token: 0x06005465 RID: 21605
		bool TryGetService<TSvcInterface>(string name, out TSvcInterface service) where TSvcInterface : class, IService;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06005466 RID: 21606
		// (remove) Token: 0x06005467 RID: 21607
		event Action<Type, IService> OnServiceInstanced;

		// Token: 0x06005468 RID: 21608
		ImmutableArray<TSvc> GetAllServices<TSvc>() where TSvc : class, IService;

		// Token: 0x06005469 RID: 21609
		void DisposeAndReset();
	}
}
