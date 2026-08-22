using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200003D RID: 61
	internal static class LuaCsInstaller
	{
		// Token: 0x0600091F RID: 2335 RVA: 0x0005BB1C File Offset: 0x00059D1C
		public static void Install()
		{
			ContentPackage luaPackage = LuaCsSetup.GetLuaCsPackage();
			if (luaPackage == null)
			{
				GameMain.Server.SendChatMessage("Couldn't find the LuaCsForBarotrauma content package.", new ChatMessageType?(ChatMessageType.ServerMessageBox), null, null, PlayerConnectionChangeType.None, ChatMode.None);
				return;
			}
			try
			{
				string path = Path.GetDirectoryName(luaPackage.Path);
				string[] filesToCopy = LuaCsInstaller.trackingFiles.Concat(from s in Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)
				where s.Contains("mscordaccore_amd64_amd64")
				select Path.GetFileName(s)).ToArray<string>();
				LuaCsInstaller.CreateMissingDirectory();
				File.Move("Barotrauma.dll", "Temp/Original/Barotrauma.dll", true);
				File.Move("Barotrauma.deps.json", "Temp/Original/Barotrauma.deps.json", true);
				File.Move("Barotrauma.pdb", "Temp/Original/Barotrauma.pdb", true);
				File.Move("BarotraumaCore.dll", "Temp/Original/BarotraumaCore.dll", true);
				File.Move("BarotraumaCore.pdb", "Temp/Original/BarotraumaCore.pdb", true);
				File.Move("System.Reflection.Metadata.dll", "Temp/Original/System.Reflection.Metadata.dll", true);
				File.Move("System.Collections.Immutable.dll", "Temp/Original/System.Collections.Immutable.dll", true);
				File.Move("System.Runtime.CompilerServices.Unsafe.dll", "Temp/Original/System.Runtime.CompilerServices.Unsafe.dll", true);
				foreach (string file in filesToCopy)
				{
					if (File.Exists(file))
					{
						File.Move(file, "Temp/ToDelete/" + file, true);
					}
					File.Copy(Path.Combine(path, "Binary", file), file, true);
				}
				File.WriteAllText("LuaCsDedicatedServer.bat", "\"%LocalAppData%/Daedalic Entertainment GmbH/Barotrauma/WorkshopMods/Installed/2559634234/Binary/DedicatedServer.exe\"");
			}
			catch (UnauthorizedAccessException e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Unauthorized file access exception. This usually means you already have LuaCs installed. $");
				defaultInterpolatedStringHandler.AppendFormatted<UnauthorizedAccessException>(e);
				LuaCsLogger.LogError(defaultInterpolatedStringHandler.ToStringAndClear(), LuaCsMessageOrigin.LuaCs);
				return;
			}
			catch (Exception e2)
			{
				LuaCsLogger.HandleException(e2, LuaCsMessageOrigin.LuaCs);
				return;
			}
			GameMain.Server.SendChatMessage("Client-Side LuaCs installed, restart your game to apply changes.", new ChatMessageType?(ChatMessageType.ServerMessageBox), null, null, PlayerConnectionChangeType.None, ChatMode.None);
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0005BD2C File Offset: 0x00059F2C
		private static void CreateMissingDirectory()
		{
			Directory.CreateDirectory("Temp/Original");
			Directory.CreateDirectory("Temp/ToDelete");
			Directory.CreateDirectory("Temp/ToDelete/Publicized");
			Directory.CreateDirectory("Temp/Old");
			Directory.CreateDirectory("Temp/Old/Publicized");
			Directory.CreateDirectory("Publicized");
		}

		// Token: 0x04000417 RID: 1047
		private static string[] trackingFiles = new string[]
		{
			"Barotrauma.dll",
			"Barotrauma.deps.json",
			"Barotrauma.pdb",
			"BarotraumaCore.dll",
			"BarotraumaCore.pdb",
			"0Harmony.dll",
			"Mono.Cecil.dll",
			"Sigil.dll",
			"Mono.Cecil.Mdb.dll",
			"Mono.Cecil.Pdb.dll",
			"Mono.Cecil.Rocks.dll",
			"MonoMod.Backports.dll",
			"MonoMod.Core.dll",
			"MonoMod.ILHelpers.dll",
			"MonoMod.RuntimeDetour.dll",
			"MonoMod.Utils.dll",
			"MonoMod.Iced.dll",
			"MoonSharp.Interpreter.dll",
			"MoonSharp.VsCodeDebugger.dll",
			"Microsoft.CodeAnalysis.dll",
			"Microsoft.CodeAnalysis.CSharp.dll",
			"Microsoft.CodeAnalysis.CSharp.Scripting.dll",
			"Microsoft.CodeAnalysis.Scripting.dll",
			"Microsoft.Toolkit.Diagnostics.dll",
			"Microsoft.Extensions.Logging.Abstractions.dll",
			"System.Reflection.Metadata.dll",
			"System.Collections.Immutable.dll",
			"System.Runtime.CompilerServices.Unsafe.dll",
			"Publicized/DedicatedServer.dll",
			"Publicized/Barotrauma.dll",
			"Publicized/BarotraumaCore.dll",
			"Basic.Reference.Assemblies.Net80.dll",
			"FluentResults.dll",
			"LightInject.dll",
			"OneOf.dll"
		};
	}
}
