using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000623 RID: 1571
	internal readonly struct TerminalMessage
	{
		// Token: 0x0600649E RID: 25758 RVA: 0x00341A77 File Offset: 0x0033FC77
		public TerminalMessage(string text, Color color, bool isWelcomeMessage)
		{
			this.Text = text;
			this.Color = color;
			this.IsWelcomeMessage = isWelcomeMessage;
		}

		// Token: 0x0600649F RID: 25759 RVA: 0x00341A8E File Offset: 0x0033FC8E
		public void Deconstruct(out string text, out Color color)
		{
			text = this.Text;
			color = this.Color;
		}

		// Token: 0x04003436 RID: 13366
		public readonly string Text;

		// Token: 0x04003437 RID: 13367
		public readonly Color Color;

		// Token: 0x04003438 RID: 13368
		public readonly bool IsWelcomeMessage;
	}
}
