using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	// Token: 0x02000301 RID: 769
	internal class LuaRequire
	{
		// Token: 0x17001066 RID: 4198
		// (get) Token: 0x06003E70 RID: 15984 RVA: 0x00233616 File Offset: 0x00231816
		// (set) Token: 0x06003E71 RID: 15985 RVA: 0x0023361E File Offset: 0x0023181E
		private Script lua { get; set; }

		// Token: 0x17001067 RID: 4199
		// (get) Token: 0x06003E72 RID: 15986 RVA: 0x00233627 File Offset: 0x00231827
		// (set) Token: 0x06003E73 RID: 15987 RVA: 0x0023362F File Offset: 0x0023182F
		private Dictionary<string, DynValue> loadedModules { get; set; }

		// Token: 0x06003E74 RID: 15988 RVA: 0x00233638 File Offset: 0x00231838
		private bool GetExistingReturnValue(string moduleName, ref DynValue returnValue)
		{
			return this.loadedModules.TryGetValue(moduleName, out returnValue);
		}

		// Token: 0x06003E75 RID: 15989 RVA: 0x00233647 File Offset: 0x00231847
		private string FixContentPackagePath(string contentPackagePath)
		{
			contentPackagePath = Path.TrimEndingDirectorySeparator(new FileInfo(contentPackagePath).Directory.FullName.CleanUpPathCrossPlatform(true, ""));
			return contentPackagePath;
		}

		// Token: 0x06003E76 RID: 15990 RVA: 0x0023366C File Offset: 0x0023186C
		private string GetContentPackagePath(string path)
		{
			IEnumerable<ContentPackage> allContentPackages = ContentPackageManager.AllPackages;
			foreach (ContentPackage contentPackage in allContentPackages)
			{
				string contentPackagePath = this.FixContentPackagePath(contentPackage.Path);
				if (path.StartsWith(contentPackagePath))
				{
					return contentPackagePath;
				}
			}
			return null;
		}

		// Token: 0x06003E77 RID: 15991 RVA: 0x002336D4 File Offset: 0x002318D4
		private string GetContentPackagePath(string moduleName, Table environment)
		{
			string filePath = this.lua.Options.ScriptLoader.ResolveModuleName(moduleName, environment);
			filePath = Path.TrimEndingDirectorySeparator(new FileInfo(filePath).Directory.FullName.CleanUpPathCrossPlatform(true, ""));
			return this.GetContentPackagePath(filePath);
		}

		// Token: 0x06003E78 RID: 15992 RVA: 0x00233721 File Offset: 0x00231921
		private void SaveReturnValue(string moduleName, DynValue returnValue)
		{
			this.loadedModules[moduleName] = returnValue;
		}

		// Token: 0x06003E79 RID: 15993 RVA: 0x00233730 File Offset: 0x00231930
		private void ExecuteModule(string moduleName, Table environment, ref DynValue returnValue)
		{
			DynValue loadFunc = this.lua.RequireModule(moduleName, environment);
			string packagePath = this.GetContentPackagePath(moduleName, environment);
			returnValue = this.lua.Call(loadFunc, new object[]
			{
				packagePath
			});
		}

		// Token: 0x06003E7A RID: 15994 RVA: 0x0023376C File Offset: 0x0023196C
		public DynValue Require(string moduleName, Table globalContext)
		{
			DynValue returnValue = null;
			Table environment = globalContext ?? this.lua.Globals;
			if (this.GetExistingReturnValue(moduleName, ref returnValue))
			{
				return returnValue;
			}
			this.ExecuteModule(moduleName, environment, ref returnValue);
			if (returnValue == null || returnValue.IsNil() || returnValue.IsVoid())
			{
				returnValue = DynValue.NewBoolean(true);
			}
			this.SaveReturnValue(moduleName, returnValue);
			return returnValue;
		}

		// Token: 0x06003E7B RID: 15995 RVA: 0x002337C6 File Offset: 0x002319C6
		public LuaRequire(Script lua)
		{
			this.lua = lua;
			this.loadedModules = new Dictionary<string, DynValue>();
		}
	}
}
