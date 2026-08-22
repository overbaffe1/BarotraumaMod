using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050A RID: 1290
	public sealed class SettingsFileParserService : IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigInfo>, IService, IDisposable, IParserServiceOneToManyAsync<IConfigResourceInfo, IConfigProfileInfo>
	{
		// Token: 0x06005385 RID: 21381 RVA: 0x002CBF74 File Offset: 0x002CA174
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

		// Token: 0x17001500 RID: 5376
		// (get) Token: 0x06005386 RID: 21382 RVA: 0x002CBFEC File Offset: 0x002CA1EC
		// (set) Token: 0x06005387 RID: 21383 RVA: 0x002CBFF9 File Offset: 0x002CA1F9
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

		// Token: 0x06005388 RID: 21384 RVA: 0x002CC007 File Offset: 0x002CA207
		public SettingsFileParserService(IStorageService storageService)
		{
			this._storageService = storageService;
		}

		// Token: 0x06005389 RID: 21385 RVA: 0x002CC024 File Offset: 0x002CA224
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

		// Token: 0x0600538A RID: 21386 RVA: 0x002CC070 File Offset: 0x002CA270
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

		// Token: 0x0600538B RID: 21387 RVA: 0x002CC0BB File Offset: 0x002CA2BB
		[CompilerGenerated]
		internal static Result <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigInfo>.TryParseResourcesAsync>g__ReturnFail|8_0(string msg)
		{
			return Result.Fail("TryParseResourcesAsync: " + msg);
		}

		// Token: 0x0600538C RID: 21388 RVA: 0x002CC0D0 File Offset: 0x002CA2D0
		[CompilerGenerated]
		internal static bool <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigInfo>.TryParseResourcesAsync>g__IsInfoValid|8_1(ConfigInfo info)
		{
			return info.OwnerPackage != null && !info.InternalName.IsNullOrWhiteSpace() && !info.DataType.IsNullOrWhiteSpace() && info.Element != null && !info.DisplayName.IsNullOrWhiteSpace() && !info.Description.IsNullOrWhiteSpace() && !info.DisplayCategory.IsNullOrWhiteSpace() && !info.Tooltip.IsNullOrWhiteSpace();
		}

		// Token: 0x0600538D RID: 21389 RVA: 0x002CC13E File Offset: 0x002CA33E
		[CompilerGenerated]
		internal static Result <Barotrauma.LuaCs.IParserServiceOneToManyAsync<Barotrauma.LuaCs.Data.IConfigResourceInfo,Barotrauma.LuaCs.Data.IConfigProfileInfo>.TryParseResourcesAsync>g__ReturnFail|9_0(string msg)
		{
			return Result.Fail("TryParseResourcesAsync: " + msg);
		}

		// Token: 0x04002C28 RID: 11304
		private AsyncReaderWriterLock _operationLock = new AsyncReaderWriterLock();

		// Token: 0x04002C29 RID: 11305
		private int _isDisposed;

		// Token: 0x04002C2A RID: 11306
		private IStorageService _storageService;
	}
}
