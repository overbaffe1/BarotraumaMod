using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;

namespace Barotrauma
{
	// Token: 0x02000217 RID: 535
	internal class LuaCsFile
	{
		// Token: 0x06002563 RID: 9571 RVA: 0x000F572C File Offset: 0x000F392C
		public static bool CanReadFromPath(string path)
		{
			LuaCsFile.<>c__DisplayClass0_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(CS$<>8__locals1.path);
			string localModsDir = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0("LocalMods");
			string workshopModsDir = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(ContentPackage.WorkshopModsDir);
			return LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath), ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(localModsDir, ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(workshopModsDir, ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(LuaCsFile.<CanReadFromPath>g__getFullPath|0_0("."), ref CS$<>8__locals1);
		}

		// Token: 0x06002564 RID: 9572 RVA: 0x000F57CC File Offset: 0x000F39CC
		public static bool CanWriteToPath(string path)
		{
			LuaCsFile.<>c__DisplayClass1_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(CS$<>8__locals1.path);
			return !LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(LuaCsSetup.GetLuaCsPackage().Path), ref CS$<>8__locals1) && (LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath), ref CS$<>8__locals1) || LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0("LocalMods"), ref CS$<>8__locals1) || LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(ContentPackage.WorkshopModsDir), ref CS$<>8__locals1));
		}

		// Token: 0x06002565 RID: 9573 RVA: 0x000F586C File Offset: 0x000F3A6C
		public static bool IsPathAllowedException(string path, bool write = true, LuaCsMessageOrigin origin = LuaCsMessageOrigin.Unknown)
		{
			if (write)
			{
				if (LuaCsFile.CanWriteToPath(path))
				{
					return true;
				}
				throw new Exception("File access to \"" + path + "\" not allowed.");
			}
			else
			{
				if (LuaCsFile.CanReadFromPath(path))
				{
					return true;
				}
				throw new Exception("File access to \"" + path + "\" not allowed.");
			}
		}

		// Token: 0x06002566 RID: 9574 RVA: 0x000F58BB File Offset: 0x000F3ABB
		public static bool IsPathAllowedLuaException(string path, bool write = true)
		{
			return LuaCsFile.IsPathAllowedException(path, write, LuaCsMessageOrigin.LuaMod);
		}

		// Token: 0x06002567 RID: 9575 RVA: 0x000F58C5 File Offset: 0x000F3AC5
		public static bool IsPathAllowedCsException(string path, bool write = true)
		{
			return LuaCsFile.IsPathAllowedException(path, write, LuaCsMessageOrigin.CSharpMod);
		}

		// Token: 0x06002568 RID: 9576 RVA: 0x000F58CF File Offset: 0x000F3ACF
		public static string Read(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return "";
			}
			return File.ReadAllText(path);
		}

		// Token: 0x06002569 RID: 9577 RVA: 0x000F58E7 File Offset: 0x000F3AE7
		public static void Write(string path, string text)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			File.WriteAllText(path, text);
		}

		// Token: 0x0600256A RID: 9578 RVA: 0x000F58FB File Offset: 0x000F3AFB
		public static void Delete(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			File.Delete(path);
		}

		// Token: 0x0600256B RID: 9579 RVA: 0x000F590E File Offset: 0x000F3B0E
		public static void DeleteDirectory(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			Directory.Delete(path, true);
		}

		// Token: 0x0600256C RID: 9580 RVA: 0x000F5922 File Offset: 0x000F3B22
		public static void Move(string path, string destination)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			if (!LuaCsFile.IsPathAllowedException(destination, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			File.Move(path, destination, true);
		}

		// Token: 0x0600256D RID: 9581 RVA: 0x000F5942 File Offset: 0x000F3B42
		public static FileStream OpenRead(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return null;
			}
			return File.Open(path, FileMode.Open, FileAccess.Read);
		}

		// Token: 0x0600256E RID: 9582 RVA: 0x000F5958 File Offset: 0x000F3B58
		public static FileStream OpenWrite(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return null;
			}
			if (File.Exists(path))
			{
				return File.Open(path, FileMode.Truncate, FileAccess.Write);
			}
			return File.Open(path, FileMode.Create, FileAccess.Write);
		}

		// Token: 0x0600256F RID: 9583 RVA: 0x000F597F File Offset: 0x000F3B7F
		public static bool Exists(string path)
		{
			return LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown) && File.Exists(path);
		}

		// Token: 0x06002570 RID: 9584 RVA: 0x000F5993 File Offset: 0x000F3B93
		public static bool CreateDirectory(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return false;
			}
			Directory.CreateDirectory(path);
			return true;
		}

		// Token: 0x06002571 RID: 9585 RVA: 0x000F59A9 File Offset: 0x000F3BA9
		public static bool DirectoryExists(string path)
		{
			return LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown) && Directory.Exists(path);
		}

		// Token: 0x06002572 RID: 9586 RVA: 0x000F59BD File Offset: 0x000F3BBD
		public static string[] GetFiles(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return null;
			}
			return Directory.GetFiles(path);
		}

		// Token: 0x06002573 RID: 9587 RVA: 0x000F59D1 File Offset: 0x000F3BD1
		public static string[] GetDirectories(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return new string[0];
			}
			return Directory.GetDirectories(path);
		}

		// Token: 0x06002574 RID: 9588 RVA: 0x000F59EC File Offset: 0x000F3BEC
		public static string[] DirSearch(string sDir)
		{
			if (!LuaCsFile.IsPathAllowedException(sDir, false, LuaCsMessageOrigin.Unknown))
			{
				return new string[0];
			}
			List<string> files = new List<string>();
			try
			{
				foreach (string f in Directory.GetFiles(sDir))
				{
					files.Add(f);
				}
				foreach (string d in Directory.GetDirectories(sDir))
				{
					foreach (string f2 in Directory.GetFiles(d))
					{
						files.Add(f2);
					}
					LuaCsFile.DirSearch(d);
				}
			}
			catch (Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}
			return files.ToArray();
		}

		// Token: 0x06002576 RID: 9590 RVA: 0x000F5AB8 File Offset: 0x000F3CB8
		[CompilerGenerated]
		internal static string <CanReadFromPath>g__getFullPath|0_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x06002577 RID: 9591 RVA: 0x000F5AC5 File Offset: 0x000F3CC5
		[CompilerGenerated]
		internal static bool <CanReadFromPath>g__pathStartsWith|0_1(string prefix, ref LuaCsFile.<>c__DisplayClass0_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002578 RID: 9592 RVA: 0x000F5AD4 File Offset: 0x000F3CD4
		[CompilerGenerated]
		internal static string <CanWriteToPath>g__getFullPath|1_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x06002579 RID: 9593 RVA: 0x000F5AE1 File Offset: 0x000F3CE1
		[CompilerGenerated]
		internal static bool <CanWriteToPath>g__pathStartsWith|1_1(string prefix, ref LuaCsFile.<>c__DisplayClass1_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}
	}
}
