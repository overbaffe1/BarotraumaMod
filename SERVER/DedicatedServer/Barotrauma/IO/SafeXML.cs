using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;

namespace Barotrauma.IO
{
	// Token: 0x020002D7 RID: 727
	[NullableContext(1)]
	[Nullable(0)]
	public static class SafeXML
	{
		// Token: 0x060030FA RID: 12538 RVA: 0x0014FE3C File Offset: 0x0014E03C
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

		// Token: 0x060030FB RID: 12539 RVA: 0x0014FED8 File Offset: 0x0014E0D8
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

		// Token: 0x060030FC RID: 12540 RVA: 0x0014FF1B File Offset: 0x0014E11B
		public static void SaveSafe(this XDocument doc, XmlWriter writer)
		{
			doc.WriteTo(writer);
		}

		// Token: 0x060030FD RID: 12541 RVA: 0x0014FF24 File Offset: 0x0014E124
		public static void WriteTo(this XDocument doc, XmlWriter writer)
		{
			writer.Write(doc);
		}
	}
}
