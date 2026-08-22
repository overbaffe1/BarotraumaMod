using System;
using System.IO;

namespace Barotrauma
{
	// Token: 0x020000CA RID: 202
	internal static class LuaCsInstaller
	{
		// Token: 0x06001ADB RID: 6875 RVA: 0x00109D6F File Offset: 0x00107F6F
		public static void Uninstall()
		{
		}

		// Token: 0x06001ADC RID: 6876 RVA: 0x00109D74 File Offset: 0x00107F74
		private static void CreateMissingDirectory()
		{
			Directory.CreateDirectory("Temp/Original");
			Directory.CreateDirectory("Temp/ToDelete");
			Directory.CreateDirectory("Temp/ToDelete/Publicized");
			Directory.CreateDirectory("Temp/Old");
			Directory.CreateDirectory("Temp/Old/Publicized");
			Directory.CreateDirectory("Publicized");
		}

		// Token: 0x04000DBC RID: 3516
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
