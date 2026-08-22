using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F6 RID: 1014
	public sealed class SettingsFileParserService : IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo>, IService, IDisposable, IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo>
	{
		// Token: 0x06003A61 RID: 14945 RVA: 0x00187834 File Offset: 0x00185A34
		public void Dispose()
		{
			using (this._operationLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this._storageService.Dispose();
					this._storageService = null;
				}
			}
		}

		// Token: 0x17000FB9 RID: 4025
		// (get) Token: 0x06003A62 RID: 14946 RVA: 0x001878AC File Offset: 0x00185AAC
		// (set) Token: 0x06003A63 RID: 14947 RVA: 0x001878B9 File Offset: 0x00185AB9
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

		// Token: 0x06003A64 RID: 14948 RVA: 0x001878C7 File Offset: 0x00185AC7
		public SettingsFileParserService(IStorageService storageService)
		{
			this._storageService = storageService;
		}

		// Token: 0x06003A65 RID: 14949 RVA: 0x001878E4 File Offset: 0x00185AE4
		Task<Result<ImmutableArray<IConfigInfo>>> IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo>.TryParseResourcesAsync(IConfigResourceInfo src)
		{
			SettingsFileParserService.<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__8 <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ImmutableArray<IConfigInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<SettingsFileParserService.<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__8>(ref <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06003A66 RID: 14950 RVA: 0x00187930 File Offset: 0x00185B30
		Task<Result<ImmutableArray<IConfigProfileInfo>>> IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo>.TryParseResourcesAsync(IConfigResourceInfo src)
		{
			SettingsFileParserService.<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__9 <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ImmutableArray<IConfigProfileInfo>>>.Create();
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.<>4__this = this;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.src = src;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.<>1__state = -1;
			<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.<>t__builder.Start<SettingsFileParserService.<Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__9>(ref <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__);
			return <Barotrauma-LuaCs-IParserServiceOneToManyAsync<Barotrauma-LuaCs-Data-IConfigResourceInfo,Barotrauma-LuaCs-Data-IConfigProfileInfo>-TryParseResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06003A67 RID: 14951 RVA: 0x0018797B File Offset: 0x00185B7B
		[CompilerGenerated]
		internal static Result <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigInfo>.TryParseResourcesAsync>g__ReturnFail|8_0(string msg)
		{
			return Result.Fail("TryParseResourcesAsync: " + msg);
		}

		// Token: 0x06003A68 RID: 14952 RVA: 0x0018798D File Offset: 0x00185B8D
		[CompilerGenerated]
		internal static bool <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigInfo>.TryParseResourcesAsync>g__IsInfoValid|8_1(ConfigInfo info)
		{
			return info.OwnerPackage != null && !info.InternalName.IsNullOrWhiteSpace() && !info.DataType.IsNullOrWhiteSpace() && info.Element != null;
		}

		// Token: 0x06003A69 RID: 14953 RVA: 0x001879BC File Offset: 0x00185BBC
		[CompilerGenerated]
		internal static Result <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigProfileInfo>.TryParseResourcesAsync>g__ReturnFail|9_0(string msg)
		{
			return Result.Fail("TryParseResourcesAsync: " + msg);
		}

		// Token: 0x04001D44 RID: 7492
		private AsyncReaderWriterLock _operationLock = new AsyncReaderWriterLock();

		// Token: 0x04001D45 RID: 7493
		private int _isDisposed;

		// Token: 0x04001D46 RID: 7494
		private IStorageService _storageService;
	}
}
