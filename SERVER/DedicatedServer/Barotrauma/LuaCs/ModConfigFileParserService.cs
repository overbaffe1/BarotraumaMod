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
	// Token: 0x020003EE RID: 1006
	public sealed class ModConfigFileParserService : IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>, IService, IDisposable, IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>, IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>
	{
		// Token: 0x060039E5 RID: 14821 RVA: 0x001829B9 File Offset: 0x00180BB9
		public ModConfigFileParserService(IStorageService storageService)
		{
			this._storageService = storageService;
			this._storageService.UseCaching = false;
		}

		// Token: 0x060039E6 RID: 14822 RVA: 0x001829E0 File Offset: 0x00180BE0
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

		// Token: 0x17000FB2 RID: 4018
		// (get) Token: 0x060039E7 RID: 14823 RVA: 0x00182A68 File Offset: 0x00180C68
		// (set) Token: 0x060039E8 RID: 14824 RVA: 0x00182A75 File Offset: 0x00180C75
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

		// Token: 0x060039E9 RID: 14825 RVA: 0x00182A84 File Offset: 0x00180C84
		Task<Result<IAssemblyResourceInfo>> IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__8 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IAssemblyResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__8>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039EA RID: 14826 RVA: 0x00182AD0 File Offset: 0x00180CD0
		Task<ImmutableArray<Result<IAssemblyResourceInfo>>> IParserServiceAsync<ResourceParserInfo, IAssemblyResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__9 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<IAssemblyResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__9>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IAssemblyResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039EB RID: 14827 RVA: 0x00182B1C File Offset: 0x00180D1C
		Task<Result<IConfigResourceInfo>> IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__10 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<IConfigResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__10>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039EC RID: 14828 RVA: 0x00182B68 File Offset: 0x00180D68
		Task<ImmutableArray<Result<IConfigResourceInfo>>> IParserServiceAsync<ResourceParserInfo, IConfigResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__11 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<IConfigResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__11>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-IConfigResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039ED RID: 14829 RVA: 0x00182BB4 File Offset: 0x00180DB4
		Task<Result<ILuaScriptResourceInfo>> IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>.TryParseResourceAsync(ResourceParserInfo src)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__12 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ILuaScriptResourceInfo>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__12>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourceAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039EE RID: 14830 RVA: 0x00182C00 File Offset: 0x00180E00
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

		// Token: 0x060039EF RID: 14831 RVA: 0x00182C6C File Offset: 0x00180E6C
		Task<ImmutableArray<Result<ILuaScriptResourceInfo>>> IParserServiceAsync<ResourceParserInfo, ILuaScriptResourceInfo>.TryParseResourcesAsync(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__14 <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<ILuaScriptResourceInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.sources = sources;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__14>(ref <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceAsync<Barotrauma-LuaCs-Data-ResourceParserInfo,Barotrauma-LuaCs-Data-ILuaScriptResourceInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039F0 RID: 14832 RVA: 0x00182CB8 File Offset: 0x00180EB8
		private Task<Result<ImmutableArray<ContentPath>>> UnsafeGetCheckedFiles(XElement srcElement, ContentPackage srcOwner, string fileExtension)
		{
			ModConfigFileParserService.<UnsafeGetCheckedFiles>d__15 <UnsafeGetCheckedFiles>d__;
			<UnsafeGetCheckedFiles>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ImmutableArray<ContentPath>>>.Create();
			<UnsafeGetCheckedFiles>d__.<>4__this = this;
			<UnsafeGetCheckedFiles>d__.srcElement = srcElement;
			<UnsafeGetCheckedFiles>d__.srcOwner = srcOwner;
			<UnsafeGetCheckedFiles>d__.fileExtension = fileExtension;
			<UnsafeGetCheckedFiles>d__.<>1__state = -1;
			<UnsafeGetCheckedFiles>d__.<>t__builder.Start<ModConfigFileParserService.<UnsafeGetCheckedFiles>d__15>(ref <UnsafeGetCheckedFiles>d__);
			return <UnsafeGetCheckedFiles>d__.<>t__builder.Task;
		}

		// Token: 0x060039F1 RID: 14833 RVA: 0x00182D13 File Offset: 0x00180F13
		[return: TupleElementNames(new string[]
		{
			"Platform",
			"Target"
		})]
		private ValueTuple<Platform, Target> GetRuntimeEnvironment(XElement element)
		{
			return new ValueTuple<Platform, Target>(element.GetAttributeEnum("Platform", Platform.Any), element.GetAttributeEnum("Target", Target.Any));
		}

		// Token: 0x060039F2 RID: 14834 RVA: 0x00182D34 File Offset: 0x00180F34
		private Task<ImmutableArray<Result<T>>> TryParseGenericResourcesAsync<T>(IEnumerable<ResourceParserInfo> sources)
		{
			ModConfigFileParserService.<TryParseGenericResourcesAsync>d__17<T> <TryParseGenericResourcesAsync>d__;
			<TryParseGenericResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<Result<T>>>.Create();
			<TryParseGenericResourcesAsync>d__.<>4__this = this;
			<TryParseGenericResourcesAsync>d__.sources = sources;
			<TryParseGenericResourcesAsync>d__.<>1__state = -1;
			<TryParseGenericResourcesAsync>d__.<>t__builder.Start<ModConfigFileParserService.<TryParseGenericResourcesAsync>d__17<T>>(ref <TryParseGenericResourcesAsync>d__);
			return <TryParseGenericResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060039F3 RID: 14835 RVA: 0x00182D80 File Offset: 0x00180F80
		[CompilerGenerated]
		internal static string <Barotrauma.LuaCs.IParserServiceAsync<Barotrauma.LuaCs.Data.ResourceParserInfo,Barotrauma.LuaCs.Data.IAssemblyResourceInfo>.TryParseResourceAsync>g__GetFallbackCompliantAssemblyName|8_0(ContentPackage package)
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

		// Token: 0x04001D05 RID: 7429
		private IStorageService _storageService;

		// Token: 0x04001D06 RID: 7430
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04001D07 RID: 7431
		private int _isDisposed;
	}
}
