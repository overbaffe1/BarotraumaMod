using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.IO
{
	// Token: 0x020002DC RID: 732
	[NullableContext(1)]
	[Nullable(0)]
	public static class File
	{
		// Token: 0x06003122 RID: 12578 RVA: 0x001504EB File Offset: 0x0014E6EB
		public static bool Exists(ContentPath path)
		{
			return File.Exists(path.Value);
		}

		// Token: 0x06003123 RID: 12579 RVA: 0x001504F8 File Offset: 0x0014E6F8
		public static bool Exists(string path)
		{
			return File.Exists(path);
		}

		// Token: 0x06003124 RID: 12580 RVA: 0x00150500 File Offset: 0x0014E700
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

		// Token: 0x06003125 RID: 12581 RVA: 0x001505D8 File Offset: 0x0014E7D8
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

		// Token: 0x06003126 RID: 12582 RVA: 0x00150708 File Offset: 0x0014E908
		public static void Delete(ContentPath path, bool catchUnauthorizedAccessExceptions = true)
		{
			File.Delete(path.Value, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x06003127 RID: 12583 RVA: 0x00150718 File Offset: 0x0014E918
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

		// Token: 0x06003128 RID: 12584 RVA: 0x00150784 File Offset: 0x0014E984
		public static DateTime GetLastWriteTime(string path)
		{
			return File.GetLastWriteTime(path);
		}

		// Token: 0x06003129 RID: 12585 RVA: 0x0015078C File Offset: 0x0014E98C
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

		// Token: 0x0600312A RID: 12586 RVA: 0x00150874 File Offset: 0x0014EA74
		[return: Nullable(2)]
		public static FileStream OpenRead(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.Open, FileAccess.Read, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x0600312B RID: 12587 RVA: 0x00150898 File Offset: 0x0014EA98
		[return: Nullable(2)]
		public static FileStream OpenWrite(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x0600312C RID: 12588 RVA: 0x001508BC File Offset: 0x0014EABC
		[return: Nullable(2)]
		public static FileStream Create(string path, bool catchUnauthorizedAccessExceptions = true)
		{
			return File.Open(path, FileMode.Create, FileAccess.Write, null, catchUnauthorizedAccessExceptions);
		}

		// Token: 0x0600312D RID: 12589 RVA: 0x001508E0 File Offset: 0x0014EAE0
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

		// Token: 0x0600312E RID: 12590 RVA: 0x0015094C File Offset: 0x0014EB4C
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

		// Token: 0x0600312F RID: 12591 RVA: 0x001509C4 File Offset: 0x0014EBC4
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

		// Token: 0x06003130 RID: 12592 RVA: 0x00150A3C File Offset: 0x0014EC3C
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

		// Token: 0x06003131 RID: 12593 RVA: 0x00150A8C File Offset: 0x0014EC8C
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

		// Token: 0x06003132 RID: 12594 RVA: 0x00150AE4 File Offset: 0x0014ECE4
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

		// Token: 0x06003133 RID: 12595 RVA: 0x00150B3C File Offset: 0x0014ED3C
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
