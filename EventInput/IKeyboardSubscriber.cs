using System;
using Microsoft.Xna.Framework.Input;

namespace EventInput
{
	// Token: 0x0200001A RID: 26
	public interface IKeyboardSubscriber
	{
		// Token: 0x06000103 RID: 259
		void ReceiveTextInput(char inputChar);

		// Token: 0x06000104 RID: 260
		void ReceiveTextInput(string text);

		// Token: 0x06000105 RID: 261
		void ReceiveCommandInput(char command);

		// Token: 0x06000106 RID: 262
		void ReceiveSpecialInput(Keys key);

		// Token: 0x06000107 RID: 263
		void ReceiveEditingInput(string text, int start, int length);

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000108 RID: 264
		// (set) Token: 0x06000109 RID: 265
		bool Selected { get; set; }
	}
}
