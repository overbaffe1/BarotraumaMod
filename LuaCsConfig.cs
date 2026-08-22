using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002EE RID: 750
	internal class LuaCsConfig
	{
		// Token: 0x06003DBA RID: 15802 RVA: 0x002307DC File Offset: 0x0022E9DC
		private static Type[] LoadDocTypes(XElement typesElem)
		{
			List<Type> result = new List<Type>();
			ImmutableArray<Type> loadedTypes = (from alc in AssemblyLoadContext.All
			where alc != AssemblyLoadContext.Default
			select alc).SelectMany((AssemblyLoadContext alc) => alc.Assemblies).SelectMany((Assembly asm) => asm.GetTypes()).ToImmutableArray<Type>();
			using (IEnumerator<XElement> enumerator = typesElem.Elements().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XElement elem = enumerator.Current;
					ImmutableList<Type> typesFound = loadedTypes.Where(delegate(Type t)
					{
						string fullName = t.FullName;
						return fullName != null && fullName.EndsWith(elem.Value);
					}).ToImmutableList<Type>();
					if (!typesFound.Any<Type>())
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 3);
						defaultInterpolatedStringHandler.AppendFormatted("LuaCsConfig");
						defaultInterpolatedStringHandler.AppendLiteral("::");
						defaultInterpolatedStringHandler.AppendFormatted("LoadDocTypes");
						defaultInterpolatedStringHandler.AppendLiteral("() | Unable to find a matching type for ");
						defaultInterpolatedStringHandler.AppendFormatted(elem.Value);
						ModUtils.Logging.PrintError(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						result.AddRange(typesFound);
					}
				}
			}
			return result.ToArray();
		}

		// Token: 0x06003DBB RID: 15803 RVA: 0x0023093C File Offset: 0x0022EB3C
		private static IEnumerable<XElement> SaveDocTypes(IEnumerable<Type> types)
		{
			return from t in types
			select new XElement("Type", t.ToString());
		}

		// Token: 0x06003DBC RID: 15804 RVA: 0x00230964 File Offset: 0x0022EB64
		private static Type GetTypeAttr(Type[] types, XElement elem)
		{
			int idx = elem.GetAttributeInt("Type", -1);
			if (idx < 0 || idx >= types.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Type index '");
				defaultInterpolatedStringHandler.AppendFormatted<int>(idx);
				defaultInterpolatedStringHandler.AppendLiteral("' is outside of saved types bounds");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return types[idx];
		}

		// Token: 0x06003DBD RID: 15805 RVA: 0x002309C4 File Offset: 0x0022EBC4
		private static LuaCsConfig.ValueType GetValueType(XElement elem)
		{
			Type typeFromHandle = typeof(LuaCsConfig.ValueType);
			XAttribute xattribute = elem.Attribute("Value");
			object result;
			Enum.TryParse(typeFromHandle, (xattribute != null) ? xattribute.Value : null, out result);
			if (result != null)
			{
				return (LuaCsConfig.ValueType)result;
			}
			return LuaCsConfig.ValueType.None;
		}

		// Token: 0x06003DBE RID: 15806 RVA: 0x00230A0C File Offset: 0x0022EC0C
		private static object ParseValue(Type[] types, XElement elem)
		{
			LuaCsConfig.ValueType type = LuaCsConfig.GetValueType(elem);
			if (elem.IsEmpty)
			{
				return null;
			}
			if (type == LuaCsConfig.ValueType.Enum)
			{
				Type tType = LuaCsConfig.GetTypeAttr(types, elem);
				if (tType == null || !tType.IsSubclassOf(typeof(Enum)))
				{
					return null;
				}
				object result;
				if (Enum.TryParse(tType, elem.Value, out result))
				{
					return result;
				}
				return null;
			}
			else
			{
				if (type == LuaCsConfig.ValueType.Collection)
				{
					Type tType2 = LuaCsConfig.GetTypeAttr(types, elem);
					Type tInt = tType2.GetInterfaces().FirstOrDefault((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
					Type gArg = tInt.GetGenericArguments()[0];
					if (!(tType2 == null))
					{
						if (tType2.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
						{
							object result2 = null;
							if (result2 == null)
							{
								ConstructorInfo ctor = tType2.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(delegate(ConstructorInfo c)
								{
									ParameterInfo[] param = c.GetParameters();
									if (param.Count<ParameterInfo>() == 1)
									{
										return param.Any((ParameterInfo p) => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
									}
									return false;
								});
								if (ctor != null)
								{
									IEnumerable<object> elements = from x in elem.Elements()
									select LuaCsConfig.ParseValue(types, x);
									object castElems = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(new Type[]
									{
										gArg
									}).Invoke(elements, new object[]
									{
										elements
									});
									result2 = ctor.Invoke(new object[]
									{
										castElems
									});
								}
							}
							if (result2 == null)
							{
								ConstructorInfo ctor2 = tType2.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((ConstructorInfo c) => c.GetParameters().Count<ParameterInfo>() == 0);
								MethodInfo addMethod = tType2.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo m)
								{
									if (m.Name != "Add")
									{
										return false;
									}
									ParameterInfo[] param = m.GetParameters();
									return param.Count<ParameterInfo>() == 1 && param[0].ParameterType == gArg;
								});
								if (ctor2 != null && addMethod != null)
								{
									IEnumerable<object> elements2 = from x in elem.Elements()
									select LuaCsConfig.ParseValue(types, x);
									result2 = ctor2.Invoke(null);
									foreach (object el3 in elements2)
									{
										addMethod.Invoke(result2, new object[]
										{
											el3
										});
									}
								}
							}
							if (result2 == null)
							{
								ConstructorInfo ctor3 = tType2.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault<ConstructorInfo>();
								MethodInfo setMethod = tType2.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo m)
								{
									if (m.Name != "Set")
									{
										return false;
									}
									ParameterInfo[] param = m.GetParameters();
									return param.Count<ParameterInfo>() == 2 && param[0].ParameterType == typeof(int) && param[1].ParameterType == gArg;
								});
								if (ctor3 != null || setMethod != null)
								{
									IEnumerable<object> elements3 = from x in elem.Elements()
									select LuaCsConfig.ParseValue(types, x);
									result2 = ctor3.Invoke(new object[]
									{
										elements3.Count<object>()
									});
									int j = 0;
									foreach (object el2 in elements3)
									{
										setMethod.Invoke(result2, new object[]
										{
											j,
											el2
										});
										j++;
									}
								}
							}
							return result2;
						}
					}
					return null;
				}
				if (type == LuaCsConfig.ValueType.Text)
				{
					return elem.Value;
				}
				if (type == LuaCsConfig.ValueType.Integer)
				{
					int num;
					int.TryParse(elem.Value, out num);
					return num;
				}
				if (type == LuaCsConfig.ValueType.Decimal)
				{
					float num2;
					float.TryParse(elem.Value, out num2);
					return num2;
				}
				if (type == LuaCsConfig.ValueType.Boolean)
				{
					bool boolean;
					bool.TryParse(elem.Value, out boolean);
					return boolean;
				}
				if (type != LuaCsConfig.ValueType.Object)
				{
					return elem.Value;
				}
				Type tType3 = LuaCsConfig.GetTypeAttr(types, elem);
				if (tType3 == null)
				{
					return null;
				}
				IEnumerable<FieldInfo> fields = tType3.GetFields(BindingFlags.Instance | BindingFlags.Public).Concat(tType3.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
				IEnumerable<PropertyInfo> properties = (from p in tType3.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				where p.GetSetMethod() != null
				select p).Concat(from p in tType3.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
				where p.GetSetMethod() != null
				select p);
				object result3 = null;
				ConstructorInfo ctor4 = tType3.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((ConstructorInfo c) => c.GetParameters().Count<ParameterInfo>() == 0);
				if (ctor4 == null)
				{
					if (!tType3.IsValueType)
					{
						return null;
					}
					result3 = Activator.CreateInstance(tType3);
				}
				else
				{
					result3 = ctor4.Invoke(null);
				}
				using (IEnumerator<XElement> enumerator3 = elem.Elements().GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						XElement el = enumerator3.Current;
						object value = LuaCsConfig.ParseValue(types, el);
						FieldInfo field = fields.FirstOrDefault((FieldInfo f) => f.Name == el.Name.LocalName);
						if (field != null)
						{
							field.SetValue(result3, value);
						}
						PropertyInfo property = properties.FirstOrDefault((PropertyInfo p) => p.Name == el.Name.LocalName);
						if (property != null)
						{
							property.SetValue(result3, value);
						}
					}
				}
				return result3;
			}
		}

		// Token: 0x06003DBF RID: 15807 RVA: 0x00230F84 File Offset: 0x0022F184
		private static void AddTypeAttr(List<Type> types, Type type, XElement elem)
		{
			if (!types.Contains(type))
			{
				types.Add(type);
			}
			elem.SetAttributeValue("Type", types.IndexOf(type));
		}

		// Token: 0x06003DC0 RID: 15808 RVA: 0x00230FB4 File Offset: 0x0022F1B4
		private static XElement ParseObject(List<Type> types, string name, object value)
		{
			XElement result = new XElement(name);
			if (value != null)
			{
				Type tType = value.GetType();
				if (tType.IsEnum)
				{
					result.SetAttributeValue("Value", LuaCsConfig.ValueType.Enum);
					LuaCsConfig.AddTypeAttr(types, tType, result);
					result.Value = (Enum.GetName(tType, value) ?? "");
				}
				else
				{
					string str = value as string;
					if (str != null)
					{
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.Text);
						result.Value = str;
					}
					else if (value is int)
					{
						int integer = (int)value;
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.Integer);
						result.Value = integer.ToString();
					}
					else if (value is float || value is double)
					{
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.Decimal);
						result.Value = value.ToString();
					}
					else if (value is bool)
					{
						bool boolean = (bool)value;
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.Boolean);
						result.Value = boolean.ToString();
					}
					else if (tType.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
					{
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.Collection);
						LuaCsConfig.AddTypeAttr(types, tType, result);
						IEnumerator enumerator = (IEnumerator)tType.GetMethod("GetEnumerator").Invoke(value, null);
						while (enumerator.MoveNext())
						{
							object value2 = enumerator.Current;
							XElement elVal = LuaCsConfig.ParseObject(types, "Item", value2);
							result.Add(elVal);
						}
					}
					else
					{
						if (tType.IsClass || tType.IsValueType)
						{
							result.SetAttributeValue("Value", LuaCsConfig.ValueType.Object);
							LuaCsConfig.AddTypeAttr(types, tType, result);
							IEnumerable<FieldInfo> fields = tType.GetFields(BindingFlags.Instance | BindingFlags.Public).Concat(tType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
							IEnumerable<PropertyInfo> properties = (from p in tType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
							where p.GetSetMethod() != null
							select p).Concat(from p in tType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
							where p.GetSetMethod() != null
							select p);
							foreach (FieldInfo field in fields)
							{
								result.Add(LuaCsConfig.ParseObject(types, field.Name, field.GetValue(value)));
							}
							using (IEnumerator<PropertyInfo> enumerator3 = properties.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									PropertyInfo property = enumerator3.Current;
									result.Add(LuaCsConfig.ParseObject(types, property.Name, property.GetValue(value)));
								}
								return result;
							}
						}
						result.SetAttributeValue("Value", LuaCsConfig.ValueType.None);
						result.Value = value.ToString();
					}
				}
			}
			return result;
		}

		// Token: 0x06003DC1 RID: 15809 RVA: 0x002312F0 File Offset: 0x0022F4F0
		public static T Load<T>(FileStream file)
		{
			XDocument doc = XDocument.Load(file);
			XElement[] rootElems = doc.Root.Elements().ToArray<XElement>();
			XElement types = rootElems[0];
			XElement elem = rootElems[1];
			object dict = LuaCsConfig.ParseValue(LuaCsConfig.LoadDocTypes(types), elem);
			if (dict.GetType() == typeof(T))
			{
				return (T)((object)dict);
			}
			throw new Exception("Loaded configuration is not of the type '" + typeof(T).Name + "'");
		}

		// Token: 0x06003DC2 RID: 15810 RVA: 0x00231370 File Offset: 0x0022F570
		public static void Save(FileStream file, object obj)
		{
			List<Type> types = new List<Type>();
			XElement elem = LuaCsConfig.ParseObject(types, "Root", obj);
			XElement root = new XElement("Configuration", new object[]
			{
				new XElement("Types", LuaCsConfig.SaveDocTypes(types)),
				elem
			});
			XDocument doc = new XDocument(new object[]
			{
				root
			});
			doc.Save(file);
		}

		// Token: 0x06003DC3 RID: 15811 RVA: 0x002313DC File Offset: 0x0022F5DC
		public static T Load<T>(string path)
		{
			T result;
			using (FileStream file = LuaCsFile.OpenRead(path))
			{
				result = LuaCsConfig.Load<T>(file);
			}
			return result;
		}

		// Token: 0x06003DC4 RID: 15812 RVA: 0x00231414 File Offset: 0x0022F614
		public static void Save(string path, object obj)
		{
			using (FileStream file = LuaCsFile.OpenWrite(path))
			{
				LuaCsConfig.Save(file, obj);
			}
		}

		// Token: 0x02000F93 RID: 3987
		private enum ValueType
		{
			// Token: 0x04005612 RID: 22034
			None,
			// Token: 0x04005613 RID: 22035
			Text,
			// Token: 0x04005614 RID: 22036
			Integer,
			// Token: 0x04005615 RID: 22037
			Decimal,
			// Token: 0x04005616 RID: 22038
			Boolean,
			// Token: 0x04005617 RID: 22039
			Collection,
			// Token: 0x04005618 RID: 22040
			Object,
			// Token: 0x04005619 RID: 22041
			Enum
		}
	}
}
