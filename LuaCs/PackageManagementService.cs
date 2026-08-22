using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Barotrauma.Extensions;
using Barotrauma.LuaCs.Data;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000504 RID: 1284
	public sealed class PackageManagementService : IPackageManagementService, IReusableService, IService, IDisposable
	{
		// Token: 0x06005326 RID: 21286 RVA: 0x002C7E84 File Offset: 0x002C6084
		public PackageManagementService(ILoggerService logger, IModConfigService modConfigService, ILuaScriptManagementService luaScriptManagementService, IPluginManagementService pluginManagementService, IConfigService configService, IConsoleCommandsService commandsService, IUIStylesService uiStylesService, IPackageManagementServiceConfig runConfig)
		{
			this._logger = logger;
			this._modConfigService = modConfigService;
			this._luaScriptManagementService = luaScriptManagementService;
			this._pluginManagementService = pluginManagementService;
			this._configService = configService;
			this._runConfig = runConfig;
			this._uiStylesService = uiStylesService;
			this._commandsService = commandsService;
			commandsService.RegisterCommand("pms_getxmlname", "Gets the XML encoded name for the given package, as used in localization.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					this._logger.LogError("Please specify the name of the package.");
					return;
				}
				ContentPackage pkg = ContentPackageManager.AllPackages.FirstOrDefault((ContentPackage p) => p.Name == args[0]);
				if (pkg != null)
				{
					this._logger.Log("Package Xml Name: '" + XmlConvert.EncodeLocalName(pkg.Name) + "'", null, ServerLog.MessageType.ServerMessage);
					return;
				}
				this._logger.Log("Could not find package with the name '" + args[0] + "'", null, ServerLog.MessageType.ServerMessage);
			}, delegate
			{
				string[][] array = new string[1][];
				array[0] = (from p in this._loadedPackages.Keys
				select p.Name).ToArray<string>();
				return array;
			}, false);
		}

		// Token: 0x06005327 RID: 21287 RVA: 0x002C7F38 File Offset: 0x002C6138
		public void Dispose()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this._logger.LogMessage("PackageManagementService is disposing.", null, null);
					this._luaScriptManagementService.Dispose();
					this._pluginManagementService.Dispose();
					this._modConfigService.Dispose();
					this._logger.Dispose();
					this._uiStylesService.Dispose();
					this._logger = null;
					this._luaScriptManagementService = null;
					this._pluginManagementService = null;
					this._modConfigService = null;
					this._uiStylesService = null;
					this._loadedPackages.Clear();
					this._runningPackages.Clear();
				}
			}
		}

		// Token: 0x170014FB RID: 5371
		// (get) Token: 0x06005328 RID: 21288 RVA: 0x002C8034 File Offset: 0x002C6234
		// (set) Token: 0x06005329 RID: 21289 RVA: 0x002C8041 File Offset: 0x002C6241
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x0600532A RID: 21290 RVA: 0x002C8050 File Offset: 0x002C6250
		public Result Reset()
		{
			Result result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (this.IsDisposed)
				{
					result = Result.Fail("PackageManagementServicefailed to reset. Has already been disposed.");
				}
				else
				{
					try
					{
						Result operationResult = new Result();
						operationResult.WithReasons(this._luaScriptManagementService.Reset().Reasons);
						operationResult.WithReasons(this._pluginManagementService.Reset().Reasons);
						operationResult.WithReasons(this._configService.Reset().Reasons);
						operationResult.WithReasons(this._uiStylesService.Reset().Reasons);
						this._runningPackages.Clear();
						this._loadedPackages.Clear();
						this._packageNameCache.Clear();
						result = operationResult;
					}
					catch (Exception e)
					{
						result = Result.Fail(new ExceptionalError(e));
					}
				}
			}
			return result;
		}

		// Token: 0x0600532B RID: 21291 RVA: 0x002C816C File Offset: 0x002C636C
		public bool TryGetLoadedPackageByName(string name, out ContentPackage package)
		{
			package = null;
			if (name.IsNullOrWhiteSpace())
			{
				return false;
			}
			bool result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				result = this._packageNameCache.TryGetValue(name, out package);
			}
			return result;
		}

		// Token: 0x0600532C RID: 21292 RVA: 0x002C81E0 File Offset: 0x002C63E0
		public Result LoadPackageInfo(ContentPackage package)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Result result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					IModConfigInfo result;
					if (this._loadedPackages.TryGetValue(package, out result))
					{
						this._logger.LogWarning("LoadPackageInfo: Tried to load already-loaded package " + package.Name + ".");
						result2 = Result.Ok();
					}
					else
					{
						Result<IModConfigInfo> pkgCfgInfo = this._modConfigService.CreateConfigAsync(package).ConfigureAwait(false).GetAwaiter().GetResult();
						if (pkgCfgInfo.IsFailed)
						{
							this._logger.LogResults(pkgCfgInfo.ToResult());
							result2 = pkgCfgInfo.ToResult();
						}
						else
						{
							result2 = this.UnsafeAddPackageInternal(package, pkgCfgInfo.Value);
						}
					}
				}
			}
			return result2;
		}

		// Token: 0x0600532D RID: 21293 RVA: 0x002C8320 File Offset: 0x002C6520
		public Result LoadPackagesInfo(ImmutableArray<ContentPackage> packages)
		{
			if (packages.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentException("LoadPackagesInfo: packages list is empty.");
			}
			Result result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					Result result = new Result();
					ImmutableArray<ContentPackage> packages2 = (from pkg in packages
					orderby (!(pkg.Name == "LuaCsForBarotrauma")) ? 1 : 0
					select pkg).ThenBy(new Func<ContentPackage, int>(packages.IndexOf)).ToImmutableArray<ContentPackage>();
					foreach (ValueTuple<ContentPackage, Result<IModConfigInfo>> pkgConfig in this._modConfigService.CreateConfigsAsync(ImmutableCollectionsMarshal.AsImmutableArray<ContentPackage>(packages2.AsSpan().ToArray())).ConfigureAwait(false).GetAwaiter().GetResult())
					{
						result.WithReasons(pkgConfig.Item2.Reasons);
						if (pkgConfig.Item2.IsSuccess)
						{
							result.WithReasons(this.UnsafeAddPackageInternal(pkgConfig.Item1, pkgConfig.Item2.Value).Reasons);
						}
					}
					result2 = result;
				}
			}
			return result2;
		}

		// Token: 0x0600532E RID: 21294 RVA: 0x002C84DC File Offset: 0x002C66DC
		private Result UnsafeAddPackageInternal(ContentPackage package, IModConfigInfo config)
		{
			PackageManagementService.<>c__DisplayClass23_0 CS$<>8__locals1 = new PackageManagementService.<>c__DisplayClass23_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.config = config;
			IModConfigInfo modConfigInfo;
			if (this._loadedPackages.TryGetValue(package, out modConfigInfo))
			{
				this._logger.LogWarning("Tried to load already-loaded package " + package.Name + ".");
				return Result.Ok();
			}
			foreach (IAssemblyResourceInfo info in CS$<>8__locals1.config.Assemblies)
			{
				PackageManagementService.<UnsafeAddPackageInternal>g__TouchMeFullPaths|23_0(info);
			}
			foreach (IConfigResourceInfo info2 in CS$<>8__locals1.config.Configs)
			{
				PackageManagementService.<UnsafeAddPackageInternal>g__TouchMeFullPaths|23_0(info2);
			}
			foreach (ILuaScriptResourceInfo info3 in CS$<>8__locals1.config.LuaScripts)
			{
				PackageManagementService.<UnsafeAddPackageInternal>g__TouchMeFullPaths|23_0(info3);
			}
			this._loadedPackages[package] = CS$<>8__locals1.config;
			this._packageNameCache[package.Name] = package;
			Result result;
			try
			{
				Result res = new Result();
				ImmutableArray<Task<Task<Result>>>.Builder tasks = ImmutableArray.CreateBuilder<Task<Task<Result>>>();
				if (!CS$<>8__locals1.config.Configs.IsDefaultOrEmpty)
				{
					tasks.Add(Task.Factory.StartNew<Task<Result>>(delegate()
					{
						PackageManagementService.<>c__DisplayClass23_0.<<UnsafeAddPackageInternal>b__1>d <<UnsafeAddPackageInternal>b__1>d;
						<<UnsafeAddPackageInternal>b__1>d.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
						<<UnsafeAddPackageInternal>b__1>d.<>4__this = CS$<>8__locals1;
						<<UnsafeAddPackageInternal>b__1>d.<>1__state = -1;
						<<UnsafeAddPackageInternal>b__1>d.<>t__builder.Start<PackageManagementService.<>c__DisplayClass23_0.<<UnsafeAddPackageInternal>b__1>d>(ref <<UnsafeAddPackageInternal>b__1>d);
						return <<UnsafeAddPackageInternal>b__1>d.<>t__builder.Task;
					}));
				}
				if (!CS$<>8__locals1.config.LuaScripts.IsDefaultOrEmpty)
				{
					tasks.Add(Task.Factory.StartNew<Task<Result>>(delegate()
					{
						PackageManagementService.<>c__DisplayClass23_0.<<UnsafeAddPackageInternal>b__2>d <<UnsafeAddPackageInternal>b__2>d;
						<<UnsafeAddPackageInternal>b__2>d.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
						<<UnsafeAddPackageInternal>b__2>d.<>4__this = CS$<>8__locals1;
						<<UnsafeAddPackageInternal>b__2>d.<>1__state = -1;
						<<UnsafeAddPackageInternal>b__2>d.<>t__builder.Start<PackageManagementService.<>c__DisplayClass23_0.<<UnsafeAddPackageInternal>b__2>d>(ref <<UnsafeAddPackageInternal>b__2>d);
						return <<UnsafeAddPackageInternal>b__2>d.<>t__builder.Task;
					}));
				}
				if (tasks.Count == 0)
				{
					result = Result.Ok();
				}
				else
				{
					if (!CS$<>8__locals1.config.Styles.IsDefaultOrEmpty)
					{
						res.WithReasons(this._uiStylesService.LoadAssets(CS$<>8__locals1.config.Styles).Reasons);
					}
					Task<Result>[] r = Task.WhenAll<Task<Result>>(tasks.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();
					foreach (Task<Result> task in r)
					{
						res.WithReasons(task.ConfigureAwait(false).GetAwaiter().GetResult().Reasons);
					}
					result = res;
				}
			}
			catch (Exception e)
			{
				result = Result.Fail(new ExceptionalError(e));
			}
			return result;
		}

		// Token: 0x0600532F RID: 21295 RVA: 0x002C874C File Offset: 0x002C694C
		public Result ExecuteLoadedPackages(ImmutableArray<ContentPackage> executionOrder, bool executeCsAssemblies)
		{
			Result result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					if (executionOrder.IsDefaultOrEmpty)
					{
						result2 = Result.Fail("ExecuteLoadedPackages: No packages in the execution order list.");
					}
					else if (!this._runningPackages.IsEmpty)
					{
						result2 = Result.Fail("ExecuteLoadedPackages: There are already packages running! List: " + this._runningPackages.Aggregate(string.Empty, (string acc, KeyValuePair<ContentPackage, IModConfigInfo> kvp) => "-" + kvp.ToString() + "\n" + kvp.Key.Name));
					}
					else if (this._loadedPackages.IsEmpty)
					{
						result2 = Result.Fail("ExecuteLoadedPackages: No packages loaded. Nothing to run!)");
					}
					else
					{
						Result result = new Result();
						ImmutableArray<KeyValuePair<ContentPackage, IModConfigInfo>> loadingOrderedPackages = (from pkg in this._loadedPackages
						orderby (!(pkg.Key.Name == "LuaCsForBarotrauma")) ? 1 : 0, executionOrder.IndexOf(pkg.Key)
						select pkg).ToImmutableArray<KeyValuePair<ContentPackage, IModConfigInfo>>();
						ImmutableArray<ContentPackage> loadOrderByPackage = (from p in loadingOrderedPackages
						select p.Key).ToImmutableArray<ContentPackage>();
						ImmutableHashSet<Identifier> toLoadPackagesIndents = loadingOrderedPackages.SelectMany((KeyValuePair<ContentPackage, IModConfigInfo> p) => p.Key.AltNames.Union(new string[]
						{
							p.Key.Name
						}).ToIdentifiers()).ToImmutableHashSet<Identifier>();
						if (executeCsAssemblies)
						{
							ImmutableArray<IAssemblyResourceInfo> plugins = PackageManagementService.SelectCompatible<IAssemblyResourceInfo>(loadingOrderedPackages.SelectMany((KeyValuePair<ContentPackage, IModConfigInfo> pkg) => pkg.Value.Assemblies).ToImmutableArray<IAssemblyResourceInfo>(), toLoadPackagesIndents, loadOrderByPackage);
							if (!plugins.IsDefaultOrEmpty)
							{
								result.WithReasons(this._pluginManagementService.LoadAssemblyResources(plugins).Reasons);
								result.WithReasons(this._pluginManagementService.ActivatePluginInstances((from p in plugins
								select p.OwnerPackage).ToImmutableArray<ContentPackage>(), false).Reasons);
							}
						}
						ImmutableArray<ILuaScriptResourceInfo> luaScripts = PackageManagementService.SelectCompatible<ILuaScriptResourceInfo>(loadingOrderedPackages.Where(delegate(KeyValuePair<ContentPackage, IModConfigInfo> pkg)
						{
							if (!executeCsAssemblies)
							{
								return !pkg.Value.LuaScripts.Any((ILuaScriptResourceInfo scr) => scr.RunUnrestricted);
							}
							return true;
						}).SelectMany((KeyValuePair<ContentPackage, IModConfigInfo> pkg) => pkg.Value.LuaScripts).ToImmutableArray<ILuaScriptResourceInfo>(), toLoadPackagesIndents, loadOrderByPackage);
						if (!luaScripts.IsDefaultOrEmpty)
						{
							result.WithReasons(this._luaScriptManagementService.ExecuteLoadedScripts(luaScripts, !executeCsAssemblies).Reasons);
						}
						foreach (KeyValuePair<ContentPackage, IModConfigInfo> package in loadingOrderedPackages)
						{
							this._runningPackages[package.Key] = package.Value;
						}
						result2 = result;
					}
				}
			}
			return result2;
		}

		// Token: 0x06005330 RID: 21296 RVA: 0x002C8AA8 File Offset: 0x002C6CA8
		private static ImmutableArray<T> SelectCompatible<T>(ImmutableArray<T> resources, ImmutableHashSet<Identifier> enabledPackagesIdents, ImmutableArray<ContentPackage> loadingOrder) where T : IBaseResourceInfo
		{
			return (from r in resources
			where r.SupportedPlatforms.HasFlag(ModUtils.Environment.CurrentPlatform)
			where r.SupportedTargets.HasFlag(ModUtils.Environment.CurrentTarget)
			where !r.Optional || ((r.RequiredPackages.IsDefaultOrEmpty || enabledPackagesIdents.Intersect(r.RequiredPackages).Any<Identifier>()) && (r.IncompatiblePackages.IsDefaultOrEmpty || enabledPackagesIdents.Intersect(r.IncompatiblePackages).None(null)))
			orderby (r.Optional > false) ? 1 : 0, loadingOrder.IndexOf(r.OwnerPackage), r.LoadPriority
			select r).ToImmutableArray<T>();
		}

		// Token: 0x06005331 RID: 21297 RVA: 0x002C8B84 File Offset: 0x002C6D84
		public Result SyncLoadedPackagesList(ImmutableArray<ContentPackage> packages)
		{
			if (packages.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentNullException("packages");
			}
			if (!this._runningPackages.IsEmpty)
			{
				ThrowHelper.ThrowInvalidOperationException("SyncLoadedPackagesList: There are packages running!");
			}
			ImmutableArray<ContentPackage> toRemove = this._loadedPackages.Keys.Except(packages).ToImmutableArray<ContentPackage>();
			ImmutableArray<ContentPackage> toAdd = (from pack in packages.Except(this._loadedPackages.Keys)
			orderby packages.IndexOf(pack)
			select pack).ToImmutableArray<ContentPackage>();
			Result result = new Result();
			if (!toRemove.IsDefaultOrEmpty)
			{
				result.WithReasons(this.UnloadPackages(toRemove).Reasons);
			}
			if (!toAdd.IsDefaultOrEmpty)
			{
				result.WithReasons(this.LoadPackagesInfo(toAdd).Reasons);
			}
			return result;
		}

		// Token: 0x06005332 RID: 21298 RVA: 0x002C8C60 File Offset: 0x002C6E60
		public Result StopRunningPackages()
		{
			Result result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					if (this._loadedPackages.IsEmpty || this._runningPackages.IsEmpty)
					{
						result = Result.Ok();
					}
					else
					{
						Result res = new Result();
						res.WithReasons(this._luaScriptManagementService.UnloadActiveScripts().Reasons);
						res.WithReasons(this._pluginManagementService.UnloadManagedAssemblies().Reasons);
						this._runningPackages.Clear();
						result = res;
					}
				}
			}
			return result;
		}

		// Token: 0x06005333 RID: 21299 RVA: 0x002C8D68 File Offset: 0x002C6F68
		public Result UnloadPackage(ContentPackage package)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Result result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					if (!this._loadedPackages.ContainsKey(package))
					{
						result2 = Result.Fail("UnloadPackage: The package is not loaded.");
					}
					else if (!this._runningPackages.IsEmpty)
					{
						result2 = Result.Fail("UnloadPackage: Packages are currently executing.");
					}
					else
					{
						Result result = new Result();
						result.WithReasons(this._luaScriptManagementService.DisposePackageResources(package).Reasons);
						result.WithReasons(this._configService.DisposePackageData(package).Reasons);
						result.WithReasons(this._uiStylesService.UnloadPackage(package).Reasons);
						IModConfigInfo modConfigInfo;
						this._loadedPackages.TryRemove(package, out modConfigInfo);
						ContentPackage contentPackage;
						this._packageNameCache.TryRemove(package.Name, out contentPackage);
						result2 = result;
					}
				}
			}
			return result2;
		}

		// Token: 0x06005334 RID: 21300 RVA: 0x002C8EC8 File Offset: 0x002C70C8
		public Result UnloadPackages(ImmutableArray<ContentPackage> packages)
		{
			if (packages.IsDefaultOrEmpty)
			{
				return Result.Fail("UnloadPackages: Package list is empty.");
			}
			Result result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					Result result = new Result();
					foreach (ContentPackage package in packages)
					{
						result.WithReasons(this.UnloadPackage(package).Reasons);
					}
					result2 = result;
				}
			}
			return result2;
		}

		// Token: 0x06005335 RID: 21301 RVA: 0x002C8FB8 File Offset: 0x002C71B8
		public Result UnloadAllPackages()
		{
			Result result2;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._executionLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					if (this._loadedPackages.IsEmpty)
					{
						result2 = Result.Ok();
					}
					else if (!this._runningPackages.IsEmpty)
					{
						result2 = Result.Fail("UnloadAllPackages: Packages are currently executing.");
					}
					else
					{
						Result result = new Result();
						result.WithReasons(this._luaScriptManagementService.DisposeAllPackageResources().Reasons);
						result.WithReasons(this._configService.DisposeAllPackageData().Reasons);
						this._loadedPackages.Clear();
						result2 = result;
					}
				}
			}
			return result2;
		}

		// Token: 0x06005336 RID: 21302 RVA: 0x002C90CC File Offset: 0x002C72CC
		public ImmutableArray<ContentPackage> GetAllLoadedPackages()
		{
			ImmutableArray<ContentPackage> result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				result = ImmutableCollectionsMarshal.AsImmutableArray<ContentPackage>(this._loadedPackages.Keys.ToArray<ContentPackage>());
			}
			return result;
		}

		// Token: 0x06005337 RID: 21303 RVA: 0x002C9140 File Offset: 0x002C7340
		public bool IsPackageRunning(ContentPackage package)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			bool result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				result = this._runningPackages.ContainsKey(package);
			}
			return result;
		}

		// Token: 0x06005338 RID: 21304 RVA: 0x002C91B8 File Offset: 0x002C73B8
		public bool IsAnyPackageLoaded()
		{
			bool result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				result = !this._loadedPackages.IsEmpty;
			}
			return result;
		}

		// Token: 0x06005339 RID: 21305 RVA: 0x002C9224 File Offset: 0x002C7424
		public bool IsAnyPackageRunning()
		{
			bool result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				result = !this._runningPackages.IsEmpty;
			}
			return result;
		}

		// Token: 0x0600533A RID: 21306 RVA: 0x002C9290 File Offset: 0x002C7490
		public ImmutableArray<ContentPackage> GetLoadedUnrestrictedPackages()
		{
			ImmutableArray<ContentPackage> result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (this._loadedPackages.IsEmpty)
				{
					result = ImmutableArray<ContentPackage>.Empty;
				}
				else
				{
					result = ImmutableCollectionsMarshal.AsImmutableArray<ContentPackage>((from cfg in this._loadedPackages.Values.Where(delegate(IModConfigInfo cfg)
					{
						if (cfg.Assemblies.IsDefaultOrEmpty)
						{
							return cfg.LuaScripts.Any((ILuaScriptResourceInfo scr) => scr.RunUnrestricted);
						}
						return true;
					})
					select cfg.Package).ToArray<ContentPackage>());
				}
			}
			return result;
		}

		// Token: 0x0600533B RID: 21307 RVA: 0x002C9364 File Offset: 0x002C7564
		public bool PackageContainsAnyRunnableResource(ContentPackage package)
		{
			bool result2;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<IModConfigInfo> result = this.GetModConfigForPackage(package);
				if (result.IsSuccess)
				{
					result2 = (result.Value.Assemblies.Any<IAssemblyResourceInfo>() || result.Value.LuaScripts.Any<ILuaScriptResourceInfo>());
				}
				else
				{
					result2 = false;
				}
			}
			return result2;
		}

		// Token: 0x0600533C RID: 21308 RVA: 0x002C9400 File Offset: 0x002C7600
		public Result<IModConfigInfo> GetModConfigForPackage(ContentPackage package)
		{
			Result<IModConfigInfo> result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				IModConfigInfo modConfig;
				if (!this._loadedPackages.TryGetValue(package, out modConfig))
				{
					result = Result.Fail("Failed to find mod config for package " + package.Name);
				}
				else
				{
					result = new Result<IModConfigInfo>().WithValue(modConfig);
				}
			}
			return result;
		}

		// Token: 0x0600533F RID: 21311 RVA: 0x002C9584 File Offset: 0x002C7784
		[CompilerGenerated]
		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
		internal static void <UnsafeAddPackageInternal>g__TouchMeFullPaths|23_0(IBaseResourceInfo info)
		{
			foreach (ContentPath contentPath in info.FilePaths)
			{
				string s = contentPath.FullPath;
			}
		}

		// Token: 0x04002BF2 RID: 11250
		private ILoggerService _logger;

		// Token: 0x04002BF3 RID: 11251
		private IModConfigService _modConfigService;

		// Token: 0x04002BF4 RID: 11252
		private IConfigService _configService;

		// Token: 0x04002BF5 RID: 11253
		private ILuaScriptManagementService _luaScriptManagementService;

		// Token: 0x04002BF6 RID: 11254
		private IPluginManagementService _pluginManagementService;

		// Token: 0x04002BF7 RID: 11255
		private IConsoleCommandsService _commandsService;

		// Token: 0x04002BF8 RID: 11256
		private IUIStylesService _uiStylesService;

		// Token: 0x04002BF9 RID: 11257
		private IPackageManagementServiceConfig _runConfig;

		// Token: 0x04002BFA RID: 11258
		private readonly ConcurrentDictionary<ContentPackage, IModConfigInfo> _loadedPackages = new ConcurrentDictionary<ContentPackage, IModConfigInfo>();

		// Token: 0x04002BFB RID: 11259
		private readonly ConcurrentDictionary<ContentPackage, IModConfigInfo> _runningPackages = new ConcurrentDictionary<ContentPackage, IModConfigInfo>();

		// Token: 0x04002BFC RID: 11260
		private readonly ConcurrentDictionary<string, ContentPackage> _packageNameCache = new ConcurrentDictionary<string, ContentPackage>();

		// Token: 0x04002BFD RID: 11261
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002BFE RID: 11262
		private readonly AsyncReaderWriterLock _executionLock = new AsyncReaderWriterLock();

		// Token: 0x04002BFF RID: 11263
		private int _isDisposed;
	}
}
