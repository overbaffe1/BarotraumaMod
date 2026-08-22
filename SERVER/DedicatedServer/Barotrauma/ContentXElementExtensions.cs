using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000164 RID: 356
	[NullableContext(1)]
	[Nullable(0)]
	public static class ContentXElementExtensions
	{
		// Token: 0x06001D3A RID: 7482 RVA: 0x000D122F File Offset: 0x000CF42F
		public static ContentXElement FromPackage(this XElement element, [Nullable(2)] ContentPackage contentPackage)
		{
			return new ContentXElement(contentPackage, element);
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x000D1238 File Offset: 0x000CF438
		public static IEnumerable<ContentXElement> Elements(this IEnumerable<ContentXElement> elements)
		{
			return elements.SelectMany((ContentXElement e) => e.Elements());
		}
	}
}
