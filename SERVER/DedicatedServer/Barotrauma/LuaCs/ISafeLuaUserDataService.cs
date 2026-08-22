using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000426 RID: 1062
	public interface ISafeLuaUserDataService : IService, IDisposable
	{
		// Token: 0x06003C3F RID: 15423
		bool IsAllowed(string typeName);

		// Token: 0x06003C40 RID: 15424
		IUserDataDescriptor RegisterType(string typeName);

		// Token: 0x06003C41 RID: 15425
		void RegisterExtensionType(string typeName);

		// Token: 0x06003C42 RID: 15426
		bool IsRegistered(string typeName);

		// Token: 0x06003C43 RID: 15427
		void UnregisterType(string typeName, bool deleteHistory = false);

		// Token: 0x06003C44 RID: 15428
		object CreateStatic(string typeName);

		// Token: 0x06003C45 RID: 15429
		bool IsTargetType(object obj, string typeName);

		// Token: 0x06003C46 RID: 15430
		string TypeOf(object obj);

		// Token: 0x06003C47 RID: 15431
		object CreateEnumTable(string typeName);

		// Token: 0x06003C48 RID: 15432
		void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName);

		// Token: 0x06003C49 RID: 15433
		void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null);

		// Token: 0x06003C4A RID: 15434
		void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName);

		// Token: 0x06003C4B RID: 15435
		void AddMethod(IUserDataDescriptor IUUD, string methodName, object function);

		// Token: 0x06003C4C RID: 15436
		void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value);

		// Token: 0x06003C4D RID: 15437
		void RemoveMember(IUserDataDescriptor IUUD, string memberName);

		// Token: 0x06003C4E RID: 15438
		bool HasMember(object obj, string memberName);

		// Token: 0x06003C4F RID: 15439
		DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor);

		// Token: 0x06003C50 RID: 15440
		DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType);

		// Token: 0x06003C51 RID: 15441
		void AddCallMetaTable(object userdata);
	}
}
