using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A0 RID: 928
	internal static class Validation
	{
		// Token: 0x06004541 RID: 17729 RVA: 0x002680B4 File Offset: 0x002662B4
		public static Validation.Skipper SkipInDebugBuilds()
		{
			Validation.SkipValidationInDebugBuilds = true;
			return default(Validation.Skipper);
		}

		// Token: 0x06004542 RID: 17730 RVA: 0x002680D0 File Offset: 0x002662D0
		[NullableContext(1)]
		public static bool CanWrite(string path, bool isDirectory)
		{
			Validation.<>c__DisplayClass5_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = Validation.<CanWrite>g__getFullPath|5_0(CS$<>8__locals1.path);
			string localModsDir = Validation.<CanWrite>g__getFullPath|5_0("LocalMods");
			string workshopModsDir = Validation.<CanWrite>g__getFullPath|5_0(ContentPackage.WorkshopModsDir);
			string workshopStagingDir = Validation.<CanWrite>g__getFullPath|5_0("WorkshopStaging");
			string tempDownloadDir = Validation.<CanWrite>g__getFullPath|5_0("TempMods_Download");
			if (!isDirectory)
			{
				Identifier extension = Path.GetExtension(CS$<>8__locals1.path).Replace(" ", "").ToIdentifier();
				if (!Validation.<CanWrite>g__pathStartsWith|5_1(workshopModsDir, ref CS$<>8__locals1) && !Validation.<CanWrite>g__pathStartsWith|5_1(localModsDir, ref CS$<>8__locals1) && !Validation.<CanWrite>g__pathStartsWith|5_1(tempDownloadDir, ref CS$<>8__locals1) && !Validation.<CanWrite>g__pathStartsWith|5_1(workshopStagingDir, ref CS$<>8__locals1) && Validation.unwritableExtensions.Any((Identifier e) => e == extension))
				{
					return false;
				}
			}
			foreach (Identifier unwritableDir in Validation.unwritableDirs)
			{
				string dir = Path.GetFullPath(unwritableDir.Value).CleanUpPath();
				if (CS$<>8__locals1.path.StartsWith(dir, StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06004544 RID: 17732 RVA: 0x00268291 File Offset: 0x00266491
		[NullableContext(1)]
		[CompilerGenerated]
		internal static string <CanWrite>g__getFullPath|5_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x06004545 RID: 17733 RVA: 0x0026829E File Offset: 0x0026649E
		[NullableContext(1)]
		[CompilerGenerated]
		internal static bool <CanWrite>g__pathStartsWith|5_1(string prefix, ref Validation.<>c__DisplayClass5_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0400241E RID: 9246
		private static readonly ImmutableArray<Identifier> unwritableDirs = new Identifier[]
		{
			"Content".ToIdentifier()
		}.ToImmutableArray<Identifier>();

		// Token: 0x0400241F RID: 9247
		private static readonly ImmutableArray<Identifier> unwritableExtensions = new string[]
		{
			".exe",
			".dll",
			".json",
			".pdb",
			".com",
			".scr",
			".dylib",
			".so",
			".a",
			".app",
			".bat",
			".sh"
		}.ToIdentifiers().ToImmutableArray<Identifier>();

		// Token: 0x04002420 RID: 9248
		public static bool SkipValidationInDebugBuilds;

		// Token: 0x020010C6 RID: 4294
		[CompilerFeatureRequired("RefStructs")]
		public ref struct Skipper
		{
			// Token: 0x06008DC7 RID: 36295 RVA: 0x003B2B12 File Offset: 0x003B0D12
			public void Dispose()
			{
				Validation.SkipValidationInDebugBuilds = false;
			}
		}
	}
}
