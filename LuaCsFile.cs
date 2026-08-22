using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;

namespace Barotrauma
{
	// Token: 0x020002FF RID: 767
	internal class LuaCsFile
	{
		// Token: 0x06003E4A RID: 15946 RVA: 0x002330BC File Offset: 0x002312BC
		public static bool CanReadFromPath(string path)
		{
			LuaCsFile.<>c__DisplayClass0_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(CS$<>8__locals1.path);
			string localModsDir = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0("LocalMods");
			string workshopModsDir = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(ContentPackage.WorkshopModsDir);
			string tempDownloadDir = LuaCsFile.<CanReadFromPath>g__getFullPath|0_0("TempMods_Download");
			return LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(LuaCsFile.<CanReadFromPath>g__getFullPath|0_0(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath), ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(localModsDir, ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(workshopModsDir, ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(tempDownloadDir, ref CS$<>8__locals1) || LuaCsFile.<CanReadFromPath>g__pathStartsWith|0_1(LuaCsFile.<CanReadFromPath>g__getFullPath|0_0("."), ref CS$<>8__locals1);
		}

		// Token: 0x06003E4B RID: 15947 RVA: 0x00233170 File Offset: 0x00231370
		public static bool CanWriteToPath(string path)
		{
			LuaCsFile.<>c__DisplayClass1_0 CS$<>8__locals1;
			CS$<>8__locals1.path = path;
			CS$<>8__locals1.path = LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(CS$<>8__locals1.path);
			return !LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(LuaCsSetup.GetLuaCsPackage().Path), ref CS$<>8__locals1) && (LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(string.IsNullOrEmpty(GameSettings.CurrentConfig.SavePath) ? SaveUtil.DefaultSaveFolder : GameSettings.CurrentConfig.SavePath), ref CS$<>8__locals1) || LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0("LocalMods"), ref CS$<>8__locals1) || LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0(ContentPackage.WorkshopModsDir), ref CS$<>8__locals1) || LuaCsFile.<CanWriteToPath>g__pathStartsWith|1_1(LuaCsFile.<CanWriteToPath>g__getFullPath|1_0("TempMods_Download"), ref CS$<>8__locals1));
		}

		// Token: 0x06003E4C RID: 15948 RVA: 0x00233224 File Offset: 0x00231424
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

		// Token: 0x06003E4D RID: 15949 RVA: 0x00233273 File Offset: 0x00231473
		public static bool IsPathAllowedLuaException(string path, bool write = true)
		{
			return LuaCsFile.IsPathAllowedException(path, write, LuaCsMessageOrigin.LuaMod);
		}

		// Token: 0x06003E4E RID: 15950 RVA: 0x0023327D File Offset: 0x0023147D
		public static bool IsPathAllowedCsException(string path, bool write = true)
		{
			return LuaCsFile.IsPathAllowedException(path, write, LuaCsMessageOrigin.CSharpMod);
		}

		// Token: 0x06003E4F RID: 15951 RVA: 0x00233287 File Offset: 0x00231487
		public static string Read(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return "";
			}
			return File.ReadAllText(path);
		}

		// Token: 0x06003E50 RID: 15952 RVA: 0x0023329F File Offset: 0x0023149F
		public static void Write(string path, string text)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			File.WriteAllText(path, text);
		}

		// Token: 0x06003E51 RID: 15953 RVA: 0x002332B3 File Offset: 0x002314B3
		public static void Delete(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			File.Delete(path);
		}

		// Token: 0x06003E52 RID: 15954 RVA: 0x002332C6 File Offset: 0x002314C6
		public static void DeleteDirectory(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return;
			}
			Directory.Delete(path, true);
		}

		// Token: 0x06003E53 RID: 15955 RVA: 0x002332DA File Offset: 0x002314DA
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

		// Token: 0x06003E54 RID: 15956 RVA: 0x002332FA File Offset: 0x002314FA
		public static FileStream OpenRead(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return null;
			}
			return File.Open(path, FileMode.Open, FileAccess.Read);
		}

		// Token: 0x06003E55 RID: 15957 RVA: 0x00233310 File Offset: 0x00231510
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

		// Token: 0x06003E56 RID: 15958 RVA: 0x00233337 File Offset: 0x00231537
		public static bool Exists(string path)
		{
			return LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown) && File.Exists(path);
		}

		// Token: 0x06003E57 RID: 15959 RVA: 0x0023334B File Offset: 0x0023154B
		public static bool CreateDirectory(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, true, LuaCsMessageOrigin.Unknown))
			{
				return false;
			}
			Directory.CreateDirectory(path);
			return true;
		}

		// Token: 0x06003E58 RID: 15960 RVA: 0x00233361 File Offset: 0x00231561
		public static bool DirectoryExists(string path)
		{
			return LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown) && Directory.Exists(path);
		}

		// Token: 0x06003E59 RID: 15961 RVA: 0x00233375 File Offset: 0x00231575
		public static string[] GetFiles(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return null;
			}
			return Directory.GetFiles(path);
		}

		// Token: 0x06003E5A RID: 15962 RVA: 0x00233389 File Offset: 0x00231589
		public static string[] GetDirectories(string path)
		{
			if (!LuaCsFile.IsPathAllowedException(path, false, LuaCsMessageOrigin.Unknown))
			{
				return new string[0];
			}
			return Directory.GetDirectories(path);
		}

		// Token: 0x06003E5B RID: 15963 RVA: 0x002333A4 File Offset: 0x002315A4
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

		// Token: 0x06003E5D RID: 15965 RVA: 0x00233470 File Offset: 0x00231670
		[CompilerGenerated]
		internal static string <CanReadFromPath>g__getFullPath|0_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x06003E5E RID: 15966 RVA: 0x0023347D File Offset: 0x0023167D
		[CompilerGenerated]
		internal static bool <CanReadFromPath>g__pathStartsWith|0_1(string prefix, ref LuaCsFile.<>c__DisplayClass0_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06003E5F RID: 15967 RVA: 0x0023348C File Offset: 0x0023168C
		[CompilerGenerated]
		internal static string <CanWriteToPath>g__getFullPath|1_0(string p)
		{
			return Path.GetFullPath(p).CleanUpPath();
		}

		// Token: 0x06003E60 RID: 15968 RVA: 0x00233499 File Offset: 0x00231699
		[CompilerGenerated]
		internal static bool <CanWriteToPath>g__pathStartsWith|1_1(string prefix, ref LuaCsFile.<>c__DisplayClass1_0 A_1)
		{
			return A_1.path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}
	}
}
