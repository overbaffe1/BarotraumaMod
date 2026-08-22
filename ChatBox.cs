using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000075 RID: 117
	internal class ChatBox
	{
		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x060010D9 RID: 4313 RVA: 0x000A4033 File Offset: 0x000A2233
		// (set) Token: 0x060010DA RID: 4314 RVA: 0x000A403B File Offset: 0x000A223B
		public bool IsSinglePlayer { get; private set; }

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x060010DB RID: 4315 RVA: 0x000A4044 File Offset: 0x000A2244
		// (set) Token: 0x060010DC RID: 4316 RVA: 0x000A404C File Offset: 0x000A224C
		public bool ToggleOpen
		{
			get
			{
				return this._toggleOpen;
			}
			set
			{
				this.SetToggleOpenState(value, true);
			}
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x000A4058 File Offset: 0x000A2258
		public static ChatBox GetChatBox()
		{
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			if (gameMode == null)
			{
				return null;
			}
			if (!gameMode.IsSinglePlayer)
			{
				GameClient client = GameMain.Client;
				if (client == null)
				{
					return null;
				}
				return client.ChatBox;
			}
			else
			{
				CrewManager crewManager = GameMain.GameSession.CrewManager;
				if (crewManager == null)
				{
					return null;
				}
				return crewManager.ChatBox;
			}
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x000A40AA File Offset: 0x000A22AA
		public static void AutoHideChatBox()
		{
			ChatBox.SetChatBoxOpen(false);
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x000A40B2 File Offset: 0x000A22B2
		private void SetToggleOpenState(bool value, bool setPreference = true)
		{
			this._toggleOpen = value;
			if (setPreference)
			{
				ChatBox.PreferChatBoxOpen = value;
			}
			if (value)
			{
				this.hideableElements.Visible = true;
			}
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x000A40D3 File Offset: 0x000A22D3
		public static void ResetChatBoxOpenState()
		{
			ChatBox chatBox = ChatBox.GetChatBox();
			if (chatBox == null)
			{
				return;
			}
			chatBox.ResetOpenState();
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x000A40E4 File Offset: 0x000A22E4
		public void ResetOpenState()
		{
			this.SetOpen(ChatBox.PreferChatBoxOpen);
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x000A40F1 File Offset: 0x000A22F1
		private static void SetChatBoxOpen(bool isOpen)
		{
			ChatBox chatBox = ChatBox.GetChatBox();
			if (chatBox == null)
			{
				return;
			}
			chatBox.SetOpen(isOpen);
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x000A4103 File Offset: 0x000A2303
		private void SetOpen(bool value)
		{
			this.SetToggleOpenState(value, false);
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x060010E4 RID: 4324 RVA: 0x000A410D File Offset: 0x000A230D
		// (set) Token: 0x060010E5 RID: 4325 RVA: 0x000A411A File Offset: 0x000A231A
		public GUITextBox.OnEnterHandler OnEnterMessage
		{
			get
			{
				return this.InputBox.OnEnterPressed;
			}
			set
			{
				this.InputBox.OnEnterPressed = value;
			}
		}

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x060010E6 RID: 4326 RVA: 0x000A4128 File Offset: 0x000A2328
		// (set) Token: 0x060010E7 RID: 4327 RVA: 0x000A4130 File Offset: 0x000A2330
		public GUIFrame GUIFrame { get; private set; }

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x060010E8 RID: 4328 RVA: 0x000A4139 File Offset: 0x000A2339
		// (set) Token: 0x060010E9 RID: 4329 RVA: 0x000A4141 File Offset: 0x000A2341
		public GUITextBox InputBox { get; private set; }

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x060010EA RID: 4330 RVA: 0x000A414A File Offset: 0x000A234A
		// (set) Token: 0x060010EB RID: 4331 RVA: 0x000A4152 File Offset: 0x000A2352
		public GUIButton ToggleButton
		{
			get
			{
				return this.toggleButton;
			}
			set
			{
				if (this.toggleButton != null)
				{
					this.toggleButton.RectTransform.Parent = null;
				}
				this.toggleButton = value;
			}
		}

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x060010EC RID: 4332 RVA: 0x000A4174 File Offset: 0x000A2374
		// (set) Token: 0x060010ED RID: 4333 RVA: 0x000A417C File Offset: 0x000A237C
		private GUIDropDown ChatModeDropDown { get; set; }

		// Token: 0x060010EE RID: 4334 RVA: 0x000A4188 File Offset: 0x000A2388
		public ChatBox(GUIComponent parent, bool isSinglePlayer)
		{
			this.IsSinglePlayer = isSinglePlayer;
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			int toggleButtonWidth = (int)(30f * GUI.Scale);
			this.GUIFrame = new GUIFrame(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.ChatBoxArea, parent.RectTransform), null, null);
			this.hideableElements = new GUIFrame(new RectTransform(Vector2.One, this.GUIFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame chatBoxHolder = new GUIFrame(new RectTransform(new Vector2(1f, 0.875f), this.hideableElements.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ChatBox", null);
			this.chatBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.95f), chatBoxHolder.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, null, null, true, false);
			this.channelSettingsFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), chatBoxHolder.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 25)
			}, "GUIFrameBottom", null);
			GUILayoutGroup channelSettingsContent = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), this.channelSettingsFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				CanBeFocused = true,
				RelativeSpacing = 0.01f
			};
			this.radioJammedWarning = new GUITextBlock(new RectTransform(Vector2.One, this.channelSettingsFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("radiojammedwarning"), new Color?(GUIStyle.Orange), null, Alignment.Center, false, "OuterGlow", new Color?(Color.Black))
			{
				ToolTip = TextManager.Get("hint.radiojammed")
			};
			GUIButton buttonLeft = new GUIButton(new RectTransform(new Vector2(0.1f, 0.8f), channelSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "DeviceButton", null)
			{
				PlaySoundOnSelect = false,
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					WifiComponent radio;
					if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, false))
					{
						this.SetChannel(radio.Channel - 1, true);
						SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
					}
					return true;
				}
			};
			GUIImage arrowIcon = new GUIImage(new RectTransform(new Vector2(0.4f), buttonLeft.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIButtonHorizontalArrow", true)
			{
				Color = new Color(51, 59, 46),
				SpriteEffects = SpriteEffects.FlipHorizontally
			};
			arrowIcon.HoverColor = (arrowIcon.PressedColor = (arrowIcon.SelectedColor = arrowIcon.Color));
			RectTransform rectT = new RectTransform(new Vector2(0.25f, 0.8f), channelSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			GUIFont digitalFont = GUIStyle.DigitalFont;
			GUITextBox guitextBox = new GUITextBox(rectT, text2, null, digitalFont, Alignment.Center, false, "DigitalFrameLight", null, false, true);
			guitextBox.textFilterFunction = delegate(string text)
			{
				string str = new string((from c in text
				where char.IsNumber(c)
				select c).ToArray<char>());
				if (str.Length > 4)
				{
					str = str.Substring(0, 4);
				}
				return str;
			};
			guitextBox.OnEnterPressed = delegate(GUITextBox tb, string text)
			{
				tb.Deselect();
				return true;
			};
			this.channelText = guitextBox;
			Vector2 textSize = this.channelText.Font.MeasureString("0000", false);
			this.channelText.TextBlock.ToolTip = TextManager.Get("currentradiochannel");
			this.channelText.TextBlock.TextScale = Math.Min((float)this.channelText.Rect.Height / textSize.Y * 0.9f, 1f);
			this.channelText.OnDeselected += delegate(GUITextBox sender, Keys key)
			{
				int newChannel;
				int.TryParse(this.channelText.Text, out newChannel);
				this.SetChannel(newChannel, true);
				SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
			};
			GUIButton buttonRight = new GUIButton(new RectTransform(new Vector2(0.1f, 0.8f), channelSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "DeviceButton", null)
			{
				PlaySoundOnSelect = false,
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					WifiComponent radio;
					if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, false))
					{
						this.SetChannel(radio.Channel + 1, true);
						SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
					}
					return true;
				}
			};
			arrowIcon = new GUIImage(new RectTransform(new Vector2(0.4f), buttonRight.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIButtonHorizontalArrow", true)
			{
				Color = new Color(51, 59, 46)
			};
			arrowIcon.HoverColor = (arrowIcon.PressedColor = (arrowIcon.PressedColor = arrowIcon.Color));
			GUIFrame channelPicker = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.6f), channelSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", null);
			this.channelPickerContent = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), channelPicker.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			for (int i = 0; i < 10; i++)
			{
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), this.channelPickerContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), i.ToString(), Alignment.Center, "GUITextBlock", null);
				guibutton.TextColor = new Color(51, 59, 46);
				guibutton.SelectedTextColor = GUIStyle.Green;
				guibutton.UserData = i;
				guibutton.PlaySoundOnSelect = false;
				guibutton.OnClicked = delegate(GUIButton btn, object userdata)
				{
					WifiComponent radio;
					if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, false))
					{
						int index = (int)userdata;
						if (this.channelMemPending)
						{
							int newChannel;
							int.TryParse(this.channelText.Text, out newChannel);
							this.SetChannelMemory(index, newChannel);
							btn.ToolTip = TextManager.GetWithVariables("radiochannelpreset", new ValueTuple<string, string>[]
							{
								new ValueTuple<string, string>("[index]", index.ToString()),
								new ValueTuple<string, string>("[channel]", radio.GetChannelMemory(index).ToString())
							});
							this.channelMemPending = false;
							this.channelPickerContent.Children.First<GUIComponent>().CanBeFocused = true;
							this.memButton.Enabled = true;
							this.channelPickerContent.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
							this.channelText.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
						this.SetChannel(radio.GetChannelMemory(index), true);
						SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
					}
					return true;
				};
			}
			this.memButton = new GUIButton(new RectTransform(new Vector2(0.2f, 0.9f), channelSettingsContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("saveradiochannelbutton"), Alignment.Center, "DeviceButton", null)
			{
				ToolTip = TextManager.Get("saveradiochannelbuttontooltip"),
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.channelMemPending = true;
					this.channelPickerContent.Children.First<GUIComponent>().CanBeFocused = false;
					foreach (GUIComponent channelButton in this.channelPickerContent.Children)
					{
						channelButton.Selected = false;
					}
					btn.Enabled = false;
					return true;
				}
			};
			GUILayoutGroup bottomContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.125f), this.hideableElements.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			RectTransform dropdownRt = new RectTransform(new Vector2(0.1f, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point((int)(0.45f * (float)bottomContainer.RectTransform.NonScaledSize.X), int.MaxValue)
			};
			ChatMode[] chatModes = new ChatMode[]
			{
				ChatMode.Local,
				ChatMode.Radio
			};
			this.ChatModeDropDown = new GUIDropDown(dropdownRt, null, chatModes.Length, "", false, true, Alignment.CenterLeft, 1f)
			{
				OnSelected = delegate(GUIComponent component, object userdata)
				{
					GameMain.ActiveChatMode = (ChatMode)userdata;
					if (this.InputBox != null && this.InputBox.Text.StartsWith("r; ") && GameMain.ActiveChatMode == ChatMode.Local)
					{
						string text4 = this.InputBox.Text;
						this.InputBox.Text = text4.Remove(0, "r; ".Length);
					}
					return true;
				}
			};
			float longestDropDownOption = 0f;
			foreach (ChatMode mode in chatModes)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
				defaultInterpolatedStringHandler.AppendLiteral("chatmode.");
				defaultInterpolatedStringHandler.AppendFormatted<ChatMode>(mode);
				LocalizedString text3 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				this.ChatModeDropDown.AddItem(text3, mode, null, null, null);
				GUITextBlock textBlock = this.ChatModeDropDown.ListBox.Content.GetChildByUserData(mode) as GUITextBlock;
				if (textBlock != null && textBlock.TextSize.X > longestDropDownOption)
				{
					longestDropDownOption = textBlock.TextSize.X;
				}
			}
			this.ChatModeDropDown.SelectItem(GameMain.ActiveChatMode);
			float num = longestDropDownOption + this.ChatModeDropDown.Padding.X;
			GUIImage dropDownIcon = this.ChatModeDropDown.DropDownIcon;
			float num2 = num + (float)((dropDownIcon != null) ? dropDownIcon.RectTransform.NonScaledSize.X : 0);
			GUIImage dropDownIcon2 = this.ChatModeDropDown.DropDownIcon;
			float minDropDownWidth = num2 + (float)(((dropDownIcon2 != null) ? dropDownIcon2.RectTransform.AbsoluteOffset.X : 0) * 2);
			this.ChatModeDropDown.RectTransform.MinSize = new Point(Math.Max((int)minDropDownWidth, this.ChatModeDropDown.RectTransform.MinSize.X), this.ChatModeDropDown.RectTransform.MinSize.Y);
			this.InputBox = new GUITextBox(new RectTransform(new Vector2(0.9f, 1f), bottomContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "ChatTextBox", null, false, true)
			{
				OverflowClip = true,
				Font = GUIStyle.SmallFont,
				MaxTextLength = new int?(200)
			};
			ChatManager.RegisterKeys(this.InputBox, this.ChatManager);
			this.InputBox.OnDeselected += delegate(GUITextBox gui, Keys Keys)
			{
				this.ChatManager.Clear();
				if (this.GUIFrame.IsParentOf(GUI.MouseOn, true))
				{
					this.CloseAfterMessageSent = false;
					return;
				}
				string message;
				ChatMessage.GetChatMessageCommand(this.InputBox.Text, out message);
				if (string.IsNullOrEmpty(message) && this.CloseAfterMessageSent)
				{
					this._toggleOpen = false;
					this.CloseAfterMessageSent = false;
				}
			};
			GUIButton chatSendButton = new GUIButton(new RectTransform(new Vector2(1f, 0.7f), this.InputBox.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIButtonToggleRight", null);
			GUIButton guibutton2 = chatSendButton;
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				this.InputBox.OnEnterPressed(this.InputBox, this.InputBox.Text);
				return true;
			}));
			chatSendButton.RectTransform.AbsoluteOffset = new Point((int)((float)this.InputBox.Rect.Height * 0.15f), 0);
			this.InputBox.TextBlock.RectTransform.MaxSize = new Point((int)((float)this.InputBox.Rect.Width - (float)chatSendButton.Rect.Width * 1.25f - this.InputBox.TextBlock.Padding.X - (float)chatSendButton.RectTransform.AbsoluteOffset.X), int.MaxValue);
			this.showNewMessagesButton = new GUIButton(new RectTransform(new Vector2(1f, 0.075f), this.GUIFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.125f)
			}, TextManager.Get("chat.shownewmessages"), Alignment.Center, "", null);
			GUIButton guibutton3 = this.showNewMessagesButton;
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				this.chatBox.ScrollBar.BarScrollValue = 1f;
				this.showNewMessagesButton.Visible = false;
				return true;
			}));
			this.showNewMessagesButton.Visible = false;
			this.SetToggleOpenState(GameSettings.CurrentConfig.ChatOpen, true);
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x000A4E31 File Offset: 0x000A3031
		public void Toggle()
		{
			this.ToggleOpen = !this.ToggleOpen;
			this.CloseAfterMessageSent = false;
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x000A4E4C File Offset: 0x000A304C
		public bool TypingChatMessage(GUITextBox textBox, string text)
		{
			string text2;
			string command = ChatMessage.GetChatMessageCommand(text, out text2);
			if (this.IsSinglePlayer && command != "r" && command != "radio")
			{
				command = "";
			}
			Color textColor;
			if (!(command == "r") && !(command == "radio"))
			{
				if (!(command == "d") && !(command == "dead"))
				{
					if (Character.Controlled != null && (Character.Controlled.IsDead || Character.Controlled.SpeechImpediment >= 100f))
					{
						textColor = ChatMessage.MessageColor[2];
					}
					else if (command != "")
					{
						textColor = ChatMessage.MessageColor[5];
					}
					else if (GameMain.ActiveChatMode == ChatMode.Radio)
					{
						textColor = ChatMessage.MessageColor[4];
					}
					else
					{
						textColor = ChatMessage.MessageColor[0];
					}
				}
				else
				{
					textColor = ChatMessage.MessageColor[2];
				}
			}
			else
			{
				textColor = ChatMessage.MessageColor[4];
			}
			textBox.TextColor = (textBox.TextBlock.SelectedTextColor = textColor);
			return true;
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x000A4F64 File Offset: 0x000A3164
		public void AddMessage(ChatMessage message)
		{
			ChatBox.<>c__DisplayClass58_0 CS$<>8__locals1 = new ChatBox.<>c__DisplayClass58_0();
			CS$<>8__locals1.message = message;
			CS$<>8__locals1.<>4__this = this;
			if (GameMain.IsSingleplayer)
			{
				bool? should = null;
				LuaCsSetup.Instance.EventService.PublishEvent<IEventChatMessage>(delegate(IEventChatMessage x)
				{
					bool? flag2 = x.OnChatMessage(CS$<>8__locals1.message.Text, CS$<>8__locals1.message.SenderClient, CS$<>8__locals1.message.Type, CS$<>8__locals1.message);
					should = ((flag2 != null) ? flag2 : should);
				});
				if (should != null && should.Value)
				{
					return;
				}
			}
			while (this.chatBox.Content.CountChildren > 60)
			{
				this.chatBox.RemoveChild(this.chatBox.Content.Children.First<GUIComponent>());
			}
			float prevSize = this.chatBox.BarSize;
			string displayedText = CS$<>8__locals1.message.TranslatedText;
			string senderName = "";
			Color senderColor = Color.White;
			if (!string.IsNullOrWhiteSpace(CS$<>8__locals1.message.SenderName))
			{
				senderName = ((CS$<>8__locals1.message.Type == ChatMessageType.Private) ? "[PM] " : "") + CS$<>8__locals1.message.SenderName;
			}
			Character senderCharacter = CS$<>8__locals1.message.SenderCharacter;
			bool flag;
			if (senderCharacter == null)
			{
				flag = (null != null);
			}
			else
			{
				CharacterInfo info = senderCharacter.Info;
				flag = (((info != null) ? info.Job : null) != null);
			}
			if (flag)
			{
				senderColor = Color.Lerp(CS$<>8__locals1.message.SenderCharacter.Info.Job.Prefab.UIColor, Color.White, 0.25f);
			}
			CS$<>8__locals1.msgHolder = new GUIFrame(new RectTransform(new Vector2(0.95f, 0f), this.chatBox.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), null, new Color?((this.chatBox.Content.CountChildren % 2 == 0) ? Color.Transparent : (Color.Black * 0.1f)));
			CS$<>8__locals1.senderNameTimestamp = new GUITextBlock(new RectTransform(new Vector2(0.98f, 0f), CS$<>8__locals1.msgHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point((int)(5f * GUI.Scale), 0)
			}, ChatMessage.GetTimeStamp(), new Color?(Color.LightGray), GUIStyle.SmallFont, Alignment.TopLeft, false, null, null)
			{
				CanBeFocused = true
			};
			if (!string.IsNullOrEmpty(senderName))
			{
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.8f, 1f), CS$<>8__locals1.senderNameTimestamp.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					AbsoluteOffset = new Point((int)CS$<>8__locals1.senderNameTimestamp.TextSize.X, 0)
				}, senderName, Alignment.TopLeft, null, new Color?(Color.Transparent));
				guibutton.TextBlock.Padding = Vector4.Zero;
				guibutton.Font = GUIStyle.SmallFont;
				guibutton.CanBeFocused = true;
				guibutton.ForceUpperCase = ForceUpperCase.No;
				guibutton.UserData = CS$<>8__locals1.message.SenderClient;
				guibutton.PlaySoundOnSelect = false;
				guibutton.OnClicked = delegate(GUIButton _, object o)
				{
					Client client = o as Client;
					if (client == null)
					{
						return false;
					}
					if (GameMain.NetLobbyScreen != null)
					{
						GameMain.NetLobbyScreen.SelectPlayer(client);
						SoundPlayer.PlayUISound(GUISoundType.Select);
					}
					return true;
				};
				guibutton.OnSecondaryClicked = delegate(GUIComponent _, object o)
				{
					Client client = o as Client;
					if (client == null)
					{
						return false;
					}
					NetLobbyScreen.CreateModerationContextMenu(client);
					return true;
				};
				guibutton.Text = senderName;
				GUIButton senderNameBlock = guibutton;
				senderNameBlock.RectTransform.NonScaledSize = senderNameBlock.TextBlock.TextSize.ToPoint();
				senderNameBlock.TextBlock.OverrideTextColor(senderColor);
				if (senderNameBlock.UserData != null)
				{
					senderNameBlock.TextBlock.HoverTextColor = Color.White;
				}
			}
			CS$<>8__locals1.msgText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.msgHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point((int)(10f * GUI.Scale), (CS$<>8__locals1.senderNameTimestamp == null) ? 0 : CS$<>8__locals1.senderNameTimestamp.Rect.Height)
			}, RichString.Rich(displayedText, null), new Color?(CS$<>8__locals1.message.Color), GUIStyle.SmallFont, Alignment.TopLeft, true, null, new Color?((this.chatBox.Content.CountChildren % 2 == 0) ? Color.Transparent : (Color.Black * 0.1f)))
			{
				UserData = CS$<>8__locals1.message.SenderName,
				CanBeFocused = false
			};
			CS$<>8__locals1.msgText.CalculateHeightFromText(0, false);
			if (CS$<>8__locals1.msgText.RichTextData != null)
			{
				foreach (RichTextData data in CS$<>8__locals1.msgText.RichTextData.Value)
				{
					GUITextBlock.ClickableArea clickableArea = new GUITextBlock.ClickableArea
					{
						Data = data
					};
					if (GameMain.NetLobbyScreen != null && GameMain.NetworkMember != null)
					{
						clickableArea.OnClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.SelectPlayer);
						clickableArea.OnSecondaryClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.ShowPlayerContextMenu);
					}
					CS$<>8__locals1.msgText.ClickableAreas.Add(clickableArea);
				}
			}
			OrderChatMessage orderChatMsg = CS$<>8__locals1.message as OrderChatMessage;
			if (orderChatMsg != null && Character.Controlled != null && orderChatMsg.TargetCharacter == Character.Controlled)
			{
				CS$<>8__locals1.msgHolder.Flash(new Color?(Color.OrangeRed * 0.6f), 5f, false, false, null);
			}
			else
			{
				CS$<>8__locals1.msgHolder.Flash(new Color?(Color.Yellow * 0.6f), 1.5f, false, false, null);
			}
			CS$<>8__locals1.msgHolder.RectTransform.SizeChanged += CS$<>8__locals1.<AddMessage>g__Recalculate|0;
			CS$<>8__locals1.<AddMessage>g__Recalculate|0();
			CoroutineManager.StartCoroutine(this.UpdateMessageAnimation(CS$<>8__locals1.msgHolder, 0.5f), "");
			this.chatBox.UpdateScrollBarSize();
			if (this.chatBox.ScrollBar.Visible && this.chatBox.ScrollBar.BarScroll < 1f)
			{
				this.showNewMessagesButton.Visible = true;
			}
			if (CS$<>8__locals1.message.Type == ChatMessageType.Server && CS$<>8__locals1.message.ChangeType != PlayerConnectionChangeType.None)
			{
				TabMenu.StorePlayerConnectionChangeMessage(CS$<>8__locals1.message);
			}
			if (!this.ToggleOpen)
			{
				GUIFrame popupMsg = new GUIFrame(new RectTransform(Vector2.One, this.GUIFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIToolTip", null)
				{
					UserData = 0f,
					CanBeFocused = false
				};
				GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), popupMsg.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
				Vector2 senderTextSize = Vector2.Zero;
				if (!string.IsNullOrEmpty(senderName))
				{
					GUITextBlock senderText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), senderName, new Color?(senderColor), GUIStyle.SmallFont, Alignment.Left, false, null, null)
					{
						CanBeFocused = false
					};
					senderTextSize = senderText.Font.MeasureString(senderText.WrappedText, false);
					senderText.RectTransform.MinSize = new Point(0, senderText.Rect.Height);
				}
				GUITextBlock msgPopupText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(displayedText, null), new Color?(CS$<>8__locals1.message.Color), GUIStyle.SmallFont, Alignment.BottomLeft, true, null, null)
				{
					CanBeFocused = false
				};
				msgPopupText.RectTransform.MinSize = new Point(0, msgPopupText.Rect.Height);
				Vector2 msgSize = msgPopupText.Font.MeasureString(msgPopupText.WrappedText, false);
				int textWidth = (int)Math.Max(msgSize.X + msgPopupText.Padding.X + msgPopupText.Padding.Z, senderTextSize.X) + 10;
				popupMsg.RectTransform.Resize(new Point((int)((float)textWidth / content.RectTransform.RelativeSize.X), (int)((senderTextSize.Y + msgSize.Y) / content.RectTransform.RelativeSize.Y)), true);
				popupMsg.RectTransform.IsFixedSize = true;
				content.Recalculate();
				this.popupMessages.Add(popupMsg);
			}
			if ((prevSize == 1f && this.chatBox.BarScroll == 0f) || (prevSize < 1f && this.chatBox.BarScroll == 1f))
			{
				this.chatBox.BarScroll = 1f;
			}
			GUISoundType soundType = GUISoundType.ChatMessage;
			if (CS$<>8__locals1.message.Type == ChatMessageType.Radio)
			{
				soundType = GUISoundType.RadioMessage;
			}
			else if (CS$<>8__locals1.message.Type == ChatMessageType.Dead)
			{
				soundType = GUISoundType.DeadMessage;
			}
			SoundPlayer.PlayUISound(soundType);
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x000A592E File Offset: 0x000A3B2E
		public void SetVisibility(bool visible)
		{
			this.GUIFrame.Parent.Visible = visible;
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x000A5941 File Offset: 0x000A3B41
		private IEnumerable<CoroutineStatus> UpdateMessageAnimation(GUIComponent message, float animDuration)
		{
			ChatBox.<UpdateMessageAnimation>d__60 <UpdateMessageAnimation>d__ = new ChatBox.<UpdateMessageAnimation>d__60(-2);
			<UpdateMessageAnimation>d__.<>3__message = message;
			<UpdateMessageAnimation>d__.<>3__animDuration = animDuration;
			return <UpdateMessageAnimation>d__;
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x000A5958 File Offset: 0x000A3B58
		private void SetUILayout()
		{
			this.GUIFrame.RectTransform.AbsoluteOffset = Point.Zero;
			this.GUIFrame.RectTransform.RelativeOffset = new Vector2((float)HUDLayoutSettings.ChatBoxArea.X / (float)GameMain.GraphicsWidth, (float)HUDLayoutSettings.ChatBoxArea.Y / (float)GameMain.GraphicsHeight);
			this.GUIFrame.RectTransform.NonScaledSize = HUDLayoutSettings.ChatBoxArea.Size;
			int toggleButtonWidth = (int)(30f * GUI.Scale);
			this.GUIFrame.RectTransform.NonScaledSize -= new Point(toggleButtonWidth, 0);
			this.GUIFrame.RectTransform.AbsoluteOffset += new Point(toggleButtonWidth, 0);
			this.popupMessageOffset = GameMain.GameSession.CrewManager.ReportButtonFrame.Rect.Width + this.GUIFrame.Rect.Width + (int)(35f * GUI.Scale);
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x000A5A60 File Offset: 0x000A3C60
		public void Update(float deltaTime)
		{
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y || this.prevUIScale != GUI.Scale)
			{
				this.SetUILayout();
				this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
				this.prevUIScale = GUI.Scale;
			}
			if (this.showNewMessagesButton.Visible && this.chatBox.ScrollBar.BarScroll == 1f)
			{
				this.showNewMessagesButton.Visible = false;
			}
			if (Screen.Selected == GameMain.GameScreen && GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (PlayerInput.KeyHit(InputType.ToggleChatMode))
				{
					try
					{
						ChatMode activeChatMode = GameMain.ActiveChatMode;
						ChatMode chatMode;
						if (activeChatMode != ChatMode.Local)
						{
							if (activeChatMode != ChatMode.Radio)
							{
								throw new NotImplementedException();
							}
							chatMode = ChatMode.Local;
						}
						else
						{
							chatMode = ChatMode.Radio;
						}
						ChatMode mode = chatMode;
						this.ChatModeDropDown.SelectItem(mode);
						goto IL_134;
					}
					catch (NotImplementedException)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error toggling chat mode: not implemented for current mode \"");
						defaultInterpolatedStringHandler.AppendFormatted<ChatMode>(GameMain.ActiveChatMode);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						goto IL_134;
					}
				}
				if (PlayerInput.KeyHit(InputType.ChatBox))
				{
					this.Toggle();
				}
			}
			IL_134:
			if (this.ToggleButton != null)
			{
				this.ToggleButton.Selected = this.ToggleOpen;
				this.ToggleButton.RectTransform.AbsoluteOffset = new Point(this.GUIFrame.Rect.Right, this.GUIFrame.Rect.Y + HUDLayoutSettings.ChatBoxArea.Height - this.ToggleButton.Rect.Height);
			}
			WifiComponent radio;
			if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, true))
			{
				if (this.prevRadio != radio)
				{
					foreach (GUIComponent presetButton in this.channelPickerContent.Children)
					{
						int index = (int)presetButton.UserData;
						presetButton.ToolTip = TextManager.GetWithVariables("radiochannelpreset", new ValueTuple<string, string>[]
						{
							new ValueTuple<string, string>("[index]", index.ToString()),
							new ValueTuple<string, string>("[channel]", radio.GetChannelMemory(index).ToString())
						});
					}
					this.SetChannel(radio.Channel, true);
					this.prevRadio = radio;
				}
				if (this.channelMemPending)
				{
					if (this.channelPickerContent.FlashTimer <= 0f)
					{
						this.channelPickerContent.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, new Vector2?(new Vector2(GUI.Scale * 5f)));
					}
					if (PlayerInput.PrimaryMouseButtonClicked() && !GUI.IsMouseOn(this.channelPickerContent))
					{
						this.channelPickerContent.Children.First<GUIComponent>().CanBeFocused = true;
						this.channelMemPending = false;
						this.memButton.Enabled = true;
						this.SetChannel(radio.Channel, true);
					}
				}
				this.channelSettingsFrame.Visible = true;
				this.radioJammedWarning.Visible = (radio != null && radio.JamTimer > 0f);
			}
			else
			{
				this.radioJammedWarning.Visible = (this.channelSettingsFrame.Visible = false);
				this.channelPickerContent.Children.First<GUIComponent>().CanBeFocused = true;
				this.channelMemPending = false;
				this.memButton.Enabled = true;
			}
			if (this.ToggleOpen)
			{
				this.GUIFrame.CanBeFocused = true;
				this.openState += deltaTime * 5f;
				foreach (GUIComponent popupMsg in this.popupMessages)
				{
					popupMsg.Parent.RemoveChild(popupMsg);
				}
				this.popupMessages.Clear();
			}
			else
			{
				this.GUIFrame.CanBeFocused = false;
				this.openState -= deltaTime * 5f;
				int yOffset = 0;
				foreach (GUIComponent popupMsg2 in this.popupMessages)
				{
					float msgTimer = (float)popupMsg2.UserData;
					int targetYOffset = (int)MathHelper.Lerp((float)popupMsg2.RectTransform.ScreenSpaceOffset.Y, (float)yOffset, deltaTime * 10f);
					if (popupMsg2 == this.popupMessages.First<GUIComponent>())
					{
						popupMsg2.UserData = msgTimer + deltaTime * (float)Math.Max(this.popupMessages.Count / 2, 1);
						if (msgTimer > 5f)
						{
							popupMsg2.RectTransform.ScreenSpaceOffset = new Point((int)MathHelper.SmoothStep((float)this.popupMessageOffset, 10f, (msgTimer - 5f) * 5f), targetYOffset);
							if (msgTimer > 5.2f)
							{
								GUIComponent parent = popupMsg2.Parent;
								if (parent != null)
								{
									parent.RemoveChild(popupMsg2);
								}
							}
						}
					}
					if (msgTimer < 5f)
					{
						if (popupMsg2 != this.popupMessages.First<GUIComponent>())
						{
							popupMsg2.UserData = Math.Min(msgTimer + deltaTime, 1f);
						}
						popupMsg2.RectTransform.ScreenSpaceOffset = new Point((int)MathHelper.SmoothStep(0f, (float)this.popupMessageOffset, msgTimer * 5f), targetYOffset);
					}
					yOffset += popupMsg2.Rect.Height + GUI.IntScale(10f);
				}
				this.popupMessages.RemoveAll((GUIComponent p) => p.Parent == null);
			}
			this.openState = MathHelper.Clamp(this.openState, 0f, 1f);
			int hiddenBoxOffset = -this.GUIFrame.Rect.Width;
			this.GUIFrame.RectTransform.AbsoluteOffset = new Point((int)MathHelper.SmoothStep((float)hiddenBoxOffset, 0f, this.openState), 0);
			this.hideableElements.Visible = (this.openState > 0f);
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x000A60FC File Offset: 0x000A42FC
		private void SetChannel(int channel, bool setText)
		{
			WifiComponent radio;
			if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, false))
			{
				radio.Channel = channel;
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.CreateEntityEvent(radio.Item, new Item.ChangePropertyEventData(radio.SerializableProperties["channel".ToIdentifier()], radio));
				}
				if (setText)
				{
					string text = radio.Channel.ToString().PadLeft(4, '0');
					if (this.channelText.Text != text)
					{
						this.channelText.Text = text;
					}
				}
				if (!this.channelMemPending)
				{
					foreach (GUIComponent channelButton in this.channelPickerContent.Children)
					{
						int buttonIndex = (int)channelButton.UserData;
						channelButton.Selected = (radio.GetChannelMemory(buttonIndex) == channel);
					}
				}
			}
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x000A6204 File Offset: 0x000A4404
		private void SetChannelMemory(int index, int channel)
		{
			WifiComponent radio;
			if (Character.Controlled != null && ChatMessage.CanUseRadio(Character.Controlled, out radio, false))
			{
				radio.SetChannelMemory(index, channel);
				radio.Item.CreateClientEvent<WifiComponent>(radio);
			}
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x000A623B File Offset: 0x000A443B
		public void ApplySelectionInputs()
		{
			this.ApplySelectionInputs(this.InputBox, true, ChatBox.ChatKeyStates.GetChatKeyStates());
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x000A6250 File Offset: 0x000A4450
		public void ApplySelectionInputs(GUITextBox inputBox, bool selectInputBox, ChatBox.ChatKeyStates chatKeyStates)
		{
			if (inputBox == null)
			{
				inputBox = this.InputBox;
			}
			ValueTuple<bool, bool, bool> valueTuple = chatKeyStates.Deconstruct();
			bool activeChatKeyHit = valueTuple.Item1;
			bool localChatKeyHit = valueTuple.Item2;
			bool radioChatKeyHit = valueTuple.Item3;
			if (localChatKeyHit || (activeChatKeyHit && GameMain.ActiveChatMode == ChatMode.Local))
			{
				this.ChatModeDropDown.SelectItem(ChatMode.Local);
				inputBox.AddToGUIUpdateList(false, 0);
				this.GUIFrame.Flash(new Color?(Color.DarkGreen), 0.5f, false, false, null);
				if (!this.ToggleOpen)
				{
					this.CloseAfterMessageSent = !this.ToggleOpen;
					this.ToggleOpen = true;
				}
				if (selectInputBox)
				{
					inputBox.Select(inputBox.Text.Length, false);
					return;
				}
			}
			else if (radioChatKeyHit || (activeChatKeyHit && GameMain.ActiveChatMode == ChatMode.Radio))
			{
				this.ChatModeDropDown.SelectItem(ChatMode.Radio);
				inputBox.AddToGUIUpdateList(false, 0);
				this.GUIFrame.Flash(new Color?(Color.YellowGreen), 0.5f, false, false, null);
				if (!this.ToggleOpen)
				{
					this.CloseAfterMessageSent = !this.ToggleOpen;
					this.ToggleOpen = true;
				}
				if (selectInputBox)
				{
					inputBox.Select(inputBox.Text.Length, false);
				}
			}
		}

		// Token: 0x0400084E RID: 2126
		public const string RadioChatString = "r; ";

		// Token: 0x0400084F RID: 2127
		private readonly GUIListBox chatBox;

		// Token: 0x04000850 RID: 2128
		private Point screenResolution;

		// Token: 0x04000851 RID: 2129
		public readonly ChatManager ChatManager = new ChatManager();

		// Token: 0x04000853 RID: 2131
		private bool _toggleOpen = true;

		// Token: 0x04000854 RID: 2132
		private float openState;

		// Token: 0x04000855 RID: 2133
		public static bool PreferChatBoxOpen = true;

		// Token: 0x04000856 RID: 2134
		public bool CloseAfterMessageSent;

		// Token: 0x04000857 RID: 2135
		private float prevUIScale;

		// Token: 0x04000858 RID: 2136
		private readonly GUIFrame channelSettingsFrame;

		// Token: 0x04000859 RID: 2137
		private readonly GUITextBlock radioJammedWarning;

		// Token: 0x0400085A RID: 2138
		private readonly GUITextBox channelText;

		// Token: 0x0400085B RID: 2139
		private readonly GUILayoutGroup channelPickerContent;

		// Token: 0x0400085C RID: 2140
		private readonly GUIButton memButton;

		// Token: 0x0400085D RID: 2141
		private WifiComponent prevRadio;

		// Token: 0x0400085E RID: 2142
		private bool channelMemPending;

		// Token: 0x0400085F RID: 2143
		private const float PopupMessageDuration = 5f;

		// Token: 0x04000860 RID: 2144
		private readonly List<GUIComponent> popupMessages = new List<GUIComponent>();

		// Token: 0x04000863 RID: 2147
		private GUIButton toggleButton;

		// Token: 0x04000864 RID: 2148
		private readonly GUIButton showNewMessagesButton;

		// Token: 0x04000865 RID: 2149
		private readonly GUIFrame hideableElements;

		// Token: 0x04000866 RID: 2150
		public const int ToggleButtonWidthRaw = 30;

		// Token: 0x04000867 RID: 2151
		private int popupMessageOffset;

		// Token: 0x0200090C RID: 2316
		public struct ChatKeyStates
		{
			// Token: 0x17001A4C RID: 6732
			// (get) Token: 0x06007091 RID: 28817 RVA: 0x003696BD File Offset: 0x003678BD
			// (set) Token: 0x06007092 RID: 28818 RVA: 0x003696C5 File Offset: 0x003678C5
			public bool ActiveChatKeyHit { readonly get; set; }

			// Token: 0x17001A4D RID: 6733
			// (get) Token: 0x06007093 RID: 28819 RVA: 0x003696CE File Offset: 0x003678CE
			// (set) Token: 0x06007094 RID: 28820 RVA: 0x003696D6 File Offset: 0x003678D6
			public bool LocalChatKeyHit { readonly get; set; }

			// Token: 0x17001A4E RID: 6734
			// (get) Token: 0x06007095 RID: 28821 RVA: 0x003696DF File Offset: 0x003678DF
			// (set) Token: 0x06007096 RID: 28822 RVA: 0x003696E7 File Offset: 0x003678E7
			public bool RadioChatKeyHit { readonly get; set; }

			// Token: 0x17001A4F RID: 6735
			// (get) Token: 0x06007097 RID: 28823 RVA: 0x003696F0 File Offset: 0x003678F0
			public bool AnyHit
			{
				get
				{
					return this.ActiveChatKeyHit || this.LocalChatKeyHit || this.RadioChatKeyHit;
				}
			}

			// Token: 0x06007098 RID: 28824 RVA: 0x0036970A File Offset: 0x0036790A
			private ChatKeyStates(bool active, bool local, bool radio)
			{
				this.ActiveChatKeyHit = active;
				this.LocalChatKeyHit = local;
				this.RadioChatKeyHit = radio;
			}

			// Token: 0x06007099 RID: 28825 RVA: 0x00369721 File Offset: 0x00367921
			public static ChatBox.ChatKeyStates GetChatKeyStates()
			{
				return new ChatBox.ChatKeyStates(PlayerInput.KeyHit(InputType.ActiveChat), PlayerInput.KeyHit(InputType.Chat), PlayerInput.KeyHit(InputType.RadioChat) && (Character.Controlled == null || Character.Controlled.SpeechImpediment < 100f));
			}

			// Token: 0x0600709A RID: 28826 RVA: 0x0036975D File Offset: 0x0036795D
			[return: TupleElementNames(new string[]
			{
				"active",
				"local",
				"radio"
			})]
			public ValueTuple<bool, bool, bool> Deconstruct()
			{
				return new ValueTuple<bool, bool, bool>(this.ActiveChatKeyHit, this.LocalChatKeyHit, this.RadioChatKeyHit);
			}
		}
	}
}
