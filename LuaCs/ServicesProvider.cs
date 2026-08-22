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
	// Token: 0x02000508 RID: 1288
	public class ServicesProvider : IServicesProvider
	{
		// Token: 0x170014FF RID: 5375
		// (get) Token: 0x06005375 RID: 21365 RVA: 0x002CB927 File Offset: 0x002C9B27
		private ServiceContainer ServiceContainer
		{
			get
			{
				return this._serviceContainerInst;
			}
		}

		// Token: 0x06005376 RID: 21366 RVA: 0x002CB92F File Offset: 0x002C9B2F
		public ServicesProvider()
		{
			this._serviceContainerInst = new ServiceContainer(new ContainerOptions
			{
				EnablePropertyInjection = false
			});
		}

		// Token: 0x06005377 RID: 21367 RVA: 0x002CB964 File Offset: 0x002C9B64
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

		// Token: 0x06005378 RID: 21368 RVA: 0x002CBA10 File Offset: 0x002C9C10
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

		// Token: 0x06005379 RID: 21369 RVA: 0x002CBADC File Offset: 0x002C9CDC
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

		// Token: 0x0600537A RID: 21370 RVA: 0x002CBB40 File Offset: 0x002C9D40
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

		// Token: 0x0600537B RID: 21371 RVA: 0x002CBBDC File Offset: 0x002C9DDC
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

		// Token: 0x0600537C RID: 21372 RVA: 0x002CBC24 File Offset: 0x002C9E24
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

		// Token: 0x0600537D RID: 21373 RVA: 0x002CBC98 File Offset: 0x002C9E98
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

		// Token: 0x0600537E RID: 21374 RVA: 0x002CBCDC File Offset: 0x002C9EDC
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

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x0600537F RID: 21375 RVA: 0x002CBD50 File Offset: 0x002C9F50
		// (remove) Token: 0x06005380 RID: 21376 RVA: 0x002CBD88 File Offset: 0x002C9F88
		public event Action<Type, IService> OnServiceInstanced;

		// Token: 0x06005381 RID: 21377 RVA: 0x002CBDC0 File Offset: 0x002C9FC0
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

		// Token: 0x06005382 RID: 21378 RVA: 0x002CBE08 File Offset: 0x002CA008
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

		// Token: 0x04002C23 RID: 11299
		private ServiceContainer _serviceContainerInst;

		// Token: 0x04002C24 RID: 11300
		private ImmutableArray<ISystem> _systemInstances = ImmutableArray<ISystem>.Empty;

		// Token: 0x04002C25 RID: 11301
		private readonly ReaderWriterLockSlim _serviceLock = new ReaderWriterLockSlim();
	}
}
