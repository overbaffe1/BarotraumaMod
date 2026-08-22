using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace EventInput
{
	// Token: 0x02000019 RID: 25
	public static class EventInput
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x060000F6 RID: 246 RVA: 0x000067DC File Offset: 0x000049DC
		// (remove) Token: 0x060000F7 RID: 247 RVA: 0x00006810 File Offset: 0x00004A10
		public static event CharEnteredHandler CharEntered;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x060000F8 RID: 248 RVA: 0x00006844 File Offset: 0x00004A44
		// (remove) Token: 0x060000F9 RID: 249 RVA: 0x00006878 File Offset: 0x00004A78
		public static event KeyEventHandler KeyDown;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060000FA RID: 250 RVA: 0x000068AC File Offset: 0x00004AAC
		// (remove) Token: 0x060000FB RID: 251 RVA: 0x000068E0 File Offset: 0x00004AE0
		public static event KeyEventHandler KeyUp;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060000FC RID: 252 RVA: 0x00006914 File Offset: 0x00004B14
		// (remove) Token: 0x060000FD RID: 253 RVA: 0x00006948 File Offset: 0x00004B48
		public static event EditingTextHandler EditingText;

		// Token: 0x060000FE RID: 254 RVA: 0x0000697C File Offset: 0x00004B7C
		public static void Initialize(GameWindow window)
		{
			if (EventInput.initialized)
			{
				return;
			}
			EventHandler<TextInputEventArgs> value;
			if ((value = EventInput.<>O.<0>__ReceiveInput) == null)
			{
				value = (EventInput.<>O.<0>__ReceiveInput = new EventHandler<TextInputEventArgs>(EventInput.ReceiveInput));
			}
			window.TextInput += value;
			EventHandler<TextInputEventArgs> value2;
			if ((value2 = EventInput.<>O.<1>__ReceiveKeyDown) == null)
			{
				value2 = (EventInput.<>O.<1>__ReceiveKeyDown = new EventHandler<TextInputEventArgs>(EventInput.ReceiveKeyDown));
			}
			window.KeyDown += value2;
			EventHandler<TextEditingEventArgs> value3;
			if ((value3 = EventInput.<>O.<2>__ReceiveTextEditing) == null)
			{
				value3 = (EventInput.<>O.<2>__ReceiveTextEditing = new EventHandler<TextEditingEventArgs>(EventInput.ReceiveTextEditing));
			}
			window.TextEditing += value3;
			EventInput.initialized = true;
		}

		// Token: 0x060000FF RID: 255 RVA: 0x000069FA File Offset: 0x00004BFA
		private static void ReceiveInput(object sender, TextInputEventArgs e)
		{
			EventInput.OnCharEntered(e.Character);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00006A07 File Offset: 0x00004C07
		private static void ReceiveKeyDown(object sender, TextInputEventArgs e)
		{
			KeyEventHandler keyDown = EventInput.KeyDown;
			if (keyDown == null)
			{
				return;
			}
			keyDown(sender, new KeyEventArgs(e.Key, e.Character));
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00006A2A File Offset: 0x00004C2A
		private static void ReceiveTextEditing(object sender, TextEditingEventArgs e)
		{
			EditingTextHandler editingText = EventInput.EditingText;
			if (editingText == null)
			{
				return;
			}
			editingText(sender, e);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00006A3D File Offset: 0x00004C3D
		public static void OnCharEntered(char character)
		{
			CharEnteredHandler charEntered = EventInput.CharEntered;
			if (charEntered == null)
			{
				return;
			}
			charEntered(null, new CharacterEventArgs(character, 0L));
		}

		// Token: 0x040000C9 RID: 201
		private static bool initialized;

		// Token: 0x02000638 RID: 1592
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003622 RID: 13858
			public static EventHandler<TextInputEventArgs> <0>__ReceiveInput;

			// Token: 0x04003623 RID: 13859
			public static EventHandler<TextInputEventArgs> <1>__ReceiveKeyDown;

			// Token: 0x04003624 RID: 13860
			public static EventHandler<TextEditingEventArgs> <2>__ReceiveTextEditing;
		}
	}
}
