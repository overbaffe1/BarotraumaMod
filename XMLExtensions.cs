using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000355 RID: 853
	public static class XMLExtensions
	{
		// Token: 0x06004277 RID: 17015 RVA: 0x0024D718 File Offset: 0x0024B918
		public static bool DefaultValueEquals(object defaultValue, object value)
		{
			if (defaultValue != null)
			{
				string valueAsString = value as string;
				Type type;
				if (valueAsString != null && XMLExtensions.Converters.TryGetKey(defaultValue.GetType(), out type))
				{
					return object.Equals(XMLExtensions.Converters[type](valueAsString, defaultValue), defaultValue);
				}
			}
			if (value != null)
			{
				string defaultValueAsString = defaultValue as string;
				Type type2;
				if (defaultValueAsString != null && XMLExtensions.Converters.TryGetKey(value.GetType(), out type2))
				{
					return object.Equals(XMLExtensions.Converters[type2](defaultValueAsString, value), value);
				}
			}
			return object.Equals(value, defaultValue);
		}

		// Token: 0x06004278 RID: 17016 RVA: 0x0024D7A0 File Offset: 0x0024B9A0
		public static string ParseContentPathFromUri(this XObject element)
		{
			if (string.IsNullOrWhiteSpace(element.BaseUri))
			{
				return "";
			}
			return Path.GetRelativePath(Environment.CurrentDirectory, element.BaseUri.CleanUpPath());
		}

		// Token: 0x06004279 RID: 17017 RVA: 0x0024D7CA File Offset: 0x0024B9CA
		public static XmlReader CreateReader(Stream stream, string baseUri = "")
		{
			return XmlReader.Create(stream, XMLExtensions.ReaderSettings, baseUri);
		}

		// Token: 0x0600427A RID: 17018 RVA: 0x0024D7D8 File Offset: 0x0024B9D8
		public static XDocument TryLoadXml(Stream stream)
		{
			XDocument doc;
			try
			{
				using (XmlReader reader = XMLExtensions.CreateReader(stream, ""))
				{
					doc = XDocument.Load(reader);
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Couldn't load xml document from stream!", e, null, false, false);
				return null;
			}
			if (((doc != null) ? doc.Root : null) == null)
			{
				DebugConsole.ThrowError("XML could not be loaded from stream: Document or the root element is invalid!", null, null, false, false);
				return null;
			}
			return doc;
		}

		// Token: 0x0600427B RID: 17019 RVA: 0x0024D858 File Offset: 0x0024BA58
		public static XDocument TryLoadXml(ContentPath path)
		{
			return XMLExtensions.TryLoadXml(path.Value);
		}

		// Token: 0x0600427C RID: 17020 RVA: 0x0024D868 File Offset: 0x0024BA68
		public static XDocument TryLoadXml(string filePath)
		{
			Exception exception;
			XDocument doc = XMLExtensions.TryLoadXml(filePath, out exception);
			if (exception != null)
			{
				DebugConsole.ThrowError("Couldn't load xml document \"" + filePath + "\"!", exception, null, false, false);
			}
			else if (doc == null)
			{
				DebugConsole.ThrowError("File \"" + filePath + "\" could not be loaded: Document or the root element is invalid!", null, null, false, false);
			}
			return doc;
		}

		// Token: 0x0600427D RID: 17021 RVA: 0x0024D8BC File Offset: 0x0024BABC
		public static XDocument TryLoadXml(string filePath, out Exception exception)
		{
			exception = null;
			XDocument doc;
			try
			{
				ToolBox.IsProperFilenameCase(filePath);
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, null, false))
				{
					using (XmlReader reader = XMLExtensions.CreateReader(stream, Path.GetFullPath(filePath)))
					{
						doc = XDocument.Load(reader, LoadOptions.SetBaseUri);
					}
				}
			}
			catch (Exception e)
			{
				exception = e;
				return null;
			}
			if (((doc != null) ? doc.Root : null) == null)
			{
				return null;
			}
			return doc;
		}

		// Token: 0x0600427E RID: 17022 RVA: 0x0024D95C File Offset: 0x0024BB5C
		public static object GetAttributeObject(XAttribute attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			return XMLExtensions.ParseToObject(attribute.Value.ToString());
		}

		// Token: 0x0600427F RID: 17023 RVA: 0x0024D974 File Offset: 0x0024BB74
		public static object ParseToObject(string value)
		{
			float floatVal;
			if (value.Contains(".") && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out floatVal))
			{
				return floatVal;
			}
			int intVal;
			if (int.TryParse(value, out intVal))
			{
				return intVal;
			}
			string lowerTrimmedVal = value.ToLowerInvariant().Trim();
			if (lowerTrimmedVal == "true")
			{
				return true;
			}
			if (lowerTrimmedVal == "false")
			{
				return false;
			}
			return value;
		}

		// Token: 0x06004280 RID: 17024 RVA: 0x0024D9F0 File Offset: 0x0024BBF0
		public static string GetAttributeString(this XElement element, string name, string defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.GetAttributeString(attribute, defaultValue);
		}

		// Token: 0x06004281 RID: 17025 RVA: 0x0024DA1C File Offset: 0x0024BC1C
		public static string GetAttributeStringUnrestricted(this XElement element, string name, string defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.GetAttributeString(attribute, defaultValue);
		}

		// Token: 0x06004282 RID: 17026 RVA: 0x0024DA44 File Offset: 0x0024BC44
		public static bool DoesAttributeReferenceFileNameAlone(this XElement element, string name)
		{
			string texName = element.GetAttributeStringUnrestricted(name, "");
			return (!texName.IsNullOrEmpty() & !texName.Contains("/")) && !texName.Contains("%ModDir", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06004283 RID: 17027 RVA: 0x0024DA8C File Offset: 0x0024BC8C
		public static ContentPath GetAttributeContentPath(this XElement element, string name, ContentPackage contentPackage)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return null;
			}
			return ContentPath.FromRaw(contentPackage, XMLExtensions.GetAttributeString(attribute, null));
		}

		// Token: 0x06004284 RID: 17028 RVA: 0x0024DABA File Offset: 0x0024BCBA
		public static Identifier GetAttributeIdentifier(this XElement element, string name, string defaultValue)
		{
			return element.GetAttributeString(name, defaultValue).ToIdentifier();
		}

		// Token: 0x06004285 RID: 17029 RVA: 0x0024DAC9 File Offset: 0x0024BCC9
		public static Identifier GetAttributeIdentifier(this XElement element, string name, Identifier defaultValue)
		{
			return element.GetAttributeIdentifier(name, defaultValue.Value);
		}

		// Token: 0x06004286 RID: 17030 RVA: 0x0024DADC File Offset: 0x0024BCDC
		private static string GetAttributeString(XAttribute attribute, string defaultValue)
		{
			string value = attribute.Value;
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			return defaultValue;
		}

		// Token: 0x06004287 RID: 17031 RVA: 0x0024DAFC File Offset: 0x0024BCFC
		public static string[] GetAttributeStringArray(this XElement element, string name, string[] defaultValue, bool trim = true, bool convertToLowerInvariant = false)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			string[] splitValue = stringValue.Split(new char[]
			{
				',',
				'，'
			});
			for (int i = 0; i < splitValue.Length; i++)
			{
				if (convertToLowerInvariant)
				{
					splitValue[i] = splitValue[i].ToLowerInvariant();
				}
				if (trim)
				{
					splitValue[i] = splitValue[i].Trim();
				}
			}
			return splitValue;
		}

		// Token: 0x06004288 RID: 17032 RVA: 0x0024DB74 File Offset: 0x0024BD74
		public static Identifier[] GetAttributeIdentifierArray(this XElement element, Identifier[] defaultValue, params string[] matchingAttributeName)
		{
			if (element == null)
			{
				return defaultValue;
			}
			foreach (string name in matchingAttributeName)
			{
				Identifier[] value = element.GetAttributeIdentifierArray(name, defaultValue, true);
				if (value != defaultValue)
				{
					return value;
				}
			}
			return defaultValue;
		}

		// Token: 0x06004289 RID: 17033 RVA: 0x0024DBAB File Offset: 0x0024BDAB
		public static Identifier[] GetAttributeIdentifierArray(this XElement element, string name, Identifier[] defaultValue, bool trim = true)
		{
			string[] attributeStringArray = element.GetAttributeStringArray(name, null, trim, false);
			return ((attributeStringArray != null) ? attributeStringArray.ToIdentifiers() : null) ?? defaultValue;
		}

		// Token: 0x0600428A RID: 17034 RVA: 0x0024DBC8 File Offset: 0x0024BDC8
		public static ImmutableHashSet<Identifier> GetAttributeIdentifierImmutableHashSet(this XElement element, string key, ImmutableHashSet<Identifier> defaultValue, bool trim = true)
		{
			Identifier[] attributeIdentifierArray = element.GetAttributeIdentifierArray(key, null, trim);
			return ((attributeIdentifierArray != null) ? attributeIdentifierArray.ToImmutableHashSet<Identifier>() : null) ?? defaultValue;
		}

		// Token: 0x0600428B RID: 17035 RVA: 0x0024DBE4 File Offset: 0x0024BDE4
		public static float? GetAttributeNullableFloat(this XElement element, string attributeName)
		{
			XAttribute attribute = element.GetAttribute(attributeName, StringComparison.OrdinalIgnoreCase);
			if (attribute != null)
			{
				return new float?(attribute.GetAttributeFloat(0f));
			}
			return null;
		}

		// Token: 0x0600428C RID: 17036 RVA: 0x0024DC18 File Offset: 0x0024BE18
		public static float GetAttributeFloat(this XElement element, float defaultValue, params string[] matchingAttributeName)
		{
			if (element == null)
			{
				return defaultValue;
			}
			foreach (string name in matchingAttributeName)
			{
				XAttribute attribute = element.GetAttribute(name, StringComparison.OrdinalIgnoreCase);
				if (attribute != null)
				{
					return attribute.GetAttributeFloat(defaultValue);
				}
			}
			return defaultValue;
		}

		// Token: 0x0600428D RID: 17037 RVA: 0x0024DC53 File Offset: 0x0024BE53
		public static float GetAttributeFloat(this XElement element, string name, float defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeFloat(defaultValue);
		}

		// Token: 0x0600428E RID: 17038 RVA: 0x0024DC6C File Offset: 0x0024BE6C
		public static float GetAttributeFloat(this XAttribute attribute, float defaultValue)
		{
			if (attribute == null)
			{
				return defaultValue;
			}
			float val = defaultValue;
			try
			{
				string strVal = attribute.Value;
				if (strVal.LastOrDefault<char>() == 'f')
				{
					strVal = strVal.Substring(0, strVal.Length - 1);
				}
				val = float.Parse(strVal, CultureInfo.InvariantCulture);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error in " + ((attribute != null) ? attribute.ToString() : null) + "! ", e, null, false, false);
			}
			return val;
		}

		// Token: 0x0600428F RID: 17039 RVA: 0x0024DCE8 File Offset: 0x0024BEE8
		public static double GetAttributeDouble(this XElement element, string name, double defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeDouble(defaultValue);
		}

		// Token: 0x06004290 RID: 17040 RVA: 0x0024DD00 File Offset: 0x0024BF00
		public static double GetAttributeDouble(this XAttribute attribute, double defaultValue)
		{
			if (attribute == null)
			{
				return defaultValue;
			}
			double val = defaultValue;
			try
			{
				string strVal = attribute.Value;
				if (strVal.LastOrDefault<char>() == 'f')
				{
					strVal = strVal.Substring(0, strVal.Length - 1);
				}
				val = double.Parse(strVal, CultureInfo.InvariantCulture);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error in " + ((attribute != null) ? attribute.ToString() : null) + "!", e, null, false, false);
			}
			return val;
		}

		// Token: 0x06004291 RID: 17041 RVA: 0x0024DD7C File Offset: 0x0024BF7C
		public static float[] GetAttributeFloatArray(this XElement element, string name, float[] defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			string[] splitValue = stringValue.Split(',', StringSplitOptions.None);
			float[] floatValue = new float[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				try
				{
					string strVal = splitValue[i];
					if (strVal.LastOrDefault<char>() == 'f')
					{
						strVal = strVal.Substring(0, strVal.Length - 1);
					}
					floatValue[i] = float.Parse(strVal, CultureInfo.InvariantCulture);
				}
				catch (Exception e)
				{
					XMLExtensions.LogAttributeError(attribute, element, e);
				}
			}
			return floatValue;
		}

		// Token: 0x06004292 RID: 17042 RVA: 0x0024DE28 File Offset: 0x0024C028
		public static bool TryGetAttributeInt(this XElement element, string name, out int result)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			result = 0;
			if (attribute == null)
			{
				return false;
			}
			int intVal;
			if (int.TryParse(attribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out intVal))
			{
				result = intVal;
				return true;
			}
			float floatVal;
			if (float.TryParse(attribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatVal))
			{
				result = (int)floatVal;
				return true;
			}
			return false;
		}

		// Token: 0x06004293 RID: 17043 RVA: 0x0024DE8C File Offset: 0x0024C08C
		public static int? GetAttributeNullableInt(this XElement element, string attributeName)
		{
			XAttribute attribute = element.GetAttribute(attributeName, StringComparison.OrdinalIgnoreCase);
			if (attribute != null)
			{
				return new int?(attribute.GetAttributeInt(0));
			}
			return null;
		}

		// Token: 0x06004294 RID: 17044 RVA: 0x0024DEBB File Offset: 0x0024C0BB
		public static int GetAttributeInt(this XElement element, string name, int defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeInt(defaultValue);
		}

		// Token: 0x06004295 RID: 17045 RVA: 0x0024DED4 File Offset: 0x0024C0D4
		public static int GetAttributeInt(this XAttribute attribute, int defaultValue)
		{
			if (attribute == null)
			{
				return defaultValue;
			}
			int val = defaultValue;
			try
			{
				if (!int.TryParse(attribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
				{
					val = (int)float.Parse(attribute.Value, CultureInfo.InvariantCulture);
				}
			}
			catch (Exception e)
			{
				XMLExtensions.LogAttributeError(attribute, attribute.Parent, e);
			}
			return val;
		}

		// Token: 0x06004296 RID: 17046 RVA: 0x0024DF38 File Offset: 0x0024C138
		public static uint GetAttributeUInt(this XElement element, string name, uint defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			uint val = defaultValue;
			try
			{
				val = uint.Parse(attribute.Value);
			}
			catch (Exception e)
			{
				XMLExtensions.LogAttributeError(attribute, element, e);
			}
			return val;
		}

		// Token: 0x06004297 RID: 17047 RVA: 0x0024DF88 File Offset: 0x0024C188
		public static ushort GetAttributeUInt16(this XElement element, string name, ushort defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			ushort val = defaultValue;
			try
			{
				val = ushort.Parse(attribute.Value);
			}
			catch (Exception e)
			{
				XMLExtensions.LogAttributeError(attribute, element, e);
			}
			return val;
		}

		// Token: 0x06004298 RID: 17048 RVA: 0x0024DFD8 File Offset: 0x0024C1D8
		public static ulong GetAttributeUInt64(this XElement element, string name, ulong defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			ulong val = defaultValue;
			try
			{
				val = ulong.Parse(attribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			catch (Exception e)
			{
				XMLExtensions.LogAttributeError(attribute, element, e);
			}
			return val;
		}

		// Token: 0x06004299 RID: 17049 RVA: 0x0024E030 File Offset: 0x0024C230
		public static Option<SerializableDateTime> GetAttributeDateTime(this XElement element, string name)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return Option<SerializableDateTime>.None();
			}
			string attrVal = attribute.Value;
			return SerializableDateTime.Parse(attrVal);
		}

		// Token: 0x0600429A RID: 17050 RVA: 0x0024E064 File Offset: 0x0024C264
		public static Version GetAttributeVersion(this XElement element, string name, Version defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			Version val = defaultValue;
			try
			{
				val = Version.Parse(attribute.Value);
			}
			catch (Exception e)
			{
				XMLExtensions.LogAttributeError(attribute, element, e);
			}
			return val;
		}

		// Token: 0x0600429B RID: 17051 RVA: 0x0024E0B4 File Offset: 0x0024C2B4
		public static int[] GetAttributeIntArray(this XElement element, string name, int[] defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			string[] splitValue = stringValue.Split(',', StringSplitOptions.None);
			int[] intValue = new int[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				try
				{
					int val = int.Parse(splitValue[i]);
					intValue[i] = val;
				}
				catch (Exception e)
				{
					XMLExtensions.LogAttributeError(attribute, element, e);
				}
			}
			return intValue;
		}

		// Token: 0x0600429C RID: 17052 RVA: 0x0024E140 File Offset: 0x0024C340
		public static ushort[] GetAttributeUshortArray(this XElement element, string name, ushort[] defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			string[] splitValue = stringValue.Split(',', StringSplitOptions.None);
			ushort[] ushortValue = new ushort[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				try
				{
					ushort val = ushort.Parse(splitValue[i]);
					ushortValue[i] = val;
				}
				catch (Exception e)
				{
					XMLExtensions.LogAttributeError(attribute, element, e);
				}
			}
			return ushortValue;
		}

		// Token: 0x0600429D RID: 17053 RVA: 0x0024E1CC File Offset: 0x0024C3CC
		public unsafe static T ParseEnumValue<T>(string value, T defaultValue, XAttribute attribute) where T : struct, Enum
		{
			T result;
			if (Enum.TryParse<T>(value, true, out result))
			{
				return result;
			}
			int resultInt;
			if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out resultInt))
			{
				return *Unsafe.As<int, T>(ref resultInt);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Error in ");
			defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(attribute);
			defaultInterpolatedStringHandler.AppendLiteral("! \"");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid ");
			defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
			defaultInterpolatedStringHandler.AppendLiteral(" value");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			return defaultValue;
		}

		// Token: 0x0600429E RID: 17054 RVA: 0x0024E278 File Offset: 0x0024C478
		public static T GetAttributeEnum<T>(this XElement element, string name, T defaultValue) where T : struct, Enum
		{
			XAttribute attr = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attr == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseEnumValue<T>(attr.Value, defaultValue, attr);
		}

		// Token: 0x0600429F RID: 17055 RVA: 0x0024E2A8 File Offset: 0x0024C4A8
		[return: NotNullIfNotNull("defaultValue")]
		public static T[] GetAttributeEnumArray<T>(this XElement element, string name, T[] defaultValue) where T : struct, Enum
		{
			string[] stringArray = element.GetAttributeStringArray(name, null, true, false);
			if (stringArray == null)
			{
				return defaultValue;
			}
			if (stringArray.Length == 0)
			{
				return new T[0];
			}
			T[] enumArray = new T[stringArray.Length];
			XAttribute attribute = element.GetAttribute(name, StringComparison.OrdinalIgnoreCase);
			for (int i = 0; i < stringArray.Length; i++)
			{
				try
				{
					enumArray[i] = XMLExtensions.ParseEnumValue<T>(stringArray[i].Trim(), default(T), attribute);
				}
				catch (Exception e)
				{
					XMLExtensions.LogAttributeError(attribute, element, e);
				}
			}
			return enumArray;
		}

		// Token: 0x060042A0 RID: 17056 RVA: 0x0024E330 File Offset: 0x0024C530
		public static bool GetAttributeBool(this XElement element, string name, bool defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return attribute.GetAttributeBool(defaultValue);
		}

		// Token: 0x060042A1 RID: 17057 RVA: 0x0024E358 File Offset: 0x0024C558
		public static bool GetAttributeBool(this XAttribute attribute, bool defaultValue)
		{
			if (attribute == null)
			{
				return defaultValue;
			}
			string val = attribute.Value.ToLowerInvariant().Trim();
			if (val == "true")
			{
				return true;
			}
			if (val == "false")
			{
				return false;
			}
			DebugConsole.ThrowError(string.Concat(new string[]
			{
				"Error in ",
				attribute.Value.ToString(),
				"! \"",
				val,
				"\" is not a valid boolean value"
			}), null, null, false, false);
			return false;
		}

		// Token: 0x060042A2 RID: 17058 RVA: 0x0024E3D8 File Offset: 0x0024C5D8
		public static Point GetAttributePoint(this XElement element, string name, Point defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParsePoint(attribute.Value, true);
		}

		// Token: 0x060042A3 RID: 17059 RVA: 0x0024E408 File Offset: 0x0024C608
		public static Vector2 GetAttributeVector2(this XElement element, string name, Vector2 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector2(attribute.Value, true);
		}

		// Token: 0x060042A4 RID: 17060 RVA: 0x0024E438 File Offset: 0x0024C638
		public static Vector3 GetAttributeVector3(this XElement element, string name, Vector3 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector3(attribute.Value, true);
		}

		// Token: 0x060042A5 RID: 17061 RVA: 0x0024E468 File Offset: 0x0024C668
		public static Vector4 GetAttributeVector4(this XElement element, string name, Vector4 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector4(attribute.Value, true);
		}

		// Token: 0x060042A6 RID: 17062 RVA: 0x0024E498 File Offset: 0x0024C698
		public static Color GetAttributeColor(this XElement element, string name, Color defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseColor(attribute.Value, true);
		}

		// Token: 0x060042A7 RID: 17063 RVA: 0x0024E4C8 File Offset: 0x0024C6C8
		public static Color? GetAttributeColor(this XElement element, string name)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return null;
			}
			return new Color?(XMLExtensions.ParseColor(attribute.Value, true));
		}

		// Token: 0x060042A8 RID: 17064 RVA: 0x0024E504 File Offset: 0x0024C704
		public static Color[] GetAttributeColorArray(this XElement element, string name, Color[] defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			string[] splitValue = stringValue.Split(';', StringSplitOptions.None);
			Color[] colorValue = new Color[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				try
				{
					Color val = XMLExtensions.ParseColor(splitValue[i], true);
					colorValue[i] = val;
				}
				catch (Exception e)
				{
					XMLExtensions.LogAttributeError(attribute, element, e);
				}
			}
			return colorValue;
		}

		// Token: 0x060042A9 RID: 17065 RVA: 0x0024E594 File Offset: 0x0024C794
		private static void LogAttributeError(XAttribute attribute, XElement element, Exception e)
		{
			string elementStr = element.ToString();
			if (elementStr.Length > 500)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error when reading attribute \"");
				defaultInterpolatedStringHandler.AppendFormatted<XAttribute>(attribute);
				defaultInterpolatedStringHandler.AppendLiteral("\"!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(38, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Error when reading attribute \"");
			defaultInterpolatedStringHandler2.AppendFormatted<XName>(attribute.Name);
			defaultInterpolatedStringHandler2.AppendLiteral("\" from ");
			defaultInterpolatedStringHandler2.AppendFormatted(elementStr);
			defaultInterpolatedStringHandler2.AppendLiteral("!");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, null, false, false);
		}

		// Token: 0x060042AA RID: 17066 RVA: 0x0024E644 File Offset: 0x0024C844
		public static KeyOrMouse GetAttributeKeyOrMouse(this XElement element, string name, KeyOrMouse defaultValue)
		{
			string strValue = element.GetAttributeString(name, ((defaultValue != null) ? defaultValue.ToString() : null) ?? "");
			Keys key;
			if (Enum.TryParse<Keys>(strValue, true, out key))
			{
				return key;
			}
			MouseButton mouseButton;
			if (Enum.TryParse<MouseButton>(strValue, out mouseButton))
			{
				return mouseButton;
			}
			int mouseButtonInt;
			if (int.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out mouseButtonInt) && Enum.GetValues<MouseButton>().Contains((MouseButton)mouseButtonInt))
			{
				return (MouseButton)mouseButtonInt;
			}
			if (string.Equals(strValue, "LeftMouse", StringComparison.OrdinalIgnoreCase))
			{
				return (!PlayerInput.MouseButtonsSwapped()) ? MouseButton.PrimaryMouse : MouseButton.SecondaryMouse;
			}
			if (string.Equals(strValue, "RightMouse", StringComparison.OrdinalIgnoreCase))
			{
				return (!PlayerInput.MouseButtonsSwapped()) ? MouseButton.SecondaryMouse : MouseButton.PrimaryMouse;
			}
			return defaultValue;
		}

		// Token: 0x060042AB RID: 17067 RVA: 0x0024E6F8 File Offset: 0x0024C8F8
		public static Rectangle GetAttributeRect(this XElement element, string name, Rectangle defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseRect(attribute.Value, false, true);
		}

		// Token: 0x060042AC RID: 17068 RVA: 0x0024E728 File Offset: 0x0024C928
		public static ValueTuple<T1, T2> GetAttributeTuple<T1, T2>(this XElement element, string name, ValueTuple<T1, T2> defaultValue)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<T1>(defaultValue.Item1);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<T2>(defaultValue.Item2);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			string strValue = element.GetAttributeString(name, defaultInterpolatedStringHandler.ToStringAndClear()).Trim();
			return XMLExtensions.ParseTuple<T1, T2>(strValue, defaultValue);
		}

		// Token: 0x060042AD RID: 17069 RVA: 0x0024E798 File Offset: 0x0024C998
		public static ValueTuple<T1, T2>[] GetAttributeTupleArray<T1, T2>(this XElement element, string name, ValueTuple<T1, T2>[] defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (string.IsNullOrEmpty(stringValue))
			{
				return defaultValue;
			}
			return (from s in stringValue.Split(';', StringSplitOptions.None)
			select XMLExtensions.ParseTuple<T1, T2>(s, default(ValueTuple<T1, T2>))).ToArray<ValueTuple<T1, T2>>();
		}

		// Token: 0x060042AE RID: 17070 RVA: 0x0024E7FC File Offset: 0x0024C9FC
		public static Range<int> GetAttributeRange(this XElement element, string name, Range<int> defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			string stringValue = attribute.Value;
			if (!string.IsNullOrEmpty(stringValue))
			{
				return XMLExtensions.ParseRange(stringValue);
			}
			return defaultValue;
		}

		// Token: 0x060042AF RID: 17071 RVA: 0x0024E834 File Offset: 0x0024CA34
		public static string ElementInnerText(this XElement el)
		{
			StringBuilder str = new StringBuilder();
			foreach (XText textNode in el.DescendantNodes().OfType<XText>())
			{
				str.Append(textNode.Value);
			}
			return str.ToString();
		}

		// Token: 0x060042B0 RID: 17072 RVA: 0x0024E898 File Offset: 0x0024CA98
		public static string PointToString(Point point)
		{
			return point.X.ToString() + "," + point.Y.ToString();
		}

		// Token: 0x060042B1 RID: 17073 RVA: 0x0024E8BC File Offset: 0x0024CABC
		public static string Vector2ToString(Vector2 vector)
		{
			return vector.X.ToString("G", CultureInfo.InvariantCulture) + "," + vector.Y.ToString("G", CultureInfo.InvariantCulture);
		}

		// Token: 0x060042B2 RID: 17074 RVA: 0x0024E8F4 File Offset: 0x0024CAF4
		public static string Vector3ToString(Vector3 vector, string format = "G")
		{
			return string.Concat(new string[]
			{
				vector.X.ToString(format, CultureInfo.InvariantCulture),
				",",
				vector.Y.ToString(format, CultureInfo.InvariantCulture),
				",",
				vector.Z.ToString(format, CultureInfo.InvariantCulture)
			});
		}

		// Token: 0x060042B3 RID: 17075 RVA: 0x0024E95C File Offset: 0x0024CB5C
		public static string Vector4ToString(Vector4 vector, string format = "G")
		{
			return string.Concat(new string[]
			{
				vector.X.ToString(format, CultureInfo.InvariantCulture),
				",",
				vector.Y.ToString(format, CultureInfo.InvariantCulture),
				",",
				vector.Z.ToString(format, CultureInfo.InvariantCulture),
				",",
				vector.W.ToString(format, CultureInfo.InvariantCulture)
			});
		}

		// Token: 0x060042B4 RID: 17076 RVA: 0x0024E9E0 File Offset: 0x0024CBE0
		[Obsolete("Prefer XMLExtensions.ToStringHex")]
		public static string ColorToString(Color color)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 4);
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.R);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.G);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.B);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.A);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060042B5 RID: 17077 RVA: 0x0024EA5C File Offset: 0x0024CC5C
		public static string ToStringHex(this Color color)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
			defaultInterpolatedStringHandler.AppendLiteral("#");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.R, "X2");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.G, "X2");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(color.B, "X2");
			string str = defaultInterpolatedStringHandler.ToStringAndClear();
			string str2;
			if (color.A >= 255)
			{
				str2 = "";
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(0, 1);
				defaultInterpolatedStringHandler2.AppendFormatted<byte>(color.A, "X2");
				str2 = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			return str + str2;
		}

		// Token: 0x060042B6 RID: 17078 RVA: 0x0024EAFC File Offset: 0x0024CCFC
		public static string RectToString(Rectangle rect)
		{
			return string.Concat(new string[]
			{
				rect.X.ToString(),
				",",
				rect.Y.ToString(),
				",",
				rect.Width.ToString(),
				",",
				rect.Height.ToString()
			});
		}

		// Token: 0x060042B7 RID: 17079 RVA: 0x0024EB68 File Offset: 0x0024CD68
		public static ValueTuple<T1, T2> ParseTuple<T1, T2>(string strValue, ValueTuple<T1, T2> defaultValue)
		{
			strValue = strValue.Trim();
			if (strValue[0] == '(')
			{
				string text = strValue;
				if (text[text.Length - 1] == ')')
				{
					string text2 = strValue;
					strValue = text2.Substring(1, text2.Length - 1 - 1);
					string[] elems = strValue.Split(',', StringSplitOptions.None);
					if (elems.Length != 2)
					{
						return defaultValue;
					}
					return new ValueTuple<T1, T2>((T1)((object)XMLExtensions.Converters[typeof(T1)](elems[0], defaultValue.Item1)), (T2)((object)XMLExtensions.Converters[typeof(T2)](elems[1], defaultValue.Item2)));
				}
			}
			return defaultValue;
		}

		// Token: 0x060042B8 RID: 17080 RVA: 0x0024EC20 File Offset: 0x0024CE20
		public static Point ParsePoint(string stringPoint, bool errorMessages = true)
		{
			string[] components = stringPoint.Split(',', StringSplitOptions.None);
			Point point = Point.Zero;
			if (components.Length == 2)
			{
				int.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out point.X);
				int.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out point.Y);
				return point;
			}
			if (!errorMessages)
			{
				return point;
			}
			DebugConsole.ThrowError("Failed to parse the string \"" + stringPoint + "\" to Vector2", null, null, false, false);
			return point;
		}

		// Token: 0x060042B9 RID: 17081 RVA: 0x0024EC98 File Offset: 0x0024CE98
		public static Vector2 ParseVector2(string stringVector2, bool errorMessages = true)
		{
			string[] components = stringVector2.Split(',', StringSplitOptions.None);
			Vector2 vector = Vector2.Zero;
			if (components.Length == 2)
			{
				float.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vector.X);
				float.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out vector.Y);
				return vector;
			}
			if (!errorMessages)
			{
				return vector;
			}
			DebugConsole.ThrowError("Failed to parse the string \"" + stringVector2 + "\" to Vector2", null, null, false, false);
			return vector;
		}

		// Token: 0x060042BA RID: 17082 RVA: 0x0024ED10 File Offset: 0x0024CF10
		public static Vector3 ParseVector3(string stringVector3, bool errorMessages = true)
		{
			string[] components = stringVector3.Split(',', StringSplitOptions.None);
			Vector3 vector = Vector3.Zero;
			if (components.Length == 3)
			{
				float.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vector.X);
				float.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out vector.Y);
				float.TryParse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture, out vector.Z);
				return vector;
			}
			if (!errorMessages)
			{
				return vector;
			}
			DebugConsole.ThrowError("Failed to parse the string \"" + stringVector3 + "\" to Vector3", null, null, false, false);
			return vector;
		}

		// Token: 0x060042BB RID: 17083 RVA: 0x0024EDA4 File Offset: 0x0024CFA4
		public static Vector4 ParseVector4(string stringVector4, bool errorMessages = true)
		{
			string[] components = stringVector4.Split(',', StringSplitOptions.None);
			Vector4 vector = Vector4.Zero;
			if (components.Length < 3)
			{
				if (errorMessages)
				{
					DebugConsole.ThrowError("Failed to parse the string \"" + stringVector4 + "\" to Vector4", null, null, false, false);
				}
				return vector;
			}
			float.TryParse(components[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.X);
			float.TryParse(components[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Y);
			float.TryParse(components[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.Z);
			if (components.Length > 3)
			{
				float.TryParse(components[3], NumberStyles.Float, CultureInfo.InvariantCulture, out vector.W);
			}
			return vector;
		}

		// Token: 0x060042BC RID: 17084 RVA: 0x0024EE54 File Offset: 0x0024D054
		public static Color ParseColor(string stringColor, bool errorMessages = true)
		{
			if (stringColor.StartsWith("gui.", StringComparison.OrdinalIgnoreCase))
			{
				Identifier colorName = stringColor.Substring(4).ToIdentifier();
				GUIColor guiColor;
				if (GUIStyle.Colors.TryGetValue(colorName, out guiColor))
				{
					return guiColor.Value;
				}
				return Color.White;
			}
			else if (stringColor.StartsWith("faction.", StringComparison.OrdinalIgnoreCase))
			{
				Identifier factionId = stringColor.Substring(8).ToIdentifier();
				FactionPrefab faction;
				if (FactionPrefab.Prefabs.TryGet(factionId, out faction))
				{
					return faction.IconColor;
				}
				return Color.White;
			}
			else
			{
				Color monoGameColor;
				if (XMLExtensions.monoGameColors.TryGetValue(stringColor.ToIdentifier(), out monoGameColor))
				{
					return monoGameColor;
				}
				string[] strComponents = stringColor.Split(',', StringSplitOptions.None);
				Color color = Color.White;
				float[] components = new float[]
				{
					1f,
					1f,
					1f,
					1f
				};
				if (strComponents.Length == 1)
				{
					bool altParseFailed = true;
					stringColor = stringColor.Trim();
					if (stringColor.Length > 0 && stringColor[0] == '#')
					{
						stringColor = stringColor.Substring(1);
						int colorInt;
						if (int.TryParse(stringColor, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out colorInt))
						{
							if (stringColor.Length == 6)
							{
								colorInt = (colorInt << 8 | 255);
							}
							components[0] = (float)(((long)colorInt & (long)((ulong)-16777216)) >> 24) / 255f;
							components[1] = (float)((colorInt & 16711680) >> 16) / 255f;
							components[2] = (float)((colorInt & 65280) >> 8) / 255f;
							components[3] = (float)(colorInt & 255) / 255f;
							altParseFailed = false;
						}
					}
					else if (stringColor.Length > 0 && stringColor[0] == '{')
					{
						stringColor = stringColor.Substring(1, stringColor.Length - 2);
						string[] mgComponents = stringColor.Split(' ', StringSplitOptions.None);
						if (mgComponents.Length == 4)
						{
							altParseFailed = false;
							string[] expectedPrefixes = new string[]
							{
								"R:",
								"G:",
								"B:",
								"A:"
							};
							for (int i = 0; i < 4; i++)
							{
								if (!mgComponents[i].StartsWith(expectedPrefixes[i], StringComparison.OrdinalIgnoreCase))
								{
									altParseFailed = true;
									break;
								}
								string strToParse = mgComponents[i].Remove(expectedPrefixes[i], StringComparison.OrdinalIgnoreCase).Trim();
								int val = 0;
								altParseFailed |= !int.TryParse(strToParse, out val);
								components[i] = (float)val / 255f;
							}
						}
					}
					if (altParseFailed)
					{
						if (errorMessages)
						{
							DebugConsole.ThrowError("Failed to parse the string \"" + stringColor + "\" to Color", null, null, false, false);
						}
						return Color.White;
					}
				}
				else
				{
					int j = 0;
					while (j < 4 && j < strComponents.Length)
					{
						float.TryParse(strComponents[j], NumberStyles.Float, CultureInfo.InvariantCulture, out components[j]);
						j++;
					}
					if (components.Any((float c) => c > 1f))
					{
						for (int k = 0; k < 4; k++)
						{
							components[k] /= 255f;
						}
						if (strComponents.Length < 4)
						{
							components[3] = 1f;
						}
					}
				}
				return new Color(components[0], components[1], components[2], components[3]);
			}
		}

		// Token: 0x060042BD RID: 17085 RVA: 0x0024F158 File Offset: 0x0024D358
		public static Rectangle ParseRect(string stringRect, bool requireSize, bool errorMessages = true)
		{
			string[] strComponents = stringRect.Split(',', StringSplitOptions.None);
			if ((strComponents.Length < 3 && requireSize) || strComponents.Length < 2)
			{
				if (errorMessages)
				{
					DebugConsole.ThrowError("Failed to parse the string \"" + stringRect + "\" to Rectangle", null, null, false, false);
				}
				return new Rectangle(0, 0, 0, 0);
			}
			int[] components = new int[4];
			int i = 0;
			while (i < 4 && i < strComponents.Length)
			{
				int.TryParse(strComponents[i], out components[i]);
				i++;
			}
			return new Rectangle(components[0], components[1], components[2], components[3]);
		}

		// Token: 0x060042BE RID: 17086 RVA: 0x0024F1E0 File Offset: 0x0024D3E0
		public static float[] ParseFloatArray(string[] stringArray)
		{
			if (stringArray == null || stringArray.Length == 0)
			{
				return null;
			}
			float[] floatArray = new float[stringArray.Length];
			for (int i = 0; i < floatArray.Length; i++)
			{
				floatArray[i] = 0f;
				float.TryParse(stringArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out floatArray[i]);
			}
			return floatArray;
		}

		// Token: 0x060042BF RID: 17087 RVA: 0x0024F230 File Offset: 0x0024D430
		public static Range<int> ParseRange(string rangeString)
		{
			if (string.IsNullOrWhiteSpace(rangeString))
			{
				return XMLExtensions.<ParseRange>g__GetDefault|75_1(rangeString);
			}
			string[] split = rangeString.Split('-', StringSplitOptions.None);
			int num = split.Length;
			int value;
			if (num != 1)
			{
				if (num == 2)
				{
					int min;
					int max;
					if (XMLExtensions.<ParseRange>g__TryParseInt|75_0(split[0], out min) && XMLExtensions.<ParseRange>g__TryParseInt|75_0(split[1], out max) && min < max)
					{
						return new Range<int>(min, max);
					}
				}
			}
			else if (XMLExtensions.<ParseRange>g__TryParseInt|75_0(split[0], out value))
			{
				return new Range<int>(value, value);
			}
			return XMLExtensions.<ParseRange>g__GetDefault|75_1(rangeString);
		}

		// Token: 0x060042C0 RID: 17088 RVA: 0x0024F2AF File Offset: 0x0024D4AF
		public static Identifier VariantOf(this XElement element)
		{
			return element.GetAttributeIdentifier("inherit", element.GetAttributeIdentifier("variantof", ""));
		}

		// Token: 0x060042C1 RID: 17089 RVA: 0x0024F2CC File Offset: 0x0024D4CC
		public static bool IsOverride(this XElement element)
		{
			Identifier identifier = element.NameAsIdentifier();
			return identifier == "override";
		}

		// Token: 0x060042C2 RID: 17090 RVA: 0x0024F2EC File Offset: 0x0024D4EC
		public static XElement GetRootExcludingOverride(this XDocument doc)
		{
			if (!doc.Root.IsOverride())
			{
				return doc.Root;
			}
			return doc.Root.FirstElement();
		}

		// Token: 0x060042C3 RID: 17091 RVA: 0x0024F30D File Offset: 0x0024D50D
		public static XElement FirstElement(this XElement element)
		{
			return element.Elements().FirstOrDefault<XElement>();
		}

		// Token: 0x060042C4 RID: 17092 RVA: 0x0024F31C File Offset: 0x0024D51C
		public static XAttribute GetAttribute(this XElement element, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return element.GetAttribute((XAttribute a) => a.Name.ToString().Equals(name, comparisonMethod));
		}

		// Token: 0x060042C5 RID: 17093 RVA: 0x0024F350 File Offset: 0x0024D550
		public static bool TrySetAttributeValue(this XElement element, string name, object value, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			XAttribute attribute = element.GetAttribute(name, comparisonMethod);
			if (attribute == null)
			{
				return false;
			}
			attribute.SetValue(value);
			return true;
		}

		// Token: 0x060042C6 RID: 17094 RVA: 0x0024F373 File Offset: 0x0024D573
		public static void SetAttribute(this XElement element, string name, object value)
		{
			if (!element.TrySetAttributeValue(name, value, StringComparison.OrdinalIgnoreCase))
			{
				element.SetAttributeValue(name, value);
			}
		}

		// Token: 0x060042C7 RID: 17095 RVA: 0x0024F38D File Offset: 0x0024D58D
		public static XAttribute GetAttribute(this XElement element, Identifier name)
		{
			return element.GetAttribute(name.Value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060042C8 RID: 17096 RVA: 0x0024F39D File Offset: 0x0024D59D
		public static XAttribute GetAttribute(this XElement element, Func<XAttribute, bool> predicate)
		{
			return element.Attributes().FirstOrDefault(predicate);
		}

		// Token: 0x060042C9 RID: 17097 RVA: 0x0024F3AC File Offset: 0x0024D5AC
		public static XElement GetChildElement(this XContainer container, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return container.Elements().FirstOrDefault((XElement e) => e.Name.ToString().Equals(name, comparisonMethod));
		}

		// Token: 0x060042CA RID: 17098 RVA: 0x0024F3E4 File Offset: 0x0024D5E4
		public static IEnumerable<XElement> GetChildElements(this XContainer container, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return from e in container.Elements()
			where e.Name.ToString().Equals(name, comparisonMethod)
			select e;
		}

		// Token: 0x060042CB RID: 17099 RVA: 0x0024F41C File Offset: 0x0024D61C
		public static IEnumerable<XElement> GetChildElements(this XContainer container, params string[] names)
		{
			return names.SelectMany((string name) => container.GetChildElements(name, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x060042CC RID: 17100 RVA: 0x0024F448 File Offset: 0x0024D648
		public static bool ComesAfter(this XElement element, XElement other)
		{
			if (element.Parent != other.Parent)
			{
				return false;
			}
			foreach (XElement child in element.Parent.Elements())
			{
				if (child == element)
				{
					return false;
				}
				if (child == other)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060042CD RID: 17101 RVA: 0x0024F4B8 File Offset: 0x0024D6B8
		public static Identifier NameAsIdentifier(this XElement elem)
		{
			return elem.Name.LocalName.ToIdentifier();
		}

		// Token: 0x060042CE RID: 17102 RVA: 0x0024F4CA File Offset: 0x0024D6CA
		public static Identifier NameAsIdentifier(this XAttribute attr)
		{
			return attr.Name.LocalName.ToIdentifier();
		}

		// Token: 0x060042D0 RID: 17104 RVA: 0x0024F6BE File Offset: 0x0024D8BE
		[CompilerGenerated]
		internal static bool <ParseRange>g__TryParseInt|75_0(string value, out int result)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			}
			result = 0;
			return false;
		}

		// Token: 0x060042D1 RID: 17105 RVA: 0x0024F6DE File Offset: 0x0024D8DE
		[CompilerGenerated]
		internal static Range<int> <ParseRange>g__GetDefault|75_1(string rangeString)
		{
			DebugConsole.ThrowError("Error parsing range: \"" + rangeString + "\" (using default value 0-99)", null, null, false, false);
			return new Range<int>(0, 99);
		}

		// Token: 0x04002291 RID: 8849
		private static readonly ImmutableDictionary<Type, Func<string, object, object>> Converters = new Dictionary<Type, Func<string, object, object>>
		{
			{
				typeof(string),
				(string str, object defVal) => str
			},
			{
				typeof(int),
				delegate(string str, object defVal)
				{
					int result;
					if (!int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return defVal;
					}
					return result;
				}
			},
			{
				typeof(uint),
				delegate(string str, object defVal)
				{
					uint result;
					if (!uint.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return defVal;
					}
					return result;
				}
			},
			{
				typeof(ulong),
				delegate(string str, object defVal)
				{
					ulong result;
					if (!ulong.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return defVal;
					}
					return result;
				}
			},
			{
				typeof(float),
				delegate(string str, object defVal)
				{
					float result;
					if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return defVal;
					}
					return result;
				}
			},
			{
				typeof(bool),
				delegate(string str, object defVal)
				{
					bool result;
					if (!bool.TryParse(str, out result))
					{
						return defVal;
					}
					return result;
				}
			},
			{
				typeof(Color),
				(string str, object defVal) => XMLExtensions.ParseColor(str, true)
			},
			{
				typeof(Vector2),
				(string str, object defVal) => XMLExtensions.ParseVector2(str, true)
			},
			{
				typeof(Vector3),
				(string str, object defVal) => XMLExtensions.ParseVector3(str, true)
			},
			{
				typeof(Vector4),
				(string str, object defVal) => XMLExtensions.ParseVector4(str, true)
			},
			{
				typeof(Rectangle),
				(string str, object defVal) => XMLExtensions.ParseRect(str, true, true)
			}
		}.ToImmutableDictionary<Type, Func<string, object, object>>();

		// Token: 0x04002292 RID: 8850
		public static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null,
			IgnoreWhitespace = true
		};

		// Token: 0x04002293 RID: 8851
		private static readonly ImmutableDictionary<Identifier, Color> monoGameColors = (from p in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public)
		where p.PropertyType == typeof(Color)
		select new ValueTuple<Identifier, Color>(p.Name.ToIdentifier(), p.GetValueFromStaticProperty<Color>())).ToImmutableDictionary<Identifier, Color>();
	}
}
