using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000396 RID: 918
	public class RichTextData
	{
		// Token: 0x060044B5 RID: 17589 RVA: 0x00264DD4 File Offset: 0x00262FD4
		public static ImmutableArray<RichTextData>? GetRichTextData(string text, out string sanitizedText)
		{
			sanitizedText = text;
			if (!string.IsNullOrEmpty(text) && text.Contains('‖', StringComparison.Ordinal))
			{
				text = text.Replace("\r", "");
				string[] segments = text.Split('‖', StringSplitOptions.None);
				sanitizedText = string.Empty;
				List<RichTextData> textColors = new List<RichTextData>();
				RichTextData tempData = null;
				int prevIndex = 0;
				int currIndex = 0;
				for (int i = 0; i < segments.Length; i++)
				{
					if (i % 2 == 0)
					{
						sanitizedText += segments[i];
						prevIndex = currIndex;
						currIndex += segments[i].Replace("\n", "").Replace("\r", "").Length;
					}
					else
					{
						string[] attributes = segments[i].Split(';', StringSplitOptions.None);
						for (int j = 0; j < attributes.Length; j++)
						{
							if (attributes[j].Contains("end", StringComparison.OrdinalIgnoreCase))
							{
								if (tempData != null)
								{
									tempData.StartIndex = prevIndex;
									tempData.EndIndex = currIndex - 1;
									textColors.Add(tempData);
								}
								tempData = null;
							}
							else if (attributes[j].StartsWith("color", StringComparison.OrdinalIgnoreCase))
							{
								if (tempData == null)
								{
									tempData = new RichTextData();
								}
								string valueStr = attributes[j].Substring(attributes[j].IndexOf(':') + 1);
								if (valueStr.Equals("null", StringComparison.InvariantCultureIgnoreCase))
								{
									tempData.Color = null;
								}
								else
								{
									tempData.Color = new Color?(XMLExtensions.ParseColor(valueStr, true));
								}
							}
							else if (attributes[j].StartsWith("metadata", StringComparison.OrdinalIgnoreCase))
							{
								if (tempData == null)
								{
									tempData = new RichTextData();
								}
								tempData.Metadata = attributes[j].Substring(attributes[j].IndexOf(':') + 1);
							}
						}
					}
				}
				return new ImmutableArray<RichTextData>?(textColors.ToImmutableArray<RichTextData>());
			}
			return null;
		}

		// Token: 0x040023F1 RID: 9201
		public int StartIndex;

		// Token: 0x040023F2 RID: 9202
		public int EndIndex;

		// Token: 0x040023F3 RID: 9203
		public Color? Color;

		// Token: 0x040023F4 RID: 9204
		public string Metadata;

		// Token: 0x040023F5 RID: 9205
		public float Alpha = 1f;

		// Token: 0x040023F6 RID: 9206
		private const char definitionIndicator = '‖';

		// Token: 0x040023F7 RID: 9207
		private const char attributeSeparator = ';';

		// Token: 0x040023F8 RID: 9208
		private const char keyValueSeparator = ':';

		// Token: 0x040023F9 RID: 9209
		private const string colorDefinition = "color";

		// Token: 0x040023FA RID: 9210
		private const string metadataDefinition = "metadata";

		// Token: 0x040023FB RID: 9211
		private const string endDefinition = "end";
	}
}
