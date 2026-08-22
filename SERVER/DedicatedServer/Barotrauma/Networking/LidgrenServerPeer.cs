using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Barotrauma.Steam;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x02000372 RID: 882
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class LidgrenServerPeer : ServerPeer<LidgrenConnection>
	{
		// Token: 0x060034A6 RID: 13478 RVA: 0x0016A4B0 File Offset: 0x001686B0
		[NullableContext(0)]
		public LidgrenServerPeer(Option<int> ownKey, [Nullable(1)] ServerSettings settings, ServerPeer.Callbacks callbacks) : base(callbacks, settings)
		{
			this.authenticators = null;
			this.netServer = null;
			this.netPeerConfiguration = new NetPeerConfiguration("barotrauma")
			{
				AcceptIncomingConnections = true,
				AutoExpandMTU = false,
				MaximumConnections = NetConfig.MaxPlayers * 2,
				EnableUPnP = this.serverSettings.EnableUPnP,
				Port = this.serverSettings.Port,
				DualStack = GameSettings.CurrentConfig.UseDualModeSockets,
				LocalAddress = this.serverSettings.ListenIPAddress
			};
			if (NetConfig.UseLenientHandshake)
			{
				this.netPeerConfiguration.ConnectionTimeout = 60f;
				this.netPeerConfiguration.ResendHandshakeInterval = 5f;
				this.netPeerConfiguration.MaximumHandshakeAttempts = 20;
			}
			this.netPeerConfiguration.DisableMessageType((NetIncomingMessageType)1810);
			this.netPeerConfiguration.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			this.incomingLidgrenMessages = new List<NetIncomingMessage>();
			this.ownerKey = ownKey;
		}

		// Token: 0x060034A7 RID: 13479 RVA: 0x0016A5A4 File Offset: 0x001687A4
		public override void Start()
		{
			if (this.netServer != null)
			{
				return;
			}
			Option.UnspecifiedNone none = Option.None;
			this.authenticators = Authenticator.GetAuthenticatorsForHost(none);
			this.incomingLidgrenMessages.Clear();
			this.netServer = new NetServer(this.netPeerConfiguration);
			this.netServer.Start();
			if (this.serverSettings.EnableUPnP)
			{
				this.InitUPnP();
				while (this.DiscoveringUPnP())
				{
				}
				this.FinishUPnP();
			}
		}

		// Token: 0x060034A8 RID: 13480 RVA: 0x0016A61C File Offset: 0x0016881C
		public override void Close()
		{
			if (this.netServer == null)
			{
				return;
			}
			for (int i = this.pendingClients.Count - 1; i >= 0; i--)
			{
				base.RemovePendingClient(this.pendingClients[i], PeerDisconnectPacket.WithReason(DisconnectReason.ServerShutdown));
			}
			for (int j = this.connectedClients.Count - 1; j >= 0; j--)
			{
				this.Disconnect(this.connectedClients[j].Connection, PeerDisconnectPacket.WithReason(DisconnectReason.ServerShutdown));
			}
			this.netServer.Shutdown(PeerDisconnectPacket.WithReason(DisconnectReason.ServerShutdown).ToLidgrenStringRepresentation());
			this.pendingClients.Clear();
			this.connectedClients.Clear();
			this.netServer = null;
			this.callbacks.OnShutdown();
		}

		// Token: 0x060034A9 RID: 13481 RVA: 0x0016A6E0 File Offset: 0x001688E0
		public override void Update(float deltaTime)
		{
			if (this.netServer == null)
			{
				return;
			}
			ToolBox.ThrowIfNull<List<NetIncomingMessage>>(this.incomingLidgrenMessages);
			this.netServer.ReadMessages(this.incomingLidgrenMessages);
			foreach (NetIncomingMessage inc in from m in this.incomingLidgrenMessages
			where m.MessageType == NetIncomingMessageType.ConnectionApproval
			select m)
			{
				this.HandleConnection(inc);
			}
			try
			{
				foreach (NetIncomingMessage inc2 in from m in this.incomingLidgrenMessages
				where m.MessageType != NetIncomingMessageType.ConnectionApproval
				select m)
				{
					NetIncomingMessageType messageType = inc2.MessageType;
					if (messageType != NetIncomingMessageType.StatusChanged)
					{
						if (messageType == NetIncomingMessageType.Data)
						{
							this.HandleDataMessage(inc2);
						}
					}
					else
					{
						this.HandleStatusChanged(inc2);
					}
				}
			}
			catch (Exception e)
			{
				string str = "Server failed to read an incoming message. {";
				Exception ex = e;
				string errorMsg = str + ((ex != null) ? ex.ToString() : null) + "}\n" + e.StackTrace.CleanupStackTrace();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
				defaultInterpolatedStringHandler.AppendLiteral("LidgrenServerPeer.Update:ClientReadException");
				defaultInterpolatedStringHandler.AppendFormatted<MethodBase>(e.TargetSite);
				GameAnalyticsManager.AddErrorEventOnce(defaultInterpolatedStringHandler.ToStringAndClear(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
			}
			for (int i = 0; i < this.pendingClients.Count; i++)
			{
				ServerPeer<LidgrenConnection>.PendingClient pendingClient = this.pendingClients[i];
				LidgrenConnection connection = pendingClient.Connection;
				if (connection.NetConnection.Status != NetConnectionStatus.InitiatedConnect && connection.NetConnection.Status != NetConnectionStatus.ReceivedInitiation && connection.NetConnection.Status != NetConnectionStatus.RespondedAwaitingApproval && connection.NetConnection.Status != NetConnectionStatus.RespondedConnect)
				{
					base.UpdatePendingClient(pendingClient);
					if (i >= this.pendingClients.Count || this.pendingClients[i] != pendingClient)
					{
						i--;
					}
				}
			}
			this.incomingLidgrenMessages.Clear();
		}

		// Token: 0x060034AA RID: 13482 RVA: 0x0016A928 File Offset: 0x00168B28
		private void InitUPnP()
		{
			if (this.netServer == null)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetPeerConfiguration>(this.netPeerConfiguration);
			this.netServer.UPnP.ForwardPort(this.netPeerConfiguration.Port, "barotrauma");
			if (SteamManager.IsInitialized)
			{
				this.netServer.UPnP.ForwardPort(this.serverSettings.QueryPort, "barotrauma");
			}
		}

		// Token: 0x060034AB RID: 13483 RVA: 0x0016A992 File Offset: 0x00168B92
		private bool DiscoveringUPnP()
		{
			return this.netServer != null && this.netServer.UPnP.Status == UPnPStatus.Discovering;
		}

		// Token: 0x060034AC RID: 13484 RVA: 0x0016A9B1 File Offset: 0x00168BB1
		private void FinishUPnP()
		{
		}

		// Token: 0x060034AD RID: 13485 RVA: 0x0016A9B4 File Offset: 0x00168BB4
		private void HandleConnection(NetIncomingMessage inc)
		{
			if (this.netServer == null)
			{
				return;
			}
			if (this.connectedClients.Count >= this.serverSettings.MaxPlayers)
			{
				inc.SenderConnection.Deny(PeerDisconnectPacket.WithReason(DisconnectReason.ServerFull).ToLidgrenStringRepresentation());
				return;
			}
			string banReason;
			if (this.serverSettings.BanList.IsBanned(new LidgrenEndpoint(inc.SenderConnection.RemoteEndPoint), out banReason))
			{
				inc.SenderConnection.Deny(PeerDisconnectPacket.Banned(banReason).ToLidgrenStringRepresentation());
				return;
			}
			if (this.pendingClients.Find((ServerPeer<LidgrenConnection>.PendingClient c) => c.Connection.NetConnection == inc.SenderConnection) == null)
			{
				ServerPeer<LidgrenConnection>.PendingClient pendingClient = new ServerPeer<LidgrenConnection>.PendingClient(new LidgrenConnection(inc.SenderConnection));
				this.pendingClients.Add(pendingClient);
				string str = "Incoming connection from ";
				NetConnection netConnection = pendingClient.Connection.NetConnection;
				string text;
				if (netConnection == null)
				{
					text = null;
				}
				else
				{
					IPEndPoint remoteEndPoint = netConnection.RemoteEndPoint;
					text = ((remoteEndPoint != null) ? remoteEndPoint.ToString() : null);
				}
				GameServer.Log(str + (text ?? "null") + ".", ServerLog.MessageType.ServerMessage);
			}
			inc.SenderConnection.Approve();
		}

		// Token: 0x060034AE RID: 13486 RVA: 0x0016AAE4 File Offset: 0x00168CE4
		private void HandleDataMessage(NetIncomingMessage lidgrenMsg)
		{
			LidgrenServerPeer.<>c__DisplayClass12_0 CS$<>8__locals1 = new LidgrenServerPeer.<>c__DisplayClass12_0();
			CS$<>8__locals1.lidgrenMsg = lidgrenMsg;
			CS$<>8__locals1.<>4__this = this;
			if (this.netServer == null)
			{
				return;
			}
			ServerPeer<LidgrenConnection>.PendingClient pendingClient = this.pendingClients.Find((ServerPeer<LidgrenConnection>.PendingClient c) => c.Connection.NetConnection == CS$<>8__locals1.lidgrenMsg.SenderConnection);
			IReadMessage inc = CS$<>8__locals1.lidgrenMsg.ToReadMessage();
			PeerPacketHeaders peerPacketHeaders = default(PeerPacketHeaders);
			try
			{
				peerPacketHeaders = INetSerializableStruct.Read<PeerPacketHeaders>(inc);
			}
			catch
			{
				if (pendingClient == null)
				{
					throw;
				}
				string str = "Received an invalid connection attempt from ";
				NetConnection netConnection = pendingClient.Connection.NetConnection;
				string text;
				if (netConnection == null)
				{
					text = null;
				}
				else
				{
					IPEndPoint remoteEndPoint = netConnection.RemoteEndPoint;
					text = ((remoteEndPoint != null) ? remoteEndPoint.ToString() : null);
				}
				GameServer.Log(str + (text ?? "null") + ". Banning the IP.", ServerLog.MessageType.DoSProtection);
				this.serverSettings.BanList.BanPlayer("Unknown", pendingClient.Connection.Endpoint, "Invalid connection attempt", null);
			}
			PeerPacketHeaders peerPacketHeaders2 = peerPacketHeaders;
			DeliveryMethod deliveryMethod;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			peerPacketHeaders2.Deconstruct(out deliveryMethod, out packetHeader2, out connectionInitialization);
			PacketHeader packetHeader = packetHeader2;
			ConnectionInitialization? initialization = connectionInitialization;
			if (packetHeader.IsConnectionInitializationStep() && pendingClient != null && initialization != null)
			{
				base.ReadConnectionInitializationStep(pendingClient, inc, initialization.Value);
				return;
			}
			if (!packetHeader.IsConnectionInitializationStep())
			{
				LidgrenConnection conn = CS$<>8__locals1.<HandleDataMessage>g__FindConnection|1(CS$<>8__locals1.lidgrenMsg.SenderConnection);
				if (conn == null)
				{
					if (pendingClient != null)
					{
						base.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationRequired));
						return;
					}
					if (CS$<>8__locals1.lidgrenMsg.SenderConnection.Status != NetConnectionStatus.Disconnected && CS$<>8__locals1.lidgrenMsg.SenderConnection.Status != NetConnectionStatus.Disconnecting)
					{
						CS$<>8__locals1.lidgrenMsg.SenderConnection.Disconnect(PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationRequired).ToLidgrenStringRepresentation());
					}
					return;
				}
				else
				{
					if (pendingClient != null)
					{
						this.pendingClients.Remove(pendingClient);
					}
					AccountId accountId;
					string banReason;
					if (this.serverSettings.BanList.IsBanned(conn.Endpoint, out banReason) || (conn.AccountInfo.AccountId.TryUnwrap(out accountId) && this.serverSettings.BanList.IsBanned(accountId, out banReason)) || conn.AccountInfo.OtherMatchingIds.Any((AccountId id) => CS$<>8__locals1.<>4__this.serverSettings.BanList.IsBanned(id, out banReason)))
					{
						this.Disconnect(conn, PeerDisconnectPacket.Banned(banReason));
						return;
					}
					try
					{
						PeerPacketMessage packet = INetSerializableStruct.Read<PeerPacketMessage>(inc);
						this.callbacks.OnMessageReceived(conn, packet.GetReadMessage(packetHeader.IsCompressed(), conn));
					}
					catch (NetStructReadException)
					{
						if (conn == this.OwnerConnection)
						{
							throw;
						}
						ServerPeer<LidgrenConnection>.ClientConnectionData connectedClient = this.connectedClients.Find((ServerPeer<LidgrenConnection>.ClientConnectionData c) => c.Connection == conn);
						if (connectedClient != null)
						{
							this.Disconnect(connectedClient.Connection, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
						}
					}
				}
			}
		}

		// Token: 0x060034AF RID: 13487 RVA: 0x0016ADF4 File Offset: 0x00168FF4
		private void HandleStatusChanged(NetIncomingMessage inc)
		{
			if (this.netServer == null)
			{
				return;
			}
			NetConnectionStatus status = inc.ReadHeader<NetConnectionStatus>();
			if (status == NetConnectionStatus.Disconnected)
			{
				LidgrenConnection conn = (from c in this.connectedClients
				select c.Connection).FirstOrDefault((LidgrenConnection c) => c.NetConnection == inc.SenderConnection);
				string disconnectMsg = inc.ReadString();
				PeerDisconnectPacket peerDisconnectPacket = PeerDisconnectPacket.FromLidgrenStringRepresentation(disconnectMsg).Fallback(PeerDisconnectPacket.WithReason(DisconnectReason.Unknown));
				if (conn != null)
				{
					if (conn == this.OwnerConnection)
					{
						DebugConsole.NewMessage("Owner disconnected: closing the server...", null, false);
						GameServer.Log("Owner disconnected: closing the server...", ServerLog.MessageType.ServerMessage);
						this.Close();
						return;
					}
					this.Disconnect(conn, peerDisconnectPacket);
					return;
				}
				else
				{
					ServerPeer<LidgrenConnection>.PendingClient pendingClient = this.pendingClients.Find(delegate(ServerPeer<LidgrenConnection>.PendingClient c)
					{
						LidgrenConnection i = c.Connection;
						return i != null && i.NetConnection == inc.SenderConnection;
					});
					if (pendingClient != null)
					{
						base.RemovePendingClient(pendingClient, peerDisconnectPacket);
					}
				}
			}
		}

		// Token: 0x060034B0 RID: 13488 RVA: 0x0016AEF0 File Offset: 0x001690F0
		private void OnSteamAuthChange(SteamId steamId, SteamId ownerId, AuthResponse status)
		{
			if (this.netServer == null)
			{
				return;
			}
			ServerPeer<LidgrenConnection>.PendingClient pendingClient = this.pendingClients.Find(delegate(ServerPeer<LidgrenConnection>.PendingClient c)
			{
				SteamId id;
				return c.AccountInfo.AccountId.TryUnwrap<SteamId>(out id) && id.Value == steamId;
			});
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
			defaultInterpolatedStringHandler.AppendFormatted<SteamId>(steamId);
			defaultInterpolatedStringHandler.AppendLiteral(" validation: ");
			defaultInterpolatedStringHandler.AppendFormatted<AuthResponse>(status);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(pendingClient != null);
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			if (pendingClient == null)
			{
				if (status == AuthResponse.OK)
				{
					return;
				}
				ServerPeer<LidgrenConnection>.ClientConnectionData clientConnectionData = this.connectedClients.Find(delegate(ServerPeer<LidgrenConnection>.ClientConnectionData c)
				{
					SteamId id;
					return c.Connection.AccountInfo.AccountId.TryUnwrap<SteamId>(out id) && id.Value == steamId;
				});
				if (clientConnectionData != null)
				{
					LidgrenConnection connection = clientConnectionData.Connection;
					if (connection != null)
					{
						this.Disconnect(connection, PeerDisconnectPacket.SteamAuthError(status));
					}
				}
				return;
			}
			else
			{
				LidgrenConnection pendingConnection = pendingClient.Connection;
				string banReason;
				if (this.serverSettings.BanList.IsBanned(pendingConnection.Endpoint, out banReason) || this.serverSettings.BanList.IsBanned(new SteamId(steamId), out banReason) || this.serverSettings.BanList.IsBanned(new SteamId(ownerId), out banReason))
				{
					base.RemovePendingClient(pendingClient, PeerDisconnectPacket.Banned(banReason));
					return;
				}
				if (status == AuthResponse.OK)
				{
					pendingClient.Connection.SetAccountInfo(new AccountInfo(new SteamId(steamId), new AccountId[]
					{
						new SteamId(ownerId)
					}));
					pendingClient.InitializationStep = (base.ShouldAskForPassword(this.serverSettings, pendingClient.Connection) ? ConnectionInitialization.Password : ConnectionInitialization.ContentPackageOrder);
					pendingClient.UpdateTime = Timing.TotalTime;
					return;
				}
				base.RemovePendingClient(pendingClient, PeerDisconnectPacket.SteamAuthError(status));
				return;
			}
		}

		// Token: 0x060034B1 RID: 13489 RVA: 0x0016B09C File Offset: 0x0016929C
		public override void Send(IWriteMessage msg, NetworkConnection conn, DeliveryMethod deliveryMethod, bool compressPastThreshold = true)
		{
			if (this.netServer == null)
			{
				return;
			}
			LidgrenConnection lidgrenConnection = conn as LidgrenConnection;
			if (lidgrenConnection == null)
			{
				DebugConsole.ThrowError("Tried to send message to connection of incorrect type: expected LidgrenConnection, got " + conn.GetType().Name, null, null, false, false);
				return;
			}
			if (!this.connectedClients.Any((ServerPeer<LidgrenConnection>.ClientConnectionData cc) => cc.Connection == lidgrenConnection))
			{
				DebugConsole.ThrowError("Tried to send message to unauthenticated connection: " + lidgrenConnection.Endpoint.StringRepresentation, null, null, false, false);
				return;
			}
			bool isCompressed;
			int num;
			byte[] bufAux = msg.PrepareForSending(compressPastThreshold, out isCompressed, out num);
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = (isCompressed ? PacketHeader.IsCompressed : PacketHeader.None),
				Initialization = null
			};
			PeerPacketMessage body = new PeerPacketMessage
			{
				Buffer = bufAux
			};
			this.SendMsgInternal(lidgrenConnection, headers, body);
		}

		// Token: 0x060034B2 RID: 13490 RVA: 0x0016B18C File Offset: 0x0016938C
		public override void Disconnect(NetworkConnection conn, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (this.netServer == null)
			{
				return;
			}
			LidgrenConnection lidgrenConn = conn as LidgrenConnection;
			if (lidgrenConn == null)
			{
				return;
			}
			int ccIndex = this.connectedClients.FindIndex((ServerPeer<LidgrenConnection>.ClientConnectionData cc) => cc.Connection == lidgrenConn);
			if (ccIndex >= 0)
			{
				lidgrenConn.Status = NetworkConnectionStatus.Disconnected;
				this.connectedClients.RemoveAt(ccIndex);
				this.callbacks.OnDisconnect(conn, peerDisconnectPacket);
				AccountId accountId;
				if (conn.AccountInfo.AccountId.TryUnwrap(out accountId))
				{
					ImmutableDictionary<AuthenticationTicketKind, Authenticator> immutableDictionary = this.authenticators;
					if (immutableDictionary != null)
					{
						immutableDictionary.Values.ForEach(delegate(Authenticator authenticator)
						{
							authenticator.EndAuthSession(accountId);
						});
					}
				}
			}
			lidgrenConn.NetConnection.Disconnect(peerDisconnectPacket.ToLidgrenStringRepresentation());
		}

		// Token: 0x060034B3 RID: 13491 RVA: 0x0016B260 File Offset: 0x00169460
		protected override void SendMsgInternal(LidgrenConnection conn, PeerPacketHeaders headers, [Nullable(2)] INetSerializableStruct body)
		{
			IWriteMessage msgToSend = new WriteOnlyMessage();
			msgToSend.WriteNetSerializableStruct(headers);
			if (body != null)
			{
				body.Write(msgToSend);
			}
			NetSendResult result = this.ForwardToLidgren(msgToSend, conn, headers.DeliveryMethod);
			if (result != NetSendResult.Sent && result != NetSendResult.Queued)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to send message to ");
				defaultInterpolatedStringHandler.AppendFormatted<LidgrenEndpoint>(conn.Endpoint);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<NetSendResult>(result);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
			}
		}

		// Token: 0x060034B4 RID: 13492 RVA: 0x0016B2EC File Offset: 0x001694EC
		protected override void CheckOwnership(ServerPeer<LidgrenConnection>.PendingClient pendingClient)
		{
			if (this.OwnerConnection == null)
			{
				LidgrenConnection i = pendingClient.Connection;
				if (i != null && IPAddress.IsLoopback(i.NetConnection.RemoteEndPoint.Address) && this.ownerKey.IsSome() && !(pendingClient.OwnerKey != this.ownerKey))
				{
					Option.UnspecifiedNone none = Option.None;
					this.ownerKey = none;
					this.OwnerConnection = pendingClient.Connection;
					this.callbacks.OnOwnerDetermined(this.OwnerConnection);
					return;
				}
			}
		}

		// Token: 0x060034B5 RID: 13493 RVA: 0x0016B378 File Offset: 0x00169578
		protected override void ProcessAuthTicket(ClientAuthTicketAndVersionPacket packet, ServerPeer<LidgrenConnection>.PendingClient pendingClient)
		{
			LidgrenServerPeer.<>c__DisplayClass20_0 CS$<>8__locals1 = new LidgrenServerPeer.<>c__DisplayClass20_0();
			CS$<>8__locals1.pendingClient = pendingClient;
			CS$<>8__locals1.packet = packet;
			CS$<>8__locals1.<>4__this = this;
			if (CS$<>8__locals1.pendingClient.AccountInfo.AccountId.IsSome())
			{
				if (CS$<>8__locals1.pendingClient.AccountInfo.AccountId != CS$<>8__locals1.packet.AccountId)
				{
					CS$<>8__locals1.<ProcessAuthTicket>g__rejectClient|1();
				}
				return;
			}
			if (this.authenticators == null && GameMain.Server.ServerSettings.RequireAuthentication)
			{
				DebugConsole.NewMessage("The server is configured to require authentication from clients, but there are no authenticators available. If you're for example trying to host a server in a local network without being connected to Steam or Epic Online Services, please set RequireAuthentication to false in the server settings.", new Color?(Color.Yellow), false);
			}
			AuthenticationTicket authTicket;
			Authenticator authenticator;
			if (this.authenticators != null && CS$<>8__locals1.packet.AuthTicket.TryUnwrap(out authTicket) && this.authenticators.TryGetValue(authTicket.Kind, out authenticator))
			{
				CS$<>8__locals1.pendingClient.AuthSessionStarted = true;
				TaskPool.Add("LidgrenServerPeer.ProcessAuth", authenticator.VerifyTicket(authTicket), delegate(Task t)
				{
					AccountInfo accountInfo;
					if (t.TryGetResult(out accountInfo) && !accountInfo.IsNone)
					{
						base.<ProcessAuthTicket>g__acceptClient|0(accountInfo);
						return;
					}
					if (GameMain.Server.ServerSettings.RequireAuthentication)
					{
						base.<ProcessAuthTicket>g__rejectClient|1();
						return;
					}
					base.<ProcessAuthTicket>g__acceptClient|0(new AccountInfo(new UnauthenticatedAccountId(CS$<>8__locals1.packet.Name), Array.Empty<AccountId>()));
				});
				return;
			}
			if (GameMain.Server.ServerSettings.RequireAuthentication)
			{
				DebugConsole.NewMessage("A client attempted to join without an authentication ticket, but the server is configured to require authentication. If you're for example trying to host a server in a local network without being connected to Steam or Epic Online Services, please set RequireAuthentication to false in the server settings.", new Color?(Color.Yellow), false);
				CS$<>8__locals1.<ProcessAuthTicket>g__rejectClient|1();
				return;
			}
			CS$<>8__locals1.<ProcessAuthTicket>g__acceptClient|0(new AccountInfo(new UnauthenticatedAccountId(CS$<>8__locals1.packet.Name), Array.Empty<AccountId>()));
		}

		// Token: 0x060034B6 RID: 13494 RVA: 0x0016B4BC File Offset: 0x001696BC
		private NetSendResult ForwardToLidgren(IWriteMessage msg, NetworkConnection connection, DeliveryMethod deliveryMethod)
		{
			ToolBox.ThrowIfNull<NetServer>(this.netServer);
			LidgrenConnection conn = (LidgrenConnection)connection;
			return this.netServer.SendMessage(msg.ToLidgren(this.netServer), conn.NetConnection, deliveryMethod.ToLidgren());
		}

		// Token: 0x04001A2E RID: 6702
		private readonly NetPeerConfiguration netPeerConfiguration;

		// Token: 0x04001A2F RID: 6703
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private ImmutableDictionary<AuthenticationTicketKind, Authenticator> authenticators;

		// Token: 0x04001A30 RID: 6704
		[Nullable(2)]
		private NetServer netServer;

		// Token: 0x04001A31 RID: 6705
		private readonly List<NetIncomingMessage> incomingLidgrenMessages;

		// Token: 0x02000C04 RID: 3076
		[NullableContext(0)]
		private enum AuthResult
		{
			// Token: 0x04003B3A RID: 15162
			Success,
			// Token: 0x04003B3B RID: 15163
			Failure
		}
	}
}
