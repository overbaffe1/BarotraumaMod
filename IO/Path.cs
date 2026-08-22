using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A4 RID: 932
	[NullableContext(1)]
	[Nullable(0)]
	public static class Path
	{
		// Token: 0x06004550 RID: 17744 RVA: 0x00268473 File Offset: 0x00266673
		public static string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		// Token: 0x06004551 RID: 17745 RVA: 0x0026847B File Offset: 0x0026667B
		public static string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		// Token: 0x06004552 RID: 17746 RVA: 0x00268483 File Offset: 0x00266683
		[NullableContext(2)]
		public static string GetPathRoot(string path)
		{
			return Path.GetPathRoot(path);
		}

		// Token: 0x06004553 RID: 17747 RVA: 0x0026848B File Offset: 0x0026668B
		public static string GetRelativePath(string relativeTo, string path)
		{
			return Path.GetRelativePath(relativeTo, path);
		}

		// Token: 0x06004554 RID: 17748 RVA: 0x00268494 File Offset: 0x00266694
		public static string GetDirectoryName(ContentPath path)
		{
			return Path.GetDirectoryName(path.Value);
		}

		// Token: 0x06004555 RID: 17749 RVA: 0x002684A1 File Offset: 0x002666A1
		[return: Nullable(2)]
		public static string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		// Token: 0x06004556 RID: 17750 RVA: 0x002684A9 File Offset: 0x002666A9
		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		// Token: 0x06004557 RID: 17751 RVA: 0x002684B1 File Offset: 0x002666B1
		public static string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		// Token: 0x06004558 RID: 17752 RVA: 0x002684B9 File Offset: 0x002666B9
		public static string Combine(params string[] s)
		{
			return Path.Combine(s);
		}

		// Token: 0x06004559 RID: 17753 RVA: 0x002684C1 File Offset: 0x002666C1
		public static string GetTempFileName()
		{
			return Path.GetTempFileName();
		}

		// Token: 0x0600455A RID: 17754 RVA: 0x002684C8 File Offset: 0x002666C8
		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		// Token: 0x0600455B RID: 17755 RVA: 0x002684D0 File Offset: 0x002666D0
		public static ImmutableHashSet<char> GetInvalidFileNameCharsCrossPlatform()
		{
			return Path.invalidFileNameChars;
		}

		// Token: 0x04002422 RID: 9250
		public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;

		// Token: 0x04002423 RID: 9251
		public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

		// Token: 0x04002424 RID: 9252
		private static readonly ImmutableHashSet<char> invalidFileNameChars = ImmutableHashSet.Create<char>(new char[]
		{
			'"',
			'<',
			'>',
			'|',
			'\0',
			'\u0001',
			'\u0002',
			'\u0003',
			'\u0004',
			'\u0005',
			'\u0006',
			'\a',
			'\b',
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			'\u000e',
			'\u000f',
			'\u0010',
			'\u0011',
			'\u0012',
			'\u0013',
			'\u0014',
			'\u0015',
			'\u0016',
			'\u0017',
			'\u0018',
			'\u0019',
			'\u001a',
			'\u001b',
			'\u001c',
			'\u001d',
			'\u001e',
			'\u001f',
			':',
			'*',
			'?',
			'\\',
			'/'
		});
	}
}
