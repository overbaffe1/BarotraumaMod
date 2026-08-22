using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AD RID: 1197
	internal class Terminal : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x06004375 RID: 17269 RVA: 0x001B144C File Offset: 0x001AF64C
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			string newOutputValue = msg.ReadString();
			if (this.item.CanClientAccess(c) && !this.Readonly)
			{
				if (newOutputValue.Length > 200)
				{
					newOutputValue = newOutputValue.Substring(0, 200);
				}
				GameServer.Log(string.Concat(new string[]
				{
					GameServer.CharacterLogName(c.Character),
					" entered \"",
					newOutputValue,
					"\" on ",
					this.item.Name
				}), ServerLog.MessageType.ItemInteraction);
				this.OutputValue = newOutputValue;
				this.ShowOnDisplay(newOutputValue, true, this.TextColor, false);
				this.item.SendSignal(newOutputValue, "signal_out");
				this.item.CreateServerEvent<Terminal>(this);
			}
		}

		// Token: 0x06004376 RID: 17270 RVA: 0x001B150C File Offset: 0x001AF70C
		public void SyncHistory()
		{
			int msgIndex = 0;
			foreach (TerminalMessage msg in this.messageHistory)
			{
				if (!msg.IsWelcomeMessage)
				{
					string msgToSend = msg.Text;
					if (string.IsNullOrEmpty(msgToSend))
					{
						this.item.CreateServerEvent<Terminal>(this, new Terminal.ServerEventData(msgIndex, msgToSend));
						msgIndex++;
					}
					else
					{
						if (msgToSend.Length > 200)
						{
							List<string> splitMessage = msgToSend.Split(' ', StringSplitOptions.None).ToList<string>();
							for (int i = 0; i < splitMessage.Count; i++)
							{
								if (splitMessage[i].Length > 200)
								{
									string temp = splitMessage[i];
									splitMessage[i] = temp.Substring(0, 200);
									splitMessage.Insert(i + 1, temp.Substring(200, temp.Length - 200));
								}
							}
							while (msgToSend.Length > 200)
							{
								string tempMsg = "";
								do
								{
									tempMsg += splitMessage[0];
									splitMessage.RemoveAt(0);
									if (!splitMessage.Any<string>())
									{
										break;
									}
									tempMsg += " ";
								}
								while (tempMsg.Length + splitMessage[0].Length < 200);
								this.item.CreateServerEvent<Terminal>(this, new Terminal.ServerEventData(msgIndex, tempMsg));
								msgToSend = msgToSend.Remove(0, tempMsg.Length);
							}
						}
						if (!string.IsNullOrEmpty(msgToSend))
						{
							this.item.CreateServerEvent<Terminal>(this, new Terminal.ServerEventData(msgIndex, msgToSend));
						}
						msgIndex++;
					}
				}
			}
		}

		// Token: 0x06004377 RID: 17271 RVA: 0x001B16E8 File Offset: 0x001AF8E8
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Terminal.ServerEventData eventData;
			if (base.TryExtractEventData<Terminal.ServerEventData>(extraData, out eventData))
			{
				msg.WriteString(eventData.MsgToSend);
				return;
			}
			msg.WriteString(this.OutputValue);
		}

		// Token: 0x170011F3 RID: 4595
		// (get) Token: 0x06004378 RID: 17272 RVA: 0x001B1719 File Offset: 0x001AF919
		// (set) Token: 0x06004379 RID: 17273 RVA: 0x001B1721 File Offset: 0x001AF921
		public LocalizedString DisplayedWelcomeMessage { get; private set; }

		// Token: 0x170011F4 RID: 4596
		// (get) Token: 0x0600437A RID: 17274 RVA: 0x001B172A File Offset: 0x001AF92A
		// (set) Token: 0x0600437B RID: 17275 RVA: 0x001B1734 File Offset: 0x001AF934
		[InGameEditable]
		[Serialize("", IsPropertySaveable.Yes, "Message to be displayed on the terminal display when it is first opened.", "terminalwelcomemsg.", true)]
		public string WelcomeMessage
		{
			get
			{
				return this.welcomeMessage;
			}
			set
			{
				if (this.welcomeMessage == value)
				{
					return;
				}
				this.welcomeMessage = value;
				this.DisplayedWelcomeMessage = TextManager.Get(this.welcomeMessage).Fallback(this.welcomeMessage.Replace("\\n", "\n"), true);
			}
		}

		// Token: 0x170011F5 RID: 4597
		// (get) Token: 0x0600437C RID: 17276 RVA: 0x001B1788 File Offset: 0x001AF988
		// (set) Token: 0x0600437D RID: 17277 RVA: 0x001B17AD File Offset: 0x001AF9AD
		public string ShowMessage
		{
			get
			{
				if (this.messageHistory.Count != 0)
				{
					return this.messageHistory.Last<TerminalMessage>().Text;
				}
				return string.Empty;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				this.ShowOnDisplay(value, true, this.TextColor, false);
			}
		}

		// Token: 0x170011F6 RID: 4598
		// (get) Token: 0x0600437E RID: 17278 RVA: 0x001B17C7 File Offset: 0x001AF9C7
		// (set) Token: 0x0600437F RID: 17279 RVA: 0x001B17CF File Offset: 0x001AF9CF
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "The terminal will use a monospace font if this box is ticked.", "", true)]
		public bool UseMonospaceFont { get; set; }

		// Token: 0x170011F7 RID: 4599
		// (get) Token: 0x06004380 RID: 17280 RVA: 0x001B17D8 File Offset: 0x001AF9D8
		// (set) Token: 0x06004381 RID: 17281 RVA: 0x001B17E0 File Offset: 0x001AF9E0
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AutoHideScrollbar { get; set; }

		// Token: 0x170011F8 RID: 4600
		// (get) Token: 0x06004382 RID: 17282 RVA: 0x001B17E9 File Offset: 0x001AF9E9
		// (set) Token: 0x06004383 RID: 17283 RVA: 0x001B17F1 File Offset: 0x001AF9F1
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool WelcomeMessageDisplayed { get; set; }

		// Token: 0x170011F9 RID: 4601
		// (get) Token: 0x06004384 RID: 17284 RVA: 0x001B17FA File Offset: 0x001AF9FA
		// (set) Token: 0x06004385 RID: 17285 RVA: 0x001B1802 File Offset: 0x001AFA02
		[Editable]
		[Serialize("50,205,50,255", IsPropertySaveable.Yes, "Color of the terminal text.", "", true)]
		public Color TextColor
		{
			get
			{
				return this.textColor;
			}
			set
			{
				this.textColor = value;
			}
		}

		// Token: 0x170011FA RID: 4602
		// (get) Token: 0x06004386 RID: 17286 RVA: 0x001B180B File Offset: 0x001AFA0B
		// (set) Token: 0x06004387 RID: 17287 RVA: 0x001B1813 File Offset: 0x001AFA13
		[Editable]
		[Serialize("> ", IsPropertySaveable.Yes, "", "", false)]
		public string LineStartSymbol { get; set; }

		// Token: 0x170011FB RID: 4603
		// (get) Token: 0x06004388 RID: 17288 RVA: 0x001B181C File Offset: 0x001AFA1C
		// (set) Token: 0x06004389 RID: 17289 RVA: 0x001B1824 File Offset: 0x001AFA24
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool Readonly
		{
			get
			{
				return this._readonly;
			}
			set
			{
				this._readonly = value;
			}
		}

		// Token: 0x170011FC RID: 4604
		// (get) Token: 0x0600438A RID: 17290 RVA: 0x001B182D File Offset: 0x001AFA2D
		// (set) Token: 0x0600438B RID: 17291 RVA: 0x001B1835 File Offset: 0x001AFA35
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool AutoScrollToBottom { get; set; }

		// Token: 0x170011FD RID: 4605
		// (get) Token: 0x0600438C RID: 17292 RVA: 0x001B183E File Offset: 0x001AFA3E
		// (set) Token: 0x0600438D RID: 17293 RVA: 0x001B1846 File Offset: 0x001AFA46
		private string OutputValue { get; set; }

		// Token: 0x0600438E RID: 17294 RVA: 0x001B184F File Offset: 0x001AFA4F
		public Terminal(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600438F RID: 17295 RVA: 0x001B1878 File Offset: 0x001AFA78
		private void ShowOnDisplay(string input, bool addToHistory, Color color, bool isWelcomeMessage)
		{
			if (addToHistory)
			{
				this.messageHistory.Add(new TerminalMessage(input, color, isWelcomeMessage));
				while (this.messageHistory.Count > 60)
				{
					this.messageHistory.RemoveAt(0);
				}
			}
		}

		// Token: 0x06004390 RID: 17296 RVA: 0x001B18B0 File Offset: 0x001AFAB0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "set_text") && !(name == "signal_in"))
			{
				if (!(name == "set_text_color"))
				{
					if (!(name == "clear_text"))
					{
						return;
					}
					if (signal.value != "0")
					{
						this.messageHistory.Clear();
					}
				}
				else if (signal.value != this.prevColorSignal)
				{
					this.TextColor = XMLExtensions.ParseColor(signal.value, false);
					this.prevColorSignal = signal.value;
					return;
				}
				return;
			}
			if (string.IsNullOrEmpty(signal.value))
			{
				return;
			}
			if (signal.value.Length > 200)
			{
				signal.value = signal.value.Substring(0, 200);
			}
			string inputSignal = signal.value.Replace("\\n", "\n");
			this.ShowOnDisplay(inputSignal, true, this.TextColor, false);
		}

		// Token: 0x06004391 RID: 17297 RVA: 0x001B19AC File Offset: 0x001AFBAC
		public override void OnItemLoaded()
		{
			bool isSubEditor = false;
			base.OnItemLoaded();
			if (!this.DisplayedWelcomeMessage.IsNullOrEmpty() && !this.WelcomeMessageDisplayed)
			{
				this.ShowOnDisplay(this.DisplayedWelcomeMessage.Value, !isSubEditor, this.TextColor, true);
				this.DisplayedWelcomeMessage = "";
				if (GameMain.GameSession != null && !isSubEditor)
				{
					this.WelcomeMessageDisplayed = true;
				}
			}
		}

		// Token: 0x06004392 RID: 17298 RVA: 0x001B1A14 File Offset: 0x001AFC14
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			for (int i = 0; i < this.messageHistory.Count; i++)
			{
				TerminalMessage msg = this.messageHistory[i];
				componentElement.Add(new XAttribute("msg" + i.ToString(), msg.Text));
				componentElement.Add(new XAttribute("color" + i.ToString(), msg.Color.ToStringHex()));
				if (msg.IsWelcomeMessage)
				{
					componentElement.Add(new XAttribute("welcomemessage" + i.ToString(), true));
				}
			}
			return componentElement;
		}

		// Token: 0x06004393 RID: 17299 RVA: 0x001B1AD8 File Offset: 0x001AFCD8
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			for (int i = 0; i < 60; i++)
			{
				string msg = componentElement.GetAttributeString("msg" + i.ToString(), null);
				if (msg == null)
				{
					break;
				}
				string key = "color" + i.ToString();
				Color color2 = this.TextColor;
				Color color = componentElement.GetAttributeColor(key, color2);
				bool isWelcomeMessage = componentElement.GetAttributeBool("welcomemessage" + i.ToString(), false);
				this.ShowOnDisplay(msg, true, color, isWelcomeMessage);
			}
		}

		// Token: 0x04002041 RID: 8257
		private const int MaxMessageLength = 200;

		// Token: 0x04002042 RID: 8258
		private const int MaxMessages = 60;

		// Token: 0x04002043 RID: 8259
		private readonly List<TerminalMessage> messageHistory = new List<TerminalMessage>(60);

		// Token: 0x04002045 RID: 8261
		private string welcomeMessage;

		// Token: 0x04002049 RID: 8265
		private Color textColor = Color.LimeGreen;

		// Token: 0x0400204B RID: 8267
		private bool _readonly;

		// Token: 0x0400204E RID: 8270
		private string prevColorSignal;

		// Token: 0x02000DF2 RID: 3570
		private readonly struct ServerEventData : ItemComponent.IEventData
		{
			// Token: 0x060068E5 RID: 26853 RVA: 0x00223AEF File Offset: 0x00221CEF
			public ServerEventData(int msgIndex, string msgToSend)
			{
				this.MsgIndex = msgIndex;
				this.MsgToSend = msgToSend;
			}

			// Token: 0x04004141 RID: 16705
			public readonly int MsgIndex;

			// Token: 0x04004142 RID: 16706
			public readonly string MsgToSend;
		}
	}
}
