using System;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020002D9 RID: 729
	public static class XmlWriterExtensions
	{
		// Token: 0x06003103 RID: 12547 RVA: 0x0014FFE2 File Offset: 0x0014E1E2
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
