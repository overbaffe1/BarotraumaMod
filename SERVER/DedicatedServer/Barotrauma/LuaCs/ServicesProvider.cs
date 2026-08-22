using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using LightInject;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F4 RID: 1012
	public class ServicesProvider : IServicesProvider
	{
		// Token: 0x17000FB8 RID: 4024
		// (get) Token: 0x06003A51 RID: 14929 RVA: 0x001871E7 File Offset: 0x001853E7
		private ServiceContainer ServiceContainer
		{
			get
			{
				return this._serviceContainerInst;
			}
		}

		// Token: 0x06003A52 RID: 14930 RVA: 0x001871EF File Offset: 0x001853EF
		public ServicesProvider()
		{
			this._serviceContainerInst = new ServiceContainer(new ContainerOptions
			{
				EnablePropertyInjection = false
			});
		}

		// Token: 0x06003A53 RID: 14931 RVA: 0x00187224 File Offset: 0x00185424
		public void RegisterServiceType<TSvcInterface, TService>(ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface
		{
			if (typeof(TSvcInterface).IsAssignableTo(typeof(ISystem)))
			{
				lifetimeInstance = new PerContainerLifetime();
			}
			if (lifetimeInstance == null)
			{
				switch (lifetime)
				{
				case ServiceLifetime.Singleton:
					lifetimeInstance = new PerContainerLifetime();
					goto IL_56;
				case ServiceLifetime.PerThread:
					lifetimeInstance = new PerThreadLifetime();
					goto IL_56;
				}
				lifetimeInstance = null;
			}
			IL_56:
			try
			{
				this._serviceLock.EnterReadLock();
				if (lifetimeInstance != null)
				{
					this.ServiceContainer.Register<TSvcInterface, TService>(lifetimeInstance);
				}
				else
				{
					this.ServiceContainer.Register<TSvcInterface, TService>();
				}
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
		}

		// Token: 0x06003A54 RID: 14932 RVA: 0x001872D0 File Offset: 0x001854D0
		public void RegisterServiceType<TSvcInterface, TService>(string name, ServiceLifetime lifetime, ILifetime lifetimeInstance = null) where TSvcInterface : class, IService where TService : class, IService, TSvcInterface
		{
			if (name.IsNullOrWhiteSpace())
			{
				throw new ArgumentNullException("Tried to register a service of type " + typeof(TService).Name + " but the name provided is null or empty.");
			}
			if (typeof(TService).IsAssignableTo(typeof(ISystem)))
			{
				lifetimeInstance = new PerContainerLifetime();
			}
			if (lifetimeInstance == null)
			{
				switch (lifetime)
				{
				case ServiceLifetime.Singleton:
					lifetimeInstance = new PerContainerLifetime();
					goto IL_86;
				case ServiceLifetime.PerThread:
					lifetimeInstance = new PerThreadLifetime();
					goto IL_86;
				}
				lifetimeInstance = new PerRequestLifeTime();
			}
			IL_86:
			try
			{
				this._serviceLock.EnterReadLock();
				this.ServiceContainer.Register<TSvcInterface, TService>(name, lifetimeInstance);
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
		}

		// Token: 0x06003A55 RID: 14933 RVA: 0x0018739C File Offset: 0x0018559C
		public void RegisterServiceResolver<TSvcInterface>(Func<ServiceContainer, TSvcInterface> factory) where TSvcInterface : class, IService
		{
			try
			{
				this._serviceLock.EnterReadLock();
				this.ServiceContainer.Register<TSvcInterface>((IServiceFactory f) => factory(this.ServiceContainer));
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
		}

		// Token: 0x06003A56 RID: 14934 RVA: 0x00187400 File Offset: 0x00185600
		public void CompileAndRun()
		{
			try
			{
				this._serviceLock.EnterWriteLock();
				this.ServiceContainer.Compile();
				if (!this._systemInstances.IsDefaultOrEmpty)
				{
					ThrowHelper.ThrowInvalidOperationException("Systems are already instanced!");
				}
				this._systemInstances = (from obj in this.ServiceContainer.GetAllInstances(typeof(ISystem))
				select (ISystem)obj).ToImmutableArray<ISystem>();
			}
			finally
			{
				this._serviceLock.ExitWriteLock();
			}
		}

		// Token: 0x06003A57 RID: 14935 RVA: 0x0018749C File Offset: 0x0018569C
		public void InjectServices<T>(T inst) where T : class
		{
			try
			{
				this._serviceLock.EnterReadLock();
				this.ServiceContainer.InjectProperties(inst);
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
		}

		// Token: 0x06003A58 RID: 14936 RVA: 0x001874E4 File Offset: 0x001856E4
		public bool TryGetService<TSvcInterface>(out TSvcInterface service) where TSvcInterface : class, IService
		{
			bool result;
			try
			{
				this._serviceLock.EnterReadLock();
				service = this.ServiceContainer.TryGetInstance<TSvcInterface>();
				result = (service != null);
			}
			catch
			{
				service = default(TSvcInterface);
				result = false;
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
			return result;
		}

		// Token: 0x06003A59 RID: 14937 RVA: 0x00187558 File Offset: 0x00185758
		public TSvcInterface GetService<TSvcInterface>() where TSvcInterface : class, IService
		{
			TSvcInterface instance;
			try
			{
				this._serviceLock.EnterReadLock();
				instance = this.ServiceContainer.GetInstance<TSvcInterface>();
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
			return instance;
		}

		// Token: 0x06003A5A RID: 14938 RVA: 0x0018759C File Offset: 0x0018579C
		public bool TryGetService<TSvcInterface>(string name, out TSvcInterface service) where TSvcInterface : class, IService
		{
			bool result;
			try
			{
				this._serviceLock.EnterReadLock();
				service = this.ServiceContainer.TryGetInstance(name);
				result = (service != null);
			}
			catch
			{
				service = default(TSvcInterface);
				result = false;
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
			return result;
		}

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06003A5B RID: 14939 RVA: 0x00187610 File Offset: 0x00185810
		// (remove) Token: 0x06003A5C RID: 14940 RVA: 0x00187648 File Offset: 0x00185848
		public event Action<Type, IService> OnServiceInstanced;

		// Token: 0x06003A5D RID: 14941 RVA: 0x00187680 File Offset: 0x00185880
		public ImmutableArray<TSvc> GetAllServices<TSvc>() where TSvc : class, IService
		{
			ImmutableArray<TSvc> result;
			try
			{
				this._serviceLock.EnterReadLock();
				result = this.ServiceContainer.GetAllInstances<TSvc>().ToImmutableArray<TSvc>();
			}
			finally
			{
				this._serviceLock.ExitReadLock();
			}
			return result;
		}

		// Token: 0x06003A5E RID: 14942 RVA: 0x001876C8 File Offset: 0x001858C8
		[MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.PreserveSig)]
		public void DisposeAndReset()
		{
			if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Assembly ");
				defaultInterpolatedStringHandler.AppendFormatted(Assembly.GetCallingAssembly().FullName);
				defaultInterpolatedStringHandler.AppendLiteral(" attempted to call ");
				defaultInterpolatedStringHandler.AppendFormatted("DisposeAndReset");
				defaultInterpolatedStringHandler.AppendLiteral("().");
				throw new MethodAccessException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			try
			{
				this._serviceLock.EnterWriteLock();
				foreach (ISystem system in this._systemInstances)
				{
					try
					{
						system.Dispose();
					}
					catch (Exception e)
					{
					}
				}
				this._systemInstances = ImmutableArray<ISystem>.Empty;
				ServiceContainer serviceContainerInst = this._serviceContainerInst;
				if (serviceContainerInst != null)
				{
					serviceContainerInst.Dispose();
				}
				this._serviceContainerInst = new ServiceContainer();
			}
			finally
			{
				this._serviceLock.ExitWriteLock();
			}
		}

		// Token: 0x04001D3F RID: 7487
		private ServiceContainer _serviceContainerInst;

		// Token: 0x04001D40 RID: 7488
		private ImmutableArray<ISystem> _systemInstances = ImmutableArray<ISystem>.Empty;

		// Token: 0x04001D41 RID: 7489
		private readonly ReaderWriterLockSlim _serviceLock = new ReaderWriterLockSlim();
	}
}
