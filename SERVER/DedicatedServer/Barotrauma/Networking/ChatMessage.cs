using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000367 RID: 871
	internal class ChatMessage
	{
		// Token: 0x06003368 RID: 13160 RVA: 0x0015B226 File Offset: 0x00159426
		private static string SanitizeText(Client client, string text)
		{
			if (!client.HasPermission(ClientPermissions.SpamImmunity))
			{
				text = text.Replace('‖', ' ');
			}
			return text;
		}

		// Token: 0x06003369 RID: 13161 RVA: 0x0015B248 File Offset: 0x00159448
		public static void ServerRead(IReadMessage msg, Client c)
		{
			c.KickAFKTimer = 0f;
			ushort ID = msg.ReadUInt16();
			ChatMessageType type = (ChatMessageType)msg.ReadRangedInteger(0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			ChatMode chatMode = (ChatMode)msg.ReadRangedInteger(0, Enum.GetValues(typeof(ChatMode)).Length - 1);
			Character orderTargetCharacter = null;
			Entity orderTargetEntity = null;
			OrderChatMessage orderMsg = null;
			Order.OrderTargetType orderTargetType = Order.OrderTargetType.Entity;
			int? wallSectionIndex = null;
			Order order = null;
			bool isNewOrder = false;
			string txt;
			if (type == ChatMessageType.Order)
			{
				OrderChatMessage.OrderMessageInfo orderMessageInfo = OrderChatMessage.ReadOrder(msg);
				Identifier identifier = orderMessageInfo.OrderIdentifier;
				if (identifier == Identifier.Empty)
				{
					DebugConsole.ThrowError("Invalid order message from client \"" + c.Name + "\" - order identifier is empty.", null, null, false, false);
					if (NetIdUtils.IdMoreRecent(ID, c.LastSentChatMsgID))
					{
						c.LastSentChatMsgID = ID;
					}
					return;
				}
				isNewOrder = orderMessageInfo.IsNewOrder;
				orderTargetCharacter = orderMessageInfo.TargetCharacter;
				orderTargetEntity = orderMessageInfo.TargetEntity;
				OrderTarget orderTargetPosition = orderMessageInfo.TargetPosition;
				orderTargetType = orderMessageInfo.TargetType;
				wallSectionIndex = orderMessageInfo.WallSectionIndex;
				OrderPrefab orderPrefab = orderMessageInfo.OrderPrefab ?? OrderPrefab.Prefabs[orderMessageInfo.OrderIdentifier];
				Identifier orderOption = orderMessageInfo.OrderOption;
				if (orderOption.IsEmpty)
				{
					Identifier identifier2;
					if (orderMessageInfo.OrderOptionIndex != null)
					{
						int? orderOptionIndex = orderMessageInfo.OrderOptionIndex;
						int num = 0;
						if (!(orderOptionIndex.GetValueOrDefault() < num & orderOptionIndex != null))
						{
							orderOptionIndex = orderMessageInfo.OrderOptionIndex;
							num = orderPrefab.Options.Length;
							if (!(orderOptionIndex.GetValueOrDefault() >= num & orderOptionIndex != null))
							{
								identifier2 = orderPrefab.Options[orderMessageInfo.OrderOptionIndex.Value];
								goto IL_1D9;
							}
						}
					}
					identifier2 = Identifier.Empty;
					IL_1D9:
					orderOption = identifier2;
				}
				if (orderTargetType == Order.OrderTargetType.Position)
				{
					order = new Order(orderPrefab, orderOption, orderTargetPosition, c.Character).WithManualPriority(orderMessageInfo.Priority);
				}
				else if (orderTargetType == Order.OrderTargetType.WallSection)
				{
					order = new Order(orderPrefab, orderOption, orderTargetEntity as Structure, wallSectionIndex, c.Character).WithManualPriority(orderMessageInfo.Priority);
				}
				else
				{
					order = new Order(orderPrefab, orderOption, orderTargetEntity, orderPrefab.GetTargetItemComponent(orderTargetEntity as Item), c.Character, false).WithManualPriority(orderMessageInfo.Priority);
				}
				orderMsg = new OrderChatMessage(order, orderTargetCharacter, c.Character, true);
				txt = orderMsg.Text;
			}
			else
			{
				txt = (msg.ReadString() ?? "");
			}
			txt = ChatMessage.SanitizeText(c, txt);
			if (!NetIdUtils.IdMoreRecent(ID, c.LastSentChatMsgID))
			{
				return;
			}
			c.LastSentChatMsgID = ID;
			if (txt.Length > 200)
			{
				txt = txt.Substring(0, 200);
			}
			c.LastSentChatMessages.Add(txt);
			if (c.LastSentChatMessages.Count > 10)
			{
				c.LastSentChatMessages.RemoveRange(0, c.LastSentChatMessages.Count - 10);
			}
			float similarityMultiplier = (orderMsg != null) ? 0.25f : 1f;
			bool flaggedAsSpam;
			ChatMessage.HandleSpamFilter(c, txt, out flaggedAsSpam, similarityMultiplier);
			if (flaggedAsSpam)
			{
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventChatMessage>(delegate(IEventChatMessage x)
			{
				bool? flag = x.OnChatMessage(txt, c, type, ChatMessage.Create(c.Name, txt, type, null, c, PlayerConnectionChangeType.None, null));
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			if (type != ChatMessageType.Order)
			{
				GameServer server = GameMain.Server;
				string txt2 = txt;
				Client c2 = c;
				ChatMode chatMode2 = chatMode;
				server.SendChatMessage(txt2, (type == ChatMessageType.Team) ? new ChatMessageType?(type) : null, c2, null, PlayerConnectionChangeType.None, chatMode2);
				return;
			}
			if (c.Character == null || c.Character.SpeechImpediment >= 100f || c.Character.IsDead)
			{
				return;
			}
			if (orderMsg.Order.IsReport)
			{
				HumanAIController.ReportProblem(orderMsg.Sender as Character, orderMsg.Order, null);
			}
			if (order != null)
			{
				if (order.TargetAllCharacters)
				{
					if (order.IsIgnoreOrder)
					{
						switch (orderTargetType)
						{
						case Order.OrderTargetType.Entity:
						{
							IIgnorable ignorableEntity = orderTargetEntity as IIgnorable;
							if (ignorableEntity != null)
							{
								IIgnorable ignorable = ignorableEntity;
								Identifier identifier = order.Identifier;
								ignorable.OrderedToBeIgnored = (identifier == "ignorethis");
							}
							break;
						}
						case Order.OrderTargetType.Position:
							throw new NotImplementedException();
						case Order.OrderTargetType.WallSection:
							if (wallSectionIndex != null)
							{
								Structure s = orderTargetEntity as Structure;
								if (s != null)
								{
									IIgnorable ignorableWall = s.GetSection(wallSectionIndex.Value);
									if (ignorableWall != null)
									{
										IIgnorable ignorable2 = ignorableWall;
										Identifier identifier = order.Identifier;
										ignorable2.OrderedToBeIgnored = (identifier == "ignorethis");
									}
								}
							}
							break;
						}
					}
					GameSession gameSession = GameMain.GameSession;
					if (gameSession != null)
					{
						CrewManager crewManager = gameSession.CrewManager;
						if (crewManager != null)
						{
							crewManager.AddOrder(order, order.IsIgnoreOrder ? null : new float?(order.FadeOutTime));
						}
					}
				}
				else if (orderTargetCharacter != null)
				{
					orderTargetCharacter.SetOrder(order, isNewOrder, true, false);
				}
			}
			GameMain.Server.SendOrderChatMessage(orderMsg);
		}

		// Token: 0x0600336A RID: 13162 RVA: 0x0015B7CC File Offset: 0x001599CC
		public static void HandleSpamFilter(Client c, string messageText, out bool flaggedAsSpam, float similarityMultiplier = 1f)
		{
			ChatMessage.<>c__DisplayClass2_0 CS$<>8__locals1;
			CS$<>8__locals1.c = c;
			float similarity = 0f;
			for (int i = 0; i < CS$<>8__locals1.c.LastSentChatMessages.Count; i++)
			{
				float closeFactor = 1f / (float)(CS$<>8__locals1.c.LastSentChatMessages.Count - i);
				if (string.IsNullOrEmpty(messageText))
				{
					similarity += closeFactor;
				}
				else
				{
					int levenshteinDist = ToolBox.LevenshteinDistance(messageText, CS$<>8__locals1.c.LastSentChatMessages[i]);
					similarity += Math.Max((float)(messageText.Length - levenshteinDist) / (float)messageText.Length * closeFactor, 0f);
				}
			}
			similarity *= similarityMultiplier;
			bool isSpamExempt = RateLimiter.IsExempt(CS$<>8__locals1.c);
			if (similarity + CS$<>8__locals1.c.ChatSpamSpeed > 5f && !isSpamExempt)
			{
				GameMain.Server.KarmaManager.OnSpamFilterTriggered(CS$<>8__locals1.c);
				CS$<>8__locals1.c.ChatSpamCount++;
				if (CS$<>8__locals1.c.ChatSpamCount > 3)
				{
					GameMain.Server.KickClient(CS$<>8__locals1.c, TextManager.Get("SpamFilterKicked").Value, false);
				}
				else
				{
					ChatMessage.<HandleSpamFilter>g__BlockBySpamFilter|2_0(ref CS$<>8__locals1);
				}
				flaggedAsSpam = true;
				return;
			}
			CS$<>8__locals1.c.ChatSpamSpeed += similarity + 0.5f;
			if (CS$<>8__locals1.c.ChatSpamTimer > 0f && !isSpamExempt)
			{
				ChatMessage.<HandleSpamFilter>g__BlockBySpamFilter|2_0(ref CS$<>8__locals1);
				flaggedAsSpam = true;
				return;
			}
			flaggedAsSpam = false;
		}

		// Token: 0x0600336B RID: 13163 RVA: 0x0015B930 File Offset: 0x00159B30
		public int EstimateLengthBytesServer(Client c)
		{
			int length = 4 + ((this.Text == null) ? 0 : Encoding.UTF8.GetBytes(this.Text).Length) + 2;
			if (this.SenderClient != null)
			{
				length += 8;
			}
			if (this.Sender != null && c.InGame)
			{
				length += 2;
			}
			if (this.SenderName != null)
			{
				length += Encoding.UTF8.GetBytes(this.SenderName).Length + 2;
			}
			return length;
		}

		// Token: 0x0600336C RID: 13164 RVA: 0x0015B9A0 File Offset: 0x00159BA0
		public virtual void ServerWrite(in SegmentTableWriter<ServerNetSegment> segmentTable, IWriteMessage msg, Client c)
		{
			segmentTable.StartNewSegment(ServerNetSegment.ChatMessage);
			msg.WriteUInt16(this.NetStateID);
			msg.WriteRangedInteger((int)this.Type, 0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			msg.WriteByte((byte)this.ChangeType);
			msg.WriteString(this.Text);
			msg.WriteString(this.SenderName);
			msg.WriteBoolean(this.SenderClient != null);
			if (this.SenderClient != null)
			{
				AccountId accountId;
				msg.WriteString(this.SenderClient.AccountId.TryUnwrap(out accountId) ? accountId.StringRepresentation : this.SenderClient.SessionId.ToString());
			}
			msg.WriteBoolean(this.Sender != null && c.InGame);
			if (this.Sender != null && c.InGame)
			{
				msg.WriteUInt16(this.Sender.ID);
			}
			msg.WriteBoolean(this.customTextColor != null);
			if (this.customTextColor != null)
			{
				msg.WriteColorR8G8B8A8(this.customTextColor.Value);
			}
			msg.WritePadBits();
			if (this.Type == ChatMessageType.ServerMessageBoxInGame)
			{
				msg.WriteString(this.IconStyle);
			}
		}

		// Token: 0x17000E51 RID: 3665
		// (get) Token: 0x0600336D RID: 13165 RVA: 0x0015BAD8 File Offset: 0x00159CD8
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

		// Token: 0x17000E52 RID: 3666
		// (get) Token: 0x0600336E RID: 13166 RVA: 0x0015BBAF File Offset: 0x00159DAF
		public Character SenderCharacter
		{
			get
			{
				return this.Sender as Character;
			}
		}

		// Token: 0x17000E53 RID: 3667
		// (get) Token: 0x0600336F RID: 13167 RVA: 0x0015BBBC File Offset: 0x00159DBC
		// (set) Token: 0x06003370 RID: 13168 RVA: 0x0015BBE7 File Offset: 0x00159DE7
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

		// Token: 0x06003371 RID: 13169 RVA: 0x0015BBF8 File Offset: 0x00159DF8
		public static string GetTimeStamp()
		{
			return "[" + DateTime.Now.ToString(ChatMessage.dateTimeFormatLongTimePattern) + "] ";
		}

		// Token: 0x17000E54 RID: 3668
		// (get) Token: 0x06003372 RID: 13170 RVA: 0x0015BC26 File Offset: 0x00159E26
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

		// Token: 0x17000E55 RID: 3669
		// (get) Token: 0x06003373 RID: 13171 RVA: 0x0015BC5D File Offset: 0x00159E5D
		// (set) Token: 0x06003374 RID: 13172 RVA: 0x0015BC65 File Offset: 0x00159E65
		public ushort NetStateID { get; set; }

		// Token: 0x17000E56 RID: 3670
		// (get) Token: 0x06003375 RID: 13173 RVA: 0x0015BC6E File Offset: 0x00159E6E
		// (set) Token: 0x06003376 RID: 13174 RVA: 0x0015BC76 File Offset: 0x00159E76
		public ChatMode ChatMode { get; set; }

		// Token: 0x06003377 RID: 13175 RVA: 0x0015BC7F File Offset: 0x00159E7F
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

		// Token: 0x06003378 RID: 13176 RVA: 0x0015BCBC File Offset: 0x00159EBC
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

		// Token: 0x06003379 RID: 13177 RVA: 0x0015BD18 File Offset: 0x00159F18
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

		// Token: 0x0600337A RID: 13178 RVA: 0x0015BD88 File Offset: 0x00159F88
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

		// Token: 0x0600337B RID: 13179 RVA: 0x0015BE55 File Offset: 0x0015A055
		public string ApplyDistanceEffect(Character listener)
		{
			if (this.Sender == null)
			{
				return this.Text;
			}
			return ChatMessage.ApplyDistanceEffect(listener, this.Sender, this.Text, 2000f, 2f);
		}

		// Token: 0x0600337C RID: 13180 RVA: 0x0015BE82 File Offset: 0x0015A082
		public static string ApplyDistanceEffect(Entity listener, Entity sender, string text, float range, float obstructionMultiplier = 2f)
		{
			return ChatMessage.ApplyDistanceEffect(text, ChatMessage.GetGarbleAmount(listener, sender, range, obstructionMultiplier));
		}

		// Token: 0x0600337D RID: 13181 RVA: 0x0015BE94 File Offset: 0x0015A094
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

		// Token: 0x0600337E RID: 13182 RVA: 0x0015BF28 File Offset: 0x0015A128
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

		// Token: 0x0600337F RID: 13183 RVA: 0x0015C138 File Offset: 0x0015A338
		public int EstimateLengthBytesClient()
		{
			return 3 + Encoding.UTF8.GetBytes(this.Text).Length + 2;
		}

		// Token: 0x06003380 RID: 13184 RVA: 0x0015C160 File Offset: 0x0015A360
		public static bool CanUseRadio(Character sender, bool ignoreJamming = false)
		{
			WifiComponent wifiComponent;
			return ChatMessage.CanUseRadio(sender, out wifiComponent, ignoreJamming);
		}

		// Token: 0x06003381 RID: 13185 RVA: 0x0015C178 File Offset: 0x0015A378
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

		// Token: 0x06003383 RID: 13187 RVA: 0x0015C370 File Offset: 0x0015A570
		[CompilerGenerated]
		internal static void <HandleSpamFilter>g__BlockBySpamFilter|2_0(ref ChatMessage.<>c__DisplayClass2_0 A_0)
		{
			ChatMessage denyMsg = ChatMessage.Create("", TextManager.Get("SpamFilterBlocked").Value, ChatMessageType.BlockedBySpamFilter, null, null, PlayerConnectionChangeType.None, null);
			A_0.c.ChatSpamTimer = 10f;
			GameMain.Server.SendDirectChatMessage(denyMsg, A_0.c);
			GameServer.Log(A_0.c.Name + " blocked by spam filter", ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x04001976 RID: 6518
		public const int MaxLength = 200;

		// Token: 0x04001977 RID: 6519
		public const int MaxMessagesPerPacket = 10;

		// Token: 0x04001978 RID: 6520
		public const float SpeakRange = 2000f;

		// Token: 0x04001979 RID: 6521
		public const float SpeakRangeVOIP = 1000f;

		// Token: 0x0400197A RID: 6522
		public const float BlockedBySpamFilterTime = 10f;

		// Token: 0x0400197B RID: 6523
		private static readonly string dateTimeFormatLongTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		// Token: 0x0400197C RID: 6524
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

		// Token: 0x0400197D RID: 6525
		public string Text;

		// Token: 0x0400197E RID: 6526
		private string translatedText;

		// Token: 0x0400197F RID: 6527
		public ChatMessageType Type;

		// Token: 0x04001980 RID: 6528
		public PlayerConnectionChangeType ChangeType;

		// Token: 0x04001981 RID: 6529
		public string IconStyle;

		// Token: 0x04001982 RID: 6530
		public readonly Entity Sender;

		// Token: 0x04001983 RID: 6531
		public readonly Client SenderClient;

		// Token: 0x04001984 RID: 6532
		public readonly string SenderName;

		// Token: 0x04001985 RID: 6533
		private Color? customTextColor;

		// Token: 0x04001986 RID: 6534
		public static ushort LastID = 0;
	}
}
