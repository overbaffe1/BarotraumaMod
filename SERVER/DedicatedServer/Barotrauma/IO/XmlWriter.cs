using System;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020002D8 RID: 728
	[NullableContext(1)]
	[Nullable(0)]
	public class XmlWriter : IDisposable
	{
		// Token: 0x060030FE RID: 12542 RVA: 0x0014FF2D File Offset: 0x0014E12D
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

		// Token: 0x060030FF RID: 12543 RVA: 0x0014FF6C File Offset: 0x0014E16C
		public static XmlWriter Create(string path, XmlWriterSettings settings)
		{
			return new XmlWriter(path, settings);
		}

		// Token: 0x06003100 RID: 12544 RVA: 0x0014FF75 File Offset: 0x0014E175
		public void Write(XDocument doc)
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot write to invalid XmlWriter", null, null, false, false);
				return;
			}
			doc.WriteTo(this.Writer);
		}

		// Token: 0x06003101 RID: 12545 RVA: 0x0014FF9A File Offset: 0x0014E19A
		public void Flush()
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot flush invalid XmlWriter", null, null, false, false);
				return;
			}
			this.Writer.Flush();
		}

		// Token: 0x06003102 RID: 12546 RVA: 0x0014FFBE File Offset: 0x0014E1BE
		public void Dispose()
		{
			if (this.Writer == null)
			{
				DebugConsole.ThrowError("Cannot dispose invalid XmlWriter", null, null, false, false);
				return;
			}
			this.Writer.Dispose();
		}

		// Token: 0x04001853 RID: 6227
		[Nullable(2)]
		public readonly XmlWriter Writer;
	}
}
