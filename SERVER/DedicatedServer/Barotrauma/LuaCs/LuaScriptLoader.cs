using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000423 RID: 1059
	public class LuaScriptLoader : ScriptLoaderBase, ILuaScriptLoader, IService, IDisposable, IScriptLoader, ISafeStorageValidation
	{
		// Token: 0x06003BFF RID: 15359 RVA: 0x0018BF96 File Offset: 0x0018A196
		public LuaScriptLoader(ISafeStorageService storageService, Lazy<ILoggerService> loggerService)
		{
			this._storageService = storageService;
			this._loggerService = loggerService;
			storageService.UseCaching = true;
		}

		// Token: 0x06003C00 RID: 15360 RVA: 0x0018BFB4 File Offset: 0x0018A1B4
		public override object LoadFile(string file, Table globalContext)
		{
			IService.CheckDisposed(this);
			if (file.IsNullOrWhiteSpace())
			{
				return null;
			}
			Result<string> res = this._storageService.TryLoadText(file, null);
			if (!res.IsFailed && res != null)
			{
				string script = res.Value;
				if (script != null)
				{
					if (script.IsNullOrWhiteSpace())
					{
						this.UnsafeLogErrors("The file '" + file + "' is  empty. ", res.ToResult());
						return null;
					}
					return script;
				}
			}
			this.UnsafeLogErrors("Failed to load file '" + file + "'.", res.ToResult());
			return null;
		}

		// Token: 0x06003C01 RID: 15361 RVA: 0x0018C039 File Offset: 0x0018A239
		public void ClearCaches()
		{
			IService.CheckDisposed(this);
			ISafeStorageService storageService = this._storageService;
			if (storageService == null)
			{
				return;
			}
			storageService.PurgeCache();
		}

		// Token: 0x06003C02 RID: 15362 RVA: 0x0018C051 File Offset: 0x0018A251
		public void SetCachingPolicy(bool useCaching)
		{
			if (this._storageService == null)
			{
				return;
			}
			if (!useCaching)
			{
				this._storageService.PurgeCache();
			}
			this._storageService.UseCaching = useCaching;
		}

		// Token: 0x06003C03 RID: 15363 RVA: 0x0018C078 File Offset: 0x0018A278
		[return: TupleElementNames(new string[]
		{
			"Path",
			null
		})]
		public Task<Result<ImmutableArray<ValueTuple<ContentPath, Result<string>>>>> CacheResourcesAsync(ImmutableArray<ILuaScriptResourceInfo> resourceInfos)
		{
			LuaScriptLoader.<CacheResourcesAsync>d__6 <CacheResourcesAsync>d__;
			<CacheResourcesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<ImmutableArray<ValueTuple<ContentPath, Result<string>>>>>.Create();
			<CacheResourcesAsync>d__.<>4__this = this;
			<CacheResourcesAsync>d__.resourceInfos = resourceInfos;
			<CacheResourcesAsync>d__.<>1__state = -1;
			<CacheResourcesAsync>d__.<>t__builder.Start<LuaScriptLoader.<CacheResourcesAsync>d__6>(ref <CacheResourcesAsync>d__);
			return <CacheResourcesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06003C04 RID: 15364 RVA: 0x0018C0C4 File Offset: 0x0018A2C4
		public override bool ScriptFileExists(string file)
		{
			IService.CheckDisposed(this);
			Result<bool> result = this._storageService.FileExists(file);
			if (result != null && result.IsFailed)
			{
				this.UnsafeLogErrors("Unable to find and load file \"" + file + "\".", result.ToResult());
				return false;
			}
			return result.Value;
		}

		// Token: 0x06003C05 RID: 15365 RVA: 0x0018C114 File Offset: 0x0018A314
		private void UnsafeLogErrors(string message, Result result = null)
		{
			this._loggerService.Value.LogError("LuaScriptLoader: " + message);
			if (result == null || result.Errors.Count <= 0)
			{
				return;
			}
			foreach (IError error in result.Errors)
			{
				this._loggerService.Value.LogError("LuaScriptLoader: Error: " + error.Message + ".");
			}
		}

		// Token: 0x06003C06 RID: 15366 RVA: 0x0018C1B4 File Offset: 0x0018A3B4
		public void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			ISafeStorageService storageService = this._storageService;
			if (storageService != null)
			{
				storageService.Dispose();
			}
			Lazy<ILoggerService> loggerService = this._loggerService;
			if (loggerService == null)
			{
				return;
			}
			loggerService.Value.Dispose();
		}

		// Token: 0x17000FE1 RID: 4065
		// (get) Token: 0x06003C07 RID: 15367 RVA: 0x0018C1EA File Offset: 0x0018A3EA
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
		}

		// Token: 0x06003C08 RID: 15368 RVA: 0x0018C1F7 File Offset: 0x0018A3F7
		public bool IsFileAccessible(string path, bool readOnly, bool checkWhitelistOnly = true)
		{
			IService.CheckDisposed(this);
			return this._storageService.IsFileAccessible(path, readOnly, checkWhitelistOnly);
		}

		// Token: 0x06003C09 RID: 15369 RVA: 0x0018C20D File Offset: 0x0018A40D
		public void AddFileToWhitelist(string path, bool readOnly = true)
		{
			IService.CheckDisposed(this);
			this._storageService.AddFileToWhitelist(path, readOnly);
		}

		// Token: 0x06003C0A RID: 15370 RVA: 0x0018C222 File Offset: 0x0018A422
		public void AddFilesToWhitelist(ImmutableArray<string> paths, bool readOnly = true)
		{
			IService.CheckDisposed(this);
			this._storageService.AddFilesToWhitelist(paths, readOnly);
		}

		// Token: 0x06003C0B RID: 15371 RVA: 0x0018C237 File Offset: 0x0018A437
		public void RemoveFileFromAllWhitelists(string path)
		{
			IService.CheckDisposed(this);
			this._storageService.RemoveFileFromAllWhitelists(path);
		}

		// Token: 0x06003C0C RID: 15372 RVA: 0x0018C24B File Offset: 0x0018A44B
		public Result SetReadOnlyWhitelist(ImmutableArray<string> filePaths)
		{
			IService.CheckDisposed(this);
			return this._storageService.SetReadOnlyWhitelist(filePaths);
		}

		// Token: 0x06003C0D RID: 15373 RVA: 0x0018C25F File Offset: 0x0018A45F
		public Result SetReadWriteWhitelist(ImmutableArray<string> filePaths)
		{
			IService.CheckDisposed(this);
			return this._storageService.SetReadWriteWhitelist(filePaths);
		}

		// Token: 0x06003C0E RID: 15374 RVA: 0x0018C273 File Offset: 0x0018A473
		public void ClearAllWhitelists()
		{
			IService.CheckDisposed(this);
			this._storageService.ClearAllWhitelists();
		}

		// Token: 0x04001D81 RID: 7553
		private readonly ISafeStorageService _storageService;

		// Token: 0x04001D82 RID: 7554
		private readonly Lazy<ILoggerService> _loggerService;

		// Token: 0x04001D83 RID: 7555
		private int _isDisposed;
	}
}
