using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020003A1 RID: 929
	[NullableContext(1)]
	[Nullable(0)]
	public static class SafeXML
	{
		// Token: 0x06004546 RID: 17734 RVA: 0x002682B0 File Offset: 0x002664B0
		public static void SaveSafe(this XDocument doc, string path, SaveOptions saveOptions = SaveOptions.None, bool throwExceptions = false, int maxRetries = 0)
		{
			if (Validation.CanWrite(path, false))
			{
				for (int i = 0; i <= maxRetries; i++)
				{
					try
					{
						doc.Save(path, saveOptions);
						break;
					}
					catch (IOException e)
					{
						if (i >= maxRetries)
						{
							throw;
						}
						DebugConsole.NewMessage("Failed save XML document {" + e.Message + "}, retrying in 250 ms...", null, false);
						Thread.Sleep(250);
					}
				}
				return;
			}
			string errorMsg = "Cannot save XML document to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.";
			if (throwExceptions)
			{
				throw new InvalidOperationException(errorMsg);
			}
			DebugConsole.ThrowError(errorMsg, null, null, false, false);
		}

		// Token: 0x06004547 RID: 17735 RVA: 0x0026834C File Offset: 0x0026654C
		public static void SaveSafe(this XElement element, string path, bool throwExceptions = false)
		{
			if (Validation.CanWrite(path, false))
			{
				element.Save(path);
				return;
			}
			string errorMsg = "Cannot save XML element to \"" + path + "\": modifying the files in this folder/with this extension is not allowed.";
			if (throwExceptions)
			{
				throw new InvalidOperationException(errorMsg);
			}
			DebugConsole.ThrowError(errorMsg, null, null, false, false);
		}

		// Token: 0x06004548 RID: 17736 RVA: 0x0026838F File Offset: 0x0026658F
		public static void SaveSafe(this XDocument doc, XmlWriter writer)
		{
			doc.WriteTo(writer);
		}

		// Token: 0x06004549 RID: 17737 RVA: 0x00268398 File Offset: 0x00266598
		public static void WriteTo(this XDocument doc, XmlWriter writer)
		{
			writer.Write(doc);
		}
	}
}
