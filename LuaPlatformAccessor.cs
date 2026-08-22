using System;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;

namespace Barotrauma
{
	// Token: 0x02000300 RID: 768
	public class LuaPlatformAccessor : PlatformAccessorBase
	{
		// Token: 0x06003E61 RID: 15969 RVA: 0x002334A8 File Offset: 0x002316A8
		public static FileMode ParseFileMode(string mode)
		{
			mode = mode.Replace("b", "");
			if (mode == "r")
			{
				return FileMode.Open;
			}
			if (mode == "r+")
			{
				return FileMode.OpenOrCreate;
			}
			if (mode == "w")
			{
				return FileMode.Create;
			}
			if (mode == "w+")
			{
				return FileMode.Truncate;
			}
			return FileMode.Append;
		}

		// Token: 0x06003E62 RID: 15970 RVA: 0x00233504 File Offset: 0x00231704
		public static FileAccess ParseFileAccess(string mode)
		{
			mode = mode.Replace("b", "");
			if (mode == "r")
			{
				return FileAccess.Read;
			}
			if (mode == "r+")
			{
				return FileAccess.ReadWrite;
			}
			if (mode == "w")
			{
				return FileAccess.ReadWrite;
			}
			if (mode == "w+")
			{
				return FileAccess.ReadWrite;
			}
			return FileAccess.Write;
		}

		// Token: 0x06003E63 RID: 15971 RVA: 0x00233560 File Offset: 0x00231760
		public override string GetEnvironmentVariable(string envvarname)
		{
			return null;
		}

		// Token: 0x06003E64 RID: 15972 RVA: 0x00233563 File Offset: 0x00231763
		public override CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module;
		}

		// Token: 0x06003E65 RID: 15973 RVA: 0x00233568 File Offset: 0x00231768
		public override Stream IO_OpenFile(Script script, string filename, Encoding encoding, string mode)
		{
			if (!LuaCsFile.IsPathAllowedLuaException(filename, true))
			{
				return Stream.Null;
			}
			return new FileStream(filename, LuaPlatformAccessor.ParseFileMode(mode), LuaPlatformAccessor.ParseFileAccess(mode), FileShare.Read | FileShare.Write | FileShare.Delete);
		}

		// Token: 0x06003E66 RID: 15974 RVA: 0x0023359B File Offset: 0x0023179B
		public override Stream IO_GetStandardStream(StandardFileType type)
		{
			switch (type)
			{
			case StandardFileType.StdIn:
				return Console.OpenStandardInput();
			case StandardFileType.StdOut:
				return Console.OpenStandardOutput();
			case StandardFileType.StdErr:
				return Console.OpenStandardError();
			default:
				throw new ArgumentException("type");
			}
		}

		// Token: 0x06003E67 RID: 15975 RVA: 0x002335CD File Offset: 0x002317CD
		public override string IO_OS_GetTempFilename()
		{
			return "LocalMods/temp.txt";
		}

		// Token: 0x06003E68 RID: 15976 RVA: 0x002335D4 File Offset: 0x002317D4
		public override void OS_ExitFast(int exitCode)
		{
			throw new ScriptRuntimeException("usage of os.exit is not allowed.");
		}

		// Token: 0x06003E69 RID: 15977 RVA: 0x002335E0 File Offset: 0x002317E0
		public override bool OS_FileExists(string file)
		{
			return LuaCsFile.Exists(file);
		}

		// Token: 0x06003E6A RID: 15978 RVA: 0x002335E8 File Offset: 0x002317E8
		public override void OS_FileDelete(string file)
		{
			LuaCsFile.Delete(file);
		}

		// Token: 0x06003E6B RID: 15979 RVA: 0x002335F0 File Offset: 0x002317F0
		public override void OS_FileMove(string src, string dst)
		{
			LuaCsFile.Move(src, dst);
		}

		// Token: 0x06003E6C RID: 15980 RVA: 0x002335F9 File Offset: 0x002317F9
		public override int OS_Execute(string cmdline)
		{
			throw new ScriptRuntimeException("usage of os.execute is not allowed.");
		}

		// Token: 0x06003E6D RID: 15981 RVA: 0x00233605 File Offset: 0x00231805
		public override string GetPlatformNamePrefix()
		{
			return "lua";
		}

		// Token: 0x06003E6E RID: 15982 RVA: 0x0023360C File Offset: 0x0023180C
		public override void DefaultPrint(string content)
		{
		}
	}
}
