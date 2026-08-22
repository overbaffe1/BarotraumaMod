using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000537 RID: 1335
	public interface ILuaUserDataService : IReusableService, IService, IDisposable
	{
		// Token: 0x17001532 RID: 5426
		// (get) Token: 0x0600552A RID: 21802
		IReadOnlyDictionary<string, IUserDataDescriptor> Descriptors { get; }

		// Token: 0x0600552B RID: 21803
		IUserDataDescriptor RegisterType(string typeName);

		// Token: 0x0600552C RID: 21804
		void RegisterExtensionType(string typeName);

		// Token: 0x0600552D RID: 21805
		bool IsRegistered(string typeName);

		// Token: 0x0600552E RID: 21806
		void UnregisterType(string typeName, bool deleteHistory = false);

		// Token: 0x0600552F RID: 21807
		object CreateStatic(string typeName);

		// Token: 0x06005530 RID: 21808
		bool IsTargetType(object obj, string typeName);

		// Token: 0x06005531 RID: 21809
		string TypeOf(object obj);

		// Token: 0x06005532 RID: 21810
		object CreateEnumTable(string typeName);

		// Token: 0x06005533 RID: 21811
		void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName);

		// Token: 0x06005534 RID: 21812
		void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null);

		// Token: 0x06005535 RID: 21813
		void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName);

		// Token: 0x06005536 RID: 21814
		void AddMethod(IUserDataDescriptor IUUD, string methodName, object function);

		// Token: 0x06005537 RID: 21815
		void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value);

		// Token: 0x06005538 RID: 21816
		void RemoveMember(IUserDataDescriptor IUUD, string memberName);

		// Token: 0x06005539 RID: 21817
		bool HasMember(object obj, string memberName);

		// Token: 0x0600553A RID: 21818
		DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor);

		// Token: 0x0600553B RID: 21819
		DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType);

		// Token: 0x0600553C RID: 21820
		void AddCallMetaTable(object userdata);
	}
}
