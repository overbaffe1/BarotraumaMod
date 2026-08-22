using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using Basic.Reference.Assemblies;
using FluentResults;
using LightInject;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Toolkit.Diagnostics;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F1 RID: 1009
	public class PluginManagementService : IAssemblyManagementService, IPluginManagementService, IReusableService, IService, IDisposable
	{
		// Token: 0x17000FB5 RID: 4021
		// (get) Token: 0x06003A1C RID: 14876 RVA: 0x00184E90 File Offset: 0x00183090
		private IEnumerable<MetadataReference> BaseMetadataReferences
		{
			get
			{
				if (this._baseMetadataReferences.IsDefaultOrEmpty)
				{
					this._baseMetadataReferences = (from ar in Net80.References.All.Union(from ass in AssemblyLoadContext.Default.Assemblies
					where !ass.IsDynamic && !ass.GetName().FullName.StartsWith("BarotraumaCore") && !ass.GetName().FullName.StartsWith("Barotrauma") && !ass.GetName().FullName.StartsWith("DedicatedServer")
					where !ass.Location.IsNullOrWhiteSpace()
					select MetadataReference.CreateFromFile(ass.Location, default(MetadataReferenceProperties), null))
					where ar != null
					select ar).ToImmutableArray<MetadataReference>();
				}
				return this._baseMetadataReferences;
			}
		}

		// Token: 0x17000FB6 RID: 4022
		// (get) Token: 0x06003A1D RID: 14877 RVA: 0x00184F6C File Offset: 0x0018316C
		private IEnumerable<MetadataReference> BaseMetadataReferencesWithBarotrauma
		{
			get
			{
				if (this._baseMetadataReferencesNonPublicized.IsDefaultOrEmpty)
				{
					this._baseMetadataReferencesNonPublicized = (from ar in Net80.References.All.Union(from ass in AssemblyLoadContext.Default.Assemblies
					where !ass.IsDynamic
					where !ass.Location.IsNullOrWhiteSpace()
					select MetadataReference.CreateFromFile(ass.Location, default(MetadataReferenceProperties), null))
					where ar != null
					select ar).ToImmutableArray<MetadataReference>();
				}
				return this._baseMetadataReferencesNonPublicized;
			}
		}

		// Token: 0x06003A1E RID: 14878 RVA: 0x00185048 File Offset: 0x00183248
		public void Dispose()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this.UnsafeDisposeResourcesInternal();
					this._assemblyLoaderFactory = null;
					this._storageService = null;
					this._eventService = null;
					this._logger = null;
					this._configService = null;
					this._luaScriptManagementService = null;
					this._luaCsInfoProvider = null;
					GC.SuppressFinalize(this);
				}
			}
		}

		// Token: 0x06003A1F RID: 14879 RVA: 0x001850E8 File Offset: 0x001832E8
		private void UnsafeDisposeResourcesInternal()
		{
			foreach (ValueTuple<ContentPackage, IAssemblyPlugin> packPlugin in this._pluginInstances.SelectMany((KeyValuePair<ContentPackage, ImmutableArray<IAssemblyPlugin>> kvp) => from pluginInst in kvp.Value
			select new ValueTuple<ContentPackage, IAssemblyPlugin>(kvp.Key, pluginInst)))
			{
				try
				{
					packPlugin.Item2.Dispose();
				}
				catch (Exception e)
				{
					this._logger.LogError("Error while disposing plugin for ContentPackage " + packPlugin.Item1.Name + ": \n" + e.Message);
				}
			}
			this._pluginInstances.Clear();
			this._pluginPackageLookup.Clear();
			ServiceContainer pluginInjectorContainer = this._pluginInjectorContainer;
			if (pluginInjectorContainer != null)
			{
				pluginInjectorContainer.Dispose();
			}
			this._pluginInjectorContainer = null;
			ReflectionUtils.ResetCache();
			foreach (KeyValuePair<ContentPackage, IAssemblyLoaderService> loader in this._assemblyLoaders)
			{
				try
				{
					loader.Value.Dispose();
					this._unloadingAssemblyLoaders.Add(loader.Value, loader.Key);
				}
				catch (Exception e2)
				{
					ILoggerService logger = this._logger;
					if (logger != null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to dispose of ");
						defaultInterpolatedStringHandler.AppendFormatted("IAssemblyLoaderService");
						defaultInterpolatedStringHandler.AppendLiteral(" for ContentPackage ");
						defaultInterpolatedStringHandler.AppendFormatted(loader.Key.Name);
						defaultInterpolatedStringHandler.AppendLiteral(": \n");
						defaultInterpolatedStringHandler.AppendFormatted(e2.Message);
						logger.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			this._assemblyLoaders.Clear();
		}

		// Token: 0x17000FB7 RID: 4023
		// (get) Token: 0x06003A20 RID: 14880 RVA: 0x001852C0 File Offset: 0x001834C0
		// (set) Token: 0x06003A21 RID: 14881 RVA: 0x001852CD File Offset: 0x001834CD
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x06003A22 RID: 14882 RVA: 0x001852DC File Offset: 0x001834DC
		public Result Reset()
		{
			Result result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this.UnsafeDisposeResourcesInternal();
				result = Result.Ok();
			}
			return result;
		}

		// Token: 0x06003A23 RID: 14883 RVA: 0x00185348 File Offset: 0x00183548
		public PluginManagementService(IAssemblyLoaderService.IFactory assemblyLoaderFactory, IStorageService storageService, ILoggerService logger, Lazy<IEventService> eventService, Lazy<ILuaScriptManagementService> luaScriptManagementService, Lazy<IConfigService> configService, Lazy<ILuaPatcher> pluginLuaPatcherService, Func<IConsoleCommandsService> consoleCommandServiceFactory, ILuaCsInfoProvider luaCsInfoProvider)
		{
			this._assemblyLoaderFactory = assemblyLoaderFactory;
			this._storageService = storageService;
			this._logger = logger;
			this._eventService = eventService;
			this._luaScriptManagementService = luaScriptManagementService;
			this._configService = configService;
			this._pluginLuaPatcherService = pluginLuaPatcherService;
			this._consoleCommandServiceFactory = consoleCommandServiceFactory;
			this._luaCsInfoProvider = luaCsInfoProvider;
			this._internalConsoleCommandsService = consoleCommandServiceFactory();
			this.RegisterCommands(this._internalConsoleCommandsService);
		}

		// Token: 0x06003A24 RID: 14884 RVA: 0x00185411 File Offset: 0x00183611
		private void RegisterCommands(IConsoleCommandsService cmdService)
		{
			cmdService.RegisterCommand("plugin_forcerungc", "Forces the GC to run", delegate(string[] cmds)
			{
				this._logger.LogMessage("Forcing GC run.", null, null);
				Task.Factory.StartNew<Task>(delegate()
				{
					PluginManagementService.<<RegisterCommands>b__43_1>d <<RegisterCommands>b__43_1>d;
					<<RegisterCommands>b__43_1>d.<>t__builder = AsyncTaskMethodBuilder.Create();
					<<RegisterCommands>b__43_1>d.<>4__this = this;
					<<RegisterCommands>b__43_1>d.<>1__state = -1;
					<<RegisterCommands>b__43_1>d.<>t__builder.Start<PluginManagementService.<<RegisterCommands>b__43_1>d>(ref <<RegisterCommands>b__43_1>d);
					return <<RegisterCommands>b__43_1>d.<>t__builder.Task;
				});
			}, null, false);
		}

		// Token: 0x06003A25 RID: 14885 RVA: 0x00185434 File Offset: 0x00183634
		private ServiceContainer CreatePluginServiceContainer()
		{
			ServiceContainer container = new ServiceContainer(new ContainerOptions
			{
				EnablePropertyInjection = true
			});
			if (this._pluginEventService == null)
			{
				this._pluginEventService = new EventService(this._logger, this._pluginLuaPatcherService.Value);
			}
			this._eventService.Value.AddDispatcherEventService(this._pluginEventService);
			container.Register<ILoggerService>((IServiceFactory fac) => this._logger);
			container.Register<IStorageService>((IServiceFactory fac) => this._storageService);
			container.Register<IEventService>((IServiceFactory fac) => this._pluginEventService);
			container.Register<IPluginManagementService>((IServiceFactory fac) => this);
			container.Register<ILuaScriptManagementService>((IServiceFactory fac) => this._luaScriptManagementService.Value);
			container.Register<IConfigService>((IServiceFactory fac) => this._configService.Value);
			container.Register<IConsoleCommandsService>(delegate(IServiceFactory fac)
			{
				Func<IConsoleCommandsService> consoleCommandServiceFactory = this._consoleCommandServiceFactory;
				if (consoleCommandServiceFactory == null)
				{
					return null;
				}
				return consoleCommandServiceFactory();
			});
			return container;
		}

		// Token: 0x06003A26 RID: 14886 RVA: 0x00185514 File Offset: 0x00183714
		public Result<ImmutableArray<Type>> GetImplementingTypes<T>(bool includeInterfaces = false, bool includeAbstractTypes = false, bool includeDefaultContext = true)
		{
			PluginManagementService.<>c__DisplayClass45_0<T> CS$<>8__locals1;
			CS$<>8__locals1.includeInterfaces = includeInterfaces;
			CS$<>8__locals1.includeAbstractTypes = includeAbstractTypes;
			if (CS$<>8__locals1.includeInterfaces)
			{
				CS$<>8__locals1.includeAbstractTypes = true;
			}
			Result<ImmutableArray<Type>> result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				CS$<>8__locals1.builder = ImmutableArray.CreateBuilder<Type>();
				if (includeDefaultContext)
				{
					foreach (Assembly ass in AssemblyLoadContext.Default.Assemblies)
					{
						PluginManagementService.<GetImplementingTypes>g__AddTypesFromAssembly|45_0<T>(ass, ref CS$<>8__locals1);
					}
				}
				foreach (Assembly ass2 in (from al in this._assemblyLoaders.Values
				where !al.IsReferenceOnlyMode
				select al).SelectMany((IAssemblyLoaderService al) => al.Assemblies))
				{
					PluginManagementService.<GetImplementingTypes>g__AddTypesFromAssembly|45_0<T>(ass2, ref CS$<>8__locals1);
				}
				result = CS$<>8__locals1.builder.ToImmutable();
			}
			return result;
		}

		// Token: 0x06003A27 RID: 14887 RVA: 0x0018568C File Offset: 0x0018388C
		[MethodImpl(MethodImplOptions.NoInlining)]
		public bool TryGetPackageForPlugin<TPlugin>(out ContentPackage ownerPackage)
		{
			return this._pluginPackageLookup.TryGetValue(typeof(TPlugin), out ownerPackage);
		}

		// Token: 0x06003A28 RID: 14888 RVA: 0x001856A4 File Offset: 0x001838A4
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Type GetType(string typeName, bool isByRefType = false, bool includeInterfaces = false, bool includeDefaultContext = true)
		{
			if (typeName.StartsWith("out ") || typeName.StartsWith("ref "))
			{
				typeName = typeName.Remove(0, 4);
				isByRefType = true;
			}
			if (includeDefaultContext)
			{
				Type type = Type.GetType(typeName, false, false);
				if (type != null && (includeInterfaces || !type.IsInterface))
				{
					if (isByRefType)
					{
						return type.MakeByRefType();
					}
					return type;
				}
				else
				{
					foreach (Assembly ass in AssemblyLoadContext.Default.Assemblies)
					{
						Type type2 = ass.GetType(typeName, false, false);
						if (type2 != null && (includeInterfaces || !type2.IsInterface))
						{
							return isByRefType ? type2.MakeByRefType() : type2;
						}
					}
				}
			}
			foreach (Assembly ass2 in (from alc in AssemblyLoadContext.All
			where alc != AssemblyLoadContext.Default
			select alc).SelectMany((AssemblyLoadContext alc) => alc.Assemblies))
			{
				Type type3 = ass2.GetType(typeName, false, false);
				if (type3 != null && (includeInterfaces || !type3.IsInterface))
				{
					return isByRefType ? type3.MakeByRefType() : type3;
				}
			}
			return null;
		}

		// Token: 0x06003A29 RID: 14889 RVA: 0x0018581C File Offset: 0x00183A1C
		[MethodImpl(MethodImplOptions.NoOptimization)]
		public Result ActivatePluginInstances(ImmutableArray<ContentPackage> executionOrder, bool excludeAlreadyRunningPackages = true)
		{
			if (executionOrder.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentNullException("ActivatePluginInstances: The ececution list provided is empty.");
			}
			Result result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (this._assemblyLoaders.IsEmpty)
				{
					result = Result.Ok();
				}
				else
				{
					Result results = new Result();
					ImmutableArray<IGrouping<ContentPackage, Type>> toLoad = (from kvp in (from al in this._assemblyLoaders
					where executionOrder.Contains(al.Key)
					where !excludeAlreadyRunningPackages || !this._pluginInstances.ContainsKey(al.Key)
					select al).SelectMany((KeyValuePair<ContentPackage, IAssemblyLoaderService> al) => from ass in al.Value.Assemblies
					select new ValueTuple<ContentPackage, Assembly>(al.Key, ass)).SelectMany(delegate([TupleElementNames(new string[]
					{
						"Key",
						"ass"
					})] ValueTuple<ContentPackage, Assembly> kvp)
					{
						try
						{
							return from type in kvp.Item2.GetTypes()
							where type != null && !type.IsInterface && !type.IsAbstract && !type.IsGenericType && type.IsAssignableTo(typeof(IAssemblyPlugin))
							select new ValueTuple<ContentPackage, Type>(kvp.Item1, type);
						}
						catch (ReflectionTypeLoadException re)
						{
							results.WithError(new Error("Failed to get types from Package '" + kvp.Item1.Name + "'"));
							results.WithError(new ExceptionalError(re));
						}
						catch (Exception e2)
						{
							results.WithError(new Error("Failed to get types from Package '" + kvp.Item1.Name + "'"));
							results.WithError(new ExceptionalError(e2));
						}
						return new List<ValueTuple<ContentPackage, Type>>();
					})
					group kvp.Item2 by kvp.Item1 into exeGrp
					orderby executionOrder.IndexOf(exeGrp.Key)
					select exeGrp).ToImmutableArray<IGrouping<ContentPackage, Type>>();
					if (toLoad.Length == 0)
					{
						result = results;
					}
					else
					{
						this._logger.LogMessage("Activating IAssemblyPlugin instances", null, null);
						ImmutableArray<ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>>>.Builder loadedPackagePlugins = ImmutableArray.CreateBuilder<ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>>>();
						if (this._pluginInjectorContainer == null)
						{
							this._pluginInjectorContainer = this.CreatePluginServiceContainer();
						}
						foreach (IGrouping<ContentPackage, Type> packageTypes in toLoad)
						{
							ImmutableArray<IAssemblyPlugin>.Builder loadedTypes = ImmutableArray.CreateBuilder<IAssemblyPlugin>();
							foreach (Type pluginType in packageTypes)
							{
								try
								{
									this._logger.LogMessage("- Instantiating " + pluginType.Name, null, null);
									IAssemblyPlugin plugin = (IAssemblyPlugin)Activator.CreateInstance(pluginType);
									this._pluginInjectorContainer.InjectProperties(plugin);
									this._pluginInjectorContainer.Register(pluginType, (IServiceFactory fac) => plugin);
									loadedTypes.Add(plugin);
									this._pluginPackageLookup.TryAdd(pluginType, packageTypes.Key);
								}
								catch (Exception e)
								{
									results.WithError(new ExceptionalError("Failed to instantiate mod: " + packageTypes.Key.Name, e));
								}
							}
							loadedPackagePlugins.Add(new ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>>(packageTypes.Key, loadedTypes.ToImmutable()));
						}
						ImmutableArray<ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>>> packPluginGroups = loadedPackagePlugins.ToImmutable();
						foreach (ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>> packagePluginGrp in packPluginGroups)
						{
							ImmutableArray<IAssemblyPlugin> plugins;
							if (this._pluginInstances.TryGetValue(packagePluginGrp.Item1, out plugins))
							{
								this._pluginInstances[packagePluginGrp.Item1] = plugins.Concat(packagePluginGrp.Item2).ToImmutableArray<IAssemblyPlugin>();
							}
							else
							{
								this._pluginInstances[packagePluginGrp.Item1] = packagePluginGrp.Item2;
							}
						}
						ImmutableArray<IAssemblyPlugin> pluginsToInit = packPluginGroups.SelectMany(([TupleElementNames(new string[]
						{
							"Package",
							"Plugins"
						})] ValueTuple<ContentPackage, ImmutableArray<IAssemblyPlugin>> ppg) => ppg.Item2).ToImmutableArray<IAssemblyPlugin>();
						foreach (IAssemblyPlugin plugin4 in pluginsToInit)
						{
							results.WithReasons(PluginManagementService.<ActivatePluginInstances>g__PluginInitRunner|48_11(plugin4, delegate(IAssemblyPlugin p)
							{
								p.PreInitPatching();
							}).Reasons);
						}
						this._eventService.Value.PublishEvent<IEventPluginPreInitialize>(delegate(IEventPluginPreInitialize sub)
						{
							sub.PreInitPatching();
						});
						foreach (IAssemblyPlugin plugin2 in pluginsToInit)
						{
							results.WithReasons(PluginManagementService.<ActivatePluginInstances>g__PluginInitRunner|48_11(plugin2, delegate(IAssemblyPlugin p)
							{
								p.Initialize();
							}).Reasons);
						}
						this._eventService.Value.PublishEvent<IEventPluginInitialize>(delegate(IEventPluginInitialize sub)
						{
							sub.Initialize();
						});
						foreach (IAssemblyPlugin plugin3 in pluginsToInit)
						{
							results.WithReasons(PluginManagementService.<ActivatePluginInstances>g__PluginInitRunner|48_11(plugin3, delegate(IAssemblyPlugin p)
							{
								p.OnLoadCompleted();
							}).Reasons);
						}
						this._eventService.Value.PublishEvent<IEventPluginLoadCompleted>(delegate(IEventPluginLoadCompleted sub)
						{
							sub.OnLoadCompleted();
						});
						result = results;
					}
				}
			}
			return result;
		}

		// Token: 0x06003A2A RID: 14890 RVA: 0x00185DA4 File Offset: 0x00183FA4
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result LoadAssemblyResources(ImmutableArray<IAssemblyResourceInfo> resources)
		{
			PluginManagementService.<>c__DisplayClass49_0 CS$<>8__locals1 = new PluginManagementService.<>c__DisplayClass49_0();
			CS$<>8__locals1.resources = resources;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.resources.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentNullException("LoadAssemblyResources The resource list is empty.)");
			}
			Result result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._storageService.UseCaching = this._luaCsInfoProvider.UseCaching;
				if (!this._luaCsInfoProvider.UseCaching)
				{
					this._storageService.PurgeCache();
				}
				ImmutableArray<IGrouping<ContentPackage, IAssemblyResourceInfo>> orderedContentPacks = (from res in CS$<>8__locals1.resources
				group res by res.OwnerPackage into res
				orderby CS$<>8__locals1.resources.FindIndex((IAssemblyResourceInfo r2) => r2.OwnerPackage == res.Key)
				select res).ToImmutableArray<IGrouping<ContentPackage, IAssemblyResourceInfo>>();
				CS$<>8__locals1.result = new Result();
				foreach (IGrouping<ContentPackage, IAssemblyResourceInfo> contentPack in orderedContentPacks)
				{
					CS$<>8__locals1.<LoadAssemblyResources>g__LoadBinaries|2(contentPack);
					CS$<>8__locals1.<LoadAssemblyResources>g__LoadAndCompileScriptAssemblies|3(contentPack);
					foreach (Assembly ass in this._assemblyLoaders[contentPack.Key].Assemblies)
					{
						ReflectionUtils.AddNonAbstractAssemblyTypes(ass, false);
					}
				}
				result = CS$<>8__locals1.result;
			}
			return result;
		}

		// Token: 0x06003A2B RID: 14891 RVA: 0x00185F50 File Offset: 0x00184150
		private string DoSourceCodeTextCompatibilityPass(string sourceCode)
		{
			return sourceCode.Replace("GameMain.LuaCs", "LuaCsSetup.Instance").Replace(" Client.ClientList", " ModUtils.Client.ClientList").Replace(" Barotrauma.Networking.Client.ClientList", " ModUtils.Client.ClientList").Replace("ItemPrefab.GetItemPrefab", "ModUtils.ItemPrefab.GetItemPrefab");
		}

		// Token: 0x06003A2C RID: 14892 RVA: 0x00185F90 File Offset: 0x00184190
		private IntPtr OnAssemblyLoaderResolvingUnmanaged(Assembly callerAssembly, string targetAssemblyName)
		{
			Guard.IsNull<Assembly>(callerAssembly, "callerAssembly");
			Guard.IsNullOrWhiteSpace(targetAssemblyName, "targetAssemblyName");
			IAssemblyLoaderService loaderService = AssemblyLoadContext.GetLoadContext(callerAssembly) as IAssemblyLoaderService;
			if (loaderService == null)
			{
				return IntPtr.Zero;
			}
			string targetDirectory = Path.GetFullPath(loaderService.OwnerPackage.Dir);
			if (!targetAssemblyName.TrimEnd().EndsWith(".dll"))
			{
				targetAssemblyName += ".dll";
			}
			Result<ImmutableArray<string>> res = this._storageService.FindFilesInPackage(loaderService.OwnerPackage, string.Empty, targetAssemblyName, true);
			if (res.IsFailed || !res.Value.Any<string>())
			{
				return IntPtr.Zero;
			}
			foreach (string path in res.Value)
			{
				IntPtr asmPtr;
				if (NativeLibrary.TryLoad(path, out asmPtr))
				{
					this._loadedNativeLibraries.Add(asmPtr);
					return asmPtr;
				}
			}
			return IntPtr.Zero;
		}

		// Token: 0x06003A2D RID: 14893 RVA: 0x00186070 File Offset: 0x00184270
		private Assembly OnAssemblyLoaderResolvingManaged(IAssemblyLoaderService requestingLoader, AssemblyName searchName)
		{
			IService.CheckDisposed(this);
			IEnumerable<KeyValuePair<ContentPackage, IAssemblyLoaderService>> assemblyLoaders = this._assemblyLoaders;
			Func<KeyValuePair<ContentPackage, IAssemblyLoaderService>, bool> <>9__0;
			Func<KeyValuePair<ContentPackage, IAssemblyLoaderService>, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((KeyValuePair<ContentPackage, IAssemblyLoaderService> kvp) => kvp.Value != requestingLoader));
			}
			foreach (IAssemblyLoaderService loader in (from kvp in assemblyLoaders.Where(predicate)
			select kvp.Value).ToImmutableArray<IAssemblyLoaderService>())
			{
				if (!loader.IsReferenceOnlyMode && loader.Assemblies.Any<Assembly>())
				{
					foreach (Assembly assembly in loader.Assemblies)
					{
						if (assembly.GetName().FullName == searchName.FullName)
						{
							return assembly;
						}
					}
					continue;
				}
			}
			return null;
		}

		// Token: 0x06003A2E RID: 14894 RVA: 0x0018617C File Offset: 0x0018437C
		private void OnAssemblyLoaderUnloading(IAssemblyLoaderService loader)
		{
			if (!loader.Assemblies.Any<Assembly>())
			{
				return;
			}
			using (IEnumerator<Assembly> enumerator = loader.Assemblies.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Assembly assembly = enumerator.Current;
					Lazy<IEventService> eventService = this._eventService;
					if (eventService != null)
					{
						IEventService value = eventService.Value;
						if (value != null)
						{
							value.PublishEvent<IEventAssemblyUnloading>(delegate(IEventAssemblyUnloading sub)
							{
								sub.OnAssemblyUnloading(assembly);
							});
						}
					}
				}
			}
			this._unloadingAssemblyLoaders.Add(loader, loader.OwnerPackage);
		}

		// Token: 0x06003A2F RID: 14895 RVA: 0x00186218 File Offset: 0x00184418
		[MethodImpl(MethodImplOptions.NoOptimization)]
		public Result UnloadManagedAssemblies()
		{
			Result result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result results = new Result();
				if (!this._pluginInstances.IsEmpty)
				{
					foreach (IAssemblyPlugin instance in this._pluginInstances.SelectMany((KeyValuePair<ContentPackage, ImmutableArray<IAssemblyPlugin>> kvp) => kvp.Value))
					{
						try
						{
							instance.Dispose();
						}
						catch (Exception e)
						{
							results.WithError(new ExceptionalError(e));
						}
					}
					this._pluginInstances.Clear();
				}
				if (this._pluginEventService != null)
				{
					this._eventService.Value.RemoveDispatcherEventService(this._pluginEventService);
					try
					{
						this._pluginEventService.Dispose();
					}
					catch (Exception e2)
					{
						results.WithError(new ExceptionalError(e2));
					}
					this._pluginEventService = null;
				}
				try
				{
					ServiceContainer pluginInjectorContainer = this._pluginInjectorContainer;
					if (pluginInjectorContainer != null)
					{
						pluginInjectorContainer.Dispose();
					}
				}
				catch (Exception e3)
				{
					results.WithError(new ExceptionalError(e3));
				}
				this._pluginInjectorContainer = null;
				ReflectionUtils.ResetCache();
				foreach (KeyValuePair<ContentPackage, IAssemblyLoaderService> loaderService in this._assemblyLoaders)
				{
					try
					{
						loaderService.Value.Dispose();
					}
					catch (Exception e4)
					{
						results.WithError(new ExceptionalError(e4));
					}
				}
				this._assemblyLoaders.Clear();
				this._storageService.PurgeCache();
				this._pluginPackageLookup.Clear();
				if (this._loadedNativeLibraries.Any<IntPtr>())
				{
					foreach (IntPtr ptr in this._loadedNativeLibraries)
					{
						try
						{
							NativeLibrary.Free(ptr);
						}
						catch
						{
						}
					}
					this._loadedNativeLibraries.Clear();
				}
				Task.Factory.StartNew<Task>(delegate()
				{
					PluginManagementService.<<UnloadManagedAssemblies>b__54_0>d <<UnloadManagedAssemblies>b__54_0>d;
					<<UnloadManagedAssemblies>b__54_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
					<<UnloadManagedAssemblies>b__54_0>d.<>4__this = this;
					<<UnloadManagedAssemblies>b__54_0>d.<>1__state = -1;
					<<UnloadManagedAssemblies>b__54_0>d.<>t__builder.Start<PluginManagementService.<<UnloadManagedAssemblies>b__54_0>d>(ref <<UnloadManagedAssemblies>b__54_0>d);
					return <<UnloadManagedAssemblies>b__54_0>d.<>t__builder.Task;
				});
				result = results;
			}
			return result;
		}

		// Token: 0x06003A30 RID: 14896 RVA: 0x00186520 File Offset: 0x00184720
		private void SafeLogUnloadingPackages()
		{
			if (!this._unloadingAssemblyLoaders.Any<KeyValuePair<IAssemblyLoaderService, ContentPackage>>())
			{
				return;
			}
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("The following ContentPackages have not unloaded their assemblies:");
			foreach (KeyValuePair<IAssemblyLoaderService, ContentPackage> kvp in this._unloadingAssemblyLoaders.ToImmutableArray<KeyValuePair<IAssemblyLoaderService, ContentPackage>>())
			{
				StringBuilder stringBuilder = sb;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral("- '");
				appendInterpolatedStringHandler.AppendFormatted(kvp.Value.Name);
				appendInterpolatedStringHandler.AppendLiteral("'");
				stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
			}
			if (this._logger == null)
			{
				DebugConsole.Log(sb.ToString());
				return;
			}
			this._logger.LogWarning(sb.ToString());
		}

		// Token: 0x06003A31 RID: 14897 RVA: 0x001865DC File Offset: 0x001847DC
		private void GCCleanupTask(TaskCompletionSource<bool> completionSuccess)
		{
			GC.RegisterForFullGCNotification(1, 1);
			try
			{
				for (int iter = 0; iter < PluginManagementService.GC_BACKGND_MAXITERATIONS; iter++)
				{
					int maxGen = GC.MaxGeneration;
					for (int currGen = 0; currGen < maxGen; currGen++)
					{
						GC.Collect(currGen, GCCollectionMode.Forced, false, false);
						GC.WaitForFullGCComplete(PluginManagementService.GC_BACKGND_GENERATION_WAIT_MILLIS);
						GC.Collect(currGen);
					}
					Thread.Sleep(PluginManagementService.GC_BACKGND_INTERVAL_MILLIS);
				}
				completionSuccess.SetResult(true);
			}
			catch (ThreadInterruptedException tie)
			{
				completionSuccess.SetResult(false);
			}
			catch (Exception e)
			{
				completionSuccess.SetException(e);
			}
			finally
			{
				GC.CancelFullGCNotification();
			}
		}

		// Token: 0x06003A32 RID: 14898 RVA: 0x00186684 File Offset: 0x00184884
		private Task RunGC(bool logResults, bool runOnMainThread)
		{
			PluginManagementService.<RunGC>d__57 <RunGC>d__;
			<RunGC>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<RunGC>d__.<>4__this = this;
			<RunGC>d__.logResults = logResults;
			<RunGC>d__.runOnMainThread = runOnMainThread;
			<RunGC>d__.<>1__state = -1;
			<RunGC>d__.<>t__builder.Start<PluginManagementService.<RunGC>d__57>(ref <RunGC>d__);
			return <RunGC>d__.<>t__builder.Task;
		}

		// Token: 0x06003A33 RID: 14899 RVA: 0x001866D8 File Offset: 0x001848D8
		public Result<Assembly> GetLoadedAssembly(OneOf<AssemblyName, string> assemblyName, in Guid[] excludedContexts)
		{
			Result<Assembly> result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guid[] guids = excludedContexts;
				Func<IAssemblyLoaderService, bool> <>9__2;
				result = assemblyName.Match<Assembly>(delegate(AssemblyName asm)
				{
					IEnumerable<IAssemblyLoaderService> values = this._assemblyLoaders.Values;
					Func<IAssemblyLoaderService, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((IAssemblyLoaderService al) => guids.Length == 0 || !guids.Contains(al.Id)));
					}
					foreach (Assembly ass in values.Where(predicate).SelectMany((IAssemblyLoaderService al) => al.Assemblies).ToImmutableArray<Assembly>())
					{
						if (ass.GetName() == asm)
						{
							return ass;
						}
					}
					return null;
				}, delegate(string asmName)
				{
					foreach (Assembly ass in this._assemblyLoaders.Values.SelectMany((IAssemblyLoaderService al) => al.Assemblies))
					{
						string name = ass.GetName().Name;
						if ((name != null) ? name.Equals(asmName) : ass.GetName().FullName.Equals(asmName))
						{
							return ass;
						}
					}
					return null;
				});
			}
			return result;
		}

		// Token: 0x06003A35 RID: 14901 RVA: 0x0018687A File Offset: 0x00184A7A
		Result<Assembly> IAssemblyManagementService.GetLoadedAssembly(OneOf<AssemblyName, string> assemblyName, in Guid[] excludedContexts)
		{
			return this.GetLoadedAssembly(assemblyName, excludedContexts);
		}

		// Token: 0x06003A3F RID: 14911 RVA: 0x00186958 File Offset: 0x00184B58
		[CompilerGenerated]
		internal static void <GetImplementingTypes>g__AddTypesFromAssembly|45_0<T>(Assembly assembly, ref PluginManagementService.<>c__DisplayClass45_0<T> A_1)
		{
			foreach (Type type in assembly.GetSafeTypes())
			{
				if ((A_1.includeInterfaces || !type.IsInterface) && (A_1.includeAbstractTypes || !type.IsAbstract) && type.IsAssignableTo(typeof(T)))
				{
					A_1.builder.Add(type);
				}
			}
		}

		// Token: 0x06003A40 RID: 14912 RVA: 0x001869DC File Offset: 0x00184BDC
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.NoOptimization)]
		internal static Result <ActivatePluginInstances>g__PluginInitRunner|48_11(IAssemblyPlugin plugin, Action<IAssemblyPlugin> action)
		{
			Result result;
			try
			{
				action(plugin);
				result = Result.Ok();
			}
			catch (Exception e)
			{
				result = Result.Fail(new ExceptionalError(e));
			}
			return result;
		}

		// Token: 0x04001D1C RID: 7452
		private static readonly CSharpParseOptions ScriptParseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols(new string[]
		{
			"SERVER"
		});

		// Token: 0x04001D1D RID: 7453
		private const string PLATFORM_TARGET = "Windows";

		// Token: 0x04001D1E RID: 7454
		private const string ARCHITECTURE_TARGET = "Server";

		// Token: 0x04001D1F RID: 7455
		private static readonly CSharpCompilationOptions CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Debug, false, false, null, null, default(ImmutableArray<byte>), null, Microsoft.CodeAnalysis.Platform.AnyCpu, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false, MetadataImportOptions.Public, NullableContextOptions.Disable).WithMetadataImportOptions(MetadataImportOptions.All).WithOptimizationLevel(OptimizationLevel.Release).WithAllowUnsafe(true);

		// Token: 0x04001D20 RID: 7456
		private static readonly SyntaxTree BaseAssemblyImports = CSharpSyntaxTree.ParseText(new StringBuilder().AppendLine("global using LuaCsHook = Barotrauma.LuaCs.Compatibility.ILuaCsHook;").AppendLine("global using System.Reflection;").AppendLine("global using Barotrauma;").AppendLine("global using Barotrauma.LuaCs;").AppendLine("global using Barotrauma.LuaCs.Compatibility;").AppendLine("using System.Runtime.CompilerServices;").AppendLine("[assembly: IgnoresAccessChecksTo(\"BarotraumaCore\")]").AppendLine("[assembly: IgnoresAccessChecksTo(\"DedicatedServer\")]").ToString(), PluginManagementService.ScriptParseOptions, "", null, default(CancellationToken));

		// Token: 0x04001D21 RID: 7457
		private ImmutableArray<MetadataReference> _baseMetadataReferences = ImmutableArray<MetadataReference>.Empty;

		// Token: 0x04001D22 RID: 7458
		private ImmutableArray<MetadataReference> _baseMetadataReferencesNonPublicized = ImmutableArray<MetadataReference>.Empty;

		// Token: 0x04001D23 RID: 7459
		private Thread _backgroundGCCleanupThread;

		// Token: 0x04001D24 RID: 7460
		private long _backgroundGCWatchdogTicks;

		// Token: 0x04001D25 RID: 7461
		private static readonly int GC_TASK_COMPLETION_TIMEOUT = 5000;

		// Token: 0x04001D26 RID: 7462
		private static readonly int GC_BACKGND_MAXITERATIONS = 2;

		// Token: 0x04001D27 RID: 7463
		private static readonly int GC_BACKGND_INTERVAL_MILLIS = 200;

		// Token: 0x04001D28 RID: 7464
		private static readonly int GC_BACKGND_GENERATION_WAIT_MILLIS = 100;

		// Token: 0x04001D29 RID: 7465
		private int _isDisposed;

		// Token: 0x04001D2A RID: 7466
		private IAssemblyLoaderService.IFactory _assemblyLoaderFactory;

		// Token: 0x04001D2B RID: 7467
		private IStorageService _storageService;

		// Token: 0x04001D2C RID: 7468
		private ILoggerService _logger;

		// Token: 0x04001D2D RID: 7469
		private Lazy<IEventService> _eventService;

		// Token: 0x04001D2E RID: 7470
		private Lazy<IConfigService> _configService;

		// Token: 0x04001D2F RID: 7471
		private Lazy<ILuaScriptManagementService> _luaScriptManagementService;

		// Token: 0x04001D30 RID: 7472
		private IEventService _pluginEventService;

		// Token: 0x04001D31 RID: 7473
		private Lazy<ILuaPatcher> _pluginLuaPatcherService;

		// Token: 0x04001D32 RID: 7474
		private Func<IConsoleCommandsService> _consoleCommandServiceFactory;

		// Token: 0x04001D33 RID: 7475
		private readonly IConsoleCommandsService _internalConsoleCommandsService;

		// Token: 0x04001D34 RID: 7476
		private ILuaCsInfoProvider _luaCsInfoProvider;

		// Token: 0x04001D35 RID: 7477
		private readonly ConcurrentDictionary<ContentPackage, IAssemblyLoaderService> _assemblyLoaders = new ConcurrentDictionary<ContentPackage, IAssemblyLoaderService>();

		// Token: 0x04001D36 RID: 7478
		private readonly ConcurrentDictionary<Type, ContentPackage> _pluginPackageLookup = new ConcurrentDictionary<Type, ContentPackage>();

		// Token: 0x04001D37 RID: 7479
		private readonly ConcurrentDictionary<ContentPackage, ImmutableArray<IAssemblyPlugin>> _pluginInstances = new ConcurrentDictionary<ContentPackage, ImmutableArray<IAssemblyPlugin>>();

		// Token: 0x04001D38 RID: 7480
		private readonly ConditionalWeakTable<IAssemblyLoaderService, ContentPackage> _unloadingAssemblyLoaders = new ConditionalWeakTable<IAssemblyLoaderService, ContentPackage>();

		// Token: 0x04001D39 RID: 7481
		private readonly ConcurrentBag<IntPtr> _loadedNativeLibraries = new ConcurrentBag<IntPtr>();

		// Token: 0x04001D3A RID: 7482
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04001D3B RID: 7483
		private ServiceContainer _pluginInjectorContainer;
	}
}
