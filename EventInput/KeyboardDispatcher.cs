using System;
using Barotrauma;
using Microsoft.Xna.Framework;

namespace EventInput
{
	// Token: 0x0200001B RID: 27
	public class KeyboardDispatcher
	{
		// Token: 0x0600010A RID: 266 RVA: 0x00006A58 File Offset: 0x00004C58
		public KeyboardDispatcher(GameWindow window)
		{
			EventInput.Initialize(window);
			EventInput.CharEntered += this.EventInput_CharEntered;
			EventInput.KeyDown += this.EventInput_KeyDown;
			EventInput.EditingText += this.EventInput_TextEditing;
			GameMain.ResetIMEWorkaround();
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00006AA9 File Offset: 0x00004CA9
		public void EventInput_TextEditing(object sender, TextEditingEventArgs e)
		{
			IKeyboardSubscriber subscriber = this._subscriber;
			if (subscriber == null)
			{
				return;
			}
			subscriber.ReceiveEditingInput(e.Text, e.Start, e.Length);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00006ACD File Offset: 0x00004CCD
		public void EventInput_KeyDown(object sender, KeyEventArgs e)
		{
			IKeyboardSubscriber subscriber = this._subscriber;
			if (subscriber != null)
			{
				subscriber.ReceiveSpecialInput(e.KeyCode);
			}
			if (char.IsControl(e.Character))
			{
				IKeyboardSubscriber subscriber2 = this._subscriber;
				if (subscriber2 == null)
				{
					return;
				}
				subscriber2.ReceiveCommandInput(e.Character);
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00006B0C File Offset: 0x00004D0C
		private void EventInput_CharEntered(object sender, CharacterEventArgs e)
		{
			IKeyboardSubscriber subscriber = this._subscriber;
			if (subscriber == null)
			{
				return;
			}
			subscriber.ReceiveTextInput(e.Character);
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600010E RID: 270 RVA: 0x00006B25 File Offset: 0x00004D25
		// (set) Token: 0x0600010F RID: 271 RVA: 0x00006B30 File Offset: 0x00004D30
		public IKeyboardSubscriber Subscriber
		{
			get
			{
				return this._subscriber;
			}
			set
			{
				if (this._subscriber == value)
				{
					return;
				}
				if (this._subscriber is GUITextBox)
				{
					TextInput.StopTextInput();
					this._subscriber.Selected = false;
				}
				GUITextBox box = value as GUITextBox;
				if (box != null)
				{
					TextInput.SetTextInputRect(box.MouseRect);
					TextInput.StartTextInput();
					TextInput.SetTextInputRect(box.MouseRect);
				}
				this._subscriber = value;
				if (value != null)
				{
					value.Selected = true;
				}
			}
		}

		// Token: 0x040000CA RID: 202
		private IKeyboardSubscriber _subscriber;
	}
}
