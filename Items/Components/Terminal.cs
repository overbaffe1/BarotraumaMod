using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DE RID: 1502
	internal class Terminal : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x06006136 RID: 24886 RVA: 0x00329ACC File Offset: 0x00327CCC
		private void RefreshInputElements()
		{
			foreach (GUIComponent inputElement in this.inputElements)
			{
				inputElement.Visible = !this._readonly;
				inputElement.IgnoreLayoutGroups = !inputElement.Visible;
			}
			GUILayoutGroup guilayoutGroup = this.layoutGroup;
			if (guilayoutGroup == null)
			{
				return;
			}
			guilayoutGroup.Recalculate();
		}

		// Token: 0x06006137 RID: 24887 RVA: 0x00329B48 File Offset: 0x00327D48
		public GUIComponent CreateFillerBlock()
		{
			this.fillerBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 1f), this.historyBox.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null)
			{
				CanBeFocused = false
			};
			return this.fillerBlock;
		}

		// Token: 0x06006138 RID: 24888 RVA: 0x00329BD4 File Offset: 0x00327DD4
		private void SendOutput(string input)
		{
			if (input.Length > 200)
			{
				input = input.Substring(0, 200);
			}
			this.OutputValue = input;
			this.ShowOnDisplay(input, true, this.TextColor, false);
			this.item.SendSignal(input, "signal_out");
		}

		// Token: 0x06006139 RID: 24889 RVA: 0x00329C23 File Offset: 0x00327E23
		public override bool Select(Character character)
		{
			this.shouldSelectInputBox = true;
			return base.Select(character);
		}

		// Token: 0x0600613A RID: 24890 RVA: 0x00329C33 File Offset: 0x00327E33
		public override void AddToGUIUpdateList(int order = 0)
		{
			base.AddToGUIUpdateList(order);
			if (this.shouldSelectInputBox && !this.Readonly)
			{
				this.inputBox.Select(-1, false);
				this.shouldSelectInputBox = false;
			}
		}

		// Token: 0x0600613B RID: 24891 RVA: 0x00329C60 File Offset: 0x00327E60
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			Terminal.ClientEventData eventData;
			if (base.TryExtractEventData<Terminal.ClientEventData>(extraData, out eventData))
			{
				msg.WriteString(eventData.Text);
			}
		}

		// Token: 0x0600613C RID: 24892 RVA: 0x00329C84 File Offset: 0x00327E84
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SendOutput(msg.ReadString());
		}

		// Token: 0x17001888 RID: 6280
		// (get) Token: 0x0600613D RID: 24893 RVA: 0x00329C92 File Offset: 0x00327E92
		// (set) Token: 0x0600613E RID: 24894 RVA: 0x00329C9A File Offset: 0x00327E9A
		public LocalizedString DisplayedWelcomeMessage { get; private set; }

		// Token: 0x17001889 RID: 6281
		// (get) Token: 0x0600613F RID: 24895 RVA: 0x00329CA3 File Offset: 0x00327EA3
		// (set) Token: 0x06006140 RID: 24896 RVA: 0x00329CAC File Offset: 0x00327EAC
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

		// Token: 0x1700188A RID: 6282
		// (get) Token: 0x06006141 RID: 24897 RVA: 0x00329D00 File Offset: 0x00327F00
		// (set) Token: 0x06006142 RID: 24898 RVA: 0x00329D25 File Offset: 0x00327F25
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

		// Token: 0x1700188B RID: 6283
		// (get) Token: 0x06006143 RID: 24899 RVA: 0x00329D3F File Offset: 0x00327F3F
		// (set) Token: 0x06006144 RID: 24900 RVA: 0x00329D47 File Offset: 0x00327F47
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "The terminal will use a monospace font if this box is ticked.", "", true)]
		public bool UseMonospaceFont { get; set; }

		// Token: 0x1700188C RID: 6284
		// (get) Token: 0x06006145 RID: 24901 RVA: 0x00329D50 File Offset: 0x00327F50
		// (set) Token: 0x06006146 RID: 24902 RVA: 0x00329D58 File Offset: 0x00327F58
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AutoHideScrollbar { get; set; }

		// Token: 0x1700188D RID: 6285
		// (get) Token: 0x06006147 RID: 24903 RVA: 0x00329D61 File Offset: 0x00327F61
		// (set) Token: 0x06006148 RID: 24904 RVA: 0x00329D69 File Offset: 0x00327F69
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool WelcomeMessageDisplayed { get; set; }

		// Token: 0x1700188E RID: 6286
		// (get) Token: 0x06006149 RID: 24905 RVA: 0x00329D72 File Offset: 0x00327F72
		// (set) Token: 0x0600614A RID: 24906 RVA: 0x00329D7C File Offset: 0x00327F7C
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
				GUITextBox input = this.inputBox;
				if (input != null)
				{
					input.TextColor = value;
				}
			}
		}

		// Token: 0x1700188F RID: 6287
		// (get) Token: 0x0600614B RID: 24907 RVA: 0x00329DA1 File Offset: 0x00327FA1
		// (set) Token: 0x0600614C RID: 24908 RVA: 0x00329DA9 File Offset: 0x00327FA9
		[Editable]
		[Serialize("> ", IsPropertySaveable.Yes, "", "", false)]
		public string LineStartSymbol { get; set; }

		// Token: 0x17001890 RID: 6288
		// (get) Token: 0x0600614D RID: 24909 RVA: 0x00329DB2 File Offset: 0x00327FB2
		// (set) Token: 0x0600614E RID: 24910 RVA: 0x00329DBA File Offset: 0x00327FBA
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
				this.RefreshInputElements();
			}
		}

		// Token: 0x17001891 RID: 6289
		// (get) Token: 0x0600614F RID: 24911 RVA: 0x00329DC9 File Offset: 0x00327FC9
		// (set) Token: 0x06006150 RID: 24912 RVA: 0x00329DD1 File Offset: 0x00327FD1
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool AutoScrollToBottom { get; set; }

		// Token: 0x17001892 RID: 6290
		// (get) Token: 0x06006151 RID: 24913 RVA: 0x00329DDA File Offset: 0x00327FDA
		// (set) Token: 0x06006152 RID: 24914 RVA: 0x00329DE2 File Offset: 0x00327FE2
		private string OutputValue { get; set; }

		// Token: 0x06006153 RID: 24915 RVA: 0x00329DEB File Offset: 0x00327FEB
		public Terminal(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.InitProjSpecific(element);
		}

		// Token: 0x06006154 RID: 24916 RVA: 0x00329E2C File Offset: 0x0032802C
		private void InitProjSpecific(XElement element)
		{
			float marginMultiplier = element.GetAttributeFloat("marginmultiplier", 1f);
			this.layoutGroup = new GUILayoutGroup(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin.Multiply(marginMultiplier), base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset.Multiply(marginMultiplier)
			}, false, Anchor.TopLeft)
			{
				ChildAnchor = Anchor.TopCenter,
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			this.historyBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.9f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				AutoHideScrollBar = this.AutoHideScrollbar
			};
			this.inputElements.Add(this.CreateFillerBlock());
			this.inputElements.Add(new GUIFrame(new RectTransform(new Vector2(0.9f, 0.01f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null));
			this.inputBox = new GUITextBox(new RectTransform(new Vector2(1f, 0.1f), this.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(this.TextColor), null, Alignment.Left, false, "", null, false, true)
			{
				MaxTextLength = new int?(200),
				OverflowClip = true,
				OnEnterPressed = delegate(GUITextBox textBox, string text)
				{
					if (GameMain.NetworkMember == null)
					{
						this.SendOutput(text);
					}
					else
					{
						this.item.CreateClientEvent<Terminal>(this, new Terminal.ClientEventData(text));
					}
					textBox.Text = string.Empty;
					return true;
				}
			};
			this.inputElements.Add(this.inputBox);
			this.RefreshInputElements();
		}

		// Token: 0x06006155 RID: 24917 RVA: 0x0032A038 File Offset: 0x00328238
		private void ShowOnDisplay(string input, bool addToHistory, Color color, bool isWelcomeMessage)
		{
			if (addToHistory)
			{
				this.messageHistory.Add(new TerminalMessage(input, color, isWelcomeMessage));
				while (this.messageHistory.Count > 60)
				{
					this.messageHistory.RemoveAt(0);
				}
				while (this.historyBox.Content.CountChildren > 60)
				{
					this.historyBox.RemoveChild(this.historyBox.Content.Children.First<GUIComponent>());
				}
			}
			GUITextBlock newBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), this.historyBox.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), this.LineStartSymbol + TextManager.Get(input).Fallback(input, true), new Color?(color), this.UseMonospaceFont ? GUIStyle.MonospacedFont : GUIStyle.Font, Alignment.Left, true, "", null)
			{
				CanBeFocused = false
			};
			if (this.fillerBlock != null)
			{
				float y = this.fillerBlock.RectTransform.RelativeSize.Y - newBlock.RectTransform.RelativeSize.Y;
				if (y > 0f)
				{
					this.fillerBlock.RectTransform.RelativeSize = new Vector2(1f, y);
				}
				else
				{
					this.historyBox.RemoveChild(this.fillerBlock);
					this.fillerBlock = null;
				}
			}
			this.historyBox.RecalculateChildren();
			this.historyBox.UpdateScrollBarSize();
			if (this.AutoScrollToBottom)
			{
				this.historyBox.ScrollBar.BarScrollValue = 1f;
			}
		}

		// Token: 0x06006156 RID: 24918 RVA: 0x0032A1F0 File Offset: 0x003283F0
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
						GUIListBox guilistBox = this.historyBox;
						GUIFrame history = (guilistBox != null) ? guilistBox.Content : null;
						if (history != null)
						{
							history.ClearChildren();
						}
						this.CreateFillerBlock();
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

		// Token: 0x06006157 RID: 24919 RVA: 0x0032A30C File Offset: 0x0032850C
		public override void OnItemLoaded()
		{
			bool flag;
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				GameSession gameSession = GameMain.GameSession;
				flag = (((gameSession != null) ? gameSession.GameMode : null) is TestGameMode);
			}
			else
			{
				flag = true;
			}
			bool isSubEditor = flag;
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

		// Token: 0x06006158 RID: 24920 RVA: 0x0032A39C File Offset: 0x0032859C
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

		// Token: 0x06006159 RID: 24921 RVA: 0x0032A460 File Offset: 0x00328660
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

		// Token: 0x04003221 RID: 12833
		private GUIListBox historyBox;

		// Token: 0x04003222 RID: 12834
		private GUITextBlock fillerBlock;

		// Token: 0x04003223 RID: 12835
		private GUITextBox inputBox;

		// Token: 0x04003224 RID: 12836
		private GUILayoutGroup layoutGroup;

		// Token: 0x04003225 RID: 12837
		private bool shouldSelectInputBox;

		// Token: 0x04003226 RID: 12838
		private readonly List<GUIComponent> inputElements = new List<GUIComponent>();

		// Token: 0x04003227 RID: 12839
		private const int MaxMessageLength = 200;

		// Token: 0x04003228 RID: 12840
		private const int MaxMessages = 60;

		// Token: 0x04003229 RID: 12841
		private readonly List<TerminalMessage> messageHistory = new List<TerminalMessage>(60);

		// Token: 0x0400322B RID: 12843
		private string welcomeMessage;

		// Token: 0x0400322F RID: 12847
		private Color textColor = Color.LimeGreen;

		// Token: 0x04003231 RID: 12849
		private bool _readonly;

		// Token: 0x04003234 RID: 12852
		private string prevColorSignal;

		// Token: 0x02001474 RID: 5236
		private readonly struct ClientEventData : ItemComponent.IEventData
		{
			// Token: 0x06009AFB RID: 39675 RVA: 0x003E3F4B File Offset: 0x003E214B
			public ClientEventData(string text)
			{
				this.Text = text;
			}

			// Token: 0x040065CF RID: 26063
			public readonly string Text;
		}
	}
}
