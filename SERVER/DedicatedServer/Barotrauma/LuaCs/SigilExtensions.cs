using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Sigil;
using Sigil.NonGeneric;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003DC RID: 988
	internal static class SigilExtensions
	{
		// Token: 0x06003900 RID: 14592 RVA: 0x0017C298 File Offset: 0x0017A498
		public static void LoadType(this Emit il, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			il.LoadConstant(type);
			il.Call(typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new Type[]
			{
				typeof(RuntimeTypeHandle)
			}, null), null);
		}

		// Token: 0x06003901 RID: 14593 RVA: 0x0017C2F4 File Offset: 0x0017A4F4
		public static void ToObject(this Emit il, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			il.DerefIfByRef(ref type);
			if (type.IsValueType)
			{
				il.Box(type);
				return;
			}
			if (type != typeof(object))
			{
				il.CastClass<object>();
			}
		}

		// Token: 0x06003902 RID: 14594 RVA: 0x0017C347 File Offset: 0x0017A547
		public static void DerefIfByRef(this Emit il, Type type)
		{
			il.DerefIfByRef(ref type);
		}

		// Token: 0x06003903 RID: 14595 RVA: 0x0017C354 File Offset: 0x0017A554
		public static void DerefIfByRef(this Emit il, ref Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type.IsByRef)
			{
				type = type.GetElementType();
				if (type.IsValueType)
				{
					il.LoadObject(type, false, null);
					return;
				}
				il.LoadIndirect(type, false, null);
			}
		}

		// Token: 0x06003904 RID: 14596 RVA: 0x0017C3B8 File Offset: 0x0017A5B8
		private static MethodInfo GetImplicitOperatorMethod(Type baseType, Type targetType)
		{
			MethodInfo result;
			try
			{
				result = Expression.Convert(Expression.Parameter(baseType, null), targetType).Method;
			}
			catch
			{
				if (baseType.BaseType != null)
				{
					result = SigilExtensions.GetImplicitOperatorMethod(baseType.BaseType, targetType);
				}
				else if (targetType.BaseType != null)
				{
					result = SigilExtensions.GetImplicitOperatorMethod(baseType, targetType.BaseType);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		// Token: 0x06003905 RID: 14597 RVA: 0x0017C42C File Offset: 0x0017A62C
		public static void LoadLocalAndCast(this Emit il, Local value, Type targetType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}
			if (value.LocalType != typeof(object))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Expected local type ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(object));
				defaultInterpolatedStringHandler.AppendLiteral("; got ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(value.LocalType);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear(), "value");
			}
			string guid = Guid.NewGuid().ToString("N");
			if (targetType.IsByRef)
			{
				targetType = targetType.GetElementType();
			}
			Local baseType = il2.DeclareLocal(typeof(Type), "cast_baseType_" + guid);
			il2.LoadLocal(value);
			il2.Call(typeof(object).GetMethod("GetType"), null);
			il2.StoreLocal(baseType);
			Local implicitOperatorMethod = il2.DeclareLocal(typeof(MethodInfo), "cast_implicitOperatorMethod_" + guid);
			il2.LoadLocal(baseType);
			il2.LoadType(targetType);
			il2.Call(typeof(SigilExtensions).GetMethod("GetImplicitOperatorMethod", BindingFlags.Static | BindingFlags.NonPublic), null);
			il2.StoreLocal(implicitOperatorMethod);
			Local castValue = il2.DeclareLocal(targetType, "cast_castValue_" + guid);
			il2.LoadLocal(implicitOperatorMethod);
			il2.Branch(delegate(Emit il)
			{
				Local methodInvokeParams = il.DeclareLocal(typeof(object[]), "cast_methodInvokeParams_" + guid);
				il.LoadConstant(1);
				il.NewArray(typeof(object));
				il.StoreLocal(methodInvokeParams);
				il.LoadLocal(methodInvokeParams);
				il.LoadConstant(0);
				il.LoadLocal(value);
				il.StoreElement<object>();
				il.LoadLocal(implicitOperatorMethod);
				il.LoadNull();
				il.LoadLocal(methodInvokeParams);
				il.Call(typeof(MethodInfo).GetMethod("Invoke", new Type[]
				{
					typeof(object),
					typeof(object[])
				}), null);
				if (targetType.IsValueType)
				{
					il.UnboxAny(targetType);
				}
				else
				{
					il.CastClass(targetType);
				}
				il.StoreLocal(castValue);
			}, delegate(Emit il)
			{
				il.LoadLocal(value);
				if (targetType.IsValueType)
				{
					il.UnboxAny(targetType);
				}
				else
				{
					il.CastClass(targetType);
				}
				il.StoreLocal(castValue);
			});
			il2.LoadLocal(castValue);
		}

		// Token: 0x06003906 RID: 14598 RVA: 0x0017C640 File Offset: 0x0017A840
		public static void FormatString(this Emit il, string format, params Local[] args)
		{
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			string guid = Guid.NewGuid().ToString("N");
			Type listType = typeof(List<>).MakeGenericType(new Type[]
			{
				typeof(object)
			});
			Local list = il.DeclareLocal(listType, "formatString_list_" + guid);
			il.NewObject(listType, Array.Empty<Type>());
			il.StoreLocal(list);
			foreach (Local arg in args)
			{
				il.LoadLocal(list);
				il.LoadLocal(arg);
				il.ToObject(arg.LocalType);
				il.CallVirtual(listType.GetMethod("Add", new Type[]
				{
					typeof(object)
				}), null, null);
			}
			Local arr = il.DeclareLocal<object[]>("formatString_arr_" + guid);
			il.LoadLocal(list);
			il.CallVirtual(listType.GetMethod("ToArray", new Type[0]), null, null);
			il.StoreLocal(arr);
			il.LoadConstant(format);
			il.LoadLocal(arr);
			il.Call(typeof(string).GetMethod("Format", new Type[]
			{
				typeof(string),
				typeof(object[])
			}), null);
		}

		// Token: 0x06003907 RID: 14599 RVA: 0x0017C7B4 File Offset: 0x0017A9B4
		public static void NewMessage(this Emit il, string message)
		{
			MethodInfo newMessage = typeof(DebugConsole).GetMethod("NewMessage", BindingFlags.Static | BindingFlags.Public, null, new Type[]
			{
				typeof(string),
				typeof(Color?),
				typeof(bool)
			}, null);
			il.LoadConstant(message);
			il.Call(typeof(Color).GetProperty("LightBlue", BindingFlags.Static | BindingFlags.Public).GetGetMethod(), null);
			il.LoadConstant(false);
			il.Call(newMessage, null);
		}

		// Token: 0x06003908 RID: 14600 RVA: 0x0017C844 File Offset: 0x0017AA44
		public static void NewMessage(this Emit il)
		{
			MethodInfo newMessage = typeof(DebugConsole).GetMethod("NewMessage", BindingFlags.Static | BindingFlags.Public, null, new Type[]
			{
				typeof(string),
				typeof(Color?),
				typeof(bool)
			}, null);
			il.Call(typeof(Color).GetProperty("LightBlue", BindingFlags.Static | BindingFlags.Public).GetGetMethod(), null);
			il.LoadConstant(false);
			il.Call(newMessage, null);
		}

		// Token: 0x06003909 RID: 14601 RVA: 0x0017C8CC File Offset: 0x0017AACC
		public static void ForEachEnumerable<T>(this Emit il, Local enumerable, Action<Emit, Local, Label> action)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (!typeof(IEnumerable<T>).IsAssignableFrom(enumerable.LocalType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Expected local type ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(IEnumerator<T>));
				defaultInterpolatedStringHandler.AppendLiteral("; got ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(enumerable.LocalType);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear(), "enumerable");
			}
			string guid = Guid.NewGuid().ToString("N");
			Local enumerator = il.DeclareLocal<IEnumerator<T>>("forEachEnumerable_enumerator_" + guid);
			il.LoadLocal(enumerable);
			il.CallVirtual(typeof(IEnumerable<T>).GetMethod("GetEnumerator"), null, null);
			il.StoreLocal(enumerator);
			il.ForEachEnumerator(enumerator, action);
		}

		// Token: 0x0600390A RID: 14602 RVA: 0x0017C9C4 File Offset: 0x0017ABC4
		public static void ForEachEnumerator<T>(this Emit il, Local enumerator, Action<Emit, Local, Label> action)
		{
			if (enumerator == null)
			{
				throw new ArgumentNullException("enumerator");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (!typeof(IEnumerator<T>).IsAssignableFrom(enumerator.LocalType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Expected local type ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(IEnumerator<T>));
				defaultInterpolatedStringHandler.AppendLiteral("; got ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(enumerator.LocalType);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear(), "enumerator");
			}
			string guid = Guid.NewGuid().ToString("N");
			Label labelLoopStart = il.DefineLabel("forEach_loopStart_" + guid);
			Label labelMoveNext = il.DefineLabel("forEach_moveNext_" + guid);
			Label labelLeave = il.DefineLabel("forEach_leave_" + guid);
			ExceptionBlock exceptionBlock;
			il.BeginExceptionBlock(out exceptionBlock);
			il.Branch(labelMoveNext);
			il.MarkLabel(labelLoopStart);
			Local current = il.DeclareLocal<T>("forEachEnumerator_current_" + guid);
			il.LoadLocal(enumerator);
			il.CallVirtual(enumerator.LocalType.GetProperty("Current").GetGetMethod(), null, null);
			il.StoreLocal(current);
			action(il, current, labelLeave);
			il.MarkLabel(labelMoveNext);
			il.LoadLocal(enumerator);
			il.CallVirtual(typeof(IEnumerator).GetMethod("MoveNext"), null, null);
			il.BranchIfTrue(labelLoopStart);
			FinallyBlock finallyBlock;
			il.BeginFinallyBlock(exceptionBlock, out finallyBlock);
			il.LoadLocal(enumerator);
			il.CallVirtual(typeof(IDisposable).GetMethod("Dispose"), null, null);
			il.EndFinallyBlock(finallyBlock);
			il.EndExceptionBlock(exceptionBlock);
			il.MarkLabel(labelLeave);
		}

		// Token: 0x0600390B RID: 14603 RVA: 0x0017CB90 File Offset: 0x0017AD90
		public static void If(this Emit il, Action<Emit> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			il.Branch(action, null);
		}

		// Token: 0x0600390C RID: 14604 RVA: 0x0017CBA8 File Offset: 0x0017ADA8
		public static void IfNot(this Emit il, Action<Emit> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			il.Branch(null, action);
		}

		// Token: 0x0600390D RID: 14605 RVA: 0x0017CBC0 File Offset: 0x0017ADC0
		public static void Branch(this Emit il, Action<Emit> @if = null, Action<Emit> @else = null)
		{
			if (@if == null && @else == null)
			{
				throw new ArgumentException("At least one of the two branches must be defined.");
			}
			string guid = Guid.NewGuid().ToString("N");
			Label labelEnd = il.DefineLabel("branch_end_" + guid);
			if (@if != null && @else != null)
			{
				Label labelElse = il.DefineLabel("branch_else_" + guid);
				il.BranchIfFalse(labelElse);
				@if(il);
				il.Branch(labelEnd);
				il.MarkLabel(labelElse);
				@else(il);
			}
			else if (@if != null)
			{
				il.BranchIfFalse(labelEnd);
				@if(il);
			}
			else
			{
				il.BranchIfTrue(labelEnd);
				@else(il);
			}
			il.MarkLabel(labelEnd);
		}
	}
}
