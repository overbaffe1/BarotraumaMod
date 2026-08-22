using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000539 RID: 1337
	public interface ISafeLuaUserDataService : IService, IDisposable
	{
		// Token: 0x0600555A RID: 21850
		bool IsAllowed(string typeName);

		// Token: 0x0600555B RID: 21851
		IUserDataDescriptor RegisterType(string typeName);

		// Token: 0x0600555C RID: 21852
		void RegisterExtensionType(string typeName);

		// Token: 0x0600555D RID: 21853
		bool IsRegistered(string typeName);

		// Token: 0x0600555E RID: 21854
		void UnregisterType(string typeName, bool deleteHistory = false);

		// Token: 0x0600555F RID: 21855
		object CreateStatic(string typeName);

		// Token: 0x06005560 RID: 21856
		bool IsTargetType(object obj, string typeName);

		// Token: 0x06005561 RID: 21857
		string TypeOf(object obj);

		// Token: 0x06005562 RID: 21858
		object CreateEnumTable(string typeName);

		// Token: 0x06005563 RID: 21859
		void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName);

		// Token: 0x06005564 RID: 21860
		void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null);

		// Token: 0x06005565 RID: 21861
		void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName);

		// Token: 0x06005566 RID: 21862
		void AddMethod(IUserDataDescriptor IUUD, string methodName, object function);

		// Token: 0x06005567 RID: 21863
		void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value);

		// Token: 0x06005568 RID: 21864
		void RemoveMember(IUserDataDescriptor IUUD, string memberName);

		// Token: 0x06005569 RID: 21865
		bool HasMember(object obj, string memberName);

		// Token: 0x0600556A RID: 21866
		DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor);

		// Token: 0x0600556B RID: 21867
		DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType);

		// Token: 0x0600556C RID: 21868
		void AddCallMetaTable(object userdata);
	}
}
