using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x0200003F RID: 63
	[NullableContext(1)]
	[Nullable(0)]
	public class ModProject
	{
		// Token: 0x06000985 RID: 2437 RVA: 0x00056B94 File Offset: 0x00054D94
		public ModProject()
		{
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x00056BEC File Offset: 0x00054DEC
		[NullableContext(2)]
		public ModProject(ContentPackage contentPackage)
		{
			if (contentPackage == null)
			{
				return;
			}
			this.Name = contentPackage.Name;
			this.AltNames = contentPackage.AltNames.ToList<string>();
			ImmutableArray<ContentFile> immutableArray = contentPackage.Files;
			Func<ContentFile, ModProject.File> selector;
			if ((selector = ModProject.<>O.<0>__FromContentFile) == null)
			{
				selector = (ModProject.<>O.<0>__FromContentFile = new Func<ContentFile, ModProject.File>(ModProject.File.FromContentFile));
			}
			this.files = immutableArray.Select(selector).ToList<ModProject.File>();
			this.ModVersion = ModProject.IncrementModVersion(contentPackage.ModVersion);
			this.IsCore = (contentPackage is CorePackage);
			this.UgcId = contentPackage.UgcId;
			this.ExpectedHash = contentPackage.Hash;
			this.InstallTime = contentPackage.InstallTime;
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000987 RID: 2439 RVA: 0x00056CDC File Offset: 0x00054EDC
		// (set) Token: 0x06000988 RID: 2440 RVA: 0x00056CE4 File Offset: 0x00054EE4
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				ImmutableHashSet<char> charsToRemove = Path.GetInvalidFileNameCharsCrossPlatform();
				this.name = string.Concat<char>(from c in value
				where !charsToRemove.Contains(c)
				select c);
			}
		}

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000989 RID: 2441 RVA: 0x00056D1F File Offset: 0x00054F1F
		public IReadOnlyList<ModProject.File> Files
		{
			get
			{
				return this.files;
			}
		}

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x0600098A RID: 2442 RVA: 0x00056D27 File Offset: 0x00054F27
		// (set) Token: 0x0600098B RID: 2443 RVA: 0x00056D2F File Offset: 0x00054F2F
		[Nullable(2)]
		public Md5Hash ExpectedHash { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x0600098C RID: 2444 RVA: 0x00056D38 File Offset: 0x00054F38
		public bool HasFile(ModProject.File file)
		{
			return this.Files.Any((ModProject.File f) => string.Equals(f.Path, file.Path, StringComparison.OrdinalIgnoreCase) && f.Type == file.Type);
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x00056D69 File Offset: 0x00054F69
		public void AddFile(ModProject.File file)
		{
			if (!this.HasFile(file))
			{
				this.files.Add(file);
				this.DiscardHashAndInstallTime();
			}
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x00056D86 File Offset: 0x00054F86
		public void RemoveFile(ModProject.File file)
		{
			if (this.HasFile(file))
			{
				this.files.Remove(file);
				this.DiscardHashAndInstallTime();
			}
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x00056DA4 File Offset: 0x00054FA4
		public void DiscardHashAndInstallTime()
		{
			this.ExpectedHash = null;
			this.InstallTime = Option<SerializableDateTime>.None();
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x00056DB8 File Offset: 0x00054FB8
		public static string IncrementModVersion(string modVersion)
		{
			if (string.IsNullOrWhiteSpace(modVersion))
			{
				return string.Empty;
			}
			int startIndex = modVersion.Length - 1;
			while (startIndex > 0 && char.IsDigit(modVersion[startIndex]))
			{
				startIndex--;
			}
			startIndex++;
			int theFinalInteger;
			if (startIndex >= modVersion.Length || !char.IsDigit(modVersion[startIndex]) || !int.TryParse(modVersion.Substring(startIndex), NumberStyles.Any, CultureInfo.InvariantCulture, out theFinalInteger))
			{
				return modVersion;
			}
			return modVersion.Substring(0, startIndex) + (theFinalInteger + 1).ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x00056E48 File Offset: 0x00055048
		public XDocument ToXDocument()
		{
			ModProject.<>c__DisplayClass24_0 CS$<>8__locals1 = new ModProject.<>c__DisplayClass24_0();
			XDocument doc = new XDocument();
			CS$<>8__locals1.rootElement = new XElement("contentpackage");
			CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<string>("name", this.Name);
			if (!this.ModVersion.IsNullOrEmpty())
			{
				CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<string>("modversion", this.ModVersion);
			}
			CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<bool>("corepackage", this.IsCore);
			ContentPackageId ugcId;
			if (this.UgcId.TryUnwrap(out ugcId))
			{
				SteamWorkshopId steamWorkshopId = ugcId as SteamWorkshopId;
				if (steamWorkshopId != null)
				{
					CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<ulong>("steamworkshopid", steamWorkshopId.Value);
				}
			}
			CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<Version>("gameversion", GameMain.Version);
			if (this.AltNames.Any<string>())
			{
				CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<string>("altnames", string.Join(",", this.AltNames));
			}
			if (this.ExpectedHash != null)
			{
				CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<string>("expectedhash", this.ExpectedHash.StringRepresentation);
			}
			SerializableDateTime installTime;
			if (this.InstallTime.TryUnwrap(out installTime))
			{
				CS$<>8__locals1.<ToXDocument>g__addRootAttribute|0<SerializableDateTime>("installtime", installTime);
			}
			this.files.ForEach(delegate(ModProject.File f)
			{
				CS$<>8__locals1.rootElement.Add(f.ToXElement());
			});
			doc.Add(CS$<>8__locals1.rootElement);
			return doc;
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00056F7C File Offset: 0x0005517C
		public void Save(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path), catchUnauthorizedAccessExceptions);
			this.ToXDocument().SaveSafe(path, SaveOptions.None, false, 0);
		}

		// Token: 0x040004F7 RID: 1271
		private string name = "";

		// Token: 0x040004F8 RID: 1272
		public readonly List<string> AltNames = new List<string>();

		// Token: 0x040004F9 RID: 1273
		private readonly List<ModProject.File> files = new List<ModProject.File>();

		// Token: 0x040004FA RID: 1274
		public string ModVersion = "1.0.0";

		// Token: 0x040004FC RID: 1276
		public bool IsCore;

		// Token: 0x040004FD RID: 1277
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<ContentPackageId> UgcId = Option<ContentPackageId>.None();

		// Token: 0x040004FE RID: 1278
		[Nullable(0)]
		public Option<SerializableDateTime> InstallTime = Option<SerializableDateTime>.None();

		// Token: 0x0200072B RID: 1835
		[Nullable(0)]
		public class File
		{
			// Token: 0x060067ED RID: 26605 RVA: 0x0034B49C File Offset: 0x0034969C
			private File(string path, Type type)
			{
				this.Path = path.CleanUpPathCrossPlatform(false, "");
				if (!type.IsSubclassOf(typeof(ContentFile)))
				{
					throw new ArgumentException(type.Name + " does not derive from ContentFile");
				}
				if (type != null)
				{
					if (type.IsAbstract)
					{
						throw new ArgumentException(type.Name + " is abstract");
					}
				}
				this.Type = type;
			}

			// Token: 0x060067EE RID: 26606 RVA: 0x0034B51B File Offset: 0x0034971B
			private File(ContentFile f)
			{
				this.Path = (f.Path.RawValue ?? "");
				this.Type = f.GetType();
			}

			// Token: 0x060067EF RID: 26607 RVA: 0x0034B549 File Offset: 0x00349749
			public static ModProject.File FromContentFile(ContentFile file)
			{
				return new ModProject.File(file);
			}

			// Token: 0x060067F0 RID: 26608 RVA: 0x0034B551 File Offset: 0x00349751
			public static ModProject.File FromPath<[Nullable(0)] T>(string path) where T : ContentFile
			{
				return new ModProject.File(path, typeof(T));
			}

			// Token: 0x060067F1 RID: 26609 RVA: 0x0034B563 File Offset: 0x00349763
			public static ModProject.File FromPath(string path, Type type)
			{
				return new ModProject.File(path, type);
			}

			// Token: 0x060067F2 RID: 26610 RVA: 0x0034B56C File Offset: 0x0034976C
			public XElement ToXElement()
			{
				if (this.Type == null)
				{
					throw new InvalidOperationException("Type must be set before calling ToXElement");
				}
				if (this.Path.IsNullOrEmpty())
				{
					throw new InvalidOperationException("Path must be set before calling ToXElement");
				}
				return new XElement(this.Type.Name.RemoveFromEnd("File", StringComparison.Ordinal), new XAttribute("file", this.Path));
			}

			// Token: 0x040038FF RID: 14591
			public readonly string Path;

			// Token: 0x04003900 RID: 14592
			public readonly Type Type;
		}

		// Token: 0x0200072C RID: 1836
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003901 RID: 14593
			[Nullable(0)]
			public static Func<ContentFile, ModProject.File> <0>__FromContentFile;
		}
	}
}
