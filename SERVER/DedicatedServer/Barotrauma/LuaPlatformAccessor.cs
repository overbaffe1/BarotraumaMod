using System;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;

namespace Barotrauma
{
	// Token: 0x02000218 RID: 536
	public class LuaPlatformAccessor : PlatformAccessorBase
	{
		// Token: 0x0600257A RID: 9594 RVA: 0x000F5AF0 File Offset: 0x000F3CF0
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

		// Token: 0x0600257B RID: 9595 RVA: 0x000F5B4C File Offset: 0x000F3D4C
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

		// Token: 0x0600257C RID: 9596 RVA: 0x000F5BA8 File Offset: 0x000F3DA8
		public override string GetEnvironmentVariable(string envvarname)
		{
			return null;
		}

		// Token: 0x0600257D RID: 9597 RVA: 0x000F5BAB File Offset: 0x000F3DAB
		public override CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module;
		}

		// Token: 0x0600257E RID: 9598 RVA: 0x000F5BB0 File Offset: 0x000F3DB0
		public override Stream IO_OpenFile(Script script, string filename, Encoding encoding, string mode)
		{
			if (!LuaCsFile.IsPathAllowedLuaException(filename, true))
			{
				return Stream.Null;
			}
			return new FileStream(filename, LuaPlatformAccessor.ParseFileMode(mode), LuaPlatformAccessor.ParseFileAccess(mode), FileShare.Read | FileShare.Write | FileShare.Delete);
		}

		// Token: 0x0600257F RID: 9599 RVA: 0x000F5BE3 File Offset: 0x000F3DE3
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

		// Token: 0x06002580 RID: 9600 RVA: 0x000F5C15 File Offset: 0x000F3E15
		public override string IO_OS_GetTempFilename()
		{
			return "LocalMods/temp.txt";
		}

		// Token: 0x06002581 RID: 9601 RVA: 0x000F5C1C File Offset: 0x000F3E1C
		public override void OS_ExitFast(int exitCode)
		{
			throw new ScriptRuntimeException("usage of os.exit is not allowed.");
		}

		// Token: 0x06002582 RID: 9602 RVA: 0x000F5C28 File Offset: 0x000F3E28
		public override bool OS_FileExists(string file)
		{
			return LuaCsFile.Exists(file);
		}

		// Token: 0x06002583 RID: 9603 RVA: 0x000F5C30 File Offset: 0x000F3E30
		public override void OS_FileDelete(string file)
		{
			LuaCsFile.Delete(file);
		}

		// Token: 0x06002584 RID: 9604 RVA: 0x000F5C38 File Offset: 0x000F3E38
		public override void OS_FileMove(string src, string dst)
		{
			LuaCsFile.Move(src, dst);
		}

		// Token: 0x06002585 RID: 9605 RVA: 0x000F5C41 File Offset: 0x000F3E41
		public override int OS_Execute(string cmdline)
		{
			throw new ScriptRuntimeException("usage of os.execute is not allowed.");
		}

		// Token: 0x06002586 RID: 9606 RVA: 0x000F5C4D File Offset: 0x000F3E4D
		public override string GetPlatformNamePrefix()
		{
			return "lua";
		}

		// Token: 0x06002587 RID: 9607 RVA: 0x000F5C54 File Offset: 0x000F3E54
		public override void DefaultPrint(string content)
		{
		}
	}
}
