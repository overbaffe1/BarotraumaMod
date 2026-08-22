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
	// Token: 0x020003DA RID: 986
	internal class NetworkingService : INetworkingService, IReusableService, IService, IDisposable, ILuaCsNetworking, ILuaCsShim, IEntityNetworkingService, IEventClientRawNetMessageReceived, IEvent<IEventClientRawNetMessageReceived>, IEvent, IEventSettingInstanceLifetime, IEvent<IEventSettingInstanceLifetime>
	{
		// Token: 0x060038C9 RID: 14537 RVA: 0x0017B6DC File Offset: 0x001798DC
		public IWriteMessage Start(NetworkingService.NetId netId)
		{
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ServerHeader);
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

		// Token: 0x060038CA RID: 14538 RVA: 0x0017B734 File Offset: 0x00179934
		public bool? OnReceivedClientNetMessage(IReadMessage netMessage, ClientPacketHeader clientPacketHeader, NetworkConnection sender)
		{
			if (clientPacketHeader != this.ClientHeader)
			{
				return null;
			}
			Client client = GameMain.Server.ConnectedClients.First((Client c) => c.Connection == sender);
			switch (netMessage.ReadByte())
			{
			case 0:
				this.HandleNetMessageId(netMessage, client);
				break;
			case 1:
				this.HandleNetMessageString(netMessage, client);
				break;
			case 2:
				this.RequestIdSingle(netMessage, client);
				break;
			case 3:
				this.WriteSync(client);
				break;
			}
			return new bool?(true);
		}

		// Token: 0x060038CB RID: 14539 RVA: 0x0017B7C8 File Offset: 0x001799C8
		private void HandleNetMessageId(IReadMessage netMessage, Client client = null)
		{
			ushort id = netMessage.ReadUInt16();
			if (this.packetToId.ContainsKey(id))
			{
				NetworkingService.NetId netId = this.packetToId[id];
				this.HandleNetMessage(netMessage, netId, client);
				return;
			}
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				ILoggerService loggerService = this._loggerService;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Received NetMessage for unknown id ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
				defaultInterpolatedStringHandler.AppendLiteral(" from ");
				defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, null));
				defaultInterpolatedStringHandler.AppendLiteral(".");
				loggerService.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x060038CC RID: 14540 RVA: 0x0017B864 File Offset: 0x00179A64
		private ushort RegisterId(NetworkingService.NetId netId)
		{
			if (this.idToPacket.ContainsKey(netId))
			{
				return this.idToPacket[netId];
			}
			if (this.currentId >= 65535)
			{
				ILoggerService loggerService = this._loggerService;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to register more than ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(ushort.MaxValue);
				defaultInterpolatedStringHandler.AppendLiteral(" network ids!");
				loggerService.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
				return 0;
			}
			this.currentId += 1;
			this.packetToId[this.currentId] = netId;
			this.idToPacket[netId] = this.currentId;
			this.WriteIdToAll(this.currentId, netId);
			return this.currentId;
		}

		// Token: 0x060038CD RID: 14541 RVA: 0x0017B924 File Offset: 0x00179B24
		private void RequestIdSingle(IReadMessage netMessage, Client client)
		{
			NetworkingService.NetId netId = NetworkingService.NetId.Read(netMessage);
			AccountId id;
			if (!this.idToPacket.ContainsKey(netId) && client.AccountId.TryUnwrap(out id))
			{
				if (!this.clientRegisterCount.ContainsKey(id.StringRepresentation))
				{
					this.clientRegisterCount[id.StringRepresentation] = 0;
				}
				Dictionary<string, int> dictionary = this.clientRegisterCount;
				string stringRepresentation = id.StringRepresentation;
				int num = dictionary[stringRepresentation];
				dictionary[stringRepresentation] = num + 1;
				if (this.clientRegisterCount[id.StringRepresentation] > 1000)
				{
					ILoggerService loggerService = this._loggerService;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
					defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, null));
					defaultInterpolatedStringHandler.AppendLiteral(" Tried to register more than ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(1000);
					defaultInterpolatedStringHandler.AppendLiteral(" Ids!");
					loggerService.Log(defaultInterpolatedStringHandler.ToStringAndClear(), null, ServerLog.MessageType.ServerMessage);
					return;
				}
			}
			this.RegisterId(netId);
		}

		// Token: 0x060038CE RID: 14542 RVA: 0x0017BA20 File Offset: 0x00179C20
		private void WriteIdToAll(ushort packet, NetworkingService.NetId netId)
		{
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ServerHeader);
			message.WriteByte(2);
			message.WriteUInt16(1);
			message.WriteUInt16(packet);
			NetworkingService.NetId.Write(message, netId);
			this.SendToClient(message, null, DeliveryMethod.Reliable);
		}

		// Token: 0x060038CF RID: 14543 RVA: 0x0017BA68 File Offset: 0x00179C68
		private void WriteSync(Client client)
		{
			WriteOnlyMessage message = new WriteOnlyMessage();
			message.WriteByte((byte)this.ServerHeader);
			message.WriteByte(2);
			message.WriteUInt16((ushort)this.packetToId.Count<KeyValuePair<ushort, NetworkingService.NetId>>());
			foreach (KeyValuePair<ushort, NetworkingService.NetId> keyValuePair in this.packetToId)
			{
				ushort num;
				NetworkingService.NetId netId2;
				keyValuePair.Deconstruct(out num, out netId2);
				ushort packet = num;
				NetworkingService.NetId netId = netId2;
				message.WriteUInt16(packet);
				NetworkingService.NetId.Write(message, netId);
			}
			this.SendToClient(message, client.Connection, DeliveryMethod.Reliable);
			foreach (INetworkSyncVar netVar in this.netVars.Keys)
			{
				this.SendNetVar(netVar, client.Connection);
			}
		}

		// Token: 0x060038D0 RID: 14544 RVA: 0x0017BB58 File Offset: 0x00179D58
		public void SendToClient(IWriteMessage netMessage, NetworkConnection connection = null, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
		{
			if (connection == null)
			{
				using (IEnumerator<NetworkConnection> enumerator = (from c in ModUtils.Client.ClientList
				select c.Connection).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetworkConnection conn = enumerator.Current;
						GameMain.Server.ServerPeer.Send(netMessage, conn, deliveryMethod, true);
					}
					return;
				}
			}
			GameMain.Server.ServerPeer.Send(netMessage, connection, deliveryMethod, true);
		}

		// Token: 0x060038D1 RID: 14545 RVA: 0x0017BBEC File Offset: 0x00179DEC
		public void Send(IWriteMessage netMessage, NetworkConnection connection = null, DeliveryMethod deliveryMethod = DeliveryMethod.Reliable)
		{
			this.SendToClient(netMessage, connection, deliveryMethod);
		}

		// Token: 0x17000F8C RID: 3980
		// (get) Token: 0x060038D2 RID: 14546 RVA: 0x0017BBF8 File Offset: 0x00179DF8
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

		// Token: 0x17000F8D RID: 3981
		// (get) Token: 0x060038D3 RID: 14547 RVA: 0x0017BC48 File Offset: 0x00179E48
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

		// Token: 0x17000F8E RID: 3982
		// (get) Token: 0x060038D4 RID: 14548 RVA: 0x0017BC96 File Offset: 0x00179E96
		public bool IsActive
		{
			get
			{
				return GameMain.NetworkMember != null;
			}
		}

		// Token: 0x17000F8F RID: 3983
		// (get) Token: 0x060038D5 RID: 14549 RVA: 0x0017BCA0 File Offset: 0x00179EA0
		// (set) Token: 0x060038D6 RID: 14550 RVA: 0x0017BCA8 File Offset: 0x00179EA8
		public bool IsSynchronized { get; private set; }

		// Token: 0x17000F90 RID: 3984
		// (get) Token: 0x060038D7 RID: 14551 RVA: 0x0017BCB1 File Offset: 0x00179EB1
		// (set) Token: 0x060038D8 RID: 14552 RVA: 0x0017BCB9 File Offset: 0x00179EB9
		public bool IsDisposed { get; private set; }

		// Token: 0x060038D9 RID: 14553 RVA: 0x0017BCC4 File Offset: 0x00179EC4
		public NetworkingService(IEventService eventService, INetworkIdProvider networkIdProvider, ILoggerService loggerService)
		{
			this._eventService = eventService;
			this._networkIdProvider = networkIdProvider;
			this._loggerService = loggerService;
			this.IsSynchronized = true;
			this.SubscribeToEvents();
		}

		// Token: 0x060038DA RID: 14554 RVA: 0x0017BD30 File Offset: 0x00179F30
		public void Receive(string netIdString, LuaCsAction callback)
		{
			this.Receive(new NetworkingService.NetId(netIdString), delegate(IReadMessage message, Client client)
			{
				callback(new object[]
				{
					message,
					client
				});
			});
		}

		// Token: 0x060038DB RID: 14555 RVA: 0x0017BD62 File Offset: 0x00179F62
		public void Receive(string netIdString, NetMessageReceived callback)
		{
			this.Receive(new NetworkingService.NetId(netIdString), callback);
		}

		// Token: 0x060038DC RID: 14556 RVA: 0x0017BD71 File Offset: 0x00179F71
		public void Receive(Guid netIdGuid, NetMessageReceived callback)
		{
			this.Receive(new NetworkingService.NetId(netIdGuid.ToString()), callback);
		}

		// Token: 0x060038DD RID: 14557 RVA: 0x0017BD8C File Offset: 0x00179F8C
		public IWriteMessage Start(string netIdString)
		{
			if (netIdString == null)
			{
				return new WriteOnlyMessage();
			}
			return this.Start(new NetworkingService.NetId(netIdString));
		}

		// Token: 0x060038DE RID: 14558 RVA: 0x0017BDA3 File Offset: 0x00179FA3
		public IWriteMessage Start(Guid netIdGuid)
		{
			return this.Start(new NetworkingService.NetId(netIdGuid.ToString()));
		}

		// Token: 0x060038DF RID: 14559 RVA: 0x0017BDBD File Offset: 0x00179FBD
		public IWriteMessage Start()
		{
			return new WriteOnlyMessage();
		}

		// Token: 0x060038E0 RID: 14560 RVA: 0x0017BDC4 File Offset: 0x00179FC4
		internal void Receive(NetworkingService.NetId netId, NetMessageReceived callback)
		{
			this.RegisterId(netId);
			this.netReceives[netId] = callback;
		}

		// Token: 0x060038E1 RID: 14561 RVA: 0x0017BDDC File Offset: 0x00179FDC
		private void HandleNetMessage(IReadMessage netMessage, NetworkingService.NetId netId, Client client = null)
		{
			if (this.netReceives.ContainsKey(netId))
			{
				try
				{
					this.netReceives[netId](netMessage, client);
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
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Received NetMessage for unknown netid ");
				defaultInterpolatedStringHandler.AppendFormatted<NetworkingService.NetId>(netId);
				defaultInterpolatedStringHandler.AppendLiteral(" from ");
				defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, null));
				defaultInterpolatedStringHandler.AppendLiteral(".");
				loggerService.LogError(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x060038E2 RID: 14562 RVA: 0x0017BEA0 File Offset: 0x0017A0A0
		private void HandleNetMessageString(IReadMessage netMessage, Client client = null)
		{
			NetworkingService.NetId netId = NetworkingService.NetId.Read(netMessage);
			this.HandleNetMessage(netMessage, netId, client);
		}

		// Token: 0x060038E3 RID: 14563 RVA: 0x0017BEBD File Offset: 0x0017A0BD
		private void SubscribeToEvents()
		{
			this._eventService.Subscribe<IEventSettingInstanceLifetime>(this);
			this._eventService.Subscribe<IEventClientRawNetMessageReceived>(this);
		}

		// Token: 0x060038E4 RID: 14564 RVA: 0x0017BED9 File Offset: 0x0017A0D9
		public Guid GetNetworkIdForInstance(INetworkSyncVar var)
		{
			return this._networkIdProvider.GetNetworkIdForInstance(var);
		}

		// Token: 0x060038E5 RID: 14565 RVA: 0x0017BEE8 File Offset: 0x0017A0E8
		public void RegisterNetVar(INetworkSyncVar netVar)
		{
			netVar.SetNetworkOwner(this);
			NetworkingService.NetId netId = new NetworkingService.NetId(netVar.InstanceId.ToString());
			this.netVars[netVar] = netId;
			this.Receive(netId, delegate(IReadMessage message, Client client)
			{
				if (netVar.SyncType == NetSync.None || netVar.SyncType == NetSync.ServerAuthority)
				{
					ILoggerService loggerService = this._loggerService;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Received net var from ");
					defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, null));
					defaultInterpolatedStringHandler.AppendLiteral(" but ");
					defaultInterpolatedStringHandler.AppendFormatted("NetSync");
					defaultInterpolatedStringHandler.AppendLiteral(" is ");
					defaultInterpolatedStringHandler.AppendFormatted(netVar.SyncType.ToString());
					loggerService.LogWarning(defaultInterpolatedStringHandler.ToStringAndClear());
					return;
				}
				if (!client.HasPermission(netVar.WritePermissions))
				{
					this._loggerService.LogWarning("Received net var from " + NetworkMember.ClientLogName(client, null) + " but the client lacks permissions to modify it");
					return;
				}
				netVar.ReadNetMessage(message);
				if (netVar.SyncType != NetSync.ClientOneWay)
				{
					this.SendNetVar(netVar);
				}
			});
		}

		// Token: 0x060038E6 RID: 14566 RVA: 0x0017BF5C File Offset: 0x0017A15C
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

		// Token: 0x060038E7 RID: 14567 RVA: 0x0017BF83 File Offset: 0x0017A183
		public void SendNetVar(INetworkSyncVar netVar)
		{
			this.SendNetVar(netVar, null);
		}

		// Token: 0x060038E8 RID: 14568 RVA: 0x0017BF90 File Offset: 0x0017A190
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
			if (netVar.SyncType == NetSync.ClientOneWay)
			{
				return;
			}
			IWriteMessage message = this.Start(netId);
			netVar.WriteNetMessage(message);
			this.SendToClient(message, connection, DeliveryMethod.Reliable);
		}

		// Token: 0x060038E9 RID: 14569 RVA: 0x0017BFE3 File Offset: 0x0017A1E3
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

		// Token: 0x060038EA RID: 14570 RVA: 0x0017C023 File Offset: 0x0017A223
		public void Dispose()
		{
			this.IsDisposed = true;
		}

		// Token: 0x060038EB RID: 14571 RVA: 0x0017C02C File Offset: 0x0017A22C
		public void HttpRequest(string url, LuaCsAction callback, string data = null, string method = "POST", string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			NetworkingService.<HttpRequest>d__57 <HttpRequest>d__;
			<HttpRequest>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<HttpRequest>d__.url = url;
			<HttpRequest>d__.callback = callback;
			<HttpRequest>d__.data = data;
			<HttpRequest>d__.method = method;
			<HttpRequest>d__.contentType = contentType;
			<HttpRequest>d__.headers = headers;
			<HttpRequest>d__.savePath = savePath;
			<HttpRequest>d__.<>1__state = -1;
			<HttpRequest>d__.<>t__builder.Start<NetworkingService.<HttpRequest>d__57>(ref <HttpRequest>d__);
		}

		// Token: 0x060038EC RID: 14572 RVA: 0x0017C097 File Offset: 0x0017A297
		public void HttpPost(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, data, "POST", contentType, headers, savePath);
		}

		// Token: 0x060038ED RID: 14573 RVA: 0x0017C0AD File Offset: 0x0017A2AD
		public void RequestPostHTTP(string url, LuaCsAction callback, string data, string contentType = "application/json", Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, data, "POST", contentType, headers, savePath);
		}

		// Token: 0x060038EE RID: 14574 RVA: 0x0017C0C3 File Offset: 0x0017A2C3
		public void HttpGet(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, null, "GET", null, headers, savePath);
		}

		// Token: 0x060038EF RID: 14575 RVA: 0x0017C0D7 File Offset: 0x0017A2D7
		public void RequestGetHTTP(string url, LuaCsAction callback, Dictionary<string, string> headers = null, string savePath = null)
		{
			this.HttpRequest(url, callback, null, "GET", null, headers, savePath);
		}

		// Token: 0x060038F0 RID: 14576 RVA: 0x0017C0EB File Offset: 0x0017A2EB
		public void CreateEntityEvent(INetSerializable entity, NetEntityEvent.IData extraData)
		{
			GameMain.NetworkMember.CreateEntityEvent(entity, extraData);
		}

		// Token: 0x17000F91 RID: 3985
		// (get) Token: 0x060038F1 RID: 14577 RVA: 0x0017C0F9 File Offset: 0x0017A2F9
		// (set) Token: 0x060038F2 RID: 14578 RVA: 0x0017C105 File Offset: 0x0017A305
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

		// Token: 0x060038F3 RID: 14579 RVA: 0x0017C112 File Offset: 0x0017A312
		public void ClientWriteLobby(Client client)
		{
			GameMain.Server.ClientWriteLobby(client);
		}

		// Token: 0x060038F4 RID: 14580 RVA: 0x0017C11F File Offset: 0x0017A31F
		public void UpdateClientPermissions(Client client)
		{
			GameMain.Server.UpdateClientPermissions(client);
		}

		// Token: 0x17000F92 RID: 3986
		// (get) Token: 0x060038F5 RID: 14581 RVA: 0x0017C12C File Offset: 0x0017A32C
		// (set) Token: 0x060038F6 RID: 14582 RVA: 0x0017C133 File Offset: 0x0017A333
		public int FileSenderMaxPacketsPerUpdate
		{
			get
			{
				return FileSender.FileTransferOut.MaxPacketsPerUpdate;
			}
			set
			{
				FileSender.FileTransferOut.MaxPacketsPerUpdate = value;
			}
		}

		// Token: 0x060038F7 RID: 14583 RVA: 0x0017C13C File Offset: 0x0017A33C
		public void OnSettingInstanceCreated<T>(T configInstance) where T : ISettingBase
		{
			INetworkSyncVar syncVar = configInstance as INetworkSyncVar;
			if (syncVar != null)
			{
				this.RegisterNetVar(syncVar);
			}
		}

		// Token: 0x060038F8 RID: 14584 RVA: 0x0017C160 File Offset: 0x0017A360
		public void OnSettingInstanceDisposed<T>(T configInstance) where T : ISettingBase
		{
			INetworkSyncVar syncVar = configInstance as INetworkSyncVar;
			if (syncVar != null)
			{
				this.DeregisterNetVar(syncVar);
			}
		}

		// Token: 0x04001CA1 RID: 7329
		private const int MaxRegisterPerClient = 1000;

		// Token: 0x04001CA2 RID: 7330
		private Dictionary<string, int> clientRegisterCount = new Dictionary<string, int>();

		// Token: 0x04001CA3 RID: 7331
		private ushort currentId;

		// Token: 0x04001CA4 RID: 7332
		private ClientPacketHeader? clientHeader;

		// Token: 0x04001CA5 RID: 7333
		private ServerPacketHeader? serverHeader;

		// Token: 0x04001CA6 RID: 7334
		private ConcurrentDictionary<INetworkSyncVar, NetworkingService.NetId> netVars = new ConcurrentDictionary<INetworkSyncVar, NetworkingService.NetId>();

		// Token: 0x04001CA7 RID: 7335
		private ConcurrentDictionary<NetworkingService.NetId, NetMessageReceived> netReceives = new ConcurrentDictionary<NetworkingService.NetId, NetMessageReceived>();

		// Token: 0x04001CA8 RID: 7336
		private ConcurrentDictionary<ushort, NetworkingService.NetId> packetToId = new ConcurrentDictionary<ushort, NetworkingService.NetId>();

		// Token: 0x04001CA9 RID: 7337
		private ConcurrentDictionary<NetworkingService.NetId, ushort> idToPacket = new ConcurrentDictionary<NetworkingService.NetId, ushort>();

		// Token: 0x04001CAC RID: 7340
		private readonly IEventService _eventService;

		// Token: 0x04001CAD RID: 7341
		private readonly ILoggerService _loggerService;

		// Token: 0x04001CAE RID: 7342
		private readonly INetworkIdProvider _networkIdProvider;

		// Token: 0x04001CAF RID: 7343
		private static readonly HttpClient client = new HttpClient();

		// Token: 0x02000C6E RID: 3182
		public readonly struct NetId : IEquatable<NetworkingService.NetId>
		{
			// Token: 0x06006417 RID: 25623 RVA: 0x002137DA File Offset: 0x002119DA
			public NetId(string netId)
			{
				this._value = netId;
			}

			// Token: 0x06006418 RID: 25624 RVA: 0x002137E3 File Offset: 0x002119E3
			public static void Write(IWriteMessage message, NetworkingService.NetId netId)
			{
				message.WriteString(netId._value);
			}

			// Token: 0x06006419 RID: 25625 RVA: 0x002137F1 File Offset: 0x002119F1
			public static NetworkingService.NetId Read(IReadMessage message)
			{
				return new NetworkingService.NetId(message.ReadString());
			}

			// Token: 0x0600641A RID: 25626 RVA: 0x00213800 File Offset: 0x00211A00
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

			// Token: 0x0600641B RID: 25627 RVA: 0x0021384C File Offset: 0x00211A4C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				return false;
			}

			// Token: 0x0600641C RID: 25628 RVA: 0x0021384F File Offset: 0x00211A4F
			[CompilerGenerated]
			public static bool operator !=(NetworkingService.NetId left, NetworkingService.NetId right)
			{
				return !(left == right);
			}

			// Token: 0x0600641D RID: 25629 RVA: 0x0021385B File Offset: 0x00211A5B
			[CompilerGenerated]
			public static bool operator ==(NetworkingService.NetId left, NetworkingService.NetId right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600641E RID: 25630 RVA: 0x00213865 File Offset: 0x00211A65
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this._value);
			}

			// Token: 0x0600641F RID: 25631 RVA: 0x00213877 File Offset: 0x00211A77
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is NetworkingService.NetId && this.Equals((NetworkingService.NetId)obj);
			}

			// Token: 0x06006420 RID: 25632 RVA: 0x0021388F File Offset: 0x00211A8F
			[CompilerGenerated]
			public bool Equals(NetworkingService.NetId other)
			{
				return EqualityComparer<string>.Default.Equals(this._value, other._value);
			}

			// Token: 0x04003C59 RID: 15449
			private readonly string _value;
		}

		// Token: 0x02000C6F RID: 3183
		private enum ClientToServer
		{
			// Token: 0x04003C5B RID: 15451
			NetMessageInternalId,
			// Token: 0x04003C5C RID: 15452
			NetMessageNetId,
			// Token: 0x04003C5D RID: 15453
			RequestSingleNetId,
			// Token: 0x04003C5E RID: 15454
			RequestSync
		}

		// Token: 0x02000C70 RID: 3184
		private enum ServerToClient
		{
			// Token: 0x04003C60 RID: 15456
			NetMessageInternalId,
			// Token: 0x04003C61 RID: 15457
			NetMessageNetId,
			// Token: 0x04003C62 RID: 15458
			ReceiveNetIds
		}
	}
}
