using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002D6 RID: 726
	internal static class Validation
	{
		// Token: 0x060030F5 RID: 12533 RVA: 0x0014FC70 File Offset: 0x0014DE70
		public static Validation.Skipper SkipInDebugBuilds()
		{
			Validation.SkipValidationInDebugBuilds = true;
			return default(Validation.Skipper);
		}

		// Token: 0x060030F6 RID: 12534 RVA: 0x0014FC8C File Offset: 0x0014DE8C
		[NullableContext(1)]
		public static bool CanWrite(string path, bool isDirectory)
		{
			Validation.<>c__DisplayClass5_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = Validation.<CanWrite>g__getFullPath|5_0(CS$<>8__locals1.path);
			string localModsDir = Validation.<CanWrite>g__getFullPath|5_0("LocalMods");
			string workshopModsDir = Validation.<CanWrite>g__getFullPath|5_0(ContentPackage.WorkshopModsDir);
			if (!isDirectory)
			{
				Identifier extension = Path.GetExtension(CS$<>8__locals1.path).Replace(" ", "").ToIdentifier();
				if (!Validation.<CanWrite>g__pathStartsWith|5_1(workshopModsDir, ref CS$<>8__locals1) && !Validation.<CanWrite>g__pathStartsWith|5_1(localModsDir, ref CS$<>8__locals1) && Validation.unwritableExtensions.Any((Identifier e) => e == extension))
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

		// Token: 0x060030F8 RID: 12536 RVA: 0x0014FE1D File Offset: 0x0014E01D
		[NullableContext(1)]
		[CompilerGenerated]
		internal static string <CanWrite>g__getFullPath|5_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x060030F9 RID: 12537 RVA: 0x0014FE2A File Offset: 0x0014E02A
		[NullableContext(1)]
		[CompilerGenerated]
		internal static bool <CanWrite>g__pathStartsWith|5_1(string prefix, ref Validation.<>c__DisplayClass5_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04001850 RID: 6224
		private static readonly ImmutableArray<Identifier> unwritableDirs = new Identifier[]
		{
			"Content".ToIdentifier()
		}.ToImmutableArray<Identifier>();

		// Token: 0x04001851 RID: 6225
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

		// Token: 0x04001852 RID: 6226
		public static bool SkipValidationInDebugBuilds;

		// Token: 0x02000B7A RID: 2938
		[CompilerFeatureRequired("RefStructs")]
		public ref struct Skipper
		{
			// Token: 0x060060ED RID: 24813 RVA: 0x0020B289 File Offset: 0x00209489
			public void Dispose()
			{
				Validation.SkipValidationInDebugBuilds = false;
			}
		}
	}
}
