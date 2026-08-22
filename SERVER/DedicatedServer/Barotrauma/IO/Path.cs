using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002DA RID: 730
	[NullableContext(1)]
	[Nullable(0)]
	public static class Path
	{
		// Token: 0x06003104 RID: 12548 RVA: 0x0014FFFF File Offset: 0x0014E1FF
		public static string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		// Token: 0x06003105 RID: 12549 RVA: 0x00150007 File Offset: 0x0014E207
		public static string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		// Token: 0x06003106 RID: 12550 RVA: 0x0015000F File Offset: 0x0014E20F
		[NullableContext(2)]
		public static string GetPathRoot(string path)
		{
			return Path.GetPathRoot(path);
		}

		// Token: 0x06003107 RID: 12551 RVA: 0x00150017 File Offset: 0x0014E217
		public static string GetRelativePath(string relativeTo, string path)
		{
			return Path.GetRelativePath(relativeTo, path);
		}

		// Token: 0x06003108 RID: 12552 RVA: 0x00150020 File Offset: 0x0014E220
		public static string GetDirectoryName(ContentPath path)
		{
			return Path.GetDirectoryName(path.Value);
		}

		// Token: 0x06003109 RID: 12553 RVA: 0x0015002D File Offset: 0x0014E22D
		[return: Nullable(2)]
		public static string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		// Token: 0x0600310A RID: 12554 RVA: 0x00150035 File Offset: 0x0014E235
		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		// Token: 0x0600310B RID: 12555 RVA: 0x0015003D File Offset: 0x0014E23D
		public static string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		// Token: 0x0600310C RID: 12556 RVA: 0x00150045 File Offset: 0x0014E245
		public static string Combine(params string[] s)
		{
			return Path.Combine(s);
		}

		// Token: 0x0600310D RID: 12557 RVA: 0x0015004D File Offset: 0x0014E24D
		public static string GetTempFileName()
		{
			return Path.GetTempFileName();
		}

		// Token: 0x0600310E RID: 12558 RVA: 0x00150054 File Offset: 0x0014E254
		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		// Token: 0x0600310F RID: 12559 RVA: 0x0015005C File Offset: 0x0014E25C
		public static ImmutableHashSet<char> GetInvalidFileNameCharsCrossPlatform()
		{
			return Path.invalidFileNameChars;
		}

		// Token: 0x04001854 RID: 6228
		public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;

		// Token: 0x04001855 RID: 6229
		public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

		// Token: 0x04001856 RID: 6230
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
