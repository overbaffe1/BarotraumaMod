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
	// Token: 0x02000536 RID: 1334
	public class LuaScriptLoader : ScriptLoaderBase, ILuaScriptLoader, IService, IDisposable, IScriptLoader, ISafeStorageValidation
	{
		// Token: 0x0600551A RID: 21786 RVA: 0x002D0756 File Offset: 0x002CE956
		public LuaScriptLoader(ISafeStorageService storageService, Lazy<ILoggerService> loggerService)
		{
			this._storageService = storageService;
			this._loggerService = loggerService;
			storageService.UseCaching = true;
		}

		// Token: 0x0600551B RID: 21787 RVA: 0x002D0774 File Offset: 0x002CE974
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

		// Token: 0x0600551C RID: 21788 RVA: 0x002D07F9 File Offset: 0x002CE9F9
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

		// Token: 0x0600551D RID: 21789 RVA: 0x002D0811 File Offset: 0x002CEA11
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

		// Token: 0x0600551E RID: 21790 RVA: 0x002D0838 File Offset: 0x002CEA38
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

		// Token: 0x0600551F RID: 21791 RVA: 0x002D0884 File Offset: 0x002CEA84
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

		// Token: 0x06005520 RID: 21792 RVA: 0x002D08D4 File Offset: 0x002CEAD4
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

		// Token: 0x06005521 RID: 21793 RVA: 0x002D0974 File Offset: 0x002CEB74
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

		// Token: 0x17001531 RID: 5425
		// (get) Token: 0x06005522 RID: 21794 RVA: 0x002D09AA File Offset: 0x002CEBAA
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
		}

		// Token: 0x06005523 RID: 21795 RVA: 0x002D09B7 File Offset: 0x002CEBB7
		public bool IsFileAccessible(string path, bool readOnly, bool checkWhitelistOnly = true)
		{
			IService.CheckDisposed(this);
			return this._storageService.IsFileAccessible(path, readOnly, checkWhitelistOnly);
		}

		// Token: 0x06005524 RID: 21796 RVA: 0x002D09CD File Offset: 0x002CEBCD
		public void AddFileToWhitelist(string path, bool readOnly = true)
		{
			IService.CheckDisposed(this);
			this._storageService.AddFileToWhitelist(path, readOnly);
		}

		// Token: 0x06005525 RID: 21797 RVA: 0x002D09E2 File Offset: 0x002CEBE2
		public void AddFilesToWhitelist(ImmutableArray<string> paths, bool readOnly = true)
		{
			IService.CheckDisposed(this);
			this._storageService.AddFilesToWhitelist(paths, readOnly);
		}

		// Token: 0x06005526 RID: 21798 RVA: 0x002D09F7 File Offset: 0x002CEBF7
		public void RemoveFileFromAllWhitelists(string path)
		{
			IService.CheckDisposed(this);
			this._storageService.RemoveFileFromAllWhitelists(path);
		}

		// Token: 0x06005527 RID: 21799 RVA: 0x002D0A0B File Offset: 0x002CEC0B
		public Result SetReadOnlyWhitelist(ImmutableArray<string> filePaths)
		{
			IService.CheckDisposed(this);
			return this._storageService.SetReadOnlyWhitelist(filePaths);
		}

		// Token: 0x06005528 RID: 21800 RVA: 0x002D0A1F File Offset: 0x002CEC1F
		public Result SetReadWriteWhitelist(ImmutableArray<string> filePaths)
		{
			IService.CheckDisposed(this);
			return this._storageService.SetReadWriteWhitelist(filePaths);
		}

		// Token: 0x06005529 RID: 21801 RVA: 0x002D0A33 File Offset: 0x002CEC33
		public void ClearAllWhitelists()
		{
			IService.CheckDisposed(this);
			this._storageService.ClearAllWhitelists();
		}

		// Token: 0x04002C65 RID: 11365
		private readonly ISafeStorageService _storageService;

		// Token: 0x04002C66 RID: 11366
		private readonly Lazy<ILoggerService> _loggerService;

		// Token: 0x04002C67 RID: 11367
		private int _isDisposed;
	}
}
