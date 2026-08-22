using System;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020003A2 RID: 930
	[NullableContext(1)]
	[Nullable(0)]
	public class XmlWriter : IDisposable
	{
		// Token: 0x0600454A RID: 17738 RVA: 0x002683A1 File Offset: 0x002665A1
		public XmlWriter(string path, XmlWriterSettings settings)
		{
			if (!Validation.CanWrite(path, false))
			{
				DebugConsole.ThrowError("Cannot write XML document to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				this.Writer = null;
				return;
			}
			this.Writer = XmlWriter.Create(path, settings);
		}

		// Token: 0x0600454B RID: 17739 RVA: 0x002683E0 File Offset: 0x002665E0
		public static XmlWriter Create(string path, XmlWriterSettings settings)
		{
			return new XmlWriter(path, settings);
		}

		// Token: 0x0600454C RID: 17740 RVA: 0x002683E9 File Offset: 0x002665E9
		public void Write(XDocument doc)
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot write to invalid XmlWriter", null, null, false, false);
				return;
			}
			doc.WriteTo(this.Writer);
		}

		// Token: 0x0600454D RID: 17741 RVA: 0x0026840E File Offset: 0x0026660E
		public void Flush()
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot flush invalid XmlWriter", null, null, false, false);
				return;
			}
			this.Writer.Flush();
		}

		// Token: 0x0600454E RID: 17742 RVA: 0x00268432 File Offset: 0x00266632
		public void Dispose()
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot dispose invalid XmlWriter", null, null, false, false);
				return;
			}
			this.Writer.Dispose();
		}

		// Token: 0x04002421 RID: 9249
		[Nullable(2)]
		public readonly XmlWriter Writer;
	}
}
