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

namespace Barotrauma
{
	// Token: 0x02000284 RID: 644
	public static class XMLExtensions
	{
		// Token: 0x06002D65 RID: 11621 RVA: 0x0012D628 File Offset: 0x0012B828
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

		// Token: 0x06002D66 RID: 11622 RVA: 0x0012D6B0 File Offset: 0x0012B8B0
		public static string ParseContentPathFromUri(this XObject element)
		{
			if (string.IsNullOrWhiteSpace(element.BaseUri))
			{
				return "";
			}
			return Path.GetRelativePath(Environment.CurrentDirectory, element.BaseUri.CleanUpPath());
		}

		// Token: 0x06002D67 RID: 11623 RVA: 0x0012D6DA File Offset: 0x0012B8DA
		public static XmlReader CreateReader(Stream stream, string baseUri = "")
		{
			return XmlReader.Create(stream, XMLExtensions.ReaderSettings, baseUri);
		}

		// Token: 0x06002D68 RID: 11624 RVA: 0x0012D6E8 File Offset: 0x0012B8E8
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

		// Token: 0x06002D69 RID: 11625 RVA: 0x0012D768 File Offset: 0x0012B968
		public static XDocument TryLoadXml(ContentPath path)
		{
			return XMLExtensions.TryLoadXml(path.Value);
		}

		// Token: 0x06002D6A RID: 11626 RVA: 0x0012D778 File Offset: 0x0012B978
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

		// Token: 0x06002D6B RID: 11627 RVA: 0x0012D7CC File Offset: 0x0012B9CC
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

		// Token: 0x06002D6C RID: 11628 RVA: 0x0012D86C File Offset: 0x0012BA6C
		public static object GetAttributeObject(XAttribute attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			return XMLExtensions.ParseToObject(attribute.Value.ToString());
		}

		// Token: 0x06002D6D RID: 11629 RVA: 0x0012D884 File Offset: 0x0012BA84
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

		// Token: 0x06002D6E RID: 11630 RVA: 0x0012D900 File Offset: 0x0012BB00
		public static string GetAttributeString(this XElement element, string name, string defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.GetAttributeString(attribute, defaultValue);
		}

		// Token: 0x06002D6F RID: 11631 RVA: 0x0012D92C File Offset: 0x0012BB2C
		public static string GetAttributeStringUnrestricted(this XElement element, string name, string defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.GetAttributeString(attribute, defaultValue);
		}

		// Token: 0x06002D70 RID: 11632 RVA: 0x0012D954 File Offset: 0x0012BB54
		public static bool DoesAttributeReferenceFileNameAlone(this XElement element, string name)
		{
			string texName = element.GetAttributeStringUnrestricted(name, "");
			return (!texName.IsNullOrEmpty() & !texName.Contains("/")) && !texName.Contains("%ModDir", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002D71 RID: 11633 RVA: 0x0012D99C File Offset: 0x0012BB9C
		public static ContentPath GetAttributeContentPath(this XElement element, string name, ContentPackage contentPackage)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return null;
			}
			return ContentPath.FromRaw(contentPackage, XMLExtensions.GetAttributeString(attribute, null));
		}

		// Token: 0x06002D72 RID: 11634 RVA: 0x0012D9CA File Offset: 0x0012BBCA
		public static Identifier GetAttributeIdentifier(this XElement element, string name, string defaultValue)
		{
			return element.GetAttributeString(name, defaultValue).ToIdentifier();
		}

		// Token: 0x06002D73 RID: 11635 RVA: 0x0012D9D9 File Offset: 0x0012BBD9
		public static Identifier GetAttributeIdentifier(this XElement element, string name, Identifier defaultValue)
		{
			return element.GetAttributeIdentifier(name, defaultValue.Value);
		}

		// Token: 0x06002D74 RID: 11636 RVA: 0x0012D9EC File Offset: 0x0012BBEC
		private static string GetAttributeString(XAttribute attribute, string defaultValue)
		{
			string value = attribute.Value;
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
			return defaultValue;
		}

		// Token: 0x06002D75 RID: 11637 RVA: 0x0012DA0C File Offset: 0x0012BC0C
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

		// Token: 0x06002D76 RID: 11638 RVA: 0x0012DA84 File Offset: 0x0012BC84
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

		// Token: 0x06002D77 RID: 11639 RVA: 0x0012DABB File Offset: 0x0012BCBB
		public static Identifier[] GetAttributeIdentifierArray(this XElement element, string name, Identifier[] defaultValue, bool trim = true)
		{
			string[] attributeStringArray = element.GetAttributeStringArray(name, null, trim, false);
			return ((attributeStringArray != null) ? attributeStringArray.ToIdentifiers() : null) ?? defaultValue;
		}

		// Token: 0x06002D78 RID: 11640 RVA: 0x0012DAD8 File Offset: 0x0012BCD8
		public static ImmutableHashSet<Identifier> GetAttributeIdentifierImmutableHashSet(this XElement element, string key, ImmutableHashSet<Identifier> defaultValue, bool trim = true)
		{
			Identifier[] attributeIdentifierArray = element.GetAttributeIdentifierArray(key, null, trim);
			return ((attributeIdentifierArray != null) ? attributeIdentifierArray.ToImmutableHashSet<Identifier>() : null) ?? defaultValue;
		}

		// Token: 0x06002D79 RID: 11641 RVA: 0x0012DAF4 File Offset: 0x0012BCF4
		public static float? GetAttributeNullableFloat(this XElement element, string attributeName)
		{
			XAttribute attribute = element.GetAttribute(attributeName, StringComparison.OrdinalIgnoreCase);
			if (attribute != null)
			{
				return new float?(attribute.GetAttributeFloat(0f));
			}
			return null;
		}

		// Token: 0x06002D7A RID: 11642 RVA: 0x0012DB28 File Offset: 0x0012BD28
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

		// Token: 0x06002D7B RID: 11643 RVA: 0x0012DB63 File Offset: 0x0012BD63
		public static float GetAttributeFloat(this XElement element, string name, float defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeFloat(defaultValue);
		}

		// Token: 0x06002D7C RID: 11644 RVA: 0x0012DB7C File Offset: 0x0012BD7C
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

		// Token: 0x06002D7D RID: 11645 RVA: 0x0012DBF8 File Offset: 0x0012BDF8
		public static double GetAttributeDouble(this XElement element, string name, double defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeDouble(defaultValue);
		}

		// Token: 0x06002D7E RID: 11646 RVA: 0x0012DC10 File Offset: 0x0012BE10
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

		// Token: 0x06002D7F RID: 11647 RVA: 0x0012DC8C File Offset: 0x0012BE8C
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

		// Token: 0x06002D80 RID: 11648 RVA: 0x0012DD38 File Offset: 0x0012BF38
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

		// Token: 0x06002D81 RID: 11649 RVA: 0x0012DD9C File Offset: 0x0012BF9C
		public static int? GetAttributeNullableInt(this XElement element, string attributeName)
		{
			XAttribute attribute = element.GetAttribute(attributeName, StringComparison.OrdinalIgnoreCase);
			if (attribute != null)
			{
				return new int?(attribute.GetAttributeInt(0));
			}
			return null;
		}

		// Token: 0x06002D82 RID: 11650 RVA: 0x0012DDCB File Offset: 0x0012BFCB
		public static int GetAttributeInt(this XElement element, string name, int defaultValue)
		{
			return ((element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null).GetAttributeInt(defaultValue);
		}

		// Token: 0x06002D83 RID: 11651 RVA: 0x0012DDE4 File Offset: 0x0012BFE4
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

		// Token: 0x06002D84 RID: 11652 RVA: 0x0012DE48 File Offset: 0x0012C048
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

		// Token: 0x06002D85 RID: 11653 RVA: 0x0012DE98 File Offset: 0x0012C098
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

		// Token: 0x06002D86 RID: 11654 RVA: 0x0012DEE8 File Offset: 0x0012C0E8
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

		// Token: 0x06002D87 RID: 11655 RVA: 0x0012DF40 File Offset: 0x0012C140
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

		// Token: 0x06002D88 RID: 11656 RVA: 0x0012DF74 File Offset: 0x0012C174
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

		// Token: 0x06002D89 RID: 11657 RVA: 0x0012DFC4 File Offset: 0x0012C1C4
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

		// Token: 0x06002D8A RID: 11658 RVA: 0x0012E050 File Offset: 0x0012C250
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

		// Token: 0x06002D8B RID: 11659 RVA: 0x0012E0DC File Offset: 0x0012C2DC
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

		// Token: 0x06002D8C RID: 11660 RVA: 0x0012E188 File Offset: 0x0012C388
		public static T GetAttributeEnum<T>(this XElement element, string name, T defaultValue) where T : struct, Enum
		{
			XAttribute attr = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attr == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseEnumValue<T>(attr.Value, defaultValue, attr);
		}

		// Token: 0x06002D8D RID: 11661 RVA: 0x0012E1B8 File Offset: 0x0012C3B8
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

		// Token: 0x06002D8E RID: 11662 RVA: 0x0012E240 File Offset: 0x0012C440
		public static bool GetAttributeBool(this XElement element, string name, bool defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return attribute.GetAttributeBool(defaultValue);
		}

		// Token: 0x06002D8F RID: 11663 RVA: 0x0012E268 File Offset: 0x0012C468
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

		// Token: 0x06002D90 RID: 11664 RVA: 0x0012E2E8 File Offset: 0x0012C4E8
		public static Point GetAttributePoint(this XElement element, string name, Point defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParsePoint(attribute.Value, true);
		}

		// Token: 0x06002D91 RID: 11665 RVA: 0x0012E318 File Offset: 0x0012C518
		public static Vector2 GetAttributeVector2(this XElement element, string name, Vector2 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector2(attribute.Value, true);
		}

		// Token: 0x06002D92 RID: 11666 RVA: 0x0012E348 File Offset: 0x0012C548
		public static Vector3 GetAttributeVector3(this XElement element, string name, Vector3 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector3(attribute.Value, true);
		}

		// Token: 0x06002D93 RID: 11667 RVA: 0x0012E378 File Offset: 0x0012C578
		public static Vector4 GetAttributeVector4(this XElement element, string name, Vector4 defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseVector4(attribute.Value, true);
		}

		// Token: 0x06002D94 RID: 11668 RVA: 0x0012E3A8 File Offset: 0x0012C5A8
		public static Color GetAttributeColor(this XElement element, string name, Color defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseColor(attribute.Value, true);
		}

		// Token: 0x06002D95 RID: 11669 RVA: 0x0012E3D8 File Offset: 0x0012C5D8
		public static Color? GetAttributeColor(this XElement element, string name)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return null;
			}
			return new Color?(XMLExtensions.ParseColor(attribute.Value, true));
		}

		// Token: 0x06002D96 RID: 11670 RVA: 0x0012E414 File Offset: 0x0012C614
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

		// Token: 0x06002D97 RID: 11671 RVA: 0x0012E4A4 File Offset: 0x0012C6A4
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

		// Token: 0x06002D98 RID: 11672 RVA: 0x0012E554 File Offset: 0x0012C754
		public static Rectangle GetAttributeRect(this XElement element, string name, Rectangle defaultValue)
		{
			XAttribute attribute = (element != null) ? element.GetAttribute(name, StringComparison.OrdinalIgnoreCase) : null;
			if (attribute == null)
			{
				return defaultValue;
			}
			return XMLExtensions.ParseRect(attribute.Value, false, true);
		}

		// Token: 0x06002D99 RID: 11673 RVA: 0x0012E584 File Offset: 0x0012C784
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

		// Token: 0x06002D9A RID: 11674 RVA: 0x0012E5F4 File Offset: 0x0012C7F4
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

		// Token: 0x06002D9B RID: 11675 RVA: 0x0012E658 File Offset: 0x0012C858
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

		// Token: 0x06002D9C RID: 11676 RVA: 0x0012E690 File Offset: 0x0012C890
		public static string ElementInnerText(this XElement el)
		{
			StringBuilder str = new StringBuilder();
			foreach (XText textNode in el.DescendantNodes().OfType<XText>())
			{
				str.Append(textNode.Value);
			}
			return str.ToString();
		}

		// Token: 0x06002D9D RID: 11677 RVA: 0x0012E6F4 File Offset: 0x0012C8F4
		public static string PointToString(Point point)
		{
			return point.X.ToString() + "," + point.Y.ToString();
		}

		// Token: 0x06002D9E RID: 11678 RVA: 0x0012E718 File Offset: 0x0012C918
		public static string Vector2ToString(Vector2 vector)
		{
			return vector.X.ToString("G", CultureInfo.InvariantCulture) + "," + vector.Y.ToString("G", CultureInfo.InvariantCulture);
		}

		// Token: 0x06002D9F RID: 11679 RVA: 0x0012E750 File Offset: 0x0012C950
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

		// Token: 0x06002DA0 RID: 11680 RVA: 0x0012E7B8 File Offset: 0x0012C9B8
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

		// Token: 0x06002DA1 RID: 11681 RVA: 0x0012E83C File Offset: 0x0012CA3C
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

		// Token: 0x06002DA2 RID: 11682 RVA: 0x0012E8B8 File Offset: 0x0012CAB8
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

		// Token: 0x06002DA3 RID: 11683 RVA: 0x0012E958 File Offset: 0x0012CB58
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

		// Token: 0x06002DA4 RID: 11684 RVA: 0x0012E9C4 File Offset: 0x0012CBC4
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

		// Token: 0x06002DA5 RID: 11685 RVA: 0x0012EA7C File Offset: 0x0012CC7C
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

		// Token: 0x06002DA6 RID: 11686 RVA: 0x0012EAF4 File Offset: 0x0012CCF4
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

		// Token: 0x06002DA7 RID: 11687 RVA: 0x0012EB6C File Offset: 0x0012CD6C
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

		// Token: 0x06002DA8 RID: 11688 RVA: 0x0012EC00 File Offset: 0x0012CE00
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

		// Token: 0x06002DA9 RID: 11689 RVA: 0x0012ECB0 File Offset: 0x0012CEB0
		public static Color ParseColor(string stringColor, bool errorMessages = true)
		{
			if (stringColor.StartsWith("gui.", StringComparison.OrdinalIgnoreCase))
			{
				return Color.White;
			}
			if (stringColor.StartsWith("faction.", StringComparison.OrdinalIgnoreCase))
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

		// Token: 0x06002DAA RID: 11690 RVA: 0x0012EF8C File Offset: 0x0012D18C
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

		// Token: 0x06002DAB RID: 11691 RVA: 0x0012F014 File Offset: 0x0012D214
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

		// Token: 0x06002DAC RID: 11692 RVA: 0x0012F064 File Offset: 0x0012D264
		public static Range<int> ParseRange(string rangeString)
		{
			if (string.IsNullOrWhiteSpace(rangeString))
			{
				return XMLExtensions.<ParseRange>g__GetDefault|74_1(rangeString);
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
					if (XMLExtensions.<ParseRange>g__TryParseInt|74_0(split[0], out min) && XMLExtensions.<ParseRange>g__TryParseInt|74_0(split[1], out max) && min < max)
					{
						return new Range<int>(min, max);
					}
				}
			}
			else if (XMLExtensions.<ParseRange>g__TryParseInt|74_0(split[0], out value))
			{
				return new Range<int>(value, value);
			}
			return XMLExtensions.<ParseRange>g__GetDefault|74_1(rangeString);
		}

		// Token: 0x06002DAD RID: 11693 RVA: 0x0012F0E3 File Offset: 0x0012D2E3
		public static Identifier VariantOf(this XElement element)
		{
			return element.GetAttributeIdentifier("inherit", element.GetAttributeIdentifier("variantof", ""));
		}

		// Token: 0x06002DAE RID: 11694 RVA: 0x0012F100 File Offset: 0x0012D300
		public static bool IsOverride(this XElement element)
		{
			Identifier identifier = element.NameAsIdentifier();
			return identifier == "override";
		}

		// Token: 0x06002DAF RID: 11695 RVA: 0x0012F120 File Offset: 0x0012D320
		public static XElement GetRootExcludingOverride(this XDocument doc)
		{
			if (!doc.Root.IsOverride())
			{
				return doc.Root;
			}
			return doc.Root.FirstElement();
		}

		// Token: 0x06002DB0 RID: 11696 RVA: 0x0012F141 File Offset: 0x0012D341
		public static XElement FirstElement(this XElement element)
		{
			return element.Elements().FirstOrDefault<XElement>();
		}

		// Token: 0x06002DB1 RID: 11697 RVA: 0x0012F150 File Offset: 0x0012D350
		public static XAttribute GetAttribute(this XElement element, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return element.GetAttribute((XAttribute a) => a.Name.ToString().Equals(name, comparisonMethod));
		}

		// Token: 0x06002DB2 RID: 11698 RVA: 0x0012F184 File Offset: 0x0012D384
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

		// Token: 0x06002DB3 RID: 11699 RVA: 0x0012F1A7 File Offset: 0x0012D3A7
		public static void SetAttribute(this XElement element, string name, object value)
		{
			if (!element.TrySetAttributeValue(name, value, StringComparison.OrdinalIgnoreCase))
			{
				element.SetAttributeValue(name, value);
			}
		}

		// Token: 0x06002DB4 RID: 11700 RVA: 0x0012F1C1 File Offset: 0x0012D3C1
		public static XAttribute GetAttribute(this XElement element, Identifier name)
		{
			return element.GetAttribute(name.Value, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002DB5 RID: 11701 RVA: 0x0012F1D1 File Offset: 0x0012D3D1
		public static XAttribute GetAttribute(this XElement element, Func<XAttribute, bool> predicate)
		{
			return element.Attributes().FirstOrDefault(predicate);
		}

		// Token: 0x06002DB6 RID: 11702 RVA: 0x0012F1E0 File Offset: 0x0012D3E0
		public static XElement GetChildElement(this XContainer container, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return container.Elements().FirstOrDefault((XElement e) => e.Name.ToString().Equals(name, comparisonMethod));
		}

		// Token: 0x06002DB7 RID: 11703 RVA: 0x0012F218 File Offset: 0x0012D418
		public static IEnumerable<XElement> GetChildElements(this XContainer container, string name, StringComparison comparisonMethod = StringComparison.OrdinalIgnoreCase)
		{
			return from e in container.Elements()
			where e.Name.ToString().Equals(name, comparisonMethod)
			select e;
		}

		// Token: 0x06002DB8 RID: 11704 RVA: 0x0012F250 File Offset: 0x0012D450
		public static IEnumerable<XElement> GetChildElements(this XContainer container, params string[] names)
		{
			return names.SelectMany((string name) => container.GetChildElements(name, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x06002DB9 RID: 11705 RVA: 0x0012F27C File Offset: 0x0012D47C
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

		// Token: 0x06002DBA RID: 11706 RVA: 0x0012F2EC File Offset: 0x0012D4EC
		public static Identifier NameAsIdentifier(this XElement elem)
		{
			return elem.Name.LocalName.ToIdentifier();
		}

		// Token: 0x06002DBB RID: 11707 RVA: 0x0012F2FE File Offset: 0x0012D4FE
		public static Identifier NameAsIdentifier(this XAttribute attr)
		{
			return attr.Name.LocalName.ToIdentifier();
		}

		// Token: 0x06002DBD RID: 11709 RVA: 0x0012F4F2 File Offset: 0x0012D6F2
		[CompilerGenerated]
		internal static bool <ParseRange>g__TryParseInt|74_0(string value, out int result)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
			}
			result = 0;
			return false;
		}

		// Token: 0x06002DBE RID: 11710 RVA: 0x0012F512 File Offset: 0x0012D712
		[CompilerGenerated]
		internal static Range<int> <ParseRange>g__GetDefault|74_1(string rangeString)
		{
			DebugConsole.ThrowError("Error parsing range: \"" + rangeString + "\" (using default value 0-99)", null, null, false, false);
			return new Range<int>(0, 99);
		}

		// Token: 0x0400164D RID: 5709
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

		// Token: 0x0400164E RID: 5710
		public static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
		{
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null,
			IgnoreWhitespace = true
		};

		// Token: 0x0400164F RID: 5711
		private static readonly ImmutableDictionary<Identifier, Color> monoGameColors = (from p in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public)
		where p.PropertyType == typeof(Color)
		select new ValueTuple<Identifier, Color>(p.Name.ToIdentifier(), p.GetValueFromStaticProperty<Color>())).ToImmutableDictionary<Identifier, Color>();
	}
}
