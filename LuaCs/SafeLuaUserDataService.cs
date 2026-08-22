using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200053A RID: 1338
	public class SafeLuaUserDataService : ISafeLuaUserDataService, IService, IDisposable
	{
		// Token: 0x17001536 RID: 5430
		// (get) Token: 0x0600556D RID: 21869 RVA: 0x002D1129 File Offset: 0x002CF329
		// (set) Token: 0x0600556E RID: 21870 RVA: 0x002D1131 File Offset: 0x002CF331
		public bool IsDisposed { get; private set; }

		// Token: 0x0600556F RID: 21871 RVA: 0x002D113A File Offset: 0x002CF33A
		public SafeLuaUserDataService(ILuaUserDataService userDataService)
		{
			this._userDataService = userDataService;
		}

		// Token: 0x17001537 RID: 5431
		public IUserDataDescriptor this[string key]
		{
			get
			{
				return this._userDataService.Descriptors.GetValueOrDefault(key);
			}
		}

		// Token: 0x06005571 RID: 21873 RVA: 0x002D115C File Offset: 0x002CF35C
		private bool CanBeRegistered(string typeName)
		{
			return !typeName.StartsWith("Barotrauma.Lua", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.Cs", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.LuaCs", StringComparison.Ordinal) && (typeName == "System.Single" || typeName == "System.Console" || typeName.StartsWith("System.Collections", StringComparison.Ordinal) || typeName.StartsWith("Microsoft.Xna", StringComparison.Ordinal) || (!typeName.StartsWith("Barotrauma.IO", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.ToolBox", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.SaveUtil", StringComparison.Ordinal) && typeName.StartsWith("Barotrauma.", StringComparison.Ordinal)));
		}

		// Token: 0x06005572 RID: 21874 RVA: 0x002D1214 File Offset: 0x002CF414
		private bool CanBeReRegistered(string typeName)
		{
			return !typeName.StartsWith("Barotrauma.Lua", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.Cs", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.LuaCs", StringComparison.Ordinal);
		}

		// Token: 0x06005573 RID: 21875 RVA: 0x002D1243 File Offset: 0x002CF443
		public bool IsAllowed(string typeName)
		{
			return (this.CanBeReRegistered(typeName) || !this.IsRegistered(typeName)) && this.CanBeRegistered(typeName);
		}

		// Token: 0x06005574 RID: 21876 RVA: 0x002D1265 File Offset: 0x002CF465
		private void CheckAllowed(string typeName)
		{
			if (!this.IsAllowed(typeName))
			{
				throw new ScriptRuntimeException("Type " + typeName + " can't be registered");
			}
		}

		// Token: 0x06005575 RID: 21877 RVA: 0x002D1286 File Offset: 0x002CF486
		public IUserDataDescriptor RegisterType(string typeName)
		{
			this.CheckAllowed(typeName);
			return this._userDataService.RegisterType(typeName);
		}

		// Token: 0x06005576 RID: 21878 RVA: 0x002D129B File Offset: 0x002CF49B
		public void RegisterExtensionType(string typeName)
		{
			this.CheckAllowed(typeName);
			this._userDataService.RegisterExtensionType(typeName);
		}

		// Token: 0x06005577 RID: 21879 RVA: 0x002D12B0 File Offset: 0x002CF4B0
		public bool IsRegistered(string typeName)
		{
			return this._userDataService.IsRegistered(typeName);
		}

		// Token: 0x06005578 RID: 21880 RVA: 0x002D12BE File Offset: 0x002CF4BE
		public void UnregisterType(string typeName, bool deleteHistory = false)
		{
			this.IsAllowed(typeName);
			this._userDataService.UnregisterType(typeName, deleteHistory);
		}

		// Token: 0x06005579 RID: 21881 RVA: 0x002D12D5 File Offset: 0x002CF4D5
		public object CreateStatic(string typeName)
		{
			return this._userDataService.CreateStatic(typeName);
		}

		// Token: 0x0600557A RID: 21882 RVA: 0x002D12E3 File Offset: 0x002CF4E3
		public bool IsTargetType(object obj, string typeName)
		{
			return this._userDataService.IsTargetType(obj, typeName);
		}

		// Token: 0x0600557B RID: 21883 RVA: 0x002D12F2 File Offset: 0x002CF4F2
		public string TypeOf(object obj)
		{
			return this._userDataService.TypeOf(obj);
		}

		// Token: 0x0600557C RID: 21884 RVA: 0x002D1300 File Offset: 0x002CF500
		public object CreateEnumTable(string typeName)
		{
			return this._userDataService.CreateEnumTable(typeName);
		}

		// Token: 0x0600557D RID: 21885 RVA: 0x002D130E File Offset: 0x002CF50E
		public void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
		{
			this._userDataService.MakeFieldAccessible(IUUD, fieldName);
		}

		// Token: 0x0600557E RID: 21886 RVA: 0x002D131D File Offset: 0x002CF51D
		public void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null)
		{
			this._userDataService.MakeMethodAccessible(IUUD, methodName, parameters);
		}

		// Token: 0x0600557F RID: 21887 RVA: 0x002D132D File Offset: 0x002CF52D
		public void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName)
		{
			this._userDataService.MakePropertyAccessible(IUUD, propertyName);
		}

		// Token: 0x06005580 RID: 21888 RVA: 0x002D133C File Offset: 0x002CF53C
		public void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
		{
			this._userDataService.AddMethod(IUUD, methodName, function);
		}

		// Token: 0x06005581 RID: 21889 RVA: 0x002D134C File Offset: 0x002CF54C
		public void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value)
		{
			this._userDataService.AddField(IUUD, fieldName, value);
		}

		// Token: 0x06005582 RID: 21890 RVA: 0x002D135C File Offset: 0x002CF55C
		public void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			this._userDataService.RemoveMember(IUUD, memberName);
		}

		// Token: 0x06005583 RID: 21891 RVA: 0x002D136B File Offset: 0x002CF56B
		public bool HasMember(object obj, string memberName)
		{
			return this._userDataService.HasMember(obj, memberName);
		}

		// Token: 0x06005584 RID: 21892 RVA: 0x002D137A File Offset: 0x002CF57A
		public DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor)
		{
			return this._userDataService.CreateUserDataFromDescriptor(scriptObject, desiredTypeDescriptor);
		}

		// Token: 0x06005585 RID: 21893 RVA: 0x002D1389 File Offset: 0x002CF589
		public DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType)
		{
			return this._userDataService.CreateUserDataFromType(scriptObject, desiredType);
		}

		// Token: 0x06005586 RID: 21894 RVA: 0x002D1398 File Offset: 0x002CF598
		public void AddCallMetaTable(object userdata)
		{
		}

		// Token: 0x06005587 RID: 21895 RVA: 0x002D139A File Offset: 0x002CF59A
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x04002C6B RID: 11371
		private readonly ILuaUserDataService _userDataService;
	}
}
