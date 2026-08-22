using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000458 RID: 1112
	internal class ChatMessage
	{
		// Token: 0x06004A95 RID: 19093 RVA: 0x00290168 File Offset: 0x0028E368
		public virtual void ClientWrite(in SegmentTableWriter<ClientNetSegment> segmentTableWriter, IWriteMessage msg)
		{
			segmentTableWriter.StartNewSegment(ClientNetSegment.ChatMessage);
			msg.WriteUInt16(this.NetStateID);
			msg.WriteRangedInteger((int)this.Type, 0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			msg.WriteRangedInteger((int)this.ChatMode, 0, Enum.GetValues(typeof(ChatMode)).Length - 1);
			msg.WriteString(this.Text);
		}

		// Token: 0x06004A96 RID: 19094 RVA: 0x002901DC File Offset: 0x0028E3DC
		public static void ClientRead(IReadMessage msg)
		{
			ushort id = msg.ReadUInt16();
			ChatMessageType type = (ChatMessageType)msg.ReadRangedInteger(0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None;
			string styleSetting = string.Empty;
			if (type != ChatMessageType.Order)
			{
				changeType = (PlayerConnectionChangeType)msg.ReadByte();
			}
			string txt = msg.ReadString();
			string senderName = msg.ReadString();
			Entity sender = null;
			Character senderCharacter = null;
			Client senderClient = null;
			bool hasSenderClient = msg.ReadBoolean();
			if (hasSenderClient)
			{
				string userId = msg.ReadString();
				senderClient = GameMain.Client.ConnectedClients.Find((Client c) => c.SessionOrAccountIdMatches(userId));
				if (senderClient != null)
				{
					senderName = senderClient.Name;
				}
			}
			bool hasSender = msg.ReadBoolean();
			if (hasSender)
			{
				sender = Entity.FindEntityByID(msg.ReadUInt16());
				senderCharacter = (sender as Character);
				bool flag = sender is Character || sender is Item;
				if (flag)
				{
					senderName = OrderChatMessage.NameFromEntityOrNull(sender);
				}
			}
			Color? textColor = null;
			if (msg.ReadBoolean())
			{
				textColor = new Color?(msg.ReadColorR8G8B8A8());
			}
			msg.ReadPadBits();
			if (type != ChatMessageType.Default)
			{
				switch (type)
				{
				case ChatMessageType.Order:
				{
					OrderChatMessage.OrderMessageInfo orderMessageInfo = OrderChatMessage.ReadOrder(msg);
					Identifier identifier = orderMessageInfo.OrderIdentifier;
					if (identifier == Identifier.Empty)
					{
						DebugConsole.ThrowError("Invalid order message - order index out of bounds.", null, null, false, false);
						if (NetIdUtils.IdMoreRecent(id, ChatMessage.LastID))
						{
							ChatMessage.LastID = id;
						}
						return;
					}
					OrderPrefab orderPrefab = orderMessageInfo.OrderPrefab ?? OrderPrefab.Prefabs[orderMessageInfo.OrderIdentifier];
					Identifier orderOption = orderMessageInfo.OrderOption;
					Identifier identifier2;
					if (orderMessageInfo.OrderOptionIndex != null)
					{
						int? orderOptionIndex = orderMessageInfo.OrderOptionIndex;
						int num = 0;
						if (orderOptionIndex.GetValueOrDefault() >= num & orderOptionIndex != null)
						{
							orderOptionIndex = orderMessageInfo.OrderOptionIndex;
							num = orderPrefab.Options.Length;
							if (orderOptionIndex.GetValueOrDefault() < num & orderOptionIndex != null)
							{
								identifier2 = orderPrefab.Options[orderMessageInfo.OrderOptionIndex.Value];
								goto IL_228;
							}
						}
					}
					identifier2 = Identifier.Empty;
					IL_228:
					identifier = identifier2;
					orderOption = orderOption.IfEmpty(identifier);
					Hull targetHull = orderMessageInfo.TargetEntity as Hull;
					if (targetHull != null)
					{
						string targetRoom = targetHull.DisplayName.Value;
					}
					else if (senderCharacter != null)
					{
						Hull currentHull = senderCharacter.CurrentHull;
						if (currentHull != null)
						{
							LocalizedString displayName = currentHull.DisplayName;
							string text = (displayName != null) ? displayName.Value : null;
						}
					}
					if (GameMain.Client.GameStarted && Screen.Selected == GameMain.GameScreen)
					{
						Order order = null;
						switch (orderMessageInfo.TargetType)
						{
						case Order.OrderTargetType.Entity:
							order = new Order(orderPrefab, orderOption, orderMessageInfo.TargetEntity, orderPrefab.GetTargetItemComponent(orderMessageInfo.TargetEntity as Item), senderCharacter, false);
							break;
						case Order.OrderTargetType.Position:
							order = new Order(orderPrefab, orderOption, orderMessageInfo.TargetPosition, senderCharacter);
							break;
						case Order.OrderTargetType.WallSection:
							order = new Order(orderPrefab, orderOption, orderMessageInfo.TargetEntity as Structure, orderMessageInfo.WallSectionIndex, senderCharacter);
							break;
						}
						if (order != null)
						{
							order = order.WithManualPriority(orderMessageInfo.Priority);
							if (order.TargetAllCharacters)
							{
								float? fadeOutTime = (!orderPrefab.IsIgnoreOrder) ? new float?(orderPrefab.FadeOutTime) : null;
								GameSession gameSession = GameMain.GameSession;
								if (gameSession != null)
								{
									CrewManager crewManager = gameSession.CrewManager;
									if (crewManager != null)
									{
										crewManager.AddOrder(order, fadeOutTime);
									}
								}
							}
							else
							{
								Character targetCharacter = orderMessageInfo.TargetCharacter;
								if (targetCharacter != null)
								{
									targetCharacter.SetOrder(order, orderMessageInfo.IsNewOrder, true, false);
								}
							}
						}
					}
					if (NetIdUtils.IdMoreRecent(id, ChatMessage.LastID))
					{
						Order order2;
						if (orderMessageInfo.TargetPosition != null)
						{
							order2 = new Order(orderPrefab, orderOption, orderMessageInfo.TargetPosition, senderCharacter).WithManualPriority(orderMessageInfo.Priority);
						}
						else if (orderMessageInfo.WallSectionIndex != null)
						{
							order2 = new Order(orderPrefab, orderOption, orderMessageInfo.TargetEntity as Structure, orderMessageInfo.WallSectionIndex, senderCharacter).WithManualPriority(orderMessageInfo.Priority);
						}
						else
						{
							order2 = new Order(orderPrefab, orderOption, orderMessageInfo.TargetEntity, orderPrefab.GetTargetItemComponent(orderMessageInfo.TargetEntity as Item), senderCharacter, false).WithManualPriority(orderMessageInfo.Priority);
						}
						GameMain.Client.AddChatMessage(new OrderChatMessage(order2, txt, orderMessageInfo.TargetCharacter, senderCharacter, true));
						ChatMessage.LastID = id;
					}
					return;
				}
				case ChatMessageType.ServerMessageBox:
					txt = TextManager.GetServerMessage(txt).Value;
					break;
				case ChatMessageType.ServerMessageBoxInGame:
					styleSetting = msg.ReadString();
					txt = TextManager.GetServerMessage(txt).Value;
					break;
				case ChatMessageType.BlockedBySpamFilter:
					GameMain.Client.BlockedBySpamFilterTimer = 10f;
					break;
				}
			}
			if (NetIdUtils.IdMoreRecent(id, ChatMessage.LastID))
			{
				switch (type)
				{
				case ChatMessageType.Console:
					DebugConsole.NewMessage(txt, new Color?((textColor == null) ? ChatMessage.MessageColor[6] : textColor.Value), false);
					goto IL_65D;
				case ChatMessageType.MessageBox:
				case ChatMessageType.ServerMessageBox:
				{
					GUIMessageBox guimessageBox = GUIMessageBox.VisibleBox as GUIMessageBox;
					RichString a;
					if (guimessageBox == null)
					{
						a = null;
					}
					else
					{
						GUITextBlock text2 = guimessageBox.Text;
						a = ((text2 != null) ? text2.Text : null);
					}
					if (!(a != txt))
					{
						goto IL_65D;
					}
					GUIMessageBox messageBox = new GUIMessageBox("", txt, null, null, GUIMessageBox.Type.Default);
					if (textColor != null)
					{
						messageBox.Text.TextColor = textColor.Value;
						goto IL_65D;
					}
					goto IL_65D;
				}
				case ChatMessageType.ServerLog:
				{
					ServerLog.MessageType messageType;
					if (!Enum.TryParse<ServerLog.MessageType>(senderName, out messageType))
					{
						return;
					}
					ServerLog serverLog = GameMain.Client.ServerSettings.ServerLog;
					if (serverLog == null)
					{
						goto IL_65D;
					}
					serverLog.WriteLine(txt, messageType, true);
					goto IL_65D;
				}
				case ChatMessageType.ServerMessageBoxInGame:
				{
					RichString headerText = "";
					RichString text3 = txt;
					LocalizedString[] buttons = Array.Empty<LocalizedString>();
					string iconStyle = styleSetting;
					GUIMessageBox messageBox2 = new GUIMessageBox(headerText, text3, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.InGame, "", null, iconStyle, null, null, false);
					if (textColor != null)
					{
						messageBox2.Text.TextColor = textColor.Value;
						goto IL_65D;
					}
					goto IL_65D;
				}
				}
				GameMain.Client.AddChatMessage(txt, type, senderName, senderClient, sender, changeType, textColor);
				WifiComponent radio;
				if (type == ChatMessageType.Radio && ChatMessage.CanUseRadio(senderCharacter, out radio, false))
				{
					Signal s = new Signal(txt, 0, senderCharacter, radio.Item, 0f, 1f);
					radio.TransmitSignal(s, true);
				}
				IL_65D:
				ChatMessage.LastID = id;
			}
		}

		// Token: 0x1700130F RID: 4879
		// (get) Token: 0x06004A97 RID: 19095 RVA: 0x0029084C File Offset: 0x0028EA4C
		public string TranslatedText
		{
			get
			{
				if (this.Type == ChatMessageType.Radio && this.Sender is Item)
				{
					if (this.translatedText.IsNullOrEmpty())
					{
						this.translatedText = TextManager.Get(this.Text).Fallback(this.Text, true).Value;
					}
					return this.translatedText;
				}
				if (this.Type.HasFlag(ChatMessageType.Server) || this.Type.HasFlag(ChatMessageType.Error) || this.Type.HasFlag(ChatMessageType.ServerLog))
				{
					if (this.translatedText.IsNullOrEmpty())
					{
						this.translatedText = TextManager.GetServerMessage(this.Text).Value;
					}
					return this.translatedText;
				}
				return this.Text;
			}
		}

		// Token: 0x17001310 RID: 4880
		// (get) Token: 0x06004A98 RID: 19096 RVA: 0x00290923 File Offset: 0x0028EB23
		public Character SenderCharacter
		{
			get
			{
				return this.Sender as Character;
			}
		}

		// Token: 0x17001311 RID: 4881
		// (get) Token: 0x06004A99 RID: 19097 RVA: 0x00290930 File Offset: 0x0028EB30
		// (set) Token: 0x06004A9A RID: 19098 RVA: 0x0029095B File Offset: 0x0028EB5B
		public Color Color
		{
			get
			{
				if (this.customTextColor == null)
				{
					return ChatMessage.MessageColor[(int)this.Type];
				}
				return this.customTextColor.Value;
			}
			set
			{
				this.customTextColor = new Color?(value);
			}
		}

		// Token: 0x06004A9B RID: 19099 RVA: 0x0029096C File Offset: 0x0028EB6C
		public static string GetTimeStamp()
		{
			return "[" + DateTime.Now.ToString(ChatMessage.dateTimeFormatLongTimePattern) + "] ";
		}

		// Token: 0x17001312 RID: 4882
		// (get) Token: 0x06004A9C RID: 19100 RVA: 0x0029099A File Offset: 0x0028EB9A
		public string TextWithSender
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(this.SenderName))
				{
					return NetworkMember.ClientLogName(this.SenderClient, this.SenderName) + ": " + this.TranslatedText;
				}
				return this.TranslatedText;
			}
		}

		// Token: 0x17001313 RID: 4883
		// (get) Token: 0x06004A9D RID: 19101 RVA: 0x002909D1 File Offset: 0x0028EBD1
		// (set) Token: 0x06004A9E RID: 19102 RVA: 0x002909D9 File Offset: 0x0028EBD9
		public ushort NetStateID { get; set; }

		// Token: 0x17001314 RID: 4884
		// (get) Token: 0x06004A9F RID: 19103 RVA: 0x002909E2 File Offset: 0x0028EBE2
		// (set) Token: 0x06004AA0 RID: 19104 RVA: 0x002909EA File Offset: 0x0028EBEA
		public ChatMode ChatMode { get; set; }

		// Token: 0x06004AA1 RID: 19105 RVA: 0x002909F3 File Offset: 0x0028EBF3
		protected ChatMessage(string senderName, string text, ChatMessageType type, Entity sender, Client client, PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None, Color? textColor = null)
		{
			this.Text = text;
			this.Type = type;
			this.Sender = sender;
			this.SenderClient = client;
			this.SenderName = senderName;
			this.ChangeType = changeType;
			this.customTextColor = textColor;
		}

		// Token: 0x06004AA2 RID: 19106 RVA: 0x00290A30 File Offset: 0x0028EC30
		public static ChatMessage Create(string senderName, string text, ChatMessageType type, Entity sender, Client client = null, PlayerConnectionChangeType changeType = PlayerConnectionChangeType.None, Color? textColor = null)
		{
			Entity sender2 = sender;
			Client client2 = client;
			if (client == null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null)
				{
					client2 = null;
				}
				else
				{
					IReadOnlyList<Client> connectedClients = networkMember.ConnectedClients;
					client2 = ((connectedClients != null) ? connectedClients.Find((Client c) => c.Character != null && c.Character == sender) : null);
				}
			}
			return new ChatMessage(senderName, text, type, sender2, client2, changeType, textColor);
		}

		// Token: 0x06004AA3 RID: 19107 RVA: 0x00290A8C File Offset: 0x0028EC8C
		public static string GetChatMessageCommand(string message, out string messageWithoutCommand)
		{
			messageWithoutCommand = message;
			int separatorIndex = message.IndexOf(";");
			if (separatorIndex == -1)
			{
				return "";
			}
			string command = "";
			try
			{
				command = message.Substring(0, separatorIndex);
				command = command.Trim();
			}
			catch
			{
				return command;
			}
			messageWithoutCommand = message.Substring(separatorIndex + 1, message.Length - separatorIndex - 1).TrimStart();
			return command;
		}

		// Token: 0x06004AA4 RID: 19108 RVA: 0x00290AFC File Offset: 0x0028ECFC
		public static float GetGarbleAmount(Entity listener, Entity sender, float range, float obstructionMultiplier = 2f)
		{
			if (listener == null || sender == null)
			{
				return 0f;
			}
			if (listener.WorldPosition == sender.WorldPosition)
			{
				return 0f;
			}
			float dist = Vector2.Distance(listener.WorldPosition, sender.WorldPosition);
			if (dist > range)
			{
				return 1f;
			}
			Hull listenerHull = (listener == null) ? null : Hull.FindHull(listener.WorldPosition, null, true, true);
			Hull sourceHull = (sender == null) ? null : Hull.FindHull(sender.WorldPosition, null, true, true);
			if (sourceHull != listenerHull && obstructionMultiplier >= 1f && (sourceHull == null || !sourceHull.GetConnectedHulls(false, new int?(2), true).Contains(listenerHull)) && Submarine.CheckVisibility(listener.SimPosition, sender.SimPosition, false, false, true, true, true, null) != null)
			{
				dist = (dist + 100f) * obstructionMultiplier;
			}
			if (dist > range)
			{
				return 1f;
			}
			return dist / range;
		}

		// Token: 0x06004AA5 RID: 19109 RVA: 0x00290BC9 File Offset: 0x0028EDC9
		public string ApplyDistanceEffect(Character listener)
		{
			if (this.Sender == null)
			{
				return this.Text;
			}
			return ChatMessage.ApplyDistanceEffect(listener, this.Sender, this.Text, 2000f, 2f);
		}

		// Token: 0x06004AA6 RID: 19110 RVA: 0x00290BF6 File Offset: 0x0028EDF6
		public static string ApplyDistanceEffect(Entity listener, Entity sender, string text, float range, float obstructionMultiplier = 2f)
		{
			return ChatMessage.ApplyDistanceEffect(text, ChatMessage.GetGarbleAmount(listener, sender, range, obstructionMultiplier));
		}

		// Token: 0x06004AA7 RID: 19111 RVA: 0x00290C08 File Offset: 0x0028EE08
		public static string ApplyDistanceEffect(string text, float garbleAmount)
		{
			if (garbleAmount < 0.3f)
			{
				return text;
			}
			if (garbleAmount >= 1f)
			{
				return "";
			}
			string textWithoutColorTags = RichString.Rich(text, null).SanitizedValue;
			int startIndex = Math.Max(textWithoutColorTags.IndexOf(':') + 1, 1);
			StringBuilder sb = new StringBuilder(text.Length);
			for (int i = 0; i < textWithoutColorTags.Length; i++)
			{
				sb.Append((i > startIndex && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < garbleAmount) ? '-' : textWithoutColorTags[i]);
			}
			return sb.ToString();
		}

		// Token: 0x06004AA8 RID: 19112 RVA: 0x00290C9C File Offset: 0x0028EE9C
		public static string ApplyDistanceEffect(string message, ChatMessageType type, Character sender, Character receiver)
		{
			if (sender == null)
			{
				return "";
			}
			float range = 2000f;
			if (type == ChatMessageType.Default && sender.SpeechImpediment > 0f)
			{
				range *= 1f - sender.SpeechImpediment / 100f;
			}
			string spokenMsg = ChatMessage.ApplyDistanceEffect(receiver, sender, message, range, 3f);
			if (type != ChatMessageType.Default)
			{
				if (type == ChatMessageType.Radio || type == ChatMessageType.Order)
				{
					if (((receiver != null) ? receiver.Inventory : null) != null && !receiver.IsDead)
					{
						foreach (Item receiverItem in receiver.Inventory.AllItems.Where(delegate(Item i)
						{
							WifiComponent component = i.GetComponent<WifiComponent>();
							return component != null && component.LinkToChat;
						}))
						{
							if (sender.Inventory != null && receiver.HasEquippedItem(receiverItem, null, null))
							{
								foreach (Item senderItem in sender.Inventory.AllItems.Where(delegate(Item i)
								{
									WifiComponent component = i.GetComponent<WifiComponent>();
									return component != null && component.LinkToChat;
								}))
								{
									if (sender.HasEquippedItem(senderItem, null, null))
									{
										WifiComponent receiverRadio = receiverItem.GetComponent<WifiComponent>();
										WifiComponent senderRadio = senderItem.GetComponent<WifiComponent>();
										if (receiverRadio.CanReceive(senderRadio))
										{
											string msg = ChatMessage.ApplyDistanceEffect(receiverItem, senderItem, message, senderRadio.Range, 0f);
											if (sender.SpeechImpediment > 0f)
											{
												msg = ChatMessage.ApplyDistanceEffect(msg, sender.SpeechImpediment / 100f);
											}
											return msg;
										}
									}
								}
							}
						}
						return spokenMsg;
					}
				}
			}
			else if (receiver != null && !receiver.IsDead)
			{
				return spokenMsg;
			}
			return message;
		}

		// Token: 0x06004AA9 RID: 19113 RVA: 0x00290EAC File Offset: 0x0028F0AC
		public int EstimateLengthBytesClient()
		{
			return 3 + Encoding.UTF8.GetBytes(this.Text).Length + 2;
		}

		// Token: 0x06004AAA RID: 19114 RVA: 0x00290ED4 File Offset: 0x0028F0D4
		public static bool CanUseRadio(Character sender, bool ignoreJamming = false)
		{
			WifiComponent wifiComponent;
			return ChatMessage.CanUseRadio(sender, out wifiComponent, ignoreJamming);
		}

		// Token: 0x06004AAB RID: 19115 RVA: 0x00290EEC File Offset: 0x0028F0EC
		public static bool CanUseRadio(Character sender, out WifiComponent radio, bool ignoreJamming = false)
		{
			radio = null;
			if (((sender != null) ? sender.Inventory : null) == null || sender.Removed)
			{
				return false;
			}
			foreach (Item item in sender.Inventory.AllItems)
			{
				WifiComponent wifiComponent = item.GetComponent<WifiComponent>();
				if (wifiComponent != null && wifiComponent.LinkToChat && wifiComponent.CanTransmit(ignoreJamming) && sender.HasEquippedItem(item, null, null) && (radio == null || wifiComponent.Range > radio.Range))
				{
					radio = wifiComponent;
				}
			}
			WifiComponent wifiComponent2 = radio;
			return ((wifiComponent2 != null) ? wifiComponent2.Item : null) != null;
		}

		// Token: 0x040026FF RID: 9983
		public const int MaxLength = 200;

		// Token: 0x04002700 RID: 9984
		public const int MaxMessagesPerPacket = 10;

		// Token: 0x04002701 RID: 9985
		public const float SpeakRange = 2000f;

		// Token: 0x04002702 RID: 9986
		public const float SpeakRangeVOIP = 1000f;

		// Token: 0x04002703 RID: 9987
		public const float BlockedBySpamFilterTime = 10f;

		// Token: 0x04002704 RID: 9988
		private static readonly string dateTimeFormatLongTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		// Token: 0x04002705 RID: 9989
		public static Color[] MessageColor = new Color[]
		{
			new Color(190, 198, 205),
			new Color(204, 74, 78),
			new Color(136, 177, 255),
			new Color(157, 225, 160),
			new Color(238, 208, 0),
			new Color(64, 240, 89),
			new Color(255, 255, 255),
			new Color(255, 255, 255),
			new Color(255, 128, 0),
			default(Color),
			default(Color),
			default(Color),
			new Color(86, 91, 205),
			new Color(255, 0, 0)
		};

		// Token: 0x04002706 RID: 9990
		public string Text;

		// Token: 0x04002707 RID: 9991
		private string translatedText;

		// Token: 0x04002708 RID: 9992
		public ChatMessageType Type;

		// Token: 0x04002709 RID: 9993
		public PlayerConnectionChangeType ChangeType;

		// Token: 0x0400270A RID: 9994
		public string IconStyle;

		// Token: 0x0400270B RID: 9995
		public readonly Entity Sender;

		// Token: 0x0400270C RID: 9996
		public readonly Client SenderClient;

		// Token: 0x0400270D RID: 9997
		public readonly string SenderName;

		// Token: 0x0400270E RID: 9998
		private Color? customTextColor;

		// Token: 0x0400270F RID: 9999
		public static ushort LastID = 0;
	}
}
