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
	// Token: 0x02000354 RID: 852
	[NullableContext(1)]
	[Nullable(0)]
	public static class StructSerialization
	{
		// Token: 0x06004263 RID: 16995 RVA: 0x0024D2DC File Offset: 0x0024B4DC
		private static bool ShouldSkip(this FieldInfo field)
		{
			return field.GetCustomAttribute<StructSerialization.SkipAttribute>() != null;
		}

		// Token: 0x06004264 RID: 16996 RVA: 0x0024D2E7 File Offset: 0x0024B4E7
		[return: Nullable(2)]
		private static StructSerialization.HandlerAttribute ExtractHandler(this FieldInfo field)
		{
			return field.GetCustomAttribute<StructSerialization.HandlerAttribute>();
		}

		// Token: 0x06004266 RID: 16998 RVA: 0x0024D388 File Offset: 0x0024B588
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

		// Token: 0x06004267 RID: 16999 RVA: 0x0024D410 File Offset: 0x0024B610
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

		// Token: 0x06004268 RID: 17000 RVA: 0x0024D478 File Offset: 0x0024B678
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

		// Token: 0x06004269 RID: 17001 RVA: 0x0024D54E File Offset: 0x0024B74E
		public static string DeserializeString(string str)
		{
			return str;
		}

		// Token: 0x0600426A RID: 17002 RVA: 0x0024D554 File Offset: 0x0024B754
		public static bool DeserializeBool(string str, bool defaultValue)
		{
			bool result;
			if (bool.TryParse(str, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x0600426B RID: 17003 RVA: 0x0024D570 File Offset: 0x0024B770
		public static float DeserializeFloat(string str, float defaultValue)
		{
			float result;
			if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x0600426C RID: 17004 RVA: 0x0024D594 File Offset: 0x0024B794
		public static int DeserializeInt32(string str, int defaultValue)
		{
			int result;
			if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x0600426D RID: 17005 RVA: 0x0024D5B8 File Offset: 0x0024B7B8
		public static Identifier DeserializeIdentifier(string str)
		{
			return str.ToIdentifier();
		}

		// Token: 0x0600426E RID: 17006 RVA: 0x0024D5C0 File Offset: 0x0024B7C0
		public static LanguageIdentifier DeserializeLanguageIdentifier(string str)
		{
			return str.ToLanguageIdentifier();
		}

		// Token: 0x0600426F RID: 17007 RVA: 0x0024D5C8 File Offset: 0x0024B7C8
		public static Color DeserializeColor(string str)
		{
			return XMLExtensions.ParseColor(str, true);
		}

		// Token: 0x06004270 RID: 17008 RVA: 0x0024D5D4 File Offset: 0x0024B7D4
		public static Enum DeserializeEnum(Type enumType, string str, Enum defaultValue)
		{
			object result;
			if (Enum.TryParse(enumType, str, out result))
			{
				return (Enum)result;
			}
			return defaultValue;
		}

		// Token: 0x06004271 RID: 17009 RVA: 0x0024D5F4 File Offset: 0x0024B7F4
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

		// Token: 0x06004272 RID: 17010 RVA: 0x0024D644 File Offset: 0x0024B844
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

		// Token: 0x06004273 RID: 17011 RVA: 0x0024D6E4 File Offset: 0x0024B8E4
		public static string SerializeBool(bool val)
		{
			if (!val)
			{
				return "false";
			}
			return "true";
		}

		// Token: 0x06004274 RID: 17012 RVA: 0x0024D6F4 File Offset: 0x0024B8F4
		public static string SerializeInt32(int val)
		{
			return val.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06004275 RID: 17013 RVA: 0x0024D702 File Offset: 0x0024B902
		public static string SerializeFloat(float val)
		{
			return val.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x06004276 RID: 17014 RVA: 0x0024D710 File Offset: 0x0024B910
		public static string SerializeColor(Color val)
		{
			return val.ToStringHex();
		}

		// Token: 0x0400228F RID: 8847
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

		// Token: 0x04002290 RID: 8848
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

		// Token: 0x0200106C RID: 4204
		[NullableContext(0)]
		public class SkipAttribute : Attribute
		{
		}

		// Token: 0x0200106D RID: 4205
		[NullableContext(0)]
		public class HandlerAttribute : Attribute
		{
			// Token: 0x06008C95 RID: 35989 RVA: 0x003AFD14 File Offset: 0x003ADF14
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

			// Token: 0x0400587E RID: 22654
			[Nullable(new byte[]
			{
				1,
				2,
				2
			})]
			public readonly Func<string, object> Read;

			// Token: 0x0400587F RID: 22655
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
