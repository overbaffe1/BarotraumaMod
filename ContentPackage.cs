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
	// Token: 0x02000250 RID: 592
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class ContentPackage
	{
		// Token: 0x17000E9C RID: 3740
		// (get) Token: 0x0600377D RID: 14205 RVA: 0x00215349 File Offset: 0x00213549
		// (set) Token: 0x0600377E RID: 14206 RVA: 0x00215351 File Offset: 0x00213551
		public string Name { get; private set; }

		// Token: 0x17000E9D RID: 3741
		// (get) Token: 0x0600377F RID: 14207 RVA: 0x0021535A File Offset: 0x0021355A
		// (set) Token: 0x06003780 RID: 14208 RVA: 0x00215362 File Offset: 0x00213562
		public string Path { get; private set; }

		// Token: 0x17000E9E RID: 3742
		// (get) Token: 0x06003781 RID: 14209 RVA: 0x0021536B File Offset: 0x0021356B
		public string Dir
		{
			get
			{
				return Barotrauma.IO.Path.GetDirectoryName(this.Path) ?? "";
			}
		}

		// Token: 0x17000E9F RID: 3743
		// (get) Token: 0x06003782 RID: 14210 RVA: 0x00215381 File Offset: 0x00213581
		// (set) Token: 0x06003783 RID: 14211 RVA: 0x00215389 File Offset: 0x00213589
		public ContentPackage.UgcStatus UgcItemStatus { get; private set; }

		// Token: 0x17000EA0 RID: 3744
		// (get) Token: 0x06003784 RID: 14212 RVA: 0x00215392 File Offset: 0x00213592
		// (set) Token: 0x06003785 RID: 14213 RVA: 0x0021539A File Offset: 0x0021359A
		[Nullable(0)]
		public Option<Item> UgcItem { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000EA1 RID: 3745
		// (get) Token: 0x06003786 RID: 14214 RVA: 0x002153A3 File Offset: 0x002135A3
		// (set) Token: 0x06003787 RID: 14215 RVA: 0x002153AB File Offset: 0x002135AB
		public Md5Hash Hash { get; private set; }

		// Token: 0x17000EA2 RID: 3746
		// (get) Token: 0x06003788 RID: 14216 RVA: 0x002153B4 File Offset: 0x002135B4
		// (set) Token: 0x06003789 RID: 14217 RVA: 0x002153BC File Offset: 0x002135BC
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

		// Token: 0x17000EA3 RID: 3747
		// (get) Token: 0x0600378A RID: 14218 RVA: 0x002153C5 File Offset: 0x002135C5
		// (set) Token: 0x0600378B RID: 14219 RVA: 0x002153CD File Offset: 0x002135CD
		[Nullable(0)]
		public ImmutableArray<ContentPackage.LoadError> FatalLoadErrors { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000EA4 RID: 3748
		// (get) Token: 0x0600378C RID: 14220 RVA: 0x002153D6 File Offset: 0x002135D6
		// (set) Token: 0x0600378D RID: 14221 RVA: 0x002153DE File Offset: 0x002135DE
		[Nullable(0)]
		public Option<ContentPackageManager.LoadProgress.Error> EnableError { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000EA5 RID: 3749
		// (get) Token: 0x0600378E RID: 14222 RVA: 0x002153E7 File Offset: 0x002135E7
		public IEnumerable<PublishedFileId> MissingDependencies
		{
			get
			{
				return this.missingDependencies;
			}
		}

		// Token: 0x17000EA6 RID: 3750
		// (get) Token: 0x0600378F RID: 14223 RVA: 0x002153F0 File Offset: 0x002135F0
		public bool HasAnyErrors
		{
			get
			{
				return this.FatalLoadErrors.Length > 0 || this.EnableError.IsSome() || this.missingDependencies.Any<PublishedFileId>();
			}
		}

		// Token: 0x06003790 RID: 14224 RVA: 0x0021542C File Offset: 0x0021362C
		public Task<bool> IsUpToDate()
		{
			ContentPackage.<IsUpToDate>d__51 <IsUpToDate>d__;
			<IsUpToDate>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<IsUpToDate>d__.<>4__this = this;
			<IsUpToDate>d__.<>1__state = -1;
			<IsUpToDate>d__.<>t__builder.Start<ContentPackage.<IsUpToDate>d__51>(ref <IsUpToDate>d__);
			return <IsUpToDate>d__.<>t__builder.Task;
		}

		// Token: 0x17000EA7 RID: 3751
		// (get) Token: 0x06003791 RID: 14225 RVA: 0x0021546F File Offset: 0x0021366F
		public int Index
		{
			get
			{
				return ContentPackageManager.EnabledPackages.IndexOf(this);
			}
		}

		// Token: 0x17000EA8 RID: 3752
		// (get) Token: 0x06003792 RID: 14226 RVA: 0x00215477 File Offset: 0x00213677
		public bool HasMultiplayerSyncedContent { get; }

		// Token: 0x06003793 RID: 14227 RVA: 0x00215480 File Offset: 0x00213680
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

		// Token: 0x06003794 RID: 14228 RVA: 0x0021573C File Offset: 0x0021393C
		public bool HashMismatches(string expectedHash)
		{
			return this.GameVersion >= ContentPackage.MinimumHashCompatibleVersion && !expectedHash.IsNullOrWhiteSpace() && !expectedHash.Equals(this.Hash.StringRepresentation, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06003795 RID: 14229 RVA: 0x00215770 File Offset: 0x00213970
		public IEnumerable<T> GetFiles<[Nullable(0)] T>() where T : ContentFile
		{
			return this.Files.OfType<T>();
		}

		// Token: 0x06003796 RID: 14230 RVA: 0x0021578C File Offset: 0x0021398C
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

		// Token: 0x06003797 RID: 14231 RVA: 0x002157F0 File Offset: 0x002139F0
		public bool NameMatches(Identifier name)
		{
			return this.Name == name || this.AltNames.Any((string n) => n == name);
		}

		// Token: 0x06003798 RID: 14232 RVA: 0x00215836 File Offset: 0x00213A36
		public bool NameMatches(string name)
		{
			return this.NameMatches(name.ToIdentifier());
		}

		// Token: 0x06003799 RID: 14233 RVA: 0x00215844 File Offset: 0x00213A44
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

		// Token: 0x0600379A RID: 14234 RVA: 0x0021591C File Offset: 0x00213B1C
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

		// Token: 0x0600379B RID: 14235 RVA: 0x00215AE8 File Offset: 0x00213CE8
		protected void AssertCondition(bool condition, string errorMsg)
		{
			if (!condition)
			{
				this.FatalLoadErrors = this.FatalLoadErrors.Add(new ContentPackage.LoadError(errorMsg, null));
			}
		}

		// Token: 0x0600379C RID: 14236 RVA: 0x00215B13 File Offset: 0x00213D13
		public void AddMissingDependency(PublishedFileId missingItemID)
		{
			this.missingDependencies.Add(missingItemID);
		}

		// Token: 0x0600379D RID: 14237 RVA: 0x00215B22 File Offset: 0x00213D22
		public void ClearMissingDependencies()
		{
			this.missingDependencies.Clear();
		}

		// Token: 0x0600379E RID: 14238 RVA: 0x00215B30 File Offset: 0x00213D30
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

		// Token: 0x0600379F RID: 14239 RVA: 0x00215B8C File Offset: 0x00213D8C
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

		// Token: 0x060037A0 RID: 14240 RVA: 0x00215BE8 File Offset: 0x00213DE8
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

		// Token: 0x060037A1 RID: 14241 RVA: 0x00215C44 File Offset: 0x00213E44
		public IEnumerable<ContentPackageManager.LoadProgress> LoadContentEnumerable()
		{
			ContentPackage.<LoadContentEnumerable>d__72 <LoadContentEnumerable>d__ = new ContentPackage.<LoadContentEnumerable>d__72(-2);
			<LoadContentEnumerable>d__.<>4__this = this;
			return <LoadContentEnumerable>d__;
		}

		// Token: 0x060037A2 RID: 14242 RVA: 0x00215C54 File Offset: 0x00213E54
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

		// Token: 0x060037A3 RID: 14243 RVA: 0x00215C80 File Offset: 0x00213E80
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

		// Token: 0x060037A4 RID: 14244 RVA: 0x00215CAC File Offset: 0x00213EAC
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

		// Token: 0x060037A5 RID: 14245 RVA: 0x00215D34 File Offset: 0x00213F34
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

		// Token: 0x060037A6 RID: 14246 RVA: 0x00215DE9 File Offset: 0x00213FE9
		public void UnloadContent()
		{
			this.Files.ForEach(delegate(ContentFile f)
			{
				f.UnloadFile();
			});
		}

		// Token: 0x060037A7 RID: 14247 RVA: 0x00215E1C File Offset: 0x0021401C
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

		// Token: 0x060037A8 RID: 14248 RVA: 0x00215F64 File Offset: 0x00214164
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

		// Token: 0x060037A9 RID: 14249 RVA: 0x00215F9C File Offset: 0x0021419C
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

		// Token: 0x060037AA RID: 14250 RVA: 0x00216000 File Offset: 0x00214200
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

		// Token: 0x060037AB RID: 14251 RVA: 0x00216124 File Offset: 0x00214324
		public bool TryDeleteLocal()
		{
			return ContentPackageManager.LocalPackages.Contains(this) && Directory.TryDelete(this.Dir, true);
		}

		// Token: 0x060037AC RID: 14252 RVA: 0x00216144 File Offset: 0x00214344
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

		// Token: 0x060037B2 RID: 14258 RVA: 0x00216239 File Offset: 0x00214439
		[CompilerGenerated]
		internal static string <LogErrors>g__errorToStr|80_0(ContentPackage.LoadError error)
		{
			return error.ToString();
		}

		// Token: 0x04001C0F RID: 7183
		public static readonly Version MinimumHashCompatibleVersion = new Version(1, 1, 0, 0);

		// Token: 0x04001C10 RID: 7184
		public const string LocalModsDir = "LocalMods";

		// Token: 0x04001C11 RID: 7185
		public static readonly string WorkshopModsDir = Barotrauma.IO.Path.Combine(new string[]
		{
			SaveUtil.DefaultSaveFolder,
			"WorkshopMods",
			"Installed"
		});

		// Token: 0x04001C12 RID: 7186
		public const string FileListFileName = "filelist.xml";

		// Token: 0x04001C13 RID: 7187
		public const string DefaultModVersion = "1.0.0";

		// Token: 0x04001C15 RID: 7189
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<string> AltNames;

		// Token: 0x04001C17 RID: 7191
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly Option<ContentPackageId> UgcId;

		// Token: 0x04001C18 RID: 7192
		public readonly Version GameVersion;

		// Token: 0x04001C19 RID: 7193
		public readonly string ModVersion;

		// Token: 0x04001C1D RID: 7197
		[Nullable(0)]
		public readonly Option<SerializableDateTime> InstallTime;

		// Token: 0x04001C21 RID: 7201
		private readonly HashSet<PublishedFileId> missingDependencies;

		// Token: 0x02000EF1 RID: 3825
		[Nullable(0)]
		public readonly struct LoadError : IEquatable<ContentPackage.LoadError>
		{
			// Token: 0x060087B4 RID: 34740 RVA: 0x003A4614 File Offset: 0x003A2814
			public LoadError(string Message, [Nullable(2)] Exception Exception)
			{
				this.Message = Message;
				this.Exception = Exception;
			}

			// Token: 0x17001C12 RID: 7186
			// (get) Token: 0x060087B5 RID: 34741 RVA: 0x003A4624 File Offset: 0x003A2824
			// (set) Token: 0x060087B6 RID: 34742 RVA: 0x003A462C File Offset: 0x003A282C
			public string Message { get; set; }

			// Token: 0x17001C13 RID: 7187
			// (get) Token: 0x060087B7 RID: 34743 RVA: 0x003A4635 File Offset: 0x003A2835
			// (set) Token: 0x060087B8 RID: 34744 RVA: 0x003A463D File Offset: 0x003A283D
			[Nullable(2)]
			public Exception Exception { [NullableContext(2)] get; [NullableContext(2)] set; }

			// Token: 0x060087B9 RID: 34745 RVA: 0x003A4648 File Offset: 0x003A2848
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

			// Token: 0x060087BA RID: 34746 RVA: 0x003A468A File Offset: 0x003A288A
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

			// Token: 0x060087BB RID: 34747 RVA: 0x003A46BF File Offset: 0x003A28BF
			[CompilerGenerated]
			public static bool operator !=(ContentPackage.LoadError left, ContentPackage.LoadError right)
			{
				return !(left == right);
			}

			// Token: 0x060087BC RID: 34748 RVA: 0x003A46CB File Offset: 0x003A28CB
			[CompilerGenerated]
			public static bool operator ==(ContentPackage.LoadError left, ContentPackage.LoadError right)
			{
				return left.Equals(right);
			}

			// Token: 0x060087BD RID: 34749 RVA: 0x003A46D5 File Offset: 0x003A28D5
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField) * -1521134295 + EqualityComparer<Exception>.Default.GetHashCode(this.<Exception>k__BackingField);
			}

			// Token: 0x060087BE RID: 34750 RVA: 0x003A46FE File Offset: 0x003A28FE
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContentPackage.LoadError && this.Equals((ContentPackage.LoadError)obj);
			}

			// Token: 0x060087BF RID: 34751 RVA: 0x003A4716 File Offset: 0x003A2916
			[CompilerGenerated]
			public bool Equals(ContentPackage.LoadError other)
			{
				return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField) && EqualityComparer<Exception>.Default.Equals(this.<Exception>k__BackingField, other.<Exception>k__BackingField);
			}

			// Token: 0x060087C0 RID: 34752 RVA: 0x003A4748 File Offset: 0x003A2948
			[CompilerGenerated]
			public void Deconstruct(out string Message, [Nullable(2)] out Exception Exception)
			{
				Message = this.Message;
				Exception = this.Exception;
			}
		}

		// Token: 0x02000EF2 RID: 3826
		[NullableContext(0)]
		public enum UgcStatus
		{
			// Token: 0x0400544B RID: 21579
			NotFetched,
			// Token: 0x0400544C RID: 21580
			Fetching,
			// Token: 0x0400544D RID: 21581
			Fetched,
			// Token: 0x0400544E RID: 21582
			Unavailable
		}

		// Token: 0x02000EF3 RID: 3827
		[NullableContext(0)]
		public enum LoadResult
		{
			// Token: 0x04005450 RID: 21584
			Success,
			// Token: 0x04005451 RID: 21585
			Failure
		}

		// Token: 0x02000EF4 RID: 3828
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005452 RID: 21586
			[Nullable(0)]
			public static Func<char, bool> <0>__IsUpper;

			// Token: 0x04005453 RID: 21587
			[Nullable(0)]
			public static Func<ContentPackage.LoadError, string> <1>__errorToStr;
		}
	}
}
