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
	// Token: 0x020003F3 RID: 1011
	public class SafeStorageService : StorageService, ISafeStorageService, IStorageService, IService, IDisposable, ISafeStorageValidation
	{
		// Token: 0x06003A43 RID: 14915 RVA: 0x00186A64 File Offset: 0x00184C64
		public SafeStorageService(IStorageServiceConfig configData) : base(configData)
		{
			base.IsReadOperationAllowedEval = ((string fp) => this.IsFileAccessible(fp, true, true));
			base.IsWriteOperationAllowedEval = ((string fp) => this.IsFileAccessible(fp, false, true));
		}

		// Token: 0x06003A44 RID: 14916 RVA: 0x00186ABD File Offset: 0x00184CBD
		private string GetFullPath(string path)
		{
			return Path.GetFullPath(path).CleanUpPathCrossPlatform(true, "");
		}

		// Token: 0x06003A45 RID: 14917 RVA: 0x00186AD0 File Offset: 0x00184CD0
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
					if (path.StartsWith(this.ConfigData.WorkshopModsDirectory) || path.StartsWith(this.ConfigData.LocalModsDirectory))
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

		// Token: 0x06003A46 RID: 14918 RVA: 0x00186BEC File Offset: 0x00184DEC
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

		// Token: 0x06003A47 RID: 14919 RVA: 0x00186D04 File Offset: 0x00184F04
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

		// Token: 0x06003A48 RID: 14920 RVA: 0x00186D48 File Offset: 0x00184F48
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

		// Token: 0x06003A49 RID: 14921 RVA: 0x00186DE4 File Offset: 0x00184FE4
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

		// Token: 0x06003A4A RID: 14922 RVA: 0x00186F78 File Offset: 0x00185178
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

		// Token: 0x06003A4B RID: 14923 RVA: 0x001870EC File Offset: 0x001852EC
		public void ClearAllWhitelists()
		{
			using (this._higherOperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fileListRead.Clear();
				this._fileListWrite.Clear();
			}
		}

		// Token: 0x06003A4C RID: 14924 RVA: 0x0018715C File Offset: 0x0018535C
		Result IStorageService.SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return base.SaveLocalBinary(package, localFilePath, bytes);
		}

		// Token: 0x06003A4D RID: 14925 RVA: 0x00187167 File Offset: 0x00185367
		Result IStorageService.SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return base.SaveLocalText(package, localFilePath, text);
		}

		// Token: 0x06003A50 RID: 14928 RVA: 0x00187188 File Offset: 0x00185388
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

		// Token: 0x04001D3C RID: 7484
		private ConcurrentDictionary<string, byte> _fileListRead = new ConcurrentDictionary<string, byte>();

		// Token: 0x04001D3D RID: 7485
		private ConcurrentDictionary<string, byte> _fileListWrite = new ConcurrentDictionary<string, byte>();

		// Token: 0x04001D3E RID: 7486
		private readonly AsyncReaderWriterLock _higherOperationsLock = new AsyncReaderWriterLock();
	}
}
