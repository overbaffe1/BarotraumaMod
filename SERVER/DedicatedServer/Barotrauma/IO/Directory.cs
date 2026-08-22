using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002DB RID: 731
	[NullableContext(1)]
	[Nullable(0)]
	public static class Directory
	{
		// Token: 0x06003111 RID: 12561 RVA: 0x00150095 File Offset: 0x0014E295
		public static string GetCurrentDirectory()
		{
			return Directory.GetCurrentDirectory();
		}

		// Token: 0x06003112 RID: 12562 RVA: 0x0015009C File Offset: 0x0014E29C
		public static void SetCurrentDirectory(string path)
		{
			Directory.SetCurrentDirectory(path);
		}

		// Token: 0x06003113 RID: 12563 RVA: 0x001500A4 File Offset: 0x0014E2A4
		private static EnumerationOptions GetEnumerationOptions(bool ignoreInaccessible, bool recursive)
		{
			return new EnumerationOptions
			{
				MatchType = MatchType.Win32,
				AttributesToSkip = (FileAttributes.Hidden | FileAttributes.System),
				IgnoreInaccessible = ignoreInaccessible,
				RecurseSubdirectories = recursive
			};
		}

		// Token: 0x06003114 RID: 12564 RVA: 0x001500C7 File Offset: 0x0014E2C7
		public static string[] GetFiles(string path)
		{
			return Directory.GetFiles(path, "*", Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06003115 RID: 12565 RVA: 0x001500DC File Offset: 0x0014E2DC
		public static string[] GetFiles(string path, string pattern, SearchOption option = SearchOption.AllDirectories)
		{
			EnumerationOptions enumerationOptions = Directory.GetEnumerationOptions(true, option == SearchOption.AllDirectories);
			return Directory.GetFiles(path, pattern, enumerationOptions);
		}

		// Token: 0x06003116 RID: 12566 RVA: 0x001500FC File Offset: 0x0014E2FC
		public static string[] GetDirectories(string path, string searchPattern = "*")
		{
			return Directory.GetDirectories(path, searchPattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06003117 RID: 12567 RVA: 0x0015010A File Offset: 0x0014E30A
		public static string[] GetFileSystemEntries(string path)
		{
			return Directory.GetFileSystemEntries(path, "*", Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06003118 RID: 12568 RVA: 0x0015011C File Offset: 0x0014E31C
		public static IEnumerable<string> EnumerateDirectories(string path, string pattern)
		{
			return Directory.EnumerateDirectories(path, pattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06003119 RID: 12569 RVA: 0x0015012A File Offset: 0x0014E32A
		public static IEnumerable<string> EnumerateFiles(string path, string pattern)
		{
			return Directory.EnumerateFiles(path, pattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x0600311A RID: 12570 RVA: 0x00150138 File Offset: 0x0014E338
		public static bool Exists(string path)
		{
			return Directory.Exists(path);
		}

		// Token: 0x0600311B RID: 12571 RVA: 0x00150140 File Offset: 0x0014E340
		[return: Nullable(2)]
		public static DirectoryInfo CreateDirectory(string path, bool catchUnauthorizedAccessExceptions = false)
		{
			if (!Validation.CanWrite(path, true))
			{
				DebugConsole.ThrowError("Cannot create directory \"" + path + "\": modifying the contents of this folder/using this extension is not allowed.", null, null, false, false);
				Validation.CanWrite(path, true);
				return null;
			}
			DirectoryInfo result;
			try
			{
				result = Directory.CreateDirectory(path);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot create directory at \"" + path + "\": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = null;
			}
			return result;
		}

		// Token: 0x0600311C RID: 12572 RVA: 0x001501B8 File Offset: 0x0014E3B8
		public static void Delete(string path, bool recursive = true, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(path, true))
			{
				DebugConsole.ThrowError("Cannot delete directory \"" + path + "\": modifying the contents of this folder/using this extension is not allowed.", null, null, false, false);
				return;
			}
			try
			{
				Directory.Delete(path, recursive);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot delete \"" + path + "\": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x0600311D RID: 12573 RVA: 0x00150224 File Offset: 0x0014E424
		public static bool TryDelete(string path, bool recursive = true)
		{
			bool result;
			try
			{
				Directory.Delete(path, recursive, false);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600311E RID: 12574 RVA: 0x00150254 File Offset: 0x0014E454
		public static DateTime GetLastWriteTime(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			DateTime result;
			try
			{
				result = Directory.GetLastWriteTime(path);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot get last write time at \"" + path + "\": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = default(DateTime);
			}
			return result;
		}

		// Token: 0x0600311F RID: 12575 RVA: 0x001502A8 File Offset: 0x0014E4A8
		public static void Copy(string src, string dest, bool overwrite = false)
		{
			if (!Validation.CanWrite(dest, true))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot copy \"");
				defaultInterpolatedStringHandler.AppendFormatted(src);
				defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler.AppendFormatted(dest);
				defaultInterpolatedStringHandler.AppendLiteral("\": modifying the contents of the destination folder is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			Directory.CreateDirectory(dest, false);
			foreach (string path in Directory.GetFiles(src))
			{
				File.Copy(path, Path.Combine(new string[]
				{
					dest,
					Path.GetRelativePath(src, path)
				}), overwrite, true);
			}
			foreach (string path2 in Directory.GetDirectories(src, "*"))
			{
				Directory.Copy(path2, Path.Combine(new string[]
				{
					dest,
					Path.GetRelativePath(src, path2)
				}), overwrite);
			}
		}

		// Token: 0x06003120 RID: 12576 RVA: 0x00150398 File Offset: 0x0014E598
		public static void Move(string src, string dest, bool overwrite = false)
		{
			if (!overwrite && Directory.Exists(dest))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler.AppendFormatted(src);
				defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler.AppendFormatted(dest);
				defaultInterpolatedStringHandler.AppendLiteral("\": destination folder already exists.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (!Validation.CanWrite(src, true))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(81, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler2.AppendFormatted(src);
				defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler2.AppendFormatted(dest);
				defaultInterpolatedStringHandler2.AppendLiteral("\": modifying the contents of the source folder is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (!Validation.CanWrite(dest, true))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler3.AppendFormatted(src);
				defaultInterpolatedStringHandler3.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler3.AppendFormatted(dest);
				defaultInterpolatedStringHandler3.AppendLiteral("\": modifying the contents of the destination folder is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (!overwrite || !Directory.Exists(dest) || Directory.TryDelete(dest, true))
			{
				Directory.Move(src, dest);
			}
		}

		// Token: 0x04001857 RID: 6231
		private static readonly EnumerationOptions IgnoreInaccessibleSystemAndHidden = new EnumerationOptions
		{
			MatchType = MatchType.Win32,
			AttributesToSkip = (FileAttributes.Hidden | FileAttributes.System),
			IgnoreInaccessible = true
		};
	}
}
