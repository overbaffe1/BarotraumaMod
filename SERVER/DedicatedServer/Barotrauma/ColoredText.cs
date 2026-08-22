using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016B RID: 363
	internal readonly struct ColoredText
	{
		// Token: 0x06001D60 RID: 7520 RVA: 0x000D198C File Offset: 0x000CFB8C
		public ColoredText(string text, Color color, bool isCommand, bool isError)
		{
			this.Text = text;
			this.Color = color;
			this.IsCommand = isCommand;
			this.IsError = isError;
			this.Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x04000D3D RID: 3389
		public readonly string Text;

		// Token: 0x04000D3E RID: 3390
		public readonly Color Color;

		// Token: 0x04000D3F RID: 3391
		public readonly bool IsCommand;

		// Token: 0x04000D40 RID: 3392
		public readonly bool IsError;

		// Token: 0x04000D41 RID: 3393
		public readonly string Time;
	}
}
