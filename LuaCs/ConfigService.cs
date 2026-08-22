using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E3 RID: 1251
	public sealed class ConfigService : IConfigService, IReusableService, IService, IDisposable, ILuaConfigService, ILuaService
	{
		// Token: 0x06005182 RID: 20866 RVA: 0x002BDB00 File Offset: 0x002BBD00
		public ImmutableArray<ISettingBase> GetDisplayableConfigs()
		{
			ImmutableArray<ISettingBase> result;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				result = (from s in this._settingsInstances.Values
				where !s.IsDisposed
				where s.GetDisplayInfo().ShowInMenus
				where !GameMain.IsMultiplayer || s.GetConfigInfo().NetSync != NetSync.ServerAuthority
				where s.GetConfigInfo().EditableStates >= this._infoProvider.CurrentRunState
				select s).ToImmutableArray<ISettingBase>();
			}
			return result;
		}

		// Token: 0x170014CB RID: 5323
		// (get) Token: 0x06005183 RID: 20867 RVA: 0x002BDBEC File Offset: 0x002BBDEC
		// (set) Token: 0x06005184 RID: 20868 RVA: 0x002BDBF9 File Offset: 0x002BBDF9
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

		// Token: 0x06005185 RID: 20869 RVA: 0x002BDC08 File Offset: 0x002BBE08
		public void Dispose()
		{
			using (this._operationLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._settingsByPackageLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
					{
						this._logger.LogDebug("ConfigService: Disposing.", null);
						this._configInfoParserService.Dispose();
						this._configProfileInfoParserService.Dispose();
						if (!this._settingsInstances.IsEmpty)
						{
							using (IEnumerator<KeyValuePair<ValueTuple<ContentPackage, string>, ISettingBase>> enumerator = this._settingsInstances.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									KeyValuePair<ValueTuple<ContentPackage, string>, ISettingBase> instance = enumerator.Current;
									try
									{
										if (instance.Value != null)
										{
											this._eventService.PublishEvent<IEventSettingInstanceLifetime>(delegate(IEventSettingInstanceLifetime sub)
											{
												sub.OnSettingInstanceDisposed<ISettingBase>(instance.Value);
											});
											instance.Value.Dispose();
										}
									}
									catch
									{
									}
								}
							}
						}
						this._settingsInstances.Clear();
						this._instanceFactory.Clear();
						this._settingsInstancesByPackage.Clear();
						this._commandsService.Dispose();
						this._storageService = null;
						this._logger = null;
						this._eventService = null;
						this._configInfoParserService = null;
						this._configProfileInfoParserService = null;
						this._commandsService = null;
						this._infoProvider = null;
					}
				}
			}
		}

		// Token: 0x06005186 RID: 20870 RVA: 0x002BDE10 File Offset: 0x002BC010
		public Result Reset()
		{
			Result result2;
			using (this._operationLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result result = new Result();
				if (!this._settingsInstances.IsEmpty)
				{
					using (IEnumerator<KeyValuePair<ValueTuple<ContentPackage, string>, ISettingBase>> enumerator = this._settingsInstances.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<ValueTuple<ContentPackage, string>, ISettingBase> instance = enumerator.Current;
							try
							{
								if (instance.Value != null)
								{
									this._eventService.PublishEvent<IEventSettingInstanceLifetime>(delegate(IEventSettingInstanceLifetime sub)
									{
										sub.OnSettingInstanceDisposed<ISettingBase>(instance.Value);
									});
									instance.Value.Dispose();
								}
							}
							catch (Exception e)
							{
								result.WithError(new ExceptionalError(e));
							}
						}
					}
				}
				this._settingsInstances.Clear();
				this._instanceFactory.Clear();
				this._settingsInstancesByPackage.Clear();
				this._storageService.PurgeCache();
				result2 = result;
			}
			return result2;
		}

		// Token: 0x06005187 RID: 20871 RVA: 0x002BDF54 File Offset: 0x002BC154
		public ConfigService(ILoggerService logger, IStorageService storageService, IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo> configInfoParserService, IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo> configProfileInfoParserService, IEventService eventService, IConsoleCommandsService commandsService, ILuaCsInfoProvider infoProvider)
		{
			this._logger = logger;
			this._storageService = storageService;
			this._configInfoParserService = configInfoParserService;
			this._configProfileInfoParserService = configProfileInfoParserService;
			this._eventService = eventService;
			this._commandsService = commandsService;
			this._infoProvider = infoProvider;
			this._storageService.UseCaching = false;
			this.InjectCommands(commandsService);
		}

		// Token: 0x06005188 RID: 20872 RVA: 0x002BDFF4 File Offset: 0x002BC1F4
		private void InjectCommands(IConsoleCommandsService commandsService)
		{
			commandsService.RegisterCommand("cfg_getvalue", "cfg_getvalue [Content Package] [InternalName] [ValueString]: gets a config value.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					this._logger.LogError("Please specify the name of the package to set the config.");
					return;
				}
				if (args.Length < 2)
				{
					this._logger.LogError("Please specify the name of the config.");
					return;
				}
				RegularPackage package = ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name == args[0]);
				if (package == null)
				{
					this._logger.LogError("Could not find the package " + args[0] + "!");
					return;
				}
				string internalName = args[1];
				ISettingBase setting;
				if (!this.TryGetConfig<ISettingBase>(package, internalName, out setting))
				{
					this._logger.LogError("Could not get config with name " + internalName);
					return;
				}
				this._logger.LogMessage("config " + internalName + " value is " + setting.GetStringValue(), new Color?(Color.Green), null);
			}, delegate
			{
				string[][] array = new string[1][];
				array[0] = (from p in ContentPackageManager.RegularPackages
				select p.Name).ToArray<string>();
				return array;
			}, false);
			commandsService.RegisterCommand("cfg_setvalue", "cfg_setvalue [Content Package] [InternalName] [ValueString]: sets a config.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					this._logger.LogError("Please specify the name of the package to set the config.");
					return;
				}
				if (args.Length < 2)
				{
					this._logger.LogError("Please specify the name of the config.");
					return;
				}
				if (args.Length < 3)
				{
					this._logger.LogError("Please specify the value to set the config to.");
					return;
				}
				RegularPackage package = ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name == args[0]);
				if (package == null)
				{
					this._logger.LogError("Could not find the package " + args[0] + "!");
					return;
				}
				string internalName = args[1];
				string valueString = args[2];
				ISettingBase setting;
				if (!this.TryGetConfig<ISettingBase>(package, internalName, out setting))
				{
					this._logger.LogError("Could not get config with name " + internalName);
					return;
				}
				if (setting.TrySetSerializedValue(valueString))
				{
					this._logger.LogMessage("Set config " + internalName + " value to " + valueString, new Color?(Color.Green), null);
					Result res = this.SaveConfigValue(setting);
					if (res != null && res.IsFailed)
					{
						this._logger.LogMessage("Failed to save new config data to disk. Reasons: " + res.ToString(), null, null);
						return;
					}
				}
				else
				{
					this._logger.LogError("Failed to set config value");
				}
			}, delegate
			{
				string[][] array = new string[1][];
				array[0] = (from p in ContentPackageManager.RegularPackages
				select p.Name).ToArray<string>();
				return array;
			}, false);
			commandsService.RegisterCommand("cfg_setprofile", "cfg_setprofile [ContentPackage] [InternalProfileName]", delegate(string[] args)
			{
				if (args.Length < 1 || args[0].IsNullOrWhiteSpace())
				{
					this._logger.LogError("Please specify the name of the package of the profile.");
					return;
				}
				if (args.Length < 2 || args[1].IsNullOrWhiteSpace())
				{
					this._logger.LogError("Please specify the name of the profile.");
					return;
				}
				RegularPackage package = ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name == args[0], null);
				if (package == null)
				{
					this._logger.LogError("Could not find the package " + args[0] + "!");
					return;
				}
				Result res = this.ApplyConfigProfile(package, args[1]);
				if (res.IsFailed)
				{
					this._logger.LogError("Errors while applying profile " + args[1] + "!");
					this._logger.LogResults(res);
					return;
				}
				this._logger.Log("Profile " + args[1] + " applied successfully!", new Color?(Color.Green), ServerLog.MessageType.ServerMessage);
			}, delegate
			{
				string[][] array = new string[1][];
				array[0] = (from p in ContentPackageManager.RegularPackages
				select p.Name).ToArray<string>();
				return array;
			}, false);
		}

		// Token: 0x06005189 RID: 20873 RVA: 0x002BE0B8 File Offset: 0x002BC2B8
		public void RegisterSettingTypeInitializer<T>(string typeIdentifier, [TupleElementNames(new string[]
		{
			"ConfigService",
			"Info"
		})] Func<ValueTuple<IConfigService, IConfigInfo>, T> settingFactory) where T : class, ISettingBase
		{
			Guard.IsNotNullOrWhiteSpace(typeIdentifier, "typeIdentifier");
			Guard.IsNotNull<Func<ValueTuple<IConfigService, IConfigInfo>, T>>(settingFactory, "settingFactory");
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (this._instanceFactory.ContainsKey(typeIdentifier))
				{
					ThrowHelper.ThrowArgumentException("RegisterSettingTypeInitializer: The type identifier " + typeIdentifier + " is already registered.");
				}
				this._instanceFactory[typeIdentifier] = settingFactory;
			}
		}

		// Token: 0x0600518A RID: 20874 RVA: 0x002BE158 File Offset: 0x002BC358
		private static ImmutableArray<T> SelectCompatible<T>(ImmutableArray<T> resources) where T : IBaseResourceInfo
		{
			return (from r in resources
			where r.SupportedPlatforms.HasFlag(ModUtils.Environment.CurrentPlatform)
			where r.SupportedTargets.HasFlag(ModUtils.Environment.CurrentTarget)
			orderby (r.Optional > false) ? 1 : 0, r.LoadPriority
			select r).ToImmutableArray<T>();
		}

		// Token: 0x0600518B RID: 20875 RVA: 0x002BE1FC File Offset: 0x002BC3FC
		public Task<Result> LoadConfigsAsync(ImmutableArray<IConfigResourceInfo> configResources)
		{
			ConfigService.<LoadConfigsAsync>d__25 <LoadConfigsAsync>d__;
			<LoadConfigsAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<LoadConfigsAsync>d__.<>4__this = this;
			<LoadConfigsAsync>d__.configResources = configResources;
			<LoadConfigsAsync>d__.<>1__state = -1;
			<LoadConfigsAsync>d__.<>t__builder.Start<ConfigService.<LoadConfigsAsync>d__25>(ref <LoadConfigsAsync>d__);
			return <LoadConfigsAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600518C RID: 20876 RVA: 0x002BE248 File Offset: 0x002BC448
		public Task<Result> LoadConfigsProfilesAsync(ImmutableArray<IConfigResourceInfo> configProfileResources)
		{
			ConfigService.<LoadConfigsProfilesAsync>d__26 <LoadConfigsProfilesAsync>d__;
			<LoadConfigsProfilesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<LoadConfigsProfilesAsync>d__.<>4__this = this;
			<LoadConfigsProfilesAsync>d__.configProfileResources = configProfileResources;
			<LoadConfigsProfilesAsync>d__.<>1__state = -1;
			<LoadConfigsProfilesAsync>d__.<>t__builder.Start<ConfigService.<LoadConfigsProfilesAsync>d__26>(ref <LoadConfigsProfilesAsync>d__);
			return <LoadConfigsProfilesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600518D RID: 20877 RVA: 0x002BE294 File Offset: 0x002BC494
		public Result LoadSavedValueForConfig(ISettingBase setting)
		{
			Guard.IsNotNull<ISettingBase>(setting, "setting");
			Result result;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<XDocument> saveFileResult = this._storageService.LoadLocalXml(setting.OwnerPackage, "SettingsData.xml");
				if (saveFileResult == null)
				{
					result = Result.Ok();
				}
				else if (saveFileResult != null && saveFileResult.IsFailed)
				{
					result = Result.Ok();
				}
				else
				{
					XElement rootElement = saveFileResult.Value.Root;
					if (rootElement == null || !string.Equals(rootElement.Name.LocalName, "Configuration", StringComparison.InvariantCultureIgnoreCase))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
						defaultInterpolatedStringHandler.AppendFormatted("LoadSavedValueForConfig");
						defaultInterpolatedStringHandler.AppendLiteral(": Root invalid for setting [");
						defaultInterpolatedStringHandler.AppendFormatted(setting.OwnerPackage.Name);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						defaultInterpolatedStringHandler.AppendFormatted(setting.InternalName);
						defaultInterpolatedStringHandler.AppendLiteral("]");
						result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						XElement childElement = rootElement.GetChildElement(XmlConvert.EncodeLocalName(setting.OwnerPackage.Name.Trim()), StringComparison.InvariantCultureIgnoreCase);
						XElement cfgValueElement = (childElement != null) ? childElement.GetChildElement(setting.InternalName, StringComparison.InvariantCultureIgnoreCase) : null;
						if (cfgValueElement == null)
						{
							result = Result.Ok();
						}
						else
						{
							bool isSuccess = setting.TrySetSerializedValue(cfgValueElement);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Failed to set value for [");
							defaultInterpolatedStringHandler2.AppendFormatted(setting.OwnerPackage.Name);
							defaultInterpolatedStringHandler2.AppendLiteral(".");
							defaultInterpolatedStringHandler2.AppendFormatted(setting.InternalName);
							defaultInterpolatedStringHandler2.AppendLiteral("]");
							result = Result.OkIf(isSuccess, new Error(defaultInterpolatedStringHandler2.ToStringAndClear()));
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600518E RID: 20878 RVA: 0x002BE484 File Offset: 0x002BC684
		public Result LoadSavedConfigsValues()
		{
			ImmutableArray<ISettingBase> cfgValues;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				cfgValues = (from kvp in this._settingsInstances
				select kvp.Value).ToImmutableArray<ISettingBase>();
			}
			Result ret = new Result();
			foreach (ISettingBase settingBase in cfgValues)
			{
				this.LoadSavedValueForConfig(settingBase);
			}
			return ret;
		}

		// Token: 0x0600518F RID: 20879 RVA: 0x002BE540 File Offset: 0x002BC740
		public Result ApplyConfigProfile(ContentPackage package, string internalName)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
			Result result2;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				IConfigProfileInfo setting;
				if (!this._settingsProfiles.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out setting))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 3);
					defaultInterpolatedStringHandler.AppendFormatted("ApplyConfigProfile");
					defaultInterpolatedStringHandler.AppendLiteral(": Could not find profile [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result2 = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result result = new Result();
					foreach (ValueTuple<string, XElement> profileValue in setting.ProfileValues)
					{
						ISettingBase instance;
						if (!this._settingsInstances.TryGetValue(new ValueTuple<ContentPackage, string>(package, profileValue.Item1), out instance))
						{
							result.WithError(new Error("ApplyConfigProfile: Could not find setting [" + profileValue.Item1 + "]."));
						}
						else if (!instance.TrySetSerializedValue(profileValue.Item2))
						{
							result.WithError(new Error("ApplyConfigProfile: Failed to set value for [" + profileValue.Item1 + "]."));
						}
					}
					result2 = result;
				}
			}
			return result2;
		}

		// Token: 0x06005190 RID: 20880 RVA: 0x002BE708 File Offset: 0x002BC908
		public Result SaveConfigValue(ISettingBase setting)
		{
			Result<XDocument> saveFileResult = this._storageService.LoadLocalXml(setting.OwnerPackage, "SettingsData.xml");
			if (saveFileResult == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 3);
				defaultInterpolatedStringHandler.AppendFormatted("SaveConfigValue");
				defaultInterpolatedStringHandler.AppendLiteral(": Storage Service Failure while trying to load file for  setting [");
				defaultInterpolatedStringHandler.AppendFormatted(setting.OwnerPackage.Name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted(setting.InternalName);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				return Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			XDocument cpCfgValues;
			if (saveFileResult.IsFailed)
			{
				cpCfgValues = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new object[]
				{
					new XElement("Configuration")
				});
			}
			else
			{
				cpCfgValues = saveFileResult.Value;
			}
			if (cpCfgValues.Root == null || cpCfgValues.Root.Name != "Configuration")
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 3);
				defaultInterpolatedStringHandler2.AppendFormatted("SaveConfigValue");
				defaultInterpolatedStringHandler2.AppendLiteral(": Bad save file format for setting: [");
				defaultInterpolatedStringHandler2.AppendFormatted(setting.OwnerPackage.Name);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				defaultInterpolatedStringHandler2.AppendFormatted(setting.InternalName);
				defaultInterpolatedStringHandler2.AppendLiteral("]");
				return Result.Fail(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			XElement currentTarget = ConfigService.<SaveConfigValue>g__GetOrAddElement|30_4(cpCfgValues.Root, XmlConvert.EncodeLocalName(setting.OwnerPackage.Name.Trim()), (string name) => new XElement(name));
			currentTarget = ConfigService.<SaveConfigValue>g__GetOrAddElement|30_4(currentTarget, setting.InternalName, (string name) => new XElement(name));
			Result ret = setting.GetSerializableValue().Match<Result>(delegate(string str)
			{
				XAttribute tgt = currentTarget.Attribute("Value");
				if (tgt == null)
				{
					XAttribute attr = new XAttribute("Value", str);
					currentTarget.Add(attr);
				}
				else
				{
					tgt.Value = str;
				}
				return Result.Ok();
			}, delegate(XElement elem)
			{
				currentTarget.ReplaceNodes(new XElement("Value", elem));
				return Result.Ok();
			});
			ret.WithReasons(this._storageService.SaveLocalXml(setting.OwnerPackage, "SettingsData.xml", cpCfgValues).Reasons);
			return ret;
		}

		// Token: 0x06005191 RID: 20881 RVA: 0x002BE930 File Offset: 0x002BCB30
		public Result DisposePackageData(ContentPackage package)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Result result2;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentBag<ISettingBase> toDispose;
				using (this._settingsByPackageLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (!this._settingsInstancesByPackage.TryRemove(package, out toDispose) || toDispose == null)
					{
						return Result.Ok();
					}
				}
				Result result = new Result();
				using (IEnumerator<ISettingBase> enumerator = toDispose.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ISettingBase setting = enumerator.Current;
						result.WithReasons(this._eventService.PublishEvent<IEventSettingInstanceLifetime>(delegate(IEventSettingInstanceLifetime sub)
						{
							sub.OnSettingInstanceDisposed<ISettingBase>(setting);
						}).Reasons);
						try
						{
							ISettingBase settingBase;
							this._settingsInstances.TryRemove(new ValueTuple<ContentPackage, string>(setting.OwnerPackage, setting.InternalName), out settingBase);
							setting.Dispose();
						}
						catch (Exception e)
						{
							result.WithError(new ExceptionalError(e));
						}
					}
				}
				result2 = result;
			}
			return result2;
		}

		// Token: 0x06005192 RID: 20882 RVA: 0x002BEAFC File Offset: 0x002BCCFC
		public Result DisposeAllPackageData()
		{
			return this.Reset();
		}

		// Token: 0x06005193 RID: 20883 RVA: 0x002BEB04 File Offset: 0x002BCD04
		public bool TryGetConfig<T>(ContentPackage package, string internalName, out T instance) where T : ISettingBase
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
			bool result;
			using (this._operationLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				using (this._settingsByPackageLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
				{
					IService.CheckDisposed(this);
					instance = default(T);
					ISettingBase inst;
					if (!this._settingsInstances.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out inst))
					{
						result = false;
					}
					else if (inst is T)
					{
						T instanceT = (T)((object)inst);
						instance = instanceT;
						result = true;
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x06005198 RID: 20888 RVA: 0x002BEFAC File Offset: 0x002BD1AC
		[CompilerGenerated]
		internal static XElement <SaveConfigValue>g__GetOrAddElement|30_4(XElement containerElement, string elementName, Func<string, XElement> factory)
		{
			XElement element = containerElement.Element(elementName);
			if (element == null)
			{
				element = factory(elementName);
				containerElement.Add(element);
			}
			return element;
		}

		// Token: 0x04002B3A RID: 11066
		private readonly AsyncReaderWriterLock _operationLock = new AsyncReaderWriterLock();

		// Token: 0x04002B3B RID: 11067
		private readonly AsyncReaderWriterLock _settingsByPackageLock = new AsyncReaderWriterLock();

		// Token: 0x04002B3C RID: 11068
		private int _isDisposed;

		// Token: 0x04002B3D RID: 11069
		private const string SaveDataFileName = "SettingsData.xml";

		// Token: 0x04002B3E RID: 11070
		[TupleElementNames(new string[]
		{
			"OwnerPackage",
			"InternalName"
		})]
		private readonly ConcurrentDictionary<ValueTuple<ContentPackage, string>, ISettingBase> _settingsInstances = new ConcurrentDictionary<ValueTuple<ContentPackage, string>, ISettingBase>();

		// Token: 0x04002B3F RID: 11071
		[TupleElementNames(new string[]
		{
			"ConfigService",
			"Info"
		})]
		private readonly ConcurrentDictionary<string, Func<ValueTuple<IConfigService, IConfigInfo>, ISettingBase>> _instanceFactory = new ConcurrentDictionary<string, Func<ValueTuple<IConfigService, IConfigInfo>, ISettingBase>>();

		// Token: 0x04002B40 RID: 11072
		private readonly ConcurrentDictionary<ContentPackage, ConcurrentBag<ISettingBase>> _settingsInstancesByPackage = new ConcurrentDictionary<ContentPackage, ConcurrentBag<ISettingBase>>();

		// Token: 0x04002B41 RID: 11073
		[TupleElementNames(new string[]
		{
			"Package",
			"ProfileName"
		})]
		private readonly ConcurrentDictionary<ValueTuple<ContentPackage, string>, IConfigProfileInfo> _settingsProfiles = new ConcurrentDictionary<ValueTuple<ContentPackage, string>, IConfigProfileInfo>();

		// Token: 0x04002B42 RID: 11074
		private IStorageService _storageService;

		// Token: 0x04002B43 RID: 11075
		private ILoggerService _logger;

		// Token: 0x04002B44 RID: 11076
		private IEventService _eventService;

		// Token: 0x04002B45 RID: 11077
		private IConsoleCommandsService _commandsService;

		// Token: 0x04002B46 RID: 11078
		private ILuaCsInfoProvider _infoProvider;

		// Token: 0x04002B47 RID: 11079
		private IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo> _configInfoParserService;

		// Token: 0x04002B48 RID: 11080
		private IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo> _configProfileInfoParserService;
	}
}
