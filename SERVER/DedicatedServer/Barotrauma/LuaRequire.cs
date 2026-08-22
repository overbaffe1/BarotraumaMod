using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	// Token: 0x02000219 RID: 537
	internal class LuaRequire
	{
		// Token: 0x17000ACE RID: 2766
		// (get) Token: 0x06002589 RID: 9609 RVA: 0x000F5C5E File Offset: 0x000F3E5E
		// (set) Token: 0x0600258A RID: 9610 RVA: 0x000F5C66 File Offset: 0x000F3E66
		private Script lua { get; set; }

		// Token: 0x17000ACF RID: 2767
		// (get) Token: 0x0600258B RID: 9611 RVA: 0x000F5C6F File Offset: 0x000F3E6F
		// (set) Token: 0x0600258C RID: 9612 RVA: 0x000F5C77 File Offset: 0x000F3E77
		private Dictionary<string, DynValue> loadedModules { get; set; }

		// Token: 0x0600258D RID: 9613 RVA: 0x000F5C80 File Offset: 0x000F3E80
		private bool GetExistingReturnValue(string moduleName, ref DynValue returnValue)
		{
			return this.loadedModules.TryGetValue(moduleName, out returnValue);
		}

		// Token: 0x0600258E RID: 9614 RVA: 0x000F5C8F File Offset: 0x000F3E8F
		private string FixContentPackagePath(string contentPackagePath)
		{
			contentPackagePath = Path.TrimEndingDirectorySeparator(new FileInfo(contentPackagePath).Directory.FullName.CleanUpPathCrossPlatform(true, ""));
			return contentPackagePath;
		}

		// Token: 0x0600258F RID: 9615 RVA: 0x000F5CB4 File Offset: 0x000F3EB4
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

		// Token: 0x06002590 RID: 9616 RVA: 0x000F5D1C File Offset: 0x000F3F1C
		private string GetContentPackagePath(string moduleName, Table environment)
		{
			string filePath = this.lua.Options.ScriptLoader.ResolveModuleName(moduleName, environment);
			filePath = Path.TrimEndingDirectorySeparator(new FileInfo(filePath).Directory.FullName.CleanUpPathCrossPlatform(true, ""));
			return this.GetContentPackagePath(filePath);
		}

		// Token: 0x06002591 RID: 9617 RVA: 0x000F5D69 File Offset: 0x000F3F69
		private void SaveReturnValue(string moduleName, DynValue returnValue)
		{
			this.loadedModules[moduleName] = returnValue;
		}

		// Token: 0x06002592 RID: 9618 RVA: 0x000F5D78 File Offset: 0x000F3F78
		private void ExecuteModule(string moduleName, Table environment, ref DynValue returnValue)
		{
			DynValue loadFunc = this.lua.RequireModule(moduleName, environment);
			string packagePath = this.GetContentPackagePath(moduleName, environment);
			returnValue = this.lua.Call(loadFunc, new object[]
			{
				packagePath
			});
		}

		// Token: 0x06002593 RID: 9619 RVA: 0x000F5DB4 File Offset: 0x000F3FB4
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

		// Token: 0x06002594 RID: 9620 RVA: 0x000F5E0E File Offset: 0x000F400E
		public LuaRequire(Script lua)
		{
			this.lua = lua;
			this.loadedModules = new Dictionary<string, DynValue>();
		}
	}
}
