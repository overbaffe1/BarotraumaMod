using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000259 RID: 601
	[NullableContext(1)]
	[Nullable(0)]
	public static class ContentXElementExtensions
	{
		// Token: 0x0600381D RID: 14365 RVA: 0x00216E47 File Offset: 0x00215047
		public static ContentXElement FromPackage(this XElement element, [Nullable(2)] ContentPackage contentPackage)
		{
			return new ContentXElement(contentPackage, element);
		}

		// Token: 0x0600381E RID: 14366 RVA: 0x00216E50 File Offset: 0x00215050
		public static IEnumerable<ContentXElement> Elements(this IEnumerable<ContentXElement> elements)
		{
			return elements.SelectMany((ContentXElement e) => e.Elements());
		}
	}
}
