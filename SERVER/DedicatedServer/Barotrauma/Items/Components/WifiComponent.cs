using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AE RID: 1198
	internal class WifiComponent : ItemComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004394 RID: 17300 RVA: 0x001B1B5E File Offset: 0x001AFD5E
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			this.SharedEventWrite(msg);
		}

		// Token: 0x06004395 RID: 17301 RVA: 0x001B1B68 File Offset: 0x001AFD68
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			int newChannel = msg.ReadRangedInteger(0, 10000);
			for (int i = 0; i < 10; i++)
			{
				this.networkReceivedChannelMemory[i] = msg.ReadRangedInteger(0, 10000);
			}
			if (this.item.CanClientAccess(c))
			{
				this.Channel = newChannel;
				for (int j = 0; j < 10; j++)
				{
					this.channelMemory[j] = this.networkReceivedChannelMemory[j];
				}
			}
			this.item.CreateServerEvent<WifiComponent>(this);
		}

		// Token: 0x170011FE RID: 4606
		// (get) Token: 0x06004396 RID: 17302 RVA: 0x001B1BE1 File Offset: 0x001AFDE1
		// (set) Token: 0x06004397 RID: 17303 RVA: 0x001B1BE9 File Offset: 0x001AFDE9
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "WiFi components can only communicate with components that have the same Team ID.", "", true)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x170011FF RID: 4607
		// (get) Token: 0x06004398 RID: 17304 RVA: 0x001B1BF2 File Offset: 0x001AFDF2
		// (set) Token: 0x06004399 RID: 17305 RVA: 0x001B1BFA File Offset: 0x001AFDFA
		[Editable]
		[Serialize(20000f, IsPropertySaveable.No, "How close the recipient has to be to receive a signal from this WiFi component.", "", true)]
		public float Range
		{
			get
			{
				return this.range;
			}
			set
			{
				this.range = Math.Max(value, 0f);
			}
		}

		// Token: 0x17001200 RID: 4608
		// (get) Token: 0x0600439A RID: 17306 RVA: 0x001B1C0D File Offset: 0x001AFE0D
		// (set) Token: 0x0600439B RID: 17307 RVA: 0x001B1C15 File Offset: 0x001AFE15
		[InGameEditable]
		[Serialize(0, IsPropertySaveable.Yes, "WiFi components can only communicate with components that use the same channel.", "", true)]
		public int Channel
		{
			get
			{
				return this.channel;
			}
			set
			{
				this.channel = MathHelper.Clamp(value, 0, 10000);
			}
		}

		// Token: 0x17001201 RID: 4609
		// (get) Token: 0x0600439C RID: 17308 RVA: 0x001B1C29 File Offset: 0x001AFE29
		// (set) Token: 0x0600439D RID: 17309 RVA: 0x001B1C31 File Offset: 0x001AFE31
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Can the component communicate with wifi components in another team's submarine (e.g. enemy sub in Combat missions, respawn shuttle). Needs to be enabled on both the component transmitting the signal and the component receiving it.", "", true)]
		public bool AllowCrossTeamCommunication { get; set; }

		// Token: 0x17001202 RID: 4610
		// (get) Token: 0x0600439E RID: 17310 RVA: 0x001B1C3A File Offset: 0x001AFE3A
		// (set) Token: 0x0600439F RID: 17311 RVA: 0x001B1C42 File Offset: 0x001AFE42
		[ConditionallyEditable(ConditionallyEditable.ConditionType.AllowLinkingWifiToChat, false)]
		[Serialize(false, IsPropertySaveable.No, "If enabled, any signals received from another chat-linked wifi component are displayed as chat messages in the chatbox of the player holding the item.", "", true)]
		public bool LinkToChat { get; set; }

		// Token: 0x17001203 RID: 4611
		// (get) Token: 0x060043A0 RID: 17312 RVA: 0x001B1C4B File Offset: 0x001AFE4B
		// (set) Token: 0x060043A1 RID: 17313 RVA: 0x001B1C53 File Offset: 0x001AFE53
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "How many seconds have to pass between signals for a message to be displayed in the chatbox. Setting this to a very low value is not recommended, because it may cause an excessive amount of chat messages to be created if there are chat-linked wifi components that transmit a continuous signal.", "", false)]
		public float MinChatMessageInterval { get; set; }

		// Token: 0x17001204 RID: 4612
		// (get) Token: 0x060043A2 RID: 17314 RVA: 0x001B1C5C File Offset: 0x001AFE5C
		// (set) Token: 0x060043A3 RID: 17315 RVA: 0x001B1C64 File Offset: 0x001AFE64
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the component will only create chat messages when the received signal changes.", "", false)]
		public bool DiscardDuplicateChatMessages { get; set; }

		// Token: 0x17001205 RID: 4613
		// (get) Token: 0x060043A4 RID: 17316 RVA: 0x001B1C6D File Offset: 0x001AFE6D
		// (set) Token: 0x060043A5 RID: 17317 RVA: 0x001B1C75 File Offset: 0x001AFE75
		public float JamTimer
		{
			get
			{
				return this.jamTimer;
			}
			set
			{
				if (value > 0f)
				{
					this.IsActive = true;
				}
				this.jamTimer = Math.Max(0f, value);
			}
		}

		// Token: 0x060043A6 RID: 17318 RVA: 0x001B1C97 File Offset: 0x001AFE97
		public WifiComponent(Item item, ContentXElement element) : base(item, element)
		{
			WifiComponent.list.Add(this);
			this.IsActive = true;
		}

		// Token: 0x060043A7 RID: 17319 RVA: 0x001B1CD0 File Offset: 0x001AFED0
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.channelMemory = componentElement.GetAttributeIntArray("channelmemory", new int[10]);
			if (this.channelMemory.Length != 10)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error when loading item ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(": the size of the channel memory doesn't match the default value of ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(10);
				defaultInterpolatedStringHandler.AppendLiteral(". Resizing...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				Array.Resize<int>(ref this.channelMemory, 10);
			}
		}

		// Token: 0x060043A8 RID: 17320 RVA: 0x001B1D74 File Offset: 0x001AFF74
		public override void OnItemLoaded()
		{
			if (this.item.Connections != null)
			{
				this.signalOutConnection = this.item.Connections.Find((Connection c) => c.Name == "signal_out");
				this.signalInConnection = this.item.Connections.Find((Connection c) => c.Name == "signal_in");
			}
			if (this.channelMemory.All((int m) => m == 0))
			{
				for (int i = 0; i < this.channelMemory.Length; i++)
				{
					this.channelMemory[i] = i;
				}
			}
		}

		// Token: 0x060043A9 RID: 17321 RVA: 0x001B1E40 File Offset: 0x001B0040
		public bool CanTransmit(bool ignoreJamming = false)
		{
			return (ignoreJamming || this.jamTimer <= 0f) && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x060043AA RID: 17322 RVA: 0x001B1E5D File Offset: 0x001B005D
		public IEnumerable<WifiComponent> GetReceiversInRange()
		{
			return from w in WifiComponent.list
			where w != this && w.CanReceive(this)
			select w;
		}

		// Token: 0x060043AB RID: 17323 RVA: 0x001B1E78 File Offset: 0x001B0078
		public bool CanReceive(WifiComponent sender)
		{
			return sender != null && sender.channel == this.channel && (sender.TeamID == this.TeamID || this.AllowCrossTeamCommunication) && this.jamTimer <= 0f && (this.LinkToChat || (this.signalOutConnection != null && this.signalOutConnection.IsConnectedToSomething())) && Vector2.DistanceSquared(this.item.WorldPosition, sender.item.WorldPosition) <= sender.range * sender.range && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x060043AC RID: 17324 RVA: 0x001B1F13 File Offset: 0x001B0113
		public IEnumerable<WifiComponent> GetTransmittersInRange()
		{
			return from w in WifiComponent.list
			where w != this && w.CanTransmit(this)
			select w;
		}

		// Token: 0x060043AD RID: 17325 RVA: 0x001B1F2C File Offset: 0x001B012C
		public bool CanTransmit(WifiComponent sender)
		{
			return sender != null && sender.channel == this.channel && (sender.TeamID == this.TeamID || this.AllowCrossTeamCommunication) && Vector2.DistanceSquared(this.item.WorldPosition, sender.item.WorldPosition) <= sender.range * sender.range && this.jamTimer <= 0f && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x060043AE RID: 17326 RVA: 0x001B1FA8 File Offset: 0x001B01A8
		public override void Update(float deltaTime, Camera cam)
		{
			this.chatMsgCooldown -= deltaTime;
			this.JamTimer -= deltaTime;
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			if (this.chatMsgCooldown <= 0f && this.JamTimer <= 0f)
			{
				this.IsActive = false;
			}
		}

		// Token: 0x060043AF RID: 17327 RVA: 0x001B200C File Offset: 0x001B020C
		public int GetChannelMemory(int index)
		{
			if (index < 0 || index >= 10)
			{
				return 0;
			}
			return this.channelMemory[index];
		}

		// Token: 0x060043B0 RID: 17328 RVA: 0x001B2021 File Offset: 0x001B0221
		public void SetChannelMemory(int index, int value)
		{
			if (index < 0 || index >= 10)
			{
				return;
			}
			this.channelMemory[index] = MathHelper.Clamp(value, 0, 10000);
		}

		// Token: 0x060043B1 RID: 17329 RVA: 0x001B2044 File Offset: 0x001B0244
		public void TransmitSignal(Signal signal, bool sentFromChat)
		{
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventWifiSignalTransmitted>(delegate(IEventWifiSignalTransmitted x)
			{
				bool? flag = x.OnWifiSignalTransmitted(this, signal, sentFromChat);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			bool chatMsgSent = false;
			IEnumerable<WifiComponent> receivers = this.GetReceiversInRange();
			if (sentFromChat)
			{
				this.item.LastSentSignalRecipients.Clear();
				foreach (WifiComponent receiver in receivers)
				{
					receiver.item.LastSentSignalRecipients.Clear();
				}
			}
			using (IEnumerator<WifiComponent> enumerator2 = receivers.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					WifiComponent wifiComp = enumerator2.Current;
					if (!sentFromChat || wifiComp.LinkToChat)
					{
						float sentSignalStrength = signal.strength * MathHelper.Clamp(1f - Vector2.Distance(this.item.WorldPosition, wifiComp.item.WorldPosition) / wifiComp.range, 0f, 1f);
						Signal s = new Signal(signal.value, signal.stepsTaken + 1, signal.sender, signal.source, 0f, sentSignalStrength);
						if (wifiComp.signalOutConnection != null)
						{
							if (signal.source != null && wifiComp.signalInConnection != null)
							{
								if (signal.source.LastSentSignalRecipients.Contains(wifiComp.signalInConnection))
								{
									continue;
								}
								signal.source.LastSentSignalRecipients.Add(wifiComp.signalInConnection);
							}
							wifiComp.item.SendSignal(s, wifiComp.signalOutConnection);
						}
						if (signal.source != null)
						{
							foreach (Connection receiver2 in wifiComp.item.LastSentSignalRecipients)
							{
								if (!signal.source.LastSentSignalRecipients.Contains(receiver2))
								{
									signal.source.LastSentSignalRecipients.Add(receiver2);
								}
							}
						}
						if ((!this.DiscardDuplicateChatMessages || !(signal.value == this.prevSignal)) && this.LinkToChat && wifiComp.LinkToChat && this.chatMsgCooldown <= 0f && !sentFromChat && wifiComp.item.ParentInventory != null && wifiComp.item.ParentInventory.Owner != null)
						{
							string chatMsg = signal.value;
							if (sentSignalStrength <= 1f)
							{
								chatMsg = ChatMessage.ApplyDistanceEffect(chatMsg, 1f - sentSignalStrength);
							}
							if (chatMsg.Length > 200)
							{
								chatMsg = chatMsg.Substring(0, 200);
							}
							if (!string.IsNullOrEmpty(chatMsg))
							{
								if (GameMain.Server != null)
								{
									Client recipientClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == wifiComp.item.ParentInventory.Owner);
									if (recipientClient != null)
									{
										GameServer server = GameMain.Server;
										Item source = signal.source;
										server.SendDirectChatMessage(ChatMessage.Create(((source != null) ? source.Name : null) ?? "", chatMsg, ChatMessageType.Radio, this.item, null, PlayerConnectionChangeType.None, null), recipientClient);
									}
								}
								chatMsgSent = true;
							}
						}
					}
				}
			}
			if (chatMsgSent)
			{
				this.chatMsgCooldown = this.MinChatMessageInterval;
				this.IsActive = true;
			}
			this.prevSignal = signal.value;
		}

		// Token: 0x060043B2 RID: 17330 RVA: 0x001B24B0 File Offset: 0x001B06B0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection == null)
			{
				return;
			}
			string name = connection.Name;
			if (!(name == "signal_in"))
			{
				int newChannel;
				if (!(name == "set_channel"))
				{
					if (!(name == "set_range"))
					{
						return;
					}
					float newRange;
					if (float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newRange))
					{
						this.Range = newRange;
					}
				}
				else if (int.TryParse(signal.value, out newChannel))
				{
					int prevChannel = this.Channel;
					this.Channel = newChannel;
					if (prevChannel != this.Channel)
					{
						this.item.CreateServerEvent<WifiComponent>(this);
						return;
					}
				}
				return;
			}
			this.TransmitSignal(signal, false);
		}

		// Token: 0x060043B3 RID: 17331 RVA: 0x001B254C File Offset: 0x001B074C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			WifiComponent.list.Remove(this);
		}

		// Token: 0x060043B4 RID: 17332 RVA: 0x001B2560 File Offset: 0x001B0760
		public override XElement Save(XElement parentElement)
		{
			XElement element = base.Save(parentElement);
			element.Add(new XAttribute("channelmemory", string.Join<int>(',', this.channelMemory)));
			return element;
		}

		// Token: 0x060043B5 RID: 17333 RVA: 0x001B2598 File Offset: 0x001B0798
		protected void SharedEventWrite(IWriteMessage msg)
		{
			msg.WriteRangedInteger(this.Channel, 0, 10000);
			for (int i = 0; i < 10; i++)
			{
				msg.WriteRangedInteger(this.channelMemory[i], 0, 10000);
			}
		}

		// Token: 0x060043B6 RID: 17334 RVA: 0x001B25D8 File Offset: 0x001B07D8
		protected void SharedEventRead(IReadMessage msg)
		{
			this.Channel = msg.ReadRangedInteger(0, 10000);
			for (int i = 0; i < 10; i++)
			{
				this.channelMemory[i] = msg.ReadRangedInteger(0, 10000);
			}
		}

		// Token: 0x0400204F RID: 8271
		private readonly int[] networkReceivedChannelMemory = new int[10];

		// Token: 0x04002050 RID: 8272
		private static readonly List<WifiComponent> list = new List<WifiComponent>();

		// Token: 0x04002051 RID: 8273
		private const int ChannelMemorySize = 10;

		// Token: 0x04002052 RID: 8274
		private const int MinChannel = 0;

		// Token: 0x04002053 RID: 8275
		private const int MaxChannel = 10000;

		// Token: 0x04002054 RID: 8276
		private float range;

		// Token: 0x04002055 RID: 8277
		private int channel;

		// Token: 0x04002056 RID: 8278
		private float chatMsgCooldown;

		// Token: 0x04002057 RID: 8279
		private string prevSignal;

		// Token: 0x04002058 RID: 8280
		private int[] channelMemory = new int[10];

		// Token: 0x04002059 RID: 8281
		private Connection signalInConnection;

		// Token: 0x0400205A RID: 8282
		private Connection signalOutConnection;

		// Token: 0x04002060 RID: 8288
		private float jamTimer;
	}
}
