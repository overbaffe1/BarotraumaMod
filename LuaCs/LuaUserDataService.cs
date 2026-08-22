using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using FluentResults;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000538 RID: 1336
	public class LuaUserDataService : ILuaUserDataService, IReusableService, IService, IDisposable
	{
		// Token: 0x17001533 RID: 5427
		// (get) Token: 0x0600553D RID: 21821 RVA: 0x002D0A46 File Offset: 0x002CEC46
		// (set) Token: 0x0600553E RID: 21822 RVA: 0x002D0A4E File Offset: 0x002CEC4E
		public bool IsDisposed { get; private set; }

		// Token: 0x17001534 RID: 5428
		// (get) Token: 0x0600553F RID: 21823 RVA: 0x002D0A57 File Offset: 0x002CEC57
		public IReadOnlyDictionary<string, IUserDataDescriptor> Descriptors
		{
			get
			{
				return this.descriptors;
			}
		}

		// Token: 0x06005540 RID: 21824 RVA: 0x002D0A5F File Offset: 0x002CEC5F
		public LuaUserDataService(IPluginManagementService pluginManagementService)
		{
			this.descriptors = new ConcurrentDictionary<string, IUserDataDescriptor>();
			this._pluginManagementService = pluginManagementService;
		}

		// Token: 0x17001535 RID: 5429
		public IUserDataDescriptor this[string key]
		{
			get
			{
				return this.descriptors.GetValueOrDefault(key);
			}
		}

		// Token: 0x06005542 RID: 21826 RVA: 0x002D0A87 File Offset: 0x002CEC87
		private Type GetType(string typeName)
		{
			return this._pluginManagementService.GetType(typeName, false, true, true);
		}

		// Token: 0x06005543 RID: 21827 RVA: 0x002D0A98 File Offset: 0x002CEC98
		public IUserDataDescriptor RegisterType(string typeName)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to register a type that doesn't exist: " + typeName + ".");
			}
			IUserDataDescriptor descriptor = UserData.RegisterType(type, InteropAccessMode.Default, null);
			this.descriptors.TryAdd(typeName, descriptor);
			return descriptor;
		}

		// Token: 0x06005544 RID: 21828 RVA: 0x002D0AE4 File Offset: 0x002CECE4
		public void RegisterExtensionType(string typeName)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to register a type that doesn't exist: " + typeName + ".");
			}
			UserData.RegisterExtensionType(type, InteropAccessMode.Default);
		}

		// Token: 0x06005545 RID: 21829 RVA: 0x002D0B20 File Offset: 0x002CED20
		public bool IsRegistered(string typeName)
		{
			Type type = this.GetType(typeName);
			return !(type == null) && UserData.GetDescriptorForType(type, true) != null;
		}

		// Token: 0x06005546 RID: 21830 RVA: 0x002D0B4C File Offset: 0x002CED4C
		public void UnregisterType(string typeName, bool deleteHistory = false)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to unregister a type that doesn't exist: " + typeName + ".");
			}
			UserData.UnregisterType(type, deleteHistory);
		}

		// Token: 0x06005547 RID: 21831 RVA: 0x002D0B88 File Offset: 0x002CED88
		public bool IsTargetType(object obj, string typeName)
		{
			if (obj == null)
			{
				throw new ScriptRuntimeException("userdata is nil");
			}
			Type targetType = this.GetType(typeName);
			if (targetType == null)
			{
				throw new ScriptRuntimeException("target type not found");
			}
			Type type = (obj is Type) ? ((Type)obj) : obj.GetType();
			return targetType.IsAssignableFrom(type);
		}

		// Token: 0x06005548 RID: 21832 RVA: 0x002D0BDD File Offset: 0x002CEDDD
		public string TypeOf(object obj)
		{
			if (obj == null)
			{
				throw new ScriptRuntimeException("userdata is nil");
			}
			return obj.GetType().FullName;
		}

		// Token: 0x06005549 RID: 21833 RVA: 0x002D0BF8 File Offset: 0x002CEDF8
		public object CreateEnumTable(string typeName)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to create an enum table with a type that doesn't exist:: " + typeName + ".");
			}
			Dictionary<string, object> result = new Dictionary<string, object>();
			foreach (object value in Enum.GetValues(type))
			{
				string name = Enum.GetName(type, value);
				result[name] = value;
			}
			return result;
		}

		// Token: 0x0600554A RID: 21834 RVA: 0x002D0C88 File Offset: 0x002CEE88
		public object CreateStatic(string typeName)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to create a static userdata of a type that doesn't exist: " + typeName + ".");
			}
			MethodInfo method = typeof(UserData).GetMethod("CreateStatic", 1, new Type[0]);
			MethodInfo generic = method.MakeGenericMethod(new Type[]
			{
				type
			});
			return generic.Invoke(null, null);
		}

		// Token: 0x0600554B RID: 21835 RVA: 0x002D0CF4 File Offset: 0x002CEEF4
		private FieldInfo FindFieldRecursively(Type type, string fieldName)
		{
			FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null && type.BaseType != null)
			{
				return this.FindFieldRecursively(type.BaseType, fieldName);
			}
			return field;
		}

		// Token: 0x0600554C RID: 21836 RVA: 0x002D0D34 File Offset: 0x002CEF34
		public void MakeFieldAccessible(IUserDataDescriptor IUUD, string fieldName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to make " + fieldName + " accessible.");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			FieldInfo field = this.FindFieldRecursively(IUUD.Type, fieldName);
			if (field == null)
			{
				throw new ScriptRuntimeException("tried to make field '" + fieldName + "' accessible, but the field doesn't exist.");
			}
			descriptor.RemoveMember(fieldName);
			descriptor.AddMember(fieldName, new FieldMemberDescriptor(field, InteropAccessMode.Default));
		}

		// Token: 0x0600554D RID: 21837 RVA: 0x002D0DA4 File Offset: 0x002CEFA4
		private MethodInfo FindMethodRecursively(Type type, string methodName, Type[] types = null)
		{
			MethodInfo method;
			if (types == null)
			{
				method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			}
			else
			{
				method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic, types);
			}
			if (method == null && type.BaseType != null)
			{
				return this.FindMethodRecursively(type.BaseType, methodName, types);
			}
			return method;
		}

		// Token: 0x0600554E RID: 21838 RVA: 0x002D0DF4 File Offset: 0x002CEFF4
		public void MakeMethodAccessible(IUserDataDescriptor IUUD, string methodName, string[] parameters = null)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to make " + methodName + " accessible.");
			}
			Type[] parameterTypes = null;
			if (parameters != null)
			{
				parameterTypes = new Type[parameters.Length];
				for (int i = 0; i < parameters.Length; i++)
				{
					Type type = this.GetType(parameters[i]);
					if (type == null)
					{
						throw new ScriptRuntimeException("invalid parameter type '" + parameters[i] + "'");
					}
					parameterTypes[i] = type;
				}
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			MethodBase method;
			try
			{
				method = this.FindMethodRecursively(IUUD.Type, methodName, parameterTypes);
			}
			catch (AmbiguousMatchException ex)
			{
				throw new ScriptRuntimeException("ambiguous method signature.");
			}
			if (method == null)
			{
				throw new ScriptRuntimeException("tried to make method '" + methodName + "' accessible, but the method doesn't exist.");
			}
			descriptor.AddMember(methodName, new MethodMemberDescriptor(method, InteropAccessMode.Default));
		}

		// Token: 0x0600554F RID: 21839 RVA: 0x002D0ECC File Offset: 0x002CF0CC
		private PropertyInfo FindPropertyRecursively(Type type, string propertyName)
		{
			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null && type.BaseType != null)
			{
				return this.FindPropertyRecursively(type.BaseType, propertyName);
			}
			return property;
		}

		// Token: 0x06005550 RID: 21840 RVA: 0x002D0F0C File Offset: 0x002CF10C
		public void MakePropertyAccessible(IUserDataDescriptor IUUD, string propertyName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to make " + propertyName + " accessible.");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			PropertyInfo property = this.FindPropertyRecursively(IUUD.Type, propertyName);
			if (property == null)
			{
				throw new ScriptRuntimeException("tried to make property '" + propertyName + "' accessible, but the property doesn't exist.");
			}
			descriptor.RemoveMember(propertyName);
			descriptor.AddMember(propertyName, new PropertyMemberDescriptor(property, InteropAccessMode.Default, property.GetGetMethod(true), property.GetSetMethod(true)));
		}

		// Token: 0x06005551 RID: 21841 RVA: 0x002D0F8C File Offset: 0x002CF18C
		public void AddMethod(IUserDataDescriptor IUUD, string methodName, object function)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to add method " + methodName + ".");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(methodName);
			descriptor.AddMember(methodName, new ObjectCallbackMemberDescriptor(methodName, delegate(object arg1, ScriptExecutionContext arg2, CallbackArguments arg3)
			{
				if (LuaCsSetup.Instance != null)
				{
					LuaCsSetup instance = LuaCsSetup.Instance;
					object function2 = function;
					object[] array = arg3.GetArray(0);
					return instance.CallLuaFunction(function2, array);
				}
				return null;
			}));
		}

		// Token: 0x06005552 RID: 21842 RVA: 0x002D0FE8 File Offset: 0x002CF1E8
		public void AddField(IUserDataDescriptor IUUD, string fieldName, DynValue value)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to add field " + fieldName + ".");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(fieldName);
			descriptor.AddMember(fieldName, new DynValueMemberDescriptor(fieldName, value));
		}

		// Token: 0x06005553 RID: 21843 RVA: 0x002D102C File Offset: 0x002CF22C
		public void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to remove the member " + memberName + ".");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(memberName);
		}

		// Token: 0x06005554 RID: 21844 RVA: 0x002D1060 File Offset: 0x002CF260
		public bool HasMember(object obj, string memberName)
		{
			if (obj == null)
			{
				throw new ScriptRuntimeException("object is nil");
			}
			Type type;
			if (obj is Type)
			{
				type = (Type)obj;
			}
			else
			{
				IUserDataDescriptor descriptor = obj as IUserDataDescriptor;
				if (descriptor != null)
				{
					type = descriptor.Type;
					if (((StandardUserDataDescriptor)descriptor).HasMember(memberName))
					{
						return true;
					}
				}
				else
				{
					type = obj.GetType();
				}
			}
			return type.GetMember(memberName).Length != 0;
		}

		// Token: 0x06005555 RID: 21845 RVA: 0x002D10C1 File Offset: 0x002CF2C1
		public DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor)
		{
			return UserData.Create(scriptObject.ToObject(desiredTypeDescriptor.Type), desiredTypeDescriptor);
		}

		// Token: 0x06005556 RID: 21846 RVA: 0x002D10D8 File Offset: 0x002CF2D8
		public DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType)
		{
			IUserDataDescriptor descriptor = UserData.GetDescriptorForType(desiredType, true);
			if (descriptor == null)
			{
				descriptor = new StandardUserDataDescriptor(desiredType, InteropAccessMode.Default, null);
			}
			return this.CreateUserDataFromDescriptor(scriptObject, descriptor);
		}

		// Token: 0x06005557 RID: 21847 RVA: 0x002D1101 File Offset: 0x002CF301
		public void AddCallMetaTable(object userdata)
		{
		}

		// Token: 0x06005558 RID: 21848 RVA: 0x002D1103 File Offset: 0x002CF303
		public void Dispose()
		{
			this.IsDisposed = true;
			this.descriptors.Clear();
		}

		// Token: 0x06005559 RID: 21849 RVA: 0x002D1117 File Offset: 0x002CF317
		public Result Reset()
		{
			this.descriptors.Clear();
			return Result.Ok();
		}

		// Token: 0x04002C69 RID: 11369
		private ConcurrentDictionary<string, IUserDataDescriptor> descriptors;

		// Token: 0x04002C6A RID: 11370
		private readonly IPluginManagementService _pluginManagementService;
	}
}
