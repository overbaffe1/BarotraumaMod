using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A5 RID: 933
	[NullableContext(1)]
	[Nullable(0)]
	public static class Directory
	{
		// Token: 0x0600455D RID: 17757 RVA: 0x00268509 File Offset: 0x00266709
		public static string GetCurrentDirectory()
		{
			return Directory.GetCurrentDirectory();
		}

		// Token: 0x0600455E RID: 17758 RVA: 0x00268510 File Offset: 0x00266710
		public static void SetCurrentDirectory(string path)
		{
			Directory.SetCurrentDirectory(path);
		}

		// Token: 0x0600455F RID: 17759 RVA: 0x00268518 File Offset: 0x00266718
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

		// Token: 0x06004560 RID: 17760 RVA: 0x0026853B File Offset: 0x0026673B
		public static string[] GetFiles(string path)
		{
			return Directory.GetFiles(path, "*", Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06004561 RID: 17761 RVA: 0x00268550 File Offset: 0x00266750
		public static string[] GetFiles(string path, string pattern, SearchOption option = SearchOption.AllDirectories)
		{
			EnumerationOptions enumerationOptions = Directory.GetEnumerationOptions(true, option == SearchOption.AllDirectories);
			return Directory.GetFiles(path, pattern, enumerationOptions);
		}

		// Token: 0x06004562 RID: 17762 RVA: 0x00268570 File Offset: 0x00266770
		public static string[] GetDirectories(string path, string searchPattern = "*")
		{
			return Directory.GetDirectories(path, searchPattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06004563 RID: 17763 RVA: 0x0026857E File Offset: 0x0026677E
		public static string[] GetFileSystemEntries(string path)
		{
			return Directory.GetFileSystemEntries(path, "*", Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06004564 RID: 17764 RVA: 0x00268590 File Offset: 0x00266790
		public static IEnumerable<string> EnumerateDirectories(string path, string pattern)
		{
			return Directory.EnumerateDirectories(path, pattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06004565 RID: 17765 RVA: 0x0026859E File Offset: 0x0026679E
		public static IEnumerable<string> EnumerateFiles(string path, string pattern)
		{
			return Directory.EnumerateFiles(path, pattern, Directory.IgnoreInaccessibleSystemAndHidden);
		}

		// Token: 0x06004566 RID: 17766 RVA: 0x002685AC File Offset: 0x002667AC
		public static bool Exists(string path)
		{
			return Directory.Exists(path);
		}

		// Token: 0x06004567 RID: 17767 RVA: 0x002685B4 File Offset: 0x002667B4
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

		// Token: 0x06004568 RID: 17768 RVA: 0x0026862C File Offset: 0x0026682C
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

		// Token: 0x06004569 RID: 17769 RVA: 0x00268698 File Offset: 0x00266898
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

		// Token: 0x0600456A RID: 17770 RVA: 0x002686C8 File Offset: 0x002668C8
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

		// Token: 0x0600456B RID: 17771 RVA: 0x0026871C File Offset: 0x0026691C
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

		// Token: 0x0600456C RID: 17772 RVA: 0x0026880C File Offset: 0x00266A0C
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

		// Token: 0x04002425 RID: 9253
		private static readonly EnumerationOptions IgnoreInaccessibleSystemAndHidden = new EnumerationOptions
		{
			MatchType = MatchType.Win32,
			AttributesToSkip = (FileAttributes.Hidden | FileAttributes.System),
			IgnoreInaccessible = true
		};
	}
}
