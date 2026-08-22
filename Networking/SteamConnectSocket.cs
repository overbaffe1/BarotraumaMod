using System;
using System.Runtime.InteropServices;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma.Networking
{
	// Token: 0x02000466 RID: 1126
	internal sealed class SteamConnectSocket : P2PSocket
	{
		// Token: 0x06004BCB RID: 19403 RVA: 0x0029CB12 File Offset: 0x0029AD12
		private SteamConnectSocket(SteamP2PEndpoint expectedEndpoint, P2PSocket.Callbacks callbacks, SteamConnectSocket.ConnectionManager connectionManager, P2PSocket.OwnerOrClient type) : base(callbacks, type)
		{
			this.expectedEndpoint = expectedEndpoint;
			this.connectionManager = connectionManager;
		}

		// Token: 0x06004BCC RID: 19404 RVA: 0x0029CB2C File Offset: 0x0029AD2C
		public static Result<P2PSocket, P2PSocket.Error> Create(SteamP2PEndpoint endpoint, P2PSocket.Callbacks callbacks, P2PSocket.OwnerOrClient type)
		{
			if (!SteamManager.IsInitialized)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.SteamNotInitialized, ""));
			}
			SteamConnectSocket.ConnectionManager connectionManager;
			try
			{
				connectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectSocket.ConnectionManager>(endpoint.SteamId.Value, 0);
			}
			catch (ArgumentException e)
			{
				DebugConsole.ThrowError("Failed to connect via SteamP2P. Are you logged in to Steam, is the same Steam account already connected to the server?", e, null, false, false);
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.FailedToCreateSteamP2PSocket, ""));
			}
			if (connectionManager == null)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.FailedToCreateSteamP2PSocket, ""));
			}
			connectionManager.SetEndpointAndCallbacks(endpoint, callbacks);
			return Result.Success<P2PSocket>(new SteamConnectSocket(endpoint, callbacks, connectionManager, type));
		}

		// Token: 0x06004BCD RID: 19405 RVA: 0x0029CBE0 File Offset: 0x0029ADE0
		public override void ProcessIncomingMessages()
		{
			this.connectionManager.Receive(32, true);
		}

		// Token: 0x06004BCE RID: 19406 RVA: 0x0029CBF4 File Offset: 0x0029ADF4
		public override bool SendMessage(P2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod)
		{
			if (endpoint != this.expectedEndpoint)
			{
				return false;
			}
			SteamConnectSocket.ConnectionManager connectionManager = this.connectionManager;
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
			Result result = connectionManager.Connection.SendMessage(buffer, 0, lengthBytes, sendType, 0);
			return result == Result.OK;
		}

		// Token: 0x06004BCF RID: 19407 RVA: 0x0029CC48 File Offset: 0x0029AE48
		public override void CloseConnection(P2PEndpoint endpoint)
		{
			if (endpoint != this.expectedEndpoint)
			{
				return;
			}
			this.connectionManager.Close(false, 0, "Closing Connection");
		}

		// Token: 0x06004BD0 RID: 19408 RVA: 0x0029CC6B File Offset: 0x0029AE6B
		public override void Dispose()
		{
			this.connectionManager.Close(false, 0, "Closing Connection");
		}

		// Token: 0x040027A5 RID: 10149
		private readonly SteamP2PEndpoint expectedEndpoint;

		// Token: 0x040027A6 RID: 10150
		private readonly SteamConnectSocket.ConnectionManager connectionManager;

		// Token: 0x020011F1 RID: 4593
		private sealed class ConnectionManager : Steamworks.ConnectionManager, IConnectionManager
		{
			// Token: 0x060092AA RID: 37546 RVA: 0x003C998C File Offset: 0x003C7B8C
			public void SetEndpointAndCallbacks(SteamP2PEndpoint endpoint, P2PSocket.Callbacks callbacks)
			{
				this.endpoint = endpoint;
				this.callbacks = callbacks;
			}

			// Token: 0x060092AB RID: 37547 RVA: 0x003C999C File Offset: 0x003C7B9C
			public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
			{
				byte[] dataArray = new byte[size];
				Marshal.Copy(data, dataArray, 0, size);
				this.callbacks.OnData(this.endpoint, new ReadWriteMessage(dataArray, 0, size * 8, false));
			}

			// Token: 0x060092AC RID: 37548 RVA: 0x003C99DC File Offset: 0x003C7BDC
			public override void OnDisconnected(ConnectionInfo info)
			{
				if (!info.Identity.IsSteamId)
				{
					return;
				}
				SteamP2PEndpoint remoteEndpoint = new SteamP2PEndpoint(new SteamId(info.Identity));
				NetConnectionEnd endReason = info.EndReason;
				DisconnectReason disconnectReason;
				if (endReason <= NetConnectionEnd.AppException_Min)
				{
					if (endReason == NetConnectionEnd.App_Min)
					{
						disconnectReason = DisconnectReason.Disconnected;
						goto IL_14A;
					}
					if (endReason == NetConnectionEnd.AppException_Min)
					{
						disconnectReason = DisconnectReason.Unknown;
						goto IL_14A;
					}
				}
				else
				{
					switch (endReason)
					{
					case NetConnectionEnd.Local_OfflineMode:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					case NetConnectionEnd.Local_ManyRelayConnectivity:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					case NetConnectionEnd.Local_HostedServerPrimaryRelay:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					case NetConnectionEnd.Local_NetworkConfig:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					case NetConnectionEnd.Local_Rights:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					case NetConnectionEnd.Local_P2P_ICE_NoPublicAddresses:
						disconnectReason = DisconnectReason.SteamP2PError;
						goto IL_14A;
					default:
						switch (endReason)
						{
						case NetConnectionEnd.Remote_Timeout:
							disconnectReason = DisconnectReason.SteamP2PTimeOut;
							goto IL_14A;
						case NetConnectionEnd.Remote_BadCrypt:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_14A;
						case NetConnectionEnd.Remote_BadCert:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_14A;
						case (NetConnectionEnd)4004:
						case (NetConnectionEnd)4005:
							break;
						case NetConnectionEnd.Remote_BadProtocolVersion:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_14A;
						case NetConnectionEnd.Remote_P2P_ICE_NoPublicAddresses:
							disconnectReason = DisconnectReason.SteamP2PError;
							goto IL_14A;
						default:
							switch (endReason)
							{
							case NetConnectionEnd.Misc_Generic:
								disconnectReason = DisconnectReason.Unknown;
								goto IL_14A;
							case NetConnectionEnd.Misc_InternalError:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							case NetConnectionEnd.Misc_Timeout:
								disconnectReason = DisconnectReason.SteamP2PTimeOut;
								goto IL_14A;
							case NetConnectionEnd.Misc_SteamConnectivity:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							case NetConnectionEnd.Misc_NoRelaySessionsToClient:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							case NetConnectionEnd.Misc_P2P_Rendezvous:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							case NetConnectionEnd.Misc_P2P_NAT_Firewall:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							case NetConnectionEnd.Misc_PeerSentNoConnection:
								disconnectReason = DisconnectReason.SteamP2PError;
								goto IL_14A;
							}
							break;
						}
						break;
					}
				}
				disconnectReason = DisconnectReason.Unknown;
				IL_14A:
				PeerDisconnectPacket peerDisconnectPacket = PeerDisconnectPacket.WithReason(disconnectReason);
				this.callbacks.OnConnectionClosed(remoteEndpoint, peerDisconnectPacket);
				base.OnDisconnected(info);
			}

			// Token: 0x04005DB1 RID: 23985
			private SteamP2PEndpoint endpoint;

			// Token: 0x04005DB2 RID: 23986
			private P2PSocket.Callbacks callbacks;
		}
	}
}
