using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.LuaCs.Data;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E8 RID: 1256
	public class UIStylesService : IUIStylesService, IReusableService, IService, IDisposable
	{
		// Token: 0x06005203 RID: 20995 RVA: 0x002C0CE8 File Offset: 0x002BEEE8
		public void Dispose()
		{
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					foreach (IUIStylesCollection collection in this._stylesCollections.Values.SelectMany((ImmutableArray<IUIStylesCollection> c) => c))
					{
						try
						{
							collection.Dispose();
						}
						catch
						{
						}
					}
					this._stylesCollections.Clear();
					this._storageService.Dispose();
					this._stylesCollectionFactory.Dispose();
					this._storageService = null;
					this._stylesCollectionFactory = null;
				}
			}
		}

		// Token: 0x170014D6 RID: 5334
		// (get) Token: 0x06005204 RID: 20996 RVA: 0x002C0DFC File Offset: 0x002BEFFC
		// (set) Token: 0x06005205 RID: 20997 RVA: 0x002C0E09 File Offset: 0x002BF009
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

		// Token: 0x06005206 RID: 20998 RVA: 0x002C0E18 File Offset: 0x002BF018
		public Result Reset()
		{
			Result result2;
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result result = Result.Ok();
				foreach (IUIStylesCollection collection in this._stylesCollections.Values.SelectMany((ImmutableArray<IUIStylesCollection> c) => c))
				{
					try
					{
						collection.Dispose();
					}
					catch (Exception e)
					{
						result.WithError(new ExceptionalError(e));
					}
				}
				this._stylesCollections.Clear();
				result2 = result;
			}
			return result2;
		}

		// Token: 0x06005207 RID: 20999 RVA: 0x002C0F14 File Offset: 0x002BF114
		public UIStylesService(IUIStylesCollection.IFactory stylesCollectionFactory, IStorageService storageService)
		{
			this._stylesCollectionFactory = stylesCollectionFactory;
			this._storageService = storageService;
		}

		// Token: 0x06005208 RID: 21000 RVA: 0x002C0F40 File Offset: 0x002BF140
		public Result<GUIColor> GetColor(ContentPackage package, string internalName, string assetName)
		{
			Result<GUIColor> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guard.IsNotNull<ContentPackage>(package, "package");
				Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
				Guard.IsNotNullOrWhiteSpace(assetName, "assetName");
				ImmutableArray<IUIStylesCollection> collection;
				if (!this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out collection) || collection.IsDefaultOrEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
					defaultInterpolatedStringHandler.AppendFormatted("UIStylesService");
					defaultInterpolatedStringHandler.AppendLiteral(": No styles loaded for [ContentPackage].[InternalName] of: [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral("].[");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result failedResult = new Result();
					foreach (IUIStylesCollection stylesCollection in collection)
					{
						Result<GUIColor> res = stylesCollection.GetColor(assetName);
						if (res.IsSuccess)
						{
							return res;
						}
						failedResult.WithErrors(res.Errors);
					}
					result = failedResult;
				}
			}
			return result;
		}

		// Token: 0x06005209 RID: 21001 RVA: 0x002C1094 File Offset: 0x002BF294
		public Result<GUICursor> GetCursor(ContentPackage package, string internalName, string assetName)
		{
			Result<GUICursor> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guard.IsNotNull<ContentPackage>(package, "package");
				Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
				Guard.IsNotNullOrWhiteSpace(assetName, "assetName");
				ImmutableArray<IUIStylesCollection> collection;
				if (!this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out collection) || collection.IsDefaultOrEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
					defaultInterpolatedStringHandler.AppendFormatted("UIStylesService");
					defaultInterpolatedStringHandler.AppendLiteral(": No styles loaded for [ContentPackage].[InternalName] of: [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral("].[");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result failedResult = new Result();
					foreach (IUIStylesCollection stylesCollection in collection)
					{
						Result<GUICursor> res = stylesCollection.GetCursor(assetName);
						if (res.IsSuccess)
						{
							return res;
						}
						failedResult.WithErrors(res.Errors);
					}
					result = failedResult;
				}
			}
			return result;
		}

		// Token: 0x0600520A RID: 21002 RVA: 0x002C11E8 File Offset: 0x002BF3E8
		public Result<GUIFont> GetFont(ContentPackage package, string internalName, string assetName)
		{
			Result<GUIFont> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guard.IsNotNull<ContentPackage>(package, "package");
				Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
				Guard.IsNotNullOrWhiteSpace(assetName, "assetName");
				ImmutableArray<IUIStylesCollection> collection;
				if (!this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out collection) || collection.IsDefaultOrEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
					defaultInterpolatedStringHandler.AppendFormatted("UIStylesService");
					defaultInterpolatedStringHandler.AppendLiteral(": No styles loaded for [ContentPackage].[InternalName] of: [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral("].[");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result failedResult = new Result();
					foreach (IUIStylesCollection stylesCollection in collection)
					{
						Result<GUIFont> res = stylesCollection.GetFont(assetName);
						if (res.IsSuccess)
						{
							return res;
						}
						failedResult.WithErrors(res.Errors);
					}
					result = failedResult;
				}
			}
			return result;
		}

		// Token: 0x0600520B RID: 21003 RVA: 0x002C133C File Offset: 0x002BF53C
		public Result<GUISprite> GetSprite(ContentPackage package, string internalName, string assetName)
		{
			Result<GUISprite> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guard.IsNotNull<ContentPackage>(package, "package");
				Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
				Guard.IsNotNullOrWhiteSpace(assetName, "assetName");
				ImmutableArray<IUIStylesCollection> collection;
				if (!this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out collection) || collection.IsDefaultOrEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
					defaultInterpolatedStringHandler.AppendFormatted("UIStylesService");
					defaultInterpolatedStringHandler.AppendLiteral(": No styles loaded for [ContentPackage].[InternalName] of: [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral("].[");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result failedResult = new Result();
					foreach (IUIStylesCollection stylesCollection in collection)
					{
						Result<GUISprite> res = stylesCollection.GetSprite(assetName);
						if (res.IsSuccess)
						{
							return res;
						}
						failedResult.WithErrors(res.Errors);
					}
					result = failedResult;
				}
			}
			return result;
		}

		// Token: 0x0600520C RID: 21004 RVA: 0x002C1490 File Offset: 0x002BF690
		public Result<GUISpriteSheet> GetSpriteSheet(ContentPackage package, string internalName, string assetName)
		{
			Result<GUISpriteSheet> result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Guard.IsNotNull<ContentPackage>(package, "package");
				Guard.IsNotNullOrWhiteSpace(internalName, "internalName");
				Guard.IsNotNullOrWhiteSpace(assetName, "assetName");
				ImmutableArray<IUIStylesCollection> collection;
				if (!this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(package, internalName), out collection) || collection.IsDefaultOrEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 3);
					defaultInterpolatedStringHandler.AppendFormatted("UIStylesService");
					defaultInterpolatedStringHandler.AppendLiteral(": No styles loaded for [ContentPackage].[InternalName] of: [");
					defaultInterpolatedStringHandler.AppendFormatted(package.Name);
					defaultInterpolatedStringHandler.AppendLiteral("].[");
					defaultInterpolatedStringHandler.AppendFormatted(internalName);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					result = Result.Fail(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					Result failedResult = new Result();
					foreach (IUIStylesCollection stylesCollection in collection)
					{
						Result<GUISpriteSheet> res = stylesCollection.GetSpriteSheet(assetName);
						if (res.IsSuccess)
						{
							return res;
						}
						failedResult.WithErrors(res.Errors);
					}
					result = failedResult;
				}
			}
			return result;
		}

		// Token: 0x0600520D RID: 21005 RVA: 0x002C15E4 File Offset: 0x002BF7E4
		public Result LoadAssets(ImmutableArray<IStylesResourceInfo> resources)
		{
			Result result;
			using (this._lock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (resources.IsDefaultOrEmpty)
				{
					ThrowHelper.ThrowArgumentNullException("resources");
				}
				Result operationSuccess = Result.Ok();
				foreach (IStylesResourceInfo resource in resources)
				{
					ImmutableArray<IUIStylesCollection>.Builder builder = ImmutableArray.CreateBuilder<IUIStylesCollection>();
					ImmutableArray<IUIStylesCollection> collection;
					if (this._stylesCollections.TryGetValue(new ValueTuple<ContentPackage, string>(resource.OwnerPackage, resource.InternalName), out collection))
					{
						builder.AddRange(collection);
					}
					try
					{
						ImmutableArray<IUIStylesCollection> newCollections = this._stylesCollectionFactory.CreateInstance(resource, this._storageService).ToImmutableArray<IUIStylesCollection>();
						foreach (IUIStylesCollection stylesCollection in newCollections)
						{
							stylesCollection.LoadFile();
						}
						builder.AddRange(newCollections);
					}
					catch (Exception e)
					{
						operationSuccess.WithError(new ExceptionalError(e));
						continue;
					}
					this._stylesCollections[new ValueTuple<ContentPackage, string>(resource.OwnerPackage, resource.InternalName)] = builder.ToImmutable();
				}
				result = operationSuccess;
			}
			return result;
		}

		// Token: 0x0600520E RID: 21006 RVA: 0x002C1740 File Offset: 0x002BF940
		public Result UnloadPackages(ImmutableArray<ContentPackage> packages)
		{
			Result result2;
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ImmutableArray<ValueTuple<ContentPackage, string>> toRemove = (from c in this._stylesCollections
				select c.Key into c
				where packages.Contains(c.Item1)
				select c).ToImmutableArray<ValueTuple<ContentPackage, string>>();
				Result result = Result.Ok();
				foreach (ValueTuple<ContentPackage, string> key in toRemove)
				{
					ImmutableArray<IUIStylesCollection> collection;
					if (this._stylesCollections.TryRemove(key, out collection) && !collection.IsDefaultOrEmpty)
					{
						foreach (IUIStylesCollection stylesCollection in collection)
						{
							try
							{
								stylesCollection.UnloadFile();
							}
							catch (Exception e)
							{
								result.WithError(new ExceptionalError(e));
							}
						}
					}
				}
				result2 = result;
			}
			return result2;
		}

		// Token: 0x0600520F RID: 21007 RVA: 0x002C1874 File Offset: 0x002BFA74
		public Result UnloadPackage(ContentPackage package)
		{
			return this.UnloadPackages(new ContentPackage[]
			{
				package
			}.ToImmutableArray<ContentPackage>());
		}

		// Token: 0x06005210 RID: 21008 RVA: 0x002C1890 File Offset: 0x002BFA90
		public Result UnloadAllPackages()
		{
			Result result2;
			using (this._lock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				Result result = Result.Ok();
				foreach (ValueTuple<ContentPackage, string> key in this._stylesCollections.Keys.ToImmutableArray<ValueTuple<ContentPackage, string>>())
				{
					ImmutableArray<IUIStylesCollection> collection;
					if (this._stylesCollections.TryRemove(key, out collection) && !collection.IsDefaultOrEmpty)
					{
						foreach (IUIStylesCollection stylesCollection in collection)
						{
							try
							{
								stylesCollection.UnloadFile();
							}
							catch (Exception e)
							{
								result.WithError(new ExceptionalError(e));
							}
						}
					}
				}
				result2 = result;
			}
			return result2;
		}

		// Token: 0x04002B69 RID: 11113
		private int _isDisposed;

		// Token: 0x04002B6A RID: 11114
		private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();

		// Token: 0x04002B6B RID: 11115
		private IStorageService _storageService;

		// Token: 0x04002B6C RID: 11116
		private IUIStylesCollection.IFactory _stylesCollectionFactory;

		// Token: 0x04002B6D RID: 11117
		[TupleElementNames(new string[]
		{
			"Package",
			"InternalName"
		})]
		private ConcurrentDictionary<ValueTuple<ContentPackage, string>, ImmutableArray<IUIStylesCollection>> _stylesCollections = new ConcurrentDictionary<ValueTuple<ContentPackage, string>, ImmutableArray<IUIStylesCollection>>();
	}
}
