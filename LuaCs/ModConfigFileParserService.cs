using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E5 RID: 1253
	public sealed class ModConfigFileParserService : IParserServiceAsync<ResourceParserInfo, IStylesResourceInfo>, IService, IDisposable, IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>, IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>, IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>
	{
		// Token: 0x060051AC RID: 20908 RVA: 0x002BF530 File Offset: 0x002BD730
		Task<Result<IStylesResourceInfo>> IParserServiceAsync<ResourceParserInfo, IStylesResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__0 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IStylesResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__0>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IStylesResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051AD RID: 20909 RVA: 0x002BF57C File Offset: 0x002BD77C
		public Task<ImmutableArray<Result<IStylesResourceInfo>>> TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<TryParseResourcesAsync>d__1 <TryParseResourcesAsync>d__;
			<TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<IStylesResourceInfo>>>.Create();
			<TryParseResourcesAsync>d__.<>4__this = this;
			<TryParseResourcesAsync>d__.sources = sources;
			<TryParseResourcesAsync>d__.<>1__state = -1;
			<TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<TryParseResourcesAsync>d__1>(ref <TryParseResourcesAsync>d__);
			return <TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051AE RID: 20910 RVA: 0x002BF5C7 File Offset: 0x002BD7C7
		public ModConfigFileParserService(IStorageService storageService)
		{
			this._storageService = storageService;
			this._storageService.UseCaching = false;
		}

		// Token: 0x060051AF RID: 20911 RVA: 0x002BF5F0 File Offset: 0x002BD7F0
		public void Dispose()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					try
					{
						this._storageService.Dispose();
						this._storageService = null;
					}
					catch
					{
					}
				}
			}
		}

		// Token: 0x170014CD RID: 5325
		// (get) Token: 0x060051B0 RID: 20912 RVA: 0x002BF678 File Offset: 0x002BD878
		// (set) Token: 0x060051B1 RID: 20913 RVA: 0x002BF685 File Offset: 0x002BD885
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

		// Token: 0x060051B2 RID: 20914 RVA: 0x002BF694 File Offset: 0x002BD894
		Task<Result<IAssemblyResourceInfo>> IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__10 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IAssemblyResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__10>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B3 RID: 20915 RVA: 0x002BF6E0 File Offset: 0x002BD8E0
		Task<ImmutableArray<Result<IAssemblyResourceInfo>>> IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__11 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<IAssemblyResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__11>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B4 RID: 20916 RVA: 0x002BF72C File Offset: 0x002BD92C
		Task<Result<IConfigResourceInfo>> IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__12 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IConfigResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__12>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B5 RID: 20917 RVA: 0x002BF778 File Offset: 0x002BD978
		Task<ImmutableArray<Result<IConfigResourceInfo>>> IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__13 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<IConfigResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__13>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B6 RID: 20918 RVA: 0x002BF7C4 File Offset: 0x002BD9C4
		Task<Result<ILuaScriptResourceInfo>> IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__14 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ILuaScriptResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__14>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B7 RID: 20919 RVA: 0x002BF810 File Offset: 0x002BDA10
		private Result CheckThrowNullRefs(ResourceParserInfo src, string elementName)
		{
			Guard.IsNotNull<ResourceParserInfo>(src, "src");
			Guard.IsNotNull<ContentPackage>(src.Owner, "Owner");
			Guard.IsNotNull<XElement>(src.Element, "Element");
			if (src.Element.Name != elementName)
			{
				return Result.Fail("Element name '" + elementName + "' is incorrect");
			}
			return Result.Ok();
		}

		// Token: 0x060051B8 RID: 20920 RVA: 0x002BF87C File Offset: 0x002BDA7C
		Task<ImmutableArray<Result<ILuaScriptResourceInfo>>> IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__16 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<ILuaScriptResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__16>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051B9 RID: 20921 RVA: 0x002BF8C8 File Offset: 0x002BDAC8
		private Task<Result<ImmutableArray<ContentPath>>> UnsafeGetCheckedFiles(XElement srcElement, ContentPackage srcOwner, string fileExtension)
		{
			ModConfigFileParserService.<UnsafeGetCheckedFiles>d__17 <UnsafeGetCheckedFiles>d__;
			<UnsafeGetCheckedFiles>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ImmutableArray<ContentPath>>>.Create();
			<UnsafeGetCheckedFiles>d__.<>4__this = this;
			<UnsafeGetCheckedFiles>d__.srcElement = srcElement;
			<UnsafeGetCheckedFiles>d__.srcOwner = srcOwner;
			<UnsafeGetCheckedFiles>d__.fileExtension = fileExtension;
			<UnsafeGetCheckedFiles>d__.<>1__state = -1;
			<UnsafeGetCheckedFiles>d__.<>t__builder.Start<ModConfigFileParserService.<UnsafeGetCheckedFiles>d__17>(ref <UnsafeGetCheckedFiles>d__);
			return <UnsafeGetCheckedFiles>d__.<>t__builder.Task;
		}

		// Token: 0x060051BA RID: 20922 RVA: 0x002BF923 File Offset: 0x002BDB23
		[return: TupleElementNames(new string[]
		{
			"Platform",
			"Target"
		})]
		private ValueTuple<Platform, Target> GetRuntimeEnvironment(XElement element)
		{
			return new ValueTuple<Platform, Target>(element.GetAttributeEnum("Platform", Platform.Any), element.GetAttributeEnum("Target", Target.Any));
		}

		// Token: 0x060051BB RID: 20923 RVA: 0x002BF944 File Offset: 0x002BDB44
		private Task<ImmutableArray<Result<T>>> TryParseGenericResourcesAsync<T>(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<TryParseGenericResourcesAsync>d__19<T> <TryParseGenericResourcesAsync>d__;
			<TryParseGenericResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<T>>>.Create();
			<TryParseGenericResourcesAsync>d__.<>4__this = this;
			<TryParseGenericResourcesAsync>d__.sources = sources;
			<TryParseGenericResourcesAsync>d__.<>1__state = -1;
			<TryParseGenericResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<TryParseGenericResourcesAsync>d__19<T>>(ref <TryParseGenericResourcesAsync>d__);
			return <TryParseGenericResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060051BC RID: 20924 RVA: 0x002BF990 File Offset: 0x002BDB90
		[CompilerGenerated]
		internal static string <Barotrauma.LuaCs.IParserServiceAsync<Barotrauma.LuaCs.Data.ResourceParserInfo,Barotrauma.LuaCs.Data.IAssemblyResourceInfo>.TryParseResourceAsync>g__GetFallbackCompliantAssemblyName|10_0(ContentPackage package)
		{
			if (package.Name.IsNullOrWhiteSpace())
			{
				return "FallbackAssemblyName";
			}
			string sanitizedPackageName = Regex.Replace(package.Name, "[^a-zA-Z0-9_]", "_");
			if (char.IsDigit(sanitizedPackageName[0]))
			{
				sanitizedPackageName = "ASM" + sanitizedPackageName;
			}
			return Regex.Replace(sanitizedPackageName, "[_.]{2,}", "_");
		}

		// Token: 0x04002B50 RID: 11088
		private IStorageService _storageService;

		// Token: 0x04002B51 RID: 11089
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002B52 RID: 11090
		private int _isDisposed;
	}
}
