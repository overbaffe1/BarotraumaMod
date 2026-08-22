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
	// Token: 0x020003ED RID: 1005
	[NullableContext(1)]
	[Nullable(0)]
	internal class LuaScriptManagementService : ILuaScriptManagementService, IReusableService, IService, IDisposable, ILuaDataService, ILuaService, IEventAssemblyUnloading, IEvent<IEventAssemblyUnloading>, IEvent
	{
		// Token: 0x17000FAF RID: 4015
		// (get) Token: 0x060039CA RID: 14794 RVA: 0x00181611 File Offset: 0x0017F811
		[Nullable(2)]
		public Script InternalScript
		{
			[NullableContext(2)]
			get
			{
				return this._script;
			}
		}

		// Token: 0x17000FB0 RID: 4016
		// (get) Token: 0x060039CB RID: 14795 RVA: 0x00181619 File Offset: 0x0017F819
		[MemberNotNullWhen(true, "_script")]
		public bool IsRunning
		{
			[MemberNotNullWhen(true, "_script")]
			get
			{
				return this._isRunning;
			}
		}

		// Token: 0x060039CC RID: 14796 RVA: 0x00181624 File Offset: 0x0017F824
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

		// Token: 0x060039CD RID: 14797 RVA: 0x001816D4 File Offset: 0x0017F8D4
		private void RegisterConsoleCommands(IConsoleCommandsService commands)
		{
			commands.RegisterCommand("lua", "lua: Runs a string.", delegate(string[] args)
			{
				Result<DynValue> result = LuaCsSetup.Instance.LuaScriptManagementService.DoString(string.Join(" ", args));
				LuaCsSetup.Instance.Logger.LogResults(result.ToResult());
			}, null, false);
			commands.RegisterCommand("reloadlua|reloadcs|reloadluacs", "Re-initializes the LuaCs environment.", delegate(string[] args)
			{
				LuaCsSetup.Instance.EventService.PublishEvent<IEventReloadAllPackages>(delegate(IEventReloadAllPackages sub)
				{
					sub.OnReloadAllPackages();
				});
			}, null, false);
			commands.RegisterCommand("toggleluadebug", "Toggles the MoonSharp Debug Server.", delegate(string[] args)
			{
				int port = 41912;
				if (args.Length != 0)
				{
					int.TryParse(args[0], out port);
				}
				throw new NotImplementedException();
			}, null, false);
			commands.RegisterCommand("install_cl_lua|install_cl|install_cl_cs|install_cl_luacs", "Installs Client-Side LuaCs into your client.", delegate(string[] args)
			{
				LuaCsInstaller.Install();
			}, null, false);
		}

		// Token: 0x17000FB1 RID: 4017
		// (get) Token: 0x060039CE RID: 14798 RVA: 0x001817A5 File Offset: 0x0017F9A5
		// (set) Token: 0x060039CF RID: 14799 RVA: 0x001817AD File Offset: 0x0017F9AD
		public bool IsDisposed { get; private set; }

		// Token: 0x060039D0 RID: 14800 RVA: 0x001817B6 File Offset: 0x0017F9B6
		public void SetCachingPolicy(bool useCaching)
		{
			ILuaScriptLoader luaScriptLoader = this._luaScriptLoader;
			if (luaScriptLoader == null)
			{
				return;
			}
			luaScriptLoader.SetCachingPolicy(useCaching);
		}

		// Token: 0x060039D1 RID: 14801 RVA: 0x001817CC File Offset: 0x0017F9CC
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

		// Token: 0x060039D2 RID: 14802 RVA: 0x00181818 File Offset: 0x0017FA18
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

		// Token: 0x060039D3 RID: 14803 RVA: 0x00181884 File Offset: 0x0017FA84
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

		// Token: 0x060039D4 RID: 14804 RVA: 0x001818F0 File Offset: 0x0017FAF0
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

		// Token: 0x060039D5 RID: 14805 RVA: 0x0018195C File Offset: 0x0017FB5C
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
			this._eventService.RegisterLuaEventAlias<IEventClientConnected>("client.connected", "OnClientConnected");
			this._eventService.RegisterLuaEventAlias<IEventClientDisconnected>("client.disconnected", "OnClientDisconnected");
			this._eventService.RegisterLuaEventAlias<IEventJobsAssigned>("jobsAssigned", "OnJobsAssigned");
			this._eventService.RegisterLuaEventAlias<IEventClientRawNetMessageReceived>("netMessageReceived", "OnReceivedClientNetMessage");
			this._eventService.RegisterLuaEventAlias<IEventClientConnected>("clientConnected", "OnClientConnected");
			this._eventService.RegisterLuaEventAlias<IEventClientDisconnected>("clientDisconnected", "OnClientDisconnected");
			this._eventService.RegisterLuaEventAlias<IEventModifyChatMessage>("modifyChatMessage", "OnModifyMessagePredicate");
		}

		// Token: 0x060039D6 RID: 14806 RVA: 0x00181D28 File Offset: 0x0017FF28
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
			this._script.Globals["SERVER"] = true;
			this._script.Globals["CLIENT"] = false;
			this._defaultLuaRegistrar.RegisterAll();
		}

		// Token: 0x060039D7 RID: 14807 RVA: 0x001824C4 File Offset: 0x001806C4
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
			this._networkingService.Receive("_luastart", delegate(IReadMessage message, Client client)
			{
				ushort num = message.ReadUInt16();
				List<Table> packages2 = new List<Table>();
				for (int i = 0; i < (int)num; i++)
				{
					Table table = new Table(this._script);
					table.Set("Name", DynValue.NewString(message.ReadString()));
					table.Set("Version", DynValue.NewString(message.ReadString()));
					table.Set("Id", DynValue.NewString(message.ReadUInt64().ToString()));
					table.Set("Hash", DynValue.NewString(message.ReadString()));
					packages2.Add(table);
				}
				this._eventService.Call("client.packages", new object[]
				{
					client,
					packages2
				});
			});
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

		// Token: 0x060039D8 RID: 14808 RVA: 0x00182708 File Offset: 0x00180908
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

		// Token: 0x060039D9 RID: 14809 RVA: 0x0018277C File Offset: 0x0018097C
		public Result UnloadActiveScripts()
		{
			this._isRunning = false;
			this._script = null;
			return Result.Ok();
		}

		// Token: 0x060039DA RID: 14810 RVA: 0x00182791 File Offset: 0x00180991
		public Result DisposePackageResources(ContentPackage package)
		{
			return Result.Ok();
		}

		// Token: 0x060039DB RID: 14811 RVA: 0x00182798 File Offset: 0x00180998
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

		// Token: 0x060039DC RID: 14812 RVA: 0x001827C4 File Offset: 0x001809C4
		public Result Reset()
		{
			IService.CheckDisposed(this);
			this._luaScriptLoader.ClearCaches();
			this._userDataService.Reset();
			this._luaCsTimer.Reset();
			this.RegisterLuaEvents();
			return this.DisposeAllPackageResources();
		}

		// Token: 0x060039DD RID: 14813 RVA: 0x001827FB File Offset: 0x001809FB
		public void Dispose()
		{
			this.IsDisposed = true;
			this._userDataService.Dispose();
			this._luaScriptLoader.Dispose();
			this._commandsService.Dispose();
		}

		// Token: 0x060039DE RID: 14814 RVA: 0x00182825 File Offset: 0x00180A25
		[return: Nullable(2)]
		public object GetGlobalTableValue(string tableName)
		{
			if (!this.IsRunning)
			{
				return null;
			}
			return this._script.Globals[tableName];
		}

		// Token: 0x060039DF RID: 14815 RVA: 0x00182844 File Offset: 0x00180A44
		public void OnAssemblyUnloading(Assembly assembly)
		{
			foreach (Type type in assembly.SafeGetTypes())
			{
				UserData.UnregisterType(type, true);
			}
		}

		// Token: 0x04001CF1 RID: 7409
		[Nullable(2)]
		private Script _script;

		// Token: 0x04001CF2 RID: 7410
		private bool _isRunning;

		// Token: 0x04001CF3 RID: 7411
		private List<ILuaScriptResourceInfo> _resourcesInfo = new List<ILuaScriptResourceInfo>();

		// Token: 0x04001CF4 RID: 7412
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04001CF5 RID: 7413
		private readonly ILuaUserDataService _userDataService;

		// Token: 0x04001CF6 RID: 7414
		private readonly ISafeLuaUserDataService _safeUserDataService;

		// Token: 0x04001CF7 RID: 7415
		private readonly ILuaScriptLoader _luaScriptLoader;

		// Token: 0x04001CF8 RID: 7416
		private readonly ILuaScriptServicesConfig _luaScriptServicesConfig;

		// Token: 0x04001CF9 RID: 7417
		private readonly ILoggerService _loggerService;

		// Token: 0x04001CFA RID: 7418
		private readonly LuaGame _luaGame;

		// Token: 0x04001CFB RID: 7419
		private readonly IEventService _eventService;

		// Token: 0x04001CFC RID: 7420
		private readonly ILuaCsTimer _luaCsTimer;

		// Token: 0x04001CFD RID: 7421
		private readonly IDefaultLuaRegistrar _defaultLuaRegistrar;

		// Token: 0x04001CFE RID: 7422
		private readonly IPluginManagementService _pluginManagementService;

		// Token: 0x04001CFF RID: 7423
		private readonly INetworkingService _networkingService;

		// Token: 0x04001D00 RID: 7424
		private readonly IConsoleCommandsService _commandsService;

		// Token: 0x04001D01 RID: 7425
		private readonly ILuaConfigService _configService;

		// Token: 0x04001D02 RID: 7426
		private readonly ILuaCsInfoProvider _luaCsInfoProvider;

		// Token: 0x04001D03 RID: 7427
		private readonly Lazy<IPackageManagementService> _packageManagementService;
	}
}
