using System;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020003A3 RID: 931
	public static class XmlWriterExtensions
	{
		// Token: 0x0600454F RID: 17743 RVA: 0x00268456 File Offset: 0x00266656
		[NullableContext(1)]
		public static void Save(this XDocument doc, XmlWriter writer)
		{
			XmlWriter writer2 = writer.Writer;
			if (writer2 == null)
			{
				throw new NullReferenceException("Unable to save XML document: XML writer is null.");
			}
			doc.Save(writer2);
		}
	}
}
