using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000503 RID: 1283
	public sealed class ModConfigService : IModConfigService, IService, IDisposable
	{
		// Token: 0x06005318 RID: 21272 RVA: 0x002C7460 File Offset: 0x002C5660
		public ModConfigService(IStorageService storageService, IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo> assemblyParserService, IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo> luaScriptParserService, IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo> configParserService, IParserServiceAsync<ResourceParserInfo, IStylesResourceInfo> stylesParserService, ILoggerService logger)
		{
			this._storageService = storageService;
			this._assemblyParserService = assemblyParserService;
			this._luaScriptParserService = luaScriptParserService;
			this._configParserService = configParserService;
			this._logger = logger;
			this._stylesParserService = stylesParserService;
			this._storageService.UseCaching = false;
		}

		// Token: 0x06005319 RID: 21273 RVA: 0x002C74B8 File Offset: 0x002C56B8
		public void Dispose()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					try
					{
						this._storageService.Dispose();
						this._logger.Dispose();
						this._assemblyParserService.Dispose();
						this._luaScriptParserService.Dispose();
						this._configParserService.Dispose();
						this._storageService = null;
						this._logger = null;
						this._assemblyParserService = null;
						this._luaScriptParserService = null;
						this._configParserService = null;
						this._stylesParserService.Dispose();
						this._stylesParserService = null;
					}
					catch
					{
					}
				}
			}
		}

		// Token: 0x170014FA RID: 5370
		// (get) Token: 0x0600531A RID: 21274 RVA: 0x002C7598 File Offset: 0x002C5798
		// (set) Token: 0x0600531B RID: 21275 RVA: 0x002C75A5 File Offset: 0x002C57A5
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

		// Token: 0x0600531C RID: 21276 RVA: 0x002C75B4 File Offset: 0x002C57B4
		public Task<Result<IModConfigInfo>> CreateConfigAsync(ContentPackage src)
		{
			ModConfigService.<CreateConfigAsync>d__13 <CreateConfigAsync>d__;
			<CreateConfigAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IModConfigInfo>>.Create();
			<CreateConfigAsync>d__.<>4__this = this;
			<CreateConfigAsync>d__.src = src;
			<CreateConfigAsync>d__.<>1__state = -1;
			<CreateConfigAsync>d__.<>t__builder.Start<ModConfigService.<CreateConfigAsync>d__13>(ref <CreateConfigAsync>d__);
			return <CreateConfigAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600531D RID: 21277 RVA: 0x002C7600 File Offset: 0x002C5800
		[return: TupleElementNames(new string[]
		{
			"Source",
			"Config"
		})]
		public Task<ImmutableArray<ValueTuple<ContentPackage, Result<IModConfigInfo>>>> CreateConfigsAsync(ImmutableArray<ContentPackage> src)
		{
			ModConfigService.<CreateConfigsAsync>d__14 <CreateConfigsAsync>d__;
			<CreateConfigsAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<ValueTuple<ContentPackage, Result<IModConfigInfo>>>>.Create();
			<CreateConfigsAsync>d__.<>4__this = this;
			<CreateConfigsAsync>d__.src = src;
			<CreateConfigsAsync>d__.<>1__state = -1;
			<CreateConfigsAsync>d__.<>t__builder.Start<ModConfigService.<CreateConfigsAsync>d__14>(ref <CreateConfigsAsync>d__);
			return <CreateConfigsAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600531E RID: 21278 RVA: 0x002C764C File Offset: 0x002C584C
		private Task<Result<XElement>> TryGetModConfigXmlAsync(ContentPackage src)
		{
			ModConfigService.<TryGetModConfigXmlAsync>d__15 <TryGetModConfigXmlAsync>d__;
			<TryGetModConfigXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<XElement>>.Create();
			<TryGetModConfigXmlAsync>d__.<>4__this = this;
			<TryGetModConfigXmlAsync>d__.src = src;
			<TryGetModConfigXmlAsync>d__.<>1__state = -1;
			<TryGetModConfigXmlAsync>d__.<>t__builder.Start<ModConfigService.<TryGetModConfigXmlAsync>d__15>(ref <TryGetModConfigXmlAsync>d__);
			return <TryGetModConfigXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600531F RID: 21279 RVA: 0x002C7698 File Offset: 0x002C5898
		private Task<Result<IModConfigInfo>> CreateFromConfigXmlAsync(ContentPackage owner, XElement src)
		{
			ModConfigService.<CreateFromConfigXmlAsync>d__16 <CreateFromConfigXmlAsync>d__;
			<CreateFromConfigXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IModConfigInfo>>.Create();
			<CreateFromConfigXmlAsync>d__.<>4__this = this;
			<CreateFromConfigXmlAsync>d__.owner = owner;
			<CreateFromConfigXmlAsync>d__.src = src;
			<CreateFromConfigXmlAsync>d__.<>1__state = -1;
			<CreateFromConfigXmlAsync>d__.<>t__builder.Start<ModConfigService.<CreateFromConfigXmlAsync>d__16>(ref <CreateFromConfigXmlAsync>d__);
			return <CreateFromConfigXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06005320 RID: 21280 RVA: 0x002C76EC File Offset: 0x002C58EC
		private Task<Result<IModConfigInfo>> CreateFromLegacyAsync(ContentPackage src)
		{
			ModConfigService.<CreateFromLegacyAsync>d__17 <CreateFromLegacyAsync>d__;
			<CreateFromLegacyAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IModConfigInfo>>.Create();
			<CreateFromLegacyAsync>d__.<>4__this = this;
			<CreateFromLegacyAsync>d__.src = src;
			<CreateFromLegacyAsync>d__.<>1__state = -1;
			<CreateFromLegacyAsync>d__.<>t__builder.Start<ModConfigService.<CreateFromLegacyAsync>d__17>(ref <CreateFromLegacyAsync>d__);
			return <CreateFromLegacyAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06005321 RID: 21281 RVA: 0x002C7738 File Offset: 0x002C5938
		[CompilerGenerated]
		internal static ImmutableArray<ResourceParserInfo> <CreateFromConfigXmlAsync>g__GetResourceElementsWithName|16_9(ContentPackage package, XElement root, string elemName, string groupName)
		{
			ImmutableArray<ResourceParserInfo>.Builder elems = ImmutableArray.CreateBuilder<ResourceParserInfo>();
			elems.AddRange((from e in root.GetChildElements(elemName, StringComparison.OrdinalIgnoreCase)
			select new ResourceParserInfo(package, e, ImmutableArray<Identifier>.Empty, ImmutableArray<Identifier>.Empty)).ToImmutableArray<ResourceParserInfo>());
			ImmutableArray<XElement> fileGroups = root.GetChildElements(groupName, StringComparison.OrdinalIgnoreCase).ToImmutableArray<XElement>();
			if (!fileGroups.IsDefaultOrEmpty)
			{
				foreach (XElement fileGroup in fileGroups)
				{
					ImmutableArray<XElement> subLuaElems = fileGroup.GetChildElements(elemName, StringComparison.OrdinalIgnoreCase).ToImmutableArray<XElement>();
					if (!subLuaElems.IsDefaultOrEmpty)
					{
						ImmutableArray<Identifier> cond = ModConfigService.<CreateFromConfigXmlAsync>g__GetDependencyIdentifiers|16_10(fileGroup, true);
						ImmutableArray<Identifier> negCond = ModConfigService.<CreateFromConfigXmlAsync>g__GetDependencyIdentifiers|16_10(fileGroup, false);
						foreach (XElement element in subLuaElems)
						{
							elems.Add(new ResourceParserInfo(package, element, cond, negCond));
						}
					}
				}
			}
			return elems.ToImmutable();
		}

		// Token: 0x06005322 RID: 21282 RVA: 0x002C7818 File Offset: 0x002C5A18
		[CompilerGenerated]
		internal static ImmutableArray<Identifier> <CreateFromConfigXmlAsync>g__GetDependencyIdentifiers|16_10(XElement fg, bool depsLoadedSetting)
		{
			return fg.GetChildElements("Conditional", StringComparison.OrdinalIgnoreCase).Where(delegate(XElement cElem)
			{
				bool isLoaded;
				return bool.TryParse(cElem.GetAttribute("IsLoaded", StringComparison.OrdinalIgnoreCase).Value, out isLoaded) && isLoaded == depsLoadedSetting;
			}).SelectMany((XElement cElem2) => from ident in cElem2.GetAttributeString("Dependencies", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			select new Identifier(ident)).ToImmutableArray<Identifier>();
		}

		// Token: 0x06005323 RID: 21283 RVA: 0x002C7878 File Offset: 0x002C5A78
		[CompilerGenerated]
		private ImmutableArray<IAssemblyResourceInfo> <CreateFromLegacyAsync>g__GetAssembliesLegacy|17_0(ContentPackage srcPackage)
		{
			ValueTuple<string, Target, Platform>[] binSearchInd = new ValueTuple<string, Target, Platform>[]
			{
				new ValueTuple<string, Target, Platform>("bin/Client/Windows", Target.Client, Platform.Windows),
				new ValueTuple<string, Target, Platform>("bin/Client/Linux", Target.Client, Platform.Linux),
				new ValueTuple<string, Target, Platform>("bin/Client/OSX", Target.Client, Platform.OSX),
				new ValueTuple<string, Target, Platform>("bin/Server/Windows", Target.Server, Platform.Windows),
				new ValueTuple<string, Target, Platform>("bin/Server/Linux", Target.Server, Platform.Linux),
				new ValueTuple<string, Target, Platform>("bin/Server/OSX", Target.Server, Platform.OSX)
			};
			ImmutableArray<IAssemblyResourceInfo>.Builder builder = ImmutableArray.CreateBuilder<IAssemblyResourceInfo>();
			Func<string, ContentPath> <>9__4;
			foreach (ValueTuple<string, Target, Platform> searchPathways in binSearchInd)
			{
				Result<ImmutableArray<string>> result = this._storageService.FindFilesInPackage(srcPackage, searchPathways.Item1, "*.dll", true);
				if (result != null && result.IsSuccess && !result.Value.IsDefaultOrEmpty)
				{
					ImmutableArray<IAssemblyResourceInfo>.Builder builder2 = builder;
					AssemblyResourceInfo assemblyResourceInfo = new AssemblyResourceInfo();
					assemblyResourceInfo.OwnerPackage = srcPackage;
					assemblyResourceInfo.InternalName = searchPathways.Item1;
					assemblyResourceInfo.SupportedPlatforms = searchPathways.Item3;
					assemblyResourceInfo.SupportedTargets = searchPathways.Item2;
					assemblyResourceInfo.LoadPriority = 0;
					ImmutableArray<string> value = result.Value;
					Func<string, ContentPath> selector;
					if ((selector = <>9__4) == null)
					{
						selector = (<>9__4 = ((string fp) => ContentPath.FromRaw(srcPackage, ("%ModDir%/" + Path.GetRelativePath(srcPackage.Dir, fp)).CleanUpPathCrossPlatform(true, ""))));
					}
					assemblyResourceInfo.FilePaths = value.Select(selector).ToImmutableArray<ContentPath>();
					assemblyResourceInfo.FriendlyName = srcPackage.Name + "." + searchPathways.Item1.Replace('/', '.');
					assemblyResourceInfo.IncompatiblePackages = ImmutableArray<Identifier>.Empty;
					assemblyResourceInfo.RequiredPackages = ImmutableArray<Identifier>.Empty;
					assemblyResourceInfo.IsScript = false;
					assemblyResourceInfo.IsReferenceModeOnly = false;
					builder2.Add(assemblyResourceInfo);
				}
			}
			Result<ImmutableArray<string>> sharedResult = this._storageService.FindFilesInPackage(srcPackage, Path.Combine(new string[]
			{
				"CSharp/Shared"
			}), "*.cs", true);
			ImmutableArray<ContentPath> sharedFiles = (sharedResult.IsSuccess && !sharedResult.Value.IsDefaultOrEmpty) ? (from fp in sharedResult.Value
			select ContentPath.FromRaw(srcPackage, ("%ModDir%/" + Path.GetRelativePath(srcPackage.Dir, fp)).CleanUpPathCrossPlatform(true, ""))).ToImmutableArray<ContentPath>() : ImmutableArray<ContentPath>.Empty;
			ValueTuple<string, Target, Platform>[] srcSearchInd = new ValueTuple<string, Target, Platform>[]
			{
				new ValueTuple<string, Target, Platform>("CSharp/Client", Target.Client, Platform.Any),
				new ValueTuple<string, Target, Platform>("CSharp/Server", Target.Server, Platform.Any)
			};
			Func<string, ContentPath> <>9__5;
			foreach (ValueTuple<string, Target, Platform> searchPathways2 in srcSearchInd)
			{
				Result<ImmutableArray<string>> result2 = this._storageService.FindFilesInPackage(srcPackage, searchPathways2.Item1, "*.cs", true);
				if (result2 != null && result2.IsSuccess && !result2.Value.IsDefaultOrEmpty)
				{
					ImmutableArray<IAssemblyResourceInfo>.Builder builder3 = builder;
					AssemblyResourceInfo assemblyResourceInfo2 = new AssemblyResourceInfo();
					assemblyResourceInfo2.OwnerPackage = srcPackage;
					assemblyResourceInfo2.InternalName = searchPathways2.Item1;
					assemblyResourceInfo2.SupportedPlatforms = searchPathways2.Item3;
					assemblyResourceInfo2.SupportedTargets = searchPathways2.Item2;
					assemblyResourceInfo2.LoadPriority = 0;
					ImmutableArray<string> value2 = result2.Value;
					Func<string, ContentPath> selector2;
					if ((selector2 = <>9__5) == null)
					{
						selector2 = (<>9__5 = ((string fp) => ContentPath.FromRaw(srcPackage, ("%ModDir%/" + Path.GetRelativePath(srcPackage.Dir, fp)).CleanUpPathCrossPlatform(true, ""))));
					}
					assemblyResourceInfo2.FilePaths = value2.Select(selector2).Concat(sharedFiles).ToImmutableArray<ContentPath>();
					assemblyResourceInfo2.FriendlyName = "InternalsAwareAssembly";
					assemblyResourceInfo2.IncompatiblePackages = ImmutableArray<Identifier>.Empty;
					assemblyResourceInfo2.RequiredPackages = ImmutableArray<Identifier>.Empty;
					assemblyResourceInfo2.UseInternalAccessName = false;
					assemblyResourceInfo2.IsScript = true;
					assemblyResourceInfo2.IsReferenceModeOnly = false;
					builder3.Add(assemblyResourceInfo2);
				}
				else if (!sharedFiles.IsDefaultOrEmpty)
				{
					builder.Add(new AssemblyResourceInfo
					{
						OwnerPackage = srcPackage,
						InternalName = searchPathways2.Item1,
						SupportedPlatforms = searchPathways2.Item3,
						SupportedTargets = searchPathways2.Item2,
						LoadPriority = 0,
						FilePaths = sharedFiles,
						FriendlyName = "InternalsAwareAssembly",
						IncompatiblePackages = ImmutableArray<Identifier>.Empty,
						RequiredPackages = ImmutableArray<Identifier>.Empty,
						UseInternalAccessName = false,
						IsScript = true,
						IsReferenceModeOnly = false
					});
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x06005324 RID: 21284 RVA: 0x002C7CAC File Offset: 0x002C5EAC
		[CompilerGenerated]
		internal static ImmutableArray<IConfigResourceInfo> <CreateFromLegacyAsync>g__GetConfigsLegacy|17_1(ContentPackage src)
		{
			return ImmutableArray<IConfigResourceInfo>.Empty;
		}

		// Token: 0x06005325 RID: 21285 RVA: 0x002C7CB4 File Offset: 0x002C5EB4
		[CompilerGenerated]
		private ImmutableArray<ILuaScriptResourceInfo> <CreateFromLegacyAsync>g__GetLuaScriptsLegacy|17_2(ContentPackage src)
		{
			ImmutableArray<ILuaScriptResourceInfo>.Builder builder = ImmutableArray.CreateBuilder<ILuaScriptResourceInfo>();
			Result<ImmutableArray<string>> result = this._storageService.FindFilesInPackage(src, "Lua", "*.lua", true);
			if (result != null && result.IsSuccess && !result.Value.IsDefaultOrEmpty)
			{
				ImmutableArray<string> cleanedResult = (from fp in result.Value
				select fp.CleanUpPathCrossPlatform(true, "")).ToImmutableArray<string>();
				ImmutableArray<string> autorun = (from fp in cleanedResult
				where fp.Contains("Lua/ForcedAutorun/") || fp.Contains("Lua/Autorun/")
				select fp).ToImmutableArray<string>();
				ImmutableArray<ContentPath> autorunFP = (from fp in autorun
				select ContentPath.FromRaw(src, ("%ModDir%/" + Path.GetRelativePath(src.Dir, fp)).CleanUpPathCrossPlatform(true, ""))).ToImmutableArray<ContentPath>();
				ImmutableArray<ContentPath> reg = (from fp in cleanedResult.Except(autorun)
				select ContentPath.FromRaw(src, ("%ModDir%/" + Path.GetRelativePath(src.Dir, fp)).CleanUpPathCrossPlatform(true, ""))).ToImmutableArray<ContentPath>();
				builder.Add(new LuaScriptsResourceInfo
				{
					OwnerPackage = src,
					InternalName = "LegacyAutorun",
					SupportedPlatforms = Platform.Any,
					SupportedTargets = Target.Any,
					LoadPriority = 1,
					FilePaths = autorunFP,
					IncompatiblePackages = ImmutableArray<Identifier>.Empty,
					RequiredPackages = ImmutableArray<Identifier>.Empty,
					IsAutorun = true,
					RunUnrestricted = false
				});
				builder.Add(new LuaScriptsResourceInfo
				{
					OwnerPackage = src,
					InternalName = "Legacy",
					SupportedPlatforms = Platform.Any,
					SupportedTargets = Target.Any,
					LoadPriority = 0,
					FilePaths = reg,
					IncompatiblePackages = ImmutableArray<Identifier>.Empty,
					RequiredPackages = ImmutableArray<Identifier>.Empty,
					IsAutorun = false,
					RunUnrestricted = false
				});
			}
			return builder.ToImmutable();
		}

		// Token: 0x04002BEA RID: 11242
		private IStorageService _storageService;

		// Token: 0x04002BEB RID: 11243
		private ILoggerService _logger;

		// Token: 0x04002BEC RID: 11244
		private IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo> _assemblyParserService;

		// Token: 0x04002BED RID: 11245
		private IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo> _luaScriptParserService;

		// Token: 0x04002BEE RID: 11246
		private IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo> _configParserService;

		// Token: 0x04002BEF RID: 11247
		private IParserServiceAsync<ResourceParserInfo, IStylesResourceInfo> _stylesParserService;

		// Token: 0x04002BF0 RID: 11248
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002BF1 RID: 11249
		private int _isDisposed;
	}
}
