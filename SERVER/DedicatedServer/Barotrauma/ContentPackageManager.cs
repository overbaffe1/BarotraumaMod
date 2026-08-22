using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x02000161 RID: 353
	[NullableContext(1)]
	[Nullable(0)]
	public static class ContentPackageManager
	{
		// Token: 0x17000848 RID: 2120
		// (get) Token: 0x06001CD3 RID: 7379 RVA: 0x000D04CC File Offset: 0x000CE6CC
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

		// Token: 0x17000849 RID: 2121
		// (get) Token: 0x06001CD4 RID: 7380 RVA: 0x000D0500 File Offset: 0x000CE700
		// (set) Token: 0x06001CD5 RID: 7381 RVA: 0x000D0507 File Offset: 0x000CE707
		[Nullable(2)]
		public static CorePackage VanillaCorePackage { [NullableContext(2)] get; [NullableContext(2)] private set; } = null;

		// Token: 0x1700084A RID: 2122
		// (get) Token: 0x06001CD6 RID: 7382 RVA: 0x000D0510 File Offset: 0x000CE710
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

		// Token: 0x1700084B RID: 2123
		// (get) Token: 0x06001CD7 RID: 7383 RVA: 0x000D0555 File Offset: 0x000CE755
		public static IEnumerable<RegularPackage> RegularPackages
		{
			get
			{
				return ContentPackageManager.LocalPackages.Regular.CollectionConcat(ContentPackageManager.WorkshopPackages.Regular);
			}
		}

		// Token: 0x1700084C RID: 2124
		// (get) Token: 0x06001CD8 RID: 7384 RVA: 0x000D0570 File Offset: 0x000CE770
		public static IEnumerable<ContentPackage> AllPackages
		{
			get
			{
				return ContentPackageManager.VanillaCorePackage.ToEnumerable<CorePackage>().CollectionConcat(ContentPackageManager.LocalPackages).CollectionConcat(ContentPackageManager.WorkshopPackages).OfType<ContentPackage>();
			}
		}

		// Token: 0x06001CD9 RID: 7385 RVA: 0x000D0595 File Offset: 0x000CE795
		public static void UpdateContentPackageList()
		{
			ContentPackageManager.LocalPackages.Refresh();
			ContentPackageManager.WorkshopPackages.Refresh();
			ContentPackageManager.EnabledPackages.DisableRemovedMods();
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x000D05B0 File Offset: 0x000CE7B0
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

		// Token: 0x06001CDB RID: 7387 RVA: 0x000D0644 File Offset: 0x000CE844
		public static void LoadVanillaFileList()
		{
			ContentPackageManager.VanillaCorePackage = new CorePackage(XDocument.Load("Content/ContentPackages/Vanilla.xml"), "Content/ContentPackages/Vanilla.xml");
			foreach (ContentPackage.LoadError error in ContentPackageManager.VanillaCorePackage.FatalLoadErrors)
			{
				DebugConsole.ThrowError(error.ToString(), null, null, false, false);
			}
		}

		// Token: 0x06001CDC RID: 7388 RVA: 0x000D06A6 File Offset: 0x000CE8A6
		public static IEnumerable<ContentPackageManager.LoadProgress> Init()
		{
			return new ContentPackageManager.<Init>d__25(-2);
		}

		// Token: 0x06001CDD RID: 7389 RVA: 0x000D06B0 File Offset: 0x000CE8B0
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

		// Token: 0x06001CDE RID: 7390 RVA: 0x000D0724 File Offset: 0x000CE924
		public static void LogEnabledRegularPackageErrors()
		{
			foreach (RegularPackage p in ContentPackageManager.EnabledPackages.Regular)
			{
				p.LogErrors();
			}
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x000D07C0 File Offset: 0x000CE9C0
		[CompilerGenerated]
		[return: Nullable(2)]
		internal static T <Init>g__findPackage|25_0<T>(IEnumerable<T> packages, [Nullable(2)] XElement elem) where T : ContentPackage
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

		// Token: 0x04000D1A RID: 3354
		public const string CopyIndicatorFileName = ".copying";

		// Token: 0x04000D1B RID: 3355
		public const string VanillaFileList = "Content/ContentPackages/Vanilla.xml";

		// Token: 0x04000D1C RID: 3356
		public const string CorePackageElementName = "corepackage";

		// Token: 0x04000D1D RID: 3357
		public const string RegularPackagesElementName = "regularpackages";

		// Token: 0x04000D1E RID: 3358
		public const string RegularPackagesSubElementName = "package";

		// Token: 0x04000D1F RID: 3359
		public static readonly ContentPackageManager.PackageSource LocalPackages = new ContentPackageManager.PackageSource("LocalMods", null, null);

		// Token: 0x04000D20 RID: 3360
		public static readonly ContentPackageManager.PackageSource WorkshopPackages = new ContentPackageManager.PackageSource(ContentPackage.WorkshopModsDir, new Predicate<string>(SteamManager.Workshop.IsInstallingToPath), delegate(string fileListPath, Exception exception)
		{
			Directory.TryDelete(Path.GetDirectoryName(fileListPath), true);
		});

		// Token: 0x020008EB RID: 2283
		[Nullable(0)]
		public static class EnabledPackages
		{
			// Token: 0x1700153C RID: 5436
			// (get) Token: 0x06005813 RID: 22547 RVA: 0x001F5E6A File Offset: 0x001F406A
			// (set) Token: 0x06005814 RID: 22548 RVA: 0x001F5E71 File Offset: 0x001F4071
			[Nullable(2)]
			public static CorePackage Core { [NullableContext(2)] get; [NullableContext(2)] private set; } = null;

			// Token: 0x1700153D RID: 5437
			// (get) Token: 0x06005815 RID: 22549 RVA: 0x001F5E79 File Offset: 0x001F4079
			public static IReadOnlyList<RegularPackage> Regular
			{
				get
				{
					return ContentPackageManager.EnabledPackages.regular;
				}
			}

			// Token: 0x1700153E RID: 5438
			// (get) Token: 0x06005816 RID: 22550 RVA: 0x001F5E80 File Offset: 0x001F4080
			// (set) Token: 0x06005817 RID: 22551 RVA: 0x001F5E87 File Offset: 0x001F4087
			public static Md5Hash MergedHash { get; private set; } = Md5Hash.Blank;

			// Token: 0x1700153F RID: 5439
			// (get) Token: 0x06005818 RID: 22552 RVA: 0x001F5E90 File Offset: 0x001F4090
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

			// Token: 0x06005819 RID: 22553 RVA: 0x001F5EC0 File Offset: 0x001F40C0
			public static void SetCore(CorePackage newCore)
			{
				ContentPackageManager.EnabledPackages.SetCoreEnumerable(newCore).Consume<ContentPackageManager.LoadProgress>();
			}

			// Token: 0x0600581A RID: 22554 RVA: 0x001F5ECD File Offset: 0x001F40CD
			public static IEnumerable<ContentPackageManager.LoadProgress> SetCoreEnumerable(CorePackage newCore)
			{
				ContentPackageManager.EnabledPackages.<SetCoreEnumerable>d__15 <SetCoreEnumerable>d__ = new ContentPackageManager.EnabledPackages.<SetCoreEnumerable>d__15(-2);
				<SetCoreEnumerable>d__.<>3__newCore = newCore;
				return <SetCoreEnumerable>d__;
			}

			// Token: 0x0600581B RID: 22555 RVA: 0x001F5EDD File Offset: 0x001F40DD
			public static void ReloadCore()
			{
				if (ContentPackageManager.EnabledPackages.Core == null)
				{
					return;
				}
				ContentPackageManager.EnabledPackages.ReloadPackage(ContentPackageManager.EnabledPackages.Core);
			}

			// Token: 0x0600581C RID: 22556 RVA: 0x001F5EF1 File Offset: 0x001F40F1
			public static void ReloadPackage(ContentPackage p)
			{
				p.UnloadContent();
				p.LoadContent();
				ContentPackageManager.EnabledPackages.SortContent();
			}

			// Token: 0x0600581D RID: 22557 RVA: 0x001F5F08 File Offset: 0x001F4108
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

			// Token: 0x0600581E RID: 22558 RVA: 0x001F5F3B File Offset: 0x001F413B
			public static void SetRegular(IReadOnlyList<RegularPackage> newRegular)
			{
				ContentPackageManager.EnabledPackages.SetRegularEnumerable(newRegular).Consume<ContentPackageManager.LoadProgress>();
			}

			// Token: 0x0600581F RID: 22559 RVA: 0x001F5F48 File Offset: 0x001F4148
			public static IEnumerable<ContentPackageManager.LoadProgress> SetRegularEnumerable(IReadOnlyList<RegularPackage> inNewRegular)
			{
				ContentPackageManager.EnabledPackages.<SetRegularEnumerable>d__20 <SetRegularEnumerable>d__ = new ContentPackageManager.EnabledPackages.<SetRegularEnumerable>d__20(-2);
				<SetRegularEnumerable>d__.<>3__inNewRegular = inNewRegular;
				return <SetRegularEnumerable>d__;
			}

			// Token: 0x06005820 RID: 22560 RVA: 0x001F5F58 File Offset: 0x001F4158
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

			// Token: 0x06005821 RID: 22561 RVA: 0x001F6044 File Offset: 0x001F4244
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

			// Token: 0x06005822 RID: 22562 RVA: 0x001F60EC File Offset: 0x001F42EC
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

			// Token: 0x06005823 RID: 22563 RVA: 0x001F6128 File Offset: 0x001F4328
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

			// Token: 0x06005824 RID: 22564 RVA: 0x001F619C File Offset: 0x001F439C
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

			// Token: 0x06005825 RID: 22565 RVA: 0x001F6204 File Offset: 0x001F4404
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

			// Token: 0x06005826 RID: 22566 RVA: 0x001F6300 File Offset: 0x001F4500
			public static void BackUp()
			{
				if (ContentPackageManager.EnabledPackages.BackupPackages.Core != null || ContentPackageManager.EnabledPackages.BackupPackages.Regular != null)
				{
					throw new InvalidOperationException("Tried to back up enabled packages multiple times");
				}
				ContentPackageManager.EnabledPackages.BackupPackages.Core = ContentPackageManager.EnabledPackages.Core;
				ContentPackageManager.EnabledPackages.BackupPackages.Regular = new ImmutableArray<RegularPackage>?(ContentPackageManager.EnabledPackages.Regular.ToImmutableArray<RegularPackage>());
			}

			// Token: 0x06005827 RID: 22567 RVA: 0x001F6354 File Offset: 0x001F4554
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

			// Token: 0x04003198 RID: 12696
			private static readonly List<RegularPackage> regular = new List<RegularPackage>();

			// Token: 0x02000E8E RID: 3726
			[NullableContext(0)]
			public static class BackupPackages
			{
				// Token: 0x040042AC RID: 17068
				[Nullable(2)]
				public static CorePackage Core;

				// Token: 0x040042AD RID: 17069
				[Nullable(new byte[]
				{
					0,
					1
				})]
				public static ImmutableArray<RegularPackage>? Regular;
			}

			// Token: 0x02000E8F RID: 3727
			[Nullable(0)]
			private class TypeComparer<[Nullable(2)] T> : IEqualityComparer<T>
			{
				// Token: 0x06006A47 RID: 27207 RVA: 0x00225F44 File Offset: 0x00224144
				public bool Equals([AllowNull] T x, [AllowNull] T y)
				{
					if (x == null || y == null)
					{
						return x == null == (y == null);
					}
					return x.GetType() == y.GetType();
				}

				// Token: 0x06006A48 RID: 27208 RVA: 0x00225F95 File Offset: 0x00224195
				public int GetHashCode([DisallowNull] T obj)
				{
					return obj.GetType().GetHashCode();
				}
			}
		}

		// Token: 0x020008EC RID: 2284
		[Nullable(0)]
		public sealed class PackageSource : ICollection<ContentPackage>, IEnumerable<ContentPackage>, IEnumerable
		{
			// Token: 0x06005829 RID: 22569 RVA: 0x001F63CF File Offset: 0x001F45CF
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

			// Token: 0x0600582A RID: 22570 RVA: 0x001F6410 File Offset: 0x001F4610
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

			// Token: 0x0600582B RID: 22571 RVA: 0x001F64A4 File Offset: 0x001F46A4
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

			// Token: 0x17001540 RID: 5440
			// (get) Token: 0x0600582C RID: 22572 RVA: 0x001F6649 File Offset: 0x001F4849
			public IEnumerable<RegularPackage> Regular
			{
				get
				{
					return this.regularPackages;
				}
			}

			// Token: 0x17001541 RID: 5441
			// (get) Token: 0x0600582D RID: 22573 RVA: 0x001F6651 File Offset: 0x001F4851
			public IEnumerable<CorePackage> Core
			{
				get
				{
					return this.corePackages;
				}
			}

			// Token: 0x0600582E RID: 22574 RVA: 0x001F6659 File Offset: 0x001F4859
			public IEnumerator<ContentPackage> GetEnumerator()
			{
				ContentPackageManager.PackageSource.<GetEnumerator>d__12 <GetEnumerator>d__ = new ContentPackageManager.PackageSource.<GetEnumerator>d__12(0);
				<GetEnumerator>d__.<>4__this = this;
				return <GetEnumerator>d__;
			}

			// Token: 0x0600582F RID: 22575 RVA: 0x001F6668 File Offset: 0x001F4868
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x06005830 RID: 22576 RVA: 0x001F6670 File Offset: 0x001F4870
			void ICollection<ContentPackage>.Add(ContentPackage item)
			{
				throw new InvalidOperationException();
			}

			// Token: 0x06005831 RID: 22577 RVA: 0x001F6677 File Offset: 0x001F4877
			void ICollection<ContentPackage>.Clear()
			{
				throw new InvalidOperationException();
			}

			// Token: 0x06005832 RID: 22578 RVA: 0x001F6680 File Offset: 0x001F4880
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

			// Token: 0x06005833 RID: 22579 RVA: 0x001F66E0 File Offset: 0x001F48E0
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

			// Token: 0x06005834 RID: 22580 RVA: 0x001F677C File Offset: 0x001F497C
			bool ICollection<ContentPackage>.Remove(ContentPackage item)
			{
				throw new InvalidOperationException();
			}

			// Token: 0x17001542 RID: 5442
			// (get) Token: 0x06005835 RID: 22581 RVA: 0x001F6783 File Offset: 0x001F4983
			public int Count
			{
				get
				{
					return this.corePackages.Count + this.regularPackages.Count;
				}
			}

			// Token: 0x17001543 RID: 5443
			// (get) Token: 0x06005836 RID: 22582 RVA: 0x001F679C File Offset: 0x001F499C
			public bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			// Token: 0x0400319A RID: 12698
			[Nullable(new byte[]
			{
				2,
				1
			})]
			private readonly Predicate<string> skipPredicate;

			// Token: 0x0400319B RID: 12699
			[Nullable(new byte[]
			{
				2,
				1,
				1
			})]
			private readonly Action<string, Exception> onLoadFail;

			// Token: 0x0400319C RID: 12700
			private readonly string directory;

			// Token: 0x0400319D RID: 12701
			private readonly HashSet<RegularPackage> regularPackages = new HashSet<RegularPackage>();

			// Token: 0x0400319E RID: 12702
			private readonly HashSet<CorePackage> corePackages = new HashSet<CorePackage>();
		}

		// Token: 0x020008ED RID: 2285
		[Nullable(0)]
		public readonly struct LoadProgress : IEquatable<ContentPackageManager.LoadProgress>
		{
			// Token: 0x06005837 RID: 22583 RVA: 0x001F679F File Offset: 0x001F499F
			public LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error> Result)
			{
				this.Result = Result;
			}

			// Token: 0x17001544 RID: 5444
			// (get) Token: 0x06005838 RID: 22584 RVA: 0x001F67A8 File Offset: 0x001F49A8
			// (set) Token: 0x06005839 RID: 22585 RVA: 0x001F67B0 File Offset: 0x001F49B0
			public Result<float, ContentPackageManager.LoadProgress.Error> Result { get; set; }

			// Token: 0x0600583A RID: 22586 RVA: 0x001F67B9 File Offset: 0x001F49B9
			public static ContentPackageManager.LoadProgress Failure(Exception exception)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Failure(new ContentPackageManager.LoadProgress.Error(exception)));
			}

			// Token: 0x0600583B RID: 22587 RVA: 0x001F67CB File Offset: 0x001F49CB
			public static ContentPackageManager.LoadProgress Failure(IEnumerable<string> errorMessages)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Failure(new ContentPackageManager.LoadProgress.Error(errorMessages)));
			}

			// Token: 0x0600583C RID: 22588 RVA: 0x001F67DD File Offset: 0x001F49DD
			public static ContentPackageManager.LoadProgress Progress(float value)
			{
				return new ContentPackageManager.LoadProgress(Result<float, ContentPackageManager.LoadProgress.Error>.Success(value));
			}

			// Token: 0x0600583D RID: 22589 RVA: 0x001F67EC File Offset: 0x001F49EC
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

			// Token: 0x0600583E RID: 22590 RVA: 0x001F6830 File Offset: 0x001F4A30
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

			// Token: 0x0600583F RID: 22591 RVA: 0x001F687C File Offset: 0x001F4A7C
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Result = ");
				builder.Append(this.Result);
				return true;
			}

			// Token: 0x06005840 RID: 22592 RVA: 0x001F6898 File Offset: 0x001F4A98
			[CompilerGenerated]
			public static bool operator !=(ContentPackageManager.LoadProgress left, ContentPackageManager.LoadProgress right)
			{
				return !(left == right);
			}

			// Token: 0x06005841 RID: 22593 RVA: 0x001F68A4 File Offset: 0x001F4AA4
			[CompilerGenerated]
			public static bool operator ==(ContentPackageManager.LoadProgress left, ContentPackageManager.LoadProgress right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005842 RID: 22594 RVA: 0x001F68AE File Offset: 0x001F4AAE
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<Result<float, ContentPackageManager.LoadProgress.Error>>.Default.GetHashCode(this.<Result>k__BackingField);
			}

			// Token: 0x06005843 RID: 22595 RVA: 0x001F68C0 File Offset: 0x001F4AC0
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContentPackageManager.LoadProgress && this.Equals((ContentPackageManager.LoadProgress)obj);
			}

			// Token: 0x06005844 RID: 22596 RVA: 0x001F68D8 File Offset: 0x001F4AD8
			[CompilerGenerated]
			public bool Equals(ContentPackageManager.LoadProgress other)
			{
				return EqualityComparer<Result<float, ContentPackageManager.LoadProgress.Error>>.Default.Equals(this.<Result>k__BackingField, other.<Result>k__BackingField);
			}

			// Token: 0x06005845 RID: 22597 RVA: 0x001F68F0 File Offset: 0x001F4AF0
			[CompilerGenerated]
			public void Deconstruct(out Result<float, ContentPackageManager.LoadProgress.Error> Result)
			{
				Result = this.Result;
			}

			// Token: 0x02000E9A RID: 3738
			[NullableContext(0)]
			public readonly struct Error : IEquatable<ContentPackageManager.LoadProgress.Error>
			{
				// Token: 0x06006A7D RID: 27261 RVA: 0x002268B3 File Offset: 0x00224AB3
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

				// Token: 0x170016B2 RID: 5810
				// (get) Token: 0x06006A7E RID: 27262 RVA: 0x002268BC File Offset: 0x00224ABC
				// (set) Token: 0x06006A7F RID: 27263 RVA: 0x002268C4 File Offset: 0x00224AC4
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

				// Token: 0x06006A80 RID: 27264 RVA: 0x002268CD File Offset: 0x00224ACD
				[NullableContext(1)]
				public Error(IEnumerable<string> errorMessages)
				{
					this = new ContentPackageManager.LoadProgress.Error(errorMessages.ToImmutableArray<string>());
				}

				// Token: 0x06006A81 RID: 27265 RVA: 0x002268E0 File Offset: 0x00224AE0
				[NullableContext(1)]
				public Error(Exception exception)
				{
					this = new ContentPackageManager.LoadProgress.Error(exception);
				}

				// Token: 0x06006A82 RID: 27266 RVA: 0x002268F0 File Offset: 0x00224AF0
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

				// Token: 0x06006A83 RID: 27267 RVA: 0x0022693C File Offset: 0x00224B3C
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("ErrorsOrException = ");
					builder.Append(this.ErrorsOrException);
					return true;
				}

				// Token: 0x06006A84 RID: 27268 RVA: 0x00226958 File Offset: 0x00224B58
				[CompilerGenerated]
				public static bool operator !=(ContentPackageManager.LoadProgress.Error left, ContentPackageManager.LoadProgress.Error right)
				{
					return !(left == right);
				}

				// Token: 0x06006A85 RID: 27269 RVA: 0x00226964 File Offset: 0x00224B64
				[CompilerGenerated]
				public static bool operator ==(ContentPackageManager.LoadProgress.Error left, ContentPackageManager.LoadProgress.Error right)
				{
					return left.Equals(right);
				}

				// Token: 0x06006A86 RID: 27270 RVA: 0x0022696E File Offset: 0x00224B6E
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return EqualityComparer<Either<ImmutableArray<string>, Exception>>.Default.GetHashCode(this.<ErrorsOrException>k__BackingField);
				}

				// Token: 0x06006A87 RID: 27271 RVA: 0x00226980 File Offset: 0x00224B80
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is ContentPackageManager.LoadProgress.Error && this.Equals((ContentPackageManager.LoadProgress.Error)obj);
				}

				// Token: 0x06006A88 RID: 27272 RVA: 0x00226998 File Offset: 0x00224B98
				[CompilerGenerated]
				public bool Equals(ContentPackageManager.LoadProgress.Error other)
				{
					return EqualityComparer<Either<ImmutableArray<string>, Exception>>.Default.Equals(this.<ErrorsOrException>k__BackingField, other.<ErrorsOrException>k__BackingField);
				}

				// Token: 0x06006A89 RID: 27273 RVA: 0x002269B0 File Offset: 0x00224BB0
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
	}
}
