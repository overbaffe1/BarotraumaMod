using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using FluentResults;
using FluentResults.LuaCs;
using Microsoft.Toolkit.Diagnostics;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050B RID: 1291
	public class StorageService : IStorageService, IService, IDisposable
	{
		// Token: 0x0600538E RID: 21390 RVA: 0x002CC150 File Offset: 0x002CA350
		public StorageService(IStorageServiceConfig configData)
		{
			this.ConfigData = configData;
			this.IsReadOperationAllowedEval = ((string str) => true);
			this.IsWriteOperationAllowedEval = ((string str) => true);
		}

		// Token: 0x17001501 RID: 5377
		// (get) Token: 0x0600538F RID: 21391 RVA: 0x002CC1CA File Offset: 0x002CA3CA
		// (set) Token: 0x06005390 RID: 21392 RVA: 0x002CC1D2 File Offset: 0x002CA3D2
		protected Func<string, bool> IsReadOperationAllowedEval
		{
			get
			{
				return this._isReadOperationAllowedEval;
			}
			set
			{
				if (value != null)
				{
					this._isReadOperationAllowedEval = value;
				}
			}
		}

		// Token: 0x17001502 RID: 5378
		// (get) Token: 0x06005391 RID: 21393 RVA: 0x002CC1DE File Offset: 0x002CA3DE
		// (set) Token: 0x06005392 RID: 21394 RVA: 0x002CC1E6 File Offset: 0x002CA3E6
		protected Func<string, bool> IsWriteOperationAllowedEval
		{
			get
			{
				return this._isWriteOperationAllowedEval;
			}
			set
			{
				if (value != null)
				{
					this._isWriteOperationAllowedEval = value;
				}
			}
		}

		// Token: 0x17001503 RID: 5379
		// (get) Token: 0x06005393 RID: 21395 RVA: 0x002CC1F2 File Offset: 0x002CA3F2
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
		}

		// Token: 0x06005394 RID: 21396 RVA: 0x002CC200 File Offset: 0x002CA400
		public virtual void Dispose()
		{
			using (this.OperationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this._fsCache.Clear();
				}
			}
		}

		// Token: 0x06005395 RID: 21397 RVA: 0x002CC270 File Offset: 0x002CA470
		public void PurgeCache()
		{
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fsCache.Clear();
			}
		}

		// Token: 0x06005396 RID: 21398 RVA: 0x002CC2D8 File Offset: 0x002CA4D8
		public void PurgeFileFromCache(string absolutePath)
		{
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (!absolutePath.IsNullOrWhiteSpace())
				{
					try
					{
						absolutePath = Path.GetFullPath(absolutePath).CleanUpPath();
						OneOf<byte[], string, XDocument> oneOf;
						this._fsCache.Remove(absolutePath, out oneOf);
					}
					catch
					{
					}
				}
			}
		}

		// Token: 0x06005397 RID: 21399 RVA: 0x002CC368 File Offset: 0x002CA568
		public void PurgeFilesFromCache(params string[] absolutePaths)
		{
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (absolutePaths.Length >= 1)
				{
					foreach (string path in absolutePaths)
					{
						try
						{
							if (!path.IsNullOrWhiteSpace())
							{
								string path2 = Path.GetFullPath(path).CleanUpPath();
								OneOf<byte[], string, XDocument> oneOf;
								this._fsCache.Remove(path2, out oneOf);
							}
						}
						catch
						{
						}
					}
				}
			}
		}

		// Token: 0x06005398 RID: 21400 RVA: 0x002CC424 File Offset: 0x002CA624
		protected Result<string> GetAbsolutePathForLocal(ContentPackage package, string localFilePath)
		{
			if (Path.IsPathRooted(localFilePath))
			{
				ThrowHelper.ThrowArgumentException("GetAbsolutePathForLocal: The path " + localFilePath + " is an absolute path.");
			}
			Result<string> result;
			try
			{
				string path = Path.GetFullPath(Path.Combine(this.ConfigData.LocalPackageDataPath.Replace(this.ConfigData.LocalDataPathRegex, XmlConvert.EncodeLocalName(package.Name)).CleanUpPathCrossPlatform(true, ""), localFilePath.CleanUpPathCrossPlatform(true, "")));
				if (!path.StartsWith(Path.GetFullPath(this.ConfigData.LocalDataSavePath)))
				{
					ThrowHelper.ThrowUnauthorizedAccessException("GetAbsolutePathForLocal: The local path of '" + path + "' is not a local path!");
				}
				result = path;
			}
			catch (Exception e)
			{
				bool flag = e is ArgumentNullException || e is ArgumentException || e is UnauthorizedAccessException;
				if (flag)
				{
					throw;
				}
				result = Result.Fail(new ExceptionalError(e));
			}
			return result;
		}

		// Token: 0x06005399 RID: 21401 RVA: 0x002CC518 File Offset: 0x002CA718
		private Result<T> LoadLocalData<T>(ContentPackage package, string localFilePath, Func<string, Result<T>> dataLoader)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Guard.IsNotNullOrWhiteSpace(localFilePath, "localFilePath");
			Result<T> result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<string> res = this.GetAbsolutePathForLocal(package, localFilePath);
				result = ((res != null && res.IsFailed) ? res.ToResult() : dataLoader(res.Value));
			}
			return result;
		}

		// Token: 0x0600539A RID: 21402 RVA: 0x002CC5BC File Offset: 0x002CA7BC
		public Result<XDocument> LoadLocalXml(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<XDocument>(package, localFilePath, new Func<string, Result<XDocument>>(this.TryLoadXml));
		}

		// Token: 0x0600539B RID: 21403 RVA: 0x002CC5D2 File Offset: 0x002CA7D2
		public Result<byte[]> LoadLocalBinary(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<byte[]>(package, localFilePath, new Func<string, Result<byte[]>>(this.TryLoadBinary));
		}

		// Token: 0x0600539C RID: 21404 RVA: 0x002CC5E9 File Offset: 0x002CA7E9
		public Result<string> LoadLocalText(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<string>(package, localFilePath, new Func<string, Result<string>>(this.TryLoadText));
		}

		// Token: 0x0600539D RID: 21405 RVA: 0x002CC600 File Offset: 0x002CA800
		private Result SaveLocalData<T>(ContentPackage package, string localFilePath, in T data, Func<string, T, Result> dataSaver)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Guard.IsNotNullOrWhiteSpace(localFilePath, "localFilePath");
			Result result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<string> res = this.GetAbsolutePathForLocal(package, localFilePath);
				result = ((res != null && res.IsFailed) ? res.ToResult() : dataSaver(res.Value, data));
			}
			return result;
		}

		// Token: 0x0600539E RID: 21406 RVA: 0x002CC6A4 File Offset: 0x002CA8A4
		public Result SaveLocalXml(ContentPackage package, string localFilePath, XDocument document)
		{
			return this.SaveLocalData<XDocument>(package, localFilePath, document, (string path, XDocument data) => this.TrySaveXml(path, data, null));
		}

		// Token: 0x0600539F RID: 21407 RVA: 0x002CC6BC File Offset: 0x002CA8BC
		public Result SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return this.SaveLocalData<byte[]>(package, localFilePath, bytes, (string path, byte[] data) => this.TrySaveBinary(path, data));
		}

		// Token: 0x060053A0 RID: 21408 RVA: 0x002CC6D3 File Offset: 0x002CA8D3
		public Result SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return this.SaveLocalData<string>(package, localFilePath, text, (string path, string data) => this.TrySaveText(path, data, null));
		}

		// Token: 0x060053A1 RID: 21409 RVA: 0x002CC6EC File Offset: 0x002CA8EC
		private Task<Result<T>> LoadLocalDataAsync<T>(ContentPackage package, string localFilePath, Func<string, Task<Result<T>>> dataLoader)
		{
			StorageService.<LoadLocalDataAsync>d__28<T> <LoadLocalDataAsync>d__;
			<LoadLocalDataAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<T>>.Create();
			<LoadLocalDataAsync>d__.<>4__this = this;
			<LoadLocalDataAsync>d__.package = package;
			<LoadLocalDataAsync>d__.localFilePath = localFilePath;
			<LoadLocalDataAsync>d__.dataLoader = dataLoader;
			<LoadLocalDataAsync>d__.<>1__state = -1;
			<LoadLocalDataAsync>d__.<>t__builder.Start<StorageService.<LoadLocalDataAsync>d__28<T>>(ref <LoadLocalDataAsync>d__);
			return <LoadLocalDataAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A2 RID: 21410 RVA: 0x002CC748 File Offset: 0x002CA948
		public Task<Result<XDocument>> LoadLocalXmlAsync(ContentPackage package, string localFilePath)
		{
			StorageService.<LoadLocalXmlAsync>d__29 <LoadLocalXmlAsync>d__;
			<LoadLocalXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<XDocument>>.Create();
			<LoadLocalXmlAsync>d__.<>4__this = this;
			<LoadLocalXmlAsync>d__.package = package;
			<LoadLocalXmlAsync>d__.localFilePath = localFilePath;
			<LoadLocalXmlAsync>d__.<>1__state = -1;
			<LoadLocalXmlAsync>d__.<>t__builder.Start<StorageService.<LoadLocalXmlAsync>d__29>(ref <LoadLocalXmlAsync>d__);
			return <LoadLocalXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A3 RID: 21411 RVA: 0x002CC79C File Offset: 0x002CA99C
		public Task<Result<byte[]>> LoadLocalBinaryAsync(ContentPackage package, string localFilePath)
		{
			StorageService.<LoadLocalBinaryAsync>d__30 <LoadLocalBinaryAsync>d__;
			<LoadLocalBinaryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<byte[]>>.Create();
			<LoadLocalBinaryAsync>d__.<>4__this = this;
			<LoadLocalBinaryAsync>d__.package = package;
			<LoadLocalBinaryAsync>d__.localFilePath = localFilePath;
			<LoadLocalBinaryAsync>d__.<>1__state = -1;
			<LoadLocalBinaryAsync>d__.<>t__builder.Start<StorageService.<LoadLocalBinaryAsync>d__30>(ref <LoadLocalBinaryAsync>d__);
			return <LoadLocalBinaryAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A4 RID: 21412 RVA: 0x002CC7F0 File Offset: 0x002CA9F0
		public Task<Result<string>> LoadLocalTextAsync(ContentPackage package, string localFilePath)
		{
			StorageService.<LoadLocalTextAsync>d__31 <LoadLocalTextAsync>d__;
			<LoadLocalTextAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<string>>.Create();
			<LoadLocalTextAsync>d__.<>4__this = this;
			<LoadLocalTextAsync>d__.package = package;
			<LoadLocalTextAsync>d__.localFilePath = localFilePath;
			<LoadLocalTextAsync>d__.<>1__state = -1;
			<LoadLocalTextAsync>d__.<>t__builder.Start<StorageService.<LoadLocalTextAsync>d__31>(ref <LoadLocalTextAsync>d__);
			return <LoadLocalTextAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A5 RID: 21413 RVA: 0x002CC844 File Offset: 0x002CAA44
		private Task<Result> SaveLocalDataAsync<T>(ContentPackage package, string localFilePath, T data, Func<string, T, Task<Result>> dataSaver)
		{
			StorageService.<SaveLocalDataAsync>d__32<T> <SaveLocalDataAsync>d__;
			<SaveLocalDataAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<SaveLocalDataAsync>d__.<>4__this = this;
			<SaveLocalDataAsync>d__.package = package;
			<SaveLocalDataAsync>d__.localFilePath = localFilePath;
			<SaveLocalDataAsync>d__.data = data;
			<SaveLocalDataAsync>d__.dataSaver = dataSaver;
			<SaveLocalDataAsync>d__.<>1__state = -1;
			<SaveLocalDataAsync>d__.<>t__builder.Start<StorageService.<SaveLocalDataAsync>d__32<T>>(ref <SaveLocalDataAsync>d__);
			return <SaveLocalDataAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A6 RID: 21414 RVA: 0x002CC8A8 File Offset: 0x002CAAA8
		public Task<Result> SaveLocalXmlAsync(ContentPackage package, string localFilePath, XDocument document)
		{
			StorageService.<SaveLocalXmlAsync>d__33 <SaveLocalXmlAsync>d__;
			<SaveLocalXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<SaveLocalXmlAsync>d__.<>4__this = this;
			<SaveLocalXmlAsync>d__.package = package;
			<SaveLocalXmlAsync>d__.localFilePath = localFilePath;
			<SaveLocalXmlAsync>d__.document = document;
			<SaveLocalXmlAsync>d__.<>1__state = -1;
			<SaveLocalXmlAsync>d__.<>t__builder.Start<StorageService.<SaveLocalXmlAsync>d__33>(ref <SaveLocalXmlAsync>d__);
			return <SaveLocalXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A7 RID: 21415 RVA: 0x002CC904 File Offset: 0x002CAB04
		public Task<Result> SaveLocalBinaryAsync(ContentPackage package, string localFilePath, byte[] bytes)
		{
			StorageService.<SaveLocalBinaryAsync>d__34 <SaveLocalBinaryAsync>d__;
			<SaveLocalBinaryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<SaveLocalBinaryAsync>d__.<>4__this = this;
			<SaveLocalBinaryAsync>d__.package = package;
			<SaveLocalBinaryAsync>d__.localFilePath = localFilePath;
			<SaveLocalBinaryAsync>d__.bytes = bytes;
			<SaveLocalBinaryAsync>d__.<>1__state = -1;
			<SaveLocalBinaryAsync>d__.<>t__builder.Start<StorageService.<SaveLocalBinaryAsync>d__34>(ref <SaveLocalBinaryAsync>d__);
			return <SaveLocalBinaryAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A8 RID: 21416 RVA: 0x002CC960 File Offset: 0x002CAB60
		public Task<Result> SaveLocalTextAsync(ContentPackage package, string localFilePath, string text)
		{
			StorageService.<SaveLocalTextAsync>d__35 <SaveLocalTextAsync>d__;
			<SaveLocalTextAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<SaveLocalTextAsync>d__.<>4__this = this;
			<SaveLocalTextAsync>d__.package = package;
			<SaveLocalTextAsync>d__.localFilePath = localFilePath;
			<SaveLocalTextAsync>d__.text = text;
			<SaveLocalTextAsync>d__.<>1__state = -1;
			<SaveLocalTextAsync>d__.<>t__builder.Start<StorageService.<SaveLocalTextAsync>d__35>(ref <SaveLocalTextAsync>d__);
			return <SaveLocalTextAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053A9 RID: 21417 RVA: 0x002CC9BC File Offset: 0x002CABBC
		private bool IsPackagePathValid(ContentPath contentPath)
		{
			return contentPath.FullPath.StartsWith(this.ConfigData.WorkshopModsDirectory) || contentPath.FullPath.StartsWith(this.ConfigData.LocalModsDirectory) || contentPath.FullPath.StartsWith(this.ConfigData.TempDownloadsDirectory) || contentPath.FullPath.StartsWith(Path.GetFullPath(ContentPackageManager.VanillaCorePackage.Dir).CleanUpPathCrossPlatform(true, ""));
		}

		// Token: 0x060053AA RID: 21418 RVA: 0x002CCA38 File Offset: 0x002CAC38
		private Result<T> LoadPackageData<T>(ContentPath contentPath, Func<string, Result<T>> dataLoader)
		{
			Guard.IsNotNull<ContentPath>(contentPath, "contentPath");
			Guard.IsNotNullOrWhiteSpace(contentPath.FullPath, "FullPath");
			Result<T> result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (!this.IsPackagePathValid(contentPath))
				{
					ThrowHelper.ThrowUnauthorizedAccessException("LoadPackageData: The filepath of `" + contentPath.FullPath + "' is not in a package directory!");
				}
				result = dataLoader(contentPath.FullPath);
			}
			return result;
		}

		// Token: 0x060053AB RID: 21419 RVA: 0x002CCAE0 File Offset: 0x002CACE0
		public Result<XDocument> LoadPackageXml(ContentPath filePath)
		{
			return this.LoadPackageData<XDocument>(filePath, (string path) => this.TryLoadXml(filePath.FullPath));
		}

		// Token: 0x060053AC RID: 21420 RVA: 0x002CCB1C File Offset: 0x002CAD1C
		public Result<byte[]> LoadPackageBinary(ContentPath filePath)
		{
			return this.LoadPackageData<byte[]>(filePath, (string path) => this.TryLoadBinary(filePath.FullPath));
		}

		// Token: 0x060053AD RID: 21421 RVA: 0x002CCB58 File Offset: 0x002CAD58
		public Result<string> LoadPackageText(ContentPath filePath)
		{
			return this.LoadPackageData<string>(filePath, (string path) => this.TryLoadText(filePath.FullPath));
		}

		// Token: 0x060053AE RID: 21422 RVA: 0x002CCB94 File Offset: 0x002CAD94
		private ImmutableArray<ValueTuple<ContentPath, Result<T>>> LoadPackageDataFiles<T>(ImmutableArray<ContentPath> filePaths, Func<string, Result<T>> dataLoader)
		{
			if (filePaths.IsDefaultOrEmpty)
			{
				ThrowHelper.ThrowArgumentNullException("LoadPackageData: File paths is empty!");
			}
			ImmutableArray<ValueTuple<ContentPath, Result<T>>> result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ImmutableArray<ValueTuple<ContentPath, Result<T>>>.Builder builder = ImmutableArray.CreateBuilder<ValueTuple<ContentPath, Result<T>>>();
				foreach (ContentPath path in filePaths)
				{
					builder.Add(new ValueTuple<ContentPath, Result<T>>(path, this.LoadPackageData<T>(path, dataLoader)));
				}
				result = builder.ToImmutable();
			}
			return result;
		}

		// Token: 0x060053AF RID: 21423 RVA: 0x002CCC40 File Offset: 0x002CAE40
		public ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>> LoadPackageXmlFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<XDocument>(filePaths, new Func<string, Result<XDocument>>(this.TryLoadXml));
		}

		// Token: 0x060053B0 RID: 21424 RVA: 0x002CCC55 File Offset: 0x002CAE55
		public ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>> LoadPackageBinaryFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<byte[]>(filePaths, new Func<string, Result<byte[]>>(this.TryLoadBinary));
		}

		// Token: 0x060053B1 RID: 21425 RVA: 0x002CCC6B File Offset: 0x002CAE6B
		public ImmutableArray<ValueTuple<ContentPath, Result<string>>> LoadPackageTextFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<string>(filePaths, new Func<string, Result<string>>(this.TryLoadText));
		}

		// Token: 0x060053B2 RID: 21426 RVA: 0x002CCC80 File Offset: 0x002CAE80
		public Result<ImmutableArray<string>> FindFilesInPackage(ContentPackage package, string localSubfolder, string regexFilter, bool searchRecursively)
		{
			Guard.IsNotNull<ContentPackage>(package, "package");
			Result<ImmutableArray<string>> result;
			try
			{
				ContentPath cp = ContentPath.FromRaw(package, package.Dir);
				string fullPath = localSubfolder.IsNullOrWhiteSpace() ? Path.GetFullPath(cp.FullPath) : Path.GetFullPath(localSubfolder, cp.FullPath);
				result = Directory.GetFiles(fullPath, regexFilter, searchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToImmutableArray<string>();
			}
			catch (Exception e)
			{
				bool flag = e is ArgumentNullException || e is ArgumentException;
				if (flag)
				{
					throw;
				}
				result = Result.Fail(new ExceptionalError(e).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, package));
			}
			return result;
		}

		// Token: 0x060053B3 RID: 21427 RVA: 0x002CCD44 File Offset: 0x002CAF44
		private Task<Result<T>> LoadPackageDataAsync<T>(ContentPath contentPath, Func<string, Task<Result<T>>> dataLoader)
		{
			StorageService.<LoadPackageDataAsync>d__46<T> <LoadPackageDataAsync>d__;
			<LoadPackageDataAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<T>>.Create();
			<LoadPackageDataAsync>d__.<>4__this = this;
			<LoadPackageDataAsync>d__.contentPath = contentPath;
			<LoadPackageDataAsync>d__.dataLoader = dataLoader;
			<LoadPackageDataAsync>d__.<>1__state = -1;
			<LoadPackageDataAsync>d__.<>t__builder.Start<StorageService.<LoadPackageDataAsync>d__46<T>>(ref <LoadPackageDataAsync>d__);
			return <LoadPackageDataAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B4 RID: 21428 RVA: 0x002CCD98 File Offset: 0x002CAF98
		public Task<Result<XDocument>> LoadPackageXmlAsync(ContentPath filePath)
		{
			StorageService.<LoadPackageXmlAsync>d__47 <LoadPackageXmlAsync>d__;
			<LoadPackageXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<XDocument>>.Create();
			<LoadPackageXmlAsync>d__.<>4__this = this;
			<LoadPackageXmlAsync>d__.filePath = filePath;
			<LoadPackageXmlAsync>d__.<>1__state = -1;
			<LoadPackageXmlAsync>d__.<>t__builder.Start<StorageService.<LoadPackageXmlAsync>d__47>(ref <LoadPackageXmlAsync>d__);
			return <LoadPackageXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B5 RID: 21429 RVA: 0x002CCDE4 File Offset: 0x002CAFE4
		public Task<Result<byte[]>> LoadPackageBinaryAsync(ContentPath filePath)
		{
			StorageService.<LoadPackageBinaryAsync>d__48 <LoadPackageBinaryAsync>d__;
			<LoadPackageBinaryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<byte[]>>.Create();
			<LoadPackageBinaryAsync>d__.<>4__this = this;
			<LoadPackageBinaryAsync>d__.filePath = filePath;
			<LoadPackageBinaryAsync>d__.<>1__state = -1;
			<LoadPackageBinaryAsync>d__.<>t__builder.Start<StorageService.<LoadPackageBinaryAsync>d__48>(ref <LoadPackageBinaryAsync>d__);
			return <LoadPackageBinaryAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B6 RID: 21430 RVA: 0x002CCE30 File Offset: 0x002CB030
		public Task<Result<string>> LoadPackageTextAsync(ContentPath filePath)
		{
			StorageService.<LoadPackageTextAsync>d__49 <LoadPackageTextAsync>d__;
			<LoadPackageTextAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<string>>.Create();
			<LoadPackageTextAsync>d__.<>4__this = this;
			<LoadPackageTextAsync>d__.filePath = filePath;
			<LoadPackageTextAsync>d__.<>1__state = -1;
			<LoadPackageTextAsync>d__.<>t__builder.Start<StorageService.<LoadPackageTextAsync>d__49>(ref <LoadPackageTextAsync>d__);
			return <LoadPackageTextAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B7 RID: 21431 RVA: 0x002CCE7C File Offset: 0x002CB07C
		private Task<ImmutableArray<ValueTuple<ContentPath, Result<T>>>> LoadPackageDataFilesAsync<T>(ImmutableArray<ContentPath> filePaths, Func<string, Task<Result<T>>> dataLoader)
		{
			StorageService.<LoadPackageDataFilesAsync>d__50<T> <LoadPackageDataFilesAsync>d__;
			<LoadPackageDataFilesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<ValueTuple<ContentPath, Result<T>>>>.Create();
			<LoadPackageDataFilesAsync>d__.<>4__this = this;
			<LoadPackageDataFilesAsync>d__.filePaths = filePaths;
			<LoadPackageDataFilesAsync>d__.dataLoader = dataLoader;
			<LoadPackageDataFilesAsync>d__.<>1__state = -1;
			<LoadPackageDataFilesAsync>d__.<>t__builder.Start<StorageService.<LoadPackageDataFilesAsync>d__50<T>>(ref <LoadPackageDataFilesAsync>d__);
			return <LoadPackageDataFilesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B8 RID: 21432 RVA: 0x002CCED0 File Offset: 0x002CB0D0
		public Task<ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>>> LoadPackageXmlFilesAsync(ImmutableArray<ContentPath> filePaths)
		{
			StorageService.<LoadPackageXmlFilesAsync>d__51 <LoadPackageXmlFilesAsync>d__;
			<LoadPackageXmlFilesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>>>.Create();
			<LoadPackageXmlFilesAsync>d__.<>4__this = this;
			<LoadPackageXmlFilesAsync>d__.filePaths = filePaths;
			<LoadPackageXmlFilesAsync>d__.<>1__state = -1;
			<LoadPackageXmlFilesAsync>d__.<>t__builder.Start<StorageService.<LoadPackageXmlFilesAsync>d__51>(ref <LoadPackageXmlFilesAsync>d__);
			return <LoadPackageXmlFilesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053B9 RID: 21433 RVA: 0x002CCF1C File Offset: 0x002CB11C
		public Task<ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>>> LoadPackageBinaryFilesAsync(ImmutableArray<ContentPath> filePaths)
		{
			StorageService.<LoadPackageBinaryFilesAsync>d__52 <LoadPackageBinaryFilesAsync>d__;
			<LoadPackageBinaryFilesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>>>.Create();
			<LoadPackageBinaryFilesAsync>d__.<>4__this = this;
			<LoadPackageBinaryFilesAsync>d__.filePaths = filePaths;
			<LoadPackageBinaryFilesAsync>d__.<>1__state = -1;
			<LoadPackageBinaryFilesAsync>d__.<>t__builder.Start<StorageService.<LoadPackageBinaryFilesAsync>d__52>(ref <LoadPackageBinaryFilesAsync>d__);
			return <LoadPackageBinaryFilesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053BA RID: 21434 RVA: 0x002CCF68 File Offset: 0x002CB168
		public Task<ImmutableArray<ValueTuple<ContentPath, Result<string>>>> LoadPackageTextFilesAsync(ImmutableArray<ContentPath> filePaths)
		{
			StorageService.<LoadPackageTextFilesAsync>d__53 <LoadPackageTextFilesAsync>d__;
			<LoadPackageTextFilesAsync>d__.<>t__builder = AsyncTaskMethodBuilder<ImmutableArray<ValueTuple<ContentPath, Result<string>>>>.Create();
			<LoadPackageTextFilesAsync>d__.<>4__this = this;
			<LoadPackageTextFilesAsync>d__.filePaths = filePaths;
			<LoadPackageTextFilesAsync>d__.<>1__state = -1;
			<LoadPackageTextFilesAsync>d__.<>t__builder.Start<StorageService.<LoadPackageTextFilesAsync>d__53>(ref <LoadPackageTextFilesAsync>d__);
			return <LoadPackageTextFilesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x17001504 RID: 5380
		// (get) Token: 0x060053BB RID: 21435 RVA: 0x002CCFB3 File Offset: 0x002CB1B3
		// (set) Token: 0x060053BC RID: 21436 RVA: 0x002CCFC0 File Offset: 0x002CB1C0
		public bool UseCaching
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._useCaching);
			}
			set
			{
				ModUtils.Threading.SetBool(ref this._useCaching, value);
			}
		}

		// Token: 0x060053BD RID: 21437 RVA: 0x002CCFCE File Offset: 0x002CB1CE
		private Result<XDocument> TryLoadXml(string filePath)
		{
			return this.TryLoadXml(filePath, null);
		}

		// Token: 0x060053BE RID: 21438 RVA: 0x002CCFD8 File Offset: 0x002CB1D8
		public virtual Result<XDocument> TryLoadXml(string filePath, Encoding encoding)
		{
			Guard.IsNotNullOrWhiteSpace(filePath, "filePath");
			Result<XDocument> result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result<string> r = this.TryLoadText(filePath, encoding);
				if (r != null && r.IsSuccess && r.Value != null)
				{
					result = XDocument.Parse(r.Value);
				}
				else
				{
					result = r.ToResult<XDocument>((string s) => null).WithError(this.GetGeneralError("LoadLocalXml", filePath));
				}
			}
			return result;
		}

		// Token: 0x060053BF RID: 21439 RVA: 0x002CD0A8 File Offset: 0x002CB2A8
		private Result<string> TryLoadText(string filePath)
		{
			return this.TryLoadText(filePath, null);
		}

		// Token: 0x060053C0 RID: 21440 RVA: 0x002CD0B4 File Offset: 0x002CB2B4
		public virtual Result<string> TryLoadText(string filePath, Encoding encoding)
		{
			Guard.IsNotNullOrWhiteSpace(filePath, "filePath");
			Result<string> result2;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Func<string, bool> isReadOperationAllowedEval = this.IsReadOperationAllowedEval;
				OneOf<byte[], string, XDocument> result;
				string cachedVal;
				OneOf<byte[], XDocument> oneOf;
				if (!(((isReadOperationAllowedEval != null) ? new bool?(isReadOperationAllowedEval(filePath)) : null) ?? false))
				{
					result2 = Result.Fail("TryLoadText: File '" + filePath + "' is not allowed.");
				}
				else if (this.UseCaching && this._fsCache.TryGetValue(filePath, out result) && result.TryPickT1(out cachedVal, out oneOf))
				{
					result2 = Result.Ok<string>(cachedVal);
				}
				else
				{
					result2 = this.IOExceptionsOperationRunner<string>("TryLoadText", filePath, delegate()
					{
						string fp = filePath.CleanUpPath();
						fp = (Path.IsPathRooted(fp) ? fp : Path.GetFullPath(fp));
						string fileText = (encoding == null) ? File.ReadAllText(fp) : File.ReadAllText(fp, encoding);
						if (this.UseCaching)
						{
							this._fsCache[filePath] = fileText;
						}
						return new Result<string>().WithSuccess("Loaded file successfully").WithValue(fileText);
					});
				}
			}
			return result2;
		}

		// Token: 0x060053C1 RID: 21441 RVA: 0x002CD1F0 File Offset: 0x002CB3F0
		public virtual Result<byte[]> TryLoadBinary(string filePath)
		{
			Guard.IsNotNullOrWhiteSpace(filePath, "filePath");
			Result<byte[]> result2;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Func<string, bool> isReadOperationAllowedEval = this.IsReadOperationAllowedEval;
				OneOf<byte[], string, XDocument> result;
				byte[] cachedVal;
				OneOf<string, XDocument> oneOf;
				if (!(((isReadOperationAllowedEval != null) ? new bool?(isReadOperationAllowedEval(filePath)) : null) ?? false))
				{
					result2 = Result.Fail("TryLoadBinary: File '" + filePath + "' is not allowed.");
				}
				else if (this.UseCaching && this._fsCache.TryGetValue(filePath, out result) && result.TryPickT0(out cachedVal, out oneOf))
				{
					result2 = Result.Ok<byte[]>(cachedVal);
				}
				else
				{
					result2 = this.IOExceptionsOperationRunner<byte[]>("TryLoadBinary", filePath, delegate()
					{
						string fp = filePath.CleanUpPath();
						fp = (Path.IsPathRooted(fp) ? fp : Path.GetFullPath(fp));
						byte[] fileData = File.ReadAllBytes(fp);
						if (this.UseCaching)
						{
							this._fsCache[filePath] = fileData;
						}
						return new Result<byte[]>().WithSuccess("Loaded file successfully").WithValue(fileData);
					});
				}
			}
			return result2;
		}

		// Token: 0x060053C2 RID: 21442 RVA: 0x002CD324 File Offset: 0x002CB524
		public virtual Result TrySaveXml(string filePath, in XDocument document, Encoding encoding = null)
		{
			string text = document.ToString();
			return this.TrySaveText(filePath, text, encoding);
		}

		// Token: 0x060053C3 RID: 21443 RVA: 0x002CD344 File Offset: 0x002CB544
		public virtual Result TrySaveText(string filePath, in string text, Encoding encoding = null)
		{
			Guard.IsNotNullOrWhiteSpace(text, "text");
			Result result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Func<string, bool> isWriteOperationAllowedEval = this.IsWriteOperationAllowedEval;
				if (!(((isWriteOperationAllowedEval != null) ? new bool?(isWriteOperationAllowedEval(filePath)) : null) ?? false))
				{
					result = Result.Fail("TrySaveText: File '" + filePath + "' is not allowed.");
				}
				else
				{
					string t = text;
					result = this.IOExceptionsOperationRunner("TrySaveText", filePath, delegate()
					{
						string fp = filePath.CleanUpPath();
						fp = (Path.IsPathRooted(fp) ? fp : Path.GetFullPath(fp));
						Directory.CreateDirectory(Path.GetDirectoryName(fp));
						File.WriteAllText(fp, t, encoding ?? Encoding.UTF8);
						if (this.UseCaching)
						{
							this._fsCache[filePath] = t;
						}
						return new Result().WithSuccess("Saved to file successfully");
					});
				}
			}
			return result;
		}

		// Token: 0x060053C4 RID: 21444 RVA: 0x002CD448 File Offset: 0x002CB648
		public virtual Result TrySaveBinary(string filePath, in byte[] bytes)
		{
			Guard.IsNotNullOrWhiteSpace(filePath, "filePath");
			Guard.IsNotNull<byte[]>(bytes, "bytes");
			Guard.HasSizeGreaterThanOrEqualTo<byte>(bytes, 1, "bytes");
			Result result;
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Func<string, bool> isWriteOperationAllowedEval = this.IsWriteOperationAllowedEval;
				if (!(((isWriteOperationAllowedEval != null) ? new bool?(isWriteOperationAllowedEval(filePath)) : null) ?? false))
				{
					result = Result.Fail("TrySaveBinary: File '" + filePath + "' is not allowed.");
				}
				else
				{
					byte[] b = new byte[bytes.Length];
					Buffer.BlockCopy(bytes, 0, b, 0, bytes.Length);
					result = this.IOExceptionsOperationRunner("TrySaveBinary", filePath, delegate()
					{
						string fp = filePath.CleanUpPath();
						fp = (Path.IsPathRooted(fp) ? fp : Path.GetFullPath(fp));
						Directory.CreateDirectory(Path.GetDirectoryName(fp));
						File.WriteAllBytes(fp, b);
						if (this.UseCaching)
						{
							this._fsCache[filePath] = b;
						}
						return new Result().WithSuccess("Saved to file successfully");
					});
				}
			}
			return result;
		}

		// Token: 0x060053C5 RID: 21445 RVA: 0x002CD57C File Offset: 0x002CB77C
		public virtual Result<bool> FileExists(string filePath)
		{
			Guard.IsNotNullOrWhiteSpace(filePath, "filePath");
			IService.CheckDisposed(this);
			Func<string, bool> isReadOperationAllowedEval = this.IsReadOperationAllowedEval;
			if (!(((isReadOperationAllowedEval != null) ? new bool?(isReadOperationAllowedEval(filePath)) : null) ?? false))
			{
				return Result.Fail("FileExists: File '" + filePath + "' is not allowed.");
			}
			return this.IOExceptionsOperationRunner<bool>("FileExists", filePath, delegate()
			{
				string fp = filePath.CleanUpPath();
				fp = (Path.IsPathRooted(fp) ? fp : Path.GetFullPath(fp));
				return File.Exists(fp);
			});
		}

		// Token: 0x060053C6 RID: 21446 RVA: 0x002CD624 File Offset: 0x002CB824
		public virtual Result<bool> DirectoryExists(string directoryPath)
		{
			Guard.IsNotNullOrWhiteSpace(directoryPath, "directoryPath");
			IService.CheckDisposed(this);
			Func<string, bool> isReadOperationAllowedEval = this.IsReadOperationAllowedEval;
			if (!(((isReadOperationAllowedEval != null) ? new bool?(isReadOperationAllowedEval(directoryPath)) : null) ?? false))
			{
				return Result.Fail("DirectoryExists: File '" + directoryPath + "' is not allowed.");
			}
			Result<bool> result;
			try
			{
				DirectoryInfo di = new DirectoryInfo(directoryPath);
				result = di.Exists;
			}
			catch (Exception ex)
			{
				result = new Result<bool>().WithError(ex.Message);
			}
			return result;
		}

		// Token: 0x060053C7 RID: 21447 RVA: 0x002CD6D0 File Offset: 0x002CB8D0
		public virtual Task<Result<XDocument>> TryLoadXmlAsync(string filePath, Encoding encoding = null)
		{
			StorageService.<TryLoadXmlAsync>d__68 <TryLoadXmlAsync>d__;
			<TryLoadXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<XDocument>>.Create();
			<TryLoadXmlAsync>d__.<>4__this = this;
			<TryLoadXmlAsync>d__.filePath = filePath;
			<TryLoadXmlAsync>d__.<>1__state = -1;
			<TryLoadXmlAsync>d__.<>t__builder.Start<StorageService.<TryLoadXmlAsync>d__68>(ref <TryLoadXmlAsync>d__);
			return <TryLoadXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053C8 RID: 21448 RVA: 0x002CD71C File Offset: 0x002CB91C
		public virtual Task<Result<string>> TryLoadTextAsync(string filePath, Encoding encoding = null)
		{
			StorageService.<TryLoadTextAsync>d__69 <TryLoadTextAsync>d__;
			<TryLoadTextAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<string>>.Create();
			<TryLoadTextAsync>d__.<>4__this = this;
			<TryLoadTextAsync>d__.filePath = filePath;
			<TryLoadTextAsync>d__.<>1__state = -1;
			<TryLoadTextAsync>d__.<>t__builder.Start<StorageService.<TryLoadTextAsync>d__69>(ref <TryLoadTextAsync>d__);
			return <TryLoadTextAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053C9 RID: 21449 RVA: 0x002CD768 File Offset: 0x002CB968
		public virtual Task<Result<byte[]>> TryLoadBinaryAsync(string filePath)
		{
			StorageService.<TryLoadBinaryAsync>d__70 <TryLoadBinaryAsync>d__;
			<TryLoadBinaryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<byte[]>>.Create();
			<TryLoadBinaryAsync>d__.<>4__this = this;
			<TryLoadBinaryAsync>d__.filePath = filePath;
			<TryLoadBinaryAsync>d__.<>1__state = -1;
			<TryLoadBinaryAsync>d__.<>t__builder.Start<StorageService.<TryLoadBinaryAsync>d__70>(ref <TryLoadBinaryAsync>d__);
			return <TryLoadBinaryAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CA RID: 21450 RVA: 0x002CD7B4 File Offset: 0x002CB9B4
		public virtual Task<Result> TrySaveXmlAsync(string filePath, XDocument document, Encoding encoding = null)
		{
			StorageService.<TrySaveXmlAsync>d__71 <TrySaveXmlAsync>d__;
			<TrySaveXmlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<TrySaveXmlAsync>d__.<>4__this = this;
			<TrySaveXmlAsync>d__.filePath = filePath;
			<TrySaveXmlAsync>d__.document = document;
			<TrySaveXmlAsync>d__.encoding = encoding;
			<TrySaveXmlAsync>d__.<>1__state = -1;
			<TrySaveXmlAsync>d__.<>t__builder.Start<StorageService.<TrySaveXmlAsync>d__71>(ref <TrySaveXmlAsync>d__);
			return <TrySaveXmlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CB RID: 21451 RVA: 0x002CD810 File Offset: 0x002CBA10
		public virtual Task<Result> TrySaveTextAsync(string filePath, string text, Encoding encoding = null)
		{
			StorageService.<TrySaveTextAsync>d__72 <TrySaveTextAsync>d__;
			<TrySaveTextAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<TrySaveTextAsync>d__.<>4__this = this;
			<TrySaveTextAsync>d__.filePath = filePath;
			<TrySaveTextAsync>d__.text = text;
			<TrySaveTextAsync>d__.encoding = encoding;
			<TrySaveTextAsync>d__.<>1__state = -1;
			<TrySaveTextAsync>d__.<>t__builder.Start<StorageService.<TrySaveTextAsync>d__72>(ref <TrySaveTextAsync>d__);
			return <TrySaveTextAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CC RID: 21452 RVA: 0x002CD86C File Offset: 0x002CBA6C
		public virtual Task<Result> TrySaveBinaryAsync(string filePath, byte[] bytes)
		{
			StorageService.<TrySaveBinaryAsync>d__73 <TrySaveBinaryAsync>d__;
			<TrySaveBinaryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<TrySaveBinaryAsync>d__.<>4__this = this;
			<TrySaveBinaryAsync>d__.filePath = filePath;
			<TrySaveBinaryAsync>d__.bytes = bytes;
			<TrySaveBinaryAsync>d__.<>1__state = -1;
			<TrySaveBinaryAsync>d__.<>t__builder.Start<StorageService.<TrySaveBinaryAsync>d__73>(ref <TrySaveBinaryAsync>d__);
			return <TrySaveBinaryAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CD RID: 21453 RVA: 0x002CD8C0 File Offset: 0x002CBAC0
		private Task<Result<T>> IOExceptionsOperationRunnerAsync<T>(string funcName, string filepath, Func<Task<Result<T>>> operation)
		{
			StorageService.<IOExceptionsOperationRunnerAsync>d__74<T> <IOExceptionsOperationRunnerAsync>d__;
			<IOExceptionsOperationRunnerAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result<T>>.Create();
			<IOExceptionsOperationRunnerAsync>d__.<>4__this = this;
			<IOExceptionsOperationRunnerAsync>d__.funcName = funcName;
			<IOExceptionsOperationRunnerAsync>d__.filepath = filepath;
			<IOExceptionsOperationRunnerAsync>d__.operation = operation;
			<IOExceptionsOperationRunnerAsync>d__.<>1__state = -1;
			<IOExceptionsOperationRunnerAsync>d__.<>t__builder.Start<StorageService.<IOExceptionsOperationRunnerAsync>d__74<T>>(ref <IOExceptionsOperationRunnerAsync>d__);
			return <IOExceptionsOperationRunnerAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CE RID: 21454 RVA: 0x002CD91C File Offset: 0x002CBB1C
		private Task<Result> IOExceptionsOperationRunnerAsync(string funcName, string filepath, Func<Task<Result>> operation)
		{
			StorageService.<IOExceptionsOperationRunnerAsync>d__75 <IOExceptionsOperationRunnerAsync>d__;
			<IOExceptionsOperationRunnerAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Result>.Create();
			<IOExceptionsOperationRunnerAsync>d__.<>4__this = this;
			<IOExceptionsOperationRunnerAsync>d__.funcName = funcName;
			<IOExceptionsOperationRunnerAsync>d__.filepath = filepath;
			<IOExceptionsOperationRunnerAsync>d__.operation = operation;
			<IOExceptionsOperationRunnerAsync>d__.<>1__state = -1;
			<IOExceptionsOperationRunnerAsync>d__.<>t__builder.Start<StorageService.<IOExceptionsOperationRunnerAsync>d__75>(ref <IOExceptionsOperationRunnerAsync>d__);
			return <IOExceptionsOperationRunnerAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060053CF RID: 21455 RVA: 0x002CD978 File Offset: 0x002CBB78
		private Result<T> IOExceptionsOperationRunner<T>(string funcName, string filepath, Func<Result<T>> operation)
		{
			Result<T> result;
			try
			{
				result = ((operation != null) ? operation() : null);
			}
			catch (Exception e)
			{
				if (e is ArgumentException)
				{
					throw;
				}
				result = this.ReturnException<Exception>(e, filepath).WithError(this.GetGeneralError(funcName, filepath));
			}
			return result;
		}

		// Token: 0x060053D0 RID: 21456 RVA: 0x002CD9D0 File Offset: 0x002CBBD0
		private Result IOExceptionsOperationRunner(string funcName, string filepath, Func<Result> operation)
		{
			Result result;
			try
			{
				result = ((operation != null) ? operation() : null);
			}
			catch (Exception e)
			{
				if (e is ArgumentException)
				{
					throw;
				}
				result = this.ReturnException<Exception>(e, filepath).WithError(this.GetGeneralError(funcName, filepath));
			}
			return result;
		}

		// Token: 0x060053D1 RID: 21457 RVA: 0x002CDA20 File Offset: 0x002CBC20
		private Error GetGeneralError(string funcName, string localfp, ContentPackage package)
		{
			return new Error(funcName + ": Failed to load local file.").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.Sources, localfp).WithMetadata(MetadataType.RootObject, package);
		}

		// Token: 0x060053D2 RID: 21458 RVA: 0x002CDA53 File Offset: 0x002CBC53
		private Error GetGeneralError(string funcName, string localfp)
		{
			return new Error(funcName + ": Failed to load local file.").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.Sources, localfp);
		}

		// Token: 0x060053D3 RID: 21459 RVA: 0x002CDA7B File Offset: 0x002CBC7B
		private Result<TReturn> ReturnException<TReturn, TException>(TException exception, ContentPackage package) where TException : Exception
		{
			return new Result<TReturn>().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, package));
		}

		// Token: 0x060053D4 RID: 21460 RVA: 0x002CDAA8 File Offset: 0x002CBCA8
		private Result ReturnException<TException>(TException exception, ContentPackage package) where TException : Exception
		{
			return new Result().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, package));
		}

		// Token: 0x060053D5 RID: 21461 RVA: 0x002CDAD5 File Offset: 0x002CBCD5
		private Result ReturnException<TException>(TException exception, string filePath) where TException : Exception
		{
			return new Result().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, filePath));
		}

		// Token: 0x060053D6 RID: 21462 RVA: 0x002CDB02 File Offset: 0x002CBD02
		Result IStorageService.SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return this.SaveLocalBinary(package, localFilePath, bytes);
		}

		// Token: 0x060053D7 RID: 21463 RVA: 0x002CDB0D File Offset: 0x002CBD0D
		Result IStorageService.SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return this.SaveLocalText(package, localFilePath, text);
		}

		// Token: 0x04002C2B RID: 11307
		private readonly ConcurrentDictionary<string, OneOf<byte[], string, XDocument>> _fsCache = new ConcurrentDictionary<string, OneOf<byte[], string, XDocument>>();

		// Token: 0x04002C2C RID: 11308
		protected readonly IStorageServiceConfig ConfigData;

		// Token: 0x04002C2D RID: 11309
		protected readonly AsyncReaderWriterLock OperationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002C2E RID: 11310
		private Func<string, bool> _isReadOperationAllowedEval;

		// Token: 0x04002C2F RID: 11311
		private Func<string, bool> _isWriteOperationAllowedEval;

		// Token: 0x04002C30 RID: 11312
		private int _isDisposed;

		// Token: 0x04002C31 RID: 11313
		private int _useCaching;
	}
}
