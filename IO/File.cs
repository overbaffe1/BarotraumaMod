using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.IO
{
	// Token: 0x020003A6 RID: 934
	[NullableContext(1)]
	[Nullable(0)]
	public static class File
	{
		// Token: 0x0600456E RID: 17774 RVA: 0x0026895F File Offset: 0x00266B5F
		public static bool Exists(ContentPath path)
		{
			return File.Exists(path.Value);
		}

		// Token: 0x0600456F RID: 17775 RVA: 0x0026896C File Offset: 0x00266B6C
		public static bool Exists(string path)
		{
			return File.Exists(path);
		}

		// Token: 0x06004570 RID: 17776 RVA: 0x00268974 File Offset: 0x00266B74
		public static void Copy(string src, string dest, bool overwrite = false, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(dest, false))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(96, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot copy \"");
				defaultInterpolatedStringHandler.AppendFormatted(src);
				defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler.AppendFormatted(dest);
				defaultInterpolatedStringHandler.AppendLiteral("\": modifying the contents of this folder/using this extension is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			try
			{
				File.Copy(src, dest, overwrite);
			}
			catch (UnauthorizedAccessException e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Cannot copy \"");
				defaultInterpolatedStringHandler2.AppendFormatted(src);
				defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler2.AppendFormatted(dest);
				defaultInterpolatedStringHandler2.AppendLiteral("\": unauthorized access. The file/folder might be read-only!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x06004571 RID: 17777 RVA: 0x00268A4C File Offset: 0x00266C4C
		public static void Move(string src, string dest, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(src, false))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler.AppendFormatted(src);
				defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler.AppendFormatted(dest);
				defaultInterpolatedStringHandler.AppendLiteral("\": modifying the contents of the source folder is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (!Validation.CanWrite(dest, false))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(85, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler2.AppendFormatted(src);
				defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler2.AppendFormatted(dest);
				defaultInterpolatedStringHandler2.AppendLiteral("\": modifying the contents of the destination folder is not allowed");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return;
			}
			try
			{
				File.Move(src, dest);
			}
			catch (UnauthorizedAccessException e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler3.AppendLiteral("Cannot move \"");
				defaultInterpolatedStringHandler3.AppendFormatted(src);
				defaultInterpolatedStringHandler3.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler3.AppendFormatted(dest);
				defaultInterpolatedStringHandler3.AppendLiteral("\": unauthorized access. The file/folder might be read-only!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x06004572 RID: 17778 RVA: 0x00268B7C File Offset: 0x00266D7C
		public static void Delete(ContentPath path, bool catchUnauthorizedAccessExceptions = true)
		{
			File.Delete(path.Value, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x06004573 RID: 17779 RVA: 0x00268B8C File Offset: 0x00266D8C
		public static void Delete(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(path, false))
			{
				DebugConsole.ThrowError("Cannot delete file \"" + path + "\": modifying the contents of this folder/using this extension is not allowed.", null, null, false, false);
				return;
			}
			try
			{
				File.Delete(path);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot delete " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x06004574 RID: 17780 RVA: 0x00268BF8 File Offset: 0x00266DF8
		public static DateTime GetLastWriteTime(string path)
		{
			return File.GetLastWriteTime(path);
		}

		// Token: 0x06004575 RID: 17781 RVA: 0x00268C00 File Offset: 0x00266E00
		[return: Nullable(2)]
		public static FileStream Open(string path, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare? share = null, bool catchUnauthorizedAccessExceptions = true)
		{
			if ((mode - FileMode.CreateNew <= 1 || mode - FileMode.OpenOrCreate <= 2) && !Validation.CanWrite(path, false))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot open \"");
				defaultInterpolatedStringHandler.AppendFormatted(path);
				defaultInterpolatedStringHandler.AppendLiteral("\" in ");
				defaultInterpolatedStringHandler.AppendFormatted<FileMode>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(" mode: modifying the contents of this folder/using this extension is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return null;
			}
			access = ((!Validation.CanWrite(path, false)) ? FileAccess.Read : access);
			FileShare shareVal = share ?? ((access == FileAccess.Read) ? FileShare.Read : FileShare.None);
			FileStream result;
			try
			{
				result = new FileStream(path, File.Open(path, mode, access, shareVal));
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot open " + path + " (stream): unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = null;
			}
			return result;
		}

		// Token: 0x06004576 RID: 17782 RVA: 0x00268CE8 File Offset: 0x00266EE8
		[return: Nullable(2)]
		public static FileStream OpenRead(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.Open, FileAccess.Read, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x06004577 RID: 17783 RVA: 0x00268D0C File Offset: 0x00266F0C
		[return: Nullable(2)]
		public static FileStream OpenWrite(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x06004578 RID: 17784 RVA: 0x00268D30 File Offset: 0x00266F30
		[return: Nullable(2)]
		public static FileStream Create(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.Create, FileAccess.Write, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x06004579 RID: 17785 RVA: 0x00268D54 File Offset: 0x00266F54
		public static void WriteAllBytes(string path, byte[] contents, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(path, false))
			{
				DebugConsole.ThrowError("Cannot write all bytes to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				return;
			}
			try
			{
				File.WriteAllBytes(path, contents);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot write at " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x0600457A RID: 17786 RVA: 0x00268DC0 File Offset: 0x00266FC0
		public static void WriteAllText(string path, string contents, [Nullable(2)] Encoding encoding = null, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(path, false))
			{
				DebugConsole.ThrowError("Cannot write all text to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				return;
			}
			try
			{
				File.WriteAllText(path, contents, encoding ?? Encoding.UTF8);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot write at " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x0600457B RID: 17787 RVA: 0x00268E38 File Offset: 0x00267038
		public static void WriteAllLines(string path, IEnumerable<string> contents, [Nullable(2)] Encoding encoding = null, bool catchUnauthorizedAccessExceptions = true)
		{
			if (!Validation.CanWrite(path, false))
			{
				DebugConsole.ThrowError("Cannot write all lines to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				return;
			}
			try
			{
				File.WriteAllLines(path, contents, encoding ?? Encoding.UTF8);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot write at " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
			}
		}

		// Token: 0x0600457C RID: 17788 RVA: 0x00268EB0 File Offset: 0x002670B0
		public static byte[] ReadAllBytes(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			byte[] result;
			try
			{
				result = File.ReadAllBytes(path);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot read " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = Array.Empty<byte>();
			}
			return result;
		}

		// Token: 0x0600457D RID: 17789 RVA: 0x00268F00 File Offset: 0x00267100
		public static string ReadAllText(string path, [Nullable(2)] Encoding encoding = null, bool catchUnauthorizedAccessExceptions = true)
		{
			string result;
			try
			{
				result = File.ReadAllText(path, encoding ?? Encoding.UTF8);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot read " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600457E RID: 17790 RVA: 0x00268F58 File Offset: 0x00267158
		public static string[] ReadAllLines(string path, [Nullable(2)] Encoding encoding = null, bool catchUnauthorizedAccessExceptions = true)
		{
			string[] result;
			try
			{
				result = File.ReadAllLines(path, encoding ?? Encoding.UTF8);
			}
			catch (UnauthorizedAccessException e)
			{
				DebugConsole.ThrowError("Cannot read " + path + ": unauthorized access. The file/folder might be read-only!", e, null, false, false);
				if (!catchUnauthorizedAccessExceptions)
				{
					throw;
				}
				result = Array.Empty<string>();
			}
			return result;
		}

		// Token: 0x0600457F RID: 17791 RVA: 0x00268FB0 File Offset: 0x002671B0
		public static string SanitizeName(string str)
		{
			string sanitized = "";
			foreach (char c in str)
			{
				char newChar = Path.GetInvalidFileNameCharsCrossPlatform().Contains(c) ? '-' : c;
				ReadOnlySpan<char> str2 = sanitized;
				char c2 = newChar;
				sanitized = str2 + new ReadOnlySpan<char>(ref c2);
			}
			return sanitized;
		}
	}
}
