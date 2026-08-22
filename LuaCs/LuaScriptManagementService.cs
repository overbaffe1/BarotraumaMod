using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FluentResults;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000502 RID: 1282
	[NullableContext(1)]
	[Nullable(0)]
	internal class LuaScriptManagementService : ILuaScriptManagementService, IReusableService, IService, IDisposable, ILuaDataService, ILuaService, IEventAssemblyUnloading, IEvent<IEventAssemblyUnloading>, IEvent
	{
		// Token: 0x170014F7 RID: 5367
		// (get) Token: 0x060052FD RID: 21245 RVA: 0x002C60BD File Offset: 0x002C42BD
		[Nullable(2)]
		public Script InternalScript
		{
			[NullableContext(2)]
			get
			{
				return this._script;
			}
		}

		// Token: 0x170014F8 RID: 5368
		// (get) Token: 0x060052FE RID: 21246 RVA: 0x002C60C5 File Offset: 0x002C42C5
		[MemberNotNullWhen(true, "_script")]
		public bool IsRunning
		{
			[MemberNotNullWhen(true, "_script")]
			get
			{
				return this._isRunning;
			}
		}

		// Token: 0x060052FF RID: 21247 RVA: 0x002C60D0 File Offset: 0x002C42D0
		public LuaScriptManagementService(ILoggerService loggerService, ILuaScriptLoader loader, ILuaUserDataService userDataService, ISafeLuaUserDataService safeUserDataService, IDefaultLuaRegistrar defaultLuaRegistrar, ILuaScriptServicesConfig luaScriptServicesConfig, IPluginManagementService pluginManagementService, INetworkingService networkingService, LuaGame luaGame, IEventService eventService, ILuaCsTimer luaCsTimer, IConsoleCommandsService commandsService, ILuaCsInfoProvider luaCsInfoProvider, ILuaConfigService configService, Lazy<IPackageManagementService> packageManagementService)
		{
			this._luaScriptLoader = loader;
			this._userDataService = userDataService;
			this._safeUserDataService = safeUserDataService;
			this._defaultLuaRegistrar = defaultLuaRegistrar;
			this._luaScriptServicesConfig = luaScriptServicesConfig;
			this._loggerService = loggerService;
			this._pluginManagementService = pluginManagementService;
			this._networkingService = networkingService;
			this._luaGame = luaGame;
			this._eventService = eventService;
			this._commandsService = commandsService;
			this._luaCsInfoProvider = luaCsInfoProvider;
			this._configService = configService;
			this._packageManagementService = packageManagementService;
			this._luaCsTimer = luaCsTimer;
			this.RegisterLuaEvents();
			this.RegisterConsoleCommands(this._commandsService);
		}

		// Token: 0x06005300 RID: 21248 RVA: 0x002C6180 File Offset: 0x002C4380
		private void RegisterConsoleCommands(IConsoleCommandsService commands)
		{
			commands.RegisterCommand("cl_reloadlua|cl_reloadcs|cl_reloadluacs", "Re-initializes the LuaCs environment.", delegate(string[] args)
			{
				LuaCsSetup.Instance.EventService.PublishEvent<IEventReloadAllPackages>(delegate(IEventReloadAllPackages sub)
				{
					sub.OnReloadAllPackages();
				});
			}, null, false);
			commands.RegisterCommand("cl_lua", "cl_lua: Runs a string on the client.", delegate(string[] args)
			{
				if (GameMain.Client != null && !GameMain.Client.HasPermission(ClientPermissions.ConsoleCommands))
				{
					DebugConsole.ThrowError("Command not permitted.", null, null, false, false);
					return;
				}
				if (LuaCsSetup.Instance.CurrentRunState != RunState.Running)
				{
					DebugConsole.ThrowError("LuaCs not initialized, use the console command cl_reloadluacs to force initialization.", null, null, false, false);
					return;
				}
				Result<DynValue> result = LuaCsSetup.Instance.LuaScriptManagementService.DoString(string.Join(" ", args));
				LuaCsSetup.Instance.Logger.LogResults(result.ToResult());
			}, null, false);
			commands.RegisterCommand("cl_toggleluadebug", "Toggles the MoonSharp Debug Server.", delegate(string[] args)
			{
				DebugConsole.Log("This command is currently not implemented. Please open a github issue if you need this feature.");
			}, null, false);
		}

		// Token: 0x170014F9 RID: 5369
		// (get) Token: 0x06005301 RID: 21249 RVA: 0x002C6220 File Offset: 0x002C4420
		// (set) Token: 0x06005302 RID: 21250 RVA: 0x002C6228 File Offset: 0x002C4428
		public bool IsDisposed { get; private set; }

		// Token: 0x06005303 RID: 21251 RVA: 0x002C6231 File Offset: 0x002C4431
		public void SetCachingPolicy(bool useCaching)
		{
			ILuaScriptLoader luaScriptLoader = this._luaScriptLoader;
			if (luaScriptLoader == null)
			{
				return;
			}
			luaScriptLoader.SetCachingPolicy(useCaching);
		}

		// Token: 0x06005304 RID: 21252 RVA: 0x002C6244 File Offset: 0x002C4444
		public Task<Result> LoadScriptResourcesAsync([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> resourcesInfo)
		{
			LuaScriptManagementService.<LoadScriptResourcesAsync>d__30 <LoadScriptResourcesAsync>d__;
			<LoadScriptResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<LoadScriptResourcesAsync>d__.<>4__this = this;
			<LoadScriptResourcesAsync>d__.resourcesInfo = resourcesInfo;
			<LoadScriptResourcesAsync>d__.<>1__state = -1;
			<LoadScriptResourcesAsync>d__.<>t__builder.Start<LuaScriptManagementService.<LoadScriptResourcesAsync>d__30>(ref <LoadScriptResourcesAsync>d__);
			return <LoadScriptResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06005305 RID: 21253 RVA: 0x002C6290 File Offset: 0x002C4490
		public Result<DynValue> DoString(string code)
		{
			IService.CheckDisposed(this);
			if (this._script == null || !this.IsRunning)
			{
				throw new Exception("Disposed");
			}
			Result<DynValue> result2;
			try
			{
				DynValue result = this._script.DoString(code, null, null);
				result2 = Result.Ok<DynValue>(result);
			}
			catch (Exception ex)
			{
				result2 = Result.Fail(new ExceptionalError(ex));
			}
			return result2;
		}

		// Token: 0x06005306 RID: 21254 RVA: 0x002C62FC File Offset: 0x002C44FC
		private DynValue DoFile(string file, [Nullable(2)] Table globalContext = null, [Nullable(2)] string codeStringFriendly = null)
		{
			if (this._script == null)
			{
				throw new Exception("Not running");
			}
			if (!LuaCsFile.CanReadFromPath(file))
			{
				throw new ScriptRuntimeException("dofile: File access to " + file + " not allowed.");
			}
			if (!LuaCsFile.Exists(file))
			{
				throw new ScriptRuntimeException("dofile: File " + file + " not found.");
			}
			return this._script.DoFile(file, globalContext, codeStringFriendly);
		}

		// Token: 0x06005307 RID: 21255 RVA: 0x002C6368 File Offset: 0x002C4568
		private DynValue LoadFile(string file, [Nullable(2)] Table globalContext = null, [Nullable(2)] string codeStringFriendly = null)
		{
			if (this._script == null)
			{
				throw new Exception("Not running");
			}
			if (!LuaCsFile.CanReadFromPath(file))
			{
				throw new ScriptRuntimeException("loadfile: File access to " + file + " not allowed.");
			}
			if (!LuaCsFile.Exists(file))
			{
				throw new ScriptRuntimeException("loadfile: File " + file + " not found.");
			}
			return this._script.LoadFile(file, globalContext, codeStringFriendly);
		}

		// Token: 0x06005308 RID: 21256 RVA: 0x002C63D4 File Offset: 0x002C45D4
		private void RegisterLuaEvents()
		{
			this._eventService.Subscribe<IEventAssemblyUnloading>(this);
			this._eventService.RegisterLuaEventAlias<IEventUpdate>("think", "OnUpdate");
			this._eventService.RegisterLuaEventAlias<IEventKeyUpdate>("keyUpdate", "OnKeyUpdate");
			this._eventService.RegisterLuaEventAlias<IEventAfflictionUpdate>("afflictionUpdate", "OnAfflictionUpdate");
			this._eventService.RegisterLuaEventAlias<IEventCharacterCreated>("character.created", "OnCharacterCreated");
			this._eventService.RegisterLuaEventAlias<IEventCharacterDeath>("character.death", "OnCharacterDeath");
			this._eventService.RegisterLuaEventAlias<IEventCharacterDamageLimb>("character.damageLimb", "OnCharacterDamageLimb");
			this._eventService.RegisterLuaEventAlias<IEventGiveCharacterJobItems>("character.giveJobItems", "OnGiveCharacterJobItems");
			this._eventService.RegisterLuaEventAlias<IEventHumanCPRSuccess>("character.CPRSuccess", "OnCharacterCPRSuccess");
			this._eventService.RegisterLuaEventAlias<IEventHumanCPRFailed>("character.CPRFailed", "OnCharacterCPRFailed");
			this._eventService.RegisterLuaEventAlias<IEventHumanCPRSuccess>("human.CPRSuccess", "OnCharacterCPRSuccess");
			this._eventService.RegisterLuaEventAlias<IEventHumanCPRFailed>("human.CPRFailed", "OnCharacterCPRFailed");
			this._eventService.RegisterLuaEventAlias<IEventCharacterApplyDamage>("character.applyDamage", "OnCharacterApplyDamage");
			this._eventService.RegisterLuaEventAlias<IEventCharacterApplyAffliction>("character.applyAffliction", "OnCharacterApplyAffliction");
			this._eventService.RegisterLuaEventAlias<IEventGapOxygenUpdate>("gapOxygenUpdate", "OnGapOxygenUpdate");
			this._eventService.RegisterLuaEventAlias<IEventClientControlHusk>("husk.clientControlHusk", "OnClientControlHusk");
			this._eventService.RegisterLuaEventAlias<IEventMeleeWeaponHandleImpact>("meleeWeapon.handleImpact", "OnMeleeWeaponHandleImpact");
			this._eventService.RegisterLuaEventAlias<IEventServerLog>("serverLog", "OnServerLog");
			this._eventService.RegisterLuaEventAlias<IEventTryClientChangeName>("tryChangeClientName", "OnTryClienChangeName");
			this._eventService.RegisterLuaEventAlias<IEventChangeFallDamage>("changeFallDamage", "OnChangeFallDamage");
			this._eventService.RegisterLuaEventAlias<IEventChatMessage>("chatMessage", "OnChatMessage");
			this._eventService.RegisterLuaEventAlias<IEventCanUseVoiceRadio>("canUseVoiceRadio", "OnCanUseVoiceRadio");
			this._eventService.RegisterLuaEventAlias<IEventChangeLocalVoiceRange>("changeLocalVoiceRange", "OnChangeLocalVoiceRange");
			this._eventService.RegisterLuaEventAlias<IEventRoundStarted>("roundStart", "OnRoundStart");
			this._eventService.RegisterLuaEventAlias<IEventRoundEnded>("roundEnd", "OnRoundEnd");
			this._eventService.RegisterLuaEventAlias<IEventMissionsEnded>("missionsEnded", "OnMissionsEnded");
			this._eventService.RegisterLuaEventAlias<IEventSignalReceived>("signalReceived", "OnSignalReceived");
			this._eventService.RegisterLuaEventAlias<IEventItemCreated>("item.created", "OnItemCreated");
			this._eventService.RegisterLuaEventAlias<IEventItemRemoved>("item.removed", "OnItemRemoved");
			this._eventService.RegisterLuaEventAlias<IEventItemUse>("item.use", "OnItemUsed");
			this._eventService.RegisterLuaEventAlias<IEventItemSecondaryUse>("item.secondaryUse", "OnItemSecondaryUsed");
			this._eventService.RegisterLuaEventAlias<IEventItemReadPropertyChange>("item.readPropertyChange", "OnItemReadPropertyChange");
			this._eventService.RegisterLuaEventAlias<IEventItemDeconstructed>("item.deconstructed", "OnItemDeconstructed");
			this._eventService.RegisterLuaEventAlias<IEventInventoryPutItem>("inventoryPutItem", "OnInventoryPutItem");
			this._eventService.RegisterLuaEventAlias<IEventInventoryItemSwap>("inventoryItemSwap", "OnInventoryItemSwap");
			this._eventService.RegisterLuaEventAlias<IEventCharacterCreated>("characterCreated", "OnCharacterCreated");
			this._eventService.RegisterLuaEventAlias<IEventCharacterDeath>("characterDeath", "OnCharacterDeath");
			this._eventService.RegisterLuaEventAlias<IEventServerRawNetMessageReceived>("netMessageReceived", "OnReceivedServerNetMessage");
		}

		// Token: 0x06005309 RID: 21257 RVA: 0x002C671C File Offset: 0x002C491C
		private void SetupEnvironment(bool enableSandbox)
		{
			this._script = new Script(CoreModules.Basic | CoreModules.GlobalConsts | CoreModules.TableIterators | CoreModules.Metatables | CoreModules.String | CoreModules.Table | CoreModules.ErrorHandling | CoreModules.Math | CoreModules.Coroutine | CoreModules.Bit32 | CoreModules.OS_Time | CoreModules.OS_System | CoreModules.IO | CoreModules.Debug | CoreModules.Dynamic | CoreModules.Json);
			this._script.Options.DebugPrint = delegate(string msg)
			{
				this._loggerService.LogMessage("[Lua] " + msg, null, null);
			};
			this.SetCachingPolicy(this._luaCsInfoProvider.UseCaching);
			this._script.Options.ScriptLoader = this._luaScriptLoader;
			this._script.Options.CheckThreadAccess = false;
			Script.GlobalOptions.ShouldPCallCatchException = ((Exception ex) => true);
			UserData.RegisterType<ILuaCsHook.HookMethodType>(InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(LuaGame), InteropAccessMode.Default, null);
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)UserData.RegisterType(typeof(EventService), InteropAccessMode.Default, null);
			descriptor.AddDynValue("HookMethodType", UserData.CreateStatic<ILuaCsHook.HookMethodType>());
			UserData.RegisterType(typeof(ILuaCsNetworking), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ILuaCsUtility), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ILuaCsTimer), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(LuaCsFile), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ILuaScriptResourceInfo), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(IResourceInfo), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(IUserDataDescriptor), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(INetworkingService), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ILuaConfigService), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ILoggerService), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(ISettingBase), InteropAccessMode.Default, null);
			UserData.RegisterType(typeof(IDataInfo), InteropAccessMode.Default, null);
			Type[] settingBaseTypes = new Type[]
			{
				typeof(ISettingBase<bool>),
				typeof(ISettingBase<string>),
				typeof(ISettingBase<byte>),
				typeof(ISettingBase<sbyte>),
				typeof(ISettingBase<ushort>),
				typeof(ISettingBase<short>),
				typeof(ISettingBase<char>),
				typeof(ISettingBase<uint>),
				typeof(ISettingBase<int>),
				typeof(ISettingBase<ulong>),
				typeof(ISettingBase<long>),
				typeof(ISettingBase<float>),
				typeof(ISettingBase<double>),
				typeof(ISettingRangeBase<float>),
				typeof(ISettingRangeBase<int>),
				typeof(ISettingList<string>),
				typeof(ISettingList<byte>),
				typeof(ISettingList<sbyte>),
				typeof(ISettingList<ushort>),
				typeof(ISettingList<short>),
				typeof(ISettingList<char>),
				typeof(ISettingList<uint>),
				typeof(ISettingList<int>),
				typeof(ISettingList<ulong>),
				typeof(ISettingList<long>),
				typeof(ISettingList<float>),
				typeof(ISettingList<double>)
			};
			Dictionary<string, Dictionary<string, object>> settingsTable = new Dictionary<string, Dictionary<string, object>>();
			foreach (Type type in settingBaseTypes)
			{
				UserData.RegisterType(type, InteropAccessMode.Default, null);
				string baseName = type.Name.RemoveFromEnd("`1", StringComparison.Ordinal).Substring(1);
				if (!settingsTable.ContainsKey(baseName))
				{
					settingsTable[baseName] = new Dictionary<string, object>();
				}
				settingsTable[baseName][type.GetGenericArguments()[0].Name] = UserData.CreateStatic(type);
			}
			foreach (KeyValuePair<string, Dictionary<string, object>> keyPair in settingsTable)
			{
				this._script.Globals[keyPair.Key] = keyPair.Value;
			}
			UserData.RegisterType(typeof(ISettingControl), InteropAccessMode.Default, null);
			this._script.Globals["SettingControl"] = UserData.CreateStatic(typeof(ISettingControl));
			new LuaConverters(this).RegisterLuaConverters();
			LuaRequire luaRequire = new LuaRequire(this._script);
			this._script.Globals["setmodulepaths"] = new Func<string[], string[]>(delegate([Nullable(1)] string[] str)
			{
				((LuaScriptLoader)this._luaScriptLoader).ModulePaths = str;
				return str;
			});
			this._script.Globals["dofile"] = new Func<string, Table, string, DynValue>(this.DoFile);
			this._script.Globals["loadfile"] = new Func<string, Table, string, DynValue>(this.LoadFile);
			this._script.Globals["require"] = new Func<string, Table, DynValue>(luaRequire.Require);
			this._script.Globals["printerror"] = new Action<DynValue>(delegate(DynValue o)
			{
				this._loggerService.LogError("[Lua] " + o.ToString());
			});
			this._script.Globals["dostring"] = new Func<string, Table, string, DynValue>(this._script.DoString);
			this._script.Globals["load"] = new Func<string, Table, string, DynValue>(this._script.LoadString);
			this._script.Globals["Game"] = this._luaGame;
			this._script.Globals["Hook"] = this._eventService;
			this._script.Globals["Timer"] = this._luaCsTimer;
			this._script.Globals["File"] = UserData.CreateStatic<LuaCsFile>();
			this._script.Globals["ConfigService"] = this._configService;
			this._script.Globals["Networking"] = this._networkingService;
			this._script.Globals["trygetpackage"] = new <>F{00000010}<string, ContentPackage, bool>(delegate(string name, out ContentPackage package)
			{
				return this._packageManagementService.Value.TryGetLoadedPackageByName(name, out package);
			});
			this._script.Globals["Logger"] = this._loggerService;
			if (enableSandbox)
			{
				UserData.RegisterType(typeof(SafeLuaUserDataService), InteropAccessMode.Default, null);
				this._script.Globals["LuaUserData"] = this._safeUserDataService;
			}
			else
			{
				UserData.RegisterType(typeof(LuaUserDataService), InteropAccessMode.Default, null);
				this._script.Globals["LuaUserData"] = this._userDataService;
			}
			Table eventsTable = new Table(this._script);
			Result<ImmutableArray<Type>> typesValue = this._pluginManagementService.GetImplementingTypes<IEvent>(true, true, true);
			if (typesValue.IsSuccess)
			{
				foreach (Type eventType in typesValue.Value)
				{
					if (!eventType.IsGenericType && eventType.IsInterface)
					{
						UserData.RegisterType(eventType, InteropAccessMode.Default, null);
						eventsTable[eventType.Name] = UserData.CreateStatic(eventType);
					}
				}
			}
			this._script.Globals["Events"] = eventsTable;
			this._script.Globals["ExecutionNumber"] = 0;
			this._script.Globals["CSActive"] = !enableSandbox;
			((Table)this._script.Globals["debug"])["breakpoint"] = new Action(delegate()
			{
				Debugger.Break();
			});
			this._script.Globals["SERVER"] = false;
			this._script.Globals["CLIENT"] = true;
			this._defaultLuaRegistrar.RegisterAll();
		}

		// Token: 0x0600530A RID: 21258 RVA: 0x002C6EF0 File Offset: 0x002C50F0
		public Result ExecuteLoadedScripts([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> executionOrder, bool enableSandbox)
		{
			if (this._isRunning)
			{
				return Result.Fail("Tried to execute Lua scripts without unloading first.");
			}
			this._loggerService.LogMessage("[Lua] Executing scripts", null, null);
			this.SetupEnvironment(enableSandbox);
			if (this._script == null)
			{
				return Result.Ok();
			}
			Result result = Result.Ok();
			this._isRunning = true;
			string[] packages = (from p in (from r in executionOrder
			select r.OwnerPackage).Distinct<ContentPackage>()
			select p.Dir + "/Lua/?.lua").ToArray<string>();
			((LuaScriptLoader)this._luaScriptLoader).ModulePaths = packages;
			Table package = (Table)this._script.Globals["package"];
			package.Set("path", DynValue.FromObject(this._script, packages));
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				IWriteMessage startMessage = this._networkingService.Start("_luastart");
				List<ContentPackage> packagesToReport = (from p in ContentPackageManager.EnabledPackages.All
				where this._packageManagementService.Value.PackageContainsAnyRunnableResource(p)
				where !p.NameMatches("LuaCsForBarotrauma")
				select p).ToList<ContentPackage>();
				startMessage.WriteUInt16((ushort)packagesToReport.Count<ContentPackage>());
				foreach (ContentPackage enabledPackage in packagesToReport)
				{
					Option<ContentPackageId> id = enabledPackage.UgcId;
					string hash = enabledPackage.Hash.StringRepresentation ?? "";
					startMessage.WriteString(enabledPackage.Name);
					startMessage.WriteString(enabledPackage.ModVersion);
					ContentPackageId packageId;
					if (!id.TryUnwrap(out packageId))
					{
						goto IL_1E2;
					}
					SteamWorkshopId steamId = packageId as SteamWorkshopId;
					if (steamId == null)
					{
						goto IL_1E2;
					}
					startMessage.WriteUInt64(steamId.Value);
					IL_1EB:
					startMessage.WriteString(hash);
					continue;
					IL_1E2:
					startMessage.WriteUInt64(0UL);
					goto IL_1EB;
				}
				this._networkingService.Send(startMessage, DeliveryMethod.Reliable);
			}
			foreach (ILuaScriptResourceInfo resource in from l in executionOrder
			where l.IsAutorun
			select l)
			{
				foreach (ContentPath filePath in resource.FilePaths)
				{
					try
					{
						this._loggerService.LogMessage("[Lua] - Run " + filePath.Value, null, null);
						this._script.Call(this._script.LoadFile(filePath.FullPath, null, null), new object[]
						{
							resource.OwnerPackage.Dir
						});
					}
					catch (Exception e)
					{
						result = result.WithError(new ExceptionalError(e));
					}
				}
			}
			this._eventService.Call("loaded", Array.Empty<object>());
			return result;
		}

		// Token: 0x0600530B RID: 21259 RVA: 0x002C7254 File Offset: 0x002C5454
		[return: Nullable(2)]
		public DynValue CallFunctionSafe(object luaFunction, params object[] args)
		{
			if (!this.IsRunning)
			{
				return null;
			}
			Script script = this._script;
			DynValue result;
			lock (script)
			{
				try
				{
					return this._script.Call(luaFunction, args);
				}
				catch (Exception e)
				{
					this._loggerService.HandleException(e, null);
				}
				result = null;
			}
			return result;
		}

		// Token: 0x0600530C RID: 21260 RVA: 0x002C72C8 File Offset: 0x002C54C8
		public Result UnloadActiveScripts()
		{
			this._isRunning = false;
			this._script = null;
			return Result.Ok();
		}

		// Token: 0x0600530D RID: 21261 RVA: 0x002C72DD File Offset: 0x002C54DD
		public Result DisposePackageResources(ContentPackage package)
		{
			return Result.Ok();
		}

		// Token: 0x0600530E RID: 21262 RVA: 0x002C72E4 File Offset: 0x002C54E4
		public Result DisposeAllPackageResources()
		{
			if (this.IsRunning)
			{
				this.UnloadActiveScripts();
			}
			this._resourcesInfo.Clear();
			this._luaScriptLoader.ClearCaches();
			return Result.Ok();
		}

		// Token: 0x0600530F RID: 21263 RVA: 0x002C7310 File Offset: 0x002C5510
		public Result Reset()
		{
			IService.CheckDisposed(this);
			this._luaScriptLoader.ClearCaches();
			this._userDataService.Reset();
			this._luaCsTimer.Reset();
			this.RegisterLuaEvents();
			return this.DisposeAllPackageResources();
		}

		// Token: 0x06005310 RID: 21264 RVA: 0x002C7347 File Offset: 0x002C5547
		public void Dispose()
		{
			this.IsDisposed = true;
			this._userDataService.Dispose();
			this._luaScriptLoader.Dispose();
			this._commandsService.Dispose();
		}

		// Token: 0x06005311 RID: 21265 RVA: 0x002C7371 File Offset: 0x002C5571
		[return: Nullable(2)]
		public object GetGlobalTableValue(string tableName)
		{
			if (!this.IsRunning)
			{
				return null;
			}
			return this._script.Globals[tableName];
		}

		// Token: 0x06005312 RID: 21266 RVA: 0x002C7390 File Offset: 0x002C5590
		public void OnAssemblyUnloading(Assembly assembly)
		{
			foreach (Type type in assembly.SafeGetTypes())
			{
				UserData.UnregisterType(type, true);
			}
		}

		// Token: 0x04002BD6 RID: 11222
		[Nullable(2)]
		private Script _script;

		// Token: 0x04002BD7 RID: 11223
		private bool _isRunning;

		// Token: 0x04002BD8 RID: 11224
		private List<ILuaScriptResourceInfo> _resourcesInfo = new List<ILuaScriptResourceInfo>();

		// Token: 0x04002BD9 RID: 11225
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002BDA RID: 11226
		private readonly ILuaUserDataService _userDataService;

		// Token: 0x04002BDB RID: 11227
		private readonly ISafeLuaUserDataService _safeUserDataService;

		// Token: 0x04002BDC RID: 11228
		private readonly ILuaScriptLoader _luaScriptLoader;

		// Token: 0x04002BDD RID: 11229
		private readonly ILuaScriptServicesConfig _luaScriptServicesConfig;

		// Token: 0x04002BDE RID: 11230
		private readonly ILoggerService _loggerService;

		// Token: 0x04002BDF RID: 11231
		private readonly LuaGame _luaGame;

		// Token: 0x04002BE0 RID: 11232
		private readonly IEventService _eventService;

		// Token: 0x04002BE1 RID: 11233
		private readonly ILuaCsTimer _luaCsTimer;

		// Token: 0x04002BE2 RID: 11234
		private readonly IDefaultLuaRegistrar _defaultLuaRegistrar;

		// Token: 0x04002BE3 RID: 11235
		private readonly IPluginManagementService _pluginManagementService;

		// Token: 0x04002BE4 RID: 11236
		private readonly INetworkingService _networkingService;

		// Token: 0x04002BE5 RID: 11237
		private readonly IConsoleCommandsService _commandsService;

		// Token: 0x04002BE6 RID: 11238
		private readonly ILuaConfigService _configService;

		// Token: 0x04002BE7 RID: 11239
		private readonly ILuaCsInfoProvider _luaCsInfoProvider;

		// Token: 0x04002BE8 RID: 11240
		private readonly Lazy<IPackageManagementService> _packageManagementService;
	}
}
