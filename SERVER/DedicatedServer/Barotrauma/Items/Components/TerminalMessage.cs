using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000500 RID: 1280
	internal readonly struct TerminalMessage
	{
		// Token: 0x060047CE RID: 18382 RVA: 0x001C8DBB File Offset: 0x001C6FBB
		public TerminalMessage(string text, Color color, bool isWelcomeMessage)
		{
			this.Text = text;
			this.Color = color;
			this.IsWelcomeMessage = isWelcomeMessage;
		}

		// Token: 0x060047CF RID: 18383 RVA: 0x001C8DD2 File Offset: 0x001C6FD2
		public void Deconstruct(out string text, out Color color)
		{
			text = this.Text;
			color = this.Color;
		}

		// Token: 0x040022B2 RID: 8882
		public readonly string Text;

		// Token: 0x040022B3 RID: 8883
		public readonly Color Color;

		// Token: 0x040022B4 RID: 8884
		public readonly bool IsWelcomeMessage;
	}
}
