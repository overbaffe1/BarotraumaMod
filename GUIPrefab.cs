using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200008F RID: 143
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class GUIPrefab : Prefab
	{
		// Token: 0x060013D1 RID: 5073 RVA: 0x000BE368 File Offset: 0x000BC568
		public GUIPrefab(ContentXElement element, UIStyleFile file) : base(file, element)
		{
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x000BE372 File Offset: 0x000BC572
		protected override Identifier DetermineIdentifier(XElement element)
		{
			return element.NameAsIdentifier();
		}

		// Token: 0x060013D3 RID: 5075 RVA: 0x000BE37C File Offset: 0x000BC57C
		protected int ParseSize(XElement element, string attributeName)
		{
			string valueStr = element.GetAttributeString(attributeName, string.Empty);
			bool relativeToWidth = valueStr.EndsWith("vw");
			bool relativeToHeight = valueStr.EndsWith("vh");
			if (relativeToWidth || relativeToHeight)
			{
				string floatStr = valueStr.Substring(0, valueStr.Length - 2);
				float relativeHeight;
				if (!float.TryParse(floatStr, NumberStyles.Any, CultureInfo.InvariantCulture, out relativeHeight))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error while parsing a ");
					defaultInterpolatedStringHandler.AppendFormatted("GUIComponentStyle");
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendFormatted(valueStr);
					defaultInterpolatedStringHandler.AppendLiteral(" is not a valid size.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return (int)(relativeHeight / 100f * (float)(relativeToWidth ? GameMain.GraphicsWidth : GameMain.GraphicsHeight));
			}
			return element.GetAttributeInt(attributeName, 0);
		}
	}
}
