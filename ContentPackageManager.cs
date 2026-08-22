using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x02000040 RID: 64
	[NullableContext(1)]
	[Nullable(0)]
	public static class ContentPackageManager
	{
		// Token: 0x06000993 RID: 2451 RVA: 0x00056F9C File Offset: 0x0005519C
		private static Task<IEnumerable<Item>> EnqueueWorkshopUpdates()
		{
			ContentPackageManager.<EnqueueWorkshopUpdates>d__1 <EnqueueWorkshopUpdates>d__;
			<EnqueueWorkshopUpdates>d__.<>t__builder = AsyncTaskMethodBuilder<IEnumerable<Item>>.Create();
			<EnqueueWorkshopUpdates>d__.<>1__state = -1;
			<EnqueueWorkshopUpdates>d__.<>t__builder.Start<ContentPackageManager.<EnqueueWorkshopUpdates>d__1>(ref <EnqueueWorkshopUpdates>d__);
			return <EnqueueWorkshopUpdates>d__.<>t__builder.Task;
		}

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000994 RID: 2452 RVA: 0x00056FD7 File Offset: 0x000551D7
		public static bool ModsEnabled
		{
			get
			{
				if (GameMain.VanillaContent != null)
				{
					return ContentPackageManager.EnabledPackages.All.Any((ContentPackage p) => p.HasMultiplayerSyncedContent && p != GameMain.VanillaContent);
				}
				return true;
			}
		}

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000995 RID: 2453 RVA: 0x0005700B File Offset: 0x0005520B
		// (set) Token: 0x06000996 RID: 2454 RVA: 0x00057012 File Offset: 0x00055212
		[Nullable(2)]
		public static CorePackage VanillaCorePackage { [NullableContext(2)] get; [NullableContext(2)] private set; } = null;

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000997 RID: 2455 RVA: 0x0005701C File Offset: 0x0005521C
		public static IEnumerable<CorePackage> CorePackages
		{
			get
			{
				IEnumerable<CorePackage> self;
				if (ContentPackageManager.VanillaCorePackage != null)
				{
					self = ContentPackageManager.VanillaCorePackage.ToEnumerable<CorePackage>();
				}
				else
				{
					IEnumerable<CorePackage> enumerable = Enumerable.Empty<CorePackage>();
					self = enumerable;
				}
				return self.CollectionConcat(ContentPackageManager.LocalPackages.Core.CollectionConcat(ContentPackageManager.WorkshopPackages.Core));
			}
		}

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000998 RID: 2456 RVA: 0x00057061 File Offset: 0x00055261
		public static IEnumerable<RegularPackage> RegularPackages
		{
			get
			{
				return ContentPackageManager.LocalPackages.Regular.CollectionConcat(ContentPackageManager.WorkshopPackages.Regular);
			}
		}

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000999 RID: 2457 RVA: 0x0005707C File Offset: 0x0005527C
		public static IEnumerable<ContentPackage> AllPackages
		{
			get
			{
				return ContentPackageManager.VanillaCorePackage.ToEnumerable<CorePackage>().CollectionConcat(ContentPackageManager.LocalPackages).CollectionConcat(ContentPackageManager.WorkshopPackages).OfType<ContentPackage>();
			}
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x000570A1 File Offset: 0x000552A1
		public static void UpdateContentPackageList()
		{
			ContentPackageManager.LocalPackages.Refresh();
			ContentPackageManager.WorkshopPackages.Refresh();
			ContentPackageManager.EnabledPackages.DisableRemovedMods();
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x000570BC File Offset: 0x000552BC
		public static Result<ContentPackage, Exception> ReloadContentPackage(ContentPackage p)
		{
			Result<ContentPackage, Exception> result = ContentPackage.TryLoad(p.Path);
			ContentPackage newPackage;
			if (result.TryUnwrapSuccess(out newPackage))
			{
				CorePackage core = newPackage as CorePackage;
				if (core == null)
				{
					RegularPackage regular = newPackage as RegularPackage;
					if (regular != null)
					{
						int index = ContentPackageManager.EnabledPackages.Regular.IndexOf(p);
						if (index >= 0)
						{
							RegularPackage[] newRegular = ContentPackageManager.EnabledPackages.Regular.ToArray<RegularPackage>();
							newRegular[index] = regular;
							ContentPackageManager.EnabledPackages.SetRegular(newRegular);
						}
					}
				}
				else if (ContentPackageManager.EnabledPackages.Core == p)
				{
					ContentPackageManager.EnabledPackages.SetCore(core);
				}
				ContentPackageManager.LocalPackages.SwapPackage(p, newPackage);
				ContentPackageManager.WorkshopPackages.SwapPackage(p, newPackage);
			}
			ContentPackageManager.EnabledPackages.DisableRemovedMods();
			return result;
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x00057150 File Offset: 0x00055350
		public static void LoadVanillaFileList()
		{
			ContentPackageManager.VanillaCorePackage = new CorePackage(XDocument.Load("Content/ContentPackages/Vanilla.xml"), "Content/ContentPackages/Vanilla.xml");
			foreach (ContentPackage.LoadError error in ContentPackageManager.VanillaCorePackage.FatalLoadErrors)
			{
				DebugConsole.ThrowError(error.ToString(), null, null, false, false);
			}
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x000571B2 File Offset: 0x000553B2
		public static IEnumerable<ContentPackageManager.LoadProgress> Init()
		{
			return new ContentPackageManager.<Init>d__26(-2);
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x000571BC File Offset: 0x000553BC
		public static void CheckMissingDependencies()
		{
			using (IEnumerator<ContentPackage> enumerator = ContentPackageManager.EnabledPackages.All.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ContentPackage enabledPackage = enumerator.Current;
					enabledPackage.ClearMissingDependencies();
					enabledPackage.TryFetchUgcChildren(delegate(PublishedFileId[] children)
					{
						if (children == null)
						{
							return;
						}
						IEnumerable<PublishedFileId> missingChildren = from childUgcItemId in children
						where ContentPackageManager.EnabledPackages.All.None(delegate(ContentPackage package)
						{
							ContentPackageId ugcId;
							if (package.UgcId.TryUnwrap(out ugcId))
							{
								SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
								if (workshopId != null)
								{
									return workshopId.Value == childUgcItemId.Value;
								}
							}
							return false;
						})
						select childUgcItemId;
						foreach (PublishedFileId missingChild in missingChildren)
						{
							if (!(missingChild.ToString() == "2559634234") && !(missingChild.ToString() == "2795927223"))
							{
								enabledPackage.AddMissingDependency(missingChild);
							}
						}
					});
				}
			}
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x00057230 File Offset: 0x00055430
		public static void LogEnabledRegularPackageErrors()
		{
			foreach (RegularPackage p in ContentPackageManager.EnabledPackages.Regular)
			{
				p.LogErrors();
			}
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x000572CC File Offset: 0x000554CC
		[CompilerGenerated]
		[return: Nullable(2)]
		internal static T <Init>g__findPackage|26_1<T>(IEnumerable<T> packages, [Nullable(2)] XElement elem) where T : ContentPackage
		{
			if (elem == null)
			{
				return default(T);
			}
			string name = elem.GetAttributeString("name", "");
			string path = elem.GetAttributeStringUnrestricted("path", "").CleanUpPathCrossPlatform(false, "");
			T result;
			if ((result = packages.FirstOrDefault((T p) => p.Path.Equals(path, StringComparison.OrdinalIgnoreCase))) == null)
			{
				result = packages.FirstOrDefault((T p) => p.NameMatches(name));
			}
			return result;
		}

		// Token: 0x040004FF RID: 1279
		public const string CopyIndicatorFileName = ".copying";

		// Token: 0x04000500 RID: 1280
		public const string VanillaFileList = "Content/ContentPackages/Vanilla.xml";

		// Token: 0x04000501 RID: 1281
		public const string CorePackageElementName = "corepackage";

		// Token: 0x04000502 RID: 1282
		public const string RegularPackagesElementName = "regularpackages";

		// Token: 0x04000503 RID: 1283
		public const string RegularPackagesSubElementName = "package";

		// Token: 0x04000504 RID: 1284
		public static readonly ContentPackageManager.PackageSource LocalPackages = new ContentPackageManager.PackageSource("LocalMods", null, null);

		// Token: 0x04000505 RID: 1285
		public static readonly ContentPackageManager.PackageSource WorkshopPackages = new ContentPackageManager.PackageSource(ContentPackage.WorkshopModsDir, new Predicate<string>(SteamManager.Workshop.IsInstallingToPath), delegate(string fileListPath, Exception exception)
		{
			Directory.TryDelete(Path.GetDirectoryName(fileListPath), true);
		});

		// Token: 0x02000730 RID: 1840
		[Nullable(0)]
		public sealed class PackageSource : ICollection<ContentPackage>, IEnumerable<ContentPackage>, IEnumerable
		{
			// Token: 0x060067FA RID: 26618 RVA: 0x0034B678 File Offset: 0x00349878
			public string SaveRegularMod(ModProject modProject)
			{
				if (modProject.IsCore)
				{
					throw new ArgumentException("ModProject must not be a core package");
				}
				string fileListPath = Path.Combine(new string[]
				{
					this.directory,
					ToolBox.RemoveInvalidFileNameChars(modProject.Name),
					"filelist.xml"
				}).CleanUpPathCrossPlatform(false, "");
				modProject.Save(fileListPath, true);
				this.Refresh();
				ContentPackageManager.EnabledPackages.DisableRemovedMods();
				return fileListPath;
			}

			// Token: 0x060067FB RID: 26619 RVA: 0x0034B6E4 File Offset: 0x003498E4
			public RegularPackage GetRegularModByPath(string fileListPath)
			{
				return this.Regular.First((RegularPackage p) => p.Path == fileListPath);
			}

			// Token: 0x060067FC RID: 26620 RVA: 0x0034B718 File Offset: 0x00349918
			public RegularPackage SaveAndEnableRegularMod(ModProject modProject)
			{
				string fileListPath = this.SaveRegularMod(modProject);
				RegularPackage package = this.GetRegularModByPath(fileListPath);
				ContentPackageManager.EnabledPackages.EnableRegular(package);
				return package;
			}

			// Token: 0x060067FD RID: 26621 RVA: 0x0034B73C File Offset: 0x0034993C
			public PackageSource(string dir, [Nullable(new byte[]
			{
				2,
				1
			})] Predicate<string> skipPredicate, [Nullable(new byte[]
			{
				2,
				1,
				1
			})] Action<string, Exception> onLoadFail)
			{
				this.skipPredicate = skipPredicate;
				this.onLoadFail = onLoadFail;
				this.directory = dir;
				Directory.CreateDirectory(this.directory, false);
			}

			// Token: 0x060067FE RID: 26622 RVA: 0x0034B77C File Offset: 0x0034997C
			public void SwapPackage(ContentPackage oldPackage, ContentPackage newPackage)
			{
				bool contains = false;
				CorePackage oldCore = oldPackage as CorePackage;
				if (oldCore != null && this.corePackages.Contains(oldCore))
				{
					this.corePackages.Remove(oldCore);
					contains = true;
				}
				else
				{
					RegularPackage oldRegular = oldPackage as RegularPackage;
					if (oldRegular != null && this.regularPackages.Contains(oldRegular))
					{
						this.regularPackages.Remove(oldRegular);
						contains = true;
					}
				}
				if (contains)
				{
					CorePackage newCore = newPackage as CorePackage;
					if (newCore != null)
					{
						this.corePackages.Add(newCore);
						return;
					}
					RegularPackage newRegular = newPackage as RegularPackage;
					if (newRegular != null)
					{
						this.regularPackages.Add(newRegular);
					}
				}
			}

			// Token: 0x060067FF RID: 26623 RVA: 0x0034B810 File Offset: 0x00349A10
			public void Refresh()
			{
				this.corePackages.RemoveWhere((CorePackage p) => !File.Exists(p.Path));
				this.regularPackages.RemoveWhere((RegularPackage p) => !File.Exists(p.Path));
				string[] subDirs = Directory.GetDirectories(this.directory, "*");
				string[] array = subDirs;
				for (int i = 0; i < array.Length; i++)
				{
					string subDir = array[i];
					string fileListPath = Path.Combine(new string[]
					{
						subDir,
						"filelist.xml"
					}).CleanUpPathCrossPlatform(true, "");
					if (!this.Any((ContentPackage p) => p.Path.Equals(fileListPath, StringComparison.OrdinalIgnoreCase)) && File.Exists(fileListPath))
					{
						Predicate<string> predicate = this.skipPredicate;
						if (!(((predicate != null) ? new bool?(predicate(fileListPath)) : null) ?? false))
						{
							Result<ContentPackage, Exception> result = ContentPackage.TryLoad(fileListPath);
							ContentPackage newPackage;
							if (!result.TryUnwrapSuccess(out newPackage))
							{
								Action<string, Exception> action = this.onLoadFail;
								if (action != null)
								{
									string fileListPath2 = fileListPath;
									Exception exception;
									if (!result.TryUnwrapFailure(out exception))
									{
										throw new UnreachableCodeException();
									}
									action(fileListPath2, exception);
								}
							}
							else
							{
								CorePackage corePackage = newPackage as CorePackage;
								if (corePackage == null)
								{
									RegularPackage regularPackage = newPackage as RegularPackage;
									if (regularPackage != null)
									{
										this.regularPackages.Add(regularPackage);
									}
								}
								else
								{
									this.corePackages.Add(corePackage);
								}
							}
						}
					}
				}
			}

			// Token: 0x170019D8 RID: 6616
			// (get) Token: 0x06006800 RID: 26624 RVA: 0x0034B9B5 File Offset: 0x00349BB5
			public IEnumerable<RegularPackage> Regular
			{
				get
				{
					return this.regularPackages;
				}
			}

			// Token: 0x170019D9 RID: 6617
			// (get) Token: 0x06006801 RID: 26625 RVA: 0x0034B9BD File Offset: 0x00349BBD
			public IEnumerable<CorePackage> Core
			{
				get
				{
					return this.corePackages;
				}
			}

			// Token: 0x06006802 RID: 26626 RVA: 0x0034B9C5 File Offset: 0x00349BC5
			public IEnumerator<ContentPackage> GetEnumerator()
			{
				ContentPackageManager.PackageSource.<GetEnumerator>d__15 <GetEnumerator>d__ = new ContentPackageManager.PackageSource.<GetEnumerator>d__15(0);
				<GetEnumerator>d__.<>4__this = this;
				return <GetEnumerator>d__;
			}

			// Token: 0x06006803 RID: 26627 RVA: 0x0034B9D4 File Offset: 0x00349BD4
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x06006804 RID: 26628 RVA: 0x0034B9DC File Offset: 0x00349BDC
			void ICollection<ContentPackage>.Add(ContentPackage item)
			{
				throw new InvalidOperationException();
			}

			// Token: 0x06006805 RID: 26629 RVA: 0x0034B9E3 File Offset: 0x00349BE3
			void ICollection<ContentPackage>.Clear()
			{
				throw new InvalidOperationException();
			}

			// Token: 0x06006806 RID: 26630 RVA: 0x0034B9EC File Offset: 0x00349BEC
			public bool Contains(ContentPackage item)
			{
				CorePackage core = item as CorePackage;
				bool result;
				if (core == null)
				{
					RegularPackage regular = item as RegularPackage;
					if (regular == null)
					{
						throw new ArgumentException("Expected regular or core package, got " + item.GetType().Name);
					}
					result = this.regularPackages.Contains(regular);
				}
				else
				{
					result = this.corePackages.Contains(core);
				}
				return result;
			}

			// Token: 0x06006807 RID: 26631 RVA: 0x0034BA4C File Offset: 0x00349C4C
			void ICollection<ContentPackage>.CopyTo(ContentPackage[] array, int arrayIndex)
			{
				foreach (CorePackage package in this.corePackages)
				{
					array[arrayIndex] = package;
					arrayIndex++;
				}
				foreach (RegularPackage package2 in this.regularPackages)
				{
					array[arrayIndex] = package2;
					arrayIndex++;
				}
			}

			// Token: 0x06006808 RID: 26632 RVA: 0x0034BAE8 File Offset: 0x00349CE8
			bool ICollection<ContentPackage>.Remove(ContentPackage item)
			{
				throw new InvalidOperationException();
			}

			// Token: 0x170019DA RID: 6618
			// (get) Token: 0x06006809 RID: 26633 RVA: 0x0034BAEF File Offset: 0x00349CEF
			public int Count
			{
				get
				{
					return this.corePackages.Count + this.regularPackages.Count;
				}
			}

			// Token: 0x170019DB RID: 6619
			// (get) Token: 0x0600680A RID: 26634 RVA: 0x0034BB08 File Offset: 0x00349D08
			public bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			// Token: 0x04003905 RID: 14597
			[Nullable(new byte[]
			{
				2,
				1
			})]
			private readonly Predicate<string> skipPredicate;

			// Token: 0x04003906 RID: 14598
			[Nullable(new byte[]
			{
				2,
				1,
				1
			})]
			private readonly Action<string, Exception> onLoadFail;

			// Token: 0x04003907 RID: 14599
			private readonly string directory;

			// Token: 0x04003908 RID: 14600
			private readonly HashSet<RegularPackage> regularPackages = new HashSet<RegularPackage>();

			// Token: 0x04003909 RID: 14601
			private readonly HashSet<CorePackage> corePackages = new HashSet<CorePackage>();
		}

		// Token: 0x02000731 RID: 1841
		[Nullable(0)]
		public static class EnabledPackages
		{
			// Token: 0x170019DC RID: 6620
			// (get) Token: 0x0600680B RID: 26635 RVA: 0x0034BB0B File Offset: 0x00349D0B
			// (set) Token: 0x0600680C RID: 26636 RVA: 0x0034BB12 File Offset: 0x00349D12
			[Nullable(2)]
			public static CorePackage Core { [NullableContext(2)] get; [NullableContext(2)] private set; } = null;

			// Token: 0x170019DD RID: 6621
			// (get) Token: 0x0600680D RID: 26637 RVA: 0x0034BB1A File Offset: 0x00349D1A
			public static IReadOnlyList<RegularPackage> Regular
			{
				get
				{
					return ContentPackageManager.EnabledPackages.regular;
				}
			}

			// Token: 0x170019DE RID: 6622
			// (get) Token: 0x0600680E RID: 26638 RVA: 0x0034BB21 File Offset: 0x00349D21
			// (set) Token: 0x0600680F RID: 26639 RVA: 0x0034BB28 File Offset: 0x00349D28
			public static Md5Hash MergedHash { get; private set; } = Md5Hash.Blank;

			// Token: 0x170019DF RID: 6623
			// (get) Token: 0x06006810 RID: 26640 RVA: 0x0034BB30 File Offset: 0x00349D30
			public static IEnumerable<ContentPackage> All
			{
				get
				{
					if (ContentPackageManager.EnabledPackages.Core == null)
					{
						return Enumerable.Empty<ContentPackage>();
					}
					return ContentPackageManager.EnabledPackages.Core.ToEnumerable<ContentPackage>().CollectionConcat(ContentPackageManager.EnabledPackages.Regular);
				}
			}

			// Token: 0x06006811 RID: 26641 RVA: 0x0034BB60 File Offset: 0x00349D60
			public static void SetCore(CorePackage newCore)
			{
				ContentPackageManager.EnabledPackages.SetCoreEnumerable(newCore).Consume<ContentPackageManager.LoadProgress>();
			}

			// Token: 0x06006812 RID: 26642 RVA: 0x0034BB6D File Offset: 0x00349D6D
			public static IEnumerable<ContentPackageManager.LoadProgress> SetCoreEnumerable(CorePackage newCore)
			{
				ContentPackageManager.EnabledPackages.<SetCoreEnumerable>d__15 <SetCoreEnumerable>d__ = new ContentPackageManager.EnabledPackages.<SetCoreEnumerable>d__15(-2);
				<SetCoreEnumerable>d__.<>3__newCore = newCore;
				return <SetCoreEnumerable>d__;
			}

			// Token: 0x06006813 RID: 26643 RVA: 0x0034BB7D File Offset: 0x00349D7D
			public static void ReloadCore()
			{
				if (ContentPackageManager.EnabledPackages.Core == null)
				{
					return;
				}
				ContentPackageManager.EnabledPackages.ReloadPackage(ContentPackageManager.EnabledPackages.Core);
			}

			// Token: 0x06006814 RID: 26644 RVA: 0x0034BB91 File Offset: 0x00349D91
			public static void ReloadPackage(ContentPackage p)
			{
				p.UnloadContent();
				p.LoadContent();
				ContentPackageManager.EnabledPackages.SortContent();
			}

			// Token: 0x06006815 RID: 26645 RVA: 0x0034BBA8 File Offset: 0x00349DA8
			public static void EnableRegular(RegularPackage p)
			{
				if (ContentPackageManager.EnabledPackages.regular.Contains(p))
				{
					return;
				}
				List<RegularPackage> newRegular = ContentPackageManager.EnabledPackages.regular.ToList<RegularPackage>();
				newRegular.Add(p);
				ContentPackageManager.EnabledPackages.SetRegular(newRegular);
			}

			// Token: 0x06006816 RID: 26646 RVA: 0x0034BBDB File Offset: 0x00349DDB
			public static void SetRegular(IReadOnlyList<RegularPackage> newRegular)
			{
				ContentPackageManager.EnabledPackages.SetRegularEnumerable(newRegular).Consume<ContentPackageManager.LoadProgress>();
			}

			// Token: 0x06006817 RID: 26647 RVA: 0x0034BBE8 File Offset: 0x00349DE8
			public static IEnumerable<ContentPackageManager.LoadProgress> SetRegularEnumerable(IReadOnlyList<RegularPackage> inNewRegular)
			{
				ContentPackageManager.EnabledPackages.<SetRegularEnumerable>d__20 <SetRegularEnumerable>d__ = new ContentPackageManager.EnabledPackages.<SetRegularEnumerable>d__20(-2);
				<SetRegularEnumerable>d__.<>3__inNewRegular = inNewRegular;
				return <SetRegularEnumerable>d__;
			}

			// Token: 0x06006818 RID: 26648 RVA: 0x0034BBF8 File Offset: 0x00349DF8
			public static void ThrowIfDuplicates(IEnumerable<ContentPackage> pkgs)
			{
				IList<ContentPackage> contentPackages = (pkgs as IList<ContentPackage>) ?? pkgs.ToArray<ContentPackage>();
				using (IEnumerator<ContentPackage> enumerator = contentPackages.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentPackage cp = enumerator.Current;
						if (contentPackages.AtLeast(2, (ContentPackage cp2) => cp == cp2))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
							defaultInterpolatedStringHandler.AppendLiteral("There are duplicates in the list of selected content packages (\"");
							defaultInterpolatedStringHandler.AppendFormatted(cp.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\", hash: ");
							Md5Hash hash = cp.Hash;
							defaultInterpolatedStringHandler.AppendFormatted(((hash != null) ? hash.ShortRepresentation : null) ?? "none");
							defaultInterpolatedStringHandler.AppendLiteral(")");
							throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
			}

			// Token: 0x06006819 RID: 26649 RVA: 0x0034BCE4 File Offset: 0x00349EE4
			private static void SortContent()
			{
				ContentPackageManager.EnabledPackages.ThrowIfDuplicates(ContentPackageManager.EnabledPackages.All);
				ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage r) => r.Files).Distinct(new ContentPackageManager.EnabledPackages.TypeComparer<ContentFile>()).ForEach(delegate(ContentFile f)
				{
					f.Sort();
				});
				ContentPackageManager.EnabledPackages.MergedHash = Md5Hash.MergeHashes(from cp in ContentPackageManager.EnabledPackages.All
				select cp.Hash);
				TextManager.IncrementLanguageVersion();
			}

			// Token: 0x0600681A RID: 26650 RVA: 0x0034BD8C File Offset: 0x00349F8C
			public static int IndexOf(ContentPackage contentPackage)
			{
				CorePackage core = contentPackage as CorePackage;
				if (core != null)
				{
					if (core == ContentPackageManager.EnabledPackages.Core)
					{
						return 0;
					}
					return -1;
				}
				else
				{
					RegularPackage reg = contentPackage as RegularPackage;
					if (reg != null)
					{
						return ContentPackageManager.EnabledPackages.Regular.IndexOf(reg) + 1;
					}
					return -1;
				}
			}

			// Token: 0x0600681B RID: 26651 RVA: 0x0034BDC8 File Offset: 0x00349FC8
			public static void DisableMods(IReadOnlyCollection<ContentPackage> mods)
			{
				if (ContentPackageManager.EnabledPackages.Core != null && mods.Contains(ContentPackageManager.EnabledPackages.Core))
				{
					CorePackage newCore = ContentPackageManager.CorePackages.FirstOrDefault((CorePackage p) => !mods.Contains(p));
					if (newCore != null)
					{
						ContentPackageManager.EnabledPackages.SetCore(newCore);
					}
				}
				ContentPackageManager.EnabledPackages.SetRegular((from p in ContentPackageManager.EnabledPackages.Regular
				where !mods.Contains(p)
				select p).ToArray<RegularPackage>());
			}

			// Token: 0x0600681C RID: 26652 RVA: 0x0034BE3C File Offset: 0x0034A03C
			public static void DisableRemovedMods()
			{
				if (ContentPackageManager.EnabledPackages.Core != null && !ContentPackageManager.CorePackages.Contains(ContentPackageManager.EnabledPackages.Core))
				{
					ContentPackageManager.EnabledPackages.SetCore(ContentPackageManager.CorePackages.First<CorePackage>());
				}
				ContentPackageManager.EnabledPackages.SetRegular((from p in ContentPackageManager.EnabledPackages.Regular
				where ContentPackageManager.RegularPackages.Contains(p)
				select p).ToArray<RegularPackage>());
			}

			// Token: 0x0600681D RID: 26653 RVA: 0x0034BEA4 File Offset: 0x0034A0A4
			public static void RefreshUpdatedMods()
			{
				if (ContentPackageManager.EnabledPackages.Core != null && !ContentPackageManager.CorePackages.Contains(ContentPackageManager.EnabledPackages.Core))
				{
					ContentPackageManager.EnabledPackages.SetCore(ContentPackageManager.WorkshopPackages.Core.FirstOrDefault((CorePackage p) => p.UgcId == ContentPackageManager.EnabledPackages.Core.UgcId) ?? ContentPackageManager.CorePackages.First<CorePackage>());
				}
				List<RegularPackage> newRegular = new List<RegularPackage>();
				using (IEnumerator<RegularPackage> enumerator = ContentPackageManager.EnabledPackages.Regular.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RegularPackage p = enumerator.Current;
						if (ContentPackageManager.RegularPackages.Contains(p))
						{
							newRegular.Add(p);
						}
						else
						{
							RegularPackage newP = ContentPackageManager.WorkshopPackages.Regular.FirstOrDefault((RegularPackage p2) => p2.UgcId == p.UgcId);
							if (newP != null)
							{
								newRegular.Add(newP);
							}
						}
					}
				}
				ContentPackageManager.EnabledPackages.SetRegular(newRegular);
			}

			// Token: 0x0600681E RID: 26654 RVA: 0x0034BFA0 File Offset: 0x0034A1A0
			public static void BackUp()
			{
				if (ContentPackageManager.EnabledPackages.BackupPackages.Core != null || ContentPackageManager.EnabledPackages.BackupPackages.Regular != null)
				{
					throw new InvalidOperationException("Tried to back up enabled packages multiple times");
				}
				ContentPackageManager.EnabledPackages.BackupPackages.Core = ContentPackageManager.EnabledPackages.Core;
				ContentPackageManager.EnabledPackages.BackupPackages.Regular = new ImmutableArray<RegularPackage>?(ContentPackageManager.EnabledPackages.Regular.ToImmutableArray<RegularPackage>());
			}

			// Token: 0x0600681F RID: 26655 RVA: 0x0034BFF4 File Offset: 0x0034A1F4
			public static void Restore()
			{
				if (ContentPackageManager.EnabledPackages.BackupPackages.Core == null || ContentPackageManager.EnabledPackages.BackupPackages.Regular == null)
				{
					DebugConsole.AddWarning("Tried to restore enabled packages multiple times/without performing a backup", null);
					return;
				}
				ContentPackageManager.EnabledPackages.SetCore(ContentPackageManager.EnabledPackages.BackupPackages.Core);
				ContentPackageManager.EnabledPackages.SetRegular(ContentPackageManager.EnabledPackages.BackupPackages.Regular);
				ContentPackageManager.EnabledPackages.BackupPackages.Core = null;
				ContentPackageManager.EnabledPackages.BackupPackages.Regular = null;
			}

			// Token: 0x0400390B RID: 14603
			private static readonly List<RegularPackage> regular = new List<RegularPackage>();

			// Token: 0x02001512 RID: 5394
			[NullableContext(0)]
			public static class BackupPackages
			{
				// Token: 0x04006757 RID: 26455
				[Nullable(2)]
				public static CorePackage Core;

				// Token: 0x04006758 RID: 26456
				[Nullable(new byte[]
				{
					0,
					1
				})]
				public static ImmutableArray<RegularPackage>? Regular;
			}

			// Token: 0x02001513 RID: 5395
			[Nullable(0)]
			private class TypeComparer<[Nullable(2)] T> : IEqualityComparer<T>
			{
				// Token: 0x06009C64 RID: 40036 RVA: 0x003EA29C File Offset: 0x003E849C
				public bool Equals([AllowNull] T x, [AllowNull] T y)
				{
					if (x == null || y == null)
					{
						return x == null == (y == null);
					}
					return x.GetType() == y.GetType();
				}

				// Token: 0x06009C65 RID: 40037 RVA: 0x003EA2ED File Offset: 0x003E84ED
				public int GetHashCode([DisallowNull] T obj)
				{
					return obj.GetType().GetHashCode();
				}
			}
		}

		// Token: 0x02000732 RID: 1842
		[Nullable(0)]
		public readonly struct LoadProgress : IEquatable<ContentPackageManager.LoadProgress>
		{
			// Token: 0x06006821 RID: 26657 RVA: 0x0034C06F File Offset: 0x0034A26F
			public LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error> Result)
			{
				this.Result = Result;
			}

			// Token: 0x170019E0 RID: 6624
			// (get) Token: 0x06006822 RID: 26658 RVA: 0x0034C078 File Offset: 0x0034A278
			// (set) Token: 0x06006823 RID: 26659 RVA: 0x0034C080 File Offset: 0x0034A280
			public Result<float, ContentPackageManager.LoadProgress.Error> Result { get; set; }

			// Token: 0x06006824 RID: 26660 RVA: 0x0034C089 File Offset: 0x0034A289
			public static ContentPackageManager.LoadProgress Failure(Exception exception)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Failure(new ContentPackageManager.LoadProgress.Error(exception)));
			}

			// Token: 0x06006825 RID: 26661 RVA: 0x0034C09B File Offset: 0x0034A29B
			public static ContentPackageManager.LoadProgress Failure(IEnumerable<string> errorMessages)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Failure(new ContentPackageManager.LoadProgress.Error(errorMessages)));
			}

			// Token: 0x06006826 RID: 26662 RVA: 0x0034C0AD File Offset: 0x0034A2AD
			public static ContentPackageManager.LoadProgress Progress(float value)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Success(value));
			}

			// Token: 0x06006827 RID: 26663 RVA: 0x0034C0BC File Offset: 0x0034A2BC
			[NullableContext(0)]
			public ContentPackageManager.LoadProgress Transform(Range<float> range)
			{
				float value;
				if (!this.Result.TryUnwrapSuccess(out value))
				{
					return this;
				}
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Success(MathHelper.Lerp(range.Start, range.End, value)));
			}

			// Token: 0x06006828 RID: 26664 RVA: 0x0034C100 File Offset: 0x0034A300
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LoadProgress");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006829 RID: 26665 RVA: 0x0034C14C File Offset: 0x0034A34C
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result);
				return true;
			}

			// Token: 0x0600682A RID: 26666 RVA: 0x0034C168 File Offset: 0x0034A368
			[CompilerGenerated]
			public static bool operator !=(ContentPackageManager.LoadProgress left, ContentPackageManager.LoadProgress right)
			{
				return !(left == right);
			}

			// Token: 0x0600682B RID: 26667 RVA: 0x0034C174 File Offset: 0x0034A374
			[CompilerGenerated]
			public static bool operator ==(ContentPackageManager.LoadProgress left, ContentPackageManager.LoadProgress right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600682C RID: 26668 RVA: 0x0034C17E File Offset: 0x0034A37E
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Result<float, ContentPackageManager.LoadProgress.Error>>.Default.GetHashCode(this.<Result>k__BackingField);
			}

			// Token: 0x0600682D RID: 26669 RVA: 0x0034C190 File Offset: 0x0034A390
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContentPackageManager.LoadProgress && this.Equals((ContentPackageManager.LoadProgress)obj);
			}

			// Token: 0x0600682E RID: 26670 RVA: 0x0034C1A8 File Offset: 0x0034A3A8
			[CompilerGenerated]
			public bool Equals(ContentPackageManager.LoadProgress other)
			{
				return EqualityComparer<Result<float, ContentPackageManager.LoadProgress.Error>>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField);
			}

			// Token: 0x0600682F RID: 26671 RVA: 0x0034C1C0 File Offset: 0x0034A3C0
			[CompilerGenerated]
			public void Deconstruct(out Result<float, ContentPackageManager.LoadProgress.Error> Result)
			{
				Result = this.Result;
			}

			// Token: 0x0200151B RID: 5403
			[NullableContext(0)]
			public readonly struct Error : IEquatable<ContentPackageManager.LoadProgress.Error>
			{
				// Token: 0x06009C8C RID: 40076 RVA: 0x003EA9C3 File Offset: 0x003E8BC3
				public Error([Nullable(new byte[]
				{
					1,
					0,
					1,
					1
				})] Either<ImmutableArray<string>, Exception> ErrorsOrException)
				{
					this.ErrorsOrException = ErrorsOrException;
				}

				// Token: 0x17001D7F RID: 7551
				// (get) Token: 0x06009C8D RID: 40077 RVA: 0x003EA9CC File Offset: 0x003E8BCC
				// (set) Token: 0x06009C8E RID: 40078 RVA: 0x003EA9D4 File Offset: 0x003E8BD4
				[Nullable(new byte[]
				{
					1,
					0,
					1,
					1
				})]
				public Either<ImmutableArray<string>, Exception> ErrorsOrException { [return: Nullable(new byte[]
				{
					1,
					0,
					1,
					1
				})] get; [param: Nullable(new byte[]
				{
					1,
					0,
					1,
					1
				})] set; }

				// Token: 0x06009C8F RID: 40079 RVA: 0x003EA9DD File Offset: 0x003E8BDD
				[NullableContext(1)]
				public Error(IEnumerable<string> errorMessages)
				{
					this = new ContentPackageManager.LoadProgress.Error(errorMessages.ToImmutableArray<string>());
				}

				// Token: 0x06009C90 RID: 40080 RVA: 0x003EA9F0 File Offset: 0x003E8BF0
				[NullableContext(1)]
				public Error(Exception exception)
				{
					this = new ContentPackageManager.LoadProgress.Error(exception);
				}

				// Token: 0x06009C91 RID: 40081 RVA: 0x003EAA00 File Offset: 0x003E8C00
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("Error");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009C92 RID: 40082 RVA: 0x003EAA4C File Offset: 0x003E8C4C
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("ErrorsOrException = ");
					builder.Append(this.ErrorsOrException);
					return true;
				}

				// Token: 0x06009C93 RID: 40083 RVA: 0x003EAA68 File Offset: 0x003E8C68
				[CompilerGenerated]
				public static bool operator !=(ContentPackageManager.LoadProgress.Error left, ContentPackageManager.LoadProgress.Error right)
				{
					return !(left == right);
				}

				// Token: 0x06009C94 RID: 40084 RVA: 0x003EAA74 File Offset: 0x003E8C74
				[CompilerGenerated]
				public static bool operator ==(ContentPackageManager.LoadProgress.Error left, ContentPackageManager.LoadProgress.Error right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009C95 RID: 40085 RVA: 0x003EAA7E File Offset: 0x003E8C7E
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return EqualityComparer<Either<ImmutableArray<string>, Exception>>.Default.GetHashCode(this.<ErrorsOrException>k__BackingField);
				}

				// Token: 0x06009C96 RID: 40086 RVA: 0x003EAA90 File Offset: 0x003E8C90
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is ContentPackageManager.LoadProgress.Error && this.Equals((ContentPackageManager.LoadProgress.Error)obj);
				}

				// Token: 0x06009C97 RID: 40087 RVA: 0x003EAAA8 File Offset: 0x003E8CA8
				[CompilerGenerated]
				public bool Equals(ContentPackageManager.LoadProgress.Error other)
				{
					return EqualityComparer<Either<ImmutableArray<string>, Exception>>.Default.Equals(this.<ErrorsOrException>k__BackingField, other.<ErrorsOrException>k__BackingField);
				}

				// Token: 0x06009C98 RID: 40088 RVA: 0x003EAAC0 File Offset: 0x003E8CC0
				[CompilerGenerated]
				public void Deconstruct([Nullable(new byte[]
				{
					1,
					0,
					1,
					1
				})] out Either<ImmutableArray<string>, Exception> ErrorsOrException)
				{
					ErrorsOrException = this.ErrorsOrException;
				}
			}
		}

		// Token: 0x02000733 RID: 1843
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400390E RID: 14606
			[Nullable(0)]
			public static Func<Item, Task> <0>__DownloadModThenEnqueueInstall;
		}
	}
}
