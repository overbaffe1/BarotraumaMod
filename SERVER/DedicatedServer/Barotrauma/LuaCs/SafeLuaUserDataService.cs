using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000427 RID: 1063
	public class SafeLuaUserDataService : ISafeLuaUserDataService, IService, IDisposable
	{
		// Token: 0x17000FE6 RID: 4070
		// (get) Token: 0x06003C52 RID: 15442 RVA: 0x0018C969 File Offset: 0x0018AB69
		// (set) Token: 0x06003C53 RID: 15443 RVA: 0x0018C971 File Offset: 0x0018AB71
		public bool IsDisposed { get; private set; }

		// Token: 0x06003C54 RID: 15444 RVA: 0x0018C97A File Offset: 0x0018AB7A
		public SafeLuaUserDataService(ILuaUserDataService userDataService)
		{
			this._userDataService = userDataService;
		}

		// Token: 0x17000FE7 RID: 4071
		public IUserDataDescriptor this[string key]
		{
			get
			{
				return this._userDataService.Descriptors.GetValueOrDefault(key);
			}
		}

		// Token: 0x06003C56 RID: 15446 RVA: 0x0018C99C File Offset: 0x0018AB9C
		private bool CanBeRegistered(string typeName)
		{
			return !typeName.StartsWith("Barotrauma.Lua", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.Cs", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.LuaCs", StringComparison.Ordinal) && (typeName == "System.Single" || typeName == "System.Console" || typeName.StartsWith("System.Collections", StringComparison.Ordinal) || typeName.StartsWith("Microsoft.Xna", StringComparison.Ordinal) || (!typeName.StartsWith("Barotrauma.IO", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.ToolBox", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.SaveUtil", StringComparison.Ordinal) && typeName.StartsWith("Barotrauma.", StringComparison.Ordinal)));
		}

		// Token: 0x06003C57 RID: 15447 RVA: 0x0018CA54 File Offset: 0x0018AC54
		private bool CanBeReRegistered(string typeName)
		{
			return !typeName.StartsWith("Barotrauma.Lua", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.Cs", StringComparison.Ordinal) && !typeName.StartsWith("Barotrauma.LuaCs", StringComparison.Ordinal);
		}

		// Token: 0x06003C58 RID: 15448 RVA: 0x0018CA83 File Offset: 0x0018AC83
		public bool IsAllowed(string typeName)
		{
			return (this.CanBeReRegistered(typeName) || !this.IsRegistered(typeName)) && this.CanBeRegistered(typeName);
		}

		// Token: 0x06003C59 RID: 15449 RVA: 0x0018CAA5 File Offset: 0x0018ACA5
		private void CheckAllowed(string typeName)
		{
			if (!this.IsAllowed(typeName))
			{
				throw new ScriptRuntimeException("Type " + typeName + " can't be registered");
			}
		}

		// Token: 0x06003C5A RID: 15450 RVA: 0x0018CAC6 File Offset: 0x0018ACC6
		public IUserDataDescriptor RegisterType(string typeName)
		{
			this.CheckAllowed(typeName);
			return this._userDataService.RegisterType(typeName);
		}

		// Token: 0x06003C5B RID: 15451 RVA: 0x0018CADB File Offset: 0x0018ACDB
		public void RegisterExtensionType(string typeName)
		{
			this.CheckAllowed(typeName);
			this._userDataService.RegisterExtensionType(typeName);
		}

		// Token: 0x06003C5C RID: 15452 RVA: 0x0018CAF0 File Offset: 0x0018ACF0
		public bool IsRegistered(string typeName)
		{
			return this._userDataService.IsRegistered(typeName);
		}

		// Token: 0x06003C5D RID: 15453 RVA: 0x0018CAFE File Offset: 0x0018ACFE
		public void UnregisterType(string typeName, bool deleteHistory = false)
		{
			this.IsAllowed(typeName);
			this._userDataService.UnregisterType(typeName, deleteHistory);
		}

		// Token: 0x06003C5E RID: 15454 RVA: 0x0018CB15 File Offset: 0x0018AD15
		public object CreateStatic(string typeName)
		{
			return this._userDataService.CreateStatic(typeName);
		}

		// Token: 0x06003C5F RID: 15455 RVA: 0x0018CB23 File Offset: 0x0018AD23
		public bool IsTargetType(object obj, string typeName)
		{
			return this._userDataService.IsTargetType(obj, typeName);
		}

		// Token: 0x06003C60 RID: 15456 RVA: 0x0018CB32 File Offset: 0x0018AD32
		public string TypeOf(object obj)
		{
			return this._userDataService.TypeOf(obj);
		}

		// Token: 0x06003C61 RID: 15457 RVA: 0x0018CB40 File Offset: 0x0018AD40
		public object CreateEnumTable(string typeName)
		{
			return this._userDataService.CreateEnumTable(typeName);
		}

		// Token: 0x06003C62 RID: 15458 RVA: 0x0018CB4E File Offset: 0x0018AD4E
		public void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
		{
			this._userDataService.MakeFieldAccessible(IUUD, fieldName);
		}

		// Token: 0x06003C63 RID: 15459 RVA: 0x0018CB5D File Offset: 0x0018AD5D
		public void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null)
		{
			this._userDataService.MakeMethodAccessible(IUUD, methodName, parameters);
		}

		// Token: 0x06003C64 RID: 15460 RVA: 0x0018CB6D File Offset: 0x0018AD6D
		public void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName)
		{
			this._userDataService.MakePropertyAccessible(IUUD, propertyName);
		}

		// Token: 0x06003C65 RID: 15461 RVA: 0x0018CB7C File Offset: 0x0018AD7C
		public void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
		{
			this._userDataService.AddMethod(IUUD, methodName, function);
		}

		// Token: 0x06003C66 RID: 15462 RVA: 0x0018CB8C File Offset: 0x0018AD8C
		public void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value)
		{
			this._userDataService.AddField(IUUD, fieldName, value);
		}

		// Token: 0x06003C67 RID: 15463 RVA: 0x0018CB9C File Offset: 0x0018AD9C
		public void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			this._userDataService.RemoveMember(IUUD, memberName);
		}

		// Token: 0x06003C68 RID: 15464 RVA: 0x0018CBAB File Offset: 0x0018ADAB
		public bool HasMember(object obj, string memberName)
		{
			return this._userDataService.HasMember(obj, memberName);
		}

		// Token: 0x06003C69 RID: 15465 RVA: 0x0018CBBA File Offset: 0x0018ADBA
		public DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor)
		{
			return this._userDataService.CreateUserDataFromDescriptor(scriptObject, desiredTypeDescriptor);
		}

		// Token: 0x06003C6A RID: 15466 RVA: 0x0018CBC9 File Offset: 0x0018ADC9
		public DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType)
		{
			return this._userDataService.CreateUserDataFromType(scriptObject, desiredType);
		}

		// Token: 0x06003C6B RID: 15467 RVA: 0x0018CBD8 File Offset: 0x0018ADD8
		public void AddCallMetaTable(object userdata)
		{
		}

		// Token: 0x06003C6C RID: 15468 RVA: 0x0018CBDA File Offset: 0x0018ADDA
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x04001D87 RID: 7559
		private readonly ILuaUserDataService _userDataService;
	}
}
