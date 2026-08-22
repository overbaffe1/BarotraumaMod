using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000283 RID: 643
	[NullableContext(1)]
	[Nullable(0)]
	public static class StructSerialization
	{
		// Token: 0x06002D51 RID: 11601 RVA: 0x0012D1EC File Offset: 0x0012B3EC
		private static bool ShouldSkip(this FieldInfo field)
		{
			return field.GetCustomAttribute<StructSerialization.SkipAttribute>() != null;
		}

		// Token: 0x06002D52 RID: 11602 RVA: 0x0012D1F7 File Offset: 0x0012B3F7
		[return: Nullable(2)]
		private static StructSerialization.HandlerAttribute ExtractHandler(this FieldInfo field)
		{
			return field.GetCustomAttribute<StructSerialization.HandlerAttribute>();
		}

		// Token: 0x06002D54 RID: 11604 RVA: 0x0012D298 File Offset: 0x0012B498
		[NullableContext(0)]
		public static void CopyPropertiesFrom<T>(this T self, in T other) where T : struct
		{
			FieldInfo[] fields = (from f in self.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
			where !f.IsInitOnly
			select f).ToArray<FieldInfo>();
			foreach (FieldInfo field in fields)
			{
				if (!field.ShouldSkip())
				{
					field.SetValue(self, field.GetValue(other));
				}
			}
		}

		// Token: 0x06002D55 RID: 11605 RVA: 0x0012D320 File Offset: 0x0012B520
		[NullableContext(0)]
		public static void DeserializeElement<T>(this T self, [Nullable(1)] XElement element) where T : struct
		{
			FieldInfo[] fields = self.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).ToArray<FieldInfo>();
			object boxedSelf = self;
			foreach (FieldInfo field in fields)
			{
				if (!field.ShouldSkip())
				{
					boxedSelf.TryDeserialize(field, element);
				}
			}
			self = (T)((object)boxedSelf);
		}

		// Token: 0x06002D56 RID: 11606 RVA: 0x0012D388 File Offset: 0x0012B588
		private static void TryDeserialize(this object boxedSelf, FieldInfo field, XElement element)
		{
			string fieldName = field.Name.ToLowerInvariant();
			string name = fieldName;
			object value = field.GetValue(boxedSelf);
			string valueStr = element.GetAttributeString(name, ((value != null) ? value.ToString() : null) ?? "");
			StructSerialization.HandlerAttribute handler = field.ExtractHandler();
			if (handler != null)
			{
				field.SetValue(boxedSelf, handler.Read(valueStr));
				return;
			}
			MethodInfo deserializeMethod;
			if (StructSerialization.deserializeMethods.TryGetValue(field.FieldType, out deserializeMethod))
			{
				object[] parameters = new object[]
				{
					valueStr
				};
				if (deserializeMethod.GetParameters().Length > 1)
				{
					Array.Resize<object>(ref parameters, 2);
					parameters[1] = field.GetValue(boxedSelf);
				}
				field.SetValue(boxedSelf, deserializeMethod.Invoke(boxedSelf, parameters));
				return;
			}
			if (field.FieldType.IsEnum)
			{
				field.SetValue(boxedSelf, StructSerialization.DeserializeEnum(field.FieldType, valueStr, (Enum)field.GetValue(boxedSelf)));
			}
		}

		// Token: 0x06002D57 RID: 11607 RVA: 0x0012D45E File Offset: 0x0012B65E
		public static string DeserializeString(string str)
		{
			return str;
		}

		// Token: 0x06002D58 RID: 11608 RVA: 0x0012D464 File Offset: 0x0012B664
		public static bool DeserializeBool(string str, bool defaultValue)
		{
			bool result;
			if (bool.TryParse(str, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x06002D59 RID: 11609 RVA: 0x0012D480 File Offset: 0x0012B680
		public static float DeserializeFloat(string str, float defaultValue)
		{
			float result;
			if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x06002D5A RID: 11610 RVA: 0x0012D4A4 File Offset: 0x0012B6A4
		public static int DeserializeInt32(string str, int defaultValue)
		{
			int result;
			if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x06002D5B RID: 11611 RVA: 0x0012D4C8 File Offset: 0x0012B6C8
		public static Identifier DeserializeIdentifier(string str)
		{
			return str.ToIdentifier();
		}

		// Token: 0x06002D5C RID: 11612 RVA: 0x0012D4D0 File Offset: 0x0012B6D0
		public static LanguageIdentifier DeserializeLanguageIdentifier(string str)
		{
			return str.ToLanguageIdentifier();
		}

		// Token: 0x06002D5D RID: 11613 RVA: 0x0012D4D8 File Offset: 0x0012B6D8
		public static Color DeserializeColor(string str)
		{
			return XMLExtensions.ParseColor(str, true);
		}

		// Token: 0x06002D5E RID: 11614 RVA: 0x0012D4E4 File Offset: 0x0012B6E4
		public static Enum DeserializeEnum(Type enumType, string str, Enum defaultValue)
		{
			object result;
			if (Enum.TryParse(enumType, str, out result))
			{
				return (Enum)result;
			}
			return defaultValue;
		}

		// Token: 0x06002D5F RID: 11615 RVA: 0x0012D504 File Offset: 0x0012B704
		[NullableContext(0)]
		public static void SerializeElement<T>(this T self, [Nullable(1)] XElement element) where T : struct
		{
			FieldInfo[] fields = self.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).ToArray<FieldInfo>();
			foreach (FieldInfo field in fields)
			{
				if (!field.ShouldSkip())
				{
					self.TrySerialize(field, element);
				}
			}
		}

		// Token: 0x06002D60 RID: 11616 RVA: 0x0012D554 File Offset: 0x0012B754
		[NullableContext(0)]
		public static void TrySerialize<T>(this T self, [Nullable(1)] FieldInfo field, [Nullable(1)] XElement element) where T : struct
		{
			string fieldName = field.Name.ToLowerInvariant();
			object fieldValue = field.GetValue(self);
			string valueStr = ((fieldValue != null) ? fieldValue.ToString() : null) ?? "";
			StructSerialization.HandlerAttribute handler = field.ExtractHandler();
			MethodInfo method;
			if (handler != null)
			{
				valueStr = (handler.Write(valueStr) ?? "");
			}
			else if (StructSerialization.serializeMethods.TryGetValue(field.FieldType, out method))
			{
				object[] parameters = new object[]
				{
					fieldValue
				};
				valueStr = (string)method.Invoke(self, parameters);
			}
			element.SetAttributeValue(fieldName, valueStr);
		}

		// Token: 0x06002D61 RID: 11617 RVA: 0x0012D5F4 File Offset: 0x0012B7F4
		public static string SerializeBool(bool val)
		{
			if (!val)
			{
				return "false";
			}
			return "true";
		}

		// Token: 0x06002D62 RID: 11618 RVA: 0x0012D604 File Offset: 0x0012B804
		public static string SerializeInt32(int val)
		{
			return val.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06002D63 RID: 11619 RVA: 0x0012D612 File Offset: 0x0012B812
		public static string SerializeFloat(float val)
		{
			return val.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06002D64 RID: 11620 RVA: 0x0012D620 File Offset: 0x0012B820
		public static string SerializeColor(Color val)
		{
			return val.ToStringHex();
		}

		// Token: 0x0400164B RID: 5707
		private static readonly ImmutableDictionary<Type, MethodInfo> deserializeMethods = (from m in typeof(StructSerialization).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(delegate(MethodInfo m)
		{
			if (!m.Name.StartsWith("Deserialize"))
			{
				return false;
			}
			ParameterInfo[] parameters = m.GetParameters();
			return parameters.Length >= 1 && parameters.Length <= 2 && !(parameters[0].ParameterType != typeof(string));
		})
		select new ValueTuple<Type, MethodInfo>(m.ReturnType, m)).ToImmutableDictionary<Type, MethodInfo>();

		// Token: 0x0400164C RID: 5708
		private static readonly ImmutableDictionary<Type, MethodInfo> serializeMethods = (from m in typeof(StructSerialization).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(delegate(MethodInfo m)
		{
			if (!m.Name.StartsWith("Serialize"))
			{
				return false;
			}
			ParameterInfo[] parameters = m.GetParameters();
			return parameters.Length == 1 && !(m.ReturnType != typeof(string));
		})
		select new ValueTuple<Type, MethodInfo>(m.GetParameters()[0].ParameterType, m)).ToImmutableDictionary<Type, MethodInfo>();

		// Token: 0x02000AF4 RID: 2804
		[NullableContext(0)]
		public class SkipAttribute : Attribute
		{
		}

		// Token: 0x02000AF5 RID: 2805
		[NullableContext(0)]
		public class HandlerAttribute : Attribute
		{
			// Token: 0x06005F07 RID: 24327 RVA: 0x002064FC File Offset: 0x002046FC
			[NullableContext(1)]
			public HandlerAttribute(Type handlerType)
			{
				StructSerialization.HandlerAttribute.<>c__DisplayClass2_0 CS$<>8__locals1 = new StructSerialization.HandlerAttribute.<>c__DisplayClass2_0();
				StructSerialization.HandlerAttribute.<>c__DisplayClass2_0 CS$<>8__locals2 = CS$<>8__locals1;
				MethodInfo method = handlerType.GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
				if (method == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Type ");
					defaultInterpolatedStringHandler.AppendFormatted(handlerType.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" does not have a static ");
					defaultInterpolatedStringHandler.AppendFormatted("Read");
					defaultInterpolatedStringHandler.AppendLiteral(" method");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				CS$<>8__locals2.readAction = method;
				StructSerialization.HandlerAttribute.<>c__DisplayClass2_0 CS$<>8__locals3 = CS$<>8__locals1;
				MethodInfo method2 = handlerType.GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
				if (method2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Type ");
					defaultInterpolatedStringHandler2.AppendFormatted(handlerType.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" does not have a static ");
					defaultInterpolatedStringHandler2.AppendFormatted("Write");
					defaultInterpolatedStringHandler2.AppendLiteral(" method");
					throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				CS$<>8__locals3.writeAction = method2;
				CS$<>8__locals1.paramArray = new object[1];
				this.Read = delegate(string s)
				{
					CS$<>8__locals1.paramArray[0] = s;
					return CS$<>8__locals1.readAction.Invoke(null, CS$<>8__locals1.paramArray);
				};
				this.Write = delegate(object o)
				{
					CS$<>8__locals1.paramArray[0] = o;
					object obj = CS$<>8__locals1.writeAction.Invoke(null, CS$<>8__locals1.paramArray);
					if (obj == null)
					{
						return null;
					}
					return obj.ToString();
				};
			}

			// Token: 0x040037D7 RID: 14295
			[Nullable(new byte[]
			{
				1,
				2,
				2
			})]
			public readonly Func<string, object> Read;

			// Token: 0x040037D8 RID: 14296
			[Nullable(new byte[]
			{
				1,
				2,
				2
			})]
			public readonly Func<object, string> Write;
		}
	}
}
