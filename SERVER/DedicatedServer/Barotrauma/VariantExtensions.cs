using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000272 RID: 626
	public static class VariantExtensions
	{
		// Token: 0x06002CBE RID: 11454 RVA: 0x00127070 File Offset: 0x00125270
		[NullableContext(1)]
		public static ContentXElement CreateVariantXML(this ContentXElement variantElement, ContentXElement baseElement, [Nullable(2)] VariantExtensions.VariantXMLChecker checker = null)
		{
			VariantExtensions.<>c__DisplayClass1_0 CS$<>8__locals1;
			CS$<>8__locals1.checker = checker;
			XElement newElement = new XElement(baseElement);
			if (baseElement.ContentPackage != null && baseElement.ContentPackage != variantElement.ContentPackage)
			{
				foreach (XElement subElement in newElement.Descendants())
				{
					foreach (XAttribute attribute in subElement.Attributes())
					{
						if (attribute.Value.Contains("%ModDir%"))
						{
							attribute.SetValue(attribute.Value.Replace("%ModDir%", string.Format("%ModDir:{0}%", baseElement.ContentPackage.Name), StringComparison.OrdinalIgnoreCase));
						}
					}
				}
			}
			VariantExtensions.<CreateVariantXML>g__ReplaceElement|1_0(newElement, variantElement, ref CS$<>8__locals1);
			return newElement.FromPackage(variantElement.ContentPackage);
		}

		// Token: 0x06002CBF RID: 11455 RVA: 0x0012717C File Offset: 0x0012537C
		[NullableContext(1)]
		[CompilerGenerated]
		internal static void <CreateVariantXML>g__ReplaceElement|1_0(XElement element, XElement replacement, ref VariantExtensions.<>c__DisplayClass1_0 A_2)
		{
			XElement originalElement = new XElement(element);
			List<XElement> newElementsFromBase = new List<XElement>(element.Elements());
			List<XElement> elementsToRemove = new List<XElement>();
			foreach (XAttribute attribute in replacement.Attributes())
			{
				VariantExtensions.<CreateVariantXML>g__ReplaceAttribute|1_1(element, attribute);
			}
			List<Identifier> elementNamesToRemove = new List<Identifier>();
			foreach (XElement subElement in replacement.Elements())
			{
				if (subElement.Name.ToString().Equals("clearall", StringComparison.OrdinalIgnoreCase))
				{
					elementNamesToRemove.AddRange(from e in subElement.Elements()
					select e.Name.ToIdentifier<XName>());
				}
			}
			using (IEnumerator<XElement> enumerator3 = replacement.Elements().GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					XElement replacementSubElement = enumerator3.Current;
					int index = replacement.Elements().ToList<XElement>().FindAll((XElement e) => e.Name.ToString().Equals(replacementSubElement.Name.ToString(), StringComparison.OrdinalIgnoreCase)).IndexOf(replacementSubElement);
					int i = 0;
					bool matchingElementFound = false;
					bool cleared = false;
					foreach (XElement subElement2 in element.Elements())
					{
						if (replacementSubElement.Name.ToString().Equals("clearall", StringComparison.OrdinalIgnoreCase))
						{
							if (elementNamesToRemove.Contains(subElement2.NameAsIdentifier()))
							{
								if (!elementsToRemove.Contains(subElement2))
								{
									elementsToRemove.Add(subElement2);
								}
								matchingElementFound = true;
							}
						}
						else
						{
							if (replacementSubElement.Name.ToString().Equals("clear", StringComparison.OrdinalIgnoreCase))
							{
								matchingElementFound = true;
								newElementsFromBase.Clear();
								elementsToRemove.AddRange(element.Elements());
								foreach (XElement elementAfterClear in replacementSubElement.ElementsAfterSelf())
								{
									element.Add(elementAfterClear);
								}
								cleared = true;
								break;
							}
							if (subElement2.Name.ToString().Equals(replacementSubElement.Name.ToString(), StringComparison.OrdinalIgnoreCase))
							{
								if (i == index)
								{
									if (!replacementSubElement.HasAttributes && !replacementSubElement.HasElements)
									{
										elementsToRemove.Add(subElement2);
									}
									else
									{
										VariantExtensions.<CreateVariantXML>g__ReplaceElement|1_0(subElement2, replacementSubElement, ref A_2);
									}
									matchingElementFound = true;
									newElementsFromBase.Remove(subElement2);
									break;
								}
								i++;
							}
						}
					}
					if (!matchingElementFound)
					{
						element.Add(replacementSubElement);
					}
					if (cleared)
					{
						break;
					}
				}
			}
			elementsToRemove.ForEach(delegate(XElement e)
			{
				e.Remove();
			});
			VariantExtensions.VariantXMLChecker checker = A_2.checker;
			if (checker != null)
			{
				checker(originalElement, replacement, element);
			}
			foreach (XElement newElement in newElementsFromBase)
			{
				VariantExtensions.VariantXMLChecker checker2 = A_2.checker;
				if (checker2 != null)
				{
					checker2(newElement, null, newElement);
				}
			}
		}

		// Token: 0x06002CC0 RID: 11456 RVA: 0x00127560 File Offset: 0x00125760
		[NullableContext(1)]
		[CompilerGenerated]
		internal static void <CreateVariantXML>g__ReplaceAttribute|1_1(XElement element, XAttribute newAttribute)
		{
			XAttribute existingAttribute = element.Attributes().FirstOrDefault((XAttribute a) => a.Name.ToString().Equals(newAttribute.Name.ToString(), StringComparison.OrdinalIgnoreCase));
			if (existingAttribute == null)
			{
				element.Add(newAttribute);
				return;
			}
			float value;
			float.TryParse(existingAttribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
			if (newAttribute.Value.StartsWith('*'))
			{
				string multiplierStr = newAttribute.Value.Substring(1, newAttribute.Value.Length - 1);
				float multiplier;
				float.TryParse(multiplierStr, NumberStyles.Any, CultureInfo.InvariantCulture, out multiplier);
				if (multiplierStr.Contains('.') || existingAttribute.Value.Contains('.'))
				{
					existingAttribute.Value = (value * multiplier).ToString("G", CultureInfo.InvariantCulture);
					return;
				}
				existingAttribute.Value = ((int)(value * multiplier)).ToString();
				return;
			}
			else
			{
				if (!newAttribute.Value.StartsWith('+'))
				{
					existingAttribute.Value = newAttribute.Value;
					return;
				}
				string additionStr = newAttribute.Value.Substring(1, newAttribute.Value.Length - 1);
				float addition;
				float.TryParse(additionStr, NumberStyles.Any, CultureInfo.InvariantCulture, out addition);
				if (additionStr.Contains('.') || existingAttribute.Value.Contains('.'))
				{
					existingAttribute.Value = (value + addition).ToString("G", CultureInfo.InvariantCulture);
					return;
				}
				existingAttribute.Value = ((int)(value + addition)).ToString();
				return;
			}
		}

		// Token: 0x02000AE3 RID: 2787
		// (Invoke) Token: 0x06005EB7 RID: 24247
		public delegate void VariantXMLChecker(XElement originalElement, [Nullable(2)] XElement variantElement, XElement result);
	}
}
