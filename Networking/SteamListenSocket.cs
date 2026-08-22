using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma.Networking
{
	// Token: 0x02000467 RID: 1127
	internal sealed class SteamListenSocket : P2PSocket
	{
		// Token: 0x06004BD1 RID: 19409 RVA: 0x0029CC7F File Offset: 0x0029AE7F
		private SteamListenSocket(P2PSocket.Callbacks callbacks, SteamListenSocket.SocketManager socketManager, P2PSocket.OwnerOrClient type) : base(callbacks, type)
		{
			this.socketManager = socketManager;
		}

		// Token: 0x06004BD2 RID: 19410 RVA: 0x0029CC90 File Offset: 0x0029AE90
		public static Result<P2PSocket, P2PSocket.Error> Create(P2PSocket.Callbacks callbacks, P2PSocket.OwnerOrClient type)
		{
			if (!SteamManager.IsInitialized)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.SteamNotInitialized, ""));
			}
			SteamListenSocket.SocketManager socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamListenSocket.SocketManager>(0);
			if (socketManager == null)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.FailedToCreateSteamP2PSocket, ""));
			}
			socketManager.SetCallbacks(callbacks);
			P2PSocket socket = new SteamListenSocket(callbacks, socketManager, type);
			socketManager.SetSocket(socket);
			return Result.Success<P2PSocket>(socket);
		}

		// Token: 0x06004BD3 RID: 19411 RVA: 0x0029CCFC File Offset: 0x0029AEFC
		public override void ProcessIncomingMessages()
		{
			int iteration;
			for (iteration = 0; iteration < 100; iteration++)
			{
				int received = this.socketManager.Receive(32, false);
				if (received < 32)
				{
					break;
				}
			}
			if (iteration >= 100)
			{
				DebugConsole.ThrowError("Steam P2P socket received too many messages in a single frame.", null, null, false, false);
			}
		}

		// Token: 0x06004BD4 RID: 19412 RVA: 0x0029CD40 File Offset: 0x0029AF40
		public override bool SendMessage(P2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod)
		{
			SteamP2PEndpoint steamP2PEndpoint = endpoint as SteamP2PEndpoint;
			return steamP2PEndpoint != null && this.socketManager.SendMessage(steamP2PEndpoint, outMsg, deliveryMethod);
		}

		// Token: 0x06004BD5 RID: 19413 RVA: 0x0029CD68 File Offset: 0x0029AF68
		public override void CloseConnection(P2PEndpoint endpoint)
		{
			SteamP2PEndpoint steamP2PEndpoint = endpoint as SteamP2PEndpoint;
			if (steamP2PEndpoint == null)
			{
				return;
			}
			this.socketManager.CloseConnection(steamP2PEndpoint);
		}

		// Token: 0x06004BD6 RID: 19414 RVA: 0x0029CD8C File Offset: 0x0029AF8C
		public override void Dispose()
		{
			this.socketManager.Close();
		}

		// Token: 0x040027A7 RID: 10151
		private readonly SteamListenSocket.SocketManager socketManager;

		// Token: 0x020011F2 RID: 4594
		private sealed class SocketManager : Steamworks.SocketManager, ISocketManager
		{
			// Token: 0x060092AE RID: 37550 RVA: 0x003C9B5B File Offset: 0x003C7D5B
			public void SetCallbacks(P2PSocket.Callbacks callbacks)
			{
				this.callbacks = callbacks;
			}

			// Token: 0x060092AF RID: 37551 RVA: 0x003C9B64 File Offset: 0x003C7D64
			public void SetSocket(P2PSocket socket)
			{
				this.socket = socket;
			}

			// Token: 0x060092B0 RID: 37552 RVA: 0x003C9B70 File Offset: 0x003C7D70
			public override void OnConnecting(Connection connection, ConnectionInfo info)
			{
				if (!info.Identity.IsSteamId)
				{
					return;
				}
				SteamP2PEndpoint remoteEndpoint = new SteamP2PEndpoint(new SteamId(info.Identity));
				this.endpointToConnection[remoteEndpoint] = connection;
				if (this.callbacks.OnIncomingConnection(remoteEndpoint))
				{
					connection.Accept();
				}
			}

			// Token: 0x060092B1 RID: 37553 RVA: 0x003C9BD4 File Offset: 0x003C7DD4
			public override void OnDisconnected(Connection connection, ConnectionInfo info)
			{
				if (!info.Identity.IsSteamId)
				{
					return;
				}
				SteamP2PEndpoint remoteEndpoint = new SteamP2PEndpoint(new SteamId(info.Identity));
				this.endpointToConnection.Remove(remoteEndpoint);
				NetConnectionEnd endReason = info.EndReason;
				DisconnectReason disconnectReason;
				if (endReason <= NetConnectionEnd.AppException_Min)
				{
					if (endReason == NetConnectionEnd.App_Min)
					{
						disconnectReason = DisconnectReason.Disconnected;
						goto IL_157;
					}
					if (endReason == NetConnectionEnd.AppException_Min)
					{
						disconnectReason = DisconnectReason.Unknown;
						goto IL_157;
					}
				}
				else
				{
					switch (endReason)
					{
					case NetConnectionEnd.Local_OfflineMode:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					case NetConnectionEnd.Local_ManyRelayConnectivity:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					case NetConnectionEnd.Local_HostedServerPrimaryRelay:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					case NetConnectionEnd.Local_NetworkConfig:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					case NetConnectionEnd.Local_Rights:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					case NetConnectionEnd.Local_P2P_ICE_NoPublicAddresses:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_157;
					default:
						switch (endReason)
						{
						case NetConnectionEnd.Remote_Timeout:
							disconnectReason = DisconnectReason.SteamP2PTimeOut;
							goto IL_157;
						case NetConnectionEnd.Remote_BadCrypt:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_157;
						case NetConnectionEnd.Remote_BadCert:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_157;
						case (NetConnectionEnd)4004:
						case (NetConnectionEnd)4005:
							break;
						case NetConnectionEnd.Remote_BadProtocolVersion:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_157;
						case NetConnectionEnd.Remote_P2P_ICE_NoPublicAddresses:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_157;
						default:
							switch (endReason)
							{
							case NetConnectionEnd.Misc_Generic:
								disconnectReason = DisconnectReason.Unknown;
								goto IL_157;
							case NetConnectionEnd.Misc_InternalError:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							case NetConnectionEnd.Misc_Timeout:
								disconnectReason = DisconnectReason.SteamP2PTimeOut;
								goto IL_157;
							case NetConnectionEnd.Misc_SteamConnectivity:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							case NetConnectionEnd.Misc_NoRelaySessionsToClient:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							case NetConnectionEnd.Misc_P2P_Rendezvous:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							case NetConnectionEnd.Misc_P2P_NAT_Firewall:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							case NetConnectionEnd.Misc_PeerSentNoConnection:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_157;
							}
							break;
						}
						break;
					}
				}
				disconnectReason = DisconnectReason.Unknown;
				IL_157:
				PeerDisconnectPacket peerDisconnectPacket = PeerDisconnectPacket.WithReason(disconnectReason);
				this.callbacks.OnConnectionClosed(remoteEndpoint, peerDisconnectPacket);
				base.OnDisconnected(connection, info);
			}

			// Token: 0x060092B2 RID: 37554 RVA: 0x003C9D5C File Offset: 0x003C7F5C
			public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
			{
				if (!identity.IsSteamId || data == IntPtr.Zero)
				{
					return;
				}
				SteamP2PEndpoint endpoint = new SteamP2PEndpoint(new SteamId(identity));
				byte[] dataArray = new byte[size];
				Marshal.Copy(data, dataArray, 0, size);
				this.callbacks.OnData(endpoint, new ReadWriteMessage(dataArray, 0, size * 8, false));
				P2PSocket p2PSocket = this.socket;
				if (!(((p2PSocket != null) ? new P2PSocket.OwnerOrClient?(p2PSocket.Type) : null) != P2PSocket.OwnerOrClient.Owner))
				{
					this.socket.dosProtection.OnPacket(endpoint);
				}
			}

			// Token: 0x060092B3 RID: 37555 RVA: 0x003C9E04 File Offset: 0x003C8004
			internal bool SendMessage(SteamP2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod)
			{
				Connection connection;
				if (!this.endpointToConnection.TryGetValue(endpoint, out connection))
				{
					return false;
				}
				byte[] buffer = outMsg.Buffer;
				int lengthBytes = outMsg.LengthBytes;
				SendType sendType;
				if (deliveryMethod == DeliveryMethod.Reliable)
				{
					sendType = SendType.Reliable;
				}
				else
				{
					sendType = SendType.Unreliable;
				}
				Result result = connection.SendMessage(buffer, 0, lengthBytes, sendType, 0);
				return result == Result.OK;
			}

			// Token: 0x060092B4 RID: 37556 RVA: 0x003C9E50 File Offset: 0x003C8050
			internal void CloseConnection(SteamP2PEndpoint endpoint)
			{
				Connection connection;
				if (!this.endpointToConnection.TryGetValue(endpoint, out connection))
				{
					return;
				}
				connection.Close(false, 0, "Closing Connection");
			}

			// Token: 0x04005DB3 RID: 23987
			private P2PSocket.Callbacks callbacks;

			// Token: 0x04005DB4 RID: 23988
			private P2PSocket socket;

			// Token: 0x04005DB5 RID: 23989
			private readonly Dictionary<SteamP2PEndpoint, Connection> endpointToConnection = new Dictionary<SteamP2PEndpoint, Connection>();
		}
	}
}
