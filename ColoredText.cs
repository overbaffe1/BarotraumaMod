using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000260 RID: 608
	internal readonly struct ColoredText
	{
		// Token: 0x06003843 RID: 14403 RVA: 0x002175C0 File Offset: 0x002157C0
		public ColoredText(string text, Color color, bool isCommand, bool isError)
		{
			this.Text = text;
			this.Color = color;
			this.IsCommand = isCommand;
			this.IsError = isError;
			this.Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x04001C42 RID: 7234
		public readonly string Text;

		// Token: 0x04001C43 RID: 7235
		public readonly Color Color;

		// Token: 0x04001C44 RID: 7236
		public readonly bool IsCommand;

		// Token: 0x04001C45 RID: 7237
		public readonly bool IsError;

		// Token: 0x04001C46 RID: 7238
		public readonly string Time;
	}
}
