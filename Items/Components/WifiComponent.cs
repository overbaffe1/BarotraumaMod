using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005DF RID: 1503
	internal class WifiComponent : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001893 RID: 6291
		// (get) Token: 0x0600615B RID: 24923 RVA: 0x0032A51B File Offset: 0x0032871B
		public Vector2 DrawSize
		{
			get
			{
				return new Vector2(this.range * 2f);
			}
		}

		// Token: 0x0600615C RID: 24924 RVA: 0x0032A530 File Offset: 0x00328730
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (!editing || !MapEntity.SelectedList.Contains(this.item))
			{
				return;
			}
			Vector2 pos = new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y);
			spriteBatch.DrawLine(pos + Vector2.UnitY * this.range, pos - Vector2.UnitY * this.range, Color.Cyan * 0.5f, 2f);
			spriteBatch.DrawLine(pos + Vector2.UnitX * this.range, pos - Vector2.UnitX * this.range, Color.Cyan * 0.5f, 2f);
			spriteBatch.DrawCircle(pos, this.range, 32, Color.Cyan * 0.5f, 3f);
		}

		// Token: 0x0600615D RID: 24925 RVA: 0x0032A62A File Offset: 0x0032882A
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			this.SharedEventWrite(msg);
		}

		// Token: 0x0600615E RID: 24926 RVA: 0x0032A633 File Offset: 0x00328833
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SharedEventRead(msg);
		}

		// Token: 0x17001894 RID: 6292
		// (get) Token: 0x0600615F RID: 24927 RVA: 0x0032A63C File Offset: 0x0032883C
		// (set) Token: 0x06006160 RID: 24928 RVA: 0x0032A644 File Offset: 0x00328844
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "WiFi components can only communicate with components that have the same Team ID.", "", true)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x17001895 RID: 6293
		// (get) Token: 0x06006161 RID: 24929 RVA: 0x0032A64D File Offset: 0x0032884D
		// (set) Token: 0x06006162 RID: 24930 RVA: 0x0032A655 File Offset: 0x00328855
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
				this.item.ResetCachedVisibleSize();
			}
		}

		// Token: 0x17001896 RID: 6294
		// (get) Token: 0x06006163 RID: 24931 RVA: 0x0032A673 File Offset: 0x00328873
		// (set) Token: 0x06006164 RID: 24932 RVA: 0x0032A67B File Offset: 0x0032887B
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

		// Token: 0x17001897 RID: 6295
		// (get) Token: 0x06006165 RID: 24933 RVA: 0x0032A68F File Offset: 0x0032888F
		// (set) Token: 0x06006166 RID: 24934 RVA: 0x0032A697 File Offset: 0x00328897
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Can the component communicate with wifi components in another team's submarine (e.g. enemy sub in Combat missions, respawn shuttle). Needs to be enabled on both the component transmitting the signal and the component receiving it.", "", true)]
		public bool AllowCrossTeamCommunication { get; set; }

		// Token: 0x17001898 RID: 6296
		// (get) Token: 0x06006167 RID: 24935 RVA: 0x0032A6A0 File Offset: 0x003288A0
		// (set) Token: 0x06006168 RID: 24936 RVA: 0x0032A6A8 File Offset: 0x003288A8
		[ConditionallyEditable(ConditionallyEditable.ConditionType.AllowLinkingWifiToChat, false)]
		[Serialize(false, IsPropertySaveable.No, "If enabled, any signals received from another chat-linked wifi component are displayed as chat messages in the chatbox of the player holding the item.", "", true)]
		public bool LinkToChat { get; set; }

		// Token: 0x17001899 RID: 6297
		// (get) Token: 0x06006169 RID: 24937 RVA: 0x0032A6B1 File Offset: 0x003288B1
		// (set) Token: 0x0600616A RID: 24938 RVA: 0x0032A6B9 File Offset: 0x003288B9
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "How many seconds have to pass between signals for a message to be displayed in the chatbox. Setting this to a very low value is not recommended, because it may cause an excessive amount of chat messages to be created if there are chat-linked wifi components that transmit a continuous signal.", "", false)]
		public float MinChatMessageInterval { get; set; }

		// Token: 0x1700189A RID: 6298
		// (get) Token: 0x0600616B RID: 24939 RVA: 0x0032A6C2 File Offset: 0x003288C2
		// (set) Token: 0x0600616C RID: 24940 RVA: 0x0032A6CA File Offset: 0x003288CA
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the component will only create chat messages when the received signal changes.", "", false)]
		public bool DiscardDuplicateChatMessages { get; set; }

		// Token: 0x1700189B RID: 6299
		// (get) Token: 0x0600616D RID: 24941 RVA: 0x0032A6D3 File Offset: 0x003288D3
		// (set) Token: 0x0600616E RID: 24942 RVA: 0x0032A6DB File Offset: 0x003288DB
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
					if (this.jamTimer <= 0f)
					{
						HintManager.OnRadioJammed(base.Item);
					}
					this.IsActive = true;
				}
				this.jamTimer = Math.Max(0f, value);
			}
		}

		// Token: 0x0600616F RID: 24943 RVA: 0x0032A715 File Offset: 0x00328915
		public WifiComponent(Item item, ContentXElement element) : base(item, element)
		{
			WifiComponent.list.Add(this);
			this.IsActive = true;
		}

		// Token: 0x06006170 RID: 24944 RVA: 0x0032A740 File Offset: 0x00328940
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

		// Token: 0x06006171 RID: 24945 RVA: 0x0032A7E4 File Offset: 0x003289E4
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

		// Token: 0x06006172 RID: 24946 RVA: 0x0032A8B0 File Offset: 0x00328AB0
		public bool CanTransmit(bool ignoreJamming = false)
		{
			return (ignoreJamming || this.jamTimer <= 0f) && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x06006173 RID: 24947 RVA: 0x0032A8CD File Offset: 0x00328ACD
		public IEnumerable<WifiComponent> GetReceiversInRange()
		{
			return from w in WifiComponent.list
			where w != this && w.CanReceive(this)
			select w;
		}

		// Token: 0x06006174 RID: 24948 RVA: 0x0032A8E8 File Offset: 0x00328AE8
		public bool CanReceive(WifiComponent sender)
		{
			return sender != null && sender.channel == this.channel && (sender.TeamID == this.TeamID || this.AllowCrossTeamCommunication) && this.jamTimer <= 0f && (this.LinkToChat || (this.signalOutConnection != null && this.signalOutConnection.IsConnectedToSomething())) && Vector2.DistanceSquared(this.item.WorldPosition, sender.item.WorldPosition) <= sender.range * sender.range && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x06006175 RID: 24949 RVA: 0x0032A983 File Offset: 0x00328B83
		public IEnumerable<WifiComponent> GetTransmittersInRange()
		{
			return from w in WifiComponent.list
			where w != this && w.CanTransmit(this)
			select w;
		}

		// Token: 0x06006176 RID: 24950 RVA: 0x0032A99C File Offset: 0x00328B9C
		public bool CanTransmit(WifiComponent sender)
		{
			return sender != null && sender.channel == this.channel && (sender.TeamID == this.TeamID || this.AllowCrossTeamCommunication) && Vector2.DistanceSquared(this.item.WorldPosition, sender.item.WorldPosition) <= sender.range * sender.range && this.jamTimer <= 0f && base.HasRequiredContainedItems(null, false, null);
		}

		// Token: 0x06006177 RID: 24951 RVA: 0x0032AA18 File Offset: 0x00328C18
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

		// Token: 0x06006178 RID: 24952 RVA: 0x0032AA7C File Offset: 0x00328C7C
		public int GetChannelMemory(int index)
		{
			if (index < 0 || index >= 10)
			{
				return 0;
			}
			return this.channelMemory[index];
		}

		// Token: 0x06006179 RID: 24953 RVA: 0x0032AA91 File Offset: 0x00328C91
		public void SetChannelMemory(int index, int value)
		{
			if (index < 0 || index >= 10)
			{
				return;
			}
			this.channelMemory[index] = MathHelper.Clamp(value, 0, 10000);
		}

		// Token: 0x0600617A RID: 24954 RVA: 0x0032AAB4 File Offset: 0x00328CB4
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
			foreach (WifiComponent wifiComp in receivers)
			{
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
							if (wifiComp.item.ParentInventory.Owner == Character.Controlled && GameMain.Client == null)
							{
								GameSession gameSession = GameMain.GameSession;
								if (gameSession != null)
								{
									CrewManager crewManager = gameSession.CrewManager;
									if (crewManager != null)
									{
										Item source = signal.source;
										crewManager.AddSinglePlayerChatMessage(((source != null) ? source.Name : null) ?? "", signal.value, ChatMessageType.Radio, this.item);
									}
								}
							}
							chatMsgSent = true;
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

		// Token: 0x0600617B RID: 24955 RVA: 0x0032AECC File Offset: 0x003290CC
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
					int num = this.Channel;
					return;
				}
				return;
			}
			this.TransmitSignal(signal, false);
		}

		// Token: 0x0600617C RID: 24956 RVA: 0x0032AF5C File Offset: 0x0032915C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			WifiComponent.list.Remove(this);
		}

		// Token: 0x0600617D RID: 24957 RVA: 0x0032AF70 File Offset: 0x00329170
		public override XElement Save(XElement parentElement)
		{
			XElement element = base.Save(parentElement);
			element.Add(new XAttribute("channelmemory", string.Join<int>(',', this.channelMemory)));
			return element;
		}

		// Token: 0x0600617E RID: 24958 RVA: 0x0032AFA8 File Offset: 0x003291A8
		protected void SharedEventWrite(IWriteMessage msg)
		{
			msg.WriteRangedInteger(this.Channel, 0, 10000);
			for (int i = 0; i < 10; i++)
			{
				msg.WriteRangedInteger(this.channelMemory[i], 0, 10000);
			}
		}

		// Token: 0x0600617F RID: 24959 RVA: 0x0032AFE8 File Offset: 0x003291E8
		protected void SharedEventRead(IReadMessage msg)
		{
			this.Channel = msg.ReadRangedInteger(0, 10000);
			for (int i = 0; i < 10; i++)
			{
				this.channelMemory[i] = msg.ReadRangedInteger(0, 10000);
			}
		}

		// Token: 0x04003235 RID: 12853
		private static readonly List<WifiComponent> list = new List<WifiComponent>();

		// Token: 0x04003236 RID: 12854
		private const int ChannelMemorySize = 10;

		// Token: 0x04003237 RID: 12855
		private const int MinChannel = 0;

		// Token: 0x04003238 RID: 12856
		private const int MaxChannel = 10000;

		// Token: 0x04003239 RID: 12857
		private float range;

		// Token: 0x0400323A RID: 12858
		private int channel;

		// Token: 0x0400323B RID: 12859
		private float chatMsgCooldown;

		// Token: 0x0400323C RID: 12860
		private string prevSignal;

		// Token: 0x0400323D RID: 12861
		private int[] channelMemory = new int[10];

		// Token: 0x0400323E RID: 12862
		private Connection signalInConnection;

		// Token: 0x0400323F RID: 12863
		private Connection signalOutConnection;

		// Token: 0x04003245 RID: 12869
		private float jamTimer;
	}
}
