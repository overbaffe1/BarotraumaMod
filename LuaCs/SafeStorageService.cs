using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.LuaCs.Data;
using FluentResults;
using FluentResults.LuaCs;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000507 RID: 1287
	public class SafeStorageService : StorageService, ISafeStorageService, IStorageService, IService, IDisposable, ISafeStorageValidation
	{
		// Token: 0x06005367 RID: 21351 RVA: 0x002CB190 File Offset: 0x002C9390
		public SafeStorageService(IStorageServiceConfig configData) : base(configData)
		{
			base.IsReadOperationAllowedEval = ((string fp) => this.IsFileAccessible(fp, true, true));
			base.IsWriteOperationAllowedEval = ((string fp) => this.IsFileAccessible(fp, false, true));
		}

		// Token: 0x06005368 RID: 21352 RVA: 0x002CB1E9 File Offset: 0x002C93E9
		private string GetFullPath(string path)
		{
			return Path.GetFullPath(path).CleanUpPathCrossPlatform(true, "");
		}

		// Token: 0x06005369 RID: 21353 RVA: 0x002CB1FC File Offset: 0x002C93FC
		public bool IsFileAccessible(string path, bool readOnly, bool checkWhitelistOnly = true)
		{
			Guard.IsNotNullOrWhiteSpace(path, "path");
			bool result;
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				try
				{
					path = this.GetFullPath(path);
					if (path.StartsWith(this.ConfigData.WorkshopModsDirectory) || path.StartsWith(this.ConfigData.LocalModsDirectory) || path.StartsWith(this.ConfigData.TempDownloadsDirectory))
					{
						result = true;
					}
					else if (!this._fileListRead.ContainsKey(path))
					{
						result = false;
					}
					else if (!readOnly && !this._fileListWrite.ContainsKey(path))
					{
						result = false;
					}
					else if (checkWhitelistOnly)
					{
						result = true;
					}
					else
					{
						using (FileStream fs = File.Open(path, FileMode.Open, readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite))
						{
							result = (readOnly ? fs.CanRead : fs.CanWrite);
						}
					}
				}
				catch
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600536A RID: 21354 RVA: 0x002CB32C File Offset: 0x002C952C
		public void AddFileToWhitelist(string path, bool readOnly = true)
		{
			Guard.IsNotNullOrWhiteSpace(path, "path");
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				try
				{
					path = this.GetFullPath(path);
					this._fileListRead.AddOrUpdate(path, (string s) => 0, (string s, byte b) => 0);
					if (!readOnly)
					{
						this._fileListWrite.AddOrUpdate(path, (string s) => 0, (string s, byte b) => 0);
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600536B RID: 21355 RVA: 0x002CB444 File Offset: 0x002C9644
		public void AddFilesToWhitelist(ImmutableArray<string> paths, bool readOnly = true)
		{
			if (paths.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentNullException("paths");
			}
			foreach (string path in paths)
			{
				this.AddFileToWhitelist(path, readOnly);
			}
		}

		// Token: 0x0600536C RID: 21356 RVA: 0x002CB488 File Offset: 0x002C9688
		public void RemoveFileFromAllWhitelists(string path)
		{
			Guard.IsNotNullOrWhiteSpace(path, "path");
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				try
				{
					path = this.GetFullPath(path);
					byte b;
					this._fileListRead.TryRemove(path, out b);
					this._fileListWrite.TryRemove(path, out b);
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600536D RID: 21357 RVA: 0x002CB524 File Offset: 0x002C9724
		public Result SetReadOnlyWhitelist(ImmutableArray<string> filePaths)
		{
			Result result;
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (filePaths.IsDefaultOrEmpty)
				{
					result = Result.Fail("SetReadOnlyWhitelist: FilePaths cannot be empty.");
				}
				else
				{
					this._fileListRead.Clear();
					Result res = new Result();
					foreach (string path in filePaths)
					{
						Guard.IsNotNullOrWhiteSpace(path, "path");
						try
						{
							string p = Path.GetFullPath(path.CleanUpPathCrossPlatform(true, ""));
							if (this._fileListRead.ContainsKey(p))
							{
								res = res.WithReason(new Success("Path already in whitelist: " + p));
							}
							else if (this._fileListRead.TryAdd(p, 0))
							{
								res = res.WithSuccess("Added path successfully: " + p);
							}
							else
							{
								res = res.WithError(new Error("Failed to add path to list: " + p));
							}
						}
						catch (Exception e)
						{
							res = res.WithError(new ExceptionalError(e).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.ExceptionDetails, e.Message).WithMetadata(MetadataType.RootObject, path));
						}
					}
					result = res;
				}
			}
			return result;
		}

		// Token: 0x0600536E RID: 21358 RVA: 0x002CB6B8 File Offset: 0x002C98B8
		public Result SetReadWriteWhitelist(ImmutableArray<string> filePaths)
		{
			if (filePaths.IsDefaultOrEmpty)
			{
				return Result.Fail("SetReadOnlyWhitelist: FilePaths cannot be empty.");
			}
			Result res;
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fileListRead.Clear();
				this._fileListWrite.Clear();
				SafeStorageService.<>c__DisplayClass10_0 CS$<>8__locals1;
				CS$<>8__locals1.res = new Result();
				foreach (string path in filePaths)
				{
					Guard.IsNotNullOrWhiteSpace(path, "path");
					try
					{
						string p = Path.GetFullPath(path.CleanUpPathCrossPlatform(true, ""));
						SafeStorageService.<SetReadWriteWhitelist>g__TryAddToList|10_0(this._fileListRead, p, ref CS$<>8__locals1);
						SafeStorageService.<SetReadWriteWhitelist>g__TryAddToList|10_0(this._fileListWrite, p, ref CS$<>8__locals1);
						CS$<>8__locals1.res = CS$<>8__locals1.res.WithError(new Error("Failed to add path to list: " + p));
					}
					catch (Exception e)
					{
						CS$<>8__locals1.res = CS$<>8__locals1.res.WithError(new ExceptionalError(e).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.ExceptionDetails, e.Message).WithMetadata(MetadataType.RootObject, path));
					}
				}
				res = CS$<>8__locals1.res;
			}
			return res;
		}

		// Token: 0x0600536F RID: 21359 RVA: 0x002CB82C File Offset: 0x002C9A2C
		public void ClearAllWhitelists()
		{
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fileListRead.Clear();
				this._fileListWrite.Clear();
			}
		}

		// Token: 0x06005370 RID: 21360 RVA: 0x002CB89C File Offset: 0x002C9A9C
		Result IStorageService.SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return base.SaveLocalBinary(package, localFilePath, bytes);
		}

		// Token: 0x06005371 RID: 21361 RVA: 0x002CB8A7 File Offset: 0x002C9AA7
		Result IStorageService.SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return base.SaveLocalText(package, localFilePath, text);
		}

		// Token: 0x06005374 RID: 21364 RVA: 0x002CB8C8 File Offset: 0x002C9AC8
		[CompilerGenerated]
		internal static void <SetReadWriteWhitelist>g__TryAddToList|10_0(ConcurrentDictionary<string, byte> dict, string p, ref SafeStorageService.<>c__DisplayClass10_0 A_2)
		{
			if (dict.ContainsKey(p))
			{
				A_2.res = A_2.res.WithReason(new Success("Path already in whitelist: " + p));
				return;
			}
			if (dict.TryAdd(p, 0))
			{
				A_2.res = A_2.res.WithSuccess("Added path successfully: " + p);
				return;
			}
		}

		// Token: 0x04002C20 RID: 11296
		private ConcurrentDictionary<string, byte> _fileListRead = new ConcurrentDictionary<string, byte>();

		// Token: 0x04002C21 RID: 11297
		private ConcurrentDictionary<string, byte> _fileListWrite = new ConcurrentDictionary<string, byte>();

		// Token: 0x04002C22 RID: 11298
		private readonly AsyncReaderWriterLock _higherOperationsLock = new AsyncReaderWriterLock();
	}
}
