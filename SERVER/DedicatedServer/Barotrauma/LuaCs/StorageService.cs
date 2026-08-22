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
	// Token: 0x020003F7 RID: 1015
	public class StorageService : IStorageService, IService, IDisposable
	{
		// Token: 0x06003A6A RID: 14954 RVA: 0x001879D0 File Offset: 0x00185BD0
		public StorageService(IStorageServiceConfig configData)
		{
			this.ConfigData = configData;
			this.IsReadOperationAllowedEval = ((string str) => true);
			this.IsWriteOperationAllowedEval = ((string str) => true);
		}

		// Token: 0x17000FBA RID: 4026
		// (get) Token: 0x06003A6B RID: 14955 RVA: 0x00187A4A File Offset: 0x00185C4A
		// (set) Token: 0x06003A6C RID: 14956 RVA: 0x00187A52 File Offset: 0x00185C52
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

		// Token: 0x17000FBB RID: 4027
		// (get) Token: 0x06003A6D RID: 14957 RVA: 0x00187A5E File Offset: 0x00185C5E
		// (set) Token: 0x06003A6E RID: 14958 RVA: 0x00187A66 File Offset: 0x00185C66
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

		// Token: 0x17000FBC RID: 4028
		// (get) Token: 0x06003A6F RID: 14959 RVA: 0x00187A72 File Offset: 0x00185C72
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
		}

		// Token: 0x06003A70 RID: 14960 RVA: 0x00187A80 File Offset: 0x00185C80
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

		// Token: 0x06003A71 RID: 14961 RVA: 0x00187AF0 File Offset: 0x00185CF0
		public void PurgeCache()
		{
			using (this.OperationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._fsCache.Clear();
			}
		}

		// Token: 0x06003A72 RID: 14962 RVA: 0x00187B58 File Offset: 0x00185D58
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

		// Token: 0x06003A73 RID: 14963 RVA: 0x00187BE8 File Offset: 0x00185DE8
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

		// Token: 0x06003A74 RID: 14964 RVA: 0x00187CA4 File Offset: 0x00185EA4
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

		// Token: 0x06003A75 RID: 14965 RVA: 0x00187D98 File Offset: 0x00185F98
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

		// Token: 0x06003A76 RID: 14966 RVA: 0x00187E3C File Offset: 0x0018603C
		public Result<XDocument> LoadLocalXml(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<XDocument>(package, localFilePath, new Func<string, Result<XDocument>>(this.TryLoadXml));
		}

		// Token: 0x06003A77 RID: 14967 RVA: 0x00187E52 File Offset: 0x00186052
		public Result<byte[]> LoadLocalBinary(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<byte[]>(package, localFilePath, new Func<string, Result<byte[]>>(this.TryLoadBinary));
		}

		// Token: 0x06003A78 RID: 14968 RVA: 0x00187E69 File Offset: 0x00186069
		public Result<string> LoadLocalText(ContentPackage package, string localFilePath)
		{
			return this.LoadLocalData<string>(package, localFilePath, new Func<string, Result<string>>(this.TryLoadText));
		}

		// Token: 0x06003A79 RID: 14969 RVA: 0x00187E80 File Offset: 0x00186080
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

		// Token: 0x06003A7A RID: 14970 RVA: 0x00187F24 File Offset: 0x00186124
		public Result SaveLocalXml(ContentPackage package, string localFilePath, XDocument document)
		{
			return this.SaveLocalData<XDocument>(package, localFilePath, document, (string path, XDocument data) => this.TrySaveXml(path, data, null));
		}

		// Token: 0x06003A7B RID: 14971 RVA: 0x00187F3C File Offset: 0x0018613C
		public Result SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return this.SaveLocalData<byte[]>(package, localFilePath, bytes, (string path, byte[] data) => this.TrySaveBinary(path, data));
		}

		// Token: 0x06003A7C RID: 14972 RVA: 0x00187F53 File Offset: 0x00186153
		public Result SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return this.SaveLocalData<string>(package, localFilePath, text, (string path, string data) => this.TrySaveText(path, data, null));
		}

		// Token: 0x06003A7D RID: 14973 RVA: 0x00187F6C File Offset: 0x0018616C
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

		// Token: 0x06003A7E RID: 14974 RVA: 0x00187FC8 File Offset: 0x001861C8
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

		// Token: 0x06003A7F RID: 14975 RVA: 0x0018801C File Offset: 0x0018621C
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

		// Token: 0x06003A80 RID: 14976 RVA: 0x00188070 File Offset: 0x00186270
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

		// Token: 0x06003A81 RID: 14977 RVA: 0x001880C4 File Offset: 0x001862C4
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

		// Token: 0x06003A82 RID: 14978 RVA: 0x00188128 File Offset: 0x00186328
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

		// Token: 0x06003A83 RID: 14979 RVA: 0x00188184 File Offset: 0x00186384
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

		// Token: 0x06003A84 RID: 14980 RVA: 0x001881E0 File Offset: 0x001863E0
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

		// Token: 0x06003A85 RID: 14981 RVA: 0x0018823C File Offset: 0x0018643C
		private bool IsPackagePathValid(ContentPath contentPath)
		{
			return contentPath.FullPath.StartsWith(this.ConfigData.WorkshopModsDirectory) || contentPath.FullPath.StartsWith(this.ConfigData.LocalModsDirectory) || contentPath.FullPath.StartsWith(Path.GetFullPath(ContentPackageManager.VanillaCorePackage.Dir).CleanUpPathCrossPlatform(true, ""));
		}

		// Token: 0x06003A86 RID: 14982 RVA: 0x001882A0 File Offset: 0x001864A0
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

		// Token: 0x06003A87 RID: 14983 RVA: 0x00188348 File Offset: 0x00186548
		public Result<XDocument> LoadPackageXml(ContentPath filePath)
		{
			return this.LoadPackageData<XDocument>(filePath, (string path) => this.TryLoadXml(filePath.FullPath));
		}

		// Token: 0x06003A88 RID: 14984 RVA: 0x00188384 File Offset: 0x00186584
		public Result<byte[]> LoadPackageBinary(ContentPath filePath)
		{
			return this.LoadPackageData<byte[]>(filePath, (string path) => this.TryLoadBinary(filePath.FullPath));
		}

		// Token: 0x06003A89 RID: 14985 RVA: 0x001883C0 File Offset: 0x001865C0
		public Result<string> LoadPackageText(ContentPath filePath)
		{
			return this.LoadPackageData<string>(filePath, (string path) => this.TryLoadText(filePath.FullPath));
		}

		// Token: 0x06003A8A RID: 14986 RVA: 0x001883FC File Offset: 0x001865FC
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

		// Token: 0x06003A8B RID: 14987 RVA: 0x001884A8 File Offset: 0x001866A8
		public ImmutableArray<ValueTuple<ContentPath, Result<XDocument>>> LoadPackageXmlFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<XDocument>(filePaths, new Func<string, Result<XDocument>>(this.TryLoadXml));
		}

		// Token: 0x06003A8C RID: 14988 RVA: 0x001884BD File Offset: 0x001866BD
		public ImmutableArray<ValueTuple<ContentPath, Result<byte[]>>> LoadPackageBinaryFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<byte[]>(filePaths, new Func<string, Result<byte[]>>(this.TryLoadBinary));
		}

		// Token: 0x06003A8D RID: 14989 RVA: 0x001884D3 File Offset: 0x001866D3
		public ImmutableArray<ValueTuple<ContentPath, Result<string>>> LoadPackageTextFiles(ImmutableArray<ContentPath> filePaths)
		{
			return this.LoadPackageDataFiles<string>(filePaths, new Func<string, Result<string>>(this.TryLoadText));
		}

		// Token: 0x06003A8E RID: 14990 RVA: 0x001884E8 File Offset: 0x001866E8
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

		// Token: 0x06003A8F RID: 14991 RVA: 0x001885AC File Offset: 0x001867AC
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

		// Token: 0x06003A90 RID: 14992 RVA: 0x00188600 File Offset: 0x00186800
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

		// Token: 0x06003A91 RID: 14993 RVA: 0x0018864C File Offset: 0x0018684C
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

		// Token: 0x06003A92 RID: 14994 RVA: 0x00188698 File Offset: 0x00186898
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

		// Token: 0x06003A93 RID: 14995 RVA: 0x001886E4 File Offset: 0x001868E4
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

		// Token: 0x06003A94 RID: 14996 RVA: 0x00188738 File Offset: 0x00186938
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

		// Token: 0x06003A95 RID: 14997 RVA: 0x00188784 File Offset: 0x00186984
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

		// Token: 0x06003A96 RID: 14998 RVA: 0x001887D0 File Offset: 0x001869D0
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

		// Token: 0x17000FBD RID: 4029
		// (get) Token: 0x06003A97 RID: 14999 RVA: 0x0018881B File Offset: 0x00186A1B
		// (set) Token: 0x06003A98 RID: 15000 RVA: 0x00188828 File Offset: 0x00186A28
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

		// Token: 0x06003A99 RID: 15001 RVA: 0x00188836 File Offset: 0x00186A36
		private Result<XDocument> TryLoadXml(string filePath)
		{
			return this.TryLoadXml(filePath, null);
		}

		// Token: 0x06003A9A RID: 15002 RVA: 0x00188840 File Offset: 0x00186A40
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

		// Token: 0x06003A9B RID: 15003 RVA: 0x00188910 File Offset: 0x00186B10
		private Result<string> TryLoadText(string filePath)
		{
			return this.TryLoadText(filePath, null);
		}

		// Token: 0x06003A9C RID: 15004 RVA: 0x0018891C File Offset: 0x00186B1C
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

		// Token: 0x06003A9D RID: 15005 RVA: 0x00188A58 File Offset: 0x00186C58
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

		// Token: 0x06003A9E RID: 15006 RVA: 0x00188B8C File Offset: 0x00186D8C
		public virtual Result TrySaveXml(string filePath, in XDocument document, Encoding encoding = null)
		{
			string text = document.ToString();
			return this.TrySaveText(filePath, text, encoding);
		}

		// Token: 0x06003A9F RID: 15007 RVA: 0x00188BAC File Offset: 0x00186DAC
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

		// Token: 0x06003AA0 RID: 15008 RVA: 0x00188CB0 File Offset: 0x00186EB0
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

		// Token: 0x06003AA1 RID: 15009 RVA: 0x00188DE4 File Offset: 0x00186FE4
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

		// Token: 0x06003AA2 RID: 15010 RVA: 0x00188E8C File Offset: 0x0018708C
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

		// Token: 0x06003AA3 RID: 15011 RVA: 0x00188F38 File Offset: 0x00187138
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

		// Token: 0x06003AA4 RID: 15012 RVA: 0x00188F84 File Offset: 0x00187184
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

		// Token: 0x06003AA5 RID: 15013 RVA: 0x00188FD0 File Offset: 0x001871D0
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

		// Token: 0x06003AA6 RID: 15014 RVA: 0x0018901C File Offset: 0x0018721C
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

		// Token: 0x06003AA7 RID: 15015 RVA: 0x00189078 File Offset: 0x00187278
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

		// Token: 0x06003AA8 RID: 15016 RVA: 0x001890D4 File Offset: 0x001872D4
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

		// Token: 0x06003AA9 RID: 15017 RVA: 0x00189128 File Offset: 0x00187328
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

		// Token: 0x06003AAA RID: 15018 RVA: 0x00189184 File Offset: 0x00187384
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

		// Token: 0x06003AAB RID: 15019 RVA: 0x001891E0 File Offset: 0x001873E0
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

		// Token: 0x06003AAC RID: 15020 RVA: 0x00189238 File Offset: 0x00187438
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

		// Token: 0x06003AAD RID: 15021 RVA: 0x00189288 File Offset: 0x00187488
		private Error GetGeneralError(string funcName, string localfp, ContentPackage package)
		{
			return new Error(funcName + ": Failed to load local file.").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.Sources, localfp).WithMetadata(MetadataType.RootObject, package);
		}

		// Token: 0x06003AAE RID: 15022 RVA: 0x001892BB File Offset: 0x001874BB
		private Error GetGeneralError(string funcName, string localfp)
		{
			return new Error(funcName + ": Failed to load local file.").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.Sources, localfp);
		}

		// Token: 0x06003AAF RID: 15023 RVA: 0x001892E3 File Offset: 0x001874E3
		private Result<TReturn> ReturnException<TReturn, TException>(TException exception, ContentPackage package) where TException : Exception
		{
			return new Result<TReturn>().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, package));
		}

		// Token: 0x06003AB0 RID: 15024 RVA: 0x00189310 File Offset: 0x00187510
		private Result ReturnException<TException>(TException exception, ContentPackage package) where TException : Exception
		{
			return new Result().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, package));
		}

		// Token: 0x06003AB1 RID: 15025 RVA: 0x0018933D File Offset: 0x0018753D
		private Result ReturnException<TException>(TException exception, string filePath) where TException : Exception
		{
			return new Result().WithError(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, filePath));
		}

		// Token: 0x06003AB2 RID: 15026 RVA: 0x0018936A File Offset: 0x0018756A
		Result IStorageService.SaveLocalBinary(ContentPackage package, string localFilePath, in byte[] bytes)
		{
			return this.SaveLocalBinary(package, localFilePath, bytes);
		}

		// Token: 0x06003AB3 RID: 15027 RVA: 0x00189375 File Offset: 0x00187575
		Result IStorageService.SaveLocalText(ContentPackage package, string localFilePath, in string text)
		{
			return this.SaveLocalText(package, localFilePath, text);
		}

		// Token: 0x04001D47 RID: 7495
		private readonly ConcurrentDictionary<string, OneOf<byte[], string, XDocument>> _fsCache = new ConcurrentDictionary<string, OneOf<byte[], string, XDocument>>();

		// Token: 0x04001D48 RID: 7496
		protected readonly IStorageServiceConfig ConfigData;

		// Token: 0x04001D49 RID: 7497
		protected readonly AsyncReaderWriterLock OperationsLock = new AsyncReaderWriterLock();

		// Token: 0x04001D4A RID: 7498
		private Func<string, bool> _isReadOperationAllowedEval;

		// Token: 0x04001D4B RID: 7499
		private Func<string, bool> _isWriteOperationAllowedEval;

		// Token: 0x04001D4C RID: 7500
		private int _isDisposed;

		// Token: 0x04001D4D RID: 7501
		private int _useCaching;
	}
}
