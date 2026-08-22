using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000373 RID: 883
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class P2PServerPeer : ServerPeer<P2PConnection>
	{
		// Token: 0x060034B7 RID: 13495 RVA: 0x0016B4FE File Offset: 0x001696FE
		public P2PServerPeer(P2PEndpoint ownerEp, int ownerKey, ServerSettings settings, ServerPeer.Callbacks callbacks) : base(callbacks, settings)
		{
			this.ownerKey = Option.Some<int>(ownerKey);
			this.ownerEndpoint = ownerEp;
			this.started = false;
		}

		// Token: 0x060034B8 RID: 13496 RVA: 0x0016B524 File Offset: 0x00169724
		public override void Start()
		{
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = (PacketHeader.IsConnectionInitializationStep | PacketHeader.IsServerMessage),
				Initialization = null
			};
			this.SendMsgInternal(this.ownerEndpoint, headers, null);
			this.started = true;
		}

		// Token: 0x060034B9 RID: 13497 RVA: 0x0016B570 File Offset: 0x00169770
		public override void Close()
		{
			if (!this.started)
			{
				return;
			}
			if (this.OwnerConnection != null)
			{
				this.OwnerConnection.Status = NetworkConnectionStatus.Disconnected;
			}
			for (int i = this.pendingClients.Count - 1; i >= 0; i--)
			{
				base.RemovePendingClient(this.pendingClients[i], PeerDisconnectPacket.WithReason(DisconnectReason.ServerShutdown));
			}
			for (int j = this.connectedClients.Count - 1; j >= 0; j--)
			{
				this.Disconnect(this.connectedClients[j].Connection, PeerDisconnectPacket.WithReason(DisconnectReason.ServerShutdown));
			}
			this.pendingClients.Clear();
			this.connectedClients.Clear();
			ChildServerRelay.ShutDown();
			this.callbacks.OnShutdown();
		}

		// Token: 0x060034BA RID: 13498 RVA: 0x0016B62C File Offset: 0x0016982C
		public override void Update(float deltaTime)
		{
			if (!this.started)
			{
				return;
			}
			for (int i = this.connectedClients.Count - 1; i >= 0; i--)
			{
				P2PConnection conn = this.connectedClients[i].Connection;
				conn.Decay(deltaTime);
				if (conn.Timeout < 0.0)
				{
					this.Disconnect(conn, PeerDisconnectPacket.WithReason(DisconnectReason.Timeout));
				}
			}
			try
			{
				foreach (byte[] incBuf in ChildServerRelay.Read())
				{
					P2PEndpoint senderEndpoint = null;
					try
					{
						IReadMessage inc = new ReadOnlyMessage(incBuf, false, 0, incBuf.Length, this.OwnerConnection);
						this.HandleDataMessage(inc, out senderEndpoint);
					}
					catch (NetStructReadException)
					{
						if (senderEndpoint != null && senderEndpoint != this.ownerEndpoint)
						{
							ServerPeer<P2PConnection>.PendingClient pendingClient = this.pendingClients.Find((ServerPeer<P2PConnection>.PendingClient c) => c.Connection.Endpoint == senderEndpoint);
							if (pendingClient != null)
							{
								base.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
							}
							ServerPeer<P2PConnection>.ClientConnectionData connectedClient = this.connectedClients.Find((ServerPeer<P2PConnection>.ClientConnectionData c) => c.Connection.Endpoint == senderEndpoint);
							if (connectedClient != null)
							{
								this.Disconnect(connectedClient.Connection, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
							}
							break;
						}
						throw;
					}
				}
			}
			catch (Exception e)
			{
				string str = "Server failed to read an incoming message. {";
				Exception ex = e;
				string errorMsg = str + ((ex != null) ? ex.ToString() : null) + "}\n" + e.StackTrace.CleanupStackTrace();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
				defaultInterpolatedStringHandler.AppendLiteral("SteamP2PServerPeer.Update:ClientReadException");
				defaultInterpolatedStringHandler.AppendFormatted<MethodBase>(e.TargetSite);
				GameAnalyticsManager.AddErrorEventOnce(defaultInterpolatedStringHandler.ToStringAndClear(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
			}
			for (int j = 0; j < this.pendingClients.Count; j++)
			{
				ServerPeer<P2PConnection>.PendingClient pendingClient2 = this.pendingClients[j];
				base.UpdatePendingClient(pendingClient2);
				if (j >= this.pendingClients.Count || this.pendingClients[j] != pendingClient2)
				{
					j--;
				}
			}
		}

		// Token: 0x060034BB RID: 13499 RVA: 0x0016B878 File Offset: 0x00169A78
		private void HandleDataMessage(IReadMessage inc, [Nullable(2)] out P2PEndpoint senderEndPoint)
		{
			senderEndPoint = null;
			if (!this.started)
			{
				return;
			}
			P2POwnerToServerHeader senderInfo = INetSerializableStruct.Read<P2POwnerToServerHeader>(inc);
			P2PEndpoint senderEndpoint;
			if (!senderInfo.Endpoint.TryUnwrap(out senderEndpoint))
			{
				return;
			}
			DeliveryMethod deliveryMethod;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			INetSerializableStruct.Read<PeerPacketHeaders>(inc).Deconstruct(out deliveryMethod, out packetHeader2, out connectionInitialization);
			PacketHeader packetHeader = packetHeader2;
			ConnectionInitialization? initialization = connectionInitialization;
			if (packetHeader.IsServerMessage())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Got server message from ");
				defaultInterpolatedStringHandler.AppendFormatted<P2PEndpoint>(senderEndpoint);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (senderEndpoint != this.ownerEndpoint)
			{
				ServerPeer<P2PConnection>.PendingClient pendingClient = this.pendingClients.Find((ServerPeer<P2PConnection>.PendingClient c) => base.<HandleDataMessage>g__connectionMatches|0(c.Connection));
				ServerPeer<P2PConnection>.ClientConnectionData connectedClient = this.connectedClients.Find((ServerPeer<P2PConnection>.ClientConnectionData c) => base.<HandleDataMessage>g__connectionMatches|0(c.Connection));
				if (pendingClient != null)
				{
					pendingClient.Connection.SetAccountInfo(senderInfo.AccountInfo);
				}
				if (pendingClient != null)
				{
					pendingClient.Heartbeat();
				}
				if (connectedClient != null)
				{
					connectedClient.Connection.Heartbeat();
				}
				string banReason;
				if (this.serverSettings.BanList.IsBanned(senderEndpoint, out banReason) || this.serverSettings.BanList.IsBanned(senderInfo.AccountInfo, out banReason))
				{
					if (pendingClient != null)
					{
						base.RemovePendingClient(pendingClient, PeerDisconnectPacket.Banned(banReason));
						return;
					}
					if (connectedClient != null)
					{
						this.Disconnect(connectedClient.Connection, PeerDisconnectPacket.Banned(banReason));
						return;
					}
					this.SendDisconnectMessage(senderEndpoint, PeerDisconnectPacket.Banned(banReason));
					return;
				}
				else if (packetHeader.IsDisconnectMessage())
				{
					if (pendingClient != null)
					{
						base.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
						return;
					}
					if (connectedClient != null)
					{
						this.Disconnect(connectedClient.Connection, PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
						return;
					}
				}
				else
				{
					if (packetHeader.IsHeartbeatMessage() || packetHeader.IsDoSProtectionMessage())
					{
						return;
					}
					if (packetHeader.IsConnectionInitializationStep())
					{
						if (initialization == null)
						{
							return;
						}
						ConnectionInitialization initializationStep = initialization.Value;
						if (pendingClient != null)
						{
							base.ReadConnectionInitializationStep(pendingClient, new ReadWriteMessage(inc.Buffer, inc.BitPosition, inc.LengthBits, false), initializationStep);
							return;
						}
						if (initializationStep == ConnectionInitialization.ConnectionStarted)
						{
							this.pendingClients.Add(new ServerPeer<P2PConnection>.PendingClient(senderEndpoint.MakeConnectionFromEndpoint()));
							return;
						}
					}
					else if (connectedClient != null)
					{
						if (!packetHeader.IsDataFragment())
						{
							PeerPacketMessage packet = INetSerializableStruct.Read<PeerPacketMessage>(inc);
							IReadMessage msg = new ReadOnlyMessage(packet.Buffer, packetHeader.IsCompressed(), 0, packet.Length, connectedClient.Connection);
							this.callbacks.OnMessageReceived(connectedClient.Connection, msg);
							return;
						}
						MessageFragment fragment = INetSerializableStruct.Read<MessageFragment>(inc);
						ImmutableArray<byte> completeMessage;
						if (!connectedClient.Defragmenter.ProcessIncomingFragment(fragment).TryUnwrap(out completeMessage))
						{
							return;
						}
						IReadMessage msg2 = new ReadOnlyMessage(completeMessage.ToArray<byte>(), false, 0, completeMessage.Length, connectedClient.Connection);
						this.callbacks.OnMessageReceived(connectedClient.Connection, msg2);
						return;
					}
				}
			}
			else
			{
				P2PConnection ownerConnection = this.OwnerConnection;
				if (ownerConnection != null)
				{
					ownerConnection.Heartbeat();
				}
				if (packetHeader.IsDisconnectMessage())
				{
					DebugConsole.ThrowError("Received disconnect message from owner", null, null, false, false);
					return;
				}
				if (packetHeader.IsServerMessage())
				{
					DebugConsole.ThrowError("Received server message from owner", null, null, false, false);
					return;
				}
				if (packetHeader.IsDoSProtectionMessage())
				{
					DoSProtectionPacket packet2 = INetSerializableStruct.Read<DoSProtectionPacket>(inc);
					PeerDisconnectPacket disconnectPacket = INetSerializableStruct.Read<PeerDisconnectPacket>(inc);
					P2PEndpoint endpoint;
					if (packet2.Endpoint.TryUnwrap(out endpoint))
					{
						ServerPeer<P2PConnection>.PendingClient pendingClient2 = this.pendingClients.Find((ServerPeer<P2PConnection>.PendingClient c) => c.Connection.Endpoint == endpoint);
						ServerPeer<P2PConnection>.ClientConnectionData connectedClientData = this.connectedClients.Find((ServerPeer<P2PConnection>.ClientConnectionData c) => c.Connection.Endpoint == endpoint);
						string clientName;
						if (pendingClient2 != null)
						{
							clientName = pendingClient2.Name;
							if (packet2.ShouldBan)
							{
								base.BanPendingClient(pendingClient2, disconnectPacket.AdditionalInformation, null);
							}
							base.RemovePendingClient(pendingClient2, disconnectPacket);
						}
						else
						{
							if (connectedClientData == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(104, 1);
								defaultInterpolatedStringHandler2.AppendLiteral("Unable to remove client ");
								defaultInterpolatedStringHandler2.AppendFormatted<P2PEndpoint>(endpoint);
								defaultInterpolatedStringHandler2.AppendLiteral(" for triggering DoS protection, client not found in pending or connected clients");
								string errorMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
								DebugConsole.ThrowError(errorMsg, null, null, false, false);
								GameServer.Log(errorMsg, ServerLog.MessageType.Error);
								return;
							}
							clientName = connectedClientData.TryGetClientName();
							if (packet2.ShouldBan)
							{
								connectedClientData.BanClient(this.serverSettings, disconnectPacket.AdditionalInformation, null);
							}
							this.Disconnect(connectedClientData.Connection, disconnectPacket);
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(58, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Client ");
						defaultInterpolatedStringHandler3.AppendFormatted(clientName ?? endpoint.ToString());
						defaultInterpolatedStringHandler3.AppendLiteral(" ");
						defaultInterpolatedStringHandler3.AppendFormatted(packet2.ShouldBan ? "banned" : "disconnected");
						defaultInterpolatedStringHandler3.AppendLiteral(" due to DoS protection (Sending too many packets).");
						GameServer.Log(defaultInterpolatedStringHandler3.ToStringAndClear(), ServerLog.MessageType.DoSProtection);
						GameServer server = GameMain.Server;
						if (server == null)
						{
							return;
						}
						server.SendChatMessage(disconnectPacket.ChatMessage(clientName).Value, new ChatMessageType?(ChatMessageType.Server), null, null, disconnectPacket.ConnectionChangeType, ChatMode.None);
					}
					return;
				}
				if (packetHeader.IsConnectionInitializationStep())
				{
					if (this.OwnerConnection == null)
					{
						P2PInitializationOwnerPacket packet3 = INetSerializableStruct.Read<P2PInitializationOwnerPacket>(inc);
						this.OwnerConnection = this.ownerEndpoint.MakeConnectionFromEndpoint();
						this.OwnerConnection.Language = GameSettings.CurrentConfig.Language;
						this.OwnerConnection.SetAccountInfo(senderInfo.AccountInfo);
						this.callbacks.OnInitializationComplete(this.OwnerConnection, packet3.Name);
						this.callbacks.OnOwnerDetermined(this.OwnerConnection);
					}
					return;
				}
				if (packetHeader.IsHeartbeatMessage())
				{
					return;
				}
				PeerPacketMessage packet4 = INetSerializableStruct.Read<PeerPacketMessage>(inc);
				IReadMessage msg3 = new ReadOnlyMessage(packet4.Buffer, packetHeader.IsCompressed(), 0, packet4.Length, this.OwnerConnection);
				this.callbacks.OnMessageReceived(this.OwnerConnection, msg3);
			}
		}

		// Token: 0x060034BC RID: 13500 RVA: 0x0016BE4C File Offset: 0x0016A04C
		public override void Send(IWriteMessage msg, NetworkConnection conn, DeliveryMethod deliveryMethod, bool compressPastThreshold = true)
		{
			if (!this.started)
			{
				return;
			}
			P2PConnection p2pConn = conn as P2PConnection;
			if (p2pConn == null)
			{
				return;
			}
			int ccIndex = this.connectedClients.FindIndex((ServerPeer<P2PConnection>.ClientConnectionData cc) => cc.Connection == p2pConn);
			if (ccIndex < 0 && conn != this.OwnerConnection)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to send message to unauthenticated connection: ");
				defaultInterpolatedStringHandler.AppendFormatted<Option<AccountId>>(p2pConn.AccountInfo.AccountId);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			bool isCompressed;
			int num;
			byte[] bufAux = msg.PrepareForSending(compressPastThreshold, out isCompressed, out num);
			PeerPacketHeaders peerPacketHeaders;
			if (bufAux.Length > 1100 && conn != this.OwnerConnection)
			{
				ServerPeer<P2PConnection>.ClientConnectionData cc2 = this.connectedClients[ccIndex];
				foreach (MessageFragment fragment in cc2.Fragmenter.FragmentMessage(msg.Buffer.AsSpan<byte>().Slice(0, msg.LengthBytes)))
				{
					peerPacketHeaders = new PeerPacketHeaders
					{
						DeliveryMethod = DeliveryMethod.Reliable,
						PacketHeader = (PacketHeader.IsServerMessage | PacketHeader.IsDataFragment),
						Initialization = null
					};
					PeerPacketHeaders fragmentHeaders = peerPacketHeaders;
					this.SendMsgInternal(p2pConn, fragmentHeaders, fragment);
				}
				return;
			}
			peerPacketHeaders = new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = ((isCompressed ? PacketHeader.IsCompressed : PacketHeader.None) | PacketHeader.IsServerMessage),
				Initialization = null
			};
			PeerPacketHeaders headers = peerPacketHeaders;
			PeerPacketMessage body = new PeerPacketMessage
			{
				Buffer = bufAux
			};
			this.SendMsgInternal(p2pConn, headers, body);
		}

		// Token: 0x060034BD RID: 13501 RVA: 0x0016BFFC File Offset: 0x0016A1FC
		private void SendDisconnectMessage(P2PEndpoint endpoint, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.started)
			{
				return;
			}
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = (PacketHeader.IsDisconnectMessage | PacketHeader.IsServerMessage),
				Initialization = null
			};
			this.SendMsgInternal(endpoint, headers, peerDisconnectPacket);
		}

		// Token: 0x060034BE RID: 13502 RVA: 0x0016C048 File Offset: 0x0016A248
		public override void Disconnect(NetworkConnection conn, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.started)
			{
				return;
			}
			P2PConnection p2pConn = conn as P2PConnection;
			if (p2pConn == null)
			{
				return;
			}
			this.SendDisconnectMessage(p2pConn.Endpoint, peerDisconnectPacket);
			int ccIndex = this.connectedClients.FindIndex((ServerPeer<P2PConnection>.ClientConnectionData cc) => cc.Connection == p2pConn);
			if (ccIndex >= 0)
			{
				p2pConn.Status = NetworkConnectionStatus.Disconnected;
				this.connectedClients.RemoveAt(ccIndex);
				this.callbacks.OnDisconnect(conn, peerDisconnectPacket);
				return;
			}
			if (p2pConn == this.OwnerConnection)
			{
				throw new InvalidOperationException("Cannot disconnect owner peer");
			}
		}

		// Token: 0x060034BF RID: 13503 RVA: 0x0016C0EB File Offset: 0x0016A2EB
		protected override void SendMsgInternal(P2PConnection conn, PeerPacketHeaders headers, [Nullable(2)] INetSerializableStruct body)
		{
			this.SendMsgInternal(conn.Endpoint, headers, body);
		}

		// Token: 0x060034C0 RID: 13504 RVA: 0x0016C0FC File Offset: 0x0016A2FC
		private void SendMsgInternal(P2PEndpoint connEndpoint, PeerPacketHeaders headers, [Nullable(2)] INetSerializableStruct body)
		{
			IWriteMessage msgToSend = new WriteOnlyMessage();
			msgToSend.WriteNetSerializableStruct(new P2PServerToOwnerHeader
			{
				EndpointStr = connEndpoint.StringRepresentation
			});
			msgToSend.WriteNetSerializableStruct(headers);
			if (body != null)
			{
				body.Write(msgToSend);
			}
			P2PServerPeer.ForwardToOwnerProcess(msgToSend);
		}

		// Token: 0x060034C1 RID: 13505 RVA: 0x0016C144 File Offset: 0x0016A344
		private static void ForwardToOwnerProcess(IWriteMessage msg)
		{
			byte[] bufToSend = (byte[])msg.Buffer.Clone();
			Array.Resize<byte>(ref bufToSend, msg.LengthBytes);
			ChildServerRelay.Write(bufToSend);
		}

		// Token: 0x060034C2 RID: 13506 RVA: 0x0016C175 File Offset: 0x0016A375
		protected override void ProcessAuthTicket(ClientAuthTicketAndVersionPacket packet, ServerPeer<P2PConnection>.PendingClient pendingClient)
		{
			pendingClient.InitializationStep = (base.ShouldAskForPassword(this.serverSettings, pendingClient.Connection) ? ConnectionInitialization.Password : ConnectionInitialization.ContentPackageOrder);
			pendingClient.Name = packet.Name;
			pendingClient.AuthSessionStarted = true;
		}

		// Token: 0x04001A32 RID: 6706
		private bool started;

		// Token: 0x04001A33 RID: 6707
		private readonly P2PEndpoint ownerEndpoint;
	}
}
