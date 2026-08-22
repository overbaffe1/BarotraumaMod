using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000034 RID: 52
	public class ChatManager
	{
		// Token: 0x060008DA RID: 2266 RVA: 0x0004FDF4 File Offset: 0x0004DFF4
		public ChatManager(bool loop, short maxCount)
		{
			this.loop = loop;
			this.maxCount = maxCount;
			this.localChanges = new string[(int)maxCount];
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0004FE4C File Offset: 0x0004E04C
		public ChatManager()
		{
			this.localChanges = new string[(int)this.maxCount];
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0004FE9C File Offset: 0x0004E09C
		public static void RegisterKeys(GUITextBox element, ChatManager manager)
		{
			if (manager.registers.Any((GUITextBox p) => element == p))
			{
				return;
			}
			element.OnKeyHit += delegate(GUITextBox sender, Keys key)
			{
				if (key != Keys.Tab && (key == Keys.Up || key == Keys.Down))
				{
					ChatManager.Direction direction = (key == Keys.Up) ? ChatManager.Direction.Up : ((key == Keys.Down) ? ChatManager.Direction.Down : ChatManager.Direction.Other);
					string newMessage = manager.SelectMessage(direction, element.Text);
					if (newMessage == null)
					{
						return;
					}
					element.Text = newMessage;
				}
			};
			manager.registers.Add(element);
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0004FF0C File Offset: 0x0004E10C
		public void Store(string message)
		{
			this.Clear();
			string strip = ChatManager.<Store>g__StripMessage|9_0(message);
			if (string.IsNullOrWhiteSpace(strip))
			{
				return;
			}
			if (this.messageList.Count > 1 && this.messageList[1] == message)
			{
				return;
			}
			this.messageList.Insert(1, message);
			if (this.messageList.Count > (int)this.maxCount)
			{
				this.messageList.RemoveAt(this.messageList.Count - 1);
			}
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x0004FF8A File Offset: 0x0004E18A
		public void Clear()
		{
			this.index = 0;
			this.localChanges = new string[(int)this.maxCount];
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0004FFA4 File Offset: 0x0004E1A4
		private string SelectMessage(ChatManager.Direction direction, string original)
		{
			int originalIndex = this.index;
			while (direction != ChatManager.Direction.Other)
			{
				this.localChanges[this.index] = original;
				int nextIndex = (int)(this.index + direction);
				if (this.loop && this.messageList.Count > 1)
				{
					nextIndex = this.<SelectMessage>g__LoopAround|11_1(nextIndex);
				}
				else if (nextIndex > this.messageList.Count - 1)
				{
					return null;
				}
				if (nextIndex >= 0 && this.<SelectMessage>g__EntryAt|11_0(nextIndex) == original && nextIndex != originalIndex && originalIndex != 0)
				{
					this.index = nextIndex;
				}
				else
				{
					if (nextIndex >= 0)
					{
						return this.<SelectMessage>g__EntryAt|11_0(this.index = nextIndex);
					}
					return this.localChanges.FirstOrDefault<string>();
				}
			}
			return null;
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0005004C File Offset: 0x0004E24C
		[CompilerGenerated]
		internal static string <Store>g__StripMessage|9_0(string text)
		{
			string msg;
			ChatMessage.GetChatMessageCommand(text, out msg);
			return msg;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x00050063 File Offset: 0x0004E263
		[CompilerGenerated]
		private string <SelectMessage>g__EntryAt|11_0(int i)
		{
			return this.localChanges[i] ?? this.messageList[i];
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0005007D File Offset: 0x0004E27D
		[CompilerGenerated]
		private int <SelectMessage>g__LoopAround|11_1(int next)
		{
			if (next > this.messageList.Count - 1)
			{
				return 1;
			}
			if (next < 1)
			{
				return this.messageList.Count - 1;
			}
			return next;
		}

		// Token: 0x04000492 RID: 1170
		private readonly bool loop;

		// Token: 0x04000493 RID: 1171
		private readonly short maxCount = 10;

		// Token: 0x04000494 RID: 1172
		private readonly List<string> messageList = new List<string>
		{
			string.Empty
		};

		// Token: 0x04000495 RID: 1173
		private readonly List<GUITextBox> registers = new List<GUITextBox>();

		// Token: 0x04000496 RID: 1174
		private int index;

		// Token: 0x04000497 RID: 1175
		private string[] localChanges;

		// Token: 0x0200071A RID: 1818
		private enum Direction
		{
			// Token: 0x040038BF RID: 14527
			Up = 1,
			// Token: 0x040038C0 RID: 14528
			Down = -1,
			// Token: 0x040038C1 RID: 14529
			Other
		}
	}
}
