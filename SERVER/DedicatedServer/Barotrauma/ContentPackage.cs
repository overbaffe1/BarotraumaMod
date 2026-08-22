using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Steam;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x0200015A RID: 346
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentPackage
	{
		// Token: 0x17000839 RID: 2105
		// (get) Token: 0x06001C8C RID: 7308 RVA: 0x000CF3DD File Offset: 0x000CD5DD
		// (set) Token: 0x06001C8D RID: 7309 RVA: 0x000CF3E5 File Offset: 0x000CD5E5
		public string Name { get; private set; }

		// Token: 0x1700083A RID: 2106
		// (get) Token: 0x06001C8E RID: 7310 RVA: 0x000CF3EE File Offset: 0x000CD5EE
		// (set) Token: 0x06001C8F RID: 7311 RVA: 0x000CF3F6 File Offset: 0x000CD5F6
		public string Path { get; private set; }

		// Token: 0x1700083B RID: 2107
		// (get) Token: 0x06001C90 RID: 7312 RVA: 0x000CF3FF File Offset: 0x000CD5FF
		public string Dir
		{
			get
			{
				return Barotrauma.IO.Path.GetDirectoryName(this.Path) ?? "";
			}
		}

		// Token: 0x1700083C RID: 2108
		// (get) Token: 0x06001C91 RID: 7313 RVA: 0x000CF415 File Offset: 0x000CD615
		// (set) Token: 0x06001C92 RID: 7314 RVA: 0x000CF41D File Offset: 0x000CD61D
		public ContentPackage.UgcStatus UgcItemStatus { get; private set; }

		// Token: 0x1700083D RID: 2109
		// (get) Token: 0x06001C93 RID: 7315 RVA: 0x000CF426 File Offset: 0x000CD626
		// (set) Token: 0x06001C94 RID: 7316 RVA: 0x000CF42E File Offset: 0x000CD62E
		[Nullable(0)]
		public Option<Item> UgcItem { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x1700083E RID: 2110
		// (get) Token: 0x06001C95 RID: 7317 RVA: 0x000CF437 File Offset: 0x000CD637
		// (set) Token: 0x06001C96 RID: 7318 RVA: 0x000CF43F File Offset: 0x000CD63F
		public Md5Hash Hash { get; private set; }

		// Token: 0x1700083F RID: 2111
		// (get) Token: 0x06001C97 RID: 7319 RVA: 0x000CF448 File Offset: 0x000CD648
		// (set) Token: 0x06001C98 RID: 7320 RVA: 0x000CF450 File Offset: 0x000CD650
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<ContentFile> Files { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] private set; }

		// Token: 0x17000840 RID: 2112
		// (get) Token: 0x06001C99 RID: 7321 RVA: 0x000CF459 File Offset: 0x000CD659
		// (set) Token: 0x06001C9A RID: 7322 RVA: 0x000CF461 File Offset: 0x000CD661
		[Nullable(0)]
		public ImmutableArray<ContentPackage.LoadError> FatalLoadErrors { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000841 RID: 2113
		// (get) Token: 0x06001C9B RID: 7323 RVA: 0x000CF46A File Offset: 0x000CD66A
		// (set) Token: 0x06001C9C RID: 7324 RVA: 0x000CF472 File Offset: 0x000CD672
		[Nullable(0)]
		public Option<ContentPackageManager.LoadProgress.Error> EnableError { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000842 RID: 2114
		// (get) Token: 0x06001C9D RID: 7325 RVA: 0x000CF47B File Offset: 0x000CD67B
		public IEnumerable<PublishedFileId> MissingDependencies
		{
			get
			{
				return this.missingDependencies;
			}
		}

		// Token: 0x17000843 RID: 2115
		// (get) Token: 0x06001C9E RID: 7326 RVA: 0x000CF484 File Offset: 0x000CD684
		public bool HasAnyErrors
		{
			get
			{
				return this.FatalLoadErrors.Length > 0 || this.EnableError.IsSome() || this.missingDependencies.Any<PublishedFileId>();
			}
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x000CF4C0 File Offset: 0x000CD6C0
		public Task<bool> IsUpToDate()
		{
			ContentPackage.<IsUpToDate>d__51 <IsUpToDate>d__;
			<IsUpToDate>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<IsUpToDate>d__.<>4__this = this;
			<IsUpToDate>d__.<>1__state = -1;
			<IsUpToDate>d__.<>t__builder.Start<ContentPackage.<IsUpToDate>d__51>(ref <IsUpToDate>d__);
			return <IsUpToDate>d__.<>t__builder.Task;
		}

		// Token: 0x17000844 RID: 2116
		// (get) Token: 0x06001CA0 RID: 7328 RVA: 0x000CF503 File Offset: 0x000CD703
		public int Index
		{
			get
			{
				return ContentPackageManager.EnabledPackages.IndexOf(this);
			}
		}

		// Token: 0x17000845 RID: 2117
		// (get) Token: 0x06001CA1 RID: 7329 RVA: 0x000CF50B File Offset: 0x000CD70B
		public bool HasMultiplayerSyncedContent { get; }

		// Token: 0x06001CA2 RID: 7330 RVA: 0x000CF514 File Offset: 0x000CD714
		protected ContentPackage(XDocument doc, string path)
		{
			Option.UnspecifiedNone none = Option.None;
			this.EnableError = none;
			this.missingDependencies = new HashSet<PublishedFileId>();
			base..ctor();
			using (DebugConsole.ErrorCatcher errorCatcher = DebugConsole.ErrorCatcher.Create())
			{
				this.Path = path.CleanUpPathCrossPlatform(true, "");
				XElement root = doc.Root;
				if (root == null)
				{
					throw new NullReferenceException("XML document is invalid: root element is null.");
				}
				XElement rootElement = root;
				this.Name = rootElement.GetAttributeString("name", "").Trim();
				this.AltNames = (from n in rootElement.GetAttributeStringArray("altnames", Array.Empty<string>(), true, false)
				select n.Trim()).ToImmutableArray<string>();
				ulong steamWorkshopId = rootElement.GetAttributeUInt64("steamworkshopid", 0UL);
				if (this.Name.IsNullOrWhiteSpace() && this.AltNames.Any<string>())
				{
					this.Name = this.AltNames.First<string>();
				}
				this.UgcId = ((steamWorkshopId != 0UL) ? Option<ContentPackageId>.Some(new SteamWorkshopId(steamWorkshopId)) : Option<ContentPackageId>.None());
				this.GameVersion = rootElement.GetAttributeVersion("gameversion", GameMain.Version);
				this.ModVersion = rootElement.GetAttributeString("modversion", "1.0.0");
				this.InstallTime = rootElement.GetAttributeDateTime("installtime");
				Result<ContentFile, ContentPackage.LoadError>[] fileResults = (from e in rootElement.Elements()
				where !ContentFile.IsLegacyContentType(e, this, true)
				select ContentFile.CreateFromXElement(this, e)).ToArray<Result<ContentFile, ContentPackage.LoadError>>();
				this.Files = fileResults.Successes<ContentFile, ContentPackage.LoadError>().ToImmutableArray<ContentFile>();
				this.FatalLoadErrors = fileResults.Failures<ContentFile, ContentPackage.LoadError>().ToImmutableArray<ContentPackage.LoadError>();
				this.AssertCondition(!string.IsNullOrEmpty(this.Name), "Name is null or empty");
				this.HasMultiplayerSyncedContent = this.Files.Any((ContentFile f) => !f.NotSyncedInMultiplayer);
				this.Hash = this.CalculateHash(false, null, null);
				string expectedHash = rootElement.GetAttributeString("expectedhash", "");
				if (this.HashMismatches(expectedHash))
				{
					this.FatalLoadErrors = this.FatalLoadErrors.Add(new ContentPackage.LoadError("Hash calculation returned " + this.Hash.StringRepresentation + ", expected " + expectedHash, null));
				}
				this.FatalLoadErrors = this.FatalLoadErrors.Concat(from err in errorCatcher.Errors
				select new ContentPackage.LoadError(err.Text, null)).ToImmutableArray<ContentPackage.LoadError>();
			}
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x000CF7D0 File Offset: 0x000CD9D0
		public bool HashMismatches(string expectedHash)
		{
			return this.GameVersion >= ContentPackage.MinimumHashCompatibleVersion && !expectedHash.IsNullOrWhiteSpace() && !expectedHash.Equals(this.Hash.StringRepresentation, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001CA4 RID: 7332 RVA: 0x000CF804 File Offset: 0x000CDA04
		public IEnumerable<T> GetFiles<[Nullable(0)] T>() where T : ContentFile
		{
			return this.Files.OfType<T>();
		}

		// Token: 0x06001CA5 RID: 7333 RVA: 0x000CF820 File Offset: 0x000CDA20
		public IEnumerable<ContentFile> GetFiles(Type type)
		{
			if (type.IsSubclassOf(typeof(ContentFile)))
			{
				return from f in this.Files
				where f.GetType() == type || f.GetType().IsSubclassOf(type)
				select f;
			}
			throw new ArgumentException("Type must be subclass of ContentFile, got " + type.Name);
		}

		// Token: 0x06001CA6 RID: 7334 RVA: 0x000CF884 File Offset: 0x000CDA84
		public bool NameMatches(Identifier name)
		{
			return this.Name == name || this.AltNames.Any((string n) => n == name);
		}

		// Token: 0x06001CA7 RID: 7335 RVA: 0x000CF8CA File Offset: 0x000CDACA
		public bool NameMatches(string name)
		{
			return this.NameMatches(name.ToIdentifier());
		}

		// Token: 0x06001CA8 RID: 7336 RVA: 0x000CF8D8 File Offset: 0x000CDAD8
		public static Result<ContentPackage, Exception> TryLoad(string path)
		{
			ValueTuple<Func<ContentPackage, Result<ContentPackage, Exception>>, Func<Exception, Result<ContentPackage, Exception>>> factoryMethods = Result<ContentPackage, Exception>.GetFactoryMethods();
			Func<ContentPackage, Result<ContentPackage, Exception>> success = factoryMethods.Item1;
			Func<Exception, Result<ContentPackage, Exception>> failure = factoryMethods.Item2;
			XDocument doc = XMLExtensions.TryLoadXml(path);
			Result<ContentPackage, Exception> result;
			try
			{
				ContentPackage contentPackage = doc.Root.GetAttributeBool("corepackage", false) ? new CorePackage(doc, path) : new RegularPackage(doc, path);
				string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
				bool? flag;
				if (fileNameWithoutExtension == null)
				{
					flag = null;
				}
				else
				{
					Func<char, bool> predicate;
					if ((predicate = ContentPackage.<>O.<0>__IsUpper) == null)
					{
						predicate = (ContentPackage.<>O.<0>__IsUpper = new Func<char, bool>(char.IsUpper));
					}
					flag = new bool?(fileNameWithoutExtension.Any(predicate));
				}
				bool? flag2 = flag;
				if (flag2 != null && flag2.GetValueOrDefault())
				{
					DebugConsole.ThrowError("Invalid filename casing. Please rename \"filelist.xml\" so it is entirely lowercase.", null, contentPackage, false, false);
				}
				result = success(contentPackage);
			}
			catch (Exception e)
			{
				result = failure(e.GetInnermost());
			}
			return result;
		}

		// Token: 0x06001CA9 RID: 7337 RVA: 0x000CF9B0 File Offset: 0x000CDBB0
		[NullableContext(2)]
		[return: Nullable(1)]
		public Md5Hash CalculateHash(bool logging = false, string name = null, string modVersion = null)
		{
			Md5Hash result;
			using (IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
			{
				if (logging)
				{
					DebugConsole.NewMessage("****************************** Calculating content package hash " + this.Name, null, false);
				}
				foreach (ContentFile file in this.Files)
				{
					try
					{
						Md5Hash hash = file.Hash;
						if (logging)
						{
							string str = "   ";
							ContentPath path = file.Path;
							DebugConsole.NewMessage(str + ((path != null) ? path.ToString() : null) + ": " + hash.StringRepresentation, null, false);
						}
						incrementalHash.AppendData(hash.ByteRepresentation);
					}
					catch (Exception e)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(164, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error while calculating the MD5 hash of the content package \"");
						defaultInterpolatedStringHandler.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" (file path: ");
						defaultInterpolatedStringHandler.AppendFormatted(this.Path);
						defaultInterpolatedStringHandler.AppendLiteral("). The content package may be corrupted. You may want to delete or reinstall the package.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
						break;
					}
				}
				string selectedName = name ?? this.Name;
				if (!selectedName.IsNullOrEmpty())
				{
					incrementalHash.AppendData(Encoding.UTF8.GetBytes(selectedName));
				}
				incrementalHash.AppendData(Encoding.UTF8.GetBytes(modVersion ?? this.ModVersion));
				Md5Hash md5Hash = Md5Hash.BytesAsHash(incrementalHash.GetHashAndReset());
				if (logging)
				{
					DebugConsole.NewMessage("****************************** Package hash: " + md5Hash.StringRepresentation, null, false);
				}
				result = md5Hash;
			}
			return result;
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x000CFB7C File Offset: 0x000CDD7C
		protected void AssertCondition(bool condition, string errorMsg)
		{
			if (!condition)
			{
				this.FatalLoadErrors = this.FatalLoadErrors.Add(new ContentPackage.LoadError(errorMsg, null));
			}
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x000CFBA7 File Offset: 0x000CDDA7
		public void AddMissingDependency(PublishedFileId missingItemID)
		{
			this.missingDependencies.Add(missingItemID);
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x000CFBB6 File Offset: 0x000CDDB6
		public void ClearMissingDependencies()
		{
			this.missingDependencies.Clear();
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x000CFBC4 File Offset: 0x000CDDC4
		[NullableContext(0)]
		public void LoadFilesOfType<T>() where T : ContentFile
		{
			(from f in this.Files
			where f is T
			select f).ForEach(delegate(ContentFile f)
			{
				f.LoadFile();
			});
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x000CFC20 File Offset: 0x000CDE20
		[NullableContext(0)]
		public void UnloadFilesOfType<T>() where T : ContentFile
		{
			(from f in this.Files
			where f is T
			select f).ForEach(delegate(ContentFile f)
			{
				f.UnloadFile();
			});
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x000CFC7C File Offset: 0x000CDE7C
		public ContentPackage.LoadResult LoadContent()
		{
			foreach (ContentPackageManager.LoadProgress p in this.LoadContentEnumerable())
			{
				if (p.Result.IsFailure)
				{
					return ContentPackage.LoadResult.Failure;
				}
			}
			return ContentPackage.LoadResult.Success;
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x000CFCD8 File Offset: 0x000CDED8
		public IEnumerable<ContentPackageManager.LoadProgress> LoadContentEnumerable()
		{
			ContentPackage.<LoadContentEnumerable>d__72 <LoadContentEnumerable>d__ = new ContentPackage.<LoadContentEnumerable>d__72(-2);
			<LoadContentEnumerable>d__.<>4__this = this;
			return <LoadContentEnumerable>d__;
		}

		// Token: 0x06001CB1 RID: 7345 RVA: 0x000CFCE8 File Offset: 0x000CDEE8
		public void TryFetchUgcDescription([Nullable(new byte[]
		{
			1,
			2
		})] Action<string> onFinished)
		{
			this.TryFetchUgcItem(delegate(Item? item)
			{
				Action<string> onFinished2 = onFinished;
				if (onFinished2 == null)
				{
					return;
				}
				onFinished2(((item != null) ? item.GetValueOrDefault().Description : null) ?? string.Empty);
			});
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x000CFD14 File Offset: 0x000CDF14
		public void TryFetchUgcChildren([Nullable(new byte[]
		{
			1,
			2
		})] Action<PublishedFileId[]> onFinished)
		{
			this.TryFetchUgcItem(delegate(Item? item)
			{
				Action<PublishedFileId[]> onFinished2 = onFinished;
				if (onFinished2 == null)
				{
					return;
				}
				onFinished2(((item != null) ? item.GetValueOrDefault().Children : null) ?? Array.Empty<PublishedFileId>());
			});
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x000CFD40 File Offset: 0x000CDF40
		private void TryFetchUgcItem(Action<Item?> onFinished)
		{
			ContentPackage.UgcStatus ugcItemStatus = this.UgcItemStatus;
			if (ugcItemStatus == ContentPackage.UgcStatus.NotFetched)
			{
				this.TryFetchUgcItem(delegate()
				{
					Item cachedItem2;
					if (this.UgcItemStatus == ContentPackage.UgcStatus.Fetched && this.UgcItem.TryUnwrap(out cachedItem2))
					{
						Action<Item?> onFinished4 = onFinished;
						if (onFinished4 == null)
						{
							return;
						}
						onFinished4(new Item?(cachedItem2));
					}
				});
				return;
			}
			if (ugcItemStatus == ContentPackage.UgcStatus.Fetched)
			{
				Item cachedItem;
				if (this.UgcItem.TryUnwrap(out cachedItem))
				{
					Action<Item?> onFinished2 = onFinished;
					if (onFinished2 == null)
					{
						return;
					}
					onFinished2(new Item?(cachedItem));
					return;
				}
			}
			Action<Item?> onFinished3 = onFinished;
			if (onFinished3 == null)
			{
				return;
			}
			onFinished3(null);
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x000CFDC8 File Offset: 0x000CDFC8
		public void TryFetchUgcItem(Action onFinished)
		{
			if (this.UgcItemStatus != ContentPackage.UgcStatus.NotFetched)
			{
				Action onFinished2 = onFinished;
				if (onFinished2 != null)
				{
					onFinished2();
				}
			}
			ContentPackageId ugcId;
			if (this.UgcId.TryUnwrap(out ugcId))
			{
				SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
				if (workshopId != null)
				{
					this.UgcItemStatus = ContentPackage.UgcStatus.Fetching;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendLiteral("PrepareToShow");
					defaultInterpolatedStringHandler.AppendFormatted<Option<ContentPackageId>>(this.UgcId);
					defaultInterpolatedStringHandler.AppendLiteral("Info");
					TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), SteamManager.Workshop.GetItem(workshopId.Value), delegate(Task task)
					{
						Option<Item> itemOption;
						Item item;
						if (!task.TryGetResult(out itemOption) || !itemOption.TryUnwrap(out item))
						{
							this.UgcItemStatus = ContentPackage.UgcStatus.Unavailable;
							return;
						}
						this.UgcItem = Option<Item>.Some(item);
						this.UgcItemStatus = ContentPackage.UgcStatus.Fetched;
						Action onFinished3 = onFinished;
						if (onFinished3 == null)
						{
							return;
						}
						onFinished3();
					});
					return;
				}
			}
			this.UgcItemStatus = ContentPackage.UgcStatus.Unavailable;
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x000CFE7D File Offset: 0x000CE07D
		public void UnloadContent()
		{
			this.Files.ForEach(delegate(ContentFile f)
			{
				f.UnloadFile();
			});
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x000CFEB0 File Offset: 0x000CE0B0
		public void ReloadSubsAndItemAssemblies()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			List<ContentFile> newFileList = new List<ContentFile>();
			XElement root = doc.Root;
			if (root == null)
			{
				throw new NullReferenceException("XML document is invalid: root element is null.");
			}
			XElement rootElement = root;
			Result<ContentFile, ContentPackage.LoadError>[] fileResults = (from e in rootElement.Elements()
			where !ContentFile.IsLegacyContentType(e, this, true)
			select ContentFile.CreateFromXElement(this, e)).ToArray<Result<ContentFile, ContentPackage.LoadError>>();
			using (IEnumerator<ContentFile> enumerator = fileResults.Successes<ContentFile, ContentPackage.LoadError>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ContentFile file = enumerator.Current;
					bool flag = file is BaseSubFile || file is ItemAssemblyFile;
					if (flag)
					{
						newFileList.Add(file);
					}
					else
					{
						ContentFile existingFile = this.Files.FirstOrDefault((ContentFile f) => f.Path == file.Path);
						newFileList.Add(existingFile ?? file);
					}
				}
			}
			this.UnloadFilesOfType<BaseSubFile>();
			this.UnloadFilesOfType<ItemAssemblyFile>();
			this.Files = newFileList.ToImmutableArray<ContentFile>();
			this.Hash = this.CalculateHash(false, null, null);
			this.LoadFilesOfType<BaseSubFile>();
			this.LoadFilesOfType<ItemAssemblyFile>();
		}

		// Token: 0x06001CB7 RID: 7351 RVA: 0x000CFFF8 File Offset: 0x000CE1F8
		public static bool PathAllowedAsLocalModFile(string path)
		{
			for (;;)
			{
				string temp = Barotrauma.IO.Path.GetDirectoryName(path) ?? "";
				if (string.IsNullOrEmpty(temp))
				{
					break;
				}
				path = temp;
			}
			return path == "LocalMods";
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x000D0030 File Offset: 0x000CE230
		public void LogErrors()
		{
			if (!this.FatalLoadErrors.Any<ContentPackage.LoadError>())
			{
				return;
			}
			string str = "The following errors occurred while loading the content package \"";
			string name = this.Name;
			string str2 = "\". The package might not work correctly.\n";
			char separator = '\n';
			ImmutableArray<ContentPackage.LoadError> fatalLoadErrors = this.FatalLoadErrors;
			Func<ContentPackage.LoadError, string> selector;
			if ((selector = ContentPackage.<>O.<1>__errorToStr) == null)
			{
				selector = (ContentPackage.<>O.<1>__errorToStr = new Func<ContentPackage.LoadError, string>(ContentPackage.<LogErrors>g__errorToStr|80_0));
			}
			DebugConsole.AddWarning(str + name + str2 + string.Join<string>(separator, fatalLoadErrors.Select(selector)), this);
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x000D0094 File Offset: 0x000CE294
		public bool TryRenameLocal(string newName)
		{
			if (!ContentPackageManager.LocalPackages.Contains(this))
			{
				return false;
			}
			if (newName.IsNullOrWhiteSpace())
			{
				DebugConsole.ThrowError("New name is blank!", null, null, false, false);
				return false;
			}
			string newDir = Barotrauma.IO.Path.Combine(new string[]
			{
				Barotrauma.IO.Path.GetFullPath("LocalMods"),
				File.SanitizeName(newName)
			});
			if (ContentPackageManager.LocalPackages.Any((ContentPackage lp) => lp.NameMatches(newName)) || Directory.Exists(newDir))
			{
				DebugConsole.ThrowError("A local package with the name or directory \"" + newName + "\" already exists!", null, null, false, false);
				return false;
			}
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			doc.Root.SetAttributeValue("name", newName);
			using (XmlWriter writer = XmlWriter.Create(this.Path, new XmlWriterSettings
			{
				Indent = true
			}))
			{
				doc.WriteTo(writer);
				writer.Flush();
			}
			Directory.Move(this.Dir, newDir, false);
			return true;
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x000D01B8 File Offset: 0x000CE3B8
		public bool TryDeleteLocal()
		{
			return ContentPackageManager.LocalPackages.Contains(this) && Directory.TryDelete(this.Dir, true);
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x000D01D8 File Offset: 0x000CE3D8
		public bool TryCreateLocalFromWorkshop()
		{
			if (!ContentPackageManager.WorkshopPackages.Contains(this))
			{
				return false;
			}
			string newDir = Barotrauma.IO.Path.Combine(new string[]
			{
				Barotrauma.IO.Path.GetFullPath("LocalMods"),
				File.SanitizeName(this.Name)
			});
			if (ContentPackageManager.LocalPackages.Any((ContentPackage lp) => lp.NameMatches(this.Name)) || Directory.Exists(newDir))
			{
				DebugConsole.ThrowError("A local package with the name or directory \"" + this.Name + "\" already exists!", null, null, false, false);
				return false;
			}
			Directory.Copy(this.Dir, newDir, false);
			return true;
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x000D02CD File Offset: 0x000CE4CD
		[CompilerGenerated]
		internal static string <LogErrors>g__errorToStr|80_0(ContentPackage.LoadError error)
		{
			return error.ToString();
		}

		// Token: 0x04000D02 RID: 3330
		public static readonly Version MinimumHashCompatibleVersion = new Version(1, 1, 0, 0);

		// Token: 0x04000D03 RID: 3331
		public const string LocalModsDir = "LocalMods";

		// Token: 0x04000D04 RID: 3332
		public static readonly string WorkshopModsDir = Barotrauma.IO.Path.Combine(new string[]
		{
			SaveUtil.DefaultSaveFolder,
			"WorkshopMods",
			"Installed"
		});

		// Token: 0x04000D05 RID: 3333
		public const string FileListFileName = "filelist.xml";

		// Token: 0x04000D06 RID: 3334
		public const string DefaultModVersion = "1.0.0";

		// Token: 0x04000D08 RID: 3336
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<string> AltNames;

		// Token: 0x04000D0A RID: 3338
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<ContentPackageId> UgcId;

		// Token: 0x04000D0B RID: 3339
		public readonly Version GameVersion;

		// Token: 0x04000D0C RID: 3340
		public readonly string ModVersion;

		// Token: 0x04000D10 RID: 3344
		[Nullable(0)]
		public readonly Option<SerializableDateTime> InstallTime;

		// Token: 0x04000D14 RID: 3348
		private readonly HashSet<PublishedFileId> missingDependencies;

		// Token: 0x020008D7 RID: 2263
		[Nullable(0)]
		public readonly struct LoadError : IEquatable<ContentPackage.LoadError>
		{
			// Token: 0x060057D1 RID: 22481 RVA: 0x001F5590 File Offset: 0x001F3790
			public LoadError(string Message, [Nullable(2)] Exception Exception)
			{
				this.Message = Message;
				this.Exception = Exception;
			}

			// Token: 0x17001538 RID: 5432
			// (get) Token: 0x060057D2 RID: 22482 RVA: 0x001F55A0 File Offset: 0x001F37A0
			// (set) Token: 0x060057D3 RID: 22483 RVA: 0x001F55A8 File Offset: 0x001F37A8
			public string Message { get; set; }

			// Token: 0x17001539 RID: 5433
			// (get) Token: 0x060057D4 RID: 22484 RVA: 0x001F55B1 File Offset: 0x001F37B1
			// (set) Token: 0x060057D5 RID: 22485 RVA: 0x001F55B9 File Offset: 0x001F37B9
			[Nullable(2)]
			public Exception Exception { [NullableContext(2)] get; [NullableContext(2)] set; }

			// Token: 0x060057D6 RID: 22486 RVA: 0x001F55C4 File Offset: 0x001F37C4
			public override string ToString()
			{
				string message = this.Message;
				Exception exception = this.Exception;
				string str;
				if (exception != null)
				{
					string stackTrace = exception.StackTrace;
					str = "\n" + stackTrace.CleanupStackTrace();
				}
				else
				{
					str = string.Empty;
				}
				return message + str;
			}

			// Token: 0x060057D7 RID: 22487 RVA: 0x001F5606 File Offset: 0x001F3806
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Message = ");
				builder.Append(this.Message);
				builder.Append(", Exception = ");
				builder.Append(this.Exception);
				return true;
			}

			// Token: 0x060057D8 RID: 22488 RVA: 0x001F563B File Offset: 0x001F383B
			[CompilerGenerated]
			public static bool operator !=(ContentPackage.LoadError left, ContentPackage.LoadError right)
			{
				return !(left == right);
			}

			// Token: 0x060057D9 RID: 22489 RVA: 0x001F5647 File Offset: 0x001F3847
			[CompilerGenerated]
			public static bool operator ==(ContentPackage.LoadError left, ContentPackage.LoadError right)
			{
				return left.Equals(right);
			}

			// Token: 0x060057DA RID: 22490 RVA: 0x001F5651 File Offset: 0x001F3851
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField) * -1521134295 + EqualityComparer<Exception>.Default.GetHashCode(this.<Exception>k__BackingField);
			}

			// Token: 0x060057DB RID: 22491 RVA: 0x001F567A File Offset: 0x001F387A
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContentPackage.LoadError && this.Equals((ContentPackage.LoadError)obj);
			}

			// Token: 0x060057DC RID: 22492 RVA: 0x001F5692 File Offset: 0x001F3892
			[CompilerGenerated]
			public bool Equals(ContentPackage.LoadError other)
			{
				return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField) && EqualityComparer<Exception>.Default.Equals(this.<Exception>k__BackingField, other.<Exception>k__BackingField);
			}

			// Token: 0x060057DD RID: 22493 RVA: 0x001F56C4 File Offset: 0x001F38C4
			[CompilerGenerated]
			public void Deconstruct(out string Message, [Nullable(2)] out Exception Exception)
			{
				Message = this.Message;
				Exception = this.Exception;
			}
		}

		// Token: 0x020008D8 RID: 2264
		[NullableContext(0)]
		public enum UgcStatus
		{
			// Token: 0x04003166 RID: 12646
			NotFetched,
			// Token: 0x04003167 RID: 12647
			Fetching,
			// Token: 0x04003168 RID: 12648
			Fetched,
			// Token: 0x04003169 RID: 12649
			Unavailable
		}

		// Token: 0x020008D9 RID: 2265
		[NullableContext(0)]
		public enum LoadResult
		{
			// Token: 0x0400316B RID: 12651
			Success,
			// Token: 0x0400316C RID: 12652
			Failure
		}

		// Token: 0x020008DA RID: 2266
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400316D RID: 12653
			[Nullable(0)]
			public static Func<char, bool> <0>__IsUpper;

			// Token: 0x0400316E RID: 12654
			[Nullable(0)]
			public static Func<ContentPackage.LoadError, string> <1>__errorToStr;
		}
	}
}
