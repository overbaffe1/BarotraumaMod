using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000424 RID: 1060
	public interface ILuaUserDataService : IReusableService, IService, IDisposable
	{
		// Token: 0x17000FE2 RID: 4066
		// (get) Token: 0x06003C0F RID: 15375
		IReadOnlyDictionary<string, IUserDataDescriptor> Descriptors { get; }

		// Token: 0x06003C10 RID: 15376
		IUserDataDescriptor RegisterType(string typeName);

		// Token: 0x06003C11 RID: 15377
		void RegisterExtensionType(string typeName);

		// Token: 0x06003C12 RID: 15378
		bool IsRegistered(string typeName);

		// Token: 0x06003C13 RID: 15379
		void UnregisterType(string typeName, bool deleteHistory = false);

		// Token: 0x06003C14 RID: 15380
		object CreateStatic(string typeName);

		// Token: 0x06003C15 RID: 15381
		bool IsTargetType(object obj, string typeName);

		// Token: 0x06003C16 RID: 15382
		string TypeOf(object obj);

		// Token: 0x06003C17 RID: 15383
		object CreateEnumTable(string typeName);

		// Token: 0x06003C18 RID: 15384
		void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName);

		// Token: 0x06003C19 RID: 15385
		void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null);

		// Token: 0x06003C1A RID: 15386
		void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName);

		// Token: 0x06003C1B RID: 15387
		void AddMethod(IUserDataDescriptor IUUD, string methodName, object function);

		// Token: 0x06003C1C RID: 15388
		void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value);

		// Token: 0x06003C1D RID: 15389
		void RemoveMember(IUserDataDescriptor IUUD, string memberName);

		// Token: 0x06003C1E RID: 15390
		bool HasMember(object obj, string memberName);

		// Token: 0x06003C1F RID: 15391
		DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor);

		// Token: 0x06003C20 RID: 15392
		DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType);

		// Token: 0x06003C21 RID: 15393
		void AddCallMetaTable(object userdata);
	}
}
