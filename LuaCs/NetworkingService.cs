using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Data;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E6 RID: 1254
	internal class NetworkingService : INetworkingService, IReusableService, IService, IDisposable, ILuaCsNetworking, ILuaCsShim, IEntityNetworkingService, IEventServerConnected, IEvent<IEventServerConnected>, IEvent, IEventServerRawNetMessageReceived, IEvent<IEventServerRawNetMessageReceived>, IEventSettingInstanceLifetime, IEvent<IEventSettingInstanceLifetime>
	{
		// Token: 0x060051BD RID: 20925 RVA: 0x002BF9F0 File Offset: 0x002BDBF0
		public void OnServerConnected()
		{
			this.ActivateNetVars();
			this.SendSyncMessage();
		}

		// Token: 0x060051BE RID: 20926 RVA: 0x002BFA00 File Offset: 0x002BDC00
		private void ActivateNetVars()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			foreach (INetworkSyncVar networkSyncVar in this.netVars.Keys)
			{
				networkSyncVar.SetNetworkOwner(this);
			}
		}

		// Token: 0x060051BF RID: 20927 RVA: 0x002BFA5C File Offset: 0x002BDC5C
		public bool? OnReceivedServerNetMessage(IReadMessage netMessage, ServerPacketHeader serverPacketHeader)
		{
			if (serverPacketHeader != this.ServerHeader)
			{
				return null;
			}
			switch (netMessage.ReadByte())
			{
			case 0:
				this.HandleNetMessageId(netMessage, null);
				break;
			case 1:
				this.HandleNetMessageString(netMessage, null);
				break;
			case 2:
				this.ReadIds(netMessage);
				break;
			}
			return new bool?(true);
		}

		// Token: 0x060051C0 RID: 20928 RVA: 0x002BFAB8 File Offset: 0x002BDCB8
		private void SendSyncMessage()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ClientHeader);
			message.WriteByte(3);
			GameMain.Client.ClientPeer.Send(message, DeliveryMethod.Reliable, true);
		}

		// Token: 0x060051C1 RID: 20929 RVA: 0x002BFAFC File Offset: 0x002BDCFC
		public IWriteMessage Start(NetworkingService.NetId netId)
		{
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ClientHeader);
			if (this.idToPacket.ContainsKey(netId))
			{
				message.WriteByte(0);
				message.WriteUInt16(this.idToPacket[netId]);
			}
			else
			{
				message.WriteByte(1);
				NetworkingService.NetId.Write(message, netId);
			}
			return message;
		}

		// Token: 0x060051C2 RID: 20930 RVA: 0x002BFB54 File Offset: 0x002BDD54
		public void SendToServer(IWriteMessage netMessage, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
		{
			GameMain.Client.ClientPeer.Send(netMessage, deliveryMethod, true);
		}

		// Token: 0x060051C3 RID: 20931 RVA: 0x002BFB68 File Offset: 0x002BDD68
		public void Send(IWriteMessage netMessage, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
		{
			this.SendToServer(netMessage, deliveryMethod);
		}

		// Token: 0x060051C4 RID: 20932 RVA: 0x002BFB74 File Offset: 0x002BDD74
		private void RequestId(NetworkingService.NetId netId)
		{
			if (this.idToPacket.ContainsKey(netId))
			{
				return;
			}
			if (GameMain.Client == null)
			{
				return;
			}
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ClientHeader);
			message.WriteByte(2);
			NetworkingService.NetId.Write(message, netId);
			this.SendToServer(message, DeliveryMethod.Reliable);
		}

		// Token: 0x060051C5 RID: 20933 RVA: 0x002BFBC4 File Offset: 0x002BDDC4
		private void HandleNetMessageId(IReadMessage netMessage, Client client = null)
		{
			ushort id = netMessage.ReadUInt16();
			if (this.packetToId.ContainsKey(id))
			{
				this.HandleNetMessage(netMessage, this.packetToId[id], client);
				return;
			}
			if (!this.receiveQueue.ContainsKey(id))
			{
				this.receiveQueue[id] = new ConcurrentQueue<IReadMessage>();
			}
			this.receiveQueue[id].Enqueue(netMessage);
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				ILoggerService loggerService = this._loggerService;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(99, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Received NetMessage with unknown id ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
				defaultInterpolatedStringHandler.AppendLiteral(" from server, storing in queue in case we receive the id later.");
				loggerService.LogMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, null);
			}
		}

		// Token: 0x060051C6 RID: 20934 RVA: 0x002BFC88 File Offset: 0x002BDE88
		private void ReadIds(IReadMessage netMessage)
		{
			ushort size = netMessage.ReadUInt16();
			for (int i = 0; i < (int)size; i++)
			{
				ushort packetId = netMessage.ReadUInt16();
				NetworkingService.NetId netId = NetworkingService.NetId.Read(netMessage);
				this.packetToId[packetId] = netId;
				this.idToPacket[netId] = packetId;
				if (this.receiveQueue.ContainsKey(packetId))
				{
					IReadMessage queueMessage;
					while (this.receiveQueue[packetId].TryDequeue(out queueMessage))
					{
						if (this.netReceives.ContainsKey(netId))
						{
							this.netReceives[netId](queueMessage);
						}
					}
				}
			}
		}

		// Token: 0x170014CE RID: 5326
		// (get) Token: 0x060051C7 RID: 20935 RVA: 0x002BFD18 File Offset: 0x002BDF18
		public ClientPacketHeader ClientHeader
		{
			get
			{
				if (this.clientHeader == null)
				{
					byte lastHeader = (byte)Enum.GetValues(typeof(ClientPacketHeader)).Cast<ClientPacketHeader>().Last<ClientPacketHeader>();
					this.clientHeader = new ClientPacketHeader?((int)lastHeader + ClientPacketHeader.UPDATE_INGAME);
				}
				return this.clientHeader.Value;
			}
		}

		// Token: 0x170014CF RID: 5327
		// (get) Token: 0x060051C8 RID: 20936 RVA: 0x002BFD68 File Offset: 0x002BDF68
		public ServerPacketHeader ServerHeader
		{
			get
			{
				if (this.serverHeader == null)
				{
					byte lastHeader = (byte)Enum.GetValues(typeof(ServerPacketHeader)).Cast<ServerPacketHeader>().Last<ServerPacketHeader>();
					this.serverHeader = new ServerPacketHeader?((int)lastHeader + ServerPacketHeader.AUTH_FAILURE);
				}
				return this.serverHeader.Value;
			}
		}

		// Token: 0x170014D0 RID: 5328
		// (get) Token: 0x060051C9 RID: 20937 RVA: 0x002BFDB6 File Offset: 0x002BDFB6
		public bool IsActive
		{
			get
			{
				return GameMain.NetworkMember != null;
			}
		}

		// Token: 0x170014D1 RID: 5329
		// (get) Token: 0x060051CA RID: 20938 RVA: 0x002BFDC0 File Offset: 0x002BDFC0
		// (set) Token: 0x060051CB RID: 20939 RVA: 0x002BFDC8 File Offset: 0x002BDFC8
		public bool IsSynchronized { get; private set; }

		// Token: 0x170014D2 RID: 5330
		// (get) Token: 0x060051CC RID: 20940 RVA: 0x002BFDD1 File Offset: 0x002BDFD1
		// (set) Token: 0x060051CD RID: 20941 RVA: 0x002BFDD9 File Offset: 0x002BDFD9
		public bool IsDisposed { get; private set; }

		// Token: 0x060051CE RID: 20942 RVA: 0x002BFDE4 File Offset: 0x002BDFE4
		public NetworkingService(IEventService eventService, INetworkIdProvider networkIdProvider, ILoggerService loggerService)
		{
			this._eventService = eventService;
			this._networkIdProvider = networkIdProvider;
			this._loggerService = loggerService;
			this.SubscribeToEvents();
		}

		// Token: 0x060051CF RID: 20943 RVA: 0x002BFE4C File Offset: 0x002BE04C
		public void Receive(string netIdString, LuaCsAction callback)
		{
			this.Receive(new NetworkingService.NetId(netIdString), delegate(IReadMessage message)
			{
				LuaCsAction callback2 = callback;
				object[] array = new object[2];
				array[0] = message;
				callback2(array);
			});
		}

		// Token: 0x060051D0 RID: 20944 RVA: 0x002BFE7E File Offset: 0x002BE07E
		public void Receive(string netIdString, NetMessageReceived callback)
		{
			this.Receive(new NetworkingService.NetId(netIdString), callback);
		}

		// Token: 0x060051D1 RID: 20945 RVA: 0x002BFE8D File Offset: 0x002BE08D
		public void Receive(Guid netIdGuid, NetMessageReceived callback)
		{
			this.Receive(new NetworkingService.NetId(netIdGuid.ToString()), callback);
		}

		// Token: 0x060051D2 RID: 20946 RVA: 0x002BFEA8 File Offset: 0x002BE0A8
		public IWriteMessage Start(string netIdString)
		{
			if (netIdString == null)
			{
				return new WriteOnlyMessage();
			}
			return this.Start(new NetworkingService.NetId(netIdString));
		}

		// Token: 0x060051D3 RID: 20947 RVA: 0x002BFEBF File Offset: 0x002BE0BF
		public IWriteMessage Start(Guid netIdGuid)
		{
			return this.Start(new NetworkingService.NetId(netIdGuid.ToString()));
		}

		// Token: 0x060051D4 RID: 20948 RVA: 0x002BFED9 File Offset: 0x002BE0D9
		public IWriteMessage Start()
		{
			return new WriteOnlyMessage();
		}

		// Token: 0x060051D5 RID: 20949 RVA: 0x002BFEE0 File Offset: 0x002BE0E0
		internal void Receive(NetworkingService.NetId netId, NetMessageReceived callback)
		{
			this.RequestId(netId);
			this.netReceives[netId] = callback;
		}

		// Token: 0x060051D6 RID: 20950 RVA: 0x002BFEF8 File Offset: 0x002BE0F8
		private void HandleNetMessage(IReadMessage netMessage, NetworkingService.NetId netId, Client client = null)
		{
			if (this.netReceives.ContainsKey(netId))
			{
				try
				{
					this.netReceives[netId](netMessage);
					return;
				}
				catch (Exception e)
				{
					this._loggerService.LogResults(new ExceptionalError("Exception thrown inside NetMessageReceive({netId})", e));
					return;
				}
			}
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				ILoggerService loggerService = this._loggerService;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Received NetMessage for unknown netid ");
				defaultInterpolatedStringHandler.AppendFormatted<NetworkingService.NetId>(netId);
				defaultInterpolatedStringHandler.AppendLiteral(" from server.");
				loggerService.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x060051D7 RID: 20951 RVA: 0x002BFFA0 File Offset: 0x002BE1A0
		private void HandleNetMessageString(IReadMessage netMessage, Client client = null)
		{
			NetworkingService.NetId netId = NetworkingService.NetId.Read(netMessage);
			this.HandleNetMessage(netMessage, netId, client);
		}

		// Token: 0x060051D8 RID: 20952 RVA: 0x002BFFBD File Offset: 0x002BE1BD
		private void SubscribeToEvents()
		{
			this._eventService.Subscribe<IEventSettingInstanceLifetime>(this);
			this._eventService.Subscribe<IEventServerConnected>(this);
			this._eventService.Subscribe<IEventServerRawNetMessageReceived>(this);
		}

		// Token: 0x060051D9 RID: 20953 RVA: 0x002BFFE6 File Offset: 0x002BE1E6
		public Guid GetNetworkIdForInstance(INetworkSyncVar var)
		{
			return this._networkIdProvider.GetNetworkIdForInstance(var);
		}

		// Token: 0x060051DA RID: 20954 RVA: 0x002BFFF4 File Offset: 0x002BE1F4
		public void RegisterNetVar(INetworkSyncVar netVar)
		{
			netVar.SetNetworkOwner(this);
			NetworkingService.NetId netId = new NetworkingService.NetId(netVar.InstanceId.ToString());
			this.netVars[netVar] = netId;
			this.Receive(netId, delegate(IReadMessage message)
			{
				if (netVar.SyncType == NetSync.None)
				{
					this._loggerService.LogWarning("Received net var from server but NetSync is " + netVar.SyncType.ToString());
					return;
				}
				netVar.ReadNetMessage(message);
			});
		}

		// Token: 0x060051DB RID: 20955 RVA: 0x002C0068 File Offset: 0x002BE268
		public void DeregisterNetVar(INetworkSyncVar netVar)
		{
			if (netVar == null)
			{
				return;
			}
			netVar.SetNetworkOwner(null);
			NetworkingService.NetId netId;
			this.netVars.TryRemove(netVar, out netId);
		}

		// Token: 0x060051DC RID: 20956 RVA: 0x002C008F File Offset: 0x002BE28F
		public void SendNetVar(INetworkSyncVar netVar)
		{
			this.SendNetVar(netVar, null);
		}

		// Token: 0x060051DD RID: 20957 RVA: 0x002C009C File Offset: 0x002BE29C
		public void SendNetVar(INetworkSyncVar netVar, NetworkConnection connection = null)
		{
			NetworkingService.NetId netId;
			if (!this.netVars.TryGetValue(netVar, out netId))
			{
				throw new InvalidOperationException("Tried to send net var across network without registering first");
			}
			if (netVar.SyncType == NetSync.None)
			{
				return;
			}
			if (netVar.SyncType == NetSync.ServerAuthority)
			{
				return;
			}
			IWriteMessage message = this.Start(netId);
			netVar.WriteNetMessage(message);
			this.SendToServer(message, DeliveryMethod.Reliable);
		}

		// Token: 0x060051DE RID: 20958 RVA: 0x002C00EE File Offset: 0x002BE2EE
		public Result Reset()
		{
			this.IsSynchronized = false;
			this.netReceives = new ConcurrentDictionary<NetworkingService.NetId, NetMessageReceived>();
			this.packetToId = new ConcurrentDictionary<ushort, NetworkingService.NetId>();
			this.idToPacket = new ConcurrentDictionary<NetworkingService.NetId, ushort>();
			this.netVars = new ConcurrentDictionary<INetworkSyncVar, NetworkingService.NetId>();
			this.SubscribeToEvents();
			return Result.Ok();
		}

		// Token: 0x060051DF RID: 20959 RVA: 0x002C012E File Offset: 0x002BE32E
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x060051E0 RID: 20960 RVA: 0x002C0138 File Offset: 0x002BE338
		public void HttpRequest(string url, LuaCsAction callback, string data = null, string method = "POST", string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			NetworkingService.<HttpRequest>d__56 <HttpRequest>d__;
			<HttpRequest>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<HttpRequest>d__.url = url;
			<HttpRequest>d__.callback = callback;
			<HttpRequest>d__.data = data;
			<HttpRequest>d__.method = method;
			<HttpRequest>d__.contentType = contentType;
			<HttpRequest>d__.headers = headers;
			<HttpRequest>d__.savePath = savePath;
			<HttpRequest>d__.<>1__state = -1;
			<HttpRequest>d__.<>t__builder.Start<NetworkingService.<HttpRequest>d__56>(ref <HttpRequest>d__);
		}

		// Token: 0x060051E1 RID: 20961 RVA: 0x002C01A3 File Offset: 0x002BE3A3
		public void HttpPost(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, data, "POST", contentType, headers, savePath);
		}

		// Token: 0x060051E2 RID: 20962 RVA: 0x002C01B9 File Offset: 0x002BE3B9
		public void RequestPostHTTP(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, data, "POST", contentType, headers, savePath);
		}

		// Token: 0x060051E3 RID: 20963 RVA: 0x002C01CF File Offset: 0x002BE3CF
		public void HttpGet(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, null, "GET", null, headers, savePath);
		}

		// Token: 0x060051E4 RID: 20964 RVA: 0x002C01E3 File Offset: 0x002BE3E3
		public void RequestGetHTTP(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, null, "GET", null, headers, savePath);
		}

		// Token: 0x060051E5 RID: 20965 RVA: 0x002C01F7 File Offset: 0x002BE3F7
		public void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData)
		{
			GameMain.NetworkMember.CreateEntityEvent(entity, extraData);
		}

		// Token: 0x170014D3 RID: 5331
		// (get) Token: 0x060051E6 RID: 20966 RVA: 0x002C0205 File Offset: 0x002BE405
		// (set) Token: 0x060051E7 RID: 20967 RVA: 0x002C0211 File Offset: 0x002BE411
		public ushort LastClientListUpdateID
		{
			get
			{
				return GameMain.NetworkMember.LastClientListUpdateID;
			}
			set
			{
				GameMain.NetworkMember.LastClientListUpdateID = value;
			}
		}

		// Token: 0x060051E8 RID: 20968 RVA: 0x002C0220 File Offset: 0x002BE420
		public void OnSettingInstanceCreated<T>(T configInstance) where T : ISettingBase
		{
			INetworkSyncVar syncVar = configInstance as INetworkSyncVar;
			if (syncVar != null)
			{
				this.RegisterNetVar(syncVar);
			}
		}

		// Token: 0x060051E9 RID: 20969 RVA: 0x002C0244 File Offset: 0x002BE444
		public void OnSettingInstanceDisposed<T>(T configInstance) where T : ISettingBase
		{
			INetworkSyncVar syncVar = configInstance as INetworkSyncVar;
			if (syncVar != null)
			{
				this.DeregisterNetVar(syncVar);
			}
		}

		// Token: 0x04002B53 RID: 11091
		private ConcurrentDictionary<ushort, ConcurrentQueue<IReadMessage>> receiveQueue = new ConcurrentDictionary<ushort, ConcurrentQueue<IReadMessage>>();

		// Token: 0x04002B54 RID: 11092
		private ClientPacketHeader? clientHeader;

		// Token: 0x04002B55 RID: 11093
		private ServerPacketHeader? serverHeader;

		// Token: 0x04002B56 RID: 11094
		private ConcurrentDictionary<INetworkSyncVar, NetworkingService.NetId> netVars = new ConcurrentDictionary<INetworkSyncVar, NetworkingService.NetId>();

		// Token: 0x04002B57 RID: 11095
		private ConcurrentDictionary<NetworkingService.NetId, NetMessageReceived> netReceives = new ConcurrentDictionary<NetworkingService.NetId, NetMessageReceived>();

		// Token: 0x04002B58 RID: 11096
		private ConcurrentDictionary<ushort, NetworkingService.NetId> packetToId = new ConcurrentDictionary<ushort, NetworkingService.NetId>();

		// Token: 0x04002B59 RID: 11097
		private ConcurrentDictionary<NetworkingService.NetId, ushort> idToPacket = new ConcurrentDictionary<NetworkingService.NetId, ushort>();

		// Token: 0x04002B5C RID: 11100
		private readonly IEventService _eventService;

		// Token: 0x04002B5D RID: 11101
		private readonly ILoggerService _loggerService;

		// Token: 0x04002B5E RID: 11102
		private readonly INetworkIdProvider _networkIdProvider;

		// Token: 0x04002B5F RID: 11103
		private static readonly HttpClient client = new HttpClient();

		// Token: 0x0200129E RID: 4766
		public readonly struct NetId : IEquatable<NetworkingService.NetId>
		{
			// Token: 0x060094B6 RID: 38070 RVA: 0x003D0E7A File Offset: 0x003CF07A
			public NetId(string netId)
			{
				this._value = netId;
			}

			// Token: 0x060094B7 RID: 38071 RVA: 0x003D0E83 File Offset: 0x003CF083
			public static void Write(IWriteMessage message, NetworkingService.NetId netId)
			{
				message.WriteString(netId._value);
			}

			// Token: 0x060094B8 RID: 38072 RVA: 0x003D0E91 File Offset: 0x003CF091
			public static NetworkingService.NetId Read(IReadMessage message)
			{
				return new NetworkingService.NetId(message.ReadString());
			}

			// Token: 0x060094B9 RID: 38073 RVA: 0x003D0EA0 File Offset: 0x003CF0A0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetId");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060094BA RID: 38074 RVA: 0x003D0EEC File Offset: 0x003CF0EC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				return false;
			}

			// Token: 0x060094BB RID: 38075 RVA: 0x003D0EEF File Offset: 0x003CF0EF
			[CompilerGenerated]
			public static bool operator !=(NetworkingService.NetId left, NetworkingService.NetId right)
			{
				return !(left == right);
			}

			// Token: 0x060094BC RID: 38076 RVA: 0x003D0EFB File Offset: 0x003CF0FB
			[CompilerGenerated]
			public static bool operator ==(NetworkingService.NetId left, NetworkingService.NetId right)
			{
				return left.Equals(right);
			}

			// Token: 0x060094BD RID: 38077 RVA: 0x003D0F05 File Offset: 0x003CF105
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this._value);
			}

			// Token: 0x060094BE RID: 38078 RVA: 0x003D0F17 File Offset: 0x003CF117
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is NetworkingService.NetId && this.Equals((NetworkingService.NetId)obj);
			}

			// Token: 0x060094BF RID: 38079 RVA: 0x003D0F2F File Offset: 0x003CF12F
			[CompilerGenerated]
			public bool Equals(NetworkingService.NetId other)
			{
				return EqualityComparer<string>.Default.Equals(this._value, other._value);
			}

			// Token: 0x04005FC7 RID: 24519
			private readonly string _value;
		}

		// Token: 0x0200129F RID: 4767
		private enum ClientToServer
		{
			// Token: 0x04005FC9 RID: 24521
			NetMessageInternalId,
			// Token: 0x04005FCA RID: 24522
			NetMessageNetId,
			// Token: 0x04005FCB RID: 24523
			RequestSingleNetId,
			// Token: 0x04005FCC RID: 24524
			RequestSync
		}

		// Token: 0x020012A0 RID: 4768
		private enum ServerToClient
		{
			// Token: 0x04005FCE RID: 24526
			NetMessageInternalId,
			// Token: 0x04005FCF RID: 24527
			NetMessageNetId,
			// Token: 0x04005FD0 RID: 24528
			ReceiveNetIds
		}
	}
}
