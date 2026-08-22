using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002CB RID: 715
	public class RichTextData
	{
		// Token: 0x06003037 RID: 12343 RVA: 0x0014B310 File Offset: 0x00149510
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

		// Token: 0x0400181F RID: 6175
		public int StartIndex;

		// Token: 0x04001820 RID: 6176
		public int EndIndex;

		// Token: 0x04001821 RID: 6177
		public Color? Color;

		// Token: 0x04001822 RID: 6178
		public string Metadata;

		// Token: 0x04001823 RID: 6179
		public float Alpha = 1f;

		// Token: 0x04001824 RID: 6180
		private const char definitionIndicator = '‖';

		// Token: 0x04001825 RID: 6181
		private const char attributeSeparator = ';';

		// Token: 0x04001826 RID: 6182
		private const char keyValueSeparator = ':';

		// Token: 0x04001827 RID: 6183
		private const string colorDefinition = "color";

		// Token: 0x04001828 RID: 6184
		private const string metadataDefinition = "metadata";

		// Token: 0x04001829 RID: 6185
		private const string endDefinition = "end";
	}
}
