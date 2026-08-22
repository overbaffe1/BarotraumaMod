using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using FluentResults;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000425 RID: 1061
	public class LuaUserDataService : ILuaUserDataService, IReusableService, IService, IDisposable
	{
		// Token: 0x17000FE3 RID: 4067
		// (get) Token: 0x06003C22 RID: 15394 RVA: 0x0018C286 File Offset: 0x0018A486
		// (set) Token: 0x06003C23 RID: 15395 RVA: 0x0018C28E File Offset: 0x0018A48E
		public bool IsDisposed { get; private set; }

		// Token: 0x17000FE4 RID: 4068
		// (get) Token: 0x06003C24 RID: 15396 RVA: 0x0018C297 File Offset: 0x0018A497
		public IReadOnlyDictionary<string, IUserDataDescriptor> Descriptors
		{
			get
			{
				return this.descriptors;
			}
		}

		// Token: 0x06003C25 RID: 15397 RVA: 0x0018C29F File Offset: 0x0018A49F
		public LuaUserDataService(IPluginManagementService pluginManagementService)
		{
			this.descriptors = new ConcurrentDictionary<string, IUserDataDescriptor>();
			this._pluginManagementService = pluginManagementService;
		}

		// Token: 0x17000FE5 RID: 4069
		public IUserDataDescriptor this[string key]
		{
			get
			{
				return this.descriptors.GetValueOrDefault(key);
			}
		}

		// Token: 0x06003C27 RID: 15399 RVA: 0x0018C2C7 File Offset: 0x0018A4C7
		private Type GetType(string typeName)
		{
			return this._pluginManagementService.GetType(typeName, false, true, true);
		}

		// Token: 0x06003C28 RID: 15400 RVA: 0x0018C2D8 File Offset: 0x0018A4D8
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

		// Token: 0x06003C29 RID: 15401 RVA: 0x0018C324 File Offset: 0x0018A524
		public void RegisterExtensionType(string typeName)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to register a type that doesn't exist: " + typeName + ".");
			}
			UserData.RegisterExtensionType(type, InteropAccessMode.Default);
		}

		// Token: 0x06003C2A RID: 15402 RVA: 0x0018C360 File Offset: 0x0018A560
		public bool IsRegistered(string typeName)
		{
			Type type = this.GetType(typeName);
			return !(type == null) && UserData.GetDescriptorForType(type, true) != null;
		}

		// Token: 0x06003C2B RID: 15403 RVA: 0x0018C38C File Offset: 0x0018A58C
		public void UnregisterType(string typeName, bool deleteHistory = false)
		{
			Type type = this.GetType(typeName);
			if (type == null)
			{
				throw new ScriptRuntimeException("tried to unregister a type that doesn't exist: " + typeName + ".");
			}
			UserData.UnregisterType(type, deleteHistory);
		}

		// Token: 0x06003C2C RID: 15404 RVA: 0x0018C3C8 File Offset: 0x0018A5C8
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

		// Token: 0x06003C2D RID: 15405 RVA: 0x0018C41D File Offset: 0x0018A61D
		public string TypeOf(object obj)
		{
			if (obj == null)
			{
				throw new ScriptRuntimeException("userdata is nil");
			}
			return obj.GetType().FullName;
		}

		// Token: 0x06003C2E RID: 15406 RVA: 0x0018C438 File Offset: 0x0018A638
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

		// Token: 0x06003C2F RID: 15407 RVA: 0x0018C4C8 File Offset: 0x0018A6C8
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

		// Token: 0x06003C30 RID: 15408 RVA: 0x0018C534 File Offset: 0x0018A734
		private FieldInfo FindFieldRecursively(Type type, string fieldName)
		{
			FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null && type.BaseType != null)
			{
				return this.FindFieldRecursively(type.BaseType, fieldName);
			}
			return field;
		}

		// Token: 0x06003C31 RID: 15409 RVA: 0x0018C574 File Offset: 0x0018A774
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

		// Token: 0x06003C32 RID: 15410 RVA: 0x0018C5E4 File Offset: 0x0018A7E4
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

		// Token: 0x06003C33 RID: 15411 RVA: 0x0018C634 File Offset: 0x0018A834
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

		// Token: 0x06003C34 RID: 15412 RVA: 0x0018C70C File Offset: 0x0018A90C
		private PropertyInfo FindPropertyRecursively(Type type, string propertyName)
		{
			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null && type.BaseType != null)
			{
				return this.FindPropertyRecursively(type.BaseType, propertyName);
			}
			return property;
		}

		// Token: 0x06003C35 RID: 15413 RVA: 0x0018C74C File Offset: 0x0018A94C
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

		// Token: 0x06003C36 RID: 15414 RVA: 0x0018C7CC File Offset: 0x0018A9CC
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

		// Token: 0x06003C37 RID: 15415 RVA: 0x0018C828 File Offset: 0x0018AA28
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

		// Token: 0x06003C38 RID: 15416 RVA: 0x0018C86C File Offset: 0x0018AA6C
		public void RemoveMember(IUserDataDescriptor IUUD, string memberName)
		{
			if (IUUD == null)
			{
				throw new ScriptRuntimeException("tried to use a UserDataDescriptor that is null to remove the member " + memberName + ".");
			}
			StandardUserDataDescriptor descriptor = (StandardUserDataDescriptor)IUUD;
			descriptor.RemoveMember(memberName);
		}

		// Token: 0x06003C39 RID: 15417 RVA: 0x0018C8A0 File Offset: 0x0018AAA0
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

		// Token: 0x06003C3A RID: 15418 RVA: 0x0018C901 File Offset: 0x0018AB01
		public DynValue CreateUserDataFromDescriptor(DynValue scriptObject, IUserDataDescriptor desiredTypeDescriptor)
		{
			return UserData.Create(scriptObject.ToObject(desiredTypeDescriptor.Type), desiredTypeDescriptor);
		}

		// Token: 0x06003C3B RID: 15419 RVA: 0x0018C918 File Offset: 0x0018AB18
		public DynValue CreateUserDataFromType(DynValue scriptObject, Type desiredType)
		{
			IUserDataDescriptor descriptor = UserData.GetDescriptorForType(desiredType, true);
			if (descriptor == null)
			{
				descriptor = new StandardUserDataDescriptor(desiredType, InteropAccessMode.Default, null);
			}
			return this.CreateUserDataFromDescriptor(scriptObject, descriptor);
		}

		// Token: 0x06003C3C RID: 15420 RVA: 0x0018C941 File Offset: 0x0018AB41
		public void AddCallMetaTable(object userdata)
		{
		}

		// Token: 0x06003C3D RID: 15421 RVA: 0x0018C943 File Offset: 0x0018AB43
		public void Dispose()
		{
			this.IsDisposed = true;
			this.descriptors.Clear();
		}

		// Token: 0x06003C3E RID: 15422 RVA: 0x0018C957 File Offset: 0x0018AB57
		public Result Reset()
		{
			this.descriptors.Clear();
			return Result.Ok();
		}

		// Token: 0x04001D85 RID: 7557
		private ConcurrentDictionary<string, IUserDataDescriptor> descriptors;

		// Token: 0x04001D86 RID: 7558
		private readonly IPluginManagementService _pluginManagementService;
	}
}
